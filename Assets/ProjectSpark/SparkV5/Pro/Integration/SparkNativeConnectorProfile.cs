using UnityEngine;

namespace ProjectSpark.SparkV5.Pro
{
    [CreateAssetMenu(menuName = "Project Spark/Native Wire/Connector Motion Profile")]
    public sealed class SparkNativeConnectorProfile : ScriptableObject
    {
        [Tooltip("Measured distance from the terminal contact to the start of insertion, in world units.")]
        [SerializeField, Min(.001f)] private float insertionStroke = .035f;
        [SerializeField, Range(.001f, .012f)] private float contactTolerance = .005f;
        [SerializeField, Range(1f, 30f)] private float axisToleranceDegrees = 10f;
        [SerializeField, Min(.001f)] private float releaseDistance = .025f;

        internal float InsertionStroke => insertionStroke;
        internal float ContactTolerance => contactTolerance;
        internal float AxisToleranceDegrees => axisToleranceDegrees;
        internal float ReleaseDistance => releaseDistance;
    }
}
