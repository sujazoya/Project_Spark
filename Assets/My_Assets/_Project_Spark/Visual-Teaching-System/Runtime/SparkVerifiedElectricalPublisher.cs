using System;
using UnityEngine;

namespace ProjectSpark.UI.VisualTeaching
{
    /// <summary>
    /// A simple, explicit handoff point for completed solver results.
    /// The actual electrical solver calls PublishVerifiedFrame AFTER solving.
    /// This component does not inspect physics or guess measurements.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkVerifiedElectricalPublisher : MonoBehaviour
    {
        public event Action<SparkVerifiedElectricalFrame> FramePublished;
        public int PublishedCount { get; private set; }

        public void PublishVerifiedFrame(SparkVerifiedElectricalFrame completedFrame)
        {
            if (completedFrame == null)
            {
                Debug.LogWarning("[Spark Solver Publisher] Ignored null frame.", this);
                return;
            }

            // Snapshot ownership is isolated from any pooled solver buffers.
            FramePublished?.Invoke(completedFrame.Copy());
            PublishedCount++;
        }
    }
}
