using System.Collections.Generic;
using ProjectSpark.Circuit;
using ProjectSpark.Electrical;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.Gameplay
{
    [DisallowMultipleComponent]
    public sealed class SparkLevel49Shear : MonoBehaviour
    {
        public static readonly string[] Gates = { "L49_Wired", "L49_Calibrated", "L49_Thirty", "L49_Sixty", "L49_Load", "L49_Recovery" };
        private const float Tick = .005f, Radius = .020f, Unit = 2 * Mathf.PI * Radius / 80;
        private const int Pitch = 160;
        private const float Offset = 80 * Unit, TrackingTravel = .025f, SyncTolerance = .006f;
        private const float RailLimit = .055f, RailAcceleration = 2.8f;
        private static readonly int[] Gray = {0,2,3,1};
        private static readonly int[] Decode = {0,-1,1,0, 1,0,0,-1, -1,0,0,1, 0,1,-1,0};
        private static readonly (int a,int b)[] Wires = {
            (0,1),(2,3),(0,4),(2,5),(6,7),(8,9),(10,11),(12,13),(14,15),(16,17),(18,19),(0,20),(2,21),(22,23),(24,25)
        };
        private enum Mode { Off, Homing, Calibrating, Manual, Auto, Fault }
        private enum Cycle { Ready, Approach, Tracking, Brake, Return }

        public SparkLevelLayout layout;
        public SparkPowerSupply source;
        public SparkMotor transportMotor, printMotor;
        public SparkSwitch transportDrive, printDrive, guardContact, homeContact;
        public SparkTerminal[] pins;
        public SparkLevel49VisualRig rig;
        public Transform[] webSlats;
        public GameObject[] lamps;
        public TMP_Text controllerDisplay, qualityDisplay;
        public Text statusText, meterText, detailText;
        public Button powerButton, homeButton, calibrateButton, manualButton, startButton, stopButton;
        public Button slowButton, fastButton, loadButton, guardButton, resetButton;

        private readonly List<SparkCircuitConnection> connections = new();
        private readonly Dictionary<SparkTerminal,SparkTerminal> nets = new();
        private SparkLevelLayoutLoader loader;
        private LevelGamePlayManager manager;
        private SparkLevelSceneBindings bindings;
        private SparkCircuitSystem circuit;
        private SparkElectricalSolver solver;
        private Mode mode;
        private Cycle cycle;
        private int topology = -1, stage, edge, count, phase, lastMark = -1, nextMark = Pitch, marks, pitch;
        private int accepted, rejected, consecutive, requestedRpm = 30;
        private float accumulator, travel, velocity, position = .018f, carriageVelocity, stroke, target = -1, markPulse;
        private float cutExpected, cutActual, error, peakSyncError, loadTime, loadBaseline, trajectoryChange;
        private float cycleRpm, driveWait;
        private int loadStartCount;
        private bool powered = true, ready, guardOpen, guardValid, homeValid, homed, calibrated, auto = true;
        private bool transitioned, loadPending, loadSeen, guardTrip, resetAfterGuard, contactRecorded, cyclePassed;
        private string wiring, fault;
        private Vector3[] markOrigins;

        private bool Running => mode is Mode.Calibrating or Mode.Manual or Mode.Auto;
        private bool Active => Running || mode == Mode.Homing;
        private bool Still => velocity < .0005f && Mathf.Abs(carriageVelocity) < .0005f;
        private bool CanHome => ready && guardValid && Still && mode == Mode.Off;
        private bool CanCalibrate => CanHome && homed && homeValid;
        private bool CanStart => CanCalibrate && (!auto || calibrated);
        private bool CanReset => mode == Mode.Fault && ready && guardValid && Still && stroke <= 0;
        private float Rpm => velocity * 60 / (2 * Mathf.PI * Radius);
        private float EncoderPosition => count * Unit + travel - edge * Unit;

        private void Update()
        {
            if (loader == null) loader = FindFirstObjectByType<SparkLevelLayoutLoader>();
            if (loader == null || loader.ActiveLayout != layout) return;
            if (manager == null) manager = FindFirstObjectByType<LevelGamePlayManager>();
            if (manager == null) return;
            circuit = manager.CircuitSystem;
            solver = manager.ElectricalSolver;
            if (circuit == null || solver == null) return;
            if (bindings != loader.ActiveBindings)
            {
                bindings = loader.ActiveBindings;
                if (bindings == null) return;
                topology = -1;
                guardContact.SetState(SparkSwitchState.Closed);
                homeContact.SetState(SparkSwitchState.Open);
            }
            if (topology != circuit.TopologyVersion)
            {
                topology = circuit.TopologyVersion;
                wiring = CheckWiring();
            }
            ReadElectrical();
            if (stage == 0 && ready && guardValid) Award();
            if (Active && (!guardValid || !ready))
                Trip(!guardValid ? "GUARD INTERLOCK OPEN OR SIGNAL LOST" : wiring ?? "SUPPLY FAULT", !guardValid);
            accumulator += Mathf.Min(Time.unscaledDeltaTime,.15f);
            while (accumulator >= Tick)
            {
                accumulator -= Tick;
                Step();
            }
            Present();
        }

        public void MainPower()
        {
            if (bindings == null) return;
            powered = !powered;
            if (!powered && Active) Trip("MAIN POWER LOST");
            source.SetOutput(powered,default);
        }

        public void HomeCarriage()
        {
            if (!CanHome) return;
            homed = false;
            mode = Mode.Homing;
            Drive(printDrive,true);
        }

        public void Calibrate()
        {
            if (!CanCalibrate) return;
            calibrated = false;
            Begin(Mode.Calibrating);
        }

        public void ToggleAuto() { if (mode == Mode.Off) auto = !auto; }
        public void Speed30() { if (mode == Mode.Off && stage < 3) requestedRpm = 30; }
        public void Speed60()
        {
            if (stage != 3 || mode != Mode.Auto || Mathf.Abs(Rpm-30f) > 1f) return;
            transitioned = true;
            requestedRpm = 60;
        }

        public void StartWeb() { if (CanStart) Begin(auto ? Mode.Auto : Mode.Manual); }

        private void Begin(Mode next)
        {
            mode = next;
            cycle = Cycle.Ready;
            lastMark = -1;
            target = -1;
            consecutive = 0;
            Drive(transportDrive,true);
        }

        public void StopWeb()
        {
            if (!Active) return;
            if (mode == Mode.Homing || cycle != Cycle.Ready) homed = false;
            mode = Mode.Off;
            StopDrives();
        }

        public void TestLoad()
        {
            if (stage != 4 || mode != Mode.Auto || loadPending || loadSeen || loadTime > 0 || cycle != Cycle.Ready || Rpm < 58) return;
            loadPending = true;
            loadBaseline = velocity;
            loadStartCount = count;
            trajectoryChange = 0;
            consecutive = 0;
        }

        public void ToggleGuard()
        {
            guardOpen = !guardOpen;
            Drive(guardContact,!guardOpen);
            if (guardOpen && Active) Trip("GUARD OPEN",true);
        }

        public void ResetFault()
        {
            if (!CanReset) return;
            resetAfterGuard = guardTrip;
            fault = null;
            mode = Mode.Off;
        }

        private static void Drive(SparkSwitch contact,bool close)
        {
            if (contact.IsClosed != close) contact.SetState(close ? SparkSwitchState.Closed : SparkSwitchState.Open);
        }

        private void StopDrives()
        {
            Drive(transportDrive,false);
            Drive(printDrive,false);
            cycle = Cycle.Ready;
            target = -1;
            loadTime = 0;
            loadPending = false;
        }

        private void Trip(string reason,bool guardFault = false)
        {
            if (mode == Mode.Fault) return;
            guardTrip |= guardFault && stage == 5 && mode == Mode.Auto;
            resetAfterGuard = false;
            fault = reason;
            mode = Mode.Fault;
            homed = false;
            StopDrives();
        }

        private void ReadElectrical()
        {
            var result = solver.LatestSolveResult;
            bool High(int plus,int minus) => result != null && result.TryGetVoltage(pins[plus],pins[minus],out float v) && v is >=4.8f and <=5.2f;
            ready = powered && wiring == null && source.IsOutputActive && !source.IsOverloaded && High(1,3) && High(4,5) && High(20,21);
            guardValid = !guardOpen && guardContact.IsClosed && Routed(22,23) && High(23,3);
            homeValid = homeContact.IsClosed && Routed(24,25) && High(25,3);
        }

        private SparkTerminal Root(SparkTerminal pin)
        {
            while (nets.TryGetValue(pin,out var parent)) pin = parent;
            return pin;
        }

        private bool Routed(int a,int b) => Root(pins[a]) == Root(pins[b]);

        private string CheckWiring()
        {
            circuit.CopyConnections(connections);
            nets.Clear();
            foreach (var wire in connections)
            {
                if (wire.Kind != SparkConnectionKind.Wire) continue;
                var a = Root(wire.A);
                var b = Root(wire.B);
                if (a != b) nets[b] = a;
            }
            for (int i = 0; i < Wires.Length; i++)
            {
                var (a,b) = Wires[i];
                if (!connections.Exists(w => w.Kind == SparkConnectionKind.Wire &&
                    (w.A == pins[a] && w.B == pins[b] || w.A == pins[b] && w.B == pins[a])))
                    return $"Connect W{i+1:00} · {pins[a].name} → {pins[b].name}.";
            }
            var distinct = new HashSet<SparkTerminal>();
            foreach (int i in new[] {0,2,6,8,10,12,14,16,18,22,24})
                if (!distinct.Add(Root(pins[i]))) return "Remove the bridge between separate wire nets.";
            return null;
        }

        private void Step()
        {
            bool webDrive = Running && transportDrive.IsClosed && Mathf.Abs(transportMotor.ElectricalState.Current) > .015f;
            bool railDrive = printDrive.IsClosed && Routed(12,13) && Mathf.Abs(printMotor.ElectricalState.Current) > .015f;
            driveWait = (Running && !webDrive || (mode == Mode.Homing || cycle != Cycle.Ready) && !railDrive) ? driveWait + Tick : 0;
            if (driveWait > .30f || transportMotor.IsOverCurrent || printMotor.IsOverCurrent)
                Trip("DRIVE CURRENT FAULT");

            float demand = (mode == Mode.Calibrating ? 18 : requestedRpm) * 2 * Mathf.PI * Radius / 60;
            if (loadPending && target >= 0 && EncoderPosition >= target-.070f)
            {
                loadPending = false;
                loadTime = 1.1f;
            }
            if (loadTime > 0)
            {
                loadTime = Mathf.Max(0,loadTime-Tick);
                demand *= .66f;
                if (loadBaseline-velocity > .023f && count-loadStartCount >= 5) loadSeen = true;
            }
            velocity = Mathf.MoveTowards(velocity,webDrive ? demand : 0,(webDrive ? .22f : .43f)*Tick);
            travel += velocity * Tick;
            ReadEncoder();
            markPulse = Mathf.Max(0,markPulse-Tick);
            ReadMarks();

            if (mode == Mode.Homing)
            {
                MoveRail(position > .00035f && railDrive ? -.042f : 0);
                if (homeValid && Mathf.Abs(carriageVelocity)<.0005f)
                {
                    homed = true;
                    mode = Mode.Off;
                    Drive(printDrive,false);
                }
            }
            else if (mode == Mode.Auto) AdvanceCycle(railDrive);
            else
            {
                MoveRail(0);
                stroke = Mathf.MoveTowards(stroke,0,14*Tick);
            }
            Drive(homeContact, position <= .0006f);
        }

        private void ReadEncoder()
        {
            int next = Mathf.FloorToInt(travel/Unit);
            while (edge != next)
            {
                edge += edge < next ? 1 : -1;
                int signal = Gray[edge & 3];
                if (ready)
                {
                    int received = (Routed(6,7) ? signal & 2 : 0) | (Routed(8,9) ? signal & 1 : 0);
                    int delta = Decode[phase*4+received];
                    count += delta;
                    if (Running && delta != 1) Trip("ENCODER DIRECTION OR PHASE INVALID");
                }
                phase = signal;
            }
        }

        private void ReadMarks()
        {
            while (travel >= nextMark*Unit)
            {
                float crossing = nextMark*Unit;
                nextMark += Pitch;
                if (!Running || !Routed(10,11)) continue;
                int measured = Mathf.RoundToInt((EncoderPosition-(travel-crossing))/Unit);
                if (lastMark >= 0)
                {
                    pitch = measured-lastMark;
                    if (Mathf.Abs(pitch-Pitch)>3) { Trip("INVALID MARK SPACING"); return; }
                }
                bool secondMark = lastMark >= 0;
                lastMark = measured;
                marks++;
                markPulse = .18f;
                if (mode == Mode.Calibrating && secondMark)
                {
                    calibrated = true;
                    if (stage == 1) Award();
                    StopWeb();
                }
                else if (mode == Mode.Auto)
                {
                    if (cycle != Cycle.Ready || !homeValid || target >= 0) { Trip("CARRIAGE NOT HOME AT NEXT MARK"); return; }
                    target = measured * Unit + Offset;
                }
            }
        }

        private void MoveRail(float demand)
        {
            carriageVelocity = Mathf.MoveTowards(carriageVelocity,Mathf.Clamp(demand,-.23f,.23f),RailAcceleration*Tick);
            position += carriageVelocity*Tick;
            if (position <= 0) { position = 0; carriageVelocity = 0; }
            if (position > RailLimit) { position = RailLimit; carriageVelocity = 0; Trip("RAIL END STOP"); }
        }

        private void AdvanceCycle(bool railDrive)
        {
            if (cycle == Cycle.Ready)
            {
                if (target >= 0 && EncoderPosition >= target-.034f)
                {
                    cycle = Cycle.Approach;
                    contactRecorded = cyclePassed = false;
                    peakSyncError = 0;
                    Drive(printDrive,true);
                }
                return;
            }
            if (!railDrive) { MoveRail(0); return; }
            if (cycle is Cycle.Approach or Cycle.Tracking)
            {
                float goal = EncoderPosition-target+TrackingTravel*.5f;
                float command = Mathf.Clamp(velocity+(goal-position)*32f,0,.22f);
                if (stage == 4 && loadSeen && loadTime > 0 && cycle == Cycle.Tracking)
                    trajectoryChange = Mathf.Max(trajectoryChange,Mathf.Abs(command-loadBaseline));
                float previousPosition = position;
                MoveRail(command);
                float mismatch = Mathf.Abs(carriageVelocity-velocity);
                bool matched = mismatch <= SyncTolerance && Mathf.Abs(position-goal) <= .0015f;
                if (cycle == Cycle.Approach && goal >= .002f && matched && Routed(18,19))
                {
                    cycle = Cycle.Tracking;
                    cycleRpm = Rpm;
                }
                if (cycle == Cycle.Approach && goal > .009f) { RejectCycle(); return; }
                if (cycle != Cycle.Tracking) return;
                peakSyncError = Mathf.Max(peakSyncError,mismatch);
                if (!matched) { RejectCycle(); return; }
                stroke = Mathf.Clamp01(1f-Mathf.Abs(goal-TrackingTravel*.5f)/(TrackingTravel*.5f));
                if (!contactRecorded && goal >= TrackingTravel*.5f)
                {
                    float lastGoal = goal-velocity*Tick;
                    float fraction = Mathf.Clamp01((TrackingTravel*.5f-lastGoal)/(goal-lastGoal));
                    float contact = Mathf.Lerp(previousPosition,position,fraction);
                    cutExpected = target;
                    cutActual = target+TrackingTravel*.5f-contact;
                    error = (cutActual-cutExpected)*1000;
                    cyclePassed = Mathf.Abs(error) <= 2 && peakSyncError <= SyncTolerance;
                    if (!cyclePassed) { RejectCycle(); return; }
                    contactRecorded = true;
                }
                if (position >= TrackingTravel+.0015f) { cycle = Cycle.Brake; stroke = 0; }
                return;
            }
            if (cycle == Cycle.Brake)
            {
                MoveRail(0);
                stroke = Mathf.MoveTowards(stroke,0,14*Tick);
                if (carriageVelocity <= 0) cycle = Cycle.Return;
                return;
            }
            MoveRail(position > .00035f ? -Mathf.Min(.18f,Mathf.Sqrt(2*RailAcceleration*position)) : 0);
            if (homeValid && Mathf.Abs(carriageVelocity)<.0005f)
            {
                cycle = Cycle.Ready;
                target = -1;
                Drive(printDrive,false);
                if (cyclePassed) RecordCut();
            }
        }

        private void RejectCycle()
        {
            rejected++;
            consecutive = 0;
            cyclePassed = false;
            cycle = Cycle.Brake;
        }

        private void RecordCut()
        {
            accepted++;
            if (stage == 2 && Mathf.Abs(cycleRpm-30)<1.5f && ++consecutive >= 3) Award();
            else if (stage == 3 && transitioned && Mathf.Abs(cycleRpm-60)<1.5f && ++consecutive >= 3) Award();
            else if (stage == 4 && loadSeen && loadTime <= 0 && trajectoryChange > .018f &&
                Mathf.Abs(cycleRpm-60)<1.5f && ++consecutive >= 2) Award();
            else if (stage == 5 && resetAfterGuard && homed && homeValid && guardValid) Award();
        }

        private void Award()
        {
            if (stage >= Gates.Length || !bindings.SetRuntimeComponentBinding(Gates[stage],source)) return;
            stage++;
            consecutive = 0;
            manager.QueueEvaluation();
        }

        private string Prompt()
        {
            if (mode == Mode.Fault) return $"SAFE STOP · {fault}  |  " + (CanReset ? "RESET, then HOME and START." : guardOpen ? "Close the guard." : wiring ?? "Waiting for safe feedback.");
            if (wiring != null) return wiring;
            if (!powered) return "Turn on MAIN POWER.";
            if (!guardValid) return "Close the guard and check W14.";
            if (mode == Mode.Homing) return "HOMING · Seeking the carriage sensor.";
            if (!homed) return "Press HOME to establish carriage position.";
            if (mode == Mode.Calibrating) return "CALIBRATING · Measuring two genuine marks.";
            return stage switch
            {
                1 => "Press CALIBRATE to measure the registration pitch.",
                2 => "Select AUTO and START. Verify three cuts at 30 RPM.",
                3 => "Select 60 RPM while running. Verify three more cuts.",
                4 when loadPending => "SPEED TEST ARMED · Waiting for the next mark.",
                4 when loadTime > 0 => "WEB SPEED DIP · Tracking the slower web.",
                4 when loadSeen => "Verify two accurate cuts after the web recovers.",
                4 => "Press SPEED TEST between cuts. Verify two recovered cuts.",
                5 when resetAfterGuard => "START and verify an accurate cut after reset.",
                5 => "Open the guard while running. Check safe stop, then close, RESET, HOME and START.",
                _ => "Cut registration and guard recovery verified."
            };
        }

        private void Present()
        {
            string guard = guardValid ? "CLOSED" : "OPEN / LOST";
            string state = mode == Mode.Auto ? cycle.ToString().ToUpperInvariant() : mode.ToString().ToUpperInvariant();
            statusText.text = Prompt();
            meterText.text = $"WEB {Rpm:F1} RPM / {velocity*1000:F0} mm/s   |   CARR {carriageVelocity*1000:F0} mm/s @ {position*1000:F1} mm   |   ΔV {(carriageVelocity-velocity)*1000:F0} mm/s";
            detailText.text = $"{state}   |   ENC {count} / MARK {marks}   |   GUARD {guard}   |   HOME {(homeValid ? "ON" : "OFF")}   |   CUT {error:+0.00;-0.00;0.00} mm   |   {manager.SatisfiedTargetCount}/6";
            controllerDisplay.text = $"{Rpm:F1} RPM  /  {count} CT\n{state}  •  GUARD {guard}";
            qualityDisplay.text = $"CUT {error:+0.00;-0.00;0.00} mm   P{accepted} / R{rejected}\nSET {cutExpected*1000:F1} / ACT {cutActual*1000:F1} mm";
            rig.ApplyState(travel*1000,position*1000,carriageVelocity*60/.008f,stroke,guardOpen,ready && guardValid,cycle == Cycle.Ready && homeValid);
            markOrigins ??= System.Array.ConvertAll(webSlats,m => m.localPosition);
            const float eyeX=.41f, visualScale=2.45f, spacing=Pitch*Unit*visualScale;
            float first=eyeX-travel*visualScale+Mathf.Ceil((.80f-eyeX+travel*visualScale)/spacing)*spacing-spacing;
            for (int i=0;i<webSlats.Length;i++)
            {
                var p=markOrigins[i];
                p.x=first-i*spacing;
                bool show=p.x>-.80f;
                if (webSlats[i].gameObject.activeSelf!=show) webSlats[i].gameObject.SetActive(show);
                if (show) webSlats[i].localPosition=p;
            }
            bool[] lights={powered,markPulse>0,(phase&2)!=0,(phase&1)!=0,homeValid,stroke>0,guardValid,accepted>0 && cyclePassed,rejected>0 || mode==Mode.Fault};
            for (int i=0;i<lamps.Length;i++) if (lamps[i].activeSelf!=lights[i]) lamps[i].SetActive(lights[i]);
            powerButton.interactable = guardButton.interactable = true;
            guardButton.GetComponentInChildren<Text>().text = guardOpen ? "CLOSE GUARD" : "OPEN GUARD";
            manualButton.interactable = mode == Mode.Off;
            manualButton.GetComponentInChildren<Text>().text = auto ? "MODE: AUTO" : "MODE: MANUAL";
            homeButton.interactable = CanHome;
            calibrateButton.interactable = CanCalibrate;
            startButton.interactable = CanStart;
            stopButton.interactable = Active;
            slowButton.interactable = requestedRpm != 30 && stage < 3 && mode == Mode.Off;
            fastButton.interactable = stage == 3 && requestedRpm == 30 && mode == Mode.Auto && Mathf.Abs(Rpm-30)<1;
            loadButton.interactable = stage == 4 && !loadPending && !loadSeen && loadTime <= 0 && cycle == Cycle.Ready && mode == Mode.Auto && Rpm>=58;
            resetButton.interactable = CanReset;
        }

        private void OnDisable()
        {
            if (gameObject.activeInHierarchy && transportDrive != null && printDrive != null) StopDrives();
            if (bindings != null) foreach (var gate in Gates) bindings.ClearRuntimeComponentBinding(gate);
            bindings = null;
        }
    }
}
