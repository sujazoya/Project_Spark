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
    public sealed class SparkLevel56Harvest : MonoBehaviour
    {
        public static readonly string[] Gates={"L56_Wired","L56_Kinetic","L56_Regenerated","L56_Optimized","L56_Reused","L56_Recovered"};
        public static readonly (int a,int b)[] Wires = {(0,1),(2,3),(0,4),(2,5),(0,6),(2,7),(0,8),(2,9),(10,11),(5,12),(13,14),(15,16),(17,18),(19,20),(4,21),(22,23),(24,25),(2,26),(27,28),(26,29),(30,31),(32,33),(34,35),(36,37),(9,38),(39,40),(41,42),(43,44),(25,45),(46,47)};
        private enum Mode { Idle, Spin, Reuse, Regen, Brake, Discharge, Fault }
        public SparkLevelLayout layout;
        public SparkPowerSupply source;
        public SparkResistor driver,brake,capLoad;
        public SparkSwitch drive,brakeDrive,protection,safety;
        public SparkTerminal[] pins;
        public SparkLevel56VisualRig rig;
        public SparkLevel56Plot plot;
        public TMP_Text controllerDisplay,qualityDisplay;
        public Text statusText,meterText,detailText,calibrationText;
        public Button powerButton,guardButton,calibrateButton,startButton,stopButton,slowButton,midButton,fastButton,motoringButton;
        public Button regenButton,reuseButton,resistiveButton,recoveryButton,optimizeButton,verifyButton,highButton,lowButton;
        public Button reuseTestButton,protectButton,phaseButton,resetButton,finalButton;

        private readonly SparkLevel56Energy energy=new();
        private readonly SparkLevel56Encoder encoder=new();
        private readonly List<SparkCircuitConnection> wires=new();
        private readonly Dictionary<SparkTerminal,SparkTerminal> nets=new();
        private readonly HashSet<(int,int)> verified=new();
        private readonly float[] traceA=new float[180],traceB=new float[180];
        private int traceCursor;
        private float traceClock, accumulator, activeTime, hold, baselineSupply, spinSupply, initialStored, startKinetic;
        private float initialLoss, initialHeat, initialDump, initialGenerator, regenEfficiency, baselineEfficiency, regenPower, peakTemp;
        private float brakeCurrent=.48f, targetRpm=90, rpmStart, rail, indexAge;
        private int stage,topology=-1;
        private bool ready,safe,powered=true,guardOpen,calibrated,armed,sawFault,verifiedSafe,repaired,compared,optimizedRun,protectTest;
        private string wiring,fault,notice;
        private SparkLevelLayoutLoader loader;
        private LevelGamePlayManager manager;
        private SparkLevelSceneBindings bindings;
        private SparkElectricalSolver solver;
        private SparkCircuitSystem circuit;
        private Mode mode;
        private bool Idle => mode==Mode.Idle;
        private bool Stopped => energy.Rpm<.35f;
        private bool At90 => Mathf.Abs(energy.Rpm-90)<.65f && encoder.Locked && Mathf.Abs(encoder.Rpm-90)<1.6f;
        private bool CanSpin => ready && safe && calibrated && mode==Mode.Idle && (Stopped || energy.Rpm<=20.4f);
        private bool CanRecover => ready && safe && calibrated && Idle && At90 && !armed;
        private bool CapRoute => Routed(15)&&Routed(16)&&Routed(17)&&Routed(18)&&Routed(19);
        private bool BrakeRoute => Routed(22)&&Routed(23)&&Routed(24)&&Routed(6)&&Routed(7);
        private bool Routed(int i)
        {
            var (a,b)=Wires[i];
            return verified.Contains((Mathf.Min(a,b),Mathf.Max(a,b)));
        }

        private void Start()
        {
            loader=FindFirstObjectByType<SparkLevelLayoutLoader>();
            manager=FindFirstObjectByType<LevelGamePlayManager>();
            if (loader && manager && pins?.Length==48) return;
            Debug.LogError("[LEVEL 56] Missing campaign manager, layout loader or wiring references.",this);
            enabled=false;
        }

        private void Update()
        {
            if (loader.ActiveLayout!=layout || loader.ActiveBindings==null) return;
            solver=manager.ElectricalSolver;
            circuit=manager.CircuitSystem;
            if (!solver || !circuit) return;
            if (bindings!=loader.ActiveBindings) {bindings=loader.ActiveBindings; topology=-1;}
            if (topology!=circuit.TopologyVersion) {topology=circuit.TopologyVersion; wiring=InspectWiring();}
            ReadPower();
            if (stage==0 && ready && safe) Award();
            if (mode!=Mode.Fault && (!Idle || energy.Rpm>2) && (!ready || !safe))
                Trip(!Routed(15)?"CAPACITOR VOLTAGE FEEDBACK LOST · W16":!safe?"GUARD INTERLOCK OPEN":wiring??"POWER OR OVERLOAD");
            accumulator+=Mathf.Min(Time.deltaTime,.10f);
            while (accumulator>=.01f) {accumulator-=.01f; Step(.01f);}
            Present();
        }

        private SparkTerminal Net(SparkTerminal pin)
        {
            while (nets.TryGetValue(pin,out var next)) pin=next;
            return pin;
        }
        private string InspectWiring()
        {
            circuit.CopyConnections(wires);
            nets.Clear();
            verified.Clear();
            foreach (var wire in wires)
            {
                if (wire.Kind!=SparkConnectionKind.Wire) continue;
                var a=Net(wire.A);
                var b=Net(wire.B);
                if (a!=b) nets[b]=a;
                int i=Array.IndexOf(pins,wire.A),j=Array.IndexOf(pins,wire.B);
                if (i>=0 || j>=0) verified.Add((Mathf.Min(i,j),Mathf.Max(i,j)));
            }
            if (Net(pins[0])==Net(pins[2])) return "Separate the supply and return nets.";
            for(int i=0;i<Wires.Length;i++)
            {
                var (a,b)=Wires[i];
                if (!Routed(i)) return $"Connect W{i+1:00} · {pins[a].name} → {pins[b].name}.";
            }
            return verified.Count==Wires.Length?null:"Remove extra wires from the Level 56 sockets.";
        }
        private void ReadPower()
        {
            var result=solver.LatestSolveResult;
            float Volts(int a,int b)=>result!=null&&result.TryGetVoltage(pins[a],pins[b],out var volts)?volts:0;
            rail=Volts(4,5);
            bool Normal(float v)=>v is >=4.75f and <=5.25f;
            ready=powered&&wiring==null&&source.IsOutputActive&&!source.IsOverloaded&&Normal(rail)&&
                Normal(Volts(1,3))&&Normal(Volts(6,7))&&Normal(Volts(8,9));
            var (_,safePin)=Wires[25];
            safe=!guardOpen&&safety.IsClosed&&Routed(25)&&Normal(Volts(safePin,3));
        }

        private static void SetContact(SparkSwitch contact,bool value)
        {
            if (contact.IsClosed!=value) contact.SetState(value?SparkSwitchState.Closed:SparkSwitchState.Open);
        }
        public void Power()
        {
            powered=!powered;
            if (!powered && (energy.Rpm>2 || !Idle)) Trip("MAIN POWER REMOVED");
            source.SetOutput(powered,default);
        }
        public void Guard()
        {
            if (!Stopped || !Idle) return;
            guardOpen=!guardOpen;
            SetContact(safety,!guardOpen);
        }
        public void Calibrate()
        {
            if (!ready || !safe || !Stopped || !Idle || !CapRoute) return;
            calibrated=true;
            notice="Current zeroed at rest. Bus and capacitor channels ready.";
        }
        public void Speed45() => SelectSpeed(45);
        public void Speed60() => SelectSpeed(60);
        public void Speed90() => SelectSpeed(90);
        private void SelectSpeed(int value)
        {
            if (stage!=6 || !Idle || armed) return;
            targetRpm=value;
        }
        public void StartMotor()
        {
            if (!CanSpin || stage==4 || stage==3 && energy.TerminalVolts>.22f) return;
            if (stage<6) targetRpm=90;
            rpmStart=energy.Rpm;
            spinSupply=energy.SourceWork;
            initialStored=energy.Stored;
            encoder.Reacquire(energy.Angle);
            indexAge=0;
            Change(Mode.Spin);
        }
        public void Motor() => StartMotor();
        public void Stop()
        {
            if (mode==Mode.Fault || Stopped) return;
            Change(Mode.Brake);
        }
        public void Resistive()
        {
            if (Idle && Stopped && ready && safe && BrakeRoute && energy.Stored>.01f) Change(Mode.Discharge);
            else Stop();
        }
        public void Low() {if ((Idle || mode==Mode.Discharge) && stage==3) {brakeCurrent=.20f;compared=false;}}
        public void High() {if ((Idle || mode==Mode.Discharge) && stage==3) {brakeCurrent=.48f;compared=false;}}
        public void Optimize()
        {
            if (stage==3 && Idle && !armed && energy.Stored>.014f && BrakeRoute) Change(Mode.Discharge);
        }
        public void BeginRecovery()
        {
            if (!CanRecover || !CapRoute || stage<2 || stage>3) return;
            StartRecovery(false);
        }
        public void Regen() => BeginRecovery();
        private void StartRecovery(bool certification)
        {
            initialStored=energy.Stored;
            startKinetic=energy.Kinetic;
            initialLoss=energy.Loss;
            initialHeat=energy.Heat;
            initialDump=energy.Dumped;
            initialGenerator=energy.GeneratorWork;
            optimizedRun=stage==3 || certification;
            regenPower=0;
            peakTemp=energy.Temperature;
            Change(Mode.Regen);
        }
        public void Verify()
        {
            if (!Idle || stage!=3 || !compared) return;
            if (regenEfficiency>=.60f && Mathf.Abs(EnergyError)<.035f) Award();
            else {compared=false; notice=$"Measured {regenEfficiency:P1}, balance error {EnergyError:F3} J. Clear the bank and repeat at a different current.";}
        }
        public void ReuseTest()
        {
            if (stage!=4 || !Idle || !ready || !safe || energy.Rpm is <19.5f or >20.4f ||
                energy.Stored<.5f || !Routed(26) || !Routed(29) || baselineSupply<=0) return;
            initialStored=energy.Stored;
            spinSupply=energy.SourceWork;
            rpmStart=energy.Rpm;
            Change(Mode.Reuse);
        }
        public void Reuse() => ReuseTest();
        public void ProtectionTest()
        {
            if (!Idle || !ready || !safe || energy.Rpm<20) return;
            protectTest=true;
            Change(Mode.Brake);
        }
        public void FaultTest()
        {
            if (stage==5 && Idle && At90 && !repaired) armed=true;
        }
        public void ResetFault()
        {
            if (mode!=Mode.Fault || !ready || !safe || !Stopped || !CapRoute) return;
            repaired |= sawFault && verifiedSafe;
            calibrated=false;
            fault=null;
            armed=false;
            Change(Mode.Idle);
        }
        public void FinalCertification()
        {
            if (stage==5 && repaired && CanRecover && CapRoute) StartRecovery(true);
        }

        private void Change(Mode next)
        {
            mode=next;
            activeTime=hold=0;
            notice=null;
        }
        private void Trip(string reason)
        {
            if (mode==Mode.Fault) return;
            fault=reason;
            sawFault |= stage==5&&armed&&!Routed(15)&&energy.Rpm>30;
            verifiedSafe=false;
            calibrated=false;
            protectTest=false;
            Change(Mode.Fault);
            SetContact(drive,false);
            SetContact(protection,false);
            SetContact(brakeDrive,safe&&BrakeRoute);
        }

        private float EnergyError {get;set;}
        private float reuseSaved;
        private void Step(float dt)
        {
            activeTime+=dt;
            bool motor=mode is Mode.Spin or Mode.Reuse;
            bool maintain=ready&&safe && (Idle || mode==Mode.Discharge) &&
                (energy.Rpm is >19.5f and <20.6f || energy.Rpm>targetRpm-.8f);
            float holdTarget=energy.Rpm<21?20:targetRpm;
            bool regen=mode==Mode.Regen && CapRoute && ready && safe && calibrated;
            bool dump=BrakeRoute && safe && (mode is Mode.Regen or Mode.Brake or Mode.Fault or Mode.Discharge);
            float current=motor?Mathf.Clamp((.008f+.0015f*energy.Omega)/SparkLevel56Energy.Ke+.22f*(targetRpm*Mathf.PI/30-energy.Omega),0,.53f):
                maintain?Mathf.Clamp((.008f+.0015f*energy.Omega)/SparkLevel56Energy.Ke+.22f*(holdTarget*Mathf.PI/30-energy.Omega),0,.10f):
                regen?brakeCurrent:.43f;
            energy.Step(dt,current,powered&&rail>4.75f?rail:0,motor||maintain,mode==Mode.Reuse,regen,dump,mode==Mode.Discharge&&dump);
            bool quadrature=encoder.Step(dt,energy.Angle,Routed(10),Routed(11),Routed(12));
            indexAge=energy.Rpm>35 && !encoder.Index ? indexAge+dt : 0;
            SetContact(drive,(motor||maintain)&&ready&&safe);
            SetContact(protection,regen || (mode==Mode.Reuse && CapRoute&&ready&&safe));
            SetContact(brakeDrive,dump&&energy.BrakePower>.001f);
            if (motor)
            {
                // Converter input is represented by an averaged electrical load.
                float load=energy.Current>.001f?Mathf.Clamp(rail*rail/Mathf.Max(.05f,energy.Voltage*energy.Current),12,500):1000;
                if (activeTime% .15f<dt && Mathf.Abs(driver.ResistanceOhms-load)>1) driver.SetResistance(load);
            }

            traceClock+=dt;
            if (traceClock>=.06f)
            {
                traceClock=0;
                traceA[traceCursor]=energy.Rpm/90;
                traceB[traceCursor]=energy.TerminalVolts/SparkLevel56Energy.MaxVolts;
                traceCursor=(traceCursor+1)%traceA.Length;
            }
            if (mode==Mode.Fault)
            {
                if (Stopped && sawFault && energy.TerminalVolts<=SparkLevel56Energy.MaxVolts) verifiedSafe=true;
                return;
            }
            if ((!quadrature||indexAge>2.2f) && ready && energy.Rpm>38) {Trip("ENCODER A/B/Z INVALID");return;}
            if (!float.IsFinite(energy.Rpm) || energy.Rpm>98 || energy.Temperature>84 ||
                energy.TerminalVolts>SparkLevel56Energy.MaxVolts+.015f || energy.BusVoltage>5.3f)
            {Trip("OVERLIMIT · SPEED, TEMPERATURE OR STORAGE");return;}
            if (activeTime>65 && !Idle) {Trip("TEST TIMED OUT");return;}

            if (motor)
            {
                bool atTarget=Mathf.Abs(energy.Rpm-targetRpm)<.65f && encoder.Locked && Mathf.Abs(encoder.Rpm-targetRpm)<1.6f;
                hold=atTarget?hold+dt:0;
                if (hold<.95f) return;
                if (mode==Mode.Reuse)
                {
                    float spent=energy.SourceWork-spinSupply;
                    reuseSaved=baselineSupply-spent;
                    if (initialStored-energy.Stored>.35f && reuseSaved>.25f && Mathf.Abs(rpmStart-20)<.55f) Award();
                }
                else if (stage==1 && At90) Award();
                else if (stage==3 && At90 && Mathf.Abs(rpmStart-20)<.55f) baselineSupply=energy.SourceWork-spinSupply;
                Change(Mode.Idle);
            }
            else if (mode==Mode.Regen)
            {
                regenPower=Mathf.Max(regenPower,-energy.Current*energy.Voltage);
                peakTemp=Mathf.Max(peakTemp,energy.Temperature);
                if (energy.Rpm>20) return;
                float released=startKinetic-energy.Kinetic;
                float stored=energy.Stored-initialStored;
                regenEfficiency=stored/Mathf.Max(.01f,released);
                EnergyError=released-stored-(energy.Loss-initialLoss)-(energy.Heat-initialHeat)-(energy.Dumped-initialDump);
                bool measured=released>3 && stored>.3f && energy.GeneratorWork-initialGenerator>.6f && regenPower>.08f && Mathf.Abs(EnergyError)<.035f;
                if (stage==2 && measured) {baselineEfficiency=regenEfficiency; Award();}
                else if (stage==3) compared=optimizedRun&&measured&&baselineSupply>0;
                else if (stage==5 && repaired && measured && regenEfficiency>=.60f) Award();
                Change(Mode.Idle);
                if (!measured) notice="Recovery did not meet measured energy and speed conditions.";
            }
            else if (mode==Mode.Brake && Stopped)
            {
                Change(Mode.Idle);
                if (protectTest) notice="Chopper braking complete. Read the recorded resistor temperature.";
                protectTest=false;
            }
            else if (mode==Mode.Discharge && energy.TerminalVolts<=.18f)
            {
                Change(Mode.Idle);
                notice="Bank reset. Start the comparable 20-to-90 RPM run.";
            }
        }

        private void Award()
        {
            if (stage>=Gates.Length || !bindings.SetRuntimeComponentBinding(Gates[stage],source)) return;
            stage++;
            manager.QueueEvaluation();
        }

        private string Prompt()
        {
            if (mode==Mode.Fault) return $"SAFE STOP · {fault} · "+(ready&&Stopped?"RESET FAULT":wiring??"Wait for safe stop.");
            if (wiring!=null) return wiring;
            if (!safe) return "Close the guard and check W26.";
            if (!ready) return "Check the solved 5 V rails.";
            if (!calibrated) return "Select CALIBRATE with the flywheel at rest.";
            if (armed) return "Undo W16 now. Reconnect after the flywheel stops.";
            if (mode!=Mode.Idle) return mode switch
            {
                Mode.Spin=>"Motoring · building measured flywheel energy.",
                Mode.Regen=>"Regenerating · motor torque opposes rotation and the bank charges.",
                Mode.Reuse=>"Reuse · stored energy assists the flywheel acceleration.",
                Mode.Brake=>"Resistive braking · watch chopper power and temperature.",
                Mode.Discharge=>"Preparing a comparable test by discharging the bank through the resistor.",
                _=>"Waiting for safe operating conditions."
            };
            if (notice!=null) return notice;
            return stage switch {
                1=>"Select START to establish the 90 RPM reference.",
                2=>"Select BEGIN RECOVERY for the first 90-to-20 RPM run.",
                3 when compared=>"Select VERIFY EFFICIENCY.",
                3 when energy.TerminalVolts>.22f=>"Select OPTIMIZE to clear the bank. Select LOW CURRENT for the next run.",
                3 when !At90=>"Select START for the comparable 20-to-90 RPM supply baseline.",
                3=>"Select BEGIN RECOVERY to test the new current setting.",
                4=>"Select ENERGY REUSE TEST at the 20 RPM reference.",
                5 when repaired&&!At90=>"Rebuild the 90 RPM reference after repair.",
                5 when repaired=>"Select FINAL CERTIFICATION.",
                5=>"Select FAULT TEST, then undo W16.",
                6=>"CERTIFIED · Stored energy reused and feedback protection verified.",
                _=>"Waiting for the electrical solver."
            };
        }
        private void Present()
        {
            statusText.text=Prompt();
            string v=CapRoute&&calibrated?$"{energy.TerminalVolts:F2} V":"NO FEEDBACK";
            meterText.text=$"{energy.Rpm:F1} RPM   ·   CAP {v}  /  {energy.Stored:F2} J   ·   FLYWHEEL {energy.Kinetic:F2} J   ·   BUS {energy.BusVoltage:F2} V";
            detailText.text=$"MOTOR {energy.Current:+0.00;-0.00;0.00} A   ·   CAP {energy.CapacitorCurrent:+0.00;-0.00;0.00} A   ·   BRAKE {energy.BrakePower:F2} W / {energy.Temperature:F0} °C   ·   Z {(encoder.Locked?"LOCK":"—")}   ·   {manager.SatisfiedTargetCount}/6";
            calibrationText.text=$"FIRST {baselineEfficiency:P1}   ·   LAST {regenEfficiency:P1}   ·   SET {brakeCurrent:F2} A   ·   GRID SAVED {reuseSaved:F2} J   ·   BALANCE ERROR {EnergyError:F3} J";
            if (controllerDisplay) controllerDisplay.text=$"{mode.ToString().ToUpperInvariant()}   {energy.Rpm:F1} RPM\n{energy.Current:F2} A   {energy.Voltage:F2} V";
            if (qualityDisplay) qualityDisplay.text=$"CAP {v}   {energy.Stored:F2} J\nRECOVERY {regenEfficiency:P1} · BRAKE {energy.Temperature:F0} °C";
            if (rig) rig.Apply(energy.Angle,guardOpen,energy.Temperature,energy.CapacitorCurrent,energy.Rpm,Time.deltaTime);
            if (plot) plot.Show(traceA,traceB,traceCursor,calibrated);

            powerButton.interactable=true;
            guardButton.interactable=Stopped&&Idle;
            guardButton.GetComponentInChildren<Text>().text=guardOpen?"CLOSE GUARD":"OPEN GUARD";
            calibrateButton.interactable=ready&&safe&&Stopped&&Idle&&!calibrated;
            startButton.interactable=motoringButton.interactable=CanSpin&&(stage!=3||energy.TerminalVolts<=.22f)&&stage!=4;
            stopButton.interactable=mode!=Mode.Fault&&!Stopped;
            slowButton.interactable=midButton.interactable=fastButton.interactable=stage==6&&Idle&&!armed;
            regenButton.interactable=recoveryButton.interactable=CanRecover&&(stage is 2 or 3)&&!compared;
            resistiveButton.interactable=Idle&&ready&&safe&&(energy.Rpm>1||energy.Stored>.02f);
            optimizeButton.interactable=stage==3&&Idle&&energy.TerminalVolts>.22f&&!compared;
            lowButton.interactable=highButton.interactable=stage==3&&(Idle||mode==Mode.Discharge)&&!compared;
            verifyButton.interactable=stage==3&&Idle&&compared;
            reuseButton.interactable=reuseTestButton.interactable=stage==4&&Idle&&ready&&safe&&energy.Rpm is >=19.5f and <=20.4f;
            protectButton.interactable=stage==6&&Idle&&ready&&safe&&energy.Rpm>=20;
            phaseButton.interactable=stage==5&&Idle&&At90&&!repaired&&!armed;
            resetButton.interactable=mode==Mode.Fault&&ready&&safe&&Stopped&&CapRoute;
            finalButton.interactable=stage==5&&repaired&&CanRecover;
        }
        private void OnDisable()
        {
            if (bindings!=null)
                foreach (string gate in Gates) bindings.ClearRuntimeComponentBinding(gate);
        }
    }
}




