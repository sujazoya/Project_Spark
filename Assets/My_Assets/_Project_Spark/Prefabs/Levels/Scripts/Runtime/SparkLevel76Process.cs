using System;
using System.Collections.Generic;
using ProjectSpark.Circuit;
using ProjectSpark.Visuals;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.Gameplay
{
    public sealed class SparkLevel76Process : MonoBehaviour
    {
        public static readonly string[] Gates={"L76_Wired","L76_Prepared","L76_Controlled","L76_Plated","L76_Disturbances","L76_Recertified"};
        public static readonly (int a,int b)[] Wires={(0,1),(2,3),(0,4),(2,5),(0,6),(2,7),(0,8),(2,9),(0,10),(2,11),(12,13),(14,15),(16,17),(18,19),(20,21),(22,23),(24,25),(26,27),(28,29),(30,31),(32,33),(34,35),(36,37),(38,39),(40,41),(42,43),(44,45),(46,47),(48,49),(50,51),(52,53),(54,55),(56,57),(58,59),(60,61),(62,63),(64,65),(66,67),(68,69),(70,71),(72,73),(74,75),(76,77),(78,79),(80,81),(82,83),(84,85),(86,87),(88,89),(90,91),(92,93),(94,95),(96,97),(98,99),(100,101),(102,103),(104,105),(106,107),(108,109),(110,111),(112,113),(114,115),(116,117),(118,119),(120,121),(122,123),(65,124),(109,125),(111,126),(115,127),(20,128),(22,129),(43,130),(21,131),(19,132),(133,134),(130,135),(43,136),(130,137),(136,138),(31,139),(125,140),(136,141),(131,142),(143,144),(73,145),(36,146),(119,147),(115,148),(47,149),(131,150),(39,151),(69,152),(77,153),(144,154),(39,155),(23,156),(44,157),(21,158),(12,159),(105,160),(35,161),(131,162),(19,163),(49,164),(136,165),(135,166),(161,167),(158,168),(132,169)};
        private const int StopInput=144, ControlReturn=3, RefPower=10, RefReturn=11, Feedback=75, PermitWire=22;
        public SparkLevelLayout layout;
        public SparkPowerSupply source;
        public SparkSwitch[] contacts;
        public SparkSwitch emergency;
        public SparkTerminal[] pins;
        public SparkLevel76VisualRig rig;
        public SparkLevel76Trend plot;
        public TMP_Text controllerDisplay,qualityDisplay;
        public Text statusText,meterText,detailText,calibrationText;
        public Button powerButton,calFlowButton,pumpButton,filterButton,degasButton,heatButton,doseButton,loadButton,seedButton,prewetButton,
            clampButton,calCurrentButton,calRefButton,waveformButton,zonesButton,plateButton,stopButton,unloadButton,rinseButton,dryButton,
            inspectButton,electricalButton,contactFaultButton,flowFaultButton,additiveFaultButton,restoreButton,eStopButton,resetButton,
            armButton,recalButton,verifyButton,certifyButton;

        private enum Fault { None, Power, Wiring, EStop, Contact, Flow, Additive, Reference, Process }
        private enum Scenario { None, Contact, Flow, Additive }
        private readonly SparkLevel76Plant plant=new();
        private readonly HashSet<(int,int)> wires=new();
        private readonly List<SparkCircuitConnection> edges=new();
        private LevelGamePlayManager manager;
        private SparkLevelLayoutLoader loader;
        private SparkLevelSceneBindings bindings;
        private int topology=-1,stage,cleaning,acceptedBatches;
        private bool power,rails,safety,refPower,wiredReference,permitVoltage,lastPermit;
        private bool calFlow,pump,filter,degas,heat,additives,loaded,seed,prewetting,wet,clamped,calCurrent,calRef,waveform,zones;
        private bool validReference,recalibrated,verified,plating,complete,unloaded,rinsed,dried,inspected,electrical,counted;
        private bool contactPassed,flowPassed,additivePassed,armed,referenceObserved,referenceReset,certify;
        private float elapsed,accumulator,hold,age,measurement,integral,transfer,scan,referenceClock;
        private float currentAtTrip,potentialAtTrip,resistanceFault,flowAtFault,pressureAtFault,additiveAtFault;
        private string wiring,latestEvent="Connect the plating controls and enable 5 V.";
        private Fault latch;
        private Scenario scenario;

        private bool Ready=>rails&&safety&&wiring==null;
        private bool Idle=>!plating&&plant.Current<.025f&&transfer<=0&&cleaning==0;
        private bool Bath=>calFlow&&pump&&filter&&degas&&heat&&additives&&plant.Flow>.88f&&
            plant.Temperature is >=30.5f and <=33.5f&&plant.Bubbles<.035f&&plant.Reserve>10&&plant.Accelerator>.65f;
        private bool CanLoad=>Ready&&Idle&&!loaded&&latch==Fault.None;
        private bool CanPlate=>stage>=2&&Ready&&Bath&&Idle&&loaded&&wet&&seed&&clamped&&waveform&&zones&&calCurrent&&validReference&&
            plant.Continuity>=.99f&&plant.Spindle>54&&plant.Lid<.03f&&plant.MeanCopper<.01f&&latch==Fault.None;
        private bool CanTest=>stage==4&&plating&&plant.Current>.70f&&plant.MeanFill<.65f&&scenario==Scenario.None;
        private bool CanArm=>stage==5&&plating&&plant.Current>.65f&&!armed&&!referenceObserved;
        private bool CanRecal=>Ready&&Idle&&latch==Fault.Reference&&!recalibrated&&referenceClock<=0;
        private bool CanVerify=>Ready&&Idle&&latch==Fault.Reference&&validReference&&!verified&&measurement<.29f;
        private bool CanReset=>Ready&&Idle&&latch!=Fault.None&&scenario==Scenario.None&&plant.Resistance<.112f&&Bath&&
            (latch!=Fault.Reference||verified);
        private bool CanInspect=>Ready&&unloaded&&dried&&!inspected&&latch==Fault.None;
        private bool CanCertify=>stage==5&&referenceReset&&counted&&acceptedBatches>=3&&Ready&&latch==Fault.None;
        private void Start()
        {
            manager=FindFirstObjectByType<LevelGamePlayManager>();loader=FindFirstObjectByType<SparkLevelLayoutLoader>();
            if(!manager||!loader||pins.Length!=170||contacts.Length!=1||!rig){Debug.LogError("[LEVEL 76] Missing electroplating references.",this);enabled=false;return;}
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
            wiredReference=Routed(Feedback);
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
            refPower=power&&Rail(RefPower,RefReturn);
            permitVoltage=power&&Rail(Wires[PermitWire].b,ControlReturn);
            if(stage==0&&Ready)Award();
            if(latch==Fault.None&&(plating||transfer>0||prewetting)&&!Ready)
            {
                bool referenceOnly=rails&&safety&&!wiredReference&&wires.Count==Wires.Length-1;
                if(referenceOnly)for(int i=0;i<Wires.Length;i++)if(i!=Feedback&&!Routed(i)){referenceOnly=false;break;}
                referenceObserved|=stage==5&&armed&&referenceOnly;
                Trip(referenceOnly?Fault.Reference:!rails?Fault.Power:!safety?Fault.EStop:Fault.Wiring);
            }
            accumulator+=Mathf.Min(Time.deltaTime,.18f)*2.25f;
            while(accumulator>=SparkLevel76Plant.Step){accumulator-=SparkLevel76Plant.Step;Tick(SparkLevel76Plant.Step);}
            Present();
        }
        private void Tick(float dt)
        {
            elapsed+=dt;
            bool signal=refPower&&wiredReference&&calRef&&plant.Probe>.92f&&(latch!=Fault.Reference||recalibrated);
            age=signal?age+dt:0;
            validReference=age>.40f;
            measurement=validReference?Mathf.Lerp(measurement,plant.Potential,dt/.09f):0;
            bool permit=Ready&&plating&&validReference&&clamped&&latch==Fault.None&&plant.Current<1.16f&&plant.Temperature<34.2f;
            if(lastPermit!=permit){lastPermit=permit;contacts[0].SetState(permit?SparkSwitchState.Closed:SparkSwitchState.Open);}
            float error=.57f-measurement;
            integral=permit?Mathf.Clamp(integral+error*dt,-.22f,.20f):0;
            float duty=Mathf.Clamp(.91f+error*.28f+integral*.42f,.30f,1.03f);
            bool circulation=rails&&safety&&(Ready||latch==Fault.Reference);
            if(transfer>0)
            {
                transfer+=dt;
                if(transfer>1.8f){transfer=0;loaded=true;Log("Prepared interconnect wafer seated for seed inspection.");}
            }
            plant.Tick(dt,circulation&&pump,filter,degas,circulation&&heat,circulation&&additives,Ready&&prewetting,clamped,
                transfer is >.1f and <1.40f,calRef||referenceClock>0,permit&&permitVoltage&&contacts[0].IsClosed,duty,zones?.004f:.085f,
                scenario==Scenario.Contact,scenario==Scenario.Flow,scenario==Scenario.Additive,cleaning,elapsed);
            if(prewetting&&plant.Wet>.975f)
            {prewetting=false;wet=true;Log("Prewet complete with low bubble fraction.");}
            if(referenceClock>0)
            {
                referenceClock=Mathf.Max(0,referenceClock-dt);
                if(referenceClock==0)
                {
                    calRef=true;age=0;
                    if(latch==Fault.Reference)recalibrated=true;
                    Log("Reference electrode calibrated at the stable idle potential.");
                }
            }
            if(cleaning==1&&plant.Residue<.03f){cleaning=0;rinsed=true;Log("Rinse complete.");}
            if(cleaning==2&&plant.Moisture<.035f){cleaning=0;dried=true;Log("Dry wafer ready for metrology.");}
            scan=Mathf.Max(0,scan-dt);
            if(plating&&latch==Fault.None)
            {
                if(scenario==Scenario.Contact&&plant.Resistance>.245f)
                {resistanceFault=plant.Resistance;contactPassed=true;Trip(Fault.Contact);}
                else if(scenario==Scenario.Flow&&plant.Flow<.58f&&plant.LinePressure>1.4f)
                {flowAtFault=plant.Flow;pressureAtFault=plant.LinePressure;flowPassed=true;Trip(Fault.Flow);}
                else if(scenario==Scenario.Additive&&plant.Accelerator<.55f)
                {additiveAtFault=plant.Accelerator;additivePassed=true;Trip(Fault.Additive);}
                else if(plant.Temperature>34.2f||!validReference)Trip(Fault.Process);
                else if(plant.LeastFill>.997f&&plant.MeanCopper>=1.071f&&!armed)
                {complete=true;StopRectifier();Log("Fill and charge targets reached. Waiting for current rundown.");}
                else if(plant.MeanCopper>1.14f)
                {StopRectifier();Log("Overburden limit reached. Unload the test wafer.");}
            }
            if(inspected&&electrical&&!counted){counted=true;acceptedBatches++;}
            bool goal=stage switch
            {
                1=>Bath&&loaded&&seed&&wet&&clamped&&plant.Continuity>=.99f&&plant.Lid<.03f,
                2=>plating&&Bath&&plant.Current>.65f&&plant.MeanFill>.10f&&plant.MassError<.001f&&validReference&&plant.Uniformity<.03f,
                3=>counted&&acceptedBatches>=1,
                4=>contactPassed&&flowPassed&&additivePassed&&counted&&acceptedBatches>=2,
                5=>CanCertify&&certify,
                _=>false
            };
            hold=Ready&&latch==Fault.None&&goal?hold+dt:0;
            if(hold>.44f)Award();
            if(plot!=null)plot.Show(plant,validReference,plating?Mathf.Clamp((int)(plant.MeanFill*16),0,15):-1);
        }
        private void Award()
        {
            if(stage>=Gates.Length||!bindings.SetRuntimeComponentBinding(Gates[stage],source))return;
            stage++;hold=0;manager.QueueEvaluation();Log($"Objective {stage}/6 verified.");
        }
        private void Log(string text)=>latestEvent=$"{elapsed:0.0}s · {text}";
        private void StopRectifier()
        {
            plating=armed=false;lastPermit=false;contacts[0].SetState(SparkSwitchState.Open);
        }
        private void Trip(Fault cause)
        {
            latch=cause;currentAtTrip=plant.Current;potentialAtTrip=plant.Potential;
            StopRectifier();prewetting=false;referenceClock=0;zones=false;
            if(transfer>0){transfer=0;loaded=clamped=false;}
            if(cause==Fault.Reference)
            {
                validReference=recalibrated=verified=false;age=measurement=0;
                Log("REFERENCE POTENTIAL INVALID · rectifier disabled; current decaying.");
            }
            else Log($"PLATING SAFETY HOLD · {cause} · rectifier disabled.");
        }

        public void Power(){power=!power;source.SetOutput(power,default);}
        public void CalFlow(){if(Ready)calFlow=true;}
        public void Pump(){if(Ready&&calFlow)pump=true;}
        public void Filter(){if(Ready&&pump)filter=true;}
        public void Degas(){if(Ready&&pump)degas=true;}
        public void Heat(){if(Ready&&pump)heat=true;}
        public void Dose(){if(Ready&&pump)additives=true;}
        public void Load()
        {
            if(!CanLoad)return;
            plant.NewWafer();transfer=.001f;
            loaded=seed=wet=clamped=complete=unloaded=rinsed=dried=inspected=electrical=counted=false;
            Log("Transferring a wafer with prepared trenches, barrier and conductive seed.");
        }
        public void Seed(){if(Ready&&loaded&&Idle&&plant.Seed>=.99f){seed=true;Log("Prepared barrier and seed continuity passed.");}}
        public void Prewet(){if(Ready&&loaded&&seed&&Bath&&!wet&&latch==Fault.None)prewetting=true;}
        public void Clamp(){if(Ready&&wet&&loaded&&latch==Fault.None)clamped=true;}
        public void CalCurrent(){if(Ready&&Idle){calCurrent=true;Log("Current shunt and voltage channel calibrated.");}}
        public void CalRef(){if(Ready&&Idle&&Bath&&!calRef&&referenceClock<=0)referenceClock=.66f;}
        public void Waveform(){if(Ready&&Idle&&calCurrent)waveform=true;}
        public void Zones(){if(Ready&&Idle&&Bath&&waveform&&validReference&&plant.Continuity>=.99f&&latch==Fault.None)
            {zones=true;Log("Multi-zone anode distribution and cathode continuity qualified.");}}
        public void Plate(){if(CanPlate){plating=true;Log("Rectifier enabled under reference-potential control.");}}
        public void Stop(){StopRectifier();Log("Rectifier stop requested.");}
        public void Unload()
        {
            if(!CanUnload)return;
            loaded=clamped=false;unloaded=complete;
            Log(unloaded?"Wafer transferred to rinse chamber.":"Interrupted wafer discarded.");
        }
        public void Rinse(){if(Ready&&unloaded&&!rinsed&&cleaning==0){cleaning=1;}}
        public void Dry(){if(Ready&&rinsed&&!dried&&cleaning==0){cleaning=2;}}
        private bool Quality=>plant.LeastFill>=.995f&&plant.WorstVoid<=.005f&&plant.Uniformity<=.03f&&
            plant.Overburden is >=.02f and <=.10f&&plant.Roughness<.025f&&plant.MassError<.001f;
        public void Inspect(){if(CanInspect){scan=1.25f;inspected=Quality;Log(inspected?"Fill, void, thickness and overburden inspection passed.":"Wafer outside quality limits.");}}
        public void Electrical(){if(Ready&&dried&&inspected&&!electrical){scan=1.25f;electrical=plant.TestResistance<=1.05f;Log(electrical?"Isolated test-structure resistance passed.":"Test-structure resistance outside tolerance.");}}
        public void ContactFault(){if(CanTest&&!contactPassed){scenario=Scenario.Contact;Log("Cathode contact degrading.");}}
        public void FlowFault(){if(CanTest&&contactPassed&&!flowPassed){scenario=Scenario.Flow;Log("Manifold restriction applied.");}}
        public void AdditiveFault(){if(CanTest&&flowPassed&&!additivePassed){scenario=Scenario.Additive;Log("Accelerator dosing drift applied.");}}
        public void Restore(){if(Ready&&latch!=Fault.None&&scenario!=Scenario.None){scenario=Scenario.None;Log("Fault corrected. Wait for bath and contact recovery.");}}
        public void EStop(){emergency.SetState(emergency.IsClosed?SparkSwitchState.Open:SparkSwitchState.Closed);if(emergency.IsOpen&&latch==Fault.None)Trip(Fault.EStop);}
        public void Reset(){if(CanReset){referenceReset|=latch==Fault.Reference&&referenceObserved;latch=Fault.None;Log("Safety reset. Requalify the current zones.");}}
        public void Arm(){if(CanArm){armed=true;Log("Remove W076 with Undo Last Wire during live plating.");}}
        public void Recal(){if(CanRecal){referenceClock=.69f;Log("Collecting fresh reference calibration.");}}
        public void Verify(){if(CanVerify){verified=true;Log("Fresh reference samples and idle potential verified.");}}
        public void Certify(){if(CanCertify)certify=true;}

        private bool CanUnload=>Ready&&Idle&&loaded&&latch==Fault.None&&!prewetting;
        private string Status()
        {
            if(stage==6)return "CERTIFIED · fresh copper fill and test structure passed.";
            if(!power)return "Enable 5 V CONTROL and complete wiring.";
            if(latch==Fault.Reference)return !Ready?"REFERENCE POTENTIAL INVALID · reconnect W076.":CanRecal?"Select RECAL REF after current reaches idle.":!verified?"Wait for fresh readings and select VERIFY REF.":"Select RESET SAFETY and requalify the bath and zones.";
            if(latch!=Fault.None)return $"PLATING SAFETY HOLD · {latch} · restore and reset.";
            if(!Ready)return wiring??"Verify 5 V supply and emergency-stop loop.";
            if(armed)return "Disconnect W076 during live plating.";
            if(transfer>0)return "Wafer robot transferring a prepared wafer.";
            if(prewetting)return "PREWET · displacing bubbles from trenches.";
            if(stage==1)return "Calibrate flow and commission the bath. Load, inspect seed, prewet and clamp.";
            if(CanCertify)return "Fresh wafer passed. Select CERTIFY.";
            if(cleaning>0)return cleaning==1?"Rinsing wafer.":"Spin drying wafer.";
            if(plating)return stage==4&&!contactPassed?"Apply CONTACT FAULT.":stage==4&&!flowPassed?"Apply FLOW RESTRICT.":stage==4&&!additivePassed?"Apply ADDITIVE DRIFT.":"PLATING · reference potential and current distribution supervised.";
            if(counted)return "Accepted wafer. Load a fresh wafer for the next sequence.";
            if(complete)return !unloaded?"Wait for current rundown, then UNLOAD.":!rinsed?"RINSE the wafer.":!dried?"SPIN DRY the wafer.":!inspected?"Run FILL METRO.":"Run ELECTRICAL.";
            return "Prepare a wafer, calibrate current and reference, set waveform, qualify zones and select PLATE.";
        }
        private void Present()
        {
            statusText.text=Status();
            meterText.text=$"FLOW {plant.Flow:0.00}   ·   BATH {plant.Temperature:0.0}°C   ·   CURRENT {plant.Current:0.00}   ·   CELL {plant.Voltage:0.00}   ·   REF {(validReference?measurement.ToString("0.00"):"STALE")}";
            detailText.text=$"FILL {plant.LeastFill:P2}   ·   VOID {plant.WorstVoid:P2}   ·   NONUNIFORMITY {plant.Uniformity:P2}   ·   OVERBURDEN {plant.Overburden:0.000}";
            calibrationText.text=$"{latestEvent}   |   CONTACT {plant.Resistance:0.000}   ·   ADDITIVE {plant.Accelerator:0.00}";
            if(controllerDisplay)controllerDisplay.text=validReference?$"REF {measurement:0.00} · I {plant.Current:0.00}":"REF STALE";
            if(qualityDisplay)qualityDisplay.text=$"{plant.LeastFill:P1} FILL · {plant.WorstVoid:P1} VOID";
            powerButton.interactable=stage<6;calFlowButton.interactable=Ready&&!calFlow;pumpButton.interactable=Ready&&calFlow&&!pump;
            filterButton.interactable=Ready&&pump&&!filter;degasButton.interactable=Ready&&pump&&!degas;heatButton.interactable=Ready&&pump&&!heat;
            doseButton.interactable=Ready&&pump&&!additives;loadButton.interactable=CanLoad;
            seedButton.interactable=Ready&&loaded&&Idle&&!seed;prewetButton.interactable=Ready&&loaded&&seed&&Bath&&!wet&&!prewetting&&latch==Fault.None;
            clampButton.interactable=Ready&&wet&&loaded&&!clamped&&latch==Fault.None;calCurrentButton.interactable=Ready&&Idle&&!calCurrent;
            calRefButton.interactable=Ready&&Idle&&Bath&&!calRef&&referenceClock<=0;waveformButton.interactable=Ready&&Idle&&calCurrent&&!waveform;
            zonesButton.interactable=Ready&&Idle&&Bath&&waveform&&validReference&&plant.Continuity>=.99f&&latch==Fault.None&&!zones;
            plateButton.interactable=CanPlate;stopButton.interactable=plating;unloadButton.interactable=CanUnload;
            rinseButton.interactable=Ready&&unloaded&&!rinsed&&cleaning==0;dryButton.interactable=Ready&&rinsed&&!dried&&cleaning==0;
            inspectButton.interactable=CanInspect;electricalButton.interactable=Ready&&dried&&inspected&&!electrical;
            contactFaultButton.interactable=CanTest&&!contactPassed;flowFaultButton.interactable=CanTest&&contactPassed&&!flowPassed;
            additiveFaultButton.interactable=CanTest&&flowPassed&&!additivePassed;restoreButton.interactable=Ready&&latch!=Fault.None&&scenario!=Scenario.None;
            eStopButton.interactable=stage<6;resetButton.interactable=CanReset;armButton.interactable=CanArm;recalButton.interactable=CanRecal;
            verifyButton.interactable=CanVerify;certifyButton.interactable=CanCertify&&!certify;
            rig.Apply(plant,elapsed,transfer,cleaning,prewetting,plating,latch!=Fault.None,scan,emergency.IsOpen);
        }
    }
}
