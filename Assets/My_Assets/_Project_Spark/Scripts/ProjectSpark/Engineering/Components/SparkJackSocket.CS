using System;
using UnityEngine;
using UnityEngine.Events;
using ProjectSpark.Circuit;

namespace ProjectSpark.Gameplay
{
    /// <summary>
    /// Represents a physical electronic jack/socket that can accept
    /// a compatible SparkPlug and create/remove its electrical
    /// terminal connections.
    ///
    /// Supports:
    /// - Automatic plug detection.
    /// - Catch distance.
    /// - Release distance.
    /// - Physical plug alignment.
    /// - Optional smooth insertion.
    /// - Automatic electrical connection.
    /// - Positive/negative terminal pairing.
    /// - PowerSocket mode.
    /// - Automatic PowerSupply connection.
    /// - Optional direct output-device connection.
    /// - Automatic electrical disconnection.
    /// - Immediate readiness for the next compatible plug.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkJackSocket : MonoBehaviour
    {
        // ---------------------------------------------------------------------
        // Types
        // ---------------------------------------------------------------------

        public enum SocketState
        {
            Empty,
            Searching,
            Occupied
        }

        /// <summary>
        /// Defines the electrical role of this socket.
        ///
        /// Connector:
        /// Normal passive connector/socket.
        ///
        /// PowerSocket:
        /// Permanently connects its positive/negative terminals
        /// to an assigned SparkPowerSupply.
        /// </summary>
        public enum SocketRole
        {
            Connector,
            PowerSocket
        }

        [Serializable]
        public sealed class PlugEvent : UnityEvent<SparkPlug>
        {
        }

        // ---------------------------------------------------------------------
        // Socket
        // ---------------------------------------------------------------------

        [Header("Socket")]
        [SerializeField]
        private string socketType = "USB-C";

        [SerializeField]
        private bool socketEnabled = true;

        [SerializeField]
        private bool singleOccupancy = true;

        // ---------------------------------------------------------------------
        // Socket Role
        // ---------------------------------------------------------------------

        [Header("Socket Role")]
        [SerializeField]
        private SocketRole socketRole =
            SocketRole.Connector;

        [SerializeField]
        private SparkPowerSupply powerSupply;

        // ---------------------------------------------------------------------
        // Output Device
        // ---------------------------------------------------------------------

        [Header("Output Device")]
        [Tooltip(
            "Optional electrical device permanently connected to this socket. " +
            "For Level 2 Socket OUT, assign the SparkLED here.")]
        [SerializeField]
        private SparkLED outputDevice;

        /*
         * These connections belong to the optional output device.
         *
         * For the Level 2 charger/output socket:
         *
         *     Socket OUT + → LED Anode
         *     Socket OUT - → LED Cathode
         *
         * They are separate from:
         *
         *     Plug + → Socket +
         *     Plug - → Socket -
         *
         * and separate from the PowerSocket connections.
         */
        private SparkCircuitConnection outputPositiveConnection;
        private SparkCircuitConnection outputNegativeConnection;

        private bool hasOutputPositiveConnection;
        private bool hasOutputNegativeConnection;

        // ---------------------------------------------------------------------
        // Physical Placement
        // ---------------------------------------------------------------------

        [Header("Physical Placement")]
        [SerializeField]
        private Transform plugPlacement;

        [SerializeField]
        private bool parentPlugWhenCaught = true;

        [SerializeField]
        private bool smoothInsertion = true;

        [Min(0.01f)]
        [SerializeField]
        private float insertionSpeed = 12f;

        [Min(0.01f)]
        [SerializeField]
        private float rotationSpeed = 12f;

        [SerializeField]
        private bool forceFinalRotation = true;

        // ---------------------------------------------------------------------
        // Connection Distance
        // ---------------------------------------------------------------------

        [Header("Connection Distance")]
        [Min(0.001f)]
        [SerializeField]
        private float catchDistance = 0.08f;

        [Min(0.001f)]
        [SerializeField]
        private float releaseDistance = 0.15f;

        [SerializeField]
        private bool automaticRelease = true;

        [SerializeField]
        private bool ejectOnAutomaticRelease;

        // ---------------------------------------------------------------------
        // Electrical Terminals
        // ---------------------------------------------------------------------

        [Header("Electrical Terminals")]
        [SerializeField]
        private SparkTerminal positiveTerminal;

        [SerializeField]
        private SparkTerminal negativeTerminal;

        [SerializeField]
        private SparkCircuitSystem circuitSystem;

        /*
         * These connections belong to the currently inserted plug.
         */
        private SparkCircuitConnection positiveConnection;
        private SparkCircuitConnection negativeConnection;

        private bool hasPositiveConnection;
        private bool hasNegativeConnection;

        // ---------------------------------------------------------------------
        // PowerSupply Connections
        // ---------------------------------------------------------------------

        /*
         * These connections belong to the PowerSocket itself.
         *
         * A PowerSocket can have:
         *
         *     PowerSupply + → Socket +
         *     Plug +        → Socket +
         *
         * and the same for negative.
         */
        private SparkCircuitConnection powerPositiveConnection;
        private SparkCircuitConnection powerNegativeConnection;

        private bool hasPowerPositiveConnection;
        private bool hasPowerNegativeConnection;

        // ---------------------------------------------------------------------
        // Automatic Catch
        // ---------------------------------------------------------------------

        [Header("Automatic Catch")]
        [SerializeField]
        private bool automaticCatch = true;

        [Min(0.01f)]
        [SerializeField]
        private float searchInterval = 0.1f;

        [SerializeField]
        private bool chooseClosestPlug = true;

        // ---------------------------------------------------------------------
        // Parenting
        // ---------------------------------------------------------------------

        [Header("Parenting")]
        [SerializeField]
        private bool restoreOriginalParent = true;

        // ---------------------------------------------------------------------
        // Events
        // ---------------------------------------------------------------------

        [Header("Events")]
        [SerializeField]
        private PlugEvent plugCaught =
            new PlugEvent();

        [SerializeField]
        private PlugEvent plugReleased =
            new PlugEvent();

        [SerializeField]
        private UnityEvent socketBecameOccupied =
            new UnityEvent();

        [SerializeField]
        private UnityEvent socketBecameEmpty =
            new UnityEvent();

        // ---------------------------------------------------------------------
        // Debug
        // ---------------------------------------------------------------------

        [Header("Debug")]
        [SerializeField]
        private bool showDebugLog;

        [SerializeField]
        private bool drawDebugGizmos = true;

        // ---------------------------------------------------------------------
        // Runtime
        // ---------------------------------------------------------------------

        private SparkPlug caughtPlug;
        private Transform originalParent;

        private SocketState state =
            SocketState.Empty;

        private float searchTimer;
        private bool insertionInProgress;

        // ---------------------------------------------------------------------
        // Public Properties
        // ---------------------------------------------------------------------

        public string SocketType =>
            socketType;

        public SocketRole Role =>
            socketRole;

        public SparkPowerSupply PowerSupply =>
            powerSupply;

        public SparkLED OutputDevice =>
            outputDevice;

        public Transform PlugPlacement =>
            plugPlacement;

        public bool IsSocketEnabled =>
            socketEnabled;

        public bool HasPlug =>
            caughtPlug != null;

        public SparkPlug CaughtPlug =>
            caughtPlug;

        public SocketState State =>
            state;

        public float CatchDistance =>
            catchDistance;

        public float ReleaseDistance =>
            releaseDistance;

        public bool IsReadyForConnection =>
            socketEnabled &&
            caughtPlug == null;

        public SparkTerminal PositiveTerminal =>
            positiveTerminal;

        public SparkTerminal NegativeTerminal =>
            negativeTerminal;

        public bool HasTerminalPair =>
            positiveTerminal != null &&
            negativeTerminal != null;

        public bool HasPositiveConnection =>
            hasPositiveConnection;

        public bool HasNegativeConnection =>
            hasNegativeConnection;

        public bool HasElectricalConnection =>
            hasPositiveConnection &&
            hasNegativeConnection;

        public bool HasPowerPositiveConnection =>
            hasPowerPositiveConnection;

        public bool HasPowerNegativeConnection =>
            hasPowerNegativeConnection;

        public bool HasPowerConnection =>
            hasPowerPositiveConnection &&
            hasPowerNegativeConnection;

        public bool HasOutputPositiveConnection =>
            hasOutputPositiveConnection;

        public bool HasOutputNegativeConnection =>
            hasOutputNegativeConnection;

        public bool HasOutputConnection =>
            hasOutputPositiveConnection &&
            hasOutputNegativeConnection;

        public Vector3 DesiredPosition =>
            plugPlacement != null
                ? plugPlacement.position
                : transform.position;

        public Quaternion DesiredRotation =>
            plugPlacement != null
                ? plugPlacement.rotation
                : transform.rotation;

        public PlugEvent PlugCaught =>
            plugCaught;

        public PlugEvent PlugReleased =>
            plugReleased;

        // ---------------------------------------------------------------------
        // Unity
        // ---------------------------------------------------------------------

        private void Awake()
        {
            ValidateConfiguration();

            state =
                SocketState.Empty;
        }

        private void Start()
        {
            /*
             * PowerSupply and output-device connections are created
             * after their Awake methods have initialized.
             */
            ConnectPowerSupply();
            ConnectOutputDevice();
        }

        private void Update()
        {
            if (!socketEnabled)
            {
                return;
            }

            if (caughtPlug != null)
            {
                UpdateConnectedPlug();
                return;
            }

            if (!automaticCatch)
            {
                return;
            }

            UpdateAutomaticCatch();
        }

        private void OnDestroy()
        {
            DisconnectElectricalConnections();
            DisconnectPowerSupply();
            DisconnectOutputDevice();
        }

        // ---------------------------------------------------------------------
        // Connected Plug
        // ---------------------------------------------------------------------

        private void UpdateConnectedPlug()
        {
            if (caughtPlug == null)
            {
                PrepareForNextConnection();
                return;
            }

            if (insertionInProgress)
            {
                UpdateInsertion();
            }

            if (!automaticRelease)
            {
                return;
            }

            CheckReleaseDistance();
        }

        private void UpdateInsertion()
        {
            if (caughtPlug == null)
            {
                insertionInProgress = false;
                return;
            }

            Transform plugTransform =
                caughtPlug.PlugTransform;

            if (plugTransform == null)
            {
                insertionInProgress = false;
                return;
            }

            Vector3 targetPosition =
                DesiredPosition;

            Quaternion targetRotation =
                DesiredRotation;

            plugTransform.position =
                Vector3.MoveTowards(
                    plugTransform.position,
                    targetPosition,
                    insertionSpeed *
                    Time.deltaTime);

            if (forceFinalRotation)
            {
                plugTransform.rotation =
                    Quaternion.RotateTowards(
                        plugTransform.rotation,
                        targetRotation,
                        rotationSpeed *
                        360f *
                        Time.deltaTime);
            }

            bool positionComplete =
                Vector3.SqrMagnitude(
                    plugTransform.position -
                    targetPosition) <=
                0.000001f;

            bool rotationComplete =
                !forceFinalRotation ||
                Quaternion.Angle(
                    plugTransform.rotation,
                    targetRotation) <=
                0.1f;

            if (!positionComplete)
            {
                return;
            }

            if (!rotationComplete)
            {
                return;
            }

            plugTransform.position =
                targetPosition;

            if (forceFinalRotation)
            {
                plugTransform.rotation =
                    targetRotation;
            }

            insertionInProgress =
                false;
        }

        private void CheckReleaseDistance()
        {
            if (caughtPlug == null)
            {
                return;
            }

            Transform plugTransform =
                caughtPlug.PlugTransform;

            if (plugTransform == null)
            {
                ReleasePlug(false);
                return;
            }

            float distance =
                Vector3.Distance(
                    plugTransform.position,
                    DesiredPosition);

            if (distance <= releaseDistance)
            {
                return;
            }

            Log(
                $"Plug '{caughtPlug.name}' moved outside release " +
                $"distance ({distance:F3}m > {releaseDistance:F3}m).");

            ReleasePlug(
                ejectOnAutomaticRelease);
        }

        // ---------------------------------------------------------------------
        // Automatic Catch
        // ---------------------------------------------------------------------

        private void UpdateAutomaticCatch()
        {
            if (!CanSearchForPlug())
            {
                return;
            }

            searchTimer -=
                Time.deltaTime;

            if (searchTimer > 0f)
            {
                return;
            }

            searchTimer =
                searchInterval;

            SetState(
                SocketState.Searching);

            SparkPlug candidate =
                FindCompatiblePlug();

            if (candidate == null)
            {
                SetState(
                    SocketState.Empty);

                return;
            }

            CatchPlug(candidate);
        }

        private bool CanSearchForPlug()
        {
            if (!socketEnabled)
            {
                return false;
            }

            if (caughtPlug != null)
            {
                return false;
            }

            if (!automaticCatch)
            {
                return false;
            }

            if (singleOccupancy &&
                HasPlug)
            {
                return false;
            }

            return true;
        }

        private SparkPlug FindCompatiblePlug()
        {
            SparkPlug[] plugs =
                FindObjectsByType<SparkPlug>(
                    FindObjectsInactive.Exclude,
                    FindObjectsSortMode.None);

            SparkPlug closestPlug = null;

            float closestDistanceSqr =
                float.MaxValue;

            for (int i = 0;
                 i < plugs.Length;
                 i++)
            {
                SparkPlug plug =
                    plugs[i];

                if (!IsCandidateCompatible(
                        plug))
                {
                    continue;
                }

                float distanceSqr =
                    Vector3.SqrMagnitude(
                        plug.PlugTransform.position -
                        DesiredPosition);

                if (distanceSqr >
                    catchDistance *
                    catchDistance)
                {
                    continue;
                }

                if (!chooseClosestPlug)
                {
                    return plug;
                }

                if (distanceSqr <
                    closestDistanceSqr)
                {
                    closestDistanceSqr =
                        distanceSqr;

                    closestPlug =
                        plug;
                }
            }

            return closestPlug;
        }

        private bool IsCandidateCompatible(
            SparkPlug plug)
        {
            if (plug == null)
            {
                return false;
            }

            if (!plug.isActiveAndEnabled)
            {
                return false;
            }

            if (!plug.IsPlugEnabled)
            {
                return false;
            }

            if (!AcceptsType(
                    plug.PlugType))
            {
                return false;
            }

            if (!plug.CanBeInserted())
            {
                return false;
            }

            if (plug.PlugTransform == null)
            {
                return false;
            }

            return true;
        }

        // ---------------------------------------------------------------------
        // Catch
        // ---------------------------------------------------------------------

        public bool CatchPlug(
            SparkPlug plug)
        {
            if (!CanCatch(plug))
            {
                return false;
            }

            return AttachPlug(plug);
        }

        private bool CanCatch(
            SparkPlug plug)
        {
            if (!socketEnabled)
            {
                return false;
            }

            if (plug == null)
            {
                return false;
            }

            if (singleOccupancy &&
                caughtPlug != null)
            {
                return false;
            }

            if (!plug.IsPlugEnabled)
            {
                return false;
            }

            if (!AcceptsType(
                    plug.PlugType))
            {
                return false;
            }

            if (!plug.CanBeInserted())
            {
                return false;
            }

            if (plug.PlugTransform == null)
            {
                return false;
            }

            float distance =
                Vector3.Distance(
                    plug.PlugTransform.position,
                    DesiredPosition);

            if (distance > catchDistance)
            {
                return false;
            }

            return true;
        }

        private bool AttachPlug(
            SparkPlug plug)
        {
            if (plug == null)
            {
                return false;
            }

            Transform plugTransform =
                plug.PlugTransform;

            if (plugTransform == null)
            {
                return false;
            }

            originalParent =
                plugTransform.parent;

            if (!plug.RegisterInsertion(this))
            {
                return false;
            }

            /*
             * PowerSocket and OutputDevice connections are independent
             * from the plug connection.
             *
             * This method only connects:
             *
             *     Plug + → Socket +
             *     Plug - → Socket -
             */
            if (!ConnectPlugElectrical(plug))
            {
                plug.RegisterRemoval(this);

                DisconnectElectricalConnections();

                Log(
                    $"Electrical connection failed for plug '{plug.name}'. " +
                    "Insertion rolled back.");

                return false;
            }

            caughtPlug =
                plug;

            if (parentPlugWhenCaught)
            {
                Transform parentTransform =
                    plugPlacement != null
                        ? plugPlacement
                        : transform;

                plugTransform.SetParent(
                    parentTransform,
                    true);
            }

            if (smoothInsertion)
            {
                insertionInProgress =
                    true;
            }
            else
            {
                plugTransform.position =
                    DesiredPosition;

                if (forceFinalRotation)
                {
                    plugTransform.rotation =
                        DesiredRotation;
                }

                insertionInProgress =
                    false;
            }

            searchTimer =
                searchInterval;

            SetState(
                SocketState.Occupied);

            plugCaught.Invoke(plug);

            Log(
                $"Plug '{plug.name}' connected to socket '{name}'.");

            return true;
        }

        // ---------------------------------------------------------------------
        // Plug Electrical Connection
        // ---------------------------------------------------------------------

        private bool ConnectPlugElectrical(
            SparkPlug plug)
        {
            if (!HasTerminalPair)
            {
                Debug.LogError(
                    $"SparkJackSocket '{name}' does not have a valid " +
                    "positive/negative terminal pair.",
                    this);

                return false;
            }

            if (circuitSystem == null)
            {
                Debug.LogError(
                    $"SparkJackSocket '{name}' has no " +
                    "SparkCircuitSystem assigned.",
                    this);

                return false;
            }

            if (plug == null)
            {
                return false;
            }

            if (!plug.HasTerminalPair)
            {
                Debug.LogError(
                    $"SparkPlug '{plug.name}' does not have a valid " +
                    "positive/negative terminal pair.",
                    plug);

                return false;
            }

            if (hasPositiveConnection ||
                hasNegativeConnection)
            {
                Debug.LogWarning(
                    $"SparkJackSocket '{name}' already has an " +
                    "electrical plug connection.",
                    this);

                return false;
            }

            // Positive
            SparkCircuitConnection
                createdPositiveConnection;

            bool positiveCreated =
                circuitSystem.TryCreateConnection(
                    plug.PositiveTerminal,
                    positiveTerminal,
                    SparkConnectionKind.Connector,
                    SparkConnectionDirection.Bidirectional,
                    out createdPositiveConnection);

            if (!positiveCreated)
            {
                Debug.LogWarning(
                    $"SparkJackSocket '{name}' failed to create " +
                    "positive electrical connection.",
                    this);

                return false;
            }

            positiveConnection =
                createdPositiveConnection;

            hasPositiveConnection =
                true;

            // Negative
            SparkCircuitConnection
                createdNegativeConnection;

            bool negativeCreated =
                circuitSystem.TryCreateConnection(
                    plug.NegativeTerminal,
                    negativeTerminal,
                    SparkConnectionKind.Connector,
                    SparkConnectionDirection.Bidirectional,
                    out createdNegativeConnection);

            if (!negativeCreated)
            {
                DisconnectPositiveConnection();

                Debug.LogWarning(
                    $"SparkJackSocket '{name}' failed to create " +
                    "negative electrical connection. " +
                    "Positive connection was rolled back.",
                    this);

                return false;
            }

            negativeConnection =
                createdNegativeConnection;

            hasNegativeConnection =
                true;

            Log(
                $"Electrical plug connection established for " +
                $"plug '{plug.name}'.");

            return true;
        }

        // ---------------------------------------------------------------------
        // Plug Electrical Disconnection
        // ---------------------------------------------------------------------

        private void DisconnectElectricalConnections()
        {
            DisconnectPositiveConnection();
            DisconnectNegativeConnection();
        }

        private void DisconnectPositiveConnection()
        {
            if (!hasPositiveConnection)
            {
                return;
            }

            if (circuitSystem != null)
            {
                circuitSystem.TryRemoveConnection(
                    positiveConnection.Id,
                    out SparkCircuitConnection removedConnection);
            }

            positiveConnection =
                default;

            hasPositiveConnection =
                false;
        }

        private void DisconnectNegativeConnection()
        {
            if (!hasNegativeConnection)
            {
                return;
            }

            if (circuitSystem != null)
            {
                circuitSystem.TryRemoveConnection(
                    negativeConnection.Id,
                    out SparkCircuitConnection removedConnection);
            }

            negativeConnection =
                default;

            hasNegativeConnection =
                false;
        }

        // ---------------------------------------------------------------------
        // PowerSocket
        // ---------------------------------------------------------------------

        private bool ConnectPowerSupply()
        {
            if (socketRole != SocketRole.PowerSocket)
            {
                return true;
            }

            if (!socketEnabled)
            {
                return false;
            }

            if (powerSupply == null)
            {
                Debug.LogWarning(
                    $"[SparkJackSocket] PowerSocket '{name}' has no " +
                    $"SparkPowerSupply assigned.",
                    this);

                return false;
            }

            if (circuitSystem == null)
            {
                Debug.LogWarning(
                    $"[SparkJackSocket] PowerSocket '{name}' has no " +
                    $"SparkCircuitSystem assigned.",
                    this);

                return false;
            }

            if (!HasTerminalPair)
            {
                Debug.LogWarning(
                    $"[SparkJackSocket] PowerSocket '{name}' requires " +
                    "Positive and Negative terminals.",
                    this);

                return false;
            }

            if (powerSupply.PositiveTerminal == null ||
                powerSupply.NegativeTerminal == null)
            {
                Debug.LogWarning(
                    $"[SparkJackSocket] Assigned SparkPowerSupply " +
                    $"'{powerSupply.name}' does not have both terminals.",
                    this);

                return false;
            }

            if (hasPowerPositiveConnection &&
                hasPowerNegativeConnection)
            {
                return true;
            }

            if (hasPowerPositiveConnection ||
                hasPowerNegativeConnection)
            {
                DisconnectPowerSupply();
            }

            // Positive
            SparkCircuitConnection
                createdPositive;

            bool positiveCreated =
                circuitSystem.TryCreateConnection(
                    powerSupply.PositiveTerminal,
                    positiveTerminal,
                    SparkConnectionKind.Connector,
                    SparkConnectionDirection.Bidirectional,
                    out createdPositive);

            if (!positiveCreated)
            {
                Debug.LogWarning(
                    $"[SparkJackSocket] PowerSocket '{name}' failed " +
                    "to connect PowerSupply positive terminal.",
                    this);

                return false;
            }

            powerPositiveConnection =
                createdPositive;

            hasPowerPositiveConnection =
                true;

            // Negative
            SparkCircuitConnection
                createdNegative;

            bool negativeCreated =
                circuitSystem.TryCreateConnection(
                    powerSupply.NegativeTerminal,
                    negativeTerminal,
                    SparkConnectionKind.Connector,
                    SparkConnectionDirection.Bidirectional,
                    out createdNegative);

            if (!negativeCreated)
            {
                circuitSystem.TryRemoveConnection(
                    powerPositiveConnection.Id,
                    out SparkCircuitConnection removedConnection);

                powerPositiveConnection =
                    default;

                hasPowerPositiveConnection =
                    false;

                Debug.LogWarning(
                    $"[SparkJackSocket] PowerSocket '{name}' failed " +
                    "to connect PowerSupply negative terminal. " +
                    "Positive connection was rolled back.",
                    this);

                return false;
            }

            powerNegativeConnection =
                createdNegative;

            hasPowerNegativeConnection =
                true;

            Debug.Log(
                $"[SparkJackSocket] PowerSocket '{name}' connected " +
                $"to PowerSupply '{powerSupply.name}'.",
                this);

            return true;
        }

        private void DisconnectPowerSupply()
        {
            if (circuitSystem == null)
            {
                powerPositiveConnection =
                    default;

                powerNegativeConnection =
                    default;

                hasPowerPositiveConnection =
                    false;

                hasPowerNegativeConnection =
                    false;

                return;
            }

            if (hasPowerPositiveConnection)
            {
                circuitSystem.TryRemoveConnection(
                    powerPositiveConnection.Id,
                    out SparkCircuitConnection removedConnection);

                hasPowerPositiveConnection =
                    false;

                powerPositiveConnection =
                    default;
            }

            if (hasPowerNegativeConnection)
            {
                circuitSystem.TryRemoveConnection(
                    powerNegativeConnection.Id,
                    out SparkCircuitConnection removedConnection);

                hasPowerNegativeConnection =
                    false;

                powerNegativeConnection =
                    default;
            }
        }

        // ---------------------------------------------------------------------
        // Output Device
        // ---------------------------------------------------------------------

        /// <summary>
        /// Connects the optional output device directly to this socket.
        ///
        /// For Level 2:
        ///
        ///     Socket OUT + → SparkLED Anode
        ///     Socket OUT - → SparkLED Cathode
        ///
        /// These are real SparkCircuitSystem topology connections.
        /// </summary>
        private bool ConnectOutputDevice()
        {
            if (outputDevice == null)
            {
                return true;
            }

            if (!socketEnabled)
            {
                return false;
            }

            if (circuitSystem == null)
            {
                Debug.LogWarning(
                    $"[SparkJackSocket] Output socket '{name}' has no " +
                    "SparkCircuitSystem assigned.",
                    this);

                return false;
            }

            if (!HasTerminalPair)
            {
                Debug.LogWarning(
                    $"[SparkJackSocket] Output socket '{name}' requires " +
                    "Positive and Negative terminals.",
                    this);

                return false;
            }

            if (outputDevice.AnodeTerminal == null ||
                outputDevice.CathodeTerminal == null)
            {
                Debug.LogWarning(
                    $"[SparkJackSocket] Output device '{outputDevice.name}' " +
                    "does not have both Anode and Cathode terminals.",
                    this);

                return false;
            }

            if (hasOutputPositiveConnection &&
                hasOutputNegativeConnection)
            {
                return true;
            }

            if (hasOutputPositiveConnection ||
                hasOutputNegativeConnection)
            {
                DisconnectOutputDevice();
            }

            // -------------------------------------------------------------
            // Socket Positive → LED Anode
            // -------------------------------------------------------------

            SparkCircuitConnection
                createdPositive;

            bool positiveCreated =
                circuitSystem.TryCreateConnection(
                    positiveTerminal,
                    outputDevice.AnodeTerminal,
                    SparkConnectionKind.Connector,
                    SparkConnectionDirection.Bidirectional,
                    out createdPositive);

            if (!positiveCreated)
            {
                Debug.LogWarning(
                    $"[SparkJackSocket] Output socket '{name}' failed " +
                    "to connect to LED anode.",
                    this);

                return false;
            }

            outputPositiveConnection =
                createdPositive;

            hasOutputPositiveConnection =
                true;

            // -------------------------------------------------------------
            // Socket Negative → LED Cathode
            // -------------------------------------------------------------

            SparkCircuitConnection
                createdNegative;

            bool negativeCreated =
                circuitSystem.TryCreateConnection(
                    negativeTerminal,
                    outputDevice.CathodeTerminal,
                    SparkConnectionKind.Connector,
                    SparkConnectionDirection.Bidirectional,
                    out createdNegative);

            if (!negativeCreated)
            {
                circuitSystem.TryRemoveConnection(
                    outputPositiveConnection.Id,
                    out SparkCircuitConnection removedConnection);

                outputPositiveConnection =
                    default;

                hasOutputPositiveConnection =
                    false;

                Debug.LogWarning(
                    $"[SparkJackSocket] Output socket '{name}' failed " +
                    "to connect to LED cathode. " +
                    "Positive connection was rolled back.",
                    this);

                return false;
            }

            outputNegativeConnection =
                createdNegative;

            hasOutputNegativeConnection =
                true;

            Debug.Log(
                $"[SparkJackSocket] Output device '{outputDevice.name}' " +
                $"connected to socket '{name}'.",
                this);

            return true;
        }

        /// <summary>
        /// Removes both output-device connections.
        /// </summary>
        private void DisconnectOutputDevice()
        {
            if (circuitSystem == null)
            {
                outputPositiveConnection =
                    default;

                outputNegativeConnection =
                    default;

                hasOutputPositiveConnection =
                    false;

                hasOutputNegativeConnection =
                    false;

                return;
            }

            if (hasOutputPositiveConnection)
            {
                circuitSystem.TryRemoveConnection(
                    outputPositiveConnection.Id,
                    out SparkCircuitConnection removedConnection);

                hasOutputPositiveConnection =
                    false;

                outputPositiveConnection =
                    default;
            }

            if (hasOutputNegativeConnection)
            {
                circuitSystem.TryRemoveConnection(
                    outputNegativeConnection.Id,
                    out SparkCircuitConnection removedConnection);

                hasOutputNegativeConnection =
                    false;

                outputNegativeConnection =
                    default;
            }
        }

        // ---------------------------------------------------------------------
        // Release
        // ---------------------------------------------------------------------

        public void ReleasePlug(
            bool eject = false)
        {
            if (caughtPlug == null)
            {
                PrepareForNextConnection();
                return;
            }

            SparkPlug releasedPlug =
                caughtPlug;

            Transform releasedTransform =
                releasedPlug.PlugTransform;

            insertionInProgress =
                false;

            /*
             * Disconnect plug topology first.
             *
             * PowerSupply and output-device connections remain intact.
             */
            DisconnectElectricalConnections();

            releasedPlug.RegisterRemoval(
                this);

            if (releasedTransform != null)
            {
                if (restoreOriginalParent)
                {
                    releasedTransform.SetParent(
                        originalParent,
                        true);
                }
                else
                {
                    releasedTransform.SetParent(
                        null,
                        true);
                }

                if (eject)
                {
                    EjectPlug(
                        releasedTransform);
                }
            }

            caughtPlug = null;
            originalParent = null;

            PrepareForNextConnection();

            plugReleased.Invoke(
                releasedPlug);

            Log(
                $"Plug '{releasedPlug.name}' released " +
                $"from socket '{name}'.");
        }

        private void EjectPlug(
            Transform plugTransform)
        {
            if (plugTransform == null)
            {
                return;
            }

            Vector3 direction =
                plugTransform.position -
                DesiredPosition;

            if (direction.sqrMagnitude <=
                0.000001f)
            {
                direction =
                    transform.forward;
            }

            direction.Normalize();

            plugTransform.position =
                DesiredPosition +
                direction *
                releaseDistance;
        }

        // ---------------------------------------------------------------------
        // State
        // ---------------------------------------------------------------------

        private void PrepareForNextConnection()
        {
            searchTimer =
                0f;

            insertionInProgress =
                false;

            if (caughtPlug == null)
            {
                SetState(
                    SocketState.Empty);
            }
            else
            {
                SetState(
                    SocketState.Occupied);
            }
        }

        private void SetState(
            SocketState newState)
        {
            if (state == newState)
            {
                return;
            }

            SocketState previousState =
                state;

            state =
                newState;

            if (newState ==
                    SocketState.Occupied &&
                previousState !=
                    SocketState.Occupied)
            {
                socketBecameOccupied.Invoke();
            }

            if (newState ==
                    SocketState.Empty &&
                previousState !=
                    SocketState.Empty)
            {
                socketBecameEmpty.Invoke();
            }
        }

        // ---------------------------------------------------------------------
        // Compatibility
        // ---------------------------------------------------------------------

        public bool AcceptsType(
            string plugType)
        {
            if (string.IsNullOrWhiteSpace(
                    socketType))
            {
                return true;
            }

            if (string.IsNullOrWhiteSpace(
                    plugType))
            {
                return false;
            }

            return string.Equals(
                socketType.Trim(),
                plugType.Trim(),
                StringComparison.OrdinalIgnoreCase);
        }

        // ---------------------------------------------------------------------
        // Public Controls
        // ---------------------------------------------------------------------

        public void SetSocketEnabled(
            bool enabled)
        {
            if (socketEnabled == enabled)
            {
                return;
            }

            socketEnabled =
                enabled;

            if (!socketEnabled)
            {
                if (caughtPlug != null)
                {
                    ReleasePlug();
                }

                DisconnectPowerSupply();
                DisconnectOutputDevice();

                state =
                    SocketState.Empty;

                return;
            }

            ConnectPowerSupply();
            ConnectOutputDevice();
        }

        public void SetAutomaticCatch(
            bool enabled)
        {
            automaticCatch =
                enabled;

            if (enabled)
            {
                searchTimer =
                    0f;
            }
        }

        public void SetAutomaticRelease(
            bool enabled)
        {
            automaticRelease =
                enabled;
        }

        public void SnapPlugIntoSocket()
        {
            if (caughtPlug == null)
            {
                return;
            }

            Transform plugTransform =
                caughtPlug.PlugTransform;

            if (plugTransform == null)
            {
                return;
            }

            plugTransform.position =
                DesiredPosition;

            if (forceFinalRotation)
            {
                plugTransform.rotation =
                    DesiredRotation;
            }

            insertionInProgress =
                false;
        }

        // ---------------------------------------------------------------------
        // Validation
        // ---------------------------------------------------------------------

        private void ValidateConfiguration()
        {
            if (catchDistance <= 0f)
            {
                catchDistance =
                    0.08f;
            }

            if (releaseDistance <= 0f)
            {
                releaseDistance =
                    0.15f;
            }

            if (releaseDistance <
                catchDistance)
            {
                releaseDistance =
                    catchDistance;
            }

            if (insertionSpeed <= 0f)
            {
                insertionSpeed =
                    12f;
            }

            if (rotationSpeed <= 0f)
            {
                rotationSpeed =
                    12f;
            }

            if (searchInterval <= 0f)
            {
                searchInterval =
                    0.1f;
            }

            if (socketType != null)
            {
                socketType =
                    socketType.Trim();
            }
        }

        // ---------------------------------------------------------------------
        // Debug
        // ---------------------------------------------------------------------

        private void Log(
            string message)
        {
            if (!showDebugLog)
            {
                return;
            }

            Debug.Log(
                $"[SparkJackSocket] {message}",
                this);
        }

        // ---------------------------------------------------------------------
        // Gizmos
        // ---------------------------------------------------------------------

        private void OnDrawGizmosSelected()
        {
            if (!drawDebugGizmos)
            {
                return;
            }

            Vector3 position =
                DesiredPosition;

            Gizmos.DrawWireSphere(
                position,
                catchDistance);

            Gizmos.DrawWireSphere(
                position,
                releaseDistance);
        }
    }
}