using UnityEngine;

namespace ProjectSpark.Gameplay
{
    /// <summary>
    /// Incandescent-style FILAMENT VISUAL driven by the SparkLED solver.
    /// This is a visual approximation; it does not change the LED electrical model.
    /// Solver supplies anode-to-cathode voltage and forward current in amperes.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkLEDHelixGlow : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Renderer helixRenderer;
        [SerializeField] private Light glowLight;

        [Header("Electrical To Heat (Visual Only)")]
        [SerializeField, Min(0.01f)] private float nominalVoltage = 3f;
        [SerializeField, Min(0.000001f)] private float nominalCurrentAmps = 0.02f;
        [SerializeField, Min(0f)] private float minimumGlowVoltage = 0.5f;
        [SerializeField, Min(0f)] private float minimumGlowCurrentAmps = 0.0001f;
        [SerializeField] private AnimationCurve heatResponse = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        [Header("Filament Thermal Inertia")]
        [SerializeField, Min(0.01f)] private float heatUpSeconds = 0.28f;
        [SerializeField, Min(0.01f)] private float coolDownSeconds = 0.65f;

        [Header("Emission And Local Light")]
        [SerializeField, Min(0f)] private float maximumEmission = 6f;
        [SerializeField, Min(0f)] private float maximumLightIntensity = 3f;
        [SerializeField] private Color lightColor = new Color(1f, 0.66f, 0.30f, 1f);

        [Header("Diagnostics")]
        [SerializeField] private bool logStateChanges;
        [SerializeField] private float solvedVoltage;
        [SerializeField] private float solvedCurrent;
        [SerializeField, Range(0f, 1f)] private float filamentHeat;

        private MaterialPropertyBlock propertyBlock;
        private static readonly int EmissionId = Shader.PropertyToID("_EmissionColor");
        private static readonly int HeatId = Shader.PropertyToID("_FilamentHeat");

        public float SolvedVoltage => solvedVoltage;
        public float SolvedCurrent => solvedCurrent;
        public float Brightness => filamentHeat;

        private void Awake()
        {
            if (helixRenderer == null) helixRenderer = GetComponent<Renderer>();
            if (helixRenderer == null) helixRenderer = GetComponentInChildren<Renderer>(true);
            propertyBlock = new MaterialPropertyBlock();
            solvedVoltage = 0f;
            solvedCurrent = 0f;
            filamentHeat = 0f;
            ApplyVisual();
        }

        private void Update()
        {
            float target = TargetHeat();
            float tau = target > filamentHeat ? heatUpSeconds : coolDownSeconds;
            // Frame-rate independent first-order thermal smoothing.
            float blend = 1f - Mathf.Exp(-Time.deltaTime / Mathf.Max(0.01f, tau));
            filamentHeat = Mathf.Lerp(filamentHeat, target, blend);
            if (target == 0f && filamentHeat < 0.0005f) filamentHeat = 0f;
            ApplyVisual();
        }

        public void SetSolvedElectricalState(float voltage, float current)
        {
            if (float.IsNaN(voltage) || float.IsInfinity(voltage)) voltage = 0f;
            if (float.IsNaN(current) || float.IsInfinity(current)) current = 0f;

            if (logStateChanges &&
                (Mathf.Abs(solvedVoltage - voltage) > 0.05f ||
                 Mathf.Abs(solvedCurrent - current) > 0.0001f))
            {
                Debug.Log($"[FILAMENT] LED V={voltage:F3} V, I={current * 1000f:F3} mA", this);
            }
            solvedVoltage = voltage;
            solvedCurrent = current;
        }

        public void ClearElectricalState()
        {
            solvedVoltage = 0f;
            solvedCurrent = 0f;
            // Heat is intentionally NOT reset here: it fades like a hot filament.
        }

        private float TargetHeat()
        {
            if (solvedVoltage <= minimumGlowVoltage || solvedCurrent <= minimumGlowCurrentAmps)
                return 0f; // Reverse-biased or nonconducting LEDs never glow.

            float current01 = Mathf.Clamp01(
                (solvedCurrent - minimumGlowCurrentAmps) /
                Mathf.Max(0.000001f, nominalCurrentAmps - minimumGlowCurrentAmps));
            float voltage01 = Mathf.Clamp01(
                (solvedVoltage - minimumGlowVoltage) /
                Mathf.Max(0.0001f, nominalVoltage - minimumGlowVoltage));

            // Forward current dominates. Voltage gently reduces heat at low bias.
            return Mathf.Clamp01(heatResponse.Evaluate(current01) *
                                 Mathf.Lerp(0.35f, 1f, voltage01));
        }

        private void ApplyVisual()
        {
            if (helixRenderer != null)
            {
                if (propertyBlock == null) propertyBlock = new MaterialPropertyBlock();
                helixRenderer.GetPropertyBlock(propertyBlock);
                propertyBlock.SetFloat(HeatId, filamentHeat);
                // Use a neutral emission magnitude: the shader supplies red/orange/hot color.
                propertyBlock.SetColor(EmissionId,
                    Color.white * (maximumEmission * filamentHeat));
                helixRenderer.SetPropertyBlock(propertyBlock);
            }

            if (glowLight != null)
            {
                glowLight.color = lightColor;
                glowLight.intensity = maximumLightIntensity * filamentHeat * filamentHeat;
                glowLight.enabled = filamentHeat > 0.01f;
            }
        }

        private void OnDisable()
        {
            solvedVoltage = 0f;
            solvedCurrent = 0f;
            filamentHeat = 0f;
            ApplyVisual();
        }

        private void OnDestroy()
        {
            if (glowLight != null) glowLight.enabled = false;
        }

        [ContextMenu("Test / Warm Filament")]
        private void TestWarm()
        {
            if (Application.isPlaying)
                SetSolvedElectricalState(nominalVoltage, nominalCurrentAmps * 0.25f);
        }

        [ContextMenu("Test / Full Filament")]
        private void TestFull()
        {
            if (Application.isPlaying)
                SetSolvedElectricalState(nominalVoltage, nominalCurrentAmps);
        }

        [ContextMenu("Test / Cool Filament")]
        private void TestCool()
        {
            if (Application.isPlaying) ClearElectricalState();
        }
    }
}
