using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectSpark.UI.VisualTeaching
{
    [Serializable]
    public sealed class SparkMeterVerifiedNode
    {
        public string terminalId;
        // Node voltage relative to the SAME solver reference for all nodes in this frame.
        public float potentialVolts;
    }

    [Serializable]
    public sealed class SparkMeterVerifiedNodeFrame
    {
        public string circuitId;
        // MUST match SparkVerifiedElectricalFrame.revision from the same completed solve.
        public int revision;
        public List<SparkMeterVerifiedNode> nodes = new List<SparkMeterVerifiedNode>();

        public SparkMeterVerifiedNodeFrame Copy()
        {
            var result = new SparkMeterVerifiedNodeFrame
            {
                circuitId = circuitId,
                revision = revision,
                nodes = new List<SparkMeterVerifiedNode>()
            };
            if (nodes != null)
                foreach (var n in nodes)
                    if (n != null)
                        result.nodes.Add(new SparkMeterVerifiedNode
                        {
                            terminalId = n.terminalId,
                            potentialVolts = n.potentialVolts
                        });
            return result;
        }
    }

    /// <summary>
    /// OPTIONAL trusted-solver API for arbitrary red-to-black node voltage.
    /// Publishes only completed solves; no runtime guessing or node inference.
    /// Call from Unity main thread. It does not modify electrical state.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkMeterVerifiedNodeFeed : MonoBehaviour
    {
        public event Action<SparkMeterVerifiedNodeFrame> FramePublished;

        public bool PublishVerifiedNodes(SparkMeterVerifiedNodeFrame frame)
        {
            if (frame == null || string.IsNullOrWhiteSpace(frame.circuitId) ||
                frame.circuitId.Length > 80 || frame.revision <= 0 ||
                frame.nodes == null || frame.nodes.Count > 128)
                return Reject("Invalid verified node frame header.");

            var ids = new HashSet<string>(StringComparer.Ordinal);
            foreach (var node in frame.nodes)
            {
                if (node == null || string.IsNullOrWhiteSpace(node.terminalId) ||
                    node.terminalId.Length > 80 || !ids.Add(node.terminalId) ||
                    float.IsNaN(node.potentialVolts) || float.IsInfinity(node.potentialVolts))
                    return Reject("Invalid, duplicate or non-finite node potential.");
            }

            FramePublished?.Invoke(frame.Copy());
            return true;
        }

        private bool Reject(string reason)
        {
            Debug.LogWarning("[Spark Multimeter Node Feed] " + reason, this);
            return false;
        }
    }
}
