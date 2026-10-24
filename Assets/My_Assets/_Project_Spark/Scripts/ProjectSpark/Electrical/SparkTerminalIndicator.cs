
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

        public enum PopupDisplayMode
        {
            NewVoltage,
            VoltageChange,
            Both
        }

        [Header("Indicator Renderer")]
        [SerializeField]
        private MeshRenderer indicatorRenderer;

        [Header("Shader Properties")]
        [SerializeField]
        private string intensityProperty = "_Intensity";

        [SerializeField]
        private bool updateElectricalShaderProperties = true;

        [Header("Indicator Mode")]
        [SerializeField]
        private IndicatorMode indicatorMode = IndicatorMode.LowHigh;

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

        [Header("Smooth Glow")]
        [SerializeField]
        private bool smoothIntensity = true;

        [SerializeField, Min(0.01f)]
        private float intensityRiseSpeed = 12f;

        [SerializeField, Min(0.01f)]
        private float intensityFallSpeed = 6f;

        [Header("Power-On Burst")]
        [SerializeField]
        private bool enablePowerOnBurst = true;

        [SerializeField, Min(0f)]
        private float burstIntensity = 2f;

        [SerializeField, Min(0.01f)]
        private float burstDuration = 0.35f;

        [Header("Blink")]
        [SerializeField, Min(0.01f)]
        private float blinkSpeed = 2f;

        [SerializeField]
        private bool smoothBlink = true;

        [SerializeField]
        private bool useUnscaledTime;

        [Header("Short Circuit")]
        [SerializeField]
        private bool shortCircuitActive;

        [SerializeField, Min(0.01f)]
        private float shortCircuitFlashSpeed = 12f;

        [SerializeField, Min(0f)]
        private float shortCircuitFlashIntensity = 5f;

        [Header("Voltage Popup")]
        [SerializeField]
        private bool enableVoltagePopup = true;

        [SerializeField]
        private SparkVoltagePopup popupPrefab;

        [SerializeField]
        private PopupDisplayMode popupDisplayMode =
            PopupDisplayMode.Both;

        [SerializeField, Min(0f)]
        private float popupVoltageThreshold = 0.05f;

        [SerializeField, Min(0f)]
        private float popupCooldown = 0.12f;

        [SerializeField]
        private bool popupOnInitialVoltage;

        [SerializeField]
        private bool reusePopup = true;

        [Header("Popup Position")]
        [SerializeField]
        private bool useIndicatorMeshPosition = true;

        [SerializeField]
        private Vector3 popupOffset =
            new Vector3(0f, 0.2f, 0f);

        [SerializeField]
        private bool offsetUsesLocalSpace = false;

        [Header("Popup Colors")]
        [SerializeField]
        private Color positiveVoltageColor =
            new Color(0.25f, 1f, 0.55f);

        [SerializeField]
        private Color negativeVoltageColor =
            new Color(0.35f, 0.7f, 1f);

        [SerializeField]
        private Color voltageDecreaseColor =
            new Color(1f, 0.7f, 0.2f);

        [SerializeField]
        private Color zeroVoltageColor =
            new Color(0.75f, 0.8f, 0.9f);

        [Header("Runtime Debug")]
        [SerializeField]
        private bool showRuntimeValues;

        [SerializeField]
        private bool isActive;

        [SerializeField]
        private float currentIntensity;

        [SerializeField]
        private float voltage;

        [SerializeField]
        private float current;

        private SparkTerminal terminal;
        private MaterialPropertyBlock propertyBlock;

        private int intensityPropertyId;

        private static readonly int VoltageId =
            Shader.PropertyToID("_Voltage");

        private static readonly int PolarityId =
            Shader.PropertyToID("_Polarity");

        private static readonly int ShortCircuitId =
            Shader.PropertyToID("_ShortCircuit");

        private SparkVoltagePopup activePopup;

        private bool hasPreviousVoltage;
        private float previousVoltage;

        private float lastPopupTime =
            float.NegativeInfinity;

        private float targetIntensity;
        private float burstRemaining;
        private float signedVoltage;

        public bool IsActive => isActive;
        public float CurrentIntensity => currentIntensity;
        public float Voltage => voltage;
        public float Current => current;

        private void Awake()
        {
            terminal = GetComponent<SparkTerminal>();

            ResolveRenderer();

            propertyBlock = new MaterialPropertyBlock();

            intensityPropertyId =
                Shader.PropertyToID(intensityProperty);
        }

        private void OnEnable()
        {
            if (terminal == null)
                terminal = GetComponent<SparkTerminal>();

            terminal.ElectricalStateChanged -=
                OnElectricalStateChanged;

            terminal.ElectricalStateChanged +=
                OnElectricalStateChanged;

            hasPreviousVoltage = false;
            lastPopupTime = float.NegativeInfinity;

            RefreshElectricalState();
            UpdateShaderProperties(currentIntensity);
        }

        private void OnDisable()
        {
            if (terminal != null)
            {
                terminal.ElectricalStateChanged -=
                    OnElectricalStateChanged;
            }

            if (activePopup != null)
            {
                Destroy(activePopup.gameObject);
                activePopup = null;
            }

            hasPreviousVoltage = false;
            burstRemaining = 0f;
            isActive = false;
            targetIntensity = offIntensity;
            currentIntensity = offIntensity;

            UpdateShaderProperties(offIntensity);
        }

        private void OnValidate()
        {
            minimumVoltage = Mathf.Max(0f, minimumVoltage);
            minimumCurrent = Mathf.Max(0f, minimumCurrent);

            highVoltageThreshold =
                Mathf.Max(minimumVoltage, highVoltageThreshold);

            highCurrentThreshold =
                Mathf.Max(minimumCurrent, highCurrentThreshold);

            blinkSpeed = Mathf.Max(0.01f, blinkSpeed);
            burstDuration = Mathf.Max(0.01f, burstDuration);

            intensityRiseSpeed =
                Mathf.Max(0.01f, intensityRiseSpeed);

            intensityFallSpeed =
                Mathf.Max(0.01f, intensityFallSpeed);

            popupVoltageThreshold =
                Mathf.Max(0f, popupVoltageThreshold);

            popupCooldown = Mathf.Max(0f, popupCooldown);

            ResolveRenderer();

            intensityPropertyId =
                Shader.PropertyToID(intensityProperty);
        }

        private void Update()
        {
            float dt = useUnscaledTime
                ? Time.unscaledDeltaTime
                : Time.deltaTime;

            float time = useUnscaledTime
                ? Time.unscaledTime
                : Time.time;

            float desiredIntensity = targetIntensity;

            if (indicatorMode == IndicatorMode.Blink && isActive)
            {
                float phase =
                    time * blinkSpeed * Mathf.PI * 2f;

                float blink = smoothBlink
                    ? (Mathf.Sin(phase) + 1f) * 0.5f
                    : (Mathf.Sin(phase) >= 0f ? 1f : 0f);

                desiredIntensity =
                    Mathf.Lerp(lowIntensity, highIntensity, blink);
            }

            if (burstRemaining > 0f)
            {
                float burstFactor =
                    burstRemaining / burstDuration;

                desiredIntensity +=
                    burstIntensity * burstFactor;

                burstRemaining =
                    Mathf.Max(0f, burstRemaining - dt);
            }

            if (shortCircuitActive)
            {
                float flash =
                    Mathf.Sin(
                        time *
                        shortCircuitFlashSpeed *
                        Mathf.PI * 2f) >= 0f
                        ? 1f
                        : 0f;

                desiredIntensity =
                    Mathf.Max(
                        desiredIntensity,
                        shortCircuitFlashIntensity * flash);
            }

            if (smoothIntensity)
            {
                float speed =
                    desiredIntensity > currentIntensity
                        ? intensityRiseSpeed
                        : intensityFallSpeed;

                currentIntensity =
                    Mathf.MoveTowards(
                        currentIntensity,
                        desiredIntensity,
                        speed * dt);
            }
            else
            {
                currentIntensity = desiredIntensity;
            }

            UpdateShaderProperties(currentIntensity);
        }

        private void OnElectricalStateChanged(
            SparkTerminal changedTerminal)
        {
            if (changedTerminal == terminal)
                RefreshElectricalState();
        }

        private void RefreshElectricalState()
        {
            if (terminal == null)
                return;

            bool wasActive = isActive;

            signedVoltage = terminal.Voltage;
            voltage = Mathf.Abs(signedVoltage);
            current = Mathf.Abs(terminal.Current);

            TryShowVoltagePopup(signedVoltage);

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
                        voltageDetected && currentDetected;
                    break;

                default:
                    isActive =
                        voltageDetected || currentDetected;
                    break;
            }

            if (indicatorMode == IndicatorMode.Off)
                isActive = false;

            if (!wasActive && isActive && enablePowerOnBurst)
                burstRemaining = burstDuration;

            if (!isActive)
            {
                targetIntensity = offIntensity;
            }
            else
            {
                bool highActivity =
                    voltage >= highVoltageThreshold ||
                    current >= highCurrentThreshold;

                targetIntensity =
                    highActivity ? highIntensity : lowIntensity;
            }

            UpdateShaderProperties(currentIntensity);
        }

        private void UpdateShaderProperties(float intensity)
        {
            if (indicatorRenderer == null)
                return;

            if (propertyBlock == null)
                propertyBlock = new MaterialPropertyBlock();

            indicatorRenderer.GetPropertyBlock(propertyBlock);

            propertyBlock.SetFloat(
                intensityPropertyId,
                Mathf.Max(0f, intensity));

            if (updateElectricalShaderProperties)
            {
                propertyBlock.SetFloat(
                    VoltageId,
                    signedVoltage);

                float polarity = 0f;

                if (signedVoltage > minimumVoltage)
                    polarity = 1f;
                else if (signedVoltage < -minimumVoltage)
                    polarity = -1f;

                propertyBlock.SetFloat(
                    PolarityId,
                    polarity);

                propertyBlock.SetFloat(
                    ShortCircuitId,
                    shortCircuitActive ? 1f : 0f);
            }

            indicatorRenderer.SetPropertyBlock(propertyBlock);
        }

        private void TryShowVoltagePopup(float newVoltage)
        {
            if (!hasPreviousVoltage)
            {
                hasPreviousVoltage = true;
                previousVoltage = newVoltage;

                if (popupOnInitialVoltage &&
                    Mathf.Abs(newVoltage) >= popupVoltageThreshold)
                {
                    ShowVoltagePopup(0f, newVoltage);
                }

                return;
            }

            float oldVoltage = previousVoltage;
            previousVoltage = newVoltage;

            if (Mathf.Abs(newVoltage - oldVoltage) <
                popupVoltageThreshold)
            {
                return;
            }

            ShowVoltagePopup(oldVoltage, newVoltage);
        }

        private void ShowVoltagePopup(
            float oldVoltage,
            float newVoltage)
        {
            if (!enableVoltagePopup || popupPrefab == null)
                return;

            float now = Time.unscaledTime;

            if (now - lastPopupTime < popupCooldown)
                return;

            lastPopupTime = now;

            Vector3 spawnPosition =
                GetPopupSpawnPosition();

            Color popupColor =
                GetPopupColor(oldVoltage, newVoltage);

            if (!reusePopup)
                activePopup = null;

            if (activePopup == null)
            {
                activePopup = Instantiate(
                    popupPrefab,
                    spawnPosition,
                    Quaternion.identity);
            }

            activePopup.Play(
                oldVoltage,
                newVoltage,
                popupColor,
                popupDisplayMode,
                spawnPosition);
        }

        private Color GetPopupColor(
            float oldVoltage,
            float newVoltage)
        {
            if (Mathf.Abs(newVoltage) < minimumVoltage)
                return zeroVoltageColor;

            if (newVoltage < -minimumVoltage)
                return negativeVoltageColor;

            if (newVoltage < oldVoltage)
                return voltageDecreaseColor;

            return positiveVoltageColor;
        }

        private Vector3 GetPopupSpawnPosition()
        {
            Vector3 anchor = transform.position;

            if (useIndicatorMeshPosition &&
                indicatorRenderer != null)
            {
                anchor = indicatorRenderer.bounds.center;
            }

            Vector3 offset = offsetUsesLocalSpace
                ? transform.TransformDirection(popupOffset)
                : popupOffset;

            return anchor + offset;
        }

        private void ResolveRenderer()
        {
            if (indicatorRenderer == null)
            {
                indicatorRenderer =
                    GetComponentInChildren<MeshRenderer>(true);
            }
        }

        public void SetMode(IndicatorMode mode)
        {
            indicatorMode = mode;
            RefreshElectricalState();
        }

        public void SetShortCircuit(bool active)
        {
            shortCircuitActive = active;
            UpdateShaderProperties(currentIntensity);
        }

        public void Refresh()
        {
            RefreshElectricalState();
        }

        public void ShowCurrentVoltagePopup()
        {
            if (terminal != null)
            {
                ShowVoltagePopup(0f, terminal.Voltage);
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Vector3 position = GetPopupSpawnPosition();

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(position, 0.035f);
            Gizmos.DrawLine(transform.position, position);
        }
#endif
    }
}
