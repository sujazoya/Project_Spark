using System;
using UnityEngine;

namespace ProjectSpark.UI.Core
{
    /// <summary>
    /// Base class for Project Spark full-screen UI pages.
    /// UIManager controls the public lifecycle through Open and Close.
    /// Derived screens customize behavior through OnOpen and OnClose.
    /// </summary>
    public abstract class UIScreen : MonoBehaviour
    {
        [Header("Screen Identity")]
        [SerializeField]
        private string screenId;

        [SerializeField]
        private UIContext context = UIContext.None;

        [Header("Behaviour")]
        [SerializeField]
        private bool deactivateWhenHidden = true;

        public string ScreenId => screenId;

        public UIContext Context => context;

        public bool IsVisible { get; private set; }

        public event Action<UIScreen> Opened;
        public event Action<UIScreen> Closed;

        protected virtual void Awake()
        {
            IsVisible = false;

            if (deactivateWhenHidden && gameObject.activeSelf)
            {
                gameObject.SetActive(false);
            }
        }

        public void Open()
        {
            if (IsVisible)
            {
                return;
            }

            IsVisible = true;

            if (!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
            }

            OnOpen();
            Opened?.Invoke(this);
        }

        public void Close()
        {
            if (!IsVisible)
            {
                return;
            }

            IsVisible = false;

            OnClose();

            if (deactivateWhenHidden)
            {
                gameObject.SetActive(false);
            }

            Closed?.Invoke(this);
        }

        public virtual void Refresh()
        {
        }

        protected virtual void OnOpen()
        {
        }

        protected virtual void OnClose()
        {
        }
    }
}
