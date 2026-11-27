using ProjectSpark.Gameplay;
using System;
using System.Collections.Generic;
using AAAUI.VFX;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;

namespace ProjectSpark.Gameplay
{
    /// <summary>
    /// Non-destructive, runtime route editor for finished SignalPath_Manager wires.
    /// Only SignalPathMesh visual control points are modified. SparkCircuitSystem
    /// connections, SparkTerminal references and stable wire IDs are untouched.
    ///
    /// Requires legacy UnityEngine.Input or Active Input Handling = Both.
    /// Select from SparkWireListUI or click near a cable while Edit Mode is on.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SparkWireRegistry))]
    [DefaultExecutionOrder(250)]
    public sealed class SparkWireRouteEditor : MonoBehaviour
    {
        public enum RouteDragPlane
        {
            CameraFacing = 0,
            WorldXZ = 1,
            WorldXY = 2,
            WorldYZ = 3
        }

        [Header("Existing ProjectSpark Systems")]
        [SerializeField] private SparkWireRegistry registry;
        [SerializeField] private SignalWireBuilder builder;
        [SerializeField] private Camera sceneCamera;

        [Header("Scene Editing")]
        [SerializeField] private bool editMode;
        [SerializeField] private bool suspendStandaloneWireDrawing = true;
        [SerializeField] private RouteDragPlane dragPlane = RouteDragPlane.CameraFacing;
        [SerializeField, Range(5f, 35f)] private float pointPickPixels = 17f;
        [SerializeField, Range(4f, 35f)] private float wirePickPixels = 14f;
        [SerializeField, Range(6, 28)] private int handleSizePixels = 12;
        [SerializeField, Min(0.001f)] private float highlightWidth = 0.025f;
        [SerializeField, Range(2, 512)] private int maxControlPoints = 256;

        [Header("Precision Tools")]
        [SerializeField] private bool snapToWorldGrid;
        [SerializeField, Min(0.0001f)] private float worldGridSize = 0.025f;
        [SerializeField, Min(0.0001f)] private float depthStep = 0.0125f;
        [SerializeField, Range(1, 100)] private int historyStepsPerWire = 40;
        [SerializeField] private bool showEditGizmos = true;

        [Header("Optional Appearance")]
        [SerializeField] private Material optionalGizmoMaterial;
        [SerializeField] private Material optionalHighlightMaterial;

        private static readonly Color EndpointColor = new Color(1f, 0.72f, 0.25f, 1f);
        private static readonly Color HandleColor = new Color(0.22f, 0.9f, 1f, 1f);
        private static readonly Color SelectedHandleColor = new Color(0.96f, 0.98f, 0.27f, 1f);

        private sealed class RouteSnapshot
        {
            public bool HasOverride;
            public Vector3[] Points;
        }

        private sealed class RouteHistory
        {
            public readonly List<RouteSnapshot> Undo = new List<RouteSnapshot>();
            public readonly List<RouteSnapshot> Redo = new List<RouteSnapshot>();
        }

        private readonly Dictionary<string, RouteHistory> histories =
            new Dictionary<string, RouteHistory>();
        private readonly List<Transform> handles = new List<Transform>();
        private readonly List<Renderer> handleRenderers = new List<Renderer>();
        private MaterialPropertyBlock handleBlock;
        private GameObject gizmoRoot;
        private LineRenderer selectionLine;
        private Material generatedHandleMaterial;
        private Material generatedLineMaterial;
        private bool ownsBuilderInput;
        private bool previousBuilderInput;
        private bool dragging;
        private bool movedDuringDrag;
        private bool pendingSegmentDrag;
        private string pendingWireId;
        private Vector2 pendingMouse;
        private Vector3 pendingWorld;
        private int selectedPointIndex = -1;
        private Vector2 dragStartScreen;
        private Vector3 dragStartPosition;
        private Vector3 dragOffset;
        private Plane currentDragPlane;
        private RouteSnapshot dragOriginal;
        private string dragWireId;
        private string displayedWireId;

