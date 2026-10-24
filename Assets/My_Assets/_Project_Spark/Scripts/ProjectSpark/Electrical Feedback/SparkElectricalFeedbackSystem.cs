using System.Collections.Generic;
using UnityEngine;
using ProjectSpark.Gameplay;

namespace ProjectSpark.Electrical
{
    [DisallowMultipleComponent]
    public sealed class SparkElectricalFeedbackSystem : MonoBehaviour
    {
        [SerializeField] private SparkElectricalSolver solver;
        [SerializeField] private bool logFaultTransitions;

        private readonly HashSet<SparkTerminalIndicator> warnedIndicators =
            new HashSet<SparkTerminalIndicator>();
        private readonly HashSet<SparkTerminalIndicator> nextWarnedIndicators =
            new HashSet<SparkTerminalIndicator>();
        private readonly HashSet<SparkElectricalSafetyLimits> evaluatedLimits =
            new HashSet<SparkElectricalSafetyLimits>();

        private void OnEnable()
        {
            if (solver == null)
                solver = GetComponent<SparkElectricalSolver>();

            if (solver == null)
            {
                Debug.LogError("[SPARK FEEDBACK] Assign a SparkElectricalSolver.", this);
                return;
            }

            solver.SolveCompleted -= HandleSolveCompleted;
            solver.SolveFailed -= HandleSolveFailed;
            solver.SolveCompleted += HandleSolveCompleted;
            solver.SolveFailed += HandleSolveFailed;

            if (!solver.IsDirty && solver.LatestSolveResult != null)
                HandleSolveCompleted();
        }

        private void OnDisable()
        {
            if (solver != null)
            {
                solver.SolveCompleted -= HandleSolveCompleted;
                solver.SolveFailed -= HandleSolveFailed;
            }
            ClearFeedback();
        }

        private void HandleSolveCompleted()
        {
            if (solver == null || solver.IsDirty || solver.LatestSolveResult == null)
            {
                ClearFeedback();
                return;
            }

            nextWarnedIndicators.Clear();
            evaluatedLimits.Clear();

            IReadOnlyList<SparkElectricalComponent> components =
                solver.ElectricalComponents;

            if (components != null)
            {
                for (int i = 0; i < components.Count; i++)
                {
                    SparkElectricalComponent component = components[i];
                    if (component == null)
                        continue;

                    SparkElectricalSafetyLimits limits =
                        component.GetComponent<SparkElectricalSafetyLimits>();
                    if (limits == null)
                        continue;

                    evaluatedLimits.Add(limits);

                    float current;
                    bool hasCurrent = solver.TryCalculateComponentCurrent(
                        component, out current);

                    bool fault = hasCurrent &&
                        !float.IsNaN(current) &&
                        !float.IsInfinity(current) &&
                        limits.EvaluateCurrent(current);

                    if (!hasCurrent || float.IsNaN(current) || float.IsInfinity(current))
                    {
                        limits.ResetFault();
                        continue;
                    }

                    if (!fault)
                        continue;

                    if (logFaultTransitions && !limits.WasReported)
                        Debug.LogWarning(
                            $"[SPARK FEEDBACK] Overcurrent: {component.name}, " +
                            $"|I|={Mathf.Abs(current):F4} A", component);

                    limits.WasReported = true;
                    SparkTerminalIndicator[] indicators =
                        component.GetComponentsInChildren<SparkTerminalIndicator>(true);
                    for (int j = 0; j < indicators.Length; j++)
                    {
                        if (indicators[j] != null)
                            nextWarnedIndicators.Add(indicators[j]);
                    }
                }
            }

            foreach (SparkTerminalIndicator indicator in warnedIndicators)
            {
                if (indicator != null && !nextWarnedIndicators.Contains(indicator))
                    indicator.SetShortCircuit(false);
            }

            foreach (SparkTerminalIndicator indicator in nextWarnedIndicators)
            {
                if (indicator != null)
                    indicator.SetShortCircuit(true);
            }

            warnedIndicators.Clear();
            foreach (SparkTerminalIndicator indicator in nextWarnedIndicators)
                warnedIndicators.Add(indicator);
            nextWarnedIndicators.Clear();
        }

        private void HandleSolveFailed()
        {
            ClearFeedback();
        }

        public void ClearFeedback()
        {
            foreach (SparkTerminalIndicator indicator in warnedIndicators)
            {
                if (indicator != null)
                    indicator.SetShortCircuit(false);
            }
            warnedIndicators.Clear();
            nextWarnedIndicators.Clear();

            foreach (SparkElectricalSafetyLimits limits in evaluatedLimits)
            {
                if (limits != null)
                    limits.ResetFault();
            }
            evaluatedLimits.Clear();
        }
    }
}
