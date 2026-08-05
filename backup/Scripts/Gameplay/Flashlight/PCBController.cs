// ============================================================================
// PCBController.cs
// ============================================================================

using UnityEngine;

namespace ProjectSpark.Gameplay.Flashlight
{
    public sealed class PCBController : MonoBehaviour
    {
        [SerializeField]
        ResistorController resistor;

        [SerializeField]
        LEDController led;

        public bool IsRepaired =>
            resistor.IsInstalled;

        public void PowerOn()
        {
            if(!IsRepaired)
                return;

            led.TurnOn();
        }
    }
}