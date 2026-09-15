using System;
using System.Collections.Generic;
using ProjectSpark.Gameplay;
using ProjectSpark.Electrical;
using UnityEngine;

namespace ProjectSpark.Scan
{
    public enum SparkScanSeverity { None, Info, Warning, Critical }

    public readonly struct SparkScanFinding
    {
        public SparkScanSeverity Severity { get; }
        public string Code { get; }
        public string Message { get; }
        public SparkScanFinding(SparkScanSeverity severity, string code, string message)
        { Severity = severity; Code = code; Message = message; }
    }

    public sealed class SparkScanReport
    {
        private readonly List<SparkScanFinding> findings = new();
        public IReadOnlyList<SparkScanFinding> Findings => findings;
        public SparkScanReport Add(SparkScanFinding finding) { findings.Add(finding); return this; }
    }

    [DisallowMultipleComponent]
    public sealed class SparkScanSystem : MonoBehaviour
    {
        public event Action<SparkElectronicObject, SparkScanReport> ScanCompleted;

        public SparkResult TryScan(SparkElectronicObject target, out SparkScanReport report)
        {
            report = new SparkScanReport();
            if (target == null) return SparkResult.Invalid("Scan target missing.");

            if (target.OperationalState == SparkOperationalState.Fault)
                report.Add(new SparkScanFinding(SparkScanSeverity.Critical, "OBJECT_FAULT", "Object is in a fault state."));

            if (target is SparkElectricalComponent electrical)
            {
                if (!electrical.ElectricalEnabled)
                    report.Add(new SparkScanFinding(SparkScanSeverity.Warning, "ELECTRICAL_DISABLED", "Electrical participation is disabled."));
                if (electrical is SparkResistor resistor && resistor.IsOverPower)
                    report.Add(new SparkScanFinding(SparkScanSeverity.Critical, "OVER_POWER", "Resistor power exceeds its rating."));
                if (electrical is SparkLED led)
                {
                    if (led.IsOverCurrent) report.Add(new SparkScanFinding(SparkScanSeverity.Critical, "LED_OVER_CURRENT", "LED forward current exceeds its limit."));
                    if (led.IsReverseVoltageExceeded) report.Add(new SparkScanFinding(SparkScanSeverity.Critical, "LED_REVERSE_VOLTAGE", "LED reverse voltage exceeds its limit."));
                }
            }

            if (report.Findings.Count == 0)
                report.Add(new SparkScanFinding(SparkScanSeverity.Info, "NO_FAULT", "No scan-level faults detected."));

            ScanCompleted?.Invoke(target, report);
            return SparkResult.Success();
        }
    }
}
