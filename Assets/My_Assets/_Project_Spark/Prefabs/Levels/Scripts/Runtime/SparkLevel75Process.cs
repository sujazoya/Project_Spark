using System;
using System.Collections.Generic;
using ProjectSpark.Circuit;
using ProjectSpark.Visuals;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.Gameplay
{
    public sealed class SparkLevel75Process : MonoBehaviour
    {
        public static readonly string[] Gates={"L75_Wired","L75_Commissioned","L75_Qualified","L75_Implanted","L75_Disturbances","L75_Recertified"};
        public static readonly (int a,int b)[] Wires={(0,1),(2,3),(0,4),(2,5),(0,6),(2,7),(0,8),(2,9),(0,10),(2,11),(12,13),(14,15),(16,17),(18,19),(20,21),(22,23),(24,25),(26,27),(28,29),(30,31),(32,33),(34,35),(36,37),(38,39),(40,41),(42,43),(44,45),(46,47),(48,49),(50,51),(52,53),(54,55),(56,57),(58,59),(60,61),(62,63),(64,65),(66,67),(68,69),(70,71),(72,73),(74,75),(76,77),(78,79),(80,81),(82,83),(84,85),(86,87),(88,89),(90,91),(92,93),(94,95),(96,97),(98,99),(100,101),(102,103),(104,105),(106,107),(108,109),(110,111),(112,113),(114,115),(116,117),(118,119),(119,120),(59,121),(122,123),(91,124),(95,125),(99,126),(125,127),(94,128),(49,129),(59,130),(130,131),(126,132),(129,133),(89,134),(134,135),(127,136),(12,137),(137,138),(43,139),(139,140),(139,141),(130,142),(142,143),(135,144),(144,145),(145,146),(61,147),(148,149),(149,150),(43,151),(151,152),(33,153),(153,154),(84,155),(127,156),(130,157),(131,158),(146,159),(146,160),(133,161),(122,162),(149,163)};
        private const int StopInput=149,ControlReturn=3,CupPower=10,CupReturn=11,Feedback=66,PermitWire=46;
        public SparkLevelLayout layout;
        public SparkPowerSupply source;
        public SparkSwitch[] contacts;
        public SparkSwitch emergency;
        public SparkTerminal[] pins;
        public SparkLevel75VisualRig rig;
        public SparkLevel75Trend plot;
        public TMP_Text controllerDisplay,qualityDisplay;
        public Text statusText,meterText,detailText,calibrationText;
        public Button powerButton,calVacButton,backButton,turboButton,loadButton,registerButton,clampButton,coolingButton,neutralButton,
            sourceOnButton,calAnalyzerButton,tuneButton,energyButton,focusButton,profileButton,calCupButton,implantButton,stopButton,unloadButton,
            activateButton,depthButton,sheetButton,analyzerDriftButton,focusFaultButton,neutralFaultButton,restoreButton,eStopButton,resetButton,
            armButton,recalButton,verifyButton,certifyButton;

        private enum Fault { None, Power, Wiring, EStop, Analyzer, Focus, Charge, Temperature, Faraday }
        private enum Scenario { None, Analyzer, Focus, Charge }
        private readonly SparkLevel75Plant plant=new();
        private readonly HashSet<(int,int)> wires=new();
        private readonly List<SparkCircuitConnection> edges=new();
        private LevelGamePlayManager manager;
        private SparkLevelLayoutLoader loader;
        private SparkLevelSceneBindings bindings;
        private int topology=-1,stage,region,tail=-1,acceptedBatches;
        private bool power,rails,safety,cupPower,wiredCup,permitVoltage,lastPermit;
        private bool calVac,backing,turbo,loaded,registered,clamped,cooling,neutral,sourceOn,calAnalyzer,tuned,energy,optics;
        private bool profile,calCup,validCup,recalibrated,verified,implanting,completed,unloaded,activating,depthPass,sheetPass;
        private bool analyzerPassed,focusPassed,chargePassed,armed,feedbackObserved,feedbackReset,certify,batchCounted;
        private float elapsed,accumulator,hold,age,loadClock,profileClock,metrology,referenceClock,measured,regionDose;
        private float impurity,widthFault,chargeFault,beamAtTrip,doseAtTrip;
        private string wiring,latestEvent="Connect the implantation controls and enable 5 V.";
        private Fault latch;
        private Scenario scenario;

        private bool Ready=>rails&&safety&&wiring==null;
        private bool Vacuum=>calVac&&backing&&turbo&&plant.Backing>.86f&&plant.Turbo>.89f&&plant.Pressure<1.4f;
        private bool AtRest=>plant.Blanker>.987f&&plant.UsefulCurrent<.020f&&!implanting;
        private bool CanLoad=>Ready&&Vacuum&&AtRest&&!loaded&&!activating&&latch==Fault.None&&loadClock<=0&&
            Mathf.Abs(plant.Pressure-plant.LockPressure)<1.2f;
        private bool CanTurbo=>Ready&&calVac&&backing&&!turbo&&plant.Backing>.66f&&plant.Pressure<135;
        private bool BeamReady=>Ready&&latch==Fault.None&&Vacuum&&sourceOn&&calAnalyzer&&tuned&&energy&&optics&&
            plant.Upstream>.66f&&plant.Purity>=.98f&&plant.Energy>.985f&&plant.Width<.266f&&Mathf.Abs(plant.Centroid)<.025f;
        private bool CanProfile=>BeamReady&&AtRest&&!profile&&profileClock<=0&&referenceClock<=0;
        private bool CanCalCup=>BeamReady&&AtRest&&profile&&!calCup&&referenceClock<=0;
        private bool CanImplant=>stage>=3&&BeamReady&&profile&&validCup&&loaded&&registered&&clamped&&cooling&&neutral&&AtRest&&
            !completed&&plant.MeanDose<.10f&&plant.Cup<.04f&&plant.Charge<.68f;
        private bool CanArm=>stage==5&&implanting&&plant.UsefulCurrent>.46f&&!armed&&!feedbackObserved;
        private bool CanRecal=>Ready&&latch==Fault.Faraday&&AtRest&&!recalibrated&&referenceClock<=0;
        private bool CanVerify=>Ready&&latch==Fault.Faraday&&AtRest&&validCup&&!verified&&plant.UsefulCurrent<.02f;
        private bool CanReset=>Ready&&latch!=Fault.None&&scenario==Scenario.None&&AtRest&&plant.Charge<.64f&&plant.Temperature<38&&
            (latch!=Fault.Faraday||verified);
        private bool CanDepth=>Ready&&unloaded&&completed&&plant.Activation>.95f&&!depthPass&&latch==Fault.None;
        private bool CanSheet=>Ready&&unloaded&&completed&&plant.Activation>.95f&&!sheetPass&&latch==Fault.None;
        private bool CanCertify=>stage==5&&feedbackReset&&batchCounted&&acceptedBatches>=3&&Ready&&latch==Fault.None;
        private void Start()
        {
            manager=FindFirstObjectByType<LevelGamePlayManager>();loader=FindFirstObjectByType<SparkLevelLayoutLoader>();
            if(!manager||!loader||pins.Length!=164||contacts.Length!=1||!rig){Debug.LogError("[LEVEL 75] Missing implantation references.",this);enabled=false;return;}
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
            wiredCup=Routed(Feedback);
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
            cupPower=power&&Rail(CupPower,CupReturn);
            permitVoltage=power&&Rail(Wires[PermitWire].b,ControlReturn);
            if(stage==0&&Ready)Award();
            if(latch==Fault.None&&(implanting||loadClock>0||activating)&&!Ready)
            {
                bool cupOnly=rails&&safety&&!wiredCup&&wires.Count==Wires.Length-1;
                if(cupOnly)for(int i=0;i<Wires.Length;i++)if(i!=Feedback&&!Routed(i)){cupOnly=false;break;}
                feedbackObserved|=stage==5&&armed&&cupOnly;
                Trip(cupOnly?Fault.Faraday:!rails?Fault.Power:!safety?Fault.EStop:Fault.Wiring);
            }
            accumulator+=Mathf.Min(Time.deltaTime,.18f)*2.15f;
            while(accumulator>=SparkLevel75Plant.Step){accumulator-=SparkLevel75Plant.Step;Tick(SparkLevel75Plant.Step);}
            Present();
        }
        private void Tick(float dt)
        {
            elapsed+=dt;
            bool signal=cupPower&&wiredCup&&calCup&&(latch!=Fault.Faraday||recalibrated);
            age=signal?age+dt:0;validCup=age>.38f;
            measured=validCup?Mathf.Lerp(measured,plant.Current,dt/.095f):0;
            bool permit=implanting&&BeamReady&&validCup&&registered&&clamped&&neutral&&cooling&&plant.Charge<1.5f&&plant.Temperature<45;
            if(permit!=lastPermit){lastPermit=permit;contacts[0].SetState(permit?SparkSwitchState.Closed:SparkSwitchState.Open);}
            bool open=permit&&permitVoltage&&contacts[0].IsClosed;
            bool pump=rails&&safety&&(Ready||latch==Fault.Faraday);
            bool upstream=sourceOn&&(Ready&&latch==Fault.None||(latch==Fault.Faraday&&plant.Blanker<.994f));
            if(loadClock>0)
            {
                loadClock+=dt;
                if(loadClock>1.9f){loadClock=0;loaded=true;Log("Patterned wafer seated. Verify mask and clamp.");}
            }
            plant.Tick(dt,pump&&backing,pump&&turbo,loadClock is >.10f and <1.58f,upstream,tuned,energy,optics,pump&&cooling,pump&&neutral,
                !open,profileClock>0||referenceClock>0,activating,scenario==Scenario.Analyzer,scenario==Scenario.Focus,scenario==Scenario.Charge,
                implanting?region:tail,registered,elapsed);
            metrology=Mathf.Max(0,metrology-dt);
            if(tail>=0&&plant.Blanker>.997f)tail=-1;
            if(profileClock>0)
            {
                profileClock=Mathf.Max(0,profileClock-dt);
                if(profileClock==0&&BeamReady){profile=true;Log("Beam profile and corner cups qualified.");}
            }
            if(referenceClock>0)
            {
                referenceClock=Mathf.Max(0,referenceClock-dt);
                if(referenceClock==0&&AtRest)
                {
                    if(latch==Fault.Faraday){recalibrated=true;calCup=true;age=0;Log("Faraday zero calibrated with the beam at the protected state.");}
                    else if(BeamReady){calCup=true;age=0;Log("Faraday reference calibrated against the selected beam.");}
                }
            }
            if(latch==Fault.None)CheckFaults();
            if(open&&implanting&&plant.UsefulCurrent>.0001f)
            {
                regionDose+=measured/.982f*(1-plant.Blanker)*(1-plant.Cup)*158*plant.Purity*dt;
                if(regionDose>=(region==15?87.2f:99.5f))
                {
                    if(region==15){completed=true;StopBeam();Log("Sixteen regions scanned. Beam blanking before unload.");}
                    else{region++;regionDose=0;}
                }
            }
            if(activating&&plant.Activation>.967f){activating=false;Log("Thermal activation complete. Run depth and sheet metrology.");}
            if(depthPass&&sheetPass&&!batchCounted){batchCounted=true;acceptedBatches++;}
            bool goal=stage switch
            {
                1=>Vacuum&&loaded&&registered&&clamped&&cooling&&neutral&&Mathf.Abs(plant.Angle-6)<.25f&&plant.Gate<.06f,
                2=>BeamReady&&profile&&validCup&&plant.Cup<.05f&&AtRest,
                3=>batchCounted&&acceptedBatches>=1,
                4=>analyzerPassed&&focusPassed&&chargePassed&&batchCounted&&acceptedBatches>=2,
                5=>CanCertify&&certify,
                _=>false
            };
            hold=Ready&&latch==Fault.None&&goal?hold+dt:0;
            if(hold>.43f)Award();
            if(plot!=null)plot.Show(plant,validCup,implanting?region:-1);
        }
        private void CheckFaults()
        {
            if(scenario==Scenario.Analyzer&&plant.Purity<.90f)
            {impurity=plant.Purity;analyzerPassed=true;Trip(Fault.Analyzer);}
            else if(scenario==Scenario.Focus&&plant.Width>.315f)
            {widthFault=plant.Width;focusPassed=true;Trip(Fault.Focus);}
            else if(implanting&&plant.Charge>1.27f)
            {chargeFault=plant.Charge;chargePassed|=scenario==Scenario.Charge;Trip(Fault.Charge);}
            else if(implanting&&plant.Temperature>43f)Trip(Fault.Temperature);
        }
        private void Award()
        {
            if(stage>=Gates.Length||!bindings.SetRuntimeComponentBinding(Gates[stage],source))return;
            stage++;hold=0;manager.QueueEvaluation();Log($"Objective {stage}/6 verified.");
        }
        private void Log(string text)=>latestEvent=$"{elapsed:0.0}s · {text}";
        private void StopBeam()
        {
            if(implanting)tail=region;
            implanting=armed=false;lastPermit=false;contacts[0].SetState(SparkSwitchState.Open);
        }
        private void Trip(Fault cause)
        {
            latch=cause;beamAtTrip=plant.Upstream;doseAtTrip=plant.MeanDose;StopBeam();
            profileClock=referenceClock=0;activating=false;profile=false;
            if(loadClock>0){loadClock=0;loaded=clamped=registered=false;}
            if(cause==Fault.Faraday){validCup=recalibrated=verified=false;age=measured=0;Log("FARADAY FEEDBACK INVALID · beam gate closing.");}
            else Log($"BEAMLINE SAFETY HOLD · {cause} · blanker commanded closed.");
        }

        public void Power(){power=!power;source.SetOutput(power,default);}
        public void CalVac(){if(Ready)calVac=true;}
        public void Back(){if(Ready&&calVac)backing=true;}
        public void Turbo(){if(CanTurbo)turbo=true;}
        public void Load()
        {
            if(!CanLoad)return;
            plant.NewWafer();region=0;tail=-1;regionDose=0;loadClock=.001f;
            loaded=registered=clamped=completed=unloaded=depthPass=sheetPass=batchCounted=false;
            Log("Load-lock opening for a fresh patterned wafer.");
        }
        public void Register(){if(Ready&&loaded&&!implanting&&latch==Fault.None){registered=true;Log("Mask verified and wafer registration angle commanded.");}}
        public void Clamp(){if(Ready&&loaded&&registered&&latch==Fault.None)clamped=true;}
        public void Cooling(){if(Ready&&AtRest&&latch==Fault.None)cooling=true;}
        public void Neutral(){if(Ready&&AtRest&&latch==Fault.None)neutral=true;}
        public void SourceOn(){if(Ready&&Vacuum&&AtRest&&latch==Fault.None)sourceOn=true;}
        public void CalAnalyzer(){if(Ready&&sourceOn&&plant.Source>.92f&&AtRest&&latch==Fault.None)calAnalyzer=true;}
        public void Tune(){if(Ready&&calAnalyzer&&AtRest&&latch==Fault.None){tuned=true;Log("Tracking selected species through the magnetic analyzer.");}}
        public void Energy(){if(Ready&&tuned&&AtRest&&latch==Fault.None)energy=true;}
        public void Focus(){if(Ready&&energy&&AtRest&&latch==Fault.None)optics=true;}
        public void Profile(){if(CanProfile){profileClock=.85f;Log("Inserting profiler and sampling the beam profile.");}}
        public void CalCup(){if(CanCalCup){referenceClock=.73f;Log("Inserting Faraday reference cup.");}}
        public void Implant(){if(CanImplant){implanting=true;region=0;regionDose=0;Log("Raster implantation across 16 wafer regions.");}}
        public void Stop(){StopBeam();Log("Beam blanking requested.");}
        public void Unload()
        {
            if(!CanUnload)return;
            loaded=clamped=registered=false;unloaded=completed;
            Log(unloaded?"Wafer in activation chamber.":"Interrupted wafer discarded.");
        }
        public void Activate(){if(Ready&&unloaded&&completed&&plant.Activation<.96f&&!activating&&latch==Fault.None)activating=true;}
        private bool DoseOK=>plant.MeanDose is >=97 and <=104&&plant.Difference<.02f&&plant.Leakage<=.01f&&plant.Damage<.37f;
        public void Depth(){if(CanDepth){metrology=1.3f;depthPass=DoseOK&&plant.Junction is >=.95f and <=1.05f;Log(depthPass?"Junction-depth profile passed.":"Depth or local dose out of tolerance.");}}
        public void Sheet(){if(CanSheet){metrology=1.3f;sheetPass=DoseOK&&plant.Sheet is >=.92f and <=1.09f;Log(sheetPass?"Sheet-resistance metrology passed.":"Sheet resistance or wafer uniformity out of tolerance.");}}
        public void AnalyzerDrift(){if(CanAnalyzerFault){scenario=Scenario.Analyzer;Log("Analyzer field drift injected.");}}
        public void FocusFault(){if(CanFocusFault){scenario=Scenario.Focus;Log("Quadrupole focus disturbance injected.");}}
        public void NeutralFault(){if(CanChargeFault){scenario=Scenario.Charge;Log("Neutralization interrupted during a live scan.");}}
        public void Restore(){if(Ready&&latch!=Fault.None&&scenario!=Scenario.None){scenario=Scenario.None;Log("Disturbance removed. Verify safe conditions before reset.");}}
        public void EStop(){emergency.SetState(emergency.IsClosed?SparkSwitchState.Open:SparkSwitchState.Closed);if(emergency.IsOpen&&latch==Fault.None)Trip(Fault.EStop);}
        public void Reset(){if(CanReset){feedbackReset|=latch==Fault.Faraday&&feedbackObserved;latch=Fault.None;Log("Safety reset. Qualify the beam profile again.");}}
        public void Arm(){if(CanArm){armed=true;Log("Remove W067 with Undo Last Wire during the live raster.");}}
        public void Recal(){if(CanRecal){referenceClock=.68f;Log("Collecting Faraday zero reference.");}}
        public void Verify(){if(CanVerify){verified=true;Log("Dose-current feedback and protected beam path verified.");}}
        public void Certify(){if(CanCertify)certify=true;}

        private bool CanUnload=>Ready&&loaded&&AtRest&&loadClock<=0&&latch==Fault.None;
        private bool CanAnalyzerFault=>stage==4&&BeamReady&&profile&&AtRest&&!analyzerPassed&&scenario==Scenario.None;
        private bool CanFocusFault=>stage==4&&BeamReady&&profile&&AtRest&&analyzerPassed&&!focusPassed&&scenario==Scenario.None;
        private bool CanChargeFault=>stage==4&&implanting&&plant.UsefulCurrent>.55f&&analyzerPassed&&focusPassed&&!chargePassed&&scenario==Scenario.None;
        private string Status()
        {
            if(stage==6)return "CERTIFIED · fresh 16-region wafer passed.";
            if(!power)return "Enable 5 V CONTROL and complete wiring.";
            if(latch==Fault.Faraday)return !Ready?"FARADAY FEEDBACK INVALID · reconnect W067.":CanRecal?"Select RECAL CUP after the gate closes.":!verified?"Wait for valid samples and select VERIFY DOSE.":"Select RESET SAFETY and requalify the beam.";
            if(latch!=Fault.None)return $"BEAMLINE SAFETY HOLD · {latch} · restore and reset.";
            if(!Ready)return wiring??"Verify power and E-stop loop.";
            if(armed)return "Disconnect W067 during the live raster.";
            if(loadClock>0)return "Patterned wafer transferring through load-lock.";
            if(stage==1)return "Calibrate vacuum, start both pumps, load, register and clamp. Enable cooling and neutralization.";
            if(activating)return "Thermal activation in progress.";
            if(CanCertify)return "Fresh wafer accepted. Select CERTIFY.";
            if(implanting)return $"IMPLANTING · REGION {region+1:00}/16 · cup current integrated.";
            if(completed&&!batchCounted)return !unloaded?"Wait for blanking, then UNLOAD.":plant.Activation<.95f?"Select ACTIVATE.":"Run DEPTH and SHEET metrology.";
            if(!profile||!validCup)return "Qualify source, species, energy, focus, beam profile and Faraday feedback.";
            if(stage==4&&!analyzerPassed)return "Apply ANALYZER DRIFT to the qualified beam.";
            if(stage==4&&!focusPassed)return "Apply FOCUS FAULT after beam requalification.";
            if(batchCounted)return "Wafer accepted. Load a fresh wafer for the next sequence.";
            return "Prepare a registered wafer and select IMPLANT.";
        }
        private void Present()
        {
            statusText.text=Status();
            meterText.text=$"VACUUM {plant.Pressure:0.00}   ·   PURITY {plant.Purity:P2}   ·   FARADAY {(validCup?measured.ToString("0.00"):"STALE")}   ·   CHARGE {plant.Charge:0.00}   ·   WAFER {plant.Temperature:0.0}°C";
            detailText.text=$"DOSE {plant.MeanDose:0.0} / 100   ·   UNIFORMITY {plant.Difference:P1}   ·   MASK LEAK {plant.Leakage:P2}   ·   GATE CLOSED {plant.Blanker:P0}";
            calibrationText.text=$"{latestEvent}   |   BEAM WIDTH {plant.Width:0.000}   ·   ACTIVATION {plant.Activation:P0}";
            if(controllerDisplay)controllerDisplay.text=$"{plant.Purity:P1} A · {plant.Energy:0.00}";
            if(qualityDisplay)qualityDisplay.text=$"{plant.MeanDose:0.0} DOSE · {plant.Activation:P0}";
            powerButton.interactable=stage<6;calVacButton.interactable=Ready&&!calVac;
            backButton.interactable=Ready&&calVac&&!backing;turboButton.interactable=CanTurbo;
            loadButton.interactable=CanLoad;registerButton.interactable=Ready&&loaded&&!registered&&latch==Fault.None;
            clampButton.interactable=Ready&&loaded&&registered&&!clamped&&latch==Fault.None;
            coolingButton.interactable=Ready&&AtRest&&!cooling&&latch==Fault.None;neutralButton.interactable=Ready&&AtRest&&!neutral&&latch==Fault.None;
            sourceOnButton.interactable=Ready&&Vacuum&&AtRest&&!sourceOn&&latch==Fault.None;
            calAnalyzerButton.interactable=Ready&&sourceOn&&plant.Source>.92f&&AtRest&&!calAnalyzer&&latch==Fault.None;
            tuneButton.interactable=Ready&&calAnalyzer&&!tuned&&AtRest&&latch==Fault.None;
            energyButton.interactable=Ready&&tuned&&!energy&&AtRest&&latch==Fault.None;focusButton.interactable=Ready&&energy&&!optics&&AtRest&&latch==Fault.None;
            profileButton.interactable=CanProfile;calCupButton.interactable=CanCalCup;
            implantButton.interactable=CanImplant;stopButton.interactable=implanting;
            unloadButton.interactable=CanUnload;activateButton.interactable=Ready&&unloaded&&completed&&plant.Activation<.96f&&!activating&&latch==Fault.None;
            depthButton.interactable=CanDepth;sheetButton.interactable=CanSheet;
            analyzerDriftButton.interactable=CanAnalyzerFault;focusFaultButton.interactable=CanFocusFault;neutralFaultButton.interactable=CanChargeFault;
            restoreButton.interactable=Ready&&latch!=Fault.None&&scenario!=Scenario.None;eStopButton.interactable=stage<6;
            resetButton.interactable=CanReset;armButton.interactable=CanArm;recalButton.interactable=CanRecal;verifyButton.interactable=CanVerify;
            certifyButton.interactable=CanCertify&&!certify;
            rig.Apply(plant,elapsed,loadClock,loaded,implanting,latch!=Fault.None,metrology,emergency.IsOpen);
        }
    }
}
