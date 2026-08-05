namespace ProjectSpark.UI
{
    /// <summary>
    /// Defines a UI component that can receive theme updates.
    /// </summary>
    public interface IThemeObject
    {
        /// <summary>
        /// Applies the currently active theme to the UI object.
        /// </summary>
        void ApplyTheme();
    }
}