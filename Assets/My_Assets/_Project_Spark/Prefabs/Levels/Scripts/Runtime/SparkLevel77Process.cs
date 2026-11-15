using System;
using System.Collections.Generic;
using ProjectSpark.Circuit;
using ProjectSpark.Visuals;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.Gameplay
{
    public sealed class SparkLevel77Process : MonoBehaviour
    {
        public static readonly string[] Gates={"L77_Wired","L77_Commissioned","L77_CopperClear","L77_Isolated","L77_Disturbances","L77_Recertified"};
        public static readonly (int a,int b)[] Wires={(0,1),(2,3),(0,4),(2,5),(0,6),(2,7),(0,8),(2,9),(0,10),(2,11),(12,13),(14,15),(16,17),(18,19),(20,21),(22,23),(24,25),(26,27),(28,29),(30,31),(32,33),(34,35),(36,37),(38,39),(40,41),(42,43),(44,45),(46,47),(48,49),(50,51),(52,53),(54,55),(56,57),(58,59),(60,61),(62,63),(64,65),(66,67),(68,69),(70,71),(72,73),(74,75),(76,77),(78,79),(80,81),(82,83),(84,85),(86,87),(88,89),(90,91),(92,93),(94,95),(96,97),(98,99),(100,101),(102,103),(104,105),(106,107),(108,109),(110,111),(112,113),(114,115),(116,117),(118,119),(120,121),(122,123),(124,125),(126,127),(119,128),(99,129),(93,130),(97,131),(119,132),(127,133),(125,134),(135,136),(97,137),(137,138),(138,139),(109,140),(74,141),(77,142),(78,143),(143,144),(81,145),(83,146),(140,147),(147,148),(147,149),(93,150),(149,151),(147,152),(107,153),(109,154),(93,155),(150,156),(149,157),(151,158),(153,159),(150,160),(161,162),(162,163),(162,164),(162,165),(20,166),(34,167),(115,168),(113,169),(110,170),(162,171),(170,172),(172,173),(173,174),(159,175)};
        private const int StopInput=162,ControlReturn=3,OpticalPower=8,OpticalReturn=9,Feedback=54,PermitWire=75;
        public SparkLevelLayout layout;
        public SparkPowerSupply source;
        public SparkSwitch[] contacts;
        public SparkSwitch emergency;
        public SparkTerminal[] pins;
        public SparkLevel77VisualRig rig;
        public SparkLevel77Trend plot;
        public TMP_Text controllerDisplay,qualityDisplay;
        public Text statusText,meterText,detailText,calibrationText;
        public Button powerButton,flowButton,drivesButton,padsButton,calCuButton,calBarrierButton,loadButton,inspectButton,
            bulkButton,transferBButton,softButton,transferCButton,barrierButton,stopButton,unloadButton,
            protectButton,brushButton,megaButton,rinseButton,dryButton,geometryButton,electricalButton,
            foulButton,driftButton,corrodeButton,restoreButton,eStopButton,resetButton,armButton,recalButton,verifyButton,certifyButton;

        private enum Fault { None, Power, Wiring, EStop, Fouling, Selectivity, Corrosion, Optical, Process }
        private enum Scenario { None, Fouling, Selectivity, Corrosion }
        private readonly SparkLevel77Plant plant=new();
        private readonly HashSet<(int,int)> wires=new();
        private readonly List<SparkCircuitConnection> edges=new();
        private LevelGamePlayManager manager;
        private SparkLevelLayoutLoader loader;
        private SparkLevelSceneBindings bindings;
        private int topology=-1,stage,station,cleaning,acceptedBatches;
        private bool power,rails,safety,opticalPower,wiredOptical,permitVoltage,lastPermit;
        private bool flows,drives,pads,calCu,calBarrier,loaded,inspected,polishing,bulkClear,copperClear,barrierClear;
        private bool validOptical,recalibrated,verified,unloaded,protect,brushed,mega,rinsed,dried,geometry,electrical,counted;
        private bool foulingPassed,selectivityPassed,corrosionPassed,armed,feedbackObserved,feedbackReset,certify;
        private float elapsed,accumulator,hold,age,calClock,transfer,endpointDwell,scan,arrival,elapsedAtTrip;
        private float sampledA,sampledB,forceAtTrip,rpmAtTrip,viewportFault,selectivityFault,corrosionFault;
        private string wiring,latestEvent="Connect the triple-stage CMP workstation and enable 5 V.";
        private Fault latch;
        private Scenario scenario;

        private bool Ready=>rails&&safety&&wiring==null;
        private bool AtRest=>!polishing&&plant.Force[station]<.025f&&plant.Rpm[station]<6&&plant.Lift[station]>.96f;
        private bool Idle=>AtRest&&transfer<=0&&cleaning==0;
        private bool Facilities=>flows&&drives&&pads&&calCu&&calBarrier&&plant.Flow[station]>.9f;
        private bool CanLoad=>Ready&&Idle&&!loaded&&latch==Fault.None&&!(counted&&(stage==3||stage==4&&foulingPassed&&selectivityPassed&&corrosionPassed));
        private bool CanStart=>stage>=2&&Ready&&Idle&&loaded&&inspected&&Facilities&&latch==Fault.None&&
            (station==0&&!bulkClear||station==1&&bulkClear&&!copperClear||station==2&&copperClear&&!barrierClear&&validOptical);
        private bool CanUnload=>Ready&&Idle&&loaded&&latch==Fault.None;
        private bool CanRecal=>Ready&&Idle&&latch is Fault.Optical or Fault.Fouling&&scenario==Scenario.None&&plant.Viewport>.91f&&!recalibrated&&calClock<=0;
        private bool CanVerify=>Ready&&Idle&&latch is Fault.Optical or Fault.Fouling&&validOptical&&!verified;
        private bool CanReset=>Ready&&Idle&&latch!=Fault.None&&scenario==Scenario.None&&
            (latch!=Fault.Optical&&latch!=Fault.Fouling||verified)&&(latch!=Fault.Corrosion||protect)&&plant.Selectivity>.92f;
        private bool CanCertify=>stage==5&&feedbackReset&&counted&&acceptedBatches>=3&&Ready&&latch==Fault.None;
        private bool CanArm=>stage==5&&station==2&&polishing&&plant.Force[2]>.52f&&!armed&&!feedbackObserved;

        private void Start()
        {
            manager=FindFirstObjectByType<LevelGamePlayManager>();loader=FindFirstObjectByType<SparkLevelLayoutLoader>();
            if(!manager||!loader||pins.Length!=176||contacts.Length!=1||!rig){Debug.LogError("[LEVEL 77] Missing copper finishing references.",this);enabled=false;return;}
            source.SetOutput(false,default);
        }
        private static (int,int) Pair(int a,int b)=>(Math.Min(a,b),Math.Max(a,b));
        private bool Routed(int i)=>wires.Contains(Pair(Wires[i].a,Wires[i].b));
        private void ReadTopology(SparkCircuitSystem circuit)
        {
            edges.Clear();wires.Clear();circuit.CopyConnections(edges);
            foreach(var edge in edges)
            {
                if(edge.Kind!=SparkConnectionKind.Wire)continue;
                int a=Array.IndexOf(pins,edge.A),b=Array.IndexOf(pins,edge.B);
                if(a>=0||b>=0)wires.Add(Pair(a,b));
            }
            wiredOptical=Routed(Feedback);
            wiring=wires.Count==Wires.Length?null:"Remove extra wires and check the route chart.";
            for(int i=0;i<Wires.Length;i++)if(!Routed(i))
            {
                wiring=$"Connect W{i+1:000} · {pins[Wires[i].a].name} → {pins[Wires[i].b].name}.";
                break;
            }
        }
        private void Update()
        {
            if(loader.ActiveLayout!=layout||loader.ActiveBindings==null)return;
            var circuit=manager.CircuitSystem;var solver=manager.ElectricalSolver;
            if(!circuit||!solver)return;
            if(bindings!=loader.ActiveBindings){bindings=loader.ActiveBindings;topology=-1;}
            if(topology!=circuit.TopologyVersion){topology=circuit.TopologyVersion;ReadTopology(circuit);}
            var result=solver.LatestSolveResult;
            bool Rail(int a,int b)=>result!=null&&result.TryGetVoltage(pins[a],pins[b],out float v)&&v is >=4.7f and <=5.3f;
            rails=power&&source.IsOutputActive&&!source.IsOverloaded;
            for(int i=0;i<10&&rails;i+=2)rails=Rail(Wires[i].b,Wires[i+1].b);
            safety=emergency.IsClosed&&Rail(StopInput,ControlReturn);
            opticalPower=power&&Rail(OpticalPower,OpticalReturn);
            permitVoltage=power&&Rail(Wires[PermitWire].b,ControlReturn);
            if(stage==0&&Ready)Award();
            if(latch==Fault.None&&(polishing||transfer>0||cleaning>0)&&!Ready)
            {
                bool opticalOnly=rails&&safety&&!wiredOptical&&wires.Count==Wires.Length-1;
                if(opticalOnly)for(int i=0;i<Wires.Length;i++)if(i!=Feedback&&!Routed(i)){opticalOnly=false;break;}
                feedbackObserved|=stage==5&&armed&&station==2&&opticalOnly;
                Trip(opticalOnly?Fault.Optical:!rails?Fault.Power:!safety?Fault.EStop:Fault.Wiring);
            }
            accumulator+=Mathf.Min(Time.deltaTime,.18f)*(armed?.32f:CanArm?1.1f:2.15f);
            while(accumulator>=SparkLevel77Plant.Step){accumulator-=SparkLevel77Plant.Step;Tick(SparkLevel77Plant.Step);}
            Present();
        }
        private void Tick(float dt)
        {
            elapsed+=dt;
            bool opticalLatch=latch is Fault.Optical or Fault.Fouling;
            bool sensor=opticalPower&&wiredOptical&&calBarrier&&plant.Viewport>.78f&&(!opticalLatch||recalibrated);
            age=sensor?age+dt:0;
            validOptical=age>.36f;
            sampledA=validOptical?plant.SpectralA:0;sampledB=validOptical?plant.SpectralB:0;
            bool permit=Ready&&polishing&&Facilities&&latch==Fault.None&&plant.Heat<42&&(station!=2||validOptical);
            if(lastPermit!=permit){lastPermit=permit;contacts[0].SetState(permit?SparkSwitchState.Closed:SparkSwitchState.Open);}
            bool engaged=permit&&permitVoltage&&contacts[0].IsClosed;
            plant.Tick(dt,station,engaged,engaged,rails&&safety&&flows,Ready&&pads&&!polishing,
                scenario==Scenario.Fouling,scenario==Scenario.Selectivity,scenario==Scenario.Corrosion,protect,cleaning,elapsed);
            if(transfer>0)
            {
                transfer+=dt;
                if(transfer>1.45f){transfer=0;station=(int)arrival;loaded=true;Log($"Wafer positioned at Station {(char)('A'+station)}.");}
            }
            if(calClock>0)
            {
                calClock=Mathf.Max(0,calClock-dt);
                if(calClock==0){calBarrier=true;age=0;recalibrated=opticalLatch;Log("Barrier spectrometer calibrated against both reference bands.");}
            }
            scan=Mathf.Max(0,scan-dt);
            if(cleaning==1&&plant.Particles<=.24f){cleaning=0;brushed=true;Log("Brush cleaning complete.");}
            if(cleaning==2&&plant.Particles<.025f){cleaning=0;mega=true;Log("Megasonic particle removal complete.");}
            if(cleaning==3&&plant.Residue<.025f){cleaning=0;rinsed=true;Log("Protected rinse complete.");}
            if(cleaning==4&&plant.Moisture<.035f){cleaning=0;dried=true;Log("Wafer dry. Run geometry and electrical tests.");}

            if(latch==Fault.None)
            {
                if(scenario==Scenario.Fouling&&plant.Viewport<.77f){viewportFault=plant.Viewport;foulingPassed=true;Trip(Fault.Fouling);}
                else if(scenario==Scenario.Selectivity&&plant.Selectivity<.64f){selectivityFault=plant.Selectivity;selectivityPassed=true;Trip(Fault.Selectivity);}
                else if(scenario==Scenario.Corrosion&&plant.Corrosion>.15f){corrosionFault=plant.Corrosion;corrosionPassed=true;Trip(Fault.Corrosion);}
                else if(polishing&&plant.Heat>42)Trip(Fault.Process);
                else if(engaged)CheckEndpoint(dt);
            }
            if(geometry&&electrical&&!counted){counted=true;acceptedBatches++;}
            bool goal=stage switch
            {
                1=>Facilities&&validOptical&&loaded&&inspected&&plant.WorstCopper>115&&plant.Wear[0]<.07f,
                2=>bulkClear&&copperClear&&AtRest&&plant.WorstDishing<5&&plant.TotalRemoved>105,
                3=>counted&&acceptedBatches>=1,
                4=>foulingPassed&&selectivityPassed&&corrosionPassed&&counted&&acceptedBatches>=2,
                5=>CanCertify&&certify,
                _=>false
            };
            hold=Ready&&latch==Fault.None&&goal?hold+dt:0;
            if(hold>.40f)Award();
            if(plot!=null)plot.Show(plant,validOptical,station);
        }
        private void CheckEndpoint(float dt)
        {
            bool endpoint=station switch
            {
                0=>plant.Eddy<20&&plant.OpticalCopper<.395f,
                1=>plant.Eddy<.65f&&plant.OpticalCopper<.028f,
                _=>validOptical&&plant.Confidence>.85f&&sampledB>.65f&&sampledA<.42f
            };
            endpointDwell=endpoint?endpointDwell+dt:0;
            if(endpointDwell<.16f)return;
            if(station==0)bulkClear=true;
            else if(station==1)copperClear=true;
            else if(!armed)barrierClear=true;
            else
            {
                StopMotion();
                Log("Endpoint reached before the fault was injected. Reload a test wafer and repeat W055.");
                return;
            }
            StopMotion();
            Log($"Station {(char)('A'+station)} endpoint verified. Carrier retracting.");
        }
        private void Award()
        {
            if(stage>=Gates.Length||!bindings.SetRuntimeComponentBinding(Gates[stage],source))return;
            stage++;hold=0;manager.QueueEvaluation();Log($"Objective {stage}/6 verified.");
        }
        private void Log(string text)=>latestEvent=$"{elapsed:0.0}s · {text}";
        private void StopMotion()
        {
            polishing=armed=false;lastPermit=false;endpointDwell=0;contacts[0].SetState(SparkSwitchState.Open);
        }
        private void Trip(Fault cause)
        {
            latch=cause;forceAtTrip=plant.Force[station];rpmAtTrip=plant.Rpm[station];elapsedAtTrip=elapsed;
            StopMotion();transfer=0;cleaning=0;calClock=0;
            if(cause is Fault.Optical or Fault.Fouling)
            {
                validOptical=recalibrated=verified=false;age=sampledA=sampledB=0;
                Log(cause==Fault.Optical?"BARRIER OPTICAL FEEDBACK INVALID · carrier retracting.":"VIEWPORT FOULED · optical confidence lost.");
            }
            else Log($"CMP SAFETY HOLD · {cause}.");
        }

        public void Power(){power=!power;source.SetOutput(power,default);}
        public void Flow(){if(Ready)flows=true;}
        public void Drives(){if(Ready)drives=true;}
        public void Pads(){if(Ready&&Idle){pads=true;Log("Three pads inspected and conditioning available.");}}
        public void CalCu(){if(Ready&&Idle){calCu=true;Log("Eddy-current and bulk optical channels calibrated.");}}
        public void CalBarrier(){if(Ready&&Idle&&plant.Viewport>.92f&&!calBarrier&&calClock<=0)calClock=.64f;}
        public void Load()
        {
            if(!CanLoad)return;
            plant.NewWafer();arrival=0;transfer=.001f;station=0;
            inspected=bulkClear=copperClear=barrierClear=unloaded=brushed=mega=rinsed=dried=geometry=electrical=counted=false;
            Log("Loading copper-plated damascene wafer with blanket overburden.");
        }
        public void Inspect(){if(Ready&&loaded&&Idle&&plant.WorstCopper>115){inspected=true;Log("Incoming copper overburden and patterned vias verified.");}}
        private void StartStation(int index){if(CanStart&&station==index){polishing=true;endpointDwell=0;Log($"Station {(char)('A'+station)} polishing.");}}
        public void Bulk()=>StartStation(0);
        public void Soft()=>StartStation(1);
        public void Barrier()=>StartStation(2);
        private void MoveTo(int target){arrival=target;transfer=.001f;Log("Gantry transferring wafer with all carriers retracted.");}
        public void TransferB(){if(Ready&&Idle&&loaded&&station==0&&bulkClear&&latch==Fault.None)MoveTo(1);}
        public void TransferC(){if(Ready&&Idle&&loaded&&station==1&&stage>=3&&copperClear&&latch==Fault.None)MoveTo(2);}
        public void Stop(){StopMotion();Log("Polishing stop and retract requested.");}
        public void Unload(){if(CanUnload){loaded=false;unloaded=barrierClear;Log(unloaded?"Wafer moved to post-CMP cleaning.":"Interrupted wafer discarded.");}}
        public void Protect(){if(Ready&&Idle){protect=true;Log("Fictional corrosion protection enabled.");}}
        public void Brush(){if(Ready&&Idle&&unloaded&&!brushed&&protect&&latch==Fault.None)cleaning=1;}
        public void Mega(){if(Ready&&Idle&&brushed&&!mega&&latch==Fault.None)cleaning=2;}
        public void Rinse(){if(Ready&&Idle&&mega&&!rinsed&&latch==Fault.None)cleaning=3;}
        public void Dry(){if(Ready&&Idle&&rinsed&&!dried&&latch==Fault.None)cleaning=4;}
        public void Geometry()
        {
            if(!CanGeometry)return;
            scan=1.25f;
            geometry=plant.WorstDishing<=5&&plant.WorstErosion<=4&&plant.WorstCopper<.8f&&plant.WorstBarrier<.75f&&
                plant.Uniformity<=.025f&&plant.Corrosion<.08f&&plant.Particles<.03f&&plant.Residue<.03f;
            Log(geometry?"Geometry, residue and corrosion inspection passed.":"Wafer outside geometry or surface limits.");
        }
        public void Electrical()
        {
            if(!CanElectrical)return;
            scan=1.25f;
            electrical=plant.ViaResistance<=1.06f&&plant.Leakage<.12f&&plant.DielectricIntegrity>.90f;
            Log(electrical?"Via Kelvin, interline isolation and dielectric tests passed.":"Electrical wafer check failed.");
        }
        public void Foul(){if(CanFoul){scenario=Scenario.Fouling;Log("Barrier viewport contamination applied.");}}
        public void Drift(){if(CanDrift){scenario=Scenario.Selectivity;Log("Barrier-fluid selectivity disturbed.");}}
        public void Corrode(){if(CanCorrode){protect=false;scenario=Scenario.Corrosion;Log("Post-CMP protection interrupted.");}}
        public void Restore(){if(Ready&&latch!=Fault.None&&scenario!=Scenario.None){scenario=Scenario.None;protect=true;Log("Fault corrected. Review process conditions before reset.");}}
        public void EStop(){emergency.SetState(emergency.IsClosed?SparkSwitchState.Open:SparkSwitchState.Closed);if(emergency.IsOpen&&latch==Fault.None)Trip(Fault.EStop);}
        public void Reset(){if(CanReset){feedbackReset|=latch==Fault.Optical&&feedbackObserved;latch=Fault.None;Log("Safety reset. Recommission a fresh wafer.");}}
        public void Arm(){if(CanArm){armed=true;Log("Remove W055 with Undo Last Wire during Stage C.");}}
        public void Recal(){if(CanRecal){calClock=.68f;Log("Collecting fresh dual-wavelength calibration samples.");}}
        public void Verify(){if(CanVerify){verified=true;Log("Viewport and fresh spectral confidence passed.");}}
        public void Certify(){if(CanCertify)certify=true;}

        private bool CanFoul=>stage==4&&station==2&&polishing&&plant.Force[2]>.50f&&!foulingPassed&&scenario==Scenario.None;
        private bool CanDrift=>stage==4&&station==2&&polishing&&plant.Force[2]>.50f&&foulingPassed&&!selectivityPassed&&scenario==Scenario.None;
        private bool CanCorrode=>stage==4&&Ready&&Idle&&unloaded&&!brushed&&foulingPassed&&selectivityPassed&&!corrosionPassed&&scenario==Scenario.None&&latch==Fault.None;
        private bool CanGeometry=>Ready&&Idle&&dried&&!geometry&&latch==Fault.None;
        private bool CanElectrical=>Ready&&Idle&&geometry&&!electrical&&latch==Fault.None;
        private string Status()
        {
            if(stage==6)return "CERTIFIED · fresh interconnect wafer passed geometry and isolation.";
            if(!power)return "Enable 5 V CONTROL and complete wiring.";
            if(latch==Fault.Optical||latch==Fault.Fouling)return !Ready?"BARRIER OPTICAL FEEDBACK INVALID · reconnect W055.":scenario!=Scenario.None?"RESTORE the fouled viewport.":CanRecal?"Select RECAL OPTICS.":!verified?"Wait for fresh spectra and select VERIFY OPTICS.":"Select RESET SAFETY and prepare a fresh wafer.";
            if(latch!=Fault.None)return $"CMP SAFETY HOLD · {latch} · restore and reset.";
            if(!Ready)return wiring??"Check control rails and the E-Stop loop.";
            if(armed)return "Slow-motion test · select Undo Last Wire to disconnect W055.";
            if(transfer>0)return "Gantry moving with carriers retracted.";
            if(CanCertify)return "Fresh wafer passed. Select CERTIFY.";
            if(CanCorrode)return "Apply CORROSION before cleaning.";
            if(CanFoul)return "Select VIEWPORT FOUL to test degraded optical feedback.";
            if(CanDrift)return "Select SELECTIVITY DRIFT to test loss of barrier selectivity.";
            if(CanArm)return "Select ARM W055 TEST while Station C is polishing.";
            if(polishing)return $"STATION {(char)('A'+station)} · {(station==0?"BULK COPPER":station==1?"SOFT LANDING":"BARRIER CLEAR")} · endpoint monitored.";
            if(counted)return "Wafer accepted. Load the next wafer.";
            if(barrierClear)return !unloaded?"UNLOAD after carrier retracts.":!brushed?"Enable PROTECT, then BRUSH.":!mega?"MEGASONIC clean.":!rinsed?"RINSE the wafer.":!dried?"SPIN DRY.":!geometry?"Run GEOMETRY.":"Run ELECTRICAL.";
            if(station==0&&bulkClear)return "TRANSFER B once the carrier retracts.";
            if(station==1&&copperClear)return "TRANSFER C once the carrier retracts.";
            if(stage==1)return "Commission fluids, drives and pads. Calibrate optics, load wafer and inspect.";
            return "Start the current station when all conditions are valid.";
        }
        private void Present()
        {
            statusText.text=Status();
            meterText.text=$"Cu {plant.MeanCopper:0.0} nm   ·   BARRIER {plant.MeanBarrier:0.0} nm   ·   EDDY {plant.Eddy:0.0} nm   ·   OPTICS {(validOptical?plant.Confidence.ToString("P0"):"STALE")}";
            detailText.text=$"DISH {plant.WorstDishing:0.00} nm   ·   EROSION {plant.WorstErosion:0.00} nm   ·   NONUNIF {plant.Uniformity:P2}   ·   LEAK {plant.Leakage:0.000}";
            calibrationText.text=$"{latestEvent}   |   RPM {plant.Rpm[station]:0}   ·   FORCE {plant.Force[station]:0.00}   ·   FLUID {plant.Reservoir[station]:P0}";
            if(controllerDisplay)controllerDisplay.text=validOptical?$"{plant.Confidence:P0} OPT · {plant.Viewport:P0} VIEW":"OPTICS STALE";
            if(qualityDisplay)qualityDisplay.text=$"{plant.MeanCopper:0.0} Cu · {plant.MeanBarrier:0.0} B";
            powerButton.interactable=stage<6;flowButton.interactable=Ready&&!flows;drivesButton.interactable=Ready&&!drives;padsButton.interactable=Ready&&Idle&&!pads;
            calCuButton.interactable=Ready&&Idle&&!calCu;calBarrierButton.interactable=Ready&&Idle&&!calBarrier&&plant.Viewport>.92f&&calClock<=0;
            loadButton.interactable=CanLoad;inspectButton.interactable=Ready&&loaded&&Idle&&!inspected&&plant.WorstCopper>115;
            bulkButton.interactable=CanStart&&station==0;softButton.interactable=CanStart&&station==1;barrierButton.interactable=CanStart&&station==2;
            transferBButton.interactable=Ready&&Idle&&loaded&&station==0&&bulkClear&&latch==Fault.None;
            transferCButton.interactable=Ready&&Idle&&loaded&&station==1&&stage>=3&&copperClear&&latch==Fault.None;
            stopButton.interactable=polishing;unloadButton.interactable=CanUnload;protectButton.interactable=Ready&&Idle&&!protect;
            brushButton.interactable=Ready&&Idle&&unloaded&&!brushed&&protect&&latch==Fault.None;
            megaButton.interactable=Ready&&Idle&&brushed&&!mega&&latch==Fault.None;rinseButton.interactable=Ready&&Idle&&mega&&!rinsed&&latch==Fault.None;
            dryButton.interactable=Ready&&Idle&&rinsed&&!dried&&latch==Fault.None;geometryButton.interactable=CanGeometry;electricalButton.interactable=CanElectrical;
            foulButton.interactable=CanFoul;driftButton.interactable=CanDrift;corrodeButton.interactable=CanCorrode;
            restoreButton.interactable=Ready&&latch!=Fault.None&&scenario!=Scenario.None;eStopButton.interactable=stage<6;
            resetButton.interactable=CanReset;armButton.interactable=CanArm;recalButton.interactable=CanRecal;verifyButton.interactable=CanVerify;certifyButton.interactable=CanCertify&&!certify;
            rig.Apply(plant,station,(int)arrival,transfer,polishing,validOptical,latch!=Fault.None,cleaning,scan,emergency.IsOpen);
        }
    }
}




