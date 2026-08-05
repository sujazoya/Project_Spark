using ProjectSpark.UI;
using UnityEngine;

/// <summary>
/// Changes the Project Spark global UI theme color.
/// </summary>
public sealed class ThemeChanger : MonoBehaviour
{
    #region Inspector

    [Header("Master Theme Controller")]

    [SerializeField]
    private MasterThemeController controller;

    [Header("Theme Color")]

    [SerializeField]
    private ThemeColor themeColor = ThemeColor.Blue;

    #endregion


    #region Unity

    private void Start()
    {
        if (controller == null)
        {
            controller =
                MasterThemeController.Instance;
        }
    }

    #endregion


    #region Public API

    /// <summary>
    /// Changes the global Project Spark UI theme color.
    /// </summary>
    public void ChangeTheme()
    {
        if (controller == null)
        {
            controller =
                MasterThemeController.Instance;
        }

        if (controller == null)
        {
            Debug.LogError(
                "ThemeChanger: MasterThemeController was not found.",
                this);

            return;
        }

        controller.SetThemeColor(
            themeColor);
    }

    /// <summary>
    /// Changes the global theme to the specified color.
    /// </summary>
    /// <param name="color">
    /// The theme color to apply.
    /// </param>
    public void ChangeTheme(
        ThemeColor color)
    {
        if (controller == null)
        {
            controller =
                MasterThemeController.Instance;
        }

        if (controller == null)
        {
            Debug.LogError(
                "ThemeChanger: MasterThemeController was not found.",
                this);

            return;
        }

        controller.SetThemeColor(
            color);
    }


    #endregion

}