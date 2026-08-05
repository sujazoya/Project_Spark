using System;
using UnityEngine;
using ProjectSpark.UI.Data;

namespace ProjectSpark.UI.Adapters
{
    public sealed class DiagnosticsUIAdapter :
        MonoBehaviour
    {
        public event Action<
            DiagnosticsViewModel>
            ViewModelChanged;

        public DiagnosticsViewModel Current
        {
            get;
            private set;
        }

        public void UpdateDiagnostics(
            string componentName,
            string componentType,
            string status,
            string faultDescription,
            bool hasFault)
        {
            Current =
                new DiagnosticsViewModel(
                    componentName,
                    componentType,
                    status,
                    faultDescription,
                    hasFault);

            ViewModelChanged?.Invoke(
                Current);
        }
    }
}