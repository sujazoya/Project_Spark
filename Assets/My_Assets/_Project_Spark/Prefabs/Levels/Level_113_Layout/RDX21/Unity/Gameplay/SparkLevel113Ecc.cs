using UnityEngine;

namespace ProjectSpark.Gameplay.RedundancyCitadel
{
    // Extended Hamming (8,4) teaching codec. Bit 8 is the overall parity bit.
    internal static class SparkLevel113Ecc
    {
        internal static byte Encode(int data)
        {
            int word = ((data & 1) << 2) | ((data & 2) << 3) | ((data & 4) << 3) | ((data & 8) << 3);
            var checks = Inspect((byte)word);
            for (int i = 0; i < 3; i++)
                if ((checks.position & (1 << i)) != 0) word |= 1 << ((1 << i) - 1);
            if (Inspect((byte)word).odd) word |= 128;
            return (byte)word;
        }

        internal static (int position, bool odd) Inspect(byte word)
        {
            int position = 0;
            bool odd = false;
            for (int bit = 1; bit <= 8; bit++)
            {
                if ((word & (1 << (bit - 1))) == 0) continue;
                odd = !odd;
                if (bit < 8) position ^= bit;
            }
            return (position, odd);
        }

        internal static (float skew, bool healthy, float timeout) TestTiming(float referencePeriod)
        {
            const float clockA = .010f, deadline = .032f, agreement = .002f;
            float maxSkew = 0f, lastAccepted = 0f, firstTimeout = 0f;
            bool healthy = true;
            for (int pulse = 1; pulse <= 8; pulse++)
            {
                float a = pulse * clockA;
                float b = pulse * referencePeriod;
                float skew = Mathf.Abs(a - b);
                maxSkew = Mathf.Max(maxSkew, skew);
                float arrival = Mathf.Max(a, b);
                if (arrival - lastAccepted >= deadline) healthy = false;
                if (skew <= agreement) lastAccepted = arrival;
            }
            // Withhold all subsequent heartbeats and observe the independent watchdog.
            for (int sample = 1; sample <= 40; sample++)
            {
                if (sample * .001f < deadline) continue;
                firstTimeout = sample;
                break;
            }
            return (maxSkew * 1000f, healthy, firstTimeout);
        }
    }
}
