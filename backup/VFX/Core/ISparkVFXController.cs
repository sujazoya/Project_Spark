using UnityEngine;

namespace ProjectSpark.UI.VFX
{
    /// <summary>
    /// Common runtime contract for all Project Spark UI VFX controllers.
    ///
    /// Implemented by:
    /// - SparkVFXController
    /// - SparkTMPVFXController
    ///
    /// This allows runtime systems such as:
    /// - SparkVFXSequencePlayer
    /// - SparkVFXLoop
    /// - SparkVFXLayeredStateMachine
    /// - SparkVFXTarget
    ///
    /// to control Image and TMP VFX through the same interface.
    /// </summary>
    public interface ISparkVFXController
    {
        // ============================================================
        // INITIALIZATION
        // ============================================================

        /// <summary>
        /// Initializes the VFX controller and creates/
        /// assigns its runtime material.
        /// </summary>
        void Initialize();


        // ============================================================
        // THEME
        // ============================================================

        /// <summary>
        /// Applies a theme color to the VFX controller.
        /// </summary>
        void ApplyThemeColor(
            Color color
        );


        /// <summary>
        /// Returns the currently active theme color.
        /// </summary>
        Color CurrentThemeColor
        {
            get;
        }


        // ============================================================
        // PROFILE
        // ============================================================

        /// <summary>
        /// Applies a Spark VFX profile.
        /// </summary>
        void ApplyProfile(
            SparkVFXProfile profile,
            Color themeColor
        );


        /// <summary>
        /// Applies a Spark VFX profile.
        ///
        /// If instant is true, values are applied immediately.
        /// Otherwise the controller may animate toward the profile.
        /// </summary>
        void ApplyProfile(
            SparkVFXProfile profile,
            Color themeColor,
            bool instant
        );


        // ============================================================
        // GLOW
        // ============================================================

        void SetGlowValue(
            float value
        );


        // ============================================================
        // SCAN
        // ============================================================

        void SetScanValue(
            float value
        );


        // ============================================================
        // SWEEP
        // ============================================================

        void SetSweepValue(
            float value
        );


        // ============================================================
        // SWEEP POSITION
        // ============================================================

        void SetSweepPositionValue(
            float value
        );


        // ============================================================
        // FLASH
        // ============================================================

        void SetFlashValue(
            float value
        );


        // ============================================================
        // GLITCH
        // ============================================================

        void SetGlitchValue(
            float value
        );


        // ============================================================
        // FLICKER
        // ============================================================

        void SetFlickerValue(
            float value
        );


        // ============================================================
        // REVEAL
        // ============================================================

        void SetRevealValue(
            float value
        );


        // ============================================================
        // DISSOLVE
        // ============================================================

        void SetDissolveValue(
            float value
        );


        // ============================================================
        // RESET
        // ============================================================

        /// <summary>
        /// Resets all VFX values to their default state.
        /// </summary>
        void ResetVFX();
    }
}