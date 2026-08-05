// ============================================================================
// ToolManager.cs
// ============================================================================

using UnityEngine;

namespace ProjectSpark.Gameplay.Flashlight
{
    public sealed class ToolManager : MonoBehaviour
    {
        public static ToolManager Instance { get; private set; }

        public ToolType CurrentTool { get; private set; }

        void Awake()
        {
            Instance = this;
        }

        public void SelectTool(ToolType tool)
        {
            CurrentTool = tool;
        }

        public bool IsSelected(ToolType tool)
        {
            return CurrentTool == tool;
        }
    }
}