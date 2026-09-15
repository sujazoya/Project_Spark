using UnityEngine;

namespace ProjectSpark.Gameplay
{
    public sealed class SparkResistor : SparkElectricalComponent
    {
        [SerializeField, Min(0.000001f)] private float resistanceOhms = 1000f;
        [SerializeField, Range(0f, 100f)] private float tolerancePercent = 5f;
        [SerializeField, Min(0.001f)] private float powerRatingWatts = 0.25f;

        public float ResistanceOhms => resistanceOhms;
        public float TolerancePercent => tolerancePercent;
        public float PowerRatingWatts => powerRatingWatts;
        public bool IsOverPower => Mathf.Abs(ElectricalState.Power) > powerRatingWatts;

        private void OnValidate()
        { resistanceOhms = Mathf.Max(0.000001f, resistanceOhms); tolerancePercent = Mathf.Clamp(tolerancePercent, 0, 100); powerRatingWatts = Mathf.Max(0.001f, powerRatingWatts); }
    }
}
