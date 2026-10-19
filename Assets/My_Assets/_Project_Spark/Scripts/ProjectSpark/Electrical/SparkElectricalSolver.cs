using System;
using System.Collections.Generic;
using UnityEngine;
using ProjectSpark.Gameplay;
using ProjectSpark.Circuit;

namespace ProjectSpark.Electrical
{
    /// <summary>
    /// Deterministic Project Spark DC electrical solver.
    ///
    /// Architecture:
    ///
    /// SparkElectricalSolver
    ///      |
    ///      +-- SparkElectricalNetworkGraph
    ///      |
    ///      +-- SparkElectricalSolveContext
    ///      |
    ///      +-- SparkElectricalSolverRegistry
    ///               |
    ///               +-- Solver Definitions
    ///
    /// The core solver contains no concrete-device equations.
    /// Device-specific topology, stamping and state application
    /// are owned by SparkElectricalSolverDefinition assets.
    ///
    /// Electrical components can be discovered automatically from
    /// a configurable hierarchy. Automatic discovery is refreshed
    /// only when topology is refreshed, never every frame.
    /// </summary>
    public sealed class SparkElectricalSolver : MonoBehaviour
    {
        // ---------------------------------------------------------------------
        // REFERENCES
        // ---------------------------------------------------------------------

        [Header("Circuit")]

        [SerializeField]
        private SparkCircuitSystem circuit;

        [SerializeField]
        private SparkElectricalSolverRegistry solverRegistry;

        // ---------------------------------------------------------------------
        // ELECTRICAL COMPONENTS
        // ---------------------------------------------------------------------

        [Header("Electrical Components")]

        [Tooltip(
            "When enabled, SparkElectricalComponent objects are automatically " +
            "discovered under Component Search Root.")]
        [SerializeField]
        private bool autoDiscoverElectricalComponents = true;

        [Tooltip(
            "Hierarchy used for automatic electrical-component discovery. " +
            "If empty, SparkCircuitSystem's GameObject is used. " +
            "If Circuit is also missing, this Solver GameObject is used.")]
        [SerializeField]
        private Transform componentSearchRoot;

        [Tooltip(
            "Used only when automatic discovery is disabled.")]
        [SerializeField]
        private SparkElectricalComponent[] electricalComponents =
            Array.Empty<SparkElectricalComponent>();

        /// <summary>
        /// Current electrical components used by the solver.
        /// </summary>
        public IReadOnlyList<SparkElectricalComponent> ElectricalComponents =>
            electricalComponents;

            private SparkElectricalComponent[] subscribedComponents =
    Array.Empty<SparkElectricalComponent>();

        // ---------------------------------------------------------------------
        // LATEST SOLVE RESULT
        // ---------------------------------------------------------------------

        private SparkElectricalSolveResult latestSolveResult;

        /// <summary>
        /// Latest successfully created electrical solve result.
        /// </summary>
        public SparkElectricalSolveResult LatestSolveResult =>
            latestSolveResult;




        // ---------------------------------------------------------------------
        // SOLVER SETTINGS
        // ---------------------------------------------------------------------

        [Header("Solver")]

        [SerializeField, Min(1)]
        private int maxIterations = 16;

        [SerializeField, Min(0.000000001f)]
        private float convergenceTolerance = 0.000001f;

        [SerializeField, Min(0.000000001f)]
        private float minimumResistance = 0.000001f;

        [SerializeField, Min(0f)]
        private float floatingNodeLeakResistance = 1000000000f;

        [SerializeField]
        private bool solveOnDirty = true;

        // ---------------------------------------------------------------------
        // TRANSIENT
        // ---------------------------------------------------------------------

        [Header("Transient")]

        [SerializeField]
        private bool simulateTransientDevices = true;

        [SerializeField, Min(0.000001f)]
        private float timeStep = 1f / 60f;

        // ---------------------------------------------------------------------
        // RUNTIME STATE
        // ---------------------------------------------------------------------

        private readonly Dictionary<
            SparkElectricalComponent,
            bool>
            nonlinearStates =
                new Dictionary<
                    SparkElectricalComponent,
                    bool>();

        private readonly Dictionary<
            SparkElectricalComponent,
            float>
            previousComponentVoltages =
                new Dictionary<
                    SparkElectricalComponent,
                    float>();

        private readonly List<
            SparkCircuitConnection>
            connections =
                new List<
                    SparkCircuitConnection>();

        private readonly List<
            SparkCircuitConnection>
            connectionBuffer =
                new List<
                    SparkCircuitConnection>();

        private readonly List<
            SparkTerminal>
            terminals =
                new List<
                    SparkTerminal>();

        private float[] nodeVoltages =
            Array.Empty<float>();

        private double[] reducedSolution =
            Array.Empty<double>();

        private SparkElectricalNetworkGraph currentGraph;

        private bool dirty = true;

        private int topologyVersion = -1;

        private bool solving;

        private bool solveRequestedDuringSolve;

        // ---------------------------------------------------------------------
        // PUBLIC STATE
        // ---------------------------------------------------------------------

        public bool IsSolving =>
            solving;

        public bool IsDirty =>
            dirty;

        public SparkElectricalNetworkGraph CurrentGraph =>
            currentGraph;

