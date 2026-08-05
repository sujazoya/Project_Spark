using TMPro;
using UnityEngine;

namespace ProjectSpark.UI.Feedback
{
    public sealed class Notification :
        MonoBehaviour
    {
        [SerializeField]
        private TMP_Text titleText;

        [SerializeField]
        private TMP_Text messageText;

        public void Setup(
            string title,
            string message)
        {
            if (titleText != null)
            {
                titleText.text =
                    title;
            }

            if (messageText != null)
            {
                messageText.text =
                    message;
            }
        }
    }
}