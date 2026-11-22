using System;
using System.Collections.Generic;
using AAAUI.VFX;
using ProjectSpark.Circuit;
using UnityEngine;

namespace ProjectSpark.Gameplay.OrbitalBastion
{
    public sealed partial class SparkLevel124Process : MonoBehaviour
    {
        public const string Design = "OBX32-EDU-1";
        public const int PartCount = 328, RouteCount = 416, ViaCount = 164, LayerCount = 40;
        public static string Proof(int index) => $"L124_Proof_{index + 1}";

        [Serializable]
        public sealed class Part
        {
            public string label, id;
            public GameObject model;
            public Transform footprint;
            public SparkSwitch contact;
        }

        [Serializable]
        public sealed class Route
        {
            public string id, group, net, viaId;
            public int layer, exitLayer;
            public SparkTerminal from, to;
            public SparkSwitch barrel;
            public LineRenderer copper;
            public Renderer viaVisual;
            public Vector3 entry, destination;
        }

        public SparkLevelLayout layout;
        public SparkLevelSceneBindings bindings;
        public Transform fixture, boardArt, referenceLayers;
        public Part[] parts;
        public Route[] routes;
        public Route jumper;
        public SparkTerminal[] witnesses;
        public SparkPowerSupply source;
        public SparkTerminal input, ground;
        public int jumperPart;
        public Renderer locationMarker, startMarker, endMarker, statusLamp;
        public GameObject[] faultOverlays;

        private readonly List<SparkCircuitConnection> edges = new();
        private LevelGamePlayManager manager;
        private SignalWireBuilder wire;
        private SparkWireRegistry registry;
        private int footprint, selectedPart, rotation, selectedRoute, layer = 1;
        private int installed, routed, plated, stage;
        private float nextPanelAt;
        private bool seated, contactsInspected, dirty = true, linked, unauthorized, powered;
        private bool PrecisionFootprint => footprint < 24 || parts[footprint].label.StartsWith("Y", StringComparison.Ordinal) ||
            parts[footprint].label.StartsWith("J", StringComparison.Ordinal);
        private string message = "Select the matching part, fit it to the footprint, then secure it.";

        public int Installed => installed;
        public int Routed => routed;
        public int Plated => plated;
        public int Stage => stage;
        public bool Powered => powered;
        public bool JumperPresent => linked;
        public bool Inhibited => Voltage(protectedFeed) < 4.5f;
        private Route Selected => selectedRoute == routes.Length ? jumper : routes[selectedRoute];
        private bool Construction => installed == PartCount && routed == RouteCount && plated == ViaCount && !unauthorized;
        private bool HasWire(Route route) => manager.CircuitSystem.HasConnectionBetween(route.from, route.to);
        private static bool Matches(SparkCircuitConnection connection, Route route) =>
            connection.A == route.from && connection.B == route.to || connection.A == route.to && connection.B == route.from;

        private void Start()
        {
            manager = FindFirstObjectByType<LevelGamePlayManager>();
            var loader = FindFirstObjectByType<SparkLevelLayoutLoader>();
            if (!manager || !loader || loader.ActiveLayout != layout) { enabled = false; return; }
            boardCamera = Camera.main;
            wire = FindFirstObjectByType<SignalWireBuilder>();
            registry = FindFirstObjectByType<SparkWireRegistry>();
            manager.CircuitSystem.TopologyChanged += MarkTopology;
            if (wire)
            {
                wire.TerminalPickerOverride = PickTerminal;
                wire.TerminalWireConnected += OnWireConnected;
            }
            FitTable();
            BuildControls();
            BoardView();
        }

        private void OnDisable()
        {
            if (manager && manager.CircuitSystem) manager.CircuitSystem.TopologyChanged -= MarkTopology;
            if (wire)
            {
                wire.TerminalWireConnected -= OnWireConnected;
                if (wire.TerminalPickerOverride == PickTerminal)
                {
                    if (wire.IsDrawing) wire.CancelWire();
                    wire.TerminalPickerOverride = null;
                }
            }
            if (Application.isPlaying) SavePendingCheckpoint();
            RestoreView();
        }

        private void MarkTopology() => dirty = true;
        private void OnWireConnected(SparkTerminal a, SparkTerminal b, SignalPath path)
        {
            ReadGraph();
            if (stage == 1 && HasWire(Selected)) ConfirmCopper(Selected);
        }

        private void ConfirmCopper(Route completed)
        {
            message = Construction && linked ? "All copper is connected. Checking board continuity." :
                $"{completed.id} connected. Continue with the highlighted route.";
            SelectNextRoute();
        }

