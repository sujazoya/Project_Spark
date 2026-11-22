using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using F = ProjectSpark.AAAArchitecture.SparkAAAUIFactory;

namespace ProjectSpark.AAAArchitecture
{
    /// <summary>A selectable mission cover. Its callers supply all access and progress information.</summary>
    public sealed class SparkAAALevelCard : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler,
        ISelectHandler, IDeselectHandler
    {
        private static readonly Color Ink = new Color32(238, 247, 251, 255);
        private static readonly Color Muted = new Color32(163, 188, 207, 255);
        private static readonly Color Edge = new Color32(42, 69, 87, 255);
        private static readonly Color Surface = new Color32(14, 27, 41, 255);
        private static readonly Color Cyan = new Color32(116, 226, 255, 255);
        private static readonly Dictionary<string, Sprite> Artwork = new();

        [SerializeField] private Button button;
        [SerializeField] private Image frame, preview, progress, accent, shade, badge;
        [SerializeField] private TextMeshProUGUI number, title, domain, status, count, placeholder, detail;
        private Action onSelect;
        private Color selectionColor = Cyan;
        private bool selected, focused, hovered;

        public Button Button => button;

        public static float HeightForWidth(float width) => Mathf.Clamp(width * .50f, 112, 166) + 124;

        public static Sprite FindArtwork(int level, int chapter)
        {
            return LoadArtwork($"L{level:000}") ?? LoadArtwork($"C{chapter:00}");
        }

        private static Sprite LoadArtwork(string name)
        {
            string path = $"SparkMissionArtwork/{name}";
            if (!Application.isPlaying)
                return Resources.Load<Sprite>(path);

            if (!Artwork.TryGetValue(name, out Sprite sprite))
            {
                sprite = Resources.Load<Sprite>(path);
                Artwork.Add(name, sprite);
            }
            return sprite;
        }

        private void OnEnable() => button.onClick.AddListener(Choose);

        private void OnDisable()
        {
            button.onClick.RemoveListener(Choose);
            hovered = false;
            focused = false;
            PaintSelection();
        }

        private void Choose() => onSelect?.Invoke();

        public void SetMission(int levelNumber, string missionTitle, string chapter, int objectiveCount,
            Sprite artwork, Action choose)
        {
            onSelect = choose;
            number.text = $"LEVEL {levelNumber:00}";
            title.text = missionTitle;
            domain.text = chapter;
            count.text = objectiveCount == 1 ? "1 objective" : $"{objectiveCount} objectives";
            preview.sprite = artwork;
            preview.enabled = artwork;
            placeholder.gameObject.SetActive(!artwork);
        }

        public void SetState(string label, Color color, float? fraction, bool isSelected, Color highlight)
        {
            status.text = label;
            status.color = color;
            badge.color = Color.Lerp(new Color32(8, 19, 30, 246), color, .14f);
            badge.color = new Color(badge.color.r, badge.color.g, badge.color.b, .98f);
            progress.color = color;
            progress.rectTransform.anchorMax = new Vector2(Mathf.Clamp01(fraction ?? 0), 1);
            progress.gameObject.SetActive(fraction.HasValue);
            selected = isSelected;
            selectionColor = highlight;
            PaintSelection();
        }

        public void Bind(SparkAAALevel level, string chapter, SparkAAAProgress state, bool active, Color theme,
            Action<SparkAAALevel> callback)
        {
            bool missing = !level.Present || level.blocked;
            SetMission(level.number, level.title, chapter, level.targets.Count,
                level.thumbnail ? level.thumbnail : FindArtwork(level.number, level.chapter), () => callback?.Invoke(level));
            string label = missing ? "Unavailable" : state.status switch
            {
                SparkAAAStatus.Unknown => "Preview",
                SparkAAAStatus.InProgress => "In progress",
                SparkAAAStatus.Requalification => "Requalify",
                _ => state.status.ToString()
            };
            Color color = missing || state.status is SparkAAAStatus.Locked or SparkAAAStatus.Unknown ? Muted :
                state.status is SparkAAAStatus.Completed or SparkAAAStatus.Certified ? new Color32(117, 236, 177, 255) : theme;
            SetState(label, color, !missing && state.status != SparkAAAStatus.Unknown ? state.fraction : null, active, theme);
        }

        public void OnPointerEnter(PointerEventData eventData) { hovered = true; PaintSelection(); }
        public void OnPointerExit(PointerEventData eventData) { hovered = false; PaintSelection(); }
        public void OnSelect(BaseEventData eventData) { focused = true; PaintSelection(); }
        public void OnDeselect(BaseEventData eventData) { focused = false; PaintSelection(); }

        private void PaintSelection()
        {
            bool highlighted = selected || hovered || focused;
            frame.color = highlighted ? selectionColor : Edge;
            accent.color = highlighted ? selectionColor : Edge;
            shade.color = new Color(0, .035f, .07f, highlighted ? .02f : .15f);
            detail.color = highlighted ? selectionColor : Muted;
            detail.text = selected ? "SELECTED" : "DETAILS  >";
        }

        public static SparkAAALevelCard Create(Transform parent = null)
        {
            RectTransform root = F.Rect("SparkLevelButton", parent);
            root.gameObject.SetActive(false);
            root.sizeDelta = new Vector2(300, HeightForWidth(300));
            var card = root.gameObject.AddComponent<SparkAAALevelCard>();
            card.frame = F.Image(root, Edge);
            card.frame.raycastTarget = true;
            card.button = root.gameObject.AddComponent<Button>();

            RectTransform surface = F.Rect("Surface", root);
            F.Stretch(surface, 2, 2, 2, 2);
            card.button.targetGraphic = F.Image(surface, Color.white);
            card.button.colors = new ColorBlock
            {
                normalColor = Surface,
                highlightedColor = new Color32(22, 47, 64, 255),
                selectedColor = new Color32(23, 46, 63, 255),
                pressedColor = new Color32(28, 67, 84, 255),
                disabledColor = Surface,
                colorMultiplier = 1,
                fadeDuration = .11f
            };

            RectTransform hero = F.Rect("HardwarePreview", surface);
            Place(hero, 0, 0, 1, 1, 0, 122, 0, 0);
            F.Image(hero, new Color32(20, 41, 57, 255));
            var art = F.Rect("MissionArtwork", hero);
            F.Stretch(art);
            card.preview = F.Image(art, Color.white);
            var shade = F.Rect("ArtworkShade", hero);
            F.Stretch(shade);
            card.shade = F.Image(shade, new Color(0, .035f, .07f, .15f));
            card.placeholder = F.Text("PreviewPlaceholder", hero, "No mission artwork", 16, Muted);
            F.Stretch(card.placeholder.rectTransform, 6, 6, 6, 6);
            card.placeholder.alignment = TextAlignmentOptions.Center;

            RectTransform levelPlate = F.Rect("LevelPlate", hero);
            Place(levelPlate, 0, 1, 0, 1, 10, -40, 113, -10);
            F.Image(levelPlate, new Color32(8, 20, 30, 242));
            card.number = F.Text("LevelNumber", levelPlate, "", 16, Ink, bold: true);
            F.Stretch(card.number.rectTransform, 9, 0, 5, 0);

            RectTransform statePlate = F.Rect("AccessPlate", hero);
            Place(statePlate, 1, 1, 1, 1, -115, -38, -10, -10);
            card.badge = F.Image(statePlate, new Color32(8, 20, 30, 242));
            card.status = F.Text("State", statePlate, "", 14, Muted, bold: true);
            F.Stretch(card.status.rectTransform, 4, 0, 4, 0);
            card.status.alignment = TextAlignmentOptions.Center;

            var divider = F.Rect("Accent", surface);
            Place(divider, 0, 0, 1, 0, 0, 120, 0, 122);
            card.accent = F.Image(divider, Edge);
            card.title = BottomText("MissionTitle", surface, 12, 12, 72, 115, 23, Ink, true);
            card.title.textWrappingMode = TextWrappingModes.Normal;
            card.title.enableAutoSizing = true;
            card.title.fontSizeMin = 16;
            card.title.fontSizeMax = 23;
            card.domain = BottomText("Division", surface, 12, 12, 52, 73, 15, Muted);
            card.count = BottomText("TargetCount", surface, 12, 110, 18, 46, 15, Ink);
            card.detail = BottomText("Details", surface, 106, 12, 18, 46, 13, Muted, true);
            card.detail.alignment = TextAlignmentOptions.MidlineRight;

            RectTransform baseline = F.Rect("ProgressTrack", surface);
            Place(baseline, 0, 0, 1, 0, 12, 10, -12, 13);
            F.Image(baseline, Edge);
            var fill = F.Rect("ProgressFill", baseline);
            F.Stretch(fill);
            fill.anchorMax = new Vector2(0, 1);
            card.progress = F.Image(fill, Cyan);

            if (!parent)
            {
                Sprite sample = FindArtwork(120, 12);
                card.SetMission(120, "The Axis Forge", "Mission-Critical Systems", 6, sample, null);
                card.SetState("Preview", Cyan, null, false, Cyan);
            }
            else
                root.gameObject.layer = parent.gameObject.layer;
            root.gameObject.SetActive(true);
            return card;
        }

        private static TextMeshProUGUI BottomText(string name, RectTransform parent, float left, float right,
            float bottom, float top, int size, Color color, bool bold = false)
        {
            var label = F.Text(name, parent, "", size, color, bold: bold);
            Place(label.rectTransform, 0, 0, 1, 0, left, bottom, -right, top);
            return label;
        }

        private static void Place(RectTransform rect, float xMin, float yMin, float xMax, float yMax,
            float left, float bottom, float right, float top)
        {
            rect.anchorMin = new Vector2(xMin, yMin);
            rect.anchorMax = new Vector2(xMax, yMax);
            rect.offsetMin = new Vector2(left, bottom);
            rect.offsetMax = new Vector2(right, top);
        }
    }
}

