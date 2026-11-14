using System;
using System.Collections.Generic;
using ProjectSpark.Circuit;
using ProjectSpark.Electrical;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.Gameplay
{
    public sealed class SparkLevel64Synchronism : MonoBehaviour
    {
        public static readonly string[] Gates={"L64_Wired","L64_Parallel","L64_Cleared","L64_Damped","L64_Backup","L64_Recovered"};
        public static readonly (int a,int b)[] Wires={(0,1),(2,3),(0,4),(2,5),(0,6),(2,7),(0,8),(2,9),(10,11),(12,13),(14,15),(16,17),(18,19),(20,21),(22,23),(24,25),(26,27),(28,29),(30,31),(32,33),(17,34),(19,35),(21,36),(37,29),(38,31),(39,33),(17,40),(19,41),(21,42),(43,29),(44,31),(45,33),(46,47),(48,49),(50,51),(17,52),(29,53),(54,55),(56,57),(58,59),(60,61),(58,62),(60,63),(64,65),(66,67),(68,69),(70,71),(72,73),(74,75),(76,77),(78,79),(80,81),(54,82),(83,84),(83,85),(86,87),(88,89),(29,90),(31,91),(33,92),(93,94),(83,95)};
        public SparkLevelLayout layout;
        public SparkPowerSupply source;
        public SparkSwitch[] contacts;
        public SparkSwitch emergency;
        public SparkTerminal[] pins;
        public SparkLevel64VisualRig rig;
        public SparkLevel64Plot plot;
        public TMP_Text controllerDisplay,qualityDisplay;
        public Text statusText,meterText,detailText,calibrationText;
        public Button powerButton,guardButton,calibrateButton,startButton,syncButton,closeButton,timingButton,faultButton,pssButton,
            stepButton,restoreButton,backupButton,serviceButton,sensorButton,resetButton,finalButton;

        private enum Test { None, Primary, Damping, Backup, Repeat }
        private const float Dt=.005f, Mechanical=.60f, Inertia=.095f, PostTransfer=1.08f, PreTransfer=1.64f, FaultTransfer=.12f;
        private struct MachinePair
        {
            public float Angle,Slip;
            public void Step(float torque,float coupling,float damping,float dt)
            {
                Slip+=(torque-coupling*Mathf.Sin(Angle)-damping*Slip)*dt/Inertia;
                Angle+=Slip*dt;
            }
        }
        private readonly bool[] closed=new bool[6];
        private readonly List<SparkCircuitConnection> wires=new();
        private readonly HashSet<(int,int)> routed=new();
        private SparkLevelLayoutLoader loader;
        private LevelGamePlayManager manager;
        private SparkLevelSceneBindings bindings;
        private SparkElectricalSolver solver;
        private SparkCircuitSystem circuit;
        private MachinePair pair;
        private int topology=-1,stage,delaySetting;
        private float accumulator,elapsed,hold,syncHold,angleA,angleB,hzA,hzB,voltsA,voltsB,cct,clearTime,peak,
            current,transfer,pssCommand;
        private bool power=true,guard=true,ready,calibrated,running,sync,pss,injection,stuck,backupTimer,cleared,sensorArmed,
            sensorObserved,resetDone,repeatPassed,certify,needService;
        private Test test;
        private string wiring,latch,last="Bench isolated.";

        private bool Measurement=>HasWire(61);
        private bool Online=>closed[0]&&closed[1];
        private bool FeederA=>closed[2]&&closed[3];
        private bool FeederB=>closed[4]&&closed[5];
        private bool Stable=>Online&&FeederB&&Mathf.Abs(pair.Slip)<.12f&&Mathf.Abs(pair.Angle)<1.25f;
        private bool Operable=>ready&&calibrated&&Measurement&&latch==null;
        private float RelativePhase=>Online?pair.Angle*Mathf.Rad2Deg:Mathf.DeltaAngle(angleB,angleA);
        private bool Window=>Operable&&running&&sync&&!Online&&
            Mathf.Abs(hzA-hzB)<.2f&&hzA>49.8f&&Mathf.Abs(RelativePhase)<5&&Mathf.Abs(voltsA-voltsB)<.12f&&voltsB>2.25f;
        private float RelayDelay=>delaySetting switch {0=>.14f,1=>.36f,_=>.65f};

        private void Start()
        {
            loader=FindFirstObjectByType<SparkLevelLayoutLoader>();
            manager=FindFirstObjectByType<LevelGamePlayManager>();
            if(loader&&manager&&pins?.Length==96&&contacts?.Length==21)return;
            Debug.LogError("[LEVEL 64] Missing campaign services or network references.",this);
            enabled=false;
        }
        private static (int,int) Pair(int a,int b)=>(Math.Min(a,b),Math.Max(a,b));
        private bool HasWire(int i)=>routed.Contains(Pair(Wires[i].a,Wires[i].b));
        private string Inspect()
        {
            routed.Clear();
            circuit.CopyConnections(wires);
            foreach(var edge in wires)
            {
                if(edge.Kind!=SparkConnectionKind.Wire)continue;
                int a=Array.IndexOf(pins,edge.A),b=Array.IndexOf(pins,edge.B);
                if(a>=0||b>=0)routed.Add(Pair(a,b));
            }
            for(int i=0;i<Wires.Length;i++)
                if(!HasWire(i))return $"Connect W{i+1:00} · {pins[Wires[i].a].name} → {pins[Wires[i].b].name}.";
            return routed.Count==Wires.Length?null:"Remove unexpected wiring from the level sockets.";
        }
        private void Update()
        {
            if(loader.ActiveLayout!=layout||loader.ActiveBindings==null)return;
            solver=manager.ElectricalSolver;
            circuit=manager.CircuitSystem;
            if(!solver||!circuit)return;
            if(bindings!=loader.ActiveBindings){bindings=loader.ActiveBindings;topology=-1;}
            if(topology!=circuit.TopologyVersion){topology=circuit.TopologyVersion;wiring=Inspect();}
            var solved=solver.LatestSolveResult;
            bool Rail(int a,int b)=>solved!=null&&solved.TryGetVoltage(pins[a],pins[b],out var v)&&v is >=4.7f and <=5.3f;
            bool control=power&&guard&&source.IsOutputActive&&!source.IsOverloaded&&Rail(1,3)&&Rail(4,5)&&Rail(6,7)&&Rail(8,9)&&Rail(94,3);
            ready=control&&wiring==null;
            if(stage==0&&ready)Award();
            bool pmuOnly=!Measurement&&routed.Count==Wires.Length-1;
            for(int i=0;i<Wires.Length-1&&pmuOnly;i++)pmuOnly=HasWire(i);
            if((running||Online)&&(!control||wiring!=null&&!pmuOnly))
                Lockout(!guard?"SAFETY GUARD OPEN":wiring??"CONTROL POWER LOST",false);
            else if(running&&!Measurement&&latch==null)
            {
                sensorObserved|=sensorArmed;
                Lockout("PMU B ANGLE SIGNAL LOST · W62",true);
            }
            accumulator+=Mathf.Min(Time.deltaTime,.10f);
            while(accumulator>=Dt){accumulator-=Dt;Tick(Dt);}
            Present();
        }

        private void Tick(float dt)
        {
            syncHold=Window?syncHold+dt:0;
            if(!Online)
            {
                float error=Mathf.DeltaAngle(angleB,angleA);
                hzA=Mathf.MoveTowards(hzA,running?50:0,dt*28);
                hzB=Mathf.MoveTowards(hzB,running?sync?50+Mathf.Clamp(error*.045f,-.95f,.95f):49.35f:0,dt*18);
                voltsA=Mathf.MoveTowards(voltsA,running?2.4f:0,dt*3);
                voltsB=Mathf.MoveTowards(voltsB,running?(sync?2.4f:2.14f):0,dt*3);
                angleA=Mathf.Repeat(angleA+hzA*360*dt,360);
                angleB=Mathf.Repeat(angleB+hzB*360*dt,360);
                return;
            }
            elapsed+=dt;
            if(injection)RunRelays();
            float coupling=injection?FaultTransfer:FeederA&&FeederB?PreTransfer:FeederA||FeederB?PostTransfer:0;
            float torque=Mechanical+(test==Test.Damping&&elapsed<.32f?.47f:0);
            pssCommand=pss?Mathf.Clamp(-.14f*pair.Slip,-.32f,.32f):0;
            pair.Step(torque+pssCommand,coupling,.12f,dt);
            transfer=coupling*Mathf.Sin(pair.Angle);
            hzA=50+pair.Slip/(4*Mathf.PI);
            hzB=50-pair.Slip/(4*Mathf.PI);
            float targetVoltage=injection?1.52f:2.4f-.17f*Mathf.Abs(transfer);
            voltsA=Mathf.MoveTowards(voltsA,targetVoltage,dt*10);
            voltsB=Mathf.MoveTowards(voltsB,targetVoltage,dt*10);
            current=injection?2.4f/((closed[2]&&closed[3]?.52f:.78f)+.15f):Mathf.Abs(transfer)/Mathf.Max(voltsA,.2f);
            if(Mathf.Abs(pair.Angle)>2.55f)
            {
                Lockout("OUT OF STEP · source breakers isolated",false);
                return;
            }
            float equilibrium=Mathf.Asin(Mathf.Clamp(Mechanical/Mathf.Max(coupling,Mechanical),-1,1));
            float excursion=Mathf.Abs(pair.Angle-equilibrium);
            if(test==Test.Damping)peak=Mathf.Max(peak,excursion);
            bool safeClear=cleared&&clearTime<cct&&Stable&&!injection&&!FeederA;
            bool interval=stage switch
            {
                1=>Stable&&FeederA&&cct>0,
                2=>test==Test.Primary&&safeClear,
                3=>test==Test.Damping&&pss&&peak>.08f&&excursion<peak*.23f&&Stable,
                4=>test==Test.Backup&&safeClear&&backupTimer&&needService,
                5=>test==Test.Repeat&&safeClear&&sensorObserved&&resetDone,
                _=>false
            };
            hold=interval?hold+dt:0;
            if(hold<1.05f)return;
            if(stage==5)
            {
                repeatPassed=true;
                if(!certify)return;
            }
            Award();
            test=Test.None;
            elapsed=0;
        }
        private void RunRelays()
        {
            if(elapsed>=RelayDelay+.028f&&closed[2])
            {
                SetBreaker(2,false);
                if(!stuck)SetBreaker(3,false);
                else last="QFA OUT failed to open. Persistent current starts backup timer.";
            }
            if(stuck&&!closed[2]&&closed[3]&&elapsed>=RelayDelay+.118f)
            {
                // A separate local series isolator interrupts the receiving-end fault.
                Isolator(false);
                backupTimer=needService=true;
                last="Receiving-end backup isolator cleared the stuck breaker.";
                ClearFault();
            }
            if(!closed[2]&&!closed[3])ClearFault();
        }
        private void ClearFault()
        {
            injection=false;
            cleared=true;
            clearTime=elapsed;
            if(!stuck)last="Distance protection opened both Feeder A ends.";
        }

        private static bool Trial(float clearing,bool damping)
        {
            MachinePair trial=new(){Angle=Mathf.Asin(Mechanical/PreTransfer)};
            float t=0;
            while(t<clearing+9)
            {
                bool on=t<clearing;
                float pssTorque=damping?Mathf.Clamp(-.14f*trial.Slip,-.32f,.32f):0;
                trial.Step(Mechanical+pssTorque,on?FaultTransfer:PostTransfer,.12f,Dt);
                if(Mathf.Abs(trial.Angle)>2.55f)return false;
                t+=Dt;
            }
            return Mathf.Abs(trial.Slip)<.15f&&Mathf.Abs(trial.Angle-Mathf.Asin(Mechanical/PostTransfer))<.13f;
        }
        private void RecomputeCct()
        {
            float lo=0,hi=1.3f;
            for(int i=0;i<10;i++)
            {
                float midpoint=(lo+hi)*.5f;
                if(Trial(midpoint,pss))lo=midpoint;
                else hi=midpoint;
            }
            cct=lo;
        }

        private void SetBreaker(int index,bool on)
        {
            closed[index]=on;
            for(int p=0;p<3;p++)
            {
                var contact=contacts[index*3+p];
                if(contact.IsClosed!=on)contact.SetState(on?SparkSwitchState.Closed:SparkSwitchState.Open);
            }
        }
        private void Isolator(bool on)
        {
            for(int i=18;i<21;i++)contacts[i].SetState(on?SparkSwitchState.Closed:SparkSwitchState.Open);
        }
        private void Lockout(string reason,bool keepReference)
        {
            SetBreaker(1,false);
            if(!keepReference)SetBreaker(0,false);
            for(int i=2;i<6;i++)SetBreaker(i,false);
            Isolator(false);
            running=keepReference;
            sync=calibrated=injection=false;
            test=Test.None;
            latch=reason;
            last=reason;
            Debug.LogWarning("[LEVEL 64] "+reason,this);
        }
        private void Award()
        {
            if(stage>=Gates.Length||!bindings.SetRuntimeComponentBinding(Gates[stage],source))return;
            stage++;
            hold=0;
            manager.QueueEvaluation();
        }
        private void Begin(Test value)
        {
            test=value;
            elapsed=peak=hold=0;
            cleared=backupTimer=false;
            stuck=value==Test.Backup;
            injection=value!=Test.Damping;
            last=injection?"Feeder A fault current detected.":"Load step applied. PSS tracking rotor slip.";
        }

        public void Power(){power=!power;source.SetOutput(power,default);}
        public void Guard(){guard=!guard;emergency.SetState(guard?SparkSwitchState.Closed:SparkSwitchState.Open);}
        public void Calibrate(){if(ready&&!Online&&latch==null)calibrated=true;}
        public void StartMachines(){if(Operable&&!Online&&!running){running=true;angleB=145;}}
        public void Sync(){if(Operable&&running&&!Online)sync=true;}
        public void Close()
        {
            if(!Window||syncHold<.72f){last="Close inhibited until the measured synchronization window is held.";return;}
            pair=new MachinePair{Angle=Mathf.Asin(Mechanical/PreTransfer)};
            Isolator(true);
            for(int i=0;i<6;i++)SetBreaker(i,true);
            RecomputeCct();
            last="QA and QB closed within sync window.";
        }
        public void Timing(){if(!injection)delaySetting=(delaySetting+1)%3;}
        public void Fault(){if(CanFault)Begin(stage==2?Test.Primary:Test.Repeat);}
        public void Pss(){if(Online&&!injection){pss=true;RecomputeCct();}}
        public void Step(){if(Online&&Stable&&stage==3&&pss&&test==Test.None)Begin(Test.Damping);}
        public void Restore()
        {
            if(!CanRestore)return;
            Isolator(true);
            SetBreaker(2,true);
            SetBreaker(3,true);
            last="Feeder A returned to service.";
        }
        public void Backup(){if(Online&&Stable&&FeederA&&stage==4&&test==Test.None)Begin(Test.Backup);}
        public void Service()
        {
            if(!needService||injection||stage<5)return;
            SetBreaker(3,false);
            needService=stuck=false;
            last="QFA OUT mechanism repaired. Restore the feeder.";
        }
        public void Sensor(){if(CanSensor)sensorArmed=true;}
        public void Reset()
        {
            if(!ready||latch==null)return;
            latch=null;
            resetDone=sensorObserved;
            sensorArmed=false;
        }
        public void Final(){if(stage==5&&repeatPassed&&Stable&&Operable)certify=true;}

        private bool CanFault=>Operable&&Stable&&FeederA&&test==Test.None&&(stage==2||stage==5&&resetDone&&!repeatPassed);
        private bool CanRestore=>Online&&Stable&&!FeederA&&!needService&&!injection&&test==Test.None&&stage>=4;
        private bool CanSensor=>stage==5&&Operable&&Stable&&FeederA&&!sensorArmed&&!sensorObserved;

        private string Prompt()
        {
            if(stage==6)return "CERTIFIED · Synchronism, protection and PMU recovery verified.";
            if(latch!=null)return latch+(ready?" · reset and recommission.":" · restore input before reset.");
            if(wiring!=null)return wiring;
            if(!ready)return "Restore the 5 V controls and close the guard.";
            if(!calibrated)return "Calibrate encoders, PMUs and current channels.";
            if(!running)return "Start both generators.";
            if(!Online)return syncHold>=.72f?"SYNC WINDOW HELD · close sources.":sync?"Acquiring the measured synchronism window.":"Select AUTO SYNC.";
            if(injection)return last;
            if(test==Test.Damping)return "Watch PSS reduce the rotor oscillation.";
            if(test!=Test.None&&!repeatPassed)return "Verifying post-fault synchronism.";
            return stage switch
            {
                2=>"Compare relay clearing delay with CCT. Apply FEEDER A FAULT.",
                3=>pss?"Apply LOAD STEP.":"Enable PSS, then apply a load step.",
                4=>FeederA?"Apply BREAKER FAILURE.":"Restore Feeder A.",
                5=>needService?"Service QFA OUT, then restore the feeder.":!FeederA&&!repeatPassed?"Restore Feeder A.":
                    repeatPassed?"Repeat fault passed. Select CERTIFY.":resetDone?"Repeat FEEDER A FAULT.":sensorArmed?"Disconnect W62 · PMU B ANGLE → PMU B IN.":"Select W62 TEST.",
                _=>last
            };
        }
        private void Present()
        {
            statusText.text=Prompt();
            meterText.text=$"A {hzA:F2} Hz / {voltsA:F2} V  ·  B {hzB:F2} Hz / {voltsB:F2} V  ·  Δθ {(Measurement?RelativePhase.ToString("F1")+"°":"INVALID")}";
            detailText.text=$"LINE {current:F2} A  ·  TRANSFER {transfer:F2} pu  ·  PSS {pssCommand:+0.00;-0.00;0.00} pu  ·  CLEAR {clearTime*1000:F0} ms  /  CCT {cct*1000:F0} ms";
            calibrationText.text=$"{manager.SatisfiedTargetCount}/6  ·  {(FeederA?"FA ON":"FA OPEN")}  ·  {(FeederB?"FB ON":"FB OPEN")}  ·  {last}";
            if(controllerDisplay)controllerDisplay.text=$"{hzA:F2} / {hzB:F2} Hz";
            if(qualityDisplay)qualityDisplay.text=$"CCT {cct*1000:F0} ms / Δθ {RelativePhase:F0}°";
            if(plot)plot.Show(RelativePhase,hzA-hzB);
            if(rig)rig.Apply(closed,guard,hzA,hzB,Time.deltaTime);
            powerButton.interactable=guardButton.interactable=true;
            calibrateButton.interactable=ready&&!calibrated&&!Online&&latch==null;
            startButton.interactable=Operable&&!running;
            syncButton.interactable=Operable&&running&&!sync&&!Online;
            closeButton.interactable=Operable&&running&&!Online;
            timingButton.interactable=Online&&test==Test.None;
            timingButton.GetComponentInChildren<Text>().text=$"RELAY {RelayDelay*1000:F0} ms";
            faultButton.interactable=CanFault;
            pssButton.interactable=Online&&stage>=3&&!pss&&!injection;
            stepButton.interactable=Online&&Stable&&stage==3&&pss&&test==Test.None;
            restoreButton.interactable=CanRestore;
            backupButton.interactable=Online&&Stable&&FeederA&&stage==4&&test==Test.None;
            serviceButton.interactable=needService&&!injection&&stage>=5;
            sensorButton.interactable=CanSensor;
            resetButton.interactable=ready&&latch!=null;
            finalButton.interactable=stage==5&&repeatPassed&&Stable&&Operable&&!certify;
        }
        private void OnDisable()
        {
            if(bindings!=null)foreach(string gate in Gates)bindings.ClearRuntimeComponentBinding(gate);
        }
    }
}

