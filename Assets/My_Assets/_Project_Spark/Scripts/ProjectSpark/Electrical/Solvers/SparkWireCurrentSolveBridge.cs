using System;
using System.Collections.Generic;
using UnityEngine;
using ProjectSpark.Circuit;
using ProjectSpark.Gameplay;
using ProjectSpark.Measurement;

namespace ProjectSpark.Electrical
{
    [DisallowMultipleComponent]
    public sealed class SparkWireCurrentSolveBridge : MonoBehaviour
    {
        [SerializeField] private SparkElectricalSolver solver;
        [SerializeField] private SparkCircuitSystem circuit;
        [SerializeField] private bool logDiagnostics;

        public event Action<IReadOnlyList<SparkWireBranchCurrentResolver.WireCurrent>> CurrentsResolved;
        public event Action CurrentsInvalidated;

        private readonly SparkWireBranchCurrentResolver resolver = new SparkWireBranchCurrentResolver();
        private readonly Dictionary<SparkTerminal, float> injections = new Dictionary<SparkTerminal, float>();
        private readonly List<SparkWireBranchCurrentResolver.WireCurrent> currents = new List<SparkWireBranchCurrentResolver.WireCurrent>();
        private readonly List<SparkCircuitConnection> connections = new List<SparkCircuitConnection>();

        public IReadOnlyList<SparkWireBranchCurrentResolver.WireCurrent> CurrentReadings => currents;

       
private void OnEnable()
{
    if (solver != null)
    {
        solver.SolveCompleted += OnSolveCompleted;
        solver.SolveFailed += Invalidate;
    }

    if (circuit != null)
    {
        circuit.TopologyChanged += Invalidate;
    }

    Invalidate();

    // The solver may have completed before this bridge
    // subscribed to SolveCompleted.
    //
    // In that case, consume the existing valid result.
    if (solver != null &&
        solver.LatestSolveResult != null &&
        !solver.IsDirty)
    {
        Debug.Log(
            "[WIRE BRIDGE] Processing existing solve result.",
            this);

        OnSolveCompleted();
    }
}


        private void OnDisable()
        {
            if (solver != null)
            {
                solver.SolveCompleted -= OnSolveCompleted;
                solver.SolveFailed -= Invalidate;
            }
            if (circuit != null) circuit.TopologyChanged -= Invalidate;
            Invalidate();
        }

        private void Invalidate()
        {
            currents.Clear();
            injections.Clear();
            CurrentsInvalidated?.Invoke();
        }

