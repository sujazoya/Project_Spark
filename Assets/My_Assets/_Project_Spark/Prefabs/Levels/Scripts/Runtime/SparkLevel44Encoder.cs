using System;
using System.Collections.Generic;
using ProjectSpark.Circuit;
using ProjectSpark.Electrical;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.Gameplay
{
    [DisallowMultipleComponent]
    public sealed class SparkLevel44Encoder : MonoBehaviour
    {
        public static readonly string[] Gates = {
            "L44_Wired", "L44_Homed", "L44_Direction", "L44_Stations", "L44_Repaired", "L44_PowerRecovery"
        };
        private static readonly int[] Gray = {0, 2, 3, 1};
        private static readonly int[] Transitions = {0,-1,1,0, 1,0,0,-1, -1,0,0,1, 0,1,-1,0};
        private const int CountsPerTurn = 80;
        private const int StationA = 20, StationB = 52;
        private enum Motion { Stop, Home, Clockwise, Counterclockwise, Station, Test }

        public SparkLevelLayout layout;
        public SparkPowerSupply source;
        public SparkPowerSupply[] signals;
        public SparkSwitch[] motorBridge;
        public SparkSwitch decoderPower;
        public SparkTerminal decoderRail;
        public SparkMotor motor;
        public SparkTerminal[] pins;
        public Button homeButton, cwButton, ccwButton, aButton, bButton, faultButton, powerButton, restartButton;
        public Text statusText, meterText;
        public TextMesh countReadout, stateReadout;
        public Transform platter, disc;
        public GameObject[] signalLamps, decoderLamps, motionLamps, markerLamps;
        public Transform[] buttons;

        private readonly List<SparkCircuitConnection> wires = new();
        private LevelGamePlayManager manager;
        private SparkLevelLayoutLoader loader;
        private SparkLevelSceneBindings bindings;
        private SparkCircuitSystem circuit;
        private SparkElectricalSolver solver;
        private int topology = -1, stage, physical = 13, count, previous, station, jogRemaining, startCount;
        private int pressed = -1;
        private float elapsed, pressUntil;
        private bool crossed, ready, power = true, homed, cwVerified, aVerified, faultSeen, cycled, z;
        private string wiring;
        private Motion motion;
        private Vector3[] rest;

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
                Stop();
                topology = circuit.TopologyVersion;
                wiring = CheckWiring();
                if (wiring != null) homed = false;
            }

            ready = wiring == null && power && ReadInputs(out _, out _);
            if (!ready)
            {
                Stop();
                Show();
                return;
            }
            if (stage == 0) Award();
            if (motion != Motion.Stop)
            {
                float rate = motion == Motion.Home ? 23f : motion == Motion.Test ? 15f : 32f;
                elapsed += Mathf.Min(Time.unscaledDeltaTime, .075f) * rate;
                while (elapsed >= 1f && motion != Motion.Stop)
                {
                    elapsed -= 1f;
                    Step();
                }
            }
            Show();
        }

        private bool ReadInputs(out int ab, out bool index)
        {
            ab = 0;
            index = false;
            if (solver.IsSolving || (solver.IsDirty && !solver.SolveNow())) return false;
            var result = solver.LatestSolveResult;
            if (result == null || solver.IsDirty || !source.IsOutputActive || source.IsOverloaded ||
                !result.TryGetVoltage(decoderRail, pins[3], out float supply) || supply < 4.8f ||
                !result.TryGetVoltage(pins[7], pins[3], out float a) ||
                !result.TryGetVoltage(pins[9], pins[3], out float b) ||
                !result.TryGetVoltage(pins[5], pins[3], out float pulse)) return false;
            ab = (a > 2.5f ? 2 : 0) | (b > 2.5f ? 1 : 0);
            index = pulse > 2.5f;
            return true;
        }

        private void Step()
        {
            if (!motor.IsRunning || motor.IsOverCurrent) return;
            int direction = motor.ElectricalState.Voltage > 0f ? 1 : -1;
            physical = Wrap(physical + direction);
            PublishSignals();
            if (!ReadInputs(out int ab, out z))
            {
                homed = false;
                Stop();
                return;
            }
            int delta = Transitions[previous * 4 + ab];
            count += delta;
            previous = ab;
            switch (motion)
            {
                case Motion.Home when z:
                    homed = true;
                    count = 0;
                    Stop();
                    if (stage == 1 || stage == 4 && faultSeen && !crossed) Award();
                    break;
                case Motion.Clockwise:
                case Motion.Counterclockwise:
                    if (--jogRemaining > 0) break;
                    Stop();
                    if (count > startCount && direction > 0) cwVerified = true;
                    if (stage == 2 && cwVerified && direction < 0 && count < startCount) Award();
                    break;
                case Motion.Station when count == station:
                    Stop();
                    if (stage == 3 && station == StationA) aVerified = true;
                    if (stage == 3 && aVerified && station == StationB || stage == 5 && cycled && station == StationB) Award();
                    break;
                case Motion.Test:
                    if (count <= startCount - 3)
                    {
                        homed = false;
                        faultSeen = true;
                        Stop();
                    }
                    else if (--jogRemaining <= 0) Stop();
                    break;
            }
        }

        private static int Wrap(int value) => (value % CountsPerTurn + CountsPerTurn) % CountsPerTurn;

        private void PublishSignals()
        {
            int phase = Gray[physical % Gray.Length];
            SetSignal(signals[0], (phase & 2) != 0);
            SetSignal(signals[1], (phase & 1) != 0);
            SetSignal(signals[2], physical == 0);
        }

        private static void SetSignal(SparkPowerSupply supply, bool high)
        {
            float volts = high ? 5f : 0f;
            if (Mathf.Approximately(supply.OutputVoltage, volts)) return;
            supply.SetOutput(false, default);
            if (!supply.TryConfigure(volts, .025f, out var reason)) Debug.LogError(reason, supply);
            supply.SetOutput(true, default);
        }

        private bool CanAct() => ready && motion == Motion.Stop && circuit.TopologyVersion == topology;
        private bool CanHome => !crossed && (stage == 1 || stage is 2 or 3 && !homed ||
            stage == 4 && faultSeen || stage == 5 && !homed);

        public void Home()
        {
            if (!CanAct() || !CanHome) return;
            Press(0);
            StartMotion(Motion.Home);
        }

        public void JogCW()
        {
            if (!CanAct() || stage != 2 || !homed) return;
            Press(1);
            jogRemaining = 7;
            StartMotion(Motion.Clockwise);
        }

        public void JogCCW()
        {
            if (!CanAct() || stage != 2 || !cwVerified || !homed) return;
            Press(2);
            jogRemaining = 7;
            StartMotion(Motion.Counterclockwise);
        }

        public void GoA()
        {
            if (!CanAct() || !homed || stage != 3) return;
            Press(3);
            Go(StationA);
        }

        public void GoB()
        {
            if (!CanAct() || !homed || !(stage == 3 && aVerified || stage == 5)) return;
            Press(4);
            Go(StationB);
        }

        private void Go(int destination)
        {
            station = destination;
            if (count != station) StartMotion(Motion.Station);
        }

        public void TestDirection()
        {
            if (!CanAct() || stage != 4 || !crossed || faultSeen) return;
            Press(5);
            jogRemaining = 8;
            StartMotion(Motion.Test);
        }

        public void TogglePower()
        {
            if (stage != 5 || motion != Motion.Stop || wiring != null || cycled && power) return;
            Press(6);
            if (power && physical == 0) return;
            power = !power;
            decoderPower.SetState(power ? SparkSwitchState.Closed : SparkSwitchState.Open);
            if (!power)
            {
                count = 0;
                homed = false;
                cycled = true;
                ready = false;
            }
        }

        private void StartMotion(Motion next)
        {
            if (!ReadInputs(out previous, out z)) return;
            motion = next;
            elapsed = 0f;
            startCount = count;
            int pair = next == Motion.Counterclockwise || next == Motion.Station && count > station ? 2 : 0;
            motorBridge[pair].SetState(SparkSwitchState.Closed);
            motorBridge[pair + 1].SetState(SparkSwitchState.Closed);
        }

        private void Stop()
        {
            motion = Motion.Stop;
            foreach (var contact in motorBridge)
                if (contact.IsClosed) contact.SetState(SparkSwitchState.Open);
        }

        public void Restart()
        {
            if (bindings == null) return;
            Stop();
            foreach (string id in Gates) bindings.ClearRuntimeComponentBinding(id);
            stage = count = 0;
            topology = -1;
            power = true;
            ready = homed = cwVerified = aVerified = faultSeen = cycled = false;
            physical = 13;
            decoderPower.SetState(SparkSwitchState.Closed);
            PublishSignals();
            manager.QueueEvaluation();
        }

        private void Award()
        {
            if (bindings.SetRuntimeComponentBinding(Gates[stage], source))
            {
                stage++;
                manager.QueueEvaluation();
            }
        }

        private string CheckWiring()
        {
            crossed = false;
            circuit.CopyConnections(wires);
            var links = new Dictionary<SparkTerminal, SparkTerminal>();
            SparkTerminal Root(SparkTerminal pin)
            {
                while (links.TryGetValue(pin, out var parent)) pin = parent;
                return pin;
            }
            foreach (var wire in wires)
            {
                if (wire.Kind != SparkConnectionKind.Wire) continue;
                var a = Root(wire.A);
                var b = Root(wire.B);
                if (a != b) links[b] = a;
            }
            var used = new HashSet<SparkTerminal>();
            foreach (var (from, to) in new[] {(0,2), (1,3), (4,5)})
            {
                var node = Root(pins[from]);
                if (node != Root(pins[to])) return $"Connect {pins[from].name} to {pins[to].name}.";
                if (!used.Add(node)) return "Separate supply, return and index wiring.";
            }
            var encoderA = Root(pins[6]);
            var encoderB = Root(pins[8]);
            if (!used.Add(encoderA) || !used.Add(encoderB)) return "Keep A and B on separate signal nets.";
            if (encoderA == Root(pins[7]) && encoderB == Root(pins[9])) return null;
            if (stage == 4 && encoderA == Root(pins[9]) && encoderB == Root(pins[7]))
            {
                crossed = true;
                return null;
            }
            return stage == 4 && !faultSeen ? "Cross ENC A to DEC B and ENC B to DEC A." : "Connect ENC A to DEC A and ENC B to DEC B.";
        }

        private string Prompt()
        {
            if (wiring != null) return wiring;
            if (!power) return "Decoder off. Turn power on to verify the count is unreferenced.";
            if (motion == Motion.Home) return "Searching clockwise for the Z index pulse…";
            if (motion == Motion.Station) return $"Indexing to Station {(station == StationA ? "A" : "B")} at +{station} counts…";
            if (motion == Motion.Test) return "Commanded clockwise. Watch the direction of the count.";
            if (stage is 2 or 3 && !homed) return "Press HOME to re-establish the reference after rewiring.";
            return stage switch
            {
                0 => "Waiting for the solved decoder supply and signal inputs.",
                1 => "Press HOME to find the Z index and zero the count.",
                2 => cwVerified ? "Jog CCW. B should lead and the count should fall." :
                    "Jog CW. A should lead and the count should rise.",
                3 => aVerified ? "Select Station B at +52 counts." : "Select Station A at +20 counts.",
                4 => !faultSeen ? crossed ? "Press TEST DIRECTION." : "Undo A and B, then cross them at the decoder." :
                    crossed ? "Direction fault. Undo the crossed leads and restore A → A and B → B." :
                    "Inputs repaired. Re-home to recover the count reference.",
                5 => !cycled ? "Index to Station B again with MOVE B, then cycle power away from home." :
                    !homed ? "The table stayed put. Press HOME to recover the count reference." :
                    "Select Station B to verify repeatability.",
                _ => "Home and both stations verified. Station B is repeatable after power loss."
            };
        }

        private void Press(int index)
        {
            pressed = index;
            pressUntil = Time.unscaledTime + .15f;
        }

        private void Show()
        {
            bool idle = motion == Motion.Stop;
            int phase = Gray[physical % 4];
            string state = !power ? "POWER OFF" : stage == 4 && faultSeen ? "DIRECTION FAULT" : homed ? "HOMED" : "UNREFERENCED";
            statusText.text = Prompt();
            string countText = power ? count.ToString("+0;-0;0") : "—";
            string lead = motion == Motion.Clockwise || motion == Motion.Test ? "CW" : motion == Motion.Counterclockwise ? "CCW" : "—";
            if (ready && motion == Motion.Clockwise) lead = "CW · A LEADS";
            if (ready && motion == Motion.Counterclockwise) lead = "CCW · B LEADS";
            meterText.text = $"A {(phase >> 1)}  B {(phase & 1)}  Z {(physical == 0 ? 1 : 0)}     COUNT {countText}     {state}     {lead}";
            countReadout.text = countText;
            stateReadout.text = state;
            platter.localRotation = disc.localRotation = Quaternion.Euler(0, physical * 4.5f, 0);
            SetLamp(signalLamps[0], (phase & 2) != 0);
            SetLamp(signalLamps[1], (phase & 1) != 0);
            SetLamp(signalLamps[2], physical == 0);
            SetLamp(decoderLamps[0], power && !homed && state != "DIRECTION FAULT");
            SetLamp(decoderLamps[1], power && homed);
            SetLamp(decoderLamps[2], power && state == "DIRECTION FAULT");
            SetLamp(motionLamps[0], !idle);
            SetLamp(motionLamps[1], idle);
            SetLamp(markerLamps[0], idle && physical == 0);
            SetLamp(markerLamps[1], idle && physical == StationA);
            SetLamp(markerLamps[2], idle && physical == StationB);
            rest ??= Array.ConvertAll(buttons, part => part.localPosition);
            for (int i = 0; i < buttons.Length; i++)
                buttons[i].localPosition = rest[i] - Vector3.up * (i == pressed && Time.unscaledTime < pressUntil ? .009f : 0f);

            homeButton.interactable = ready && idle && CanHome;
            cwButton.interactable = ready && idle && stage == 2 && homed;
            ccwButton.interactable = cwButton.interactable && cwVerified;
            aButton.interactable = ready && idle && homed && stage == 3;
            bButton.interactable = ready && idle && homed && (stage == 3 && aVerified || stage == 5);
            faultButton.interactable = ready && idle && stage == 4 && crossed && !faultSeen;
            powerButton.interactable = idle && stage == 5 && wiring == null && (!power || !cycled && physical != 0);
            powerButton.GetComponentInChildren<Text>().text = power ? "POWER OFF" : "POWER ON";
            restartButton.interactable = true;
        }

        private static void SetLamp(GameObject lamp, bool active)
        {
            if (lamp.activeSelf != active) lamp.SetActive(active);
        }

        private void OnDisable()
        {
            if (motorBridge != null && gameObject.activeInHierarchy) Stop();
            if (bindings != null)
                foreach (var id in Gates) bindings.ClearRuntimeComponentBinding(id);
            bindings = null;
        }
    }
}
