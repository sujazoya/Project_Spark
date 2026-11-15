using UnityEngine;

namespace ProjectSpark.Gameplay
{
    // Fictional CMP process with separately modeled industrial drives and 5 V controls.
    public sealed class SparkLevel73Plant
    {
        public const float Step=.035f;
        public readonly float[] Film=new float[3], Force=new float[3], RemovalRate=new float[3], Height=new float[3];
        private readonly float[] trim=new float[3], original=new float[3];
        private static readonly float[] Radius={.027f,.066f,.098f};
        private float platenOmega,carrierOmega,optical,eddy,trackedRate;
        public float PlatenRpm=>platenOmega*9.5493f;
        public float CarrierRpm=>carrierOmega*9.5493f;
        public float PlatenTorque {get;private set;}
        public float CarrierTorque {get;private set;}
        public float Current=>.43f+Mathf.Abs(PlatenTorque)*3.5f+Mathf.Abs(CarrierTorque)*3.2f;
        public float Feed {get;private set;}
        public float Concentration {get;private set;}
        public float Reserve {get;private set;}=360f;
        public float Pad {get;private set;}=.86f;
        public float Wear {get;private set;}
        public float Position {get;private set;}
        public float Ring {get;private set;}
        public float Downforce=>Force[0]+Force[1]+Force[2];
        public float Temperature {get;private set;}=23f;
        public float MotorTemperature {get;private set;}=23f;
        public float Vibration {get;private set;}
        public float Acoustic {get;private set;}
        public float Dishing {get;private set;}
        public float Erosion {get;private set;}
        public float Defects {get;private set;}
        public float Residue {get;private set;}
        public float Moisture {get;private set;}
        public float Optical=>optical;
        public float Eddy=>eddy;
        public float Rate=>trackedRate;
        public float MeanFilm=>(Film[0]+Film[1]+Film[2])/3;
        public float Planarity=>Mathf.Max(Height[0],Mathf.Max(Height[1],Height[2]))-Mathf.Min(Height[0],Mathf.Min(Height[1],Height[2]));
        public float RemovalUniformity
        {
            get
            {
                float a=original[0]-Film[0],b=original[1]-Film[1],c=original[2]-Film[2];
                return (Mathf.Max(a,Mathf.Max(b,c))-Mathf.Min(a,Mathf.Min(b,c)))/Mathf.Max(1,(a+b+c)/3);
            }
        }

        public SparkLevel73Plant()=>NewWafer();
        public void NewWafer()
        {
            original[0]=Film[0]=102;original[1]=Film[1]=101;original[2]=Film[2]=100;
            Height[0]=13;Height[1]=25;Height[2]=39;
            Dishing=Erosion=Defects=Residue=Moisture=trackedRate=0;
            optical=MeanFilm/(MeanFilm+35);eddy=MeanFilm;
            for(int i=0;i<trim.Length;i++)trim[i]=0;
        }

