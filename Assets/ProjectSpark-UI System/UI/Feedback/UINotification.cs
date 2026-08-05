using TMPro;
using UnityEngine;

namespace ProjectSpark.UI.Feedback
{
    public sealed class UINotification :
        MonoBehaviour
    {
        [SerializeField]
        private TMP_Text titleText;

        [SerializeField]
        private TMP_Text messageText;

        public void Setup(
            UIFeedbackRequest request)
        {
            if (titleText != null)
            {
                titleText.text =
                    request.Title;
            }

            if (messageText != null)
            {
                messageText.text =
                    request.Message;
            }
        }
    }
}