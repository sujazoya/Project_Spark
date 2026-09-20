using UnityEngine;

namespace ProjectSpark.Gameplay
{
    public enum SparkSwitchState
    {
        Open,
        Closed
    }

    [DisallowMultipleComponent]
    public sealed class SparkSwitch :
        SparkElectricalComponent,
        ISparkConductiveDevice
    {
        #region Inspector

        [Header("Switch")]
        [SerializeField]
        private SparkSwitchState state =
            SparkSwitchState.Open;

        [Header("Terminals")]
        [SerializeField]
        private SparkTerminal inputTerminal;

        [SerializeField]
        private SparkTerminal outputTerminal;

        [Header("Electrical")]
        [SerializeField, Min(0f)]
        private float closedResistance = 0.01f;

        [SerializeField, Min(0f)]
        private float openResistance = 1000000000f;

        [SerializeField, Min(0f)]
        private float voltageThreshold = 0.001f;

        [SerializeField, Min(0f)]
        private float currentThreshold = 0.001f;

        [Header("Runtime Polarity")]
        [SerializeField, Min(0.000001f)]
        private float runtimePolarityThreshold = 0.001f;
        public void RefreshRuntimePolarity()
{
    UpdateRuntimeTerminalPolarity();
}

        #endregion


        #region Basic Switch State

        public SparkSwitchState State =>
            state;

        public bool IsClosed =>
            state == SparkSwitchState.Closed;

        public bool IsOpen =>
            state == SparkSwitchState.Open;

        public bool IsConducting =>
            IsClosed &&
            ElectricalEnabled;

        public bool IsConfiguredToConduct =>
            IsClosed &&
            ElectricalEnabled;

        public bool IsSolverConducting =>
            ElectricalState.Conduction ==
            SparkConductionState.Conducting;

        public bool IsActuallyConducting =>
            IsConfiguredToConduct &&
            IsSolverConducting;

        #endregion


        #region Resistance

        public float Resistance =>
            IsClosed
                ? closedResistance
                : openResistance;

        public float ClosedResistance =>
            closedResistance;

        public float OpenResistance =>
            openResistance;

        #endregion


        #region Terminals

        public SparkTerminal InputTerminal =>
            inputTerminal;

        public SparkTerminal OutputTerminal =>
            outputTerminal;

        #endregion


        #region Input Electrical State

        public float InputVoltage =>
            inputTerminal != null
                ? inputTerminal.ElectricalState.Voltage
                : 0f;

        public float InputCurrent =>
            inputTerminal != null
                ? inputTerminal.ElectricalState.Current
                : 0f;

        public float InputPower =>
            inputTerminal != null
                ? inputTerminal.ElectricalState.Power
                : 0f;

        public bool InputHasPower =>
            inputTerminal != null &&
            Mathf.Abs(InputVoltage) >=
            voltageThreshold;

        #endregion


        #region Output Electrical State

        public float OutputVoltage =>
            outputTerminal != null
                ? outputTerminal.ElectricalState.Voltage
                : 0f;

        public float OutputCurrent =>
            outputTerminal != null
                ? outputTerminal.ElectricalState.Current
                : 0f;

        public float OutputPower =>
            outputTerminal != null
                ? outputTerminal.ElectricalState.Power
                : 0f;

        public bool OutputHasPower =>
            outputTerminal != null &&
            Mathf.Abs(OutputVoltage) >=
            voltageThreshold;

        #endregion


        #region Runtime Polarity

        public SparkTerminalPolarity InputPolarity =>
            GetRuntimePolarity(InputVoltage);

        public SparkTerminalPolarity OutputPolarity =>
            GetRuntimePolarity(OutputVoltage);

        public SparkTerminalPolarity RuntimePolarity
        {
            get
            {
                if (!IsClosed ||
                    !ElectricalEnabled)
                {
                    return SparkTerminalPolarity.None;
                }

                SparkTerminalPolarity input =
                    InputPolarity;

                if (input !=
                    SparkTerminalPolarity.None)
                {
                    return input;
                }

                return OutputPolarity;
            }
        }

        private SparkTerminalPolarity GetRuntimePolarity(
            float voltage)
        {
            if (voltage >
                runtimePolarityThreshold)
            {
                return SparkTerminalPolarity.Positive;
            }

            if (voltage <
                -runtimePolarityThreshold)
            {
                return SparkTerminalPolarity.Negative;
            }

            return SparkTerminalPolarity.None;
        }

       private void UpdateRuntimeTerminalPolarity()
        {
            
            if (InputTerminal == null ||
        OutputTerminal == null)
    {
        return;
    }

    // Open or disabled switch cannot propagate
    // runtime polarity to the output.
    if (!IsClosed || !ElectricalEnabled)
    {
        OutputTerminal.ClearRuntimePolarity();
        return;
    }

    SparkTerminalPolarity inputPolarity =
        InputTerminal.SolvedPolarity;

    if (inputPolarity == SparkTerminalPolarity.None)
    {
        OutputTerminal.ClearRuntimePolarity();
        return;
    }

    // Propagate BOTH polarities.
    OutputTerminal.SetRuntimePolarity(
        inputPolarity);
        }

        #endregion


        #region Connection State

        public bool HasInputConnection =>
            inputTerminal != null &&
            inputTerminal.ConnectionCount > 0;

        public bool HasOutputConnection =>
            outputTerminal != null &&
            outputTerminal.ConnectionCount > 0;

        public bool HasBothConnections =>
            HasInputConnection &&
            HasOutputConnection;

        public int InputConnectionCount =>
            inputTerminal != null
                ? inputTerminal.ConnectionCount
                : 0;

        public int OutputConnectionCount =>
            outputTerminal != null
                ? outputTerminal.ConnectionCount
                : 0;

        #endregion


        #region Component Electrical State

        public float Voltage =>
            ElectricalState.Voltage;

        public float Current =>
            ElectricalState.Current;

        public float Power =>
            ElectricalState.Power;

        public SparkConductionState Conduction =>
            ElectricalState.Conduction;

        public bool HasVoltage =>
            Mathf.Abs(Voltage) >=
            voltageThreshold;

        public bool HasCurrent =>
            Mathf.Abs(Current) >=
            currentThreshold;

        public bool IsElectricallyConducting =>
            Conduction ==
            SparkConductionState.Conducting;

        public bool IsClosedAndConducting =>
            IsClosed &&
            IsElectricallyConducting;

        public bool HasPowerFlow =>
            IsClosedAndConducting &&
            HasCurrent;

        public bool IsPoweredAndClosed =>
            IsClosed &&
            (
                HasVoltage ||
                HasCurrent ||
                Mathf.Abs(Power) >=
                voltageThreshold *
                currentThreshold
            );

        public bool IsReadyForOutput =>
            IsClosedAndConducting;

        public bool CanProvideOutput =>
            ElectricalEnabled &&
            IsClosed;

        public bool HasReadyOutput =>
            HasOutputConnection &&
            IsReadyForOutput;

        #endregion


        #region Conduction

        public bool CanConductBetween(
            SparkTerminal from,
            SparkTerminal to)
        {
            if (!ElectricalEnabled ||
                !IsClosed)
            {
                return false;
            }

            if (from == null ||
                to == null)
            {
                return false;
            }

            if (inputTerminal == null ||
                outputTerminal == null)
            {
                return false;
            }

            return
                (from == inputTerminal &&
                 to == outputTerminal) ||

                (from == outputTerminal &&
                 to == inputTerminal);
        }

        #endregion


        #region Electrical Solver Integration

        /// <summary>
        /// Applies the electrical state calculated
        /// by the solver.
        ///
        /// The base class stores the authoritative
        /// electrical state.
        ///
        /// Runtime polarity is then synchronized
        /// from the solved terminal voltages.
        /// </summary>
        public override void ApplyElectricalState(
    in SparkElectricalState state)
{
    base.ApplyElectricalState(state);
}



        #endregion


        #region Switch Control

        public void ToggleState()
        {
            SetState(
                state ==
                SparkSwitchState.Open
                    ? SparkSwitchState.Closed
                    : SparkSwitchState.Open);
        }

        public void SetState(
    SparkSwitchState newState)
{
    if (state == newState)
        return;

    state = newState;

    /*
     * Immediately update visual/runtime polarity.
     */
    UpdateRuntimeTerminalPolarity();

    /*
     * Tell the electrical system to solve again.
     */
    NotifyElectricalConfigurationChanged();
}

        public void ToggleFromButton()
        {
            ToggleState();
        }

        public void SetSwitchFromToggle(
            bool closed)
        {
            SetState(
                closed
                    ? SparkSwitchState.Closed
                    : SparkSwitchState.Open);
        }

        public SparkResult Toggle(
            in SparkInteractionContext context)
        {
            if (!CanInteract(
                    context,
                    out string reason))
            {
                return SparkResult.Rejected(reason);
            }

            ToggleState();

            return SparkResult.Success();
        }

        public override SparkResult BeginInteraction(
            in SparkInteractionContext context)
        {
            if (context.InteractionType ==
                SparkInteractionType.Toggle)
            {
                return Toggle(context);
            }

            return base.BeginInteraction(context);
        }

        #endregion


        #region Validation

        private void OnValidate()
        {
            closedResistance =
                Mathf.Max(
                    0f,
                    closedResistance);

            openResistance =
                Mathf.Max(
                    closedResistance,
                    openResistance);

            voltageThreshold =
                Mathf.Max(
                    0f,
                    voltageThreshold);

            currentThreshold =
                Mathf.Max(
                    0f,
                    currentThreshold);

            runtimePolarityThreshold =
                Mathf.Max(
                    0.000001f,
                    runtimePolarityThreshold);
        }

        #endregion
    }
}
