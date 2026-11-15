using UnityEngine;

namespace ProjectSpark.Gameplay
{
    /// <summary>Lumped HVAC model. Flow m³/s, pressures gauge Pa, temperature °C, humidity kg water/kg dry air.</summary>
    public sealed class SparkLevel70AirPlant
    {
        public const float Step=.04f;
        public const float OutdoorParticles=75000f, OutdoorTemperature=28f;
        private const float AirDensity=1.19f;
        public readonly Zone Room=new(34f,26f,.0123f,14800f);
        public readonly Zone Lock=new(11f,26.8f,.0129f,23100f);
        public readonly Filter[] Filters={new(),new()};
        public readonly Fan[] Fans={new(2950),new(2580),new(2440),new(2050),new(2050)};
        public readonly float[] Dampers=new float[4];
        public float InnerDoor {get;private set;}
        public float OuterDoor {get;private set;}
        public float DoorFlow {get;private set;}
        public float LockLeak {get;private set;}
        public float PressureRoom {get;private set;}
        public float PressureLock {get;private set;}
        public float SupplyRoom {get;private set;}
        public float SupplyLock {get;private set;}
        public float Return {get;private set;}
        public float Exhaust {get;private set;}
        public float Velocity=>SupplyRoom/2.15f;
        public float Condensate {get;private set;}
        public float Drained {get;private set;}
        public float CoilTemperature {get;private set;}=15f;
        public float ReheatKw {get;private set;}
        public float HumidifierKgSec {get;private set;}
        public float SupplyTemperature {get;private set;}=25f;
        public float SupplyHumidity {get;private set;}=.0115f;
        public float LastMassResidual {get;private set;}
        public float LeakPressure=>PressureRoom-PressureLock;
        public bool HadDoorConflict {get;private set;}

        public sealed class Zone
        {
            internal readonly float Volume;
            public float Temperature {get;internal set;}
            public float Humidity {get;internal set;}
            public float Particles {get;internal set;}
            public float RelativeHumidity=>SparkLevel70AirPlant.RelativeHumidity(Temperature,Humidity);
            public float DewPoint=>SparkLevel70AirPlant.DewPointOf(Humidity);
            public Zone(float volume,float temp,float humidity,float particles)
            {Volume=volume; Temperature=temp; Humidity=humidity; Particles=particles;}
        }
        public sealed class Filter
        {
            public float Loading {get;internal set;}
            public float Drop {get;internal set;}
            public float Flow {get;internal set;}
            public float Isolation {get;internal set;}
            public bool SealFailed {get;internal set;}
            public float Penetration=>SealFailed?.66f:.0027f;
            internal void Replace(){Loading=0;SealFailed=false;}
        }
        public sealed class Fan
        {
            public readonly float Limit;
            public float Speed {get;private set;}
            public float Rpm=>Speed*Limit;
            public float Power=>.36f*Speed*Speed*Speed;
            public Fan(float limit){Limit=limit;}
            public void Step(float command,float dt)
            {
                float torque=Mathf.Clamp((command-Speed)*2.8f+Speed*Speed*.18f,-.95f,1.35f);
                Speed=Mathf.Clamp01(Speed+(torque-Speed*Speed*.18f)*dt/.82f);
            }
        }
        public sealed class Controller
        {
            private float integral,previous,rate;
            private bool started;
            public void Reset(){integral=previous=rate=0;started=false;}
            public float Step(float error,float kp,float ki,float kd,float bias,float min,float max,float dt)
            {
                if(started) rate=Mathf.Lerp(rate,(error-previous)/dt,dt/(dt+.20f));
                started=true;previous=error;
                float raw=bias+kp*error+ki*integral+kd*rate;
                if((raw<max||error<0)&&(raw>min||error>0)) integral=Mathf.Clamp(integral+error*dt,-20,20);
                return Mathf.Clamp(bias+kp*error+ki*integral+kd*rate,min,max);
            }
        }

        public static float SaturationPa(float t)=>610.94f*Mathf.Exp(17.625f*t/(243.04f+t));
        public static float HumidityRatio(float temperature,float fraction)
        {
            float vapor=SaturationPa(temperature)*Mathf.Clamp01(fraction);
            return .62198f*vapor/(101325f-vapor);
        }
        public static float RelativeHumidity(float temperature,float humidity)
        {
            float vapor=101325f*humidity/(.62198f+humidity);
            return Mathf.Clamp01(vapor/SaturationPa(temperature));
        }
        public static float DewPointOf(float humidity)
        {
            float vapor=101325f*humidity/(.62198f+humidity);
            float l=Mathf.Log(Mathf.Max(3,vapor)/610.94f);
            return 243.04f*l/(17.625f-l);
        }
        private static float SignedRoot(float delta)=>Mathf.Sign(delta)*Mathf.Sqrt(Mathf.Abs(delta));

