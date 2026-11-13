using System.Collections.Generic;
using ProjectSpark.Circuit;
using ProjectSpark.Electrical;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.Gameplay
{
    [DisallowMultipleComponent]
    public sealed class SparkLevel48Registration : MonoBehaviour
    {
        public static readonly string[] Gates = {
            "L48_Circuit", "L48_Calibration", "L48_Thirty", "L48_Sixty", "L48_Glare", "L48_Recovery"
        };
        private static readonly int[] Gray = {0, 2, 3, 1};
        private static readonly int[] Decode = {0,-1,1,0, 1,0,0,-1, -1,0,0,1, 0,1,-1,0};
        private const int CountsPerRevolution = 80, MarkPitch = 160;
        private const float Radius = .020f, DistancePerCount = 2f * Mathf.PI * Radius / CountsPerRevolution;
        private const float Tick = .005f, MaxError = .002f, StrokeTime = .075f, ReturnTime = .095f;
        private const float EyeX = -.31f, HeadX = .16f;
        private enum Mode { Off, Calibrating, Manual, Auto, Fault }

        public SparkLevelLayout layout;
        public SparkPowerSupply source;
        public SparkMotor transportMotor, printMotor;
        public SparkSwitch transportDrive, printDrive;
        public SparkTerminal[] pins;
        public TMP_Text controllerDisplay, qualityDisplay;
        public Transform roller, encoder, shaft, head;
        public Transform[] webSlats;
        public GameObject[] lamps;
        public Text statusText, meterText, detailText;
        public Button powerButton, calibrateButton, manualButton, startButton, stopButton, resetButton;
        public Button slowButton, fastButton, glareButton, coverButton, restartButton;

        private readonly List<SparkCircuitConnection> wires = new();
        private readonly Dictionary<SparkTerminal, SparkTerminal> parents = new();
        private readonly HashSet<SparkTerminal> distinctNets = new();
        // Pins are ordered by first appearance in W01–W13. The first four power
        // connections and the final two share only the two supply sockets.
        private static readonly (int from, int to)[] RequiredWires = {
            (0,1),(2,3),(0,4),(2,5),(6,7),(8,9),(10,11),(12,13),(14,15),(16,17),(18,19),(0,20),(2,21)
        };
        private SparkLevelLayoutLoader loader;
        private LevelGamePlayManager manager;
        private SparkLevelSceneBindings bindings;
        private SparkCircuitSystem circuit;
        private SparkElectricalSolver solver;
        private int topology = -1, stage, physicalEdge, count, nextMark, lastMark, spacing, phase, requestedRpm = 30;
        private int marks, accepted, rejected, consecutive, qualifying, falsePulses, printsAfterGlare, printsAfterReset;
        private float accumulator, travel, velocity, target, actuator, errorMm, markLamp, motorAngle;
        private float expectedPrint, actualPrint, detectedMarkPosition, driveWait;
        private bool powered = true, ready, auto = true, calibrated, covered, glarePending, glareRejected, transitioned;
        private bool missingTest, recoveryReset, stampRecorded, lastAccepted;
        private string wiring, fault;
        private Mode mode;
        private Vector3 headOrigin;
        private Vector3[] slatOrigins;

        private bool Running => mode is Mode.Calibrating or Mode.Manual or Mode.Auto;
        private bool Home => actuator <= 0f;
        private bool Stopped => velocity < .0005f;
        private bool CanStart => ready && Stopped && mode == Mode.Off && (!auto || calibrated);
        private bool CanReset => ready && Stopped && Home && !covered && mode == Mode.Fault;
        private float Rpm => velocity * 60f / (2f * Mathf.PI * Radius);
        private float EncoderPosition => count * DistancePerCount + travel - physicalEdge * DistancePerCount;

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
                Restart();
            }

            if (topology != circuit.TopologyVersion)
            {
                topology = circuit.TopologyVersion;
                wiring = CheckWiring();
            }
            ready = powered && wiring == null && ReadPower();
            if (stage == 0 && ready) Award();
            if (Running && !ready) Trip(wiring ?? "SUPPLY OR DRIVER FAULT");
            accumulator += Mathf.Min(Time.unscaledDeltaTime, .15f);
            while (accumulator >= Tick)
            {
                accumulator -= Tick;
                Simulate();
            }
            Present();
        }

        public void MainPower()
        {
            if (bindings == null) return;
            powered = !powered;
            if (!powered) StopWeb();
            source.SetOutput(powered, default);
        }

        public void ToggleAuto() { if (!Running && mode != Mode.Fault) auto = !auto; }
        public void Speed30() { if (stage < 3 || !Running) requestedRpm = 30; }
        public void Speed60()
        {
            if (stage < 3) return;
            transitioned |= Running && Mathf.Abs(Rpm - 30f) < 2f && requestedRpm == 30;
            requestedRpm = 60;
        }

        public void Calibrate()
        {
            if (!ready || Running || !Stopped || covered || mode == Mode.Fault) return;
            calibrated = false;
            Begin(Mode.Calibrating);
        }

        public void StartWeb()
        {
            if (CanStart) Begin(auto ? Mode.Auto : Mode.Manual);
        }

        private void Begin(Mode next)
        {
            lastMark = -1;
            target = -1;
            qualifying = 0;
            mode = next;
            transportDrive.SetState(SparkSwitchState.Closed);
        }

        public void StopWeb()
        {
            if (mode != Mode.Fault) mode = Mode.Off;
            StopDrive();
        }

        public void TestGlare()
        {
            if (stage == 4 && mode == Mode.Auto && !covered && !glareRejected) glarePending = true;
        }

        public void ToggleCover()
        {
            if (covered || stage == 5 && mode == Mode.Auto) covered = !covered;
        }

        public void ResetFault()
        {
            if (!CanReset) return;
            recoveryReset = missingTest;
            printsAfterReset = 0;
            fault = null;
            mode = Mode.Off;
        }

        public void Restart()
        {
            if (bindings == null) return;
            StopDrive();
            foreach (var gate in Gates) bindings.ClearRuntimeComponentBinding(gate);
            topology = -1;
            stage = physicalEdge = count = spacing = phase = marks = accepted = rejected = consecutive = qualifying = 0;
            falsePulses = printsAfterGlare = printsAfterReset = 0;
            nextMark = MarkPitch;
            lastMark = -1;
            travel = velocity = actuator = errorMm = markLamp = motorAngle = accumulator = 0;
            detectedMarkPosition = expectedPrint = actualPrint = driveWait = 0;
            target = -1;
            powered = auto = true;
            covered = calibrated = glarePending = glareRejected = transitioned = missingTest = recoveryReset = lastAccepted = false;
            requestedRpm = 30;
            fault = null;
            mode = Mode.Off;
            source.SetOutput(true, default);
            manager.QueueEvaluation();
        }

        private bool ReadPower()
        {
            var result = solver.LatestSolveResult;
            if (result == null || !source.IsOutputActive || source.IsOverloaded) return false;
            bool InRange(int plus, int minus) =>
                result.TryGetVoltage(pins[plus], pins[minus], out float volts) && volts >= 4.8f && volts <= 5.2f;
            return InRange(1,3) && InRange(4,5) && InRange(20,21);
        }

        private SparkTerminal Root(SparkTerminal pin)
        {
            while (parents.TryGetValue(pin, out var parent)) pin = parent;
            return pin;
        }

        private bool Routed(int output, int input) => Root(pins[output]) == Root(pins[input]);

        private string CheckWiring()
        {
            circuit.CopyConnections(wires);
            parents.Clear();
            foreach (var wire in wires)
            {
                if (wire.Kind != SparkConnectionKind.Wire) continue;
                var a = Root(wire.A);
                var b = Root(wire.B);
                if (a != b) parents[b] = a;
            }
            for (int i = 0; i < RequiredWires.Length; i++)
            {
                var (from,to) = RequiredWires[i];
                if (!wires.Exists(w => w.Kind == SparkConnectionKind.Wire &&
                    (w.A == pins[from] && w.B == pins[to] || w.B == pins[from] && w.A == pins[to])))
                    return $"Connect W{i + 1:00} · {pins[from].name} → {pins[to].name}.";
            }
            distinctNets.Clear();
            foreach (var pin in new[] {0,2,6,8,10,12,14,16,18})
                if (!distinctNets.Add(Root(pins[pin]))) return "Remove the bridge between separate power or signal nets.";
            return null;
        }

        private void Simulate()
        {
            float desired = (mode == Mode.Calibrating ? 18 : requestedRpm) * 2f * Mathf.PI * Radius / 60f;
            float current = Mathf.Abs(transportMotor.ElectricalState.Current);
            bool drive = Running && transportDrive.IsClosed && current > .015f;
            driveWait = Running && !drive ? driveWait + Tick : 0f;
            if (driveWait > .25f) Trip("TRANSPORT DRIVE FAILED");
            // Average motor torque drives the roller through the geared feed.
            // The speed governor limits requested torque near the setpoint.
            float acceleration = drive
                ? Mathf.Clamp((desired - velocity) * 5f, -.22f, current * 1.12f)
                : mode == Mode.Fault ? -.35f : -.22f;
            float previous = travel;
            velocity = Mathf.Max(0f, velocity + acceleration * Tick);
            travel += velocity * Tick;
            ReadEncoder();
            markLamp = Mathf.Max(0f, markLamp - Tick);
            if (Running && (transportMotor.IsOverCurrent || printMotor.IsOverCurrent)) Trip("MOTOR OVERCURRENT");
            if (Running)
            {
                // The eye sees a mark when the material reaches its physical position.
                // The independently decoded encoder supplies the measurement at that event.
                while (travel >= nextMark * DistancePerCount)
                {
                    float crossing = nextMark * DistancePerCount;
                    nextMark += MarkPitch;
                    if (!covered && Routed(10, 11))
                        ObserveMark(Mathf.RoundToInt((EncoderPosition - (travel - crossing)) / DistancePerCount));
                }
                if (glarePending && lastMark >= 0 && count - lastMark is > 44 and < 76)
                {
                    glarePending = false;
                    ObserveMark(count);
                }
                if (lastMark >= 0 && count - lastMark > MarkPitch + 8)
                {
                    missingTest = covered && stage == 5;
                    Trip("REGISTRATION MARK MISSING");
                }
            }
            if (mode == Mode.Auto && target >= 0 && Home && velocity > .02f && !covered && Routed(12, 13) && Routed(18, 19) &&
                EncoderPosition + velocity * (StrokeTime + Tick * .5f) >= target)
            {
                printDrive.SetState(SparkSwitchState.Closed);
                actuator = float.Epsilon;
                stampRecorded = false;
            }
            else MoveHead(previous);
        }

        private void ReadEncoder()
        {
            int edge = Mathf.FloorToInt(travel / DistancePerCount);
            while (physicalEdge != edge)
            {
                physicalEdge += physicalEdge < edge ? 1 : -1;
                int next = Gray[physicalEdge & 3];
                if (ready)
                {
                    int received = (Routed(6, 7) ? next & 2 : 0) | (Routed(8, 9) ? next & 1 : 0);
                    int delta = Decode[phase * 4 + received];
                    count += delta;
                    if (Running && delta != 1) Trip("INVALID OR REVERSED ENCODER");
                }
                phase = next;
            }
        }

        private void ObserveMark(int position)
        {
            int interval = lastMark < 0 ? 0 : position - lastMark;
            if (interval != 0 && Mathf.Abs(interval - MarkPitch) > 4)
            {
                if (mode == Mode.Calibrating) { lastMark = -1; return; }
                if (interval < MarkPitch && stage == 4)
                {
                    falsePulses++;
                    glareRejected = true;
                }
                return;
            }
            lastMark = position;
            marks++;
            markLamp = .22f;
            if (interval != 0) spacing = interval;
            if (mode == Mode.Calibrating && interval != 0)
            {
                calibrated = true;
                if (stage == 1) Award();
                StopWeb();
            }
            if (mode != Mode.Auto || !calibrated) return;
            if (target >= 0 || !Home) { Trip("PRINT HEAD NOT HOME AT REGISTRATION"); return; }
            detectedMarkPosition = position * DistancePerCount;
            target = detectedMarkPosition + CountsPerRevolution * DistancePerCount;
        }

        private void MoveHead(float previousTravel)
        {
            if (Home) return;
            if (stampRecorded)
            {
                actuator = Mathf.MoveTowards(actuator, 0f, Tick * StrokeTime / ReturnTime);
                return;
            }
            if (!printDrive.IsClosed || Mathf.Abs(printMotor.ElectricalState.Current) < .015f)
            {
                Trip("PRINT MOTOR DRIVE FAILED");
                return;
            }
            motorAngle += 720f * Tick;
            float old = actuator;
            actuator = Mathf.Min(StrokeTime, actuator + Tick);
            if (actuator < StrokeTime) return;
            float fraction = Mathf.Clamp01((StrokeTime - old) / Tick);
            expectedPrint = target;
            actualPrint = Mathf.Lerp(previousTravel, travel, fraction);
            errorMm = (actualPrint - expectedPrint) * 1000f;
            lastAccepted = Mathf.Abs(actualPrint - expectedPrint) <= MaxError;
            if (lastAccepted) { accepted++; consecutive++; }
            else { rejected++; consecutive = qualifying = 0; }
            stampRecorded = true;
            target = -1;
            printDrive.SetState(SparkSwitchState.Open);
            if (!lastAccepted) return;
            if (stage == 2 && Mathf.Abs(Rpm - 30) < 2f && ++qualifying >= 3) Award();
            if (stage == 3 && transitioned && Mathf.Abs(Rpm - 60) < 2f && ++qualifying >= 3) Award();
            if (stage == 4 && glareRejected && ++printsAfterGlare >= 1) Award();
            if (stage == 5 && recoveryReset && !covered && ready && ++printsAfterReset >= 1) Award();
        }

        private void StopDrive()
        {
            target = -1;
            if (!Home) stampRecorded = true;
            glarePending = false;
            if (transportDrive.IsClosed) transportDrive.SetState(SparkSwitchState.Open);
            if (printDrive.IsClosed) printDrive.SetState(SparkSwitchState.Open);
        }

        private void Trip(string reason)
        {
            if (mode == Mode.Fault) return;
            mode = Mode.Fault;
            fault = reason;
            StopDrive();
            Debug.Log($"[LEVEL 48] SAFE STOP · {reason}", this);
        }

        private void Award()
        {
            if (stage >= Gates.Length || !bindings.SetRuntimeComponentBinding(Gates[stage], source)) return;
            stage++;
            qualifying = 0;
            manager.QueueEvaluation();
        }

        private string Prompt()
        {
            if (mode == Mode.Fault)
                return $"SAFE STOP · {fault}  |  " + (covered ? "Restore the optical eye." : CanReset ? "RESET FAULT, then START." : wiring ?? "Waiting for safe feedback.");
            if (!powered) return "Turn on MAIN POWER.";
            if (!ready) return wiring ?? "Waiting for solved 5 V at controller, eye and driver.";
            if (mode == Mode.Calibrating) return "CALIBRATING · Observe two genuine marks 160 counts apart.";
            return stage switch
            {
                1 => "CALIBRATE the optical eye.",
                2 => "Select AUTO and START at 30 RPM. Verify three accurate prints.",
                3 => "While the web is running, select 60 RPM and verify three more prints.",
                4 => "Select GLARE TEST. Reject the false pulse and verify the next genuine print.",
                5 when recoveryReset => "START and verify the first recovered print.",
                5 when covered => "Optical eye covered. Waiting for the missing mark.",
                5 => "Select MISSING MARK while running. Restore the eye after the safe stop.",
                _ => "Print registration and safe recovery verified."
            };
        }

        private void Present()
        {
            string sensor = covered ? "COVERED" : ready ? "ONLINE" : "OFFLINE";
            string print = Home ? "HOME" : stampRecorded ? "RETURN" : "PRINTING";
            string next = target < 0 ? "—" : $"{target / DistancePerCount:F0} ct";
            statusText.text = Prompt();
            meterText.text = $"WEB {Rpm:F1} RPM   |   ENCODER {count} ct   |   EYE {sensor}   |   MARKS {marks}   |   PITCH {spacing} ct";
            detailText.text = $"{mode.ToString().ToUpperInvariant()}   |   {print}   |   NEXT {next}   |   ERR {errorMm:+0.00;-0.00;0.00} mm   |   PASS {accepted} / REJECT {rejected}   |   {manager.SatisfiedTargetCount}/6";
            controllerDisplay.text = $"{Rpm:F1} RPM   /   {count} CT\nEYE {sensor}  •  HEAD {print}";
            qualityDisplay.text = $"REG {errorMm:+0.00;-0.00;0.00} mm\nPASS {accepted}  /  REJECT {rejected}";
            if (slatOrigins == null)
            {
                headOrigin = head.localPosition;
                slatOrigins = System.Array.ConvertAll(webSlats, slat => slat.localPosition);
            }
            float rotation = travel / Radius * Mathf.Rad2Deg;
            roller.localRotation = Quaternion.AngleAxis(-rotation, Vector3.forward);
            encoder.localRotation = Quaternion.AngleAxis(-rotation, Vector3.forward);
            shaft.localRotation = Quaternion.AngleAxis(motorAngle, Vector3.right);
            head.localPosition = headOrigin - Vector3.up * (.035f * actuator / StrokeTime);
            const float visibleMin = -.69f, visibleMax = .70f;
            const float visualScale = (HeadX - EyeX) / (CountsPerRevolution * DistancePerCount);
            const float visualPitch = MarkPitch * DistancePerCount * visualScale;
            int firstMark = Mathf.CeilToInt((EyeX + travel * visualScale - visibleMax) / visualPitch);
            for (int i = 0; i < webSlats.Length; i++)
            {
                var p = slatOrigins[i];
                p.x = EyeX + travel * visualScale - (firstMark + i) * visualPitch;
                bool visible = p.x >= visibleMin;
                if (webSlats[i].gameObject.activeSelf != visible) webSlats[i].gameObject.SetActive(visible);
                if (visible) webSlats[i].localPosition = p;
            }
            bool[] lit = {powered, ready && !covered, markLamp > 0, (phase & 2) != 0, (phase & 1) != 0,
                ready && Home, !Home && !stampRecorded, printDrive.IsClosed, accepted > 0 && lastAccepted,
                rejected > 0 && !lastAccepted || mode == Mode.Fault, accepted > 0 && lastAccepted};
            for (int i = 0; i < lamps.Length; i++)
                if (lamps[i].activeSelf != lit[i]) lamps[i].SetActive(lit[i]);
            powerButton.interactable = restartButton.interactable = true;
            calibrateButton.interactable = ready && Stopped && mode == Mode.Off && !covered;
            manualButton.interactable = mode == Mode.Off;
            manualButton.GetComponentInChildren<Text>().text = auto ? "MODE: AUTO" : "MODE: MANUAL";
            startButton.interactable = CanStart;
            stopButton.interactable = Running;
            slowButton.interactable = requestedRpm != 30 && (!Running || stage < 3);
            fastButton.interactable = stage >= 3 && requestedRpm == 30;
            glareButton.interactable = stage == 4 && mode == Mode.Auto && !glarePending && !glareRejected;
            coverButton.interactable = covered || stage == 5 && mode == Mode.Auto;
            coverButton.GetComponentInChildren<Text>().text = covered ? "RESTORE EYE" : "MISSING MARK";
            resetButton.interactable = CanReset;
        }

        private void OnDisable()
        {
            if (gameObject.activeInHierarchy && transportDrive != null && printDrive != null) StopDrive();
            if (bindings != null)
                foreach (var gate in Gates) bindings.ClearRuntimeComponentBinding(gate);
            bindings = null;
        }
    }
}
