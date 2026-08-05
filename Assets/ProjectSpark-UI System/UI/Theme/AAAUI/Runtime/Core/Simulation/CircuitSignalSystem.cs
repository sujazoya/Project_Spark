using UnityEngine;

namespace AAAUI
{
    public enum CircuitSignalState
    {
        Off,
        Powered,
        Weak,
        Overload,
        ShortCircuit,
        Fault
    }

    [DisallowMultipleComponent]
    public sealed class CircuitSignalSystem : MonoBehaviour
    {
        [Header("Signal")]
        [SerializeField]
        private CircuitSignalState state =
            CircuitSignalState.Off;

        [SerializeField]
        private float voltage;

        [SerializeField]
        private float current;

        [SerializeField]
        private bool reversed;

        [Header("Visual")]
        [SerializeField]
        private SignalFlowVFX signalFlow;

        public CircuitSignalState State =>
            state;

        public float Voltage =>
            voltage;

        public float Current =>
            current;

        public bool Reversed =>
            reversed;

        private void Awake()
        {
            if (signalFlow == null)
            {
                signalFlow =
                    GetComponent<SignalFlowVFX>();
            }

            RefreshVisual();
        }

        public void SetSignal(
            float newVoltage,
            float newCurrent)
        {
            voltage =
                Mathf.Max(
                    0f,
                    newVoltage
                );

            current =
                Mathf.Max(
                    0f,
                    newCurrent
                );

            EvaluateState();

            RefreshVisual();
        }

        public void SetDirection(
            bool reverse)
        {
            reversed = reverse;

            if (signalFlow != null)
            {
                signalFlow.SetDirection(
                    reversed
                );
            }
        }

        public void SetState(
            CircuitSignalState newState)
        {
            state = newState;

            RefreshVisual();
        }

        private void EvaluateState()
        {
            if (voltage <= 0.001f)
            {
                state =
                    CircuitSignalState.Off;

                return;
            }

            if (current > 10f)
            {
                state =
                    CircuitSignalState.ShortCircuit;

                return;
            }

            if (current > 5f)
            {
                state =
                    CircuitSignalState.Overload;

                return;
            }

            if (voltage < 1f)
            {
                state =
                    CircuitSignalState.Weak;

                return;
            }

            state =
                CircuitSignalState.Powered;
        }

        private void RefreshVisual()
        {
            if (signalFlow == null)
                return;

            switch (state)
            {
                case CircuitSignalState.Off:

                    signalFlow.Stop();

                    break;

                case CircuitSignalState.Weak:

                    signalFlow.SetSpeed(0.35f);
                    signalFlow.SetIntensity(0.25f);
                    signalFlow.Play();

                    break;

                case CircuitSignalState.Powered:

                    signalFlow.SetSpeed(1f);
                    signalFlow.SetIntensity(1f);
                    signalFlow.Play();

                    break;

                case CircuitSignalState.Overload:

                    signalFlow.SetSpeed(2.5f);
                    signalFlow.SetIntensity(1.5f);
                    signalFlow.Play();

                    break;

                case CircuitSignalState.ShortCircuit:

                    signalFlow.SetSpeed(5f);
                    signalFlow.SetIntensity(2f);
                    signalFlow.Play();

                    break;

                case CircuitSignalState.Fault:

                    signalFlow.SetSpeed(0.15f);
                    signalFlow.SetIntensity(0.15f);
                    signalFlow.Play();

                    break;
            }

            signalFlow.SetDirection(
                reversed
            );
        }
    }
}