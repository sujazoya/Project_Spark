using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.UI.Feedback
{
    public sealed class UIToast :
        MonoBehaviour
    {
        [SerializeField]
        private TMP_Text titleText;

        [SerializeField]
        private TMP_Text messageText;

        [SerializeField]
        private Image typeIcon;

        [SerializeField]
        private CanvasGroup canvasGroup;

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

            ApplyType(
                request.Type);
        }

        public void SetAlpha(
            float alpha)
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha =
                    alpha;
            }
        }

        private void ApplyType(
            UIFeedbackType type)
        {
            // Visual type styling should be
            // handled by the UI prefab/theme.
            //
            // Do not hard-code colors here.
            //
            // Future theme system can map:
            //
            // Info
            // Success
            // Warning
            // Error
            // Objective
            // System
        }
    }
}