
using UnityEngine;
using ProjectSpark.Circuit;

namespace ProjectSpark.Gameplay
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SparkResistorBurn))]
    public sealed class SparkResistor :
        SparkElectricalComponent,
        ISparkConductiveDevice
    {
        [Header("Terminals")]
        [SerializeField] private SparkTerminal terminalA;
        [SerializeField] private SparkTerminal terminalB;

        [Header("Resistance")]
        [SerializeField, Min(0.000001f)]
        private float resistance = 1000f;

        [Header("Power Rating")]
        [SerializeField, Min(0f)]
        private float maximumPowerWatts = 0.25f;

        [Header("Burn System")]
        [SerializeField] private SparkResistorBurn burnSystem;
        [SerializeField] private SparkCircuitSystem circuitSystem;

        public SparkTerminal TerminalA => terminalA;
        public SparkTerminal TerminalB => terminalB;

        public float Resistance => resistance;

        public float ResistanceOhms =>
            Mathf.Max(resistance, 0.000001f);

        // Failed resistors behave as open circuits.
        // IMPORTANT: The solver must actually read this property
        // for the open-circuit behavior to take effect.
        public float EffectiveResistance =>
            IsBurned
                ? float.PositiveInfinity
                : ResistanceOhms;

        public float Voltage => ElectricalState.Voltage;
        public float Current => ElectricalState.Current;
        public float Power => ElectricalState.Power;

        public float MaximumPowerWatts =>
            maximumPowerWatts;

        public float PowerWatts =>
            ElectricalState.Power;

        public float PowerMarginWatts =>
            maximumPowerWatts - ElectricalState.Power;

        public SparkResistorBurn BurnSystem =>
            burnSystem;

        public bool IsBurned =>
            burnSystem != null &&
            burnSystem.IsFailed;

        public bool IsOverPower =>
            ElectricalEnabled &&
            !IsBurned &&
            ElectricalState.Power > maximumPowerWatts;

        public bool IsConducting =>
            ElectricalEnabled &&
            !IsBurned &&
            ElectricalState.Conduction ==
                SparkConductionState.Conducting;

        private void Awake()
        {
            ResolveReferences();
        }

        private void OnEnable()
        {
            ResolveReferences();

            if (burnSystem != null)
            {
                burnSystem.Failed -= HandleBurnFailure;
                burnSystem.Failed += HandleBurnFailure;
            }
        }

        private void OnDisable()
        {
            if (burnSystem != null)
            {
                burnSystem.Failed -= HandleBurnFailure;
                burnSystem.SetDeenergized();
            }
        }

        private void ResolveReferences()
        {
            if (burnSystem == null)
            {
                burnSystem =
                    GetComponent<SparkResistorBurn>();
            }

            if (circuitSystem == null)
            {
                circuitSystem =
                    FindFirstObjectByType<SparkCircuitSystem>();
            }
        }

        public bool CanConductBetween(
            SparkTerminal from,
            SparkTerminal to)
        {
            if (!ElectricalEnabled || IsBurned)
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

        public override void ApplyElectricalState(
            in SparkElectricalState state)
        {
            base.ApplyElectricalState(state);

            if (!ElectricalEnabled || IsBurned)
            {
                burnSystem?.SetDeenergized();
                ClearTerminalStates();
                return;
            }

            ApplyTerminalStates(state);

            if (burnSystem != null)
            {
                // Use the solver's calculated dissipated
                // power directly rather than recomputing
                // it from potentially signed V and I.
                burnSystem.SetDissipatedPower(
                    Mathf.Max(0f, state.Power));
            }
        }

        private void HandleBurnFailure(
            SparkResistorBurn failedBurn)
        {
            // Stop any further heating immediately.
            failedBurn.SetDeenergized();

            // Clear the old solved terminal display.
            ClearTerminalStates();

            Debug.LogWarning(
                "[RESISTOR BURN] " + name +
                " failed. Resistance=" +
                ResistanceOhms + " Ω. " +
                "Requesting electrical recalculation.",
                this);

            if (circuitSystem == null)
            {
                ResolveReferences();
            }

            if (circuitSystem != null)
            {
                // Request a new solve. Do not remove
                // the player's physical wires.
                circuitSystem.RequestElectricalReevaluation();
            }
            else
            {
                Debug.LogWarning(
                    "[RESISTOR BURN] Circuit system not found.",
                    this);
            }
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

            if (Mathf.Approximately(resistance, value))
                return;

            resistance = value;
            NotifyElectricalConfigurationChanged();
        }

        [ContextMenu("Burn / Repair Resistor")]
        public void RepairResistor()
        {
            if (burnSystem == null)
                return;

            burnSystem.ResetBurn();

            NotifyElectricalConfigurationChanged();

            if (circuitSystem == null)
                ResolveReferences();

            circuitSystem?.RequestElectricalReevaluation();
        }

        private void OnValidate()
        {
            resistance =
                Mathf.Max(0.000001f, resistance);

            maximumPowerWatts =
                Mathf.Max(0f, maximumPowerWatts);
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
