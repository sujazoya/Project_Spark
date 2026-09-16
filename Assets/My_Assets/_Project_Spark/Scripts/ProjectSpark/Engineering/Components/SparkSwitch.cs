using UnityEngine;

namespace ProjectSpark.Gameplay
{
    public enum SparkSwitchState
    {
        Open,
        Closed
    }

    [DisallowMultipleComponent]
    public sealed class SparkSwitch : SparkElectricalComponent
    {
        // =========================================================
        // CONFIGURATION
        // =========================================================

        [Header("Switch")]
        [SerializeField]
        private SparkSwitchState state =
            SparkSwitchState.Open;

        [Header("Electrical")]
        [SerializeField, Min(0f)]
        private float closedResistance = 0.01f;

        [SerializeField, Min(0f)]
        private float openResistance = 1000000000f;

        [SerializeField, Min(0f)]
        private float voltageThreshold = 0.001f;

        [SerializeField, Min(0f)]
        private float currentThreshold = 0.001f;


        // =========================================================
        // STATE
        // =========================================================

        public SparkSwitchState State =>
            state;

        /// <summary>
        /// True when the physical switch is closed.
        /// </summary>
        public bool IsClosed =>
            state == SparkSwitchState.Closed;

        /// <summary>
        /// True when the physical switch is open.
        /// </summary>
        public bool IsOpen =>
            state == SparkSwitchState.Open;

        /// <summary>
        /// True when the switch should electrically conduct.
        /// </summary>
        public bool IsConducting =>
            IsClosed && ElectricalEnabled;


        // =========================================================
        // RESISTANCE
        // =========================================================

        /// <summary>
        /// Resistance presented by the switch according to
        /// its current physical state.
        /// </summary>
        public float Resistance =>
            IsClosed
                ? closedResistance
                : openResistance;

        public float ClosedResistance =>
            closedResistance;

        public float OpenResistance =>
            openResistance;


        // =========================================================
        // TERMINALS
        // =========================================================

        /// <summary>
        /// Terminal 0.
        /// </summary>
        public SparkTerminal InputTerminal =>
            GetTerminal(0);

        /// <summary>
        /// Terminal 1.
        /// </summary>
        public SparkTerminal OutputTerminal =>
            GetTerminal(1);


        // =========================================================
        // CONNECTION STATUS
        // =========================================================

        public bool HasInputConnection =>
            InputTerminal != null &&
            InputTerminal.ConnectionCount > 0;

        public bool HasOutputConnection =>
            OutputTerminal != null &&
            OutputTerminal.ConnectionCount > 0;

        public bool HasBothConnections =>
            HasInputConnection &&
            HasOutputConnection;


        // =========================================================
        // CONNECTION COUNTS
        // =========================================================

        public int InputConnectionCount =>
            InputTerminal != null
                ? InputTerminal.ConnectionCount
                : 0;

        public int OutputConnectionCount =>
            OutputTerminal != null
                ? OutputTerminal.ConnectionCount
                : 0;


        // =========================================================
        // ELECTRICAL STATE
        // =========================================================

        /// <summary>
        /// Voltage reported by the electrical solver.
        /// </summary>
        public float Voltage =>
            ElectricalState.Voltage;

        /// <summary>
        /// Current reported by the electrical solver.
        /// </summary>
        public float Current =>
            ElectricalState.Current;

        /// <summary>
        /// Power reported by the electrical solver.
        /// </summary>
        public float Power =>
            ElectricalState.Power;

        /// <summary>
        /// Conduction state reported by the electrical solver.
        /// </summary>
        public SparkConductionState Conduction =>
            ElectricalState.Conduction;


        // =========================================================
        // ELECTRICAL STATUS
        // =========================================================

        /// <summary>
        /// True when the solver reports a meaningful voltage
        /// across the switch.
        /// </summary>
        public bool HasVoltage =>
            Mathf.Abs(Voltage) >= voltageThreshold;

        /// <summary>
        /// True when the solver reports meaningful current
        /// through the switch.
        /// </summary>
        public bool HasCurrent =>
            Mathf.Abs(Current) >= currentThreshold;

        /// <summary>
        /// True when the solver reports actual electrical
        /// conduction through the switch.
        /// </summary>
        public bool IsElectricallyConducting =>
            Conduction ==
            SparkConductionState.Conducting;

        /// <summary>
        /// True when the switch is physically closed and the
        /// solver confirms electrical conduction.
        /// </summary>
        public bool IsClosedAndConducting =>
            IsClosed &&
            IsElectricallyConducting;

        /// <summary>
        /// True when the switch is closed and electrical current
        /// is actually flowing.
        /// </summary>
        public bool HasPowerFlow =>
            IsClosed &&
            IsElectricallyConducting &&
            HasCurrent;

        /// <summary>
        /// True when the switch has an electrical source condition
        /// and is closed.
        ///
        /// This does not mean that a load is necessarily connected.
        /// </summary>
        public bool IsPoweredAndClosed =>
            IsClosed &&
            (
                HasVoltage ||
                HasCurrent ||
                Mathf.Abs(Power) >=
                voltageThreshold * currentThreshold
            );

