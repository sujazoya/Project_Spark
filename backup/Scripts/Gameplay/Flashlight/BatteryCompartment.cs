// ============================================================================
// BatteryCompartment.cs
// ============================================================================

using UnityEngine;

namespace ProjectSpark.Gameplay.Flashlight
{
    public sealed class BatteryCompartment : MonoBehaviour
    {
        [SerializeField]
        Transform battery;

        [SerializeField]
        Transform removeTarget;

        bool removed;

        public void RemoveBattery()
        {
            if (removed)
                return;

            removed=true;

            battery.position=
                removeTarget.position;
        }

        public void InsertBattery()
        {
            removed=false;
        }
    }
}