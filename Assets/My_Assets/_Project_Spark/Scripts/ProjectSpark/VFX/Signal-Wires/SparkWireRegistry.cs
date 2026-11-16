using System;
using System.Collections.Generic;
using AAAUI.VFX;
using ProjectSpark.Circuit;
using ProjectSpark.Gameplay;
using UnityEngine;

namespace ProjectSpark.Gameplay
{
    /// <summary>
    /// Public, runtime record for a finished player wire. IDs remain stable
    /// throughout this registry's session, even after undo/restore.
    /// The circuit connection ID may change when a removed wire is restored.
    /// </summary>
    [Serializable]
    public sealed class SparkWireRecord
    {
        [SerializeField] private string wireId;
        [SerializeField] private SignalPath path;
        [SerializeField] private SparkTerminal terminalA;
        [SerializeField] private SparkTerminal terminalB;
        [SerializeField] private int waypoints;
        [SerializeField] private bool circuitBound;
        private ulong connectionId;

        public string WireId => wireId;
        public ulong ConnectionId => connectionId;
        public SignalPath Path => path;
        public SparkTerminal TerminalA => terminalA;
        public SparkTerminal TerminalB => terminalB;
        public int WaypointCount => waypoints;
        public bool IsElectricallyConnected => circuitBound;
        public string FromLabel => terminalA != null ? terminalA.name : "Visual start";
        public string ToLabel => terminalB != null ? terminalB.name : "Visual end";

        internal SparkWireRecord(string id, SignalPath visual,
            bool hasConnection, SparkCircuitConnection connection)
        {
            wireId = id;
            Update(visual, hasConnection, connection);
        }

        internal void Update(SignalPath visual, bool hasConnection,
            SparkCircuitConnection connection)
        {
            path = visual;
            connectionId = hasConnection ? connection.Id : 0UL;
            circuitBound = hasConnection;
            terminalA = hasConnection ? connection.A : null;
            terminalB = hasConnection ? connection.B : null;
            SignalPathMesh mesh = visual != null
                ? visual.GetComponent<SignalPathMesh>() : null;
            waypoints = mesh != null ? mesh.EffectiveWaypointCount :
                (visual != null ? visual.PointCount : 0);
        }
    }

