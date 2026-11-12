using System;
using System.Collections.Generic;
using ProjectSpark.Circuit;
using ProjectSpark.Electrical;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.Gameplay
{
    [DisallowMultipleComponent]
    public sealed class SparkLevel36ThermalInterlock : MonoBehaviour
    {
        public static readonly string[] Gates =
        {
            "L36_ColdStart", "L36_HotTrip", "L36_WarmHold",
            "L36_CoolRecovery", "L36_OpenFault", "L36_ShortAndReset"
        };

        private enum Temperature { Cool, Warm, Hot }
        private enum SensorMode { Normal, Open, Short }
        private enum FaultTest { RestoreOpen, ResetOpen, VerifyOpenReset, InjectShort, RestoreShort, ResetShort, VerifyReset }

        private const float HotThreshold = 2f;
        private const float CoolThreshold = 2.7f;
        private const float ShortThreshold = .15f;
        private const float OpenMargin = .15f;

        public SparkLevelLayout layout;
        public SparkPowerSupply source;
        public SparkResistor pullUp, thermistor, heater, ledResistor;
        public SparkMotor fan;
        public SparkLED faultLed;
        public SparkSwitch sensorDisconnect, sensorBypass, fanContact, heaterContact, faultContact;
        public SparkTerminal sensorInput, sensorReturn;
        public Transform fanRotor;
        public GameObject heaterGlow, warningGlow;
        public Transform fanArm, heaterArm, faultArm;
        public GameObject supplyStatus, sensorHeat, fanStatus, heaterStatus, faultStatus;
        public TextMesh senseReadout, outputReadout;
        public Text statusText, meterText, outputsText;
        public Button coolButton, warmButton, hotButton, openButton, shortButton, restoreButton, resetButton, restartButton;

        private SparkLevelLayoutLoader loader;
        private LevelGamePlayManager manager;
        private SparkElectricalSolver solver;
        private SparkCircuitSystem circuit;
        private SparkLevelSceneBindings bindings;
        private SparkElectricalNetworkGraph checkedGraph;
        private readonly List<SparkCircuitConnection> playerConnections = new List<SparkCircuitConnection>();
        private SparkElectricalSolveResult measuredResult;
        private string wiringProblem;
        private int topology = -1;
        private int stage;
        private bool ready, cooling, faultLatched, coolReturnObserved;
        private bool fanOn, heaterOn, warningOn;
        private float sense, stableTime;
        private Temperature temperature;
        private SensorMode sensorMode;
        private FaultTest faultTest;

        public void SelectCool() => SetTemperature(Temperature.Cool);
        public void SelectWarm() => SetTemperature(Temperature.Warm);
        public void SelectHot() => SetTemperature(Temperature.Hot);

        private void SetTemperature(Temperature requested)
        {
            if (!CanControl() || sensorMode != SensorMode.Normal || faultLatched) return;
            temperature = requested;
            thermistor.SetResistance(requested switch
            {
                Temperature.Cool => 15000f,
                Temperature.Warm => 8200f,
                _ => 4700f
            });
            stableTime = 0f;
        }

        public void OpenSensor()
        {
            if (CanControl() && stage == 4 && sensorMode == SensorMode.Normal)
                SetSensorMode(SensorMode.Open);
        }

        public void ShortSensor()
        {
            if (CanControl() && stage == 5 && faultTest == FaultTest.InjectShort)
                SetSensorMode(SensorMode.Short);
        }

        public void RestoreSensor()
        {
            if (CanControl() && stage == 5 &&
                (faultTest == FaultTest.RestoreOpen || faultTest == FaultTest.RestoreShort))
                SetSensorMode(SensorMode.Normal);
        }

        public void ResetFault()
        {
            if (!CanControl() || !CanResetFault()) return;
            faultLatched = false;
            cooling = false;
            faultTest = faultTest == FaultTest.ResetOpen ? FaultTest.VerifyOpenReset : FaultTest.VerifyReset;
            stableTime = 0f;
        }

        public void RestartTest()
        {
            if (CanControl() && stage < Gates.Length) ResetTrial();
        }

        private bool CanControl()
        {
            if (!isActiveAndEnabled || loader == null || loader.ActiveLayout != layout || circuit == null ||
                solver == null || solver.IsSolving || circuit.TopologyVersion != topology)
                return false;
            if (solver.IsDirty || !ReferenceEquals(solver.LatestSolveResult, measuredResult))
                RefreshReadings(0f);
            return ready && !solver.IsDirty && ReferenceEquals(solver.LatestSolveResult, measuredResult);
        }

        private bool CanResetFault() =>
            stage == 5 && faultLatched && sensorMode == SensorMode.Normal && sense > CoolThreshold &&
            (faultTest == FaultTest.ResetOpen || faultTest == FaultTest.ResetShort);

        private void SetSensorMode(SensorMode mode)
        {
            sensorMode = mode;
            SetContact(sensorBypass, mode == SensorMode.Short);
            SetContact(sensorDisconnect, mode != SensorMode.Open);
            stableTime = 0f;
        }

        private void Update() => RefreshReadings(Time.unscaledDeltaTime);

        private void RefreshReadings(float elapsed)
        {
            if (loader == null) loader = FindFirstObjectByType<SparkLevelLayoutLoader>();
            if (layout == null || loader == null || loader.ActiveLayout != layout) return;
            if (manager == null) manager = FindFirstObjectByType<LevelGamePlayManager>();
            if (solver == null) solver = FindFirstObjectByType<SparkElectricalSolver>();
            if (circuit == null) circuit = FindFirstObjectByType<SparkCircuitSystem>();
            if (solver == null || circuit == null)
            {
                Show("Waiting for the circuit and solver.");
                return;
            }
            if (bindings != loader.ActiveBindings || topology != circuit.TopologyVersion)
            {
                ResetTrial();
                bindings = loader.ActiveBindings;
                topology = circuit.TopologyVersion;
                checkedGraph = null;
            }
            if (bindings == null || Array.Exists(Gates, id => !bindings.IsComponentIdDeclared(id)))
            {
                Invalidate("Check the six Level 36 objective bindings.");
                return;
            }
            if (solver.SolverRegistry == null || !solver.SolverRegistry.TryGetDefinition(fan, out _))
            {
                Invalidate("Register the existing SparkMotor solver in the active solver registry.");
                return;
            }
            if (solver.IsSolving) return;
            if ((solver.IsDirty || solver.LatestSolveResult == null) && !solver.SolveNow())
            {
                Invalidate("Check the circuit connections. A solve is unavailable.");
                return;
            }
            var result = solver.LatestSolveResult;
            if (result == null || solver.IsDirty) return;
            if (!ReferenceEquals(checkedGraph, result.Context.Graph))
            {
                checkedGraph = result.Context.Graph;
                wiringProblem = CheckWiring(checkedGraph);
            }
            if (wiringProblem != null)
            {
                Invalidate(wiringProblem);
                return;
            }
            float rail = source.ElectricalState.Voltage;
            if (!source.IsOutputActive || source.IsOverloaded || !(rail >= 4.8f && rail <= 5.2f) ||
                !result.TryGetVoltage(sensorInput, source.NegativeTerminal, out sense) ||
                float.IsNaN(sense) || float.IsInfinity(sense))
            {
                Invalidate("Check the supply and the SENSE connection.");
                return;
            }
            measuredResult = result;
            ready = true;

            bool openReading = sense >= rail - OpenMargin;
            bool shortReading = sense <= ShortThreshold;
            if (openReading || shortReading) faultLatched = true;
            if (sense < HotThreshold) cooling = true;
            else if (sense > CoolThreshold) cooling = false;

            bool outputChanged = SetContact(fanContact, faultLatched || cooling);
            outputChanged |= SetContact(heaterContact, !faultLatched && !cooling);
            outputChanged |= SetContact(faultContact, faultLatched);
            if (outputChanged || solver.IsDirty || !ReferenceEquals(result, solver.LatestSolveResult)) return;

            float fanCurrent = Mathf.Abs(fan.ElectricalState.Current);
            float heaterCurrent = Mathf.Abs(heater.ElectricalState.Current);
            float ledCurrent = Mathf.Abs(faultLed.ElectricalState.Current);
            fanOn = fan.IsRunning && fanCurrent > .06f;
            heaterOn = heaterCurrent > .03f;
            warningOn = faultLed.IsOn && ledCurrent > .002f;
            bool coldOutputs = !faultLatched && fanCurrent < .001f && heaterOn && ledCurrent < .001f;
            bool hotOutputs = !faultLatched && fanOn && heaterCurrent < .001f && ledCurrent < .001f;
            bool faultOutputs = faultLatched && fanOn && heaterCurrent < .001f && warningOn;
            bool coolReading = sensorMode == SensorMode.Normal && temperature == Temperature.Cool && sense > CoolThreshold && !openReading;
            bool warmReading = sensorMode == SensorMode.Normal && temperature == Temperature.Warm &&
                sense >= HotThreshold && sense <= CoolThreshold;

            bool passed = false;
            switch (stage)
            {
                case 0:
                    passed = coolReading && coldOutputs;
                    break;
                case 1:
                    passed = temperature == Temperature.Hot && sense < HotThreshold && !shortReading && hotOutputs;
                    break;
                case 2:
                    passed = warmReading && hotOutputs;
                    break;
                case 3:
                    if (coolReading && coldOutputs) coolReturnObserved = true;
                    passed = coolReturnObserved && warmReading && coldOutputs;
                    break;
                case 4:
                    passed = sensorMode == SensorMode.Open && openReading && faultOutputs;
                    break;
                case 5:
                    passed = VerifyFaultTest(shortReading, faultOutputs, coldOutputs, openReading);
                    break;
            }
            stableTime = passed ? stableTime + elapsed : 0f;
            if (stableTime >= .2f && !Award())
            {
                Invalidate("Could not record the Level 36 objective.");
                return;
            }
            if (fanOn && fanRotor != null) fanRotor.Rotate(Vector3.up, 1100f * elapsed, Space.Self);
            Show(Prompt());
        }

        private bool VerifyFaultTest(bool shortReading, bool faultOutputs, bool coldOutputs, bool openReading)
        {
            bool restoredReading = sensorMode == SensorMode.Normal && !shortReading && !openReading;
            if (faultTest == FaultTest.RestoreOpen && restoredReading && faultOutputs)
                faultTest = FaultTest.ResetOpen;
            else if (faultTest == FaultTest.VerifyOpenReset && restoredReading && coldOutputs)
                faultTest = FaultTest.InjectShort;
            else if (faultTest == FaultTest.InjectShort && sensorMode == SensorMode.Short && shortReading && faultOutputs)
                faultTest = FaultTest.RestoreShort;
            else if (faultTest == FaultTest.RestoreShort && restoredReading && faultOutputs)
                faultTest = FaultTest.ResetShort;
            return faultTest == FaultTest.VerifyReset && restoredReading && coldOutputs;
        }

        private string Prompt() => stage switch
        {
            0 => "Wire the sensor and the three output branches.",
            1 => "Cold start verified. Select HOT.",
            2 => "Fan running. Select WARM and check that cooling continues.",
            3 => coolReturnObserved ? "Select WARM again. The fan should stay off."
                : "Select COOL. Check that the fan stops and the heater returns.",
            4 => sensorMode == SensorMode.Open ? "Checking the open sensor response."
                : "Select COOL to prepare for reset, then press OPEN SENSOR.",
            5 => faultTest switch
            {
                FaultTest.RestoreOpen => "Open fault latched. Press RESTORE SENSOR.",
                FaultTest.ResetOpen => "The fault held after restoration. Press RESET FAULT.",
                FaultTest.VerifyOpenReset => "Checking normal operation after the open-fault reset.",
                FaultTest.InjectShort => "Press SHORT SENSOR to test the low-voltage fault.",
                FaultTest.RestoreShort => "Short fault latched. Press RESTORE SENSOR.",
                FaultTest.ResetShort => "The short fault held after restoration. Press RESET FAULT.",
                _ => "Checking normal operation after the final reset."
            },
            _ => manager != null && manager.SceneBindings == bindings && manager.IsCompleted
                ? "Thermal interlock verified. All six checks passed."
                : "All six checks recorded. Waiting for level completion."
        };

        private void Show(string message)
        {
            if (statusText != null) statusText.text = message;
            string sensorLabel = sensorMode switch
            {
                SensorMode.Open => "OPEN SENSOR",
                SensorMode.Short => "SHORTED SENSOR",
                _ => temperature.ToString().ToUpperInvariant()
            };
            if (meterText != null) meterText.text = ready ? $"SENSE {sense:0.00} V    |    {sensorLabel}"
                : "SENSE —";
            if (outputsText != null) outputsText.text = ready
                ? $"FAN {(fanOn ? "ON" : "OFF")}    |    HEATER {(heaterOn ? "ON" : "OFF")}    |    FAULT {(warningOn ? "LATCHED" : "CLEAR")}"
                : "FAN —    |    HEATER —    |    FAULT —";
            if (senseReadout != null)
                senseReadout.text = ready ? $"SENSE {sense:0.00} V  /  {sensorLabel}" : "SENSE —";
            if (outputReadout != null)
            {
                outputReadout.text = ready
                    ? $"FAN {(fanOn ? "ON" : "OFF")}    HEAT {(heaterOn ? "ON" : "OFF")}    FAULT {(warningOn ? "LATCHED" : "CLEAR")}"
                    : "FAN —    HEAT —    FAULT —";
                outputReadout.color = warningOn ? new Color(1f, .59f, .28f) : new Color(.89f, .96f, 1f);
            }
            SetVisual(heaterGlow, ready && heaterOn);
            SetVisual(warningGlow, ready && warningOn);
            SetVisual(supplyStatus, source != null && source.IsOutputActive);
            SetVisual(sensorHeat, ready && temperature == Temperature.Hot && sensorMode == SensorMode.Normal);
            ShowContact(fanArm, fanStatus, ready && fanContact.IsClosed);
            ShowContact(heaterArm, heaterStatus, ready && heaterContact.IsClosed);
            ShowContact(faultArm, faultStatus, ready && faultContact.IsClosed);
            SetButton(hotButton, ready && stage == 1);
            SetButton(warmButton, ready && (stage == 2 || stage == 3 && coolReturnObserved));
            SetButton(coolButton, ready && (stage == 3 && !coolReturnObserved || stage == 4 && temperature != Temperature.Cool));
            SetButton(openButton, ready && stage == 4 && sensorMode == SensorMode.Normal &&
                temperature == Temperature.Cool && sense > CoolThreshold);
            SetButton(shortButton, ready && stage == 5 && faultTest == FaultTest.InjectShort && !faultLatched);
            SetButton(restoreButton, ready && stage == 5 && sensorMode != SensorMode.Normal &&
                (faultTest == FaultTest.RestoreOpen || faultTest == FaultTest.RestoreShort));
            SetButton(resetButton, ready && CanResetFault());
            SetButton(restartButton, ready && stage > 0 && stage < Gates.Length);
        }

        private bool Award()
        {
            if (!bindings.SetRuntimeComponentBinding(Gates[stage], source)) return false;
            stage++;
            stableTime = 0f;
            QueueEvaluation();
            return true;
        }

        private void QueueEvaluation()
        {
            if (manager != null && manager.SceneBindings == bindings) manager.QueueEvaluation();
        }

        private void ClearGates()
        {
            if (bindings != null)
                foreach (string gate in Gates) bindings.ClearRuntimeComponentBinding(gate);
            if (stage > 0) QueueEvaluation();
        }

        private void ResetTrial()
        {
            ClearGates();
            stage = 0;
            stableTime = 0f;
            measuredResult = null;
            ready = cooling = faultLatched = coolReturnObserved = false;
            fanOn = heaterOn = warningOn = false;
            temperature = Temperature.Cool;
            faultTest = FaultTest.RestoreOpen;
            if (thermistor != null) thermistor.SetResistance(15000f);
            SetSensorMode(SensorMode.Normal);
            SetContact(fanContact, false);
            SetContact(heaterContact, false);
            SetContact(faultContact, false);
        }

        private void Invalidate(string reason)
        {
            if (ready || stage > 0) ResetTrial();
            Show(reason);
        }

        private void OnDisable()
        {
            if (gameObject.activeInHierarchy) ResetTrial();
            else ClearGates();
            bindings = null;
            ready = false;
        }

        private static bool SetContact(SparkSwitch contact, bool closed)
        {
            if (contact == null || contact.IsClosed == closed) return false;
            contact.SetState(closed ? SparkSwitchState.Closed : SparkSwitchState.Open);
            return true;
        }

        private static void SetVisual(GameObject visual, bool on)
        {
            if (visual != null && visual.activeSelf != on) visual.SetActive(on);
        }

        private static void ShowContact(Transform arm, GameObject status, bool closed)
        {
            if (arm != null) arm.localRotation = Quaternion.Euler(0, 0, closed ? -14f : 0f);
            SetVisual(status, closed);
        }

        private static void SetButton(Button button, bool enabled)
        {
            if (button != null) button.interactable = enabled;
        }

        private string CheckWiring(SparkElectricalNetworkGraph graph)
        {
            if (graph == null) return "Waiting for the solved circuit graph.";
            var parents = new Dictionary<int, int>();
            int Root(int node)
            {
                while (parents.TryGetValue(node, out int parent)) node = parent;
                return node;
            }
            void Join(SparkTerminal a, SparkTerminal b)
            {
                if (a == null || b == null || !graph.TryGetNode(a, out int an) ||
                    !graph.TryGetNode(b, out int bn)) return;
                int aRoot = Root(an);
                int bRoot = Root(bn);
                if (aRoot != bRoot) parents[bRoot] = aRoot;
            }
            circuit.CopyConnections(playerConnections);
            foreach (var connection in playerConnections)
                if (connection.Kind == SparkConnectionKind.Wire) Join(connection.A, connection.B);

            foreach (var entry in graph.ComponentTerminals)
            {
                if (!(entry.Key is SparkSingleWire wire) || !wire.isActiveAndEnabled || !wire.ElectricalEnabled ||
                    !graph.TryGetPrimaryComponentTerminals(wire, out var a, out var b)) continue;
                Join(a, b);
            }
            var nets = new[]
            {
                new[] { source.PositiveTerminal, pullUp.TerminalA, fanContact.InputTerminal, heaterContact.InputTerminal, faultContact.InputTerminal },
                new[] { source.NegativeTerminal, sensorReturn, fan.NegativeTerminal, heater.TerminalB, faultLed.CathodeTerminal },
                new[] { pullUp.TerminalB, sensorInput },
                new[] { fanContact.OutputTerminal, fan.PositiveTerminal },
                new[] { heaterContact.OutputTerminal, heater.TerminalA },
                new[] { faultContact.OutputTerminal, ledResistor.TerminalA },
                new[] { ledResistor.TerminalB, faultLed.AnodeTerminal }
            };
            var used = new Dictionary<int, string>();
            foreach (var net in nets)
            {
                if (net[0] == null || !graph.TryGetNode(net[0], out int first))
                    return "A required terminal is missing from the solver graph.";
                int root = Root(first);
                if (used.TryGetValue(root, out string other)) return $"Separate {net[0].name} from {other}.";
                used.Add(root, net[0].name);
                for (int i = 1; i < net.Length; i++)
                {
                    var pin = net[i];
                    if (pin == null || !graph.TryGetNode(pin, out int node)) return $"A terminal is missing on {net[0].name}.";
                    if (Root(node) != root) return $"Connect {net[0].name} to {pin.name}.";
                }
            }
            return null;
        }
    }
}
