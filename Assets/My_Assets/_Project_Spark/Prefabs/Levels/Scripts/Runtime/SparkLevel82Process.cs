using System;
using System.Collections.Generic;
using ProjectSpark.Circuit;
using ProjectSpark.Visuals;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Mode=ProjectSpark.Gameplay.SparkLevel82Plant.Mode;

namespace ProjectSpark.Gameplay
{
    public sealed class SparkLevel82Process : MonoBehaviour
    {
        public static readonly string[] Gates={"L82_Wired","L82_Calibrated","L82_Emissions","L82_Immunity","L82_Disturbances","L82_Recertified"};
        public static readonly (int a,int b)[] Wires={(0,1),(2,3),(0,4),(2,5),(0,6),(2,7),(0,8),(2,9),(0,10),(2,11),(12,13),(14,15),(14,16),(17,18),(19,20),(21,22),(23,24),(25,26),(27,28),(29,30),(31,32),(33,34),(35,36),(37,38),(39,40),(41,42),(43,44),(45,46),(47,48),(49,50),(51,52),(53,54),(55,56),(57,58),(59,60),(61,62),(63,64),(65,66),(67,68),(69,70),(71,72),(73,74),(75,76),(77,78),(79,80),(81,82),(51,83),(84,85),(86,87),(88,89),(90,91),(90,92),(93,94),(95,96),(97,98),(99,100),(101,102),(103,104),(105,106),(107,108),(109,110),(111,112),(113,114),(115,116),(117,118),(119,120),(121,122),(123,124),(125,126),(127,128),(129,130),(131,132),(133,134),(135,136),(137,138),(139,140),(141,142),(143,144),(145,146),(146,147),(62,148),(148,149),(74,150),(150,151),(98,152),(152,153),(153,154),(154,155),(124,156),(156,157),(116,158),(158,159),(152,160),(153,161),(161,162),(162,163),(151,164),(32,165),(20,166),(165,167),(164,168),(168,169),(165,170),(166,171),(76,172),(171,173),(169,174),(72,175),(167,176),(176,177),(66,178),(149,179),(110,180),(151,181),(178,182),(182,183),(154,184),(185,186),(187,188),(159,189),(190,191),(186,192),(193,194),(155,195),(159,196),(156,197),(103,198),(99,199),(177,200),(186,201),(192,202),(202,203),(200,204),(204,205)};
        private const int StopInput=194,ControlReturn=3,FieldSupply=8,FieldReturn=9,DoorInput=186,Feedback=118,PermitWire=120;
        public SparkLevelLayout layout;
        public SparkPowerSupply source;
        public SparkSwitch[] contacts;
        public SparkSwitch emergency,doorSwitch;
        public SparkTerminal[] pins;
        public SparkLevel82VisualRig rig;
        public SparkLevel82Trend plot;
        public TMP_Text controllerDisplay,qualityDisplay;
        public Text statusText,meterText,detailText,calibrationText;
        public Button powerButton,doorButton,calMotionButton,calEmiButton,calFieldButton,conductButton,radiateButton,nearButton,
            harnessButton,gasketButton,filterButton,sensorButton,armRfButton,immunityButton,stopButton,harnessFaultButton,seamFaultButton,
            sensorFaultButton,diagnoseButton,verifyButton,resetButton,armButton,eStopButton,certifyButton;

        private enum Fault { None, Power, Wiring, Door, EStop, Field, Sensor, Range }
        private enum Repair { None, Harness, Seam }
        private readonly SparkLevel82Plant plant=new();
        private readonly float[] conductedTrace=new float[SparkLevel82Plant.Bins],baselineTrace=new float[SparkLevel82Plant.Bins],
            radiatedTrace=new float[SparkLevel82Plant.Bins],radiation=new float[SparkLevel82Plant.Poses],nearH=new float[16],nearE=new float[16];
        private readonly HashSet<(int,int)> wires=new();
        private readonly List<SparkCircuitConnection> edges=new();
        private LevelGamePlayManager manager;
        private SparkLevelLayoutLoader loader;
        private SparkLevelSceneBindings bindings;
        private Mode mode;
        private Fault latch;
        private Repair disturbance;
        private int topology=-1,stage,index,completedPoses,nearSamples,band,validBands,qualifiedSets,fieldSequence,sequenceAtTrip;
        private bool power,rails,safety,fieldPower,wiredFeedback,permitVoltage,doorVoltage,lastPermit;
        private bool doorOpen=true,route,gasket,filter,sensor,desense,motionCal,emiCal,fieldCal,trusted;
        private bool baselineConducted,baselineRadiated,nearComplete,diagnosed,diagnosedFault,conducted,radiated,immune;
        private bool rfArmed,rfRundown,verified,faultDiagnosed,harnessPassed,seamPassed,sensorPassed,armed,feedbackObserved,feedbackReset,certify;
        private float elapsed,accumulator,modeTime,dwell,awardDwell,sampleClock,sampleAge=.5f,sampleField,fieldZero,fieldGain=1;
        private float angle,height=.18f,pol,distance=.52f,probeX,probeY,probeZ=.17f;
        private float conductedWorst,radiatedWorst,highH,highE,ampAtTrip,desenseAtTrip;
        private string wiring,latestEvent="Connect the EMC qualification chamber.";

