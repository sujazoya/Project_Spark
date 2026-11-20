using System;
using System.Collections.Generic;
using AAAUI.VFX;
using ProjectSpark.Circuit;
using ProjectSpark.VoltageDominion;
using UnityEngine;

namespace ProjectSpark.Gameplay
{
    public sealed partial class SparkLevel110Process : MonoBehaviour
    {
        public const string Design = "PWR18-EDU-1";
        public static string Gate(int index) => $"L110_Proof_{index + 1}";

        [Serializable] public sealed class Part
        {
            public string id;
            public string label;
            public GameObject visual;
            public SparkSwitch contact;
        }

        [Serializable] public sealed class Route
        {
            public string id;
            public string group;
            public int layer;
            public SparkTerminal from, to;
            public LineRenderer copper;
            public SparkSwitch via;
            public string viaId;
            public int exitLayer;
        }

        [Serializable] public sealed class Branch
        {
            public SparkTerminal sense;
            public string name;
        }

        public SparkLevelLayout layout;
        public SparkLevelSceneBindings bindings;
        public Transform board;
        public Part[] parts;
        public Route[] routes;
        public Route jumper;
        public Branch[] branches;
        public SparkPowerSupply source, healthTransmitter;
        public SparkTerminal input, ground, healthReceiver;
        public SparkSwitch workloadInterlock;
        public GameObject completedReference;
        public GameObject[] cooling;
        public int[] faultParts;
        private LevelGamePlayManager manager;
        private SignalWireBuilder wire;
        private SparkGameplayController gameplay;
        private readonly HashSet<int> placed = new();
        private readonly HashSet<int> soldered = new();
        private readonly List<SparkCircuitConnection> edges = new();
        private readonly SparkDominionVrm model = new();
        private readonly Vector2[] trace = new Vector2[96];
        private readonly bool[] repaired = new bool[3];
        private int traceHead, traceCount, simulationSteps, routeIndex, layer = 1, footprint, inventory, orientation;
        private int stage, fault = -1, pendingRepair = -1, faultEvidence, revision, runRevision;
        private int routed, plated;
        private double accumulator, healthClock = double.NegativeInfinity;
        private float scopeMinimum, scopeMaximum;
        private double producerClock = double.NegativeInfinity;
        private double measuredAt = double.NegativeInfinity;
        private double runStart, runMinimum, runMaximum, runRipple, runSpread, runEfficiency;
        private double lastDroop, lastOvershoot, lastRipple, lastSpread, lastEfficiency;
        private double nativeInput, nextUi;
        private bool powered, calibrated, testing, testQualified, healthy, inhibited = true, lossObserved, resetVerified;
        private bool topologyDirty = true, unauthorized, ready, showCopper, flux, protectionVerified, signalReceived;
        private bool protectedCapture, coolingMounted;
        private string message = "Select the matching shelf part, align it, fit it, then flux and solder.";
        private string lastReading = "No trace captured.";
        private Camera camera;
        public int Stage => stage;
        public int Installed => soldered.Count;
        public int Routed => routed;
        public int Plated => plated;
        public int SelectedRoute => routeIndex;
        public bool Powered => powered;
        public bool Testing => testing;
        public bool HasQualifiedRun => testQualified;
        public bool Inhibited => inhibited;
        public bool HealthPresent => model.Time - healthClock <= .32;
        public SparkDominionVrm Model => model;
        public bool CapturedTransient
        {
            get
            {
                for (int i = 0; i < traceCount; i++)
                    if (trace[i].y - trace[i].x >= .10f) return true;
                return false;
            }
        }

