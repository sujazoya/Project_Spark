using System;
using UnityEngine;

namespace ProjectSpark.Scanner
{
    /// <summary>
    /// Coordinates the complete holographic scanner process.
    ///
    /// Process:
    ///
    /// Acquire
    ///     ->
    /// Scan
    ///     ->
    /// Analyze
    ///     ->
    /// Result
    ///
    /// This controller owns PROCESS STATE and SCAN TIMING.
    ///
    /// It does not generate simulation data.
    /// It does not determine faults.
    /// It does not determine voltage.
    /// It does not determine topology.
    ///
    /// Those values must come from the real simulation and
    /// diagnostic systems.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ScannerProcessController
        : MonoBehaviour
    {
        [Header("State Transition")]
        [SerializeField]
        private ScannerStateTransitionController
            stateController;

        [Header("Effect 05")]
        [SerializeField]
        private ScannerCircuitTraceController
            circuitTraceController;

        [Header("Effect 07")]
        [SerializeField]
        private ScannerComponentScanController
            componentScanController;

        [Header("Effect 10")]
        [SerializeField]
        private ScannerTopologyController
            topologyController;

        [Header("Effect 11")]
        [SerializeField]
        private ScannerCircuitFlowController
            flowController;

        [Header("Effect 12")]
        [SerializeField]
        private ScannerSignalPulseController
            pulseController;

        [Header("Effect 13")]
        [SerializeField]
        private ScannerCircuitVoltageController
            voltageController;

        [Header("Effect 14")]
        [SerializeField]
        private ScannerFaultLocalizationController
            faultController;

        [Header("Effect 15")]
        [SerializeField]
        private ScannerFaultEnergyController
            faultEnergyController;

        [Header("Effect 16")]
        [SerializeField]
        private ScannerDiagnosticPanelController
            diagnosticPanelController;

        [Header("Initial State")]
        [SerializeField]
        private ScannerState initialState =
            ScannerState.Acquire;

        [Header("Scan Process")]
        [SerializeField, Min(0.01f)]
        private float scanDuration = 1.5f;

        [SerializeField]
        private bool useUnscaledTime = true;

        [Header("Process Behaviour")]
        [SerializeField]
        private bool resetOnStart = true;

        [SerializeField]
        private bool resetOnCancel = true;

        private ScannerState currentState;

        private bool running;

        private bool scanActive;

        private bool scanCompleted;

        private float scanStartTime;

        private float scanProgress;

        public event Action<ScannerState> StateChanged;

        public event Action<float> ScanProgressChanged;

        public event Action ScanCompleted;

        public event Action<bool> AnalysisCompleted;

        public ScannerState CurrentState
        {
            get
            {
                return currentState;
            }
        }

        public bool IsRunning
        {
            get
            {
                return running;
            }
        }

        public bool IsScanActive
        {
            get
            {
                return scanActive;
            }
        }

        public bool IsScanCompleted
        {
            get
            {
                return scanCompleted;
            }
        }

        public float ScanProgress
        {
            get
            {
                return scanProgress;
            }
        }

        public float ScanRemainingTime
        {
            get
            {
                if (!scanActive)
                {
                    return 0f;
                }

                return Mathf.Max(
                    0f,
                    scanDuration *
                    (1f - scanProgress));
            }
        }

        // ==============================================================
        // UNITY
        // ==============================================================

        private void Awake()
        {
            currentState =
                initialState;

            running = false;

            scanActive = false;

            scanCompleted = false;

            scanProgress = 0f;

            ResetVisualSystems();

            if (stateController != null)
            {
                stateController.SetStateImmediate(
                    initialState);
            }
        }

        private void Update()
        {
            if (!scanActive)
            {
                return;
            }

            UpdateScanProcess();
        }

        private void OnDisable()
        {
            if (scanActive)
            {
                CancelScanProcess();
            }
        }

        // ==============================================================
        // COMPLETE PROCESS
        // ==============================================================

        public void StartScanProcess()
        {
            if (resetOnStart)
            {
                ResetVisualSystems();
            }

            running = true;

            scanActive = false;

            scanCompleted = false;

            scanProgress = 0f;

            EnterState(
                ScannerState.Acquire);
        }

        public void CancelScanProcess()
        {
            if (!running &&
                !scanActive)
            {
                return;
            }

            scanActive = false;

            scanCompleted = false;

            scanProgress = 0f;

            if (resetOnCancel)
            {
                ResetVisualSystems();
            }

            running = false;

            EnterState(
                ScannerState.Acquire);
        }

        public void EndProcess()
        {
            scanActive = false;

            running = false;

            scanCompleted = false;

            scanProgress = 0f;
        }

        // ==============================================================
        // ACQUIRE
        // ==============================================================

        public void BeginAcquire()
        {
            running = true;

            scanActive = false;

            scanCompleted = false;

            scanProgress = 0f;

            EnterState(
                ScannerState.Acquire);
        }

        public void CompleteAcquire()
        {
            if (!running)
            {
                running = true;
            }

            BeginScan();
        }

        // ==============================================================
        // SCAN
        // ==============================================================

        public void BeginScan()
        {
            if (!running)
            {
                running = true;
            }

            scanActive = true;

            scanCompleted = false;

            scanProgress = 0f;

            scanStartTime =
                GetCurrentTime();

            EnterState(
                ScannerState.Scan);

            StartScanVisuals();

            ApplyScanProgress(
                0f);
        }

        private void UpdateScanProcess()
        {
            if (!scanActive)
            {
                return;
            }

            float elapsed =
                GetCurrentTime() -
                scanStartTime;

            if (scanDuration <= 0f)
            {
                CompleteScan();
                return;
            }

            float progress =
                Mathf.Clamp01(
                    elapsed /
                    scanDuration);

            SetScanProgress(
                progress);

            if (progress >= 1f)
            {
                CompleteScan();
            }
        }

        public void SetScanProgress(
            float progress)
        {
            if (!running ||
                !scanActive)
            {
                return;
            }

            progress =
                Mathf.Clamp01(
                    progress);

            ApplyScanProgress(
                progress);
        }

        private void ApplyScanProgress(
            float progress)
        {
            scanProgress =
                Mathf.Clamp01(
                    progress);

            if (circuitTraceController != null)
            {
                circuitTraceController.SetProgress(
                    scanProgress);
            }

            if (componentScanController != null)
            {
                componentScanController.SetProgress(
                    scanProgress);
            }

            if (topologyController != null)
            {
                topologyController.SetProgress(
                    scanProgress);
            }

            ScanProgressChanged?.Invoke(
                scanProgress);
        }

        public void CompleteScan()
        {
            if (!running)
            {
                return;
            }

            if (scanCompleted)
            {
                return;
            }

            scanActive = false;

            scanCompleted = true;

            scanProgress = 1f;

            if (circuitTraceController != null)
            {
                circuitTraceController.CompleteTrace();
            }

            if (componentScanController != null)
            {
                componentScanController.CompleteScan();
            }

            if (topologyController != null)
            {
                topologyController.CompleteTopology();
            }

            ScanProgressChanged?.Invoke(
                1f);

            ScanCompleted?.Invoke();

            EnterState(
                ScannerState.Analyze);
        }

        // ==============================================================
        // ANALYZE
        // ==============================================================

        public void BeginAnalyze()
        {
            if (!running)
            {
                running = true;
            }

            scanActive = false;

            EnterState(
                ScannerState.Analyze);

            EnterAnalyzeVisuals();
        }

        /// <summary>
        /// Called by the REAL simulation/diagnostic system
        /// after analysis has finished.
        ///
        /// No fault is generated here.
        /// </summary>
        public void CompleteAnalysis(
            bool faultDetected)
        {
            if (!running)
            {
                return;
            }

            scanActive = false;

            EnterState(
                ScannerState.Result);

            AnalysisCompleted?.Invoke(
                faultDetected);

            if (!faultDetected)
            {
                HideDiagnosticPanel();
            }
        }

        // ==============================================================
        // RESULT
        // ==============================================================

        public void ShowResult()
        {
            if (!running)
            {
                running = true;
            }

            scanActive = false;

            EnterState(
                ScannerState.Result);
        }

        // ==============================================================
        // STATE
        // ==============================================================

        private void EnterState(
            ScannerState newState)
        {
            if (currentState ==
                newState)
            {
                return;
            }

            ScannerState previousState =
                currentState;

            OnStateExited(
                previousState);

            currentState =
                newState;

            if (stateController != null)
            {
                stateController.TransitionTo(
                    newState);
            }

            OnStateEntered(
                newState);

            StateChanged?.Invoke(
                newState);
        }

        private void OnStateEntered(
            ScannerState state)
        {
            switch (state)
            {
                case ScannerState.Acquire:
                    EnterAcquireVisuals();
                    break;

                case ScannerState.Scan:
                    EnterScanVisuals();
                    break;

                case ScannerState.Analyze:
                    EnterAnalyzeVisuals();
                    break;

                case ScannerState.Result:
                    EnterResultVisuals();
                    break;
            }
        }

        private void OnStateExited(
            ScannerState state)
        {
            switch (state)
            {
                case ScannerState.Acquire:
                    ExitAcquireVisuals();
                    break;

                case ScannerState.Scan:
                    ExitScanVisuals();
                    break;

                case ScannerState.Analyze:
                    ExitAnalyzeVisuals();
                    break;

                case ScannerState.Result:
                    ExitResultVisuals();
                    break;
            }
        }

        // ==============================================================
        // ACQUIRE VISUALS
        // ==============================================================

        private void EnterAcquireVisuals()
        {
        }

        private void ExitAcquireVisuals()
        {
        }

        // ==============================================================
        // SCAN VISUALS
        // ==============================================================

        private void StartScanVisuals()
        {
            if (circuitTraceController != null)
            {
                circuitTraceController.StartTrace();
            }

            if (componentScanController != null)
            {
                componentScanController.ResetScan();
            }

            if (topologyController != null)
            {
                topologyController.ResetTopology();
            }

            if (topologyController != null)
            {
                topologyController.RefreshRuntimePaths();
            }
        }

        private void EnterScanVisuals()
        {
            StartScanVisuals();
        }

        private void ExitScanVisuals()
        {
            if (circuitTraceController != null)
            {
                circuitTraceController.StopTrace();
            }
        }

        // ==============================================================
        // ANALYZE VISUALS
        // ==============================================================

        private void EnterAnalyzeVisuals()
        {
            /*
             * Analysis effects are driven by real simulation data.
             */

            if (topologyController != null)
            {
                topologyController.RefreshRuntimePaths();
            }

            if (flowController != null)
            {
                flowController.RefreshRuntimePaths();
            }

            if (pulseController != null)
            {
                pulseController.RefreshRuntimePaths();
            }

            if (voltageController != null)
            {
                voltageController.RefreshRuntimePaths();
            }

            if (faultController != null)
            {
                faultController.RefreshPaths();
            }
        }

        private void ExitAnalyzeVisuals()
        {
        }

        // ==============================================================
        // RESULT VISUALS
        // ==============================================================

        private void EnterResultVisuals()
        {
            /*
             * Result systems must be populated by actual
             * diagnostic/simulation data.
             */
        }

        private void ExitResultVisuals()
        {
        }

        // ==============================================================
        // RESET
        // ==============================================================

        public void ResetVisualSystems()
        {
            scanActive = false;

            scanCompleted = false;

            scanProgress = 0f;

            if (circuitTraceController != null)
            {
                circuitTraceController.StopTrace();
            }

            if (componentScanController != null)
            {
                componentScanController.ResetScan();
            }

            if (topologyController != null)
            {
                topologyController.ResetTopology();
            }

            if (flowController != null)
            {
                flowController.StopAllFlow();
            }

            if (pulseController != null)
            {
                pulseController.StopAllPulses();
            }

            if (voltageController != null)
            {
                voltageController.ClearAllVoltage();
            }

            if (faultController != null)
            {
                faultController.ClearAllFaults();
            }

            if (faultEnergyController != null)
            {
                faultEnergyController.ClearAll();
            }

            HideDiagnosticPanel();
        }

        private void HideDiagnosticPanel()
        {
            if (diagnosticPanelController != null)
            {
                diagnosticPanelController.Hide();
            }
        }

        // ==============================================================
        // TIME
        // ==============================================================

        private float GetCurrentTime()
        {
            if (useUnscaledTime)
            {
                return Time.unscaledTime;
            }

            return Time.time;
        }
    }
}