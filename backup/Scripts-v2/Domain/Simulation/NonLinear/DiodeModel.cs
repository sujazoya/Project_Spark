using System;

namespace ProjectSpark.Domain.Simulation.NonLinear
{
    public abstract class DiodeModel
        : DeviceModel
    {
        public double SaturationCurrent =
            1e-12;

        public double ThermalVoltage =
            0.02585;

        public override void Stamp(
            NonLinearContext context)
        {
            // Shockley equation

            // I = Is *
            // (exp(Vd/Vt)-1)
        }

        public double Current(
            double voltage)
        {
            return SaturationCurrent *
                (Math.Exp(
                    voltage /
                    ThermalVoltage)
                - 1.0);
        }
    }
}
