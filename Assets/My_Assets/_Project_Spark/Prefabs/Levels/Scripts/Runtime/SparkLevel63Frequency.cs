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
    public sealed class SparkLevel63Frequency : MonoBehaviour
    {
        public static readonly string[] Gates={"L63_Wired","L63_Reserve","L63_Trip","L63_Tuned","L63_Renewable","L63_Recovered"};
        public static readonly (int a,int b)[] Wires={(0,1),(2,3),(0,4),(2,5),(0,6),(2,7),(0,8),(2,9),(10,11),(12,13),(14,15),(16,17),(18,19),(20,21),(22,23),(24,25),(26,27),(28,17),(29,19),(30,21),(31,32),(33,34),(35,36),(37,38),(39,40),(41,42),(43,44),(45,46),(47,48),(49,50),(51,52),(53,54),(53,55),(56,57),(58,50),(59,48),(48,60),(50,61),(62,63),(64,17),(65,19),(66,21),(67,68),(67,69),(67,70),(71,72),(73,74),(75,76),(35,77),(17,78),(19,79),(21,80),(67,81),(82,83),(67,84),(39,85),(86,87),(88,89)};
        public SparkLevelLayout layout;
        public SparkPowerSupply source;
        public SparkSwitch[] contacts;
        public SparkSwitch emergency;
        public SparkTerminal[] pins;
        public SparkLevel63VisualRig rig;
        public SparkLevel63Plot plot;
        public TMP_Text controllerDisplay,qualityDisplay;
        public Text statusText,meterText,detailText,calibrationText;
        public Button powerButton,guardButton,mainButton,calibrateButton,chargeButton,inertiaButton,droopButton,dampingButton,
            tripButton,motorButton,solarButton,faultButton,resetButton,finalButton;

        private enum Event { Normal, GeneratorTrip, MotorStep, RenewableLoss, Recovery }
        private const float RotorInertia=.0038f, RatedSpeed=145f, ReserveSpeed=92f, Efficiency=.90f;
        private const float TargetFrequency=50f, NadirLimit=49.50f, BatteryCapacityJ=162f;
        private readonly List<SparkCircuitConnection> edges=new();
        private readonly HashSet<(int,int)> wired=new();
        private SparkLevelLayoutLoader loader;
        private LevelGamePlayManager manager;
        private SparkLevelSceneBindings bindings;
        private SparkElectricalSolver solver;
        private SparkCircuitSystem circuit;
        private int topology=-1,stage;
        private float clock,hold,elapsed,frequency,rocof,nadir=50,maximumRocof,omega,genOmega,batteryJ=129.6f,
            batteryPower,flywheelPower,netLoad,renewable,gridPower,generatorPower,dispatch,measuredFrequency,measuredRocof,
            supportJ,speedBefore,flywheelFloor,recordedNadir=50,recordedRocof;
        private bool power=true,guard=true,ready,main,calibrated,charge,inertia,droop,damping,encoderTest,
            sensorObserved,resetDone,repeatPassed,certify,generatorClosed,isolatedSupport;
        private string wiring,fault;
        private Event scenario;

        private float RotorEnergy=>.5f*RotorInertia*omega*omega;
        private float SpeedRpm=>omega*60/(2*Mathf.PI);
        private float Vibration=>.05f+.09f*Mathf.Pow(omega/RatedSpeed,2);
        private bool EncoderValid=>Routed(57);
        private bool Tuned=>inertia&&droop&&damping;
        private bool Stable=>frequency is >=49.90f and <=50.10f&&Mathf.Abs(rocof)<.20f;
        private bool FullReserve=>omega>=129&&batteryJ>45;
        private bool Usable=>ready&&main&&calibrated&&fault==null&&EncoderValid;
        private bool CanTest=>Usable&&Tuned&&FullReserve&&Stable&&scenario==Event.Normal;

        private void Start()
        {
            loader=FindFirstObjectByType<SparkLevelLayoutLoader>();
            manager=FindFirstObjectByType<LevelGamePlayManager>();
            if(loader&&manager&&pins?.Length==90&&contacts?.Length==9)return;
            Debug.LogError("[LEVEL 63] Missing layout services, sockets or breaker poles.",this);
            enabled=false;
        }
        private static (int,int) Pair(int a,int b)=>(Math.Min(a,b),Math.Max(a,b));
        private bool Routed(int index)=>wired.Contains(Pair(Wires[index].a,Wires[index].b));
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
                if(!Routed(i))return $"Connect W{i+1:00} · {pins[Wires[i].a].name} → {pins[Wires[i].b].name}.";
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
            bool controls=power&&guard&&source.IsOutputActive&&!source.IsOverloaded&&Rail(1,3)&&Rail(4,5)&&Rail(6,7)&&Rail(8,9)&&Rail(87,3);
            ready=controls&&wiring==null;
            if(stage==0&&ready)Award();
            bool encoderOnly=!EncoderValid&&wired.Count==Wires.Length-1;
            for(int i=0;i<Wires.Length-1&&encoderOnly;i++)encoderOnly=Routed(i);
            if(main&&(!controls||wiring!=null&&!encoderOnly))
            {
                main=false;
                Poles(0,false);
                SetGenerator(false);
                Latch(!guard?"GUARD OPEN":wiring??"CONTROL POWER LOST");
            }
            else if(main&&!EncoderValid&&fault==null&&(charge||encoderTest))
            {
                sensorObserved|=encoderTest;
                Latch("ENCODER FEEDBACK LOST · W58");
            }

            clock+=Mathf.Min(Time.deltaTime,.10f);
            while(clock>=.005f){clock-=.005f;Tick(.005f);}
            Present();
        }

        private void Tick(float dt)
        {
            elapsed+=dt;
            bool transient=scenario!=Event.Normal;
            bool rotorSupport=Usable&&charge&&Tuned&&transient&&omega>ReserveSpeed;
            renewable=main?(scenario==Event.RenewableLoss?.10f:.85f):0;
            netLoad=main?2.60f+(scenario==Event.MotorStep?.85f+.23f*Mathf.Exp(-elapsed/.42f):0):0;
            genOmega+=( (main&&generatorClosed?157.08f:0)-genOmega)*(1-Mathf.Exp(-dt/.70f));
            generatorPower=main&&generatorClosed?1.02f*Mathf.Clamp01(genOmega/157.08f):0;
            float baselineLoad=2.60f-.85f-generatorPower;
            float deficit=Mathf.Max(0,TargetFrequency-frequency);
            float inertiaWatts=rotorSupport?Mathf.Max(0,-measuredRocof)*.32f:0;
            float droopWatts=droop?deficit*2.85f:0;
            float dampingWatts=damping?Mathf.Max(0,-measuredRocof)*.17f:0;
            float demanded=rotorSupport?Mathf.Clamp(inertiaWatts+droopWatts+dampingWatts,0,.78f):0;
            flywheelPower=Mathf.MoveTowards(flywheelPower,demanded,5.5f*dt);
            float chargePower=Usable&&charge&&!transient&&omega<RatedSpeed?Mathf.Min(7.4f,(RatedSpeed-omega)*2.4f):0;
            float friction=omega*.0009f;
            float torque=chargePower*Efficiency/Mathf.Max(omega,24)-flywheelPower/(Efficiency*Mathf.Max(omega,1))-friction*.035f;
            omega=Mathf.Clamp(omega+dt*torque/RotorInertia,0,RatedSpeed+2);
            if(transient){supportJ+=flywheelPower*dt;flywheelFloor=Mathf.Min(omega,flywheelFloor);}
            if(Vibration>.28f&&main)Latch("BEARING VIBRATION LIMIT");

            batteryPower=main&&transient&&batteryJ>30&&Tuned?Mathf.Min(.60f,Mathf.Max(0,deficit*1.65f-measuredRocof*.12f)):0;
            batteryJ=Mathf.Clamp(batteryJ-dt*(batteryPower/Efficiency-(main&&!transient&&batteryJ<129.6f?.21f:0)),0,BatteryCapacityJ);
            if(!main)
            {
                frequency=Mathf.MoveTowards(frequency,0,60*dt);
                rocof=measuredRocof=measuredFrequency=0;
                dispatch=gridPower=0;
                return;
            }
            float transformerTarget=transient?Mathf.Clamp(netLoad-generatorPower-renewable-chargePower,0,3.65f):Mathf.Max(0,baselineLoad);
            if(scenario is Event.GeneratorTrip or Event.Recovery)
                transformerTarget=Mathf.Max(transformerTarget,2.60f-.85f);
            dispatch=Mathf.MoveTowards(dispatch,transformerTarget,(transient?.82f:4.5f)*dt);
            gridPower=dispatch+chargePower;
            if(!transient&&frequency<49.9f)
            {
                // Synchronize the teaching bus to the standing grid before a disturbance.
                frequency=Mathf.MoveTowards(frequency,TargetFrequency,55*dt);
            }
            float mismatch=gridPower+generatorPower+renewable+flywheelPower+batteryPower-netLoad-chargePower;
            rocof=(mismatch-(frequency-TargetFrequency)*(damping?2.8f:1.25f))*.92f;
            frequency=Mathf.Clamp(frequency+rocof*dt,45,52);
            measuredFrequency+=(frequency-measuredFrequency)*Mathf.Min(1,dt*20);
            measuredRocof+=(rocof-measuredRocof)*Mathf.Min(1,dt*17);
            if(transient)
            {
                nadir=Mathf.Min(nadir,frequency);
                maximumRocof=Mathf.Max(maximumRocof,Mathf.Abs(rocof));
            }
            if(main&&frequency<48.5f&&transient){Latch("UNDERFREQUENCY PROTECTION");return;}
            bool response=Stable&&nadir>=NadirLimit&&maximumRocof<1.5f&&flywheelFloor<speedBefore-.12f&&supportJ>.035f;
            bool interval=stage switch
            {
                1=>Usable&&charge&&Tuned&&FullReserve&&Stable&&generatorPower>.8f,
                2=>scenario==Event.GeneratorTrip&&!generatorClosed&&response,
                3=>scenario==Event.MotorStep&&netLoad>3.25f&&response,
                4=>scenario==Event.RenewableLoss&&renewable<.12f&&response,
                5=>scenario==Event.Recovery&&resetDone&&!generatorClosed&&response,
                _=>false
            };
            hold=interval?hold+dt:0;
            if(hold<1.25f)return;
            if(transient){recordedNadir=nadir;recordedRocof=maximumRocof;}
            if(stage==5)
            {
                repeatPassed=true;
                if(!certify)return;
            }
            Award();
            Begin(Event.Normal);
        }

        private void Poles(int index,bool on)
        {
            for(int i=index;i<index+3;i++)
            {
                var item=contacts[i];
                if(item.IsClosed!=on)item.SetState(on?SparkSwitchState.Closed:SparkSwitchState.Open);
            }
        }
        private void SetGenerator(bool on){generatorClosed=on;Poles(3,on);}
        private void Begin(Event next)
        {
            scenario=next;
            elapsed=hold=supportJ=maximumRocof=0;
            nadir=TargetFrequency;
            speedBefore=flywheelFloor=omega;
            if(main)SetGenerator(next is not (Event.GeneratorTrip or Event.Recovery));
        }
        private void Latch(string reason)
        {
            fault=reason;
            calibrated=charge=false;
            flywheelPower=0;
            isolatedSupport=true;
            Poles(6,false);
            Begin(Event.Normal);
            Debug.LogWarning("[LEVEL 63] "+reason,this);
        }
        private void Award()
        {
            if(stage>=Gates.Length||!bindings.SetRuntimeComponentBinding(Gates[stage],source))return;
            stage++;
            hold=0;
            manager.QueueEvaluation();
        }

        public void Power(){power=!power;source.SetOutput(power,default);}
        public void Guard(){guard=!guard;emergency.SetState(guard?SparkSwitchState.Closed:SparkSwitchState.Open);}
        public void Main(){if(ready&&fault==null){main=true;frequency=50;Poles(0,true);SetGenerator(true);}}
        public void Calibrate(){if(ready&&main&&EncoderValid&&fault==null)calibrated=true;}
        public void Charge(){if(Usable){charge=true;isolatedSupport=false;Poles(6,true);}}
        public void Inertia(){if(scenario==Event.Normal)inertia=!inertia;}
        public void Droop(){if(scenario==Event.Normal)droop=!droop;}
        public void Damping(){if(scenario==Event.Normal)damping=!damping;}
        private bool CanRetry=>Usable&&Tuned&&Stable&&elapsed>4&&scenario!=Event.Normal;
        public void Trip(){if(CanTest&&(stage==2||stage==5&&resetDone&&!repeatPassed))Begin(stage==2?Event.GeneratorTrip:Event.Recovery);else if(CanRetry)Begin(Event.Normal);}
        public void Motor(){if(CanTest&&stage==3)Begin(Event.MotorStep);else if(CanRetry)Begin(Event.Normal);}
        public void Solar(){if(CanTest&&stage==4)Begin(Event.RenewableLoss);else if(CanRetry)Begin(Event.Normal);}
        public void Fault(){if(stage==5&&CanTest&&!sensorObserved)encoderTest=true;}
        public void Reset(){if(ready&&fault!=null){fault=null;resetDone=sensorObserved;encoderTest=false;}}
        public void Final(){if(stage==5&&repeatPassed&&Usable&&Stable)certify=true;}

        private string Prompt()
        {
            if(stage==6)return "CERTIFIED · Frequency recovery and encoder protection verified.";
            if(fault!=null)return fault+(ready?" · reset, recalibrate and charge.":" · reconnect W58 before reset.");
            if(wiring!=null)return wiring;
            if(!ready)return "Restore the 5 V controls and close the safety guard.";
            if(!main)return "Select MAIN ON.";
            if(!calibrated)return "Calibrate encoder, vibration and grid sensors.";
            if(!charge)return "Select CHARGE ROTOR.";
            if(!Tuned)return "Enable virtual inertia, droop and damping.";
            if(repeatPassed)return "Recovered trip verified · select CERTIFY.";
            if(scenario!=Event.Normal)return CanRetry?"Frequency response recorded. If limits were exceeded, select the test again to recharge.":"Observe frequency nadir, RoCoF and reserve delivery.";
            if(!FullReserve)return "Accelerating the flywheel to its reserve speed.";
            return stage switch{2=>"Apply GENERATOR TRIP.",3=>"Apply MOTOR STEP.",4=>"Apply RENEWABLE LOSS.",
                5=>sensorObserved?"Repeat GENERATOR TRIP after recommissioning.":encoderTest?"Disconnect W58 · ENC OUT → ENC IN.":"Select W58 TEST.",
                _=>"Holding the commissioned frequency baseline."};
        }
        private void Present()
        {
            statusText.text=Prompt();
            meterText.text=$"GRID {measuredFrequency:F2} Hz  ·  RoCoF {measuredRocof:+0.00;-0.00;0.00} Hz/s  ·  NADIR {(scenario==Event.Normal?recordedNadir:nadir):F2} Hz";
            detailText.text=$"ROTOR {(EncoderValid?SpeedRpm.ToString("F0")+" RPM":"INVALID")}  ·  ENERGY {RotorEnergy:F1} J  ·  FLY {flywheelPower:F2} W  ·  BAT {batteryPower:F2} W / {batteryJ/BatteryCapacityJ:P0}";
            calibrationText.text=$"GRID {gridPower:F2} W  ·  GEN {generatorPower:F2} W  ·  PV {renewable:F2} W  ·  LOAD {netLoad:F2} W  ·  VIB {Vibration:F2}  ·  {manager.SatisfiedTargetCount}/6";
            if(controllerDisplay)controllerDisplay.text=$"{measuredFrequency:F2} Hz / NADIR {recordedNadir:F2}";
            if(qualityDisplay)qualityDisplay.text=$"ROTOR {SpeedRpm:F0} RPM / {RotorEnergy:F1} J";
            if(plot)plot.Show(measuredFrequency,measuredRocof);
            if(rig)rig.Apply(main,generatorClosed,guard,genOmega,omega,scenario==Event.MotorStep,scenario==Event.RenewableLoss,Time.deltaTime);
            powerButton.interactable=guardButton.interactable=true;
            mainButton.interactable=ready&&!main&&fault==null;
            calibrateButton.interactable=ready&&main&&!calibrated&&fault==null;
            chargeButton.interactable=Usable&&!charge;
            inertiaButton.interactable=droopButton.interactable=dampingButton.interactable=Usable&&scenario==Event.Normal;
            inertiaButton.GetComponentInChildren<Text>().text=inertia?"VSM ON":"VSM OFF";
            droopButton.GetComponentInChildren<Text>().text=droop?"DROOP ON":"DROOP OFF";
            dampingButton.GetComponentInChildren<Text>().text=damping?"DAMPING ON":"DAMPING OFF";
            tripButton.interactable=CanTest&&(stage==2||stage==5&&resetDone&&!repeatPassed)||CanRetry&&(scenario is Event.GeneratorTrip or Event.Recovery)&&!repeatPassed;
            motorButton.interactable=stage==3&&(CanTest||CanRetry);
            solarButton.interactable=stage==4&&(CanTest||CanRetry);
            faultButton.interactable=stage==5&&CanTest&&!sensorObserved&&!encoderTest;
            resetButton.interactable=ready&&fault!=null;
            finalButton.interactable=stage==5&&repeatPassed&&Usable&&Stable&&!certify;
        }
        private void OnDisable()
        {
            if(bindings!=null)foreach(string gate in Gates)bindings.ClearRuntimeComponentBinding(gate);
        }
    }
}

