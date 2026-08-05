using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ProjectSpark.UI.Components
{
    public sealed class PSButton :
        MonoBehaviour
    {
        [SerializeField]
        private Button button;

        [SerializeField]
        private TMP_Text label;

        [SerializeField]
        private Image icon;

        public void SetText(
            string text)
        {
            if (label != null)
            {
                label.text = text;
            }
        }

        public void SetIcon(
            Sprite sprite)
        {
            if (icon == null)
            {
                return;
            }

            icon.sprite = sprite;

            icon.gameObject.SetActive(
                sprite != null);
        }

        public void SetInteractable(
            bool interactable)
        {
            if (button != null)
            {
                button.interactable =
                    interactable;
            }
        }

        public void AddListener(
            UnityAction action)
        {
            if (button == null)
            {
                return;
            }

            button.onClick.AddListener(
                action);
        }

        public void RemoveAllListeners()
        {
            if (button == null)
            {
                return;
            }

            button.onClick
                .RemoveAllListeners();
        }
    }
}