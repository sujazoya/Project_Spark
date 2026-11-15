using UnityEngine;

namespace ProjectSpark.Gameplay
{
    /// <summary>Fixed-step, equal-density water model. Flow in L/min, temperatures in Celsius, heat in kW, energy in kJ.</summary>
    public sealed class SparkLevel68ThermalPlant
    {
        public const float Step = .05f;
        public const float WaterHeat = 4.184f; // kJ/(L K)
        public const float HeaterLimit = 62f;
        public const float NormalFlow = 12f;
        public const float Ambient = 20f;
        private const float HotLitres = 5.5f, ColdLitres = 6f, BufferLitres = 7f, HoldLitres = 1.15f;
        private const float InitialSupply = 180f;
        public float Hot { get; private set; } = 46f;
        public float Cold { get; private set; } = 20f;
        public float Buffer { get; private set; } = 44f;
        public float Product { get; private set; } = 25f;
        public float Inlet { get; private set; } = 25f;
        public float Preheated { get; private set; } = 25f;
        public float Discharge { get; private set; } = 25f;
        public float TrimEfficiency { get; private set; }
        public float RegenerativeKw { get; private set; }
        public float TrimKw { get; private set; }
        public float CoolingKw { get; private set; }
        public float HeaterKw { get; private set; }
        public float StorageAdjustedUtility => TrimKw+.033f*(Hot-Ambient)+.020f*(Buffer-Ambient);
        public float DifferentialKpa { get; private set; }
        public float Fouling { get; private set; }
        private double supply = InitialSupply, onSpecLitres, divertedLitres;
        public float Supply => (float)supply;
        public float OnSpecLitres => (float)onSpecLitres;
        public float DivertedLitres => (float)divertedLitres;
        public float UtilityEnergy { get; private set; }
        public float RecoveredEnergy { get; private set; }
        public float TrimEnergy { get; private set; }
        private double balanceError, energyThroughput;
        public float BalanceRelative => (float)(balanceError/System.Math.Max(1,energyThroughput));
        public float MassError => (float)System.Math.Abs(supply+onSpecLitres+divertedLitres-InitialSupply);
        public readonly float[] Valves = new float[5];
        public readonly Pump[] Pumps = {new(19f),new(16f),new(16f)};

        public sealed class Pump
        {
            private readonly float maximumFlow;
            public float Rpm { get; private set; }
            public float Flow { get; private set; }
            public float Current { get; private set; }
            public Pump(float maxFlow) { maximumFlow=maxFlow; }
            public void Step(float command,float restriction,float available,float dt)
            {
                float speed=Rpm/1800f;
                float drag=(.09f+.07f*restriction)*speed*speed;
                float torque=command>0?Mathf.Clamp((command-speed)*2.5f+drag,0,1.2f):0;
                speed=command>0?Mathf.Clamp01(speed+(torque-drag)*dt/.85f):Mathf.MoveTowards(speed,0,.92f*dt);
                Rpm=speed*1800;
                Flow=maximumFlow*speed*restriction*Mathf.Clamp01(available);
                Current=torque>0?.16f+torque*1.55f:0;
            }
            public void SetDelivered(float flow) { Flow=flow; }
        }
        public sealed class Pid
        {
            private float integral,previous,rate;
            private bool started;
            public void Reset(){started=false;integral=previous=rate=0;}
            public float Update(float error,float kp,float ki,float kd,float bias,float min,float max,float dt)
            {
                if(started) rate=Mathf.Lerp(rate,(error-previous)/dt,dt/(.45f+dt));
                started=true; previous=error;
                float result=bias+kp*error+ki*integral+kd*rate;
                if((result<max||error<0)&&(result>min||error>0))
                    integral=Mathf.Clamp(integral+error*dt,-60,60);
                return Mathf.Clamp(bias+kp*error+ki*integral+kd*rate,min,max);
            }
        }

        private sealed class ExchangerWall
        {
            private readonly float capacity;
            private float temperature;
            public double Energy => (double)capacity*temperature;
            public ExchangerWall(float capacity,float temperature)
            {
                this.capacity=capacity;
                this.temperature=temperature;
            }

            public (float donor,float receiver) Transfer(float steadyDuty,float hot,float cold,float dt)
            {
                if(steadyDuty<=0) return (0,0);
                float storage=Mathf.Clamp(capacity*((hot+cold)*.5f-temperature)/1.35f,-steadyDuty*.60f,steadyDuty*.60f);
                temperature+=storage*dt/capacity;
                return (steadyDuty+storage*.5f,steadyDuty-storage*.5f);
            }
        }

        private readonly ExchangerWall regenerativeWall=new(1.20f,25f), trimWall=new(1.55f,36f);
        private double StoredEnergy => WaterHeat*((double)Hot*HotLitres+(double)Cold*ColdLitres+(double)Buffer*BufferLitres+(double)Product*HoldLitres)
            +regenerativeWall.Energy+trimWall.Energy;

        private static float Exchange(float ua,float aCapacity,float bCapacity,float a,float b)
        {
            float min=Mathf.Min(aCapacity,bCapacity);
            return min>.00001f?min*(1-Mathf.Exp(-ua/min))*(a-b):0;
        }
        private static float CapacityRate(float flow)=>flow*WaterHeat/60;

