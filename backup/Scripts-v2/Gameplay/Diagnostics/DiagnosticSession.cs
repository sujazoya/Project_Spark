namespace ProjectSpark.Gameplay.Diagnostics
{
    public sealed class DiagnosticSession
    {
        public DiagnosticReport Report =
            new();

        public bool Completed;
    }
}
