using UnityEngine;

namespace ProjectSpark.Scanner
{
    /// <summary>
    /// Explicit bridge point between the existing Project Spark simulation and the scanner.
    /// Your simulation adapter writes its live state here. The scanner only reads the completed capture.
    /// </summary>
    public sealed class ScannerFeed : MonoBehaviour, IScannerFeed
    {
        [SerializeField] private bool verboseLogging;

        private readonly ScannerCapture capture = new();
        private int version;
        private bool capturing;

        public ScannerCapture Capture => capture;
        public int Version => version;

        public void BeginCapture()
        {
            capture.Clear();
            capturing = true;
        }

        public void AddComponent(ScannerComponentData component)
        {
            if (!capturing)
                return;

            capture.Components.Add(component);
        }

        public void AddConnection(ScannerConnectionData connection)
        {
            if (!capturing)
                return;

            capture.Connections.Add(connection);
        }

        public void SetNodeVoltage(ScannerNodeVoltage node)
        {
            if (!capturing)
                return;

            capture.Voltages.Add(node);
        }

        public void EndCapture()
        {
            if (!capturing)
                return;

            capturing = false;
            version++;

            if (verboseLogging)
                Debug.Log($"[SCANNER] Capture v{version}: {capture.Components.Count} components, {capture.Connections.Count} connections, {capture.Voltages.Count} voltage nodes.");
        }
    }
}
