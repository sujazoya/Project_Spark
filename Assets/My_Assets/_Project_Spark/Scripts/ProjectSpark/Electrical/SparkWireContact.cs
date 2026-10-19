using System.Collections.Generic;
using UnityEngine;

namespace ProjectSpark.Gameplay
{
    /// <summary>
    /// Physical electrical contact detector for SparkSingleWire.
    ///
    /// No mouse input.
    /// No raycast.
    /// No armed state.
    ///
    /// Physical collider contact determines electrical endpoints.
    ///
    /// First external terminal  -> Wire Terminal A
    /// Second external terminal -> Wire Terminal B
    ///
    /// Multiple colliders belonging to the same SparkTerminal
    /// are treated as ONE physical terminal contact.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkWireContact : MonoBehaviour
    {
        // ============================================================
        // REFERENCES
        // ============================================================

        [Header("Wire")]
        [SerializeField]
        private SparkSingleWire wire;

        [Header("Contact Collider")]
        [SerializeField]
        private Collider contactCollider;

        [Header("Detection")]
        [SerializeField]
        private LayerMask detectionLayers = ~0;


        // ============================================================
        // RUNTIME CONTACT TRACKING
        // ============================================================

        /*
         * One SparkTerminal may have several colliders.
         *
         * Example:
         *
         * Anode
         *   ├── Collider_1
         *   ├── Collider_2
         *   └── Collider_3
         *
         * We must not disconnect Anode until ALL of its
         * colliders have left the wire contact.
         */
        private readonly Dictionary<
            SparkTerminal,
            HashSet<Collider>>
            touchingTerminalColliders =
                new Dictionary<
                    SparkTerminal,
                    HashSet<Collider>>();


        // ============================================================
        // PUBLIC
        // ============================================================

        public SparkSingleWire Wire =>
            wire;

        public Collider ContactCollider =>
            contactCollider;

        public int TouchingTerminalCount =>
            touchingTerminalColliders.Count;


        // ============================================================
        // UNITY
        // ============================================================

        private void Awake()
        {
            if (wire == null)
            {
                wire =
                    GetComponentInParent<SparkSingleWire>();
            }

            if (contactCollider == null)
            {
                contactCollider =
                    GetComponent<Collider>();
            }

            if (wire == null)
            {
                Debug.LogError(
                    $"[SparkWireContact] " +
                    $"No SparkSingleWire found in parent hierarchy of {name}.",
                    this);
            }

            if (contactCollider == null)
            {
                Debug.LogError(
                    $"[SparkWireContact] " +
                    $"No Collider found on {name}.",
                    this);
            }
        }


        // ============================================================
        // TRIGGER ENTER
        // ============================================================

        private void OnTriggerEnter(
            Collider other)
        {
            TryConnectCollider(other);
        }


        // ============================================================
        // TRIGGER STAY
        // ============================================================

        private void OnTriggerStay(
            Collider other)
        {
            TryConnectCollider(other);
        }


        // ============================================================
        // TRIGGER EXIT
        // ============================================================

        private void OnTriggerExit(
            Collider other)
        {
            TryDisconnectCollider(other);
        }


        // ============================================================
        // CONNECT COLLIDER
        // ============================================================

       private void TryConnectCollider(Collider other)
{
    if (wire == null || other == null)
        return;

    if (contactCollider != null &&
        other == contactCollider)
    {
        return;
    }

    if (!IsLayerAllowed(other.gameObject.layer))
        return;

    SparkTerminal terminal =
        other.GetComponentInParent<SparkTerminal>();

    if (terminal == null)
        return;

    if (terminal == wire.TerminalA ||
        terminal == wire.TerminalB)
    {
        return;
    }

    if (!touchingTerminalColliders.TryGetValue(
        terminal,
        out HashSet<Collider> colliders))
    {
        colliders = new HashSet<Collider>();

        touchingTerminalColliders.Add(
            terminal,
            colliders);
    }

    // Always register the physical collider.
    colliders.Add(other);

    // Check the connection owned by THIS physical end.
    bool isContactA = wire.ContactA == this;
    bool isContactB = wire.ContactB == this;

    if (!isContactA && !isContactB)
        return;

    SparkTerminal currentTarget =
        isContactA
            ? wire.ConnectedTerminalA
            : wire.ConnectedTerminalB;

    bool hasConnection =
        isContactA
            ? wire.HasTerminalAConnection
            : wire.HasTerminalBConnection;

    // Already connected to this exact terminal.
    if (hasConnection && currentTarget == terminal)
        return;

    // This physical end is occupied by another terminal.
    if (hasConnection)
        return;

    // IMPORTANT:
    // Retry the electrical connection even if this collider
    // was registered during a previous physics callback.
    //
    // This allows rejected contacts to connect later when
    // terminal capacity becomes available.
    bool connected =
        wire.AddPhysicalContact(
            terminal,
            this);

    if (!connected)
        return;

#if UNITY_EDITOR || DEVELOPMENT_BUILD

    Debug.Log(
        $"[WIRE CONTACT] {wire.name} | " +
        $"Source={name} | " +
        $"External={terminal.name} | " +
        $"End={(isContactA ? "A" : "B")} | " +
        $"FullyConnected={wire.IsFullyConnected}",
        this);

#endif
}
        // ============================================================
        // DISCONNECT COLLIDER
        // ============================================================

private void TryDisconnectCollider(Collider other)
{
    if (wire == null || other == null)
        return;

    SparkTerminal terminal =
        other.GetComponentInParent<SparkTerminal>();

    if (terminal == null)
        return;

    if (!touchingTerminalColliders.TryGetValue(
        terminal,
        out HashSet<Collider> colliders))
    {
        return;
    }

    // Ignore exits for colliders we never registered.
    if (!colliders.Remove(other))
        return;

    // Another collider belonging to the same terminal
    // is still physically touching this wire end.
    if (colliders.Count > 0)
        return;

    touchingTerminalColliders.Remove(terminal);

    // Only disconnect the electrical connection owned
    // by this specific SparkWireContact.
    wire.RemovePhysicalContact(
        terminal,
        this);

#if UNITY_EDITOR || DEVELOPMENT_BUILD

    Debug.Log(
        $"[WIRE CONTACT EXIT] {wire.name} | " +
        $"Source={name} | " +
        $"External={terminal.name}",
        this);

#endif
}


        // ============================================================
        // DISABLE CLEANUP
        // ============================================================

        private void OnDisable()
        {
            DisconnectAll();
        }


       private void DisconnectAll()
{
    if (touchingTerminalColliders.Count == 0)
        return;

    SparkTerminal[] terminals =
        new SparkTerminal[
            touchingTerminalColliders.Count];

    touchingTerminalColliders.Keys.CopyTo(
        terminals,
        0);

    touchingTerminalColliders.Clear();

    if (wire == null)
        return;

    for (int i = 0; i < terminals.Length; i++)
    {
        SparkTerminal terminal = terminals[i];

        if (terminal == null)
            continue;

        wire.RemovePhysicalContact(
            terminal,
            this);
    }
}


        // ============================================================
        // LAYER
        // ============================================================

        private bool IsLayerAllowed(
            int layer)
        {
            return
                (detectionLayers.value &
                 (1 << layer)) != 0;
        }


        // ============================================================
        // SETTERS
        // ============================================================

        public void SetWire(
            SparkSingleWire value)
        {
            wire = value;
        }

        public void SetCollider(
            Collider value)
        {
            contactCollider = value;
        }
    }
}