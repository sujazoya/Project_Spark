namespace ProjectSpark.UI.Feedback
{
    /// <summary>
    /// Central UI-facing integration state.
    ///
    /// This class contains no Unity lifecycle logic.
    /// It can be created and owned by UIManager.
    /// </summary>
    public sealed class UIIntegrationService
    {
        public UIEventHub Events
        {
            get;
        }

        public UIStateStore State
        {
            get;
        }

        public UIIntegrationService()
        {
            Events = new UIEventHub();

            State = new UIStateStore();

            SubscribeEvents();
        }

        private void SubscribeEvents()
        {
            Events.ObjectiveUpdated +=
                State.SetObjective;

            Events.ToolChanged +=
                State.SetTool;

            Events.DiagnosticsUpdated +=
                State.SetDiagnostics;

            Events.SimulationUpdated +=
                State.SetSimulation;

            Events.LevelStarted +=
                State.SetLevel;
            Events.ProcessChanged +=
           State.SetProcess;
        }
    }
}