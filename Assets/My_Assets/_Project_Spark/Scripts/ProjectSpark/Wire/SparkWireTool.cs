using ProjectSpark.Gameplay;
using UnityEngine;

namespace ProjectSpark.Wire
{
    public sealed class SparkWireTool : SparkTool
    {
        [SerializeField] private SparkWireSystem wireSystem;
        private SparkTerminal sourceTerminal;

        protected override SparkToolType GetToolType() => SparkToolType.Wire;
        protected override void Awake()
        {
            base.Awake();
            if (wireSystem == null) wireSystem = GetComponentInParent<SparkWireSystem>();
        }

        public override bool CanBegin(in SparkToolContext context, out string reason)
        {
            if (!base.CanBegin(context, out reason)) return false;
            if (wireSystem == null) { reason = "Wire system is not configured."; return false; }
            var terminal = context.TargetHit.Target.GetComponentInParent<SparkTerminal>();
            if (terminal == null) { reason = "Target has no terminal."; return false; }
            if (sourceTerminal != null && sourceTerminal == terminal)
            { reason = "Source and target terminals are the same."; return false; }
            reason = null; return true;
        }

        protected override SparkResult OnBegin(SparkToolContext context)
        {
            var terminal = context.TargetHit.Target.GetComponentInParent<SparkTerminal>();
            if (sourceTerminal == null) { sourceTerminal = terminal; return SparkResult.Success("Wire source selected."); }
            var result = wireSystem.TryConnect(sourceTerminal, terminal, CreateContext(context), out _);
            if (result.Succeeded) sourceTerminal = null;
            return result;
        }

        protected override SparkResult OnCancel(SparkToolContext context) { sourceTerminal = null; return SparkResult.Cancelled(); }
        private SparkInteractionContext CreateContext(SparkToolContext c) => new(c.Gameplay, c.TargetHit.Target,
            SparkInteractionType.Connect, ToolType, c.TargetHit.Point, c.TargetHit.Normal, Vector2.zero,
            c.TargetHit.Collider != null ? c.TargetHit.Collider.gameObject : null);
    }
}
