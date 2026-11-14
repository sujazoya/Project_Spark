using UnityEngine;

namespace ProjectSpark.Gameplay
{
    // Educational averaged converter, SI units. This model owns the flywheel and
    // bank states. It is not a switching-frequency or electrochemical model.
    internal sealed class SparkLevel56Energy
    {
        public const float Inertia=.105f, Capacitance=.8f, MaxVolts=4.8f, Winding=1.15f, Ke=.405f;
        public const float Esr=.075f, BrakeResistance=12f;
        public float Omega { get; private set; }
        public float Angle { get; private set; }
        public float Stored { get; private set; }
        public float Temperature { get; private set; }=24;
        public float Current { get; private set; }
        public float Voltage { get; private set; }
        public float CapacitorCurrent { get; private set; }
        public float BusVoltage { get; private set; }
        public float BrakePower { get; private set; }
        public float SourceWork { get; private set; }
        public float StorageDraw { get; private set; }
        public float GeneratorWork { get; private set; }
        public float Heat { get; private set; }
        public float Loss { get; private set; }
        public float Dumped { get; private set; }
        public float TerminalVolts => Mathf.Sqrt(2*Stored/Capacitance);
        public float Kinetic => .5f*Inertia*Omega*Omega;
        public float Rpm => Omega*30/Mathf.PI;

        public void Step(float dt,float requested,float solvedRail,bool motoring,bool reuse,bool regen,bool dump,bool discharge)
        {
            float oldKinetic=Kinetic;
            float oldStored=Stored;
            float frictionTorque=.008f+.0015f*Omega;
            float emf=Ke*Omega;
            CapacitorCurrent=BrakePower=0;
            BusVoltage=solvedRail;
            Current=0;
            if (motoring)
                Current=Mathf.Min(requested,Mathf.Max(0,(solvedRail-emf)/Winding));
            else if ((regen || dump) && Omega>.02f)
            {
                float upper=Mathf.Min(requested,emf/(2*Winding));
                float capLimit=regen ? Mathf.Min(MaxVolts*Capacitance*.95f,Mathf.Max(0,(.5f*Capacitance*MaxVolts*MaxVolts-Stored)/dt)) : 0;
                float resistorLimit=dump && Temperature<78 ? 4.7f*4.7f/BrakeResistance : 0;
                float accepted=(capLimit+resistorLimit)/.91f;
                // Use the lower current root of P=I(E-RI). Reduce braking torque
                // when the available sink cannot accept the generated power.
                float acceptableCurrent=accepted>=emf*emf/(4*Winding) ? upper :
                    (emf-Mathf.Sqrt(Mathf.Max(0,emf*emf-4*Winding*accepted)))/(2*Winding);
                Current=-Mathf.Min(upper,acceptableCurrent);
            }

            float torque=Ke*Current;
            Omega=Mathf.Max(0,Omega+(torque-frictionTorque)*dt/Inertia);
            Angle+=Omega*dt;
            float mechanicalDelta=Kinetic-oldKinetic;
            float mechanicalWork=Ke*Current*(Kinetic>0?(Omega+emf/Ke)*.5f:0)*dt;
            float friction=Mathf.Max(0,mechanicalWork-mechanicalDelta);
            float copper=Current*Current*Winding*dt;
            Heat+=copper;
            Voltage=emf+Current*Winding;

            if (Current>0)
            {
                float needed=(mechanicalWork+copper)/.91f;
                float consumed=reuse && TerminalVolts>.45f ? Mathf.Min(needed*.67f,Mathf.Min(Stored,TerminalVolts*.92f*dt)) : 0;
                float esrLoss=consumed>0 ? Mathf.Pow(consumed/dt/TerminalVolts,2)*Esr*dt : 0;
                Stored-=consumed;
                StorageDraw+=consumed;
                SourceWork+=needed-consumed+esrLoss;
                Loss+=friction+needed-mechanicalWork-copper+esrLoss;
            }
            else if (Current<0)
            {
                float generated=Mathf.Max(0,-mechanicalWork-copper);
                GeneratorWork+=generated;
                float available=generated*.91f;
                float capacity=regen ? Mathf.Max(0,.5f*Capacitance*MaxVolts*MaxVolts-Stored) : 0;
                // At low voltage the current limit binds before the energy limit.
                float capV=TerminalVolts;
                float impedance=Esr+dt/(2*Capacitance);
                float permitted=Mathf.Min(.95f, Mathf.Max(0,(MaxVolts-capV)*Capacitance/dt));
                float power=available/dt;
                float amps=power>0?Mathf.Min(permitted,2*power/(capV+Mathf.Sqrt(capV*capV+4*impedance*power))):0;
                float esrLoss=amps*amps*Esr*dt;
                float gained=Mathf.Min(capacity,(capV+amps*dt/(2*Capacitance))*amps*dt);
                if (!regen) {gained=0; esrLoss=0;}
                Stored+=gained;
                float waste=Mathf.Max(0,available-gained-esrLoss);
                float sink=dump && Temperature<78 ? Mathf.Min(waste,4.7f*4.7f/BrakeResistance*dt) : 0;
                BrakePower=sink/dt;
                Dumped+=sink;
                // The converter's internal snubber absorbs a capped transient;
                // all other non-recovered energy is included in loss accounting.
                Loss+=friction+generated-available+esrLoss+waste-sink;
                BusVoltage=dump && BrakePower>0 ? Mathf.Min(5.25f,Mathf.Max(solvedRail,TerminalVolts+.25f)) : solvedRail;
            }
            else
            {
                Loss+=friction;
            }

            if (discharge)
            {
                float discharged=Mathf.Min(Stored,Mathf.Min(TerminalVolts*.85f,1.45f)*dt);
                Stored-=discharged;
                Dumped+=discharged;
                BrakePower+=discharged/dt;
            }
            Temperature+=(BrakePower-(Temperature-24)*.18f)*dt/2.6f;
            CapacitorCurrent=(TerminalVolts-Mathf.Sqrt(2*oldStored/Capacitance))*Capacitance/dt;
        }
    }
}

