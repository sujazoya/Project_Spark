using System;
using UnityEngine;

namespace ProjectSpark.Gameplay.MemoryMatrix
{
    internal sealed class SparkLevel104Memory
    {
        public const int Bursts = 64;
        public const int TestBits = 32768;
        public const float UnitIntervalPs = 625f;
        private readonly byte[,] cells = new byte[4, 256];
        public int Bits { get; private set; }
        public int Errors { get; private set; }
        public string FirstFailure { get; private set; }
        public bool ModeReady { get; private set; }

        internal readonly struct Window
        {
            public readonly float ReadMargin, WriteMargin;
            public readonly int ReadTap, WriteTap;
            public bool Pass => ReadMargin >= 85f && WriteMargin >= 85f;

            public Window(float readMargin, float writeMargin, int readTap, int writeTap)
            {
                ReadMargin = readMargin;
                WriteMargin = writeMargin;
                ReadTap = readTap;
                WriteTap = writeTap;
            }
        }

        public void Initialize()
        {
            Reset();
            ModeReady = true;
        }

        public void Reset()
        {
            ModeReady = false;
            Array.Clear(cells, 0, cells.Length);
            ResetTest();
        }

        public void ResetTest()
        {
            Bits = Errors = 0;
            FirstFailure = "";
        }

        public static Window Train(float dqsMm, float dqMm, float reference, float termination, bool responsive, int lane)
        {
            if (!responsive) return default;
            float skew = (dqMm - dqsMm) * 6.15f;
            float reflection = Mathf.Abs((termination - 47f) / (termination + 47f));
            float loss = Mathf.Max(dqsMm, dqMm) * .25f + Mathf.Abs(reference - .60f) * 660f +
                248f * reflection;
            float halfRead = UnitIntervalPs * .5f - 69f - loss;
            float halfWrite = UnitIntervalPs * .5f - 83f - loss;
            var read = SampleWindow(skew + 18f + lane * 9f, halfRead);
            var write = SampleWindow(-skew - 28f + lane * 8f, halfWrite);
            return new Window(read.margin, write.margin, read.tap, write.tap);
        }

        private static (float margin, int tap) SampleWindow(float center, float halfWidth)
        {
            float best = 0f;
            int preferred = 0;
            for (int tap = -8; tap <= 8; tap++)
            {
                float margin = halfWidth - Mathf.Abs(tap * 32f - center);
                if (margin > best) { best = margin; preferred = tap; }
            }
            return (best, preferred);
        }

        public void Burst(int index, bool u3Dq0Continuity)
        {
            if (!ModeReady || index is < 0 or >= Bursts) return;
            int pattern = index / 16;
            int first = index % 16 * 16;
            for (int lane = 0; lane < 4; lane++)
            {
                for (int offset = 0; offset < 16; offset++)
                {
                    int address = first + offset;
                    cells[lane, address] = Pattern(pattern, lane, address);
                }
                for (int offset = 0; offset < 16; offset++)
                {
                    int address = first + offset;
                    byte expected = Pattern(pattern, lane, address);
                    byte actual = cells[lane, address];
                    if (!u3Dq0Continuity && lane == 3) actual &= 0xFE;
                    Bits += 8;
                    int mismatch = expected ^ actual;
                    if (mismatch == 0) continue;
                    if (Errors == 0) FirstFailure = $"U3 BYTE 3 · ADDRESS {address:X2} · {expected:X2} → {actual:X2}";
                    for (int bit = 0; bit < 8; bit++) Errors += (mismatch >> bit) & 1;
                }
            }
        }

        private static byte Pattern(int pattern, int lane, int address)
        {
            int walk = 1 << (address & 7);
            switch (pattern)
            {
                case 0: return (byte)(address ^ lane * 79);
                case 1: return (byte)walk;
                case 2: return (byte)~walk;
                default:
                    uint word = ((uint)(address + lane * 257) ^ 0x9E3779B9u) * 1664525u;
                    word ^= word >> 13;
                    return (byte)(word ^ word >> 8);
            }
        }
    }
}
