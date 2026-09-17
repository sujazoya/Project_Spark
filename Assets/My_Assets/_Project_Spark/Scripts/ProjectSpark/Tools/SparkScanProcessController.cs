using System;
using ProjectSpark.Gameplay;
using ProjectSpark.Scanner;
using UnityEngine;

namespace ProjectSpark.Gameplay
{
    public enum SparkScanProcessState
    {
        Idle,
        Acquiring,
        Scanning,
        Identified,
        Completed,
        Cancelled,
        Failed
    }

    public readonly struct SparkScanProcessResult
    {
        public bool Success { get; }
        public SparkScanProcessState State { get; }
        public ScannerComponentTarget Target { get; }
        public float Progress { get; }
        public string Message { get; }

        public SparkScanProcessResult(
            bool success,
            SparkScanProcessState state,
            ScannerComponentTarget target,
            float progress,
            string message)
        {
            Success = success;
            State = state;
            Target = target;
            Progress = progress;
            Message = message;
        }
    }

    [DisallowMultipleComponent]
    public sealed class SparkScanProcessController : MonoBehaviour
    {
        [Header("References")]

        [SerializeField]
        private ScannerComponentScanController scanner;

        [Header("Timing")]

        [SerializeField, Min(0.01f)]
        private float scanDuration = 1.5f;

        [SerializeField, Min(0f)]
        private float acquisitionDuration = 0.15f;

        [Header("Behaviour")]

        [SerializeField]
        private bool resetVisualsOnStart = true;

        [SerializeField]
        private bool resetVisualsOnCancel = true;

        [SerializeField]
        private bool keepCompletedScan = true;

        [SerializeField]
        private bool useUnscaledTime = true;

        private ScannerComponentTarget activeTarget;

        private SparkScanProcessState state =
            SparkScanProcessState.Idle;

        private float processStartTime;
        private float acquisitionStartTime;
        private float progress;

        private bool completed;
        private bool identified;

        public event Action<SparkScanProcessResult> ProcessChanged;

        public ScannerComponentTarget ActiveTarget
        {
            get
            {
                return activeTarget;
            }
        }

        public SparkScanProcessState State
        {
            get
            {
                return state;
            }
        }

        public float Progress
        {
            get
            {
                return progress;
            }
        }

        public bool IsActive
        {
            get
            {
                return state == SparkScanProcessState.Acquiring ||
                       state == SparkScanProcessState.Scanning;
            }
        }

        public bool IsIdentified
        {
            get
            {
                return identified;
            }
        }

        public bool IsCompleted
        {
            get
            {
                return completed;
            }
        }

        public bool HasTarget
        {
            get
            {
                return activeTarget != null;
            }
        }

        public float RemainingTime
        {
            get
            {
                if (!IsActive)
                {
                    return 0f;
                }

                return Mathf.Max(
                    0f,
                    scanDuration * (1f - progress));
            }
        }

        private void Awake()
        {
            if (scanner == null)
            {
                scanner =
                    GetComponentInParent<ScannerComponentScanController>();
            }

            ResetProcess();
        }

        private void Update()
        {
            if (state == SparkScanProcessState.Acquiring)
            {
                UpdateAcquisition();
                return;
            }

            if (state == SparkScanProcessState.Scanning)
            {
                UpdateScanning();
            }
        }

        public bool CanStart(
            ScannerComponentTarget target,
            out string reason)
        {
            if (scanner == null)
            {
                reason =
                    "ScannerComponentScanController is not configured.";

                return false;
            }

            if (target == null)
            {
                reason = "Scanner target is missing.";
                return false;
            }

            if (IsActive)
            {
                reason = "A scan process is already active.";
                return false;
            }

            reason = null;
            return true;
        }

        public SparkScanProcessResult StartScan(
            ScannerComponentTarget target)
        {
            if (!CanStart(target, out string reason))
            {
                return Fail(reason);
            }

            activeTarget = target;

            progress = 0f;
            identified = false;
            completed = false;

            processStartTime = GetTime();
            acquisitionStartTime = processStartTime;

            state = SparkScanProcessState.Acquiring;

            if (resetVisualsOnStart)
            {
                ResetTargetVisuals();
            }

            ApplyVisualProgress(0f);

            SparkScanProcessResult result =
                CreateResult(
                    true,
                    SparkScanProcessState.Acquiring,
                    "Scanner target acquired.");

            ProcessChanged?.Invoke(result);

            return result;
        }

        public SparkScanProcessResult UpdateProcess()
        {
            if (!IsActive)
            {
                return CreateResult(
                    false,
                    state,
                    "No active scan process.");
            }

            if (activeTarget == null)
            {
                return Fail(
                    "Active scanner target was destroyed.");
            }

            if (state == SparkScanProcessState.Acquiring)
            {
                UpdateAcquisition();
            }
            else if (state == SparkScanProcessState.Scanning)
            {
                UpdateScanning();
            }

            return CreateResult(
                true,
                state,
                null);
        }

        public SparkScanProcessResult CompleteScan()
        {
            if (activeTarget == null)
            {
                return Fail(
                    "Cannot complete scan without an active target.");
            }

            progress = 1f;
            identified = true;
            completed = true;

            ApplyVisualProgress(1f);

            state = SparkScanProcessState.Identified;

            SparkScanProcessResult identifiedResult =
                CreateResult(
                    true,
                    SparkScanProcessState.Identified,
                    "Component identified.");

            ProcessChanged?.Invoke(identifiedResult);

            state = SparkScanProcessState.Completed;

            SparkScanProcessResult completedResult =
                CreateResult(
                    true,
                    SparkScanProcessState.Completed,
                    "Component scan completed.");

            ProcessChanged?.Invoke(completedResult);

            return completedResult;
        }

