using UnityEngine;

namespace ProjectSpark.Gameplay.Electronics
{
    public sealed class LEDComponent : ElectronicComponent
    {
        [SerializeField]
        private Renderer ledRenderer;

        [SerializeField]
        private Material offMaterial;

        [SerializeField]
        private Material onMaterial;

        [SerializeField]
        private float forwardVoltage = 2.1f;

        public override void Simulate(float deltaTime)
        {
            bool on =
                State.Voltage >= forwardVoltage &&
                State.Current > 0.005f &&
                !State.IsBroken;

            State.IsActive = on;

            if (ledRenderer != null)
            {
                ledRenderer.sharedMaterial =
                    on ? onMaterial : offMaterial;
            }
        }
    }
}
