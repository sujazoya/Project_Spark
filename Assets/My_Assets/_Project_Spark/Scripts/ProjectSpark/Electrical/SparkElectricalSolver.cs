using System;
using System.Collections.Generic;
using ProjectSpark.Circuit;
using ProjectSpark.Gameplay;
using UnityEngine;

namespace ProjectSpark.Electrical
{
    /// <summary>
    /// Deterministic Project Spark DC network solver.
    /// Supports resistors, switches, piecewise-linear diodes/LEDs,
    /// current-limited supplies, and capacitor transient behavior.
    ///
    /// For two-terminal semiconductor components, terminal 0 is treated as
    /// anode and terminal 1 as cathode.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkElectricalSolver : MonoBehaviour
    {
        [SerializeField] private SparkCircuitSystem circuit;
        [SerializeField] private SparkPowerSupply[] powerSupplies = Array.Empty<SparkPowerSupply>();
        [SerializeField] private SparkElectricalComponent[] electricalComponents = Array.Empty<SparkElectricalComponent>();

        [Header("Solver")]
        [SerializeField, Min(1)] private int maxIterations = 16;
        [SerializeField, Min(0.0000001f)] private float convergenceTolerance = 0.000001f;
        [SerializeField, Min(0.000000001f)] private float minimumResistance = 0.000001f;
        [SerializeField, Min(0f)] private float floatingNodeLeakResistance = 1.0e9f;
        [SerializeField] private bool solveOnDirty = true;

        [Header("Capacitors")]
        [SerializeField] private bool simulateCapacitors = true;
        [SerializeField, Min(0.000001f)] private float editorTimeStep = 1f / 60f;

        private readonly List<SparkCircuitConnection> connections = new();
        private readonly List<SparkTerminal> terminals = new();
        private readonly Dictionary<SparkElectricalComponent, bool> diodeStates = new();
        private readonly Dictionary<SparkCapacitor, float> capacitorPreviousVoltage = new();
        private readonly HashSet<SparkElectricalComponent> subscribedComponents = new();
        private bool dirty = true;

        private int lastSolvedTopologyVersion = -1;

        public event Action SolveCompleted;
        public event Action SolveFailed;
        public bool IsDirty => dirty;

        private void Awake()
{
    if (circuit == null)
        circuit = GetComponent<SparkCircuitSystem>();

    RefreshComponentCache();

    if (circuit != null)
    {
        lastSolvedTopologyVersion =
            circuit.TopologyVersion;

        circuit.TopologyChanged += MarkDirty;
    }
}

        private void OnEnable()
        {
            dirty = true;
            RefreshComponentCache();
        }

        private void OnDisable()
        {
            foreach (var component in subscribedComponents)
                if (component != null) component.ElectricalConfigurationChanged -= MarkDirty;
            subscribedComponents.Clear();
        }

        private void OnDestroy()
        {
            if (circuit != null) circuit.TopologyChanged -= MarkDirty;
            foreach (var component in subscribedComponents)
                if (component != null) component.ElectricalConfigurationChanged -= MarkDirty;
            subscribedComponents.Clear();
        }

       private void Update()
{
    if (circuit != null)
    {
        int currentTopologyVersion =
            circuit.TopologyVersion;

        if (currentTopologyVersion !=
            lastSolvedTopologyVersion)
        {
            dirty = true;
        }
    }

    if (!solveOnDirty &&
        !HasDynamicCapacitor() &&
        !dirty)
    {
        return;
    }

    if (dirty ||
        (simulateCapacitors &&
         HasDynamicCapacitor()))
    {
        Solve();
    }
}

        public void MarkDirty()
{
    dirty = true;

    Debug.Log(
        $"[SPARK SOLVER] TOPOLOGY DIRTY RECEIVED | " +
        $"Circuit={circuit?.name ?? "NULL"} | " +
        $"Version={circuit?.TopologyVersion ?? -1}",
        this);
}
private readonly List<SparkCircuitConnection> connectionBuffer = new();

        public void SolveNow()
        {
            dirty = true;
            Solve();
        }

        public void Solve()
        {
            dirty = false;
            if (circuit != null)
    {
        lastSolvedTopologyVersion =
            circuit.TopologyVersion;
    }
    Debug.Log(
        $"[SPARK SOLVER] SOLVE START | " +
        $"Circuit={circuit?.name ?? "NULL"} | " +
        $"TopologyVersion={circuit?.TopologyVersion ?? -1}",
        this);
            RefreshComponentCache();

            if (circuit == null)
            {
                SolveFailed?.Invoke();
                return;
            }

            circuit.RebuildTopology();

            var graph = BuildGraph();

            if (graph.NodeCount == 0)
            {
                ZeroStates();
                SolveCompleted?.Invoke();
                return;
            }

            var voltages = new float[graph.NodeCount];
            bool converged = false;
            float timeStep = Mathf.Max(0.000001f, Application.isPlaying ? Time.deltaTime : editorTimeStep);

            for (int iteration = 0; iteration < Mathf.Max(1, maxIterations); iteration++)
            {
                if (!SolveResistiveNetwork(graph, voltages, timeStep))
                {
                    ZeroStates();
                    SolveFailed?.Invoke();
                    return;
                }

                float maxDelta = ApplyDeviceStates(graph, voltages);
                if (maxDelta <= convergenceTolerance)
                {
                    converged = true;
                    break;
                }
            }

            if (!converged)
            {
                // The final state is still useful for UI/diagnostics, but report
                // failure so callers can distinguish an unconverged network.
                SolveFailed?.Invoke();
                return;
            }

            CommitTransientState(graph, voltages);
            SolveCompleted?.Invoke();
        }

        private void RefreshComponentCache()
        {
            var components = new List<SparkElectricalComponent>();
            if (electricalComponents != null)
                components.AddRange(electricalComponents);

            var sceneComponents = FindObjectsByType<SparkElectricalComponent>(FindObjectsSortMode.InstanceID);
            for (int i = 0; i < sceneComponents.Length; i++)
                if (!components.Contains(sceneComponents[i])) components.Add(sceneComponents[i]);

            electricalComponents = components.ToArray();

            var supplies = new List<SparkPowerSupply>();
            if (powerSupplies != null)
                supplies.AddRange(powerSupplies);
            for (int i = 0; i < electricalComponents.Length; i++)
                if (electricalComponents[i] is SparkPowerSupply supply && !supplies.Contains(supply)) supplies.Add(supply);
            powerSupplies = supplies.ToArray();

            var current = new HashSet<SparkElectricalComponent>();
            for (int i = 0; i < electricalComponents.Length; i++)
            {
                var component = electricalComponents[i];
                if (component == null) continue;
                current.Add(component);
                if (subscribedComponents.Add(component))
                    component.ElectricalConfigurationChanged += MarkDirty;
            }

            var stale = new List<SparkElectricalComponent>();
            foreach (var component in subscribedComponents)
                if (!current.Contains(component)) stale.Add(component);
            for (int i = 0; i < stale.Count; i++)
            {
                stale[i].ElectricalConfigurationChanged -= MarkDirty;
                subscribedComponents.Remove(stale[i]);
            }
        }

        private bool HasDynamicCapacitor()
        {
            if (!simulateCapacitors) return false;
            for (int i = 0; i < electricalComponents.Length; i++)
                if (electricalComponents[i] is SparkCapacitor capacitor && capacitor.ElectricalEnabled)
                    return true;
            return false;
        }

        private void ZeroStates()
        {
            for (int i = 0; i < electricalComponents.Length; i++)
            {
                var component = electricalComponents[i];
                if (component == null) continue;
                component.ApplyElectricalState(new SparkElectricalState(0, 0, 0, SparkConductionState.NonConducting));
                if (component is SparkCapacitor capacitor) capacitorPreviousVoltage[capacitor] = 0f;
                if (component is SparkDiode || component is SparkLED) diodeStates[component] = false;
                if (component is SparkPowerSupply supply) supply.SetCurrentLimited(false);
            }
            for (int i = 0; i < terminals.Count; i++)
                {
                    if (terminals[i] == null)
                    {
                        continue;
                    }
                terminals[i].ApplyElectricalState(
                    new SparkTerminalElectricalState(
                        0f,
                        0f));

                terminals[i].ClearRuntimePolarity();
                }
        }

        private NetworkGraph BuildGraph()
        {
            terminals.Clear();
            var terminalSet = new HashSet<SparkTerminal>();

            for (int i = 0; i < electricalComponents.Length; i++)
            {
                var component = electricalComponents[i];
                if (component == null) continue;
                var childTerminals = component.GetComponentsInChildren<SparkTerminal>(true);
                for (int t = 0; t < childTerminals.Length; t++) terminalSet.Add(childTerminals[t]);
            }

            // Include terminals participating in the circuit. Do not automatically
            // include unrelated scene terminals; isolated measurement/debug terminals
            // otherwise create singular floating matrices.
            for (int i = 0; i < electricalComponents.Length; i++)
            {
                var component = electricalComponents[i];
                if (component == null) continue;
                var childTerminals = component.GetComponentsInChildren<SparkTerminal>(true);
                for (int t = 0; t < childTerminals.Length; t++)
                {
                    circuit.GetConnections(childTerminals[t], connections);
                    for (int c = 0; c < connections.Count; c++)
                    {
                        terminalSet.Add(connections[c].A);
                        terminalSet.Add(connections[c].B);
                    }
                }
            }

            terminals.AddRange(terminalSet);
            var graph = new NetworkGraph();
            var uf = new UnionFind(terminals.Count);
            var index = new Dictionary<SparkTerminal, int>(terminals.Count);
            for (int i = 0; i < terminals.Count; i++) index[terminals[i]] = i;

            for (int i = 0; i < terminals.Count; i++)
            {
                circuit.GetConnections(terminals[i], connections);
                for (int c = 0; c < connections.Count; c++)
                {
                    var con = connections[c];
                    // A probe is a measurement relationship, not an electrical short.
                    if (con.Kind == SparkConnectionKind.Probe) continue;
                    if (index.TryGetValue(con.A, out var a) && index.TryGetValue(con.B, out var b))
                        uf.Union(a, b);
                }
            }

            for (int i = 0; i < terminals.Count; i++)
            {
                int root = uf.Find(i);
                if (!graph.RootToNode.TryGetValue(root, out int node))
                {
                    node = graph.RootToNode.Count;
                    graph.RootToNode.Add(root, node);
                }
                graph.TerminalNode[terminals[i]] = node;
            }

            graph.NodeCount = graph.RootToNode.Count;

// ============================================================
// DEBUG — GRAPH CONNECTIVITY
// ============================================================

Debug.Log(
    $"[SPARK GRAPH DETAIL] " +
    $"Nodes={graph.NodeCount} | " +
    $"Terminals={terminals.Count}",
    this);

for (int i = 0; i < terminals.Count; i++)
{
    SparkTerminal terminal = terminals[i];

    if (terminal == null)
        continue;

    connectionBuffer.Clear();

    circuit.GetConnections(
        terminal,
        connectionBuffer);

    Debug.Log(
        $"[SPARK GRAPH TERMINAL] " +
        $"{terminal.name} | " +
        $"Owner={terminal.Owner?.name ?? "NULL"} | " +
        $"Connections={connectionBuffer.Count} | " +
        $"Voltage={terminal.ElectricalState.Voltage:F3} | " +
        $"Polarity={terminal.EffectivePolarity}",
        terminal);

    for (int c = 0; c < connectionBuffer.Count; c++)
    {
        SparkCircuitConnection connection =
            connectionBuffer[c];

        SparkTerminal other =
    connection.A == terminal
        ? connection.B
        : connection.A;

        Debug.Log(
            $"[SPARK GRAPH CONNECTION] " +
            $"{terminal.name} -> " +
            $"{other?.name ?? "NULL"} | " +
            $"Kind={connection.Kind} | " +
            $"Valid={connection.IsValid}",
            terminal);
    }
}



            return graph;
        }

        private bool SolveResistiveNetwork(NetworkGraph graph, float[] voltages, float timeStep)
        {
            int n = graph.NodeCount;
            if (n == 0) return true;
            int reference = FindGroundNode(graph);
            if (reference < 0) reference = 0;
            int m = n - 1;
            if (m <= 0)
            {
                voltages[0] = 0f;
                return true;
            }

            var A = new double[m, m];
            var b = new double[m];

            // Tiny leakage gives otherwise-floating nodes a stable numerical reference
            // without materially affecting normal circuit values.
            double leakG = floatingNodeLeakResistance > 0f
                ? 1.0 / Math.Max(minimumResistance, floatingNodeLeakResistance)
                : 0.0;
            if (leakG > 0.0)
                for (int i = 0; i < m; i++) A[i, i] += leakG;

            for (int i = 0; i < electricalComponents.Length; i++)
            {
                var component = electricalComponents[i];
                if (component == null || !component.ElectricalEnabled) continue;
                if (!TryGetTwoTerminals(component, out var ta, out var tb)) continue;
                if (!graph.TerminalNode.TryGetValue(ta, out int na) || !graph.TerminalNode.TryGetValue(tb, out int nb)) continue;

                int ai = MapNode(na, reference);
                int bi = MapNode(nb, reference);

                if (component is SparkResistor resistor)
                {
                    AddConductance(A, b, ai, bi, 1.0 / Math.Max(resistor.ResistanceOhms, minimumResistance));
                }
               else if (component is SparkSwitch sw)
                {
                    if (sw.IsConducting)
                    {
                        double resistance = Math.Max(
                            minimumResistance,
                            sw.ClosedResistance);

                        double conductance = 1.0 / resistance;

                        AddConductance(
                            A,
                            b,
                            ai,
                            bi,
                            conductance);
                    }
                }
                else if (component is SparkDiode diode)
                {
                    bool on = GetDiodeState(diode, ta, tb, graph, voltages);
                    if (on)
                        StampForwardDrop(A, b, ai, bi, diode.ForwardVoltage, diode.OnResistance);
                }
                else if (component is SparkLED led)
                {
                    bool on = GetDiodeState(led, ta, tb, graph, voltages);
                    if (on)
                        StampForwardDrop(A, b, ai, bi, led.ForwardVoltage, led.OnResistance);
                }
                else if (component is SparkCapacitor capacitor && simulateCapacitors)
                {
                    double g = Math.Max(1.0e-12, capacitor.CapacitanceFarads / Math.Max(timeStep, 1.0e-6f));
                    capacitorPreviousVoltage.TryGetValue(capacitor, out float previousVoltage);
                    AddConductance(A, b, ai, bi, g);
                    // Backward-Euler companion model: i = G*V - G*Vprevious.
                    AddCurrentSource(A, b, ai, bi, -g * previousVoltage);
                }
            }

            // Current-limited supply: Thevenin equivalent. Below the limit it behaves
            // close to an ideal source; at a short it cannot exceed CurrentLimit.
            for (int i = 0; i < powerSupplies.Length; i++)
            {
                var supply = powerSupplies[i];
                if (supply == null || !supply.IsOutputActive || !supply.ElectricalEnabled) continue;
                if (!TryGetTwoTerminals(supply, out var positive, out var negative)) continue;
                if (!graph.TerminalNode.TryGetValue(positive, out int pNode) || !graph.TerminalNode.TryGetValue(negative, out int nNode)) continue;

                float seriesResistance = Mathf.Max(minimumResistance, supply.OutputVoltage / Mathf.Max(0.0001f, supply.CurrentLimit));
                AddConductance(A, b, MapNode(pNode, reference), MapNode(nNode, reference),
                    1.0 / seriesResistance, supply.OutputVoltage);
            }

            var x = GaussianSolve(A, b);
            if (x == null) return false;

            for (int node = 0, k = 0; node < n; node++)
                voltages[node] = node == reference ? 0f : (float)x[k++];

            return true;
        }

        private bool GetDiodeState(SparkElectricalComponent component, SparkTerminal anode, SparkTerminal cathode,
            NetworkGraph graph, float[] currentVoltages)
        {
            if (!diodeStates.TryGetValue(component, out bool on))
                on = component.ElectricalState.Conduction == SparkConductionState.Conducting;

            if (!graph.TerminalNode.TryGetValue(anode, out int a) || !graph.TerminalNode.TryGetValue(cathode, out int c))
                return false;

            float voltage = currentVoltages[a] - currentVoltages[c];
            float forwardVoltage = component is SparkDiode diode ? diode.ForwardVoltage : ((SparkLED)component).ForwardVoltage;

            // Active-set behavior. An initially-off diode is allowed to turn on as soon
            // as the solved node voltage reaches its forward threshold.
            if (on)
                on = voltage >= forwardVoltage - 0.005f;
            else
                on = voltage >= forwardVoltage;

            diodeStates[component] = on;
            return on;
        }

        private float ApplyDeviceStates(NetworkGraph graph, float[] voltages)        
{
    float maxDelta = 0f;

    for (int i = 0;
         i < electricalComponents.Length;
         i++)
    {
        SparkElectricalComponent component =
            electricalComponents[i];

        if (component == null)
            continue;

        if (!TryGetTwoTerminals(
                component,
                out SparkTerminal ta,
                out SparkTerminal tb))
        {
            continue;
        }

        if (!graph.TerminalNode.TryGetValue(
                ta,
                out int na))
        {
            continue;
        }

        if (!graph.TerminalNode.TryGetValue(
                tb,
                out int nb))
        {
            continue;
        }

        float voltage =
            voltages[na] -
            voltages[nb];

        float current = 0f;

        // ------------------------------------------------------------
        // COMPONENT CURRENT
        // ------------------------------------------------------------

        if (component is SparkResistor resistor)
        {
            current =
                voltage /
                Mathf.Max(
                    resistor.ResistanceOhms,
                    minimumResistance);
        }
        else if (component is SparkSwitch sw)
        {
            if (sw.IsConducting)
            {
                float resistance =
                    Mathf.Max(
                        minimumResistance,
                        sw.ClosedResistance);

                current =
                    voltage /
                    resistance;
            }
            else
            {
                current = 0f;
            }
        }
        else if (component is SparkDiode diode)
        {
            bool wasOn =
                diodeStates.TryGetValue(
                    diode,
                    out bool diodeState) &&
                diodeState;

            bool on =
                wasOn
                    ? voltage >=
                      diode.ForwardVoltage - 0.005f
                    : voltage >=
                      diode.ForwardVoltage;

            if (on)
            {
                current =
                    Mathf.Max(
                        0f,
                        (voltage -
                         diode.ForwardVoltage) /
                        diode.OnResistance);
            }

            diodeStates[diode] =
                on && current > 0f;
        }
        else if (component is SparkLED led)
        {
            bool wasOn =
                diodeStates.TryGetValue(
                    led,
                    out bool ledState) &&
                ledState;

            bool on =
                wasOn
                    ? voltage >=
                      led.ForwardVoltage - 0.005f
                    : voltage >=
                      led.ForwardVoltage;

            if (on)
            {
                current =
                    Mathf.Max(
                        0f,
                        (voltage -
                         led.ForwardVoltage) /
                        led.OnResistance);
            }

            diodeStates[led] =
                on && current > 0f;
        }
        else if (component is SparkCapacitor capacitor &&
                 simulateCapacitors)
        {
            capacitorPreviousVoltage.TryGetValue(
                capacitor,
                out float previousVoltage);

            current =
                capacitor.CapacitanceFarads *
                (voltage - previousVoltage) /
                Mathf.Max(
                    0.000001f,
                    Application.isPlaying
                        ? Time.deltaTime
                        : editorTimeStep);
        }
        else if (component is SparkPowerSupply supply &&
                 supply.IsOutputActive)
        {
            float seriesResistance =
                Mathf.Max(
                    minimumResistance,
                    supply.OutputVoltage /
                    Mathf.Max(
                        0.0001f,
                        supply.CurrentLimit));

            current =
                Mathf.Max(
                    0f,
                    (supply.OutputVoltage - voltage) /
                    seriesResistance);

            supply.SetCurrentLimited(
                current >=
                supply.CurrentLimit * 0.999f);
        }

        float power =
            voltage * current;

        if (component is SparkPowerSupply)
            power = -Mathf.Abs(power);

        // ------------------------------------------------------------
        // PREVIOUS COMPONENT STATE
        // ------------------------------------------------------------

        SparkElectricalState previous =
            component.ElectricalState;

        // ------------------------------------------------------------
        // COMPONENT STATE
        // ------------------------------------------------------------

        SparkConductionState conduction =
            Mathf.Abs(current) > 0.000001f
                ? SparkConductionState.Conducting
                : SparkConductionState.NonConducting;

        SparkElectricalState state =
            new SparkElectricalState(
                voltage,
                current,
                power,
                conduction);

        // ------------------------------------------------------------
        // TERMINAL STATES FIRST
        // ------------------------------------------------------------

        float voltageA =
            voltages[na];

        float voltageB =
            voltages[nb];

        SparkTerminalElectricalState previousA =
            ta.ElectricalState;

        SparkTerminalElectricalState previousB =
            tb.ElectricalState;

        ApplyTerminalElectricalStates(
            voltages,
            ta,
            tb,
            na,
            nb,
            current);

        // ------------------------------------------------------------
        // COMPONENT STATE SECOND
        // ------------------------------------------------------------

        component.ApplyElectricalState(
            state);

        // ------------------------------------------------------------
        // SWITCH POLARITY THIRD
        // ------------------------------------------------------------

        if (component is SparkSwitch sparkSwitch)
        {
            sparkSwitch.RefreshRuntimePolarity();
        }

        // ------------------------------------------------------------
        // COMPONENT CONVERGENCE
        // ------------------------------------------------------------

        maxDelta =
            Mathf.Max(
                maxDelta,
                Mathf.Abs(
                    previous.Voltage -
                    state.Voltage));

        maxDelta =
            Mathf.Max(
                maxDelta,
                Mathf.Abs(
                    previous.Current -
                    state.Current));

        // ------------------------------------------------------------
        // TERMINAL CONVERGENCE
        // ------------------------------------------------------------

        maxDelta =
            Mathf.Max(
                maxDelta,
                Mathf.Abs(
                    previousA.Voltage -
                    voltageA));

        maxDelta =
            Mathf.Max(
                maxDelta,
                Mathf.Abs(
                    previousB.Voltage -
                    voltageB));
    }

    return maxDelta;

        }

        private static void ApplyTerminalElectricalStates(
    float[] voltages,
    SparkTerminal terminalA,
    SparkTerminal terminalB,
    int nodeA,
    int nodeB,
    float current)
{
    if (voltages == null ||
        terminalA == null ||
        terminalB == null)
    {
        return;
    }

    if (nodeA < 0 ||
        nodeA >= voltages.Length ||
        nodeB < 0 ||
        nodeB >= voltages.Length)
    {
        return;
    }

    float voltageA =
        voltages[nodeA];

    float voltageB =
        voltages[nodeB];

    terminalA.ApplyElectricalState(
        new SparkTerminalElectricalState(
            voltageA,
            current));

    terminalB.ApplyElectricalState(
        new SparkTerminalElectricalState(
            voltageB,
            -current));
}

        private void CommitTransientState(NetworkGraph graph, float[] voltages)
        {
            if (!simulateCapacitors) return;
            for (int i = 0; i < electricalComponents.Length; i++)
            {
                if (!(electricalComponents[i] is SparkCapacitor capacitor)) continue;
                if (!TryGetTwoTerminals(capacitor, out var a, out var b)) continue;
                if (!graph.TerminalNode.TryGetValue(a, out int na) || !graph.TerminalNode.TryGetValue(b, out int nb)) continue;
                capacitorPreviousVoltage[capacitor] = voltages[na] - voltages[nb];
            }
        }

        private int FindGroundNode(NetworkGraph graph)
        {
            for (int i = 0; i < terminals.Count; i++)
                if (terminals[i].Kind == SparkTerminalKind.Ground && graph.TerminalNode.TryGetValue(terminals[i], out var node))
                    return node;
            return -1;
        }

        private static bool TryGetTwoTerminals(
    Component component,
    out SparkTerminal a,
    out SparkTerminal b)
{
    a = null;
    b = null;

    if (component == null)
    {
        return false;
    }

    if (component is SparkSwitch sparkSwitch)
    {
        a = sparkSwitch.InputTerminal;
        b = sparkSwitch.OutputTerminal;

        return a != null &&
               b != null;
    }

    if (component is SparkLED sparkLED)
    {
        a = sparkLED.AnodeTerminal;
        b = sparkLED.CathodeTerminal;

        return a != null &&
               b != null;
    }

    var childTerminals =
        component.GetComponentsInChildren<SparkTerminal>(true);

    if (childTerminals.Length < 2)
    {
        return false;
    }

    a = childTerminals[0];
    b = childTerminals[1];

    return a != null &&
           b != null;
}

        private static int MapNode(int node, int reference) => node == reference ? -1 : (node < reference ? node : node - 1);

        private static void AddConductance(double[,] A, double[] b, int ai, int bi, double g, double voltage = 0.0)
        {
            if (g <= 0.0 || double.IsNaN(g) || double.IsInfinity(g)) return;
            if (ai >= 0) { A[ai, ai] += g; b[ai] += g * voltage; }
            if (bi >= 0) { A[bi, bi] += g; b[bi] -= g * voltage; }
            if (ai >= 0 && bi >= 0) { A[ai, bi] -= g; A[bi, ai] -= g; }
        }

        private static void StampForwardDrop(double[,] A, double[] b, int ai, int bi, float forwardVoltage, float resistance)
        {
            // I = (Vab - Vf) / R  =>  G*Vab - G*Vf.
            AddConductance(A, b, ai, bi, 1.0 / Math.Max(0.01f, resistance), forwardVoltage);
        }

        private static void AddCurrentSource(double[,] A, double[] b, int ai, int bi, double currentFromAtoB)
        {
            // Current source I from A -> B contributes +I on A and -I on B in MNA/KCL form.
            if (ai >= 0) b[ai] -= currentFromAtoB;
            if (bi >= 0) b[bi] += currentFromAtoB;
        }

        private static double[] GaussianSolve(double[,] a, double[] b)
        {
            int n = b.Length;
            for (int i = 0; i < n; i++)
            {
                int pivot = i;
                double best = Math.Abs(a[i, i]);
                for (int r = i + 1; r < n; r++)
                {
                    double value = Math.Abs(a[r, i]);
                    if (value > best) { best = value; pivot = r; }
                }

                if (best < 1.0e-15) return null;
                if (pivot != i)
                {
                    for (int c = i; c < n; c++) (a[i, c], a[pivot, c]) = (a[pivot, c], a[i, c]);
                    (b[i], b[pivot]) = (b[pivot], b[i]);
                }

                double diagonal = a[i, i];
                for (int c = i; c < n; c++) a[i, c] /= diagonal;
                b[i] /= diagonal;

                for (int r = i + 1; r < n; r++)
                {
                    double factor = a[r, i];
                    if (Math.Abs(factor) < 1.0e-18) continue;
                    for (int c = i; c < n; c++) a[r, c] -= factor * a[i, c];
                    b[r] -= factor * b[i];
                }
            }

            var x = new double[n];
            for (int i = n - 1; i >= 0; i--)
            {
                double sum = b[i];
                for (int c = i + 1; c < n; c++) sum -= a[i, c] * x[c];
                x[i] = sum;
            }
            return x;
        }

        private sealed class NetworkGraph
        {
            public int NodeCount;
            public readonly Dictionary<int, int> RootToNode = new();
            public readonly Dictionary<SparkTerminal, int> TerminalNode = new();
        }

        private sealed class UnionFind
        {
            private readonly int[] parent;
            private readonly byte[] rank;
            public UnionFind(int n)
            {
                parent = new int[n];
                rank = new byte[n];
                for (int i = 0; i < n; i++) parent[i] = i;
            }
            public int Find(int x)
            {
                while (parent[x] != x)
                {
                    parent[x] = parent[parent[x]];
                    x = parent[x];
                }
                return x;
            }
            public void Union(int a, int b)
            {
                a = Find(a); b = Find(b);
                if (a == b) return;
                if (rank[a] < rank[b]) (a, b) = (b, a);
                parent[b] = a;
                if (rank[a] == rank[b]) rank[a]++;
            }
        }
    }
}
