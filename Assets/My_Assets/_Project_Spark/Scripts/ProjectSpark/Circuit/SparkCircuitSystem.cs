using System;
using System.Collections.Generic;
using UnityEngine;
using ProjectSpark.Gameplay;

namespace ProjectSpark.Circuit
{
    [DisallowMultipleComponent]
    public sealed class SparkCircuitSystem : MonoBehaviour
    {
        // =========================================================
        // CONFIGURATION
        // =========================================================

        [Header("Connection Policy")]
        [SerializeField]
        private bool allowParallelConnections;

        [SerializeField]
        private bool automaticallyRebuildTopology = true;

        [Header("Diagnostics")]
        [SerializeField]
        private bool logConnectionChanges;

        [SerializeField]
        private bool logRejectedConnections;

        // =========================================================
        // ELECTRIC FLOW VISUALS
        // =========================================================

        [Header("Electric Flow Materials")]
        [Tooltip(
            "Material used when a wire is disconnected or has no "
            + "electrical state.")]
        [SerializeField]
        private Material neutralWireMaterial;

        [Tooltip(
            "Material used when positive voltage is present but "
            + "meaningful current is not flowing.")]
        [SerializeField]
        private Material positiveVoltageMaterial;

        [Tooltip(
            "Material used when negative/return voltage is present "
            + "but meaningful current is not flowing.")]
        [SerializeField]
        private Material negativeVoltageMaterial;

        [Tooltip(
            "Material used when positive current is actually flowing.")]
        [SerializeField]
        private Material positiveCurrentFlowMaterial;

        [Tooltip(
            "Material used when negative/return current is actually flowing.")]
        [SerializeField]
        private Material negativeCurrentFlowMaterial;

        [Tooltip(
            "Material used for a detected electrical fault or short.")]
        [SerializeField]
        private Material shortCircuitMaterial;

        [Header("Electric Flow Thresholds")]
        [SerializeField, Min(0f)]
        private float voltageVisualThreshold = 0.001f;

        [SerializeField, Min(0f)]
        private float currentVisualThreshold = 0.001f;

        // =========================================================
        // CONNECTION STORAGE
        // =========================================================

        private readonly Dictionary<ulong, SparkCircuitConnection>
            connections =
                new Dictionary<ulong, SparkCircuitConnection>(256);

        // Terminal -> connection IDs.
        private readonly Dictionary<SparkTerminal, List<ulong>>
            terminalConnections =
                new Dictionary<SparkTerminal, List<ulong>>(256);

        // Reusable query buffer.
        private readonly List<ulong>
            connectionIdBuffer =
                new List<ulong>(64);

        private readonly List<SparkCircuitConnection>
            connectionBuffer =
                new List<SparkCircuitConnection>(64);

        // =========================================================
        // RUNTIME STATE
        // =========================================================

        private ulong nextConnectionId = 1UL;

        private int topologyVersion;

        private bool topologyDirty;

        private bool rebuilding;

        // =========================================================
        // PUBLIC STATE
        // =========================================================

        public int ConnectionCount =>
            connections.Count;

        public int TopologyVersion =>
            topologyVersion;

        public bool IsTopologyDirty =>
            topologyDirty;

        public bool IsEmpty =>
            connections.Count == 0;

        // =========================================================
        // ELECTRIC FLOW VISUAL PUBLIC STATE
        // =========================================================

        public Material NeutralWireMaterial =>
            neutralWireMaterial;

        public Material PositiveVoltageMaterial =>
            positiveVoltageMaterial;

        public Material NegativeVoltageMaterial =>
            negativeVoltageMaterial;

        public Material PositiveCurrentFlowMaterial =>
            positiveCurrentFlowMaterial;

        public Material NegativeCurrentFlowMaterial =>
            negativeCurrentFlowMaterial;

        public Material ShortCircuitMaterial =>
            shortCircuitMaterial;

        public float VoltageVisualThreshold =>
            voltageVisualThreshold;

        public float CurrentVisualThreshold =>
            currentVisualThreshold;

