using UnityEngine;

namespace ProjectSpark.Gameplay.ThermalAegis
{
    internal sealed class SparkLevel127Thermal
    {
        internal const int FrameCount = 64, FirstHeat = 12, FirstBackup = 32, FirstRecovery = 48;
        internal const float MinTemperature = 15f, MaxTemperature = 65f;
        internal const float StepSeconds = 2f;
        private const float CoreCapacity = 120f, LoopCapacity = 95f, SinkTemperature = 10f;
        private const float InitialCore = 18.4f, InitialLoop = 17.4f;
        private const float MaximumSensorAge = 1.5f;

        internal enum Phase { Cold, Heat, Backup, Recovery }

        internal readonly struct Inputs
        {
            internal readonly float PumpA, PumpB, CoreWatts, HeaterWatts, RadiatorCurrent, ReferenceGain;
            internal readonly int SensorCount;

            internal Inputs(float pumpA, float pumpB, float coreWatts, float heaterWatts, float radiatorCurrent,
                float referenceGain, int sensorCount)
            {
                PumpA = pumpA; PumpB = pumpB; CoreWatts = coreWatts; HeaterWatts = heaterWatts;
                RadiatorCurrent = radiatorCurrent; ReferenceGain = referenceGain; SensorCount = sensorCount;
            }
        }

        internal struct Sample
        {
            internal Phase Segment;
            internal float Core, Coolant, Flow, SensorAge, Confidence;
            internal int Loop;
        }

        internal sealed class Replay
        {
            internal int Sent, Good, HeaterSamples, BackupSamples, RecoveredSamples, PumpLoss, Drift, Stale;
            internal float Core = InitialCore, Coolant = InitialLoop, Minimum = InitialCore, Peak = InitialCore;
            internal float HeatJ, SinkJ, MaxDisagreement, OutletAge, PeakAge;
            internal bool Invalid, HeatRequested;
            internal Sample Last;
            internal float Residual => Mathf.Abs(HeatJ - SinkJ - CoreCapacity * (Core - InitialCore) -
                LoopCapacity * (Coolant - InitialLoop));
            internal bool Complete => Sent >= FrameCount;
            internal bool Qualified => Complete && !Invalid && Good == FrameCount && HeaterSamples >= 3 &&
                BackupSamples == FirstRecovery - FirstBackup && RecoveredSamples == FrameCount - FirstRecovery &&
                Minimum >= MinTemperature && Peak < MaxTemperature && Peak > 35f && Core < Peak - 2f && Residual < .5f;
        }

        internal static Phase PhaseAt(int index) => index < FirstHeat ? Phase.Cold :
            index < FirstBackup ? Phase.Heat : index < FirstRecovery ? Phase.Backup : Phase.Recovery;
        internal static bool OutletDrop(int sample) => sample % 5 is 1 or 2;

        internal static bool RequestHeater(Replay run)
        {
            if (run.Core < 20.5f) run.HeatRequested = true;
            if (run.Core > 24f || PhaseAt(run.Sent) != Phase.Cold) run.HeatRequested = false;
            return run.HeatRequested;
        }

        internal static bool RequestRadiator(Replay run, int sensors, float referenceGain, float flow)
        {
            return !run.Invalid && sensors == 12 && referenceGain is >= .96f and <= 1.04f &&
                flow > .30f && run.Core is >= 30f and < MaxTemperature;
        }

        internal static float Flow(float pumpA, float pumpB)
        {
            if (pumpA >= .035f) return Mathf.Min(1.1f, pumpA / .05f * 1.1f);
            return pumpB >= .028f ? Mathf.Min(.715f, pumpB / .04f * .715f) : 0f;
        }

        internal void Advance(Replay run, Inputs sample)
        {
            int index = run.Sent++;
            Phase phase = PhaseAt(index);
            if (!Nonnegative(sample.PumpA) || !Nonnegative(sample.PumpB) || !Nonnegative(sample.CoreWatts) ||
                !Nonnegative(sample.HeaterWatts) || !Nonnegative(sample.RadiatorCurrent) ||
                !Nonnegative(sample.ReferenceGain) || sample.SensorCount is < 0 or > 12)
            {
                run.Invalid = true;
                return;
            }

            float flow = Flow(sample.PumpA, sample.PumpB);
            int loop = sample.PumpA >= .035f ? 1 : sample.PumpB >= .028f ? 2 : 0;
            float coreHeat = sample.CoreWatts + sample.HeaterWatts;
            float flowFactor = flow / 1.1f;
            float circulating = (.65f + 4.85f * flowFactor) * (run.Core - run.Coolant);
            float radiatorBoost = Mathf.Min(1f, sample.RadiatorCurrent / .0227f) * .85f;
            float lost = (.25f + 2.15f * flowFactor + radiatorBoost) * (run.Coolant - SinkTemperature);
            // Each two-second step transfers the same joules from the core into the coolant.
            // With 120/95 J/K capacities and the bounded conductances, this explicit step is stable.
            run.Core += (coreHeat - circulating) * StepSeconds / CoreCapacity;
            run.Coolant += (circulating - lost) * StepSeconds / LoopCapacity;
            run.HeatJ += coreHeat * StepSeconds;
            run.SinkJ += lost * StepSeconds;
            run.Peak = Mathf.Max(run.Peak, run.Core);
            run.Minimum = Mathf.Min(run.Minimum, run.Core);

            run.OutletAge = sample.SensorCount == 12 ? 0f : run.OutletAge + StepSeconds;
            run.PeakAge = Mathf.Max(run.PeakAge, run.OutletAge);
            bool stale = run.OutletAge > MaximumSensorAge;
            float independent = run.Coolant + .13f * Mathf.Sin(index * .43f);
            float reported = run.Coolant + .11f * Mathf.Cos(index * .57f) + (sample.ReferenceGain - 1f) * 23f;
            float mismatch = Mathf.Abs(reported - independent);
            run.MaxDisagreement = Mathf.Max(run.MaxDisagreement, mismatch);
            bool drift = mismatch >= 3f;
            bool expectedLoop = phase == Phase.Backup ? loop == 2 : loop == 1;
            if (phase != Phase.Backup && loop != 1) run.PumpLoss++;
            if (drift) run.Drift++;
            if (stale) run.Stale++;
            if (phase == Phase.Cold && sample.HeaterWatts > 35f) run.HeaterSamples++;
            if (phase == Phase.Backup && loop == 2) run.BackupSamples++;
            if (phase == Phase.Recovery && loop == 1) run.RecoveredSamples++;
            if (expectedLoop && !stale && !drift && run.Core is >= MinTemperature and <= MaxTemperature &&
                (phase is not (Phase.Heat or Phase.Backup) || sample.CoreWatts > 86f))
                run.Good++;

            run.Last = new Sample
            {
                Segment = phase, Core = run.Core, Coolant = run.Coolant, Flow = flow, Loop = loop,
                SensorAge = run.OutletAge, Confidence = stale ? 0f : Mathf.Clamp01(1f - mismatch / 12f)
            };
        }

        private static bool Nonnegative(float value) => value >= 0f && !float.IsInfinity(value);
    }
}
