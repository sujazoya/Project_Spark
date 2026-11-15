using System;
using System.Collections.Generic;
using ProjectSpark.Circuit;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.Gameplay
{
    public sealed class SparkLevel72Process : MonoBehaviour
    {
        public static readonly string[] Gates={"L72_Wired","L72_Commissioned","L72_Cycle","L72_Film","L72_Disturbances","L72_Recertified"};
        public static readonly (int a,int b)[] Wires={(0,1),(2,3),(0,4),(2,5),(0,6),(2,7),(0,8),(2,9),(0,10),(2,11),(12,13),(14,15),(16,17),(18,19),(20,21),(22,23),(24,25),(26,27),(28,29),(30,31),(32,33),(34,35),(36,37),(38,39),(40,41),(42,43),(44,45),(46,47),(48,49),(50,51),(52,53),(54,55),(56,57),(58,59),(60,61),(62,63),(64,65),(66,67),(68,69),(70,71),(72,73),(74,75),(76,77),(78,79),(80,81),(82,83),(84,85),(86,87),(88,89),(90,91),(92,93),(94,95),(96,97),(98,99),(100,101),(102,103),(104,105),(106,107),(105,108),(106,109),(23,110),(33,111),(106,112),(110,113),(41,114),(51,115),(103,116),(39,117),(106,118),(59,119),(61,120),(87,121),(93,122),(39,123),(113,124),(110,125),(71,126),(73,127),(75,128),(118,129),(130,131),(131,132),(41,133),(41,134),(101,135),(23,136),(117,137),(75,138),(103,139),(29,140),(59,141),(118,142),(87,143),(144,145)};
        private const int StopInput=131, ControlReturn=3, PurgePower=10, PurgeReturn=11, Feedback=93, PermitWire=24;
        public SparkLevelLayout layout;
        public SparkPowerSupply source;
        public SparkSwitch[] contacts;
        public SparkSwitch emergency;
        public SparkTerminal[] pins;
        public SparkLevel72VisualRig rig;
        public SparkLevel72Trend plot;
        public TMP_Text controllerDisplay,qualityDisplay;
        public Text statusText,meterText,detailText,calibrationText;
        public Button windowButton,powerButton,calPressureButton,calFlowButton,calPurgeButton,calHeatButton,isolationButton,backingButton,turboButton,
            loadButton,clampButton,heatButton,pressurePidButton,conditionButton,doseAButton,purgeAButton,doseBButton,purgeBButton,
            autoButton,metrologyButton,stopButton,restrictionButton,driftButton,stuckButton,repairButton,eStopButton,resetButton,
            armButton,recalButton,verifyButton,certifyButton;

        private enum Phase { Idle, DoseA, PurgeA, DoseB, PurgeB }
        private enum Fault { None, Power, Wiring, EStop, Purge, Heat, Valve, Feedback }
        private enum Scenario { None, Restriction, Heater, Valve }
        private readonly SparkLevel72Plant plant=new();
        private readonly HashSet<(int,int)> wires=new();
        private readonly List<SparkCircuitConnection> edges=new();
        private LevelGamePlayManager manager;
        private SparkLevelLayoutLoader loader;
        private SparkLevelSceneBindings bindings;
        private int topology=-1, stage, cycles, manualCycles, acceptedBatches;
        private bool power,rails,safety,purgePower,wiredPurge,permitVoltage,lastPermit;
        private bool calPressure,calFlow,calPurge,calHeat,validPurge,recalibrated,flowVerified;
        private bool backing,turbo,isolation,loaded,clamped,heat,condition,autoPressure,automatic,measured,certify;
        private bool restrictionPassed,driftPassed,valvePassed,repaired,armed,feedbackObserved,feedbackReset;
        private float elapsed,accumulator,hold,age,phaseClock,loadClock,integral,pressure=500,flow,metrology,settled;
        private float maxResidual,lowestFlow,heaterSpan,stuckPosition;
        private string wiring,latestEvent="Connect the deposition controls and enable 5 V.";
        private Phase phase;
        private Fault latch;
        private Scenario scenario;

        private float doseWindow=1.16f;
        private bool Ready=>rails&&safety&&wiring==null;
        private bool Pumping=>backing&&turbo&&isolation&&plant.Backing>.87f&&plant.Turbo>.87f&&plant.Isolation>.89f;
        private bool Thermal=>calHeat&&heat&&Mathf.Abs(plant.Temperature[0]-48)<2.5f&&plant.Span<2.1f;
        private bool Stable=>Ready&&latch==Fault.None&&Pumping&&loaded&&clamped&&heat&&condition&&autoPressure&&
            calPressure&&calFlow&&calHeat&&plant.Gate<.05f&&pressure is >4.7f and <11.5f;
        private bool CanLoad=>Ready&&Pumping&&!condition&&phase==Phase.Idle&&latch==Fault.None&&loadClock<=0&&
            plant.Pressure<2.3f&&Mathf.Abs(plant.Pressure-plant.Lock)<1.9f&&(!loaded||stage>=4);
        private bool CanTurbo=>Ready&&calPressure&&backing&&!turbo&&plant.Backing>.65f&&plant.Pressure<145;
        private bool CanDoseA=>stage>=2&&Stable&&Thermal&&validPurge&&permitVoltage&&!measured&&cycles<120&&phase==Phase.Idle;
        private bool CanPurgeA=>phase==Phase.DoseA&&phaseClock>=doseWindow+.02f&&latch==Fault.None;
        private bool ClearA=>validPurge&&flow>1.45f&&plant.ResidualA<.022f&&plant.ValveA<.025f;
        private bool ClearB=>validPurge&&flow>1.45f&&plant.ResidualB<.022f&&plant.ValveB<.025f;
        private bool CanDoseB=>phase==Phase.PurgeA&&phaseClock>=.40f&&ClearA&&Stable&&permitVoltage&&!armed;
        private bool CanPurgeB=>phase==Phase.DoseB&&phaseClock>=doseWindow+.02f&&latch==Fault.None;
        private bool CanMetrology=>Ready&&latch==Fault.None&&phase==Phase.Idle&&cycles==120&&!measured&&settled>.45f;
        private bool CanTest=>stage==4&&Stable&&Thermal&&validPurge&&permitVoltage&&phase==Phase.Idle&&scenario==Scenario.None&&!measured&&cycles==0;
        private bool CanRecal=>Ready&&latch==Fault.Feedback&&!recalibrated;
        private bool CanVerify=>Ready&&latch==Fault.Feedback&&validPurge&&!flowVerified&&flow>1.7f&&plant.ResidualA<.07f&&plant.ResidualB<.07f;
        private bool CanReset=>Ready&&latch!=Fault.None&&plant.ValveA<.04f&&plant.ValveB<.04f&&plant.ResidualA<.085f&&plant.ResidualB<.085f&&
            (latch==Fault.Feedback?flowVerified:scenario==Scenario.None);
        private bool CanCertify=>stage==5&&feedbackReset&&measured&&acceptedBatches>=3&&Ready&&latch==Fault.None;

        private void Start()
        {
            manager=FindFirstObjectByType<LevelGamePlayManager>(); loader=FindFirstObjectByType<SparkLevelLayoutLoader>();
            if(!manager||!loader||pins.Length!=146||contacts.Length!=1||!rig){Debug.LogError("[LEVEL 72] Missing ALD workstation references.",this);enabled=false;return;}
            source.SetOutput(false,default);
        }
        private static (int,int) Pair(int a,int b)=>(Math.Min(a,b),Math.Max(a,b));
        private bool Routed(int i)=>wires.Contains(Pair(Wires[i].a,Wires[i].b));
        private void ReadTopology(SparkCircuitSystem circuit)
        {
            edges.Clear(); wires.Clear(); circuit.CopyConnections(edges);
            foreach(var edge in edges)
            {
                if(edge.Kind!=SparkConnectionKind.Wire)continue;
                int a=Array.IndexOf(pins,edge.A),b=Array.IndexOf(pins,edge.B);
                if(a>=0||b>=0)wires.Add(Pair(a,b));
            }
            wiredPurge=Routed(Feedback);
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
            purgePower=power&&Rail(PurgePower,PurgeReturn);
            permitVoltage=power&&Rail(Wires[PermitWire].b,ControlReturn);
            if(stage==0&&Ready) Award();
            if(latch==Fault.None&&(phase!=Phase.Idle||armed||loadClock>0)&&!Ready)
            {
                bool feedback=rails&&safety&&!wiredPurge&&wires.Count==Wires.Length-1;
                if(feedback)
                    for(int i=0;i<Feedback;i++)if(!Routed(i)){feedback=false;break;}
                feedbackObserved|=stage==5&&armed&&feedback;
                Trip(feedback?Fault.Feedback:!rails?Fault.Power:!safety?Fault.EStop:Fault.Wiring);
            }
            accumulator+=Mathf.Min(Time.deltaTime,.18f)*(automatic?26f:2.3f);
            for(int steps=0;accumulator>=SparkLevel72Plant.Step&&steps<128;steps++)
            {
                accumulator-=SparkLevel72Plant.Step;
                Tick(SparkLevel72Plant.Step);
            }
            Present();
        }
        private void Tick(float dt)
        {
            elapsed+=dt;phaseClock+=dt;
            if(loadClock>0)
            {
                loadClock+=dt;
                if(loadClock>2.1f){loadClock=0;loaded=true;plant.NewWafer();cycles=0;measured=false;Log("Wafer seated. Clamp the chuck.");}
            }
            metrology=Mathf.Max(0,metrology-dt);
            bool signal=purgePower&&wiredPurge&&calPurge&&(latch!=Fault.Feedback||recalibrated);
            age=signal?age+dt:0;
            validPurge=age>.40f;
            flow=validPurge?Mathf.Lerp(flow,plant.PurgeFlow,dt/.14f):0;
            pressure=Mathf.Lerp(pressure,plant.Pressure,dt/.17f);
            bool powered=rails&&safety&&(Ready||latch==Fault.Feedback);
            bool permit=Stable&&validPurge;
            if(permit!=lastPermit)
            {
                lastPermit=permit;contacts[0].SetState(permit?SparkSwitchState.Closed:SparkSwitchState.Open);
            }
            bool gas=permit&&permitVoltage&&contacts[0].IsClosed;
            float error=pressure-SparkLevel72Plant.PressureTarget;
            float throttle=1;
            if(condition&&autoPressure)
            {
                float target=.085f+.048f*error+integral*.056f;
                if(target is >.035f and <.95f||target<=.035f&&error>0||target>=.95f&&error<0)
                    integral=Mathf.Clamp(integral+error*dt,-4f,11f);
                throttle=Mathf.Clamp(.085f+.048f*error+integral*.056f,.035f,.95f);
            }
            bool purging=phase==Phase.PurgeA||phase==Phase.PurgeB||latch!=Fault.None;
            plant.Tick(dt,powered&&backing,powered&&turbo,powered&&isolation,throttle,
                powered&&condition,loadClock is >.10f and <1.78f,powered&&heat&&clamped,
                gas&&phase==Phase.DoseA&&phaseClock<doseWindow,gas&&phase==Phase.DoseB&&phaseClock<doseWindow,
                powered&&purging,scenario==Scenario.Restriction,scenario==Scenario.Heater,scenario==Scenario.Valve,gas,1,1);

            if(latch==Fault.None)
            {
                if(phase==Phase.PurgeA)
                {
                    if(scenario==Scenario.Restriction)
                    {
                        if(phaseClock>.40f) lowestFlow=Mathf.Min(lowestFlow,plant.PurgeFlow);maxResidual=Mathf.Max(maxResidual,plant.ResidualA);
                    }
                    if(scenario==Scenario.Valve)stuckPosition=Mathf.Max(stuckPosition,plant.ValveA);
                    if(scenario==Scenario.Valve&&phaseClock>.42f&&plant.ValveA>.14f)
                    {valvePassed=true;Trip(Fault.Valve);}
                    else if(!armed&&phaseClock>2.2f&&!ClearA)
                    {
                        restrictionPassed|=scenario==Scenario.Restriction&&lowestFlow<.70f&&maxResidual>.08f;
                        Trip(Fault.Purge);
                    }
                }
                if(scenario==Scenario.Heater)
                {
                    heaterSpan=Mathf.Max(heaterSpan,plant.Span);
                    if(plant.Span>5f){driftPassed=true;Trip(Fault.Heat);}
                }
                if(phase==Phase.PurgeB)
                {
                    if(phaseClock>.42f&&ClearB)
                    {
                        if(plant.SurfaceB<.94f||plant.Contamination>.05f) Trip(Fault.Purge);
                        else
                        {
                            plant.CompleteCycle();cycles++;if(!automatic)manualCycles++;
                            Enter(Phase.Idle);
                            if(cycles>=120)automatic=false;
                        }
                    }
                    else if(phaseClock>2.2f&&!ClearB)Trip(Fault.Purge);
                }
                if(automatic)
                {
                    if(CanDoseA)DoseA(); else if(CanPurgeA)PurgeA(); else if(CanDoseB)DoseB(); else if(CanPurgeB)PurgeB();
                }
            }
            settled=phase==Phase.Idle?settled+dt:0;
            bool goal=stage switch
            {
                1=>Stable&&Thermal&&validPurge&&Mathf.Abs(pressure-7.5f)<.55f,
                2=>manualCycles>=3&&plant.LastGrowth is >.095f and <.106f,
                3=>measured&&acceptedBatches>=1,
                4=>restrictionPassed&&driftPassed&&valvePassed&&repaired&&measured&&acceptedBatches>=2,
                5=>CanCertify&&certify,
                _=>false
            };
            hold=Ready&&latch==Fault.None&&goal?hold+dt:0;
            if(hold>(stage==1?1.6f:.40f))Award();
            if(plot!=null)plot.Sample(dt,pressure,plant.Temperature[0],plant.MeanFilm,validPurge?flow/3:0);
        }
        private void Enter(Phase next){phase=next;phaseClock=0;}
        private void Award()
        {
            if(stage>=Gates.Length||!bindings.SetRuntimeComponentBinding(Gates[stage],source))return;
            stage++;hold=0;manager.QueueEvaluation();Log($"Objective {stage}/6 verified.");
        }
        private void Log(string text)=>latestEvent=$"{elapsed:0.0}s · {text}";
        private void Trip(Fault cause)
        {
            latch=cause;automatic=false;armed=false;Enter(Phase.Idle);lastPermit=false;contacts[0].SetState(SparkSwitchState.Open);
            if(loadClock>0){loadClock=0;loaded=clamped=false;}
            if(cause==Fault.Feedback){validPurge=recalibrated=flowVerified=false;age=flow=0;Log("PURGE FEEDBACK INVALID · A and B inhibited; safe purge requested.");}
            else Log($"SAFETY HOLD · {cause} · dosing permit removed.");
        }

        public void Window(){if(Ready&&phase==Phase.Idle&&!automatic){doseWindow=doseWindow>1.4f?.40f:doseWindow<.6f?1.16f:1.54f;Log($"Exposure window {doseWindow:0.00}s.");}}
        public void Power(){power=!power;source.SetOutput(power,default);}
        public void CalPressure(){if(Ready)calPressure=true;}
        public void CalFlow(){if(Ready)calFlow=true;}
        public void CalPurge(){if(Ready&&latch==Fault.None){calPurge=true;age=0;}}
        public void CalHeat(){if(Ready)calHeat=true;}
        public void Isolation(){if(Ready&&phase==Phase.Idle)isolation=!isolation;}
        public void Backing(){if(Ready&&calPressure)backing=true;}
        public void Turbo(){if(CanTurbo)turbo=true;}
        public void Load(){if(CanLoad){loaded=clamped=heat=false;loadClock=.001f;Log("Load-lock and wafer robot transferring.");}}
        public void Clamp(){if(Ready&&loaded&&!clamped)clamped=true;}
        public void Heat(){if(Ready&&clamped&&phase==Phase.Idle)heat=!heat;}
        public void PressurePid(){if(Ready&&phase==Phase.Idle){autoPressure=!autoPressure;integral=0;}}
        public void Condition(){if(Ready&&Pumping&&clamped&&phase==Phase.Idle){condition=!condition;integral=0;}}
        public void DoseA(){if(CanDoseA){plant.BeginCycle();Enter(Phase.DoseA);}}
        public void PurgeA(){if(CanPurgeA)Enter(Phase.PurgeA);}
        public void DoseB(){if(CanDoseB)Enter(Phase.DoseB);}
        public void PurgeB(){if(CanPurgeB)Enter(Phase.PurgeB);}
        public void Auto(){if(stage>=3&&Stable&&Thermal&&validPurge&&!measured&&!armed&&!automatic){automatic=true;Log("AUTO active. Each cycle verifies both purges.");}}
        public void Stop(){automatic=false;armed=false;condition=false;Enter(Phase.Idle);Log("Stopped. Pumping down.");}
        public void Metrology()
        {
            if(!CanMetrology)return;
            metrology=1.3f;
            if(Mathf.Abs(plant.MeanFilm-12f)<=.30f&&plant.Uniformity<=.03f&&plant.Conformality>=.95f&&plant.Contamination<.05f&&
                Mathf.Abs(plant.Qcm-plant.Optical)<.20f)
            {
                measured=true;acceptedBatches++;Log("METROLOGY PASSED · thickness, uniformity and conformality.");
            }
            else Log("Film out of tolerance. STOP and load a new wafer.");
        }
        private void Diagnostic(Scenario test)
        {
            scenario=test;
            maxResidual=heaterSpan=stuckPosition=0;lowestFlow=10;
            Log($"Diagnostic challenge · {test}.");
        }
        public void Restriction(){if(CanTest&&!restrictionPassed)Diagnostic(Scenario.Restriction);}
        public void Drift(){if(CanTest&&restrictionPassed&&!driftPassed)Diagnostic(Scenario.Heater);}
        public void Stuck(){if(CanTest&&driftPassed&&!valvePassed)Diagnostic(Scenario.Valve);}
        public void Repair(){if(Ready&&latch!=Fault.None&&scenario!=Scenario.None){scenario=Scenario.None;repaired=true;Log("Fault repaired. Wait for safe reset.");}}
        public void EStop(){emergency.SetState(emergency.IsClosed?SparkSwitchState.Open:SparkSwitchState.Closed);if(emergency.IsOpen&&latch==Fault.None)Trip(Fault.EStop);}
        public void Reset(){if(CanReset){feedbackReset|=latch==Fault.Feedback&&feedbackObserved;latch=Fault.None;Log("Safety reset. Recommission with a fresh wafer.");}}
        public void Arm(){if(stage==5&&phase==Phase.PurgeA&&validPurge&&!feedbackObserved&&!armed){armed=true;Log("Remove W94 with Undo Last Wire.");}}
        public void Recal(){if(CanRecal){recalibrated=true;age=0;Log("Purge transmitter recalibrated. Wait for fresh flow samples.");}}
        public void Verify(){if(CanVerify){flowVerified=true;Log("Fresh purge flow and residual samples verified.");}}
        public void Certify(){if(CanCertify)certify=true;}

        private string Status()
        {
            if(stage==6)return "CERTIFIED · fresh conformal film passed.";
            if(!power)return "Enable 5 V CONTROL and complete wiring.";
            if(latch==Fault.Feedback)return !Ready?"PURGE FEEDBACK INVALID · reconnect W94.":CanRecal?"Select RECAL PURGE.":!flowVerified?"Wait for valid flow and select VERIFY PURGE.":"Select RESET SAFETY, stop and load a fresh wafer.";
            if(latch!=Fault.None)return $"SAFETY HOLD · {latch} · repair the cause before reset.";
            if(!Ready)return wiring??"Check 5 V controls and the E-Stop loop.";
            if(loadClock>0)return "Wafer transfer in progress.";
            if(armed)return "Disconnect W94 · purge transmitter to ALD sequencer.";
            if(stage==1)return "Calibrate. Start backing and turbo. Load and clamp, then enable heat, pressure PID and carrier.";
            if(phase==Phase.DoseA)return "A DOSE · surface coverage rises toward saturation.";
            if(phase==Phase.PurgeA)return "PURGE A · next exposure requires fresh flow and low A residual.";
            if(phase==Phase.DoseB)return "B DOSE · reaction uses available A surface sites.";
            if(phase==Phase.PurgeB)return "PURGE B · clear the chamber to certify the current cycle.";
            if(CanCertify)return "Fresh wafer accepted. Select CERTIFY.";
            if(CanMetrology)return "120 cycles recorded. Run METROLOGY.";
            if(stage==4&&!restrictionPassed)return measured?"STOP, load and commission a test wafer.":"Select PURGE RESTRICT, run A DOSE then PURGE A.";
            if(stage==4&&restrictionPassed&&!driftPassed)return "After reset and heat recovery, select HEATER DRIFT.";
            if(stage==4&&driftPassed&&!valvePassed)return "After reset and heat recovery, select STUCK A and test the A-to-purge transition.";
            if(stage==5&&!feedbackObserved&&!measured)return "Start A DOSE and PURGE A, then select W94 TEST.";
            if(measured)return "STOP to pump down, then load a fresh wafer.";
            if(stage==2)return "Demonstrate three manual A dose, purge, B dose and purge cycles.";
            if(stage==4&&valvePassed)return "After repair and reset, load and commission a fresh wafer, then run AUTO.";
            return "Commission the wafer, then run AUTO for the film target.";
        }
        private void Present()
        {
            statusText.text=Status();
            meterText.text=$"CHAMBER {pressure:0.00} Pa   ·   PURGE {(validPurge?flow.ToString("0.00"):"INVALID")}   ·   ZONES {plant.Temperature[0]:0.0} / {plant.Temperature[1]:0.0} / {plant.Temperature[2]:0.0}°C   ·   CYCLES {cycles}/120";
            detailText.text=$"A / B RESIDUAL {plant.ResidualA:0.000} / {plant.ResidualB:0.000}   ·   FILM {plant.MeanFilm:0.00} nm   ·   TRENCH {plant.Conformality:P1}   ·   UNIFORMITY {plant.Uniformity:P1}";
            calibrationText.text=$"{latestEvent}   |   QCM {plant.Qcm:0.00} nm   ·   OPTICAL {plant.Optical:0.00} nm";
            if(controllerDisplay)controllerDisplay.text=$"{pressure:0.00} Pa · {phase}";
            if(qualityDisplay)qualityDisplay.text=$"{plant.MeanFilm:0.00} nm · {cycles}/120";
            powerButton.interactable=stage<6;windowButton.interactable=stage>=2&&Ready&&!automatic&&phase==Phase.Idle;
            calPressureButton.interactable=Ready&&!calPressure;calFlowButton.interactable=Ready&&!calFlow;
            calPurgeButton.interactable=Ready&&!calPurge;calHeatButton.interactable=Ready&&!calHeat;
            isolationButton.interactable=Ready&&phase==Phase.Idle&&!automatic;backingButton.interactable=Ready&&calPressure&&!backing;turboButton.interactable=CanTurbo;
            loadButton.interactable=CanLoad;clampButton.interactable=Ready&&loaded&&!clamped;heatButton.interactable=Ready&&clamped&&phase==Phase.Idle&&!automatic;
            pressurePidButton.interactable=Ready&&phase==Phase.Idle&&!automatic;conditionButton.interactable=Ready&&Pumping&&clamped&&phase==Phase.Idle&&!automatic;
            doseAButton.interactable=CanDoseA&&!automatic;purgeAButton.interactable=CanPurgeA&&!automatic;
            doseBButton.interactable=CanDoseB&&!automatic;purgeBButton.interactable=CanPurgeB&&!automatic;
            autoButton.interactable=stage>=3&&Stable&&Thermal&&validPurge&&!measured&&!automatic&&!armed&&cycles<120&&
                (stage!=5||feedbackReset)&&(stage!=4||scenario==Scenario.None);
            metrologyButton.interactable=CanMetrology;stopButton.interactable=phase!=Phase.Idle||automatic||condition;
            restrictionButton.interactable=CanTest&&!restrictionPassed;driftButton.interactable=CanTest&&restrictionPassed&&!driftPassed;
            stuckButton.interactable=CanTest&&driftPassed&&!valvePassed;
            repairButton.interactable=Ready&&latch!=Fault.None&&scenario!=Scenario.None;eStopButton.interactable=stage<6;
            resetButton.interactable=CanReset;armButton.interactable=stage==5&&phase==Phase.PurgeA&&validPurge&&!feedbackObserved&&!armed;
            recalButton.interactable=CanRecal;verifyButton.interactable=CanVerify;certifyButton.interactable=CanCertify&&!certify;
            rig.Apply(plant,elapsed,loadClock,loaded,clamped,metrology,(int)phase,emergency.IsOpen);
        }
    }
}
