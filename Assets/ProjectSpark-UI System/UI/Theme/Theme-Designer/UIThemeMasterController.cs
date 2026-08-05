using UnityEngine;

namespace ProjectSpark.UI
{
/// <summary>
/// Project Spark Master Theme Controller.
///
/// This component provides a single control point for changing
/// the global UI theme and global theme color.
///
/// Example:
///
/// MasterThemeController.SetThemeColor(ThemeColor.Blue);
///
/// All registered Project Spark UI theme objects are then refreshed.
///
/// Architecture:
///
/// MasterThemeController
///         |
///         v
///     ThemeManager
///         |
///         v
///       UITheme
///         |
///         v
///  Registered UI Objects
///
/// ButtonDesigner, PanelDesigner, WindowDesigner, etc.
/// </summary>
[DisallowMultipleComponent]
public sealed class MasterThemeController :
MonoBehaviour
{
#region Singleton


    private static MasterThemeController instance;

    /// <summary>
    /// Gets the active MasterThemeController instance.
    /// </summary>
    public static MasterThemeController Instance =>
        instance;

    #endregion


    #region Inspector

    [Header("Master Theme")]

    [Tooltip(
        "The UITheme asset used as the main Project Spark UI theme.")]
    [SerializeField]
    private UITheme mainTheme;

    [Tooltip(
        "The global color used by all UI objects that follow the master theme.")]
    [SerializeField]
    private ThemeColor globalThemeColor =
        ThemeColor.White;

    [Header("Startup")]

    [Tooltip(
        "Automatically applies the master theme when this component is enabled.")]
    [SerializeField]
    private bool applyOnEnable = true;

    [Tooltip(
        "Refreshes all registered UI objects after changing the master theme.")]
    [SerializeField]
    private bool refreshAllUI = true;

    #endregion


    #region Properties

    /// <summary>
    /// Gets the main UITheme asset.
    /// </summary>
    public UITheme MainTheme =>
        mainTheme;


    /// <summary>
    /// Gets the current global theme color.
    /// </summary>
    public ThemeColor GlobalThemeColor =>
        globalThemeColor;

    #endregion


    #region Unity Lifecycle

    /// <summary>
    /// Initializes the MasterThemeController.
    /// </summary>
    private void Awake()
    {
        if (instance != null &&
            instance != this)
        {
            Destroy(
                gameObject);

            return;
        }

        instance = this;
    }


    /// <summary>
    /// Applies the configured master theme when enabled.
    /// </summary>
    private void OnEnable()
    {
        if (!applyOnEnable)
        {
            return;
        }

        ApplyMasterTheme();
    }


    /// <summary>
    /// Clears the singleton reference.
    /// </summary>
    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    #endregion


    #region Theme

    /// <summary>
    /// Sets the main UITheme asset.
    /// </summary>
    /// <param name="theme">
    /// The UITheme asset to use as the main theme.
    /// </param>
    public void SetMainTheme(
        UITheme theme)
    {
        if (theme == null)
        {
            return;
        }

        mainTheme =
            theme;

        ApplyMasterTheme();
    }


    /// <summary>
    /// Sets the global Project Spark theme color.
    /// </summary>
    /// <param name="color">
    /// The new global theme color.
    /// </param>
    public void SetThemeColor(
        ThemeColor color)
    {
        globalThemeColor =
            color;

        ApplyMasterTheme();
    }


    /// <summary>
    /// Applies the complete master theme.
    /// </summary>
    public void ApplyMasterTheme()
    {
        ThemeManager manager =
            ThemeManager.Instance;

        if (manager == null)
        {
            return;
        }

        // ----------------------------------------------------
        // Apply Main Theme Asset
        // ----------------------------------------------------

        if (mainTheme != null)
        {
            manager.SetTheme(
                mainTheme);
        }

        // ----------------------------------------------------
        // Apply Global Theme Color
        // ----------------------------------------------------

        manager.SetGlobalThemeColor(
            globalThemeColor);

        // ----------------------------------------------------
        // Refresh Registered UI
        // ----------------------------------------------------

        if (refreshAllUI)
        {
            manager.RefreshAllUI();
        }
    }

    #endregion


    #region Color Shortcuts

    /// <summary>
    /// Sets the global theme to White.
    /// </summary>
    public void SetWhite()
    {
        SetThemeColor(
            ThemeColor.White);
    }


    /// <summary>
    /// Sets the global theme to Red.
    /// </summary>
    public void SetRed()
    {
        SetThemeColor(
            ThemeColor.Red);
    }


    /// <summary>
    /// Sets the global theme to Blue.
    /// </summary>
    public void SetBlue()
    {
        SetThemeColor(
            ThemeColor.Blue);
    }


    /// <summary>
    /// Sets the global theme to Pink.
    /// </summary>
    public void SetPink()
    {
        SetThemeColor(
            ThemeColor.Pink);
    }


    /// <summary>
    /// Sets the global theme to Orange.
    /// </summary>
    public void SetOrange()
    {
        SetThemeColor(
            ThemeColor.Orange);
    }


    /// <summary>
    /// Sets the global theme to Green.
    /// </summary>
    public void SetGreen()
    {
        SetThemeColor(
            ThemeColor.Green);
    }

    #endregion


    #region Refresh

    /// <summary>
    /// Forces all registered Project Spark UI components
    /// to refresh using the current master theme.
    /// </summary>
    public void RefreshUI()
    {
        ThemeManager manager =
            ThemeManager.Instance;

        if (manager == null)
        {
            return;
        }

        manager.SetGlobalThemeColor(
            globalThemeColor);

        manager.RefreshAllUI();
    }

    #endregion


    #region Editor


#if UNITY_EDITOR


    /// <summary>
    /// Applies the master theme when a serialized value
    /// changes in the Unity Inspector.
    /// </summary>
    private void OnValidate()
    {
        if (Application.isPlaying)
        {
            return;
        }

        if (mainTheme == null)
        {
            return;
        }

        UnityEditor.EditorApplication.delayCall -=
            DelayedEditorApply;

        UnityEditor.EditorApplication.delayCall +=
            DelayedEditorApply;
    }


    /// <summary>
    /// Safely applies the master theme after Unity finishes
    /// processing serialized Inspector changes.
    /// </summary>
    private void DelayedEditorApply()
    {
        if (this == null)
        {
            return;
        }

        if (gameObject == null)
        {
            return;
        }

        if (mainTheme == null)
        {
            return;
        }

        ApplyMasterTheme();

        UnityEditor.EditorUtility.SetDirty(
            this);

        UnityEditor.SceneView.RepaintAll();
    }


#endif

    #endregion
}


}
