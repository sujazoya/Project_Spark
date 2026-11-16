using System;
using System.Collections.Generic;
using ProjectSpark.Circuit;
using ProjectSpark.Visuals;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.Gameplay
{
    public sealed class SparkLevel81Process : MonoBehaviour
    {
        public static readonly string[] Gates={"L81_Wired","L81_ECC","L81_TMR","L81_Mission","L81_Disturbances","L81_Recertified"};
        public static readonly (int a,int b)[] Wires={(0,1),(2,3),(0,4),(2,5),(0,6),(2,7),(0,8),(2,9),(0,10),(2,11),(12,13),(14,15),(16,17),(18,19),(20,21),(22,23),(24,25),(26,27),(28,29),(30,31),(32,33),(34,35),(36,37),(38,39),(40,41),(42,43),(44,45),(46,47),(48,49),(50,51),(52,53),(54,55),(56,57),(58,59),(60,61),(62,63),(64,65),(66,67),(68,69),(70,71),(72,73),(74,75),(76,77),(78,79),(80,81),(82,83),(84,85),(86,87),(88,89),(90,91),(92,93),(94,95),(96,97),(98,99),(100,101),(102,103),(104,105),(106,107),(108,109),(110,111),(112,113),(112,114),(115,116),(117,118),(119,120),(121,122),(123,124),(125,126),(127,128),(129,130),(131,132),(133,134),(135,136),(137,138),(139,140),(141,142),(143,144),(130,145),(132,146),(130,147),(140,148),(142,149),(120,150),(150,151),(47,152),(49,153),(85,154),(87,155),(89,156),(151,157),(157,158),(158,159),(61,160),(161,162),(61,163),(70,164),(151,165),(109,166),(126,167),(151,168),(23,169),(25,170),(27,171),(83,172),(83,173),(173,174),(63,175),(65,176),(163,177),(172,178),(81,179),(111,180),(107,181),(130,182),(151,183),(171,184),(39,185),(27,186),(187,188),(188,189),(183,190),(171,191),(163,192),(175,193),(74,194),(180,195),(43,196),(149,197),(116,198),(198,199)};
        private const int StopInput=188,ControlReturn=3,EccSupply=4,EccReturn=5,Feedback=93,PermitWire=23;
        private static readonly string[] Phases={"READY","NAVIGATION ACQUISITION","AUTONOMOUS MANEUVER","DATA RETURN"};

        public SparkLevelLayout layout;
        public SparkPowerSupply source;
        public SparkSwitch[] contacts;
        public SparkSwitch emergency;
        public SparkTerminal[] pins;
        public SparkLevel81VisualRig rig;
        public SparkLevel81Trend plot;
        public TMP_Text controllerDisplay,qualityDisplay;
        public Text statusText,meterText,detailText,calibrationText;
        public Button powerButton,initButton,singleButton,scrubButton,tmrButton,laneButton,watchButton,opticalButton,missionButton,
            doubleButton,commonButton,packetsButton,diagnoseButton,restoreButton,revalidateButton,resetButton,armButton,eStopButton,certifyButton;

        private enum Fault { None, Wiring, Power, EStop, Memory, CommonMode, Packet, Watchdog, EccLink }
        private enum Job { None, Tmr, Lane, Common, Restore }
        private readonly SparkLevel81Plant plant=new();
        private readonly HashSet<(int,int)> wires=new();
        private readonly List<SparkCircuitConnection> edges=new();
        private LevelGamePlayManager manager;
        private SparkLevelLayoutLoader loader;
        private SparkLevelSceneBindings bindings;
        private Fault latch;
        private Job job;
        private int topology=-1,stage,phase,acceptedMissions,commandSnapshot,w094Sequence;
        private int heartbeatSequence,receivedSequence,receivedSyndrome,receivedQuarantine,firstProofCorrections;
        private uint ticks,challenge=17;
        private bool power,rails,safety,eccPower,wiredFeedback,permitVoltage,lastPermit,trusted,scrubbing,singleInjected;
        private bool tmrVerified,laneVerified,watchVerified,opticalVerified,diagnosed,restored,revalidated;
        private bool memoryPassed,commonPassed,packetPassed,armed,feedbackObserved,feedbackReset,certify,missionComplete;
        private float elapsed,accumulator,heartbeatAge=.5f,heartbeatClock,hold,phaseClock,jobClock,holdTravel;
        private string wiring,latestEvent="Wire the mission reliability workstation.";

        private bool Ready=>rails&&safety&&wiring==null;
        private bool Idle=>phase==0&&job==Job.None;
        private bool Healthy=>Ready&&latch==Fault.None;
        private bool CanInit=>Healthy&&Idle&&!plant.Initialized;
        private bool CanSingle=>Healthy&&Idle&&plant.Initialized&&!singleInjected;
        private bool CanScrub=>Healthy&&Idle&&singleInjected&&!scrubbing;
        private bool CanTmr=>Healthy&&Idle&&stage>=2&&trusted&&!tmrVerified;
        private bool CanLane=>Healthy&&Idle&&tmrVerified&&!laneVerified;
        private bool CanWatch=>Healthy&&Idle&&tmrVerified&&!watchVerified;
        private bool CanOptical=>Healthy&&Idle&&tmrVerified&&!opticalVerified;
        private bool AwaitsAward=>missionComplete&&(stage==3||stage==4&&memoryPassed&&commonPassed&&packetPassed);
        private bool CanMission=>Healthy&&Idle&&stage>=3&&trusted&&tmrVerified&&laneVerified&&watchVerified&&opticalVerified&&!AwaitsAward;
        private bool CanDouble=>Healthy&&Idle&&stage==4&&missionComplete&&!memoryPassed;
        private bool CanCommon=>Healthy&&Idle&&stage==4&&memoryPassed&&!commonPassed;
        private bool CanPackets=>Healthy&&Idle&&stage==4&&commonPassed&&!packetPassed;
        private bool CanRestore=>Ready&&Idle&&latch!=Fault.None&&diagnosed&&!restored&&holdTravel>.95f;
        private bool CanVerify=>Ready&&Idle&&latch!=Fault.None&&diagnosed&&restored&&!revalidated&&trusted;
        private bool CanReset=>Ready&&Idle&&latch!=Fault.None&&revalidated&&trusted&&holdTravel>.95f;
        private bool CanArm=>stage==5&&phase>0&&trusted&&lastPermit&&!armed&&!feedbackObserved;
        private bool CanCertify=>stage==5&&feedbackReset&&missionComplete&&acceptedMissions>=3&&Healthy&&trusted;

        private void Start()
        {
            manager=FindFirstObjectByType<LevelGamePlayManager>();loader=FindFirstObjectByType<SparkLevelLayoutLoader>();
            if(!manager||!loader||pins.Length!=200||contacts.Length!=1||!rig)
            {
                Debug.LogError("[LEVEL 81] Missing mission reliability references.",this);enabled=false;return;
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
            eccPower=power&&Rail(EccSupply,EccReturn);
            permitVoltage=power&&Rail(Wires[PermitWire].b,ControlReturn);
            if(!wiredFeedback&&trusted)
            {
                trusted=false;heartbeatAge=.5f;
            }
            if(latch==Fault.None&&!Idle&&!Ready)
            {
                bool feedbackOnly=rails&&safety&&!wiredFeedback&&wires.Count==Wires.Length-1;
                if(feedbackOnly)
                    for(int i=0;i<Wires.Length;i++)
                        if(i!=Feedback&&!Routed(i)){feedbackOnly=false;break;}
                feedbackObserved|=stage==5&&armed&&phase>0&&feedbackOnly;
                Trip(feedbackOnly?Fault.EccLink:!rails?Fault.Power:!safety?Fault.EStop:Fault.Wiring);
            }
            float rate=armed?.26f:stage==5&&!feedbackObserved&&phase>0?.55f:2.15f;
            accumulator+=Mathf.Min(Time.deltaTime,.18f)*rate;
            while(accumulator>=SparkLevel81Plant.Step){accumulator-=SparkLevel81Plant.Step;Tick(SparkLevel81Plant.Step);}
            Present();
        }
        private void Tick(float dt)
        {
            ticks++;elapsed+=dt;plant.Cool(dt);
            heartbeatAge+=dt;heartbeatClock+=dt;
            if(heartbeatClock>=.14f)
            {
                heartbeatClock=0;
                if(eccPower)heartbeatSequence++;
                if(eccPower&&wiredFeedback&&heartbeatSequence>receivedSequence)
                {
                    receivedSequence=heartbeatSequence;heartbeatAge=0;
                    receivedSyndrome=plant.LastSyndrome;receivedQuarantine=plant.QuarantineCount;
                }
            }
            trusted=eccPower&&wiredFeedback&&receivedSequence>0&&heartbeatAge<.31f;
            holdTravel=Mathf.MoveTowards(holdTravel,latch!=Fault.None?1:0,dt*1.8f);
            if(scrubbing&&eccPower)plant.Scrub(2);
            if(latch==Fault.None&&plant.QuarantineCount>0)
            {
                memoryPassed|=stage==4;Trip(Fault.Memory);
            }
            if(job!=Job.None)TickJob(dt);
            if(phase>0&&latch==Fault.None)TickMission(dt);
            bool goal=stage switch
            {
                0=>trusted,
                1=>trusted&&scrubbing&&plant.SingleVerified&&plant.Corrections>firstProofCorrections&&plant.ValidateMemory(),
                2=>trusted&&tmrVerified&&laneVerified&&watchVerified&&opticalVerified,
                3=>missionComplete&&acceptedMissions>=1,
                4=>memoryPassed&&commonPassed&&packetPassed&&plant.CheckpointVerified&&missionComplete&&acceptedMissions>=2,
                5=>CanCertify&&certify,
                _=>false
            };
            hold=Healthy&&goal?hold+dt:0;
            if(hold>.30f)Award();
            if(plot!=null)plot.Show(plant,trusted,phase,holdTravel);
        }
        private void TickJob(float dt)
        {
            jobClock+=dt;
            if(!Ready)return;
            int faults=job==Job.Lane?1:job==Job.Common?2:0;
            if(!plant.Compute(1,dt,faults)){if(jobClock>1.2f)Trip(Fault.Watchdog);return;}
            switch(job)
            {
                case Job.Tmr:
                    tmrVerified=plant.VoterValid;
                    Log("Three independently computed outputs agree with the reference.");break;
                case Job.Lane:
                    laneVerified=plant.VoterValid&&plant.FailedLane==0;
                    Log("One faulty lane was detected and masked by the majority.");break;
                case Job.Common:
                    commonPassed=!plant.VoterValid&&plant.Vote!=plant.Reference;
                    Trip(Fault.CommonMode);return;
                case Job.Restore:
                    restored=plant.VoterValid&&plant.ValidateMemory()&&plant.Retransmit(ticks)&&
                        plant.CheckWatchdog(++challenge,SparkLevel81Plant.WatchdogReply(challenge),.12f);
                    Log(restored?"Recovery computation, memory and retransmission checked.":"Restore could not validate all subsystems.");break;
            }
            job=Job.None;
        }
        private void TickMission(float dt)
        {
            phaseClock+=dt;
            if(!trusted){Trip(Fault.EccLink);return;}
            if(phaseClock<.20f&&!plant.PacketFresh(ticks))
                plant.Accept(plant.Make(ticks),ticks);
            if(ticks%9==0)plant.Accept(plant.Make(ticks),ticks);
            if(!plant.PacketFresh(ticks)){Trip(Fault.Packet);return;}
            bool vote=plant.Compute(phase,dt,0)&&plant.VoterValid;
            bool wd=plant.CheckWatchdog(++challenge,SparkLevel81Plant.WatchdogReply(challenge),phaseClock*.24f);
            bool safe=vote&&wd&&receivedQuarantine==0&&plant.QuarantineCount==0&&plant.PacketFresh(ticks)&&holdTravel<.03f;
            SetPermit(safe);
            if(phaseClock>1.7f){Trip(Fault.Watchdog);return;}
            if(phaseClock<.98f||!safe||!permitVoltage||!contacts[0].IsClosed)return;
            plant.RecordCommand(true);
            Log(Phases[phase]+" committed through the verified gate.");
            if(++phase==4)
            {
                phase=0;armed=false;SetPermit(false);
                missionComplete=true;acceptedMissions++;
            }
            else BeginPhase();
        }
        private void BeginPhase(){phaseClock=0;plant.StartComputation();SetPermit(false);}
        private void SetPermit(bool value)
        {
            if(lastPermit==value)return;
            lastPermit=value;contacts[0].SetState(value?SparkSwitchState.Closed:SparkSwitchState.Open);
        }
        private void Trip(Fault cause)
        {
            latch=cause;
            if(phase>0||job==Job.Common)plant.RecordCommand(false);
            phase=0;job=Job.None;armed=missionComplete=false;SetPermit(false);
            diagnosed=restored=revalidated=false;
            if(cause==Fault.EccLink)
            {
                w094Sequence=receivedSequence;trusted=false;heartbeatAge=.5f;
                Log("ECC HEARTBEAT STALE · command relay open, mission safe hold.");
            }
            else Log($"MISSION SAFE HOLD · {cause}.");
        }
        private void Award()
        {
            if(stage>=Gates.Length||!bindings.SetRuntimeComponentBinding(Gates[stage],source))return;
            stage++;hold=0;manager.QueueEvaluation();Log($"Objective {stage}/6 verified.");
        }
        private void StartJob(Job next){job=next;jobClock=0;plant.StartComputation();}
        private void Log(string message)=>latestEvent=$"{elapsed:0.0}s · {message}";

        public void Power(){power=!power;source.SetOutput(power,default);}
        public void Init(){if(CanInit){plant.Initialize();Log("Both ECC banks encoded with 256 protected words.");}}
        public void Single()
        {
            if(!CanSingle)return;
            firstProofCorrections=plant.Corrections;singleInjected=true;plant.Inject(false);Log("One bit flipped in a Bank A codeword.");
        }
        public void Scrub(){if(CanScrub){scrubbing=true;Log("Background scrubber active; corrected words verified and written back.");}}
        public void Tmr(){if(CanTmr)StartJob(Job.Tmr);}
        public void Lane(){if(CanLane){plant.Storm=.75f;StartJob(Job.Lane);}}
        public void Watch()
        {
            if(!CanWatch)return;
            uint key=++challenge,answer=SparkLevel81Plant.WatchdogReply(key);
            bool stopped=!plant.CheckWatchdog(key,answer^1,.10f)&&!plant.CheckWatchdog(key,answer,.61f);
            watchVerified=stopped&&plant.CheckWatchdog(key,answer,.11f);
            Log(watchVerified?"Watchdog rejected a bad reply and a timeout, then passed a fresh challenge.":"Watchdog proof failed.");
        }
        public void Optical()
        {
            if(!CanOptical)return;
            opticalVerified=plant.Accept(plant.Make(ticks),ticks)==SparkLevel81Plant.Packet.Accepted;
            Log("Optical telemetry accepted with current CRC and sequence.");
        }
        public void Mission()
        {
            if(!CanMission)return;
            missionComplete=false;commandSnapshot=plant.Committed;phase=1;BeginPhase();Log("Starting fresh three-phase mission.");
        }
        public void Double(){if(CanDouble){plant.Inject(true);plant.Scrub(1);Log("Two bits flipped in the same Bank B codeword.");}}
        public void Common(){if(CanCommon){plant.Storm=.9f;StartJob(Job.Common);}}
        public void Packets()
        {
            if(!CanPackets)return;
            packetPassed=plant.PacketDisturbance(ticks);Trip(Fault.Packet);
        }
        public void Diagnose()
        {
            if(latch==Fault.None||diagnosed)return;
            diagnosed=true;
            Log(latch switch
            {
                Fault.Memory=>"Double-bit syndrome detected; Bank B page is quarantined.",
                Fault.CommonMode=>"Two wrong lanes agreed. Independent reference blocked the vote.",
                Fault.Packet=>"CRC failure, sequence gap, duplicate, replay and expired frame classified separately.",
                Fault.EccLink=>"Trusted heartbeat is missing; cached syndrome is stale.",
                _=>"Interlock and mission timing diagnostics recorded."
            });
        }
        public void Restore()
        {
            if(!CanRestore)return;
            if(!plant.Initialized)plant.Initialize();
            if(latch==Fault.Memory&&!plant.RestoreVault()){Log("Checkpoint identity or CRC failed.");return;}
            plant.Recovery=1;StartJob(Job.Restore);
        }
        public void Revalidate()
        {
            if(!CanVerify)return;
            revalidated=plant.ValidateMemory()&&plant.VoterValid&&plant.Watchdog&&plant.PacketFresh(ticks)&&
                (latch!=Fault.EccLink||receivedSequence>w094Sequence);
            if(!revalidated&&plant.Retransmit(ticks)&&plant.ValidateMemory()&&plant.VoterValid&&plant.Watchdog)
                revalidated=latch!=Fault.EccLink||receivedSequence>w094Sequence;
            Log(revalidated?"Fresh ECC, memory, reference, watchdog and packet evidence verified.":"Recovery remains unqualified.");
        }
        public void Reset(){if(CanReset){feedbackReset|=latch==Fault.EccLink&&feedbackObserved;latch=Fault.None;Log("Explicit safety reset. Start a fresh three-phase mission.");}}
        public void Arm(){if(CanArm){armed=true;Log("Remove W094 using Undo Last Wire during the live mission.");}}
        public void EStop(){emergency.SetState(emergency.IsClosed?SparkSwitchState.Open:SparkSwitchState.Closed);if(emergency.IsOpen&&latch==Fault.None)Trip(Fault.EStop);}
        public void Certify(){if(CanCertify)certify=true;}

        private string Status()
        {
            if(stage==6)return "CERTIFIED · navigation, maneuver and data return completed safely.";
            if(!power)return "Enable 5 V CONTROL and complete wiring.";
            if(latch!=Fault.None)
            {
                if(!diagnosed)return $"SAFE HOLD · {latch} · select DIAGNOSE.";
                if(!Ready)return latch==Fault.EccLink?"ECC HEARTBEAT STALE · reconnect W094.":"Restore the electrical safety interlock.";
                if(job==Job.Restore)return "Validating checkpoint, computation and retransmission.";
                if(!restored)return "Select RESTORE / RECHECK.";
                return revalidated?"Select RESET SAFETY.":"Select VERIFY RECOVERY.";
            }
            if(!Ready)return wiring??"Check 5 V rails and the safe-hold loop.";
            if(armed)return "Live W094 test · use Undo Last Wire to remove the ECC heartbeat.";
            if(CanArm)return "Select ARM W094 TEST during the live command phase.";
            if(phase>0)return Phases[phase]+" · protected mission running.";
            if(job!=Job.None)return "Three lanes computing and checking the reference.";
            if(CanCertify)return "Fresh mission passed · select CERTIFY.";
            if(!plant.Initialized||!scrubbing)return "Initialize both banks, inject one bit flip, then scrub and verify.";
            if(stage<=2)return "Prove three lanes, mask one faulty lane, and check watchdog and telemetry.";
            if(CanDouble)return "Inject a double-bit upset into one codeword.";
            if(CanCommon)return "Test unsafe agreement between two faulty lanes.";
            if(CanPackets)return "Test corrupt, missing, expired and replayed telemetry.";
            return "Start a fresh three-phase mission.";
        }
        private void Present()
        {
            statusText.text=Status();
            meterText.text=$"ECC {(trusted?"FRESH":"STALE")}   ·   SCRUB {plant.Inspected}   ·   CORRECTED {plant.Corrections}   ·   QUARANTINE {plant.QuarantineCount}";
            detailText.text=$"VOTE {(plant.VoterValid?"VALID":"HOLD")}   ·   WATCHDOG {(plant.Watchdog?"READY":"WAIT")}   ·   COMMITS {plant.Committed}   ·   BLOCKED {plant.Blocked}";
            calibrationText.text=$"{latestEvent}   |   CRC {plant.CorruptFrames} · REPLAY {plant.Replays} · GAP {plant.Gaps} · DUP {plant.Duplicates}";
            if(controllerDisplay)controllerDisplay.text=trusted?$"ECC #{receivedSequence} · {receivedSyndrome:X2}":"ECC STALE";
            if(qualityDisplay)qualityDisplay.text=latch==Fault.None?Phases[phase]:"MISSION SAFE HOLD";
            powerButton.interactable=stage<6;initButton.interactable=CanInit;singleButton.interactable=CanSingle;scrubButton.interactable=CanScrub;
            tmrButton.interactable=CanTmr;laneButton.interactable=CanLane;watchButton.interactable=CanWatch;opticalButton.interactable=CanOptical;
            missionButton.interactable=CanMission;doubleButton.interactable=CanDouble;commonButton.interactable=CanCommon;packetsButton.interactable=CanPackets;
            diagnoseButton.interactable=latch!=Fault.None&&!diagnosed;restoreButton.interactable=CanRestore;revalidateButton.interactable=CanVerify;
            resetButton.interactable=CanReset;armButton.interactable=CanArm;eStopButton.interactable=stage<6;certifyButton.interactable=CanCertify&&!certify;
            rig.Apply(plant,trusted,phase,elapsed,holdTravel,latch!=Fault.None,armed);
        }
    }
}




