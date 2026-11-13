using System;
using System.Collections.Generic;
using ProjectSpark.Circuit;
using ProjectSpark.Electrical;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.Gameplay
{
    [DisallowMultipleComponent]
    public sealed class SparkLevel46Sync : MonoBehaviour
    {
        public static readonly string[] Gates = {"L46_Build","L46_Direction","L46_30RPM","L46_60RPM","L46_Load","L46_Repair"};
        private static readonly int[] Gray = {0,2,3,1};
        private static readonly int[] Delta = {0,-1,1,0, 1,0,0,-1, -1,0,0,1, 0,1,-1,0};
        private static readonly float[] Brake = {0,.003f,.007f,.043f};
        private static readonly string[] Loads = {"RELEASED","LIGHT LOAD","TEST LOAD","OVERLOAD"};
        private enum Mode { Off, Open, Sync, Test, Fault }
        private const float StepSize=.01f, Edge=2*Mathf.PI/80f, ToRpm=60f/(2*Mathf.PI);

        [Serializable]
        public sealed class Channel
        {
            public SparkMotor motor;
            public SparkSwitch enable;
            public SparkResistor impedance;
            public SparkPowerSupply pwm, a, b;
            public SparkTerminal pwmIn, aIn, bIn;
            [NonSerialized] public float omega, rpm, measured, duty, applied, integral, angle, noEdge, stall;
            [NonSerialized] public int ticks, previous, count, sampled;
            [NonSerialized] public int forward, reverse;

            public void Reset()
            {
                omega=rpm=measured=duty=applied=integral=angle=noEdge=stall=0;
                ticks=previous=count=sampled=forward=reverse=0;
            }
        }

        public SparkLevelLayout layout;
        public SparkPowerSupply source;
        public SparkTerminal[] pins;
        public Channel master, follower;
        public SparkLevel46VisualRig rig;
        public Text statusText, meterText, detailText;
        public TMP_Text masterDisplay, followerDisplay, tensionDisplay;
        public Button jogButton, openButton, syncButton, rpm30Button, rpm60Button, loadButton, testButton, resetButton, restartButton;
        public Transform[] webSlats;
        public GameObject[] lamps;
        [SerializeField, Min(.001f)] private float rollerRadius=.025f;
        [SerializeField, Min(.0001f)] private float inertia=.0027f;
        [SerializeField, Min(.1f)] private float complianceRelaxation=2.5f;
        [SerializeField, Min(1f)] private float tensionLimit=3.5f;

        private readonly List<SparkCircuitConnection> wires=new();
        private readonly Dictionary<SparkTerminal,SparkTerminal> links=new();
        private SparkLevelLayoutLoader loader;
        private LevelGamePlayManager manager;
        private SparkLevelSceneBindings bindings;
        private SparkCircuitSystem circuit;
        private SparkElectricalSolver solver;
        private Mode mode;
        private int topology=-1, stage, target=30, load;
        private float accumulator, controlTimer, sampleTimer, hold, reference, strain, tension, tensionTime, travel;
        private float baseline, baselineDuty;
        private bool ready, crossed, loadDrop, dutyRise, directionFault, repaired;
        private string wiring, fault;
        private Vector3[] slatOrigin;

        private void Update()
        {
            if (loader==null) loader=FindFirstObjectByType<SparkLevelLayoutLoader>();
            if (loader==null || loader.ActiveLayout!=layout) return;
            if (manager==null) manager=FindFirstObjectByType<LevelGamePlayManager>();
            if (manager==null) return;
            circuit=manager.CircuitSystem;
            solver=manager.ElectricalSolver;
            if (circuit==null || solver==null) return;
            if (bindings!=loader.ActiveBindings)
            {
                bindings=loader.ActiveBindings;
                if (bindings==null) return;
                Restart();
            }
            if (topology!=circuit.TopologyVersion)
            {
                topology=circuit.TopologyVersion;
                wiring=CheckWiring();
                if (Running && (wiring!=null || crossed)) Stop(Mode.Off);
            }
            ready=wiring==null && ReadSupply();
            if (stage==0 && ready) Award();
            accumulator+=Mathf.Min(Time.unscaledDeltaTime,.10f);
            while (accumulator>=StepSize)
            {
                accumulator-=StepSize;
                Simulate(StepSize);
            }
            Present();
        }

        private bool Running => mode is Mode.Open or Mode.Sync or Mode.Test;
        private bool CanRun => ready && circuit.TopologyVersion==topology && mode!=Mode.Fault;
        public void Jog() { if (CanRun && !crossed && stage==1) BeginDrive(Mode.Open); }
        public void Open() { if (CanRun && !crossed) { if (mode==Mode.Open) Stop(Mode.Off); else BeginDrive(Mode.Open); } }
        public void Synchronize() { if (CanRun && !crossed) { if (mode==Mode.Sync) Stop(Mode.Off); else BeginDrive(Mode.Sync); } }
        public void Select30() => Select(30);
        public void Select60() => Select(60);
        private void Select(int value) { if (mode!=Mode.Fault) { target=value; hold=0; } }
        public void TestDirection() { if (CanRun && crossed && stage==5 && !directionFault) BeginDrive(Mode.Test); }
        public void CycleBrake() => SetBrake((load+1)%Brake.Length);
        public void TestLoad() => SetBrake(load==2 ? 0 : 2);

        private void SetBrake(int setting)
        {
            if (setting==2 && load!=2 && stage==4 && mode==Mode.Sync && Stable())
            {
                baseline=follower.rpm;
                baselineDuty=follower.duty;
                loadDrop=dutyRise=false;
            }
            else if (setting!=2) baseline=0;
            load=setting;
            hold=0;
        }

        public void ResetFault()
        {
            if (mode!=Mode.Fault || !ready || crossed || load==3 || master.rpm>3 || follower.rpm>3 ||
                !ReadPhase(master,out _) || !ReadPhase(follower,out _)) return;
            repaired=directionFault;
            fault=null;
            Stop(Mode.Off);
        }

        public void Restart()
        {
            if (bindings==null) return;
            Stop(Mode.Off);
            foreach (string gate in Gates) bindings.ClearRuntimeComponentBinding(gate);
            master.Reset();
            follower.Reset();
            stage=load=0;
            target=30;
            topology=-1;
            accumulator=controlTimer=sampleTimer=hold=reference=strain=tension=tensionTime=travel=baseline=0;
            loadDrop=dutyRise=directionFault=repaired=false;
            fault=null;
            manager.QueueEvaluation();
        }

        private void BeginDrive(Mode next)
        {
            if (!ReadPhase(master,out master.previous) || !ReadPhase(follower,out follower.previous)) return;
            mode=next;
            reference=0;
            hold=controlTimer=sampleTimer=tensionTime=0;
            Prepare(master);
            Prepare(follower);
        }

        private static void Prepare(Channel c)
        {
            c.forward=c.reverse=0;
            c.sampled=c.count;
            c.noEdge=c.stall=c.integral=0;
        }

        private void Simulate(float dt)
        {
            if (Running && !ready) Trip("SUPPLY OR FEEDBACK LOST");
            controlTimer+=dt;
            if (Running && controlTimer>=.049f)
            {
                controlTimer=0;
                reference=Mathf.MoveTowards(reference, mode==Mode.Test ? 26f : target, 65f*.05f);
                bool closed=mode==Mode.Sync;
                float followerTarget=Mathf.Clamp(master.measured,0,reference+3f);
                float followerReference=Mathf.Lerp(reference,followerTarget,.55f);
                Actuate(master,Command(master,reference,reference,closed));
                if (Running) Actuate(follower,Command(follower,reference,followerReference,closed));
            }

            float masterSurface=master.omega*rollerRadius, followerSurface=follower.omega*rollerRadius;
            float difference=followerSurface-masterSurface;
            strain+=(difference-strain*complianceRelaxation)*dt;
            tension=70f*strain+1.0f*difference;
            travel=Mathf.Repeat(travel+(masterSurface+followerSurface)*.5f*dt,1f);
            Integrate(master,0,dt);
            Integrate(follower,Brake[load],dt);
            SampleEncoder(master,dt);
            SampleEncoder(follower,dt);
            sampleTimer+=dt;
            if (sampleTimer>=.499f)
            {
                Measure(master,sampleTimer);
                Measure(follower,sampleTimer);
                sampleTimer=0;
            }
            if (!Running) return;
            if (mode==Mode.Test && follower.reverse>=3 && follower.rpm>3f)
            {
                directionFault=true;
                Trip("FOLLOWER ENCODER REVERSED");
                return;
            }
            if (master.reverse>0 || follower.reverse>0 && mode!=Mode.Test) { Trip("ENCODER DIRECTION MISMATCH"); return; }
            if (Unsafe(master,dt) || Unsafe(follower,dt)) return;
            tensionTime=Mathf.Abs(tension)>tensionLimit ? tensionTime+dt : 0;
            if (tensionTime>.45f) { Trip("WEB TENSION LIMIT"); return; }

            if (stage==1 && master.forward>=12 && follower.forward>=12 && master.rpm>10 && follower.rpm>10)
            {
                Award();
                if (mode==Mode.Open) Stop(Mode.Off);
            }
            if (stage==4 && baseline>0 && load==2)
            {
                loadDrop |= follower.rpm<baseline-1.3f;
                dutyRise |= follower.duty>baselineDuty+.065f;
            }
            bool requested=stage==2 && target==30 || stage==3 && target==60 ||
                stage==4 && target==60 && load==2 && loadDrop && dutyRise ||
                stage==5 && target==60 && repaired && master.forward>=12 && follower.forward>=12;
            hold=mode==Mode.Sync && requested && Stable() ? hold+dt : 0f;
            if (hold>=2f) Award();
        }

        private static float Command(Channel c,float feed,float setpoint,bool closed)
        {
            if (!closed) return Mathf.Clamp01(feed/85.5f);
            float error=setpoint-c.measured;
            float candidate=Mathf.Clamp(c.integral+.010f*error*.05f,-.28f,.60f);
            float output=feed/85.5f+.0045f*error+candidate;
            if (output is >=0 and <=.98f || output>.98f && error<0 || output<0 && error>0)
                c.integral=candidate;
            return Mathf.Clamp(feed/85.5f+.0045f*error+c.integral,0,.98f);
        }

        private void Actuate(Channel c,float duty)
        {
            c.duty=duty;
            duty=Mathf.Round(duty*100f)*.01f;
            SetSource(c.pwm,duty*5f);
            var solved=solver.LatestSolveResult;
            if (solved==null || !solved.TryGetVoltage(c.pwmIn,source.NegativeTerminal,out float volts))
            {
                Trip("PWM SIGNAL LOST");
                return;
            }
            float applied=Mathf.Clamp(volts/5f,0,.985f);
            if (applied<.008f)
            {
                if (c.enable.IsClosed) c.enable.SetState(SparkSwitchState.Open);
                c.applied=0;
                return;
            }
            if (Mathf.Abs(applied-c.applied)>.004f || c.enable.IsOpen)
            {
                c.impedance.SetResistance(c.motor.WindingResistance*(1f/applied-1f));
                c.applied=applied;
                if (c.enable.IsOpen) c.enable.SetState(SparkSwitchState.Closed);
            }
        }

        private void Integrate(Channel c,float brake,float dt)
        {
            float current=c.enable.IsClosed ? Mathf.Abs(c.motor.ElectricalState.Current) : 0f;
            float torque=.24f*Mathf.Max(0,current-.25f*c.omega/c.motor.WindingResistance);
            c.omega=Mathf.Max(0,c.omega+(torque-.00215f*c.omega-brake)*dt/inertia);
            c.rpm=c.omega*ToRpm;
        }

        private void SampleEncoder(Channel c,float dt)
        {
            c.angle+=c.omega*dt;
            c.noEdge+=dt;
            while (c.angle>=Edge)
            {
                c.angle-=Edge;
                c.ticks=(c.ticks+1)%80;
                if (!ready || !ReadPhase(c,out int next)) continue;
                int delta=Delta[c.previous*4+next];
                if (Running && delta==0 && c.previous!=next) Trip("INVALID QUADRATURE TRANSITION");
                c.previous=next;
                c.count+=delta;
                if (delta>0) c.forward++;
                if (delta<0) c.reverse++;
                if (delta!=0) c.noEdge=0;
            }
        }

        private static void Measure(Channel c,float interval)
        {
            c.measured=Mathf.Lerp(c.measured,(c.count-c.sampled)*60f/(80f*interval),.68f);
            c.sampled=c.count;
        }

        private bool Unsafe(Channel c,float dt)
        {
            if (c.rpm>92f || c.motor.IsOverCurrent || source.IsOverloaded) { Trip("MOTOR OVERLOAD OR OVERSPEED"); return true; }
            if (c.rpm>12f && c.noEdge>.24f) { Trip("ENCODER FEEDBACK MISSING"); return true; }
            c.stall=c.duty>.34f && c.rpm<4f ? c.stall+dt : 0f;
            if (c.stall>1.15f) { Trip("MOTOR STALL · RELEASE BRAKE"); return true; }
            return false;
        }

        private bool Stable() => Mathf.Abs(master.rpm-target)<=3 && Mathf.Abs(follower.rpm-target)<=3 &&
            Mathf.Abs(master.measured-target)<=3 && Mathf.Abs(follower.measured-target)<=3 &&
            Mathf.Abs(master.rpm-follower.rpm)<=2 && Mathf.Abs(master.measured-follower.measured)<=2 &&
            Mathf.Abs(tension)<.6f;

        private static void SetSource(SparkPowerSupply supply,float volts)
        {
            if (Mathf.Abs(supply.OutputVoltage-volts)<.004f) return;
            supply.SetOutput(false,default);
            if (!supply.TryConfigure(volts,.025f,out var reason)) Debug.LogError(reason,supply);
            supply.SetOutput(true,default);
        }

        private bool ReadSupply()
        {
            if (solver.IsSolving || solver.IsDirty && !solver.SolveNow()) return false;
            var result=solver.LatestSolveResult;
            return result!=null && source.IsOutputActive && !source.IsOverloaded &&
                result.TryGetVoltage(pins[1],pins[3],out float v) && v>=4.8f && v<=5.2f;
        }

        private bool ReadPhase(Channel c,out int phase)
        {
            phase=Gray[c.ticks%4];
            if (crossed && c==follower) phase=((phase&1)<<1)|(phase>>1);
            return ready;
        }

        private string CheckWiring()
        {
            crossed=false;
            circuit.CopyConnections(wires);
            links.Clear();
            SparkTerminal Root(SparkTerminal pin)
            {
                while (links.TryGetValue(pin,out var p)) pin=p;
                return pin;
            }
            foreach (var wire in wires)
            {
                if (wire.Kind!=SparkConnectionKind.Wire) continue;
                var a=Root(wire.A);
                var b=Root(wire.B);
                if (a!=b) links[b]=a;
            }
            var nets=new HashSet<SparkTerminal>();
            for (int i=0;i<20;i+=2)
            {
                var root=Root(pins[i]);
                if (root!=Root(pins[i+1])) return $"Connect {pins[i].name} → {pins[i+1].name}.";
                if (!nets.Add(root)) return "Separate the wired signal and power nets.";
            }
            var fa=Root(pins[20]);
            var fb=Root(pins[22]);
            if (!nets.Add(fa) || !nets.Add(fb)) return "Separate follower A and B.";
            if (fa==Root(pins[21]) && fb==Root(pins[23])) return null;
            if (stage==5 && !directionFault && fa==Root(pins[23]) && fb==Root(pins[21]))
            {
                crossed=true;
                return null;
            }
            return "Connect follower A → A and B → B. During the direction test, cross both leads.";
        }

        private void Stop(Mode next)
        {
            mode=next;
            hold=0;
            foreach (Channel c in new[]{master,follower})
            {
                if (c.enable.IsClosed) c.enable.SetState(SparkSwitchState.Open);
                SetSource(c.pwm,0);
                c.duty=c.applied=0;
            }
        }

        private void Trip(string reason)
        {
            if (mode==Mode.Fault) return;
            fault=reason;
            Stop(Mode.Fault);
            Debug.Log($"[LEVEL 46] SAFE STOP · {reason}",this);
        }

        private void Award()
        {
            if (!bindings.SetRuntimeComponentBinding(Gates[stage],source)) return;
            stage++;
            hold=0;
            manager.QueueEvaluation();
        }

        private string Prompt()
        {
            if (mode==Mode.Fault) return $"SAFE STOP · {fault}  |  "+(ready ? "Wait for both shafts to stop, then RESET FAULT." : wiring);
            if (!ready) return wiring ?? "Waiting for the 5 V reference.";
            return stage switch
            {
                1=>"Jog forward. Verify both encoders count up.",
                2=>"Select 30 RPM and start SYNC.",
                3=>"Select 60 RPM and hold synchronization.",
                4=>"Apply TEST LOAD while synchronized at 60 RPM.",
                5=>repaired ? "Restart SYNC at 60 RPM." : crossed ? "Press TEST DIRECTION." : "Stop SYNC, undo W12 and W11, then cross follower A/B.",
                _=>"Both motors synchronized at 60 RPM. Transport verified."
            };
        }

        private void Present()
        {
            bool stable=mode==Mode.Sync && Stable();
            string status=mode switch {
                Mode.Fault=>"SAFE STOP", Mode.Open=>"OPEN LOOP", Mode.Test=>"DIRECTION TEST",
                Mode.Sync when stable=>"SYNCHRONIZED", Mode.Sync when load==2=>"LOAD RECOVERY",
                Mode.Sync=>"SYNCHRONIZING", _=>ready ? "READY" : "OFF"
            };
            statusText.text=Prompt();
            meterText.text=$"TARGET {target} RPM   |   MASTER {master.measured:F1}   |   FOLLOWER {follower.measured:F1}   |   Δ {Mathf.Abs(master.measured-follower.measured):F1} RPM";
            detailText.text=$"{status}   |   PWM {master.duty:P0} / {follower.duty:P0}   |   WEB {tension:+0.00;-0.00;0.00} N   |   {Loads[load]}   |   {manager.SatisfiedTargetCount}/6";
            masterDisplay.text=$"MASTER {master.measured:F1} RPM\nPWM {master.duty:P0}   CNT {master.count}";
            followerDisplay.text=$"FOLLOWER {follower.measured:F1} RPM\nPWM {follower.duty:P0}   CNT {follower.count}";
            tensionDisplay.text=$"{tension:+0.00;-0.00;0.00} N\n{status}";
            rig.ApplyReadings(master.rpm,follower.rpm,load/3f,stable,mode==Mode.Fault,status);
            slatOrigin??=Array.ConvertAll(webSlats,slat=>slat.localPosition);
            for (int i=0;i<webSlats.Length;i++)
            {
                var p=slatOrigin[i];
                p.x=Mathf.Repeat(p.x+.42f+travel,.84f)-.42f;
                p.y-=Mathf.Clamp(-tension*.014f,0,.045f)*(1f-Mathf.Abs(p.x)/.42f);
                webSlats[i].localPosition=p;
            }
            bool[] light={source.IsOutputActive,master.enable.IsClosed,follower.enable.IsClosed,stable,stable,load>0,
                stable,tension<-.32f,mode==Mode.Fault};
            for (int i=0;i<lamps.Length;i++) if (lamps[i].activeSelf!=light[i]) lamps[i].SetActive(light[i]);
            jogButton.interactable=CanRun && stage==1 && !Running;
            openButton.interactable=CanRun && !crossed;
            syncButton.interactable=CanRun && stage>=2 && !crossed;
            rpm30Button.interactable=rpm60Button.interactable=mode!=Mode.Fault;
            loadButton.interactable=true;
            loadButton.GetComponentInChildren<Text>().text=Loads[load];
            testButton.interactable=CanRun && stage==5 && crossed && !Running && !directionFault;
            resetButton.interactable=mode==Mode.Fault && ready && !crossed && master.rpm<=3 && follower.rpm<=3 && load!=3;
            restartButton.interactable=true;
        }

        private void OnDisable()
        {
            if (gameObject.activeInHierarchy && master!=null && follower!=null) Stop(Mode.Off);
            if (bindings!=null) foreach (string gate in Gates) bindings.ClearRuntimeComponentBinding(gate);
            bindings=null;
        }
    }
}