        private void ReadGraph()
        {
            dirty = false;
            int previousRoutes = routed, previousVias = plated;
            bool wasLinked = linked, wasUnauthorized = unauthorized;
            routed = plated = 0;
            foreach (var route in routes)
            {
                bool completeVia = !route.barrel || route.barrel.IsConducting;
                bool connected = HasWire(route);
                if (route.barrel && completeVia) plated++;
                if (connected && completeVia) routed++;
            }
            linked = HasWire(jumper);
            parts[jumperPart].model.SetActive(linked || seated && stage == 0 && footprint == jumperPart);
            edges.Clear();
            manager.CircuitSystem.CopyConnections(edges);
            unauthorized = edges.Exists(edge => edge.Kind == SparkConnectionKind.Wire &&
                (edge.A.transform.IsChildOf(transform) || edge.B.transform.IsChildOf(transform)) &&
                !Matches(edge, jumper) && Array.Find(routes, route => Matches(edge, route)) == null);
            RefreshCopper();
            if (previousRoutes == routed && previousVias == plated && wasLinked == linked && wasUnauthorized == unauthorized) return;
            if (stage >= 2)
            {
                if (stage == 5 && powered && !linked && wasLinked && armed && Construction)
                {
                    interrupted = true;
                    AbortProtected();
                    safetyReset = false;
                    message = "W417 opened during the protected attitude run. U3 inhibited output and retained telemetry power.";
                }
                InvalidateDiagnostics();
                if (powered && (!Construction || !linked && !interrupted)) IsolatePower();
            }
            QueueCheckpoint();
        }

        private void Update()
        {
            if (!manager) return;
            if (dirty) ReadGraph();
            if (stage == 0 && installed == PartCount && Award())
            {
                SelectRoute(0);
                message = "Plate marked vias on their entry layer, then connect the highlighted contacts.";
            }
            if (stage == 1 && Construction && linked && Array.TrueForAll(witnesses, pin => Voltage(pin) > 4.7f) && Award())
            {
                routingView = false;
                RefreshCopper();
                message = "Enable bench power, measure both reference clocks, then calibrate the sensors.";
            }

            TickController(Mathf.Min(Time.unscaledDeltaTime, .075f));
            if (saveAt <= time) SavePendingCheckpoint();
            if (time < nextPanelAt) return;
            nextPanelAt = time + .060f;
            RefreshPanel();
        }

        public float Voltage(SparkTerminal pin)
        {
            var result = manager && manager.ElectricalSolver ? manager.ElectricalSolver.LatestSolveResult : null;
            return result != null && result.TryGetVoltage(pin, ground, out float value) ? value : 0f;
        }

        private bool Award()
        {
            if (stage >= 6 || !bindings.SetRuntimeComponentBinding(Proof(stage), source)) return false;
            stage++;
            QueueCheckpoint();
            manager.QueueEvaluation();
            return true;
        }

        private static void SetContact(SparkSwitch contact, bool closed)
        {
            if (contact.IsClosed != closed) contact.SetState(closed ? SparkSwitchState.Closed : SparkSwitchState.Open);
        }

        public void SelectFootprint(int index)
        {
            if (index < 0 || index >= parts.Length) return;
            if (stage is >= 2 and <= 5)
            {
                repairTarget = footprint = index;
                return;
            }
            if (stage != 0) return;
            UnseatPart();
            footprint = index;
        }

        public void SelectPart(int index)
        {
            if (index < 0 || index >= parts.Length) return;
            selectedPart = index;
            rotation = 0;
            UnseatPart();
        }

        public void RotatePart()
        {
            rotation = (rotation + 90) % 360;
            UnseatPart();
        }

        private void UnseatPart()
        {
            if (seated) parts[footprint].model.SetActive(false);
            seated = false;
            contactsInspected = false;
        }

        public bool FitPart()
        {
            if (stage != 0 || powered || parts[footprint].contact.IsClosed)
                return Reject("Select an unfinished footprint with bench power isolated.");
            if (footprint != selectedPart) return Reject("The selected part does not match this footprint.");
            if (rotation != 0) return Reject("Align the part’s pin-one mark to 0° before fitting.");
            seated = true;
            parts[footprint].model.SetActive(true);
            message = PrecisionFootprint ? "Inspect pin one and the contacts before securing this package." :
                "Part aligned. Secure it to close its assembly contact.";
            return true;
        }

