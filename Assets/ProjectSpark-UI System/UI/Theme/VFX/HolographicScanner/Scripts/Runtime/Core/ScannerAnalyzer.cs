using System.Collections.Generic;
using UnityEngine;

namespace ProjectSpark.Scanner
{
    public sealed class ScannerAnalyzer
    {
        private readonly List<ScannerFault> faults = new();

        public IReadOnlyList<ScannerFault> Faults => faults;

        public ScannerResult Analyze(ScannerCapture capture)
        {
            faults.Clear();

            if (capture == null || capture.Components.Count == 0)
            {
                faults.Add(new ScannerFault
                {
                    type = ScannerFaultType.DeadNode,
                    code = "FAULT_00",
                    title = "NO CIRCUIT DATA",
                    detail = "The scanner did not receive a valid circuit capture.",
                    worldPosition = Vector3.zero
                });

                return ScannerResult.Fault;
            }

            CheckConnections(capture);
            CheckVoltage(capture);

            return faults.Count == 0 ? ScannerResult.Pass : ScannerResult.Fault;
        }

        private void CheckConnections(ScannerCapture capture)
        {
            for (int i = 0; i < capture.Connections.Count; i++)
            {
                ScannerConnectionData connection = capture.Connections[i];

                if (connection.electricallyClosed)
                    continue;

                ScannerComponentData component = capture.FindComponent(connection.componentA);

                faults.Add(new ScannerFault
                {
                    type = ScannerFaultType.OpenCircuit,
                    componentId = component.id,
                    code = $"FAULT_{component.id:00}",
                    title = "OPEN CIRCUIT",
                    detail = string.IsNullOrWhiteSpace(component.reference)
                        ? "Electrical path is open."
                        : $"{component.reference} path is open.",
                    worldPosition = component.worldPosition,
                    measuredValue = connection.voltage,
                    expectedValue = 0f
                });
            }
        }

        private void CheckVoltage(ScannerCapture capture)
        {
            const float maximumExpectedVoltage = 24f;

            for (int i = 0; i < capture.Voltages.Count; i++)
            {
                ScannerNodeVoltage node = capture.Voltages[i];

                if (node.voltage <= maximumExpectedVoltage)
                    continue;

                faults.Add(new ScannerFault
                {
                    type = ScannerFaultType.OverVoltage,
                    componentId = node.nodeId,
                    code = $"FAULT_{node.nodeId:00}",
                    title = "OVER VOLTAGE",
                    detail = $"Node voltage {node.voltage:0.00} V exceeds the scanner limit.",
                    worldPosition = node.worldPosition,
                    measuredValue = node.voltage,
                    expectedValue = maximumExpectedVoltage
                });
            }
        }
    }
}
