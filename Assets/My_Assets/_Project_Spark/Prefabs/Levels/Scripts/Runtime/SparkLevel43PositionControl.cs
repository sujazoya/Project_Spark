using System;
using System.Collections.Generic;
using ProjectSpark.Circuit;
using ProjectSpark.Electrical;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.Gameplay
{
    [DisallowMultipleComponent]
    public sealed class SparkLevel43PositionControl : MonoBehaviour
    {
        public static readonly string[] Gates = {
            "L43_Wired", "L43_Feedback", "L43_StationA", "L43_StationB", "L43_Repaired", "L43_Recovery"
        };

        public SparkLevelLayout layout;
        public SparkPowerSupply source, command, feedback;
        public SparkMotor motor;
        public SparkSwitch[] bridge;
        public SparkTerminal[] wirePins;
        public SparkLevel43VisualMotionRig rig;
        public Text statusText, meterText;
        public Button jogLeft, jogRight, stationA, stationB, faultTest, nudgeLeft, nudgeRight, restart;
        public GameObject[] controllerLamps, driverLamps, modeLamps;
        public Transform[] pressVisuals;
        public TextMesh commandReadout, positionReadout, driveReadout, errorReadout;

        private readonly List<SparkCircuitConnection> connections = new();
        private const float Deadband = .03f;
        private const float StationA = .25f, StationB = .75f;
        private SparkLevelLayoutLoader loader;
        private LevelGamePlayManager manager;
        private SparkLevelSceneBindings bindings;
        private SparkCircuitSystem circuit;
        private SparkElectricalSolver solver;
        private int topology = -1, stage, drive, jog, recovery;
        private float position = .5f, target = .5f, commandVolts, feedbackVolts, jogUntil, hold, publishAt;
        private bool ready, crossed, automatic, testing, faultSeen, repairing;
        private string wiring;
        private Vector3[] pressRest;
        private int pressed = -1;
        private float pressUntil;

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
                Drive(0);
                topology = circuit.TopologyVersion;
                wiring = CheckWiring();
                if (stage < 4 && wiring != null) RestartProgress();
                if (stage >= 4 && wiring != null)
                {
                    automatic = testing = repairing = false;
                    hold = 0f;
                    if (stage == 5) recovery = 0;
                }
            }

            ready = wiring == null && ReadSignals();
            if (!ready)
            {
                Drive(0);
                Present(wiring ?? "Waiting for the 5 V reference and solved position feedback.");
                return;
            }

            if (stage == 0) Award();
            float error = (commandVolts - feedbackVolts) / 5f;
            int wanted = automatic
                ? Mathf.Abs(error) <= Deadband ? 0 : error > 0f ? 1 : -1
                : Time.unscaledTime < jogUntil ? jog : 0;
            if ((position <= 0f && wanted < 0) || (position >= 1f && wanted > 0)) wanted = 0;
            Drive(wanted);

            float dt = Mathf.Min(Time.unscaledDeltaTime, .05f);
            int actual = motor.IsRunning && !motor.IsOverCurrent ? (motor.ElectricalState.Voltage > 0f ? 1 : -1) : 0;
            if (actual != 0)
            {
                position = Mathf.Clamp01(position + actual * dt * .38f);
                if (Time.unscaledTime >= publishAt || testing)
                {
                    publishAt = Time.unscaledTime + .05f;
                    SetVoltage(feedback, 5f * position);
                }
                if (testing && actual != wanted && position < .46f)
                {
                    faultSeen = true;
                    automatic = testing = false;
                    Drive(0);
                    actual = 0;
                }
            }
            else if (Mathf.Abs(feedback.OutputVoltage - 5f * position) > .0005f)
                SetVoltage(feedback, 5f * position);

            if (stage == 1 && !automatic && jog > 0 && position > .55f && feedbackVolts > 2.70f) Award();
            bool settled = automatic && Mathf.Abs(position - target) <= Deadband &&
                Mathf.Abs(error) <= Deadband && drive == 0 && !motor.IsRunning;
            hold = settled ? hold + dt : 0f;
            if (stage == 2 && target == StationA && hold >= .35f) Award();
            if (stage == 3 && target == StationB && hold >= 1f) Award();
            if (stage == 4 && repairing && !crossed && target == StationB && hold >= .45f) Award();
            if (stage == 5 && recovery is 1 or 3 && hold >= .45f)
            {
                recovery++;
                if (recovery == 4) Award();
            }

            rig.SetNormalizedPosition(position);
            rig.SetDriveDirection(actual);
            Present(Prompt());
        }

        private bool ReadSignals()
        {
            if (solver.IsSolving || (solver.IsDirty && !solver.SolveNow())) return false;
            var result = solver.LatestSolveResult;
            return result != null && !solver.IsDirty && source.IsOutputActive &&
                !source.IsOverloaded && Mathf.Abs(source.ElectricalState.Voltage - 5f) <= .2f &&
                command.IsOutputActive && feedback.IsOutputActive &&
                result.TryGetVoltage(wirePins[5], source.NegativeTerminal, out commandVolts) &&
                result.TryGetVoltage(wirePins[7], source.NegativeTerminal, out feedbackVolts);
        }

        private bool CanAct() => ready && circuit != null && circuit.TopologyVersion == topology;
        public void JogLeft() => Jog(-1, 0);
        public void JogRight() => Jog(1, 1);
        private void Jog(int direction, int visual)
        {
            if (!CanAct() || stage != 1) return;
            Press(visual);
            automatic = false;
            jog = direction;
            jogUntil = Time.unscaledTime + .42f;
        }

        public void GoA()
        {
            if (CanAct() && stage == 2) { Press(2); Go(StationA); }
        }

        public void GoB()
        {
            if (!CanAct()) return;
            if (stage == 3) { Press(3); Go(StationB); }
            else if (stage == 4 && faultSeen && !crossed)
            {
                repairing = true;
                Press(3);
                Go(StationB);
            }
            else if (stage == 5 && !automatic) Go(StationB);
        }

        private void Go(float destination)
        {
            target = destination;
            jogUntil = hold = 0f;
            automatic = true;
            SetVoltage(command, 5f * target);
        }

        public void TestFault()
        {
            if (!CanAct() || stage != 4 || !crossed || faultSeen || testing) return;
            position = .5f;
            SetVoltage(feedback, 2.5f);
            testing = true;
            Go(StationB);
        }

        public void NudgeLeft() => Nudge(-.18f, 0, 4);
        public void NudgeRight() => Nudge(.18f, 2, 5);
        private void Nudge(float offset, int expected, int visual)
        {
            if (!CanAct() || stage != 5 || recovery != expected || !automatic || drive != 0) return;
            Press(visual);
            recovery++;
            hold = 0f;
            position = Mathf.Clamp01(position + offset);
            SetVoltage(feedback, 5f * position);
        }

        public void Restart()
        {
            if (bindings == null) return;
            RestartProgress();
            position = target = .5f;
            SetVoltage(command, 2.5f);
            SetVoltage(feedback, 2.5f);
            rig.SetNormalizedPosition(position);
            topology = -1;
        }

        private void RestartProgress()
        {
            foreach (var id in Gates) bindings.ClearRuntimeComponentBinding(id);
            stage = recovery = jog = 0;
            hold = jogUntil = 0f;
            ready = automatic = testing = repairing = faultSeen = false;
            Drive(0);
            manager.QueueEvaluation();
        }

        private void Award()
        {
            if (bindings.SetRuntimeComponentBinding(Gates[stage], source))
            {
                stage++;
                hold = 0f;
                manager.QueueEvaluation();
            }
        }

        private void Drive(int value)
        {
            if (value == drive) return;
            drive = value;
            // Break before make so the bridge cannot briefly short the supply.
            foreach (var contact in bridge) if (contact.IsClosed) contact.SetState(SparkSwitchState.Open);
            if (value != 0)
            {
                int index = value > 0 ? 0 : 2;
                bridge[index].SetState(SparkSwitchState.Closed);
                bridge[index + 1].SetState(SparkSwitchState.Closed);
            }
            if (value == 0 && rig != null) rig.StopVisualShaft();
        }

        private static void SetVoltage(SparkPowerSupply supply, float volts)
        {
            if (Mathf.Abs(supply.OutputVoltage - volts) < .0005f) return;
            supply.SetOutput(false, default);
            bool configured = supply.TryConfigure(volts, .01f, out string reason);
            supply.SetOutput(true, default);
            if (!configured) Debug.LogError(reason, supply);
        }

        private string CheckWiring()
        {
            crossed = false;
            circuit.CopyConnections(connections);
            var parents = new Dictionary<SparkTerminal, SparkTerminal>();
            SparkTerminal Root(SparkTerminal pin)
            {
                while (parents.TryGetValue(pin, out var parent)) pin = parent;
                return pin;
            }
            foreach (var wire in connections)
            {
                if (wire.Kind != SparkConnectionKind.Wire) continue;
                var a = Root(wire.A);
                var b = Root(wire.B);
                if (a != b) parents[b] = a;
            }

            var used = new HashSet<SparkTerminal>();
            foreach (var (from, to) in new[] { (0, 2), (1, 3), (4, 5), (6, 7) })
            {
                var net = Root(wirePins[from]);
                if (net != Root(wirePins[to])) return $"Connect {wirePins[from].name} to {wirePins[to].name}.";
                if (!used.Add(net)) return $"Separate the {wirePins[from].name} circuit from the other connections.";
            }
            var aOut = Root(wirePins[8]);
            var bOut = Root(wirePins[10]);
            if (!used.Add(aOut) || !used.Add(bOut)) return "Separate the two motor outputs from supply and signal wiring.";
            if (aOut == Root(wirePins[9]) && bOut == Root(wirePins[11])) return null;
            if (stage == 4 && aOut == Root(wirePins[11]) && bOut == Root(wirePins[9]))
            {
                crossed = true;
                return null;
            }
            return "Connect OUT A to MOTOR A and OUT B to MOTOR B.";
        }

        private string Prompt() => stage switch
        {
            0 => "Wire the control loop.",
            1 => "Jog right in MANUAL. Watch POSITION rise.",
            2 => "Select AUTO A. Watch the carriage settle at Station A.",
            3 => "Select AUTO B. Hold at Station B with the motor stopped.",
            4 => !faultSeen ? crossed ? "Press TEST FAULT and watch the initial direction." :
                    "Undo the two motor wires. Cross OUT A and OUT B at the motor." :
                crossed ? "SAFE STOP. Undo the crossed wires and reconnect A to A, B to B." :
                "Press AUTO B to verify the repaired drive.",
            5 => !automatic ? "Press AUTO B to resume holding Station B." :
                recovery == 0 ? "Nudge left and watch the automatic recovery." :
                recovery == 2 ? "Now nudge right and watch the recovery." : "Recovering to Station B…",
            _ => "Station B held after both disturbances."
        };

        private void Press(int index)
        {
            pressed = index;
            pressUntil = Time.unscaledTime + .16f;
        }

        private void Present(string message)
        {
            string mode = automatic ? "AUTO" : "MANUAL";
            string state = stage == 4 && faultSeen && !repairing ? "SAFE STOP" :
                drive < 0 ? "LEFT" : drive > 0 ? "RIGHT" : "STOP";
            statusText.text = message;
            meterText.text = ready
                ? $"{mode}     COMMAND {commandVolts:F2} V     POSITION {feedbackVolts:F2} V     DRIVE {state}"
                : $"{mode}     COMMAND —     POSITION —     DRIVE {state}";
            if (commandReadout != null) commandReadout.text = ready ? $"{commandVolts:F2} V" : "—";
            if (positionReadout != null) positionReadout.text = ready ? $"{feedbackVolts:F2} V" : "—";
            if (driveReadout != null) driveReadout.text = state;
            if (errorReadout != null) errorReadout.text = ready ? $"{commandVolts - feedbackVolts:+0.00;-0.00;0.00} V" : "—";
            SetLamps(controllerLamps, drive + 1);
            SetLamps(driverLamps, drive + 1);
            SetLamps(modeLamps, automatic ? 1 : 0);
            if (pressVisuals != null && pressVisuals.Length > 0)
            {
                pressRest ??= Array.ConvertAll(pressVisuals, part => part.localPosition);
                for (int i = 0; i < pressVisuals.Length; i++)
                    pressVisuals[i].localPosition = pressRest[i] - Vector3.up *
                        (i == pressed && Time.unscaledTime < pressUntil ? .009f : 0f);
            }

            jogLeft.interactable = jogRight.interactable = ready && stage == 1;
            stationA.interactable = ready && stage == 2;
            stationB.interactable = ready && (stage == 3 || stage == 4 && faultSeen && !crossed || stage == 5 && !automatic);
            faultTest.interactable = ready && stage == 4 && crossed && !faultSeen && !testing;
            nudgeLeft.interactable = ready && automatic && stage == 5 && recovery == 0 && drive == 0;
            nudgeRight.interactable = ready && automatic && stage == 5 && recovery == 2 && drive == 0;
            restart.interactable = bindings != null;
        }

        private static void SetLamps(GameObject[] lamps, int index)
        {
            if (lamps == null) return;
            for (int i = 0; i < lamps.Length; i++)
                if (lamps[i].activeSelf != (i == index)) lamps[i].SetActive(i == index);
        }

        private void OnDisable()
        {
            if (bindings != null)
                foreach (var id in Gates) bindings.ClearRuntimeComponentBinding(id);
            if (gameObject.activeInHierarchy) Drive(0);
            if (rig != null) rig.StopVisualShaft();
            bindings = null;
        }
    }
}
