using System;
using System.Collections.Generic;
using ProjectSpark.Circuit;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.Gameplay
{
    public sealed class SparkLevel69Process : MonoBehaviour
    {
        public static readonly string[] Gates={"L69_Wired","L69_Commission","L69_Zones","L69_Defrost","L69_Protection","L69_Reconnected"};
        public static readonly (int a,int b)[] Wires={(0,1),(2,3),(0,4),(2,5),(0,6),(2,7),(0,8),(2,9),(0,10),(2,11),(0,12),(2,13),(0,14),(2,15),(0,16),(2,17),(0,18),(2,19),(20,21),(22,23),(24,25),(26,27),(28,29),(30,31),(32,33),(34,35),(36,37),(38,39),(40,41),(42,43),(44,45),(46,47),(48,49),(50,51),(52,53),(54,55),(56,57),(58,59),(60,61),(62,63),(64,65),(66,67),(68,69),(70,71),(72,73),(74,75),(76,77),(78,79),(80,81),(82,83),(84,85),(86,87),(88,89),(90,91),(92,93),(94,95),(96,97),(98,99),(100,101),(102,103),(104,105),(24,106),(30,107),(34,108),(60,109),(62,110),(68,111),(70,112),(72,113),(74,114),(76,115),(78,116),(80,117),(86,118),(92,119),(94,120),(96,121),(103,122),(44,123),(46,124),(36,125),(126,127)};
        private const int StopInput=91, TempPositive=8, TempReturn=9, FeedbackWire=81, CompressorWire=18;
        public SparkLevelLayout layout;
        public SparkPowerSupply source;
        public SparkSwitch[] contacts;
        public SparkSwitch emergency;
        public SparkTerminal[] pins;
        public SparkLevel69VisualRig rig;
        public SparkLevel69Trend plot;
        public TMP_Text controllerDisplay,qualityDisplay;
        public Text statusText,meterText,detailText,calibrationText;
        public Button powerButton,safetyCheckButton,calPressureButton,calTempButton,calMassButton,condenserButton,evaporatorButton,
            startButton,setRpmButton,enableAButton,enableBButton,calAButton,calBButton,targetAButton,targetBButton,autoZonesButton,
            tuneShButton,tuneZonesButton,optimizeButton,doorTestButton,frostTestButton,defrostButton,fanFaultButton,armFeedbackButton,
            recoverButton,resetButton,certifyButton,stopButton,eStopButton;

        private enum Fault { None, Power, Stop, Wiring, EvapSensor, HighPressure, LiquidReturn, Charge }
        private enum DefrostPhase { Idle, Isolating, Melting, Drip, Restored }
        private readonly SparkLevel69ColdPlant plant=new();
        private readonly SparkLevel69ColdPlant.Pi pressure=new();
        private readonly SparkLevel69ColdPlant.Pi[] zones={new(),new()},valves={new(),new()};
        private readonly List<SparkCircuitConnection> edges=new();
        private readonly HashSet<(int,int)> wires=new();
        private readonly Queue<string> events=new();
        private readonly float[] target={4,2}, measuredAir={9.8f,8.3f}, measuredSuperheat=new float[2];
        private static readonly string[] Tuning={"LOW","TUNED","HIGH"};
        private LevelGamePlayManager manager;
        private SparkLevelLayoutLoader loader;
        private SparkLevelSceneBindings bindings;
        private int topology=-1,stage,shGain,zoneGain;
        private bool power,rails,safety,tempPower,wiredSensor,drivePath,calPressure,calTemp,calMass,safetyChecked,calA,calB,validA,recalibrated;
        private bool condenserFans,evaporatorFans,running,eevA,eevB,auto,optimized,door,FrostHumidity,fanFailure,fanObserved,fanReset;
        private bool doorPassed,armed,disconnectObserved,feedbackReset,freshDoor,certify,previousDrive;
        private float elapsed,accumulator,held,measurementAge,rpmLimit=.95f,measuredPressure,measuredMass,liquidTimer,defrostClock;
        private float doorClock,doorAir,doorPeak,doorDuty,doorPeakDuty,fanStartPressure,badFrost,defrostStartMelt,defrostStartDrain;
        private float baselineCop,copTime,copEnergyStart,copCoolingStart,efficiencyCop,optimizationSince;
        private string wiring,latestEvent="Build the refrigeration network and enable 5 V control.";
        private Fault latch;
        private DefrostPhase defrost;

        private bool Ready=>rails&&safety&&wiring==null;
        private bool Calibrated=>calPressure&&calTemp&&calMass;
        private bool Permit=>Ready&&Calibrated&&safetyChecked&&validA&&latch==Fault.None;
        private bool AtRest=>plant.Rpm<25;
        private bool ThreePhases=>plant.Receiver>.14f&&plant.MassFlow>.003f&&plant.Rpm>350&&
            plant.LiquidTemperature<plant.Condenser-.5f&&plant.SuctionPressure<plant.DischargePressure*.60f&&
            plant.CoolingKw>.50f&&plant.Accumulator<.029f&&plant.SuctionTemperature-plant.Evaporation>2f;
        private bool SafeSuperheat=>validA&&measuredSuperheat[0] is >=4.7f and <=8.4f&&measuredSuperheat[1] is >=4.7f and <=8.4f;
        private bool OnTarget=>Mathf.Abs(measuredAir[0]-target[0])<.85f&&Mathf.Abs(measuredAir[1]-target[1])<.85f&&
            Mathf.Abs(target[0]-4)<.1f&&Mathf.Abs(target[1]-2)<.1f&&plant.Zones[0].Cargo<6.7f&&plant.Zones[1].Cargo<5.7f;
        private bool Stable=>Permit&&running&&auto&&eevA&&eevB&&calA&&calB&&ThreePhases&&SafeSuperheat&&OnTarget;
        private bool CoolingB=>defrost==DefrostPhase.Idle||defrost==DefrostPhase.Restored;

        private void Start()
        {
            manager=FindFirstObjectByType<LevelGamePlayManager>();
            loader=FindFirstObjectByType<SparkLevelLayoutLoader>();
            if(!manager||!loader||pins.Length!=128||contacts.Length!=1||!rig)
            {
                Debug.LogError("[LEVEL 69] Missing campaign bindings or refrigeration rig.",this);
                enabled=false;
                return;
            }
            source.SetOutput(false,default);
        }

        private static (int,int) Pair(int a,int b)=>(Math.Min(a,b),Math.Max(a,b));
        private bool Routed(int i)=>wires.Contains(Pair(Wires[i].a,Wires[i].b));
        private bool OnlySensorMissing()
        {
            if(wiredSensor||wires.Count!=Wires.Length-1) return false;
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
            wiredSensor=Routed(FeedbackWire);
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
            for(int i=0;i<18&&rails;i+=2) rails=Rail(Wires[i].b,Wires[i+1].b);
            safety=emergency.IsClosed&&Rail(StopInput,7);
            tempPower=power&&Rail(TempPositive,TempReturn);
            drivePath=power&&Rail(Wires[CompressorWire].b,7);
            if(stage==0&&Ready) Award();
            if(latch==Fault.None&&(running||armed)&&!Ready)
            {
                bool lost=rails&&safety&&OnlySensorMissing();
                disconnectObserved|=stage==5&&armed&&lost;
                Trip(lost?Fault.EvapSensor:!rails?Fault.Power:!safety?Fault.Stop:Fault.Wiring);
            }
            accumulator+=Mathf.Min(Time.deltaTime,.20f)*2.7f;
            const float dt=SparkLevel69ColdPlant.Step;
            while(accumulator>=dt){accumulator-=dt;Tick(dt);}
            Present();
        }

        private void Tick(float dt)
        {
            elapsed+=dt;
            bool hasTemperature=calTemp&&tempPower&&wiredSensor&&(latch!=Fault.EvapSensor||recalibrated);
            measurementAge=hasTemperature?measurementAge+dt:0;
            validA=measurementAge>.60f;
            float lpf=dt/(.25f+dt);
            measuredPressure=Mathf.Lerp(measuredPressure,plant.SuctionPressure,lpf);
            measuredMass=Mathf.Lerp(measuredMass,plant.MassFlow,lpf);
            for(int i=0;i<2;i++)
            {
                var zone=plant.Zones[i];
                measuredAir[i]=Mathf.Lerp(measuredAir[i],zone.Air,lpf);
                float sh=zone.Feed>.0001f?zone.Outlet-SparkLevel69Refrigerant.Saturation(measuredPressure):0;
                measuredSuperheat[i]=i==0&&!validA?0:Mathf.Lerp(measuredSuperheat[i],sh,lpf);
            }

            float highLimit=SparkLevel69Refrigerant.Pressure(52f);
            if(running&&latch==Fault.None&&plant.DischargePressure>=highLimit)
            {
                fanObserved|=fanFailure&&plant.DischargePressure>fanStartPressure+125;
                Trip(Fault.HighPressure);
            }
            liquidTimer=running&&(plant.Accumulator>.044f||plant.Oil<.014f)?liquidTimer+dt:0;
            if(latch==Fault.None&&liquidTimer>.65f) Trip(Fault.LiquidReturn);
            if(running&&latch==Fault.None&&plant.Receiver<.055f) Trip(Fault.Charge);
            bool contact=Permit&&running&&condenserFans&&evaporatorFans;
            if(contact!=previousDrive)
            {
                previousDrive=contact;
                contacts[0].SetState(contact?SparkSwitchState.Closed:SparkSwitchState.Open);
                ResetControllers();
            }

            bool pumping=contact&&drivePath&&contacts[0].IsClosed;
            float fanA=0,fanB=0,valveA=0,valveB=0,drive=0;
            if(pumping)
            {
                float RequestedFan(int i,bool feed)
                {
                    if(!feed) return 0;
                    if(!auto) return .57f;
                    var z=plant.Zones[i];
                    float available=z.Capacity(plant.Evaporation,1);
                    float demand=zones[i].StepToward(measuredAir[i]-target[i],zoneGain==1?.51f:zoneGain==2?1.2f:.19f,
                        zoneGain==1?.063f:zoneGain==2?.21f:.017f,z.Leak*(SparkLevel69ColdPlant.Ambient-target[i]),.17f,2.30f,dt,.025f);
                    return Mathf.Clamp((demand/Mathf.Max(available,.19f)-.10f)/.90f,.10f,1);
                }
                fanA=RequestedFan(0,eevA);
                fanB=RequestedFan(1,eevB&&CoolingB);
                float qA=eevA?plant.Zones[0].Capacity(plant.Evaporation,fanA):0;
                float qB=eevB&&CoolingB?plant.Zones[1].Capacity(plant.Evaporation,fanB):0;
                float lift=plant.LatentLift(6.3f);
                float Valve(int i,float q,bool enabled)
                {
                    if(!enabled) return 0;
                    float desired=q/lift/Mathf.Max(.0001f,plant.ValveFlowCoefficient);
                    if(i==0?calA:calB)
                    {
                        var z=plant.Zones[i];
                        float err=z.Feed>.0001f?measuredSuperheat[i]-6.3f:0f;
                        float corrected=valves[i].StepToward(err,shGain==1?.013f:shGain==2?.037f:.0035f,
                            shGain==1?.0052f:shGain==2?.014f:.0011f,desired,Mathf.Max(.015f,desired-.16f),Mathf.Min(.98f,desired+.16f),dt,.0008f);
                        return Mathf.Clamp(corrected,.015f,.98f);
                    }
                    return Mathf.Clamp(desired,.015f,.98f);
                }
                valveA=Valve(0,qA,eevA);
                valveB=Valve(1,qB,eevB&&CoolingB);
                float ratio=plant.DischargePressure/Mathf.Max(plant.SuctionPressure,78);
                float capacity=.048f*(plant.SuctionPressure/220f)*Mathf.Clamp(1-.065f*(ratio-1),.36f,1);
                float expected=(qA+qB)/lift/Mathf.Max(capacity,.001f);
                drive=pressure.StepToward(plant.Evaporation-(optimized?-7.2f:-12.5f),.060f,.014f,expected,.15f,rpmLimit,dt);
            }
            float condFan=rails&&condenserFans&&!fanFailure?(optimized?.96f:.64f):0;
            bool heater=Permit&&defrost==DefrostPhase.Melting&&plant.Zones[1].Valve<.028f&&plant.Zones[1].AirFan<.055f;
            plant.Tick(dt,drive,condFan,fanA,fanB,valveA,valveB,FrostHumidity,heater,door);
            ObserveDefrost(dt);
            ObserveDoor(dt);
            ObserveEfficiency(dt);
            CheckObjectives(dt);
            if(plot!=null) plot.Sample(plant,validA,calPressure);
        }

        private void ResetControllers()
        {
            pressure.Reset();
            for(int i=0;i<2;i++){zones[i].Reset();valves[i].Reset();}
        }
        private void Trip(Fault cause)
        {
            latch=cause;running=false;previousDrive=false;
            contacts[0].SetState(SparkSwitchState.Open);
            door=false;FrostHumidity=false;
            if(!CoolingB) defrost=DefrostPhase.Idle;
            fanFailure=false;
            held=0;
            if(cause==Fault.EvapSensor){validA=false;recalibrated=false;measurementAge=0;measuredSuperheat[0]=0;}
            Log(cause==Fault.EvapSensor?"EVAP A TEMP INVALID · compressor isolated and EEVs closing.":$"COMPRESSOR PROTECTION · {cause} · stop verified.");
        }
        private void Log(string text)
        {
            latestEvent=$"{elapsed:0.0}s · {text}";
            events.Enqueue(latestEvent);
            if(events.Count>8) events.Dequeue();
        }
        private void ObserveDefrost(float dt)
        {
            var b=plant.Zones[1];
            switch(defrost)
            {
                case DefrostPhase.Isolating:
                    if(b.Valve<.025f&&b.AirFan<.05f){defrost=DefrostPhase.Melting;Log("EEV B and fan isolated. Defrost heater active.");}
                    break;
                case DefrostPhase.Melting:
                    if(b.Frost<.006f){defrostClock=0;defrost=DefrostPhase.Drip;Log("Ice melted. Drip delay underway.");}
                    break;
                case DefrostPhase.Drip:
                    defrostClock+=dt;
                    if(defrostClock>4f&&plant.Drain<.009f){defrost=DefrostPhase.Restored;Log("Drain clear. Cooling B recommissioning.");}
                    break;
            }
        }
        private void ObserveDoor(float dt)
        {
            if(doorClock<=0) return;
            doorClock+=dt;
            doorPeak=Mathf.Max(doorPeak,plant.Zones[0].Air);
            doorPeakDuty=Mathf.Max(doorPeakDuty,plant.Zones[0].Duty);
            if(doorClock>=10f) door=false;
            if(doorClock<15f||!Stable&&doorClock<95f) return;
            bool observed=doorPeak>doorAir+.50f&&doorPeakDuty>doorDuty+.18f&&Stable;
            if(stage==4) doorPassed|=observed;
            if(stage==5&&feedbackReset) freshDoor|=observed;
            doorClock=0;
            Log(observed?"DOOR LOAD VERIFIED · increased heat load and controlled recovery.":"Door test incomplete. Wait for stable temperatures, then retry.");
        }
        private void ObserveEfficiency(float dt)
        {
            bool collecting=stage==4&&doorPassed&&doorClock==0&&Stable&&(!fanObserved||fanReset&&optimized);
            if(!collecting)
            {
                copTime=0;
                return;
            }
            if(copTime==0){copEnergyStart=plant.EnergyInput;copCoolingStart=plant.UsefulCooling;}
            copTime+=dt;
            if(copTime<7.0f||optimized&&elapsed-optimizationSince<12f) return;
            float cop=(plant.UsefulCooling-copCoolingStart)/Mathf.Max(.001f,plant.EnergyInput-copEnergyStart);
            if(!optimized&&baselineCop<=0){baselineCop=cop;Log($"Nominal measured COP {cop:0.00}.");}
            if(optimized) efficiencyCop=cop;
            copTime=0;
        }

        private void CheckObjectives(float dt)
        {
            bool goal=stage switch
            {
                1=>Permit&&running&&ThreePhases&&plant.RejectionKw>.8f&&plant.Zones[0].Duty>.23f&&plant.Zones[1].Duty>.23f,
                2=>Stable&&zoneGain==1&&shGain==1,
                3=>Stable&&defrost==DefrostPhase.Restored&&plant.Melted-defrostStartMelt>.045f&&
                    plant.Drained-defrostStartDrain>.040f&&plant.Zones[1].Frost<badFrost*.40f&&plant.Zones[1].Duty>.40f,
                4=>Stable&&doorPassed&&fanObserved&&fanReset&&optimized&&baselineCop>.1f&&efficiencyCop>baselineCop*1.045f,
                5=>Stable&&disconnectObserved&&feedbackReset&&freshDoor&&certify&&plant.MassError<.0001f&&plant.CycleResidual<.001f,
                _=>false
            };
            held=goal?held+dt:0;
            if(held>(stage==2?5f:stage==5?3.5f:2.4f)) Award();
        }
        private void Award()
        {
            if(stage>=Gates.Length||!bindings.SetRuntimeComponentBinding(Gates[stage],source)) return;
            stage++;held=0;manager.QueueEvaluation();Log($"OBJECTIVE {stage}/6 verified.");
        }

        public void Power(){power=!power;source.SetOutput(power,default);}
        public void SafetyCheck(){if(Ready){safetyChecked=true;Log("Stop loop, charge and protection ready.");}}
        public void CalPressure(){if(Ready&&!running){calPressure=true;Log("High and low pressure references calibrated.");}}
        public void CalTemp(){if(Ready&&!running){calTemp=true;measurementAge=0;Log("Temperature sensors calibrated.");}}
        public void CalMass(){if(Ready&&!running){calMass=true;Log("Mass flow sensor calibrated.");}}
        public void Condenser(){if(Ready&&!running) condenserFans=!condenserFans;}
        public void Evaporator(){if(Ready&&!running) evaporatorFans=!evaporatorFans;}
        public void StartCompressor(){if(CanStart) running=true;}
        public void SetRpm(){if(stage is 1 or 2) rpmLimit=rpmLimit>=.949f?.74f:rpmLimit+.07f;}
        public void EnableA(){if(Ready&&latch==Fault.None&&!running) eevA=!eevA;}
        public void EnableB(){if(Ready&&latch==Fault.None&&!running) eevB=!eevB;}
        public void CalA(){if(running&&validA&&plant.Zones[0].Feed>.0001f) calA=true;}
        public void CalB(){if(running&&plant.Zones[1].Feed>.0001f) calB=true;}
        public void TargetA(){if(stage==2) target[0]=target[0]>=5.9f?3:target[0]+1;}
        public void TargetB(){if(stage==2) target[1]=target[1]>=3.9f?1:target[1]+1;}
        public void AutoZones(){if(Permit&&stage>=2&&calA&&calB){auto=!auto;ResetControllers();}}
        public void TuneSh(){if(stage==2){shGain=(shGain+1)%3;valves[0].Reset();valves[1].Reset();}}
        public void TuneZones(){if(stage==2){zoneGain=(zoneGain+1)%3;zones[0].Reset();zones[1].Reset();}}
        public void Optimize(){if(CanOptimize){optimized=true;optimizationSince=elapsed;copTime=0;pressure.Reset();Log("Efficiency mode raised suction target and optimized condenser airflow.");}}
        public void DoorTest()
        {
            if(!CanDoor) return;
            door=true;doorClock=.01f;
            doorPeak=doorAir=plant.Zones[0].Air;doorPeakDuty=doorDuty=plant.Zones[0].Duty;
            Log("Chamber A door opened. Warm-air load applied.");
        }
        public void FrostTest(){if(CanFrost){FrostHumidity=true;Log("Humid air applied to Evaporator B.");}}
        public void Defrost()
        {
            if(!CanDefrost) return;
            badFrost=plant.Zones[1].Frost;defrostStartMelt=plant.Melted;defrostStartDrain=plant.Drained;
            FrostHumidity=false;defrost=DefrostPhase.Isolating;
            Log("Defrost requested. Isolating branch B.");
        }
        public void FanFault()
        {
            if(!CanFanFault) return;
            fanStartPressure=plant.DischargePressure;fanFailure=true;
            Log("Condenser fan supply failed. Watch high-side pressure.");
        }
        public void ArmFeedback(){if(CanArm){armed=true;Log("Undo W82 · Evaporator A outlet sensor to superheat controller.");}}
        public void Recover()
        {
            if(!CanRecover) return;
            recalibrated=true;measurementAge=0;
            Log("Evaporator A sensor recalibrated. Await fresh temperature.");
        }
        public void Reset()
        {
            if(!CanReset) return;
            fanReset|=latch==Fault.HighPressure&&fanObserved;
            feedbackReset|=latch==Fault.EvapSensor&&disconnectObserved;
            armed=false;latch=Fault.None;
            Log("Protection reset. Start compressor to recommission both zones.");
        }
        public void Certify(){if(CanCertify) certify=true;}
        public void Stop(){running=false;door=false;doorClock=0;}
        public void EStop()
        {
            emergency.SetState(emergency.IsClosed?SparkSwitchState.Open:SparkSwitchState.Closed);
            if(emergency.IsOpen&&latch==Fault.None) Trip(Fault.Stop);
        }

        private bool CanStart=>Permit&&!running&&condenserFans&&evaporatorFans&&eevA&&eevB&&AtRest;
        private bool CanDoor=>Stable&&doorClock==0&&(stage==4&&!doorPassed||stage==5&&feedbackReset&&!freshDoor);
        private bool CanFrost=>stage==3&&Stable&&defrost==DefrostPhase.Idle&&!FrostHumidity;
        private bool CanDefrost=>stage==3&&Permit&&running&&FrostHumidity&&defrost==DefrostPhase.Idle&&plant.Zones[1].Frost>=.072f;
        private bool CanFanFault=>stage==4&&doorPassed&&Stable&&baselineCop>.1f&&!fanFailure&&!fanObserved;
        private bool CanOptimize=>stage==4&&fanReset&&Stable&&!optimized&&baselineCop>.1f;
        private bool CanArm=>stage==5&&Stable&&!disconnectObserved&&!armed;
        private bool CanRecover=>latch==Fault.EvapSensor&&Ready&&AtRest&&!recalibrated;
        private bool CanReset=>latch!=Fault.None&&Ready&&AtRest&&validA&&plant.Condenser<39.5f&&plant.Accumulator<.030f&&plant.Receiver>.09f;
        private bool CanCertify=>stage==5&&Stable&&feedbackReset&&freshDoor&&!certify;

        private string Status()
        {
            if(stage==6) return "CERTIFIED · both chambers stable, defrost verified and Evaporator A feedback restored.";
            if(!power) return "Enable 5 V CONTROL and build the refrigeration network.";
            if(latch==Fault.EvapSensor)
                return !Ready?"EVAP A TEMP INVALID · reconnect W82.":CanRecover?"Recalibrate Evaporator A when the compressor stops.":"Await fresh sensor data and safe conditions, then RESET.";
            if(latch!=Fault.None) return $"COMPRESSOR PROTECTION LATCHED · {latch}.";
            if(!Ready) return wiring??"Check the 5 V rails and stop loop.";
            if(!safetyChecked||!Calibrated) return "Run SAFETY CHECK and calibrate pressure, temperature and mass flow.";
            if(armed) return "Disconnect W82 · EVAP A TEMP OUT → SH CONTROLLER A TEMP IN.";
            if(!condenserFans||!evaporatorFans||!eevA||!eevB||!running) return "Enable both fan groups and both EEVs, then START COMPRESSOR.";
            if(fanFailure) return "Fan fault test · observe high-side pressure and compressor trip.";
            if(doorClock>0) return door?"Door open · watch the rising cooling load.":"Door closed · wait for both zones to recover.";
            if(!CoolingB) return defrost==DefrostPhase.Melting?"DEFROST · measured heat is melting frost.":defrost==DefrostPhase.Drip?"DRIP DELAY · wait for measured condensate to drain.":"ISOLATING · wait for EEV B and fan to stop.";
            return stage switch
            {
                1=>"Commissioning · watch pressure separation, mass flow and condenser heat rejection.",
                2=>"Calibrate both superheat channels. Tune both loops and enable DUAL-ZONE AUTO.",
                3=>defrost==DefrostPhase.Restored?"Wait for Chamber B to recover.":FrostHumidity?CanDefrost?"Frost measured and airflow reduced. REQUEST DEFROST.":"Observe frost growth and reduced coil capacity.":"Apply FROST TEST to Evaporator B.",
                4=>!doorPassed?"Apply DOOR TEST.":baselineCop<=0?"Recording the nominal COP.":!fanObserved?"Apply FAN FAULT.":!optimized?"Verify stable recovery and select OPTIMIZE.":"Measuring optimized energy performance.",
                5=>feedbackReset?freshDoor?"Both zones stable. CERTIFY.":"Repeat DOOR TEST after recovery.":"Arm W82 TEST when both zones are stable.",
                _=>"Build the refrigeration network."
            };
        }
        private void Present()
        {
            statusText.text=Status();
            string shA=validA?$"{measuredSuperheat[0]:0.0} K":"INVALID";
            string shB=calTemp&&rails?$"{measuredSuperheat[1]:0.0} K":"—";
            string pressureDisplay=calPressure&&rails?$"{plant.SuctionPressure:0}/{plant.DischargePressure:0} kPa":"—";
            string mass=calMass&&rails?$"{plant.MassFlow*1000:0.0} g/s":"—";
            meterText.text=$"A {measuredAir[0]:0.0}/{target[0]:0}°C  ·  B {measuredAir[1]:0.0}/{target[1]:0}°C  |  SH A {shA}  ·  SH B {shB}  |  LP/HP {pressureDisplay}";
            detailText.text=$"{plant.Rpm:0} RPM  ·  {mass}  ·  {plant.ElectricKw:0.00} kW  ·  COP {plant.Cop:0.00}  ·  FROST B {plant.Zones[1].Frost*1000:0} g  ·  DRAIN {plant.Drained*1000:0} g";
            calibrationText.text=$"{latestEvent}  |  PID {Tuning[shGain]}/{Tuning[zoneGain]}";
            if(controllerDisplay) controllerDisplay.text=$"{plant.Rpm:0} RPM · {plant.Cop:0.00} COP";
            if(qualityDisplay) qualityDisplay.text=validA?$"AIR A {measuredAir[0]:0.0}°C  /  B {measuredAir[1]:0.0}°C":"EVAP A TEMP INVALID";
            powerButton.interactable=stage<6;
            safetyCheckButton.interactable=Ready&&!safetyChecked;
            calPressureButton.interactable=Ready&&!calPressure;
            calTempButton.interactable=Ready&&!calTemp;
            calMassButton.interactable=Ready&&!calMass;
            condenserButton.interactable=Ready&&!running&&stage<6;
            evaporatorButton.interactable=condenserButton.interactable;
            startButton.interactable=CanStart&&stage<6;
            setRpmButton.interactable=stage is 1 or 2;
            enableAButton.interactable=enableBButton.interactable=Ready&&!running&&latch==Fault.None&&stage<6;
            calAButton.interactable=running&&validA&&!calA&&plant.Zones[0].Feed>.0001f;
            calBButton.interactable=running&&!calB&&plant.Zones[1].Feed>.0001f;
            targetAButton.interactable=targetBButton.interactable=tuneShButton.interactable=tuneZonesButton.interactable=stage==2;
            autoZonesButton.interactable=Permit&&stage is >=2 and <6&&calA&&calB;
            optimizeButton.interactable=CanOptimize;
            doorTestButton.interactable=CanDoor;
            frostTestButton.interactable=CanFrost;
            defrostButton.interactable=CanDefrost;
            fanFaultButton.interactable=CanFanFault;
            armFeedbackButton.interactable=CanArm;
            recoverButton.interactable=CanRecover;
            resetButton.interactable=CanReset;
            certifyButton.interactable=CanCertify;
            stopButton.interactable=running;
            eStopButton.interactable=stage<6;
            rig.Apply(plant,elapsed,!Ready||latch!=Fault.None);
        }
    }
}

