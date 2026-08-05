namespace ProjectSpark.UI.Feedback
{
    public readonly struct UILevelState
    {
        public readonly string LevelId;

        public readonly string LevelTitle;

        public readonly float Completion;

        public readonly float Time;

        public readonly int Mistakes;

        public readonly float Accuracy;

        public readonly float Efficiency;

        public UILevelState(
            string levelId,
            string levelTitle,
            float completion,
            float time,
            int mistakes,
            float accuracy,
            float efficiency)
        {
            LevelId = levelId;
            LevelTitle = levelTitle;
            Completion = completion;
            Time = time;
            Mistakes = mistakes;
            Accuracy = accuracy;
            Efficiency = efficiency;
        }
    }
}