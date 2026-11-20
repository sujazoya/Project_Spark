using System;

namespace ProjectSpark.VoltageDominion
{
    // Averaged four-phase teaching model. Time, current and voltage use SI units.
    public sealed class SparkDominionVrm
    {
        public enum Defect { None, OutputFilter, PhaseSense, Feedback }
        public enum Protection { None, UnderVoltage, OverVoltage, OverCurrent, OverTemperature }

        public const double StepSeconds = 0.00002;
        public const double Setpoint = 1.05;
        public const double Ambient = 25;
        private const double Inductance = 38e-6;
        private const double SwitchingFrequency = 160000;
        private readonly double[] phases = new double[4];
        private readonly double[] temperatures = { Ambient, Ambient, Ambient, Ambient };
        private readonly double setpoint, frequency, loadLine;
        private double capacitorVoltage, integral, reference, qualifiedTime, previousLoad;
        private double load;
        private double input;
        public SparkDominionVrm() : this(Setpoint, SwitchingFrequency, 0) { }

        internal SparkDominionVrm(double setpoint, double frequency, double loadLine)
        {
            this.setpoint = setpoint;
            this.frequency = frequency;
            this.loadLine = loadLine;
        }

        public int ConnectedPhases { get; set; } = 15;
        public double Phase2SenseOffset { get; set; }
        public double Phase4ExtraLoss { get; set; }
        public double TargetVoltage { get; private set; }
        public long Tick { get; private set; }
        public double Time => Tick * StepSeconds;
        public double Voltage { get; private set; }
        public double Ripple { get; private set; }
        public double Efficiency { get; private set; }
        public double FeedbackVoltage { get; private set; }
        public double Load => load;
        public double Capacitance => Fault == Defect.OutputFilter ? 1500e-6 : 3300e-6;
        public double Esr => Fault == Defect.OutputFilter ? .018 : .003;
        public double Temperature => Math.Max(Math.Max(temperatures[0], temperatures[1]), Math.Max(temperatures[2], temperatures[3]));
        public double CoolingThermalResistance { get; set; } = 24;
        public Protection Trip { get; private set; }
        public Defect Fault { get; set; }
        public bool PowerGood => qualifiedTime >= .10 && Trip == Protection.None;
        public double PhaseCurrent(int index) => phases[index];
        public double PhaseTemperature(int index) => temperatures[index];
        public double ControllerCurrent(int index) => phases[index] + (index == 1 ? Phase2SenseOffset :
            index == 2 && Fault == Defect.PhaseSense ? 1.0 : 0);
        public double CurrentSpread => Math.Max(Math.Max(phases[0], phases[1]), Math.Max(phases[2], phases[3]))
            - Math.Min(Math.Min(phases[0], phases[1]), Math.Min(phases[2], phases[3]));

        public void Step(double vin, bool enabled, double requestedAmps)
        {
            const double dt = StepSeconds;
            Tick++;
            input = double.IsFinite(vin) ? Math.Clamp(vin, 0, 24) : 0;
            bool requested = enabled && Trip == Protection.None;
            if (requested && (input < 9 || input > 15))
                Trip = input < 9 ? Protection.UnderVoltage : Protection.OverVoltage;
            bool driven = enabled && Trip == Protection.None;
            TargetVoltage = setpoint - loadLine * Math.Clamp(requestedAmps, 0, 30);
            reference = driven ? Math.Min(TargetVoltage, reference + 4 * dt) : 0;
            double unstable = Fault == Defect.Feedback ? .085 * Math.Sin(Time * 2 * Math.PI * 46) - .035 : 0;
            FeedbackVoltage = capacitorVoltage + unstable;
            double error = reference - FeedbackVoltage;
            integral = driven ? Math.Clamp(integral + error * dt * 32, -.10, .18) : 0;
            load = driven ? Math.Clamp(requestedAmps, 0, 30) * Math.Clamp(capacitorVoltage / .42, 0, 1) : 0;

            double average = (ControllerCurrent(0) + ControllerCurrent(1) + ControllerCurrent(2) + ControllerCurrent(3)) / 4;
            double current = 0, loss = 0;
            for (int phase = 0; phase < phases.Length; phase++)
            {
                double resistance = .031 * (1 + .004 * (temperatures[phase] - Ambient));
                bool available = (ConnectedPhases & 1 << phase) != 0;
                double duty = driven && available ? Math.Clamp(reference / input + .17 * error + integral
                    + .024 * (average - ControllerCurrent(phase)), 0, .33) : 0;
                phases[phase] = Math.Clamp(phases[phase] + dt * (duty * input - capacitorVoltage - resistance * phases[phase]) / Inductance, 0, 16);
                double phaseLoss = phases[phase] * phases[phase] * resistance +
                    (driven && available ? .095 + (phase == 3 ? Phase4ExtraLoss : 0) : 0);
                temperatures[phase] += dt * (Ambient + CoolingThermalResistance * phaseLoss - temperatures[phase]) / 1.2;
                if (available) current += phases[phase];
                loss += phaseLoss;
            }

            double capacitorCurrent = current - load - capacitorVoltage * .4;
            capacitorVoltage = Math.Clamp(capacitorVoltage + dt * capacitorCurrent / Capacitance, 0, 2);
            double loadSlew = Math.Clamp((load - previousLoad) / dt, -80000, 80000);
            previousLoad = load;
            Voltage = Math.Clamp(capacitorVoltage + capacitorCurrent * Esr - 60e-9 * loadSlew, 0, 2);
            if (!driven) Ripple = 0;
            else if (loadLine == 0)
                Ripple = Math.Max(0, (input - capacitorVoltage) * (reference / input)
                    / (Inductance * frequency) * (Esr + 1 / (8 * frequency * Capacitance)) * 4);
            else if (Tick % 32 == 0)
                Ripple = InterleavedRipple();
            double power = Voltage * load;
            Efficiency = power > 0 ? power / (power + loss) : 0;

            if (driven)
            {
                if (current > 26 || requestedAmps > 26) Trip = Protection.OverCurrent;
                else if (Voltage > 1.30) Trip = Protection.OverVoltage;
                else if (Temperature > 82) Trip = Protection.OverTemperature;
            }
            qualifiedTime = driven && Math.Abs(Voltage - TargetVoltage) < .035 && CurrentSpread < .25
                && Ripple < .014 ? qualifiedTime + dt : 0;
        }

        private double InterleavedRipple()
        {
            double duty = Math.Clamp(reference / input, .001, .99);
            double amplitude = Math.Max(0, (input - capacitorVoltage) * duty / (Inductance * frequency));
            double min = double.PositiveInfinity, max = double.NegativeInfinity;
            for (int sample = 0; sample < 32; sample++)
            {
                double sum = 0;
                for (int phase = 0; phase < 4; phase++)
                {
                    if ((ConnectedPhases & 1 << phase) == 0) continue;
                    double position = ((sample + 8 * phase) % 32) / 32.0;
                    sum += (position < duty ? position / duty : (1 - position) / (1 - duty)) - .5;
                }
                min = Math.Min(min, sum);
                max = Math.Max(max, sum);
            }
            return (max - min) * amplitude * (Esr + 1 / (8 * frequency * Capacitance));
        }

        public bool ResetProtection()
        {
            if (Voltage > .08 || Temperature > 55) return false;
            Trip = Protection.None;
            return true;
        }
    }
}
