using System;
using System.Collections.Generic;
using ProjectSpark.Circuit;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.Gameplay
{
    public sealed class SparkLevel71Process : MonoBehaviour
    {
        public static readonly string[] Gates={"L71_Wired","L71_Vacuum","L71_Plasma","L71_Etched","L71_Disturbances","L71_Reconnected"};
        public static readonly (int a,int b)[] Wires={(0,1),(2,3),(0,4),(2,5),(0,6),(2,7),(0,8),(2,9),(0,10),(2,11),(12,13),(14,15),(16,17),(18,19),(20,21),(22,23),(24,25),(26,27),(28,29),(30,31),(32,33),(34,35),(36,37),(38,39),(40,41),(42,43),(44,45),(46,47),(48,49),(50,51),(52,53),(54,55),(56,57),(58,59),(60,61),(62,63),(64,65),(66,67),(68,69),(70,71),(72,73),(74,75),(76,77),(78,79),(80,81),(82,83),(84,85),(86,87),(88,89),(90,91),(92,93),(94,95),(96,97),(98,99),(100,101),(102,103),(101,104),(102,105),(23,106),(33,107),(102,108),(106,109),(27,110),(100,111),(87,112),(99,113),(49,114),(51,115),(29,116),(33,117),(99,118),(81,119),(83,120),(71,121),(102,122),(86,123),(124,125),(85,126),(31,127),(125,128),(95,129),(89,130),(130,131),(99,132),(69,133),(71,134),(99,135),(29,136),(49,137),(138,139)};
        private const int StopInput=125, SafeReturn=3, OpticalPower=10, OpticalReturn=11, Feedback=89, RfWire=36;
        public SparkLevelLayout layout;
        public SparkPowerSupply source;
        public SparkSwitch[] contacts;
        public SparkSwitch emergency;
        public SparkTerminal[] pins;
        public SparkLevel71VisualRig rig;
        public SparkLevel71Trend plot;
        public TMP_Text controllerDisplay,qualityDisplay;
        public Text statusText,meterText,detailText,calibrationText;
        public Button powerButton,calPressureButton,calGasButton,calRfButton,calOpticalButton,backingButton,turboButton,pressurePidButton,
            isolationButton,throttleButton,loadButton,clampButton,chillerButton,setAButton,setBButton,gasButton,rfButton,setPowerButton,
            tuneAButton,tuneBButton,matchButton,igniteButton,etchButton,endpointButton,stopButton,metrologyButton,
            leakButton,driftButton,coolingButton,eStopButton,resetButton,armButton,recalButton,certifyButton;

        private enum Fault { None, Power, EStop, Wiring, Optical, Temperature, Arc, Vacuum }
        private enum Scenario { None, Leak, Drift, Cooling }
        private readonly SparkLevel71EtchPlant plant=new();
        private readonly SparkLevel71EtchPlant.Pi pid=new();
        private readonly HashSet<(int,int)> wires=new();
        private readonly List<SparkCircuitConnection> edges=new();
        private readonly Queue<string> events=new();
        private LevelGamePlayManager manager;
        private SparkLevelLayoutLoader loader;
        private SparkLevelSceneBindings bindings;
        private int topology=-1,acceptedBatches;
        private int stage;
        private bool power,rails,safety,opticalPower,wiredOptical,rfPath,previousContact;
        private bool calPressure,calGas,calRf,calOptical,validOptical,recalibrated;
        private bool backing,turbo,isolation,autoPressure,clamped,loaded,chiller,gas,rf,matching,ignite,etching,endpointEnabled;
        private bool endpointConfirmed,measured,leakPassed,driftPassed,coolingPassed,coolingReset,armed,opticalObserved,opticalReset,certify;
        private float elapsed,accumulator,hold,age,pressure=500,spectral,endpointDwell,loadClock,thermalStart,metrologyTime,arcDwell;
        private float capA=.24f,capB=.71f,manualThrottle=1,gasA=.68f,gasB=.42f,commandPower=.76f;
        private float testClock,baselineP,baselineReflection,peakP,peakReflection;
        private string wiring,latestEvent="Connect the plasma workstation and enable 5 V control.";
        private Fault latch;
        private Scenario scenario;

        private bool Ready=>rails&&safety&&wiring==null;
        private bool Pumping=>backing&&turbo&&isolation&&plant.Backing>.88f&&plant.Turbo>.88f&&plant.Isolation>.90f;
        private bool WaferTransferReady=>Ready&&calPressure&&Pumping&&!gas&&!rf&&!etching&&loadClock<=0&&
            plant.Pressure<2.5f&&Mathf.Abs(plant.Pressure-plant.LockPressure)<2.2f&&latch==Fault.None;
        private bool ProcessBase=>Ready&&Pumping&&gas&&calPressure&&calGas&&calRf&&loaded&&clamped&&chiller&&
            plant.FlowA+plant.FlowB>.60f&&plant.Pressure is >3f and <14.5f&&latch==Fault.None&&loadClock<=0&&plant.Gate<.025f;
        private bool PlasmaReady=>ProcessBase&&rf&&ignite&&plant.Activity>.53f&&plant.Reflection<.075f&&
            Mathf.Abs(pressure-SparkLevel71EtchPlant.SetPressure)<.70f&&plant.Temperature<51f;
        private bool CanProcess=>PlasmaReady&&validOptical&&endpointEnabled&&!etching&&!endpointConfirmed&&plant.MeanFilm>270;
        private bool CanArm=>stage==5&&etching&&PlasmaReady&&validOptical&&!armed&&!opticalObserved;
        private bool CanRecal=>latch==Fault.Optical&&Ready&&!recalibrated;
        private bool CanReset=>latch!=Fault.None&&Ready&&!rf&&plant.Activity<.03f&&plant.Temperature<45&&
            (latch!=Fault.Optical||validOptical);
        private bool CanCertify=>stage==5&&opticalReset&&measured&&acceptedBatches>=3&&latch==Fault.None;

        private void Start()
        {
            manager=FindFirstObjectByType<LevelGamePlayManager>();loader=FindFirstObjectByType<SparkLevelLayoutLoader>();
            if(!manager||!loader||pins.Length!=140||contacts.Length!=1||!rig){Debug.LogError("[LEVEL 71] Missing reactor references.",this);enabled=false;return;}
            source.SetOutput(false,default);
        }
        private static (int,int) Pair(int a,int b)=>(Math.Min(a,b),Math.Max(a,b));
        private bool Routed(int i)=>wires.Contains(Pair(Wires[i].a,Wires[i].b));
        private bool OpticalOnlyMissing()
        {
            if(wiredOptical||wires.Count!=Wires.Length-1)return false;
            for(int i=0;i<Feedback;i++) if(!Routed(i))return false;
            return true;
        }
        private void ReadTopology(SparkCircuitSystem circuit)
        {
            edges.Clear();wires.Clear();circuit.CopyConnections(edges);
            foreach(var edge in edges)
            {
                if(edge.Kind!=SparkConnectionKind.Wire) continue;
                int a=Array.IndexOf(pins,edge.A),b=Array.IndexOf(pins,edge.B);
                if(a>=0||b>=0) wires.Add(Pair(a,b));
            }
            wiredOptical=Routed(Feedback);
            wiring=wires.Count==Wires.Length?null:"Remove unexpected or incomplete wiring.";
            for(int i=0;i<Wires.Length;i++) if(!Routed(i))
            {
                wiring=$"Connect W{i+1:00} · {pins[Wires[i].a].name} → {pins[Wires[i].b].name}.";
                break;
            }
        }
        private void Update()
        {
            if(loader.ActiveLayout!=layout||loader.ActiveBindings==null) return;
            var circuit=manager.CircuitSystem;var solver=manager.ElectricalSolver;
            if(!circuit||!solver) return;
            if(bindings!=loader.ActiveBindings){bindings=loader.ActiveBindings;topology=-1;}
            if(topology!=circuit.TopologyVersion){topology=circuit.TopologyVersion;ReadTopology(circuit);}
            var result=solver.LatestSolveResult;
            bool Rail(int a,int b)=>result!=null&&result.TryGetVoltage(pins[a],pins[b],out float v)&&v is >=4.7f and <=5.3f;
            rails=power&&source.IsOutputActive&&!source.IsOverloaded;
            for(int i=0;i<10&&rails;i+=2) rails=Rail(Wires[i].b,Wires[i+1].b);
            safety=emergency.IsClosed&&Rail(StopInput,SafeReturn);
            opticalPower=power&&Rail(OpticalPower,OpticalReturn);
            rfPath=power&&Rail(Wires[RfWire].b,SafeReturn);
            if(stage==0&&Ready) Award();
            if(latch==Fault.None&&(rf||armed||etching||loadClock>0)&&!Ready)
            {
                bool optical=rails&&safety&&OpticalOnlyMissing();
                opticalObserved|=stage==5&&armed&&optical;
                Trip(optical?Fault.Optical:!rails?Fault.Power:!safety?Fault.EStop:Fault.Wiring);
            }
            accumulator+=Mathf.Min(Time.deltaTime,.20f)*2.5f;
            const float dt=SparkLevel71EtchPlant.Step;
            while(accumulator>=dt){accumulator-=dt;Tick(dt);}
            Present();
        }
        private void Tick(float dt)
        {
            elapsed+=dt;
            bool routed=opticalPower&&wiredOptical&&(latch!=Fault.Optical||recalibrated)&&calOptical;
            age=routed?age+dt:0;
            validOptical=age>.68f;
            pressure=Mathf.Lerp(pressure,plant.Pressure,dt/(.13f+dt));
            if(loadClock>0)
            {
                loadClock+=dt;
                if(loadClock>2.65f)
                {
                    loadClock=0;loaded=true;clamped=false;endpointConfirmed=measured=false;
                    plant.NewFilm();Log("New wafer seated. Clamp the chuck.");
                }
            }
            metrologyTime=Mathf.Max(0,metrologyTime-dt);
            if(rf&&latch==Fault.None&&plant.Temperature>56.5f)
            {
                coolingPassed|=scenario==Scenario.Cooling&&plant.Chiller<.18f&&plant.Temperature-thermalStart>6;
                Trip(Fault.Temperature);
            }
            arcDwell=rf&&plant.Forward>.2f&&plant.Reflection>.64f?arcDwell+dt:0;
            if(latch==Fault.None&&arcDwell>1.25f) Trip(Fault.Arc);
            if(rf&&latch==Fault.None&&plant.Pressure>17.2f) Trip(Fault.Vacuum);
            bool contact=ProcessBase&&rf&&(!etching||validOptical);
            if(contact!=previousContact)
            {
                previousContact=contact;contacts[0].SetState(contact?SparkSwitchState.Closed:SparkSwitchState.Open);
            }
            float reqThrottle=manualThrottle;
            if(gas&&autoPressure)
            {
                float bias=(plant.Inflow/7.5f/2.09f-.10f)/.90f;
                reqThrottle=pid.Drive(pressure-7.5f,bias,.065f,.085f,.027f,1,dt);
            }
            bool pumpPermit=rails&&safety&&(Ready||latch==Fault.Optical);
            plant.Tick(dt,pumpPermit&&backing,pumpPermit&&turbo,pumpPermit&&isolation,reqThrottle,
                gas&&Ready?gasA:0,gas&&Ready?gasB:0,contact&&rfPath&&contacts[0].IsClosed,commandPower,
                matching,capA,capB,ignite,etching&&validOptical,chiller&&rails&&scenario!=Scenario.Cooling,
                scenario==Scenario.Leak&&testClock<5.5f?4.3f:0,scenario==Scenario.Drift&&testClock<5.5f?1:0,elapsed,loadClock is >.1f and <2.37f);
            UpdateEndpoint(dt);
            ObserveTest(dt);
            bool goal=Ready && stage switch
            {
                1=>Ready&&calPressure&&Pumping&&loaded&&clamped&&plant.Pressure<2.5f,
                2=>PlasmaReady&&autoPressure,
                3=>measured&&acceptedBatches>=1,
                4=>leakPassed&&driftPassed&&coolingPassed&&coolingReset&&measured&&acceptedBatches>=2&&latch==Fault.None,
                5=>CanCertify&&certify&&plant.BalanceError<.0001f,
                _=>false
            };
            hold=goal?hold+dt:0;
            if(hold>(stage==2?2f:.8f)) Award();
            if(plot!=null) plot.Sample(dt,plant.Pressure,plant.Temperature,plant.Reflected,validOptical?spectral:0);
        }
        private void UpdateEndpoint(float dt)
        {
            if(!validOptical||plant.Reference<=.16f)
            {
                spectral=0;endpointDwell=0;return;
            }
            float measuredFeature=plant.Signal/plant.Reference;
            spectral=spectral<=.001f?measuredFeature:Mathf.Lerp(spectral,measuredFeature,dt/(.12f+dt));
            if(!etching||!endpointEnabled) return;
            endpointDwell=spectral<.225f?endpointDwell+dt:0;
            if(endpointDwell<.17f) return;
            endpointConfirmed=true;StopRf();
            Log("OPTICAL ENDPOINT · sustained low film feature stopped RF.");
        }
        private void ObserveTest(float dt)
        {
            if(scenario==Scenario.None) return;
            testClock+=dt;peakP=Mathf.Max(peakP,plant.Pressure);peakReflection=Mathf.Max(peakReflection,plant.Reflection);
            if(scenario==Scenario.Cooling||testClock<8||!PlasmaReady&&testClock<22) return;
            bool pass=PlasmaReady&&(scenario==Scenario.Leak?peakP-baselineP>.16f:peakReflection-baselineReflection>.035f);
            if(scenario==Scenario.Leak) leakPassed|=pass;
            else driftPassed|=pass;
            Log(pass?(scenario==Scenario.Leak?"VACUUM LEAK VERIFIED · pressure deviation and recovery measured.":"RF DRIFT VERIFIED · reflected power rose and recovered."):"Wait for stable plasma, then retry the test.");
            scenario=Scenario.None;
        }
        private void Begin(Scenario value)
        {
            scenario=value;testClock=0;baselineP=peakP=plant.Pressure;baselineReflection=peakReflection=plant.Reflection;
            thermalStart=plant.Temperature;
            Log(value==Scenario.Leak?"Vacuum leak applied.":value==Scenario.Drift?"RF load drift applied.":"Chiller capacity interrupted.");
        }
        private void StopRf()
        {
            rf=ignite=etching=false;previousContact=false;contacts[0].SetState(SparkSwitchState.Open);
        }
        private void Trip(Fault cause)
        {
            latch=cause;StopRf();gas=false;scenario=Scenario.None;hold=0;
            if(loadClock>0)
            {
                loadClock=0;
                loaded=clamped=false;
            }
            if(cause==Fault.Optical)
            {
                validOptical=false;age=0;recalibrated=false;spectral=0;endpointDwell=0;
                Log("OPTICAL FEEDBACK INVALID · etch stopped and RF isolated.");
            }
            else Log($"REACTOR PROTECTION · {cause} · RF isolated.");
        }
        private void Log(string message)
        {
            latestEvent=$"{elapsed:0.0}s · {message}";events.Enqueue(latestEvent);
            if(events.Count>8)events.Dequeue();
        }
        private void Award()
        {
            if(stage>=Gates.Length||!bindings.SetRuntimeComponentBinding(Gates[stage],source))return;
            stage++;hold=0;manager.QueueEvaluation();Log($"OBJECTIVE {stage}/6 verified.");
        }

        public void Power(){power=!power;source.SetOutput(power,default);}
        public void CalPressure(){if(Ready){calPressure=true;Log("Pressure instruments calibrated.");}}
        public void CalGas(){if(Ready){calGas=true;Log("Virtual mass flow channels calibrated.");}}
        public void CalRf(){if(Ready&&!rf){calRf=true;Log("Forward and reflected RF channels calibrated.");}}
        public void CalOptical(){if(Ready&&!rf&&latch==Fault.None){calOptical=true;age=0;Log("Dark reference and optical gain calibrated.");}}
        public void Backing(){if(Ready&&calPressure)backing=true;}
        public void Turbo(){if(CanTurbo)turbo=true;}
        public void PressurePid(){if(Ready&&stage>=2){autoPressure=!autoPressure;pid.Reset();}}
        public void Isolation(){if(Ready&&!rf&&!etching)isolation=!isolation;}
        public void Throttle(){if(Ready&&!autoPressure&&!rf)manualThrottle=manualThrottle>.93f?.48f:manualThrottle+.19f;}
        public void Load(){if(CanLoad){loadClock=.001f;loaded=clamped=false;Log("Load-lock gate and robot transferring wafer.");}}
        public void Clamp(){if(Ready&&loaded&&loadClock<=0&&!rf)clamped=true;}
        public void Chiller(){if(Ready&&!rf)chiller=!chiller;}
        public void SetA(){if(!rf)gasA=gasA>=.79f?.56f:gasA+.12f;}
        public void SetB(){if(!rf)gasB=gasB>=.51f?.32f:gasB+.10f;}
        public void Gas(){if(CanGas){gas=!gas;pid.Reset();}}
        public void Rf(){if(ProcessBase&&!etching)rf=!rf;}
        public void SetPower(){if(!rf)commandPower=commandPower>=.87f?.64f:commandPower+.12f;}
        public void TuneA(){if(!matching)capA=capA>=.83f?.24f:capA+.10f;}
        public void TuneB(){if(!matching)capB=capB>=.83f?.31f:capB+.10f;}
        public void Match(){if(Ready&&!etching)matching=!matching;}
        public void Ignite(){if(CanIgnite)ignite=true;}
        public void Etch(){if(CanProcess){etching=true;endpointDwell=0;Log("Film removal started under optical supervision.");}}
        public void Endpoint(){if(Ready&&validOptical&&!etching)endpointEnabled=!endpointEnabled;}
        public void Stop(){StopRf();gas=false;Log("Process stopped. Pumping chamber down.");}
        public void Metrology()
        {
            if(!CanMeasure)return;
            metrologyTime=1.6f;
            if(plant.WorstFilm<=8&&plant.NonUniformity<=.05f&&plant.Damage<1.6f&&plant.MaskLoss<27f)
            {
                measured=true;acceptedBatches++;gas=false;
                Log($"WAFER ACCEPTED · film {plant.MeanFilm:0.0} nm · nonuniformity {plant.NonUniformity:P1}.");
            }
            else{measured=false;gas=false;Log("Wafer out of limits. Pump down and load a fresh wafer.");}
        }
        public void Leak(){if(CanLeak) Begin(Scenario.Leak);}
        public void Drift(){if(CanDrift) Begin(Scenario.Drift);}
        public void Cooling(){if(CanCooling) Begin(Scenario.Cooling);}
        public void EStop(){emergency.SetState(emergency.IsClosed?SparkSwitchState.Open:SparkSwitchState.Closed);if(emergency.IsOpen&&latch==Fault.None) Trip(Fault.EStop);}
        public void Reset(){if(CanReset){coolingReset|=latch==Fault.Temperature&&coolingPassed;opticalReset|=latch==Fault.Optical&&opticalObserved;armed=false;latch=Fault.None;Log("Protection reset. Recommission vacuum, wafer and RF.");}}
        public void Arm(){if(CanArm){armed=true;Log("Remove W90 with Undo Last Wire while the endpoint loop is active.");}}
        public void Recal(){if(CanRecal){recalibrated=true;age=0;Log("Spectrometer recalibrated. Await fresh optical samples.");}}
        public void Certify(){if(CanCertify)certify=true;}

        private bool CanTurbo=>Ready&&calPressure&&backing&&!turbo&&plant.Backing>.65f&&plant.Pressure<155;
        private bool CanLoad=>WaferTransferReady&&(!loaded||stage>=4||endpointConfirmed);
        private bool CanGas=>Ready&&stage>=2&&Pumping&&calGas&&loaded&&clamped&&!rf&&latch==Fault.None;
        private bool CanIgnite=>ProcessBase&&rf&&!ignite&&plant.Delivered>.40f&&plant.Reflection<.12f;
        private bool CanMeasure=>Ready&&endpointConfirmed&&!measured&&!rf&&plant.Activity<.045f&&latch==Fault.None;
        private bool CanLeak=>stage==4&&PlasmaReady&&!etching&&scenario==Scenario.None&&!leakPassed;
        private bool CanDrift=>stage==4&&PlasmaReady&&!etching&&scenario==Scenario.None&&leakPassed&&!driftPassed;
        private bool CanCooling=>stage==4&&PlasmaReady&&!etching&&scenario==Scenario.None&&driftPassed&&!coolingPassed;
        private string Status()
        {
            if(stage==6)return "CERTIFIED · fresh optical endpoint and wafer metrology verified.";
            if(!power)return "Enable 5 V CONTROL and complete the reactor wiring.";
            if(latch==Fault.Optical)return !Ready?"OPTICAL FEEDBACK INVALID · reconnect W90.":CanRecal?"Recalibrate the spectrometer.":"Wait for valid optical samples and safely RESET.";
            if(latch!=Fault.None)return $"PROTECTION LATCH · {latch} · wait for safe reset.";
            if(!Ready)return wiring??"Check 5 V control rails and E-Stop loop.";
            if(!calPressure||!isolation||!backing||!turbo)return "Calibrate pressure, open isolation, start backing, then turbo.";
            if(loadClock>0)return "LOAD-LOCK · gate and wafer robot transferring.";
            if(armed)return "Disconnect W90 · spectrometer output → endpoint controller input.";
            if(scenario!=Scenario.None)return $"TEST · {scenario} · monitor the physical response.";
            if(etching)return "ETCH ACTIVE · optical film feature monitored.";
            if(endpointConfirmed&&!measured)return "OPTICAL ENDPOINT confirmed. Wait for plasma to decay, then run METROLOGY.";
            if(stage==1||!loaded||!clamped)return "Pump down, LOAD WAFER and CLAMP.";
            if(stage==5&&CanCertify)return "Fresh wafer accepted. Select CERTIFY.";
            if(stage==4&&measured&&!coolingReset)return !leakPassed?"Reignite stabilized plasma, then VAC LEAK.":!driftPassed?"Apply RF DRIFT.":"Apply COOLING TEST.";
            if(stage>=4&&measured||stage==5&&opticalReset&&!etching&&plant.MeanFilm<270)return "STOP to pump down, then load a fresh wafer.";
            if(!PlasmaReady)return "Calibrate gas/RF, enable pressure PID and cooling, match RF, then IGNITE.";
            return stage==5&&!opticalObserved?"Enable endpoint and start an etch. Arm W90 during the run.":"Enable ENDPOINT AUTO and select ETCH.";
        }
        private void Present()
        {
            statusText.text=Status();
            meterText.text=$"CHAMBER {pressure:0.00} Pa  ·  LOAD LOCK {plant.LockPressure:0.00} Pa  ·  VIRTUAL FLOW {plant.FlowA:0.00}/{plant.FlowB:0.00}  ·  WAFER {plant.Temperature:0.0}°C";
            detailText.text=$"RF FWD {plant.Forward:0.00}  ·  REF {plant.Reflected:0.00}  ·  DEL {plant.Delivered:0.00} kW  |  FILM {plant.MeanFilm:0.0} nm  ·  OES {(validOptical?spectral.ToString("0.00"):"INVALID")}";
            calibrationText.text=$"{latestEvent}  |  THROTTLE {plant.Throttle:P0}  ·  MATCH {(matching?"AUTO":"MANUAL")}  ·  ACCEPTED {acceptedBatches}";
            if(controllerDisplay)controllerDisplay.text=$"{plant.Pressure:0.00} Pa · {plant.Temperature:0.0}°C";
            if(qualityDisplay)qualityDisplay.text=validOptical?$"{plant.MeanFilm:0.0} nm · OES {spectral:0.00}":"OPTICAL INVALID";
            powerButton.interactable=stage<6;
            calPressureButton.interactable=Ready&&!calPressure;
            calGasButton.interactable=Ready&&!calGas;
            calRfButton.interactable=Ready&&!calRf&&!rf;
            calOpticalButton.interactable=Ready&&!calOptical&&!rf;
            backingButton.interactable=Ready&&calPressure&&!backing;
            turboButton.interactable=CanTurbo;
            pressurePidButton.interactable=Ready&&stage>=2&&!etching;
            isolationButton.interactable=Ready&&!rf&&!etching;
            throttleButton.interactable=Ready&&!autoPressure&&!rf;
            loadButton.interactable=CanLoad;
            clampButton.interactable=Ready&&loaded&&!clamped&&loadClock<=0&&!rf;
            chillerButton.interactable=Ready&&!rf;
            setAButton.interactable=setBButton.interactable=setPowerButton.interactable=Ready&&!rf;
            gasButton.interactable=CanGas;
            rfButton.interactable=ProcessBase&&!rf;
            tuneAButton.interactable=tuneBButton.interactable=Ready&&!matching&&!etching;
            matchButton.interactable=Ready&&!etching;
            igniteButton.interactable=CanIgnite;
            etchButton.interactable=CanProcess&&stage<6;
            endpointButton.interactable=Ready&&validOptical&&!etching;
            stopButton.interactable=rf||gas||etching;
            metrologyButton.interactable=CanMeasure;
            leakButton.interactable=CanLeak;driftButton.interactable=CanDrift;coolingButton.interactable=CanCooling;
            eStopButton.interactable=stage<6;
            resetButton.interactable=CanReset;
            armButton.interactable=CanArm;
            recalButton.interactable=CanRecal;
            certifyButton.interactable=CanCertify&&!certify;
            rig.Apply(plant,elapsed,loadClock,loaded,clamped,metrologyTime);
        }
    }
}
