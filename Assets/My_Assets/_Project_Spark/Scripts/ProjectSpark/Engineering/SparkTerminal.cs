using System;
using UnityEngine;

namespace ProjectSpark.Gameplay
{
    public enum SparkTerminalKind { Generic, Power, Ground, Signal, Measurement, Data }
    public enum SparkConnectionKind { Wire, Probe, Connector }
    public enum SparkConnectionDirection { Bidirectional, OutputToInput, InputToOutput }

    [DisallowMultipleComponent]
    public sealed class SparkTerminal : MonoBehaviour
    {
        [SerializeField] private SparkElectronicObject owner;
        [SerializeField] private SparkTerminalKind kind = SparkTerminalKind.Generic;
        [SerializeField] private bool acceptsInput = true;
        [SerializeField] private bool providesOutput = true;
        [SerializeField, Min(1)] private int maxConnections = 1;

        private int connectionCount;

        public SparkElectronicObject Owner => owner;
        public SparkTerminalKind Kind => kind;
        public bool AcceptsInput => acceptsInput;
        public bool ProvidesOutput => providesOutput;
        public int MaxConnections => maxConnections;
        public int ConnectionCount => connectionCount;
        // Kept as a compatibility alias for earlier code.
        public int ActiveConnectionCount => connectionCount;
        public bool AtCapacity => connectionCount >= maxConnections;
        public event Action<SparkTerminal> ConnectionStateChanged;

        private void Awake()
        {
            if (owner == null) owner = GetComponentInParent<SparkElectronicObject>();
            maxConnections = Mathf.Max(1, maxConnections);
        }

        private void OnValidate()
        {
            if (owner == null) owner = GetComponentInParent<SparkElectronicObject>();
            maxConnections = Mathf.Max(1, maxConnections);
        }

        public bool CanAccept(SparkConnectionKind connectionKind, out string reason)
        {
            if (owner == null || !owner.isActiveAndEnabled) { reason = "Terminal owner unavailable."; return false; }
            if (!owner.InteractionsEnabled) { reason = "Terminal owner disabled."; return false; }
            if (AtCapacity) { reason = "Terminal is at connection capacity."; return false; }
            if (connectionKind == SparkConnectionKind.Probe && kind != SparkTerminalKind.Measurement && kind != SparkTerminalKind.Signal)
            { reason = "Terminal does not accept a measurement probe."; return false; }
            reason = null;
            return true;
        }

        public bool CanConnectTo(SparkTerminal other, SparkConnectionKind connectionKind,
            SparkConnectionDirection direction, out string reason)
        {
            if (other == null) { reason = "Target terminal missing."; return false; }
            if (other == this) { reason = "A terminal cannot connect to itself."; return false; }
            if (!CanAccept(connectionKind, out reason)) return false;
            if (!other.CanAccept(connectionKind, out reason)) return false;
            if (!ValidateDirection(other, direction)) { reason = "Terminal direction is incompatible."; return false; }
            if (connectionKind == SparkConnectionKind.Wire &&
                kind == SparkTerminalKind.Ground && other.kind == SparkTerminalKind.Ground)
            { reason = "Ground-to-ground connection is invalid."; return false; }
            reason = null;
            return true;
        }

        private bool ValidateDirection(SparkTerminal other, SparkConnectionDirection direction)
        {
            return direction switch
            {
                SparkConnectionDirection.Bidirectional => true,
                SparkConnectionDirection.OutputToInput => providesOutput && other.acceptsInput,
                SparkConnectionDirection.InputToOutput => acceptsInput && other.providesOutput,
                _ => false
            };
        }

        public bool RegisterConnection(out string reason)
        {
            if (AtCapacity) { reason = "Terminal is at capacity."; return false; }
            connectionCount++;
            ConnectionStateChanged?.Invoke(this);
            reason = null;
            return true;
        }

        public bool UnregisterConnection()
        {
            if (connectionCount <= 0) return false;
            connectionCount--;
            ConnectionStateChanged?.Invoke(this);
            return true;
        }
    }
}
