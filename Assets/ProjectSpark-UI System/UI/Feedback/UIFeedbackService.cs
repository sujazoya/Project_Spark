using ProjectSpark.UI.Core;
using UnityEngine;

namespace ProjectSpark.UI.Feedback
{
    public sealed class UIFeedbackService :
        MonoBehaviour
    {
        [SerializeField]
        private UIToastQueue toastQueue;

        [SerializeField]
        private UINotificationQueue
            notificationQueue;

        public void ShowToast(
            UIFeedbackRequest request)
        {
            if (toastQueue == null)
            {
                return;
            }

            toastQueue.Enqueue(
                request);
        }

        public void ShowNotification(
            UIFeedbackRequest request)
        {
            if (notificationQueue == null)
            {
                return;
            }

            notificationQueue.Enqueue(
                request);
        }

        public void Info(
            string title,
            string message)
        {
            ShowToast(
                new UIFeedbackRequest(
                    UIFeedbackType.Info,
                    UIFeedbackPriority.Normal,
                    title,
                    message));
        }

        public void Success(
            string title,
            string message)
        {
            ShowToast(
                new UIFeedbackRequest(
                    UIFeedbackType.Success,
                    UIFeedbackPriority.Normal,
                    title,
                    message));
        }

        public void Warning(
            string title,
            string message)
        {
            ShowToast(
                new UIFeedbackRequest(
                    UIFeedbackType.Warning,
                    UIFeedbackPriority.High,
                    title,
                    message));
        }

        public void Error(
            string title,
            string message)
        {
            ShowNotification(
                new UIFeedbackRequest(
                    UIFeedbackType.Error,
                    UIFeedbackPriority.High,
                    title,
                    message));
        }

        public void Objective(
            string title,
            string message)
        {
            ShowNotification(
                new UIFeedbackRequest(
                    UIFeedbackType.Objective,
                    UIFeedbackPriority.High,
                    title,
                    message));
        }

        public void System(
            string title,
            string message)
        {
            ShowToast(
                new UIFeedbackRequest(
                    UIFeedbackType.System,
                    UIFeedbackPriority.Normal,
                    title,
                    message));
        }
    }
}