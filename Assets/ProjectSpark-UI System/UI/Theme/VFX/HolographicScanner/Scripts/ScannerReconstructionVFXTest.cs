using UnityEngine;
using UnityEngine.InputSystem;

namespace ProjectSpark.Scanner
{
    [DisallowMultipleComponent]
    public sealed class ScannerReconstructionVFXTest : MonoBehaviour
    {
        [Header("Controller")]
        [SerializeField]
        private ScannerReconstructionVFXController controller;

        [Header("Test")]
        [SerializeField, Min(0.01f)]
        private float testSpeed = 0.4f;

        [SerializeField]
        private bool holdVToScan = true;

        private float progress;
        private bool scanning;

        private void Update()
        {
            if (controller == null)
                return;

            Keyboard keyboard =
                Keyboard.current;

            if (keyboard == null)
                return;

            HandleStart(
                keyboard);

            HandleScan(
                keyboard);

            HandleComplete(
                keyboard);

            HandleReset(
                keyboard);
        }

        private void HandleStart(
            Keyboard keyboard)
        {
            if (!keyboard.vKey.wasPressedThisFrame)
                return;

            progress = 0f;
            scanning = true;

            controller.StartReconstruction();
        }

        private void HandleScan(
            Keyboard keyboard)
        {
            if (!scanning)
                return;

            if (holdVToScan &&
                !keyboard.vKey.isPressed)
            {
                return;
            }

            progress +=
                testSpeed *
                Time.deltaTime;

            progress =
                Mathf.Clamp01(
                    progress);

            controller.SetProgress(
                progress);

            if (progress >= 1f)
            {
                progress = 1f;
                scanning = false;

                controller.CompleteReconstruction();
            }
        }

        private void HandleComplete(
            Keyboard keyboard)
        {
            if (!keyboard.bKey.wasPressedThisFrame)
                return;

            scanning = false;
            progress = 1f;

            controller.CompleteReconstruction();
        }

        private void HandleReset(
            Keyboard keyboard)
        {
            if (!keyboard.nKey.wasPressedThisFrame)
                return;

            scanning = false;
            progress = 0f;

            controller.ResetVFX();
        }
    }
}