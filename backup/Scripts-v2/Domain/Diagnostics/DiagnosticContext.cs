using ProjectSpark.Domain.Simulation;

namespace ProjectSpark.Domain.Diagnostics
{
    public sealed class DiagnosticContext
    {
        public SimulationContext Simulation;

        public DiagnosticMode Mode;
    }
}
