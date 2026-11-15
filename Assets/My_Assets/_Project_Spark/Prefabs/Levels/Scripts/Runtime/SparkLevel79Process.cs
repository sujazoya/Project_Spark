using System;
using System.Collections.Generic;
using ProjectSpark.Circuit;
using ProjectSpark.Visuals;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Cycle=ProjectSpark.Gameplay.SparkLevel79Plant.Cycle;
using Disturbance=ProjectSpark.Gameplay.SparkLevel79Plant.Disturbance;

namespace ProjectSpark.Gameplay
{
    public sealed class SparkLevel79Process : MonoBehaviour
    {
        public static readonly string[] Gates={"L79_Wired","L79_Commissioned","L79_FirstPair","L79_Qualified","L79_Disturbances","L79_Recertified"};
        public static readonly (int a,int b)[] Wires={(0,1),(2,3),(0,4),(2,5),(0,6),(2,7),(0,8),(2,9),(0,10),(2,11),(12,13),(14,15),(16,17),(18,19),(20,21),(22,23),(24,25),(26,27),(28,29),(30,31),(32,33),(34,35),(36,37),(38,39),(40,41),(42,43),(44,45),(46,47),(48,49),(50,51),(52,53),(54,55),(56,57),(58,59),(60,61),(62,63),(64,65),(64,66),(67,68),(69,70),(69,71),(72,73),(72,74),(75,76),(77,78),(79,80),(81,82),(83,84),(85,86),(87,88),(89,90),(91,92),(93,94),(95,96),(97,98),(99,100),(101,102),(103,104),(105,106),(107,108),(109,110),(111,112),(113,114),(115,116),(117,118),(119,120),(121,122),(123,124),(125,126),(127,128),(129,130),(131,132),(133,134),(135,136),(137,138),(139,140),(106,141),(141,142),(142,143),(143,144),(144,145),(145,146),(142,147),(144,148),(148,149),(149,150),(146,151),(140,152),(152,153),(148,154),(104,155),(155,156),(102,157),(157,158),(156,159),(104,160),(104,161),(161,162),(162,163),(163,164),(164,165),(165,166),(156,167),(166,168),(169,170),(170,171),(170,172),(86,173),(110,174),(29,175),(73,176),(176,177),(177,178),(29,179),(74,180),(156,181),(163,182),(166,183),(170,184),(70,185),(185,186),(166,187)};
        private const int StopInput=170,ControlReturn=3,LaserPower=8,LaserReturn=9,Feedback=36,PermitWire=53;
        private static readonly string[] Chips={"LOGIC","MEMORY","I/O","SENSOR"};

        public SparkLevelLayout layout;
        public SparkPowerSupply source;
        public SparkSwitch[] contacts;
        public SparkSwitch emergency;
        public SparkTerminal[] pins;
        public SparkLevel79VisualRig rig;
        public SparkLevel79Trend plot;
        public TMP_Text controllerDisplay,qualityDisplay;
        public Text statusText,meterText,detailText,calibrationText;
        public Button powerButton,homeButton,cameraButton,laserButton,waferButton,inspectButton,prepButton,loadButton,
            alignButton,approachButton,growButton,scanButton,annealButton,testButton,stopButton,discardButton,
            driftButton,chuckButton,particleButton,restoreButton,eStopButton,resetButton,armButton,verifyButton,certifyButton;

        private enum Fault { None, Power, Wiring, EStop, Vision, Chuck, Particle, Laser, Quality }
        private readonly SparkLevel79Plant plant=new();
        private readonly HashSet<(int,int)> wires=new();
        private readonly List<SparkCircuitConnection> edges=new();
        private SparkLevelLayoutLoader loader;
        private LevelGamePlayManager manager;
        private SparkLevelSceneBindings bindings;
        private Fault latch;
        private Disturbance disturbance;
        private Cycle cycle;
        private int topology=-1,stage,chip,scanned,acceptedBatches;
        private bool power,rails,safety,laserPower,wiredFeedback,permitVoltage,lastPermit;
        private bool homed,cameraCal,laserCal,verified,validLaser,baseLoaded,baseTransfer,inspected,prepared;
        private bool loaded,aligned,approached,bonded,annealed,counted,rejected;
        private bool driftPassed,chuckPassed,particlePassed,armed,feedbackObserved,feedbackReset,certify;
        private float elapsed,accumulator,hold,taskTime,cameraClock,laserClock,sampleAge,samplePosition;
        private float speedAtTrip,forceAtTrip,overlayAtTrip,chuckAtTrip,particlesAtTrip,maximumResistance,worstBer;
        private string wiring,latestEvent="Connect the hybrid bonding workbench.";

