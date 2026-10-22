
using System;
using UnityEngine;

namespace ProjectSpark.Gameplay
{
    [CreateAssetMenu(
        fileName = "SparkChapterDefinition",
        menuName = "Project Spark/Chapter Definition")]
    public sealed class SparkChapterDefinition : ScriptableObject
    {
        public enum ChapterAccessMode
        {
            Open = 0,
            CompletePreviousChapter = 1,
            PassAssessment = 2,
            CompletePreviousOrPassAssessment = 3
        }

        [Header("Identity")]
        [SerializeField]
        private string chapterId = "Chapter_01";

        [SerializeField]
        private string displayName = "Electrical Foundations";

        [TextArea(2, 5)]
        [SerializeField]
        private string description;

        [SerializeField, Min(1)]
        private int chapterNumber = 1;

        [Header("Levels")]
        [SerializeField]
        private SparkLevelDefinition[] levels =
            Array.Empty<SparkLevelDefinition>();

        [Header("Access Rules")]
        [SerializeField]
        private ChapterAccessMode accessMode =
            ChapterAccessMode.CompletePreviousOrPassAssessment;

        [SerializeField]
        private bool unlockedByDefault;

        [SerializeField, Range(0, 100)]
        private int requiredAssessmentScore = 80;

        [Header("Gameplay")]
        [SerializeField]
        private bool allowEasyMode = true;

        [SerializeField]
        private bool allowAdvancedMode = true;

        [SerializeField]
        private bool allowBuildMode = true;

        [SerializeField]
        private bool allowRepairMode = true;

        public string ChapterId => chapterId;

        public string DisplayName => displayName;

        public string Description => description;

        public int ChapterNumber => chapterNumber;

        public SparkLevelDefinition[] Levels => levels;

        public int LevelCount =>
            levels != null ? levels.Length : 0;

        public ChapterAccessMode AccessMode => accessMode;

        public bool UnlockedByDefault => unlockedByDefault;

        public int RequiredAssessmentScore =>
            requiredAssessmentScore;

        public bool AllowEasyMode => allowEasyMode;

        public bool AllowAdvancedMode => allowAdvancedMode;

        public bool AllowBuildMode => allowBuildMode;

        public bool AllowRepairMode => allowRepairMode;

#if UNITY_EDITOR
        private void OnValidate()
        {
            chapterNumber = Mathf.Max(1, chapterNumber);

            requiredAssessmentScore =
                Mathf.Clamp(requiredAssessmentScore, 0, 100);

            if (levels == null)
                levels = Array.Empty<SparkLevelDefinition>();
        }
#endif
    }
}