        // =========================================================
        // EVENTS
        // =========================================================

        public event Action
            TopologyChanged;

        public event Action<SparkCircuitConnection>
            ConnectionCreated;

        public event Action<SparkCircuitConnection>
            ConnectionRemoved;

        /// <summary>
        /// Fired when the electrical visual state of a connection
        /// should be refreshed.
        ///
        /// The electrical solver should trigger this after it has
        /// calculated voltage/current states.
        /// </summary>
        public event Action
            ElectricalFlowVisualsChanged;

        // =========================================================
        // UNITY
        // =========================================================

        private void Awake()
        {
            if (automaticallyRebuildTopology)
            {
                RebuildTopology();
            }
        }

        private void OnDestroy()
        {
            ClearConnections(false);
        }

        // =========================================================
        // CREATE CONNECTION
        // =========================================================

        public bool TryCreateConnection(
            SparkTerminal a,
            SparkTerminal b,
            SparkConnectionKind kind,
            SparkConnectionDirection direction,
            out SparkCircuitConnection connection)
        {
            connection = default;

            if (!ValidateConnectionRequest(
                    a,
                    b,
                    kind,
                    direction,
                    out string reason))
            {
                LogRejected(
                    a,
                    b,
                    reason);

                return false;
            }

            if (!allowParallelConnections &&
                HasConnectionBetween(
                    a,
                    b))
            {
                LogRejected(
                    a,
                    b,
                    "A connection already exists between these terminals.");

                return false;
            }

            if (!a.CanConnectTo(
                    b,
                    kind,
                    direction,
                    out reason))
            {
                LogRejected(
                    a,
                    b,
                    reason);

                return false;
            }

            if (!b.CanAccept(
                    kind,
                    out reason))
            {
                LogRejected(
                    a,
                    b,
                    reason);

                return false;
            }

/*            Debug.Log(
    $"[CONNECT REQUEST] " +
    $"A={GetTerminalDebugName(a)} | " +
    $"B={GetTerminalDebugName(b)} | " +
    $"Kind={kind} | Direction={direction}");*/

            ulong id =
                AllocateConnectionId();

            bool registeredA =
                a.RegisterConnection(
                    kind,
                    out string registerReason);

            if (!registeredA)
            {
                LogRejected(
                    a,
                    b,
                    registerReason);

                return false;
            }

            bool registeredB =
                b.RegisterConnection(
                    kind,
                    out registerReason);

                   /* Debug.Log(
    $"[CONNECT REGISTER RESULT] " +
    $"A={GetTerminalDebugName(a)} | " +
    $"B={GetTerminalDebugName(b)} | " +
    $"RegisteredA={registeredA} | " +
    $"RegisteredB={registeredB}");*/

            if (!registeredB)
            {
                a.UnregisterConnection(
                    kind);

                LogRejected(
                    a,
                    b,
                    registerReason);

                return false;
            }

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

            AddTerminalConnection(
                a,
                id);

            AddTerminalConnection(
                b,
                id);

            MarkTopologyChanged();

            ConnectionCreated?.Invoke(
                connection);

            NotifyElectricalFlowVisualsChanged();

            if (logConnectionChanges)
            {
                Debug.Log(
                    $"[SPARK CIRCUIT] Connection created: " +
                    $"{connection}",
                    this);
            }

            return true;
        }

        // =========================================================
        // REMOVE CONNECTION
        // =========================================================

        public bool TryRemoveConnection(
            ulong id,
            out SparkCircuitConnection connection)
        {
            if (!connections.TryGetValue(
                    id,
                    out connection))
            {
                return false;
            }

            connections.Remove(id);

            UnregisterTerminal(
                connection.A);

            UnregisterTerminal(
                connection.B);

            RemoveTerminalConnection(
                connection.A,
                id);

            RemoveTerminalConnection(
                connection.B,
                id);

            MarkTopologyChanged();

            ConnectionRemoved?.Invoke(
                connection);

            NotifyElectricalFlowVisualsChanged();

            if (logConnectionChanges)
            {
                Debug.Log(
                    $"[SPARK CIRCUIT] Connection removed: " +
                    $"{connection}",
                    this);
            }

            return true;
        }

