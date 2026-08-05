namespace ProjectSpark.UI.VFX
{
    /// <summary>
    /// Defines the primary visual effects available
    /// in the Project Spark UI VFX system.
    /// </summary>
    public enum SparkVFXMode
    {
        None = 0,

        /// <summary>
        /// Used when a UI element appears.
        /// </summary>
        Reveal = 1,

        /// <summary>
        /// Used when the pointer hovers over an interactive element.
        /// </summary>
        Hover = 2,

        /// <summary>
        /// Used when an element is selected.
        /// </summary>
        Selected = 3,

        /// <summary>
        /// Used for clicks, confirmations and activations.
        /// </summary>
        Activate = 4,

        /// <summary>
        /// Used for warnings and error states.
        /// </summary>
        Warning = 5,

        /// <summary>
        /// Used for target or lock-on states.
        /// </summary>
        Target = 6,

        /// <summary>
        /// Used when an element disappears.
        /// </summary>
        Destroy = 7
    }
}