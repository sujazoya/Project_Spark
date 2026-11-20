using System;
using System.Collections.Generic;
using AAAUI.VFX;
using ProjectSpark.Circuit;
using UnityEngine;

namespace ProjectSpark.Gameplay
{
    public sealed partial class SparkLevel107Process : MonoBehaviour
    {
        public const string Design = "PFX15-EDU-1";
        public static string Gate(int index) => $"L107_Proof_{index + 1}";

        [Serializable] public sealed class Part
        {
            public string label;
            public GameObject model;
            public Transform footprint;
            public SparkSwitch solder;
        }

        [Serializable] public sealed class Route
        {
            public string id, group, viaId;
            public int layer, exitLayer;
            public SparkTerminal from, to;
            public SparkSwitch via;
            public LineRenderer copper;
        }

        public SparkLevelLayout layout;
        public SparkLevelSceneBindings bindings;
        public Transform fixture, modelSpace;
        public Part[] parts;
        public Route[] routes;
        public Route jumper;
        public SparkTerminal[] witnesses;
        public SparkPowerSupply source;
        public SparkSwitch benchPower, primaryContact, recoveryContact, programGate, pullupContact, requestButton, heartbeat, workload;
        public SparkTerminal ground, input, standby, primaryBus, recoveryBus, writeSense, requestSense, receiver, protectedFeed;
        public Renderer placementMarker, fromMarker, toMarker;
        public Renderer primaryLamp, recoveryLamp, trustLamp;
        public int primaryIndex, recoveryIndex, pullupIndex, jumperIndex;
        private readonly List<SparkCircuitConnection> edges = new();
        private LevelGamePlayManager manager;
        private SignalWireBuilder wire;
        private SparkWireRegistry registry;
        private Camera boardCamera;
        private int footprint, selectedPart, selectedRoute, layer = 1, installed, routed, plated, stage;
        private bool seated, powered, topologyDirty = true, jumperPresent, unauthorized, routing;
        private string message = "Select, fit, and solder the first component.";
        public int Stage => stage;
        public int Installed => installed;
        public int Routed => routed;
        public int Plated => plated;
        public int SelectedRoute => selectedRoute;
        public bool Powered => powered;
        public bool JumperPresent => jumperPresent;
        private int Count => routes.Length + 1;
        private Route Selected => selectedRoute == routes.Length ? jumper : routes[selectedRoute];
        private bool Construction => installed == parts.Length && routed == routes.Length && plated == 46 && !unauthorized;
        private bool RepairRouting => stage is >= 2 and < 6 && !Construction && !powered;

        private static bool Matches(SparkCircuitConnection edge, Route route) =>
            edge.A == route.from && edge.B == route.to || edge.A == route.to && edge.B == route.from;
        private bool HasWire(Route route) => manager.CircuitSystem.HasConnectionBetween(route.from, route.to);

        private void Start()
        {
            manager = FindFirstObjectByType<LevelGamePlayManager>();
            var loader = FindFirstObjectByType<SparkLevelLayoutLoader>();
            if (!manager || !loader || loader.ActiveLayout != layout) { enabled = false; return; }
            boardCamera = Camera.main;
            wire = FindFirstObjectByType<SignalWireBuilder>();
            registry = FindFirstObjectByType<SparkWireRegistry>();
            manager.CircuitSystem.TopologyChanged += InvalidateTopology;
            if (wire)
            {
                wire.TerminalPickerOverride = PickTerminal;
                wire.TerminalWireConnected += OnWireConnected;
            }
            BuildControls();
            FitTable();
            BoardView();
        }

        private void OnDisable()
        {
            if (manager && manager.CircuitSystem) manager.CircuitSystem.TopologyChanged -= InvalidateTopology;
            if (wire)
            {
                wire.TerminalWireConnected -= OnWireConnected;
                if (wire.TerminalPickerOverride == PickTerminal)
                {
                    if (wire.IsDrawing) wire.CancelWire();
                    wire.TerminalPickerOverride = null;
                }
            }
            RestoreView();
        }

        private void InvalidateTopology() => topologyDirty = true;
        private void OnWireConnected(SparkTerminal a, SparkTerminal b, SignalPath path)
        {
            ReadGraph();
            if ((stage == 1 || RepairRouting) && HasWire(Selected)) SelectNextRoute();
        }

        private void ReadGraph()
        {
            topologyDirty = false;
            int previousRouted = routed;
            bool previousJumper = jumperPresent;
            bool previousUnauthorized = unauthorized;
            routed = plated = 0;
            foreach (var route in routes)
            {
                bool connected = HasWire(route);
                if (connected && (!route.via || route.via.IsConducting)) routed++;
                if (route.via && route.via.IsConducting) plated++;
                route.copper.enabled = routing && route.layer == layer && connected;
            }
            jumperPresent = HasWire(jumper);
            jumper.copper.enabled = routing && jumperPresent && jumper.layer == layer;
            edges.Clear();
            manager.CircuitSystem.CopyConnections(edges);
            unauthorized = edges.Exists(edge => edge.Kind == SparkConnectionKind.Wire &&
                (edge.A.transform.IsChildOf(transform) || edge.B.transform.IsChildOf(transform)) &&
                !Matches(edge, jumper) && Array.Find(routes, route => Matches(edge, route)) == null);
            if (stage >= 2 && (previousRouted != routed || previousJumper != jumperPresent || previousUnauthorized != unauthorized))
                OnCircuitChanged(previousJumper);
            if (previousRouted != routed || previousJumper != jumperPresent) QueueCheckpoint();
            if (jumperIndex >= 0 && parts[jumperIndex].solder.IsClosed)
                parts[jumperIndex].model.SetActive(jumperPresent);
        }

