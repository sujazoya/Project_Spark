using TMPro;
using ProjectSpark.UI.Core;
using ProjectSpark.UI.Feedback;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.UI.Gameplay
{
    public sealed class DiagnosticsPanelUI :
        MonoBehaviour
    {
        [SerializeField]
        private TMP_Text statusText;

        [SerializeField]
        private TMP_Text messageText;

        [SerializeField]
        private TMP_Text targetText;

        [SerializeField]
        private TMP_Text confidenceText;

        [SerializeField]
        private Slider progressSlider;
        [SerializeField]
        private UIFeedbackService feedback;

        public UIFeedbackService Feedback =>
            feedback;

        private void OnEnable()
        {
            if (UIManager.Instance == null)
            {
                return;
            }

            UIManager.Instance.State
                .DiagnosticsChanged +=
                    HandleDiagnosticsChanged;
        }

        private void OnDisable()
        {
            if (UIManager.Instance == null)
            {
                return;
            }

            UIManager.Instance.State
                .DiagnosticsChanged -=
                    HandleDiagnosticsChanged;
        }

        private void HandleDiagnosticsChanged(
            UIDiagnosticsState state)
        {
            if (statusText != null)
            {
                statusText.text =
                    state.Status
                        .ToString()
                        .ToUpperInvariant();
            }

            if (messageText != null)
            {
                messageText.text =
                    state.Message;
            }

            if (targetText != null)
            {
                targetText.text =
                    string.IsNullOrWhiteSpace(
                        state.ComponentId)
                        ? "--"
                        : state.ComponentId;
            }

            if (confidenceText != null)
            {
                confidenceText.text =
                    $"{state.Confidence * 100f:0}%";
            }

            if (progressSlider != null)
            {
                progressSlider.value =
                    Mathf.Clamp01(
                        state.Progress);
            }
        }
    }
}