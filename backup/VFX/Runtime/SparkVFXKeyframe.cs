using System;
using UnityEngine;

[Serializable]
public sealed class SparkVFXKeyframe
{
    [Range(0f, 1f)]
    public float time;

    public float glow;
    public float scan;
    public float sweep;
    public float flash;
    public float glitch;
    public float flicker;
    public float reveal;
    public float dissolve;
    public float sweepPosition;

    [Header("Transition")]

    [SerializeField]
    private AnimationCurve transitionCurve =
        AnimationCurve.EaseInOut(
            0f,
            0f,
            1f,
            1f
        );

    public AnimationCurve TransitionCurve
    {
        get
        {
            return transitionCurve;
        }
    }
}