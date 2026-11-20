using System;
using System.Collections.Generic;
using AAAUI.VFX;
using ProjectSpark.Circuit;
using UnityEngine;

namespace ProjectSpark.Gameplay
{
    public sealed partial class SparkLevel108Process : MonoBehaviour
    {
        public const string Design = "DFX16-EDU-1";
        public static string Gate(int index) => $"L108_Proof_{index + 1}";

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
        public SparkSwitch benchPower, leakContact, memoryContact, clockContact, heartbeat, workload;
        public SparkTerminal ground, input, shuntFrom, shuntTo, memory, clock, receiver, protectedFeed;
        public int[] repairParts;
        public Renderer placementMarker, fromMarker, toMarker;
        private readonly List<SparkCircuitConnection> edges = new();
        private readonly List<string> notebook = new();
        private readonly bool[] supported = new bool[3];
        private readonly bool[] repaired = new bool[3];
        private LevelGamePlayManager manager;
        private SignalWireBuilder wire;
        private SparkWireRegistry registry;
        private Camera boardCamera;
        private int footprint, selectedPart, selectedRoute, layer = 1, installed, routed, plated;
        private int stage, incident, evidence, qualification, transitions, bootStage;
        private bool seated, powered, topologyDirty = true, jumperPresent, unauthorized, routing, interrupted, reset, lastHigh, armReady;
        private bool testing, sampleOK;
        private float elapsed, testAge, heat = 24f, nextPulse, lastFresh = float.NegativeInfinity;
        private float baselineCurrent, peakCurrent;
        private int clockDrops, memoryDrops;
        private string message = "Fit the matching board part, then solder its contact.";
        private string measurement = "No capture recorded.";
        private Test runningTest;

        public enum Test { Standby, Isolate, ColdBoot, WarmMemory, PCIeClock }
        public int Stage => stage;
        public int Installed => installed;
        public int Routed => routed;
        public int Plated => plated;
        public int SelectedRoute => selectedRoute;
        public bool Testing => testing;
        public bool Powered => powered;
        public bool Inhibited => !workload.IsConducting;
        public bool Fresh => elapsed - lastFresh < .42f && transitions >= 2 && jumperPresent;
        public bool Qualified => qualification == 7;
        public string LastCapture => measurement;
        public bool IsSupported(int index) => supported[index];
        public bool IsRepaired(int index) => repaired[index];
        private int Count => routes.Length + 1;
        private Route Selected => selectedRoute == routes.Length ? jumper : routes[selectedRoute];
        private bool Construction => installed == parts.Length && routed == routes.Length && plated == 50 && !unauthorized;
        private bool GraphPowered => Construction && powered && jumperPresent;
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
            if (stage == 1 && HasWire(Selected)) SelectNextRoute();
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
            if (stage < 2 || (previousRouted == routed && previousJumper == jumperPresent && previousUnauthorized == unauthorized)) return;
            if (stage == 5 && !jumperPresent && previousJumper && armReady && powered)
            {
                interrupted = true;
                reset = false;
                message = "W157 opened. The supervisor isolated the protected output.";
            }
            qualification = transitions = 0;
            lastFresh = float.NegativeInfinity;
            testing = false;
            bootStage = 0;
            workload.SetState(SparkSwitchState.Open);
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
            if (stage == 0 && installed == parts.Length) Award();
            if (stage == 1 && Construction && jumperPresent &&
                Array.TrueForAll(witnesses, terminal => Voltage(terminal, ground) > 11f)) Award();

            float delta = Mathf.Min(Time.unscaledDeltaTime, .09f);
            elapsed += delta;
            bool isolation = testing && runningTest == Test.Isolate;
            bool warm = testing && (runningTest == Test.WarmMemory || runningTest == Test.PCIeClock);
            heat = Mathf.MoveTowards(heat, warm ? 68f : 24f, delta * (warm ? 77f : 95f));
            SetContact(leakContact, powered && !repaired[0] && !isolation);
            bool standbyReady = powered && Voltage(shuntTo, ground) > 10f && Mathf.Abs(Voltage(shuntFrom, shuntTo)) < .20f;
            SetContact(memoryContact, standbyReady && (repaired[1] || heat < 51f));
            bool stress = testing && runningTest == Test.PCIeClock && heat > 45f;
            SetContact(clockContact, standbyReady && (!stress || repaired[2] || Mathf.FloorToInt(testAge / .16f) % 2 == 0));
            UpdateSafety();
            if (testing) RecordSample(delta);
            if (stage == 3 && Array.TrueForAll(supported, value => value))
            {
                Award();
                message = "Complete repairs and capture fresh cold boot, warm memory and PCIe passes.";
            }
            if (stage == 4 && Array.TrueForAll(repaired, value => value) && Qualified)
            {
                Award();
                message = "Wait for fresh W157 heartbeat, then open the jumper under power.";
            }
            if (stage == 5 && interrupted && reset && Qualified && Fresh && !Inhibited &&
                Voltage(protectedFeed, ground) > 11f)
            {
                Award();
                message = "Protected diagnostic operation verified. DFX-16 qualification complete.";
            }
            RefreshPanel();
        }

