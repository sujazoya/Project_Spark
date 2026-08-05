using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectSpark.UI.Feedback
{
    public sealed class UINotificationQueue :
        MonoBehaviour
    {
        [SerializeField]
        private UINotification notificationPrefab;

        [SerializeField]
        private Transform notificationContainer;

        private readonly List<
            UIFeedbackRequest>
            pending =
            new List<UIFeedbackRequest>();

        private bool processing;

        public void Enqueue(
            UIFeedbackRequest request)
        {
            pending.Add(request);

            SortByPriority();

            if (!processing)
            {
                StartCoroutine(
                    ProcessQueue());
            }
        }

        private void SortByPriority()
        {
            pending.Sort(
                ComparePriority);
        }

        private int ComparePriority(
            UIFeedbackRequest a,
            UIFeedbackRequest b)
        {
            return b.Priority.CompareTo(
                a.Priority);
        }

        private IEnumerator ProcessQueue()
        {
            processing = true;

            while (pending.Count > 0)
            {
                UIFeedbackRequest request =
                    pending[0];

                pending.RemoveAt(0);

                yield return ShowNotification(
                    request);
            }

            processing = false;
        }

        private IEnumerator ShowNotification(
            UIFeedbackRequest request)
        {
            if (notificationPrefab == null ||
                notificationContainer == null)
            {
                yield break;
            }

            UINotification notification =
                Instantiate(
                    notificationPrefab,
                    notificationContainer);

            notification.Setup(request);

            yield return new WaitForSecondsRealtime(
                Mathf.Max(
                    0.1f,
                    request.Duration));

            Destroy(
                notification.gameObject);
        }
    }
}