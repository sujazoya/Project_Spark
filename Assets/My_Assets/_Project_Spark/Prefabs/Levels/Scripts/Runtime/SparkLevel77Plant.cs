using UnityEngine;

namespace ProjectSpark.Gameplay
{
    // Fictional reduced-order finishing model for a previously plated damascene wafer.
    public sealed class SparkLevel77Plant
    {
        public const float Step=.025f;
        public readonly float[] Copper=new float[16], Barrier=new float[16], Dishing=new float[16], Erosion=new float[16];
        public readonly float[] Rpm=new float[3], Carrier=new float[3], Force=new float[3], Flow=new float[3], Wear=new float[3], Lift={1,1,1};
        public readonly float[] Reservoir={1,1,1}, Pump=new float[3];
        public float Viewport {get;private set;}=1;
        public float Selectivity {get;private set;}=1;
        public float OpticalCopper {get;private set;}
        public float Eddy {get;private set;}
        public float SpectralA {get;private set;}
        public float SpectralB {get;private set;}
        public float Confidence=>Mathf.Clamp01((SpectralA+SpectralB)*Viewport*.96f);
        public float Corrosion {get;private set;}
        public float Particles {get;private set;}=1;
        public float Moisture {get;private set;}=1;
        public float Residue {get;private set;}=1;
        public float Dryer {get;private set;}
        public float TotalRemoved {get;private set;}
        public float RemovedRate {get;private set;}
        public float Heat {get;private set;}=23;
        public float MeanCopper=>Mean(Copper);
        public float MeanBarrier=>Mean(Barrier);
        public float WorstCopper=>Max(Copper);
        public float WorstBarrier=>Max(Barrier);
        public float WorstDishing=>Max(Dishing);
        public float WorstErosion=>Max(Erosion);
        public float Uniformity
        {
            get
            {
                float min=float.PositiveInfinity,max=0;
                for(int i=0;i<16;i++){float z=InitialCu(i)-Copper[i];min=Mathf.Min(min,z);max=Mathf.Max(max,z);}
                return (max-min)/Mathf.Max(1,118-MeanCopper);
            }
        }
        public float ViaResistance=>1+WorstDishing*.007f+Corrosion*.14f;
        public float Leakage=>Mathf.Clamp01(WorstCopper*.13f+WorstBarrier*.18f+Corrosion*.09f+Residue*.03f);
        public float DielectricIntegrity=>Mathf.Clamp01(1-WorstErosion*.019f-Corrosion*.12f);
        private static float InitialCu(int i)=>118+(i%4-1.5f)*.54f+(i/4-1.5f)*.22f;
        private static float Mean(float[] values){float v=0;foreach(float x in values)v+=x;return v/values.Length;}
        private static float Max(float[] values){float v=0;foreach(float x in values)v=Mathf.Max(v,x);return v;}

