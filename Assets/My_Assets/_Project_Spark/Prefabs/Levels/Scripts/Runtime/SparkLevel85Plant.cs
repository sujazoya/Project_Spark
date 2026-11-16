using UnityEngine;

namespace ProjectSpark.Gameplay
{
    public sealed class SparkLevel85Plant
    {
        public const float Step=.04f, ContactPlane=.16f, Staging=112;
        public Vector3 Position{get;private set;}=new(Staging,.62f,-.48f);
        public Vector3 Velocity{get;private set;}
        public Vector3 EstimatedPosition{get;private set;}=new(Staging,.62f,-.48f);
        public Vector3 EstimatedVelocity{get;private set;}
        public Quaternion Body{get;private set;}=Quaternion.Euler(3.4f,-4.3f,2.3f);
        public Vector3 AngularRate{get;private set;}
        public float Clock,Calibration,LidarA,LidarB,Radar,Confidence,NavAge=9,RadarClosing,LidarClosing,ScanA,ScanB;
        public float Fuel=1,PeakPulse,CalibrationImpulse,ValveSeverity,LidarSeverity,LatchSeverity;
        public float CaptureTravel,Seal,Coupler,Pressure,InterfaceVoltage,TouchSpeed,TouchAttitude;
        public readonly float[] Valve=new float[6],Latch=new float[6];
        public bool Contact,Collision,Qualified;
        public bool LiDARHealthy=>LidarA>.78f&&LidarB>.78f;
        public bool OwnNavFresh=>NavAge<.50f&&Confidence>.70f&&Calibration>.99f;
        public float Range=>Position.x;
        public float Closing=>-Velocity.x;
        public float Lateral=>new Vector2(Position.y,Position.z).magnitude;
        public float AttitudeError=>Quaternion.Angle(Body,Quaternion.identity);
        public int Locks
        {
            get{int count=0;foreach(float travel in Latch)if(travel>.988f)count++;return count;}
        }
        public float LatchSpread=>Mathf.Max(Mathf.Max(Latch[0],Latch[1]),Mathf.Max(Latch[2],Mathf.Max(Latch[4],Latch[5])))-Latch[3];
        public float PressureLoss=>1-Pressure;
        private float sampleClock,previousLidar=Staging,previousRadar=Staging;
        private bool started;
        public void ClearContact()
        {
            if(!Contact)return;
            Contact=false;Qualified=false;Collision=false;
        }

