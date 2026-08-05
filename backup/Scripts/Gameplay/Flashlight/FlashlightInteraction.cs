// ============================================================================
// FlashlightInteraction.cs
// ============================================================================

using UnityEngine;

namespace ProjectSpark.Gameplay.Flashlight
{
    public sealed class FlashlightInteraction : MonoBehaviour
    {
        [SerializeField]
        FlashlightController flashlight;

        void OnMouseDown()
        {
            if (ToolManager.Instance.IsSelected(ToolType.Screwdriver))
            {
                flashlight.Open();
            }
        }
    }
}