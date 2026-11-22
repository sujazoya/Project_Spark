using UnityEngine;
using ProjectSpark.Level125;

namespace ProjectSpark.Gameplay.SignalHorizon
{
    internal sealed class SparkLevel125Link
    {
        internal const int FrameCount = 64;
        internal const float SkewLimitNs = 125f;
        internal const float CurrentLimit = .30f;
        private const float FrequencyMHz = 2200f;
        private const float FrameSpacingSeconds = .05f;
        private const float MaximumFrameAge = .13f;

        internal enum Segment { Nominal, Weak, Failover, Recovery }
        internal enum Hold { None, Timing, Margin, Overcurrent, Duplicate, OldFrame, Uncorrectable }
        internal enum Challenge { None, Duplicate, Stale, Corrupt }

        internal readonly struct Inputs
        {
            internal readonly bool ReceiverA, ReceiverB, TransmitterA, TransmitterB, Trim, Overcurrent;
            internal readonly float FilterVoltage;
            internal Inputs(bool receiverA, bool receiverB, bool transmitterA, bool transmitterB,
                bool trim, bool overcurrent, float filterVoltage)
            {
                ReceiverA = receiverA; ReceiverB = receiverB;
                TransmitterA = transmitterA; TransmitterB = transmitterB;
                Trim = trim; Overcurrent = overcurrent; FilterVoltage = filterVoltage;
            }

            internal bool Ready => ReceiverA && ReceiverB && TransmitterA && TransmitterB &&
                Trim && !Overcurrent && FilterVoltage > 4.60f;
        }

        internal struct Sample
        {
            internal Segment Phase;
            internal Hold Reason;
            internal int Receiver, Tx;
            internal float Margin, SkewNs, DistanceKm;
        }

        internal sealed class Replay
        {
            internal int Sent, Received, Accepted, Corrected, LowMargin, BackupFrames, Switchovers;
            internal int StaleClock, NoisyFilter, CurrentHolds;
            internal ulong Seen;
            internal int Preferred, MissingPrimary, HealthyPrimary;
            internal float MinimumMargin = float.PositiveInfinity;
            internal Sample Last;
            internal bool Complete => Sent == FrameCount;
            internal bool Qualified => Complete && Accepted == FrameCount && Corrected >= 8 &&
                LowMargin == 16 && BackupFrames == 17 && Switchovers == 2 && MinimumMargin > 2f;
        }

        internal bool Calibrated { get; private set; }

        internal bool Calibrate()
        {
            // Reference vector from the exercise and the standard CRC-16/CCITT-FALSE check text.
            ushort crc = 0xFFFF;
            foreach (char digit in "123456789") crc = UpdateCrc(crc, (byte)digit);
            return Calibrated = crc == 0x29B1 && SparkL125LinkTeachingDemo.TryPathLoss(
                FrequencyMHz, 100000f, 30f, 12f, 55f, 3f, -120f, out _, out _, out float margin) &&
                Mathf.Abs(margin - 14.71f) < .03f;
        }

        internal void ClearCalibration() => Calibrated = false;

        internal static float ClockSkew(int frame, bool trim) => trim ?
            41f + 19f * Mathf.Abs(Mathf.Sin(frame * .63f)) : 218f + 57f * Mathf.Abs(Mathf.Sin(frame * .78f));

