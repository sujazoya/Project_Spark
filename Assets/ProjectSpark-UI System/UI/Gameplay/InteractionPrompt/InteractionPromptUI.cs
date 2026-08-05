using TMPro;
using UnityEngine;

namespace ProjectSpark.UI.Gameplay
{
    public sealed class InteractionPromptUI :
        MonoBehaviour
    {
        [SerializeField]
        private GameObject root;

        [SerializeField]
        private TMP_Text actionText;

        public void Show(
            string action)
        {
            if (actionText != null)
            {
                actionText.text =
                    action;
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