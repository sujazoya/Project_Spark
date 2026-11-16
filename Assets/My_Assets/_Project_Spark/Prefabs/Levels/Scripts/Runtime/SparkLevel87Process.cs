using System;
using System.Collections.Generic;
using AAAUI.VFX;
using ProjectSpark.Circuit;
using ProjectSpark.Visuals;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.Gameplay
{
    public sealed class SparkLevel87Process : MonoBehaviour
    {
        public static readonly string[] Gates={"L87_Network","L87_Levitation","L87_Spin","L87_Gravity","L87_Faults","L87_Recertified"};
        public SparkLevelLayout layout;
        public SparkPowerSupply source,transducer,secondary;
        public SparkSwitch[] contacts;
        public SparkSwitch emergency;
        public SparkTerminal[] pins;
        public SparkTerminal ground;
        public SparkLevel87VisualRig rig;
        public Transform arena;
        public TextMesh[] pinLabels,componentLabels;
        public string[] componentNames;
        public Text statusText,meterText,detailText,eventText;
        public Button[] buttons;
        public readonly SparkLevel87Plant Plant=new();
        public int Stage{get;private set;}
        public int RunId{get;private set;}
        public int Topology{get;private set;}=-1;
        public bool Trusted{get;private set;}
        public bool FeedbackLatched{get;private set;}
        public bool FeedbackObserved{get;private set;}
        public float SpeedAtLoss{get;private set;}
        public bool OverspeedLatched{get;private set;}
        private const int W137=SparkLevel87Wiring.Feedback,W135=134,W123=122,W124=123;
        private static readonly (int a,int b)[] Wires=SparkLevel87Wiring.Wires;
        private readonly Dictionary<SparkTerminal,int> lookup=new();
        private readonly List<SparkCircuitConnection> connections=new();
        private readonly HashSet<(int,int)> edges=new();
        private readonly bool[] routed=new bool[158], repaired=new bool[4];
        private LevelGamePlayManager manager;
        private SparkLevelLayoutLoader loader;
        private SparkLevelSceneBindings bindings;
        private SignalWireBuilder wireTool;
        private SparkWireRegistry registry;
        private SparkLevel87ArenaView view;
        private readonly Dictionary<Renderer,bool> hiddenCableRenderers=new();
        private bool focusCables,choseCableView;
        private enum Phase{Rest,Lifting,Levitation,Precharging,Trial,Scanning,Scanned,SpinUp,Hold,Stopping}
        private Phase phase;
        private float accumulator,lastRail=-5,lastEmergency=-5,lastPermit=-5,contactClock,scanTime;
        private float signalRefresh,signalLevel=-1,secondaryLevel=-1,goodGapTime,primaryGap,secondaryGap;
        private int wireFault=-2,shownWire=-3,shownPin=-2,safetyStep,fault,lastRepairRun,tripRun,sampleAtTrip,calibrationAtTrip;
        private string latest="Connect the numbered equipment terminals.";
        private bool power,rails,emergencyOk,permit,safety,cal,arm,feedbackAtEdge,feedbackReset,checkedSafe,checking,holdLatch;
        private bool balance,habitat,diagnosed,servicing,certifiedRun,rotary,lowTestPassed;
        private bool Ready=>wireFault==-1&&rails&&emergencyOk&&permit&&safety;
        private bool Live=>Ready&&Trusted&&!FeedbackLatched&&!holdLatch&&!OverspeedLatched;
        private bool OnlyFeedbackMissing=>wireFault==W137&&edges.Count==Wires.Length-1;
        private bool AllRepairs=>repaired[1]&&repaired[2]&&repaired[3];
        private bool CanLift=>Stage is >=1 and <6&&Live&&phase==Phase.Rest&&Plant.Supported&&fault==0;
        private bool CanDisturb=>Stage==4&&Live&&phase==Phase.Hold&&Plant.RingAtTarget&&fault==0;
        private bool CanArm=>Stage==5&&Live&&phase==Phase.Trial&&Plant.LevitationQualified&&Plant.Rpm is >.90f and <1.42f&&!arm&&!FeedbackObserved;
        private bool CanCheck=>(FeedbackLatched||holdLatch||OverspeedLatched)&&Ready&&routed[W137]&&Plant.Supported&&
            Plant.Bus<.025f&&Plant.BearingBus<.025f&&Plant.AmplifierTemperature<60&&Plant.TouchdownHeat<62;
        private bool FaultEvidence=>fault switch
        {
            1=>Mathf.Abs(primaryGap-secondaryGap)>.73f,
            2=>Plant.AmplifierTemperature>69&&Plant.CurrentLimit<4.7f,
            3=>Plant.MassShift>.70f&&Plant.Vibration>.34f,
            _=>false
        };
        private void Start()
        {
            manager=FindFirstObjectByType<LevelGamePlayManager>();
            loader=FindFirstObjectByType<SparkLevelLayoutLoader>();
            wireTool=FindFirstObjectByType<SignalWireBuilder>();
            if(!manager||!loader||!arena||pins.Length!=SparkLevel87Wiring.Pins.Length)
            {Debug.LogError("Level 87 arena bindings are incomplete.",this);enabled=false;return;}
            for(int i=0;i<pins.Length;i++)lookup.Add(pins[i],i);
            source.SetOutput(false,default);
            registry=FindFirstObjectByType<SparkWireRegistry>();
            if(registry)
            {
                registry.WiresChanged+=UpdateCableView;
                registry.SelectionChanged+=SelectCable;
            }
            view=gameObject.AddComponent<SparkLevel87ArenaView>();
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
                if(OnlyFeedbackMissing&&arm&&Plant.Rpm>.68f&&Plant.Touchdown<.06f)
                {feedbackAtEdge=true;SpeedAtLoss=Plant.Rpm;}
            }
            var solve=solver.LatestSolveResult;
            bool Read(int cable,out float voltage)
            {voltage=0;return solve!=null&&solve.TryGetVoltage(pins[Wires[cable].b],ground,out voltage);}
            bool High(int cable)=>Read(cable,out float volts)&&volts is >=4.6f and <=5.3f;
            bool present=power&&source.IsOutputActive&&!source.IsOverloaded;
            foreach(int branch in SparkLevel87Wiring.Power)present&=routed[branch]&&High(branch);
            if(present)lastRail=Plant.Clock;
            if(High(W123))lastEmergency=Plant.Clock;
            if(High(W124))lastPermit=Plant.Clock;
            rails=power&&source.IsOutputActive&&!source.IsOverloaded&&Plant.Clock-lastRail<.34f;
            emergencyOk=emergency.IsClosed&&routed[W123]&&Plant.Clock-lastEmergency<.34f;
            permit=contacts[0].IsClosed&&routed[W124]&&Plant.Clock-lastPermit<.34f;
            bool validPrimary=Read(W137,out float primaryVoltage)&&primaryVoltage is >1.15f and <3.9f;
            bool validSecondary=Read(W135,out float secondaryVoltage)&&secondaryVoltage is >1.15f and <3.9f;
            primaryGap=validPrimary?(primaryVoltage-2.5f)/.57f+5:0;
            secondaryGap=validSecondary?(secondaryVoltage-2.5f)/.57f+5:0;
            accumulator+=Mathf.Min(.14f,Time.deltaTime)*3.45f;
            while(accumulator>=SparkLevel87Plant.Step)
            {
                accumulator-=SparkLevel87Plant.Step;
                Tick(validPrimary,validSecondary);
            }
            Present();
        }

        private void Drive(SparkPowerSupply transmitter,bool active,float voltage,ref float previous)
        {
            if(!active)
            {
                if(transmitter.IsOutputActive)transmitter.SetOutput(false,default);
                previous=-1;return;
            }
            if(transmitter.IsOutputActive&&Mathf.Abs(previous-voltage)<.020f)return;
            transmitter.SetOutput(false,default);
            if(!transmitter.TryConfigure(voltage,.01f,out string reason)){Event(reason);return;}
            transmitter.SetOutput(true,default);previous=voltage;
        }

        private void Tick(bool validPrimary,bool validSecondary)
        {
            const float dt=SparkLevel87Plant.Step;
            contactClock+=dt;
            if(safetyStep==1&&!permit&&emergencyOk&&contactClock>.21f)
            {contacts[0].SetState(SparkSwitchState.Closed);safetyStep=2;contactClock=0;}
            else if(safetyStep==2&&permit&&emergencyOk&&contactClock>.21f)
            {emergency.SetState(SparkSwitchState.Open);safetyStep=3;contactClock=0;}
            else if(safetyStep==3&&!emergencyOk&&permit&&contactClock>.21f)
            {emergency.SetState(SparkSwitchState.Closed);safetyStep=4;contactClock=0;}
            else if(safetyStep==4&&permit&&emergencyOk&&contactClock>.21f)
            {safety=true;safetyStep=0;Event("Independent isolation relay and emergency-stop path tested.");}

            signalRefresh+=dt;
            if(signalRefresh>=.16f)
            {
                signalRefresh=0;
                float primaryCarrier=Plant.Calibration>.98f&&Plant.SampleAge<.35f?2.5f+.57f*(Plant.Primary-5):.52f;
                Drive(transducer,rails&&routed[W137], primaryCarrier, ref signalLevel);
                Drive(secondary,rails&&routed[W135],2.5f+.57f*(Plant.Secondary-5),ref secondaryLevel);
            }
            bool agreement=validPrimary&&validSecondary&&Mathf.Abs(primaryGap-secondaryGap)<.38f;
            Trusted=routed[W137]&&rails&&Plant.Calibration>.98f&&Plant.SampleAge<.30f&&agreement;
            if(!routed[W137]&&Stage>=2&&phase!=Phase.Rest&&!FeedbackLatched)
            {
                FeedbackLatched=true;FeedbackObserved|=arm&&feedbackAtEdge;arm=false;
                sampleAtTrip=Plant.SampleSequence;calibrationAtTrip=Plant.CalibrationRevision;tripRun=RunId;
                checking=checkedSafe=false;StartStop("W137 open. Rim drives isolated and touchdown support engaging.");
            }
            if(Stage>=2&&(!emergencyOk||!permit)&&!holdLatch&&safetyStep==0)
            {
                holdLatch=true;sampleAtTrip=Plant.SampleSequence;calibrationAtTrip=Plant.CalibrationRevision;
                checking=checkedSafe=false;StartStop("Independent safety relay withdrew motion permission.");
            }
            if(Plant.Rpm>3.25f&&!OverspeedLatched)
            {
                OverspeedLatched=true;sampleAtTrip=Plant.SampleSequence;calibrationAtTrip=Plant.CalibrationRevision;
                checking=checkedSafe=false;StartStop("Independent speed monitor tripped. Bounded braking active.");
            }

            // The trusted signal is required for normal bearing control. During the
            // diagnostic drift, bounded degraded operation exposes the actual bias.
            bool driftTest=fault==1&&routed[W137]&&validPrimary&&validSecondary;
            bool motion=(Live||driftTest&&Ready&&!FeedbackLatched&&!holdLatch)&&
                phase!=Phase.Rest&&phase!=Phase.Stopping;
            bool motor=motion&&phase>=Phase.Precharging;
            float rpm=phase is Phase.SpinUp or Phase.Hold?3f:phase==Phase.Scanning?1.42f:phase>=Phase.Trial?1.18f:0;
            if(driftTest&&!Trusted)rpm=Mathf.Min(rpm,1.18f);
            Plant.Tick(rails,cal,motion,motor,rotary&&motion,habitat&&motion,
                phase==Phase.Stopping||FeedbackLatched||holdLatch||OverspeedLatched,rpm,balance||servicing&&fault==3,fault,servicing);
            if(phase==Phase.Stopping&&Plant.Supported){phase=Phase.Rest;Event("Rotor mechanically supported. Drives isolated.");}
            if(servicing&&RepairMeasured())
            {
                repaired[fault]=true;lastRepairRun=RunId;fault=0;
                diagnosed=servicing=false;Event("Measured repair complete. Run a fresh gravity cycle.");
            }
            if(checking)
            {
                goodGapTime=CanCheck&&Trusted&&Plant.CalibrationRevision>calibrationAtTrip&&Plant.SampleSequence>sampleAtTrip+4?goodGapTime+dt:0;
                if(goodGapTime>.40f)
                {checking=false;checkedSafe=true;Event("Fresh primary and diagnostic gaps agree. Safety reset enabled.");}
            }
            if(Stage==0&&Ready&&Trusted)Award();
            if(Live&&fault==0)Advance(dt);
        }

        private bool RepairMeasured()=>fault switch
        {
            1=>Plant.Drift<.06f&&Trusted&&Plant.Calibration>=1,
            2=>Plant.AmplifierTemperature<48&&Plant.CurrentLimit>5.8f,
            3=>Mathf.Abs(Plant.WeightA-(.46f+Plant.MassShift*.36f))<.035f&&
                Mathf.Abs(Plant.WeightB-(.47f-Plant.MassShift*.31f))<.035f&&Plant.Vibration<.18f,
            _=>false
        };

        private void Advance(float dt)
        {
            if(phase==Phase.Lifting&&Plant.LevitationQualified)
            {
                phase=Phase.Levitation;Event("Eight coils hold a stable gap. Touchdown race unloaded.");
                if(Stage==1)Award();
            }
            if(phase==Phase.Scanning)
            {
                scanTime=Plant.LevitationQualified&&Plant.Rpm is >1.33f and <1.5f?scanTime+dt:0;
                if(scanTime>4.0f&&Plant.ReadSpectrum())
                {phase=Phase.Scanned;Event($"Low-speed spectrum measured at {Plant.PeakFrequency:0.00} Hz.");}
            }
            if(phase==Phase.SpinUp&&Plant.RingAtTarget&&Plant.LevitationQualified&&Mathf.Abs(Plant.StationMomentum)<9000)
            {
                phase=Phase.Hold;Event("Measured 3 RPM and station momentum stable.");
                if(Stage==2)Award();
            }
            if(phase!=Phase.Hold||certifiedRun||Plant.RestoredThermalTime<4.2f||Plant.MissionEnergy<16000)return;
            certifiedRun=true;
            if(Stage==3||Stage==4&&AllRepairs&&RunId>lastRepairRun||Stage==5&&feedbackReset&&RunId>tripRun)
                Award();
        }

        private void StartStop(string reason)
        {
            phase=Phase.Stopping;habitat=rotary=balance=false;
            Plant.BeginMission();Event(reason);
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
        public void Calibrate()
        {
            if(Ready&&phase==Phase.Rest&&Plant.Supported&&Stage==0&&!cal)
            {cal=true;Plant.StartCalibration();Event("Calibrating primary, diagnostic and axial gap channels against touchdown reference.");}
        }
        public void Levitate()
        {
            if(!CanLift)return;
            RunId++;certifiedRun=lowTestPassed=habitat=rotary=balance=false;
            phase=Phase.Lifting;Plant.BeginMission();Event($"Run {RunId} · bearing bus precharge and lift.");
        }
        public void Precharge(){if(Live&&phase==Phase.Levitation){phase=Phase.Precharging;Event("Precharging both rim inverters.");}}
        public void LowSpeed(){if(Live&&phase==Phase.Precharging&&Plant.Bus>.95f){phase=Phase.Trial;Event("Low-speed rotor trial at 1.18 RPM.");}}
        public void Scan()
        {
            if(Live&&phase==Phase.Trial&&!arm&&Plant.Rpm is >1.06f and <1.30f)
            {scanTime=0;phase=Phase.Scanning;Event("Sweeping low speeds and collecting a vibration sample window.");}
        }
        public void Balance()
        {
            if(Live&&phase==Phase.Scanned)
            {balance=lowTestPassed=true;Event("Moving both trim weights to their measured balance positions.");}
        }
        private bool Trimmed=>Mathf.Abs(Plant.WeightA-(.46f+Plant.MassShift*.36f))<.035f&&
            Mathf.Abs(Plant.WeightB-(.47f-Plant.MassShift*.31f))<.035f;
        public void Spin()
        {
            if(Live&&phase==Phase.Scanned&&lowTestPassed&&Trimmed&&Plant.LevitationQualified)
            {phase=Phase.SpinUp;Event("Controlled acceleration through to 3 RPM.");}
        }
        public void Loads()
        {
            if(!Live||phase!=Phase.Hold||habitat)return;
            habitat=rotary=true;Plant.BeginMission();Event("Rotary power, telemetry and changing essential habitat loads enabled.");
        }
        public void Rundown(){if(phase!=Phase.Rest&&phase!=Phase.Stopping&&!arm)StartStop("Controlled stop. Rotor speed and touchdown remain measured.");}
        private void Disturb(int test)
        {
            if(!CanDisturb||repaired[test])return;
            fault=test;
            Event(test==1?"Primary radial bias applied. Compare independent samples.":test==2?
                "Amplifier cooling fault applied. Observe heat and coil current limiting.":"Cargo shifted. Measure the synchronous vibration.");
        }
        public void DriftFault()=>Disturb(1);
        public void HeatFault()=>Disturb(2);
        public void ImbalanceFault()=>Disturb(3);
        public void Diagnose()
        {
            if(Ready&&FaultEvidence&&!diagnosed)
            {diagnosed=true;StartStop("Fault measured. Wait for safe support before repairing.");}
        }
        public void Repair()
        {
            if(!Ready||!diagnosed||servicing||!Plant.Supported)return;
            servicing=true;
            if(fault==1){cal=true;Plant.StartCalibration();}
            Event("Repair in progress. Recovery requires fresh measured evidence.");
        }
        public void Arm(){if(CanArm){arm=true;feedbackAtEdge=false;Event("W137 test armed. Remove the amber P019 to P135 cable.");}}
        public void CheckSafe()
        {
            if(!CanCheck||checkedSafe||checking)return;
            checking=cal=true;goodGapTime=0;Plant.StartCalibration();
            Event("Recalibrating against the supported rotor and comparing new independent samples.");
        }
        public void ResetSafety()
        {
            if(!CanCheck||!checkedSafe||!Trusted)return;
            feedbackReset|=FeedbackLatched&&FeedbackObserved;
            FeedbackLatched=holdLatch=OverspeedLatched=checkedSafe=false;
            Event("Safety reset. A fresh full levitation, spin-up and load cycle is required.");
        }
        public void Stop()=>emergency.SetState(emergency.IsClosed?SparkSwitchState.Open:SparkSwitchState.Closed);
        public void ArenaView()=>view?.UseOverview();
        public void RotorView()=>view?.UseRotorView();
        public void SocketView()=>view?.UseSocketView();
        public void ShowcaseView()=>view?.UseShowcaseView();
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
            var (sourcePin,targetPin)=Wires[W137];
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
            if(FeedbackLatched||holdLatch||OverspeedLatched)
                return checkedSafe?"Confirm RESET SAFETY to start a new run.":checking?
                    "Comparing post-repair gap samples.":"Restore W137 and the interlocks. Wait for touchdown, then RECAL / CHECK.";
            if(wireFault>=0)
            {
                var (a,b)=Wires[wireFault];
                return $"W{wireFault+1:000}  ·  {SparkLevel87Wiring.Labels[a]} → {SparkLevel87Wiring.Labels[b]}";
            }
            if(wireFault==-2)return "Check for a misplaced or extra connection.";
            if(!power)return "Turn on the protected control supply.";
            if(!safety)return "Test the independent protection before energizing the ring.";
            if(Stage==0)return "Calibrate the gap sensors against the supported rotor.";
            if(Stage==6)return "GRAVITY ENGINE QUALIFIED";
            if(arm)return "W137 ARMED  ·  REMOVE THE AMBER CABLE IN WIRE CONTROLS";
            if(fault!=0)return servicing?"Waiting for measured recovery.":diagnosed?"When the rotor is supported select REPAIR.":"Observe gap, temperature and vibration. Select DIAGNOSE when evidence is available.";
            return phase switch
            {
                Phase.Rest=>"Request levitation for a new operating cycle.",
                Phase.Lifting=>"Precharging the bearings and unloading touchdown support.",
                Phase.Levitation=>"Precharge both rim inverters.",
                Phase.Precharging=>"Wait for full inverter precharge, then run the low-speed trial.",
                Phase.Trial=>CanArm?"Arm W137 at supervised low speed, or proceed with the resonance scan.":"Run the resonance scan after the trial stabilizes.",
                Phase.Scanning=>"Recording the low-speed vibration spectrum.",
                Phase.Scanned=>!lowTestPassed?"Balance the rotor with both trim masses.":"Spin up once the actual weights reach their setpoints.",
                Phase.SpinUp=>"Accelerating under measured clearance and momentum limits.",
                Phase.Hold when !habitat=>"Enable rotary power, telemetry and essential habitat loads.",
                Phase.Hold when !certifiedRun=>"Verify a sustained loaded gravity window.",
                Phase.Hold when Stage==4&&!AllRepairs=>"Use the next fault injector and verify a measured repair.",
                Phase.Hold=>"Command a controlled stop before the next full qualification.",
                _=>"Wait for the rotor to coast safely onto touchdown support."
            };
        }
        private void Present()
        {
            int hovered=wireTool&&wireTool.HoveredTerminal&&lookup.TryGetValue(wireTool.HoveredTerminal,out int pin)?pin:-1;
            if(pinLabels!=null&&(shownWire!=wireFault||shownPin!=hovered))
            {
                shownWire=wireFault;shownPin=hovered;
                var (a,b)=wireFault>=0?Wires[wireFault]:(-1,-1);
                for(int i=0;i<pinLabels.Length;i++)
                {
                    if(pinLabels[i])pinLabels[i].gameObject.SetActive(i==a||i==b||i==hovered);
                    if(componentLabels!=null&&componentLabels[i])componentLabels[i].gameObject.SetActive(false);
                }
                if(componentLabels!=null)
                    foreach(int index in new[]{a,b,hovered})
                        if(index>=0&&componentLabels[index])componentLabels[index].gameObject.SetActive(true);
            }
            statusText.text=$"{Stage}/6  ·  {Instruction()}";
            meterText.text=$"{Plant.Rpm:0.00} RPM / {Plant.Gravity:0.000} g (45 m)   ·   GAP A/B {primaryGap:0.00}/{secondaryGap:0.00} mm   ·   VIB {Plant.Vibration:0.00} mm   ·   W137 {(Trusted?"FRESH":"UNTRUSTED")}";
            detailText.text=$"8 COILS {Plant.CoilsMean:0.0} A   ·   AMP {Plant.AmplifierTemperature:0.0} °C   ·   LOAD {Plant.LoadPower/1000:0.0} kW   ·   DATA {Plant.OpticalQuality:P0}   ·   RUN {RunId}";
            if(componentNames!=null&&componentNames.Length==pins.Length)
            {
                if(hovered>=0)
                    detailText.text=$"{componentNames[hovered]}   |   {SparkLevel87Wiring.Labels[hovered]}   |   {pins[hovered].name.Split(new[]{'_'},3)[2].Replace('_',' ')}";
                else if(wireFault>=0)
                {
                    var (from,to)=Wires[wireFault];
                    detailText.text=$"{SparkLevel87Wiring.Labels[from]}  {componentNames[from]}   →   {SparkLevel87Wiring.Labels[to]}  {componentNames[to]}";
                }
            }
            eventText.text=latest;
            bool[] enabled={true,rails&&emergencyOk&&safetyStep==0&&!safety,Ready&&phase==Phase.Rest&&Plant.Supported&&Stage==0&&!cal,
                CanLift,Live&&phase==Phase.Levitation,Live&&phase==Phase.Precharging&&Plant.Bus>.95f,
                Live&&phase==Phase.Trial&&!arm&&Plant.Rpm is >1.06f and <1.30f,Live&&phase==Phase.Scanned&&!balance,
                Live&&phase==Phase.Scanned&&Trimmed&&lowTestPassed&&Plant.LevitationQualified,Live&&phase==Phase.Hold&&!habitat,
                phase!=Phase.Rest&&phase!=Phase.Stopping&&!arm,CanDisturb&&!repaired[1],CanDisturb&&!repaired[2],
                CanDisturb&&!repaired[3],Ready&&FaultEvidence&&!diagnosed,Ready&&diagnosed&&!servicing&&Plant.Supported,
                CanArm,CanCheck&&!checkedSafe&&!checking,CanCheck&&checkedSafe&&Trusted,true,true,true,true,true,true};
            for(int i=0;i<buttons.Length;i++)buttons[i].interactable=enabled[i];
            if(!rig)return;
            rig.SetActualRingDynamics(Plant.Angle,Plant.CounterAngle,Mathf.Clamp01((Plant.MinimumGap-3.1f)/1.6f),Plant.Vibration);
            rig.SetActualBearingMechanisms(Plant.WeightA,Plant.WeightB,Plant.Touchdown);
            rig.SetActualSafety(Plant.LevitationQualified,Plant.RingAtTarget,Trusted,FeedbackLatched||holdLatch||OverspeedLatched);
        }
    }
}



