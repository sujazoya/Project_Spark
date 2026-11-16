using UnityEngine;

namespace ProjectSpark.Gameplay
{
    public sealed class SparkLevel83Plant
    {
        public const float Step=.04f, Nominal=28, Cycle=120;
        private const float Capacitance=.58f, BusEnergyLimit=.5f*Capacitance*32*32;
        public sealed class Wing
        {
            public float Deploy, Azimuth, Elevation, Voltage, Current, Efficiency, Available, Delivered;
            public bool Home, SweepDone;
            private float clock, priorPower, direction=1, target=.42f, sweepBest, sweepBestTarget;
            private int sweepIndex=-1;
            public void StartSweep(){sweepIndex=0;sweepBest=0;clock=0;target=.18f;SweepDone=false;}
            public void StepMotion(float dt,bool deploy,bool home,bool track,float sunAz,float sunEl,bool jam)
            {
                Deploy=Mathf.MoveTowards(Deploy,deploy?1:0,dt*.27f);
                if(!jam)
                {
                    Azimuth=Mathf.MoveTowards(Azimuth,track?sunAz:0,dt*42);
                    Elevation=Mathf.MoveTowards(Elevation,track?sunEl:0,dt*32);
                }
                if(home&&Deploy>.99f&&Mathf.Abs(Azimuth)+Mathf.Abs(Elevation)<.6f)Home=true;
            }
            public void StepPv(float dt,float sun,float sunAz,float sunEl,bool track,bool mppt,float shade)
            {
                float cosine=Mathf.Clamp01(Mathf.Cos((Azimuth-sunAz)*Mathf.Deg2Rad)*Mathf.Cos((Elevation-sunEl)*Mathf.Deg2Rad));
                float irradiance=sun*Deploy*cosine*cosine;
                float open=42-4*irradiance-9*shade;
                float isc=2.45f*irradiance*(1-.34f*shade);
                float x=Mathf.MoveTowards(open>.1f?Voltage/open:0,target,dt*1.6f);
                Voltage=sun>0?x*open:0;
                Current=isc*(1-Mathf.Pow(x,5));
                float p=Voltage*Current;
                Available=open*isc*.6988f*.8333f;
                Efficiency=Available>1?Mathf.Clamp01(p/Available):0;
                clock+=dt;
                if(clock<.6f)return;
                clock=0;
                if(sweepIndex>=0&&track&&irradiance>.2f)
                {
                    if(p>sweepBest){sweepBest=p;sweepBestTarget=x;}
                    if(++sweepIndex==9){sweepIndex=-1;SweepDone=sweepBest>5;target=sweepBestTarget;priorPower=0;}
                    else target=.18f+.095f*sweepIndex;
                }
                else if(mppt&&SweepDone&&irradiance>.25f)
                {
                    if(p<priorPower-.035f)direction=-direction;
                    target=Mathf.Clamp(target+direction*.014f,.46f,.88f);
                    priorPower=p;
                }
            }
        }
        public sealed class Pack
        {
            public const float Capacity=2600;
            public float Energy=Capacity*.69f, Temperature=25, Mismatch=.025f, Current, Contact;
            public float SOC=>Energy/Capacity;
            public float Low=>3.26f+SOC*.94f-Mismatch-Mathf.Max(0,Current)*.017f;
            public float High=>3.26f+SOC*.94f+Mismatch+Mathf.Max(0,-Current)*.012f;
            public bool Healthy=>Temperature<48&&Low>3.05f&&High<4.25f&&SOC>.10f;
            public float ChargeCapacity(float dt)=>High<4.15f&&SOC<.92f?Mathf.Min(27,(Capacity*.92f-Energy)/(.92f*dt)):0;
            public float StepPower(float dt,float requested,bool command,bool balance,float bus)
            {
                Contact=Mathf.MoveTowards(Contact,command&&Healthy?1:0,dt*1.7f);
                float charge=ChargeCapacity(dt);
                float discharge=Low>3.13f?Mathf.Min(49,Mathf.Max(0,Energy-Capacity*.10f)*.94f/dt):0;
                float actual=Mathf.Clamp(requested,-charge,discharge)*Contact;
                Current=actual/Mathf.Max(19,bus);
                Energy=Mathf.Clamp(Energy+(actual>=0?-actual/.94f:-actual*.92f)*dt,0,Capacity);
                Temperature+=dt*(Current*Current*.052f-(Temperature-25)*.042f);
                if(balance&&Mismatch>.012f)
                {
                    Mismatch=Mathf.MoveTowards(Mismatch,.012f,dt*.035f);
                    Energy=Mathf.Max(0,Energy-.65f*dt);
                }
                return actual;
            }
        }