        private void Start()
        {
            manager = FindFirstObjectByType<LevelGamePlayManager>();
            var loader = FindFirstObjectByType<SparkLevelLayoutLoader>();
            ready = manager && loader && loader.ActiveLayout == layout;
            if (!ready) { enabled = false; return; }
            camera = Camera.main;
            gameplay = FindFirstObjectByType<SparkGameplayController>();
            wire = FindFirstObjectByType<SignalWireBuilder>();
            if (wire)
            {
                wire.TerminalPickerOverride = PickTerminal;
                wire.TerminalWireConnected += OnWire;
            }
            manager.CircuitSystem.TopologyChanged += OnTopology;
            manager.ElectricalSolver.SolveCompleted += ReadNative;
            foreach (var part in parts) part.visual.SetActive(false);
            completedReference.SetActive(false);
            source.SetOutput(true, default);
            manager.ElectricalSolver.SolveNow();
            FitTable();
            BuildPanel();
            Home();
        }

        private void OnDisable()
        {
            if (manager && ready)
            {
                manager.CircuitSystem.TopologyChanged -= OnTopology;
                manager.ElectricalSolver.SolveCompleted -= ReadNative;
            }
            if (wire)
            {
                wire.TerminalWireConnected -= OnWire;
                if (wire.TerminalPickerOverride == PickTerminal)
                {
                    if (wire.IsDrawing) wire.CancelWire();
                    wire.TerminalPickerOverride = null;
                }
            }
            RestoreCamera();
        }

        private void OnTopology() => topologyDirty = true;
        private void OnWire(SparkTerminal a, SparkTerminal b, SignalPath path)
        {
            ReadTopology();
            if (routeIndex < routes.Length && HasEdge(routes[routeIndex])) NextUnfinishedRoute();
        }

        private Route Selected => routeIndex == routes.Length ? jumper : routes[routeIndex];
        private bool HasEdge(Route route) => manager.CircuitSystem.HasConnectionBetween(route.from, route.to);
        private bool CompleteRoute(Route route) => HasEdge(route) && (!route.via || route.via.IsConducting);

        private void ReadTopology()
        {
            topologyDirty = false;
            int total = 0, vias = 0;
            foreach (var route in routes)
            {
                if (CompleteRoute(route)) total++;
                if (route.via && route.via.IsConducting) vias++;
                route.copper.enabled = showCopper && route.layer == layer && HasEdge(route);
            }
            jumper.copper.enabled = showCopper && layer == jumper.layer && HasEdge(jumper);
            edges.Clear();
            manager.CircuitSystem.CopyConnections(edges);
            unauthorized = false;
            foreach (var edge in edges)
            {
                if (edge.Kind != SparkConnectionKind.Wire
                    || !(edge.A.transform.IsChildOf(transform) || edge.B.transform.IsChildOf(transform))) continue;
                if (!Matches(edge, jumper) && Array.Find(routes, route => Matches(edge, route)) == null) unauthorized = true;
            }
            bool connected = HasEdge(jumper);
            if (routed != total || plated != vias || connected != jumperPresent || unauthorized != previousUnauthorized)
            {
                revision++;
                measuredAt = double.NegativeInfinity;
                testQualified = false;
                resetVerified = false;
                if (jumperPresent && !connected && stage == 5 && powered && !inhibited) lossObserved = true;
            }
            routed = total;
            plated = vias;
            jumperPresent = connected;
            previousUnauthorized = unauthorized;
        }

        private static bool Matches(SparkCircuitConnection edge, Route route) =>
            edge.A == route.from && edge.B == route.to || edge.B == route.from && edge.A == route.to;

        private bool jumperPresent, previousUnauthorized;
        private void ReadNative()
        {
            var solve = manager.ElectricalSolver.LatestSolveResult;
            nativeInput = solve != null && solve.TryGetVoltage(input, ground, out var volts) ? volts : 0;
            healthy = solve != null && Array.TrueForAll(branches, branch =>
                solve.TryGetVoltage(branch.sense, ground, out var voltage) && voltage > 11.6f);
        }

        private bool SignalReceived()
        {
            var solve = manager.ElectricalSolver.LatestSolveResult;
            return jumperPresent && solve != null
                && solve.TryGetVoltage(healthReceiver, ground, out var signal) && signal > 2.5f;
        }

