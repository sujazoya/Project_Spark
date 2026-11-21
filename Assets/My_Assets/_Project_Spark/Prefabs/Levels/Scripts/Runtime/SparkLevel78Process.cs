using System;
using System.Collections.Generic;
using ProjectSpark.Circuit;
using ProjectSpark.Visuals;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Cycle=ProjectSpark.Gameplay.SparkLevel78Plant.Cycle;
using Disturbance=ProjectSpark.Gameplay.SparkLevel78Plant.Disturbance;

namespace ProjectSpark.Gameplay
{
    public sealed partial class SparkLevel78Process : MonoBehaviour
    {
        public static readonly string[] Gates={"L78_Wired","L78_Commissioned","L78_Capped","L78_Qualified","L78_Disturbances","L78_Recertified"};
        public static readonly (int a,int b)[] Wires={(0,1),(2,3),(0,4),(2,5),(0,6),(2,7),(0,8),(2,9),(0,10),(2,11),(12,13),(14,15),(16,17),(16,18),(19,20),(21,22),(23,24),(25,26),(27,28),(29,30),(31,32),(33,34),(35,36),(37,38),(39,40),(41,42),(43,44),(45,46),(47,48),(49,50),(51,52),(53,54),(55,56),(57,58),(59,60),(61,62),(63,64),(65,66),(67,68),(69,70),(71,72),(73,74),(75,76),(77,78),(79,80),(81,82),(83,84),(85,86),(87,88),(89,90),(91,92),(93,94),(95,96),(97,98),(99,100),(101,102),(103,104),(105,106),(107,108),(109,110),(111,112),(113,114),(115,116),(117,118),(119,120),(121,122),(123,124),(125,126),(127,128),(129,130),(118,131),(131,132),(132,133),(114,134),(110,135),(74,136),(132,137),(108,138),(135,139),(36,140),(140,141),(133,142),(143,144),(141,145),(145,146),(146,147),(135,148),(149,150),(40,151),(76,152),(74,153),(104,154),(106,155),(110,156),(130,157),(131,158),(136,159),(137,160),(139,161),(145,162),(140,163),(161,164),(165,166),(166,167),(61,168),(83,169),(115,170),(142,171),(167,172),(172,173),(173,174),(164,175),(145,176),(176,177),(46,178),(173,179),(164,180),(172,181)};
        private const int StopInput=166,ControlReturn=3,MeterPower=8,MeterReturn=9,Feedback=82,PermitWire=87;

        public SparkLevelLayout layout;
        public SparkPowerSupply source;
        public SparkSwitch[] contacts;
        public SparkSwitch emergency;
        public SparkTerminal[] pins;
        public SparkLevel78VisualRig rig;
        public SparkLevel78Trend plot;
        public TMP_Text controllerDisplay,qualityDisplay;
        public Text statusText,meterText,detailText,calibrationText;
        public Button powerButton,pumpButton,loadButton,inspectButton,prepButton,calGasButton,calRfButton,calMeterButton,
            capButton,capInspectButton,lowKButton,cureButton,metroButton,stressButton,stopButton,discardButton,
            gasFaultButton,chargeFaultButton,uvFaultButton,restoreButton,eStopButton,resetButton,armButton,verifyButton,certifyButton;

        private enum Fault { None, Power, Wiring, EStop, Gas, Charge, UV, Leakage, Vacuum, Quality }
        private readonly SparkLevel78Plant plant=new();
        private readonly HashSet<(int,int)> wires=new();
        private readonly List<SparkCircuitConnection> edges=new();
        private LevelGamePlayManager manager;
        private SparkLevelLayoutLoader loader;
        private SparkLevelSceneBindings bindings;
        private Cycle cycle;
        private Disturbance disturbance;
        private Fault latch;
        private int topology=-1,stage,acceptedBatches,measurementCount;
        private bool power,rails,safety,meterPower,wiredFeedback,permitVoltage,lastPermit;
        private bool pumps,loaded,inspected,prepared,gasCal,rfCal,meterCal,verified,validLeak;
        private bool capReady,capInspected,lowKReady,cured,metrology,screened,counted,rejected;
        private bool gasPassed,chargePassed,uvPassed,armed,feedbackObserved,feedbackReset,certify;
        private float elapsed,accumulator,hold,transfer,scan,meterClock,gasClock,rfClock,sampleAge,sampleLeak,maxLeak,fieldAtTrip;
        private float gasAtTrip,chargeAtTrip,shutterAtTrip;
        private string wiring,latestEvent="Connect the dielectric cluster and enable 5 V.";

