
// ============================================================================
// Level01FlowController.cs
// ============================================================================

using UnityEngine;
using ProjectSpark.Gameplay.UI;

namespace ProjectSpark.Gameplay.Flashlight
{
    public sealed class Level01FlowController : MonoBehaviour
    {
        [SerializeField] FlashlightController flashlight;
        [SerializeField] ObjectiveController objectives;
        [SerializeField] FinalPowerTest powerTest;
       [SerializeField] SuccessPanel success;

        public void OpenFlashlight()
        {
            flashlight.Open();
            objectives.Next();
        }

        public void RemoveBattery()
        {
            flashlight.RemoveBattery();
            objectives.Next();
        }

        public void InstallResistor()
        {
            objectives.Next();
        }

        public void FinishSolder()
        {
            objectives.Next();
        }

        public void RunPowerTest()
        {
            powerTest.Test();
            success.Show();
        }
    }
}