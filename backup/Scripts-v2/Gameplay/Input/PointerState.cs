using UnityEngine;

namespace ProjectSpark.Gameplay.Input
{
    public struct PointerState
    {
        public Vector2 ScreenPosition;

        public Vector2 Delta;

        public Vector2 Scroll;

        public bool Pressed;

        public bool Held;

        public bool Released;

        public bool IsOverUI;
    }
}
