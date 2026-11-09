using UnityEngine;
using System.Collections.Generic;
using ProjectSpark.Circuit;
using ProjectSpark.Electrical;

namespace ProjectSpark.Gameplay
{
    /// <summary>
    /// Level-27-only DC transient scheduler. Uses the existing capacitor
    /// solver definition and shared SolveNow API; no shared solver changes.
    /// No AC waveform or ripple measurement is claimed.
    /// Place on the ORIGINAL Level_27_Layout prefab root.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkLevel27ReservoirDriver : MonoBehaviour
    {
        [SerializeField, Min(0.02f)] private float updateInterval = 0.05f;
        [SerializeField, Min(0.5f)] private float simulationWindowSeconds = 5f;
        [SerializeField] private SparkElectricalSolver solver;
        [SerializeField] private SparkCircuitSystem circuit;

        private float runUntil;
        private float nextSolve;
        private bool subscribed;
        private readonly List<SparkElectricalComponent> watched = new List<SparkElectricalComponent>();

        private void OnEnable()
        {
            Resolve();
            Subscribe();
            WatchLevelComponents();
            RestartWindow();
        }

        private void Start()
        {
            Resolve();
            Subscribe();
            WatchLevelComponents();
            RestartWindow();
        }

        private void OnDisable()
        {
            if (subscribed && circuit != null)
                circuit.TopologyChanged -= RestartWindow;
            subscribed = false;
            foreach (var item in watched)
                if (item != null) item.ElectricalConfigurationChanged -= RestartWindow;
            watched.Clear();
        }

        private void Resolve()
        {
            if (solver == null)
                solver = FindFirstObjectByType<SparkElectricalSolver>();
            if (circuit == null)
                circuit = FindFirstObjectByType<SparkCircuitSystem>();
        }

        private void Subscribe()
        {
            if (subscribed || circuit == null) return;
            circuit.TopologyChanged += RestartWindow;
            subscribed = true;
        }

        private void WatchLevelComponents()
        {
            foreach (var item in GetComponentsInChildren<SparkElectricalComponent>(true))
            {
                if (item == null || watched.Contains(item)) continue;
                item.ElectricalConfigurationChanged += RestartWindow;
                watched.Add(item);
            }
        }

        [ContextMenu("Project Spark / Restart Level 27 Reservoir Solve")]
        private void RestartWindow()
        {
            runUntil = Time.unscaledTime + simulationWindowSeconds;
            nextSolve = Time.unscaledTime;
        }

        private void Update()
        {
            if (Time.unscaledTime > runUntil || Time.unscaledTime < nextSolve)
                return;
            if (solver == null || circuit == null)
            {
                Resolve();
                Subscribe();
                if (solver == null || circuit == null) return;
            }
            if (!solver.isActiveAndEnabled || solver.IsSolving) return;
            nextSolve = Time.unscaledTime + updateInterval;
            solver.MarkDirty();
            solver.SolveNow();
        }
    }
}
