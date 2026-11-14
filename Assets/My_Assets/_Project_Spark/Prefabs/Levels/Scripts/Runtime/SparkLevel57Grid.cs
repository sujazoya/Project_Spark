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
    public sealed class SparkLevel57Grid : MonoBehaviour
    {
        public static readonly string[] Gates={"L57_Wired","L57_Reference","L57_Parallel","L57_Shared","L57_Island","L57_Recovered"};
        public static readonly (int a,int b)[] Wires={(0,1),(2,3),(0,4),(2,5),(0,6),(2,7),(0,8),(2,9),(10,11),(12,13),(14,15),(16,17),(18,19),(20,21),(22,23),(24,17),(11,25),(13,26),(15,27),(17,28),(11,29),(13,30),(15,31),(32,33),(34,35),(36,37),(38,39),(40,41),(42,43),(44,45),(46,47),(48,49),(50,51),(52,53)};
        public SparkLevelLayout layout;
        public SparkPowerSupply source;
        public SparkSwitch isolation, poleA, poleB, poleC, emergency;
        public SparkTerminal[] pins;
        public SparkLevel57VisualRig rig;
        public SparkLevel57Plot plot;
        public TMP_Text controllerDisplay,qualityDisplay;
        public Text statusText,meterText,detailText,calibrationText;
        public Button powerButton,guardButton,calibrateButton,startButton,enableButton,checkButton,pllButton,hzButton,voltageButton;
        public Button syncButton,closeButton,openButton,autoButton,manualButton,moreButton,lessButton,stepButton,islandButton;
        public Button restoreButton,phaseButton,faultButton,resetButton,finalButton;
        private readonly SparkLevel57Phasor ac=new();
        private readonly List<SparkCircuitConnection> wires=new();
        private readonly Dictionary<SparkTerminal,SparkTerminal> nets=new();
        private readonly HashSet<(int,int)> verified=new();
        private SparkLevelLayoutLoader loader;
        private LevelGamePlayManager manager;
        private SparkLevelSceneBindings bindings;
        private SparkElectricalSolver solver;
        private SparkCircuitSystem circuit;
        private string wiring,fault,notice;
        private int topology=-1,stage;
        private float stepClock,referenceHold,syncHold,parallelHold,shareHold,islandHold,load=1,trim=1.8f,magnitudeMinimum;
        private bool ready,guardOK,powered=true,stopped,calibrated,primary,secondary,island,restoring,sync,matchHz,matchV;
        private bool autoShare,loadStep,armed,faultObserved,repaired,finalRequested,phaseView;
        private bool Closed => poleA.IsClosed && poleB.IsClosed && poleC.IsClosed;
        private bool Reference => ready && guardOK && calibrated && fault==null && GridFeedback && primary && isolation.IsClosed;
        private bool GridFeedback => Routed(23);
        private bool Sequence => Routed(8)&&Routed(9)&&Routed(10)&&Routed(12)&&Routed(13)&&Routed(14);
        private bool Window => Reference && secondary && !island && !Closed && Sequence && Routed(24)&&Routed(25)&&ac.InWindow;
        private bool CanClose => Window && syncHold>=.8f;
        private bool StableBus => ac.BusVolts is >=2.15f and <=2.52f && Mathf.Abs(ac.BusHz-50)<.2f &&
            Mathf.Abs(ac.Residual)<.002f && ac.CurrentA<.55f && ac.CurrentB<.55f;
        private bool Shared => Closed && !island && primary && secondary && StableBus &&
            ac.Share is >=.40f and <=.60f && ac.Circulating<.19f;
        private bool Routed(int i)
        {
            var (a,b)=Wires[i];
            return verified.Contains((Mathf.Min(a,b),Mathf.Max(a,b)));
        }

        private void Start()
        {
            loader=FindFirstObjectByType<SparkLevelLayoutLoader>();
            manager=FindFirstObjectByType<LevelGamePlayManager>();
            if (loader && manager && pins?.Length==54) return;
            Debug.LogError("[LEVEL 57] Missing manager, loader, or 54 terminals.",this);
            enabled=false;
        }
        private void Update()
        {
            if (loader.ActiveLayout!=layout || loader.ActiveBindings==null) return;
            solver=manager.ElectricalSolver;
            circuit=manager.CircuitSystem;
            if (!solver || !circuit) return;
            if (bindings!=loader.ActiveBindings) {bindings=loader.ActiveBindings;topology=-1;}
            if (topology!=circuit.TopologyVersion) {topology=circuit.TopologyVersion;wiring=InspectWiring();}
            ReadPower();
            if (stage==0 && ready && guardOK) Award();
            if ((primary||secondary) && fault==null && (!ready||!guardOK))
                Trip(!GridFeedback?"GRID VOLTAGE FEEDBACK LOST · W24":!guardOK?"EMERGENCY STOP OR INTERLOCK":wiring??"POWER RAIL INVALID");
            stepClock+=Mathf.Min(Time.deltaTime,.12f);
            while(stepClock>=.01f) {stepClock-=.01f;Step(.01f);}
            Present();
        }

        private SparkTerminal Net(SparkTerminal pin)
        {
            while(nets.TryGetValue(pin,out var parent)) pin=parent;
            return pin;
        }
        private string InspectWiring()
        {
            circuit.CopyConnections(wires);
            nets.Clear();
            verified.Clear();
            foreach(var wire in wires)
            {
                if (wire.Kind!=SparkConnectionKind.Wire) continue;
                var a=Net(wire.A);var b=Net(wire.B);
                if(a!=b) nets[b]=a;
                int i=Array.IndexOf(pins,wire.A),j=Array.IndexOf(pins,wire.B);
                if(i>=0||j>=0) verified.Add((Mathf.Min(i,j),Mathf.Max(i,j)));
            }
            var distinct=new HashSet<SparkTerminal>();
            foreach(int index in new[]{0,2,11,13,15,17,19,21,23})
                if(!distinct.Add(Net(pins[index]))) return "Separate DC rails, neutral, bus phases and incoming B phases.";
            for(int i=0;i<Wires.Length;i++)
            {
                var (a,b)=Wires[i];
                if(!Routed(i)) return $"Connect W{i+1:00} · {pins[a].name} → {pins[b].name}.";
            }
            return verified.Count==Wires.Length?null:"Remove extra wires from the Level 57 sockets.";
        }
        private void ReadPower()
        {
            var result=solver.LatestSolveResult;
            bool Rail(int a,int b)=>result!=null&&result.TryGetVoltage(pins[a],pins[b],out var v)&&v is >=4.75f and <=5.25f;
            ready=powered&&wiring==null&&source.IsOutputActive&&!source.IsOverloaded&&Rail(1,3)&&Rail(4,5)&&Rail(6,7)&&Rail(8,9);
            guardOK=!stopped&&Routed(33)&&emergency.IsClosed&&Rail(53,3);
        }
        private static void Contact(SparkSwitch value,bool closed)
        {
            if(value.IsClosed!=closed)value.SetState(closed?SparkSwitchState.Closed:SparkSwitchState.Open);
        }
        private void SetPoles(bool closed)
        {
            Contact(poleA,closed);
            Contact(poleB,closed);
            Contact(poleC,closed);
            parallelHold=shareHold=syncHold=0;
        }

        public void Power()
        {
            powered=!powered;
            if(!powered && (primary||secondary)) Trip("MAIN POWER REMOVED");
            source.SetOutput(powered,default);
        }
        public void Emergency()
        {
            stopped=!stopped;
            Contact(emergency,!stopped);
            if(stopped && (primary||secondary)) Trip("EMERGENCY STOP");
        }
        public void Calibrate()
        {
            if(ready&&guardOK&&fault==null&&!primary&&!secondary) calibrated=true;
        }
        public void StartGrid()
        {
            if(!ready||!guardOK||!calibrated||fault!=null||island) return;
            primary=true;
            Contact(isolation,true);
        }
        public void EnableB()
        {
            if(!Reference || stage<2) return;
            secondary=true;
        }
        public void CheckSync()
        {
            notice=CanClose?"All tolerances held. Close the contactor.":"Waiting for phase, frequency and voltage to remain in the synchronization window.";
        }
        public void LockPll()
        {
            if(Reference&&secondary&&!Closed) sync=true;
        }
        public void AutoSync()
        {
            if(!Reference||!secondary||Closed)return;
            sync=matchHz=matchV=true;
            notice=null;
        }
        public void MatchHz() {if(Reference&&secondary&&!Closed)matchHz=true;}
        public void MatchVoltage() {if(Reference&&secondary&&!Closed)matchV=true;}
        public void Close()
        {
            if(!CanClose){notice="Closing inhibited by the synchronization relay.";return;}
            SetPoles(true);
            notice=null;
        }
        public void Open()
        {
            if(Closed&&!island) {SetPoles(false);sync=false; notice=null;}
        }
        public void AutoShare() {if(Closed&&!island){autoShare=true;notice=null;}}
        public void ManualShare() {if(Closed&&!island){autoShare=false;notice=null;}}
        public void More()
        {
            if(!Closed||island)return;
            if(autoShare)load=Mathf.Min(2.25f,load+.15f);
            else trim=Mathf.Min(3.5f,trim+.3f);
        }
        public void Less()
        {
            if(!Closed||island)return;
            if(autoShare)load=Mathf.Max(.75f,load-.15f);
            else trim=Mathf.Max(-3.5f,trim-.3f);
        }
        public void LoadStep()
        {
            if(stage!=3||!Closed||island)return;
            load=2.05f;
            loadStep=true;
            shareHold=0;
        }
        public void Island()
        {
            if(stage!=4||!Shared||!Routed(31)||!Routed(32))return;
            Contact(isolation,false);
            island=true;
            islandHold=0;
            magnitudeMinimum=ac.BusVolts;
            load=1.15f;
            notice=null;
        }
        public void Restore()
        {
            if(stage!=4||!island||islandHold<2.5f)return;
            // Open the local tie before restoring A. The remaining source
            // cannot export through the open upstream isolation contact.
            SetPoles(false);
            Contact(isolation,true);
            island=false;
            restoring=true;
            sync=matchHz=matchV=false;
            ac.AngleB=Mathf.Repeat(ac.AngleB+26,360);
            notice=null;
        }
        public void PhaseView() => phaseView=!phaseView;
        public void FaultTest()
        {
            if(stage==5&&Closed&&Shared&&!repaired) {armed=true;notice=null;}
        }
        public void ResetFault()
        {
            if(fault==null||!ready||!guardOK||!GridFeedback)return;
            repaired|=faultObserved;
            fault=null;
            armed=false;
            calibrated=false;
            notice=null;
        }
        public void Final()
        {
            if(stage==5&&repaired&&Shared) {finalRequested=true;shareHold=0;notice=null;}
        }
        private void Trip(string reason)
        {
            faultObserved|=stage==5&&armed&&!GridFeedback;
            fault=reason;
            SetPoles(false);
            Contact(isolation,false);
            primary=secondary=island=sync=calibrated=finalRequested=false;
        }

        private void Step(float dt)
        {
            bool a=primary&&isolation.IsClosed;
            bool b=secondary&&Closed;
            ac.Step(dt,a,b,island,sync,matchHz,matchV,autoShare,trim,load,b&&!island&&stage>=3);
            if(fault!=null)return;
            if(ac.CurrentA>.58f||ac.CurrentB>.58f||ac.Circulating>.36f)
            {Trip("PHASE CURRENT OR CIRCULATING CURRENT LIMIT");return;}
            referenceHold=Reference&&StableBus?referenceHold+dt:0;
            syncHold=Window?syncHold+dt:0;
            parallelHold=Closed&&!island&&StableBus?parallelHold+dt:0;
            shareHold=Shared?shareHold+dt:0;
            if(island)
            {
                magnitudeMinimum=Mathf.Min(magnitudeMinimum,ac.BusVolts);
                islandHold=!a&&b&&StableBus&&ac.LoadWatts>.7f?islandHold+dt:0;
            }
            if(stage==1&&referenceHold>=1.2f) Award();
            else if(stage==2&&parallelHold>=1.1f) Award();
            else if(stage==3&&loadStep&&shareHold>=1.8f) Award();
            else if(stage==4&&restoring&&CanClose) {restoring=false;Award();}
            else if(stage==5&&repaired&&finalRequested&&shareHold>=1.5f) Award();
        }
        private void Award()
        {
            if(stage>=Gates.Length||!bindings.SetRuntimeComponentBinding(Gates[stage],source))return;
            stage++;
            manager.QueueEvaluation();
        }

        private string Prompt()
        {
            if(fault!=null)return $"PROTECTION · {fault} · "+(ready&&guardOK?"RESET FAULT":wiring??"Release the stop and restore rails.");
            if(wiring!=null)return wiring;
            if(!guardOK)return "Release the emergency stop and check W34.";
            if(!ready)return "Turn on the DC supply and check the rail wiring.";
            if(!calibrated)return "Select CALIBRATE with both inverters off.";
            if(!primary)return "Select START GRID A.";
            if(armed&&!repaired)return "Undo W24 now to test loss of the grid-voltage reference.";
            if(stage==6)return "CERTIFIED · Final parallel sharing verified after W24 recovery.";
            if(island)return islandHold>=2.5f?"Island stable on inverter B. Select RESTORE GRID.":"Inverter B is forming the island. Observe essential-load voltage.";
            if(!secondary&&stage>=2)return "Select ENABLE INVERTER B.";
            if(!Closed&&stage>=2)return CanClose?"Synchronization window held. Select CLOSE CONTACTOR.":
                notice??"Select AUTO SYNCHRONIZE. Wait for the relay window.";
            return stage switch {
                1=>"Measuring the stable 50 Hz reference.",
                2=>"Monitoring the newly parallel sources.",
                3=>"Select AUTO SHARE and apply LOAD STEP. Hold 40–60% power share.",
                4=>"Select ISLAND MODE once sharing is stable.",
                5 when repaired=>"Select FINAL CERTIFY after parallel sharing stabilizes.",
                5=>"Select FAULT TEST, then undo W24.",
                _=>"Waiting for verified circuit state."
            };
        }
        private void Present()
        {
            statusText.text=Prompt();
            meterText.text=$"BUS {ac.BusVolts:F2} V RMS  {ac.BusHz:F2} Hz  ·  ΔF {ac.FrequencyB-ac.FrequencyA:+0.00;-0.00;0.00} Hz  ·  Δφ {ac.Delta:+0.0;-0.0;0.0}°  ·  ΔV {ac.Mismatch:P1}";
            detailText.text=$"A {ac.WattsA:F2} W / {ac.CurrentA:F2} A  ·  B {ac.WattsB:F2} W / {ac.CurrentB:F2} A  ·  LOAD {ac.LoadWatts:F2} W  ·  SHARE {ac.Share:P0} / {1-ac.Share:P0}  ·  {manager.SatisfiedTargetCount}/6";
            calibrationText.text=phaseView?$"A {Phase(0):+0.00;-0.00;0.00} V   B {Phase(-120):+0.00;-0.00;0.00} V   C {Phase(120):+0.00;-0.00;0.00} V":
                $"Q {ac.Vars:F2} var  ·  PF {ac.PowerFactor:F2}  ·  LOSS {ac.LossWatts:F3} W  ·  BALANCE {ac.Residual:F4} W  ·  {(Closed?"TIE CLOSED":CanClose?"READY TO CLOSE":"TIE OPEN")}";
            float Phase(float angle)=>ac.BusVolts*Mathf.Sqrt(2)*Mathf.Sin((ac.AngleA+angle)*Mathf.Deg2Rad);
            if(controllerDisplay)controllerDisplay.text=$"{(island?"ISLAND / B":Closed?"PARALLEL":"REFERENCE")}\n{ac.BusVolts:F2} V   {ac.BusHz:F2} Hz";
            if(qualityDisplay)qualityDisplay.text=$"A {ac.WattsA:F2} W  B {ac.WattsB:F2} W\nPF {ac.PowerFactor:F2}";
            if(rig)rig.Apply(ac.Delta,Closed,island,stopped,ac.MotorRpm,ac.LoadWatts,Time.deltaTime);
            if(plot)plot.Show(ac.BusVolts,ac.Delta,phaseView);
            powerButton.interactable=guardButton.interactable=phaseButton.interactable=true;
            guardButton.GetComponentInChildren<Text>().text=stopped?"RELEASE E-STOP":"E-STOP";
            calibrateButton.interactable=ready&&guardOK&&fault==null&&!calibrated&&!primary;
            startButton.interactable=ready&&guardOK&&calibrated&&!primary&&fault==null;
            enableButton.interactable=Reference&&stage>=2&&!secondary;
            checkButton.interactable=pllButton.interactable=hzButton.interactable=voltageButton.interactable=
                syncButton.interactable=Reference&&secondary&&!Closed;
            closeButton.interactable=Reference&&secondary&&!Closed;
            openButton.interactable=Closed&&!island&&fault==null;
            autoButton.interactable=manualButton.interactable=moreButton.interactable=lessButton.interactable=Closed&&!island;
            stepButton.interactable=stage==3&&Closed&&!island;
            islandButton.interactable=stage==4&&Shared&&!restoring;
            restoreButton.interactable=stage==4&&island&&islandHold>=2.5f;
            faultButton.interactable=stage==5&&Shared&&!repaired&&!armed;
            resetButton.interactable=fault!=null&&ready&&guardOK&&GridFeedback;
            finalButton.interactable=stage==5&&repaired&&Shared&&!finalRequested;
        }
        private void OnDisable()
        {
            if(bindings!=null)foreach(string gate in Gates)bindings.ClearRuntimeComponentBinding(gate);
        }
    }
}

