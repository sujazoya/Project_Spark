using System;

namespace ProjectSpark.UI.Feedback
{
    /// <summary>
    /// Stores the latest UI-facing state.
    ///
    /// This is a presentation cache.
    /// It is not the source of gameplay truth.
    /// </summary>
    public sealed class UIStateStore
    {
        public UIObjectiveState CurrentObjective
        {
            get;
            private set;
        }

        public UIToolState CurrentTool
        {
            get;
            private set;
        }

        public UIDiagnosticsState Diagnostics
        {
            get;
            private set;
        }

        public UIProcessState Process
        {
            get;
            private set;
        }

        public event Action<UIProcessState>
            ProcessChanged;

        public void SetProcess(
            UIProcessState state)
        {
            Process = state;

            ProcessChanged?.Invoke(
                state);
        }

        public UISimulationState Simulation
        {
            get;
            private set;
        }

        public UILevelState CurrentLevel
        {
            get;
            private set;
        }

        public event Action<UIObjectiveState>
            ObjectiveChanged;

        public event Action<UIToolState>
            ToolChanged;

        public event Action<UIDiagnosticsState>
            DiagnosticsChanged;

        public event Action<UISimulationState>
            SimulationChanged;

        public event Action<UILevelState>
            LevelChanged;

        public void SetObjective(
            UIObjectiveState state)
        {
            CurrentObjective = state;

            ObjectiveChanged?.Invoke(
                state);
        }

        public void SetTool(
            UIToolState state)
        {
            CurrentTool = state;

            ToolChanged?.Invoke(
                state);
        }

        public void SetDiagnostics(
            UIDiagnosticsState state)
        {
            Diagnostics = state;

            DiagnosticsChanged?.Invoke(
                state);
        }

        public void SetSimulation(
            UISimulationState state)
        {
            Simulation = state;

            SimulationChanged?.Invoke(
                state);
        }

        public void SetLevel(
            UILevelState state)
        {
            CurrentLevel = state;

            LevelChanged?.Invoke(
                state);
        }
    }
}