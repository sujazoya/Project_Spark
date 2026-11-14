using System;
using System.Collections.Generic;
using ProjectSpark.Circuit;
using ProjectSpark.Electrical;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.Gameplay
{
    [DisallowMultipleComponent]
    public sealed class SparkLevel60Resilience : MonoBehaviour
    {
        public static readonly string[] Gates={"L60_Wired","L60_Dispatch","L60_Solar","L60_Island","L60_Emergency","L60_Certified"};
        public static readonly (int a,int b)[] Wires={(0,1),(2,3),(0,4),(2,5),(0,6),(2,7),(0,8),(2,9),(10,11),(12,13),(14,15),(16,17),(17,18),(13,19),(20,21),(22,23),(24,25),(26,27),(28,29),(30,31),(32,17),(33,13),(34,35),(17,36),(13,37),(38,39),(40,41),(42,43),(44,45),(46,47),(48,49),(50,51),(52,53),(23,54),(25,55),(23,56),(25,57),(27,58),(27,59),(23,60),(61,62),(61,63),(64,65),(66,67),(68,69),(70,71)};
        public SparkLevelLayout layout;
        public SparkPowerSupply source;
        public SparkSwitch[] contacts;
        public SparkSwitch emergency;
        public SparkTerminal[] pins;
        public SparkLevel60VisualRig rig;
        public TMP_Text controllerDisplay,qualityDisplay;
        public Text statusText,meterText,detailText,calibrationText;
        public Button powerButton,guardButton,calibrateButton,reserveButton,thresholdButton,policyButton,autoButton,cloudButton,outageButton,
            surgeButton,failureButton,repairButton,resetButton,gridButton,motorButton,auxButton,finalButton;

        private const float CapacityWh=.18f, MinimumSoc=.19f, BatteryLimit=2.45f, GridLimit=5.4f, GenLimit=3.1f, Efficiency=.91f, SolarRated=1.62f;
        private static readonly float[] Loads={.49f,.62f,1.14f,.52f};
        private readonly bool[] feed=new bool[4];
        private readonly float[] age=new float[4];
        private readonly List<SparkCircuitConnection> edges=new();
        private readonly HashSet<(int,int)> wired=new();
        private SparkLevelLayoutLoader loader;
        private LevelGamePlayManager manager;
        private SparkLevelSceneBindings bindings;
        private SparkElectricalSolver solver;
        private SparkCircuitSystem circuit;
        private int topology=-1,stage;
        private string wiring,protection,eventText="Waiting for wiring";
        private bool power=true,guard=true,ready,calibrated,reserveSet,thresholdSet,policySet,automatic,cloud,outage,surge,
            gridAvailable=true,gridConnected,genConnected,genStart,genFault,repaired,resetDone,gridRequested,certified;
        private bool essentialInterrupted,surgeSeen,generatorContributed,emergencyProven,generatorRecovered;
        private float accumulator,time,hold,soc=.76f,solar=1,mppt=.84f,pvWatts,batteryWatts,gridWatts,genWatts,loadWatts,
            busVolts,busHz,rpm,genVoltage,genPhase,gridPhase,busPhase,gridVoltage,gridSyncHold,genSyncHold,motorSpeed,unserved,residual;
        private float ReserveSoc=>reserveSet?.35f:.18f;
        private float StartMargin=>thresholdSet?.66f:.20f;
        private float BatteryLimitNow=>soc>MinimumSoc?BatteryLimit*Mathf.Clamp01((soc-MinimumSoc)/.06f):0;
        private float BackupHz=>rpm/30f;
        private float GenAngle=>Mathf.DeltaAngle(busPhase,genPhase);
        private float GridAngle=>Mathf.DeltaAngle(busPhase,gridPhase);
        private bool Operational=>ready&&automatic&&protection==null;
        private bool Stable=>busVolts is >=2.23f and <=2.49f&&Mathf.Abs(busHz-50)<.28f&&unserved<.015f;
        private bool Essential=>feed[0]&&busVolts>2.15f&&unserved<.025f;
        private bool Full=>Array.TrueForAll(feed,on=>on)&&motorSpeed>.92f;
        private float RuntimeMinutes=>batteryWatts>.01f?Mathf.Max(0,(soc-ReserveSoc)*CapacityWh)*60/batteryWatts:float.PositiveInfinity;

        private void Start()
        {
            loader=FindFirstObjectByType<SparkLevelLayoutLoader>();
            manager=FindFirstObjectByType<LevelGamePlayManager>();
            if(loader&&manager&&pins?.Length==72&&contacts?.Length==17)return;
            Debug.LogError("[LEVEL 60] Missing runtime services, terminals or breaker poles.",this);
            enabled=false;
        }
        private static (int,int) Pair(int a,int b)=>(Math.Min(a,b),Math.Max(a,b));
        private string Inspect()
        {
            wired.Clear();
            circuit.CopyConnections(edges);
            foreach(var edge in edges)
            {
                if(edge.Kind!=SparkConnectionKind.Wire)continue;
                int a=Array.IndexOf(pins,edge.A),b=Array.IndexOf(pins,edge.B);
                if(a>=0||b>=0)wired.Add(Pair(a,b));
            }
            for(int i=0;i<Wires.Length;i++)
            {
                var (a,b)=Wires[i];
                if(!wired.Contains(Pair(a,b)))return $"Connect W{i+1:00} · {pins[a].name} → {pins[b].name}.";
            }
            return wired.Count==Wires.Length?null:"Remove extra wires from the level sockets.";
        }
        private void Update()
        {
            if(loader.ActiveLayout!=layout||loader.ActiveBindings==null)return;
            solver=manager.ElectricalSolver;
            circuit=manager.CircuitSystem;
            if(!solver||!circuit)return;
            if(bindings!=loader.ActiveBindings){bindings=loader.ActiveBindings;topology=-1;}
            if(topology!=circuit.TopologyVersion){topology=circuit.TopologyVersion;wiring=Inspect();}
            var result=solver.LatestSolveResult;
            bool Rail(int a,int b)=>result!=null&&result.TryGetVoltage(pins[a],pins[b],out var v)&&v is >=4.7f and <=5.3f;
            ready=power&&guard&&wiring==null&&source.IsOutputActive&&!source.IsOverloaded&&
                Rail(1,3)&&Rail(4,5)&&Rail(6,7)&&Rail(8,9)&&Rail(71,3);
            if(stage==0&&ready)Award();
            if(automatic&&!ready)Trip(!guard?"GUARD OPEN":wiring??"CONTROL POWER LOST");
            accumulator+=Mathf.Min(Time.deltaTime,.12f);
            while(accumulator>=.01f){accumulator-=.01f;Step(.01f);}
            Present();
        }

        private void Sources(float dt)
        {
            solar=Mathf.MoveTowards(solar,cloud?.12f:1f,dt*.66f);
            mppt=Mathf.MoveTowards(mppt,Operational?.95f:.84f,dt*.20f);
            pvWatts=Operational?SolarRated*solar*mppt:0;
            float deficitForecast=LoadDemand(false)-pvWatts;
            if(Operational&&outage&&!gridConnected&&!genFault&&
                (resetDone||deficitForecast>BatteryLimitNow-StartMargin||soc<ReserveSoc+.10f))genStart=true;
            float genTarget=Operational&&genStart&&!genFault?(genConnected?1500f:1500+Mathf.Clamp(-GenAngle*1.25f,-27f,27f)):0;
            rpm+=(genTarget-rpm)*(1-Mathf.Exp(-dt/(genTarget>0?.72f:.47f)));
            genVoltage=Mathf.MoveTowards(genVoltage,rpm>800&&genStart?busVolts:0,dt*1.8f);
            gridVoltage=Mathf.MoveTowards(gridVoltage,gridAvailable?2.4f:0,dt*3.4f);
            float gridHz=gridConnected?50f:50f+Mathf.Clamp(-GridAngle*.040f,-.95f,.95f);
            gridPhase=gridConnected?busPhase:Mathf.Repeat(gridPhase+gridHz*360*dt,360);
            genPhase=genConnected?busPhase:Mathf.Repeat(genPhase+BackupHz*360*dt,360);
            float referenceAdvance=busHz*360*dt;
            bool GenWindow()=>Operational&&genStart&&!genFault&&!genConnected&&
                Window(BackupHz,genVoltage,Mathf.DeltaAngle(busPhase+referenceAdvance,genPhase));
            genSyncHold=GenWindow()?genSyncHold+dt:0;
            gridSyncHold=Operational&&gridRequested&&gridAvailable&&!gridConnected&&Window(gridHz,gridVoltage,
                Mathf.DeltaAngle(busPhase+referenceAdvance,gridPhase))?gridSyncHold+dt:0;
            if(genSyncHold>=.65f)
            {
                SetGenerator(true);
                Log("QGEN synchronized and closed");
            }
            if(gridSyncHold>=.65f)
            {
                SetGrid(true);
                Log("QGRID synchronized and closed");
            }
            Contact(12,Operational&&BatteryLimitNow>0);
            Contact(13,Operational&&pvWatts>.02f);
            for(int i=14;i<17;i++)Contact(i,Operational&&BatteryLimitNow>0);
        }
        private bool Window(float hz,float volts,float angle)=>Stable&&Mathf.Abs(hz-busHz)<.20f&&
            Mathf.Abs(volts-busVolts)<=.05f*busVolts&&Mathf.Abs(angle)<5;
        private float LoadDemand(bool transient)
        {
            float result=0;
            for(int i=0;i<4;i++)
                if(feed[i])result+=Loads[i]*(1+(transient?(.38f+(i==2?.44f:0))*Mathf.Exp(-age[i]/.52f):0));
            if(surge&&feed[2])result+=1.15f;
            return result;
        }
        private void Dispatch(float dt)
        {
            for(int i=0;i<age.Length;i++)age[i]+=dt;
            float budget=(gridConnected?GridLimit:0)+(genConnected?GenLimit:0)+pvWatts+BatteryLimitNow;
            loadWatts=Operational?LoadDemand(true):0;
            if(Operational&&policySet&&loadWatts>budget-.04f)
                for(int i=3;i>=1&&loadWatts>budget-.04f;i--)
                    if(feed[i])
                    {
                        Feeder(i,false);
                        loadWatts=LoadDemand(true);
                        Log($"Q{i+1} shed on forecast capacity shortfall");
                    }
            float solarUsed=Mathf.Min(loadWatts,pvWatts);
            float remaining=loadWatts-solarUsed;
            genWatts=genConnected?Mathf.Min(GenLimit,remaining*(gridConnected?.42f:.70f)):0;
            remaining-=genWatts;
            gridWatts=gridConnected?Mathf.Min(GridLimit,remaining):0;
            remaining-=gridWatts;
            float discharge=Operational?Mathf.Min(BatteryLimitNow,remaining):0;
            unserved=Mathf.Max(0,remaining-discharge);
            float chargeInput=Operational&&soc<.92f?Mathf.Min(.52f,pvWatts-solarUsed+Mathf.Min(.32f,gridConnected?GridLimit-gridWatts:0)):0;
            float gridCharge=Mathf.Max(0,chargeInput-(pvWatts-solarUsed));
            gridWatts+=gridCharge;
            float starterWatts=genStart&&rpm<1000&&soc>MinimumSoc?.28f:0;
            batteryWatts=discharge/Efficiency-chargeInput*Efficiency+starterWatts;
            soc=Mathf.Clamp01(soc-batteryWatts*dt/(3600*CapacityWh));
            residual=solarUsed+genWatts+gridWatts-gridCharge+discharge+unserved-loadWatts;
            bool referred=Operational&&(gridConnected||BatteryLimitNow>.10f);
            busVolts=Mathf.MoveTowards(busVolts,referred?2.4f-.025f*loadWatts-.28f*unserved:0,4.4f*dt);
            busHz=Mathf.MoveTowards(busHz,referred?50-.032f*loadWatts-1.25f*unserved:0,68*dt);
            busPhase=Mathf.Repeat(busPhase+busHz*360*dt,360);
            motorSpeed=Mathf.MoveTowards(motorSpeed,feed[2]&&busVolts>2.1f?1:0,dt);
        }
        private void Step(float dt)
        {
            time+=dt;
            Sources(dt);
            Dispatch(dt);
            if(stage is >=3 and <=5 && outage && !Essential)essentialInterrupted=true;
            surgeSeen|=stage==4&&surge&&feed[2]&&loadWatts>3.1f;
            generatorContributed|=surgeSeen&&genConnected&&genWatts>.60f;
            generatorRecovered|=resetDone&&genConnected&&genWatts>.45f;
            bool interval=stage switch
            {
                1=>Operational&&Stable&&Full&&gridConnected&&pvWatts>1.3f,
                2=>cloud&&Stable&&pvWatts<.22f&&gridWatts>1&&Essential,
                3=>outage&&!gridConnected&&Stable&&Essential&&!essentialInterrupted&&batteryWatts>0,
                4=>genFault&&generatorContributed&&!genConnected&&!feed[2]&&!feed[3]&&Stable&&Essential&&!essentialInterrupted,
                5=>certified&&emergencyProven&&generatorRecovered&&gridConnected&&genConnected&&Full&&Stable&&!essentialInterrupted&&soc>MinimumSoc,
                _=>false
            };
            hold=interval?hold+dt:0;
            if(hold>=1.4f)
            {
                if(stage==4)emergencyProven=true;
                Award();
            }
        }

        private void Log(string value)
        {
            eventText=$"{time:F1}s  {value}";
            Debug.Log("[LEVEL 60] "+eventText,this);
        }
        private void Award()
        {
            if(stage>=Gates.Length||!bindings.SetRuntimeComponentBinding(Gates[stage],source))return;
            stage++;
            hold=0;
            manager.QueueEvaluation();
        }
        private void Contact(int index,bool on)
        {
            var c=contacts[index];
            if(c.IsClosed!=on)c.SetState(on?SparkSwitchState.Closed:SparkSwitchState.Open);
        }
        private void SetGrid(bool on){gridConnected=on;for(int i=3;i<6;i++)Contact(i,on);}
        private void SetGenerator(bool on){genConnected=on;for(int i=0;i<3;i++)Contact(i,on);}
        private void Feeder(int i,bool on)
        {
            if(feed[i]==on)return;
            feed[i]=on;
            age[i]=0;
            Contact(i+6,on);
            if(i==2){Contact(10,on);Contact(11,on);}
        }
        private void Trip(string reason)
        {
            automatic=calibrated=genStart=false;
            SetGrid(false);
            SetGenerator(false);
            for(int i=0;i<4;i++)Feeder(i,false);
            if(outage&&stage<6)essentialInterrupted=true;
            protection=reason;
            Log(reason);
        }

        public void Power(){power=!power;source.SetOutput(power,default);}
        public void Guard(){guard=!guard;emergency.SetState(guard?SparkSwitchState.Closed:SparkSwitchState.Open);}
        public void Calibrate(){if(ready&&!automatic&&protection==null){calibrated=true;Log("CT, voltage, PV and source reference channels calibrated");}}
        public void Reserve(){if(!automatic)reserveSet=!reserveSet;}
        public void Threshold(){if(!automatic)thresholdSet=!thresholdSet;}
        public void Policy(){if(!automatic)policySet=!policySet;}
        private bool CanAuto=>ready&&protection==null&&calibrated&&reserveSet&&thresholdSet&&policySet&&!automatic;
        public void Auto()
        {
            if(!CanAuto)return;
            automatic=true;
            if(gridAvailable&&!outage)SetGrid(true);
            Feeder(0,true);
            Feeder(1,true);
            if(!outage){Feeder(2,true);Feeder(3,true);}
            Log("Predictive dispatch enabled");
        }
        public void Cloud(){if(Operational&&stage==2&&!cloud){cloud=true;Log("Cloud front reduced array irradiance");}}
        public void Outage()
        {
            if(!Operational||stage!=3||outage)return;
            outage=true;
            gridAvailable=false;
            SetGrid(false);
            Log("Utility lost · QGRID opened · inverter forms the island");
        }
        public void Surge(){if(Operational&&stage==4&&!surge&&feed[2]&&genConnected){surge=true;Log("Industrial surge forecast and load applied");}}
        public void Failure()
        {
            if(stage!=4||!Operational||!genConnected||!generatorContributed||genFault)return;
            genFault=true;
            genStart=false;
            SetGenerator(false);
            Log("Generator failed · QGEN opened · emergency dispatch");
        }
        public void Repair(){if(genFault&&emergencyProven&&rpm<90){repaired=true;surge=false;Log("Generator repaired and surge cleared");}}
        public void Reset()
        {
            if(!ready)return;
            if(genFault&&repaired){genFault=false;resetDone=true;Log("Generator fault reset · restart authorized");}
            if(protection!=null){protection=null;Log("Protection reset · recalibrate and enable AUTO");}
        }
        public void Grid()
        {
            if(!Operational||stage!=5||!generatorRecovered||gridAvailable)return;
            gridAvailable=gridRequested=true;
            cloud=false;
            Log("Utility available · acquiring synchronization window");
        }
        private bool CanMotor=>stage==5&&Operational&&gridConnected&&generatorRecovered&&Stable&&!feed[2];
        private bool CanAux=>stage==5&&Operational&&gridConnected&&feed[2]&&motorSpeed>.92f&&Stable&&!feed[3];
        public void Motor(){if(CanMotor)Feeder(2,true);}
        public void Aux(){if(CanAux)Feeder(3,true);}
        public void Final(){if(stage==5&&gridConnected&&genConnected&&Full&&Stable&&!essentialInterrupted)certified=true;}

        private string Prompt()
        {
            if(stage==6)return "CERTIFIED · Utility and generator synchronized, all four loads restored.";
            if(wiring!=null)return wiring;
            if(!ready)return "Restore the control supply and close the guard.";
            if(protection!=null)return protection+" · reset the protection latch.";
            if(!calibrated)return "Calibrate the voltage, current and source channels.";
            if(!automatic)return "Set reserve, start margin and shedding priority, then enable AUTO.";
            if(essentialInterrupted)return "Essential service was interrupted. Restart Level 60 for a fresh run.";
            return stage switch
            {
                1=>"Recording the healthy grid-connected baseline.",
                2=>cloud?"Observe the dispatch response as PV falls.":"Select CLOUD COVER.",
                3=>outage?"QGRID OPEN · verifying essential service on the island.":"Select UTILITY OUTAGE.",
                4=>genFault?"Emergency dispatch · proving essential continuity.":!surge?(genConnected?"Select MOTOR SURGE.":"Wait for the backup generator before applying the surge."):!generatorContributed?"Await backup generator synchronization and contribution.":"Select GEN FAILURE.",
                5=>genFault?(repaired?"Reset the latched generator fault.":"Wait for the rotor to stop, then repair."):
                    !generatorRecovered?"Restarting and synchronizing the generator.":!gridAvailable?"Select RESTORE UTILITY.":
                    !gridConnected?"Acquiring the utility sync window.":!Full?"Restore the motor, then auxiliary.":"Select CERTIFY after full restoration.",
                _=>"Wire the resilience network."
            };
        }
        private void Present()
        {
            statusText.text=Prompt();
            meterText.text=$"BUS {busVolts:F2} V  {busHz:F2} Hz  ·  SOC {soc:P0}  ·  PV {pvWatts:F2} W  ·  GEN {rpm:F0} RPM";
            detailText.text=$"GRID {gridWatts:F2} W  ·  GEN {genWatts:F2} W  ·  BAT {batteryWatts:+0.00;-0.00;0.00} W  ·  LOAD {loadWatts:F2} W  ·  RESERVE ETA {(float.IsInfinity(RuntimeMinutes)?"—":$"{RuntimeMinutes:F1} min")}";
            calibrationText.text=$"{manager.SatisfiedTargetCount}/6  ·  Δφ GEN {GenAngle:F1}° GRID {GridAngle:F1}°  ·  {eventText}";
            if(controllerDisplay)controllerDisplay.text=$"{busVolts:F2} V / {busHz:F2} Hz";
            if(qualityDisplay)qualityDisplay.text=$"SOC {soc:P0} / PV {pvWatts:F2} W";
            if(rig)rig.Apply(gridConnected,genConnected,Operational&&BatteryLimitNow>0,pvWatts>.02f,feed,guard,rpm,genStart&&rpm<1000,solar,motorSpeed,Time.deltaTime);
            powerButton.interactable=guardButton.interactable=true;
            calibrateButton.interactable=ready&&!automatic&&!calibrated&&protection==null;
            reserveButton.interactable=thresholdButton.interactable=policyButton.interactable=ready&&!automatic;
            reserveButton.GetComponentInChildren<Text>().text=$"RESERVE {ReserveSoc:P0}";
            thresholdButton.GetComponentInChildren<Text>().text=$"START MARGIN {StartMargin:F2} W";
            policyButton.GetComponentInChildren<Text>().text=policySet?"AUX → MOTOR → PRI":"SET LOAD PRIORITY";
            autoButton.interactable=CanAuto;
            cloudButton.interactable=stage==2&&Operational&&!cloud;
            outageButton.interactable=stage==3&&Operational&&!outage;
            surgeButton.interactable=stage==4&&Operational&&!surge&&feed[2]&&genConnected;
            failureButton.interactable=stage==4&&Operational&&genConnected&&generatorContributed&&!genFault;
            repairButton.interactable=stage==5&&genFault&&!repaired&&emergencyProven&&rpm<90;
            resetButton.interactable=ready&&(protection!=null||genFault&&repaired);
            gridButton.interactable=stage==5&&Operational&&generatorRecovered&&!gridAvailable;
            motorButton.interactable=CanMotor;
            auxButton.interactable=CanAux;
            finalButton.interactable=stage==5&&gridConnected&&genConnected&&Full&&Stable&&!essentialInterrupted&&!certified;
        }
        private void OnDisable()
        {
            if(bindings!=null)foreach(string gate in Gates)bindings.ClearRuntimeComponentBinding(gate);
        }
    }
}


