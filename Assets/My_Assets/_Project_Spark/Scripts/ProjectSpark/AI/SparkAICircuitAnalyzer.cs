using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectSpark.AI
{
    /// <summary>
    /// Deterministic structural analyzer for the Project Spark circuit.
    ///
    /// IMPORTANT:
    /// This is an observational AI-side analyzer.
    ///
    /// It does NOT:
    /// - modify the circuit
    /// - modify SparkCircuitSystem
    /// - modify SparkElectricalSolver
    /// - calculate authoritative voltage/current
    /// - replace level evaluation
    ///
    /// It analyzes the topology already captured by SparkAIWorld.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkAICircuitAnalyzer : MonoBehaviour
    {
        [Header("Spark AI")]
        [SerializeField]
        private SparkAIWorld world;

        private readonly Dictionary<int, List<int>> adjacency =
            new Dictionary<int, List<int>>(128);

        private readonly HashSet<int> visited =
            new HashSet<int>();

        private readonly Queue<int> searchQueue =
            new Queue<int>(128);

        private readonly Dictionary<int, SparkAITerminalSnapshot> terminalById =
            new Dictionary<int, SparkAITerminalSnapshot>(128);

        private bool initialized;

        public bool IsInitialized =>
            initialized;

        private void Awake()
        {
            if (world == null)
            {
                Debug.LogError(
                    "[Spark AI Circuit Analyzer] " +
                    "SparkAIWorld reference is missing.",
                    this);

                return;
            }

            initialized = true;
        }

        /// <summary>
        /// Analyzes the latest authoritative AI world snapshot.
        /// </summary>
        public SparkAICircuitAnalysis AnalyzeCurrentCircuit()
        {
            if (!initialized)
                return SparkAICircuitAnalysis.Unavailable();

            return Analyze(
                world.LatestSnapshot);
        }

        /// <summary>
        /// Analyzes a supplied world snapshot.
        /// </summary>
        public SparkAICircuitAnalysis Analyze(
            SparkAIWorldSnapshot snapshot)
        {
            if (!initialized)
                return SparkAICircuitAnalysis.Unavailable();

            if (snapshot == null)
                return SparkAICircuitAnalysis.Unavailable();

            BuildIndexes(snapshot);

            if (snapshot.Terminals == null ||
                snapshot.Terminals.Count == 0)
            {
                return SparkAICircuitAnalysis.NoCircuit();
            }

            if (snapshot.Connections == null ||
                snapshot.Connections.Count == 0)
            {
                return SparkAICircuitAnalysis.NoConnections();
            }

            int poweredPositiveTerminalId =
                FindLikelyPoweredPositiveTerminal(snapshot);

            int poweredNegativeTerminalId =
                FindLikelyPoweredNegativeTerminal(snapshot);

            bool hasPositiveSource =
                poweredPositiveTerminalId != 0;

            bool hasNegativeSource =
                poweredNegativeTerminalId != 0;

            bool hasSource =
                hasPositiveSource &&
                hasNegativeSource;

            bool returnPathClosed = false;

            if (hasSource)
            {
                returnPathClosed =
                    AreConnected(
                        poweredPositiveTerminalId,
                        poweredNegativeTerminalId);
            }

            bool sourceShorted =
                DetectSourceShort(
                    poweredPositiveTerminalId,
                    poweredNegativeTerminalId);

            int reachableFromPositive = 0;

            if (poweredPositiveTerminalId != 0)
            {
                reachableFromPositive =
                    CountReachableTerminals(
                        poweredPositiveTerminalId);
            }

            int connectedComponentCount =
                CountConnectedComponents();

            SparkAICircuitTopologyState topologyState;

            if (sourceShorted)
            {
                topologyState =
                    SparkAICircuitTopologyState.ShortCircuit;
            }
            else if (!hasSource)
            {
                topologyState =
                    SparkAICircuitTopologyState.NoSource;
            }
            else if (!returnPathClosed)
            {
                topologyState =
                    SparkAICircuitTopologyState.OpenCircuit;
            }
            else
            {
                topologyState =
                    SparkAICircuitTopologyState.ClosedCircuit;
            }

            return new SparkAICircuitAnalysis(
                true,
                topologyState,
                hasSource,
                returnPathClosed,
                sourceShorted,
                hasPositiveSource,
                hasNegativeSource,
                poweredPositiveTerminalId,
                poweredNegativeTerminalId,
                reachableFromPositive,
                connectedComponentCount,
                snapshot.Connections != null
                    ? snapshot.Connections.Count
                    : 0,
                snapshot.Terminals != null
                    ? snapshot.Terminals.Count
                    : 0);
        }

        /// <summary>
        /// Builds the AI-side topology graph from the authoritative
        /// world snapshot.
        /// </summary>
        private void BuildIndexes(
            SparkAIWorldSnapshot snapshot)
        {
            adjacency.Clear();
            terminalById.Clear();

            IReadOnlyList<SparkAITerminalSnapshot> terminals =
                snapshot.Terminals;

            if (terminals != null)
            {
                for (int i = 0;
                     i < terminals.Count;
                     i++)
                {
                    SparkAITerminalSnapshot terminal =
                        terminals[i];

                    if (terminal.InstanceId == 0)
                        continue;

                    terminalById[
                        terminal.InstanceId] =
                        terminal;

                    if (!adjacency.ContainsKey(
                            terminal.InstanceId))
                    {
                        adjacency.Add(
                            terminal.InstanceId,
                            new List<int>(4));
                    }
                }
            }

            IReadOnlyList<SparkAIConnectionSnapshot> connections =
                snapshot.Connections;

            if (connections == null)
                return;

            for (int i = 0;
                 i < connections.Count;
                 i++)
            {
                SparkAIConnectionSnapshot connection =
                    connections[i];

                if (!connection.Valid)
                    continue;

                int sourceTerminalId =
                    connection.SourceTerminalInstanceId;

                int targetTerminalId =
                    connection.TargetTerminalInstanceId;

                if (sourceTerminalId == 0 ||
                    targetTerminalId == 0)
                {
                    continue;
                }

                AddEdge(
                    sourceTerminalId,
                    targetTerminalId);

                AddEdge(
                    targetTerminalId,
                    sourceTerminalId);
            }
        }

        private void AddEdge(
            int from,
            int to)
        {
            if (!adjacency.TryGetValue(
                    from,
                    out List<int> neighbors))
            {
                neighbors =
                    new List<int>(4);

                adjacency.Add(
                    from,
                    neighbors);
            }

            if (!neighbors.Contains(to))
                neighbors.Add(to);
        }

        /// <summary>
        /// Finds a likely powered positive source terminal.
        ///
        /// This is deliberately conservative.
        /// </summary>
        private int FindLikelyPoweredPositiveTerminal(
            SparkAIWorldSnapshot snapshot)
        {
            IReadOnlyList<SparkAITerminalSnapshot> terminals =
                snapshot.Terminals;

            if (terminals == null)
                return 0;

            for (int i = 0;
                 i < terminals.Count;
                 i++)
            {
                SparkAITerminalSnapshot terminal =
                    terminals[i];

                if (terminal.InstanceId == 0)
                    continue;

                if (!terminal.Powered)
                    continue;

                if (IsPositivePolarity(
                        terminal.Polarity) ||
                    IsPositivePolarity(
                        terminal.EffectivePolarity))
                {
                    return terminal.InstanceId;
                }
            }

            return 0;
        }

        /// <summary>
        /// Finds a likely powered negative source terminal.
        ///
        /// The terminal must be powered as well as having
        /// negative polarity. This prevents an arbitrary
        /// negative load terminal from being treated as the
        /// source negative terminal.
        /// </summary>
        private int FindLikelyPoweredNegativeTerminal(
            SparkAIWorldSnapshot snapshot)
        {
            IReadOnlyList<SparkAITerminalSnapshot> terminals =
                snapshot.Terminals;

            if (terminals == null)
                return 0;

            for (int i = 0;
                 i < terminals.Count;
                 i++)
            {
                SparkAITerminalSnapshot terminal =
                    terminals[i];

                if (terminal.InstanceId == 0)
                    continue;

                if (!terminal.Powered)
                    continue;

                if (!IsNegativePolarity(
                        terminal.Polarity) &&
                    !IsNegativePolarity(
                        terminal.EffectivePolarity))
                {
                    continue;
                }

                return terminal.InstanceId;
            }

            return 0;
        }

        private bool IsPositivePolarity(
            string polarity)
        {
            if (string.IsNullOrWhiteSpace(polarity))
                return false;

            return string.Equals(
                       polarity,
                       "Positive",
                       StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(
                       polarity,
                       "Anode",
                       StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(
                       polarity,
                       "+",
                       StringComparison.OrdinalIgnoreCase);
        }

        private bool IsNegativePolarity(
            string polarity)
        {
            if (string.IsNullOrWhiteSpace(polarity))
                return false;

            return string.Equals(
                       polarity,
                       "Negative",
                       StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(
                       polarity,
                       "Cathode",
                       StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(
                       polarity,
                       "-",
                       StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Determines whether two terminals belong to the same
        /// connected topology.
        /// </summary>
        private bool AreConnected(
            int start,
            int target)
        {
            if (start == 0 ||
                target == 0)
            {
                return false;
            }

            if (start == target)
                return true;

            visited.Clear();
            searchQueue.Clear();

            visited.Add(start);
            searchQueue.Enqueue(start);

            while (searchQueue.Count > 0)
            {
                int current =
                    searchQueue.Dequeue();

                if (!adjacency.TryGetValue(
                        current,
                        out List<int> neighbors))
                {
                    continue;
                }

                for (int i = 0;
                     i < neighbors.Count;
                     i++)
                {
                    int next =
                        neighbors[i];

                    if (next == target)
                        return true;

                    if (visited.Add(next))
                        searchQueue.Enqueue(next);
                }
            }

            return false;
        }

        /// <summary>
        /// Detects a direct source-to-source connection.
        ///
        /// This is a topology warning, not an electrical
        /// solver result.
        /// </summary>
        private bool DetectSourceShort(
            int positiveTerminalId,
            int negativeTerminalId)
        {
            if (positiveTerminalId == 0 ||
                negativeTerminalId == 0)
            {
                return false;
            }

            if (!adjacency.TryGetValue(
                    positiveTerminalId,
                    out List<int> neighbors))
            {
                return false;
            }

            for (int i = 0;
                 i < neighbors.Count;
                 i++)
            {
                if (neighbors[i] ==
                    negativeTerminalId)
                {
                    return true;
                }
            }

            return false;
        }

        private int CountReachableTerminals(
            int start)
        {
            if (start == 0)
                return 0;

            visited.Clear();
            searchQueue.Clear();

            visited.Add(start);
            searchQueue.Enqueue(start);

            while (searchQueue.Count > 0)
            {
                int current =
                    searchQueue.Dequeue();

                if (!adjacency.TryGetValue(
                        current,
                        out List<int> neighbors))
                {
                    continue;
                }

                for (int i = 0;
                     i < neighbors.Count;
                     i++)
                {
                    int next =
                        neighbors[i];

                    if (visited.Add(next))
                        searchQueue.Enqueue(next);
                }
            }

            return visited.Count;
        }

        private int CountConnectedComponents()
        {
            visited.Clear();

            int componentCount = 0;

            foreach (KeyValuePair<int, List<int>> pair
                     in adjacency)
            {
                int start =
                    pair.Key;

                if (visited.Contains(start))
                    continue;

                componentCount++;

                searchQueue.Clear();
                searchQueue.Enqueue(start);
                visited.Add(start);

                while (searchQueue.Count > 0)
                {
                    int current =
                        searchQueue.Dequeue();

                    if (!adjacency.TryGetValue(
                            current,
                            out List<int> neighbors))
                    {
                        continue;
                    }

                    for (int i = 0;
                         i < neighbors.Count;
                         i++)
                    {
                        int next =
                            neighbors[i];

                        if (visited.Add(next))
                            searchQueue.Enqueue(next);
                    }
                }
            }

            return componentCount;
        }
    }

    public enum SparkAICircuitTopologyState
    {
        Unknown = 0,
        NoCircuit = 1,
        NoConnections = 2,
        NoSource = 3,
        OpenCircuit = 4,
        ClosedCircuit = 5,
        ShortCircuit = 6
    }

    /// <summary>
    /// Immutable result of one topology analysis.
    /// </summary>
    public readonly struct SparkAICircuitAnalysis
    {
        public bool IsValid { get; }

        public SparkAICircuitTopologyState TopologyState { get; }

        public bool HasSource { get; }

        public bool ReturnPathClosed { get; }

        public bool SourceShorted { get; }

        public bool HasPositiveSource { get; }

        public bool HasNegativeSource { get; }

        public int PositiveSourceTerminalId { get; }

        public int NegativeSourceTerminalId { get; }

        public int ReachableTerminalCount { get; }

        public int ConnectedComponentCount { get; }

        public int ConnectionCount { get; }

        public int TerminalCount { get; }

        public SparkAICircuitAnalysis(
            bool isValid,
            SparkAICircuitTopologyState topologyState,
            bool hasSource,
            bool returnPathClosed,
            bool sourceShorted,
            bool hasPositiveSource,
            bool hasNegativeSource,
            int positiveSourceTerminalId,
            int negativeSourceTerminalId,
            int reachableTerminalCount,
            int connectedComponentCount,
            int connectionCount,
            int terminalCount)
        {
            IsValid = isValid;
            TopologyState = topologyState;
            HasSource = hasSource;
            ReturnPathClosed = returnPathClosed;
            SourceShorted = sourceShorted;
            HasPositiveSource = hasPositiveSource;
            HasNegativeSource = hasNegativeSource;
            PositiveSourceTerminalId =
                positiveSourceTerminalId;
            NegativeSourceTerminalId =
                negativeSourceTerminalId;
            ReachableTerminalCount =
                reachableTerminalCount;
            ConnectedComponentCount =
                connectedComponentCount;
            ConnectionCount =
                connectionCount;
            TerminalCount =
                terminalCount;
        }

        public static SparkAICircuitAnalysis Unavailable()
        {
            return new SparkAICircuitAnalysis(
                false,
                SparkAICircuitTopologyState.Unknown,
                false,
                false,
                false,
                false,
                false,
                0,
                0,
                0,
                0,
                0,
                0);
        }

        public static SparkAICircuitAnalysis NoCircuit()
        {
            return new SparkAICircuitAnalysis(
                true,
                SparkAICircuitTopologyState.NoCircuit,
                false,
                false,
                false,
                false,
                false,
                0,
                0,
                0,
                0,
                0,
                0);
        }

        public static SparkAICircuitAnalysis NoConnections()
        {
            return new SparkAICircuitAnalysis(
                true,
                SparkAICircuitTopologyState.NoConnections,
                false,
                false,
                false,
                false,
                false,
                0,
                0,
                0,
                0,
                0,
                0);
        }
    }
}