        private void Update()
        {
            if (!ready) return;
            if (topologyDirty) ReadTopology();
            signalReceived = SignalReceived();
            if (stage == 0 && Installed == parts.Length && coolingMounted) Award();
            if (stage == 1 && routed == routes.Length && plated == 56 && jumperPresent && healthy && !unauthorized) Award();

            accumulator += Math.Min(Time.unscaledDeltaTime, .048);
            int steps = (int)(accumulator / SparkDominionVrm.StepSeconds);
            accumulator -= steps * SparkDominionVrm.StepSeconds;
            for (int i = 0; i < steps; i++) Tick();
            bool transmit = powered && calibrated && model.Time - producerClock <= .32;
            if (healthTransmitter.IsOutputActive != transmit) healthTransmitter.SetOutput(transmit, default);
            bool interlock = powered && !inhibited && model.Trip == SparkDominionVrm.Protection.None;
            if (workloadInterlock.IsClosed != interlock)
                workloadInterlock.SetState(interlock ? SparkSwitchState.Closed : SparkSwitchState.Open);
            if (Time.unscaledTimeAsDouble >= nextUi)
            {
                nextUi = Time.unscaledTimeAsDouble + .10;
                RefreshPanel();
            }
        }

        private void Tick()
        {
            bool path = healthy && !unauthorized;
            bool command = powered && path;
            double age = model.Time - runStart;
            // The virtual probe load bank can stimulate the regulator with the CPU isolated.
            // After the safety reset this same profile runs through the protected workload.
            bool loadStep = testing && age >= .20 && age < .66 && (!protectedCapture || !inhibited);
            double amps = loadStep ? 8 : 2;
            model.Step(nativeInput, command, amps);
            if (command && model.PowerGood && calibrated) producerClock = model.Time;
            if (command && signalReceived && model.Time - producerClock <= .32) healthClock = producerClock;
            if (!jumperPresent || !HealthPresent || !command) inhibited = true;
            if (testing)
            {
                float volts = (float)model.Voltage;
                if (simulationSteps++ == 0) scopeMinimum = scopeMaximum = volts;
                else
                {
                    scopeMinimum = Math.Min(scopeMinimum, volts);
                    scopeMaximum = Math.Max(scopeMaximum, volts);
                }
                if (simulationSteps == 500)
                {
                    trace[traceHead] = new Vector2(scopeMinimum, scopeMaximum);
                    traceHead = (traceHead + 1) % trace.Length;
                    traceCount = Math.Min(traceCount + 1, trace.Length);
                    simulationSteps = 0;
                }
            }

            if (!testing) return;
            if (runRevision != revision || !command || protectedCapture && inhibited)
            {
                testing = false;
                message = "Capture canceled because the board changed or input was lost.";
                return;
            }
            if (age >= .20)
            {
                runMinimum = Math.Min(runMinimum, model.Voltage);
                runMaximum = Math.Max(runMaximum, model.Voltage);
                runRipple = Math.Max(runRipple, model.Ripple);
                runSpread = Math.Max(runSpread, model.CurrentSpread);
                if (age >= .38 && age < .66) runEfficiency = Math.Min(runEfficiency, model.Efficiency);
            }
            if (age >= .92) FinishCapture();
        }