    /// <summary>
    /// Runtime authoritative public index of all *finished* SignalPath_Manager
    /// wires (both legacy and terminal-to-terminal). The existing manager and
    /// SparkCircuitSystem still own visual and electrical wire lifecycles.
    /// </summary>
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(200)]
    public sealed class SparkWireRegistry : MonoBehaviour
    {
        [Header("Existing ProjectSpark Systems")]
        [SerializeField] private SignalPath_Manager pathManager;
        [SerializeField] private SignalWireBuilder wireBuilder;

        [Header("Runtime Registry (readable in Inspector)")]
        [SerializeField] private List<SparkWireRecord> wires = new List<SparkWireRecord>();
        [SerializeField, Min(0.05f)] private float reconciliationInterval = 0.15f;
        [SerializeField, Range(1, 200)] private int maxUndoHistory = 40;
        [SerializeField] private bool logChanges;

        private readonly Dictionary<SignalPath, SparkWireRecord> byPath =
            new Dictionary<SignalPath, SparkWireRecord>();
        private readonly List<RemovedWireSnapshot> removalHistory =
            new List<RemovedWireSnapshot>();
        private readonly HashSet<SignalPath> seenPaths = new HashSet<SignalPath>();
        private readonly List<SparkCircuitConnection> connectionLookup =
            new List<SparkCircuitConnection>();
        private uint nextWireNumber = 1;
        private float nextReconciliationTime;
        private bool dirty = true;
        private string selectedWireId;

        public event Action WiresChanged;
        public event Action<string> SelectionChanged;

        /// <summary>Read-only public index of completed wires. Runtime only.</summary>
        public IReadOnlyList<SparkWireRecord> Wires => wires;
        public int Count => wires.Count;
        public int UndoHistoryCount => removalHistory.Count;
        /// <summary>Enumerate archived IDs for building an undo UI.</summary>
        public string GetRemovedWireIdAt(int index)
        {
            return index >= 0 && index < removalHistory.Count
                ? removalHistory[index].WireId : null;
        }
        public string SelectedWireId => selectedWireId;
        public SignalPath_Manager PathManager => pathManager;

        private sealed class RemovedWireSnapshot
        {
            public string WireId;
            public Vector3[] Points;
            public Vector3[] ManualPoints;
            public SparkTerminal A;
            public SparkTerminal B;
            public bool HasConnection;
            public SparkConnectionKind Kind;
            public SparkConnectionDirection Direction;
            public WirePolarity Polarity;
            public bool TerminalCable;
            public TerminalCableRouting Routing;
            public bool FollowAnchors;
            public int OriginalIndex;
        }

        private void Awake()
        {
            ResolveReferences();
            // Inspector records must never revive destroyed wire objects
            // across Play Mode sessions or with Enter Play Mode reload disabled.
            wires.Clear();
            byPath.Clear();
            removalHistory.Clear();
            nextWireNumber = 1;
            selectedWireId = null;
        }

        private void OnEnable()
        {
            ResolveReferences();
            if (wireBuilder != null)
                wireBuilder.TerminalWireConnected += OnTerminalWireConnected;
            dirty = true;
            nextReconciliationTime = 0f;
        }

        private void OnDisable()
        {
            if (wireBuilder != null)
                wireBuilder.TerminalWireConnected -= OnTerminalWireConnected;
        }

        private void LateUpdate()
        {
            if (dirty || Time.unscaledTime >= nextReconciliationTime)
                RefreshNow();
        }

        private void ResolveReferences()
        {
            if (pathManager == null) pathManager = GetComponent<SignalPath_Manager>();
            if (pathManager == null) pathManager = GetComponentInParent<SignalPath_Manager>();
            if (pathManager == null) pathManager = FindFirstObjectByType<SignalPath_Manager>();
            if (wireBuilder == null) wireBuilder = FindFirstObjectByType<SignalWireBuilder>();
        }

        private void OnTerminalWireConnected(SparkTerminal a,
            SparkTerminal b, SignalPath path)
        {
            // Builder fires before it finishes its current path. Reconcile
            // in LateUpdate, after it releases the temporary drawing state.
            dirty = true;
        }

        /// <summary>
        /// Detect wires completed by *any* controller, as well as external
        /// deletions performed via the manager or electrical circuit.
        /// Does not register previews or create/delete electrical topology.
        /// </summary>
        public void RefreshNow()
        {
            if (pathManager == null) ResolveReferences();
            dirty = false;
            nextReconciliationTime = Time.unscaledTime + reconciliationInterval;
            if (pathManager == null) return;

            SignalPath[] paths = pathManager.Paths;
            SignalPath drawingPath = pathManager.CurrentPath;
            seenPaths.Clear();
            if (paths != null)
            {
                for (int i = 0; i < paths.Length; i++)
                {
                    SignalPath path = paths[i];
                    if (path == null || path == drawingPath ||
                        !path.gameObject.activeInHierarchy) continue;
                    seenPaths.Add(path);
                }
            }

            bool changed = false;
            for (int i = wires.Count - 1; i >= 0; i--)
            {
                SparkWireRecord record = wires[i];
                if (record != null && record.Path != null &&
                    seenPaths.Contains(record.Path))
                    continue;

                if (record != null) byPath.Remove(record.Path);
                wires.RemoveAt(i);
                changed = true;
            }

            if (paths != null)
            {
                for (int i = 0; i < paths.Length; i++)
                {
                    SignalPath path = paths[i];
                    if (!seenPaths.Contains(path)) continue;
                    SparkCircuitConnection connection;
                    bool hasConnection = pathManager.TryGetElectricalConnection(
                        path, out connection);
                    SparkWireRecord record;
                    if (byPath.TryGetValue(path, out record))
                    {
                        if (record.ConnectionId != (hasConnection ? connection.Id : 0UL) ||
                            record.IsElectricallyConnected != hasConnection ||
                            record.TerminalA != (hasConnection ? connection.A : null) ||
                            record.TerminalB != (hasConnection ? connection.B : null) ||
                            record.WaypointCount != (path.GetComponent<SignalPathMesh>() != null
                                ? path.GetComponent<SignalPathMesh>().EffectiveWaypointCount
                                : path.PointCount))
                        {
                            record.Update(path, hasConnection, connection);
                            changed = true;
                        }
                        continue;
                    }

                    string wireId = GetOrAssignIdentity(path);
                    record = new SparkWireRecord(wireId, path, hasConnection, connection);
                    byPath.Add(path, record);
                    wires.Add(record);
                    changed = true;
                    if (logChanges) Debug.Log("[WIRE REGISTRY] Added " + wireId, this);
                }
            }

            if (!string.IsNullOrEmpty(selectedWireId) &&
                !ContainsWireId(selectedWireId))
            {
                selectedWireId = null;
                SelectionChanged?.Invoke(null);
                changed = true;
            }
            if (changed)
            {
                // Rebuild the key index after external Unity object destruction.
                byPath.Clear();
                for (int i = 0; i < wires.Count; i++)
                    if (wires[i].Path != null) byPath[wires[i].Path] = wires[i];
                WiresChanged?.Invoke();
            }
        }

        private string GetOrAssignIdentity(SignalPath path)
        {
            SparkWireIdentity identity = path.GetComponent<SparkWireIdentity>();
            if (identity == null) identity = path.gameObject.AddComponent<SparkWireIdentity>();
            string candidate = identity.WireId;
            // An already-tagged path (e.g. after registry reinitialization)
            // should retain its ID, unless another live/archived wire uses it.
            if (!string.IsNullOrEmpty(candidate) && !IsIdReserved(candidate))
                return candidate;
            do { candidate = "PS-W" + (nextWireNumber++).ToString("D5"); }
            while (IsIdReserved(candidate));
            identity.Assign(candidate);
            return candidate;
        }

        private bool IsIdReserved(string id)
        {
            if (ContainsWireId(id)) return true;
            for (int i = 0; i < removalHistory.Count; i++)
                if (removalHistory[i].WireId == id) return true;
            return false;
        }

        private bool ContainsWireId(string id)
        {
            for (int i = 0; i < wires.Count; i++)
                if (wires[i].WireId == id) return true;
            return false;
        }

        public bool TryGetWire(string wireId, out SparkWireRecord wire)
        {
            wire = null;
            if (string.IsNullOrEmpty(wireId)) return false;
            for (int i = 0; i < wires.Count; i++)
                if (wires[i].WireId == wireId)
                {
                    wire = wires[i];
                    return true;
                }
            return false;
        }

        public bool TryGetWire(ulong connectionId, out SparkWireRecord wire)
        {
            wire = null;
            for (int i = 0; i < wires.Count; i++)
                if (wires[i].IsElectricallyConnected &&
                    wires[i].ConnectionId == connectionId)
                {
                    wire = wires[i];
                    return true;
                }
            return false;
        }

        public bool SelectWire(string wireId)
        {
            if (!string.IsNullOrEmpty(wireId) && !ContainsWireId(wireId))
                return false;
            if (selectedWireId == wireId) return true;
            selectedWireId = wireId;
            SelectionChanged?.Invoke(selectedWireId);
            return true;
        }

        /// <summary>Remove one exact wire by stable, public ID.</summary>
        public bool RemoveWire(string wireId)
        {
            RefreshNow();
            SparkWireRecord record;
            if (!TryGetWire(wireId, out record) || pathManager == null) return false;
            return RemoveRecord(record);
        }

        /// <summary>Remove a wire by SparkCircuitSystem connection ID.</summary>
        public bool RemoveWire(ulong connectionId)
        {
            RefreshNow();
            SparkWireRecord record;
            return TryGetWire(connectionId, out record) && RemoveRecord(record);
        }

        public bool RemoveSelectedWire() => RemoveWire(selectedWireId);

        /// <summary>
        /// Undo any one selected wire (not only the newest). Removal is
        /// reversible with RestoreLastRemovedWire while endpoints are valid.
        /// </summary>
        public bool UndoWire(string wireId) => RemoveWire(wireId);
        public bool UndoSelectedWire() => RemoveSelectedWire();

        /// <summary>Undo only the latest *completed* wire in this registry.</summary>
        public bool UndoLastWire()
        {
            RefreshNow();
            if (wires.Count == 0) return false;
            return RemoveRecord(wires[wires.Count - 1]);
        }

        private bool RemoveRecord(SparkWireRecord record)
        {
            if (record == null || record.Path == null || pathManager == null) return false;
            SparkCircuitConnection connection;
            bool hasConnection = pathManager.TryGetElectricalConnection(
                record.Path, out connection);
            // Do not knowingly orphan circuit topology if a manager was
            // configured without the electrical system.
            if (hasConnection && pathManager.Circuit == null)
            {
                Debug.LogWarning("[WIRE REGISTRY] Electrical manager missing; removal aborted.", this);
                return false;
            }
            RemovedWireSnapshot snapshot = CaptureSnapshot(record, hasConnection, connection);
            if (!pathManager.RemoveWire(record.Path)) return false;

            removalHistory.Add(snapshot);
            if (removalHistory.Count > maxUndoHistory)
                removalHistory.RemoveAt(0);
            if (logChanges) Debug.Log("[WIRE REGISTRY] Removed " + record.WireId, this);
            RefreshNow(); // emits WiresChanged with updated history
            return true;
        }

        private RemovedWireSnapshot CaptureSnapshot(SparkWireRecord record,
            bool hasConnection, SparkCircuitConnection connection)
        {
            SignalPathMesh mesh = pathManager.GetMeshForPath(record.Path);
            Vector3[] points = record.Path.GetRenderPoints();
            return new RemovedWireSnapshot
            {
                WireId = record.WireId,
                Points = points != null ? (Vector3[])points.Clone() : new Vector3[0],
                ManualPoints = mesh != null && mesh.HasManualRoute
                    ? mesh.GetEffectiveRoutePoints() : null,
                A = hasConnection ? connection.A : null,
                B = hasConnection ? connection.B : null,
                HasConnection = hasConnection,
                Kind = hasConnection ? connection.Kind : SparkConnectionKind.Wire,
                Direction = hasConnection ? connection.Direction : SparkConnectionDirection.Bidirectional,
                Polarity = DeterminePolarity(hasConnection, connection),
                TerminalCable = mesh != null && mesh.OriginalUsesTerminalCable,
                Routing = mesh != null ? mesh.OriginalCableRouting : TerminalCableRouting.Straight,
                FollowAnchors = mesh != null && mesh.FollowsTerminalAnchors,
                OriginalIndex = wires.IndexOf(record)
            };
        }

        private static WirePolarity DeterminePolarity(bool hasConnection,
            SparkCircuitConnection connection)
        {
            if (!hasConnection || connection.A == null) return WirePolarity.Neutral;
            switch (connection.A.EffectivePolarity)
            {
                case SparkTerminalPolarity.Positive: return WirePolarity.Positive;
                case SparkTerminalPolarity.Negative: return WirePolarity.Negative;
                default: return WirePolarity.Neutral;
            }
        }

        /// <summary>
        /// Restore the latest wire removed *through this registry*. Its wire
        /// ID and route are preserved; the circuit allocates a fresh connection
        /// ID. If the original terminals are no longer available/compatible,
        /// restoration safely fails and leaves the undo snapshot available.
        /// </summary>
        public bool RestoreLastRemovedWire()
        {
            return removalHistory.Count > 0 &&
                RestoreSnapshotAt(removalHistory.Count - 1);
        }

        /// <summary>Undo removal of an exact archived wire by its stable ID.</summary>
        public bool RestoreRemovedWire(string wireId)
        {
            if (string.IsNullOrEmpty(wireId)) return false;
            for (int i = removalHistory.Count - 1; i >= 0; i--)
                if (removalHistory[i].WireId == wireId)
                    return RestoreSnapshotAt(i);
            return false;
        }

        private bool RestoreSnapshotAt(int historyIndex)
        {
            if (historyIndex < 0 || historyIndex >= removalHistory.Count)
                return false;
            if (pathManager == null) ResolveReferences();
            if (pathManager == null || pathManager.CurrentPath != null) return false;

            RemovedWireSnapshot snapshot = removalHistory[historyIndex];
            if (snapshot.Points == null || snapshot.Points.Length < 2) return false;
            if (snapshot.HasConnection && (snapshot.A == null || snapshot.B == null ||
                pathManager.Circuit == null)) return false;
            // Don't conflict with currently present copies of the same ID.
            if (ContainsWireId(snapshot.WireId)) return false;
            if (snapshot.HasConnection && !ValidateRestoration(snapshot)) return false;

            SignalPath newPath = null;
            bool success = false;
            try
            {
                SignalPathMesh mesh = pathManager.CreateNextWire(snapshot.Polarity);
                newPath = pathManager.CurrentPath;
                if (mesh == null || newPath == null) return false;
                if (snapshot.TerminalCable)
                    mesh.ConfigureTerminalCable(snapshot.Routing);
                for (int i = 0; i < snapshot.Points.Length; i++)
                    newPath.AddPoint(snapshot.Points[i]);

                SparkCircuitConnection newConnection = default(SparkCircuitConnection);
                bool newConnectionBound = false;
                if (snapshot.HasConnection)
                {
                    if (!pathManager.Circuit.TryCreateConnection(snapshot.A,
                        snapshot.B, snapshot.Kind, snapshot.Direction,
                        out newConnection))
                    {
                        Debug.LogWarning("[WIRE REGISTRY] Restore rejected by circuit for " + snapshot.WireId, this);
                        return false;
                    }
                    pathManager.BindElectricalConnection(newPath, newConnection);
                    newConnectionBound = true;
                }

                if (snapshot.TerminalCable)
                    mesh.BindTerminalAnchors(
                        snapshot.A != null ? snapshot.A.transform : null,
                        snapshot.B != null ? snapshot.B.transform : null,
                        snapshot.FollowAnchors);
                if (snapshot.ManualPoints != null && snapshot.ManualPoints.Length >= 2 &&
                    !mesh.SetManualRoute(snapshot.ManualPoints))
                    return false;
                mesh.Rebuild();
                pathManager.FinishCurrentPath();
                // Add explicitly, so the restored wire keeps its original ID.
                SparkWireIdentity identity = newPath.GetComponent<SparkWireIdentity>();
                if (identity == null) identity = newPath.gameObject.AddComponent<SparkWireIdentity>();
                identity.Assign(snapshot.WireId);
                SparkWireRecord record = new SparkWireRecord(snapshot.WireId,
                    newPath, newConnectionBound, newConnection);
                int insertion = Mathf.Clamp(snapshot.OriginalIndex, 0, wires.Count);
                wires.Insert(insertion, record);
                byPath[newPath] = record;
                removalHistory.RemoveAt(historyIndex);
                success = true;
                RefreshNow();
                SelectWire(snapshot.WireId);
                WiresChanged?.Invoke();
                if (logChanges) Debug.Log("[WIRE REGISTRY] Restored " + snapshot.WireId, this);
                return true;
            }
            finally
            {
                if (!success)
                {
                    if (newPath != null) pathManager.RemovePath(newPath);
                    pathManager.FinishCurrentPath();
                }
            }
        }

        private bool ValidateRestoration(RemovedWireSnapshot snapshot)
        {
            if (!snapshot.A.CanConnectTo(snapshot.B, snapshot.Kind,
                snapshot.Direction, out string reason))
            {
                Debug.LogWarning("[WIRE REGISTRY] Cannot restore " + snapshot.WireId +
                    ": " + reason, this);
                return false;
            }
            if (snapshot.A.AtCapacity || snapshot.B.AtCapacity)
            {
                Debug.LogWarning("[WIRE REGISTRY] Cannot restore " + snapshot.WireId +
                    ": terminal is at capacity.", this);
                return false;
            }
            // Match the builder's duplicate-connection rule, including reverse order.
            connectionLookup.Clear();
            pathManager.Circuit.GetConnections(snapshot.A, connectionLookup);
            foreach (SparkCircuitConnection item in connectionLookup)
            {
                if (item.A == null || item.B == null) continue;
                if ((item.A == snapshot.A && item.B == snapshot.B) ||
                    (item.A == snapshot.B && item.B == snapshot.A))
                {
                    Debug.LogWarning("[WIRE REGISTRY] Cannot restore duplicate " +
                        snapshot.WireId, this);
                    return false;
                }
            }
            return true;
        }

        // Convenient zero-argument functions for standard Unity UI buttons.
        public void OnRemoveSelectedClicked() { RemoveSelectedWire(); }
        public void OnUndoLastClicked() { UndoLastWire(); }
        public void OnRestoreLastClicked() { RestoreLastRemovedWire(); }
    }
}
