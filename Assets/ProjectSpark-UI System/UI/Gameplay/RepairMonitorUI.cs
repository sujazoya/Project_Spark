using TMPro;
using ProjectSpark.UI.Core;
using ProjectSpark.UI.Feedback;
using UnityEngine;

namespace ProjectSpark.UI.Gameplay
{
    public sealed class RepairMonitorUI :
        MonoBehaviour
    {
        [Header("System")]

        [SerializeField]
        private TMP_Text systemStatusText;

        [SerializeField]
        private TMP_Text simulationStatusText;

        [Header("Process")]

        [SerializeField]
        private TMP_Text processText;

        [SerializeField]
        private TMP_Text objectiveText;

        [SerializeField]
        private TMP_Text progressText;

        [SerializeField]
        private UnityEngine.UI.Slider progressSlider;

        [Header("Tool")]

        [SerializeField]
        private TMP_Text activeToolText;

        [Header("Message")]

        [SerializeField]
        private TMP_Text systemMessageText;

        private void OnEnable()
        {
            if (UIManager.Instance == null)
            {
                return;
            }

            UIManager.Instance.State
                .ObjectiveChanged +=
                    HandleObjectiveChanged;

            UIManager.Instance.State
                .ToolChanged +=
                    HandleToolChanged;

            UIManager.Instance.State
                .DiagnosticsChanged +=
                    HandleDiagnosticsChanged;

            UIManager.Instance.State
                .SimulationChanged +=
                    HandleSimulationChanged;
        }

        private void OnDisable()
        {
            if (UIManager.Instance == null)
            {
                return;
            }

            UIManager.Instance.State
                .ObjectiveChanged -=
                    HandleObjectiveChanged;

            UIManager.Instance.State
                .ToolChanged -=
                    HandleToolChanged;

            UIManager.Instance.State
                .DiagnosticsChanged -=
                    HandleDiagnosticsChanged;

            UIManager.Instance.State
                .SimulationChanged -=
                    HandleSimulationChanged;
        }

        private void HandleObjectiveChanged(
            UIObjectiveState state)
        {
            if (objectiveText != null)
            {
                objectiveText.text =
                    state.Title;
            }

            float progress =
                Mathf.Clamp01(
                    state.Progress);

            if (progressText != null)
            {
                progressText.text =
                    $"{progress * 100f:0}%";
            }

            if (progressSlider != null)
            {
                progressSlider.value =
                    progress;
            }
        }

        private void HandleToolChanged(
            UIToolState state)
        {
            if (activeToolText != null)
            {
                activeToolText.text =
                    state.DisplayName;
            }
        }

        private void HandleDiagnosticsChanged(
            UIDiagnosticsState state)
        {
            if (state.Status ==
                UIDiagnosticsStatus.Complete)
            {
                if (systemMessageText != null)
                {
                    systemMessageText.text =
                        state.Message;
                }

                if (processText != null)
                {
                    processText.text =
                        "DIAGNOSTICS COMPLETE";
                }
            }
            else
            {
                if (processText != null)
                {
                    processText.text =
                        state.Status
                            .ToString()
                            .ToUpperInvariant();
                }
            }
        }

        private void HandleSimulationChanged(
            UISimulationState state)
        {
            if (simulationStatusText != null)
            {
                simulationStatusText.text =
                    state.Status
                        .ToString()
                        .ToUpperInvariant();
            }

            if (systemStatusText != null)
            {
                systemStatusText.text =
                    GetSystemStatus(
                        state.Status);
            }

            if (systemMessageText != null &&
                !string.IsNullOrWhiteSpace(
                    state.Message))
            {
                systemMessageText.text =
                    state.Message;
            }
        }

        private string GetSystemStatus(
            UISimulationStatus status)
        {
            switch (status)
            {
                case UISimulationStatus.Running:
                case UISimulationStatus.Complete:
                    return "ONLINE";

                case UISimulationStatus.Fault:
                    return "FAULT";

                case UISimulationStatus.Initializing:
                    return "INITIALIZING";

                case UISimulationStatus.Paused:
                    return "PAUSED";

                default:
                    return "OFFLINE";
            }
        }
    }
}