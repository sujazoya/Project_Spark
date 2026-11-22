using System.Collections;
using System.Linq;
using UnityEngine;

namespace ProjectSpark.Gameplay.RecoveryLab
{
    public sealed partial class SparkL130Workshop
    {
        private const float Ambient = 24f, StepSeconds = 2f, HeatCapacity = 88f;
        private const int ThermalFrames = 28;
        private int thermalSamples;
        private float cooled;
        public int ThermalSamples => thermalSamples;
        public int ExpectedThermalSamples => ThermalFrames;
        public float Temperature => temperature;
        public bool ThermalPassed => thermalPassed;
        public int MotorRpm => manager && Live(fanMotor, 12f, .044f)
            ? Mathf.RoundToInt(1200f * Mathf.Clamp(Amps(fanMotor) / .060f, .78f, 1.22f)) : 0;
        public int TachRpm => manager && MotorRpm >= 900 && Live(fanTach, 12f, .020f)
            ? Mathf.RoundToInt(1200f * Mathf.Clamp(Amps(fanTach) / .030f, .78f, 1.22f)) : 0;

        public bool BeginThermal()
        {
            bool challenge = Stage == 4 && activeFault == 2 && !candidateRepair;
            if (Stage is < 4 or > 5 || !running || !postPassed || IsBusy || faultLatched ||
                checkedRails != AllRails || checkedPorts != AllDevices || boot < minimumBoot)
                return Reject("Thermal testing requires the current boot's loaded power, POST and five device proofs.");
            if (Stage == 4 && activeFault != 2) return Reject("Open the cooling case before loading the CPU.");
            if (challenge && (!BeforeIsCurrent() || MotorRpm < 900 || TachRpm != 0))
                return Reject("Measure the fan motor and missing tach at the selected site before the guard challenge.");
            if (!challenge && TachRpm < 900) return Reject("Cooling feedback is missing. Inspect the tach path before applying load.");

            thermalPassed = false;
            thermalSamples = 0;
            peak = temperature = 27f;
            trace.Clear();
            trial = StartCoroutine(TemperatureRun(challenge));
            return Note(challenge ? "Guard armed. A brief native load will verify shutdown with the fan feedback circuit open." :
                "Two load pulses and a cooldown are running. Verify that the temperature and tach traces remain in range.");
        }

        private IEnumerator TemperatureRun(bool challenge)
        {
            yield return null;
            int missing = 0;
            while (thermalSamples < ThermalFrames)
            {
                thermalSamples++;
                bool loaded = challenge || thermalSamples is >= 4 and <= 11 or >= 15 and <= 22;
                Switch(boost.contact, loaded);
                RefreshElectrical();
                int rpm = TachRpm;
                int motion = MotorRpm;
                float watts = 76f * (Mathf.Abs(rails[3].load.Power) + Mathf.Abs(boost.load.Power));
                float cooling = motion >= 900 ? 3.15f : .28f;
                temperature += StepSeconds * (watts - cooling * (temperature - Ambient)) / HeatCapacity;
                peak = Mathf.Max(peak, temperature);
                RefreshTrace(temperature, rpm, loaded);

                missing = rpm < 900 ? missing + 1 : 0;
                if (missing == 3 || temperature > 62f || !RailOK(3))
                {
                    bool expectedFault = challenge && missing == 3 && motion >= 900 && Amps(boost) > .55f;
                    faultLatched = true;
                    ShutdownMain(true);
                    guardObserved = expectedFault && !running && Amps(rails[3]) < .001f &&
                        Amps(boost) < .001f && RailOK(0);
                    if (guardObserved)
                        Record("F3", "CPU_FAN1", "Three missing tach samples with the motor running; VCORE and soak current removed while standby stayed live", true);
                    EndTest(guardObserved ? "Guard verified. The motor was powered before the trip. Standby is still live: isolate it before repair." :
                        "Thermal guard latched outside the expected test conditions. Isolate and check cooling.");
                    yield break;
                }
                yield return new WaitForSecondsRealtime(.145f);
            }
            Switch(boost.contact, false);
            RefreshElectrical();
            cooled = temperature;
            thermalPassed = !challenge && thermalSamples == ThermalFrames && peak > 28.5f && peak < 62f &&
                cooled < peak - .2f && TachRpm >= 900 && RailOK(3);
            detail = $"Peak {peak:F1} °C · recovery {cooled:F1} °C · tach {TachRpm} RPM · {thermalSamples}/{ThermalFrames} samples";
            EndTest(thermalPassed ? Stage == 5 ? "Current cold-start thermal evidence verified. Sign the final audit." :
                "Tach repair withstood both loads. Verify the repair against the original fault evidence." :
                "The run did not demonstrate a managed rise and cooldown with valid tach feedback.");
        }

        public bool ResetProtection()
        {
            if (!faultLatched || standby || running || IsBusy || HasForeignWire() ||
                activeFault == 2 && !candidateRepair)
                return Reject("Finish the isolated tach repair before acknowledging the thermal guard.");
            faultLatched = false;
            thermalPassed = false;
            return Note("Guard reset. Apply standby, measure all six loaded points, then repeat POST, devices and thermal testing.");
        }

        public bool SignAudit()
        {
            if (Stage != 5 || repairedMask != 7 || boot < minimumBoot || !running || !postPassed || !thermalPassed ||
                faultLatched || checkedRails != AllRails || checkedPorts != AllDevices || IsBusy ||
                !Enumerable.Range(0, rails.Length).All(RailOK) || TachRpm < 900)
                return Reject("The final isolated restart must supply fresh power, POST, device and thermal evidence on one boot.");
            Record("O6", "Cold restart", $"All current evidence verified on boot {boot}; peak {peak:F1} °C; cooldown {cooled:F1} °C", true);
            if (!Award()) return false;
            return Note("Recovery audit accepted. All six objectives passed on the current isolated restart.");
        }
    }
}
