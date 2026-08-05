using System.Collections.Generic;
using UnityEngine;

namespace ProjectSpark.UI.Feedback
{
    public sealed class NotificationManager :
        MonoBehaviour
    {
        [SerializeField]
        private Notification
            notificationPrefab;

        [SerializeField]
        private Transform
            notificationContainer;

        private readonly List<
            Notification> active =
            new();

        public void Show(
            string title,
            string message)
        {
            if (notificationPrefab == null ||
                notificationContainer == null)
            {
                return;
            }

            Notification notification =
                Instantiate(
                    notificationPrefab,
                    notificationContainer);

            notification.Setup(
                title,
                message);

            active.Add(
                notification);
        }

        public void ClearAll()
        {
            for (int i = active.Count - 1;
                 i >= 0;
                 i--)
            {
                if (active[i] != null)
                {
                    Destroy(
                        active[i].gameObject);
                }
            }

            active.Clear();
        }
    }
}