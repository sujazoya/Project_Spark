using System;
using System.Collections.Generic;
using ProjectSpark.Circuit;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.Gameplay
{
    public sealed class SparkLevel70Process : MonoBehaviour
    {
        public static readonly string[] Gates={"L70_Wired","L70_Commission","L70_Climate","L70_Cleanliness","L70_Breach","L70_Reconnected"};
        public static readonly (int a,int b)[] Wires={(0,1),(2,3),(0,4),(2,5),(0,6),(2,7),(0,8),(2,9),(10,11),(12,13),(14,15),(16,17),(18,19),(20,21),(22,23),(24,25),(26,27),(28,29),(30,31),(32,33),(34,35),(36,37),(38,39),(40,41),(42,43),(44,45),(46,47),(48,49),(50,51),(48,52),(0,53),(2,54),(55,56),(57,58),(0,59),(2,60),(61,62),(63,64),(0,65),(2,66),(67,68),(69,70),(0,71),(2,72),(73,74),(75,76),(0,77),(2,78),(79,80),(81,82),(83,84),(85,86),(87,88),(89,90),(91,92),(93,94),(95,96),(97,98),(99,100),(101,102),(103,104),(105,106),(50,107),(64,108),(50,109),(45,110),(58,111),(112,113),(33,114),(115,116),(11,117),(74,118),(22,119),(82,120),(55,121),(12,122),(38,123),(42,124),(96,125),(74,126),(84,127),(85,128),(33,129),(70,130),(114,131),(132,133)};
        public static readonly int[] RailPairs={0,2,4,6,30,34,38,42,46};
        private const int StopInput=113, SafeReturn=3, PressurePlus=53, PressureReturn=54, FeedbackWire=85, RobotWire=69;
        public SparkLevelLayout layout;
        public SparkPowerSupply source;
        public SparkSwitch[] contacts;
        public SparkSwitch emergency;
        public SparkTerminal[] pins;
        public SparkLevel70VisualRig rig;
        public SparkLevel70Trend plot;
        public TMP_Text controllerDisplay,qualityDisplay;
        public Text statusText,meterText,detailText,calibrationText;
        public Button powerButton,calPressureButton,calAirButton,calParticlesButton,calTempButton,calHumidityButton,
            supplyButton,returnFanButton,exhaustButton,ffuAButton,ffuBButton,setRoomButton,setLockButton,tunePressureButton,
            tuneAirButton,setTempButton,setRhButton,environmentButton,waferButton,doorButton,loadingButton,diagnoseButton,
            leakButton,serviceButton,isolateButton,replaceButton,verifyButton,resetButton,armButton,recalButton,certifyButton,eStopButton;

        private enum Fault { None, Power, Stop, Wiring, Contamination, PressureFeedback }
        private enum Maintenance { Normal, Requested, Isolating, Isolated, Replaced, Verifying, Verified }
        private readonly SparkLevel70AirPlant plant=new();
        private readonly SparkLevel70AirPlant.Controller pressure=new(),air=new(),lockPressure=new();
        private readonly List<SparkCircuitConnection> edges=new();
        private readonly HashSet<(int,int)> wires=new();
        private readonly Queue<string> events=new();
        private static readonly string[] Gains={"LOW","TUNED","HIGH"};
        private LevelGamePlayManager manager;
        private SparkLevelLayoutLoader loader;
        private SparkLevelSceneBindings bindings;
        private int topology=-1,stage,pressureGain,airGain,validSamples,batches,freshBatches;
        private bool power,rails,safeStop,pressurePower,wiredPressure,robotPath;
        private bool pressureCal,airCal,particleCal,tempCal,rhCal,pressureValid,pressureRecalibrated;
        private bool supply,returnFan,exhaust,ffuA,ffuB,auto,loading,loadDiagnosed,sealTest,breachObserved,breachReset;
        private bool doorPassed,freshDoor,armed,pressureLossObserved,pressureReset,certify,previousRobot;
        private float elapsed,accumulator,held,pressureAge,particleClock,particleRoomIntegral,particleLockIntegral;
        private float measuredRoom,measuredLock,measuredFlow,measuredRoomParticles,measuredLockParticles,measuredDrop;
        private float targetRoom=15,targetLock=8,targetTemp=22,targetRh=.45f,requestedAir;
        private float doorClock,doorStartLock,doorMinimum,doorStartParticles,doorMaxParticles,loadBaseDrop,sealBaseParticles;
        private float waferClock,waferExposure,lastExposure,verifyClock,robotSafeDwell;
        private string wiring,latestEvent="Build the cleanroom network and enable 5 V control.";
        private Fault latch;
        private Maintenance service;

        private bool Ready=>rails&&safeStop&&wiring==null;
        private bool Calibrated=>pressureCal&&airCal&&particleCal&&tempCal&&rhCal;
        private bool AllFans=>supply&&returnFan&&exhaust&&ffuA&&ffuB;
        private bool CanAuto=>Ready&&Calibrated&&AllFans&&pressureValid&&latch==Fault.None;
        private bool PhysicalDoorClosed=>plant.InnerDoor<.025f&&plant.OuterDoor<.025f;
        private bool Circulating=>plant.Fans[0].Rpm>650&&plant.Fans[1].Rpm>600&&plant.Fans[2].Rpm>500&&
            plant.SupplyRoom>.65f&&plant.SupplyLock>.10f&&plant.Return>.45f&&plant.Velocity>.30f;
        private bool Climate=>Ready&&pressureValid&&Calibrated&&auto&&Circulating&&
            Mathf.Abs(measuredRoom-targetRoom)<1.4f&&Mathf.Abs(measuredLock-targetLock)<1.35f&&plant.LeakPressure>3.5f&&
            Mathf.Abs(plant.Room.Temperature-targetTemp)<.88f&&Mathf.Abs(plant.Room.RelativeHumidity-targetRh)<.046f;
        private bool Clean=>validSamples>=3&&measuredRoomParticles<3000&&measuredLockParticles<8200&&plant.Room.Particles<3200;
        private bool RobotConditions=>Climate&&Clean&&PhysicalDoorClosed&&doorClock<=0&&latch==Fault.None&&
            !plant.Filters[0].SealFailed&&(service==Maintenance.Normal||service==Maintenance.Verified);
        private bool RobotReady=>RobotConditions&&robotSafeDwell>3.7f;
        private bool NominalTargets=>Mathf.Abs(targetRoom-15)<.01f&&Mathf.Abs(targetLock-8)<.01f&&
            Mathf.Abs(targetTemp-22)<.01f&&Mathf.Abs(targetRh-.45f)<.001f;

        private void Start()
        {
            manager=FindFirstObjectByType<LevelGamePlayManager>();
            loader=FindFirstObjectByType<SparkLevelLayoutLoader>();
            if(!manager||!loader||pins.Length!=134||contacts.Length!=1||!rig)
            {
                Debug.LogError("[LEVEL 70] Missing campaign bindings, cleanroom terminals or motion rig.",this);
                enabled=false;
                return;
            }
            source.SetOutput(false,default);
        }

        private static (int,int) Pair(int a,int b)=>(Math.Min(a,b),Math.Max(a,b));
        private bool Routed(int i)=>wires.Contains(Pair(Wires[i].a,Wires[i].b));
        private bool OnlyFeedbackMissing()
        {
            if(wiredPressure||wires.Count!=Wires.Length-1) return false;
            for(int i=0;i<FeedbackWire;i++) if(!Routed(i)) return false;
            return true;
        }
        private void ReadTopology(SparkCircuitSystem circuit)
        {
            edges.Clear();wires.Clear();
            circuit.CopyConnections(edges);
            foreach(var edge in edges)
            {
                if(edge.Kind!=SparkConnectionKind.Wire) continue;
                int a=Array.IndexOf(pins,edge.A),b=Array.IndexOf(pins,edge.B);
                if(a>=0||b>=0) wires.Add(Pair(a,b));
            }
            wiredPressure=Routed(FeedbackWire);
            wiring=wires.Count==Wires.Length?null:"Remove unexpected wiring or complete the listed connections.";
            for(int i=0;i<Wires.Length;i++)
                if(!Routed(i))
                {
                    wiring=$"Connect W{i+1:00} · {pins[Wires[i].a].name} → {pins[Wires[i].b].name}.";
                    break;
                }
        }
        private void Update()
        {
            if(loader.ActiveLayout!=layout||loader.ActiveBindings==null) return;
            var circuit=manager.CircuitSystem;
            var solver=manager.ElectricalSolver;
            if(!circuit||!solver) return;
            if(bindings!=loader.ActiveBindings){bindings=loader.ActiveBindings;topology=-1;}
            if(topology!=circuit.TopologyVersion){topology=circuit.TopologyVersion;ReadTopology(circuit);}
            var result=solver.LatestSolveResult;
            bool Rail(int a,int b)=>result!=null&&result.TryGetVoltage(pins[a],pins[b],out float v)&&v is >=4.7f and <=5.3f;
            rails=power&&source.IsOutputActive&&!source.IsOverloaded;
            foreach(int wire in RailPairs) if(rails) rails=Rail(Wires[wire].b,Wires[wire+1].b);
            safeStop=emergency.IsClosed&&Rail(StopInput,SafeReturn);
            pressurePower=power&&Rail(PressurePlus,PressureReturn);
            robotPath=power&&Rail(Wires[RobotWire].b,SafeReturn);
            if(stage==0&&Ready) Award();
            if(latch==Fault.None&&(AllFans||armed)&&!Ready)
            {
                bool lost=rails&&safeStop&&OnlyFeedbackMissing();
                pressureLossObserved|=stage==5&&armed&&lost;
                Trip(lost?Fault.PressureFeedback:!rails?Fault.Power:!safeStop?Fault.Stop:Fault.Wiring);
            }
            accumulator+=Mathf.Min(Time.deltaTime,.20f)*2.5f;
            const float dt=SparkLevel70AirPlant.Step;
            while(accumulator>=dt){accumulator-=dt;Tick(dt);}
            Present();
        }

        private void Tick(float dt)
        {
            elapsed+=dt;
            bool available=pressureCal&&pressurePower&&wiredPressure&&(latch!=Fault.PressureFeedback||pressureRecalibrated);
            pressureAge=available?pressureAge+dt:0;
            pressureValid=pressureAge>.72f;
            float filter=dt/(.21f+dt);
            if(pressureValid) measuredRoom=Mathf.Lerp(measuredRoom,plant.PressureRoom,filter);
            measuredLock=Mathf.Lerp(measuredLock,plant.PressureLock,filter);
            measuredFlow=Mathf.Lerp(measuredFlow,plant.SupplyRoom,filter);
            measuredDrop=Mathf.Lerp(measuredDrop,plant.Filters[0].Drop,filter);
            CountParticles(dt);

            if(sealTest&&latch==Fault.None&&plant.Filters[0].SealFailed&&
                measuredRoomParticles>Mathf.Max(3600,sealBaseParticles+2400))
            {
                breachObserved=true;
                Trip(Fault.Contamination);
            }
            bool stateAllowsAir=rails&&safeStop&&(Ready||latch==Fault.PressureFeedback&&OnlyFeedbackMissing());
            bool controlled=stateAllowsAir&&auto&&pressureValid&&latch is Fault.None or Fault.Contamination;
            float sup=0,ret=0,exh=0,roomDamper=0,lockDamper=0,relief=0;
            if(stateAllowsAir)
            {
                sup=supply?.78f:0;ret=returnFan?.77f:0;exh=exhaust?.55f:0;
                roomDamper=.92f;lockDamper=.51f;
                if(controlled)
                {
                    requestedAir=pressure.Step(targetRoom-measuredRoom,pressureGain==1?.022f:pressureGain==2?.063f:.006f,
                        pressureGain==1?.028f:pressureGain==2?.055f:.009f,.0012f,.99f,.65f,1.30f,dt);
                    sup=air.Step(requestedAir-measuredFlow,airGain==1?.48f:airGain==2?1.1f:.20f,
                        airGain==1?.62f:airGain==2?1.2f:.14f,.007f,.78f,.21f,1,dt);
                    lockDamper=lockPressure.Step(targetLock-measuredLock,.025f,.035f,.001f,.51f,.12f,.96f,dt);
                    roomDamper=Mathf.Clamp(.90f+.13f*(requestedAir-.99f),.65f,.99f);
                    relief=Mathf.InverseLerp(23f,33f,measuredRoom)*.65f;
                }
                else if(latch==Fault.PressureFeedback)
                {
                    sup=supply?.54f:0;ret=returnFan?.47f:0;exh=exhaust?.43f:0;
                    roomDamper=.82f;lockDamper=.47f;
                }
            }
            bool outer=doorClock is >0 and <4.5f&&latch==Fault.None;
            bool shower=doorClock is >=4.5f and <12.0f&&latch==Fault.None;
            bool inner=doorClock is >12.0f and <12.85f&&plant.OuterDoor<.028f&&
                plant.PressureLock>6&&measuredLockParticles<9400&&plant.LeakPressure>3.5f&&latch==Fault.None;
            bool isolated=service is Maintenance.Isolating or Maintenance.Isolated or Maintenance.Replaced;
            if(isolated&&stateAllowsAir){ret=Mathf.Min(ret,.42f);roomDamper=.96f;}
            plant.Tick(dt,new Vector3(sup,ret,exh),stateAllowsAir&&ffuA,stateAllowsAir&&ffuB,
                .265f,roomDamper,lockDamper,relief,outer,inner,shower,loading,isolated,controlled,targetTemp,targetRh);
            if(!controlled) requestedAir=0;
            ObserveDoor(dt);
            ObserveService(dt);
            ObserveRobot(dt);
            CheckObjectives(dt);
            if(plot!=null) plot.Sample(dt,pressureValid?measuredRoom:0,measuredLock,measuredRoomParticles,plant.Room.RelativeHumidity);
        }

        private void CountParticles(float dt)
        {
            if(!particleCal||!rails||!airCal)
            {
                particleClock=particleRoomIntegral=particleLockIntegral=0;validSamples=0;
                return;
            }
            particleRoomIntegral+=plant.Room.Particles*dt;
            particleLockIntegral+=plant.Lock.Particles*dt;
            particleClock+=dt;
            if(particleClock<1.24f) return;
            float sampleVolume=.012f*particleClock;
            measuredRoomParticles=Mathf.Round(.012f*particleRoomIntegral)/sampleVolume;
            measuredLockParticles=Mathf.Round(.012f*particleLockIntegral)/sampleVolume;
            particleClock=particleRoomIntegral=particleLockIntegral=0;
            validSamples=Math.Min(validSamples+1,8);
        }
        private void ObserveDoor(float dt)
        {
            if(doorClock<=0) return;
            doorClock+=dt;
            doorMinimum=Mathf.Min(doorMinimum,plant.PressureLock);
            doorMaxParticles=Mathf.Max(doorMaxParticles,plant.Lock.Particles);
            if(doorClock<15||!Climate&&doorClock<47) return;
            bool passed=doorStartLock-doorMinimum>3.0f&&doorMaxParticles-doorStartParticles>650f&&Climate&&PhysicalDoorClosed&&!plant.HadDoorConflict;
            if(stage==4) doorPassed|=passed;
            if(stage==5&&pressureReset) freshDoor|=passed;
            doorClock=0;
            Log(passed?"AIRLOCK VERIFIED · pressure and particle exchange observed, then recovered.":"Wait for stable pressure and retry the airlock test.");
        }
        private void ObserveService(float dt)
        {
            if(service==Maintenance.Isolating&&plant.Filters[0].Isolation>.96f&&plant.Fans[3].Rpm<35&&plant.Filters[0].Flow<.028f)
            {
                service=Maintenance.Isolated;Log("FFU A isolated and stopped. HEPA A may be replaced.");
            }
            if(service!=Maintenance.Verifying) return;
            bool verified=!plant.Filters[0].SealFailed&&plant.Filters[0].Loading<.06f&&plant.Filters[0].Flow>.25f&&
                measuredDrop>20&&measuredDrop<loadBaseDrop+45;
            verifyClock=verified?verifyClock+dt:0;
            if(verifyClock>2.0f){service=Maintenance.Verified;Log("HEPA A seal verified in flow. Purge until readings recover.");}
        }
        private void ObserveRobot(float dt)
        {
            robotSafeDwell=RobotConditions?robotSafeDwell+dt:0f;
            bool permit=RobotReady;
            if(permit!=previousRobot)
            {
                previousRobot=permit;
                contacts[0].SetState(permit?SparkSwitchState.Closed:SparkSwitchState.Open);
            }
            if(waferClock<=0) return;
            if(!permit||!robotPath||!contacts[0].IsClosed)
            {
                waferClock=waferExposure=0;
                Log("Wafer motion stopped by safety permit.");
                return;
            }
            waferClock+=dt;
            waferExposure+=plant.Room.Particles*dt;
            if(waferClock<3.5f) return;
            lastExposure=waferExposure/Mathf.Max(.01f,waferClock);
            if(lastExposure<3000f)
            {
                if(stage==3) batches++;
                if(stage==5&&pressureReset) freshBatches++;
                Log($"Wafer inspected and accepted · mean exposure {lastExposure:0} particles/m³.");
            }
            else Log("Wafer rejected by measured exposure.");
            waferClock=waferExposure=0;
        }
        private void CheckObjectives(float dt)
        {
            bool goal=stage switch
            {
                1=>Ready&&Calibrated&&AllFans&&pressureValid&&Circulating&&validSamples>=2&&plant.Filters[0].Drop>25&&plant.Filters[1].Drop>25,
                2=>Climate&&NominalTargets&&pressureGain==1&&airGain==1&&latch==Fault.None,
                3=>RobotReady&&batches>=2&&plant.Room.Particles<3000,
                4=>doorPassed&&loadDiagnosed&&breachObserved&&breachReset&&service==Maintenance.Verified&&RobotReady,
                5=>pressureLossObserved&&pressureReset&&freshDoor&&freshBatches>0&&certify&&RobotReady&&plant.LastMassResidual<.001f,
                _=>false
            };
            held=goal?held+dt:0;
            if(held>(stage==2?4.3f:stage==3?2.0f:stage==5?3f:2.4f)) Award();
        }
        private void ResetControllers(){pressure.Reset();air.Reset();lockPressure.Reset();}
        private void Trip(Fault cause)
        {
            latch=cause;
            contacts[0].SetState(SparkSwitchState.Open);
            previousRobot=false;robotSafeDwell=0;held=0;waferClock=waferExposure=0;doorClock=0;
            if(cause!=Fault.Contamination){auto=false;ResetControllers();}
            if(cause==Fault.PressureFeedback){pressureValid=false;pressureRecalibrated=false;pressureAge=0;}
            Log(cause==Fault.PressureFeedback?"PRESSURE FEEDBACK INVALID · robot and doors held; fallback ventilation running.":
                cause==Fault.Contamination?"HEPA A BYPASS · particle rise confirmed and wafer transfer inhibited.":$"SAFETY LATCH · {cause}.");
        }
        private void Log(string text)
        {
            latestEvent=$"{elapsed:0.0}s · {text}";
            events.Enqueue(latestEvent);
            if(events.Count>8)events.Dequeue();
        }
        private void Award()
        {
            if(stage>=Gates.Length||!bindings.SetRuntimeComponentBinding(Gates[stage],source)) return;
            stage++;held=0;manager.QueueEvaluation();Log($"OBJECTIVE {stage}/6 verified.");
        }

        public void Power(){power=!power;source.SetOutput(power,default);}
        public void CalPressure(){if(Ready&&latch==Fault.None){pressureCal=true;pressureAge=0;Log("Pressure references calibrated.");}}
        public void CalAir(){if(Ready&&latch==Fault.None){airCal=true;Log("Flow and velocity calibrated.");}}
        public void CalParticles(){if(Ready&&latch==Fault.None){particleCal=true;validSamples=0;Log("Particle counters calibrated.");}}
        public void CalTemp(){if(Ready&&latch==Fault.None){tempCal=true;Log("Temperature and dew-point channel calibrated.");}}
        public void CalHumidity(){if(Ready&&latch==Fault.None){rhCal=true;Log("Humidity channel calibrated.");}}
        public void Supply(){if(Ready&&latch==Fault.None) supply=true;}
        public void ReturnFan(){if(Ready&&latch==Fault.None) returnFan=true;}
        public void Exhaust(){if(Ready&&latch==Fault.None) exhaust=true;}
        public void FfuA(){if(Ready&&latch==Fault.None) ffuA=true;}
        public void FfuB(){if(Ready&&latch==Fault.None) ffuB=true;}
        public void SetRoom(){if(stage==2) targetRoom=targetRoom>=17?13:targetRoom+2;}
        public void SetLock(){if(stage==2) targetLock=targetLock>=10?6:targetLock+2;}
        public void TunePressure(){if(stage==2){pressureGain=(pressureGain+1)%3;pressure.Reset();}}
        public void TuneAir(){if(stage==2){airGain=(airGain+1)%3;air.Reset();}}
        public void SetTemp(){if(stage==2) targetTemp=targetTemp>=23?21:targetTemp+1;}
        public void SetRh(){if(stage==2) targetRh=targetRh>=.49f?.40f:targetRh+.05f;}
        public void Environment(){if(CanAuto){auto=!auto;ResetControllers();}}
        public void Wafer(){if(CanWafer){waferClock=.001f;waferExposure=0;Log("Wafer transfer started; integrating exposure.");}}
        public void Door()
        {
            if(!CanDoor) return;
            doorClock=.01f;doorMinimum=doorStartLock=plant.PressureLock;doorMaxParticles=doorStartParticles=plant.Lock.Particles;
            Log("Outer airlock door opening with inner door locked.");
        }
        public void Loading()
        {
            if(!CanLoad) return;
            loadBaseDrop=measuredDrop;loading=true;
            Log("Particulate loading applied to HEPA A.");
        }
        public void Diagnose()
        {
            if(!CanDiagnose) return;
            loading=false;loadDiagnosed=true;
            Log("HEPA A loading confirmed from increased measured pressure drop.");
        }
        public void Leak()
        {
            if(!CanLeak) return;
            sealBaseParticles=measuredRoomParticles;sealTest=true;
            plant.Filters[0].SealFailed=true;
            Log("HEPA A seal defect activated. Monitor actual particle transport.");
        }
        public void Service(){if(CanService){service=Maintenance.Requested;Log("Filter service requested. Isolate branch A.");}}
        public void Isolate(){if(service==Maintenance.Requested){service=Maintenance.Isolating;Log("Closing branch A and stopping FFU A.");}}
        public void Replace()
        {
            if(service!=Maintenance.Isolated) return;
            plant.Filters[0].Replace();
            service=Maintenance.Replaced;
            Log("New HEPA A seated. Verify its seal under flow.");
        }
        public void Verify(){if(service==Maintenance.Replaced){verifyClock=0;service=Maintenance.Verifying;Log("Reopening A for seal test.");}}
        public void Reset()
        {
            if(!CanReset) return;
            breachReset|=latch==Fault.Contamination&&breachObserved;
            pressureReset|=latch==Fault.PressureFeedback&&pressureLossObserved;
            armed=false;latch=Fault.None;
            Log("Fault reset. Restore automatic control if needed.");
        }
        public void Arm(){if(CanArm){armed=true;Log("Use Undo Last Wire to remove pressure feedback W86.");}}
        public void Recal()
        {
            if(!CanRecal) return;
            pressureRecalibrated=true;pressureAge=0;
            Log("Main differential-pressure channel recalibrated. Await fresh samples.");
        }
        public void Certify(){if(CanCertify) certify=true;}
        public void EStop()
        {
            emergency.SetState(emergency.IsClosed?SparkSwitchState.Open:SparkSwitchState.Closed);
            if(emergency.IsOpen&&latch==Fault.None) Trip(Fault.Stop);
        }

        private bool CanWafer=>RobotReady&&!armed&&robotPath&&waferClock<=0&&(stage==3&&batches<2||stage==5&&pressureReset&&freshBatches==0);
        private bool CanDoor=>RobotReady&&waferClock<=0&&doorClock<=0&&(stage==4&&!doorPassed||stage==5&&pressureReset&&!freshDoor);
        private bool CanLoad=>stage==4&&doorPassed&&Climate&&!loading&&!loadDiagnosed;
        private bool CanDiagnose=>stage==4&&loading&&measuredDrop>loadBaseDrop+28&&plant.Filters[0].Loading>.23f;
        private bool CanLeak=>stage==4&&loadDiagnosed&&Climate&&!sealTest;
        private bool CanService=>latch==Fault.Contamination&&service==Maintenance.Normal;
        private bool CanArm=>stage==5&&RobotReady&&!pressureLossObserved&&!armed;
        private bool CanRecal=>latch==Fault.PressureFeedback&&Ready&&!pressureRecalibrated;
        private bool CanReset=>latch!=Fault.None&&Ready&&pressureValid&&PhysicalDoorClosed&&
            (latch!=Fault.Contamination||service==Maintenance.Verified&&Clean);
        private bool CanCertify=>stage==5&&RobotReady&&pressureReset&&freshDoor&&freshBatches>0&&!certify;

        private string Status()
        {
            if(stage==6) return "CERTIFIED · stable pressure, fresh wafer inspection and airlock recovery.";
            if(!power) return "Enable 5 V CONTROL and build the environmental network.";
            if(latch==Fault.PressureFeedback)
                return !Ready?"PRESSURE FEEDBACK INVALID · reconnect W86.":CanRecal?"Recalibrate main differential pressure.":"Await fresh pressure feedback, then RESET SAFETY.";
            if(latch==Fault.Contamination)
                return service switch
                {
                    Maintenance.Normal=>"HEPA A BYPASS · request maintenance. Wafer transfer held.",
                    Maintenance.Requested=>"Isolate HEPA branch A.",
                    Maintenance.Isolating=>"Wait for FFU A to stop and branch flow to fall.",
                    Maintenance.Isolated=>"Branch A isolated. Replace HEPA A.",
                    Maintenance.Replaced=>"New filter seated. Select VERIFY SEAL.",
                    Maintenance.Verifying=>"Verifying seal and pressure drop.",
                    _=>"Filter verified. Wait for clean samples, then RESET SAFETY."
                };
            if(latch!=Fault.None) return $"SAFETY LATCH · {latch} · verify and reset.";
            if(!Ready) return wiring??"Check 5 V control and the emergency stop loop.";
            if(!Calibrated) return "Calibrate pressure, airflow, particle, temperature and humidity channels.";
            if(!AllFans) return "Start supply, return and exhaust blowers. Enable both FFUs.";
            if(armed) return "Disconnect W86 · cleanroom DP output → cascade feedback input.";
            if(doorClock>0) return doorClock<4.5f?"OUTER DOOR TEST · measure the pressure drop and particle exchange.":"Airlock purge underway. Verify pressure and particle recovery.";
            if(waferClock>0) return "WAFER TRANSFER · integrating exposure for inspection.";
            return stage switch
            {
                1=>"Commissioning · verify airflow, filter pressure drop and measured velocity.",
                2=>"Tune PRESSURE and AIRFLOW gains, then enable AUTO ENV at nominal setpoints.",
                3=>Clean?$"Start wafer test {batches+1}/2.": "Wait for three valid particle samples within limits.",
                4=>!doorPassed?"Run AIRLOCK TEST.":!loadDiagnosed?loading?"Confirm loading when HEPA A pressure drop rises.":"Run FILTER LOAD.":!sealTest?"Run HEPA LEAK.":"Observe particle bypass and inhibit wafer transfer.",
                5=>pressureReset?!auto?"Re-enable AUTO ENV.":freshDoor?freshBatches>0?"Fresh evidence accepted. CERTIFY.":"Run a fresh wafer test.":"Repeat AIRLOCK TEST after recovery.":"Arm W86 TEST after a clean stable run.",
                _=>"Build the cleanroom network."
            };
        }

        private void Present()
        {
            statusText.text=Status();
            string dp=pressureValid?$"{measuredRoom:+0.0;-0.0;0.0} Pa":"INVALID";
            string cnt=validSamples>0?$"{measuredRoomParticles:0} / {measuredLockParticles:0}":"—";
            meterText.text=$"ROOM {dp}  ·  AIRLOCK {measuredLock:+0.0;-0.0;0.0} Pa  |  {plant.Room.Temperature:0.0}°C  ·  RH {plant.Room.RelativeHumidity:P0}  ·  DEW {plant.Room.DewPoint:0.0}°C";
            detailText.text=$"AIR {plant.SupplyRoom:0.00} m³/s  ·  {plant.Velocity:0.00} m/s  |  HEPA A {measuredDrop:0} Pa  |  PARTICLES {cnt} /m³";
            calibrationText.text=$"{latestEvent}  |  PID {Gains[pressureGain]}/{Gains[airGain]}  ·  {targetRoom:+0} / {targetLock:+0} Pa  ·  {targetTemp:0}°C / {targetRh:P0}";
            if(controllerDisplay) controllerDisplay.text=pressureValid?$"ROOM {measuredRoom:+0.0;-0.0;0.0} Pa":"ROOM DP INVALID";
            if(qualityDisplay) qualityDisplay.text=$"HEPA A {measuredDrop:0} Pa · {measuredRoomParticles:0}/m³";
            powerButton.interactable=stage<6;
            calPressureButton.interactable=Ready&&!pressureCal;
            calAirButton.interactable=Ready&&!airCal;
            calParticlesButton.interactable=Ready&&!particleCal;
            calTempButton.interactable=Ready&&!tempCal;
            calHumidityButton.interactable=Ready&&!rhCal;
            supplyButton.interactable=Ready&&!supply&&latch==Fault.None;
            returnFanButton.interactable=Ready&&!returnFan&&latch==Fault.None;
            exhaustButton.interactable=Ready&&!exhaust&&latch==Fault.None;
            ffuAButton.interactable=Ready&&!ffuA&&latch==Fault.None;
            ffuBButton.interactable=Ready&&!ffuB&&latch==Fault.None;
            setRoomButton.interactable=setLockButton.interactable=setTempButton.interactable=setRhButton.interactable=
                tunePressureButton.interactable=tuneAirButton.interactable=stage==2;
            environmentButton.interactable=CanAuto&&stage is >=2 and <6;
            waferButton.interactable=CanWafer;
            doorButton.interactable=CanDoor;
            loadingButton.interactable=CanLoad;
            diagnoseButton.interactable=CanDiagnose;
            leakButton.interactable=CanLeak;
            serviceButton.interactable=CanService;
            isolateButton.interactable=service==Maintenance.Requested;
            replaceButton.interactable=service==Maintenance.Isolated;
            verifyButton.interactable=service==Maintenance.Replaced;
            resetButton.interactable=CanReset;
            armButton.interactable=CanArm;
            recalButton.interactable=CanRecal;
            certifyButton.interactable=CanCertify;
            eStopButton.interactable=stage<6;
            rig.Apply(plant,elapsed,waferClock,doorClock is >=4.5f and <12f,service is Maintenance.Isolated or Maintenance.Replaced);
        }
    }
}



