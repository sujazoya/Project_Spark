using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace ProjectSpark.AAAArchitecture
{
    public enum SparkAAAStatus { Unknown, Locked, Available, InProgress, Completed, Certified, Requalification }
    [Serializable] public sealed class SparkAAAProgress
    {
        public SparkAAAStatus status;
        [Range(0, 1)] public float fraction;
        public string rank, reason;
        public bool canLaunch;
        public SparkAAAProgress Copy() => (SparkAAAProgress)MemberwiseClone();
    }
    // Presentation-only interface. The existing gameplay/progression systems own all decisions.
    public abstract class SparkAAAMissionGateway : MonoBehaviour
    {
        public event Action Changed;
        protected void NotifyChanged() => Changed?.Invoke();
        public abstract SparkAAAProgress Read(SparkAAALevel level);
        public abstract bool TryLaunch(SparkAAALevel level, out string reason);
    }
}