        // =========================================================
        // REMOVE ALL
        // =========================================================

        public int ClearConnections()
        {
            return ClearConnections(true);
        }

        private int ClearConnections(
            bool notify)
        {
            if (connections.Count == 0)
            {
                return 0;
            }

            connectionBuffer.Clear();

            foreach (
                SparkCircuitConnection connection
                in connections.Values)
            {
                connectionBuffer.Add(
                    connection);
            }

            int removedCount =
                connectionBuffer.Count;

            for (int i = 0;
                 i < connectionBuffer.Count;
                 i++)
            {
                SparkCircuitConnection connection =
                    connectionBuffer[i];

                UnregisterTerminal(
                    connection.A);

                UnregisterTerminal(
                    connection.B);

                if (notify)
                {
                    ConnectionRemoved?.Invoke(
                        connection);
                }
            }

            connections.Clear();
            terminalConnections.Clear();

            MarkTopologyChanged();

            if (notify)
            {
                NotifyElectricalFlowVisualsChanged();
            }

            return removedCount;
        }

        // =========================================================
        // LOOKUP
        // =========================================================

        public bool HasConnection(
            ulong id)
        {
            return connections.ContainsKey(id);
        }

        public bool TryGetConnection(
            ulong id,
            out SparkCircuitConnection connection)
        {
            return connections.TryGetValue(
                id,
                out connection);
        }

        public bool HasConnectionBetween(
            SparkTerminal a,
            SparkTerminal b)
        {
            if (a == null ||
                b == null)
            {
                return false;
            }

            if (!terminalConnections.TryGetValue(
                    a,
                    out List<ulong> ids))
            {
                return false;
            }

            for (int i = 0;
                 i < ids.Count;
                 i++)
            {
                ulong id =
                    ids[i];

                if (!connections.TryGetValue(
                        id,
                        out SparkCircuitConnection connection))
                {
                    continue;
                }

                if (connection.Connects(a, b))
                {
                    return true;
                }
            }

            return false;
        }

        // =========================================================
        // TERMINAL CONNECTION QUERY
        // =========================================================

        public int GetConnectionCount(
            SparkTerminal terminal)
        {
            if (terminal == null)
            {
                return 0;
            }

            if (!terminalConnections.TryGetValue(
                    terminal,
                    out List<ulong> ids))
            {
                return 0;
            }

            return ids.Count;
        }

        public void GetConnections(
            SparkTerminal terminal,
            List<SparkCircuitConnection> results)
        {
            if (results == null)
            {
                throw new ArgumentNullException(
                    nameof(results));
            }

            results.Clear();

            if (terminal == null)
            {
                return;
            }

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
                ulong id =
                    ids[i];

                if (!connections.TryGetValue(
                        id,
                        out SparkCircuitConnection connection))
                {
                    continue;
                }

                if (!connection.IsValid)
                {
                    continue;
                }

                results.Add(
                    connection);
            }
        }

        public int CopyConnections(
            List<SparkCircuitConnection> results)
        {
            if (results == null)
            {
                throw new ArgumentNullException(
                    nameof(results));
            }

            results.Clear();

            foreach (
                SparkCircuitConnection connection
                in connections.Values)
            {
                if (!connection.IsValid)
                {
                    continue;
                }

                results.Add(
                    connection);
            }

            return results.Count;
        }

        // =========================================================
        // TOPOLOGY
        // =========================================================

        public void RebuildTopology()
        {
            if (rebuilding)
                return;

            if (!topologyDirty)
                return;

            rebuilding = true;

            try
            {
                terminalConnections.Clear();

                foreach (
                    SparkCircuitConnection connection
                    in connections.Values)
                {
                    if (!connection.IsValid)
                        continue;

                    AddTerminalConnection(
                        connection.A,
                        connection.Id);

                    AddTerminalConnection(
                        connection.B,
                        connection.Id);
                }

                topologyDirty = false;
            }
            finally
            {
                rebuilding = false;
            }
        }

