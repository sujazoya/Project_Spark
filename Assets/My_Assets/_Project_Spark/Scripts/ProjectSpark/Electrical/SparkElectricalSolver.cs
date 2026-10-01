using System;
using System.Collections.Generic;
using ProjectSpark.Circuit;
using ProjectSpark.Gameplay;
using UnityEngine;
using ProjectSpark.Measurement;

namespace ProjectSpark.Electrical
{
    /// <summary>
    /// Deterministic Project Spark DC network solver.
    ///
    /// Supports:
    /// - Resistors
    /// - Binary SparkSwitch
    /// - Indexed SparkSwitchIndex
    /// - Piecewise-linear diodes / LEDs
    /// - Current-limited power supplies
    /// - Capacitor transient behavior
    /// - Multimeter internal shunt
    /// - Hair dryer motor / heater
    ///
    /// SparkSwitchIndex is solved as a controlled voltage source:
    ///
    ///     Vout = Vin * IndexVoltagePercent / 100
    ///
    /// Therefore:
    ///
    ///     Index 0 = OFF
    ///     Index 1 = 50%
    ///     Index 2 = 100%
    ///
    /// Example with 230 V input:
    ///
    ///     Index 0 = 0 V
    ///     Index 1 = 115 V
    ///     Index 2 = 230 V
    ///
    /// This is a DC simulation model. It does not model real AC mains
    /// waveform behavior.
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

        private readonly Dictionary<SparkSwitchIndex, float> indexedSwitchCurrents = new();

        private readonly HashSet<SparkElectricalComponent> subscribedComponents = new();

        private readonly List<SparkCircuitConnection> connectionBuffer = new();

        private bool dirty = true;

        private int lastSolvedTopologyVersion = -1;

        public event Action SolveCompleted;
        public event Action SolveFailed;

        public bool IsDirty => dirty;

        // ============================================================
        // LIFECYCLE
        // ============================================================

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
            {
                if (component != null)
                    component.ElectricalConfigurationChanged -= MarkDirty;
            }