        private static float Drive(float dt, bool enabled,float request,float inertia,float load,ref float speed)
        {
            float torque=enabled?Mathf.Clamp(.57f*(request-speed)+load,-1.15f,1.3f):0;
            speed=Mathf.Max(0,speed+dt*(torque-.048f*speed-load)/inertia);
            return torque;
        }
        public void Tick(float dt,bool platen,bool carrier,float platenRequest,float carrierRequest,bool feed,bool cool,
            bool conditioning,bool contact,bool removal,bool glaze,bool restriction,bool drift,int wash,float time)
        {
            float drag=(.075f+.18f*(1-Mathf.Clamp01(Feed))+.09f*(1-Pad))*Downforce/3;
            PlatenTorque=Drive(dt,platen,platenRequest*.10472f,.13f,drag,ref platenOmega);
            CarrierTorque=Drive(dt,carrier,carrierRequest*.10472f,.085f,drag*.47f,ref carrierOmega);
            Position=Mathf.MoveTowards(Position,contact?1:0,dt*(contact?2.2f:1.3f));
            Feed=Mathf.Min(Reserve/dt,Mathf.MoveTowards(Feed,feed?(restriction?.19f:1.04f):0,dt*2.2f));
            Reserve-=Feed*dt*.32f;
            Concentration=Mathf.Lerp(Concentration,Feed>.08f?.87f:0,dt/.47f);
            Pad=Mathf.Clamp(Pad+dt*(conditioning?.31f:glaze?-.58f:-Downforce*.0003f),.29f,1);
            Wear=Mathf.Clamp01(Wear+dt*(conditioning?.010f:Downforce*.00035f));
            Ring=Mathf.MoveTowards(Ring,contact?.82f:0,dt*3f);
            float sum=0;
            for(int i=0;i<Film.Length;i++)
            {
                // Mean squared sliding speed around each ring from two distinct rotation axes.
                float translation=platenOmega*.23f, spin=(platenOmega-carrierOmega)*Radius[i];
                float velocity=Mathf.Sqrt(translation*translation+spin*spin);
                float gap=Mathf.Clamp01((Position-.38f)/.62f);
                float request=contact?gap*(1.02f/(1+Radius[i]*.056f)+(drift&&i==2?.43f:0)):0;
                if(contact)
                {
                    trim[i]=Mathf.Clamp(trim[i]+(request-Force[i])*dt*1.18f,-.10f,.19f);
                    request+=trim[i];
                }
                else trim[i]=0;
                Force[i]=Mathf.MoveTowards(Force[i],Mathf.Clamp(request,0,1.6f),dt*(contact?3.3f:1.55f));
                float rate=removal?5.5f*Force[i]*velocity*Mathf.Clamp01(Feed)*Concentration*Pad*(1-.18f*Wear):0;
                RemovalRate[i]=Mathf.Min(Film[i]/dt,rate);
                Film[i]-=RemovalRate[i]*dt;
                Height[i]=9+(Height[i]-9)*Mathf.Exp(-RemovalRate[i]*dt*.036f);
                sum+=RemovalRate[i];
            }
            trackedRate=Mathf.Lerp(trackedRate,sum/3,dt/.22f);
            Temperature+=dt*(6.0f*drag+.028f*(Mathf.Abs(PlatenTorque)*platenOmega+Mathf.Abs(CarrierTorque)*carrierOmega)
                -(.065f+(cool?.24f:0))*(Temperature-23));
            MotorTemperature+=dt*(Current*.24f-.12f*(MotorTemperature-23));
            Vibration=.028f+.06f*(1-Pad)*Downforce+.025f*Mathf.Abs(Force[0]-Force[2]);
            Acoustic=.056f+drag*.34f+Vibration*.42f;
            Dishing+=dt*trackedRate*.025f*(MeanFilm<26?2.1f:1);
            Erosion+=dt*trackedRate*.021f*(1+Mathf.Max(0,Force[2]-Force[0]));
            if(removal)
            {
                Defects+=dt*Downforce*(Mathf.Max(0,.68f-Feed)*.35f+Mathf.Max(0,.58f-Pad)*.24f);
                Residue=Mathf.Min(1,Residue+dt*Feed*.18f);
            }
            if(wash==1){Residue=Mathf.MoveTowards(Residue,.16f,dt*.9f);Moisture=Mathf.MoveTowards(Moisture,1,dt*1.9f);}
            if(wash==2){Residue=Mathf.MoveTowards(Residue,0,dt*.43f);Defects=Mathf.MoveTowards(Defects,Defects*.88f,dt*.015f);}
            if(wash==3)Moisture=Mathf.MoveTowards(Moisture,0,dt*.68f);
            float feature=MeanFilm/(MeanFilm+35)+.006f*Mathf.Sin(time*12.9f)+.014f*(1-Mathf.Clamp01(Feed));
            optical=Mathf.Lerp(optical,feature,dt/.16f);
            eddy=Mathf.Lerp(eddy,MeanFilm+.31f*Mathf.Sin(time*9.5f),dt/.20f);
        }
    }
}