        public void MarkTopologyDirty()
        {
            MarkTopologyChanged();
        }

        // =========================================================
        // ELECTRICAL FLOW VISUAL NOTIFICATION
        // =========================================================

        /// <summary>
        /// Called by the electrical solver after a new electrical
        /// solution has been calculated.
        ///
        /// This does NOT calculate electricity.
        /// It only tells visual systems that their materials/state
        /// may need refreshing.
        /// </summary>
        public void NotifyElectricalFlowVisualsChanged()
        {
            ElectricalFlowVisualsChanged?.Invoke();
        }

        // =========================================================
        // VISUAL STATE CLASSIFICATION
        // =========================================================

        public enum ElectricFlowVisualState
        {
            None,
            Connected,
            PositiveVoltage,
            NegativeVoltage,
            PositiveCurrent,
            NegativeCurrent,
            Short
        }

        /// <summary>
        /// Converts a solved voltage/current state into a visual state.
        ///
        /// Voltage and current are supplied by the electrical solver.
        /// </summary>
        public ElectricFlowVisualState GetElectricFlowVisualState(
            float voltage,
            float current,
            bool shortCircuit)
        {
            if (shortCircuit)
            {
                return ElectricFlowVisualState.Short;
            }

            bool hasVoltage =
                Mathf.Abs(voltage) >=
                voltageVisualThreshold;

            bool hasCurrent =
                Mathf.Abs(current) >=
                currentVisualThreshold;

            if (hasCurrent)
            {
                if (current > 0f)
                {
                    return ElectricFlowVisualState.PositiveCurrent;
                }

                return ElectricFlowVisualState.NegativeCurrent;
            }

            if (hasVoltage)
            {
                if (voltage > 0f)
                {
                    return ElectricFlowVisualState.PositiveVoltage;
                }

                return ElectricFlowVisualState.NegativeVoltage;
            }

            return ElectricFlowVisualState.Connected;
        }

        /// <summary>
        /// Returns the material configured for an electrical
        /// visual state.
        /// </summary>
        public Material GetMaterialForFlowState(
            ElectricFlowVisualState state)
        {
            switch (state)
            {
                case ElectricFlowVisualState.PositiveVoltage:
                    return positiveVoltageMaterial;

                case ElectricFlowVisualState.NegativeVoltage:
                    return negativeVoltageMaterial;

                case ElectricFlowVisualState.PositiveCurrent:
                    return positiveCurrentFlowMaterial;

                case ElectricFlowVisualState.NegativeCurrent:
                    return negativeCurrentFlowMaterial;

                case ElectricFlowVisualState.Short:
                    return shortCircuitMaterial;

                case ElectricFlowVisualState.Connected:
                case ElectricFlowVisualState.None:
                default:
                    return neutralWireMaterial;
            }
        }

        // =========================================================
        // VALIDATION
        // =========================================================

        private bool ValidateConnectionRequest(
            SparkTerminal a,
            SparkTerminal b,
            SparkConnectionKind kind,
            SparkConnectionDirection direction,
            out string reason)
        {
            reason = null;

            if (a == null)
            {
                reason =
                    "Connection terminal A is missing.";

                return false;
            }

            if (b == null)
            {
                reason =
                    "Connection terminal B is missing.";

                return false;
            }

            if (a == b)
            {
                reason =
                    "A terminal cannot connect to itself.";

                return false;
            }

            SparkElectronicObject ownerA =
                a.Owner;

            SparkElectronicObject ownerB =
                b.Owner;

            if (ownerA == null)
            {
                reason =
                    $"Terminal '{a.name}' has no owner.";

                return false;
            }

            if (ownerB == null)
            {
                reason =
                    $"Terminal '{b.name}' has no owner.";

                return false;
            }

            if (!a.isActiveAndEnabled)
            {
                reason =
                    $"Terminal '{a.name}' is disabled.";

                return false;
            }

            if (!b.isActiveAndEnabled)
            {
                reason =
                    $"Terminal '{b.name}' is disabled.";

                return false;
            }

            if (!ownerA.isActiveAndEnabled)
            {
                reason =
                    $"Owner '{ownerA.name}' is disabled.";

                return false;
            }

            if (!ownerB.isActiveAndEnabled)
            {
                reason =
                    $"Owner '{ownerB.name}' is disabled.";

                return false;
            }

            if (ownerA.OperationalState ==
                SparkOperationalState.Disabled)
            {
                reason =
                    $"Owner '{ownerA.name}' is operationally disabled.";

                return false;
            }

            if (ownerB.OperationalState ==
                SparkOperationalState.Disabled)
            {
                reason =
                    $"Owner '{ownerB.name}' is operationally disabled.";

                return false;
            }

            return true;
        }

