using System;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace ProjectSpark.Level116
{
    /// <summary>
    /// Simple isolated educational crash-consistency and PLP demonstrations.
    /// This is NOT a physical/electrical NAND model and NEVER awards gameplay objectives.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkL116StorageTeachingDemo : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float capacitanceFarads = .22f;
        [SerializeField, Min(0f)] private float initialVoltage = 3.30f;
        [SerializeField, Min(0f)] private float minimumVoltage = 2.70f;
        [SerializeField, Min(.01f)] private float flushPowerWatts = 1.2f;
        [SerializeField] private bool runAtStartup = false;
        [SerializeField] private string lastDemonstration = "NOT_RUN";
        public bool IsAuthoritative => false;
        public string LastDemonstration => lastDemonstration;

        private void Start() { if (runAtStartup) RunIllustrativeStorageDemo(); }

        [ContextMenu("Run isolated storage and hold-up teaching demo")]
        public void RunIllustrativeStorageDemo()
        {
            float storedJoules = HoldUpJoules(capacitanceFarads, initialVoltage, minimumVoltage);
            float idealSeconds = storedJoules / Mathf.Max(.01f, flushPowerWatts);
            // An old committed journal transaction survives a simulated interrupted new one.
            string checkpoint = "L116:committed_page_A";
            string pending = "L116:uncommitted_page_B";
            string recovered = ReplayIllustrativeCheckpoint(checkpoint, pending, false);
            lastDemonstration = "DEMO_ONLY: idealized_energy_J=" + storedJoules.ToString("F4") +
                "; idealized_hold_up_s=" + idealSeconds.ToString("F4") +
                "; old_commit_recovered=" + (recovered == checkpoint) +
                "; hash_prefix=" + HashPrefix(recovered) +
                "; NO_ELECTRICAL_OR_STORAGE_CERTIFICATION";
            Debug.Log("[L116][DEMO] " + lastDemonstration, this);
        }

        public static float HoldUpJoules(float capacitance, float vHigh, float vLow)
        {
            if (capacitance <= 0f || vHigh <= vLow || vLow < 0f) return 0f;
            return .5f * capacitance * (vHigh * vHigh - vLow * vLow);
        }

        public static string ReplayIllustrativeCheckpoint(string durable, string pending, bool pendingCommitted)
        {
            return pendingCommitted ? (pending ?? string.Empty) : (durable ?? string.Empty);
        }

        private static string HashPrefix(string text)
        {
            using (SHA256 algorithm = SHA256.Create())
            {
                byte[] digest = algorithm.ComputeHash(Encoding.UTF8.GetBytes(text ?? string.Empty));
                return BitConverter.ToString(digest, 0, 4).Replace("-", string.Empty);
            }
        }
    }
}
