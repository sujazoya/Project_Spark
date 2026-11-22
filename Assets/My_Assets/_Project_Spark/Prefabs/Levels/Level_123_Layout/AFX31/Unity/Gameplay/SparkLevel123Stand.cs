using UnityEngine;

namespace ProjectSpark.Gameplay.FlightSovereign
{
    internal sealed class SparkLevel123Stand
    {
        internal const int FrameCount = 64;
        internal const float MaximumClockSkew = 175f;
        private const float StepSeconds = .05f;
        internal const float CurrentCeiling = 1.7f;

        internal enum Phase { Spinup, Gust, Descent, Limits }
        internal enum Challenge { None, CommonImuBias, Overload, EmergencyCut }
        internal enum Hold { None, DriveOpen, StaleImu, ImuDisagreement, Altitude, Current, Emergency }

        internal readonly struct Inputs
        {
            internal readonly int Motors;
            internal readonly bool ImuA, ImuB, ImuC, Barometer, GnssA, GnssB;
            internal readonly float Reference;
            internal Inputs(int motors, bool imuA, bool imuB, bool imuC, bool barometer, bool gnssA, bool gnssB, float reference)
            {
                Motors = motors;
                ImuA = imuA; ImuB = imuB; ImuC = imuC; Barometer = barometer;
                GnssA = gnssA; GnssB = gnssB; Reference = reference;
            }
            internal bool Ready => Motors == 255 && ImuA && ImuB && ImuC && Barometer && GnssA && GnssB && Reference > 4.60f;
        }

        internal struct Sample
        {
            internal Phase Segment;
            internal Hold Reason;
            internal bool RequestGate => Reason == Hold.None;
            internal float Height, Attitude, PeakDemand, Mix, VoterConfidence;
        }

        internal sealed class Replay
        {
            internal readonly float[] Speed = new float[8];
            internal readonly float[] Current = new float[8];
            internal int Sent, QualifiedSamples, MissingMotor, StaleB, AltitudeHolds, OvercurrentCuts, EmergencyCuts, Recovered;
            internal float Height, VerticalSpeed, PeakAngle, GustPeak, GustEnd, PeakAltitudeError, DescentStart, LastHeight, ResidualSpeed;
            internal Vector3 Attitude, Rates;
            internal Sample Last;
            internal bool Complete => Sent >= FrameCount;
            internal bool Qualified => Complete && QualifiedSamples == FrameCount &&
                OvercurrentCuts == 4 && EmergencyCuts == 4 && Recovered >= 2 &&
                PeakAngle < .38f && GustPeak > .015f && GustEnd < GustPeak &&
                DescentStart > .23f && LastHeight < .12f && PeakAltitudeError < .11f && ResidualSpeed < .05f;
        }

        private readonly float[] imuOffsets = new float[3];
        private float barometerOffset, gnssAOffset, gnssBOffset;
        internal bool Calibrated { get; private set; }
        internal void ClearCalibration() => Calibrated = false;

        internal void Calibrate()
        {
            imuOffsets[0] = ReferenceMean(.022f);
            imuOffsets[1] = ReferenceMean(-.018f);
            imuOffsets[2] = ReferenceMean(.011f);
            barometerOffset = ReferenceMean(.041f);
            gnssAOffset = ReferenceMean(-.027f);
            gnssBOffset = ReferenceMean(.030f);
            Calibrated = true;
        }

        private static float ReferenceMean(float bias)
        {
            float sum = 0;
            for (int i = 0; i < 8; i++) sum += bias + Mathf.Sin(i * Mathf.PI / 4f) * .006f;
            return sum / 8f;
        }

        internal static float ClockSkew(int sample) => 77f + 24f * Mathf.Abs(Mathf.Sin(sample * .69f));
        internal static bool ImuDropout(int sample) => sample % 5 is 1 or 2;

