using System;
using System.Collections.Generic;
using UnityEngine;
using ProjectSpark.Circuit;
using ProjectSpark.Gameplay;

namespace ProjectSpark.Electrical
{
    /// <summary>
    /// Resolves signed ideal-wire currents using KCL for a known set of
    /// terminal injections. Positive wire current means connection A -> B.
    ///
    /// Exact results are returned only for bridge edges (edges that are not
    /// part of a wire-only cycle). Cyclic wire currents are underdetermined.
    /// Caller must provide the net current ENTERING each terminal from its
    /// attached components, with positive meaning into the terminal/wire node.
    /// </summary>
    public sealed class SparkWireBranchCurrentResolver
    {
        public struct WireCurrent
        {
            public ulong ConnectionId;
            public float AmpsAtoB;
            public WireCurrent(ulong id, float amps) { ConnectionId = id; AmpsAtoB = amps; }
        }

        private struct Edge
        {
            public int A, B;
            public ulong Id;
            public Edge(int a, int b, ulong id) { A = a; B = b; Id = id; }
        }

        /// <summary>
        /// injections: signed current entering the wire network at each
        /// terminal from components. Include zero entries for known terminals.
        /// Returns only currents that are uniquely fixed by KCL.
        /// If terminal injections are incomplete, do not call this method.
        /// </summary>
        public bool TryResolve(
            SparkCircuitSystem circuit,
            IReadOnlyDictionary<SparkTerminal, float> injections,
            List<WireCurrent> resolved,
            out string diagnostic,
            float tolerance = 0.0001f)
        {
            if (resolved == null) throw new ArgumentNullException(nameof(resolved));
            resolved.Clear();
            diagnostic = null;
            if (circuit == null || injections == null)
            {
                diagnostic = "Missing circuit or terminal injections.";
                return false;
            }

            var connections = new List<SparkCircuitConnection>();
            circuit.CopyConnections(connections);
            var indices = new Dictionary<SparkTerminal, int>();
            var terminals = new List<SparkTerminal>();
            var edges = new List<Edge>();
            foreach (var connection in connections)
            {
                if (!connection.IsValid || connection.Kind == SparkConnectionKind.Probe ||
                    connection.A == null || connection.B == null ||
                    connection.A.Owner == null || connection.B.Owner == null ||
                    !connection.A.Owner.isActiveAndEnabled ||
                    !connection.B.Owner.isActiveAndEnabled) continue;
                int a = GetIndex(connection.A, indices, terminals);
                int b = GetIndex(connection.B, indices, terminals);
                edges.Add(new Edge(a, b, connection.Id));
            }
            int n = terminals.Count;
            var injection = new double[n];
            for (int i = 0; i < n; i++)
            {
                if (!injections.TryGetValue(terminals[i], out float value) ||
                    float.IsNaN(value) || float.IsInfinity(value))
                {
                    diagnostic = "Missing/invalid injection for terminal: " + terminals[i].name;
                    return false;
                }
                injection[i] = value;
            }

            var adjacency = new List<int>[n];
            for (int i = 0; i < n; i++) adjacency[i] = new List<int>();
            for (int i = 0; i < edges.Count; i++)
            {
                adjacency[edges[i].A].Add(i);
                adjacency[edges[i].B].Add(i);
            }

            // Tarjan bridge detection, correctly handles parallel wire edges.
            var tin = new int[n];
            var low = new int[n];
            for (int i = 0; i < n; i++) tin[i] = -1;
            var bridge = new bool[edges.Count];
            int timer = 0;
            Action<int, int> dfs = null;
            dfs = (v, parentEdge) =>
            {
                tin[v] = low[v] = timer++;
                foreach (int ei in adjacency[v])
                {
                    if (ei == parentEdge) continue;
                    Edge e = edges[ei];
                    int to = e.A == v ? e.B : e.A;
                    if (tin[to] >= 0) low[v] = Math.Min(low[v], tin[to]);
                    else
                    {
                        dfs(to, ei);
                        low[v] = Math.Min(low[v], low[to]);
                        if (low[to] > tin[v]) bridge[ei] = true;
                    }
                }
            };
            for (int i = 0; i < n; i++) if (tin[i] < 0) dfs(i, -1);

            // A bridge splits the connected network into two sets. The sum of
            // injections on the A side equals current A -> B across that bridge.
            var seen = new bool[n];
            int unresolved = 0;
            for (int ei = 0; ei < edges.Count; ei++)
            {
                if (!bridge[ei]) { unresolved++; continue; }
                Array.Clear(seen, 0, n);
                var queue = new Queue<int>();
                queue.Enqueue(edges[ei].A);
                seen[edges[ei].A] = true;
                double sum = 0;
                while (queue.Count > 0)
                {
                    int v = queue.Dequeue();
                    sum += injection[v];
                    foreach (int otherEdge in adjacency[v])
                    {
                        if (otherEdge == ei) continue;
                        Edge e = edges[otherEdge];
                        int to = e.A == v ? e.B : e.A;
                        if (seen[to]) continue;
                        seen[to] = true;
                        queue.Enqueue(to);
                    }
                }
                resolved.Add(new WireCurrent(edges[ei].Id, (float)sum));
            }

            // Check conservation for each connected component.
            Array.Clear(seen, 0, n);
            for (int i = 0; i < n; i++)
            {
                if (seen[i]) continue;
                var queue = new Queue<int>();
                queue.Enqueue(i);
                seen[i] = true;
                double total = 0;
                while (queue.Count > 0)
                {
                    int v = queue.Dequeue();
                    total += injection[v];
                    foreach (int ei in adjacency[v])
                    {
                        Edge e = edges[ei];
                        int to = e.A == v ? e.B : e.A;
                        if (seen[to]) continue;
                        seen[to] = true;
                        queue.Enqueue(to);
                    }
                }
                if (Math.Abs(total) > tolerance)
                {
                    resolved.Clear();
                    diagnostic = "Terminal injections violate KCL; residual " + total.ToString("G6") + " A.";
                    return false;
                }
            }
            diagnostic = unresolved == 0 ? "All wire currents resolved." :
                unresolved + " wire(s) lie on ideal-wire cycles; currents not uniquely determined.";
            return true;
        }

        private static int GetIndex(SparkTerminal terminal,
            Dictionary<SparkTerminal, int> indices, List<SparkTerminal> terminals)
        {
            if (indices.TryGetValue(terminal, out int index)) return index;
            index = terminals.Count;
            indices.Add(terminal, index);
            terminals.Add(terminal);
            return index;
        }
    }
}
