using TMPro;
using ProjectSpark.UI.Core;
using UnityEngine;

namespace ProjectSpark.UI.Screens
{
    public sealed class LevelBriefingScreen :
        UIScreen
    {
        [Header("Mission Information")]

        [SerializeField]
        private TMP_Text missionNumberText;

        [SerializeField]
        private TMP_Text titleText;

        [SerializeField]
        private TMP_Text descriptionText;

        [SerializeField]
        private TMP_Text difficultyText;

        [Header("Preview")]

        [SerializeField]
        private UnityEngine.UI.Image previewImage;

        private LevelSelectData selectedLevel;

        public void SetLevel(
            LevelSelectData level)
        {
            selectedLevel = level;

            if (level == null)
            {
                return;
            }

            if (missionNumberText != null)
            {
                missionNumberText.text =
                    level.MissionNumber;
            }

            if (titleText != null)
            {
                titleText.text =
                    level.Title;
            }

            if (descriptionText != null)
            {
                descriptionText.text =
                    level.Description;
            }

            if (difficultyText != null)
            {
                difficultyText.text =
                    GetDifficultyText(
                        level.Difficulty);
            }

            if (previewImage != null)
            {
                previewImage.sprite =
                    level.PreviewImage;
            }
        }

        public void StartLevel()
        {
            if (selectedLevel == null)
            {
                return;
            }

            // The actual level loading integration
            // will connect to LevelManager in Phase 9.
            Debug.Log(
                $"Start level requested: " +
                $"{selectedLevel.LevelId}");
        }

        public void Back()
        {
            UIManager.Instance.ShowScreen(
                UIScreenIds.LevelSelect);
        }

        private string GetDifficultyText(
            int difficulty)
        {
            if (difficulty <= 0)
            {
                return "UNRATED";
            }

            return new string(
                '★',
                Mathf.Clamp(
                    difficulty,
                    1,
                    5));
        }
    }
}