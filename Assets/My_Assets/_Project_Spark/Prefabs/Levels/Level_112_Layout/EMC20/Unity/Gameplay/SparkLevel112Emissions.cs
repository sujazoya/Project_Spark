using UnityEngine;

namespace ProjectSpark.Gameplay.EmcGauntlet
{
    internal static class SparkLevel112Emissions
    {
        public const int BandCount = 8;
        public const float ConductedLimit = 49f, RadiatedLimit = 41f, DetectorUncertainty = .8f;
        private const int WindowSamples = 96;
        private const float FixtureOhms = 50f, ProbeDistanceMetres = .25f, ProbeOrientation = .78f;
        private static readonly float[] Frequencies = { .15f, .45f, 1.5f, 6f, 24f, 60f, 120f, 240f };
        private static readonly float[] Harmonics = { .55f, .80f, .85f, .70f, 1f, .88f, .62f, .49f };

        internal readonly struct Band
        {
            public readonly float FrequencyMHz, ConductedDb, RadiatedDb, CommonMicroamps, DifferentialMicroamps, InsertionLossDb;

            public Band(float frequency, float conducted, float radiated, float cm, float dm, float insertionLoss)
            {
                FrequencyMHz = frequency;
                ConductedDb = conducted;
                RadiatedDb = radiated;
                CommonMicroamps = cm;
                DifferentialMicroamps = dm;
                InsertionLossDb = insertionLoss;
            }

            public bool InsideLimits => ConductedDb + DetectorUncertainty < ConductedLimit &&
                RadiatedDb + DetectorUncertainty < RadiatedLimit;
        }

        public static void Capture(Band[] bins, float currentAmps, float fixtureGain, float chokeVolts,
            float bondOhms, bool returnOpen, bool spread)
        {
            float core = Mathf.Clamp01(chokeVolts / 3.25f);
            float seam = Mathf.Clamp01((bondOhms - .023f) / .37f);
            for (int i = 0; i < BandCount; i++)
            {
                float f = Frequencies[i];
                float spreadLoss = spread && i > 0 ? .73f + .025f * ((i + 2) % 4) : 1f;
                float harmonic = Harmonics[i] * spreadLoss;
                float common = (1.5f + currentAmps * 16f * harmonic) * (returnOpen ? 1.85f : 1f);
                float differential = .9f + currentAmps * 6f * harmonic;
                float insertionLoss = (3f + 19f * f / (f + .70f)) * Mathf.Lerp(.08f, 1f, core);
                float filtered = common * Mathf.Pow(10f, -insertionLoss / 20f);
                float ring = returnOpen ? currentAmps * 6.3f * f / (f + 5f) : 0f;
                float atPort = FixtureOhms * fixtureGain * Mathf.Sqrt(filtered * filtered +
                    (differential * .26f + ring) * (differential * .26f + ring));
                float radiationCoupling = (.90f + .60f * f / (f + 7f) + seam * 11f + (returnOpen ? 4f : 0f));
                float atProbe = common * radiationCoupling * ProbeOrientation * (.25f / ProbeDistanceMetres);
                float inputRms = Detector(common, i);
                float outputRms = Detector(filtered, i);
                bins[i] = new Band(f, Db(Detector(atPort, i)), Db(Detector(atProbe, i + 1)),
                    outputRms, Detector(differential, i), Db(inputRms / outputRms));
            }
        }

        private static float Db(float ratio) => 20f * Mathf.Log10(Mathf.Max(.000001f, ratio));

        // Coherent quadrature detector after the modeled narrow-band probe.
        // Each window spans four periods of the selected center frequency.
        private static float Detector(float rms, int band)
        {
            float i = 0f, q = 0f;
            float phase = band * .74f;
            for (int n = 0; n < WindowSamples; n++)
            {
                float angle = n * (Mathf.PI * 8f / WindowSamples);
                float sample = rms * (1.41421356f * Mathf.Cos(angle + phase) + .057f * Mathf.Sin(2f * angle + .8f));
                i += sample * Mathf.Cos(angle);
                q += sample * Mathf.Sin(angle);
            }
            return Mathf.Sqrt(i * i + q * q) * (1.41421356f / WindowSamples);
        }
    }

    internal sealed class SparkLevel112Packets
    {
        public const int BurstCount = 16;
        private const int PacketBytes = 16;
        private readonly byte[] expected = new byte[PacketBytes], observed = new byte[PacketBytes];
        public int Bursts { get; private set; }
        public int Packets { get; private set; }
        public int Bytes { get; private set; }
        public int Errors { get; private set; }
        public int FirstLane { get; private set; } = -1;

        public void Begin()
        {
            Bursts = Packets = Bytes = Errors = 0;
            FirstLane = -1;
        }

        public void CompareBurst(float[] eyes)
        {
            for (int lane = 0; lane < eyes.Length; lane++)
            for (int frame = 0; frame < 8; frame++)
            {
                int sequence = Packets;
                uint state = 0xe11220u ^ (uint)(sequence + 1) * 747796405u;
                expected[0] = (byte)sequence;
                expected[1] = (byte)(sequence >> 8);
                expected[2] = (byte)lane;
                for (int i = 3; i < PacketBytes; i++)
                {
                    state ^= state << 13;
                    state ^= state >> 17;
                    state ^= state << 5;
                    expected[i] = (byte)state;
                }
                System.Array.Copy(expected, observed, PacketBytes);
                if (eyes[lane] < 320f && (frame + lane + Bursts) % 3 != 0)
                    observed[3 + sequence % (PacketBytes - 3)] ^= (byte)(1 << (sequence % 8));
                if (Check(expected) != Check(observed))
                {
                    Errors++;
                    if (FirstLane < 0) FirstLane = lane;
                }
                Packets++;
                Bytes += PacketBytes;
            }
            Bursts++;
        }

        private static int Check(byte[] frame)
        {
            int crc = 0xffff;
            foreach (int value in frame)
            {
                crc ^= value << 8;
                for (int bit = 0; bit < 8; bit++)
                    crc = ((crc << 1) ^ ((crc & 0x8000) != 0 ? 0x1021 : 0)) & 0xffff;
            }
            return crc;
        }
    }
}
