using System;
using System.Collections.Generic;
using UnityEngine;
using ProjectSpark.Electrical;
using ProjectSpark.Circuit;

namespace ProjectSpark.Gameplay
{
    /// <summary>Level-local observer/scheduler. Never writes solved voltage/current or calls CompleteCurrentLevel.</summary>
    [DisallowMultipleComponent]
    public sealed class SparkLevel29AutomaticBackup : MonoBehaviour
    {
        public static readonly string[] Gates = { "L29_Wiring", "L29_Charged", "L29_Isolated", "L29_Backup", "L29_Restored", "L29_Recharged" };
        [SerializeField] private SparkElectricalSolver solver;
        [SerializeField] private SparkLevelLayoutLoader loader;
        [SerializeField] private SparkCircuitSystem circuit;
        [SerializeField] private bool showStatus = true;
        [SerializeField, Min(.1f)] private float backupSeconds = 2f;
        [SerializeField] private float chargedVoltage = 4.1f;
        [SerializeField] private float minimumBackupCurrent = .002f;
        [SerializeField] private float maximumLedCurrent = .009f;
        [SerializeField] private float isolationCurrent = .0001f;
        [SerializeField, TextArea] private string status = "Waiting for active Level 29.";
        public string Status => status;
        public int Stage => stage;
        private SparkLevelLayout layout;
        private SparkLevelSceneBindings bindings;
        private SparkSwitch main;
        private SparkCapacitor cap;
        private SparkResistor charge, load;
        private SparkDiode dMain, dCharge, dBackup;
        private SparkLED led;
        private SparkElectricalComponent source;
        private SparkTerminal positive, negative;
        private SparkElectricalComponent[] watched;
        private readonly bool[] passed = new bool[6];
        private int stage, topology = -1;
        private float baseline = float.NaN, backupStart, elapsed, restoreStart, nextSolve, lastFresh = -1;
        private bool subscribed;

