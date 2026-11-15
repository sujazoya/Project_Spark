using System;
using System.Collections.Generic;
using ProjectSpark.Circuit;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.Gameplay
{
    public sealed class SparkLevel68Process : MonoBehaviour
    {
        public static readonly string[] Gates={"L68_Wired","L68_Commission","L68_Cascade","L68_Recovery","L68_Disturbances","L68_Reconnected"};
        public static readonly (int a,int b)[] Wires={(0,1),(2,3),(0,4),(2,5),(0,6),(2,7),(0,8),(2,9),(0,10),(2,11),(0,12),(2,13),(0,14),(2,15),(0,16),(2,17),(0,18),(2,19),(20,21),(22,23),(24,25),(26,27),(28,29),(30,31),(32,33),(34,35),(36,37),(38,39),(40,41),(42,43),(44,45),(46,47),(48,49),(50,51),(52,53),(54,55),(56,57),(58,59),(60,61),(62,63),(64,65),(66,67),(68,69),(70,71),(72,73),(74,75),(76,77),(78,79),(80,81),(82,83),(84,85),(86,87),(88,89),(90,91),(92,93),(94,95),(96,97),(98,99),(100,101),(24,102),(34,103),(42,104),(50,105),(56,106),(58,107),(60,108),(62,109),(86,110),(88,111),(90,112),(92,113),(94,114),(95,115),(65,116),(82,117),(113,118),(45,119),(120,121)};
        private const int StopInput=97, RtdPositive=8, RtdReturn=9, FeedbackWire=77, HeaterWire=31;
        public SparkLevelLayout layout;
        public SparkPowerSupply source;
        public SparkSwitch[] contacts;
        public SparkSwitch emergency;
        public SparkTerminal[] pins;
        public SparkLevel68VisualRig rig;
        public SparkLevel68Trend plot;
        public TMP_Text controllerDisplay,qualityDisplay;
        public Text statusText,meterText,detailText,calibrationText;
        public Button powerButton,calTempButton,calFlowButton,calPressureButton,startHotButton,startColdButton,startProductButton,
            heaterButton,cascadeButton,tuneOuterButton,tuneInnerButton,setTargetButton,regenerateButton,bypassButton,transferButton,
            coldTestButton,surgeButton,foulButton,diagnoseButton,armFeedbackButton,recoverButton,resetButton,certifyButton,stopButton,eStopButton;

        private enum Fault { None, Power, Stop, Wiring, OutletRtd, ThermalLimit, NoCirculation }
        private enum Test { None, Cold, Surge, Fouling }
        private readonly SparkLevel68ThermalPlant plant=new();
        private readonly SparkLevel68ThermalPlant.Pid outer=new(), inner=new();
        private readonly List<SparkCircuitConnection> edges=new();
        private readonly HashSet<(int,int)> wires=new();
        private readonly Queue<string> events=new();
        private static readonly string[] GainNames={"LOW","TUNED","HIGH"};
        private LevelGamePlayManager manager;
        private SparkLevelLayoutLoader loader;
        private SparkLevelSceneBindings bindings;
        private int topology=-1,stage,outerGain,innerGain;
        private bool power,rails,safety,rtdPower,wiredRtd,heaterPath,calTemp,calFlow,calPressure,validRtd,rtdRecalibrated;
        private bool hotRun,coldRun,productRun,heat,cascade,regeneration, bypass=true,transfer,fouled;
        private bool coldPassed,surgePassed,foulingPassed,armed,disconnectObserved,feedbackReset,freshCold,certify,previousHeatGate;
        private float elapsed,accumulator,held,measurementAge,measuredOut=25,measuredHot=46,measuredFlow,measuredDp;
        private float target=65,hotTarget,demand,utilityMean,netUtilityMean,baselineUtility,baselineTemperature,baselineFlow,baselineDp,baselineTrimEff;
        private float sampleTime,startInlet,startProductFlow,minInlet,maxFlow,maxUtility,startUtility,startTrim,minTrim,rangeHigh,rangeLow;
        private float recoveryAnchor,trimAnchor,deliveredAtReset,comparisonSavings,comparisonRecovery,circulationLoss;
        private string wiring,latestEvent="Wire the thermal network and enable 5 V control.";
        private Fault latch;
        private Test scenario;

        private bool Ready=>rails&&safety&&wiring==null;
        private bool Running=>hotRun||coldRun||productRun;
        private bool Calibrated=>calTemp&&calFlow&&calPressure;
        private bool Permit=>Ready&&Calibrated&&latch==Fault.None&&validRtd;
        private bool AtRest=>plant.Pumps[0].Rpm<15&&plant.Pumps[1].Rpm<15&&plant.Pumps[2].Rpm<15;
        private bool FlowOkay=>plant.Pumps[0].Flow>5&&plant.Pumps[1].Flow>4&&plant.Pumps[2].Flow>4;
        private bool OnTarget=>validRtd&&Mathf.Abs(measuredOut-target)<.92f&&Mathf.Abs(plant.Product-target)<1f&&target<=70f;
        private bool StableProduction=>Permit&&Running&&heat&&cascade&&FlowOkay&&OnTarget;
        private bool BypassClosed=>plant.Valves[3]<.025f;
        private bool EndOfTest=>sampleTime>=22f&&scenario!=Test.Fouling;

        private void Start()
        {
            manager=FindFirstObjectByType<LevelGamePlayManager>();
            loader=FindFirstObjectByType<SparkLevelLayoutLoader>();
            if(!manager||!loader||pins.Length!=122||contacts.Length!=1||!rig)
            {
                Debug.LogError("[LEVEL 68] Missing campaign, terminal or motion references.",this);
                enabled=false;
                return;
            }
            source.SetOutput(false,default);
        }

        private static (int,int) Pair(int a,int b)=>(Math.Min(a,b),Math.Max(a,b));
        private bool Routed(int i)=>wires.Contains(Pair(Wires[i].a,Wires[i].b));
        private bool OnlyRtdMissing()
        {
            if(wiredRtd||wires.Count!=Wires.Length-1) return false;
            for(int i=0;i<FeedbackWire;i++) if(!Routed(i)) return false;
            return true;
        }
        private void ReadTopology(SparkCircuitSystem circuit)
        {
            edges.Clear(); wires.Clear();
            circuit.CopyConnections(edges);
            foreach(var edge in edges)
            {
                if(edge.Kind!=SparkConnectionKind.Wire) continue;
                int a=Array.IndexOf(pins,edge.A),b=Array.IndexOf(pins,edge.B);
                if(a>=0||b>=0) wires.Add(Pair(a,b));
            }
            wiredRtd=Routed(FeedbackWire);
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
            rtdPower=power&&Rail(RtdPositive,RtdReturn);
            heaterPath=power&&Rail(Wires[HeaterWire].b,7);
            if(stage==0&&Ready) Award();
            if(latch==Fault.None&&(Running||armed)&&!Ready)
            {
                bool feedbackLost=rails&&safety&&OnlyRtdMissing();
                disconnectObserved|=stage==5&&armed&&feedbackLost;
                Trip(feedbackLost?Fault.OutletRtd:!rails?Fault.Power:!safety?Fault.Stop:Fault.Wiring);
            }
            accumulator+=Mathf.Min(Time.deltaTime,.20f)*2.50f;
            const float dt=SparkLevel68ThermalPlant.Step;
            while(accumulator>=dt){accumulator-=dt;Tick(dt);}
            Present();
        }
        private void Tick(float dt)
        {
            elapsed+=dt;
            bool available=calTemp&&rtdPower&&wiredRtd&&(latch!=Fault.OutletRtd||rtdRecalibrated);
            measurementAge=available?measurementAge+dt:0;
            bool nowValid=measurementAge>=.70f;
            float filter=dt/(.34f+dt);
            if(nowValid) measuredOut=validRtd?Mathf.Lerp(measuredOut,plant.Product,filter):plant.Product;
            validRtd=nowValid;
            measuredHot=Mathf.Lerp(measuredHot,plant.Hot,filter);
            measuredFlow=Mathf.Lerp(measuredFlow,plant.Pumps[2].Flow,filter);
            measuredDp=Mathf.Lerp(measuredDp,plant.DifferentialKpa,filter);

            bool thermalLimit=plant.Product>=72||plant.Hot>=97;
            if(latch==Fault.None&&thermalLimit) Trip(Fault.ThermalLimit);
            circulationLoss=heat&&(!FlowOkay||plant.Supply<.25f)?circulationLoss+dt:0;
            if(latch==Fault.None&&circulationLoss>.40f) Trip(Fault.NoCirculation);

            bool requestHeat=Permit&&heat&&FlowOkay;
            if(requestHeat!=previousHeatGate)
            {
                previousHeatGate=requestHeat;
                contacts[0].SetState(requestHeat?SparkSwitchState.Closed:SparkSwitchState.Open);
                outer.Reset(); inner.Reset();
            }

            demand=0;
            if(requestHeat)
            {
                if(cascade)
                {
                    float e=target-measuredOut;
                    float ff=plant.Preheated+(target-plant.Preheated)/Mathf.Max(.38f,plant.TrimEfficiency);
                    hotTarget=outer.Update(e,outerGain==1?1.60f:outerGain==2?5.5f:.22f,
                        outerGain==1?.29f:outerGain==2?1.3f:.027f,.12f,Mathf.Clamp(ff,45,91),44,94,dt);
                    float baseDemand=plant.TrimKw+.033f*(plant.Hot-20)+.85f*(plant.Hot-plant.Buffer);
                    demand=inner.Update(hotTarget-measuredHot,innerGain==1?6.7f:innerGain==2?18f:1.35f,
                        innerGain==1?1.06f:innerGain==2?7f:.10f,.09f,baseDemand,0,SparkLevel68ThermalPlant.HeaterLimit,dt);
                }
                else
                {
                    hotTarget=68;
                    demand=Mathf.Clamp(plant.TrimKw+1.8f+4.8f*(hotTarget-measuredHot),0,48);
                }
            }
            bool coolTest=scenario==Test.Cold&&sampleTime<16;
            bool surgeActive=scenario==Test.Surge&&sampleTime<14;
            bool run=Permit;
            bool regen=run&&regeneration&&!bypass;
            bool accept=run&&transfer&&stage>=3&&OnTarget&&plant.Discharge<=55;
            plant.Tick(run&&hotRun,run&&coldRun,run&&productRun,requestHeat&&heaterPath&&contacts[0].IsClosed,
                accept,regen,demand,coolTest?15:25,surgeActive?.955f:.75f,fouled,target,dt);

            utilityMean=Mathf.Lerp(utilityMean,plant.HeaterKw,dt/(2.8f+dt));
            netUtilityMean=Mathf.Lerp(netUtilityMean,plant.StorageAdjustedUtility,dt/(2.8f+dt));
            if(stage>=3&&baselineUtility>0&&StableProduction&&Mathf.Abs(measuredFlow-baselineFlow)<.8f)
            {
                comparisonSavings=1-netUtilityMean/baselineUtility;
                float recovery=plant.RecoveredEnergy-recoveryAnchor,trim=plant.TrimEnergy-trimAnchor;
                comparisonRecovery=recovery/Mathf.Max(.001f,recovery+trim);
            }
            ObserveTest(dt);
            CheckObjectives(dt);
            if(plot!=null) plot.Sample(dt,nowValid?measuredOut:0,measuredHot,plant.Inlet,plant.Buffer);
        }

        private void Trip(Fault cause)
        {
            latch=cause;
            hotRun=coldRun=productRun=heat=cascade=false;
            if(cause==Fault.OutletRtd){validRtd=false;measurementAge=0;rtdRecalibrated=false;}
            previousHeatGate=false;
            contacts[0].SetState(SparkSwitchState.Open);
            held=0;scenario=Test.None;
            Log(cause==Fault.OutletRtd?"OUTLET RTD INVALID · heater and product release isolated.":$"THERMAL PROTECTION · {cause} · heater and product release isolated.");
        }
        private void Log(string text)
        {
            latestEvent=$"{elapsed:0.0}s · {text}";
            events.Enqueue(latestEvent);
            if(events.Count>8) events.Dequeue();
        }
        private void Begin(Test test)
        {
            scenario=test;sampleTime=0;
            minInlet=startInlet=plant.Inlet;
            maxFlow=startProductFlow=plant.Pumps[2].Flow;
            maxUtility=startUtility=utilityMean;
            minTrim=startTrim=plant.TrimEfficiency;
            rangeHigh=rangeLow=plant.Product;
            if(test==Test.Fouling){baselineDp=measuredDp;baselineTrimEff=startTrim;fouled=true;}
            Log(test switch {Test.Cold=>"Cold inlet applied to product feed.",Test.Surge=>"Product pump surge applied.",_=>"Deposits added to exchanger."});
        }
        private void ObserveTest(float dt)
        {
            if(scenario==Test.None) return;
            sampleTime+=dt;
            minInlet=Mathf.Min(minInlet,plant.Inlet);
            maxFlow=Mathf.Max(maxFlow,plant.Pumps[2].Flow);
            maxUtility=Mathf.Max(maxUtility,utilityMean);
            minTrim=Mathf.Min(minTrim,plant.TrimEfficiency);
            rangeHigh=Mathf.Max(rangeHigh,plant.Product);
            rangeLow=Mathf.Min(rangeLow,plant.Product);
            if(!EndOfTest) return;
            bool cold=scenario==Test.Cold;
            bool response=cold?startInlet-minInlet>8.5f&&maxUtility-startUtility>.9f:
                maxFlow-startProductFlow>2.2f&&maxUtility-startUtility>.8f;
            bool recovered=StableProduction&&rangeHigh<72&&rangeLow<target+.1f;
            if(!recovered&&sampleTime<44f) return;
            bool passed=response&&recovered;
            if(cold&&stage==4) coldPassed|=passed;
            if(cold&&stage==5&&feedbackReset) freshCold|=passed;
            if(!cold) surgePassed|=passed;
            Log(passed?(cold?"COLD INLET VERIFIED · extra heat and recovery measured.":"FLOW SURGE VERIFIED · flow and utility response measured."):
                "Retry after the product and utility baseline settle.");
            scenario=Test.None;
        }

        private void CheckObjectives(float dt)
        {
            bool goal=stage switch
            {
                1=>Permit&&FlowOkay&&plant.HeaterKw>9&&plant.Hot>52&&plant.Cold>20.2f&&plant.Product>32&&plant.TrimKw>3,
                2=>StableProduction&&outerGain==1&&innerGain==1&&!regeneration&&Mathf.Abs(target-65)<.1f&&Mathf.Abs(utilityMean-netUtilityMean)<3,
                3=>StableProduction&&regenReady&&transfer&&plant.OnSpecLitres>.5f&&Mathf.Abs(measuredFlow-baselineFlow)<.8f&&
                    Mathf.Abs(measuredOut-baselineTemperature)<1.3f&&plant.Preheated>plant.Inlet+13&&plant.RegenerativeKw>10&&
                    comparisonRecovery>.50f&&comparisonSavings>.43f&&Mathf.Abs(utilityMean-netUtilityMean)<4&&plant.BalanceRelative<.001f,
                4=>StableProduction&&coldPassed&&surgePassed&&foulingPassed&&transfer,
                5=>StableProduction&&disconnectObserved&&feedbackReset&&freshCold&&certify&&transfer&&plant.OnSpecLitres>deliveredAtReset+.65f&&
                    plant.BalanceRelative<.001f&&plant.MassError<.04f,
                _=>false
            };
            held=goal?held+dt:0;
            if(held>(stage==2?4.8f:stage==3?4.1f:stage==5?2.8f:2f)) Award();
        }
        private bool regenReady=>regeneration&&!bypass&&BypassClosed;
        private bool DiagnoseReady=>scenario==Test.Fouling&&plant.Fouling>=.55f&&measuredDp-baselineDp>3.7f&&
            plant.TrimEfficiency<baselineTrimEff-.045f&&calPressure&&StableProduction;

        private void Award()
        {
            if(stage>=Gates.Length||!bindings.SetRuntimeComponentBinding(Gates[stage],source)) return;
            if(stage==2){baselineUtility=netUtilityMean;baselineFlow=measuredFlow;baselineTemperature=measuredOut;recoveryAnchor=plant.RecoveredEnergy;trimAnchor=plant.TrimEnergy;}
            stage++;held=0;manager.QueueEvaluation();Log($"OBJECTIVE {stage}/6 verified.");
        }
        public void Power(){power=!power;source.SetOutput(power,default);}
        public void CalTemp(){if(Ready&&!Running&&latch==Fault.None){calTemp=true;measurementAge=0;Log("Temperature channels calibrated.");}}
        public void CalFlow(){if(Ready&&!Running&&latch==Fault.None){calFlow=true;Log("Flow channels calibrated.");}}
        public void CalPressure(){if(Ready&&!Running&&latch==Fault.None){calPressure=true;Log("DP channel calibrated.");}}
        public void StartHot(){if(Permit) hotRun=true;}
        public void StartCold(){if(Permit) coldRun=true;}
        public void StartProduct(){if(Permit&&plant.Supply>1) productRun=true;}
        public void Heater(){if(Permit&&FlowOkay) heat=!heat;}
        public void Cascade(){if(Permit&&stage>=2&&heat){cascade=!cascade;outer.Reset();inner.Reset();}}
        public void TuneOuter(){if(stage==2){outerGain=(outerGain+1)%3;outer.Reset();}}
        public void TuneInner(){if(stage==2){innerGain=(innerGain+1)%3;inner.Reset();}}
        public void SetTarget(){if(stage==2){target=target>=68.9f?63:target+2;outer.Reset();}}
        public void Regenerate(){if(Permit&&stage>=3){regeneration=!regeneration;if(regeneration){recoveryAnchor=plant.RecoveredEnergy;trimAnchor=plant.TrimEnergy;}}}
        public void Bypass(){if(Permit&&stage>=3){bypass=!bypass;if(!bypass){recoveryAnchor=plant.RecoveredEnergy;trimAnchor=plant.TrimEnergy;}}}
        public void Transfer(){if(Permit&&stage>=3) transfer=!transfer;}
        public void ColdTest(){if(CanCold) Begin(Test.Cold);}
        public void Surge(){if(CanSurge) Begin(Test.Surge);}
        public void Foul(){if(CanFoul) Begin(Test.Fouling);}
        public void Diagnose()
        {
            if(!DiagnoseReady) return;
            foulingPassed=true;scenario=Test.None;
            Log("FOULING VERIFIED · DP rose and measured exchanger effectiveness fell.");
        }
        public void ArmFeedback(){if(CanArm){armed=true;Log("Undo W78 · outlet RTD → cascade product temperature input.");}}
        public void Recover()
        {
            if(!CanRecover) return;
            rtdRecalibrated=true;measurementAge=0;validRtd=false;
            Log("Outlet RTD recalibrated. Waiting for fresh samples.");
        }
        public void Reset()
        {
            if(!CanReset) return;
            feedbackReset|=latch==Fault.OutletRtd&&disconnectObserved;armed=false;latch=Fault.None;
            deliveredAtReset=plant.OnSpecLitres;
            Log("Protection reset. Restart all pumps, then heat and cascade.");
        }
        public void Certify(){if(CanCertify) certify=true;}
        public void Stop(){hotRun=coldRun=productRun=heat=cascade=false;scenario=Test.None;}
        public void EStop()
        {
            emergency.SetState(emergency.IsClosed?SparkSwitchState.Open:SparkSwitchState.Closed);
            if(emergency.IsOpen&&latch==Fault.None) Trip(Fault.Stop);
        }

        private bool CanCold=>StableProduction&&regenReady&&transfer&&scenario==Test.None&&(stage==4&&!coldPassed||stage==5&&feedbackReset&&!freshCold);
        private bool CanSurge=>StableProduction&&regenReady&&transfer&&scenario==Test.None&&stage==4&&coldPassed&&!surgePassed;
        private bool CanFoul=>StableProduction&&regenReady&&transfer&&scenario==Test.None&&stage==4&&surgePassed&&!foulingPassed;
        private bool CanArm=>stage==5&&StableProduction&&!armed&&!disconnectObserved&&transfer;
        private bool CanRecover=>latch==Fault.OutletRtd&&Ready&&AtRest&&!rtdRecalibrated;
        private bool CanReset=>latch!=Fault.None&&Ready&&AtRest&&validRtd&&plant.Hot<93&&plant.Product<69&&plant.Supply>1;
        private bool CanCertify=>stage==5&&StableProduction&&freshCold&&plant.OnSpecLitres>deliveredAtReset+.65f&&!certify;

        private string Status()
        {
            if(stage==6) return "CERTIFIED · Stable temperature, heat recovery and verified RTD repair.";
            if(!power) return "Enable 5 V CONTROL and build the thermal network.";
            if(latch==Fault.OutletRtd)
                return !Ready?"OUTLET RTD INVALID · reconnect W78.":CanRecover?"Recalibrate the outlet RTD when pumps stop.":"Await fresh RTD samples, then RESET FAULT.";
            if(latch!=Fault.None) return $"THERMAL PROTECTION LATCHED · {latch}.";
            if(!Ready) return wiring??"Check 5 V rails and the E-STOP contact.";
            if(!Calibrated) return "Calibrate temperature, flow and differential pressure.";
            if(armed) return "Disconnect W78 · OUTLET RTD OUT → PID PRODUCT TEMP IN.";
            if(!hotRun||!coldRun||!productRun) return "Start hot water, cooling water and product circulation.";
            if(!heat) return "Confirm circulation, then ENABLE HEATER.";
            if(scenario!=Test.None) return scenario==Test.Fouling?DiagnoseReady?"Compare pressure drop and exchanger effectiveness. CONFIRM FOULING.":"Observe actual fouling and outlet recovery.":$"TEST · {scenario} · observe process recovery.";
            return stage switch
            {
                1=>"Commissioning · verify actual heater output and temperature rise.",
                2=>"Select TUNED inner and outer gains. Enable CASCADE at 65°C.",
                3=>"Enable REGENERATION, close BYPASS and request PRODUCT TRANSFER.",
                4=>!coldPassed?"Apply COLD INLET.":!surgePassed?"Apply FLOW SURGE.":"Apply FOULING TEST.",
                5=>feedbackReset?!cascade?"Re-enable CASCADE.":freshCold?"Observe stable on-spec output, then CERTIFY.":"Repeat COLD INLET after recovery.":"Arm W78 TEST when product is on-spec.",
                _=>"Wire the thermal process network."
            };
        }

        private void Present()
        {
            statusText.text=Status();
            string outletTemp=validRtd?$"{measuredOut:0.0}°C":"INVALID";
            meterText.text=$"OUTLET {outletTemp} / {target:0}°C  ·  INLET {plant.Inlet:0.0}°C  ·  HOT {plant.Hot:0.0}°C  ·  COOL {plant.Cold:0.0}°C  ·  BUFFER {plant.Buffer:0.0}°C";
            detailText.text=$"PRODUCT {plant.Pumps[2].Flow:0.0} L/min  ·  HEAT {plant.HeaterKw:0.0} kW  ·  REGEN {plant.RegenerativeKw:0.0} kW  ·  DP {measuredDp:0.0} kPa  ·  EFFECTIVENESS {plant.TrimEfficiency:P0}";
            calibrationText.text=$"{latestEvent}  |  PID {GainNames[outerGain]}/{GainNames[innerGain]}  ·  SAVE {Mathf.Max(0,comparisonSavings):P0}  ·  ON-SPEC {plant.OnSpecLitres:0.0} L";
            if(controllerDisplay) controllerDisplay.text=$"{plant.HeaterKw:0.0} kW · {plant.Hot:0.0}°C";
            if(qualityDisplay) qualityDisplay.text=validRtd?$"OUTLET {measuredOut:0.0}°C":"OUTLET RTD INVALID";

            powerButton.interactable=stage<6;
            calTempButton.interactable=Ready&&!calTemp&&latch==Fault.None;
            calFlowButton.interactable=Ready&&!calFlow&&latch==Fault.None;
            calPressureButton.interactable=Ready&&!calPressure&&latch==Fault.None;
            startHotButton.interactable=Permit&&!hotRun&&stage<6;
            startColdButton.interactable=Permit&&!coldRun&&stage<6;
            startProductButton.interactable=Permit&&!productRun&&plant.Supply>1&&stage<6;
            heaterButton.interactable=Permit&&FlowOkay&&stage<6;
            cascadeButton.interactable=Permit&&heat&&stage is >=2 and <6;
            tuneOuterButton.interactable=tuneInnerButton.interactable=setTargetButton.interactable=stage==2;
            regenerateButton.interactable=bypassButton.interactable=transferButton.interactable=Permit&&stage is >=3 and <6;
            coldTestButton.interactable=CanCold;
            surgeButton.interactable=CanSurge;
            foulButton.interactable=CanFoul;
            diagnoseButton.interactable=DiagnoseReady;
            armFeedbackButton.interactable=CanArm;
            recoverButton.interactable=CanRecover;
            resetButton.interactable=CanReset;
            certifyButton.interactable=CanCertify;
            stopButton.interactable=Running;
            eStopButton.interactable=stage<6;
            rig.Apply(plant,elapsed,emergency.IsOpen,!Ready||latch!=Fault.None);
        }
    }
}



