using System;
using UnityEngine;

namespace ProjectSpark.Gameplay
{
    // Bounded fictional navigation exercise. Position, timing, and fuel are in game units.
    internal sealed class SparkLevel91Plant
    {
        public const float ObservationDuration = 10f;
        public const float MissionDuration = 22f;
        private const float Tau = 2f * Mathf.PI;
        private readonly System.Random random = new(91363);
        private readonly float[] cosine = new float[4];
        private readonly float[] sine = new float[4];
        private readonly float[] rate = { 1f, 1.2f, 1.4f, 1.6f };
        private static readonly Vector3[] Directions = {
            new Vector3(1, 1, 1).normalized, new Vector3(1, -1, -1).normalized,
            new Vector3(-1, 1, -1).normalized, new Vector3(-1, -1, 1).normalized
        };
        public readonly float[] Azimuth = new float[4];
        public readonly float[] Elevation = new float[4];
        public readonly float[] Quality = new float[4];
        public readonly int[] Photons = new int[4];
        private float time, biasCalibration;
        private bool acquiring, correcting;
        private Vector3 position = new(.045f, -.037f, .028f);
        private Vector3 velocity = new(.00045f, -.00025f, .00015f);
        private Vector3 target;
        private int missionIndex;

        public float Cooling { get; private set; }
        public float ClockTemp { get; private set; } = .28f;
        public float ClockBias { get; private set; } = .025f;
        public float ClockResidual => ClockBias - biasCalibration;
        public float BackupBias => .018f;
        public bool ClockAvailable => Mathf.Abs(ClockResidual) < .19f;
        public bool Calibrated { get; private set; }
        public bool Homed { get; private set; }
        public bool ObservationReady { get; private set; }
        public float Background { get; private set; }
        public float EphemerisError { get; private set; }
        public float Covariance { get; private set; } = 1f;
        public float CourseError => Vector3.Distance(position, target);
        public Vector3 Estimate { get; private set; }
        public float EstimatedClock { get; private set; }
        public float DeltaV { get; private set; }
        public float Fuel { get; private set; } = 20f;
        public bool BurnActive { get; private set; }
        public bool Corrected => ObservationReady && DeltaV > .002f && CourseError < .04f;
        public bool Healthy => Homed && ClockAvailable && ClockTemp < .42f && Background < .4f && EphemerisError < .01f;
        public string Phase => acquiring ? "FOLDING PHOTONS" : BurnActive ? "CORRECTION BURN" : "COAST / VERIFY";

        public void Calibrate()
        {
            biasCalibration = ClockBias;
            Calibrated = true;
        }

        public void BeginObservation(bool mission)
        {
            time = DeltaV = 0f;
            Covariance = 1f;
            ObservationReady = false;
            acquiring = true;
            correcting = mission;
            if (mission)
            {
                target = (missionIndex++ % 3) switch
                {
                    0 => Vector3.zero,
                    1 => new Vector3(.058f, -.025f, .02f),
                    _ => new Vector3(-.052f, .024f, -.018f)
                };
            }
            Array.Clear(cosine, 0, 4);
            Array.Clear(sine, 0, 4);
            Array.Clear(Photons, 0, 4);
            Array.Clear(Quality, 0, 4);
        }

        public void StopRun()
        {
            acquiring = correcting = BurnActive = false;
        }

        public void Tick(float step, bool powered, bool permitted, int fault)
        {
            Cooling = Mathf.MoveTowards(Cooling, powered ? 1f : 0f, step * .9f);
            ClockTemp = Mathf.MoveTowards(ClockTemp, fault == 2 ? .9f : .28f, step * .3f);
            ClockBias = Mathf.MoveTowards(ClockBias, .025f + (ClockTemp - .28f) * .22f, step * .075f);
            Background = Mathf.MoveTowards(Background, fault == 1 ? 2f : .14f, step * 1.25f);
            EphemerisError = Mathf.MoveTowards(EphemerisError, fault == 3 ? .09f : 0f, step * .065f);
            Homed = powered && Calibrated;
            for (int i = 0; i < 4; i++)
            {
                float az = powered && Calibrated ? 16f + i * 12f : 0f;
                float el = powered && Calibrated ? 11f + i * 5f : 0f;
                Azimuth[i] = Mathf.MoveTowards(Azimuth[i], az, step * 28f);
                Elevation[i] = Mathf.MoveTowards(Elevation[i], el, step * 17f);
                Homed &= Mathf.Abs(Azimuth[i] - az) < .02f && Mathf.Abs(Elevation[i] - el) < .02f;
            }

            BurnActive = false;
            if (acquiring && permitted && Healthy)
            {
                AcquireEvents(step);
                if (time >= ObservationDuration - .001f) FitSolution();
            }

            if (correcting && permitted && Healthy && ObservationReady && Fuel > .2f)
            {
                Vector3 desired = -.48f * (Estimate - target);
                Vector3 acceleration = Vector3.ClampMagnitude((desired - velocity) * 1.55f, .028f);
                float impulse = acceleration.magnitude * step;
                BurnActive = impulse > .000015f;
                velocity += acceleration * step;
                DeltaV += impulse;
                Fuel -= impulse * 5f;
            }
            position += velocity * step;
            if (ObservationReady) Estimate += velocity * step;
        }

        private void AcquireEvents(float step)
        {
            // Thinning a Poisson stream produces individual event times with the pulse profile.
            float upper = 155f + Background * 60f;
            for (int channel = 0; channel < 4; channel++)
            {
                float offset = Vector3.Dot(Directions[channel], position) + (channel == 3 ? EphemerisError : 0f);
                double eventTime = -Math.Log(1d - random.NextDouble()) / upper;
                while (eventTime < step)
                {
                    float timestamp = time + (float)eventTime;
                    float phase = Tau * (rate[channel] * timestamp - offset);
                    float intensity = 85f + 68f * Mathf.Cos(phase) + Background * 60f;
                    if (random.NextDouble() * upper < intensity)
                    {
                        float observed = Tau * (rate[channel] * timestamp + ClockResidual);
                        cosine[channel] += Mathf.Cos(observed);
                        sine[channel] += Mathf.Sin(observed);
                        Photons[channel]++;
                    }
                    eventTime += -Math.Log(1d - random.NextDouble()) / upper;
                }
                Quality[channel] = Photons[channel] > 0
                    ? Mathf.Clamp01(new Vector2(cosine[channel], sine[channel]).magnitude / (Photons[channel] * .5f))
                    : 0f;
            }
            time += step;
        }

        private void FitSolution()
        {
            acquiring = false;
            // The four tetrahedral sight lines have sum(n)=0 and sum(n*nᵀ)=4I/3.
            // This separates the common clock bias from the three spatial coordinates.
            Vector3 estimate = Vector3.zero;
            float bias = 0f;
            float variance = 0f;
            bool valid = true;
            for (int i = 0; i < 4; i++)
            {
                float phase = Mathf.Atan2(sine[i], cosine[i]) / Tau;
                estimate += Directions[i] * (.75f * phase);
                bias += .25f * phase;
                float coherence = Mathf.Max(.05f, Quality[i]);
                variance += 1f / (Mathf.Max(1, Photons[i]) * coherence * coherence * Tau * Tau);
                valid &= Photons[i] >= 480 && Quality[i] > .38f;
            }
            Covariance = variance * .5625f;
            Estimate = estimate;
            EstimatedClock = bias;

            ObservationReady = valid && Covariance < .0012f && Vector3.Distance(Estimate, position) < .04f &&
                Mathf.Abs(EstimatedClock - ClockResidual) < .065f;
        }
    }
}