            subscribedComponents.Clear();
        }

        private void OnDestroy()
        {
            if (circuit != null)
                circuit.TopologyChanged -= MarkDirty;

            foreach (var component in subscribedComponents)
            {
                if (component != null)
                    component.ElectricalConfigurationChanged -= MarkDirty;
            }

            subscribedComponents.Clear();
        }

        private void Update()
        {
            if (circuit != null)
            {
                int currentTopologyVersion =
                    circuit.TopologyVersion;

                if (currentTopologyVersion != lastSolvedTopologyVersion)
                    dirty = true;
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

        // ============================================================
        // DIRTY / SOLVE ENTRY
        // ============================================================

        public void MarkDirty()
        {
            dirty = true;
        }

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

            float timeStep =
                Mathf.Max(
                    0.000001f,
                    Application.isPlaying
                        ? Time.deltaTime
                        : editorTimeStep);

            for (int iteration = 0;
                 iteration < Mathf.Max(1, maxIterations);
                 iteration++)
            {
                if (!SolveResistiveNetwork(
                        graph,
                        voltages,
                        timeStep))
                {
                    ZeroStates();
                    SolveFailed?.Invoke();
                    return;
                }

                float maxDelta =
                    ApplyDeviceStates(
                        graph,
                        voltages);

                if (maxDelta <= convergenceTolerance)
                {
                    converged = true;
                    break;
                }
            }

            if (!converged)
            {
                SolveFailed?.Invoke();
                return;
            }

            CommitTransientState(
                graph,
                voltages);

            // Existing project behavior retained.
            // SolveCompleted?.Invoke();
        }

        // ============================================================
        // COMPONENT CACHE
        // ============================================================

        private void RefreshComponentCache()
        {
            var components =
                new List<SparkElectricalComponent>();

            if (electricalComponents != null)
                components.AddRange(electricalComponents);

            var sceneComponents =
                FindObjectsByType<SparkElectricalComponent>(
                    FindObjectsSortMode.InstanceID);

            for (int i = 0;
                 i < sceneComponents.Length;
                 i++)
            {
                if (!components.Contains(sceneComponents[i]))
                    components.Add(sceneComponents[i]);
            }

            electricalComponents =
                components.ToArray();

            var supplies =
                new List<SparkPowerSupply>();

            if (powerSupplies != null)
                supplies.AddRange(powerSupplies);

            for (int i = 0;
                 i < electricalComponents.Length;
                 i++)
            {
                if (electricalComponents[i] is SparkPowerSupply supply &&
                    !supplies.Contains(supply))
                {
                    supplies.Add(supply);
                }
            }

            powerSupplies =
                supplies.ToArray();

            var current =
                new HashSet<SparkElectricalComponent>();

            for (int i = 0;
                 i < electricalComponents.Length;
                 i++)
            {
                var component =
                    electricalComponents[i];

                if (component == null)
                    continue;

                current.Add(component);

                if (subscribedComponents.Add(component))
                {
                    component.ElectricalConfigurationChanged +=
                        MarkDirty;
                }
            }

            var stale =
                new List<SparkElectricalComponent>();

            foreach (var component in subscribedComponents)
            {
                if (!current.Contains(component))
                    stale.Add(component);
            }

            for (int i = 0;
                 i < stale.Count;
                 i++)
            {
                stale[i].ElectricalConfigurationChanged -=
                    MarkDirty;

                subscribedComponents.Remove(
                    stale[i]);
            }
        }

        private bool HasDynamicCapacitor()
        {
            if (!simulateCapacitors)
                return false;

            for (int i = 0;
                 i < electricalComponents.Length;
                 i++)
            {
                if (electricalComponents[i] is SparkCapacitor capacitor &&
                    capacitor.ElectricalEnabled)
                {
                    return true;
                }
            }

            return false;
        }

        // ============================================================
        // ZERO STATES
        // ============================================================

        private void ZeroStates()
        {
            indexedSwitchCurrents.Clear();

            for (int i = 0;
                 i < electricalComponents.Length;
                 i++)
            {
                var component =
                    electricalComponents[i];

                if (component == null)
                    continue;

                component.ApplyElectricalState(
                    new SparkElectricalState(
                        0f,
                        0f,
                        0f,
                        SparkConductionState.NonConducting));

                if (component is SparkCapacitor capacitor)
                    capacitorPreviousVoltage[capacitor] = 0f;

                if (component is SparkDiode ||
                    component is SparkLED)
                {
                    diodeStates[component] = false;
                }

                if (component is SparkPowerSupply supply)
                    supply.SetCurrentLimited(false);
            }

            for (int i = 0;
                 i < terminals.Count;
                 i++)
            {
                if (terminals[i] == null)
                    continue;

                terminals[i].ApplyElectricalState(
                    new SparkTerminalElectricalState(
                        0f,
                        0f));

                terminals[i].ClearRuntimePolarity();
            }
        }

        // ============================================================
        // GRAPH
        // ============================================================

        private NetworkGraph BuildGraph()
        {
            terminals.Clear();

            var terminalSet =
                new HashSet<SparkTerminal>();

            for (int i = 0;
                 i < electricalComponents.Length;
                 i++)
            {
                var component =
                    electricalComponents[i];

                if (component == null)
                    continue;

                var childTerminals =
                    component.GetComponentsInChildren<SparkTerminal>(
                        true);

                for (int t = 0;
                     t < childTerminals.Length;
                     t++)
                {
                    terminalSet.Add(
                        childTerminals[t]);
                }
            }

            // Include terminals participating in the circuit.
            for (int i = 0;
                 i < electricalComponents.Length;
                 i++)
            {
                var component =
                    electricalComponents[i];

                if (component == null)
                    continue;

                var childTerminals =
                    component.GetComponentsInChildren<SparkTerminal>(
                        true);

                for (int t = 0;
                     t < childTerminals.Length;
                     t++)
                {
                    circuit.GetConnections(
                        childTerminals[t],
                        connections);

                    for (int c = 0;
                         c < connections.Count;
                         c++)
                    {
                        terminalSet.Add(
                            connections[c].A);

                        terminalSet.Add(
                            connections[c].B);
                    }
                }
            }

            terminals.AddRange(terminalSet);

            var graph =
                new NetworkGraph();

            var uf =
                new UnionFind(
                    terminals.Count);

            var index =
                new Dictionary<SparkTerminal, int>(
                    terminals.Count);

            for (int i = 0;
                 i < terminals.Count;
                 i++)
            {
                index[terminals[i]] = i;
            }

            for (int i = 0;
                 i < terminals.Count;
                 i++)
            {
                circuit.GetConnections(
                    terminals[i],
                    connections);

                for (int c = 0;
                     c < connections.Count;
                     c++)
                {
                    var con =
                        connections[c];

                    // Probe is measurement relationship,
                    // not electrical short.
                    if (con.Kind ==
                        SparkConnectionKind.Probe)
                    {
                        continue;
                    }

                    if (index.TryGetValue(
                            con.A,
                            out var a) &&
                        index.TryGetValue(
                            con.B,
                            out var b))
                    {
                        uf.Union(a, b);
                    }
                }
            }

            for (int i = 0;
                 i < terminals.Count;
                 i++)
            {
                int root =
                    uf.Find(i);

                if (!graph.RootToNode.TryGetValue(
                        root,
                        out int node))
                {
                    node =
                        graph.RootToNode.Count;

                    graph.RootToNode.Add(
                        root,
                        node);
                }

                graph.TerminalNode[
                    terminals[i]] = node;
            }

            graph.NodeCount =
                graph.RootToNode.Count;



#if UNITY_EDITOR || DEVELOPMENT_BUILD

for (int i = 0; i < powerSupplies.Length; i++)
{
    SparkPowerSupply supply = powerSupplies[i];

    if (supply == null)
        continue;

    if (graph.TerminalNode.TryGetValue(
            supply.PositiveTerminal,
            out int positiveNode) &&
        graph.TerminalNode.TryGetValue(
            supply.NegativeTerminal,
            out int negativeNode))
    {
        Debug.Log(
            $"[SUPPLY GRAPH] " +
            $"{supply.name} | " +
            $"PositiveNode={positiveNode} | " +
            $"NegativeNode={negativeNode} | " +
            $"SameNode={positiveNode == negativeNode}");
    }
}

for (int i = 0; i < electricalComponents.Length; i++)
{
    if (!(electricalComponents[i]
        is SparkHairDryerMotorElectrical motor))
    {
        continue;
    }

    if (graph.TerminalNode.TryGetValue(
            motor.LiveTerminal,
            out int liveNode) &&
        graph.TerminalNode.TryGetValue(
            motor.NeutralTerminal,
            out int neutralNode))
    {
        Debug.Log(
            $"[MOTOR GRAPH] " +
            $"LiveNode={liveNode} | " +
            $"NeutralNode={neutralNode} | " +
            $"SameNode={liveNode == neutralNode}");
    }
}

for (int i = 0; i < electricalComponents.Length; i++)
{
    if (!(electricalComponents[i]
        is SparkHairDryerMotorElectrical motor))
    {
        continue;
    }

    bool liveFound =
        graph.TerminalNode.TryGetValue(
            motor.LiveTerminal,
            out int liveNode);

    bool neutralFound =
        graph.TerminalNode.TryGetValue(
            motor.NeutralTerminal,
            out int neutralNode);

    Debug.Log(
        $"[MOTOR GRAPH] {motor.name} | " +
        $"LiveTerminal={motor.LiveTerminal?.name} | " +
        $"NeutralTerminal={motor.NeutralTerminal?.name} | " +
        $"LiveFound={liveFound} | " +
        $"NeutralFound={neutralFound} | " +
        $"LiveNode={liveNode} | " +
        $"NeutralNode={neutralNode} | " +
        $"SameNode={liveFound && neutralFound && liveNode == neutralNode}");
}

#endif










            return graph;
        }

        // ============================================================
        // MAIN NETWORK SOLVER
        // ============================================================

        private bool SolveResistiveNetwork(
            NetworkGraph graph,
            float[] voltages,
            float timeStep)
        {
            int n =
                graph.NodeCount;

            if (n == 0)
                return true;

            int reference =
                FindGroundNode(graph);

            if (reference < 0)
                reference = 0;

            int m =
                n - 1;

            if (m <= 0)
            {
                voltages[0] = 0f;
                indexedSwitchCurrents.Clear();
                return true;
            }

            // ========================================================
            // COUNT ACTIVE INDEXED VOLTAGE SOURCES
            // ========================================================

            int indexedSourceCount =
                CountActiveIndexedSwitches(
                    graph);

            int matrixSize =
                m + indexedSourceCount;

            var A =
                new double[matrixSize, matrixSize];

            var b =
                new double[matrixSize];

            // ========================================================
            // FLOATING NODE LEAK
            // ========================================================

            double leakG =
                floatingNodeLeakResistance > 0f
                    ? 1.0 /
                      Math.Max(
                          minimumResistance,
                          floatingNodeLeakResistance)
                    : 0.0;

            if (leakG > 0.0)
            {
                for (int i = 0;
                     i < m;
                     i++)
                {
                    A[i, i] += leakG;
                }
            }

            // ========================================================
            // ELECTRICAL COMPONENTS
            // ========================================================

            for (int i = 0;
                 i < electricalComponents.Length;
                 i++)
            {
                var component =
                    electricalComponents[i];

                if (component == null ||
                    !component.ElectricalEnabled)
                {
                    continue;
                }

                if (!TryGetTwoTerminals(
                        component,
                        out SparkTerminal ta,
                        out SparkTerminal tb))
                {
                    continue;
                }

                if (!graph.TerminalNode.TryGetValue(
                        ta,
                        out int na) ||
                    !graph.TerminalNode.TryGetValue(
                        tb,
                        out int nb))
                {
                    continue;
                }

                int ai =
                    MapNode(
                        na,
                        reference);

                int bi =
                    MapNode(
                        nb,
                        reference);

                // ====================================================
                // INDEXED SWITCH
                //
                // Vout = Vin * alpha
                //
                // This is stamped as a controlled voltage source.
                // ====================================================

                if (component is SparkSwitchIndex indexedSwitch)
                {
                    if (!indexedSwitch.IsConducting)
                        continue;

                    float alpha =
                        Mathf.Clamp01(
                            indexedSwitch.CurrentIndexVoltagePercent /
                            100f);

                    if (alpha <= 0f)
                        continue;

                    int sourceIndex =
                        GetIndexedSourceMatrixIndex(
                            graph,
                            indexedSwitch,
                            reference);

                    if (sourceIndex >= 0)
                    {
                        StampControlledVoltageSource(
                            A,
                            b,
                            ai,
                            bi,
                            sourceIndex,
                            alpha);
                    }

                    continue;
                }

                // ====================================================
                // MULTIMETER INTERNAL CURRENT SHUNT
                // ====================================================

                if (component is SparkMultimeterElectricalComponent multimeter)
                {
                    double resistance =
                        Math.Max(
                            minimumResistance,
                            multimeter.ShuntResistanceOhms);

                    double conductance =
                        1.0 / resistance;

                    AddConductance(
                        A,
                        b,
                        ai,
                        bi,
                        conductance);
                }

                // ====================================================
                // RESISTOR
                // ====================================================

                else if (component is SparkResistor resistor)
                {
                    double resistance =
                        Math.Max(
                            resistor.ResistanceOhms,
                            minimumResistance);

                    AddConductance(
                        A,
                        b,
                        ai,
                        bi,
                        1.0 / resistance);
                }

                // ====================================================
                // BINARY SWITCH
                // ====================================================

                else if (component is SparkSwitch sw)
                {
                    if (sw.IsConducting)
                    {
                        double resistance =
                            Math.Max(
                                minimumResistance,
                                sw.ClosedResistance);

                        double conductance =
                            1.0 / resistance;

                        AddConductance(
                            A,
                            b,
                            ai,
                            bi,
                            conductance);
                    }
                }

                // ====================================================
                // HAIR DRYER MOTOR
                // ====================================================

                else if (component is SparkHairDryerMotorElectrical motor)
                {
                    double resistance =
                        Math.Max(
                            minimumResistance,
                            motor.Resistance);

                    AddConductance(
                        A,
                        b,
                        ai,
                        bi,
                        1.0 / resistance);
                }

                // ====================================================
                // HAIR DRYER HEATER
                // ====================================================

                else if (component is SparkHairDryerHeaterElectrical heater)
                {
                    double resistance =
                        Math.Max(
                            minimumResistance,
                            heater.Resistance);

                    AddConductance(
                        A,
                        b,
                        ai,
                        bi,
                        1.0 / resistance);
                }

                // ====================================================
                // DIODE
                // ====================================================

                else if (component is SparkDiode diode)
                {
                    bool on =
                        GetDiodeState(
                            diode,
                            ta,
                            tb,
                            graph,
                            voltages);

                    if (on)
                    {
                        StampForwardDrop(
                            A,
                            b,
                            ai,
                            bi,
                            diode.ForwardVoltage,
                            diode.OnResistance);
                    }
                }

                // ====================================================
                // LED
                // ====================================================

                else if (component is SparkLED led)
                {
                    bool on =
                        GetDiodeState(
                            led,
                            ta,
                            tb,
                            graph,
                            voltages);

                    if (on)
                    {
                        StampForwardDrop(
                            A,
                            b,
                            ai,
                            bi,
                            led.ForwardVoltage,
                            led.OnResistance);
                    }
                }

                // ====================================================
                // CAPACITOR
                // ====================================================

                else if (component is SparkCapacitor capacitor &&
                         simulateCapacitors)
                {
                    double g =
                        Math.Max(
                            1.0e-12,
                            capacitor.CapacitanceFarads /
                            Math.Max(
                                timeStep,
                                1.0e-6f));

                    capacitorPreviousVoltage.TryGetValue(
                        capacitor,
                        out float previousVoltage);

                    AddConductance(
                        A,
                        b,
                        ai,
                        bi,
                        g);

                    AddCurrentSource(
                        A,
                        b,
                        ai,
                        bi,
                        -g * previousVoltage);
                }
            }

            // ========================================================
            // POWER SUPPLIES
            // ========================================================

            for (int i = 0;
                 i < powerSupplies.Length;
                 i++)
            {
                SparkPowerSupply supply =
                    powerSupplies[i];

                if (supply == null ||
                    !supply.IsOutputActive ||
                    !supply.ElectricalEnabled)
                {
                    continue;
                }

                if (!TryGetTwoTerminals(
                        supply,
                        out SparkTerminal positive,
                        out SparkTerminal negative))
                {
                    continue;
                }

                if (!graph.TerminalNode.TryGetValue(
                        positive,
                        out int pNode) ||
                    !graph.TerminalNode.TryGetValue(
                        negative,
                        out int nNode))
                {
                    continue;
                }

                float seriesResistance;

                if (supply.IsCurrentLimited)
                {
                    seriesResistance =
                        Mathf.Max(
                            minimumResistance,
                            supply.OutputVoltage /
                            Mathf.Max(
                                0.0001f,
                                supply.CurrentLimit));
                }
                else
                {
                    seriesResistance =
                        minimumResistance;
                }

                AddConductance(
                    A,
                    b,
                    MapNode(
                        pNode,
                        reference),
                    MapNode(
                        nNode,
                        reference),
                    1.0 / seriesResistance,
                    supply.OutputVoltage);
            }

            // ========================================================
            // SOLVE MNA MATRIX
            // ========================================================

            var x =
                GaussianSolve(
                    A,
                    b);

            if (x == null)
                return false;

            // ========================================================
            // COPY NODE VOLTAGES
            // ========================================================

            for (int node = 0, k = 0;
                 node < n;
                 node++)
            {
                voltages[node] =
                    node == reference
                        ? 0f
                        : (float)x[k++];
            }

            // ========================================================
            // READ INDEXED SWITCH SOURCE CURRENTS
            // ========================================================

            indexedSwitchCurrents.Clear();

            int sourceCounter = 0;

            for (int i = 0;
                 i < electricalComponents.Length;
                 i++)
            {
                if (!(electricalComponents[i]
                      is SparkSwitchIndex indexedSwitch))
                {
                    continue;
                }

                if (!indexedSwitch.ElectricalEnabled ||
                    !indexedSwitch.IsConducting)
                {
                    continue;
                }

                float alpha =
                    Mathf.Clamp01(
                        indexedSwitch.CurrentIndexVoltagePercent /
                        100f);

                if (alpha <= 0f)
                    continue;

                if (!TryGetTwoTerminals(
                        indexedSwitch,
                        out SparkTerminal input,
                        out SparkTerminal output))
                {
                    continue;
                }

                if (!graph.TerminalNode.TryGetValue(
                        input,
                        out int inputNode) ||
                    !graph.TerminalNode.TryGetValue(
                        output,
                        out int outputNode))
                {
                    continue;
                }

                int sourceIndex =
                    m + sourceCounter;

                sourceCounter++;

                if (sourceIndex >= 0 &&
                    sourceIndex < x.Length)
                {
                    indexedSwitchCurrents[indexedSwitch] =
                        (float)x[sourceIndex];
                }
            }

            return true;
        }

        // ============================================================
        // INDEXED SWITCH SOURCE COUNT
        // ============================================================

        private int CountActiveIndexedSwitches(
            NetworkGraph graph)
        {
            int count = 0;

            for (int i = 0;
                 i < electricalComponents.Length;
                 i++)
            {
                if (!(electricalComponents[i]
                      is SparkSwitchIndex indexedSwitch))
                {
                    continue;
                }

                if (!indexedSwitch.ElectricalEnabled ||
                    !indexedSwitch.IsConducting)
                {
                    continue;
                }

                if (!TryGetTwoTerminals(
                        indexedSwitch,
                        out SparkTerminal input,
                        out SparkTerminal output))
                {
                    continue;
                }

                if (!graph.TerminalNode.ContainsKey(input) ||
                    !graph.TerminalNode.ContainsKey(output))
                {
                    continue;
                }

                float alpha =
                    Mathf.Clamp01(
                        indexedSwitch.CurrentIndexVoltagePercent /
                        100f);

                if (alpha <= 0f)
                    continue;

                count++;
            }

            return count;
        }

        // ============================================================
        // GET INDEXED SOURCE MATRIX INDEX
        // ============================================================

        private int GetIndexedSourceMatrixIndex(
            NetworkGraph graph,
            SparkSwitchIndex target,
            int reference)
        {
            int m =
                graph.NodeCount - 1;

            int counter = 0;

            for (int i = 0;
                 i < electricalComponents.Length;
                 i++)
            {
                if (!(electricalComponents[i]
                      is SparkSwitchIndex indexedSwitch))
                {
                    continue;
                }

                if (!indexedSwitch.ElectricalEnabled ||
                    !indexedSwitch.IsConducting)
                {
                    continue;
                }

                if (!TryGetTwoTerminals(
                        indexedSwitch,
                        out SparkTerminal input,
                        out SparkTerminal output))
                {
                    continue;
                }

                if (!graph.TerminalNode.TryGetValue(
                        input,
                        out int inputNode) ||
                    !graph.TerminalNode.TryGetValue(
                        output,
                        out int outputNode))
                {
                    continue;
                }

                float alpha =
                    Mathf.Clamp01(
                        indexedSwitch.CurrentIndexVoltagePercent /
                        100f);

                if (alpha <= 0f)
                    continue;

                if (indexedSwitch == target)
                {
                    return m + counter;
                }

                counter++;
            }

            return -1;
        }

        // ============================================================
        // CONTROLLED VOLTAGE SOURCE
        // ============================================================
        //
        // Constraint:
        //
        //     Vout - alpha * Vin = 0
        //
        // Source current is the additional MNA unknown.
        //
        // KCL:
        //
        //     +I at input
        //     -I at output
        //
        // Constraint row:
        //
        //     -alpha * Vin + Vout = 0
        //
        // This produces:
        //
        //     Index 1, alpha=.5
        //         Vout = .5 * Vin
        //
        //     Index 2, alpha=1
        //         Vout = Vin
        // ============================================================

        private static void StampControlledVoltageSource(
            double[,] A,
            double[] b,
            int ai,
            int bi,
            int sourceIndex,
            float alpha)
        {
            if (A == null ||
                b == null)
            {
                return;
            }

            if (sourceIndex < 0 ||
                sourceIndex >= b.Length)
            {
                return;
            }

            alpha =
                Mathf.Clamp01(alpha);

            int matrixSize =
                b.Length;

            // KCL contribution from source current.
            if (ai >= 0 &&
                ai < matrixSize)
            {
                A[ai, sourceIndex] += 1.0;
                A[sourceIndex, ai] -= alpha;
            }

            if (bi >= 0 &&
                bi < matrixSize)
            {
                A[bi, sourceIndex] -= 1.0;
                A[sourceIndex, bi] += 1.0;
            }
        }

        // ============================================================
        // DIODE
        // ============================================================

        private bool GetDiodeState(
            SparkElectricalComponent component,
            SparkTerminal anode,
            SparkTerminal cathode,
            NetworkGraph graph,
            float[] currentVoltages)
        {
            if (!diodeStates.TryGetValue(
                    component,
                    out bool on))
            {
                on =
                    component.ElectricalState.Conduction ==
                    SparkConductionState.Conducting;
            }

            if (!graph.TerminalNode.TryGetValue(
                    anode,
                    out int a) ||
                !graph.TerminalNode.TryGetValue(
                    cathode,
                    out int c))
            {
                return false;
            }

            float voltage =
                currentVoltages[a] -
                currentVoltages[c];

            float forwardVoltage =
                component is SparkDiode diode
                    ? diode.ForwardVoltage
                    : ((SparkLED)component).ForwardVoltage;

            if (on)
            {
                on =
                    voltage >=
                    forwardVoltage - 0.005f;
            }
            else
            {
                on =
                    voltage >=
                    forwardVoltage;
            }

            diodeStates[component] =
                on;

            return on;
        }

        // ============================================================
        // MAXIMUM ACTIVE SUPPLY VOLTAGE
        // ============================================================

        private float GetMaximumActiveSupplyVoltage()
        {
            float maximumVoltage = 0f;

            for (int i = 0;
                 i < powerSupplies.Length;
                 i++)
            {
                SparkPowerSupply supply =
                    powerSupplies[i];

                if (supply == null ||
                    !supply.IsOutputActive ||
                    !supply.ElectricalEnabled)
                {
                    continue;
                }

                maximumVoltage =
                    Mathf.Max(
                        maximumVoltage,
                        Mathf.Abs(
                            supply.OutputVoltage));
            }

            return maximumVoltage;
        }

        // ============================================================
        // LED EFFECTIVE RESISTANCE
        // ============================================================

        private float GetLEDEffectiveResistance(
            SparkLED led)
        {
            if (led == null)
                return minimumResistance;

            float referenceVoltage =
                GetMaximumActiveSupplyVoltage();

            float currentLimit =
                Mathf.Max(
                    0.000001f,
                    led.MaximumForwardCurrent);

            float voltageAboveForward =
                Mathf.Max(
                    0f,
                    referenceVoltage -
                    led.ForwardVoltage);

            float currentLimitResistance =
                voltageAboveForward /
                currentLimit;

            return Mathf.Max(
                minimumResistance,
                led.OnResistance,
                currentLimitResistance);
        }

        // ============================================================
        // APPLY DEVICE STATES
        // ============================================================

        private float ApplyDeviceStates(
            NetworkGraph graph,
            float[] voltages)
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

                    if (component is SparkHairDryerMotorElectrical motorDebug)
{
    Debug.Log(
        $"[MOTOR SOLVER] " +
        $"Live={voltages[na]:F3} V | " +
        $"Neutral={voltages[nb]:F3} V | " +
        $"MotorV={voltage:F3} V | " +
        $"R={motorDebug.Resistance:F3} Ω",
        motorDebug);
}

                float current = 0f;

                // ====================================================
                // INDEXED SWITCH
                // ====================================================

                if (component is SparkSwitchIndex indexedSwitch)
                {
                    if (indexedSwitch.IsConducting)
                    {
                        indexedSwitchCurrents.TryGetValue(
                            indexedSwitch,
                            out current);
                    }
                    else
                    {
                        current = 0f;
                    }
                }

                // ====================================================
                // MULTIMETER
                // ====================================================

                else if (component is SparkMultimeterElectricalComponent multimeter)
                {
                    float resistance =
                        Mathf.Max(
                            minimumResistance,
                            multimeter.ShuntResistanceOhms);

                    current =
                        voltage /
                        resistance;
                }

                // ====================================================
                // RESISTOR
                // ====================================================

                else if (component is SparkResistor resistor)
                {
                    current =
                        voltage /
                        Mathf.Max(
                            resistor.ResistanceOhms,
                            minimumResistance);
                }

                // ====================================================
                // BINARY SWITCH
                // ====================================================

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

                // ====================================================
                // HAIR DRYER MOTOR
                // ====================================================

                else if (component is SparkHairDryerMotorElectrical motor)
                {
                    current =
                        voltage /
                        Mathf.Max(
                            motor.Resistance,
                            minimumResistance);
                }

                // ====================================================
                // HAIR DRYER HEATER
                // ====================================================

                else if (component is SparkHairDryerHeaterElectrical heater)
                {
                    current =
                        voltage /
                        Mathf.Max(
                            heater.Resistance,
                            minimumResistance);
                }

                // ====================================================
                // DIODE
                // ====================================================

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
                              diode.ForwardVoltage -
                              0.005f
                            : voltage >=
                              diode.ForwardVoltage;

                    if (on)
                    {
                        current =
                            Mathf.Max(
                                0f,
                                (voltage -
                                 diode.ForwardVoltage) /
                                Mathf.Max(
                                    0.01f,
                                    diode.OnResistance));
                    }

                    diodeStates[diode] =
                        on &&
                        current > 0f;
                }

                // ====================================================
                // LED
                // ====================================================

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
                              led.ForwardVoltage -
                              led.ConductionHysteresis
                            : voltage >=
                              led.ForwardVoltage;

                    if (on)
                    {
                        float effectiveResistance =
                            GetLEDEffectiveResistance(
                                led);

                        current =
                            Mathf.Max(
                                0f,
                                (voltage -
                                 led.ForwardVoltage) /
                                effectiveResistance);

                        current =
                            Mathf.Min(
                                current,
                                led.MaximumForwardCurrent);
                    }

                    diodeStates[led] =
                        on &&
                        current > 0.000001f;
                }

                // ====================================================
                // CAPACITOR
                // ====================================================

                else if (component is SparkCapacitor capacitor &&
                         simulateCapacitors)
                {
                    capacitorPreviousVoltage.TryGetValue(
                        capacitor,
                        out float previousVoltage);

                    current =
                        capacitor.CapacitanceFarads *
                        (voltage -
                         previousVoltage) /
                        Mathf.Max(
                            0.000001f,
                            Application.isPlaying
                                ? Time.deltaTime
                                : editorTimeStep);
                }

                // ====================================================
                // POWER SUPPLY
                // ====================================================

                else if (component is SparkPowerSupply supply &&
                         supply.IsOutputActive)
                {
                    current =
                        CalculateSupplyOutputCurrent(
                            supply,
                            graph);
                }

                float power =
                    voltage *
                    current;

                if (component is SparkPowerSupply)
                    power =
                        -Mathf.Abs(power);

                SparkElectricalState previous =
                    component.ElectricalState;

                SparkConductionState conduction =
                    Mathf.Abs(current) >
                    0.000001f
                        ? SparkConductionState.Conducting
                        : SparkConductionState.NonConducting;

                SparkElectricalState state =
                    new SparkElectricalState(
                        voltage,
                        current,
                        power,
                        conduction);

                // ====================================================
                // TERMINAL STATES FIRST
                // ====================================================

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

                // ====================================================
                // COMPONENT STATE
                // ====================================================

                component.ApplyElectricalState(
                    state);

                // ====================================================
                // BINARY SWITCH POLARITY
                // ====================================================

                if (component is SparkSwitch sparkSwitch)
                {
                    sparkSwitch.RefreshRuntimePolarity();
                }

                // ====================================================
                // CONVERGENCE
                // ====================================================

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

            // ========================================================
            // SUPPLY CURRENT-LIMIT STATE
            // ========================================================

            for (int i = 0;
                 i < powerSupplies.Length;
                 i++)
            {
                SparkPowerSupply supply =
                    powerSupplies[i];

                if (supply == null ||
                    !supply.IsOutputActive ||
                    !supply.ElectricalEnabled)
                {
                    continue;
                }

                float outputCurrent =
                    CalculateSupplyOutputCurrent(
                        supply,
                        graph);

                bool limited =
                    outputCurrent >=
                    supply.CurrentLimit;

                supply.SetCurrentLimited(
                    limited);
            }

            return maxDelta;
        }

        // ============================================================
        // SUPPLY CURRENT
        // ============================================================

        private float CalculateSupplyOutputCurrent(
            SparkPowerSupply supply,
            NetworkGraph graph)
        {
            if (supply == null ||
                graph == null ||
                !TryGetTwoTerminals(
                    supply,
                    out SparkTerminal positive,
                    out SparkTerminal negative))
            {
                return 0f;
            }

            float totalCurrent = 0f;

            for (int i = 0;
                 i < electricalComponents.Length;
                 i++)
            {
                SparkElectricalComponent component =
                    electricalComponents[i];

                if (component == null ||
                    component == supply ||
                    !component.ElectricalEnabled)
                {
                    continue;
                }

                if (!TryGetTwoTerminals(
                        component,
                        out SparkTerminal terminalA,
                        out SparkTerminal terminalB))
                {
                    continue;
                }

                SparkElectricalState state =
                    component.ElectricalState;

                float componentCurrent =
                    state.Current;

                if (terminalA == positive)
                {
                    totalCurrent +=
                        componentCurrent;
                }
                else if (terminalB == positive)
                {
                    totalCurrent -=
                        componentCurrent;
                }
            }

            return Mathf.Max(
                0f,
                totalCurrent);
        }

        // ============================================================
        // TERMINAL STATES
        // ============================================================

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

        // ============================================================
        // CAPACITOR COMMIT
        // ============================================================

        private void CommitTransientState(
            NetworkGraph graph,
            float[] voltages)
        {
            if (!simulateCapacitors)
                return;

            for (int i = 0;
                 i < electricalComponents.Length;
                 i++)
            {
                if (!(electricalComponents[i]
                      is SparkCapacitor capacitor))
                {
                    continue;
                }

                if (!TryGetTwoTerminals(
                        capacitor,
                        out var a,
                        out var b))
                {
                    continue;
                }

                if (!graph.TerminalNode.TryGetValue(
                        a,
                        out int na) ||
                    !graph.TerminalNode.TryGetValue(
                        b,
                        out int nb))
                {
                    continue;
                }

                capacitorPreviousVoltage[capacitor] =
                    voltages[na] -
                    voltages[nb];
            }
        }

        // ============================================================
        // GROUND
        // ============================================================

        private int FindGroundNode(
            NetworkGraph graph)
        {
            for (int i = 0;
                 i < terminals.Count;
                 i++)
            {
                if (terminals[i].Kind ==
                    SparkTerminalKind.Ground &&
                    graph.TerminalNode.TryGetValue(
                        terminals[i],
                        out var node))
                {
                    return node;
                }
            }

            return -1;
        }

        // ============================================================
        // TWO TERMINALS
        // ============================================================

      private static bool TryGetTwoTerminals(
    Component component,
    out SparkTerminal a,
    out SparkTerminal b)
{
    a = null;
    b = null;

    if (component == null)
        return false;

    // ============================================================
    // POWER SUPPLY
    // ============================================================

    if (component is SparkPowerSupply powerSupply)
    {
        a = powerSupply.PositiveTerminal;
        b = powerSupply.NegativeTerminal;

        return a != null &&
               b != null;
    }

    // ============================================================
    // MULTIMETER
    // ============================================================

    if (component is SparkMultimeterElectricalComponent multimeter)
    {
        return multimeter.TryGetTerminals(
            out a,
            out b);
    }

    // ============================================================
    // INDEXED SWITCH
    // ============================================================

    if (component is SparkSwitchIndex indexedSwitch)
    {
        a = indexedSwitch.InputTerminal;
        b = indexedSwitch.OutputTerminal;

        return a != null &&
               b != null;
    }

    // ============================================================
    // BINARY SWITCH
    // ============================================================

    if (component is SparkSwitch sparkSwitch)
    {
        a = sparkSwitch.InputTerminal;
        b = sparkSwitch.OutputTerminal;

        return a != null &&
               b != null;
    }

    // ============================================================
    // LED
    // ============================================================

    if (component is SparkLED sparkLED)
    {
        a = sparkLED.AnodeTerminal;
        b = sparkLED.CathodeTerminal;

        return a != null &&
               b != null;
    }

    // ============================================================
    // HAIR DRYER MOTOR
    // ============================================================

    if (component is SparkHairDryerMotorElectrical motor)
    {
        a = motor.LiveTerminal;
        b = motor.NeutralTerminal;

        return a != null &&
               b != null;
    }

    // ============================================================
    // HAIR DRYER HEATER
    // ============================================================

    if (component is SparkHairDryerHeaterElectrical heater)
    {
        a = heater.LiveTerminal;
        b = heater.NeutralTerminal;

        return a != null &&
               b != null;
    }

    // ============================================================
    // FALLBACK
    // ============================================================

    var childTerminals =
        component.GetComponentsInChildren<SparkTerminal>(true);

    if (childTerminals.Length < 2)
        return false;

    a = childTerminals[0];
    b = childTerminals[1];

    return a != null &&
           b != null;
}
        // ============================================================
        // NODE MAPPING
        // ============================================================

        private static int MapNode(
            int node,
            int reference)
        {
            return node == reference
                ? -1
                : (node < reference
                    ? node
                    : node - 1);
        }

        // ============================================================
        // CONDUCTANCE
        // ============================================================

        private static void AddConductance(
            double[,] A,
            double[] b,
            int ai,
            int bi,
            double g,
            double voltage = 0.0)
        {
            if (g <= 0.0 ||
                double.IsNaN(g) ||
                double.IsInfinity(g))
            {
                return;
            }

            if (ai >= 0)
            {
                A[ai, ai] += g;
                b[ai] +=
                    g * voltage;
            }

            if (bi >= 0)
            {
                A[bi, bi] += g;
                b[bi] -=
                    g * voltage;
            }

            if (ai >= 0 &&
                bi >= 0)
            {
                A[ai, bi] -= g;
                A[bi, ai] -= g;
            }
        }

        // ============================================================
        // FORWARD DROP
        // ============================================================

        private static void StampForwardDrop(
            double[,] A,
            double[] b,
            int ai,
            int bi,
            float forwardVoltage,
            float resistance)
        {
            AddConductance(
                A,
                b,
                ai,
                bi,
                1.0 /
                Math.Max(
                    0.01f,
                    resistance),
                forwardVoltage);
        }

        // ============================================================
        // CURRENT SOURCE
        // ============================================================

        private static void AddCurrentSource(
            double[,] A,
            double[] b,
            int ai,
            int bi,
            double currentFromAtoB)
        {
            if (ai >= 0)
                b[ai] -=
                    currentFromAtoB;

            if (bi >= 0)
                b[bi] +=
                    currentFromAtoB;
        }

        // ============================================================
        // GAUSSIAN SOLVER
        // ============================================================

        private static double[] GaussianSolve(
            double[,] a,
            double[] b)
        {
            int n =
                b.Length;

            for (int i = 0;
                 i < n;
                 i++)
            {
                int pivot =
                    i;

                double best =
                    Math.Abs(
                        a[i, i]);

                for (int r = i + 1;
                     r < n;
                     r++)
                {
                    double value =
                        Math.Abs(
                            a[r, i]);

                    if (value > best)
                    {
                        best =
                            value;

                        pivot =
                            r;
                    }
                }

                if (best < 1.0e-15)
                    return null;

                if (pivot != i)
                {
                    for (int c = i;
                         c < n;
                         c++)
                    {
                        (
                            a[i, c],
                            a[pivot, c]
                        ) =
                        (
                            a[pivot, c],
                            a[i, c]
                        );
                    }

                    (
                        b[i],
                        b[pivot]
                    ) =
                    (
                        b[pivot],
                        b[i]
                    );
                }

                double diagonal =
                    a[i, i];

                for (int c = i;
                     c < n;
                     c++)
                {
                    a[i, c] /=
                        diagonal;
                }

                b[i] /=
                    diagonal;

                for (int r = i + 1;
                     r < n;
                     r++)
                {
                    double factor =
                        a[r, i];

                    if (Math.Abs(factor) <
                        1.0e-18)
                    {
                        continue;
                    }

                    for (int c = i;
                         c < n;
                         c++)
                    {
                        a[r, c] -=
                            factor *
                            a[i, c];
                    }

                    b[r] -=
                        factor *
                        b[i];
                }
            }

            var x =
                new double[n];

            for (int i = n - 1;
                 i >= 0;
                 i--)
            {
                double sum =
                    b[i];

                for (int c = i + 1;
                     c < n;
                     c++)
                {
                    sum -=
                        a[i, c] *
                        x[c];
                }

                x[i] =
                    sum;
            }

            return x;
        }

        // ============================================================
        // NETWORK GRAPH
        // ============================================================

        private sealed class NetworkGraph
        {
            public int NodeCount;

            public readonly Dictionary<int, int>
                RootToNode = new();

            public readonly Dictionary<SparkTerminal, int>
                TerminalNode = new();
        }

        // ============================================================
        // UNION FIND
        // ============================================================

        private sealed class UnionFind
        {
            private readonly int[] parent;
            private readonly byte[] rank;

            public UnionFind(int n)
            {
                parent =
                    new int[n];

                rank =
                    new byte[n];

                for (int i = 0;
                     i < n;
                     i++)
                {
                    parent[i] =
                        i;
                }
            }

            public int Find(int x)
            {
                while (parent[x] != x)
                {
                    parent[x] =
                        parent[parent[x]];

                    x =
                        parent[x];
                }

                return x;
            }

            public void Union(
                int a,
                int b)
            {
                a =
                    Find(a);

                b =
                    Find(b);

                if (a == b)
                    return;

                if (rank[a] < rank[b])
                {
                    (
                        a,
                        b
                    ) =
                    (
                        b,
                        a
                    );
                }

                parent[b] =
                    a;

                if (rank[a] == rank[b])
                    rank[a]++;
            }
        }
    }
}
