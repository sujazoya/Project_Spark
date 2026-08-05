using TMPro;
using UnityEngine;

namespace ProjectSpark.UI.Gameplay
{
    public sealed class ComponentInspectorUI :
        MonoBehaviour
    {
        [Header("Identity")]

        [SerializeField]
        private TMP_Text componentIdText;

        [SerializeField]
        private TMP_Text componentTypeText;

        [Header("Specifications")]

        [SerializeField]
        private TMP_Text valueText;

        [SerializeField]
        private TMP_Text ratingText;

        [Header("Electrical")]

        [SerializeField]
        private TMP_Text voltageText;

        [SerializeField]
        private TMP_Text currentText;

        [SerializeField]
        private TMP_Text powerText;

        [SerializeField]
        private TMP_Text temperatureText;

        [Header("Status")]

        [SerializeField]
        private TMP_Text statusText;

        [SerializeField]
        private TMP_Text faultText;

        public void Show(
            UIComponentInspectionData data)
        {
            SetText(
                componentIdText,
                data.ComponentId);

            SetText(
                componentTypeText,
                data.ComponentType);

            SetText(
                valueText,
                data.Value);

            SetText(
                ratingText,
                data.Rating);

            SetText(
                voltageText,
                $"{data.Voltage:0.###} V");

            SetText(
                currentText,
                $"{data.Current:0.###} A");

            SetText(
                powerText,
                $"{data.Power:0.###} W");

            SetText(
                temperatureText,
                $"{data.Temperature:0.###} °C");

            SetText(
                statusText,
                data.Status);

            SetText(
                faultText,
                string.IsNullOrWhiteSpace(
                    data.Fault)
                    ? "NONE"
                    : data.Fault);
        }

        public void Clear()
        {
            SetText(
                componentIdText,
                "--");

            SetText(
                componentTypeText,
                "--");

            SetText(
                valueText,
                "--");

            SetText(
                ratingText,
                "--");

            SetText(
                voltageText,
                "--");

            SetText(
                currentText,
                "--");

            SetText(
                powerText,
                "--");

            SetText(
                temperatureText,
                "--");

            SetText(
                statusText,
                "--");

            SetText(
                faultText,
                "NONE");
        }

        private void SetText(
            TMP_Text target,
            string value)
        {
            if (target != null)
            {
                target.text =
                    value;
            }
        }
    }
}