using System;
using UnityEngine;

namespace ProjectSpark.Gameplay
{
    // Reduced-order, fictional lithography process. Stage state is in double-precision nanometres.
    public sealed class SparkLevel74Plant
    {
        public const float Step=.025f, ScanTime=.94f;
        public enum FieldState : byte { Open, Scanning, Exposed, Reject, Developed, Verified }
        public sealed class Field
        {
            public FieldState State;
            public double Dose, WorstOverlay, WorstFocus;
            public float Latent, CriticalDimension, Quality;
            public readonly double Height, X, Y, Transmission;
            public Field(int index)
            {
                X=(index%4-1.5)*19500;Y=(index/4-1.5)*16800;
                Height=230*Math.Sin(index*.73)+115*Math.Cos(index*.42);
                double pupilRadiusSquared=X*X/(41000d*41000d)+Y*Y/(39000d*39000d);
                Transmission=1-.009*pupilRadiusSquared;
            }
            public void Clear()
            {
                State=FieldState.Open;
                Dose=WorstOverlay=WorstFocus=0;
                Latent=CriticalDimension=Quality=0;
            }
        }
        private sealed class StageAxis
        {
            public double Position,Velocity;
            public void Step(double dt,double target,double feedForward,bool drive,double damping)
            {
                double acceleration=drive?(target-Position)*325+28*(feedForward-Velocity):-Velocity*damping;
                Velocity+=dt*Math.Clamp(acceleration,-9000000,9000000);
                Position+=dt*Velocity;
            }
        }
        private readonly StageAxis x=new(),y=new(),reticleX=new(),reticleY=new();
        public readonly Field[] Fields=new Field[16];
        public double X=>x.Position;
        public double Y=>y.Position;
        public double ReticleX=>reticleX.Position;
        public double ReticleY=>reticleY.Position;
        public double Velocity=>x.Velocity;
        public double ReticleVelocity=>reticleX.Velocity;
        public double Speed=>Math.Max(Math.Max(Math.Abs(x.Velocity),Math.Abs(y.Velocity)),Math.Max(Math.Abs(reticleX.Velocity)/4,Math.Abs(reticleY.Velocity)/4));
        public double MeasuredX {get;private set;}
        public double Overlay {get;private set;}
        public double Focus {get;private set;}
        public double SyncError=>Math.Max(Math.Abs(reticleX.Position/4-x.Position),Math.Abs(reticleY.Position/4-y.Position));
        public float Vibration {get;private set;}
        public float Spin {get;private set;}
        public float Flow {get;private set;}
        public float Coating {get;private set;}
        public float CoatingSpread {get;private set;}
        public float Resist {get;private set;}=8;
        public float Hotplate {get;private set;}=24;
        public float WaferTemperature {get;private set;}=24;
        public float Lens {get;private set;}=24;
        public float Lamp {get;private set;}
        public float Shutter {get;private set;}
        public float Aperture {get;private set;}=.74f;
        public float BakeEnergy {get;private set;}
        public float Developer {get;private set;}
        public float Residue {get;private set;}=1;
        public float Moisture {get;private set;}=1;
        public float PatternScore {get;private set;}
        public double DoseVariation {get;private set;}
        public int Interruptions {get;private set;}
        private double z,doseTargetOffset=5,angleOffset=13;
        private float liquid;