        private void OnEnable()
        {
            layout = GetComponentInParent<SparkLevelLayout>();
            ResolveServices();
            ResetAttempt("Build the automatic backup circuit; MAIN OFF while wiring.");
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
        private bool Active => layout != null && loader != null && loader.ActiveLayout == layout &&
            layout.LevelDefinition != null && layout.LevelDefinition.LevelId == "level_29";

        private void LateUpdate()
        {
            ResolveServices();
            if (!Active || solver == null || !solver.isActiveAndEnabled || circuit == null) return;
            var active = loader.ActiveBindings;
            if (active != bindings)
            {
                ClearGates(); bindings = active;
                ResetAttempt("Waiting for Level 29 runtime bindings.");
            }
            // No solver settings are changed. One requested solve per due frame; no catch-up burst.
            if (Time.timeScale <= 0 || Time.time < nextSolve || solver.IsSolving) return;
            float dt = solver.LatestSolveResult != null ? solver.LatestSolveResult.TimeStep : 1f/60f;
            nextSolve = Time.time + Mathf.Clamp(dt, .001f, .1f);
            solver.MarkDirty();
            solver.SolveNow();
        }

        private void OnDisable()
        {
            if (subscribed && solver != null) { solver.SolveCompleted -= OnSolved; solver.SolveFailed -= OnFailed; }
            subscribed = false;
            Unwatch(); ClearGates(); bindings = null;
        }
        private void ClearGates()
        {
            if (bindings != null) foreach (string id in Gates) bindings.SetRuntimeComponentBinding(id, null);
        }
        private void Publish()
        {
            // Gate aliases resolve to the REAL LED only after their recorded checks pass.
            // No proxy electrical component, fabricated measurement or solver stamp is used.
            for (int i=0;i<Gates.Length;i++) bindings.SetRuntimeComponentBinding(Gates[i],passed[i] ? led : null);
        }
        private void ResetAttempt(string reason)
        {
            stage=0; elapsed=0; baseline=float.NaN; lastFresh=-1;
            Array.Clear(passed,0,passed.Length); ClearGates(); status=reason;
        }
        private void OnFailed() { if (Active) ResetAttempt("Solve failed. Repair the circuit and restart the charge test."); }
        private void Unwatch()
        {
            if (watched != null) foreach(var c in watched) if(c != null) c.ElectricalConfigurationChanged -= OnConfigurationChanged;
            watched=null;
        }
        private void OnConfigurationChanged() => ResetAttempt("Component settings changed. Repeat the sequence.");
        private bool ResolveParts()
        {
            if (bindings == null) return false;
            foreach(string gate in Gates) if(!bindings.IsComponentIdDeclared(gate)) { status="Declare runtime gate ID: " + gate; return false; }
            main=Get<SparkSwitch>("Power_Switch"); cap=Get<SparkCapacitor>("C_STORE_Component");
            charge=Get<SparkResistor>("R_CHARGE_Component"); load=Get<SparkResistor>("R_LED_Component");
            dMain=Get<SparkDiode>("D_MAIN_Component"); dCharge=Get<SparkDiode>("D_CHARGE_Component");
            dBackup=Get<SparkDiode>("D_BACKUP_Component"); led=Get<SparkLED>("led");
            source=Get<SparkElectricalComponent>("PowerSupply_01");
            bindings.TryGetTerminal("Battery_positive",out positive); bindings.TryGetTerminal("Battery_negative",out negative);
            var parts=new SparkElectricalComponent[]{main,cap,charge,load,dMain,dCharge,dBackup,led,source};
            foreach(var p in parts) if(p == null || !p.isActiveAndEnabled || !p.ElectricalEnabled || !p.transform.IsChildOf(layout.transform)) return false;
            if(positive==null || negative==null) return false;
            bool changed=watched==null;
            if(!changed) for(int i=0;i<parts.Length;i++) if(parts[i]!=watched[i]) changed=true;
            if(changed)
            {
                Unwatch(); ResetAttempt("Components resolved. Start the charge test."); watched=parts;
                // Main switch changes are expected. All other component changes invalidate phase history.
                for(int i=1;i<watched.Length;i++) watched[i].ElectricalConfigurationChanged += OnConfigurationChanged;
            }
            return true;
        }
        private T Get<T>(string id) where T:SparkElectricalComponent =>
            bindings.TryGetComponent(id,out var c) ? c as T : null;
        private bool Healthy()
        {
            foreach(var p in watched)
                if(!Finite(p.ElectricalState.Voltage) || !Finite(p.ElectricalState.Current) || !Finite(p.ElectricalState.Power)) return false;
            return !charge.IsBurned && !load.IsBurned && !charge.IsOverPower && !load.IsOverPower &&
                !cap.IsOverVoltage && !led.IsOverCurrent && !led.IsReverseVoltageExceeded &&
                !dMain.IsReverseFault && !dCharge.IsReverseFault && !dBackup.IsReverseFault &&
                Near(charge.ResistanceOhms,220,1) && Near(load.ResistanceOhms,330,1) &&
                Near(cap.CapacitanceFarads,.01f,.00001f) && cap.VoltageRating>=10 &&
                Near(dMain.ForwardVoltage,.7f,.02f) && Near(dCharge.ForwardVoltage,.7f,.02f) &&
                Near(dBackup.ForwardVoltage,.7f,.02f) && Near(led.ForwardVoltage,2,.02f) &&
                Mathf.Abs(source.ElectricalState.Voltage)>=4.8f && Mathf.Abs(source.ElectricalState.Voltage)<=5.2f;
        }
        private static bool Finite(float v) => !float.IsNaN(v) && !float.IsInfinity(v);
        private static bool Near(float x,float y,float e) => Mathf.Abs(x-y)<=e;
        private static float I(SparkElectricalComponent c) => Mathf.Abs(c.ElectricalState.Current);

        private bool TopologyCorrect()
        {
            var graph=solver.CurrentGraph;
            if(graph==null) return false;
            // Each row is one required wire-net. Distinct rows must never be shorted together.
            var nets=new SparkTerminal[][] {
                new[]{positive,main.InputTerminal},
                new[]{main.OutputTerminal,dMain.AnodeTerminal,dCharge.AnodeTerminal},
                new[]{dCharge.CathodeTerminal,charge.TerminalA},
                new[]{charge.TerminalB,cap.PositiveTerminal,dBackup.AnodeTerminal},
                new[]{dMain.CathodeTerminal,dBackup.CathodeTerminal,load.TerminalA},
                new[]{load.TerminalB,led.AnodeTerminal},
                new[]{negative,cap.NegativeTerminal,led.CathodeTerminal}
            };
            var roots=new HashSet<int>();
            foreach(var net in nets)
            {
                if(!graph.TryGetNode(net[0],out int root) || !roots.Add(root)) return false;
                foreach(var t in net) if(!graph.TryGetNode(t,out int n) || n!=root) return false;
            }
            return true;
        }
        private bool Lit => led.IsOn && I(load)>=minimumBackupCurrent && I(load)<=maximumLedCurrent;
        private bool Normal => main.IsClosed && Lit && dMain.IsForwardConducting && I(dBackup)<=isolationCurrent;
        private bool Backup => main.IsOpen && Lit && dBackup.IsForwardConducting &&
            I(source)<=isolationCurrent && I(dMain)<=isolationCurrent && I(dCharge)<=isolationCurrent && I(charge)<=isolationCurrent;

        private void OnSolved()
        {
            if(!Active || bindings==null || Time.timeScale<=0) return;
            if(!ResolveParts()) { ClearGates(); stage=0; Array.Clear(passed,0,6); baseline=float.NaN; status="Missing/disabled part or runtime binding. Check setup."; return; }
            if(circuit.TopologyVersion!=topology) { topology=circuit.TopologyVersion; ResetAttempt("Wiring changed. Repeat the charge-to-backup sequence."); }
            var result=solver.LatestSolveResult;
            if(result==null || result.Context==null || !result.Context.SimulateTransientDevices || result.TimeStep<=0 || result.TimeStep>.1f)
            { ResetAttempt("Enable capacitor transient simulation; Time Step must be positive and <= 0.1 s."); return; }
            if(!Healthy() || !TopologyCorrect()) { ResetAttempt("Check values, diode direction, wiring, health and 5 V supply. See the wiring diagram."); return; }
            float dt=result.TimeStep;
            // Interrupted observation cannot count as sustained backup. Time is accepted solver time.
            if(lastFresh>=0 && Time.time-lastFresh>.25f && stage==3) { ResetAttempt("Backup sampling interrupted. Recharge and retry."); return; }
            lastFresh=Time.time;
            float v=cap.ElectricalState.Voltage;
            if(v<-.05f || v>5.2f) { ResetAttempt("Storage capacitor polarity/voltage is invalid."); return; }
            if(stage==0) { passed[0]=true; baseline=v; stage=1; }
            switch(stage)
            {
                case 1:
                    baseline=Mathf.Min(baseline,v);
                    status="CHARGE: MAIN ON. Reach 4.10 V after an observed rise of at least 0.30 V.";
                    if(Normal && v>=chargedVoltage && v-baseline>=.3f) { passed[1]=true; stage=2; status="CHARGED. Turn MAIN OFF; do not change wires."; }
                    break;
                case 2:
                    status="CHARGED. Turn MAIN OFF to test automatic transfer.";
                    if(main.IsOpen)
                    {
                        if(!Backup) { ResetAttempt("Automatic transfer failed. Check diode polarity and charge again."); break; }
                        passed[2]=true; backupStart=v; elapsed=0; stage=3;
                    }
                    else if(!Normal || v<chargedVoltage-.1f) ResetAttempt("Charge condition lost before transfer. Retry.");
                    break;
                case 3:
                    if(!Backup) { ResetAttempt("Backup failed or MAIN restored too early. Recharge and retry."); break; }
                    elapsed+=dt;
                    status="BACKUP: " + elapsed.ToString("F2") + " / " + backupSeconds.ToString("F2") + " simulated s; LED >= 2 mA.";
                    if(elapsed>=backupSeconds && backupStart-v>=.2f)
                    { passed[3]=true; stage=4; status="BACKUP PASSED. Restore MAIN power now."; }
                    break;
                case 4:
                    status="BACKUP PASSED. Restore MAIN power.";
                    if(main.IsClosed)
                    {
                        if(!Normal || charge.ElectricalState.Current<=.0002f) { ResetAttempt("Restore did not resume safe charging. Retry."); break; }
                        passed[4]=true; restoreStart=v; elapsed=0; stage=5;
                    }
                    break;
                case 5:
                    if(!Normal) { ResetAttempt("MAIN must remain ON during recovery. Repeat the sequence."); break; }
                    if(v>=chargedVoltage && v-restoreStart>=.15f) elapsed+=dt; else elapsed=0;
                    status="RECOVER: recharge to 4.10 V, then hold normal operation for 0.5 simulated s.";
                    if(elapsed>=.5f) { passed[5]=true; stage=6; status="SEQUENCE PASSED: automatic transfer, isolation and recharge verified."; }
                    break;
                case 6:
                    if(!Normal) ResetAttempt("Final normal operation lost. Repeat the sequence.");
                    break;
            }
            Publish();
        }
        private void OnGUI()
        {
            if(!showStatus || !Active) return;
            GUI.Box(new Rect(15,Screen.height-105,Mathf.Min(850,Screen.width-30),85),"LEVEL 29 — AUTOMATIC BACKUP");
            GUI.Label(new Rect(28,Screen.height-78,Mathf.Min(820,Screen.width-55),55),status);
        }
    }
}
