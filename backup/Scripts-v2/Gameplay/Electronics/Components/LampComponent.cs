using UnityEngine;

namespace ProjectSpark.Gameplay.Electronics
{
    /// <summary>
    /// Simple lamp component.
    /// The lamp is lit whenever the component is powered.
    /// </summary>
    public sealed class LampComponent : ElectronicComponent
    {
        public bool IsLit => State.IsPowered;

        public override void Initialize()
        {
            base.Initialize();
        }

        public override void Simulate(float deltaTime)
        {
            base.Simulate(deltaTime);

            // A lamp is lit whenever power is present.
            State.IsActive = State.IsPowered;
        }

        public override void ResetComponent()
        {
            base.ResetComponent();
            State.IsActive = false;
        }
    }
}