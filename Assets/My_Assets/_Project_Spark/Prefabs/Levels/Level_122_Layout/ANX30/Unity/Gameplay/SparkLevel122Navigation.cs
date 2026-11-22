using UnityEngine;

namespace ProjectSpark.Gameplay.AutonomySentinel
{
    internal sealed class SparkLevel122Navigation
    {
        internal const int FrameCount = 64;
        internal const float MaximumPhaseNs = 62f;
        private const float StepSeconds = .1f;
        private const float CommandTimeout = .25f;
        private const float Geofence = 9.30f;
        private const float RobotRadius = .28f;
        private const float MaxRadius = .48f;

        internal enum Hold { None, Obstacle, Boundary, StaleCommand, SatelliteIntegrity, RadarQuality, ImuDivergence, Uncertain }
        internal enum Challenge { None, Spoof, Stale, ImuSplit }
        internal enum Zone { Outdoor, Indoor, Exit }

        internal readonly struct Inputs
        {
            internal readonly bool GnssA, GnssB, RadarA, RadarB, ImuA, ImuB, Clock, Trim, Reference;
            internal Inputs(bool gnssA, bool gnssB, bool radarA, bool radarB, bool imuA, bool imuB,
                bool clock, bool trim, bool reference)
            {
                GnssA = gnssA; GnssB = gnssB; RadarA = radarA; RadarB = radarB;
                ImuA = imuA; ImuB = imuB; Clock = clock; Trim = trim; Reference = reference;
            }

            internal bool Ready => GnssA && GnssB && RadarA && RadarB && ImuA && ImuB && Clock && Trim && Reference;
        }

        internal struct Sample
        {
            internal Zone Area;
            internal Hold Reason;
            internal bool Trusted;
            internal bool Move => Trusted && Reason == Hold.None;
            internal float Position, Yaw, Sigma, RadarRange, Clearance, BrakeDistance, PhaseNs, ImuDelta, Error, Confidence;
        }

        internal sealed class Replay
        {
            internal int Sent, Trusted, SafeDecisions, BrakeHolds, BoundaryHolds, AgedCommands, Indoor, MissingA, BadRadar, Diverged;
            internal float MaxError, MaxUncertainty, PeakPhase, PeakImuDelta;
            internal float Position, Yaw, Variance;
            internal Sample Last;
            internal bool Complete => Sent >= FrameCount;
            internal bool Qualified => Complete && Trusted == FrameCount && SafeDecisions == FrameCount &&
                BrakeHolds >= 8 && BoundaryHolds >= 5 && AgedCommands >= 4 && Indoor >= 16 &&
                MaxError < .22f && MaxUncertainty < MaxRadius;
        }

        private float receiverAOffset, receiverBOffset, radarAOffset, radarBOffset, imuAOffset, imuBOffset;
        internal bool Calibrated { get; private set; }

        internal void ClearCalibration() => Calibrated = false;

        internal void Calibrate()
        {
            // The reference fixture supplies known positions, reflector range and a stationary heading.
            // Average symmetric readings so the channel offsets come from the fixture.
            receiverAOffset = ReferenceMean(.037f);
            receiverBOffset = ReferenceMean(-.049f);
            radarAOffset = ReferenceMean(.024f);
            radarBOffset = ReferenceMean(-.031f);
            imuAOffset = ReferenceMean(.018f);
            imuBOffset = ReferenceMean(-.014f);
            Calibrated = true;
        }

        private static float ReferenceMean(float bias)
        {
            float total = 0;
            for (int i = 0; i < 8; i++) total += bias + Mathf.Sin(i * Mathf.PI / 4f) * .005f;
            return total / 8f;
        }

        internal static float Phase(int index, bool trim) =>
            trim ? 22f + 7f * Mathf.Abs(Mathf.Sin(index * .73f)) :
            124f + 47f * Mathf.Abs(Mathf.Sin(index * .83f));

        internal static bool GnssDropout(int index) => index % 5 is 1 or 2;
        private static bool Outside(int index) => index < 16 || index >= 48;

