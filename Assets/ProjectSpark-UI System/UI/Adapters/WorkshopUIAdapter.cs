using System;
using UnityEngine;
using ProjectSpark.UI.Data;

namespace ProjectSpark.UI.Adapters
{
    public sealed class WorkshopUIAdapter :
        MonoBehaviour
    {
        public event Action<
            WorkshopViewModel>
            ViewModelChanged;

        public WorkshopViewModel Current
        {
            get;
            private set;
        }

        public void UpdateWorkshop(
            string toolName,
            string componentName,
            bool toolActive,
            string statusText)
        {
            Current =
                new WorkshopViewModel(
                    toolName,
                    componentName,
                    toolActive,
                    statusText);

            ViewModelChanged?.Invoke(
                Current);
        }
    }
}