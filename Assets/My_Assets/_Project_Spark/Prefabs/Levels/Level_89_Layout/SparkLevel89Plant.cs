using UnityEngine;

namespace ProjectSpark.Gameplay
{
    // Fictional game-scale storm, shielding and essential-utility model.
    public sealed class SparkLevel89Plant
    {
        public const float StormDuration = 24f;
        public const float DoseBudget = 4.2f;
        private const float ReserveFloor = 24f;
        private readonly float[] velocities = new float[4];
        private bool sealRequested;
        private float doorTravel;

        public float[] Positions { get; } = new float[4];
        public float[] Water { get; } = new float[4];
        public float Reserve { get; private set; } = 88f;
        public float Energy { get; private set; } = 90f;
        public float Airflow { get; private set; }
        public float PumpRate { get; private set; }
        public float LockEngagement { get; private set; }
        public float Coverage { get; private set; }
        public float StormIntensity { get; private set; }
        public float HighEnergy { get; private set; }
        public float DoseRate { get; private set; }
        public string Phase { get; private set; } = "Standby";
        public bool DetectorsInRange { get; private set; } = true;
        public bool DoorClosed => doorTravel >= .99f;
        public bool Locked => LockEngagement >= .99f;
        public bool Ready => DoorClosed && Locked && Coverage >= .96f && Airflow >= .92f &&
            Reserve >= ReserveFloor && Energy > 12f;

        public void RequestSeal() => sealRequested = true;

        public void BeginJam()
        {
            Positions[2] = .56f;
            velocities[2] = 0f;
            LockEngagement = 0f;
        }

        public void Tick(float step, bool essential, bool motion, float elapsed, int fault, bool hardened)
        {
            Airflow = Mathf.MoveTowards(Airflow, essential ? 1f : 0f, step * .55f);
            float allocation = 0f;
            float coverage = 0f;
            float softTransmission = 0f;
            float hardTransmission = 0f;

            for (int i = 0; i < Positions.Length; i++)
            {
                bool deploy = sealRequested && motion && Energy > 12f && (fault != 2 || i != 2);
                velocities[i] = Mathf.MoveTowards(velocities[i], deploy ? .48f : 0f, step * 1.25f);
                Positions[i] = Mathf.Clamp01(Positions[i] + velocities[i] * step);

                float fill = deploy ? Mathf.Min(1f - Water[i], .48f * step, (Reserve - ReserveFloor) / 11f) : 0f;
                Water[i] += fill;
                Reserve -= fill * 11f;
                allocation += fill;
                float panelCoverage = Positions[i];
                coverage += panelCoverage;
                softTransmission += 1f - panelCoverage + panelCoverage * Mathf.Exp(-4f * Water[i]);
                hardTransmission += 1f - panelCoverage + panelCoverage * Mathf.Exp(-2.65f * Water[i]);
            }

            PumpRate = Mathf.Clamp01(allocation / (step * 1.92f));
            Coverage = coverage / 4f;
            if (sealRequested && motion) doorTravel = Mathf.MoveTowards(doorTravel, 1f, step * .62f);
            if (fault == 2) LockEngagement = 0f;
            else if (motion && DoorClosed && Coverage >= .995f)
                LockEngagement = Mathf.MoveTowards(LockEngagement, 1f, step * 1.1f);

            float demand = (essential ? .75f : 0f) + PumpRate * 1.65f + (motion && !Locked ? .65f : 0f);
            Energy = Mathf.Clamp(Energy + step * ((essential && elapsed < 0f ? 2.25f : 0f) - demand), 0f, 90f);

            Phase = elapsed < 0f ? "Standby" : elapsed < 3f ? "Quiet" : elapsed < 9f ? "Rise" :
                elapsed < 15f ? "Peak" : "Decay";
            float envelope = elapsed < 3f ? .04f : elapsed < 9f ? Mathf.Lerp(.04f, 1f, (elapsed - 3f) / 6f) :
                elapsed < 15f ? 1f : Mathf.Lerp(1f, .04f, (elapsed - 15f) / 9f);
            StormIntensity = fault == 3 ? 1f : envelope;
            HighEnergy = fault == 3 ? 3.65f : envelope * (hardened ? .76f : .93f);
            DetectorsInRange = fault != 1;
            DoseRate = (StormIntensity * 1.75f * softTransmission + HighEnergy * hardTransmission) / 4f;
            if (!DoorClosed) DoseRate += StormIntensity * .4f;
        }
    }
}

