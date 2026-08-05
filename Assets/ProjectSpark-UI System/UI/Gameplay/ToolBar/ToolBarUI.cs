using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.UI.Gameplay
{
    public sealed class ToolSlotUI :
        MonoBehaviour
    {
        [SerializeField]
        private string toolId;

        [SerializeField]
        private Image icon;

        [SerializeField]
        private GameObject selectedState;

        [SerializeField]
        private GameObject lockedState;

        [SerializeField]
        private GameObject disabledState;

        [SerializeField]
        private TMP_Text toolNameText;

        public string ToolId =>
            toolId;

        public void SetToolName(
            string displayName)
        {
            if (toolNameText != null)
            {
                toolNameText.text =
                    displayName;
            }
        }

        public void SetSelected(
            bool selected)
        {
            if (selectedState != null)
            {
                selectedState.SetActive(
                    selected);
            }
        }

        public void SetLocked(
            bool locked)
        {
            if (lockedState != null)
            {
                lockedState.SetActive(
                    locked);
            }
        }

        public void SetAvailable(
            bool available)
        {
            if (disabledState != null)
            {
                disabledState.SetActive(
                    !available);
            }
        }
    }
}