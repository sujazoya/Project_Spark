using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.UI
{
    /// <summary>
    /// Applies the active Project Spark UI theme to a UI element.
    ///
    /// Supported components:
    ///
    /// Image
    /// - Receives the active theme color.
    ///
    /// TMP_Text
    /// - Receives the TMP Font Asset assigned to the
    ///   active theme color.
    /// - Receives the active theme color.
    ///
    /// This component implements IThemeObject and is therefore
    /// refreshed by ThemeManager.RefreshAllUI().
    /// </summary>
    [DisallowMultipleComponent]
    [ExecuteAlways]
    public sealed class ThemeApplier :
        MonoBehaviour,
        IThemeObject
    {
        #region Inspector

        [Header("Theme Targets")]

        [Tooltip(
            "Apply the theme to an Image on this GameObject.")]
        [SerializeField]
        private bool applyImage =
            false;

        [Tooltip(
            "Apply the theme to a TMP text component on this GameObject.")]
        [SerializeField]
        private bool applyTMP =
            false;


        [Header("Search Children")]

        [Tooltip(
            "If enabled, searches child objects for Image and TMP components.")]
        [SerializeField]
        private bool includeChildren =
            false;


        [Tooltip(
            "If enabled, inactive child objects are also searched.")]
        [SerializeField]
        private bool includeInactiveChildren =
            false;


        [Header("Theme Color")]

        [Tooltip(
            "If enabled, Image and TMP text receive the theme color.")]
        [SerializeField]
        private bool applyThemeColor =
            false;


        [Header("TMP Font")]

        [Tooltip(
            "If enabled, TMP text receives the font assigned to the theme color.")]
        [SerializeField]
        private bool applyThemeFont =
            false;

        #endregion


        #region Cached Components

        private Image image;

        private TMP_Text tmpText;

        private Image[] childImages;

        private TMP_Text[] childTMPTexts;

        #endregion


        #region Unity Lifecycle

        /// <summary>
        /// Initializes component references.
        /// </summary>
        private void Awake()
        {
            CacheComponents();
        }


        /// <summary>
        /// Registers this component with the ThemeManager.
        /// </summary>
        private void OnEnable()
        {
            CacheComponents();

            ThemeManager manager =
                ThemeManager.Instance;

            if (manager != null)
            {
                manager.Register(
                    this);
            }

            ApplyTheme();
        }


        /// <summary>
        /// Unregisters this component.
        /// </summary>
        private void OnDisable()
        {
            ThemeManager manager =
                ThemeManager.Instance;

            if (manager != null)
            {
                manager.Unregister(
                    this);
            }
        }


        /// <summary>
        /// Refreshes component references when the
        /// component configuration changes in the Inspector.
        /// </summary>
        private void OnValidate()
        {
            CacheComponents();

            ApplyTheme();
        }

        #endregion


        #region Cache

        /// <summary>
        /// Caches all supported UI components.
        /// </summary>
        private void CacheComponents()
        {
            image =
                GetComponent<Image>();

            tmpText =
                GetComponent<TMP_Text>();

            if (includeChildren)
            {
                childImages =
                    GetComponentsInChildren<Image>(
                        includeInactiveChildren);

                childTMPTexts =
                    GetComponentsInChildren<TMP_Text>(
                        includeInactiveChildren);
            }
            else
            {
                childImages =
                    null;

                childTMPTexts =
                    null;
            }
        }

        #endregion


        #region Theme

        /// <summary>
        /// Applies the currently active Project Spark theme.
        ///
        /// This method is called by ThemeManager.RefreshAllUI().
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

            ThemeColor themeColor =
                manager.GlobalThemeColor;

            Color color =
                theme.GetColor(
                    themeColor);

            TMP_FontAsset font =
                theme.GetFont(
                    themeColor);

            ApplyImageTheme(
                color);

            ApplyTMPTheme(
                color,
                font);
        }

        #endregion


        #region Image

        /// <summary>
        /// Applies the theme color to Image components.
        /// </summary>
        /// <param name="color">
        /// The active theme color.
        /// </param>
        private void ApplyImageTheme(
            Color color)
        {
            if (!applyImage)
            {
                return;
            }

            if (image != null &&
                image != this)
            {
                if (applyThemeColor)
                {
                    image.color =
                        color;
                }
            }

            if (childImages == null)
            {
                return;
            }

            for (int i = 0;
                 i < childImages.Length;
                 i++)
            {
                Image target =
                    childImages[i];

                if (target == null)
                {
                    continue;
                }

                if (target == image)
                {
                    continue;
                }

                if (applyThemeColor)
                {
                    target.color =
                        color;
                }
            }
        }

        #endregion


        #region TMP

        /// <summary>
        /// Applies the theme color and font to TMP components.
        /// </summary>
        /// <param name="color">
        /// The active theme color.
        /// </param>
        /// <param name="font">
        /// The TMP font assigned to the active theme color.
        /// </param>
        private void ApplyTMPTheme(
            Color color,
            TMP_FontAsset font)
        {
            if (!applyTMP)
            {
                return;
            }

            if (tmpText != null)
            {
                ApplyTMP(
                    tmpText,
                    color,
                    font);
            }

            if (childTMPTexts == null)
            {
                return;
            }

            for (int i = 0;
                 i < childTMPTexts.Length;
                 i++)
            {
                TMP_Text target =
                    childTMPTexts[i];

                if (target == null)
                {
                    continue;
                }

                if (target == tmpText)
                {
                    continue;
                }

                ApplyTMP(
                    target,
                    color,
                    font);
            }
        }


        /// <summary>
        /// Applies the theme data to a TMP component.
        /// </summary>
        /// <param name="target">
        /// Target TMP component.
        /// </param>
        /// <param name="color">
        /// Active theme color.
        /// </param>
        /// <param name="font">
        /// Active theme font.
        /// </param>
        private void ApplyTMP(
            TMP_Text target,
            Color color,
            TMP_FontAsset font)
        {
            if (target == null)
            {
                return;
            }

            if (applyThemeColor)
            {
                target.color =
                    color;
            }

            if (applyThemeFont &&
                font != null)
            {
                target.font =
                    font;
            }
        }

        #endregion
    }
}