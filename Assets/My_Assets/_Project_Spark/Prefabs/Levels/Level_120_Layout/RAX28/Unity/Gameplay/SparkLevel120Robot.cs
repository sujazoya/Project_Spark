using UnityEngine;

namespace ProjectSpark.Gameplay.AxisForge
{
    // Virtual mechanism and averaged actuator model. All electrical permissions
    // arrive from the level's native solved contacts.
    internal sealed class SparkLevel120Robot
    {
        internal const float Dt = .005f, PathSeconds = 3.95f, HomeSeconds = 1.40f;
        private const float BaseHeight = .31f, Upper = .42f, Fore = .34f, Tool = .085f;
        private static readonly float[] Park = { -8f, 38f, -68f, 7f, -8f, 12f };
        private static readonly float[] Home = { 0f, 46f, -75f, 0f, 10f, 0f };
        private static readonly float[] PointA = { 23f, 56f, -77f, 28f, -10f, 18f };
        private static readonly float[] PointB = { -22f, 37f, -64f, -21f, 22f, -17f };
        private static readonly float[] Min = { -108f, 6f, -132f, -105f, -78f, -120f };
        private static readonly float[] Max = { 108f, 81f, -14f, 105f, 78f, 120f };
        private static readonly float[] Inertia = { .027f, .032f, .024f, .016f, .013f, .011f };
        private static readonly float[] TorqueLimit = { 2.0f, 2.3f, 1.72f, 1.32f, 1.05f, .92f };
        private static readonly float[] NmPerAmp = { .24f, .25f, .23f, .19f, .18f, .17f };
        private static readonly Pose StartPose = Forward(PointA).Tip;
        private static readonly Pose EndPose = Forward(PointB).Tip;

        internal sealed class Joint
        {
            internal float Position, Encoder, Reference, Velocity, Torque, CurrentU, CurrentV, CurrentW;
            internal float Temperature = 24f, ObservedTemperature = 24f, EncoderAge, ThermalAge, Target;
            internal float Range, MinPosition, MaxPosition, PeakCurrent, PeakError, Regen, PeakTemperature = 24f;
            internal bool EncoderPresent, ThermalPresent;
        }

        internal readonly Joint[] Joints = new Joint[6];
        private readonly float[] positions = new float[6], target = new float[6], oldTarget = new float[6], homeStart = new float[6];
        private readonly float[] forbidden = new float[6];
        internal readonly struct Geometry
        {
            internal readonly Vector3 Shoulder, Elbow, Wrist;
            internal readonly Pose Tip;
            internal Geometry(Vector3 shoulder, Vector3 elbow, Vector3 wrist, Pose tip)
            { Shoulder = shoulder; Elbow = elbow; Wrist = wrist; Tip = tip; }
        }
        internal Geometry Actual { get; private set; }
        internal Pose Goal { get; private set; }
        internal float Bus { get; private set; }
        internal float Elapsed { get; private set; }
        internal float PeakError { get; private set; }
        internal float PeakToolErrorMm { get; private set; }
        internal float MaxIkResidualMm { get; private set; }
        internal float MaxIkOrientationDeg { get; private set; }
        internal float MinimumBus { get; private set; }
        internal float BrakeEnergy { get; private set; }
        internal float RegenEnergy { get; private set; }
        internal float PeakCurrent { get; private set; }
        internal float EncoderDisagreement { get; private set; }
        internal float ThermalDerateTime { get; private set; }
        internal float PeakPhaseImbalance { get; private set; }
        internal float PeakTemperature { get; private set; } = 24f;
        internal int EncoderMisses { get; private set; }
        internal int ClippedPhaseSamples { get; private set; }
        internal int CartesianSamples { get; private set; }
        internal bool EnvelopeTrip { get; private set; }
        internal bool EncoderTrip { get; private set; }
        internal bool ProtectionTrip { get; private set; }
        internal bool TorqueOn { get; private set; }
        internal float PeakSpeed => Mathf.Max(Mathf.Max(Mathf.Abs(Joints[0].Velocity), Mathf.Abs(Joints[1].Velocity)),
            Mathf.Max(Mathf.Max(Mathf.Abs(Joints[2].Velocity), Mathf.Abs(Joints[3].Velocity)),
                Mathf.Max(Mathf.Abs(Joints[4].Velocity), Mathf.Abs(Joints[5].Velocity))));
        internal bool Homed { get; private set; }
        internal bool Qualified
        {
            get
            {
                if (Elapsed < PathSeconds || EnvelopeTrip || EncoderTrip || ProtectionTrip || EncoderMisses != 0 ||
                    ClippedPhaseSamples != 0 || ThermalDerateTime > 0f || CartesianSamples < 200 ||
                    PeakError > 3.2f || PeakToolErrorMm > 28f || MaxIkResidualMm > 1.2f || MaxIkOrientationDeg > .25f ||
                    MinimumBus < 10.6f || PeakCurrent > 8.4f || PeakTemperature > 44f || BrakeEnergy < .012f ||
                    PeakSpeed > 4f) return false;
                foreach (var joint in Joints)
                    if (joint.Range < 12f || Mathf.Abs(joint.Position - joint.Target) > 1.1f) return false;
                return true;
            }
        }

