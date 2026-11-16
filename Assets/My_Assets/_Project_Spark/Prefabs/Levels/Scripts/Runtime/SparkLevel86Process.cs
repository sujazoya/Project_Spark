using System;
using System.Collections.Generic;
using AAAUI.VFX;
using ProjectSpark.Circuit;
using ProjectSpark.Visuals;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.Gameplay
{
    public sealed class SparkLevel86Process : MonoBehaviour
    {
        public static readonly string[] Gates={"L86_Network","L86_Commission","L86_Extracted","L86_Cooling","L86_Faults","L86_Recertified"};
        public SparkLevelLayout layout;
        public SparkPowerSupply source,transducer;
        public SparkSwitch[] contacts;
        public SparkSwitch emergency;
        public SparkTerminal[] pins;
        public SparkTerminal ground;
        public SparkLevel86VisualRig rig;
        public Transform arena,failedVisual,replacementVisual;
        public TextMesh[] pinLabels;
        public Text statusText,meterText,detailText,eventText;
        public Button[] buttons;
        public readonly SparkLevel86Plant Plant=new();
        public int Stage{get;private set;}
        public int RunId{get;private set;}
        public int Topology{get;private set;}=-1;
        public bool Trusted{get;private set;}
        public bool FeedbackLatched{get;private set;}
        public bool FeedbackObserved{get;private set;}
        public float SpeedAtLoss{get;private set;}
        public float MeasuredForce{get;private set;}
        private const int W129=SparkLevel86Wiring.Feedback,W113=112,W114=113;
        private static readonly (int a,int b)[] Wires=SparkLevel86Wiring.Wires;
        private readonly Dictionary<SparkTerminal,int> lookup=new();
        private readonly List<SparkCircuitConnection> connections=new();
        private readonly HashSet<(int,int)> edges=new();
        private readonly bool[] routed=new bool[152],repaired=new bool[4];
        private LevelGamePlayManager manager;
        private SparkLevelLayoutLoader loader;
        private SparkLevelSceneBindings bindings;
        private SignalWireBuilder wireTool;
        private SparkWireRegistry registry;
        private SparkLevel86ArenaView view;
        private readonly Dictionary<Renderer,bool> hiddenCableRenderers=new();
        private bool focusCables,choseCableView;
        private enum Phase {Idle,TrialOut,TrialHome,Isolating,Approaching,AtPanel,Unbolting,Gripping,Grabbed,
            ExtractClear,ExtractCradle,AwaitFetch,Fetching,Pickup,HoldingReplacement,InstallClear,Installing,Installed,Fastening,ReadyFlow,Cooling,Done}
        private Phase phase;
        private Vector3 failedStart,replacementStart;
        private float accumulator,lastRail=-5,lastEmergency=-5,lastPermit=-5,contactClock,lastSample=-5,signalRefresh,signalLevel=-1;
        private int wireFault=-2,shownWire=-3,shownPin=-2,safetyStep,fault,repairRun,tripRun,sampleAtTrip,recalibrationStart,serviceItem=1;
        private string latest="Connect the numbered equipment terminal pairs.";
        private bool power,rails,emergencyOk,permit,safety,cal,homeCommand,grip,pump=true,armed,feedbackAtEdge,feedbackReset,checkedSafe,recalibrating,holdLatch,diagnosed,servicing;
        private bool Ready=>wireFault==-1&&rails&&emergencyOk&&permit&&safety;
        private bool Live=>Ready&&!FeedbackLatched&&!holdLatch;
        private bool OnlyFeedbackMissing=>wireFault==W129&&edges.Count==Wires.Length-1;
        private bool Operational=>Live&&Trusted&&Plant.ContactSafe;
        private bool CanStart=>Operational&&fault==0&&Stage is >=2 and <6&&
            (phase==Phase.Idle||phase==Phase.Done&&(Stage!=4||AllRepairs));
        private bool AllRepairs=>repaired[1]&&repaired[2]&&repaired[3];
        private bool CanArm=>Stage==5&&Operational&&!armed&&!FeedbackObserved&&
            (phase is Phase.ExtractClear or Phase.ExtractCradle)&&Plant.Carried!=0&&Plant.TipSpeed>.12f&&Plant.Force>4;
        private bool CanSafe=>(FeedbackLatched||holdLatch)&&Ready&&Trusted&&Plant.Stopped&&Plant.ContactSafe&&
            Plant.WrenchSequence>sampleAtTrip&&Mathf.Abs(MeasuredForce-Plant.Force)<2f;
        private bool CanDisturb=>Operational&&Stage==4&&phase==Phase.Done&&fault==0;
        private Vector3 SupportPoint=>serviceItem==1?SparkLevel86Plant.Cradle:SparkLevel86Plant.ReplacementRack;
        private bool FaultEvidence=>fault switch
        {
            1=>Plant.EncoderBias>3&&Plant.MoveError>.08f,
            2=>Plant.FlexMotion>.125f,
            3=>Plant.LeftLoad-Plant.RightLoad>4,
            _=>false
        };

        private void Start()
        {
            manager=FindFirstObjectByType<LevelGamePlayManager>();
            loader=FindFirstObjectByType<SparkLevelLayoutLoader>();
            wireTool=FindFirstObjectByType<SignalWireBuilder>();
            if(!manager||!loader||!arena||pins.Length!=SparkLevel86Wiring.Pins.Length)
            {Debug.LogError("Level 86 arena bindings are incomplete.",this);enabled=false;return;}
            for(int i=0;i<pins.Length;i++)lookup.Add(pins[i],i);
            source.SetOutput(false,default);
            registry=FindFirstObjectByType<SparkWireRegistry>();
            if(registry)
            {
                registry.WiresChanged+=UpdateCableView;
                registry.SelectionChanged+=SelectCable;
            }
            view=gameObject.AddComponent<SparkLevel86ArenaView>();
            view.Initialize(arena);
            if(failedVisual)failedStart=failedVisual.localPosition;
            if(replacementVisual)replacementStart=replacementVisual.localPosition;
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
            if(edges.Count!=Wires.Length-missing)wireFault=-2;
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
                if(OnlyFeedbackMissing&&armed&&Plant.Carried!=0&&Plant.TipSpeed>.07f)
                {feedbackAtEdge=true;SpeedAtLoss=Plant.TipSpeed;}
            }
            var solve=solver.LatestSolveResult;
            bool Read(int cable,out float voltage)
            {voltage=0;return solve!=null&&solve.TryGetVoltage(pins[Wires[cable].b],ground,out voltage);}
            bool High(int cable)=>Read(cable,out float volts)&&volts is >=4.6f and <=5.3f;
            bool present=power&&source.IsOutputActive&&!source.IsOverloaded;
            foreach(int branch in SparkLevel86Wiring.Power)present&=routed[branch]&&High(branch);
            if(present)lastRail=Plant.Clock;
            if(High(W113))lastEmergency=Plant.Clock;
            if(High(W114))lastPermit=Plant.Clock;
            rails=power&&source.IsOutputActive&&!source.IsOverloaded&&Plant.Clock-lastRail<.34f;
            emergencyOk=emergency.IsClosed&&routed[W113]&&Plant.Clock-lastEmergency<.34f;
            permit=contacts[0].IsClosed&&routed[W114]&&Plant.Clock-lastPermit<.34f;
            bool hasVoltage=Read(W129,out float volts);
            bool carrier=hasVoltage&&volts is >1.8f and <3.6f;
            MeasuredForce=carrier?(volts-2.35f)/.020f:0;
            accumulator+=Mathf.Min(.14f,Time.deltaTime)*(armed?.35f:3.6f);
            while(accumulator>=SparkLevel86Plant.Step)
            {
                accumulator-=SparkLevel86Plant.Step;
                Tick(SparkLevel86Plant.Step,carrier,hasVoltage&&volts>.36f);
            }
            Present();
        }

        private void DriveSignal(float value)
        {
            bool active=rails&&routed[W129];
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
            {emergency.SetState(SparkSwitchState.Open);safetyStep=3;contactClock=0;}
            else if(safetyStep==3&&permit&&!emergencyOk&&contactClock>.21f)
            {emergency.SetState(SparkSwitchState.Closed);safetyStep=4;contactClock=0;}
            else if(safetyStep==4&&permit&&emergencyOk&&contactClock>.21f)
            {safety=true;safetyStep=0;Event("Independent contact relay and emergency stop circuit verified.");}

            signalRefresh+=dt;
            if(signalRefresh>.16f)
            {
                signalRefresh=0;
                DriveSignal(Plant.Calibration>.95f?2.35f+.020f*Mathf.Clamp(Plant.Force,0,45):.52f);
            }
            bool solvedSample=carrier&&routed[W129]&&Plant.Calibration>.95f&&rails;
            if(solvedSample)lastSample=Plant.Clock;
            Trusted=solvedSample&&Plant.Clock-lastSample<.49f&&Plant.WrenchSampleAge<.47f;
            if(Stage>=2&&!routed[W129]&&!FeedbackLatched&&Plant.Clock-lastSample>.50f)
            {
                FeedbackLatched=true;FeedbackObserved|=armed&&feedbackAtEdge;armed=false;
                checkedSafe=recalibrating=false;tripRun=RunId;sampleAtTrip=Plant.WrenchSequence;
                Event("W129 stale. Motion braked. Gripper retains the supported load.");
            }
            if(Stage>=2&&(!emergencyOk||!permit)&&!holdLatch&&safetyStep==0)
            {
                holdLatch=true;checkedSafe=recalibrating=false;sampleAtTrip=Plant.WrenchSequence;
                Event("Independent interlock stopped motion.");
            }

            int boltCommand=Operational?(phase==Phase.Unbolting?-1:phase==Phase.Fastening?1:0):0;
            Plant.Tick(dt,rails,Operational||Live&&Stage==1&&homeCommand,cal,solvedSample,grip,fault is 2 or 3,
                boltCommand,pump,fault,servicing);
            if(recalibrating)
            {
                if(!CanSafe)recalibrating=false;
                else if(Plant.WrenchSequence>=recalibrationStart+3)
                {
                    checkedSafe=true;recalibrating=false;
                    Event("Three fresh wrench samples agree with the stationary wrist. Safety reset is available.");
                }
            }
            if(servicing&&Plant.EncoderBias<.18f&&Plant.FlexMotion<.075f&&Plant.JawFault<.06f)
            {
                repaired[fault]=true;repairRun=RunId;fault=0;servicing=diagnosed=false;
                Event("Repair measured and verified. Run a fresh radiator exchange.");
            }
            if(Stage==0&&Ready&&continuity)Award();
            if(Operational&&fault==0)Advance();
        }

        private void Move(Phase next,Vector3 target)
        {
            Plant.Move(target);phase=next;
            Event($"Run {RunId}  ·  {next}");
        }

        private void Advance()
        {
            switch(phase)
            {
                case Phase.TrialOut when Plant.AtGoal:
                    Plant.Home();phase=Phase.TrialHome;
                    break;
                case Phase.TrialHome when Plant.HomeReady:
                    phase=Phase.Idle;Award();
                    break;
                case Phase.Approaching when Plant.AtGoal:
                    phase=Phase.AtPanel;
                    break;
                case Phase.Gripping when Plant.BalancedGrip:
                    Plant.Attach(serviceItem);
                    if(Plant.Carried!=0)phase=Phase.Grabbed;
                    break;
                case Phase.ExtractClear when Plant.AtGoal:
                    Move(Phase.ExtractCradle,SupportPoint);
                    break;
                case Phase.ExtractCradle when Plant.AtGoal&&armed:
                    Move(Phase.ExtractClear,SparkLevel86Plant.RetreatPoint);
                    break;
                case Phase.ExtractCradle when Plant.AtGoal&&!armed:
                    Plant.Support(SupportPoint);
                    if(Plant.Carried==0)
                    {
                        grip=false;phase=Phase.AwaitFetch;
                        if(Stage==2&&serviceItem==1)Award();
                    }
                    break;
                case Phase.Fetching when Plant.AtGoal:
                    grip=true;phase=Phase.Pickup;
                    break;
                case Phase.Pickup when Plant.BalancedGrip:
                    Plant.Attach(2);
                    if(Plant.Carried==2)phase=Phase.HoldingReplacement;
                    break;
                case Phase.InstallClear when Plant.AtGoal:
                    Move(Phase.Installing,SparkLevel86Plant.PanelSocket);
                    break;
                case Phase.Installing when Plant.AtGoal:
                    Plant.Install();
                    if(Plant.Carried==0){grip=false;phase=Phase.Installed;}
                    break;
                case Phase.Fastening when Plant.Fastened:
                    phase=Phase.ReadyFlow;
                    break;
                case Phase.Cooling when Plant.Cool:
                    phase=Phase.Done;
                    if(Stage==3||Stage==4&&AllRepairs&&RunId>repairRun||Stage==5&&feedbackReset&&RunId>tripRun)Award();
                    break;
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
        public void Home(){if(Live&&Stage==1&&!homeCommand){Plant.Home();homeCommand=true;Event("Homing the traverse and seven joints.");}}
        public void Calibrate(){if(Live&&Plant.HomeReady&&homeCommand&&Stage==1){cal=true;Event("Calibrating encoders, stereo pose and wrist wrench.");}}
        public void Trial(){if(Operational&&Plant.HomeReady&&Stage==1)Move(Phase.TrialOut,SparkLevel86Plant.TestPoint);}
        public void Isolate()
        {
            if(!CanStart)return;
            RunId++;pump=false;grip=false;phase=Phase.Isolating;
            serviceItem=Plant.ReplacementInstalled?2:1;Plant.BeginService();
            Event("Coolant isolated. Wait for pressure and flow to fall.");
        }
        public void Approach(){if(Operational&&phase==Phase.Isolating&&Plant.Vented)Move(Phase.Approaching,SparkLevel86Plant.PanelSocket);}
        public void Unbolt(){if(Operational&&phase==Phase.AtPanel&&Plant.AtGoal)phase=Phase.Unbolting;}
        public void Grip(){if(Operational&&phase==Phase.Unbolting&&Plant.Unbolted){grip=true;phase=Phase.Gripping;}}
        public void Extract()
        {
            if(Operational&&phase==Phase.Grabbed&&Plant.BalancedGrip)
            {Plant.ReplacementInstalled=false;Move(Phase.ExtractClear,SparkLevel86Plant.RetreatPoint);}
        }
        public void Fetch(){if(Operational&&phase==Phase.AwaitFetch)Move(Phase.Fetching,Plant.ReplacementPose);}
        public void Install(){if(Operational&&phase==Phase.HoldingReplacement)Move(Phase.InstallClear,SparkLevel86Plant.RetreatPoint);}
        public void Fasten(){if(Operational&&phase==Phase.Installed&&Plant.AtGoal)phase=Phase.Fastening;}
        public void Flow(){if(Operational&&phase==Phase.ReadyFlow){pump=true;Plant.BeginCooling();phase=Phase.Cooling;}}
        private void Disturb(int test)
        {
            if(!CanDisturb||repaired[test])return;
            fault=test;Event(test==1?"Encoder drift applied. Compare actual tool pose to command.":test==2?
                "Flexible load excitation applied. Observe the measured deflection.":"Jaw pressure fault applied. Compare measured loads.");
        }
        public void EncoderFault()=>Disturb(1);
        public void FlexFault()=>Disturb(2);
        public void JawFault()=>Disturb(3);
        public void Diagnose(){if(Live&&FaultEvidence){diagnosed=true;Event("Physical fault isolated from live measurements.");}}
        public void Repair(){if(Live&&diagnosed&&!servicing){servicing=true;Event("Repair started. Wait for measured recovery.");}}
        public void Arm(){if(CanArm){armed=true;feedbackAtEdge=false;Event("W129 armed during panel transport. Remove the amber wire now.");}}
        public void CheckSafe()
        {
            if(!CanSafe)return;
            recalibrating=true;recalibrationStart=Plant.WrenchSequence;
            Event("Checking three fresh force samples with the brake engaged.");
        }
        public void ResetSafety()
        {
            if(!checkedSafe||!CanSafe)return;
            feedbackReset|=FeedbackLatched&&FeedbackObserved;
            FeedbackLatched=holdLatch=checkedSafe=false;
            Event("Complete the interrupted exchange. Start a new exchange to recertify.");
        }
        public void Stop()=>emergency.SetState(emergency.IsClosed?SparkSwitchState.Open:SparkSwitchState.Closed);
        public void ArenaView()=>view?.UseOverview();
        public void ServiceView()=>view?.UseServiceView();

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
            var (sourcePin,targetPin)=Wires[W129];
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
            if(FeedbackLatched||holdLatch)return checkedSafe?"Reset safety and resume the interrupted exchange.":
                "Restore the interlock and W129, then select RECAL / CHECK.";
            if(wireFault>=0)
            {
                var (a,b)=Wires[wireFault];
                return $"W{wireFault+1:000}  ·  {SparkLevel86Wiring.Labels[a]} → {SparkLevel86Wiring.Labels[b]}";
            }
            if(wireFault==-2)return "Check for an extra or misplaced cable.";
            if(!power)return "Turn on the 5 V control network.";
            if(!safety)return "Test the independent contact interlock.";
            if(Stage==6)return "ORBITAL RADIATOR SERVICE QUALIFIED";
            if(armed)return "W129 ARMED  ·  REMOVE THE AMBER CABLE";
            if(fault!=0)return diagnosed?"Repair the measured fault.":"Inspect live measurements and select DIAGNOSE.";
            if(Stage==1&&phase==Phase.Idle)return !Plant.HomeReady||!homeCommand?"Home all seven axes and the traverse.":
                !Trusted?"Calibrate the sensor channels.":"Run the dry trial path.";
            return phase switch
            {
                Phase.Idle=>"Isolate and vent the old radiator.",
                Phase.Isolating=>Plant.Vented?"Approach the isolated radiator.":"Venting the coolant loop.",
                Phase.AtPanel=>"Release all four captive bolts.",
                Phase.Unbolting=>Plant.Unbolted?"Close the gripper on the panel.":"Waiting for all captive bolts to release.",
                Phase.Grabbed=>"Extract and lower to the support cradle.",
                Phase.AwaitFetch=>"Pick up the replacement radiator.",
                Phase.HoldingReplacement=>"Install the replacement in the mounting truss.",
                Phase.Installed=>"Torque all four fasteners.",
                Phase.ReadyFlow=>"Test the seal and restore coolant flow.",
                Phase.Cooling=>"Verifying pressure, flow and falling temperature.",
                Phase.Done=>Stage==4&&!AllRepairs?"Test encoder drift, flex and asymmetric grip.":"Start the next service run.",
                Phase.ExtractClear or Phase.ExtractCradle when CanArm=>"Arm W129 while the panel is moving.",
                _=>"Allow the robot to complete the active movement."
            };
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
            meterText.text=$"WRIST {Plant.Force:0.0} N  ·  JAWS {Plant.LeftLoad:0.0}/{Plant.RightLoad:0.0} N  ·  DRIFT {Plant.EncoderBias:0.0}°  ·  FLEX {Plant.FlexMotion:0.00}  ·  W129 {(Trusted?"FRESH":"STALE")}";
            detailText.text=$"COOLANT {Plant.Temperature:0.0} °C  ·  PRESSURE {Plant.Pressure:P0}  ·  FLOW {Plant.Flow:P0}  ·  RUN {RunId}";
            eventText.text=latest;
            bool[] enabled={true,rails&&emergencyOk&&safetyStep==0&&!safety,Live&&Stage==1&&!homeCommand,
                Live&&Plant.HomeReady&&homeCommand&&Stage==1&&!cal,Operational&&Plant.HomeReady&&Stage==1&&phase==Phase.Idle,
                CanStart,Operational&&phase==Phase.Isolating&&Plant.Vented,Operational&&phase==Phase.AtPanel&&Plant.AtGoal,
                Operational&&phase==Phase.Unbolting&&Plant.Unbolted,Operational&&phase==Phase.Grabbed&&Plant.BalancedGrip,
                Operational&&phase==Phase.AwaitFetch,Operational&&phase==Phase.HoldingReplacement,
                Operational&&phase==Phase.Installed&&Plant.AtGoal,Operational&&phase==Phase.ReadyFlow,
                CanDisturb&&!repaired[1],CanDisturb&&!repaired[2],CanDisturb&&!repaired[3],
                Live&&FaultEvidence&&!diagnosed,Live&&diagnosed&&!servicing,CanArm,CanSafe&&!checkedSafe&&!recalibrating,checkedSafe&&CanSafe,true,true,true,true};
            for(int i=0;i<buttons.Length;i++)buttons[i].interactable=enabled[i];
            if(!rig)return;
            rig.SetActualRobotJointAngles(Plant.J(0),Plant.J(1),Plant.J(2),Plant.J(3),Plant.J(4),Plant.J(5),Plant.J(6),Plant.Rail);
            rig.SetActualServicingMechanisms(Plant.Jaw,0,0,Plant.Valve*90);
            rig.SetServiceMotion(Plant.Clock,Plant.Bolts,Plant.Jaw,Plant.TipSpeed,Plant.Flow,Plant.Calibration,
                FeedbackLatched||holdLatch,Plant.FailedPose==SparkLevel86Plant.Cradle);
            rig.SetActualSafetyState(Trusted,FeedbackLatched||holdLatch,Plant.Cool&&phase==Phase.Done);
            if(failedVisual)failedVisual.localPosition=failedStart+(Plant.FailedPose-SparkLevel86Plant.PanelSocket)*.096f;
            if(replacementVisual)replacementVisual.localPosition=replacementStart+(Plant.ReplacementPose-SparkLevel86Plant.ReplacementRack)*.096f;
        }
    }
}

