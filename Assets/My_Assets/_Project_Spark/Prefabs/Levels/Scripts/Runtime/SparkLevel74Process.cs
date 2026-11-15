using System;
using System.Collections.Generic;
using ProjectSpark.Circuit;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.Gameplay
{
    public sealed class SparkLevel74Process : MonoBehaviour
    {
        public static readonly string[] Gates={"L74_Wired","L74_Prepared","L74_Aligned","L74_Printed","L74_Disturbances","L74_Recertified"};
        public static readonly (int a,int b)[] Wires={(0,1),(2,3),(0,4),(2,5),(0,6),(2,7),(0,8),(2,9),(0,10),(2,11),(12,13),(14,15),(16,17),(18,19),(20,21),(22,23),(24,25),(26,27),(28,29),(30,31),(32,33),(34,35),(36,37),(38,39),(40,41),(42,43),(44,45),(46,47),(48,49),(50,51),(52,53),(54,55),(56,57),(58,59),(60,61),(62,63),(64,65),(66,67),(68,69),(70,71),(72,73),(74,75),(76,77),(78,79),(80,81),(82,83),(84,85),(86,87),(88,89),(90,91),(92,93),(94,95),(96,97),(98,99),(100,101),(102,103),(104,105),(106,107),(108,109),(110,111),(112,113),(114,115),(105,116),(109,117),(111,118),(113,119),(37,120),(115,121),(115,122),(95,123),(123,124),(124,125),(125,126),(107,127),(113,128),(71,129),(59,130),(51,131),(89,132),(126,133),(134,135),(135,136),(77,137),(41,138),(101,139),(43,140),(41,141),(59,142),(75,143),(111,144),(25,145),(29,146),(47,147),(95,148),(75,149),(133,150),(136,151),(41,152),(101,153),(113,154),(59,155),(156,157)};
        private const int StopInput=135, ControlReturn=3, SensorPower=10, SensorReturn=11, Feedback=101, PermitWire=42;
        public SparkLevelLayout layout;
        public SparkPowerSupply source;
        public SparkSwitch[] contacts;
        public SparkSwitch emergency;
        public SparkTerminal[] pins;
        public SparkLevel74VisualRig rig;
        public SparkLevel74Trend plot;
        public TMP_Text controllerDisplay,qualityDisplay;
        public Text statusText,meterText,detailText,calibrationText;
        public Button powerButton,calPrepButton,loadButton,coatButton,softBakeButton,transferButton,reticleButton,clampButton,
            homeButton,calPositionButton,alignButton,focusButton,isolateButton,syncButton,dryScanButton,lampButton,calDoseButton,
            apertureButton,printButton,stopButton,postBakeButton,developButton,rinseButton,dryButton,metrologyButton,nozzleButton,
            shakeButton,lensButton,restoreButton,eStopButton,resetButton,armButton,recalButton,referenceButton,certifyButton;

        private enum Fault { None, Power, Wiring, EStop, Position, Nozzle, Vibration, Lens, Exposure }
        private enum Scenario { None, Nozzle, Shake, Lens }
        private readonly SparkLevel74Plant plant=new();
        private readonly HashSet<(int,int)> wires=new();
        private readonly List<SparkCircuitConnection> edges=new();
        private LevelGamePlayManager manager;
        private SparkLevelLayoutLoader loader;
        private SparkLevelSceneBindings bindings;
        private int topology=-1,stage,processing,field=-1,acceptedBatches;
        private bool power,rails,safety,sensorPower,wiredPosition,permitVoltage,lastPermit;
        private bool calPrep,calPosition,calDose,validPosition,recalibrated,referenced,referenceRequest;
        private bool loaded,coated,baked,transferred,reticle,clamped,home,aligned,mapped,isolation,sync,lamp,dryPassed;
        private bool dryRun,printing,scanning,postBaked,developed,rinsed,dried,measured,certify;
        private bool nozzlePassed,shakePassed,lensPassed,armed,positionObserved,positionReset;
        private float elapsed,accumulator,hold,sampleAge,workClock,scanClock,settled,transferClock;
        private float aperture=.74f,restrictedThickness,restrictedFlow,shakePeak,lensPeak;
        private double measuredStop,errorPeak;
        private string wiring,latestEvent="Connect the scanner controls and enable 5 V.";
        private Fault latch;
        private Scenario scenario;

        private bool Ready=>rails&&safety&&wiring==null;
        private bool Idle=>!printing&&!dryRun&&processing==0&&transferClock<=0;
        private bool Stopped=>plant.Speed<8&&plant.Shutter<.02f;
        private bool CanLoad=>Ready&&Idle&&Stopped&&latch==Fault.None&&(!loaded||stage>=3);
        private bool CanCoat=>Ready&&Idle&&loaded&&!coated&&!transferred&&calPrep&&latch==Fault.None;
        private bool BaseScan=>Ready&&latch==Fault.None&&transferred&&reticle&&clamped&&home&&validPosition&&aligned&&mapped&&isolation&&sync;
        private bool CanDry=>stage>=2&&BaseScan&&Idle&&!dryPassed;
        private bool CanPrint=>stage>=3&&BaseScan&&Idle&&dryPassed&&lamp&&calDose&&plant.Lamp>.985f&&
            Math.Abs(plant.Aperture-.74f)<.019f&&plant.Lens<27.15f&&!postBaked&&!measured&&plant.CountExposed()==0&&permitVoltage;
        private bool CanRecal=>Ready&&latch==Fault.Position&&!recalibrated&&Stopped;
        private bool CanReference=>Ready&&latch==Fault.Position&&recalibrated&&validPosition&&!referenced&&!referenceRequest;
        private bool CanReset=>Ready&&latch!=Fault.None&&Stopped&&scenario==Scenario.None&&
            (latch!=Fault.Position||referenced&&validPosition)&&(latch!=Fault.Lens||plant.Lens<25.9f);
        private bool CanMeasure=>Ready&&Idle&&dried&&!measured&&latch==Fault.None;
        private bool CanCertify=>stage==5&&positionReset&&measured&&acceptedBatches>=3&&Ready&&latch==Fault.None;

        private void Start()
        {
            manager=FindFirstObjectByType<LevelGamePlayManager>();loader=FindFirstObjectByType<SparkLevelLayoutLoader>();
            if(!manager||!loader||pins.Length!=158||contacts.Length!=1||!rig){Debug.LogError("[LEVEL 74] Missing scanner references.",this);enabled=false;return;}
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
            wiredPosition=Routed(Feedback);
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
            sensorPower=power&&Rail(SensorPower,SensorReturn);
            permitVoltage=power&&Rail(Wires[PermitWire].b,ControlReturn);
            if(stage==0&&Ready)Award();
            if(latch==Fault.None&&(!Idle||armed)&&!Ready)
            {
                bool positionOnly=rails&&safety&&!wiredPosition&&wires.Count==Wires.Length-1;
                if(positionOnly)for(int i=0;i<Feedback;i++)if(!Routed(i)){positionOnly=false;break;}
                positionObserved|=stage==5&&armed&&positionOnly;
                Trip(positionOnly?Fault.Position:!rails?Fault.Power:!safety?Fault.EStop:Fault.Wiring);
            }
            accumulator+=Mathf.Min(Time.deltaTime,.18f)*1.75f;
            while(accumulator>=SparkLevel74Plant.Step){accumulator-=SparkLevel74Plant.Step;Tick(SparkLevel74Plant.Step);}
            Present();
        }
        private void Tick(float dt)
        {
            elapsed+=dt;
            bool sensor=sensorPower&&wiredPosition&&calPosition&&(latch!=Fault.Position||recalibrated);
            sampleAge=sensor?sampleAge+dt:0;
            validPosition=sampleAge>.42f;
            bool permit=BaseScan&&lamp&&calDose&&plant.Lens<29f;
            if(lastPermit!=permit){lastPermit=permit;contacts[0].SetState(permit?SparkSwitchState.Closed:SparkSwitchState.Open);}
            bool drive=Ready&&(home&&latch==Fault.None||referenceRequest);
            int activeField=dryRun?5:field;
            double velocity=scanning?5400:0;
            bool open=printing&&scanning&&permit&&permitVoltage&&contacts[0].IsClosed;
            plant.Tick(dt,drive,sync,isolation,scenario==Scenario.Shake,scenario==Scenario.Lens,latch!=Fault.Lens||scenario==Scenario.None,
                Ready&&lamp,open,aperture,validPosition,aligned,mapped,activeField,velocity*scanClock,velocity,processing,
                scenario==Scenario.Nozzle,elapsed);
            if(referenceRequest&&Math.Abs(plant.X)<2&&Math.Abs(plant.Y)<2&&Stopped)
            {
                referenceRequest=false;referenced=true;home=true;Log("Wafer stage re-referenced with fresh interferometer samples.");
            }
            if(transferClock>0)
            {
                transferClock+=dt;
                if(transferClock>1.7f){transferClock=0;transferred=true;Log("Prepared wafer seated at the scanner.");}
            }
            if(processing!=0)ProcessWafer(dt);
            if(scenario==Scenario.Lens&&latch==Fault.None)
            {
                lensPeak=Mathf.Max(lensPeak,plant.Lens);
                if(plant.Lens>31&&Math.Abs(plant.Focus)>125){lensPassed=true;mapped=false;Trip(Fault.Lens);}
            }
            if(dryRun&&scenario==Scenario.Shake)
            {
                shakePeak=Mathf.Max(shakePeak,plant.Vibration);errorPeak=Math.Max(errorPeak,plant.SyncError);
                if(shakePeak>17&&errorPeak>6){shakePassed=true;Trip(Fault.Vibration);}
            }
            if((printing||dryRun)&&latch==Fault.None)RunScan(dt,activeField);
            bool goal=stage switch
            {
                1=>transferred&&coated&&baked,
                2=>dryPassed&&BaseScan,
                3=>measured&&acceptedBatches>=1,
                4=>nozzlePassed&&shakePassed&&lensPassed&&measured&&acceptedBatches>=2,
                5=>CanCertify&&certify,
                _=>false
            };
            hold=Ready&&latch==Fault.None&&goal?hold+dt:0;
            if(hold>.42f)Award();
            if(plot!=null)plot.Show(plant.Fields,validPosition);
        }

        private void ProcessWafer(float dt)
        {
            workClock+=dt;
            if(processing==1&&workClock>=2.55f)
            {
                processing=0;
                if(plant.Coating is >=335 and <=365&&plant.CoatingSpread<.04f&&plant.Spin>760)
                {coated=true;Log($"Coating verified · {plant.Coating:0.0} nm.");}
                else
                {
                    restrictedFlow=plant.Flow;restrictedThickness=plant.Coating;
                    nozzlePassed|=scenario==Scenario.Nozzle&&restrictedFlow<.50f&&restrictedThickness<330;
                    Trip(Fault.Nozzle);
                }
            }
            else if(processing==2&&workClock>1.6f&&plant.WaferTemperature>55){processing=0;baked=true;Log("Soft bake verified.");}
            else if(processing==3&&plant.BakeEnergy>=1.43f){processing=0;postBaked=true;Log("Post-exposure bake verified.");}
            else if(processing==4&&plant.Developer>.985f)
            {
                processing=0;developed=true;
                foreach(var f in plant.Fields)if(f.State==SparkLevel74Plant.FieldState.Exposed)f.State=SparkLevel74Plant.FieldState.Developed;
                Log("Resist developed. Rinse before drying.");
            }
            else if(processing==5&&plant.Residue<.025f){processing=0;rinsed=true;Log("Rinse verified.");}
            else if(processing==6&&plant.Moisture<.035f){processing=0;dried=true;Log("Dry wafer ready for CD and overlay metrology.");}
        }
        private void RunScan(float dt,int f)
        {
            if(!BaseScan||printing&&(!permitVoltage||!lastPermit)){Trip(Fault.Exposure);return;}
            if(!scanning)
            {
                var target=plant.Fields[f];
                bool settledNow=Math.Abs(plant.X-target.X)<4&&Math.Abs(plant.Y-target.Y)<4&&plant.Speed<12&&plant.SyncError<3&&
                    Math.Abs(plant.Focus)<30&&plant.Vibration<5;
                settled=settledNow?settled+dt:0;
                if(settled<.13f)return;
                scanning=true;scanClock=0;
                if(printing)plant.StartField(field);
                return;
            }
            scanClock+=dt;
            if(printing)plant.IntegrateField(field,dt);
            if(scanClock<SparkLevel74Plant.ScanTime)return;
            scanning=false;scanClock=settled=0;
            if(dryRun)
            {
                dryRun=false;dryPassed=plant.SyncError<3&&Math.Abs(plant.ReticleVelocity-4*plant.Velocity)<8&&plant.Shutter<.025f;
                if(dryPassed)Log("Dry scan verified · actual 4:1 motion, shutter closed.");
                else Trip(Fault.Exposure);
                return;
            }
            if(!plant.EndField(field)){Trip(Fault.Exposure);return;}
            field++;
            if(field==16)
            {printing=false;field=-1;armed=false;Log("16 fields exposed. Post-bake, develop, rinse and dry.");}
        }

        private void Award()
        {
            if(stage>=Gates.Length||!bindings.SetRuntimeComponentBinding(Gates[stage],source))return;
            stage++;hold=0;manager.QueueEvaluation();Log($"Objective {stage}/6 verified.");
        }
        private void Log(string text)=>latestEvent=$"{elapsed:0.0}s · {text}";
        private void StopScan()
        {
            plant.Interrupt(field);printing=dryRun=scanning=armed=false;field=-1;scanClock=settled=0;
            lastPermit=false;contacts[0].SetState(SparkSwitchState.Open);
        }
        private void Trip(Fault cause)
        {
            latch=cause;measuredStop=plant.X;StopScan();transferClock=0;processing=0;dryPassed=false;
            if(cause==Fault.Position)
            {
                validPosition=recalibrated=referenced=referenceRequest=aligned=mapped=home=false;sampleAge=0;
                Log("POSITION FEEDBACK INVALID · shutter closing; stage braking.");
            }
            else Log($"SCANNER SAFETY HOLD · {cause}.");
        }

        public void Power(){power=!power;source.SetOutput(power,default);}
        public void CalPrep(){if(Ready&&Idle){calPrep=true;Log("Spin encoder, flow and thermal sensor calibrated.");}}
        public void Load()
        {
            if(!CanLoad)return;
            plant.NewWafer();loaded=true;coated=baked=transferred=clamped=aligned=mapped=dryPassed=postBaked=developed=rinsed=dried=measured=false;
            Log("Raw wafer loaded into the spin coater.");
        }
        private void StartProcess(int mode){processing=mode;workClock=0;}
        public void Coat(){if(CanCoat){plant.ReworkCoating();StartProcess(1);}}
        public void SoftBake(){if(Ready&&Idle&&coated&&!baked&&latch==Fault.None)StartProcess(2);}
        public void Transfer(){if(Ready&&Idle&&baked&&!transferred&&latch==Fault.None)transferClock=.001f;}
        public void Reticle(){if(Ready&&Idle&&latch==Fault.None){reticle=true;Log("4:1 reticle seated and pattern inspection verified.");}}
        public void Clamp(){if(Ready&&Idle&&transferred&&latch==Fault.None)clamped=true;}
        public void Home(){if(Ready&&Idle&&clamped&&reticle&&latch==Fault.None){home=true;Log("Both stages homing at the scanner datum.");}}
        public void CalPosition(){if(Ready&&Idle&&home&&Stopped&&latch==Fault.None){calPosition=true;sampleAge=0;Log("Encoder and laser interferometer calibrated.");}}
        public void Align(){if(Ready&&Idle&&clamped&&home&&validPosition&&latch==Fault.None){aligned=true;Log("Wafer translation and rotation aligned.");}}
        public void Focus(){if(Ready&&Idle&&aligned&&plant.Lens<27&&latch==Fault.None){mapped=true;Log("Height map acquired. Closed-loop focus enabled.");}}
        public void Isolate(){if(Ready&&Idle&&latch==Fault.None)isolation=true;}
        public void Sync(){if(Ready&&Idle&&home&&calPosition&&latch==Fault.None)sync=true;}
        public void DryScan(){if(CanDry){dryRun=true;scanning=false;scanClock=settled=0;Log("Dry scan · shutter held closed.");}}
        public void Lamp(){if(Ready&&Idle&&latch==Fault.None)lamp=!lamp;}
        public void CalDose(){if(Ready&&Idle&&lamp&&plant.Lamp>.985f&&latch==Fault.None){calDose=true;Log("Photodiode calibrated against the virtual light source.");}}
        public void Aperture(){if(Ready&&Idle&&latch==Fault.None){aperture=aperture>.80f?.67f:aperture<.70f?.74f:.82f;Log($"Numerical aperture {aperture:0.00}.");}}
        public void Print(){if(CanPrint){printing=true;field=0;settled=scanClock=0;Log("Starting 16-field synchronized exposure.");}}
        public void Stop(){StopScan();processing=0;transferClock=0;Log("Scanner stopped.");}
        public void PostBake(){if(Ready&&Idle&&!postBaked&&plant.CountExposed()==16&&latch==Fault.None)StartProcess(3);}
        public void Develop(){if(Ready&&Idle&&postBaked&&!developed&&latch==Fault.None)StartProcess(4);}
        public void Rinse(){if(Ready&&Idle&&developed&&!rinsed&&latch==Fault.None)StartProcess(5);}
        public void Dry(){if(Ready&&Idle&&rinsed&&!dried&&latch==Fault.None)StartProcess(6);}
        public void Metrology(){if(CanMeasure){if(plant.Inspect()){measured=true;acceptedBatches++;Log("METROLOGY PASSED · CD, overlay and pattern quality.");}else Log("Wafer outside certification limits. Review field measurements.");}}
        public void Nozzle(){if(CanNozzle){scenario=Scenario.Nozzle;Log("Nozzle restriction applied. Run a coating cycle.");}}
        public void Shake(){if(CanShake){scenario=Scenario.Shake;shakePeak=0;errorPeak=0;Log("Isolation disturbed. Run a dry scan.");}}
        public void Lens(){if(CanLens){scenario=Scenario.Lens;lensPeak=0;Log("Lens temperature disturbed. Wait for thermal protection.");}}
        public void Restore(){if(Ready&&latch!=Fault.None&&scenario!=Scenario.None){scenario=Scenario.None;Log("Fault corrected. Wait for settled conditions, then reset.");}}
        public void EStop(){emergency.SetState(emergency.IsClosed?SparkSwitchState.Open:SparkSwitchState.Closed);if(emergency.IsOpen&&latch==Fault.None)Trip(Fault.EStop);}
        public void Reset(){if(CanReset){positionReset|=latch==Fault.Position&&positionObserved;latch=Fault.None;Log("Safety reset. Recheck alignment and focus.");}}
        public void Arm(){if(CanArm){armed=true;Log("Remove W102 with Undo Last Wire during exposure.");}}
        public void Recal(){if(CanRecal){recalibrated=true;sampleAge=0;Log("Interferometer recalibrated. Await fresh samples.");}}
        public void Reference(){if(CanReference){referenceRequest=true;Log("Re-referencing wafer stage at controlled speed.");}}
        public void Certify(){if(CanCertify)certify=true;}

        private bool CanNozzle=>stage==4&&CanCoat&&scenario==Scenario.None&&!nozzlePassed;
        private bool CanShake=>stage==4&&CanDry&&scenario==Scenario.None&&nozzlePassed&&!shakePassed;
        private bool CanLens=>stage==4&&Idle&&Ready&&mapped&&scenario==Scenario.None&&shakePassed&&!lensPassed&&latch==Fault.None;
        private bool CanArm=>stage==5&&printing&&scanning&&plant.Shutter>.85f&&!armed&&!positionObserved;

        private string Status()
        {
            if(stage==6)return "CERTIFIED · fresh 16-field wafer meets CD, dose, overlay and focus limits.";
            if(!power)return "Enable 5 V CONTROL and complete wiring.";
            if(latch==Fault.Position)return !Ready?"POSITION FEEDBACK INVALID · reconnect W102.":CanRecal?"Allow braking, then RECAL LASER.":!referenced?"Wait for fresh feedback, then REFERENCE.":"Select RESET SAFETY, then prepare a fresh wafer.";
            if(latch!=Fault.None)return $"SCANNER SAFETY HOLD · {latch} · restore and reset when safe.";
            if(!Ready)return wiring??"Check the 5 V and emergency-stop loop.";
            if(armed)return "Disconnect W102 during exposure.";
            if(processing>0)return $"WAFER PROCESS ACTIVE · {processing switch {1=>"COAT",2=>"SOFT BAKE",3=>"POST BAKE",4=>"DEVELOP",5=>"RINSE",_=>"DRY"}}";
            if(dryRun)return "DRY SCAN · evaluating actual 4:1 stage motion.";
            if(printing)return $"EXPOSURE · FIELD {field+1:00}/16 · {(scanning?"scanning":"settling")}.";
            if(CanCertify)return "Fresh wafer accepted. Select CERTIFY.";
            if(measured)return "Print a new wafer for the next challenge.";
            if(stage==4&&CanNozzle)return "Apply NOZZLE RESTRICT, then COAT.";
            if(stage==4&&CanShake)return "Apply ISOLATION TEST, then DRY SCAN.";
            if(stage==4&&CanLens)return "Apply LENS DRIFT. Restore and recheck focus after reset.";
            if(plant.CountExposed()==16)return !postBaked?"POST BAKE before development.":!developed?"DEVELOP the latent pattern.":!rinsed?"RINSE the developed wafer.":!dried?"DRY before inspection.":"Run METROLOGY.";
            if(!transferred)return "Calibrate prep, load, coat, soft bake, then transfer.";
            if(!dryPassed)return "Load reticle, clamp, home, calibrate, align, focus, isolate, sync and dry scan.";
            return "Commission virtual light and photodiode, then EXPOSE 16.";
        }
        private void Present()
        {
            statusText.text=Status();
            meterText.text=$"RESIST {plant.Coating:0.0} nm   ·   LASER {(validPosition?"VALID":"STALE")}   ·   FOCUS {plant.Focus:0.0} nm   ·   OVERLAY {plant.Overlay:0.0} nm   ·   VIB {plant.Vibration:0.0}";
            detailText.text=$"FIELDS {plant.CountExposed()}/16   ·   STAGE {plant.X:0} nm   ·   LENS {plant.Lens:0.0}°C   ·   NA {plant.Aperture:0.00}   ·   SHUTTER {plant.Shutter:P0}";
            calibrationText.text=$"{latestEvent}   |   FOLLOW {plant.SyncError:0.00} nm   ·   PATTERN {plant.PatternScore:P1}";
            if(controllerDisplay)controllerDisplay.text=validPosition?$"{plant.SyncError:0.0} nm · SYNC":"LASER STALE";
            if(qualityDisplay)qualityDisplay.text=$"{plant.CountExposed()}/16 · {plant.Coating:0} nm";
            powerButton.interactable=stage<6;calPrepButton.interactable=Ready&&Idle&&!calPrep;
            loadButton.interactable=CanLoad;coatButton.interactable=CanCoat;softBakeButton.interactable=Ready&&Idle&&coated&&!baked&&latch==Fault.None;
            transferButton.interactable=Ready&&Idle&&baked&&!transferred&&latch==Fault.None;reticleButton.interactable=Ready&&Idle&&!reticle&&latch==Fault.None;
            clampButton.interactable=Ready&&Idle&&transferred&&!clamped&&latch==Fault.None;homeButton.interactable=Ready&&Idle&&clamped&&reticle&&!home&&latch==Fault.None;
            calPositionButton.interactable=Ready&&Idle&&home&&Stopped&&!calPosition&&latch==Fault.None;
            alignButton.interactable=Ready&&Idle&&clamped&&home&&validPosition&&!aligned&&latch==Fault.None;
            focusButton.interactable=Ready&&Idle&&aligned&&!mapped&&plant.Lens<27&&latch==Fault.None;
            isolateButton.interactable=Ready&&Idle&&!isolation&&latch==Fault.None;syncButton.interactable=Ready&&Idle&&home&&calPosition&&!sync&&latch==Fault.None;
            dryScanButton.interactable=CanDry;lampButton.interactable=Ready&&Idle&&latch==Fault.None;
            calDoseButton.interactable=Ready&&Idle&&lamp&&plant.Lamp>.985f&&!calDose&&latch==Fault.None;
            apertureButton.interactable=Ready&&Idle&&latch==Fault.None;printButton.interactable=CanPrint;stopButton.interactable=!Idle;
            postBakeButton.interactable=Ready&&Idle&&!postBaked&&plant.CountExposed()==16&&latch==Fault.None;
            developButton.interactable=Ready&&Idle&&postBaked&&!developed&&latch==Fault.None;
            rinseButton.interactable=Ready&&Idle&&developed&&!rinsed&&latch==Fault.None;dryButton.interactable=Ready&&Idle&&rinsed&&!dried&&latch==Fault.None;
            metrologyButton.interactable=CanMeasure;nozzleButton.interactable=CanNozzle;shakeButton.interactable=CanShake;lensButton.interactable=CanLens;
            restoreButton.interactable=Ready&&latch!=Fault.None&&scenario!=Scenario.None;eStopButton.interactable=stage<6;
            resetButton.interactable=CanReset;armButton.interactable=CanArm;recalButton.interactable=CanRecal;
            referenceButton.interactable=CanReference;certifyButton.interactable=CanCertify&&!certify;
            rig.Apply(plant,elapsed,transferClock,processing,loaded,transferred,reticle,emergency.IsOpen);
        }
    }
}
