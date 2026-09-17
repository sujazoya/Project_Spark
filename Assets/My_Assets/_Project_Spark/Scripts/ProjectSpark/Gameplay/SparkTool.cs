using UnityEngine;

namespace ProjectSpark.Gameplay
{
    public abstract class SparkTool : MonoBehaviour
    {
        [SerializeField]
        private SparkToolType toolType;

        public SparkToolType ToolType
        {
            get
            {
                return toolType;
            }
        }

        public SparkInteractionSession Session
        {
            get;
            private set;
        }

        public bool IsBusy
        {
            get
            {
                return Session != null &&
                       Session.IsActive;
            }
        }

        protected virtual void Awake()
        {
            toolType = GetToolType();
        }

        protected abstract SparkToolType GetToolType();

        public virtual bool CanBegin(
            in SparkToolContext context,
            out string reason)
        {
            if (IsBusy)
            {
                reason = "Tool is busy.";
                return false;
            }

            reason = null;
            return true;
        }

        public SparkResult Begin(
            SparkToolContext context)
        {
            if (!CanBegin(
                    context,
                    out string reason))
            {
                return SparkResult.Rejected(reason);
            }

            if (context.Session == null)
            {
                return SparkResult.Rejected(
                    "Tool context does not contain a session.");
            }

            Session = context.Session;
            Session.Start();

            SparkResult result =
                OnBegin(context);

            if (!result.Succeeded)
            {
                Session.Stop();
                Session = null;
            }

            return result;
        }

        public SparkResult Tick(
            SparkToolContext context)
        {
            if (!IsBusy)
            {
                return SparkResult.Unavailable(
                    "Tool has no active session.");
            }

            return OnUpdate(context);
        }

        public SparkResult End(
            SparkToolContext context)
        {
            if (!IsBusy)
            {
                return SparkResult.Unavailable(
                    "Tool has no active session.");
            }

            SparkResult result =
                OnEnd(context);

            Session.Stop();
            Session = null;

            return result;
        }

        public SparkResult Cancel(
            SparkToolContext context)
        {
            if (!IsBusy)
            {
                return SparkResult.Unavailable(
                    "Tool has no active session.");
            }

            SparkResult result =
                OnCancel(context);

            Session.Stop();
            Session = null;

            return result;
        }

        protected abstract SparkResult OnBegin(
            SparkToolContext context);

        protected virtual SparkResult OnUpdate(
            SparkToolContext context)
        {
            return SparkResult.Success();
        }

        protected virtual SparkResult OnEnd(
            SparkToolContext context)
        {
            return SparkResult.Success();
        }

        protected virtual SparkResult OnCancel(
            SparkToolContext context)
        {
            return SparkResult.Cancelled();
        }
        
    }
    public sealed class SparkToolContext
    {
        public SparkGameplayController Gameplay
        {
            get;
        }

        public SparkToolController Tools
        {
            get;
        }

        public SparkInteractionSession Session
        {
            get;
        }

        public SparkTargetHit TargetHit
        {
            get;
        }

        public SparkInteractionContext InteractionContext
        {
            get;
        }

        public Vector2 ScreenPosition
        {
            get;
        }

        public Camera Camera
        {
            get;
        }

        public Ray PointerRay
        {
            get;
        }

        public bool HasCamera
        {
            get
            {
                return Camera != null;
            }
        }

        public bool HasTarget
        {
            get
            {
                return TargetHit.Target != null;
            }
        }

        public SparkToolContext(
            SparkGameplayController gameplay,
            SparkToolController tools,
            SparkInteractionSession session,
            in SparkTargetHit targetHit,
            Vector2 screenPosition,
            Camera camera,
            SparkInteractionContext interactionContext)
        {
            Gameplay = gameplay;
            Tools = tools;
            Session = session;
            TargetHit = targetHit;
            ScreenPosition = screenPosition;
            Camera = camera;
            InteractionContext = interactionContext;

            if (camera != null)
            {
                PointerRay =
                    camera.ScreenPointToRay(
                        screenPosition);
            }
            else
            {
                PointerRay = default;
            }
        }
    }
}