        internal SparkLevel120Robot()
        {
            for (int i = 0; i < Joints.Length; i++)
            {
                float rest = Park[i];
                positions[i] = target[i] = oldTarget[i] = rest;
                Joints[i] = new Joint { Position = rest, Encoder = rest, Reference = rest, Target = rest };
            }
            Actual = Forward(Park);
            Goal = Actual.Tip;
        }

        internal void Begin()
        {
            Elapsed = PeakError = PeakToolErrorMm = MaxIkResidualMm = MaxIkOrientationDeg = PeakCurrent = BrakeEnergy = RegenEnergy =
                EncoderDisagreement = ThermalDerateTime = PeakPhaseImbalance = 0f;
            MinimumBus = Bus;
            PeakTemperature = 24f;
            EncoderMisses = ClippedPhaseSamples = CartesianSamples = 0;
            EnvelopeTrip = EncoderTrip = ProtectionTrip = TorqueOn = Homed = false;
            for (int axis = 0; axis < Joints.Length; axis++)
            {
                var joint = Joints[axis];
                homeStart[axis] = target[axis] = oldTarget[axis] = joint.Position;
                joint.EncoderAge = joint.ThermalAge = joint.PeakCurrent = joint.PeakError = joint.Range = joint.Regen = 0f;
                joint.PeakTemperature = joint.Temperature;
                joint.MinPosition = joint.MaxPosition = joint.Position;
            }
        }

        internal void CutTorque()
        {
            TorqueOn = false;
            foreach (var joint in Joints)
                joint.Torque = joint.CurrentU = joint.CurrentV = joint.CurrentW = 0f;
        }

        internal bool RejectsForbiddenTarget()
        {
            var belowSafeFloor = new Pose(new Vector3(.48f, .075f, .02f), Quaternion.identity);
            return Inverse(belowSafeFloor, forbidden, false) && !Inverse(belowSafeFloor, forbidden, true);
        }