        internal void Advance(Replay replay, Inputs inputs, Challenge challenge = Challenge.None)
        {
            int i = replay.Sent++;
            var phase = (Phase)(i / 16);
            bool disturbance = i is >= 17 and < 23;
            bool overload = i is >= 49 and < 53 || challenge == Challenge.Overload;
            bool stop = i >= 60 || challenge == Challenge.EmergencyCut;
            float heightTarget = i < 16 ? Mathf.Lerp(0f, .68f, i / 15f) :
                i < 32 ? .68f : i < 48 ? Mathf.Lerp(.68f, .03f, (i - 32) / 15f) : 0f;
            Vector3 command = new(0f, 0f, .045f * Mathf.Sin(i * .18f));
            if (i is >= 9 and < 17) command.x = .07f;

            float axis = replay.Attitude.x;
            float imuA = axis + .022f - imuOffsets[0] + .008f * Mathf.Sin(i * .8f);
            float imuB = axis - .018f - imuOffsets[1] + .009f * Mathf.Cos(i * .65f);
            float imuC = axis + .011f - imuOffsets[2] + .006f * Mathf.Sin(i * .91f + .4f);
            if (challenge == Challenge.CommonImuBias) { imuA += .32f; imuB += .32f; imuC += .32f; }
            float imuEstimate = inputs.ImuB ? (imuA + imuB + imuC) / 3f : (imuA + imuC) * .5f;
            float disagreement = Mathf.Max(Mathf.Abs(imuA - imuC),
                inputs.ImuB ? Mathf.Max(Mathf.Abs(imuA - imuB), Mathf.Abs(imuB - imuC)) : 0f);
            float fixtureTilt = axis + .010f * Mathf.Sin(i * .53f);
            bool inertialAgreement = inputs.ImuA && inputs.ImuC && disagreement < .11f &&
                Mathf.Abs(imuEstimate - fixtureTilt) < .14f;

            float baro = replay.Height + .041f - barometerOffset +
                .018f * Mathf.Sin(i * .78f) + Mathf.Max(0f, 5f - inputs.Reference) * .35f;
            float gnssA = replay.Height - .027f - gnssAOffset + .024f * Mathf.Cos(i * .51f);
            float gnssB = replay.Height + .030f - gnssBOffset + .029f * Mathf.Sin(i * .31f);
            float gnss = (gnssA + gnssB) * .5f;
            float heightAgreement = Mathf.Abs(baro - gnss);
            bool altitudeValid = inputs.Barometer && inputs.GnssA && inputs.GnssB &&
                Mathf.Abs(gnssA - gnssB) < .095f && heightAgreement < .18f;

            var estimatedTilt = new Vector3(imuEstimate, replay.Attitude.y, replay.Attitude.z);
            var attitudeDemand = (command - estimatedTilt) * 1.18f - replay.Rates * .44f;
            float collective = Mathf.Clamp(.47f + (heightTarget - (baro * .55f + gnss * .45f)) * .70f -
                replay.VerticalSpeed * .31f, .10f, .78f);
            float current = 0f;
            float effortSum = 0f;
            var torque = Vector3.zero;
            Hold reason = !Calibrated || !inertialAgreement ? Hold.ImuDisagreement :
                !inputs.ImuB ? Hold.StaleImu : !altitudeValid ? Hold.Altitude :
                inputs.Motors != 255 ? Hold.DriveOpen : Hold.None;

            for (int motor = 0; motor < replay.Speed.Length; motor++)
            {
                float arm = motor * Mathf.PI / 4f;
                var axes = new Vector3(Mathf.Sin(arm), Mathf.Cos(arm), motor % 2 == 0 ? .40f : -.40f);
                bool circuit = (inputs.Motors & (1 << motor)) != 0;
                float effort = circuit ? Mathf.Clamp01(collective + Vector3.Dot(attitudeDemand, axes)) : 0f;
                float draw = circuit ? .09f + effort * effort * 1.24f : 0f;
                if (overload && motor == 2 && circuit) draw += 2.1f;
                replay.Current[motor] = draw;
                current = Mathf.Max(current, draw);
                replay.Speed[motor] = Mathf.Lerp(replay.Speed[motor], stop || overload ? 0f : effort, .54f);
                effortSum += replay.Speed[motor];
                torque += axes * replay.Speed[motor];
            }
            if (stop) reason = Hold.Emergency;
            else if (current > CurrentCeiling) reason = Hold.Current;

            var gust = disturbance ? new Vector3(.95f, -.38f, .21f) : Vector3.zero;
            replay.Rates += (torque * 2.8f - replay.Rates * 2.1f + gust) * StepSeconds;
            replay.Attitude += replay.Rates * StepSeconds;
            float mean = effortSum / replay.Speed.Length;
            float lift = (mean - .47f) * 11f - replay.VerticalSpeed * 2.25f;
            replay.VerticalSpeed = Mathf.Clamp(replay.VerticalSpeed + lift * StepSeconds, -1.0f, 1.4f);
            replay.Height = Mathf.Max(0f, replay.Height + replay.VerticalSpeed * StepSeconds);
            if (replay.Height <= 0f) replay.VerticalSpeed = 0f;

            if (inputs.Motors != 255) replay.MissingMotor++;
            if (!inputs.ImuB) replay.StaleB++;
            if (!altitudeValid) replay.AltitudeHolds++;
            if (overload && reason == Hold.Current) replay.OvercurrentCuts++;
            if (stop && reason == Hold.Emergency)
            {
                replay.EmergencyCuts++;
                replay.ResidualSpeed = 0f;
                foreach (float speed in replay.Speed) replay.ResidualSpeed = Mathf.Max(replay.ResidualSpeed, speed);
            }
            if (inputs.Ready && altitudeValid && inertialAgreement && (reason == Hold.None || overload && reason == Hold.Current ||
                stop && reason == Hold.Emergency))
                replay.QualifiedSamples++;
            float tilt = replay.Attitude.magnitude;
            if (i is >= 17 and < 31) replay.GustPeak = Mathf.Max(replay.GustPeak, tilt);
            if (i == 31) replay.GustEnd = tilt;
            if (i is >= 29 and <= 31 && tilt < .10f) replay.Recovered++;
            if (i == 32) replay.DescentStart = replay.Height;
            replay.PeakAngle = Mathf.Max(replay.PeakAngle, tilt);
            replay.PeakAltitudeError = Mathf.Max(replay.PeakAltitudeError, heightAgreement);
            replay.LastHeight = replay.Height;
            replay.Last = new Sample
            {
                Segment = phase, Reason = reason, Height = replay.Height, Attitude = tilt,
                PeakDemand = current, Mix = mean,
                VoterConfidence = inertialAgreement ? inputs.ImuB ? 1f - disagreement : .67f : 0f
            };
        }
    }
}