        public readonly Wing Port=new(), Starboard=new();
        public readonly Pack A=new(), B=new();
        public float OrbitTime, BusEnergy, SuperEnergy=44, ActualContactor, MainContactor, ActualLoad, Supply, OpticalActual, MinVoltage=40, Harvested, Used, Spilled;
        public float LastEnergyError, AbsError, Sun, Demand, P0, P1, P2, P3, Thermal;
        public float Bus=>Mathf.Sqrt(2*BusEnergy/Capacitance);
        public float Phase=>OrbitTime/Cycle*360;
        public float SunAz=>-28+57*Mathf.Sin(Phase*Mathf.Deg2Rad);
        public float SunEl=>15+9*Mathf.Sin((Phase-18)*Mathf.Deg2Rad);
        public float Reserve=>A.Energy+B.Energy-34*35/.94f;
        public bool Eclipse=>OrbitTime>=45&&OrbitTime<80;
        public bool Recharged;
        public bool Running, Completed, RunFailed, SawEclipse, SawReturn, OpticalDelivered, ScienceDelivered;
        public bool DischargedA, DischargedB, ChargedA, ChargedB;
        public float EclipseTime, OpticalWork, ScienceWork;
        public void StartRun()
        {
            OrbitTime=EclipseTime=OpticalWork=ScienceWork=0; MinVoltage=40;
            Running=true;Recharged=Completed=RunFailed=SawEclipse=SawReturn=OpticalDelivered=ScienceDelivered=false;
        }
        public void EndRun(){Running=false;RunFailed=true;}
        public void StepPlant(float dt,bool deployA,bool deployB,bool home,bool track,bool mppt,bool precharge,bool closeMain,
            bool packs,bool balance,bool normal,bool powered,bool emergency,bool checkUv,bool plan,
            float measured,float jam,float imbalance,bool burst,bool capOptical)
        {
            if(Running)OrbitTime=Mathf.Min(Cycle,OrbitTime+dt);
            Sun=OrbitTime switch { <43=>1, <45=>(45-OrbitTime)/2, <80=>0, <82=>(OrbitTime-80)/2, _=>1 };
            Port.StepMotion(dt,deployA,home,track,SunAz,SunEl,jam>.5f);
            Starboard.StepMotion(dt,deployB,home,track,SunAz,SunEl,false);
            Port.StepPv(dt,Sun,SunAz,SunEl,track,mppt,jam*.34f);
            Starboard.StepPv(dt,Sun,SunAz,SunEl,track,mppt,0);
            if(!Running&&imbalance>0)B.Mismatch=Mathf.MoveTowards(B.Mismatch,imbalance,dt*.12f);
            bool isolate=!powered||emergency;
            ActualContactor=Mathf.MoveTowards(ActualContactor,precharge&&!isolate?1:0,dt*.8f);
            MainContactor=Mathf.MoveTowards(MainContactor,closeMain&&!isolate?1:0,dt*1.7f);
            float bus=Bus;
            float feedback=normal?measured:0;
            bool keepEssentials=checkUv&&!isolate&&MainContactor>.85f;
            float p0=keepEssentials?19:0,p1=keepEssentials?11:0,thermal=keepEssentials?4:0;
            float optical=normal&&plan&&Running&&OrbitTime>=88&&OrbitTime<113?18:0;
            if(burst&&!capOptical&&normal&&Eclipse)optical+=64;
            OpticalActual=Mathf.MoveTowards(OpticalActual,optical,dt*85);
            float science=normal&&plan&&Running&&OrbitTime>=8&&OrbitTime<34?11:0;
            P0=p0;P1=p1;P2=keepEssentials?OpticalActual:0;P3=keepEssentials?science:0;Thermal=thermal;
            Demand=P0+P1+P2+P3+Thermal;
            float loadScale=Mathf.Clamp01(bus/21);
            float load=Demand*loadScale;
            float loss=.065f*bus;
            float drop=Nominal-feedback;
            float desired=normal?Mathf.Clamp(load+loss+drop*14,0,120):Mathf.Clamp(load+loss+Mathf.Clamp(24-bus,-2,5)*7,0,52);
            if(MainContactor<.9f)desired=ActualContactor*Mathf.Clamp((26.6f-bus)*4,0,19);
            if(isolate)desired=0;
            float availableA=Mathf.Min(49,Port.Voltage*Port.Current*.91f);
            float availableB=Mathf.Min(49,Starboard.Voltage*Starboard.Current*.90f);
            float total=availableA+availableB;
            float charge=normal&&packs&&MainContactor>.9f?
                Mathf.Clamp((total-load-4)*.55f,0,2*Mathf.Min(A.ChargeCapacity(dt)*A.Contact,B.ChargeCapacity(dt)*B.Contact)):0;
            float production=mppt&&!isolate?Mathf.Min(total,desired+charge):0;
            float share=total>.001f?availableA/total:.5f;
            Port.Delivered=Mathf.Min(availableA,Mathf.MoveTowards(Port.Delivered,production*share,dt*85));
            Starboard.Delivered=Mathf.Min(availableB,Mathf.MoveTowards(Starboard.Delivered,production*(1-share),dt*85));
            if(isolate)Port.Delivered=Starboard.Delivered=0;
            float solar=Port.Delivered+Starboard.Delivered;
            float dispatch=packs&&MainContactor>.9f&&!isolate?Mathf.Clamp(desired-solar,-charge,normal?95:47):0;
            if(charge>1&&solar>load+loss+2)dispatch=-Mathf.Min(charge,solar-load-loss);
            float a=A.StepPower(dt,dispatch*.5f,packs&&!isolate,balance,bus);
            float b=B.StepPower(dt,dispatch*.5f,packs&&!isolate,balance,bus);
            DischargedA|=A.Current>.18f;DischargedB|=B.Current>.18f;
            ChargedA|=A.Current<-.16f;ChargedB|=B.Current<-.16f;
            float cap=MainContactor>.8f&&!isolate&&bus<26.7f?Mathf.Min(SuperEnergy/dt,Mathf.Min(33,(26.7f-bus)*18)):
                normal&&bus>27.4f&&solar>load+loss+6?-Mathf.Min(3,(44-SuperEnergy)/(.94f*dt)):0;
            SuperEnergy=Mathf.Clamp(SuperEnergy+(cap>=0?-cap/.96f:-cap*.94f)*dt,0,44);
            float delta=(solar+a+b+cap-load-loss)*dt;
            float before=BusEnergy;
            BusEnergy=Mathf.Clamp(before+delta,0,BusEnergyLimit);
            float spill=Mathf.Max(0,before+delta-BusEnergy), unavailable=Mathf.Max(0,BusEnergy-before-delta);
            LastEnergyError=BusEnergy-before-delta+spill-unavailable;
            AbsError+=Mathf.Abs(LastEnergyError);
            Harvested+=solar*dt;Used+=Mathf.Max(0,load*dt-unavailable);Spilled+=spill;
            ActualLoad=load;Supply=solar+a+b+cap;
            if(!Running)return;
            MinVoltage=Mathf.Min(MinVoltage,Bus);
            RunFailed|=Bus<23.5f||Bus>30.2f||!checkUv||isolate||!A.Healthy||!B.Healthy||p0<18||p1<10||!normal;
            if(Eclipse){SawEclipse=true;EclipseTime+=dt;RunFailed|=solar>.05f||!DischargedA||!DischargedB||A.SOC<.18f||B.SOC<.18f;}
            SawReturn|=SawEclipse&&OrbitTime>82&&solar>15;
            Recharged|=SawReturn&&A.Current<-.15f&&B.Current<-.15f;
            if(Bus>25&&normal){OpticalWork+=Mathf.Min(P2,18)*dt;ScienceWork+=P3*dt;}
            OpticalDelivered=OpticalWork>=350;ScienceDelivered=ScienceWork>=245;
            if(OrbitTime>=Cycle)
            {
                Completed=SawEclipse&&SawReturn&&EclipseTime>34.9f&&!RunFailed&&OpticalDelivered&&ScienceDelivered&&Recharged;
                Running=false;
            }
        }
    }
}
