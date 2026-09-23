using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.Gameplay
{
    /// <summary>
    /// Presentation-only theme controller for the Project Spark level monitor.
    ///
    /// This component changes monitor visuals from the validation state.
    /// It does not modify gameplay, circuit topology, or electrical simulation.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkLevelMonitorTheme : MonoBehaviour
    {
        // ============================================================
        // STATUS
        // ============================================================

        [Header("Status")]
        [SerializeField]
        private Image statusIndicator;

        [SerializeField]
        private TMP_Text statusText;

        // ============================================================
        // FAULT
        // ============================================================

        [Header("Fault")]
        [SerializeField]
        private GameObject faultPanel;

        [SerializeField]
        private TMP_Text faultText;

        // ============================================================
        // PROGRESS
        // ============================================================

        [Header("Progress")]
        [SerializeField]
        private Image progressFill;

        // ============================================================
        // OPTIONAL ACCENT
        // ============================================================

        [Header("Optional Accent")]
        [SerializeField]
        private Image accentLine;

        // ============================================================
        // COLORS
        // ============================================================

        [Header("Colors")]
        [SerializeField]
        private Color runningColor =
            new Color(
                0.35f,
                0.85f,
                1f,
                1f);

        [SerializeField]
        private Color completedColor =
            new Color(
                0.35f,
                1f,
                0.55f,
                1f);

        [SerializeField]
        private Color wrongConnectionColor =
            new Color(
                1f,
                0.75f,
                0.25f,
                1f);

        [SerializeField]
        private Color shortCircuitColor =
            new Color(
                1f,
                0.3f,
                0.2f,
                1f);

        [SerializeField]
        private Color targetShortColor =
            new Color(
                1f,
                0.45f,
                0.25f,
                1f);

        [SerializeField]
        private Color overloadColor =
            new Color(
                1f,
                0.4f,
                0.2f,
                1f);

        [SerializeField]
        private Color solverFaultColor =
            new Color(
                1f,
                0.3f,
                0.3f,
                1f);

        [SerializeField]
        private Color configurationFaultColor =
            new Color(
                1f,
                0.55f,
                0.2f,
                1f);

        // ============================================================
        // APPLY SNAPSHOT
        // ============================================================

        /// <summary>
        /// Applies the complete visual state represented by a monitor
        /// snapshot.
        ///
        /// This is the normal runtime entry point.
        /// </summary>
        public void Apply(
            SparkLevelMonitorSnapshot snapshot)
        {
            Color stateColor =
                GetStateColor(
                    snapshot.state);

            // --------------------------------------------------------
            // STATUS
            // --------------------------------------------------------

            if (statusIndicator != null)
            {
                statusIndicator.color =
                    stateColor;
            }

            if (statusText != null)
            {
                statusText.text =
                    snapshot.stateText;

                statusText.color =
                    stateColor;
            }

            // --------------------------------------------------------
            // FAULT
            // --------------------------------------------------------

            if (faultPanel != null)
            {
                faultPanel.SetActive(
                    snapshot.faultActive);
            }

            if (faultText != null)
            {
                faultText.text =
                    snapshot.faultActive
                        ? snapshot.faultText
                        : "NONE";

                faultText.color =
                    stateColor;
            }

            // --------------------------------------------------------
            // PROGRESS
            // --------------------------------------------------------

            if (progressFill != null)
            {
                progressFill.color =
                    stateColor;
            }

            // --------------------------------------------------------
            // ACCENT
            // --------------------------------------------------------

            if (accentLine != null)
            {
                accentLine.color =
                    stateColor;
            }
        }

        // ============================================================
        // APPLY STATE
        // ============================================================

        /// <summary>
        /// Applies only a validation state.
        ///
        /// Intended primarily for editor/context-menu previews.
        /// </summary>
        public void ApplyState(
            SparkLevelValidationState state)
        {
            Color stateColor =
                GetStateColor(state);

            if (statusIndicator != null)
            {
                statusIndicator.color =
                    stateColor;
            }

            if (statusText != null)
            {
                statusText.color =
                    stateColor;

                statusText.text =
                    SparkLevelMonitorSnapshot
                        .GetStateText(state);
            }

            if (progressFill != null)
            {
                progressFill.color =
                    stateColor;
            }

            if (accentLine != null)
            {
                accentLine.color =
                    stateColor;
            }

            if (faultPanel != null)
            {
                faultPanel.SetActive(
                    IsFaultState(state));
            }

            if (faultText != null)
            {
                faultText.text =
                    IsFaultState(state)
                        ? SparkLevelMonitorSnapshot
                            .GetStateText(state)
                        : "NONE";

                faultText.color =
                    stateColor;
            }
        }

        // ============================================================
        // FAULT STATE
        // ============================================================

        private bool IsFaultState(
            SparkLevelValidationState state)
        {
            switch (state)
            {
                case SparkLevelValidationState.WrongConnection:
                case SparkLevelValidationState.ShortCircuit:
                case SparkLevelValidationState.TargetShort:
                case SparkLevelValidationState.Overload:
                case SparkLevelValidationState.SolverFault:
                case SparkLevelValidationState.InvalidConfiguration:
                    return true;

                default:
                    return false;
            }
        }

        // ============================================================
        // STATE COLOR
        // ============================================================

        private Color GetStateColor(
            SparkLevelValidationState state)
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

                case SparkLevelValidationState.Playing:
                default:
                    return runningColor;
            }
        }

        // ============================================================
        // EDITOR PREVIEWS
        // ============================================================

        [ContextMenu("Preview Running")]
        private void PreviewRunning()
        {
            ApplyState(
                SparkLevelValidationState.Playing);
        }

        [ContextMenu("Preview Complete")]
        private void PreviewComplete()
        {
            ApplyState(
                SparkLevelValidationState.Completed);
        }

        [ContextMenu("Preview Wrong Connection")]
        private void PreviewWrongConnection()
        {
            ApplyState(
                SparkLevelValidationState.WrongConnection);
        }

        [ContextMenu("Preview Short Circuit")]
        private void PreviewShortCircuit()
        {
            ApplyState(
                SparkLevelValidationState.ShortCircuit);
        }

        [ContextMenu("Preview Target Short")]
        private void PreviewTargetShort()
        {
            ApplyState(
                SparkLevelValidationState.TargetShort);
        }

        [ContextMenu("Preview Overload")]
        private void PreviewOverload()
        {
            ApplyState(
                SparkLevelValidationState.Overload);
        }

        [ContextMenu("Preview Solver Fault")]
        private void PreviewSolverFault()
        {
            ApplyState(
                SparkLevelValidationState.SolverFault);
        }

        [ContextMenu("Preview Configuration Fault")]
        private void PreviewConfigurationFault()
        {
            ApplyState(
                SparkLevelValidationState.InvalidConfiguration);
        }
    }
}
