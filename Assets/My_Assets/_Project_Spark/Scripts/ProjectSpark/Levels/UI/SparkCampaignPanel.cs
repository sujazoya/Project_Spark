using System;
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

        private static readonly Color32 Background = new(8, 17, 27, 255);
        private static readonly Color32 Surface = new(15, 31, 45, 255);
        private static readonly Color32 Divider = new(36, 65, 82, 255);
        private static readonly Color32 Accent = new(107, 225, 250, 255);
        private static readonly Color32 TextColor = new(234, 245, 252, 255);
        private static readonly Color32 Muted = new(157, 186, 204, 255);
        private static readonly Color32 Complete = new(119, 233, 175, 255);
        private static readonly Color32 Locked = new(246, 203, 140, 255);

        private sealed class ChapterRow
        {
            public SparkChapterDefinition Chapter;
            public Button Button;
            public TMP_Text Status;
        }

        private sealed class MissionRow
        {
            public int Index;
            public Sprite Artwork;
            public SparkAAALevelCard Card;
        }

        private readonly List<ChapterRow> chapters = new();
        private readonly List<MissionRow> missions = new();
        private LevelGamePlayManager manager;
        private SparkLevelProgression progression;
        private TMP_FontAsset font;
        private SparkUIManager uiManager;
        private SparkUIPanel uiRegistration;
        private RectTransform overlay, shell, chapterColumn, missionColumn, briefColumn, body;
        private ScrollRect chapterScroll, missionScroll, briefScroll;
        private GridLayoutGroup missionGrid;
        private Image briefArtwork;
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
            Fit();
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

            overlay.gameObject.SetActive(false);
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
            SetImage(shell, Background);
            SetImage(Top("Header", shell, 0, 0, -72, 0), Surface);
            var heading = Top("Header Title", shell, 24, 156, -48, -8);
            Text(heading, manager.Campaign.DisplayName, 27, TextColor, TextAlignmentOptions.MidlineLeft, true);
            var count = Top("Campaign Progress", shell, 24, 156, -67, -43);
            total = Text(count, "", 15, Accent, TextAlignmentOptions.MidlineLeft);
            close = CreateButton("Close", shell, "Back to game", 15);
            Place((RectTransform)close.transform, 1, 1, 1, 1, -142, -55, -16, -17);
            var track = Top("Campaign Progress Track", shell, 0, 0, -74, -71);
            SetImage(track, Divider);
            completionFill = Rect("Campaign Progress Fill", track);
            Place(completionFill, 0, 0, 0, 1, 0, 0, 0, 0);
            SetImage(completionFill, Accent);

            body = Area("Campaign Content", shell, 16, 86, 16, 66);
            chapterColumn = Rect("Chapter Column", body);
            missionColumn = Rect("Level Column", body);
            briefColumn = Rect("Briefing Column", body);
            SetImage(chapterColumn, Surface);
            SetImage(briefColumn, Surface);
            Text(Top("Chapters Heading", chapterColumn, 12, 8, -38, -4),
                "CHAPTERS", 16, Accent, TextAlignmentOptions.MidlineLeft, true);
            chapterScroll = CreateScroll("Chapters List", chapterColumn, 38);
            chapterHeading = Text(Top("Chapter Title", missionColumn, 8, 8, -34, 0),
                "", 23, TextColor, TextAlignmentOptions.MidlineLeft, true);
            chapterDescription = Text(Top("Chapter Description", missionColumn, 8, 8, -77, -35),
                "", 15, Muted, TextAlignmentOptions.TopLeft, wrap: true);
            missionScroll = CreateScroll("Levels List", missionColumn, 79, grid: true);
            missionGrid = missionScroll.content.gameObject.AddComponent<GridLayoutGroup>();
            missionGrid.padding = new RectOffset(7, 7, 8, 8);
            missionGrid.spacing = new Vector2(12, 12);
            missionGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;

            briefNumber = Text(Top("Level Number", briefColumn, 12, 12, -32, -8),
                "", 15, Accent, TextAlignmentOptions.MidlineLeft, true);
            briefScroll = CreateScroll("Briefing", briefColumn, 36, 12);
            var cover = Rect("Briefing Artwork", briefScroll.content);
            briefArtwork = SetImage(cover, Color.white);
            cover.gameObject.AddComponent<LayoutElement>().preferredHeight = 156;
            briefTitle = FlowText(briefScroll, "Level Title", 28, TextColor, true);
            description = FlowText(briefScroll, "Level Description", 17, TextColor);
            rule = FlowText(briefScroll, "Completion Rule", 16, Muted);
            status = FlowText(briefScroll, "Level Access", 16, Accent, true);
            objectivesHeading = FlowText(briefScroll, "Objectives Heading", 14, Accent, true);
            objectives = FlowText(briefScroll, "Objective List", 16, TextColor);

            resume = CreateButton("Continue Campaign", shell, "Continue campaign", 16);
            Place((RectTransform)resume.transform, 0, 0, .5f, 0, 16, 12, -8, 52);
            resume.onClick.AddListener(() => Launch(ResumeIndex()));
            launch = CreateButton("Play Selected", shell, "", 16);
            Place((RectTransform)launch.transform, .5f, 0, 1, 0, 8, 12, -16, 52);
            ColorBlock launchColors = launch.colors;
            launchColors.normalColor = new Color32(21, 66, 85, 255);
            launchColors.highlightedColor = new Color32(36, 107, 130, 255);
            launch.colors = launchColors;
            launchText = launch.GetComponentInChildren<TMP_Text>();
            launch.onClick.AddListener(() => Launch(selectedIndex));
            close.onClick.AddListener(Close);

            foreach (SparkChapterDefinition chapter in manager.Campaign.Chapters)
            {
                if (!chapter)
                    continue;
                var entry = new ChapterRow { Chapter = chapter };
                entry.Button = CreateRow(chapterScroll, "Chapter " + chapter.ChapterNumber,
                    chapter.ChapterNumber.ToString("00"), chapter.DisplayName, out entry.Status, 49);
                entry.Button.onClick.AddListener(() => SelectChapter(chapter));
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
            shell.sizeDelta = new Vector2(Mathf.Min(1700, size.x - 32), Mathf.Min(900, size.y - 32));
            bool compact = shell.sizeDelta.x < 1050;
            float left = compact ? .30f : .17f;
            Place(chapterColumn, 0, 0, left, 1, 0, 0, -5, 0);
            if (compact)
            {
                Place(missionColumn, left, .40f, 1, 1, 5, 5, 0, 0);
                Place(briefColumn, left, 0, 1, .40f, 5, 0, 0, -5);
            }
            else
            {
                Place(missionColumn, left, 0, .749f, 1, 5, 0, -5, 0);
                Place(briefColumn, .749f, 0, 1, 1, 5, 0, 0, 0);
            }
        }

        private void FitMissionGrid()
        {
            float available = missionScroll.viewport.rect.width - missionGrid.padding.horizontal;
            if (available <= 0)
                return;

            int columns = Mathf.Clamp(Mathf.FloorToInt((available + missionGrid.spacing.x) / 284), 1, 3);
            float width = (available - missionGrid.spacing.x * (columns - 1)) / columns;
            var size = new Vector2(width, SparkAAALevelCard.HeightForWidth(width));
            if (missionGrid.constraintCount == columns && missionGrid.cellSize == size)
                return;

            missionGrid.constraintCount = columns;
            missionGrid.cellSize = size;
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
                    entry.Artwork, () => SelectMission(index));
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

            chapterHeading.text = chapter.DisplayName;
            chapterDescription.text = chapter.Description;
            missionScroll.verticalNormalizedPosition = 1;
            SelectMission(selectedIndex);
        }

        private void SelectMission(int index)
        {
            selectedIndex = index;
            launchFailed = false;
            briefScroll.verticalNormalizedPosition = 1;
            Refresh();
        }

        private int ResumeIndex() => progression ? progression.GetFirstIncompleteLevel() : manager.GetFirstCampaignIndex();

        private void Launch(int index)
        {
            if (!manager.CanStartLevel(index))
                return;
            if (IsLoaded(index) || manager.StartLevel(index))
            {
                Close();
                return;
            }

            launchFailed = true;
            status.text = "The workbench could not open this level. Try again.";
            status.color = new Color32(255, 161, 144, 255);
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
                row.Card.SetState(label, color, fraction, row.Index == selectedIndex, Accent);
            }

            SparkLevelDefinition selected = manager.GetLevel(selectedIndex);
            int resumeIndex = ResumeIndex();
            resume.interactable = resumeIndex >= 0;
            resume.GetComponentInChildren<TMP_Text>().text = resumeIndex >= 0
                ? $"Continue campaign  /  {manager.GetLevel(resumeIndex).LevelNumber:00}"
                : "Campaign complete";
            launch.interactable = selected && manager.CanStartLevel(selectedIndex);
            launchText.text = !selected ? "Select a level" : IsLoaded(selectedIndex) ? "Return to level" :
                launch.interactable ? $"Play Level {selected.LevelNumber:00}" : "Level locked";
            if (!selected)
                return;

            Sprite artwork = missions.Find(row => row.Index == selectedIndex)?.Artwork;
            briefArtwork.sprite = artwork;
            briefArtwork.gameObject.SetActive(artwork);
            briefNumber.text = $"LEVEL {selected.LevelNumber:00}";
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
            objectivesHeading.text = names.Length > 0 ? "OBJECTIVES" : string.Empty;
            objectives.text = names.ToString().TrimEnd();

            if (!launchFailed)
            {
                string selectedStatus = LevelStatus(selectedIndex, out Color32 accessColor);
                bool firstInChapter = selectedChapter.LevelCount > 0 && selectedChapter.Levels[0] == selected;
                status.text = showLive ? $"{lastSatisfied}/{lastTargetCount} objectives passed" :
                    launch.interactable ? selectedStatus : firstInChapter
                    ? "Complete the previous chapter to unlock this level."
                    : "Complete the previous level to unlock this level.";
                status.color = accessColor;
            }
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
                color = Accent;
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
            Text(numberRect, number, 16, Accent, TextAlignmentOptions.MidlineLeft, true);
            int labelOffset = number.Length >= 3 ? 48 : 40;
            var label = Text(Area("Title", rect, labelOffset, 2, 6, height * .40f),
                title, 16, TextColor, TextAlignmentOptions.MidlineLeft, true);
            label.enableAutoSizing = true;
            label.fontSizeMin = 13;
            label.fontSizeMax = 16;
            rowStatus = Text(Area("Status", rect, labelOffset, height * .55f, 6, 3), "", 12, Muted, TextAlignmentOptions.MidlineLeft);
            return button;
        }

        private static void MarkSelected(Button button, bool value)
        {
            ColorBlock colors = button.colors;
            colors.normalColor = value ? new Color32(30, 77, 98, 255) : Surface;
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
            return button;
        }

        private static ScrollRect CreateScroll(string name, RectTransform parent, float top, float inset = 6, bool grid = false)
        {
            var frame = Area(name, parent, inset, top, inset, 6);
            SetImage(frame, new Color32(9, 21, 32, 255), true);
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
            return Text(Rect(name, scroll.content), "", size, color, TextAlignmentOptions.TopLeft, bold, wrap: true);
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
            return text;
        }

        private static void Reveal(ScrollRect scroll, RectTransform target)
        {
            if (!target || !target.IsChildOf(scroll.content))
                return;

            Bounds bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(scroll.viewport, target);
            Rect view = scroll.viewport.rect;
            Vector2 offset = scroll.content.anchoredPosition;
            if (bounds.min.y < view.yMin)
                offset.y += view.yMin - bounds.min.y;
            else if (bounds.max.y > view.yMax)
                offset.y -= bounds.max.y - view.yMax;
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