        private bool Ready=>rails&&safety&&wiring==null;
        private bool Idle=>cycle==Cycle.Idle&&plant.PoseMotion<.035f&&plant.Force<.025f;
        private bool Healthy=>Ready&&Idle&&latch==Fault.None&&!rejected;
        private bool PoseTrusted=>homed&&cameraCal&&validLaser;
        private bool PendingAward=>counted&&(stage==3||stage==4&&driftPassed&&chuckPassed&&particlePassed);
        private bool CanHome=>Ready&&Idle&&(!homed||latch==Fault.Laser&&!laserCal)&&latch is Fault.None or Fault.Laser;
        private bool CanCamera=>Ready&&Idle&&homed&&!cameraCal&&cameraClock<=0&&plant.CameraOffset<.06f&&latch==Fault.None;
        private bool CanLaser=>Ready&&Idle&&homed&&!laserCal&&laserPower&&wiredFeedback&&laserClock<=0&&latch is Fault.None or Fault.Laser;
        private bool CanBase=>Ready&&Idle&&latch==Fault.None&&homed&&(!baseLoaded||counted)&&!PendingAward&&cameraClock<=0&&laserClock<=0;
        private bool CanInspect=>Healthy&&baseLoaded&&!inspected;
        private bool CanPrep=>Healthy&&baseLoaded&&inspected&&!prepared;
        private bool CanLoad=>stage>=2&&Healthy&&prepared&&PoseTrusted&&!loaded&&scanned<4;
        private bool CanAlign=>Healthy&&PoseTrusted&&loaded&&!aligned;
        private bool CanApproach=>Healthy&&PoseTrusted&&loaded&&aligned&&!approached&&plant.WorstOverlay<.15f&&plant.Tilt<.045f;
        private bool CanBond=>Healthy&&PoseTrusted&&loaded&&approached&&!bonded&&plant.Gap is >.035f and <.13f&&plant.SurfaceReady;
        private bool CanScan=>Healthy&&loaded&&bonded;
        private bool CanAnneal=>Healthy&&scanned==4&&!annealed;
        private bool CanTest=>Healthy&&scanned==4&&annealed&&!counted&&plant.Temperature<.45f;
        private bool CanDrift=>stage==4&&cycle==Cycle.Align&&!driftPassed&&disturbance==Disturbance.None;
        private bool CanChuck=>stage==4&&cycle==Cycle.Approach&&plant.Gap<.84f&&driftPassed&&!chuckPassed&&disturbance==Disturbance.None;
        private bool CanParticle=>stage==4&&cycle==Cycle.Bond&&plant.Force>.29f&&chuckPassed&&!particlePassed&&disturbance==Disturbance.None;
        private bool CanArm=>stage==5&&cycle==Cycle.Approach&&plant.Motion>.30f&&plant.Gap>.21f&&!armed&&!feedbackObserved;
        private bool CanVerify=>Ready&&Idle&&latch==Fault.Laser&&homed&&validLaser&&!verified;
        private bool CanReset=>Ready&&Idle&&latch!=Fault.None&&disturbance==Disturbance.None&&
            (latch!=Fault.Laser||verified&&homed)&&plant.CameraOffset<.08f&&plant.ChuckResidual<.08f;
        private bool CanCertify=>stage==5&&feedbackReset&&counted&&acceptedBatches>=3&&Ready&&latch==Fault.None;

