using System;
using System.Collections.Generic;
using ProjectSpark.Circuit;
using ProjectSpark.Electrical;
using UnityEngine;

namespace ProjectSpark.Gameplay
{
    [DisallowMultipleComponent]
    public sealed class Level1CircuitChecker : MonoBehaviour
    {
        [Serializable]
        private sealed class PowerSourceDefinition
        {
            [SerializeField]
            private string sourceName;

            [SerializeField]
            private SparkTerminal positiveTerminal;

            [SerializeField]
            private SparkTerminal negativeTerminal;

            [SerializeField, Min(0f)]
            private float nominalVoltage = 5f;

           

            public string SourceName
            {
                get
                {
                    return string.IsNullOrWhiteSpace(sourceName)
                        ? "POWER SOURCE"
                        : sourceName;
                }
            }

            public SparkTerminal PositiveTerminal =>
                positiveTerminal;

            public SparkTerminal NegativeTerminal =>
                negativeTerminal;

            public float NominalVoltage =>
                nominalVoltage;
        }

        [Header("Circuit System")]
        [SerializeField]
        private SparkCircuitSystem circuitSystem;

        [Header("Electrical Solver")]
        [Tooltip(
            "The solver is the authoritative source of voltage/current. " +
            "Leave empty to auto-find it in the parent hierarchy.")]
        [SerializeField]
        private SparkElectricalSolver electricalSolver;

        [Header("Target Component")]
        [Tooltip(
            "Positive/power terminal of the component being powered. " +
            "For an LED this is normally the anode.")]
        [SerializeField]
        private SparkTerminal targetPositiveTerminal;

        [Tooltip(
            "Negative/return terminal of the component being powered. " +
            "For an LED this is normally the cathode.")]
        [SerializeField]
        private SparkTerminal targetNegativeTerminal;

        [Header("Accepted Power Sources")]
        [Tooltip(
            "Any active source in this list may power the target.")]
        [SerializeField]
        private PowerSourceDefinition[] powerSources;

        [Header("Electrical Requirements")]
        [SerializeField]
        private bool requireMinimumVoltage = true;

        [SerializeField, Min(0f)]
        private float minimumVoltage = 1f;

        [SerializeField]
        private bool requireClosedReturnPath = true;

        [SerializeField]
        private bool rejectShortCircuit = true;

        [Header("Topology Rules")]
        [Tooltip(
            "If enabled, target + and target - may not be in the same " +
            "wire/connectivity group.")]
        [SerializeField]
        private bool rejectTargetShort = true;

        [Tooltip(
            "If enabled, source-to-target paths may contain multiple " +
            "wire/connector hops. If disabled, the source terminal must " +
            "connect directly to the target terminal.")]
        [SerializeField]
        private bool allowIntermediateConnections = true;

        [Tooltip(
            "If enabled, any configured source can satisfy the level. " +
            "If disabled, only the first valid configured source is accepted.")]
        [SerializeField]
        private bool allowAnyConfiguredPowerSource = true;

        [Header("Output")]
        [SerializeField]
        private GameObject lightObject;

        [SerializeField]
        private GameObject poweredIndicator;

        [SerializeField]
        private GameObject faultIndicator;

        [Header("State")]
        [SerializeField]
        private bool positivePowered;

        [SerializeField]
        private bool negativeReturned;

        [SerializeField]
        private bool targetShorted;

        [SerializeField]
        private bool sourceShorted;

        [SerializeField]
        private bool targetPowered;

        [SerializeField]
        private float detectedVoltage;

        [SerializeField]
        private string activePowerSource;

        [Header("Debug")]
        [SerializeField]
        private bool logConnections = true;

        [SerializeField]
        private bool logTopology = true;

        [SerializeField]
        private bool logPowerAnalysis = true;

        private readonly List<SparkCircuitConnection> connectionBuffer =
            new List<SparkCircuitConnection>();

        private readonly Queue<SparkTerminal> traversalQueue =
            new Queue<SparkTerminal>();

        private readonly HashSet<SparkTerminal> visitedTerminals =
            new HashSet<SparkTerminal>();

        private readonly HashSet<SparkTerminal> positiveReachable =
            new HashSet<SparkTerminal>();

        private readonly HashSet<SparkTerminal> negativeReachable =
            new HashSet<SparkTerminal>();

        private readonly List<PowerSourceDefinition> validSources =
            new List<PowerSourceDefinition>();

        private bool evaluating;

      [SerializeField]  private bool gameWon;

