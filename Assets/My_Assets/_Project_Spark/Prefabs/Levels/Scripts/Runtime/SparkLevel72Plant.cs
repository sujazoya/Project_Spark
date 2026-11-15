using UnityEngine;

namespace ProjectSpark.Gameplay
{
    // Reduced-order model for the fictional deposition simulator.
    public sealed class SparkLevel72Plant
    {
        public const float Step=.04f, PressureTarget=7.5f, HeatTarget=48f;
        public readonly float[] Temperature={23,23,23};
        public readonly float[] Film=new float[7];
        private readonly float[] coverage=new float[7], reaction=new float[7];
        private static readonly float[] transport={1f,.93f,.88f,1f,.88f,.76f,.68f};
        private float carrier=500, pressurePurge, qcm, optical;
        public float Pressure=>carrier+ResidualA+ResidualB+pressurePurge;
        public float Lock {get;private set;}=500;
        public float ResidualA {get;private set;}
        public float ResidualB {get;private set;}
        public float Backing {get;private set;}
        public float Turbo {get;private set;}
        public float Isolation {get;private set;}
        public float Throttle {get;private set;}=1;
        public float Gate {get;private set;}
        public float ValveA {get;private set;}
        public float ValveB {get;private set;}
        public float PurgeValve {get;private set;}
        public float FlowA {get;private set;}
        public float FlowB {get;private set;}
        public float PurgeFlow {get;private set;}
        public float StockA {get;private set;}=2800;
        public float StockB {get;private set;}=2800;
        public float StockP {get;private set;}=7000;
        public float SurfaceA=>coverage[0];
        public float SurfaceB=>reaction[0];
        public float Overlap=>ResidualA*ResidualB;
        public float Contamination {get;private set;}
        public float LastGrowth {get;private set;}
        public float Qcm=>qcm;
        public float Optical=>optical;
        public float MeanFilm=>(Film[0]+Film[1]+Film[2])/3f;
        public float Conformality=>Film[3]>.0001f?Film[6]/Film[3]:0;
        public float Uniformity=>(Mathf.Max(Film[0],Mathf.Max(Film[1],Film[2]))-Mathf.Min(Film[0],Mathf.Min(Film[1],Film[2])))/Mathf.Max(.001f,MeanFilm);
        public float Span=>Mathf.Max(Temperature[0],Mathf.Max(Temperature[1],Temperature[2]))-Mathf.Min(Temperature[0],Mathf.Min(Temperature[1],Temperature[2]));

        public void NewWafer()
        {
            for(int i=0;i<Film.Length;i++) Film[i]=0;
            Contamination=LastGrowth=qcm=optical=0;
            BeginCycle();
        }
        public void BeginCycle()
        {
            for(int i=0;i<Film.Length;i++) coverage[i]=reaction[i]=0;
        }
        public void CompleteCycle()
        {
            float before=MeanFilm;
            for(int i=0;i<Film.Length;i++)
                Film[i]+=.1008f*reaction[i]*Mathf.Clamp(1-.006f*Mathf.Abs(Temperature[i%3]-HeatTarget),.76f,1);
            LastGrowth=MeanFilm-before;
        }

        public void Tick(float dt,bool backing,bool turbo,bool isolation,float throttle,bool conditioning,
            bool load,bool heat,bool pulseA,bool pulseB,bool purge,bool restriction,bool drift,bool sticking,bool upstream,float a,float b)
        {
            Backing=Mathf.MoveTowards(Backing,backing?1:0,dt*1.04f);
            Turbo=Mathf.MoveTowards(Turbo,turbo&&Backing>.65f?1:0,dt*.78f);
            Isolation=Mathf.MoveTowards(Isolation,isolation?1:0,dt*2.4f);
            Throttle=Mathf.MoveTowards(Throttle,throttle,dt*2.1f);
            Gate=Mathf.MoveTowards(Gate,load?1:0,dt*3.5f);
            ValveA=Mathf.MoveTowards(ValveA,pulseA?1:sticking?.30f:0,dt*11f);
            ValveB=Mathf.MoveTowards(ValveB,pulseB?1:0,dt*11f);
            PurgeValve=Mathf.MoveTowards(PurgeValve,purge?1:0,dt*10f);
            FlowA=Mathf.Min(StockA/dt,upstream?5.4f*a*ValveA:0);
            FlowB=Mathf.Min(StockB/dt,upstream?5.4f*b*ValveB:0);
            PurgeFlow=Mathf.Min(StockP/dt,PurgeValve*(restriction?.32f:2.65f));
            StockA-=FlowA*dt; StockB-=FlowB*dt; StockP-=PurgeFlow*dt;
            float pump=Isolation*(Backing*.75f+Turbo*2.8f)*(.12f+.88f*Throttle);
            float cross=Gate*.45f*(Lock-carrier);
            carrier=Mathf.Max(.001f,carrier+dt*((conditioning?5.1f:.27f)-pump*carrier+cross));
            Lock=Mathf.Max(.001f,Lock+dt*(.45f-(.74f*Backing+1.27f*Turbo)*Lock-1.7f*cross));
            float clearing=pump+PurgeFlow*1.65f;
            ResidualA=Mathf.Max(0,ResidualA+dt*(FlowA-clearing*ResidualA));
            ResidualB=Mathf.Max(0,ResidualB+dt*(FlowB-clearing*ResidualB));
            pressurePurge=Mathf.Max(0,pressurePurge+dt*(PurgeFlow*.79f-pump*pressurePurge));
            for(int i=0;i<Temperature.Length;i++)
            {
                float t=Temperature[i];
                float demand=heat?Mathf.Clamp(.22f*(HeatTarget-t)+3.75f,0,20):0;
                if(heat&&drift&&i==2) demand*=.10f;
                Temperature[i]=t+dt*(demand-.15f*(t-23)+.038f*(Temperature[(i+1)%3]-t));
            }
            Contamination+=dt*Mathf.Max(0,Overlap-.038f)*.014f;
            for(int i=0;i<Film.Length;i++)
            {
                float rate=transport[i]*Mathf.Clamp(1-.015f*Mathf.Abs(Temperature[i%3]-HeatTarget),.38f,1);
                if(pulseA) coverage[i]+=dt*4.9f*ResidualA*rate*(1-coverage[i]);
                if(pulseB) reaction[i]+=dt*4.9f*ResidualB*rate*(coverage[i]-reaction[i]);
                coverage[i]=Mathf.Clamp01(coverage[i]);
                reaction[i]=Mathf.Min(coverage[i],Mathf.Clamp01(reaction[i]));
            }
            qcm=Mathf.Lerp(qcm,Film[0]*1.003f,dt/.28f);
            optical=Mathf.Lerp(optical,MeanFilm*.998f,dt/.41f);
        }
    }
}
