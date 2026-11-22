using System;
using ProjectSpark.AI.Tutorial;
using ProjectSpark.Gameplay;
using ProjectSpark.UI.VisualTeaching;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.UI
{
    /// <summary>Displays the loaded level and opens its existing objective whiteboard.</summary>
    [DisallowMultipleComponent]
    public sealed class SparkLevelDetailsButton : MonoBehaviour
    {
        [SerializeField] private LevelGamePlayManager levelManager;
        [SerializeField] private SparkDrawCurrentLevel drawing;
        [SerializeField] private SparkVisualTeachingManager teaching;
        [SerializeField] private Button previousLevel;
        [SerializeField] private Button nextLevel;
        [SerializeField] private TMP_Text labelStyle;

        private static readonly Color Ink = new Color32(11, 25, 38, 255);
        private static readonly Color Stroke = new Color32(47, 86, 107, 255);
        private static readonly Color Sky = new Color32(92, 225, 255, 255);
        private static readonly Color White = new Color32(236, 248, 253, 255);
        private static readonly Color Secondary = new Color32(167, 197, 211, 255);
        private static readonly Color Passed = new Color32(118, 236, 172, 255);
        private static readonly Color Failed = new Color32(255, 153, 134, 255);

        private RectTransform bar;
        private Button details;
        private TMP_Text identifier;
        private TMP_Text title;
        private TMP_Text progress;
        private TMP_Text action;
        private RectTransform fill;
        private Image progressHighlight;
        private SparkLevelDefinition displayedLevel;
        private bool detailsRequested;
        private float nextRefresh;

        private void Awake()
        {
            if (!levelManager || !drawing || !teaching || !previousLevel || !nextLevel || !labelStyle)
                return;

            bar = Rect("Level Navigation", transform);
            bar.anchorMin = bar.anchorMax = new Vector2(.5f, 1);
            bar.pivot = new Vector2(.5f, 1);
            bar.anchoredPosition = new Vector2(0, -65);
            FitWidth();

            StyleNavigation(previousLevel, 0, 0, 48, "<");
            StyleNavigation(nextLevel, 1, -48, 0, ">");

            var card = Rect("Level Details", bar);
            Inset(card, 54, 0, 54, 0);
            var surface = card.gameObject.AddComponent<Image>();
            details = card.gameObject.AddComponent<Button>();
            StyleButton(details, surface);
            details.onClick.AddListener(ToggleDetails);

            Block("Level Surface", card, new Color32(4, 14, 23, 255), 1, 1, 1, 1);
            Block("Level Accent", card, Sky, 1, 1, 0, 1, 3);
            identifier = Label(card, "Level ID", "LEVEL", 13, Sky, TextAlignmentOptions.TopLeft, 14, 6, 144, 28);
            title = Label(card, "Level Title", "Workbench", 20, White, TextAlignmentOptions.BottomLeft, 14, 22, 88, 10);
            progress = Label(card, "Objective Count", "", 12, Secondary, TextAlignmentOptions.TopRight, 110, 6, 12, 28);
            action = Label(card, "Details Action", "Details  >", 14, Sky, TextAlignmentOptions.BottomRight, 98, 23, 12, 10);
            var track = Block("Objective Progress Track", card, Stroke, 14, 50, 12, 6);
            progressHighlight = Block("Objective Progress Value", track.rectTransform, Sky, 0, 0, 0, 0);
            fill = progressHighlight.rectTransform;

            var nav = details.navigation;
            nav.mode = UnityEngine.UI.Navigation.Mode.Explicit;
            nav.selectOnLeft = previousLevel;
            nav.selectOnRight = nextLevel;
            details.navigation = nav;
            LinkToDetails(previousLevel, false);
            LinkToDetails(nextLevel, true);
            if (levelManager.Campaign)
            {
                var campaign = gameObject.AddComponent<SparkCampaignPanel>();
                campaign.Initialize(levelManager, (RectTransform)transform, bar, labelStyle.font, () =>
                {
                    if (IsDetailsOpen())
                        ToggleDetails();
                });
            }
            Refresh();
        }

        private void Update()
        {
            if (!details || Time.unscaledTime < nextRefresh) return;
            nextRefresh = Time.unscaledTime + .2f;
            Refresh();
        }

        private void OnRectTransformDimensionsChange()
        {
            if (bar) FitWidth();
        }

        private void FitWidth()
        {
            var hud = (RectTransform)transform;
            bar.sizeDelta = new Vector2(Mathf.Clamp(hud.rect.width - 24, 260, 440), 60);
        }

        private void ToggleDetails()
        {
            if (IsDetailsOpen())
            {
                detailsRequested = false;
                teaching.ClosePanel();
            }
            else
            {
                detailsRequested = drawing.ShowCurrentOnWhiteboard(objectives: true);
            }
            Refresh();
        }

        private bool IsDetailsOpen() => detailsRequested && teaching.PanelOpen &&
            teaching.LevelDrawing == drawing && teaching.View == SparkTeachingView.WhiteboardOnly;

        private void Refresh()
        {
            var context = SparkAILevelContext.Capture(levelManager);
            var level = context.Definition;
            if (level != displayedLevel)
            {
                displayedLevel = level;
                string composed = SparkAILevelContext.Title(level);
                int separator = composed.IndexOf(" · ", StringComparison.Ordinal);
                identifier.text = separator >= 0 ? composed.Substring(0, separator).ToUpperInvariant() : "LEVEL";
                title.text = level ? level.DisplayName : "Workbench";
            }

            bool verified = context.HasEvaluation;
            int total = verified ? levelManager.ActiveTargetCount : 0;
            int passed = verified ? levelManager.SatisfiedTargetCount : 0;
            details.interactable = level;
            int index = levelManager.ActiveLevelIndex;
            previousLevel.interactable = level && levelManager.CanStartLevel(levelManager.GetAdjacentLevelIndex(index, -1));
            nextLevel.interactable = level && levelManager.CanStartLevel(levelManager.GetAdjacentLevelIndex(index, 1));
            progress.text = !level ? "NO LEVEL LOADED" : !verified ? "AWAITING EVALUATION" :
                levelManager.IsCompleted ? $"COMPLETE  {passed}/{total}" :
                levelManager.IsFailed ? $"FAILED  {passed}/{total}" : $"{passed}/{total} OBJECTIVES";
            progress.color = verified && levelManager.IsCompleted ? Passed :
                verified && levelManager.IsFailed ? Failed : Secondary;
            progressHighlight.color = verified && levelManager.IsCompleted ? Passed : Sky;
            fill.anchorMax = new Vector2(total > 0 ? Mathf.Clamp01((float)passed / total) : 0f, 1);
            action.text = IsDetailsOpen() ? "Close  <" : "Details  >";
        }

        private void StyleNavigation(Button button, float side, float left, float right, string glyph)
        {
            var rect = (RectTransform)button.transform;
            rect.SetParent(bar, false);
            rect.localScale = Vector3.one;
            rect.anchorMin = new Vector2(side, 0);
            rect.anchorMax = new Vector2(side, 1);
            rect.offsetMin = new Vector2(left, 6);
            rect.offsetMax = new Vector2(right, -6);
            foreach (TMP_Text legacyLabel in button.GetComponentsInChildren<TMP_Text>(true))
                legacyLabel.gameObject.SetActive(false);
            var image = (Image)button.targetGraphic;
            image.sprite = null;
            image.color = Color.white;
            StyleButton(button, image);
            Block("Navigation Surface", rect, Ink, 1, 1, 1, 1);
            Label(rect, "Navigation Arrow", glyph, 22, Sky, TextAlignmentOptions.Center, 0, 0, 0, 0);
        }

        private void LinkToDetails(Button button, bool left)
        {
            var nav = button.navigation;
            nav.mode = UnityEngine.UI.Navigation.Mode.Explicit;
            if (left) nav.selectOnLeft = details;
            else nav.selectOnRight = details;
            button.navigation = nav;
        }

        private static void StyleButton(Button button, Image target)
        {
            button.targetGraphic = target;
            button.transition = Selectable.Transition.ColorTint;
            button.colors = new ColorBlock
            {
                normalColor = Stroke,
                highlightedColor = Sky,
                selectedColor = Sky,
                pressedColor = new Color32(39, 140, 171, 255),
                disabledColor = new Color32(28, 46, 59, 255),
                colorMultiplier = 1,
                fadeDuration = .1f
            };
        }

        private TMP_Text Label(RectTransform parent, string name, string value, int size,
            Color color, TextAlignmentOptions alignment, float left, float top, float right, float bottom)
        {
            var rect = Rect(name, parent);
            Inset(rect, left, top, right, bottom);
            var text = rect.gameObject.AddComponent<TextMeshProUGUI>();
            text.font = labelStyle.font;
            text.fontStyle = FontStyles.Bold;
            text.color = color;
            text.alignment = alignment;
            text.fontSize = size;
            text.textWrappingMode = TextWrappingModes.NoWrap;
            text.overflowMode = TextOverflowModes.Ellipsis;
            text.raycastTarget = false;
            text.text = value;
            return text;
        }

        private static RectTransform Rect(string name, Transform parent)
        {
            var rect = (RectTransform)new GameObject(name, typeof(RectTransform)).transform;
            rect.SetParent(parent, false);
            return rect;
        }

        private static void Inset(RectTransform rect, float left, float top, float right, float bottom)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(left, bottom);
            rect.offsetMax = new Vector2(-right, -top);
        }

        private static Image Block(string name, RectTransform parent, Color color,
            float left, float top, float right, float bottom, float width = 0)
        {
            var rect = Rect(name, parent);
            Inset(rect, left, top, right, bottom);
            if (width > 0)
            {
                rect.anchorMax = new Vector2(0, 1);
                rect.offsetMax = new Vector2(left + width, -top);
            }
            var block = rect.gameObject.AddComponent<Image>();
            block.color = color;
            block.raycastTarget = false;
            return block;
        }
    }
}
