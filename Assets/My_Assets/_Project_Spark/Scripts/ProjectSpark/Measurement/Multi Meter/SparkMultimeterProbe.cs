using UnityEngine;
using ProjectSpark.Circuit;
using ProjectSpark.Gameplay;

namespace ProjectSpark.Measurement
{
    public enum SparkMultimeterProbeColor
    {
        Red,
        Black
    }

    [DisallowMultipleComponent]
    public sealed class SparkMultimeterProbe : MonoBehaviour
    {
        [Header("Probe")]
        [SerializeField]
        private SparkMultimeter multimeter;

        [SerializeField]
        private SparkTerminal probeTerminal;

        [Header("Physical Contact")]
        [SerializeField]
        private SparkMultimeterProbe otherProbe;

        [SerializeField]
        private SparkMultimeterProbeColor probeColor =
            SparkMultimeterProbeColor.Red;

        [Header("Circuit")]
        [SerializeField]
        private SparkCircuitSystem circuit;

        private SparkTerminal connectedTerminal;

        // Normal measurement-only Probe connection.
        private ulong connectionId;

        public SparkTerminal ProbeTerminal =>
            probeTerminal;

        public SparkTerminal ConnectedTerminal =>
            connectedTerminal;

        public SparkMultimeterProbe OtherProbe =>
            otherProbe;

        public SparkMultimeterProbeColor ProbeColor =>
            probeColor;

