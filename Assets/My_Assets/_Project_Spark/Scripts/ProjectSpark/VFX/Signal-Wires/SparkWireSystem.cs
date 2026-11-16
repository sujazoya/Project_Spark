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
        [Header("Circuit")]

        [SerializeField]
        private SparkCircuitSystem circuit;

        private readonly Dictionary<ulong, SparkWire> wires =
            new Dictionary<ulong, SparkWire>();


        // =========================================================
        // PUBLIC STATE
        // =========================================================

        public int WireCount =>
            wires.Count;


        // =========================================================
        // EVENTS
        // =========================================================

        public event Action<SparkWire> WireCreated;

        public event Action<SparkWire> WireRemoved;


        // =========================================================
        // UNITY
        // =========================================================

        private void Awake()
        {
            if (circuit == null)
            {
                circuit =
                    GetComponent<SparkCircuitSystem>();
            }

            if (circuit == null)
            {
                circuit =
                    GetComponentInParent<SparkCircuitSystem>();
            }

            if (circuit == null)
            {
                Debug.LogError(
                    "[SPARK WIRE] SparkCircuitSystem is not configured.",
                    this);
            }
        }


        // =========================================================
        // CONNECT
        // =========================================================

        public SparkResult TryConnect(
            SparkTerminal source,
            SparkTerminal target,
            in SparkInteractionContext context,
            out SparkWire wire)
        {
            wire = null;


            // -----------------------------------------------------
            // VALIDATE SOURCE
            // -----------------------------------------------------

            if (source == null)
            {
                return SparkResult.Invalid(
                    "Source terminal is missing.");
            }


            // -----------------------------------------------------
            // VALIDATE TARGET
            // -----------------------------------------------------

            if (target == null)
            {
                return SparkResult.Invalid(
                    "Target terminal is missing.");
            }


            // -----------------------------------------------------
            // SAME TERMINAL
            // -----------------------------------------------------

            if (source == target)
            {
                return SparkResult.Invalid(
                    "Source and target terminals cannot be the same.");
            }


            // -----------------------------------------------------
            // CIRCUIT SYSTEM
            // -----------------------------------------------------

            if (circuit == null)
            {
                return SparkResult.Unavailable(
                    "Circuit system is not configured.");
            }


            // -----------------------------------------------------
            // CREATE CIRCUIT CONNECTION
            // -----------------------------------------------------

            SparkCircuitConnection connection;

            bool created =
                circuit.TryCreateConnection(
                    source,
                    target,
                    SparkConnectionKind.Wire,
                    SparkConnectionDirection.Bidirectional,
                    out connection);


            // -----------------------------------------------------
            // CONNECTION FAILED
            // -----------------------------------------------------

            if (!created)
            {
                return SparkResult.Rejected(
                    $"Unable to connect '{source.name}' " +
                    $"to '{target.name}'.");
            }


            // -----------------------------------------------------
            // CREATE WIRE RECORD
            // -----------------------------------------------------

            wire =
                new SparkWire(
                    connection.Id,
                    source,
                    target);


            // -----------------------------------------------------
            // PROTECT AGAINST DUPLICATE ID
            // -----------------------------------------------------

            if (wires.ContainsKey(wire.Id))
            {
                SparkCircuitConnection rollbackConnection;

                circuit.TryRemoveConnection(
                    wire.Id,
                    out rollbackConnection);

                wire = null;

                return SparkResult.Rejected(
                    $"A wire with connection ID " +
                    $"'{connection.Id}' already exists.");
            }


            // -----------------------------------------------------
            // STORE
            // -----------------------------------------------------

            wires.Add(
                wire.Id,
                wire);


            // -----------------------------------------------------
            // EVENT
            // -----------------------------------------------------

            WireCreated?.Invoke(
                wire);


            // -----------------------------------------------------
            // SUCCESS
            // -----------------------------------------------------

            return SparkResult.Success();
        }


        // =========================================================
        // DISCONNECT
        // =========================================================

        public SparkResult TryDisconnect(
            ulong connectionId,
            in SparkInteractionContext context)
        {
            // -----------------------------------------------------
            // CIRCUIT SYSTEM
            // -----------------------------------------------------

            if (circuit == null)
            {
                return SparkResult.Unavailable(
                    "Circuit system is not configured.");
            }


            // -----------------------------------------------------
            // FIND WIRE
            // -----------------------------------------------------

            if (!wires.TryGetValue(
                    connectionId,
                    out SparkWire wire))
            {
                return SparkResult.Invalid(
                    "Wire not found.");
            }


            // -----------------------------------------------------
            // REMOVE CIRCUIT CONNECTION
            // -----------------------------------------------------

            SparkCircuitConnection connection;

            bool removed =
                circuit.TryRemoveConnection(
                    connectionId,
                    out connection);


            // -----------------------------------------------------
            // REMOVE FAILED
            // -----------------------------------------------------

            if (!removed)
            {
                return SparkResult.Rejected(
                    $"Unable to remove wire connection " +
                    $"'{connectionId}'.");
            }


            // -----------------------------------------------------
            // REMOVE WIRE RECORD
            // -----------------------------------------------------

            wires.Remove(
                connectionId);


            // -----------------------------------------------------
            // EVENT
            // -----------------------------------------------------

            WireRemoved?.Invoke(
                wire);


            // -----------------------------------------------------
            // SUCCESS
            // -----------------------------------------------------

            return SparkResult.Success();
        }


        // =========================================================
        // QUERY
        // =========================================================

        public bool ContainsWire(
            ulong connectionId)
        {
            return wires.ContainsKey(
                connectionId);
        }


        public bool TryGetWire(
            ulong connectionId,
            out SparkWire wire)
        {
            return wires.TryGetValue(
                connectionId,
                out wire);
        }


        // =========================================================
        // CLEAR
        // =========================================================

        public void ClearWires()
        {
            if (circuit != null)
            {
                List<ulong> connectionIds =
                    new List<ulong>(
                        wires.Keys);

                for (
                    int i = connectionIds.Count - 1;
                    i >= 0;
                    i--)
                {
                    circuit.TryRemoveConnection(
                        connectionIds[i],
                        out _);
                }
            }


            if (wires.Count > 0)
            {
                List<SparkWire> removedWires =
                    new List<SparkWire>(
                        wires.Values);

                wires.Clear();


                for (
                    int i = 0;
                    i < removedWires.Count;
                    i++)
                {
                    if (removedWires[i] != null)
                    {
                        WireRemoved?.Invoke(
                            removedWires[i]);
                    }
                }
            }
        }


        // =========================================================
        // DESTROY
        // =========================================================

        private void OnDestroy()
        {
            wires.Clear();
        }
    }


    // =============================================================
    // WIRE SYSTEM INTERFACE
    // =============================================================

    public interface ISparkWireSystem
    {
        SparkResult TryConnect(
            SparkTerminal source,
            SparkTerminal target,
            in SparkInteractionContext context,
            out SparkWire wire);


        SparkResult TryDisconnect(
            ulong connectionId,
            in SparkInteractionContext context);
    }


    // =============================================================
    // WIRE DATA
    // =============================================================

    public sealed class SparkWire
    {
        public ulong Id { get; }

        public SparkTerminal Source { get; }

        public SparkTerminal Target { get; }


        public SparkWire(
            ulong id,
            SparkTerminal source,
            SparkTerminal target)
        {
            Id = id;
            Source = source;
            Target = target;
        }
    }
}