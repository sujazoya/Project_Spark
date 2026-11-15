using UnityEngine;

namespace ProjectSpark.Gameplay
{
    // The beam and activation domains are normalized, fictional training models.
    public sealed class SparkLevel75Plant
    {
        public const float Step=.025f;
        public readonly float[] Dose=new float[16], Masked=new float[16], Depth=new float[16];
        public float Pressure {get;private set;}=500;
        public float LockPressure {get;private set;}=500;
        public float Backing {get;private set;}
        public float Turbo {get;private set;}
        public float Gate {get;private set;}
        public float Throttle {get;private set;}=1;
        public float Source {get;private set;}
        public float Inventory {get;private set;}=650;
        public float Field {get;private set;}=.77f;
        public float Energy {get;private set;}=.63f;
        public float Lens {get;private set;}=.44f;
        public float Steering {get;private set;}=.41f;
        public float Purity {get;private set;}
        public float Transmission {get;private set;}
        public float Upstream {get;private set;}
        public float NeutralFraction {get;private set;}
        public float Width {get;private set;}
        public float Centroid {get;private set;}
        public float Blanker {get;private set;}=1;
        public float Cup {get;private set;}
        public float Current {get;private set;}
        public float UsefulCurrent {get;private set;}
        public float Charge {get;private set;}
        public float Temperature {get;private set;}=23;
        public float Damage {get;private set;}
        public float Activation {get;private set;}
        public float Heater {get;private set;}=23;
        public float MeasuredDose {get;private set;}
        public float Angle {get;private set;}
        public float Rotation {get;private set;}
        public float ScanX {get;private set;}
        public float ScanY {get;private set;}
        public float Sheet => 1/Mathf.Max(.001f,MeanDose/100*Activation);
        public float Junction => MeanDepth*(.94f+.06f*Activation);
        public float MeanDose
        {
            get { float sum=0; foreach(float n in Dose)sum+=n;return sum/16; }
        }
        public float MeanDepth
        {
            get { float sum=0;foreach(float n in Depth)sum+=n;return sum/16; }
        }
        public float Difference
        {
            get
            {
                float min=float.PositiveInfinity,max=0;
                foreach(float n in Dose){min=Mathf.Min(min,n);max=Mathf.Max(max,n);}
                return (max-min)/Mathf.Max(1,MeanDose);
            }
        }
        public float Leakage
        {
            get {float max=0;foreach(float v in Masked)max=Mathf.Max(max,v);return max/Mathf.Max(1,MeanDose);}
        }
        public void NewWafer()
        {
            for(int i=0;i<16;i++)Dose[i]=Masked[i]=Depth[i]=0;
            Charge=Damage=Activation=MeasuredDose=0;
        }
        public void Tick(float dt,bool backing,bool turbo,bool load,bool source,bool analyzer,bool energy,bool optics,
            bool cooling,bool neutralize,bool blank,bool cup,bool activate,bool drift,bool defocus,bool noNeutral,int region,
            bool register,float time)
        {
            Backing=Mathf.MoveTowards(Backing,backing?1:0,dt*.93f);
            Turbo=Mathf.MoveTowards(Turbo,turbo&&Backing>.6f?1:0,dt*.59f);
            Gate=Mathf.MoveTowards(Gate,load?1:0,dt*3.01f);
            Throttle=Mathf.MoveTowards(Throttle,turbo&&Pressure<3?.81f:1,dt);
            float coupling=Gate*(LockPressure-Pressure)*.32f;
            Pressure=Mathf.Max(.001f,Pressure+dt*(.045f+(source?.24f:0)
                -Pressure*(.67f*Backing+2.4f*Turbo)*Throttle+coupling));
            LockPressure=Mathf.Max(.001f,LockPressure+dt*(.11f-LockPressure*(.68f*Backing+Turbo)-coupling*1.55f));

            Source=Mathf.MoveTowards(Source,source&&Inventory>.01f?1:0,dt*1.21f);
            Inventory=Mathf.Max(0,Inventory-dt*Source*.11f);
            Field=Mathf.MoveTowards(Field,analyzer?(drift?.89f:.60f):.77f,dt*.44f);
            Energy=Mathf.MoveTowards(Energy,energy?1:.63f,dt*.69f);
            Lens=Mathf.MoveTowards(Lens,optics?(defocus?.43f:.71f):.44f,dt*.58f);
            Steering=Mathf.MoveTowards(Steering,optics?.52f:.41f,dt*.61f);
            float error=(Field-.60f)/.12f;
            float selected=.74f*Mathf.Exp(-error*error*.88f);
            float competitor=.21f*Mathf.Exp(-((Field-.89f)/.09f)*((Field-.89f)/.09f));
            float neutrals=.008f+.021f*Mathf.Max(0,Pressure-.20f);
            Transmission=(selected+competitor)*Mathf.Exp(-Pressure*.08f);
            Purity=selected/Mathf.Max(.0001f,selected+competitor+neutrals);
            NeutralFraction=neutrals/(selected+competitor+neutrals);
            Upstream=Source*Transmission*(.75f+.25f*Energy);
            Width=.23f+Mathf.Abs(Lens-.71f)*1.3f+Pressure*.025f;
            Centroid=(Steering-.52f)*.85f+(Field-.60f)*.24f;
            Cup=Mathf.MoveTowards(Cup,cup?1:0,dt*4);
            Blanker=Mathf.MoveTowards(Blanker,blank?1:0,dt*(blank?4.8f:3.1f));
            Current=Mathf.Max(0,Upstream*.982f+.0015f*Mathf.Sin(time*17));
            UsefulCurrent=Upstream*(1-Blanker)*(1-Cup);
            Angle=Mathf.MoveTowards(Angle,register?6:0,dt*14);
            Rotation=Mathf.MoveTowards(Rotation,region>=0?8:0,dt*13);
            ScanX=Mathf.MoveTowards(ScanX,region>=0?(region%4-1.5f)/1.5f:0,dt*12);
            ScanY=Mathf.MoveTowards(ScanY,region>=0?(region/4-1.5f)/1.5f:0,dt*12);
            Charge=Mathf.Max(0,Charge+dt*(UsefulCurrent*2.65f-(neutralize&&!noNeutral?3.7f:.12f)*Charge));
            Temperature+=dt*(UsefulCurrent*Energy*6.1f-(cooling?.70f:.08f)*(Temperature-23));
            Heater=Mathf.Lerp(Heater,activate?73:23,dt/.49f);
            if(activate&&Heater>55)
            {
                Activation=Mathf.MoveTowards(Activation,1,dt*.69f*Mathf.Clamp01((Heater-55)/15));
                Damage=Mathf.MoveTowards(Damage,0,dt*.13f);
            }

            // Useful current is physically blocked by the gate or intercepted by the inserted cup.
            if(region<0||UsefulCurrent<.0001f)return;
            float radial=(Mathf.Abs(region%4-1.5f)+Mathf.Abs(region/4-1.5f))/3;
            float increment=UsefulCurrent*dt*158*Purity*(1.003f-radial*.007f-
                Mathf.Abs(Centroid)*.10f-Mathf.Max(0,Width-.245f)*.23f);
            float depth=Mathf.Sqrt(Energy)*(.994f+.003f*radial)*Mathf.Clamp01(1-Mathf.Abs(Angle-6)*.003f);
            Depth[region]=(Depth[region]*Dose[region]+depth*increment)/Mathf.Max(.00001f,Dose[region]+increment);
            Dose[region]+=increment;
            Masked[region]+=increment*(.0025f+.004f*Width+.006f*Mathf.Abs(Centroid));
            Damage+=increment*.00034f;
            MeasuredDose+=increment/16;
        }
    }
}
