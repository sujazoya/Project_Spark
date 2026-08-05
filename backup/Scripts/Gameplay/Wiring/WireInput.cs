// Assets/My_Assets/_Project_Spark/Scripts/Gameplay/Wiring/WireInput.cs

using UnityEngine;
using UnityEngine.InputSystem;

namespace ProjectSpark.Gameplay.Wiring
{
    public sealed class WireInput : MonoBehaviour
    {
        public static WireInput Instance { get; private set; }

        public UnityEngine.Camera Camera;

        public Vector2 MousePosition =>
            Mouse.current.position.ReadValue();

        public bool ClickDown =>
            Mouse.current.leftButton.wasPressedThisFrame;

        public bool ClickHeld =>
            Mouse.current.leftButton.isPressed;

        public bool ClickUp =>
            Mouse.current.leftButton.wasReleasedThisFrame;

        private void Awake()
        {
            Instance = this;

            if (Camera == null)
                Camera = UnityEngine.Camera.main;
        }
    }
}