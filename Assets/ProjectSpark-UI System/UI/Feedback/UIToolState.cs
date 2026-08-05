namespace ProjectSpark.UI.Feedback
{
    public readonly struct UIToolState
    {
        public readonly string ToolId;

        public readonly string DisplayName;

        public readonly bool IsAvailable;

        public readonly bool IsLocked;

        public readonly bool IsSelected;

        public UIToolState(
            string toolId,
            string displayName,
            bool isAvailable,
            bool isLocked,
            bool isSelected)
        {
            ToolId = toolId;
            DisplayName = displayName;
            IsAvailable = isAvailable;
            IsLocked = isLocked;
            IsSelected = isSelected;
        }
    }
}