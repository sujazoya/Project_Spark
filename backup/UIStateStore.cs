using System;
using UnityEngine;

namespace ProjectSpark.UI.Core
{
    public sealed class UIStateStore : MonoBehaviour
    {
        public string CurrentScreenId
        {
            get;
            private set;
        }

        public bool IsGameplayActive
        {
            get;
            private set;
        }

        public bool IsPaused
        {
            get;
            private set;
        }

        public bool IsMonitorOpen
        {
            get;
            private set;
        }

        public bool IsInputLocked
        {
            get;
            private set;
        }

        public event Action StateChanged;

        public void SetCurrentScreen(
            string screenId)
        {
            CurrentScreenId = screenId;

            NotifyChanged();
        }

        public void SetGameplayActive(
            bool active)
        {
            IsGameplayActive = active;

            NotifyChanged();
        }

        public void SetPaused(
            bool paused)
        {
            IsPaused = paused;

            NotifyChanged();
        }

        public void SetMonitorOpen(
            bool open)
        {
            IsMonitorOpen = open;

            NotifyChanged();
        }

        public void SetInputLocked(
            bool locked)
        {
            IsInputLocked = locked;

            NotifyChanged();
        }

        private void NotifyChanged()
        {
            StateChanged?.Invoke();
        }
    }
}