        private void Awake()
        {
            if (probeTerminal == null)
            {
                probeTerminal =
                    GetComponent<SparkTerminal>();
            }

            if (multimeter == null)
            {
                multimeter =
                    GetComponentInParent<SparkMultimeter>();
            }

            if (circuit == null)
            {
                circuit =
                    FindFirstObjectByType<SparkCircuitSystem>();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            TryConnect(other);
        }

        private void OnCollisionEnter(Collision collision)
        {
            TryConnect(collision.collider);
        }

        private void OnTriggerExit(Collider other)
        {
            TryDisconnect(other);
        }

        private void OnCollisionExit(Collision collision)
        {
            TryDisconnect(collision.collider);
        }

        // ============================================================
        // CONNECT
        // ============================================================

        private void TryConnect(Collider other)
        {
            if (probeTerminal == null)
                return;

            // ========================================================
            // PHYSICAL RED ↔ BLACK PROBE CONTACT
            // ========================================================

            if (otherProbe != null &&
                otherProbe != this &&
                otherProbe.multimeter == multimeter &&
                otherProbe.probeColor != probeColor)
            {
                Transform hitTransform = other.transform;

                bool belongsToOtherProbe =
                    hitTransform == otherProbe.transform ||
                    hitTransform.IsChildOf(otherProbe.transform);

                if (belongsToOtherProbe)
                {
                    multimeter?.SetProbeTipContact(true);
                    return;
                }
            }

            // ========================================================
            // FIND CIRCUIT TERMINAL
            // ========================================================

            if (circuit == null)
                return;

            SparkTerminal target =
                other.GetComponentInParent<SparkTerminal>();

            if (target == null)
                return;

            if (target == probeTerminal)
                return;

            if (connectedTerminal == target)
                return;

            if (connectedTerminal != null)
            {
                Disconnect();
            }

            connectedTerminal = target;

            // Tell multimeter which REAL circuit terminal
            // this probe is touching.
            if (multimeter != null)
            {
                multimeter.NotifyProbeConnected(
                    this,
                    target);
            }

            // ========================================================
            // NORMAL MEASUREMENT MODE
            // ========================================================
            //
            // Probe connection is measurement-only.
            //
            // CurrentDC uses the meter's actual electrical terminals
            // instead.
            // ========================================================

            if (multimeter == null ||
                !multimeter.IsCurrentMeasurementActive)
            {
                CreateMeasurementProbeConnection();
            }
        }

        // ============================================================
        // NORMAL PROBE CONNECTION
        // ============================================================


/*
        private void CreateMeasurementProbeConnection()
        {
            if (circuit == null ||
                probeTerminal == null ||
                connectedTerminal == null)
            {
                return;
            }

            bool canConnect =
                connectedTerminal.CanConnectTo(
                    probeTerminal,
                    SparkConnectionKind.Probe,
                    SparkConnectionDirection.Bidirectional,
                    out string reason);

            if (!canConnect)
            {
                Debug.LogWarning(
                    $"[MULTIMETER PROBE] CONNECTION REJECTED → " +
                    $"{probeTerminal.name} -> " +
                    $"{connectedTerminal.name} | " +
                    $"Reason={reason}",
                    this);

                return;
            }

            bool created =
                circuit.TryCreateConnection(
                    probeTerminal,
                    connectedTerminal,
                    SparkConnectionKind.Probe,
                    SparkConnectionDirection.Bidirectional,
                    out SparkCircuitConnection connection);

            if (!created ||
                connection == null)
            {
                Debug.LogWarning(
                    "[MULTIMETER PROBE] PROBE CONNECTION FAILED",
                    this);

                return;
            }

            connectionId =
                connection.Id;

            ApplyProbeToMultimeter();
        }*/
        private void CreateMeasurementProbeConnection()
{
    if (circuit == null ||
        probeTerminal == null ||
        connectedTerminal == null)
    {
        Debug.Log(
            $"[MULTIMETER PROBE] CREATE FAILED → " +
            $"Circuit={circuit != null} " +
            $"ProbeTerminal={probeTerminal?.name ?? "NULL"} " +
            $"ConnectedTerminal={connectedTerminal?.name ?? "NULL"}",
            this);

        return;
    }

    bool canConnect =
        connectedTerminal.CanConnectTo(
            probeTerminal,
            SparkConnectionKind.Probe,
            SparkConnectionDirection.Bidirectional,
            out string reason);

    Debug.Log(
        $"[MULTIMETER PROBE] CAN CONNECT → " +
        $"Probe={probeTerminal.name} " +
        $"Target={connectedTerminal.name} " +
        $"Result={canConnect} " +
        $"Reason={reason ?? "NONE"}",
        this);

    if (!canConnect)
    {
        return;
    }

    bool created =
        circuit.TryCreateConnection(
            probeTerminal,
            connectedTerminal,
            SparkConnectionKind.Probe,
            SparkConnectionDirection.Bidirectional,
            out SparkCircuitConnection connection);

/*    Debug.Log(
        $"[MULTIMETER PROBE] CREATE RESULT → " +
        $"Created={created} " +
        $"Connection={(connection != null ? connection.ToString() : "NULL")}",
        this);*/

    if (!created ||
        connection == null)
    {
        return;
    }

    connectionId =
        connection.Id;

    ApplyProbeToMultimeter();
}

        // ============================================================
        // DISCONNECT
        // ============================================================

        private void TryDisconnect(Collider other)
        {
            // Physical probe contact.
            if (otherProbe != null &&
                otherProbe != this &&
                otherProbe.multimeter == multimeter &&
                otherProbe.probeColor != probeColor)
            {
                Transform hitTransform = other.transform;

                bool belongsToOtherProbe =
                    hitTransform == otherProbe.transform ||
                    hitTransform.IsChildOf(otherProbe.transform);

                if (belongsToOtherProbe)
                {
                    multimeter?.SetProbeTipContact(false);
                    return;
                }
            }

            if (connectedTerminal == null)
                return;

            SparkTerminal target =
                other.GetComponentInParent<SparkTerminal>();

            if (target != connectedTerminal)
                return;

            Disconnect();
        }

        private void Disconnect()
        {
            RemoveMeasurementProbeConnection();

            if (multimeter != null)
            {
                multimeter.NotifyProbeDisconnected(
                    this,
                    connectedTerminal);
            }

            connectedTerminal = null;

            ClearProbeFromMultimeter();
        }

        // ============================================================
        // NORMAL PROBE CONNECTION REMOVE
        // ============================================================

        private void RemoveMeasurementProbeConnection()
        {
            if (circuit != null &&
                connectionId != 0UL)
            {
                circuit.TryRemoveConnection(
                    connectionId,
                    out _);
            }

            connectionId = 0UL;
        }

        // ============================================================
        // MULTIMETER PROBE STATE
        // ============================================================

        private void ApplyProbeToMultimeter()
        {
            if (multimeter == null)
                return;

            if (probeColor ==
                SparkMultimeterProbeColor.Red)
            {
                multimeter.SetRedProbe(
                    probeTerminal);
            }
            else
            {
                multimeter.SetBlackProbe(
                    probeTerminal);
            }
        }

        private void ClearProbeFromMultimeter()
        {
            if (multimeter == null)
                return;

            if (probeColor ==
                SparkMultimeterProbeColor.Red)
            {
                multimeter.SetRedProbe(null);
            }
            else
            {
                multimeter.SetBlackProbe(null);
            }
        }

        // ============================================================
        // CURRENT MODE
        // ============================================================

        public void EnterCurrentMeasurementMode()
        {
            if (connectedTerminal == null)
                return;

            // Remove measurement-only Probe connection.
            //
            // The actual electrical path will be:
            //
            // Circuit -> Meter Red -> Shunt -> Meter Black -> Circuit
            //
            RemoveMeasurementProbeConnection();
        }

        public void ExitCurrentMeasurementMode()
        {
            if (connectedTerminal == null)
                return;

            // Restore normal measurement-only connection.
            if (connectionId == 0UL)
            {
                CreateMeasurementProbeConnection();
            }
        }
    }
}