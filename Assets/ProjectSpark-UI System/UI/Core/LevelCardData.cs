public readonly struct LevelCardData
{
    public readonly int LevelId;
    public readonly string Title;
    public readonly string Description;
    public readonly float Progress;
    public readonly bool IsUnlocked;
    public readonly bool IsCompleted;

    public LevelCardData(
        int levelId,
        string title,
        string description,
        float progress,
        bool isUnlocked,
        bool isCompleted)
    {
        LevelId = levelId;
        Title = title;
        Description = description;
        Progress = progress;
        IsUnlocked = isUnlocked;
        IsCompleted = isCompleted;
    }
}