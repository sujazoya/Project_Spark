using System;
using System.Collections.Generic;
using ProjectSpark.Circuit;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.Gameplay
{
    public sealed class SparkLevel73Process : MonoBehaviour
    {
        public static readonly string[] Gates={"L73_Wired","L73_Commissioned","L73_Pressure","L73_Planarized","L73_Disturbances","L73_Recertified"};
        public static readonly (int a,int b)[] Wires={(0,1),(2,3),(0,4),(2,5),(0,6),(2,7),(0,8),(2,9),(0,10),(2,11),(12,13),(14,15),(16,17),(18,19),(20,21),(22,23),(24,25),(26,27),(28,29),(30,31),(32,33),(34,35),(36,37),(38,39),(40,41),(42,43),(44,45),(46,47),(48,49),(50,51),(52,53),(54,55),(56,57),(58,59),(60,61),(62,63),(64,65),(66,67),(68,69),(70,71),(72,73),(74,75),(76,77),(78,79),(80,81),(82,83),(84,85),(86,87),(88,89),(90,91),(92,93),(94,95),(96,97),(98,99),(100,101),(102,103),(104,105),(106,107),(108,109),(110,111),(105,112),(105,113),(109,114),(111,115),(29,116),(83,117),(67,118),(101,119),(39,120),(37,121),(87,122),(51,123),(33,124),(31,125),(63,126),(71,127),(67,128),(103,129),(61,130),(65,131),(132,133),(133,134),(45,135),(47,136),(48,137),(67,138),(138,139),(139,140),(71,141),(73,142),(45,143),(83,144),(63,145),(29,146),(127,147),(134,148),(41,149),(150,151)};
        private const int StopInput=133, ControlReturn=3, ForcePower=10, ForceReturn=11, Feedback=97, PermitWire=26;
        public SparkLevelLayout layout;
        public SparkPowerSupply source;
        public SparkSwitch[] contacts;
        public SparkSwitch emergency;
        public SparkTerminal[] pins;
        public SparkLevel73VisualRig rig;
        public SparkLevel73Trend plot;
        public TMP_Text controllerDisplay,qualityDisplay;
        public Text statusText,meterText,detailText,calibrationText;
        public Button powerButton,calSpeedButton,calForceButton,calSlurryButton,calTorqueButton,calEndpointButton,inspectButton,conditionButton,
            platenButton,carrierButton,slurryButton,coolButton,loadButton,clampButton,pidButton,engageButton,stopButton,unloadButton,rinseButton,
            brushButton,dryButton,metrologyButton,glazeButton,restrictButton,driftButton,restoreButton,eStopButton,resetButton,armButton,recalButton,
            verifyButton,certifyButton,speedButton;

        private enum Fault { None, Power, Wiring, Emergency, Glaze, Slurry, Edge, Force, Overload }
        private enum Scenario { None, Glaze, Slurry, Edge }
        private readonly SparkLevel73Plant plant=new();
        private readonly HashSet<(int,int)> wires=new();
        private readonly List<SparkCircuitConnection> edges=new();
        private LevelGamePlayManager manager;
        private SparkLevelLayoutLoader loader;
        private SparkLevelSceneBindings bindings;
        private int topology=-1,stage,wash,acceptedBatches;
        private bool power,rails,safety,forcePower,wiredForce,permitVoltage,lastPermit;
        private bool calSpeed,calForce,calSlurry,calTorque,calEndpoint,inspected,conditioned,platen,carrier,slurry,cool,conditioning,pid;
        private bool forceValid,recalibrated,verified,loaded,clamped,engaged,endpoint,unloaded,rinsed,brushed,dried,measured;
        private bool glazePassed,restrictionPassed,driftPassed,armed,feedbackObserved,feedbackReset,certify;
        private float elapsed,accumulator,hold,forceAge,measuredForce,processTime,endpointTime,loadClock,washClock,scanClock,testClock;
        private float platenRequest=64f,referenceRate,glazedRate,restrictedFlow,edgeImbalance,tripForce;
        private string wiring,latestEvent="Connect the CMP controls and enable 5 V.";
        private Fault latch;
        private Scenario scenario;

        private bool Ready=>rails&&safety&&wiring==null;
        private bool AtRest=>plant.Position<.035f&&plant.Downforce<.07f;
        private bool Running=>platen&&carrier&&plant.PlatenRpm>platenRequest*.84f&&plant.CarrierRpm>40f;
        private bool Calibrated=>calSpeed&&calForce&&calSlurry&&calTorque&&calEndpoint;
        private bool CanLoad=>Ready&&AtRest&&!loaded&&!engaged&&loadClock<=0&&wash==0&&latch==Fault.None;
        private bool CanUnload=>Ready&&loaded&&AtRest&&!engaged&&loadClock<=0&&latch==Fault.None;
        private bool CanEngage=>stage>=2&&Ready&&latch==Fault.None&&Calibrated&&loaded&&clamped&&pid&&forceValid&&Running&&
            slurry&&cool&&plant.Feed>.82f&&plant.Pad>.78f&&!conditioning&&!engaged&&!endpoint;
        private bool CanTest=>stage==4&&engaged&&processTime>1.2f&&plant.Rate>2f&&scenario==Scenario.None&&latch==Fault.None&&plant.MeanFilm>52;
        private bool CanRecal=>Ready&&latch==Fault.Force&&AtRest&&!recalibrated;
        private bool CanVerify=>Ready&&latch==Fault.Force&&forceValid&&!verified&&AtRest&&measuredForce<.08f;
        private bool CanReset=>Ready&&latch!=Fault.None&&AtRest&&scenario==Scenario.None&&(latch!=Fault.Force||verified)&&
            (latch!=Fault.Glaze||plant.Pad>.91f)&&(latch!=Fault.Slurry||plant.Feed>.82f);
        private bool CanCertify=>stage==5&&feedbackReset&&measured&&acceptedBatches>=3&&Ready&&latch==Fault.None;

        private void Start()
        {
            manager=FindFirstObjectByType<LevelGamePlayManager>();loader=FindFirstObjectByType<SparkLevelLayoutLoader>();
            if(!manager||!loader||pins.Length!=152||contacts.Length!=1||!rig){Debug.LogError("[LEVEL 73] Missing CMP workstation references.",this);enabled=false;return;}
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
            wiredForce=Routed(Feedback);
            wiring=wires.Count==Wires.Length?null:"Remove extra wires and check the route chart.";
            for(int i=0;i<Wires.Length;i++)if(!Routed(i))
            {
                wiring=$"Connect W{i+1:00} · {pins[Wires[i].a].name} → {pins[Wires[i].b].name}.";
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
            forcePower=power&&Rail(ForcePower,ForceReturn);
            permitVoltage=power&&Rail(Wires[PermitWire].b,ControlReturn);
            if(stage==0&&Ready)Award();
            if(latch==Fault.None&&(engaged||armed||loadClock>0)&&!Ready)
            {
                bool forceOnly=rails&&safety&&!wiredForce&&wires.Count==Wires.Length-1;
                if(forceOnly)for(int i=0;i<Feedback;i++)if(!Routed(i)){forceOnly=false;break;}
                feedbackObserved|=stage==5&&armed&&forceOnly;
                Trip(forceOnly?Fault.Force:!rails?Fault.Power:!safety?Fault.Emergency:Fault.Wiring);
            }
            accumulator+=Mathf.Min(Time.deltaTime,.18f)*2.2f;
            while(accumulator>=SparkLevel73Plant.Step)
            {
                accumulator-=SparkLevel73Plant.Step;Tick(SparkLevel73Plant.Step);
            }
            Present();
        }
        private void Tick(float dt)
        {
            elapsed+=dt;
            bool signal=forcePower&&wiredForce&&calForce&&(latch!=Fault.Force||recalibrated);
            forceAge=signal?forceAge+dt:0;
            forceValid=forceAge>.45f;
            measuredForce=forceValid?Mathf.Lerp(measuredForce,plant.Downforce,dt/.14f):0;
            bool permit=Ready&&latch==Fault.None&&loaded&&clamped&&forceValid&&pid&&Running&&cool&&slurry;
            if(permit!=lastPermit)
            {
                lastPermit=permit;contacts[0].SetState(permit?SparkSwitchState.Closed:SparkSwitchState.Open);
            }
            if(engaged&&(!permit||plant.Temperature>50||plant.Vibration>.58f||plant.Downforce>4.3f))
                Trip(Fault.Overload);
            bool physical=engaged&&permit&&permitVoltage&&contacts[0].IsClosed;
            if(loadClock>0)
            {
                loadClock+=dt;
                if(loadClock>2.15f){loadClock=0;loaded=true;Log("Wafer aligned. Clamp the carrier.");}
            }
            if(wash>0)washClock+=dt;
            scanClock=Mathf.Max(0,scanClock-dt);
            plant.Tick(dt,Ready&&latch==Fault.None&&platen,Ready&&latch==Fault.None&&carrier,platenRequest,48,
                rails&&safety&&slurry,rails&&cool,Ready&&conditioning&&AtRest,physical,physical&&plant.Position>.40f,
                scenario==Scenario.Glaze,scenario==Scenario.Slurry,scenario==Scenario.Edge,wash,elapsed);
            if(conditioning&&plant.Pad>.985f)
            {
                conditioning=false;conditioned=true;
                if(scenario==Scenario.Glaze)scenario=Scenario.None;
                Log("Pad conditioned. Groove performance restored.");
            }
            if(wash==1&&washClock>1.18f&&plant.Residue<=.16f){rinsed=true;wash=0;Log("Rinse complete.");}
            if(wash==2&&washClock>.80f&&plant.Residue<.035f){brushed=true;wash=0;Log("Brush cleaning complete.");}
            if(wash==3&&washClock>1.4f&&plant.Moisture<.035f){dried=true;wash=0;Log("Spin drying complete. Run metrology.");}

            if(physical)
            {
                processTime+=dt;
                CheckTest(dt);
                if(engaged&&scenario==Scenario.None&&!armed&&calEndpoint&&forceValid&&processTime>3f)
                {
                    bool feature=plant.Optical<.460f&&plant.Eddy<30.5f&&plant.Vibration<.30f;
                    endpointTime=feature?endpointTime+dt:0;
                    if(endpointTime>.23f)
                    {
                        endpoint=true;StopMotion();
                        Log("Endpoint confirmed by optical and eddy readings. Carrier retracting.");
                    }
                }
            }
            bool goal=stage switch
            {
                1=>Calibrated&&forceValid&&inspected&&conditioned&&Running&&slurry&&plant.Feed>.90f&&cool&&loaded&&clamped&&AtRest,
                2=>physical&&processTime>1.5f&&measuredForce is >2.65f and <3.5f&&plant.Ring>.65f&&plant.Rate>3f&&plant.MeanFilm<96,
                3=>measured&&acceptedBatches>=1,
                4=>glazePassed&&restrictionPassed&&driftPassed&&measured&&acceptedBatches>=2,
                5=>CanCertify&&certify,
                _=>false
            };
            hold=Ready&&latch==Fault.None&&goal?hold+dt:0;
            if(hold>.48f)Award();
            if(plot!=null)plot.Sample(dt,forceValid?measuredForce:0,plant.Feed,plant.MeanFilm,plant.Pad);
        }

        private void CheckTest(float dt)
        {
            if(scenario==Scenario.None)return;
            testClock+=dt;
            if(scenario==Scenario.Glaze&&plant.Pad<.60f&&plant.Rate<referenceRate*.75f)
            {glazedRate=plant.Rate;glazePassed=true;Trip(Fault.Glaze);}
            else if(scenario==Scenario.Slurry&&plant.Feed<.34f&&plant.Acoustic>.11f)
            {restrictedFlow=plant.Feed;restrictionPassed=true;Trip(Fault.Slurry);}
            else if(scenario==Scenario.Edge&&testClock>.48f&&plant.Force[2]-plant.Force[0]>.25f)
            {edgeImbalance=plant.Force[2]-plant.Force[0];driftPassed=true;Trip(Fault.Edge);}
        }
        private void Award()
        {
            if(stage>=Gates.Length||!bindings.SetRuntimeComponentBinding(Gates[stage],source))return;
            stage++;hold=0;manager.QueueEvaluation();Log($"Objective {stage}/6 verified.");
        }
        private void Log(string text)=>latestEvent=$"{elapsed:0.0}s · {text}";
        private void StopMotion()
        {
            engaged=platen=carrier=armed=false;lastPermit=false;contacts[0].SetState(SparkSwitchState.Open);
        }
        private void Trip(Fault cause)
        {
            latch=cause;tripForce=plant.Downforce;StopMotion();conditioning=false;
            if(loadClock>0){loadClock=0;loaded=clamped=false;}
            if(cause==Fault.Force)
            {
                forceValid=recalibrated=verified=false;forceAge=measuredForce=0;
                Log("DOWNFORCE FEEDBACK INVALID · force control disabled; carrier retracting.");
            }
            else Log($"CMP SAFETY HOLD · {cause} · carrier retracting.");
        }

        public void Power(){power=!power;source.SetOutput(power,default);}
        public void CalSpeed(){if(Ready)calSpeed=true;}
        public void CalForce(){if(Ready&&AtRest&&latch==Fault.None){calForce=true;forceAge=0;}}
        public void CalSlurry(){if(Ready)calSlurry=true;}
        public void CalTorque(){if(Ready)calTorque=true;}
        public void CalEndpoint(){if(Ready&&AtRest)calEndpoint=true;}
        public void Inspect(){if(Ready&&AtRest){inspected=true;Log($"Groove inspection · pad {plant.Pad:P0}, wear {plant.Wear:P0}.");}}
        public void Condition(){if(CanCondition){conditioning=true;Log("Conditioner restoring pad contact effectiveness.");}}
        public void Platen(){if(Ready&&latch==Fault.None&&calSpeed)platen=true;}
        public void Carrier(){if(Ready&&latch==Fault.None&&calSpeed)carrier=true;}
        public void Slurry(){if(Ready&&!engaged&&latch==Fault.None)slurry=!slurry;}
        public void Cool(){if(Ready&&!engaged&&latch==Fault.None)cool=!cool;}
        public void Load()
        {
            if(!CanLoad)return;
            plant.NewWafer();loadClock=.001f;
            endpoint=unloaded=rinsed=brushed=dried=measured=clamped=false;
            Log("Robot transferring a new wafer.");
        }
        public void Clamp(){if(Ready&&loaded&&latch==Fault.None)clamped=true;}
        public void Pid(){if(Ready&&!engaged&&forceValid){pid=!pid;Log(pid?"Multi-zone pressure control enabled.":"Multi-zone pressure control disabled.");}}
        public void Engage(){if(CanEngage&&permitVoltage){engaged=true;processTime=endpointTime=0;Log("Lowering carrier and closing zone loops.");}}
        public void Stop(){StopMotion();Log("Controlled stop and retract.");}
        public void Unload()
        {
            if(!CanUnload)return;
            loaded=clamped=false;unloaded=endpoint;
            Log(unloaded?"Wafer transferred to the rinse chamber.":"Test wafer discarded.");
        }
        public void Rinse(){if(unloaded&&!rinsed&&wash==0){wash=1;washClock=0;}}
        public void Brush(){if(rinsed&&!brushed&&wash==0){wash=2;washClock=0;}}
        public void Dry(){if(brushed&&!dried&&wash==0){wash=3;washClock=0;}}
        public void Metrology()
        {
            if(!CanMeasure)return;
            scanClock=1.24f;
            if(plant.MeanFilm is >=23 and <=32&&plant.Planarity<=10&&plant.RemovalUniformity<=.03f&&plant.Dishing<=5&&
                plant.Erosion<=5&&plant.Defects<.35f&&plant.Residue<.04f&&plant.Moisture<.04f)
            {
                measured=true;acceptedBatches++;Log("METROLOGY PASSED · film, flatness, uniformity and defects.");
            }
            else Log("Quality outside limits. Load a fresh wafer after reviewing the film readings.");
        }
        private void Test(Scenario value){scenario=value;testClock=0;referenceRate=plant.Rate;Log($"Applying {scenario} disturbance.");}
        public void Glaze(){if(CanTest&&!glazePassed)Test(Scenario.Glaze);}
        public void Restrict(){if(CanTest&&glazePassed&&!restrictionPassed)Test(Scenario.Slurry);}
        public void Drift(){if(CanTest&&restrictionPassed&&!driftPassed)Test(Scenario.Edge);}
        public void Restore(){if(Ready&&latch is Fault.Slurry or Fault.Edge){scenario=Scenario.None;Log("Fault corrected; verify safe conditions before reset.");}}
        public void EStop(){emergency.SetState(emergency.IsClosed?SparkSwitchState.Open:SparkSwitchState.Closed);if(emergency.IsOpen&&latch==Fault.None)Trip(Fault.Emergency);}
        public void Reset()
        {
            if(!CanReset)return;
            feedbackReset|=latch==Fault.Force&&feedbackObserved;
            latch=Fault.None;Log("Safety reset. Recommission both drives before polishing.");
        }
        public void Arm(){if(CanArm){armed=true;Log("Remove W98 with Undo Last Wire while contact is active.");}}
        public void Recal(){if(CanRecal){recalibrated=true;forceAge=0;Log("Load cell zeroed. Wait for fresh unloaded samples.");}}
        public void Verify(){if(CanVerify){verified=true;Log("Fresh unloaded force samples verified.");}}
        public void Certify(){if(CanCertify)certify=true;}
        public void Speed(){if(Ready&&!engaged&&!platen){platenRequest=platenRequest>66?58:platenRequest<61?64:70;Log($"Platen speed request {platenRequest:0} RPM.");}}

        private bool CanCondition=>Ready&&AtRest&&!conditioning&&inspected&&(latch==Fault.None||latch==Fault.Glaze);
        private bool CanMeasure=>Ready&&endpoint&&dried&&!measured&&latch==Fault.None;
        private bool CanArm=>stage==5&&engaged&&plant.Downforce>2.4f&&!armed&&!feedbackObserved;

        private string Status()
        {
            if(stage==6)return "CERTIFIED · fresh wafer met flatness and quality limits.";
            if(!power)return "Enable 5 V CONTROL and complete wiring.";
            if(latch==Fault.Force)return !Ready?"DOWNFORCE FEEDBACK INVALID · reconnect W98.":CanRecal?"Select RECAL FORCE.":!verified?"Wait for fresh force samples and select VERIFY ZERO.":"Select RESET SAFETY and recommission.";
            if(latch!=Fault.None)return $"CMP SAFETY HOLD · {latch} · restore the affected system.";
            if(!Ready)return wiring??"Check the 5 V and emergency-stop loop.";
            if(loadClock>0)return "Robot transferring and aligning wafer.";
            if(armed)return "Disconnect W98 from the downforce sensor to the pressure controller.";
            if(stage==1)return "Calibrate, inspect and condition the pad, start both drives and slurry, enable cooling, and load and clamp.";
            if(wash>0)return wash==1?"Rinsing wafer.":wash==2?"Brush cleaning wafer.":"Spin drying wafer.";
            if(CanCertify)return "Fresh wafer accepted. Select CERTIFY.";
            if(measured)return stage==4?"Load a test wafer and run the three disturbances.":"Wafer accepted. Load a new test wafer.";
            if(endpoint)return !unloaded?"ENDPOINT CONFIRMED · wait for retraction and unload.":!rinsed?"RINSE the wafer.":!brushed?"BRUSH CLEAN the wafer.":!dried?"SPIN DRY the wafer.":"Run METROLOGY.";
            if(engaged&&stage==4&&!driftPassed)return !glazePassed?"Apply PAD GLAZE.":!restrictionPassed?"Apply SLURRY RESTRICT.":"Apply EDGE DRIFT.";
            if(engaged&&stage==5&&!feedbackObserved)return "At stable force select W98 TEST.";
            return engaged?"POLISHING · optical and eddy-current features monitored.":"Start both drives, load and clamp, enable zone PID, and engage.";
        }
        private void Present()
        {
            statusText.text=Status();
            meterText.text=$"PLATEN {plant.PlatenRpm:0} RPM   ·   CARRIER {plant.CarrierRpm:0} RPM   ·   FORCE {(forceValid?measuredForce.ToString("0.00"):"INVALID")}   ·   SLURRY {plant.Feed:0.00}   ·   INTERFACE {plant.Temperature:0.0}°C";
            detailText.text=$"FILM {plant.MeanFilm:0.0} nm   ·   HEIGHT {plant.Planarity:0.0} nm   ·   PAD {plant.Pad:P0}   ·   EDGE {plant.Force[2]:0.00}   ·   NONUNIFORMITY {plant.RemovalUniformity:P1}";
            calibrationText.text=$"{latestEvent}   |   EDDY {plant.Eddy:0.0} nm   ·   OPT {plant.Optical:0.000}   ·   DRIVE {plant.Current:0.00}   ·   VIB {plant.Vibration:0.00}";
            if(controllerDisplay)controllerDisplay.text=$"{plant.PlatenRpm:0} / {plant.CarrierRpm:0} RPM";
            if(qualityDisplay)qualityDisplay.text=$"{plant.MeanFilm:0.0} nm · {plant.Planarity:0.0} nm Δ";
            powerButton.interactable=stage<6;calSpeedButton.interactable=Ready&&!calSpeed;calForceButton.interactable=Ready&&!calForce&&AtRest;
            calSlurryButton.interactable=Ready&&!calSlurry;calTorqueButton.interactable=Ready&&!calTorque;calEndpointButton.interactable=Ready&&!calEndpoint&&AtRest;
            inspectButton.interactable=Ready&&AtRest;conditionButton.interactable=CanCondition;
            platenButton.interactable=Ready&&latch==Fault.None&&calSpeed&&!platen;carrierButton.interactable=Ready&&latch==Fault.None&&calSpeed&&!carrier;
            slurryButton.interactable=Ready&&!engaged&&latch==Fault.None;coolButton.interactable=Ready&&!engaged&&latch==Fault.None;
            loadButton.interactable=CanLoad;clampButton.interactable=Ready&&loaded&&!clamped&&latch==Fault.None;
            pidButton.interactable=Ready&&!engaged&&forceValid&&!pid;engageButton.interactable=CanEngage&&permitVoltage;
            stopButton.interactable=engaged||platen||carrier;unloadButton.interactable=CanUnload;
            rinseButton.interactable=Ready&&unloaded&&!rinsed&&wash==0;brushButton.interactable=Ready&&rinsed&&!brushed&&wash==0;
            dryButton.interactable=Ready&&brushed&&!dried&&wash==0;metrologyButton.interactable=CanMeasure;
            glazeButton.interactable=CanTest&&!glazePassed;restrictButton.interactable=CanTest&&glazePassed&&!restrictionPassed;driftButton.interactable=CanTest&&restrictionPassed&&!driftPassed;
            restoreButton.interactable=Ready&&scenario!=Scenario.None&&latch is Fault.Slurry or Fault.Edge;eStopButton.interactable=stage<6;
            resetButton.interactable=CanReset;armButton.interactable=CanArm;recalButton.interactable=CanRecal;
            verifyButton.interactable=CanVerify;certifyButton.interactable=CanCertify&&!certify;speedButton.interactable=Ready&&!engaged&&!platen;
            rig.Apply(plant,elapsed,loadClock,loaded,conditioning,wash,scanClock,emergency.IsOpen);
        }
    }
}
