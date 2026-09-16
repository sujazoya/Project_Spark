using ProjectSpark.Gameplay;
using UnityEngine;

namespace ProjectSpark.Wire
{
    public sealed class SparkWireTool : SparkTool
    {
        [Header("Wire System")]
        [SerializeField]
        private SparkWireSystem wireSystem;

        [Header("Wire")]
        [SerializeField]
        private bool clearSourceAfterFailedConnection = false;

        private SparkTerminal sourceTerminal;

        public SparkTerminal SourceTerminal
        {
            get { return sourceTerminal; }
        }

        public bool HasSourceTerminal
        {
            get { return sourceTerminal != null; }
        }

        protected override SparkToolType GetToolType()
        {
            return SparkToolType.Wire;
        }

        protected override void Awake()
        {
            base.Awake();

            if (wireSystem == null)
            {
                wireSystem =
                    GetComponentInParent<SparkWireSystem>();
            }
        }

        public override bool CanBegin(
            in SparkToolContext context,
            out string reason)
        {
            if (!base.CanBegin(
                    context,
                    out reason))
            {
                return false;
            }

            if (wireSystem == null)
            {
                reason =
                    "Wire system is not configured.";

                return false;
            }

            if (context.TargetHit.Target == null)
            {
                reason =
                    "No target component.";

                return false;
            }

            SparkTerminal terminal =
                context.TargetHit.Target
                    .GetComponentInParent<SparkTerminal>();

            if (terminal == null)
            {
                reason =
                    "Target has no terminal.";

                return false;
            }

            if (sourceTerminal != null &&
                sourceTerminal == terminal)
            {
                reason =
                    "Source and target terminals are the same.";

                return false;
            }

            reason = null;
            return true;
        }

        protected override SparkResult OnBegin(
            SparkToolContext context)
        {
            if (context.TargetHit.Target == null)
            {
                return SparkResult.Invalid(
                    "No target component.");
            }

            SparkTerminal terminal =
                context.TargetHit.Target
                    .GetComponentInParent<SparkTerminal>();

            if (terminal == null)
            {
                return SparkResult.Invalid(
                    "Target has no terminal.");
            }

            // --------------------------------------------------------
            // FIRST TERMINAL
            // --------------------------------------------------------

            if (sourceTerminal == null)
            {
                sourceTerminal =
                    terminal;

                return SparkResult.Success(
                    "Wire source selected.");
            }

            // --------------------------------------------------------
            // SECOND TERMINAL
            // --------------------------------------------------------

            SparkInteractionContext interactionContext =
                context.Gameplay.CreateInteractionContext(
                    context.TargetHit,
                    SparkInteractionType.Connect);

            SparkResult result =
                wireSystem.TryConnect(
                    sourceTerminal,
                    terminal,
                    interactionContext,
                    out _);

            if (result.Succeeded)
            {
                sourceTerminal = null;
            }
            else if (clearSourceAfterFailedConnection)
            {
                sourceTerminal = null;
            }

            return result;
        }

        protected override SparkResult OnCancel(
            SparkToolContext context)
        {
            sourceTerminal = null;

            return SparkResult.Cancelled(
                "Wire connection cancelled.");
        }
    }
}