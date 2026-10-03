using System;
using System.Collections.Generic;
using UnityEngine;
using ProjectSpark.Circuit;
using ProjectSpark.Gameplay;

namespace ProjectSpark.Electrical
{
    /// <summary>
    /// Deterministic Project Spark DC electrical solver.
    ///
    /// Supports:
    /// - SparkResistor
    /// - SparkSwitch
    /// - SparkSwitchIndex
    /// - SparkDiode
    /// - SparkLED
    /// - SparkCapacitor
    /// - SparkPowerSupply
    ///
    /// The solver builds the electrical topology from SparkTerminals,
    /// solves node voltages internally, calculates component currents,
    /// and commits the final solved state only after the numerical
    /// solve has completed.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkElectricalSolver : MonoBehaviour
    {
        // ================================================================
        // REFERENCES
        // ================================================================

        [Header("References")]

        [SerializeField]
        private SparkCircuitSystem circuit;

        [SerializeField]
        private SparkPowerSupply[] powerSupplies =
            Array.Empty<SparkPowerSupply>();

        [SerializeField]
        private SparkElectricalComponent[] electricalComponents =
            Array.Empty<SparkElectricalComponent>();


        // ================================================================
        // SOLVER SETTINGS
        // ================================================================

        [Header("Solver")]

        [SerializeField, Min(1)]
        private int maxIterations = 16;

        [SerializeField, Min(0.0000001f)]
        private float convergenceTolerance = 0.000001f;

        [SerializeField, Min(0.000000001f)]
        private float minimumResistance = 0.000001f;

        [SerializeField, Min(0f)]
        private float floatingNodeLeakResistance = 1.0e9f;

        [SerializeField]
        private bool solveOnDirty = true;


        // ================================================================
        // CAPACITORS
        // ================================================================

        [Header("Capacitors")]

        [SerializeField]
        private bool simulateCapacitors = true;

        [SerializeField, Min(0.000001f)]
        private float editorTimeStep = 1f / 60f;


        // ================================================================
        // INTERNAL COLLECTIONS
        // ================================================================

        private readonly List<SparkCircuitConnection> connections =
            new List<SparkCircuitConnection>();

        private readonly List<SparkCircuitConnection> connectionBuffer =
            new List<SparkCircuitConnection>();

        private readonly List<SparkTerminal> terminals =
            new List<SparkTerminal>();

        private readonly Dictionary<SparkElectricalComponent, bool> diodeStates =
            new Dictionary<SparkElectricalComponent, bool>();

        private readonly Dictionary<SparkCapacitor, float> capacitorPreviousVoltage =
            new Dictionary<SparkCapacitor, float>();


        // ================================================================
        // INTERNAL STATE
        // ================================================================

        private bool dirty = true;

        private int lastSolvedTopologyVersion = -1;


        // ================================================================
        // EVENTS
        // ================================================================

        public event Action SolveCompleted;

        public event Action SolveFailed;


        // ================================================================
        // PUBLIC
        // ================================================================

        public bool IsDirty =>
            dirty;


        // ================================================================
        // UNITY
        // ================================================================

        private void Awake()
        {
            if (circuit == null)
            {
                circuit =
                    GetComponent<SparkCircuitSystem>();
            }

            RefreshComponentCache();

            dirty = true;
        }


        private void OnEnable()
        {
            dirty = true;

            RefreshComponentCache();
        }


        private void Update()
        {
            if (circuit != null)
            {
                if (circuit.TopologyVersion !=
                    lastSolvedTopologyVersion)
                {
                    dirty = true;
                }
            }

            bool dynamicCapacitor =
                simulateCapacitors &&
                HasDynamicCapacitor();

            if (solveOnDirty)
            {
                if (dirty)
                {
                    Solve();
                }
            }
            else if (dynamicCapacitor)
            {
                Solve();
            }
        }


        // ================================================================
        // DIRTY
        // ================================================================

        public void MarkDirty()
        {
            dirty = true;
        }


        public void SolveNow()
        {
            dirty = true;
            Solve();
        }


        // ================================================================
        // COMPONENT CACHE
        // ================================================================

        private void RefreshComponentCache()
        {
            SparkElectricalComponent[] found =
                FindObjectsByType<SparkElectricalComponent>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None);

            var componentSet =
                new HashSet<SparkElectricalComponent>();

            if (electricalComponents != null)
            {
                for (int i = 0;
                     i < electricalComponents.Length;
                     i++)
                {
                    SparkElectricalComponent component =
                        electricalComponents[i];

                    if (component != null)
                    {
                        componentSet.Add(component);
                    }
                }
            }

            for (int i = 0;
                 i < found.Length;
                 i++)
            {
                SparkElectricalComponent component =
                    found[i];

                if (component != null)
                {
                    componentSet.Add(component);
                }
            }

            electricalComponents =
                new List<SparkElectricalComponent>(
                    componentSet).ToArray();


            var supplies =
                new List<SparkPowerSupply>();

            if (powerSupplies != null)
            {
                for (int i = 0;
                     i < powerSupplies.Length;
                     i++)
                {
                    SparkPowerSupply supply =
                        powerSupplies[i];

                    if (supply != null &&
                        !supplies.Contains(supply))
                    {
                        supplies.Add(supply);
                    }
                }
            }

            for (int i = 0;
                 i < electricalComponents.Length;
                 i++)
            {
                if (electricalComponents[i]
                    is SparkPowerSupply supply)
                {
                    if (!supplies.Contains(supply))
                    {
                        supplies.Add(supply);
                    }
                }
            }

            powerSupplies =
                supplies.ToArray();
        }


        // ================================================================
        // CAPACITOR
        // ================================================================

