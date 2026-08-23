using UnityEngine;

namespace ProjectSpark.Scanner
{
    [DisallowMultipleComponent]
    public sealed class ScannerDiagnosticDataProvider
        : MonoBehaviour
    {
        [SerializeField]
        private ScannerComponentTarget target;

        [Header("Diagnostic Data")]
        [SerializeField]
        private ScannerDiagnosticData data;

        public ScannerDiagnosticData Data =>
            data;

        public ScannerComponentTarget Target =>
            target;

        public void SetData(
            ScannerDiagnosticData newData)
        {
            data =
                newData;
        }
    }
}