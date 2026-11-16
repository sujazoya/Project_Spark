using System;
using System.Collections.Generic;
using AAAUI.VFX;
using ProjectSpark.Circuit;
using ProjectSpark.Visuals;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.Gameplay
{
    public sealed class SparkLevel84Process : MonoBehaviour
    {
        public static readonly string[] Gates={"L84_Network","L84_Detumbled","L84_Precision","L84_Optical","L84_Faults","L84_Recertified"};
        public SparkLevelLayout layout;
        public SparkPowerSupply source,transducer;
        public SparkSwitch[] contacts;
        public SparkSwitch emergency;
        public SparkTerminal[] pins;
        public SparkTerminal ground;
        public SparkLevel84VisualRig rig;
        public Transform arena;
        public TextMesh[] pinLabels;
        public Text statusText,meterText,detailText,eventText;
        public Button[] buttons;
        public readonly SparkLevel84Plant Plant=new();
        public int Stage{get;private set;}
        public int RunId{get;private set;}
        public bool Trusted{get;private set;}
        public bool FeedbackLatched{get;private set;}
        public bool FeedbackObserved{get;private set;}
        public bool LinkAtDisconnect{get;private set;}
        public int Topology{get;private set;}=-1;
        public bool Unloaded=>Plant.DumpImpulse>.12f&&Plant.DumpChange>.035f;
        private const int W097=SparkLevel84Wiring.Feedback,W095=94,W106=105;
        private static readonly (int a,int b)[] Wires=SparkLevel84Wiring.Wires;
        private readonly Dictionary<SparkTerminal,int> lookup=new();
        private readonly List<SparkCircuitConnection> connections=new();
        private readonly HashSet<(int,int)> edges=new();
        private readonly bool[] routed=new bool[142];
        private readonly bool[] repaired=new bool[4];
        private LevelGamePlayManager manager;
        private SparkLevelLayoutLoader loader;
        private SparkLevelSceneBindings bindings;
        private SignalWireBuilder wireTool;
        private SparkWireRegistry registry;
        private SparkLevel84ArenaView view;
        private readonly Dictionary<Renderer,bool> hiddenCableRenderers=new();
        private bool focusCables, choseCableView;
        private int wireFault=-2,safetyStep,shownWire=int.MinValue,shownPin=-1,fault,starSequenceAtTrip;
        private float accumulator,dwell,contactClock,lastRail=-5,lastEmergency=-5,lastPermit=-5;
        private float sampleLevel=-1,repairTime,lastStarCommand;
        private bool power,rails,emergencyOk,permit,safety,cal,detumble,baffle,stars,fine,dump,align,armed,storedRun=true,diagnosed,servicing,
            motionLatch,checkedSafe,feedbackRecovered;
        private string latest="Connect the numbered equipment terminals.";
        private bool AllWires=>wireFault==-1;
        private bool Ready=>AllWires&&rails&&emergencyOk&&permit&&safety;
        private bool Live=>Ready&&!FeedbackLatched&&!motionLatch;
        private bool OnlyFeedbackMissing=>wireFault==W097&&edges.Count==141;
        private bool AllRepairs=>repaired[1]&&repaired[2]&&repaired[3];
        private bool CanRun=>Stage>=3&&Stage<6&&Live&&Trusted&&fine&&align&&Plant.OpticalError<.26f&&
            Unloaded&&Plant.SafeActuators&&Plant.QualifiedFine&&!Plant.Running&&fault==0;
        private bool CanFault=>Stage==4&&Live&&Trusted&&!Plant.Running&&fault==0;
        private bool FaultEvidence=>fault switch
        {
            1=>Plant.GyroResidual.magnitude>.092f,
            2=>Plant.MaxWheel>.87f,
            3=>Plant.Jitter>.37f,
            _=>false
        };
        private bool CanArm=>Stage==5&&Live&&Plant.OpticalLocked&&Plant.Running&&!armed&&!FeedbackObserved;
        private bool CanCheck=>Ready&&(FeedbackLatched||motionLatch)&&Trusted&&Plant.SafeActuators&&
            Plant.StarSequence>starSequenceAtTrip+2&&Plant.Rate.magnitude<.12f&&Plant.GyroResidual.magnitude<.030f&&
            Quaternion.Angle(Plant.Estimate,Plant.Body)<.85f&&lastStarCommand>0;

        private void Start()
        {
            manager=FindFirstObjectByType<LevelGamePlayManager>();
            loader=FindFirstObjectByType<SparkLevelLayoutLoader>();
            wireTool=FindFirstObjectByType<SignalWireBuilder>();
            if(!manager||!loader||!arena||pins.Length!=SparkLevel84Wiring.Pins.Length)
            {Debug.LogError("Level 84 arena bindings are incomplete.",this);enabled=false;return;}
            for(int i=0;i<pins.Length;i++)lookup.Add(pins[i],i);
            source.SetOutput(false,default);
            registry=FindFirstObjectByType<SparkWireRegistry>();
            if(registry)
            {
                registry.WiresChanged+=UpdateCableView;
                registry.SelectionChanged+=SelectCable;
            }
            view=gameObject.AddComponent<SparkLevel84ArenaView>();
            view.Initialize(arena);
        }

        private static (int,int) Pair(int a,int b)=>a<b?(a,b):(b,a);
        private void ReadTopology(SparkCircuitSystem circuit)
        {
            connections.Clear();edges.Clear();circuit.CopyConnections(connections);
            foreach(var edge in connections)
            {
                if(edge.Kind!=SparkConnectionKind.Wire)continue;
                bool a=lookup.TryGetValue(edge.A,out int ai),b=lookup.TryGetValue(edge.B,out int bi);
                if(a||b)edges.Add(Pair(a?ai:-1,b?bi:-1));
            }
            int missing=0;
            wireFault=-1;
            for(int i=0;i<Wires.Length;i++)
            {
                routed[i]=edges.Contains(Pair(Wires[i].a,Wires[i].b));
                if(routed[i])continue;
                missing++;
                if(wireFault<0)wireFault=i;
            }
            if(edges.Count!=Wires.Length-missing||missing>1&&wireFault==W097)wireFault=-2;
        }

        private void Update()
        {
            if(loader.ActiveLayout!=layout||!loader.ActiveBindings)return;
            var circuit=manager.CircuitSystem;var solver=manager.ElectricalSolver;
            if(!circuit||!solver)return;
            if(bindings!=loader.ActiveBindings){bindings=loader.ActiveBindings;Topology=-1;}
            if(Topology!=circuit.TopologyVersion)
            {
                Topology=circuit.TopologyVersion;ReadTopology(circuit);
                if(OnlyFeedbackMissing&&armed&&Plant.OpticalLocked)LinkAtDisconnect=true;
            }
            var solve=solver.LatestSolveResult;
            bool Read(int cable,out float voltage)
            {
                voltage=0;return solve!=null&&solve.TryGetVoltage(pins[Wires[cable].b],ground,out voltage);
            }
            bool High(int cable)=>Read(cable,out float volts)&&volts is >=4.6f and <=5.3f;
            bool present=power&&source.IsOutputActive&&!source.IsOverloaded;
            for(int i=0;i<10;i++)present&=routed[i]&&High(i);
            if(present)lastRail=Plant.Clock;
            if(High(W106))lastEmergency=Plant.Clock;
            if(High(W095))lastPermit=Plant.Clock;
            rails=power&&source.IsOutputActive&&!source.IsOverloaded&&Plant.Clock-lastRail<.32f;
            emergencyOk=emergency.IsClosed&&routed[W106]&&Plant.Clock-lastEmergency<.32f;
            permit=contacts[0].IsClosed&&routed[W095]&&Plant.Clock-lastPermit<.32f;
            bool signal=Read(W097,out float s)&&s is >2.9f and <3.5f;
            bool continuity=Read(W097,out float pilot)&&pilot is >.35f and <3.5f;
            accumulator+=Mathf.Min(.14f,Time.deltaTime)*(armed?.55f:2.2f);
            while(accumulator>=SparkLevel84Plant.Step)
            {accumulator-=SparkLevel84Plant.Step;Tick(SparkLevel84Plant.Step,signal,continuity);}
            Present();
        }

        private void DriveSignal(float value)
        {
            bool active=rails&&routed[W097];
            if(!active)
            {
                if(transducer.IsOutputActive)transducer.SetOutput(false,default);
                sampleLevel=-1;return;
            }
            if(sampleLevel==value&&transducer.IsOutputActive)return;
            transducer.SetOutput(false,default);
            if(!transducer.TryConfigure(value,.01f,out string reason)){Event(reason);return;}
            transducer.SetOutput(true,default);sampleLevel=value;
        }

        private void Tick(float dt,bool carrier,bool continuity)
        {
            contactClock+=dt;
            if(safetyStep==1&&!permit&&emergencyOk&&contactClock>.20f)
            {contacts[0].SetState(SparkSwitchState.Closed);safetyStep=2;contactClock=0;}
            else if(safetyStep==2&&permit&&emergencyOk&&contactClock>.20f)
            {safety=true;safetyStep=0;Event("Independent motion permit and E-stop circuits verified.");}
            DriveSignal(stars&&Plant.Catalogue?3.15f:.52f);
            bool transport=rails&&routed[W097]&&carrier&&stars;
            Trusted=Plant.SampleFresh&&transport;
            bool coarse=Stage>0&&detumble&&(!fine||!Trusted||FeedbackLatched||motionLatch);
            if(Stage>=3&&OnlyFeedbackMissing&&!FeedbackLatched&&Plant.StarAge>.38f)
            {
                FeedbackLatched=true;FeedbackObserved|=armed&&LinkAtDisconnect;
                armed=false;checkedSafe=false;lastStarCommand=0;starSequenceAtTrip=Plant.StarSequence;
                Plant.EndMission();Event("W097 stale. Optical authorization withdrawn and coarse Sun-safe control retained.");
            }
            if(Stage>0&&!emergencyOk&&!motionLatch)
            {motionLatch=true;Plant.EndMission();checkedSafe=false;lastStarCommand=0;starSequenceAtTrip=Plant.StarSequence;}
            Plant.Tick(dt,rails,emergencyOk&&permit,cal,baffle,transport,coarse,Live&&fine&&Trusted,
                dump,align,fault,servicing);
            if(servicing)
            {
                repairTime+=dt;
                float remaining=fault==1?Plant.BiasFault:fault==2?Plant.WheelFault:Plant.FlexFault;
                if(remaining<.045f&&repairTime>1)
                {
                    repaired[fault]=true;fault=0;servicing=false;diagnosed=false;
                    Event("Measured disturbance recovered. Requalify with a new two-pass mission.");
                }
            }
            if(Stage==0&&Ready&&continuity){Award();return;}
            bool next=Stage==1&&Live&&Trusted&&Plant.Calibration>.98f&&Plant.Rate.magnitude<.075f||
                Stage==2&&Live&&Plant.QualifiedFine&&Plant.MaxWheel<.98f;
            dwell=next?dwell+dt:0;
            if(dwell>1.25f){Award();dwell=0;}
            if(!storedRun&&!Plant.Running)
            {
                storedRun=true;
                if(Plant.Completed)
                {
                    if(Stage==3||Stage==4&&AllRepairs||Stage==5&&feedbackRecovered)Award();
                    Event($"Mission {RunId} logged two distinct qualified optical passes.");
                }
                else if(!FeedbackLatched)Event($"Mission {RunId} ended with {Plant.PassCount}/2 link windows. Check pointing, momentum and jitter.");
            }
        }
        private void Award()
        {
            if(Stage>=6||!bindings.SetRuntimeComponentBinding(Gates[Stage],source))return;
            Stage++;manager.QueueEvaluation();Event($"Objective {Stage}/6 verified.");
            if(Stage==1&&!choseCableView){focusCables=true;UpdateCableView();}
        }
        private void Event(string text)=>latest=$"{Plant.Clock:0.0}s  ·  {text}";
        public void Power(){power=!power;source.SetOutput(power,default);}
        public void Safety(){if(rails&&emergencyOk&&safetyStep==0){safety=false;safetyStep=1;contactClock=0;contacts[0].SetState(SparkSwitchState.Open);}}
        public void CalGyro(){if(Live){cal=true;Event("Gyro rate bias calibration started.");}}
        public void Detumble(){if(Live&&cal){detumble=true;Event("Body rates are being reduced using projected magnetic torque and finite wheel braking.");}}
        public void FindStars()
        {
            if(Ready&&detumble)
            {baffle=true;stars=true;lastStarCommand=Plant.Clock;Event("Primary star tracker baffle opening. Wait for a fresh catalog match on W097.");}
        }
        public void Slew(){if(Stage>=2&&Live&&Trusted){fine=true;Event("Slew to the three-axis alignment target.");}}
        public void Dump(){if(Stage>=3&&Live){dump=true;Event("Unloading wheel momentum against the local magnetic field.");}}
        public void Align(){if(Stage>=3&&Live&&Unloaded){align=true;Event("Aligning the optical boresight under fine control.");}}
        public void Run(){if(CanRun){RunId++;storedRun=false;Plant.StartMission();Event($"Mission {RunId} scheduled. Track both moving ground windows.");}}
        private void Disturb(int test)
        {
            if(!CanFault||repaired[test])return;
            fault=test;diagnosed=servicing=false;
            Event(test==1?"Gyroscope bias injected. Compare the gyro residual.":test==2?
                "Wheel drive bias applied. Watch stored momentum and body reaction.":"Solar wing flex excitation applied. Watch line-of-sight jitter.");
        }
        public void GyroFault()=>Disturb(1);
        public void WheelFault()=>Disturb(2);
        public void FlexFault()=>Disturb(3);
        public void Diagnose()
        {
            if(!Live||!FaultEvidence)return;
            diagnosed=true;Event(fault==1?"Measured gyro residual exceeds calibration margin.":fault==2?
                "Wheel momentum approached the independent saturation limit.":"Measured jitter correlates with the wing’s flexible motion.");
        }
        public void Repair()
        {
            if(Live&&diagnosed&&!servicing)
            {servicing=true;repairTime=0;Event("Service started. Wait for the measured disturbance to settle.");}
        }
        public void Arm()
        {if(CanArm){armed=true;LinkAtDisconnect=false;Event("W097 armed in a live link window. Remove the amber W097 cable now.");}}
        public void CheckSafe(){if(CanCheck){checkedSafe=true;Event("Fresh primary stars agree with calibrated gyro and coarse safe motion.");}}
        public void ResetSafety()
        {
            if(!checkedSafe||!CanCheck)return;
            feedbackRecovered|=FeedbackLatched&&FeedbackObserved;
            FeedbackLatched=motionLatch=checkedSafe=false;Event("Safety reset. Start a fresh two-pass qualification.");
        }
        public void Stop()=>emergency.SetState(emergency.IsClosed?SparkSwitchState.Open:SparkSwitchState.Closed);
        public void ArenaView()=>view?.UseOverview();
        public void SocketView()=>view?.UseSocketView();
        public void CableView()
        {
            choseCableView=true;focusCables=!focusCables;
            UpdateCableView();
            Event(focusCables?"Equipment view shows the trusted and selected cables.":"Showing all installed cables.");
        }
        private void SelectCable(string wire)=>UpdateCableView();
        private void RestoreCableView()
        {
            foreach(var item in hiddenCableRenderers)
                if(item.Key)item.Key.forceRenderingOff=item.Value;
            hiddenCableRenderers.Clear();
        }
        private void UpdateCableView()
        {
            RestoreCableView();
            if(!focusCables||!registry)return;
            var (sourcePin,targetPin)=Wires[W097];
            foreach(var wire in registry.Wires)
            {
                if(!wire.Path||!wire.TerminalA||!wire.TerminalB||wire.WireId==registry.SelectedWireId)continue;
                if(!lookup.TryGetValue(wire.TerminalA,out int a)||!lookup.TryGetValue(wire.TerminalB,out int b)||
                    Pair(a,b)==Pair(sourcePin,targetPin))continue;
                foreach(var renderer in wire.Path.GetComponentsInChildren<Renderer>(true))
                {
                    hiddenCableRenderers[renderer]=renderer.forceRenderingOff;
                    renderer.forceRenderingOff=true;
                }
            }
        }
        private void OnDisable()
        {
            if(registry)
            {
                registry.WiresChanged-=UpdateCableView;
                registry.SelectionChanged-=SelectCable;
            }
            RestoreCableView();
        }

        private string Instruction()
        {
            if(wireFault>=0)
            {
                var (a,b)=Wires[wireFault];
                return $"W{wireFault+1:000}  ·  {SparkLevel84Wiring.Labels[a]} → {SparkLevel84Wiring.Labels[b]}";
            }
            if(wireFault==-2)return "Check the fixed terminals for an extra or misplaced cable.";
            if(!power)return "Turn on the 5 V control network.";
            if(!safety)return "Test the independent motion interlock.";
            if(FeedbackLatched||motionLatch)return !Trusted?"Acquire fresh primary stars after restoring W097.":!checkedSafe?
                "Check safe gyro and star agreement.":"Reset safety and run a fresh two-pass mission.";
            if(Stage==6)return "ADCS AND OPTICAL LINK QUALIFIED";
            if(Stage==1)return !cal?"Calibrate the gyro.":!detumble?"Detumble the spacecraft.":"Wait for low body rates and acquire primary stars.";
            if(Stage==2)return "Command a fine three-axis slew and hold the target.";
            if(armed)return "W097 ARMED  ·  REMOVE THE AMBER CABLE NOW";
            if(Plant.Running)return $"MISSION {RunId}  ·  {Plant.MissionTime:0.0}s  ·  {(Plant.OpticalLocked?"OPTICAL LOCK":"TRACKING")}  ·  PASSES {Plant.PassCount}/2";
            if(Stage==3)return !Unloaded?"Unload wheel momentum in the changing magnetic field.":"Align the optics and run both ground passes.";
            if(Stage==4)return fault!=0?"Diagnose the measured fault and repair it.":AllRepairs?"Run a fresh two-pass mission after repairs.":"Inject each disturbance and diagnose from live instruments.";
            return feedbackRecovered?"Run the final two-pass mission.":"Start a mission, arm W097 during optical lock, and remove its cable.";
        }
        private void Present()
        {
            int hovered=wireTool&&wireTool.HoveredTerminal&&lookup.TryGetValue(wireTool.HoveredTerminal,out int pin)?pin:-1;
            if(pinLabels!=null&&(shownWire!=wireFault||shownPin!=hovered))
            {
                shownWire=wireFault;shownPin=hovered;
                var (a,b)=wireFault>=0?Wires[wireFault]:(-1,-1);
                for(int i=0;i<pinLabels.Length;i++)if(pinLabels[i])pinLabels[i].gameObject.SetActive(i==a||i==b||i==hovered);
            }
            statusText.text=$"{Stage}/6  ·  {Instruction()}";
            meterText.text=$"BODY {Plant.RateDegrees.magnitude:0.00}°/s  ·  POINTING {Plant.PointError:0.00}°  ·  JITTER {Plant.Jitter:0.00}°  ·  W097 {(Trusted?"FRESH":"STALE")}";
            detailText.text=$"WHEELS {Plant.MaxWheel:P0}  ·  UNLOADED {Plant.DumpChange:0.00}   ·   STARS {Plant.Confidence:P0}  ·  GYRO RESIDUAL {Plant.GyroResidual.magnitude*Mathf.Rad2Deg:0.00}°/s  ·  PASSES {Plant.PassCount}/2";
            eventText.text=latest;
            bool[] enabled={true,rails&&emergencyOk&&safetyStep==0&&!safety,Live&&!cal,Live&&cal&&!detumble,
                Ready&&detumble&&(!stars||FeedbackLatched&&!checkedSafe||motionLatch&&!checkedSafe),Live&&Stage>=2&&Trusted&&!fine,
                Live&&Stage>=3&&!dump,Live&&Stage>=3&&Unloaded&&!align,CanRun,
                CanFault&&!repaired[1],CanFault&&!repaired[2],CanFault&&!repaired[3],Live&&FaultEvidence&&!diagnosed,
                Live&&diagnosed&&!servicing,CanArm,CanCheck&&!checkedSafe,checkedSafe,true,true,true,true};
            for(int i=0;i<buttons.Length;i++)buttons[i].interactable=enabled[i];
            if(!rig)return;
            rig.SetActualAttitude(Plant.Body,Plant.RateDegrees);
            rig.SetActualReactionWheels(Plant.WheelRpm,Plant.WheelUtil,Plant.WheelTemp);
            rig.SetActualMagneticSystem(Plant.Dipole,Plant.Field,Plant.Unloading);
            rig.SetActualNavigationSensors(Plant.Confidence,Plant.Uncertainty,Trusted,Plant.Catalogue,
                Mathf.Clamp01(Plant.GyroResidual.magnitude/.18f),Plant.Baffle,Plant.Calibration);
            rig.SetActualOpticalPointing(Plant.PointError,Plant.Jitter,Plant.QualifiedFine,Plant.OpticalLocked,Plant.LoadActivity,Plant.PassCount);
            rig.SetActualSafetyAndFaults(Trusted,FeedbackLatched||motionLatch,FeedbackLatched||motionLatch,
                Plant.BiasFault,Plant.WheelFault,Plant.FlexFault);
            rig.SetActualMissionState(Plant.MissionProgress,Plant.QualifiedFine?1:0,Stage/6f);
        }
    }
}
