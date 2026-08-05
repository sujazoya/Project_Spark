using UnityEngine;

namespace ProjectSpark.Domain.Diagnostics
{
    public sealed class DiagnosticManager
        : MonoBehaviour
    {
        public DiagnosticMode
            CurrentMode { get; private set; }

        public void SetMode(
            DiagnosticMode mode)
        {
            if (CurrentMode == mode)
                return;

            CurrentMode = mode;

            DiagnosticEvents
                .RaiseModeChanged(mode);
        }

        public void Disable()
        {
            SetMode(
                DiagnosticMode.Disabled);
        }
    }
}