        private bool Ready=>rails&&safety&&wiring==null;
        private bool Idle=>cycle==Cycle.Idle&&transfer<=0&&plant.Field<.025f;
        private bool Active=>cycle!=Cycle.Idle;
        private bool SafeForProcess=>Ready&&Idle&&latch==Fault.None&&loaded&&!rejected&&plant.VacuumReady;
        private bool AwaitingAward=>counted&&(stage==3||stage==4&&gasPassed&&chargePassed&&uvPassed);
        private bool CanLoad=>stage<6&&Ready&&Idle&&plant.VacuumReady&&latch==Fault.None&&(!loaded||counted)&&!AwaitingAward&&gasClock<=0&&rfClock<=0&&meterClock<=0;
        private bool CanInspect=>SafeForProcess&&!inspected;
        private bool CanPrep=>SafeForProcess&&inspected&&!prepared;
        private bool CanCalGas=>Ready&&Idle&&plant.VacuumReady&&latch==Fault.None&&!gasCal&&gasClock<=0;
        private bool CanCalRf=>Ready&&Idle&&plant.VacuumReady&&latch==Fault.None&&!rfCal&&rfClock<=0;
        private bool CanCalMeter=>Ready&&Idle&&meterPower&&wiredFeedback&&meterClock<=0&&
            (latch==Fault.Leakage&&!meterCal||latch==Fault.None&&!meterCal);
        private bool CanCap=>SafeForProcess&&stage>=2&&prepared&&!capReady&&gasCal&&rfCal;
        private bool CanCapInspect=>SafeForProcess&&capReady&&!capInspected;
        private bool CanLowK=>SafeForProcess&&stage>=3&&capInspected&&!lowKReady;
        private bool CanCure=>SafeForProcess&&lowKReady&&!cured;
        private bool CanMetro=>SafeForProcess&&cured&&!metrology;
        private bool CanStress=>SafeForProcess&&metrology&&validLeak&&!screened;
        private bool CanGasFault=>stage==4&&cycle==Cycle.Cap&&plant.CapFlow>.75f&&!gasPassed&&disturbance==Disturbance.None;
        private bool CanChargeFault=>stage==4&&cycle==Cycle.LowK&&plant.Match>.85f&&gasPassed&&!chargePassed&&disturbance==Disturbance.None;
        private bool CanUvFault=>stage==4&&cycle==Cycle.Cure&&plant.Shutter>.75f&&chargePassed&&!uvPassed&&disturbance==Disturbance.None;
        private bool CanVerify=>Ready&&Idle&&latch==Fault.Leakage&&validLeak&&!verified;
        private bool CanReset=>Ready&&Idle&&latch!=Fault.None&&disturbance==Disturbance.None&&
            (latch!=Fault.Leakage||verified)&&plant.FlowBalance>.94f&&plant.Charge<.1f;
        private bool CanArm=>stage==5&&cycle==Cycle.Stress&&plant.Field>.66f&&!armed&&!feedbackObserved;
        private bool CanCertify=>stage==5&&feedbackReset&&counted&&acceptedBatches>=3&&Ready&&latch==Fault.None;

