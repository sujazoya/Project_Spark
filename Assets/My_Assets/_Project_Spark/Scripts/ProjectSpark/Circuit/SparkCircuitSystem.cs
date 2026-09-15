using System;
using System.Collections.Generic;
using ProjectSpark.Gameplay;
using UnityEngine;

namespace ProjectSpark.Circuit
{
    [DisallowMultipleComponent]
    public sealed class SparkCircuitSystem : MonoBehaviour
    {
        private readonly Dictionary<ulong, SparkCircuitConnection> connections = new();
        private readonly Dictionary<SparkTerminal, List<ulong>> terminalConnections = new();
        private ulong nextId = 1;
        private bool topologyDirty;
        private int topologyVersion;

        public int TopologyVersion => topologyVersion;
        public int ConnectionCount => connections.Count;
        public bool IsTopologyDirty => topologyDirty;
        public event Action TopologyChanged;
        public event Action<SparkCircuitConnection> ConnectionCreated;
        public event Action<SparkCircuitConnection> ConnectionRemoved;

        public SparkResult TryCreateConnection(SparkTerminal a, SparkTerminal b, SparkConnectionKind kind,
            SparkConnectionDirection direction, out SparkCircuitConnection connection)
        {
            connection = default;
            if (a == null || b == null) return SparkResult.Invalid("Missing terminal.");
            if (!a.CanConnectTo(b, kind, direction, out var reason)) return SparkResult.Rejected(reason);
            if (HasConnection(a, b, kind)) return SparkResult.Rejected("Connection already exists.");

            if (!RegisterTerminal(a, out reason)) return SparkResult.Rejected(reason);
            if (!RegisterTerminal(b, out reason))
            {
                a.UnregisterConnection();
                if (terminalConnections.TryGetValue(a, out var rollbackList) && rollbackList.Count == 0)
                    terminalConnections.Remove(a);
                return SparkResult.Rejected(reason);
            }

            var id = nextId++;
            if (id == 0) id = nextId++;
            connection = new SparkCircuitConnection(id, a, b, kind, direction);
            connections.Add(id, connection);
            terminalConnections[a].Add(id);
            terminalConnections[b].Add(id);

            MarkTopologyDirty();
            ConnectionCreated?.Invoke(connection);
            return SparkResult.Success();
        }

        public SparkResult TryRemoveConnection(ulong id, out SparkCircuitConnection connection)
        {
            if (!connections.TryGetValue(id, out connection)) return SparkResult.Invalid("Connection not found.");

            connections.Remove(id);
            UnregisterTerminal(id, connection.A);
            UnregisterTerminal(id, connection.B);
            connection.A.UnregisterConnection();
            connection.B.UnregisterConnection();

            MarkTopologyDirty();
            ConnectionRemoved?.Invoke(connection);
            return SparkResult.Success();
        }

        public bool HasConnection(SparkTerminal a, SparkTerminal b, SparkConnectionKind? kind = null)
        {
            foreach (var pair in connections)
            {
                var connection = pair.Value;
                if (kind.HasValue && connection.Kind != kind.Value) continue;
                if ((connection.A == a && connection.B == b) || (connection.A == b && connection.B == a))
                    return true;
            }
            return false;
        }

        public bool TryGetConnection(ulong id, out SparkCircuitConnection connection) => connections.TryGetValue(id, out connection);

        public void GetConnections(SparkTerminal terminal, List<SparkCircuitConnection> buffer)
        {
            buffer.Clear();
            if (terminal == null || !terminalConnections.TryGetValue(terminal, out var ids)) return;
            for (int i = 0; i < ids.Count; i++)
                if (connections.TryGetValue(ids[i], out var connection)) buffer.Add(connection);
        }

        public void RebuildTopology()
        {
            // Connections are kept authoritative in the dictionaries. Rebuilding is
            // intentionally lightweight; the solver reads this data directly.
            topologyDirty = false;
        }

        private bool RegisterTerminal(SparkTerminal terminal, out string reason)
        {
            if (!terminal.RegisterConnection(out reason)) return false;
            if (!terminalConnections.ContainsKey(terminal))
                terminalConnections.Add(terminal, new List<ulong>(2));
            return true;
        }

        private void UnregisterTerminal(ulong id, SparkTerminal terminal)
        {
            if (!terminalConnections.TryGetValue(terminal, out var list)) return;
            list.Remove(id);
            if (list.Count == 0) terminalConnections.Remove(terminal);
        }

        private void MarkTopologyDirty()
        {
            topologyDirty = true;
            topologyVersion++;
            TopologyChanged?.Invoke();
        }
    }
}
