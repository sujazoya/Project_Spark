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
    public sealed class SparkLevel62VoltageRestoration : MonoBehaviour
    {
        public static readonly string[] Gates={"L62_Wired","L62_Commissioned","L62_Sag","L62_Quality","L62_Inrush","L62_Recovered"};
        public static readonly (int a,int b)[] Wires={(0,1),(2,3),(0,4),(2,5),(0,6),(2,7),(0,8),(2,9),(10,11),(12,13),(14,15),(16,17),(18,19),(20,21),(17,22),(19,23),(21,24),(25,26),(27,28),(29,30),(31,32),(33,34),(35,36),(32,37),(34,38),(36,39),(40,41),(42,43),(44,45),(45,46),(43,47),(48,49),(50,51),(17,52),(19,53),(21,54),(26,55),(28,56),(30,57),(58,59),(60,61),(62,63),(26,64),(28,65),(30,66),(26,67),(68,69),(70,71),(72,73),(74,75),(76,77),(78,79),(80,81),(82,83)};

        public SparkLevelLayout layout;
        public SparkPowerSupply source;
        public SparkSwitch[] contacts;
        public SparkSwitch emergency;
        public SparkTerminal[] pins;
        public SparkLevel62VisualRig rig;
        public SparkLevel62Plot plot;
        public TMP_Text controllerDisplay,qualityDisplay;
        public Text statusText,meterText,detailText,calibrationText;
        public Button powerButton,guardButton,mainButton,calibrateButton,storeButton,prechargeButton,pllButton,enableButton,
            sagButton,swellButton,unbalanceButton,motorButton,faultButton,resetButton,finalButton;

        private enum Event { Normal, Sag, Swell, Unbalance, Motor }
        private const float Capacitance=4f, StartVoltage=5.2f, MinimumEnergy=5.2f, LinkCapacitance=.09f;
        private readonly float[] input=new float[3], output=new float[3], injected=new float[3];
        private readonly List<SparkCircuitConnection> edges=new();
        private readonly HashSet<(int,int)> wired=new();
        private SparkLevelLayoutLoader loader;
        private LevelGamePlayManager manager;
        private SparkLevelSceneBindings bindings;
        private SparkElectricalSolver solver;
        private SparkCircuitSystem circuit;
        private int topology=-1,stage;
        private float accumulator,hold,gridAngle,phase,pllHz,pllHold,link,storedEnergy=.5f*Capacitance*StartVoltage*StartVoltage,
            temp=24,elapsed,rmsCurrent,heavySpeed,motorSpeed,converterPower;
        private bool power=true,guard=true,ready,main,calibrated,storeChecked,precharge,pll,active,bypass,
            swellPassed,faultTest,sensorObserved,resetDone,repeatPassed,certify,rideThroughFailed;
        private string wiring,fault;
        private Event scenario;
        private bool Feedback=>Routed(53);
        private bool Locked=>pll&&pllHold>=.5f;
        private bool Healthy=>ready&&active&&Locked&&Feedback&&link>4.45f&&storedEnergy>MinimumEnergy;
        private bool VoltageValid=>output[0] is >=.95f and <=1.05f&&output[1] is >=.95f and <=1.05f&&output[2] is >=.95f and <=1.05f;
        private float StoreVoltage=>Mathf.Sqrt(2*storedEnergy/Capacitance);
        private float Unbalance(float[] value)
        {
            float mean=(value[0]+value[1]+value[2])/3;
            if(mean<.01f)return 0;
            return Mathf.Max(Mathf.Abs(value[0]-mean),Mathf.Max(Mathf.Abs(value[1]-mean),Mathf.Abs(value[2]-mean)))/mean;
        }
        private void Start()
        {
            loader=FindFirstObjectByType<SparkLevelLayoutLoader>();
            manager=FindFirstObjectByType<LevelGamePlayManager>();
            if(loader&&manager&&pins?.Length==84&&contacts?.Length==9)return;
            Debug.LogError("[LEVEL 62] Missing layout services, sockets or contactors.",this);
            enabled=false;
        }
        private static (int,int) Pair(int a,int b)=>(Math.Min(a,b),Math.Max(a,b));
        private bool Routed(int i)=>wired.Contains(Pair(Wires[i].a,Wires[i].b));
        private string Inspect()
        {
            wired.Clear();
            circuit.CopyConnections(edges);
            foreach(var e in edges)
            {
                if(e.Kind!=SparkConnectionKind.Wire)continue;
                int a=Array.IndexOf(pins,e.A),b=Array.IndexOf(pins,e.B);
                if(a>=0||b>=0)wired.Add(Pair(a,b));
            }
            for(int i=0;i<Wires.Length;i++)
                if(!Routed(i))return $"Connect W{i+1:00} · {pins[Wires[i].a].name} → {pins[Wires[i].b].name}.";
            return wired.Count==Wires.Length?null:"Remove extra wires from the DVR sockets.";
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
            bool controls=power&&guard&&source.IsOutputActive&&!source.IsOverloaded&&Rail(1,3)&&Rail(4,5)&&Rail(6,7)&&Rail(8,9)&&Rail(81,3);
            ready=controls&&wiring==null;
            if(stage==0&&ready)Award();
            int expectedWithoutSensor=Wires.Length-1;
            bool missingFeedbackOnly=!Feedback&&wired.Count==expectedWithoutSensor;
            for(int i=0;i<expectedWithoutSensor&&missingFeedbackOnly;i++)missingFeedbackOnly=Routed(i);
            if(main&&(!controls||wiring!=null&&!missingFeedbackOnly))
            {
                main=false;
                SetPoles(0,false);
                Trip(!guard?"GUARD OPEN":wiring??"CONTROL POWER LOST",false);
            }
            else if(main&&!Feedback&&(active||faultTest)&&fault==null)
            {
                sensorObserved|=faultTest;
                Trip("INJECTION FEEDBACK LOST · W54",true);
            }
            accumulator+=Mathf.Min(Time.deltaTime,.1f);
            while(accumulator>=.005f){accumulator-=.005f;Tick(.005f);}
            Present();
        }

        private void Tick(float dt)
        {
            elapsed+=dt;
            gridAngle=Mathf.Repeat(gridAngle+50*360*dt,360);
            if(pll&&main)
            {
                float e=Mathf.DeltaAngle(phase+50*360*dt,gridAngle);
                pllHz=50+Mathf.Clamp(e*.27f,-12,12);
                phase=Mathf.Repeat(phase+pllHz*360*dt,360);
                pllHold=Mathf.Abs(e)<4&&Mathf.Abs(pllHz-50)<.2f?pllHold+dt:0;
            }
            else {pllHz=0;pllHold=0;}

            // RC precharge into the link, with source-side conversion loss and a regulated hold voltage.
            float targetLink=main&&precharge&&storedEnergy>MinimumEnergy?Mathf.Min(5.05f,StoreVoltage):0;
            float oldLink=link;
            link+=(targetLink-link)*(1-Mathf.Exp(-dt/(precharge?.51f:.22f)));
            float linkWatts=Mathf.Max(0,.5f*LinkCapacitance*(link*link-oldLink*oldLink)/dt);
            bool motorTest=scenario==Event.Motor;
            float heavyCurrent=motorTest?.48f+2.35f*Mathf.Exp(-elapsed/.64f):0;
            heavySpeed=Mathf.MoveTowards(heavySpeed,motorTest?1:0,dt*.80f);
            rmsCurrent=.58f+heavyCurrent;
            float maximum=Mathf.Min(.43f,.43f*link/4.8f);
            converterPower=0;
            for(int i=0;i<3;i++)
            {
                float requestedInput=!main?0:scenario switch
                {
                    Event.Sag=>.65f, Event.Swell=>1.15f,
                    Event.Unbalance=>i==0?.72f:i==1?.85f:.93f,
                    Event.Motor=>Mathf.Max(.66f,1-.126f*heavyCurrent),
                    _=>1
                };
                // Release a disturbance gradually while the series filter follows the recovering source.
                input[i]=scenario==Event.Normal&&Healthy&&input[i]>.1f
                    ?Mathf.MoveTowards(input[i],requestedInput,dt*.75f)
                    :requestedInput;
                float command=Healthy?Mathf.Clamp((1-input[i])/.96f,-maximum,maximum):0;
                injected[i]=Healthy?Mathf.MoveTowards(injected[i],command,dt*6.4f):0;
                float series=.96f*injected[i];
                output[i]=main&&(active||bypass)?Mathf.Max(0,input[i]+series):0;
                converterPower+=series*.58f*2.4f;
            }
            float watts=converterPower>=0?converterPower/.91f:converterPower*.88f;
            float recharge=main&&precharge&&scenario==Event.Normal&&storedEnergy<52?.22f:0;
            storedEnergy=Mathf.Clamp(storedEnergy+dt*(recharge-watts-linkWatts/.91f),0,55);
            temp+=(Mathf.Abs(converterPower)*2.2f-(temp-24)*.41f)*dt;
            motorSpeed=Mathf.MoveTowards(motorSpeed,output[0]>.84f?1:0,dt*(output[0]>.84f?1.5f:3.5f));
            if(scenario!=Event.Normal&&elapsed>.25f&&(output[0]<.85f||output[1]<.85f||output[2]<.85f))
                rideThroughFailed=true;
            if(active&&(storedEnergy<=MinimumEnergy||temp>63||link<4.35f))Trip("DVR ENERGY OR THERMAL LIMIT",true);

            bool stable=Healthy&&VoltageValid&&!rideThroughFailed;
            bool interval=stage switch
            {
                1=>Healthy&&VoltageValid&&scenario==Event.Normal,
                2=>stable&&scenario==Event.Sag&&injected[0]>.30f,
                3=>stable&&(scenario==Event.Swell&&injected[0]<-.12f||
                    scenario==Event.Unbalance&&Unbalance(output)<.025f),
                4=>stable&&motorTest&&heavySpeed>.92f&&motorSpeed>.95f&&rmsCurrent>.9f,
                5=>stable&&scenario==Event.Sag&&sensorObserved&&resetDone&&injected[0]>.30f,
                _=>false
            };
            hold=interval?hold+dt:0;
            if(hold<1.20f)return;
            if(stage==3&&scenario==Event.Swell)
            {
                swellPassed=true;
                Begin(Event.Normal);
            }
            else if(stage==5)
            {
                repeatPassed=true;
                if(certify)Award();
            }
            else
            {
                Award();
                Begin(Event.Normal);
            }
        }

        private void SetPoles(int offset,bool state)
        {
            for(int i=offset;i<offset+3;i++)
            {
                var item=contacts[i];
                if(item.IsClosed!=state)item.SetState(state?SparkSwitchState.Closed:SparkSwitchState.Open);
            }
        }
        private void Path(bool inject,bool useBypass)
        {
            // Break before make prevents parallel bypass and injection paths.
            if(inject){SetPoles(6,false);SetPoles(3,true);}
            else{SetPoles(3,false);SetPoles(6,useBypass);}
            active=inject;
            bypass=useBypass;
        }
        private void Trip(string reason,bool allowBypass)
        {
            fault=reason;
            calibrated=pll=false;
            Path(false,allowBypass&&main);
            Begin(Event.Normal);
            Debug.LogWarning("[LEVEL 62] "+reason,this);
        }
        private void Award()
        {
            if(stage>=Gates.Length||!bindings.SetRuntimeComponentBinding(Gates[stage],source))return;
            stage++;
            hold=0;
            manager.QueueEvaluation();
        }
        private void Begin(Event value)
        {
            scenario=value;
            elapsed=hold=0;
            rideThroughFailed=false;
        }

        public void Power(){power=!power;source.SetOutput(power,default);}
        public void Guard(){guard=!guard;emergency.SetState(guard?SparkSwitchState.Closed:SparkSwitchState.Open);}
        public void Main(){if(ready&&fault==null){main=true;SetPoles(0,true);Path(false,true);}}
        public void Calibrate(){if(ready&&main&&fault==null)calibrated=true;}
        public void Store(){if(ready&&calibrated&&storedEnergy>MinimumEnergy)storeChecked=true;}
        public void Precharge(){if(ready&&main&&storeChecked)precharge=true;}
        public void Pll(){if(ready&&calibrated&&main){phase=Mathf.Repeat(gridAngle-120,360);pll=true;}}
        private bool CanEnable=>ready&&main&&fault==null&&calibrated&&Locked&&link>=4.65f&&!active;
        public void Enable(){if(CanEnable)Path(true,false);}
        public void Sag(){if(Healthy&&(stage==2||stage==5&&resetDone&&!repeatPassed))Begin(Event.Sag);}
        public void Swell(){if(Healthy&&stage==3&&!swellPassed)Begin(Event.Swell);}
        public void UnbalanceTest(){if(Healthy&&stage==3&&swellPassed)Begin(Event.Unbalance);}
        public void Motor(){if(Healthy&&stage==4)Begin(Event.Motor);}
        public void Fault(){if(Healthy&&stage==5&&!sensorObserved)faultTest=true;}
        public void Reset(){if(ready&&fault!=null){fault=null;resetDone=sensorObserved;faultTest=false;}}
        public void Final(){if(stage==5&&repeatPassed&&Healthy&&VoltageValid)certify=true;}

        private string Prompt()
        {
            if(stage==6)return "CERTIFIED · Ride-through and safe sensor recovery verified.";
            if(fault!=null)return fault+(ready?" · reset and recommission.":" · reconnect W54 while DVR is in bypass.");
            if(wiring!=null)return wiring;
            if(!ready)return "Restore the 5 V control supply and close the safety guard.";
            if(!main)return "Select MAIN ON to feed the protected bus through bypass.";
            if(!calibrated)return "Calibrate input, output and injection voltage sensors.";
            if(!storeChecked)return "Check the supercapacitor reserve.";
            if(!precharge)return "Select PRECHARGE.";
            if(!Locked)return pll?"Acquiring 50 Hz phase reference…":"Select LOCK PLL.";
            if(!active)return link<4.65f?"Waiting for DC-link voltage.":"Enable DVR to switch from bypass to series injection.";
            if(rideThroughFailed)return "Voltage protection interrupted. Repeat the active disturbance test.";
            if(stage==2)return scenario==Event.Sag?"Restoring deep sag…":"Apply DEEP SAG.";
            if(stage==3)return swellPassed?"Apply UNBALANCE.":scenario==Event.Swell?"Injecting opposing voltage…":"Apply SWELL.";
            if(stage==4)return scenario==Event.Motor?"Heavy motor starting; watch protected voltage.":"Select HEAVY MOTOR.";
            if(stage==5)return sensorObserved?repeatPassed?"Recovered sag verified. Select CERTIFY.":"Repeat DEEP SAG after recommissioning.":
                faultTest?"Disconnect W54 · INJ V OUT → DVR V IN.":"Select W54 TEST.";
            return "Verifying steady protected output.";
        }
        private void Present()
        {
            statusText.text=Prompt();
            meterText.text=$"INPUT {input[0]:F2} / {input[1]:F2} / {input[2]:F2} pu   ·   PROTECTED {output[0]:F2} / {output[1]:F2} / {output[2]:F2} pu";
            detailText.text=$"INJECT {injected[0]:+0.00;-0.00;0.00} / {injected[1]:+0.00;-0.00;0.00} / {injected[2]:+0.00;-0.00;0.00} pu   ·   DC {link:F2} V   ·   STORE {storedEnergy:F1} J";
            calibrationText.text=$"{manager.SatisfiedTargetCount}/6  ·  {(active?"DVR":bypass?"BYPASS":"OPEN")}  ·  PLL {(Locked?"LOCKED":"—")}  ·  IN/OUT IMBALANCE {Unbalance(input):P1}/{Unbalance(output):P1}  ·  LOAD {rmsCurrent:F2} A  ·  {temp:F1} °C";
            if(controllerDisplay)controllerDisplay.text=$"{output[0]:F2}/{output[1]:F2}/{output[2]:F2} pu";
            if(qualityDisplay)qualityDisplay.text=$"DC {link:F1} V / STORE {storedEnergy:F0} J";
            if(plot)plot.Show(input,output);
            if(rig)rig.Apply(main,bypass,active,guard,heavySpeed,motorSpeed,temp,Time.deltaTime);
            powerButton.interactable=guardButton.interactable=true;
            mainButton.interactable=ready&&!main&&fault==null;
            calibrateButton.interactable=ready&&main&&!calibrated&&fault==null;
            storeButton.interactable=ready&&calibrated&&!storeChecked;
            prechargeButton.interactable=ready&&storeChecked&&!precharge;
            pllButton.interactable=ready&&calibrated&&precharge&&!pll;
            enableButton.interactable=CanEnable;
            sagButton.interactable=Healthy&&(stage==2||stage==5&&resetDone&&!repeatPassed)&&(scenario==Event.Normal||rideThroughFailed);
            swellButton.interactable=Healthy&&stage==3&&!swellPassed&&(scenario==Event.Normal||rideThroughFailed);
            unbalanceButton.interactable=Healthy&&stage==3&&swellPassed&&(scenario==Event.Normal||rideThroughFailed);
            motorButton.interactable=Healthy&&stage==4&&(scenario==Event.Normal||rideThroughFailed);
            faultButton.interactable=Healthy&&stage==5&&!sensorObserved&&!faultTest;
            resetButton.interactable=ready&&fault!=null;
            finalButton.interactable=stage==5&&repeatPassed&&Healthy&&VoltageValid&&!certify;
        }
        private void OnDisable()
        {
            if(bindings!=null)foreach(string gate in Gates)bindings.ClearRuntimeComponentBinding(gate);
        }
    }
}



