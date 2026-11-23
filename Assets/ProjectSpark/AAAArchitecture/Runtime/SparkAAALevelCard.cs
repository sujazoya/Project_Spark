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
        [SerializeField] private Image badge, scan, focusFrame;
        [SerializeField] private Image stateIcon;
        [SerializeField] private Image neonNumberPlate, neonBezel, neonFooter;
        [SerializeField] private TextMeshProUGUI neonDetails;
        private int boundLevel;
        private Color displayColor;
        private Color hoverColor;
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

            // Keep the label and status area fixed; scale the hardware artwork with width.
            return Mathf.Ceil(Mathf.Max(196f, width * .65f + 83f));
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
                if (entry.thumbnail != null && (Application.isMobilePlatform || entry.thumbnail.rect.width >= 768)) return entry.thumbnail;
                var full = string.IsNullOrEmpty(entry.bannerResourcePath) ? null : Resources.Load<Sprite>(entry.bannerResourcePath);
                return full ? full : entry.thumbnail;
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
            Color missionColor = SparkProfessionalSkin.MissionColor(boundLevel, theme);
            bool neon = SparkProfessionalSkin.NeonEnabled;
            displayColor = active ? SparkCinematicSkin.Gold : neon ? SparkProfessionalSkin.Glow(missionColor,.90f) : Color.clear;
            hoverColor=active ? SparkCinematicSkin.Gold : missionColor;
            frame.color = frame.sprite || !badge ? displayColor : Color.clear;
            accent.color = active ? SparkCinematicSkin.Gold : neon ? SparkProfessionalSkin.Glow(missionColor,1f) : new Color(theme.r,theme.g,theme.b,.25f);
            if (neon)
            {
                if (neonNumberPlate) neonNumberPlate.color = SparkProfessionalSkin.SurfaceTint(missionColor,.68f);
                if (neonBezel) neonBezel.color = SparkProfessionalSkin.SurfaceTint(missionColor,.78f);
                if (neonFooter) neonFooter.color = SparkProfessionalSkin.SurfaceTint(missionColor,.22f);
                if (neonDetails) neonDetails.color = missionColor;
                if (scan) scan.color = SparkProfessionalSkin.Glow(active ? SparkCinematicSkin.Gold : missionColor,.72f);
                domain.color = Color.Lerp(missionColor, Color.white,.24f);
                number.color = Color.Lerp(missionColor, Color.white,.42f);
            }
            if (badge) badge.color = Color.Lerp(new Color(.78f,.78f,.78f), stateColor, .15f);
            if (focusFrame) focusFrame.color = Color.clear;
            status.text = string.IsNullOrWhiteSpace(label) ? "UNKNOWN" : label.ToUpperInvariant();
            if (status.text == "ACCESS NOT VERIFIED") status.text = "UNVERIFIED";
            if (neon)
            {
                switch (status.text)
                {
                    case "LOCKED": stateColor = new Color32(255,192,66,255); break;
                    case "COMPLETE": case "COMPLETED": case "CERTIFIED": stateColor = new Color32(58,255,170,255); break;
                    case "AVAILABLE": stateColor = new Color32(55,227,255,255); break;
                    case "IN PLAY": case "INPROGRESS": case "IN PROGRESS": stateColor = new Color32(191,132,255,255); break;
                    case "RETRY": case "FAILED": stateColor = new Color32(255,95,115,255); break;
                }
                if (badge) badge.color = SparkProfessionalSkin.SurfaceTint(stateColor,.54f);
            }
            status.color = stateColor;
            if (stateIcon)
            {
                string glyph = status.text == "LOCKED" ? "LockedIcon" :
                    status.text == "COMPLETE" || status.text == "COMPLETED" || status.text == "CERTIFIED" ? "CompleteIcon" :
                    status.text == "AVAILABLE" || status.text == "IN PLAY" || status.text == "INPROGRESS" || status.text == "IN PROGRESS" ? "ReadyIcon" : "UnknownIcon";
                stateIcon.sprite = SparkProfessionalSkin.Get(glyph); stateIcon.color = stateColor; stateIcon.enabled = stateIcon.sprite != null;
            }

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
            boundLevel = levelNumber;
            number.text = badge ? "LEVEL " + levelNumber.ToString("000") : levelNumber.ToString("000");
            title.text = missionTitle ?? string.Empty;
            domain.text = chapter ?? string.Empty;
            count.text = targetLabel;

            // Clearing first ensures a recycled card never shows the old mission.
            preview.sprite = artwork;
            preview.color = artwork != null ? Color.white : (badge ? Color.clear : F.C("#0A131B"));
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
            if (scan) scan.gameObject.SetActive(false);
            if (focusFrame) focusFrame.color = Color.clear;
        }

        private void Animate(float scale)
        {
            if (!isActiveAndEnabled) return;
            if (SparkCinematicClock.ReducedMotion) { ResetHover(); return; }
            if (animation != null) StopCoroutine(animation);
            if (focusFrame && focusFrame.sprite) focusFrame.color = scale > 1 ? new Color(hoverColor.r, hoverColor.g, hoverColor.b, .6f) : Color.clear;
            if (scan) scan.gameObject.SetActive(scale > 1 && scan.sprite != null);
            animation = StartCoroutine(Scale(scale));
        }

        private IEnumerator Scale(float target)
        {
            float t = 0f;
            bool sweeping = target > 1 && scan && scan.gameObject.activeSelf;
            float duration = sweeping ? .55f : .16f;
            Vector3 from = transform.localScale;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                transform.localScale = Vector3.Lerp(from, Vector3.one * target,
                    Mathf.SmoothStep(0f, 1f, t / .16f));
                if (sweeping && scan) { var sr = scan.rectTransform; float y = Mathf.Lerp(.94f,.06f,Mathf.Clamp01(t/duration)); sr.anchorMin = new Vector2(0,y); sr.anchorMax = new Vector2(1,y); sr.offsetMin = new Vector2(0,-9); sr.offsetMax = new Vector2(0,9); }
                yield return null;
            }

            transform.localScale = Vector3.one * target;
            if (scan) scan.gameObject.SetActive(false);
            animation = null;
        }

        // Both factory overloads are supported.
        public static SparkAAALevelCard Create() => Create(null);

        public static SparkAAALevelCard Create(Transform parent)
        {
            RectTransform r = F.Rect("SparkLevelButton", null);
            r.sizeDelta = new Vector2(320f, HeightForWidth(320f));

            // AddComponent can invoke OnEnable immediately on active objects.
            // Keep the new card inactive until all serialized references are assigned.
            r.gameObject.SetActive(false);
            if (parent != null) r.SetParent(parent, false);

            var card = r.gameObject.AddComponent<SparkAAALevelCard>();
            var bg = F.Image(r, F.Surface); bg.raycastTarget = true;
            SparkProfessionalSkin.Apply(bg,"GalleryCard");
            card.button = r.gameObject.AddComponent<Button>();
            card.button.targetGraphic = bg;
            card.button.transition = Selectable.Transition.ColorTint;
            var colors = card.button.colors;
            colors.normalColor=Color.white; colors.highlightedColor=new Color(.92f,1f,1f); colors.selectedColor=new Color(.82f,.96f,1f);
            colors.pressedColor=new Color(.72f,.82f,.88f); colors.disabledColor=new Color(.5f,.5f,.5f); colors.fadeDuration=.10f;
            card.button.colors=colors;
            var hero = Slot("HardwarePreview",r,0,0,1,1,4,88,-4,-4);
            var heroBg=F.Image(hero,F.C("#09151F")); SparkProfessionalSkin.Apply(heroBg,"Chamber");
            
            var art=Slot("Artwork",hero,0,0,1,1,9,6,-9,-32);
            card.preview=F.Image(art,Color.clear); card.preview.preserveAspect=true;
            card.placeholder=Label("PreviewPlaceholder",hero,"ENGINEERING MISSION",15,F.Muted,0,0,1,1,12,0,-12,0,false);
            card.placeholder.alignment=TextAlignmentOptions.Center;
            var shade=F.Image(Slot("Artwork Fade",hero,0,0,1,1,0,0,0,0),Color.white); SparkProfessionalSkin.Apply(shade,"Fade");
            var scanRect=Slot("Scan Sweep",hero,0,.5f,1,.5f,4,-9,-4,9);
            card.scan=F.Image(scanRect,Color.clear);
            if(SparkCinematicSkin.Get("Scan")) SparkCinematicSkin.Apply(card.scan,"Scan",new Color(.36f,.9f,1f,.60f),false);
            card.scan.gameObject.SetActive(false);
            var footer=F.Image(Slot("Mission Information",r,0,0,1,0,4,4,-4,89),F.Surface); SparkProfessionalSkin.Apply(footer,"CardFooter");
            var bezel=F.Image(Slot("Hardware Bezel",hero,0,0,1,1,0,0,0,0),Color.clear); if (SparkProfessionalSkin.Get("CardBezel")) SparkProfessionalSkin.Apply(bezel,"CardBezel"); else bezel.color=Color.clear; bezel.raycastTarget=false;
            var head=Slot("Level Plate",r,0,1,.48f,1,11,-38,-3,-8);
            var headImage=F.Image(head,F.Surface); SparkProfessionalSkin.Apply(headImage,"Index");
            card.neonNumberPlate=headImage; card.neonBezel=bezel; card.neonFooter=footer;
            card.number=Label("LevelNumber",head,"LEVEL 000",19,SparkCinematicSkin.Ink,0,0,1,1,8,0,-4,0,true);
            var statePlate=Slot("Status Plate",r,.48f,1,1,1,3,-36,-11,-8);
            card.badge=F.Image(statePlate,F.Outline); SparkProfessionalSkin.Apply(card.badge,"Notice");
            card.stateIcon=F.Image(Slot("Status Icon",statePlate,0,.5f,0,.5f,8,-9,26,9),Color.white); card.stateIcon.preserveAspect=true; card.stateIcon.raycastTarget=false;
            card.status=Label("State",statePlate,"UNKNOWN",14,Color.white,0,0,1,1,29,0,-5,0,true);
            card.status.alignment=TextAlignmentOptions.Center;
            // Status stays readable over the darker neutral badge; tint text rather than fill.
            card.title=Label("MissionTitle",r,"Mission",24,F.Ink,0,0,1,0,13,37,-13,86,true);
            card.title.textWrappingMode=TextWrappingModes.Normal;
            card.domain=Label("Division",r,"Division",14,F.Muted,0,0,1,0,14,22,-14,39,false);
            card.domain.gameObject.SetActive(true);
            card.count=Label("TargetCount",r,"0 TARGETS",13,F.Muted,0,0,.65f,0,14,5,0,20,false);
            var detail=Label("Details",r,"DETAILS  ›",13,SparkCinematicSkin.Cyan,.65f,0,1,0,0,5,-14,20,true);
            detail.alignment=TextAlignmentOptions.MidlineRight; card.neonDetails=detail;
            card.accent=F.Image(Slot("Accent",r,0,0,1,0,4,88,-4,90),SparkCinematicSkin.Cyan);
            var track=Slot("ProgressTrack",r,0,0,1,0,4,1,-4,3); F.Image(track,F.Outline);
            var fill=F.Rect("ProgressFill",track); F.Stretch(fill); card.progress=F.Image(fill,SparkCinematicSkin.Cyan);
            card.SetProgress(0f,F.Muted,false);
            card.frame=SparkProfessionalSkin.Frame(r,SparkCinematicSkin.Neutral);
            card.focusFrame=SparkProfessionalSkin.Frame(r,Color.clear,"Focus Highlight",3);
            card.frame.raycastTarget=false;
            // Match the existing campaign UI layer without changing the factory API.
            if (parent != null)
                SetLayer(r, parent.gameObject.layer);

            r.gameObject.SetActive(true);
            return card;
        }

        private static RectTransform Slot(string name, Transform parent, float ax,float ay,float bx,float by,float l,float b,float r,float t)
        { return SparkCinematicSkin.Rect(name,parent,new Vector2(ax,ay),new Vector2(bx,by),new Vector2(l,b),new Vector2(r,t)); }
        private static TextMeshProUGUI Label(string name, Transform parent,string value,float size,Color color,float ax,float ay,float bx,float by,float l,float b,float r,float t,bool bold)
        {
            var rect=Slot(name,parent,ax,ay,bx,by,l,b,r,t);
            var text=rect.gameObject.AddComponent<TextMeshProUGUI>();
            text.font=TMP_Settings.defaultFontAsset; text.text=value; text.fontSize=size; text.color=color;
            text.alignment=TextAlignmentOptions.MidlineLeft; text.raycastTarget=false; text.richText=false;
            text.textWrappingMode=TextWrappingModes.NoWrap; text.overflowMode=TextOverflowModes.Ellipsis;
            text.enableAutoSizing=true; text.fontSizeMin=Mathf.Max(10,size*.72f); text.fontSizeMax=size;
            SparkCinematicSkin.StyleText(text,bold); SparkProfessionalSkin.ApplyTypography(text,bold); return text;
        }

        private static void SetLayer(Transform node, int layer)
        {
            node.gameObject.layer = layer;
            for (int i = 0; i < node.childCount; i++)
                SetLayer(node.GetChild(i), layer);
        }
    }
}
