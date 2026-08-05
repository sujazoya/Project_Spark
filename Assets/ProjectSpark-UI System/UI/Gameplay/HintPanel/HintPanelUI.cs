using TMPro;
using UnityEngine;

namespace ProjectSpark.UI.Gameplay
{
    public sealed class HintPanelUI :
        MonoBehaviour
    {
        [SerializeField]
        private GameObject panelRoot;

        [SerializeField]
        private TMP_Text hintText;

        public bool IsVisible =>
            panelRoot != null &&
            panelRoot.activeSelf;

        public void ShowHint(
            string message)
        {
            if (hintText != null)
            {
                hintText.text =
                    message;
            }

            if (panelRoot != null)
            {
                panelRoot.SetActive(true);
            }
        }

        public void HideHint()
        {
            if (panelRoot != null)
            {
                panelRoot.SetActive(false);
            }
        }
    }
}