using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace ProjectSpark.HolographicViewer
{
    public sealed class HolographicInspectionInput : MonoBehaviour
    {
        [SerializeField]
        private HolographicObjectVisualState visualState;

#if ENABLE_INPUT_SYSTEM
        private void Update()
        {
            if (Keyboard.current == null)
                return;

            if (Keyboard.current.digit1Key.wasPressedThisFrame)
                visualState.SetMode(0);

            if (Keyboard.current.digit2Key.wasPressedThisFrame)
                visualState.SetMode(1);

            if (Keyboard.current.digit3Key.wasPressedThisFrame)
                visualState.SetMode(2);

            if (Keyboard.current.digit4Key.wasPressedThisFrame)
                visualState.SetMode(3);

            if (Keyboard.current.digit5Key.wasPressedThisFrame)
                visualState.SetMode(4);
        }
#endif
    }
}