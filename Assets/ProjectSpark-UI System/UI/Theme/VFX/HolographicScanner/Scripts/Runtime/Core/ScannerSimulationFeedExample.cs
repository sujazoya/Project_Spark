using UnityEngine;

namespace ProjectSpark.Scanner
{
    /// <summary>
    /// Example adapter for testing the scanner without touching your real simulation.
    /// Delete this component from production scenes.
    /// </summary>
    public sealed class ScannerSimulationFeedExample : MonoBehaviour
    {
        [SerializeField] private ScannerFeed feed;
        [SerializeField] private Transform boardRoot;

        [ContextMenu("Write Example Capture")]
        public void WriteExampleCapture()
        {
            if (feed == null)
                return;

            feed.BeginCapture();

            Transform root = boardRoot != null ? boardRoot : transform;

            feed.AddComponent(new ScannerComponentData
            {
                id = 12,
                reference = "R12",
                displayName = "RESISTOR",
                value = "10kΩ ±5%",
                worldPosition = root.position,
                powered = true
            });

            feed.AddComponent(new ScannerComponentData
            {
                id = 13,
                reference = "C13",
                displayName = "CAPACITOR",
                value = "100nF",
                worldPosition = root.position + Vector3.right * 0.25f,
                powered = true
            });

            feed.AddConnection(new ScannerConnectionData
            {
                id = 1,
                componentA = 12,
                componentB = 13,
                electricallyClosed = false,
                voltage = 0f,
                worldStart = root.position,
                worldEnd = root.position + Vector3.right * 0.25f
            });

            feed.SetNodeVoltage(new ScannerNodeVoltage
            {
                nodeId = 12,
                voltage = 5f,
                worldPosition = root.position
            });

            feed.EndCapture();
        }
    }
}
