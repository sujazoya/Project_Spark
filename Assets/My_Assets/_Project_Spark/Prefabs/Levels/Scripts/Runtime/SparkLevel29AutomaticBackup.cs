using System;
using System.Collections.Generic;
using UnityEngine;
using ProjectSpark.Electrical;
using ProjectSpark.Circuit;

namespace ProjectSpark.Gameplay
{
    /// <summary>
    /// Level 29 automatic emergency backup observer.
    /// Records a real charge -> isolation -> sustained backup -> restore -> recharge sequence.
    /// Uses the existing solver, circuit graph, and six LED gate aliases. It never fabricates
    /// electrical state, alters topology, or completes the level directly.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkLevel29AutomaticBackup : MonoBehaviour
    {
        public static readonly string[] Gates =
        {
            "L29_Wiring", "L29_Charged", "L29_Isolated",
            "L29_Backup", "L29_Restored", "L29_Recharged"
        };

        [Header("Services")]
        [SerializeField] private SparkElectricalSolver solver;
        [SerializeField] private SparkLevelLayoutLoader loader;
        [SerializeField] private SparkCircuitSystem circuit;

        [Header("Level Requirements")]
        [SerializeField, Min(0.1f)] private float backupSeconds = 2f;
        [SerializeField] private float chargedVoltage = 4.1f;
        [SerializeField, Min(0f)] private float minimumBackupCurrent = 0.002f;
        [SerializeField, Min(0f)] private float maximumLedCurrent = 0.009f;
        [SerializeField, Min(0f)] private float isolationCurrent = 0.0001f;
        [SerializeField, Min(0f)] private float requiredBackupVoltageDrop = 0.20f;
        [SerializeField, Min(0f)] private float requiredRechargeRise = 0.15f;
        [SerializeField, Min(0f)] private float recoveryHoldSeconds = 0.50f;

        [Header("Diagnostics")]
        [SerializeField] private bool showStatus = true;
        [SerializeField] private bool logStageChanges = true;
        [SerializeField] private bool logHeartbeat = true;
        [SerializeField, Min(0.5f)] private float heartbeatSeconds = 2f;
        [SerializeField, TextArea(2, 4)]
        private string status = "Waiting for active Level 29.";

        public string Status => status;
        public int Stage => stage;

        private SparkLevelLayout layout;
        private SparkLevelSceneBindings bindings;
        private SparkSwitch main;
        private SparkCapacitor cap;
        private SparkResistor charge;
        private SparkResistor load;
        private SparkDiode dMain;
        private SparkDiode dCharge;
        private SparkDiode dBackup;
        private SparkLED led;
        private SparkElectricalComponent source;
        private SparkTerminal positive;
        private SparkTerminal negative;
        private SparkTerminal chargeInput, chargeOutput, loadInput, loadOutput;
        private SparkElectricalComponent[] watched;
        private readonly bool[] passed = new bool[6];

        // Stage: 0 waiting for circuit, 1 charging, 2 waiting for off,
        // 3 qualifying backup, 4 waiting for restore, 5 recharge, 6 complete.
        private int stage;
        private int topology = -1;
        private float baseline = float.NaN;
        private float backupStart;
        private float elapsed;
        private float restoreStart;
        private float nextSolve;
        private float nextHeartbeat;
        private bool backupIntervalActive;
        private bool subscribed;
        private int solveEventCount;
        private int lastLoggedStage = -1;
        private string lastLoggedProblem;

        private void OnEnable()
        {
            layout = GetComponentInParent<SparkLevelLayout>();
            ResolveServices();
            ResetAttempt("Build automatic backup circuit. MAIN OFF while wiring.");
        }

        private void ResolveServices()
        {
            if (solver == null) solver = FindFirstObjectByType<SparkElectricalSolver>();
            if (loader == null) loader = FindFirstObjectByType<SparkLevelLayoutLoader>();
            if (circuit == null) circuit = FindFirstObjectByType<SparkCircuitSystem>();
            if (!subscribed && solver != null)
            {
                solver.SolveCompleted += OnSolved;
                solver.SolveFailed += OnFailed;
                subscribed = true;
            }
        }

        private bool Active =>
            layout != null && loader != null &&
            loader.ActiveLayout == layout &&
            layout.LevelDefinition != null &&
            layout.LevelDefinition.LevelId == "level_29";

        private void LateUpdate()
        {
            ResolveServices();

            // Heartbeat is deliberately OUTSIDE OnSolved and before every guard:
            // it reports missing observer references even when no solve occurs.
            if (logHeartbeat && Time.unscaledTime >= nextHeartbeat)
            {
                nextHeartbeat = Time.unscaledTime + heartbeatSeconds;
                Debug.Log(
                    "[LEVEL 29 DIAGNOSTIC] " +
                    "Active=" + Active +
                    " | Solver=" + (solver != null ? solver.name : "NULL") +
                    " | Subscribed=" + subscribed +
                    " | SolveEvents=" + solveEventCount +
                    " | Bindings=" + (bindings != null ? bindings.name : "NULL") +
                    " | Stage=" + stage +
                    " | Gates=" + string.Join(",", passed) +
                    " | Status=" + status, this);
            }

            if (!Active || solver == null || !solver.isActiveAndEnabled || circuit == null)
                return;

            SparkLevelSceneBindings activeBindings = loader.ActiveBindings;
            if (bindings != activeBindings)
            {
                ClearGates();
                bindings = activeBindings;
                Unwatch();
                topology = -1;
                ResetAttempt("Level 29 runtime bindings changed. Resolve installed parts.");
            }

            if (Time.timeScale <= 0f || Time.time < nextSolve || solver.IsSolving)
                return;

            float dt = solver.LatestSolveResult != null
                ? solver.LatestSolveResult.TimeStep : 1f / 60f;
            nextSolve = Time.time + Mathf.Clamp(dt, 0.001f, 0.1f);
            solver.MarkDirty();
            solver.SolveNow();
        }

        private void OnDisable()
        {
            if (subscribed && solver != null)
            {
                solver.SolveCompleted -= OnSolved;
                solver.SolveFailed -= OnFailed;
            }
            subscribed = false;
            Unwatch();
            ClearGates();
            bindings = null;
        }

        private void ClearGates()
        {
            if (bindings == null) return;
            foreach (string id in Gates)
                bindings.SetRuntimeComponentBinding(id, null);
        }

        private void Publish()
        {
            if (bindings == null) return;
            // Existing level definition uses LEDOn targets. Each completed phase
            // aliases the REAL LED; this does not freeze or fabricate LED state.
            // Consequently the evaluator may temporarily uncheck a historical gate
            // if that actual LED is off. A custom latched target type would remove
            // that separate evaluator limitation.
            for (int i = 0; i < Gates.Length; ++i)
                bindings.SetRuntimeComponentBinding(Gates[i], passed[i] ? led : null);
        }

        private void ResetAttempt(string reason)
        {
            if (stage >= 2)
                Debug.LogWarning("[LEVEL 29 RESET] Stage=" + stage + " | " + reason, this);

            stage = 0;
            baseline = float.NaN;
            elapsed = 0f;
            backupStart = 0f;
            restoreStart = 0f;
            backupIntervalActive = false;
            lastLoggedStage = -1;
            lastLoggedProblem = null;
            Array.Clear(passed, 0, passed.Length);
            ClearGates();
            status = reason;
        }

        private void OnFailed()
        {
            if (Active)
                ResetAttempt("Electrical solve failed. Repair the circuit and restart.");
        }

        private void Unwatch()
        {
            if (watched != null)
            {
                foreach (SparkElectricalComponent part in watched)
                    if (part != null)
                        part.ElectricalConfigurationChanged -= OnConfigurationChanged;
            }
            watched = null;
        }

        private void OnConfigurationChanged()
        {
            if (Active)
                ResetAttempt("Component settings changed. Repeat the sequence.");
        }

        private T Get<T>(string id) where T : SparkElectricalComponent
        {
            return bindings != null && bindings.TryGetComponent(id, out var c)
                ? c as T : null;
        }

        private bool ResolveParts()
        {
            if (bindings == null)
            {
                status = "No active SparkLevelSceneBindings.";
                return false;
            }

            foreach (string gate in Gates)
                if (!bindings.IsComponentIdDeclared(gate))
                {
                    status = "Missing runtime gate ID: " + gate;
                    return false;
                }

            main = Get<SparkSwitch>("Power_Switch");
            cap = Get<SparkCapacitor>("C_STORE_Component");
            charge = Get<SparkResistor>("R_CHARGE_Component");
            load = Get<SparkResistor>("R_LED_Component");
            dMain = Get<SparkDiode>("D_MAIN_Component");
            dCharge = Get<SparkDiode>("D_CHARGE_Component");
            dBackup = Get<SparkDiode>("D_BACKUP_Component");
            led = Get<SparkLED>("led");
            source = Get<SparkElectricalComponent>("PowerSupply_01");

            string[] ids =
            {
                "Power_Switch", "C_STORE_Component", "R_CHARGE_Component",
                "R_LED_Component", "D_MAIN_Component", "D_CHARGE_Component",
                "D_BACKUP_Component", "led", "PowerSupply_01"
            };
            SparkElectricalComponent[] parts =
            {
                main, cap, charge, load, dMain, dCharge, dBackup, led, source
            };

            for (int i = 0; i < parts.Length; i++)
            {
                var part = parts[i];
                if (part == null) { status = "UNBOUND / WRONG TYPE: " + ids[i]; return false; }
                if (!part.isActiveAndEnabled) { status = "INACTIVE COMPONENT: " + ids[i]; return false; }
                if (!part.ElectricalEnabled) { status = "ELECTRICALLY DISABLED: " + ids[i]; return false; }
                if (layout == null || !part.transform.IsChildOf(layout.transform))
                {
                    status = "OUTSIDE ACTIVE LAYOUT: " + ids[i];
                    return false;
                }
            }

            if (!bindings.TryGetTerminal("Battery_positive", out positive) || positive == null)
            {
                status = "Missing terminal: Battery_positive";
                return false;
            }
            if (!bindings.TryGetTerminal("Battery_negative", out negative) || negative == null)
            {
                status = "Missing terminal: Battery_negative";
                return false;
            }

            if (!TryResolveResistorPins("R_CHARGE", charge, out chargeInput, out chargeOutput) ||
                !TryResolveResistorPins("R_LED", load, out loadInput, out loadOutput))
                return false;

            bool changed = watched == null || watched.Length != parts.Length;
            if (!changed)
                for (int i = 0; i < parts.Length; i++)
                    if (watched[i] != parts[i]) { changed = true; break; }

            if (changed)
            {
                Unwatch();
                watched = parts;
                ResetAttempt("Parts resolved. Begin charge test with MAIN ON.");
                // Main switch is deliberately excluded: opening/closing MAIN
                // is a required action, not a configuration fault.
                for (int i = 1; i < watched.Length; i++)
                    watched[i].ElectricalConfigurationChanged += OnConfigurationChanged;
            }
            return true;
        }

        private static bool Finite(float v) => !float.IsNaN(v) && !float.IsInfinity(v);
        private static bool Near(float a, float b, float tolerance) =>
            Mathf.Abs(a - b) <= tolerance;
        private static float I(SparkElectricalComponent part) =>
            Mathf.Abs(part.ElectricalState.Current);

        private bool TryResolveResistorPins(string prefix, SparkResistor resistor,
            out SparkTerminal input, out SparkTerminal output)
        {
            bindings.TryGetTerminal(prefix + "_Input", out input);
            bindings.TryGetTerminal(prefix + "_Output", out output);
            bool valid = input != null && output != null && input != output &&
                ((input == resistor.TerminalA && output == resistor.TerminalB) ||
                 (input == resistor.TerminalB && output == resistor.TerminalA));
            if (!valid)
                status = $"Check {prefix}_Input and {prefix}_Output on the installed resistor.";
            return valid;
        }

        private string ComponentProblem()
        {
            foreach (var part in watched)
            {
                var s = part.ElectricalState;
                if (!Finite(s.Voltage) || !Finite(s.Current) || !Finite(s.Power))
                    return "Non-finite solved state: " + part.name;
            }

            if (charge.IsBurned || charge.IsOverPower)
                return "Slot 03 charging resistor burned or over its power rating.";
            if (load.IsBurned || load.IsOverPower)
                return "Slot 05 LED resistor burned or over its power rating.";
            if (cap.IsOverVoltage)
                return "Slot 04 capacitor voltage rating exceeded.";
            if (led.IsOverCurrent || led.IsReverseVoltageExceeded)
                return "LED overcurrent or reverse-voltage fault.";
            if (dMain.IsReverseFault || dCharge.IsReverseFault || dBackup.IsReverseFault)
                return "Diode reverse fault. Check polarity.";

            if (!Near(charge.ResistanceOhms, 220f, 1f))
                return "Slot 03 requires 220 ohms; found " + charge.ResistanceOhms + ".";
            if (!Near(load.ResistanceOhms, 330f, 1f))
                return "Slot 05 requires 330 ohms; found " + load.ResistanceOhms + ".";
            if (!Near(cap.CapacitanceFarads, 0.01f, 0.00001f))
                return "Slot 04 requires 10000 uF; found " + cap.CapacitanceFarads * 1e6f + " uF.";
            if (cap.VoltageRating < 10f)
                return "Slot 04 requires at least 10 V capacitor rating.";
            if (!Near(dMain.ForwardVoltage, 0.7f, 0.02f))
                return "D_MAIN requires ~0.7 V forward drop.";
            if (!Near(dCharge.ForwardVoltage, 0.7f, 0.02f))
                return "D_CHARGE requires ~0.7 V forward drop.";
            if (!Near(dBackup.ForwardVoltage, 0.7f, 0.02f))
                return "D_BACKUP requires ~0.7 V forward drop.";
            if (!Near(led.ForwardVoltage, 2f, 0.02f))
                return "LED requires ~2.0 V forward drop.";

            float supplyVoltage = Mathf.Abs(source.ElectricalState.Voltage);
            if (supplyVoltage < 4.8f || supplyVoltage > 5.2f)
                return "Supply is " + supplyVoltage.ToString("F2") + " V; expected 4.8-5.2 V.";
            return null;
        }

        private string WiringProblem()
        {
            var graph = solver.CurrentGraph;
            if (graph == null)
                return "No solved circuit graph. Check that the solver is active.";

            // Required groups must share a solver node, and each group must be separate.
            var nets = new (string Name, (string Label, SparkTerminal Terminal)[] Pins)[]
            {
                ("battery +", new[] { ("Battery +", positive), ("MAIN IN", main.InputTerminal) }),
                ("MAIN OUT", new[] { ("MAIN OUT", main.OutputTerminal), ("D_MAIN anode", dMain.AnodeTerminal), ("D_CHARGE anode", dCharge.AnodeTerminal) }),
                ("charge resistor input", new[] { ("D_CHARGE cathode", dCharge.CathodeTerminal), ("220 ohm IN", chargeInput) }),
                ("capacitor +", new[] { ("220 ohm OUT", chargeOutput), ("Capacitor +", cap.PositiveTerminal), ("D_BACKUP anode", dBackup.AnodeTerminal) }),
                ("LED resistor input", new[] { ("330 ohm IN", loadInput), ("D_MAIN cathode", dMain.CathodeTerminal), ("D_BACKUP cathode", dBackup.CathodeTerminal) }),
                ("LED anode", new[] { ("330 ohm OUT", loadOutput), ("LED anode", led.AnodeTerminal) }),
                ("battery -", new[] { ("Battery -", negative), ("Capacitor -", cap.NegativeTerminal), ("LED cathode", led.CathodeTerminal) })
            };
            // The solver gives finite-resistance wire components separate nodes.
            // Merge only enabled wire endpoints for the lesson's continuity check.
            var wireParents = new Dictionary<int, int>();
            int WireRoot(int node)
            {
                while (wireParents.TryGetValue(node, out int parent))
                    node = parent;
                return node;
            }

            foreach (var entry in graph.ComponentTerminals)
            {
                if (!(entry.Key is SparkSingleWire wire) ||
                    !wire.isActiveAndEnabled || !wire.ElectricalEnabled ||
                    !graph.TryGetPrimaryComponentTerminals(wire, out var a, out var b) ||
                    !graph.TryGetNode(a, out int aNode) || !graph.TryGetNode(b, out int bNode))
                    continue;

                int aRoot = WireRoot(aNode);
                int bRoot = WireRoot(bNode);
                if (aRoot != bRoot)
                    wireParents[bRoot] = aRoot;
            }

            var groupsByNode = new Dictionary<int, string>();
            foreach (var net in nets)
            {
                var first = net.Pins[0];
                if (first.Terminal == null || !graph.TryGetNode(first.Terminal, out int root))
                    return $"{first.Label} is missing from the solver graph.";
                root = WireRoot(root);
                if (groupsByNode.TryGetValue(root, out var otherGroup))
                    return $"Separate {net.Name} from {otherGroup}. They share a solver node.";
                groupsByNode.Add(root, net.Name);

                for (int i = 1; i < net.Pins.Length; i++)
                {
                    var pin = net.Pins[i];
                    if (pin.Terminal == null || !graph.TryGetNode(pin.Terminal, out int node))
                        return $"{pin.Label} is missing from the solver graph.";
                    if (WireRoot(node) != root)
                        return $"Connect {first.Label} to {pin.Label}. The solver sees separate nodes.";
                }
            }
            return null;
        }
        private float ChargeCurrentIntoStorage()
        {
            float current = charge.ElectricalState.Current;
            return chargeInput == charge.TerminalA ? current : -current;
        }

        private bool Lit =>
            led != null && load != null && led.IsOn &&
            I(load) >= minimumBackupCurrent && I(load) <= maximumLedCurrent;

        private bool Normal =>
            main.IsClosed && Lit && dMain.IsForwardConducting &&
            I(dBackup) <= isolationCurrent;

        private string BackupProblem()
        {
            if (!main.IsOpen) return "MAIN must be OFF.";
            if (!Lit)
                return "LED path=" + (I(load) * 1000f).ToString("F3") +
                    " mA, LED on=" + led.IsOn + "; required " +
                    (minimumBackupCurrent * 1000f).ToString("F1") + "-" +
                    (maximumLedCurrent * 1000f).ToString("F1") + " mA.";
            if (!dBackup.IsForwardConducting)
                return "D_BACKUP not forward conducting. I=" +
                    (dBackup.ElectricalState.Current * 1000f).ToString("F3") + " mA.";
            if (I(source) > isolationCurrent || I(dMain) > isolationCurrent ||
                I(dCharge) > isolationCurrent || I(charge) > isolationCurrent)
                return "Isolation >" + (isolationCurrent * 1000f).ToString("F2") +
                    "mA: source=" + (I(source) * 1000f).ToString("F3") +
                    ", D_MAIN=" + (I(dMain) * 1000f).ToString("F3") +
                    ", D_CHARGE=" + (I(dCharge) * 1000f).ToString("F3") +
                    ", R_CHARGE=" + (I(charge) * 1000f).ToString("F3") + " mA.";
            return null;
        }

        private void ReportStageChange()
        {
            if (!logStageChanges || stage == lastLoggedStage)
                return;
            lastLoggedStage = stage;
            Debug.Log("[LEVEL 29] Stage=" + stage +
                " | Status=" + status +
                " | LED=" + (led != null && led.IsOn) +
                " | Gates=" + string.Join(",", passed), this);
        }

        private void OnSolved()
        {
            solveEventCount++;
            if (!Active || bindings == null || Time.timeScale <= 0f)
                return;

            if (!ResolveParts())
            {
                // Keep the specific ResolveParts() status, not a generic error.
                if (stage != 0 || Array.Exists(passed, value => value))
                {
                    stage = 0;
                    Array.Clear(passed, 0, passed.Length);
                    baseline = float.NaN;
                    backupIntervalActive = false;
                }
                ClearGates();
                return;
            }

            if (circuit == null)
            {
                status = "Circuit system not found.";
                return;
            }
            if (circuit.TopologyVersion != topology)
            {
                topology = circuit.TopologyVersion;
                ResetAttempt("Wiring changed. Repeat charge-to-backup sequence.");
            }

            var result = solver.LatestSolveResult;
            if (result == null || result.Context == null ||
                !result.Context.SimulateTransientDevices ||
                result.TimeStep <= 0f || result.TimeStep > 0.1f)
            {
                // This is a hard prerequisite, not a failed backup transfer.
                status = "Enable transient capacitor simulation. Solver TimeStep must be 0-0.1 s.";
                return;
            }

            string problem = WiringProblem() ?? ComponentProblem();
            if (problem != null)
            {
                // Real component/topology faults invalidate the entire attempt.
                if (stage != 0 || !string.Equals(status, problem, StringComparison.Ordinal))
                    ResetAttempt(problem);
                else
                    status = problem;
                return;
            }

            float dt = result.TimeStep;
            float v = cap.ElectricalState.Voltage;
            if (!Finite(v) || v < -0.05f || v > 5.2f)
            {
                ResetAttempt("Invalid storage polarity / voltage: " + v + " V.");
                return;
            }

            if (stage == 0)
            {
                passed[0] = true;
                baseline = v;
                stage = 1;
            }

            switch (stage)
            {
                case 1: // CHARGE
                    baseline = Mathf.Min(baseline, v);
                    status = "CHARGE: MAIN ON; capacitor=" + v.ToString("F3") +
                        " V; observed rise=" + (v - baseline).ToString("F3") + " V.";
                    if (Normal && v >= chargedVoltage && v - baseline >= 0.30f)
                    {
                        passed[1] = true;
                        stage = 2;
                        status = "CHARGED. Turn MAIN OFF; do not change wiring.";
                    }
                    break;

                case 2: // WAIT FOR AUTOMATIC TRANSFER
                    status = "CHARGED. Turn MAIN OFF to test automatic backup.";
                    if (main.IsOpen)
                    {
                        string transferProblem = BackupProblem();
                        if (transferProblem == null)
                        {
                            passed[2] = true;
                            backupIntervalActive = true;
                            backupStart = v;
                            elapsed = 0f;
                            stage = 3;
                            status = "TRANSFER PASSED. Observing continuous backup.";
                        }
                        else
                        {
                            // A transient current spike / LED drop at the switching
                            // boundary is not proof that the entire circuit was wrong.
                            // Keep the real charge and wiring achievements intact.
                            status = "TRANSFER WAIT (cap " + v.ToString("F3") +
                                " V): " + transferProblem;
                            LogProblemOnce(transferProblem);
                        }
                    }
                    else if (!Normal || v < chargedVoltage - 0.1f)
                    {
                        status = "CHARGED: maintain normal power, then turn MAIN OFF.";
                    }
                    break;

                case 3: // SUSTAINED BACKUP
                    if (!main.IsOpen)
                    {
                        // Early restoration invalidates the incomplete backup
                        // interval but does not erase real charging history.
                        passed[2] = false;
                        backupIntervalActive = false;
                        elapsed = 0f;
                        stage = 2;
                        status = "MAIN restored too early. Repeat transfer / backup test.";
                        break;
                    }
                    string backupProblem = BackupProblem();
                    if (backupProblem != null)
                    {
                        // A continuous backup interval must restart after a gap.
                        // We retain the historical automatic-transfer success.
                        backupIntervalActive = false;
                        elapsed = 0f;
                        status = "BACKUP WAIT (cap " + v.ToString("F3") +
                            " V): " + backupProblem;
                        LogProblemOnce(backupProblem);
                        break;
                    }
                    if (!backupIntervalActive)
                    {
                        backupStart = v;
                        elapsed = 0f;
                        backupIntervalActive = true;
                    }
                    elapsed += dt;
                    status = "BACKUP: " + elapsed.ToString("F2") + "/" +
                        backupSeconds.ToString("F2") + " simulated s; cap " +
                        v.ToString("F3") + " V; LED=" +
                        (I(load) * 1000f).ToString("F2") + " mA; drop=" +
                        (backupStart - v).ToString("F3") + " V.";
                    if (elapsed >= backupSeconds &&
                        backupStart - v >= requiredBackupVoltageDrop)
                    {
                        passed[3] = true;
                        stage = 4;
                        status = "BACKUP PASSED. Restore MAIN power.";
                    }
                    break;

                case 4: // WAIT FOR RESTORE
                    status = "BACKUP PASSED. Turn MAIN ON to recharge.";
                    if (main.IsClosed)
                    {
                        if (!Normal)
                        {
                            status = "RESTORE WAIT: LED/main diode has not settled.";
                            break;
                        }
                        if (ChargeCurrentIntoStorage() <= 0.0002f)
                        {
                            status = "RESTORE WAIT: positive charge current <= 0.20 mA; " +
                                "R_CHARGE=" + (ChargeCurrentIntoStorage() * 1000f).ToString("F3") + " mA.";
                            break;
                        }
                        passed[4] = true;
                        restoreStart = v;
                        elapsed = 0f;
                        stage = 5;
                        status = "RESTORED. Waiting for verified recharge.";
                    }
                    break;

                case 5: // VERIFY RECHARGE
                    if (!Normal)
                    {
                        elapsed = 0f;
                        status = "RECHARGE WAIT: MAIN ON, normal path must remain valid.";
                        break;
                    }
                    if (v >= chargedVoltage && v - restoreStart >= requiredRechargeRise)
                        elapsed += dt;
                    else
                        elapsed = 0f;
                    status = "RECOVERY: cap " + v.ToString("F3") + " V; rise " +
                        (v - restoreStart).ToString("F3") + " V; hold " +
                        elapsed.ToString("F2") + "/" + recoveryHoldSeconds.ToString("F2") + " s.";
                    if (elapsed >= recoveryHoldSeconds)
                    {
                        passed[5] = true;
                        stage = 6;
                        status = "LEVEL 29 SEQUENCE PASSED: charge, isolated backup, restoration, recharge.";
                    }
                    break;

                case 6:
                    status = "LEVEL 29 SEQUENCE PASSED. Waiting for level evaluator.";
                    // Do not delete legitimate historical phase results if the
                    // player operates MAIN after completion. The evaluator remains
                    // authoritative for actual level completion.
                    break;
            }

            Publish();
            ReportStageChange();
        }

        private void LogProblemOnce(string reason)
        {
            if (!logStageChanges || string.Equals(lastLoggedProblem, reason, StringComparison.Ordinal))
                return;
            lastLoggedProblem = reason;
            Debug.LogWarning("[LEVEL 29 BACKUP] " + reason, this);
        }

        private void OnGUI()
        {
            if (!showStatus || !Active) return;
            GUI.Box(new Rect(15f, Screen.height - 105f,
                Mathf.Min(850f, Screen.width - 30f), 85f),
                "LEVEL 29 — AUTOMATIC BACKUP");
            GUI.Label(new Rect(28f, Screen.height - 78f,
                Mathf.Min(820f, Screen.width - 55f), 55f), status);
        }
    }
}
