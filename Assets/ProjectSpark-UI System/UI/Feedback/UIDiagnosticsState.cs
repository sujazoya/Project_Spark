namespace ProjectSpark.UI.Feedback
{
    public enum UIDiagnosticsStatus
    {
        Idle,
        Scanning,
        Analyzing,
        Detecting,
        Identifying,
        Complete,
        Failed
    }

    public readonly struct UIDiagnosticsState
    {
        public readonly UIDiagnosticsStatus Status;

        public readonly float Progress;

        public readonly string Message;

        public readonly string ComponentId;

        public readonly float Confidence;

        public UIDiagnosticsState(
            UIDiagnosticsStatus status,
            float progress,
            string message,
            string componentId,
            float confidence)
        {
            Status = status;
            Progress = progress;
            Message = message;
            ComponentId = componentId;
            Confidence = confidence;
        }
    }
}