        private void OnSolveCompleted()
        {
            Debug.Log(
    "[WIRE BRIDGE] SolveCompleted received | " +
    $"HasResult={solver != null && solver.LatestSolveResult != null} | " +
    $"Dirty={solver != null && solver.IsDirty}",
    this);
            Invalidate();
            if (solver == null || circuit == null || solver.LatestSolveResult == null || solver.IsDirty)
                return;
            var result = solver.LatestSolveResult;
            if (result.Context == null || result.Context.Graph == null) return;

            connections.Clear();
            circuit.CopyConnections(connections);
            foreach (var connection in connections)
            {
                if (!connection.IsValid || connection.Kind == SparkConnectionKind.Probe ||
                    connection.A == null || connection.B == null ||
                    connection.A.Owner == null || connection.B.Owner == null ||
                    !connection.A.Owner.isActiveAndEnabled || !connection.B.Owner.isActiveAndEnabled)
                    continue;
                injections[connection.A] = 0f;
                injections[connection.B] = 0f;
            }

            foreach (SparkElectricalComponent component in solver.ElectricalComponents)
            {
                if (component == null || !component.isActiveAndEnabled || !component.ElectricalEnabled)
                    continue;
                    
            SparkElectricalSolverRegistry registry =
                solver.SolverRegistry;

            if (registry != null && registry.IsIgnored(component))
            {
                if (logDiagnostics)
                {
                    Debug.Log(
                        "[WIRE BRIDGE] Ignored by solver registry: " +
                        component.name +
                        " | Type=" + component.GetType().Name,
                        component);
                }

                continue;
            }


                    // Temporary multimeter probes are measurement helpers,
                // not devices that inject current into the circuit.
                if (component is SparkTemporaryObject)
                {
                    if (logDiagnostics)
                    {
                        Debug.Log(
                            "[WIRE BRIDGE] Skipping temporary measurement object: " +
                            component.name,
                            component);
                    }

                    continue;
                }
                
                // SparkSingleWire represents an ideal physical wire.
                // Its branch current is resolved from the circuit
                // connections and device terminal injections.
                //
                // Do not treat it as an additional two-terminal device.
                if (component is SparkSingleWire)
                {
                    if (logDiagnostics)
                    {
                        Debug.Log(
                            "[WIRE BRIDGE] Skipping ideal wire component: " +
                            component.name,
                            component);
                    }

                    continue;
                }


                // A component without a participating wire terminal cannot affect these wire currents.
                // Components with attached wires require an explicitly verified signed-current adapter.
                SparkTerminal a = null, b = null;
                float amps = 0f;
                bool supported = true;

                if (component is SparkResistor resistor)
                {
                    a = resistor.TerminalA;
                    b = resistor.TerminalB;
                    supported = solver.TryCalculateComponentCurrent(component, out amps);
                    if (supported) { AddInjection(a, -amps); AddInjection(b, amps); }
                }
                else if (component is SparkSwitch sw)
                {
                    a = sw.InputTerminal;
                    b = sw.OutputTerminal;
                    supported = solver.TryCalculateComponentCurrent(component, out amps);
                    if (supported) { AddInjection(a, -amps); AddInjection(b, amps); }
                }
                else if (component is SparkPowerSupply supply)
                {
                    a = supply.PositiveTerminal;
                    b = supply.NegativeTerminal;
                    if (supply.IsOutputActive)
                        supported = result.TryGetVoltageSourceCurrent(supply, out amps);
                    if (supported) { AddInjection(a, amps); AddInjection(b, -amps); }
                }
                
                else if (component is SparkLED led)
                {
                    a = led.AnodeTerminal;
                    b = led.CathodeTerminal;

                    // The LED solver calculates positive current
                    // from anode to cathode.
                    supported =
                        a != null &&
                        b != null &&
                        solver.TryCalculateComponentCurrent(
                            led,
                            out amps);

                    if (supported)
                    {
                        // Current leaves the external wire network
                        // at the anode and returns at the cathode.
                        AddInjection(a, -amps);
                        AddInjection(b, +amps);
                    }
                }
                
                else if (component is SparkMultimeterElectricalComponent meter)
                {
                    // Voltage, resistance, and continuity modes:
                    // the internal current shunt is disconnected.
                    if (!meter.CurrentMeasurementEnabled)
                    {
                        continue;
                    }

                    // Current mode: the shunt connects RED to BLACK.
                    a = meter.RedTerminal;
                    b = meter.BlackTerminal;

                    supported =
                        a != null &&
                        b != null &&
                        solver.TryCalculateComponentCurrent(
                            meter,
                            out amps);

                    if (supported)
                    {
                        // Positive shunt current flows RED -> BLACK.
                        AddInjection(a, -amps);
                        AddInjection(b, +amps);
                    }
                }


                else
                {
                    // Unknown devices may be active electrical participants.
                    // Fail closed until an explicit adapter is added.
                    supported = false;
                }

               if (!supported || float.IsNaN(amps) || float.IsInfinity(amps))
{
    Debug.LogWarning(
        "[WIRE BRIDGE BLOCKED] " +
        $"Component={component.name} | " +
        $"Type={component.GetType().FullName} | " +
        $"Supported={supported} | " +
        $"Current={amps} | " +
        $"Active={component.isActiveAndEnabled} | " +
        $"ElectricalEnabled={component.ElectricalEnabled}",
        component);

    Invalidate();
    return;
}
            }
            Debug.Log(
    $"[WIRE BRIDGE] Starting resolver | " +
    $"Connections={connections.Count} | " +
    $"TerminalInjections={injections.Count}",
    this);

           bool resolved = resolver.TryResolve(
    circuit,
    injections,
    currents,
    out string diagnostic);

Debug.Log(
    "[WIRE BRIDGE RESOLVER] " +
    $"Success={resolved} | " +
    $"WireReadings={currents.Count} | " +
    $"Diagnostic={diagnostic}",
    this);

if (!resolved)
{
    Invalidate();
    return;
}

if (logDiagnostics)
{
    foreach (var reading in currents)
    {
        Debug.Log(
            "[WIRE CURRENT VALUE] " +
            $"ConnectionId={reading.ConnectionId} | " +
            $"Current={reading.AmpsAtoB:F6} A | " +
            $"Magnitude={Mathf.Abs(reading.AmpsAtoB):F6} A | " +
            $"Direction={(reading.AmpsAtoB >= 0f ? "A -> B" : "B -> A")}",
            this);
    }
}


CurrentsResolved?.Invoke(currents);
        }

        private void AddInjection(SparkTerminal terminal, float amps)
        {
            if (terminal != null && injections.TryGetValue(terminal, out float previous))
                injections[terminal] = previous + amps;
        }
    }
}
