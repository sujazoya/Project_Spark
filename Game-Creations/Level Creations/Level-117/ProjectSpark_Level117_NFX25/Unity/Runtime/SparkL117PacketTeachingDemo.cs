using System;
using UnityEngine;

namespace ProjectSpark.Level117
{
    /// <summary>
    /// Educational four-port packet/PTP demonstration only. Does not touch the circuit graph,
    /// repair tools or LevelGamePlayManager. No authorization or objective progress is exposed.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkL117PacketTeachingDemo : MonoBehaviour
    {
        [Header("Illustrative, not a real hardware waveform")]
        [SerializeField, Range(0, 100000)] private int packetsPerPort = 1024;
        [SerializeField, Range(32, 9000)] private int packetBytes = 512;
        [SerializeField, Range(0f, 250f)] private float oscillatorDriftPpm = 7.5f;
        [SerializeField, Range(0f, 1f)] private float illustrativeDropProbability = 0.001f;
        [SerializeField] private uint deterministicSeed = 11725;
        [SerializeField] private bool simulateTransmitterFailurePort3;
        [SerializeField] private bool simulateViaFailurePort4;

        [Serializable]
        public struct PortResult
        {
            public int portId;
            public int packetCount;
            public int observedDrops;
            public int packetsDelivered;
            public long bytesDelivered;
            public bool illustrativeLinkUp;
            public string diagnosis;
        }

        [Serializable]
        public struct PtpResult
        {
            public double elapsedSeconds;
            public double clockOffsetMicroseconds;
            public bool isEducationalOnly;
        }

        public bool IsAuthoritative => false;
        public bool CanAwardObjectives => false;

        /// <summary>Bounded deterministic sample; does not send real network traffic.</summary>
        public PortResult[] GeneratePacketExample()
        {
            int count = Mathf.Clamp(packetsPerPort, 0, 100000);
            int packetSize = Mathf.Clamp(packetBytes, 32, 9000);
            System.Random random = new System.Random(unchecked((int)deterministicSeed));
            PortResult[] output = new PortResult[4];
            for (int index = 0; index < output.Length; index++)
            {
                bool healthy = !(index == 2 && simulateTransmitterFailurePort3) &&
                               !(index == 3 && simulateViaFailurePort4);
                int drops = 0;
                for (int k = 0; k < count; k++)
                    if (!healthy || random.NextDouble() < illustrativeDropProbability) drops++;
                output[index] = new PortResult
                {
                    portId = index + 1, packetCount = count, observedDrops = drops,
                    packetsDelivered = count - drops,
                    bytesDelivered = (long)(count - drops) * packetSize,
                    illustrativeLinkUp = healthy,
                    diagnosis = healthy ? "Illustrative traffic example only" :
                        "Illustrative physical impairment; check approved topology"
                };
            }
            return output;
        }

        public PtpResult CalculateIllustrativePtpOffset(double simulatedElapsedSeconds)
        {
            double elapsed = Math.Max(0d, Math.Min(3600d, simulatedElapsedSeconds));
            return new PtpResult
            {
                elapsedSeconds = elapsed,
                clockOffsetMicroseconds = elapsed * Mathf.Clamp(oscillatorDriftPpm, 0f, 250f),
                isEducationalOnly = true
            };
        }
    }
}