        public float[] NodeVoltages =>
            nodeVoltages;

        public double[] ReducedSolution =>
            reducedSolution;

        public SparkElectricalSolverRegistry SolverRegistry =>
            solverRegistry;

        // ---------------------------------------------------------------------
        // SOLVE EVENTS
        // ---------------------------------------------------------------------

        /// <summary>
        /// Raised after a complete electrical solve succeeds.
        /// </summary>
        public event Action SolveCompleted;

        /// <summary>
        /// Raised when an electrical solve fails.
        /// </summary>
        public event Action SolveFailed;

        // ---------------------------------------------------------------------
        // UNITY
        // ---------------------------------------------------------------------

        private void Awake()
        {

            
            dirty = true;
            latestSolveResult = null;
        }

       private void OnEnable()
{
    dirty = true;
    latestSolveResult = null;

    if (solverRegistry != null)
    {
        solverRegistry.ClearRuntimeCache();
    }

    if (circuit != null)
    {
        circuit.TopologyChanged += HandleCircuitTopologyChanged;
    }

    RefreshElectricalComponents();
    if (isActiveAndEnabled)
{
    RebindElectricalComponentEvents();
}
}

private void OnDisable()
{
    if (circuit != null)
    {
        circuit.TopologyChanged -= HandleCircuitTopologyChanged;
    }

    UnbindElectricalComponentEvents();
}

private void HandleCircuitTopologyChanged()
{
    MarkTopologyDirty();
}


private void HandleElectricalConfigurationChanged()
{
    MarkDirty();

    if (!isActiveAndEnabled)
    {
        return;
    }

    if (solving)
    {
        // Preserve the request until the current solve finishes.
        solveRequestedDuringSolve = true;
        return;
    }

    SolveNow();
}
private void RebindElectricalComponentEvents()
{
    UnbindElectricalComponentEvents();

    if (electricalComponents == null)
    {
        subscribedComponents =
            Array.Empty<SparkElectricalComponent>();

        Debug.LogWarning(
            "[SPARK SOLVER] Cannot subscribe: electricalComponents is null.",
            this);

        return;
    }

    subscribedComponents =
        (SparkElectricalComponent[])electricalComponents.Clone();

    int subscribedCount = 0;

    for (int i = 0; i < subscribedComponents.Length; i++)
    {
        SparkElectricalComponent component =
            subscribedComponents[i];

        if (component == null)
        {
            Debug.LogWarning(
                $"[SPARK SOLVER] Null component at index {i}.",
                this);

            continue;
        }

        component.ElectricalConfigurationChanged +=
            HandleElectricalConfigurationChanged;

        subscribedCount++;

       /* Debug.Log(
            $"[SPARK SOLVER] Subscribed to configuration changes | " +
            $"Component={component.name} | " +
            $"InstanceID={component.GetInstanceID()}",
            component);*/
    }

   /* Debug.Log(
        $"[SPARK SOLVER] Event subscriptions rebound | " +
        $"Components={subscribedComponents.Length} | " +
        $"Subscribed={subscribedCount}",
        this);*/
}

private void UnbindElectricalComponentEvents()
{
    if (subscribedComponents == null)
        return;

    for (int i = 0; i < subscribedComponents.Length; i++)
    {
        SparkElectricalComponent component =
            subscribedComponents[i];

        if (component == null)
            continue;

        component.ElectricalConfigurationChanged -=
            HandleElectricalConfigurationChanged;
    }

    subscribedComponents =
        Array.Empty<SparkElectricalComponent>();
}

        // ---------------------------------------------------------------------
        // EXTERNAL DIRTY
        // ---------------------------------------------------------------------
public void MarkDirty()
{
    dirty = true;

    Debug.Log(
        $"[SPARK SOLVER] MarkDirty called | " +
        $"InstanceID={GetInstanceID()} | " +
        $"TopologyVersion={topologyVersion} | " +
        $"HasResult={latestSolveResult != null}",
        this);
}

        /// <summary>
        /// Marks both electrical topology and solver-definition lookup
        /// state as needing refresh.
        /// </summary>
        public void MarkTopologyDirty()
        {
            dirty = true;

            // Force graph rebuild even if the circuit's own
            // TopologyVersion did not change.
            topologyVersion = -1;

            currentGraph = null;

            if (solverRegistry != null)
            {
                solverRegistry.ClearRuntimeCache();
            }
        }

        // ---------------------------------------------------------------------
        // MAIN SOLVE
        // ---------------------------------------------------------------------



        void Update()
        {
            if (UnityEngine.InputSystem.Keyboard.current != null &&
        UnityEngine.InputSystem.Keyboard.current.kKey.wasPressedThisFrame)
    {
        SolveNow();
    }
            
        }

        private int solveCallSequence;


