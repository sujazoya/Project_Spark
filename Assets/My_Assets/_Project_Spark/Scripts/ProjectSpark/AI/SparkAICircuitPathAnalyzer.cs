using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectSpark.AI
{
    /// <summary>
    /// Traces topology between two Spark terminals.
    ///
    /// AI-side observation only.
    ///
    /// This component does NOT:
    /// - modify SparkCircuitSystem
    /// - modify SparkElectricalSolver
    /// - create/remove connections
    /// - calculate authoritative voltage/current
    /// - determine level completion
    ///
    /// It answers:
    ///
    /// "Is there a connection path from terminal A to terminal B?"
    ///
    /// and returns the ordered terminals and authoritative connections
    /// that form that path.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkAICircuitPathAnalyzer : MonoBehaviour
    {
        [Header("Spark AI")]
        [SerializeField]
        private SparkAIWorld world;

        private readonly Dictionary<int, List<int>> adjacency =
            new Dictionary<int, List<int>>(128);

        private readonly Dictionary<int, SparkAITerminalSnapshot>
            terminalById =
                new Dictionary<int, SparkAITerminalSnapshot>(128);

        private readonly Dictionary<ulong, SparkAIConnectionSnapshot>
            connectionById =
                new Dictionary<ulong, SparkAIConnectionSnapshot>(128);

        private readonly Dictionary<int, int> previous =
            new Dictionary<int, int>(128);

        private readonly Queue<int> queue =
            new Queue<int>(128);

        private readonly HashSet<int> visited =
            new HashSet<int>();

        private readonly List<int> terminalPathBuffer =
            new List<int>(64);

        private readonly List<SparkAIConnectionSnapshot>
            connectionPathBuffer =
                new List<SparkAIConnectionSnapshot>(64);

        private bool initialized;

        public bool IsInitialized =>
            initialized;

        private void Awake()
        {
            if (world == null)
            {
                Debug.LogError(
                    "[Spark AI Path Analyzer] " +
                    "SparkAIWorld reference is missing.",
                    this);

                return;
            }

            initialized = true;
        }

        /// <summary>
        /// Finds a topology path between two terminals using
        /// the latest Spark AI world snapshot.
        /// </summary>
        public SparkAICircuitPathAnalysis FindPath(
            int startTerminalId,
            int targetTerminalId)
        {
            if (!initialized)
            {
                return SparkAICircuitPathAnalysis.Unavailable();
            }

            return FindPath(
                world.LatestSnapshot,
                startTerminalId,
                targetTerminalId);
        }

        /// <summary>
        /// Finds a topology path between two terminals in the
        /// supplied snapshot.
        /// </summary>
        public SparkAICircuitPathAnalysis FindPath(
            SparkAIWorldSnapshot snapshot,
            int startTerminalId,
            int targetTerminalId)
        {
            if (!initialized)
            {
                return SparkAICircuitPathAnalysis.Unavailable();
            }

            if (snapshot == null)
            {
                return SparkAICircuitPathAnalysis.Unavailable();
            }

            if (startTerminalId == 0 ||
                targetTerminalId == 0)
            {
                return SparkAICircuitPathAnalysis.InvalidTerminals();
            }

            BuildGraph(snapshot);

            if (!terminalById.ContainsKey(startTerminalId) ||
                !terminalById.ContainsKey(targetTerminalId))
            {
                return SparkAICircuitPathAnalysis.TerminalNotFound();
            }

            /*
             * Same terminal means a valid zero-length path.
             */
            if (startTerminalId == targetTerminalId)
            {
                terminalPathBuffer.Clear();
                connectionPathBuffer.Clear();

                terminalPathBuffer.Add(
                    startTerminalId);

                return new SparkAICircuitPathAnalysis(
                    true,
                    true,
                    startTerminalId,
                    targetTerminalId,
                    terminalPathBuffer.ToArray(),
                    connectionPathBuffer.ToArray());
            }

            bool found =
                Search(
                    startTerminalId,
                    targetTerminalId);

            if (!found)
            {
                return new SparkAICircuitPathAnalysis(
                    true,
                    false,
                    startTerminalId,
                    targetTerminalId,
                    Array.Empty<int>(),
                    Array.Empty<SparkAIConnectionSnapshot>());
            }

            BuildPath(
                startTerminalId,
                targetTerminalId);

            return new SparkAICircuitPathAnalysis(
                true,
                terminalPathBuffer.Count > 0,
                startTerminalId,
                targetTerminalId,
                terminalPathBuffer.ToArray(),
                connectionPathBuffer.ToArray());
        }

        /// <summary>
        /// Builds an undirected topology graph from the authoritative
        /// circuit snapshot.
        ///
        /// Direction is retained inside each connection snapshot,
        /// but electrical topology traversal itself is treated as
        /// connectivity between terminals.
        /// </summary>
        private void BuildGraph(
            SparkAIWorldSnapshot snapshot)
        {
            adjacency.Clear();
            terminalById.Clear();
            connectionById.Clear();

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

                int sourceId =
                    connection.SourceTerminalInstanceId;

                int targetId =
                    connection.TargetTerminalInstanceId;

                if (sourceId == 0 ||
                    targetId == 0)
                {
                    continue;
                }

                connectionById[
                    connection.Id] =
                    connection;

                AddEdge(
                    sourceId,
                    targetId);

                AddEdge(
                    targetId,
                    sourceId);
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
            {
                neighbors.Add(to);
            }
        }

        /// <summary>
        /// Breadth-first search.
        ///
        /// This produces a deterministic shortest topology path.
        /// </summary>
        private bool Search(
            int start,
            int target)
        {
            previous.Clear();
            visited.Clear();
            queue.Clear();

            visited.Add(start);
            previous[start] = 0;

            queue.Enqueue(start);

            while (queue.Count > 0)
            {
                int current =
                    queue.Dequeue();

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

                    if (!visited.Add(next))
                        continue;

                    previous[next] =
                        current;

                    if (next == target)
                    {
                        return true;
                    }

                    queue.Enqueue(next);
                }
            }

            return false;
        }

        /// <summary>
        /// Reconstructs the terminal path and connection path
        /// after BFS has found the target.
        /// </summary>
        private void BuildPath(
            int start,
            int target)
        {
            terminalPathBuffer.Clear();
            connectionPathBuffer.Clear();

            int current =
                target;

            while (current != 0)
            {
                terminalPathBuffer.Add(
                    current);

                if (current == start)
                    break;

                if (!previous.TryGetValue(
                        current,
                        out int parent))
                {
                    terminalPathBuffer.Clear();
                    connectionPathBuffer.Clear();
                    return;
                }

                SparkAIConnectionSnapshot connection;

                if (TryFindConnection(
                        parent,
                        current,
                        out connection))
                {
                    connectionPathBuffer.Add(
                        connection);
                }

                current =
                    parent;
            }

            terminalPathBuffer.Reverse();
            connectionPathBuffer.Reverse();
        }

        /// <summary>
        /// Finds the authoritative connection joining two terminals.
        ///
        /// SparkAIConnectionSnapshot stores source/target terminal IDs,
        /// so the pair is checked in both orientations.
        /// </summary>
        private bool TryFindConnection(
            int firstTerminalId,
            int secondTerminalId,
            out SparkAIConnectionSnapshot connection)
        {
            foreach (KeyValuePair<
                         ulong,
                         SparkAIConnectionSnapshot> pair
                     in connectionById)
            {
                SparkAIConnectionSnapshot candidate =
                    pair.Value;

                if (!candidate.Valid)
                    continue;

                bool forward =
                    candidate.SourceTerminalInstanceId ==
                        firstTerminalId &&
                    candidate.TargetTerminalInstanceId ==
                        secondTerminalId;

                bool reverse =
                    candidate.SourceTerminalInstanceId ==
                        secondTerminalId &&
                    candidate.TargetTerminalInstanceId ==
                        firstTerminalId;

                if (forward || reverse)
                {
                    connection =
                        candidate;

                    return true;
                }
            }

            connection =
                default;

            return false;
        }
    }

    /// <summary>
    /// Immutable result of a topology path search.
    /// </summary>
    public readonly struct SparkAICircuitPathAnalysis
    {
        public bool IsValid { get; }

        public bool PathExists { get; }

        public int StartTerminalId { get; }

        public int TargetTerminalId { get; }

        public int[] TerminalPath { get; }

        public SparkAIConnectionSnapshot[] ConnectionPath { get; }

        public int TerminalCount =>
            TerminalPath != null
                ? TerminalPath.Length
                : 0;

        public int ConnectionCount =>
            ConnectionPath != null
                ? ConnectionPath.Length
                : 0;

        public SparkAICircuitPathAnalysis(
            bool isValid,
            bool pathExists,
            int startTerminalId,
            int targetTerminalId,
            int[] terminalPath,
            SparkAIConnectionSnapshot[] connectionPath)
        {
            IsValid =
                isValid;

            PathExists =
                pathExists;

            StartTerminalId =
                startTerminalId;

            TargetTerminalId =
                targetTerminalId;

            TerminalPath =
                terminalPath ??
                Array.Empty<int>();

            ConnectionPath =
                connectionPath ??
                Array.Empty<SparkAIConnectionSnapshot>();
        }

        public static SparkAICircuitPathAnalysis
            Unavailable()
        {
            return new SparkAICircuitPathAnalysis(
                false,
                false,
                0,
                0,
                Array.Empty<int>(),
                Array.Empty<SparkAIConnectionSnapshot>());
        }

        public static SparkAICircuitPathAnalysis
            InvalidTerminals()
        {
            return new SparkAICircuitPathAnalysis(
                false,
                false,
                0,
                0,
                Array.Empty<int>(),
                Array.Empty<SparkAIConnectionSnapshot>());
        }

        public static SparkAICircuitPathAnalysis
            TerminalNotFound()
        {
            return new SparkAICircuitPathAnalysis(
                true,
                false,
                0,
                0,
                Array.Empty<int>(),
                Array.Empty<SparkAIConnectionSnapshot>());
        }
    }
}