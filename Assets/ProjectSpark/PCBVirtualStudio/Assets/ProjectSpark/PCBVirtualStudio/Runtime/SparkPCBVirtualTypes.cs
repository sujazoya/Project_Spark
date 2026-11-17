using System;
using UnityEngine;

namespace ProjectSpark.PCBVirtualStudio
{
    // These are virtual inspection / repair commands, not electrical-solver results.
    public enum SparkVirtualTool
    {
        Inspect, Multimeter, Continuity, Oscilloscope, LogicAnalyzer,
        ThermalCamera, NetTracer, FaultScanner, Microscope,
        Flux, Solder, Desolder, Reflow, HotAir, TracePatch,
        ViaRepair, ReplaceComponent, Clean, BGAReball, SolderMask, Calibrate
    }

    public enum SparkBoardDefect
    {
        None, ColdJoint, OpenPad, SolderBridge, OpenTrace,
        OpenVia, FailedComponent, BGABallFault, Contamination,
        CalibrationDrift, WrongValue, OverTemperature
    }

    public enum SparkPCBTargetKind
    {
        IC, BGA, Resistor, Capacitor, MOSFET, Connector,
        TestPad, Via, CopperTrace, Sensor, Clock, Regulator
    }

    [Serializable]
    public struct SparkRepairToolResult
    {
        public bool accepted;
        public bool resolved;
        public string message;
        public SparkRepairToolResult(bool accepted, bool resolved, string message)
        {
            this.accepted = accepted;
            this.resolved = resolved;
            this.message = message;
        }
    }

    // Authoritative game projects implement this interface in their existing electrical / repair adapter.
    // Return false when operation is not legal or is not yet accepted by actual board state.
    public interface ISparkPCBRepairAuthority
    {
        bool TryApplyRepair(SparkVirtualPCBTarget target, SparkVirtualTool tool, out string message);
        bool TryReadProbe(SparkVirtualPCBTarget target, SparkVirtualTool tool, out string reading);
    }

    public static class SparkPCBToolCatalog
    {
        public static bool IsDiagnostic(SparkVirtualTool t) =>
            t <= SparkVirtualTool.Microscope;

        public static bool IsPreparation(SparkVirtualTool t) =>
            t == SparkVirtualTool.Flux || t == SparkVirtualTool.HotAir ||
            t == SparkVirtualTool.SolderMask;

        public static string DisplayName(SparkVirtualTool t)
        {
            switch (t)
            {
                case SparkVirtualTool.Multimeter: return "DMM PROBE";
                case SparkVirtualTool.Continuity: return "CONTINUITY";
                case SparkVirtualTool.Oscilloscope: return "OSCILLOSCOPE";
                case SparkVirtualTool.LogicAnalyzer: return "LOGIC ANALYZER";
                case SparkVirtualTool.ThermalCamera: return "THERMAL SCAN";
                case SparkVirtualTool.NetTracer: return "NET TRACER";
                case SparkVirtualTool.FaultScanner: return "FAULT SCAN";
                case SparkVirtualTool.Microscope: return "MICROSCOPE";
                case SparkVirtualTool.HotAir: return "HOT AIR";
                case SparkVirtualTool.TracePatch: return "TRACE PATCH";
                case SparkVirtualTool.ViaRepair: return "VIA REPAIR";
                case SparkVirtualTool.ReplaceComponent: return "REPLACE PART";
                case SparkVirtualTool.BGAReball: return "BGA REBALL";
                case SparkVirtualTool.SolderMask: return "UV MASK";
                default: return t.ToString().ToUpperInvariant();
            }
        }

        public static string Guidance(SparkVirtualTool tool)
        {
            switch (tool)
            {
                case SparkVirtualTool.Inspect: return "Tap a pad or component to inspect its identity and condition.";
                case SparkVirtualTool.Multimeter: return "Tap an electrical target to inspect a virtual DMM reading.";
                case SparkVirtualTool.Continuity: return "Probe a copper path, via or pad to check continuity.";
                case SparkVirtualTool.Oscilloscope: return "Probe a signal target to view waveform diagnostics.";
                case SparkVirtualTool.LogicAnalyzer: return "Tap a digital target to inspect timing states.";
                case SparkVirtualTool.ThermalCamera: return "Tap a component for localized thermal inspection.";
                case SparkVirtualTool.NetTracer: return "Tap a target to illuminate its net identity.";
                case SparkVirtualTool.FaultScanner: return "Tap suspect components to locate simulated defect evidence.";
                case SparkVirtualTool.Microscope: return "Inspect fine solder joints and pads at high magnification.";
                case SparkVirtualTool.Flux: return "Apply flux to an affected solder joint before solder/reflow.";
                case SparkVirtualTool.Solder: return "Tap a flux-prepared cold joint or open pad to repair.";
                case SparkVirtualTool.Desolder: return "Tap a solder bridge to remove the unwanted connection.";
                case SparkVirtualTool.Reflow: return "Tap a flux-prepared cold joint for controlled reflow.";
                case SparkVirtualTool.HotAir: return "Prepare a failed IC / BGA part for removal or repair.";
                case SparkVirtualTool.TracePatch: return "Tap a broken copper trace to patch its route.";
                case SparkVirtualTool.ViaRepair: return "Tap an open via to restore the layer transition.";
                case SparkVirtualTool.ReplaceComponent: return "Tap a faulty part after heating/rework preparation.";
                case SparkVirtualTool.Clean: return "Tap flux residue or contamination to clean the surface.";
                case SparkVirtualTool.BGAReball: return "Tap a flux- and heat-prepared BGA fault to reball.";
                case SparkVirtualTool.SolderMask: return "Apply UV mask to an already repaired trace or via.";
                case SparkVirtualTool.Calibrate: return "Tap a drifting sensor to run a calibration sequence.";
                default: return "Choose a tool, then tap the target on the board.";
            }
        }
    }
}
