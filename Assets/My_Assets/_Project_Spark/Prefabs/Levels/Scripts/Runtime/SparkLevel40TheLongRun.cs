using System;
using System.Collections.Generic;
using ProjectSpark.Circuit;
using ProjectSpark.Electrical;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.Gameplay
{
    [DisallowMultipleComponent]
    public sealed class SparkLevel40TheLongRun : MonoBehaviour
    {
        public static readonly string[] Gates =
        {
            "L40_LiveZero", "L40_Midscale", "L40_LongCable",
            "L40_ComplianceLimit", "L40_FullScale", "L40_OpenAndRecover"
        };

        private const float MinimumHeadroom = 2f;
        private const float CurrentTolerance = .00015f;
        private const float OpenThreshold = .0002f;
        private const float ShortCableOhms = 50f;
        private const float LongCableOhms = 350f;

        public SparkLevelLayout layout;
        public SparkPowerSupply source;
        public SparkResistor transmitter, cable, receiver;
        public SparkSwitch lineContact;
        public Transform tankFill;
        public GameObject normalIndicator, faultIndicator;
        public Text statusText, meterText, tankText, cableLabel, lineLabel;
        public Button midscaleButton, fullscaleButton, cableButton, lineButton, restartButton;

        private SparkLevelLayoutLoader loader;
        private LevelGamePlayManager manager;
        private SparkCircuitSystem circuit;
        private SparkElectricalSolver solver;
        private SparkLevelSceneBindings bindings;
        private SparkElectricalSolveResult measuredResult;
        private readonly List<SparkCircuitConnection> connections = new List<SparkCircuitConnection>();
        private int topology = -1;
        private int stage, tankPercent;
        private bool ready, longCable, openObserved, lineOpen, limited;
        private float current, receiverVoltage, transmitterVoltage, stableTime;
        private string wiringIssue, feedback;

        private float CommandCurrent => .004f + .00016f * tankPercent;

        public float TankFraction => tankPercent / 100f;
        public bool HasLoopReading => ready;
        public float LoopCurrentAmps => current;
        public float ReceiverVoltage => receiverVoltage;
        public bool IsHeadroomLimited => limited;
        public bool IsLineOpen => lineOpen;
        public float ReceiverFraction => Mathf.Clamp01((current - .004f) / .016f);

        public void SelectMidscale()
        {
            if (CanControl() && stage == 1) tankPercent = 50;
        }

        public void SelectFullscale()
        {
            if (CanControl() && stage == 3) tankPercent = 100;
        }

        public void ToggleCable()
        {
            if (!CanControl() || !CanToggleCable()) return;
            longCable = !longCable;
            cable.SetResistance(longCable ? LongCableOhms : ShortCableOhms);
        }

        public void ToggleLine()
        {
            if (!CanControl() || !CanToggleLine()) return;
            SetLine(!lineContact.IsClosed);
        }

        public void Restart()
        {
            if (CanControl() && stage < Gates.Length) ResetTrial();
        }

        private bool CanControl()
        {
            if (!isActiveAndEnabled || loader == null || loader.ActiveLayout != layout || circuit == null ||
                solver == null || solver.IsSolving || topology != circuit.TopologyVersion)
                return false;
            if (solver.IsDirty || !ReferenceEquals(solver.LatestSolveResult, measuredResult))
                RefreshReadings(0f);
            return ready && !solver.IsDirty && ReferenceEquals(solver.LatestSolveResult, measuredResult);
        }

        private bool CanToggleCable() => ready && (stage == 2 && !longCable || stage == 4 && longCable);
        private bool CanToggleLine() => ready && stage == 5 && (openObserved ? !lineContact.IsClosed : lineContact.IsClosed);

        private void Update() => RefreshReadings(Time.unscaledDeltaTime);

        private void RefreshReadings(float elapsed)
        {
            if (loader == null) loader = FindFirstObjectByType<SparkLevelLayoutLoader>();
            if (layout == null || loader == null || loader.ActiveLayout != layout) return;
            if (manager == null) manager = FindFirstObjectByType<LevelGamePlayManager>();
            if (circuit == null) circuit = manager != null ? manager.CircuitSystem : FindFirstObjectByType<SparkCircuitSystem>();
            if (solver == null) solver = manager != null ? manager.ElectricalSolver : FindFirstObjectByType<SparkElectricalSolver>();
            if (circuit == null || solver == null)
            {
                Show("Waiting for the circuit and solver.");
                return;
            }
            if (bindings != loader.ActiveBindings || topology != circuit.TopologyVersion)
            {
                ResetTrial();
                bindings = loader.ActiveBindings;
                topology = circuit.TopologyVersion;
                wiringIssue = CheckWiring();
            }
            if (bindings == null || Array.Exists(Gates, gate => !bindings.IsComponentIdDeclared(gate)))
            {
                Hold("Check the six Level 40 objective bindings.");
                return;
            }
            if (wiringIssue != null)
            {
                Hold(wiringIssue);
                return;
            }
            if (!Available(source) || !Available(transmitter) || !Available(cable) || !Available(receiver) ||
                !Available(lineContact) || transmitter.IsBurned || cable.IsBurned || receiver.IsBurned)
            {
                Hold("Check the five loop components.");
                return;
            }
            if (solver.IsSolving) return;
            if ((solver.IsDirty || solver.LatestSolveResult == null) && !solver.SolveNow())
            {
                Hold("The solver could not evaluate the loop.");
                return;
            }
            var result = solver.LatestSolveResult;
            if (result == null || solver.IsDirty) return;
            float rail = source.ElectricalState.Voltage;
            if (!source.IsOutputActive || source.IsOverloaded || !(rail >= 11.7f && rail <= 12.3f) ||
                !result.TryGetVoltage(transmitter.TerminalA, transmitter.TerminalB, out transmitterVoltage) ||
                !result.TryGetVoltage(receiver.TerminalA, receiver.TerminalB, out receiverVoltage) ||
                !IsFinite(transmitterVoltage) || !IsFinite(receiverVoltage))
            {
                Hold("Check the 12 V supply and receiver polarity.");
                return;
            }
            current = receiver.ElectricalState.Current;
            lineOpen = !lineContact.IsClosed && Mathf.Abs(current) < OpenThreshold && Mathf.Abs(receiverVoltage) < .05f;
            limited = false;
            if (lineContact.IsClosed)
            {
                if (current <= OpenThreshold || !IsFinite(current))
                {
                    Hold("The closed loop is missing test current. Check the series connections.");
                    return;
                }
                // Infer the rest of the loop from solved voltage and current, including contact and wire losses.
                float externalOhms = (rail - transmitterVoltage) / current;
                if (!IsFinite(externalOhms) || externalOhms <= 0f)
                {
                    Hold("The transmitter could not measure the loop resistance.");
                    return;
                }
                float availableCurrent = (rail - MinimumHeadroom) / externalOhms;
                float regulatedCurrent = Mathf.Min(CommandCurrent, availableCurrent);
                if (regulatedCurrent <= OpenThreshold)
                {
                    Hold("The loop resistance is too high for the transmitter.");
                    return;
                }
                limited = availableCurrent < CommandCurrent - CurrentTolerance;
                if (Mathf.Abs(current - regulatedCurrent) > .000025f || transmitterVoltage < MinimumHeadroom - .03f)
                {
                    transmitter.SetResistance(Mathf.Max(.1f, rail / regulatedCurrent - externalOhms));
                    stableTime = 0f;
                    return;
                }
            }
            else if (!lineOpen)
            {
                Hold("Waiting for current to fall after the line opens.");
                return;
            }

            ready = true;
            measuredResult = result;
            bool passing = CheckObjective(rail);
            stableTime = passing ? stableTime + elapsed : 0f;
            if (stableTime >= .15f) Award();
            Show(feedback ?? Prompt());
        }

        private bool CheckObjective(float rail)
        {
            bool fullscale = tankPercent == 100 && !longCable && !limited && Near(.020f,5f);
            switch (stage)
            {
                case 0: return tankPercent == 0 && !longCable && !limited && Near(.004f,1f);
                case 1: return tankPercent == 50 && !longCable && !limited && Near(.012f,3f);
                case 2: return tankPercent == 50 && longCable && !limited && Near(.012f,3f) && transmitterVoltage < 5.2f;
                case 3:
                    float expectedLimitedCurrent = (rail - MinimumHeadroom) / (LongCableOhms + 250f);
                    return tankPercent == 100 && longCable && limited &&
                        Near(expectedLimitedCurrent, expectedLimitedCurrent * 250f) &&
                        Mathf.Abs(transmitterVoltage - MinimumHeadroom) <= .08f;
                case 4: return fullscale;
                case 5:
                    if (lineOpen) openObserved = true;
                    return openObserved && fullscale;
                default: return false;
            }
        }

        private bool Near(float expectedCurrent,float expectedVoltage) => !lineOpen &&
            Mathf.Abs(current - expectedCurrent) <= CurrentTolerance &&
            Mathf.Abs(receiverVoltage - expectedVoltage) <= .055f;

        private string Prompt() => stage switch
        {
            0 => "Wire the series loop. At 0%, live zero should read 4 mA.",
            1 => "Live zero verified. Select 50% TANK.",
            2 => "Midscale verified. Fit the 350 Ω LONG CABLE.",
            3 => "The long cable still reads 50%. Select 100% TANK to test headroom.",
            4 => "The transmitter reached its 2 V limit. Fit the 50 Ω SHORT CABLE.",
            5 => openObserved ? "LINE OPEN verified. Close the line and check full scale returns."
                : "Full scale restored. Open the line to test the fault indication.",
            _ => manager != null && manager.SceneBindings == bindings && manager.IsCompleted
                ? "Loop verified. Live zero, full scale, and open-line recovery passed."
                : "All six checks recorded. Waiting for level completion."
        };

        private void Show(string message)
        {
            if (statusText != null) statusText.text = message;
            if (meterText != null)
            {
                string state = lineOpen ? "LINE OPEN" : limited ? "HEADROOM LIMIT" : "REGULATED";
                meterText.text = ready
                    ? $"LOOP {current * 1000f:0.00} mA    |    RX {receiverVoltage:0.00} V    |    TX {transmitterVoltage:0.00} V    |    {state}"
                    : "LOOP —    |    RX —    |    TX —";
            }
            if (tankText != null)
            {
                string decoded = ready && !lineOpen ? $"{(ReceiverFraction * 100f):0}%"
                    : ready ? "LINE OPEN" : "—";
                tankText.text = $"TANK {tankPercent}%    |    RECEIVER {decoded}    |    CABLE {(longCable ? 350 : 50)} Ω";
            }
            if (tankFill != null)
            {
                float height = .30f * tankPercent / 100f;
                tankFill.gameObject.SetActive(height > 0f);
                tankFill.localPosition = new Vector3(0f, .04f + height * .5f, 0f);
                tankFill.localScale = new Vector3(.16f, Mathf.Max(.001f,height), .05f);
            }
            if (normalIndicator != null) normalIndicator.SetActive(ready && !lineOpen && !limited);
            if (faultIndicator != null) faultIndicator.SetActive(ready && (lineOpen || limited));
            if (cableLabel != null) cableLabel.text = longCable ? "FIT SHORT CABLE" : "FIT LONG CABLE";
            if (lineLabel != null) lineLabel.text = lineContact != null && lineContact.IsClosed ? "OPEN LINE" : "CLOSE LINE";
            if (midscaleButton != null) midscaleButton.interactable = ready && stage == 1 && tankPercent == 0;
            if (fullscaleButton != null) fullscaleButton.interactable = ready && stage == 3 && tankPercent == 50;
            if (cableButton != null) cableButton.interactable = CanToggleCable();
            if (lineButton != null) lineButton.interactable = CanToggleLine();
            if (restartButton != null) restartButton.interactable = ready && stage > 0 && stage < Gates.Length;
        }

        private void Award()
        {
            if (!bindings.SetRuntimeComponentBinding(Gates[stage], source))
            {
                feedback = "Could not record the Level 40 objective.";
                return;
            }
            stage++;
            stableTime = 0f;
            QueueEvaluation();
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
            stage = tankPercent = 0;
            stableTime = 0f;
            ready = longCable = openObserved = lineOpen = limited = false;
            feedback = null;
            measuredResult = null;
            if (transmitter != null) transmitter.SetResistance(2700f);
            if (cable != null) cable.SetResistance(ShortCableOhms);
            SetLine(true);
        }

        private void SetLine(bool closed)
        {
            if (lineContact != null && lineContact.IsClosed != closed)
                lineContact.SetState(closed ? SparkSwitchState.Closed : SparkSwitchState.Open);
        }

        private void Hold(string message)
        {
            ready = false;
            stableTime = 0f;
            measuredResult = null;
            Show(message);
        }

        private void OnDisable()
        {
            if (gameObject.activeInHierarchy) ResetTrial();
            else ClearGates();
            bindings = null;
            ready = false;
        }

        private static bool Available(SparkElectricalComponent component) =>
            component != null && component.isActiveAndEnabled && component.ElectricalEnabled;

        private static bool IsFinite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);

        private string CheckWiring()
        {
            if (source == null || transmitter == null || cable == null || receiver == null || lineContact == null)
                return "The loop prefab is missing a component.";

            circuit.CopyConnections(connections);
            var parents = new Dictionary<SparkTerminal, SparkTerminal>();
            SparkTerminal Root(SparkTerminal terminal)
            {
                while (parents.TryGetValue(terminal, out var parent)) terminal = parent;
                return terminal;
            }
            foreach (var wire in connections)
            {
                if (wire.Kind != SparkConnectionKind.Wire || wire.A == null || wire.B == null) continue;
                var a = Root(wire.A);
                var b = Root(wire.B);
                if (a != b) parents[b] = a;
            }
            var nets = new[]
            {
                (source.PositiveTerminal, transmitter.TerminalA),
                (transmitter.TerminalB, cable.TerminalA),
                (cable.TerminalB, lineContact.InputTerminal),
                (lineContact.OutputTerminal, receiver.TerminalA),
                (receiver.TerminalB, source.NegativeTerminal)
            };
            var used = new Dictionary<SparkTerminal, string>();
            foreach (var (a,b) in nets)
            {
                if (a == null || b == null) return "A required loop terminal is missing.";
                var root = Root(a);
                if (root != Root(b)) return $"Connect {a.name} to {b.name}.";
                if (used.TryGetValue(root,out string other)) return $"Separate {a.name} from {other}.";
                used.Add(root,a.name);
            }
            return null;
        }
    }
}
