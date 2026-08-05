using TMPro;
using UnityEngine;

namespace ProjectSpark.UI.Feedback
{
    public sealed class Toast :
        MonoBehaviour
    {
        [SerializeField]
        private TMP_Text messageText;

        public void SetMessage(
            string message)
        {
            if (messageText != null)
            {
                messageText.text =
                    message;
            }
        }
    }
}