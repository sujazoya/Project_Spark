namespace ProjectSpark.UI.Feedback
{
    public readonly struct UIFeedbackRequest
    {
        public readonly UIFeedbackType Type;

        public readonly UIFeedbackPriority Priority;

        public readonly string Title;

        public readonly string Message;

        public readonly float Duration;

        public UIFeedbackRequest(
            UIFeedbackType type,
            UIFeedbackPriority priority,
            string title,
            string message,
            float duration = 3f)
        {
            Type = type;
            Priority = priority;
            Title = title;
            Message = message;
            Duration = duration;
        }
    }
}