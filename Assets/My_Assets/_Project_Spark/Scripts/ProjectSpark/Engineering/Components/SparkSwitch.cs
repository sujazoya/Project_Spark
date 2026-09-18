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

        public float Resistance =>
            IsClosed
                ? closedResistance
                : openResistance;

        public float ClosedResistance =>
            closedResistance;

        public float OpenResistance =>
            openResistance;

        public SparkTerminal InputTerminal =>
            inputTerminal;

        public SparkTerminal OutputTerminal =>
            outputTerminal;



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
    inputTerminal.ElectricalState.IsPowered;

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
    outputTerminal.ElectricalState.IsPowered;

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
            {
                return;
            }

            state = newState;

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
        }
    }
}