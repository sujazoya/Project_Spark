
using System;
using UnityEngine;

namespace ProjectSpark.Gameplay
{
    [DisallowMultipleComponent]
    public sealed class SparkElectricalSequenceObjective
        : MonoBehaviour
    {
        public enum SequenceStep
        {
            WaitingForOpenCircuit,
            WaitingForClosedCircuit,
            Completed
        }

        [Header("Target")]
        [SerializeField]
        private SparkTerminal monitoredTerminal;

        [Header("Thresholds")]
        [SerializeField, Min(0f)]
        private float currentOffThreshold = 0.0001f;

        [SerializeField, Min(0f)]
        private float currentOnThreshold = 0.001f;

        [Header("Progress")]
        [SerializeField]
        private SequenceStep currentStep =
            SequenceStep.WaitingForOpenCircuit;

        [SerializeField]
        private bool requireInitialInteraction = true;

        private bool hasObservedElectricalChange;

        public SequenceStep CurrentStep => currentStep;

        public bool IsCompleted =>
            currentStep == SequenceStep.Completed;

        public event Action<SequenceStep> StepChanged;

        public event Action SequenceCompleted;

        private void OnEnable()
        {
            if (monitoredTerminal != null)
            {
                monitoredTerminal.ElectricalStateChanged +=
                    OnElectricalStateChanged;
            }
        }

        private void OnDisable()
        {
            if (monitoredTerminal != null)
            {
                monitoredTerminal.ElectricalStateChanged -=
                    OnElectricalStateChanged;
            }
        }

        public void ResetObjective()
        {
            currentStep =
                SequenceStep.WaitingForOpenCircuit;

            hasObservedElectricalChange = false;

            StepChanged?.Invoke(currentStep);
        }

        private void OnElectricalStateChanged(
            SparkTerminal terminal)
        {
            if (terminal == null ||
                currentStep == SequenceStep.Completed)
            {
                return;
            }

            hasObservedElectricalChange = true;

            float current =
                Mathf.Abs(terminal.Current);

            switch (currentStep)
            {
                case SequenceStep.WaitingForOpenCircuit:
                {
                    if (requireInitialInteraction &&
                        !hasObservedElectricalChange)
                    {
                        return;
                    }

                    if (current <= currentOffThreshold)
                    {
                        currentStep =
                            SequenceStep.WaitingForClosedCircuit;

                        StepChanged?.Invoke(currentStep);
                    }

                    break;
                }

                case SequenceStep.WaitingForClosedCircuit:
                {
                    if (current >= currentOnThreshold)
                    {
                        currentStep =
                            SequenceStep.Completed;

                        StepChanged?.Invoke(currentStep);
                        SequenceCompleted?.Invoke();
                    }

                    break;
                }
            }
        }
    }
}
