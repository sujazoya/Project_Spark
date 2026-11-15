using UnityEngine;

namespace ProjectSpark.Gameplay
{
    // Normalized hybrid-bonding training model. The regions represent an interface, not a fabrication recipe.
    public sealed class SparkLevel79Plant
    {
        public const float Step=.025f;
        public enum Cycle { Idle, Home, Prep, Transfer, Align, Approach, Bond, Anneal, Scan, Test }
        public enum Disturbance { None, Vision, Chuck, Particle }
        public readonly float[] Position=new float[6], Velocity=new float[6], Error=new float[6];
        private readonly float[] front=new float[16], voids=new float[16];
        public float Gap=>Mathf.Clamp01(1+Position[2]);
        public float Force {get;private set;}
        public float Imbalance {get;private set;}
        public float CameraOffset {get;private set;}
        public float ChuckResidual {get;private set;}
        public float ParticleMass {get;private set;}
        public float Energy {get;private set;}
        public float Flatness {get;private set;}
        public float Temperature {get;private set;}
        public float ThermalDose {get;private set;}
        public float Motion=>Mathf.Abs(Velocity[0])+Mathf.Abs(Velocity[1])+Mathf.Abs(Velocity[2]);
        public float PoseMotion=>Motion+Mathf.Abs(Velocity[3])+Mathf.Abs(Velocity[4])+Mathf.Abs(Velocity[5]);
        public float WorstOverlay=>Mathf.Max(Mathf.Abs(Error[0]),Mathf.Abs(Error[1]))+Mathf.Abs(Error[3])*.3f+CameraOffset+Flatness*.08f;
        public float Tilt=>Mathf.Max(Mathf.Abs(Error[4]),Mathf.Abs(Error[5]));
        public float StackStress=>Flatness*.24f+Mathf.Max(0,Temperature-.80f)*.7f+WorstVoid()*.6f+ThermalDose*.008f;
        public bool SurfaceReady=>ParticleMass<.022f&&Energy>.91f&&Flatness<.11f;
        public float RegionBond(int i)=>front[i];
        public float RegionVoid(int i)=>voids[i];

        public SparkLevel79Plant()=>NewAssembly();
        public void NewAssembly()
        {
            for(int i=0;i<16;i++){front[i]=0;voids[i]=0;}
            ParticleMass=.073f;
            Energy=.25f;
            Flatness=.084f;
            ThermalDose=0;
            NewChiplet(0);
        }
        public void NewChiplet(int chip)
        {
            for(int i=0;i<6;i++)Error[i]=((i+chip)%2==0?1:-1)*(.33f+.044f*((i+2*chip)%5));
        }
        public float Coverage(int chip)
        {
            float min=1;
            for(int i=chip*4;i<chip*4+4;i++)min=Mathf.Min(min,front[i]*(1-voids[i]));
            return min;
        }
        public float WorstVoid()
        {
            float result=0;
            foreach(float v in voids)result=Mathf.Max(result,v);
            return result;
        }
        public float Resistance(int chip)=>1+(1-Coverage(chip))*2.1f+StackStress*.46f+Mathf.Max(0,1-ThermalDose)*.34f;
        public float PathDelay(int a,int b)=>.42f+.26f*(Resistance(a)+Resistance(b))+StackStress*.52f;
        public float LinkErrors(int a,int b)=>Mathf.Max(0,PathDelay(a,b)-.97f)*.011f+WorstVoid()*.18f;

        public void Tick(float dt,Cycle cycle,Disturbance fault,int chip,bool drive)
        {
            CameraOffset=Mathf.MoveTowards(CameraOffset,fault==Disturbance.Vision?.51f:0,dt*.65f);
            ChuckResidual=Mathf.MoveTowards(ChuckResidual,fault==Disturbance.Chuck?.65f:0,dt*.84f);
            if(fault==Disturbance.Particle)ParticleMass=Mathf.MoveTowards(ParticleMass,.31f,dt*.41f);
            if(drive&&cycle==Cycle.Prep)
            {
                ParticleMass=Mathf.MoveTowards(ParticleMass,.007f,dt*.21f);
                Energy=Mathf.MoveTowards(Energy,.98f,dt*.62f);
                Flatness=Mathf.MoveTowards(Flatness,.033f,dt*.053f);
            }
            for(int axis=0;axis<6;axis++)
            {
                float target=0;
                bool move=drive&&cycle is Cycle.Home or Cycle.Transfer or Cycle.Align or Cycle.Approach or Cycle.Scan;
                if(cycle is Cycle.Transfer or Cycle.Align or Cycle.Approach or Cycle.Scan)
                {
                    target=axis switch
                    {
                        0=>-.62f+chip*.38f,
                        1=>.23f+(chip%2)*.24f,
                        2=>cycle==Cycle.Approach?-.923f:0,
                        _=>0
                    };
                    if(cycle==Cycle.Align&&axis!=2)
                    {
                        Error[axis]=Mathf.MoveTowards(Error[axis],(axis==0?CameraOffset*.5f:0),dt*(.42f+.039f*axis));
                        if(axis>=3)target=Error[axis]*.25f;
                    }
                    if(cycle==Cycle.Align&&axis==2)Error[2]=Mathf.MoveTowards(Error[2],0,dt*.50f);
                }
                float desire=move?Mathf.Clamp((target-Position[axis])*5.2f,-.61f,.61f):0;
                Velocity[axis]=Mathf.MoveTowards(Velocity[axis],desire,dt*(move?2.6f:1.12f));
                Position[axis]+=Velocity[axis]*dt;
            }
            bool apply=drive&&cycle==Cycle.Bond&&Gap is >.033f and <.14f;
            Force=Mathf.MoveTowards(Force,apply?.47f:0,dt*(apply?1.8f:1.22f));
            Imbalance=Mathf.MoveTowards(Imbalance,apply?Tilt*.68f+ChuckResidual*.44f+ParticleMass*.05f:0,dt*.67f);
            if(apply&&Force>.28f)
            {
                for(int i=chip*4;i<chip*4+4;i++)
                {
                    float mask=1-(i%4)*.036f;
                    float advance=dt*.86f*Force*Energy*mask*Mathf.Clamp01(1-WorstOverlay);
                    front[i]=Mathf.Clamp01(front[i]+advance);
                    float debris=Mathf.Max(0,ParticleMass-.016f);
                    voids[i]=Mathf.Clamp01(voids[i]+advance*(.001f+debris*.61f+Imbalance*.12f));
                }
            }
            bool heat=drive&&cycle==Cycle.Anneal;
            Temperature=Mathf.MoveTowards(Temperature,heat?.75f:.17f,dt*(heat?.49f:.38f));
            if(heat&&Temperature>.61f)ThermalDose+=dt*Temperature*.68f;
        }
    }
}

