using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.UI.Components
{
    public sealed class PSProgressBar :
        MonoBehaviour
    {
        [SerializeField]
        private Slider slider;

        [SerializeField]
        private TMP_Text valueText;

        public float Progress =>
            slider != null
                ? slider.value
                : 0f;

        public void SetProgress(
            float value)
        {
            value =
                Mathf.Clamp01(value);

            if (slider != null)
            {
                slider.value = value;
            }

            if (valueText != null)
            {
                valueText.text =
                    $"{Mathf.RoundToInt(value * 100f)}%";
            }
        }
    }
}