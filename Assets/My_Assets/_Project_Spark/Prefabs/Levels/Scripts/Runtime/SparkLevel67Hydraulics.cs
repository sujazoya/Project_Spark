using UnityEngine;

namespace ProjectSpark.Gameplay
{
    /// <summary>One fixed-step process plant. Volumes are litres, flow L/min, pressure kPa and shaft speed RPM.</summary>
    public sealed class SparkLevel67Hydraulics
    {
        public const float Step = .05f;
        public const float RpmLimit = 1800f;
        public const float Density = 998f;
        public const float Gravity = 9.81f;
        public const float NominalProductLitres = 3f;

        public sealed class Vessel
        {
            public readonly float Capacity, FullDepth;
            public float A { get; private set; }
            public float B { get; private set; }
            public float Volume => A + B;
            public float Head => FullDepth * Volume / Capacity;
            public float FractionA => Volume > .0001f ? A / Volume : 0f;
            public Vessel(float capacity, float depth, float a, float b)
            {
                Capacity = capacity; FullDepth = depth; A = a; B = b;
            }
            public float TransferTo(Vessel receiver, float requested)
            {
                float room = receiver == null ? requested : Mathf.Max(0, receiver.Capacity - receiver.Volume);
                float volume = Mathf.Clamp(requested, 0, Mathf.Min(Volume, room));
                float a = Mathf.Min(A, volume * FractionA);
                float b = Mathf.Min(B, volume - a);
                A -= a; B -= b;
                if (receiver != null) { receiver.A += a; receiver.B += b; }
                return a + b;
            }
        }

        public sealed class Pump
        {
            private readonly float shutoffHead, droop, resistance, elevation, minSuction;
            public float Rpm, Head, Flow, Suction, Current, Temperature=22;
            public bool Primed, Cavitating;
            public Pump(float shutoffHead,float droop,float resistance,float elevation,float minimumSuction)
            {
                this.shutoffHead=shutoffHead; this.droop=droop; this.resistance=resistance;
                this.elevation=elevation; minSuction=minimumSuction;
            }
            public float Advance(float command, float opening, float suction, float dt)
            {
                Suction = Mathf.Max(0,suction);
                float speed = Rpm / RpmLimit;
                float hydraulicKw = Density * Gravity * Mathf.Max(0,Head) * Flow / 60000f / 1000f;
                float load = .07f * speed * speed + hydraulicKw * .58f;
                float torque = command > 0 ? Mathf.Clamp((command-speed)*2.2f+load,0,.9f) : 0;
                speed = command > 0 ? Mathf.Clamp01(speed+(torque-load)*dt/.8f) : Mathf.MoveTowards(speed,0,dt*.67f);
                Rpm = speed * RpmLimit;
                Current = torque > 0 ? .17f + torque * 1.7f : 0f;
                Temperature += (Current*Current*.30f-(Temperature-22f)*.06f)*dt;
                Cavitating = command > .25f && Primed && Suction < minSuction*speed*speed;
                float efficiency = Primed && Suction > .015f ? (Cavitating ? Mathf.Clamp01(Suction/(minSuction*speed*speed)) : 1f) : 0f;
                float maxHead = shutoffHead*speed*speed*efficiency;
                float restriction = resistance/Mathf.Max(.012f,opening*opening);
                float potential = Mathf.Sqrt(Mathf.Max(0,maxHead+Suction-elevation)/(droop+restriction));
                return opening > .015f && efficiency > 0 ? potential : 0f;
            }
            public void RecordFlow(float flow)
            {
                Flow=flow;
                Head=Mathf.Max(0,shutoffHead*Mathf.Pow(Rpm/RpmLimit,2)*(Cavitating?.45f:1f)-droop*flow*flow);
            }
        }

        public sealed class Controller
        {
            private float integral, previous, derivative;
            private bool initialized;
            public void Reset() { integral=previous=derivative=0; initialized=false; }
            public float Update(float error,float kp,float ki,float kd,float bias,float minimum,float maximum,float dt)
            {
                if (initialized) derivative=Mathf.Lerp(derivative,(error-previous)/dt,dt/(.22f+dt));
                else initialized=true;
                previous=error;
                float unconstrained=bias+kp*error+ki*integral+kd*derivative;
                if ((unconstrained<maximum||error<0)&&(unconstrained>minimum||error>0))
                    integral=Mathf.Clamp(integral+error*dt,-18f,18f);
                return Mathf.Clamp(bias+kp*error+ki*integral+kd*derivative,minimum,maximum);
            }
        }

        public readonly Vessel[] Tanks={new(58,2.42f,41,0),new(42,2.42f,0,29),new(8,1.73f,.56f,.24f),new(8,2.14f,2.065f,.885f)};
        public readonly Pump[] Pumps={new(11,.0115f,.0105f,.67f,.42f),new(10,.051f,.048f,.70f,.39f),new(12,.0043f,.006f,.47f,.33f)};
        public readonly float[] Valves=new float[4];
        public float MixerRpm { get; private set; }
        public float Withdrawn { get; private set; }
        public float TransferTotal { get; private set; }
        public float WithdrawalFlow { get; private set; }
        public float MixingFraction { get; private set; } = .70f;
        public float MassError => Mathf.Abs(Tanks[0].Volume+Tanks[1].Volume+Tanks[2].Volume+Tanks[3].Volume+Withdrawn-73.75f);

        public void Tick(Vector3 speeds, bool feedA, bool feedB, bool transfer, bool mixer, bool release,
            bool plantSafe, float pressureLoss, float demand, float dt)
        {
            Valves[0]=Mathf.MoveTowards(Valves[0],feedA?.86f:0,dt*.83f);
            Valves[1]=Mathf.MoveTowards(Valves[1],feedB?.86f:0,dt*.83f);
            Valves[2]=Mathf.MoveTowards(Valves[2],release?1:0,dt*.95f);
            Valves[3]=Mathf.MoveTowards(Valves[3],plantSafe?1:0,dt*1.2f);
            MixerRpm=Mathf.MoveTowards(MixerRpm,mixer?520:0,450*dt);
            float qA=Pumps[0].Advance(speeds.x,Valves[0],Tanks[0].Head-pressureLoss,dt);
            float qB=Pumps[1].Advance(speeds.y,Valves[1],Tanks[1].Head-pressureLoss*.55f,dt);
            float qP=Pumps[2].Advance(speeds.z,Valves[3],Tanks[2].Head,dt);
            float factor=dt/60;
            float movedA=Tanks[0].TransferTo(Tanks[2],feedA?qA*factor:0);
            float movedB=Tanks[1].TransferTo(Tanks[2],feedB?qB*factor:0);
            MixingFraction=Mathf.Lerp(MixingFraction,Tanks[2].FractionA,dt*(MixerRpm>330?1.6f:.11f));
            float movedP=Tanks[2].TransferTo(Tanks[3],transfer?qP*factor:0);
            float removed=Tanks[3].TransferTo(null,release?demand*Mathf.Sqrt(Tanks[3].Volume/NominalProductLitres)*Valves[2]*Valves[3]*factor:0);
            Pumps[0].RecordFlow(movedA/factor);
            Pumps[1].RecordFlow(movedB/factor);
            Pumps[2].RecordFlow(movedP/factor);
            TransferTotal+=movedP;
            Withdrawn+=removed;
            WithdrawalFlow=removed/factor;
        }
    }
}
