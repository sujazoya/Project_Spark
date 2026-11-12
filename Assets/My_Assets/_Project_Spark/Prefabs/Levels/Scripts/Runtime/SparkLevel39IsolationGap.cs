using System;
using System.Collections.Generic;
using ProjectSpark.Circuit;
using ProjectSpark.Electrical;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.Gameplay
{
    [DisallowMultipleComponent]
    public sealed class SparkLevel39IsolationGap : MonoBehaviour
    {
        public static readonly string[] Gates =
        {
            "L39_Wired", "L39_Send", "L39_Release",
            "L39_IndependentPower", "L39_BridgeDetected", "L39_IsolationRestored"
        };

        private const float TransferCurrent = .004f;
        private const float OffCurrent = .0005f;
        private const float OutputLow = .15f;
        private const float OutputHigh = 4.7f;

        public SparkLevelLayout layout;
        public SparkPowerSupply transmitterSupply, receiverSupply;
        public SparkSwitch sendContact, receiverContact;
        public SparkResistor transmitterResistor, pullUp;
        public SparkLED transmitterLed;
        public GameObject transmitterGlow, receiverGlow, isolationGlow, bridgeGlow;
        public GameObject opticalBeam, txSupplyLight, rxSupplyLight, sendLight;
        public Transform sendRocker;
        public Quaternion sendRockerRest = Quaternion.identity;
        public TextMesh isolationReadout, txReadout, rxReadout, monitorReadout;
        public Text statusText, meterText;
        public Button sendButton, receiverPowerButton, restartButton;
        public Text sendLabel, receiverPowerLabel;

        private SparkLevelLayoutLoader loader;
        private LevelGamePlayManager manager;
        private SparkCircuitSystem circuit;
        private SparkElectricalSolver solver;
        private SparkLevelSceneBindings bindings;
        private SparkElectricalSolveResult measuredResult;
        private readonly List<SparkCircuitConnection> wires = new List<SparkCircuitConnection>();
        private string wiringIssue, feedback;
        private int topology = -1;
        private int stage;
        private bool ready, bridged, groundJumper, receiverOffObserved, restoredOff, ledOn, receiving;
        private float transmitterCurrent, receiverOutput;

        public void ToggleSend()
        {
            if (CanControl() && CanToggleSend())
                SetContact(sendContact, !sendContact.IsClosed);
        }

        public void ToggleReceiverPower()
        {
            if (!CanControl() || !CanToggleReceiverPower()) return;
            receiverSupply.SetOutput(!receiverSupply.IsOutputActive, default);
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
                RefreshReadings();
            return ready && !solver.IsDirty && ReferenceEquals(solver.LatestSolveResult, measuredResult);
        }

        private bool CanToggleSend() => ready && !bridged &&
            (stage == 1 && !sendContact.IsClosed ||
             stage == 2 && sendContact.IsClosed ||
             stage == 3 && !sendContact.IsClosed && !receiverOffObserved ||
             stage == 5 && (restoredOff ? !sendContact.IsClosed : sendContact.IsClosed));

        private bool CanToggleReceiverPower() => ready && !bridged && stage == 3 && sendContact.IsClosed && ledOn &&
            (receiverSupply.IsOutputActive ? !receiverOffObserved && receiving : receiverOffObserved);

        private void Update() => RefreshReadings();

        private void RefreshReadings()
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
            if (bindings != loader.ActiveBindings)
            {
                ResetTrial();
                bindings = loader.ActiveBindings;
                topology = -1;
            }
            if (bindings == null || Array.Exists(Gates, gate => !bindings.IsComponentIdDeclared(gate)))
            {
                Hold("Check the six Level 39 objective bindings.");
                return;
            }
            if (!Available(transmitterSupply) || !Available(receiverSupply) || !Available(sendContact) ||
                !Available(receiverContact) || !Available(transmitterResistor) || !Available(pullUp) ||
                !Available(transmitterLed) || transmitterResistor.IsBurned || pullUp.IsBurned)
            {
                Hold("Check that the bench components are active and healthy.");
                return;
            }
            if (topology != circuit.TopologyVersion)
            {
                topology = circuit.TopologyVersion;
                wiringIssue = CheckWiring();
            }
            if (wiringIssue != null)
            {
                Hold(wiringIssue);
                return;
            }
            if (solver.IsSolving) return;
            if ((solver.IsDirty || solver.LatestSolveResult == null) && !solver.SolveNow())
            {
                Hold("The solver could not evaluate the circuit.");
                return;
            }
            var result = solver.LatestSolveResult;
            if (result == null || solver.IsDirty) return;
            if (!transmitterSupply.IsOutputActive || transmitterSupply.IsOverloaded ||
                !IsFiveVolts(transmitterSupply.ElectricalState.Voltage) || receiverSupply.IsOverloaded ||
                transmitterLed.IsOverCurrent || transmitterLed.IsReverseVoltageExceeded)
            {
                Hold("Check supply voltage and transmitter LED polarity.");
                return;
            }
            transmitterCurrent = Mathf.Abs(transmitterLed.ElectricalState.Current);
            ledOn = transmitterLed.IsOn && transmitterCurrent >= TransferCurrent;
            if (SetContact(receiverContact, ledOn)) return;

            bool powered = receiverSupply.IsOutputActive;
            if (powered && (!IsFiveVolts(receiverSupply.ElectricalState.Voltage) ||
                !result.TryGetVoltage(receiverContact.InputTerminal, receiverSupply.NegativeTerminal, out receiverOutput) ||
                float.IsNaN(receiverOutput) || float.IsInfinity(receiverOutput)))
            {
                Hold("Check the receiver supply and collector connection.");
                return;
            }
            float receiverCurrent = Mathf.Abs(pullUp.ElectricalState.Current);
            receiving = powered && receiverContact.IsClosed && Mathf.Abs(receiverOutput) <= OutputLow && receiverCurrent > .003f;
            bool idle = powered && !receiverContact.IsClosed && receiverOutput >= OutputHigh && receiverCurrent < OffCurrent;
            bool transmitterOff = !sendContact.IsClosed && transmitterCurrent < OffCurrent;
            ready = true;
            measuredResult = result;
            EvaluateObjectives(powered, receiverCurrent, idle, transmitterOff);
            Show(feedback ?? Prompt());
        }

        private void EvaluateObjectives(bool powered, float receiverCurrent, bool idle, bool transmitterOff)
        {
            if (bridged)
            {
                if (stage == 4 && groundJumper && ledOn && receiving) Award();
                return;
            }
            switch (stage)
            {
                case 0:
                case 2:
                    if (transmitterOff && idle) Award();
                    break;
                case 1:
                    if (sendContact.IsClosed && ledOn && receiving) Award();
                    break;
                case 3:
                    if (ledOn && !powered && receiverCurrent < OffCurrent) receiverOffObserved = true;
                    if (receiverOffObserved && ledOn && receiving) Award();
                    break;
                case 5:
                    if (transmitterOff && idle) restoredOff = true;
                    if (restoredOff && sendContact.IsClosed && ledOn && receiving) Award();
                    break;
            }
        }

        private string Prompt()
        {
            if (bridged && stage != 5) return "BRIDGED. Remove the wire between TX RETURN and RX RETURN.";
            return stage switch
            {
                0 => "Wire both circuits and verify the receiver idles HIGH.",
                1 => "Receiver idle HIGH. Turn SEND on.",
                2 => "Receiver LOW. Turn SEND off to verify the pull-up.",
                3 => !sendContact.IsClosed ? "Turn SEND on again for the power independence test."
                    : receiverOffObserved ? "TX is still sending. Restore RX POWER."
                    : "Turn RX POWER off while SEND stays on.",
                4 => "Add a temporary wire from TX RETURN to RX RETURN.",
                5 => bridged ? "Bridge detected while RX still works. Remove the return jumper."
                    : restoredOff ? "Isolation restored and RX HIGH. Turn SEND on to verify LOW."
                    : "Isolation restored. Turn SEND off to verify HIGH.",
                _ => manager != null && manager.SceneBindings == bindings && manager.IsCompleted
                    ? "Isolation and both receiver states verified."
                    : "Six checks recorded. Waiting for level completion."
            };
        }

        private void Show(string message)
        {
            if (statusText != null) statusText.text = message;
            bool txPowered = transmitterSupply != null && transmitterSupply.IsOutputActive;
            bool rxPowered = receiverSupply != null && receiverSupply.IsOutputActive;
            bool sending = sendContact != null && sendContact.IsClosed;
            string isolation = bridged ? "BRIDGED" : ready ? "ISOLATED" : "CHECK WIRING";
            string tx = ready ? $"{transmitterCurrent * 1000f:0.00} mA" : "—";
            string rx = !ready ? "—" : rxPowered ? $"{receiverOutput:0.00} V" : "UNPOWERED";
            if (meterText != null) meterText.text = $"TX LED {tx}    |    RX OUT {rx}    |    {isolation}";
            if (sendLabel != null) sendLabel.text = sending ? "SEND OFF" : "SEND ON";
            if (receiverPowerLabel != null) receiverPowerLabel.text = rxPowered ? "RX POWER OFF" : "RX POWER ON";
            if (isolationReadout != null)
            {
                isolationReadout.text = isolation;
                isolationReadout.color = bridged ? new Color(1f, .37f, .39f) : new Color(.53f, .94f, 1f);
            }
            if (txReadout != null) txReadout.text = $"TX LED {tx}";
            if (rxReadout != null) rxReadout.text = $"RX OUT {rx}";
            if (monitorReadout != null)
                monitorReadout.text = !ready ? "—" : !rxPowered ? "OFF" : receiving ? "LOW" :
                    receiverOutput >= OutputHigh ? "HIGH" : "CHECK";
            if (sendRocker != null)
                sendRocker.localRotation = sendRockerRest * Quaternion.Euler(sending ? 12f : -12f, 0, 0);
            SetVisual(transmitterGlow, ready && ledOn);
            SetVisual(receiverGlow, ready && receiving);
            SetVisual(isolationGlow, ready && !bridged);
            SetVisual(bridgeGlow, bridged);
            SetVisual(opticalBeam, ready && ledOn);
            SetVisual(txSupplyLight, txPowered);
            SetVisual(rxSupplyLight, rxPowered);
            SetVisual(sendLight, sending);
            if (sendButton != null) sendButton.interactable = CanToggleSend();
            if (receiverPowerButton != null) receiverPowerButton.interactable = CanToggleReceiverPower();
            if (restartButton != null) restartButton.interactable = ready && stage > 0 && stage < Gates.Length;
        }

        private static void SetVisual(GameObject visual, bool on)
        {
            if (visual != null && visual.activeSelf != on) visual.SetActive(on);
        }

        private void Award()
        {
            if (!bindings.SetRuntimeComponentBinding(Gates[stage], transmitterSupply))
            {
                feedback = "Could not record the Level 39 objective.";
                return;
            }
            stage++;
            if (manager != null && manager.SceneBindings == bindings) manager.QueueEvaluation();
        }

        private void ResetTrial()
        {
            ClearGates();
            stage = 0;
            ready = receiverOffObserved = restoredOff = ledOn = receiving = false;
            feedback = null;
            measuredResult = null;
            SetContact(sendContact, false);
            SetContact(receiverContact, false);
            if (receiverSupply != null && !receiverSupply.IsOutputActive)
                receiverSupply.SetOutput(true, default);
        }

        private void ClearGates()
        {
            if (bindings != null)
                foreach (string gate in Gates) bindings.ClearRuntimeComponentBinding(gate);
            if (stage > 0 && manager != null && manager.SceneBindings == bindings) manager.QueueEvaluation();
        }

        private void Hold(string message)
        {
            ready = false;
            measuredResult = null;
            SetContact(receiverContact, false);
            Show(message);
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

        private static bool Available(SparkElectricalComponent component) =>
            component != null && component.isActiveAndEnabled && component.ElectricalEnabled;

        private static bool IsFiveVolts(float voltage) => voltage >= 4.8f && voltage <= 5.2f;

        private string CheckWiring()
        {
            bridged = groundJumper = false;
            circuit.CopyConnections(wires);
            var parents = new Dictionary<SparkTerminal, SparkTerminal>();
            SparkTerminal Root(SparkTerminal terminal)
            {
                while (parents.TryGetValue(terminal, out var parent)) terminal = parent;
                return terminal;
            }
            SparkTerminal txReturn = transmitterSupply.NegativeTerminal;
            SparkTerminal rxReturn = receiverSupply.NegativeTerminal;
            foreach (var wire in wires)
            {
                if (wire.Kind != SparkConnectionKind.Wire || wire.A == null || wire.B == null) continue;
                if (wire.A == txReturn && wire.B == rxReturn || wire.A == rxReturn && wire.B == txReturn)
                    groundJumper = true;
                SparkTerminal a = Root(wire.A);
                SparkTerminal b = Root(wire.B);
                if (a != b) parents[b] = a;
            }

            var nets = new[]
            {
                (transmitterSupply.PositiveTerminal, sendContact.InputTerminal),
                (sendContact.OutputTerminal, transmitterResistor.TerminalA),
                (transmitterResistor.TerminalB, transmitterLed.AnodeTerminal),
                (transmitterLed.CathodeTerminal, txReturn),
                (receiverSupply.PositiveTerminal, pullUp.TerminalA),
                (pullUp.TerminalB, receiverContact.InputTerminal),
                (receiverContact.OutputTerminal, rxReturn)
            };
            var roots = new SparkTerminal[nets.Length];
            for (int i = 0; i < nets.Length; i++)
            {
                var (a, b) = nets[i];
                if (a == null || b == null) return "A required terminal is missing.";
                roots[i] = Root(a);
            }
            bool unexpectedBridge = false;
            for (int tx = 0; tx < 4; tx++)
            for (int rx = 4; rx < nets.Length; rx++)
            {
                // Inspect both endpoints so an incomplete circuit cannot hide a bridge.
                var (txA, txB) = nets[tx];
                var (rxA, rxB) = nets[rx];
                bool joined = Root(txA) == Root(rxA) || Root(txA) == Root(rxB) ||
                    Root(txB) == Root(rxA) || Root(txB) == Root(rxB);
                if (!joined) continue;
                bridged = true;
                if (tx != 3 || rx != 6 || !groundJumper) unexpectedBridge = true;
            }
            if (unexpectedBridge) return "Remove the unexpected wire across the isolation gap.";
            for (int i = 0; i < nets.Length; i++)
            {
                var (a, b) = nets[i];
                if (roots[i] != Root(b)) return $"Connect {a.name} to {b.name}.";
                for (int j = 0; j < i; j++)
                {
                    if (roots[i] != roots[j] || groundJumper && i == 6 && j == 3) continue;
                    return $"Separate {a.name} from {nets[j].Item1.name}.";
                }
            }
            return null;
        }
    }
}
