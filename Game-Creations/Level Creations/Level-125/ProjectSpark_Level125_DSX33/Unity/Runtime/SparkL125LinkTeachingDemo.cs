using UnityEngine;
namespace ProjectSpark.Level125
{
    // Illustrative free-space link calculation: no RF device control, no objective authority.
    [DisallowMultipleComponent]
    public sealed class SparkL125LinkTeachingDemo : MonoBehaviour
    {
        [SerializeField, Min(0.001f)] private float frequencyMHz = 2200f;
        [SerializeField, Min(0.001f)] private float distanceKm = 100000f;
        [SerializeField] private float transmitPowerDbm = 30f;
        [SerializeField] private float transmitGainDbi = 12f;
        [SerializeField] private float receiveGainDbi = 55f;
        [SerializeField, Min(0f)] private float otherLossDb = 3f;
        [SerializeField] private float receiverSensitivityDbm = -120f;
        public bool IsCertified => false;
        public bool CanAuthorizeTransmitter => false;
        public bool TryCalculate(out float freeSpaceLossDb, out float receivedDbm, out float linkMarginDb)
        {
            freeSpaceLossDb = receivedDbm = linkMarginDb = 0f;
            if (!(frequencyMHz > 0f) || !(distanceKm > 0f) || otherLossDb < 0f) return false;
            freeSpaceLossDb = 32.44f + 20f * Mathf.Log10(frequencyMHz) + 20f * Mathf.Log10(distanceKm);
            receivedDbm = transmitPowerDbm + transmitGainDbi + receiveGainDbi - freeSpaceLossDb - otherLossDb;
            linkMarginDb = receivedDbm - receiverSensitivityDbm;
            return !(float.IsNaN(freeSpaceLossDb) || float.IsInfinity(freeSpaceLossDb) || float.IsNaN(linkMarginDb) || float.IsInfinity(linkMarginDb));
        }
    }
}
