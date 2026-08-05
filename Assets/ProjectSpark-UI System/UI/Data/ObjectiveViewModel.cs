namespace ProjectSpark.UI.Data
{
    public sealed class ObjectiveViewModel
    {
        public string ObjectiveId
        {
            get;
        }

        public string Title
        {
            get;
        }

        public string Description
        {
            get;
        }

        public float Progress
        {
            get;
        }

        public bool IsCompleted
        {
            get;
        }

        public bool IsFailed
        {
            get;
        }

        public ObjectiveViewModel(
            string objectiveId,
            string title,
            string description,
            float progress,
            bool isCompleted,
            bool isFailed)
        {
            ObjectiveId =
                objectiveId;

            Title =
                title;

            Description =
                description;

            Progress =
                progress;

            IsCompleted =
                isCompleted;

            IsFailed =
                isFailed;
        }
    }
}