        internal void Advance(Replay replay, Inputs inputs, Challenge challenge = Challenge.None)
        {
            int i = replay.Sent++;
            var phase = (Segment)(i / 16);
            float distance = phase switch
            {
                Segment.Weak => 270000f + (i % 16) * 3450f,
                Segment.Failover => 198000f + (i % 16) * 1500f,
                _ => 100000f + (i % 16) * 1450f
            };
            bool validMargin = SparkL125LinkTeachingDemo.TryPathLoss(FrequencyMHz, distance, 30f, 12f, 55f, 3f, -120f,
                out _, out _, out float margin);
            float noiseLoss = Mathf.Max(0f, 5f - inputs.FilterVoltage) * 11f;
            float backupMargin = margin - 1.5f - noiseLoss;
            bool primary = inputs.ReceiverA && phase != Segment.Failover && validMargin && margin > 2f;
            bool backup = inputs.ReceiverB && validMargin && backupMargin > 2f;

            // Receive on the healthy path while the preferred route changes only after two samples.
            replay.MissingPrimary = !primary && backup ? replay.MissingPrimary + 1 : 0;
            replay.HealthyPrimary = primary ? replay.HealthyPrimary + 1 : 0;
            int preferred = replay.MissingPrimary >= 2 ? 1 : replay.HealthyPrimary >= 2 ? 0 : replay.Preferred;
            if (preferred != replay.Preferred)
            {
                replay.Preferred = preferred;
                replay.Switchovers++;
            }
            int receiver = replay.Preferred == 1 && backup ? 1 : primary ? 0 : backup ? 1 : -1;
            float activeMargin = receiver == 1 ? backupMargin : margin;
            float skew = ClockSkew(i, inputs.Trim);
            if (skew > SkewLimitNs) replay.StaleClock++;
            if (inputs.ReceiverB && noiseLoss > 6f && backupMargin <= 2f) replay.NoisyFilter++;
            if (inputs.Overcurrent) replay.CurrentHolds++;
            if (phase == Segment.Weak && activeMargin is > 2f and < 6.5f) replay.LowMargin++;

            Hold reason = !Calibrated || skew > SkewLimitNs ? Hold.Timing : receiver < 0 ? Hold.Margin : Hold.None;
            int tx = -1;
            if (reason == Hold.None)
            {
                uint payload = (uint)(i | ((i * 73 + 17) & 255) << 8 | ((i * 19 + 81) & 255) << 16);
                ulong packet = payload | (ulong)Checksum(payload) << 24;
                ulong first = packet, second = packet, third = packet;
                if (phase == Segment.Weak && (i % 2 == 0) || i is 4 or 45) first ^= 1UL << (i % 39);
                if (challenge == Challenge.Corrupt) { first ^= 0x300UL; second ^= 0x300UL; }
                float timestamp = i * FrameSpacingSeconds;
                float age = challenge == Challenge.Stale ? .40f : .04f;
                if (challenge == Challenge.Duplicate) replay.Seen |= 1UL << i;
                reason = Receive(replay, first, second, third, timestamp, timestamp + age);
            }
            if (reason == Hold.None)
            {
                replay.Received++;
                if (receiver == 1) replay.BackupFrames++;
                if (inputs.Overcurrent) reason = Hold.Overcurrent;
                else if ((receiver == 0 ? inputs.TransmitterA : inputs.TransmitterB))
                {
                    tx = receiver;
                    if (inputs.Ready)
                    {
                        replay.Accepted++;
                        replay.MinimumMargin = Mathf.Min(replay.MinimumMargin, activeMargin);
                    }
                }
                else reason = Hold.Margin;
            }

            replay.Last = new Sample
            {
                Phase = phase, Reason = reason, Receiver = receiver, Tx = tx,
                Margin = activeMargin, SkewNs = skew, DistanceKm = distance
            };
        }

        private static Hold Receive(Replay replay, ulong a, ulong b, ulong c, float stamp, float now)
        {
            if (now - stamp > MaximumFrameAge) return Hold.OldFrame;
            ulong majority = a & b | a & c | b & c;
            uint payload = (uint)(majority & 0xFFFFFFu);
            if (Checksum(payload) != (ushort)(majority >> 24)) return Hold.Uncorrectable;
            ulong changed = (a ^ majority) | (b ^ majority) | (c ^ majority);
            if ((changed & (changed - 1)) != 0) return Hold.Uncorrectable;
            int sequence = (int)(payload & 255);
            if (sequence >= FrameCount) return Hold.Uncorrectable;
            ulong flag = 1UL << sequence;
            if ((replay.Seen & flag) != 0) return Hold.Duplicate;
            replay.Seen |= flag;
            if (changed != 0) replay.Corrected++;
            return Hold.None;
        }

        private static ushort Checksum(uint payload)
        {
            ushort crc = 0xFFFF;
            for (int offset = 0; offset < 24; offset += 8) crc = UpdateCrc(crc, (byte)(payload >> offset));
            return crc;
        }

        private static ushort UpdateCrc(ushort value, byte data)
        {
            int crc = value ^ data << 8;
            for (int i = 0; i < 8; i++) crc = (crc & 0x8000) != 0 ? (crc << 1) ^ 0x1021 : crc << 1;
            return (ushort)crc;
        }
    }
}
