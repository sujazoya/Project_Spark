using UnityEngine;

namespace AAAOutline
{
    /// <summary>
    /// Available visual states for an outline.
    /// Higher values have higher priority.
    /// </summary>
    public enum OutlineState
    {
        Normal = 0,
        Hover = 1,
        Selected = 2,
        Target = 3,
        Warning = 4
    }

    /// <summary>
    /// Controls how the outline depth test behaves.
    /// </summary>
    public enum OutlineVisibilityMode
    {
        /// <summary>
        /// Outline is visible only where the object itself is visible.
        /// </summary>
        Occluded = 0,

        /// <summary>
        /// Outline renders regardless of scene depth.
        /// </summary>
        AlwaysVisible = 1,

        /// <summary>
        /// Outline is visible through scene geometry with reduced alpha.
        /// </summary>
        ThroughWalls = 2
    }

    /// <summary>
    /// Determines where the controller gets its source renderers.
    /// </summary>
    public enum OutlineRendererSource
    {
        AutomaticChildren = 0,
        Manual = 1
    }
}