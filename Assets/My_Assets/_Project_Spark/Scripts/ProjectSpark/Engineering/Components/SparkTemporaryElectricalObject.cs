using UnityEngine;

namespace ProjectSpark.Gameplay
{
    /// <summary>
    /// Simple temporary electrical object used for
    /// Level 3 prototyping and testing.
    ///
    /// This is NOT a switch.
    /// This is NOT an LED.
    ///
    /// It simply provides two electrical terminals,
    /// participates as a conductive device, receives
    /// the solver result, and exposes its electrical
    /// working state.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkTemporaryElectricalObject :
        SparkElectricalComponent,
        ISparkConductiveDevice
    {
        #region Inspector

        [Header("Object")]
        [SerializeField]
        private string objectName = "Temporary Electrical Object";

        [Header("Terminals")]
        [SerializeField]
        private SparkTerminal inputTerminal;

        [SerializeField]
        private SparkTerminal outputTerminal;

        [Header("Electrical")]
        [SerializeField, Min(0f)]
        private float resistance = 0.1f;

        [SerializeField, Min(0f)]
        private float voltageThreshold = 0.001f;

        [SerializeField, Min(0f)]
        private float currentThreshold = 0.001f;

        [Header("Runtime")]
        [SerializeField]
        private bool showDebugLog;

        #endregion


        #region Identity

        public string ObjectName =>
            objectName;

        #endregion


        #region Terminals

        public SparkTerminal InputTerminal =>
            inputTerminal;

        public SparkTerminal OutputTerminal =>
            outputTerminal;

        public bool IsConfigured =>
            inputTerminal != null &&
            outputTerminal != null;

        #endregion


        #region Electrical Configuration

        public float Resistance =>
            resistance;

        public float VoltageThreshold =>
            voltageThreshold;

        public float CurrentThreshold =>
            currentThreshold;

        #endregion


        #region Input State

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

        public bool InputHasVoltage =>
            Mathf.Abs(InputVoltage) >=
            voltageThreshold;

        public bool InputHasCurrent =>
            Mathf.Abs(InputCurrent) >=
            currentThreshold;

        #endregion


        #region Output State

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

        public bool OutputHasVoltage =>
            Mathf.Abs(OutputVoltage) >=
            voltageThreshold;

        public bool OutputHasCurrent =>
            Mathf.Abs(OutputCurrent) >=
            currentThreshold;

        #endregion


        #region Component State

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

        public bool HasPower =>
            Mathf.Abs(Power) >=
            voltageThreshold *
            currentThreshold;

        public bool IsConducting =>
            ElectricalEnabled &&
            Conduction ==
            SparkConductionState.Conducting;

        public bool HasPowerFlow =>
            IsConducting &&
            HasCurrent;

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


        #region Working State

        /// <summary>
        /// Object has a valid electrical connection
        /// and is receiving voltage.
        /// </summary>
        public bool IsPowered =>
            ElectricalEnabled &&
            HasVoltage;

        /// <summary>
        /// Object has an active electrical path.
        /// </summary>
        public bool IsWorking =>
            ElectricalEnabled &&
            HasBothConnections &&
            IsConducting &&
            HasPowerFlow;

        /// <summary>
        /// Useful for Level 3 UI/visual feedback.
        /// </summary>
        public string WorkingState
        {
            get
            {
                if (!ElectricalEnabled)
                    return "DISABLED";

                if (!HasInputConnection)
                    return "NO INPUT";

                if (!HasOutputConnection)
                    return "NO OUTPUT";

                if (!HasVoltage)
                    return "NO POWER";

                if (!IsConducting)
                    return "NOT CONDUCTING";

                if (!HasPowerFlow)
                    return "NO POWER FLOW";

                return "WORKING";
            }
        }

        #endregion


        #region Conductive Device

        public bool CanConductBetween(
            SparkTerminal from,
            SparkTerminal to)
        {
            if (!ElectricalEnabled)
                return false;

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


        #region Electrical Solver

        public override void ApplyElectricalState(
            in SparkElectricalState state)
        {
            base.ApplyElectricalState(state);

            if (showDebugLog)
            {
                Debug.Log(
                    $"[TEMP ELECTRICAL OBJECT] " +
                    $"{objectName} | " +
                    $"V={state.Voltage:F3} | " +
                    $"I={state.Current:F3} | " +
                    $"P={state.Power:F3} | " +
                    $"Conduction={state.Conduction}",
                    this);
            }
        }

        #endregion


        #region Runtime Helpers

        public bool HasValidElectricalState()
        {
            return
                ElectricalEnabled &&
                IsConfigured &&
                HasVoltage &&
                IsConducting;
        }

        public void RefreshElectricalState()
        {
            NotifyElectricalConfigurationChanged();
        }

        #endregion


        #region Validation

        private void OnValidate()
        {
            resistance =
                Mathf.Max(
                    0f,
                    resistance);

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
