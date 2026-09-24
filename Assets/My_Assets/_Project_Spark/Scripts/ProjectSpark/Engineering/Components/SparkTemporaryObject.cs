
using UnityEngine;

namespace ProjectSpark.Gameplay
{
    /// <summary>
    /// Plain temporary electrical gameplay object.
    ///
    /// No terminals.
    /// No switch behavior.
    /// No LED behavior.
    /// No custom interaction system.
    ///
    /// Intended to be moved, rotated and manipulated
    /// by the existing Project Spark Tools System.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkTemporaryObject :
        SparkElectricalComponent,
        ISparkConductiveDevice
    {
        #region Inspector

        [Header("Temporary Object")]
        [SerializeField]
        private string objectName = "Temporary Object";

        #endregion


        #region Properties

        public string ObjectName =>
            objectName;

        #endregion


        #region Conductive Device

        public bool CanConductBetween(
            SparkTerminal from,
            SparkTerminal to)
        {
            /*
             * This object currently has no terminals.
             *
             * It exists only as a plain electrical component
             * that can later receive functionality.
             */

            return false;
        }

        #endregion


        #region Electrical State

        public float Voltage =>
            ElectricalState.Voltage;

        public float Current =>
            ElectricalState.Current;

        public float Power =>
            ElectricalState.Power;

        public SparkConductionState Conduction =>
            ElectricalState.Conduction;

        public bool IsConducting =>
            Conduction ==
            SparkConductionState.Conducting;

        #endregion


        #region Solver

        public override void ApplyElectricalState(
            in SparkElectricalState state)
        {
            base.ApplyElectricalState(state);
        }

        #endregion
    }
}