        private bool Ready=>rails&&safety&&wiring==null;
        private bool Healthy=>Ready&&latch==Fault.None;
        private bool Idle=>mode==Mode.Idle;
        private bool Sealed=>plant.Closed&&doorSwitch.IsClosed&&doorVoltage;
        private bool MotionSettled=>Mathf.Abs(angle-plant.Azimuth)<.4f&&Mathf.Abs(height-plant.Height)<.006f&&
            Mathf.Abs(pol-plant.Polarization)<.4f&&Mathf.Abs(distance-plant.Distance)<.006f&&
            Mathf.Abs(probeX-plant.ProbeX)<.009f&&Mathf.Abs(probeY-plant.ProbeY)<.009f&&Mathf.Abs(probeZ-plant.ProbeZ)<.009f;
        private bool RepairsSettled=>Mathf.Abs(plant.Harness-(route?1:0))<.02f&&Mathf.Abs(plant.Gasket-(gasket?1:0))<.02f&&
            Mathf.Abs(plant.PowerFilter-(filter?1:0))<.02f&&Mathf.Abs(plant.SensorFilter-(sensor?1:0))<.02f;
        private bool CanCalField=>Ready&&Idle&&Sealed&&plant.RfSafe&&fieldPower&&wiredFeedback&&!fieldCal;
        private bool CanScan=>stage>=2&&Healthy&&Idle&&Sealed&&plant.RfSafe&&motionCal&&emiCal&&RepairsSettled&&plant.Load>.65f;
        private bool CanDiagnose=>Idle&&(latch!=Fault.None&&!faultDiagnosed||
            latch==Fault.None&&nearComplete&&(!diagnosed&&baselineConducted&&baselineRadiated||disturbance!=Repair.None&&!diagnosedFault));
        private bool CanRepair=>Healthy&&Idle&&plant.RfSafe&&diagnosed;
        private bool CanHarness=>CanRepair&&!route&&(disturbance==Repair.None||disturbance==Repair.Harness&&diagnosedFault);
        private bool CanGasket=>CanRepair&&!gasket&&(disturbance==Repair.None||disturbance==Repair.Seam&&diagnosedFault);
        private bool CanSensor=>Idle&&Ready&&plant.RfSafe&&(!sensor&&diagnosed&&latch==Fault.None||latch==Fault.Sensor&&faultDiagnosed&&desense);
        private bool CanArmRf=>stage>=3&&Healthy&&Idle&&Sealed&&plant.RfSafe&&trusted&&conducted&&radiated&&sensor&&RepairsSettled&&plant.FunctionValid&&!rfArmed;
        private bool CanExposure=>Healthy&&Idle&&Sealed&&plant.RfSafe&&trusted&&rfArmed&&conducted&&radiated&&plant.FunctionValid;
        private bool CanHarnessFault=>stage==4&&Healthy&&Idle&&immune&&!harnessPassed&&disturbance==Repair.None;
        private bool CanSeamFault=>stage==4&&Healthy&&Idle&&harnessPassed&&!seamPassed&&disturbance==Repair.None&&RepairsSettled;
        private bool CanSensorFault=>stage==4&&Healthy&&harnessPassed&&seamPassed&&!sensorPassed&&!desense&&mode==Mode.Exposure&&plant.Field>.43f;
        private bool CanArm=>stage==5&&mode==Mode.Exposure&&plant.Field>.42f&&trusted&&lastPermit&&!feedbackObserved&&!armed;
        private bool CanVerify=>Ready&&Idle&&latch!=Fault.None&&faultDiagnosed&&Sealed&&trusted&&plant.RfSafe&&plant.Susceptibility<.028f&&plant.FunctionValid&&!verified&&
            (latch!=Fault.Field||fieldSequence>sequenceAtTrip);
        private bool CanReset=>CanRecoverReady&&verified;
        private bool CanRecoverReady=>Ready&&Idle&&latch!=Fault.None&&trusted&&Sealed&&plant.RfSafe&&plant.Susceptibility<.028f;
        private bool CanCertify=>stage==5&&feedbackReset&&conducted&&radiated&&immune&&qualifiedSets>=3&&Healthy&&Sealed&&trusted&&plant.RfSafe;

