using ProjectSpark.Gameplay;
using UnityEngine;

namespace ProjectSpark.Tools
{
    /// <summary>
    /// Project Spark selection tool.
    ///
    /// The selection controller owns the actual selection logic.
    /// This tool only connects the active SparkTool interaction
    /// with SparkSelectionController.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkSelectTool : SparkTool
    {
        // ============================================================
        // REFERENCES
        // ============================================================

        [Header("References")]

        [SerializeField]
        private SparkSelectionController selectionController;


        // ============================================================
        // TOOL TYPE
        // ============================================================

        protected override SparkToolType GetToolType()
        {
            return SparkToolType.Select;
        }


        // ============================================================
        // UNITY
        // ============================================================

        protected override void Awake()
        {
            base.Awake();

            ResolveReferences();
        }


        // ============================================================
        // REFERENCES
        // ============================================================

        private void ResolveReferences()
        {
            if (selectionController == null)
            {
                selectionController =
                    GetComponentInParent<
                        SparkSelectionController>();
            }
        }


        // ============================================================
        // VALIDATION
        // ============================================================

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

            ResolveReferences();

            if (selectionController == null)
            {
                reason =
                    "SparkSelectionController is not configured.";

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


        // ============================================================
        // BEGIN
        // ============================================================

        protected override SparkResult OnBegin(
            SparkToolContext context)
        {
            ResolveReferences();

            if (selectionController == null)
            {
                return SparkResult.Invalid(
                    "SparkSelectionController is not configured.");
            }

            bool selected =
                selectionController.TrySelect(
                    context.ScreenPosition,
                    out string reason);

            if (!selected)
            {
                return SparkResult.Invalid(
                    string.IsNullOrEmpty(reason)
                        ? "Unable to select an object."
                        : reason);
            }

            return SparkResult.Success(
                "Object selected.");
        }


        // ============================================================
        // END
        // ============================================================

        protected override SparkResult OnEnd(
            SparkToolContext context)
        {
            return SparkResult.Success();
        }


        // ============================================================
        // CANCEL
        // ============================================================

        protected override SparkResult OnCancel(
            SparkToolContext context)
        {
            return SparkResult.Cancelled();
        }
    }
}