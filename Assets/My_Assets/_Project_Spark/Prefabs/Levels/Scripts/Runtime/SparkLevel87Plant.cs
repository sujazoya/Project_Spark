using UnityEngine;

namespace ProjectSpark.Gameplay
{
    public sealed class SparkLevel87Plant
    {
        public const float Step=.025f;
        public const float Radius=45f;
        private const float Inertia=180000f, FlywheelCapacity=64000f, MaxSourcePower=27000f;
        private const float RpmFactor=60f/(2*Mathf.PI);
        public sealed class Bearing
        {
            public float Gap=5, Current, Requested, Force, Temperature=32;
        }

        public readonly Bearing[] Bearings=new Bearing[8];
        private readonly Vector2[] normals=new Vector2[8];
        private readonly float[] vibrationHistory=new float[128];
        private int historyHead,historyCount;
        private float acquisition,sensorTimer,angularSpeed,phase,offsetAxial,velocityAxial;
        private bool cargoShiftInstalled;
        private Vector2 center,centerVelocity,mode,modeVelocity;
        public float Clock{get;private set;}
        public float Rpm=>angularSpeed*RpmFactor;
        public float Angle=>phase*Mathf.Rad2Deg;
        public float Gravity=>angularSpeed*angularSpeed*Radius/9.80665f;
        public float Energy=>.5f*Inertia*angularSpeed*angularSpeed;
        public float Momentum=>Inertia*angularSpeed;
        public float CounterMomentum{get;private set;}
        public float CounterAngle{get;private set;}
        public float StationMomentum=>Momentum+CounterMomentum;
        public float Bus,BearingBus,Touchdown=1,LevitationTime,Primary,Secondary,Axial,SampleAge=2;
        public int SampleSequence{get;private set;}
        public int CalibrationRevision{get;private set;}
        public float Calibration{get;private set;}
        public float Drift{get;private set;}
        public float AmplifierTemperature{get;private set;}=35;
        public float CurrentLimit=>Mathf.Lerp(6f,2.25f,Mathf.InverseLerp(58,83,AmplifierTemperature));
        public float InverterA,InverterB,InputPower,RotaryPower,OpticalQuality,HabitatTemperature=24,LoadDuty,LoadPower;
        public float WeightA=.18f,WeightB=.72f,Vibration,PeakFrequency,RestoredThermalTime,MissionEnergy,TouchdownHeat=30;
        public float MassShift{get;private set;}
        public float MinimumGap{get;private set;}=5;
        public float MaximumGap{get;private set;}=5;
        public float CoilsMean{get;private set;}
        public bool LevitationQualified=>LevitationTime>.8f;
        public bool Supported=>Touchdown>.98f&&Rpm<.055f;
        public bool RingAtTarget=>Mathf.Abs(Rpm-3)<.085f&&Gravity is >.42f and <.49f&&Vibration<.30f;
        public bool BearingHealthy=>MinimumGap>3.40f&&MaximumGap<6.60f&&AmplifierTemperature<65;
        public bool MissionHealthy=>RingAtTarget&&LevitationQualified&&BearingHealthy&&LoadDuty>.97f&&
            OpticalQuality>.85f&&HabitatTemperature is >18 and <30&&InputPower<MaxSourcePower&&Mathf.Abs(StationMomentum)<9000;

        public SparkLevel87Plant()
        {
            for(int i=0;i<Bearings.Length;i++)
            {
                float a=i*Mathf.PI*.25f;
                normals[i]=new Vector2(Mathf.Cos(a),Mathf.Sin(a));
                Bearings[i]=new Bearing();
            }
        }

        public void StartCalibration()
        {
            Calibration=0;
        }

        public void BeginMission()
        {
            MissionEnergy=0;
            RestoredThermalTime=0;
        }