        public bool IsEditing => editMode;
        public bool SnapToGrid => snapToWorldGrid;
        public int SelectedPointIndex => selectedPointIndex;
        public string SelectedWireId => registry != null ? registry.SelectedWireId : null;
        public int UndoCount => GetHistory(SelectedWireId, false)?.Undo.Count ?? 0;
        public int RedoCount => GetHistory(SelectedWireId, false)?.Redo.Count ?? 0;
        public event Action EditorStateChanged;

        private void Awake()
        {
            ResolveReferences();
            // Never silently capture gameplay input on scene start.
            editMode = false;
        }

        private void OnEnable()
        {
            ResolveReferences();
            if (registry != null)
            {
                registry.SelectionChanged += OnSelectionChanged;
                registry.WiresChanged += OnRegistryChanged;
            }
        }

        private void OnDisable()
        {
            if (registry != null)
            {
                registry.SelectionChanged -= OnSelectionChanged;
                registry.WiresChanged -= OnRegistryChanged;
            }
            DisableEditing();
            DestroyGizmos();
        }

        private void OnDestroy() { DestroyGizmos(); }

        private void ResolveReferences()
        {
            if (registry == null) registry = GetComponent<SparkWireRegistry>();
            if (registry == null) registry = FindFirstObjectByType<SparkWireRegistry>();
            if (builder == null) builder = FindFirstObjectByType<SignalWireBuilder>();
            if (sceneCamera == null) sceneCamera = Camera.main;
        }

        public void EnableEditing()
        {
            if (editMode) return;
            ResolveReferences();
            if (registry == null || sceneCamera == null)
            {
                Debug.LogWarning("[WIRE EDITOR] Registry or gameplay camera is missing.", this);
                return;
            }
            if (suspendStandaloneWireDrawing && builder != null)
            {
                previousBuilderInput = builder.ExternalInputControl;
                // Cancels an unfinished preview; does NOT delete finished wires.
                builder.SetExternalInputControl(true);
                ownsBuilderInput = true;
            }
            editMode = true;
            SyncSceneGizmos();
            EditorStateChanged?.Invoke();
        }

        public void DisableEditing()
        {
            if (dragging) CancelDragging();
            pendingSegmentDrag = false;
            bool wasEditing = editMode;
            editMode = false;
            selectedPointIndex = -1;
            displayedWireId = null;
            if (gizmoRoot != null) gizmoRoot.SetActive(false);
            if (ownsBuilderInput)
            {
                if (builder != null) builder.SetExternalInputControl(previousBuilderInput);
                ownsBuilderInput = false;
            }
            if (wasEditing) EditorStateChanged?.Invoke();
        }

        public void ToggleEditingSelected()
        {
            if (editMode) DisableEditing();
            else EnableEditing();
        }

        public void SetEditMode(bool enabled)
        {
            if (enabled) EnableEditing();
            else DisableEditing();
        }

        public bool SelectWire(string wireId)
        {
            if (registry == null) ResolveReferences();
            return registry != null && registry.SelectWire(wireId);
        }

        private void OnSelectionChanged(string id)
        {
            if (dragging) CancelDragging();
            pendingSegmentDrag = false;
            selectedPointIndex = -1;
            displayedWireId = null;
            SyncSceneGizmos();
            EditorStateChanged?.Invoke();
        }

