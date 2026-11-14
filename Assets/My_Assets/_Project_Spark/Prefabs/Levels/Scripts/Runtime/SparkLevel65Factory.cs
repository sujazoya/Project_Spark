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
    public sealed class SparkLevel65Factory : MonoBehaviour
    {
        public static readonly string[] Gates={"L65_Wired","L65_Vision","L65_Sorted","L65_Rejected","L65_Throughput","L65_Certified"};
        public static readonly (int a,int b)[] Wires={(0,1),(2,3),(0,4),(2,5),(0,6),(2,7),(0,8),(2,9),(0,10),(2,11),(0,12),(2,13),(0,14),(2,15),(0,16),(2,17),(18,19),(20,21),(18,22),(20,23),(24,25),(26,27),(18,28),(18,29),(20,30),(31,32),(31,33),(18,34),(35,36),(37,38),(39,40),(39,41),(42,43),(44,45),(46,47),(46,48),(46,49),(50,51),(52,53),(54,55),(56,57),(58,59),(60,61),(62,63),(62,64),(65,66),(67,68),(69,70),(71,72),(73,74),(75,76),(77,78),(79,80),(79,81),(79,82),(83,84),(85,86),(87,88),(89,90),(91,92),(91,93),(94,95),(96,97),(98,99),(100,101),(102,103)};
        public SparkLevelLayout layout;
        public SparkPowerSupply source;
        public SparkSwitch[] contacts;
        public SparkSwitch emergency;
        public SparkTerminal[] pins;
        public SparkLevel65VisualRig rig;
        public TMP_Text controllerDisplay,qualityDisplay;
        public Text statusText,meterText,detailText,calibrationText;
        public Button powerButton,commissionButton,calibrateButton,visionButton,trialButton,homeButton,servicesButton,sortButton,
            rejectButton,speedButton,cycleButton,mixedButton,faultButton,resetButton,finalButton,guardButton,stopButton,curtainButton;

        private enum Job { None, Trial, Sort, Reject, Mixed, Final }
        private enum Motion { Feed, Pick, Carry, Divert, ToBin, Retract }
        private const float ScanSeconds=.02f, PulsesPerUnit=960f;
        private readonly List<SparkCircuitConnection> edges=new();
        private readonly HashSet<(int,int)> wired=new();
        private readonly int[] counts=new int[4];
        private SparkLevelLayoutLoader loader;
        private LevelGamePlayManager manager;
        private SparkLevelSceneBindings bindings;
        private SparkElectricalSolver solver;
        private SparkCircuitSystem circuit;
        private GameObject part;
        private int stage,topology=-1,kind,classification,spawned,finished,correct,lost,escapes,batchSize,encoder;
        private float accumulator,elapsed,itemTime,position,travel,belt,vacuum,air,gate,confidence,scanTravel;
        private Vector3 routeStart;
        private bool power=true,door=true,stop=false,curtain=true,ready,controls,commissioned,calibrated,camera,homed,homing,services,
            fastBelt,fastRobot,observedEntry,attached,faultArmed,faultObserved,resetAfterRepair;
        private Job job;
        private Motion motion;
        private string wiring,latch,notice="Connect the factory network.";

        private bool EncoderValid=>Routed(65);
        private bool CanRestart=>ready&&calibrated&&latch==null&&commissioned;
        private bool Automation=>CanRestart&&camera&&homed&&services&&air>.83f;
        private float Ppm=>elapsed>.01f?60*finished/elapsed:0;
        private bool HighPerformance=>fastBelt&&fastRobot;

        private void Start()
        {
            loader=FindFirstObjectByType<SparkLevelLayoutLoader>();
            manager=FindFirstObjectByType<LevelGamePlayManager>();
            if(loader&&manager&&pins?.Length==104&&rig&&rig.products?.Length==4)return;
            Debug.LogError("[LEVEL 65] Missing factory layout services, products or bindings.",this);
            enabled=false;
        }
        private static (int,int) Pair(int a,int b)=>(Math.Min(a,b),Math.Max(a,b));
        private bool Routed(int index)=>wired.Contains(Pair(Wires[index].a,Wires[index].b));
        private string Inspect()
        {
            wired.Clear();
            circuit.CopyConnections(edges);
            foreach(var e in edges)
            {
                if(e.Kind!=SparkConnectionKind.Wire)continue;
                int a=Array.IndexOf(pins,e.A),b=Array.IndexOf(pins,e.B);
                if(a>=0||b>=0)wired.Add(Pair(a,b));
            }
            for(int i=0;i<Wires.Length;i++)
                if(!Routed(i))return $"Connect W{i+1:00} · {pins[Wires[i].a].name} → {pins[Wires[i].b].name}.";
            return wired.Count==Wires.Length?null:"Remove unexpected wires from the factory sockets.";
        }
        private void Update()
        {
            if(loader.ActiveLayout!=layout||loader.ActiveBindings==null)return;
            solver=manager.ElectricalSolver;
            circuit=manager.CircuitSystem;
            if(!solver||!circuit)return;
            if(bindings!=loader.ActiveBindings){bindings=loader.ActiveBindings;topology=-1;}
            if(topology!=circuit.TopologyVersion){topology=circuit.TopologyVersion;wiring=Inspect();}
            var result=solver.LatestSolveResult;
            bool Rail(int a,int b)=>result!=null&&result.TryGetVoltage(pins[a],pins[b],out var v)&&v is >=4.7f and <=5.3f;
            controls=power&&source.IsOutputActive&&!source.IsOverloaded;
            for(int i=0;i<8&&controls;i++)
            {
                var a=Wires[i*2]; var b=Wires[i*2+1];
                controls=Rail(a.b,b.b);
            }
            bool safe=door&&!stop&&curtain&&Rail(92,3)&&Rail(93,3)&&Rail(95,3)&&Rail(97,3);
            ready=controls&&safe&&wiring==null;
            if(stage==0&&ready)Award();
            if(commissioned&&latch==null&&!ready)
            {
                bool encoderOnly=controls&&safe&&!EncoderValid&&wired.Count==Wires.Length-1;
                for(int i=0;i<Wires.Length-1&&encoderOnly;i++)encoderOnly=Routed(i);
                if(encoderOnly)faultObserved|=faultArmed;
                Trip(encoderOnly?"CONVEYOR ENCODER LOST · W66":!safe?"SAFETY INTERLOCK OPEN":wiring??"CONTROL POWER LOST");
            }
            scanTravel=0;
            accumulator+=Mathf.Min(Time.deltaTime,.12f);
            while(accumulator>=ScanSeconds){accumulator-=ScanSeconds;Scan(ScanSeconds);}
            Present();
        }

        private void Scan(float dt)
        {
            bool run=CanRestart&&(job!=Job.None||faultArmed);
            float target=run?(fastBelt?.88f:.44f):0;
            belt=Mathf.MoveTowards(belt,target,dt*(run?2.8f:12f));
            scanTravel+=belt*dt;
            if(EncoderValid&&calibrated)
            {
                travel+=belt*dt;
                encoder=Mathf.FloorToInt(travel*PulsesPerUnit);
            }
            air=Mathf.MoveTowards(air,services&&ready&&latch==null?1:0,dt*2.4f);
            vacuum=Mathf.MoveTowards(vacuum,ready&&latch==null&&(attached||job!=Job.None&&motion==Motion.Pick)?1:0,dt*3.7f);
            gate=Mathf.MoveTowards(gate,ready&&air>.78f&&job!=Job.None&&motion==Motion.Divert?1:0,dt*5);
            if(homing&&ready&&latch==null&&rig.Home(dt*190)){homing=false;homed=true;}
            if(!run||job==Job.None)return;
            elapsed+=dt;
            itemTime+=dt;
            if(!part&&motion!=Motion.Retract)Spawn();
            switch(motion)
            {
                case Motion.Feed:
                    position=Mathf.MoveTowards(position,1,belt*dt);
                    part.transform.localPosition=position<.52f?Vector3.Lerp(rig.feed,rig.vision,position/.52f):
                        Vector3.Lerp(rig.vision,rig.pick,(position-.52f)/.48f);
                    if(position>=.28f)observedEntry=true;
                    if(position>=.52f&&classification<0)InspectPart();
                    if(position<1)break;
                    if(job==Job.Trial)
                    {
                        if(observedEntry&&classification==kind&&encoder>120){Award();EndJob();}
                        else Trip("VISION COMMISSIONING FAILED");
                    }
                    else if(classification<0)Trip("VISION CONFIDENCE TOO LOW");
                    else SwitchMotion(classification==3?Motion.Divert:Motion.Pick);
                    break;
                case Motion.Pick:
                    bool nearPart=rig.MoveTool(rig.pick,(fastRobot?200:92)*dt);
                    if(nearPart&&vacuum>.83f)
                    {
                        part.transform.SetParent(rig.gripper,true);
                        attached=true;
                        SwitchMotion(Motion.Carry);
                    }
                    else if(itemTime>1.4f)Trip("PICKUP OR VACUUM TIMEOUT");
                    break;
                case Motion.Carry:
                    var destination=rig.DropTarget(classification);
                    bool nearDestination=rig.MoveTool(destination,(fastRobot?210:105)*dt);
                    if(nearDestination)
                    {
                        attached=false;
                        part.transform.SetParent(rig.bench,true);
                        routeStart=part.transform.localPosition;
                        SwitchMotion(Motion.ToBin);
                    }
                    else if(itemTime>1.55f)Trip("ROBOT PATH TIMEOUT");
                    break;
                case Motion.Divert:
                    part.transform.localPosition=Vector3.Lerp(rig.pick,rig.gatePosition,Mathf.Clamp01(itemTime/.37f));
                    if(gate>.96f&&itemTime>=.37f){routeStart=part.transform.localPosition;SwitchMotion(Motion.ToBin);}
                    else if(itemTime>.9f)Trip("REJECT GATE TIMEOUT");
                    break;
                case Motion.ToBin:
                    float t=Mathf.Clamp01(itemTime/(fastBelt?.43f:.78f));
                    part.transform.localPosition=t<.42f?Vector3.Lerp(routeStart,rig.sortStarts[classification],t/.42f):
                        Vector3.Lerp(rig.sortStarts[classification],rig.bins[classification],(t-.42f)/.58f);
                    if(t>=1&&Vector3.Distance(part.transform.localPosition,rig.bins[classification])<.008f)
                    {
                        counts[classification]++;
                        finished++;
                        if(kind==classification)correct++;
                        if(kind==3&&classification!=3)escapes++;
                        Destroy(part);part=null;
                        SwitchMotion(Motion.Retract);
                    }
                    break;
                case Motion.Retract:
                    bool parked=rig.Home(dt*(fastRobot?335:155));
                    if(parked&&gate<.05f)
                    {
                        if(finished>=batchSize)Complete();
                        else SwitchMotion(Motion.Feed);
                    }
                    break;
            }
        }

        private void InspectPart()
        {
            if(!camera||!observedEntry||!EncoderValid)return;
            // The virtual camera measures the visible part material. The PLC compares the observation against its calibrated references.
            var material=part.GetComponentInChildren<Renderer>().sharedMaterial;
            Color sample=material.GetColor("_BaseColor");
            Color[] references={new(.933f,.282f,.361f),new(.086f,.443f,.886f),new(1,.659f,.169f),new(.635f,.510f,1)};
            float best=float.PositiveInfinity;
            for(int i=0;i<references.Length;i++)
            {
                Color c=references[i];
                float distance=Mathf.Abs(sample.r-c.r)+Mathf.Abs(sample.g-c.g)+Mathf.Abs(sample.b-c.b);
                if(distance<best){best=distance;classification=i;}
            }
            confidence=Mathf.Clamp01(1-best*.7f-Mathf.Max(0,belt-.80f)*.62f);
            if(confidence<.72f)classification=-1;
        }
        private void Spawn()
        {
            kind=job switch {Job.Trial=>0,Job.Reject=>3,Job.Sort=>spawned,_=>(spawned*3+spawned/4)%4};
            spawned++;
            part=rig.CreatePart(kind);
            part.transform.localPosition=rig.feed;
            classification=-1;
            observedEntry=false;
            position=0;
            confidence=0;
        }
        private void SwitchMotion(Motion next){motion=next;itemTime=0;}
        private void Begin(Job next,int amount)
        {
            job=next;
            batchSize=amount;
            spawned=finished=correct=lost=escapes=0;
            elapsed=0;
            Array.Clear(counts,0,counts.Length);
            SwitchMotion(Motion.Feed);
        }
        private void Complete()
        {
            bool accurate=correct==batchSize&&finished==batchSize&&lost==0&&escapes==0;
            bool gates=job switch
            {
                Job.Sort=>counts[0]>0&&counts[1]>0&&counts[2]>0,
                Job.Reject=>counts[3]==batchSize,
                Job.Mixed=>Ppm>=12&&HighPerformance,
                Job.Final=>Ppm>=12&&batchSize==20&&resetAfterRepair,
                _=>false
            };
            notice=$"Batch {finished}/{batchSize} · {correct} correct · {Ppm:F1} parts/min.";
            Debug.Log("[LEVEL 65] "+job+" · "+notice,this);
            if(accurate&&gates)Award();
            else notice+=" Adjust settings and rerun the batch.";
            EndJob();
        }
        private void EndJob()
        {
            if(part)Destroy(part);
            part=null;
            attached=false;
            job=Job.None;
        }
        private void Trip(string reason)
        {
            if(part)lost++;
            EndJob();
            latch=reason;
            calibrated=homing=homed=services=false;
            belt=vacuum=gate=0;
            Debug.LogWarning("[LEVEL 65] "+reason,this);
        }
        private void Award()
        {
            if(stage>=Gates.Length||!bindings.SetRuntimeComponentBinding(Gates[stage],source))return;
            stage++;
            manager.QueueEvaluation();
        }

        public void Power(){power=!power;source.SetOutput(power,default);}
        public void Guard(){door=!door;contacts[0].SetState(door?SparkSwitchState.Closed:SparkSwitchState.Open);}
        public void Stop(){stop=!stop;emergency.SetState(!stop?SparkSwitchState.Closed:SparkSwitchState.Open);}
        public void Curtain(){curtain=!curtain;contacts[1].SetState(curtain?SparkSwitchState.Closed:SparkSwitchState.Open);}
        public void Commission(){if(ready&&latch==null)commissioned=true;}
        public void Calibrate(){if(ready&&commissioned&&job==Job.None){calibrated=true;travel=0;encoder=0;}}
        public void Vision(){if(CanRestart)camera=true;}
        public void Trial(){if(stage==1&&CanRestart&&camera&&job==Job.None)Begin(Job.Trial,1);}
        public void Home(){if(CanRestart&&job==Job.None)homing=true;}
        public void Services(){if(CanRestart&&homed)services=true;}
        public void Sort(){if(stage==2&&Automation&&job==Job.None)Begin(Job.Sort,3);}
        public void Reject(){if(stage==3&&Automation&&job==Job.None)Begin(Job.Reject,2);}
        public void Speed(){if(job==Job.None)fastBelt=!fastBelt;}
        public void Cycle(){if(job==Job.None)fastRobot=!fastRobot;}
        public void Mixed(){if(stage==4&&Automation&&job==Job.None)Begin(Job.Mixed,8);}
        public void Fault(){if(stage==5&&Automation&&job==Job.None&&!faultObserved)faultArmed=true;}
        public void Reset()
        {
            if(!ready||!calibrated||latch==null)return;
            resetAfterRepair=faultObserved;
            faultArmed=false;
            latch=null;
        }
        public void Final(){if(stage==5&&Automation&&job==Job.None&&resetAfterRepair)Begin(Job.Final,20);}

        private string Prompt()
        {
            if(stage==6)return "CERTIFIED · 20 products inspected and routed after encoder recovery.";
            if(latch!=null)return latch+(ready?calibrated?" · reset and recommission.":" · recalibrate encoder before reset.":" · restore wiring and safety.");
            if(wiring!=null)return wiring;
            if(!ready)return "Enable 5 V control and check each safety channel.";
            if(!commissioned)return "Commission the PLC.";
            if(!calibrated)return "Calibrate conveyor encoder and optical sensor.";
            if(!camera)return "Calibrate the vision system.";
            if(job!=Job.None)return $"{job} · Part {spawned}/{batchSize} · {motion}";
            if(stage==1)return "Feed a test part to commission vision.";
            if(!homed)return "Home the six robot axes.";
            if(!services)return "Enable vacuum and pneumatic air.";
            return stage switch
            {
                2=>"Run A / B / C sorting.",
                3=>"Run the defect test.",
                4=>notice.StartsWith("Batch")?notice:"Select fast belt and robot cycle, then run a mixed batch.",
                5=>resetAfterRepair?"Run the final 20-product batch.":faultArmed?"Disconnect W66 · ENC PULSE → PLC ENCODER.":"Select W66 TEST.",
                _=>notice
            };
        }
        private void Present()
        {
            statusText.text=Prompt();
            meterText.text=$"BELT {belt:F2}  ·  ENCODER {(EncoderValid?encoder.ToString():"INVALID")}  ·  VISION {confidence:P0}  ·  VAC {vacuum:P0}  AIR {air:P0}";
            detailText.text=$"A {counts[0]}   B {counts[1]}   C {counts[2]}   REJECT {counts[3]}  ·  CORRECT {correct}/{finished}  ·  {Ppm:F1} PARTS/MIN";
            calibrationText.text=$"{manager.SatisfiedTargetCount}/6  ·  {(ready?"SAFETY READY":"SAFETY HOLD")}  ·  {(attached?"GRIP CONFIRMED":"GRIP OPEN")}  ·  {(gate>.96f?"REJECT EXTENDED":gate<.05f?"RETRACTED":"GATE MOVING")}";
            if(controllerDisplay)controllerDisplay.text=$"PLC {(commissioned?"READY":"IDLE")} / BELT {belt:F2}";
            if(qualityDisplay)qualityDisplay.text=$"{correct}/{finished} OK  /  {Ppm:F1} PPM";
            rig.Present(scanTravel,gate,door,stop,controls,Time.deltaTime);
            bool idle=job==Job.None;
            powerButton.interactable=guardButton.interactable=stopButton.interactable=curtainButton.interactable=true;
            commissionButton.interactable=ready&&!commissioned&&latch==null;
            calibrateButton.interactable=ready&&commissioned&&!calibrated&&idle;
            visionButton.interactable=CanRestart&&!camera;
            trialButton.interactable=stage==1&&CanRestart&&camera&&idle;
            homeButton.interactable=CanRestart&&stage>=2&&!homed&&!homing&&idle;
            servicesButton.interactable=CanRestart&&homed&&!services;
            sortButton.interactable=Automation&&stage==2&&idle;
            rejectButton.interactable=Automation&&stage==3&&idle;
            speedButton.interactable=cycleButton.interactable=stage>=4&&ready&&latch==null&&idle;
            speedButton.GetComponentInChildren<Text>().text=fastBelt?"BELT FAST":"BELT SLOW";
            cycleButton.GetComponentInChildren<Text>().text=fastRobot?"ROBOT FAST":"ROBOT SLOW";
            mixedButton.interactable=stage==4&&Automation&&idle;
            faultButton.interactable=stage==5&&Automation&&idle&&!faultArmed&&!faultObserved;
            resetButton.interactable=ready&&calibrated&&latch!=null;
            finalButton.interactable=stage==5&&Automation&&idle&&resetAfterRepair;
        }
        private void OnDisable()
        {
            EndJob();
            if(bindings!=null)foreach(string gate in Gates)bindings.ClearRuntimeComponentBinding(gate);
        }
    }
}

