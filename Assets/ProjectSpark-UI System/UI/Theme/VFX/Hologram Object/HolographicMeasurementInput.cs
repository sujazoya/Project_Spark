using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace ProjectSpark.HolographicViewer
{
    public sealed class HolographicMeasurementInput
        : MonoBehaviour
    {
        [SerializeField]
        private HolographicMeasurementController measurement;

#if ENABLE_INPUT_SYSTEM

        private void Update()
        {
            if (Keyboard.current == null)
                return;

            if (Keyboard.current.mKey.wasPressedThisFrame)
            {
                measurement.Toggle();
            }

            if (Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                measurement.SetActive(false);
            }
        }

#endif
    }
}