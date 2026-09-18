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
        private ScannerProcessController processController;

        [SerializeField]
        private ScannerComponentScanController componentScanController;

        [Header("Behaviour")]
        [SerializeField]
        private bool retainCompletedScan = true;

        [SerializeField]
        private bool cancelWhenTargetChanges = true;

        private ScannerComponentTarget activeTarget;
        private bool scanStarted;
        private bool resultShown;
        [SerializeField]
private ScannerDiagnosticPanelController diagnosticPanelController;

        public ScannerComponentTarget ActiveTarget
        {
            get
            {
                return activeTarget;
            }
        }

        public bool IsScanStarted
        {
            get
            {
                return scanStarted;
            }
        }

        public bool IsScanning
        {
            get
            {
                return processController != null &&
                       processController.IsScanActive;
            }
        }

        public bool IsCompleted
        {
            get
            {
                return processController != null &&
                       processController.IsScanCompleted;
            }
        }

        public bool IsResultShown
        {
            get
            {
                return resultShown;
            }
        }

        public float Progress
        {
            get
            {
                if (processController == null)
                {
                    return 0f;
                }

                return processController.ScanProgress;
            }
        }

        protected override SparkToolType GetToolType()
        {
            return SparkToolType.Scan;
        }

        protected override void Awake()
        {
            base.Awake();
            ResolveReferences();
        }

        private void OnDisable()
        {
            if (processController != null &&
                processController.IsScanActive)
            {
                processController.CancelScanProcess();
            }

            activeTarget = null;
            scanStarted = false;
            resultShown = false;
        }

        private void Update()
        {
            if (!scanStarted ||
                processController == null)
            {
                return;
            }

            if (!processController.IsScanCompleted)
            {
                return;
            }

            if (resultShown)
            {
                return;
            }

            ShowScanResult();
        }

       private void ResolveReferences()
        {
            if (processController == null)
            {
                processController =
                    GetComponentInParent<
                        ScannerProcessController>();
            }

            if (componentScanController == null)
            {
                componentScanController =
                    GetComponentInParent<
                        ScannerComponentScanController>();
            }

            if (diagnosticPanelController == null)
            {
                diagnosticPanelController =
                    GetComponentInParent<
                        ScannerDiagnosticPanelController>();
            }
        }

        public override bool CanBegin(
            in SparkToolContext context,
            out string reason)
        {
            if (!base.CanBegin(context, out reason))
            {
                return false;
            }

            ResolveReferences();

            if (processController == null)
            {
                reason =
                    "ScannerProcessController is missing.";

                return false;
            }

            if (componentScanController == null)
            {
                reason =
                    "ScannerComponentScanController is missing.";

                return false;
            }

            if (!context.HasTarget)
            {
                reason = "No target hit.";
                return false;
            }

            ScannerComponentTarget target =
                FindScannerTarget(
                    context.TargetHit.Collider);

            if (target == null)
            {
                reason =
                    "No ScannerComponentTarget found on hit hierarchy.";

                return false;
            }

            if (processController.IsScanActive)
            {
                reason =
                    "Scanner is already active.";

                return false;
            }

            reason = null;
            return true;
        }

        protected override SparkResult OnBegin(
            SparkToolContext context)
        {
            ResolveReferences();

            ScannerComponentTarget target =
                FindScannerTarget(
                    context.TargetHit.Collider);

            if (target == null)
            {
                return SparkResult.Invalid(
                    "ScannerComponentTarget not found.");
            }

            if (processController == null)
            {
                return SparkResult.Unavailable(
                    "ScannerProcessController missing.");
            }

            if (componentScanController == null)
            {
                return SparkResult.Unavailable(
                    "ScannerComponentScanController missing.");
            }

            activeTarget = target;
            scanStarted = false;
            resultShown = false;

            processController.StartScanProcess();

            processController.CompleteAcquire();

            scanStarted = true;

            return SparkResult.Success(
                "Scanner started.");
        }

        protected override SparkResult OnUpdate(
            SparkToolContext context)
        {
            if (!scanStarted)
            {
                return SparkResult.Unavailable(
                    "Scanner has not started.");
            }

            if (processController == null)
            {
                return SparkResult.Unavailable(
                    "ScannerProcessController missing.");
            }

            if (activeTarget == null)
            {
                processController.CancelScanProcess();

                scanStarted = false;

                return SparkResult.Invalid(
                    "Active scan target is missing.");
            }

            if (cancelWhenTargetChanges &&
                context.HasTarget)
            {
                ScannerComponentTarget currentTarget =
                    FindScannerTarget(
                        context.TargetHit.Collider);

                if (currentTarget != activeTarget)
                {
                    processController.CancelScanProcess();

                    activeTarget = null;
                    scanStarted = false;
                    resultShown = false;

                    return SparkResult.Cancelled(
                        "Scanner target changed.");
                }
            }

            if (processController.IsScanCompleted)
            {
                if (!resultShown)
                {
                    ShowScanResult();
                }

                return SparkResult.Success(
                    "Scan completed.");
            }

            if (!processController.IsScanActive)
            {
                return SparkResult.Unavailable(
                    "Scanner process is not active.");
            }

            return SparkResult.Success();
        }

        protected override SparkResult OnEnd(
            SparkToolContext context)
        {
            if (!scanStarted)
            {
                activeTarget = null;
                return SparkResult.Success();
            }

            if (processController == null)
            {
                activeTarget = null;
                scanStarted = false;
                resultShown = false;

                return SparkResult.Unavailable(
                    "ScannerProcessController missing.");
            }

            if (processController.IsScanCompleted)
            {
                ShowScanResult();

                activeTarget = null;
                scanStarted = false;

                if (retainCompletedScan)
                {
                    return SparkResult.Success(
                        "Scan completed. Result shown.");
                }

                processController.ResetVisualSystems();

                resultShown = false;

                return SparkResult.Success(
                    "Scan completed and cleared.");
            }

            processController.CancelScanProcess();

            activeTarget = null;
            scanStarted = false;
            resultShown = false;

            return SparkResult.Cancelled(
                "Scan released before completion.");
        }

        protected override SparkResult OnCancel(
            SparkToolContext context)
        {
            if (processController != null &&
                processController.IsScanActive)
            {
                processController.CancelScanProcess();
            }

            activeTarget = null;
            scanStarted = false;
            resultShown = false;

            return SparkResult.Cancelled(
                "Scan cancelled.");
        }

        public SparkResult CompleteCurrentScan()
        {
            if (processController == null)
            {
                return SparkResult.Unavailable(
                    "ScannerProcessController missing.");
            }

            if (!scanStarted ||
                activeTarget == null)
            {
                return SparkResult.Unavailable(
                    "No active scan.");
            }

            if (processController.IsScanCompleted)
            {
                ShowScanResult();

                return SparkResult.Success(
                    "Scan already completed.");
            }

            processController.CompleteScan();

            ShowScanResult();

            return SparkResult.Success(
                "Scan completed.");
        }

        public SparkResult CancelCurrentScan()
        {
            if (processController == null)
            {
                activeTarget = null;
                scanStarted = false;
                resultShown = false;

                return SparkResult.Unavailable(
                    "ScannerProcessController missing.");
            }

            if (!scanStarted)
            {
                return SparkResult.Unavailable(
                    "No active scan.");
            }

            processController.CancelScanProcess();

            activeTarget = null;
            scanStarted = false;
            resultShown = false;

            return SparkResult.Cancelled(
                "Scan cancelled.");
        }

        public void ResetScanner()
        {
            if (processController != null)
            {
                processController.ResetVisualSystems();
            }

            activeTarget = null;
            scanStarted = false;
            resultShown = false;
        }

        private void ShowScanResult()
{
    if (resultShown)
    {
        return;
    }

    if (processController == null)
    {
        return;
    }

    if (!processController.IsScanCompleted)
    {
        return;
    }

    if (activeTarget == null)
    {
        return;
    }

    if (diagnosticPanelController == null)
    {
        Debug.LogError(
            "SparkScanTool: " +
            "ScannerDiagnosticPanelController is not assigned.",
            this);

        return;
    }

    ScannerDiagnosticData data =
        activeTarget.CreateDiagnosticData();

    diagnosticPanelController.ShowComponent(
        activeTarget,
        data);

    resultShown = true;
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
    }
}