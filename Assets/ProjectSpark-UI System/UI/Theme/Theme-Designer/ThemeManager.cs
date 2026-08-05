using System.Collections.Generic;
using UnityEngine;

namespace ProjectSpark.UI
{
    /// <summary>
    /// Central manager responsible for controlling the active Project Spark UI theme.
    /// </summary>
    [ExecuteAlways]
    public sealed class ThemeManager : MonoBehaviour
    {
        #region Singleton

        private static ThemeManager instance;

        /// <summary>
        /// Gets the active ThemeManager instance.
        /// </summary>
        public static ThemeManager Instance => instance;

        #endregion

        #region Inspector

        [Header("Theme")]

        [SerializeField]
        private UITheme defaultTheme;

        [SerializeField]
        private UITheme activeTheme;

        #endregion

        #region Runtime Data

        private readonly HashSet<IThemeObject> registeredObjects =
            new HashSet<IThemeObject>();

        #endregion

        #region Properties

        /// <summary>
        /// Gets the currently active UI theme.
        /// </summary>
        public UITheme ActiveTheme => activeTheme != null
            ? activeTheme
            : defaultTheme;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            InitializeSingleton();
            InitializeTheme();
        }

        private void OnEnable()
        {
            InitializeSingleton();
            InitializeTheme();
        }

        private void OnDisable()
        {
            if (instance == this)
            {
                instance = null;
            }
        }

        #endregion

        #region Global Theme Color

/// <summary>
/// Gets the globally selected theme color.
/// </summary>
public ThemeColor GlobalThemeColor =>
    globalThemeColor;

#endregion
[SerializeField]
private ThemeColor globalThemeColor =
    ThemeColor.White;

    /// <summary>
/// Sets the global theme color used by registered UI objects.
/// </summary>
/// <param name="color">
/// The new global theme color.
/// </param>
public void SetGlobalThemeColor(
    ThemeColor color)
{
    globalThemeColor = color;
            RefreshAllUI();
  }
/// <summary>
/// Gets the active global theme color.
/// </summary>
/// <returns>
/// The currently selected global theme color.
/// </returns>
public ThemeColor GetGlobalThemeColor()
{
    return globalThemeColor;
}

        #region Initialization

        private void InitializeSingleton()
        {
            if (instance == null)
            {
                instance = this;
                return;
            }

            if (instance != this)
            {
                Debug.LogWarning(
                    "Multiple Project Spark ThemeManager instances detected. " +
                    "Only one ThemeManager should exist in a scene.",
                    this);
            }
        }

        private void InitializeTheme()
        {
            if (activeTheme == null)
            {
                activeTheme = defaultTheme;
            }
        }

        #endregion

        #region Registration
       
        /// </summary>
        /// <param name="themeObject">
        /// The UI object to register.
        /// </param>
        public void Register(
            IThemeObject themeObject)
        {
            if (themeObject == null)
            {
                return;
            }

            if (!registeredObjects.Add(
                    themeObject))
            {
                return;
            }

            themeObject.ApplyTheme();
        }

        /// <summary>
        /// Unregisters a UI object from the theme manager.
        /// </summary>
        /// <param name="themeObject">The UI object to unregister.</param>
        public void Unregister(IThemeObject themeObject)
        {
            if (themeObject == null)
            {
                return;
            }

            registeredObjects.Remove(themeObject);
        }

        #endregion

        #region Theme Management

        /// <summary>
        /// Sets the active UI theme and refreshes all registered UI objects.
        /// </summary>
        /// <param name="theme">The new active theme.</param>
        public void SetTheme(UITheme theme)
        {
            if (theme == null)
            {
                return;
            }

            activeTheme = theme;

            RefreshAllUI();
        }

