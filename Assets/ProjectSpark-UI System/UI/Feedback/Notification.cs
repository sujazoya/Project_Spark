using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.UI.Feedback
{
    public sealed class Notification : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField]
        private TMP_Text titleText;

        [SerializeField]
        private TMP_Text messageText;

        [SerializeField]
        private Button closeButton;

        private void Awake()
        {
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(Close);
            }
        }

        public void Setup(
            string title,
            string message)
        {
            if (titleText != null)
            {
                titleText.text = title;
            }

            if (messageText != null)
            {
                messageText.text = message;
            }
        }

        public void Close()
        {
            Destroy(gameObject);
        }

        private void OnDestroy()
        {
            if (closeButton != null)
            {
                closeButton.onClick.RemoveListener(Close);
            }
        }
    }
}