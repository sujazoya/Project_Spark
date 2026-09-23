using System;
using System.Collections.Generic;
using UnityEngine;
using ProjectSpark.Gameplay;
using ProjectSpark.Circuit;

namespace AAAUI.VFX
{
    public enum WirePolarity
    {
        Neutral,
        Positive,
        Negative
    }

    /// <summary>
    /// Handles interactive visual wire construction.
    ///
    /// IMPORTANT:
    /// This class owns the visual wire/path interaction.
    /// Electrical topology must be registered through SparkCircuitSystem.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SignalWireBuilder : MonoBehaviour
    {
        // ============================================================
        // REFERENCES
        // ============================================================

        [Header("Visual Wire System")]
        [SerializeField]
        private BreadboardGrid breadboard;

        [SerializeField]
        private SignalPath_Manager pathManager;

        [Header("Electrical System")]
        [SerializeField]
        private SparkCircuitSystem circuit;

        [Header("Input")]
        [SerializeField]
        private Camera targetCamera;

        [SerializeField]
        private LayerMask breadboardLayer;

        [SerializeField]
        private LayerMask terminalLayer = ~0;

        // ============================================================
        // WIRE SETTINGS
        // ============================================================

        [Header("Wire Settings")]
        [SerializeField, Min(0.001f)]
        private float snapDistance = 0.08f;

        [SerializeField, Min(0.001f)]
        private float terminalHitDistance = 1000f;

        [SerializeField]
        private SparkConnectionKind connectionKind =
            SparkConnectionKind.Wire;

        [SerializeField]
        private SparkConnectionDirection connectionDirection =
            SparkConnectionDirection.Bidirectional;

        [Header("Behaviour")]
        [SerializeField]
        private bool requireTerminalAtEnd = true;

        [SerializeField]
        private bool rejectSameTerminal = true;

        [SerializeField]
        private bool rejectCapacity = true;

        [SerializeField]
        private bool rejectDuplicateConnection = true;

        [SerializeField]
        private bool logWireOperations = true;
        [Header("Electrical System")]

[SerializeField]
private bool externalInputControl;
public bool ExternalInputControl
{
    get
    {
        return externalInputControl;
    }
}



[Header("Level Validation")]
[SerializeField]
//private Level1CircuitChecker level1Checker;

        // ============================================================
        // STATE
        // ============================================================

        private SignalPath currentPath;

        private WirePolarity currentPolarity;

        private SparkTerminal startTerminal;

        private bool drawing;

        private Vector3 currentEnd;

        private int lastColumn = -1;

        private int lastRow = -1;

        // Cached terminal connection lookup.
        private readonly List<SparkCircuitConnection> connectionBuffer =
            new List<SparkCircuitConnection>();

        // ============================================================
        // PUBLIC API
        // ============================================================

        public SignalPath CurrentPath =>
            currentPath;

        public bool IsDrawing =>
            drawing;

        public SparkTerminal StartTerminal =>
            startTerminal;

        public WirePolarity CurrentPolarity =>
            currentPolarity;

        public Vector3 CurrentEnd =>
            currentEnd;

        public SparkCircuitSystem Circuit =>
            circuit;

            public void SetExternalInputControl(
    bool enabled)
{
    externalInputControl = enabled;

    if (enabled && drawing)
    {
        CancelWire();
    }
}

        // ============================================================
        // UNITY
        // ============================================================

        private void Awake()
        {
            if (targetCamera == null)
                targetCamera = Camera.main;

            if (circuit == null)
                circuit = GetComponentInParent<SparkCircuitSystem>();

            ValidateReferences();
        }

        private void OnDisable()
        {
            if (drawing)
                CancelWire();
        }

       
        private void Update()
{
    if (externalInputControl)
        return;

    if (targetCamera == null)
        return;

    if (!drawing)
    {
        if (Input.GetMouseButtonDown(0))
            TryBeginWire();

        return;
    }

    UpdateDragging();

    if (Input.GetMouseButtonUp(0))
        EndWire();

    if (Input.GetMouseButtonDown(1))
        CancelWire();

    if (Input.GetKeyDown(KeyCode.Escape))
        CancelWire();
}

        // ============================================================
        // VALIDATION
        // ============================================================

        private void ValidateReferences()
        {
            if (breadboard == null)
            {
                Debug.LogError(
                    "SignalWireBuilder: Breadboard is NULL.",
                    this);
            }

            if (pathManager == null)
            {
                Debug.LogError(
                    "SignalWireBuilder: Path Manager is NULL.",
                    this);
            }

            if (circuit == null)
            {
                Debug.LogWarning(
                    "SignalWireBuilder: SparkCircuitSystem is NULL. " +
                    "Visual wires can still be drawn, but electrical " +
                    "topology cannot be committed.",
                    this);
            }
        }

        // ============================================================
        // PUBLIC START
        // ============================================================

        /// <summary>
        /// Starts a wire directly from a SparkTerminal.
        /// </summary>
        public bool BeginWire(
            SparkTerminal terminal,
            WirePolarity polarity)
        {
            if (terminal == null)
            {
                Debug.LogWarning(
                    "SignalWireBuilder: Start terminal is NULL.",
                    this);

                return false;
            }

            return BeginWire(
                terminal.transform.position,
                polarity,
                terminal);
        }

        /// <summary>
        /// Starts a wire from a position and SparkTerminal.
        /// </summary>
        public bool BeginWire(
            Vector3 startPosition,
            WirePolarity polarity,
            SparkTerminal terminal)
        {
            if (drawing)
            {
                if (logWireOperations)
                {
                    Debug.LogWarning(
                        "[WIRE] Already drawing.",
                        this);
                }

                return false;
            }

            if (terminal == null)
            {
                Debug.LogWarning(
                    "[WIRE] START TERMINAL = NULL",
                    this);

                return false;
            }

            if (!terminal.CanAccept(
                    connectionKind,
                    out string reason))
            {
                Debug.LogWarning(
                    $"[WIRE] START REJECTED: {reason}",
                    terminal);

                return false;
            }

            startTerminal = terminal;

            if (logWireOperations)
            {
                Debug.Log(
                    $"[WIRE] START = {terminal.name} | " +
                    $"Kind = {terminal.Kind} | " +
                    $"Polarity = {polarity}",
                    terminal);
            }

            return BeginWire(
                startPosition,
                polarity);
        }

        /// <summary>
        /// Starts a visual wire without an electrical terminal.
        /// Useful only for free visual wires.
        /// </summary>
        public bool BeginWire(
            Vector3 startPosition,
            WirePolarity polarity)
        {
            currentPolarity = polarity;

            if (breadboard == null)
            {
                Debug.LogError(
                    "SignalWireBuilder: Breadboard is NULL.",
                    this);

                return false;
            }

            if (pathManager == null)
            {
                Debug.LogError(
                    "SignalWireBuilder: Path Manager is NULL.",
                    this);

                return false;
            }

            if (!breadboard.TryGetNearestHole(
                    startPosition,
                    snapDistance,
                    out Vector3 hole,
                    out int column,
                    out int row))
            {
                Debug.LogWarning(
                    "[WIRE] No breadboard hole found at start.",
                    this);

                startTerminal = null;

                return false;
            }

            pathManager.CreateNextWire(
                polarity);

            currentPath =
                pathManager.CurrentPath;

            if (currentPath == null)
            {
                Debug.LogError(
                    "[WIRE] Path Manager failed to create path.",
                    this);

                startTerminal = null;

                return false;
            }

            currentPath.AddPoint(hole);
            currentPath.SetPreviewPoint(hole);

            lastColumn = column;
            lastRow = row;

            currentEnd = hole;

            drawing = true;

            Rebuild();

            return true;
        }

        // ============================================================
        // DEFAULT MOUSE START
        // ============================================================

        private void TryBeginWire()
        {
            Ray ray =
                targetCamera.ScreenPointToRay(
                    Input.mousePosition);

            if (!Physics.Raycast(
                    ray,
                    out RaycastHit hit,
                    terminalHitDistance,
                    terminalLayer))
            {
                return;
            }

            SparkTerminal terminal =
                hit.collider.GetComponentInParent<SparkTerminal>();

            if (terminal == null)
                return;

            WirePolarity polarity =
                DeterminePolarity(terminal);

            BeginWire(
                terminal.transform.position,
                polarity,
                terminal);
        }

        // ============================================================
        // POLARITY
        // ============================================================

        private WirePolarity DeterminePolarity(
            SparkTerminal terminal)
        {
            if (terminal == null)
                return WirePolarity.Positive;

            SparkElectricalComponent component =
                terminal.GetComponentInParent<SparkElectricalComponent>();

            if (component is SparkPowerSupply supply)
            {
                SparkTerminal[] terminals =
                    component.GetComponentsInChildren<SparkTerminal>(true);

                if (terminals.Length > 0 &&
                    terminals[0] == terminal)
                {
                    return WirePolarity.Positive;
                }

                if (terminals.Length > 1 &&
                    terminals[1] == terminal)
                {
                    return WirePolarity.Negative;
                }
            }

            return WirePolarity.Positive;
        }

        // ============================================================
        // DRAGGING
        // ============================================================

        private void UpdateDragging()
        {
            if (currentPath == null)
                return;

            if (breadboard == null)
                return;

            if (breadboard.GridOrigin == null)
                return;

            Ray ray =
                targetCamera.ScreenPointToRay(
                    Input.mousePosition);

            Transform gridTransform =
                breadboard.GridOrigin;

            Plane plane =
                new Plane(
                    gridTransform.up,
                    gridTransform.position);

            if (!plane.Raycast(
                    ray,
                    out float distance))
            {
                return;
            }

            Vector3 worldPosition =
                ray.GetPoint(distance);

            UpdateWire(worldPosition);
        }

        // ============================================================
        // UPDATE WIRE
        // ============================================================

        public void UpdateWire(
            Vector3 worldPosition)
        {
            if (!drawing)
                return;

            if (currentPath == null)
                return;

            currentEnd = worldPosition;

            if (breadboard.TryGetNearestHole(
                    worldPosition,
                    snapDistance,
                    out Vector3 hole,
                    out int column,
                    out int row))
            {
                currentEnd = hole;

                currentPath.SetPreviewPoint(hole);

                if (column != lastColumn ||
                    row != lastRow)
                {
                    currentPath.AddPoint(hole);

                    lastColumn = column;
                    lastRow = row;
                }
            }
            else
            {
                currentPath.SetPreviewPoint(
                    worldPosition);
            }

            Rebuild();
        }

        // ============================================================
        // FINISH
        // ============================================================

        public void EndWire(){
        if (!drawing)
        return;

    if (logWireOperations)
    {
        Debug.Log(
            "[WIRE] RELEASE",
            this);
    }

    SparkTerminal endTerminal =
        FindTerminalAtScreenPosition(
            Input.mousePosition);

    if (endTerminal == null)
    {
        Debug.LogWarning(
            "[WIRE] END TERMINAL = NULL\n" +
            "No SparkTerminal was detected under the mouse.",
            this);

        FinishVisualWireOnly();
        return;
    }

    if (logWireOperations)
    {
        Debug.Log(
            $"[WIRE] END = {endTerminal.name} | " +
            $"Kind = {endTerminal.Kind}",
            endTerminal);
    }

    if (startTerminal == null)
    {
        Debug.LogWarning(
            "[WIRE] CONNECTION NOT CREATED: " +
            "Start terminal is NULL.",
            this);

        FinishVisualWireOnly();
        return;
    }

    bool connected =
        TryCommitElectricalConnection(
            startTerminal,
            endTerminal);

    if (connected)
    {
        Debug.Log(
            $"[WIRE] TOPOLOGY SUCCESS: " +
            $"{startTerminal.name} ↔ {endTerminal.name}",
            this);
    }
    else
    {
        Debug.LogWarning(
            $"[WIRE] TOPOLOGY FAILED: " +
            $"{startTerminal.name} ↔ {endTerminal.name}",
            this);
    }

    FinishVisualWireOnly();
}


public void EndWire(Vector2 screenPosition)
{
    if (!drawing)
        return;

    //if (logWireOperations)
    //{
        Debug.Log(
            $"[WIRE] RELEASE | Screen={screenPosition}",
            this);
   // }

    SparkTerminal endTerminal =
        FindTerminalAtScreenPosition(screenPosition);

    if (endTerminal == null)
    {
        Debug.LogWarning(
            "[WIRE] END TERMINAL = NULL\n" +
            $"Screen Position = {screenPosition}",
            this);

        FinishVisualWireOnly();
        return;
    }

    Debug.Log(
        $"[WIRE] END = {endTerminal.name} | " +
        $"Kind = {endTerminal.Kind}",
        endTerminal);

    if (startTerminal == null)
    {
        Debug.LogWarning(
            "[WIRE] CONNECTION FAILED: Start terminal is NULL.",
            this);

        FinishVisualWireOnly();
        return;
    }

    bool connected =
        TryCommitElectricalConnection(
            startTerminal,
            endTerminal);

    if (connected)
    {
        Debug.Log(
            $"[WIRE] TOPOLOGY SUCCESS: " +
            $"{startTerminal.name} ↔ {endTerminal.name}",
            this);
    }
    else
    {
        Debug.LogWarning(
            $"[WIRE] TOPOLOGY FAILED: " +
            $"{startTerminal.name} ↔ {endTerminal.name}",
            this);
    }

    FinishVisualWireOnly();
}
private bool TryCommitElectricalConnection(
    SparkTerminal start,
    SparkTerminal end)
{
    // ============================================================
    // BASIC VALIDATION
    // ============================================================

    if (start == null)
    {
        Debug.LogWarning(
            "[WIRE] CONNECTION FAILED: Start terminal is NULL.",
            this);

        return false;
    }

    if (end == null)
    {
        Debug.LogWarning(
            "[WIRE] CONNECTION FAILED: End terminal is NULL.",
            this);

        return false;
    }

    if (circuit == null)
    {
        Debug.LogError(
            "[WIRE] CONNECTION FAILED: " +
            "SparkCircuitSystem is NULL.",
            this);

        return false;
    }

    // ============================================================
    // SAME TERMINAL
    // ============================================================

    if (rejectSameTerminal &&
        start == end)
    {
        Debug.LogWarning(
            $"[WIRE] CONNECTION REJECTED: " +
            $"Same terminal: {start.name}",
            this);

        return false;
    }

    // ============================================================
    // TERMINAL COMPATIBILITY
    // ============================================================

    if (!start.CanConnectTo(
            end,
            connectionKind,
            connectionDirection,
            out string reason))
    {
        Debug.LogWarning(
            $"[WIRE] CONNECTION REJECTED:\n" +
            $"{start.name} -> {end.name}\n" +
            $"Reason: {reason}",
            this);

        return false;
    }

    // ============================================================
    // CAPACITY
    // ============================================================

    if (rejectCapacity)
    {
        if (start.AtCapacity)
        {
            Debug.LogWarning(
                $"[WIRE] CONNECTION REJECTED:\n" +
                $"START AT CAPACITY: {start.name}",
                start);

            return false;
        }

        if (end.AtCapacity)
        {
            Debug.LogWarning(
                $"[WIRE] CONNECTION REJECTED:\n" +
                $"END AT CAPACITY: {end.name}",
                end);

            return false;
        }
    }

    // ============================================================
    // DUPLICATE
    // ============================================================

    if (rejectDuplicateConnection &&
        HasExistingConnection(start, end))
    {
        Debug.LogWarning(
            $"[WIRE] CONNECTION REJECTED:\n" +
            $"DUPLICATE: {start.name} ↔ {end.name}",
            this);

        return false;
    }

    // ============================================================
    // REGISTER REAL ELECTRICAL CONNECTION
    // ============================================================

    Debug.Log(
        $"[WIRE] REGISTERING ELECTRICAL CONNECTION:\n" +
        $"A = {start.name}\n" +
        $"B = {end.name}\n" +
        $"Kind = {connectionKind}\n" +
        $"Direction = {connectionDirection}",
        this);

    SparkCircuitConnection connection;

    bool created =
        circuit.TryCreateConnection(
            start,
            end,
            connectionKind,
            connectionDirection,
            out connection);

    // ============================================================
    // CREATION FAILED
    // ============================================================

    if (!created)
    {
        Debug.LogError(
            $"[WIRE] ELECTRICAL CONNECTION FAILED:\n" +
            $"{start.name} ↔ {end.name}",
            this);

        return false;
    }

    // ============================================================
    // SUCCESS
    // ============================================================

    Debug.Log(
        $"[WIRE] ELECTRICAL CONNECTION CREATED:\n" +
        $"ID = {connection.Id}\n" +
        $"A = {connection.A.name}\n" +
        $"B = {connection.B.name}\n" +
        $"Kind = {connection.Kind}\n" +
        $"Direction = {connection.Direction}\n" +
        $"Circuit Connections = {circuit.ConnectionCount}",
        this);

    // ============================================================
    // BIND VISUAL PATH TO ELECTRICAL CONNECTION
    // ============================================================

    if (pathManager != null &&
        currentPath != null)
    {
        pathManager.BindElectricalConnection(
            currentPath,
            connection);
    }

    // ============================================================
    // LEVEL 1
    // ============================================================

    /*if (level1Checker != null)
    {
        Debug.Log(
            $"[WIRE] LEVEL 1 CONNECTION REGISTERED:\n" +
            $"{start.name} ↔ {end.name}",
            this);
    }*/

    return true;
}
private SparkTerminal FindTerminalAtScreenPosition(
    Vector2 screenPosition)
{
    if (targetCamera == null)
        return null;

    Ray ray =
        targetCamera.ScreenPointToRay(
            screenPosition);

    RaycastHit[] hits =
        Physics.RaycastAll(
            ray,
            terminalHitDistance,
            terminalLayer,
            QueryTriggerInteraction.Collide);

    if (hits == null ||
        hits.Length == 0)
    {
        return null;
    }

    SparkTerminal closest = null;

    float closestDistance =
        float.MaxValue;

    for (int i = 0;
         i < hits.Length;
         i++)
    {
        Collider collider =
            hits[i].collider;

        if (collider == null)
            continue;

        SparkTerminal terminal =
            collider.GetComponentInParent<SparkTerminal>();

        if (terminal == null)
            continue;

        if (hits[i].distance < closestDistance)
        {
            closestDistance =
                hits[i].distance;

            closest =
                terminal;
        }
    }

    if (closest != null &&
        logWireOperations)
    {
        Debug.Log(
            $"[WIRE] TERMINAL HIT = {closest.name}",
            closest);
    }

    return closest;
}
        // ============================================================
        // DUPLICATE CHECK
        // ============================================================

        private bool HasExistingConnection(
            SparkTerminal a,
            SparkTerminal b)
        {
            if (circuit == null)
                return false;

            connectionBuffer.Clear();

            circuit.GetConnections(
                a,
                connectionBuffer);

            
            for (int i = 0;
     i < connectionBuffer.Count;
     i++)
        {
            SparkCircuitConnection connection =
                connectionBuffer[i];

            bool sameDirection =
                connection.A == a &&
                connection.B == b;

            bool reverseDirection =
                connection.A == b &&
                connection.B == a;

            if (sameDirection ||
                reverseDirection)
            {
                return true;
            }
        }

            return false;
        }

        public void UpdateWireFromScreenPosition(
    Vector2 screenPosition)
{
    if (!drawing)
        return;

    if (targetCamera == null)
        return;

    if (breadboard == null)
        return;

    if (breadboard.GridOrigin == null)
        return;

    Ray ray =
        targetCamera.ScreenPointToRay(
            screenPosition);

    Transform gridTransform =
        breadboard.GridOrigin;

    Plane plane =
        new Plane(
            gridTransform.up,
            gridTransform.position);

    if (!plane.Raycast(
            ray,
            out float distance))
    {
        return;
    }

    Vector3 worldPosition =
        ray.GetPoint(distance);

    UpdateWire(worldPosition);
}

        // ============================================================
        // FIND TERMINAL
        // ============================================================

        private SparkTerminal FindTerminalUnderMouse()
        {
            if (targetCamera == null)
                return null;

            Ray ray =
                targetCamera.ScreenPointToRay(
                    Input.mousePosition);

            RaycastHit[] hits =
                Physics.RaycastAll(
                    ray,
                    terminalHitDistance,
                    terminalLayer);

            if (hits == null ||
                hits.Length == 0)
            {
                return null;
            }

            SparkTerminal closest = null;

            float closestDistance =
                float.MaxValue;

            for (int i = 0;
                 i < hits.Length;
                 i++)
            {
                Collider collider =
                    hits[i].collider;

                if (collider == null)
                    continue;

                SparkTerminal terminal =
                    collider.GetComponentInParent<SparkTerminal>();

                if (terminal == null)
                    continue;

                if (hits[i].distance < closestDistance)
                {
                    closestDistance =
                        hits[i].distance;

                    closest =
                        terminal;
                }
            }

            if (closest != null &&
                logWireOperations)
            {
                Debug.Log(
                    $"[WIRE] TERMINAL HIT = {closest.name}",
                    closest);
            }

            return closest;
        }

        // ============================================================
        // FINISH VISUAL WIRE
        // ============================================================

        private void FinishVisualWireOnly()
        {
            drawing = false;

            if (currentPath != null)
            {
                currentPath.ClearPreview();
            }

            Rebuild();

            if (pathManager != null)
            {
                pathManager.FinishCurrentPath();
            }

            currentPath = null;
            startTerminal = null;

            lastColumn = -1;
            lastRow = -1;

            currentEnd = Vector3.zero;
        }

        // ============================================================
        // CANCEL
        // ============================================================

        public void CancelWire()
        {
            if (!drawing)
                return;

            if (logWireOperations)
                Debug.Log(
                    "[WIRE] CANCEL",
                    this);

            drawing = false;

            if (currentPath != null)
            {
                currentPath.ClearPreview();
            }

            Rebuild();

            if (pathManager != null)
            {
                pathManager.FinishCurrentPath();
            }

            currentPath = null;
            startTerminal = null;

            lastColumn = -1;
            lastRow = -1;

            currentEnd = Vector3.zero;
        }

        // ============================================================
        // MESH
        // ============================================================

        private void Rebuild()
        {
            if (pathManager == null)
                return;

            SignalPathMesh mesh =
                pathManager.CurrentMesh;

            if (mesh == null)
                return;

            mesh.Rebuild();
        }

        // ============================================================
        // DEBUG
        // ============================================================

        [ContextMenu("Debug Wire State")]
        private void DebugWireState()
        {
            Debug.Log(
                $"[WIRE STATE]\n" +
                $"Drawing: {drawing}\n" +
                $"Polarity: {currentPolarity}\n" +
                $"Start: {(startTerminal != null ? startTerminal.name : "NULL")}\n" +
                $"Path: {(currentPath != null ? "VALID" : "NULL")}\n" +
                $"Circuit: {(circuit != null ? "VALID" : "NULL")}",
                this);
        }
    }
}