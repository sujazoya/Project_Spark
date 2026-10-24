using UnityEngine;

namespace ProjectSpark.Electrical
{
    /// <summary>Drives a UV-flow wire material and optional current popup from solved component current.</summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SparkComponentCurrentFeedback))]
    public sealed class SparkCurrentVisual : MonoBehaviour
    {
        [Header("Wire visuals")]
        [SerializeField] private Renderer wireRenderer;
        [SerializeField] private bool enableWireFlow = true;
        [SerializeField, Min(0.000001f)] private float currentForFullEffect = 0.1f;
        [SerializeField, Min(0f)] private float maximumFlowSpeed = 3f;
        [SerializeField, Min(0f)] private float maximumGlow = 2f;
        [SerializeField] private bool invertVisualDirection;
        [SerializeField] private int materialIndex = -1;

        [Header("Current popup")]
        [SerializeField] private bool enablePopup = true;
        [SerializeField] private SparkCurrentPopup popupPrefab;
        [SerializeField] private Transform popupAnchor;
        [SerializeField] private Vector3 popupOffset = new Vector3(0f, 0.15f, 0f);
        [SerializeField, Min(0f)] private float popupMinimumChangeAmps = 0.001f;
        [SerializeField, Min(0f)] private float popupCooldown = 0.2f;
        [SerializeField] private bool popupOnFirstReading;
        [SerializeField] private Color positiveColor = new Color(0.2f, 1f, 0.8f);
        [SerializeField] private Color negativeColor = new Color(0.4f, 0.7f, 1f);

        private static readonly int FlowSpeedId = Shader.PropertyToID("_CurrentFlowSpeed");
        private static readonly int FlowGlowId = Shader.PropertyToID("_CurrentGlow");
        private static readonly int CurrentId = Shader.PropertyToID("_CurrentAmps");
        private SparkComponentCurrentFeedback feedback;
        private MaterialPropertyBlock block;
        private bool hadReading;
        private float lastReading;
        private float lastPopupTime = float.NegativeInfinity;

        private void Awake()
        {
            feedback = GetComponent<SparkComponentCurrentFeedback>();
            block = new MaterialPropertyBlock();
        }

        private void OnEnable()
        {
            if (feedback == null) feedback = GetComponent<SparkComponentCurrentFeedback>();
            feedback.ReadingChanged += OnReadingChanged;
            OnReadingChanged(feedback);
        }

        private void OnDisable()
        {
            if (feedback != null) feedback.ReadingChanged -= OnReadingChanged;
            hadReading = false;
            ApplyMaterial(0f, 0f, 0f);
        }

        private void OnReadingChanged(SparkComponentCurrentFeedback reading)
        {
            bool valid = reading != null && reading.HasReading;
            float amps = valid ? reading.SignedCurrentAmps : 0f;
            float normalized = valid ? Mathf.Clamp01(Mathf.Abs(amps) / Mathf.Max(currentForFullEffect, 0.000001f)) : 0f;
            float direction = invertVisualDirection ? -1f : 1f;
            float speed = valid ? Mathf.Sign(amps) * direction * maximumFlowSpeed * normalized : 0f;
            ApplyMaterial(amps, enableWireFlow ? speed : 0f, enableWireFlow ? normalized * maximumGlow : 0f);

            bool changed = !hadReading || Mathf.Abs(amps - lastReading) >= popupMinimumChangeAmps;
            if (valid && enablePopup && popupPrefab != null && changed &&
                (hadReading || popupOnFirstReading) &&
                Time.unscaledTime - lastPopupTime >= popupCooldown)
            {
                Vector3 position = (popupAnchor != null ? popupAnchor.position : transform.position) + popupOffset;
                SparkCurrentPopup popup = Instantiate(popupPrefab, position, Quaternion.identity);
                popup.Play(amps, position, amps < 0f ? negativeColor : positiveColor);
                lastPopupTime = Time.unscaledTime;
            }
            hadReading = valid;
            lastReading = amps;
        }

        private void ApplyMaterial(float amps, float speed, float glow)
        {
            if (wireRenderer == null) return;
            if (block == null) block = new MaterialPropertyBlock();
            if (materialIndex >= 0) wireRenderer.GetPropertyBlock(block, materialIndex);
            else wireRenderer.GetPropertyBlock(block);
            block.SetFloat(CurrentId, amps);
            block.SetFloat(FlowSpeedId, speed);
            block.SetFloat(FlowGlowId, glow);
            if (materialIndex >= 0) wireRenderer.SetPropertyBlock(block, materialIndex);
            else wireRenderer.SetPropertyBlock(block);
        }
    }
}
