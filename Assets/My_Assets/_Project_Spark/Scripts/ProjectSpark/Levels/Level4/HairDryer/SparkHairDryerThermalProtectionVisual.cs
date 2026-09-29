using UnityEngine;

namespace ProjectSpark.Gameplay
{
    /// <summary>
    /// Visual representation of the hair-dryer's thermal protection.
    ///
    /// Visual-only component.
    ///
    /// It can show:
    /// - Normal state
    /// - Heating state
    /// - Hot state
    /// - Protection/cutoff state
    ///
    /// The actual thermal protection will later be handled
    /// by the electrical/thermal simulation.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkHairDryerThermalProtectionVisual : MonoBehaviour
    {
        private static readonly int TemperatureID =
            Shader.PropertyToID("_Temperature");

        [Header("Protection Component")]
        [SerializeField]
        private Renderer protectionRenderer;

        [Header("Visual Temperature")]
        [SerializeField]
        [Range(0f, 1f)]
        private float temperature;

        [SerializeField]
        [Range(0f, 1f)]
        private float targetTemperature;

        [SerializeField]
        [Min(0f)]
        private float responseSpeed = 1.5f;

        [Header("Protection Threshold")]
        [SerializeField]
        [Range(0f, 1f)]
        private float protectionThreshold = 0.85f;

        [Header("Material")]
        [SerializeField]
        private bool createMaterialInstance = true;

        [Header("Runtime")]
        [SerializeField]
        private bool protectionActive;

        private Material runtimeMaterial;
        private bool materialInstanceCreated;
        private float lastTemperature = -1f;

        public float Temperature => temperature;

        public bool ProtectionActive =>
            protectionActive;

        public bool IsHot =>
            temperature >= protectionThreshold;

        private void Awake()
        {
            InitializeMaterial();
            ApplyVisual(true);
        }

        private void Update()
        {
            UpdateTemperature();
            ApplyVisual(false);
        }

        private void OnDestroy()
        {
            ReleaseMaterial();
        }

        private void InitializeMaterial()
        {
            if (protectionRenderer == null)
                return;

            if (createMaterialInstance)
            {
                runtimeMaterial =
                    protectionRenderer.material;

                materialInstanceCreated =
                    runtimeMaterial != null;
            }
            else
            {
                runtimeMaterial =
                    protectionRenderer.sharedMaterial;
            }
        }

        private void ReleaseMaterial()
        {
            if (!materialInstanceCreated)
                return;

            if (runtimeMaterial == null)
                return;

            if (Application.isPlaying)
                Destroy(runtimeMaterial);
            else
                DestroyImmediate(runtimeMaterial);

            runtimeMaterial = null;
            materialInstanceCreated = false;
        }

        private void UpdateTemperature()
        {
            temperature =
                Mathf.MoveTowards(
                    temperature,
                    targetTemperature,
                    responseSpeed * Time.deltaTime);
        }

        private void ApplyVisual(bool force)
        {
            if (runtimeMaterial == null)
                return;

            if (!runtimeMaterial.HasProperty(TemperatureID))
                return;

            if (!force &&
                Mathf.Approximately(
                    temperature,
                    lastTemperature))
            {
                return;
            }

            runtimeMaterial.SetFloat(
                TemperatureID,
                temperature);

            lastTemperature =
                temperature;
        }

        public void SetTemperature(float value)
        {
            targetTemperature =
                Mathf.Clamp01(value);
        }

        public void SetTemperatureImmediate(float value)
        {
            temperature =
                Mathf.Clamp01(value);

            targetTemperature =
                temperature;

            ApplyVisual(true);
        }

        public void SetProtectionActive(bool active)
        {
            protectionActive = active;
        }

        public void ResetProtection()
        {
            protectionActive = false;
            SetTemperature(0f);
        }

        [ContextMenu("Test / Normal")]
        private void TestNormal()
        {
            SetProtectionActive(false);
            SetTemperature(0.15f);
        }

        [ContextMenu("Test / Heating")]
        private void TestHeating()
        {
            SetProtectionActive(false);
            SetTemperature(0.60f);
        }

        [ContextMenu("Test / Hot")]
        private void TestHot()
        {
            SetProtectionActive(false);
            SetTemperature(0.90f);
        }

        [ContextMenu("Test / Protection")]
        private void TestProtection()
        {
            SetProtectionActive(true);
            SetTemperature(1f);
        }

        [ContextMenu("Test / Reset")]
        private void TestReset()
        {
            ResetProtection();
        }
    }
}