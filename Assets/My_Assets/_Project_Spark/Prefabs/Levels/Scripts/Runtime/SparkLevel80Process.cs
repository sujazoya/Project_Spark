using System;
using System.Collections.Generic;
using ProjectSpark.Circuit;
using ProjectSpark.Visuals;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Disturbance=ProjectSpark.Gameplay.SparkLevel80Plant.Disturbance;

namespace ProjectSpark.Gameplay
{
    public sealed partial class SparkLevel80Process : MonoBehaviour
    {
        public static readonly string[] Gates={"L80_Wired","L80_Commissioned","L80_Locked","L80_Qualified","L80_Disturbances","L80_Recertified"};
        public static readonly (int a,int b)[] Wires={(0,1),(2,3),(0,4),(2,5),(0,6),(2,7),(0,8),(2,9),(0,10),(2,11),(12,13),(14,15),(16,17),(16,18),(19,20),(21,22),(23,24),(25,26),(27,28),(29,30),(31,32),(33,34),(35,36),(37,38),(39,40),(41,42),(43,44),(16,45),(46,47),(48,49),(50,51),(52,53),(54,55),(56,57),(58,59),(60,61),(62,63),(64,65),(66,67),(68,69),(70,71),(72,73),(74,75),(76,77),(78,79),(80,81),(82,83),(84,85),(86,87),(88,89),(90,91),(92,93),(94,95),(96,97),(98,99),(100,101),(102,103),(104,105),(106,107),(108,109),(110,111),(112,113),(114,115),(116,117),(118,119),(120,121),(122,123),(124,125),(126,127),(128,129),(130,131),(132,133),(134,135),(134,136),(137,138),(139,140),(141,142),(143,144),(135,145),(131,146),(131,147),(129,148),(138,149),(140,150),(63,151),(65,152),(131,153),(153,154),(153,155),(155,156),(154,157),(153,158),(157,159),(133,160),(133,161),(77,162),(57,163),(55,164),(59,165),(61,166),(44,167),(167,168),(168,169),(142,170),(34,171),(171,172),(44,173),(173,174),(173,175),(89,176),(142,177),(71,178),(162,179),(154,180),(181,182),(182,183),(22,184),(184,185),(162,186),(185,187),(145,188),(14,189),(22,190),(145,191),(186,192),(182,193)};
        private const int StopInput=182,ControlReturn=3,RxSupply=8,RxReturn=9,Feedback=77,PermitWire=10;
        public SparkLevelLayout layout;
        public SparkPowerSupply source;
        public SparkSwitch[] contacts;
        public SparkSwitch emergency;
        public SparkTerminal[] pins;
        public SparkLevel80VisualRig rig;
        public SparkLevel80Trend plot;
        public TMP_Text controllerDisplay,qualityDisplay;
        public Text statusText,meterText,detailText,calibrationText;
        public Button powerButton,safetyButton,packageButton,calSourceButton,dockButton,calRxButton,sourceButton,tuneAButton,tuneBButton,
            tuneCButton,tuneDButton,compButton,spectrumButton,budgetButton,receiverButton,sendButton,fecButton,testButton,driftButton,
            thermalButton,bendButton,restoreButton,eStopButton,resetButton,armButton,verifyButton,certifyButton;

        private enum Fault { None, Power, Wiring, EStop, Drift, Thermal, Bend, Feedback }
        private readonly SparkLevel80Plant plant=new();
        private readonly bool[] tune=new bool[4];
        private readonly HashSet<(int,int)> wires=new();
        private readonly List<SparkCircuitConnection> edges=new();
        private LevelGamePlayManager manager;
        private SparkLevelLayoutLoader loader;
        private SparkLevelSceneBindings bindings;
        private Fault latch;
        private Disturbance fault;
        private int topology=-1,stage,acceptedRuns;
        private bool power,rails,safety,rxPower,wiredFeedback,permitVoltage,lastPermit;
        private bool safetyProof,safetyOpenObserved,package,sourceCal,docked,rxCal,rxValid,shutterVerified,sourceRequested,compensate;
        private bool spectrum,budget,receiver,sending,fec,counted,armed,feedbackObserved,feedbackReset,certify;
        private bool driftPassed,thermalPassed,bendPassed;
        private float elapsed,accumulator,awardDwell,safetyClock,sourceClock,rxClock,rxAge,rxSample,lockDwell,trainTime,scan;
        private float launchAtTrip,driftAtTrip,thermalAtTrip,bendAtTrip;
        private string wiring,latestEvent="Connect the optical workbench and enable 5 V.";

