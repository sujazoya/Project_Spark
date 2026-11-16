using UnityEngine;

namespace ProjectSpark.Gameplay
{
    // Normalized game model. Spectral path power adds conservatively; the model does not solve Maxwell's equations.
    public sealed class SparkLevel82Plant
    {
        public const float Step=.025f;
        public const int Angles=12,Heights=3,Polarizations=2,Poses=Angles*Heights*Polarizations,Bins=16;
        public const float ConductedLimit=.345f,RadiatedGuardedLimit=.27f;
        public enum Mode { Idle, MotionCal, ReceiverCal, FieldCal, Conducted, Radiated, Near, Exposure }
        public float Door {get;private set;}=1;
        public float Azimuth {get;private set;}
        public float Height {get;private set;}=.18f;
        public float Polarization {get;private set;}
        public float Distance {get;private set;}=.52f;
        public float ProbeX {get;private set;}
        public float ProbeY {get;private set;}
        public float ProbeZ {get;private set;}=.17f;
        public float Harness {get;private set;}
        public float Gasket {get;private set;}
        public float PowerFilter {get;private set;}
        public float SensorFilter {get;private set;}
        public float Susceptibility {get;private set;}
        public float Amp {get;private set;}
        public float Shutter {get;private set;}=1;
        public float FieldProbe(float stimulus)=>stimulus*(.966f+Thermal*.042f)+.006f;
        public float Field=>Amp*(1-Shutter)*(.96f+.04f*Height);
        public float Thermal {get;private set;}
        public float Load {get;private set;}
        public float AdcError {get;private set;}
        public float Jitter {get;private set;}
        public float OpticalLoss {get;private set;}
        public float SensorLatency=>.05f+.21f*SensorFilter;
        public bool Closed=>Door<.012f;
        public bool RfSafe=>Shutter>.988f&&Amp<.026f&&Field<.008f;
        public bool FunctionValid=>Load>.65f&&Thermal<.72f&&AdcError<.128f&&Jitter<.106f&&OpticalLoss<.094f&&SensorLatency<.32f;
        public int BlockedUnsafe {get;private set;}
        public int ValidCycles {get;private set;}

        private float CommonSource=>Load*(.85f-.724f*Harness);
        private float SeamSource=>Load*(.75f-.665f*Gasket);
        private float Converter=>Load*(.66f-.47f*PowerFilter);
        private float Clock=>Load*.27f;
        private float differential=>Converter*(.55f-.14f*PowerFilter);

        public void Tick(float dt,float time,bool powered,bool doorOpen,float angle,float height,float polarization,float distance,
            float x,float y,float z,bool route,bool gasket,bool filter,bool sensor,bool desense,bool rf,float demand,int band)
        {
            Door=Mathf.MoveTowards(Door,doorOpen?1:0,dt*.97f);
            Azimuth=Mathf.MoveTowards(Azimuth,angle,dt*435f);
            Height=Mathf.MoveTowards(Height,height,dt*1.73f);
            Polarization=Mathf.MoveTowards(Polarization,polarization,dt*228f);
            Distance=Mathf.MoveTowards(Distance,distance,dt*.88f);
            ProbeX=Mathf.MoveTowards(ProbeX,x,dt*2.18f);
            ProbeY=Mathf.MoveTowards(ProbeY,y,dt*2.02f);
            ProbeZ=Mathf.MoveTowards(ProbeZ,z,dt*1.48f);
            Harness=Mathf.MoveTowards(Harness,route?1:0,dt*.98f);
            Gasket=Mathf.MoveTowards(Gasket,gasket?1:0,dt*.86f);
            PowerFilter=Mathf.MoveTowards(PowerFilter,filter?1:0,dt*.90f);
            SensorFilter=Mathf.MoveTowards(SensorFilter,sensor?1:0,dt*.92f);
            Susceptibility=Mathf.MoveTowards(Susceptibility,desense?1:0,dt*.98f);
            Load=Mathf.MoveTowards(Load,powered?.82f+.037f*Mathf.Sin(time*7.2f):0,dt*1.8f);
            Thermal=Mathf.MoveTowards(Thermal,.21f+Load*(.22f+.08f*Gasket+.075f*PowerFilter)+Field*.078f,dt*.15f);
            bool allow=rf&&Closed;
            Amp=Mathf.MoveTowards(Amp,allow?demand:0,dt*(allow?1.75f:.83f));
            Shutter=Mathf.MoveTowards(Shutter,allow?0:1,dt*(allow?2.7f:3.15f));
            float response=.73f+band*.14f;
            float ac=Field*response;
            AdcError=.033f*Converter+ac*(.33f+.70f*Susceptibility)/(1+SensorFilter*4.2f);
            Jitter=.039f*Clock+.029f*CommonSource+ac*(.073f+.05f*Susceptibility);
            OpticalLoss=.061f*CommonSource+ac*(.061f+.035f*Susceptibility);
        }

        private static float Lobe(float bin,float center,float width)
        {
            float offset=(bin-center)/width;
            return Mathf.Exp(-offset*offset);
        }
        public float Conducted(int bin)
        {
            float dm=differential*(.35f+.65f*Lobe(bin,3,2.0f)+.36f*Lobe(bin,7,1.62f));
            float cm=CommonSource*(.20f+.69f*Lobe(bin,6,2.2f)+.17f*Lobe(bin,11,1.45f));
            return Mathf.Sqrt(dm*dm+cm*cm)+.014f;
        }
        public float Radiated(int bin)
        {
            const float radians=.017453293f;
            float a=Azimuth*radians,p=Polarization*radians;
            float cable=CommonSource*(.25f+.75f*Lobe(bin,6,2.25f))*(.29f+.71f*Mathf.Abs(Mathf.Sin(a+.38f+p)));
            float seam=SeamSource*(.23f+.77f*Lobe(bin,11,2.03f))*(.32f+.68f*Mathf.Abs(Mathf.Cos(a-.70f-p)));
            float direct=Clock*(.20f+.70f*Lobe(bin,13,1.86f))*(.38f+.30f*Height);
            float nearFactor=(1-.22f*Distance)*(.84f+.155f*Height);
            return Mathf.Sqrt(cable*cable+seam*seam+direct*direct)*nearFactor+.013f;
        }
        private static float Spot(float x,float y,float u,float v)
        {
            float dx=x-u,dy=y-v;
            return Mathf.Exp(-(dx*dx+dy*dy)*19);
        }
        public (float h,float e) NearField()
        {
            float cable=CommonSource*Spot(ProbeX,ProbeY,.14f,.35f);
            float seam=SeamSource*Spot(ProbeX,ProbeY,.84f,.65f);
            float converter=Converter*Spot(ProbeX,ProbeY,.37f,.19f);
            float rolloff=1/(1+.80f*ProbeZ);
            return ((cable+.16f*seam+.39f*converter)*rolloff,(.21f*cable+seam+.29f*converter)*rolloff);
        }
        public void SampleWorkload()
        {
            if(FunctionValid)ValidCycles++;
            else BlockedUnsafe++;
        }
    }
}