            public bool Solve()
            {
                int callId = ++solveCallSequence;

                Debug.Log(
                    $"[SPARK SOLVER #{callId}] ENTER | " +
                    $"Solving={solving} | Dirty={dirty} | " +
                    $"HasResult={latestSolveResult != null} | " +
                    $"SolveOnDirty={solveOnDirty} | " +
                    $"TopologyVersion={topologyVersion}",
                    this);

               
              if (solving)
            {
                solveRequestedDuringSolve = true;

                Debug.LogWarning(
                    $"[SPARK SOLVER #{callId}] " +
                    "Solve request deferred because a solve is already running.",
                    this);

                return false;
            }

                // Reuse the cached result only when it is still valid.
                if (solveOnDirty &&
                    !dirty &&
                    latestSolveResult != null)
                {
                    Debug.Log(
                        $"[SPARK SOLVER #{callId}] SKIPPED: clean cached result.",
                        this);

                    return true;
                }

                solving = true;

                bool solved = false;

                try
                {
                    Debug.Log(
                        $"[SPARK SOLVER #{callId}] EXECUTING SolveInternal.",
                        this);

                    solved = SolveInternal();
                }
                catch (Exception exception)
                {
                    Debug.LogError(
                        $"[SPARK SOLVER #{callId}] EXCEPTION.",
                        this);

                    Debug.LogException(exception, this);

                    // Invalidate results if the solve failed unexpectedly.
                    try
                    {
                        ZeroStates();
                    }
                    catch (Exception resetException)
                    {
                        Debug.LogError(
                            "[SPARK SOLVER] Failed to reset electrical states " +
                            "after a solve exception.",
                            this);

                        Debug.LogException(resetException, this);
                    }

                    dirty = true;
                    solved = false;
                }
                finally
                {
                    solving = false;
                }

                // Preserve requests made while the solver was running.
                
                    bool runFollowUpSolve = solveRequestedDuringSolve;

                    if (runFollowUpSolve)
                    {
                        solveRequestedDuringSolve = false;
                        dirty = true;

                        if (isActiveAndEnabled)
                        {
                            return SolveNow();
                        }

                        // Keep the request pending until the solver can run again.
                        solveRequestedDuringSolve = true;
                    }


                if (solved)
                {
                    Debug.Log(
                        $"[SPARK SOLVER #{callId}] COMPLETED EVENT.",
                        this);

                   InvokeSolveEventSafely(
                    SolveCompleted,
                    nameof(SolveCompleted));
                }
                else
                {
                    // A failed numerical solve must remain eligible for retry.
                    dirty = true;

                  InvokeSolveEventSafely(
                    SolveFailed,
                    nameof(SolveFailed));
                }

                Debug.Log(
                    $"[SPARK SOLVER #{callId}] EXIT | " +
                    $"Success={solved} | Dirty={dirty} | " +
                    $"HasResult={latestSolveResult != null}",
                    this);

                return solved;
            }


        private void InvokeSolveEventSafely(
            Action solveEvent,
            string eventName)
        {
            if (solveEvent == null)
                return;

            Delegate[] subscribers = solveEvent.GetInvocationList();

            for (int i = 0; i < subscribers.Length; i++)
            {
                try
                {
                    ((Action)subscribers[i]).Invoke();
                }
                catch (Exception exception)
                {
                    Debug.LogError(
                        "[SPARK SOLVER] Subscriber failed during " +
                        eventName + ".",
                        this);

                    Debug.LogException(exception, this);
                }
            }
        }


        /// <summary>
        /// Explicit public solve entry point used by measurement,
        /// level gameplay and other systems.
        /// </summary>
        public bool SolveNow()
        {
            return Solve();
        }

