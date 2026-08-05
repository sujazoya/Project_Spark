
using System;

namespace ProjectSpark.UI.VFX
{
    /// <summary>
    /// Defines all visual events supported by the
    /// Project Spark UI VFX system.
    ///
    /// Events are grouped into:
    ///
    /// 1. Base States
    /// 2. Pointer / Interaction
    /// 3. Feedback
    /// 4. System
    /// </summary>
    public enum SparkVFXEventType
    {
        // ============================================================
        // NONE
        // ============================================================

        None = 0,


        // ============================================================
        // BASE STATES
        // Persistent visual states.
        // ============================================================

        Normal = 1,

        HoverEnter = 2,

        HoverExit = 3,

        Selected = 4,

        Target = 5,

        Disabled = 6,

        Locked = 7,


        // ============================================================
        // INTERACTION
        // Temporary user interaction events.
        // ============================================================

        Press = 10,

        Release = 11,

        Submit = 12,


        // ============================================================
        // FEEDBACK
        // Temporary feedback events.
        // ============================================================

        Success = 20,

        Error = 21,

        Warning = 22,

        Confirm = 23,

        Cancel = 24,


        // ============================================================
        // SYSTEM
        // System-level visual events.
        // ============================================================

        Unlock = 30,

        LevelComplete = 31,

        Notification = 32,

        Alert = 33,

        Show = 34,

        Hide = 35
    }
}

