using UnityEngine;

namespace ProjectSpark.UI
{
    /// <summary>
    /// Defines the neon visual configuration associated with
    /// one Project Spark UI theme color.
    /// </summary>
    [System.Serializable]
    public sealed class UINeonThemeData
    {
        #region Colors

        [Header("Base")]

        /// <summary>
        /// Base UI color.
        /// </summary>
        [ColorUsage(true, true)]
        public Color baseColor = Color.white;

        [Header("Gradient")]

        /// <summary>
        /// First gradient color.
        /// </summary>
        [ColorUsage(true, true)]
        public Color gradientColorA = Color.white;

        /// <summary>
        /// Second gradient color.
        /// </summary>
        [ColorUsage(true, true)]
        public Color gradientColorB = Color.white;

        /// <summary>
        /// Third gradient color.
        /// </summary>
        [ColorUsage(true, true)]
        public Color gradientColorC = Color.white;

        [Header("Outline")]

        /// <summary>
        /// Outline color.
        /// </summary>
        [ColorUsage(true, true)]
        public Color outlineColor = Color.white;

        [Header("Emission")]

        /// <summary>
        /// Emission color.
        /// </summary>
        [ColorUsage(true, true)]
        public Color emissionColor = Color.white;

        [Header("Glow")]

        /// <summary>
        /// Glow color.
        /// </summary>
        [ColorUsage(true, true)]
        public Color glowColor = Color.white;

        #endregion

        #region Gradient

        [Header("Gradient Settings")]

        /// <summary>
        /// Gradient direction.
        /// </summary>
        public UI_Advanced_Neon_Controller.GradientDirection direction =
            UI_Advanced_Neon_Controller.GradientDirection.Horizontal;

        /// <summary>
        /// Gradient animation speed.
        /// </summary>
        public float animationSpeed = 0.25f;

        /// <summary>
        /// Gradient offset.
        /// </summary>
        [Range(0f, 1f)]
        public float gradientOffset = 0f;

        /// <summary>
        /// Horizontal gradient scale.
        /// </summary>
        public float gradientScaleX = 1f;

        /// <summary>
        /// Vertical gradient scale.
        /// </summary>
        public float gradientScaleY = 1f;

        #endregion

        #region Outline

        [Header("Outline Settings")]

        /// <summary>
        /// Outline width.
        /// </summary>
        [Range(0f, 0.1f)]
        public float outlineWidth = 0.01f;

        #endregion

        #region Emission

        [Header("Emission Settings")]

        /// <summary>
        /// Emission intensity.
        /// </summary>
        [Range(0f, 20f)]
        public float emissionIntensity = 2f;

        #endregion

        #region Glow

        [Header("Glow Settings")]

        /// <summary>
        /// Glow strength.
        /// </summary>
        [Range(0f, 10f)]
        public float glowStrength = 1.5f;

        /// <summary>
        /// Glow size.
        /// </summary>
        [Range(0f, 0.25f)]
        public float glowSize = 0.05f;

        /// <summary>
        /// Glow softness.
        /// </summary>
        [Range(0.001f, 0.25f)]
        public float glowSoftness = 0.05f;

        #endregion


    }
}