using UnityEngine;

namespace ProjectSpark.Gameplay
{
    [DisallowMultipleComponent]
    public sealed class SparkMotor :
        SparkElectricalComponent,
        ISparkConductiveDevice
    {
        [Header("Terminals")]
        [SerializeField]
        private SparkTerminal positiveTerminal;

        [SerializeField]
        private SparkTerminal negativeTerminal;

        [Header("Motor")]
        [SerializeField, Min(0f)]
        private float startupVoltage = 3f;

        [SerializeField, Min(0.000001f)]
        private float windingResistance = 2f;

        [SerializeField, Min(0f)]
        private float maximumCurrent = 2f;

        public SparkTerminal PositiveTerminal =>
            positiveTerminal;

        public SparkTerminal NegativeTerminal =>
            negativeTerminal;

        public float StartupVoltage =>
            startupVoltage;

        public float WindingResistance =>
            windingResistance;

        public float MaximumCurrent =>
            maximumCurrent;

        public bool IsRunning =>
            ElectricalEnabled &&
            Mathf.Abs(
                ElectricalState.Voltage) >=
            startupVoltage &&
            ElectricalState.Conduction ==
            SparkConductionState.Conducting;

        public bool IsOverCurrent =>
            Mathf.Abs(
                ElectricalState.Current) >
            maximumCurrent;

        public bool CanConductBetween(
            SparkTerminal from,
            SparkTerminal to)
        {
            if (!ElectricalEnabled ||
                from == null ||
                to == null)
            {
                return false;
            }

            return
                (from == positiveTerminal &&
                 to == negativeTerminal) ||
                (from == negativeTerminal &&
                 to == positiveTerminal);
        }

        private void OnValidate()
        {
            startupVoltage =
                Mathf.Max(
                    0f,
                    startupVoltage);

            windingResistance =
                Mathf.Max(
                    0.000001f,
                    windingResistance);

            maximumCurrent =
                Mathf.Max(
                    0f,
                    maximumCurrent);
        }
    }
}