using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

namespace ProjectSpark.UI
{
    /// <summary>
    /// Defines the visual states supported by ButtonDesigner.
    /// </summary>
    public enum ButtonVisualState
    {
        /// <summary>
        /// Default button state.
        /// </summary>
        Normal,

        /// <summary>
        /// Pointer is hovering over the button.
        /// </summary>
        Hover,

        /// <summary>
        /// Pointer or input is pressing the button.
        /// </summary>
        Pressed,

        /// <summary>
        /// Button currently has UI selection focus.
        /// </summary>
        Selected,

        /// <summary>
        /// Button is not interactable.
        /// </summary>
        Disabled
    }

    /// <summary>
    /// Defines how button corner elements are activated.
    /// </summary>
    public enum CornerMode
    {
        /// <summary>
        /// Corners are manually controlled by the designer.
        /// </summary>
        CustomActive,

        /// <summary>
        /// Corners are randomly selected.
        /// </summary>
        RandomActive
    }

    /// <summary>
    /// Defines the supported decorative button image positions.
    /// </summary>
    public enum ButtonImageSlot
    {
        /// <summary>
        /// Top image.
        /// </summary>
        Top,

        /// <summary>
        /// Bottom image.
        /// </summary>
        Bottom,

        /// <summary>
        /// Left image.
        /// </summary>
        Left,

        /// <summary>
        /// Right image.
        /// </summary>
        Right,

        /// <summary>
        /// Left top corner.
        /// </summary>
        LeftTop,

        /// <summary>
        /// Right top corner.
        /// </summary>
        RightTop,

        /// <summary>
        /// Left bottom corner.
        /// </summary>
        LeftBottom,

        /// <summary>
        /// Right bottom corner.
        /// </summary>
        RightBottom
    }
   

