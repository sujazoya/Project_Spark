using TMPro;
using ProjectSpark.UI.Core;
using UnityEngine;

namespace ProjectSpark.UI.Screens
{
    public sealed class ResultsScreen :
        UIScreen
    {
        [Header("Status")]

        [SerializeField]
        private TMP_Text resultText;

        [SerializeField]
        private TMP_Text levelTitleText;

        [Header("Statistics")]

        [SerializeField]
        private TMP_Text completionText;

        [SerializeField]
        private TMP_Text timeText;

        [SerializeField]
        private TMP_Text accuracyText;

        [SerializeField]
        private TMP_Text mistakesText;

        [SerializeField]
        private TMP_Text efficiencyText;

        private UIResultsData results;

        private string retryLevelId;

        public void SetResults(
            UIResultsData data)
        {
            results = data;

            retryLevelId =
                data.LevelId;

            if (resultText != null)
            {
                resultText.text =
                    data.Completed
                        ? "MISSION COMPLETE"
                        : "MISSION FAILED";
            }

            if (levelTitleText != null)
            {
                levelTitleText.text =
                    data.LevelTitle;
            }

            if (completionText != null)
            {
                completionText.text =
                    $"{data.CompletionPercentage:0}%";
            }

            if (timeText != null)
            {
                timeText.text =
                    FormatTime(data.Time);
            }

            if (accuracyText != null)
            {
                accuracyText.text =
                    $"{data.Accuracy:0}%";
            }

            if (mistakesText != null)
            {
                mistakesText.text =
                    data.Mistakes.ToString();
            }

            if (efficiencyText != null)
            {
                efficiencyText.text =
                    $"{data.Efficiency:0}%";
            }
        }

        public void Retry()
        {
            if (string.IsNullOrWhiteSpace(
                    retryLevelId))
            {
                return;
            }

            // Actual level restart integration
            // will be connected to LevelManager.
            Debug.Log(
                $"Retry requested: " +
                $"{retryLevelId}");
        }

        public void Continue()
        {
            UIManager.Instance.ShowScreen(
                UIScreenIds.LevelSelect);
        }

        public void OpenLevelSelect()
        {
            UIManager.Instance.ShowScreen(
                UIScreenIds.LevelSelect);
        }

        private string FormatTime(
            float seconds)
        {
            int minutes =
                Mathf.FloorToInt(
                    seconds / 60f);

            int remainingSeconds =
                Mathf.FloorToInt(
                    seconds % 60f);

            return
                $"{minutes:00}:{remainingSeconds:00}";
        }
    }
}