using TMPro;
using UnityEngine;

namespace ProjectSpark.UI
{
    /// <summary>
    /// Defines the complete visual configuration for a Project Spark UI theme.
    ///
    /// Each supported theme color contains:
    /// - Base UI Color
    /// - TMP Font Asset
    /// - Neon Theme Data
    ///
    /// The neon data is applied at runtime or edit time to the
    /// UI_Advanced_Neon_Controller attached to the target UI element.
    ///
    /// The UITheme asset does not store references to scene-based
    /// UI_Advanced_Neon_Controller components.
    /// </summary>
    [CreateAssetMenu(
        fileName = "UITheme",
        menuName = "Project Spark/UI/Theme",
        order = 0)]
    public sealed class UITheme : ScriptableObject
    {
        #region Inspector

        [Header("Theme")]

        [Tooltip("Display name of this UI theme.")]
        [SerializeField]
        private string themeName = "Default Theme";

        // ========================================================
        // WHITE
        // ========================================================

        [Header("White")]

        [Tooltip("Base UI color used by the White theme.")]
        [SerializeField]
        private Color white = Color.white;

        [Tooltip("TMP Font Asset used by the White theme.")]
        [SerializeField]
        private TMP_FontAsset whiteFont;

        [Tooltip("Neon configuration used by the White theme.")]
        [SerializeField]
        private UINeonThemeData whiteNeon =
            new UINeonThemeData();

        // ========================================================
        // RED
        // ========================================================

        [Header("Red")]

        [Tooltip("Base UI color used by the Red theme.")]
        [SerializeField]
        private Color red = Color.red;

        [Tooltip("TMP Font Asset used by the Red theme.")]
        [SerializeField]
        private TMP_FontAsset redFont;

        [Tooltip("Neon configuration used by the Red theme.")]
        [SerializeField]
        private UINeonThemeData redNeon =
            new UINeonThemeData();

        // ========================================================
        // BLUE
        // ========================================================

        [Header("Blue")]

        [Tooltip("Base UI color used by the Blue theme.")]
        [SerializeField]
        private Color blue = Color.blue;

        [Tooltip("TMP Font Asset used by the Blue theme.")]
        [SerializeField]
        private TMP_FontAsset blueFont;

        [Tooltip("Neon configuration used by the Blue theme.")]
        [SerializeField]
        private UINeonThemeData blueNeon =
            new UINeonThemeData();

        // ========================================================
        // PINK
        // ========================================================

        [Header("Pink")]

        [Tooltip("Base UI color used by the Pink theme.")]
        [SerializeField]
        private Color pink =
            new Color(
                1f,
                0.2f,
                0.6f,
                1f);

        [Tooltip("TMP Font Asset used by the Pink theme.")]
        [SerializeField]
        private TMP_FontAsset pinkFont;

        [Tooltip("Neon configuration used by the Pink theme.")]
        [SerializeField]
        private UINeonThemeData pinkNeon =
            new UINeonThemeData();

        // ========================================================
        // ORANGE
        // ========================================================

        [Header("Orange")]

        [Tooltip("Base UI color used by the Orange theme.")]
        [SerializeField]
        private Color orange =
            new Color(
                1f,
                0.45f,
                0f,
                1f);

        [Tooltip("TMP Font Asset used by the Orange theme.")]
        [SerializeField]
        private TMP_FontAsset orangeFont;

        [Tooltip("Neon configuration used by the Orange theme.")]
        [SerializeField]
        private UINeonThemeData orangeNeon =
            new UINeonThemeData();

        // ========================================================
        // GREEN
        // ========================================================

        [Header("Green")]

        [Tooltip("Base UI color used by the Green theme.")]
        [SerializeField]
        private Color green = Color.green;

        [Tooltip("TMP Font Asset used by the Green theme.")]
        [SerializeField]
        private TMP_FontAsset greenFont;

        [Tooltip("Neon configuration used by the Green theme.")]
        [SerializeField]
        private UINeonThemeData greenNeon =
            new UINeonThemeData();

        // ========================================================
        // STATE BRIGHTNESS
        // ========================================================

        [Header("State Brightness")]

        [Tooltip("Brightness multiplier for the Hover state.")]
        [Range(0f, 2f)]
        [SerializeField]
        private float hoverBrightness = 1.15f;

        [Tooltip("Brightness multiplier for the Pressed state.")]
        [Range(0f, 2f)]
        [SerializeField]
        private float pressedBrightness = 0.80f;