        internal void Advance(Replay replay, Inputs inputs, Challenge challenge = Challenge.None)
        {
            int i = replay.Sent++;
            bool outside = Outside(i);
            bool occlusion = i is >= 24 and < 28;
            bool crossing = i is >= 16 and < 32;
            bool approach = i is >= 40 and < 48;
            float position = i < 48 ? .65f + i * .115f : 6.8f + (i - 48) * .147f;
            float speed = .92f + .44f * Mathf.Sin(i * .126f);
            float yaw = .28f * Mathf.Sin(i * .09f);
            float range = crossing ? .42f + .045f * (i % 4) : approach ? .64f + .035f * (i % 4) : 2.10f + .12f * Mathf.Cos(i * .22f);
            float obstacleSpeed = crossing ? .25f : approach ? .45f : 0f;
            float age = i % 13 == 12 ? .43f : .07f + (i % 4) * .019f;
            if (challenge == Challenge.Stale) age = .54f;

            float phase = inputs.Clock ? Phase(i, inputs.Trim) : 200f;
            float imuA = yaw + .018f - imuAOffset + .009f * Mathf.Sin(i * .91f);
            float imuB = yaw - .014f - imuBOffset + .008f * Mathf.Cos(i * .72f);
            if (!inputs.Reference || challenge == Challenge.ImuSplit) imuB += .34f;
            float imuDelta = Mathf.Abs(imuA - imuB);
            float a = position + .037f - receiverAOffset + .052f * Mathf.Sin(i * .67f);
            float b = position - .049f - receiverBOffset + .044f * Mathf.Cos(i * .45f);
            if (challenge == Challenge.Spoof) a += 2.4f;
            float variance = i == 0 ? .042f : replay.Variance + .006f;
            float predicted = i == 0 ? .70f : replay.Position +
                (i <= 47 ? .115f : i == 48 ? .745f : .147f) + .004f * Mathf.Sin(i);
            bool gnssValid = inputs.GnssA && inputs.GnssB && Mathf.Abs(a - b) < .20f &&
                Mathf.Abs(a - predicted) < .24f && Mathf.Abs(b - predicted) < .24f;
            bool radarClean = inputs.RadarA && inputs.RadarB && phase <= MaximumPhaseNs;
            float rangeA = range + .024f - radarAOffset + Mathf.Sin(i * .51f) * .024f;
            float rangeB = range - .031f - radarBOffset + Mathf.Cos(i * .64f) * .030f;
            if (!radarClean) rangeA += .47f;

            if (outside && gnssValid) Observe(ref predicted, ref variance, (a + b) * .5f, .031f);
            const float mappedReflector = 13f;
            float measuredReflector = mappedReflector - position + .067f * Mathf.Cos(i * .31f);
            if (inputs.RadarB && radarClean && !occlusion)
                Observe(ref predicted, ref variance, mappedReflector - measuredReflector, .064f);

            float uncertainty = 2f * Mathf.Sqrt(variance);
            float nearest = occlusion ? rangeB : Mathf.Min(rangeA, rangeB);
            float clearance = nearest - RobotRadius - .25f * uncertainty;
            float braking = StopDistance(speed, obstacleSpeed, age);
            Hold sensorHold = !Calibrated || uncertainty >= MaxRadius ? Hold.Uncertain : Hold.None;
            if (!radarClean || Mathf.Abs(rangeA - rangeB) > .16f) sensorHold = Hold.RadarQuality;
            if (outside && !gnssValid) sensorHold = Hold.SatelliteIntegrity;
            if (!inputs.ImuA || !inputs.ImuB || imuDelta >= .12f) sensorHold = Hold.ImuDivergence;

            var reason = sensorHold != Hold.None ? sensorHold : StopRule(age, predicted, uncertainty, clearance, braking);
            var truth = StopRule(age, position, .22f, range - RobotRadius - .055f, braking);
            if (reason == Hold.Obstacle) replay.BrakeHolds++;
            if (reason == Hold.Boundary) replay.BoundaryHolds++;
            if (reason == Hold.StaleCommand) replay.AgedCommands++;
            if (!outside) replay.Indoor++;
            if (outside && !inputs.GnssA) replay.MissingA++;
            if (!radarClean) replay.BadRadar++;
            if (!inputs.ImuA || !inputs.ImuB || imuDelta >= .12f) replay.Diverged++;
            if (sensorHold == Hold.None)
            {
                replay.Trusted++;
                if (reason == truth) replay.SafeDecisions++;
            }

            replay.Position = predicted;
            replay.Yaw = i == 0 ? (imuA + imuB) * .5f : Mathf.Lerp(replay.Yaw, (imuA + imuB) * .5f, .70f);
            replay.Variance = variance;
            replay.MaxError = Mathf.Max(replay.MaxError, Mathf.Abs(predicted - position));
            replay.MaxUncertainty = Mathf.Max(replay.MaxUncertainty, uncertainty);
            replay.PeakPhase = Mathf.Max(replay.PeakPhase, phase);
            replay.PeakImuDelta = Mathf.Max(replay.PeakImuDelta, imuDelta);
            replay.Last = new Sample
            {
                Area = outside ? i < 16 ? Zone.Outdoor : Zone.Exit : Zone.Indoor, Reason = reason,
                Trusted = sensorHold == Hold.None, Position = predicted, Yaw = replay.Yaw, Sigma = uncertainty,
                RadarRange = nearest, Clearance = clearance, BrakeDistance = braking, PhaseNs = phase,
                ImuDelta = imuDelta, Error = Mathf.Abs(predicted - position),
                Confidence = Mathf.Clamp01(1f - uncertainty * .39f)
            };
        }

        private static void Observe(ref float estimate, ref float variance, float observation, float measurementVariance)
        {
            float gain = variance / (variance + measurementVariance);
            estimate += gain * (observation - estimate);
            variance *= 1f - gain;
        }

        private static float StopDistance(float speed, float obstacleSpeed, float age)
        {
            const float deceleration = 2.6f;
            float reaction = Mathf.Min(age, CommandTimeout) + StepSeconds;
            return speed * reaction + speed * speed / (2f * deceleration) +
                obstacleSpeed * (reaction + speed / deceleration) + .12f;
        }

        private static Hold StopRule(float age, float position, float radius, float clearance, float braking)
        {
            if (age > CommandTimeout) return Hold.StaleCommand;
            if (position + braking + radius > Geofence) return Hold.Boundary;
            return clearance < braking ? Hold.Obstacle : Hold.None;
        }
    }
}