        private bool SolveInternal()
        {
                  if (solverRegistry == null)
        {
            Debug.LogError(
                "[ELECTRICAL SOLVER] Solver registry is missing.",
                this);

            ZeroStates();
            dirty = true;

            return false;
        }

                if (circuit == null)
        {
            Debug.LogError(
                "[ELECTRICAL SOLVER] Circuit reference is missing.",
                this);

            ZeroStates();
            dirty = true;

            return false;
        }

            if (!RefreshTopologyIfRequired())
            {
                return true;
            }

           if (currentGraph == null)
            {
                ZeroStates();
                dirty = true;

                return false;
            }

            int referenceNode =
                FindGroundNode(
                    currentGraph);

            if (referenceNode < 0)
            {
                referenceNode = 0;
            }

            SparkElectricalSolveContext context =
                new SparkElectricalSolveContext(
                    currentGraph,
                    referenceNode,
                    solverRegistry,
                    electricalComponents,
                    nonlinearStates,
                    previousComponentVoltages,
                    simulateTransientDevices,
                    timeStep,
                    minimumResistance,
                    floatingNodeLeakResistance);

            // -------------------------------------------------------------
            // REGISTER DEVICE TOPOLOGY
            // -------------------------------------------------------------

            RegisterDeviceTopology(
                context);

            // -------------------------------------------------------------
            // CREATE MNA MATRIX
            // -------------------------------------------------------------

            context.InitializeMatrix();

            if (context.MatrixSize <= 0)
            {
                nodeVoltages =
                    new float[
                        currentGraph.NodeCount];

                reducedSolution =
                    Array.Empty<double>();

                if (referenceNode >= 0 &&
                    referenceNode < nodeVoltages.Length)
                {
                    nodeVoltages[referenceNode] =
                        0f;
                }

                ApplyZeroResult(
                    context);

                dirty = false;

                return true;
            }

            // -------------------------------------------------------------
            // ITERATIVE NONLINEAR SOLVE
            // -------------------------------------------------------------

            EnsureNodeVoltageBuffer();

            float[] iterationVoltages =
                new float[
                    currentGraph.NodeCount];

            Array.Copy(
                nodeVoltages,
                iterationVoltages,
                nodeVoltages.Length);
            double[] finalSolution = null;
            bool solved = false;
            bool converged = false;

            for (int iteration = 0;
                 iteration < maxIterations;
                 iteration++)
            {
                context.SetIterationVoltages(
                    iterationVoltages);

                context.InitializeMatrix();

                context.AddFloatingNodeLeaks();

                StampAllDevices(
                    context);

                if (!SolveMatrix(
                        context.Matrix,
                        context.RightHandSide,
                        out double[] solution))
                {
                    break;
                }

                context.RestoreNodeVoltages(
                    solution,
                    nodeVoltages);

                float maximumDelta =
                    CalculateMaximumVoltageDelta(
                        iterationVoltages,
                        nodeVoltages);

                finalSolution =
                    solution;

                solved =
                    true;

                if (maximumDelta <= convergenceTolerance)
                {
                    converged = true;
                    break;
                }

                Array.Copy(
                    nodeVoltages,
                    iterationVoltages,
                    nodeVoltages.Length);
            }

                if (!solved ||
                !converged ||
                finalSolution == null)
            {
                ZeroStates();
                dirty = true;

                return false;
            }
            // -------------------------------------------------------------
            // SAVE FINAL SOLUTION
            // -------------------------------------------------------------

            reducedSolution =
                finalSolution;
                

            // -------------------------------------------------------------
            // BUILD RESULT
            // -------------------------------------------------------------

            SparkElectricalSolveResult result =
                new SparkElectricalSolveResult(
                    context,
                    nodeVoltages,
                    reducedSolution);

            latestSolveResult =
                result;

            // -------------------------------------------------------------
            // APPLY DEVICE STATES
            // -------------------------------------------------------------

            
        ApplyAllDeviceStates(result);

        CommitTransientStates(result);

        // Update terminal voltages from the newly solved matrix first.
        ApplyTerminalStates(result);

        // Now diagnostics read the current solve, not the previous terminal state.
        DebugPowerSupplyVoltage();

        dirty = false;

        Debug.Log(
            $"[SOLVER TEST] Finished | Success={solved} | " +
            $"Dirty={dirty} | HasResult={latestSolveResult != null}",
            this);


                    return true;
        }

        // ---------------------------------------------------------------------
        // AUTOMATIC COMPONENT DISCOVERY
        // ---------------------------------------------------------------------

        /// <summary>
        /// Discovers all SparkElectricalComponent objects under the
        /// configured search root.
        ///
        /// Returns true when the discovered component list changed.
        /// </summary>
        private bool RefreshElectricalComponents()
        {
            if (!autoDiscoverElectricalComponents)
            {
                if (electricalComponents == null)
                {
                    electricalComponents =
                        Array.Empty<SparkElectricalComponent>();

                    return true;
                }

                return false;
            }

            Transform root =
                componentSearchRoot;

            if (root == null)
            {
                if (circuit != null)
                {
                    root =
                        circuit.transform;
                }
                else
                {
                    root =
                        transform;
                }
            }

            SparkElectricalComponent[] discovered =
                root.GetComponentsInChildren<
                    SparkElectricalComponent>(
                        true);

            if (discovered == null)
            {
                discovered =
                    Array.Empty<SparkElectricalComponent>();
            }

            bool changed =
                !AreComponentArraysEqual(
                    electricalComponents,
                    discovered);

            if (!changed)
            {
                return false;
            }

            electricalComponents =
                discovered;

             if (isActiveAndEnabled)
            {
                RebindElectricalComponentEvents();
            }

            // Component topology changed.
            dirty = true;
            topologyVersion = -1;
            currentGraph = null;

            if (solverRegistry != null)
            {
                solverRegistry.ClearRuntimeCache();
            }

            return true;
        }

        /// <summary>
        /// Compares component arrays by object reference.
        /// No LINQ or allocations are used.
        /// </summary>
        private bool AreComponentArraysEqual(
            SparkElectricalComponent[] first,
            SparkElectricalComponent[] second)
        {
            if (ReferenceEquals(
                    first,
                    second))
            {
                return true;
            }

            if (first == null ||
                second == null)
            {
                return false;
            }

            if (first.Length !=
                second.Length)
            {
                return false;
            }

            for (int i = 0;
                 i < first.Length;
                 i++)
            {
                if (first[i] !=
                    second[i])
                {
                    return false;
                }
            }

            return true;
        }

        // ---------------------------------------------------------------------
        // TOPOLOGY
        // ---------------------------------------------------------------------

      
        private bool RefreshTopologyIfRequired()
        {
            bool componentsChanged = RefreshElectricalComponents();

            int currentVersion = circuit.TopologyVersion;

            bool graphNeedsRefresh =
                currentGraph == null ||
                currentVersion != topologyVersion ||
                componentsChanged;

            if (graphNeedsRefresh)
            {
                currentGraph = BuildGraph();
                topologyVersion = currentVersion;
                dirty = true;
            }

            if (solveOnDirty && !dirty)
                return false;

            return true;
        }


