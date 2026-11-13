using System.Collections.Generic;
using ProjectSpark.Circuit;
using ProjectSpark.Electrical;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.Gameplay
{
    [DisallowMultipleComponent]
    public sealed class SparkLevel50Factory : MonoBehaviour
    {
        public static readonly string[] Gates = {
            "L50_Wired","L50_Calibrated","L50_Normal","L50_Mixed","L50_Disturbance","L50_Recovery"
        };
        public static readonly (int a,int b)[] Wires = {
            (0,1),(2,3),(0,4),(2,5),(0,6),(2,7),(0,8),(2,9),(10,11),(12,13),(14,15),
            (16,17),(18,19),(20,21),(22,23),(24,25),(26,27),(28,29),(30,31),(32,33),(34,35),(36,37)
        };
        private const float Dt=.005f, Radius=.020f, Unit=2*Mathf.PI*Radius/80, Pitch=160*Unit;
        private const float PrintOffset=80*Unit, CutOffset=160*Unit, InspectOffset=240*Unit, SortOffset=320*Unit;
        private const float Down=.075f, Return=.09f, RailAcceleration=2.8f, CutTrack=.025f, CutSync=.006f;
        private static readonly int[] Gray={0,2,3,1};
        private static readonly int[] Decode={0,-1,1,0, 1,0,0,-1, -1,0,0,1, 0,1,-1,0};
        private enum Mode { Off, Homing, Calibrating, Manual, Auto, Fault }
        private enum Phase { Registered, Printed, Cut, Inspected, Routing }
        private enum Defect { None, PrintOffset, MissingPrint, CutOffset }
        private enum CutCycle { Ready, Approach, Track, Brake, Return }

        private sealed class Product
        {
            public int id;
            public float mark, printError, cutError, binTravel, launchRpm;
            public bool printed, cut, passed, expectedGood, rejectBin;
            public Defect defect;
            public Phase phase;
            public Transform token;
        }

        public SparkLevelLayout layout;
        public SparkPowerSupply source;
        public SparkMotor transportMotor, sortMotor;
        public SparkSwitch transportDrive, sortDrive, safetyContact, homeContact;
        public SparkTerminal[] pins;
        public SparkLevel50VisualRig rig;
        public Transform[] webMarks,productTokens;
        public GameObject scanner,visionLight;
        public Transform emergencyButton;
        public TMP_Text controllerDisplay,qualityDisplay;
        public Text statusText,meterText,detailText;
        public Button powerButton,homeButton,calibrateButton,manualButton,startButton,stopButton,resetButton;
        public Button slowButton,fastButton,normalButton,mixedButton,loadButton,glareButton,safetyButton;

        private readonly List<SparkCircuitConnection> wires=new();
        private readonly Dictionary<SparkTerminal,SparkTerminal> nets=new();
        private readonly List<Product> products=new();
        private SparkLevelLayoutLoader loader;
        private LevelGamePlayManager manager;
        private SparkLevelSceneBindings bindings;
        private SparkCircuitSystem circuit;
        private SparkElectricalSolver solver;
        private Mode mode;
        private CutCycle cutCycle;
        private Product printing,cutting;
        private Vector3[] markOrigins,tokenOrigins;
        private Vector3 stopOrigin;
        private int topology=-1,stage,edge,count,phase,markNumber=1,lastMark=-1,measuredPitch;
        private int nextId,good,rejected,quarantined,wrongBin,mixedGood,mixedReject,qualified;
        private int glareRejected,processedAfterDisturbance,recoveryGood,mixedSequence,disturbanceStartCount;
        private int requestedRpm=30;
        private float accumulator,travel,velocity,gateAngle=19,printTimer,printStroke,cutStroke,rail,railSpeed;
        private float speedDip,baselineSpeed,peakDip,driveWait,lastPrintError,lastCutError;
        private bool powered=true,ready,safe,home,homed,calibrated,auto=true,mixed,safetyOpen;
        private bool glarePending,speedObserved,emergencyObserved,recoveryReset;
        private string wiring,fault,lastResult="—";

        private float Rpm=>velocity*60/(2*Mathf.PI*Radius);
        private float EncoderPosition=>count*Unit+travel-edge*Unit;
        private bool Running=>mode is Mode.Calibrating or Mode.Manual or Mode.Auto;
        private bool Active=>Running || mode==Mode.Homing;
        private bool Still=>velocity<.0005f && Mathf.Abs(railSpeed)<.0005f && printStroke<=0 && cutStroke<=0;
        private bool CanHome=>mode==Mode.Off && ready && safe && Still;
        private bool CanCalibrate=>CanHome && home && homed;
        private bool CanStart=>CanCalibrate && (!auto || calibrated);
        private bool CanReset=>mode==Mode.Fault && ready && safe && Still && Mathf.Abs(gateAngle)<.2f;

        private void Update()
        {
            if(loader==null) loader=FindFirstObjectByType<SparkLevelLayoutLoader>();
            if(loader==null || loader.ActiveLayout!=layout) return;
            if(manager==null) manager=FindFirstObjectByType<LevelGamePlayManager>();
            if(manager==null) return;
            circuit=manager.CircuitSystem;
            solver=manager.ElectricalSolver;
            if(circuit==null || solver==null) return;
            if(bindings!=loader.ActiveBindings)
            {
                bindings=loader.ActiveBindings;
                if(bindings==null) return;
                topology=-1;
            }
            if(topology!=circuit.TopologyVersion)
            {
                topology=circuit.TopologyVersion;
                wiring=CheckWiring();
            }
            ReadPower();
            if(stage==0 && ready && safe) Award();
            if(Active && (!safe || !ready)) Trip(!safe ? "SAFETY INPUT LOST" : wiring??"POWER FAULT",!safe);
            accumulator+=Mathf.Min(Time.unscaledDeltaTime,.15f);
            while(accumulator>=Dt) { accumulator-=Dt; Step(); }
            Present();
        }

        public void MainPower()
        {
            powered=!powered;
            if(!powered && Active) Trip("MAIN POWER LOST");
            source.SetOutput(powered,default);
        }
        public void Home()
        {
            if(!CanHome) return;
            homed=false;
            mode=Mode.Homing;
            Drive(sortDrive,true);
        }
        public void Calibrate()
        {
            if(!CanCalibrate) return;
            calibrated=false;
            Begin(Mode.Calibrating);
        }
        public void StartLine() { if(CanStart) Begin(auto?Mode.Auto:Mode.Manual); }
        public void StopLine()
        {
            if(!Active) return;
            mode=Mode.Off;
            Halt();
        }
        public void ToggleAuto() { if(mode==Mode.Off) auto=!auto; }
        public void Speed30() { if(stage<=2 && mode==Mode.Off) requestedRpm=30; }
        public void Speed60() { if(stage>=3 && mode is Mode.Auto or Mode.Off) requestedRpm=60; }
        public void Mixed()
        {
            if(stage!=3 || requestedRpm!=60) return;
            mixed=true;
            mixedSequence=0;
        }
        public void Normal() => mixed=false;
        public void TestSpeed()
        {
            if(stage!=4 || mode!=Mode.Auto || speedObserved || speedDip>0 || products.Count<2) return;
            speedDip=1.35f;
            peakDip=0;
            baselineSpeed=velocity;
            disturbanceStartCount=count;
            processedAfterDisturbance=0;
        }
        public void TestGlare()
        {
            if(stage==4 && mode==Mode.Auto && glareRejected==0) glarePending=true;
        }
        public void ToggleSafety()
        {
            safetyOpen=!safetyOpen;
            Drive(safetyContact,!safetyOpen);
            if(safetyOpen && Active) Trip("EMERGENCY STOP",true);
        }
        public void ResetFault()
        {
            if(!CanReset) return;
            mode=Mode.Off;
            fault=null;
            recoveryReset=emergencyObserved;
            recoveryGood=0;
            homed=calibrated=false;
            mixed=false;
        }

        private void Begin(Mode next)
        {
            mode=next;
            lastMark=-1;
            Drive(transportDrive,true);
        }

        private void Halt()
        {
            Drive(transportDrive,false);
            Drive(sortDrive,false);
            quarantined+=products.Count;
            foreach(var p in products) p.token.gameObject.SetActive(false);
            products.Clear();
            printing=cutting=null;
            cutCycle=CutCycle.Ready;
            speedDip=0;
            glarePending=false;
        }

        private void Trip(string why,bool emergency=false)
        {
            if(mode==Mode.Fault) return;
            emergencyObserved|=emergency && stage==5 && mode==Mode.Auto;
            recoveryReset=false;
            fault=why;
            homed=calibrated=false;
            mode=Mode.Fault;
            Halt();
        }

        private static void Drive(SparkSwitch contact,bool on)
        {
            if(contact.IsClosed!=on) contact.SetState(on?SparkSwitchState.Closed:SparkSwitchState.Open);
        }

        private void ReadPower()
        {
            var result=solver.LatestSolveResult;
            bool High(int a,int b)=>result!=null && result.TryGetVoltage(pins[a],pins[b],out float v) && v is >=4.8f and <=5.2f;
            ready=powered && wiring==null && source.IsOutputActive && !source.IsOverloaded &&
                High(1,3) && High(4,5) && High(6,7) && High(8,9);
            safe=!safetyOpen && safetyContact.IsClosed && Routed(21) && High(37,3);
            home=homeContact.IsClosed && Routed(18) && High(31,3);
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
        private string CheckWiring()
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
            for(int i=0;i<Wires.Length;i++)
            {
                var (a,b)=Wires[i];
                if(!wires.Exists(w=>w.Kind==SparkConnectionKind.Wire &&
                    (w.A==pins[a] && w.B==pins[b] || w.B==pins[a] && w.A==pins[b])))
                    return $"Connect W{i+1:00} · {pins[a].name} → {pins[b].name}.";
            }
            var unique=new HashSet<SparkTerminal>{Root(pins[0]),Root(pins[2])};
            if(unique.Count!=2) return "Separate the power rails.";
            for(int i=8;i<Wires.Length;i++)
                if(!unique.Add(Root(pins[Wires[i].a]))) return "Separate the factory signal nets.";
            return null;
        }

        private void Step()
        {
            bool web=Running && transportDrive.IsClosed && Mathf.Abs(transportMotor.ElectricalState.Current)>.015f;
            bool sort=sortDrive.IsClosed && Mathf.Abs(sortMotor.ElectricalState.Current)>.015f;
            driveWait=(Running && !web || sortDrive.IsClosed && !sort)?driveWait+Dt:0;
            if(driveWait>.3f || transportMotor.IsOverCurrent || sortMotor.IsOverCurrent) Trip("DRIVE CURRENT FAULT");
            float demand=(mode==Mode.Calibrating?18:requestedRpm)*2*Mathf.PI*Radius/60;
            if(speedDip>0)
            {
                speedDip=Mathf.Max(0,speedDip-Dt);
                demand*=.68f;
                peakDip=Mathf.Max(peakDip,baselineSpeed-velocity);
                if(peakDip>.022f && count-disturbanceStartCount>=8) speedObserved=true;
            }
            float prior=travel;
            velocity=Mathf.MoveTowards(velocity,web?demand:0,(web?.20f:.47f)*Dt);
            travel+=velocity*Dt;
            ReadEncoder();
            ReadMarks();
            if(mode==Mode.Homing)
            {
                gateAngle=Mathf.MoveTowards(gateAngle,0,(sort?180:0)*Dt);
                MoveRail(rail>0?-.05f:0);
                if(home && rail<=0 && Still) { homed=true; mode=Mode.Off; Drive(sortDrive,false); }
            }
            else if(mode==Mode.Auto) AdvanceFactory(prior,sort);
            else
            {
                MoveRail(0);
                gateAngle=Mathf.MoveTowards(gateAngle,0,160*Dt);
                printStroke=Mathf.MoveTowards(printStroke,0,14*Dt);
                cutStroke=Mathf.MoveTowards(cutStroke,0,14*Dt);
            }
            Drive(homeContact,Mathf.Abs(gateAngle)<.2f);
        }

        private void ReadEncoder()
        {
            int next=Mathf.FloorToInt(travel/Unit);
            while(edge!=next)
            {
                edge+=edge<next?1:-1;
                int signal=Gray[edge&3];
                if(ready)
                {
                    int sample=(Routed(9)?signal&2:0)|(Routed(10)?signal&1:0);
                    int delta=Decode[phase*4+sample];
                    count+=delta;
                    if(Running && delta!=1) Trip("ENCODER PHASE OR DIRECTION INVALID");
                }
                phase=signal;
            }
        }

        private void ReadMarks()
        {
            while(travel>=markNumber*Pitch)
            {
                float mark=markNumber++*Pitch;
                if(!Running || !Routed(8)) continue;
                int measured=Mathf.RoundToInt((EncoderPosition-travel+mark)/Unit);
                bool second=lastMark>=0;
                if(second)
                {
                    measuredPitch=measured-lastMark;
                    if(Mathf.Abs(measuredPitch-160)>3) { Trip("REGISTRATION MARK SPACING INVALID"); return; }
                }
                lastMark=measured;
                if(mode==Mode.Calibrating && second)
                {
                    calibrated=true;
                    StopLine();
                    if(stage==1) Award();
                }
                else if(mode==Mode.Auto) CreateProduct(measured*Unit);
            }
            if(glarePending && lastMark>=0 && count-lastMark is >=45 and <=75 && Routed(8))
            {
                // This deliberate extra pulse fails the same pitch window used for a genuine mark.
                glarePending=false;
                if(Mathf.Abs(count-lastMark-160)>3) { glareRejected++; processedAfterDisturbance=0; }
            }
        }

        private void CreateProduct(float mark)
        {
            Transform token=System.Array.Find(productTokens,t=>!t.gameObject.activeSelf);
            if(token==null) { Trip("PRODUCT TRACKING BUFFER FULL"); return; }
            Defect defect=Defect.None;
            if(mixed && stage==3 && Rpm>58)
                defect=(mixedSequence++%4) switch { 1=>Defect.PrintOffset,3=>Defect.MissingPrint,_=>Defect.None };
            token.gameObject.SetActive(true);
            products.Add(new Product {id=++nextId,mark=mark,launchRpm=Rpm,defect=defect,expectedGood=defect==Defect.None,token=token});
        }

        private void AdvanceFactory(float prior,bool sortPowered)
        {
            if(mode!=Mode.Auto || !safe) return;
            AdvancePrint(prior);
            AdvanceCut();
            if(mode!=Mode.Auto) return;
            Product routing=null;
            foreach(var p in products)
            {
                float progress=EncoderPosition-p.mark;
                if(p.phase==Phase.Cut && progress>=InspectOffset)
                {
                    p.passed=p.printed && p.cut && Mathf.Abs(p.printError)<=2 && Mathf.Abs(p.cutError)<=2;
                    if(!Routed(p.passed?13:14)) { Trip("VISION SIGNAL LOST"); return; }
                    p.phase=Phase.Inspected;
                    lastResult=$"#{p.id} "+(p.passed?"PASS":!p.printed?"MISSING PRINT":Mathf.Abs(p.printError)>2?"PRINT OFFSET":"CUT ERROR");
                }
                if(p.phase==Phase.Routing || p.phase==Phase.Inspected && progress>=SortOffset-.037f)
                {
                    routing=p;
                    break;
                }
            }
            bool divert=routing!=null && !routing.passed && Routed(15);
            Drive(sortDrive,divert);
            gateAngle=Mathf.MoveTowards(gateAngle,divert?35:0,(divert?(sortPowered?235:0):195)*Dt);
            if(routing==null) return;
            if(routing.phase==Phase.Inspected && EncoderPosition-routing.mark>=SortOffset)
            {
                routing.rejectBin=gateAngle>25;
                routing.phase=Phase.Routing;
            }
            if(routing.phase!=Phase.Routing) return;
            routing.binTravel=Mathf.Min(1,routing.binTravel+Dt/.29f);
            if(routing.binTravel>=1) ConfirmBin(routing);
        }

        private void AdvancePrint(float previousTravel)
        {
            if(printing==null)
            {
                printStroke=Mathf.MoveTowards(printStroke,0,Dt/Return);
                if(printStroke>0 || !Routed(11)) return;
                printing=products.Find(p=>p.phase==Phase.Registered && EncoderPosition+velocity*(Down+.5f*Dt)>=p.mark+PrintOffset+
                    (p.defect==Defect.PrintOffset?.008f:0));
                printTimer=0;
                return;
            }
            float old=printTimer;
            printTimer+=Dt;
            printStroke=Mathf.Clamp01(printTimer/Down);
            if(printTimer<Down) return;
            printing.printed=printing.defect!=Defect.MissingPrint;
            printing.printError=(Mathf.Lerp(previousTravel,travel,Mathf.Clamp01((Down-old)/Dt))-printing.mark-PrintOffset)*1000;
            lastPrintError=printing.printError;
            printing.phase=Phase.Printed;
            printing=null;
        }

        private void MoveRail(float command)
        {
            railSpeed=Mathf.MoveTowards(railSpeed,Mathf.Clamp(command,-.22f,.22f),RailAcceleration*Dt);
            rail+=railSpeed*Dt;
            if(rail<=0) { rail=0; railSpeed=0; }
            if(rail>.055f) { rail=.055f; railSpeed=0; Trip("CUTTER RAIL END STOP"); }
        }

        private void AdvanceCut()
        {
            if(cutCycle==CutCycle.Ready)
            {
                if(!Routed(12)) return;
                cutting=products.Find(p=>p.phase==Phase.Printed && EncoderPosition>=p.mark+CutOffset-.034f);
                if(cutting!=null) cutCycle=CutCycle.Approach;
                return;
            }
            if(cutCycle is CutCycle.Approach or CutCycle.Track)
            {
                float target=cutting.mark+CutOffset+(cutting.defect==Defect.CutOffset?.008f:0);
                float goal=EncoderPosition-target+CutTrack*.5f;
                float previous=rail;
                MoveRail(Mathf.Clamp(velocity+(goal-rail)*32f,0,.22f));
                bool synced=Mathf.Abs(railSpeed-velocity)<=CutSync && Mathf.Abs(goal-rail)<.0017f;
                if(cutCycle==CutCycle.Approach && goal>=.002f && synced) cutCycle=CutCycle.Track;
                if(cutCycle==CutCycle.Approach && goal>.009f || cutCycle==CutCycle.Track && !synced)
                {
                    cutting.phase=Phase.Cut;
                    cutting.cut=false;
                    cutCycle=CutCycle.Brake;
                    return;
                }
                if(cutCycle!=CutCycle.Track) return;
                cutStroke=Mathf.Clamp01(1-Mathf.Abs(goal-CutTrack*.5f)/(CutTrack*.5f));
                if(cutting.phase!=Phase.Cut && goal>=CutTrack*.5f)
                {
                    float lastGoal=goal-velocity*Dt;
                    float atContact=Mathf.Lerp(previous,rail,Mathf.Clamp01((CutTrack*.5f-lastGoal)/(goal-lastGoal)));
                    cutting.cut=true;
                    cutting.cutError=(target+CutTrack*.5f-atContact-cutting.mark-CutOffset)*1000;
                    lastCutError=cutting.cutError;
                    cutting.phase=Phase.Cut;
                }
                if(rail>=CutTrack+.0015f) { cutCycle=CutCycle.Brake; cutStroke=0; }
                return;
            }
            cutStroke=Mathf.MoveTowards(cutStroke,0,14*Dt);
            if(cutCycle==CutCycle.Brake)
            {
                MoveRail(0);
                if(railSpeed<=0) cutCycle=CutCycle.Return;
                return;
            }
            MoveRail(rail>0?-Mathf.Min(.17f,Mathf.Sqrt(2*RailAcceleration*rail)):0);
            if(rail<=0) { cutCycle=CutCycle.Ready; cutting=null; }
        }

        private void ConfirmBin(Product p)
        {
            if(!Routed(p.rejectBin?20:19)) { Trip("COLLECTION SENSOR LOST"); return; }
            p.token.gameObject.SetActive(false);
            products.Remove(p);
            if(p.rejectBin==p.passed) { wrongBin++; Trip("WRONG BIN"); return; }
            if(p.rejectBin) rejected++; else good++;
            if(stage==2)
            {
                qualified=p.passed && Mathf.Abs(p.launchRpm-30)<1.5f?qualified+1:0;
                if(qualified>=3) Award();
            }
            else if(stage==3 && mixed && p.launchRpm>=58)
            {
                if(p.expectedGood!=p.passed) { Trip("MIXED BATCH CLASSIFICATION MISMATCH"); return; }
                if(p.passed) mixedGood++; else mixedReject++;
                if(mixedGood>=2 && mixedReject>=2) { mixed=false; Award(); }
            }
            else if(stage==4 && speedObserved && speedDip<=0 && glareRejected>0 && p.passed)
            {
                if(++processedAfterDisturbance>=2) Award();
            }
            else if(stage==5 && recoveryReset && p.passed && home && calibrated)
            {
                if(++recoveryGood>=4) Award();
            }
        }

        private void Award()
        {
            if(stage>=Gates.Length || !bindings.SetRuntimeComponentBinding(Gates[stage],source)) return;
            stage++;
            manager.QueueEvaluation();
        }

        private string Prompt()
        {
            if(mode==Mode.Fault) return $"SAFE STOP · {fault} | "+(CanReset?"RESET FAULT, then HOME and CALIBRATE.":safetyOpen?"Restore safety and wait for the line to stop.":wiring??"Waiting for safe feedback.");
            if(wiring!=null) return wiring;
            if(!powered) return "Turn on MAIN POWER.";
            if(!safe) return "Restore safety and check W22.";
            if(mode==Mode.Homing) return "HOMING · Verifying the cutter and diverter.";
            if(mode==Mode.Calibrating) return "CALIBRATING · Waiting for two genuine optical marks.";
            if(!homed) return "Press HOME to verify the diverter and cutter.";
            if(!calibrated) return "Press CALIBRATE to measure mark pitch.";
            return stage switch {
                2=>"START in AUTO at 30 RPM. Deliver three GOOD products.",
                3=>"Select 60 RPM and MIXED BATCH. Confirm two GOOD and two REJECT products.",
                4=>"Run SPEED TEST and FALSE MARK with products in flight. Verify two subsequent GOOD products.",
                5 when recoveryReset=>"START. Deliver four new GOOD products.",
                5=>"Trigger EMERGENCY STOP while running. Restore, RESET, HOME and CALIBRATE.",
                _=>"Factory commissioning complete."
            };
        }

        private void Present()
        {
            statusText.text=Prompt();
            meterText.text=$"{Rpm:F1} RPM  |  ENC {count}  |  PITCH {measuredPitch}  |  IN FLIGHT {products.Count}  |  GOOD {good}  /  REJECT {rejected}  /  QUARANTINE {quarantined}";
            detailText.text=$"{mode.ToString().ToUpperInvariant()}  |  {(mixed?"MIXED":"NORMAL")}  |  SAFE {(safe?"OK":"OPEN")}  |  HOME {(home?"ON":"OFF")}  |  PRINT {lastPrintError:+0.00;-0.00;0.00} / CUT {lastCutError:+0.00;-0.00;0.00} mm  |  {manager.SatisfiedTargetCount}/6";
            controllerDisplay.text=$"{Rpm:F1} RPM / {count} CT\n{mode.ToString().ToUpperInvariant()}  •  {products.Count} IN FLIGHT";
            qualityDisplay.text=$"GOOD {good}   REJECT {rejected}\n{lastResult}";
            rig.ApplyVisualState(travel,rail,printStroke,cutStroke,gateAngle);
            scanner.SetActive(ready && safe);
            visionLight.SetActive(ready && safe);
            if(markOrigins==null)
            {
                markOrigins=System.Array.ConvertAll(webMarks,t=>t.localPosition);
                tokenOrigins=System.Array.ConvertAll(productTokens,t=>t.localPosition);
                stopOrigin=emergencyButton.localPosition;
            }
            emergencyButton.localPosition=stopOrigin+Vector3.forward*(safetyOpen?.012f:0);
            const float eye=-.68f, scale=.34f/PrintOffset, gap=Pitch*scale;
            float first=eye+travel*scale-Mathf.Floor((eye+travel*scale+.87f)/gap)*gap;
            for(int i=0;i<webMarks.Length;i++)
            {
                var p=markOrigins[i];
                p.x=first+i*gap;
                bool show=p.x<.87f;
                if(webMarks[i].gameObject.activeSelf!=show) webMarks[i].gameObject.SetActive(show);
                if(show) webMarks[i].localPosition=p;
            }
            foreach(var product in products)
            {
                var start=tokenOrigins[System.Array.IndexOf(productTokens,product.token)];
                start.x=eye+Mathf.Min(SortOffset,travel-product.mark)*scale;
                if(product.phase==Phase.Routing)
                    start=Vector3.Lerp(start,new Vector3(product.rejectBin?.89f:.61f,start.y-.065f,product.rejectBin?-.25f:.43f),product.binTravel);
                product.token.localPosition=start;
            }
            powerButton.interactable=safetyButton.interactable=true;
            safetyButton.GetComponentInChildren<Text>().text=safetyOpen?"RESTORE SAFETY":"EMERGENCY STOP";
            manualButton.interactable=mode==Mode.Off;
            manualButton.GetComponentInChildren<Text>().text=auto?"MODE: AUTO":"MODE: MANUAL";
            homeButton.interactable=CanHome;
            calibrateButton.interactable=CanCalibrate;
            startButton.interactable=CanStart;
            stopButton.interactable=Active;
            resetButton.interactable=CanReset;
            slowButton.interactable=requestedRpm!=30 && stage<=2 && mode==Mode.Off;
            fastButton.interactable=stage>=3 && requestedRpm!=60 && mode is Mode.Auto or Mode.Off;
            normalButton.interactable=mixed;
            mixedButton.interactable=stage==3 && requestedRpm==60 && !mixed;
            loadButton.interactable=stage==4 && mode==Mode.Auto && !speedObserved && speedDip<=0 && products.Count>=2;
            glareButton.interactable=stage==4 && mode==Mode.Auto && !glarePending && glareRejected==0;
        }

        private void OnDisable()
        {
            if(gameObject.activeInHierarchy && transportDrive!=null && sortDrive!=null) Halt();
            if(bindings!=null) foreach(string gate in Gates) bindings.ClearRuntimeComponentBinding(gate);
            bindings=null;
        }
    }
}
