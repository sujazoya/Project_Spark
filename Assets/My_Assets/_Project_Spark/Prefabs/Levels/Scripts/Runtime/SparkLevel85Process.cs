using System;
using System.Collections.Generic;
using AAAUI.VFX;
using ProjectSpark.Circuit;
using ProjectSpark.Visuals;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.Gameplay
{
    public sealed class SparkLevel85Process : MonoBehaviour
    {
        public static readonly string[] Gates={"L85_Network","L85_Calibrated","L85_Rendezvous","L85_Docked","L85_Faults","L85_Recertified"};
        public SparkLevelLayout layout;
        public SparkPowerSupply source,transducer;
        public SparkSwitch[] contacts;
        public SparkSwitch emergency;
        public SparkTerminal[] pins;
        public SparkTerminal ground;
        public SparkLevel85VisualRig rig;
        public Transform arena;
        public TextMesh[] pinLabels;
        public Text statusText,meterText,detailText,eventText;
        public Button[] buttons;
        public readonly SparkLevel85Plant Plant=new();
        public int Stage{get;private set;}
        public int RunId{get;private set;}
        public int Topology{get;private set;}=-1;
        public bool Trusted{get;private set;}
        public bool FeedbackLatched{get;private set;}
        public bool FeedbackObserved{get;private set;}
        public float ClosingAtLoss{get;private set;}
        public float RateEstimate{get;private set;}
        private const int W123=SparkLevel85Wiring.Feedback,W116=115,W117=116;
        private static readonly (int a,int b)[] Wires=SparkLevel85Wiring.Wires;
        private readonly Dictionary<SparkTerminal,int> lookup=new();
        private readonly List<SparkCircuitConnection> connections=new();
        private readonly HashSet<(int,int)> edges=new();
        private readonly bool[] routed=new bool[146], repaired=new bool[4];
        private LevelGamePlayManager manager;
        private SparkLevelLayoutLoader loader;
        private SparkLevelSceneBindings bindings;
        private SignalWireBuilder wireTool;
        private SparkWireRegistry registry;
        private SparkLevel85ArenaView view;
        private readonly Dictionary<Renderer,bool> hiddenCableRenderers=new();
        private bool focusCables,choseCableView;
        private enum Phase{Staging,Hold100,Hold30,Hold5,Final,Returning}
        private Phase phase;
        private float target=SparkLevel85Plant.Staging,accumulator,lastRail=-5,lastEmergency=-5,lastPermit=-5;
        private float contactClock,lastSample=-5,signalRefresh,signalLevel=-1,heldTime,repairTime,pulseUntil;
        private int wireFault=-2,shownWire=-3,shownPin=-2,safetyStep,fault,repairRun,tripRun,captureRun;
        private string latest="Connect the numbered docking terminal pairs.";
        private bool power,rails,emergencyOk,permit,safety,cal,pulsing,pulseDone,capture,latch,seal,release;
        private bool armed,feedbackAtEdge,feedbackReset,checkedSafe,holdLatch,diagnosed,servicing;
        private bool AllWires=>wireFault==-1;
        private bool Ready=>AllWires&&rails&&emergencyOk&&permit&&safety;
        private bool Live=>Ready&&!FeedbackLatched&&!holdLatch;
        private bool OnlyFeedbackMissing=>wireFault==W123&&edges.Count==145;
        private bool AtHold=>heldTime>.42f;
        private bool Captured=>Plant.Contact&&Plant.CaptureTravel>.98f;
        private bool CanApproach=>Stage>=2&&Stage<6&&Live&&Trusted&&AtHold&&fault==0&&!release&&
            phase is Phase.Staging or Phase.Hold100 or Phase.Hold30;
        private bool CanCapture=>Stage>=3&&Live&&Trusted&&phase==Phase.Hold5&&AtHold&&!capture;
        private bool CanArm=>Stage==5&&Live&&Trusted&&!armed&&!FeedbackObserved&&!Plant.Contact&&
            phase is Phase.Hold100 or Phase.Hold30 or Phase.Hold5 or Phase.Final&&Plant.Closing>.28f&&Plant.Range>2;
        private bool CanSafe=>(FeedbackLatched||holdLatch)&&Ready&&Trusted&&Plant.LiDARHealthy&&
            Mathf.Abs(Plant.RadarClosing-Plant.LidarClosing)<.085f&&Plant.Velocity.magnitude<.14f&&Plant.Fuel>.08f;
        private bool FaultEvidence=>fault switch
        {
            1=>Plant.LidarA<.31f&&Plant.LidarB>.78f,
            2=>Plant.ValveSeverity>.69f&&Plant.Valve[2]>.245f,
            3=>Plant.LatchSpread>.39f&&Plant.Contact,
            _=>false
        };

        private void Start()
        {
            manager=FindFirstObjectByType<LevelGamePlayManager>();
            loader=FindFirstObjectByType<SparkLevelLayoutLoader>();
            wireTool=FindFirstObjectByType<SignalWireBuilder>();
            if(!manager||!loader||!arena||pins.Length!=SparkLevel85Wiring.Pins.Length)
            {Debug.LogError("Level 85 arena bindings are incomplete.",this);enabled=false;return;}
            for(int i=0;i<pins.Length;i++)lookup.Add(pins[i],i);
            source.SetOutput(false,default);
            registry=FindFirstObjectByType<SparkWireRegistry>();
            if(registry)
            {
                registry.WiresChanged+=UpdateCableView;
                registry.SelectionChanged+=SelectCable;
            }
            view=gameObject.AddComponent<SparkLevel85ArenaView>();
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
            if(edges.Count!=Wires.Length-missing||missing>1&&wireFault==W123)wireFault=-2;
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
                if(OnlyFeedbackMissing&&armed&&!Plant.Contact&&Plant.Closing>.18f)
                {feedbackAtEdge=true;ClosingAtLoss=Plant.Closing;}
            }
            var solve=solver.LatestSolveResult;
            bool Read(int cable,out float voltage)
            {voltage=0;return solve!=null&&solve.TryGetVoltage(pins[Wires[cable].b],ground,out voltage);}
            bool High(int cable)=>Read(cable,out float volts)&&volts is >=4.6f and <=5.3f;
            bool present=power&&source.IsOutputActive&&!source.IsOverloaded;
            foreach(int branch in SparkLevel85Wiring.Power)present&=routed[branch]&&High(branch);
            if(present)lastRail=Plant.Clock;
            if(High(W116))lastEmergency=Plant.Clock;
            if(High(W117))lastPermit=Plant.Clock;
            rails=power&&source.IsOutputActive&&!source.IsOverloaded&&Plant.Clock-lastRail<.34f;
            emergencyOk=emergency.IsClosed&&routed[W116]&&Plant.Clock-lastEmergency<.34f;
            permit=contacts[0].IsClosed&&routed[W117]&&Plant.Clock-lastPermit<.34f;
            bool hasVoltage=Read(W123,out float volts);
            bool carrier=hasVoltage&&volts is >1.55f and <3.55f;
            RateEstimate=carrier?(volts-2.55f)/.105f:0;
            accumulator+=Mathf.Min(.14f,Time.deltaTime)*(armed?.58f:2.55f);
            while(accumulator>=SparkLevel85Plant.Step)
            {
                accumulator-=SparkLevel85Plant.Step;
                Tick(SparkLevel85Plant.Step,carrier,hasVoltage&&volts>.36f);
            }
            Present();
        }

        private void DriveSignal(float value)
        {
            bool active=rails&&routed[W123];
            if(!active)
            {
                if(transducer.IsOutputActive)transducer.SetOutput(false,default);
                signalLevel=-1;return;
            }
            if(Mathf.Abs(signalLevel-value)<.018f&&transducer.IsOutputActive)return;
            transducer.SetOutput(false,default);
            if(!transducer.TryConfigure(value,.01f,out string reason)){Event(reason);return;}
            transducer.SetOutput(true,default);signalLevel=value;
        }

        private void Tick(float dt,bool carrier,bool continuity)
        {
            contactClock+=dt;
            if(safetyStep==1&&!permit&&emergencyOk&&contactClock>.21f)
            {contacts[0].SetState(SparkSwitchState.Closed);safetyStep=2;contactClock=0;}
            else if(safetyStep==2&&permit&&emergencyOk&&contactClock>.21f)
            {safety=true;safetyStep=0;Event("Independent propulsion permit and emergency hold verified.");}
            signalRefresh+=dt;
            if(signalRefresh>.24f)
            {
                signalRefresh=0;
                float encoded=2.55f+.105f*Mathf.Clamp(-Plant.EstimatedVelocity.x,-8.7f,8.7f);
                DriveSignal(Plant.OwnNavFresh&&Plant.LiDARHealthy?encoded:.52f);
            }
            if(carrier&&routed[W123]&&Plant.OwnNavFresh&&Plant.LiDARHealthy&&rails)lastSample=Plant.Clock;
            Trusted=routed[W123]&&Plant.Clock-lastSample<.48f&&Plant.OwnNavFresh&&Plant.LiDARHealthy&&rails;
            if(Stage>=2&&OnlyFeedbackMissing&&!FeedbackLatched&&Plant.Clock-lastSample>.50f)
            {
                FeedbackLatched=true;FeedbackObserved|=armed&&feedbackAtEdge;armed=false;
                checkedSafe=false;tripRun=RunId;
                Event("W123 stale. Capture permission withdrawn and independent radar braking engaged.");
            }
            if(Stage>=2&&!emergencyOk&&!holdLatch)
            {holdLatch=true;checkedSafe=false;Event("Emergency hold engaged through independent motion safety.");}
            if(pulsing&&Plant.Clock>=pulseUntil)
            {
                pulsing=false;pulseDone=Plant.CalibrationImpulse>.08f&&Plant.Fuel<1;
                Event(pulseDone?"Finite RCS pulse and propellant usage verified.":"Pulse evidence is incomplete. Repeat calibration.");
            }
            bool braking=!Trusted||FeedbackLatched||holdLatch||!emergencyOk||
                (capture&&!Plant.Contact&&Plant.Range<1.2f&&RateEstimate>.58f);
            bool moving=phase!=Phase.Staging||pulsing||fault==2;
            Plant.Tick(dt,rails,permit,cal,pulsing,moving,target,braking,capture&&Live&&Trusted,
                latch,seal,release,fault,servicing);
            bool settled=Mathf.Abs(Plant.Range-target)<.25f&&Plant.Velocity.magnitude<.20f&&Plant.Lateral<.23f;
            heldTime=settled?heldTime+dt:0;
            if(phase==Phase.Returning&&AtHold)
            {
                phase=Phase.Staging;release=capture=latch=seal=false;
                Event("Service spacecraft back at staging. A new approach is ready.");
            }
            if(servicing)
            {
                repairTime+=dt;
                float severity=fault==1?Plant.LidarSeverity:fault==2?Plant.ValveSeverity:Plant.LatchSeverity;
                if(severity<.047f&&repairTime>1.0f)
                {
                    repaired[fault]=true;repairRun=RunId;fault=0;servicing=diagnosed=false;
                    Event("Measured fault cleared. A fresh full docking is required after the repairs.");
                }
            }
            if(Stage==0&&Ready&&continuity)Award();
            else if(Stage==1&&Live&&Trusted&&pulseDone)Award();
            else if(Stage==2&&phase==Phase.Hold5&&AtHold&&Trusted&&Plant.Fuel>.2f)Award();
            else if(Plant.Qualified&&captureRun==RunId&&Live&&Trusted)
            {
                if(Stage==3)Award();
                else if(Stage==4&&repaired[1]&&repaired[2]&&repaired[3]&&RunId>repairRun)Award();
                else if(Stage==5&&feedbackReset&&RunId>tripRun)Award();
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
        public void CalNav(){if(Ready){cal=true;Event("Calibrate stereo, LiDAR A/B, radar and inertial sampling.");}}
        public void CalPulse()
        {
            if(Live&&Trusted&&!pulseDone&&!pulsing)
            {pulsing=true;pulseUntil=Plant.Clock+.65f;Event("Running finite RCS pulse in the isolated checkout fixture.");}
        }
        public void Approach()
        {
            if(!CanApproach)return;
            if(phase==Phase.Staging){RunId++;phase=Phase.Hold100;target=100;}
            else if(phase==Phase.Hold100){phase=Phase.Hold30;target=30;}
            else {phase=Phase.Hold5;target=5;}
            heldTime=0;
            Event($"Run {RunId} advancing to {target:0} m. Wait for physically settled hold.");
        }
        public void Capture()
        {
            if(CanCapture){phase=Phase.Final;target=.07f;capture=true;captureRun=RunId;heldTime=0;Event("Terminal approach authorized. Contact and soft capture must settle.");}
        }
        public void Latch()
        {
            if(Live&&Trusted&&Captured&&!latch)
            {latch=true;Event("Six independent latches closing. Compare their actual travel.");}
        }
        public void Seal()
        {
            if(Live&&Trusted&&Captured&&Plant.Locks==6&&!seal)
            {seal=true;Event("Testing coupler extension, interface voltage and pressure seal.");}
        }
        public void Retreat()
        {
            if(Live&&Trusted&&!armed&&Stage<6&&phase!=Phase.Staging&&phase!=Phase.Returning)
            {
                phase=Phase.Returning;target=SparkLevel85Plant.Staging;heldTime=0;
                release=true;capture=latch=seal=false;
                Event("Controlled undock and retreat. Latches and seals release before RCS translation.");
            }
        }
        private void Disturb(int test)
        {
            if(!Live||Stage!=4||fault!=0||repaired[test])return;
            fault=test;diagnosed=servicing=false;
            Event(test==1?"Primary LiDAR shows a real dropout. Compare the other range channels.":test==2?
                "A lateral valve remains open. Observe actual flow during standby.":"One latch is obstructed. Command latches and observe unequal travel.");
        }
        public void LidarFault(){if(Plant.Qualified)Disturb(1);}
        public void ValveFault(){if(phase==Phase.Staging&&AtHold)Disturb(2);}
        public void LatchFault(){if(Captured&&!latch)Disturb(3);}
        public void Diagnose()
        {if(Live&&FaultEvidence){diagnosed=true;Event("Physical fault isolated from live equipment measurements.");}}
        public void Repair()
        {if(Live&&diagnosed&&!servicing){servicing=true;repairTime=0;Event("Service started. Wait for the actual channel or mechanism to recover.");}}
        public void Arm()
        {if(CanArm){armed=true;feedbackAtEdge=false;Event("W123 armed under active closing motion. Remove the amber wire now.");}}
        public void CheckSafe(){if(CanSafe){checkedSafe=true;Event("Fresh range-rate agrees with independent radar and retreat is stable.");}}
        public void ResetSafety()
        {
            if(!checkedSafe||!CanSafe)return;
            feedbackReset|=FeedbackLatched&&FeedbackObserved;
            FeedbackLatched=holdLatch=checkedSafe=false;Event("Safety reset. Retreat to staging and run a new full docking.");
        }
        public void Stop()=>emergency.SetState(emergency.IsClosed?SparkSwitchState.Open:SparkSwitchState.Closed);
        public void ArenaView()=>view?.UseOverview();
        public void SocketView()=>view?.UseSocketView();
        public void DockView()=>view?.UseDockView();
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
            var (sourcePin,targetPin)=Wires[W123];
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
            if(registry){registry.WiresChanged-=UpdateCableView;registry.SelectionChanged-=SelectCable;}
            RestoreCableView();
        }

        private string Instruction()
        {
            if(wireFault>=0)
            {
                var (a,b)=Wires[wireFault];
                return $"W{wireFault+1:000}  ·  {SparkLevel85Wiring.Labels[a]} → {SparkLevel85Wiring.Labels[b]}";
            }
            if(wireFault==-2)return "Check the fixed terminals for an extra or misplaced cable.";
            if(!power)return "Turn on the 5 V control network.";
            if(!safety)return "Test the independent motion permit.";
            if(FeedbackLatched||holdLatch)return checkedSafe?"Reset safety and retreat to staging.":"Wait for trusted rate and independent radar to agree, then check safe.";
            if(Stage==6)return "RENDEZVOUS AND DOCKING QUALIFIED";
            if(Stage==1)return !cal?"Calibrate relative-navigation channels.":"Verify a finite RCS calibration pulse.";
            if(armed)return "W123 ARMED  ·  REMOVE THE AMBER CABLE NOW";
            if(fault!=0)return "Diagnose the measured fault, then service the affected mechanism.";
            if(Plant.Qualified)return Stage==4&&!repaired[1]?"Apply the primary LiDAR blind spot.":"Undock for the next flight qualification.";
            if(phase==Phase.Returning)return "Controlled retreat to 112 m staging.";
            if(Captured)return Plant.Locks<6?"Lock all six latches.":"Verify seal pressure and electrical mate.";
            if(phase==Phase.Final)return "Terminal approach and compliant capture in progress.";
            if(phase==Phase.Hold5&&AtHold)return "Hold verified. Start soft capture when authorized.";
            if(Stage==4&&phase==Phase.Staging&&!repaired[2])return "Test the stuck valve at staging, then conduct a new approach.";
            return AtHold?"Advance to the next corridor hold point.":$"Settling at the {target:0} m hold.";
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
            meterText.text=$"RANGE {Plant.Range:0.00} m  ·  CLOSING {Plant.Closing:+0.00;-0.00;0.00} m/s  ·  LATERAL {Plant.Lateral:0.00} m  ·  W123 {(Trusted?"FRESH":"STALE")}";
            detailText.text=$"RCS {Plant.Fuel:P0}  ·  LiDAR A/B {Plant.LidarA:P0}/{Plant.LidarB:P0}  ·  LATCHES {Plant.Locks}/6  ·  SEAL {Plant.Pressure:P0}  ·  MATE {Plant.InterfaceVoltage:0.0} V";
            eventText.text=latest;
            bool canFault=Stage==4&&Live&&fault==0;
            bool[] enabled={true,rails&&emergencyOk&&safetyStep==0&&!safety,Ready&&!cal,Live&&Trusted&&!pulseDone&&!pulsing,
                CanApproach,CanCapture,Live&&Trusted&&Captured&&!latch,Live&&Trusted&&Captured&&Plant.Locks==6&&!seal,
                Live&&Trusted&&!armed&&Stage<6&&phase!=Phase.Staging&&phase!=Phase.Returning,
                canFault&&!repaired[1]&&Plant.Qualified,canFault&&!repaired[2]&&phase==Phase.Staging&&AtHold,
                canFault&&!repaired[3]&&Captured&&!latch,Live&&FaultEvidence&&!diagnosed,
                Live&&diagnosed&&!servicing,CanArm,CanSafe&&!checkedSafe,checkedSafe,true,true,true,true,true};
            for(int i=0;i<buttons.Length;i++)buttons[i].interactable=enabled[i];
            if(!rig)return;
            float x=.58f*(1-Mathf.Clamp01((Plant.Range-SparkLevel85Plant.ContactPlane)/(SparkLevel85Plant.Staging-SparkLevel85Plant.ContactPlane)));
            rig.SetActualServiceSpacecraftPose(new Vector3(x,Plant.Position.y*.035f,-Plant.Position.z*.035f),Plant.Body);
            rig.SetActualRelativeNavigation(Plant.EstimatedPosition.x,RateEstimate,Plant.Lateral,Plant.AttitudeError,Trusted);
            rig.SetActualLiDARScanHeadAngles(Plant.ScanA,Plant.ScanB);
            rig.SetActualThrusterPulseLevels(Plant.Valve[0],Plant.Valve[1],Plant.Valve[2],Plant.Valve[3],Plant.Valve[4],Plant.Valve[5]);
            rig.SetActualDockingMechanisms(Plant.CaptureTravel,Plant.Locks/6f,Plant.Seal,Plant.Coupler,Plant.Contact,Plant.Qualified);
            rig.SetActualSafetyState(Trusted,FeedbackLatched||holdLatch,FeedbackLatched||holdLatch||phase==Phase.Returning);
        }
    }
}