        public bool InspectContacts()
        {
            if (stage != 0 || !seated || !PrecisionFootprint || rotation != 0 || selectedPart != footprint ||
                parts[footprint].contact.IsClosed)
                return Reject("Seat the matching precision package before inspecting it.");
            contactsInspected = true;
            FocusPart();
            message = "Alignment and contact checked. Secure the part at this footprint.";
            return true;
        }

        public bool Solder()
        {
            if (stage != 0 || !seated || powered || parts[footprint].contact.IsClosed)
                return Reject("Fit the matching part before securing it.");
            if (PrecisionFootprint && !contactsInspected) return Reject("Inspect the alignment and contact surfaces before securing this assembly.");
            var part = parts[footprint];
            SetContact(part.contact, true);
            part.model.SetActive(footprint != jumperPart);
            if (PrecisionFootprint) BoardView();
            installed++;
            seated = contactsInspected = false;
            QueueCheckpoint();
            int next = Array.FindIndex(parts, candidate => !candidate.contact.IsClosed);
            if (next >= 0) { SelectFootprint(next); SelectPart(next); }
            message = next >= 0 ? $"Seated {part.label}. The next footprint is selected." : "All components mounted. Verify the circuit across all 40 layers.";
            return true;
        }

        public void SelectRoute(int index)
        {
            if (index < 0 || index > routes.Length) return;
            if (wire && wire.IsDrawing) wire.CancelWire();
            selectedRoute = index;
            layer = Selected.layer;
            routingView = true;
            RefreshCopper();
            if (focused) PadView();
        }

        private void SelectNextRoute()
        {
            int count = routes.Length + 1;
            for (int step = 1; step < count; step++)
            {
                int next = (selectedRoute + step) % count;
                var candidate = next == routes.Length ? jumper : routes[next];
                if (!HasWire(candidate) || candidate.barrel && !candidate.barrel.IsConducting)
                { SelectRoute(next); return; }
            }
        }

        public void SetLayer(int number)
        {
            if (number < 1 || number > LayerCount) return;
            if (wire && wire.IsDrawing) wire.CancelWire();
            layer = number;
            routingView = true;
            RefreshCopper();
        }

        public bool PlateVia()
        {
            var route = Selected;
            if (stage is < 1 or > 5 || powered || !route.barrel || route.barrel.IsClosed || layer != route.layer)
                return Reject("Select an open via on its entry layer with the bench isolated.");
            SetContact(route.barrel, true);
            ReadGraph();
            message = $"{route.viaId} now links L{route.layer:D2} to L{route.exitLayer:D2}.";
            return true;
        }

        private bool MayConnect(Route route) => stage >= 1 && (stage <= 5 && !powered || stage >= 5 && route == jumper) &&
            layer == route.layer && (!route.barrel || route.barrel.IsConducting) && !HasWire(route);

        public bool ConnectPads()
        {
            var route = Selected;
            if (!MayConnect(route)) return Reject("Use the route’s entry layer and close its via first.");
            if (!CreateWire(route)) return Reject("The circuit rejected this contact pair.");
            ReadGraph();
            if (stage == 1) ConfirmCopper(route);
            return true;
        }

        private bool CreateWire(Route route) => manager.CircuitSystem.TryCreateConnection(route.from, route.to,
            SparkConnectionKind.Wire, SparkConnectionDirection.Bidirectional, out _);

        public bool RemoveSelectedRoute() => stage is >= 1 and <= 5 && !powered && RemoveWire(Selected);

        private bool RemoveWire(Route route)
        {
            edges.Clear();
            manager.CircuitSystem.CopyConnections(edges);
            var connection = edges.Find(candidate => candidate.Kind == SparkConnectionKind.Wire && Matches(candidate, route));
            if (connection.Id == 0 || !(registry && registry.RemoveWire(connection.Id)) &&
                !manager.CircuitSystem.TryRemoveConnection(connection.Id, out _)) return false;
            ReadGraph();
            return true;
        }

        private SparkTerminal PickTerminal(Vector2 screen, SparkTerminal start)
        {
            if (!boardCamera || !routingView || !MayConnect(Selected)) return null;
            var a = Selected.from;
            var b = Selected.to;
            if (start && start != a && start != b) return null;
            float limit = Mathf.Clamp(boardCamera.pixelHeight * .024f, 14f, 34f);
            float Distance(SparkTerminal pin)
            {
                var point = boardCamera.WorldToScreenPoint(pin.transform.position);
                return pin != start && point.z > 0 ? Vector2.Distance(point, screen) : float.PositiveInfinity;
            }
            float da = Distance(a), db = Distance(b);
            return Mathf.Min(da, db) <= limit ? da <= db ? a : b : null;
        }

        private bool Reject(string text) { message = text; return false; }
    }
}
