using UnityEngine;

namespace ProjectSpark.Gameplay
{
    // Normalized fictional PECVD and protected coupon model for the training level.
    public sealed class SparkLevel78Plant
    {
        public const float Step=.025f;
        public enum Cycle { Idle, Prepare, Cap, LowK, Cure, Stress }
        public enum Disturbance { None, Gas, Charge, UV }

        public readonly float[] Cap=new float[16],Film=new float[16],Damage=new float[16],Cure=new float[16],
            Moisture=new float[16],Pores=new float[16],Residual=new float[16],Warpage=new float[16];
        public float Pressure {get;private set;}=1;
        public float LoadLockPressure {get;private set;}=1;
        public float Turbo {get;private set;}
        public float Backing {get;private set;}
        public float Surface {get;private set;}
        public float CapFlow {get;private set;}
        public float MatrixFlow {get;private set;}
        public float PorogenFlow {get;private set;}
        public float FlowBalance {get;private set;}=1;
        public float RF {get;private set;}
        public float Match {get;private set;}
        public float Charge {get;private set;}
        public float Heater {get;private set;}
        public float Lamp {get;private set;}
        public float Shutter {get;private set;}
        public float Platen {get;private set;}
        public float Field {get;private set;}
        public float Exposure {get;private set;}
        public float CouponWear {get;private set;}
        public float CapInventory {get;private set;}=1;
        public float MatrixInventory {get;private set;}=1;
        public float PorogenInventory {get;private set;}=1;
        public float MeanCap=>Mean(Cap);
        public float MeanFilm=>Mean(Film);
        public float CureProgress=>Min(Cure);
        public float Pinhole
        {
            get
            {
                float worst=0;
                for(int i=0;i<Cap.Length;i++)worst=Mathf.Max(worst,1-Coverage(i));
                return worst;
            }
        }
        public float Coverage(int region)=>Mathf.Clamp01((Cap[region]-1.3f-(region%4)*.1f)/18.5f)*Surface;
        public float Diffusion=>.12f+Pinhole*.9f+Max(Damage)*.64f;
        public float Uniformity=>MeanFilm>0?(Max(Film)-Min(Film))/MeanFilm:0;
        public float Dielectric=>3.36f-1.15f*Mean(Pores)-.33f*CureProgress+.12f*Max(Moisture)+.18f*Max(Damage);
        public float Humidity=>Max(Moisture);
        public float Stress=>Max(Warpage);
        public float Delay=>.64f*Dielectric+.15f*Stress+.16f*Diffusion;
        public float LeakageIndex=>.020f+.31f*Pinhole+.76f*Humidity+.38f*Max(Damage)+.24f*Max(Residual)+CouponWear*.045f+.06f*Stress;
        public float LeakageSignal=>Field*LeakageIndex;
        public bool VacuumReady=>Backing>.96f&&Turbo>.94f&&Pressure<.045f&&LoadLockPressure<.08f;

        public SparkLevel78Plant()=>NewWafer();
        public void NewWafer()
        {
            for(int i=0;i<Cap.Length;i++)
            {
                Cap[i]=Film[i]=Damage[i]=Cure[i]=Pores[i]=Residual[i]=Warpage[i]=0;
                Moisture[i]=.037f+(i%4)*.002f;
            }
            Surface=.25f;
            Exposure=CouponWear=0;
        }