        // ---------------------------------------------------------------------
        // GRAPH BUILD
        // ---------------------------------------------------------------------

        private SparkElectricalNetworkGraph BuildGraph()
        {
            terminals.Clear();
            connections.Clear();

            var terminalSet =
                new HashSet<SparkTerminal>();

            var componentTerminals =
                new Dictionary<
                    SparkElectricalComponent,
                    SparkTerminal[]>();

            // -------------------------------------------------------------
            // COMPONENT TERMINALS
            // -------------------------------------------------------------

            if (electricalComponents != null)
            {
                for (int i = 0;
                     i < electricalComponents.Length;
                     i++)
                {
                    SparkElectricalComponent component =
                        electricalComponents[i];

                    if (component == null)
                        continue;

                    SparkTerminal[] found =
                        component.GetComponentsInChildren<
                            SparkTerminal>(
                            true);

                    if (found == null ||
                        found.Length == 0)
                    {
                        continue;
                    }

                    var validComponentTerminals =
                        new List<SparkTerminal>(
                            found.Length);

                    for (int t = 0;
                         t < found.Length;
                         t++)
                    {
                        SparkTerminal terminal =
                            found[t];

                        if (terminal == null)
                            continue;

                        terminalSet.Add(
                            terminal);

                        if (!validComponentTerminals.Contains(
                                terminal))
                        {
                            validComponentTerminals.Add(
                                terminal);
                        }
                    }

                    if (validComponentTerminals.Count > 0)
                    {
                        componentTerminals[component] =
                            validComponentTerminals.ToArray();
                    }
                }
            }

            // -------------------------------------------------------------
            // CIRCUIT CONNECTIONS
            // -------------------------------------------------------------

            connectionBuffer.Clear();

            circuit.CopyConnections(
                connectionBuffer);

            for (int i = 0;
                 i < connectionBuffer.Count;
                 i++)
            {
                SparkCircuitConnection connection =
                    connectionBuffer[i];

                if (!connection.IsValid)
                    continue;

                connections.Add(
                    connection);

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

            // -------------------------------------------------------------
            // FINAL TERMINAL LIST
            // -------------------------------------------------------------

            foreach (SparkTerminal terminal
                     in terminalSet)
            {
                if (terminal != null)
                {
                    terminals.Add(
                        terminal);
                }
            }

            // -------------------------------------------------------------
            // TERMINAL INDEX
            // -------------------------------------------------------------

            var terminalIndex =
                new Dictionary<
                    SparkTerminal,
                    int>(
                    terminals.Count);

            for (int i = 0;
                 i < terminals.Count;
                 i++)
            {
                terminalIndex[
                    terminals[i]] =
                    i;
            }

            // -------------------------------------------------------------
            // UNION FIND
            // -------------------------------------------------------------

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
                    continue;

                // Probe connections intentionally do NOT
                // electrically short nodes.
                if (IsProbeConnection(
                        connection))
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

            // -------------------------------------------------------------
            // GRAPH NODE IDS
            // -------------------------------------------------------------

            var rootToNode =
                new Dictionary<int, int>();

            var terminalNode =
                new Dictionary<
                    SparkTerminal,
                    int>();

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

            return new SparkElectricalNetworkGraph(
                nodeCount,
                rootToNode,
                terminalNode,
                componentTerminals);
        }

        // ---------------------------------------------------------------------
        // DEFINITION LOOKUP
        // ---------------------------------------------------------------------
private bool TryGetSolverDefinition(
    SparkElectricalComponent component,
    out SparkElectricalSolverDefinition definition)
{
    definition = null;

    if (component == null)
        return false;

    if (solverRegistry == null)
        return false;

    if (solverRegistry.IsIgnored(component))
        return false;

    return solverRegistry.TryGetDefinition(
        component,
        out definition);
}


private bool ShouldIgnoreComponent(
    SparkElectricalComponent component)
{
    if (component == null)
        return true;

    if (solverRegistry == null)
        return false;

    return solverRegistry.IsIgnored(component);
}
        // ---------------------------------------------------------------------
        // TOPOLOGY REGISTRATION
        // ---------------------------------------------------------------------

        private void RegisterDeviceTopology(
    SparkElectricalSolveContext context)
{
    if (electricalComponents == null)
        return;

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

        // Helper / temporary objects are not solver devices.
        if (ShouldIgnoreComponent(component))
        {
            continue;
        }

        if (!TryGetSolverDefinition(
                component,
                out SparkElectricalSolverDefinition definition))
        {
            Debug.LogError(
                $"[ELECTRICAL SOLVER] No solver definition " +
                $"registered for {component.GetType().Name} " +
                $"on '{component.name}'.",
                component);

            continue;
        }

        definition.RegisterTopology(
            component,
            context);
    }
}

        // ---------------------------------------------------------------------
        // DEVICE STAMPING
        // ---------------------------------------------------------------------

