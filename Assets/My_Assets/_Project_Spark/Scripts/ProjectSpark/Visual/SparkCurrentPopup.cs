using TMPro;
using UnityEngine;

namespace ProjectSpark.Electrical
{
    /// <summary>World-space floating current label. Use 3D TMP or a World Space Canvas.</summary>
    public sealed class SparkCurrentPopup : MonoBehaviour
    {
        [SerializeField] private TMP_Text label;
        [SerializeField, Min(0.05f)] private float duration = 0.85f;
        [SerializeField] private float riseDistance = 0.2f;
        [SerializeField] private bool faceCamera = true;
        [SerializeField] private bool unscaledTime = true;

        private float age;
        private Vector3 origin;
        private Color baseColor;
        private Vector3 initialScale;

        private void Awake()
        {
            if (label == null) label = GetComponentInChildren<TMP_Text>(true);
            initialScale = transform.localScale;
        }

        public void Play(float signedAmps, Vector3 worldPosition, Color color)
        {
            if (label == null) label = GetComponentInChildren<TMP_Text>(true);
            if (label == null) { Debug.LogWarning("Current popup needs TMP_Text", this); return; }
            origin = worldPosition;
            transform.position = origin;
            transform.localScale = initialScale;
            age = 0f;
            baseColor = color;
            label.text = Mathf.Abs(signedAmps) < 1f
                ? $"{signedAmps * 1000f:+0.##;-0.##;0} mA"
                : $"{signedAmps:+0.###;-0.###;0} A";
            label.color = baseColor;
        }

        private void LateUpdate()
        {
            if (label == null) return;
            age += unscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            float t = Mathf.Clamp01(age / duration);
            transform.position = origin + Vector3.up * (riseDistance * t);
            transform.localScale = initialScale * (1f + 0.18f * Mathf.Sin(t * Mathf.PI));
            Color c = baseColor;
            c.a *= 1f - Mathf.SmoothStep(0f, 1f, t);
            label.color = c;
            if (faceCamera && Camera.main != null)
                transform.rotation = Camera.main.transform.rotation;
            if (age >= duration) Destroy(gameObject);
        }
    }
}
