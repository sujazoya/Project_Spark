using System;
using UnityEngine;

namespace ProjectSpark.Gameplay
{
    // =========================================================
    // TERMINAL TYPES
    // =========================================================

    public enum SparkTerminalKind
    {
        Generic,
        Power,
        Ground,
        Signal,
        Measurement,
        Data
    }

    public enum SparkTerminalPolarity
    {
        None,
        Positive,
        Negative
    }

    public enum SparkConnectionKind
    {
        Wire,
        Probe,
        Connector
    }

    public enum SparkConnectionDirection
    {
        Bidirectional,
        OutputToInput,
        InputToOutput
    }


    // =========================================================
    // TERMINAL
    // =========================================================

    [DisallowMultipleComponent]
    public sealed class SparkTerminal : MonoBehaviour
    {
        [Header("Owner")]

        [SerializeField]
        private SparkElectronicObject owner;


        [Header("Terminal")]

        [SerializeField]
        private SparkTerminalKind kind =
            SparkTerminalKind.Generic;


        [Header("Electrical Polarity")]

        [SerializeField]
        private SparkTerminalPolarity polarity =
            SparkTerminalPolarity.None;


        [Header("Direction")]

        [SerializeField]
        private bool acceptsInput = true;

        [SerializeField]
        private bool providesOutput = true;


        [Header("Connections")]

        [SerializeField, Min(1)]
        private int maxConnections = 1;


        private int connectionCount;


        // =========================================================
        // PUBLIC STATE
        // =========================================================

        public SparkElectronicObject Owner =>
            owner;

        public SparkTerminalKind Kind =>
            kind;

        public SparkTerminalPolarity Polarity =>
            polarity;

        public bool AcceptsInput =>
            acceptsInput;

        public bool ProvidesOutput =>
            providesOutput;

        public int MaxConnections =>
            maxConnections;

        public int ConnectionCount =>
            connectionCount;

        // Compatibility alias.
        public int ActiveConnectionCount =>
            connectionCount;

        public bool AtCapacity =>
            connectionCount >= maxConnections;


        public event Action<SparkTerminal>
            ConnectionStateChanged;


        // =========================================================
        // UNITY
        // =========================================================

        private void Awake()
        {
            if (owner == null)
            {
                owner =
                    GetComponentInParent<SparkElectronicObject>();
            }

            maxConnections =
                Mathf.Max(1, maxConnections);
        }


        private void OnValidate()
        {
            if (owner == null)
            {
                owner =
                    GetComponentInParent<SparkElectronicObject>();
            }

            maxConnections =
                Mathf.Max(1, maxConnections);
        }


        // =========================================================
        // CONNECTION VALIDATION
        // =========================================================

        public bool CanAccept(
            SparkConnectionKind connectionKind,
            out string reason)
        {
            if (owner == null ||
                !owner.isActiveAndEnabled)
            {
                reason =
                    "Terminal owner unavailable.";

                return false;
            }

            if (!owner.InteractionsEnabled)
            {
                reason =
                    "Terminal owner disabled.";

                return false;
            }

            if (AtCapacity)
            {
                reason =
                    "Terminal is at connection capacity.";

                return false;
            }

            if (connectionKind ==
                SparkConnectionKind.Probe &&
                kind != SparkTerminalKind.Measurement &&
                kind != SparkTerminalKind.Signal)
            {
                reason =
                    "Terminal does not accept a measurement probe.";

                return false;
            }

            reason = null;
            return true;
        }


        // =========================================================
        // CONNECT TO TERMINAL
        // =========================================================

        public bool CanConnectTo(
            SparkTerminal other,
            SparkConnectionKind connectionKind,
            SparkConnectionDirection direction,
            out string reason)
        {
            if (other == null)
            {
                reason =
                    "Target terminal missing.";

                return false;
            }

            if (other == this)
            {
                reason =
                    "A terminal cannot connect to itself.";

                return false;
            }

            if (!CanAccept(
                    connectionKind,
                    out reason))
            {
                return false;
            }

            if (!other.CanAccept(
                    connectionKind,
                    out reason))
            {
                return false;
            }

            if (!ValidateDirection(
                    other,
                    direction))
            {
                reason =
                    "Terminal direction is incompatible.";

                return false;
            }

            if (connectionKind ==
                SparkConnectionKind.Wire &&
                kind == SparkTerminalKind.Ground &&
                other.kind == SparkTerminalKind.Ground)
            {
                reason =
                    "Ground-to-ground connection is invalid.";

                return false;
            }

            /*
             * IMPORTANT:
             *
             * Polarity is intentionally NOT validated here.
             *
             * A player must be allowed to make an incorrect
             * connection such as:
             *
             *       + → -
             *
             * so the level/circuit logic can detect it.
             *
             * The START terminal determines the wire polarity.
             */

            reason = null;
            return true;
        }


        // =========================================================
        // DIRECTION
        // =========================================================

        private bool ValidateDirection(
            SparkTerminal other,
            SparkConnectionDirection direction)
        {
            switch (direction)
            {
                case SparkConnectionDirection.Bidirectional:
                    return true;

                case SparkConnectionDirection.OutputToInput:
                    return
                        providesOutput &&
                        other.acceptsInput;

                case SparkConnectionDirection.InputToOutput:
                    return
                        acceptsInput &&
                        other.providesOutput;

                default:
                    return false;
            }
        }


        // =========================================================
        // REGISTER
        // =========================================================

        public bool RegisterConnection(
            out string reason)
        {
            if (AtCapacity)
            {
                reason =
                    "Terminal is at capacity.";

                return false;
            }

            connectionCount++;

            ConnectionStateChanged?.Invoke(this);

            reason = null;

            return true;
        }


        // =========================================================
        // UNREGISTER
        // =========================================================

        public bool UnregisterConnection()
        {
            if (connectionCount <= 0)
                return false;

            connectionCount--;

            ConnectionStateChanged?.Invoke(this);

            return true;
        }
    }
}