        public void Tick(float dt,bool power,bool propulsion,bool calibrate,bool pulse,bool moving,float target,bool hold,bool capture,
            bool latch,bool seal,bool release,int fault,bool service)
        {
            Clock+=dt;
            bool energized=power&&propulsion&&Fuel>.04f;
            if(calibrate&&power)Calibration=Mathf.MoveTowards(Calibration,1,dt*.67f);
            LidarSeverity=Mathf.MoveTowards(LidarSeverity,fault==1&&!service?1:0,dt*.92f);
            ValveSeverity=Mathf.MoveTowards(ValveSeverity,fault==2&&!service?1:0,dt*.78f);
            LatchSeverity=Mathf.MoveTowards(LatchSeverity,fault==3&&!service?1:0,dt*.86f);
            ScanA=Mathf.Sin(Clock*3.8f)*31;
            ScanB=Mathf.Sin(Clock*3.8f+1.1f)*24;
            NavAge+=dt;sampleClock+=dt;
            Estimate(dt,power);
            if(!moving&&!Contact&&!started&&!pulse)return;
            started|=moving;
            Vector3 requested=Vector3.zero;
            if(energized&&!Contact)
            {
                if(pulse)requested=new Vector3(0,.23f,0);
                else if(hold)requested=Radar>.75f?new Vector3(RadarClosing*2.6f,-Velocity.y*2.6f-Position.y*.42f,
                    -Velocity.z*2.6f-Position.z*.42f):Vector3.zero;
                else
                {
                    float distance=target-EstimatedPosition.x;
                    float speedLimit=Range>104?7:Range>35?8:Range>8?5:Range>3?2:Range>1.2f?.7f:Range>.45f?.24f:.095f;
                    if(distance>0)speedLimit=Range<2?.36f:8.5f;
                    float speed=Mathf.Min(speedLimit,Mathf.Sqrt(Mathf.Abs(distance)*.86f));
                    float sign=distance>=0?1:-1;
                    float xGoal=Mathf.Abs(distance)<.11f&&target>.5f?0:speed*sign;
                    requested=new Vector3((xGoal-EstimatedVelocity.x)*2.15f,-EstimatedPosition.y*1.10f-EstimatedVelocity.y*2.55f,
                        -EstimatedPosition.z*1.15f-EstimatedVelocity.z*2.55f);
                }
            }
            float thrustFactor=Mathf.Clamp(.78f+Fuel*.22f,0,1);
            Vector3 acceleration=Vector3.zero;
            for(int i=0;i<6;i++)
            {
                int axis=i/2;
                float raw=requested[axis]*(i%2==0?-1:1);
                float targetValve=energized&&!Contact?Mathf.Clamp01(raw/1.75f):0;
                if(i==2&&energized&&!Contact)targetValve=Mathf.Max(targetValve,ValveSeverity*.36f);
                Valve[i]=Mathf.MoveTowards(Valve[i],targetValve,dt*(targetValve>Valve[i]?3.4f:5f));
                float output=energized?Valve[i]*1.75f*thrustFactor:0;
                acceleration[axis]+=output*(i%2==0?-1:1);
                Fuel=Mathf.Max(0,Fuel-output*dt*.00031f);
                PeakPulse=Mathf.Max(PeakPulse,Valve[i]);
            }
            if(pulse)CalibrationImpulse+=Mathf.Abs(acceleration.y)*dt;
            if(Contact)
            {
                CaptureTravel=Mathf.MoveTowards(CaptureTravel,release?0:1,dt*.86f);
                float desired=ContactPlane-.05f*CaptureTravel;
                var reaction=new Vector3((desired-Position.x)*32,-Position.y*22,-Position.z*22)-Velocity*10;
                Velocity+=reaction*dt;
                Position+=Velocity*dt;
            }
            else if(moving||started)
            {
                // A bounded relative-orbit disturbance and rotating-frame coupling persist during HOLD.
                Vector3 drift=new(.000036f*(Position.x-ContactPlane)+.0011f*Velocity.z,.0007f*Mathf.Sin(Clock*.26f),
                    -.0011f*Velocity.x+.0005f*Mathf.Cos(Clock*.24f));
                Velocity+=(acceleration+drift)*dt;
                Position+=Velocity*dt;
            }
            if(!Contact&&Position.x<=ContactPlane)
            {
                TouchSpeed=Mathf.Abs(Closing);TouchAttitude=AttitudeError;
                if(capture&&TouchSpeed<.20f&&Lateral<.22f&&TouchAttitude<2.1f&&!hold)
                {Contact=true;Collision=false;}
                else
                {
                    Collision=true;
                    Position=new Vector3(ContactPlane,Position.y,Position.z);
                    Velocity=new Vector3(Mathf.Max(.062f,TouchSpeed*.36f),Velocity.y,Velocity.z);
                }
            }
            if(started&&energized)
            {
                Quaternion error=Quaternion.Inverse(Body);
                if(error.w<0)error=new Quaternion(-error.x,-error.y,-error.z,-error.w);
                Vector3 torque=new Vector3(error.x,error.y,error.z)*4.2f-AngularRate*2.65f;
                torque+=new Vector3(0,.019f,0)*ValveSeverity;
                AngularRate+=Vector3.ClampMagnitude(torque,.42f)*dt;
                float angle=AngularRate.magnitude*dt;
                if(angle>1e-6f)Body=(Body*Quaternion.AngleAxis(angle*Mathf.Rad2Deg,AngularRate.normalized)).normalized;
            }
            for(int i=0;i<6;i++)
            {
                float goal=!release&&latch&&Contact&&CaptureTravel>.94f?1:0;
                if(i==3)goal=Mathf.Min(goal,1-.86f*LatchSeverity);
                Latch[i]=Mathf.MoveTowards(Latch[i],goal,dt*(.56f-.035f*i));
            }
            Seal=Mathf.MoveTowards(Seal,seal&&!release&&Locks==6?1:0,dt*.70f);
            Coupler=Mathf.MoveTowards(Coupler,seal&&!release&&Locks==6?1:0,dt*.62f);
            Pressure=Mathf.Clamp01(Pressure+dt*(Seal>.985f?.86f*(1-Pressure):-.83f*Pressure));
            InterfaceVoltage=Coupler>.99f&&power?5:0;
            Qualified=Contact&&Locks==6&&Seal>.99f&&Coupler>.99f&&Pressure>.975f&&InterfaceVoltage>4.6f;
            if(release&&Contact&&Locks==0&&Seal<.015f&&Coupler<.015f&&CaptureTravel<.04f)
            {
                ClearContact();
                Position=new Vector3(Mathf.Max(ContactPlane+.005f,Position.x),Position.y,Position.z);
            }
        }

        private void Estimate(float dt,bool power)
        {
            if(started)EstimatedPosition+=EstimatedVelocity*dt;
            if(!power||sampleClock<.16f)return;
            float interval=sampleClock;sampleClock=0;
            float signal=Calibration*(1-LidarSeverity);
            LidarA=Mathf.MoveTowards(LidarA,signal,interval*2.6f);
            LidarB=Mathf.MoveTowards(LidarB,Calibration*.99f,interval*2.6f);
            Radar=Mathf.MoveTowards(Radar,Calibration*.98f,interval*2.6f);
            float noise=.0012f*Mathf.Sin(Clock*5.1f);
            float radar=Range+.0029f*Mathf.Cos(Clock*3.6f);
            float laser=Range+noise;
            RadarClosing=(previousRadar-radar)/interval;
            LidarClosing=(previousLidar-laser)/interval;
            previousRadar=radar;previousLidar=laser;
            Confidence=Mathf.MoveTowards(Confidence,Calibration*(.41f*LidarB+.37f*Radar+.22f*LidarA),interval*2);
            if(Calibration<.95f||Radar<.75f||LidarB<.75f)return;
            var sensed=new Vector3(laser*.62f+radar*.38f,Position.y+noise,Position.z-noise*.77f);
            var rate=new Vector3(-(.58f*LidarClosing+.42f*RadarClosing),Velocity.y+noise*.2f,Velocity.z-noise*.2f);
            EstimatedPosition=Vector3.Lerp(EstimatedPosition,sensed,.81f);
            EstimatedVelocity=Vector3.Lerp(EstimatedVelocity,rate,.73f);
            NavAge=0;
        }
    }
}
