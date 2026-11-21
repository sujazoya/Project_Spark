using System;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace ProjectSpark.Level115
{
    /// <summary>
    /// Educational demo ONLY. Hash matching tests integrity but is NOT signature verification
    /// or secure boot. Never grants or updates LevelGamePlayManager objectives.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkL115TrustFlowTeachingDemo : MonoBehaviour
    {
        [SerializeField, TextArea(2, 4)] private string illustrativeFirmware = "ProjectSpark-SHX23-EducationalFirmware-v1";
        [SerializeField] private bool runAtStartup;
        [SerializeField] private string lastIllustrativeDigest;
        [SerializeField] private string lastOutcome = "NOT_RUN";
        public bool IsAuthoritative => false;
        public string LastIllustrativeDigest => lastIllustrativeDigest;
        public string LastOutcome => lastOutcome;

        private void Start()
        {
            if (runAtStartup) RunTeachingDemo();
        }

        [ContextMenu("Run educational SHA-256 integrity demonstration")]
        public void RunTeachingDemo()
        {
            lastIllustrativeDigest = CalculateSha256Hex(illustrativeFirmware);
            lastOutcome = "DEMO_HASH_ONLY_NOT_A_DIGITAL_SIGNATURE";
            Debug.Log("[L115][DEMO] SHA-256=" + lastIllustrativeDigest +
                "; this does NOT attest firmware authenticity or award Level 115 objectives.", this);
        }

        public static string CalculateSha256Hex(string value)
        {
            using (SHA256 hash = SHA256.Create())
            {
                byte[] bytes = hash.ComputeHash(Encoding.UTF8.GetBytes(value ?? string.Empty));
                StringBuilder result = new StringBuilder(bytes.Length * 2);
                for (int i = 0; i < bytes.Length; i++) result.Append(bytes[i].ToString("x2"));
                return result.ToString();
            }
        }
    }
}
