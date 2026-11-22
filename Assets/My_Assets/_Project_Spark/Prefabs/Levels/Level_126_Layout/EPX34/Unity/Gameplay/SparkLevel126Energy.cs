using UnityEngine;

namespace ProjectSpark.Gameplay.EclipseCore
{
    internal sealed class SparkLevel126Energy
    {
        internal const int FrameCount = 64, FirstEclipse = 6, FirstSunrise = 42;
        internal const float Capacity = 240f, InitialEnergy = 192f, Reserve = 60f;
        internal const float EssentialTarget = 60f, AuxiliaryTarget = 45f;
        internal const float PeakSolarCurrent = .25f;
        private const float HoursPerSample = 2.5f / 60f;
        private const float ChargeEfficiency = .92f, DischargeEfficiency = .90f;

        internal enum Phase { SunCheck, Eclipse, Sunrise }

        internal readonly struct Inputs
        {
            internal readonly float Solar, Essential, Auxiliary, Bus, LoadedBus, SenseGain;
            internal readonly bool SolarTripped;
            internal Inputs(float solar, float essential, float auxiliary, float bus, float loadedBus, float senseGain, bool solarTripped)
            {
                Solar = solar; Essential = essential; Auxiliary = auxiliary; Bus = bus;
                LoadedBus = loadedBus; SenseGain = senseGain; SolarTripped = solarTripped;
            }
        }

        internal struct Sample
        {
            internal Phase Segment;
            internal float Stored, Solar, Auxiliary, Bus, Temperature, HoursToReserve;
        }

        internal sealed class Replay
        {
            internal int Sent, Good, ShedSamples, RecoverySamples, SolarTrips, SenseHolds, SagHolds;
            internal float Stored = InitialEnergy, Meter = InitialEnergy, Minimum = InitialEnergy, EclipseEnd;
            internal float SolarWh, ServedWh, LossWh, SpilledWh, UnservedEssentialWh;
            internal float MaxMeterError, PeakSag, MinimumLoadedBus = 5f, Temperature = 27f;
            internal bool Invalid, ShedUntilSunrise;
            internal Sample Last;
            internal float BalanceError => Mathf.Abs(InitialEnergy + SolarWh - ServedWh - LossWh - SpilledWh - Stored);
            internal bool Complete => Sent >= FrameCount;
            internal bool Qualified => Complete && !Invalid && Good == FrameCount && ShedSamples == FirstSunrise - FirstEclipse &&
                RecoverySamples == FrameCount - FirstSunrise && EclipseEnd is > 88f and < 96f &&
                Stored - EclipseEnd is > 60f and < 67f && Minimum > Reserve &&
                UnservedEssentialWh < .8f && MaxMeterError < 4f && BalanceError < .05f;
        }

        internal static Phase PhaseAt(int frame) => frame < FirstEclipse ? Phase.SunCheck :
            frame < FirstSunrise ? Phase.Eclipse : Phase.Sunrise;

        internal static bool RequestAux(Replay run, float solarWatts, float busVoltage)
        {
            if (busVoltage < 4.55f || run.Invalid || run.Stored <= Reserve) return false;
            if (PhaseAt(run.Sent) != Phase.Eclipse) return solarWatts > EssentialTarget + AuxiliaryTarget + 4f;
            float projectedUse = (FirstSunrise - run.Sent) * HoursPerSample *
                (EssentialTarget + AuxiliaryTarget) / DischargeEfficiency;
            if (run.Stored - projectedUse < Reserve) run.ShedUntilSunrise = true;
            return !run.ShedUntilSunrise;
        }

        internal void Advance(Replay run, Inputs inputs)
        {
            Phase phase = PhaseAt(run.Sent++);
            if (!FinitePositive(inputs.Solar) || !FinitePositive(inputs.Essential) ||
                !FinitePositive(inputs.Auxiliary) || !FinitePositive(inputs.Bus) ||
                !FinitePositive(inputs.LoadedBus) || !FinitePositive(inputs.SenseGain))
            {
                run.Invalid = true;
                return;
            }

            float before = run.Stored;
            float essentialDemand = inputs.Essential * HoursPerSample;
            float draw = (inputs.Essential + inputs.Auxiliary) * HoursPerSample;
            float solar = inputs.Solar * HoursPerSample;
            float served = draw, loss = 0f, spill = 0f;
            // The first six readings prove the energized circuit before advancing mission time.
            if (phase != Phase.SunCheck)
            {
                if (solar >= draw)
                {
                    float surplus = solar - draw;
                    float input = Mathf.Min(surplus, (Capacity - run.Stored) / ChargeEfficiency);
                    run.Stored += input * ChargeEfficiency;
                    loss = input * (1f - ChargeEfficiency);
                    spill = surplus - input;
                }
                else
                {
                    float withdrawal = Mathf.Min(run.Stored, (draw - solar) / DischargeEfficiency);
                    float delivered = withdrawal * DischargeEfficiency;
                    run.Stored -= withdrawal;
                    loss = withdrawal - delivered;
                    served = solar + delivered;
                }
                run.SolarWh += solar;
                run.ServedWh += served;
                run.LossWh += loss;
                run.SpilledWh += spill;
                // Essential loads receive the available energy before auxiliary loads.
                run.UnservedEssentialWh += Mathf.Max(0f, EssentialTarget * HoursPerSample - Mathf.Min(essentialDemand, served));
                run.Meter += (run.Stored - before) * inputs.SenseGain;
                run.Minimum = Mathf.Min(run.Minimum, run.Stored);
            }

            float meterError = Mathf.Abs(run.Meter - run.Stored);
            float dip = Mathf.Max(0f, inputs.Bus - inputs.LoadedBus);
            bool drift = Mathf.Abs(1f - inputs.SenseGain) > .10f;
            bool sag = inputs.LoadedBus < 4.55f || dip > .18f;
            if (inputs.SolarTripped) run.SolarTrips++;
            if (drift) run.SenseHolds++;
            if (sag) run.SagHolds++;
            run.MaxMeterError = Mathf.Max(run.MaxMeterError, meterError);
            run.PeakSag = Mathf.Max(run.PeakSag, dip);
            run.MinimumLoadedBus = Mathf.Min(run.MinimumLoadedBus, inputs.LoadedBus);
            run.Temperature += (27f + loss * 8f + (inputs.SolarTripped ? 12f : 0f) - run.Temperature) * .15f;

            if (phase == Phase.Eclipse && inputs.Auxiliary < .5f) run.ShedSamples++;
            if (phase == Phase.Sunrise && run.Stored > before && inputs.Auxiliary > 40f) run.RecoverySamples++;
            if (run.Sent == FirstSunrise) run.EclipseEnd = run.Stored;
            bool correctSolar = phase == Phase.Eclipse ? inputs.Solar < .5f : inputs.Solar > 170f;
            if (correctSolar && !inputs.SolarTripped && !drift && !sag && inputs.Essential > 55f &&
                run.Stored > Reserve && run.Temperature < 65f) run.Good++;

            float discharge = inputs.Essential + inputs.Auxiliary - inputs.Solar;
            run.Last = new Sample
            {
                Segment = phase, Stored = run.Stored, Solar = inputs.Solar, Auxiliary = inputs.Auxiliary,
                Bus = inputs.LoadedBus, Temperature = run.Temperature,
                HoursToReserve = discharge > .5f ? Mathf.Max(0f, run.Stored - Reserve) * DischargeEfficiency / discharge : -1f
            };
        }

        private static bool FinitePositive(float value) => value >= 0f && !float.IsInfinity(value);
    }
}
