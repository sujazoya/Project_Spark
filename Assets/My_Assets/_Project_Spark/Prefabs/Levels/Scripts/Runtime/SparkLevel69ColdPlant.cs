using UnityEngine;

namespace ProjectSpark.Gameplay
{
    /// <summary>Reduced-order educational fluid PS-69. Absolute pressure kPa, enthalpy kJ/kg, temperature °C.</summary>
    public static class SparkLevel69Refrigerant
    {
        public const float GasConstant=.080f, GasHeat=1.02f, LiquidHeat=1.38f;
        public const float MinTemperature=-32f,MaxTemperature=64f;
        public static float Pressure(float saturationTemperature)=>260f*Mathf.Exp(.037f*saturationTemperature);
        public static float Saturation(float pressure)=>Mathf.Log(Mathf.Max(1,pressure)/260f)/.037f;
        public static float Liquid(float temperature)=>185f+LiquidHeat*temperature;
        public static float Vapor(float saturationTemperature)=>Liquid(saturationTemperature)+178f-.49f*saturationTemperature;
        public static float Temperature(float pressure,float enthalpy)
        {
            float t=Saturation(pressure),l=Liquid(t),v=Vapor(t);
            return enthalpy<l?t+(enthalpy-l)/LiquidHeat:enthalpy>v?t+(enthalpy-v)/GasHeat:t;
        }
        public static float Quality(float pressure,float enthalpy)
        {
            float t=Saturation(pressure);
            return Mathf.Clamp01((enthalpy-Liquid(t))/(Vapor(t)-Liquid(t)));
        }
        public static float VaporMass(float pressure,float temperature,float volume)=>pressure*volume/(GasConstant*(temperature+273.15f));
    }

    /// <summary>One mass-conserving refrigeration circuit with finite thermal stores and per-branch evaporation.</summary>
    public sealed class SparkLevel69ColdPlant
    {
        public const float Step=.05f, Charge=.74f, Ambient=23f, RpmLimit=3600;
        private const float LowVolume=.0140f, HighVolume=.0028f;
        public readonly Zone[] Zones={new(9.8f,8.4f,4,.040f,.18f),new(8.3f,7.5f,2,.035f,.18f)};
        private float vaporLow=SparkLevel69Refrigerant.VaporMass(SparkLevel69Refrigerant.Pressure(-3),2,LowVolume);
        private float oilCarry;
        private float condenser=28, regulatorLoss;
        public float Condenser=>condenser;
        public float SuctionPressure {get;private set;}=SparkLevel69Refrigerant.Pressure(-3);
        public float DischargePressure {get;private set;}=SparkLevel69Refrigerant.Pressure(28);
        public float Evaporation=>SparkLevel69Refrigerant.Saturation(SuctionPressure);
        public float LiquidTemperature {get;private set;}=25;
        public float DischargeTemperature {get;private set;}
        public float SuctionTemperature {get;private set;}=2;
        public float Rpm {get;private set;}
        public float MassFlow {get;private set;}
        public float Fan {get;private set;}
        public float HeaterKw {get;private set;}
        public float CompressorKw {get;private set;}
        public float ElectricKw {get;private set;}
        public float RejectionKw {get;private set;}
        public float CoolingKw=>Zones[0].Duty+Zones[1].Duty;
        public float Cop=>ElectricKw>.045f?CoolingKw/ElectricKw:0;
        public float Receiver {get;private set;}=.45f;
        public float Accumulator {get;private set;}
        public float HighVapor {get;private set;}
        public float Oil=>.052f-oilCarry;
        public float Melted {get;private set;}
        public float Drain {get;private set;}
        public float Drained {get;private set;}
        public float CondensationFraction=>Mathf.Clamp01((Receiver-.045f)/.21f);
        public float EnergyInput {get;private set;}
        public float UsefulCooling {get;private set;}
        public float MassError=>Mathf.Abs(vaporLow+HighVapor+Receiver+Accumulator-Charge);
        public float CycleResidual=>Mathf.Abs((Enthalpy[1]-Enthalpy[2])-(Enthalpy[1]-Enthalpy[0])-(Enthalpy[0]-Enthalpy[3]));
        public readonly float[] Enthalpy=new float[4];
        public float ValveFlowCoefficient=>.00102f*Mathf.Sqrt(Mathf.Max(0,DischargePressure-SuctionPressure))*Mathf.Clamp01((Receiver-.020f)/.080f);
        public float LatentLift(float sh)=>Mathf.Max(30,SparkLevel69Refrigerant.Vapor(Evaporation)+sh*SparkLevel69Refrigerant.GasHeat-Enthalpy[3]);

