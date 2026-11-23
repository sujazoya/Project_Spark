using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using ProjectSpark.AAAArchitecture;
using ProjectSpark.AI.Tutorial;
using ProjectSpark.Gameplay;
using ProjectSpark.UIManagement;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace ProjectSpark.UI
{
    /// <summary>Chapter and mission browser for the active workbench campaign.</summary>
    [DisallowMultipleComponent]
    public sealed class SparkCampaignPanel : MonoBehaviour
    {
        private const string PanelId = "Campaign";
        private const int ModalSortingOrder = 31500;

        private static readonly Color32 Background = new(5, 13, 21, 255);
        private static readonly Color32 Surface = new(12, 27, 40, 255);
        private static readonly Color32 Divider = new(35, 71, 88, 255);
        private static readonly Color32 Accent = new(107, 225, 250, 255);
        private static readonly Color32 TextColor = new(234, 245, 252, 255);
        private static readonly Color32 Muted = new(157, 186, 204, 255);
        private static readonly Color32 Complete = new(119, 233, 175, 255);
        private static readonly Color32 Locked = new(246, 203, 140, 255);

        private sealed class ChapterRow
        {
            public SparkChapterDefinition Chapter;
            public Button Button;
            public TMP_Text Status, Number;
            public Image Icon, NeonRail;
        }

        private sealed class ObjectiveRow
        {
            public RectTransform Root;
            public TMP_Text Heading, Body;
        }

        private sealed class MissionRow
        {
            public int Index;
            public Sprite Artwork;
            public SparkAAALevelCard Card;
        }

        private readonly List<ChapterRow> chapters = new();
        private readonly List<MissionRow> missions = new();
        private readonly List<ObjectiveRow> objectiveRows = new();
        private RectTransform objectiveContainer;
        private Image accessPlate;
        private LevelGamePlayManager manager;
        private SparkLevelProgression progression;
        private TMP_FontAsset font;
        private SparkUIManager uiManager;
        private SparkUIPanel uiRegistration;
        private RectTransform overlay, shell, chapterColumn, missionColumn, briefColumn, body;
        private ScrollRect chapterScroll, missionScroll, briefScroll;
        private GridLayoutGroup missionGrid;
        private Image briefArtwork;
        private SparkLivePreview livePreview;
        private SparkFinalPresentation finalPresentation;
        private RectTransform header, compactTabs, headerTitleRect, showcase, cover, galleryHeading;
        private TMP_Text campaignEyebrow;
        private TMP_Text showcaseTitle, showcaseIndex, showcaseState, galleryCount;
        private bool showcaseWide;
        private Image showcaseRail;
        private int cachedBannerLevel = -1;
        private Sprite cachedBanner;
        private Button previousMission, nextMission;
        private Image chapterIcon;
        private ContentSizeFitter missionFitter;
        private readonly List<Image> disciplineIcons = new();
        private Button themeButton, motionButton;
        private TMP_Text themeLabel, motionLabel;
        private readonly Button[] pageButtons = new Button[3];
        private bool narrow;
        private int narrowPage = 1;
        private Coroutine launchRoutine;
        private bool launching;
        private Color ThemeAccent => finalPresentation ? finalPresentation.Accent : (Color)Accent;
        private int lastSatisfied = -1, lastTargetCount = -1;
        private Button opener, close, launch, resume;
        private TMP_Text total, chapterHeading, chapterDescription, briefNumber, briefTitle;
        private TMP_Text description, rule, objectivesHeading, objectives, status, launchText;
        private RectTransform completionFill;
        private SparkChapterDefinition selectedChapter;
        private int selectedIndex = -1;
        private Vector2 lastSize;
        private GameObject lastFocus;
        private bool launchFailed;

        public bool IsOpen => overlay && overlay.gameObject.activeSelf;

        public void Initialize(LevelGamePlayManager levelManager, RectTransform hud, RectTransform navigation,
            TMP_FontAsset labelFont, Action beforeOpen)
        {
            manager = levelManager;
            progression = manager.LevelProgression;
            font = labelFont;
            if (!manager.Campaign)
                return;

            opener = CreateButton("Chapters", navigation, "Chapters", 15);
            var openerRect = (RectTransform)opener.transform;
            openerRect.anchorMin = openerRect.anchorMax = new Vector2(.5f, 0);
            openerRect.pivot = new Vector2(.5f, 1);
            openerRect.anchoredPosition = new Vector2(0, -4);
            openerRect.sizeDelta = new Vector2(146, 32);
            opener.onClick.AddListener(() =>
            {
                beforeOpen?.Invoke();
                Open();
            });

            Build(hud);
            uiManager = SparkUIManager.Instance;
            if (uiManager)
                uiRegistration = uiManager.RegisterNative(PanelId, "Chapters", SparkUILayer.Modal,
                    "", true, SetVisible, () => IsOpen);
            if (progression)
                progression.ProgressChanged += Refresh;
            manager.LevelStarted += OnLevelStarted;
        }

        private void OnDestroy()
        {
            if (uiManager && uiRegistration)
            {
                uiManager.Unregister(PanelId);
                Destroy(uiRegistration.gameObject);
            }
            if (progression)
                progression.ProgressChanged -= Refresh;
            if (manager)
                manager.LevelStarted -= OnLevelStarted;
            if (overlay)
                Destroy(overlay.gameObject);
        }

        private void OnDisable() => Close();

        private void Update()
        {
            if (!IsOpen)
                return;
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                Close();
                return;
            }

            if (launching) return;
            Fit();
            FitMissionGrid();
            if (lastSatisfied != manager.SatisfiedTargetCount || lastTargetCount != manager.ActiveTargetCount)
                Refresh();

            GameObject focus = EventSystem.current ? EventSystem.current.currentSelectedGameObject : null;
            if (focus == lastFocus || !focus)
                return;
            if (!focus.transform.IsChildOf(overlay))
            {
                close.Select();
                return;
            }
            lastFocus = focus;
            RectTransform target = focus.transform as RectTransform;
            Reveal(chapterScroll, target);
            Reveal(missionScroll, target);
        }

        public void Open()
        {
            if (uiManager && uiRegistration)
                uiManager.Open(PanelId);
            else
                Show();
        }

        public void Close()
        {
            if (uiManager && uiRegistration)
                uiManager.Close(PanelId);
            else
                Hide();
        }

        private void SetVisible(bool visible)
        {
            if (visible)
                Show();
            else
                Hide();
        }

        private void Show()
        {
            if (!overlay || IsOpen)
                return;

            overlay.gameObject.SetActive(true);
            narrowPage = 1;
            lastSize = Vector2.zero;
            Fit();
            if (finalPresentation) finalPresentation.Show();
            var active = manager.LayoutLoader ? manager.LayoutLoader.ActiveLayout : null;
            int index = active ? manager.GetLevelIndex(active.LevelDefinition) : ResumeIndex();
            SparkChapterDefinition chapter = manager.Campaign.FindChapter(manager.GetLevel(index));
            SelectChapter(chapter ? chapter : manager.Campaign.GetChapter(0), index);
            close.Select();
            ChapterRow chapterRow = chapters.Find(row => row.Chapter == selectedChapter);
            MissionRow missionRow = missions.Find(row => row.Index == selectedIndex);
            Canvas.ForceUpdateCanvases();
            FitMissionGrid();
            Canvas.ForceUpdateCanvases();
            if (chapterRow != null)
                Reveal(chapterScroll, (RectTransform)chapterRow.Button.transform);
            if (missionRow != null)
                Reveal(missionScroll, (RectTransform)missionRow.Card.transform);
        }

        private void Hide()
        {
            if (!IsOpen)
                return;

            if (launchRoutine != null) StopCoroutine(launchRoutine);
            launchRoutine = null;
            launching = false;
            if (finalPresentation) finalPresentation.EndLaunch();
            overlay.gameObject.SetActive(false);
            cachedBanner=null; cachedBannerLevel=-1;
            lastFocus = null;
            if (isActiveAndEnabled && opener)
                opener.Select();
        }

        private void Build(RectTransform hud)
        {
            var root = new GameObject("Campaign Browser", typeof(RectTransform), typeof(Canvas),
                typeof(CanvasScaler), typeof(GraphicRaycaster));
            root.layer = hud.gameObject.layer;
            overlay = (RectTransform)root.transform;
            var modalCanvas = root.GetComponent<Canvas>();
            modalCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            modalCanvas.sortingOrder = ModalSortingOrder;
            var scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = .5f;
            SetImage(overlay, new Color32(2, 8, 14, 224), true);

            shell = Rect("Academy", overlay);
            shell.anchorMin = shell.anchorMax = new Vector2(.5f, .5f);
            SparkProfessionalSkin.Apply(SetImage(shell, Background), "Shell");
            header = Top("Header", shell, 0, 0, -104, 0);
            SparkProfessionalSkin.Apply(SetImage(header, Surface), "Header");
            var panorama = SparkProfessionalSkin.Get("HeaderPanorama");
            if (panorama)
            {
                var panoramaImage = SetImage(Area("Cinematic Laboratory Panorama", header, 4, 4, 4, 4), Color.white);
                panoramaImage.sprite = panorama; panoramaImage.type = Image.Type.Simple; panoramaImage.preserveAspect = false;
            }
            campaignEyebrow = Text(Top("Campaign Eyebrow", shell, 32, 340, -28, -8), "ENGINEERING  /  CAMPAIGN", 12, Accent, TextAlignmentOptions.MidlineLeft);
            var heading = Top("Header Title", shell, 30, 350, -72, -27);
            headerTitleRect = heading;
            var titleText = Text(heading, manager.Campaign.DisplayName, 40, TextColor, TextAlignmentOptions.MidlineLeft, true);
            campaignEyebrow.characterSpacing=2.0f; campaignEyebrow.enableAutoSizing=true; campaignEyebrow.fontSizeMin=8; campaignEyebrow.fontSizeMax=12;
            titleText.enableAutoSizing = true; titleText.fontSizeMin=14; titleText.fontSizeMax=40;
            var count = Top("Campaign Progress", shell, 32, 334, -93, -70);
            total = Text(count, "", 15, Accent, TextAlignmentOptions.MidlineLeft);
            total.enableAutoSizing=true; total.fontSizeMin=9; total.fontSizeMax=15;
            close = CreateButton("Close", shell, "Back to game", 15);
            Place((RectTransform)close.transform, 1, 1, 1, 1, -142, -55, -16, -17);
            themeButton = CreateButton("Theme", shell, "BLUE", 13);
            Place((RectTransform)themeButton.transform, 1, 1, 1, 1, -238, -55, -149, -17);
            themeLabel = themeButton.GetComponentInChildren<TMP_Text>();
            themeButton.onClick.AddListener(() => { if (!finalPresentation) return; finalPresentation.CycleTheme(); ApplyTheme(); Refresh(); });
            motionButton = CreateButton("Motion", shell, "FX ON", 13);
            Place((RectTransform)motionButton.transform, 1, 1, 1, 1, -327, -55, -245, -17);
            motionLabel = motionButton.GetComponentInChildren<TMP_Text>();
            motionButton.onClick.AddListener(() => { if (!finalPresentation) return; finalPresentation.ToggleMotion(); ApplyTheme(); });
            var track = Top("Campaign Progress Track", shell, 32, 32, -104, -101);
            SetImage(track, Divider);
            completionFill = Rect("Campaign Progress Fill", track);
            Place(completionFill, 0, 0, 0, 1, 0, 0, 0, 0);
            SetImage(completionFill, Accent);

            body = Area("Campaign Content", shell, 24, 120, 24, 86);
            chapterColumn = Rect("Chapter Column", body);
            missionColumn = Rect("Level Column", body);
            briefColumn = Rect("Briefing Column", body);
            SparkProfessionalSkin.Apply(SetImage(chapterColumn, Surface), "Rail");
            SetImage(missionColumn, Color.clear);
            SparkProfessionalSkin.Apply(SetImage(briefColumn, Surface), "Brief");
            Text(Top("Chapters Heading", chapterColumn, 14, 12, -41, -7),
                "CHAPTERS  /  DIVISIONS", 16, Accent, TextAlignmentOptions.MidlineLeft, true);
            chapterScroll = CreateScroll("Chapters List", chapterColumn, 44);
            var chapterHeader = Top("Division Header Plate", missionColumn, 8, 8, -73, -7);
            SparkProfessionalSkin.Apply(SetImage(chapterHeader, Surface), "Header");
            chapterIcon = SetImage(Top("Chapter Icon", missionColumn, 15, 15, -42, -11), Color.white);
            Place(chapterIcon.rectTransform, 0, 1, 0, 1, 15, -42, 46, -11);
            chapterIcon.preserveAspect = true;
            chapterHeading = Text(Top("Chapter Title", missionColumn, 53, 20, -39, -8),
                "", 31, TextColor, TextAlignmentOptions.MidlineLeft, true);
            chapterDescription = Text(Top("Chapter Description", missionColumn, 20, 20, -71, -40),
                "", 16, Muted, TextAlignmentOptions.TopLeft, wrap: true);
            missionScroll = CreateScroll("Levels List", missionColumn, 81, grid: true);
            missionFitter = missionScroll.content.GetComponent<ContentSizeFitter>();
            missionGrid = missionScroll.content.gameObject.AddComponent<GridLayoutGroup>();
            missionGrid.padding = new RectOffset(7, 7, 8, 8);
            missionGrid.spacing = new Vector2(14, 14);
            missionGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            showcase = Rect("Mission Showcase", missionColumn);
            SparkProfessionalSkin.Apply(SetImage(showcase, Background), "Chamber");
            showcaseRail = SetImage(Top("Stage Accent", showcase, 16, 16, -5, -2), Accent);
            showcaseIndex = Text(Top("Showcase Index", showcase, 24, 180, -35, -10), "MISSION / ---", 14, Accent, TextAlignmentOptions.MidlineLeft, true);
            showcaseState = Text(Top("Showcase Access", showcase, 180, 24, -35, -10), "", 15, Locked, TextAlignmentOptions.MidlineRight, true);
            showcaseTitle = Text(Top("Showcase Title", showcase, 26, 26, -102, -35), "", 52, TextColor, TextAlignmentOptions.MidlineLeft, true);
            showcaseIndex.characterSpacing=2.0f; showcaseTitle.enableAutoSizing = true; showcaseTitle.fontSizeMin=20; showcaseTitle.fontSizeMax=52;
            galleryHeading = Rect("Gallery Header", missionColumn);
            Text(Area("Gallery Title", galleryHeading, 12, 0, 110, 0), "MISSION GALLERY", 14, TextColor, TextAlignmentOptions.MidlineLeft, true);
            galleryCount = Text(Area("Gallery Count", galleryHeading, 128, 0, 104, 0), "", 14, Muted, TextAlignmentOptions.MidlineRight);
            previousMission = CreateButton("Previous Mission",galleryHeading,"‹",20);
            nextMission = CreateButton("Next Mission",galleryHeading,"›",20);
            Place((RectTransform)previousMission.transform,1,0,1,1,-91,2,-51,-2);
            Place((RectTransform)nextMission.transform,1,0,1,1,-47,2,-7,-2);
            previousMission.onClick.AddListener(()=>StepMission(-1)); nextMission.onClick.AddListener(()=>StepMission(1));
            showcase.gameObject.SetActive(false); galleryHeading.gameObject.SetActive(false);

            briefNumber = Text(Top("Level Number", briefColumn, 17, 17, -39, -12),
                "", 15, Accent, TextAlignmentOptions.MidlineLeft, true);
            briefScroll = CreateScroll("Briefing", briefColumn, 46, 16);
            cover = Rect("Briefing Artwork", briefScroll.content);
            SparkProfessionalSkin.Apply(SetImage(cover, Background), "Chamber");
            var artworkRect = Area("Actual Level Artwork", cover, 5, 5, 5, 5);
            briefArtwork = SetImage(artworkRect, Color.white);
            briefArtwork.preserveAspect = true;
            
            cover.gameObject.AddComponent<LayoutElement>().preferredHeight = 252;
            livePreview = SparkLivePreview.Attach(briefArtwork);
            briefTitle = FlowText(briefScroll, "Level Title", 30, TextColor, true);
            description = FlowText(briefScroll, "Level Description", 17, TextColor);
            rule = FlowText(briefScroll, "Completion Rule", 16, Muted);
            var access = Rect("Access Requirements", briefScroll.content);
            accessPlate = SetImage(access, Surface);
            SparkProfessionalSkin.Apply(accessPlate, "Notice");
            var accessLayout = access.gameObject.AddComponent<VerticalLayoutGroup>();
            accessLayout.padding = new RectOffset(12,12,8,8);
            accessLayout.childControlWidth = true; accessLayout.childControlHeight = true;
            accessLayout.childForceExpandWidth = true; accessLayout.childForceExpandHeight = false;
            status = Text(Rect("Level Access", access), "", 16, Accent, TextAlignmentOptions.TopLeft, true, true);
            
            objectivesHeading = FlowText(briefScroll, "Objectives Heading", 16, Accent, true);
            objectives = FlowText(briefScroll, "Objective List", 16, TextColor);
            objectives.gameObject.SetActive(false); // Preserve the legacy text field; present structured rows instead.
            objectiveContainer = Rect("Objective Cards", briefScroll.content);
            var objectiveLayout = objectiveContainer.gameObject.AddComponent<VerticalLayoutGroup>();
            objectiveLayout.spacing = 6; objectiveLayout.childControlWidth = true; objectiveLayout.childControlHeight = true;
            objectiveLayout.childForceExpandWidth = true; objectiveLayout.childForceExpandHeight = false;

            resume = CreateButton("Continue Campaign", shell, "Continue campaign", 16);
            Place((RectTransform)resume.transform, 0, 0, .5f, 0, 16, 12, -8, 58);
            resume.onClick.AddListener(() => Launch(ResumeIndex()));
            launch = CreateButton("Play Selected", shell, "", 16);
            Place((RectTransform)launch.transform, .5f, 0, 1, 0, 8, 12, -16, 58);
            ColorBlock launchColors = launch.colors;
            launchColors.normalColor = new Color32(21, 66, 85, 255);
            launchColors.highlightedColor = new Color32(36, 107, 130, 255);
            launch.colors = launchColors;
            launchText = launch.GetComponentInChildren<TMP_Text>();
            launch.onClick.AddListener(() => Launch(selectedIndex));
            close.onClick.AddListener(Close);
            SparkProfessionalSkin.StyleButton(resume, false);
            SparkProfessionalSkin.StyleButton(launch, true);
            finalPresentation = SparkFinalPresentation.Attach(overlay, shell, briefColumn, font);
            // Keep the existing custom laboratory art under the new presentation.
            // The full-bleed artwork is the environment; no busy second header backdrop.
            compactTabs = Top("Compact Navigation", shell, 18, 18, -149, -111);
            string[] tabNames = { "CHAPTERS", "MISSIONS", "BRIEFING" };
            for (int i = 0; i < pageButtons.Length; i++)
            {
                int page = i;
                pageButtons[i] = CreateButton(tabNames[i], compactTabs, tabNames[i], 15);
                Place((RectTransform)pageButtons[i].transform, i / 3f, 0, (i + 1) / 3f, 1, 2, 0, -2, 0);
                pageButtons[i].onClick.AddListener(() => SetNarrowPage(page));
            }
            compactTabs.gameObject.SetActive(false);
            ApplyTheme();

            foreach (SparkChapterDefinition chapter in manager.Campaign.Chapters)
            {
                if (!chapter)
                    continue;
                var entry = new ChapterRow { Chapter = chapter };
                entry.Button = CreateRow(chapterScroll, "Chapter " + chapter.ChapterNumber,
                    chapter.ChapterNumber.ToString("00"), chapter.DisplayName, out entry.Status, 56);
                entry.Button.onClick.AddListener(() => { SelectChapter(chapter); if (narrow) SetNarrowPage(1); });
                var indexTransform = entry.Button.transform.Find("Number");
                if (!indexTransform) indexTransform = entry.Button.transform.Find("Chapter Number");
                if (indexTransform) entry.Number = indexTransform.GetComponentInChildren<TMP_Text>();
                var neonRail = Rect("Neon Division Rail", (RectTransform)entry.Button.transform);
                Place(neonRail,0,0,0,1,2,5,6,-5);
                entry.NeonRail = SetImage(neonRail,Color.clear);
                entry.NeonRail.raycastTarget = false;
                var symbol = finalPresentation.Icon(chapter.ChapterNumber);
                if (symbol)
                {
                    var iconRect = Rect("Discipline Icon", (RectTransform)entry.Button.transform);
                    Place(iconRect, 1, .5f, 1, .5f, -26, -10, -6, 10);
                    var icon = SetImage(iconRect, ThemeAccent); icon.sprite = symbol; icon.preserveAspect = true; disciplineIcons.Add(icon); entry.Icon=icon;
                    var titleLabel = entry.Button.transform.Find("Title") as RectTransform;
                    if (titleLabel) titleLabel.offsetMax = new Vector2(-30, titleLabel.offsetMax.y);
                    entry.Status.rectTransform.offsetMax = new Vector2(-30,entry.Status.rectTransform.offsetMax.y);
                }
                chapters.Add(entry);
            }

            overlay.gameObject.SetActive(false);
        }

        private void Fit()
        {
            Vector2 size = overlay.rect.size;
            if (size == lastSize || size.x <= 0 || size.y <= 0)
                return;

            lastSize = size;
            shell.sizeDelta = new Vector2(Mathf.Max(1, Mathf.Min(2560, size.x)), Mathf.Max(1, Mathf.Min(1440, size.y)));
            narrow = shell.sizeDelta.x < 1100 || shell.sizeDelta.y > shell.sizeDelta.x;
            ConfigureShowcase(!narrow && shell.sizeDelta.y >= 850 && SparkProfessionalSkin.UseShowcase);
            float right = shell.sizeDelta.x >= 660 ? 334 : shell.sizeDelta.x >= 520 ? 245 : 151;
            if(campaignEyebrow) campaignEyebrow.rectTransform.offsetMax = new Vector2(-right,campaignEyebrow.rectTransform.offsetMax.y);
            if (headerTitleRect) headerTitleRect.offsetMax = new Vector2(-right, headerTitleRect.offsetMax.y);
            total.rectTransform.offsetMax = new Vector2(-right,total.rectTransform.offsetMax.y);
            compactTabs.gameObject.SetActive(narrow);
            if (narrow)
            {
                Inset(body, 16, 158, 16, 76);
                Place(chapterColumn,0,0,1,1,0,0,0,0);
                Place(missionColumn,0,0,1,1,0,0,0,0);
                Place(briefColumn,0,0,1,1,0,0,0,0);
                SetNarrowPage(narrowPage);
                motionButton.gameObject.SetActive(shell.sizeDelta.x >= 660);
                themeButton.gameObject.SetActive(shell.sizeDelta.x >= 520);
                return;
            }
            Inset(body,24,120,24,86);
            chapterColumn.gameObject.SetActive(true); missionColumn.gameObject.SetActive(true); briefColumn.gameObject.SetActive(true);
            motionButton.gameObject.SetActive(true); themeButton.gameObject.SetActive(true);
            bool compact = shell.sizeDelta.x < 1100;
            float left = .179f;
            Place(chapterColumn, 0, 0, left, 1, 0, 0, -5, 0);
            if (compact)
            {
                Place(missionColumn, left, .40f, 1, 1, 5, 5, 0, 0);
                Place(briefColumn, left, 0, 1, .40f, 5, 0, 0, -5);
            }
            else
            {
                Place(missionColumn, left, 0, .744f, 1, 14, 0, -14, 0);
                Place(briefColumn, .744f, 0, 1, 1, 0, 0, 0, 0);
            }
        }

        private void ConfigureShowcase(bool wide)
        {
            if (!cover || !showcase) return;
            showcaseWide=wide;
            ConfigureGallery(wide);
            if (wide)
            {
                showcase.gameObject.SetActive(true); galleryHeading.gameObject.SetActive(true);
                Place(showcase,0,.31f,1,1,0,14,0,-86);
                if (cover.parent != showcase) cover.SetParent(showcase,false);
                Place(cover,0,0,1,1,5,6,-5,-104);
                Place(galleryHeading,0,.31f,1,.31f,0,-29,0,8);
                Place((RectTransform)missionScroll.transform,0,0,1,.31f,0,0,0,-34);
            }
            else
            {
                if (cover.parent != briefScroll.content) { cover.SetParent(briefScroll.content,false); cover.SetAsFirstSibling(); }
                showcase.gameObject.SetActive(false); galleryHeading.gameObject.SetActive(false);
                Inset((RectTransform)missionScroll.transform,6,81,6,6);
            }
        }

        private void ApplyTheme()
        {
            if (!finalPresentation) return;
            themeLabel.text = finalPresentation.ThemeName; themeLabel.color = ThemeAccent;
            motionLabel.text = finalPresentation.ReducedMotion ? "FX LOW" : "FX ON";
            total.color = ThemeAccent; briefNumber.color = ThemeAccent; if(campaignEyebrow) campaignEyebrow.color=ThemeAccent;
            if(showcaseRail) showcaseRail.color=ThemeAccent; if(showcaseIndex) showcaseIndex.color=ThemeAccent;
            objectivesHeading.color = ThemeAccent;
            var fill = completionFill.GetComponent<Image>(); if (fill) fill.color = ThemeAccent;
            if (chapterIcon) chapterIcon.color = ThemeAccent;
            foreach (var row in chapters) StyleChapterNeon(row);
            if (selectedChapter) ApplyChapterAccent(selectedChapter.ChapterNumber);
        }
        private void ApplyChapterAccent(int chapter)
        {
            var color = SparkProfessionalSkin.ChapterColor(chapter, ThemeAccent);
            if (chapterIcon) chapterIcon.color = color;
            if (chapterHeading) chapterHeading.color = SparkProfessionalSkin.NeonEnabled ? Color.Lerp(color, Color.white,.36f) : (Color)TextColor;
            if (showcaseRail) showcaseRail.color = color;
            if (showcaseIndex) showcaseIndex.color = color;
            if (objectivesHeading) objectivesHeading.color = color;
        }
        private void StyleChapterNeon(ChapterRow row)
        {
            var color = SparkProfessionalSkin.ChapterColor(row.Chapter.ChapterNumber, ThemeAccent);
            bool selected = row.Chapter == selectedChapter;
            if (row.Icon) row.Icon.color = color;
            if (row.Number) row.Number.color = SparkProfessionalSkin.NeonEnabled ? Color.Lerp(color,Color.white,.28f) : (Color)Accent;
            if (row.NeonRail) row.NeonRail.color = SparkProfessionalSkin.NeonEnabled ? SparkProfessionalSkin.Glow(color,selected ? 1f : .72f) : Color.clear;
            if (row.Button && row.Button.image && SparkProfessionalSkin.NeonEnabled)
                row.Button.image.color = SparkProfessionalSkin.SurfaceTint(color,selected ? .85f : .27f);
        }
        private void SetNarrowPage(int page)
        {
            narrowPage = Mathf.Clamp(page,0,2);
            if (!narrow) return;
            chapterColumn.gameObject.SetActive(narrowPage == 0);
            missionColumn.gameObject.SetActive(narrowPage == 1);
            briefColumn.gameObject.SetActive(narrowPage == 2);
            for(int i=0;i<pageButtons.Length;i++) if(pageButtons[i]) MarkSelected(pageButtons[i],i==narrowPage);
            if (EventSystem.current && pageButtons[narrowPage])
            {
                var focused = EventSystem.current.currentSelectedGameObject;
                if (!focused || !focused.activeInHierarchy) pageButtons[narrowPage].Select();
            }
        }
        private void ConfigureGallery(bool wide)
        {
            if (missionScroll.horizontal == wide) return;
            var content = missionScroll.content;
            var fitter = missionFitter;
            missionScroll.StopMovement();
            missionScroll.horizontal = wide; missionScroll.vertical = !wide;
            if (missionScroll.verticalScrollbar) missionScroll.verticalScrollbar.gameObject.SetActive(!wide);
            if (wide)
            {
                fitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;
                content.anchorMin = Vector2.zero; content.anchorMax = Vector2.up;
                content.pivot = new Vector2(0,1); content.sizeDelta = Vector2.zero;
                fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
                missionGrid.constraint = GridLayoutGroup.Constraint.FixedRowCount;
                missionGrid.constraintCount = 1;
            }
            else
            {
                fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
                content.anchorMin = new Vector2(0,1); content.anchorMax = Vector2.one;
                content.pivot = new Vector2(.5f,1); content.sizeDelta = Vector2.zero;
                fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                missionGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            }
            content.anchoredPosition = Vector2.zero;
        }

        private void FitMissionGrid()
        {
            float available = missionScroll.viewport.rect.width - missionGrid.padding.horizontal;
            if (available <= 0) return;
            if (showcaseWide)
            {
                float h = missionScroll.viewport.rect.height - missionGrid.padding.vertical;
                if (h <= 0) return;
                // A true artwork strip: card heights follow the viewport, not an overflowing grid.
                float width = Mathf.Clamp(available / 3.15f - missionGrid.spacing.x, 220, 340);
                var size = new Vector2(width, Mathf.Max(1,h));
                if (missionGrid.cellSize != size) missionGrid.cellSize = size;
                return;
            }
            int columns = Mathf.Clamp(Mathf.FloorToInt((available + missionGrid.spacing.x) / 267), 1, 3);
            float w = (available - missionGrid.spacing.x * (columns - 1)) / columns;
            var compactSize = new Vector2(w, SparkAAALevelCard.HeightForWidth(w));
            if (missionGrid.constraintCount != columns) missionGrid.constraintCount = columns;
            if (missionGrid.cellSize != compactSize) missionGrid.cellSize = compactSize;
        }

        private void SelectChapter(SparkChapterDefinition chapter, int preferred = -1)
        {
            if (!chapter)
                return;

            selectedChapter = chapter;
            selectedIndex = -1;
            int slot = 0;
            int recommended = -1;
            foreach (SparkLevelDefinition level in chapter.Levels)
            {
                int index = manager.GetLevelIndex(level);
                if (index < 0)
                    continue;
                if (slot == missions.Count)
                    missions.Add(new MissionRow { Card = SparkAAALevelCard.Create(missionScroll.content) });
                MissionRow entry = missions[slot++];
                entry.Index = index;
                entry.Artwork = SparkAAALevelCard.FindArtwork(level.LevelNumber, chapter.ChapterNumber);
                entry.Card.name = $"Level {level.LevelNumber}";
                entry.Card.SetMission(level.LevelNumber, level.DisplayName, chapter.DisplayName, level.TargetCount,
                    entry.Artwork, () => { SelectMission(index); if (narrow) SetNarrowPage(2); });
                entry.Card.gameObject.SetActive(true);
                if (recommended < 0 && manager.CanStartLevel(index) && !(progression && progression.IsCompleted(index)))
                    recommended = index;
            }

            for (int i = slot; i < missions.Count; i++)
            {
                missions[i].Index = -1;
                missions[i].Card.gameObject.SetActive(false);
            }

            if (preferred >= 0 && missions.Exists(entry => entry.Index == preferred))
                selectedIndex = preferred;
            else if (recommended >= 0)
                selectedIndex = recommended;
            else if (slot > 0)
                selectedIndex = missions[0].Index;

            chapterIcon.sprite = finalPresentation ? finalPresentation.Icon(chapter.ChapterNumber) : null;
            chapterIcon.enabled = chapterIcon.sprite != null;
            ApplyChapterAccent(chapter.ChapterNumber);
            chapterHeading.text = chapter.DisplayName;
            chapterDescription.text = chapter.Description;
            galleryCount.text = $"{slot:00} MISSIONS";
            if (showcaseWide) missionScroll.horizontalNormalizedPosition = 0; else missionScroll.verticalNormalizedPosition = 1;
            SelectMission(selectedIndex);
            if (selectedIndex >= 0)
            {
                Canvas.ForceUpdateCanvases(); FitMissionGrid(); Canvas.ForceUpdateCanvases();
                var row = missions.Find(entry => entry.Index == selectedIndex);
                if (row != null) Reveal(missionScroll, (RectTransform)row.Card.transform);
            }
        }

        private void StepMission(int delta)
        {
            int slot=missions.FindIndex(row=>row.Index==selectedIndex);
            int target=slot+delta;
            if(slot<0 || target<0 || target>=missions.Count || missions[target].Index<0)return;
            SelectMission(missions[target].Index);
            Canvas.ForceUpdateCanvases();
            Reveal(missionScroll,(RectTransform)missions[target].Card.transform);
        }

        private void SelectMission(int index)
        {
            bool changed = selectedIndex != index;
            selectedIndex = index;
            launchFailed = false;
            briefScroll.verticalNormalizedPosition = 1;
            Refresh();
            if (changed && finalPresentation) finalPresentation.SelectionChanged();
        }

        private int ResumeIndex() => progression ? progression.GetFirstIncompleteLevel() : manager.GetFirstCampaignIndex();

        private void Launch(int index)
        {
            if (launching || !IsOpen || !manager.CanStartLevel(index)) return;
            if (IsLoaded(index)) { Close(); return; }
            var definition = manager.GetLevel(index);
            if (!definition) return;
            launching = true;
            if (finalPresentation) finalPresentation.BeginLaunch(definition.LevelNumber, definition.DisplayName);
            launchRoutine = StartCoroutine(LaunchAfterFrame(index, definition));
        }

        private IEnumerator LaunchAfterFrame(int index, SparkLevelDefinition expected)
        {
            // Permit the real opening status to render; no invented progress percentage.
            yield return null;
            yield return new WaitForEndOfFrame();
            launchRoutine = null; // LevelStarted may synchronously close this panel.
            if (!IsOpen || !manager || manager.GetLevel(index) != expected || !manager.CanStartLevel(index))
            {
                launching = false; if (finalPresentation) finalPresentation.EndLaunch(); Refresh(); yield break;
            }
            bool opened = false;
            try { opened = manager.StartLevel(index); }
            catch (Exception exception) { Debug.LogException(exception, this); }
            launching = false;
            if (finalPresentation) finalPresentation.EndLaunch();
            if (opened) { Close(); yield break; }
            if (!IsOpen) yield break;
            launchFailed = true;
            status.text = "The workbench could not open this level. Try again.";
            status.color = new Color32(255,161,144,255);
            if (narrow) SetNarrowPage(2);
            Refresh();
        }
        private bool IsLoaded(int index) => manager.ActiveLevelIndex == index &&
            manager.LayoutLoader && manager.LayoutLoader.HasActiveLayout &&
            manager.LayoutLoader.ActiveLayout.LevelDefinition == manager.GetLevel(index);

        private void OnLevelStarted(SparkLevelDefinition level)
        {
            if (IsOpen)
                Close();
        }

        private void Refresh()
        {
            if (!IsOpen)
                return;

            int completed = 0;
            int count = 0;
            foreach (ChapterRow row in chapters)
            {
                int chapterCompleted = 0;
                int levelCount = 0;
                foreach (SparkLevelDefinition level in row.Chapter.Levels)
                {
                    int index = manager.GetLevelIndex(level);
                    if (index < 0)
                        continue;
                    levelCount++;
                    if (progression && progression.IsCompleted(index))
                        chapterCompleted++;
                }
                row.Status.text = $"{chapterCompleted}/{levelCount} COMPLETE";
                row.Status.color = chapterCompleted == levelCount && levelCount > 0 ? Complete : Muted;
                MarkSelected(row.Button, row.Chapter == selectedChapter);
                StyleChapterNeon(row);
                completed += chapterCompleted;
                count += levelCount;
            }

            total.text = $"{completed} of {count} levels complete";
            completionFill.anchorMax = new Vector2(count > 0 ? (float)completed / count : 0, 1);

            var live = SparkAILevelContext.Capture(manager);
            lastSatisfied = manager.SatisfiedTargetCount;
            lastTargetCount = manager.ActiveTargetCount;
            foreach (MissionRow row in missions)
            {
                if (row.Index < 0)
                    continue;

                string label = LevelStatus(row.Index, out Color32 color);
                bool completedBefore = progression && progression.IsCompleted(row.Index);
                float? fraction = completedBefore ? 1f : null;
                if (IsLoaded(row.Index) && live.HasEvaluation && lastTargetCount > 0)
                {
                    fraction = (float)lastSatisfied / lastTargetCount;
                    label = manager.IsCompleted ? "Complete" : manager.IsFailed ? "Retry" : "In play";
                }
                row.Card.SetState(label, color, fraction, row.Index == selectedIndex, ThemeAccent);
            }

            SparkLevelDefinition selected = selectedIndex >= 0 ? manager.GetLevel(selectedIndex) : null;
            int resumeIndex = ResumeIndex();
            resume.interactable = resumeIndex >= 0;
            resume.GetComponentInChildren<TMP_Text>().text = resumeIndex >= 0
                ? $"Continue campaign  /  {manager.GetLevel(resumeIndex).LevelNumber:00}"
                : "Campaign complete";
            launch.interactable = selected && manager.CanStartLevel(selectedIndex);
            SparkProfessionalSkin.Apply(launch.image, launch.interactable ? "Action" : "Notice");
            launchText.color = launch.interactable ? new Color32(15,20,23,255) : Locked;
            launchText.text = !selected ? "Select a level" : IsLoaded(selectedIndex) ? "Return to level" :
                launch.interactable ? $"Play Level {selected.LevelNumber:00}" : "Level locked";
            int currentSlot=missions.FindIndex(row=>row.Index==selectedIndex && row.Index>=0);
            if(previousMission) previousMission.interactable=currentSlot>0;
            if(nextMission) nextMission.interactable=currentSlot>=0 && currentSlot+1<missions.Count && missions[currentSlot+1].Index>=0;
            if (!selected)
            {
                briefArtwork.gameObject.SetActive(false);
                if (livePreview) livePreview.SelectLevel(-1, null);
                briefTitle.text = "Select a mission"; briefNumber.text = ""; description.text = "";
                showcaseTitle.text="Select a mission"; showcaseIndex.text="MISSION / ---"; showcaseState.text="";
                rule.text = ""; status.text = ""; objectivesHeading.text = ""; objectives.text = "";
                foreach (var row in objectiveRows) row.Root.gameObject.SetActive(false);
                return;
            }

            if(cachedBannerLevel!=selected.LevelNumber)
            { cachedBannerLevel=selected.LevelNumber; cachedBanner=SparkProfessionalSkin.Banner(selected.LevelNumber); }
            Sprite artwork = cachedBanner;
            if (!artwork) artwork = missions.Find(row => row.Index == selectedIndex)?.Artwork;
            showcaseTitle.text=selected.DisplayName; showcaseIndex.text=$"MISSION / {selected.LevelNumber:000}";
            showcaseState.text=LevelStatus(selectedIndex,out Color32 stageStatus).ToUpperInvariant(); showcaseState.color=stageStatus;
            briefArtwork.sprite = artwork;
            briefArtwork.gameObject.SetActive(artwork);
            if (livePreview) livePreview.SelectLevel(selected.LevelNumber, selected);
            briefNumber.text = $"MISSION BRIEFING  /  {selected.LevelNumber:000}";
            briefTitle.text = selected.DisplayName;
            description.text = selected.Description;
            rule.text = SparkAILevelContext.CompletionRule(selected);
            bool showLive = live.HasEvaluation && live.Definition == selected;
            var names = new StringBuilder();
            if (selected.Targets != null)
            {
                int number = 0;
                foreach (SparkLevelTarget target in selected.Targets)
                {
                    if (target == null)
                        continue;
                    string title = string.IsNullOrWhiteSpace(target.DisplayName) ? target.TargetId : target.DisplayName;
                    names.Append(++number).Append(". ").Append(title);
                    if (showLive)
                    {
                        bool? isSatisfied = live.Status(target);
                        if (isSatisfied.HasValue)
                            names.Append(isSatisfied.Value ? " [passed]" : " [pending]");
                    }
                    names.AppendLine();
                    if (!string.IsNullOrWhiteSpace(target.Description) && target.Description.Trim() != title)
                        names.AppendLine(target.Description.Trim());
                    names.AppendLine();
                }
            }
            objectivesHeading.text = names.Length > 0 ? $"OBJECTIVES / {selected.TargetCount:00}" : string.Empty;
            objectives.text = names.ToString().TrimEnd();
            int used = 0;
            if (selected.Targets != null)
            {
                foreach (SparkLevelTarget target in selected.Targets)
                {
                    if (target == null) continue;
                    ObjectiveRow row = GetObjectiveRow(used);
                    bool? result = showLive ? live.Status(target) : (bool?)null;
                    string title = string.IsNullOrWhiteSpace(target.DisplayName) ? target.TargetId : target.DisplayName;
                    row.Heading.text = $"{used + 1:00}  {title}" + (result.HasValue ? (result.Value ? "  [PASSED]" : "  [PENDING]") : "");
                    row.Heading.color = result == true ? Complete : TextColor;
                    string detail = target.Description?.Trim() ?? "";
                    row.Body.text = detail;
                    row.Body.gameObject.SetActive(detail.Length != 0 && detail != title);
                    row.Root.gameObject.SetActive(true);
                    used++;
                }
            }
            for (int i = used; i < objectiveRows.Count; i++) objectiveRows[i].Root.gameObject.SetActive(false);

            if (!launchFailed)
            {
                string selectedStatus = LevelStatus(selectedIndex, out Color32 accessColor);
                bool firstInChapter = selectedChapter.LevelCount > 0 && selectedChapter.Levels[0] == selected;
                status.text = showLive ? $"{lastSatisfied}/{lastTargetCount} objectives passed" :
                    launch.interactable ? selectedStatus : firstInChapter
                    ? "Complete the previous chapter to unlock this level."
                    : "Complete the previous level to unlock this level.";
                status.color = accessColor;
                if (accessPlate) accessPlate.color = launch.interactable ? Color.white : new Color(1f,.86f,.60f,1f);
            }
        }

        private ObjectiveRow GetObjectiveRow(int index)
        {
            if (index < objectiveRows.Count) return objectiveRows[index];
            var root = Rect("Objective Card " + (index + 1), objectiveContainer);
            SparkProfessionalSkin.Apply(SetImage(root, Surface), "Objective");
            var layout = root.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(13,10,9,9); layout.spacing = 4;
            layout.childControlWidth = true; layout.childControlHeight = true;
            layout.childForceExpandWidth = true; layout.childForceExpandHeight = false;
            var row = new ObjectiveRow { Root = root,
                Heading = Text(Rect("Objective Title",root), "",16,TextColor,TextAlignmentOptions.TopLeft,true,true),
                Body = Text(Rect("Objective Evidence",root), "",14,Muted,TextAlignmentOptions.TopLeft,false,true) };
            row.Heading.overflowMode = TextOverflowModes.Overflow; row.Body.overflowMode = TextOverflowModes.Overflow;
            objectiveRows.Add(row); return row;
        }

        private string LevelStatus(int index, out Color32 color)
        {
            if (progression && progression.IsCompleted(index))
            {
                color = Complete;
                return "Complete";
            }
            if (manager.CanStartLevel(index))
            {
                color = (Color32)ThemeAccent;
                return IsLoaded(index) ? "In play" : "Available";
            }
            color = Locked;
            return "Locked";
        }

        private Button CreateRow(ScrollRect scroll, string name, string number, string title,
            out TMP_Text rowStatus, float height)
        {
            Button button = CreateButton(name, scroll.content, null, 0);
            var rect = (RectTransform)button.transform;
            rect.gameObject.AddComponent<LayoutElement>().preferredHeight = height;
            var numberRect = Rect("Number", rect);
            Place(numberRect, 0, 0, 0, 1, 8, 4, 47, -4);
            SparkProfessionalSkin.Apply(SetImage(numberRect, Surface), "Index");
            var num = Text(Area("Chapter Index", numberRect, 2, 0, 2, 0), number, 17, Accent, TextAlignmentOptions.Center, true);
            int labelOffset = number.Length >= 3 ? 48 : 40;
            var label = Text(Area("Title", rect, labelOffset, 2, 6, height * .40f),
                title, 17, TextColor, TextAlignmentOptions.MidlineLeft, true);
            label.enableAutoSizing = true;
            label.fontSizeMin = 13;
            label.fontSizeMax = 17;
            rowStatus = Text(Area("Status", rect, labelOffset, height * .55f, 6, 3), "", 12, Muted, TextAlignmentOptions.MidlineLeft);
            return button;
        }

        private static void MarkSelected(Button button, bool value)
        {
            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            if (button.image) SparkProfessionalSkin.Apply(button.image, value ? "ActiveTab" : "Tab");
            button.colors = colors;
        }

        private Button CreateButton(string name, RectTransform parent, string value, int size)
        {
            var rect = Rect(name, parent);
            var button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = SetImage(rect, Color.white, true);
            button.colors = new ColorBlock
            {
                normalColor = Surface,
                highlightedColor = new Color32(41, 88, 109, 255),
                selectedColor = new Color32(48, 102, 124, 255),
                pressedColor = new Color32(36, 114, 140, 255),
                disabledColor = new Color32(20, 35, 46, 255),
                colorMultiplier = 1,
                fadeDuration = .12f
            };
            if (value != null)
                Text(Area("Label", rect, 6, 1, 6, 1), value, size, TextColor, TextAlignmentOptions.Center, true);
            SparkProfessionalSkin.StyleButton(button, false);
            return button;
        }

        private static ScrollRect CreateScroll(string name, RectTransform parent, float top, float inset = 6, bool grid = false)
        {
            var frame = Area(name, parent, inset, top, inset, 6);
            SetImage(frame, new Color32(6, 14, 22, 100), true);
            var scroll = frame.gameObject.AddComponent<ScrollRect>();
            var viewport = Area("Viewport", frame, 3, 3, 10, 3);
            viewport.gameObject.AddComponent<RectMask2D>();
            var content = Rect("Content", viewport);
            Place(content, 0, 1, 1, 1, 0, 0, 0, 0);
            content.pivot = new Vector2(.5f, 1);
            if (!grid)
            {
                var layout = content.gameObject.AddComponent<VerticalLayoutGroup>();
                layout.spacing = 5;
                layout.padding = new RectOffset(4, 4, 4, 4);
                layout.childControlWidth = true;
                layout.childControlHeight = true;
                layout.childForceExpandWidth = true;
                layout.childForceExpandHeight = false;
            }
            content.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            scroll.viewport = viewport;
            scroll.content = content;
            scroll.horizontal = false;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 24;
            var bar = Rect("Scrollbar", frame);
            Place(bar, 1, 0, 1, 1, -8, 3, -3, -3);
            SetImage(bar, Divider, true);
            var handle = Area("Thumb", bar, 0, 0, 0, 0);
            var scrollbar = bar.gameObject.AddComponent<Scrollbar>();
            scrollbar.targetGraphic = SetImage(handle, Accent, true);
            scrollbar.handleRect = handle;
            scrollbar.direction = Scrollbar.Direction.BottomToTop;
            scroll.verticalScrollbar = scrollbar;
            scroll.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHide;
            return scroll;
        }

        private TMP_Text FlowText(ScrollRect scroll, string name, int size, Color32 color, bool bold = false)
        {
            var text = Text(Rect(name, scroll.content), "", size, color, TextAlignmentOptions.TopLeft, bold, wrap: true);
            text.overflowMode = TextOverflowModes.Overflow;
            return text;
        }

        private TMP_Text Text(RectTransform rect, string value, int size, Color32 color,
            TextAlignmentOptions align, bool bold = false, bool wrap = false)
        {
            var text = rect.gameObject.AddComponent<TextMeshProUGUI>();
            if (font)
                text.font = font;
            text.fontSize = size;
            text.color = color;
            text.alignment = align;
            text.fontStyle = bold ? FontStyles.Bold : FontStyles.Normal;
            text.textWrappingMode = wrap ? TextWrappingModes.Normal : TextWrappingModes.NoWrap;
            text.overflowMode = TextOverflowModes.Ellipsis;
            text.raycastTarget = false;
            text.text = value;
            SparkCinematicSkin.StyleText(text, bold);
            SparkProfessionalSkin.ApplyTypography(text, bold);
            return text;
        }

        private static void Reveal(ScrollRect scroll, RectTransform target)
        {
            if (!target || !target.IsChildOf(scroll.content))
                return;

            Bounds bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(scroll.viewport, target);
            Rect view = scroll.viewport.rect;
            Vector2 offset = scroll.content.anchoredPosition;
            if (scroll.horizontal)
            {
                if (bounds.min.x < view.xMin) offset.x += view.xMin - bounds.min.x;
                else if (bounds.max.x > view.xMax) offset.x -= bounds.max.x - view.xMax;
            }
            if (scroll.vertical)
            {
                if (bounds.min.y < view.yMin) offset.y += view.yMin - bounds.min.y;
                else if (bounds.max.y > view.yMax) offset.y -= bounds.max.y - view.yMax;
            }
            scroll.content.anchoredPosition = offset;
        }

        private static RectTransform Rect(string name, RectTransform parent)
        {
            var rect = (RectTransform)new GameObject(name, typeof(RectTransform)).transform;
            rect.SetParent(parent, false);
            rect.gameObject.layer = parent.gameObject.layer;
            return rect;
        }

        private static RectTransform Area(string name, RectTransform parent,
            float left, float top, float right, float bottom)
        {
            var rect = Rect(name, parent);
            Inset(rect, left, top, right, bottom);
            return rect;
        }

        private static RectTransform Top(string name, RectTransform parent,
            float left, float right, float bottom, float top)
        {
            var rect = Rect(name, parent);
            Place(rect, 0, 1, 1, 1, left, bottom, -right, top);
            return rect;
        }

        private static void Inset(RectTransform rect, float left, float top, float right, float bottom) =>
            Place(rect, 0, 0, 1, 1, left, bottom, -right, -top);

        private static void Place(RectTransform rect, float minX, float minY, float maxX, float maxY,
            float left, float bottom, float right, float top)
        {
            rect.anchorMin = new Vector2(minX, minY);
            rect.anchorMax = new Vector2(maxX, maxY);
            rect.offsetMin = new Vector2(left, bottom);
            rect.offsetMax = new Vector2(right, top);
        }

        private static Image SetImage(RectTransform rect, Color color, bool raycast = false)
        {
            var image = rect.gameObject.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = raycast;
            return image;
        }
    }
}