        private float Voltage(SparkTerminal a, SparkTerminal b)
        {
            var solve = manager.ElectricalSolver.LatestSolveResult;
            return solve != null && solve.TryGetVoltage(a, b, out float value) ? value : 0;
        }


        private void Update()
        {
            if (!manager || !bindings) return;
            if (topologyDirty) ReadGraph();
            if (stage == 0 && Installed == parts.Length)
            {
                if (Award()) { SelectRoute(0); message = "Plate each marked via and connect the highlighted pads."; }
            }
            if (stage == 1 && Construction && jumperPresent &&
                Array.TrueForAll(witnesses, terminal => Voltage(terminal, ground) > 11f))
            {
                if (Award()) { BoardView(); message = "Apply bench power, then compare the two firmware banks."; }
            }
            TickFirmware(Mathf.Min(Time.unscaledDeltaTime, .08f));
            RefreshPanel();
            if (time >= saveAt) FlushCheckpoint();
        }

        private bool Award()
        {
            if (stage >= 6 || !bindings.SetRuntimeComponentBinding(Gate(stage), source)) return false;
            stage++;
            manager.QueueEvaluation();
            QueueCheckpoint();
            return true;
        }

        public void SelectFootprint(int index)
        {
            if (index < 0 || index >= parts.Length) return;
            footprint = index;
            seated = false;
        }
        public void SelectPart(int index)
        {
            if (index >= 0 && index < parts.Length) selectedPart = index;
        }
        public bool FitPart()
        {
            if (stage != 0 || powered || footprint != selectedPart || parts[footprint].solder.IsClosed)
                return Reject("Select the matching shelf part and isolate power before fitting.");
            seated = true;
            parts[footprint].model.SetActive(footprint != jumperIndex);
            message = "Component seated. Solder its contact.";
            return true;
        }
        public bool Solder()
        {
            if (stage != 0 || powered || !seated || parts[footprint].solder.IsClosed) return Reject("Fit the selected component first.");
            parts[footprint].solder.SetState(SparkSwitchState.Closed);
            installed++;
            QueueCheckpoint();
            seated = false;
            if (installed < parts.Length)
            {
                int next = footprint;
                do { next = (next + 1) % parts.Length; } while (parts[next].solder.IsClosed);
                SelectFootprint(next);
                SelectPart(next);
            }
            return true;
        }

        public void SelectRoute(int index)
        {
            if (index < 0 || index >= Count) return;
            selectedRoute = index;
            layer = Selected.layer;
            routing = true;
            if (manager) ReadGraph();
            if (focused) PadView();
        }
        private void SelectNextRoute()
        {
            for (int step = 1; step < Count; step++)
            {
                int next = (selectedRoute + step) % Count;
                if (!HasWire(next == routes.Length ? jumper : routes[next])) { SelectRoute(next); return; }
            }
        }
        public bool PlateVia()
        {
            var route = Selected;
            if (stage is < 1 or > 5 || powered || !route.via || route.via.IsClosed || layer != route.layer)
                return Reject("Select an open via on its entry layer.");
            SetContact(route.via, true);
            ReadGraph();
            QueueCheckpoint();
            message = $"{route.viaId} plated between L{route.layer:D2} and L{route.exitLayer:D2}.";
            return true;
        }
        private bool MayConnect(Route route) => ((stage is >= 1 and <= 5 && !powered) || stage == 5 && route == jumper)
            && layer == route.layer && (!route.via || route.via.IsConducting) && !HasWire(route);
        public bool ConnectPads()
        {
            var route = Selected;
            if (!MayConnect(route)) return Reject("Select the route's entry layer and plate its via first.");
            if (!manager.CircuitSystem.TryCreateConnection(route.from, route.to, SparkConnectionKind.Wire,
                SparkConnectionDirection.Bidirectional, out _)) return Reject("These pads could not connect.");
            ReadGraph();
            if (stage == 1 || RepairRouting) SelectNextRoute();
            return true;
        }
        private SparkTerminal PickTerminal(Vector2 screen, SparkTerminal start)
        {
            var route = Selected;
            if (!boardCamera || !routing || !MayConnect(route)) return null;
            var a = route.from;
            var b = route.to;
            if (start && start != a && start != b) return null;
            float limit = Mathf.Clamp(boardCamera.pixelHeight * .025f, 15f, 41f);
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