        public sealed class Zone
        {
            public readonly float Nominal, Leak, Coil;
            public float Air,Cargo,Outlet,Superheat,Valve,AirFan,Frost=.004f,Duty,Feed,Quality=1f,Door;
            public Zone(float air,float cargo,float target,float leak,float coil)
            {
                Air=air;Cargo=cargo;Nominal=target;Leak=leak;Coil=coil;
            }
            public float Capacity(float evaporation,float fan)=>Coil*Mathf.Max(0,Air-evaporation)*(.10f+.90f*fan)/(1+13f*Frost);
        }

        public sealed class Pi
        {
            private float integral,prior,derivative;
            private bool started;
            public void Reset(){integral=prior=derivative=0;started=false;}
            public float StepToward(float error,float kp,float ki,float bias,float min,float max,float dt,float kd=0)
            {
                if(started) derivative=Mathf.Lerp(derivative,(error-prior)/dt,dt/(dt+.32f));
                started=true;prior=error;
                float raw=bias+kp*error+ki*integral+kd*derivative;
                if((raw<max||error<0)&&(raw>min||error>0)) integral=Mathf.Clamp(integral+error*dt,-36,36);
                return Mathf.Clamp(bias+kp*error+ki*integral+kd*derivative,min,max);
            }
        }

        public void Tick(float dt,float speed,float condenserFan,float fanA,float fanB,float openingA,float openingB,
            bool humid,bool defrost,bool doorOpen)
        {
            Fan=Mathf.MoveTowards(Fan,condenserFan,.65f*dt);
            float rpm=Rpm/RpmLimit;
            float ratio=DischargePressure/Mathf.Max(SuctionPressure,70);
            float drag=(.075f+.035f*ratio)*rpm;
            float torque=speed>0?Mathf.Clamp((speed-rpm)*3f+drag,0,1.15f):0;
            rpm=speed>0?Mathf.Clamp01(rpm+(torque-drag)*dt/.74f):Mathf.MoveTowards(rpm,0,.90f*dt);
            Rpm=rpm*RpmLimit;
            float evaporating=Mathf.Clamp(Evaporation,SparkLevel69Refrigerant.MinTemperature,15);
            float saturation=Mathf.Clamp(condenser,Ambient,SparkLevel69Refrigerant.MaxTemperature);
            DischargePressure=SparkLevel69Refrigerant.Pressure(saturation);
            float subcool=Mathf.Clamp((.7f+4.2f*Fan)*CondensationFraction,.20f,5.2f);
            LiquidTemperature=saturation-subcool;
            float liquid=SparkLevel69Refrigerant.Liquid(LiquidTemperature);
            Enthalpy[2]=Enthalpy[3]=liquid;
            float satVapor=SparkLevel69Refrigerant.Vapor(evaporating);
            float sumReturnVapor=0,weightedSH=0;
            float budget=Mathf.Max(0,Receiver-.012f)/dt;
            Zones[0].Door=Mathf.MoveTowards(Zones[0].Door,doorOpen?1:0,2*dt);
            for(int i=0;i<2;i++)
            {
                var z=Zones[i];
                float incoming=i==0?openingA:openingB;
                z.Valve=Mathf.MoveTowards(z.Valve,incoming,(incoming==0?1.7f:.65f)*dt);
                z.AirFan=Mathf.MoveTowards(z.AirFan,i==0?fanA:fanB,1.15f*dt);
                z.Feed=Mathf.Min(budget,ValveFlowCoefficient*z.Valve);
                budget-=z.Feed;
                float q=z.Capacity(evaporating,z.AirFan);
                z.Duty=Mathf.Min(q,z.Feed*Mathf.Max(0,satVapor+19*SparkLevel69Refrigerant.GasHeat-liquid));
                float h=z.Feed>.00005f?liquid+z.Duty/z.Feed:satVapor;
                z.Quality=SparkLevel69Refrigerant.Quality(SuctionPressure,h);
                z.Outlet=z.Feed>.00005f?SparkLevel69Refrigerant.Temperature(SuctionPressure,h):z.Air;
                z.Superheat=z.Feed>.00005f?Mathf.Max(0,z.Outlet-evaporating):0;
                float vapor=z.Feed*z.Quality;
                sumReturnVapor+=vapor;
                weightedSH+=vapor*z.Superheat;

                float latentFrost=0;
                if(z.Duty>.10f&&evaporating<0&&z.AirFan>.25f)
                {
                    float rate=(humid&&i==1?.022f:.00016f)*z.AirFan;
                    rate=Mathf.Min(rate,Mathf.Max(0,.155f-z.Frost)/dt);
                    rate=Mathf.Min(rate,z.Duty/334f*.76f);
                    z.Frost+=rate*dt;
                    latentFrost=rate*334f;
                }
                float ambientLoad=z.Leak*(Ambient-z.Air)+.105f*z.Door*(Ambient-z.Air)+.19f*(z.Cargo-z.Air);
                float doorCargo=.19f*(z.Air-z.Cargo);
                z.Air+=(ambientLoad-z.Duty+latentFrost)*dt/(i==0?10f:9f);
                z.Cargo+=doorCargo*dt/(i==0?14f:13f);
            }
            float receiverTemperature=SuctionTemperature;
            SuctionTemperature=evaporating+(sumReturnVapor>.00005f?weightedSH/sumReturnVapor:3.0f);
            SuctionPressure=Mathf.Clamp(vaporLow*SparkLevel69Refrigerant.GasConstant*(receiverTemperature+273.15f)/LowVolume,78,380);
            float actualSaturation=SparkLevel69Refrigerant.Saturation(SuctionPressure);
            float suctionH=SparkLevel69Refrigerant.Vapor(actualSaturation)+Mathf.Max(0,SuctionTemperature-actualSaturation)*SparkLevel69Refrigerant.GasHeat;
            Enthalpy[0]=suctionH;
            float specificWork=SparkLevel69Refrigerant.GasHeat*(SuctionTemperature+273.15f)*
                (Mathf.Pow(Mathf.Max(1,DischargePressure/SuctionPressure),.128f)-1)/.76f;
            Enthalpy[1]=suctionH+specificWork;
            DischargeTemperature=SparkLevel69Refrigerant.Temperature(DischargePressure,Enthalpy[1]);
            MassFlow=Mathf.Min(vaporLow/dt,rpm*.048f*(SuctionPressure/220f)*Mathf.Clamp(1-.065f*(ratio-1),.36f,1));
            CompressorKw=MassFlow*specificWork/.92f;
            float carry=Mathf.Max(0,Zones[0].Feed+Zones[1].Feed-sumReturnVapor);
            float boil=Mathf.Min(Accumulator/dt,.0018f);
            Accumulator+=(carry-boil)*dt;
            vaporLow+=(sumReturnVapor+boil-MassFlow)*dt;
            vaporLow=Mathf.Max(.005f,vaporLow);
            oilCarry=Mathf.Clamp(oilCarry+(MassFlow*.010f-oilCarry*.32f)*dt,0,.041f);

            float condDuty=MassFlow*Mathf.Max(0,Enthalpy[1]-liquid);
            RejectionKw=(.031f+.41f*Fan)*(condenser-Ambient);
            regulatorLoss=Mathf.Max(0,condenser-61)*.15f;
            condenser+=(condDuty-RejectionKw-regulatorLoss)*dt/3.3f;
            condenser=Mathf.Clamp(condenser,Ambient,SparkLevel69Refrigerant.MaxTemperature);
            HighVapor=Mathf.Min(Mathf.Max(0,Charge-Accumulator-vaporLow),SparkLevel69Refrigerant.VaporMass(DischargePressure,saturation,HighVolume));
            Receiver=Mathf.Max(0,Charge-vaporLow-Accumulator-HighVapor);

            HeaterKw=defrost?1.55f:0;
            if(defrost)
            {
                var z=Zones[1];
                float melt=Mathf.Min(z.Frost,HeaterKw*.81f*dt/334f);
                z.Frost-=melt;Melted+=melt;Drain+=melt;
                z.Air+=(HeaterKw*dt-334f*melt)/9f;
            }
            float drain=Mathf.Min(Drain,.005f*dt);
            Drain-=drain;Drained+=drain;
            ElectricKw=CompressorKw+.028f+.17f*Fan*Fan*Fan+.040f*(Zones[0].AirFan+Zones[1].AirFan)+HeaterKw;
            EnergyInput+=ElectricKw*dt;
            UsefulCooling+=CoolingKw*dt;
        }
    }
}

