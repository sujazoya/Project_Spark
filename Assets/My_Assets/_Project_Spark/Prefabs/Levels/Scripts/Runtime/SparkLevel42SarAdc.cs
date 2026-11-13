using System;
using System.Collections.Generic;
using ProjectSpark.Circuit;
using ProjectSpark.Electrical;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.Gameplay
{
    [DisallowMultipleComponent]
    public sealed class SparkLevel42SarAdc : MonoBehaviour
    {
        public static readonly string[] Gates =
        {
            "L42_Wired", "L42_Msb", "L42_SensorA", "L42_SensorB", "L42_InputFault", "L42_Sweep"
        };

        public SparkLevelLayout layout;
        public SparkPowerSupply source, sensor;
        public SparkSwitch[] highContacts, lowContacts;
        public SparkTerminal[] bitOutputs;
        public SparkResistor[] bitResistors;
        public SparkResistor link21, link10, termination;
        public SparkSwitch terminationContact;
        public SparkTerminal comparatorPlus, comparatorMinus;
        public Text statusText, meterText, sweepText;
        public Button testButton, keepButton, clearButton, submitButton, nextButton, sweepButton, restartButton;
        public TextMesh trialReadout, voltageReadout, comparisonReadout, sensorReadout;
        public Transform[] rockers;
        public Transform selector;
        public GameObject referenceLamp, terminationLamp, aboveLamp, belowLamp, trialLamp, submitLamp, sweepLamp;
        public GameObject[] bitLamps, sensorLamps, sweepBars;

        private readonly List<SparkCircuitConnection> wires = new List<SparkCircuitConnection>();
        private readonly int[] sweepResults = new int[8];
        private LevelGamePlayManager manager;
        private SparkLevelLayoutLoader loader;
        private SparkCircuitSystem circuit;
        private SparkElectricalSolver solver;
        private SparkLevelSceneBindings bindings;
        private int topology = -1, stage, sensorIndex, bit, kept, trialCode, sweepCount, comparisons;
        private bool ready, trial, submitted, sweeping, reversed, faultSeen, faultHigh, faultLow, faultTested;
        private float dac, input, difference, reference, dwell;
        private string wiring, feedback;

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
                topology = -1;
                ResetTrial();
            }
            if (bindings == null) return;
            if (topology != circuit.TopologyVersion)
            {
                if (stage > 0 && stage < 4) ResetTrial();
                if (sweeping) { sweeping = false; sweepCount = 0; }
                if (stage == 4) faultTested = false;
                topology = circuit.TopologyVersion;
                wiring = CheckWiring();
            }
            if (!Refresh())
            {
                Show(wiring ?? "Waiting for solved voltages.");
                return;
            }
            if (stage == 0 && !reversed && Mathf.Abs(dac) < .025f) Award();
            if (stage == 4 && faultTested)
            {
                if (reversed && trialCode == 4 && difference < -.05f) faultSeen = true;
                if (!reversed && faultSeen && trialCode == 4 && difference > .05f) faultHigh = true;
                if (!reversed && faultHigh && trialCode == 6 && difference < -.05f) faultLow = true;
                if (faultLow) Award();
            }
            if (sweeping) StepSweep();
            Show(feedback ?? Prompt());
        }

        private bool Refresh()
        {
            ready = false;
            if (wiring != null || solver.IsSolving || (solver.IsDirty && !solver.SolveNow())) return false;
            var result = solver.LatestSolveResult;
            var ground = source.NegativeTerminal;
            reference = source.ElectricalState.Voltage;
            ready = result != null && !solver.IsDirty && source.IsOutputActive && sensor.IsOutputActive &&
                !source.IsOverloaded && !sensor.IsOverloaded && reference >= 4.8f && reference <= 5.2f &&
                result.TryGetVoltage(bitResistors[2].TerminalB, ground, out dac) &&
                result.TryGetVoltage(sensor.PositiveTerminal, ground, out input) &&
                result.TryGetVoltage(comparatorPlus, comparatorMinus, out difference) &&
                Finite(dac) && Finite(input) && Finite(difference);
            return ready;
        }

        private static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
        private bool CanAct() => ready && !sweeping && circuit != null && circuit.TopologyVersion == topology && Refresh();
        private bool Manual => stage >= 1 && stage <= 3 && !submitted;
        private bool Above => difference >= -0.0001f;

        public void TestBit()
        {
            if (!CanAct()) return;
            feedback = null;
            if (Manual && !trial && bit >= 0)
            {
                Drive(kept | (1 << bit));
                trial = true;
            }
            else if (stage == 4 && (reversed || faultSeen))
            {
                faultTested = true;
                Drive(!reversed && faultHigh ? 6 : 4);
            }
        }

        public void Keep() => Decide(true);
        public void Clear() => Decide(false);

        private void Decide(bool keep)
        {
            if (!CanAct() || !Manual || !trial || keep != Above) 
            {
                if (ready && Manual && trial) feedback = "Read the comparator again and choose the other decision.";
                return;
            }
            feedback = null;
            if (keep) kept = trialCode;
            comparisons++;
            bit--;
            trial = false;
            Drive(kept);
            if (stage == 1) Award();
        }

        public void Submit()
        {
            if (!CanAct() || !Manual || trial || bit >= 0 || comparisons != 3) return;
            int expected = Mathf.Clamp(Mathf.FloorToInt(input * 8f / reference), 0, 7);
            if (kept != expected) { feedback = "That code missed the input band. Restart the conversion."; return; }
            submitted = true;
            feedback = null;
            Award();
        }

        public void NextSignal()
        {
            if (!CanAct() || !submitted || stage < 3 || stage > 4) return;
            SelectSensor(stage == 3 ? 1 : 2, stage == 3 ? 1.100f : 3.200f);
            if (stage == 4) Drive(4);
        }

        public void RunSweep()
        {
            if (!CanAct() || stage != 5 || reversed) return;
            sweepCount = 0;
            sweeping = true;
            BeginSweepCode();
        }

        private void BeginSweepCode()
        {
            SelectSensor(2, reference * (sweepCount + .5f) / 8f);
            Drive(4);
            dwell = 0f;
        }

        private void StepSweep()
        {
            dwell += Time.unscaledDeltaTime;
            if (dwell < .24f) return;
            dwell = 0f;
            if (Above) kept = trialCode;
            if (--bit >= 0) { Drive(kept | (1 << bit)); return; }
            if (kept != sweepCount || Mathf.Abs(dac - reference * trialCode / 8f) > .035f)
            {
                sweeping = false;
                feedback = "Sweep failed. Check the comparator inputs and ladder.";
                return;
            }
            sweepResults[sweepCount++] = kept;
            Drive(kept);
            if (sweepCount == 8) { sweeping = false; Award(); }
            else BeginSweepCode();
        }

        public void Restart()
        {
            if (bindings == null || sweeping) return;
            ResetTrial();
            wiring = CheckWiring();
        }

        private void SelectSensor(int index, float volts)
        {
            sensorIndex = index;
            bit = 2;
            kept = comparisons = 0;
            trial = submitted = faultTested = false;
            feedback = null;
            sensor.SetOutput(false, default);
            bool configured = sensor.TryConfigure(volts, .005f, out var reason);
            sensor.SetOutput(true, default);
            if (!configured) feedback = "Sensor source could not be configured. " + reason;
            Drive(0);
        }

        private void Drive(int code)
        {
            trialCode = code;
            for (int i = 0; i < 3; i++)
            {
                bool high = (code & (1 << i)) != 0;
                (high ? lowContacts[i] : highContacts[i]).SetState(SparkSwitchState.Open);
                (high ? highContacts[i] : lowContacts[i]).SetState(SparkSwitchState.Closed);
            }
        }

        private void Award()
        {
            if (!bindings.SetRuntimeComponentBinding(Gates[stage], source))
            {
                feedback = "Check the Level 42 objective bindings.";
                return;
            }
            stage++;
            manager.QueueEvaluation();
        }

        private void ResetTrial()
        {
            foreach (string id in Gates) bindings?.ClearRuntimeComponentBinding(id);
            stage = sweepCount = 0;
            ready = sweeping = faultSeen = faultHigh = faultLow = false;
            SelectSensor(0, 3.200f);
            manager.QueueEvaluation();
        }

        private string Prompt() => stage switch
        {
            0 => "Wire the ladder and both comparator inputs.",
            1 => trial ? "Sensor A. Keep B2 when SENSOR is at or above DAC." : "Sensor A. Test B2 first.",
            2 => trial ? "Decide from the comparator. Keep if above, clear if below." :
                bit < 0 ? "Submit the Sensor A code." : $"Test B{bit}.",
            3 => submitted ? "Sensor A verified. Load Sensor B." : trial ? "Use the comparison to decide." :
                bit < 0 ? "Submit the Sensor B code." : $"Test B{bit}.",
            4 => submitted ? "Sensor B verified. Load the 3.200 V fault test." :
                !faultSeen ? "Undo the two input wires. Cross SENSOR OUT and DAC TAP, then test 100." :
                reversed ? "The result is reversed. Undo both crossed wires and reconnect by label." :
                faultHigh ? "Test 110 to verify SENSOR BELOW DAC." : "Test 100 to verify SENSOR ABOVE DAC.",
            5 => sweeping ? "The analyzer is testing three comparisons in each input band." : "Inputs restored. Run the eight-code sweep.",
            _ => "All eight input bands verified."
        };

        private void Show(string message)
        {
            bool comparing = ready && (trial || sweeping || stage == 4 && faultTested || stage == 5 && faultTested);
            string relation = comparing ? Above ? "SENSOR AT / ABOVE DAC" : "SENSOR BELOW DAC" : "—";
            string code = ready ? Convert.ToString(trialCode, 2).PadLeft(3, '0') : "—";
            string dacText = ready ? $"{dac:0.000} V" : "—";
            string signal = sensorIndex == 0 ? "SENSOR A" : sensorIndex == 1 ? "SENSOR B" : "TEST INPUT";
            string sensorText = ready && (submitted || sensorIndex == 2) ? $"{signal}  {input:0.000} V" : signal;
            statusText.text = message;
            meterText.text = $"TRIAL {code}    |    DAC {dacText}    |    {relation}";
            sweepText.text = sweepCount > 0 ? $"{sensorText}    |    SWEEP {sweepCount}/8" : sensorText;
            testButton.interactable = ready && !sweeping && (Manual && !trial && bit >= 0 ||
                stage == 4 && !submitted && (reversed || faultSeen));
            keepButton.interactable = clearButton.interactable = ready && Manual && trial;
            submitButton.interactable = ready && Manual && !trial && bit < 0;
            nextButton.interactable = ready && submitted && (stage == 3 || stage == 4);
            nextButton.GetComponentInChildren<Text>().text = stage == 4 ? "LOAD FAULT TEST" : "LOAD SENSOR B";
            sweepButton.interactable = ready && stage == 5 && !reversed && !sweeping;
            restartButton.interactable = !sweeping;
            SetText(trialReadout, code);
            SetText(voltageReadout, dacText);
            SetText(comparisonReadout, comparing ? Above ? "ABOVE" : "BELOW" : "—");
            SetText(sensorReadout, sensorText);
            SetLamp(referenceLamp, source.IsOutputActive);
            SetLamp(terminationLamp, terminationContact.IsClosed);
            SetLamp(aboveLamp, comparing && Above);
            SetLamp(belowLamp, comparing && !Above);
            SetLamp(trialLamp, trial);
            SetLamp(submitLamp, submitted);
            SetLamp(sweepLamp, sweeping || sweepCount == 8);
            for (int i = 0; i < 3; i++)
            {
                bool high = (trialCode & (1 << i)) != 0;
                if (rockers != null && rockers.Length == 3) rockers[i].localRotation = Quaternion.Euler(0, 0, high ? -12f : 12f);
                if (bitLamps != null && bitLamps.Length == 3) SetLamp(bitLamps[i], ready && high);
                if (sensorLamps != null && sensorLamps.Length == 3) SetLamp(sensorLamps[i], sensorIndex == i);
            }
            if (selector != null) selector.localRotation = Quaternion.Euler(0, (sensorIndex - 1) * 38f, 0);
            if (sweepBars != null)
                for (int i = 0; i < sweepBars.Length; i++) SetLamp(sweepBars[i], i < sweepCount);
        }

        private static void SetText(TextMesh label, string value) { if (label != null) label.text = value; }
        private static void SetLamp(GameObject lamp, bool state) { if (lamp != null && lamp.activeSelf != state) lamp.SetActive(state); }

        private string CheckWiring()
        {
            reversed = false;
            circuit.CopyConnections(wires);
            var parents = new Dictionary<SparkTerminal, SparkTerminal>();
            SparkTerminal Root(SparkTerminal pin)
            {
                while (parents.TryGetValue(pin, out var parent)) pin = parent;
                return pin;
            }
            foreach (var wire in wires)
            {
                if (wire.Kind != SparkConnectionKind.Wire) continue;
                var a = Root(wire.A);
                var b = Root(wire.B);
                if (a != b) parents[b] = a;
            }
            var nets = new[]
            {
                new[] {bitOutputs[2], bitResistors[2].TerminalA},
                new[] {bitOutputs[1], bitResistors[1].TerminalA},
                new[] {bitOutputs[0], bitResistors[0].TerminalA},
                new[] {bitResistors[2].TerminalB, link21.TerminalA},
                new[] {link21.TerminalB, bitResistors[1].TerminalB, link10.TerminalA},
                new[] {link10.TerminalB, bitResistors[0].TerminalB, termination.TerminalA},
                new[] {terminationContact.OutputTerminal, source.NegativeTerminal}
            };
            var used = new HashSet<SparkTerminal> { Root(source.PositiveTerminal), Root(sensor.PositiveTerminal) };
            if (used.Count != 2) return "Separate the reference and sensor outputs.";
            foreach (var net in nets)
            {
                var root = Root(net[0]);
                if (!used.Add(root)) return $"Separate {net[0].name} from the other ladder nodes.";
                for (int i = 1; i < net.Length; i++)
                    if (Root(net[i]) != root) return $"Connect {net[0].name} to {net[i].name}.";
            }
            var signal = Root(sensor.PositiveTerminal);
            var tap = Root(bitResistors[2].TerminalB);
            var plus = Root(comparatorPlus);
            var minus = Root(comparatorMinus);
            if (plus == signal && minus == tap) return null;
            if (stage == 4 && sensorIndex == 2 && plus == tap && minus == signal)
            {
                reversed = true;
                return null;
            }
            return stage == 4 && sensorIndex == 2 && !faultSeen
                ? "Cross SENSOR OUT to DAC − and DAC TAP to SENSOR + to reproduce the fault."
                : "Connect SENSOR OUT to SENSOR + and DAC TAP to DAC −.";
        }
    }
}
