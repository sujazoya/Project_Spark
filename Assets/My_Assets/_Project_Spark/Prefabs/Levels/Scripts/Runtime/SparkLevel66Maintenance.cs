using System;
using System.Collections.Generic;
using ProjectSpark.Circuit;
using ProjectSpark.Electrical;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.Gameplay
{
    public sealed class SparkLevel66Maintenance : MonoBehaviour
    {
        public static readonly string[] Gates={"L66_Wired","L66_Baseline","L66_Diagnosis","L66_Repair","L66_Load","L66_Recovered"};
        public static readonly (int a,int b)[] Wires={(0,1),(2,3),(0,4),(2,5),(0,6),(2,7),(0,8),(2,9),(0,10),(2,11),(0,12),(2,13),(0,14),(2,15),(0,16),(2,17),(0,18),(2,19),(20,21),(22,23),(24,25),(26,27),(28,29),(30,31),(32,33),(34,35),(36,37),(38,39),(40,41),(42,43),(44,45),(44,46),(44,47),(48,49),(50,51),(52,53),(54,55),(56,57),(58,59),(60,61),(62,63),(64,65),(66,67),(68,69),(70,71),(72,73),(74,75),(74,76),(77,78),(77,79),(80,81),(80,82),(75,83),(77,84),(85,86),(20,87),(88,89),(90,91),(92,93),(56,94),(95,96),(97,98),(99,100),(99,101),(102,103),(34,104),(88,105),(35,106),(75,107),(108,109)};
        public SparkLevelLayout layout;
        public SparkPowerSupply source;
        public SparkSwitch[] contacts;
        public SparkSwitch emergency;
        public SparkTerminal[] pins;
        public SparkLevel66VisualRig rig;
        public SparkLevel66Plot plot;
        public TMP_Text controllerDisplay,qualityDisplay;
        public Text statusText,meterText,detailText,calibrationText;
        public Button powerButton,calibrateButton,runButton,baselineButton,wearButton,diagnoseButton,stopButton,isolateButton,
            lockButton,guardButton,extractButton,installButton,alignButton,torqueButton,speedButton,loadButton,faultButton,resetButton,
            finalButton,emergencyStopButton;

        private const int Samples=256;
        private const float Dt=.002f, SampleRate=500f;
        private readonly float[] wave=new float[Samples], real=new float[Samples], imag=new float[Samples], bins=new float[Samples/2];
        private readonly List<SparkCircuitConnection> edges=new();
        private readonly HashSet<(int,int)> connections=new();
        private LevelGamePlayManager manager;
        private SparkLevelLayoutLoader loader;
        private SparkLevelSceneBindings bindings;
        private SparkCircuitSystem circuit;
        private int topology=-1,stage,index,sampleCount,fftFrames,repair,qualify;
        private float accumulator,hold,rpm,command,angle,damage,heat=24,motorHeat=24,current,vibration,bpfo,bpfi,raceMagnitude,baselineRms,
            baselinePeak,gearMagnitude,alignmentMagnitude,baselineCurrent,baselineTemperature,serviceMotion;
        private bool power=true,ready,rails,door=true,stopped,run,calibrated,armed,recording,wear,isolated,locked,
            load,faultArmed,sensorObserved,resetDone,certify;
        private string wiring,latch,report="Awaiting a valid baseline.";
        private bool Feedback=>Routed(69);
        private bool AtRest=>rpm<4;
        private bool SafeService=>rails&&isolated&&locked&&!door&&AtRest&&latch==null;
        private bool CanRun=>ready&&calibrated&&door&&!stopped&&!isolated&&!locked&&latch==null&&contacts[1].IsClosed;
        private bool Fresh=>calibrated&&Feedback&&fftFrames>=2;
        private bool Healthy=>Fresh&&vibration<Mathf.Max(.30f,baselineRms*1.65f)&&raceMagnitude<.15f&&heat<52&&motorHeat<57&&current<2.1f;

        private void Start()
        {
            manager=FindFirstObjectByType<LevelGamePlayManager>();
            loader=FindFirstObjectByType<SparkLevelLayoutLoader>();
            if(manager&&loader&&pins?.Length==110&&contacts?.Length==2&&rig)return;
            Debug.LogError("[LEVEL 66] Missing level wiring, safety contacts or motion references.",this);
            enabled=false;
        }

        private static (int,int) Pair(int a,int b)=>(Math.Min(a,b),Math.Max(a,b));
        private bool Routed(int i)=>connections.Contains(Pair(Wires[i].a,Wires[i].b));
        private string Inspect()
        {
            circuit.CopyConnections(edges);
            connections.Clear();
            foreach(var edge in edges)
            {
                if(edge.Kind!=SparkConnectionKind.Wire)continue;
                int a=Array.IndexOf(pins,edge.A),b=Array.IndexOf(pins,edge.B);
                if(a>=0||b>=0)connections.Add(Pair(a,b));
            }
            for(int i=0;i<Wires.Length;i++)
                if(!Routed(i))return $"Connect W{i+1:00} · {pins[Wires[i].a].name} → {pins[Wires[i].b].name}.";
            return connections.Count==Wires.Length?null:"Remove unexpected factory wiring.";
        }
        private void Update()
        {
            if(loader.ActiveLayout!=layout||loader.ActiveBindings==null)return;
            circuit=manager.CircuitSystem;
            var solver=manager.ElectricalSolver;
            if(!circuit||!solver)return;
            if(bindings!=loader.ActiveBindings){bindings=loader.ActiveBindings;topology=-1;}
            if(topology!=circuit.TopologyVersion){topology=circuit.TopologyVersion;wiring=Inspect();}
            var result=solver.LatestSolveResult;
            bool Rail(int a,int b)=>result!=null&&result.TryGetVoltage(pins[a],pins[b],out var value)&&value is >=4.7f and <=5.3f;
            rails=power&&source.IsOutputActive&&!source.IsOverloaded;
            for(int i=0;i<9&&rails;i++)rails=Rail(Wires[i*2].b,Wires[i*2+1].b);
            ready=rails&&door&&!stopped&&wiring==null&&Rail(100,3)&&Rail(101,3)&&Rail(103,3);
            if(stage==0&&ready)Award();
            if(latch==null&&(run||faultArmed)&&!ready)
            {
                bool sensorOnly=rails&&door&&!stopped&&!Feedback&&connections.Count==69;
                for(int i=0;i<69&&sensorOnly;i++)sensorOnly=Routed(i);
                sensorObserved|=sensorOnly&&faultArmed;
                Trip(sensorOnly?"PRIMARY VIBRATION LOST · W70":!door||stopped?"SAFETY INTERLOCK OPEN":wiring??"CONTROL POWER LOST");
            }

            accumulator+=Mathf.Min(Time.deltaTime,.12f);
            while(accumulator>=Dt){accumulator-=Dt;Sample(Dt);}
            Present();
        }

        private void Sample(float dt)
        {
            if(wear&&run)damage=Mathf.MoveTowards(damage,.80f,dt*.30f);
            if(!run)rpm=Mathf.MoveTowards(rpm,0,(isolated?710:640)*dt);
            else
            {
                float inertia=.021f;
                float loadTorque=(load?.45f:.19f)+.20f*damage;
                float torque=Mathf.Clamp(loadTorque+(command-rpm)*.0065f,0,2.3f);
                rpm=Mathf.Max(0,rpm+(torque-loadTorque)/inertia*dt*60/(2*Mathf.PI));
                current=.38f+torque*.85f;
            }
            if(!run)current=0;
            float fr=rpm/60;
            bpfo=3.2f*fr;
            bpfi=4.8f*fr;
            angle=Mathf.Repeat(angle+2*Mathf.PI*fr*dt,Mathf.PI*10);
            shaftTurns+=fr*dt;
            float intensity=rpm/1200f;
            // Outer-race impacts follow shaft speed. Alternatives occupy the alignment and gear mesh bands.
            float sample=intensity*(.21f*Mathf.Sin(angle)+(.045f+1.22f*damage)*Mathf.Sin(2*Mathf.PI*3.2f*shaftTurns)+
                .085f*Mathf.Sin(2*Mathf.PI*2*shaftTurns)+.045f*Mathf.Sin(2*Mathf.PI*11*shaftTurns));
            heat+=dt*((run?2.4f*damage+current*.42f:0)-(heat-24)*.14f);
            motorHeat+=dt*(current*current*1.30f-(motorHeat-24)*.16f);
            serviceMotion=Mathf.MoveTowards(serviceMotion,repair==1?1:0,dt*3);
            if(calibrated&&Feedback&&rails)
            {
                wave[index++]=sample;
                sampleCount++;
                if(index==Samples)
                {
                    index=0;
                    Spectrum(fr);
                }
            }
            if(run&&(heat>66||motorHeat>70||current>2.7f))Trip("MOTOR PROTECTION TRIP");
            bool nominal=Mathf.Abs(rpm-1200)<20;
            bool condition=stage switch
            {
                1=>recording&&Fresh&&nominal&&vibration<.30f,
                4=>run&&Healthy&&(qualify==0?Mathf.Abs(rpm-720)<18:nominal&&(qualify!=2||load)),
                5=>certify&&sensorObserved&&resetDone&&run&&nominal&&load&&Healthy,
                _=>false
            };
            hold=condition?hold+dt:0;
            if(hold<1.12f)return;
            hold=0;
            if(stage==1)
            {
                baselineRms=vibration;baselinePeak=raceMagnitude;baselineCurrent=current;baselineTemperature=heat;
                report=$"BASELINE {baselineRms:F2} mm/s · {baselineCurrent:F2} A · {baselineTemperature:F1} °C";
                Award();
            }
            else if(stage==4&&++qualify<3)report=qualify==1?"Low-speed result recorded. Select SPEED TEST.":"Nominal result recorded. Apply LOAD TEST.";
            else Award();
        }
        private float shaftTurns;

        private void Spectrum(float fr)
        {
            float sum=0;
            for(int i=0;i<Samples;i++)
            {
                sum+=wave[i]*wave[i];
                real[i]=wave[i]*(.5f-.5f*Mathf.Cos(2*Mathf.PI*i/(Samples-1)));
                imag[i]=0;
            }
            vibration=Mathf.Sqrt(sum/Samples);
            for(int i=1,j=0;i<Samples;i++)
            {
                int bit=Samples>>1;
                for(; (j&bit)!=0;bit>>=1)j^=bit;
                j^=bit;
                if(i>=j)continue;
                (real[i],real[j])=(real[j],real[i]);
            }
            for(int width=2;width<=Samples;width*=2)
                for(int first=0;first<Samples;first+=width)
                    for(int j=0;j<width/2;j++)
                    {
                        float x=-2*Mathf.PI*j/width;
                        int a=first+j,b=a+width/2;
                        float re=real[b]*Mathf.Cos(x)-imag[b]*Mathf.Sin(x);
                        float im=real[b]*Mathf.Sin(x)+imag[b]*Mathf.Cos(x);
                        real[b]=real[a]-re;imag[b]=imag[a]-im;
                        real[a]+=re;imag[a]+=im;
                    }
            for(int i=0;i<bins.Length;i++)bins[i]=4*Mathf.Sqrt(real[i]*real[i]+imag[i]*imag[i])/Samples;
            float Peak(float frequency)
            {
                int center=Mathf.Clamp(Mathf.RoundToInt(frequency*Samples/SampleRate),1,bins.Length-2);
                return Mathf.Max(bins[center],Mathf.Max(bins[center-1],bins[center+1]));
            }
            raceMagnitude=Peak(3.2f*fr);
            alignmentMagnitude=Peak(2*fr);
            gearMagnitude=Peak(11*fr);
            fftFrames++;
        }
        private void Invalidate()
        {
            index=sampleCount=fftFrames=0;
            Array.Clear(bins,0,bins.Length);
        }
        private void Trip(string reason)
        {
            run=armed=certify=calibrated=false;
            contacts[1].SetState(SparkSwitchState.Open);
            isolated=true;
            latch=reason;
            Invalidate();
            Debug.LogWarning("[LEVEL 66] "+reason,this);
        }
        private void Award()
        {
            if(stage>=Gates.Length||!bindings.SetRuntimeComponentBinding(Gates[stage],source))return;
            stage++;
            hold=0;
            manager.QueueEvaluation();
        }

        public void Power(){power=!power;source.SetOutput(power,default);}
        public void Calibrate(){if(ready&&!run&&AtRest){calibrated=true;Invalidate();}}
        public void Run()
        {
            if(!CanRun)return;
            run=armed=true;
            command=stage==4&&qualify==0?720:1200;
            load=false;
        }
        public void Baseline(){if(stage==1&&run&&Fresh)recording=true;}
        public void Wear(){if(stage==2&&run)wear=true;}
        private bool DiagnosisReady=>stage==2&&wear&&Fresh&&vibration>baselineRms*2&&raceMagnitude>Mathf.Max(.26f,baselinePeak*2)&&raceMagnitude>Mathf.Max(alignmentMagnitude,gearMagnitude)*1.35f;
        public void Diagnose()
        {
            if(!DiagnosisReady)return;
            report=$"DRIVE-END OUTER RACE · {bpfo:F1} Hz · {raceMagnitude:F2} mm/s";
            Award();
        }
        public void Stop(){run=armed=false;}
        public void Isolate()
        {
            if(run||!AtRest||!door||locked)return;
            isolated=!isolated;
            contacts[1].SetState(isolated?SparkSwitchState.Open:SparkSwitchState.Closed);
        }
        public void Lock()
        {
            if(run||!AtRest||!isolated||!door||stage!=3)return;
            locked=!locked;
            if(!locked&&repair==4)Award();
        }
        public void Guard()
        {
            if(door&&!(locked&&isolated&&AtRest))return;
            door=!door;
            contacts[0].SetState(door?SparkSwitchState.Closed:SparkSwitchState.Open);
        }
        public void Extract(){if(SafeService&&repair==0&&damage>.2f){repair=1;wear=false;}}
        public void Install(){if(SafeService&&repair==1&&serviceMotion>.95f){repair=2;damage=.025f;}}
        public void Align(){if(SafeService&&repair==2)repair=3;}
        public void Torque(){if(SafeService&&repair==3)repair=4;}
        public void Speed(){if(stage==4&&run&&qualify==1)command=1200;}
        public void Load(){if((stage==4&&qualify==2||stage==5&&resetDone)&&run)load=true;}
        public void Fault(){if(stage==5&&run&&Healthy&&!sensorObserved)faultArmed=true;}
        public void Reset()
        {
            if(!ready||!calibrated||!Fresh||latch==null||!AtRest)return;
            latch=null;
            resetDone=sensorObserved;
            faultArmed=false;
        }
        public void Final(){if(stage==5&&resetDone&&Healthy&&run&&load)certify=true;}
        public void EmergencyStop()
        {
            stopped=!stopped;
            emergency.SetState(stopped?SparkSwitchState.Open:SparkSwitchState.Closed);
        }

        private string Prompt()
        {
            if(stage==6)return "CERTIFIED · Healthy loaded run and vibration feedback recovered.";
            if(latch!=null)return latch+(ready?" · Recalibrate, wait for samples, then reset.":" · Restore wiring and safety.");
            if(wiring!=null)return wiring;
            if(!rails)return "Restore the 5 V control rails.";
            if(stage==3)
            {
                if(run)return "Stop the motor and wait for zero speed.";
                if(!AtRest)return "Waiting for shaft to stop.";
                if(!isolated)return "Isolate the VFD drive.";
                if(!locked)return "Engage lockout.";
                if(door)return repair==4?"Release lockout.":"Open the guard for service.";
                return repair switch{0=>"Extract the damaged drive-end bearing.",1=>"Fit the healthy replacement.",2=>"Align the shaft.",3=>"Verify assembly torque.",_=>"Close the guard."};
            }
            if(!calibrated)return "Calibrate vibration, encoder, current and temperature channels.";
            if(isolated)return "Restore drive power.";
            if(!run)return "Start the motor.";
            return stage switch
            {
                1=>recording?"Recording steady healthy samples.":"Record a healthy baseline.",
                2=>!wear?"Apply BEARING FAULT.":DiagnosisReady?"Outer-race peak predominates. Select DIAGNOSE BPFO.":"Compare outer-race spectrum with alignment and gear bands.",
                4=>qualify==0?"Holding the low-speed test.":qualify==1?"Select SPEED TEST for nominal speed.":"Apply LOAD TEST.",
                5=>resetDone?load?"Certify a fresh loaded run.":"Apply LOAD TEST.":faultArmed?"Disconnect W70 · VIB3 X → FFT X.":"Select W70 TEST.",
                _=>report
            };
        }
        private void Present()
        {
            statusText.text=Prompt();
            meterText.text=$"SHAFT {rpm:F0} RPM  ·  GEARBOX {rpm/4:F0} RPM  ·  CURRENT {current:F2} A  ·  VIB {(Fresh?vibration.ToString("F2")+" mm/s":"INVALID")}";
            detailText.text=$"BPFO {bpfo:F1} Hz / BPFI {bpfi:F1} Hz  ·  OUTER RACE {raceMagnitude:F2}  ·  BEARING {heat:F1} °C  ·  MOTOR {motorHeat:F1} °C";
            calibrationText.text=$"{manager.SatisfiedTargetCount}/6  ·  {(locked?"LOCKED OUT":isolated?"DRIVE ISOLATED":"DRIVE READY")}  ·  {report}";
            if(controllerDisplay)controllerDisplay.text=$"{rpm:F0} RPM / {current:F2} A";
            if(qualityDisplay)qualityDisplay.text=Fresh?$"{vibration:F2} mm/s / {heat:F0} °C":"SIGNAL INVALID";
            if(plot)plot.Show(bins,Fresh);
            rig.Apply(rpm,door,stopped,damage,repair,serviceMotion,motorHeat,Time.deltaTime);
            powerButton.interactable=emergencyStopButton.interactable=true;
            calibrateButton.interactable=ready&&!calibrated&&!run&&AtRest;
            runButton.interactable=CanRun&&!run&&stage!=3;
            baselineButton.interactable=stage==1&&run&&Fresh&&!recording;
            wearButton.interactable=stage==2&&run&&!wear;
            diagnoseButton.interactable=DiagnosisReady;
            stopButton.interactable=run;
            isolateButton.interactable=AtRest&&!run&&!locked&&door&&(stage==3||latch==null&&isolated);
            lockButton.interactable=stage==3&&AtRest&&isolated&&door&&(!locked||repair==4);
            lockButton.GetComponentInChildren<Text>().text=locked?"RELEASE LOCKOUT":"LOCKOUT";
            guardButton.interactable=!door||locked&&isolated&&AtRest;
            extractButton.interactable=SafeService&&repair==0&&damage>.2f;
            installButton.interactable=SafeService&&repair==1&&serviceMotion>.95f;
            alignButton.interactable=SafeService&&repair==2;
            torqueButton.interactable=SafeService&&repair==3;
            speedButton.interactable=stage==4&&run&&qualify==1&&command<1000;
            loadButton.interactable=(stage==4&&qualify==2||stage==5&&resetDone)&&run&&!load;
            faultButton.interactable=stage==5&&run&&Healthy&&!sensorObserved&&!faultArmed;
            resetButton.interactable=ready&&calibrated&&Fresh&&latch!=null&&AtRest;
            finalButton.interactable=stage==5&&resetDone&&Healthy&&run&&load&&!certify;
        }
        private void OnDisable()
        {
            if(bindings!=null)foreach(string gate in Gates)bindings.ClearRuntimeComponentBinding(gate);
        }
    }
}