        public SparkLevel74Plant()
        {
            for(int i=0;i<Fields.Length;i++)Fields[i]=new Field(i);
        }
        public void NewWafer()
        {
            foreach(var f in Fields)f.Clear();
            liquid=Coating=CoatingSpread=BakeEnergy=Developer=PatternScore=0;
            Residue=Moisture=1;WaferTemperature=24;doseTargetOffset=5;angleOffset=13;DoseVariation=0;
        }
        public void ReworkCoating(){liquid=Coating=CoatingSpread=0;}
        public void Tick(float dt,bool drive,bool synchronized,bool isolation,bool shake,bool lensDrift,bool thermalControl,
            bool lamp,bool expose,float aperture, bool interferometer,bool aligned,bool mapped,int field,double travel,double commandSpeed,
            int processing,bool restricted,float time)
        {
            float vibrationTarget=!isolation||shake?26f:1.3f;
            Vibration=Mathf.Lerp(Vibration,vibrationTarget,dt/.17f);
            float heatTarget=lensDrift?38.5f:thermalControl?24.7f:lamp?26.4f:24f;
            Lens=Mathf.Lerp(Lens,heatTarget,dt/(lensDrift?1.4f:.84f));
            Lamp=Mathf.MoveTowards(Lamp,lamp?1.0f:0,dt*1.12f);
            Shutter=Mathf.MoveTowards(Shutter,expose?1:0,dt*22f);
            Aperture=Mathf.MoveTowards(Aperture,aperture,dt*.65f);
            var f=Fields[Math.Clamp(field,0,15)];
            double setX=field>=0?f.X+travel:0;
            double setY=field>=0?f.Y:0;
            double motorStageBias=shake?18*Math.Sin(time*6.1):0;
            x.Step(dt,setX+motorStageBias,commandSpeed,drive,16); y.Step(dt,setY,0,drive,16);
            double ratio=synchronized?4:3.85;
            reticleX.Step(dt,ratio*setX,ratio*commandSpeed,drive,16);reticleY.Step(dt,ratio*setY,0,drive,16);
            if(interferometer)MeasuredX=Math.Round((x.Position+.8*Math.Sin(time*19))*4)/4;
            doseTargetOffset*=aligned?Math.Exp(-dt*12):1;
            angleOffset*=aligned?Math.Exp(-dt*13):1;
            double demand=mapped?f.Height:0;
            z+=(demand-z)*dt*21;
            Focus=f.Height-z+(Lens-24.7)*27+Vibration*.50;
            Overlay=SyncError+doseTargetOffset+angleOffset+Vibration*.22+Math.Abs(Lens-24.7)*.72;

            float spinTarget=processing==1?810:processing is 4 or 5?460:processing==6?970:0;
            Spin=Mathf.MoveTowards(Spin,spinTarget,dt*1700);
            float flowTarget=processing==1?(restricted?.33f:1):0;
            Flow=Mathf.Min(Resist/dt,Mathf.MoveTowards(Flow,flowTarget,dt*3.4f));
            Resist=Mathf.Max(0,Resist-Flow*dt*.06f);
            if(processing==1)
            {
                float spread=Mathf.Clamp01(Spin/740);
                liquid+=dt*(Flow*.97f-liquid*.78f)*spread;
                Coating=350*liquid/1.042f;
                CoatingSpread=(1-spread)*.11f+Mathf.Max(0,.86f-Flow)*.14f+.011f;
            }
            float bake=processing==2?62:processing==3?68:24;
            Hotplate=Mathf.Lerp(Hotplate,bake,dt/.56f);
            WaferTemperature=Mathf.Lerp(WaferTemperature,Hotplate,dt/.43f);
            if(processing==3)BakeEnergy+=dt*Mathf.Clamp01((WaferTemperature-45)/16);
            if(processing==4)Developer=Mathf.MoveTowards(Developer,1,dt*.41f);
            if(processing==5)Residue=Mathf.MoveTowards(Residue,0,dt*.94f);
            if(processing==6)Moisture=Mathf.MoveTowards(Moisture,0,dt*Mathf.Clamp01(Spin/855)*1.05f);
        }

        public void StartField(int index)
        {
            var f=Fields[index];f.Clear();f.State=FieldState.Scanning;
        }
        public void IntegrateField(int index, float dt)
        {
            var f=Fields[index];
            double local=Math.Max(0,1-Math.Abs(Focus)/2150)*Math.Max(0,1-Math.Abs(Aperture-.74f)*.42);
            f.Dose+=dt/ScanTime*Lamp*Shutter*local*f.Transmission;
            f.WorstFocus=Math.Max(f.WorstFocus,Math.Abs(Focus));
            f.WorstOverlay=Math.Max(f.WorstOverlay,Overlay);
        }
        public bool EndField(int index)
        {
            var f=Fields[index];
            bool acceptable=f.Dose is >.91 and <1.045&&f.WorstOverlay<=12&&f.WorstFocus<=100;
            f.State=acceptable?FieldState.Exposed:FieldState.Reject;
            if(acceptable)f.Latent=(float)(1-Math.Exp(-f.Dose*1.85));
            return acceptable;
        }
        public void Interrupt(int index)
        {
            if(index<0||Fields[index].State!=FieldState.Scanning)return;
            Fields[index].State=FieldState.Reject;
            Interruptions++;
        }
        public int CountExposed()
        {
            int result=0;
            foreach(var f in Fields)if(f.State is FieldState.Exposed or FieldState.Developed or FieldState.Verified)result++;
            return result;
        }
        public bool Inspect()
        {
            if(CountExposed()!=16||BakeEnergy<1.3f||Developer<.96f||Residue>.04f||Moisture>.06f)return false;
            float quality=0;
            double total=0,min=2,max=0;
            foreach(var f in Fields)
            {
                total+=f.Dose;min=Math.Min(min,f.Dose);max=Math.Max(max,f.Dose);
                float contrast=(f.Latent-.833f)*(1+.035f*(BakeEnergy-1.45f));
                f.CriticalDimension=120+31*contrast+.037f*(Coating-350)+(float)(f.WorstFocus*.009);
                f.Quality=1-.0011f*Mathf.Abs(f.CriticalDimension-120)-(float)(.00055*f.WorstOverlay+.00006*f.WorstFocus)-.031f*CoatingSpread;
                quality+=f.Quality;
                if(Mathf.Abs(f.CriticalDimension-120)>8||f.WorstOverlay>12||f.WorstFocus>100)return false;
            }
            DoseVariation=(max-min)/(total/16);
            PatternScore=quality/16;
            if(DoseVariation>.02||PatternScore<.97f)return false;
            foreach(var f in Fields)f.State=FieldState.Verified;
            return true;
        }
    }
}
