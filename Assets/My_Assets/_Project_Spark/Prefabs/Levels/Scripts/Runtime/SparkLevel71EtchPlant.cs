using UnityEngine;

namespace ProjectSpark.Gameplay
{
    // Fictional normalized chemistry and reduced-order vacuum/RF model for the training simulation.
    public sealed class SparkLevel71EtchPlant
    {
        public const float Step=.04f, SetPressure=7.5f, InitialFilm=300f;
        public float Pressure {get;private set;}=500f;
        public float LockPressure {get;private set;}=500f;
        public float Backing {get;private set;}
        public float Turbo {get;private set;}
        public float Gate {get;private set;}
        public float SupplyA {get;private set;}=800f;
        public float SupplyB {get;private set;}=600f;
        public float Throttle {get;private set;}
        public float Isolation {get;private set;}
        public float FlowA {get;private set;}
        public float FlowB {get;private set;}
        public float TuneA {get;private set;}=.24f;
        public float TuneB {get;private set;}=.71f;
        public float Forward {get;private set;}
        public float Reflected {get;private set;}
        public float Delivered {get;private set;}
        public float Reflection {get;private set;}
        public float Activity {get;private set;}
        public float Temperature {get;private set;}=23f;
        public float Chiller {get;private set;}
        public float Damage {get;private set;}
        public float MaskLoss {get;private set;}
        public float Signal {get;private set;}
        public float Reference {get;private set;}
        public float Conductance {get;private set;}
        public float Inflow {get;private set;}
        public float BalanceError {get;private set;}
        public readonly float[] Film={300,300,300,300,300};

        public float MeanFilm =>(Film[0]+Film[1]+Film[2]+Film[3]+Film[4])*.2f;
        public float WorstFilm =>Mathf.Max(Mathf.Max(Film[0],Film[1]),Mathf.Max(Film[2],Mathf.Max(Film[3],Film[4])));
        public float NonUniformity
        {
            get
            {
                float min=InitialFilm,max=0;
                foreach(float region in Film){min=Mathf.Min(region,min);max=Mathf.Max(region,max);}
                return (max-min)/Mathf.Max(1,InitialFilm-MeanFilm);
            }
        }
        public void NewFilm()
        {
            for(int i=0;i<Film.Length;i++) Film[i]=InitialFilm;
            Damage=MaskLoss=0;
        }

        public sealed class Pi
        {
            private float integrated;
            public void Reset()=>integrated=0;
            public float Drive(float error,float bias,float kp,float ki,float lo,float hi,float dt)
            {
                float raw=bias+error*kp+integrated*ki;
                if((raw<hi||error<0)&&(raw>lo||error>0)) integrated=Mathf.Clamp(integrated+error*dt,-18,18);
                return Mathf.Clamp(bias+error*kp+integrated*ki,lo,hi);
            }
        }
        public void Tick(float dt,bool backing,bool turbo,bool isolation,float throttle,float commandA,float commandB,
            bool rf,float power,bool matching,float capA,float capB,bool ignite,bool etching,bool cooling,float leak,float drift,float time,bool transfer)
        {
            Backing=Mathf.MoveTowards(Backing,backing?1:0,dt*.85f);
            Turbo=Mathf.MoveTowards(Turbo,turbo&&Backing>.6f?1:0,dt*.55f);
            Isolation=Mathf.MoveTowards(Isolation,isolation?1:0,dt*1.45f);
            Throttle=Mathf.MoveTowards(Throttle,throttle,dt*1.2f);
            Gate=Mathf.MoveTowards(Gate,transfer?1:0,dt*2.6f);
            FlowA=Mathf.Min(SupplyA/dt,Mathf.MoveTowards(FlowA,commandA,dt*.75f));
            SupplyA-=FlowA*dt;
            FlowB=Mathf.Min(SupplyB/dt,Mathf.MoveTowards(FlowB,commandB,dt*.66f));
            SupplyB-=FlowB*dt;
            Inflow=.36f+5.1f*(FlowA+FlowB)+leak;
            Conductance=(.66f*Backing+1.43f*Turbo)*Isolation*(.10f+.90f*Throttle);
            float prior=Pressure;
            float equalization=Gate*.52f*(LockPressure-prior);
            float expected=dt*(Inflow-Conductance*prior+equalization);
            Pressure=Mathf.Max(.001f,prior+expected);
            BalanceError=Mathf.Abs(Pressure-prior-expected);
            LockPressure=Mathf.Max(.001f,LockPressure+dt*(.55f-(.70f*Backing+1.02f*Turbo)*LockPressure-1.8f*equalization));

            float desiredA=.59f+.11f*FlowA+.17f*drift;
            float desiredB=.36f+.11f*FlowB-.18f*drift;
            TuneA=Mathf.MoveTowards(TuneA,matching?Mathf.Clamp01(desiredA):capA,dt*.225f);
            TuneB=Mathf.MoveTowards(TuneB,matching?Mathf.Clamp01(desiredB):capB,dt*.225f);
            float resistance=1.03f+3f*(TuneA-desiredA);
            float reactance=5.5f*(TuneB-desiredB)+2.4f*(TuneA-desiredA);
            resistance=Mathf.Max(.17f,resistance);
            Reflection=Mathf.Clamp01(((resistance-1)*(resistance-1)+reactance*reactance)/
                ((resistance+1)*(resistance+1)+reactance*reactance));
            Forward=rf?Mathf.MoveTowards(Forward,power,dt*1.25f):0;
            Reflected=Forward*Reflection;
            Delivered=(Forward-Reflected)*.92f;
            float pressureQuality=Mathf.Clamp01(1-Mathf.Abs(Pressure-SetPressure)/8.5f);
            float flow=FlowA+FlowB;
            float chemistry=flow>.05f?Mathf.Clamp01(1-Mathf.Abs(FlowA/flow-.62f)*1.6f):0;
            float activity=ignite&&rf&&flow>.55f?Mathf.Clamp01((Delivered-.15f)*1.55f)*pressureQuality*chemistry:0;
            Activity=Mathf.MoveTowards(Activity,activity,dt*.88f);
            Chiller=Mathf.MoveTowards(Chiller,cooling?1:0,dt*.8f);
            Temperature+=dt*(4.85f*Activity+.6f*Delivered-(.030f+.181f*Chiller)*(Temperature-22f));
            if(etching&&Activity>.13f)
            {
                float rate=(19+21*Delivered)*Activity*chemistry*Mathf.Clamp(1+(Temperature-36)*.006f,.75f,1.18f);
                for(int i=0;i<Film.Length;i++)
                {
                    float removed=rate*dt*(1+(i-2)*.0043f*(1+2*Reflection));
                    Damage+=Mathf.Max(0,removed-Film[i])*.041f;
                    Film[i]=Mathf.Max(0,Film[i]-removed);
                }
                MaskLoss+=rate*dt*.067f;
            }

            Reference=Mathf.Max(0,Activity*(.82f+.16f*FlowB));
            float feature=.072f+.928f*MeanFilm/(MeanFilm+14f);
            Signal=Mathf.Max(0,Reference*(feature+.009f*Mathf.Sin(time*11.9f)+.004f*Mathf.Sin(time*31.1f)));
        }
    }
}
