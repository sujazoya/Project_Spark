namespace ProjectSpark.Domain.Simulation
{
    public sealed class CircuitAnalyzer
    {
        private readonly VoltageSolver _voltage =
            new();

        private readonly CurrentSolver _current =
            new();

        private readonly PowerCalculator _power =
            new();

        private readonly TemperatureSolver _temperature =
            new();

        private readonly ShortCircuitDetector _shorts =
            new();

        private readonly OpenCircuitDetector _opens =
            new();

        public SimulationResult Analyze(
            CircuitGraph graph)
        {
            _voltage.Solve(graph);

            _current.Solve(graph);

            foreach (CircuitEdge edge in graph.Edges)
            {
                float power = _power.Calculate(
                    edge.A.Voltage,
                    edge.Current);

                // TODO:
                // Store or use 'power' if needed.
            }

            _temperature.Solve(graph);

            bool shortCircuit =
                _shorts.Detect(graph);

            bool openCircuit =
                _opens.Detect(graph);

            return new SimulationResult(
                !shortCircuit,
                shortCircuit || openCircuit,
                shortCircuit
                    ? "Short Circuit"
                    : openCircuit
                        ? "Open Circuit"
                        : "Simulation OK");
        }
    }
}
