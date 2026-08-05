using System;

namespace ProjectSpark.Domain.Diagnostics
{
    public static class DiagnosticEvents
    {
        public static event Action<DiagnosticMode>
            ModeChanged;

        public static void RaiseModeChanged(
            DiagnosticMode mode)
        {
            ModeChanged?.Invoke(mode);
        }
    }
}
