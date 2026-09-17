using ProjectSpark.Gameplay;
using ProjectSpark.Scanner;
using UnityEngine;

namespace ProjectSpark.Tools
{
    [DisallowMultipleComponent]
    public sealed class SparkScanTool : SparkTool
    {
        [Header("References")]

        [SerializeField]
        private SparkScanProcessController processController;

        [Header("Behaviour")]

        [SerializeField]
        private bool keepCompletedScan = true;

        private ScannerComponentTarget activeTarget;

        protected override SparkToolType GetToolType()
        {
            return SparkToolType.Scan;
        }

        protected override void Awake()
        {
            base.Awake();

            if (processController == null)
            {
                processController =
                    GetComponentInParent<SparkScanProcessController>();
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

            if (processController == null)
            {
                reason =
                    "SparkScanProcessController is not configured.";

                return false;
            }

            if (!context.HasTarget)
            {
                reason = "No scan target was hit.";
                return false;
            }

            ScannerComponentTarget target =
                FindScannerTarget(
                    context.TargetHit.Collider);

            if (target == null)
            {
                reason =
                    "Hit object does not contain a scanner target.";

                return false;
            }

            if (!processController.CanStart(
                    target,
                    out reason))
            {
                return false;
            }

            return true;
        }

        protected override SparkResult OnBegin(
            SparkToolContext context)
        {
            activeTarget =
                FindScannerTarget(
                    context.TargetHit.Collider);

            if (activeTarget == null)
            {
                return SparkResult.Invalid(
                    "Scan target is missing.");
            }

            SparkScanProcessResult result =
                processController.StartScan(
                    activeTarget);

            if (!result.Success)
            {
                activeTarget = null;

                return SparkResult.Rejected(
                    result.Message);
            }

            return SparkResult.Success(
                result.Message);
        }

        protected override SparkResult OnUpdate(
            SparkToolContext context)
        {
            if (processController == null)
            {
                return SparkResult.Unavailable(
                    "Scan process controller is missing.");
            }

            if (processController.State ==
                SparkScanProcessState.Completed)
            {
                return SparkResult.Success(
                    "Component scan completed.");
            }

            if (!processController.IsActive)
            {
                return SparkResult.Unavailable(
                    "Scan process is not active.");
            }

            SparkScanProcessResult result =
                processController.UpdateProcess();

            if (!result.Success)
            {
                return SparkResult.Invalid(
                    result.Message);
            }

            return SparkResult.Success();
        }

        protected override SparkResult OnEnd(
            SparkToolContext context)
        {
            if (processController == null)
            {
                activeTarget = null;

                return SparkResult.Unavailable(
                    "Scan process controller is missing.");
            }

            if (processController.IsCompleted &&
                keepCompletedScan)
            {
                activeTarget = null;

                return SparkResult.Success(
                    "Component scan completed.");
            }

            SparkScanProcessResult result =
                processController.EndScan();

            activeTarget = null;

            if (!result.Success)
            {
                return SparkResult.Cancelled(
                    result.Message);
            }

            return SparkResult.Success(
                result.Message);
        }

        protected override SparkResult OnCancel(
            SparkToolContext context)
        {
            if (processController == null)
            {
                activeTarget = null;

                return SparkResult.Cancelled(
                    "Scan process controller is missing.");
            }

            SparkScanProcessResult result =
                processController.CancelScan();

            activeTarget = null;

            return SparkResult.Cancelled(
                result.Message);
        }

        private ScannerComponentTarget FindScannerTarget(
            Collider collider)
        {
            if (collider == null)
            {
                return null;
            }

            ScannerComponentTarget target =
                collider.GetComponentInParent<
                    ScannerComponentTarget>();

            if (target != null)
            {
                return target;
            }

            Transform current =
                collider.transform;

            while (current != null)
            {
                target =
                    current.GetComponent<
                        ScannerComponentTarget>();

                if (target != null)
                {
                    return target;
                }

                current = current.parent;
            }

            return null;
        }

        private void OnDisable()
        {
            if (processController != null &&
                processController.IsActive)
            {
                processController.CancelScan();
            }

            activeTarget = null;
        }
    }
}