       private void StampAllDevices(
    SparkElectricalSolveContext context)
{
    if (electricalComponents == null)
        return;

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

        if (ShouldIgnoreComponent(component))
        {
            continue;
        }

        if (!TryGetSolverDefinition(
                component,
                out SparkElectricalSolverDefinition definition))
        {
            continue;
        }
        
#if UNITY_EDITOR
for (int j = 0; j < electricalComponents.Length; j++)
{
    if (!(electricalComponents[j] is SparkSwitch sw))
        continue;

    if (!context.Graph.TryGetNode(
            sw.InputTerminal, out int inputNode) ||
        !context.Graph.TryGetNode(
            sw.OutputTerminal, out int outputNode))
    {
        Debug.LogWarning(
            "[SWITCH MATRIX DEBUG] Could not resolve switch nodes.",
            sw);

        continue;
    }
   
}
#endif
        definition.Stamp(
            component,
            context);
    }
}

        // ---------------------------------------------------------------------
        // DEVICE STATE APPLICATION
        // ---------------------------------------------------------------------

       private void ApplyAllDeviceStates(
    SparkElectricalSolveResult result)
{
    if (electricalComponents == null)
        return;

    for (int i = 0;
         i < electricalComponents.Length;
         i++)
    {
        SparkElectricalComponent component =
            electricalComponents[i];

        if (component == null)
            continue;

        if (ShouldIgnoreComponent(component))
        {
            continue;
        }

        if (!TryGetSolverDefinition(
                component,
                out SparkElectricalSolverDefinition definition))
        {
            continue;
        }

        definition.ApplySolvedState(
            component,
            result);
    }
}
        // ---------------------------------------------------------------------
        // TRANSIENT COMMIT
        // ---------------------------------------------------------------------