        private void FinishCapture()
        {
            testing = false;
            lastDroop = SparkDominionVrm.Setpoint - runMinimum;
            lastOvershoot = runMaximum - SparkDominionVrm.Setpoint;
            lastRipple = runRipple;
            lastSpread = runSpread;
            lastEfficiency = runEfficiency;
            testQualified = model.Trip == SparkDominionVrm.Protection.None && model.PowerGood
                && lastDroop < .22 && lastOvershoot < .20 && lastRipple < .014 && lastSpread < .25 && lastEfficiency > .82;
            lastReading = $"Droop {lastDroop * 1000:F0} mV · Overshoot {lastOvershoot * 1000:F0} mV\n"
                + $"Ripple estimate {lastRipple * 1000:F1} mVpp · Spread {lastSpread:F2} A · Efficiency {lastEfficiency:P0}";
            message = testQualified ? "Load-step capture meets the training limits." : "Capture exceeds a training limit. Compare the evidence channels.";
            if (fault == 0 && (lastDroop >= .22 || lastRipple >= .014 || model.Trip != SparkDominionVrm.Protection.None)) faultEvidence |= 1;
            if (fault == 1 && lastSpread >= .25) faultEvidence |= 1;
            if (fault == 2 && (!model.PowerGood || lastOvershoot >= .19)) faultEvidence |= 1;
            if (!testQualified) return;
            measuredAt = model.Time;
            if (stage == 2)
            {
                Award();
                if (Array.TrueForAll(repaired, value => value)) Award();
            }
            else if (stage == 3 && pendingRepair >= 0)
            {
                repaired[pendingRepair] = true;
                pendingRepair = -1;
                if (Array.TrueForAll(repaired, value => value)) Award();
            }
            else if (stage == 4 && protectionVerified)
            {
                if (Award())
                {
                    inhibited = !HealthPresent;
                    routeIndex = routes.Length;
                    layer = jumper.layer;
                    message = "Commissioned. Disconnect W173 while the rail is live and observe the interlock.";
                }
            }
            else if (stage == 5 && protectedCapture && lossObserved && resetVerified && !inhibited && HealthPresent && jumperPresent
                && workloadInterlock.IsConducting) Award();
        }

        private bool Award()
        {
            if (stage >= 6 || !bindings.SetRuntimeComponentBinding(Gate(stage), source)) return false;
            stage++;
            manager.QueueEvaluation();
            SaveBoard();
            return true;
        }

        public void SelectFootprint(int index)
        {
            if (index < 0 || index >= parts.Length) return;
            footprint = index;
            flux = false;
            orientation = 0;
        }

        public void SelectInventory(int index)
        {
            if (index < 0 || index >= parts.Length) return;
            inventory = index;
            orientation = 0;
        }

        public void RotatePart() => orientation = (orientation + 90) % 360;
        public bool Fit()
        {
            if (powered || stage != 0 || footprint != inventory || orientation != 0 || soldered.Contains(footprint))
            {
                message = "Select the matching part, align its reference mark and isolate power before fitting.";
                return false;
            }
            if (!placed.Add(footprint)) return false;
            parts[footprint].visual.SetActive(true);
            flux = false;
            message = "Part seated. Flux and solder its contact.";
            return true;
        }

        public void ApplyFlux()
        {
            flux = placed.Contains(footprint) && !powered && !soldered.Contains(footprint);
            if (!flux) message = "Fit the selected component before applying flux.";
        }

        public bool Solder()
        {
            if (!flux || powered || !placed.Contains(footprint) || !soldered.Add(footprint)) return false;
            parts[footprint].contact.SetState(SparkSwitchState.Closed);
            flux = false;
            for (int i = 1; i < parts.Length && soldered.Count < parts.Length; i++)
            {
                int next = (footprint + i) % parts.Length;
                if (soldered.Contains(next)) continue;
                SelectFootprint(next);
                SelectInventory(next);
                break;
            }
            return true;
        }

        public bool MountCooling()
        {
            if (powered || Installed != parts.Length || coolingMounted) return false;
            foreach (var piece in cooling) piece.SetActive(true);
            coolingMounted = true;
            message = "Cooling hardware mounted. Select a route to construct the training network.";
            return true;
        }

        public void SelectRoute(int index)
        {
            if (index < 0 || index > routes.Length) return;
            routeIndex = index;
            layer = Selected.layer;
            SetCopperView(true);
        }

        private void NextUnfinishedRoute()
        {
            for (int i = 1; i <= routes.Length; i++)
            {
                int next = (routeIndex + i) % (routes.Length + 1);
                Route route = next < routes.Length ? routes[next] : jumper;
                if (!CompleteRoute(route)) { SelectRoute(next); return; }
            }
        }

