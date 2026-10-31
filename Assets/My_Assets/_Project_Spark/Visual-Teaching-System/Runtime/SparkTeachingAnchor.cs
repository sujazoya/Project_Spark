using System.Collections.Generic;
using UnityEngine;

namespace ProjectSpark.UI.VisualTeaching
{
    [DisallowMultipleComponent]
    public sealed class SparkTeachingAnchor : MonoBehaviour
    {
        private static readonly Dictionary<string, SparkTeachingAnchor> Registry =
            new Dictionary<string, SparkTeachingAnchor>(System.StringComparer.Ordinal);

        [SerializeField] private string anchorId = "resistor";
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 worldOffset;

        public string AnchorId => anchorId;
        public Vector3 WorldPosition => (target != null ? target : transform).TransformPoint(worldOffset);

        private void OnEnable()
        {
            if (string.IsNullOrWhiteSpace(anchorId)) return;
            if (Registry.TryGetValue(anchorId, out var other) && other != null && other != this)
                Debug.LogWarning("[Spark Teaching] Duplicate scene anchor: " + anchorId, this);
            Registry[anchorId] = this;
        }

        private void OnDisable()
        {
            if (Registry.TryGetValue(anchorId, out var active) && active == this)
                Registry.Remove(anchorId);
        }

        public static bool TryFind(string id, out SparkTeachingAnchor anchor)
        {
            anchor = null;
            return !string.IsNullOrWhiteSpace(id) && Registry.TryGetValue(id, out anchor)
                   && anchor != null && anchor.isActiveAndEnabled;
        }
    }
}