        public bool IsGameWon => gameWon;
        public bool PositivePowered => positivePowered;
        public bool NegativeReturned => negativeReturned;
        public bool TargetShorted => targetShorted;
        public bool SourceShorted => sourceShorted;
        public bool TargetPowered => targetPowered;
        public float DetectedVoltage => detectedVoltage;
        public string ActivePowerSource => activePowerSource;
        public int ConnectedTerminalCount => positiveReachable.Count;

        // =========================================================
        // UNITY
        // =========================================================

        private void Reset()
        {
            circuitSystem =
                GetComponentInParent<SparkCircuitSystem>();

            electricalSolver =
                GetComponentInParent<SparkElectricalSolver>();
        }

        private void Awake()
        {
            ResolveDependencies();
            SetOutputState(false);
        }

        private void OnEnable()
        {
            ResolveDependencies();

            if (circuitSystem != null)
            {
                circuitSystem.ConnectionCreated +=
                    OnConnectionCreated;

                circuitSystem.ConnectionRemoved +=
                    OnConnectionRemoved;

                circuitSystem.TopologyChanged +=
                    OnTopologyChanged;
            }

            if (electricalSolver != null)
            {
                electricalSolver.SolveCompleted +=
                    OnSolverCompleted;

                electricalSolver.SolveFailed +=
                    OnSolverFailed;
            }

            RecalculateCircuit();
        }

        private void OnDisable()
        {
            if (circuitSystem != null)
            {
                circuitSystem.ConnectionCreated -=
                    OnConnectionCreated;

                circuitSystem.ConnectionRemoved -=
                    OnConnectionRemoved;

                circuitSystem.TopologyChanged -=
                    OnTopologyChanged;
            }

            if (electricalSolver != null)
            {
                electricalSolver.SolveCompleted -=
                    OnSolverCompleted;

                electricalSolver.SolveFailed -=
                    OnSolverFailed;
            }
        }

        // =========================================================
        // EVENTS
        // =========================================================

        private void OnConnectionCreated(
    SparkCircuitConnection connection)
{
    if (logConnections)
    {
        Debug.Log(
            $"[LEVEL 1] Connection Created: " +
            $"{GetName(connection.A)} ↔ " +
            $"{GetName(connection.B)}",
            this);
    }

    QueueRecalculation();
}

private void OnConnectionRemoved(
    SparkCircuitConnection connection)
{
    if (logConnections)
    {
        Debug.Log(
            $"[LEVEL 1] Connection Removed: " +
            $"{GetName(connection.A)} ↔ " +
            $"{GetName(connection.B)}",
            this);
    }

    QueueRecalculation();
}

        private bool recalculationQueued;

        private void OnTopologyChanged()
        {
            QueueRecalculation();
        }

        private void QueueRecalculation()
        {
            if (recalculationQueued)
                return;

            recalculationQueued = true;
        }
      private void LateUpdate()
            {
                if (!recalculationQueued)
                    return;

                recalculationQueued = false;

                ResolveDependencies();

                if (electricalSolver != null)
                {
                    electricalSolver.SolveNow();
                    return;
                }

                EvaluateCircuit();
            }

               private void OnSolverCompleted()
            {
                if (evaluating)
                    return;

                if (logPowerAnalysis)
                {
                    Debug.Log(
                        "[LEVEL 1] SOLVER COMPLETED → EVALUATING RESULT",
                        this);
                }

                EvaluateFromCurrentElectricalState();
            }

                    private void OnSolverFailed()
            {
                ResetAnalysisState();
                SetOutputState(false);

                Debug.LogError(
                    "[LEVEL 1] SOLVER FAILED / DID NOT CONVERGE",
                    this);
            }
                    // =========================================================
                    // MAIN ANALYSIS
                    // =========================================================

                private void RecalculateCircuit()
                {
                    QueueRecalculation();
                }
            private bool isEvaluating;


            /// <summary>
            /// Performs the actual circuit evaluation while preventing
            /// recursive evaluation caused by synchronous callbacks.
            /// </summary>
            private void EvaluateCircuit()
            {
                if (isEvaluating)
                    return;

                isEvaluating = true;

                try
                {
                    EvaluateFromCurrentElectricalState();
                }
                finally
                {
                    isEvaluating = false;
                }
            }



            /// <summary>
            /// Handles the solver completion callback.
            /// SolveNow() invokes this synchronously while isEvaluating is true.
            /// </summary>

        

