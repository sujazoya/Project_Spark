namespace ProjectSpark.UI.VFX
{
    public enum SparkVFXBaseState
    {
        Normal,
        Hover,
        Selected,
        Target,
        Disabled,
        Locked
    }


    public enum SparkVFXLoopType
    {
        None,
        Target,
        Selected,
        Warning,
        Error,
        Active,
        Scan,
        Processing,
        Locked
    }


    public enum SparkVFXOverrideType
    {
        None,
        Press,
        Release,
        Focus,
        Unfocus,
        Success,
        Error,
        Warning,
        Confirm,
        Cancel,
        Unlock,
        LevelComplete,
        Notification,
        Alert
    }
}