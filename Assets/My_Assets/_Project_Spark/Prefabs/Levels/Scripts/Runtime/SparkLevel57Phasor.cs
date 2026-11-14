using System.Numerics;
using UnityEngine;

namespace ProjectSpark.Gameplay
{
    internal sealed class SparkLevel57Phasor
    {
        public const float Nominal=2.4f;
        private static readonly Complex Impedance=new(.20,.16);
        public float FrequencyA=50,FrequencyB=49.35f, VoltageA=Nominal,VoltageB=2.18f, AngleA,AngleB=42;
        public float BusVolts,BusHz,CurrentA,CurrentB,WattsA,WattsB,Vars,LoadWatts,LossWatts,Residual,Circulating,MotorRpm;
        public float Delta => Mathf.DeltaAngle(AngleA,AngleB);
        public float Mismatch => Mathf.Abs(VoltageA-VoltageB)/Nominal;
        public bool InWindow => Mathf.Abs(FrequencyA-FrequencyB)<=.2f && Mathf.Abs(Delta)<=5 && Mismatch<=.05f;
        public float Share => WattsA+WattsB>.01f ? WattsA/(WattsA+WattsB) : 0;
        public float PowerFactor => LoadWatts>.001f ? LoadWatts/Mathf.Sqrt(LoadWatts*LoadWatts+Vars*Vars) : 0;

        public void Step(float dt,bool a,bool b,bool island,bool sync,bool matchHz,bool matchV,
            bool autoShare,float trim,float load,bool motor)
        {
            FrequencyA=Mathf.MoveTowards(FrequencyA,50,dt);
            float bTarget=island?50-.016f*LoadWatts: b&&a?FrequencyA:
                sync?FrequencyA-Mathf.Clamp(Delta*.025f,-1.1f,1.1f):matchHz?50:49.35f;
            FrequencyB=Mathf.MoveTowards(FrequencyB,bTarget,dt*2.8f);
            VoltageA=Mathf.MoveTowards(VoltageA,Nominal,dt*.6f);
            float goalV=island?Nominal: matchV||sync||b&&a?Nominal:2.18f;
            VoltageB=Mathf.MoveTowards(VoltageB,goalV,dt*.6f);
            AngleA=Mathf.Repeat(AngleA+360*FrequencyA*dt,360);
            AngleB=Mathf.Repeat(AngleB+360*FrequencyB*dt,360);
            if (a&&b)
            {
                float target=autoShare?0:trim;
                AngleB=Mathf.Repeat(Mathf.MoveTowardsAngle(AngleB,AngleA+target,dt*25),360);
            }

            Complex sourceA=Complex.FromPolarCoordinates(VoltageA,AngleA*Mathf.Deg2Rad);
            Complex sourceB=Complex.FromPolarCoordinates(VoltageB,AngleB*Mathf.Deg2Rad);
            // Per-phase solution for a balanced three-phase training network.
            // Phase B and C are rotations of this solution by -120° and +120°.
            Complex admittance=new(load/18.0+(motor ? .021 : 0),-(motor ? .013 : .003));
            int sources=(a?1:0)+(b?1:0);
            var v=sources==0?Complex.Zero:((a?sourceA:Complex.Zero)+(b?sourceB:Complex.Zero))/
                (sources+Impedance*admittance);
            var ia=a?(sourceA-v)/Impedance:Complex.Zero;
            var ib=b?(sourceB-v)/Impedance:Complex.Zero;
            var loadPower=3*v*Complex.Conjugate(v*admittance);
            BusVolts=(float)v.Magnitude;
            BusHz=a?FrequencyA:b?FrequencyB:0;
            CurrentA=(float)ia.Magnitude;
            CurrentB=(float)ib.Magnitude;
            WattsA=(float)(3*sourceA*Complex.Conjugate(ia)).Real;
            WattsB=(float)(3*sourceB*Complex.Conjugate(ib)).Real;
            Vars=(float)loadPower.Imaginary;
            LoadWatts=(float)loadPower.Real;
            LossWatts=(CurrentA*CurrentA+CurrentB*CurrentB)*3*(float)Impedance.Real;
            Residual=WattsA+WattsB-LoadWatts-LossWatts;
            Circulating=a&&b?(float)((sourceA-sourceB)/(2*Impedance)).Magnitude:0;
            MotorRpm=Mathf.MoveTowards(MotorRpm,motor&&BusVolts>1?BusHz*30*(1-.055f*load):0,dt*410);
        }
    }
}