        private void Start()
        {
            manager=FindFirstObjectByType<LevelGamePlayManager>();loader=FindFirstObjectByType<SparkLevelLayoutLoader>();
            if(!manager||!loader||pins.Length!=206||contacts.Length!=1||doorSwitch==null||!rig)
            {
                Debug.LogError("[LEVEL 82] Missing EMC chamber references.",this);enabled=false;return;
            }
            source.SetOutput(false,default);
        }
        private static (int a,int b) Pair(int a,int b)=>a<b?(a,b):(b,a);
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
            wiring=wires.Count==Wires.Length?null:"Remove extra wires or check the routing chart.";
            for(int i=0;i<Wires.Length;i++)
            {
                if(Routed(i))continue;
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
            fieldPower=power&&Rail(FieldSupply,FieldReturn);
            permitVoltage=power&&Rail(Wires[PermitWire].b,ControlReturn);
            doorVoltage=power&&Rail(DoorInput,ControlReturn);
            if(!wiredFeedback){trusted=false;sampleAge=.5f;}
            if(latch==Fault.None&&!Idle&&!Healthy)
            {
                bool onlyField=rails&&safety&&!wiredFeedback&&wires.Count==Wires.Length-1;
                if(onlyField)
                    for(int i=0;i<Wires.Length;i++)
                        if(i!=Feedback&&!Routed(i)){onlyField=false;break;}
                feedbackObserved|=stage==5&&armed&&mode==Mode.Exposure&&plant.Field>.35f&&onlyField;
                Trip(onlyField?Fault.Field:!rails?Fault.Power:!safety?Fault.EStop:Fault.Wiring);
            }
            float speed=armed?.29f:stage==5&&!feedbackObserved&&mode==Mode.Exposure?.72f:2.48f;
            accumulator+=Mathf.Min(Time.deltaTime,.16f)*speed;
            while(accumulator>=SparkLevel82Plant.Step){accumulator-=SparkLevel82Plant.Step;Tick(SparkLevel82Plant.Step);}
            Present();
        }
        private void Tick(float dt)
        {
            elapsed+=dt;modeTime+=dt;
            sampleAge+=dt;sampleClock+=dt;
            if(sampleClock>.072f)
            {
                sampleClock=0;
                if(fieldPower&&wiredFeedback&&fieldCal)
                {
                    sampleField=Mathf.Max(0,(plant.FieldProbe(plant.Field)-fieldZero)*fieldGain);
                    sampleAge=0;fieldSequence++;
                }
            }
            trusted=fieldPower&&wiredFeedback&&fieldCal&&sampleAge<.16f;
            if(mode==Mode.Exposure&&latch==Fault.None&&(!Sealed||!trusted))
                Trip(!Sealed?Fault.Door:Fault.Field);
            bool permit=Healthy&&Sealed&&trusted&&mode==Mode.Exposure;
            SetPermit(permit);
            bool drive=permit&&permitVoltage&&contacts[0].IsClosed;
            float demand=Mode.Exposure==mode?Demand(band):0;
            plant.Tick(dt,elapsed,rails&&safety,doorOpen,angle,height,pol,distance,probeX,probeY,probeZ,route,gasket,filter,sensor,desense,drive,demand,band);
            bool closed=plant.Closed&&!doorOpen;
            if(doorSwitch.IsClosed!=closed)doorSwitch.SetState(closed?SparkSwitchState.Closed:SparkSwitchState.Open);
            if(disturbance==Repair.Harness&&route&&plant.Harness>.985f){harnessPassed=true;disturbance=Repair.None;Log("Harness route and ferrite closure verified.");}
            if(disturbance==Repair.Seam&&gasket&&plant.Gasket>.985f){seamPassed=true;disturbance=Repair.None;Log("Gasket compression verified.");}
            if(mode==Mode.Exposure&&desense&&plant.AdcError>.132f)
            {
                desenseAtTrip=plant.AdcError;sensorPassed=true;plant.SampleWorkload();Trip(Fault.Sensor);
            }
            else if(mode==Mode.Exposure&&sampleField>.82f)Trip(Fault.Range);
            Advance(dt);
            if(rfRundown&&plant.RfSafe)
            {
                rfRundown=false;
                immune=validBands==4&&conducted&&radiated&&trusted&&plant.FunctionValid&&latch==Fault.None;
                if(immune){qualifiedSets++;Log("Four monitored immunity bands passed and output is safe.");}
            }
            bool goal=stage switch
            {
                0=>fieldPower&&wiredFeedback,
                1=>Sealed&&motionCal&&emiCal&&fieldCal&&trusted&&plant.RfSafe,
                2=>diagnosed&&route&&gasket&&filter&&sensor&&conducted&&radiated&&completedPoses==SparkLevel82Plant.Poses&&plant.FunctionValid,
                3=>immune&&qualifiedSets>=1,
                4=>harnessPassed&&seamPassed&&sensorPassed&&immune&&qualifiedSets>=2,
                5=>CanCertify&&certify,
                _=>false
            };
            awardDwell=Healthy&&goal?awardDwell+dt:0;
            if(awardDwell>.32f)Award();
            if(plot!=null)plot.Show(radiation,completedPoses,conductedTrace,baselineTrace,nearH,nearE,plant.Field,trusted,latch!=Fault.None);
        }
        private static float Demand(int b)=>.53f+b*.061f;
        private void Advance(float dt)
        {
            if(Idle||!Ready)return;
            switch(mode)
            {
                case Mode.MotionCal when MotionSettled&&modeTime>.65f:
                    motionCal=true;End();Log("Table, mast, polarization and XYZ probe encoder movement verified.");break;
                case Mode.ReceiverCal when modeTime>.72f:
                    emiCal=Sealed&&plant.RfSafe&&plant.Load>.65f;End();Log(emiCal?"Spectrum reference, receiver and conducted clamps calibrated.":"Receiver calibration aborted.");break;
                case Mode.FieldCal when modeTime>.78f:
                    fieldCal=fieldPower&&wiredFeedback&&plant.RfSafe&&Sealed;
                    fieldZero=plant.FieldProbe(0);
                    float response=plant.FieldProbe(.25f)-fieldZero;
                    fieldCal&=response is >.21f and <.28f;
                    if(fieldCal)fieldGain=.25f/response;
                    sampleAge=.5f;End();Log(fieldCal?"Field probe zero and reference accepted. Waiting for fresh samples.":"Field probe calibration aborted.");break;
                case Mode.Conducted:
                    dwell+=dt;
                    if(dwell<.09f)return;
                    conductedTrace[index]=plant.Conducted(index);
                    conductedWorst=Mathf.Max(conductedWorst,conductedTrace[index]);dwell=0;
                    if(++index<SparkLevel82Plant.Bins)return;
                    if(!baselineConducted)Array.Copy(conductedTrace,baselineTrace,SparkLevel82Plant.Bins);
                    baselineConducted=true;conducted=conductedWorst<=SparkLevel82Plant.ConductedLimit&&plant.FunctionValid;
                    End();Log(conducted?"Conducted emission margin passed.":"Conducted scan above the scenario limit.");break;
                case Mode.Radiated:
                    dwell=MotionSettled?dwell+dt:0;
                    if(dwell<.125f)return;
                    float peak=0;
                    for(int i=0;i<SparkLevel82Plant.Bins;i++)
                    {
                        float measure=plant.Radiated(i);
                        radiatedTrace[i]=Mathf.Max(radiatedTrace[i],measure);peak=Mathf.Max(peak,measure);
                    }
                    radiation[index]=peak;radiatedWorst=Mathf.Max(radiatedWorst,peak);
                    completedPoses++;dwell=0;
                    if(++index<SparkLevel82Plant.Poses){Pose(index);return;}
                    baselineRadiated=true;radiated=radiatedWorst<=SparkLevel82Plant.RadiatedGuardedLimit&&plant.FunctionValid;
                    End();Log(radiated?"72 settled antenna poses passed the guarded limit.":"72 poses measured. Worst-case radiation is above limit.");break;
                case Mode.Near:
                    dwell=MotionSettled?dwell+dt:0;
                    if(dwell<.10f)return;
                    var (h,e)=plant.NearField();nearH[index]=h;nearE[index]=e;
                    highH=Mathf.Max(highH,h);highE=Mathf.Max(highE,e);
                    nearSamples++;dwell=0;
                    if(++index<16){Probe(index);return;}
                    nearComplete=true;End();Log("16 settled XYZ near-field positions mapped for E and H.");break;
                case Mode.Exposure:
                    if(modeTime>10.5f||modeTime>3.8f&&plant.Field<.35f){Trip(Fault.Range);return;}
                    if(!trusted||Mathf.Abs(sampleField-Demand(band))>.085f||!plant.FunctionValid){dwell=0;return;}
                    dwell+=dt;
                    if(dwell<(stage==5&&!feedbackObserved?1.19f:.64f))return;
                    plant.SampleWorkload();validBands++;dwell=0;
                    if(band<3){band++;return;}
                    rfRundown=true;End();SetPermit(false);Log("Sweep complete. Confirming physical amplifier rundown.");break;
            }
        }
        private void Pose(int p)
        {
            int group=p/12,az=p%12;
            angle=(group%2==0?az:11-az)*30;height=.18f+.34f*(group%3);pol=group<3?0:90;distance=.52f;
        }
        private void Probe(int i)
        {
            int row=i/4,col=i%4;
            probeX=(row%2==0?col:3-col)/3f;probeY=row/3f;probeZ=.14f+.08f*(row%2);
        }
        private void Begin(Mode next){mode=next;index=0;modeTime=dwell=0;}
        private void End(){mode=Mode.Idle;armed=false;rfArmed=false;}
        private void SetPermit(bool state)
        {
            if(lastPermit==state)return;
            lastPermit=state;contacts[0].SetState(state?SparkSwitchState.Closed:SparkSwitchState.Open);
        }
        private void Invalidate(){conducted=radiated=immune=rfArmed=false;}
        private void Trip(Fault cause)
        {
            latch=cause;ampAtTrip=plant.Amp;
            End();SetPermit(false);Invalidate();rfRundown=false;verified=faultDiagnosed=false;
            if(cause==Fault.Field)
            {
                trusted=fieldCal=false;sampleAge=.5f;sampleField=0;sequenceAtTrip=fieldSequence;
                Log("CRITICAL RF FIELD FEEDBACK LOST · amplifier inhibited, shutter closing.");
            }
            else Log($"RF SAFE HOLD · {cause}.");
        }
        private void Award()
        {
            if(stage>=Gates.Length||!bindings.SetRuntimeComponentBinding(Gates[stage],source))return;
            stage++;awardDwell=0;manager.QueueEvaluation();Log($"Objective {stage}/6 verified.");
        }
        private void Log(string message)=>latestEvent=$"{elapsed:0.0}s · {message}";

