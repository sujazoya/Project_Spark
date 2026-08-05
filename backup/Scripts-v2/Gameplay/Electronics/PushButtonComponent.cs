using UnityEngine;

namespace ProjectSpark.Gameplay.Electronics
{
    public sealed class PushButtonComponent : ElectronicComponent
    {
        public bool IsPressed { get; private set; }

        public void Press()
        {
            IsPressed = true;
        }

        public void Release()
        {
            IsPressed = false;
        }

        public override void Simulate(float deltaTime)
        {
            State.IsActive = IsPressed;
        }
    }
}