        private void EvaluateFromCurrentElectricalState()
        {
            if (evaluating)
                return;

            evaluating = true;

            try
            {
                ResetAnalysisState();

                if (circuitSystem == null)
                {
                    SetOutputState(false);
                    return;
                }

                if (targetPositiveTerminal == null ||
                    targetNegativeTerminal == null)
                {
                    Debug.LogWarning(
                        "[LEVEL 1] Target terminals are not assigned.",
                        this);

                    SetOutputState(false);
                    return;
                }

                FindValidPowerSources();

                if (validSources.Count == 0)
                {
                    if (logPowerAnalysis)
                    {
                        /*Debug.Log(
                            "[LEVEL 1] No active/valid power source configured.",
                            this);*/
                    }

                    SetOutputState(false);
                    return;
                }

                BuildPositiveReachability();
                BuildNegativeReachability();

                targetShorted =
                    IsTargetShorted();

                sourceShorted =
                    IsAnyPowerSourceShorted();

                bool hasTargetElectricalState =
                    TryGetTargetElectricalState(
                        out float targetVoltage,
                        out float targetCurrent,
                        out bool targetConducting);

                detectedVoltage =
                    hasTargetElectricalState
                        ? targetVoltage
                        : 0f;

                positivePowered =
                    IsAnyAcceptedSourceConnectedTo(
                        targetPositiveTerminal);

                negativeReturned =
                    IsAnyAcceptedSourceConnectedTo(
                        targetNegativeTerminal);

                EvaluateAcceptedSource(
                    targetVoltage,
                    targetCurrent,
                    hasTargetElectricalState,
                    targetConducting);

                if (logPowerAnalysis)
                {
                    LogAnalysis();
                }

                CheckWin();

                SetOutputState(
                    targetPowered &&
                    !targetShorted &&
                    !sourceShorted);
            }
            finally
            {
                evaluating = false;
            }
        }

        private void EvaluateAcceptedSource(
            float targetVoltage,
            float targetCurrent,
            bool hasTargetElectricalState,
            bool targetConducting)
        {
            int sourceCount =
                allowAnyConfiguredPowerSource
                    ? validSources.Count
                    : Mathf.Min(1, validSources.Count);

            PowerSourceDefinition selectedSource = null;

            for (int i = 0;
                 i < sourceCount;
                 i++)
            {
                PowerSourceDefinition source =
                    validSources[i];

                if (source == null)
                    continue;

                bool positivePath =
                    IsReachableFromSource(
                        source.PositiveTerminal,
                        targetPositiveTerminal);

                bool negativePath =
                    IsReachableFromSource(
                        source.NegativeTerminal,
                        targetNegativeTerminal);

                if (!positivePath || !negativePath)
                    continue;

                if (rejectShortCircuit &&
                    IsSourceShorted(source))
                {
                    continue;
                }

                selectedSource = source;

                break;
            }

            if (selectedSource == null)
            {
                positivePowered = false;
                negativeReturned = false;
                targetPowered = false;
                detectedVoltage = 0f;
                activePowerSource = string.Empty;
                return;
            }

            positivePowered = true;
            negativeReturned = true;
            activePowerSource = selectedSource.SourceName;

            // With the current ZIP architecture, the solver is the authority.
            // Do not use nominalVoltage when a real solved state is available.
            if (!hasTargetElectricalState)
            {
                // When the real solver is present, missing target electrical
                // state means the target could not be evaluated by the same
                // model that powers the circuit. Do not fall back to a
                // configured nominal voltage and create a false win.
                if (electricalSolver != null)
                {
                    detectedVoltage = 0f;
                    targetPowered = false;
                    return;
                }

                // Legacy fallback when no electrical solver exists.
                detectedVoltage =
                    selectedSource.NominalVoltage;

                targetPowered =
                    !requireMinimumVoltage ||
                    detectedVoltage >= minimumVoltage;

                if (requireClosedReturnPath &&
                    !negativeReturned)
                {
                    targetPowered = false;
                }

                if (rejectTargetShort && targetShorted)
                {
                    targetPowered = false;
                }
            }

            if (!hasTargetElectricalState)
            {
                return;
            }

            // Actual solved target voltage is signed according to the
            // configured target + -> target - orientation.
            if (requireMinimumVoltage &&
                targetVoltage < minimumVoltage)
            {
                targetPowered = false;
                return;
            }

            if (requireClosedReturnPath &&
                !negativeReturned)
            {
                targetPowered = false;
                return;
            }

            if (rejectTargetShort &&
                targetShorted)
            {
                targetPowered = false;
                return;
            }

            if (rejectShortCircuit &&
                sourceShorted)
            {
                targetPowered = false;
                return;
            }

            // If the target is an actual LED, require the solver to report
            // conduction/current rather than accepting topology alone.
            SparkLED led =
                targetPositiveTerminal.Owner as SparkLED;

            if (led != null)
            {
                targetPowered = led.IsOn;
                return;
            }

            // Generic electrical targets are considered powered when they
            // have the required real solved voltage. Current may legitimately
            // be very small for a high-impedance target.
            targetPowered =
                targetVoltage >= minimumVoltage ||
                !requireMinimumVoltage;
        }