       private void CommitTransientStates(
    SparkElectricalSolveResult result)
{
    if (!simulateTransientDevices)
        return;

    if (electricalComponents == null)
        return;

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

        if (ShouldIgnoreComponent(component))
        {
            continue;
        }

        if (!TryGetSolverDefinition(
                component,
                out SparkElectricalSolverDefinition definition))
        {
            continue;
        }

        definition.CommitTransientState(
            component,
            result);
    }
}

        // ---------------------------------------------------------------------
        // TERMINAL STATE
        // ---------------------------------------------------------------------

            private void ApplyTerminalStates(
            SparkElectricalSolveResult result)
        {
            if (result == null)
                return;

            SparkElectricalSolveContext context =
                result.Context;

            if (context == null)
                return;

            SparkElectricalNetworkGraph graph =
                context.Graph;

            if (graph == null)
                return;

            IReadOnlyDictionary<SparkTerminal, int> terminalNodes =
                graph.TerminalNode;

            if (terminalNodes == null)
                return;

            foreach (KeyValuePair<SparkTerminal, int> pair
                in terminalNodes)
            {
                SparkTerminal terminal =
                    pair.Key;

                if (terminal == null)
                    continue;

                int node =
                    pair.Value;

                
            if (!result.TryGetNodeVoltage(
                    node,
                    out float voltage))
            {
                if (terminal.name == "Anode" ||
                    terminal.name == "Cathode")
                {
                    Debug.LogWarning(
                        "[SPARK SOLVER TERMINAL VOLTAGE MISSING] " +
                        "Terminal=" + terminal.name +
                        " | Owner=" +
                        (terminal.Owner != null
                            ? terminal.Owner.name
                            : "None") +
                        " | InstanceID=" + terminal.GetInstanceID() +
                        " | Node=" + node);
                }

                terminal.ClearElectricalState();
                continue;
            }

            if (terminal.Owner != null &&
                terminal.Owner.name == "LED")
            {
                Debug.Log(
                    "[SPARK SOLVER LED TERMINAL VOLTAGE] " +
                    "Terminal=" + terminal.name +
                    " | InstanceID=" + terminal.GetInstanceID() +
                    " | Node=" + node +
                    " | SolvedVoltage=" + voltage);
            }


                // Terminal current is deliberately reset here.
                //
                // Individual solver definitions are responsible for
                // calculating component currents. The terminal voltage,
                // however, comes directly from the solved MNA node.
                terminal.ApplyElectricalState(
                    new SparkTerminalElectricalState(
                        voltage,
                        0f));
            }
        }
        // ---------------------------------------------------------------------
        // GENERIC CURRENT CALCULATION
        // ---------------------------------------------------------------------

        /// <summary>
        /// Calculates current through a component using the solver
        /// definition associated with that component.
        ///
        /// This is used by measurement systems such as the multimeter.
        /// </summary>
       public bool TryCalculateComponentCurrent(
    SparkElectricalComponent component,
    out float current)
{
            if (dirty)
        {
            current = 0f;
            return false;
        }
    current = 0f;

            if (component == null ||
            ShouldIgnoreComponent(component))
        {
            current = 0f;
            return false;
        }

    if (latestSolveResult == null)
        return false;

    if (ShouldIgnoreComponent(component))
        return false;

    if (!TryGetSolverDefinition(
            component,
            out SparkElectricalSolverDefinition definition))
    {
        return false;
    }

    return definition.TryCalculateCurrent(
        component,
        latestSolveResult,
        out current);
}
        // ---------------------------------------------------------------------
        // MAXIMUM ACTIVE SOURCE VOLTAGE
        // ---------------------------------------------------------------------

        public float GetMaximumActiveSourceVoltage()
{
    float maximum =
        0f;

    if (solverRegistry == null ||
        electricalComponents == null)
    {
        return maximum;
    }

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

        if (ShouldIgnoreComponent(component))
        {
            continue;
        }

        if (!TryGetSolverDefinition(
                component,
                out SparkElectricalSolverDefinition definition))
        {
            continue;
        }

        if (!definition.TryGetSourceVoltage(
                component,
                out float voltage))
        {
            continue;
        }

        maximum =
            Mathf.Max(
                maximum,
                Mathf.Abs(voltage));
    }

    return maximum;
}

        // ---------------------------------------------------------------------
        // MATRIX SOLVER
        // ---------------------------------------------------------------------

        private bool SolveMatrix(
            double[,] matrix,
            double[] rhs,
            out double[] solution)
        {
            solution =
                null;

            if (matrix == null ||
                rhs == null)
            {
                return false;
            }

            int size =
                rhs.Length;

            if (matrix.GetLength(0) != size ||
                matrix.GetLength(1) != size)
            {
                return false;
            }

            if (size == 0)
            {
                solution =
                    Array.Empty<double>();

                return true;
            }

            double[,] a =
                new double[
                    size,
                    size];

            double[] b =
                new double[size];

            Array.Copy(
                matrix,
                a,
                matrix.Length);

            Array.Copy(
                rhs,
                b,
                rhs.Length);

            const double pivotEpsilon =
                1e-12;

            for (int column = 0;
                 column < size;
                 column++)
            {
                int pivotRow =
                    column;

                double pivotMagnitude =
                    Math.Abs(
                        a[column, column]);

                for (int row = column + 1;
                     row < size;
                     row++)
                {
                    double magnitude =
                        Math.Abs(
                            a[row, column]);

                    if (magnitude >
                        pivotMagnitude)
                    {
                        pivotMagnitude =
                            magnitude;

                        pivotRow =
                            row;
                    }
                }

                if (pivotMagnitude <
                    pivotEpsilon)
                {
                    return false;
                }

                if (pivotRow != column)
                {
                    for (int j = column;
                         j < size;
                         j++)
                    {
                        double temp =
                            a[column, j];

                        a[column, j] =
                            a[pivotRow, j];

                        a[pivotRow, j] =
                            temp;
                    }

                    double rhsTemp =
                        b[column];

                    b[column] =
                        b[pivotRow];

                    b[pivotRow] =
                        rhsTemp;
                }

                double pivot =
                    a[column, column];

                for (int row = column + 1;
                     row < size;
                     row++)
                {
                    double factor =
                        a[row, column] /
                        pivot;

                    if (Math.Abs(factor) <
                        pivotEpsilon)
                    {
                        continue;
                    }

                    a[row, column] =
                        0d;

                    for (int j = column + 1;
                         j < size;
                         j++)
                    {
                        a[row, j] -=
                            factor *
                            a[column, j];
                    }

                    b[row] -=
                        factor *
                        b[column];
                }
            }

            solution =
                new double[size];

            for (int row = size - 1;
                 row >= 0;
                 row--)
            {
                double value =
                    b[row];

                for (int column = row + 1;
                     column < size;
                     column++)
                {
                    value -=
                        a[row, column] *
                        solution[column];
                }

                double diagonal =
                    a[row, row];

                if (Math.Abs(diagonal) <
                    pivotEpsilon)
                {
                    return false;
                }

                solution[row] =
                    value /
                    diagonal;
            }

            return true;
        }

        // ---------------------------------------------------------------------
        // CONVERGENCE
        // ---------------------------------------------------------------------

        private float CalculateMaximumVoltageDelta(
            float[] previous,
            float[] current)
        {
            if (previous == null ||
                current == null)
            {
                return float.MaxValue;
            }

            int count =
                Mathf.Min(
                    previous.Length,
                    current.Length);

            float maximum =
                0f;

            for (int i = 0;
                 i < count;
                 i++)
            {
                float delta =
                    Mathf.Abs(
                        current[i] -
                        previous[i]);

                if (delta > maximum)
                {
                    maximum =
                        delta;
                }
            }

            return maximum;
        }

        // ---------------------------------------------------------------------
        // GROUND
        // ---------------------------------------------------------------------

        private int FindGroundNode(
            SparkElectricalNetworkGraph graph)
        {
            if (graph == null)
                return -1;

            // -------------------------------------------------------------
            // REGISTERED SOURCE REFERENCE
            // -------------------------------------------------------------

            if (electricalComponents != null)
            {
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

                    if (ShouldIgnoreComponent(component))
                    {
                        continue;
                    }

                    if (!TryGetSolverDefinition(
                            component,
                            out SparkElectricalSolverDefinition definition))
                    {
                        continue;
                    }

                    if (!definition.TryGetReferenceTerminal(
                            component,
                            out SparkTerminal terminal))
                    {
                        continue;
                    }

                    if (terminal == null)
                        continue;

                    if (graph.TryGetNode(
                            terminal,
                            out int node))
                    {
                        return node;
                    }
                }
            }

            // -------------------------------------------------------------
            // FALLBACK GROUND TERMINAL
            // -------------------------------------------------------------

            foreach (KeyValuePair<
                         SparkTerminal,
                         int> pair
                     in graph.TerminalNode)
            {
                SparkTerminal terminal =
                    pair.Key;

                if (terminal == null)
                    continue;

                string terminalName =
                    terminal.name;

                if (string.Equals(
                        terminalName,
                        "GND",
                        StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(
                        terminalName,
                        "Ground",
                        StringComparison.OrdinalIgnoreCase))
                {
                    return pair.Value;
                }
            }

            return graph.NodeCount > 0
                ? 0
                : -1;
        }

        // ---------------------------------------------------------------------
        // ZERO STATE
        // ---------------------------------------------------------------------

                
        private void ZeroStates()
        {
            dirty = true;

            nonlinearStates.Clear();
            previousComponentVoltages.Clear();

            latestSolveResult = null;

            nodeVoltages = Array.Empty<float>();
            reducedSolution = Array.Empty<double>();

            // Clear terminal states so a failed solve cannot leave
            // voltage readings from the previous successful solve.
            if (terminals != null)
            {
                for (int i = 0; i < terminals.Count; i++)
                {
                    SparkTerminal terminal = terminals[i];

                    if (terminal != null)
                        terminal.ClearElectricalState();
                }
            }

            if (electricalComponents == null)
                return;

            for (int i = 0; i < electricalComponents.Length; i++)
            {
                SparkElectricalComponent component =
                    electricalComponents[i];

                if (component == null)
                    continue;

                if (ShouldIgnoreComponent(component))
                    continue;

                if (!TryGetSolverDefinition(
                        component,
                        out SparkElectricalSolverDefinition definition))
                {
                    continue;
                }

                definition.ResetRuntimeState(component);
            }
        }

       
