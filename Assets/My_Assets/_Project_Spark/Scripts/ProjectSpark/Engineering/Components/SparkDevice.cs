
using UnityEngine;

namespace ProjectSpark.Gameplay
{
    [DisallowMultipleComponent]
    public sealed class SparkDevice :
        SparkElectricalComponent
    {
        #region Inspector

        [Header("Device Terminal")]
        [SerializeField]
        private SparkTerminal terminal;

        [Header("Power Detection")]
        [SerializeField, Min(0f)]
        private float voltageThreshold = 0.001f;

        [SerializeField, Min(0f)]
        private float currentThreshold = 0.001f;

        [Header("Runtime")]
        [SerializeField]
        private bool powered;

        #endregion


        #region Terminal

        public SparkTerminal Terminal =>
            terminal;

        public bool HasTerminal =>
            terminal != null;

        public bool IsConfigured =>
            terminal != null;

        #endregion


        #region Electrical State

        public float Voltage =>
            terminal != null
                ? terminal.ElectricalState.Voltage
                : 0f;

        public float Current =>
            terminal != null
                ? terminal.ElectricalState.Current
                : 0f;

        public float Power =>
            terminal != null
                ? terminal.ElectricalState.Power
                : 0f;

        public SparkTerminalPolarity Polarity =>
            terminal != null
                ? terminal.SolvedPolarity
                : SparkTerminalPolarity.None;

        #endregion


        #region Power State

        public bool IsPowered =>
            powered;

        public bool HasVoltage =>
            Mathf.Abs(Voltage) >=
            voltageThreshold;

        public bool HasCurrent =>
            Mathf.Abs(Current) >=
            currentThreshold;

        #endregion


        #region Electrical State Application

        public override void ApplyElectricalState(
            in SparkElectricalState state)
        {
            base.ApplyElectricalState(state);

            UpdatePowerState();
        }

        private void UpdatePowerState()
        {
            if (!ElectricalEnabled ||
                terminal == null)
            {
                powered = false;
                return;
            }

            powered =
                Mathf.Abs(
                    terminal.ElectricalState.Voltage)
                >= voltageThreshold;
        }

        #endregion


        #region Validation

        private void OnValidate()
        {
            voltageThreshold =
                Mathf.Max(
                    0f,
                    voltageThreshold);

            currentThreshold =
                Mathf.Max(
                    0f,
                    currentThreshold);
        }

        #endregion
    }
}
