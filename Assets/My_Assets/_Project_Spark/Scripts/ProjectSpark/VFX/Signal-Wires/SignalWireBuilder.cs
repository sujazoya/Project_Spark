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
    /// Selects how a visual/electrical wire is constructed.
    /// Value 0 deliberately preserves all existing scene and prefab settings.
    /// </summary>
    public enum WireDrawingMode
    {
        BreadboardLegacy = 0,
        TerminalToTerminal = 1
    }

    public enum TerminalWireInputStyle
    {
        DragAndRelease = 0,
        ClickToConnect = 1
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

        [Header("AAA Wire Drawing Mode (legacy is unchanged)")]
        [SerializeField] private WireDrawingMode wireDrawingMode = WireDrawingMode.BreadboardLegacy;

        [Header("Terminal To Terminal - Input")]
        [SerializeField] private TerminalWireInputStyle terminalInputStyle = TerminalWireInputStyle.DragAndRelease;
        [SerializeField] private bool avoidStartingOverUI = true;
        [SerializeField] private bool allowSphereSnap = true;
        [SerializeField, Min(0.001f)] private float terminalSphereSnapRadius = 0.035f;
        [SerializeField, Range(1f, 100f)] private float terminalScreenSnapPixels = 28f;
        [SerializeField, Min(0.0001f)] private float minimumTerminalSeparation = 0.005f;
        [SerializeField, Min(0.00001f)] private float previewMoveThreshold = 0.0005f;

        [Header("Terminal To Terminal - Cable")]
        // FreehandDrag follows the actual mouse route instead of an automatic arc.
        [SerializeField] private TerminalCableRouting terminalCableRouting = TerminalCableRouting.FreehandDrag;
        [SerializeField] private bool followMovingTerminals = true;

        [Header("Terminal To Terminal - Mouse Drag Waypoints")]
        [Tooltip("Minimum mouse movement before storing another bend. Small = accurate but heavier meshes.")]
        [SerializeField, Range(2f, 50f)] private float dragPointSpacingPixels = 12f;
        [Tooltip("Minimum world-space spacing between stored route points (Unity units).")]
        [SerializeField, Min(0.0001f)] private float dragPointSpacingWorld = 0.01f;
        [Tooltip("Safety cap on saved route points. Preview remains responsive after the cap.")]
        [SerializeField, Range(8, 256)] private int maxDragRoutePoints = 160;

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

        // Preallocated hit buffers; the new picking path is allocation-free.
        private readonly RaycastHit[] terminalRayHits = new RaycastHit[64];
        private readonly RaycastHit[] terminalSphereHits = new RaycastHit[64];
        private SparkTerminal hoveredTerminal;

        // Freehand sampled points are committed to SignalPath itself; the
        // manager, scanner and other visual consumers retain the real route.
        private int dragRoutePoints;
        private Vector3 lastDragRoutePosition;
        private Vector2 lastDragRouteScreen;
        private bool lastDragRouteHasScreen;

        public event Action<SparkTerminal> HoverTerminalChanged;
        public event Action<SparkTerminal, SparkTerminal, SignalPath> TerminalWireConnected;

        private WirePolarity currentPolarity;

        private SparkTerminal startTerminal;

        private bool drawing;
        private static readonly HashSet<SignalWireBuilder> activeDrawings = new HashSet<SignalWireBuilder>();

        public static bool AnyWireDrawing
        {
            get
            {
                foreach (var builder in activeDrawings)
                    if (builder != null && builder.isActiveAndEnabled && builder.drawing) return true;
                return false;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetActiveDrawings() { activeDrawings.Clear(); }

        private void SetDrawing(bool value)
        {
            drawing = value;
            if (value) activeDrawings.Add(this);
            else activeDrawings.Remove(this);
        }

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

        public WireDrawingMode DrawingMode => wireDrawingMode;
        public TerminalWireInputStyle TerminalInputStyle => terminalInputStyle;
        public TerminalCableRouting CableRouting => terminalCableRouting;
        public int DragRoutePointCount => dragRoutePoints;
        public SparkTerminal HoveredTerminal => hoveredTerminal;

        /// <summary>Change the route for the next created wire.</summary>
        public void SetTerminalCableRouting(TerminalCableRouting route)
        {
            terminalCableRouting = route;
        }

        /// <summary>Switch modes without changing any existing method signatures.</summary>
        public void SetWireDrawingMode(WireDrawingMode newMode)
        {
            if (wireDrawingMode == newMode) return;
            if (drawing) CancelWire();
            wireDrawingMode = newMode;
            SetHoveredTerminal(null);
            ValidateReferences();
        }

        public void SetTerminalInputStyle(TerminalWireInputStyle style)
        {
            terminalInputStyle = style;
        }

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
            SetHoveredTerminal(null);
        }

       
        private void Update()
{
    if (wireDrawingMode == WireDrawingMode.TerminalToTerminal)
    {
        UpdateTerminalToTerminalInput();
        return;
    }

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



#region Wire Completion

/// <summary>
/// Ends construction using the current mouse position.
/// Used by standalone mouse input.
/// </summary>
public void EndWire()
{
    TryEndWire((Vector2)Input.mousePosition);
}

/// <summary>
/// Ends construction using a supplied screen position.
/// Preserves compatibility with existing tool callers.
/// </summary>
public void EndWire(Vector2 screenPosition)
{
    TryEndWire(screenPosition);
}

/// <summary>
/// Attempts to create a real electrical connection.
/// Returns true only when SparkCircuitSystem
/// successfully registers the connection.
/// </summary>


public bool TryEndWire(Vector2 screenPosition)
{
   if (wireDrawingMode == WireDrawingMode.TerminalToTerminal)
        return TryFinishTerminalWire(FindTerminalForNewMode(screenPosition));

    if (!drawing)
        return false;

    // =========================================================
    // 1. RESOLVE DESTINATION TERMINAL
    // =========================================================

    SparkTerminal endTerminal =
        FindTerminalAtScreenPosition(screenPosition);

    if (startTerminal == null ||
        endTerminal == null ||
        currentPath == null)
    {
        if (logWireOperations)
        {
            Debug.LogWarning(
                "[WIRE] Connection cancelled: " +
                "Missing start terminal, end terminal, " +
                "or visual path.",
                this);
        }

        DiscardVisualWire();
        return false;
    }

    // =========================================================
    // 2. VALIDATE AND COMMIT ELECTRICAL CONNECTION
    // =========================================================

    bool connected = TryCommitElectricalConnection(
        startTerminal,
        endTerminal);

    if (!connected)
    {
        if (logWireOperations)
        {
            Debug.LogWarning(
                $"[WIRE] Connection rejected: " +
                $"{startTerminal.name} -> {endTerminal.name}",
                this);
        }

        DiscardVisualWire();
        return false;
    }

    // =========================================================
    // 3. COMMIT PHYSICAL CABLE ENDPOINT
    // =========================================================

    // The temporary preview must not become a permanent
    // routing point.
    currentPath.ClearPreview();

    // Add the exact destination terminal position.
    currentPath.AddPoint(
        endTerminal.transform.position);

    // Generate the final rounded cable mesh.
    Rebuild();

    // =========================================================
    // 4. REPORT SUCCESS
    // =========================================================

    if (logWireOperations)
    {
        Debug.Log(
            $"[WIRE] CONNECTED\n" +
            $"Start: {startTerminal.name}\n" +
            $"End: {endTerminal.name}\n" +
            $"Path Points: {currentPath.PointCount}\n" +
            $"Circuit Connections: " +
            $"{(circuit != null ? circuit.ConnectionCount : 0)}",
            this);
    }

    // =========================================================
    // 5. PRESERVE FINISHED CABLE
    // =========================================================

    // This must release the builder's active state,
    // NOT delete the completed cable.
    CompleteVisualWire();

    return true;
}



private void CompleteVisualWire()
{
    if (currentPath != null)
    {
        currentPath.ClearPreview();
        Rebuild();
    }

    if (pathManager != null)
    {
        pathManager.FinishCurrentPath();
    }

    SetDrawing(false);
    currentPath = null;
    startTerminal = null;

    lastColumn = -1;
    lastRow = -1;

    currentEnd = Vector3.zero;
    dragRoutePoints = 0;
    lastDragRouteHasScreen = false;
}

private void DiscardVisualWire()
{
    SignalPath rejectedPath = currentPath;

    if (pathManager != null && rejectedPath != null)
    {
        pathManager.RemovePath(rejectedPath);
    }

    ResetWireState();
}

private void ResetWireState()
{
    SetDrawing(false);

    if (pathManager != null)
    {
        pathManager.FinishCurrentPath();
    }

    currentPath = null;
    startTerminal = null;

    lastColumn = -1;
    lastRow = -1;

    currentEnd = Vector3.zero;
    dragRoutePoints = 0;
    lastDragRouteHasScreen = false;
}

#endregion


        // ============================================================
        // VALIDATION
        // ============================================================

        private void ValidateReferences()
        {
            // The new mode must work with BreadboardGrid completely unassigned.
            if (wireDrawingMode == WireDrawingMode.BreadboardLegacy && breadboard == null)
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
            if (wireDrawingMode == WireDrawingMode.TerminalToTerminal)
                return BeginTerminalWireAt(startPosition, polarity);

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

            SetDrawing(true);

            Rebuild();

            return true;
        }

        // ============================================================
        // DEFAULT MOUSE START
        // ============================================================

        private void TryBeginWire()
        {
            if (wireDrawingMode == WireDrawingMode.TerminalToTerminal)
            {
                SparkTerminal selectedTerminal = FindTerminalForNewMode((Vector2)Input.mousePosition);
                if (selectedTerminal != null)
                    BeginWire(selectedTerminal, DeterminePolarity(selectedTerminal));
                return;
            }

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
        return WirePolarity.Neutral;

    switch (terminal.EffectivePolarity)
    {
        case SparkTerminalPolarity.Positive:
            return WirePolarity.Positive;

        case SparkTerminalPolarity.Negative:
            return WirePolarity.Negative;

        default:
            return WirePolarity.Neutral;
    }
}


        // ============================================================
        // DRAGGING
        // ============================================================

        private void UpdateDragging()
        {
            if (wireDrawingMode == WireDrawingMode.TerminalToTerminal)
            {
                UpdateWireFromScreenPosition((Vector2)Input.mousePosition);
                return;
            }

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

       
public void UpdateWire(Vector3 worldPosition)
{
    if (!drawing || currentPath == null)
        return;

    if (wireDrawingMode == WireDrawingMode.TerminalToTerminal)
    {
        // External gameplay tools may drive a full 3D freehand route using
        // UpdateWire(worldPosition), without mouse input or a breadboard.
        UpdateTerminalPreview(worldPosition,
            terminalCableRouting == TerminalCableRouting.FreehandDrag &&
            terminalInputStyle == TerminalWireInputStyle.DragAndRelease);
        return;
    }

    currentEnd = worldPosition;

    // Snap the preview endpoint when close to a hole.
    // Do not permanently add every visited hole.
    if (breadboard != null &&
        breadboard.TryGetNearestHole(
            worldPosition,
            snapDistance,
            out Vector3 hole,
            out int column,
            out int row))
    {
        currentEnd = hole;
        lastColumn = column;
        lastRow = row;
    }

    // Only update the temporary endpoint.
    currentPath.SetPreviewPoint(currentEnd);

    Rebuild();
}

       

        // ============================================================
        // FINISH
        // ============================================================


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

    if (wireDrawingMode == WireDrawingMode.TerminalToTerminal)
    {
        UpdateTerminalPreviewFromScreen(screenPosition);
        return;
    }

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
            SetDrawing(false);

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
    {
        Debug.Log("[WIRE] CANCEL", this);
    }

    DiscardVisualWire();
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
        // NEW: TERMINAL -> TERMINAL (NO BREADBOARD DEPENDENCY)
        // ============================================================

        private void UpdateTerminalToTerminalInput()
        {
            if (externalInputControl) return;
            if (targetCamera == null) targetCamera = Camera.main;
            if (targetCamera == null) return;

            // Cancel first: do not commit on the same frame as Escape/right-click.
            if (drawing && (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape)))
            {
                CancelWire();
                SetHoveredTerminal(null);
                return;
            }

            Vector2 cursor = (Vector2)Input.mousePosition;

            if (!drawing)
            {
                if (avoidStartingOverUI && IsPointerOverUI())
                {
                    SetHoveredTerminal(null);
                    return;
                }

                SparkTerminal candidate = FindTerminalForNewMode(cursor);
                SetHoveredTerminal(candidate);
                if (Input.GetMouseButtonDown(0) && candidate != null)
                    BeginWire(candidate, DeterminePolarity(candidate));
                return;
            }

            UpdateTerminalPreviewFromScreen(cursor);

            if (terminalInputStyle == TerminalWireInputStyle.DragAndRelease)
            {
                if (Input.GetMouseButtonUp(0))
                    TryEndWire(cursor);
            }
            else if (Input.GetMouseButtonDown(0) &&
                     (!avoidStartingOverUI || !IsPointerOverUI()))
            {
                TryEndWire(cursor);
            }
        }

        private bool BeginTerminalWireAt(Vector3 startPosition, WirePolarity polarity)
        {
            if (drawing || startTerminal == null)
                return false;

            if (pathManager == null)
            {
                Debug.LogError("[WIRE AAA] SignalPath_Manager is required.", this);
                startTerminal = null;
                return false;
            }
            if (circuit == null)
            {
                circuit = pathManager.Circuit;
                if (circuit == null) circuit = GetComponentInParent<SparkCircuitSystem>();
                if (circuit == null) circuit = FindFirstObjectByType<SparkCircuitSystem>();
            }
            if (circuit == null)
            {
                Debug.LogError("[WIRE AAA] SparkCircuitSystem is required for real wires.", this);
                startTerminal = null;
                return false;
            }
            if (pathManager.CurrentPath != null)
            {
                Debug.LogWarning("[WIRE AAA] Another wire is being edited by this manager.", this);
                startTerminal = null;
                return false;
            }

            currentPolarity = polarity;
            SignalPathMesh wireMesh = pathManager.CreateNextWire(polarity);
            currentPath = pathManager.CurrentPath;
            if (wireMesh == null || currentPath == null)
            {
                if (currentPath != null) pathManager.RemovePath(currentPath);
                else pathManager.FinishCurrentPath();
                currentPath = null;
                startTerminal = null;
                return false;
            }

            // The start point belongs to the terminal, not any breadboard hole.
            wireMesh.ConfigureTerminalCable(terminalCableRouting);
            currentPath.AddPoint(startTerminal.transform.position);
            currentEnd = startTerminal.transform.position;
            currentPath.SetPreviewPoint(currentEnd);
            dragRoutePoints = 0;
            lastDragRoutePosition = currentEnd;
            lastDragRouteScreen = targetCamera != null
                ? (Vector2)targetCamera.WorldToScreenPoint(currentEnd)
                : Vector2.zero;
            lastDragRouteHasScreen = targetCamera != null;
            lastColumn = lastRow = -1;
            SetDrawing(true);
            wireMesh.Rebuild();
            return true;
        }

        private void UpdateTerminalPreviewFromScreen(Vector2 screenPosition)
        {
            if (!drawing || currentPath == null || startTerminal == null) return;
            if (targetCamera == null) targetCamera = Camera.main;
            if (targetCamera == null) return;

            SparkTerminal candidate = FindTerminalForNewMode(screenPosition);
            SetHoveredTerminal(candidate);
            if (candidate != null && candidate != startTerminal)
            {
                UpdateTerminalPreview(candidate.transform.position);
                return;
            }

            // Screen-projected plane passing through Terminal 1: does not
            // depend on any grid orientation, hole, or ground collider.
            Ray ray = targetCamera.ScreenPointToRay(screenPosition);
            Plane plane = new Plane(targetCamera.transform.forward, startTerminal.transform.position);
            if (plane.Raycast(ray, out float distance) && distance >= 0f)
            {
                bool sampleMouseDrag = terminalCableRouting == TerminalCableRouting.FreehandDrag &&
                    terminalInputStyle == TerminalWireInputStyle.DragAndRelease &&
                    (externalInputControl || Input.GetMouseButton(0));
                UpdateTerminalPreview(ray.GetPoint(distance), sampleMouseDrag,
                    screenPosition, true);
            }
        }

        /// <summary>
        /// Records a sparse polyline from the actual drag, not a generated arc.
        /// SignalPath.AddPoint is append-only in the supplied API: sample at a
        /// controlled rate and simplify visually in SignalPathMesh.
        /// </summary>
        private bool RecordDragRoutePoint(Vector3 worldPosition, Vector2 screenPoint,
            bool screenPointAvailable)
        {
            if (terminalCableRouting != TerminalCableRouting.FreehandDrag ||
                terminalInputStyle != TerminalWireInputStyle.DragAndRelease ||
                currentPath == null || startTerminal == null ||
                dragRoutePoints >= maxDragRoutePoints)
                return false;

            float minWorldSq = dragPointSpacingWorld * dragPointSpacingWorld;
            if ((worldPosition - lastDragRoutePosition).sqrMagnitude < minWorldSq)
                return false;

            if (screenPointAvailable && lastDragRouteHasScreen &&
                (screenPoint - lastDragRouteScreen).sqrMagnitude <
                dragPointSpacingPixels * dragPointSpacingPixels)
                return false;

            currentPath.AddPoint(worldPosition);
            lastDragRoutePosition = worldPosition;
            if (screenPointAvailable)
            {
                lastDragRouteScreen = screenPoint;
                lastDragRouteHasScreen = true;
            }
            else
            {
                lastDragRouteHasScreen = false;
            }
            dragRoutePoints++;
            return true;
        }

        private void UpdateTerminalPreview(Vector3 newEnd, bool recordRoute = false,
            Vector2 screenPoint = default(Vector2), bool hasScreenPoint = false)
        {
            if (!drawing || currentPath == null) return;
            bool recordedPoint = recordRoute &&
                RecordDragRoutePoint(newEnd, screenPoint, hasScreenPoint);
            if (!recordedPoint &&
                (currentEnd - newEnd).sqrMagnitude <
                previewMoveThreshold * previewMoveThreshold)
                return;
            currentEnd = newEnd;
            currentPath.SetPreviewPoint(currentEnd);
            Rebuild();
        }

        /// <summary>
        /// Complete the wire against a known terminal. Safe for external tools:
        /// no mouse, physics query, or BreadboardGrid is required.
        /// </summary>
        public bool TryFinishTerminalWire(SparkTerminal terminal2)
        {
            if (wireDrawingMode != WireDrawingMode.TerminalToTerminal || !drawing)
                return false;

            SparkTerminal terminal1 = startTerminal;
            SignalPath finishedPath = currentPath;
            if (terminal1 == null || terminal2 == null || finishedPath == null)
            {
                if (logWireOperations)
                    Debug.LogWarning("[WIRE AAA] No valid destination terminal. Preview discarded.", this);
                DiscardVisualWire();
                SetHoveredTerminal(null);
                return false;
            }

            if ((terminal1.transform.position - terminal2.transform.position).sqrMagnitude <
                minimumTerminalSeparation * minimumTerminalSeparation)
            {
                if (logWireOperations) Debug.LogWarning("[WIRE AAA] Terminals are too close.", this);
                DiscardVisualWire();
                SetHoveredTerminal(null);
                return false;
            }

            // TryCommitElectricalConnection performs the original compatibility,
            // capacity and duplicate checks, then creates and binds topology.
            // No permanent mesh is retained if the electrical commit fails.
            if (!TryCommitElectricalConnection(terminal1, terminal2))
            {
                DiscardVisualWire();
                SetHoveredTerminal(null);
                return false;
            }

            finishedPath.ClearPreview();
            finishedPath.AddPoint(terminal2.transform.position);
            SignalPathMesh mesh = pathManager != null ? pathManager.GetMeshForPath(finishedPath) : null;
            if (mesh != null)
            {
                mesh.BindTerminalAnchors(terminal1.transform, terminal2.transform, followMovingTerminals);
                mesh.Rebuild();
            }

            if (logWireOperations)
                Debug.Log($"[WIRE AAA] CONNECTED: {terminal1.name} -> {terminal2.name}", this);
            TerminalWireConnected?.Invoke(terminal1, terminal2, finishedPath);
            CompleteVisualWire();
            SetHoveredTerminal(null);
            return true;
        }

        /// <summary>Creates a fully registered connection from code, with zero mouse input.</summary>
        public bool TryConnectTerminals(SparkTerminal terminal1, SparkTerminal terminal2)
        {
            if (wireDrawingMode != WireDrawingMode.TerminalToTerminal || drawing ||
                terminal1 == null || terminal2 == null)
                return false;
            if (!BeginWire(terminal1, DeterminePolarity(terminal1)))
                return false;
            return TryFinishTerminalWire(terminal2);
        }

        private SparkTerminal FindTerminalForNewMode(Vector2 screenPosition)
        {
            if (targetCamera == null) targetCamera = Camera.main;
            if (targetCamera == null) return null;

            Ray ray = targetCamera.ScreenPointToRay(screenPosition);
            int count = Physics.RaycastNonAlloc(ray, terminalRayHits, terminalHitDistance,
                terminalLayer, QueryTriggerInteraction.Collide);
            SparkTerminal nearest = null;
            float distance = float.PositiveInfinity;

            for (int i = 0; i < count; i++)
            {
                Collider hitCollider = terminalRayHits[i].collider;
                if (hitCollider == null) continue;
                SparkTerminal terminal = hitCollider.GetComponentInParent<SparkTerminal>();
                if (terminal != null && terminalRayHits[i].distance < distance)
                {
                    nearest = terminal;
                    distance = terminalRayHits[i].distance;
                }
            }
            if (nearest != null || !allowSphereSnap) return nearest;

            // Forgiving pointer selection: only choose terminals that also
            // project within the allowed pixel distance from the cursor.
            count = Physics.SphereCastNonAlloc(ray, terminalSphereSnapRadius, terminalSphereHits,
                terminalHitDistance, terminalLayer, QueryTriggerInteraction.Collide);
            float maxSquarePixels = terminalScreenSnapPixels * terminalScreenSnapPixels;
            float bestSquarePixels = maxSquarePixels;
            for (int i = 0; i < count; i++)
            {
                Collider hitCollider = terminalSphereHits[i].collider;
                if (hitCollider == null) continue;
                SparkTerminal terminal = hitCollider.GetComponentInParent<SparkTerminal>();
                if (terminal == null) continue;
                Vector3 projected = targetCamera.WorldToScreenPoint(terminal.transform.position);
                if (projected.z <= 0f) continue;
                float pixelSquare = ((Vector2)projected - screenPosition).sqrMagnitude;
                if (pixelSquare <= bestSquarePixels)
                {
                    bestSquarePixels = pixelSquare;
                    nearest = terminal;
                }
            }
            return nearest;
        }

        private static bool IsPointerOverUI()
        {
            UnityEngine.EventSystems.EventSystem events = UnityEngine.EventSystems.EventSystem.current;
            return events != null && events.IsPointerOverGameObject();
        }

        private void SetHoveredTerminal(SparkTerminal terminal)
        {
            if (hoveredTerminal == terminal) return;
            hoveredTerminal = terminal;
            HoverTerminalChanged?.Invoke(terminal);
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
