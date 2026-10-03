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

        private void TryConnectCollider(
            Collider other)
        {
            if (wire == null)
                return;

            if (other == null)
                return;

            if (contactCollider != null &&
                other == contactCollider)
            {
                return;
            }

            if (!IsLayerAllowed(
                    other.gameObject.layer))
            {
                return;
            }

            SparkTerminal terminal =
                other.GetComponentInParent<SparkTerminal>();

            if (terminal == null)
                return;

            // Never connect the wire to its own
            // internal terminals.
            if (terminal == wire.TerminalA ||
                terminal == wire.TerminalB)
            {
                return;
            }


            // --------------------------------------------------------
            // Get/create collider set for this terminal
            // --------------------------------------------------------

            if (!touchingTerminalColliders.TryGetValue(
                    terminal,
                    out HashSet<Collider> colliders))
            {
                colliders =
                    new HashSet<Collider>();

                touchingTerminalColliders.Add(
                    terminal,
                    colliders);
            }


            // --------------------------------------------------------
            // This exact collider is already registered.
            // --------------------------------------------------------

            if (colliders.Contains(other))
            {
                return;
            }


            // --------------------------------------------------------
            // Register physical overlap FIRST.
            //
            // This is important because OnTriggerStay may run before
            // another collider belonging to the same terminal exits.
            // --------------------------------------------------------

            colliders.Add(other);


            // --------------------------------------------------------
            // If this terminal is already electrically connected,
            // we only need to remember the new collider.
            // --------------------------------------------------------

            if (wire.ConnectedTerminalA == terminal ||
                wire.ConnectedTerminalB == terminal)
            {
                return;
            }


            // --------------------------------------------------------
            // Ask the wire to occupy its next endpoint.
            // --------------------------------------------------------

            bool connected =
                wire.AddPhysicalContact(
                    terminal,
                    this);

            if (!connected)
            {
                /*
                 * Important:
                 *
                 * Keep the physical collider registered even when
                 * electrical connection was rejected.
                 *
                 * This prevents OnTriggerStay from repeatedly trying
                 * the same collider every physics frame.
                 */
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.Log(
                    $"[SparkWireContact] " +
                    $"CONTACT REJECTED | " +
                    $"Wire={wire.name} | " +
                    $"Target={terminal.name}",
                    this);
#endif

                return;
            }


#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log(
                $"[SparkWireContact] " +
                $"CONNECTED | " +
                $"Wire={wire.name} | " +
                $"Target={terminal.name} | " +
                $"A={wire.HasTerminalAConnection} | " +
                $"B={wire.HasTerminalBConnection} | " +
                $"Fully={wire.IsFullyConnected}",
                this);
#endif
        }


        // ============================================================
        // DISCONNECT COLLIDER
        // ============================================================

        private void TryDisconnectCollider(
            Collider other)
        {
            if (wire == null)
                return;

            if (other == null)
                return;

            SparkTerminal terminal =
                other.GetComponentInParent<SparkTerminal>();

            if (terminal == null)
                return;


            // --------------------------------------------------------
            // Find the collider set belonging to this terminal.
            // --------------------------------------------------------

            if (!touchingTerminalColliders.TryGetValue(
                    terminal,
                    out HashSet<Collider> colliders))
            {
                return;
            }


            // --------------------------------------------------------
            // Remove ONLY this collider.
            // --------------------------------------------------------

            colliders.Remove(other);


            // --------------------------------------------------------
            // Other colliders of the SAME terminal are still touching.
            //
            // Therefore the electrical connection must remain.
            // --------------------------------------------------------

            if (colliders.Count > 0)
            {
                return;
            }


            // --------------------------------------------------------
            // No colliders remain.
            //
            // Now the terminal has physically left the wire.
            // --------------------------------------------------------

            touchingTerminalColliders.Remove(
                terminal);

            wire.RemovePhysicalContact(
                terminal);


#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log(
                $"[SparkWireContact] " +
                $"DISCONNECTED | " +
                $"Wire={wire.name} | " +
                $"Target={terminal.name} | " +
                $"A={wire.HasTerminalAConnection} | " +
                $"B={wire.HasTerminalBConnection} | " +
                $"Fully={wire.IsFullyConnected}",
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


            for (int i = 0;
                 i < terminals.Length;
                 i++)
            {
                SparkTerminal terminal =
                    terminals[i];

                if (terminal == null)
                    continue;

                wire.RemovePhysicalContact(
                    terminal);
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