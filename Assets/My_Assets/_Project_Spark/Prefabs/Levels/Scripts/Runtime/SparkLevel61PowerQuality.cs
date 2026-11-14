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
    public sealed class SparkLevel61PowerQuality : MonoBehaviour
    {
        public static readonly string[] Gates={"L61_Wired","L61_Baseline","L61_Clean","L61_PF","L61_Step","L61_Recovered"};
        public static readonly (int a,int b)[] Wires={(0,1),(2,3),(0,4),(2,5),(0,6),(2,7),(0,8),(2,9),(10,11),(12,13),(14,15),(16,17),(18,19),(20,21),(17,22),(19,23),(21,24),(17,25),(19,26),(21,27),(28,29),(17,30),(19,31),(32,33),(34,17),(35,19),(36,21),(37,38),(39,40),(41,42),(43,44),(45,46),(47,48),(49,50),(51,52),(53,54),(55,56),(22,57),(58,59),(17,60),(19,61),(21,62),(63,64),(65,66),(58,67),(68,69),(70,71),(72,73),(74,75),(76,77)};

        public SparkLevelLayout layout;
        public SparkPowerSupply source;
        public SparkSwitch[] contacts;
        public SparkSwitch emergency;
        public SparkTerminal[] pins;
        public SparkLevel61VisualRig rig;
        public SparkLevel61Plot plot;
        public TMP_Text controllerDisplay,qualityDisplay;
        public Text statusText,meterText,detailText,calibrationText;
        public Button powerButton,guardButton,mainButton,calibrateButton,loadButton,pllButton,baselineButton,filterButton,
            gainButton,capButton,stepButton,faultButton,resetButton,finalButton;

        private const float LineVoltage=2.4f, CurrentLimit=.56f;
        private static readonly int[] Orders={5,7,11,13};
        private readonly float[] loadHarmonics=new float[4], injection=new float[4], residual=new float[4], baselineSpectrum=new float[4];
        private readonly List<SparkCircuitConnection> edges=new();
        private readonly HashSet<(int,int)> wired=new();
        private SparkLevelLayoutLoader loader;
        private LevelGamePlayManager manager;
        private SparkLevelSceneBindings bindings;
        private SparkElectricalSolver solver;
        private SparkCircuitSystem circuit;
        private int topology=-1,stage,caps,gain;
        private float accumulator,hold,baselineThd,baselinePf,link,temperature=25,load,fundamental,sourceThd,q,dpF,truePF,phase,gridPhase,pllHz,pllHold,amps,volts,linkEnergy;
        private bool power=true,guard=true,ready,main,calibrated,loads,pll,recording,filter,step,faultArmed,sensorObserved,resetDone,certify;
        private string wiring,fault,notice="Connect the power quality network.";
        private bool Feedback => Routed(49);
        private bool Locked => pll&&pllHold>=.55f;
        private bool Clean => filter&&Locked&&Feedback&&sourceThd<.05f&&amps<=CurrentLimit+.001f&&link>4.4f&&temperature<64;
        private bool Corrected => Clean&&dpF>=.98f&&q>=-.15f;
        private float Gain => gain switch {0=>.58f,1=>.82f,_=>.97f};

        private void Start()
        {
            loader=FindFirstObjectByType<SparkLevelLayoutLoader>();
            manager=FindFirstObjectByType<LevelGamePlayManager>();
            if(loader&&manager&&pins?.Length==78&&contacts?.Length==12)return;
            Debug.LogError("[LEVEL 61] Missing runtime services, terminals or contactors.",this);
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
            return wired.Count==Wires.Length?null:"Remove extra wires from the Power Quality sockets.";
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
            bool controls=power&&guard&&source.IsOutputActive&&!source.IsOverloaded&&Rail(1,3)&&Rail(4,5)&&Rail(6,7)&&Rail(8,9)&&Rail(75,3);
            ready=controls&&wiring==null;
            if(stage==0&&ready)Award();
            if(main&&(!controls||wiring!=null&&Feedback))
                ShutDown(!guard?"GUARD OPEN":wiring??"CONTROL RAIL INVALID");
            else if(main&&!Feedback&&(filter||faultArmed)&&fault==null)
            {
                sensorObserved|=faultArmed;
                TripFilter("FILTER CT SIGNAL LOST · W50");
            }
            accumulator+=Mathf.Min(Time.deltaTime,.10f);
            while(accumulator>=.002f){accumulator-=.002f;Sample(.002f);}
            Present();
        }

        private void Sample(float dt)
        {
            gridPhase=Mathf.Repeat(gridPhase+50f*360f*dt,360);
            if(pll&&main&&calibrated)
            {
                float error=Mathf.DeltaAngle(phase+50f*360f*dt,gridPhase);
                pllHz=50f+Mathf.Clamp(error*.30f,-15f,15f);
                phase=Mathf.Repeat(phase+pllHz*360f*dt,360);
                pllHold=Mathf.Abs(error)<4f&&Mathf.Abs(pllHz-50)<.2f?pllHold+dt:0;
            }
            else {pllHz=0;pllHold=0;}

            load=Mathf.MoveTowards(load,main&&loads?(step?1.40f:1f):0,dt*2.6f);
            volts=main?LineVoltage-.065f*load:0;
            float real=volts*.82f*load;
            q=main?volts*.40f*load-.54f*caps:0;
            fundamental=volts>.1f?Mathf.Sqrt(real*real+q*q)/volts:0;
            loadHarmonics[0]=load*(.184f+(step?.016f:0));
            loadHarmonics[1]=load*.123f;
            loadHarmonics[2]=load*.067f;
            loadHarmonics[3]=load*(.048f+(step?.017f:0));
            float requestedSq=0;
            for(int i=0;i<Orders.Length;i++)requestedSq+=loadHarmonics[i]*loadHarmonics[i]*Gain*Gain;
            float multiplier=requestedSq>0?Mathf.Min(1,CurrentLimit/Mathf.Sqrt(requestedSq)):1;
            linkEnergy=Mathf.Max(0,linkEnergy+((main?(12.50f-linkEnergy)*2.4f:-linkEnergy*2)-amps*amps*.27f)*dt);
            link=Mathf.Sqrt(2*linkEnergy);
            bool inject=filter&&Locked&&Feedback&&main&&link>=4.4f;
            float harmSq=0, ampSq=0;
            for(int i=0;i<Orders.Length;i++)
            {
                float phaseError=Mathf.DeltaAngle(phase,gridPhase)*Mathf.Deg2Rad*Orders[i];
                float target=inject?loadHarmonics[i]*Gain*multiplier*Mathf.Cos(phaseError):0;
                injection[i]=inject?Mathf.MoveTowards(injection[i],target,dt*1.8f):0;
                float quadrature=inject?loadHarmonics[i]*Gain*multiplier*Mathf.Sin(phaseError):0;
                residual[i]=Mathf.Sqrt(Mathf.Pow(loadHarmonics[i]-injection[i],2)+quadrature*quadrature);
                harmSq+=residual[i]*residual[i];
                ampSq+=injection[i]*injection[i]+quadrature*quadrature;
            }
            amps=Mathf.Sqrt(ampSq);
            sourceThd=fundamental>.01f?Mathf.Sqrt(harmSq)/fundamental:0;
            dpF=fundamental>.01f?real/(volts*fundamental):0;
            truePF=fundamental>.01f?real/(volts*Mathf.Sqrt(fundamental*fundamental+harmSq)):0;
            temperature+=(amps*amps*15f-(temperature-25f)*.46f)*dt;
            if(filter&&(temperature>=64||link<4.2f))TripFilter(temperature>=64?"FILTER OVERTEMPERATURE":"DC LINK UNDERVOLTAGE");

            bool interval=stage switch
            {
                1=>recording&&ready&&Locked&&load>.99f&&!filter&&sourceThd>.15f,
                2=>Clean&&baselineThd>.15f,
                3=>Corrected,
                4=>step&&load>1.38f&&Corrected,
                5=>certify&&sensorObserved&&resetDone&&Corrected&&load>1.38f,
                _=>false
            };
            hold=interval?hold+dt:0;
            if(hold<1.25f)return;
            if(stage==1)
            {
                baselineThd=sourceThd;
                baselinePf=truePF;
                Array.Copy(residual,baselineSpectrum,residual.Length);
            }
            Award();
        }

        private void SetPoles(int offset,bool closed)
        {
            for(int i=offset;i<offset+3;i++)
            {
                var c=contacts[i];
                if(c.IsClosed!=closed)c.SetState(closed?SparkSwitchState.Closed:SparkSwitchState.Open);
            }
        }
        private void ShutDown(string reason)
        {
            main=loads=false;
            caps=0;
            SetPoles(0,false);
            SetPoles(6,false);
            SetPoles(9,false);
            TripFilter(reason);
        }
        private void TripFilter(string reason)
        {
            fault=reason;
            filter=calibrated=pll=false;
            Array.Clear(injection,0,injection.Length);
            SetPoles(3,false);
            Debug.LogWarning("[LEVEL 61] "+reason,this);
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
        public void Main(){if(ready&&fault==null){main=true;SetPoles(0,true);}}
        public void Calibrate(){if(ready&&main&&fault==null){calibrated=true;notice="Voltage and current channels calibrated.";}}
        public void Load(){if(ready&&main&&calibrated){loads=true;SetPoles(9,true);}}
        public void Pll(){if(ready&&main&&calibrated&&fault==null){phase=Mathf.Repeat(gridPhase-110,360);pll=true;}}
        public void Baseline(){if(ready&&Locked&&loads&&stage==1)recording=true;}
        public void Filter()
        {
            if(!ready||fault!=null||stage<2||!Locked||link<4.4f)return;
            filter=true;
            SetPoles(3,true);
        }
        public void GainSelect(){if(filter&&fault==null)gain=(gain+1)%3;}
        public void Cap()
        {
            if(stage<3||!main||fault!=null)return;
            caps=(caps+1)%4;
            if(caps>2 && load<1.2f || q<-.45f&&caps>0)
            {
                caps=0;
                notice="Capacitor protection rejected excessive leading current.";
            }
            SetPoles(6,caps>0);
        }
        public void LoadStep(){if(stage==4&&Corrected)step=true;}
        public void Fault(){if(stage==5&&Corrected&&!sensorObserved)faultArmed=true;}
        public void Reset()
        {
            if(!ready||fault==null)return;
            resetDone=sensorObserved;
            faultArmed=false;
            fault=null;
        }
        public void Final(){if(stage==5&&Corrected&&sensorObserved&&resetDone)certify=true;}

        private string Prompt()
        {
            if(stage==6)return "CERTIFIED · Clean current, corrected PF and safe sensor recovery.";
            if(fault!=null)return fault+(ready?" · RESET FAULT, recalibrate and reacquire PLL.":" · Reconnect the signal before reset.");
            if(wiring!=null)return wiring;
            if(!ready)return "Restore the 5 V rails and close the safety guard.";
            if(!main)return "Select MAIN ON.";
            if(!calibrated)return "Calibrate the voltage and current channels.";
            if(!loads)return "Enable nonlinear loads.";
            if(!Locked)return pll?"Tracking fundamental phase…":"Select LOCK PLL.";
            return stage switch
            {
                1=>recording?"Recording the baseline harmonic spectrum.":"Select RECORD THD.",
                2=>filter?"Set filter gain to reduce source THD below 5%.":"Enable the active filter when the DC link is ready.",
                3=>notice.StartsWith("Capacitor")?notice:"Switch capacitor stages to correct displacement PF.",
                4=>step?"Observe the changing spectrum and the filter response.":"Apply the industrial load step.",
                5=>sensorObserved?(filter?"Hold low THD and corrected PF, then CERTIFY.":"Enable the filter after PLL reference lock."):
                    faultArmed?"Disconnect W50 · FILTER CT OUT → APF CT IN.":"Select W50 TEST.",
                _=>notice
            };
        }
        private void Present()
        {
            statusText.text=Prompt();
            meterText.text=$"SOURCE THD {sourceThd:P1}  ·  DPF {dpF:F3}  ·  TRUE PF {truePF:F3}  ·  REACTIVE {q:+0.00;-0.00;0.00} var";
            detailText.text=$"LINK {link:F2} V  ·  FILTER {amps:F3} A  ·  TEMP {temperature:F1} °C  ·  PLL {(Locked?"LOCKED":"—")}  ·  {manager.SatisfiedTargetCount}/6";
            calibrationText.text=stage>=2?$"BASELINE THD {baselineThd:P1} / PF {baselinePf:F3}  ·  H5 {baselineSpectrum[0]:F2} A  H7 {baselineSpectrum[1]:F2} A  ·  {(Feedback?"CT VALID":"CT INVALID")}":"5th / 7th / 11th / 13th harmonic current";
            if(controllerDisplay)controllerDisplay.text=$"THD {sourceThd:P1} / DPF {dpF:F3}";
            if(qualityDisplay)qualityDisplay.text=$"DC {link:F1} V / {(Feedback?"CT VALID":"CT INVALID")}";
            if(plot)plot.Show(fundamental,loadHarmonics,residual);
            if(rig)rig.Apply(main,loads,caps>0,guard,filter,load,temperature,Time.deltaTime);
            powerButton.interactable=guardButton.interactable=true;
            mainButton.interactable=ready&&!main&&fault==null;
            calibrateButton.interactable=ready&&main&&!calibrated&&fault==null;
            loadButton.interactable=ready&&main&&calibrated&&!loads;
            pllButton.interactable=ready&&main&&calibrated&&!pll;
            baselineButton.interactable=stage==1&&ready&&Locked&&loads&&!recording;
            filterButton.interactable=stage>=2&&ready&&Locked&&!filter&&fault==null&&link>=4.4f;
            gainButton.interactable=filter&&fault==null;
            gainButton.GetComponentInChildren<Text>().text=$"GAIN {Gain:P0}";
            capButton.interactable=stage>=3&&main&&fault==null;
            capButton.GetComponentInChildren<Text>().text=$"CAP STAGES {caps}";
            stepButton.interactable=stage==4&&!step&&Corrected;
            faultButton.interactable=stage==5&&Corrected&&!sensorObserved&&!faultArmed;
            resetButton.interactable=ready&&fault!=null;
            finalButton.interactable=stage==5&&Corrected&&sensorObserved&&resetDone&&!certify;
        }
        private void OnDisable()
        {
            if(bindings!=null)foreach(string gate in Gates)bindings.ClearRuntimeComponentBinding(gate);
        }
    }
}


