using UnityEngine;
using ProjectSpark.UI.Adapters;
using ProjectSpark.UI.Data;
using ProjectSpark.UI.Components;

namespace ProjectSpark.UI.Views
{
    public sealed class ObjectiveHUD :
        MonoBehaviour
    {
        [SerializeField]
        private ObjectiveUIAdapter adapter;

        [SerializeField]
        private PSProgressBar progressBar;

        [SerializeField]
        private TMPro.TMP_Text titleText;

        [SerializeField]
        private TMPro.TMP_Text
            descriptionText;

        [SerializeField]
        private GameObject
            completedIndicator;

        private void OnEnable()
        {
            if (adapter != null)
            {
                adapter.ViewModelChanged +=
                    Refresh;
            }
        }

        private void OnDisable()
        {
            if (adapter != null)
            {
                adapter.ViewModelChanged -=
                    Refresh;
            }
        }

        private void Refresh(
            ObjectiveViewModel viewModel)
        {
            if (viewModel == null)
            {
                return;
            }

            if (titleText != null)
            {
                titleText.text =
                    viewModel.Title;
            }

            if (descriptionText != null)
            {
                descriptionText.text =
                    viewModel.Description;
            }

            if (progressBar != null)
            {
                progressBar.SetProgress(
                    viewModel.Progress);
            }

            if (completedIndicator != null)
            {
                completedIndicator.SetActive(
                    viewModel.IsCompleted);
            }
        }
    }
}