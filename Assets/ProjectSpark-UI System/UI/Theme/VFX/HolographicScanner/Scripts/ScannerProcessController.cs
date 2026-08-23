using UnityEngine;

namespace ProjectSpark.Scanner
{
    /// <summary>
    /// Coordinates the holographic scanner process.
    ///
    /// This class does not perform simulation.
    /// It only coordinates existing scanner visual systems.
    ///
    /// Simulation/gameplay code remains responsible for:
    /// - component identification
    /// - voltage
    /// - signal flow
    /// - topology
    /// - fault detection
    /// - diagnostic data
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

        private ScannerState currentState;

        private bool running;

        public ScannerState CurrentState =>
            currentState;

        public bool IsRunning =>
            running;

        // ==============================================================
        // UNITY
        // ==============================================================

        private void Awake()
        {
            currentState =
                initialState;

            ResetVisualSystems();

            if (stateController != null)
            {
                stateController.SetStateImmediate(
                    initialState);
            }
        }

        // ==============================================================
        // START
        // ==============================================================

        public void StartScanProcess()
        {
            running = true;

            ResetVisualSystems();

            EnterState(
                ScannerState.Acquire);
        }

        // ==============================================================
        // ACQUIRE
        // ==============================================================

        public void BeginAcquire()
        {
            running = true;

            EnterState(
                ScannerState.Acquire);
        }

        public void CompleteAcquire()
        {
            if (!running)
                return;

            EnterState(
                ScannerState.Scan);
        }

        // ==============================================================
        // SCAN
        // ==============================================================

        public void BeginScan()
        {
            if (!running)
                running = true;

            EnterState(
                ScannerState.Scan);

            if (circuitTraceController != null)
                circuitTraceController.StartTrace();

            if (componentScanController != null)
                componentScanController.ResetScan();

            if (topologyController != null)
                topologyController.ResetTopology();
        }

        public void SetScanProgress(
            float progress)
        {
            if (!running)
                return;

            if (circuitTraceController != null)
            {
                circuitTraceController.SetProgress(
                    progress);
            }

            if (componentScanController != null)
            {
                componentScanController.SetProgress(
                    progress);
            }

            if (topologyController != null)
            {
                topologyController.SetProgress(
                    progress);
            }
        }

        public void CompleteScan()
        {
            if (!running)
                return;

            if (circuitTraceController != null)
                circuitTraceController.CompleteTrace();

            if (componentScanController != null)
                componentScanController.CompleteScan();

            if (topologyController != null)
                topologyController.CompleteTopology();

            EnterState(
                ScannerState.Analyze);
        }

        // ==============================================================
        // ANALYZE
        // ==============================================================

        public void BeginAnalyze()
        {
            if (!running)
                running = true;

            EnterState(
                ScannerState.Analyze);

            /*
             * IMPORTANT:
             *
             * Effects 11, 12, 13 are driven by real simulation
             * data. We do not automatically invent values here.
             *
             * Their controllers are already available to gameplay
             * and simulation code.
             *
             * Effect 10 topology reconstruction has already been
             * completed by CompleteScan().
             */
        }

        /// <summary>
        /// Call this when the REAL simulation/diagnostic system
        /// has finished analysis.
        /// </summary>
        public void CompleteAnalysis(
            bool faultDetected)
        {
            if (!running)
                return;

            EnterState(
                ScannerState.Result);

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
                running = true;

            EnterState(
                ScannerState.Result);
        }

        public void EndProcess()
        {
            running = false;
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

            ScannerState previous =
                currentState;

            currentState =
                newState;

            if (stateController != null)
            {
                stateController.TransitionTo(
                    newState);
            }

            OnStateExited(previous);
            OnStateEntered(newState);
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
            /*
             * Target-lock/acquisition visuals are deliberately left
             * to the existing acquisition system.
             *
             * Effect 17 provides the transition itself.
             */
        }

        private void ExitAcquireVisuals()
        {
        }

        // ==============================================================
        // SCAN VISUALS
        // ==============================================================

        private void EnterScanVisuals()
        {
            if (circuitTraceController != null)
                circuitTraceController.StartTrace();

            if (componentScanController != null)
                componentScanController.ResetScan();

            if (topologyController != null)
                topologyController.ResetTopology();
        }

        private void ExitScanVisuals()
        {
            if (circuitTraceController != null)
                circuitTraceController.StopTrace();
        }

        // ==============================================================
        // ANALYZE VISUALS
        // ==============================================================

        private void EnterAnalyzeVisuals()
        {
            /*
             * Simulation-driven systems remain active here.
             *
             * Example:
             *
             * flowController.SetFlow(...)
             * pulseController.SendPulse(...)
             * voltageController.SetVoltage(...)
             *
             * These are called by the actual simulation, not
             * fabricated by this controller.
             */
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
             * Fault localization, fault energy and the diagnostic
             * panel are driven by the actual analysis result.
             *
             * This controller intentionally does not fabricate
             * a fault or diagnostic result.
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
                diagnosticPanelController.Hide();
        }
    }
}