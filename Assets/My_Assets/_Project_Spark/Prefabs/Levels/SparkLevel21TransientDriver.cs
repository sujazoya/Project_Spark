using UnityEngine;
using ProjectSpark.Electrical;

namespace ProjectSpark.Gameplay
{
    /// <summary>
    /// Level 21 ONLY: periodically requests ordinary solves from the existing
    /// dirty/cached SparkElectricalSolver so its capacitor history advances.
    /// Does not alter any shared solver, evaluator, or electrical state directly.
    /// Attach inside Level_21_Layout (never to the global solver GameObject).
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkLevel21TransientDriver : MonoBehaviour
    {
        [Header("Level 21 References")]
        [SerializeField] private SparkElectricalSolver solver;
        [SerializeField] private LevelGamePlayManager levelManager;
        [SerializeField] private SparkCapacitor capacitor;
        [SerializeField] private SparkSwitch powerSwitch; // Optional scene reference; not used as a conduction gate.
        [SerializeField] private SparkPowerSupply powerSupply;

        [Header("Scheduling")]
        [Tooltip("Match SparkElectricalSolver's existing Transient/Time Step. " +
                 "Default 0.05 seconds means at most 20 scheduled solves per second.")]
        [SerializeField, Min(0.01f)] private float solveIntervalSeconds = 0.05f;
        [SerializeField] private bool stopWhenCharged = true;
        [SerializeField, Range(0.90f, 0.9999f)] private float settledFraction = 0.999f;
        [SerializeField, Min(0f)] private float minimumTargetVoltage = 0.1f;
        [SerializeField] private bool logStateChanges;

        private const string RequiredLevelId = "level_21";
        private float accumulator;
        private bool wasRunning;
        private bool wasSettled;

        private void Awake()
        {
            ResolveReferences();
        }

        private void OnEnable()
        {
            accumulator = 0f;
            wasRunning = false;
            wasSettled = false;
            ResolveReferences();
        }

        private void FixedUpdate()
        {
            if (!CanDrive())
            {
                if (wasRunning && logStateChanges)
                    Debug.Log("[Level21 Transient] Paused (inactive level/supply).", this);

                wasRunning = false;
                accumulator = 0f;
                return;
            }

            float target = Mathf.Abs(powerSupply.OutputVoltage);
            bool settled = stopWhenCharged &&
                           target >= minimumTargetVoltage &&
                           Mathf.Abs(capacitor.ElectricalState.Voltage) >=
                               target * settledFraction;

            if (settled)
            {
                if (!wasSettled && logStateChanges)
                    Debug.Log("[Level21 Transient] Capacitor reached settled threshold.", this);
                wasSettled = true;
                wasRunning = false;
                accumulator = 0f;
                return;
            }

            wasSettled = false;
            wasRunning = true;
            accumulator += Time.fixedDeltaTime;

            if (accumulator < solveIntervalSeconds)
                return;

            // Do not catch up after frame stalls (avoids solve bursts).
            accumulator = 0f;
            if (solver.IsSolving)
                return;

            // Do NOT gate this on SparkSwitch.IsConducting: in the current game
            // that flag can be false until AFTER current is solved, causing a
            // startup deadlock. The actual circuit graph determines charging.
            // The stable solver is cache/dirty-based. Dirty is necessary for
            // each backward-Euler capacitor time step to be committed.
            solver.MarkDirty();
            if (!solver.SolveNow() && logStateChanges)
                Debug.LogWarning("[Level21 Transient] Solve failed; verify circuit/solver convergence.", this);
        }

       
private bool CanDrive()
{
    if (solver == null ||
        capacitor == null ||
        powerSupply == null ||
        levelManager == null)
    {
        return false;
    }

    if (!solver.isActiveAndEnabled ||
        !capacitor.isActiveAndEnabled ||
        !capacitor.ElectricalEnabled ||
        !powerSupply.isActiveAndEnabled ||
        !powerSupply.ElectricalEnabled ||
        !powerSupply.IsOutputActive)
    {
        return false;
    }

    SparkLevelDefinition level =
        levelManager.ActiveLevel;

    if (level == null ||
        level.LevelId != "level_21")
    {
        return false;
    }

    // Continue charging after level completion.
    // The level reaches 3/3 at approximately 95%,
    // but capacitor simulation should continue.

    LevelGamePlayManager.LevelRuntimeState state =
        levelManager.RuntimeState;

    return state ==
               LevelGamePlayManager.LevelRuntimeState.Playing ||
           state ==
               LevelGamePlayManager.LevelRuntimeState.Completed;
}


        private void ResolveReferences()
        {
            if (solver == null)
                solver = FindFirstObjectByType<SparkElectricalSolver>();
            if (levelManager == null)
                levelManager = FindFirstObjectByType<LevelGamePlayManager>();
            if (capacitor == null)
                capacitor = GetComponentInChildren<SparkCapacitor>(true);
            if (powerSwitch == null)
                powerSwitch = GetComponentInChildren<SparkSwitch>(true);
            if (powerSupply == null)
                powerSupply = GetComponentInChildren<SparkPowerSupply>(true);
        }
    }
}