    /// <summary>
    /// Provides theme-aware visual design and state handling
    /// for a Unity UI Button.
    /// </summary>
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Button))]
    public sealed class ButtonDesigner :
        MonoBehaviour,
        IThemeObject,
        IPointerEnterHandler,
        IPointerExitHandler,
        IPointerDownHandler,
        IPointerUpHandler,
        ISelectHandler,
        IDeselectHandler
    {
        #region Inspector

        [Header("Theme")]

        [SerializeField]
        private ThemeColor themeColor = ThemeColor.White;

        [Header("Preview")]

        [SerializeField]
        private ButtonVisualState previewState =
            ButtonVisualState.Normal;

        [Header("Corner")]

        [SerializeField]
        private CornerMode cornerMode =
            CornerMode.CustomActive;

        [Range(1, 4)]
        [SerializeField]
        private int cornerActiveCount = 1;

        [SerializeField]
        private bool leftTopActive;

        [SerializeField]
        private bool rightTopActive;

        [SerializeField]
        private bool leftBottomActive;

        [SerializeField]
        private bool rightBottomActive;

        [Header("Images")]

        [SerializeField]
        private Image topImage;

        [SerializeField]
        private Image bottomImage;

        [SerializeField]
        private Image leftImage;

        [SerializeField]
        private Image rightImage;

        [SerializeField]
        private Image leftTopImage;

        [SerializeField]
        private Image rightTopImage;

        [SerializeField]
        private Image leftBottomImage;

        [SerializeField]
        private Image rightBottomImage;

        #endregion

        #region Cached Components

        private Button button;

        #endregion

        #region Runtime State

        private ButtonVisualState runtimeState =
            ButtonVisualState.Normal;

        private bool pointerInside;

        #endregion

         #region Theme Targets

[Header("Theme Targets")]

[SerializeField]
private bool useGlobalThemeColor = true;

[SerializeField]
private Image backgroundImage;

[SerializeField]
private bool applyThemeToChildTMP = true;



#endregion
#region Cached Theme Components

private UI_Advanced_Neon_Controller neonController;

private TMP_Text[] childTMPTexts;
private Image logo;

        #endregion

        private void CacheThemeComponents()
{
    if (backgroundImage == null)
    {
        backgroundImage=GetComponent<Image>();
    }


        neonController =
        backgroundImage.GetComponent<
            UI_Advanced_Neon_Controller>();
            if (logo == null)
            {
                transform.Find("Logo")?.GetComponentInChildren<Image>(true);
            }

            if (applyThemeToChildTMP)
    {
        childTMPTexts =
            GetComponentsInChildren<TMP_Text>(
                true);
    }
}

        #region Properties

        /// <summary>
        /// Gets or sets the theme color.
        /// </summary>
        public ThemeColor ThemeColor
        {
            get => themeColor;
            set
            {
                themeColor = value;
                ApplyTheme();
            }
        }

        /// <summary>
        /// Gets or sets the preview state.
        /// </summary>
        public ButtonVisualState PreviewState
        {
            get => previewState;
            set
            {
                previewState = value;
                ApplyVisualState(previewState);
            }
        }

        /// <summary>
        /// Gets or sets the corner activation mode.
        /// </summary>
        public CornerMode CornerMode
        {
            get => cornerMode;
            set
            {
                cornerMode = value;

                if (cornerMode == CornerMode.RandomActive)
                {
                    RandomizeCorners();
                }

                ApplyCornerVisibility();
            }
        }

        /// <summary>
        /// Gets or sets the number of active random corners.
        /// </summary>
        public int CornerActiveCount
        {
            get => cornerActiveCount;
            set
            {
                cornerActiveCount =
                    Mathf.Clamp(value, 1, 4);

                if (cornerMode == CornerMode.RandomActive)
                {
                    RandomizeCorners();
                }

                ApplyCornerVisibility();
            }
        }

        #endregion

        /// <summary>
        /// Automatically searches the ButtonDesigner hierarchy for the
        /// Image that contains a UI_Advanced_Neon_Controller and assigns
        /// that Image as the button background target.
        ///
        /// The neon controller attached to that Image is then cached.
        /// </summary>
        public void AutoFindBackground()
        {
            Image[] images =
                GetComponentsInChildren<Image>(true);

            if (images == null ||
                images.Length == 0)
            {
                backgroundImage = null;
                neonController = null;
                return;
            }

            for (int i = 0;
                 i < images.Length;
                 i++)
            {
                Image image =
                    images[i];

                if (image == null)
                {
                    continue;
                }

                UI_Advanced_Neon_Controller controller =
                    image.GetComponent<
                        UI_Advanced_Neon_Controller>();

                if (controller == null)
                {
                    continue;
                }

                backgroundImage =
                    image;

                neonController =
                    controller;

                return;
            }

            backgroundImage = null;
            neonController = null;
        }
        /// <summary>
        /// Gets the UI_Advanced_Neon_Controller assigned to the
        /// button background.
        ///
        /// If the controller has not yet been cached, it is automatically
        /// retrieved from the assigned background Image.
        /// </summary>
        /// <returns>
        /// The cached UI_Advanced_Neon_Controller, or null if one
        /// cannot be found.
        /// </returns>
        public UI_Advanced_Neon_Controller
            GetBackgroundNeonController()
        {
            if (backgroundImage == null)
            {
                return null;
            }

            if (neonController == null)
            {
                neonController =
                    backgroundImage.GetComponent<
                        UI_Advanced_Neon_Controller>();
            }

            return neonController;
        }
        #region Editor Preview

        /// <summary>
        /// Applies the active UI theme neon data to the button's
        /// UI_Advanced_Neon_Controller.
        ///
        /// This method is intended for editor live preview and runtime
        /// theme refresh operations.
        /// </summary>
        public void ApplyNeonThemePreview()
        {
            // ------------------------------------------------------------
            // Get Theme Manager
            // ------------------------------------------------------------

            ThemeManager manager =
                ThemeManager.Instance;

            if (manager == null)
            {
                return;
            }

            // ------------------------------------------------------------
            // Get Active Theme
            // ------------------------------------------------------------

            UITheme theme =
                manager.ActiveTheme;

            if (theme == null)
            {
                return;
            }

            // ------------------------------------------------------------
            // Determine Effective Theme Color
            //
            // Global Theme:
            //     Uses ThemeManager.GlobalThemeColor
            //
            // Local Theme:
            //     Uses ButtonDesigner.themeColor
            // ------------------------------------------------------------

            ThemeColor effectiveColor;

            if (useGlobalThemeColor)
            {
                effectiveColor =
                    manager.GlobalThemeColor;
            }
            else
            {
                effectiveColor =
                    themeColor;
            }

            // ------------------------------------------------------------
            // Get Neon Controller
            // ------------------------------------------------------------

            UI_Advanced_Neon_Controller neon =
                GetBackgroundNeonController();

            if (neon == null)
            {
                return;
            }

            // ------------------------------------------------------------
            // Get Theme Neon Data
            // ------------------------------------------------------------

            UINeonThemeData neonData =
                theme.GetNeonData(
                    effectiveColor);

            if (neonData == null)
            {
                return;
            }

            // ------------------------------------------------------------
            // Apply Theme Data
            //
            // IMPORTANT:
            //
            // UITheme
            //     ↓
            // UINeonThemeData
            //     ↓
            // UI_Advanced_Neon_Controller
            //
            // The theme data is applied TO the controller.
            // ------------------------------------------------------------

            neon.ApplyThemeData(
                neonData);
        }


        /// <summary>
        /// Refreshes the complete ButtonDesigner editor preview.
        ///
        /// Applies:
        /// - Theme color
        /// - Theme brightness
        /// - Neon theme
        /// - Child TMP theme
        /// - Button images
        /// - Corner visibility
        ///
        /// This method is safe to call from the custom Unity Editor.
        /// </summary>
        public void RefreshEditorPreview()
        {
            // ------------------------------------------------------------
            // Apply Main Button Theme
            // ------------------------------------------------------------

            ApplyTheme();

            // ------------------------------------------------------------
            // Apply Neon Background Theme
            // ------------------------------------------------------------

            ApplyNeonThemePreview();

            // ------------------------------------------------------------
            // Apply Corner Configuration
            // ------------------------------------------------------------

            ApplyCornerVisibility();

#if UNITY_EDITOR

            // ------------------------------------------------------------
            // Refresh Unity Editor Scene View
            // ------------------------------------------------------------

            UnityEditor.EditorUtility.SetDirty(
                this);

            UnityEditor.SceneView.RepaintAll();

#endif
        }

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            CacheComponents();

            if (cornerMode == CornerMode.RandomActive)
            {
                RandomizeCorners();
            }

            ApplyCornerVisibility();
        }

        private void OnEnable()
        {
            CacheComponents();

            RegisterWithThemeManager();

            ApplyTheme();
        }

        private void OnDisable()
        {
            UnregisterFromThemeManager();
        }

        private void OnValidate()
        {
            CacheComponents();

            cornerActiveCount =
                Mathf.Clamp(cornerActiveCount, 1, 4);

            ApplyCornerVisibility();
            ApplyTheme();

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                UnityEditor.EditorUtility.SetDirty(this);
            }