        public bool PlateVia()
        {
            Route selected = Selected;
            if (stage != 1 || powered || !selected.via || selected.via.IsConducting || layer != selected.layer) return false;
            selected.via.SetState(SparkSwitchState.Closed);
            ReadTopology();
            message = $"{selected.viaId} plated between L{selected.layer:D2} and L{selected.exitLayer:D2}.";
            return true;
        }

        private bool MayRoute(Route route) =>
            (stage == 1 && !powered || stage == 5 && route == jumper) && layer == route.layer
            && (!route.via || route.via.IsConducting) && !HasEdge(route);

        public bool ConnectRoute()
        {
            var route = Selected;
            if (!MayRoute(route)) return false;
            if (!manager.CircuitSystem.TryCreateConnection(route.from, route.to, SparkConnectionKind.Wire,
                SparkConnectionDirection.Bidirectional, out _)) return false;
            ReadTopology();
            if (stage == 1) NextUnfinishedRoute();
            return true;
        }

        public bool BreakJumper()
        {
            if (stage != 5 || !powered || !jumperPresent) return false;
            foreach (var edge in edges)
            {
                if (!Matches(edge, jumper) || edge.Kind != SparkConnectionKind.Wire) continue;
                if (!wire || !wire.RemoveConnectedWire(edge.Id)) manager.CircuitSystem.TryRemoveConnection(edge.Id, out _);
                ReadTopology();
                inhibited = true;
                healthClock = double.NegativeInfinity;
                workloadInterlock.SetState(SparkSwitchState.Open);
                message = "W173 open. Protected workload isolated. Low-power rail remains available.";
                return true;
            }
            return false;
        }

        private SparkTerminal PickTerminal(Vector2 screen, SparkTerminal start)
        {
            var route = Selected;
            if (!camera || !showCopper || !MayRoute(route)) return null;
            if (start && start != route.from && start != route.to) return null;
            SparkTerminal best = null;
            float limit = Mathf.Clamp(Screen.height * .019f, 14, 42);
            float distance = limit * limit;
            foreach (var terminal in new[] { route.from, route.to })
            {
                if (terminal == start) continue;
                Vector3 point = camera.WorldToScreenPoint(terminal.transform.position);
                float candidate = ((Vector2)point - screen).sqrMagnitude;
                if (point.z > 0 && candidate < distance) { distance = candidate; best = terminal; }
            }
            return best;
        }

        public bool Power()
        {
            if (powered)
            {
                powered = false;
                calibrated = testing = testQualified = resetVerified = false;
                measuredAt = double.NegativeInfinity;
                producerClock = healthClock = double.NegativeInfinity;
                inhibited = true;
                workloadInterlock.SetState(SparkSwitchState.Open);
                message = "Input isolated. Wait for the output to discharge before rework.";
                return true;
            }
            if (stage < 2 || !healthy || unauthorized || !model.ResetProtection()) return false;
            powered = true;
            if (stage == 5 && !jumperPresent) inhibited = true;
            return true;
        }

        public bool Calibrate()
        {
            calibrated = powered && nativeInput > 11.5 && model.Trip == SparkDominionVrm.Protection.None
                && model.Voltage > .97 && model.Voltage < 1.14 && model.CurrentSpread < .25;
            message = calibrated ? "Voltage and independent phase probes calibrated." : "Wait for a healthy rail and balanced phase currents before calibration.";
            return calibrated;
        }

        public bool Capture()
        {
            if (!powered || !healthy || testing || stage < 2 || (fault < 0 && (!calibrated || !model.PowerGood))) return false;
            testing = true;
            protectedCapture = stage == 5 && resetVerified && workloadInterlock.IsConducting;
            testQualified = false;
            traceHead = traceCount = simulationSteps = 0;
            runRevision = revision;
            runStart = model.Time;
            runMinimum = 2;
            runMaximum = runRipple = runSpread = 0;
            runEfficiency = 1;
            message = "Capturing idle, an 8 A load step, and load release.";
            return true;
        }

