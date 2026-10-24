using System.Collections.Generic;
using UnityEngine;
using ProjectSpark.Gameplay;

namespace ProjectSpark.Electrical
{
    /// <summary>Publishes solver-calculated component currents to feedback components.
    /// Does not modify terminal currents or numerical solver state.</summary>
    [DisallowMultipleComponent]
    public sealed class SparkCurrentFeedbackBridge : MonoBehaviour
    {
        [SerializeField] private SparkElectricalSolver solver;
        [SerializeField] private bool clearOnDirty = true;

        private readonly HashSet<SparkComponentCurrentFeedback> published =
            new HashSet<SparkComponentCurrentFeedback>();
        private readonly HashSet<SparkComponentCurrentFeedback> currentPass =
            new HashSet<SparkComponentCurrentFeedback>();
        private readonly List<SparkComponentCurrentFeedback> removalBuffer =
            new List<SparkComponentCurrentFeedback>();

        private void OnEnable()
        {
            if (solver == null) solver = GetComponent<SparkElectricalSolver>();
            if (solver == null)
            {
                Debug.LogError("[SPARK CURRENT] Assign a SparkElectricalSolver.", this);
                return;
            }

            solver.SolveCompleted += OnSolveCompleted;
            solver.SolveFailed += OnSolveFailed;
            if (!solver.IsDirty && solver.LatestSolveResult != null)
                OnSolveCompleted();
        }

        private void OnDisable()
        {
            if (solver != null)
            {
                solver.SolveCompleted -= OnSolveCompleted;
                solver.SolveFailed -= OnSolveFailed;
            }
            ClearAll();
        }

        private void Update()
        {
            // A topology/configuration change may invalidate readings before the next solve.
            if (clearOnDirty && solver != null && solver.IsDirty && published.Count > 0)
                ClearAll();
        }

        private void OnSolveFailed() => ClearAll();

        private void OnSolveCompleted()
        {
            if (solver == null || solver.IsDirty || solver.LatestSolveResult == null)
            {
                ClearAll();
                return;
            }

            currentPass.Clear();
            IReadOnlyList<SparkElectricalComponent> components = solver.ElectricalComponents;
            if (components != null)
            {
                for (int i = 0; i < components.Count; i++)
                {
                    SparkElectricalComponent component = components[i];
                    if (component == null) continue;
                    SparkComponentCurrentFeedback feedback =
                        component.GetComponent<SparkComponentCurrentFeedback>();
                    if (feedback == null) continue;

                    currentPass.Add(feedback);
                    if (solver.TryCalculateComponentCurrent(component, out float amps) &&
                        !float.IsNaN(amps) && !float.IsInfinity(amps))
                    {
                        feedback.ApplyReading(amps);
                        published.Add(feedback);
                    }
                    else
                    {
                        feedback.ClearReading();
                        published.Remove(feedback);
                    }
                }
            }

            removalBuffer.Clear();
            foreach (SparkComponentCurrentFeedback old in published)
                if (old == null || !currentPass.Contains(old)) removalBuffer.Add(old);

            for (int i = 0; i < removalBuffer.Count; i++)
            {
                SparkComponentCurrentFeedback old = removalBuffer[i];
                if (old != null) old.ClearReading();
                published.Remove(old);
            }
            removalBuffer.Clear();
            currentPass.Clear();
        }

        public void ClearAll()
        {
            foreach (SparkComponentCurrentFeedback feedback in published)
                if (feedback != null) feedback.ClearReading();
            published.Clear();
            currentPass.Clear();
            removalBuffer.Clear();
        }
    }
}
