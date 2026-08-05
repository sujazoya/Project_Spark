namespace ProjectSpark.UI.Feedback
{
    public enum UIProcessStage
    {
        None,
        Inspection,
        Diagnostics,
        FaultIdentification,
        ComponentRemoval,
        ComponentReplacement,
        Soldering,
        Verification,
        Complete
    }

    public readonly struct UIProcessState
    {
        public readonly UIProcessStage Stage;

        public readonly string DisplayName;

        public readonly float Progress;

        public UIProcessState(
            UIProcessStage stage,
            string displayName,
            float progress)
        {
            Stage = stage;
            DisplayName = displayName;
            Progress = progress;
        }
    }
}