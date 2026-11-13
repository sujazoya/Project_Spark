using System;
using System.Collections.Generic;
using ProjectSpark.Circuit;
using ProjectSpark.Electrical;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.Gameplay
{
    [DisallowMultipleComponent]
    public sealed class SparkLevel41AnalogOutput : MonoBehaviour
    {
        public static readonly string[] Gates =
        {
            "L41_Zero", "L41_Lsb", "L41_Weights",
            "L41_RequestedVoltage", "L41_OpenTermination", "L41_RepairedSweep"
        };

        // Driver arrays use bit number as the index, starting with the LSB.
        public SparkLevelLayout layout;
        public SparkPowerSupply source;
        public SparkSwitch[] highContacts, lowContacts;
        public SparkTerminal[] bitOutputs;
        public SparkResistor[] bitResistors;
        public SparkResistor link21, link10, termination;
        public SparkSwitch terminationContact;
        public Text statusText, meterText, sweepText, linkLabel;
        public Text[] bitLabels;
        public Button[] bitButtons;
        public Button applyButton, linkButton, diagnoseOpenButton, diagnoseBitButton, sweepButton, restartButton;
        public GameObject healthyLamp, faultLamp;
        public GameObject referenceLamp;
        public GameObject[] bitLamps;
        public Transform[] bitRockers;
        public Renderer[] voltageBars;
        public TextMesh voltageReadout, codeReadout, linkReadout;

        private SparkLevelLayoutLoader loader;
        private LevelGamePlayManager manager;
        private SparkCircuitSystem circuit;
        private SparkElectricalSolver solver;
        private SparkLevelSceneBindings bindings;
        private SparkElectricalSolveResult measuredResult;
        private readonly List<SparkCircuitConnection> connections = new List<SparkCircuitConnection>();
        private readonly float[] sweepVoltages = new float[8];
        private int topology = -1;
        private int stage, selectedCode, appliedCode, sweepCount;
        private bool ready, applyPending, middleChecked, msbChecked, healthyFullScaleSeen, sweeping;
        private float rail, output, sweepElapsed;
        private string wiringIssue, feedback;

        public void ToggleBit0() => ToggleBit(0);
        public void ToggleBit1() => ToggleBit(1);
        public void ToggleBit2() => ToggleBit(2);

        private void ToggleBit(int bit)
        {
            if (!CanControl() || !CanSelectCode()) return;
            selectedCode ^= 1 << bit;
            feedback = null;
        }

        public void ApplyCode()
        {
            if (!CanControl() || !CanSelectCode() || applyPending) return;
            applyPending = true;
            feedback = null;
            DriveCode(selectedCode);
        }

        public void ToggleTermination()
        {
            if (!CanControl()) return;
            if (stage == 4 && healthyFullScaleSeen && terminationContact.IsClosed)
                SetContact(terminationContact,false);
            else if (stage == 5 && !terminationContact.IsClosed)
                SetContact(terminationContact,true);
        }

        public void DiagnoseOpen()
        {
            if (CanControl() && CanDiagnose()) Award();
        }

        public void DiagnoseBit()
        {
            if (CanControl() && CanDiagnose())
                feedback = "The input is still 111. Check which connection changed before the output rose.";
        }

        public void RunSweep()
        {
            if (!CanControl() || !CanSweep()) return;
            sweeping = true;
            sweepCount = 0;
            sweepElapsed = 0f;
            feedback = null;
            selectedCode = 0;
            DriveCode(0);
        }

        public void Restart()
        {
            if (CanControl() && !sweeping && stage < Gates.Length) ResetTrial();
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

        private bool CanSelectCode() => ready && stage >= 1 && stage <= 4 && !healthyFullScaleSeen;
        private bool CanDiagnose() => ready && stage == 4 && healthyFullScaleSeen &&
            !terminationContact.IsClosed && Mathf.Abs(output - rail) <= .05f;
        private bool CanSweep() => ready && stage == 5 && !sweeping && terminationContact.IsClosed &&
            appliedCode == 7 && MatchesCode(7);

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
            if (!HasComponents())
            {
                Hold("Check the ladder and bit driver components.");
                return;
            }
            if (bindings != loader.ActiveBindings || topology != circuit.TopologyVersion)
            {
                ResetTrial();
                bindings = loader.ActiveBindings;
                topology = circuit.TopologyVersion;
                wiringIssue = CheckWiring();
            }
            if (bindings == null || Array.Exists(Gates,gate => !bindings.IsComponentIdDeclared(gate)))
            {
                Hold("Check the six Level 41 objective bindings.");
                return;
            }
            if (wiringIssue != null)
            {
                Hold(wiringIssue);
                return;
            }
            if (solver.IsSolving) return;
            if ((solver.IsDirty || solver.LatestSolveResult == null) && !solver.SolveNow())
            {
                Hold("The solver could not evaluate the ladder.");
                return;
            }
            var result = solver.LatestSolveResult;
            if (result == null || solver.IsDirty) return;
            rail = source.ElectricalState.Voltage;
            if (!source.IsOutputActive || source.IsOverloaded || !(rail >= 4.8f && rail <= 5.2f) ||
                !result.TryGetVoltage(bitResistors[2].TerminalB,source.NegativeTerminal,out output) ||
                float.IsNaN(output) || float.IsInfinity(output))
            {
                Hold("Check the 5 V reference and ladder output.");
                return;
            }
            ready = true;
            measuredResult = result;
            if (stage == 0 && MatchesCode(0)) Award();
            else if (applyPending) CheckAppliedCode();
            if (sweeping) AdvanceSweep(elapsed);
            Show(feedback ?? Prompt());
        }

        private bool MatchesCode(int code) => Mathf.Abs(output - rail * code / 8f) <= .035f;

        private void CheckAppliedCode()
        {
            applyPending = false;
            if (!MatchesCode(appliedCode))
            {
                feedback = "The output is outside the expected range. Check the ladder wiring.";
                return;
            }
            switch (stage)
            {
                case 1:
                    if (appliedCode == 1) Award();
                    else feedback = "Measure the smallest nonzero step with only B0 set.";
                    break;
                case 2:
                    if (appliedCode == 2) middleChecked = true;
                    if (appliedCode == 4) msbChecked = true;
                    if (middleChecked && msbChecked) Award();
                    else if (appliedCode != 2 && appliedCode != 4) feedback = "Test 010 and 100 individually.";
                    break;
                case 3:
                    if (appliedCode == 5) Award();
                    else feedback = "Compare the measured output with the requested 3.125 V.";
                    break;
                case 4:
                    healthyFullScaleSeen = appliedCode == 7;
                    if (!healthyFullScaleSeen) feedback = "Apply 111 to measure full scale before opening the link.";
                    break;
            }
        }

        private void AdvanceSweep(float elapsed)
        {
            sweepElapsed += elapsed;
            if (sweepElapsed < .22f) return;
            float step = rail / 8f;
            if (!MatchesCode(appliedCode) ||
                sweepCount > 0 && Mathf.Abs(output - sweepVoltages[sweepCount-1] - step) > .045f)
            {
                sweeping = false;
                selectedCode = 7;
                DriveCode(7);
                feedback = "The sweep found an uneven step. Check the termination and run the sweep again.";
                return;
            }
            sweepVoltages[sweepCount++] = output;
            sweepElapsed = 0f;
            if (sweepCount == sweepVoltages.Length)
            {
                sweeping = false;
                Award();
                return;
            }
            selectedCode = sweepCount;
            DriveCode(sweepCount);
        }

        private void DriveCode(int code)
        {
            appliedCode = code;
            for (int bit = 0; bit < 3; bit++)
            {
                bool high = (code & 1 << bit) != 0;
                // Open the outgoing throw before closing the new throw.
                SetContact(high ? lowContacts[bit] : highContacts[bit],false);
                SetContact(high ? highContacts[bit] : lowContacts[bit],true);
            }
        }

        private static void SetContact(SparkSwitch contact,bool closed)
        {
            if (contact != null && contact.IsClosed != closed)
                contact.SetState(closed ? SparkSwitchState.Closed : SparkSwitchState.Open);
        }

        private string Prompt() => stage switch
        {
            0 => "Wire the ladder. The released code 000 should read 0.000 V.",
            1 => "Apply 001 to measure one step.",
            2 => middleChecked ? "Middle weight verified. Apply 100."
                : msbChecked ? "MSB weight verified. Apply 010." : "Apply 010 and 100 to compare their weights.",
            3 => "Set the output to 3.125 V. Select the bits, then press APPLY CODE.",
            4 => !healthyFullScaleSeen ? "Apply 111 to record healthy full scale."
                : terminationContact.IsClosed ? "Healthy full scale recorded. Press OPEN LINK."
                : "The code stayed at 111. Diagnose why the output rose toward 5 V.",
            5 => sweeping ? "Checking each code and each voltage step."
                : terminationContact.IsClosed ? "The link is restored. Press RUN SWEEP."
                : "Open termination confirmed. Restore the link, then run the sweep.",
            _ => manager != null && manager.SceneBindings == bindings && manager.IsCompleted
                ? "Eight output levels verified. The ladder is restored."
                : "Six checks recorded. Waiting for level completion."
        };

        private void Show(string message)
        {
            if (statusText != null) statusText.text = message;
            UpdateWorkbenchDisplays();
            if (meterText != null)
                meterText.text = ready
                    ? $"SELECTED {Bits(selectedCode)}    |    APPLIED {Bits(appliedCode)}    |    OUTPUT {output:0.000} V    |    REF {rail:0.00} V"
                    : "SELECTED 000    |    APPLIED —    |    OUTPUT —    |    REF —";
            if (sweepText != null)
                sweepText.text = sweepCount > 0 ? $"SWEEP {sweepCount}/8    |    LAST VERIFIED {sweepVoltages[sweepCount-1]:0.000} V"
                    : "B2 is the most significant bit. B0 is the least significant bit.";
            if (bitLabels != null)
                for (int bit=0;bit<bitLabels.Length;bit++)
                    if (bitLabels[bit] != null) bitLabels[bit].text = $"B{bit}  {(selectedCode >> bit & 1)}";
            if (bitButtons != null)
                foreach (var button in bitButtons) SetButton(button,CanSelectCode());
            if (linkLabel != null) linkLabel.text = stage >= 5 ? "RESTORE LINK" : "OPEN LINK";
            SetButton(applyButton,CanSelectCode() && !applyPending);
            SetButton(linkButton,ready && (stage == 4 && healthyFullScaleSeen && terminationContact.IsClosed ||
                stage == 5 && !terminationContact.IsClosed));
            SetButton(diagnoseOpenButton,CanDiagnose());
            SetButton(diagnoseBitButton,CanDiagnose());
            SetButton(sweepButton,CanSweep());
            SetButton(restartButton,ready && stage > 0 && stage < Gates.Length && !sweeping);
            if (healthyLamp != null) healthyLamp.SetActive(ready && terminationContact.IsClosed);
            if (faultLamp != null) faultLamp.SetActive(ready && !terminationContact.IsClosed);
        }

        private void UpdateWorkbenchDisplays()
        {
            if (referenceLamp != null) referenceLamp.SetActive(source != null && source.IsOutputActive);
            bool open = ready && !terminationContact.IsClosed;
            if (voltageReadout != null)
            {
                voltageReadout.text = ready ? $"{output:0.000} V" : "— V";
                voltageReadout.color = open ? new Color(1f, .68f, .26f) : new Color(.47f, 1f, .76f);
            }
            if (codeReadout != null)
                codeReadout.text = !ready ? "CODE —" : sweeping || sweepCount == 8
                    ? $"CODE {Bits(appliedCode)}  /  SWEEP {sweepCount}/8" : $"CODE {Bits(appliedCode)}";
            if (linkReadout != null)
            {
                linkReadout.text = !ready ? "CHECK WIRING" : open ? "LINK OPEN" : "LINK CLOSED";
                linkReadout.color = open ? new Color(1f, .68f, .26f) : new Color(.52f, .92f, 1f);
            }
            if (bitRockers != null && bitLamps != null)
                for (int bit = 0; bit < bitRockers.Length && bit < bitLamps.Length; bit++)
                {
                    bool high = (appliedCode & (1 << bit)) != 0;
                    if (bitRockers[bit] != null) bitRockers[bit].localRotation = Quaternion.Euler(0, 0, high ? -12f : 12f);
                    if (bitLamps[bit] != null) bitLamps[bit].SetActive(ready && high);
                }
            if (voltageBars != null)
            {
                int lit = ready && rail > 0f ? Mathf.RoundToInt(Mathf.Clamp01(output / rail) * voltageBars.Length) : 0;
                for (int i = 0; i < voltageBars.Length; i++)
                    if (voltageBars[i] != null) voltageBars[i].enabled = i < lit;
            }
        }

        private static string Bits(int code) => Convert.ToString(code,2).PadLeft(3,'0');
        private static void SetButton(Button button,bool enabled) { if (button != null) button.interactable = enabled; }

        private void Award()
        {
            if (!bindings.SetRuntimeComponentBinding(Gates[stage],source))
            {
                feedback = "Could not record the Level 41 objective.";
                return;
            }
            stage++;
            feedback = null;
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
            stage = selectedCode = appliedCode = sweepCount = 0;
            ready = applyPending = middleChecked = msbChecked = healthyFullScaleSeen = sweeping = false;
            feedback = null;
            measuredResult = null;
            if (HasComponents()) DriveCode(0);
            SetContact(terminationContact,true);
        }

        private void Hold(string message)
        {
            ready = false;
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

        private bool HasComponents() =>
            Available(source) && Available(terminationContact) &&
            highContacts != null && highContacts.Length == 3 && Array.TrueForAll(highContacts,contact => Available(contact)) &&
            lowContacts != null && lowContacts.Length == 3 && Array.TrueForAll(lowContacts,contact => Available(contact)) &&
            bitOutputs != null && bitOutputs.Length == 3 && Array.TrueForAll(bitOutputs,pin => pin != null) &&
            bitResistors != null && bitResistors.Length == 3 && Array.TrueForAll(bitResistors,Healthy) &&
            Healthy(link21) && Healthy(link10) && Healthy(termination);

        private static bool Available(SparkElectricalComponent component) =>
            component != null && component.isActiveAndEnabled && component.ElectricalEnabled;
        private static bool Healthy(SparkResistor resistor) => Available(resistor) && !resistor.IsBurned;

        private string CheckWiring()
        {
            circuit.CopyConnections(connections);
            var parents = new Dictionary<SparkTerminal,SparkTerminal>();
            SparkTerminal Root(SparkTerminal pin)
            {
                while (parents.TryGetValue(pin,out var parent)) pin = parent;
                return pin;
            }
            foreach (var wire in connections)
            {
                if (wire.Kind != SparkConnectionKind.Wire || wire.A == null || wire.B == null) continue;
                var a=Root(wire.A);
                var b=Root(wire.B);
                if (a != b) parents[b]=a;
            }
            var nets = new[]
            {
                new[] { bitOutputs[2], bitResistors[2].TerminalA },
                new[] { bitOutputs[1], bitResistors[1].TerminalA },
                new[] { bitOutputs[0], bitResistors[0].TerminalA },
                new[] { bitResistors[2].TerminalB, link21.TerminalA },
                new[] { link21.TerminalB, bitResistors[1].TerminalB, link10.TerminalA },
                new[] { link10.TerminalB, bitResistors[0].TerminalB, termination.TerminalA },
                new[] { terminationContact.OutputTerminal, source.NegativeTerminal }
            };
            var used = new Dictionary<SparkTerminal,string>();
            if (source.PositiveTerminal == null) return "A reference terminal is missing.";
            used.Add(Root(source.PositiveTerminal),source.PositiveTerminal.name);
            foreach (var net in nets)
            {
                if (net[0] == null) return "A ladder terminal is missing.";
                var root=Root(net[0]);
                if (used.TryGetValue(root,out string other)) return $"Separate {net[0].name} from {other}.";
                used.Add(root,net[0].name);
                for (int i=1;i<net.Length;i++)
                {
                    if (net[i] == null) return "A ladder terminal is missing.";
                    if (Root(net[i]) != root) return $"Connect {net[0].name} to {net[i].name}.";
                }
            }
            return null;
        }
    }
}
