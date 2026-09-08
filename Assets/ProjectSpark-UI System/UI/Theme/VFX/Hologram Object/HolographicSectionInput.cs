using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace ProjectSpark.HolographicViewer
{
    public sealed class HolographicSectionInput : MonoBehaviour
    {
        [SerializeField]
        private HolographicSectionController section;

        [SerializeField]
        private HolographicObjectVisualState visualState;

        [SerializeField]
        private float wheelSensitivity = 0.05f;

#if ENABLE_INPUT_SYSTEM

        private void Update()
        {
            if (Mouse.current == null)
                return;

            if (section == null ||
                visualState == null)
                return;

            if (visualState.GetMode() !=
                (int)HolographicInspectionMode.Section)
            {
                return;
            }

            if (!Keyboard.current.leftShiftKey.isPressed &&
                !Keyboard.current.rightShiftKey.isPressed)
            {
                return;
            }

            float wheel =
                Mouse.current.scroll.ReadValue().y;

            if (Mathf.Abs(wheel) < 0.01f)
                return;

            section.Move(
                wheel * wheelSensitivity
            );
        }

#endif
    }
}