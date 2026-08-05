using System;

namespace ProjectSpark.UI.Feedback
{
    /// <summary>
    /// Central event channel for UI-facing gameplay events.
    ///
    /// This class does not own gameplay state.
    /// It only broadcasts events intended for UI presentation.
    /// </summary>
    public sealed class UIEventHub
    {
        public event Action<UIObjectiveState> ObjectiveUpdated;

        public event Action<UIObjectiveState> ObjectiveCompleted;

        public event Action<UIObjectiveState> ObjectiveFailed;

        public event Action<UIToolState> ToolChanged;

        public event Action<UIDiagnosticsState> DiagnosticsUpdated;

        public event Action<UISimulationState> SimulationUpdated;

        public event Action<UILevelState> LevelStarted;

        public event Action<UILevelState> LevelCompleted;

        public event Action<UILevelState> LevelFailed;
        public event Action<UIProcessState> ProcessChanged;

        public void PublishProcessChanged(
            UIProcessState state)
        {
            ProcessChanged?.Invoke(state);
        }

        public void PublishObjectiveUpdated(
            UIObjectiveState state)
        {
            ObjectiveUpdated?.Invoke(state);
        }

        public void PublishObjectiveCompleted(
            UIObjectiveState state)
        {
            ObjectiveCompleted?.Invoke(state);
        }

        public void PublishObjectiveFailed(
            UIObjectiveState state)
        {
            ObjectiveFailed?.Invoke(state);
        }

        public void PublishToolChanged(
            UIToolState state)
        {
            ToolChanged?.Invoke(state);
        }

        public void PublishDiagnosticsUpdated(
            UIDiagnosticsState state)
        {
            DiagnosticsUpdated?.Invoke(state);
        }

        public void PublishSimulationUpdated(
            UISimulationState state)
        {
            SimulationUpdated?.Invoke(state);
        }

        public void PublishLevelStarted(
            UILevelState state)
        {
            LevelStarted?.Invoke(state);
        }

        public void PublishLevelCompleted(
            UILevelState state)
        {
            LevelCompleted?.Invoke(state);
        }

        public void PublishLevelFailed(
            UILevelState state)
        {
            LevelFailed?.Invoke(state);
        }
    }
}