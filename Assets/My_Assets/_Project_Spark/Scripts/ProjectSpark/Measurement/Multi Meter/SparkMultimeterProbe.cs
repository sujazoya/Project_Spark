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
        private ulong connectionId;

        public SparkTerminal ProbeTerminal => probeTerminal;
        public SparkTerminal ConnectedTerminal => connectedTerminal;

        public SparkMultimeterProbe OtherProbe => otherProbe;

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

    Debug.Log(
        $"[MULTIMETER PROBE] SETUP → " +
        $"ProbeTerminal={probeTerminal?.name ?? "NULL"} | " +
        $"Multimeter={multimeter?.name ?? "NULL"} | " +
        $"Circuit={circuit?.name ?? "NULL"}",
        this);
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
private void TryConnect(Collider other)
{
    if (probeTerminal == null)
        return;

    /*
     * ---------------------------------------------------------
     * PHYSICAL PROBE ↔ PROBE CONTACT
     * ---------------------------------------------------------
     */

    /*
 * ---------------------------------------------------------
 * PHYSICAL PROBE ↔ PROBE CONTACT
 * ---------------------------------------------------------
 */ 

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

        Debug.Log(
            $"[MULTIMETER PROBE] PROBE CONTACT → " +
            $"{probeColor} ↔ {otherProbe.probeColor}",
            this);

        return;
    }
}

    /*
     * ---------------------------------------------------------
     * NORMAL CIRCUIT TERMINAL CONNECTION
     * ---------------------------------------------------------
     */

    if (circuit == null)
        return;

    SparkTerminal target =
        other.GetComponentInParent<SparkTerminal>();

    if (target == null)
        return;

    if (target == probeTerminal)
        return;

    if (connectedTerminal != null)
    {
        if (connectedTerminal == target)
            return;

        Disconnect();
    }

    Debug.Log(
        $"[MULTIMETER PROBE] TARGET FOUND → " +
        $"Probe={probeTerminal.name} | " +
        $"Target={target.name}",
        this);

    bool canConnect =
        target.CanConnectTo(
            probeTerminal,
            SparkConnectionKind.Probe,
            SparkConnectionDirection.Bidirectional,
            out string reason);

    Debug.Log(
        $"[MULTIMETER PROBE] CanConnectTo → " +
        $"Result={canConnect} | " +
        $"Reason={reason ?? "NONE"}",
        this);

    if (!canConnect)
        return;

    bool created =
        circuit.TryCreateConnection(
            probeTerminal,
            target,
            SparkConnectionKind.Probe,
            SparkConnectionDirection.Bidirectional,
            out SparkCircuitConnection connection);

    if (!created || connection == null)
    {
        Debug.LogWarning(
            "[MULTIMETER PROBE] CONNECTION FAILED",
            this);

        return;
    }

    connectedTerminal = target;
    connectionId = connection.Id;

    ApplyProbeToMultimeter(target);

    Debug.Log(
        $"[MULTIMETER PROBE] CONNECTED → " +
        $"{probeTerminal.name} -> {target.name} | " +
        $"ID={connectionId}",
        this);
}
        private void TryDisconnect(Collider other)
{
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

            Debug.Log(
                $"[MULTIMETER PROBE] PROBE CONTACT LOST → " +
                $"{probeColor} ↔ {otherProbe.probeColor}",
                this);

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
            if (circuit != null &&
                connectionId != 0UL)
            {
                circuit.TryRemoveConnection(
                    connectionId,
                    out _);
            }

            Debug.Log(
                $"[MULTIMETER PROBE] DISCONNECTED → " +
                $"{probeTerminal?.name ?? "NULL"}",
                this);

            connectedTerminal = null;
            connectionId = 0UL;

            ClearProbeFromMultimeter();
        }

        private void ApplyProbeToMultimeter(
            SparkTerminal target)
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
    }
}