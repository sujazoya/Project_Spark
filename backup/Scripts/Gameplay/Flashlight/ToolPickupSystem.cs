// ============================================================================
// Assets/My_Assets/_Project_Spark/Scripts/Gameplay/Flashlight/ToolPickupSystem.cs
// ============================================================================

using UnityEngine;

namespace ProjectSpark.Gameplay.Flashlight
{
    public sealed class ToolPickupSystem : MonoBehaviour
    {
        [SerializeField] ToolType toolType;

        Renderer[] renderers;

        void Awake()
        {
            renderers = GetComponentsInChildren<Renderer>();
        }

        void OnMouseDown()
        {
            ToolManager.Instance.SelectTool(toolType);

            foreach (Renderer r in renderers)
                r.material.EnableKeyword("_EMISSION");
        }
    }
}