        /// <summary>
        /// Indicates that the switch is physically closed and
        /// the solver has confirmed that it is conducting.
        ///
        /// This is the safe state for downstream systems to use
        /// as "switch ready for output".
        /// </summary>
        public bool IsReadyForOutput =>
            IsClosedAndConducting;


        // =========================================================
        // ELECTRICAL OUTPUT
        // =========================================================

        /// <summary>
        /// True when the switch is closed and capable of passing
        /// electrical current according to its configuration.
        ///
        /// This is a configuration/state property and does not
        /// replace the solver's actual conduction result.
        /// </summary>
        public bool CanProvideOutput =>
            ElectricalEnabled &&
            IsClosed;

        /// <summary>
        /// True when an output connection exists and the switch
        /// is electrically ready.
        /// </summary>
        public bool HasReadyOutput =>
            HasOutputConnection &&
            IsReadyForOutput;


        // =========================================================
        // TERMINAL LOOKUP
        // =========================================================

        private SparkTerminal GetTerminal(int index)
        {
            SparkTerminal[] terminals =
                GetComponentsInChildren<SparkTerminal>(true);

            if (terminals == null ||
                terminals.Length <= index)
            {
                return null;
            }

            return terminals[index];
        }


        // =========================================================
        // INTERACTION
        // =========================================================

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


        // =========================================================
        // TOGGLE
        // =========================================================

        /// <summary>
        /// Toggles between Open and Closed.
        /// </summary>
        public void ToggleState()
        {
            SetState(
                state == SparkSwitchState.Open
                    ? SparkSwitchState.Closed
                    : SparkSwitchState.Open
            );
        }


        // =========================================================
        // SET STATE
        // =========================================================

        /// <summary>
        /// Sets the physical switch state.
        /// </summary>
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


        // =========================================================
        // UI BUTTON
        // =========================================================

        /// <summary>
        /// Assign directly to a Unity UI Button OnClick event.
        /// </summary>
        public void ToggleFromButton()
        {
            ToggleState();
        }


        // =========================================================
        // UI TOGGLE
        // =========================================================

        /// <summary>
        /// Assign to Unity UI Toggle OnValueChanged.
        ///
        /// true  = Closed
        /// false = Open
        /// </summary>
        public void SetSwitchFromToggle(
            bool closed)
        {
            SetState(
                closed
                    ? SparkSwitchState.Closed
                    : SparkSwitchState.Open
            );
        }


        // =========================================================
        // INTERACTION SYSTEM
        // =========================================================

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


        // =========================================================
        // DEBUG
        // =========================================================

        /// <summary>
        /// Prints complete switch state and electrical
        /// information to the Unity Console.
        /// </summary>
        public void DebugConnections()
        {
            SparkTerminal input =
                InputTerminal;

            SparkTerminal output =
                OutputTerminal;

            string inputName =
                input != null
                    ? input.name
                    : "NULL";

            string outputName =
                output != null
                    ? output.name
                    : "NULL";

            int inputCount =
                input != null
                    ? input.ConnectionCount
                    : 0;

            int outputCount =
                output != null
                    ? output.ConnectionCount
                    : 0;

            Debug.Log(
                "\n" +
                "========================================\n" +
                $"[SPARK SWITCH] {name}\n" +
                "========================================\n" +
                $"State                 : {State}\n" +
                $"Open                  : {IsOpen}\n" +
                $"Closed                : {IsClosed}\n" +
                $"Conducting            : {IsConducting}\n" +
                $"Electrical Conducting : {IsElectricallyConducting}\n" +
                $"Closed + Conducting   : {IsClosedAndConducting}\n" +
                $"Powered + Closed      : {IsPoweredAndClosed}\n" +
                $"Power Flow            : {HasPowerFlow}\n" +
                $"Ready For Output      : {IsReadyForOutput}\n" +
                $"Ready Output          : {HasReadyOutput}\n" +
                "\n" +
                $"Voltage               : {Voltage:F4} V\n" +
                $"Current               : {Current:F4} A\n" +
                $"Power                 : {Power:F4} W\n" +
                $"Conduction State      : {Conduction}\n" +
                $"Resistance            : {Resistance:F4} Ohm\n" +
                "\n" +
                $"Input Terminal        : {inputName}\n" +
                $"Input Connections     : {inputCount}\n" +
                "\n" +
                $"Output Terminal       : {outputName}\n" +
                $"Output Connections    : {outputCount}\n" +
                "\n" +
                $"Both Connected        : {HasBothConnections}\n" +
                "========================================",
                this
            );
        }


        // =========================================================
        // VALIDATION
        // =========================================================

        private void OnValidate()
        {
            closedResistance =
                Mathf.Max(0f, closedResistance);

            openResistance =
                Mathf.Max(
                    closedResistance,
                    openResistance
                );

            voltageThreshold =
                Mathf.Max(0f, voltageThreshold);

            currentThreshold =
                Mathf.Max(0f, currentThreshold);

            SparkTerminal[] terminals =
                GetComponentsInChildren<SparkTerminal>(
                    true
                );

            if (terminals == null ||
                terminals.Length < 2)
            {
                Debug.LogWarning(
                    $"SparkSwitch '{name}' requires at least " +
                    "2 SparkTerminal components.\n" +
                    "Terminal 0 = Input\n" +
                    "Terminal 1 = Output",
                    this
                );
            }
        }
    }
}