        public SparkScanProcessResult CancelScan()
        {
            if (!IsActive)
            {
                return CreateResult(
                    false,
                    state,
                    "No active scan process to cancel.");
            }

            progress = 0f;
            identified = false;
            completed = false;

            if (resetVisualsOnCancel)
            {
                ResetTargetVisuals();
            }

            state = SparkScanProcessState.Cancelled;

            SparkScanProcessResult result =
                CreateResult(
                    false,
                    SparkScanProcessState.Cancelled,
                    "Component scan cancelled.");

            ProcessChanged?.Invoke(result);

            activeTarget = null;

            state = SparkScanProcessState.Idle;

            return result;
        }

        public SparkScanProcessResult EndScan()
        {
            if (state == SparkScanProcessState.Completed &&
                keepCompletedScan)
            {
                return CreateResult(
                    true,
                    SparkScanProcessState.Completed,
                    "Completed scan retained.");
            }

            if (state == SparkScanProcessState.Identified)
            {
                state = SparkScanProcessState.Completed;

                return CreateResult(
                    true,
                    SparkScanProcessState.Completed,
                    "Completed scan retained.");
            }

            if (!IsActive)
            {
                return CreateResult(
                    false,
                    state,
                    "No active scan process.");
            }

            return CancelScan();
        }

        public void ResetProcess()
        {
            if (activeTarget != null &&
                resetVisualsOnCancel)
            {
                ResetTargetVisuals();
            }

            activeTarget = null;

            progress = 0f;
            identified = false;
            completed = false;

            state = SparkScanProcessState.Idle;

            if (scanner != null)
            {
                scanner.ClearProjection();
            }
        }

        public void ResetCompletedScan()
        {
            if (!completed &&
                state != SparkScanProcessState.Completed)
            {
                return;
            }

            ResetProcess();
        }

        public bool IsTargetCompleted(
            ScannerComponentTarget target)
        {
            return target != null &&
                   activeTarget == target &&
                   completed;
        }

        private void UpdateAcquisition()
        {
            if (activeTarget == null)
            {
                Fail(
                    "Scanner target disappeared during acquisition.");

                return;
            }

            if (acquisitionDuration <= 0f)
            {
                BeginScanning();
                return;
            }

            float elapsed =
                GetTime() - acquisitionStartTime;

            if (elapsed >= acquisitionDuration)
            {
                BeginScanning();
            }
        }

        private void BeginScanning()
        {
            state = SparkScanProcessState.Scanning;

            processStartTime = GetTime();

            progress = 0f;

            ApplyVisualProgress(0f);

            SparkScanProcessResult result =
                CreateResult(
                    true,
                    SparkScanProcessState.Scanning,
                    "Component scanning started.");

            ProcessChanged?.Invoke(result);
        }

        private void UpdateScanning()
        {
            if (activeTarget == null)
            {
                Fail(
                    "Scanner target disappeared during scan.");

                return;
            }

            if (scanDuration <= 0f)
            {
                CompleteScan();
                return;
            }

            float elapsed =
                GetTime() - processStartTime;

            progress =
                Mathf.Clamp01(
                    elapsed / scanDuration);

            ApplyVisualProgress(progress);

            SparkScanProcessResult progressResult =
                CreateResult(
                    true,
                    SparkScanProcessState.Scanning,
                    null);

            ProcessChanged?.Invoke(progressResult);

            if (progress >= 1f)
            {
                CompleteScan();
            }
        }

        private void ApplyVisualProgress(
            float value)
        {
            if (scanner == null ||
                activeTarget == null)
            {
                return;
            }

            value = Mathf.Clamp01(value);

            scanner.SetProgress(
                activeTarget,
                value);

            if (value >= 1f)
            {
                scanner.SetProjectionProgress(1f);
                scanner.SetProjectionVisible(true);
            }
            else
            {
                scanner.SetProjectionProgress(value);
                scanner.SetProjectionVisible(value > 0.001f);
            }
        }

        private void ResetTargetVisuals()
        {
            if (scanner == null ||
                activeTarget == null)
            {
                return;
            }

            scanner.SetProgress(
                activeTarget,
                0f);

            scanner.SetInteractionProgress(
                activeTarget,
                0f);

            scanner.SetInteractionVisible(
                activeTarget,
                false);

            scanner.SetProjectionProgress(0f);
            scanner.SetProjectionVisible(false);
        }

        private SparkScanProcessResult Fail(
            string message)
        {
            progress = 0f;
            identified = false;
            completed = false;

            state = SparkScanProcessState.Failed;

            SparkScanProcessResult result =
                CreateResult(
                    false,
                    SparkScanProcessState.Failed,
                    message);

            ProcessChanged?.Invoke(result);

            return result;
        }

        private SparkScanProcessResult CreateResult(
            bool success,
            SparkScanProcessState resultState,
            string message)
        {
            return new SparkScanProcessResult(
                success,
                resultState,
                activeTarget,
                progress,
                message);
        }

        private float GetTime()
        {
            if (useUnscaledTime)
            {
                return Time.unscaledTime;
            }

            return Time.time;
        }

        private void OnDisable()
        {
            if (IsActive)
            {
                CancelScan();
            }
        }
    }
}