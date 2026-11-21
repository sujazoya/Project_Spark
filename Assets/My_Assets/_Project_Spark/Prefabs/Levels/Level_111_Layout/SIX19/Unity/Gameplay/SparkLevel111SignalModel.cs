using UnityEngine;

namespace ProjectSpark.Gameplay.SignalCrucible
{
    internal static class SparkLevel111SignalModel
    {
        public const float PropagationMmPerPs = .16f;

        internal readonly struct Snapshot
        {
            public readonly float Impedance, EyeMv, EyePs, JitterPs, SkewPs, ReturnLoss, EchoPs, ProjectedErrors;

            public Snapshot(float impedance, float eyeMv, float eyePs, float jitter, float skew, float returnLoss, float echo, float errors)
            {
                Impedance = impedance;
                EyeMv = eyeMv;
                EyePs = eyePs;
                JitterPs = jitter;
                SkewPs = skew;
                ReturnLoss = returnLoss;
                EchoPs = echo;
                ProjectedErrors = errors;
            }

            public bool Healthy => Impedance is >= 90f and <= 110f && EyeMv >= 310f && EyePs >= 45f &&
                JitterPs < 18f && SkewPs < 25f && ReturnLoss >= 14f;
        }

        public static Snapshot Measure(int lane, float swingVolts, float longestMm, float pairSkewMm, float referenceJitter,
            float equalizerBias, float equalization, bool returnOpen, float returnDistanceMm)
        {
            float impedance = 98.5f + lane + (returnOpen ? 32f : 0f);
            float reflection = Mathf.Abs((impedance - 100f) / (impedance + 100f)) + .058f + (returnOpen ? .25f : 0f);
            float returnLoss = -20f * Mathf.Log10(reflection);
            float biasLoss = Mathf.Clamp01((1.78f - equalizerBias) / 1.78f);
            float skew = 3f + .75f * pairSkewMm + (returnOpen ? 45f : 0f);
            float jitter = referenceJitter + skew * .10f + biasLoss * 27f;
            float attenuation = Mathf.Clamp01(.19f + longestMm * .0012f + reflection * .92f + biasLoss * 1.34f);
            float eyeMv = Mathf.Max(0f, 680f * (swingVolts / 1.70f) * (1f - attenuation) +
                equalization * 170f * (1f - biasLoss) - jitter * 3.2f);
            float eyePs = Mathf.Clamp(100f - 2f * jitter - skew * .75f - reflection * 27f, 0f, 100f);
            float echo = 2f * (returnOpen ? returnDistanceMm : longestMm) / PropagationMmPerPs;
            float exponent = Mathf.Clamp(3f + 11f * Mathf.Min(eyeMv / 545f, eyePs / 82f) - jitter * .028f, 1f, 14f);
            return new Snapshot(impedance, eyeMv, eyePs, jitter, skew, returnLoss, echo, 1e9f * Mathf.Pow(10f, -exponent));
        }
    }

    internal sealed class SparkLevel111Pattern
    {
        public const int BurstCount = 16;
        private const int WordsPerLane = 32;
        private readonly uint[] shift = new uint[4];
        public int Bursts { get; private set; }
        public int Bits { get; private set; }
        public int Errors { get; private set; }
        public int FirstErrorLane { get; private set; } = -1;

        public void Reset()
        {
            Bursts = Bits = Errors = 0;
            FirstErrorLane = -1;
            for (int lane = 0; lane < shift.Length; lane++) shift[lane] = (0x7fffffffU >> lane) ^ 0x11119U;
        }

        public void Compare(SparkLevel111SignalModel.Snapshot[] channels)
        {
            for (int lane = 0; lane < shift.Length; lane++)
            {
                var channel = channels[lane];
                float deficit = Mathf.Max(Mathf.Clamp01((310f - channel.EyeMv) / 310f),
                    Mathf.Clamp01((45f - channel.EyePs) / 45f));
                if (!channel.Healthy) deficit = Mathf.Max(.12f, deficit);
                int errorWindow = Mathf.RoundToInt(deficit * 83f);
                for (int word = 0; word < WordsPerLane; word++)
                {
                    uint expected = NextWord(lane);
                    int position = (word * 23 + Bursts * 41 + lane * 17) % 97;
                    uint observed = position < errorWindow ? expected ^ (1U << ((word + Bursts) % 32)) : expected;
                    uint mismatch = observed ^ expected;
                    while (mismatch != 0)
                    {
                        Errors++;
                        if (FirstErrorLane < 0) FirstErrorLane = lane;
                        mismatch &= mismatch - 1;
                    }
                    Bits += 32;
                }
            }
            Bursts++;
        }

        private uint NextWord(int lane)
        {
            uint state = shift[lane], result = 0;
            for (int bit = 0; bit < 32; bit++)
            {
                result = (result << 1) | (state >> 30);
                state = ((state << 1) | ((state >> 30 ^ state >> 27) & 1U)) & 0x7fffffffU;
            }
            shift[lane] = state;
            return result;
        }
    }
}
