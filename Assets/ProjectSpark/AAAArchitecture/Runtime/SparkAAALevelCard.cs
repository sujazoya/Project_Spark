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
        [SerializeField] private Image frame, preview, progress, accent;
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

            // 227 = text/track heights + seven gaps + padding + frame inset.
            // The preview grows toward 16:9; keep the original 340 minimum.
            return Mathf.Ceil(Mathf.Max(340f, 227f + Mathf.Max(96f, (width - 24f) * 9f / 16f)));
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
            // The visual architect stores thumbnails outside Resources.
            // Prefer its catalog, which incorporates source approval checks.
            var catalog = UnityEditor.AssetDatabase.LoadAssetAtPath<SparkAAACatalog>(
                GeneratedRoot + "/SparkCampaignCatalog.asset");
            if (catalog != null)
            {
                SparkAAALevel entry = catalog.Find(key);
                if (entry == null || !entry.Present || entry.blocked) return null;
                if (entry.thumbnail != null) return entry.thumbnail;
                return string.IsNullOrEmpty(entry.bannerResourcePath)
                    ? null : Resources.Load<Sprite>(entry.bannerResourcePath);
            }
#endif
            // In player builds AssetDatabase is unavailable. The visual architect
            // already exports the mission banner under Resources. An optional
            // exact-level Thumbnail sprite at this location takes precedence.
            string resourcePath = VisualResourceRoot + key + "/";
            Sprite artwork = Resources.Load<Sprite>(resourcePath + "Thumbnail");
            return artwork != null ? artwork : Resources.Load<Sprite>(resourcePath + "Banner");
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
                Mathf.Max(0, targetCount) + " TARGETS", artwork, "ENGINEERING\nMISSION");

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
                level.Present ? targetCount + " TARGETS" : "SOURCE REQUIRED",
                missing ? null : level.thumbnail,
                level.Present ? "ENGINEERING\nMISSION" : "MISSING\nDEFINITION");

            bool unknown = state == null || state.status == SparkAAAStatus.Unknown;
            string label = missing ? "UNAVAILABLE" :
                unknown ? "ACCESS NOT VERIFIED" : state.status.ToString();
            Color stateColor = missing ? F.Muted :
                !unknown && state.status == SparkAAAStatus.Certified ? F.C("#55D4A0") :
                !unknown && state.status == SparkAAAStatus.Requalification ? F.C("#F2B167") : theme;

            float? fraction = !missing && !unknown ? (float?)state.fraction : null;
            SetState(label, stateColor, fraction, active, theme);
        }

        private void SetIdentity(int levelNumber, string missionTitle, string chapter,
            string targetLabel, Sprite artwork, string placeholderLabel)
        {
            number.text = levelNumber.ToString("000");
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

            var body = F.Rect("Surface", r);
            F.Stretch(body, 2, 2, 2, 2);
            F.Image(body, F.Surface);
            F.Vertical(body, 10, 6);

            var hero = F.Rect("HardwarePreview", body);
            F.Size(hero, -1, 96, -1, 1);
            card.preview = F.Image(hero, F.C("#0A131B"));
            card.preview.preserveAspect = true;
            card.placeholder = F.Text("PreviewPlaceholder", hero, "ENGINEERING\nMISSION", 16, F.Muted);
            F.Stretch(card.placeholder.rectTransform, 6, 6, 6, 6);
            card.placeholder.alignment = TextAlignmentOptions.Center;

            card.accent = F.Image(F.Rect("Accent", body), F.Outline);
            F.Size(card.accent.rectTransform, -1, 2);
            card.number = F.Text("LevelNumber", body, "000", 34, F.Ink, 37, true);
            card.title = F.Text("MissionTitle", body, "Mission", 21, F.Ink, 49, true);
            card.domain = F.Text("Division", body, "Division", 16, F.Muted, 23);
            card.count = F.Text("TargetCount", body, "0 TARGETS", 16, F.Muted, 23);
            card.status = F.Text("State", body, "UNKNOWN", 16, F.Muted, 23, true);

            var track = F.Rect("ProgressTrack", body);
            F.Size(track, -1, 4);
            F.Image(track, F.Outline);
            var fill = F.Rect("ProgressFill", track);
            F.Stretch(fill);
            card.progress = F.Image(fill, Color.cyan);
            card.progress.type = Image.Type.Simple;
            card.SetProgress(0f, F.Muted, false);

            // Match the existing campaign UI layer without changing the factory API.
            if (parent != null)
                SetLayer(r, parent.gameObject.layer);

            r.gameObject.SetActive(true);
            return card;
        }

        private static void SetLayer(Transform node, int layer)
        {
            node.gameObject.layer = layer;
            for (int i = 0; i < node.childCount; i++)
                SetLayer(node.GetChild(i), layer);
        }
    }
}
