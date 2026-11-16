using UnityEngine;

namespace ProjectSpark.Gameplay
{
    public sealed class SparkLevel86Plant
    {
        public const float Step=.04f;
        public static readonly Vector3 TestPoint=new(2.45f,4.94f,1.28f);
        public static readonly Vector3 PanelSocket=new(6.80f,5.50f,2.17f);
        public static readonly Vector3 RetreatPoint=new(5.72f,5.36f,1.76f);
        public static readonly Vector3 Cradle=new(8.38f,1.64f,-2.72f);
        public static readonly Vector3 ReplacementRack=new(7.55f,1.74f,-3.23f);
        private static readonly Vector3 TipHome=new(1.65f,5.22f,1.52f);
        private static readonly Vector3[] Centers={
            new(-4.55f,2.52f,1.52f),new(-4.15f,3.03f,1.52f),new(-3.30f,3.67f,1.52f),new(-2.35f,4.22f,1.52f),
            new(-1.30f,4.74f,1.52f),new(-.55f,5.15f,1.52f),new(.25f,5.42f,1.52f)};
        private static readonly Vector3[] Axes={Vector3.up,Vector3.forward,Vector3.forward,Vector3.forward,Vector3.right,Vector3.forward,Vector3.right};
        private static readonly float[] Limits={108,86,100,112,92,95,120};
        private readonly float[] goal=new float[8];
        private readonly float[] actual={-5,7,-4,8,2,-5,3,0};
        public readonly float[] Speed=new float[8];
        public float J(int index)=>actual[index];
        public float Rail=>actual[7];
        public Vector3 Tool{get;private set;}
        public Vector3 Goal{get;private set;}=TipHome;
        public float MoveError=>Vector3.Distance(Tool,Goal);
        public float PlanError{get;private set;}
        public Vector3 WrenchForce{get;private set;}
        public Vector3 WrenchTorque{get;private set;}
        public float Force=>WrenchForce.magnitude;
        public float Clock,EncoderBias,LeftLoad,RightLoad,Jaw=1,FlexMotion,JawFault,DynamicDeflection;
        public float EncoderAlignment{get;private set;}
        public float StereoAlignment{get;private set;}
        public float WrenchCalibration{get;private set;}
        public float Calibration=>Mathf.Min(EncoderAlignment,Mathf.Min(StereoAlignment,WrenchCalibration));
        public float Pressure=.79f,Flow=.44f,Temperature=70.0f,Valve=.86f,Leak=.08f,HeatRejected,CoolStart;
        public readonly float[] Bolts={1,1,1,1};
        public int Carried{get;private set;}
        public bool ReplacementInstalled,LoopTested;
        public Vector3 FailedPose{get;private set;}=PanelSocket;
        public Vector3 ReplacementPose{get;private set;}=ReplacementRack;
        private float flexVelocity,jawImbalance,toolSpeed,settleTime,leakTime,wrenchSampleClock;
        public float WrenchSampleAge{get;private set;}=10;
        public int WrenchSequence{get;private set;}
        public bool AtGoal=>settleTime>.28f;
        public bool Stopped=>toolSpeed<.10f&&MaxSpeed<.55f;
        public float TipSpeed=>toolSpeed;
        public float MaxSpeed
        {
            get {float speed=0;for(int i=0;i<7;i++)speed=Mathf.Max(speed,Mathf.Abs(Speed[i]));return speed;}
        }
        public bool HomeReady=>Mathf.Abs(Rail)<.025f&&AtGoal&&Vector3.Distance(Tool,TipHome)<.065f;
        public bool Unbolted=>Bolts[0]<.015f&&Bolts[1]<.015f&&Bolts[2]<.015f&&Bolts[3]<.015f;
        public bool Fastened=>Bolts[0]>.987f&&Bolts[1]>.987f&&Bolts[2]>.987f&&Bolts[3]>.987f;
        public bool ContactSafe=>Force<25&&WrenchTorque.magnitude<12;
        public bool BalancedGrip=>Jaw<.19f&&LeftLoad>4.3f&&RightLoad>4.3f&&Mathf.Abs(LeftLoad-RightLoad)<3.1f;
        public bool Cool=>LoopTested&&Pressure>.71f&&Flow>.79f&&Leak<.018f&&Temperature<=CoolStart-2.25f&&HeatRejected>.54f;
        public bool Vented=>Flow<.055f&&Pressure<.09f;
        public SparkLevel86Plant(){Tool=Forward(actual);}