        private bool Ready=>rails&&safety&&wiring==null;
        private bool Healthy=>Ready&&latch==Fault.None;
        private bool Setup=>safetyProof&&package&&sourceCal&&docked&&plant.Coupling>.93f&&rxCal&&rxValid;
        private bool Running=>sourceRequested&&plant.Launch>.76f&&plant.MainClosed<.02f&&plant.EmergencyClosed<.02f;
        private bool QualifiedSpectrum=>lockDwell>.76f&&plant.AllLocked&&plant.CrossTalk<.092f;
        private bool CanSafety=>Healthy&&!safetyProof&&safetyClock<=0&&plant.SafelyClosed;
        private bool CanCalSource=>Healthy&&safetyProof&&!sourceRequested&&!sourceCal&&sourceClock<=0&&plant.SourceTemperature>.42f;
        private bool CanCalRx=>Ready&&safetyProof&&!rxCal&&rxClock<=0&&rxPower&&wiredFeedback&&plant.SafelyClosed&&latch is Fault.None or Fault.Feedback;
        private bool CanSource=>Healthy&&stage>=2&&Setup;
        private bool CanTune(int i)=>Healthy&&Running&&rxValid&&!tune[i];
        private bool CanComp=>Healthy&&Running&&tune[0]&&tune[1]&&tune[2]&&tune[3]&&!compensate;
        private bool CanSpectrum=>Healthy&&compensate&&QualifiedSpectrum&&rxValid&&!spectrum;
        private bool CanBudget=>Healthy&&spectrum&&QualifiedSpectrum&&rxValid&&plant.BudgetGood()&&!budget;
        private bool CanReceiver=>Healthy&&budget&&QualifiedSpectrum&&rxValid&&!receiver;
        private bool CanSend=>Healthy&&receiver&&plant.ClocksReady&&plant.BudgetGood()&&rxValid&&!sending;
        private bool CanTest=>Healthy&&sending&&fec&&rxValid&&QualifiedSpectrum&&!counted&&trainTime>2.7f&&plant.PassData();
        private bool CanDrift=>stage==4&&Running&&plant.ClocksReady&&!driftPassed&&fault==Disturbance.None;
        private bool CanThermal=>stage==4&&Running&&plant.ClocksReady&&driftPassed&&!thermalPassed&&fault==Disturbance.None;
        private bool CanBend=>stage==4&&Running&&plant.ClocksReady&&thermalPassed&&!bendPassed&&fault==Disturbance.None;
        private bool CanArm=>stage==5&&counted&&Running&&sending&&rxValid&&!armed&&!feedbackObserved;
        private bool CanVerify=>Ready&&latch==Fault.Feedback&&rxValid&&plant.SafelyClosed&&!shutterVerified;
        private bool CanReset=>Ready&&latch!=Fault.None&&fault==Disturbance.None&&plant.SafelyClosed&&
            (latch!=Fault.Feedback||shutterVerified&&rxCal&&rxValid)&&plant.WavelengthOffset<.035f&&plant.BendLoss<.035f;
        private bool CanCertify=>stage==5&&feedbackReset&&counted&&acceptedRuns>=3&&Healthy&&rxValid&&plant.PassData();

