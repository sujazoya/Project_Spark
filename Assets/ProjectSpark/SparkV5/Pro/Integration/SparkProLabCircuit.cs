using System;
using ProjectSpark.Circuit;
using ProjectSpark.Electrical;
using ProjectSpark.Gameplay;
using UnityEngine;

namespace ProjectSpark.SparkV5.Pro
{
    public sealed class SparkProLabCircuit : MonoBehaviour
    {
        public SparkCircuitSystem circuit;
        public SparkElectricalSolver solver;
        public SparkSwitch control;
        public SparkResistor load;
        public SparkTerminal supplyPositive, supplyReturn, switchIn, switchOut, resistorIn, resistorOut;

        internal static string Demonstrate(string request)
        {
            string topic = SparkCortex.Normalize(request);
            if (topic.Contains("for me") || !(topic.Contains("ohm") || topic.Contains("switch") ||
                topic.Contains("continuity") || topic.Contains("closed path")))
                return "This experiment supports a switch and resistor on an isolated trainer. Use the normal tools for placement, wiring or instrument operation on your workbench.";
            var prefab = Resources.Load<SparkProLabCircuit>("SparkProIsolatedLab");
            if (!prefab) return "The isolated experiment prefab is unavailable.";
            var lab = Instantiate(prefab);
            try
            {
                if (!lab.TryRun(out float openAmps, out float closedAmps, out float volts, out float brokenAmps))
                    return "The isolated simulator did not produce valid measurements.";
                return $"Isolated solver observation: switch open {Math.Abs(openAmps) * 1000:G3} mA; " +
                    $"switch closed {Math.Abs(closedAmps) * 1000:G3} mA with {Math.Abs(volts):G3} V across the load. " +
                    $"Removing the return gave {Math.Abs(brokenAmps) * 1000:G3} mA. The experiment uses a separate virtual supply and return.";
            }
            finally
            {
                lab.gameObject.SetActive(false);
                Destroy(lab.gameObject);
            }
        }

        private bool TryRun(out float open, out float closed, out float volts, out float disconnected)
        {
            open = closed = volts = disconnected = 0;
            if (!circuit || !solver || !Owned(supplyPositive) || !Owned(supplyReturn) || !Owned(switchIn) ||
                !Owned(switchOut) || !Owned(resistorIn) || !Owned(resistorOut)) return false;
            bool wired;
            SparkCircuitConnection returnLead = default;
            using (solver.BeginConfigurationBatch())
            {
                control.SetState(SparkSwitchState.Open);
                wired = Connect(supplyPositive, switchIn) && Connect(switchOut, resistorIn) &&
                    circuit.TryCreateConnection(resistorOut, supplyReturn, SparkConnectionKind.Wire,
                        SparkConnectionDirection.Bidirectional, out returnLead);
            }
            if (!wired || !solver.SolveNow()) return false;
            if (!solver.TryCalculateComponentCurrent(load, out open)) return false;
            control.SetState(SparkSwitchState.Closed);
            if (!solver.SolveNow() || !solver.TryCalculateComponentCurrent(load, out closed)) return false;
            if (solver.LatestSolveResult == null || !solver.LatestSolveResult.TryGetVoltage(resistorIn, resistorOut, out volts) ||
                !float.IsFinite(volts) || Math.Abs(closed) <= Math.Abs(open)) return false;
            return circuit.TryRemoveConnection(returnLead.Id, out _) && solver.SolveNow() &&
                solver.TryCalculateComponentCurrent(load, out disconnected) && Math.Abs(disconnected) < Math.Abs(closed);
        }

        private bool Owned(SparkTerminal pin) => pin && pin.transform.IsChildOf(transform);
        private bool Connect(SparkTerminal a, SparkTerminal b) =>
            circuit.TryCreateConnection(a, b, SparkConnectionKind.Wire, SparkConnectionDirection.Bidirectional, out _);
    }
}
