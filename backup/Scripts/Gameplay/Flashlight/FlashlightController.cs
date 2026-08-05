// ============================================================================
// Assets/My_Assets/_Project_Spark/Scripts/Gameplay/Flashlight/FlashlightController.cs
// ============================================================================

using UnityEngine;

namespace ProjectSpark.Gameplay.Flashlight
{
    public enum FlashlightState
    {
        Closed,
        Opened,
        BatteryRemoved,
        Repaired,
        Tested
    }

    public sealed class FlashlightController : MonoBehaviour
    {
        [SerializeField] CoverController cover;
        [SerializeField] BatteryCompartment battery;
        [SerializeField] PCBController pcb;

        public FlashlightState State { get; private set; }

        public void Open()
        {
            if (State != FlashlightState.Closed)
                return;

            cover.Open();

            State = FlashlightState.Opened;
        }

        public void RemoveBattery()
        {
            if (State != FlashlightState.Opened)
                return;

            battery.RemoveBattery();

            State = FlashlightState.BatteryRemoved;
        }

        public void RepairCompleted()
        {
            State = FlashlightState.Repaired;
        }

        public void Test()
        {
            if (State != FlashlightState.Repaired)
                return;

            pcb.PowerOn();

            State = FlashlightState.Tested;
        }
    }
}