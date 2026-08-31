using TMPro;
using UnityEngine;

namespace ProjectSpark.HolographicViewer
{
    public sealed class HolographicComponentHUD : MonoBehaviour
    {
        [Header("Identity")]
        [SerializeField] private TMP_Text componentName;
        [SerializeField] private TMP_Text partNumber;
        [SerializeField] private TMP_Text componentType;

        [Header("Description")]
        [SerializeField] private TMP_Text description;

        [Header("Specifications")]
        [SerializeField] private TMP_Text specification01;
        [SerializeField] private TMP_Text specification02;
        [SerializeField] private TMP_Text specification03;

        private void Start()
        {
            Clear();
        }

        public void Show(
            HolographicComponentData data)
        {
            if (data == null)
            {
                Clear();
                return;
            }

            componentName.text =
                data.ComponentName;

            partNumber.text =
                "PART " + data.PartNumber;

            componentType.text =
                data.ComponentType;

            description.text =
                data.Description;

            specification01.text =
                data.Specification01;

            specification02.text =
                data.Specification02;

            specification03.text =
                data.Specification03;
        }

        public void Clear()
        {
            componentName.text =
                "NO COMPONENT SELECTED";

            partNumber.text =
                "PART ---";

            componentType.text =
                "---";

            description.text =
                "Select a component for inspection.";

            specification01.text =
                "---";

            specification02.text =
                "---";

            specification03.text =
                "---";
        }
    }
}