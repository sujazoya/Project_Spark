using System;
using System.Collections.Generic;
using AAAUI.VFX;
using ProjectSpark.Circuit;
using ProjectSpark.Visuals;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.Gameplay
{
    public sealed class SparkLevel83Process : MonoBehaviour
    {
        public static readonly string[] Gates={"L83_Wired","L83_Solar","L83_Storage","L83_Eclipse","L83_Disturbances","L83_Recertified"};
        public SparkLevelLayout layout;
        public SparkPowerSupply source,transducer;
        public SparkSwitch[] contacts;
        public SparkSwitch emergency;
        public SparkTerminal[] pins;
        public TextMesh[] pinLabels;
        public SparkTerminal ground;
        public SparkLevel83VisualRig rig;
        public Text statusText,meterText,detailText,eventText;
        public Button[] buttons;

        public readonly SparkLevel83Plant Plant=new();
        public int Stage{get;private set;}
        public int RunId{get;private set;}
        public bool Trusted{get;private set;}
        public bool FeedbackLatched{get;private set;}
        public bool FeedbackObserved{get;private set;}
        public bool SafeChecked{get;private set;}
        public float Measured{get;private set;}
        public float FaultBus{get;private set;}
        public int topology=-1;
        private const int W104=SparkLevel83Wiring.Feedback;
        private const int W129=128,W130=129;
        private static readonly (int a,int b)[] Wires=SparkLevel83Wiring.Wires;
        private readonly Dictionary<SparkTerminal,int> lookup=new();
        private readonly List<SparkCircuitConnection> connections=new();
        private readonly HashSet<(int,int)> current=new();
        private readonly bool[] routed=new bool[138];
        private LevelGamePlayManager manager;
        private SparkLevelLayoutLoader loader;
        private SparkLevelSceneBindings bindings;
        private SignalWireBuilder wireTool;
        private int shownWire=int.MinValue,shownPin=-2;
        private bool power,rails,estopOk,uvOk,safetyPassed,otherLatch;
        private bool port,starboard,home,track,mppt,precharge,main,packs,balancing,plan,exercise;
        private bool calibrated,feedbackArmed,feedbackUnderLoad,feedbackReset,fieldDrive,diagnosed,jamPassed,imbalancePassed,burstPassed;
        private bool isolateTest,runHandled=true;
        private int safetyStep,calibrationStep,wireFault=-2,disturbance;
        private float clock,accumulator,sampleClock,sampleAge=5,calClock,calZero,calGain=1,highDwell,jam,imbalance;
        private float lastGoodRail=-5,lastGoodEmergency=-5,lastGoodUv=-5;
        private float ampWritten=-99,energyAtFault,burstPeak;
        private string latest="Connect the 138 wires at the equipment sockets.";
        private bool RequiredWiring=>wireFault==-1;
        private bool CanPower=>rails&&estopOk&&safetyPassed;
        private bool Unlatched=>!FeedbackLatched&&!otherLatch;
        private bool Normal=>CanPower&&uvOk&&RequiredWiring&&Trusted&&Unlatched&&main;
        private bool FieldPower=>rails&&routed[9]&&routed[W104];
        private bool CalOK=>FieldPower&&estopOk&&safetyPassed&&calibrationStep==0&&!Plant.Running;
        private bool Live=>CanPower&&RequiredWiring&&Unlatched;
        private bool Locked=>Plant.Port.Efficiency>.90f&&Plant.Starboard.Efficiency>.90f;
        private bool CanRun=>Stage>=3&&Stage<6&&Normal&&plan&&Plant.Reserve>250&&
            Plant.A.Healthy&&Plant.B.Healthy&&Plant.Port.Home&&Plant.Starboard.Home&&track&&mppt&&
            !Plant.Running&&disturbance!=1&&disturbance!=2;
        private bool FaultOnly=>!routed[W104]&&current.Count==Wires.Length-1&&wireFault==W104;
        private bool CanExperiment=>Stage==4&&Normal&&!Plant.Running&&disturbance==0;
        private bool CanArmFeedback=>Stage==5&&Normal&&Plant.Running&&Plant.Bus>25&&!FeedbackObserved&&!feedbackArmed;
        private bool FaultMeasured=>disturbance switch
        {
            1=>jam>.6f&&Mathf.Abs(Plant.Port.Azimuth-Plant.SunAz)>28&&Plant.Port.Available<Plant.Starboard.Available*.80f,
            2=>Plant.B.High-Plant.B.Low>.28f,
            3=>Plant.Eclipse&&burstPeak>45,
            _=>false
        };

        private void Start()
        {
            manager=FindFirstObjectByType<LevelGamePlayManager>();
            loader=FindFirstObjectByType<SparkLevelLayoutLoader>();
            wireTool=FindFirstObjectByType<SignalWireBuilder>();
            if(!manager||!loader||!source||!transducer||!ground||pins.Length!=SparkLevel83Wiring.Pins.Length)
            {Debug.LogError("Level 83 power and socket bindings are incomplete.",this);enabled=false;return;}
            for(int i=0;i<pins.Length;i++)lookup.Add(pins[i],i);
            source.SetOutput(false,default);
        }
        private static (int,int) Pair(int a,int b)=>a<b?(a,b):(b,a);
        private void ReadTopology(SparkCircuitSystem circuit)
        {
            current.Clear();connections.Clear();circuit.CopyConnections(connections);
            foreach(var c in connections)
            {
                if(c.Kind!=SparkConnectionKind.Wire)continue;
                bool from=lookup.TryGetValue(c.A,out int a),to=lookup.TryGetValue(c.B,out int b);
                if(from||to)current.Add(Pair(from?a:-1,to?b:-1));
            }
            int missing=0;
            wireFault=-1;
            for(int i=0;i<Wires.Length;i++)
            {
                var (a,b)=Wires[i];routed[i]=current.Contains(Pair(a,b));
                if(routed[i])continue;
                missing++;
                if(wireFault==-1)wireFault=i;
            }
            if(current.Count!=Wires.Length-missing||missing>1&&wireFault==W104)wireFault=-2;
        }
        private void Update()
        {
            if(loader.ActiveLayout!=layout||!loader.ActiveBindings)return;
            var circuit=manager.CircuitSystem;
            var solver=manager.ElectricalSolver;
            if(!circuit||!solver)return;
            if(bindings!=loader.ActiveBindings){bindings=loader.ActiveBindings;topology=-1;}
            if(topology!=circuit.TopologyVersion){topology=circuit.TopologyVersion;ReadTopology(circuit);}
            var solve=solver.LatestSolveResult;
            bool Read(int port,out float value)
            {
                value=0;
                return solve!=null&&solve.TryGetVoltage(pins[Wires[port].b],ground,out value);
            }
            bool High(int port)=>Read(port,out float value)&&value is >=4.6f and <=5.3f;
            bool railPresent=power&&source.IsOutputActive&&!source.IsOverloaded;
            for(int i=0;i<10;i++)railPresent&=routed[i]&&High(i);
            if(railPresent)lastGoodRail=clock;
            if(High(W130))lastGoodEmergency=clock;
            if(High(W129))lastGoodUv=clock;
            rails=power&&source.IsOutputActive&&!source.IsOverloaded&&clock-lastGoodRail<.32f;
            estopOk=emergency.IsClosed&&routed[W130]&&clock-lastGoodEmergency<.32f;
            uvOk=contacts[0].IsClosed&&routed[W129]&&clock-lastGoodUv<.32f;
            // Save the live condition at the physical disconnect edge, before stale sample timeout.
            if(Stage==5&&feedbackArmed&&Plant.Running&&FaultOnly&&Plant.Bus>23&&Plant.ActualLoad>25)
                feedbackUnderLoad=true;
            float speed=feedbackArmed&&!FeedbackLatched?.75f:3.5f;
            accumulator+=Mathf.Min(Time.deltaTime,.18f)*speed;
            int steps=0;
            while(accumulator>=SparkLevel83Plant.Step&&steps++<24)
            {
                accumulator-=SparkLevel83Plant.Step;
                bool valid=Read(W104,out float sample);
                Tick(SparkLevel83Plant.Step,valid,sample);
            }
            Publish();
        }

        private void DriveTransducer(float output)
        {
            bool active=FieldPower;
            if(!active)
            {
                if(fieldDrive)transducer.SetOutput(false,default);
                fieldDrive=false;
                return;
            }
            if(fieldDrive&&Mathf.Abs(output-ampWritten)<.004f)return;
            transducer.SetOutput(false,default);
            if(!transducer.TryConfigure(output,.01f,out var reason)){Event(reason);return;}
            transducer.SetOutput(true,default);
            fieldDrive=true;ampWritten=output;
        }
        private static float Sensor(float bus)=>.173f+.1275f*bus;
        private void Tick(float dt,bool valid,float sample)
        {
            clock+=dt;calClock+=dt;sampleClock+=dt;sampleAge+=dt;
            if(safetyStep==1&&!uvOk&&estopOk&&calClock>.2f){contacts[0].SetState(SparkSwitchState.Closed);safetyStep=2;calClock=0;}
            if(safetyStep==2&&uvOk&&estopOk&&calClock>.2f){safetyPassed=true;safetyStep=0;Event("Independent undervoltage contact states verified.");}
            if(safetyStep==0)
            {
                bool isolate=main&&Plant.MainContactor>.9f&&Plant.Bus<20.8f;
                var state=isolate?SparkSwitchState.Open:SparkSwitchState.Closed;
                if(contacts[0].IsClosed==isolate)contacts[0].SetState(state);
            }
            if(sampleClock>=.24f)
            {
                sampleClock=0;
                if(FieldPower&&valid&&sample is >.08f and <4.7f)
                {
                    sampleAge=0;
                    Measured=Mathf.Clamp((sample-calZero)*calGain,0,34);
                }
                float refVoltage=calibrationStep==1?0:calibrationStep==2?28:Plant.Bus;
                DriveTransducer(Sensor(refVoltage));
            }
            if(calibrationStep!=0)
            {
                if(!FieldPower){calibrationStep=0;calibrated=false;Event("Calibration cancelled. Restore the actual sensing circuit.");}
                else if(calClock>1&&valid&&sampleAge<.5f)
                {
                    if(calibrationStep==1&&sample is >.10f and <.30f){calZero=sample;calibrationStep=2;calClock=0;}
                    else if(calibrationStep==2&&sample is >3.4f and <4.0f)
                    {calGain=28/(sample-calZero);calibrated=true;calibrationStep=0;sampleAge=5;Event("Two-point transducer calibration verified on the W104 circuit.");}
                }
            }
            Trusted=calibrated&&calibrationStep==0&&FieldPower&&sampleAge<.72f&&Mathf.Abs(Measured-Plant.Bus)<2.2f;
            if(Stage>=1&&main&&!FeedbackLatched&&calibrationStep==0&&!routed[W104]&&sampleAge>.72f&&FaultOnly)
            {
                FaultBus=Plant.Bus;energyAtFault=Plant.A.Energy+Plant.B.Energy;
                FeedbackObserved=Stage==5&&feedbackArmed&&feedbackUnderLoad;
                FeedbackLatched=true;calibrated=false;Trusted=false;
                SafeChecked=false;feedbackArmed=false;Plant.EndRun();
                Event("W104 is stale. Normal regulation is inhibited. Essential loads remain under independent protection.");
            }
            bool isolated=!power||!estopOk;
            if(Stage>=1&&main&&isolated&&!otherLatch){otherLatch=true;Plant.EndRun();SafeChecked=false;Event("Emergency isolation. Check the control rail and reset safety.");}
            if(disturbance==1)
            {
                jam=Mathf.MoveTowards(jam,1,dt*.4f);
                if(!diagnosed&&jam>.42f)Plant.Port.Azimuth=Mathf.MoveTowards(Plant.Port.Azimuth,-76,dt*28);
            }
            if(disturbance==3&&Plant.Eclipse)burstPeak=Mathf.Max(burstPeak,Plant.P2);
            Plant.StepPlant(dt,port,starboard,home,track,mppt,precharge,main,packs,balancing,Normal,
                rails,!estopOk,uvOk||!main,plan,Measured,jam,imbalance,disturbance==3,false);
            highDwell=Normal&&Plant.Bus is >26 and <29.8f&&Locked?highDwell+dt:0;
            if(Stage==0&&CanPower&&RequiredWiring&&Trusted)Award();
            else if(Stage==1&&highDwell>2.5f&&Plant.Port.Home&&Plant.Starboard.Home&&Plant.MainContactor>.99f)Award();
            else if(Stage==2&&packs&&balancing&&Plant.A.Contact>.97f&&Plant.B.Contact>.97f&&
                    Plant.ChargedA&&Plant.ChargedB&&Plant.DischargedA&&Plant.DischargedB&&Plant.A.Mismatch<.016f&&Plant.B.Mismatch<.016f&&
                    Plant.A.Healthy&&Plant.B.Healthy&&highDwell>2)Award();
            if(!runHandled&&!Plant.Running)
            {
                runHandled=true;
                if(Plant.Completed)
                {
                    if(Stage==3||Stage==4&&jamPassed&&imbalancePassed&&burstPassed||Stage==5&&feedbackReset)Award();
                    Event($"Orbit {RunId} qualified. Eclipse {Plant.EclipseTime:0.0}s · minimum {Plant.MinVoltage:0.00} V · downlink on time.");
                }
                else if(!FeedbackLatched)Event($"Orbit {RunId} interrupted or outside limits. Min {Plant.MinVoltage:0.00} V. Review the mission plan.");
            }
        }
        private void Event(string message)=>latest=$"{clock:0}s · {message}";
        private void Award()
        {
            if(Stage>=6||!bindings.SetRuntimeComponentBinding(Gates[Stage],source))return;
            Stage++;
            manager.QueueEvaluation();
            Event($"Objective {Stage}/6 verified.");
        }

        public void Power(){power=!power;source.SetOutput(power,default);}
        public void TestSafety()
        {if(rails&&estopOk&&!main&&routed[W129]&&safetyStep==0){safetyPassed=false;safetyStep=1;calClock=0;contacts[0].SetState(SparkSwitchState.Open);}}
        public void DeployPort(){if(Live)port=true;}
        public void DeployStarboard(){if(Live)starboard=true;}
        public void Home(){if(Live&&Mathf.Min(Plant.Port.Deploy,Plant.Starboard.Deploy)>.98f){home=true;track=false;}}
        public void Track(){if(Live&&Plant.Port.Home&&Plant.Starboard.Home)track=true;}
        public void PortSweep(){if(Live&&track&&!Plant.Running)Plant.Port.StartSweep();}
        public void StarboardSweep(){if(Live&&track&&!Plant.Running)Plant.Starboard.StartSweep();}
        public void Mppt(){if(Live&&Plant.Port.SweepDone&&Plant.Starboard.SweepDone)mppt=true;}
        public void Precharge(){if(Live&&mppt)precharge=true;}
        public void CloseMain(){if(Live&&precharge&&Plant.Bus>25.8f)main=true;}
        public void Calibrate()
        {
            if(!CalOK)return;
            calibrated=false;calibrationStep=1;calClock=0;SafeChecked=false;
            Event("Sampling 0 V and 28 V references through W104.");
        }
        public void Packs(){if(Live&&Stage>=2)packs=true;}
        public void Balance()
        {
            if(Live&&packs&&!Plant.Running)
            {
                balancing=true;
                if(!exercise&&Stage==2)StartCoroutine(DischargeCheck());
            }
        }
        private System.Collections.IEnumerator DischargeCheck()
        {
            exercise=true;
            mppt=false;
            float until=clock+1.6f;
            while(clock<until&&Normal)yield return null;
            mppt=true;
            Event("Pack charge and discharge measurements recorded. Balancing continues until cell spread settles.");
        }
        public void Plan()
        {if(Live&&packs&&Plant.Reserve>250){plan=true;Event("Reserve sufficient. Schedule science in daylight and optical downlink after sunlight return.");}}
        public void StartOrbit()
        {
            if(!CanRun)return;
            RunId++;runHandled=false;Plant.StartRun();
            Event($"Orbit {RunId} started. 45s sunlight, 35s eclipse, then 40s recovery (simulation time).");
        }
        public void Jam()
        {if(CanExperiment&&!jamPassed){disturbance=1;diagnosed=false;jam=.02f;Event("Port pointing disturbance applied. Compare sun angle and encoder.");}}
        public void Imbalance()
        {if(CanExperiment&&jamPassed&&!imbalancePassed){disturbance=2;diagnosed=false;imbalance=.225f;balancing=false;Event("Pack B cell-group imbalance introduced.");}}
        public void Burst()
        {if(CanExperiment&&imbalancePassed&&!burstPassed){disturbance=3;diagnosed=false;burstPeak=0;StartOrbit();Event("Optical surge scheduled at eclipse entry.");}}
        public void Diagnose()
        {
            if(!Live||!FaultMeasured)return;
            diagnosed=true;
            Event(disturbance==1?"Port gimbal is stalled off-sun.":disturbance==2?"Pack B group spread exceeded safe charging margin.":"Optical branch surge confirmed while solar input was zero.");
        }
        public void Repair()
        {
            if(!diagnosed||!Live)return;
            if(disturbance==1){jam=0;jamPassed=true;Plant.Port.Home=false;Home();Event("Service the port actuator. Home both gimbals, then reacquire the sun.");}
            if(disturbance==2){imbalance=0;imbalancePassed=true;balancing=true;Event("Pack B charge reduced while passive balancing removes excess cell energy.");}
            if(disturbance==3){burstPassed=true;Plant.EndRun();Event("Optical burst capped. Run a new orbit using the planned daylight downlink.");}
            disturbance=0;diagnosed=false;
        }
        public void ArmFeedback()
        {
            if(CanArmFeedback)
            {feedbackArmed=true;feedbackUnderLoad=false;Event("Test armed. Remove W104 through the Wire Controls list or Undo Last.");}
        }
        public void CheckSafe()
        {
            if((FeedbackLatched||otherLatch)&&RequiredWiring&&Trusted&&estopOk&&uvOk&&
                Plant.Bus is >21 and <30.5f&&Plant.A.Healthy&&Plant.B.Healthy&&(!FeedbackLatched||energyAtFault>0))
            {SafeChecked=true;Event("Fresh feedback, independent protection and both packs verified.");}
        }
        public void ResetSafety()
        {
            if(!SafeChecked||!RequiredWiring||!Trusted||!uvOk||!estopOk)return;
            feedbackReset|=FeedbackLatched&&FeedbackObserved;
            FeedbackLatched=false;otherLatch=false;SafeChecked=false;
            Event("Safety reset. Start a fresh full orbit to qualify.");
        }
        public void Emergency()=>emergency.SetState(emergency.IsClosed?SparkSwitchState.Open:SparkSwitchState.Closed);

        private string Instruction()
        {
            if(wireFault>=0)return $"W{wireFault+1:000} · {pins[Wires[wireFault].a].name} → {pins[Wires[wireFault].b].name}";
            if(wireFault==-2)return "Check the wiring panel for missing or extra connections.";
            if(!power)return "Turn on the 5 V control supply.";
            if(!safetyPassed)return "Run the independent safety self-test.";
            if(FeedbackLatched||otherLatch)return !Trusted?"Recalibrate the repaired W104 input.":!SafeChecked?"Check the independent safety channels.":"Reset safety and run a fresh orbit.";
            if(Stage==6)return "6/6 • ORBITAL POWER QUALIFIED";
            if(!calibrated)return "Calibrate the trusted bus transducer.";
            if(Stage<=1)return "Deploy and home both wings, track, sweep both arrays, enable MPPT, precharge and close the main contactor.";
            if(Stage==2)return "Commission both packs and run the cell balance and load-transfer test.";
            if(Plant.Running)return feedbackArmed?"W104 armed • remove cable now.":$"Orbit {RunId} • {(Plant.Eclipse?"ECLIPSE":Plant.SawReturn?"RECHARGE":"SUNLIGHT")}";
            if(Stage==3)return "Plan mission demand and run the first full eclipse.";
            if(Stage==4)return disturbance!=0?"Diagnose the physical disturbance before repair.":!jamPassed?"Apply a port gimbal jam.":!imbalancePassed?"Apply the Pack B cell imbalance.":!burstPassed?"Apply the eclipse optical burst.":"Run a fresh post-repair orbit.";
            return feedbackReset?"Run the final full orbit.":"Start an orbit, arm W104 and remove the actual cable while the bus is live.";
        }
        private void Publish()
        {
            if(pinLabels!=null&&pinLabels.Length==pins.Length)
            {
                int hovered=wireTool&&wireTool.HoveredTerminal&&lookup.TryGetValue(wireTool.HoveredTerminal,out int pin)?pin:-1;
                if(shownWire!=wireFault||shownPin!=hovered)
                {
                    shownWire=wireFault;shownPin=hovered;
                    var (a,b)=wireFault>=0?Wires[wireFault]:(-1,-1);
                    for(int index=0;index<pinLabels.Length;index++)
                        if(pinLabels[index])pinLabels[index].gameObject.SetActive(index==a||index==b||index==hovered);
                }
            }
            statusText.text=$"{Stage}/6 · {Instruction()}";
            meterText.text=$"BUS {Plant.Bus:0.00} V   ·   W104 {(Trusted?$"{Measured:0.00} V":"STALE")}   ·   SUN {Plant.Sun:P0}   ·   P0/P1 {Plant.P0+Plant.P1:0} W";
            detailText.text=$"PV {Plant.Port.Delivered:0}/{Plant.Starboard.Delivered:0} W   ·   PACK A/B {Plant.A.SOC:P0}/{Plant.B.SOC:P0}   ·   B CELL {Plant.B.Low:0.00}–{Plant.B.High:0.00} V   ·   RESERVE {Plant.Reserve:0} J";
            eventText.text=latest;
            bool[] enabled={true,rails&&estopOk&&!main,Live&&!port,Live&&!starboard,Live&&Mathf.Min(Plant.Port.Deploy,Plant.Starboard.Deploy)>.98f,
                Live&&Plant.Port.Home&&Plant.Starboard.Home&&!track,Live&&track&&!Plant.Running&&!mppt,Live&&track&&!Plant.Running&&!mppt,
                Live&&!mppt&&Plant.Port.SweepDone&&Plant.Starboard.SweepDone,Live&&mppt&&!precharge,Live&&precharge&&!main&&Plant.Bus>25.8f,
                CalOK&&!Trusted,Live&&Stage>=2&&!packs,Live&&packs&&!Plant.Running,Live&&packs&&Plant.Reserve>250&&!plan,CanRun,
                CanExperiment&&!jamPassed,CanExperiment&&jamPassed&&!imbalancePassed,CanExperiment&&imbalancePassed&&!burstPassed,
                FaultMeasured&&Live,diagnosed&&Live,CanArmFeedback,
                (FeedbackLatched||otherLatch)&&RequiredWiring&&Trusted&&!SafeChecked,SafeChecked,true};
            for(int i=0;i<buttons.Length;i++)buttons[i].interactable=enabled[i];
            if(!rig)return;
            rig.SetActualOrbitalState(Plant.Phase,Plant.Sun,1-Plant.Sun);
            rig.SetActualSolarArrays(Plant.Port.Deploy,Plant.Starboard.Deploy,Plant.Port.Azimuth,Plant.Port.Elevation,
                Plant.Starboard.Azimuth,Plant.Starboard.Elevation,Plant.Sun>.05f);
            rig.SetActualPowerMeasurements(Plant.Port.Efficiency,Plant.Starboard.Efficiency,Plant.Bus/28,Plant.A.SOC,Plant.B.SOC,
                Mathf.Abs(Plant.Demand-Plant.Supply)/45,Normal,Plant.A.Current<-.12f);
            rig.SetActualBatteryMechanisms(Plant.A.Contact,Plant.B.Contact,balancing?1:0,Plant.MainContactor,Plant.SuperEnergy/44);
            rig.SetActualLoadDistribution(Plant.P0/19,Plant.P1/11,Plant.P2/64,Plant.P3/11,Plant.Thermal/4);
            rig.SetActualSafetyAndFaults(Trusted,FeedbackLatched||otherLatch,FeedbackLatched,jam,Plant.B.Mismatch/.25f,Plant.P2/64,Stage/6f);
        }
    }
}
