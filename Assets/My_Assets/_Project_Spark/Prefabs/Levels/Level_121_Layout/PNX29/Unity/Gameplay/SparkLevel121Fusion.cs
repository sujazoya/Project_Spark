using System;
using UnityEngine;

namespace ProjectSpark.Gameplay.PerceptionNexus
{
    // Deterministic sensor bench with a known virtual target, independent observations,
    // measured calibration, CRC checks and fail-closed fusion.
    internal sealed class SparkLevel121Fusion
    {
        internal const int FrameCount = 64;
        internal const float MaxSkewUs = 220f;
        private const float FocalPixels = 420f;
        private static readonly float[] Positions = { -.085f, .085f, -.115f, .115f };
        private static readonly float[] PixelBias = { 1.35f, -1.8f, 1.05f, -1.15f };
        private readonly float[] calibratedPixels = new float[4];
        private float rangeBias, yawBias;
        internal bool Calibrated { get; private set; }
        internal enum InjectedFrame { None, Stale, Duplicate, Corrupt }

        internal readonly struct Reading
        {
            internal readonly float Depth, Lateral, Confidence, Skew;
            internal readonly bool Valid, Obstacle;
            internal Reading(float depth, float lateral, float confidence, float skew, bool valid, bool obstacle)
            { Depth = depth; Lateral = lateral; Confidence = confidence; Skew = skew; Valid = valid; Obstacle = obstacle; }
        }

        internal sealed class Replay
        {
            internal int Sent, Accepted, Rejected, MissingB, Stale, RangeDrops, CrcFailures, Duplicates;
            internal int Obstacles, CorrectObstacles;
            internal float PeakErrorMm, PeakSkew, MinimumConfidence = 1f, MinimumRail = 5f, PeakSag;
            internal Reading Last;
            internal ulong SeenSequences;
            internal bool Complete => Sent >= FrameCount;
            internal bool Qualified => Complete && Accepted == FrameCount && CorrectObstacles == FrameCount &&
                Obstacles >= 8 && PeakErrorMm < 38f && MinimumConfidence >= .75f && PeakSkew <= MaxSkewUs;
        }

        internal void ForgetCalibration() => Calibrated = false;

        internal void Calibrate()
        {
            Array.Clear(calibratedPixels, 0, calibratedPixels.Length);
            rangeBias = yawBias = 0;
            // Calibration fixture: surveyed target on-axis at 1.6 m, platform at rest.
            for (int sample = 0; sample < 12; sample++)
            {
                for (int i = 0; i < 4; i++)
                    calibratedPixels[i] += (Project(i, 0, 1.6f, sample) + FocalPixels * Positions[i] / 1.6f) / 12f;
                rangeBias += (Lidar(1.6f, sample) - 1.6f) / 12f;
                yawBias += Imu(0, sample) / 12f;
            }
            Calibrated = true;
        }

        internal static float TriggerSkew(int frame, bool trimmed) =>
            89f + 34f * Mathf.Sin(frame * .63f) + (trimmed ? 0 : frame * 6.8f + 280f);

