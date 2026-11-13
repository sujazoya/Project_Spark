using System.Collections.Generic;
using ProjectSpark.Circuit;
using ProjectSpark.Electrical;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.Gameplay
{
    [DisallowMultipleComponent]
    public sealed class SparkLevel52Balance : MonoBehaviour
    {
        public static readonly string[] Gates={"L52_Wired","L52_Baseline","L52_Influence","L52_Cancelled","L52_HighSpeed","L52_Recovered"};
        public static readonly (int a,int b)[] Wires={
            (0,1),(2,3),(0,4),(2,5),(0,6),(2,7),(0,8),(2,9),(10,11),(12,13),
            (14,15),(16,17),(18,19),(20,21),(22,23),(24,25),(26,27),(28,29),(30,31)
        };
        private const float Step=.01f, Mass=.62f, PhaseTimeout=1.95f;
        private static readonly int[] Gray={0,2,3,1};
        private static readonly int[] Delta={0,-1,1,0,1,0,0,-1,-1,0,0,1,0,1,-1,0};
        private enum Motion { Idle, Trial, Correcting }

        public SparkLevelLayout layout;
        public SparkPowerSupply source;
        public SparkMotor motor;
        public SparkResistor driver,actuator;
        public SparkSwitch drive,balanceDrive,safety;
        public SparkTerminal[] pins;
        public SparkLevel52VisualRig rig;
        public TMP_Text controllerDisplay,qualityDisplay;
        public Text statusText,meterText,detailText,calibrationText;
        public Button powerButton,slowButton,fastButton,startButton,stopButton,baselineButton,modeButton,trialButton;
        public Button calculateButton,balanceButton,verifyButton,phaseButton,guardButton,resetButton;

        private readonly List<SparkCircuitConnection> wires=new();
        private readonly Dictionary<SparkTerminal,SparkTerminal> nets=new();
        private SparkLevelLayoutLoader loader;
        private LevelGamePlayManager manager;
        private SparkLevelSceneBindings bindings;
        private SparkCircuitSystem circuit;
        private SparkElectricalSolver solver;
        private Motion motion;
        private int topology=-1,stage,targetRpm=45,edge,count,phase,indexCount,tachCount,revolutions;
        private float accumulator,omega,angle,driveClock,tachClock,rpm,sinceIndex,weightA=45,weightB=225,goalA=45,goalB=225;
        private float square,weightSum,rms,hold,lastVoltage,lastCurrent;
        private Vector2 integral,vector,baseline,trialVector,correction,influence;
        private bool powered=true,running,ready,safe,guardOpen,phaseValid,armed,automatic,verify,changedSpeed,phaseLost,recovered;
        private float vibrationSample;
        private readonly float[] history=new float[240];
        private int historyIndex,historyCount;
        private string wiring,fault,notice;

        private float ActualRpm=>omega*60/(2*Mathf.PI);
        private bool Stopped=>!running && !drive.IsClosed && ActualRpm<.5f;
        private bool Stable=>running && phaseValid && Mathf.Abs(rpm-targetRpm)<1.5f && Mathf.Abs(ActualRpm-targetRpm)<1.5f;
        private bool Settled=>Mathf.Abs(Mathf.DeltaAngle(weightA,goalA))<.15f && Mathf.Abs(Mathf.DeltaAngle(weightB,goalB))<.15f;
        private bool Low=>baseline.magnitude>.1f && vector.magnitude<=baseline.magnitude*.30f && vector.magnitude<.35f;
        private bool CanStart=>Stopped && ready && safe && fault==null;
        private bool CanReset=>Stopped && ready && safe && Routed(18) && fault!=null;

        private void Update()
        {
            if(loader==null) loader=FindFirstObjectByType<SparkLevelLayoutLoader>();
            if(loader==null || loader.ActiveLayout!=layout) return;
            if(manager==null) manager=FindFirstObjectByType<LevelGamePlayManager>();
            if(manager==null) return;
            solver=manager.ElectricalSolver;
            circuit=manager.CircuitSystem;
            if(solver==null || circuit==null) return;
            if(bindings!=loader.ActiveBindings) { bindings=loader.ActiveBindings; topology=-1; }
            if(bindings==null) return;
            if(topology!=circuit.TopologyVersion) { topology=circuit.TopologyVersion; wiring=CheckWires(); }
            ReadPower();
            if(stage==0 && ready && safe) Award();
            if(running && (!ready || !safe))
                Trip(!Routed(18)?"ENCODER Z LOST":!safe?"GUARD INTERLOCK LOST":wiring??"POWER OR SENSOR FAULT");
            accumulator+=Mathf.Min(Time.unscaledDeltaTime,.15f);
            while(accumulator>=Step) { accumulator-=Step; Simulate(); }
            Present();
        }

        public void Power()
        {
            powered=!powered;
            if(!powered) StopMachine();
            source.SetOutput(powered,default);
        }
        public void Speed45() { if(Stopped && stage<4) targetRpm=45; }
        public void Speed60()
        {
            if(!running || stage!=4 || Mathf.Abs(ActualRpm-45)>2) return;
            targetRpm=60;
            changedSpeed=true;
            ClearMeasure();
        }
        public void StartMachine()
        {
            if(!CanStart) return;
            running=true;
            notice=null;
            sinceIndex=0;
            phaseValid=false;
            ClearMeasure();
            SetContact(drive,true);
        }
        public void StopMachine()
        {
            running=false;
            phaseValid=false;
            verify=false;
            if(motion!=Motion.Idle)
            {
                goalA=weightA;
                goalB=weightB;
            }
            motion=Motion.Idle;
            SetContact(drive,false);
            SetContact(balanceDrive,false);
        }
        public void ToggleMode() { if(motion==Motion.Idle && stage<=3) automatic=!automatic; }
        public void Baseline()
        {
            if(stage!=1 || !Stable || revolutions<5 || vector.magnitude<.55f ||
                Mathf.Abs(Mathf.Abs(Mathf.DeltaAngle(weightA,weightB))-180)>.25f) return;
            baseline=vector;
            Award();
        }
        public void Trial()
        {
            if(stage!=2 || !Stable || motion!=Motion.Idle || revolutions<2) return;
            trialVector=Polar(.54f,18);
            if(!SetCorrection(trialVector,Motion.Trial)) return;
            notice="Measuring a known trial correction.";
        }
        public void Calculate()
        {
            if(stage!=2 || !Stable || motion!=Motion.Trial || !Settled || revolutions<4) return;
            influence=Divide(vector-baseline,trialVector);
            if(influence.magnitude<.15f) { notice="Influence too small. Check vibration feedback."; return; }
            correction=Divide(-baseline,influence);
            if(float.IsNaN(correction.magnitude) || float.IsInfinity(correction.magnitude) || correction.magnitude>2*Mass)
            { notice="Correction exceeds ring capacity."; return; }
            motion=Motion.Idle;
            notice=null;
            Award();
        }
        public void AutoBalance()
        {
            if(stage!=3 || !Stable || !automatic || motion!=Motion.Idle) return;
            if(SetCorrection(correction,Motion.Correcting)) notice="Moving both correction masses.";
        }
        public void Verify()
        {
            if(stage<3 || stage>5 || !Stable || !Settled || motion==Motion.Trial) return;
            verify=true;
            ClearMeasure();
        }
        public void PhaseTest()
        {
            if(stage!=5 || !running) return;
            armed=true;
            notice="Use Undo Last Wire to remove W19, then reconnect ENCODER Z → Z IN.";
        }
        public void Guard()
        {
            if(!Stopped) return;
            guardOpen=!guardOpen;
            SetContact(safety,!guardOpen);
        }
        public void ResetFault()
        {
            if(!CanReset) return;
            recovered=phaseLost;
            fault=null;
            armed=false;
            phaseValid=false;
            ClearMeasure();
        }

        private static Vector2 Polar(float radius,float degrees)
        {
            float a=degrees*Mathf.Deg2Rad;
            return radius*new Vector2(Mathf.Cos(a),Mathf.Sin(a));
        }
        private static Vector2 Multiply(Vector2 a,Vector2 b)=>new(a.x*b.x-a.y*b.y,a.x*b.y+a.y*b.x);
        private static Vector2 Divide(Vector2 a,Vector2 b)=>Multiply(a,new Vector2(b.x,-b.y))/b.sqrMagnitude;
        private static float Phase(Vector2 v)=>Mathf.Repeat(Mathf.Atan2(v.y,v.x)*Mathf.Rad2Deg,360);
        private bool SetCorrection(Vector2 desired,Motion next)
        {
            if(!phaseValid || !ready || !safe || !Routed(15) || desired.magnitude>2*Mass) return false;
            float spread=Mathf.Acos(Mathf.Clamp01(desired.magnitude/(2*Mass)))*Mathf.Rad2Deg;
            goalA=Mathf.Repeat(Phase(desired)+spread,360);
            goalB=Mathf.Repeat(Phase(desired)-spread,360);
            motion=next;
            ClearMeasure();
            SetContact(balanceDrive,true);
            return true;
        }
        private static void SetContact(SparkSwitch sw,bool on)
        {
            if(sw.IsClosed!=on) sw.SetState(on?SparkSwitchState.Closed:SparkSwitchState.Open);
        }
        private void Trip(string reason)
        {
            if(fault!=null) return;
            phaseLost|=reason=="ENCODER Z LOST" && stage==5;
            recovered=false;
            fault=reason;
            StopMachine();
        }
        private SparkTerminal Root(SparkTerminal pin)
        {
            while(nets.TryGetValue(pin,out var parent)) pin=parent;
            return pin;
        }
        private bool Routed(int i)
        {
            var (a,b)=Wires[i];
            return Root(pins[a])==Root(pins[b]);
        }
        private string CheckWires()
        {
            circuit.CopyConnections(wires);
            nets.Clear();
            foreach(var w in wires)
            {
                if(w.Kind!=SparkConnectionKind.Wire) continue;
                var a=Root(w.A);
                var b=Root(w.B);
                if(a!=b) nets[b]=a;
            }
            var unique=new HashSet<SparkTerminal>{Root(pins[0]),Root(pins[2])};
            if(unique.Count!=2) return "Separate +5 V and return.";
            for(int i=0;i<Wires.Length;i++)
            {
                var (a,b)=Wires[i];
                if(!wires.Exists(w=>w.Kind==SparkConnectionKind.Wire && (w.A==pins[a] && w.B==pins[b] || w.B==pins[a] && w.A==pins[b])))
                    return $"Connect W{i+1:00} · {pins[a].name} → {pins[b].name}.";
                if(i>=8 && !unique.Add(Root(pins[a]))) return "Separate signal and drive nets.";
            }
            return null;
        }
        private void ReadPower()
        {
            var result=solver.LatestSolveResult;
            bool High(int a,int b)=>result!=null && result.TryGetVoltage(pins[a],pins[b],out float volts) && volts is >=4.8f and <=5.2f;
            ready=powered && wiring==null && source.IsOutputActive && !source.IsOverloaded &&
                High(1,3) && High(4,5) && High(6,7) && High(8,9);
            safe=!guardOpen && safety.IsClosed && Routed(14) && High(23,3);
            lastVoltage=Mathf.Abs(motor.ElectricalState.Voltage);
            lastCurrent=Mathf.Abs(motor.ElectricalState.Current);
        }
        private void Simulate()
        {
            float drag=.096f+.0031f*omega;
            float goal=targetRpm*Mathf.PI/30;
            if(running && Routed(8))
            {
                driveClock+=Step;
                if(driveClock>=.15f)
                {
                    driveClock=0;
                    float demand=Mathf.Clamp(drag+(goal-omega)*.041f,.065f,.197f);
                    float resistance=5/demand-22.03f;
                    if(Mathf.Abs(driver.ResistanceOhms-resistance)>.12f) driver.SetResistance(resistance);
                }
            }
            omega=Mathf.Max(0,omega+((running?lastCurrent:0)-drag)/.016f*Step);
            float previous=angle;
            angle+=omega*Step;
            sinceIndex+=Step;
            while(edge<Mathf.FloorToInt(angle*80/(2*Mathf.PI)))
            {
                int code=Gray[++edge&3];
                int received=(Routed(12)?code&2:0)|(Routed(13)?code&1:0);
                if(running && ready && Delta[phase*4+received]!=1) Trip("ENCODER A/B INVALID");
                count+=Delta[phase*4+received];
                phase=code;
                if(edge%80==0 && running && Routed(18))
                {
                    if(phaseValid && count-indexCount!=80) { Trip("ENCODER INDEX COUNT INVALID"); break; }
                    FinishRevolution();
                    indexCount=count;
                    sinceIndex=0;
                    phaseValid=true;
                }
            }
            tachClock+=Step;
            if(tachClock>=.5f)
            {
                rpm=Mathf.Lerp(rpm,(count-tachCount)*60/(80*tachClock),.73f);
                tachCount=count;
                tachClock=0;
            }
            if(running && ActualRpm>40 && sinceIndex>PhaseTimeout) Trip("ENCODER Z LOST");
            if(running && (motor.IsOverCurrent || source.IsOverloaded)) Trip("DRIVE OVERLOAD");
            if(!running) return;
            if(!Settled && phaseValid && ready && safe && balanceDrive.IsClosed &&
                Mathf.Abs(actuator.ElectricalState.Current)>.025f && Routed(15))
            {
                weightA=Mathf.Repeat(Mathf.MoveTowardsAngle(weightA,goalA,82*Step),360);
                weightB=Mathf.Repeat(Mathf.MoveTowardsAngle(weightB,goalB,82*Step),360);
            }
            if(Settled) SetContact(balanceDrive,false);
            float encoderAngle=(count-indexCount)*Mathf.PI/40;
            Vector2 unbalance=Polar(.88f,71)+Polar(Mass,weightA)+Polar(Mass,weightB);
            Vector2 response=Multiply(Polar(1.58f,34),unbalance)*Mathf.Pow(ActualRpm/45,1.65f);
            vibrationSample=Routed(11)?Vector2.Dot(response,Polar(1,angle*Mathf.Rad2Deg))+.035f*Mathf.Sin(5*angle):0;
            history[historyIndex]=vibrationSample;
            historyIndex=(historyIndex+1)%history.Length;
            historyCount=Mathf.Min(historyCount+1,history.Length);
            if(!Stable || !Settled || !Routed(11)) { ClearMeasure(); return; }
            float rotation=angle-previous;
            integral+=2*vibrationSample*new Vector2(Mathf.Cos(encoderAngle),Mathf.Sin(encoderAngle))*rotation;
            square+=vibrationSample*vibrationSample*rotation;
            weightSum+=rotation;
            if(verify && Low) hold+=Step;
        }
        private void FinishRevolution()
        {
            if(Stable && Settled && weightSum>5.95f)
            {
                var observed=integral/weightSum;
                vector=revolutions==0?observed:Vector2.Lerp(vector,observed,.45f);
                rms=Mathf.Sqrt(square/weightSum);
                revolutions++;
                if(stage==3 && verify && motion==Motion.Correcting && Low && revolutions>=4 && hold>3)
                { motion=Motion.Idle; notice=null; Award(); }
                else if(stage==4 && verify && changedSpeed && targetRpm==60 && Low && revolutions>=4 && hold>2)
                    Award();
                else if(stage==5 && verify && recovered && targetRpm==60 && Low && revolutions>=4 && hold>2)
                    Award();
            }
            integral=Vector2.zero;
            square=weightSum=0;
        }
        private void ClearMeasure()
        {
            integral=Vector2.zero;
            square=weightSum=hold=0;
            revolutions=0;
        }
        private void Award()
        {
            if(stage>=Gates.Length || !bindings.SetRuntimeComponentBinding(Gates[stage],source)) return;
            stage++;
            verify=false;
            manager.QueueEvaluation();
        }
        private string Prompt()
        {
            if(fault!=null) return $"SAFE STOP · {fault} | "+(CanReset?"RESET, then START and VERIFY.":wiring??"Wait for the rotor to stop.");
            if(wiring!=null) return wiring;
            if(!safe) return "Close the guard to run.";
            if(!powered) return "Turn on POWER.";
            if(!running && stage<6) return "Press START to acquire the once-per-turn phase reference.";
            if(armed || notice!=null) return notice;
            return stage switch {
                1=>"Record the unbalanced baseline after five stable revolutions.",
                2=>"RUN TRIAL, then CALCULATE once both masses settle and the trial window is complete.",
                3=>"Select AUTO, apply AUTO BALANCE and VERIFY after the masses settle.",
                4=>"Select 60 RPM while running, then VERIFY at stable speed.",
                5 when recovered=>"VERIFY balance again with the restored Z reference.",
                5=>"Run PHASE FAULT TEST and remove W19.",
                6=>"BALANCE VERIFIED · Phase recovery complete.",
                _=>"Waiting for valid power and guard feedback."
            };
        }
        private void Present()
        {
            statusText.text=Prompt();
            float ratio=baseline.magnitude>.01f?100*vector.magnitude/baseline.magnitude:0;
            meterText.text=$"{(Stopped?0:rpm):F1}/{targetRpm} RPM  |  {lastVoltage:F2} V  |  {lastCurrent*1000:F0} mA  |  RMS {rms:F2} mm/s  |  1× {vector.magnitude:F2} mm/s";
            detailText.text=$"PHASE {(phaseValid?Phase(vector).ToString("F0")+"°":"—")}  |  A {weightA:F0}°  B {weightB:F0}°  |  RESIDUAL {ratio:F0}%  |  REV {revolutions}  |  {manager.SatisfiedTargetCount}/6";
            calibrationText.text=$"SHAFT {Mathf.Repeat(angle*Mathf.Rad2Deg,360):F0}° / {count} CT   |   Z {(phaseValid?"LOCKED":"—")}   |   BASE {baseline.magnitude:F2}   |   TRIAL {trialVector.magnitude:F2}∠{Phase(trialVector):F0}°   |   H {influence.magnitude:F2}∠{Phase(influence):F0}°   |   CORR {correction.magnitude:F2}∠{Phase(correction):F0}°";
            controllerDisplay.text=$"{(automatic?"AUTO":"MANUAL")} • {(fault!=null?"SAFE STOP":running?"RUN":"READY")}\nA {weightA:F0}°  /  B {weightB:F0}°";
            qualityDisplay.text=$"1× {vector.magnitude:F2} mm/s  /  {ratio:F0}%\n{(phaseValid?$"PHASE {Phase(vector):F0}°":"Z REFERENCE LOST")}";
            rig.SetMeasuredRPM(ActualRpm,Stopped?0:rpm);
            rig.SetMeasuredCorrectionAngles(weightA,weightB);
            rig.SetSafetyGuardOpen(guardOpen);
            rig.SetCondition(!phaseValid && running,fault!=null);
            powerButton.interactable=true;
            startButton.interactable=CanStart;
            stopButton.interactable=running;
            slowButton.interactable=Stopped && targetRpm!=45 && stage<4;
            fastButton.interactable=stage==4 && running && targetRpm==45 && Mathf.Abs(ActualRpm-45)<2;
            baselineButton.interactable=stage==1 && Stable && revolutions>=5;
            modeButton.interactable=stage<=3 && motion==Motion.Idle;
            modeButton.GetComponentInChildren<Text>().text=automatic?"MODE: AUTO":"MODE: MANUAL";
            trialButton.interactable=stage==2 && Stable && motion==Motion.Idle && revolutions>=2;
            calculateButton.interactable=stage==2 && Stable && motion==Motion.Trial && Settled && revolutions>=4;
            balanceButton.interactable=stage==3 && Stable && automatic && motion==Motion.Idle;
            verifyButton.interactable=stage is >=3 and <=5 && !verify && Stable && Settled &&
                (stage==3?motion==Motion.Correcting:stage==4?targetRpm==60:recovered);
            phaseButton.interactable=stage==5 && running && !recovered && !armed;
            guardButton.interactable=Stopped;
            guardButton.GetComponentInChildren<Text>().text=guardOpen?"CLOSE GUARD":"OPEN GUARD";
            resetButton.interactable=CanReset;
        }
        public int SampleCount=>historyCount;
        public float WaveSample(int i)=>history[(historyIndex-historyCount+i+history.Length)%history.Length];
        public Vector2 MeasuredVector=>phaseValid?vector:Vector2.zero;
        public Vector2 BaselineVector=>baseline;
        public Vector2 CorrectionA=>Polar(Mass,weightA);
        public Vector2 CorrectionB=>Polar(Mass,weightB);
        private void OnDisable()
        {
            if(gameObject.activeInHierarchy && drive!=null) StopMachine();
            if(bindings!=null) foreach(string gate in Gates) bindings.ClearRuntimeComponentBinding(gate);
            bindings=null;
        }
    }
}