        public void Tick(bool power,bool calibrate,bool lift,bool motor,bool rotary,bool loads,bool protection,float target,
            bool balance,int fault,bool repair)
        {
            const float dt=Step;
            Clock+=dt;
            Drift=Mathf.MoveTowards(Drift,fault==1&&!repair?1.34f:0,dt*.73f);
            if(fault==3)cargoShiftInstalled=true;
            MassShift=Mathf.MoveTowards(MassShift,cargoShiftInstalled?1.10f:0,dt*.66f);
            if(calibrate&&power&&Supported&&Mathf.Abs(Drift)<.10f&&Calibration<1)
            {
                Calibration=Mathf.MoveTowards(Calibration,1,dt*1.2f);
                if(Calibration>=1)CalibrationRevision++;
            }
            float referenceA=.46f+MassShift*.36f, referenceB=.47f-MassShift*.31f;
            if(balance&&Rpm<1.65f)
            {
                WeightA=Mathf.MoveTowards(WeightA,referenceA,dt*.37f);
                WeightB=Mathf.MoveTowards(WeightB,referenceB,dt*.37f);
            }

            BearingBus=Mathf.MoveTowards(BearingBus,power&&lift?1:0,dt*(lift?.8f:1.5f));
            bool fieldOn=BearingBus>.80f&&lift;
            Touchdown=Mathf.MoveTowards(Touchdown,fieldOn?0:1,dt*(fieldOn?.82f:1.75f));
            var imbalance=new Vector2(Mathf.Cos(phase),Mathf.Sin(phase))*
                (.22f+Mathf.Abs(referenceA-WeightA)+Mathf.Abs(referenceB-WeightB))*Rpm*Rpm;
            modeVelocity+=(imbalance*2.4f-mode*21f-modeVelocity*(balance?4.7f:2.8f))*dt;
            mode+=modeVelocity*dt;
            Vector2 force=Vector2.zero;
            float coilWatts=0;
            MinimumGap=8;MaximumGap=0;CoilsMean=0;
            for(int i=0;i<Bearings.Length;i++)
            {
                var station=Bearings[i];var normal=normals[i];
                station.Gap=5-Vector2.Dot(center+mode*.36f,normal)-Mathf.Abs(offsetAxial)*.13f;
                float measured=5-station.Gap+(i==0?Drift*.67f:0);
                float support=-2.9f*measured-1.8f*Vector2.Dot(centerVelocity,normal)-normal.y*.58f;
                station.Requested=fieldOn?Mathf.Clamp(2.8f+support,0,6):0;
                station.Current=Mathf.MoveTowards(station.Current,Mathf.Min(station.Requested,CurrentLimit),dt*13f);
                station.Force=station.Current*5/Mathf.Max(station.Gap,3.1f);
                force+=normal*station.Force;
                station.Temperature+=dt*(.12f*station.Current*station.Current+.12f*(AmplifierTemperature-station.Temperature)-
                    .10f*(station.Temperature-32));
                coilWatts+=22*station.Current*station.Current;
                CoilsMean+=station.Current/8;
                MinimumGap=Mathf.Min(MinimumGap,station.Gap);MaximumGap=Mathf.Max(MaximumGap,station.Gap);
            }
            Vector2 supportForce=Touchdown*(-center*31-centerVelocity*6);
            centerVelocity+=(force*.47f+imbalance*.32f+supportForce+new Vector2(0,-1.0f))*dt;
            center+=centerVelocity*dt;
            center=Vector2.ClampMagnitude(center,Touchdown>.2f?1.39f:2.26f);
            velocityAxial+=(Mathf.Cos(phase*2)*Rpm*.085f-offsetAxial*(fieldOn?11:18)-velocityAxial*3.5f)*dt;
            offsetAxial+=velocityAxial*dt;
            float heat=fault==2&&!repair?19:0;
            AmplifierTemperature+=dt*(coilWatts*.00115f+heat-(AmplifierTemperature-32)*(repair?.86f:.30f));
            LevitationTime=fieldOn&&Touchdown<.025f&&BearingHealthy&&CoilsMean>1&&Mathf.Abs(offsetAxial)<.4f?LevitationTime+dt:0;

            Bus=Mathf.MoveTowards(Bus,power&&motor?1:0,dt*(motor?.75f:1.1f));
            float demand=loads?4400+380*Mathf.Sin(phase*2):220;
            float rotaryBudget=power&&rotary?Mathf.Min(demand/.84f,6500f):0;
            float auxiliaries=power?1250+coilWatts+rotaryBudget:0;
            float available=Mathf.Max(0,MaxSourcePower-auxiliaries);
            float maxTorque=Mathf.Min(24500,available*.85f/Mathf.Max(.1f,angularSpeed));
            float drag=angularSpeed*(1550+Touchdown*9000)+rotaryBudget*.017f;
            float wanted=motor&&Bus>.94f&&!protection?
                Mathf.Clamp((target/RpmFactor-angularSpeed)*Inertia*1.8f+drag, -maxTorque,maxTorque):0;
            InverterA=Mathf.MoveTowards(InverterA,wanted*.52f,dt*10200);
            InverterB=Mathf.MoveTowards(InverterB,wanted*.48f,dt*9200);
            float brake=protection?Mathf.Min(27000,angularSpeed*Inertia*1.16f):0;
            float torque=InverterA+InverterB;
            angularSpeed=Mathf.Max(0,angularSpeed+(torque-drag-brake)*dt/Inertia);
            phase=Mathf.Repeat(phase+angularSpeed*dt,2*Mathf.PI);
            CounterMomentum=Mathf.MoveTowards(CounterMomentum,Mathf.Clamp(-Momentum,-FlywheelCapacity,FlywheelCapacity),dt*23000);
            CounterAngle=Mathf.Repeat(CounterAngle+CounterMomentum*.008f*dt,360);
            TouchdownHeat+=dt*(angularSpeed*Touchdown*9-(TouchdownHeat-30)*.25f);

            RotaryPower=rotaryBudget*.84f;
            LoadPower=Mathf.Min(RotaryPower,demand);
            LoadDuty=loads?LoadPower/demand:0;
            InputPower=auxiliaries+Mathf.Max(0,torque*angularSpeed)/.85f;
            OpticalQuality=Mathf.MoveTowards(OpticalQuality,rotary&&power?Mathf.Clamp01(.99f-mode.magnitude*.16f-Mathf.Abs(offsetAxial)*.18f):0,dt*.92f);
            float heatLoad=loads?LoadPower*.00021f:0;
            HabitatTemperature+=dt*(heatLoad-(HabitatTemperature-24)*(LoadDuty>.96f?.68f:.18f));
            if(MissionHealthy)
            {
                RestoredThermalTime+=dt;
                MissionEnergy+=LoadPower*dt;
            }
            else {RestoredThermalTime=0;MissionEnergy=0;}

            float amplitude=mode.magnitude+centerVelocity.magnitude*.12f+Mathf.Abs(offsetAxial)*.1f;
            Vibration=Mathf.Lerp(Vibration,amplitude,dt*4);
            acquisition+=dt;
            if(acquisition>=.10f)
            {
                acquisition-=.10f;
                vibrationHistory[historyHead++%vibrationHistory.Length]=mode.x+centerVelocity.x*.09f;
                historyCount=Mathf.Min(historyCount+1,vibrationHistory.Length);
            }
            SampleAge+=dt;
            sensorTimer+=dt;
            if(power&&sensorTimer>=.10f)
            {
                sensorTimer-=.10f;SampleAge=0;SampleSequence++;
                Primary=5-center.x+Drift+.017f*Mathf.Sin(Clock*19);
                Secondary=5-center.x+.012f*Mathf.Sin(Clock*17);
                Axial=offsetAxial+.008f*Mathf.Sin(Clock*23);
            }
        }

        public bool ReadSpectrum()
        {
            int count=historyCount;
            if(count<36)return false;
            float best=0;
            for(int i=1;i<=count/4;i++)
            {
                float r=0,j=0;
                for(int n=0;n<count;n++)
                {
                    float value=vibrationHistory[(historyHead-count+n)%vibrationHistory.Length];
                    float angle=2*Mathf.PI*i*n/count;
                    r+=value*Mathf.Cos(angle);j+=value*Mathf.Sin(angle);
                }
                float energy=r*r+j*j;
                if(energy>best){best=energy;PeakFrequency=10f*i/count;}
            }
            return best>1e-8f;
        }
    }
}

