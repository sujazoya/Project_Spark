using AAAUI.VFX;
using UnityEngine;

namespace ProjectSpark.SparkV5.Pro
{
    /// <summary>
    /// v17 visual QA only. The existing player SignalWireBuilder remains authoritative.
    /// Never creates, destroys or changes a wire's electrical connection.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkAAAWireVisualGuard : MonoBehaviour
    {
        [SerializeField] private SparkAINativeSignalWirePort nativePort;
        [SerializeField, Min(0.1f)] private float diagnosticInterval = 0.4f;
        [SerializeField] private bool warnAboutRecordedRoutePoints = true;

        private SignalWireBuilder watched;
        private bool warned;
        private float nextProbe;
        public string Status { get; private set; } = "No AI-native cable active.";
        public int LastRecordedWaypoints { get; private set; }

        private void Awake()
        {
            if (!nativePort) nativePort = GetComponent<SparkAINativeSignalWirePort>();
        }

        private void OnDisable()
        {
            watched = null;
            warned = false;
            Status = "Visual QA disabled.";
        }

        private void Update()
        {
            if (Time.unscaledTime < nextProbe) return;
            nextProbe = Time.unscaledTime + diagnosticInterval;
            if (!nativePort || !nativePort.IsWiring)
            {
                watched = null;
                warned = false;
                Status = "No AI-native cable active.";
                LastRecordedWaypoints = 0;
                return;
            }

            SignalWireBuilder builder = nativePort.ResolvedPlayerBuilder;
            if (!builder)
            {
                Status = "AI port has not resolved a player wire builder.";
                return;
            }
            if (builder != watched) { watched = builder; warned = false; }
            LastRecordedWaypoints = builder.DragRoutePointCount;
            Status = "AI-native cable: routing=" + builder.CableRouting +
                ", sampled waypoints=" + LastRecordedWaypoints +
                ", drawing=" + builder.IsDrawing;

            if (!nativePort.UsesPlayerPointer && warnAboutRecordedRoutePoints && LastRecordedWaypoints > 0 && !warned)
            {
                warned = true;
                Debug.LogWarning("[Spark AAA v17] Native AI wire contains " +
                    LastRecordedWaypoints + " saved drag points. These may create" +
                    " travel-route loops. Existing points were NOT deleted; inspect" +
                    " AI port setup and mesh route ownership.", this);
            }
        }
    }
}
