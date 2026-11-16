using UnityEngine;

namespace ProjectSpark.Gameplay
{
    public sealed class SparkLevel84Plant
    {
        public const float Step=.02f;
        private static readonly Vector3 Inertia=new(1.35f,1.12f,.93f);
        private static readonly Vector3[] Axes={new Vector3(1,1,1).normalized,new Vector3(1,-1,-1).normalized,
            new Vector3(-1,1,-1).normalized,new Vector3(-1,-1,1).normalized};
        public static readonly Quaternion SlewTarget=Quaternion.Euler(9,21,-7);
        public Quaternion Body{get;private set;}=Quaternion.Euler(12,-16,21);
        public Quaternion Estimate{get;private set;}=Quaternion.Euler(14,-12,20);
        public Vector3 Rate{get;private set;}=new(.34f,-.27f,.22f);
        public Vector3 RateDegrees=>Rate*Mathf.Rad2Deg;
        public Vector3 Field,Dipole,GyroResidual;
        public float Confidence,Uncertainty=6,Calibration,Baffle,PointError,Jitter,Energy=1,OpticalError=2.1f;
        public float BiasFault,WheelFault,FlexFault;
        public float[] WheelRpm{get;}=new float[4];
        public float[] WheelUtil{get;}=new float[4];
        public float[] WheelTemp{get;}=new[]{23f,23f,23f,23f};
        public float Clock{get;private set;}
        public float MissionTime{get;private set;}
        public int PassCount{get;private set;}
        public bool Running,Completed,OpticalLocked,SampleFresh,QualifiedFine,Unloading;
        public float LoadActivity,DumpChange,DumpImpulse,RelativeMomentum;
        public float StarAge{get;private set;}=10;
        public int StarSequence{get;private set;}
        public float MaxWheel=>Mathf.Max(Mathf.Max(WheelUtil[0],WheelUtil[1]),Mathf.Max(WheelUtil[2],WheelUtil[3]));
        public bool SafeActuators=>WheelTemp[0]<61&&WheelTemp[1]<61&&WheelTemp[2]<61&&WheelTemp[3]<61&&Rate.magnitude<1.3f;
        public bool Catalogue=>Baffle>.94f&&Rate.magnitude<.19f&&Confidence>.76f;
        private readonly float[] spin={.44f,-.27f,.29f,.11f};
        private readonly Vector3 baseBias=new(.013f,-.010f,.007f);
        private Vector3 learnedBias;
        private float starTimer,coarseTimer,dumpStart,flexPos,flexVelocity;
        private float firstWork,secondWork;
        private bool lastDump;
        public float MissionProgress=>MissionTime/37;
        public void StartMission()
        {
            MissionTime=0;PassCount=0;Running=true;Completed=false;firstWork=secondWork=0;
        }
        public void EndMission(){Running=false;OpticalLocked=false;}
        private static Quaternion Spin(Vector3 radians)
        {
            float length=radians.magnitude;
            return length<1e-7f?Quaternion.identity:Quaternion.AngleAxis(length*Mathf.Rad2Deg,radians/length);
        }
        private static Vector3 Error(Quaternion desired, Quaternion estimate)
        {
            var q=Quaternion.Inverse(estimate)*desired;
            if(q.w<0)q=new Quaternion(-q.x,-q.y,-q.z,-q.w);
            q.ToAngleAxis(out float angle,out Vector3 axis);
            return angle<.0001f||axis.sqrMagnitude<.01f?Vector3.zero:axis*(angle*Mathf.Deg2Rad);
        }
        private static Vector3 ScaleInverse(Vector3 a,Vector3 b)=>new(a.x/b.x,a.y/b.y,a.z/b.z);
        private Vector3 WheelMomentum()
        {
            Vector3 wheel=Vector3.zero;
            for(int i=0;i<4;i++)wheel+=Axes[i]*spin[i];
            return wheel;
        }
        public void Tick(float dt,bool power,bool permit,bool cal,bool open,bool sampleChannel,bool coarse,bool fine,bool dump,bool align,
            int fault,bool repair)
        {
            Clock+=dt;if(Running)MissionTime+=dt;
            bool drives=power&&permit&&SafeActuators;
            BiasFault=Mathf.MoveTowards(BiasFault,fault==1&&!repair?1:0,dt*.64f);
            WheelFault=Mathf.MoveTowards(WheelFault,fault==2&&!repair?1:0,dt*.55f);
            FlexFault=Mathf.MoveTowards(FlexFault,fault==3&&!repair?1:0,dt*.62f);
            Vector3 bias=baseBias+BiasFault*new Vector3(.148f,-.070f,.084f);
            if(cal&&power&&fault!=1)
            {
                Calibration=Mathf.MoveTowards(Calibration,1,dt*.65f);
                learnedBias=Vector3.MoveTowards(learnedBias,bias,dt*.08f);
            }
            GyroResidual=bias-learnedBias;
            var measuredRate=Rate+GyroResidual;
            Estimate=(Estimate*Spin(measuredRate*dt)).normalized;
            Baffle=Mathf.MoveTowards(Baffle,open&&power?1:0,dt*1.35f);
            float sunAngle=Vector3.Angle(Body*Vector3.up,new Vector3(.45f,.35f,-.82f));
            bool visible=Baffle>.94f&&Rate.magnitude<.19f&&sunAngle>27;
            Confidence=Mathf.MoveTowards(Confidence,visible?.98f:0,dt*.78f);
            starTimer+=dt;StarAge+=dt;
            if(starTimer>=.11f)
            {
                starTimer=0;
                if(sampleChannel&&Catalogue&&visible&&power)
                {
                    StarAge=0;StarSequence++;
                    var observation=Body*Quaternion.Euler(.036f*Mathf.Sin(Clock*5),.027f*Mathf.Cos(Clock*4),.02f);
                    Estimate=Quaternion.Slerp(Estimate,observation,.78f);
                    Uncertainty=Mathf.MoveTowards(Uncertainty,.12f,.67f);
                }
            }
            SampleFresh=sampleChannel&&StarAge<.34f&&Catalogue;
            if(!SampleFresh)Uncertainty=Mathf.Min(22,Uncertainty+dt*(.48f+GyroResidual.magnitude*9));
            var fieldWorld=new Vector3(Mathf.Sin(Clock*.49f)*.68f,.46f,Mathf.Cos(Clock*.37f)*.63f);
            Field=Quaternion.Inverse(Body)*fieldWorld;
            var momentum=WheelMomentum();
            RelativeMomentum=momentum.magnitude;
            if(dump&&!lastDump){dumpStart=RelativeMomentum;DumpImpulse=0;DumpChange=0;}
            if(dump)DumpChange=Mathf.Max(DumpChange,dumpStart-RelativeMomentum);
            lastDump=dump;
            Vector3 external=(Body*Vector3.up)*.0001f;
            Dipole=Vector3.zero;
            if(drives&&coarse&&!fine)
            {
                // The B-dot capture projects rate damping onto the observable field plane.
                Dipole=Vector3.ClampMagnitude(Vector3.Cross(Field,-measuredRate*.75f)/(Field.sqrMagnitude+.05f),.63f);
                coarseTimer+=dt;
            }
            if(drives&&dump&&fine)
                Dipole=Vector3.ClampMagnitude(Vector3.Cross(Field,-momentum*.88f)/(Field.sqrMagnitude+.08f),.75f);
            var magnetic=Vector3.Cross(Dipole,Field);
            Unloading=drives&&dump&&fine&&magnetic.magnitude>.006f;
            if(Unloading)DumpImpulse+=magnetic.magnitude*dt;
            external+=magnetic;
            Quaternion target=SlewTarget;
            if(Running||Completed)
            {
                float change=Mathf.SmoothStep(0,1,Mathf.InverseLerp(16,22,MissionTime));
                target=Quaternion.Euler(Mathf.Lerp(9,-6,change),Mathf.Lerp(21,6,change)+1.15f*Mathf.Sin(MissionTime*.53f),
                    Mathf.Lerp(-7,9,change));
            }
            Vector3 demand=Vector3.zero;
            if(drives)
            {
                if(fine)demand=Error(target,Estimate)*1.68f-measuredRate*2.35f-magnetic;
                else if(coarse)
                {
                    demand=-measuredRate*(coarseTimer<2?.30f:1.4f);
                    if(!sampleChannel&&Rate.magnitude<.15f)
                    {
                        // Coarse Sun alignment uses the body Sun vector and gyro rates only.
                        Vector3 sunBody=Quaternion.Inverse(Body)*new Vector3(.1f,1,.12f).normalized;
                        demand+=Vector3.Cross(Vector3.up,sunBody)*.14f;
                    }
                }
            }
            Vector3 reaction=Vector3.zero;
            for(int i=0;i<4;i++)
            {
                float torque=drives?Mathf.Clamp(-.75f*Vector3.Dot(Axes[i],demand),-.56f,.56f):0;
                if(drives&&i==0&&WheelFault>0)
                {
                    // Biased drive injects momentum into the wheel and the opposite torque into the body.
                    torque=Mathf.Lerp(torque,spin[i]<.91f?.62f:0,WheelFault);
                }
                if(drives&&dump&&fine)
                {
                    float mean=(spin[0]+spin[1]+spin[2]+spin[3])*.25f;
                    torque-=.46f*mean;
                }
                float before=spin[i];
                spin[i]=Mathf.Clamp(before+torque*dt,-1,1);
                float actual=(spin[i]-before)/dt;
                reaction-=Axes[i]*actual;
                WheelRpm[i]=spin[i]*5800;WheelUtil[i]=Mathf.Abs(spin[i]);
                WheelTemp[i]+=dt*(Mathf.Abs(actual)*.62f-(WheelTemp[i]-23)*.064f);
            }
            float flexForce=FlexFault*.16f*Mathf.Sin(Clock*12.6f)+reaction.magnitude*.008f;
            flexVelocity+=dt*(flexForce-flexPos*160-flexVelocity*(1.4f+(repair?7:0)));
            flexPos+=dt*flexVelocity;
            external+=new Vector3(flexPos*.58f,0,-flexPos*.43f);
            var bodyMomentum=Vector3.Scale(Inertia,Rate)+WheelMomentum();
            Rate+=ScaleInverse(external+reaction-Vector3.Cross(Rate,bodyMomentum),Inertia)*dt;
            Body=(Body*Spin(Rate*dt)).normalized;
            if(align&&fine&&drives)OpticalError=Mathf.MoveTowards(OpticalError,.05f,dt*.75f);
            PointError=Quaternion.Angle(target,Body);
            float expectedJitter=.055f+Rate.magnitude*.85f+Mathf.Abs(flexVelocity)*32+FlexFault*.44f;
            Jitter=Mathf.Lerp(Jitter,expectedJitter,dt*7);
            QualifiedFine=fine&&SampleFresh&&Uncertainty<.7f&&PointError<2.3f&&Jitter<.33f;
            bool inFirst=MissionTime>=6&&MissionTime<16,inSecond=MissionTime>=25&&MissionTime<35;
            OpticalLocked=Running&&QualifiedFine&&OpticalError<.26f&&Energy>.25f&&(inFirst||inSecond);
            LoadActivity=Mathf.MoveTowards(LoadActivity,OpticalLocked?1:0,dt*3.1f);
            Energy=Mathf.Clamp01(Energy+dt*(.013f-(OpticalLocked?.031f:drives?.006f:.001f)));
            if(!Running)return;
            if(OpticalLocked&&inFirst)firstWork+=dt;
            if(OpticalLocked&&inSecond)secondWork+=dt;
            PassCount=(firstWork>=7.2f?1:0)+(secondWork>=7.2f?1:0);
            if(MissionTime>=37)
            {
                Completed=PassCount==2;
                EndMission();
            }
        }
    }
}
