using System;
using System.Collections.Generic;
using ProjectSpark.Gameplay;
using UnityEngine;

namespace ProjectSpark.Circuit
{
    [DisallowMultipleComponent]
    public sealed class SparkCircuitSystem : MonoBehaviour
    {
        private readonly Dictionary<
            ulong,
            SparkCircuitConnection> connections =
            new Dictionary<
                ulong,
                SparkCircuitConnection>();


        private readonly Dictionary<
            SparkTerminal,
            List<ulong>> terminalConnections =
            new Dictionary<
                SparkTerminal,
                List<ulong>>();


        private ulong nextId = 1;

        private bool topologyDirty;

        private int topologyVersion;


        // =========================================================
        // PUBLIC STATE
        // =========================================================

        public int TopologyVersion =>
            topologyVersion;

        public int ConnectionCount =>
            connections.Count;

        public bool IsTopologyDirty =>
            topologyDirty;


        // =========================================================
        // EVENTS
        // =========================================================

        public event Action TopologyChanged;

        public event Action<SparkCircuitConnection>
            ConnectionCreated;

        public event Action<SparkCircuitConnection>
            ConnectionRemoved;


        // =========================================================
        // CREATE
        // =========================================================

        public SparkResult TryCreateConnection(
            SparkTerminal a,
            SparkTerminal b,
            SparkConnectionKind kind,
            SparkConnectionDirection direction,
            out SparkCircuitConnection connection)
        {
            connection = default;


            if (a == null ||
                b == null)
            {
                return SparkResult.Invalid(
                    "Missing terminal.");
            }


            if (!a.CanConnectTo(
                    b,
                    kind,
                    direction,
                    out string reason))
            {
                return SparkResult.Rejected(reason);
            }


            if (HasConnection(
                    a,
                    b,
                    kind))
            {
                return SparkResult.Rejected(
                    "Connection already exists.");
            }


            // -----------------------------------------------------
            // REGISTER A
            // -----------------------------------------------------

            if (!RegisterTerminal(
                    a,
                    out reason))
            {
                return SparkResult.Rejected(reason);
            }


            // -----------------------------------------------------
            // REGISTER B
            // -----------------------------------------------------

            if (!RegisterTerminal(
                    b,
                    out reason))
            {
                a.UnregisterConnection();

                if (terminalConnections.TryGetValue(
                        a,
                        out List<ulong> rollbackList) &&
                    rollbackList.Count == 0)
                {
                    terminalConnections.Remove(a);
                }

                return SparkResult.Rejected(reason);
            }


            // -----------------------------------------------------
            // ID
            // -----------------------------------------------------

            ulong id = nextId++;

            if (id == 0)
            {
                id = nextId++;
            }


            // -----------------------------------------------------
            // CONNECTION
            // -----------------------------------------------------

            connection =
                new SparkCircuitConnection(
                    id,
                    a,
                    b,
                    kind,
                    direction);


            connections.Add(
                id,
                connection);


            terminalConnections[a].Add(id);

            terminalConnections[b].Add(id);


            // -----------------------------------------------------
            // TOPOLOGY
            // -----------------------------------------------------

            MarkTopologyDirty();


            ConnectionCreated?.Invoke(
                connection);


            return SparkResult.Success(
                "Circuit connection created.");
        }


        // =========================================================
        // REMOVE
        // =========================================================

        public SparkResult TryRemoveConnection(
            ulong id,
            out SparkCircuitConnection connection)
        {
            if (!connections.TryGetValue(
                    id,
                    out connection))
            {
                return SparkResult.Invalid(
                    "Connection not found.");
            }


            connections.Remove(id);


            UnregisterTerminal(
                id,
                connection.A);

            UnregisterTerminal(
                id,
                connection.B);


            connection.A.UnregisterConnection();

            connection.B.UnregisterConnection();


            MarkTopologyDirty();


            ConnectionRemoved?.Invoke(
                connection);


            return SparkResult.Success(
                "Circuit connection removed.");
        }


        // =========================================================
        // HAS CONNECTION
        // =========================================================

        public bool HasConnection(
            SparkTerminal a,
            SparkTerminal b,
            SparkConnectionKind? kind = null)
        {
            foreach (
                KeyValuePair<
                    ulong,
                    SparkCircuitConnection> pair
                in connections)
            {
                SparkCircuitConnection connection =
                    pair.Value;


                if (kind.HasValue &&
                    connection.Kind != kind.Value)
                {
                    continue;
                }


                if (
                    (connection.A == a &&
                     connection.B == b) ||

                    (connection.A == b &&
                     connection.B == a)
                   )
                {
                    return true;
                }
            }

            return false;
        }


        // =========================================================
        // GET CONNECTION
        // =========================================================

        public bool TryGetConnection(
            ulong id,
            out SparkCircuitConnection connection)
        {
            return connections.TryGetValue(
                id,
                out connection);
        }


        // =========================================================
        // GET TERMINAL CONNECTIONS
        // =========================================================

        public void GetConnections(
            SparkTerminal terminal,
            List<SparkCircuitConnection> buffer)
        {
            buffer.Clear();


            if (terminal == null)
                return;


            if (!terminalConnections.TryGetValue(
                    terminal,
                    out List<ulong> ids))
            {
                return;
            }


            for (int i = 0;
                 i < ids.Count;
                 i++)
            {
                if (connections.TryGetValue(
                        ids[i],
                        out SparkCircuitConnection connection))
                {
                    buffer.Add(connection);
                }
            }
        }


        // =========================================================
        // REBUILD
        // =========================================================

        public void RebuildTopology()
        {
            topologyDirty = false;
        }


        // =========================================================
        // REGISTER TERMINAL
        // =========================================================

        private bool RegisterTerminal(
            SparkTerminal terminal,
            out string reason)
        {
            if (!terminal.RegisterConnection(
                    out reason))
            {
                return false;
            }


            if (!terminalConnections.ContainsKey(
                    terminal))
            {
                terminalConnections.Add(
                    terminal,
                    new List<ulong>(2));
            }


            return true;
        }


        // =========================================================
        // UNREGISTER TERMINAL
        // =========================================================

        private void UnregisterTerminal(
            ulong id,
            SparkTerminal terminal)
        {
            if (!terminalConnections.TryGetValue(
                    terminal,
                    out List<ulong> list))
            {
                return;
            }


            list.Remove(id);


            if (list.Count == 0)
            {
                terminalConnections.Remove(
                    terminal);
            }
        }


        // =========================================================
        // TOPOLOGY DIRTY
        // =========================================================

        private void MarkTopologyDirty()
        {
            topologyDirty = true;

            topologyVersion++;


            TopologyChanged?.Invoke();
        }
    }
}