namespace ProjectSpark.UI.Feedback
{
    public enum UISimulationStatus
    {
        Offline,
        Initializing,
        Running,
        Paused,
        Fault,
        Complete
    }

    public readonly struct UISimulationState
    {
        public readonly UISimulationStatus Status;

        public readonly string Message;

        public readonly float Progress;

        public UISimulationState(
            UISimulationStatus status,
            string message,
            float progress)
        {
            Status = status;
            Message = message;
            Progress = progress;
        }
    }
}