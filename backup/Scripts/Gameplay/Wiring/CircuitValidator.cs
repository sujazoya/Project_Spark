// Assets/My_Assets/_Project_Spark/Scripts/Gameplay/Wiring/CircuitValidator.cs

using UnityEngine;

namespace ProjectSpark.Gameplay.Wiring
{
    public sealed class CircuitValidator : MonoBehaviour
    {
        [Header("Wires")]
        [SerializeField]
        private WireController[] wires;

        [Header("Required Connectors")]
        [SerializeField]
        private WireConnector batteryPositive;

        [SerializeField]
        private WireConnector batteryNegative;

        [SerializeField]
        private WireConnector bulbPositive;

        [SerializeField]
        private WireConnector bulbNegative;

        [Header("Output")]
        [SerializeField]
        private CurrentFlowController currentFlow;

        private readonly CircuitGraph graph = new();

        public bool Validate()
        {
            graph.Clear();

            foreach (WireController wire in wires)
            {
                if (wire == null)
                    continue;

                if (!wire.IsConnected)
                    continue;

                graph.AddConnection(
                    wire.StartConnector,
                    wire.EndConnector);
            }

            bool powered =
                graph.HasPath(batteryPositive, bulbPositive) &&
                graph.HasPath(bulbNegative, batteryNegative);

            if (currentFlow != null)
                currentFlow.SetPowered(powered);

            return powered;
        }
    }
}