        private void LogAnalysis()
        {
            Debug.Log(
                "\n" +
                "========================================\n" +
                "[PROJECT SPARK - LEVEL 1 ANALYSIS]\n" +
                "========================================\n" +
                $"Target +          : {GetName(targetPositiveTerminal)}\n" +
                $"Target -          : {GetName(targetNegativeTerminal)}\n" +
                "\n" +
                $"Positive Powered  : {positivePowered}\n" +
                $"Negative Returned : {negativeReturned}\n" +
                $"Target Shorted    : {targetShorted}\n" +
                $"Source Shorted    : {sourceShorted}\n" +
                "\n" +
                $"Detected Voltage  : {detectedVoltage:F3} V\n" +
                $"Active Source     : {activePowerSource}\n" +
                $"Target Powered    : {targetPowered}\n" +
                $"Game Won          : {gameWon}\n" +
                "\n" +
                $"Positive Nodes    : {positiveReachable.Count}\n" +
                $"Negative Nodes    : {negativeReachable.Count}\n" +
                $"Connections       : {GetConnectionCount()}\n" +
                $"Topology Version  : {GetTopologyVersion()}\n" +
                "========================================",
                this);
        }

        // =========================================================
        // RESET STATE
        // =========================================================

        private void ResetAnalysisState()
        {
            positivePowered = false;
            negativeReturned = false;
            targetShorted = false;
            sourceShorted = false;
            targetPowered = false;
            detectedVoltage = 0f;
            activePowerSource = string.Empty;

            positiveReachable.Clear();
            negativeReachable.Clear();
            validSources.Clear();

            gameWon = false;
        }

        // =========================================================
        // POWER SOURCES
        // =========================================================