        private void OnRegistryChanged()
        {
            if (!editMode || registry == null) return;
            if (!string.IsNullOrEmpty(registry.SelectedWireId))
            {
                SparkWireRecord record;
                if (!registry.TryGetWire(registry.SelectedWireId, out record))
                {
                    if (dragging) CancelDragging();
                    selectedPointIndex = -1;
                }
            }
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        private void OnApplicationPause(bool paused)
        {
            if (paused) CancelTouchEdit();
        }

        private void CancelTouchEdit()
        {
            if (dragging) CancelDragging();
            pendingSegmentDrag = false;
        }
#endif

        private void Update()
        {
            if (!editMode || registry == null || sceneCamera == null) return;
#if UNITY_ANDROID && !UNITY_EDITOR
            if (SparkBenchPointer.Cancelled || SparkBenchPointer.IsOverUI() || !Application.isFocused)
            {
                CancelTouchEdit();
                return;
            }
#endif
            bool ctrl = SparkBenchPointer.KeyHeld(KeyCode.LeftControl) || SparkBenchPointer.KeyHeld(KeyCode.RightControl);
            if (ctrl && SparkBenchPointer.KeyDown(KeyCode.Z)) { UndoRoute(); return; }
            if (ctrl && SparkBenchPointer.KeyDown(KeyCode.Y)) { RedoRoute(); return; }
            if (SparkBenchPointer.KeyDown(KeyCode.Escape))
            {
                if (dragging) CancelDragging();
                else DisableEditing();
                return;
            }
            if (SparkBenchPointer.KeyDown(KeyCode.G))
            {
                snapToWorldGrid = !snapToWorldGrid;
                EditorStateChanged?.Invoke();
            }
            if (SparkBenchPointer.KeyDown(KeyCode.Delete) || SparkBenchPointer.KeyDown(KeyCode.Backspace))
            {
                RemoveSelectedPoint();
                return;
            }
            if (dragging)
            {
                if (SparkBenchPointer.Released) FinishDragging();
                else if (SparkBenchPointer.Held) ContinueDragging();
                return;
            }
            if (pendingSegmentDrag)
            {
                if (SparkBenchPointer.Released) pendingSegmentDrag = false;
                else if (SparkBenchPointer.Held &&
                    ((Vector2)SparkBenchPointer.Position - pendingMouse).sqrMagnitude >= 36f)
                {
                    pendingSegmentDrag = false;
                    if (registry.TryGetWire(pendingWireId, out SparkWireRecord pendingRecord))
                    {
                        int at = FindClosestInsertionIndex(pendingRecord, pendingWorld);
                        if (InsertPoint(pendingWireId, at, pendingWorld))
                        {
                            selectedPointIndex = at;
                            StartDragging(pendingWireId, at, pendingWorld, pendingMouse);
                            ContinueDragging();
                        }
                    }
                }
                return;
            }
#if !(UNITY_ANDROID && !UNITY_EDITOR)
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;
#endif

            // Alt + wheel: depth-adjust the selected interior waypoint in 3D.
            float scroll = SparkBenchPointer.ScrollY;
            if (SparkBenchPointer.KeyHeld(KeyCode.LeftAlt) || SparkBenchPointer.KeyHeld(KeyCode.RightAlt))
            {
                if (Mathf.Abs(scroll) > 0.001f && TryGetSelected(out SparkWireRecord target))
                {
                    SignalPathMesh targetMesh = GetMesh(target);
                    if (targetMesh != null)
                    {
                        Vector3[] pts = targetMesh.GetEditableWorldPoints();
                        if (selectedPointIndex > 0 && selectedPointIndex < pts.Length - 1)
                            MovePoint(target.WireId, selectedPointIndex,
                                pts[selectedPointIndex] + sceneCamera.transform.forward * scroll * depthStep);
                    }
                }
            }

            if (!SparkBenchPointer.Pressed) return;
            Vector2 mouse = SparkBenchPointer.Position;
            SparkWireRecord selected;
            if (TryGetSelected(out selected))
            {
                SignalPathMesh selectedMesh = GetMesh(selected);
                if (selectedMesh != null)
                {
                    Vector3[] points = selectedMesh.GetEditableWorldPoints();
                    int hitIndex = FindPoint(mouse, points, pointPickPixels);
                    if (hitIndex >= 0)
                    {
                        selectedPointIndex = hitIndex;
                        if (hitIndex > 0 && hitIndex < points.Length - 1)
                            StartDragging(selected.WireId, hitIndex, points[hitIndex], mouse);
                        EditorStateChanged?.Invoke();
                        return;
                    }
                }
            }

            // Shift-click a wire to add a route bend, then drag immediately.
            if (SparkBenchPointer.KeyHeld(KeyCode.LeftShift) || SparkBenchPointer.KeyHeld(KeyCode.RightShift))
            {
                if (TryFindWire(mouse, out SparkWireRecord near,
                    out Vector3 picked, out float d) && near != null)
                {
                    if (registry.SelectedWireId != near.WireId)
                        registry.SelectWire(near.WireId);
                    int insertAt = FindClosestInsertionIndex(near, picked);
                    if (InsertPoint(near.WireId, insertAt, picked))
                    {
                        selectedPointIndex = insertAt;
                        StartDragging(near.WireId, insertAt, picked, mouse);
                    }
                    return;
                }
            }
            if (TryFindWire(mouse, out SparkWireRecord wire,
                out Vector3 nearest, out float pixelDistance))
            {
                if (registry.SelectedWireId != wire.WireId)
                    registry.SelectWire(wire.WireId);
                else
                {
                    // Dragging a selected cable span automatically creates a
                    // bend; a simple click merely keeps the wire selected.
                    pendingSegmentDrag = true;
                    pendingWireId = wire.WireId;
                    pendingMouse = mouse;
                    pendingWorld = nearest;
                }
                return;
            }
            registry.SelectWire(null);
        }

        private void LateUpdate()
        {
            if (!editMode || !showEditGizmos) return;
            SyncSceneGizmos();
        }

        private bool TryGetSelected(out SparkWireRecord record)
        {
            record = null;
            return registry != null &&
                registry.TryGetWire(registry.SelectedWireId, out record) &&
                record != null && record.Path != null;
        }

        private static SignalPathMesh GetMesh(SparkWireRecord record)
        {
            return record != null && record.Path != null
                ? record.Path.GetComponent<SignalPathMesh>() : null;
        }

        private void StartDragging(string wireId, int index, Vector3 position, Vector2 screen)
        {
            SparkWireRecord record;
            if (registry == null || !registry.TryGetWire(wireId, out record)) return;
            SignalPathMesh mesh = GetMesh(record);
            if (mesh == null) return;
            dragOriginal = Capture(mesh);
            dragWireId = wireId;
            dragStartScreen = screen;
            dragStartPosition = position;
            currentDragPlane = new Plane(GetPlaneNormal(), position);
            Ray ray = sceneCamera.ScreenPointToRay(screen);
            dragOffset = Vector3.zero;
            if (currentDragPlane.Raycast(ray, out float enter))
                dragOffset = position - ray.GetPoint(enter);
            dragging = true;
            movedDuringDrag = false;
        }

        private Vector3 GetPlaneNormal()
        {
            switch (dragPlane)
            {
                case RouteDragPlane.WorldXZ: return Vector3.up;
                case RouteDragPlane.WorldXY: return Vector3.forward;
                case RouteDragPlane.WorldYZ: return Vector3.right;
                default: return sceneCamera.transform.forward;
            }
        }

        private void ContinueDragging()
        {
            SparkWireRecord record;
            if (!registry.TryGetWire(dragWireId, out record)) { CancelDragging(); return; }
            SignalPathMesh mesh = GetMesh(record);
            if (mesh == null) { CancelDragging(); return; }
            Vector3[] route = mesh.GetEditableWorldPoints();
            if (selectedPointIndex <= 0 || selectedPointIndex >= route.Length - 1)
                return;
            Vector2 mouse = SparkBenchPointer.Position;
            Vector3 next;
            if (SparkBenchPointer.KeyHeld(KeyCode.V))
            {
                float unitsPerPixel = sceneCamera.orthographic
                    ? 2f * sceneCamera.orthographicSize / Mathf.Max(1f, Screen.height)
                    : 2f * Mathf.Max(0.05f, Vector3.Dot(dragStartPosition - sceneCamera.transform.position,
                        sceneCamera.transform.forward)) *
                        Mathf.Tan(sceneCamera.fieldOfView * Mathf.Deg2Rad * 0.5f) /
                        Mathf.Max(1f, Screen.height);
                next = dragStartPosition + Vector3.up *
                    ((mouse.y - dragStartScreen.y) * unitsPerPixel);
            }
            else
            {
                Ray ray = sceneCamera.ScreenPointToRay(mouse);
                if (!currentDragPlane.Raycast(ray, out float enter)) return;
                next = ray.GetPoint(enter) + dragOffset;
            }
            if (snapToWorldGrid) next = RoundToGrid(next);
            if ((route[selectedPointIndex] - next).sqrMagnitude < 0.0000000001f) return;
            route[selectedPointIndex] = next;
            if (mesh.SetManualRoute(route)) movedDuringDrag = true;
        }

        private void FinishDragging()
        {
            if (!dragging) return;
            bool modified = movedDuringDrag;
            string wireId = dragWireId;
            RouteSnapshot before = dragOriginal;
            dragging = false;
            movedDuringDrag = false;
            dragWireId = null;
            dragOriginal = null;
            if (modified)
            {
                PushUndo(wireId, before);
                registry.RefreshNow();
                EditorStateChanged?.Invoke();
            }
        }

        private void CancelDragging()
        {
            if (!dragging) return;
            string wireId = dragWireId;
            RouteSnapshot before = dragOriginal;
            dragging = false;
            movedDuringDrag = false;
            dragWireId = null;
            dragOriginal = null;
            if (registry != null && registry.TryGetWire(wireId, out SparkWireRecord record))
            {
                SignalPathMesh mesh = GetMesh(record);
                if (mesh != null && before != null) Restore(mesh, before);
            }
            EditorStateChanged?.Invoke();
        }

        private Vector3 RoundToGrid(Vector3 p)
        {
            float inv = 1f / Mathf.Max(0.0001f, worldGridSize);
            return new Vector3(Mathf.Round(p.x * inv) / inv,
                Mathf.Round(p.y * inv) / inv,
                Mathf.Round(p.z * inv) / inv);
        }

        // ---- Public programming API: precise edits by stable wire ID --------

        public bool GetRoute(string wireId, out Vector3[] points)
        {
            points = null;
            if (registry == null || !registry.TryGetWire(wireId, out SparkWireRecord wire))
                return false;
            SignalPathMesh mesh = GetMesh(wire);
            if (mesh == null) return false;
            points = mesh.GetEditableWorldPoints();
            return points.Length >= 2;
        }

        public bool MovePoint(string wireId, int index, Vector3 newWorldPosition)
        {
            if (!GetTarget(wireId, out SignalPathMesh mesh)) return false;
            Vector3[] points = mesh.GetEditableWorldPoints();
            if (index <= 0 || index >= points.Length - 1) return false;
            if (snapToWorldGrid) newWorldPosition = RoundToGrid(newWorldPosition);
            if ((points[index] - newWorldPosition).sqrMagnitude < 0.0000000001f) return false;
            RouteSnapshot before = Capture(mesh);
            points[index] = newWorldPosition;
            if (!mesh.SetManualRoute(points)) return false;
            PushUndo(wireId, before);
            registry.RefreshNow();
            EditorStateChanged?.Invoke();
            return true;
        }

        public bool InsertPoint(string wireId, int insertAt, Vector3 worldPosition)
        {
            if (!GetTarget(wireId, out SignalPathMesh mesh)) return false;
            Vector3[] points = mesh.GetEditableWorldPoints();
            if (points.Length >= maxControlPoints || insertAt < 1 || insertAt >= points.Length)
                return false;
            RouteSnapshot before = Capture(mesh);
            Vector3[] next = new Vector3[points.Length + 1];
            Array.Copy(points, 0, next, 0, insertAt);
            next[insertAt] = snapToWorldGrid ? RoundToGrid(worldPosition) : worldPosition;
            Array.Copy(points, insertAt, next, insertAt + 1, points.Length - insertAt);
            if (!mesh.SetManualRoute(next)) return false;
            PushUndo(wireId, before);
            registry.RefreshNow();
            EditorStateChanged?.Invoke();
            return true;
        }

        public bool DeletePoint(string wireId, int index)
        {
            if (!GetTarget(wireId, out SignalPathMesh mesh)) return false;
            Vector3[] points = mesh.GetEditableWorldPoints();
            if (points.Length <= 2 || index <= 0 || index >= points.Length - 1)
                return false;
            RouteSnapshot before = Capture(mesh);
            Vector3[] next = new Vector3[points.Length - 1];
            Array.Copy(points, 0, next, 0, index);
            Array.Copy(points, index + 1, next, index, next.Length - index);
            if (!mesh.SetManualRoute(next)) return false;
            PushUndo(wireId, before);
            if (wireId == SelectedWireId) selectedPointIndex = -1;
            registry.RefreshNow();
            EditorStateChanged?.Invoke();
            return true;
        }

        public bool RemoveSelectedPoint()
        {
            return DeletePoint(SelectedWireId, selectedPointIndex);
        }

        public bool ResetSelectedRoute()
        {
            if (!GetTarget(SelectedWireId, out SignalPathMesh mesh) ||
                !mesh.HasManualRoute) return false;
            RouteSnapshot before = Capture(mesh);
            mesh.ClearManualRoute();
            PushUndo(SelectedWireId, before);
            selectedPointIndex = -1;
            registry.RefreshNow();
            EditorStateChanged?.Invoke();
            return true;
        }

        public bool UndoRoute()
        {
            if (!GetTarget(SelectedWireId, out SignalPathMesh mesh)) return false;
            RouteHistory history = GetHistory(SelectedWireId, false);
            if (history == null || history.Undo.Count == 0) return false;
            int last = history.Undo.Count - 1;
            RouteSnapshot state = history.Undo[last];
            history.Undo.RemoveAt(last);
            history.Redo.Add(Capture(mesh));
            Restore(mesh, state);
            selectedPointIndex = -1;
            registry.RefreshNow();
            EditorStateChanged?.Invoke();
            return true;
        }

        public bool RedoRoute()
        {
            if (!GetTarget(SelectedWireId, out SignalPathMesh mesh)) return false;
            RouteHistory history = GetHistory(SelectedWireId, false);
            if (history == null || history.Redo.Count == 0) return false;
            int last = history.Redo.Count - 1;
            RouteSnapshot state = history.Redo[last];
            history.Redo.RemoveAt(last);
            history.Undo.Add(Capture(mesh));
            Restore(mesh, state);
            selectedPointIndex = -1;
            registry.RefreshNow();
            EditorStateChanged?.Invoke();
            return true;
        }

        private bool GetTarget(string id, out SignalPathMesh mesh)
        {
            mesh = null;
            if (registry == null || string.IsNullOrEmpty(id) ||
                !registry.TryGetWire(id, out SparkWireRecord record)) return false;
            mesh = GetMesh(record);
            return mesh != null;
        }

        private static RouteSnapshot Capture(SignalPathMesh mesh)
        {
            return new RouteSnapshot {
                HasOverride = mesh.HasManualRoute,
                Points = mesh.GetEditableWorldPoints()
            };
        }

        private static void Restore(SignalPathMesh mesh, RouteSnapshot state)
        {
            if (state.HasOverride) mesh.SetManualRoute(state.Points);
            else mesh.ClearManualRoute();
        }

        private RouteHistory GetHistory(string wireId, bool create)
        {
            if (string.IsNullOrEmpty(wireId)) return null;
            if (histories.TryGetValue(wireId, out RouteHistory history)) return history;
            if (!create) return null;
            history = new RouteHistory();
            histories.Add(wireId, history);
            return history;
        }

        private void PushUndo(string wireId, RouteSnapshot before)
        {
            RouteHistory history = GetHistory(wireId, true);
            history.Undo.Add(before);
            if (history.Undo.Count > historyStepsPerWire) history.Undo.RemoveAt(0);
            history.Redo.Clear();
        }

        // ---- Camera-based selection; no wire MeshCollider required --------

        private int FindPoint(Vector2 screen, Vector3[] points, float radiusPixels)
        {
            int best = -1;
            float radiusSquared = radiusPixels * radiusPixels;
            for (int i = 0; i < points.Length; i++)
            {
                Vector3 sp = sceneCamera.WorldToScreenPoint(points[i]);
                if (sp.z <= 0f) continue;
                float d = ((Vector2)sp - screen).sqrMagnitude;
                if (d <= radiusSquared) { radiusSquared = d; best = i; }
            }
            return best;
        }

        private bool TryFindWire(Vector2 screen, out SparkWireRecord hit,
            out Vector3 worldPoint, out float distancePixels)
        {
            hit = null;
            worldPoint = Vector3.zero;
            distancePixels = wirePickPixels;
            if (registry == null) return false;
            for (int r = 0; r < registry.Wires.Count; r++)
            {
                SparkWireRecord record = registry.Wires[r];
                SignalPathMesh mesh = GetMesh(record);
                if (mesh == null || !mesh.isActiveAndEnabled ||
                    !record.Path.gameObject.activeInHierarchy) continue;
                Vector3[] centerline = mesh.GetRenderedCenterline();
                if (centerline.Length < 2) centerline = mesh.GetEditableWorldPoints();
                for (int i = 0; i < centerline.Length - 1; i++)
                {
                    Vector3 a = sceneCamera.WorldToScreenPoint(centerline[i]);
                    Vector3 b = sceneCamera.WorldToScreenPoint(centerline[i + 1]);
                    if (a.z <= 0f || b.z <= 0f) continue;
                    float t;
                    float d = DistanceToScreenSegment(screen, a, b, out t);
                    if (d >= distancePixels) continue;
                    distancePixels = d;
                    worldPoint = Vector3.Lerp(centerline[i], centerline[i + 1], t);
                    hit = record;
                }
            }
            return hit != null;
        }

        private int FindClosestInsertionIndex(SparkWireRecord wire, Vector3 point)
        {
            SignalPathMesh mesh = GetMesh(wire);
            if (mesh == null) return 1;
            Vector3[] route = mesh.GetEditableWorldPoints();
            float closest = float.MaxValue;
            int best = 1;
            for (int i = 0; i < route.Length - 1; i++)
            {
                Vector3 ab = route[i + 1] - route[i];
                float t = ab.sqrMagnitude > 0.00000001f
                    ? Mathf.Clamp01(Vector3.Dot(point - route[i], ab) / ab.sqrMagnitude) : 0f;
                float d = (point - (route[i] + ab * t)).sqrMagnitude;
                if (d < closest) { closest = d; best = i + 1; }
            }
            return best;
        }

        private static float DistanceToScreenSegment(Vector2 p, Vector2 a,
            Vector2 b, out float t)
        {
            Vector2 ab = b - a;
            t = ab.sqrMagnitude > 0.00000001f
                ? Mathf.Clamp01(Vector2.Dot(p - a, ab) / ab.sqrMagnitude) : 0f;
            return Vector2.Distance(p, a + ab * t);
        }

        // ---- Scene helpers: unobtrusive highlight and handles -------------

        private void EnsureGizmos()
        {
            if (gizmoRoot != null) return;
            gizmoRoot = new GameObject("Wire Edit Handles (runtime)");
            gizmoRoot.hideFlags = HideFlags.DontSave;
            if (sceneCamera == null) sceneCamera = Camera.main;
            Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null) shader = Shader.Find("Sprites/Default");
            if (shader == null) shader = Shader.Find("Standard");
            if (shader == null)
            {
                Debug.LogWarning("[WIRE EDITOR] No gizmo shader found. Assign a material in the Inspector.", this);
                return;
            }
            generatedHandleMaterial = optionalGizmoMaterial == null ? new Material(shader) : null;
            generatedLineMaterial = optionalHighlightMaterial == null ? new Material(shader) : null;
            Material handleMat = optionalGizmoMaterial != null ? optionalGizmoMaterial : generatedHandleMaterial;
            Material lineMat = optionalHighlightMaterial != null ? optionalHighlightMaterial : generatedLineMaterial;
            if (generatedHandleMaterial != null)
                SetMaterialColor(generatedHandleMaterial, Color.white);
            if (generatedLineMaterial != null) SetMaterialColor(generatedLineMaterial, new Color(.2f, .9f, 1f, .9f));
            GameObject lineGo = new GameObject("Selected cable outline", typeof(LineRenderer));
            lineGo.transform.SetParent(gizmoRoot.transform, false);
            selectionLine = lineGo.GetComponent<LineRenderer>();
            selectionLine.useWorldSpace = true;
            selectionLine.sharedMaterial = lineMat;
            selectionLine.widthMultiplier = highlightWidth;
            selectionLine.numCornerVertices = 4;
            selectionLine.numCapVertices = 4;
            selectionLine.alignment = LineAlignment.View;
            selectionLine.shadowCastingMode = ShadowCastingMode.Off;
            selectionLine.receiveShadows = false;
            selectionLine.sortingOrder = 200;
            handleBlock = new MaterialPropertyBlock();
        }

