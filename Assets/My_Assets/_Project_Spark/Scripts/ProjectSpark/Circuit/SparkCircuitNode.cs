using System.Collections.Generic;

namespace ProjectSpark.Circuit
{
    public sealed class SparkCircuitNode
    {
        private readonly List<ProjectSpark.Gameplay.SparkTerminal> terminals = new();
        public int Id { get; }
        public IReadOnlyList<ProjectSpark.Gameplay.SparkTerminal> Terminals => terminals;
        public SparkCircuitNode(int id) { Id = id; }
        public void Add(ProjectSpark.Gameplay.SparkTerminal terminal) { if (terminal != null && !terminals.Contains(terminal)) terminals.Add(terminal); }
    }
}