      private void FindValidPowerSources()
{
    validSources.Clear();

    if (powerSources == null ||
        powerSources.Length == 0)
    {
        Debug.LogWarning(
            "[LEVEL 1] NO POWER SOURCES CONFIGURED.",
            this);

        return;
    }

    for (int i = 0;
         i < powerSources.Length;
         i++)
    {
        PowerSourceDefinition source =
            powerSources[i];

        if (source == null)
        {
            Debug.LogWarning(
                $"[LEVEL 1] SOURCE [{i}] = NULL",
                this);

            continue;
        }

        SparkTerminal positive =
            source.PositiveTerminal;

        SparkTerminal negative =
            source.NegativeTerminal;


        // =========================================================
        // TERMINAL REFERENCES
        // =========================================================

        if (positive == null)
        {
            Debug.LogWarning(
                $"[LEVEL 1] SOURCE '{source.SourceName}' " +
                "POSITIVE TERMINAL = NULL",
                this);

            continue;
        }

        if (negative == null)
        {
            Debug.LogWarning(
                $"[LEVEL 1] SOURCE '{source.SourceName}' " +
                "NEGATIVE TERMINAL = NULL",
                this);

            continue;
        }

        if (positive == negative)
        {
            Debug.LogWarning(
                $"[LEVEL 1] SOURCE '{source.SourceName}' " +
                "POSITIVE AND NEGATIVE TERMINALS ARE THE SAME.",
                this);

            continue;
        }


        // =========================================================
        // TERMINAL COMPONENT STATE
        // =========================================================

        if (!positive.isActiveAndEnabled)
        {
            Debug.LogWarning(
                $"[LEVEL 1] SOURCE '{source.SourceName}' " +
                $"POSITIVE TERMINAL DISABLED: {positive.name}",
                positive);

            continue;
        }

        if (!negative.isActiveAndEnabled)
        {
            Debug.LogWarning(
                $"[LEVEL 1] SOURCE '{source.SourceName}' " +
                $"NEGATIVE TERMINAL DISABLED: {negative.name}",
                negative);

            continue;
        }


        // =========================================================
        // RESOLVE ACTUAL POWER SUPPLY
        // =========================================================

        SparkPowerSupply supply =
            ResolvePowerSupply(positive);

        if (supply == null)
        {
            supply =
                ResolvePowerSupply(negative);
        }

        if (supply == null)
        {
            Debug.LogWarning(
                $"[LEVEL 1] SOURCE '{source.SourceName}' " +
                "COULD NOT RESOLVE SparkPowerSupply.\n" +
                $"Positive Terminal : {positive.name}\n" +
                $"Negative Terminal : {negative.name}\n" +
                $"Positive Owner    : {GetOwnerName(positive)}\n" +
                $"Negative Owner    : {GetOwnerName(negative)}",
                this);

            continue;
        }


        // =========================================================
        // VERIFY BOTH TERMINALS BELONG TO THIS SOURCE
        // =========================================================

        SparkPowerSupply positiveSupply =
            ResolvePowerSupply(positive);

        SparkPowerSupply negativeSupply =
            ResolvePowerSupply(negative);

        if (positiveSupply != null &&
            negativeSupply != null &&
            positiveSupply != negativeSupply)
        {
            Debug.LogWarning(
                $"[LEVEL 1] SOURCE '{source.SourceName}' " +
                "POSITIVE AND NEGATIVE TERMINALS BELONG " +
                "TO DIFFERENT POWER SUPPLIES.\n" +
                $"+ Supply = {positiveSupply.name}\n" +
                $"- Supply = {negativeSupply.name}",
                this);

            continue;
        }


        // =========================================================
        // POWER SUPPLY ACTIVE
        // =========================================================

        if (!supply.isActiveAndEnabled)
        {
            Debug.LogWarning(
                $"[LEVEL 1] SOURCE '{source.SourceName}' " +
                $"POWER SUPPLY DISABLED: {supply.name}",
                supply);

            continue;
        }


        // =========================================================
        // ELECTRICAL ENABLED
        // =========================================================

        if (!supply.ElectricalEnabled)
        {
            Debug.LogWarning(
                $"[LEVEL 1] SOURCE '{source.SourceName}' " +
                $"ELECTRICAL ENABLED = FALSE | " +
                $"Supply = {supply.name}",
                supply);

            continue;
        }


        // =========================================================
        // OUTPUT ACTIVE
        // =========================================================

        if (!supply.IsOutputActive)
        {
            Debug.LogWarning(
                $"[LEVEL 1] SOURCE '{source.SourceName}' " +
                $"OUTPUT ACTIVE = FALSE | " +
                $"Supply = {supply.name}",
                supply);

            continue;
        }


        // =========================================================
        // VALID
        // =========================================================

        validSources.Add(source);

        Debug.Log(
            $"[LEVEL 1] VALID POWER SOURCE\n" +
            $"Source   : {source.SourceName}\n" +
            $"Supply   : {supply.name}\n" +
            $"Positive : {positive.name}\n" +
            $"Negative : {negative.name}\n" +
            $"Voltage  : {source.NominalVoltage:F2} V",
            this);
    }


    // =========================================================
    // FINAL SOURCE COUNT
    // =========================================================

    Debug.Log(
        $"[LEVEL 1] VALID POWER SOURCES = " +
        $"{validSources.Count} / {powerSources.Length}",
        this);
}

private SparkPowerSupply ResolvePowerSupply(
    SparkTerminal terminal)
{
    if (terminal == null)
        return null;


    // =========================================================
    // 1. DIRECT OWNER
    // =========================================================

    SparkPowerSupply supply =
        terminal.Owner as SparkPowerSupply;

    if (supply != null)
        return supply;


    // =========================================================
    // 2. OWNER HIERARCHY
    // =========================================================

    SparkElectricalComponent electricalOwner =
        terminal.Owner as SparkElectricalComponent;

    if (electricalOwner != null)
    {
        supply =
            electricalOwner.GetComponentInParent<SparkPowerSupply>();

        if (supply != null)
            return supply;
    }


    // =========================================================
    // 3. TERMINAL HIERARCHY
    // =========================================================

    supply =
        terminal.GetComponentInParent<SparkPowerSupply>();

    if (supply != null)
        return supply;


    // =========================================================
    // 4. CHILD/HIERARCHY SEARCH FROM TERMINAL ROOT
    // =========================================================

    Transform root =
        terminal.transform.root;

    if (root != null)
    {
        supply =
            root.GetComponentInChildren<SparkPowerSupply>(
                true);

        if (supply != null)
            return supply;
    }


    return null;
}


private static string GetOwnerName(
    SparkTerminal terminal)
{
    if (terminal == null)
        return "NULL";

    if (terminal.Owner == null)
        return "NULL";

    return terminal.Owner.name;
}
        // =========================================================
        // GRAPH TRAVERSAL
        // =========================================================

        private void BuildPositiveReachability()
        {
            positiveReachable.Clear();

            int sourceCount =
                allowAnyConfiguredPowerSource
                    ? validSources.Count
                    : Mathf.Min(1, validSources.Count);

            for (int i = 0;
                 i < sourceCount;
                 i++)
            {
                PowerSourceDefinition source =
                    validSources[i];

                if (source == null)
                    continue;

                TraverseConductiveNetwork(
                    source.PositiveTerminal,
                    positiveReachable);
            }
        }