        // =========================================================
        // TERMINAL REGISTRATION
        // =========================================================

        private void AddTerminalConnection(
            SparkTerminal terminal,
            ulong connectionId)
        {
            if (terminal == null)
            {
                return;
            }

            if (!terminalConnections.TryGetValue(
                    terminal,
                    out List<ulong> ids))
            {
                ids =
                    new List<ulong>(4);

                terminalConnections.Add(
                    terminal,
                    ids);
            }

            if (!ids.Contains(connectionId))
            {
                ids.Add(
                    connectionId);
            }
        }

        private void RemoveTerminalConnection(
            SparkTerminal terminal,
            ulong connectionId)
        {
            if (terminal == null)
            {
                return;
            }

            if (!terminalConnections.TryGetValue(
                    terminal,
                    out List<ulong> ids))
            {
                return;
            }

            ids.Remove(
                connectionId);

            if (ids.Count == 0)
            {
                terminalConnections.Remove(
                    terminal);
            }
        }

        private static void UnregisterTerminal(
            SparkTerminal terminal)
        {
            if (terminal == null)
            {
                return;
            }

            terminal.UnregisterConnection();
        }

        // =========================================================
        // ID GENERATION
        // =========================================================

        private ulong AllocateConnectionId()
        {
            if (nextConnectionId == 0UL)
            {
                nextConnectionId = 1UL;
            }

            ulong id =
                nextConnectionId++;

            while (id == 0UL ||
                   connections.ContainsKey(id))
            {
                if (nextConnectionId == 0UL)
                {
                    nextConnectionId = 1UL;
                }

                id =
                    nextConnectionId++;
            }

            return id;
        }

        // =========================================================
        // TOPOLOGY CHANGE
        // =========================================================

        private void MarkTopologyChanged()
        {
            topologyDirty = true;

            topologyVersion++;

            TopologyChanged?.Invoke();
        }

        // =========================================================
        // INVALID CONNECTION CLEANUP
        // =========================================================

        public int RemoveInvalidConnections()
        {
            connectionIdBuffer.Clear();

            foreach (
                KeyValuePair<
                    ulong,
                    SparkCircuitConnection> pair
                in connections)
            {
                SparkCircuitConnection connection =
                    pair.Value;

                if (!connection.IsValid)
                {
                    connectionIdBuffer.Add(
                        pair.Key);

                    continue;
                }

                if (!IsOwnerValid(
                        connection.A) ||
                    !IsOwnerValid(
                        connection.B))
                {
                    connectionIdBuffer.Add(
                        pair.Key);
                }
            }

            int removed = 0;

            for (int i = 0;
                 i < connectionIdBuffer.Count;
                 i++)
            {
                if (TryRemoveConnection(
                        connectionIdBuffer[i],
                        out _))
                {
                    removed++;
                }
            }

            return removed;
        }

        private static bool IsOwnerValid(
            SparkTerminal terminal)
        {
            if (terminal == null)
            {
                return false;
            }

            SparkElectronicObject owner =
                terminal.Owner;

            if (owner == null)
            {
                return false;
            }

            return owner.isActiveAndEnabled;
        }

        // =========================================================
        // DIAGNOSTICS
        // =========================================================

