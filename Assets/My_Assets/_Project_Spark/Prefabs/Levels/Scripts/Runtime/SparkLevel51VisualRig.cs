using UnityEngine;

namespace ProjectSpark.Gameplay
{
    /// <summary>
    /// Level 51 VISUAL ONLY. Drives mesh pivots from externally supplied, authoritative
    /// motor/encoder/condition data. Does not simulate motor physics, electrical
    /// readings, bearing faults, health scores, or objective completion.
    ///
    /// The imported FBX's named groups have centered pivots. Drag them to the
    /// Inspector references, keeping this controller on the layout root.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkLevel51VisualRig : MonoBehaviour
    {
        [Header("Rotating FBX Groups")]
        [SerializeField] private Transform motorShaftRotateX;
        [SerializeField] private Transform diagnosticRotorRotateX;
        [SerializeField] private Transform encoderDiscRotateX;
        [Header("Interactive FBX Groups")]
        [SerializeField] private Transform safetyDoorHingeY;
        [SerializeField] private GameObject eccentricTestWeight;
        [SerializeField] private GameObject serviceBearingCartridge;
        [SerializeField] private GameObject healthyReplacementBearing;
        [SerializeField] private GameObject wornReplacementBearing;
        [Header("Visual Indicator Renderers (Optional)")]
        [SerializeField] private Renderer machineHealthIndicator;
        [SerializeField] private Renderer warningIndicator;
        [SerializeField] private Color healthyColor = new Color(.12f, 1f, .55f);
        [SerializeField] private Color warningColor = new Color(1f, .62f, .13f);
        [SerializeField] private Color faultColor = new Color(1f, .18f, .22f);
        [Header("Display-only Motion")]
        [SerializeField] private float doorOpenAngle = 90f;
        [SerializeField, Min(0.01f)] private float doorLerpSpeed = 5f;
        [SerializeField] private bool showWarningsInConsole;

        private float actualMotorRpm;
        private float actualEncoderRpm;
        private float targetDoorAngle;
        private bool fault;
        private bool warning;
        private Quaternion doorInitial;
        private MaterialPropertyBlock block;

        private void Awake()
        {
            if (safetyDoorHingeY != null) doorInitial = safetyDoorHingeY.localRotation;
            block = new MaterialPropertyBlock();
            RefreshIndicators();
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            // Rotations are aesthetic only. Mechanically measured RPM is supplied externally.
            float motorDegrees = actualMotorRpm * 6f * dt;
            float encoderDegrees = actualEncoderRpm * 6f * dt;
            if (motorShaftRotateX != null)
                motorShaftRotateX.Rotate(Vector3.right, motorDegrees, Space.Self);
            if (diagnosticRotorRotateX != null)
                diagnosticRotorRotateX.Rotate(Vector3.right, motorDegrees, Space.Self);
            if (encoderDiscRotateX != null)
                encoderDiscRotateX.Rotate(Vector3.right, encoderDegrees, Space.Self);

            if (safetyDoorHingeY != null)
            {
                Quaternion desired = doorInitial * Quaternion.Euler(0f, targetDoorAngle, 0f);
                safetyDoorHingeY.localRotation = Quaternion.Slerp(
                    safetyDoorHingeY.localRotation, desired,
                    Mathf.Clamp01(doorLerpSpeed * dt));
            }
        }

        /// <summary>Apply genuine measured shaft and encoder RPMs. Never call with guessed readings.</summary>
        public void SetMeasuredSpeed(float motorRpm, float encoderRpm)
        {
            actualMotorRpm = float.IsNaN(motorRpm) || float.IsInfinity(motorRpm)
                ? 0f : motorRpm;
            actualEncoderRpm = float.IsNaN(encoderRpm) || float.IsInfinity(encoderRpm)
                ? 0f : encoderRpm;
        }

        /// <summary>Reflect the actual sensor/maintenance state, not an objective gate.</summary>
        public void SetCondition(bool isWarning, bool isFault)
        {
            warning = isWarning; fault = isFault;
            RefreshIndicators();
        }

        public void SetGuardOpen(bool isOpen)
        {
            targetDoorAngle = isOpen ? doorOpenAngle : 0f;
        }

        public void SetEccentricWeightInstalled(bool installed)
        {
            if (eccentricTestWeight != null) eccentricTestWeight.SetActive(installed);
        }

        public void SetBearingServiceState(bool serviceCartridgeInstalled, bool healthySpareVisible, bool wornSpareVisible)
        {
            if (serviceBearingCartridge != null)
                serviceBearingCartridge.SetActive(serviceCartridgeInstalled);
            if (healthyReplacementBearing != null)
                healthyReplacementBearing.SetActive(healthySpareVisible);
            if (wornReplacementBearing != null)
                wornReplacementBearing.SetActive(wornSpareVisible);
        }

        private void RefreshIndicators()
        {
            if (block == null) block = new MaterialPropertyBlock();
            SetRendererColor(machineHealthIndicator, fault ? faultColor : warning ? warningColor : healthyColor);
            SetRendererColor(warningIndicator, fault ? faultColor : warning ? warningColor : Color.black);
            if (fault && showWarningsInConsole)
                Debug.Log("[Level 51 Visuals] Fault lamp enabled by external condition state.", this);
        }

        private void SetRendererColor(Renderer renderer, Color color)
        {
            if (renderer == null) return;
            renderer.GetPropertyBlock(block);
            Material mat = renderer.sharedMaterial;
            if (mat != null && mat.HasProperty("_BaseColor")) block.SetColor("_BaseColor", color);
            if (mat != null && mat.HasProperty("_Color")) block.SetColor("_Color", color);
            if (mat != null && mat.HasProperty("_EmissionColor")) block.SetColor("_EmissionColor", color);
            renderer.SetPropertyBlock(block);
            block.Clear();
        }
    }
}
