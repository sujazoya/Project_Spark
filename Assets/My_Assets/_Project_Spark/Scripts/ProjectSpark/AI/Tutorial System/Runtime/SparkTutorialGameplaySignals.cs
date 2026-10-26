using UnityEngine;
using ProjectSpark.Circuit;

namespace ProjectSpark.AI.Tutorial
{
    /// <summary>
    /// Drop-in bridge for connection events. Other gameplay systems can call the public
    /// report methods on successful authoritative actions (never on mere mouse clicks).
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkTutorialGameplaySignals : MonoBehaviour
    {
        [SerializeField] private SparkTutorialDirector director;
        [SerializeField] private SparkCircuitSystem circuit;
        private void Awake()
        {
            if (director == null) director = FindFirstObjectByType<SparkTutorialDirector>();
            if (circuit == null) circuit = FindFirstObjectByType<SparkCircuitSystem>();
        }
        private void OnEnable()
        {
            if (circuit == null) circuit = FindFirstObjectByType<SparkCircuitSystem>();
            if (circuit != null)
            {
                circuit.ConnectionCreated += OnConnectionCreated;
                circuit.ConnectionRemoved += OnConnectionRemoved;
            }
        }
        private void OnDisable()
        {
            if (circuit != null)
            {
                circuit.ConnectionCreated -= OnConnectionCreated;
                circuit.ConnectionRemoved -= OnConnectionRemoved;
            }
        }
        private void OnConnectionCreated(SparkCircuitConnection _) => Signal("circuit.connection_created");
        private void OnConnectionRemoved(SparkCircuitConnection _) => Signal("circuit.connection_removed");
        public void ReportShelfItemSelected() => Signal("shelf.item_selected");
        public void ReportShelfItemPlaced() => Signal("shelf.item_placed");
        public void ReportShelfItemReturned() => Signal("shelf.item_returned");
        public void ReportSwitchToggled() => Signal("switch.toggled");
        public void ReportLevelCompleted() => Signal("level.completed");
        public void ReportMeasurementTaken() => Signal("measurement.taken");
        public void Signal(string name)
        {
            if (director == null) director = FindFirstObjectByType<SparkTutorialDirector>();
            director?.Signal(name);
        }
    }
}
