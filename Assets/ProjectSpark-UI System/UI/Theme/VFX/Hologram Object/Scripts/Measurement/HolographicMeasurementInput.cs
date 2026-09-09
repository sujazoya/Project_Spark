using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace ProjectSpark.HolographicViewer
{
    [DisallowMultipleComponent]
    public sealed class HolographicMeasurementInput
        : MonoBehaviour
    {
        [SerializeField]
        private HolographicMeasurementController measurement;

#if ENABLE_INPUT_SYSTEM

        private void Update()
        {
            if (measurement == null ||
                Keyboard.current == null)
            {
                return;
            }

            Keyboard keyboard =
                Keyboard.current;

            if (keyboard.dKey.wasPressedThisFrame)
            {
                Activate(
                    HolographicMeasurementType.Distance
                );

                return;
            }

            if (keyboard.aKey.wasPressedThisFrame)
            {
                Activate(
                    HolographicMeasurementType.Angle
                );

                return;
            }

            if (keyboard.rKey.wasPressedThisFrame)
            {
                Activate(
                    HolographicMeasurementType.Radius
                );

                return;
            }

            if (keyboard.cKey.wasPressedThisFrame)
            {
                Activate(
                    HolographicMeasurementType.Diameter
                );

                return;
            }

            if (keyboard.escapeKey.wasPressedThisFrame)
            {
                measurement.SetActive(false);

                return;
            }

            if (keyboard.deleteKey.wasPressedThisFrame)
            {
                measurement.ClearMeasurement();

                return;
            }
        }

        private void Activate(
            HolographicMeasurementType type)
        {
            measurement.SetMeasurementType(
                type
            );

            measurement.SetActive(
                true
            );
        }

#endif
    }
}