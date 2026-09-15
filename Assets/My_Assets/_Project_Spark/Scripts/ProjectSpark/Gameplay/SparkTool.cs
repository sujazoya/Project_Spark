using UnityEngine;

namespace ProjectSpark.Gameplay
{
    public abstract class SparkTool : MonoBehaviour
    {
        [SerializeField] private SparkToolType toolType;
        public SparkToolType ToolType => toolType;
        public SparkInteractionSession Session { get; private set; }
        public bool IsBusy => Session != null && Session.IsActive;

        protected virtual void Awake() => toolType = GetToolType();
        protected abstract SparkToolType GetToolType();

        public virtual bool CanBegin(in SparkToolContext context, out string reason)
        {
            if (IsBusy) { reason = "Tool is busy."; return false; }
            reason = null; return true;
        }

        public SparkResult Begin(SparkToolContext context)
        {
            if (!CanBegin(context, out var reason)) return SparkResult.Rejected(reason);
            Session = context.Session; Session.Start();
            var result = OnBegin(context);
            if (!result.Succeeded) { Session.Stop(); Session = null; }
            return result;
        }

        public SparkResult Tick(SparkToolContext context)
        {
            if (!IsBusy) return SparkResult.Unavailable("Tool has no active session.");
            return OnUpdate(context);
        }

        public SparkResult End(SparkToolContext context)
        {
            if (!IsBusy) return SparkResult.Unavailable("Tool has no active session.");
            var result = OnEnd(context); Session.Stop(); Session = null; return result;
        }

        public SparkResult Cancel(SparkToolContext context)
        {
            if (!IsBusy) return SparkResult.Unavailable("Tool has no active session.");
            var result = OnCancel(context); Session.Stop(); Session = null; return result;
        }

        protected abstract SparkResult OnBegin(SparkToolContext context);
        protected virtual SparkResult OnUpdate(SparkToolContext context) => SparkResult.Success();
        protected virtual SparkResult OnEnd(SparkToolContext context) => SparkResult.Success();
        protected virtual SparkResult OnCancel(SparkToolContext context) => SparkResult.Cancelled();
    }

    public sealed class SparkToolContext
    {
        public SparkGameplayController Gameplay { get; }
        public SparkToolController Tools { get; }
        public SparkInteractionSession Session { get; }
        public SparkTargetHit TargetHit { get; }
        public SparkToolContext(SparkGameplayController gameplay, SparkToolController tools,
            SparkInteractionSession session, in SparkTargetHit targetHit)
        { Gameplay = gameplay; Tools = tools; Session = session; TargetHit = targetHit; }
    }
}
