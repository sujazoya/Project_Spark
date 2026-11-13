using System.Collections.Generic;
using ProjectSpark.Circuit;
using ProjectSpark.Electrical;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.Gameplay
{
    [DisallowMultipleComponent]
    public sealed class SparkLevel53Couple : MonoBehaviour
    {

        public static readonly string[] Gates={"L53_Wired","L53_Couple","L53_TrialA","L53_TrialB","L53_Balanced","L53_Recovered"};
        public static readonly (int a,int b)[] Wires={(0, 1),(2, 3),(0, 4),(2, 5),(0, 6),(2, 7),(0, 8),(2, 9),(0, 10),(2, 11),(12, 13),(14, 15),(16, 17),(18, 19),(20, 21),(22, 23),(24, 25),(26, 27),(28, 29),(30, 31),(32, 33),(34, 35),(36, 37),(38, 39)};
        private const float Step=.01f, Capacity=1.10f, PhaseTimeout=1.95f;
        private static readonly int[] Gray={0,2,3,1};
        private static readonly int[] Delta={0,-1,1,0,1,0,0,-1,-1,0,0,1,0,1,-1,0};
        private enum Motion { Idle, TrialA, Neutral, TrialB, Correcting }

        public SparkLevelLayout layout;
        public SparkPowerSupply source;
        public SparkMotor motor;
        public SparkResistor driver,actuatorA,actuatorB;
        public SparkSwitch drive,driveA,driveB,safety;
        public Transform massAArt,massBArt;
        public SparkTerminal[] pins;
        public SparkLevel53VisualRig rig;
        public TMP_Text controllerDisplay,qualityDisplay;
        public Text statusText,meterText,detailText,calibrationText;
        public Button powerButton,slowButton,fastButton,startButton,stopButton,baselineButton,modeButton,trialAButton,trialBButton;
        public Button solveButton,applyButton,verifyButton,phaseButton,guardButton,resetButton;

        private readonly List<SparkCircuitConnection> wires=new();
        private readonly Dictionary<SparkTerminal,SparkTerminal> nets=new();
        private SparkLevelLayoutLoader loader;
        private LevelGamePlayManager manager;
        private SparkLevelSceneBindings bindings;
        private SparkCircuitSystem circuit;
        private SparkElectricalSolver solver;
        private Motion motion;
        private int topology=-1,stage,targetRpm=45,edge,count,phase,indexCount,tachCount,revolutions;

        private float accumulator,omega,angle,driveClock,tachClock,rpm,sinceIndex,weightA=35,weightB=225,goalA=35,goalB=225;
        private float radiusA,radiusB,goalRadiusA,goalRadiusB,square,rightSquare,weightSum,rms,rightRms,hold,lastVoltage,lastCurrent;
        private Vector2 integral,rightIntegral,vector,rightVector,baseline,rightBaseline,correctionA,correctionB,hLA,hRA,hLB,hRB;
        private Vector3 artA,artB;
        private bool artReady,powered=true,running,ready,safe,guardOpen,phaseValid,armed,automatic,verify,changedSpeed,sensorLost,recovered,certified;
        private string wiring,fault,notice;
        private static readonly Vector2 TrialVector=Polar(.48f,23);
        private static readonly Vector2 PhysicalLA=Polar(1.61f,26),PhysicalRA=Polar(.52f,-13);
        private static readonly Vector2 PhysicalLB=Polar(.54f,21),PhysicalRB=Polar(1.57f,-22);
        private static readonly Vector2 UnbalanceA=Polar(.81f,49),UnbalanceB=Polar(.78f,232);

        private float ActualRpm=>omega*60/(2*Mathf.PI);
        private bool Stopped=>!running && !drive.IsClosed && ActualRpm<.5f;
        private bool Stable=>running && phaseValid && Mathf.Abs(rpm-targetRpm)<1.5f && Mathf.Abs(ActualRpm-targetRpm)<1.5f;
        private bool Settled=>Mathf.Abs(Mathf.DeltaAngle(weightA,goalA))<.15f && Mathf.Abs(Mathf.DeltaAngle(weightB,goalB))<.15f && Mathf.Abs(radiusA-goalRadiusA)<.001f && Mathf.Abs(radiusB-goalRadiusB)<.001f;

        private bool Low=>baseline.magnitude>.1f && rightBaseline.magnitude>.1f &&
            vector.magnitude<=Mathf.Min(.35f,baseline.magnitude*.30f) &&
            rightVector.magnitude<=Mathf.Min(.35f,rightBaseline.magnitude*.30f);
        private bool CanStart=>Stopped && ready && safe && fault==null;
        private bool CanReset=>Stopped && ready && safe && Routed(14) && fault!=null;

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
                Trip(!Routed(14)?"RIGHT SENSOR FEEDBACK LOST":!safe?"GUARD INTERLOCK LOST":wiring??"POWER OR SENSOR FAULT");
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
            if(!running || stage!=5 || Mathf.Abs(ActualRpm-45)>2) return;
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
                goalA=weightA; goalRadiusA=radiusA;
                goalB=weightB; goalRadiusB=radiusB;
            }
            motion=Motion.Idle;
            SetContact(drive,false);
            SetContact(driveA,false); SetContact(driveB,false);
        }

        public void ToggleMode() { if(motion==Motion.Idle) automatic=!automatic; }
        public void Baseline()
        {
            if(stage!=1 || !Stable || revolutions<5 || radiusA>.005f || radiusB>.005f ||
                vector.magnitude<.5f || rightVector.magnitude<.5f || Vector2.Dot(vector.normalized,rightVector.normalized)>-.25f) return;
            // Each bearing defines a disk of acceptable corrections in a single
            // plane. Disjoint disks rule out even an unlimited single actuator.
            bool Disjoint(Vector2 leftResponse,Vector2 rightResponse)
            {
                Vector2 leftCenter=Divide(-vector,leftResponse);
                Vector2 rightCenter=Divide(-rightVector,rightResponse);
                float tolerance=.30f*(vector.magnitude/leftResponse.magnitude+rightVector.magnitude/rightResponse.magnitude);
                return Vector2.Distance(leftCenter,rightCenter)>tolerance;
            }
            if(!Disjoint(PhysicalLA,PhysicalRA) || !Disjoint(PhysicalLB,PhysicalRB)) return;
            baseline=vector;
            rightBaseline=rightVector;
            Award();
        }
        public void TrialA()
        {
            if(stage==2 && Stable && motion==Motion.Idle) Command(TrialVector,Vector2.zero,Motion.TrialA);
        }
        public void TrialB()
        {
            if(stage==3 && Stable && Settled && motion==Motion.Idle) Command(Vector2.zero,TrialVector,Motion.TrialB);
        }
        public void Solve()
        {
            if(stage!=3 || !Stable || motion!=Motion.TrialB || !Settled || revolutions<4) return;
            hLB=Divide(vector-baseline,TrialVector);
            hRB=Divide(rightVector-rightBaseline,TrialVector);
            Vector2 determinant=Multiply(hLA,hRB)-Multiply(hLB,hRA);
            float scale=Mathf.Sqrt((hLA.sqrMagnitude+hRA.sqrMagnitude)*(hLB.sqrMagnitude+hRB.sqrMagnitude));
            if(scale<.10f || determinant.magnitude<.20f*scale) { notice="Influence matrix is poorly conditioned."; return; }
            correctionA=Divide(Multiply(hLB,rightBaseline)-Multiply(hRB,baseline),determinant);
            correctionB=Divide(Multiply(hRA,baseline)-Multiply(hLA,rightBaseline),determinant);
            if(!(correctionA.magnitude<=Capacity && correctionB.magnitude<=Capacity))
            { notice="Required correction exceeds actuator travel."; return; }
            motion=Motion.Idle;
            Award();
        }
        public void Apply()
        {
            if(stage==4 && Stable && automatic && motion==Motion.Idle) Command(correctionA,correctionB,Motion.Correcting);
        }
        public void Verify()
        {
            if(!Stable || !Settled || !(stage==4 && motion==Motion.Correcting || stage==5 && targetRpm==60)) return;
            verify=true;
            ClearMeasure();
        }
        public void PhaseTest()
        {
            if(stage!=5 || !certified || !running) return;
            armed=true;
            notice="Undo W15 now. Restore right sensor feedback after safe stop.";
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
            recovered=sensorLost;
            notice=null;
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

        private void Command(Vector2 a,Vector2 b,Motion next)
        {
            if(!phaseValid || !ready || !safe || !Routed(18) || a.magnitude>Capacity || b.magnitude>Capacity) return;
            goalA=a.sqrMagnitude>.00001f?Phase(a):weightA;
            goalB=b.sqrMagnitude>.00001f?Phase(b):weightB;
            goalRadiusA=a.magnitude;
            goalRadiusB=b.magnitude;
            motion=next;
            ClearMeasure();
            SetContact(driveA,true);
            SetContact(driveB,true);
        }
        private static void SetContact(SparkSwitch sw,bool on)
        {
            if(sw.IsClosed!=on) sw.SetState(on?SparkSwitchState.Closed:SparkSwitchState.Open);
        }
        private void Trip(string reason)
        {
            if(fault!=null) return;
            sensorLost|=reason=="RIGHT SENSOR FEEDBACK LOST" && stage==5 && certified;
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
                if(i>=10 && !unique.Add(Root(pins[a]))) return "Separate signal and drive nets.";
            }
            return null;
        }
        private void ReadPower()
        {
            var result=solver.LatestSolveResult;
            bool High(int a,int b)=>result!=null && result.TryGetVoltage(pins[a],pins[b],out float volts) && volts is >=4.8f and <=5.2f;
            ready=powered && wiring==null && source.IsOutputActive && !source.IsOverloaded &&
                High(1,3) && High(4,5) && High(6,7) && High(8,9) && High(10,11);
            safe=!guardOpen && safety.IsClosed && Routed(23) && High(39,3);
            lastVoltage=Mathf.Abs(motor.ElectricalState.Voltage);
            lastCurrent=Mathf.Abs(motor.ElectricalState.Current);
        }
        private void Simulate()
        {
            float drag=.096f+.0031f*omega;
            float goal=targetRpm*Mathf.PI/30;
            if(running && Routed(10))
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
                int received=(Routed(15)?code&2:0)|(Routed(16)?code&1:0);
                if(running && ready && Delta[phase*4+received]!=1) Trip("ENCODER A/B INVALID");
                count+=Delta[phase*4+received];
                phase=code;
                if(edge%80==0 && running && Routed(17))
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

            if(phaseValid && ready && safe && Routed(18))
            {
                if(driveA.IsClosed && Mathf.Abs(actuatorA.ElectricalState.Current)>.025f)
                {
                    weightA=Mathf.Repeat(Mathf.MoveTowardsAngle(weightA,goalA,85*Step),360);
                    radiusA=Mathf.MoveTowards(radiusA,goalRadiusA,.90f*Step);
                }
                if(driveB.IsClosed && Mathf.Abs(actuatorB.ElectricalState.Current)>.025f)
                {
                    weightB=Mathf.Repeat(Mathf.MoveTowardsAngle(weightB,goalB,85*Step),360);
                    radiusB=Mathf.MoveTowards(radiusB,goalRadiusB,.90f*Step);
                }
            }
            if(Settled) { SetContact(driveA,false); SetContact(driveB,false); }
            if(!Stable || !Settled || !Routed(13) || !Routed(14)) { ClearMeasure(); return; }
            float encoderAngle=(count-indexCount)*Mathf.PI/40;
            Vector2 a=UnbalanceA+Polar(radiusA,weightA), b=UnbalanceB+Polar(radiusB,weightB);
            float gain=Mathf.Pow(ActualRpm/45,1.65f);
            Vector2 bearingL=(Multiply(PhysicalLA,a)+Multiply(PhysicalLB,b))*gain;
            Vector2 bearingR=(Multiply(PhysicalRA,a)+Multiply(PhysicalRB,b))*gain;
            var rotor=new Vector2(Mathf.Cos(angle),Mathf.Sin(angle));
            float left=Vector2.Dot(bearingL,rotor)+.025f*Mathf.Sin(5*angle);
            float right=Vector2.Dot(bearingR,rotor)+.025f*Mathf.Sin(7*angle);
            float rotation=angle-previous;
            var basis=new Vector2(Mathf.Cos(encoderAngle),Mathf.Sin(encoderAngle));
            integral+=2*left*basis*rotation;
            rightIntegral+=2*right*basis*rotation;
            square+=left*left*rotation;
            rightSquare+=right*right*rotation;
            weightSum+=rotation;
            if(verify && Low) hold+=Step;
        }
        private void FinishRevolution()
        {
            if(Stable && Settled && weightSum>5.95f)
            {
                var observed=integral/weightSum;
                var observedRight=rightIntegral/weightSum;
                vector=revolutions==0?observed:Vector2.Lerp(vector,observed,.45f);
                rightVector=revolutions==0?observedRight:Vector2.Lerp(rightVector,observedRight,.45f);
                rms=Mathf.Sqrt(square/weightSum);
                rightRms=Mathf.Sqrt(rightSquare/weightSum);
                revolutions++;
                if(stage==2 && motion==Motion.TrialA && revolutions>=4)
                {
                    hLA=Divide(vector-baseline,TrialVector);
                    hRA=Divide(rightVector-rightBaseline,TrialVector);
                    if(hLA.magnitude>.15f && hRA.magnitude>.12f) Command(Vector2.zero,Vector2.zero,Motion.Neutral);
                }
                else if(stage==2 && motion==Motion.Neutral && revolutions>=1)
                { motion=Motion.Idle; Award(); }
                else if(stage==4 && motion==Motion.Correcting && verify && Low && revolutions>=4 && hold>2)
                { motion=Motion.Idle; Award(); }
                else if(stage==5 && verify && changedSpeed && targetRpm==60 && Low && revolutions>=4 && hold>2)
                {
                    verify=false;
                    if(recovered) Award(); else certified=true;
                }
            }
            integral=rightIntegral=Vector2.zero;
            square=rightSquare=weightSum=0;
        }
        private void ClearMeasure()
        {
            integral=rightIntegral=Vector2.zero;
            square=rightSquare=weightSum=hold=0;
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
            if(!safe) return "Close the guard and check W24.";
            if(!powered) return "Turn on POWER.";
            if(!running && stage<6) return "Press START to acquire encoder Z.";
            if(notice!=null) return notice;
            return stage switch {
                1=>"At 45 RPM, record five valid revolutions and compare the bearing phases.",
                2=>motion==Motion.Idle?"Run TRIAL A. Both bearings measure the response.":"Measuring trial A and returning the plane to neutral.",
                3=>motion==Motion.Idle?"Run TRIAL B.":"Wait for trial B to settle, then SOLVE.",
                4=>motion==Motion.Idle?"Select AUTO and APPLY the calculated corrections.":"Wait for both corrections to settle, then VERIFY.",
                5 when recovered=>"VERIFY both bearings at 60 RPM again.",
                5 when certified=>"Use SENSOR FAULT TEST, then undo W15.",
                5=>"Select 60 RPM while running and VERIFY.",
                6=>"CERTIFIED · Both bearings passed after right sensor recovery.",
                _=>"Waiting for verified rails and guard feedback."
            };
        }
        private void Present()
        {
            statusText.text=Prompt();
            meterText.text=$"{(Stopped?0:rpm):F1}/{targetRpm} RPM  |  {lastCurrent*1000:F0} mA  |  L {rms:F2} RMS / {vector.magnitude:F2} 1×  |  R {rightRms:F2} RMS / {rightVector.magnitude:F2} 1× mm/s";
            detailText.text=$"L {Phase(vector):F0}°  R {Phase(rightVector):F0}°  |  A {radiusA:F2}∠{weightA:F0}°  B {radiusB:F2}∠{weightB:F0}°  |  Z {(phaseValid?"LOCK":"—")}  |  GUARD {(safe?"OK":"OPEN")}  |  {manager.SatisfiedTargetCount}/6";
            string C(Vector2 v)=>$"{v.magnitude:F2}∠{Phase(v):F0}°";
            Vector2 predictL=baseline+Multiply(hLA,correctionA)+Multiply(hLB,correctionB);
            Vector2 predictR=rightBaseline+Multiply(hRA,correctionA)+Multiply(hRB,correctionB);
            calibrationText.text=stage==2 && motion==Motion.Idle
                ? $"COUPLE · BASELINE PHASE SPLIT {Vector2.Angle(baseline,rightBaseline):F0}° · NEITHER SINGLE PLANE CAN MEET BOTH LIMITS"
                : $"H [{C(hLA)}, {C(hLB)}; {C(hRA)}, {C(hRB)}]  |  PRED L/R {predictL.magnitude:F2}/{predictR.magnitude:F2}  |  SHAFT {Mathf.Repeat(angle*Mathf.Rad2Deg,360):F0}°";
            controllerDisplay.text=$"{(automatic?"AUTO":"MANUAL")} • {(fault!=null?"SAFE STOP":running?"RUN":"READY")}\nA {C(Polar(radiusA,weightA))} / B {C(Polar(radiusB,weightB))}";
            qualityDisplay.text=$"L {C(vector)}   R {C(rightVector)}\n{(phaseValid?"Z LOCKED":"Z UNLOCKED")}";
            rig.SetMeasuredRPM(ActualRpm,Stopped?0:rpm);
            rig.SetMeasuredShaftAngle(angle*Mathf.Rad2Deg);
            rig.SetCorrectionAngles(weightA,weightB);
            rig.SetGuardOpen(guardOpen);
            rig.SetSensorValidity(Routed(13),Routed(14));
            rig.SetSafetyCondition(safe && fault==null);
            if(!artReady) { artA=massAArt.localScale; artB=massBArt.localScale; artReady=true; }
            massAArt.localScale=Vector3.Scale(artA,new Vector3(1,.15f+.85f*radiusA/Capacity,.15f+.85f*radiusA/Capacity));
            massBArt.localScale=Vector3.Scale(artB,new Vector3(1,.15f+.85f*radiusB/Capacity,.15f+.85f*radiusB/Capacity));
            powerButton.interactable=true;
            startButton.interactable=CanStart;
            stopButton.interactable=running;
            slowButton.interactable=Stopped && targetRpm!=45 && stage<5;
            fastButton.interactable=stage==5 && running && targetRpm==45 && Mathf.Abs(ActualRpm-45)<2;
            baselineButton.interactable=stage==1 && Stable && revolutions>=5;
            modeButton.interactable=motion==Motion.Idle;
            modeButton.GetComponentInChildren<Text>().text=automatic?"MODE: AUTO":"MODE: MANUAL";
            trialAButton.interactable=stage==2 && Stable && motion==Motion.Idle;
            trialBButton.interactable=stage==3 && Stable && motion==Motion.Idle;
            solveButton.interactable=stage==3 && Stable && motion==Motion.TrialB && Settled && revolutions>=4;
            applyButton.interactable=stage==4 && Stable && automatic && motion==Motion.Idle;
            verifyButton.interactable=!verify && Stable && Settled &&
                (stage==4 && motion==Motion.Correcting || stage==5 && targetRpm==60 && (!certified || recovered));
            phaseButton.interactable=stage==5 && running && certified && !recovered && !armed;
            guardButton.interactable=Stopped;
            guardButton.GetComponentInChildren<Text>().text=guardOpen?"CLOSE GUARD":"OPEN GUARD";
            resetButton.interactable=CanReset;
        }
        private void OnDisable()
        {
            if(gameObject.activeInHierarchy && drive!=null) StopMachine();
            if(bindings!=null) foreach(string gate in Gates) bindings.ClearRuntimeComponentBinding(gate);
            bindings=null;
        }
    }
}
