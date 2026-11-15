using System;
using System.Collections.Generic;
using ProjectSpark.Circuit;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.Gameplay
{
    public sealed class SparkLevel67Process : MonoBehaviour
    {
        public static readonly string[] Gates = { "L67_Wired", "L67_Commission", "L67_Cascade", "L67_Recipe", "L67_Disturbances", "L67_Recovered" };
        public static readonly (int a,int b)[] Wires = { (0,1),(2,3),(0,4),(2,5),(0,6),(2,7),(0,8),(2,9),(0,10),(2,11),(0,12),(2,13),(0,14),(2,15),(0,16),(2,17),(0,18),(2,19),(20,21),(22,23),(24,25),(26,27),(28,29),(30,31),(32,33),(34,35),(36,37),(38,39),(40,41),(42,43),(44,45),(46,47),(48,49),(50,51),(52,53),(54,55),(56,57),(58,59),(60,61),(62,63),(64,65),(66,67),(68,69),(70,71),(72,73),(74,75),(76,77),(78,79),(80,81),(82,83),(84,85),(86,87),(88,89),(90,91),(92,93),(94,95),(96,97),(33,98),(35,99),(37,100),(38,101),(40,102),(44,103),(46,104),(48,105),(50,106),(52,107),(74,108),(88,109),(43,110),(82,111),(66,112),(78,113),(114,115) };
        private const int StopInput=85, FlowBPositive=10, FlowBReturn=11, FeedbackWire=73;
        public SparkLevelLayout layout;
        public SparkPowerSupply source;
        public SparkSwitch[] contacts;
        public SparkSwitch emergency;
        public SparkTerminal[] pins;
        public SparkLevel67VisualRig rig;
        public SparkLevel67Trend plot;
        public TMP_Text controllerDisplay,qualityDisplay;
        public Text statusText,meterText,detailText,calibrationText;
        public Button powerButton,calibrateButton,primeAButton,primeBButton,primePButton,startAButton,startBButton,startPButton,
            mixerButton,cascadeButton,tuneLevelButton,tuneFlowButton,setLevelButton,setFlowButton,setRecipeButton,ratioButton,
            produceButton,outletButton,surgeButton,pressureButton,failBButton,armFeedbackButton,recoverButton,resetButton,certifyButton,
            stopButton,eStopButton;

        private enum Test { None, Demand, Pressure, PumpB }
        private enum Fault { None, Power, Stop, Wiring, FlowB, PumpB, Hydraulic }
        private readonly SparkLevel67Hydraulics plant = new();
        private readonly SparkLevel67Hydraulics.Controller outer = new(), inner = new(), adjustA = new(), adjustB = new();
        private readonly List<SparkCircuitConnection> edges = new();
        private readonly HashSet<(int,int)> wires = new();
        private readonly Queue<string> events = new();
        private static readonly string[] GainProfiles = { "LOW", "TUNED", "HIGH" };
        private LevelGamePlayManager manager;
        private SparkLevelLayoutLoader loader;
        private SparkLevelSceneBindings bindings;
        private int topology=-1, stage, flowPreset, levelPreset;
        private bool power, rails, safety, calibrated, validB, startedA, startedB, startedP, mixer, cascade, ratio, production, outlet;
        private bool demandPassed, pressurePassed, failureObserved, failureReset, feedbackArmed, feedbackObserved, feedbackReset, freshDemand, certRequested;
        private bool feedbackRail, feedbackConnected, hydraulicStop, lastPermit;
        private float time, accumulator, stable, sensorAge, feedbackAge, flowA, flowB, flowP, level, mixQuality=.70f, productQuality=.70f;
        private float totalSetpoint=20, levelSetpoint=3, recipe=.70f, flowSetpoint, onSpecLitres, restartCompliant, priorTransferred;
        private float testClock, testLevel, testPressure, testWithdrawal, maxWithdrawal, minLevel, minPressure, maxTransfer, hydraulicUnsafe;
        private Test scenario;
        private Fault latch;
        private string wiring, latestEvent="Connect the process network and enable 5 V control.";

        private bool Ready => rails && safety && wiring == null;
        private bool Permitted => Ready && latch == Fault.None && calibrated && validB && sensorAge>.24f;
        private bool Running => startedA || startedB || startedP;
        private bool Settled => plant.Pumps[0].Rpm<12 && plant.Pumps[1].Rpm<12 && plant.Pumps[2].Rpm<12;
        private bool OnRecipe => validB && flowA>2 && flowB>1 && Mathf.Abs(mixQuality-recipe)<=.025f &&
            Mathf.Abs(productQuality-recipe)<=.025f && Mathf.Abs(plant.Tanks[2].FractionA-recipe)<=.025f &&
            Mathf.Abs(plant.Tanks[3].FractionA-recipe)<=.025f && Mathf.Abs(flowA/(flowA+flowB)-recipe)<=.055f;
        private bool PlantStable => Running && startedP && validB && Mathf.Abs(level-levelSetpoint)<.40f &&
            Mathf.Abs(flowP-flowSetpoint)<3.3f && flowP>10 && !hydraulicStop;
        private bool DisturbanceBusy => scenario!=Test.None;

        private void Start()
        {
            manager=FindFirstObjectByType<LevelGamePlayManager>();
            loader=FindFirstObjectByType<SparkLevelLayoutLoader>();
            if(!manager||!loader||pins.Length!=116||contacts.Length!=1||!rig)
            {
                Debug.LogError("[LEVEL 67] Missing the level manager, control circuit or motion references.",this);
                enabled=false;
                return;
            }
            source.SetOutput(false,default);
        }

        private static (int,int) Pair(int a,int b) => (Math.Min(a,b),Math.Max(a,b));
        private bool Routed(int i) => wires.Contains(Pair(Wires[i].a,Wires[i].b));
        private bool OnlyFeedbackMissing()
        {
            if (feedbackConnected || wires.Count!=Wires.Length-1) return false;
            for(int i=0;i<FeedbackWire;i++) if(!Routed(i)) return false;
            return true;
        }
        private void ReadTopology(SparkCircuitSystem circuit)
        {
            circuit.CopyConnections(edges);
            wires.Clear();
            foreach(var edge in edges)
            {
                if(edge.Kind!=SparkConnectionKind.Wire) continue;
                int a=Array.IndexOf(pins,edge.A), b=Array.IndexOf(pins,edge.B);
                if(a>=0||b>=0) wires.Add(Pair(a,b));
            }
            feedbackConnected=Routed(FeedbackWire);
            wiring=wires.Count==Wires.Length?null:"Remove unexpected connections or complete the wiring.";
            for(int i=0;i<Wires.Length;i++)
                if(!Routed(i))
                {
                    wiring=$"Connect W{i+1:00} · {pins[Wires[i].a].name} → {pins[Wires[i].b].name}.";
                    break;
                }
        }
        private void Update()
        {
            if(loader.ActiveLayout!=layout || loader.ActiveBindings==null) return;
            var circuit=manager.CircuitSystem;
            var solver=manager.ElectricalSolver;
            if(!circuit||!solver) return;
            if(bindings!=loader.ActiveBindings) { bindings=loader.ActiveBindings; topology=-1; }
            if(topology!=circuit.TopologyVersion) { topology=circuit.TopologyVersion; ReadTopology(circuit); }
            var result=solver.LatestSolveResult;
            bool Rail(int a,int b) => result!=null && result.TryGetVoltage(pins[a],pins[b],out float v) && v is >=4.7f and <=5.3f;
            rails=power && source.IsOutputActive && !source.IsOverloaded;
            for(int i=0;i<18&&rails;i+=2) rails=Rail(Wires[i].b,Wires[i+1].b);
            feedbackRail=power && Rail(FlowBPositive,FlowBReturn);
            safety=emergency.IsClosed && Rail(StopInput,7);
            if(stage==0 && Ready) Award();
            if(latch==Fault.None && (Running||feedbackArmed) && !Ready)
            {
                bool realLoss=rails && safety && OnlyFeedbackMissing();
                feedbackObserved|=stage==5 && feedbackArmed && realLoss;
                Trip(realLoss?Fault.FlowB:!power||!rails?Fault.Power:!safety?Fault.Stop:Fault.Wiring);
            }
            accumulator += Mathf.Min(Time.deltaTime,.20f)*1.35f;
            const float dt=SparkLevel67Hydraulics.Step;
            while(accumulator>=dt) { accumulator-=dt; Tick(dt); }
            Present();
        }

        private void Tick(float dt)
        {
            time+=dt;
            sensorAge=calibrated&&rails ? sensorAge+dt : 0;
            feedbackAge=calibrated&&feedbackConnected&&feedbackRail && (latch!=Fault.FlowB||feedbackRecalibrated) ? feedbackAge+dt : 0;
            validB=feedbackAge>.5f;
            bool permit=Permitted;
            if(permit!=lastPermit)
            {
                lastPermit=permit;
                contacts[0].SetState(permit?SparkSwitchState.Closed:SparkSwitchState.Open);
                inner.Reset(); outer.Reset(); adjustA.Reset(); adjustB.Reset();
            }
            bool drive=permit && contacts[0].IsClosed;
            bool runningA=drive && startedA, runningB=drive && startedB && scenario!=Test.PumpB;
            bool pumping=drive && startedP && plant.Tanks[2].Volume>.40f;
            float dose=totalSetpoint;
            if(ratio && pumping)
                dose=Mathf.Clamp(flowSetpoint+(1.8f-plant.Tanks[2].Volume)*3.8f,12,30);
            dose*=Mathf.InverseLerp(7.15f,4.5f,plant.Tanks[2].Volume);
            float split=ratio?recipe:.70f;
            float targetA=dose*split, targetB=dose*(1-split);
            float a=runningA&&dose>1.5f?adjustA.Update(targetA-flowA,.027f,.055f,0,.018f*targetA+.33f,.24f,.97f,dt):0;
            float b=runningB&&dose>1.5f?adjustB.Update(targetB-flowB,.058f,.087f,0,.042f*targetB+.28f,.20f,.98f,dt):0;
            float transfer=0;
            if(pumping)
            {
                float flowKp=flowPreset==1?.036f:flowPreset==0?.009f:.145f;
                float flowKi=flowPreset==1?.070f:flowPreset==0?.009f:.19f;
                float levelKp=levelPreset==1?11:levelPreset==0?1.1f:34f;
                float levelKi=levelPreset==1?2.2f:levelPreset==0?.18f:7.5f;
                flowSetpoint=cascade?outer.Update(levelSetpoint-level,levelKp,levelKi,.24f,totalSetpoint,9,30,dt):totalSetpoint;
                transfer=cascade?inner.Update(flowSetpoint-flowP,flowKp,flowKi,.003f,flowSetpoint*.022f+.18f,0,1,dt):.61f;
            }
            else flowSetpoint=0;
            bool mixing=drive&&mixer;
            bool qualityPermit=OnRecipe && plant.MixerRpm>330;
            bool release=drive && outlet && qualityPermit && (stage==2||ratio&&production) && scenario!=Test.PumpB;
            bool transferPermit=pumping && qualityPermit && (stage==2||ratio&&production) && scenario!=Test.PumpB;
            float loss=scenario==Test.Pressure ? .65f*Mathf.Clamp01(testClock/.75f) : 0;
            float demand=scenario==Test.Demand && testClock<7f?24f:20f;
            plant.Tick(new Vector3(a,b,transferPermit?transfer:0),runningA,runningB,transferPermit,mixing,release,drive,loss,demand,dt);
            if(sensorAge>.20f)
            {
                float filter=dt/(.22f+dt);
                flowA=Mathf.Lerp(flowA,plant.Pumps[0].Flow,filter);
                flowB=validB?Mathf.Lerp(flowB,plant.Pumps[1].Flow,filter):0;
                flowP=Mathf.Lerp(flowP,plant.Pumps[2].Flow,filter);
                level=Mathf.Lerp(level,plant.Tanks[3].Volume,filter);
                mixQuality=Mathf.Lerp(mixQuality,plant.MixingFraction,filter);
                productQuality=Mathf.Lerp(productQuality,plant.Tanks[3].FractionA,filter);
            }
            else flowA=flowB=flowP=0;

            hydraulicStop=plant.Pumps[0].Cavitating||plant.Pumps[1].Cavitating||plant.Pumps[2].Cavitating||
                plant.Tanks[2].Volume>7.85f || plant.Tanks[3].Volume>7.85f ||
                runningA&&plant.Tanks[0].Volume<.5f || runningB&&plant.Tanks[1].Volume<.5f ||
                plant.Pumps[0].Head>13 || plant.Pumps[1].Head>13 || plant.Pumps[2].Head>13;
            hydraulicUnsafe=hydraulicStop?hydraulicUnsafe+dt:0;
            if(hydraulicUnsafe>.5f && latch==Fault.None) Trip(Fault.Hydraulic);
            float received=plant.TransferTotal-priorTransferred;
            priorTransferred=plant.TransferTotal;
            if(received>0 && transferPermit && ratio && production && Mathf.Abs(recipe-.70f)<.001f && OnRecipe)
            {
                onSpecLitres+=received;
                if(feedbackReset) restartCompliant+=received;
            }
            ObserveTest(dt);
            CheckObjectives(dt);
            plot?.Sample(dt,flowA,validB?flowB:0,flowP,level);
        }

        private bool feedbackRecalibrated;
        private void Trip(Fault cause)
        {
            latch=cause;
            startedA=startedB=startedP=false;
            if(cause==Fault.FlowB)
            {
                validB=false;
                feedbackAge=0;
                feedbackRecalibrated=false;
                flowB=0;
            }
            contacts[0].SetState(SparkSwitchState.Open);
            lastPermit=false;
            scenario=Test.None;
            stable=0;
            Log(cause==Fault.FlowB?"FLOW B SIGNAL LOST · dosing and release isolated.":$"PROTECT · {cause} · dosing and release isolated.");
        }

        private void ObserveTest(float dt)
        {
            if(scenario==Test.None) return;
            testClock+=dt;
            maxWithdrawal=Mathf.Max(maxWithdrawal,plant.WithdrawalFlow);
            maxTransfer=Mathf.Max(maxTransfer,plant.Pumps[2].Flow);
            minLevel=Mathf.Min(minLevel,plant.Tanks[3].Volume);
            minPressure=Mathf.Min(minPressure,plant.Pumps[0].Suction);
            if(scenario==Test.PumpB)
            {
                if(validB && flowB<1.8f && plant.Pumps[1].Rpm<220 && plant.WithdrawalFlow<.01f)
                {
                    failureObserved=true;
                    Trip(Fault.PumpB);
                }
                return;
            }
            if(scenario==Test.Demand && testClock>=11f)
            {
                bool passed=maxWithdrawal>testWithdrawal+2.1f && minLevel<testLevel-.075f &&
                    maxTransfer>20.7f && Mathf.Abs(level-levelSetpoint)<.45f;
                if(stage==4) demandPassed|=passed;
                if(stage==5 && feedbackReset) freshDemand|=passed;
                scenario=Test.None;
                Log(passed?"DEMAND VERIFIED · withdrawal stepped, level dipped and PID recovered.":"DEMAND RETRY · let level and flow settle before the surge.");
            }
            if(scenario==Test.Pressure && testClock>=8f)
            {
                pressurePassed|=testPressure-minPressure>.35f && OnRecipe && !hydraulicStop;
                scenario=Test.None;
                Log(pressurePassed?"PRESSURE VERIFIED · suction fell and recipe was maintained.":"PRESSURE RETRY · allow measured ratio to settle.");
            }
        }

        private void CheckObjectives(float dt)
        {
            bool condition=stage switch
            {
                1 => Permitted&&startedA&&startedB&&plant.Pumps[2].Primed&&plant.MixerRpm>330&&
                    flowA>11&&flowB>4&&plant.Pumps[0].Head>.8f&&plant.Pumps[1].Head>.6f&&plant.Tanks[2].Volume>1.3f,
                2 => Permitted&&cascade&&flowPreset==1&&levelPreset==1&&PlantStable&&outlet&&plant.WithdrawalFlow>11,
                3 => Permitted&&production&&ratio&&Mathf.Abs(recipe-.70f)<.001f&&OnRecipe&&onSpecLitres>.80f&&PlantStable,
                4 => Permitted&&demandPassed&&pressurePassed&&failureObserved&&failureReset&&OnRecipe&&PlantStable&&outlet,
                5 => Permitted&&feedbackObserved&&feedbackReset&&freshDemand&&certRequested&&restartCompliant>.75f&&
                    Mathf.Abs(recipe-.70f)<.001f&&OnRecipe&&PlantStable&&plant.MassError<.025f,
                _ => false
            };
            stable=condition?stable+dt:0;
            if(stable>(stage==2?3.7f:1.15f)) Award();
        }

        private void Award()
        {
            if(stage>=Gates.Length || !bindings.SetRuntimeComponentBinding(Gates[stage],source)) return;
            stage++; stable=0; manager.QueueEvaluation();
            Log($"OBJECTIVE {stage}/6 verified.");
        }
        private void Log(string message)
        {
            latestEvent=$"{time:0.0}s · {message}";
            events.Enqueue(latestEvent);
            if(events.Count>6) events.Dequeue();
        }
        private void StartTest(Test test)
        {
            scenario=test; testClock=0; testLevel=level; testPressure=plant.Pumps[0].Suction;
            testWithdrawal=plant.WithdrawalFlow; maxWithdrawal=maxTransfer=0; minLevel=level; minPressure=testPressure;
            Log(test switch { Test.Demand=>"Demand surge started.",Test.Pressure=>"Supply pressure loss started.",_=>"Pump B drive availability lost." });
        }
        public void Power(){power=!power;source.SetOutput(power,default);}
        public void Calibrate(){if(Ready&&!Running&&latch==Fault.None){calibrated=true;sensorAge=feedbackAge=0;Log("Flow, level, pressure and QA transmitters calibrated.");}}
        public void PrimeA(){if(Ready&&plant.Tanks[0].Volume>.5f) plant.Pumps[0].Primed=true;}
        public void PrimeB(){if(Ready&&plant.Tanks[1].Volume>.5f) plant.Pumps[1].Primed=true;}
        public void PrimeP(){if(Ready&&plant.Tanks[2].Volume>1.15f) plant.Pumps[2].Primed=true;}
        public void StartA(){if(Permitted&&plant.Pumps[0].Primed) startedA=true;}
        public void StartB(){if(Permitted&&plant.Pumps[1].Primed) startedB=true;}
        public void StartP(){if(Permitted&&plant.Pumps[2].Primed&&stage>=2) startedP=true;}
        public void Mixer(){if(Permitted) mixer=!mixer;}
        public void Cascade(){if(Permitted&&stage>=2){cascade=!cascade;outer.Reset();inner.Reset();}}
        public void TuneLevel(){if(stage==2){levelPreset=(levelPreset+1)%3;outer.Reset();}}
        public void TuneFlow(){if(stage==2){flowPreset=(flowPreset+1)%3;inner.Reset();}}
        public void SetLevel(){if(stage==2) levelSetpoint=levelSetpoint>=3.49f?2.5f:levelSetpoint+.5f;}
        public void SetFlow(){if(stage is 2 or 3) totalSetpoint=totalSetpoint>=21.9f?18:totalSetpoint+2;}
        public void SetRecipe(){if(stage==3) recipe=recipe>=.749f?.65f:recipe+.05f;}
        public void Ratio(){if(Permitted&&stage>=3) ratio=!ratio;}
        public void Produce(){if(Permitted&&stage>=3&&ratio) production=!production;}
        public void Outlet(){if(Permitted&&stage>=2) outlet=!outlet;}
        public void Surge(){if(CanSurge) StartTest(Test.Demand);}
        public void Pressure(){if(CanPressure) StartTest(Test.Pressure);}
        public void FailB(){if(CanFail) StartTest(Test.PumpB);}
        public void ArmFeedback(){if(CanArm){feedbackArmed=true;Log("Undo W74 · Flow B output → ratio controller B input.");}}
        public void Recover()
        {
            if(!CanRecover) return;
            feedbackRecalibrated=true; feedbackAge=0; flowB=0;
            Log("Flow B calibrated. Waiting for fresh feedback.");
        }
        public void Reset()
        {
            if(!CanReset) return;
            failureReset|=latch==Fault.PumpB && failureObserved;
            feedbackReset|=latch==Fault.FlowB && feedbackObserved;
            feedbackArmed=false;
            latch=Fault.None;
            Log("Protection reset. Restart A, B and transfer.");
        }
        public void Certify(){if(CanCertify) certRequested=true;}
        public void Stop()
        {
            startedA=startedB=startedP=false;
            if(scenario==Test.PumpB) Trip(Fault.PumpB);
            else scenario=Test.None;
        }
        public void EStop()
        {
            emergency.SetState(emergency.IsClosed?SparkSwitchState.Open:SparkSwitchState.Closed);
            if(emergency.IsOpen && latch==Fault.None) Trip(Fault.Stop);
        }
        private bool CanSurge=>Permitted&&PlantStable&&outlet&&OnRecipe&&!DisturbanceBusy&&
            (stage==4&&!demandPassed||stage==5&&feedbackReset&&!freshDemand);
        private bool CanPressure=>Permitted&&PlantStable&&OnRecipe&&stage==4&&demandPassed&&!pressurePassed&&!DisturbanceBusy;
        private bool CanFail=>Permitted&&PlantStable&&OnRecipe&&stage==4&&pressurePassed&&!failureObserved&&!DisturbanceBusy;
        private bool CanArm=>Permitted&&stage==5&&PlantStable&&OnRecipe&&!feedbackObserved&&!feedbackArmed&&latch==Fault.None;
        private bool CanRecover=>latch==Fault.FlowB&&Ready&&Settled&&!feedbackRecalibrated;
        private bool CanReset=>latch!=Fault.None&&Ready&&Settled&&validB&&!hydraulicStop;
        private bool CanCertify=>stage==5&&feedbackReset&&freshDemand&&restartCompliant>.75f&&OnRecipe&&PlantStable&&!certRequested;
        private string Status()
        {
            if(stage==6) return "CERTIFIED · stable 70/30 production and fresh disturbance recovery.";
            if(!power) return "Enable 5 V CONTROL before commissioning.";
            if(latch==Fault.FlowB) return !Ready?"FLOW B SIGNAL LOST · reconnect W74.":CanRecover?"Recalibrate FLOW B after the pumps stop.":"Await fresh Flow B data, then RESET FAULT.";
            if(latch!=Fault.None) return $"PROTECTION LATCHED · {latch} · reset when safe.";
            if(!Ready) return wiring??"Verify the control rails and emergency stop contact.";
            if(DisturbanceBusy) return $"TEST · {scenario} · monitor live flow and level.";
            if(feedbackArmed) return "Disconnect W74 with Undo Last Wire · FLOW B OUT → RATIO B IN.";
            if(!calibrated) return "CALIBRATE · Flow, level, pressure and quality transmitters.";
            if(stage>=2&&(!startedA||!startedB||!startedP)) return "Start A, B and transfer. Verify the mixer and outlet.";
            return stage switch
            {
                1=>"Prime and start A/B, enable MIXER, then prime transfer as the vessel fills.",
                2=>"Set level and flow gains to TUNED. Enable CASCADE and open PRODUCT OUTLET.",
                3=>"Enable RATIO CONTROL and PRODUCTION. Verify 70/30 and compliant volume.",
                4=>!demandPassed?"Apply DEMAND SURGE.":!pressurePassed?"Apply PRESSURE LOSS.":!failureObserved?"Apply PUMP B FAILURE.":"Restore and stabilize the process.",
                5=>feedbackReset?freshDemand?"Wait for fresh on-spec volume, then CERTIFY.":"Repeat DEMAND SURGE under restored feedback.":"Arm W74 TEST while product is on-spec.",
                _=>"Wire the smart process plant."
            };
        }
        private void Present()
        {
            statusText.text=Status();
            meterText.text=$"A {flowA:0.0}  ·  B {(validB?flowB.ToString("0.0"):"INVALID")}  ·  PRODUCT {flowP:0.0} L/min    |    SP {flowSetpoint:0.0}    |    BLEND {productQuality:P1} A";
            detailText.text=$"A {plant.Tanks[0].Volume:0.0} L  ·  B {plant.Tanks[1].Volume:0.0} L  ·  MIX {plant.Tanks[2].Volume:0.00} L  ·  PRODUCT {plant.Tanks[3].Volume:0.00}/{levelSetpoint:0.0} L  ·  SUCTION {plant.Pumps[0].Suction*9.79f:0.0} kPa";
            string[] modes={"LOW","TUNED","HIGH"};
            calibrationText.text=$"{latestEvent}   |   PID L {GainProfiles[levelPreset]} / F {GainProfiles[flowPreset]}   ·   {totalSetpoint:0} L/min   ·   {recipe:P0} A   ·   {onSpecLitres:0.0} L on spec";
            if(controllerDisplay) controllerDisplay.text=$"{plant.Pumps[0].Rpm:0}/{plant.Pumps[1].Rpm:0} RPM";
            if(qualityDisplay) qualityDisplay.text=validB?$"{productQuality:P0} A · {plant.Pumps[2].Head*9.79f:0} kPa":"FLOW B INVALID";

            powerButton.interactable=stage<6;
            calibrateButton.interactable=Ready&&!Running&&!calibrated&&latch==Fault.None;
            primeAButton.interactable=Ready&&!plant.Pumps[0].Primed;
            primeBButton.interactable=Ready&&!plant.Pumps[1].Primed;
            primePButton.interactable=Ready&&!plant.Pumps[2].Primed&&plant.Tanks[2].Volume>1.15f;
            startAButton.interactable=Permitted&&!startedA&&plant.Pumps[0].Primed&&stage<6;
            startBButton.interactable=Permitted&&!startedB&&plant.Pumps[1].Primed&&stage<6;
            startPButton.interactable=Permitted&&!startedP&&plant.Pumps[2].Primed&&stage is >=2 and <6;
            mixerButton.interactable=Permitted&&stage<6;
            cascadeButton.interactable=Permitted&&stage is >=2 and <6;
            tuneLevelButton.interactable=tuneFlowButton.interactable=setLevelButton.interactable=stage==2;
            setFlowButton.interactable=stage is 2 or 3;
            setRecipeButton.interactable=stage==3;
            ratioButton.interactable=Permitted&&stage is >=3 and <6;
            produceButton.interactable=Permitted&&ratio&&stage is >=3 and <6;
            outletButton.interactable=Permitted&&stage is >=2 and <6;
            surgeButton.interactable=CanSurge;
            pressureButton.interactable=CanPressure;
            failBButton.interactable=CanFail;
            armFeedbackButton.interactable=CanArm;
            recoverButton.interactable=CanRecover;
            resetButton.interactable=CanReset;
            certifyButton.interactable=CanCertify;
            stopButton.interactable=Running;
            eStopButton.interactable=stage<6;
            rig.Apply(plant,time,emergency.IsOpen,!Ready||latch!=Fault.None);
        }
    }
}





