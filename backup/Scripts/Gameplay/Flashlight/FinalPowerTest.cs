// ============================================================================
// FinalPowerTest.cs
// ============================================================================

using UnityEngine;

namespace ProjectSpark.Gameplay.Flashlight
{
    public sealed class FinalPowerTest : MonoBehaviour
    {
        [SerializeField] PlacementValidator placement;
        [SerializeField] RepairValidator repair;
        [SerializeField] LEDController led;
        [SerializeField] Animator flashlightAnimator;

        public void Test()
        {
            if (!placement.Completed)
                return;

            if (!repair.Validate())
                return;

            flashlightAnimator.SetTrigger("Power");

            led.TurnOn();

            Debug.Log("LEVEL 1 COMPLETE");
        }
    }
}