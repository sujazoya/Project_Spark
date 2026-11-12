using System;
using System.Collections.Generic;
using ProjectSpark.Circuit;
using ProjectSpark.Electrical;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.Gameplay
{
    [DisallowMultipleComponent]
    public sealed class SparkLevel33JamGuard : MonoBehaviour
    {
        public static readonly string[] Gates = {
            "L33_RunningClear", "L33_BriefIgnored", "L33_JamTrip",
            "L33_ResetDenied", "L33_ClearedNoRestart", "L33_ManualRestart"
        };

        public SparkLevelLayout layout;
        public SparkPowerSupply source;
        public SparkSwitch chargeContact, clearContact, motorContact;
        public SparkResistor chargeResistor, dischargeResistor;
        public SparkCapacitor timerCapacitor;
        public SparkMotor motor;
        public Transform[] beltSlats;
        public Transform shaft, parcel;
        public Light runLight, jamLight;
        public Text statusText, readingsText, jamLabel;
        public Button startButton, briefButton, jamButton, resetButton, retryButton;
        public Transform jamRocker, clearCap, contactArm, detectorBeam, alarmBeacon;
        public Renderer monitorRun, monitorWait, monitorTrip;
        public TextMesh timerReadout;
        public Vector3 blockedParcelPosition = new Vector3(-.03f, .12f, 0);
        public float beltStartX = -.48f, beltLength = .96f;

        private const float TripVoltage = 3f;
        private const float ResetVoltage = 1f;
        private const float IdleVoltage = .25f;
        private const float PulseSeconds = .50f;
        private SparkLevelLayoutLoader loader;
        private LevelGamePlayManager manager;
        private SparkElectricalSolver solver;
        private SparkCircuitSystem circuit;
        private SparkLevelSceneBindings bindings;
        private SparkElectricalSolveResult observed;
        private SparkElectricalNetworkGraph checkedGraph;
        private string wiringProblem;
        private int topology = -1;
        private int stage;
        private bool ready, started, blocked, tripped, pulseActive, pulseFinished, pulseLostDrive, resetDenied, restarted;
        private float voltage, peak, pulseTime, nextSolve, beltOffset;
        private Vector3 clearCapRest;

        private void Awake()
        {
            if (clearCap != null) clearCapRest = clearCap.localPosition;
        }

        public void StartTest()
        {
            if (!CanControl() || stage != 0 || started || voltage >= IdleVoltage) return;
            started = true;
            SetContact(motorContact, true);
        }

        public void BriefBlock()
        {
            if (!CanControl() || stage != 1 || pulseActive || pulseFinished || tripped || voltage >= IdleVoltage) return;
            pulseActive = true;
            pulseTime = peak = 0;
            SetBlocked(true);
        }

        public void ToggleJam()
        {
            if (!CanControl()) return;
            if (stage == 2 && !blocked && !tripped) SetBlocked(true);
            else if (stage == 4 && blocked) SetBlocked(false);
        }

        public void ResetTrip()
        {
            if (!CanControl() || !tripped) return;
            if (stage == 3 && blocked)
            {
                resetDenied = true;
                return;
            }
            if (stage != 5 || blocked || voltage >= ResetVoltage) return;
            tripped = false;
            restarted = true;
            SetContact(motorContact, true);
        }

        public void RestartTrial()
        {
            if (CanControl()) ResetTrial();
        }

        private bool CanControl() => ready && isActiveAndEnabled && loader != null && loader.ActiveLayout == layout &&
            circuit != null && circuit.TopologyVersion == topology && solver != null && !solver.IsSolving && !solver.IsDirty;

        private void Update()
        {
            if (loader == null) loader = FindFirstObjectByType<SparkLevelLayoutLoader>();
            if (layout == null || loader == null || loader.ActiveLayout != layout) return;
            if (manager == null) manager = FindFirstObjectByType<LevelGamePlayManager>();
            if (solver == null) solver = FindFirstObjectByType<SparkElectricalSolver>();
            if (circuit == null) circuit = FindFirstObjectByType<SparkCircuitSystem>();
            if (solver == null || circuit == null) { Show("Waiting for the circuit and solver.", false); return; }
            if (bindings != loader.ActiveBindings || topology != circuit.TopologyVersion)
            {
                ResetTrial();
                bindings = loader.ActiveBindings;
                topology = circuit.TopologyVersion;
                checkedGraph = null;
            }
            if (bindings == null || Array.Exists(Gates, id => !bindings.IsComponentIdDeclared(id)))
            { Fault("Check the six Level 33 objective bindings."); return; }
            if (!Available(source) || !Available(chargeContact) || !Available(clearContact) || !Available(motorContact) ||
                !Available(chargeResistor) || !Available(dischargeResistor) || !Available(timerCapacitor) || !Available(motor) ||
                solver.SolverRegistry == null || !solver.SolverRegistry.TryGetDefinition(timerCapacitor, out _) ||
                !solver.SolverRegistry.TryGetDefinition(motor, out _))
            { Fault("Check the eight bench components and the capacitor and motor solver registrations."); return; }

            if (solver.IsSolving) return;
            var result = solver.LatestSolveResult;
            if (result == null || solver.IsDirty)
            {
                if (!solver.SolveNow()) { Fault("The solver could not solve this circuit. Check the wiring."); return; }
                result = solver.LatestSolveResult;
            }
            if (result == null || solver.IsDirty) return;
            if (!result.Context.SimulateTransientDevices)
            { Fault("Enable Simulate Transient Devices on the active electrical solver."); return; }

            if (!ReferenceEquals(checkedGraph, result.Context.Graph))
            {
                checkedGraph = result.Context.Graph;
                wiringProblem = CheckWiring(checkedGraph);
            }
            if (wiringProblem != null) { Fault(wiringProblem); return; }

            // Advance only when the shared solver has not already accepted a fresh step.
            bool timerChanging = blocked ? !tripped || voltage < source.ElectricalState.Voltage - .05f : voltage > .025f;
            if (ReferenceEquals(observed, result) && (pulseActive || timerChanging) &&
                Time.time >= nextSolve)
            {
                solver.MarkDirty();
                if (!solver.SolveNow()) { Fault("The transient solve failed. Check the circuit and retry."); return; }
                result = solver.LatestSolveResult;
            }
            if (result == null || solver.IsDirty) return;
            bool fresh = !ReferenceEquals(observed, result);
            if (fresh)
            {
                observed = result;
                nextSolve = Time.time + result.TimeStep;
            }

            if (!source.IsOutputActive || source.ElectricalState.Voltage < 4.8f ||
                source.ElectricalState.Voltage > 5.2f || source.IsOverloaded || timerCapacitor.IsOverVoltage || motor.IsOverCurrent)
            { Fault("Check the 5 V supply, capacitor polarity, and motor current."); return; }
            if (!result.TryGetVoltage(timerCapacitor.PositiveTerminal, timerCapacitor.NegativeTerminal, out voltage) ||
                float.IsNaN(voltage) || float.IsInfinity(voltage) || voltage < -.10f)
            { Fault("Check the capacitor polarity and TIMER node."); return; }
            ready = true;

            bool running = motorContact.IsClosed && motor.IsRunning && motor.ElectricalState.Current > .02f;
            bool stopped = motorContact.IsOpen && Mathf.Abs(motor.ElectricalState.Current) < .001f;
            if (blocked && started && !tripped && voltage >= TripVoltage)
            {
                tripped = true;
                SetContact(motorContact, false);
                if (pulseActive) EndPulse(true);
                Show("3.0 V reached. Motor contact opened.", false);
                return;
            }

            if (pulseActive && fresh)
            {
                pulseTime += result.TimeStep;
                peak = Mathf.Max(peak, voltage);
                pulseLostDrive |= !running;
                if (pulseTime >= PulseSeconds - .0001f)
                {
                    EndPulse(false);
                    Show("Brief blockage cleared. Watch TIMER discharge.", running);
                    return;
                }
            }
            Observe(running, stopped);
            Show(Prompt(), running);
            if (running) AnimateBelt(Time.deltaTime);
        }

        private void Observe(bool running, bool stopped)
        {
            bool passed = stage switch {
                0 => started && !blocked && !tripped && voltage < IdleVoltage && running,
                1 => pulseFinished && !pulseLostDrive && peak >= .55f && peak < TripVoltage &&
                    voltage < IdleVoltage && running && !tripped,
                2 => blocked && tripped && stopped,
                3 => blocked && tripped && stopped && resetDenied,
                4 => !blocked && tripped && stopped && voltage < ResetVoltage,
                5 => !blocked && !tripped && restarted && running,
                _ => false
            };
            if (passed && bindings.SetRuntimeComponentBinding(Gates[stage], source))
            {
                stage++;
                QueueEvaluation();
            }
        }

        private void QueueEvaluation()
        {
            if (manager != null && manager.SceneBindings == bindings) manager.QueueEvaluation();
        }

        private string Prompt()
        {
            return stage switch {
                0 => voltage >= IdleVoltage ? "Waiting for TIMER to discharge below 0.25 V." : "Wiring verified. Press START TEST.",
                1 when pulseActive => "Brief blockage. The conveyor should keep running.",
                1 when pulseFinished && (pulseLostDrive || peak < .55f) => "Brief test failed. Check the timer and press RESTART TEST.",
                1 when pulseFinished => $"Peak {peak:0.00} V. Wait for TIMER to discharge.",
                1 => "Press BRIEF BLOCK to test a 0.50 s obstruction.",
                2 => blocked ? "Sustained jam. Watch TIMER climb to 3.0 V." : "Press JAM and leave the obstruction in place.",
                3 => "Press RESET with the obstruction still in place.",
                4 => blocked ? "Reset refused. Press CLEAR JAM to remove the obstruction." : "Wait for TIMER to fall below 1.0 V. The conveyor stays stopped.",
                5 => "Jam clear and TIMER below 1.0 V. Press RESET to restart.",
                _ => "Jam Guard complete. Brief blockage ignored, sustained jam stopped, reset verified."
            };
        }

        private void SetBlocked(bool value)
        {
            blocked = value;
            // Break before make: only one timer path is closed in a settled state.
            if (value) { SetContact(clearContact, false); SetContact(chargeContact, true); }
            else { SetContact(chargeContact, false); SetContact(clearContact, true); }
        }

        private void EndPulse(bool failed)
        {
            pulseActive = false;
            pulseFinished = true;
            pulseLostDrive |= failed;
            SetBlocked(false);
        }

        private void ResetTrial()
        {
            if (bindings != null) foreach (string id in Gates) bindings.ClearRuntimeComponentBinding(id);
            if (stage > 0) QueueEvaluation();
            ready = started = tripped = pulseActive = pulseFinished = pulseLostDrive = resetDenied = restarted = false;
            stage = 0;
            peak = pulseTime = voltage = nextSolve = 0;
            observed = null;
            SetContact(motorContact, false);
            SetBlocked(false);
        }

        private void Fault(string reason)
        {
            if (ready || started || blocked) ResetTrial();
            Show(reason, false);
        }

        private void Show(string message, bool running)
        {
            if (runLight != null) runLight.enabled = running;
            if (jamLight != null) jamLight.enabled = tripped;
            if (parcel != null)
            {
                if (blocked) parcel.localPosition = blockedParcelPosition;
                SetVisible(parcel, blocked);
            }
            SetVisible(detectorBeam, blocked);
            SetVisible(alarmBeacon, tripped);
            if (jamRocker != null && chargeContact != null)
                jamRocker.localRotation = Quaternion.Euler(chargeContact.IsClosed ? 12f : -12f, 0, 0);
            if (clearCap != null && clearContact != null)
                clearCap.localPosition = clearCapRest + Vector3.down * (clearContact.IsClosed ? .009f : 0);
            if (contactArm != null && motorContact != null)
                contactArm.localRotation = Quaternion.Euler(0, 0, motorContact.IsClosed ? 0 : -16f);
            if (monitorRun != null) monitorRun.enabled = running && !blocked;
            if (monitorWait != null) monitorWait.enabled = ready && !tripped && (blocked || !running);
            if (monitorTrip != null) monitorTrip.enabled = tripped;
            if (timerReadout != null)
            {
                timerReadout.text = ready ? $"{voltage:0.00} V" : "— V";
                timerReadout.color = tripped ? new Color(1f,.40f,.34f) : new Color(.49f,.95f,1f);
            }
            if (statusText != null && statusText.text != message) statusText.text = message;
            if (readingsText != null)
                readingsText.text = ready ? $"TIMER {voltage:0.00} V   |   MOTOR {1000f * Mathf.Abs(motor.ElectricalState.Current):0} mA   |   {(tripped ? "JAM TRIPPED" : running ? "RUN" : "STOPPED")}" : "TIMER —   |   MOTOR —";
            if (jamLabel != null) jamLabel.text = stage == 4 ? "CLEAR JAM" : "JAM";
            if (startButton != null) startButton.interactable = ready && stage == 0 && !started && voltage < IdleVoltage;
            if (briefButton != null) briefButton.interactable = ready && stage == 1 && !pulseActive && !pulseFinished && !tripped;
            if (jamButton != null) jamButton.interactable = ready && (stage == 2 && !blocked || stage == 4 && blocked);
            if (resetButton != null) resetButton.interactable = ready && (stage == 3 && blocked || stage == 5 && !blocked && voltage < ResetVoltage);
            if (retryButton != null) retryButton.interactable = ready && stage < Gates.Length && (started || stage > 0);
        }

        private void AnimateBelt(float dt)
        {
            if (shaft != null) shaft.Rotate(Vector3.right, 720f * dt, Space.Self);
            if (beltSlats == null || beltSlats.Length == 0) return;
            beltOffset = Mathf.Repeat(beltOffset + dt * .52f, beltLength);
            for (int i=0; i<beltSlats.Length; i++)
            {
                if (beltSlats[i] == null) continue;
                var p=beltSlats[i].localPosition;
                p.x=Mathf.Repeat(i * beltLength / beltSlats.Length + beltOffset, beltLength) + beltStartX;
                beltSlats[i].localPosition=p;
            }
        }

        private static void SetVisible(Transform model, bool visible)
        {
            if (model != null && model.gameObject.activeSelf != visible) model.gameObject.SetActive(visible);
        }

        private void OnDisable()
        {
            if (gameObject.activeInHierarchy) ResetTrial();
            else if (bindings != null) foreach (string id in Gates) bindings.ClearRuntimeComponentBinding(id);
            bindings = null;
            ready = false;
            if (runLight != null) runLight.enabled = false;
            if (jamLight != null) jamLight.enabled = false;
        }

        private static void SetContact(SparkSwitch contact, bool closed)
        {
            if (contact != null && contact.IsClosed != closed)
                contact.SetState(closed ? SparkSwitchState.Closed : SparkSwitchState.Open);
        }

        private static bool Available(SparkElectricalComponent component) => component != null && component.isActiveAndEnabled && component.ElectricalEnabled;

        private string CheckWiring(SparkElectricalNetworkGraph graph)
        {
            if (graph == null) return "Waiting for the solved circuit graph.";
            var parents = new Dictionary<int,int>();
            int Root(int node)
            {
                while (parents.TryGetValue(node,out int parent)) node = parent;
                return node;
            }
            foreach (var entry in graph.ComponentTerminals)
            {
                if (!(entry.Key is SparkSingleWire wire) || !Available(wire) ||
                    !graph.TryGetPrimaryComponentTerminals(wire,out var a,out var b) ||
                    !graph.TryGetNode(a,out int an) || !graph.TryGetNode(b,out int bn)) continue;
                int aRoot = Root(an), bRoot = Root(bn);
                if (aRoot != bRoot) parents[bRoot] = aRoot;
            }

            var nets = new[] {
                new[] { source.PositiveTerminal, chargeContact.InputTerminal, motorContact.InputTerminal },
                new[] { chargeContact.OutputTerminal, chargeResistor.TerminalA },
                new[] { timerCapacitor.PositiveTerminal, chargeResistor.TerminalB, dischargeResistor.TerminalA },
                new[] { dischargeResistor.TerminalB, clearContact.InputTerminal },
                new[] { source.NegativeTerminal, timerCapacitor.NegativeTerminal, clearContact.OutputTerminal, motor.NegativeTerminal },
                new[] { motorContact.OutputTerminal, motor.PositiveTerminal }
            };
            var used = new Dictionary<int,string>();
            foreach (var net in nets)
            {
                if (net[0] == null || !graph.TryGetNode(net[0], out int first)) return "A required terminal is missing from the solver graph.";
                int root = Root(first);
                if (used.TryGetValue(root,out string other)) return $"Separate {net[0].name} from {other}. Check for a bypass wire.";
                used.Add(root,net[0].name);
                foreach (var pin in net)
                {
                    if (pin == null || !graph.TryGetNode(pin,out int node)) return $"A terminal is missing on the {net[0].name} path.";
                    if (Root(node) != root) return $"Connect {net[0].name} to {pin.name}. See the wiring diagram.";
                }
            }
            return null;
        }
    }
}
