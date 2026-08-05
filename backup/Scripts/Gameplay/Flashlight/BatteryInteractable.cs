// ============================================================================
// BatteryInteractable.cs
// ============================================================================

using UnityEngine;

namespace ProjectSpark.Gameplay.Flashlight
{
    public sealed class BatteryInteractable : Interactable
    {
        [SerializeField]
        BatteryCompartment compartment;

        public override void Interact()
        {
            if (!ToolManager.Instance.IsSelected(
                ToolType.Tweezers))
                return;

            compartment.RemoveBattery();
        }
    }
}