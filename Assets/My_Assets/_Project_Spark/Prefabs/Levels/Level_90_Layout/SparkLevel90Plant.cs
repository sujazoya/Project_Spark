using UnityEngine;

namespace ProjectSpark.Gameplay
{
    /// <summary>Dimensionless training simulation for the four propulsion channels.</summary>
    internal sealed class SparkLevel90Plant
    {
        public const float DepartureDuration = 24f;
        public const float TargetDeltaV = 1.6f;
        private bool ignitionRequested;
        private float releaseTravel;
        private float departureTime;
        public readonly float[] Tanks = { 48f, 48f };
        public readonly float[] Flow = new float[4];
        public readonly float[] Precharge = new float[4];
        public readonly float[] Discharge = new float[4];
        public readonly float[] Heat = new float[4];
        public readonly float[] Yaw = new float[4];
        public bool Calibrated { get; private set; }
        public float Energy { get; private set; } = 100f;
        public float Coolant { get; private set; }
        public float Solar { get; private set; }
        public float Thrust { get; private set; }
        public float DeltaV { get; private set; }
        public float Separation { get; private set; }
        public float NeutralizerCurrent { get; private set; }
        public float IndependentCurrent { get; private set; }
        public bool NeutralizerAvailable => NeutralizerCurrent > .15f;
        public string Phase => releaseTravel < 1f ? "DOCK RELEASE" : departureTime < 18f ? "TRANSFER BURN" : "GUIDANCE HOLD";
        public bool Commissioned => Energy > 15f && Coolant >= .9f &&
            Precharge[0] >= .98f && Precharge[1] >= .98f && Precharge[2] >= .98f && Precharge[3] >= .98f &&
            Flow[0] >= .75f && Flow[1] >= .75f && Flow[2] >= .75f && Flow[3] >= .75f;
        public bool Stable => Commissioned && Thrust > 2f && Mathf.Abs(Yaw[2]) < 3f &&
            Discharge[0] > .7f && Discharge[1] > .7f && Discharge[2] > .7f && Discharge[3] > .7f;
        public bool DischargeIsOff => Discharge[0] + Discharge[1] + Discharge[2] + Discharge[3] < .05f;

        public void Calibrate() => Calibrated = true;
        public void RequestIgnition() => ignitionRequested = true;
        public void StopIgnition() => ignitionRequested = false;

        public void BeginDeparture()
        {
            releaseTravel = departureTime = DeltaV = Separation = 0f;
        }

        public void Tick(float step, bool controls, bool permit, bool departure, int fault)
        {
            Coolant = Mathf.MoveTowards(Coolant, controls ? 1f : 0f, step * .8f);
            Solar = Mathf.MoveTowards(Solar, controls ? 1f : 0f, step * .3f);
            NeutralizerCurrent = Mathf.MoveTowards(NeutralizerCurrent, controls ? .65f : 0f, step);
            IndependentCurrent = Mathf.MoveTowards(IndependentCurrent, controls ? .62f : 0f, step * .7f);
            Thrust = 0f;
            float power = 0f;
            for (int channel = 0; channel < 4; channel++)
            {
                int tank = channel / 2;
                bool ready = Calibrated && controls && Energy > 15f && Tanks[tank] > 6f;
                float demand = ready ? Mathf.Clamp01(Tanks[tank] / 18f) : 0f;
                if (fault == 1 && channel == 0) demand *= .18f;
                Flow[channel] = Mathf.MoveTowards(Flow[channel], demand, step * .9f);
                Precharge[channel] = Mathf.MoveTowards(Precharge[channel], ready ? 1f : 0f, step * .7f);

                bool coilFault = fault == 2 && channel == 1;
                bool ignite = ignitionRequested && permit && ready && Precharge[channel] > .98f &&
                    Flow[channel] > .6f && Heat[channel] < .8f;
                Discharge[channel] = Mathf.MoveTowards(Discharge[channel], ignite ? .88f : 0f, step * (ignite ? .65f : 2.8f));
                float targetHeat = coilFault ? 1f : Discharge[channel] * (.72f - .25f * Coolant);
                Heat[channel] = Mathf.MoveTowards(Heat[channel], targetHeat, step * (coilFault ? .42f : .36f));
                Yaw[channel] = Mathf.MoveTowards(Yaw[channel], fault == 3 && channel == 2 ? 12f : 0f, step * 8f);

                float beam = Discharge[channel] * Flow[channel];
                Tanks[tank] = Mathf.Max(0f, Tanks[tank] - beam * step * (departure ? .105f : .008f));
                power += Discharge[channel] * .72f + Precharge[channel] * .09f;
                Thrust += beam * Mathf.Cos(Yaw[channel] * Mathf.Deg2Rad);
            }

            Energy = Mathf.Clamp(Energy + step * ((departure ? 1.8f : 4.2f) * Solar - power), 0f, 100f);
            if (!departure || !permit || !Stable) return;
            departureTime += step;
            releaseTravel = Mathf.MoveTowards(releaseTravel, 1f, step * .5f);
            if (releaseTravel < 1f) return;
            DeltaV += Thrust * .032f * step;
            Separation += DeltaV * step;
        }
    }
}