        private static void SetMaterialColor(Material material, Color color)
        {
            if (material == null) return;
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
            if (material.HasProperty("_Color")) material.SetColor("_Color", color);
        }

        private void SyncSceneGizmos()
        {
            if (!editMode || !showEditGizmos || !TryGetSelected(out SparkWireRecord wire))
            {
                if (gizmoRoot != null) gizmoRoot.SetActive(false);
                return;
            }
            SignalPathMesh mesh = GetMesh(wire);
            if (mesh == null) { if (gizmoRoot != null) gizmoRoot.SetActive(false); return; }
            EnsureGizmos();
            if (selectionLine == null) return;
            gizmoRoot.SetActive(true);
            Vector3[] controls = mesh.GetEditableWorldPoints();
            if (controls.Length < 2) return;
            Vector3[] linePoints = mesh.GetRenderedCenterline();
            if (linePoints.Length < 2) linePoints = controls;
            selectionLine.positionCount = linePoints.Length;
            selectionLine.SetPositions(linePoints);
            displayedWireId = wire.WireId;

            for (int i = handles.Count; i < controls.Length; i++)
            {
                GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                sphere.name = "Route Point " + i;
                sphere.transform.SetParent(gizmoRoot.transform, false);
                sphere.layer = 2; // Ignore Raycast; scene picking uses screen distance.
                Collider collider = sphere.GetComponent<Collider>();
                if (collider != null) collider.enabled = false;
                Renderer renderer = sphere.GetComponent<Renderer>();
                renderer.sharedMaterial = optionalGizmoMaterial != null
                    ? optionalGizmoMaterial : generatedHandleMaterial;
                handles.Add(sphere.transform);
                handleRenderers.Add(renderer);
            }
            for (int i = 0; i < handles.Count; i++)
            {
                bool visible = i < controls.Length;
                if (handles[i].gameObject.activeSelf != visible)
                    handles[i].gameObject.SetActive(visible);
                if (!visible) continue;
                handles[i].position = controls[i];
                float size = WorldSizeForPixels(controls[i], handleSizePixels);
                handles[i].localScale = Vector3.one * Mathf.Clamp(size, .008f, .25f);
                Color color = i == 0 || i == controls.Length - 1 ? EndpointColor :
                    i == selectedPointIndex ? SelectedHandleColor : HandleColor;
                handleBlock.Clear();
                handleBlock.SetColor("_BaseColor", color);
                handleBlock.SetColor("_Color", color);
                handleRenderers[i].SetPropertyBlock(handleBlock);
            }
        }