        public void Tick(float dt,Cycle mode,bool pumps,bool drive,bool lidSealed,Disturbance fault)
        {
            Backing=Mathf.MoveTowards(Backing,pumps?1:0,dt*.9f);
            Turbo=Mathf.MoveTowards(Turbo,pumps&&Backing>.68f?1:0,dt*.87f);
            float vacuum=pumps ? Mathf.Lerp(1,.018f,Backing*.38f+Turbo*.62f) : 1;
            Pressure=Mathf.Lerp(Pressure,vacuum,dt*2.2f);
            LoadLockPressure=Mathf.Lerp(LoadLockPressure,lidSealed?vacuum:1,dt*2.8f);
            bool cap=drive&&mode==Cycle.Cap,low=drive&&mode==Cycle.LowK,curing=drive&&mode==Cycle.Cure;
            bool plasma=cap||low;
            FlowBalance=Mathf.MoveTowards(FlowBalance,fault==Disturbance.Gas?.42f:1,dt*1.35f);
            CapInventory=Mathf.Clamp01(CapInventory+dt*(cap?-.024f*CapFlow:.16f));
            MatrixInventory=Mathf.Clamp01(MatrixInventory+dt*(low?-.027f*MatrixFlow:.16f));
            PorogenInventory=Mathf.Clamp01(PorogenInventory+dt*(low?-.042f*PorogenFlow:.16f));
            CapFlow=Mathf.MoveTowards(CapFlow,cap?Mathf.Clamp01(CapInventory*4)*FlowBalance:0,dt*2.8f);
            MatrixFlow=Mathf.MoveTowards(MatrixFlow,low?Mathf.Clamp01(MatrixInventory*4):0,dt*2.7f);
            PorogenFlow=Mathf.MoveTowards(PorogenFlow,low?.41f*Mathf.Clamp01(PorogenInventory*4):0,dt*1.8f);
            Match=Mathf.MoveTowards(Match,plasma?(fault==Disturbance.Charge?.46f:.98f):.12f,dt*1.65f);
            RF=Mathf.MoveTowards(RF,plasma?.83f:0,dt*2.65f);
            Charge=Mathf.MoveTowards(Charge,low?(fault==Disturbance.Charge?.72f:.067f):0,dt*(low?.96f:1.1f));
            Heater=Mathf.MoveTowards(Heater,curing?.74f:plasma?.53f:.17f,dt*.77f);
            Lamp=Mathf.MoveTowards(Lamp,curing?.94f:0,dt*2f);
            Shutter=Mathf.MoveTowards(Shutter,curing?(fault==Disturbance.UV?.17f:1):0,dt*1.8f);
            Platen=Mathf.MoveTowards(Platen,curing?180:0,dt*390);
            if(drive&&mode==Cycle.Prepare)Surface=Mathf.MoveTowards(Surface,1,dt*.81f);
            for(int i=0;i<Cap.Length;i++)
            {
                float edge=(i%4+i/4-3)/3f;
                if(cap&&VacuumReady&&Heater>.43f)
                {
                    Cap[i]+=dt*5.1f*CapFlow*RF*Match*(1+edge*(.012f+.23f*(1-FlowBalance)))*Surface;
                    Damage[i]+=dt*(1-FlowBalance)*.034f*(edge+1.4f);
                }
                if(low&&VacuumReady&&Heater>.43f)
                {
                    float block=1+edge*.012f;
                    Film[i]+=dt*18.4f*MatrixFlow*RF*Match*block;
                    Pores[i]=Mathf.Lerp(Pores[i],PorogenFlow*.77f,dt*.68f);
                    Residual[i]=Mathf.Lerp(Residual[i],PorogenFlow*.43f,dt*.63f);
                    Moisture[i]=Mathf.Lerp(Moisture[i],.086f+PorogenFlow*.07f,dt*.55f);
                    Damage[i]+=dt*Mathf.Max(0,Charge-.11f)*.40f;
                    Warpage[i]+=dt*(.0077f+Charge*.004f)*(1+edge*.12f);
                }
                if(curing)
                {
                    float dose=dt*Lamp*Shutter*Mathf.Clamp01(Heater/.71f)*(1+edge*.016f)*Mathf.Clamp01(Platen/165);
                    Cure[i]=Mathf.Clamp01(Cure[i]+dose*.40f);
                    Residual[i]*=Mathf.Exp(-dose*1.55f);
                    Moisture[i]*=Mathf.Exp(-dose*1.48f);
                    Pores[i]+=(.02f+Pores[i]*.017f)*dose;
                    Warpage[i]+=dose*.022f*Mathf.Max(0,Heater-.68f);
                }
            }
            Field=Mathf.MoveTowards(Field,drive&&mode==Cycle.Stress?.86f:0,dt*(drive&&mode==Cycle.Stress?1.6f:1.15f));
            if(drive&&mode==Cycle.Stress&&Field>.5f)
            {
                Exposure+=dt*Field;
                CouponWear+=dt*Field*LeakageIndex*.34f;
            }
        }
        private static float Mean(float[] a){float x=0;foreach(float v in a)x+=v;return x/a.Length;}
        private static float Min(float[] a){float x=float.PositiveInfinity;foreach(float v in a)x=Mathf.Min(x,v);return x;}
        private static float Max(float[] a){float x=0;foreach(float v in a)x=Mathf.Max(x,v);return x;}
    }
}
