using TMPro;
using ProjectSpark.UI.Feedback;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.UI.Gameplay
{
    public sealed class ObjectiveTrackerUI :
        MonoBehaviour
    {
        [SerializeField]
        private TMP_Text objectiveTitleText;

        [SerializeField]
        private TMP_Text objectiveDescriptionText;

        [SerializeField]
        private TMP_Text statusText;

        [SerializeField]
        private TMP_Text progressText;

        [SerializeField]
        private Slider progressSlider;

        private void OnEnable()
        {
            if (ProjectSpark.UI.Core.UIManager.Instance == null)
            {
                return;
            }

            ProjectSpark.UI.Core.UIManager.Instance
                .State
                .ObjectiveChanged +=
                    HandleObjectiveChanged;
        }

        private void OnDisable()
        {
            if (ProjectSpark.UI.Core.UIManager.Instance == null)
            {
                return;
            }

            ProjectSpark.UI.Core.UIManager.Instance
                .State
                .ObjectiveChanged -=
                    HandleObjectiveChanged;
        }

        private void HandleObjectiveChanged(
            UIObjectiveState state)
        {
            if (objectiveTitleText != null)
            {
                objectiveTitleText.text =
                    state.Title;
            }

            if (objectiveDescriptionText != null)
            {
                objectiveDescriptionText.text =
                    state.Description;
            }

            if (statusText != null)
            {
                statusText.text =
                    state.Status.ToString()
                        .ToUpperInvariant();
            }

            float progress =
                Mathf.Clamp01(
                    state.Progress);

            if (progressSlider != null)
            {
                progressSlider.value =
                    progress;
            }

            if (progressText != null)
            {
                progressText.text =
                    $"{progress * 100f:0}%";
            }
        }
    }
}