        private void BuildNegativeReachability()
        {
            negativeReachable.Clear();

            int sourceCount =
                allowAnyConfiguredPowerSource
                    ? validSources.Count
                    : Mathf.Min(1, validSources.Count);

            for (int i = 0;
                 i < sourceCount;
                 i++)
            {
                PowerSourceDefinition source =
                    validSources[i];

                if (source == null)
                    continue;

                TraverseConductiveNetwork(
                    source.NegativeTerminal,
                    negativeReachable);
            }
        }

        private void TraverseConductiveNetwork(
            SparkTerminal start,
            HashSet<SparkTerminal> visited)
        {
            if (start == null || visited == null)
                return;

            traversalQueue.Clear();
            visitedTerminals.Clear();

            traversalQueue.Enqueue(start);
            visited.Add(start);
            visitedTerminals.Add(start);

            while (traversalQueue.Count > 0)
            {
                SparkTerminal current =
                    traversalQueue.Dequeue();

                if (current == null)
                    continue;

                circuitSystem.GetConnections(
                    current,
                    connectionBuffer);

                for (int i = 0;
                     i < connectionBuffer.Count;
                     i++)
                {
                    SparkCircuitConnection connection =
                        connectionBuffer[i];

                    if (!CanConduct(connection))
                        continue;

                    SparkTerminal next =
                        GetOtherTerminal(
                            connection,
                            current);

                    if (next == null || visited.Contains(next))
                        continue;

                    visited.Add(next);
                    visitedTerminals.Add(next);
                    traversalQueue.Enqueue(next);
                }
            }
        }

        private bool IsReachableFromSource(
            SparkTerminal sourceTerminal,
            SparkTerminal targetTerminal)
        {
            if (sourceTerminal == null || targetTerminal == null)
                return false;

            if (sourceTerminal == targetTerminal)
                return true;

            if (!allowIntermediateConnections)
            {
                return HasDirectConductiveConnection(
                    sourceTerminal,
                    targetTerminal);
            }

            traversalQueue.Clear();
            visitedTerminals.Clear();

            traversalQueue.Enqueue(sourceTerminal);
            visitedTerminals.Add(sourceTerminal);

            while (traversalQueue.Count > 0)
            {
                SparkTerminal current =
                    traversalQueue.Dequeue();

                circuitSystem.GetConnections(
                    current,
                    connectionBuffer);

                for (int i = 0;
                     i < connectionBuffer.Count;
                     i++)
                {
                    SparkCircuitConnection connection =
                        connectionBuffer[i];

                    if (!CanConduct(connection))
                        continue;

                    SparkTerminal next =
                        GetOtherTerminal(
                            connection,
                            current);

                    if (next == null)
                        continue;

                    if (next == targetTerminal)
                        return true;

                    if (visitedTerminals.Contains(next))
                        continue;

                    visitedTerminals.Add(next);
                    traversalQueue.Enqueue(next);
                }
            }

            return false;
        }

        private bool HasDirectConductiveConnection(
            SparkTerminal a,
            SparkTerminal b)
        {
            circuitSystem.GetConnections(
                a,
                connectionBuffer);

            for (int i = 0;
                 i < connectionBuffer.Count;
                 i++)
            {
                SparkCircuitConnection connection =
                    connectionBuffer[i];

                if (!CanConduct(connection))
                    continue;

                if (connection.Connects(a, b))
                    return true;
            }

            return false;
        }

        // =========================================================
        // CONDUCTIVITY
        // =========================================================

        private bool CanConduct(
            SparkCircuitConnection connection)
        {
            if (!connection.IsValid)
                return false;

            // The electrical solver explicitly treats Probe as a
            // measurement relationship, not an electrical connection.
            return connection.Kind != SparkConnectionKind.Probe;
        }

        // =========================================================
        // TARGET POWER
        // =========================================================

