using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.Gameplay
{
    /// <summary>
    /// Presentation-only theme controller for the Project Spark level monitor.
    /// It changes monitor visuals from the validation state without changing
    /// gameplay, circuit topology, or electrical simulation.
    /// </summary>
    public class SparkLevelMonitorTheme : MonoBehaviour
    {
        [Header("Status")]
        [SerializeField] private Image statusIndicator;
        [SerializeField] private TMP_Text statusText;

        [Header("Fault")]
        [SerializeField] private GameObject faultPanel;
        [SerializeField] private TMP_Text faultText;

        [Header("Progress")]
        [SerializeField] private Image progressFill;

        [Header("Optional Accent")]
        [SerializeField] private Image accentLine;

        [Header("Colors")]
        [SerializeField] private Color runningColor = new Color(0.35f, 0.85f, 1f, 1f);
        [SerializeField] private Color completedColor = new Color(0.35f, 1f, 0.55f, 1f);
        [SerializeField] private Color wrongConnectionColor = new Color(1f, 0.75f, 0.25f, 1f);
        [SerializeField] private Color shortCircuitColor = new Color(1f, 0.3f, 0.2f, 1f);
        [SerializeField] private Color targetShortColor = new Color(1f, 0.45f, 0.25f, 1f);
        [SerializeField] private Color overloadColor = new Color(1f, 0.4f, 0.2f, 1f);
        [SerializeField] private Color solverFaultColor = new Color(1f, 0.3f, 0.3f, 1f);
        [SerializeField] private Color configurationFaultColor = new Color(1f, 0.55f, 0.2f, 1f);

        /// <summary>
        /// Applies the visual state represented by a monitor snapshot.
        /// </summary>
        public void Apply(SparkLevelMonitorSnapshot snapshot)
        {
            Color stateColor = GetStateColor(snapshot.state);

            if (statusIndicator != null)
                statusIndicator.color = stateColor;

            if (statusText != null)
            {
                statusText.text = snapshot.stateText;
                statusText.color = stateColor;
            }

            if (faultPanel != null)
                faultPanel.SetActive(snapshot.faultActive);

            if (faultText != null)
            {
                faultText.text = snapshot.faultActive
                    ? snapshot.faultText
                    : "NONE";

                faultText.color = stateColor;
            }

            if (progressFill != null)
                progressFill.color = stateColor;

            if (accentLine != null)
                accentLine.color = stateColor;
        }

        public void ApplyState(SparkLevelValidationState state)
        {
            Color stateColor = GetStateColor(state);

            if (statusIndicator != null)
                statusIndicator.color = stateColor;

            if (statusText != null)
                statusText.color = stateColor;

            if (progressFill != null)
                progressFill.color = stateColor;

            if (accentLine != null)
                accentLine.color = stateColor;

            if (faultPanel != null)
            {
                bool fault =
                    state == SparkLevelValidationState.WrongConnection ||
                    state == SparkLevelValidationState.ShortCircuit ||
                    state == SparkLevelValidationState.TargetShort ||
                    state == SparkLevelValidationState.Overload ||
                    state == SparkLevelValidationState.SolverFault ||
                    state == SparkLevelValidationState.InvalidConfiguration;

                faultPanel.SetActive(fault);
            }
        }

        private Color GetStateColor(SparkLevelValidationState state)
        {
            switch (state)
            {
                case SparkLevelValidationState.Completed:
                    return completedColor;

                case SparkLevelValidationState.WrongConnection:
                    return wrongConnectionColor;

                case SparkLevelValidationState.ShortCircuit:
                    return shortCircuitColor;

                case SparkLevelValidationState.TargetShort:
                    return targetShortColor;

                case SparkLevelValidationState.Overload:
                    return overloadColor;

                case SparkLevelValidationState.SolverFault:
                    return solverFaultColor;

                case SparkLevelValidationState.InvalidConfiguration:
                    return configurationFaultColor;

                default:
                    return runningColor;
            }
        }

        [ContextMenu("Preview Running")]
        private void PreviewRunning()
        {
            ApplyState(SparkLevelValidationState.Playing);
        }

        [ContextMenu("Preview Complete")]
        private void PreviewComplete()
        {
            ApplyState(SparkLevelValidationState.Completed);
        }

        [ContextMenu("Preview Wrong Connection")]
        private void PreviewWrongConnection()
        {
            ApplyState(SparkLevelValidationState.WrongConnection);
        }

        [ContextMenu("Preview Short Circuit")]
        private void PreviewShortCircuit()
        {
            ApplyState(SparkLevelValidationState.ShortCircuit);
        }

        [ContextMenu("Preview Target Short")]
        private void PreviewTargetShort()
        {
            ApplyState(SparkLevelValidationState.TargetShort);
        }
    }
}
