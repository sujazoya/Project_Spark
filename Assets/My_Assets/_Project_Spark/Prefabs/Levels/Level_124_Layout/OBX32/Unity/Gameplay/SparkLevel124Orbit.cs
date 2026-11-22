using UnityEngine;

namespace ProjectSpark.Gameplay.OrbitalBastion
{
    internal sealed class SparkLevel124Orbit
    {
        internal const int FrameCount = 64;
        internal const float MaximumClockSkew = 140f;
        internal const float CurrentLimit = .30f;
        private const float Step = .08f;

        internal enum Phase { Acquire, Eclipse, Unload, SafeMode }
        internal enum Hold { None, TrackerStale, AttitudeInvalid, WheelCurrent, TachDisagreement, EccSafeMode }
        internal enum Challenge { None, TrackerStale, Overcurrent, Uncorrectable }
        private enum MemoryStatus { Clean, Corrected, Uncorrectable }

        internal readonly struct Inputs
        {
            internal readonly int Wheels;
            internal readonly bool A, B, Gyro, Sun, Tach, Overcurrent;
            internal readonly float Reference;
            internal Inputs(int wheels, bool a, bool b, bool gyro, bool sun, bool tach, bool overcurrent, float reference)
            {
                Wheels = wheels; A = a; B = b; Gyro = gyro; Sun = sun; Tach = tach;
                Overcurrent = overcurrent; Reference = reference;
            }
            internal bool Ready => Wheels == 4 && A && B && Gyro && Sun && Tach && !Overcurrent && Reference > 4.6f;
        }

        internal struct Sample
        {
            internal Phase Segment;
            internal Hold Reason;
            internal bool Desaturating;
            internal bool RequestGate => Reason == Hold.None;
            internal float PointingError, Momentum, Confidence;
        }

        internal sealed class Replay
        {
            internal int Sent, Trusted, StaleB, OvercurrentHolds, TachHolds, EccCorrected, EccSafeHolds, DumpSamples;
            internal float BeforeDump, AfterDump, PeakTachDifference, PointingEnd, PointingDuringEclipse;
            internal readonly float[] WheelMomentum = { .44f, .53f, .82f, .64f };
            internal Vector3 Attitude = new(-.34f, .23f, -.12f);
            internal Vector3 Rate, Estimate;
            internal bool SafeMode;
            internal Sample Last;
            internal bool Complete => Sent >= FrameCount;
            internal bool Qualified => Complete && Trusted == FrameCount && EccCorrected == 2 && EccSafeHolds == 6 &&
                DumpSamples == 16 && BeforeDump > .72f && AfterDump < .36f &&
                PointingEnd < .09f && PointingDuringEclipse < .20f;
        }

        // Small body-axis rotation coordinates in radians, with right-handed local X/Y/Z.
        // The fixture stays inside the small-angle range throughout a successful trial.
        private static readonly Vector3[] WheelAxes =
        {
            new(.57735f, .57735f, .57735f), new(.57735f, -.57735f, -.57735f),
            new(-.57735f, .57735f, -.57735f), new(-.57735f, -.57735f, .57735f)
        };

        private float trackerAZero, trackerBZero;
        internal bool Calibrated { get; private set; }
        internal void ClearCalibration() => Calibrated = false;

        internal void Calibrate()
        {
            trackerAZero = ReferenceMean(.019f);
            trackerBZero = ReferenceMean(-.023f);
            Calibrated = true;
        }

        private static float ReferenceMean(float bias)
        {
            float total = 0f;
            for (int i = 0; i < 8; i++) total += bias + .006f * Mathf.Sin(i * Mathf.PI / 4f);
            return total / 8f;
        }

        internal static bool TrackerDropout(int sample) => sample % 5 is 1 or 2;
        internal static float ClockSkew(int sample) => 52f + 21f * Mathf.Abs(Mathf.Sin(sample * .64f));