#endif
        }

        #endregion

        #region Initialization

        private void CacheComponents()
        {
            if (button == null)
            {
                button = GetComponent<Button>();
            }
            CacheThemeComponents();
        }

        private void RegisterWithThemeManager()
        {
            ThemeManager manager =
                ThemeManager.Instance;

            if (manager != null)
            {
                manager.Register(this);
            }
        }

        private void UnregisterFromThemeManager()
        {
            ThemeManager manager =
                ThemeManager.Instance;

            if (manager != null)
            {
                manager.Unregister(this);
            }
        }

        #endregion

        #region Theme

        /// Applies the selected theme color to the complete button.
        /// This includes:
        ///
        /// 1. Button background neon controller.
        /// 2. Button decorative images.
        /// 3. Optional child TMP text.
        /// 4. Theme-specific TMP font.
        /// </summary>
        /// <summary>
        /// Applies the selected theme color to the complete button.
        /// This includes:
        ///
        /// 1. Button background neon controller.
        /// 2. Button decorative images.
        /// 3. Optional child TMP text.
        /// 4. Theme-specific TMP font.
        /// </summary>
        /// 
        private ThemeColor GetEffectiveThemeColor()
        {
            if (useGlobalThemeColor)
            {
                ThemeManager manager =
                    ThemeManager.Instance;

                if (manager != null)
                {
                    return manager.GlobalThemeColor;
                }
            }

            return themeColor;
        }
        /// <summary>
        /// Applies the complete theme to this button.
        /// </summary>
        public void ApplyTheme()
        {
            ThemeManager manager =
                ThemeManager.Instance;

            if (manager == null)
            {
                return;
            }

            UITheme theme =
                manager.ActiveTheme;

            if (theme == null)
            {
                return;
            }
             logo =
               transform.Find("Logo")
               ?.GetComponentInChildren<Image>(true);

            if (logo != null)
            {
                logo.color =
                    ThemeManager.Instance
                        .ActiveTheme
                        .GetColor(
                            ThemeManager.Instance.GlobalThemeColor);
            }



            ThemeColor effectiveColor =
                GetEffectiveThemeColor();

            Color baseColor =
                theme.GetColor(
                    effectiveColor);           

            ButtonVisualState currentState =
                Application.isPlaying
                    ? runtimeState
                    : previewState;

            float brightness =
                theme.GetStateBrightness(
                    currentState);

            Color finalColor =
                MultiplyBrightness(
                    baseColor,
                    brightness);

            // Apply color to button images.
            ApplyColorToAllImages(
                finalColor);

            // Apply TMP font and color.
            ApplyTMPTheme(
                theme,
                effectiveColor,
                finalColor);

            // Apply neon configuration.
            ApplyNeonTheme(
                theme,
                effectiveColor);

            // Apply selected corners.
            ApplyCornerVisibility();
        }

        private void ApplyNeonTheme(
           UITheme theme,
           ThemeColor effectiveColor)
        {
            if (theme == null)
            {
                return;
            }

            if (backgroundImage == null)
            {
                return;
            }

            if (neonController == null)
            {
                neonController =
                    backgroundImage.GetComponent<
                        UI_Advanced_Neon_Controller>();
            }

            if (neonController == null)
            {
                return;
            }

            UINeonThemeData neonData =
                theme.GetNeonData(
                    effectiveColor);

            if (neonData == null)
            {
                return;
            }

            neonController.ApplyThemeData(
                neonData);
        }
        private void ApplyTMPTheme(
            UITheme theme,
            ThemeColor effectiveColor,
            Color finalColor)
        {
            if (!applyThemeToChildTMP)
            {
                return;
            }

            if (childTMPTexts == null)
            {
                childTMPTexts =
                    GetComponentsInChildren<TMP_Text>(
                        true);
            }

            if (childTMPTexts == null)
            {
                return;
            }

            TMP_FontAsset font =
                theme.GetFont(
                    effectiveColor);

            for (int i = 0;
                 i < childTMPTexts.Length;
                 i++)
            {
                TMP_Text text =
                    childTMPTexts[i];

                if (text == null)
                {
                    continue;
                }

                if (font != null)
                {
                    text.font = font;
                }

                text.color =
                    finalColor;
            }
        }




        private Color GetThemeColorFromManager()
        {
            ThemeManager manager =
                ThemeManager.Instance;

            if (manager != null)
            {
                return manager.GetThemeColor(themeColor);
            }

            return GetEditorPreviewColor();
        }

        private float GetStateBrightnessFromManager(
            ButtonVisualState state)
        {
            ThemeManager manager =
                ThemeManager.Instance;

            if (manager != null)
            {
                return manager.GetStateBrightness(state);
            }

            return GetEditorPreviewBrightness(state);
        }

        #endregion

        #region Visual State

        /// <summary>
        /// Applies a specific visual state to the button.
        /// </summary>
        /// <param name="state">The desired visual state.</param>
        public void ApplyVisualState(
            ButtonVisualState state)
        {
            if (Application.isPlaying)
            {
                runtimeState = state;
            }
            else
            {
                previewState = state;
            }

            ApplyTheme();
        }

        private void ApplyColorToAllImages(Color color)
        {
            SetImageColor(topImage, color);
            SetImageColor(bottomImage, color);
            SetImageColor(leftImage, color);
            SetImageColor(rightImage, color);

            SetImageColor(leftTopImage, color);
            SetImageColor(rightTopImage, color);
            SetImageColor(leftBottomImage, color);
            SetImageColor(rightBottomImage, color);
        }

        private static void SetImageColor(
            Image image,
            Color color)
        {
            if (image == null)
            {
                return;
            }

            Color currentColor =
                image.color;

            color.a =
                currentColor.a;

            image.color = color;
        }

        private static Color MultiplyBrightness(
            Color color,
            float brightness)
        {
            color.r =
                Mathf.Clamp01(
                    color.r * brightness);

            color.g =
                Mathf.Clamp01(
                    color.g * brightness);

            color.b =
                Mathf.Clamp01(
                    color.b * brightness);

            return color;
        }

        #endregion

        #region Corner System

        /// <summary>
        /// Randomly activates the configured number of corners.
        /// No corner is selected more than once.
        /// </summary>
        public void RandomizeCorners()
        {
            cornerActiveCount =
                Mathf.Clamp(
                    cornerActiveCount,
                    1,
                    4);

            leftTopActive = false;
            rightTopActive = false;
            leftBottomActive = false;
            rightBottomActive = false;

            int[] indices =
            {
                0,
                1,
                2,
                3
            };

            for (int i = indices.Length - 1;
                 i > 0;
                 i--)
            {
                int randomIndex =
                    Random.Range(0, i + 1);

                int temporary =
                    indices[i];

                indices[i] =
                    indices[randomIndex];

                indices[randomIndex] =
                    temporary;
            }

            for (int i = 0;
                 i < cornerActiveCount;
                 i++)
            {
                SetCornerActive(
                    indices[i],
                    true);
            }

            ApplyCornerVisibility();
        }

        /// <summary>
        /// Applies the current corner activation configuration.
        /// </summary>
        public void ApplyCornerVisibility()
        {
            if (cornerMode ==
                CornerMode.CustomActive)
            {
                ApplyCustomCorners();
            }

            SetImageActive(
                leftTopImage,
                leftTopActive);

            SetImageActive(
                rightTopImage,
                rightTopActive);

            SetImageActive(
                leftBottomImage,
                leftBottomActive);

            SetImageActive(
                rightBottomImage,
                rightBottomActive);
        }

        private void ApplyCustomCorners()
        {
            // Custom mode intentionally does not modify
            // the corner boolean values.
            // The designer controls them manually.
        }

        private void SetCornerActive(
            int index,
            bool active)
        {
            switch (index)
            {
                case 0:
                    leftTopActive = active;
                    break;

                case 1:
                    rightTopActive = active;
                    break;

                case 2:
                    leftBottomActive = active;
                    break;

                case 3:
                    rightBottomActive = active;
                    break;
            }
        }

        private static void SetImageActive(
            Image image,
            bool active)
        {
            if (image == null)
            {
                return;
            }

            image.gameObject.SetActive(active);
        }

        #endregion

        #region Pointer Events

        /// <inheritdoc />
        public void OnPointerEnter(
            PointerEventData eventData)
        {
            pointerInside = true;

            if (!IsInteractable())
            {
                return;
            }

            runtimeState =
                ButtonVisualState.Hover;

            ApplyTheme();
        }

        /// <inheritdoc />
        public void OnPointerExit(
            PointerEventData eventData)
        {
            pointerInside = false;

            if (!IsInteractable())
            {
                return;
            }

            runtimeState =
                button != null &&
                button.IsInteractable()
                    ? ButtonVisualState.Normal
                    : ButtonVisualState.Disabled;

            ApplyTheme();
        }

        /// <inheritdoc />
        public void OnPointerDown(
            PointerEventData eventData)
        {
            if (!IsInteractable())
            {
                return;
            }

            runtimeState =
                ButtonVisualState.Pressed;

            ApplyTheme();
        }

        /// <inheritdoc />
        public void OnPointerUp(
            PointerEventData eventData)
        {
            if (!IsInteractable())
            {
                return;
            }

            runtimeState =
                pointerInside
                    ? ButtonVisualState.Hover
                    : ButtonVisualState.Normal;

            ApplyTheme();
        }

        #endregion

        #region Selection Events

        /// <inheritdoc />
        public void OnSelect(
            BaseEventData eventData)
        {
            if (!IsInteractable())
            {
                return;
            }

            runtimeState =
                ButtonVisualState.Selected;

            ApplyTheme();
        }

        /// <inheritdoc />
        public void OnDeselect(
            BaseEventData eventData)
        {
            if (!IsInteractable())
            {
                return;
            }

            runtimeState =
                pointerInside
                    ? ButtonVisualState.Hover
                    : ButtonVisualState.Normal;

            ApplyTheme();
        }

        #endregion

        #region Utility

        private bool IsInteractable()
        {
            return button != null &&
                   button.IsInteractable();
        }

        private static float GetEditorPreviewBrightness(
            ButtonVisualState state)
        {
            switch (state)
            {
                case ButtonVisualState.Hover:
                    return 1.15f;

                case ButtonVisualState.Pressed:
                    return 0.80f;

                case ButtonVisualState.Selected:
                    return 1.10f;

                case ButtonVisualState.Disabled:
                    return 0.40f;

                case ButtonVisualState.Normal:
                default:
                    return 1f;
            }
        }

        private Color GetEditorPreviewColor()
        {
            switch (themeColor)
            {
                case ThemeColor.White:
                    return Color.white;

                case ThemeColor.Red:
                    return Color.red;

                case ThemeColor.Blue:
                    return Color.blue;

                case ThemeColor.Pink:
                    return new Color(
                        1f,
                        0.2f,
                        0.6f,
                        1f);

                case ThemeColor.Orange:
                    return new Color(
                        1f,
                        0.45f,
                        0f,
                        1f);

                case ThemeColor.Green:
                    return Color.green;

                default:
                    return Color.white;
            }
        }

        #endregion

        #region Editor Accessors

