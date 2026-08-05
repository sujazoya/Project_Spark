using UnityEngine;

namespace ProjectSpark.UI.Core
{
    /// <summary>
    /// Describes the current presentation context of the UI.
    /// This is UI state only. It does not represent gameplay state.
    /// </summary>
    public enum UIContext
    {
        None,
        Splash,
        MainMenu,
        LevelSelect,
        LevelBriefing,
        Gameplay,
        Pause,
        Results
    }

    /// <summary>
    /// Controls whether gameplay input should be considered blocked
    /// by the UI layer.
    /// </summary>
    public enum UIInputState
    {
        Gameplay,
        UIOnly
    }
}