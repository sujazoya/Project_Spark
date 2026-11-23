using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using F = ProjectSpark.AAAArchitecture.SparkAAAUIFactory;

namespace ProjectSpark.AAAArchitecture
{
    /// <summary>
    /// Shared presentation card for SparkCampaignPanel and SparkAAAMissionPanel.
    /// Neither API grants level access, changes progression or starts gameplay.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkAAALevelCard : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private const string GeneratedRoot = "Assets/ProjectSpark/AAAArchitecture/Generated";
        private const string VisualResourceRoot = "SparkAAAVisuals/";

        // Preserve serialized names and types for previously generated prefabs.
        [SerializeField] private Button button;
        public Button Button => button;
        [SerializeField] private Image frame, preview, progress, accent, badge;
        [SerializeField] private TextMeshProUGUI number, title, domain, status, count, placeholder;

        private Action<SparkAAALevel> selected;
        private SparkAAALevel item;
        private Action missionSelected;
        private Coroutine animation;

        private void OnEnable()
        {
            if (button == null) button = GetComponent<Button>();
            if (button == null) return;
            button.onClick.RemoveListener(Click);
            button.onClick.AddListener(Click);
        }

        private void OnDisable()
        {
            if (button != null) button.onClick.RemoveListener(Click);
            ResetHover();
        }

        private void Click()
        {
            if (missionSelected != null)
                missionSelected.Invoke();
            else if (item != null)
                selected?.Invoke(item);
        }

        /// <summary>Responsive height for the existing campaign GridLayoutGroup.</summary>
        public static float HeightForWidth(float width)
        {
            if (float.IsNaN(width) || float.IsInfinity(width) || width <= 0f)
                return 340f;

            return Mathf.Ceil(130 + Mathf.Clamp(width * .55f, 116, 173));
        }

        /// <summary>
        /// Resolve exact-level artwork only. No deceptive chapter-level substitute.
        /// The chapter parameter is retained for compatibility with SparkCampaignPanel.
        /// </summary>
        public static Sprite FindArtwork(int levelNumber, int chapterNumber)
        {
            if (levelNumber <= 0) return null;
            string key = "PS-L" + levelNumber.ToString("000");

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                var catalog = UnityEditor.AssetDatabase.LoadAssetAtPath<SparkAAACatalog>(
                    GeneratedRoot + "/SparkCampaignCatalog.asset");
                var entry = catalog ? catalog.Find(key) : null;
                if (entry != null && entry.Present && !entry.blocked && entry.thumbnail)
                    return entry.thumbnail;
            }
#endif
            string path = VisualResourceRoot + key + "/";
            Sprite artwork = Resources.Load<Sprite>(path + "Thumbnail");
            if (!artwork) artwork = Resources.Load<Sprite>(path + "Banner");

