using System.Collections.Generic;
using ProjectSpark.Circuit;
using ProjectSpark.Electrical;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.Gameplay
{
    [DisallowMultipleComponent]
    public sealed class SparkLevel51Maintenance : MonoBehaviour
    {
        public static readonly string[] Gates={"L51_Wired","L51_Baseline","L51_Imbalance","L51_Bearing","L51_Prevented","L51_Certified"};
        public static readonly (int a,int b)[] Wires={
            (0,1),(2,3),(0,4),(2,5),(6,7),(8,9),(10,11),(12,13),(0,14),(2,15),
            (16,17),(0,18),(2,19),(20,21),(22,23),(24,25),(26,27),(28,29),(30,31)
        };
        private const float Step=.02f, Ambient=24f, ThermalLimit=61f;
        private const int Window=200;
        private static readonly int[] Gray={0,2,3,1};
        private static readonly int[] Decode={0,-1,1,0,1,0,0,-1,-1,0,0,1,0,1,-1,0};
        private enum Diagnosis { RotorImbalance, BearingWear, ExcessiveFriction, Healthy }
        private readonly struct Sample
        {
            public readonly float vibration, sin, cos, current, temp, rpm;
            public Sample(float v,float angle,float a,float t,float speed)
            { vibration=v; sin=Mathf.Sin(angle); cos=Mathf.Cos(angle); current=a; temp=t; rpm=speed; }
        }

        public SparkLevelLayout layout;
        public SparkPowerSupply source;
        public SparkMotor motor;
        public SparkResistor shunt,driver;
        public SparkSwitch drive,safety;
        public SparkTerminal[] pins;
        public SparkLevel51VisualRig rig;
        public TMP_Text controllerDisplay,qualityDisplay;
        public Text statusText,meterText,detailText;
        public Button powerButton,slowButton,fastButton,startButton,stopButton,baselineButton,verifyButton;
        public Button imbalanceButton,bearingButton,wearButton,answerButton,analyzeButton,preventButton;
        public Button guardButton,weightButton,cartridgeButton,resetButton;

        private readonly List<SparkCircuitConnection> wires=new();
        private readonly Dictionary<SparkTerminal,SparkTerminal> nets=new();
        private readonly Sample[] samples=new Sample[Window];
        private SparkLevelLayoutLoader loader;
        private LevelGamePlayManager manager;
        private SparkLevelSceneBindings bindings;
        private SparkCircuitSystem circuit;
        private SparkElectricalSolver solver;
        private int topology=-1,stage,targetRpm=45,edge,count,phase,sampleIndex,sampleCount;
        private int encoderReference;
        private float accumulator,omega,angle,time,driveClock,encoderClock,rpm,current,heat=Ambient,temperature=Ambient;
        private float rms,synchronous,irregular,meanCurrent,meanRpm,thermalTrend,wear,baselineRms,baselineCurrent,baselineTemperature;
        private float verificationTime,predictionTime,analysisClock;
        private bool powered=true,running,ready,safe,guardOpen,imbalance,cartridgeInstalled=true;
        private bool progressive,warning,preventRequested,serviceRequired,removed,replaced,balanced,verification;
        private bool hasBaseline;
        private Diagnosis answer;
        private string wiring,fault,feedback="";
        private float ActualRpm=>omega*60/(2*Mathf.PI);
        private bool Stopped=>!running && !drive.IsClosed && ActualRpm<.5f;
        private bool ServiceAllowed=>Stopped && guardOpen;
        private bool Stable=>running && sampleCount==Window && Mathf.Abs(meanRpm-targetRpm)<1.8f && Mathf.Abs(rpm-targetRpm)<2.6f;
        private bool CanStart=>Stopped && ready && safe && cartridgeInstalled && fault==null && !serviceRequired;
        private bool CanReset=>Stopped && ready && safe && cartridgeInstalled && wear<.01f && !imbalance && replaced && balanced && temperature<45;

        private void Update()
        {
            if(loader==null) loader=FindFirstObjectByType<SparkLevelLayoutLoader>();
            if(loader==null || loader.ActiveLayout!=layout) return;
            if(manager==null) manager=FindFirstObjectByType<LevelGamePlayManager>();
            if(manager==null) return;
            solver=manager.ElectricalSolver;
            circuit=manager.CircuitSystem;
            if(solver==null || circuit==null) return;
            if(bindings!=loader.ActiveBindings) { bindings=loader.ActiveBindings; topology=-1; }
            if(bindings==null) return;
            if(topology!=circuit.TopologyVersion) { topology=circuit.TopologyVersion; wiring=CheckWires(); }
            ReadElectrical();
            if(stage==0 && ready && safe) Award();
            if(running && (!ready || !safe)) Trip(!safe?"GUARD INTERLOCK LOST":wiring??"POWER OR SENSOR FAULT");
            accumulator+=Mathf.Min(Time.unscaledDeltaTime,.16f);
            while(accumulator>=Step) { accumulator-=Step; Simulate(); }
            Present();
        }

        public void Power()
        {
            powered=!powered;
            if(!powered) StopMachine();
            source.SetOutput(powered,default);
        }
        public void Speed45() { if(Stopped) { targetRpm=45; ClearSamples(); } }
        public void Speed60() { if(Stopped && stage>=2 && stage<5) { targetRpm=60; ClearSamples(); } }
        public void StartMachine()
        {
            if(!CanStart) return;
            running=true;
            feedback="";
            ClearSamples();
            SetContact(drive,true);
        }
        public void StopMachine()
        {
            running=false;
            verification=false;
            SetContact(drive,false);
        }
        public void Baseline()
        {
            if(stage!=1 || !Stable || targetRpm!=45 || imbalance || wear>.01f || temperature>35 || !safe) return;
            baselineRms=rms;
            baselineCurrent=meanCurrent;
            baselineTemperature=temperature;
            hasBaseline=true;
            Award();
        }
        public void Imbalance()
        {
            if(stage!=2 || !Stopped || !safe) return;
            imbalance=true;
            feedback="Eccentric test mass installed. Start and collect data.";
            ClearSamples();
        }
        public void Bearing()
        {
            if(stage!=3 || !Stopped || !safe) return;
            imbalance=false;
            wear=.48f;
            feedback="Worn test cartridge installed. Start and collect data.";
            ClearSamples();
        }
        public void Progressive()
        {
            if(stage!=4 || !Stopped || !safe || wear<.25f) return;
            if(fault!=null)
            {
                if(temperature>=38 || fault!="CRITICAL FAILURE RISK") return;
                fault=null;
                wear=.48f;
            }
            progressive=true;
            imbalance=true;
            feedback="Progressive test loaded. Start and watch the trend.";
            ClearSamples();
        }
        public void SelectDiagnosis() => answer=(Diagnosis)(((int)answer+1)%4);
        public void Analyze()
        {
            if(!Stable || !hasBaseline || !safe) return;
            var measured=Classify();
            if(answer!=measured) { feedback="Diagnosis does not match the measured signature."; return; }
            if(stage==2 && measured==Diagnosis.RotorImbalance && imbalance ||
               stage==3 && measured==Diagnosis.BearingWear && wear>.25f)
            {
                feedback="Diagnosis confirmed: "+DiagnosisName(measured);
                Award();
            }
        }
        public void Prevent()
        {
            if(stage!=4 || !running || !warning || fault!=null) return;
            preventRequested=true;
            progressive=false;
            StopMachine();
        }
        public void Guard()
        {
            if(!Stopped && !guardOpen) return;
            guardOpen=!guardOpen;
            SetContact(safety,!guardOpen);
        }
        public void Balance()
        {
            if(stage!=5 || !ServiceAllowed) return;
            imbalance=false;
            balanced=true;
        }
        public void Service()
        {
            if(stage!=5 || !ServiceAllowed || replaced) return;
            if(!removed)
            {
                removed=true;
                cartridgeInstalled=false;
                feedback="Worn cartridge removed. Install the healthy spare.";
                return;
            }
            wear=0;
            cartridgeInstalled=true;
            replaced=true;
            feedback="Healthy cartridge installed.";
        }
        public void ResetFault()
        {
            if(!CanReset) return;
            serviceRequired=false;
            fault=null;
            warning=false;
            targetRpm=45;
            ClearSamples();
        }
        public void Verify()
        {
            if(stage!=5 || !Stable || targetRpm!=45 || !replaced || !balanced || serviceRequired || !safe) return;
            verification=true;
            verificationTime=0;
        }

        private static void SetContact(SparkSwitch sw,bool closed)
        {
            if(sw.IsClosed!=closed) sw.SetState(closed?SparkSwitchState.Closed:SparkSwitchState.Open);
        }
        private void Trip(string reason)
        {
            fault=reason;
            preventRequested=false;
            progressive=false;
            StopMachine();
        }
        private SparkTerminal Root(SparkTerminal pin)
        {
            while(nets.TryGetValue(pin,out var parent)) pin=parent;
            return pin;
        }
        private bool Routed(int index)
        {
            var (a,b)=Wires[index];
            return Root(pins[a])==Root(pins[b]);
        }
        private string CheckWires()
        {
            circuit.CopyConnections(wires);
            nets.Clear();
            foreach(var w in wires)
            {
                if(w.Kind!=SparkConnectionKind.Wire) continue;
                var a=Root(w.A);
                var b=Root(w.B);
                if(a!=b) nets[b]=a;
            }
            var seen=new HashSet<SparkTerminal>();
            for(int i=0;i<Wires.Length;i++)
            {
                var (a,b)=Wires[i];
                if(!wires.Exists(w=>w.Kind==SparkConnectionKind.Wire && (w.A==pins[a] && w.B==pins[b] || w.B==pins[a] && w.A==pins[b])))
                    return $"Connect W{i+1:00} · {pins[a].name} → {pins[b].name}.";
                if(a!=0 && a!=2 && !seen.Add(Root(pins[a]))) return "Separate the signal and motor connections.";
                if(a!=0 && a!=2 && (Root(pins[a])==Root(pins[0]) || Root(pins[a])==Root(pins[2]))) return "Remove the power-to-signal bridge.";
            }
            return Root(pins[0])==Root(pins[2])?"Separate +5 V and return.":null;
        }
        private void ReadElectrical()
        {
            var result=solver.LatestSolveResult;
            bool Voltage(int a,int b,out float v)
            {
                v=0;
                return result!=null && result.TryGetVoltage(pins[a],pins[b],out v);
            }
            bool High(int a,int b)=>Voltage(a,b,out float v) && v is >=4.8f and <=5.2f;
            ready=powered && wiring==null && source.IsOutputActive && !source.IsOverloaded &&
                High(1,3) && High(4,5) && High(Wires[8].b,Wires[9].b) && High(Wires[11].b,Wires[12].b);
            safe=!guardOpen && safety.IsClosed && Routed(18) && High(Wires[18].b,3);
            current=Routed(14) && Routed(15) && Voltage(Wires[14].b,Wires[15].b,out float drop)
                ? Mathf.Abs(drop)/shunt.ResistanceOhms : 0;
        }

        private void Simulate()
        {
            time+=Step;
            if(running && progressive) wear=Mathf.Min(1,wear+.016f*Step*Mathf.Clamp01(ActualRpm/45));
            float drag=.090f+.102f*wear+.002f*omega;
            float target=targetRpm*2*Mathf.PI/60;
            if(running)
            {
                driveClock+=Step;
                if(driveClock>=.16f)
                {
                    driveClock=0;
                    float desired=Mathf.Clamp(drag+(target-omega)*.034f,.075f,.214f);
                    float resistance=Mathf.Max(.06f,5/desired-22.245f);
                    if(Mathf.Abs(driver.ResistanceOhms-resistance)>.13f) driver.SetResistance(resistance);
                }
            }
            float torque=running && drive.IsClosed?Mathf.Abs(motor.ElectricalState.Current):0;
            omega=Mathf.Max(0,omega+(torque-drag)/.015f*Step);
            angle+=omega*Step;
            int next=Mathf.FloorToInt(angle*80/(2*Mathf.PI));
            while(edge<next)
            {
                int code=Gray[++edge&3];
                if(ready)
                {
                    int received=(Routed(16)?code&2:0)|(Routed(17)?code&1:0);
                    count+=Decode[phase*4+received];
                }
                phase=code;
            }
            encoderClock+=Step;
            if(encoderClock>=.50f)
            {
                rpm=Mathf.Lerp(rpm,(count-encoderReference)*60/(80*encoderClock),.68f);
                encoderReference=count;
                encoderClock=0;
            }
            float heatInput=(.34f+3.3f*wear)*omega+2f*torque*torque;
            heat+=(heatInput-.56f*(heat-Ambient))/5.5f*Step;
            temperature=Mathf.Lerp(temperature,heat,1f-Mathf.Exp(-Step/1.3f));
            if(running && ready && safe && Routed(10) && Routed(13))
            {
                float speed=omega/4.7124f;
                float v=speed*(.25f+(imbalance?1.23f:0))*Mathf.Sin(angle) +
                    speed*(.10f+1.45f*wear)*(.68f*Mathf.Sin(angle*5.31f+Mathf.Sin(time*4.2f))+.42f*Mathf.Sin(angle*8.67f));
                samples[sampleIndex]=new Sample(v,angle,current,temperature,rpm);
                sampleIndex=(sampleIndex+1)%Window;
                sampleCount=Mathf.Min(Window,sampleCount+1);
                analysisClock+=Step;
                if(analysisClock>=.20f && sampleCount==Window) { analysisClock=0; AnalyzeWindow(); }
            }
            if(running && (motor.IsOverCurrent || temperature>=ThermalLimit || wear>=.98f))
                Trip(motor.IsOverCurrent?"MOTOR OVERLOAD":"CRITICAL FAILURE RISK");
            if(preventRequested && Stopped && !drive.IsClosed && fault==null)
            {
                preventRequested=false;
                serviceRequired=true;
                Award();
            }
            if(verification)
            {
                bool healthy=Stable && Classify()==Diagnosis.Healthy && Mathf.Abs(meanCurrent-baselineCurrent)<.012f &&
                    rms<baselineRms+.20f && temperature<Mathf.Max(34,baselineTemperature+5) && thermalTrend<.24f;
                verificationTime=healthy?verificationTime+Step:0;
                if(verificationTime>=4f) { verification=false; Award(); }
            }
        }

        private void ClearSamples()
        {
            sampleIndex=sampleCount=0;
            rms=synchronous=irregular=meanCurrent=meanRpm=thermalTrend=analysisClock=0;
        }
        private void AnalyzeWindow()
        {
            float sum=0,sin=0,cos=0,amps=0,speed=0,first=0,last=0;
            for(int i=0;i<Window;i++)
            {
                var x=samples[(sampleIndex+i)%Window];
                sum+=x.vibration*x.vibration;
                sin+=x.vibration*x.sin;
                cos+=x.vibration*x.cos;
                amps+=x.current;
                speed+=x.rpm;
                if(i<25) first+=x.temp;
                if(i>=Window-25) last+=x.temp;
            }
            rms=Mathf.Sqrt(sum/Window);
            synchronous=2*Mathf.Sqrt(sin*sin+cos*cos)/Window;
            irregular=Mathf.Sqrt(Mathf.Max(0,rms*rms-synchronous*synchronous*.5f));
            meanCurrent=amps/Window;
            meanRpm=speed/Window;
            thermalTrend=(last-first)/(25*(Window-25)*Step);
            predictionTime=thermalTrend>.12f && temperature<ThermalLimit?(ThermalLimit-temperature)/thermalTrend:0;
            warning=progressive && Stable && temperature>36 && thermalTrend>.18f && predictionTime is >0 and <110 &&
                meanCurrent>baselineCurrent+.04f && irregular>.45f;
        }
        private Diagnosis Classify()
        {
            if(irregular>.39f && meanCurrent>baselineCurrent+.028f && (thermalTrend>.12f || temperature>baselineTemperature+4))
                return meanCurrent>.198f?Diagnosis.ExcessiveFriction:Diagnosis.BearingWear;
            if(synchronous>.73f && rms>baselineRms+.30f && meanCurrent<baselineCurrent+.026f && thermalTrend<.85f)
                return Diagnosis.RotorImbalance;
            return Diagnosis.Healthy;
        }
        private static string DiagnosisName(Diagnosis d)=>d switch {
            Diagnosis.RotorImbalance=>"ROTOR IMBALANCE",Diagnosis.BearingWear=>"BEARING WEAR",
            Diagnosis.ExcessiveFriction=>"EXCESSIVE FRICTION",_=>"HEALTHY"
        };
        private void Award()
        {
            if(stage>=Gates.Length || !bindings.SetRuntimeComponentBinding(Gates[stage],source)) return;
            stage++;
            manager.QueueEvaluation();
        }
        private string Prompt()
        {
            if(wiring!=null) return wiring;
            if(!powered) return "Turn on POWER.";
            if(fault!=null) return "PROTECTIVE STOP · "+fault+
                (stage==4 && fault=="CRITICAL FAILURE RISK"?" · Cool below 38 °C, then repeat WEAR TEST.":"");
            if(stage==5)
            {
                if(serviceRequired) return !guardOpen ? "Open guard after the rotor stops, or RESET after repair." :
                    !replaced ? "Remove the worn bearing, then install the healthy spare." : "Correct the balance, close guard and RESET.";
                return verification?"Verifying against the healthy baseline.":"Start at 45 RPM, then VERIFY HEALTH.";
            }
            if(warning) return "MAINTENANCE REQUIRED · Press PREVENTIVE STOP before the critical threshold.";
            return stage switch {
                1=>"Start at 45 RPM. Capture BASELINE after readings stabilize.",
                2=>"Stop, install the imbalance test weight, restart and diagnose.",
                3=>"Stop, install the worn bearing test cartridge, restart and diagnose.",
                4=>"Stop, load PROGRESSIVE WEAR, restart and watch the trend.",
                6=>"CERTIFIED · Stable operation restored.",
                _=>"Waiting for powered sensors and closed guard."
            };
        }
        private void Present()
        {
            float index=Mathf.Clamp(100-(Mathf.Max(0,rms-baselineRms)*28+Mathf.Max(0,meanCurrent-baselineCurrent)*350+
                Mathf.Max(0,temperature-32)*1.3f),0,100);
            string condition=fault??(warning?"MAINTENANCE REQUIRED":serviceRequired?"SERVICE REQUIRED":!ready||!safe?"SENSOR / SAFETY OFFLINE":
                Stable&&hasBaseline?DiagnosisName(Classify()):"COLLECTING");
            statusText.text=Prompt();
            string mode=running?"RUN":Stopped?"STOPPED":"COAST";
            float tach=Stopped?0:rpm;
            meterText.text=$"{tach:F1}/{targetRpm} RPM  |  {motor.ElectricalState.Voltage:F2} V  |  SHUNT {current*1000:F1} mA  |  RMS {rms:F2} mm/s  |  {temperature:F1} °C  |  HEALTH {index:F0}/100";
            detailText.text=$"1× {synchronous:F2}  |  IRREGULAR {irregular:F2} mm/s  |  TREND {thermalTrend:+0.00;-0.00;0.00} °C/s  |  {condition}  |  {manager.SatisfiedTargetCount}/6";
            controllerDisplay.text=$"{mode} • {tach:F1} RPM • {current*1000:F0} mA\n{(guardOpen?"GUARD OPEN":wear>.01f?"WORN BEARING":imbalance?"TEST WEIGHT":"GUARD CLOSED")}";
            qualityDisplay.text=$"{condition}\n"+(warning?$"THERMAL EST. {predictionTime:F0} s":$"RMS {rms:F2}  /  {temperature:F1} °C");
            rig.SetMeasuredSpeed(ActualRpm,rpm);
            rig.SetCondition(warning||serviceRequired,fault!=null);
            rig.SetGuardOpen(guardOpen);
            rig.SetEccentricWeightInstalled(imbalance);
            rig.SetBearingServiceState(cartridgeInstalled,!replaced,removed);
            powerButton.interactable=true;
            slowButton.interactable=Stopped && targetRpm!=45;
            fastButton.interactable=Stopped && targetRpm!=60 && stage>=2 && stage<5;
            startButton.interactable=CanStart;
            stopButton.interactable=running;
            baselineButton.interactable=stage==1 && Stable && targetRpm==45 && temperature<35;
            imbalanceButton.interactable=stage==2 && Stopped && !imbalance && safe;
            bearingButton.interactable=stage==3 && Stopped && wear<.25f && safe;
            wearButton.interactable=stage==4 && Stopped && !progressive && wear>.25f && safe &&
                (fault==null || fault=="CRITICAL FAILURE RISK" && temperature<38);
            answerButton.interactable=stage is 2 or 3;
            answerButton.GetComponentInChildren<Text>().text=DiagnosisName(answer);
            analyzeButton.interactable=answerButton.interactable && Stable;
            preventButton.interactable=running && warning && stage==4;
            guardButton.interactable=Stopped;
            guardButton.GetComponentInChildren<Text>().text=guardOpen?"CLOSE GUARD":"OPEN GUARD";
            weightButton.interactable=stage==5 && ServiceAllowed && !balanced;
            cartridgeButton.interactable=stage==5 && ServiceAllowed && !replaced;
            cartridgeButton.GetComponentInChildren<Text>().text=removed?"INSTALL BEARING":"REMOVE BEARING";
            resetButton.interactable=CanReset && serviceRequired;
            verifyButton.interactable=stage==5 && !serviceRequired && !verification && Stable && targetRpm==45;
            if(feedback.Length>0 && (stage is 2 or 3) && running) statusText.text+="  "+feedback;
        }
        private void OnDisable()
        {
            if(gameObject.activeInHierarchy && drive!=null) StopMachine();
            if(bindings!=null) foreach(string gate in Gates) bindings.ClearRuntimeComponentBinding(gate);
            bindings=null;
        }
    }
}