        public void Power(){power=!power;source.SetOutput(power,default);}
        public void Door(){if(Ready&&Idle&&plant.RfSafe&&!rfArmed){doorOpen=!doorOpen;Log(doorOpen?"Chamber opening with RF isolated.":"Chamber closing. Wait for the hardwired interlock.");}}
        public void CalMotion(){if(Healthy&&Idle&&Sealed&&!motionCal&&plant.RfSafe){angle=30;height=.61f;pol=90;distance=.63f;probeX=.2f;probeY=.28f;Begin(Mode.MotionCal);}}
        public void CalEmi(){if(Healthy&&Idle&&Sealed&&motionCal&&!emiCal&&plant.RfSafe)Begin(Mode.ReceiverCal);}
        public void CalField(){if(CanCalField)Begin(Mode.FieldCal);}
        public void Conduct()
        {
            if(!CanScan)return;
            conducted=immune=false;conductedWorst=0;Array.Clear(conductedTrace,0,conductedTrace.Length);
            Begin(Mode.Conducted);
        }
        public void Radiate()
        {
            if(!CanScan)return;
            radiated=immune=false;radiatedWorst=0;completedPoses=0;
            Array.Clear(radiatedTrace,0,radiatedTrace.Length);Array.Clear(radiation,0,radiation.Length);
            Pose(0);Begin(Mode.Radiated);
        }
        public void Near()
        {
            if(!CanScan)return;
            nearComplete=false;nearSamples=0;highH=highE=0;Probe(0);Begin(Mode.Near);
        }
        public void Harness(){if(CanHarness){route=true;Invalidate();Log("Rerouting shielded harness and closing its ferrite.");}}
        public void Gasket(){if(CanGasket){gasket=true;Invalidate();Log("Compressing shield gasket at the leaking seam.");}}
        public void Filter(){if(CanRepair&&!filter){filter=true;Invalidate();Log("Fitting LC and feedthrough network to the power path.");}}
        public void Sensor()
        {
            if(!CanSensor)return;
            sensor=true;desense=false;Invalidate();Log("Fitting sensor filter and checking the input latency.");
        }
        public void ArmRf(){if(CanArmRf){rfArmed=true;Log("W119, door, E-stop and output isolation proved before arming.");}}
        public void Immunity()
        {
            if(!CanExposure)return;
            band=0;validBands=0;immune=false;rfArmed=false;
            Begin(Mode.Exposure);Log("Protected four-band immunity sweep started.");
        }
        public void Stop()
        {
            if(mode!=Mode.Exposure&&!rfArmed)return;
            End();SetPermit(false);immune=false;rfRundown=false;Log("RF stop requested. Shutter closing independently.");
        }
        public void HarnessFault()
        {
            if(!CanHarnessFault)return;
            route=false;disturbance=Repair.Harness;diagnosedFault=nearComplete=false;Invalidate();Log("Cable moved into a high common-mode route.");
        }
        public void SeamFault()
        {
            if(!CanSeamFault)return;
            gasket=false;disturbance=Repair.Seam;diagnosedFault=nearComplete=false;Invalidate();Log("Shield seam opened by a gasket compression loss.");
        }
        public void SensorFault(){if(CanSensorFault){desense=true;Log("Virtual RF sensor desense increases pickup.");}}
        public void Diagnose()
        {
            if(!CanDiagnose)return;
            if(latch!=Fault.None){faultDiagnosed=true;Log(latch==Fault.Sensor?"Live ADC error exceeded the protected command range.":"Safe-hold diagnostics recorded.");return;}
            if(disturbance!=Repair.None)
            {
                diagnosedFault=disturbance==Repair.Harness?highH>.29f:highE>.29f;
                Log(diagnosedFault?"Near-field scan identified the disturbed path.":"Path evidence below diagnostic threshold.");return;
            }
            diagnosed=highH>.29f&&highE>.29f;
            Log(diagnosed?"Located converter/harness magnetic and seam electric paths.":"Repeat near-field scan to locate both paths.");
        }
        public void Verify(){if(CanVerify){verified=true;Log("Fresh field, door interlock, safe amplifier and EUT checks verified.");}}
        public void Reset(){if(CanReset){feedbackReset|=latch==Fault.Field&&feedbackObserved;latch=Fault.None;Log("Explicit RF safety reset. Repeat all scans and exposure.");}}
        public void Arm(){if(CanArm){armed=true;Log("Remove W119 with Undo Last Wire during this live exposure.");}}
        public void EStop(){emergency.SetState(emergency.IsClosed?SparkSwitchState.Open:SparkSwitchState.Closed);if(emergency.IsOpen&&latch==Fault.None)Trip(Fault.EStop);}
        public void Certify(){if(CanCertify)certify=true;}

