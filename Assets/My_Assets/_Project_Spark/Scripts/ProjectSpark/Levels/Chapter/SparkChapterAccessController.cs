using System;
using UnityEngine;

namespace ProjectSpark.Gameplay
{
    /// <summary>
    /// Optional chapter entry policy. Does not modify electrical simulation,
    /// level evaluation, or legacy level completion data.
    /// Attach to the same GameObject as SparkLevelProgression and assign a campaign.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkChapterAccessController : MonoBehaviour
    {
        public enum PlayMode { Learner = 0, Expert = 1 }

        [SerializeField] private SparkCampaignDefinition campaign;
        [SerializeField] private SparkLevelProgression progression;
        [SerializeField] private PlayMode activeMode = PlayMode.Learner;
        [SerializeField] private string profileId = "Default";

        public PlayMode ActiveMode => activeMode;
        public SparkCampaignDefinition Campaign => campaign;

        private void Awake()
        {
            if (progression == null) progression = GetComponent<SparkLevelProgression>();
        }

        public void SetMode(PlayMode mode) { activeMode = mode; }

        /// <summary>Record a validated assessment result, never a raw UI score.</summary>
        public bool RecordAssessment(string chapterId, int verifiedScore)
        {
            SparkChapterDefinition chapter = FindChapter(chapterId);
            if (chapter == null || verifiedScore < 0 || verifiedScore > 100) return false;
            string key = AssessmentKey(chapter.ChapterId);
            PlayerPrefs.SetInt(key, Mathf.Max(PlayerPrefs.GetInt(key, 0), verifiedScore));
            PlayerPrefs.Save();
            return true;
        }

        public bool CanEnterChapter(SparkChapterDefinition chapter)
        {
            if (chapter == null || campaign == null || progression == null) return false;
            int index = ChapterIndex(chapter);
            if (index < 0) return false;
            if (index == 0 || chapter.UnlockedByDefault || chapter.AccessMode == SparkChapterDefinition.ChapterAccessMode.Open)
                return true;

            bool previousCompleted = PreviousChapterCompleted(index);
            if (activeMode == PlayMode.Learner) return previousCompleted;

            bool assessmentPassed = PlayerPrefs.GetInt(AssessmentKey(chapter.ChapterId), -1)
                                    >= chapter.RequiredAssessmentScore;
            switch (chapter.AccessMode)
            {
                case SparkChapterDefinition.ChapterAccessMode.CompletePreviousChapter: return previousCompleted;
                case SparkChapterDefinition.ChapterAccessMode.PassAssessment: return assessmentPassed;
                case SparkChapterDefinition.ChapterAccessMode.CompletePreviousOrPassAssessment:
                    return previousCompleted || assessmentPassed;
                default: return false;
            }
        }

        /// <summary>
        /// Only grants entry to the FIRST level of an eligible chapter.
        /// Later levels continue to use ordinary level-by-level progression.
        /// </summary>
        public bool CanEnterLevel(SparkLevelDefinition level)
        {
            if (level == null || campaign == null) return false;
            SparkChapterDefinition chapter = campaign.FindChapter(level);
            return chapter != null && chapter.LevelCount > 0 && chapter.Levels[0] == level
                   && CanEnterChapter(chapter);
        }

        private bool PreviousChapterCompleted(int chapterIndex)
        {
            SparkChapterDefinition previous = campaign.GetChapter(chapterIndex - 1);
            if (previous == null || previous.LevelCount == 0 || progression.LevelManager == null) return false;
            SparkLevelDefinition[] all = progression.LevelManager.Levels;
            if (all == null) return false;
            foreach (SparkLevelDefinition required in previous.Levels)
            {
                if (required == null) return false;
                int index = Array.IndexOf(all, required);
                if (index < 0 || !progression.IsCompleted(index)) return false;
            }
            return true;
        }

        private int ChapterIndex(SparkChapterDefinition chapter)
        {
            for (int i = 0; i < campaign.ChapterCount; i++)
                if (campaign.GetChapter(i) == chapter) return i;
            return -1;
        }

        private SparkChapterDefinition FindChapter(string id)
        {
            if (campaign == null || string.IsNullOrEmpty(id)) return null;
            for (int i = 0; i < campaign.ChapterCount; i++)
            {
                SparkChapterDefinition chapter = campaign.GetChapter(i);
                if (chapter != null && chapter.ChapterId == id) return chapter;
            }
            return null;
        }

        private string AssessmentKey(string chapterId)
        {
            string campaignId = campaign != null ? campaign.CampaignId : "NoCampaign";
            return "ProjectSpark.Assessment." + profileId + "." + campaignId + "." + chapterId;
        }
    }
}
