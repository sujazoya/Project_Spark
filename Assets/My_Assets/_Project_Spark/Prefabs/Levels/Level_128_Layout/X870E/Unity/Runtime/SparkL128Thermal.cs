using System.Collections;
using UnityEngine;

namespace ProjectSpark.Gameplay.AorusWorkshop
{
    public sealed partial class SparkL128Workshop
    {
        private const float Ambient = 25f, StepSeconds = 2f, HeatCapacity = 92f;
        private const int SampleCount = 24;
        private int thermalSamples;
        private float thermalStart, cooled;
        public int ThermalSamples => thermalSamples;
        public float Temperature => temperature;
        public int FanRpm => manager && Live(fan, 12f, .046f)
            ? Mathf.RoundToInt(1200f * Mathf.Clamp(Amps(fan) / .060f, .8f, 1.2f)) : 0;

        public bool BeginThermal()
        {
            if (Stage != 5 || repaired != 3 || !running || !postPassed || checkedRails != AllRails ||
                checkedPorts != AllLinks || IsBusy || faultLatched)
                return Reject("Complete current rails, POST and all link measurements before the thermal load.");
            if (guardObserved && (!fanRestored || !freshReboot || coldBoots < 2))
                return Reject("Verify two fully isolated and independently measured boot cycles after restoring cooling.");
            if (FanRpm < 850) return Reject("Check the fan header and confirm tach feedback before applying a load.");

            thermalSamples = 0;
            thermalStart = temperature = 28f;
            peak = temperature;
            heatQualified = false;
            trace?.Clear();
            var stopTest = !guardObserved;
            trial = StartCoroutine(ThermalTrial(stopTest));
            return Note(stopTest ? "Warm the training CPU under load. Once tach is armed, open the fan path to verify the protection latch." :
                "Fresh reboot verified. Confirming sustained heat rejection with restored airflow.");
        }

        public bool SimulateFanLoss()
        {
            if (trial == null || Stage != 5 || guardObserved || thermalSamples < 8 || !running || !boost.contact.IsConducting)
                return Reject("Arm the guard under sustained thermal load before opening the fan feedback path.");
            Switch(fan.contact, false);
            RefreshElectrical();
            return Note("Fan power path opened. Watch actual tach and protected CPU power.");
        }

        private IEnumerator ThermalTrial(bool stopTest)
        {
            yield return null;
            int lostFeedback = 0;
            var fanChallengeDeadline = Time.realtimeSinceStartup + 30f;
            while (thermalSamples < SampleCount || (stopTest && Time.realtimeSinceStartup < fanChallengeDeadline))
            {
                if (thermalSamples < SampleCount) thermalSamples++;
                bool loaded = thermalSamples >= 5 && (stopTest || thermalSamples <= 17);
                Switch(boost.contact, loaded);
                RefreshElectrical();
                var power = 70f * (Mathf.Abs(rails[2].load.Power) + Mathf.Abs(boost.load.Power));
                var tach = FanRpm;
                float conductance = tach >= 850 ? 2.9f : .24f;
                temperature += StepSeconds * (power - conductance * (temperature - Ambient)) / HeatCapacity;
                peak = Mathf.Max(peak, temperature);
                RefreshTrace(temperature, tach, loaded);
                lostFeedback = tach < 850 ? lostFeedback + 1 : 0;
                if (temperature > 62f || lostFeedback >= 3)
                {
                    bool intentionalDrop = stopTest && lostFeedback >= 3 && loaded && RailOK(2) && temperature >= 28.5f;
                    faultLatched = true;
                    ShutdownMain(true);
                    var powerOff = !running && Amps(rails[2]) < .001f && Amps(boost) < .001f;
                    guardObserved = intentionalDrop && powerOff && RailOK(0);
                    EndTest(guardObserved ? "Guard latched. CPU load is off and standby remains live. Isolate all power before restoring cooling." :
                        "Guard latched on an unqualified condition. Restore the fan and repeat from a fresh POST.");
                    yield break;
                }
                yield return new WaitForSecondsRealtime(.16f);
            }

            Switch(boost.contact, false);
            RefreshElectrical();
            if (stopTest)
            {
                EndTest("Thermal window ended before a verified fan failure. Restart the challenge and remove the fan while loaded.");
                yield break;
            }

            cooled = temperature;
            heatQualified = thermalSamples == SampleCount && peak > thermalStart + 1f && peak < 62f && cooled < peak - .20f &&
                FanRpm >= 850 && RailOK(2);
            if (heatQualified && guardObserved && freshReboot && coldBoots == 2 && postPassed &&
                checkedRails == AllRails && checkedPorts == AllLinks && Award())
            {
                detail = $"Peak {peak:F1} °C · recovery {cooled:F1} °C · {FanRpm} RPM · {thermalSamples}/24 samples";
                EndTest("AORUS workshop qualified. Cooling recovered and both independently measured cold boots passed.");
            }
            else EndTest("Sustained run did not meet the heat rise, controlled cooldown and fan-feedback checks.");
        }

        public bool RestoreFan()
        {
            if (Stage != 5 || !faultLatched || standby || running)
                return Reject("Isolate standby and main power before restoring the fan.");
            fanRestored = true;
            return Note("Fan reseated. It will remain isolated until an explicit reset and fresh startup.");
        }

        public bool ResetGuard()
        {
            if (Stage != 5 || !faultLatched || !fanRestored || standby || running || HasForeignWire())
                return Reject("Restore cooling with all power isolated before acknowledging the guard.");
            faultLatched = false;
            freshReboot = false;
            coldBoots = 0;
            temperature = 28f;
            return Note("Guard reset. Requalify rails, POST and all links twice, with full isolation between starts.");
        }

        private void RecordRestart()
        {
            if (Stage == 5 && guardObserved && fanRestored && running && !faultLatched && !freshReboot &&
                checkedRails == AllRails && checkedPorts == AllLinks && postPassed)
            {
                coldBoots = Mathf.Min(2, coldBoots + 1);
                freshReboot = true;
            }
        }
    }
}
