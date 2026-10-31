using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.UI.VisualTeaching
{
    public enum SparkLibraryTier { Foundation, Applied, Diagnostics }

    [Serializable]
    public sealed class SparkLibraryLessonEntry
    {
        public string id;
        public SparkLibraryTier tier;
        public string topic;
        [TextArea(2, 4)] public string summary;
        public string keywords;
        public SparkVisualLesson lesson;
    }

    /// <summary>
    /// A read-only library for authored SparkVisualLesson assets. Bookmarks/visit counts
    /// are study navigation data, not evaluator progress or verified electrical facts.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkAILessonLibrary : MonoBehaviour
    {
        private const string BookmarkPrefix = "ProjectSpark.LessonLibrary.Bookmark.v1.";
        private const string VisitPrefix = "ProjectSpark.LessonLibrary.Visit.v1.";
        private const string LastOpenedKey = "ProjectSpark.LessonLibrary.LastOpened.v1";

        [Header("Existing System")]
        [SerializeField] private SparkVisualTeachingManager teaching;
        [Header("Editor-created Lesson Catalog")]
        [SerializeField] private SparkLibraryLessonEntry[] entries = Array.Empty<SparkLibraryLessonEntry>();
        [Header("Editor-created UI")]
        [SerializeField] private GameObject libraryCard;
        [SerializeField] private TMP_InputField searchField;
        [SerializeField] private RectTransform listContent;
        [SerializeField] private ScrollRect listScroll;
        [SerializeField] private GameObject rowTemplate;
        [SerializeField] private TMP_Text summaryText;
        [SerializeField] private TMP_Text selectedTitleText;
        [SerializeField] private TMP_Text selectedTierText;
        [SerializeField] private TMP_Text selectedSummaryText;
        [SerializeField] private TMP_Text selectedStepsText;
        [SerializeField] private TMP_Text statusText;
        [SerializeField] private TMP_Text bookmarkLabel;
        [SerializeField] private TMP_Text favoritesLabel;
        [SerializeField] private TMP_Text recommendationText;

        private readonly List<GameObject> rows = new List<GameObject>();
        private string selectedId = string.Empty;
        private SparkLibraryTier? filterTier;
        private bool bookmarksOnly;
        private bool listening;

        private void Awake()
        {
            Resolve();
            if (libraryCard != null) libraryCard.SetActive(false);
        }

        private void OnEnable()
        {
            Resolve();
            if (searchField != null && !listening)
            {
                searchField.onValueChanged.AddListener(OnSearchChanged);
                listening = true;
            }
            RefreshLibrary();
        }

        private void OnDisable()
        {
            if (searchField != null && listening)
                searchField.onValueChanged.RemoveListener(OnSearchChanged);
            listening = false;
            DestroyRows();
        }

        private void Resolve()
        {
            if (teaching == null)
                teaching = FindFirstObjectByType<SparkVisualTeachingManager>(FindObjectsInactive.Include);
        }

        public void Show()
        {
            if (libraryCard == null) return;
            libraryCard.SetActive(true);
            RefreshLibrary();
        }
        public void Hide()
        {
            if (libraryCard != null) libraryCard.SetActive(false);
        }
        public void Toggle()
        {
            if (libraryCard == null) return;
            if (libraryCard.activeSelf) Hide();
            else Show();
        }

        private void OnSearchChanged(string _) => RefreshLibrary();
        public void AllLessons() { filterTier = null; bookmarksOnly = false; RefreshLibrary(); }
        public void Foundation() { filterTier = SparkLibraryTier.Foundation; bookmarksOnly = false; RefreshLibrary(); }
        public void Applied() { filterTier = SparkLibraryTier.Applied; bookmarksOnly = false; RefreshLibrary(); }
        public void Diagnostics() { filterTier = SparkLibraryTier.Diagnostics; bookmarksOnly = false; RefreshLibrary(); }
        public void Bookmarks() { filterTier = null; bookmarksOnly = true; RefreshLibrary(); }

        private bool Valid(SparkLibraryLessonEntry entry)
        {
            return entry != null && !string.IsNullOrWhiteSpace(entry.id) &&
                   entry.lesson != null && entry.lesson.steps != null && entry.lesson.steps.Length > 0;
        }

        private SparkLibraryLessonEntry Get(string id)
        {
            if (entries == null) return null;
            foreach (var entry in entries)
                if (Valid(entry) && string.Equals(entry.id, id, StringComparison.Ordinal))
                    return entry;
            return null;
        }

        private bool Bookmarked(string id) => PlayerPrefs.GetInt(BookmarkPrefix + id, 0) == 1;
        private int Visits(string id) => PlayerPrefs.GetInt(VisitPrefix + id, 0);

        private bool Matches(SparkLibraryLessonEntry entry, string query)
        {
            if (filterTier.HasValue && entry.tier != filterTier.Value) return false;
            if (bookmarksOnly && !Bookmarked(entry.id)) return false;
            if (string.IsNullOrWhiteSpace(query)) return true;
            string title = entry.lesson != null ? entry.lesson.title : "";
            return Contains(title, query) || Contains(entry.topic, query) ||
                   Contains(entry.summary, query) || Contains(entry.keywords, query) ||
                   Contains(entry.id, query);
        }

        private static bool Contains(string source, string query) =>
            !string.IsNullOrEmpty(source) &&
            source.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0;

        public void RefreshLibrary()
        {
            DestroyRows();
            if (entries == null) entries = Array.Empty<SparkLibraryLessonEntry>();

            string query = searchField != null ? searchField.text.Trim() : "";
            int visible = 0, total = 0, bookmarked = 0;
            var seen = new HashSet<string>(StringComparer.Ordinal);
            SparkLibraryLessonEntry first = null;
            bool selectedVisible = false;
            foreach (SparkLibraryLessonEntry entry in entries)
            {
                if (!Valid(entry) || !seen.Add(entry.id)) continue;
                total++;
                if (Bookmarked(entry.id)) bookmarked++;
                if (!Matches(entry, query)) continue;
                if (first == null) first = entry;
                if (entry.id == selectedId) selectedVisible = true;
                visible++;
                MakeRow(entry);
            }

            if (!selectedVisible) selectedId = first == null ? string.Empty : first.id;
            if (summaryText != null) summaryText.text = visible + " SHOWN   /   " + total + " LESSONS";
            if (favoritesLabel != null) favoritesLabel.text = "BOOKMARKS (" + bookmarked + ")";
            if (listContent != null)
                LayoutRebuilder.MarkLayoutForRebuild(listContent);
            UpdateDetail();
        }

        private void MakeRow(SparkLibraryLessonEntry entry)
        {
            if (rowTemplate == null || listContent == null) return;
            GameObject row = Instantiate(rowTemplate, listContent, false);
            row.name = "Lesson_" + entry.id;
            row.SetActive(true);
            rows.Add(row);
            TMP_Text label = row.transform.Find("Label")?.GetComponent<TMP_Text>();
            if (label != null)
                label.text = (Bookmarked(entry.id) ? "★  " : "") + entry.lesson.title +
                             "\n<size=11><color=#91B9C7>" + entry.tier + "  •  " + entry.topic + "</color></size>";
            Button button = row.GetComponent<Button>();
            if (button != null)
            {
                string id = entry.id;
                button.onClick.AddListener(() => Select(id));
            }
            Image background = row.GetComponent<Image>();
            if (background != null && entry.id == selectedId)
                background.color = new Color(.14f, .34f, .45f, 1f);
        }

        private void DestroyRows()
        {
            foreach (GameObject row in rows)
            {
                if (row == null) continue;
                row.SetActive(false); // Remove from layout immediately before deferred Destroy.
                Destroy(row);
            }
            rows.Clear();
        }

        private void Select(string id)
        {
            if (Get(id) == null) return;
            selectedId = id;
            UpdateDetail();
            // Refresh selected-row highlighting while preserving the current scroll position.
            float scrollValue = listScroll != null ? listScroll.verticalNormalizedPosition : 1f;
            RefreshLibrary();
            if (listScroll != null) listScroll.verticalNormalizedPosition = scrollValue;
        }

        private void UpdateDetail()
        {
            SparkLibraryLessonEntry entry = Get(selectedId);
            if (selectedTitleText != null) selectedTitleText.text = entry != null ? entry.lesson.title : "SELECT A LESSON";
            if (selectedTierText != null) selectedTierText.text = entry != null ? entry.tier + "  /  " + entry.topic : "NO MATCHING LESSONS";
            if (selectedSummaryText != null) selectedSummaryText.text = entry != null ? entry.summary : "Clear your search or choose ALL.";
            if (selectedStepsText != null)
            {
                if (entry == null) selectedStepsText.text = "";
                else
                {
                    string steps = "";
                    int count = Mathf.Min(entry.lesson.steps.Length, 6);
                    for (int i = 0; i < count; i++)
                        steps += (i + 1) + ". " + entry.lesson.steps[i].title + "\n";
                    selectedStepsText.text = "LESSON STEPS\n" + steps;
                }
            }
            if (bookmarkLabel != null)
                bookmarkLabel.text = entry == null ? "BOOKMARK" : Bookmarked(entry.id) ? "REMOVE BOOKMARK" : "SAVE BOOKMARK";
            if (recommendationText != null)
            {
                var recommendation = PickRecommended();
                recommendationText.text = recommendation != null
                    ? "NEXT PATH: " + recommendation.lesson.title
                    : "NEXT PATH: No lessons installed";
            }
            if (statusText != null && entry != null)
                statusText.text = Visits(entry.id) > 0
                    ? "PREVIOUSLY OPENED  •  Starting replaces the current visual lesson."
                    : "NOT YET OPENED  •  Starting replaces the current visual lesson.";
        }

        public void ToggleBookmark()
        {
            SparkLibraryLessonEntry entry = Get(selectedId);
            if (entry == null) return;
            PlayerPrefs.SetInt(BookmarkPrefix + entry.id, Bookmarked(entry.id) ? 0 : 1);
            PlayerPrefs.Save();
            RefreshLibrary();
        }

        private SparkLibraryLessonEntry PickRecommended()
        {
            if (entries == null) return null;
            // Catalog order is the learning path. Recommend the first lesson not yet opened.
            foreach (var entry in entries)
                if (Valid(entry) && Visits(entry.id) == 0) return entry;
            // If all have been visited, cycle to the one after the last opened.
            string last = PlayerPrefs.GetString(LastOpenedKey, "");
            for (int i = 0; i < entries.Length; i++)
                if (Valid(entries[i]) && entries[i].id == last)
                    for (int j = 1; j <= entries.Length; j++)
                    {
                        var next = entries[(i + j) % entries.Length];
                        if (Valid(next)) return next;
                    }
            foreach (var entry in entries) if (Valid(entry)) return entry;
            return null;
        }

        public void SelectRecommended()
        {
            SparkLibraryLessonEntry recommendation = PickRecommended();
            if (recommendation == null) return;
            // Search/filter might hide the recommended entry; clear UI constraints first.
            filterTier = null;
            bookmarksOnly = false;
            if (searchField != null) searchField.SetTextWithoutNotify("");
            selectedId = recommendation.id;
            RefreshLibrary();
            if (statusText != null) statusText.text = "Recommended lesson selected. Choose START LESSON to open it.";
        }

        public void StartSelectedLesson()
        {
            SparkLibraryLessonEntry entry = Get(selectedId);
            Resolve();
            if (entry == null || teaching == null)
            {
                if (statusText != null) statusText.text = "Lesson or Visual Teaching Manager unavailable.";
                return;
            }
            // User-initiated replacement only. No simulated measurements or solver calls.
            teaching.StartLesson(entry.lesson);
            PlayerPrefs.SetString(LastOpenedKey, entry.id);
            PlayerPrefs.SetInt(VisitPrefix + entry.id, Mathf.Clamp(Visits(entry.id) + 1, 0, 1000000));
            PlayerPrefs.Save();
            if (statusText != null) statusText.text = "Opened " + entry.lesson.title + ". Educational diagram, not a live circuit reading.";
            Hide();
        }

        public void OpenCurrentTeachingBoard()
        {
            Resolve();
            if (teaching == null)
            {
                if (statusText != null) statusText.text = "Visual Teaching Manager unavailable.";
                return;
            }
            teaching.OpenTeachingBoard();
        }
    }
}
