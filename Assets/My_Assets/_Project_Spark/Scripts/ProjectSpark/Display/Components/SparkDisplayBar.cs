
using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.Display
{
    [DisallowMultipleComponent]
    public sealed class SparkDisplayBar : MonoBehaviour
    {
        [SerializeField]
        private Image fill;

        [SerializeField, Min(0.01f)]
        private float speed = 8f;

        private float target;

        public void SetTarget(
            float normalized)
        {
            target =
                Mathf.Clamp01(
                    normalized);
        }

        private void Awake()
        {
            speed =
                Mathf.Max(
                    0.01f,
                    speed);
        }

        private void Update()
        {
            if (fill == null)
                return;

            fill.fillAmount =
                Mathf.MoveTowards(
                    fill.fillAmount,
                    target,
                    speed *
                    Time.unscaledDeltaTime);
        }
    }
}