        private void LogRejected(
            SparkTerminal a,
            SparkTerminal b,
            string reason)
        {
            if (!logRejectedConnections)
            {
                return;
            }

            Debug.LogWarning(
                $"[SPARK CIRCUIT] Connection rejected.\n" +
                $"A: {a?.name ?? "null"}\n" +
                $"B: {b?.name ?? "null"}\n" +
                $"Reason: {reason}",
                this);
        }

        // =========================================================
        // EDITOR VALIDATION
        // =========================================================

        private void OnValidate()
        {
            if (nextConnectionId == 0UL)
            {
                nextConnectionId = 1UL;
            }

            voltageVisualThreshold =
                Mathf.Max(
                    0f,
                    voltageVisualThreshold);

            currentVisualThreshold =
                Mathf.Max(
                    0f,
                    currentVisualThreshold);
        }

        // =========================================================
        // DIAGNOSTICS
        // =========================================================

        [ContextMenu("Diagnostics / Print All Connections")]
        private void PrintAllConnections()
        {
            Debug.Log(
                $"========== SPARK CIRCUIT CONNECTIONS ==========\n" +
                $"Count = {connections.Count}");

            foreach (
                KeyValuePair<
                    ulong,
                    SparkCircuitConnection> pair
                in connections)
            {
                SparkCircuitConnection connection =
                    pair.Value;

                if (connection == null)
                    continue;

                Debug.Log(
                    $"[CONNECTION] " +
                    $"ID={connection.Id} | " +
                    $"A={connection.A?.name ?? "NULL"} " +
                    $"({connection.A?.Owner?.name ?? "NO OWNER"}) | " +
                    $"B={connection.B?.name ?? "NULL"} " +
                    $"({connection.B?.Owner?.name ?? "NO OWNER"}) | " +
                    $"Kind={connection.Kind} | " +
                    $"Direction={connection.Direction}");
            }

            Debug.Log(
                $"========== END CONNECTIONS ==========");
        }

        [ContextMenu("Diagnostics / Print Connection Terminals")]
        private void PrintConnectionTerminals()
        {
            Debug.Log(
                "========== CONNECTION TERMINAL DETAILS ==========");

            foreach (
                KeyValuePair<
                    ulong,
                    SparkCircuitConnection> pair
                in connections)
            {
                SparkCircuitConnection connection =
                    pair.Value;

                if (connection == null)
                    continue;

                Debug.Log(
                    $"ID={connection.Id} | " +
                    $"A={connection.A?.name ?? "NULL"} | " +
                    $"A InstanceID=" +
                    $"{(connection.A != null ? connection.A.GetInstanceID() : 0)} | " +
                    $"A Owner={connection.A?.Owner?.name ?? "NULL"} | " +
                    $"B={connection.B?.name ?? "NULL"} | " +
                    $"B InstanceID=" +
                    $"{(connection.B != null ? connection.B.GetInstanceID() : 0)} | " +
                    $"B Owner={connection.B?.Owner?.name ?? "NULL"} | " +
                    $"Kind={connection.Kind}");
            }

            Debug.Log(
                "========== END CONNECTION TERMINAL DETAILS ==========");
        }
        private string GetTerminalDebugName(
    SparkTerminal terminal)
{
    if (terminal == null)
        return "NULL";

    string ownerName = "NoOwner";
    string ownerObjectName = "NoOwnerObject";

    if (terminal.Owner != null)
    {
        ownerName =
            terminal.Owner.GetType().Name;

        if (terminal.Owner is Component ownerComponent)
        {
            ownerObjectName =
                ownerComponent.gameObject.name;
        }
    }

    return
        $"{terminal.name}" +
        $" [ID={terminal.GetInstanceID()}]" +
        $" [Owner={ownerName}]" +
        $" [GO={ownerObjectName}]" +
        $" [TerminalGO={terminal.gameObject.name}]" +
        $" [Connections={terminal.ConnectionCount}/{terminal.MaxConnections}]";
}
    }
}