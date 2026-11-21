using System;
using UnityEngine;

namespace ProjectSpark.Gameplay.ThermalEndurance
{
    internal sealed class SparkLevel109ThermalModel
    {
        public const float StepSeconds = .05f;
        private const int Junction = 0, Package = 1, Sink = 2, Fet = 3, Choke = 7, Spreader = 11, U2 = 12, U3 = 13, Board = 14;
        private static readonly float[] Capacity =
        {
            .42f, .72f, 1.25f, .31f, .31f, .31f, .31f, .41f, .41f, .41f, .41f, 1.47f, .40f, .40f, 2.30f
        };
        private readonly float[] temperature = new float[Capacity.Length], watts = new float[Capacity.Length];

        public float Cpu => temperature[Junction];
        public float Heatsink => temperature[Sink];
        public float VrmSpreader => temperature[Spreader];
        public float MemoryA => temperature[U2];
        public float MemoryB => temperature[U3];
        public float Pcb => temperature[Board];
        public float Phase(int i) => temperature[Fet + i];

        public SparkLevel109ThermalModel()
        {
            for (int i = 0; i < temperature.Length; i++) temperature[i] = 25f;
        }

        public void Step(float ambient, float pwm, float tim, bool spreader, float cpuPower, float memoryA, float memoryB,
            float[] currents, float[] losses)
        {
            Array.Clear(watts, 0, watts.Length);
            watts[Junction] = Mathf.Max(0f, cpuPower) * 1.15f;
            watts[U2] = Mathf.Max(0f, memoryA) * .62f;
            watts[U3] = Mathf.Max(0f, memoryB) * .67f;
            Exchange(Junction, Package, .62f);
            Exchange(Package, Sink, Mathf.Max(0f, tim));
            Exchange(Package, Board, .061f);
            Exchange(U2, Board, .056f);
            Exchange(U3, Board, .056f);
            Cool(Junction, .022f);
            Cool(Sink, .145f + pwm * .15f);
            Cool(U2, .053f + pwm * .015f);
            Cool(U3, .048f + pwm * .015f);
            Cool(Board, .27f + pwm * .10f);
            Cool(Spreader, .13f + pwm * .12f);
            for (int i = 0; i < 4; i++)
            {
                var amps = Mathf.Max(0f, currents[i]);
                watts[Fet + i] = Mathf.Max(0f, losses[i]) * 2.8f + .36f * amps;
                watts[Choke + i] = .24f * amps * amps;
                Exchange(Fet + i, Choke + i, .067f);
                Exchange(Fet + i, Board, .057f);
                Exchange(Fet + i, Spreader, spreader ? .12f : 0f);
                Exchange(Choke + i, Board, .08f);
                Cool(Fet + i, .032f + pwm * .033f);
                Cool(Choke + i, .046f);
            }
            for (int i = 0; i < temperature.Length; i++)
                temperature[i] += watts[i] * StepSeconds / Capacity[i];

            void Cool(int point, float conductance) => watts[point] += (ambient - temperature[point]) * conductance;
        }

        private void Exchange(int a, int b, float conductance)
        {
            float flow = (temperature[a] - temperature[b]) * conductance;
            watts[a] -= flow;
            watts[b] += flow;
        }
    }

    internal sealed class SparkLevel109Endurance
    {
        public const int TotalTicks = 960;
        private uint pattern;
        public int Tick { get; private set; }
        public int Bits { get; private set; }
        public int Errors { get; private set; }
        public int HotSamples { get; private set; }
        public int ColdSamples { get; private set; }
        public int HotErrors { get; private set; }
        public int ColdErrors { get; private set; }
        public int HotOpenings { get; private set; }
        public int ColdRecoveries { get; private set; }
        public float PeakCpu { get; private set; }
        public float PeakDelta { get; private set; }
        public float PeakPhaseSpread { get; private set; }
        public float PeakPhaseLossRatio { get; private set; }
        public float PeakMemory { get; private set; }
        public bool RailsStable { get; private set; }
        public bool PcieStable { get; private set; }
        public bool Derated { get; private set; }

        public bool Healthy => Tick == TotalTicks && Bits > 0 && HotSamples > 0 && ColdSamples > 0 && Errors == 0 &&
            PeakCpu < 85f && PeakDelta < 18f && PeakPhaseSpread < 5f && PeakPhaseLossRatio < 2.4f &&
            RailsStable && PcieStable && !Derated;

        public void Begin()
        {
            Tick = Bits = Errors = HotSamples = ColdSamples = HotErrors = ColdErrors = HotOpenings = ColdRecoveries = 0;
            PeakCpu = PeakDelta = PeakPhaseSpread = PeakPhaseLossRatio = PeakMemory = 0;
            RailsStable = PcieStable = true;
            Derated = false;
            pattern = 0x7ffff6d1U;
        }

        public void Sample(SparkLevel109ThermalModel thermal, float[] losses, bool rails, bool pcie, bool data, bool derated)
        {
            PeakCpu = Mathf.Max(PeakCpu, thermal.Cpu);
            PeakDelta = Mathf.Max(PeakDelta, thermal.Cpu - thermal.Heatsink);
            PeakMemory = Mathf.Max(PeakMemory, thermal.MemoryB);
            float peerPhase = Mathf.Max(thermal.Phase(0), Mathf.Max(thermal.Phase(1), thermal.Phase(3)));
            PeakPhaseSpread = Mathf.Max(PeakPhaseSpread, thermal.Phase(2) - peerPhase);
            float peerLoss = (losses[0] + losses[1] + losses[3]) / 3f;
            if (peerLoss > .03f) PeakPhaseLossRatio = Mathf.Max(PeakPhaseLossRatio, losses[2] / peerLoss);
            RailsStable &= rails;
            PcieStable &= pcie;
            Derated |= derated;
            if (Tick % 10 == 0) CompareMemory(data, thermal.MemoryB);
            Tick++;
        }

        public void RecordContact(bool closed)
        {
            if (closed) ColdRecoveries++;
            else HotOpenings++;
        }

        private void CompareMemory(bool arrives, float temperature)
        {
            int start = Errors;
            for (int word = 0; word < 16; word++)
            {
                uint expected = 0;
                for (int bit = 0; bit < 32; bit++)
                {
                    expected = (expected << 1) | (pattern >> 30);
                    pattern = ((pattern << 1) | ((pattern >> 30 ^ pattern >> 27) & 1U)) & 0x7fffffffU;
                }
                uint result = arrives ? expected : expected & ~1U;
                if (result != expected) Errors++;
                Bits += 32;
            }
            if (temperature >= 49f) { HotSamples++; HotErrors += Errors - start; }
            if (temperature <= 44f) { ColdSamples++; ColdErrors += Errors - start; }
        }
    }
}
