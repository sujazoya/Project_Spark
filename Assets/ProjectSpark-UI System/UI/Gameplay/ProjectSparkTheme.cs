using UnityEngine;

namespace ProjectSpark.UI.Theme
{
    [CreateAssetMenu(
        fileName = "ProjectSparkTheme",
        menuName = "ProjectSpark/UI/Theme")]
    public sealed class ProjectSparkTheme :
        ScriptableObject
    {
        [Header("Colors")]

        public UIThemeColors colors;

        [Header("Typography")]

        public UIThemeTypography typography;

        [Header("Spacing")]

        public UIThemeSpacing spacing;

        [Header("Shapes")]

        public UIThemeShapes shapes;

        [Header("Component Styles")]

        public UIThemeComponents components;
    }
}