        private void Start()
        {
            manager=FindFirstObjectByType<LevelGamePlayManager>();loader=FindFirstObjectByType<SparkLevelLayoutLoader>();
            if(!manager||!loader||pins.Length!=182||contacts.Length!=1||!rig)
            {
                Debug.LogError("[LEVEL 78] Missing dielectric workstation references.",this);enabled=false;return;
            }
            source.SetOutput(false,default);
            BindEquipmentWiring();
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
            wiredFeedback=Routed(Feedback);
            suggestedWire = -1;
            wiring=wires.Count==Wires.Length?null:"Remove extra wires and check the route chart.";
            for(int i=0;i<Wires.Length;i++)
            {
                if(Routed(i))continue;
                suggestedWire = i;
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
            meterPower=power&&Rail(MeterPower,MeterReturn);
            permitVoltage=power&&Rail(Wires[PermitWire].b,ControlReturn);
            if(stage==0&&Ready)Award();
            if(latch==Fault.None&&(Active||transfer>0||plant.Field>.025f)&&!Ready)
            {
                bool feedbackOnly=rails&&safety&&!wiredFeedback&&wires.Count==Wires.Length-1;
                if(feedbackOnly)
                    for(int i=0;i<Wires.Length;i++)
                        if(i!=Feedback&&!Routed(i)){feedbackOnly=false;break;}
                feedbackObserved|=stage==5&&armed&&cycle==Cycle.Stress&&plant.Field>.6f&&feedbackOnly;
                Trip(feedbackOnly?Fault.Leakage:!rails?Fault.Power:!safety?Fault.EStop:Fault.Wiring);
            }
            accumulator+=Mathf.Min(Time.deltaTime,.18f)*(armed?.32f:CanArm?1.1f:2.15f);
            while(accumulator>=SparkLevel78Plant.Step)
            {
                accumulator-=SparkLevel78Plant.Step;
                Tick(SparkLevel78Plant.Step);
            }
            Present();
            PresentEquipmentWiring();
        }
        private void Tick(float dt)
        {
            elapsed+=dt;
            if(transfer>0)
            {
                transfer+=dt;
                if(transfer>1.35f){transfer=0;loaded=true;Log("Wafer loaded. Let the load-lock equalize before inspection.");}
            }
            TickCalibration(dt);
            bool sensor=meterPower&&wiredFeedback&&meterCal;
            sampleAge=sensor?sampleAge+dt:0;
            validLeak=sampleAge>.32f;
            sampleLeak=validLeak?plant.LeakageSignal:0;
            bool permit=Ready&&latch==Fault.None&&cycle==Cycle.Stress&&validLeak;
            if(permit!=lastPermit){lastPermit=permit;contacts[0].SetState(permit?SparkSwitchState.Closed:SparkSwitchState.Open);}
            bool drive=Ready&&latch==Fault.None&&(cycle==Cycle.Stress?permit&&permitVoltage&&contacts[0].IsClosed:plant.VacuumReady);
            plant.Tick(dt,cycle,rails&&safety&&pumps,drive,transfer<=0||transfer>.32f,disturbance);
            scan=Mathf.Max(0,scan-dt);
            if(latch==Fault.None)
            {
                if(disturbance==Disturbance.Gas&&plant.FlowBalance<.64f){gasAtTrip=plant.FlowBalance;gasPassed=true;Trip(Fault.Gas);}
                else if(disturbance==Disturbance.Charge&&plant.Charge>.34f){chargeAtTrip=plant.Charge;chargePassed=true;Trip(Fault.Charge);}
                else if(disturbance==Disturbance.UV&&plant.Shutter<.43f){shutterAtTrip=plant.Shutter;uvPassed=true;Trip(Fault.UV);}
                else if(Active&&cycle!=Cycle.Stress&&!plant.VacuumReady)Trip(Fault.Vacuum);
                else if(drive)CheckEndpoint();
            }
            if(screened&&Idle&&!counted&&!rejected){counted=true;acceptedBatches++;}
            bool goal=stage switch
            {
                1=>prepared&&inspected&&plant.VacuumReady&&gasCal&&rfCal,
                2=>capInspected&&plant.Pinhole<=.002f,
                3=>counted&&acceptedBatches>=1,
                4=>gasPassed&&chargePassed&&uvPassed&&counted&&acceptedBatches>=2,
                5=>CanCertify&&certify,
                _=>false
            };
            hold=Ready&&latch==Fault.None&&goal?hold+dt:0;
            if(hold>.36f)Award();
            if(plot!=null)plot.Show(plant,validLeak,cured||cycle==Cycle.Cure?2:lowKReady||cycle==Cycle.LowK?1:0);
        }
        private void TickCalibration(float dt)
        {
            bool Finished(ref float clock)
            {
                if(clock<=0)return false;
                clock=Mathf.Max(0,clock-dt);
                return clock==0;
            }
            if(Finished(ref gasClock)){gasCal=plant.VacuumReady&&plant.FlowBalance>.94f;Log(gasCal?"Virtual MFC references calibrated.":"Gas calibration aborted.");}
            if(Finished(ref rfClock)){rfCal=plant.VacuumReady;Log(rfCal?"RF matching reference locked.":"RF calibration aborted.");}
            if(Finished(ref meterClock))
            {
                meterCal=meterPower&&wiredFeedback&&plant.Field<.025f;
                sampleAge=0;
                Log(meterCal?"Electrometer zero established. Waiting for new samples.":"Electrometer zero invalid.");
            }
        }
        private void CheckEndpoint()
        {
            switch(cycle)
            {
                case Cycle.Prepare when plant.Surface>=.999f:
                    cycle=Cycle.Idle;prepared=true;Log("Incoming copper surface ready for capping.");break;
                case Cycle.Cap when plant.MeanCap>=21.5f && disturbance==Disturbance.None:
                    cycle=Cycle.Idle;capReady=true;Log("Cap deposition stopped at target thickness.");break;
                case Cycle.LowK when plant.MeanFilm>=76 && disturbance==Disturbance.None:
                    cycle=Cycle.Idle;lowKReady=true;Log("Low-k thickness target reached.");break;
                case Cycle.Cure when plant.CureProgress>=.995f && plant.Humidity<=.004f && disturbance==Disturbance.None:
                    cycle=Cycle.Idle;cured=true;Log("UV cure and moisture conditioning complete.");break;
                case Cycle.Stress when plant.Field>.63f:
                    measurementCount++;
                    maxLeak=Mathf.Max(maxLeak,sampleLeak/Mathf.Max(plant.Field,.63f));
                    if(plant.Exposure<2.9f||measurementCount<26)return;
                    StopDrive();
                    screened=validLeak&&maxLeak<=.10f&&plant.CouponWear<.16f;
                    rejected=!screened;
                    Log(screened?"Coupon screen passed. Fixture is discharging.":"Coupon screen failed.");
                    break;
            }
        }
        private void StopDrive()
        {
            cycle=Cycle.Idle;armed=false;lastPermit=false;contacts[0].SetState(SparkSwitchState.Open);
        }
        private void Trip(Fault cause)
        {
            latch=cause;fieldAtTrip=plant.Field;
            StopDrive();
            rejected=loaded;transfer=0;meterClock=gasClock=rfClock=0;
            if(cause==Fault.Leakage)
            {
                validLeak=meterCal=verified=false;sampleAge=sampleLeak=0;
                Log("LEAKAGE FEEDBACK INVALID · source inhibited and fixture discharging.");
            }
            else Log($"PROTECTION LATCHED · {cause}.");
        }
        private void Award()
        {
            if(stage>=Gates.Length||!bindings.SetRuntimeComponentBinding(Gates[stage],source))return;
            stage++;hold=0;manager.QueueEvaluation();Log($"Objective {stage}/6 verified.");
        }
        private void Log(string message)=>latestEvent=$"{elapsed:0.0}s · {message}";

        public void Power(){power=!power;source.SetOutput(power,default);}
        public void Pump(){if(Ready&&!pumps){pumps=true;Log("Backing pump accelerating. Turbo and vacuum indications follow.");}}
        public void Load()
        {
            if(!CanLoad)return;
            plant.NewWafer();
            loaded=inspected=prepared=capReady=capInspected=lowKReady=cured=metrology=screened=counted=rejected=false;
            transfer=.001f;measurementCount=0;maxLeak=0;
            Log("Loading a planarized Level 77 copper wafer and fresh monitor coupon.");
        }
        public void Inspect(){if(CanInspect){inspected=true;scan=.82f;Log("Patterned copper surface accepted.");}}
        public void Prep(){if(CanPrep){cycle=Cycle.Prepare;Log("Surface treatment running.");}}
        public void CalGas(){if(CanCalGas)gasClock=.65f;}
        public void CalRf(){if(CanCalRf)rfClock=.76f;}
        public void CalMeter(){if(CanCalMeter)meterClock=.82f;}
        public void Cap(){if(CanCap){cycle=Cycle.Cap;Log("Cap plasma enabled. Monitoring delivery and cap growth.");}}
        public void CapInspect()
        {
            if(!CanCapInspect)return;
            scan=.9f;
            capInspected=plant.Pinhole<=.002f&&plant.MeanCap is >=19f and <=23f&&plant.Diffusion<=.5f;
            if(!capInspected)Trip(Fault.Quality);
            else Log("Cap continuity, pinholes and copper diffusion protection passed.");
        }
        public void LowK(){if(CanLowK){cycle=Cycle.LowK;Log("Low-k chamber running with separate matrix and porogen channels.");}}
        public void Cure(){if(CanCure){cycle=Cycle.Cure;Log("UV chamber running. Monitor shutter, cure and outgassing.");}}
        public void Metro()
        {
            if(!CanMetro)return;
            scan=1.2f;
            metrology=plant.MeanFilm is >=74f and <=80f&&plant.Uniformity<=.03f&&plant.Dielectric<=3&&plant.Humidity<=.01f&&
                plant.Pinhole<=.002f&&plant.Diffusion<=.5f&&plant.Stress<=.13f&&plant.Delay<=2.05f;
            if(!metrology)Trip(Fault.Quality);
            else Log("Film thickness, uniformity, effective k, moisture, stress and delay passed.");
        }
        public void Stress(){if(CanStress){cycle=Cycle.Stress;Log("Protected coupon stress started.");}}
        public void Stop()
        {
            if(!Active)return;
            StopDrive();rejected=true;Log("Cycle stopped. Discard the interrupted wafer after rundown.");
        }
        public void Discard()
        {
            if(Idle&&latch==Fault.None&&loaded&&!counted){loaded=false;rejected=false;Log("Interrupted wafer discarded.");}
        }
        public void GasFault(){if(CanGasFault){disturbance=Disturbance.Gas;Log("Cap delivery asymmetry injected.");}}
        public void ChargeFault(){if(CanChargeFault){disturbance=Disturbance.Charge;Log("RF disturbance destabilizes match and charge.");}}
        public void UvFault(){if(CanUvFault){disturbance=Disturbance.UV;Log("Cure shutter travel restricted.");}}
        public void Restore(){if(Ready&&latch!=Fault.None&&disturbance!=Disturbance.None){disturbance=Disturbance.None;Log("Fault corrected. Wait for the safe reset window.");}}
        public void EStop()
        {
            emergency.SetState(emergency.IsClosed?SparkSwitchState.Open:SparkSwitchState.Closed);
            if(emergency.IsOpen&&latch==Fault.None)Trip(Fault.EStop);
        }
        public void Reset()
        {
            if(!CanReset)return;
            feedbackReset|=latch==Fault.Leakage&&feedbackObserved;
            latch=Fault.None;Log("Protection reset. Prepare a fresh wafer.");
        }
        public void Arm(){if(CanArm){armed=true;Log("Open W083 during the live coupon test.");}}
        public void Verify(){if(CanVerify){verified=true;Log("Fresh zero-field electrometer samples verified.");}}
        public void Certify(){if(CanCertify)certify=true;}

        private string Status()
        {
            if(stage==6)return "6/6 · CERTIFIED · cap, low-k film and monitor coupon qualified.";
            if(!power)return "Enable 5 V CONTROL and complete wiring.";
            if(latch==Fault.Leakage)
            {
                if(!Ready)return "LEAKAGE FEEDBACK INVALID · reconnect W083.";
                if(!Idle)return "Protected fixture is discharging.";
                if(!meterCal)return "Select CAL ELECTROMETER at zero fixture field.";
                return verified?"Select RESET SAFETY.":"Select VERIFY ELECTROMETER when fresh samples arrive.";
            }
            if(latch!=Fault.None)return $"PROTECTION LATCHED · {latch} · restore and reset.";
            if(!Ready)return wiring??"Check solved rails and the emergency isolation loop.";
            if(armed)return "Slow-motion stress test · select OPEN W083 to disconnect the live cable.";
            if(CanArm)return "Select ARM W083 TEST during the live coupon screen.";
            if(CanGasFault)return "Select CAP FLOW FAULT to test delivery asymmetry.";
            if(CanChargeFault)return "Select RF CHARGE FAULT to test plasma charging.";
            if(CanUvFault)return "Select UV SHUTTER FAULT to test cure-dose loss.";
            if(Active)return $"RUNNING · {cycle} · tracking process response.";
            if(transfer>0)return "Load-lock and robot are transferring the wafer.";
            if(CanCertify)return "Fresh wafer and coupon qualified · select CERTIFY.";
            if(rejected)return "Discard the interrupted wafer and load a fresh one.";
            if(!pumps||!plant.VacuumReady)return "Start vacuum and wait for the backing and turbo stages.";
            if(!loaded||counted)return "Load the next wafer.";
            if(!inspected)return "Inspect the incoming copper wafer.";
            if(!prepared||!gasCal||!rfCal)return "Prepare copper and calibrate gas and RF.";
            if(!capInspected)return capReady?"Check cap continuity.":"Deposit the cap.";
            if(!lowKReady)return "Deposit low-k insulation.";
            if(!cured)return "Run UV cure.";
            if(!metrology)return "Run film metrology.";
            if(!meterCal)return "Calibrate the electrometer, then screen the coupon.";
            return "Screen the coupon and verify discharge.";
        }
        private void Present()
        {
            statusText.text=Status();
            meterText.text=$"VAC {plant.Pressure:0.000}   ·   CAP {plant.MeanCap:0.0} nm   ·   FILM {plant.MeanFilm:0.0} nm   ·   CURE {plant.CureProgress:P0}";
            detailText.text=$"HOLES {plant.Pinhole:P2}   ·   k {plant.Dielectric:0.00}   ·   H₂O {plant.Humidity:P2}   ·   FIELD {plant.Field:0.000}   ·   LEAK {(validLeak?sampleLeak.ToString("0.000"):"STALE")}";
            calibrationText.text=$"{latestEvent}  |  CHG {plant.Charge:0.00} · SHUTTER {plant.Shutter:P0}";
            if(controllerDisplay)controllerDisplay.text=validLeak?$"LEAK {sampleLeak:0.000}":"LEAKAGE STALE";
            if(qualityDisplay)qualityDisplay.text=$"{plant.MeanCap:0.0} CAP · {plant.MeanFilm:0.0} FILM";
            powerButton.interactable=stage<6;pumpButton.interactable=Ready&&!pumps;loadButton.interactable=CanLoad;
            inspectButton.interactable=CanInspect;prepButton.interactable=CanPrep;calGasButton.interactable=CanCalGas;
            calRfButton.interactable=CanCalRf;calMeterButton.interactable=CanCalMeter;capButton.interactable=CanCap;
            capInspectButton.interactable=CanCapInspect;lowKButton.interactable=CanLowK;cureButton.interactable=CanCure;
            metroButton.interactable=CanMetro;stressButton.interactable=CanStress;stopButton.interactable=Active;
            discardButton.interactable=Idle&&latch==Fault.None&&loaded&&!counted;gasFaultButton.interactable=CanGasFault;
            chargeFaultButton.interactable=CanChargeFault;uvFaultButton.interactable=CanUvFault;
            restoreButton.interactable=Ready&&latch!=Fault.None&&disturbance!=Disturbance.None;eStopButton.interactable=stage<6;
            resetButton.interactable=CanReset;armButton.interactable=CanArm;verifyButton.interactable=CanVerify;
            certifyButton.interactable=CanCertify&&!certify;
            rig.Apply(plant,cycle,transfer,scan,validLeak,lastPermit,latch!=Fault.None,emergency.IsOpen);
        }
    }
}


