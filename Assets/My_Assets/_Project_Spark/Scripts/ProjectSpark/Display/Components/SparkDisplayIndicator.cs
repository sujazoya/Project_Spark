using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.Display
{
    public sealed class SparkDisplayIndicator : MonoBehaviour
    {
        [SerializeField] private Graphic target;
        [SerializeField] private float pulseSpeed = 5f;
        [SerializeField] private float pulseAmount = 0.15f;

        private bool active;

        public void SetActive(bool value)
        {
            active = value;
            if (!active && target != null)
                target.canvasRenderer.SetAlpha(1f);
        }

        private void Update()
        {
            if (!active || target == null)
                return;

            float pulse = 1f - Mathf.Abs(Mathf.Sin(Time.unscaledTime * pulseSpeed)) * pulseAmount;
            target.canvasRenderer.SetAlpha(pulse);
        }
    }
}