        private bool HasDynamicCapacitor()
        {
            if (!simulateCapacitors)
            {
                return false;
            }

            for (int i = 0;
                 i < electricalComponents.Length;
                 i++)
            {
                if (electricalComponents[i]
                    is SparkCapacitor capacitor &&
                    capacitor.ElectricalEnabled)
                {
                    return true;
                }
            }

            return false;
        }


        // ================================================================
        // MAIN SOLVER
        // ================================================================

        public void Solve()
        {
            dirty = false;

            if (circuit == null)
            {
                SolveFailed?.Invoke();
                return;
            }

            RefreshComponentCache();

            circuit.RebuildTopology();

            lastSolvedTopologyVersion =
                circuit.TopologyVersion;


            NetworkGraph graph =
                BuildGraph();

            if (graph.NodeCount <= 0)
            {
                ZeroStates();

                SolveCompleted?.Invoke();
                return;
            }


            float[] voltages =
                new float[graph.NodeCount];

            float[] previousVoltages =
                new float[graph.NodeCount];


            bool converged = false;


            // ============================================================
            // NUMERICAL ITERATION
            //
            // IMPORTANT:
            //
            // No component or terminal electrical state is written here.
            // The solver works entirely on the local voltage array and
            // diode-state cache.
            // ============================================================

            for (int iteration = 0;
                 iteration < maxIterations;
                 iteration++)
            {
                Array.Copy(
                    voltages,
                    previousVoltages,
                    voltages.Length);


                bool solved =
                    SolveResistiveNetwork(
                        graph,
                        voltages);

                if (!solved)
                {
                    ZeroStates();

                    SolveFailed?.Invoke();
                    return;
                }


                float maximumDelta =
                    CalculateMaximumVoltageDelta(
                        voltages,
                        previousVoltages);


                /*
                 * The first iteration starts from zero, so always allow
                 * the solver to establish its initial network state.
                 */
                if (iteration > 0 &&
                    maximumDelta <=
                    convergenceTolerance)
                {
                    converged = true;
                    break;
                }
            }


            /*
             * If maxIterations was reached, preserve the numerical
             * result instead of destroying the circuit state.
             *
             * This retains the previous solver behavior for practical
             * diode networks while keeping state commits atomic.
             */
            if (!converged)
            {
                /*
                 * One final solve using the current diode state ensures
                 * the voltage array represents the latest device model.
                 */
                bool finalSolved =
                    SolveResistiveNetwork(
                        graph,
                        voltages);

                if (!finalSolved)
                {
                    ZeroStates();

                    SolveFailed?.Invoke();
                    return;
                }
            }


            // ============================================================
            // FINAL COMMIT
            //
            // Only now do gameplay-visible electrical states change.
            // ============================================================

            ApplyDeviceStates(
                graph,
                voltages);


            ApplyGraphTerminalStates(
                graph,
                voltages);


            CommitTransientState(
                graph,
                voltages);


            SolveCompleted?.Invoke();
        }


        // ================================================================
        // GRAPH
        // ================================================================

        private NetworkGraph BuildGraph()
        {
            terminals.Clear();
            connections.Clear();


            var terminalSet =
                new HashSet<SparkTerminal>();


            // ------------------------------------------------------------
            // COMPONENT TERMINALS
            // ------------------------------------------------------------

            for (int i = 0;
                 i < electricalComponents.Length;
                 i++)
            {
                SparkElectricalComponent component =
                    electricalComponents[i];

                if (component == null)
                {
                    continue;
                }

                SparkTerminal[] found =
                    component.GetComponentsInChildren<SparkTerminal>(
                        true);

                for (int t = 0;
                     t < found.Length;
                     t++)
                {
                    if (found[t] != null)
                    {
                        terminalSet.Add(found[t]);
                    }
                }
            }


            // ------------------------------------------------------------
            // CIRCUIT CONNECTIONS
            // ------------------------------------------------------------

            if (circuit != null)
            {
                connectionBuffer.Clear();

                circuit.CopyConnections(
                    connectionBuffer);

                for (int i = 0;
                     i < connectionBuffer.Count;
                     i++)
                {
                    SparkCircuitConnection connection =
                        connectionBuffer[i];

                    /*
                     * SparkCircuitConnection is a readonly struct.
                     * It cannot be compared against null.
                     */
                    if (!connection.IsValid)
                    {
                        continue;
                    }

                    connections.Add(connection);

                    if (connection.A != null)
                    {
                        terminalSet.Add(
                            connection.A);
                    }

                    if (connection.B != null)
                    {
                        terminalSet.Add(
                            connection.B);
                    }
                }
            }


            foreach (SparkTerminal terminal
                     in terminalSet)
            {
                if (terminal != null)
                {
                    terminals.Add(terminal);
                }
            }


            // ------------------------------------------------------------
            // TERMINAL INDEX
            // ------------------------------------------------------------

            var terminalIndex =
                new Dictionary<SparkTerminal, int>();

            for (int i = 0;
                 i < terminals.Count;
                 i++)
            {
                terminalIndex[
                    terminals[i]] = i;
            }


            // ------------------------------------------------------------
            // UNION FIND
            // ------------------------------------------------------------

            var unionFind =
                new UnionFind(
                    terminals.Count);


            for (int i = 0;
                 i < connections.Count;
                 i++)
            {
                SparkCircuitConnection connection =
                    connections[i];

                if (!connection.IsValid)
                {
                    continue;
                }


                /*
                 * Probe connections are measurement connections.
                 * They must NOT short the circuit nodes together.
                 */
                if (IsProbeConnection(connection))
                {
                    continue;
                }


                SparkTerminal a =
                    connection.A;

                SparkTerminal b =
                    connection.B;

                if (a == null ||
                    b == null)
                {
                    continue;
                }


                if (terminalIndex.TryGetValue(
                        a,
                        out int ia) &&
                    terminalIndex.TryGetValue(
                        b,
                        out int ib))
                {
                    unionFind.Union(
                        ia,
                        ib);
                }
            }


            // ------------------------------------------------------------
            // GRAPH NODE IDS
            // ------------------------------------------------------------

            var rootToNode =
                new Dictionary<int, int>();

            var terminalNode =
                new Dictionary<SparkTerminal, int>();


            int nodeCount = 0;


            for (int i = 0;
                 i < terminals.Count;
                 i++)
            {
                int root =
                    unionFind.Find(i);


                if (!rootToNode.TryGetValue(
                        root,
                        out int node))
                {
                    node =
                        nodeCount++;

                    rootToNode[root] =
                        node;
                }


                terminalNode[
                    terminals[i]] =
                    node;
            }


            return new NetworkGraph(
                nodeCount,
                rootToNode,
                terminalNode);
        }


