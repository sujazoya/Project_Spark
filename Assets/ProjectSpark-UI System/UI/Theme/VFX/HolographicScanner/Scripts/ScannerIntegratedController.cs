using UnityEngine;

namespace ProjectSpark.Scanner
{
    [DisallowMultipleComponent]
    public sealed class ScannerIntegratedController
        : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField]
        private ScannerIntegratedSettings settings;

        [Header("Process")]
        [SerializeField]
        private ScannerProcessController processController;

        [Header("State Transition")]
        [SerializeField]
        private ScannerStateTransitionController stateController;

        [Header("Runtime Systems")]
        [SerializeField]
        private ScannerTopologyController topologyController;

        [SerializeField]
        private ScannerCircuitFlowController flowController;

        [SerializeField]
        private ScannerSignalPulseController pulseController;

        [SerializeField]
        private ScannerCircuitVoltageController voltageController;

        [SerializeField]
        private ScannerFaultLocalizationController
            faultController;

        [SerializeField]
        private ScannerFaultEnergyController
            faultEnergyController;

        [SerializeField]
        private ScannerDiagnosticPanelController
            diagnosticPanelController;

        private ScannerState currentState;

        public ScannerState CurrentState =>
            currentState;

        // ==============================================================
        // UNITY
        // ==============================================================

        private void Awake()
        {
            currentState =
                ScannerState.Acquire;

            RefreshRuntimeSystems();

            HideAnalysisSystems();
            HideResultSystems();
        }

        private void OnDisable()
        {
            if (settings == null ||
                settings.HideAllEffectsOnDisable)
            {
                ResetAllVisuals();
            }
        }

        // ==============================================================
        // RUNTIME REFRESH
        // ==============================================================

        public void RefreshRuntimeSystems()
        {
            if (topologyController != null)
                topologyController.RefreshRuntimePaths();

            if (flowController != null)
                flowController.RefreshRuntimePaths();

            if (pulseController != null)
                pulseController.RefreshRuntimePaths();

            if (voltageController != null)
                voltageController.RefreshRuntimePaths();

            if (faultController != null)
                faultController.RefreshPaths();

        }

        // ==============================================================
        // ACQUIRE
        // ==============================================================

        public void EnterAcquire()
        {
            currentState =
                ScannerState.Acquire;

            HideScanSystems();
            HideAnalysisSystems();
            HideResultSystems();

            PlayStateTransition(
                ScannerState.Acquire);
        }

        // ==============================================================
        // SCAN
        // ==============================================================

        public void EnterScan()
        {
            currentState =
                ScannerState.Scan;

            HideAnalysisSystems();
            HideResultSystems();

            EnableScanSystems();

            PlayStateTransition(
                ScannerState.Scan);
        }

        // ==============================================================
        // ANALYZE
        // ==============================================================

        public void EnterAnalyze()
        {
            currentState =
                ScannerState.Analyze;

            DisableTraceAfterScan();

            EnableAnalysisSystems();

            HideResultSystems();

            PlayStateTransition(
                ScannerState.Analyze);
        }

        // ==============================================================
        // RESULT
        // ==============================================================

        public void EnterResult()
        {
            currentState =
                ScannerState.Result;

            HideScanSystems();

            EnableResultSystems();

            PlayStateTransition(
                ScannerState.Result);
        }

        // ==============================================================
        // SCAN SYSTEMS
        // ==============================================================

        private void EnableScanSystems()
        {
            /*
             * Effect 05/07/10 are started through the existing
             * process controller.
             *
             * Effect 08/09 are owned by their existing targets
             * and become active from component identification.
             */

            if (processController != null)
                processController.BeginScan();

            if (topologyController != null)
                topologyController.RefreshRuntimePaths();
        }

        private void HideScanSystems()
        {
            /*
             * Effect 05/07 are explicitly reset.
             *
             * Effect 08/09 are reset through their component
             * target state when the process is reset.
             */

            if (processController != null)
                processController.ResetVisualSystems();
        }

        private void DisableTraceAfterScan()
        {
            /*
             * The real circuit itself remains.
             *
             * Only the active scanner tracing animation is stopped.
             */
        }

        // ==============================================================
        // ANALYSIS SYSTEMS
        // ==============================================================

        private void EnableAnalysisSystems()
        {
            if (topologyController != null)
                topologyController.RefreshRuntimePaths();

            if (flowController != null)
                flowController.RefreshRuntimePaths();

            if (pulseController != null)
                pulseController.RefreshRuntimePaths();

            if (voltageController != null)
                voltageController.RefreshRuntimePaths();
        }

        private void HideAnalysisSystems()
        {
            if (flowController != null)
                flowController.StopAllFlow();

            if (pulseController != null)
                pulseController.StopAllPulses();

            if (voltageController != null)
                voltageController.ClearAllVoltage();
        }

        // ==============================================================
        // RESULT SYSTEMS
        // ==============================================================

        private void EnableResultSystems()
        {
            /*
             * Fault localization, fault energy and diagnostic
             * information are controlled by the real diagnostic
             * result.
             *
             * Do not fabricate a fault here.
             */
        }

        private void HideResultSystems()
        {
            if (faultController != null)
                faultController.ClearAllFaults();

            if (faultEnergyController != null)
                faultEnergyController.ClearAll();

            if (diagnosticPanelController != null)
                diagnosticPanelController.Hide();
        }

        // ==============================================================
        // TRANSITION
        // ==============================================================

        private void PlayStateTransition(
            ScannerState state)
        {
            if (stateController == null)
                return;

            stateController.SetStateImmediate(
                state);
        }

        // ==============================================================
        // RESET
        // ==============================================================

        public void ResetAllVisuals()
        {
            currentState =
                ScannerState.Acquire;

            if (processController != null)
                processController.ResetVisualSystems();

            if (flowController != null)
                flowController.StopAllFlow();

            if (pulseController != null)
                pulseController.StopAllPulses();

            if (voltageController != null)
                voltageController.ClearAllVoltage();

            if (faultController != null)
                faultController.ClearAllFaults();

            if (faultEnergyController != null)
                faultEnergyController.ClearAll();

            if (diagnosticPanelController != null)
                diagnosticPanelController.Hide();

            if (stateController != null)
            {
                stateController.SetStateImmediate(
                    ScannerState.Acquire);
            }
        }
    }
}