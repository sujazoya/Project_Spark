using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectSpark.AI
{
    /// <summary>
    /// Deterministic AI-side reasoning layer for circuit topology.
    ///
    /// Converts structural circuit analysis into human-readable
    /// reasoning facts that higher-level AI systems can use.
    ///
    /// IMPORTANT:
    /// This component is observational only.
    ///
    /// It does NOT:
    /// - modify the circuit
    /// - modify SparkCircuitSystem
    /// - modify SparkElectricalSolver
    /// - modify level state
    /// - declare level completion
    /// - replace SparkLevelEvaluator
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkAICircuitReasoner : MonoBehaviour
    {
        [Header("Spark AI")]
        [SerializeField]
        private SparkAIWorld world;

        [SerializeField]
        private SparkAICircuitAnalyzer circuitAnalyzer;

        [SerializeField]
        private SparkAICircuitPathAnalyzer pathAnalyzer;

        private readonly List<string> observations =
            new List<string>(16);

        private readonly List<string> problems =
            new List<string>(8);

        private readonly List<string> suggestions =
            new List<string>(8);

        private bool initialized;

        public bool IsInitialized =>
            initialized;

        private void Awake()
        {
            if (world == null)
            {
                Debug.LogError(
                    "[Spark AI Circuit Reasoner] " +
                    "SparkAIWorld reference is missing.",
                    this);

                return;
            }

            if (circuitAnalyzer == null)
            {
                Debug.LogError(
                    "[Spark AI Circuit Reasoner] " +
                    "SparkAICircuitAnalyzer reference is missing.",
                    this);

                return;
            }

            if (pathAnalyzer == null)
            {
                Debug.LogError(
                    "[Spark AI Circuit Reasoner] " +
                    "SparkAICircuitPathAnalyzer reference is missing.",
                    this);

                return;
            }

            initialized = true;
        }

        /// <summary>
        /// Reasons about the latest observed circuit.
        /// </summary>
        public SparkAICircuitReasoning ReasonCurrentCircuit()
        {
            if (!initialized)
                return SparkAICircuitReasoning.Unavailable();

            SparkAIWorldSnapshot snapshot =
                world.LatestSnapshot;

            return Reason(snapshot);
        }

        /// <summary>
        /// Reasons about a supplied world snapshot.
        /// </summary>
        public SparkAICircuitReasoning Reason(
            SparkAIWorldSnapshot snapshot)
        {
            if (!initialized)
                return SparkAICircuitReasoning.Unavailable();

            if (snapshot == null)
                return SparkAICircuitReasoning.Unavailable();

            observations.Clear();
            problems.Clear();
            suggestions.Clear();

            SparkAICircuitAnalysis circuit =
                circuitAnalyzer.Analyze(snapshot);

            if (!circuit.IsValid)
            {
                observations.Add(
                    "Circuit topology analysis is unavailable.");

                return BuildResult(
                    snapshot,
                    circuit);
            }

            AnalyzeTopology(
                snapshot,
                circuit);

            AnalyzeSource(
                snapshot,
                circuit);

            AnalyzeConnectivity(
                snapshot,
                circuit);

            AnalyzePlayerContext(
                snapshot);

            BuildSuggestions(
                circuit);

            return BuildResult(
                snapshot,
                circuit);
        }

        private void AnalyzeTopology(
            SparkAIWorldSnapshot snapshot,
            SparkAICircuitAnalysis circuit)
        {
            switch (circuit.TopologyState)
            {
                case SparkAICircuitTopologyState.NoCircuit:

                    observations.Add(
                        "No circuit terminals are currently available.");

                    problems.Add(
                        "There is no observable circuit.");

                    break;

                case SparkAICircuitTopologyState.NoConnections:

                    observations.Add(
                        "Circuit terminals exist, but no connections are present.");

                    problems.Add(
                        "The circuit has not been connected yet.");

                    break;

                case SparkAICircuitTopologyState.NoSource:

                    observations.Add(
                        "No powered positive and negative source pair was identified.");

                    problems.Add(
                        "A usable power source has not been identified.");

                    break;

                case SparkAICircuitTopologyState.OpenCircuit:

                    observations.Add(
                        "A source was identified, but the positive and negative source terminals are not connected through the observed topology.");

                    problems.Add(
                        "The circuit appears to have an open path.");

                    break;

                case SparkAICircuitTopologyState.ClosedCircuit:

                    observations.Add(
                        "A continuous topology exists between the identified positive and negative source terminals.");

                    break;

                case SparkAICircuitTopologyState.ShortCircuit:

                    observations.Add(
                        "The positive and negative source terminals are directly connected.");

                    problems.Add(
                        "The source appears to be shorted.");

                    break;

                default:

                    observations.Add(
                        "The circuit topology could not be classified.");
                    break;
            }

            if (circuit.ConnectedComponentCount > 1)
            {
                observations.Add(
                    $"The circuit contains {circuit.ConnectedComponentCount} disconnected topology groups.");
            }
        }

        private void AnalyzeSource(
            SparkAIWorldSnapshot snapshot,
            SparkAICircuitAnalysis circuit)
        {
            if (circuit.HasPositiveSource)
            {
                string positiveName =
                    GetTerminalName(
                        snapshot,
                        circuit.PositiveSourceTerminalId);

                if (!string.IsNullOrEmpty(positiveName))
                {
                    observations.Add(
                        $"Positive source terminal identified as {positiveName}.");
                }
            }
            else
            {
                observations.Add(
                    "No powered positive source terminal was identified.");
            }

            if (circuit.HasNegativeSource)
            {
                string negativeName =
                    GetTerminalName(
                        snapshot,
                        circuit.NegativeSourceTerminalId);

                if (!string.IsNullOrEmpty(negativeName))
                {
                    observations.Add(
                        $"Negative source terminal identified as {negativeName}.");
                }
            }
            else
            {
                observations.Add(
                    "No powered negative source terminal was identified.");
            }
        }

        private void AnalyzeConnectivity(
            SparkAIWorldSnapshot snapshot,
            SparkAICircuitAnalysis circuit)
        {
            if (!circuit.HasPositiveSource)
                return;

            int positiveId =
                circuit.PositiveSourceTerminalId;

            int negativeId =
                circuit.NegativeSourceTerminalId;

            if (negativeId == 0)
                return;

            SparkAICircuitPathAnalysis path =
                pathAnalyzer.FindPath(
                    snapshot,
                    positiveId,
                    negativeId);

            if (!path.IsValid)
            {
                problems.Add(
                    "The source path could not be analyzed.");

                return;
            }

            if (!path.PathExists)
            {
                problems.Add(
                    "No topology path was found between the positive and negative source terminals.");

                suggestions.Add(
                    "Look for a missing connection in the return path.");

                return;
            }

            observations.Add(
                $"The source path contains {path.TerminalCount} terminals and {path.ConnectionCount} connections.");

            if (path.ConnectionCount == 1)
            {
                observations.Add(
                    "The positive and negative source terminals are directly connected.");
            }
        }

        private void AnalyzePlayerContext(
            SparkAIWorldSnapshot snapshot)
        {
            SparkAIPlayerSnapshot player =
                snapshot.Player;

            if (!player.HasSelection)
                return;

            if (!string.IsNullOrEmpty(
                    player.SelectedObjectName))
            {
                observations.Add(
                    $"Player is currently inspecting {player.SelectedObjectName}.");
            }

            SparkAIPlayerActivitySnapshot activity =
                player.LatestActivity;

            if (activity.Type !=
                SparkAIPlayerActivityType.None)
            {
                if (!string.IsNullOrEmpty(
                        activity.Description))
                {
                    observations.Add(
                        activity.Description);
                }
            }
        }

        private void BuildSuggestions(
            SparkAICircuitAnalysis circuit)
        {
            switch (circuit.TopologyState)
            {
                case SparkAICircuitTopologyState.NoCircuit:

                    suggestions.Add(
                        "Place or inspect the required circuit components.");

                    break;

                case SparkAICircuitTopologyState.NoConnections:

                    suggestions.Add(
                        "Connect the circuit terminals step by step.");

                    break;

                case SparkAICircuitTopologyState.NoSource:

                    suggestions.Add(
                        "Identify a valid power source before troubleshooting the load.");

                    break;

                case SparkAICircuitTopologyState.OpenCircuit:

                    suggestions.Add(
                        "Trace the circuit from the source positive terminal and find where the return path stops.");

                    break;

                case SparkAICircuitTopologyState.ShortCircuit:

                    suggestions.Add(
                        "Separate the source positive and negative terminals and route the circuit through the intended load.");

                    break;

                case SparkAICircuitTopologyState.ClosedCircuit:

                    suggestions.Add(
                        "The source has a continuous topology. Inspect the intended load and polarity next.");

                    break;
            }
        }

        private string GetTerminalName(
            SparkAIWorldSnapshot snapshot,
            int terminalId)
        {
            if (terminalId == 0)
                return string.Empty;

            IReadOnlyList<SparkAITerminalSnapshot> terminals =
                snapshot.Terminals;

            if (terminals == null)
                return string.Empty;

            for (int i = 0;
                 i < terminals.Count;
                 i++)
            {
                SparkAITerminalSnapshot terminal =
                    terminals[i];

                if (terminal.InstanceId != terminalId)
                    continue;

                if (!string.IsNullOrEmpty(
                        terminal.Name))
                {
                    return terminal.Name;
                }

                return terminal.OwnerName;
            }

            return string.Empty;
        }

        private SparkAICircuitReasoning BuildResult(
            SparkAIWorldSnapshot snapshot,
            SparkAICircuitAnalysis circuit)
        {
            string summary =
                BuildSummary(
                    snapshot,
                    circuit);

            return new SparkAICircuitReasoning(
                true,
                Time.time,
                circuit.TopologyState,
                circuit.HasSource,
                circuit.ReturnPathClosed,
                circuit.SourceShorted,
                circuit.ReachableTerminalCount,
                circuit.ConnectedComponentCount,
                summary,
                observations.ToArray(),
                problems.ToArray(),
                suggestions.ToArray());
        }

        private string BuildSummary(
            SparkAIWorldSnapshot snapshot,
            SparkAICircuitAnalysis circuit)
        {
            switch (circuit.TopologyState)
            {
                case SparkAICircuitTopologyState.NoCircuit:
                    return "There is currently no observable circuit.";

                case SparkAICircuitTopologyState.NoConnections:
                    return "Circuit components are present, but they are not connected.";

                case SparkAICircuitTopologyState.NoSource:
                    return "The circuit does not currently contain an identifiable powered source.";

                case SparkAICircuitTopologyState.OpenCircuit:
                    return "A power source is present, but the circuit does not have a complete return path.";

                case SparkAICircuitTopologyState.ShortCircuit:
                    return "The power source appears to be directly shorted.";

                case SparkAICircuitTopologyState.ClosedCircuit:
                    return "The circuit has a continuous topology between the identified source terminals.";

                default:
                    return "The circuit topology is currently unknown.";
            }
        }
    }

    /// <summary>
    /// Immutable deterministic reasoning result for one circuit observation.
    /// </summary>
    public readonly struct SparkAICircuitReasoning
    {
        public bool IsValid { get; }

        public float CapturedAt { get; }

        public SparkAICircuitTopologyState TopologyState { get; }

        public bool HasSource { get; }

        public bool ReturnPathClosed { get; }

        public bool SourceShorted { get; }

        public int ReachableTerminalCount { get; }

        public int ConnectedComponentCount { get; }

        public string Summary { get; }

        public string[] Observations { get; }

        public string[] Problems { get; }

        public string[] Suggestions { get; }

        public SparkAICircuitReasoning(
            bool isValid,
            float capturedAt,
            SparkAICircuitTopologyState topologyState,
            bool hasSource,
            bool returnPathClosed,
            bool sourceShorted,
            int reachableTerminalCount,
            int connectedComponentCount,
            string summary,
            string[] observations,
            string[] problems,
            string[] suggestions)
        {
            IsValid = isValid;
            CapturedAt = capturedAt;
            TopologyState = topologyState;
            HasSource = hasSource;
            ReturnPathClosed = returnPathClosed;
            SourceShorted = sourceShorted;
            ReachableTerminalCount =
                reachableTerminalCount;
            ConnectedComponentCount =
                connectedComponentCount;

            Summary =
                summary ?? string.Empty;

            Observations =
                observations ??
                Array.Empty<string>();

            Problems =
                problems ??
                Array.Empty<string>();

            Suggestions =
                suggestions ??
                Array.Empty<string>();
        }

        public static SparkAICircuitReasoning Unavailable()
        {
            return new SparkAICircuitReasoning(
                false,
                Time.time,
                SparkAICircuitTopologyState.Unknown,
                false,
                false,
                false,
                0,
                0,
                "Circuit reasoning is unavailable.",
                Array.Empty<string>(),
                Array.Empty<string>(),
                Array.Empty<string>());
        }
    }
}