using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.UI.Components
{
    public sealed class PS_ProgressBar : MonoBehaviour
    {
        [SerializeField]
        private Image fill;

        [SerializeField]
        [Range(0f, 1f)]
        private float progress;

        public float Progress
        {
            get => progress;
            private set => progress = Mathf.Clamp01(value);
        }

        public void SetProgress(float value)
        {
            Progress = value;

            if (fill != null)
            {
                fill.fillAmount = Progress;
            }
        }
    }
}