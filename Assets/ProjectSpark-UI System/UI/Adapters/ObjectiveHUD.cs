using UnityEngine;
using ProjectSpark.UI.Components;
using ProjectSpark.UI.Data;

namespace ProjectSpark.UI.Gameplay
{
    public sealed class ObjectiveHUD :
        MonoBehaviour
    {
        [SerializeField]
        private PS_ProgressBar progressBar;

        [SerializeField]
        private PS_StatusBadge statusBadge;

        public void Display(
            ObjectiveViewModel viewModel)
        {
            if (viewModel == null)
            {
                return;
            }

            if (progressBar != null)
            {
                progressBar.SetProgress(
                    viewModel.Progress);
            }

            if (statusBadge != null)
            {
                statusBadge.SetStatus(
                    viewModel.IsCompleted
                        ? UIStatusType.Success
                        : viewModel.IsFailed
                            ? UIStatusType.Error
                            : UIStatusType.Active);
            }
        }
    }
}