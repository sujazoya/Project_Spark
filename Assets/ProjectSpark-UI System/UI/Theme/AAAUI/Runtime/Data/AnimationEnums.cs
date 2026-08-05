namespace AAAUI
{
    public enum UIPlaybackState { Stopped, Playing, Paused }
    public enum UIPlaybackDirection { Forward, Reverse }
    public enum UIAnimationSequenceType { Open, Close, Loop }
    public enum UIEase
    {
        Linear, EaseIn, EaseOut, EaseInOut,
        SmoothStep, SmootherStep
    }
}