using UnityEngine;
using UnityEngine.Events;
using ProjectSpark.Circuit;

namespace ProjectSpark.Gameplay
{
    /// <summary>
    /// Physical 3D plug definition for Project Spark.
    ///
    /// Responsibilities:
    /// - Identifies the physical plug type.
    /// - Provides the plug's main transform.
    /// - Stores physical plug configuration.
    /// - Tracks whether the plug is currently inserted into a jack.
    /// - Provides physical insertion/release events.
    /// - Provides the electrical terminal pair carried by this plug.
    /// - Optionally forms a permanent two-conductor electrical link
    ///   to another SparkPlug.
    ///
    /// Physical movement is handled by the Tools System.
    /// Physical socket interaction is handled by SparkJackSocket.
    /// Electrical connectivity is handled by the Terminal/Circuit systems.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkPlug : MonoBehaviour
    {
        // =====================================================================
        // EVENTS
        // =====================================================================

        [Header("Events")]

        [Tooltip("Invoked when this plug is physically inserted into a jack.")]
        [SerializeField]
        private UnityEvent<SparkJackSocket> inserted =
            new UnityEvent<SparkJackSocket>();

        [Tooltip("Invoked when this plug is physically removed from a jack.")]
        [SerializeField]
        private UnityEvent<SparkJackSocket> removed =
            new UnityEvent<SparkJackSocket>();


        // =====================================================================
        // PLUG
        // =====================================================================

        [Header("Plug")]

        [Tooltip(
            "Physical plug type. " +
            "Must match the socket type for insertion.")]
        [SerializeField]
        private string plugType = "USB-C";

        [Tooltip(
            "Display name used by gameplay/UI systems. " +
            "If empty, the GameObject name is used.")]
        [SerializeField]
        private string displayName;

        [Tooltip(
            "If disabled, this plug cannot be physically inserted " +
            "into a SparkJackSocket.")]
        [SerializeField]
        private bool plugEnabled = true;


        // =====================================================================
        // ELECTRICAL TERMINAL PAIR
        // =====================================================================

        [Header("Electrical Terminal Pair")]

        [Tooltip(
            "Positive electrical terminal of this plug. " +
            "This terminal connects to the socket positive terminal " +
            "and optionally to the linked plug positive terminal.")]
        [SerializeField]
        private SparkTerminal positiveTerminal;

        [Tooltip(
            "Negative electrical terminal of this plug. " +
            "This terminal connects to the socket negative terminal " +
            "and optionally to the linked plug negative terminal.")]
        [SerializeField]
        private SparkTerminal negativeTerminal;

        public SparkTerminal PositiveTerminal =>
            positiveTerminal;

        public SparkTerminal NegativeTerminal =>
            negativeTerminal;

        public bool HasTerminalPair =>
            positiveTerminal != null &&
            negativeTerminal != null;


        // =====================================================================
        // INTERNAL PLUG LINK
        // =====================================================================

        [Header("Internal Plug Link")]

        [Tooltip(
            "Optional second plug that is electrically connected to this plug " +
            "using two conductors: positive-to-positive and negative-to-negative.")]
        [SerializeField]
        private SparkPlug linkedPlug;

        [Tooltip(
            "Circuit system used to create the real electrical connections " +
            "between this plug and the linked plug.")]
        [SerializeField]
        private ProjectSpark.Circuit.SparkCircuitSystem circuitSystem;

        [Tooltip(
            "When enabled, this plug is allowed to establish its internal " +
            "two-conductor connection to the linked plug.")]
        [SerializeField]
        private bool internalLinkEnabled = true;


        // =====================================================================
        // PHYSICAL
        // =====================================================================

        [Header("Physical")]

        [Tooltip(
            "Optional transform representing the actual connector tip. " +
            "Useful for precise socket alignment.")]
        [SerializeField]
        private Transform connectorTip;

        [Tooltip(
            "Optional transform representing the plug body/root used " +
            "for physical manipulation.")]
        [SerializeField]
        private Transform plugBody;

        [Tooltip(
            "If enabled, the plug can only be inserted into one socket " +
            "at a time.")]
        [SerializeField]
        private bool singleSocketConnection = true;


        // =====================================================================
        // DEBUG
        // =====================================================================

        [Header("Debug")]

        [SerializeField]
        private bool showDebugLog = false;


        // =====================================================================
        // RUNTIME
        // =====================================================================

        private SparkJackSocket connectedSocket;

        private SparkCircuitConnection internalPositiveConnection;
        private SparkCircuitConnection internalNegativeConnection;

        private bool hasInternalPositiveConnection;
        private bool hasInternalNegativeConnection;


        // =====================================================================
        // PUBLIC PROPERTIES
        // =====================================================================

        public string PlugType => plugType;

        public string DisplayName
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(displayName))
                    return displayName;