        private bool IsProbeConnection(
            SparkCircuitConnection connection)
        {
            string kind =
                connection.Kind.ToString();

            return kind.IndexOf(
                       "Probe",
                       StringComparison.OrdinalIgnoreCase)
                   >= 0;
        }


        // ================================================================
        // RESISTIVE NETWORK
        // ================================================================

        private bool SolveResistiveNetwork(
            NetworkGraph graph,
            float[] voltages)
        {
            int nodeCount =
                graph.NodeCount;


            if (nodeCount <= 0)
            {
                return true;
            }


            int reference =
                FindGroundNode(graph);


            if (reference < 0)
            {
                reference = 0;
            }


            int matrixSize =
                nodeCount - 1;


            if (matrixSize <= 0)
            {
                voltages[reference] =
                    0f;

                return true;
            }


            double[,] A =
                new double[
                    matrixSize,
                    matrixSize];


            double[] b =
                new double[
                    matrixSize];


            // ------------------------------------------------------------
            // FLOATING NODE LEAK
            // ------------------------------------------------------------

            double leakConductance = 0.0;


            if (floatingNodeLeakResistance > 0f)
            {
                leakConductance =
                    1.0 /
                    Math.Max(
                        minimumResistance,
                        floatingNodeLeakResistance);
            }


            if (leakConductance > 0.0)
            {
                for (int i = 0;
                     i < matrixSize;
                     i++)
                {
                    A[i, i] +=
                        leakConductance;
                }
            }


            // ------------------------------------------------------------
            // COMPONENTS
            // ------------------------------------------------------------

            for (int i = 0;
                 i < electricalComponents.Length;
                 i++)
            {
                SparkElectricalComponent component =
                    electricalComponents[i];

                if (component == null ||
                    !component.ElectricalEnabled)
                {
                    continue;
                }


                /*
                 * Power supplies are stamped separately below.
                 */
                if (component is SparkPowerSupply)
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


                if (!graph.TerminalNode.TryGetValue(
                        terminalA,
                        out int nodeA) ||
                    !graph.TerminalNode.TryGetValue(
                        terminalB,
                        out int nodeB))
                {
                    continue;
                }


                int ai =
                    MapNode(
                        nodeA,
                        reference);

                int bi =
                    MapNode(
                        nodeB,
                        reference);


                // --------------------------------------------------------
                // RESISTOR
                // --------------------------------------------------------

                if (component
                    is SparkResistor resistor)
                {
                    float resistance =
                        Mathf.Max(
                            minimumResistance,
                            resistor.ResistanceOhms);


                    AddConductance(
                        A,
                        b,
                        ai,
                        bi,
                        1.0 / resistance);

                    continue;
                }


                // --------------------------------------------------------
                // SWITCH
                // --------------------------------------------------------

                if (component
                    is SparkSwitch sparkSwitch)
                {
                    if (!sparkSwitch.IsConducting)
                    {
                        continue;
                    }


                    float resistance =
                        Mathf.Max(
                            minimumResistance,
                            sparkSwitch.ClosedResistance);


                    AddConductance(
                        A,
                        b,
                        ai,
                        bi,
                        1.0 / resistance);

                    continue;
                }


                // --------------------------------------------------------
                // INDEX SWITCH
                // --------------------------------------------------------

                if (component
                    is SparkSwitchIndex indexedSwitch)
                {
                    if (!indexedSwitch.IsConducting)
                    {
                        continue;
                    }


                    float supplyVoltage =
                        GetMaximumActiveSupplyVoltage();


                    float outputVoltage =
                        supplyVoltage *
                        indexedSwitch.CurrentIndexVoltagePercent /
                        100f;


                    float resistance =
                        Mathf.Max(
                            minimumResistance,
                            0.001f);


                    AddConductance(
                        A,
                        b,
                        ai,
                        bi,
                        1.0 / resistance,
                        outputVoltage);

                    continue;
                }


                // --------------------------------------------------------
                // DIODE
                // --------------------------------------------------------

                if (component
                    is SparkDiode diode)
                {
                    bool conducting =
                        GetDiodeState(
                            diode,
                            diode.AnodeTerminal,
                            diode.CathodeTerminal,
                            graph,
                            voltages);


                    if (!conducting)
                    {
                        continue;
                    }


                    StampForwardDrop(
                        A,
                        b,
                        ai,
                        bi,
                        diode.ForwardVoltage,
                        Mathf.Max(
                            minimumResistance,
                            diode.OnResistance));

                    continue;
                }


                // --------------------------------------------------------
                // LED
                // --------------------------------------------------------

                if (component
                    is SparkLED led)
                {
                    bool conducting =
                        GetDiodeState(
                            led,
                            led.AnodeTerminal,
                            led.CathodeTerminal,
                            graph,
                            voltages);


                    if (!conducting)
                    {
                        continue;
                    }


                    float effectiveResistance =
                        GetLEDEffectiveResistance(
                            led);


                    StampForwardDrop(
                        A,
                        b,
                        ai,
                        bi,
                        led.ForwardVoltage,
                        effectiveResistance);

                    continue;
                }


                // --------------------------------------------------------
                // CAPACITOR
                // --------------------------------------------------------

                if (component
                    is SparkCapacitor capacitor)
                {
                    if (!simulateCapacitors)
                    {
                        continue;
                    }


                    float capacitance =
                        Mathf.Max(
                            0.000000001f,
                            capacitor.CapacitanceFarads);


                    float dt =
                        Mathf.Max(
                            0.000001f,
                            editorTimeStep);


                    double conductance =
                        capacitance /
                        dt;


                    capacitorPreviousVoltage.TryGetValue(
                        capacitor,
                        out float previousVoltage);


                    AddConductance(
                        A,
                        b,
                        ai,
                        bi,
                        conductance,
                        previousVoltage);
                }
            }


            // ============================================================
            // POWER SUPPLIES
            // ============================================================

            for (int i = 0;
                 i < powerSupplies.Length;
                 i++)
            {
                SparkPowerSupply supply =
                    powerSupplies[i];

                if (supply == null ||
                    !supply.ElectricalEnabled ||
                    !supply.IsOutputActive)
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
                        out int positiveNode) ||
                    !graph.TerminalNode.TryGetValue(
                        negative,
                        out int negativeNode))
                {
                    continue;
                }