        private void Start()
        {
            manager=FindFirstObjectByType<LevelGamePlayManager>();loader=FindFirstObjectByType<SparkLevelLayoutLoader>();
            if(!manager||!loader||pins.Length!=188||contacts.Length!=1||!rig)
            {
                Debug.LogError("[LEVEL 79] Missing hybrid bonding references.",this);enabled=false;return;
            }
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
            wiredFeedback=Routed(Feedback);
            wiring=wires.Count==Wires.Length?null:"Remove extra wires and check the route chart.";
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
            laserPower=power&&Rail(LaserPower,LaserReturn);
            permitVoltage=power&&Rail(Wires[PermitWire].b,ControlReturn);
            if(stage==0&&Ready)Award();
            if((cycle!=Cycle.Idle||plant.PoseMotion>.035f)&&!Ready)
            {
                bool feedbackOnly=rails&&safety&&!wiredFeedback&&wires.Count==Wires.Length-1;
                if(feedbackOnly)
                    for(int i=0;i<Wires.Length;i++)
                        if(i!=Feedback&&!Routed(i)){feedbackOnly=false;break;}
                feedbackObserved|=stage==5&&armed&&cycle==Cycle.Approach&&plant.Motion>.24f&&feedbackOnly;
                if(latch==Fault.None||cycle==Cycle.Home)
                    Trip(feedbackOnly?Fault.Laser:!rails?Fault.Power:!safety?Fault.EStop:Fault.Wiring);
            }
            accumulator+=Mathf.Min(Time.deltaTime,.18f)*(armed?.28f:CanArm?.85f:2.15f);
            while(accumulator>=SparkLevel79Plant.Step)
            {
                accumulator-=SparkLevel79Plant.Step;
                Tick(SparkLevel79Plant.Step);
            }
            Present();
        }
        private void Tick(float dt)
        {
            elapsed+=dt;
            if(cycle!=Cycle.Idle)taskTime+=dt;
            TickCalibration(dt);
            bool sensor=laserPower&&wiredFeedback&&laserCal&&homed;
            sampleAge=sensor?sampleAge+dt:0;
            validLaser=sampleAge>.30f;
            samplePosition=validLaser?plant.Position[2]:0;
            bool permit=Ready&&latch==Fault.None&&PoseTrusted&&cycle is Cycle.Approach or Cycle.Bond;
            if(permit!=lastPermit){lastPermit=permit;contacts[0].SetState(permit?SparkSwitchState.Closed:SparkSwitchState.Open);}
            bool drive=Ready&&(latch==Fault.None||cycle==Cycle.Home&&latch==Fault.Laser)&&
                (cycle is not (Cycle.Approach or Cycle.Bond)||permit&&permitVoltage&&contacts[0].IsClosed);
            plant.Tick(dt,cycle,disturbance,chip,drive);
            if(latch==Fault.None)
            {
                if(disturbance==Disturbance.Vision&&plant.CameraOffset>.24f)
                {
                    overlayAtTrip=plant.WorstOverlay;driftPassed=true;cameraCal=false;Trip(Fault.Vision);
                }
                else if(disturbance==Disturbance.Chuck&&plant.ChuckResidual>.29f)
                {
                    chuckAtTrip=plant.ChuckResidual;chuckPassed=true;Trip(Fault.Chuck);
                }
                else if(disturbance==Disturbance.Particle&&plant.ParticleMass>.118f)
                {
                    particlesAtTrip=plant.ParticleMass;particlePassed=true;Trip(Fault.Particle);
                }
                else if(drive)CheckEndpoint();
            }
            else if(cycle==Cycle.Home&&drive)CheckEndpoint();
            bool goal=stage switch
            {
                1=>baseLoaded&&inspected&&prepared&&plant.SurfaceReady&&PoseTrusted,
                2=>scanned>=2,
                3=>counted&&acceptedBatches>=1,
                4=>driftPassed&&chuckPassed&&particlePassed&&counted&&acceptedBatches>=2,
                5=>CanCertify&&certify,
                _=>false
            };
            hold=Ready&&latch==Fault.None&&goal?hold+dt:0;
            if(hold>.34f)Award();
            if(plot!=null)plot.Show(plant,validLaser,chip);
        }
        private void TickCalibration(float dt)
        {
            bool Finished(ref float clock)
            {
                if(clock<=0)return false;
                clock=Mathf.Max(0,clock-dt);
                return clock==0;
            }
            if(Finished(ref cameraClock))
            {
                cameraCal=Ready&&homed&&plant.CameraOffset<.06f;
                Log(cameraCal?"Two-camera registration and fiducial agreement verified.":"Camera calibration aborted.");
            }
            if(Finished(ref laserClock))
            {
                laserCal=Ready&&homed&&wiredFeedback&&laserPower;
                sampleAge=0;Log(laserCal?"Laser zero established. Wait for fresh feedback.":"Laser calibration aborted.");
            }
        }
        private void CheckEndpoint()
        {
            switch(cycle)
            {
                case Cycle.Home when taskTime>.58f&&plant.Gap>.992f&&plant.PoseMotion<.026f&&
                    Mathf.Abs(plant.Position[0])+Mathf.Abs(plant.Position[1])<.017f:
                    StopDrive();homed=true;Log("Six axes homed and motion stopped.");break;
                case Cycle.Prep when plant.SurfaceReady:
                    StopDrive();prepared=true;Log("Clean and activated surfaces passed.");break;
                case Cycle.Transfer when taskTime>1.52f&&plant.PoseMotion<.035f&&plant.Gap>.98f:
                    StopDrive();
                    if(baseTransfer){baseTransfer=false;baseLoaded=true;Log("Base wafer seated in lower chuck.");}
                    else{loaded=true;Log($"{Chips[chip]} chiplet held over its pad field.");}
                    break;
                case Cycle.Align when taskTime>.55f&&plant.WorstOverlay<.052f&&plant.Tilt<.018f&&disturbance==Disturbance.None:
                    StopDrive();aligned=true;Log("Overlay, theta, tip and tilt qualified.");break;
                case Cycle.Approach when plant.Gap<.095f&&plant.PoseMotion<.12f&&disturbance==Disturbance.None:
                    if(armed){StopDrive();rejected=true;Log("Motion test window elapsed. Discard and retry the W037 test.");}
                    else{StopDrive();approached=true;Log("Safe approach gap reached. Contact may now be initiated.");}
                    break;
                case Cycle.Bond when plant.Coverage(chip)>=.997f&&disturbance==Disturbance.None:
                    StopDrive();bonded=true;Log("Bond front reached all four interface regions. Scan the bond.");break;
                case Cycle.Anneal when plant.ThermalDose>=1.05f:
                    StopDrive();annealed=plant.StackStress<.12f;Log("Qualified thermal dose reached. Wait for cooldown.");break;
                case Cycle.Scan when taskTime>=.92f&&plant.Gap>.987f&&plant.PoseMotion<.035f:
                    StopDrive();
                    if(plant.Coverage(chip)<.995f||plant.WorstVoid()>.005f){Trip(Fault.Quality);break;}
                    scanned++;loaded=aligned=approached=bonded=false;
                    Log($"Acoustic and IR scan passed for {Chips[chip]} ({scanned}/4).");break;
                case Cycle.Test when taskTime>=1.22f:
                    FinishElectrical();break;
            }
        }
        private void FinishElectrical()
        {
            maximumResistance=worstBer=0;
            float chain=0;
            bool routing=true;
            for(int i=0;i<4;i++)
            {
                float r=plant.Resistance(i);
                chain+=r;maximumResistance=Mathf.Max(maximumResistance,r);
                int next=(i+1)%4;
                routing&=plant.Coverage(i)>=.995f&&plant.Coverage(next)>=.995f&&plant.PathDelay(i,next)<1.12f;
                worstBer=Mathf.Max(worstBer,plant.LinkErrors(i,next));
            }
            StopDrive();
            if(maximumResistance>1.08f||chain>4.32f||!routing||worstBer>.002f){Trip(Fault.Quality);return;}
            counted=true;acceptedBatches++;Log("Kelvin, daisy chain, inter-chiplet links and timing passed.");
        }
        private void StartCycle(Cycle next){cycle=next;taskTime=0;}
        private void StopDrive()
        {
            cycle=Cycle.Idle;armed=false;lastPermit=false;contacts[0].SetState(SparkSwitchState.Open);
        }
        private void Trip(Fault cause)
        {
            latch=cause;speedAtTrip=plant.Motion;forceAtTrip=plant.Force;
            StopDrive();rejected=baseLoaded||baseTransfer;baseTransfer=false;
            cameraClock=laserClock=0;
            if(cause==Fault.Laser)
            {
                validLaser=laserCal=verified=homed=false;sampleAge=samplePosition=0;
                Log("LASER FEEDBACK INVALID · contact inhibited, controlled axis hold.");
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
        public void Home(){if(CanHome){homed=false;StartCycle(Cycle.Home);Log("Homing to a safe, raised reference pose.");}}
        public void Camera(){if(CanCamera)cameraClock=.69f;}
        public void Laser(){if(CanLaser)laserClock=.76f;}
        public void Wafer()
        {
            if(!CanBase)return;
            plant.NewAssembly();chip=scanned=0;
            baseLoaded=inspected=prepared=loaded=aligned=approached=bonded=annealed=counted=rejected=false;
            baseTransfer=true;StartCycle(Cycle.Transfer);Log("Transferring the prepared Level 78 base wafer.");
        }
        public void Inspect(){if(CanInspect){inspected=true;Log("Known-good dies, base topography and particles inspected.");}}
        public void Prep(){if(CanPrep)StartCycle(Cycle.Prep);}
        public void Load(){if(CanLoad){chip=scanned;plant.NewChiplet(chip);StartCycle(Cycle.Transfer);}}
        public void Align(){if(CanAlign)StartCycle(Cycle.Align);}
        public void Approach(){if(CanApproach)StartCycle(Cycle.Approach);}
        public void Grow(){if(CanBond)StartCycle(Cycle.Bond);}
        public void Scan(){if(CanScan)StartCycle(Cycle.Scan);}
        public void Anneal(){if(CanAnneal)StartCycle(Cycle.Anneal);}
        public void Test(){if(CanTest)StartCycle(Cycle.Test);}
        public void Stop()
        {
            if(cycle==Cycle.Idle)return;
            StopDrive();rejected=baseLoaded||baseTransfer;baseTransfer=false;Log("Cycle stopped. Discard interrupted assembly after rundown.");
        }
        public void Discard(){if(Idle&&latch==Fault.None&&(baseLoaded||rejected)&&!counted){baseLoaded=rejected=false;Log("Interrupted assembly quarantined.");}}
        public void Drift(){if(CanDrift){disturbance=Disturbance.Vision;Log("Dual-camera registration drift applied.");}}
        public void Chuck(){if(CanChuck){disturbance=Disturbance.Chuck;Log("Abnormal residual chuck force applied.");}}
        public void Particle(){if(CanParticle){disturbance=Disturbance.Particle;Log("Interface particle contamination applied.");}}
        public void Restore(){if(Ready&&latch!=Fault.None&&disturbance!=Disturbance.None){disturbance=Disturbance.None;Log("Disturbance corrected. Wait for a safe reset.");}}
        public void EStop(){emergency.SetState(emergency.IsClosed?SparkSwitchState.Open:SparkSwitchState.Closed);if(emergency.IsOpen&&latch==Fault.None)Trip(Fault.EStop);}
        public void Reset(){if(CanReset){feedbackReset|=latch==Fault.Laser&&feedbackObserved;latch=Fault.None;Log("Protection reset. Prepare a fresh assembly.");}}
        public void Arm(){if(CanArm){armed=true;Log("Undo Last Wire while the fine axis is moving.");}}
        public void Verify(){if(CanVerify){verified=true;Log("Fresh laser feedback verified at the homed reference.");}}
        public void Certify(){if(CanCertify)certify=true;}

        private string Status()
        {
            if(stage==6)return "CERTIFIED · four bonded chiplets and the interconnect network passed.";
            if(!power)return "Enable 5 V CONTROL and complete wiring.";
            if(latch==Fault.Laser)
            {
                if(!Ready)return "LASER FEEDBACK INVALID · reconnect W037.";
                if(cycle==Cycle.Home)return "Safe homing in progress.";
                if(!Idle)return "Fine axes braking to a controlled hold.";
                if(!homed)return "Reconnect verified · select HOME MOTION.";
                if(!laserCal)return "Select CAL LASER at the home position.";
                return verified?"Select RESET SAFETY.":"Select VERIFY LASER when fresh feedback arrives.";
            }
            if(latch!=Fault.None)return $"PROTECTION LATCHED · {latch} · restore and reset.";
            if(!Ready)return wiring??"Check 5 V rails and the E-Stop loop.";
            if(armed)return "Slow-motion approach · use Undo Last Wire to remove W037.";
            if(CanArm)return "Select ARM W037 TEST while the fine axis is moving.";
            if(CanDrift)return "Select CAMERA DRIFT during alignment.";
            if(CanChuck)return "Select CHUCK ANOMALY during approach.";
            if(CanParticle)return "Select PARTICLE FAULT while the contact front is growing.";
            if(cycle!=Cycle.Idle)return $"RUNNING · {cycle} · {Chips[chip]}";
            if(CanCertify)return "Fresh four-chiplet assembly passed · select CERTIFY.";
            if(rejected)return "Discard the interrupted assembly and start a fresh wafer.";
            if(!homed||!cameraCal||!laserCal)return "Home all axes and calibrate cameras and laser.";
            if(!baseLoaded||counted)return "Load the base wafer.";
            if(!inspected||!prepared)return "Inspect the incoming surfaces, then prepare them.";
            if(scanned==4)return annealed?"Wait for cooldown and test the four chiplets.":"Consolidate the four bonded interfaces.";
            if(!loaded)return $"Load {Chips[scanned]} chiplet.";
            if(!aligned)return "Align X, Y, Z, theta, tip and tilt.";
            if(!approached)return "Approach to the measured contact gap.";
            return bonded?"Scan interface coverage and voids.":"Initiate the controlled bond front.";
        }
        private void Present()
        {
            statusText.text=Status();
            meterText.text=$"OVERLAY {plant.WorstOverlay:0.000}   ·   TILT {plant.Tilt:0.000}   ·   GAP {plant.Gap:0.000}   ·   FORCE {plant.Force:0.00}";
            detailText.text=$"{Chips[chip]}   ·   COVER {plant.Coverage(chip):P2}   ·   VOIDS {plant.WorstVoid():P2}   ·   SCANNED {scanned}/4   ·   STRESS {plant.StackStress:0.000}";
            calibrationText.text=$"{latestEvent}   |   LASER {(validLaser?samplePosition.ToString("0.000"):"STALE")}";
            if(controllerDisplay)controllerDisplay.text=validLaser?$"Z {samplePosition:0.000}":"LASER STALE";
            if(qualityDisplay)qualityDisplay.text=$"{scanned}/4 BONDS · {plant.WorstVoid():P1} VOID";
            powerButton.interactable=stage<6;homeButton.interactable=CanHome;cameraButton.interactable=CanCamera;
            laserButton.interactable=CanLaser;waferButton.interactable=CanBase;inspectButton.interactable=CanInspect;
            prepButton.interactable=CanPrep;loadButton.interactable=CanLoad;alignButton.interactable=CanAlign;
            approachButton.interactable=CanApproach;growButton.interactable=CanBond;scanButton.interactable=CanScan;
            annealButton.interactable=CanAnneal;testButton.interactable=CanTest;stopButton.interactable=cycle!=Cycle.Idle;
            discardButton.interactable=Idle&&latch==Fault.None&&(baseLoaded||rejected)&&!counted;
            driftButton.interactable=CanDrift;chuckButton.interactable=CanChuck;particleButton.interactable=CanParticle;
            restoreButton.interactable=Ready&&latch!=Fault.None&&disturbance!=Disturbance.None;eStopButton.interactable=stage<6;
            resetButton.interactable=CanReset;armButton.interactable=CanArm;verifyButton.interactable=CanVerify;
            certifyButton.interactable=CanCertify&&!certify;
            rig.Apply(plant,baseTransfer?Cycle.Idle:cycle,taskTime,chip,baseLoaded?scanned:0,baseLoaded&&loaded,validLaser,lastPermit,latch!=Fault.None,emergency.IsOpen);
        }
    }
}



