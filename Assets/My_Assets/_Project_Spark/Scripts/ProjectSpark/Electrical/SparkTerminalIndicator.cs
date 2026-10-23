
using UnityEngine;

namespace ProjectSpark.Gameplay
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SparkTerminal))]
    public sealed class SparkTerminalIndicator : MonoBehaviour
    {
        public enum IndicatorMode
        {
            Off,
            LowHigh,
            Blink
        }

        public enum DetectionMode
        {
            Voltage,
            Current,
            VoltageOrCurrent,
            VoltageAndCurrent
        }

        [Header("Indicator Renderer")]
        [SerializeField]
        private MeshRenderer indicatorRenderer;

        [Header("Material Property")]
        [Tooltip(
            "Shader float property controlling brightness. " +
            "Example: _Intensity")]
        [SerializeField]
        private string intensityProperty = "_Intensity";

        [Header("Indicator Mode")]
        [SerializeField]
        private IndicatorMode indicatorMode =
            IndicatorMode.LowHigh;

        [SerializeField]
        private DetectionMode detectionMode =
            DetectionMode.VoltageOrCurrent;

        [Header("Detection Thresholds")]
        [SerializeField, Min(0f)]
        private float minimumVoltage = 0.05f;

        [SerializeField, Min(0f)]
        private float minimumCurrent = 0.001f;

        [Header("Intensity")]
        [SerializeField, Min(0f)]
        private float offIntensity = 0f;

        [SerializeField, Min(0f)]
        private float lowIntensity = 0.5f;

        [SerializeField, Min(0f)]
        private float highIntensity = 3f;

        [SerializeField, Min(0f)]
        private float highVoltageThreshold = 3f;

        [SerializeField, Min(0f)]
        private float highCurrentThreshold = 0.02f;

        [Header("Blink Settings")]
        [SerializeField, Min(0.01f)]
        private float blinkSpeed = 2f;

        [SerializeField]
        private bool smoothBlink = true;

        [SerializeField]
        private bool useUnscaledTime;

        [Header("Runtime Debug")]
        [SerializeField]
        private bool showRuntimeValues;

        [SerializeField]
        private bool isActive;

        [SerializeField]
        private float currentIntensity;

        private SparkTerminal terminal;
        private MaterialPropertyBlock propertyBlock;
        private int intensityPropertyId;

        private float voltage;
        private float current;

        public bool IsActive => isActive;

        public float CurrentIntensity => currentIntensity;

        private void Awake()
        {
            terminal = GetComponent<SparkTerminal>();

            ResolveRenderer();
            InitializeMaterial();
        }

        private void OnEnable()
        {
            if (terminal == null)
                terminal = GetComponent<SparkTerminal>();

            terminal.ElectricalStateChanged +=
                OnElectricalStateChanged;

            RefreshElectricalState();
        }

        private void OnDisable()
        {
            if (terminal != null)
            {
                terminal.ElectricalStateChanged -=
                    OnElectricalStateChanged;
            }

            isActive = false;
            SetIntensity(offIntensity);
        }

        private void OnValidate()
        {
            minimumVoltage = Mathf.Max(
                0f, minimumVoltage);

            minimumCurrent = Mathf.Max(
                0f, minimumCurrent);

            highVoltageThreshold = Mathf.Max(
                minimumVoltage, highVoltageThreshold);

            highCurrentThreshold = Mathf.Max(
                minimumCurrent, highCurrentThreshold);

            blinkSpeed = Mathf.Max(
                0.01f, blinkSpeed);

            ResolveRenderer();

            intensityPropertyId =
                Shader.PropertyToID(intensityProperty);
        }

        private void Update()
        {
            if (indicatorMode != IndicatorMode.Blink)
                return;

            if (!isActive)
                return;

            float time = useUnscaledTime
                ? Time.unscaledTime
                : Time.time;

            float phase =
                time * blinkSpeed * Mathf.PI * 2f;

            float blinkValue;

            if (smoothBlink)
            {
                blinkValue =
                    (Mathf.Sin(phase) + 1f) * 0.5f;
            }
            else
            {
                blinkValue =
                    Mathf.Sin(phase) >= 0f ? 1f : 0f;
            }

            float intensity = Mathf.Lerp(
                lowIntensity,
                highIntensity,
                blinkValue);

            SetIntensity(intensity);
        }

        private void OnElectricalStateChanged(
            SparkTerminal changedTerminal)
        {
            RefreshElectricalState();
        }

        private void RefreshElectricalState()
        {
            if (terminal == null)
                return;

            voltage = Mathf.Abs(terminal.Voltage);
            current = Mathf.Abs(terminal.Current);

            bool voltageDetected =
                voltage >= minimumVoltage;

            bool currentDetected =
                current >= minimumCurrent;

            switch (detectionMode)
            {
                case DetectionMode.Voltage:
                    isActive = voltageDetected;
                    break;

                case DetectionMode.Current:
                    isActive = currentDetected;
                    break;

                case DetectionMode.VoltageAndCurrent:
                    isActive =
                        voltageDetected &&
                        currentDetected;
                    break;

                default:
                    isActive =
                        voltageDetected ||
                        currentDetected;
                    break;
            }

            if (indicatorMode == IndicatorMode.Off ||
                !isActive)
            {
                SetIntensity(offIntensity);
                return;
            }

            if (indicatorMode == IndicatorMode.LowHigh)
            {
                bool highActivity =
                    voltage >= highVoltageThreshold ||
                    current >= highCurrentThreshold;

                SetIntensity(
                    highActivity
                        ? highIntensity
                        : lowIntensity);
            }
        }

        private void ResolveRenderer()
        {
            if (indicatorRenderer == null)
            {
                indicatorRenderer =
                    GetComponentInChildren<MeshRenderer>(
                        true);
            }
        }

        private void InitializeMaterial()
        {
            propertyBlock =
                new MaterialPropertyBlock();

            intensityPropertyId =
                Shader.PropertyToID(intensityProperty);

            SetIntensity(offIntensity);
        }

        private void SetIntensity(float intensity)
        {
            currentIntensity = intensity;

            if (indicatorRenderer == null)
                return;

            if (propertyBlock == null)
            {
                propertyBlock =
                    new MaterialPropertyBlock();
            }

            indicatorRenderer.GetPropertyBlock(
                propertyBlock);

            propertyBlock.SetFloat(
                intensityPropertyId,
                intensity);

            indicatorRenderer.SetPropertyBlock(
                propertyBlock);
        }

        public void SetMode(IndicatorMode mode)
        {
            indicatorMode = mode;
            RefreshElectricalState();
        }

        public void Refresh()
        {
            RefreshElectricalState();
        }
    }
}
