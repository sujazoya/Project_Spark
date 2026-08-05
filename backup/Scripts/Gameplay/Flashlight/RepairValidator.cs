// ============================================================================
// RepairValidator.cs
// ============================================================================

using UnityEngine;

namespace ProjectSpark.Gameplay.Flashlight
{
    public sealed class RepairValidator : MonoBehaviour
    {
        [SerializeField]
        FlashlightController flashlight;

        [SerializeField]
        PCBController pcb;

        [SerializeField]
        BatteryCompartment battery;

        [SerializeField]
        SolderingController solder;

        public bool Validate()
        {
            if(!pcb.IsRepaired)
                return false;

            if(!solder.Completed)
                return false;

            flashlight.RepairCompleted();

            return true;
        }
    }
}