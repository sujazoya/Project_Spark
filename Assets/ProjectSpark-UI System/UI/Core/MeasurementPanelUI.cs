using TMPro;
using UnityEngine;

namespace ProjectSpark.UI.Gameplay
{
    public sealed class MeasurementPanelUI :
        MonoBehaviour
    {
        [SerializeField]
        private GameObject root;

        [SerializeField]
        private TMP_Text measurementTypeText;

        [SerializeField]
        private TMP_Text valueText;

        [SerializeField]
        private TMP_Text unitText;

        public void Show(
            string measurementType,
            float value,
            string unit)
        {
            if (measurementTypeText != null)
            {
                measurementTypeText.text =
                    measurementType;
            }

            if (valueText != null)
            {
                valueText.text =
                    $"{value:0.###}";
            }

            if (unitText != null)
            {
                unitText.text =
                    unit;
            }

            if (root != null)
            {
                root.SetActive(true);
            }
        }

        public void Hide()
        {
            if (root != null)
            {
                root.SetActive(false);
            }
        }
    }
}