        [Tooltip("Brightness multiplier for the Selected state.")]
        [Range(0f, 2f)]
        [SerializeField]
        private float selectedBrightness = 1.10f;

        [Tooltip("Brightness multiplier for the Disabled state.")]
        [Range(0f, 2f)]
        [SerializeField]
        private float disabledBrightness = 0.40f;

        #endregion


        #region Properties

        /// <summary>
        /// Gets the display name of this UI theme.
        /// </summary>
        public string ThemeName =>
            themeName;

        /// <summary>
        /// Gets the brightness multiplier used for Hover state.
        /// </summary>
        public float HoverBrightness =>
            hoverBrightness;

        /// <summary>
        /// Gets the brightness multiplier used for Pressed state.
        /// </summary>
        public float PressedBrightness =>
            pressedBrightness;

        /// <summary>
        /// Gets the brightness multiplier used for Selected state.
        /// </summary>
        public float SelectedBrightness =>
            selectedBrightness;

        /// <summary>
        /// Gets the brightness multiplier used for Disabled state.
        /// </summary>
        public float DisabledBrightness =>
            disabledBrightness;

        #endregion


        #region Color

        /// <summary>
        /// Gets the configured base color for the specified theme color.
        /// </summary>
        /// <param name="themeColor">
        /// The requested Project Spark theme color.
        /// </param>
        /// <returns>
        /// The configured Unity color.
        /// </returns>
        public Color GetColor(
            ThemeColor themeColor)
        {
            switch (themeColor)
            {
                case ThemeColor.White:
                    return white;

                case ThemeColor.Red:
                    return red;

                case ThemeColor.Blue:
                    return blue;

                case ThemeColor.Pink:
                    return pink;

                case ThemeColor.Orange:
                    return orange;

                case ThemeColor.Green:
                    return green;

                default:
                    return white;
            }
        }

        #endregion


        #region TMP Font

        /// <summary>
        /// Gets the TMP Font Asset assigned to the specified theme color.
        /// </summary>
        /// <param name="themeColor">
        /// The requested Project Spark theme color.
        /// </param>
        /// <returns>
        /// The configured TMP Font Asset.
        /// Returns null when no font has been assigned.
        /// </returns>
        public TMP_FontAsset GetFont(
            ThemeColor themeColor)
        {
            switch (themeColor)
            {
                case ThemeColor.White:
                    return whiteFont;

                case ThemeColor.Red:
                    return redFont;

                case ThemeColor.Blue:
                    return blueFont;

                case ThemeColor.Pink:
                    return pinkFont;

                case ThemeColor.Orange:
                    return orangeFont;

                case ThemeColor.Green:
                    return greenFont;

                default:
                    return whiteFont;
            }
        }

        #endregion


        #region Neon Theme Data

        /// <summary>
        /// Gets the neon theme configuration assigned to the specified
        /// Project Spark theme color.
        ///
        /// This returns configuration data rather than a scene-based
        /// UI_Advanced_Neon_Controller component.
        ///
        /// The returned data can be passed to a
        /// UI_Advanced_Neon_Controller attached to a UI background.
        /// </summary>
        /// <param name="themeColor">
        /// The requested Project Spark theme color.
        /// </param>
        /// <returns>
        /// The configured neon theme data.
        /// </returns>
        public UINeonThemeData GetNeonData(
            ThemeColor themeColor)
        {
            switch (themeColor)
            {
                case ThemeColor.White:
                    return whiteNeon;

                case ThemeColor.Red:
                    return redNeon;

                case ThemeColor.Blue:
                    return blueNeon;

                case ThemeColor.Pink:
                    return pinkNeon;

                case ThemeColor.Orange:
                    return orangeNeon;

                case ThemeColor.Green:
                    return greenNeon;

                default:
                    return whiteNeon;
            }
        }

        #endregion


        #region State Brightness

        /// <summary>
        /// Gets the brightness multiplier associated with a UI state.
        /// </summary>
        /// <param name="state">
        /// The requested button visual state.
        /// </param>
        /// <returns>
        /// The configured brightness multiplier.
        /// </returns>
        public float GetStateBrightness(
            ButtonVisualState state)
        {
            switch (state)
            {
                case ButtonVisualState.Hover:
                    return hoverBrightness;

                case ButtonVisualState.Pressed:
                    return pressedBrightness;

                case ButtonVisualState.Selected:
                    return selectedBrightness;

                case ButtonVisualState.Disabled:
                    return disabledBrightness;

                case ButtonVisualState.Normal:
                default:
                    return 1f;
            }
        }

        #endregion
    }
}