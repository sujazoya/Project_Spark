using ProjectSpark.Gameplay;
using ProjectSpark.HolographicViewer;
using UnityEngine;

namespace ProjectSpark.Tools
{
    [DisallowMultipleComponent]
    public sealed class SparkMoveTool : SparkTool
    {
        [Header("References")]
        [SerializeField]
        private SparkSelectionController selectionController;

        [SerializeField]
        private HolographicComponentManipulator manipulator;

        protected override SparkToolType GetToolType()
        {
            return SparkToolType.Move;
        }

        protected override void Awake()
        {
            base.Awake();

            if (selectionController == null)
            {
                selectionController =
                    GetComponentInParent<SparkSelectionController>();
            }

            if (manipulator == null)
            {
                manipulator =
                    GetComponentInParent<HolographicComponentManipulator>();
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

            if (manipulator == null)
            {
                reason =
                    "HolographicComponentManipulator is not configured.";

                return false;
            }

            if (!selectionController.HasSelection)
            {
                reason =
                    "No object is selected.";

                return false;
            }

            if (!context.HasCamera)
            {
                reason =
                    "Tool context does not contain a camera.";

                return false;
            }

            reason = null;
            return true;
        }

        protected override SparkResult OnBegin(
            SparkToolContext context)
        {
            SparkSelectable selected =
                selectionController.SelectedObject;

            if (selected == null)
            {
                return SparkResult.Invalid(
                    "No object is selected.");
            }

            /*
             * Safety:
             * If a previous manipulation somehow remained active,
             * cancel it before starting a new one.
             */
            if (manipulator.IsManipulating)
            {
                manipulator.Cancel();
            }

            bool started =
                manipulator.BeginMove(
                    selected.transform,
                    context.ScreenPosition,
                    out string reason);

            if (!started)
            {
                return SparkResult.Invalid(
                    string.IsNullOrEmpty(reason)
                        ? "Unable to start move manipulation."
                        : reason);
            }

            return SparkResult.Success(
                "Move manipulation started.");
        }

        protected override SparkResult OnUpdate(
            SparkToolContext context)
        {
            if (manipulator == null)
            {
                return SparkResult.Invalid(
                    "HolographicComponentManipulator is not configured.");
            }

            if (!manipulator.IsManipulating)
            {
                return SparkResult.Unavailable(
                    "Move manipulation is not active.");
            }

            manipulator.UpdateManipulation(
                context.ScreenPosition);

            return SparkResult.Success();
        }

        protected override SparkResult OnEnd(
            SparkToolContext context)
        {
            if (manipulator == null)
            {
                return SparkResult.Invalid(
                    "HolographicComponentManipulator is not configured.");
            }

            if (manipulator.IsManipulating)
            {
                manipulator.Commit();
            }

            /*
             * Do NOT clear the selection here.
             *
             * The object remains selected so Move can be activated
             * again immediately.
             */

            return SparkResult.Success(
                "Move manipulation committed.");
        }

        protected override SparkResult OnCancel(
            SparkToolContext context)
        {
            if (manipulator == null)
            {
                return SparkResult.Invalid(
                    "HolographicComponentManipulator is not configured.");
            }

            if (manipulator.IsManipulating)
            {
                manipulator.Cancel();
            }

            return SparkResult.Cancelled(
                "Move manipulation cancelled.");
        }

        private void OnDisable()
        {
            if (manipulator != null &&
                manipulator.IsManipulating)
            {
                manipulator.Cancel();
            }
        }
    }
}