using UnityEngine;
using UnityEngine.EventSystems;

namespace ProjectSpark.Gameplay.Input
{
    public sealed class InputManager : MonoBehaviour
    {
        public static InputManager Instance { get; private set; }

        public PointerState Pointer { get; private set; }

        public InputContext Context { get; private set; }
            = InputContext.Gameplay;

        private Vector2 _lastMouse;

        private void Awake()
        {
            Instance = this;
        }

        private void Update()
        {
            UpdatePointer();
        }

        private void UpdatePointer()
        {
            Vector2 mouse = UnityEngine.Input.mousePosition;

            Pointer = new PointerState
            {
                ScreenPosition = mouse,
                Delta = mouse - _lastMouse,
                Scroll = UnityEngine.Input.mouseScrollDelta,

                Pressed =
                    UnityEngine.Input.GetMouseButtonDown(0),

                Held =
                    UnityEngine.Input.GetMouseButton(0),

                Released =
                    UnityEngine.Input.GetMouseButtonUp(0),

                IsOverUI =
                    EventSystem.current != null &&
                    EventSystem.current.IsPointerOverGameObject()
            };

            _lastMouse = mouse;
        }

        public void SetContext(InputContext context)
        {
            Context = context;
        }
    }
}