        internal void Step(Replay replay, bool[] cameras, bool clock, bool trimmed, bool lidarReady, bool imuReady,
            float idleRail, float pulseRail, InjectedFrame injection = InjectedFrame.None)
        {
            if (replay.Complete) return;
            int frame = replay.Sent++;
            float depth = frame % 20 < 6 ? .69f + .052f * Mathf.Cos(frame * .85f) :
                1.49f + .45f * Mathf.Sin(frame * .19f);
            float lateral = .13f * Mathf.Sin(frame * .43f);
            bool obstacle = depth < .90f && Mathf.Abs(lateral) < .22f;
            if (obstacle) replay.Obstacles++;
            float yaw = .086f * Mathf.Sin(frame * .22f);
            float localX = lateral * Mathf.Cos(yaw) - depth * Mathf.Sin(yaw);
            float localZ = depth * Mathf.Cos(yaw) + lateral * Mathf.Sin(yaw);
            float trigger = TriggerSkew(frame, trimmed);
            // A shared integer acquisition epoch bounds per-device timestamp deltas.
            long epochUs = 23000000L + frame * 20000L;
            long cameraB = epochUs + (long)trigger + (injection == InjectedFrame.Stale ? 21000L : 0L);
            float skew = cameraB - epochUs;
            replay.PeakSkew = Mathf.Max(replay.PeakSkew, Mathf.Abs(skew));
            replay.MinimumRail = Mathf.Min(replay.MinimumRail, pulseRail);
            replay.PeakSag = Mathf.Max(replay.PeakSag, idleRail - pulseRail);
            bool clockValid = clock && skew <= MaxSkewUs;
            bool ranged = lidarReady && pulseRail >= 4.52f;
            if (!cameras[1]) replay.MissingB++;
            if (!clockValid) replay.Stale++;
            if (!ranged) replay.RangeDrops++;
            int sequence = injection == InjectedFrame.Duplicate && frame > 0 ? 0 : frame;
            ulong bit = 1UL << sequence;
            bool duplicate = (replay.SeenSequences & bit) != 0;
            replay.SeenSequences |= bit;
            if (duplicate) replay.Duplicates++;

            uint payload = unchecked((uint)(sequence + 1) * 0x9e3779b9u);
            uint packetCrc = Crc(payload, sequence);
            uint observedCrc = Crc(injection == InjectedFrame.Corrupt ? payload ^ 0x00000100u : payload, sequence);
            bool crcOk = packetCrc == observedCrc;
            if (!crcOk) replay.CrcFailures++;
            if (!Calibrated || !clockValid || !ranged || !imuReady || duplicate || !crcOk ||
                !cameras[0] || !cameras[1] || !cameras[2] || !cameras[3])
            {
                replay.Rejected++;
                replay.Last = new Reading(0, 0, 0, skew, false, false);
                return;
            }

            float a = Project(0, localX, localZ, frame) - calibratedPixels[0];
            float b = Project(1, localX, localZ, frame) - calibratedPixels[1];
            float c = Project(2, localX, localZ, frame) - calibratedPixels[2];
            float d = Project(3, localX, localZ, frame) - calibratedPixels[3];
            if (a - b <= 1f || c - d <= 1f)
            { replay.Rejected++; replay.Last = default; return; }
            float stereoA = FocalPixels * .17f / (a - b);
            float stereoB = FocalPixels * .23f / (c - d);
            float range = Lidar(localZ, frame) - rangeBias;
            float agreement = Mathf.Max(Mathf.Abs(stereoA - range), Mathf.Abs(stereoB - range));
            float confidence = Mathf.Clamp01(1f - agreement * 4.5f - skew / 3500f - (idleRail - pulseRail) * .12f);
            float z = (stereoA * .28f + stereoB * .32f + range * .40f);
            float x = (a + b + c + d) * .25f * z / FocalPixels;
            float heading = Imu(yaw, frame) - yawBias;
            float fusedZ = z * Mathf.Cos(heading) - x * Mathf.Sin(heading);
            float fusedX = x * Mathf.Cos(heading) + z * Mathf.Sin(heading);
            bool valid = confidence >= .75f && agreement < .075f;
            bool foundObstacle = fusedZ < .90f && Mathf.Abs(fusedX) < .22f;
            replay.Last = new Reading(fusedZ, fusedX, confidence, skew, valid, foundObstacle);
            if (!valid) { replay.Rejected++; return; }
            replay.Accepted++;
            if (foundObstacle == obstacle) replay.CorrectObstacles++;
            replay.MinimumConfidence = Mathf.Min(replay.MinimumConfidence, confidence);
            float dx = fusedX - lateral, dz = fusedZ - depth;
            replay.PeakErrorMm = Mathf.Max(replay.PeakErrorMm, Mathf.Sqrt(dx * dx + dz * dz) * 1000f);
        }

        private static float Project(int camera, float lateral, float depth, int frame) =>
            FocalPixels * (lateral - Positions[camera]) / depth + PixelBias[camera] + .14f * Mathf.Sin(frame * .51f + camera * 1.27f);
        private static float Lidar(float depth, int frame) => depth + .023f + .0038f * Mathf.Sin(frame * .72f);
        private static float Imu(float yaw, int frame) => yaw + .011f + .0012f * Mathf.Sin(frame * .43f);

        private static uint Crc(uint payload, int sequence)
        {
            ulong input = ((ulong)payload << 32) | (uint)sequence;
            uint crc = 0xffffffffu;
            for (int i = 0; i < 8; i++)
            {
                crc ^= (byte)(input >> (i * 8));
                for (int bit = 0; bit < 8; bit++)
                    crc = (crc >> 1) ^ ((crc & 1u) != 0 ? 0xedb88320u : 0u);
            }
            return crc ^ 0xffffffffu;
        }
    }
}
