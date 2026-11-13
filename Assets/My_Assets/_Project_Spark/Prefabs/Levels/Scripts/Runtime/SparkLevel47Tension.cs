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
    public sealed class SparkLevel47Tension : MonoBehaviour
    {
        public static readonly string[] Gates = {"L47_Build","L47_Calibration","L47_Stable","L47_Growth","L47_Load","L47_Repair"};
        private static readonly int[] Gray = {0,2,3,1};
        private static readonly int[] Delta = {0,-1,1,0, 1,0,0,-1, -1,0,0,1, 0,1,-1,0};
        private static readonly float[] Brake = {0,.003f,.007f,.043f};
        private static readonly string[] Loads = {"RELEASED","LIGHT LOAD","TEST LOAD","OVERLOAD"};
        private enum Mode { Off, Manual, Auto, Fault }
        private const float StepSize=.01f, Edge=2*Mathf.PI/80f, ToRpm=60f/(2*Mathf.PI);

        [Serializable]
        public sealed class Channel
        {
            public SparkMotor motor;
            public SparkSwitch enable;
            public SparkResistor impedance;
            public SparkPowerSupply pwm, a, b;
            public SparkTerminal pwmIn, aIn, bIn;
            [NonSerialized] public float omega, rpm, measured, duty, applied, integral, angle, noEdge, stall;
            [NonSerialized] public int ticks, previous, count, sampled;
            [NonSerialized] public int forward, reverse;

            public void Reset()
            {
                omega=rpm=measured=duty=applied=integral=angle=noEdge=stall=0;
                ticks=previous=count=sampled=forward=reverse=0;
            }
        }

        public SparkLevelLayout layout;
        public SparkPowerSupply source, sensor;
        public SparkTerminal[] pins;
        public Channel feed, takeup;
        public SparkLevel47VisualRig rig;
        public Text statusText, meterText, detailText;
        public TMP_Text feedDisplay, takeupDisplay, tensionDisplay;
        public Button powerButton, calibrateButton, manualButton, autoButton, startButton, stopButton, loadButton, testButton, releaseButton, resetButton, restartButton;
        public Transform[] webSlats;
        public GameObject[] lamps;
        [SerializeField, Min(.001f)] private float feedCore=.034f, takeupCore=.025f;
        [SerializeField, Min(.0001f)] private float materialThickness=.0013f;
        [SerializeField, Min(.2f)] private float totalLength=3.8f;
        [SerializeField, Min(.01f)] private float dancerInertia=.040f, dancerSpring=4f, dancerDamping=.68f;
        [SerializeField, Min(0)] private float tensionCorrection=.027f, speedP=.0025f, speedI=.006f;
        [SerializeField, Min(.0001f)] private float inertia=.0027f;
        [SerializeField, Min(.1f)] private float complianceRelaxation=2.5f;
        [SerializeField, Min(1f)] private float tensionLimit=3.5f;

        private readonly List<SparkCircuitConnection> wires=new();
        private readonly Dictionary<SparkTerminal,SparkTerminal> links=new();
        private SparkLevelLayoutLoader loader;
        private LevelGamePlayManager manager;
        private SparkLevelSceneBindings bindings;
        private SparkCircuitSystem circuit;
        private SparkElectricalSolver solver;
        private Mode mode;
        private int topology=-1, stage, load;
        private float accumulator, controlTimer, sampleTimer, hold, reference, strain, tension, tensionTime, travel;
        private float baseline, baselineDuty, baselineTakeup, neutral, sensorVolts, dancer, dancerVelocity, wound;
        private float feedRadius, takeupRadius, initialFeed, growthStart, growthFeedRadius, growthTakeupRadius, growthRatio;
        private Mode selected=Mode.Auto;
        private bool powered=true, calibrated, sensorConnected;
        private bool ready, loadDrop, dutyRise, sensorFault, repaired;
        private string wiring, fault;
        private Vector3[] slatOrigin;

        private void Update()
        {
            if (loader==null) loader=FindFirstObjectByType<SparkLevelLayoutLoader>();
            if (loader==null || loader.ActiveLayout!=layout) return;
            if (manager==null) manager=FindFirstObjectByType<LevelGamePlayManager>();
            if (manager==null) return;
            circuit=manager.CircuitSystem;
            solver=manager.ElectricalSolver;
            if (circuit==null || solver==null) return;
            if (bindings!=loader.ActiveBindings)
            {
                bindings=loader.ActiveBindings;
                if (bindings==null) return;
                Restart();
            }
            if (topology!=circuit.TopologyVersion)
            {
                topology=circuit.TopologyVersion;
                wiring=CheckWiring();
                if (Running && wiring!=null) Trip(!sensorConnected ? "SENSOR SIGNAL LOST" : "WIRING LOST");
            }
            ready=powered && wiring==null && ReadSupply() && ReadSensor();
            if (!powered && Running) Stop(Mode.Off);
            if (stage==0 && ready) Award();
            accumulator+=Mathf.Min(Time.unscaledDeltaTime,.10f);
            while (accumulator>=StepSize)
            {
                accumulator-=StepSize;
                Simulate(StepSize);
            }
            Present();
        }

        private bool Running => mode is Mode.Manual or Mode.Auto;
        private bool CanRun => ready && circuit.TopologyVersion==topology && mode!=Mode.Fault;
        public void MainPower()
        {
            if (bindings==null) return;
            if (Running) Stop(Mode.Off);
            powered=!powered;
            source.SetOutput(powered,default);
        }
        public void SelectManual() { if (!Running && mode!=Mode.Fault) selected=Mode.Manual; }
        public void SelectAuto() { if (!Running && mode!=Mode.Fault) selected=Mode.Auto; }
        public void StartWinding()
        {
            if (!CanRun || Running || !calibrated && selected==Mode.Auto) return;
            if (!ReadPhase(feed,out feed.previous) || !ReadPhase(takeup,out takeup.previous)) return;
            mode=selected;
            reference=0;
            hold=controlTimer=sampleTimer=tensionTime=0;
            Prepare(feed);
            Prepare(takeup);
        }
        public void StopWinding() { if (Running) Stop(Mode.Off); }
        private bool CanCalibrate => CanRun && !Running && feed.rpm<1 && takeup.rpm<1 &&
            Mathf.Abs(dancer)<.038f && Mathf.Abs(sensorVolts-2.5f)<.12f;
        public void Calibrate()
        {
            if (!CanCalibrate) return;
            neutral=sensorVolts;
            calibrated=true;
            if (stage==1) Award();
        }
        public void CycleBrake() => SetBrake((load+1)%Brake.Length);
        public void TestLoad() => SetBrake(2);
        public void ReleaseBrake() => SetBrake(0);
        private void SetBrake(int setting)
        {
            if (setting==2 && load!=2 && stage==4 && mode==Mode.Auto && Stable())
            {
                baseline=sensorVolts;
                baselineDuty=feed.duty;
                baselineTakeup=takeup.duty;
                loadDrop=dutyRise=false;
            }
            else if (setting!=2) baseline=0;
            load=setting;
            hold=0;
        }

        public void ResetFault()
        {
            if (mode!=Mode.Fault || !ready || load==3 || feed.rpm>2 || takeup.rpm>2 || !calibrated ||
                Mathf.Abs(sensorVolts-neutral)>.18f) return;
            repaired=sensorFault;
            fault=null;
            Stop(Mode.Off);
        }

        public void Restart()
        {
            if (bindings==null) return;
            Stop(Mode.Off);
            foreach (string gate in Gates) bindings.ClearRuntimeComponentBinding(gate);
            feed.Reset();
            takeup.Reset();
            powered=true;
            source.SetOutput(true,default);
            stage=load=0;
            selected=Mode.Auto;
            topology=-1;
            accumulator=controlTimer=sampleTimer=hold=reference=strain=tension=tensionTime=travel=baseline=0;
            wound=dancer=dancerVelocity=0;
            neutral=2.5f;
            calibrated=loadDrop=dutyRise=sensorFault=repaired=false;
            fault=null;
            SetSource(sensor,2.5f);
            UpdateRadii();
            initialFeed=feedRadius;
            manager.QueueEvaluation();
        }

        private static void Prepare(Channel c)
        {
            c.forward=c.reverse=0;
            c.sampled=c.count;
            c.noEdge=c.stall=c.integral=0;
        }

        private void UpdateRadii()
        {
            feedRadius=Mathf.Sqrt(feedCore*feedCore+(totalLength-wound)*materialThickness/Mathf.PI);
            takeupRadius=Mathf.Sqrt(takeupCore*takeupCore+wound*materialThickness/Mathf.PI);
        }

        private void Simulate(float dt)
        {
            if (Running && !ready) Trip("SUPPLY OR SENSOR FEEDBACK INVALID");
            controlTimer+=dt;
            if (controlTimer>=.049f)
            {
                controlTimer=0;
                SetSource(sensor,Mathf.Clamp(2.5f+5f*dancer,.5f,4.5f));
                if (ready) ReadSensor();
                if (Running)
                {
                    reference=Mathf.MoveTowards(reference,32f,36f*.05f);
                    float expected=reference*feedRadius/takeupRadius;
                    float feedSpeed=Mathf.Lerp(reference,Mathf.Clamp(feed.measured,0,reference+4f),.25f);
                    float takeupTarget=mode==Mode.Auto
                        ? Mathf.Clamp((feedSpeed/ToRpm*feedRadius-tensionCorrection*(sensorVolts-neutral))/takeupRadius*ToRpm,0,78)
                        : reference;
                    Actuate(feed,Command(feed,reference,reference));
                    if (Running) Actuate(takeup,Command(takeup,mode==Mode.Auto ? expected : reference,takeupTarget));
                }
            }

            Integrate(feed,Brake[load],dt);
            Integrate(takeup,0,dt);
            float feedSurface=feed.omega*feedRadius, takeupSurface=takeup.omega*takeupRadius;
            float difference=takeupSurface-feedSurface;
            strain+=(difference-strain*complianceRelaxation)*dt;
            tension=86f*strain+1.8f*difference;
            dancerVelocity+=(.78f*tension-dancerSpring*dancer-dancerDamping*dancerVelocity)*dt/dancerInertia;
            dancer=Mathf.Clamp(dancer+dancerVelocity*dt,-.40f,.40f);
            if (Mathf.Abs(dancer)>=.399f && Mathf.Sign(dancerVelocity)==Mathf.Sign(dancer)) dancerVelocity=0;
            float transfer=Mathf.Min(feedSurface,takeupSurface)*dt;
            wound=Mathf.Min(totalLength,wound+transfer);
            travel=Mathf.Repeat(travel+transfer,1f);
            UpdateRadii();
            SampleEncoder(feed,dt);
            SampleEncoder(takeup,dt);
            sampleTimer+=dt;
            if (sampleTimer>=.499f)
            {
                Measure(feed,sampleTimer);
                Measure(takeup,sampleTimer);
                sampleTimer=0;
            }
            if (!Running) return;
            if (feed.reverse>0 || takeup.reverse>0) { Trip("ENCODER DIRECTION MISMATCH"); return; }
            if (Unsafe(feed,dt) || Unsafe(takeup,dt)) return;
            tensionTime=Mathf.Abs(tension)>tensionLimit ? tensionTime+dt : 0;
            if (tensionTime>.45f) { Trip(tension>0 ? "OVER-TENSION" : "EXCESSIVE SLACK"); return; }
            if (wound>=totalLength-.002f) { Trip("FEED ROLL EMPTY · RESTART TRIAL"); return; }

            if (stage==4 && baseline>0 && load==2)
            {
                loadDrop |= sensorVolts>=baseline+.12f;
                dutyRise |= feed.duty>baselineDuty+.055f && takeup.duty<baselineTakeup-.018f;
            }
            bool conserved=Mathf.Abs(totalLength-MaterialOn(feedRadius,feedCore)-MaterialOn(takeupRadius,takeupCore))<.001f;
            bool growth=wound-growthStart>=.15f && growthFeedRadius-feedRadius>.0005f &&
                takeupRadius-growthTakeupRadius>.001f && conserved && feed.rpm>12f &&
                takeup.rpm/feed.rpm<growthRatio-.035f;
            bool requested=stage==2 && wound>.025f || stage==3 && growth ||
                stage==4 && load==2 && loadDrop && dutyRise || stage==5 && repaired && feed.forward>=12 && takeup.forward>=12;
            hold=mode==Mode.Auto && requested && Stable() ? hold+dt : 0f;
            if (hold>=2f) Award();
        }

        private float MaterialOn(float radius,float core) => Mathf.PI*(radius*radius-core*core)/materialThickness;

        private float Command(Channel c,float feedforward,float setpoint)
        {
            float error=setpoint-c.measured;
            float candidate=Mathf.Clamp(c.integral+speedI*error*.05f,-.28f,.60f);
            float output=feedforward/85.5f+speedP*error+candidate;
            if (output is >=0 and <=.98f || output>.98f && error<0 || output<0 && error>0)
                c.integral=candidate;
            return Mathf.Clamp(feedforward/85.5f+speedP*error+c.integral,0,.98f);
        }

        private void Actuate(Channel c,float duty)
        {
            c.duty=duty;
            duty=Mathf.Round(duty*100f)*.01f;
            SetSource(c.pwm,duty*5f);
            var solved=solver.LatestSolveResult;
            if (solved==null || !solved.TryGetVoltage(c.pwmIn,source.NegativeTerminal,out float volts))
            {
                Trip("PWM SIGNAL LOST");
                return;
            }
            float applied=Mathf.Clamp(volts/5f,0,.985f);
            if (applied<.008f)
            {
                if (c.enable.IsClosed) c.enable.SetState(SparkSwitchState.Open);
                c.applied=0;
                return;
            }
            if (Mathf.Abs(applied-c.applied)>.004f || c.enable.IsOpen)
            {
                c.impedance.SetResistance(c.motor.WindingResistance*(1f/applied-1f));
                c.applied=applied;
                if (c.enable.IsOpen) c.enable.SetState(SparkSwitchState.Closed);
            }
        }

        private void Integrate(Channel c,float brake,float dt)
        {
            float current=c.enable.IsClosed ? c.motor.ElectricalState.Current : 0f;
            float torque=.24f*Mathf.Max(0,current-.25f*c.omega/c.motor.WindingResistance);
            c.omega=Mathf.Max(0,c.omega+Mathf.Clamp((torque-.00215f*c.omega-brake)/inertia,-24f,18f)*dt);
            c.rpm=c.omega*ToRpm;
        }

        private void SampleEncoder(Channel c,float dt)
        {
            c.angle+=c.omega*dt;
            c.noEdge+=dt;
            while (c.angle>=Edge)
            {
                c.angle-=Edge;
                c.ticks=(c.ticks+1)%80;
                if (!ready || !ReadPhase(c,out int next)) continue;
                int delta=Delta[c.previous*4+next];
                if (Running && delta==0 && c.previous!=next) Trip("INVALID QUADRATURE TRANSITION");
                c.previous=next;
                c.count+=delta;
                if (delta>0) c.forward++;
                if (delta<0) c.reverse++;
                if (delta!=0) c.noEdge=0;
            }
        }

        private static void Measure(Channel c,float interval)
        {
            c.measured=Mathf.Lerp(c.measured,(c.count-c.sampled)*60f/(80f*interval),.68f);
            c.sampled=c.count;
        }

        private bool Unsafe(Channel c,float dt)
        {
            if (c.rpm>92f || c.motor.IsOverCurrent || source.IsOverloaded) { Trip("MOTOR OVERLOAD OR OVERSPEED"); return true; }
            if (c.rpm>12f && c.noEdge>.24f) { Trip("ENCODER FEEDBACK MISSING"); return true; }
            c.stall=c.duty>.34f && c.rpm<4f ? c.stall+dt : 0f;
            if (c.stall>1.15f) { Trip("MOTOR STALL · RELEASE BRAKE"); return true; }
            return false;
        }

        private bool Stable() => ready && calibrated && feed.rpm>15f && takeup.rpm>15f &&
            sensorVolts>=2.30f && sensorVolts<=2.70f && Mathf.Abs(sensorVolts-neutral)<=.20f &&
            Mathf.Abs(tension)<.48f;

        private static void SetSource(SparkPowerSupply supply,float volts)
        {
            if (Mathf.Abs(supply.OutputVoltage-volts)<.004f) return;
            supply.SetOutput(false,default);
            if (!supply.TryConfigure(volts,.025f,out var reason)) Debug.LogError(reason,supply);
            supply.SetOutput(true,default);
        }

        private bool ReadSupply()
        {
            if (solver.IsSolving || solver.IsDirty && !solver.SolveNow()) return false;
            var result=solver.LatestSolveResult;
            return result!=null && source.IsOutputActive && !source.IsOverloaded &&
                result.TryGetVoltage(pins[1],pins[3],out float v) && v>=4.8f && v<=5.2f;
        }

        private bool ReadSensor()
        {
            var result=solver.LatestSolveResult;
            return sensorConnected && result!=null && !solver.IsDirty &&
                result.TryGetVoltage(pins[24],pins[25],out float rail) && rail>=4.8f && rail<=5.2f &&
                result.TryGetVoltage(pins[27],pins[3],out sensorVolts) && sensorVolts>=.47f && sensorVolts<=4.53f &&
                Mathf.Abs(sensorVolts-sensor.OutputVoltage)<.08f;
        }

        private bool ReadPhase(Channel c,out int phase)
        {
            phase=Gray[c.ticks%4];

            return ready;
        }

        private string CheckWiring()
        {
            sensorConnected=false;
            circuit.CopyConnections(wires);
            links.Clear();
            SparkTerminal Root(SparkTerminal pin)
            {
                while (links.TryGetValue(pin,out var p)) pin=p;
                return pin;
            }
            foreach (var wire in wires)
            {
                if (wire.Kind!=SparkConnectionKind.Wire) continue;
                var a=Root(wire.A);
                var b=Root(wire.B);
                if (a!=b) links[b]=a;
            }
            sensorConnected=Root(pins[26])==Root(pins[27]);
            var nets=new HashSet<SparkTerminal>();
            for (int i=0;i<24;i+=2)
            {
                var root=Root(pins[i]);
                if (root!=Root(pins[i+1])) return $"Connect {pins[i].name} → {pins[i+1].name}.";
                if (!nets.Add(root)) return "Separate the wired signal and power nets.";
            }
            if (Root(pins[0])!=Root(pins[24])) return "Connect W13 to sensor +5 V.";
            if (Root(pins[2])!=Root(pins[25])) return "Connect W14 to sensor GND.";
            if (!sensorConnected) return "Connect W15: SENSOR SIGNAL → TENSION IN.";
            if (!nets.Add(Root(pins[26]))) return "Separate the sensor signal from the other nets.";
            return null;
        }

        private void Stop(Mode next)
        {
            mode=next;
            hold=0;
            foreach (Channel c in new[]{feed,takeup})
            {
                if (c.enable.IsClosed) c.enable.SetState(SparkSwitchState.Open);
                SetSource(c.pwm,0);
                c.duty=c.applied=0;
            }
        }

        private void Trip(string reason)
        {
            if (mode==Mode.Fault) return;
            fault=reason;
            sensorFault=stage==5 && reason=="SENSOR SIGNAL LOST";
            repaired=false;
            Stop(Mode.Fault);
            Debug.Log($"[LEVEL 47] SAFE STOP · {reason}",this);
        }

        private void Award()
        {
            if (!bindings.SetRuntimeComponentBinding(Gates[stage],source)) return;
            stage++;
            if (stage==3)
            {
                growthStart=wound;
                growthFeedRadius=feedRadius;
                growthTakeupRadius=takeupRadius;
                growthRatio=feedRadius/takeupRadius;
            }
            hold=0;
            manager.QueueEvaluation();
        }

        private string Prompt()
        {
            if (mode==Mode.Fault) return $"SAFE STOP · {fault}  |  "+(ready ? "Wait for both shafts to stop, then RESET FAULT." : wiring);
            if (!powered) return "Turn on MAIN POWER.";
            if (!ready) return wiring ?? "Waiting for solved sensor power and voltage.";
            return stage switch
            {
                1=>"Calibrate the centered dancer with both motors stopped.",
                2=>"Select AUTO and START. Hold the calibrated tension while winding.",
                3=>"Keep winding. Watch the FEED radius shrink and the TAKE-UP radius grow.",
                4=>"Apply TEST LOAD once tension is stable. Watch the dancer and motor duty recover.",
                5=>repaired ? "Select AUTO and START to verify recovery." : "While winding, undo W15 to test sensor-loss protection.",
                _=>"Stable winding and safety recovery verified."
            };
        }

        private void Present()
        {
            bool stable=mode==Mode.Auto && Stable();
            string status=mode switch {
                Mode.Fault=>"SAFE STOP", Mode.Manual=>"MANUAL", Mode.Auto when stable=>"WINDING / STABLE",
                Mode.Auto when load==2=>"DISTURBANCE RECOVERY", Mode.Auto=>"AUTO TENSION",
                _=>ready ? $"READY / {selected.ToString().ToUpperInvariant()}" : "OFF"
            };
            statusText.text=Prompt();
            string signal=ready ? $"{sensorVolts:F2} V" : "—";
            meterText.text=$"FEED {feed.measured:F1} RPM / {feedRadius*1000:F1} mm    |    TAKE-UP {takeup.measured:F1} RPM / {takeupRadius*1000:F1} mm    |    SENSOR {signal}";
            detailText.text=$"{status}   |   PWM {feed.duty:P0}/{takeup.duty:P0}   |   SET {neutral:F2} V   |   ERR {(ready ? (sensorVolts-neutral).ToString("+0.00;-0.00;0.00") : "—")}   |   DANCER {dancer*Mathf.Rad2Deg:+0.0;-0.0;0.0}°   |   {Loads[load]}   |   {manager.SatisfiedTargetCount}/6";
            feedDisplay.text=$"FEED {feed.measured:F1} RPM   •   TAKE-UP {takeup.measured:F1} RPM\n{status}   •   {wound:F2} m WOUND";
            takeupDisplay.text=$"{signal}\nDANCER {dancer*Mathf.Rad2Deg:+0.0;-0.0;0.0}°";
            tensionDisplay.text=$"{signal}  /  SET {neutral:F2} V\nPWM {feed.duty:P0} / {takeup.duty:P0}";
            rig.SetVisualState(feed.rpm,takeup.rpm,dancer/.40f,load/3f,wound/totalLength);
            rig.SetMaterialScales(feedRadius/initialFeed,takeupRadius/takeupCore);
            slatOrigin??=Array.ConvertAll(webSlats,slat=>slat.localPosition);
            for (int i=0;i<webSlats.Length;i++)
            {
                var p=slatOrigin[i];
                p.x=Mathf.Repeat(p.x+.42f+travel,.84f)-.42f;
                p.y-=Mathf.Clamp(-tension*.015f,-.015f,.035f)*(1f-Mathf.Abs(p.x)/.42f);
                webSlats[i].localPosition=p;
            }
            int f=Gray[feed.ticks%4], t=Gray[takeup.ticks%4];
            bool[] light={source.IsOutputActive,feed.enable.IsClosed,takeup.enable.IsClosed,mode==Mode.Manual,
                mode==Mode.Auto,mode==Mode.Fault,load>0,stable,tension<-.32f,mode==Mode.Fault,ready,
                Mathf.Abs(dancer)<.04f,(f&2)!=0,(f&1)!=0,(t&2)!=0,(t&1)!=0};
            for (int i=0;i<lamps.Length;i++) if (lamps[i].activeSelf!=light[i]) lamps[i].SetActive(light[i]);
            powerButton.interactable=true;
            calibrateButton.interactable=CanCalibrate;
            manualButton.interactable=autoButton.interactable=!Running && mode!=Mode.Fault;
            startButton.interactable=CanRun && !Running && (selected==Mode.Manual || calibrated);
            stopButton.interactable=Running;
            loadButton.interactable=true;
            loadButton.GetComponentInChildren<Text>().text=Loads[load];
            testButton.interactable=Running && load!=2;
            releaseButton.interactable=load!=0;
            resetButton.interactable=mode==Mode.Fault && ready && feed.rpm<=2 && takeup.rpm<=2 &&
                load!=3 && calibrated && Mathf.Abs(sensorVolts-neutral)<=.18f;
            restartButton.interactable=true;
        }

        private void OnDisable()
        {
            if (gameObject.activeInHierarchy && feed!=null && takeup!=null) Stop(Mode.Off);
            if (bindings!=null) foreach (string gate in Gates) bindings.ClearRuntimeComponentBinding(gate);
            if (rig!=null) rig.StopVisualMotion();
            bindings=null;
        }
    }
}
