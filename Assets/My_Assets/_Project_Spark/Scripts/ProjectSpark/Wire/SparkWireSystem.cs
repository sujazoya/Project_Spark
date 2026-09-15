using System;
using System.Collections.Generic;
using ProjectSpark.Circuit;
using ProjectSpark.Gameplay;
using UnityEngine;

namespace ProjectSpark.Wire
{
    [DisallowMultipleComponent]
    public sealed class SparkWireSystem : MonoBehaviour, ISparkWireSystem
    {
        [SerializeField] private SparkCircuitSystem circuit;
        private readonly Dictionary<ulong, SparkWire> wires = new();
        public int WireCount => wires.Count;
        public event Action<SparkWire> WireCreated;
        public event Action<SparkWire> WireRemoved;

        private void Awake()
        {
            if (circuit == null) circuit = GetComponent<SparkCircuitSystem>();
            if (circuit == null) circuit = GetComponentInParent<SparkCircuitSystem>();
        }

        public SparkResult TryConnect(SparkTerminal source, SparkTerminal target, in SparkInteractionContext context, out SparkWire wire)
        {
            wire = null;
            if (circuit == null) return SparkResult.Unavailable("Circuit system is not configured.");
            var result = circuit.TryCreateConnection(source, target, SparkConnectionKind.Wire,
                SparkConnectionDirection.Bidirectional, out var connection);
            if (!result.Succeeded) return result;
            wire = new SparkWire(connection.Id, source, target);
            wires.Add(wire.Id, wire);
            WireCreated?.Invoke(wire);
            return result;
        }

        public SparkResult TryDisconnect(ulong connectionId, in SparkInteractionContext context)
        {
            if (circuit == null) return SparkResult.Unavailable("Circuit system is not configured.");
            if (!wires.TryGetValue(connectionId, out var wire)) return SparkResult.Invalid("Wire not found.");
            var result = circuit.TryRemoveConnection(connectionId, out _);
            if (!result.Succeeded) return result;
            wires.Remove(connectionId);
            WireRemoved?.Invoke(wire);
            return result;
        }
    }

    public interface ISparkWireSystem
    {
        SparkResult TryConnect(SparkTerminal source, SparkTerminal target, in SparkInteractionContext context, out SparkWire wire);
        SparkResult TryDisconnect(ulong connectionId, in SparkInteractionContext context);
    }

    public sealed class SparkWire
    {
        public ulong Id { get; }
        public SparkTerminal Source { get; }
        public SparkTerminal Target { get; }
        public SparkWire(ulong id, SparkTerminal source, SparkTerminal target) { Id = id; Source = source; Target = target; }
    }
}