                int pi =
                    MapNode(
                        positiveNode,
                        reference);

                int ni =
                    MapNode(
                        negativeNode,
                        reference);


                /*
                 * Preserve the existing supply model:
                 *
                 * normal supply:
                 * extremely small series resistance
                 *
                 * current limit:
                 * equivalent resistance based on V / I
                 */
                float resistance =
                    minimumResistance;


                if (supply.CurrentLimit > 0f)
                {
                    float currentLimitResistance =
                        Mathf.Abs(
                            supply.OutputVoltage) /
                        Mathf.Max(
                            0.000001f,
                            supply.CurrentLimit);


                    resistance =
                        Mathf.Max(
                            resistance,
                            currentLimitResistance);
                }


                AddConductance(
                    A,
                    b,
                    pi,
                    ni,
                    1.0 / resistance,
                    supply.OutputVoltage);
            }


            // ------------------------------------------------------------
            // GAUSSIAN SOLVE
            // ------------------------------------------------------------

            double[] solution =
                GaussianSolve(
                    A,
                    b);


            if (solution == null)
            {
                return false;
            }


            // ------------------------------------------------------------
            // RESTORE FULL NODE VOLTAGES
            // ------------------------------------------------------------

            for (int node = 0;
                 node < nodeCount;
                 node++)
            {
                if (node == reference)
                {
                    voltages[node] =
                        0f;

                    continue;
                }


                int reducedIndex =
                    node < reference
                        ? node
                        : node - 1;


                if (reducedIndex >= 0 &&
                    reducedIndex < solution.Length)
                {
                    voltages[node] =
                        (float)solution[
                            reducedIndex];
                }
            }


            return true;
        }


        // ================================================================
        // DIODE STATE
        // ================================================================

        private bool GetDiodeState(
            SparkElectricalComponent component,
            SparkTerminal anode,
            SparkTerminal cathode,
            NetworkGraph graph,
            float[] voltages)
        {
            if (component == null ||
                anode == null ||
                cathode == null)
            {
                return false;
            }


            if (!graph.TerminalNode.TryGetValue(
                    anode,
                    out int anodeNode) ||
                !graph.TerminalNode.TryGetValue(
                    cathode,
                    out int cathodeNode))
            {
                return false;
            }


            float voltage =
                voltages[anodeNode] -
                voltages[cathodeNode];


            float forwardVoltage;
            float hysteresis;


            if (component
                is SparkLED led)
            {
                forwardVoltage =
                    led.ForwardVoltage;

                hysteresis =
                    led.ConductionHysteresis;
            }
            else if (component
                     is SparkDiode diode)
            {
                forwardVoltage =
                    diode.ForwardVoltage;

                hysteresis =
                    0.01f;
            }
            else
            {
                return false;
            }


            diodeStates.TryGetValue(
                component,
                out bool wasOn);


            bool on;


            if (wasOn)
            {
                on =
                    voltage >=
                    forwardVoltage -
                    hysteresis;
            }
            else
            {
                on =
                    voltage >=
                    forwardVoltage;
            }


            /*
             * Reverse voltage cannot create forward conduction.
             */
            if (voltage < 0f)
            {
                on = false;
            }


            diodeStates[component] =
                on;


            return on;
        }


        // ================================================================
        // LED EFFECTIVE RESISTANCE
        // ================================================================

        private float GetLEDEffectiveResistance(
            SparkLED led)
        {
            if (led == null)
            {
                return minimumResistance;
            }


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


        // ================================================================
        // APPLY FINAL DEVICE STATES
        // ================================================================

        private void ApplyDeviceStates(
            NetworkGraph graph,
            float[] voltages)
        {
            /*
             * First pass:
             *
             * Calculate every non-supply component state.
             *
             * This prevents power-supply current from depending on the
             * order of electricalComponents[].
             */
            for (int i = 0;
                 i < electricalComponents.Length;
                 i++)
            {
                SparkElectricalComponent component =
                    electricalComponents[i];

                if (component == null ||
                    component is SparkPowerSupply)
                {
                    continue;
                }


                ApplySingleDeviceState(
                    component,
                    graph,
                    voltages);
            }


            /*
             * Second pass:
             *
             * Calculate power supply states after load component
             * currents have been established.
             */
            for (int i = 0;
                 i < electricalComponents.Length;
                 i++)
            {
                SparkElectricalComponent component =
                    electricalComponents[i];

                if (!(component
                      is SparkPowerSupply supply))
                {
                    continue;
                }


                ApplySingleDeviceState(
                    supply,
                    graph,
                    voltages);
            }


            // ------------------------------------------------------------
            // POWER SUPPLY CURRENT LIMIT STATE
            // ------------------------------------------------------------

            for (int i = 0;
                 i < powerSupplies.Length;
                 i++)
            {
                SparkPowerSupply supply =
                    powerSupplies[i];

                if (supply == null)
                {
                    continue;
                }


                float current =
                    CalculateSupplyOutputCurrent(
                        supply,
                        graph);


                if (supply.CurrentLimit > 0f)
                {
                    bool limited =
                        current >=
                        supply.CurrentLimit;


                    supply.SetCurrentLimited(
                        limited);
                }
            }
        }


        // ================================================================
        // APPLY SINGLE DEVICE STATE
        // ================================================================

        private void ApplySingleDeviceState(
            SparkElectricalComponent component,
            NetworkGraph graph,
            float[] voltages)
        {
            if (component == null)
            {
                return;
            }


            if (!TryGetTwoTerminals(
                    component,
                    out SparkTerminal terminalA,
                    out SparkTerminal terminalB))
            {
                return;
            }


            if (!graph.TerminalNode.TryGetValue(
                    terminalA,
                    out int nodeA) ||
                !graph.TerminalNode.TryGetValue(
                    terminalB,
                    out int nodeB))
            {
                return;
            }


            float voltage =
                voltages[nodeA] -
                voltages[nodeB];


            float current =
                0f;


            SparkConductionState conduction =
                SparkConductionState.NonConducting;


            // ============================================================
            // RESISTOR
            // ============================================================

            if (component
                is SparkResistor resistor)
            {
                float resistance =
                    Mathf.Max(
                        minimumResistance,
                        resistor.ResistanceOhms);


                current =
                    voltage /
                    resistance;


                if (Mathf.Abs(current) >
                    0.000001f)
                {
                    conduction =
                        SparkConductionState.Conducting;
                }
            }


            // ============================================================
            // SWITCH
            // ============================================================

            else if (component
                     is SparkSwitch sparkSwitch)
            {
                if (sparkSwitch.IsConducting)
                {
                    float resistance =
                        Mathf.Max(
                            minimumResistance,
                            sparkSwitch.ClosedResistance);


                    current =
                        voltage /
                        resistance;


                    conduction =
                        SparkConductionState.Conducting;
                }
            }


            // ============================================================
            // INDEX SWITCH
            // ============================================================

            else if (component
                     is SparkSwitchIndex indexedSwitch)
            {
                if (indexedSwitch.IsConducting)
                {
                    float supplyVoltage =
                        GetMaximumActiveSupplyVoltage();


                    float outputVoltage =
                        supplyVoltage *
                        indexedSwitch.CurrentIndexVoltagePercent /
                        100f;


                    float resistance =
                        Mathf.Max(
                            minimumResistance,
                            0.001f);


                    current =
                        (voltage -
                         outputVoltage) /
                        resistance;


                    conduction =
                        SparkConductionState.Conducting;
                }
            }


            // ============================================================
            // DIODE
            // ============================================================

            else if (component
                     is SparkDiode diode)
            {
                bool on =
                    GetDiodeState(
                        diode,
                        diode.AnodeTerminal,
                        diode.CathodeTerminal,
                        graph,
                        voltages);


                if (on)
                {
                    float resistance =
                        Mathf.Max(
                            minimumResistance,
                            diode.OnResistance);


                    current =
                        Mathf.Max(
                            0f,
                            (voltage -
                             diode.ForwardVoltage) /
                            resistance);


                    conduction =
                        current >
                        0.000001f
                            ? SparkConductionState.Conducting
                            : SparkConductionState.NonConducting;
                }
            }


            // ============================================================
            // LED
            // ============================================================

            else if (component
                     is SparkLED led)
            {
                SparkTerminal anode =
                    led.AnodeTerminal;

                SparkTerminal cathode =
                    led.CathodeTerminal;


                if (anode == null ||
                    cathode == null)
                {
                    WriteElectricalState(
                        component,
                        0f,
                        0f,
                        SparkConductionState.NonConducting);

                    return;
                }


                if (!graph.TerminalNode.TryGetValue(
                        anode,
                        out int anodeNode) ||
                    !graph.TerminalNode.TryGetValue(
                        cathode,
                        out int cathodeNode))
                {
                    return;
                }


                /*
                 * LED voltage is ALWAYS:
                 *
                 * Anode - Cathode
                 */
                float ledVoltage =
                    voltages[anodeNode] -
                    voltages[cathodeNode];


                bool on =
                    GetDiodeState(
                        led,
                        anode,
                        cathode,
                        graph,
                        voltages);


                if (on)
                {
                    float effectiveResistance =
                        GetLEDEffectiveResistance(
                            led);


                    current =
                        Mathf.Max(
                            0f,
                            (ledVoltage -
                             led.ForwardVoltage) /
                            effectiveResistance);


                    current =
                        Mathf.Min(
                            current,
                            led.MaximumForwardCurrent);


                    conduction =
                        current >
                        0.000001f
                            ? SparkConductionState.Conducting
                            : SparkConductionState.NonConducting;
                }
                else
                {
                    current =
                        0f;

                    conduction =
                        SparkConductionState.NonConducting;
                }


                voltage =
                    ledVoltage;


                diodeStates[led] =
                    conduction ==
                    SparkConductionState.Conducting &&
                    current >
                    0.000001f;
            }


            // ============================================================
            // CAPACITOR
            // ============================================================

            else if (component
                     is SparkCapacitor capacitor)
            {
                if (simulateCapacitors)
                {
                    float capacitance =
                        Mathf.Max(
                            0.000000001f,
                            capacitor.CapacitanceFarads);


                    capacitorPreviousVoltage.TryGetValue(
                        capacitor,
                        out float previousVoltage);


                    float dt =
                        Mathf.Max(
                            0.000001f,
                            editorTimeStep);


                    current =
                        capacitance *
                        (voltage -
                         previousVoltage) /
                        dt;


                    conduction =
                        SparkConductionState.Conducting;
                }
            }


            // ============================================================
            // POWER SUPPLY
            // ============================================================

            else if (component
                     is SparkPowerSupply supply)
            {
                current =
                    CalculateSupplyOutputCurrent(
                        supply,
                        graph);


                conduction =
                    supply.IsOutputActive
                        ? SparkConductionState.Conducting
                        : SparkConductionState.NonConducting;
            }


            // ============================================================
            // WRITE FINAL COMPONENT STATE
            // ============================================================

            WriteElectricalState(
                component,
                voltage,
                current,
                conduction);


            // ============================================================
            // FINAL TERMINAL STATE
            // ============================================================

            ApplyTerminalElectricalStates(
                terminalA,
                terminalB,
                nodeA,
                nodeB,
                voltages,
                current);


            // ============================================================
            // SWITCH RUNTIME POLARITY
            // ============================================================

            if (component
                is SparkSwitch runtimeSwitch)
            {
                runtimeSwitch.RefreshRuntimePolarity();
            }
        }


        // ================================================================
        // ELECTRICAL STATE
        // ================================================================

        private void WriteElectricalState(
            SparkElectricalComponent component,
            float voltage,
            float current,
            SparkConductionState conduction)
        {
            if (component == null)
            {
                return;
            }


            float power =
                voltage *
                current;


            SparkElectricalState state =
                new SparkElectricalState(
                    voltage,
                    current,
                    power,
                    conduction);


            component.ApplyElectricalState(
                state);
        }


        // ================================================================
        // TERMINAL ELECTRICAL STATE
        // ================================================================

        private void ApplyTerminalElectricalStates(
            SparkTerminal terminalA,
            SparkTerminal terminalB,
            int nodeA,
            int nodeB,
            float[] voltages,
            float current)
        {
            if (voltages == null)
            {
                return;
            }


            if (nodeA < 0 ||
                nodeA >= voltages.Length)
            {
                return;
            }


            if (nodeB < 0 ||
                nodeB >= voltages.Length)
            {
                return;
            }


            float voltageA =
                voltages[nodeA];

            float voltageB =
                voltages[nodeB];


            if (terminalA != null)
            {
                terminalA.ApplyElectricalState(
                    new SparkTerminalElectricalState(
                        voltageA,
                        current));
            }


            if (terminalB != null)
            {
                terminalB.ApplyElectricalState(
                    new SparkTerminalElectricalState(
                        voltageB,
                        -current));
            }
        }


        // ================================================================
        // FINAL GRAPH TERMINAL STATES
        // ================================================================

        private void ApplyGraphTerminalStates(
            NetworkGraph graph,
            float[] voltages)
        {
            if (graph == null ||
                voltages == null)
            {
                return;
            }


            foreach (KeyValuePair<SparkTerminal, int> pair
                     in graph.TerminalNode)
            {
                SparkTerminal terminal =
                    pair.Key;

                int node =
                    pair.Value;


                if (terminal == null ||
                    node < 0 ||
                    node >= voltages.Length)
                {
                    continue;
                }


                /*
                 * Node voltage is authoritative.
                 *
                 * Preserve the current already calculated for the
                 * terminal rather than inventing a new current here.
                 */
                terminal.ApplyElectricalState(
                    new SparkTerminalElectricalState(
                        voltages[node],
                        terminal.ElectricalState.Current));
            }
        }


        // ================================================================
        // SUPPLY CURRENT
        // ================================================================

        private float CalculateSupplyOutputCurrent(
            SparkPowerSupply supply,
            NetworkGraph graph)
        {
            if (supply == null ||
                !supply.ElectricalEnabled ||
                !supply.IsOutputActive)
            {
                return 0f;
            }


            if (!TryGetTwoTerminals(
                    supply,
                    out SparkTerminal positive,
                    out SparkTerminal negative))
            {
                return 0f;
            }


            if (!graph.TerminalNode.TryGetValue(
                    positive,
                    out int positiveNode))
            {
                return 0f;
            }


            float total =
                0f;


            for (int i = 0;
                 i < electricalComponents.Length;
                 i++)
            {
                SparkElectricalComponent component =
                    electricalComponents[i];


                if (component == null ||
                    component == supply)
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


                if (!graph.TerminalNode.TryGetValue(
                        terminalA,
                        out int nodeA) ||
                    !graph.TerminalNode.TryGetValue(
                        terminalB,
                        out int nodeB))
                {
                    continue;
                }


                /*
                 * Current entering/leaving the supply positive node.
                 */
                if (nodeA == positiveNode)
                {
                    total +=
                        component.ElectricalState.Current;
                }


                if (nodeB == positiveNode)
                {
                    total -=
                        component.ElectricalState.Current;
                }
            }


            return Mathf.Max(
                0f,
                total);
        }


        // ================================================================
        // SUPPLY VOLTAGE
        // ================================================================

        private float GetMaximumActiveSupplyVoltage()
        {
            float maximum =
                0f;


            for (int i = 0;
                 i < powerSupplies.Length;
                 i++)
            {
                SparkPowerSupply supply =
                    powerSupplies[i];


                if (supply == null ||
                    !supply.ElectricalEnabled ||
                    !supply.IsOutputActive)
                {
                    continue;
                }


                maximum =
                    Mathf.Max(
                        maximum,
                        Mathf.Abs(
                            supply.OutputVoltage));
            }


            return maximum;
        }


        // ================================================================
        // TRANSIENT STATE
        // ================================================================

        private void CommitTransientState(
            NetworkGraph graph,
            float[] voltages)
        {
            if (!simulateCapacitors)
            {
                return;
            }


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
                        out SparkTerminal a,
                        out SparkTerminal b))
                {
                    continue;
                }


                if (!graph.TerminalNode.TryGetValue(
                        a,
                        out int nodeA) ||
                    !graph.TerminalNode.TryGetValue(
                        b,
                        out int nodeB))
                {
                    continue;
                }


                capacitorPreviousVoltage[
                    capacitor] =
                    voltages[nodeA] -
                    voltages[nodeB];
            }
        }


        // ================================================================
        // ZERO STATES
        // ================================================================

        private void ZeroStates()
        {
            for (int i = 0;
                 i < electricalComponents.Length;
                 i++)
            {
                SparkElectricalComponent component =
                    electricalComponents[i];


                if (component == null)
                {
                    continue;
                }


                WriteElectricalState(
                    component,
                    0f,
                    0f,
                    SparkConductionState.NonConducting);


                diodeStates[component] =
                    false;


                if (component
                    is SparkCapacitor capacitor)
                {
                    capacitorPreviousVoltage[
                        capacitor] =
                        0f;
                }
            }


            for (int i = 0;
                 i < terminals.Count;
                 i++)
            {
                SparkTerminal terminal =
                    terminals[i];


                if (terminal == null)
                {
                    continue;
                }


                terminal.ApplyElectricalState(
                    new SparkTerminalElectricalState(
                        0f,
                        0f));
            }
        }


        // ================================================================
        // GROUND
        // ================================================================

        private int FindGroundNode(
            NetworkGraph graph)
        {
            /*
             * Power supply negative is the preferred reference node.
             */
            for (int i = 0;
                 i < powerSupplies.Length;
                 i++)
            {
                SparkPowerSupply supply =
                    powerSupplies[i];


                if (supply == null)
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


                if (graph.TerminalNode.TryGetValue(
                        negative,
                        out int node))
                {
                    return node;
                }
            }


            /*
             * Fall back to GND / Ground.
             */
            foreach (KeyValuePair<SparkTerminal, int> pair
                     in graph.TerminalNode)
            {
                SparkTerminal terminal =
                    pair.Key;


                if (terminal == null)
                {
                    continue;
                }


                string name =
                    terminal.name;


                if (string.Equals(
                        name,
                        "GND",
                        StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(
                        name,
                        "Ground",
                        StringComparison.OrdinalIgnoreCase))
                {
                    return pair.Value;
                }
            }


            return 0;
        }


        // ================================================================
        // NODE MAPPING
        // ================================================================

        private int MapNode(
            int node,
            int reference)
        {
            if (node == reference)
            {
                return -1;
            }


            return node < reference
                ? node
                : node - 1;
        }


        // ================================================================
        // TERMINALS
        // ================================================================

        private bool TryGetTwoTerminals(
            SparkElectricalComponent component,
            out SparkTerminal terminalA,
            out SparkTerminal terminalB)
        {
            terminalA = null;
            terminalB = null;


            if (component == null)
            {
                return false;
            }


            // ------------------------------------------------------------
            // LED
            // ------------------------------------------------------------

            if (component
                is SparkLED led)
            {
                terminalA =
                    led.AnodeTerminal;

                terminalB =
                    led.CathodeTerminal;

                return terminalA != null &&
                       terminalB != null;
            }


            // ------------------------------------------------------------
            // DIODE
            // ------------------------------------------------------------

            if (component
                is SparkDiode diode)
            {
                terminalA =
                    diode.AnodeTerminal;

                terminalB =
                    diode.CathodeTerminal;

                return terminalA != null &&
                       terminalB != null;
            }


            // ------------------------------------------------------------
            // SWITCH
            // ------------------------------------------------------------

            if (component
                is SparkSwitch sparkSwitch)
            {
                terminalA =
                    sparkSwitch.InputTerminal;

                terminalB =
                    sparkSwitch.OutputTerminal;

                return terminalA != null &&
                       terminalB != null;
            }


            // ------------------------------------------------------------
            // INDEX SWITCH
            // ------------------------------------------------------------

            if (component
                is SparkSwitchIndex indexedSwitch)
            {
                terminalA =
                    indexedSwitch.InputTerminal;

                terminalB =
                    indexedSwitch.OutputTerminal;

                return terminalA != null &&
                       terminalB != null;
            }


            // ------------------------------------------------------------
            // POWER SUPPLY
            // ------------------------------------------------------------

            if (component
                is SparkPowerSupply supply)
            {
                terminalA =
                    supply.PositiveTerminal;

                terminalB =
                    supply.NegativeTerminal;

                return terminalA != null &&
                       terminalB != null;
            }


            // ------------------------------------------------------------
            // GENERIC COMPONENT
            // ------------------------------------------------------------

            SparkTerminal[] childTerminals =
                component.GetComponentsInChildren<SparkTerminal>(
                    true);


            if (childTerminals == null ||
                childTerminals.Length < 2)
            {
                return false;
            }


            for (int i = 0;
                 i < childTerminals.Length;
                 i++)
            {
                SparkTerminal terminal =
                    childTerminals[i];


                if (terminal == null)
                {
                    continue;
                }


                if (terminalA == null)
                {
                    terminalA =
                        terminal;

                    continue;
                }


                if (terminal != terminalA)
                {
                    terminalB =
                        terminal;

                    break;
                }
            }


            return terminalA != null &&
                   terminalB != null;
        }


        // ================================================================
        // MATRIX STAMPING
        // ================================================================

        private void AddConductance(
            double[,] A,
            double[] b,
            int ai,
            int bi,
            double conductance)
        {
            AddConductance(
                A,
                b,
                ai,
                bi,
                conductance,
                0f);
        }


        private void AddConductance(
            double[,] A,
            double[] b,
            int ai,
            int bi,
            double conductance,
            float voltage)
        {
            if (conductance <= 0.0)
            {
                return;
            }


            if (ai >= 0)
            {
                A[ai, ai] +=
                    conductance;
            }


            if (bi >= 0)
            {
                A[bi, bi] +=
                    conductance;
            }


            if (ai >= 0 &&
                bi >= 0)
            {
                A[ai, bi] -=
                    conductance;

                A[bi, ai] -=
                    conductance;
            }


            /*
             * Norton equivalent:
             *
             * V = voltage
             * through resistance = 1 / G
             */
            double sourceCurrent =
                conductance *
                voltage;


            if (ai >= 0)
            {
                b[ai] +=
                    sourceCurrent;
            }


            if (bi >= 0)
            {
                b[bi] -=
                    sourceCurrent;
            }
        }


        private void StampForwardDrop(
            double[,] A,
            double[] b,
            int ai,
            int bi,
            float forwardVoltage,
            float resistance)
        {
            float safeResistance =
                Mathf.Max(
                    minimumResistance,
                    resistance);


            double conductance =
                1.0 /
                safeResistance;


            AddConductance(
                A,
                b,
                ai,
                bi,
                conductance,
                forwardVoltage);
        }


        // ================================================================
        // GAUSSIAN SOLVER
        // ================================================================

        private double[] GaussianSolve(
            double[,] matrix,
            double[] vector)
        {
            int n =
                vector.Length;


            if (n == 0)
            {
                return Array.Empty<double>();
            }


            double[,] a =
                new double[
                    n,
                    n + 1];


            for (int row = 0;
                 row < n;
                 row++)
            {
                for (int col = 0;
                     col < n;
                     col++)
                {
                    a[row, col] =
                        matrix[row, col];
                }


                a[row, n] =
                    vector[row];
            }


            // ------------------------------------------------------------
            // ELIMINATION
            // ------------------------------------------------------------

            for (int column = 0;
                 column < n;
                 column++)
            {
                int pivot =
                    column;


                double maximum =
                    Math.Abs(
                        a[column, column]);


                for (int row = column + 1;
                     row < n;
                     row++)
                {
                    double value =
                        Math.Abs(
                            a[row, column]);


                    if (value > maximum)
                    {
                        maximum =
                            value;

                        pivot =
                            row;
                    }
                }


                if (maximum < 1.0e-15)
                {
                    return null;
                }


                if (pivot != column)
                {
                    for (int col = column;
                         col <= n;
                         col++)
                    {
                        double temp =
                            a[column, col];


                        a[column, col] =
                            a[pivot, col];


                        a[pivot, col] =
                            temp;
                    }
                }


                double divisor =
                    a[column, column];


                for (int col = column;
                     col <= n;
                     col++)
                {
                    a[column, col] /=
                        divisor;
                }


                for (int row = 0;
                     row < n;
                     row++)
                {
                    if (row == column)
                    {
                        continue;
                    }


                    double factor =
                        a[row, column];


                    if (Math.Abs(factor) <
                        1.0e-20)
                    {
                        continue;
                    }


                    for (int col = column;
                         col <= n;
                         col++)
                    {
                        a[row, col] -=
                            factor *
                            a[column, col];
                    }
                }
            }


            // ------------------------------------------------------------
            // RESULT
            // ------------------------------------------------------------

            double[] result =
                new double[n];


            for (int i = 0;
                 i < n;
                 i++)
            {
                result[i] =
                    a[i, n];
            }


            return result;
        }


        // ================================================================
        // CONVERGENCE
        // ================================================================

        private float CalculateMaximumVoltageDelta(
            float[] currentVoltages,
            float[] previousVoltages)
        {
            if (currentVoltages == null ||
                previousVoltages == null)
            {
                return float.MaxValue;
            }


            int count =
                Mathf.Min(
                    currentVoltages.Length,
                    previousVoltages.Length);


            float maximum =
                0f;


            for (int i = 0;
                 i < count;
                 i++)
            {
                float delta =
                    Mathf.Abs(
                        currentVoltages[i] -
                        previousVoltages[i]);


                if (delta > maximum)
                {
                    maximum =
                        delta;
                }
            }


            return maximum;
        }


        // ================================================================
        // GRAPH
        // ================================================================

        private sealed class NetworkGraph
        {
            public readonly int NodeCount;

            public readonly Dictionary<int, int>
                RootToNode;

            public readonly Dictionary<SparkTerminal, int>
                TerminalNode;


            public NetworkGraph(
                int nodeCount,
                Dictionary<int, int> rootToNode,
                Dictionary<SparkTerminal, int> terminalNode)
            {
                NodeCount =
                    nodeCount;

                RootToNode =
                    rootToNode;

                TerminalNode =
                    terminalNode;
            }
        }


        // ================================================================
        // UNION FIND
        // ================================================================

        private sealed class UnionFind
        {
            private readonly int[] parent;

            private readonly byte[] rank;


            public UnionFind(int count)
            {
                parent =
                    new int[count];

                rank =
                    new byte[count];


                for (int i = 0;
                     i < count;
                     i++)
                {
                    parent[i] =
                        i;
                }
            }


            public int Find(int value)
            {
                int root =
                    value;


                while (parent[root] != root)
                {
                    root =
                        parent[root];
                }


                while (parent[value] != value)
                {
                    int next =
                        parent[value];


                    parent[value] =
                        root;


                    value =
                        next;
                }


                return root;
            }


            public void Union(
                int a,
                int b)
            {
                int rootA =
                    Find(a);

                int rootB =
                    Find(b);


                if (rootA == rootB)
                {
                    return;
                }


                if (rank[rootA] <
                    rank[rootB])
                {
                    parent[rootA] =
                        rootB;
                }
                else if (rank[rootA] >
                         rank[rootB])
                {
                    parent[rootB] =
                        rootA;
                }
                else
                {
                    parent[rootB] =
                        rootA;

                    rank[rootA]++;
                }
            }
        }
    }
}