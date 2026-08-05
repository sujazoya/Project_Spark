using UnityEngine;

namespace ProjectSpark.Gameplay.Tutorials
{
    [System.Serializable]
    public sealed class TutorialStep
    {
        [TextArea]
        public string Message;

        public string HighlightID;

        public bool WaitForPlayerAction = true;

        public float AutoAdvanceDelay = 0f;
    }
}