        private static Vector3 Forward(float[] settings)
        {
            Vector3 point=Centers[0]+Vector3.right*settings[7];
            var rotation=Quaternion.identity;
            for(int i=0;i<7;i++)
            {
                rotation*=Quaternion.AngleAxis(settings[i],Axes[i]);
                Vector3 next=i==6?TipHome:Centers[i+1];
                point+=rotation*(next-Centers[i]);
            }
            return point;
        }
        public void Home()
        {
            System.Array.Clear(goal,0,goal.Length);
            Goal=TipHome;PlanError=0;settleTime=0;
        }
        public void Move(Vector3 target)
        {
            // Solve only when the requested work station changes. The real axes then
            // follow the setpoints with bounded acceleration and position feedback.
            Goal=target;settleTime=0;
            var trial=new float[8];
            trial[7]=Mathf.Clamp(target.x-TipHome.x,0,10.8f);
            var columns=new Vector3[8];
            for(int iteration=0;iteration<215;iteration++)
            {
                Vector3 position=Forward(trial),error=target-position;
                if(error.magnitude<.022f)break;
                Vector3 rowX=new(.025f,0,0),rowY=new(0,.025f,0),rowZ=new(0,0,.025f);
                for(int i=0;i<8;i++)
                {
                    float step=i==7?.035f:.3f;
                    trial[i]+=step;
                    Vector3 column=(Forward(trial)-position)*(i==7?.55f/step:10/step);
                    trial[i]-=step;columns[i]=column;
                    rowX+=column*column.x;rowY+=column*column.y;rowZ+=column*column.z;
                }
                float det=Vector3.Dot(rowX,Vector3.Cross(rowY,rowZ));
                var solved=(Vector3.Cross(rowY,rowZ)*error.x+Vector3.Cross(rowZ,rowX)*error.y+
                    Vector3.Cross(rowX,rowY)*error.z)/det;
                for(int i=0;i<8;i++)
                {
                    float scale=i==7?.55f:10;
                    float lower=i==7?0:-Limits[i],upper=i==7?10.8f:Limits[i];
                    trial[i]=Mathf.Clamp(trial[i]+scale*Mathf.Clamp(Vector3.Dot(columns[i],solved),-.65f,.65f),lower,upper);
                }
            }
            PlanError=Vector3.Distance(Forward(trial),target);
            System.Array.Copy(trial,goal,8);
        }

