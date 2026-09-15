using ProjectSpark.Gameplay;

namespace ProjectSpark.Circuit
{
    public readonly struct SparkCircuitConnection
    {
        public ulong Id { get; }
        public SparkTerminal A { get; }
        public SparkTerminal B { get; }
        public SparkConnectionKind Kind { get; }
        public SparkConnectionDirection Direction { get; }
        public SparkCircuitConnection(ulong id, SparkTerminal a, SparkTerminal b, SparkConnectionKind kind, SparkConnectionDirection direction)
        { Id = id; A = a; B = b; Kind = kind; Direction = direction; }
    }
}
