using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectSpark.UI.Feedback
{
    public sealed class ToastManager :
        MonoBehaviour
    {
        [SerializeField]
        private Toast toastPrefab;

        [SerializeField]
        private Transform toastContainer;

        [SerializeField]
        private float displayDuration = 3f;

        private readonly Queue<string> queue =
            new();

        private bool isShowing;

        public void Show(
            string message)
        {
            if (string.IsNullOrWhiteSpace(
                message))
            {
                return;
            }

            queue.Enqueue(message);

            ProcessQueue();
        }

        private void ProcessQueue()
        {
            if (isShowing)
            {
                return;
            }

            if (queue.Count == 0)
            {
                return;
            }

            string message =
                queue.Dequeue();

            StartCoroutine(
                ShowRoutine(message));
        }

        private IEnumerator ShowRoutine(
            string message)
        {
            isShowing = true;

            Toast toast =
                Instantiate(
                    toastPrefab,
                    toastContainer);

            toast.SetMessage(message);

            yield return new WaitForSeconds(
                displayDuration);

            if (toast != null)
            {
                Destroy(toast.gameObject);
            }

            isShowing = false;

            ProcessQueue();
        }
    }
}