        public SparkLevel77Plant()=>NewWafer();
        public void NewWafer()
        {
            for(int i=0;i<16;i++){Copper[i]=InitialCu(i);Barrier[i]=11.6f+(i%4)*.17f;Dishing[i]=Erosion[i]=0;}
            TotalRemoved=RemovedRate=Corrosion=0;
            Particles=Moisture=Residue=1;
            Eddy=MeanCopper;OpticalCopper=MeanCopper/(MeanCopper+32);SpectralA=.94f;SpectralB=.20f;
        }
        public void Tick(float dt,int station,bool process,bool drive,bool fluids,bool conditioning,bool fouling,bool drift,bool corrosionFault,
            bool protect,int cleaning,float time)
        {
            float platen=0,carrier=0,force=0,flow=0;
            for(int i=0;i<3;i++)
            {
                bool working=i==station&&drive;
                Rpm[i]=Mathf.MoveTowards(Rpm[i],working?74-i*9:0,dt*88);
                Carrier[i]=Mathf.MoveTowards(Carrier[i],working?56-i*6:0,dt*96);
                float command=i==station&&process&&Rpm[i]>45&&Carrier[i]>38?1f-i*.19f:0;
                Force[i]=Mathf.MoveTowards(Force[i],command,dt*(command>0?3.40f:1.55f));
                Lift[i]=Mathf.MoveTowards(Lift[i],working?0:1,dt*2.35f);
                Pump[i]=Mathf.MoveTowards(Pump[i],fluids?1:0,dt*2.4f);
                Reservoir[i]=Mathf.Clamp01(Reservoir[i]+dt*(working?-Pump[i]*.052f:fluids?.23f:0));
                float available=Mathf.Clamp01(Reservoir[i]*5);
                Flow[i]=Mathf.MoveTowards(Flow[i],1.04f*Pump[i]*available,dt*1.6f);
                Wear[i]=Mathf.Clamp01(Wear[i]+dt*(conditioning?-.26f:Force[i]*.0021f));
                if(i==station){platen=Rpm[i];carrier=Carrier[i];force=Force[i];flow=Flow[i];}
            }
            Viewport=Mathf.MoveTowards(Viewport,fouling?.42f:.995f,dt*.71f);
            Selectivity=Mathf.MoveTowards(Selectivity,drift?.37f:1,dt*.71f);
            Heat+=dt*(force*platen*.018f-(.095f+flow*.23f)*(Heat-23));
            float rate=0;
            if(station>=0&&process&&force>.01f)
            {
                float v=Mathf.Sqrt(platen*platen*.00011f+carrier*carrier*.000047f);
                float work=force*v*Mathf.Clamp01(flow)*(1-Wear[station]*.46f);
                float nominal=station==0?24:station==1?16:14f;
                for(int i=0;i<16;i++)
                {
                    float edge=(Mathf.Abs(i%4-1.5f)+Mathf.Abs(i/4-1.5f))/3-.6667f;
                    float budget=dt*work*nominal*(1+edge*.009f);
                    float oldCu=Copper[i],oldBarrier=Barrier[i];
                    if(station<2)
                    {
                        float cu=Mathf.Min(oldCu,budget);
                        Copper[i]-=cu;
                        Dishing[i]+=(budget-cu)*.071f+cu*.005f;
                        Erosion[i]+=(budget-cu)*.058f;
                    }
                    else
                    {
                        Copper[i]=Mathf.Max(0,oldCu-budget*.23f);
                        Barrier[i]=Mathf.Max(0,oldBarrier-budget*Selectivity);
                        Dishing[i]+=budget*(.039f+.20f*(1-Selectivity));
                        Erosion[i]+=budget*(.025f+.23f*(1-Selectivity));
                        if(oldBarrier<.35f)Erosion[i]+=budget*.12f;
                    }
                    rate+=(oldCu-Copper[i])+(oldBarrier-Barrier[i]);
                }
                TotalRemoved+=rate/16;
            }
            RemovedRate=Mathf.Lerp(RemovedRate,rate/16/dt,dt/.14f);
            Eddy=Mathf.Lerp(Eddy,MeanCopper+.045f*Mathf.Sin(time*11f),dt/.12f);
            OpticalCopper=Mathf.Lerp(OpticalCopper,MeanCopper/(MeanCopper+32),dt/.13f);
            float feature=MeanBarrier/(MeanBarrier+2.6f);
            SpectralA=Mathf.Lerp(SpectralA,Viewport*(.27f+.78f*feature),dt/.11f);
            SpectralB=Mathf.Lerp(SpectralB,Viewport*(.76f-.69f*feature),dt/.11f);
            if(cleaning==1)Particles=Mathf.MoveTowards(Particles,.24f,dt*.85f);
            if(cleaning==2)Particles=Mathf.MoveTowards(Particles,.014f,dt*.64f);
            if(cleaning==3)Residue=Mathf.MoveTowards(Residue,0,dt*.98f);
            Dryer=Mathf.MoveTowards(Dryer,cleaning==4?490:0,dt*1120);
            if(cleaning==4)Moisture=Mathf.MoveTowards(Moisture,0,dt*Dryer/510);
            if(corrosionFault)Corrosion=Mathf.MoveTowards(Corrosion,.75f,dt*(.21f+.17f*Moisture));
            else if(protect&&Corrosion<.08f)Corrosion=Mathf.MoveTowards(Corrosion,.004f,dt*.10f);
        }
    }
}