        public bool InjectNextFault()
        {
            if (stage != 3 || !powered || fault >= 0 || pendingRepair >= 0 || testing) return false;
            fault = Array.FindIndex(repaired, value => !value);
            model.Fault = (SparkDominionVrm.Defect)(fault + 1);
            faultEvidence = 0;
            calibrated = testQualified = false;
            revision++;
            message = "A seeded fault is active. Capture a load step, then compare diagnostic channels.";
            return true;
        }

        public bool Diagnose()
        {
            if (fault < 0 || (faultEvidence & 1) == 0) return false;
            if (fault == 0)
            {
                if (powered || model.Voltage > .08) { message = "Discharge the rail before measuring capacitance and ESR."; return false; }
                if (model.Esr < .01 || model.Capacitance > .002) return false;
                lastReading = $"Filter diagnostic: {model.Capacitance * 1e6:F0} µF / {model.Esr * 1000:F1} mΩ ESR.";
            }
            else if (fault == 1)
            {
                if (!powered || Math.Abs(model.ControllerCurrent(2) - model.PhaseCurrent(2)) < .5) return false;
                lastReading = $"Phase 3 independent {model.PhaseCurrent(2):F2} A / controller {model.ControllerCurrent(2):F2} A.";
            }
            else
            {
                if (!powered || Math.Abs(model.Voltage - SparkDominionVrm.Setpoint) < .025 && lastOvershoot < .19) return false;
                lastReading = $"Feedback {model.FeedbackVoltage:F3} V. VCORE {model.Voltage:F3} V, commanded {SparkDominionVrm.Setpoint:F3} V.";
            }
            faultEvidence |= 2;
            message = $"Evidence supports {parts[faultParts[fault]].label}. Select that footprint and isolate power before replacement.";
            return true;
        }

        public bool Repair()
        {
            if (fault < 0 || faultEvidence != 3 || footprint != faultParts[fault] || powered || model.Voltage > .08) return false;
            using (manager.ElectricalSolver.BeginConfigurationBatch())
            {
                var contact = parts[footprint].contact;
                contact.SetState(SparkSwitchState.Open);
                model.Fault = SparkDominionVrm.Defect.None;
                contact.SetState(SparkSwitchState.Closed);
            }
            pendingRepair = fault;
            fault = -1;
            calibrated = testQualified = false;
            revision++;
            message = "Replacement fitted and resoldered. Recommission, calibrate and repeat the load step.";
            return true;
        }

        public bool TestProtection()
        {
            if (stage != 4 || !powered || !calibrated || testing) return false;
            protectionVerified = ProtectionBench();
            message = protectionVerified ? "Protection bench passed UVP, OVP, OCP and thermal cutoff. Capture the board load step."
                : "The protection bench did not observe all four trips.";
            return protectionVerified;
        }

        public static bool ProtectionBench()
        {
            var uvp = new SparkDominionVrm();
            var ovp = new SparkDominionVrm();
            var ocp = new SparkDominionVrm();
            var otp = new SparkDominionVrm { CoolingThermalResistance = 420 };
            uvp.Step(8, true, 2);
            ovp.Step(16, true, 2);
            ocp.Step(12, true, 28);
            for (int i = 0; i < 160000 && otp.Trip == SparkDominionVrm.Protection.None; i++) otp.Step(12, true, 14);
            return uvp.Trip == SparkDominionVrm.Protection.UnderVoltage && ovp.Trip == SparkDominionVrm.Protection.OverVoltage
                && ocp.Trip == SparkDominionVrm.Protection.OverCurrent && otp.Trip == SparkDominionVrm.Protection.OverTemperature;
        }

        public bool ResetSafety()
        {
            if (stage != 5 || !lossObserved || !jumperPresent || !HealthPresent || !testQualified
                || model.Time - measuredAt > 12 || !model.PowerGood || !powered || !SignalReceived()) return false;
            inhibited = false;
            resetVerified = true;
            workloadInterlock.SetState(SparkSwitchState.Closed);
            message = "Independent reset accepted. Make one fresh protected load capture.";
            return true;
        }
    }
}
