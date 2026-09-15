using System;
using UnityEngine;

namespace ProjectSpark.Gameplay
{
    public enum SparkConductionState { Unknown, NonConducting, Conducting }

    public readonly struct SparkElectricalState
    {
        public float Voltage { get; }
        public float Current { get; }
        public float Power { get; }
        public SparkConductionState Conduction { get; }
        public SparkElectricalState(float voltage, float current, float power, SparkConductionState conduction)
        { Voltage = voltage; Current = current; Power = power; Conduction = conduction; }
    }

    public interface ISparkElectricalDevice
    {
        string ElectricalModelId { get; }
        bool ElectricalEnabled { get; }
    }

    public interface ISparkElectricalStateReceiver
    {
        SparkElectricalState ElectricalState { get; }
        void ApplyElectricalState(in SparkElectricalState state);
    }

    public class SparkElectricalComponent : SparkComponent, ISparkElectricalDevice, ISparkElectricalStateReceiver
    {
        [SerializeField] private string electricalModelId = "generic";
        [SerializeField] private bool electricalEnabled = true;
        private SparkElectricalState electricalState;

        public string ElectricalModelId => electricalModelId;
        public bool ElectricalEnabled => electricalEnabled;
        public SparkElectricalState ElectricalState => electricalState;

        public event Action<SparkElectricalState> ElectricalStateChanged;
        /// <summary>Raised when an input/configuration that affects the solver changes.</summary>
        public event Action ElectricalConfigurationChanged;

        public void SetElectricalEnabled(bool enabled)
        {
            if (electricalEnabled == enabled) return;
            electricalEnabled = enabled;
            if (!enabled)
                ApplyElectricalState(new SparkElectricalState(0, 0, 0, SparkConductionState.NonConducting));
            NotifyElectricalConfigurationChanged();
        }

        /// <summary>Derived electrical components call this after changing solver inputs.</summary>
        protected void NotifyElectricalConfigurationChanged() => ElectricalConfigurationChanged?.Invoke();

        public virtual void ApplyElectricalState(in SparkElectricalState state)
        {
            if (!IsFinite(state.Voltage) || !IsFinite(state.Current) || !IsFinite(state.Power))
            {
                Debug.LogError($"Invalid electrical state received by {name}.", this);
                return;
            }
            electricalState = state;
            ElectricalStateChanged?.Invoke(state);
        }

        private static bool IsFinite(float v) => !float.IsNaN(v) && !float.IsInfinity(v);
    }
}