        private bool IsAnyAcceptedSourceConnectedTo(
            SparkTerminal terminal)
        {
            if (terminal == null)
                return false;

            int sourceCount =
                allowAnyConfiguredPowerSource
                    ? validSources.Count
                    : Mathf.Min(1, validSources.Count);

            for (int i = 0;
                 i < sourceCount;
                 i++)
            {
                PowerSourceDefinition source =
                    validSources[i];

                if (source == null)
                    continue;

                if (IsReachableFromSource(
                        source.PositiveTerminal,
                        terminal))
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsTargetShorted()
        {
            if (!rejectTargetShort)
                return false;

            if (targetPositiveTerminal == null ||
                targetNegativeTerminal == null)
            {
                return false;
            }

            return positiveReachable.Contains(
                       targetNegativeTerminal) ||
                   negativeReachable.Contains(
                       targetPositiveTerminal);
        }

        private bool IsAnyPowerSourceShorted()
        {
            if (!rejectShortCircuit)
                return false;

            int sourceCount =
                allowAnyConfiguredPowerSource
                    ? validSources.Count
                    : Mathf.Min(1, validSources.Count);

            for (int i = 0;
                 i < sourceCount;
                 i++)
            {
                PowerSourceDefinition source =
                    validSources[i];

                if (source == null)
                    continue;

                if (IsSourceShorted(source))
                    return true;
            }

            return false;
        }

        private bool IsSourceShorted(
            PowerSourceDefinition source)
        {
            if (source == null)
                return false;

            bool topologyShort =
                IsReachableFromSource(
                    source.PositiveTerminal,
                    source.NegativeTerminal) ||
                IsReachableFromSource(
                    source.NegativeTerminal,
                    source.PositiveTerminal);

            if (topologyShort)
                return true;

            SparkPowerSupply supply =
                source.PositiveTerminal != null
                    ? source.PositiveTerminal.Owner as SparkPowerSupply
                    : null;

            if (supply == null)
                return false;

            if (!supply.ElectricalEnabled ||
                !supply.IsOutputActive)
            {
                return false;
            }

            SparkElectricalComponent electrical =
                supply as SparkElectricalComponent;

            if (electrical == null)
                return false;

            float current =
                Mathf.Abs(
                    electrical.ElectricalState.Current);

            float limit =
                Mathf.Max(
                    0.0001f,
                    supply.CurrentLimit);

            // SparkElectricalSolver marks the source current-limited at
            // approximately the same threshold. This catches shorts that
            // pass through a closed switching device instead of a direct wire.
            return current >=
                   limit * 0.999f;
        }

        // =========================================================
        // TARGET ELECTRICAL STATE
        // =========================================================

        private bool TryGetTargetElectricalState(
            out float voltage,
            out float current,
            out bool conducting)
        {
            voltage = 0f;
            current = 0f;
            conducting = false;

            if (targetPositiveTerminal == null ||
                targetNegativeTerminal == null)
            {
                return false;
            }

            SparkElectricalComponent electrical =
                targetPositiveTerminal.Owner as SparkElectricalComponent;

            if (electrical == null)
                return false;

            SparkTerminal[] terminals =
                electrical.GetComponentsInChildren<SparkTerminal>(true);

            if (terminals == null || terminals.Length < 2)
                return false;

            int orientation = 0;

            // This intentionally mirrors SparkElectricalSolver.TryGetTwoTerminals:
            // the first two child terminals define the component voltage/current
            // orientation used by the solver.
            if (terminals[0] == targetPositiveTerminal &&
                terminals[1] == targetNegativeTerminal)
            {
                orientation = 1;
            }
            else if (terminals[0] == targetNegativeTerminal &&
                     terminals[1] == targetPositiveTerminal)
            {
                orientation = -1;
            }

            if (orientation == 0)
                return false;

            voltage =
                electrical.ElectricalState.Voltage *
                orientation;

            current =
                electrical.ElectricalState.Current *
                orientation;

            conducting =
                electrical.ElectricalState.Conduction ==
                SparkConductionState.Conducting;

            return true;
        }

        // =========================================================
        // WIN
        // =========================================================

        private void CheckWin()
        {
            if (gameWon)
                return;

            if (!targetPowered)
                return;

            gameWon = true;

            Debug.Log(
                "[LEVEL 1] TARGET POWERED — LEVEL COMPLETE",
                this);
        }

        

        // =========================================================
        // OUTPUT
        // =========================================================

        private void SetOutputState(
            bool powered)
        {
            if (lightObject != null)
            {
                lightObject.SetActive(powered);
            }

            if (poweredIndicator != null)
            {
                poweredIndicator.SetActive(powered);
            }

            if (faultIndicator != null)
            {
                faultIndicator.SetActive(
                    targetShorted ||
                    sourceShorted);
            }
        }

        // =========================================================
        // RESET
        // =========================================================

        public void ResetLevel()
        {
            gameWon = false;

            ResetAnalysisState();

            SetOutputState(false);

            RecalculateCircuit();

            Debug.Log(
                "[LEVEL 1] RESET",
                this);
        }

        public void Recalculate()
        {
            RecalculateCircuit();
        }

        // =========================================================
        // DEBUG STATUS
        // =========================================================

        public void DebugStatus()
        {
            Debug.Log(
                "\n" +
                "========================================\n" +
                "[PROJECT SPARK LEVEL 1]\n" +
                "========================================\n" +
                $"Target +        : {GetName(targetPositiveTerminal)}\n" +
                $"Target -        : {GetName(targetNegativeTerminal)}\n" +
                $"Positive Power  : {positivePowered}\n" +
                $"Negative Return : {negativeReturned}\n" +
                $"Target Short    : {targetShorted}\n" +
                $"Source Short    : {sourceShorted}\n" +
                $"Powered         : {targetPowered}\n" +
                $"Voltage         : {detectedVoltage:F3} V\n" +
                $"Power Source    : {activePowerSource}\n" +
                $"Game Won        : {gameWon}\n" +
                $"Connections     : {GetConnectionCount()}\n" +
                $"Topology        : {GetTopologyVersion()}\n" +
                "========================================",
                this);
        }

        // =========================================================
        // DEBUG GRAPH
        // =========================================================

        public void DebugPowerGraph()
        {
            if (circuitSystem == null)
            {
                Debug.LogError(
                    "[LEVEL 1] Circuit system is missing.",
                    this);

                return;
            }

            Debug.Log(
                $"[LEVEL 1] Positive reachable terminals: " +
                $"{positiveReachable.Count}",
                this);

            foreach (SparkTerminal terminal in positiveReachable)
            {
                if (terminal == null)
                    continue;

                Debug.Log(
                    $"[POWER +] {terminal.name}",
                    terminal);
            }

            Debug.Log(
                $"[LEVEL 1] Negative reachable terminals: " +
                $"{negativeReachable.Count}",
                this);

            foreach (SparkTerminal terminal in negativeReachable)
            {
                if (terminal == null)
                    continue;

                Debug.Log(
                    $"[POWER -] {terminal.name}",
                    terminal);
            }
        }

        // =========================================================
        // DEBUG CONNECTIONS
        // =========================================================

        public void DebugConnections()
        {
            if (circuitSystem == null)
            {
                Debug.LogError(
                    "[LEVEL 1] Circuit system is missing.",
                    this);

                return;
            }

            Debug.Log(
                $"[LEVEL 1] Total connections: " +
                $"{circuitSystem.ConnectionCount}",
                this);

            DebugTerminal(targetPositiveTerminal);
            DebugTerminal(targetNegativeTerminal);

            for (int i = 0;
                 i < validSources.Count;
                 i++)
            {
                PowerSourceDefinition source =
                    validSources[i];

                if (source == null)
                    continue;

                DebugTerminal(source.PositiveTerminal);
                DebugTerminal(source.NegativeTerminal);
            }
        }

        private void DebugTerminal(
            SparkTerminal terminal)
        {
            if (terminal == null)
                return;

            circuitSystem.GetConnections(
                terminal,
                connectionBuffer);

            Debug.Log(
                $"[LEVEL 1] Terminal {terminal.name}: " +
                $"{connectionBuffer.Count} connection(s).",
                terminal);

            for (int i = 0;
                 i < connectionBuffer.Count;
                 i++)
            {
                SparkCircuitConnection connection =
                    connectionBuffer[i];

                Debug.Log(
                    $"    {connection.Id}: " +
                    $"{GetName(connection.A)} ↔ " +
                    $"{GetName(connection.B)} | " +
                    $"Kind={connection.Kind} | " +
                    $"Direction={connection.Direction}",
                    terminal);
            }
        }

        // =========================================================
        // HELPERS
        // =========================================================

        private void ResolveDependencies()
        {
            if (circuitSystem == null)
            {
                circuitSystem =
                    GetComponentInParent<SparkCircuitSystem>();
            }

            if (electricalSolver == null)
            {
                electricalSolver =
                    GetComponentInParent<SparkElectricalSolver>();
            }
        }

        private static SparkTerminal GetOtherTerminal(
            SparkCircuitConnection connection,
            SparkTerminal current)
        {
            if (current == null)
                return null;

            if (connection.A == current)
                return connection.B;

            if (connection.B == current)
                return connection.A;

            return null;
        }

        private int GetConnectionCount()
        {
            if (circuitSystem == null)
                return 0;

            return circuitSystem.ConnectionCount;
        }

        private int GetTopologyVersion()
        {
            if (circuitSystem == null)
                return 0;

            return circuitSystem.TopologyVersion;
        }

        private static string GetName(
            SparkTerminal terminal)
        {
            return terminal != null
                ? terminal.name
                : "NULL";
        }
    }
}
