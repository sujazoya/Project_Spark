using UnityEngine;

namespace ProjectSpark.Gameplay
{
    // Fictional normalized electrochemical model. Plating power is separate from the 5 V controls.
    public sealed class SparkLevel76Plant
    {
        public const float Step=.025f;
        public readonly float[] Fill=new float[16], Wall=new float[16], Copper=new float[16], Void=new float[16];
        public float Flow {get;private set;}
        public float Pump {get;private set;}
        public float LinePressure {get;private set;}
        public float Temperature {get;private set;}=23;
        public float Bubbles {get;private set;}=.28f;
        public float Wet {get;private set;}
        public float Suppressor {get;private set;}
        public float Accelerator {get;private set;}
        public float Leveler {get;private set;}
        public float Reserve {get;private set;}=60;
        public float Resistance {get;private set;}=.095f;
        public float Current {get;private set;}
        public float Voltage {get;private set;}
        public float Potential {get;private set;}=.23f;
        public float Spindle {get;private set;}
        public float Probe {get;private set;}
        public float DryerSpin {get;private set;}
        public float Lid {get;private set;}
        public float Lift {get;private set;}
        public float IntegratedCharge {get;private set;}
        public float AccountedCopper {get;private set;}
        public float Moisture {get;private set;}=1;
        public float Residue {get;private set;}=1;
        public float Roughness {get;private set;}
        public float Conductivity=>.9f+.09f*(Temperature-23)/9f+Reserve*.0009f;
        public float Seed=>.998f;
        public float Continuity=>Mathf.Clamp01(Seed-Mathf.Max(0,Resistance-.10f)*.33f);
        public float MeanFill=>Mean(Fill);
        public float MeanCopper=>Mean(Copper);
        public float WorstVoid=>Max(Void);
        public float LeastFill=>Min(Fill);
        public float Overburden=>Mathf.Max(0,MeanCopper-1.04f);
        public float Uniformity=>(Max(Copper)-Min(Copper))/Mathf.Max(.001f,MeanCopper);
        public float TestResistance=>1/Mathf.Max(.05f,MeanFill*(1-WorstVoid)*Continuity);
        public float MassError=>Mathf.Abs(AccountedCopper-MeanCopper);
        private static float Mean(float[] values){float sum=0;foreach(float x in values)sum+=x;return sum/values.Length;}
        private static float Min(float[] values){float v=float.PositiveInfinity;foreach(float x in values)v=Mathf.Min(v,x);return v;}
        private static float Max(float[] values){float v=0;foreach(float x in values)v=Mathf.Max(v,x);return v;}

        public void NewWafer()
        {
            for(int i=0;i<16;i++)Fill[i]=Wall[i]=Copper[i]=Void[i]=0;
            Wet=IntegratedCharge=AccountedCopper=Roughness=0;
            Moisture=Residue=1;
        }

        public void Tick(float dt,bool pump,bool filter,bool degas,bool heat,bool additives,bool prewet,bool clamped,bool transfer,
            bool probe,bool rectifier,float duty,float zoneTrim,bool contactFault,bool blocked,bool dosingDrift,int clean,float clock)
        {
            Pump=Mathf.MoveTowards(Pump,pump?1:0,dt*.93f);
            Flow=Mathf.MoveTowards(Flow,Pump*(blocked?.43f:filter?1.04f:.78f),dt*1.78f);
            LinePressure=Mathf.Lerp(LinePressure,Pump*(blocked?1.72f:1.08f),dt/.24f);
            Temperature+=dt*((heat?4.2f-.41f*(Temperature-23):0)+Current*.13f-.06f*(Temperature-23));
            Bubbles=Mathf.Lerp(Bubbles,degas&&filter?.012f:.29f+Current*.02f,dt/(degas?.68f:.42f));
            Suppressor=Mathf.MoveTowards(Suppressor,additives?.73f:0,dt*.45f);
            Accelerator=Mathf.MoveTowards(Accelerator,additives?(dosingDrift?.31f:.71f):0,dt*.42f);
            Leveler=Mathf.MoveTowards(Leveler,additives?.69f:0,dt*.43f);
            if(prewet)Wet=Mathf.MoveTowards(Wet,1-Bubbles*.25f,dt*Flow*.65f);
            Resistance=Mathf.MoveTowards(Resistance,contactFault?.39f:.095f,dt*.54f);
            Spindle=Mathf.MoveTowards(Spindle,clamped?62:0,dt*70);
            Lid=Mathf.MoveTowards(Lid,transfer?1:0,dt*3.2f);
            Lift=Mathf.MoveTowards(Lift,clamped?0:transfer?.80f:.45f,dt*2);
            Probe=Mathf.MoveTowards(Probe,probe?1:0,dt*3.1f);
            DryerSpin=Mathf.MoveTowards(DryerSpin,clean==2?490:0,dt*1350);
            float pulse=Mathf.Repeat(clock*3.65f,1)<.69f?1.09f:.85f;
            float target=rectifier?Mathf.Min(1.1f,duty*pulse)*Conductivity/(1+Resistance*.38f):0;
            Current=Mathf.MoveTowards(Current,target,dt*(rectifier?3.4f:2.23f));
            Voltage=.24f+Current*(.42f+Resistance+.15f/Conductivity);
            Potential=Mathf.Lerp(Potential,.23f+Current*(.31f+Resistance*.49f),dt/.13f);
            if(clean==1)Residue=Mathf.MoveTowards(Residue,0,dt*.89f);
            if(clean==2)Moisture=Mathf.MoveTowards(Moisture,0,dt*.91f*Mathf.Clamp01(DryerSpin/470));
            if(Current<.0001f||!clamped)return;

            // Charge drives copper mass. Local current distribution sums to the global deposition.
            float charge=dt*Current;
            float efficiency=Mathf.Clamp(.95f-Bubbles*.15f-Mathf.Abs(Accelerator-.71f)*.15f,.6f,.96f);
            float amount=charge*.146f*efficiency;
            IntegratedCharge+=charge;
            AccountedCopper+=amount;
            Reserve=Mathf.Max(0,Reserve-amount*.65f);
            for(int i=0;i<16;i++)
            {
                float rim=(Mathf.Abs(i%4-1.5f)+Mathf.Abs(i/4-1.5f))/3;
                float balance=rim-2f/3f;
                float distributed=amount*(1+balance*(zoneTrim+(1-Flow)*.21f+(Resistance-.095f)*.43f));
                Copper[i]+=distributed;
                float bottom=distributed*Mathf.Clamp(1.012f+(Accelerator-.71f)*.75f-(1-Wet)*.40f- Bubbles*.22f,.41f,1.05f);
                float wall=distributed*(.13f+(.73f-Suppressor)*.37f+(.69f-Leveler)*.14f+Bubbles*.15f);
                if(Wall[i]<.5f)Fill[i]=Mathf.Min(1,Fill[i]+bottom);
                Wall[i]+=Mathf.Max(0,wall);
                if(Wall[i]>=.5f)Void[i]=Mathf.Max(Void[i],1-Fill[i]);
                Void[i]+=distributed*Mathf.Max(0,Bubbles-.021f)*.024f;
            }
            Roughness+=amount*(Mathf.Max(0,.65f-Leveler)*.08f+Mathf.Max(0,.78f-Flow)*.05f);
        }
    }
}
