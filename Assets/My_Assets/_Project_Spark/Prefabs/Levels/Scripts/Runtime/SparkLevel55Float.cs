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
    public sealed class SparkLevel55Float : MonoBehaviour
    {
        public static readonly string[] Gates = {"L55_Wired","L55_Float","L55_Reference","L55_Disturbances","L55_Certified","L55_Recovered"};
        public static readonly (int a,int b)[] Wires = {
            (0,1),(2,3),(0,4),(2,5),(0,6),(2,7),(0,8),(2,9),(0,10),(2,11),
            (12,13),(14,15),(16,17),(18,19),(20,21),(22,23),(24,25),(26,27),
            (28,29),(30,31),(32,33),(34,35),(36,37),(38,39),(40,41),(42,43),(44,45),(46,47)
        };

        private enum Run { Idle, Reference, High, Return, Coast }
        private const float Step=.001f, TorqueConstant=.12f, Inertia=.0019f, KpDefault=2350, KdDefault=7;
        public SparkLevelLayout layout;
        public SparkPowerSupply source;
        public SparkMotor motor;
        public SparkResistor driver, leftCoils, rightCoils;
        public SparkSwitch drive, leftDrive, rightDrive, safety;
        public SparkTerminal[] pins;
        public SparkLevel55VisualRig rig;
        public SparkLevel55Plot plot;
        public TMP_Text controllerDisplay, qualityDisplay;
        public Text statusText, meterText, detailText, calibrationText;
        public Button powerButton, guardButton, calibrateButton, enableButton, liftButton, centerButton, startButton, stopButton;
        public Button slowButton, fastButton, autoButton, manualButton, kpUpButton, kpDownButton, kdUpButton, kdDownButton;
        public Button leftButton, rightButton, recoveryButton, highButton, coastButton, phaseButton, resetButton, reliftButton, finalButton;

        private readonly SparkLevel55Shaft shaft=new();
        private readonly SparkLevel55Encoder encoder=new();
        private readonly SparkLevel55Sensor[] sensors={new(.00016f),new(-.00012f),new(-.00017f),new(.00014f)};
        private readonly float[] historyX=new float[180], historyY=new float[180];
        private int historyIndex, historyDecimation;
        private readonly List<SparkCircuitConnection> wires=new();
        private readonly Dictionary<SparkTerminal,SparkTerminal> nets=new();
        private readonly bool[] routed=new bool[Wires.Length];
        private readonly bool[] disturbancePassed=new bool[2];
        private readonly float[] peaks=new float[2], settling=new float[2];
        private SparkLevelLayoutLoader loader;
        private LevelGamePlayManager manager;
        private SparkLevelSceneBindings bindings;
        private SparkCircuitSystem circuit;
        private SparkElectricalSolver solver;
        private int topology=-1,stage,targetRpm=45,disturbance=-1;
        private float accumulator, omega, angle, setpoint, driveClock, motorCurrent, coilLeft, coilRight, liftFraction;
        private float kp=KpDefault,kd=KdDefault,stableTime,hold,runTime,disturbanceAge,disturbanceHold,saturationAge, encoderWatchdog, coilLossAge;
        private float squareSum, observationTime, runPeak, referenceRms, highRms, certifiedKp, certifiedKd;
        private bool powered=true, ready, safe, hubPower, guardOpen, enabledMagnets, floating, running, manual;
        private bool armed, seenFault, verifiedTouchdown, recovered, referenceAfterRepair, certified75;
        private string wiring,fault,notice;
        private Run run;

        private float Rpm => omega * 30/Mathf.PI;
        private bool Contact => routed[27] && shaft.Contact;
        private bool Stopped => !running && Rpm < .35f;
        private bool Zeroed => Array.TrueForAll(sensors,s=>s.Calibrated);
        private bool Feedback => Array.TrueForAll(sensors,s=>s.Valid);
        private float Excursion => Mathf.Max(shaft.Left.magnitude,shaft.Right.magnitude);
        private bool Centered => Excursion < .00030f && shaft.Tilt < .0017f &&
            Mathf.Max(shaft.LeftVelocity.magnitude,shaft.RightVelocity.magnitude) < .004f && !shaft.Contact;
        private bool Supported => floating && Centered && Feedback && Zeroed && coilLeft > .04f && coilRight > .04f;
        private bool AtSpeed => running && encoder.Locked && Mathf.Abs(Rpm-targetRpm)<1.0f && Mathf.Abs(encoder.Rpm-targetRpm)<2f;
        private bool Ready45 => AtSpeed && targetRpm==45 && Supported;
        private bool CanReset => fault!=null && ready && safe && Stopped && Contact && (!seenFault || verifiedTouchdown);
        private bool CanCalibrate => ready && safe && Stopped && Contact && !floating && fault==null && Feedback && !Zeroed;
        private bool CanLift => ready && safe && Zeroed && enabledMagnets && !floating && Stopped && Contact && fault==null;
        private bool CanStart => ready && safe && Stopped && Supported && stableTime>=.8f && fault==null;
        private bool CanTune => stage==3 && Ready45 && disturbance<0 && fault==null;
        private bool CanTest => stage==3 && Ready45 && disturbance<0 && run==Run.Idle && fault==null;
        private bool CanHigh => Ready45 && run==Run.Idle && (stage==4 || stage==5 && referenceAfterRepair) &&
            Mathf.Approximately(kp,certifiedKp) && Mathf.Approximately(kd,certifiedKd);

        private void Start()
        {
            loader=FindFirstObjectByType<SparkLevelLayoutLoader>();
            manager=FindFirstObjectByType<LevelGamePlayManager>();
            if (loader && manager && pins!=null && pins.Length==48) return;
            Debug.LogError("[LEVEL 55] Missing layout manager or 48 socket references.",this);
            enabled=false;
        }

        private void Update()
        {
            if (loader.ActiveLayout!=layout || loader.ActiveBindings==null) return;
            solver=manager.ElectricalSolver;
            circuit=manager.CircuitSystem;
            if (!solver || !circuit) return;
            if (loader.ActiveBindings!=bindings) { bindings=loader.ActiveBindings; topology=-1; }
            if (topology!=circuit.TopologyVersion) { topology=circuit.TopologyVersion; wiring=InspectWiring(); }
            ReadElectrical();
            if (stage==0 && ready && safe) Award();
            if ((floating || running) && (!ready || !safe))
                Trip(!routed[12] ? "RIGHT X FEEDBACK LOST · W13" : !safe ? "GUARD INTERLOCK OPEN" :
                    source.IsOverloaded ? "SUPPLY OVERLOAD" : wiring ?? "INVALID SOLVED RAIL");
            accumulator+=Mathf.Min(Time.deltaTime,.10f);
            while (accumulator>=Step) { accumulator-=Step; Simulate(); }
            Present();
        }

        private void ReadElectrical()
        {
            var result=solver.LatestSolveResult;
            bool Rail(int a,int b)=>result!=null && result.TryGetVoltage(pins[a],pins[b],out float volts) && volts is >=4.8f and <=5.2f;
            hubPower=Rail(10,11);
            ready=powered && wiring==null && hubPower && Rail(1,3) && Rail(4,5) && Rail(6,7) && Rail(8,9) &&
                source.IsOutputActive && !source.IsOverloaded;
            safe=!guardOpen && safety.IsClosed && routed[26] && Rail(45,3);
            motorCurrent=Mathf.Abs(motor.ElectricalState.Current);
                        // The solved terminal voltage and installed load give the branch current
            // even during the first solve after closing a driver relay.
            float leftReading=Current(leftCoils,37,39);
            float rightReading=Current(rightCoils,41,43);
            // The averaged drive holds the previous sample across brief solver
            // transitions, but opens immediately on an actual topology fault.
            if (!floating || !leftDrive.IsClosed || leftReading >= .04f) coilLeft=leftReading;
            if (!floating || !rightDrive.IsClosed || rightReading >= .04f) coilRight=rightReading;
            coilLossAge = floating && (leftReading < .04f || rightReading < .04f)
                ? coilLossAge + Mathf.Min(Time.deltaTime,.05f) : 0;
            float Current(SparkResistor load,int a,int b) => result != null &&
                result.TryGetVoltage(pins[a],pins[b],out float voltage) ?
                Mathf.Abs(voltage)/load.EffectiveResistance : 0;
        }

        public void Power()
        {
            powered=!powered;
            if (!powered && (floating || running)) Trip("MAIN POWER LOST");
            source.SetOutput(powered,default);
        }
        public void Guard()
        {
            if (!Stopped || floating) return;
            guardOpen=!guardOpen;
            SetContact(safety,!guardOpen);
        }
        public void Calibrate()
        {
            if (!CanCalibrate) return;
            sensors[0].Calibrate(shaft.Left.x,0);
            sensors[1].Calibrate(shaft.Left.y,-SparkLevel55Shaft.Clearance);
            sensors[2].Calibrate(shaft.Right.x,0);
            sensors[3].Calibrate(shaft.Right.y,-SparkLevel55Shaft.Clearance);
            notice=null;
        }
        public void EnableMagnets()
        {
            if (ready && safe && Zeroed && Stopped && fault==null) enabledMagnets=true;
        }
        public void Lift()
        {
            if (!CanLift) return;
            floating=true;
            stableTime=0;
            liftFraction=0;
            SetContact(leftDrive,true);
            SetContact(rightDrive,true);
            solver.SolveNow();
            ReadElectrical();
        }
        public void Center()
        {
            if (floating && !running && fault==null) notice="Holding closed-loop centering at zero radial offset.";
        }
        public void StartMotor()
        {
            if (!CanStart) return;
            running=true;
            targetRpm=45;
            setpoint=0;
            encoder.Reacquire(angle);
            encoderWatchdog=0;
            run=stage==2 || recovered && !referenceAfterRepair ? Run.Reference : Run.Idle;
            BeginObservation();
            SetContact(drive,true);
        }
        public void StopMotor()
        {
            if (!running) return;
            targetRpm=0;
            runTime=0;
            run=Run.Coast;
            hold=0;
            disturbance=-1;
        }
        public void Coast() => StopMotor();
        public void Speed45()
        {
            if (!running || armed || stage<5 || run!=Run.Idle) return;
            targetRpm=45;
        }
        public void Speed75()
        {
            if (stage==6 && run==Run.Idle && running && Supported) targetRpm=75;
        }

        public void Auto() { if (CanTune) Tune(KpDefault,KdDefault,false); }
        public void Manual() { if (CanTune) manual=true; }
        public void MoreKp() { if (manual && CanTune) Tune(kp+225,kd,true); }
        public void LessKp() { if (manual && CanTune) Tune(kp-225,kd,true); }
        public void MoreKd() { if (manual && CanTune) Tune(kp,kd+1,true); }
        public void LessKd() { if (manual && CanTune) Tune(kp,kd-1,true); }
        private void Tune(float stiffness,float damping,bool manualMode)
        {
            kp=Mathf.Clamp(stiffness,1000,3250);
            kd=Mathf.Clamp(damping,4,11);
            manual=manualMode;
            Array.Clear(disturbancePassed,0,2);
            Array.Clear(peaks,0,2);
            Array.Clear(settling,0,2);
            notice=null;
        }
        public void LeftTest() => StartDisturbance(0);
        public void RightTest() => StartDisturbance(1);
        private void StartDisturbance(int side)
        {
            if (!CanTest) return;
            disturbance=side;
            disturbancePassed[side]=false;
            peaks[side]=settling[side]=disturbanceAge=disturbanceHold=0;
            notice=null;
        }
        public void VerifyRecovery()
        {
            if (!CanTest || !disturbancePassed[0] || !disturbancePassed[1]) return;
            certifiedKp=kp;
            certifiedKd=kd;
            Award();
        }
        public void HighTest()
        {
            if (!CanHigh) return;
            run=Run.High;
            targetRpm=75;
            BeginObservation();
        }
        public void FaultTest()
        {
            if (stage!=5 || !Ready45 || recovered || fault!=null || run!=Run.Idle) return;
            armed=true;
        }
        public void ResetFault()
        {
            if (!CanReset) return;
            fault=null;
            recovered |= seenFault && verifiedTouchdown;
            armed=false;
            notice=null;
            encoder.Reacquire(angle);
            encoderWatchdog=0;
        }
        public void Relift() => Lift();
        public void FinalCertification() { if (recovered && stage==5) HighTest(); }

        private void HardStop()
        {
            running=floating=enabledMagnets=false;
            liftFraction=0;
            stableTime=0;
            coilLossAge=0;
            targetRpm=45;
            run=Run.Idle;
            disturbance=-1;
            SetContact(drive,false);
            SetContact(leftDrive,false);
            SetContact(rightDrive,false);
            encoder.Reacquire(angle);
            encoderWatchdog=0;
        }

        private void Trip(string message)
        {
            if (fault!=null) return;
            seenFault |= stage==5 && armed && running && !routed[12];
            verifiedTouchdown=false;
            referenceAfterRepair=false;
            fault=message;
            HardStop();
            foreach (var sensor in sensors) sensor.Invalidate();
        }

        private void Simulate()
        {
            float drag=.0103f+.00043f*omega;
            setpoint=Mathf.MoveTowards(setpoint,targetRpm,Step*(run==Run.Coast ? 17 : 9));
            driveClock+=Step;
            if (running && driveClock>=.12f)
            {
                driveClock=0;
                float desired=Mathf.Clamp((drag+.0058f*(setpoint*Mathf.PI/30-omega))/TorqueConstant,.007f,.197f);
                float resistance=5/desired-22.03f;
                if (Mathf.Abs(driver.ResistanceOhms-resistance)>.10f) driver.SetResistance(resistance);
            }

            omega=Mathf.Max(0,omega+((running?motorCurrent*TorqueConstant:0)-drag-(shaft.Contact ? .0025f*omega:0))*Step/Inertia);
            angle+=omega*Step;
            bool transition=encoder.Step(Step,angle,routed[14],routed[15],routed[16]);
            encoderWatchdog = running && Rpm > 36 && !encoder.Index ? encoderWatchdog + Step : 0;
            sensors[0].Sample(Step,shaft.Left.x,hubPower&&routed[10]);
            sensors[1].Sample(Step,shaft.Left.y,hubPower&&routed[11]);
            sensors[2].Sample(Step,shaft.Right.x,hubPower&&routed[12]);
            sensors[3].Sample(Step,shaft.Right.y,hubPower&&routed[13]);
            bool feedbackActive=floating && Feedback && Zeroed;
            liftFraction=feedbackActive?Mathf.MoveTowards(liftFraction,1,Step*2.2f):0;
            Vector2 Demand(int i) => new(
                -kp*sensors[i].Position-kd*sensors[i].Velocity,
                SparkLevel55Shaft.Mass*SparkLevel55Shaft.Gravity/2-kp*sensors[i+1].Position-kd*sensors[i+1].Velocity);
            Vector2 push=disturbance>=0 && disturbanceAge<.055f ? new Vector2(.74f,-.10f) : Vector2.zero;
            shaft.Step(Step,angle,omega,feedbackActive?Demand(0):Vector2.zero,feedbackActive?Demand(2):Vector2.zero,
                feedbackActive?coilLeft:0,feedbackActive?coilRight:0,liftFraction,
                disturbance==0?push:Vector2.zero,disturbance==1?-push:Vector2.zero);

            if (++historyDecimation==10)
            {
                historyDecimation=0;
                historyX[historyIndex]=shaft.Left.x*1000;
                historyY[historyIndex]=shaft.Left.y*1000;
                historyIndex=(historyIndex+1)%historyX.Length;
            }
            if (fault!=null && seenFault && Stopped && Contact) verifiedTouchdown=true;
            if (!floating) return;
            saturationAge=shaft.Limited?saturationAge+Step:Mathf.Max(0,saturationAge-Step*3);
            if (!float.IsFinite(Excursion) || !Feedback || motor.IsOverCurrent || Rpm>82 ||
                coilLeft>.14f || coilRight>.14f || saturationAge>.65f)
                Trip("INVALID FEEDBACK, MOTOR OR COIL LIMIT");
            else if (liftFraction>=1 && (coilLossAge>.12f || coilLeft<.04f || coilRight<.04f))
                Trip(leftCoils.IsBurned || rightCoils.IsBurned ? "MAGNETIC WINDING THERMAL TRIP" : "MAGNETIC DRIVER CURRENT LOST");
            else if (running && (!transition || encoderWatchdog>2.1f))
                Trip(!transition?"ENCODER A/B TRANSITION INVALID":$"ENCODER Z TIMEOUT · {encoderWatchdog:F2}s");
            else if (running && (Contact || shaft.Tilt>.0045f))
                Trip("UNSAFE SHAFT POSITION · TOUCHDOWN");
            if (!floating) return;

            stableTime=Supported?stableTime+Step:0;
            if (stage==1 && stableTime>=1.35f) Award();
            if (disturbance>=0) ObserveDisturbance();
            ObserveRun();
        }

        private void ObserveDisturbance()
        {
            disturbanceAge+=Step;
            peaks[disturbance]=Mathf.Max(peaks[disturbance],Excursion*1000);
            if (disturbanceAge>.12f)
                disturbanceHold=Centered?disturbanceHold+Step:0;
            if (disturbanceHold>=.40f)
            {
                settling[disturbance]=disturbanceAge-.40f;
                disturbancePassed[disturbance]=peaks[disturbance]>.19f;
                notice=disturbancePassed[disturbance]?null:"Disturbance too small to characterize. Repeat the test.";
                disturbance=-1;
            }
            else if (disturbanceAge>3)
            {
                notice="Recovery exceeded 3 seconds. Tune the gains and repeat.";
                disturbance=-1;
            }
        }

        private void BeginObservation()
        {
            hold=runTime=squareSum=observationTime=runPeak=0;
        }
        private void ObserveRun()
        {
            if (run==Run.Idle) return;
            runTime+=Step;
            if (runTime>30) { Trip("SPEED CONTROL TIMEOUT"); return; }
            if (run==Run.Coast)
            {
                if (Rpm<.45f && setpoint<=.1f)
                {
                    running=false;
                    SetContact(drive,false);
                    run=Run.Idle;
                    targetRpm=45;
                }
                return;
            }

            bool valid=AtSpeed && Supported;
            if (!valid) { hold=squareSum=observationTime=0; return; }
            hold+=Step;
            squareSum+=(shaft.Left.sqrMagnitude+shaft.Right.sqrMagnitude)*.5f*Step;
            observationTime+=Step;
            runPeak=Mathf.Max(runPeak,Excursion*1000);
            if (run==Run.Reference && hold>=4)
            {
                referenceRms=Mathf.Sqrt(squareSum/observationTime)*1000;
                run=Run.Idle;
                if (stage==2) Award();
                if (recovered) referenceAfterRepair=true;
            }
            else if (run==Run.High && hold>=3)
            {
                highRms=Mathf.Sqrt(squareSum/observationTime)*1000;
                certified75=true;
                run=Run.Return;
                targetRpm=45;
                BeginObservation();
            }
            else if (run==Run.Return && hold>=1.5f && certified75)
            {
                certified75=false;
                run=Run.Idle;
                if (stage==4 || stage==5 && recovered && referenceAfterRepair && verifiedTouchdown) Award();
            }
        }

        private void Award()
        {
            if (stage>=Gates.Length || !bindings.SetRuntimeComponentBinding(Gates[stage],source)) return;
            stage++;
            notice=null;
            manager.QueueEvaluation();
        }

        private string Prompt()
        {
            if (fault!=null) return $"SAFE STOP · {fault} · " + (CanReset?"RESET FAULT":wiring??"Wait for stopped touchdown.");
            if (wiring!=null) return wiring;
            if (!safe) return "Close the guard and check W27.";
            if (!ready) return "Check the solved 5 V rails and returns.";
            if (stage==6) return "CERTIFIED · Contact-free operation verified after W13 recovery.";
            if (!Zeroed) return "At rest on touchdown bearings, select CALIBRATE ALL.";
            if (!enabledMagnets) return "Select ENABLE MAGNETS.";
            if (!floating) return recovered?"Select RELIFT.":"Select LIFT SHAFT.";
            if (armed) return "Undo W13 now. Wait for contact and zero speed before resetting.";
            if (run==Run.High) return "75 RPM CONTACT-FREE TEST · Observe both bearing gaps.";
            if (run==Run.Return) return "Controlled deceleration to the 45 RPM reference.";
            if (run==Run.Coast) return "Controlled motor stop. The bearings continue holding the shaft.";
            if (disturbance>=0) return $"Recording {(disturbance==0?"left":"right")} disturbance and contact-free recovery.";
            if (notice!=null) return notice;
            return stage switch {
                1=>"Lifting and centering. Wait for both journals to clear the touchdown bearings.",
                2=>"Select START MOTOR and hold the 45 RPM reference.",
                3=>running?"Test LEFT and RIGHT disturbances at 45 RPM, then VERIFY RECOVERY.":"Start the motor for disturbance tests.",
                4=>running?"Select HIGH-SPEED TEST.":"Start the motor, then run the high-speed test.",
                5 when !recovered=>"Select SENSOR FAULT TEST while stable at 45 RPM.",
                5 when !running=>"Start the motor to record a fresh 45 RPM reference.",
                5 when !referenceAfterRepair=>"Recording a fresh contact-free 45 RPM reference.",
                5=>"Select FINAL CERTIFICATION.",
                _=>"Waiting for the electrical solver."
            };
        }

        private void Present()
        {
            statusText.text=Prompt();
            string L(int i)=>sensors[i].Valid && Zeroed ? $"{sensors[i].Position*1000:+0.00;-0.00;0.00}" : "—";
            meterText.text=$"{Rpm:F1}/{targetRpm} RPM  ·  XL {L(0)}   YL {L(1)}   XR {L(2)}   YR {L(3)} mm  ·  TILT {shaft.Tilt*1000:F2} mrad";
            detailText.text=$"COILS L/R {coilLeft*1000:F0}/{coilRight*1000:F0} mA bus  ·  MOTOR {motorCurrent*1000:F0} mA  ·  {(Contact?"TOUCHDOWN":routed[27]?"CONTACT FREE":"TD UNKNOWN")}  ·  Z {(encoder.Locked?"LOCK":"—")}  ·  {manager.SatisfiedTargetCount}/6";
            calibrationText.text=$"KP {kp:F0}  KD {kd:F0}  ·  LEFT {peaks[0]:F2} mm / {settling[0]:F2}s    RIGHT {peaks[1]:F2} mm / {settling[1]:F2}s  ·  RMS 45/75 {referenceRms:F3}/{highRms:F3} mm";
            if (controllerDisplay) controllerDisplay.text=$"{Rpm:F1} RPM · {(fault!=null?"SAFE STOP":Contact?"TOUCHDOWN":floating?"LEVITATING":"READY")}\nL {shaft.LeftForce.magnitude:F2} N · R {shaft.RightForce.magnitude:F2} N";
            if (qualityDisplay) qualityDisplay.text=$"L {L(0)} / {L(1)}  ·  R {L(2)} / {L(3)} mm\nCLEARANCE {(SparkLevel55Shaft.Clearance-Excursion)*1000:F2} mm";
            rig.ApplyState(angle*Mathf.Rad2Deg,shaft.Left.x*1000,shaft.Left.y*1000,shaft.Right.x*1000,shaft.Right.y*1000,
                guardOpen?1:0,disturbance>=0&&disturbanceAge<.055f?1:0);
            rig.ShowStatus(coilLeft,coilRight,fault!=null);
            if (plot) plot.Show(historyX,historyY,historyIndex,Feedback&&Zeroed);

            powerButton.interactable=true;
            guardButton.interactable=Stopped&&!floating;
            guardButton.GetComponentInChildren<Text>().text=guardOpen?"CLOSE GUARD":"OPEN GUARD";
            calibrateButton.interactable=CanCalibrate;
            enableButton.interactable=ready&&safe&&Zeroed&&Stopped&&!enabledMagnets&&fault==null;
            liftButton.interactable=CanLift&&!recovered;
            reliftButton.interactable=CanLift&&recovered;
            centerButton.interactable=floating&&Stopped&&fault==null;
            startButton.interactable=CanStart;
            stopButton.interactable=coastButton.interactable=running&&run!=Run.Coast;
            slowButton.interactable=running&&targetRpm!=45&&stage>=5&&run==Run.Idle&&!armed;
            fastButton.interactable=running&&targetRpm!=75&&stage==6&&run==Run.Idle;
            autoButton.interactable=CanTune&&manual;
            manualButton.interactable=CanTune&&!manual;
            kpUpButton.interactable=kpDownButton.interactable=kdUpButton.interactable=kdDownButton.interactable=CanTune&&manual;
            leftButton.interactable=rightButton.interactable=CanTest;
            recoveryButton.interactable=CanTest&&disturbancePassed[0]&&disturbancePassed[1];
            highButton.interactable=CanHigh&&stage==4;
            phaseButton.interactable=Ready45&&stage==5&&!armed&&!recovered&&run==Run.Idle;
            resetButton.interactable=CanReset;
            finalButton.interactable=CanHigh&&stage==5&&recovered;
        }

        private void OnDisable()
        {
            if (drive && leftDrive && rightDrive && gameObject.activeInHierarchy) HardStop();
            if (bindings!=null)
                foreach (string gate in Gates) bindings.ClearRuntimeComponentBinding(gate);
            bindings=null;
        }
        private SparkTerminal Net(SparkTerminal pin)
        {
            while (nets.TryGetValue(pin, out var next)) pin = next;
            return pin;
        }

        private string InspectWiring()
        {
            circuit.CopyConnections(wires);
            nets.Clear();
            Array.Clear(routed, 0, routed.Length);
            foreach (var wire in wires)
            {
                if (wire.Kind != SparkConnectionKind.Wire) continue;
                var a = Net(wire.A);
                var b = Net(wire.B);
                if (a != b) nets[b] = a;
            }

            string issue = null;
            for (int i = 0; i < Wires.Length; i++)
            {
                var (a, b) = Wires[i];
                routed[i] = wires.Exists(wire => wire.Kind == SparkConnectionKind.Wire &&
                    (wire.A == pins[a] && wire.B == pins[b] || wire.A == pins[b] && wire.B == pins[a]));
                if (!routed[i] && issue == null)
                    issue = $"Connect W{i + 1:00} · {pins[a].name} → {pins[b].name}.";
            }

            var distinct = new HashSet<SparkTerminal> {Net(pins[0]), Net(pins[2])};
            if (distinct.Count != 2) return "Separate +5 V from return.";
            if (issue != null) return issue;
            for (int i = 10; i < Wires.Length; i++)
                if (!distinct.Add(Net(pins[Wires[i].a]))) return "Separate the signal and drive nets.";

            int localWires = wires.FindAll(w => w.Kind == SparkConnectionKind.Wire &&
                (Array.IndexOf(pins, w.A) >= 0 || Array.IndexOf(pins, w.B) >= 0)).Count;
            return localWires == Wires.Length ? null : "Remove extra wires from the Level 55 sockets.";
        }

        private static void SetContact(SparkSwitch contact, bool closed)
        {
            if (contact.IsClosed != closed) contact.SetState(closed ? SparkSwitchState.Closed : SparkSwitchState.Open);
        }

    }
}






