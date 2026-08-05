using System;
using UnityEngine;
using ProjectSpark.UI.Data;

namespace ProjectSpark.UI.Adapters
{
    public sealed class ObjectiveUIAdapter :
        MonoBehaviour
    {
        public event Action<
            ObjectiveViewModel>
            ViewModelChanged;

        public ObjectiveViewModel Current
        {
            get;
            private set;
        }

        public void UpdateObjective(
            string id,
            string title,
            string description,
            float progress,
            bool completed,
            bool failed)
        {
            Current =
                new ObjectiveViewModel(
                    id,
                    title,
                    description,
                    progress,
                    completed,
                    failed);

            ViewModelChanged?.Invoke(
                Current);
        }
    }
}