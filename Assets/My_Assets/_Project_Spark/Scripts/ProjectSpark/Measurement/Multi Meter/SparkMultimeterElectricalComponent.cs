
using UnityEngine;
using ProjectSpark.Gameplay;

namespace ProjectSpark.Measurement
{
    /// <summary>
    /// Electrical model of the multimeter's internal current shunt.
    ///
    /// This component participates in the normal SparkElectricalSolver
    /// topology when the multimeter is being used as an ammeter.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkMultimeterElectricalComponent
        : SparkElectricalComponent
    {
        [Header("Current Shunt")]
        [SerializeField, Min(0.000001f)]
        private float shuntResistanceOhms = 0.05f;

        [Header("Terminals")]
        [SerializeField]
        private SparkTerminal redTerminal;

        [SerializeField]
        private SparkTerminal blackTerminal;

        public float ShuntResistanceOhms =>
            Mathf.Max(0.000001f, shuntResistanceOhms);

        public SparkTerminal RedTerminal =>
            redTerminal;

        public SparkTerminal BlackTerminal =>
            blackTerminal;

        public bool TryGetTerminals(
            out SparkTerminal red,
            out SparkTerminal black)
        {
            red = redTerminal;
            black = blackTerminal;

            return red != null &&
                   black != null;
        }

        private void OnValidate()
        {
            shuntResistanceOhms =
                Mathf.Max(
                    0.000001f,
                    shuntResistanceOhms);
        }
    }
}