        private float WorldSizeForPixels(Vector3 position, float pixels)
        {
            float h = Mathf.Max(1f, Screen.height);
            if (sceneCamera.orthographic)
                return (sceneCamera.orthographicSize * 2f / h) * pixels;
            float d = Mathf.Max(0.02f, Vector3.Dot(position - sceneCamera.transform.position,
                sceneCamera.transform.forward));
            return 2f * d * Mathf.Tan(sceneCamera.fieldOfView * Mathf.Deg2Rad * 0.5f) *
                pixels / h;
        }

        private void DestroyGizmos()
        {
            handles.Clear();
            handleRenderers.Clear();
            if (gizmoRoot != null)
            {
                if (Application.isPlaying) Destroy(gizmoRoot);
                else DestroyImmediate(gizmoRoot);
                gizmoRoot = null;
            }
            selectionLine = null;
            if (generatedHandleMaterial != null)
            {
                if (Application.isPlaying) Destroy(generatedHandleMaterial);
                else DestroyImmediate(generatedHandleMaterial);
                generatedHandleMaterial = null;
            }
            if (generatedLineMaterial != null)
            {
                if (Application.isPlaying) Destroy(generatedLineMaterial);
                else DestroyImmediate(generatedLineMaterial);
                generatedLineMaterial = null;
            }
        }
    }
}
