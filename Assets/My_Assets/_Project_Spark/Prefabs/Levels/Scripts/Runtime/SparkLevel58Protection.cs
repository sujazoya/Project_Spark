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
    public sealed class SparkLevel58Protection : MonoBehaviour
    {
        public static readonly string[] Gates={"L58_Wired","L58_Coordinated","L58_Overload","L58_Short","L58_Backup","L58_Recovered"};
        public static readonly (int a,int b)[] Wires={(0,1),(2,3),(0,4),(2,5),(0,6),(2,7),(0,8),(2,9),(10,11),(12,13),(14,15),(16,17),(18,19),(20,21),(22,23),(24,25),(17,26),(19,27),(21,28),(17,29),(19,30),(21,31),(17,32),(19,33),(21,34),(35,36),(37,38),(39,40),(41,42),(43,44),(45,46),(47,48),(49,50),(51,52),(53,54),(17,55),(56,57),(58,59)};
        public SparkLevelLayout layout;
        public SparkPowerSupply source;
        public SparkSwitch[] contacts;
        public SparkSwitch emergency;
        public SparkTerminal[] pins;
        public SparkLevel58VisualRig rig;
        public TMP_Text controllerDisplay,qualityDisplay;
        public Text statusText,meterText,detailText,calibrationText;
        public Button powerButton,guardButton,calibrateButton,pickupButton,curveButton,tmsButton,highButton,verifyButton,armButton;
        public Button closeButton,overloadButton,shortButton,backupButton,clearButton,repairButton,resetButton,sensorButton,finalButton;

        private enum Test { None, Overload, Short, Backup }
        private enum Breaker { Open, Closing, Closed, Opening, Tripped, FailedToOpen, LockedOut }
        private sealed class Zone
        {
            public Breaker State;
            public float Delay, Current, Filtered, Progress, PickupAt=-1, RequestAt=-1, OpenAt=-1;
            public bool Closed => State is Breaker.Closed or Breaker.Opening or Breaker.FailedToOpen;
            public bool BreakerReady => State == Breaker.Closed;
        }

        private readonly Zone[] zones={new(),new(),new(),new()};
        private readonly List<SparkCircuitConnection> wires=new();
        private readonly HashSet<(int,int)> verified=new();
        private readonly Dictionary<SparkTerminal,SparkTerminal> nets=new();
        private SparkLevelLayoutLoader loader;
        private LevelGamePlayManager manager;
        private SparkLevelSceneBindings bindings;
        private SparkElectricalSolver solver;
        private SparkCircuitSystem circuit;
        private string wiring,latched,notice="Wire the protection network.",history="No protection events.";
        private int topology=-1,stage;
        private float clock,stepClock,busVoltage,backupTime,clearTime;
        private bool ready,powered=true,guardClosed=true,calibrated,armed,coordinated;
        private bool sensorTest,sensorObserved,repeatPassed,damaged,reviewed;
        private bool correctPickup,shortDelay,standardCurve,selectiveHigh;
        private Test activeTest;
        private bool AllClosed => Array.TrueForAll(zones,z=>z.BreakerReady);
        private bool Healthy => ready&&guardClosed&&latched==null&&calibrated&&coordinated&&armed;
        private bool TestReady => Healthy&&AllClosed&&activeTest==Test.None;
        private bool SafeToClose => Healthy&&activeTest==Test.None&&!damaged;
        private float Pick(int i)=>correctPickup?(i==0?.62f:.34f):.12f;
        private float Multiplier(int i)=>i==0?.95f:shortDelay?.075f:.28f;
        private float HighSet(int i)=>i==0?6.5f:selectiveHigh?1.55f:.28f;

        private void Start()
        {
            loader=FindFirstObjectByType<SparkLevelLayoutLoader>();
            manager=FindFirstObjectByType<LevelGamePlayManager>();
            if (loader && manager && pins?.Length==60 && contacts?.Length==12) return;
            Debug.LogError("[LEVEL 58] Missing level loader, manager, sockets or breaker poles.",this);
            enabled=false;
        }

        private void Update()
        {
            if(loader.ActiveLayout!=layout || loader.ActiveBindings==null)return;
            solver=manager.ElectricalSolver;
            circuit=manager.CircuitSystem;
            if(!solver || !circuit)return;
            if(bindings!=loader.ActiveBindings){bindings=loader.ActiveBindings;topology=-1;}
            if(topology!=circuit.TopologyVersion){topology=circuit.TopologyVersion;wiring=CheckWires();}
            var result=solver.LatestSolveResult;
            bool Rail(int a,int b)=>result!=null&&result.TryGetVoltage(pins[a],pins[b],out var v)&&v is >=4.7f and <=5.3f;
            // The 5 V rails are solved by Project Spark. AC current and phase live in the level's RMS model.
            ready=powered&&source.IsOutputActive&&!source.IsOverloaded&&wiring==null&&
                Rail(1,3)&&Rail(4,5)&&Rail(6,7)&&Rail(8,9)&&Rail(59,3);
            if(stage==0&&ready)Award();
            if(!Routed(15)&&latched==null&&stage>=1&&(sensorTest||zones[2].Closed))
            {
                sensorObserved|=sensorTest;
                calibrated=armed=false;
                latched="CTB SIGNAL LOST · W16";
                Request(2,"R2 invalid CTB feedback");
            }
            else if((!guardClosed||!powered||!ready&&latched==null)&&Array.Exists(zones,z=>z.Closed||z.State==Breaker.Closing))
            {
                calibrated=armed=false;
                latched=guardClosed?wiring??"CONTROL POWER LOST":"SAFETY GUARD OPEN";
                for(int i=0;i<4;i++) Open(i,Breaker.LockedOut);
            }
            stepClock+=Mathf.Min(Time.deltaTime,.12f);
            while(stepClock>=.01f){stepClock-=.01f;Step(.01f);}
            Present();
        }

        private SparkTerminal Root(SparkTerminal pin)
        {
            while(nets.TryGetValue(pin,out var parent))pin=parent;
            return pin;
        }
        private static (int,int) Pair(int a,int b)=>(Math.Min(a,b),Math.Max(a,b));
        private bool Routed(int i)=>verified.Contains(Pair(Wires[i].a,Wires[i].b));
        private string CheckWires()
        {
            circuit.CopyConnections(wires);
            verified.Clear();
            nets.Clear();
            foreach(var wire in wires)
            {
                if(wire.Kind!=SparkConnectionKind.Wire)continue;
                int a=Array.IndexOf(pins,wire.A),b=Array.IndexOf(pins,wire.B);
                if(a<0&&b<0)continue;
                verified.Add(Pair(a,b));
                var one=Root(wire.A);var two=Root(wire.B);
                if(one!=two)nets[two]=one;
            }
            if(Root(pins[0])==Root(pins[2]))return "Separate the +5 V and return rails.";
            for(int i=0;i<Wires.Length;i++)
                if(!Routed(i))return $"Connect W{i+1:00} · {pins[Wires[i].a].name} → {pins[Wires[i].b].name}.";
            return verified.Count==Wires.Length?null:"Remove extra or crossed wires.";
        }

        private static float TripTime(float amps,float pickup,float tms,bool standard)
        {
            if(amps>=pickup*1.001f)
            {
                float multiple=amps/pickup;
                return standard?.14f*tms/(Mathf.Pow(multiple,.02f)-1):13.5f*tms/(multiple-1);
            }
            return float.PositiveInfinity;
        }

        private void SampleNetwork(float dt)
        {
            float gA=zones[1].Closed?1/(activeTest==Test.Overload?3.3f:12f):0;
            float gB=zones[2].Closed?1/(activeTest is Test.Short or Test.Backup?.62f:10f):0;
            float gC=zones[3].Closed?1/15f:0;
            // Three-phase source RMS with shared equivalent source impedance and separate feeder load impedances.
            float conductance=gA+gB+gC;
            busVoltage=zones[0].Closed?2.4f/(1+.15f*conductance):0;
            zones[1].Current=busVoltage*gA;
            zones[2].Current=busVoltage*gB;
            zones[3].Current=busVoltage*gC;
            zones[0].Current=busVoltage*conductance;
            for(int i=0;i<zones.Length;i++)
            {
                var z=zones[i];
                float secondary=Mathf.Min(z.Current/20,.25f);
                z.Filtered=Mathf.MoveTowards(z.Filtered,secondary*20,dt*90);
            }
        }
        private void Step(float dt)
        {
            clock+=dt;
            for(int i=0;i<zones.Length;i++)
            {
                var zone=zones[i];
                if(zone.State is not (Breaker.Closing or Breaker.Opening))continue;
                zone.Delay-=dt;
                if(zone.Delay>0)continue;
                if(zone.State==Breaker.Closing)
                {
                    if(SafeToClose){zone.State=Breaker.Closed;ApplyPoles(i,true);}
                    else Open(i,Breaker.LockedOut);
                }
                else if(i==2&&activeTest==Test.Backup&&latched==null)
                {
                    zone.State=Breaker.FailedToOpen;
                    damaged=true;
                    Log("Q2 failed to open · backup timer started");
                }
                else Open(i,Breaker.Tripped);
            }
            SampleNetwork(dt);
            bool backup=damaged&&zones[2].Closed&&zones[2].Filtered>Pick(2)&&zones[0].Closed;
            backupTime=backup?backupTime+dt:0;
            if(backupTime>=.42f&&zones[0].State==Breaker.Closed)Request(0,"R0 breaker failure backup");
            if(Healthy&&Routed(15))
                for(int i=0;i<zones.Length;i++) UpdateRelay(i,dt);
            VerifyClearance();
        }
        private void UpdateRelay(int i,float dt)
        {
            var zone=zones[i];
            if(zone.State!=Breaker.Closed)return;
            if(zone.Filtered<Pick(i))
            {
                zone.Progress=Mathf.Max(0,zone.Progress-dt);
                return;
            }
            if(zone.PickupAt<0)zone.PickupAt=clock;
            float delay=zone.Filtered>=HighSet(i)?.055f:TripTime(zone.Filtered,Pick(i),Multiplier(i),standardCurve);
            zone.Progress+=dt/Mathf.Max(.055f,delay);
            if(zone.Progress>=1)Request(i,zone.Filtered>=HighSet(i)?$"R{i} high-set":$"R{i} inverse-time");
        }

        private void Request(int i,string reason)
        {
            var zone=zones[i];
            if(zone.State is not (Breaker.Closed or Breaker.Closing))return;
            zone.RequestAt=clock;
            zone.State=Breaker.Opening;
            zone.Delay=.085f;
            Log(reason+$" → Q{i} trip request");
        }
        private void Open(int i,Breaker state)
        {
            bool moved=zones[i].Closed;
            zones[i].State=state;
            zones[i].OpenAt=clock;
            ApplyPoles(i,false);
            if(moved)Log($"Q{i} opened · {clock-RequestTime(i):F2} s since request");
        }
        private float RequestTime(int zone)=>zones[zone].RequestAt>=0?zones[zone].RequestAt:clock;
        private void ApplyPoles(int i,bool on)
        {
            for(int p=0;p<3;p++)
            {
                var contact=contacts[i*3+p];
                if(contact.IsClosed!=on)contact.SetState(on?SparkSwitchState.Closed:SparkSwitchState.Open);
            }
        }
        private bool Selective(int zone)
        {
            if(zones[zone].OpenAt<clearTime||zones[zone].RequestAt<clearTime||zones[zone].Current>.015f||busVoltage<2.1f)return false;
            if(zones[zone].State!=Breaker.Tripped||zones[zone].PickupAt<clearTime)return false;
            for(int i=0;i<4;i++)if(i!=zone&&(!zones[i].Closed||zones[i].Current<.08f))return false;
            return true;
        }
        private void VerifyClearance()
        {
            if(reviewed)return;
            if(activeTest==Test.Overload&&Selective(1)&&stage==2)
            {
                reviewed=true;Award();Log("Selective overload · Q1 isolated · B/C healthy");
            }
            else if(activeTest==Test.Short&&Selective(2))
            {
                reviewed=true;
                if(stage==3)Award();
                else if(stage==5&&sensorObserved)repeatPassed=true;
                Log("Selective short clearance · Q2 isolated · A/C healthy");
            }
            else if(activeTest==Test.Backup&&damaged&&zones[2].State==Breaker.FailedToOpen&&
                zones[0].State==Breaker.Tripped&&zones[0].RequestAt-zones[2].RequestAt>=.42f&&zones[0].Current<.015f)
            {
                reviewed=true;Award();Log("Q0 backup interrupted persistent B fault · Q2 held closed");
            }
        }

        private void Log(string message)
        {
            history=$"{clock:F2}s  {message}";
            Debug.Log("[LEVEL 58] "+history,this);
        }
        private void Award()
        {
            if(stage>=Gates.Length||!bindings.SetRuntimeComponentBinding(Gates[stage],source))return;
            stage++;
            manager.QueueEvaluation();
        }
        private void SettingsChanged()
        {
            coordinated=armed=false;
            notice="Verify the relay settings, then arm protection.";
        }
        public void Power(){powered=!powered;source.SetOutput(powered,default);}
        public void Guard()
        {
            guardClosed=!guardClosed;
            emergency.SetState(guardClosed?SparkSwitchState.Closed:SparkSwitchState.Open);
        }
        public void Calibrate()
        {
            if(!ready||latched!=null||AllClosed)return;
            calibrated=true;
            notice="CT0 / CTA / CTB / CTC calibrated · 20:1";
        }
        public void Pickup(){correctPickup=!correctPickup;SettingsChanged();}
        public void Curve(){standardCurve=!standardCurve;SettingsChanged();}
        public void Tms(){shortDelay=!shortDelay;SettingsChanged();}
        public void High(){selectiveHigh=!selectiveHigh;SettingsChanged();}
        public void Verify()
        {
            float overloadTime=TripTime(.68f,Pick(1),Multiplier(1),standardCurve);
            float upstream=TripTime(3.2f,Pick(0),Multiplier(0),standardCurve);
            coordinated=ready&&calibrated&&correctPickup&&shortDelay&&standardCurve&&selectiveHigh&&overloadTime<1.3f&&upstream>.70f;
            notice=coordinated?"Coordination verified · arm relays and close Q0–Q3.":"Choose coordinated pickup, standard inverse, feeder TMS 0.075 and high-set 1.55 A.";
        }
        public void Arm(){if(ready&&calibrated&&coordinated&&latched==null)armed=true;}
        public void Close()
        {
            if(!SafeToClose)return;
            foreach(var z in zones)
                if(!z.Closed){z.State=Breaker.Closing;z.Delay=.12f;}
        }
        public void Overload(){if(TestReady&&stage==2)Begin(Test.Overload);}
        public void Short(){if(TestReady&&(stage==3||(stage==5&&sensorObserved&&!repeatPassed)))Begin(Test.Short);}
        public void Backup(){if(TestReady&&stage==4)Begin(Test.Backup);}
        private void Begin(Test test)
        {
            clearTime=clock;
            backupTime=0;
            reviewed=false;
            activeTest=test;
            foreach(var z in zones){z.PickupAt=z.RequestAt=z.OpenAt=-1;z.Progress=0;}
            Log(test+" applied");
        }
        public void Clear()
        {
            activeTest=Test.None;
            foreach(var z in zones){z.Progress=0;z.PickupAt=-1;}
        }
        public void Repair()
        {
            if(activeTest!=Test.None||zones[0].Closed||!damaged)return;
            Open(2,Breaker.Tripped);
            damaged=false;
            Log("Q2 mechanism serviced and contacts opened");
        }
        public void Reset()
        {
            if(!ready||activeTest!=Test.None||damaged)return;
            sensorTest=false;
            latched=null;
            foreach(var z in zones)
            {
                if(z.State is Breaker.Tripped or Breaker.LockedOut)z.State=Breaker.Open;
                z.Progress=0;
            }
            notice=calibrated?"Protection reset. Close Q0–Q3.":"Calibrate CTs, verify settings and rearm.";
        }
        public void Sensor(){if(TestReady&&stage==5&&!sensorObserved)sensorTest=true;}
        public void Final(){if(TestReady&&stage==5&&repeatPassed)Award();}

        private void Present()
        {
            string state=stage==6?"CERTIFIED · Selectivity, backup and sensor recovery verified.":
                latched!=null?$"{latched} · "+(ready?"Reset, recalibrate and rearm.":"Restore the connection before reset."):
                wiring!=null?wiring:!ready?"Close the safety guard and restore 5 V power.":
                damaged?"Clear the test, service Q2, reset and close Q0–Q3.":
                activeTest!=Test.None?reviewed?"Test verified · clear, reset and close Q0–Q3.":"Fault active · watch relay timing and breaker feedback.":
                sensorTest?"Disconnect W16 · CTB OUT to R2 I.":!AllClosed?notice:
                stage switch {2=>"Apply A OVERLOAD.",3=>"Apply B SHORT.",4=>"Apply Q2 FAILURE.",
                    5=>repeatPassed?"Select CERTIFY.":sensorObserved?"Repeat B SHORT.":"Select W16 TEST.",_=>notice};
            if(stage==1&&Healthy&&AllClosed&&zones[3].Current>.08f)Award();
            if(!reviewed&&activeTest!=Test.None&&clock-clearTime>6&&latched==null)
            {
                latched="FAULT CLEARANCE TIMEOUT";
                Open(0,Breaker.LockedOut);
            }
            statusText.text=state;
            meterText.text=$"BUS {busVoltage:F2} V RMS  {(zones[0].Closed?50:0)} Hz  ·  I0 {zones[0].Current:F2} A  ·  IA {zones[1].Filtered:F2} A  ·  IB {(Routed(15)?zones[2].Filtered.ToString("F2"):"INVALID")}  ·  IC {zones[3].Filtered:F2} A";
            detailText.text=$"Q0 {zones[0].State}  /  Q1 {zones[1].State}  /  Q2 {zones[2].State}  /  Q3 {zones[3].State}    ·    {manager.SatisfiedTargetCount}/6";
            calibrationText.text=history;
            if(controllerDisplay)controllerDisplay.text=$"{busVoltage:F2} V RMS\n{(zones[0].Closed?50:0)} Hz";
            if(qualityDisplay)qualityDisplay.text=Routed(15)?$"CTB {zones[2].Filtered:F2} A":"CTB INVALID";
            if(rig)rig.Apply(zones[0].Closed,zones[1].Closed,zones[2].Closed,zones[3].Closed,guardClosed,zones[2].Current,Time.deltaTime);
            powerButton.interactable=guardButton.interactable=true;
            calibrateButton.interactable=ready&&latched==null&&!AllClosed&&!calibrated;
            pickupButton.interactable=curveButton.interactable=tmsButton.interactable=highButton.interactable=ready&&!AllClosed&&activeTest==Test.None;
            pickupButton.GetComponentInChildren<Text>().text=$"PICKUP {Pick(1):F2} A";
            curveButton.GetComponentInChildren<Text>().text=standardCurve?"STANDARD INVERSE":"VERY INVERSE";
            tmsButton.GetComponentInChildren<Text>().text=$"TMS {Multiplier(1):F3}";
            highButton.GetComponentInChildren<Text>().text=$"HIGH-SET {HighSet(1):F2} A";
            verifyButton.interactable=ready&&calibrated&&!AllClosed&&latched==null;
            armButton.interactable=ready&&calibrated&&coordinated&&!armed&&latched==null;
            closeButton.interactable=SafeToClose&&!AllClosed&&Array.TrueForAll(zones,z=>z.State is Breaker.Open or Breaker.Closed);
            overloadButton.interactable=TestReady&&stage==2;
            shortButton.interactable=TestReady&&(stage==3||(stage==5&&sensorObserved&&!repeatPassed));
            backupButton.interactable=TestReady&&stage==4;
            clearButton.interactable=activeTest!=Test.None&&(reviewed||latched!=null);
            repairButton.interactable=activeTest==Test.None&&!zones[0].Closed&&damaged;
            resetButton.interactable=ready&&activeTest==Test.None&&!damaged&&(latched!=null||Array.Exists(zones,z=>z.State==Breaker.Tripped));
            sensorButton.interactable=TestReady&&stage==5&&!sensorTest&&!sensorObserved;
            finalButton.interactable=TestReady&&stage==5&&repeatPassed;
        }
        private void OnDisable()
        {
            if(bindings!=null)foreach(string gate in Gates)bindings.ClearRuntimeComponentBinding(gate);
        }
    }
}