            // Exact-level previews exported from the project's creation kits cover unrendered missions.
            return artwork ? artwork : Resources.Load<Sprite>($"SparkMissionArtwork/L{levelNumber:000}");
        }

        /// <summary>Existing campaign-panel API. Rebinding replaces the old click callback.</summary>
        public void SetMission(int levelNumber, string missionTitle, string chapter,
            int targetCount, Sprite artwork, Action onSelected)
        {
            item = null;
            selected = null;
            missionSelected = onSelected;
            ResetHover();

            SetIdentity(levelNumber, missionTitle, chapter,
                ObjectiveLabel(targetCount), artwork, "ENGINEERING\nMISSION");

            // Reset pooled state until the campaign supplies the authoritative status.
            SetState("Unknown", F.Muted, null, false, F.Muted);
        }

        /// <summary>
        /// Existing campaign-panel API. Nullable progress means no validated reading.
        /// Locked cards remain selectable for briefing; the panel gates the launch.
        /// </summary>
        public void SetState(string label, Color stateColor, float? fraction,
            bool active, Color theme)
        {
            frame.color = active ? theme : F.Outline;
            accent.color = active ? theme : F.C("#537080");
            status.text = string.IsNullOrWhiteSpace(label) ? "UNKNOWN" : label.ToUpperInvariant();
            status.color = stateColor;
            if (badge)
                badge.color = Color.Lerp(new Color32(10, 22, 33, 255), stateColor, .17f);

            bool hasProgress = fraction.HasValue &&
                !float.IsNaN(fraction.Value) && !float.IsInfinity(fraction.Value);
            SetProgress(hasProgress ? fraction.Value : 0f, stateColor, hasProgress);
        }

        /// <summary>Original AAA mission-panel API retained without signature changes.</summary>
        public void Bind(SparkAAALevel level, string chapter, SparkAAAProgress state,
            bool active, Color theme, Action<SparkAAALevel> callback)
        {
            missionSelected = null;
            item = level;
            selected = callback;
            ResetHover();

            if (level == null)
            {
                SetIdentity(0, "Unavailable", chapter, "SOURCE REQUIRED", null, "MISSING\nDEFINITION");
                SetState("Unavailable", F.Muted, null, active, theme);
                return;
            }

            bool missing = !level.Present || level.blocked;
            int targetCount = level.targets == null ? 0 : level.targets.Count;
            SetIdentity(level.number, level.title, chapter,
                level.Present ? ObjectiveLabel(targetCount) : "SOURCE REQUIRED",
                missing ? null : level.thumbnail ? level.thumbnail : FindArtwork(level.number, level.chapter),
                level.Present ? "ENGINEERING\nMISSION" : "MISSING\nDEFINITION");

            bool unknown = state == null || state.status == SparkAAAStatus.Unknown;
            string label = missing ? "UNAVAILABLE" :
                unknown ? "PREVIEW" : state.status == SparkAAAStatus.InProgress ? "IN PLAY" : state.status.ToString();
            Color stateColor = missing ? F.Muted :
                !unknown && state.status == SparkAAAStatus.Certified ? F.C("#55D4A0") :
                !unknown && state.status == SparkAAAStatus.Requalification ? F.C("#F2B167") : theme;

            float? fraction = !missing && !unknown ? (float?)state.fraction : null;
            SetState(label, stateColor, fraction, active, theme);
        }

        private void SetIdentity(int levelNumber, string missionTitle, string chapter,
            string targetLabel, Sprite artwork, string placeholderLabel)
        {
            number.text = $"LEVEL {levelNumber:00}";
            title.text = missionTitle ?? string.Empty;
            domain.text = chapter ?? string.Empty;
            count.text = targetLabel;

            // Clearing first ensures a recycled card never shows the old mission.
            preview.sprite = artwork;
            preview.color = artwork != null ? Color.white : F.C("#0A131B");
            placeholder.text = placeholderLabel;
            placeholder.gameObject.SetActive(artwork == null);
        }

        private void SetProgress(float fraction, Color color, bool visible)
        {
            progress.color = color;
            RectTransform rect = progress.rectTransform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = new Vector2(Mathf.Clamp01(fraction), 1f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            progress.gameObject.SetActive(visible);
        }

        public void OnPointerEnter(PointerEventData e) => Animate(1.018f);
        public void OnPointerExit(PointerEventData e) => Animate(1f);

        private void ResetHover()
        {
            if (animation != null) StopCoroutine(animation);
            animation = null;
            transform.localScale = Vector3.one;
        }

        private void Animate(float scale)
        {
            if (!isActiveAndEnabled) return;
            if (animation != null) StopCoroutine(animation);
            animation = StartCoroutine(Scale(scale));
        }

        private IEnumerator Scale(float target)
        {
            float t = 0f;
            Vector3 from = transform.localScale;
            while (t < .16f)
            {
                t += Time.unscaledDeltaTime;
                transform.localScale = Vector3.Lerp(from, Vector3.one * target,
                    Mathf.SmoothStep(0f, 1f, t / .16f));
                yield return null;
            }

            transform.localScale = Vector3.one * target;
            animation = null;
        }

        // Both factory overloads are supported.
        public static SparkAAALevelCard Create() => Create(null);

        public static SparkAAALevelCard Create(Transform parent)
        {
            RectTransform r = F.Rect("SparkLevelButton", null);

            // AddComponent can invoke OnEnable immediately on active objects.
            // Keep the new card inactive until all serialized references are assigned.
            r.gameObject.SetActive(false);
            if (parent != null) r.SetParent(parent, false);

            var card = r.gameObject.AddComponent<SparkAAALevelCard>();
            card.frame = F.Image(r, F.Outline);
            card.frame.raycastTarget = true;
            card.button = r.gameObject.AddComponent<Button>();
            card.button.targetGraphic = card.frame;
            card.button.transition = Selectable.Transition.None;

            r.sizeDelta = new Vector2(300, HeightForWidth(300));
            var body = F.Rect("Surface", r);
            F.Stretch(body, 2, 2, 2, 2);
            Image surface = F.Image(body, Color.white);
            card.button.targetGraphic = surface;
            card.button.transition = Selectable.Transition.ColorTint;
            var colors = card.button.colors;
            colors.normalColor = F.Surface;
            colors.highlightedColor = F.C("#19374B");
            colors.selectedColor = colors.highlightedColor;
            colors.pressedColor = F.C("#235162");
            colors.fadeDuration = .12f;
            card.button.colors = colors;

            var hero = F.Rect("HardwarePreview", body);
            F.Stretch(hero, 0, 128, 0, 0);
            F.Image(hero, F.C("#0A131B"));
            var artwork = F.Rect("MissionArtwork", hero);
            F.Stretch(artwork);
            card.preview = F.Image(artwork, F.C("#0A131B"));
            card.placeholder = F.Text("PreviewPlaceholder", hero, "No mission artwork", 16, F.Muted);
            F.Stretch(card.placeholder.rectTransform, 6, 6, 6, 6);
            card.placeholder.alignment = TextAlignmentOptions.Center;

            var numberPlate = F.Rect("LevelPlate", hero);
            SetBand(numberPlate, 0, 1, 0, 1, 9, -37, 132, -9);
            F.Image(numberPlate, F.C("#0B1822"));
            card.number = F.Text("LevelNumber", numberPlate, "", 16, F.Ink, bold: true);
            card.number.enableAutoSizing = true;
            card.number.fontSizeMin = 12;
            card.number.fontSizeMax = 16;
            card.number.textWrappingMode = TextWrappingModes.NoWrap;
            F.Stretch(card.number.rectTransform, 8, 0, 3, 0);
            var statusPlate = F.Rect("AccessPlate", hero);
            SetBand(statusPlate, 1, 1, 1, 1, -116, -37, -9, -9);
            card.badge = F.Image(statusPlate, F.C("#142839"));
            card.status = F.Text("State", statusPlate, "", 13, F.Muted, bold: true);
            F.Stretch(card.status.rectTransform, 4, 0, 4, 0);
            card.status.alignment = TextAlignmentOptions.Center;

            var divider = F.Rect("Accent", body);
            SetBand(divider, 0, 0, 1, 0, 0, 126, 0, 128);
            card.accent = F.Image(divider, F.Outline);
            card.title = FooterLabel("MissionTitle", body, 12, 12, 75, 120, 23, F.Ink, true);
            card.title.enableAutoSizing = true;
            card.title.fontSizeMin = 16;
            card.title.fontSizeMax = 23;
            card.domain = FooterLabel("Division", body, 12, 12, 51, 76, 15, F.Muted);
            card.count = FooterLabel("TargetCount", body, 12, 100, 19, 48, 15, F.Ink);
            var details = FooterLabel("Details", body, 100, 12, 19, 48, 13, F.C("#77DEF4"), true);
            details.text = "DETAILS  >";
            details.alignment = TextAlignmentOptions.MidlineRight;

            var track = F.Rect("ProgressTrack", body);
            SetBand(track, 0, 0, 1, 0, 12, 10, -12, 13);
            F.Image(track, F.Outline);
            var fill = F.Rect("ProgressFill", track);
            F.Stretch(fill);
            card.progress = F.Image(fill, Color.cyan);
            card.SetProgress(0f, F.Muted, false);

            if (!parent)
            {
                card.SetMission(120, "The Axis Forge", "Mission-Critical Systems", 6, FindArtwork(120, 12), null);
                card.SetState("Preview", F.C("#77DEF4"), null, false, F.Outline);
            }

            // Match the existing campaign UI layer without changing the factory API.
            if (parent != null)
                SetLayer(r, parent.gameObject.layer);

            r.gameObject.SetActive(true);
            return card;
        }

        private static string ObjectiveLabel(int count) => count == 1 ? "1 objective" : $"{Mathf.Max(0, count)} objectives";

        private static TextMeshProUGUI FooterLabel(string name, RectTransform parent, float left, float right,
            float bottom, float top, int size, Color color, bool bold = false)
        {
            var text = F.Text(name, parent, "", size, color, bold: bold);
            SetBand(text.rectTransform, 0, 0, 1, 0, left, bottom, -right, top);
            return text;
        }

        private static void SetBand(RectTransform rect, float xMin, float yMin, float xMax, float yMax,
            float left, float bottom, float right, float top)
        {
            rect.anchorMin = new Vector2(xMin, yMin);
            rect.anchorMax = new Vector2(xMax, yMax);
            rect.offsetMin = new Vector2(left, bottom);
            rect.offsetMax = new Vector2(right, top);
        }

        private static void SetLayer(Transform node, int layer)
        {
            node.gameObject.layer = layer;
            for (int i = 0; i < node.childCount; i++)
                SetLayer(node.GetChild(i), layer);
        }
    }
}