        /// <summary>
        /// Refreshes every registered UI object.
        /// </summary>
       /*   public void RefreshAllUI()
          {
              Debug.Log(
                  "ThemeManager.RefreshAllUI() CALLED",
                  this);

              ButtonDesigner[] buttons =
                  FindObjectsByType<ButtonDesigner>(
                      FindObjectsInactive.Include,
                      FindObjectsSortMode.None);

              Debug.Log(
                  "FOUND BUTTONS: " +
                  buttons.Length);

              for (int i = 0;
                   i < buttons.Length;
                   i++)
              {
                  ButtonDesigner button =
                      buttons[i];

                  if (button == null)
                  {
                      continue;
                  }

                  Debug.Log(
                      "APPLYING THEME TO: " +
                      button.gameObject.name);

                  button.ApplyTheme();
              }
          }*/
        /// <summary>
        /// Refreshes every registered Project Spark UI theme object.
        ///
        /// This is the single official UI refresh entry point.
        /// Every registered IThemeObject receives ApplyTheme().
        /// </summary>
        /// <summary>
        /// Refreshes every registered Project Spark UI theme object.
        ///
        /// This is the single official UI refresh entry point.
        /// Every registered IThemeObject receives ApplyTheme().
        /// </summary>
        /// 
        public void RefreshAllUI()
        {
            Debug.Log(
                "ThemeManager.RefreshAllUI() CALLED",
                this);

            MonoBehaviour[] components =
                FindObjectsByType<MonoBehaviour>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None);

            int refreshedCount = 0;

            for (int i = 0;
                 i < components.Length;
                 i++)
            {
                MonoBehaviour component =
                    components[i];

                if (component == null)
                {
                    continue;
                }

                IThemeObject themeObject =
                    component as IThemeObject;

                if (themeObject == null)
                {
                    continue;
                }

                themeObject.ApplyTheme();

                refreshedCount++;
            }

            Debug.Log(
                "ThemeManager: Refreshed " +
                refreshedCount +
                " IThemeObject components.",
                this);
        }

        #endregion

        #region Theme Access

        /// <summary>
        /// Gets a color from the active theme.
        /// </summary>
        /// <param name="themeColor">The requested theme color.</param>
        /// <returns>The configured color.</returns>
        public Color GetThemeColor(ThemeColor themeColor)
        {
            UITheme theme = ActiveTheme;

            if (theme == null)
            {
                return Color.white;
            }

            return theme.GetColor(themeColor);
        }

        /// <summary>
        /// Gets the brightness multiplier for a UI state.
        /// </summary>
        /// <param name="state">The requested visual state.</param>
        /// <returns>The state brightness multiplier.</returns>
        public float GetStateBrightness(ButtonVisualState state)
        {
            UITheme theme = ActiveTheme;

            if (theme == null)
            {
                return 1f;
            }

            return theme.GetStateBrightness(state);
        }

        #endregion
        #region TMP Font

/// <summary>
/// Gets the TMP font associated with the specified theme color.
/// </summary>
/// <param name="themeColor">
/// The requested theme color.
/// </param>
/// <returns>
/// The configured TMP font asset.
/// </returns>
public TMPro.TMP_FontAsset GetThemeFont(
    ThemeColor themeColor)
{
    UITheme theme = ActiveTheme;

    if (theme == null)
    {
        return null;
    }

    return theme.GetFont(themeColor);
}

        #endregion

        #region Neon Controller

        /// <summary>
        /// Gets the Advanced Neon Controller associated with
        /// the specified theme color.
        /// </summary>
        /// <param name="themeColor">
        /// The requested theme color.
        /// </param>
        /// <returns>
        /// The configured neon controller.
        /// </returns>
        /// <summary>
        /// Gets the neon theme configuration for the specified theme color.
        /// </summary>
        /// <param name="themeColor">
        /// The requested Project Spark theme color.
        /// </param>
        /// <returns>
        /// The neon configuration associated with the requested theme color.
        /// Returns null if no active theme is available.
        /// </returns>
        public UINeonThemeData GetNeonData(
            ThemeColor themeColor)
        {
            if (activeTheme == null)
            {
                return null;
            }

            return activeTheme.GetNeonData(
                themeColor);
        }

        #endregion
    }
}