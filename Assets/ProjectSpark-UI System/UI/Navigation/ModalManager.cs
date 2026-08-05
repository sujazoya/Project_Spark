using UnityEngine;

namespace ProjectSpark.UI.Navigation
{
    public sealed class ModalManager :
        MonoBehaviour
    {
        private ModalBase currentModal;

        public bool IsOpen =>
            currentModal != null;

        public void Open(
            ModalBase modal)
        {
            if (modal == null)
            {
                return;
            }

            CloseCurrent();

            currentModal = modal;

            currentModal.Open();
        }

        public void CloseCurrent()
        {
            if (currentModal == null)
            {
                return;
            }

            currentModal.Cancel();

            currentModal = null;
        }
    }
}