        private string Status()
        {
            if(stage==6)return "PROJECT SPARK EMC QUALIFIED";
            if(!power)return "Enable 5 V CONTROL and complete wiring.";
            if(latch!=Fault.None)
            {
                if(!faultDiagnosed)return $"SAFE HOLD · {latch} · select DIAGNOSE.";
                if(!Ready)return latch==Fault.Field?"CRITICAL RF FIELD FEEDBACK LOST · reconnect W119.":"Restore the independent safety circuit.";
                if(!plant.RfSafe)return "Waiting for actual amplifier rundown and shutter closure.";
                if(latch==Fault.Field&&!fieldCal)return "Recalibrate the field probe after reconnecting W119.";
                if(latch==Fault.Sensor&&desense)return "Fit the protected sensor filter.";
                return verified?"Select RESET SAFETY.":"Verify shutter, door and fresh field evidence.";
            }
            if(!Ready)return wiring??"Check the 5 V rails and RF emergency stop.";
            if(armed)return "Live W119 test · use Undo Last Wire to remove trusted field feedback.";
            if(CanArm)return "Select ARM W119 TEST while exposure is active.";
            if(CanSensorFault)return "Inject SENSOR DESENSE during live exposure.";
            if(!Idle)return $"ACQUIRING · {mode} · {completedPoses}/72 antenna poses.";
            if(rfRundown)return "Verifying physical amplifier rundown.";
            if(CanCertify)return "Fresh emissions and immunity qualified · select CERTIFY.";
            if(!Sealed)return "Close the chamber and wait for the actual door interlock.";
            if(stage<2)return "Calibrate motion, receiver and trusted field probe.";
            if(CanHarnessFault)return "Inject COMMON-MODE FAULT to diagnose the cable path.";
            if(CanSeamFault)return "Inject SEAM GAP to diagnose leakage.";
            if(disturbance!=Repair.None)return diagnosedFault?"Repair the diagnosed path.":"Map NEAR-FIELD E/H, then select DIAGNOSE.";
            if(!diagnosed)return "Acquire baseline conducted, radiated and near-field evidence, then diagnose.";
            if(!route||!gasket||!filter||!sensor)return "Repair harness, gasket, power filter and sensor path.";
            if(!conducted||!radiated)return "Repeat conducted and all 72 radiated measurements.";
            return rfArmed?"Run the protected immunity sweep.":"Arm the RF source after safe-state checks.";
        }
        private void Present()
        {
            statusText.text=Status();
            meterText.text=$"COND {conductedWorst:0.000}   ·   RAD {radiatedWorst:0.000}   ·   FIELD {(trusted?sampleField.ToString("0.000"):"STALE")}   ·   AMP {plant.Amp:0.00}";
            detailText.text=$"AZ {plant.Azimuth:0}° · HEIGHT {plant.Height:0.00} · POL {plant.Polarization:0}°   /   ADC {plant.AdcError:0.000} · JITTER {plant.Jitter:0.000} · OPT {plant.OpticalLoss:0.000}";
            calibrationText.text=$"{latestEvent}  |  H {highH:0.00} / E {highE:0.00} · BANDS {validBands}/4";
            if(controllerDisplay)controllerDisplay.text=trusted?$"FIELD {sampleField:0.00}":"FIELD STALE";
            if(qualityDisplay)qualityDisplay.text=$"{completedPoses}/72 POSES · {(plant.RfSafe?"RF SAFE":"RF ACTIVE")}";
            powerButton.interactable=stage<6;doorButton.interactable=Ready&&Idle&&plant.RfSafe&&!rfArmed;
            calMotionButton.interactable=Healthy&&Idle&&Sealed&&!motionCal&&plant.RfSafe;
            calEmiButton.interactable=Healthy&&Idle&&Sealed&&motionCal&&!emiCal&&plant.RfSafe;
            calFieldButton.interactable=CanCalField;conductButton.interactable=CanScan;radiateButton.interactable=CanScan;nearButton.interactable=CanScan;
            harnessButton.interactable=CanHarness;gasketButton.interactable=CanGasket;filterButton.interactable=CanRepair&&!filter;sensorButton.interactable=CanSensor;
            armRfButton.interactable=CanArmRf;immunityButton.interactable=CanExposure;stopButton.interactable=mode==Mode.Exposure||rfArmed;
            harnessFaultButton.interactable=CanHarnessFault;seamFaultButton.interactable=CanSeamFault;sensorFaultButton.interactable=CanSensorFault;
            diagnoseButton.interactable=CanDiagnose;verifyButton.interactable=CanVerify;resetButton.interactable=CanReset;armButton.interactable=CanArm;
            eStopButton.interactable=stage<6;certifyButton.interactable=CanCertify&&!certify;
            rig.Apply(plant,mode,sampleField,trusted,lastPermit,armRfButton.interactable,conducted,radiated,completedPoses,latch!=Fault.None,
                1-conductedWorst,1-radiatedWorst,Mathf.Max(highH,highE));
        }
    }
}




