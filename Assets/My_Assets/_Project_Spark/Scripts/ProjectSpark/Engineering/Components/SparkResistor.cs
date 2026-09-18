using UnityEngine;

namespace ProjectSpark.Gameplay
{
    [DisallowMultipleComponent]
    public sealed class SparkResistor :
        SparkElectricalComponent,
        ISparkConductiveDevice
    {
        [Header("Terminals")]
        [SerializeField]
        private SparkTerminal terminalA;

        [SerializeField]
        private SparkTerminal terminalB;

        [Header("Resistance")]
        [SerializeField, Min(0.000001f)]
        private float resistance = 1000f;

        [Header("Power Rating")]
        [SerializeField, Min(0f)]
        private float maximumPowerWatts = 0.25f;

        public SparkTerminal TerminalA => terminalA;
        public SparkTerminal TerminalB => terminalB;

        public float Resistance => resistance;

        public float ResistanceOhms =>
            Mathf.Max(resistance, 0.000001f);

        public float EffectiveResistance =>
            ResistanceOhms;

        public float Voltage =>
            ElectricalState.Voltage;

        public float Current =>
            ElectricalState.Current;

        public float Power =>
            ElectricalState.Power;

        public float MaximumPowerWatts =>
            maximumPowerWatts;

        public float PowerWatts =>
            ElectricalState.Power;

        public float PowerMarginWatts =>
            maximumPowerWatts - ElectricalState.Power;

        public bool IsOverPower =>
            ElectricalEnabled &&
            ElectricalState.Power > maximumPowerWatts;

        public bool IsConducting =>
            ElectricalEnabled &&
            ElectricalState.Conduction ==
            SparkConductionState.Conducting;

        /// <summary>
        /// Returns true when this resistor can electrically conduct
        /// between the supplied terminals.
        /// </summary>
        public bool CanConductBetween(
            SparkTerminal from,
            SparkTerminal to)
        {
            if (!ElectricalEnabled)
                return false;

            if (from == null || to == null)
                return false;

            if (terminalA == null || terminalB == null)
                return false;

            if (resistance <= 0f)
                return false;

            return
                (from == terminalA && to == terminalB) ||
                (from == terminalB && to == terminalA);
        }

        /// <summary>
        /// Applies the solved electrical state to the resistor
        /// and synchronizes both physical terminals.
        /// </summary>
        public override void ApplyElectricalState(
            in SparkElectricalState state)
        {
            base.ApplyElectricalState(state);

            if (!ElectricalEnabled)
            {
                ClearTerminalStates();
                return;
            }

            ApplyTerminalStates(state);
        }

        private void ApplyTerminalStates(
            in SparkElectricalState state)
        {
            if (terminalA != null)
            {
                terminalA.ApplyElectricalState(
                    new SparkTerminalElectricalState(
                        state.Voltage,
                        state.Current));
            }

            if (terminalB != null)
            {
                terminalB.ApplyElectricalState(
                    new SparkTerminalElectricalState(
                        state.Voltage,
                        state.Current));
            }
        }

        private void ClearTerminalStates()
        {
            if (terminalA != null)
                terminalA.ClearElectricalState();

            if (terminalB != null)
                terminalB.ClearElectricalState();
        }

        public void SetResistance(float value)
        {
            if (!IsFinitePositive(value))
                return;

            if (Mathf.Approximately(
                    resistance,
                    value))
            {
                return;
            }

            resistance = value;

            NotifyElectricalConfigurationChanged();
        }

        private void OnValidate()
        {
            resistance =
                Mathf.Max(
                    0.000001f,
                    resistance);

            maximumPowerWatts =
                Mathf.Max(
                    0f,
                    maximumPowerWatts);
        }

        private static bool IsFinitePositive(float value)
        {
            return
                !float.IsNaN(value) &&
                !float.IsInfinity(value) &&
                value > 0f;
        }
    }
}