        private static void SetContact(SparkSwitch contact, bool closed)
        {
            if (contact.IsClosed != closed) contact.SetState(closed ? SparkSwitchState.Closed : SparkSwitchState.Open);
        }

        private void UpdateSafety()
        {
            bool producerReady = stage >= 5 && powered && Construction && Array.TrueForAll(repaired, value => value)
                && Voltage(shuntTo, ground) > 10f && Voltage(memory, ground) > 10f;
            if (producerReady && elapsed >= nextPulse)
            {
                nextPulse = elapsed + .14f;
                SetContact(heartbeat, !heartbeat.IsClosed);
                manager.ElectricalSolver.SolveNow();
            }
            else if (!producerReady) SetContact(heartbeat, false);
            bool high = producerReady && jumperPresent && Voltage(receiver, ground) > 2.5f;
            if (high && !lastHigh) { transitions++; lastFresh = elapsed; }
            lastHigh = high;
            if (!producerReady || !jumperPresent) { transitions = 0; lastFresh = float.NegativeInfinity; }
            SetContact(workload, stage >= 5 && GraphPowered && Fresh && (!interrupted || reset));
            if (!Inhibited && !interrupted && stage == 5) armReady = true;
        }

        private void Award()
        {
            if (stage >= 6 || !bindings.SetRuntimeComponentBinding(Gate(stage), source)) return;
            stage++;
            manager.QueueEvaluation();
            if (stage == 1) { SelectRoute(0); message = "Plate each marked via and connect the highlighted pads."; }
            if (stage == 2) message = "Switch on bench power and capture the standby baseline.";
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
            parts[footprint].model.SetActive(true);
            message = "Component seated. Solder its contact.";
            return true;
        }
        public bool Solder()
        {
            if (stage != 0 || powered || !seated || parts[footprint].solder.IsClosed) return Reject("Fit the selected component first.");
            parts[footprint].solder.SetState(SparkSwitchState.Closed);
            installed++;
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
            if (stage != 1 || powered || !route.via || route.via.IsClosed || layer != route.layer)
                return Reject("Select an open via on its entry layer.");
            SetContact(route.via, true);
            ReadGraph();
            message = $"{route.viaId} plated between L{route.layer:D2} and L{route.exitLayer:D2}.";
            return true;
        }
        private bool MayConnect(Route route) => (stage == 1 && !powered || stage == 5 && route == jumper)
            && layer == route.layer && (!route.via || route.via.IsConducting) && !HasWire(route);
        public bool ConnectPads()
        {
            var route = Selected;
            if (!MayConnect(route)) return Reject("Select the route's entry layer and plate its via first.");
            if (!manager.CircuitSystem.TryCreateConnection(route.from, route.to, SparkConnectionKind.Wire,
                SparkConnectionDirection.Bidirectional, out _)) return Reject("These pads could not connect.");
            ReadGraph();
            if (stage == 1) SelectNextRoute();
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

        public bool TogglePower()
        {
            if (!powered && (stage < 2 || !Construction || !jumperPresent)) return Reject("Finish the routing and connect W157 before applying bench power.");
            powered = !powered;
            testing = false;
            bootStage = 0;
            qualification = 0;
            SetContact(benchPower, powered);
            if (!powered) SetContact(workload, false);
            message = powered ? "Bench power on. Capture a diagnostic trace." : "Bench isolated for rework.";
            return true;
        }

        public bool Run(Test test)
        {
            if (stage < 2 || stage >= 6 || !GraphPowered || testing) return Reject("Close W157 and apply bench power before starting a capture.");
            if (stage == 2 && test != Test.Standby) return Reject("Record the initial standby baseline first.");
            testAge = 0;
            clockDrops = memoryDrops = 0;
            peakCurrent = 0;
            sampleOK = true;
            bootStage = 0;
            runningTest = test;
            testing = true;
            return true;
        }

        private void RecordSample(float delta)
        {
            testAge += delta;
            float standbyCurrent = Mathf.Abs(Voltage(shuntFrom, shuntTo));
            float memoryVoltage = Voltage(memory, ground);
            float clockVoltage = Voltage(clock, ground);
            if (testAge > .62f)
            {
                peakCurrent = Mathf.Max(peakCurrent, standbyCurrent);
                if (memoryVoltage < 10f) memoryDrops++;
                if (clockVoltage < 10f) clockDrops++;
                sampleOK &= Voltage(shuntTo, ground) > 10f && standbyCurrent < .20f;
            }
            if (bootStage < 8 && sampleOK && standbyCurrent < .20f && Voltage(shuntTo, ground) > 10f &&
                testAge >= .16f * (bootStage + 1) && (bootStage < 5 || memoryVoltage > 10f) &&
                (bootStage < 6 || clockVoltage > 10f)) bootStage++;
            if (testAge < 1.65f) return;
            testing = false;
            measurement = $"{runningTest}  ·  standby {peakCurrent:F3} A  ·  {heat:F0} °C  ·  low samples DDR {memoryDrops} / CLK {clockDrops}";
            notebook.Add(measurement);
            if (stage == 2)
            {
                baselineCurrent = peakCurrent;
                evidence = baselineCurrent > .20f ? 1 : 0;
                Award();
                message = "Compare standby current with the suspect branch isolated.";
                return;
            }
            if (incident == 0 && runningTest == Test.Isolate && sampleOK && baselineCurrent - peakCurrent > .16f) evidence |= 2;
            if (incident == 1 && runningTest == Test.ColdBoot && sampleOK && memoryDrops == 0 && bootStage == 8) evidence |= 1;
            if (incident == 1 && runningTest == Test.WarmMemory && sampleOK && memoryDrops > 0) evidence |= 2;
            if (incident == 2 && runningTest == Test.ColdBoot && sampleOK && clockDrops == 0 && bootStage == 8) evidence |= 1;
            if (incident == 2 && runningTest == Test.PCIeClock && sampleOK && clockDrops > 0 && Voltage(shuntTo, ground) > 10f) evidence |= 2;

            if (incident == 3 && sampleOK && bootStage == 8 && memoryDrops == 0 && clockDrops == 0)
            {
                int bit = runningTest == Test.ColdBoot ? 1 : runningTest == Test.WarmMemory ? 2 : runningTest == Test.PCIeClock ? 4 : 0;
                qualification |= bit;
            }
            else if (incident == 3 && runningTest >= Test.ColdBoot) qualification = 0;
            message = "Capture recorded in the diagnostic notebook.";
        }

        public bool ConfirmDiagnosis()
        {
            if (stage < 3 || incident >= 3 || evidence != 3 || testing) return Reject("Capture a baseline and a discriminating test for this fault.");
            supported[incident] = true;
            notebook.Add($"F{incident + 1} supported by comparative measurements.");
            message = $"Root cause supported at {parts[repairParts[incident]].label}. Isolate power before rework.";
            return true;
        }
        public void LocateRepair()
        {
            if (incident < 3 && supported[incident]) SelectFootprint(repairParts[incident]);
        }
        public bool Repair()
        {
            if (incident >= 3 || !supported[incident] || powered || footprint != repairParts[incident])
                return Reject("Confirm the cause, select its footprint and isolate power.");
            repaired[incident] = true;
            notebook.Add($"F{incident + 1} reworked at {parts[footprint].label}.");
            incident++;
            evidence = 0;
            qualification = 0;
            message = incident < 3 ? "Apply power and compare cold operation with the stressed subsystem."
                : "Apply power and repeat all three qualification captures.";
            return true;
        }
        public bool OpenW157()
        {
            if (stage != 5 || !powered || !armReady || !jumperPresent || Inhibited || interrupted)
                return Reject("Wait for W157 freshness and protected output enable.");
            registry?.RefreshNow();
            if (registry)
                foreach (var item in registry.Wires)
                    if ((item.TerminalA == jumper.from && item.TerminalB == jumper.to ||
                         item.TerminalA == jumper.to && item.TerminalB == jumper.from) && registry.RemoveWire(item.WireId))
                    { ReadGraph(); SelectRoute(routes.Length); return true; }
            foreach (var edge in edges)
                if (edge.Kind == SparkConnectionKind.Wire && Matches(edge, jumper) && manager.CircuitSystem.TryRemoveConnection(edge.Id, out _))
                { ReadGraph(); SelectRoute(routes.Length); return true; }
            return Reject("W157 was already open.");
        }
        public bool ResetSafety()
        {
            if (stage != 5 || !interrupted || reset || !GraphPowered || !Fresh || !Qualified || testing)
                return Reject("Restore W157 and record fresh cold, warm and PCIe qualification captures.");
            reset = true;
            notebook.Add("Supervisor reset after fresh heartbeat and complete retest.");
            return true;
        }

        private bool Reject(string text) { message = text; return false; }
    }
}
