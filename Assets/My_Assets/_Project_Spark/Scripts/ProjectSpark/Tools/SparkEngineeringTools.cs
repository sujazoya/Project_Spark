using ProjectSpark.Gameplay;
using ProjectSpark.Measurement;
using ProjectSpark.Scan;
using UnityEngine;

namespace ProjectSpark.Tools
{
    public sealed class SparkSelectTool : SparkTool
    {
        protected override SparkToolType GetToolType() => SparkToolType.Select;
        protected override SparkResult OnBegin(SparkToolContext context) { context.TargetHit.Target.SetSelected(true); return SparkResult.Success(); }
    }

    public sealed class SparkInspectTool : SparkTool
    {
        protected override SparkToolType GetToolType() => SparkToolType.Inspect;
        protected override SparkResult OnBegin(SparkToolContext context) => context.TargetHit.Target.Inspect(CreateContext(context));
        private SparkInteractionContext CreateContext(SparkToolContext c) => new(c.Gameplay, c.TargetHit.Target, SparkInteractionType.Inspect, ToolType, c.TargetHit.Point, c.TargetHit.Normal, Vector2.zero, c.TargetHit.Collider != null ? c.TargetHit.Collider.gameObject : null);
    }

    public sealed class SparkMeasureTool : SparkTool
    {
        [SerializeField] private SparkMeasurementSystem measurementSystem;
        [SerializeField] private SparkMeasurementType measurementType = SparkMeasurementType.Voltage;
        public SparkMeasurementReading LastReading { get; private set; }
        protected override SparkToolType GetToolType() => SparkToolType.Measure;
        protected override SparkResult OnBegin(SparkToolContext context)
        {
            if (measurementSystem == null) return SparkResult.Unavailable("Measurement system is not configured.");
            var terminal = context.TargetHit.Target.GetComponentInParent<SparkTerminal>();
            if (terminal == null) return SparkResult.Invalid("Target has no measurement terminal.");
            var result = measurementSystem.TryMeasureTerminal(terminal, measurementType, out var reading);
            if (result.Succeeded) LastReading = reading;
            return result;
        }
    }

    public sealed class SparkScanTool : SparkTool
    {
        [SerializeField] private SparkScanSystem scanSystem;
        public SparkScanReport LastReport { get; private set; }
        protected override SparkToolType GetToolType() => SparkToolType.Scan;
        protected override SparkResult OnBegin(SparkToolContext context)
        {
            if (scanSystem == null) return SparkResult.Unavailable("Scan system is not configured.");
            var result = scanSystem.TryScan(context.TargetHit.Target, out var report);
            if (result.Succeeded) LastReport = report;
            return result;
        }
    }

    public sealed class SparkTransformManipulable : MonoBehaviour, ISparkMovable, ISparkRotatable
    {
        [SerializeField] private bool allowMove = true;
        [SerializeField] private bool allowRotate = true;
        [SerializeField, Min(0f)] private float moveSnap = 0f;
        [SerializeField] private Vector3 rotationAxis = Vector3.up;
        [SerializeField, Min(0f)] private float rotationDegreesPerPixel = 0.25f;
        [SerializeField, Min(0f)] private float rotationSnap = 0f;

        private Camera camera;
        private Plane plane;
        private Vector3 startPosition;
        private Quaternion startRotation;
        private Vector3 grabOffset;
        private Vector2 startPointer;

        public bool CanMove(in SparkInteractionContext context, out string reason) { reason = allowMove ? null : "Movement disabled."; return allowMove; }
        public bool CanRotate(in SparkInteractionContext context, out string reason) { reason = allowRotate ? null : "Rotation disabled."; return allowRotate; }

        public SparkResult BeginMove(in SparkInteractionContext context)
        {
            if (!CanMove(context, out var reason)) return SparkResult.Rejected(reason);
            camera = Camera.main;
            if (camera == null) return SparkResult.Unavailable("No camera available.");
            startPosition = transform.position;
            plane = new Plane(camera.transform.forward, context.HitPosition);
            if (!TryProject(context.ScreenPosition, out var point)) return SparkResult.Rejected("Pointer cannot be projected.");
            grabOffset = transform.position - point;
            return SparkResult.Success();
        }
        public SparkResult UpdateMove(in SparkInteractionContext context)
        {
            if (camera == null) return SparkResult.Unavailable("Movement camera unavailable.");
            if (!TryProject(context.ScreenPosition, out var point)) return SparkResult.Rejected("Pointer cannot be projected.");
            var target = point + grabOffset;
            if (moveSnap > 0f) target = new Vector3(Mathf.Round(target.x / moveSnap) * moveSnap, Mathf.Round(target.y / moveSnap) * moveSnap, Mathf.Round(target.z / moveSnap) * moveSnap);
            transform.position = target; return SparkResult.Success();
        }
        public SparkResult EndMove(in SparkInteractionContext context) { camera = null; return SparkResult.Success(); }
        public SparkResult CancelMove(in SparkInteractionContext context) { transform.position = startPosition; camera = null; return SparkResult.Cancelled("Movement reverted."); }

        public SparkResult BeginRotate(in SparkInteractionContext context) { if (!CanRotate(context, out var r)) return SparkResult.Rejected(r); startRotation = transform.rotation; startPointer = context.ScreenPosition; return SparkResult.Success(); }
        public SparkResult UpdateRotate(in SparkInteractionContext context)
        {
            float delta = (context.ScreenPosition.x - startPointer.x) - (context.ScreenPosition.y - startPointer.y);
            float angle = delta * rotationDegreesPerPixel;
            if (rotationSnap > 0f) angle = Mathf.Round(angle / rotationSnap) * rotationSnap;
            transform.rotation = startRotation * Quaternion.AngleAxis(angle, rotationAxis.sqrMagnitude > 0f ? rotationAxis.normalized : Vector3.up);
            return SparkResult.Success();
        }
        public SparkResult EndRotate(in SparkInteractionContext context) => SparkResult.Success();
        public SparkResult CancelRotate(in SparkInteractionContext context) { transform.rotation = startRotation; return SparkResult.Cancelled("Rotation reverted."); }

        private bool TryProject(Vector2 screen, out Vector3 world)
        { var ray = camera.ScreenPointToRay(screen); if (plane.Raycast(ray, out var enter) && enter >= 0f) { world = ray.GetPoint(enter); return true; } world = default; return false; }
    }
}
