using UnityEngine;

namespace ProjectSpark.Gameplay.Electronics
{
    public sealed class CapacitorComponent : ElectronicComponent
    {
        [Header("Electrical")]

        [SerializeField]
        private float capacitance = 100e-6f;

        [SerializeField]
        private float maxVoltage = 16f;

        private float storedCharge;

        public float Capacitance => capacitance;

        public float Charge => storedCharge;

        public override void Simulate(float deltaTime)
        {
            if (State.IsBroken)
                return;

            storedCharge +=
                State.Current * deltaTime;

            storedCharge =
                Mathf.Clamp(
                    storedCharge,
                    0f,
                    capacitance * maxVoltage);

            State.Voltage =
                storedCharge /
                Mathf.Max(capacitance, 0.000001f);

            if (State.Voltage > maxVoltage)
            {
                State.IsBroken = true;
            }

            State.IsActive =
                storedCharge > 0.001f;
        }
    }
}
