using UnityEngine;

namespace ProjectSpark.Gameplay
{
    internal sealed class SparkLevel92Plant
    {
        public const float MissionDuration = 22f;
        private const int SurveyCells = 12;
        private bool surveying, deploy, extraction;
        private float scanTime, height = .55f, speed, proof, rpm, feed, depth, heat, torque;
        private float core, debris, seal, leak, hold;
        private readonly float[] echoes = new float[SurveyCells];
        private int scanned, site = -1;
        private int phase;
        public readonly float[] Extension = new float[6];
        public readonly float[] Grip = new float[6];
        public float Coverage => scanned / (float)SurveyCells;
        public float Uncertainty { get; private set; } = 1f;
        public int Site => site + 1;
        public bool Surveyed => scanned == SurveyCells && site >= 0 && Uncertainty < .1f;
        public float AnchorLoad { get; private set; }
        public float Margin { get; private set; }
        public float Tether { get; private set; }
        public float Rpm => rpm;
        public float Depth => depth;
        public float Heat => heat;
        public float Torque => torque;
        public float Seal => seal;
        public float Leak => leak;
        public float SampleMass => core;
        public float Integrity => core / Mathf.Max(.01f, core + debris);
        public bool Stable => proof >= 1.25f && Margin > .55f;
        public bool MotorOff => rpm < 1f;
        public bool Sealed => phase == 4 && core >= .48f && Integrity >= .73f && Leak < .025f && hold >= 1.5f;
        public string Phase => phase switch { 0 => "SPIN UP", 1 => "CORING", 2 => "RETRACT", 3 => "SEAL TEST", _ => "CORE SECURED" };

        public void BeginSurvey()
        {
            surveying = true;
            scanned = 0;
            scanTime = 0f;
            site = -1;
            Uncertainty = 1f;
        }

        public void Deploy() => deploy = true;

        public void BeginExtraction()
        {
            extraction = true;
            phase = 0;
            core = debris = seal = leak = hold = 0f;
        }

        public void StopRun() => extraction = false;

        private static float Layer(int cell) => .28f + ((cell * 17 + 3) % 13) * .041f;

        public void Tick(float step, bool powered, bool permit, int fault)
        {
            if (powered && surveying) Survey(step);
            if (powered && deploy)
            {
                Tether = Mathf.MoveTowards(Tether, .8f, step * .22f);
                speed = Mathf.MoveTowards(speed, .18f, step * .23f);
                height = Mathf.Max(0f, height - speed * step);
            }

            float capacity = 0f;
            for (int i = 0; i < 6; i++)
            {
                Extension[i] = Mathf.MoveTowards(Extension[i], powered && deploy ? 1f : 0f, step * .42f);
                float contact = .92f - .018f * i;
                bool attached = height < .03f && Extension[i] >= contact && (fault != 1 || i != 0);
                Grip[i] = Mathf.MoveTowards(Grip[i], attached ? 1f : 0f, step * .75f);
                capacity += Grip[i] * .24f;
            }
            AnchorLoad = deploy ? .22f + torque * .24f + (fault == 1 ? .57f : 0f) : 0f;
            Margin = capacity - AnchorLoad;
            bool contactProof = powered && deploy && capacity >= 1.42f && Margin > .55f;
            proof = contactProof ? Mathf.Min(2f, proof + step) : 0f;

            bool active = extraction && permit && Stable;
            bool stall = permit && fault == 2;
            bool spin = active && phase <= 1;
            rpm = Mathf.MoveTowards(rpm, stall ? 90f : spin ? 680f : 0f, step * 580f);
            float resistance = depth < Layer(Mathf.Max(0, site)) ? .46f : depth < .86f ? .63f : .78f;
            torque = stall ? .96f : spin ? resistance * rpm / 680f : 0f;
            heat = Mathf.MoveTowards(heat, stall ? .92f : torque * .7f, step * .32f);
            feed = Mathf.MoveTowards(feed, active && phase == 1 && heat < .72f ? .19f * (1f - torque * .42f) : 0f, step * .35f);
            if (stall || !active) feed = 0f;
            seal = Mathf.MoveTowards(seal, fault == 3 ? .35f : phase >= 3 ? 1f : 0f, step);
            leak = Mathf.MoveTowards(leak, fault == 3 ? .4f : (1f - seal) * .13f, step * .2f);
            if (!active)
            {
                if (MotorOff) depth = Mathf.MoveTowards(depth, 0f, step * .32f);
                return;
            }

            switch (phase)
            {
                case 0 when rpm >= 670f:
                    Advance();
                    break;
                case 1:
                    float cut = Mathf.Min(1f - depth, feed * step);
                    depth += cut;
                    float retention = resistance < .7f ? .84f : .78f;
                    core += cut * retention;
                    debris += cut * (1f - retention);
                    if (depth >= .999f) Advance();
                    break;
                case 2:
                    depth = Mathf.MoveTowards(depth, 0f, step * .46f);
                    if (depth <= .001f) Advance();
                    break;
                case 3:
                    hold = seal > .98f && leak < .025f ? hold + step : 0f;
                    if (hold >= 1.5f) Advance();
                    break;
            }
        }

        private void Advance()
        {
            phase++;
        }

        private void Survey(float step)
        {
            scanTime += step;
            if (scanTime < .28f) return;
            scanTime -= .28f;
            float equivalentDepth = Layer(scanned);
            float repeatedEcho = equivalentDepth + Mathf.Sin(scanned * 2.41f) * .035f;
            echoes[scanned++] = repeatedEcho;
            Uncertainty = .21f / Mathf.Sqrt(scanned);
            if (scanned < SurveyCells) return;
            surveying = false;
            for (int i = 1; i < SurveyCells - 1; i++)
                if (site < 0 || SiteScore(i) > SiteScore(site)) site = i;
        }

        private float SiteScore(int index) => echoes[index] -
            .5f * (Mathf.Abs(echoes[index - 1] - echoes[index]) + Mathf.Abs(echoes[index + 1] - echoes[index]));
    }
}


