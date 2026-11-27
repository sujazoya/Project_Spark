using System;
using UnityEngine;

namespace ProjectSpark.UI.VisualTeaching
{
    [CreateAssetMenu(menuName = "Project Spark/AI/Visual Teaching Lesson", fileName = "SparkVisualLesson")]
    public sealed class SparkVisualLesson : ScriptableObject
    {
        public string lessonId = "tutorial";
        public string title = "Circuit Tutorial";
        public string goal = "Learn the circuit";
        public SparkTeachingStep[] steps = Array.Empty<SparkTeachingStep>();
    }
}
