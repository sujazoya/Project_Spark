using System;
using UnityEngine;

namespace AAAUI
{
    [Serializable]
    public abstract class UIAnimationTrack
    {
        [SerializeField, Min(0f)] private float startTime;
        [SerializeField, Min(0f)] private float duration = 0.2f;
        [SerializeField] private UIEase ease = UIEase.EaseOut;
        [SerializeField] private int targetIndex;

        [SerializeField] private bool useSequenceDuration = false;

        public bool UseSequenceDuration => useSequenceDuration;

        public float StartTime => startTime;
        public float Duration => duration;
        public UIEase Ease => ease;
        public int TargetIndex => targetIndex;
        public float EndTime => startTime + duration;

        public abstract string DisplayName { get; }

        public void SetTiming(float start, float length)
        {
            startTime = Mathf.Max(0f, start);
            duration = Mathf.Max(0f, length);
        }
        public void SetUseSequenceDuration(
    bool value,
    float sequenceDuration)
        {
            useSequenceDuration = value;

            if (value)
                duration = Mathf.Max(
                    0f,
                    sequenceDuration
                );
        }

        public void SetTargetIndex(int index) => targetIndex = Mathf.Max(0, index);
        public void SetEase(UIEase value) => ease = value;

      public float EvaluateWeight(float time)
        {
            if (duration <= 0f) return time >= startTime ? 1f : 0f;
            float t = Mathf.Clamp01((time - startTime) / duration);
            return UIEaseUtility.Evaluate(t, ease);
        }

        public bool IsActive(float time) => time >= startTime && time <= EndTime;
    }


    public static class UIEaseUtility
    {
        public static float Evaluate(float t, UIEase ease)
        {
            t = Mathf.Clamp01(t);
            switch (ease)
            {
                case UIEase.EaseIn: return t * t;
                case UIEase.EaseOut: return 1f - (1f - t) * (1f - t);
                case UIEase.EaseInOut:
                    return t < 0.5f ? 2f * t * t : 1f - Mathf.Pow(-2f * t + 2f, 2f) * 0.5f;
                case UIEase.SmoothStep: return t * t * (3f - 2f * t);
                case UIEase.SmootherStep: return t * t * t * (t * (t * 6f - 15f) + 10f);
                default: return t;
            }
        }

    }

}