        private void Start()
        {
            manager=FindFirstObjectByType<LevelGamePlayManager>();loader=FindFirstObjectByType<SparkLevelLayoutLoader>();
            if(!manager||!loader||pins.Length!=194||contacts.Length!=1||!rig)
            {
                Debug.LogError("[LEVEL 80] Missing photonics workstation references.",this);enabled=false;return;
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
            rxPower=power&&Rail(RxSupply,RxReturn);
            permitVoltage=power&&Rail(Wires[PermitWire].b,ControlReturn);
            if(stage==0&&Ready)Award();
            if(latch==Fault.None&&(sourceRequested||safetyClock>0)&&!Ready)
            {
                bool feedbackOnly=rails&&safety&&!wiredFeedback&&wires.Count==Wires.Length-1;
                if(feedbackOnly)
                    for(int i=0;i<Wires.Length;i++)
                        if(i!=Feedback&&!Routed(i)){feedbackOnly=false;break;}
                feedbackObserved|=stage==5&&armed&&sending&&Running&&feedbackOnly;
                Trip(feedbackOnly?Fault.Feedback:!rails?Fault.Power:!safety?Fault.EStop:Fault.Wiring);
            }
            accumulator+=Mathf.Min(Time.deltaTime,.18f)*(armed?.39f:2.10f);
            while(accumulator>=SparkLevel80Plant.Step)
            {
                accumulator-=SparkLevel80Plant.Step;
                Tick(SparkLevel80Plant.Step);
            }
            Present();
        }
        private void Tick(float dt)
        {
            elapsed+=dt;
            bool proof=safetyClock>0&&Healthy;
            if(proof)
            {
                safetyClock+=dt;
                if(plant.MainClosed<.03f&&plant.EmergencyClosed<.03f)safetyOpenObserved=true;
                if(safetyClock>1.5f&&plant.SafelyClosed)
                {
                    safetyClock=0;safetyProof=safetyOpenObserved;Log("Source inhibited while both shutters opened and reclosed.");
                }
            }
            if(sourceClock>0)
            {
                sourceClock=Mathf.Max(0,sourceClock-dt);
                if(sourceClock==0){sourceCal=Healthy&&plant.SourceTemperature>.42f;Log("Source temperature, launch and spectral references calibrated.");}
            }
            if(rxClock>0)
            {
                rxClock=Mathf.Max(0,rxClock-dt);
                if(rxClock==0){rxCal=Ready&&rxPower&&wiredFeedback&&plant.SafelyClosed;rxAge=0;Log("Receiver power monitor zero calibrated.");}
            }
            bool sensor=rxPower&&wiredFeedback&&rxCal;
            rxAge=sensor?rxAge+dt:0;rxValid=rxAge>.35f;rxSample=rxValid?plant.AverageRx:0;
            bool permit=Healthy&&sourceRequested&&rxValid;
            if(permit!=lastPermit){lastPermit=permit;contacts[0].SetState(permit?SparkSwitchState.Closed:SparkSwitchState.Open);}
            bool emission=permit&&permitVoltage&&contacts[0].IsClosed;
            bool shutter=emission&&plant.Launch>.22f;
            bool link=emission&&sending&&rxValid;
            plant.Tick(dt,Ready,docked,tune,compensate,proof,safetyClock is >0 and <.72f,
                emission,shutter,shutter&&safety,receiver,link,fec,fault);
            scan=Mathf.Max(0,scan-dt);
            lockDwell=plant.AllLocked&&rxValid?lockDwell+dt:0;
            trainTime=link&&plant.ClocksReady?trainTime+dt:0;
            if(latch==Fault.None)
            {
                if(fault==Disturbance.Drift&&plant.WavelengthOffset>.19f){driftAtTrip=plant.WavelengthOffset;driftPassed=true;Trip(Fault.Drift);}
                else if(fault==Disturbance.Thermal&&plant.CrossTalk>.20f){thermalAtTrip=plant.CrossTalk;thermalPassed=true;Trip(Fault.Thermal);}
                else if(fault==Disturbance.Bend&&plant.BendLoss>.32f){bendAtTrip=plant.BendLoss;bendPassed=true;Trip(Fault.Bend);}
            }
            bool goal=stage switch
            {
                1=>Setup&&plant.SafelyClosed,
                2=>spectrum&&QualifiedSpectrum&&rxValid,
                3=>counted&&acceptedRuns>=1,
                4=>driftPassed&&thermalPassed&&bendPassed&&counted&&acceptedRuns>=2,
                5=>CanCertify&&certify,
                _=>false
            };
            awardDwell=Healthy&&goal?awardDwell+dt:0;
            if(awardDwell>.36f)Award();
            if(plot!=null)plot.Show(plant,rxValid,link);
        }
        private void ClearLink()
        {
            for(int i=0;i<tune.Length;i++)tune[i]=false;
            compensate=spectrum=budget=receiver=sending=fec=counted=armed=false;
            lockDwell=trainTime=0;plant.ClearData();
        }
        private void StopSource()
        {
            sourceRequested=lastPermit=false;
            contacts[0].SetState(SparkSwitchState.Open);
        }
        private void Trip(Fault cause)
        {
            latch=cause;launchAtTrip=plant.Launch;StopSource();ClearLink();safetyClock=sourceClock=rxClock=0;
            if(cause==Fault.Feedback)
            {
                rxCal=rxValid=shutterVerified=false;rxSample=rxAge=0;
                Log("RX POWER FEEDBACK INVALID · source off, both shutters closing.");
            }
            else Log($"PROTECTION LATCHED · {cause}.");
        }
        private void Award()
        {
            if(stage>=Gates.Length||!bindings.SetRuntimeComponentBinding(Gates[stage],source))return;
            stage++;awardDwell=0;manager.QueueEvaluation();Log($"Objective {stage}/6 verified.");
        }
        private void Log(string message)=>latestEvent=$"{elapsed:0.0}s · {message}";

        public void Power(){power=!power;source.SetOutput(power,default);}
        public void Safety(){if(CanSafety){safetyClock=.001f;safetyOpenObserved=false;Log("Proving two shutter mechanisms with laser emission inhibited.");}}
        public void Package(){if(Healthy&&safetyProof){package=true;Log("Four-chiplet package and separate photonic engine accepted.");}}
        public void CalSource(){if(CanCalSource)sourceClock=.70f;}
        public void Dock(){if(Healthy&&package){docked=true;Log("Fiber dock moving to the edge coupler.");}}
        public void CalRx(){if(CanCalRx)rxClock=.83f;}
        public void Source()
        {
            if(!CanSource)return;
            if(sourceRequested){StopSource();ClearLink();Log("Optical source disabled.");}
            else{sourceRequested=true;Log("Optical source starting under the verified shutter interlock.");}
        }
        private void Tune(int i){if(CanTune(i)){tune[i]=true;Log($"Heater and sensor loop armed for Ring {(char)('A'+i)}.");}}
        public void TuneA()=>Tune(0);
        public void TuneB()=>Tune(1);
        public void TuneC()=>Tune(2);
        public void TuneD()=>Tune(3);
        public void Comp(){if(CanComp){compensate=true;Log("Adjacent heater compensation active.");}}
        public void Spectrum(){if(CanSpectrum){spectrum=true;scan=1.05f;Log("Four stable carrier positions and adjacent spacing verified.");}}
        public void Budget(){if(CanBudget){budget=true;scan=1.05f;Log("All calibrated receiver channels pass the optical power budget.");}}
        public void Receiver(){if(CanReceiver){receiver=true;Log("Photodiodes, transimpedance stages and four CDR loops enabled.");}}
        public void Send(){if(CanSend){plant.ClearData();trainTime=0;sending=true;Log("Four independent chiplet PRBS streams on air.");}}
        public void Fec(){if(Healthy&&sending&&!fec){fec=true;plant.ClearData();trainTime=0;Log("FEC enabled. Fresh eye and error samples accumulating.");}}
        public void Test()
        {
            if(!CanTest)return;
            counted=true;acceptedRuns++;scan=1.1f;Log("All four eye, raw BER, FEC and route checks passed.");
        }
        public void Drift(){if(CanDrift){fault=Disturbance.Drift;Log("Laser wavelength reference drift applied.");}}
        public void Thermal(){if(CanThermal){fault=Disturbance.Thermal;Log("Adjacent heater coupling elevated.");}}
        public void Bend(){if(CanBend){fault=Disturbance.Bend;Log("Virtual fiber microbend attenuating the link.");}}
        public void Restore(){if(Ready&&latch!=Fault.None&&fault!=Disturbance.None){fault=Disturbance.None;Log("Disturbance corrected. Wait for a safe reset.");}}
        public void EStop(){emergency.SetState(emergency.IsClosed?SparkSwitchState.Open:SparkSwitchState.Closed);if(emergency.IsOpen&&latch==Fault.None)Trip(Fault.EStop);}
        public void Reset(){if(CanReset){feedbackReset|=latch==Fault.Feedback&&feedbackObserved;latch=Fault.None;Log("Protection reset. Retune all four carriers and rerun traffic.");}}
        public void Arm(){if(CanArm){armed=true;Log("Use OPEN W078 while all four streams are live.");}}
        public void Verify(){if(CanVerify){shutterVerified=true;Log("Fresh receiver samples, safe laser output, and both shutters verified.");}}
        public void Certify(){if(CanCertify)certify=true;}

        private string Status()
        {
            if(stage==6)return "CERTIFIED · four wavelengths, four recovered data streams.";
            if(!power)return "Enable 5 V CONTROL and complete wiring.";
            if(latch==Fault.Feedback)
            {
                if(!Ready)return "RX POWER FEEDBACK INVALID · reconnect W078.";
                if(!plant.SafelyClosed)return "Wait for both shutters and source output to settle.";
                if(!rxCal)return "Select CAL RX POWER.";
                return shutterVerified?"Select RESET SAFETY.":"Select VERIFY RX / SAFE when fresh samples arrive.";
            }
            if(latch!=Fault.None)return $"PROTECTION LATCHED · {latch} · restore and reset.";
            if(!Ready)return wiring??"Check 5 V rails and the safety loop.";
            if(armed)return "Live fault test · select OPEN W078 to remove W078.";
            if(CanCertify)return "Fresh four-channel link qualified · select CERTIFY.";
            if(CanArm)return "Select ARM W078 TEST with all four streams active.";
            if(stage==4&&counted)
            {
                if(CanDrift)return "Select SOURCE DRIFT to test spectral tracking.";
                if(CanThermal)return "Select THERMAL FAULT to test adjacent heater coupling.";
                if(CanBend)return "Select FIBER BEND to test insertion loss.";
            }
            if(fault!=Disturbance.None)return "Optical disturbance applied · watching protection thresholds.";
            if(!safetyProof)return "Test both shutters with the optical source inhibited.";
            if(!Setup)return "Verify package, calibrate both meters, and dock the fiber.";
            if(!sourceRequested)return "Enable the protected optical source.";
            if(!spectrum)return "Tune Rings A–D, compensate and verify the spectrum.";
            if(!budget)return "Verify calibrated receiver power on all four channels.";
            if(!receiver||!plant.ClocksReady)return "Recover four clocks and wait for lock.";
            if(!sending||!fec)return "Send four streams and enable FEC.";
            return counted?"Four-channel run qualified.":"Collect eye and bit-error samples, then test.";
        }
        private void Present()
        {
            PresentEquipmentWiring();
            statusText.text=Status();
            meterText.text=$"TX {plant.Launch:0.00}   ·   RX {(rxValid?rxSample.ToString("0.00"):"STALE")}   ·   SPACING {plant.SpectrumGap:0.00}   ·   CROSSTALK {plant.CrossTalk:0.00}";
            detailText.text=$"A {plant.Rx[0]:0.00}  /  B {plant.Rx[1]:0.00}  /  C {plant.Rx[2]:0.00}  /  D {plant.Rx[3]:0.00}    ·    SHUTTERS {plant.MainClosed:P0} / {plant.EmergencyClosed:P0}";
            calibrationText.text=$"{latestEvent}   |   BITS {plant.Bits[3]} · FEC {(fec?"ON":"OFF")}";
            if(controllerDisplay)controllerDisplay.text=rxValid?$"RX {rxSample:0.00}":"RX STALE";
            if(qualityDisplay)qualityDisplay.text=plant.ClocksReady?"4 CLOCKS LOCKED":"WAITING FOR LINK";
            powerButton.interactable=stage<6;safetyButton.interactable=CanSafety;packageButton.interactable=Healthy&&safetyProof&&!package;
            calSourceButton.interactable=CanCalSource;dockButton.interactable=Healthy&&package&&!docked;calRxButton.interactable=CanCalRx;
            sourceButton.interactable=CanSource;tuneAButton.interactable=CanTune(0);tuneBButton.interactable=CanTune(1);
            tuneCButton.interactable=CanTune(2);tuneDButton.interactable=CanTune(3);compButton.interactable=CanComp;
            spectrumButton.interactable=CanSpectrum;budgetButton.interactable=CanBudget;receiverButton.interactable=CanReceiver;
            sendButton.interactable=CanSend;fecButton.interactable=Healthy&&sending&&!fec;testButton.interactable=CanTest;
            driftButton.interactable=CanDrift;thermalButton.interactable=CanThermal;bendButton.interactable=CanBend;
            restoreButton.interactable=Ready&&latch!=Fault.None&&fault!=Disturbance.None;eStopButton.interactable=stage<6;
            resetButton.interactable=CanReset;armButton.interactable=CanArm;verifyButton.interactable=CanVerify;
            certifyButton.interactable=CanCertify&&!certify;
            rig.Apply(plant,rxValid,lastPermit,sending,latch!=Fault.None,scan,armed);
        }
    }
}