        internal void Advance(float feed, bool precharge, bool contactor, bool running, bool home, bool allowed,
            bool phaseBGood, bool[] drives, bool[] encoders, bool[] thermal)
        {
            if (precharge || contactor)
                Bus += Mathf.Max(0f, feed - Bus) * Dt / (contactor ? .102f : .40f);
            else Bus = Mathf.Max(0f, Bus - Dt * (2.5f + .32f * Bus));

            if (running)
            {
                Elapsed += Dt;
                if (!Plan(home)) EnvelopeTrip = true;
            }
            for (int axis = 0; axis < Joints.Length; axis++)
            {
                var joint = Joints[axis];
                joint.Reference = joint.Position;
                joint.EncoderPresent = encoders[axis];
                joint.ThermalPresent = thermal[axis];
                if (joint.EncoderPresent)
                {
                    joint.Encoder = Mathf.Round(joint.Position * 48f) / 48f;
                    joint.EncoderAge = 0f;
                }
                else if (running)
                {
                    joint.EncoderAge += Dt;
                    EncoderMisses++;
                    EncoderDisagreement = Mathf.Max(EncoderDisagreement, Mathf.Abs(joint.Encoder - joint.Reference));
                    if (joint.EncoderAge > .065f) EncoderTrip = true;
                }
                if (joint.ThermalPresent)
                {
                    joint.ThermalAge = 0f;
                    joint.ObservedTemperature = joint.Temperature;
                }
                else joint.ThermalAge += Dt;
            }

            float loaded = 0f, recovered = 0f;
            bool mayDrive = running && allowed && !EncoderTrip && !EnvelopeTrip && !ProtectionTrip && Bus > 10.4f;
            TorqueOn = false;
            for (int axis = 0; axis < Joints.Length; axis++)
            {
                var joint = Joints[axis];
                joint.Target = target[axis];
                float torque = 0f;
                bool derated = joint.ThermalAge > .028f || joint.Temperature > 47f;
                if (mayDrive && drives[axis])
                {
                    float rate = (target[axis] - oldTarget[axis]) / Dt;
                    float acceleration = (target[axis] - joint.Encoder) * 96f + (rate - joint.Velocity) * 21f;
                    float cap = TorqueLimit[axis] * (derated ? .075f : 1f);
                    torque = Mathf.Clamp(Inertia[axis] * acceleration * Mathf.Deg2Rad +
                        .032f * joint.Velocity * Mathf.Deg2Rad, -cap, cap);
                    if (axis == 5 && derated) ThermalDerateTime += Dt;
                    if (axis == 2 && !phaseBGood && Mathf.Abs(torque) > .017f)
                    {
                        torque *= .47f;
                        ClippedPhaseSamples++;
                        PeakPhaseImbalance = Mathf.Max(PeakPhaseImbalance, Mathf.Abs(torque / NmPerAmp[axis]) * .56f);
                    }
                    joint.Velocity += (torque - .032f * joint.Velocity * Mathf.Deg2Rad) /
                        Inertia[axis] * Mathf.Rad2Deg * Dt;
                    TorqueOn |= Mathf.Abs(torque) > .0001f;
                }
                else joint.Velocity *= 1f - 8.2f * Dt;
                joint.Velocity = Mathf.Clamp(joint.Velocity, -100f, 100f);
                joint.Position += joint.Velocity * Dt;
                oldTarget[axis] = target[axis];
                joint.Torque = torque;

                float radians = joint.Position * Mathf.Deg2Rad * (8f + axis);
                float amplitude = torque / NmPerAmp[axis];
                joint.CurrentU = Mathf.Cos(radians) * amplitude;
                joint.CurrentV = Mathf.Cos(radians - 2.094395f) * amplitude;
                joint.CurrentW = Mathf.Cos(radians + 2.094395f) * amplitude;
                if (axis == 2 && !phaseBGood) joint.CurrentV *= .44f;
                float power = torque * joint.Velocity * Mathf.Deg2Rad;
                loaded += Mathf.Max(0f, power) + amplitude * amplitude * .075f;
                if (power < 0f)
                {
                    recovered += -power * .66f;
                    if (running) joint.Regen += -power * .66f * Dt;
                }
                joint.Temperature += (amplitude * amplitude * .21f - (joint.Temperature - 24f) * .25f) * Dt;
                if (running)
                {
                    joint.MinPosition = Mathf.Min(joint.MinPosition, joint.Position);
                    joint.MaxPosition = Mathf.Max(joint.MaxPosition, joint.Position);
                    joint.Range = joint.MaxPosition - joint.MinPosition;
                    joint.PeakError = Mathf.Max(joint.PeakError, Mathf.Abs(joint.Position - target[axis]));
                    joint.PeakCurrent = Mathf.Max(joint.PeakCurrent, Mathf.Abs(amplitude));
                    joint.PeakTemperature = Mathf.Max(joint.PeakTemperature, joint.Temperature);
                    PeakTemperature = Mathf.Max(PeakTemperature, joint.PeakTemperature);
                    PeakError = Mathf.Max(PeakError, joint.PeakError);
                    PeakCurrent = Mathf.Max(PeakCurrent, joint.PeakCurrent);
                    if (joint.Temperature >= 55f || Mathf.Abs(amplitude) > 10.5f) ProtectionTrip = true;
                }
                positions[axis] = joint.Position;
            }

            const float capacitance = .085f;
            if (contactor)
            {
                Bus = Mathf.Sqrt(Mathf.Max(0f, Bus * Bus + (recovered - loaded * .10f) * 2f * Dt / capacitance));
                float chopper = Mathf.Clamp01((Bus - 12.002f) * 27f) * 5.6f;
                Bus = Mathf.Sqrt(Mathf.Max(0f, Bus * Bus - chopper * 2f * Dt / capacitance));
                if (running)
                {
                    RegenEnergy += recovered * Dt;
                    BrakeEnergy += chopper * Dt;
                    MinimumBus = Mathf.Min(MinimumBus, Bus);
                    if (Bus > 14f || Bus < 10.4f) ProtectionTrip = true;
                }
            }
            Actual = Forward(positions);
            if (running)
            {
                if (!Allowed(positions, Actual)) EnvelopeTrip = true;
                if (!home && Elapsed >= 1.10f && Elapsed <= 2.45f)
                    PeakToolErrorMm = Mathf.Max(PeakToolErrorMm, Vector3.Distance(Actual.Tip.position, Goal.position) * 1000f);
                if (EncoderTrip || EnvelopeTrip || ProtectionTrip) CutTorque();
                if (home && Elapsed >= HomeSeconds && !EncoderTrip && !EnvelopeTrip && !ProtectionTrip)
                {
                    Homed = true;
                    foreach (var joint in Joints)
                        Homed &= joint.EncoderPresent && Mathf.Abs(joint.Encoder - joint.Target) < 1.1f &&
                            Mathf.Abs(joint.Encoder - joint.Reference) < .05f && Mathf.Abs(joint.Velocity) < 3f;
                }
            }
        }