        // Door commands are requested by the PLC. Geometry follows the actual normalized movement.
        public void Tick(float dt,Vector3 speeds,bool ffuA,bool ffuB,float fresh,float roomDamper,float lockDamper,float relief,
            bool outer,bool inner,bool shower,bool loading,bool isolation,bool autoThermal,float tempTarget,float rhTarget)
        {
            if(outer&&inner){HadDoorConflict=true;outer=inner=false;}
            InnerDoor=Mathf.MoveTowards(InnerDoor,inner&&OuterDoor<.028f?1:0,dt*1.8f);
            OuterDoor=Mathf.MoveTowards(OuterDoor,outer&&InnerDoor<.028f?1:0,dt*1.8f);
            for(int i=0;i<3;i++) Fans[i].Step(speeds[i],dt);
            Fans[3].Step(ffuA&&!isolation?.86f:0,dt);
            Fans[4].Step(ffuB?.86f:0,dt);
            Dampers[0]=Mathf.MoveTowards(Dampers[0],fresh,dt*.66f);
            Dampers[1]=Mathf.MoveTowards(Dampers[1],roomDamper,dt*.66f);
            Dampers[2]=Mathf.MoveTowards(Dampers[2],lockDamper,dt*.70f);
            Dampers[3]=Mathf.MoveTowards(Dampers[3],relief,dt*.86f);
            Filters[0].Isolation=Mathf.MoveTowards(Filters[0].Isolation,isolation?1:0,dt*.85f);
            Filters[0].Loading=Mathf.Clamp01(Filters[0].Loading+(loading?.075f:.000014f)*Filters[0].Flow*dt);
            Filters[1].Loading=Mathf.Clamp01(Filters[1].Loading+.000014f*Filters[1].Flow*dt);

            float drive=Fans[0].Speed;
            float manifold=410*drive*drive;
            float aEff=(.13f+.87f*Dampers[1]);
            float a0=Mathf.Sqrt(Mathf.Max(0,manifold+42*Fans[3].Speed*Fans[3].Speed-PressureRoom)/(870*(1+.74f*Filters[0].Loading)));
            float a1=Mathf.Sqrt(Mathf.Max(0,manifold+42*Fans[4].Speed*Fans[4].Speed-PressureRoom)/(870*(1+.74f*Filters[1].Loading)));
            Filters[0].Flow=drive>.035f?a0*aEff*(1-Filters[0].Isolation):0;
            Filters[1].Flow=drive>.035f?a1*aEff:0;
            SupplyRoom=Filters[0].Flow+Filters[1].Flow;
            Filters[0].Drop=365*Filters[0].Flow*Filters[0].Flow*(1+2.55f*Filters[0].Loading);
            Filters[1].Drop=365*Filters[1].Flow*Filters[1].Flow*(1+2.55f*Filters[1].Loading);
            SupplyLock=drive>.035f?Mathf.Sqrt(Mathf.Max(0,manifold-PressureLock)/1050)*( .06f+.94f*Dampers[2]):0;
            Return=.94f*Fans[1].Speed;
            Exhaust=.37f*Fans[2].Speed;
            float roomLeak=(.039f+.20f*Dampers[3])*SignedRoot(PressureRoom);
            DoorFlow=(.043f+.46f*InnerDoor)*SignedRoot(PressureRoom-PressureLock);
            LockLeak=(.055f+.51f*OuterDoor)*SignedRoot(PressureLock);
            float imbalanceRoom=SupplyRoom-Return-roomLeak-DoorFlow;
            float imbalanceLock=SupplyLock+DoorFlow-Exhaust-LockLeak;
            // Effective pneumatic compliance includes the plenum and lumped duct storage.
            float previousRoom=PressureRoom,previousLock=PressureLock;
            PressureRoom+=imbalanceRoom*dt/.060f;
            PressureLock+=imbalanceLock*dt/.041f;
            LastMassResidual=Mathf.Abs((PressureRoom-previousRoom)*.060f+(PressureLock-previousLock)*.041f-
                (SupplyRoom+SupplyLock-Return-Exhaust-roomLeak-LockLeak)*dt);

            float mixedTemp=Mathf.Lerp(Room.Temperature,OutdoorTemperature,Dampers[0]);
            float outsideHumidity=HumidityRatio(OutdoorTemperature,.56f);
            float mixedHumidity=Mathf.Lerp(Room.Humidity,outsideHumidity,Dampers[0]);
            float targetHumidity=HumidityRatio(tempTarget,rhTarget);
            float adjustedHumidity=Mathf.Clamp(targetHumidity+(targetHumidity-Room.Humidity)*.85f,.0045f,.0115f);
            float surfaceRequest=autoThermal?Mathf.Clamp(DewPointOf(adjustedHumidity)-2.0f,5.8f,14.5f):16;
            CoilTemperature=Mathf.MoveTowards(CoilTemperature,surfaceRequest,dt*1.25f);
            float heatCapacity=AirDensity*1.006f*(SupplyRoom+SupplyLock);
            float conditionedT=mixedTemp+.71f*(Mathf.Min(CoilTemperature,mixedTemp)-mixedTemp);
            float conditionedW=mixedHumidity+.88f*(Mathf.Min(HumidityRatio(CoilTemperature,1),mixedHumidity)-mixedHumidity);
            float reqSupplyTemp=tempTarget+(tempTarget-Room.Temperature)*1.3f-.68f/Mathf.Max(.52f,heatCapacity);
            float heat=autoThermal?Mathf.Clamp((reqSupplyTemp-conditionedT)*heatCapacity,0,17):0;
            ReheatKw=Mathf.MoveTowards(ReheatKw,heat,dt*11f);
            float humid=autoThermal?Mathf.Clamp((adjustedHumidity-conditionedW)*heatCapacity/1.006f,0,.0045f):0;
            HumidifierKgSec=Mathf.MoveTowards(HumidifierKgSec,humid,dt*.004f);
            float mixingFlow=SupplyRoom+SupplyLock;
            if(mixingFlow>.03f)
            {
                SupplyTemperature=conditionedT+ReheatKw/heatCapacity;
                SupplyHumidity=Mathf.Min(HumidityRatio(SupplyTemperature,.95f),
                    conditionedW+HumidifierKgSec/(mixingFlow*AirDensity));
                float condensate=Mathf.Max(0,mixedHumidity-conditionedW)*mixingFlow*AirDensity*dt;
                Condensate+=condensate;
            }
            float discharge=Mathf.Min(Condensate,dt*.0022f);
            Condensate-=discharge;Drained+=discharge;

            float fanFeed=Room.Particles*(1-Dampers[0])+OutdoorParticles*Dampers[0];
            float particleSupplyRoom=fanFeed*(Filters[0].Flow*Filters[0].Penetration+Filters[1].Flow*Filters[1].Penetration);
            float particleSupplyLock=fanFeed*SupplyLock*.0030f;
            float doorMix=.16f*OuterDoor, innerMix=.08f*InnerDoor;
            float roomIn=Mathf.Max(0,-DoorFlow)+innerMix, lockIn=Mathf.Max(0,DoorFlow)+innerMix;
            float infiltrationRoom=Mathf.Max(0,-roomLeak), infiltrationLock=Mathf.Max(0,-LockLeak)+doorMix;
            float roomParticlesChange=particleSupplyRoom-SupplyRoom*Room.Particles+
                roomIn*(Lock.Particles-Room.Particles)+infiltrationRoom*(OutdoorParticles-Room.Particles)+
                125f-.012f*Room.Volume*Room.Particles;
            float lockParticlesChange=particleSupplyLock-SupplyLock*Lock.Particles+
                lockIn*(Room.Particles-Lock.Particles)+infiltrationLock*(OutdoorParticles-Lock.Particles)+
                100f-(shower?.52f:0)*Lock.Particles-.015f*Lock.Volume*Lock.Particles;
            float roomHeat=SupplyRoom*(SupplyTemperature-Room.Temperature)+roomIn*(Lock.Temperature-Room.Temperature)+
                infiltrationRoom*(OutdoorTemperature-Room.Temperature)+.57f;
            float lockHeat=SupplyLock*(SupplyTemperature-Lock.Temperature)+lockIn*(Room.Temperature-Lock.Temperature)+
                infiltrationLock*(OutdoorTemperature-Lock.Temperature)+.24f;
            float roomWater=SupplyRoom*(SupplyHumidity-Room.Humidity)+roomIn*(Lock.Humidity-Room.Humidity)+
                infiltrationRoom*(outsideHumidity-Room.Humidity)+.00030f;
            float lockWater=SupplyLock*(SupplyHumidity-Lock.Humidity)+lockIn*(Room.Humidity-Lock.Humidity)+
                infiltrationLock*(outsideHumidity-Lock.Humidity)+.00012f;
            Room.Particles=Mathf.Max(0,Room.Particles+roomParticlesChange*dt/Room.Volume);
            Lock.Particles=Mathf.Max(0,Lock.Particles+lockParticlesChange*dt/Lock.Volume);
            Room.Temperature+=roomHeat*dt/(Room.Volume*.66f);
            Lock.Temperature+=lockHeat*dt/(Lock.Volume*.82f);
            Room.Humidity=Mathf.Max(0,Room.Humidity+roomWater*dt/Room.Volume);
            Lock.Humidity=Mathf.Max(0,Lock.Humidity+lockWater*dt/Lock.Volume);
        }
    }
}
