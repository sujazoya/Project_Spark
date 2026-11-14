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
    public sealed class SparkLevel59BlackStart : MonoBehaviour
    {
        public static readonly string[] Gates={"L59_Wired","L59_Energized","L59_Essential","L59_Synchronized","L59_Full","L59_Recovered"};
        public static readonly (int a,int b)[] Wires={(0,1),(2,3),(0,4),(2,5),(0,6),(2,7),(0,8),(2,9),(10,11),(12,13),(14,15),(16,17),(17,18),(13,19),(20,21),(22,23),(24,25),(26,27),(28,29),(30,31),(17,32),(13,33),(34,35),(36,37),(38,39),(40,41),(42,43),(44,45),(46,47),(48,49),(23,50),(25,51),(27,52),(23,53),(25,54),(27,55),(23,56),(23,57),(58,59),(60,61),(62,63),(64,65)};
        public SparkLevelLayout layout;
        public SparkPowerSupply source;
        public SparkSwitch[] contacts;
        public SparkSwitch emergency;
        public SparkTerminal[] pins;
        public SparkLevel59VisualRig rig;
        public TMP_Text controllerDisplay,qualityDisplay;
        public Text statusText,meterText,detailText,calibrationText;
        public Button powerButton,guardButton,bmsButton,prechargeButton,inverterButton,lightButton,controlButton,coolingButton;
        public Button startButton,syncButton,closeButton,motorButton,auxButton,failureButton,repairButton,resetButton,finalButton,isolationButton;

        private const float CapacityWh=.12f, MinimumSoc=.24f, PhaseVoltage=2.4f, InverterCapacity=2.45f, GeneratorCapacity=4.5f;
        private static readonly float[] SteadyLoad={.32f,.41f,.61f,1.37f,.53f};
        private readonly bool[] feeders=new bool[5];
        private readonly float[] ages=new float[5];
        private readonly List<SparkCircuitConnection> connections=new();
        private readonly HashSet<(int,int)> routed=new();
        private SparkLevelLayoutLoader loader;
        private LevelGamePlayManager manager;
        private SparkLevelSceneBindings bindings;
        private SparkElectricalSolver solver;
        private SparkCircuitSystem circuit;
        private int topology=-1,stage;
        private string wiring,fault,notice="Build the 42-wire black-start network.";
        private bool power=true,guard=true,ready,bms,precharge,inverter,isolated,starting,sync,parallel,failed,repaired,
            failureProven,startedMotor,certifiedRecovery;
        private float accumulator,soc=.82f,link,rpm,genVolts,busVolts,busHz,genPhase,busPhase,syncHold,busHold,
            essentialHold,genHold,fullHold,failureHold,batteryWatts,generatorWatts,loadWatts,motorSpeed,batteryWhUsed;
        private float BatteryVoltage => 4.6f+soc*.95f;
        private float GeneratorHz => rpm/30f;
        private float PhaseError => Mathf.DeltaAngle(busPhase,genPhase);
        private bool Usable => ready&&guard&&fault==null&&soc>MinimumSoc;
        private bool StableBus => busVolts>=2.24f&&Mathf.Abs(busHz-50f)<.2f;
        private bool Essentials => feeders[0]&&feeders[1]&&feeders[2];
        private bool Full => Essentials&&feeders[3]&&feeders[4]&&motorSpeed>.94f&&parallel&&generatorWatts>.5f&&StableBus;
        private bool Window => starting&&sync&&Essentials&&StableBus&&Mathf.Abs(GeneratorHz-busHz)<=.2f&&
            Mathf.Abs(PhaseError)<=5f&&Mathf.Abs(genVolts-busVolts)<=busVolts*.05f;

        private void Start()
        {
            loader=FindFirstObjectByType<SparkLevelLayoutLoader>();
            manager=FindFirstObjectByType<LevelGamePlayManager>();
            if(loader&&manager&&pins?.Length==66&&contacts?.Length==13)return;
            Debug.LogError("[LEVEL 59] Missing layout services, terminal bindings or contacts.",this);
            enabled=false;
        }
        private static (int,int) Pair(int x,int y)=>(Math.Min(x,y),Math.Max(x,y));
        private string Inspect()
        {
            routed.Clear();
            circuit.CopyConnections(connections);
            foreach(var edge in connections)
            {
                if(edge.Kind!=SparkConnectionKind.Wire)continue;
                int a=Array.IndexOf(pins,edge.A),b=Array.IndexOf(pins,edge.B);
                if(a>=0||b>=0)routed.Add(Pair(a,b));
            }
            for(int i=0;i<Wires.Length;i++)
            {
                var (a,b)=Wires[i];
                if(!routed.Contains(Pair(a,b)))return $"Connect W{i+1:00} · {pins[a].name} → {pins[b].name}.";
            }
            return routed.Count==Wires.Length?null:"Remove extra wires from the black-start sockets.";
        }

        private void Update()
        {
            if(loader.ActiveLayout!=layout||loader.ActiveBindings==null)return;
            solver=manager.ElectricalSolver;
            circuit=manager.CircuitSystem;
            if(!solver||!circuit)return;
            if(bindings!=loader.ActiveBindings){bindings=loader.ActiveBindings;topology=-1;}
            if(topology!=circuit.TopologyVersion){topology=circuit.TopologyVersion;wiring=Inspect();}
            var solved=solver.LatestSolveResult;
            bool Rail(int a,int b)=>solved!=null&&solved.TryGetVoltage(pins[a],pins[b],out var v)&&v is >=4.7f and <=5.3f;
            ready=power&&guard&&wiring==null&&source.IsOutputActive&&!source.IsOverloaded&&
                Rail(1,3)&&Rail(4,5)&&Rail(6,7)&&Rail(8,9)&&Rail(65,3);
            if(stage==0&&ready)Award();
            if((bms||inverter||starting)&&(!ready||soc<=MinimumSoc))
                Trip(!guard?"GUARD OPEN":soc<=MinimumSoc?"BATTERY RESERVE LIMIT":wiring??"CONTROL POWER LOST");
            accumulator+=Mathf.Min(Time.deltaTime,.12f);
            while(accumulator>=.01f){accumulator-=.01f;Step(.01f);}
            Present();
        }
        private void Step(float dt)
        {
            float maxLink=bms&&ready?BatteryVoltage:0;
            float priorLink=link;
            link+=(precharge?maxLink-link:-link)*(1-Mathf.Exp(-dt/(precharge?.62f:.48f)));
            float prechargeWatts=.13f*link*Mathf.Max(0,link-priorLink)/dt;

            float targetRpm=starting&&!failed&&inverter?(parallel?1500f:sync?1500f+Mathf.Clamp(-PhaseError*1.2f,-18f,18f):1455f):0;
            rpm+=(targetRpm-rpm)*(1-Mathf.Exp(-dt/(targetRpm>0?.78f:.52f)));
            genVolts=Mathf.MoveTowards(genVolts,rpm>500f&&!failed?(sync||parallel?busVolts:2.19f):0,dt*1.9f);
            if(parallel)genPhase=busPhase;
            else genPhase=Mathf.Repeat(genPhase+GeneratorHz*360f*dt,360f);

            loadWatts=0;
            for(int i=0;i<feeders.Length;i++)
            {
                if(!feeders[i])continue;
                ages[i]+=dt;
                loadWatts+=SteadyLoad[i]*(1+(i==3?.65f:.32f)*Mathf.Exp(-ages[i]/(i==3?.57f:.35f)));
            }
            float available=InverterCapacity+(parallel?GeneratorCapacity:0);
            float voltageTarget=inverter&&link>=BatteryVoltage*.86f?PhaseVoltage*(1-.018f*loadWatts-.23f*Mathf.Max(0,loadWatts-available)):0;
            busVolts=Mathf.MoveTowards(busVolts,voltageTarget,dt*4.25f);
            float hzTarget=inverter?50f-.02f*loadWatts-.45f*Mathf.Max(0,loadWatts-available):0;
            busHz=Mathf.MoveTowards(busHz,hzTarget,dt*66f);
            busPhase=Mathf.Repeat(busPhase+busHz*360f*dt,360f);
            if(parallel)genPhase=busPhase;
            float served=loadWatts*Mathf.Pow(busVolts/PhaseVoltage,2);
            generatorWatts=parallel?Math.Min(GeneratorCapacity,served*.69f):0;
            batteryWatts=bms?(inverter?Mathf.Max(0,served-generatorWatts)/.91f+.045f:0)+prechargeWatts+
                (starting&&rpm<1200&&!failed?.45f:0):0;
            batteryWhUsed+=batteryWatts*dt/3600;
            soc=Mathf.Clamp01(.82f-batteryWhUsed/CapacityWh);
            motorSpeed=Mathf.MoveTowards(motorSpeed,feeders[3]&&StableBus?1f:0,dt*1.25f);

            syncHold=Window&&!parallel?syncHold+dt:0;
            busHold=StableBus?busHold+dt:0;
            essentialHold=Essentials&&StableBus?essentialHold+dt:0;
            genHold=parallel&&generatorWatts>.5f&&StableBus?genHold+dt:0;
            fullHold=Full?fullHold+dt:0;
            failureHold=failed&&!parallel&&!feeders[3]&&!feeders[4]&&Essentials&&StableBus?failureHold+dt:0;
            failureProven|=failureHold>=1.4f;
            if(stage==1&&busHold>=1.35f&&isolated)Award();
            if(stage==2&&essentialHold>=1.3f)Award();
            if(stage==3&&genHold>=1.0f)Award();
            if(stage==4&&startedMotor&&fullHold>=1.5f)Award();
            if(stage==5&&certifiedRecovery&&repaired&&!failed&&failureProven&&fullHold>=1.5f)Award();
        }
        private void Award()
        {
            if(stage>=Gates.Length||!bindings.SetRuntimeComponentBinding(Gates[stage],source))return;
            stage++;
            manager.QueueEvaluation();
        }
        private void Contact(int index,bool state)
        {
            var c=contacts[index];
            if(c.IsClosed!=state)c.SetState(state?SparkSwitchState.Closed:SparkSwitchState.Open);
        }
        private void GeneratorContacts(bool state)
        {
            parallel=state;
            for(int i=0;i<3;i++)Contact(i,state);
        }
        private void Feeder(int i,bool state)
        {
            if(feeders[i]==state)return;
            feeders[i]=state;
            ages[i]=0;
            Contact(6+i,state);
            if(i==3){Contact(11,state);Contact(12,state);}
        }
        private void Trip(string reason)
        {
            GeneratorContacts(false);
            for(int i=0;i<5;i++)Feeder(i,false);
            bms=precharge=inverter=starting=sync=isolated=false;
            fault=reason;
            Debug.LogWarning("[LEVEL 59] "+reason,this);
        }
        public void Power(){power=!power;source.SetOutput(power,default);}
        public void Guard()
        {
            guard=!guard;
            emergency.SetState(guard?SparkSwitchState.Closed:SparkSwitchState.Open);
        }
        public void Bms(){if(Usable){bms=true;notice="BMS online. Verify the utility breaker is open.";}}
        public void Isolation()
        {
            if(!Usable||!bms)return;
            for(int p=3;p<6;p++)Contact(p,false);
            isolated=true;
        }
        public void Precharge(){if(Usable&&isolated&&bms)precharge=true;}
        public void Inverter(){if(Usable&&isolated&&bms&&precharge&&link>=BatteryVoltage*.94f)inverter=true;}
        private bool CanFeed(int i)=>Usable&&StableBus&&!feeders[i]&&
            (i==0||feeders[i-1])&&(i<3||parallel&&genHold>=.7f)&&(i!=4||motorSpeed>.94f);
        public void Light(){if(CanFeed(0))Feeder(0,true);}
        public void Control(){if(CanFeed(1))Feeder(1,true);}
        public void Cooling(){if(CanFeed(2))Feeder(2,true);}
        public void StartGenerator(){if(Usable&&StableEssentials()&&!failed)starting=true;}
        public void Sync(){if(Usable&&starting&&!parallel&&rpm>=1200&&!failed)sync=true;}
        public void Close()
        {
            if(!Usable||failed||!Window||syncHold<.8f)
            {
                notice="QGEN inhibited · wait for voltage, phase and frequency agreement.";
                return;
            }
            GeneratorContacts(true);
        }
        public void Motor(){if(CanFeed(3)){Feeder(3,true);startedMotor=true;}}
        public void Aux(){if(CanFeed(4))Feeder(4,true);}
        public void Failure()
        {
            if(stage!=5||!Full||repaired||failed||fullHold<1.5f)return;
            // Keep the battery inverter as the island reference. Drop the two lowest priorities.
            fault="GENERATOR POWER LOST";
            failed=true;
            starting=sync=false;
            GeneratorContacts(false);
            Feeder(4,false);
            Feeder(3,false);
        }
        public void Repair(){if(failed&&failureProven&&rpm<90&&ready){repaired=true;notice="Generator serviced. Reset its latched fault.";}}
        public void Reset()
        {
            if(!ready||fault==null||failed&&!repaired)return;
            failed=false;
            fault=null;
        }
        public void Final(){if(stage==5&&failureProven&&repaired&&fullHold>=1.5f)certifiedRecovery=true;}
        private bool StableEssentials()=>Essentials&&essentialHold>=1.3f;
        

        private string Instruction()
        {
            if(stage==6)return "CERTIFIED · Full load restored after generator-loss recovery.";
            if(wiring!=null)return wiring;
            if(!ready)return "Close the guard and restore the solved 5 V rails.";
            if(failed)return repaired?"Reset the generator fault.":failureProven?"Wait for the rotor to stop, then repair.":"QGEN open · motor and auxiliary shed · proving essential service.";
            if(fault!=null)return fault+" · reset once ready.";
            if(!bms)return "Enable BMS.";
            if(!isolated)return "Verify main utility breaker QG is open.";
            if(!precharge)return "Precharge the DC link.";
            if(!inverter)return link>=BatteryVoltage*.94f?"Precharge complete. Form the dead bus.":"Observe the DC link rising toward battery voltage.";
            if(!Essentials)return "Restore lighting, controls and cooling in that order.";
            if(!starting)return "Start the backup generator.";
            if(!parallel&&!sync&&rpm>=1200)notice="Select AUTO SYNC. The relay will check voltage, frequency and phase.";
            if(!parallel)return syncHold>=.8f?"SYNC WINDOW HELD · close QGEN.":sync?"Synchronizing phase, frequency and voltage.":notice;
            if(!feeders[3]||!feeders[4])return "Restore the motor, wait for acceleration, then restore the auxiliary load.";
            return stage<5?"Observing stable full-load power sharing.":repaired?"Hold full load, then certify.":"Run the generator failure test.";
        }
        private void Present()
        {
            statusText.text=Instruction();
            meterText.text=$"BUS {busVolts:F2} V RMS / {busHz:F2} Hz  ·  LINK {link:F2} V  ·  BATTERY {soc:P0} / {batteryWatts/BatteryVoltage:F2} A  ·  GEN {rpm:F0} RPM";
            detailText.text=$"Δf {GeneratorHz-busHz:+0.00;-0.00;0.00} Hz  ·  Δφ {PhaseError:+0.0;-0.0;0.0}°  ·  GEN {generatorWatts:F2} W  ·  BAT {batteryWatts:F2} W  ·  LOAD {loadWatts:F2} W";
            calibrationText.text=$"QG OPEN  ·  QGEN {(parallel?"CLOSED":"OPEN")}  ·  ESSENTIAL {(Essentials&&StableBus?"ON":"PENDING")}  ·  {manager.SatisfiedTargetCount}/6  ·  USED {batteryWhUsed*1000:F2} mWh";
            if(controllerDisplay)controllerDisplay.text=$"{busVolts:F2} V  /  {busHz:F2} Hz";
            if(qualityDisplay)qualityDisplay.text=$"SOC {soc:P0}  /  GEN {rpm:F0} RPM";
            if(rig)rig.Apply(parallel,feeders,guard,rpm,starting&&rpm<1200,motorSpeed,StableBus,Time.deltaTime);
            powerButton.interactable=guardButton.interactable=true;
            bmsButton.interactable=Usable&&!bms;
            isolationButton.interactable=Usable&&bms&&!isolated;
            prechargeButton.interactable=Usable&&bms&&isolated&&!precharge;
            inverterButton.interactable=Usable&&precharge&&!inverter&&link>=BatteryVoltage*.94f;
            lightButton.interactable=CanFeed(0);
            controlButton.interactable=CanFeed(1);
            coolingButton.interactable=CanFeed(2);
            startButton.interactable=Usable&&StableEssentials()&&!starting&&!failed;
            syncButton.interactable=Usable&&starting&&!sync&&rpm>=1200;
            closeButton.interactable=Usable&&starting&&!parallel;
            motorButton.interactable=CanFeed(3);
            auxButton.interactable=CanFeed(4);
            failureButton.interactable=stage==5&&Full&&!repaired&&!failed&&fullHold>=1.5f;
            repairButton.interactable=failed&&!repaired&&failureProven&&rpm<90&&ready;
            resetButton.interactable=ready&&fault!=null&&(!failed||repaired);
            finalButton.interactable=stage==5&&repaired&&!failed&&fullHold>=1.5f;
        }
        private void OnDisable()
        {
            if(bindings!=null)foreach(string gate in Gates)bindings.ClearRuntimeComponentBinding(gate);
        }
    }
}


