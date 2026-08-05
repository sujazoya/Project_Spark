// ============================================================================
// BatteryInteraction.cs
// ============================================================================

using UnityEngine;

namespace ProjectSpark.Gameplay.Flashlight
{
    public sealed class BatteryInteraction : MonoBehaviour
    {
        [SerializeField]
        BatteryCompartment battery;

        void OnMouseDown()
        {
            if (!ToolManager.Instance.IsSelected(ToolType.Tweezers))
                return;

            battery.RemoveBattery();
        }
    }
}