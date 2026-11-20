using System;
using UnityEngine;

namespace ProjectSpark.Gameplay.SignalPhantom
{
    internal static class SparkLevel102Channel
    {
        public const int TestedBits = 4096, Phases = 33, EyeTraces = 24;
        public const float VelocityMmPerNs = 165f;
        private const float LineOhms = 100f;
        private const float HalfSwing = .40f;

        internal sealed class Capture
        {
            public float lengthMm, skewPs, terminationOhms, roundTripNs, stubReturnNs, stubReflection, coupling;
            public float height, width, tdrSpanNs;
            public int errors;
            public float[] tdr, eye;

            public bool Passing => Mathf.Abs(terminationOhms - LineOhms) <= 10f && stubReflection < .16f &&
                coupling < .15f && height >= .16f && width >= .45f && skewPs <= 15f && errors == 0;
        }

        internal static Capture Measure(float positiveMm, float negativeMm, float termination,
            float spacingMm, float parallelMm, float viaTapMm, float stubMm, bool aggressor, int lane)
        {
            var capture = new Capture
            {
                lengthMm = .5f * (positiveMm + negativeMm),
                skewPs = Mathf.Abs(positiveMm - negativeMm) / VelocityMmPerNs * 1000f,
                stubReturnNs = 2f * viaTapMm / VelocityMmPerNs,
                stubReflection = Mathf.Min(.31f, stubMm * .20f),
                coupling = aggressor ? .52f * Mathf.Exp(-spacingMm / .42f) * Mathf.Min(parallelMm / 50f, 1f) : 0f,
                eye = new float[Phases * EyeTraces],
                tdr = new float[193]
            };
            capture.roundTripNs = 2f * capture.lengthMm / VelocityMmPerNs;
            capture.tdrSpanNs = capture.roundTripNs + .34f;
            float gamma = (termination - LineOhms) / (termination + LineOhms);
            float loss = Mathf.Exp(-capture.lengthMm * .0026f);
            float stubDelay = 2f * stubMm / VelocityMmPerNs;
            for (int i = 0; i < capture.tdr.Length; i++)
            {
                float time = i * capture.tdrSpanNs / (capture.tdr.Length - 1);
                float mismatch = gamma * loss * loss * Edge(time - capture.roundTripNs, .026f);
                float stub = capture.stubReflection * (Edge(time - capture.stubReturnNs, .005f) -
                    .68f * Edge(time - capture.stubReturnNs - stubDelay, .007f));
                capture.tdr[i] = mismatch - stub;
            }
            float recoveredGamma = (capture.tdr[capture.tdr.Length - 1] + .32f * capture.stubReflection) / (loss * loss);
            capture.terminationOhms = LineOhms * (1f + recoveredGamma) / (1f - recoveredGamma);

            var symbols = Prbs(lane == 0 ? 0x5D : 0x4B);
            var interferer = Prbs(lane == 0 ? 0x73 : 0x65);
            var low = new float[Phases];
            var high = new float[Phases];
            Array.Fill(low, float.NegativeInfinity);
            Array.Fill(high, float.PositiveInfinity);
            for (int bit = 1; bit <= TestedBits; bit++)
            {
                float jitter = .016f * Mathf.Sin(bit * 1.713f) + capture.skewPs * .001f;
                float noise = .009f * Mathf.Sin(bit * 2.613f + lane);
                float symbol = symbols[bit];
                float echo = (.18f * gamma + .31f * capture.stubReflection) * symbols[bit - 1];
                for (int phase = 0; phase < Phases; phase++)
                {
                    float t = phase / (float)(Phases - 1) - jitter;
                    float s = Edge(t, .18f);
                    float next = Edge(t - 1f, .18f);
                    float output = HalfSwing * loss * (Mathf.Lerp(symbols[bit - 1], symbol, s) +
                        next * (symbols[bit + 1] - symbol) + echo + capture.coupling * (interferer[bit] - interferer[bit - 1])) + noise;
                    if (symbol > 0f) high[phase] = Mathf.Min(high[phase], output);
                    else low[phase] = Mathf.Max(low[phase], output);
                    if (phase == Phases / 2 && (output > 0f) != (symbol > 0f)) capture.errors++;
                    if (bit <= EyeTraces) capture.eye[(bit - 1) * Phases + phase] = output;
                }
            }

            int mid = Phases / 2;
            capture.height = Mathf.Max(0f, high[mid] - low[mid]);
            int left = mid;
            int right = mid;
            while (left > 0 && high[left - 1] - low[left - 1] >= .16f) left--;
            while (right + 1 < Phases && high[right + 1] - low[right + 1] >= .16f) right++;
            capture.width = (right - left) / (float)(Phases - 1);
            return capture;
        }

        private static float[] Prbs(int state)
        {
            var symbols = new float[TestedBits + 2];
            for (int i = 0; i < symbols.Length; i++)
            {
                symbols[i] = (state & 1) == 0 ? -1f : 1f;
                int feedback = ((state >> 6) ^ (state >> 5)) & 1;
                state = ((state << 1) | feedback) & 127;
            }
            return symbols;
        }

        private static float Edge(float time, float rise) => Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(-rise * .5f, rise * .5f, time));
    }
}
