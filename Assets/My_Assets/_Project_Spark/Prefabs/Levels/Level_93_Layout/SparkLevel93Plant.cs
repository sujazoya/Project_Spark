using UnityEngine;

namespace ProjectSpark.Gameplay
{
    // A bounded, fictional mineral analysis model in normalized game units.
    internal sealed class SparkLevel93Plant
    {
        private const int ProjectionBins = 24;
        private readonly bool[] projections = new bool[ProjectionBins];
        private readonly float[] reference = { .52f, .31f, .17f };
        private readonly float[] spectra = new float[3];
        private readonly float[] massSpectrum = new float[3];
        private bool commissioning, mapping, analyzing;
        private float mapTime, transfer, heat, fragment, massExposure;
        private int covered;
        public float Contamination { get; private set; } = .48f;
        public float Pressure { get; private set; } = .82f;
        public float BackupPressure { get; private set; } = .82f;
        public float Gain { get; private set; } = 1f;
        public float Angle { get; private set; }
        public float EncoderError { get; private set; }
        public float Reserve { get; private set; } = 1f;
        public float Consumed { get; private set; }
        public float Coverage => covered / (float)ProjectionBins;
        public float Uncertainty { get; private set; } = 1f;
        public float SpectralError { get; private set; } = 1f;
        public float Heat => heat;
        public float Transfer => transfer;
        public bool Qualified { get; private set; }
        public bool Calibrated => transfer > .99f && Contamination < .11f && Pressure < .16f &&
            Mathf.Abs(Gain - 1f) < .03f && Mathf.Abs(EncoderError) < 2f;
        public bool Mapped => covered == ProjectionBins && Uncertainty < .045f && SpectralError < .04f;
        public bool HeaterOff => heat < .025f;

        public void Commission() => commissioning = true;

        public void BeginMap()
        {
            System.Array.Clear(projections, 0, projections.Length);
            System.Array.Clear(spectra, 0, spectra.Length);
            covered = 0;
            mapTime = 0f;
            Uncertainty = SpectralError = 1f;
            mapping = true;
        }

        public bool BeginAnalysis()
        {
            if (Reserve < .48f) return false;
            fragment = .095f;
            Reserve -= fragment;
            Consumed += fragment;
            massExposure = 0f;
            System.Array.Clear(massSpectrum, 0, massSpectrum.Length);
            Qualified = false;
            analyzing = true;
            BeginMap();
            return true;
        }

        public void StopRun() => mapping = analyzing = false;

        public void Tick(float step, bool powered, bool permit, int fault)
        {
            bool purge = powered && commissioning;
            Contamination = Mathf.MoveTowards(Contamination, fault == 1 ? .75f : purge ? .045f : .48f, step * .35f);
            Pressure = Mathf.MoveTowards(Pressure, powered ? .09f : .82f, step * .43f);
            BackupPressure = Mathf.MoveTowards(BackupPressure, Pressure + .012f, step * .56f);
            Gain = Mathf.MoveTowards(Gain, fault == 3 ? 1.48f : 1f, step * .42f);
            EncoderError = Mathf.MoveTowards(EncoderError, fault == 2 ? 24f : 0f, step * 26f);
            transfer = Mathf.MoveTowards(transfer, purge ? 1f : 0f, step * .55f);

            if (mapping && permit && Calibrated)
            {
                Angle = Mathf.Repeat(Angle + step * 72f, 360f);
                int bin = Mathf.Clamp(Mathf.FloorToInt(Angle / 360f * ProjectionBins), 0, ProjectionBins - 1);
                if (!projections[bin])
                {
                    projections[bin] = true;
                    covered++;
                }

                mapTime += step;
                // Three instrument responses share the same specimen, with different noise phases.
                SpectralError = 0f;
                for (int i = 0; i < spectra.Length; i++)
                {
                    float measurement = reference[i] + .044f * Mathf.Sin(Angle * Mathf.Deg2Rad * (i + 2)) +
                        Contamination * .06f;
                    spectra[i] += measurement * step;
                    SpectralError += Mathf.Abs(spectra[i] / mapTime - reference[i]);
                }
                Uncertainty = .16f / Mathf.Sqrt(Mathf.Max(1, covered));
                if (Mapped) mapping = false;
            }

            bool heatPermit = analyzing && Mapped && permit && Calibrated;
            heat = Mathf.MoveTowards(heat, heatPermit ? .82f : 0f, step * (heatPermit ? .45f : 1.9f));
            if (!heatPermit || heat < .7f) return;
            float exposure = step * fragment;
            massExposure += step;
            for (int i = 0; i < massSpectrum.Length; i++)
                massSpectrum[i] += exposure * reference[i] * Gain;
            if (massExposure < 2.5f) return;

            float massError = 0f;
            for (int i = 0; i < massSpectrum.Length; i++)
                massError += Mathf.Abs(massSpectrum[i] / (massExposure * fragment) - reference[i]);
            Qualified = massError < .05f && Reserve >= .38f;
            analyzing = false;
        }
    }
}

