using System.Collections.Generic;
using UnityEngine;

namespace ProjectSpark.UI.Feedback
{
    public sealed class UIModalManager :
        MonoBehaviour
    {
        [SerializeField]
        private GameObject modalRoot;

        [SerializeField]
        private TMPro.TMP_Text titleText;

        [SerializeField]
        private TMPro.TMP_Text messageText;

        [SerializeField]
        private TMPro.TMP_Text primaryButtonText;

        [SerializeField]
        private TMPro.TMP_Text
            secondaryButtonText;

        [SerializeField]
        private GameObject secondaryButton;

        private readonly Queue<
            UIModalRequest>
            queue =
            new Queue<UIModalRequest>();

        private UIModalRequest current;

        public bool IsOpen =>
            current != null;

        public void Open(
            UIModalRequest request)
        {
            if (request == null)
            {
                return;
            }

            queue.Enqueue(request);

            TryShowNext();
        }

        public void Primary()
        {
            if (current == null)
            {
                return;
            }

            current.PrimaryAction?.Invoke();

            Close();
        }

        public void Secondary()
        {
            if (current == null)
            {
                return;
            }

            current.SecondaryAction?.Invoke();

            Close();
        }

        public void Close()
        {
            if (current == null)
            {
                return;
            }

            current.Closed?.Invoke();

            current = null;

            if (modalRoot != null)
            {
                modalRoot.SetActive(false);
            }

            TryShowNext();
        }

        private void TryShowNext()
        {
            if (current != null)
            {
                return;
            }

            if (queue.Count == 0)
            {
                return;
            }

            current =
                queue.Dequeue();

            ShowCurrent();
        }

        private void ShowCurrent()
        {
            if (modalRoot != null)
            {
                modalRoot.SetActive(true);
            }

            if (titleText != null)
            {
                titleText.text =
                    current.Title;
            }

            if (messageText != null)
            {
                messageText.text =
                    current.Message;
            }

            if (primaryButtonText != null)
            {
                primaryButtonText.text =
                    current.PrimaryText;
            }

            bool showSecondary =
                current.ShowSecondaryButton;

            if (secondaryButton != null)
            {
                secondaryButton.SetActive(
                    showSecondary);
            }

            if (showSecondary &&
                secondaryButtonText != null)
            {
                secondaryButtonText.text =
                    current.SecondaryText;
            }
        }
    }
}