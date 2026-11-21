using UnityEngine;
namespace ProjectSpark.Level126
{
    // Idealized energy accounting only, never a charger or flight-rated controller.
    [DisallowMultipleComponent]
    public sealed class SparkL126EnergyTeachingDemo : MonoBehaviour
    {
        [SerializeField, Min(0.01f)] private float capacityWh = 240f;
        [SerializeField, Range(0f, 1f)] private float initialSoc = 0.8f;
        [SerializeField, Min(0f)] private float solarWatts = 180f;
        [SerializeField, Min(0f)] private float essentialWatts = 60f;
        [SerializeField, Min(0f)] private float auxiliaryWatts = 45f;
        [SerializeField, Range(0.01f,1f)] private float chargeEfficiency = 0.92f;
        [SerializeField, Range(0.01f,1f)] private float dischargeEfficiency = 0.90f;
        [SerializeField, Range(0f,1f)] private float reserveSoc = 0.25f;
        public bool IsCertified => false;
        public bool CanAuthorizeLoads => false;
        public bool TryPredict(bool eclipse, float durationMinutes, bool auxiliaryEnabled,
            out float finalSoc, out float unservedWh, out bool reserveReached)
        {
            finalSoc = unservedWh = 0f; reserveReached = true;
            if (!(capacityWh > 0f) || !(durationMinutes >= 0f) || float.IsInfinity(durationMinutes) ||
                !(chargeEfficiency > 0f && chargeEfficiency <= 1f) ||
                !(dischargeEfficiency > 0f && dischargeEfficiency <= 1f) ||
                !(solarWatts >= 0f && essentialWatts >= 0f && auxiliaryWatts >= 0f)) return false;
            float energy = capacityWh * Mathf.Clamp01(initialSoc);
            float hours = durationMinutes / 60f;
            float balance = (eclipse ? 0f : solarWatts) - essentialWatts - (auxiliaryEnabled ? auxiliaryWatts : 0f);
            float delta = balance >= 0f ? balance * chargeEfficiency * hours : balance / dischargeEfficiency * hours;
            float raw = energy + delta;
            if (float.IsNaN(raw) || float.IsInfinity(raw)) return false;
            finalSoc = Mathf.Clamp01(raw / capacityWh);
            unservedWh = Mathf.Max(0f, -raw) * dischargeEfficiency;
            reserveReached = finalSoc <= reserveSoc;
            return true;
        }
    }
}
