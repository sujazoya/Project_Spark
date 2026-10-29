using System.Collections;
using UnityEngine;
using UnityEngine.VFX;

namespace ProjectSpark.Gameplay
{
    /// <summary>
    /// VISUAL-ONLY preview of the complete resistor failure effect.
    /// Uses EXISTING, Inspector-assigned scene objects. Does not damage
    /// the resistor, change its electrical state, or alter level progress.
    /// Add to the existing FailureVFX GameObject in the Unity Editor.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkFullBurnVFXDemo : MonoBehaviour
    {
        [Header("Existing references: drag in Inspector")]
        [SerializeField] private SparkResistorFailureBurstVFX failureBurst;
        [SerializeField] private VisualEffect smokeVisualEffect;
        [SerializeField] private SparkResistorSmokeGraphDriver smokeDriver;

        [Header("Demo trigger")]
        [SerializeField] private bool playOnStart;
        [SerializeField] private bool enableKeyboardShortcut = true;
        [SerializeField] private KeyCode previewKey = KeyCode.B;

        [Header("Smoke preview")]
        [SerializeField, Min(0f)] private float lightSmokeRate = 8f;
        [SerializeField, Min(0f)] private float heavySmokeRate = 55f;
        [SerializeField, Min(0f)] private float warmupSeconds = 1.3f;
        [SerializeField, Min(0f)] private float heavySmokeSeconds = 2f;
        [SerializeField, Min(0f)] private float fadeSeconds = 1.5f;

        private static readonly int SmokeRateId = Shader.PropertyToID("SmokeRate");
        private Coroutine demoCoroutine;
        private bool restoreSmokeDriver;
        private bool hasSmokeDriverOverride;

        private void Awake()
        {
            if (failureBurst == null)
                failureBurst = GetComponent<SparkResistorFailureBurstVFX>();
        }

        private void Start()
        {
            if (playOnStart)
                PlayDemo();
        }

        private void Update()
        {
            if (enableKeyboardShortcut && Input.GetKeyDown(previewKey))
                PlayDemo();
        }

        [ContextMenu("VFX / Play Complete Burn Demo")]
        public void PlayDemo()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("[Burn VFX Demo] Enter Play Mode first.", this);
                return;
            }

            StopDemo();
            demoCoroutine = StartCoroutine(RunDemo());
        }

        [ContextMenu("VFX / Stop Complete Burn Demo")]
        public void StopDemo()
        {
            if (demoCoroutine != null)
            {
                StopCoroutine(demoCoroutine);
                demoCoroutine = null;
            }

            SetSmokeRate(0f);
            RestoreDriver();
        }

        private IEnumerator RunDemo()
        {
            // The normal smoke driver updates the same SmokeRate value.
            // Temporarily disable it so this preview has exclusive control.
            if (smokeDriver != null)
            {
                restoreSmokeDriver = smokeDriver.enabled;
                hasSmokeDriverOverride = true;
                smokeDriver.enabled = false;
            }

            if (failureBurst == null)
                Debug.LogWarning("[Burn VFX Demo] Assign Failure Burst.", this);
            if (smokeVisualEffect == null)
                Debug.LogWarning("[Burn VFX Demo] Assign Smoke Visual Effect.", this);

            // 1. Pre-failure light smoke, rising smoothly.
            yield return RampSmoke(0f, lightSmokeRate, warmupSeconds);

            // 2. Flash + sparks + debris, using your existing burst script.
            if (failureBurst != null)
                failureBurst.PlayFailureBurst();

            // 3. Heavy smoke after the burst.
            SetSmokeRate(heavySmokeRate);
            if (heavySmokeSeconds > 0f)
                yield return new WaitForSeconds(heavySmokeSeconds);

            // 4. Stop generating new smoke gradually.
            yield return RampSmoke(heavySmokeRate, 0f, fadeSeconds);
            SetSmokeRate(0f);

            demoCoroutine = null;
            RestoreDriver();
            Debug.Log("[Burn VFX Demo] Visual preview complete. No electrical state changed.", this);
        }

        private IEnumerator RampSmoke(float from, float to, float duration)
        {
            if (duration <= 0f)
            {
                SetSmokeRate(to);
                yield break;
            }

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                t = t * t * (3f - 2f * t);
                SetSmokeRate(Mathf.Lerp(from, to, t));
                yield return null;
            }
            SetSmokeRate(to);
        }

        private void SetSmokeRate(float rate)
        {
            if (smokeVisualEffect != null && smokeVisualEffect.HasFloat(SmokeRateId))
                smokeVisualEffect.SetFloat(SmokeRateId, Mathf.Max(0f, rate));
        }

        private void RestoreDriver()
        {
            if (smokeDriver != null && hasSmokeDriverOverride)
            {
                smokeDriver.enabled = restoreSmokeDriver;
                hasSmokeDriverOverride = false;
                restoreSmokeDriver = false;
            }
        }

        private void OnDisable()
        {
            StopDemo();
        }

        private void OnValidate()
        {
            warmupSeconds = Mathf.Max(0f, warmupSeconds);
            heavySmokeSeconds = Mathf.Max(0f, heavySmokeSeconds);
            fadeSeconds = Mathf.Max(0f, fadeSeconds);
        }
    }
}
