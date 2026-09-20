using System;
using UnityEngine;

namespace ProjectSpark.Gameplay
{
public readonly struct SparkTerminalElectricalState
{
    public float Voltage { get; }

    public float Current { get; }

    public float Power =>
        Voltage * Current;

    public bool IsPowered =>
        Mathf.Abs(Voltage) > 0.001f;

    public bool HasCurrent =>
        Mathf.Abs(Current) > 0.001f;

    public bool HasPower =>
        Mathf.Abs(Power) > 0.000001f;

    public SparkTerminalElectricalState(
        float voltage,
        float current)
    {
        Voltage = voltage;
        Current = current;
    }
}

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

          #region Runtime Electrical Polarity

[SerializeField, Min(0f)]
private float runtimePolarityThreshold = 0.05f;

[SerializeField]
private SparkTerminalPolarity runtimePolarity =
    SparkTerminalPolarity.None;

/// <summary>
/// Explicit runtime polarity assigned by a conductive component
/// such as a switch.
///
/// This is NOT the serialized/authored polarity.
/// </summary>
public SparkTerminalPolarity RuntimePolarity =>
    runtimePolarity;

/// <summary>
/// Polarity derived directly from the current solved voltage.
/// </summary>
public SparkTerminalPolarity SolvedPolarity
{
    get
    {
        if (electricalState.Voltage >
            runtimePolarityThreshold)
        {
            return SparkTerminalPolarity.Positive;
        }

        if (electricalState.Voltage <
            -runtimePolarityThreshold)
        {
            return SparkTerminalPolarity.Negative;
        }

        return SparkTerminalPolarity.None;
    }
}

/// <summary>
/// Sets the runtime polarity propagated by a component.
/// </summary>
public void SetRuntimePolarity(
    SparkTerminalPolarity newPolarity)
{
    if (runtimePolarity == newPolarity)
        return;

    runtimePolarity = newPolarity;

    ElectricalStateChanged?.Invoke(this);
}

/// <summary>
/// Clears explicitly propagated runtime polarity.
/// </summary>
public void ClearRuntimePolarity()
{
    SetRuntimePolarity(
        SparkTerminalPolarity.None);
}
/// <summary>
/// Final polarity used by runtime systems such as
/// wire visuals.
///
/// Priority:
/// Runtime → Solved → Authored
/// </summary>
public SparkTerminalPolarity EffectivePolarity
{
    get
    {
        if (runtimePolarity !=
            SparkTerminalPolarity.None)
        {
            return runtimePolarity;
        }

        SparkTerminalPolarity solved =
            SolvedPolarity;

        if (solved !=
            SparkTerminalPolarity.None)
        {
            return solved;
        }

        return polarity;
    }
}

#endregion


        [Header("Direction")]

        [SerializeField]
        private bool acceptsInput = true;

        [SerializeField]
        private bool providesOutput = true;


        [Header("Connections")]

        [SerializeField, Min(1)]
        private int maxConnections = 1;


        private int connectionCount;


        private SparkTerminalElectricalState
            electricalState;


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

        public int ActiveConnectionCount =>
            connectionCount;

        public bool AtCapacity =>
            connectionCount >= maxConnections;


        // =========================================================
        // ELECTRICAL STATE
        // =========================================================

        public SparkTerminalElectricalState ElectricalState =>
            electricalState;


        public float Voltage =>
            electricalState.Voltage;


        public float Current =>
            electricalState.Current;


        public bool IsPowered =>
            electricalState.IsPowered;


        public bool HasCurrent =>
            electricalState.HasCurrent;


        // =========================================================
        // OWNER STATE
        // =========================================================

        public bool IsOwnerActive =>
            owner != null &&
            owner.isActiveAndEnabled;


        public bool IsElectricalEnabled
        {
            get
            {
                if (owner == null)
                {
                    return false;
                }

                SparkElectricalComponent electrical =
                    owner as SparkElectricalComponent;

                if (electrical == null)
                {
                    return true;
                }

                return electrical.ElectricalEnabled;
            }
        }


        // =========================================================
        // EVENTS
        // =========================================================

        public event Action<SparkTerminal>
            ConnectionStateChanged;


        public event Action<SparkTerminal>
            ElectricalStateChanged;


        // =========================================================
        // UNITY
        // =========================================================

        private void Awake()
        {
            ResolveOwner();

            maxConnections =
                Mathf.Max(
                    1,
                    maxConnections);

            electricalState =
                new SparkTerminalElectricalState(
                    0f,
                    0f);
        }


        private void OnValidate()
        {
            ResolveOwner();

            maxConnections =
                Mathf.Max(
                    1,
                    maxConnections);
        }


        // =========================================================
        // ELECTRICAL STATE
        // =========================================================

       public void ApplyElectricalState(
    in SparkTerminalElectricalState state)
{
    if (float.IsNaN(state.Voltage) ||
        float.IsInfinity(state.Voltage) ||
        float.IsNaN(state.Current) ||
        float.IsInfinity(state.Current))
    {
        return;
    }

    bool changed =
        Mathf.Abs(
            electricalState.Voltage -
            state.Voltage) >
        0.000001f ||
        Mathf.Abs(
            electricalState.Current -
            state.Current) >
        0.000001f;

    electricalState = state;

    if (changed)
    {
        ElectricalStateChanged?.Invoke(this);
    }
}


        public void ClearElectricalState()
        {
            ApplyElectricalState(
                new SparkTerminalElectricalState(
                    0f,
                    0f));
        }


        // =========================================================
        // OWNER
        // =========================================================

        private void ResolveOwner()
        {
            if (owner == null)
            {
                owner =
                    GetComponentInParent<SparkElectronicObject>();
            }
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

            reason = null;

            return true;
        }


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
        // CONNECTION REGISTRATION
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

            ConnectionStateChanged?.Invoke(
                this);

            reason = null;

            return true;
        }


        public bool UnregisterConnection()
        {
            if (connectionCount <= 0)
            {
                return false;
            }

            connectionCount--;

            ConnectionStateChanged?.Invoke(
                this);

            return true;
        }
    }
}