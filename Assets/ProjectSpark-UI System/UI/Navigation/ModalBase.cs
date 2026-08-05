using System;
using UnityEngine;

namespace ProjectSpark.UI.Navigation
{
    public abstract class ModalBase :
        MonoBehaviour
    {
        public bool IsOpen
        {
            get;
            private set;
        }

        private Action<
            ModalResult> resultCallback;

        public void Open(
            Action<ModalResult>
                callback = null)
        {
            resultCallback =
                callback;

            IsOpen = true;

            gameObject.SetActive(true);
        }

        protected void Resolve(
            ModalResult result)
        {
            if (!IsOpen)
            {
                return;
            }

            IsOpen = false;

            gameObject.SetActive(false);

            Action<ModalResult>
                callback =
                    resultCallback;

            resultCallback = null;

            callback?.Invoke(result);
        }

        public void Confirm()
        {
            Resolve(
                ModalResult.Confirmed);
        }

        public void Cancel()
        {
            Resolve(
                ModalResult.Cancelled);
        }
    }
}