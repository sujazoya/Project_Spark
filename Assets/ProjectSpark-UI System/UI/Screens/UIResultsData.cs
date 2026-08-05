namespace ProjectSpark.UI.Screens
{
    public readonly struct UIResultsData
    {
        public readonly bool Completed;

        public readonly string LevelId;

        public readonly string LevelTitle;

        public readonly float CompletionPercentage;

        public readonly float Time;

        public readonly float Accuracy;

        public readonly int Mistakes;

        public readonly float Efficiency;

        public UIResultsData(
            bool completed,
            string levelId,
            string levelTitle,
            float completionPercentage,
            float time,
            float accuracy,
            int mistakes,
            float efficiency)
        {
            Completed =
                completed;

            LevelId =
                levelId;

            LevelTitle =
                levelTitle;

            CompletionPercentage =
                completionPercentage;

            Time =
                time;

            Accuracy =
                accuracy;

            Mistakes =
                mistakes;

            Efficiency =
                efficiency;
        }
    }
}