using UnityEngine;
using ProjectSpark.UI.Core;

namespace ProjectSpark.UI.Navigation
{
    public abstract class ScreenBase : MonoBehaviour
    {
        [SerializeField]
        private string screenId;

        public string ScreenId =>
            screenId;

        public UILifecycleState State
        {
            get;
            private set;
        } = UILifecycleState.Unregistered;

        public bool IsOpen =>
            State == UILifecycleState.Open;

        public virtual void Initialize()
        {
            State =
                UILifecycleState.Closed;

            gameObject.SetActive(false);
        }

        public virtual void Open()
        {
            if (State == UILifecycleState.Open ||
                State == UILifecycleState.Opening)
            {
                return;
            }

            State =
                UILifecycleState.Opening;

            gameObject.SetActive(true);

            OnOpened();

            State =
                UILifecycleState.Open;
        }

        public virtual void Close()
        {
            if (State == UILifecycleState.Closed ||
                State == UILifecycleState.Closing)
            {
                return;
            }

            State =
                UILifecycleState.Closing;

            OnClosed();

            gameObject.SetActive(false);

            State =
                UILifecycleState.Closed;
        }

        protected virtual void OnOpened()
        {
        }

        protected virtual void OnClosed()
        {
        }
    }
}