        private bool Plan(bool home)
        {
            if (home) Interpolate(homeStart, Home, Ease(Elapsed / 1.05f), target);
            else if (Elapsed <= 1.10f) Interpolate(Home, PointA, Ease(Elapsed / 1.10f), target);
            else if (Elapsed <= 2.45f)
            {
                float fraction = Ease((Elapsed - 1.10f) / 1.35f);
                Goal = new Pose(Vector3.Lerp(StartPose.position, EndPose.position, fraction),
                    Quaternion.Slerp(StartPose.rotation, EndPose.rotation, fraction));
                if (!Inverse(Goal, target, true)) return false;
                CartesianSamples++;
                Pose roundTrip = Forward(target).Tip;
                MaxIkResidualMm = Mathf.Max(MaxIkResidualMm, Vector3.Distance(Goal.position, roundTrip.position) * 1000f);
                MaxIkOrientationDeg = Mathf.Max(MaxIkOrientationDeg, Quaternion.Angle(Goal.rotation, roundTrip.rotation));
                return true;
            }
            else Interpolate(PointB, Home, Ease((Elapsed - 2.45f) / 1.10f), target);
            var next = Forward(target);
            Goal = next.Tip;
            return Allowed(target, next);
        }

        private static void Interpolate(float[] from, float[] to, float t, float[] output)
        {
            for (int axis = 0; axis < output.Length; axis++) output[axis] = Mathf.Lerp(from[axis], to[axis], t);
        }

        private static float Ease(float time)
        {
            float t = Mathf.Clamp01(time);
            return t * t * t * (10f + t * (-15f + 6f * t));
        }

        internal static Geometry Forward(float[] q)
        {
            float azimuth = q[0] * Mathf.Deg2Rad;
            float shoulderAngle = q[1] * Mathf.Deg2Rad, lower = (q[1] + q[2]) * Mathf.Deg2Rad;
            Vector3 planar = new(Mathf.Cos(azimuth), 0f, Mathf.Sin(azimuth));
            var shoulder = Vector3.up * BaseHeight;
            var elbow = shoulder + Upper * (planar * Mathf.Cos(shoulderAngle) + Vector3.up * Mathf.Sin(shoulderAngle));
            var wrist = elbow + Fore * (planar * Mathf.Cos(lower) + Vector3.up * Mathf.Sin(lower));
            var attitude = Carriage(q[0], q[1], q[2]) * Quaternion.Euler(q[3], q[4], q[5]);
            return new Geometry(shoulder, elbow, wrist, new Pose(wrist + attitude * Vector3.right * Tool, attitude));
        }

        private static Quaternion Carriage(float yaw, float upper, float lower) =>
            Quaternion.AngleAxis(-yaw, Vector3.up) * Quaternion.AngleAxis(upper + lower, Vector3.forward);

        private static bool Inverse(Pose aim, float[] output, bool enforceEnvelope)
        {
            Vector3 wrist = aim.position - aim.rotation * Vector3.right * Tool;
            float radial = new Vector2(wrist.x, wrist.z).magnitude;
            float height = wrist.y - BaseHeight;
            float cosine = (radial * radial + height * height - Upper * Upper - Fore * Fore) / (2f * Upper * Fore);
            if (radial < .06f || Mathf.Abs(cosine) >= .985f) return false;
            float elbow = -Mathf.Acos(cosine);
            float shoulder = Mathf.Atan2(height, radial) - Mathf.Atan2(Fore * Mathf.Sin(elbow), Upper + Fore * cosine);
            output[0] = Mathf.Atan2(wrist.z, wrist.x) * Mathf.Rad2Deg;
            output[1] = shoulder * Mathf.Rad2Deg;
            output[2] = elbow * Mathf.Rad2Deg;
            var euler = (Quaternion.Inverse(Carriage(output[0], output[1], output[2])) * aim.rotation).eulerAngles;
            output[3] = Mathf.DeltaAngle(0f, euler.x);
            output[4] = Mathf.DeltaAngle(0f, euler.y);
            output[5] = Mathf.DeltaAngle(0f, euler.z);
            return !enforceEnvelope || Allowed(output, Forward(output));
        }

        private static bool Allowed(float[] q, Geometry pose)
        {
            for (int axis = 0; axis < q.Length; axis++)
                if (q[axis] < Min[axis] || q[axis] > Max[axis]) return false;
            Vector3 tip = pose.Tip.position;
            float radial = new Vector2(tip.x, tip.z).magnitude;
            return pose.Elbow.y > .13f && pose.Wrist.y > .12f && tip.y is > .14f and < .85f &&
                radial is > .24f and < .82f && tip.x > .20f;
        }
    }
}
