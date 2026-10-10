using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectSpark.AI
{
    /// <summary>
    /// Coordinates the deterministic AI observation layer.
    ///
    /// Takes ONE authoritative SparkAIWorldSnapshot and combines:
    /// - Circuit topology reasoning
    /// - Exact circuit path reasoning
    /// - Component reasoning
    /// - Electrical behavior reasoning
    ///
    /// This class does not solve circuits and does not modify
    /// any Project Spark gameplay/electrical system.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkAIObservationCoordinator : MonoBehaviour
    {
        [Header("AI World")]
        [SerializeField]
        private SparkAIWorld world;

        [Header("Analyzers")]
        [SerializeField]
        private SparkAICircuitAnalyzer circuitAnalyzer;

        [SerializeField]
        private SparkAICircuitPathAnalyzer pathAnalyzer;

        [SerializeField]
        private SparkAICircuitReasoner circuitReasoner;

        [SerializeField]
        private SparkAIComponentReasoner componentReasoner;

        [SerializeField]
        private SparkAIElectricalReasoner electricalReasoner;

        private SparkAIUnifiedObservation currentObservation;

        private bool initialized;

        public bool IsInitialized =>
            initialized;

        public SparkAIUnifiedObservation CurrentObservation =>
            currentObservation;

        private void Awake()
        {
            if (world == null)
            {
                Debug.LogError(
                    "[Spark AI Observation Coordinator] " +
                    "SparkAIWorld reference is missing.",
                    this);

                return;
            }

            if (circuitAnalyzer == null)
            {
                Debug.LogError(
                    "[Spark AI Observation Coordinator] " +
                    "SparkAICircuitAnalyzer reference is missing.",
                    this);

                return;
            }

            if (pathAnalyzer == null)
            {
                Debug.LogError(
                    "[Spark AI Observation Coordinator] " +
                    "SparkAICircuitPathAnalyzer reference is missing.",
                    this);

                return;
            }

            if (circuitReasoner == null)
            {
                Debug.LogError(
                    "[Spark AI Observation Coordinator] " +
                    "SparkAICircuitReasoner reference is missing.",
                    this);

                return;
            }

            if (componentReasoner == null)
            {
                Debug.LogError(
                    "[Spark AI Observation Coordinator] " +
                    "SparkAIComponentReasoner reference is missing.",
                    this);

                return;
            }

            if (electricalReasoner == null)
            {
                Debug.LogError(
                    "[Spark AI Observation Coordinator] " +
                    "SparkAIElectricalReasoner reference is missing.",
                    this);

                return;
            }

            initialized = true;
        }

        /// <summary>
        /// Builds one unified observation from the latest
        /// authoritative world snapshot.
        /// </summary>
       public SparkAIUnifiedObservation ObserveCurrentWorld()
{
    if (!initialized || world == null)
    {
        currentObservation =
            SparkAIUnifiedObservation.Invalid();

        return currentObservation;
    }

    // Capture a fresh snapshot of the authoritative world.
    // This does not run or modify the electrical solver.
    SparkAIWorldSnapshot snapshot =
        world.CaptureSnapshot();

    if (snapshot == null)
    {
        currentObservation =
            SparkAIUnifiedObservation.Invalid();

        return currentObservation;
    }

    return Observe(snapshot);
}

        /// <summary>
        /// Builds one unified observation from the supplied snapshot.
        ///
        /// All analyzers receive the same snapshot so the resulting
        /// reasoning belongs to the same moment in the simulation.
        /// </summary>
        public SparkAIUnifiedObservation Observe(
            SparkAIWorldSnapshot snapshot)
        {
            if (!initialized ||
                snapshot == null)
            {
                currentObservation =
                    SparkAIUnifiedObservation.Invalid();

                return currentObservation;
            }

            SparkAICircuitAnalysis circuitAnalysis =
                circuitAnalyzer.Analyze(snapshot);

            SparkAICircuitReasoning circuitReasoning =
                circuitReasoner.Reason(snapshot);

            IReadOnlyList<SparkAIElectronicObjectSnapshot>
                electronicObjects =
                    snapshot.ElectronicObjects;

            List<SparkAIComponentReasoning>
                componentReasonings =
                    new List<SparkAIComponentReasoning>(
                        electronicObjects != null
                            ? electronicObjects.Count
                            : 0);

            if (electronicObjects != null)
            {
                for (int i = 0;
                     i < electronicObjects.Count;
                     i++)
                {
                    SparkAIElectronicObjectSnapshot component =
                        electronicObjects[i];

                    if (!component.IsElectricalComponent)
                        continue;

                    if (componentReasoner.TryAnalyzeComponent(
                            snapshot,
                            component.InstanceId,
                            out SparkAIComponentReasoning reasoning))
                    {
                        componentReasonings.Add(reasoning);
                    }
                }
            }

            IReadOnlyList<SparkAIElectricalReasoning>
                electricalReasonings =
                    electricalReasoner.Analyze(snapshot);

            SparkAIPathObservation pathObservation =
                BuildImportantPathObservation(
                    snapshot,
                    circuitAnalysis);

            string summary =
                BuildUnifiedSummary(
                    snapshot,
                    circuitReasoning,
                    componentReasonings,
                    electricalReasonings);

            string[] observations =
                BuildUnifiedObservations(
                    snapshot,
                    circuitReasoning,
                    componentReasonings,
                    electricalReasonings,
                    pathObservation);

            string[] problems =
                BuildUnifiedProblems(
                    circuitReasoning,
                    electricalReasonings,
                    pathObservation);

            string[] suggestions =
                BuildUnifiedSuggestions(
                    circuitReasoning,
                    electricalReasonings,
                    pathObservation);

            currentObservation =
                new SparkAIUnifiedObservation(
                    true,
                    snapshot.CapturedAt,
                    snapshot,
                    circuitAnalysis,
                    circuitReasoning,
                    pathObservation,
                    componentReasonings,
                    electricalReasonings,
                    summary,
                    observations,
                    problems,
                    suggestions);

            return currentObservation;
        }

        /// <summary>
        /// Finds the most important source-to-source path currently
        /// available in the circuit.
        ///
        /// This is an observation only. No topology is changed.
        /// </summary>
        private SparkAIPathObservation
            BuildImportantPathObservation(
                SparkAIWorldSnapshot snapshot,
                SparkAICircuitAnalysis circuitAnalysis)
        {
            if (!circuitAnalysis.IsValid)
            {
                return SparkAIPathObservation.Invalid();
            }

            if (!circuitAnalysis.HasPositiveSource ||
                !circuitAnalysis.HasNegativeSource)
            {
                return SparkAIPathObservation.NoPath();
            }

            SparkAICircuitPathAnalysis path =
                pathAnalyzer.FindPath(
                    snapshot,
                    circuitAnalysis.PositiveSourceTerminalId,
                    circuitAnalysis.NegativeSourceTerminalId);

            if (!path.IsValid)
            {
                return SparkAIPathObservation.NoPath();
            }

            return new SparkAIPathObservation(
                true,
                path.PathExists,
                path.StartTerminalId,
                path.TargetTerminalId,
                path.TerminalPath,
                path.ConnectionPath,
                path.TerminalCount,
                path.ConnectionCount);
        }

        private string BuildUnifiedSummary(
            SparkAIWorldSnapshot snapshot,
            SparkAICircuitReasoning circuitReasoning,
            List<SparkAIComponentReasoning> componentReasonings,
            IReadOnlyList<SparkAIElectricalReasoning> electricalReasonings)
        {
            if (!circuitReasoning.IsValid)
            {
                return "The AI could not establish a valid circuit observation.";
            }

            string summary =
                circuitReasoning.Summary;

            int energizedCount = 0;
            int conductingCount = 0;
            int voltageOnlyCount = 0;

            if (electricalReasonings != null)
            {
                for (int i = 0;
                     i < electricalReasonings.Count;
                     i++)
                {
                    SparkAIElectricalReasoning reasoning =
                        electricalReasonings[i];

                    if (reasoning.HasVoltage)
                        energizedCount++;

                    if (reasoning.HasCurrent)
                        conductingCount++;

                    if (reasoning.State ==
                        SparkAIElectricalState.VoltageWithoutCurrent)
                    {
                        voltageOnlyCount++;
                    }
                }
            }

            summary +=
                $" {componentReasonings.Count} electrical components are being observed.";

            summary +=
                $" {energizedCount} show significant voltage.";

            summary +=
                $" {conductingCount} show significant current.";

            if (voltageOnlyCount > 0)
            {
                summary +=
                    $" {voltageOnlyCount} show voltage without significant current.";
            }

            if (snapshot.Player.HasSelection)
            {
                summary +=
                    $" Player is currently focused on " +
                    $"{snapshot.Player.SelectedObjectName}.";
            }

            return summary;
        }

        private string[] BuildUnifiedObservations(
            SparkAIWorldSnapshot snapshot,
            SparkAICircuitReasoning circuitReasoning,
            List<SparkAIComponentReasoning> componentReasonings,
            IReadOnlyList<SparkAIElectricalReasoning> electricalReasonings,
            SparkAIPathObservation pathObservation)
        {
            List<string> observations =
                new List<string>(16);

            if (!string.IsNullOrEmpty(
                    circuitReasoning.Summary))
            {
                observations.Add(
                    circuitReasoning.Summary);
            }

            if (pathObservation.IsValid)
            {
                if (pathObservation.PathExists)
                {
                    observations.Add(
                        $"A connected path exists between the identified positive and negative source terminals. " +
                        $"The observed path contains {pathObservation.TerminalCount} terminals and " +
                        $"{pathObservation.ConnectionCount} connections.");
                }
                else
                {
                    observations.Add(
                        "No complete source-to-source path was found between the identified positive and negative terminals.");
                }
            }

            if (electricalReasonings != null)
            {
                for (int i = 0;
                     i < electricalReasonings.Count;
                     i++)
                {
                    SparkAIElectricalReasoning reasoning =
                        electricalReasonings[i];

                    if (!reasoning.IsValid)
                        continue;

                    if (reasoning.HasCurrent ||
                        reasoning.HasVoltage)
                    {
                        observations.Add(
                            reasoning.Summary);
                    }
                }
            }

            if (componentReasonings != null)
            {
                for (int i = 0;
                     i < componentReasonings.Count;
                     i++)
                {
                    SparkAIComponentReasoning reasoning =
                        componentReasonings[i];

                    if (!reasoning.IsValid)
                        continue;

                    if (reasoning.State !=
                        SparkAIComponentState.Unknown)
                    {
                        observations.Add(
                            reasoning.Summary);
                    }
                }
            }

            if (snapshot.Player.HasSelection)
            {
                observations.Add(
                    $"Player selected {snapshot.Player.SelectedObjectName}.");
            }

            return observations.ToArray();
        }

        private string[] BuildUnifiedProblems(
            SparkAICircuitReasoning circuitReasoning,
            IReadOnlyList<SparkAIElectricalReasoning> electricalReasonings,
            SparkAIPathObservation pathObservation)
        {
            List<string> problems =
                new List<string>(8);

            if (circuitReasoning.Problems != null)
            {
                for (int i = 0;
                     i < circuitReasoning.Problems.Length;
                     i++)
                {
                    string problem =
                        circuitReasoning.Problems[i];

                    if (!string.IsNullOrEmpty(problem))
                    {
                        problems.Add(problem);
                    }
                }
            }

            if (circuitReasoning.TopologyState ==
                SparkAICircuitTopologyState.OpenCircuit)
            {
                if (pathObservation.IsValid &&
                    !pathObservation.PathExists)
                {
                    problems.Add(
                        "The identified source terminals do not currently have a complete connected path.");
                }
            }

            if (electricalReasonings != null)
            {
                for (int i = 0;
                     i < electricalReasonings.Count;
                     i++)
                {
                    SparkAIElectricalReasoning reasoning =
                        electricalReasonings[i];

                    if (!reasoning.IsValid)
                        continue;

                    if (reasoning.State ==
                        SparkAIElectricalState.VoltageWithoutCurrent)
                    {
                        problems.Add(
                            $"{reasoning.Name} has significant voltage but no significant current is observed.");
                    }
                }
            }

            return problems.ToArray();
        }

        private string[] BuildUnifiedSuggestions(
            SparkAICircuitReasoning circuitReasoning,
            IReadOnlyList<SparkAIElectricalReasoning> electricalReasonings,
            SparkAIPathObservation pathObservation)
        {
            List<string> suggestions =
                new List<string>(8);

            if (circuitReasoning.Suggestions != null)
            {
                for (int i = 0;
                     i < circuitReasoning.Suggestions.Length;
                     i++)
                {
                    string suggestion =
                        circuitReasoning.Suggestions[i];

                    if (!string.IsNullOrEmpty(suggestion))
                    {
                        suggestions.Add(suggestion);
                    }
                }
            }

            if (pathObservation.IsValid &&
                !pathObservation.PathExists)
            {
                suggestions.Add(
                    "Trace the circuit from the positive source terminal toward the negative return path.");
            }

            if (electricalReasonings != null)
            {
                for (int i = 0;
                     i < electricalReasonings.Count;
                     i++)
                {
                    SparkAIElectricalReasoning reasoning =
                        electricalReasonings[i];

                    if (!reasoning.IsValid)
                        continue;

                    if (reasoning.Suggestions == null)
                        continue;

                    for (int j = 0;
                         j < reasoning.Suggestions.Length;
                         j++)
                    {
                        string suggestion =
                            reasoning.Suggestions[j];

                        if (!string.IsNullOrEmpty(suggestion))
                        {
                            suggestions.Add(suggestion);
                        }
                    }
                }
            }

            return suggestions.ToArray();
        }
    }

    /// <summary>
    /// Unified deterministic observation of the current Project Spark
    /// world.
    /// </summary>
    public readonly struct SparkAIUnifiedObservation
    {
        public bool IsValid { get; }

        public float CapturedAt { get; }

        public SparkAIWorldSnapshot World { get; }

        public SparkAICircuitAnalysis CircuitAnalysis { get; }

        public SparkAICircuitReasoning CircuitReasoning { get; }

        public SparkAIPathObservation PathObservation { get; }

        public IReadOnlyList<SparkAIComponentReasoning>
            ComponentReasonings { get; }

        public IReadOnlyList<SparkAIElectricalReasoning>
            ElectricalReasonings { get; }

        public string Summary { get; }

        public string[] Observations { get; }

        public string[] Problems { get; }

        public string[] Suggestions { get; }

        public SparkAIUnifiedObservation(
            bool isValid,
            float capturedAt,
            SparkAIWorldSnapshot world,
            SparkAICircuitAnalysis circuitAnalysis,
            SparkAICircuitReasoning circuitReasoning,
            SparkAIPathObservation pathObservation,
            IReadOnlyList<SparkAIComponentReasoning> componentReasonings,
            IReadOnlyList<SparkAIElectricalReasoning> electricalReasonings,
            string summary,
            string[] observations,
            string[] problems,
            string[] suggestions)
        {
            IsValid = isValid;
            CapturedAt = capturedAt;
            World = world;
            CircuitAnalysis = circuitAnalysis;
            CircuitReasoning = circuitReasoning;
            PathObservation = pathObservation;
            ComponentReasonings = componentReasonings;
            ElectricalReasonings = electricalReasonings;
            Summary = summary ?? string.Empty;
            Observations = observations ?? Array.Empty<string>();
            Problems = problems ?? Array.Empty<string>();
            Suggestions = suggestions ?? Array.Empty<string>();
        }

        public static SparkAIUnifiedObservation Invalid()
        {
            return new SparkAIUnifiedObservation(
                false,
                Time.time,
                null,
                default,
                default,
                SparkAIPathObservation.Invalid(),
                Array.Empty<SparkAIComponentReasoning>(),
                Array.Empty<SparkAIElectricalReasoning>(),
                string.Empty,
                Array.Empty<string>(),
                Array.Empty<string>(),
                Array.Empty<string>());
        }
    }

    /// <summary>
    /// Important topology path selected for educational reasoning.
    /// </summary>
   /// <summary>
/// Important topology path selected for educational reasoning.
///
/// The connection path keeps the complete authoritative
/// SparkAIConnectionSnapshot objects rather than only connection IDs.
/// </summary>
public readonly struct SparkAIPathObservation
{
    public bool IsValid { get; }

    public bool PathExists { get; }

    public int StartTerminalId { get; }

    public int TargetTerminalId { get; }

    public IReadOnlyList<int> TerminalPath { get; }

    public IReadOnlyList<SparkAIConnectionSnapshot> ConnectionPath { get; }

    public int TerminalCount { get; }

    public int ConnectionCount { get; }

    public SparkAIPathObservation(
        bool isValid,
        bool pathExists,
        int startTerminalId,
        int targetTerminalId,
        IReadOnlyList<int> terminalPath,
        IReadOnlyList<SparkAIConnectionSnapshot> connectionPath,
        int terminalCount,
        int connectionCount)
    {
        IsValid = isValid;
        PathExists = pathExists;
        StartTerminalId = startTerminalId;
        TargetTerminalId = targetTerminalId;

        TerminalPath =
            terminalPath ??
            Array.Empty<int>();

        ConnectionPath =
            connectionPath ??
            Array.Empty<SparkAIConnectionSnapshot>();

        TerminalCount = terminalCount;
        ConnectionCount = connectionCount;
    }

    public static SparkAIPathObservation Invalid()
    {
        return new SparkAIPathObservation(
            false,
            false,
            0,
            0,
            Array.Empty<int>(),
            Array.Empty<SparkAIConnectionSnapshot>(),
            0,
            0);
    }

    public static SparkAIPathObservation NoPath()
    {
        return new SparkAIPathObservation(
            true,
            false,
            0,
            0,
            Array.Empty<int>(),
            Array.Empty<SparkAIConnectionSnapshot>(),
            0,
            0);
    }

    }
}