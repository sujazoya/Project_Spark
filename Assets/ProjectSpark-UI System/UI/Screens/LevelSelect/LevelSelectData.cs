using UnityEngine;

namespace ProjectSpark.UI.Screens
{
    [CreateAssetMenu(
        fileName = "LevelSelectData",
        menuName = "ProjectSpark/UI/Level Select Data")]
    public sealed class LevelSelectData :
        ScriptableObject
    {
        [SerializeField]
        private string levelId;

        [SerializeField]
        private string missionNumber;

        [SerializeField]
        private string title;

        [TextArea(2, 5)]
        [SerializeField]
        private string description;

        [SerializeField]
        private Sprite previewImage;

        [SerializeField]
        private int difficulty;

        public string LevelId =>
            levelId;

        public string MissionNumber =>
            missionNumber;

        public string Title =>
            title;

        public string Description =>
            description;

        public Sprite PreviewImage =>
            previewImage;

        public int Difficulty =>
            difficulty;
    }
}