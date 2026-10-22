
using System;
using UnityEngine;

namespace ProjectSpark.Gameplay
{
    [CreateAssetMenu(
        fileName = "SparkCampaignDefinition",
        menuName = "Project Spark/Campaign Definition")]
    public sealed class SparkCampaignDefinition : ScriptableObject
    {
        [Header("Campaign")]
        [SerializeField]
        private string campaignId = "MainCampaign";

        [SerializeField]
        private string displayName = "ProjectSpark Academy";

        [Header("Chapters")]
        [SerializeField]
        private SparkChapterDefinition[] chapters =
            Array.Empty<SparkChapterDefinition>();

        public string CampaignId => campaignId;

        public string DisplayName => displayName;

        public SparkChapterDefinition[] Chapters => chapters;

        public int ChapterCount =>
            chapters != null ? chapters.Length : 0;

        public SparkChapterDefinition GetChapter(int index)
        {
            if (chapters == null ||
                index < 0 ||
                index >= chapters.Length)
            {
                return null;
            }

            return chapters[index];
        }

        public SparkChapterDefinition FindChapter(
            SparkLevelDefinition level)
        {
            if (level == null || chapters == null)
                return null;

            foreach (SparkChapterDefinition chapter in chapters)
            {
                if (chapter == null || chapter.Levels == null)
                    continue;

                foreach (SparkLevelDefinition entry in chapter.Levels)
                {
                    if (entry == level)
                        return chapter;
                }
            }

            return null;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (chapters == null)
                chapters = Array.Empty<SparkChapterDefinition>();
        }
#endif
    }
}