        public void BeginService(){LoopTested=false;leakTime=0;}
        public void BeginCooling(){CoolStart=Temperature;leakTime=0;}
        public void Attach(int item){if(Carried==0&&BalancedGrip&&AtGoal)Carried=item;}
        public void Support(Vector3 mount)
        {
            if(!AtGoal||Vector3.Distance(Tool,mount)>.24f||Carried==0)return;
            if(Carried==1)FailedPose=mount;else ReplacementPose=mount;
            Carried=0;
        }
        public void Install(){if(Carried==2&&AtGoal){Support(PanelSocket);ReplacementInstalled=true;}}
        public void Tick(float dt,bool power,bool motion,bool calibrate,bool trustedSample,bool grip,bool preload,
            int bolts,bool pump,int disturbance,bool repair)
        {
            Clock+=dt;
            if(calibrate&&power&&HomeReady)
            {
                EncoderAlignment=Mathf.MoveTowards(EncoderAlignment,1,dt*.93f);
                StereoAlignment=Mathf.MoveTowards(StereoAlignment,1,dt*.79f);
                if(Force<1.2f&&WrenchTorque.magnitude<.4f)
                    WrenchCalibration=Mathf.MoveTowards(WrenchCalibration,1,dt*.85f);
            }
            if(power&&trustedSample&&Calibration>.95f)
            {
                wrenchSampleClock+=dt;
                if(wrenchSampleClock>=.16f){wrenchSampleClock=0;WrenchSequence++;WrenchSampleAge=0;}
            }
            WrenchSampleAge+=dt;
            EncoderBias=Mathf.MoveTowards(EncoderBias,disturbance==1&&!repair?5.9f:0,dt*3.9f);
            jawImbalance=Mathf.MoveTowards(jawImbalance,disturbance==3&&!repair?1:0,dt*.85f);
            JawFault=jawImbalance;
            Vector3 last=Tool;
            for(int i=0;i<8;i++)
            {
                float error=goal[i]-actual[i]-(i==2&&motion?EncoderBias:0);
                float limit=i==7?3.8f:64;
                if(Carried!=0)limit*=.62f;
                float acceleration=i==7?5.5f:162;
                float desire=motion?Mathf.Clamp(error*(i==7?5.2f:6.5f),-limit,limit):0;
                Speed[i]=Mathf.MoveTowards(Speed[i],desire,dt*acceleration);
                actual[i]+=Speed[i]*dt;
            }
            Tool=Forward(actual);
            toolSpeed=Vector3.Distance(last,Tool)/dt;
            settleTime=MoveError<.115f&&Stopped?settleTime+dt:0;
            bool hold=Carried!=0||power&&(grip||preload);
            Jaw=Mathf.MoveTowards(Jaw,hold?.10f:1,dt*1.25f);
            float seating=hold&&(Carried!=0||AtGoal||preload)?1:0;
            float load=seating*Mathf.Max(0,.71f-Jaw)*12.9f;
            LeftLoad=load*(1+.46f*jawImbalance);
            RightLoad=load*(1-.67f*jawImbalance);
            float exciting=disturbance==2&&!repair?1:0;
            flexVelocity+=(exciting*3.1f*Mathf.Sin(Clock*8.25f)+toolSpeed*.08f-DynamicDeflection*71-
                flexVelocity*(repair?7.2f:1.15f))*dt;
            DynamicDeflection+=flexVelocity*dt;
            FlexMotion=Mathf.Lerp(FlexMotion,Mathf.Abs(DynamicDeflection)+Mathf.Abs(flexVelocity)*.10f,dt*4);
            WrenchForce=new Vector3(seating*2.6f+.032f*toolSpeed,LeftLoad+RightLoad,DynamicDeflection*14);
            WrenchTorque=new Vector3(.53f*(LeftLoad-RightLoad),.20f*toolSpeed,flexVelocity*1.5f);
            if(Carried==1)FailedPose=Tool;
            if(Carried==2)ReplacementPose=Tool;
            if(bolts!=0&&AtGoal&&power)
                for(int i=0;i<4;i++)Bolts[i]=Mathf.MoveTowards(Bolts[i],bolts>0?1:0,dt*(.75f-i*.08f));
            Valve=Mathf.MoveTowards(Valve,pump?1:0,dt*.70f);
            Pressure=Mathf.MoveTowards(Pressure,pump?.88f:.038f,dt*(pump?.23f:.33f));
            float required=pump&&ReplacementInstalled&&Fastened?Valve: pump?.26f:0;
            Flow=Mathf.MoveTowards(Flow,required,dt*.48f);
            Leak=Mathf.MoveTowards(Leak,pump&&(!ReplacementInstalled||!Fastened)?.082f:.005f,dt*.029f);
            leakTime=pump&&Pressure>.72f&&Leak<.018f&&ReplacementInstalled?leakTime+dt:0;
            LoopTested|=leakTime>1.2f;
            HeatRejected=Mathf.Max(0,Temperature-27)*(ReplacementInstalled?.036f:.009f)*Flow;
            float heat=Temperature>94?.38f:.56f;
            Temperature+=dt*(heat-HeatRejected)/1.42f;
        }
    }
}