#if UNITY_EDITOR

        /// <summary>
        /// Gets the assigned top image.
        /// </summary>
        public Image TopImage => topImage;

        /// <summary>
        /// Gets the assigned bottom image.
        /// </summary>
        public Image BottomImage => bottomImage;

        /// <summary>
        /// Gets the assigned left image.
        /// </summary>
        public Image LeftImage => leftImage;

        /// <summary>
        /// Gets the assigned right image.
        /// </summary>
        public Image RightImage => rightImage;

        /// <summary>
        /// Gets the assigned left top image.
        /// </summary>
        public Image LeftTopImage => leftTopImage;

        /// <summary>
        /// Gets the assigned right top image.
        /// </summary>
        public Image RightTopImage => rightTopImage;

        /// <summary>
        /// Gets the assigned left bottom image.
        /// </summary>
        public Image LeftBottomImage => leftBottomImage;

        /// <summary>
        /// Gets the assigned right bottom image.
        /// </summary>
        public Image RightBottomImage => rightBottomImage;

        /// <summary>
        /// Sets an image slot.
        /// </summary>
        /// <param name="slot">Image slot.</param>
        /// <param name="image">Image reference.</param>
        public void SetImage(
            ButtonImageSlot slot,
            Image image)
        {
            switch (slot)
            {
                case ButtonImageSlot.Top:
                    topImage = image;
                    break;

                case ButtonImageSlot.Bottom:
                    bottomImage = image;
                    break;

                case ButtonImageSlot.Left:
                    leftImage = image;
                    break;

                case ButtonImageSlot.Right:
                    rightImage = image;
                    break;

                case ButtonImageSlot.LeftTop:
                    leftTopImage = image;
                    break;

                case ButtonImageSlot.RightTop:
                    rightTopImage = image;
                    break;

                case ButtonImageSlot.LeftBottom:
                    leftBottomImage = image;
                    break;

                case ButtonImageSlot.RightBottom:
                    rightBottomImage = image;
                    break;
            }
        }

#endif

        #endregion
    }
}