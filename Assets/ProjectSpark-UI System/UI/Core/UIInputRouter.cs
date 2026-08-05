using System;
using UnityEngine;

namespace ProjectSpark.UI.Input
{
    public sealed class UIInputRouter :
        MonoBehaviour
    {
        public static UIInputRouter Instance
        {
            get;
            private set;
        }

        private readonly
            UIInputContextStack contextStack =
            new UIInputContextStack();

        public UIInputContext CurrentContext =>
            contextStack.Current;

        public event Action<
            UIInputContext>
            ContextChanged;

        private void Awake()
        {
            if (Instance != null &&
                Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            contextStack.Clear();

            contextStack.Push(
                UIInputContext.Gameplay);
        }

        public void PushContext(
            UIInputContext context)
        {
            contextStack.Push(
                context);

            NotifyChanged();
        }

        public void PopContext()
        {
            contextStack.Pop();

            NotifyChanged();
        }

        public void ResetToGameplay()
        {
            contextStack.Clear();

            contextStack.Push(
                UIInputContext.Gameplay);

            NotifyChanged();
        }

        private void NotifyChanged()
        {
            ContextChanged?.Invoke(
                CurrentContext);
        }

        public bool IsGameplayInputAllowed()
        {
            return CurrentContext ==
                UIInputContext.Gameplay;
        }

        public bool IsUIInputAllowed()
        {
            return CurrentContext ==
                       UIInputContext.UI ||
                   CurrentContext ==
                       UIInputContext.Pause ||
                   CurrentContext ==
                       UIInputContext.Modal;
        }

        public bool IsModalInputAllowed()
        {
            return CurrentContext ==
                UIInputContext.Modal;
        }
    }
}