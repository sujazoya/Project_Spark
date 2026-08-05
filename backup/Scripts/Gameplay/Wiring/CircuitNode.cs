// Assets/My_Assets/_Project_Spark/Scripts/Gameplay/Wiring/CircuitNode.cs

namespace ProjectSpark.Gameplay.Wiring
{
    public sealed class CircuitNode
    {
        public WireConnector Connector;

        public CircuitNode(WireConnector connector)
        {
            Connector = connector;
        }
    }
}