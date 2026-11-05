using UnityEngine;
namespace ProjectSpark.Gameplay
{
    [DisallowMultipleComponent]
    public sealed class SparkLDR : SparkElectricalComponent
    {
        [SerializeField] private SparkTerminal terminalA;
        [SerializeField] private SparkTerminal terminalB;
        [SerializeField, Range(0f, 1f)] private float illumination = 0.5f;
        [SerializeField, Min(1f)] private float darkResistanceOhms = 100000f;
        [SerializeField, Min(1f)] private float brightResistanceOhms = 1000f;
        public SparkTerminal TerminalA => terminalA;
        public SparkTerminal TerminalB => terminalB;
        public float Illumination => illumination;
        // Geometric interpolation avoids a sudden resistance jump.
        public float EffectiveResistance => Mathf.Exp(Mathf.Lerp(
            Mathf.Log(Mathf.Max(1f, darkResistanceOhms)),
            Mathf.Log(Mathf.Max(1f, brightResistanceOhms)), illumination));
        public void SetIllumination(float value)
        {
            value = Mathf.Clamp01(value);
            if (Mathf.Approximately(illumination, value)) return;
            illumination = value;
            NotifyElectricalConfigurationChanged();
        }
        private void OnValidate()
        {
            darkResistanceOhms = Mathf.Max(1f, darkResistanceOhms);
            brightResistanceOhms = Mathf.Max(1f, brightResistanceOhms);
            illumination = Mathf.Clamp01(illumination);
        }
    }
}