                return gameObject.name;
            }
        }

        public Transform PlugTransform => transform;

        public Transform ConnectorTip => connectorTip;

        public Transform PlugBody
        {
            get
            {
                return plugBody != null
                    ? plugBody
                    : transform;
            }
        }

        public bool IsPlugEnabled => plugEnabled;

        public bool IsInserted =>
            connectedSocket != null;

        public SparkJackSocket ConnectedSocket =>
            connectedSocket;

        public SparkPlug LinkedPlug =>
            linkedPlug;

        public bool InternalLinkEnabled =>
            internalLinkEnabled;

        public bool HasInternalPositiveConnection =>
            hasInternalPositiveConnection;

        public bool HasInternalNegativeConnection =>
            hasInternalNegativeConnection;

        public bool HasInternalConnection =>
            hasInternalPositiveConnection &&
            hasInternalNegativeConnection;

        public UnityEvent<SparkJackSocket> Inserted =>
            inserted;

        public UnityEvent<SparkJackSocket> Removed =>
            removed;


        // =====================================================================
        // UNITY
        // =====================================================================

        private void Start()
        {
            ConnectInternalPlug();
        }

        private void OnDestroy()
        {
            DisconnectInternalPlug();
        }


        // =====================================================================
        // SOCKET VALIDATION
        // =====================================================================

        public bool CanBeInserted()
        {
            if (!plugEnabled)
                return false;

            if (singleSocketConnection &&
                connectedSocket != null)
            {
                return false;
            }

            return true;
        }


        public bool IsCompatibleWith(
            SparkJackSocket socket)
        {
            if (socket == null)
                return false;

            if (!CanBeInserted())
                return false;

            return socket.AcceptsType(
                plugType);
        }


        // =====================================================================
        // INTERNAL SOCKET API
        // =====================================================================

        public bool RegisterInsertion(
            SparkJackSocket socket)
        {
            if (socket == null)
                return false;

            if (!plugEnabled)
                return false;

            if (connectedSocket != null &&
                connectedSocket != socket)
            {
                return false;
            }

            connectedSocket = socket;

            inserted.Invoke(socket);

            Log(
                $"INSERTED → {socket.name}");

            return true;
        }


        public bool RegisterRemoval(
            SparkJackSocket socket)
        {
            if (socket == null)
                return false;

            if (connectedSocket != socket)
                return false;

            connectedSocket = null;

            removed.Invoke(socket);

            Log(
                $"REMOVED ← {socket.name}");

            return true;
        }


        // =====================================================================
        // INTERNAL PLUG ELECTRICAL CONNECTION
        // =====================================================================

        private bool ConnectInternalPlug()
        {
            if (!internalLinkEnabled)
                return true;

            if (linkedPlug == null)
                return true;

            if (linkedPlug == this)
            {
                Debug.LogWarning(
                    "[SparkPlug] A plug cannot link to itself.",
                    this);

                return false;
            }

            if (!HasTerminalPair)
            {
                Debug.LogWarning(
                    "[SparkPlug] Internal link requires both terminals.",
                    this);

                return false;
            }

            if (!linkedPlug.HasTerminalPair)
            {
                Debug.LogWarning(
                    "[SparkPlug] Linked plug does not have a complete terminal pair.",
                    this);

                return false;
            }

            if (circuitSystem == null)
            {
                Debug.LogWarning(
                    "[SparkPlug] Internal link requires a SparkCircuitSystem.",
                    this);

                return false;
            }

            if (linkedPlug.circuitSystem != null &&
                linkedPlug.circuitSystem != circuitSystem)
            {
                Debug.LogWarning(
                    "[SparkPlug] Linked plugs must use the same SparkCircuitSystem.",
                    this);

                return false;
            }

            // Only one of the two plugs creates the connection.
            // This prevents both plugs from attempting to create
            // the same positive/negative pair.
            if (GetInstanceID() > linkedPlug.GetInstanceID())
                return true;

            if (hasInternalPositiveConnection &&
                hasInternalNegativeConnection)
            {
                return true;
            }

            if (hasInternalPositiveConnection ||
                hasInternalNegativeConnection)
            {
                DisconnectInternalPlug();
            }

            bool positiveCreated =
                circuitSystem.TryCreateConnection(
                    positiveTerminal,
                    linkedPlug.positiveTerminal,
                    SparkConnectionKind.Connector,
                    SparkConnectionDirection.Bidirectional,
                    out SparkCircuitConnection createdPositive);

            if (!positiveCreated)
            {
                Debug.LogWarning(
                    "[SparkPlug] Failed to create internal POSITIVE conductor.",
                    this);

                return false;
            }

            internalPositiveConnection =
                createdPositive;

            hasInternalPositiveConnection =
                true;


            bool negativeCreated =
                circuitSystem.TryCreateConnection(
                    negativeTerminal,
                    linkedPlug.negativeTerminal,
                    SparkConnectionKind.Connector,
                    SparkConnectionDirection.Bidirectional,
                    out SparkCircuitConnection createdNegative);

            if (!negativeCreated)
            {
                circuitSystem.TryRemoveConnection(
                    internalPositiveConnection.Id,
                    out _);

                internalPositiveConnection =
                    default;

                hasInternalPositiveConnection =
                    false;

                Debug.LogWarning(
                    "[SparkPlug] Failed to create internal NEGATIVE conductor. " +
                    "Positive conductor was rolled back.",
                    this);

                return false;
            }

            internalNegativeConnection =
                createdNegative;

            hasInternalNegativeConnection =
                true;

            Log(
                $"INTERNAL LINK CONNECTED → {linkedPlug.name}");

            return true;
        }


        private void DisconnectInternalPlug()
        {
            if (circuitSystem == null)
            {
                internalPositiveConnection =
                    default;

                internalNegativeConnection =
                    default;

                hasInternalPositiveConnection =
                    false;

                hasInternalNegativeConnection =
                    false;

                return;
            }

            if (hasInternalPositiveConnection)
            {
                circuitSystem.TryRemoveConnection(
                    internalPositiveConnection.Id,
                    out _);

                internalPositiveConnection =
                    default;

                hasInternalPositiveConnection =
                    false;
            }

            if (hasInternalNegativeConnection)
            {
                circuitSystem.TryRemoveConnection(
                    internalNegativeConnection.Id,
                    out _);

                internalNegativeConnection =
                    default;

                hasInternalNegativeConnection =
                    false;
            }

            Log(
                "INTERNAL LINK DISCONNECTED");
        }


        // =====================================================================
        // CONTROL
        // =====================================================================

        public void SetPlugEnabled(
            bool enabled)
        {
            if (plugEnabled == enabled)
                return;

            plugEnabled = enabled;

            if (!plugEnabled)
            {
                DisconnectInternalPlug();
                return;
            }

            ConnectInternalPlug();
        }


        public void SetPlugType(
            string type)
        {
            if (string.IsNullOrWhiteSpace(type))
            {
                Debug.LogWarning(
                    $"[{nameof(SparkPlug)}] " +
                    $"Cannot assign an empty plug type.",
                    this);

                return;
            }

            plugType = type.Trim();
        }


        // =====================================================================
        // INTERNAL LINK CONFIGURATION
        // =====================================================================

        public void SetLinkedPlug(
            SparkPlug plug)
        {
            if (plug == this)
            {
                Debug.LogWarning(
                    "[SparkPlug] A plug cannot link to itself.",
                    this);

                return;
            }

            if (linkedPlug == plug)
                return;

            DisconnectInternalPlug();

            linkedPlug = plug;

            ConnectInternalPlug();
        }


        public void SetInternalLinkEnabled(
            bool enabled)
        {
            if (internalLinkEnabled == enabled)
                return;

            internalLinkEnabled = enabled;

            if (!internalLinkEnabled)
            {
                DisconnectInternalPlug();
                return;
            }

            ConnectInternalPlug();
        }


        // =====================================================================
        // VALIDATION
        // =====================================================================

        private void OnValidate()
        {
            if (plugType != null)
                plugType = plugType.Trim();

            if (displayName != null)
                displayName = displayName.Trim();
        }


        // =====================================================================
        // DEBUG
        // =====================================================================

        private void Log(
            string message)
        {
            if (!showDebugLog)
                return;

            Debug.Log(
                $"[PLUG] {message}",
                this);
        }
    }
}