        public void Tick(bool hotPump,bool coldPump,bool productPump,bool heaterPermit,bool release,bool regen,
            float powerDemand,float inletTarget,float productCommand,bool foul,float target,float dt)
        {
            double previousEnergy=StoredEnergy;
            Inlet=Mathf.MoveTowards(Inlet,inletTarget,4*dt);
            Fouling=Mathf.MoveTowards(Fouling,foul?.66f:0,dt*.11f);
            Valves[0]=Mathf.MoveTowards(Valves[0],hotPump?.92f:0,dt*.65f);
            Valves[1]=Mathf.MoveTowards(Valves[1],coldPump?.92f:0,dt*.65f);
            Valves[2]=Mathf.MoveTowards(Valves[2],hotPump?Mathf.Clamp01(powerDemand/HeaterLimit*.22f+.76f):0,dt*.67f);
            Valves[3]=Mathf.MoveTowards(Valves[3],regen?0:1,dt*.60f);
            Valves[4]=Mathf.MoveTowards(Valves[4],release?1:0,dt*.85f);
            Pumps[0].Step(hotPump?.93f:0,Valves[0]*(.64f+.36f*Valves[2]),1,dt);
            Pumps[1].Step(coldPump?.88f:0,Valves[1],1,dt);
            Pumps[2].Step(productPump?productCommand:0,1,Supply/1.5f,dt);
            float flow=Mathf.Min(Pumps[2].Flow,Supply*60f/dt);
            Pumps[2].SetDelivered(flow);
            float p=CapacityRate(flow),h=CapacityRate(Pumps[0].Flow),c=CapacityRate(Pumps[1].Flow);
            float raw=Inlet, held=Product;
            float regenCapacity=p*(1-Valves[3]);
            float regenNtu=2.0f/(1+.8f*Fouling)/Mathf.Max(.0001f,regenCapacity);
            // Counterflow effectiveness at equal hot and cold heat-capacity rates.
            float regenerativeDuty=Mathf.Max(0,regenNtu/(1+regenNtu)*regenCapacity*(held-raw));
            var regenerative=regenerativeWall.Transfer(regenerativeDuty,held,raw,dt);
            RegenerativeKw=regenerative.receiver;
            Preheated=p>.00001f?raw+RegenerativeKw/p:raw;
            float min=Mathf.Min(h,p);
            float ua=1.28f/(1+.52f*Fouling);
            TrimEfficiency=p>.00001f&&min>.00001f?min*(1-Mathf.Exp(-ua/min))/p:0;
            float trimDuty=Mathf.Max(0,Exchange(ua,h,p,Hot,Preheated));
            var trim=trimWall.Transfer(trimDuty,Hot,Preheated,dt);
            TrimKw=trim.receiver;
            float hotEffluent=p>.00001f?held-regenerative.donor/p:held;
            CoolingKw=Mathf.Max(0,Exchange(.46f,c,p,hotEffluent,Cold));
            Discharge=p>.00001f?hotEffluent-CoolingKw/p:hotEffluent;
            DifferentialKpa=(8.2f+9*Fouling)*Mathf.Pow(flow/NormalFlow,2);
            float bufferHeat=hotPump?.85f*(Buffer-Hot):.13f*(Buffer-Hot);
            float hotLoss=.033f*(Hot-Ambient),bufferLoss=.020f*(Buffer-Ambient);
            float rejection=Mathf.Max(0,(coldPump?1.25f:.20f)*(Cold-Ambient));
            float power=heaterPermit?Mathf.Clamp(powerDemand,0,HeaterLimit):0;
            HeaterKw=heaterPermit?Mathf.MoveTowards(HeaterKw,power,96*dt):0;
            Hot+=(HeaterKw-trim.donor+bufferHeat-hotLoss)*dt/(HotLitres*WaterHeat);
            Cold+=(CoolingKw-rejection)*dt/(ColdLitres*WaterHeat);
            Buffer+=(-bufferHeat-bufferLoss)*dt/(BufferLitres*WaterHeat);
            Product+=(p*(Preheated-held)+TrimKw)*dt/(HoldLitres*WaterHeat);
            float moved=Mathf.Min(Supply,flow*dt/60);
            float accepted=release&&Mathf.Abs(held-target)<=1f&&held<72&&Discharge<=55?moved*Valves[4]:0;
            supply-=moved;
            onSpecLitres+=accepted;
            divertedLitres+=(double)moved-accepted;
            UtilityEnergy+=HeaterKw*dt;
            RecoveredEnergy+=RegenerativeKw*dt;
            TrimEnergy+=TrimKw*dt;
            double external=(HeaterKw+p*(raw-Discharge)-hotLoss-bufferLoss-rejection)*dt;
            balanceError+=System.Math.Abs(StoredEnergy-previousEnergy-external);
            energyThroughput+=(HeaterKw+RegenerativeKw+TrimKw+CoolingKw+Mathf.Abs((float)external))*dt;
        }
    }
}