        internal void Advance(Replay replay, Inputs inputs, Challenge challenge = Challenge.None)
        {
            int i = replay.Sent++;
            var phase = (Phase)(i / 16);
            bool trackerB = inputs.B && challenge != Challenge.TrackerStale;
            bool sunVisible = inputs.Sun && phase != Phase.Eclipse;
            var target = new Vector3(.22f, -.10f, .065f);
            Vector3 prior = i == 0 ? replay.Attitude : replay.Estimate + replay.Rate * Step;
            Vector3 a = replay.Attitude + new Vector3(.019f - trackerAZero + .008f * Mathf.Sin(i * .79f), .005f * Mathf.Cos(i), 0f);
            Vector3 b = replay.Attitude + new Vector3(-.023f - trackerBZero + .009f * Mathf.Cos(i * .66f), 0f, .006f * Mathf.Sin(i));
            float trackDifference = (a - b).magnitude;
            bool synchronized = inputs.A && trackerB && trackDifference < .06f;
            if (inputs.A) prior += (a - prior) * .43f;
            if (trackerB) prior += (b - prior) * .38f;
            if (sunVisible) prior += (replay.Attitude + new Vector3(.006f, 0f, 0f) - prior) * .09f;
            replay.Estimate = prior;

            float independentSpeed = replay.WheelMomentum[2] + .013f * Mathf.Sin(i * .57f);
            float tach = replay.WheelMomentum[2] + .009f * Mathf.Cos(i * .62f) +
                Mathf.Max(0, 5f - inputs.Reference) * .31f;
            float speedDifference = Mathf.Abs(tach - independentSpeed);
            bool tachValid = inputs.Tach && speedDifference < .20f;
            bool overcurrent = inputs.Overcurrent || challenge == Challenge.Overcurrent;
            int payload = (i * 7 + 3) & 15;
            int encoded = Encode(payload);
            if (i is 49 or 52) encoded ^= 1 << (i % 8);
            if (i == 58 || challenge == Challenge.Uncorrectable) encoded ^= 0b00100100;
            MemoryStatus memory = Read(ref encoded, out int decoded);
            if (memory == MemoryStatus.Corrected && decoded == payload) replay.EccCorrected++;
            if (memory == MemoryStatus.Uncorrectable) replay.SafeMode = true;

            Hold reason = overcurrent ? Hold.WheelCurrent : !trackerB ? Hold.TrackerStale :
                !Calibrated || !synchronized || !inputs.Gyro || inputs.Wheels != 4 ? Hold.AttitudeInvalid :
                !tachValid ? Hold.TachDisagreement : replay.SafeMode ? Hold.EccSafeMode : Hold.None;
            var demand = (target - replay.Estimate) * 1.22f - replay.Rate * .72f;
            var torque = Vector3.zero;
            float largest = 0f;
            bool unloading = phase == Phase.Unload && reason == Hold.None;
            for (int wheel = 0; wheel < WheelAxes.Length; wheel++)
            {
                var axis = WheelAxes[wheel];
                float effort = reason == Hold.None ? Mathf.Clamp(Vector3.Dot(axis, demand), -.52f, .52f) : 0f;
                torque += axis * effort;
                replay.WheelMomentum[wheel] = Mathf.Clamp01(replay.WheelMomentum[wheel] + .0008f + Mathf.Abs(effort) * .009f -
                    (unloading ? .043f : 0f));
                largest = Mathf.Max(largest, replay.WheelMomentum[wheel]);
            }

            var disturbance = i is >= 21 and < 25 ? new Vector3(.18f, -.09f, .06f) : Vector3.zero;
            replay.Rate += (torque * 2.6f - replay.Rate * 2.15f + disturbance) * Step;
            replay.Attitude += replay.Rate * Step;
            float pointing = (target - replay.Attitude).magnitude;
            if (i == 31) replay.BeforeDump = largest;
            if (i == 47) replay.AfterDump = largest;
            if (i >= 24 && i < 32) replay.PointingDuringEclipse = Mathf.Max(replay.PointingDuringEclipse, pointing);
            if (unloading) replay.DumpSamples++;
            if (reason == Hold.EccSafeMode) replay.EccSafeHolds++;
            if (!trackerB) replay.StaleB++;
            if (overcurrent) replay.OvercurrentHolds++;
            if (!tachValid) replay.TachHolds++;
            if (inputs.Ready && (reason is Hold.None or Hold.EccSafeMode)) replay.Trusted++;
            replay.PeakTachDifference = Mathf.Max(replay.PeakTachDifference, speedDifference);
            replay.PointingEnd = pointing;
            replay.Last = new Sample
            {
                Segment = phase, Reason = reason, Desaturating = unloading,
                PointingError = pointing, Momentum = largest,
                Confidence = !inputs.A || !inputs.Gyro ? 0f :
                    synchronized ? Mathf.Clamp01(.99f - 2.5f * trackDifference - (sunVisible ? 0f : .06f)) : .60f
            };
        }

        // Extended Hamming (8,4) on a virtual four-bit status word. The eighth
        // bit distinguishes a correctable single-bit error from a detected double error.
        private static int Encode(int value)
        {
            int word = (value & 1) << 2 | (value & 14) << 3;
            for (int parity = 1; parity <= 4; parity <<= 1)
                if (Parity(word, parity) != 0) word |= 1 << (parity - 1);
            if (Parity(word, 0) != 0) word |= 128;
            return word;
        }

        private static MemoryStatus Read(ref int word, out int value)
        {
            int syndrome = Parity(word, 1) + 2 * Parity(word, 2) + 4 * Parity(word, 4);
            bool odd = Parity(word, 0) != 0;
            value = 0;
            if (syndrome != 0 && !odd) return MemoryStatus.Uncorrectable;
            if (odd) word ^= 1 << (syndrome == 0 ? 7 : syndrome - 1);
            value = (word >> 2 & 1) | (word >> 3 & 14);
            return odd ? MemoryStatus.Corrected : MemoryStatus.Clean;
        }

        private static int Parity(int word, int column)
        {
            int value = 0;
            for (int position = 1; position <= 8; position++)
                if (column == 0 || (position & column) != 0) value ^= word >> (position - 1) & 1;
            return value;
        }
    }
}