private void ApplyZeroResult(
    SparkElectricalSolveContext context)
{
    if (context == null)
    {
        throw new ArgumentNullException(
            nameof(context));
    }

    // Ensure the zero-state result always has valid arrays.
    if (nodeVoltages == null)
    {
        nodeVoltages = Array.Empty<float>();
    }

    if (reducedSolution == null)
    {
        reducedSolution = Array.Empty<double>();
    }

    
        SparkElectricalSolveResult result =
            new SparkElectricalSolveResult(
                context,
                nodeVoltages,
                reducedSolution);

        // Apply the newly calculated solution.
        ApplyAllDeviceStates(result);
        ApplyTerminalStates(result);

        // Commit transient state only after the solve and
        // state application have succeeded.
        CommitTransientStates(result);

        DebugPowerSupplyVoltage();

        // Publish after state application succeeds.
        latestSolveResult = result;
        dirty = false;

        }


        // ---------------------------------------------------------------------
        // NODE BUFFER
        // ---------------------------------------------------------------------

        private void EnsureNodeVoltageBuffer()
        {
            int count =
                currentGraph != null
                    ? currentGraph.NodeCount
                    : 0;

            if (nodeVoltages == null ||
                nodeVoltages.Length != count)
            {
                nodeVoltages =
                    new float[count];
            }
        }

        // ---------------------------------------------------------------------
        // PROBE CONNECTION
        // ---------------------------------------------------------------------

        private bool IsProbeConnection(
            SparkCircuitConnection connection)
        {
            if (!connection.IsValid)
            {
                return false;
            }

            return connection.Kind ==
                   SparkConnectionKind.Probe;
        }

        // ---------------------------------------------------------------------
        // UNION FIND
        // ---------------------------------------------------------------------

        private sealed class UnionFind
        {
            private readonly int[] parent;
            private readonly byte[] rank;

            public UnionFind(
                int count)
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

            public int Find(
                int value)
            {
                if (value < 0 ||
                    value >= parent.Length)
                {
                    return -1;
                }

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

                if (rootA < 0 ||
                    rootB < 0 ||
                    rootA == rootB)
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
                private void DebugPowerSupplyVoltage()
{
    if (electricalComponents == null)
        return;

    for (int i = 0; i < electricalComponents.Length; i++)
    {
        if (!(electricalComponents[i] is SparkPowerSupply supply))
            continue;

        float positiveVoltage =
            supply.PositiveTerminal != null
                ? supply.PositiveTerminal.ElectricalState.Voltage
                : float.NaN;

        float negativeVoltage =
            supply.NegativeTerminal != null
                ? supply.NegativeTerminal.ElectricalState.Voltage
                : float.NaN;

/*        Debug.Log(
            $"[POWER SUPPLY DIAGNOSTIC] {supply.name} | " +
            $"ConfiguredV={supply.OutputVoltage:F6} | " +
            $"SupplyActive={supply.IsOutputActive} | " +
            $"ElectricalEnabled={supply.ElectricalEnabled} | " +
            $"Dirty={dirty} | " +
            $"HasResult={latestSolveResult != null} | " +
            $"PositiveV={positiveVoltage:F6} | " +
            $"NegativeV={negativeVoltage:F6} | " +
            $"DeltaV={positiveVoltage - negativeVoltage:F6} | " +
            $"SupplyStateV={supply.ElectricalState.Voltage:F6}",
            supply);*/
    }
}
    }
}