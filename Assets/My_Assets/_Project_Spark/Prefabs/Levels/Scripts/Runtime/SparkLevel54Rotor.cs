using UnityEngine;

namespace ProjectSpark.Gameplay
{
    // Single lateral mode per axis. Metres, seconds, kilograms and newtons.
    internal sealed class SparkLevel54Rotor
    {
        public const float Mass = .65f;
        public const float ShaftStiffness = 45f;
        public const float SupportStiffness = 60f;
        public static readonly float Stiffness = 1f / (1f / ShaftStiffness + 1f / SupportStiffness);
        public static readonly float NaturalRpm = Mathf.Sqrt(Stiffness / Mass) * 30f / Mathf.PI;
        public const float PassiveDamping = .220f;
        public const float ForceLimit = .032f;
        private const float EccentricMoment = .000041f;

        public Vector2 Position { get; private set; }
        public Vector2 Velocity { get; private set; }
        public Vector2 DamperForce { get; private set; }
        public float ExtraDamping { get; private set; }
        public bool Limited { get; private set; }

        public void Step(float dt, float angle, float omega, float requestedDamping, bool actuatorPowered)
        {
            ExtraDamping = actuatorPowered ? Mathf.MoveTowards(ExtraDamping, requestedDamping, 2.7f * dt) : 0;
            // Implicit viscous update prevents the damper from reversing the
            // velocity within a substep, even at the highest gain.
            Vector2 drive = EccentricMoment * omega * omega *
                new Vector2(Mathf.Cos(angle), .88f * Mathf.Sin(angle));
            Vector2 free = (Velocity + dt / Mass * (drive - Stiffness * Position))
                / (1 + dt * PassiveDamping / Mass);
            Vector2 damped = free / (1 + dt * ExtraDamping / Mass);
            Vector2 requestedForce = -ExtraDamping * damped;
            Limited = requestedForce.sqrMagnitude > ForceLimit * ForceLimit;
            DamperForce = Vector2.ClampMagnitude(requestedForce, ForceLimit);
            Velocity = Limited ? free + DamperForce * (dt / Mass) : damped;
            Position += dt * Velocity;
        }
    }

    internal sealed class SparkLevel54Encoder
    {
        private static readonly int[] Gray = { 0, 2, 3, 1 };
        private static readonly int[] Delta = {0,-1,1,0,1,0,0,-1,-1,0,0,1,0,1,-1,0};
        private int edge, state, count, indexCount, tachCount;
        private float tachClock, indexAge;
        public bool Locked { get; private set; }
        public bool Index { get; private set; }
        public float Rpm { get; private set; }
        public float Phase => (count - indexCount) * (Mathf.PI / 40);
        public bool A => (state & 2) != 0;
        public bool B => (state & 1) != 0;
        public bool TimedOut => indexAge > 2.1f;
        public void ClearLock() { Locked = false; indexAge = 0; }

        public bool Step(float dt, float angle, bool a, bool b, bool z)
        {
            indexAge += dt;
            Index = false;
            bool valid = true;
            int end = Mathf.FloorToInt(angle * (40f / Mathf.PI));
            while (edge < end)
            {
                var sent = Gray[++edge & 3];
                var received = (a ? sent & 2 : 0) | (b ? sent & 1 : 0);
                int transition = Delta[4 * state + received];
                state = received;
                count += transition;
                valid &= transition == 1;
                if (edge % 80 != 0 || !z) continue;
                valid &= !Locked || count - indexCount == 80;
                Locked = valid;
                indexCount = count;
                indexAge = 0;
                Index = true;
            }
            if (!a || !b || !z || !valid) Locked = false;
            tachClock += dt;
            if (tachClock >= .60f)
            {
                Rpm = (count - tachCount) * 60 / (80 * tachClock);
                tachCount = count;
                tachClock = 0;
            }
            return valid;
        }
    }

    internal sealed class SparkLevel54Probe
    {
        public readonly float[] History = new float[160];
        public float Offset { get; private set; }
        public float Instant { get; private set; }
        public float Filtered { get; private set; }
        public float Timestamp { get; private set; }
        public int Cursor { get; private set; }
        public bool Powered { get; private set; }
        public bool Valid { get; private set; }
        public bool Calibrated { get; private set; }
        private readonly float mountingOffset;
        public SparkLevel54Probe(float mountingOffset) { this.mountingOffset = mountingOffset; }
        public void Zero(float position)
        {
            Offset = position + mountingOffset;
            Calibrated = true;
            Filtered = 0;
        }
        public void InvalidateCalibration() { Calibrated = false; }
        public void Read(float dt, float elapsed, float position, bool powered, bool routed)
        {
            Powered = powered;
            Valid = powered && routed && Mathf.Abs(position + mountingOffset) < 12f;
            if (!Valid) return;
            Instant = position + mountingOffset - Offset;
            Filtered += (Instant - Filtered) * (dt / (.020f + dt));
            Timestamp = elapsed;
            History[Cursor] = Filtered;
            Cursor = (Cursor + 1) % History.Length;
        }
    }

    internal sealed class SparkLevel54Measurement
    {
        internal struct Reading
        {
            public float Rpm, Xrms, Yrms, RadialRms, Phase, Peak, Time;
        }
        private float time, rpmSum, xx, yy, cosine, sine, peak;
        public void Clear() { time = rpmSum = xx = yy = cosine = sine = peak = 0; }
        public void Sample(float dt, float rpm, Vector2 displacement, float phase)
        {
            time += dt;
            rpmSum += rpm * dt;
            xx += displacement.x * displacement.x * dt;
            yy += displacement.y * displacement.y * dt;
            cosine += displacement.x * Mathf.Cos(phase) * dt;
            sine += displacement.x * Mathf.Sin(phase) * dt;
            peak = Mathf.Max(peak, displacement.magnitude);
        }
        public Reading Result(float elapsed) => new()
        {
            Rpm = rpmSum / time,
            Xrms = Mathf.Sqrt(xx / time),
            Yrms = Mathf.Sqrt(yy / time),
            RadialRms = Mathf.Sqrt((xx + yy) / time),
            Phase = Mathf.Repeat(Mathf.Atan2(sine, cosine) * Mathf.Rad2Deg, 360),
            Peak = peak,
            Time = elapsed
        };
    }
}
