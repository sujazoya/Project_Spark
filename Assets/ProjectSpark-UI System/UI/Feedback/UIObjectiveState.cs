namespace ProjectSpark.UI.Feedback
{
    public enum UIObjectiveStatus
    {
        Locked,
        Available,
        Active,
        Completed,
        Failed
    }

    public readonly struct UIObjectiveState
    {
        public readonly string Id;

        public readonly string Title;

        public readonly string Description;

        public readonly UIObjectiveStatus Status;

        public readonly float Progress;

        public UIObjectiveState(
            string id,
            string title,
            string description,
            UIObjectiveStatus status,
            float progress)
        {
            Id = id;
            Title = title;
            Description = description;
            Status = status;
            Progress = progress;
        }
    }
}