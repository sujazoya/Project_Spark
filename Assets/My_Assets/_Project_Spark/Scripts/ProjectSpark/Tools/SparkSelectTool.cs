using ProjectSpark.Gameplay;
using UnityEngine;

namespace ProjectSpark.Tools
{
    [DisallowMultipleComponent]
    public sealed class SparkSelectTool : SparkTool
    {
        [Header("References")]
        [SerializeField]
        private SparkSelectionController selectionController;

        protected override SparkToolType GetToolType()
        {
            return SparkToolType.Select;
        }

        protected override void Awake()
        {
            base.Awake();

            if (selectionController == null)
            {
                selectionController =
                    GetComponentInParent<SparkSelectionController>();
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

            if (selectionController == null)
            {
                reason =
                    "SparkSelectionController is not configured.";

                return false;
            }

            if (!context.HasTarget)
            {
                reason = "No selectable object was hit.";
                return false;
            }

            reason = null;
            return true;
        }

        protected override SparkResult OnBegin(
            SparkToolContext context)
        {
            if (selectionController == null)
            {
                return SparkResult.Invalid(
                    "SparkSelectionController is not configured.");
            }

            if (!context.HasTarget)
            {
                selectionController.ClearSelection();

                return SparkResult.Invalid(
                    "No selectable object was hit.");
            }

            SparkSelectable selectable =
                context.TargetHit.Collider
                    .GetComponentInParent<SparkSelectable>();

            if (selectable == null)
            {
                selectionController.ClearSelection();

                return SparkResult.Invalid(
                    "Hit object is not selectable.");
            }

            /*
             * IMPORTANT:
             *
             * Select() automatically deselects the previous object
             * and selects this object.
             */
            selectionController.Select(selectable);

            return SparkResult.Success(
                "Object selected.");
        }

        protected override SparkResult OnEnd(
            SparkToolContext context)
        {
            return SparkResult.Success();
        }

        protected override SparkResult OnCancel(
            SparkToolContext context)
        {
            return SparkResult.Cancelled();
        }
    }
}