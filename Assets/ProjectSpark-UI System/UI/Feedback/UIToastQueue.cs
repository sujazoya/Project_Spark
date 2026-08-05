using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectSpark.UI.Feedback
{
    public sealed class UIToastQueue :
        MonoBehaviour
    {
        [SerializeField]
        private UIToast toastPrefab;

        [SerializeField]
        private Transform toastContainer;

        [SerializeField]
        private int maximumVisibleToasts = 3;

        private readonly Queue<UIFeedbackRequest>
            pending =
                new Queue<UIFeedbackRequest>();

        private readonly List<UIToast>
            visible =
                new List<UIToast>();

        private bool processing;

        public void Enqueue(
            UIFeedbackRequest request)
        {
            pending.Enqueue(request);

            if (!processing)
            {
                StartCoroutine(
                    ProcessQueue());
            }
        }

        private IEnumerator ProcessQueue()
        {
            processing = true;

            while (pending.Count > 0)
            {
                while (visible.Count >=
                       maximumVisibleToasts)
                {
                    yield return null;
                }

                UIFeedbackRequest request =
                    pending.Dequeue();

                yield return ShowToast(
                    request);
            }

            processing = false;
        }

        private IEnumerator ShowToast(
            UIFeedbackRequest request)
        {
            if (toastPrefab == null ||
                toastContainer == null)
            {
                yield break;
            }

            UIToast toast =
                Instantiate(
                    toastPrefab,
                    toastContainer);

            toast.Setup(request);

            visible.Add(toast);

            yield return FadeIn(toast);

            yield return new WaitForSecondsRealtime(
                Mathf.Max(
                    0.1f,
                    request.Duration));

            yield return FadeOut(toast);

            visible.Remove(toast);

            Destroy(
                toast.gameObject);
        }

        private IEnumerator FadeIn(
            UIToast toast)
        {
            float time = 0f;

            while (time < 0.2f)
            {
                time +=
                    Time.unscaledDeltaTime;

                float progress =
                    Mathf.Clamp01(
                        time / 0.2f);

                toast.SetAlpha(
                    progress);

                yield return null;
            }

            toast.SetAlpha(1f);
        }

        private IEnumerator FadeOut(
            UIToast toast)
        {
            float time = 0f;

            while (time < 0.2f)
            {
                time +=
                    Time.unscaledDeltaTime;

                float progress =
                    Mathf.Clamp01(
                        time / 0.2f);

                toast.SetAlpha(
                    1f - progress);

                yield return null;
            }

            toast.SetAlpha(0f);
        }
    }
}