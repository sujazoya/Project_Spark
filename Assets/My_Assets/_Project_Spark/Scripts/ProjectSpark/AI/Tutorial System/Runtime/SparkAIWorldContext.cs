
using System;
using System.Text;
using UnityEngine;
using ProjectSpark.Gameplay;
using ProjectSpark.Gameplay.Shelf;

namespace ProjectSpark.AI.Tutorial
{
    /// <summary>
    /// Global, read-only gameplay context for Spark AI.
    ///
    /// Automatically discovers the active level,
    /// evaluated objectives, and placed shelf components.
    ///
    /// No level IDs, slot IDs, or resistor values
    /// are hardcoded.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkAIWorldContext : MonoBehaviour
    {
        private LevelGamePlayManager levelManager;
        private LevelGamePlayManager subscribedManager;

        public LevelGamePlayManager LevelManager
        {
            get
            {
                ResolveManager();
                return levelManager;
            }
        }

        public SparkLevelDefinition ActiveLevel
        {
            get
            {
                return LevelManager != null
                    ? LevelManager.ActiveLevel
                    : null;
            }
        }

        private void OnEnable()
        {
            ResolveManager();
            Subscribe();
        }

        private void Start()
        {
            ResolveManager();
            Subscribe();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private void ResolveManager()
        {
            if (levelManager == null)
            {
                levelManager =
                    FindFirstObjectByType<
                        LevelGamePlayManager>();
            }

            if (isActiveAndEnabled &&
                levelManager != subscribedManager)
            {
                Subscribe();
            }
        }

        private void Subscribe()
        {
            if (subscribedManager == levelManager)
                return;

            Unsubscribe();

            if (levelManager == null)
                return;

            subscribedManager = levelManager;

            subscribedManager.LevelStarted += OnLevelChanged;
            subscribedManager.LevelReset += OnLevelChanged;
            subscribedManager.LevelCompleted += OnLevelChanged;
            subscribedManager.LevelFailed += OnLevelFailed;
        }

        private void Unsubscribe()
        {
            if (subscribedManager == null)
                return;

            subscribedManager.LevelStarted -= OnLevelChanged;
            subscribedManager.LevelReset -= OnLevelChanged;
            subscribedManager.LevelCompleted -= OnLevelChanged;
            subscribedManager.LevelFailed -= OnLevelFailed;

            subscribedManager = null;
        }

        private void OnLevelChanged(
            SparkLevelDefinition level)
        {
            // Do not retain scene-object references.
            // The next snapshot discovers the new runtime state.
        }

        private void OnLevelFailed(
            SparkLevelDefinition level,
            string reason)
        {
            // The manager remains the authority.
        }

        /// <summary>
        /// Finds all active shelf components currently
        /// registered as placed at the workstation.
        /// No particular slot name is required.
        /// </summary>
        public SparkShelfItem[] GetPlacedComponents()
        {
            SparkShelfItem[] all =
                FindObjectsByType<SparkShelfItem>(
                    FindObjectsInactive.Exclude,
                    FindObjectsSortMode.None);

            System.Collections.Generic.List<SparkShelfItem>
                placed =
                    new System.Collections.Generic.List<
                        SparkShelfItem>();

            foreach (SparkShelfItem item in all)
            {
                if (item == null)
                    continue;

                if (!item.IsAtWorkbench)
                    continue;

                if (item.CurrentSlot == null)
                    continue;

                // Confirm that the slot really owns
                // this item, not just its visual position.
                if (item.CurrentSlot.Occupant != item)
                    continue;

                placed.Add(item);
            }

            return placed.ToArray();
        }

        /// <summary>
        /// Builds a fresh snapshot on demand.
        /// This avoids stale values after solver updates.
        /// </summary>
        public string BuildSnapshot()
        {
            LevelGamePlayManager manager = LevelManager;

            if (manager == null ||
                manager.ActiveLevel == null)
            {
                return
                    "No active level. Runtime circuit " +
                    "state is unavailable.";
            }

            StringBuilder sb = new StringBuilder();

            sb.AppendLine("PROJECTSPARK AI WORLD CONTEXT");
            sb.AppendLine();

            sb.AppendLine(
                "Level ID: " +
                manager.ActiveLevel.LevelId);

            sb.AppendLine(
                "Level: " +
                manager.ActiveLevel.DisplayName);

            sb.AppendLine(
                "Runtime state: " +
                manager.RuntimeState);

            sb.AppendLine(
                "Completed: " + manager.IsCompleted);

            sb.AppendLine(
                "Failed: " + manager.IsFailed);

            sb.AppendLine(
                "Objectives: " +
                manager.SatisfiedTargetCount + "/" +
                manager.ActiveTargetCount);

            // ==========================================
            // ELECTRICAL EVALUATION
            // ==========================================

            sb.AppendLine();
            sb.AppendLine("CIRCUIT EVALUATION");

            sb.AppendLine(
                "Valid power source: " +
                manager.HasValidPowerSource);

            sb.AppendLine(
                "Closed return: " +
                manager.ClosedReturn);

            sb.AppendLine(
                "Source shorted: " +
                manager.SourceShorted);

            sb.AppendLine(
                "Target shorted: " +
                manager.TargetShorted);

            sb.AppendLine(
                "Overloaded: " +
                manager.Overloaded);

            sb.AppendLine(
                "Wrong connection: " +
                manager.WrongConnection);

            sb.AppendLine(
                "Evaluated target voltage: " +
                manager.TargetVoltage.ToString("F4") +
                " V");

            sb.AppendLine(
                "Evaluated target current: " +
                manager.TargetCurrent.ToString("F6") +
                " A");

            sb.AppendLine(
                "Evaluated target power: " +
                manager.TargetPower.ToString("F5") +
                " W");

            // ==========================================
            // LEVEL OBJECTIVES
            // ==========================================

            sb.AppendLine();
            sb.AppendLine("ACTIVE LEVEL OBJECTIVES");

            if (manager.ActiveLevel.Targets != null)
            {
                foreach (SparkLevelTarget target
                    in manager.ActiveLevel.Targets)
                {
                    if (target == null)
                        continue;

                    string name =
                        string.IsNullOrWhiteSpace(
                            target.DisplayName)
                            ? target.TargetId
                            : target.DisplayName;

                    sb.AppendLine();
                    sb.AppendLine("Target: " + name);
                    sb.AppendLine("ID: " + target.TargetId);
                    sb.AppendLine("Type: " + target.Type);

                    bool hasResult = false;

                    if (manager.TargetResults != null)
                    {
                        foreach (var result
                            in manager.TargetResults)
                        {
                            if (result.targetId !=
                                target.TargetId)
                                continue;

                            hasResult = true;

                            sb.AppendLine(
                                "Evaluator: " +
                                (result.satisfied
                                    ? "PASSED"
                                    : "PENDING"));

                            break;
                        }
                    }

                    if (!hasResult)
                        sb.AppendLine(
                            "Evaluator: UNKNOWN");

                    sb.AppendLine(
                        "Target component binding: " +
                        target.TargetComponentId);

                    sb.AppendLine(
                        "Requires conduction: " +
                        target.RequireConduction);

                    sb.AppendLine(
                        "Requires electrical enabled: " +
                        target.RequireElectricalEnabled);

                    sb.AppendLine(
                        "Inverted: " + target.Inverted);

                    if (target.Type ==
                        SparkLevelTarget.TargetType.CurrentRange)
                    {
                        sb.AppendLine(
                            "Minimum current: " +
                            (target.MinimumCurrent * 1000f)
                                .ToString("F3") +
                            " mA");

                        sb.AppendLine(
                            target.UseMaximumCurrent
                                ? "Maximum current: " +
                                  (target.MaximumCurrent * 1000f)
                                      .ToString("F3") + " mA"
                                : "Maximum current: disabled");
                    }
                }
            }

            // ==========================================
            // RUNTIME PLACED COMPONENTS
            // ==========================================

            sb.AppendLine();
            sb.AppendLine("PLACED WORKSTATION COMPONENTS");

            SparkShelfItem[] placed =
                GetPlacedComponents();

            sb.AppendLine(
                "Registered component count: " +
                placed.Length);

            foreach (SparkShelfItem item in placed)
            {
                sb.AppendLine();

                sb.AppendLine(
                    "Item: " + item.ItemId);

                sb.AppendLine(
                    "Slot: " +
                    item.CurrentSlot.SlotId);

                SparkResistor resistor =
                    item.GetComponent<SparkResistor>();

                if (resistor == null)
                {
                    resistor =
                        item.GetComponentInChildren<
                            SparkResistor>(true);
                }

                if (resistor != null)
                {
                    sb.AppendLine(
                        "Component type: Resistor");

                    sb.AppendLine(
                        "Resistance: " +
                        resistor.ResistanceOhms.ToString("F2") +
                        " ohms");

                    sb.AppendLine(
                        "Solved voltage: " +
                        resistor.Voltage.ToString("F4") +
                        " V");

                    sb.AppendLine(
                        "Solved current: " +
                        (resistor.Current * 1000f)
                            .ToString("F3") +
                        " mA");

                    sb.AppendLine(
                        "Solved power: " +
                        (resistor.PowerWatts * 1000f)
                            .ToString("F3") +
                        " mW");

                    sb.AppendLine(
                        "Conducting: " +
                        resistor.IsConducting);

                    sb.AppendLine(
                        "Burned: " +
                        resistor.IsBurned);
                }
                else
                {
                    sb.AppendLine(
                        "No resistor measurement adapter.");
                }
            }

            sb.AppendLine();
            sb.AppendLine("AI SAFETY RULES");

            sb.AppendLine(
                "The level evaluator owns target " +
                "pass/fail decisions.");

            sb.AppendLine(
                "A placed component is not automatically " +
                "the component bound to an objective.");

            sb.AppendLine(
                "Do not equate level target readings " +
                "with individual component readings.");

            sb.AppendLine(
                "Do not invent unresolved component " +
                "bindings or electrical measurements.");

                sb.AppendLine();
            sb.AppendLine("AUTHORITATIVE OBJECTIVE EVIDENCE");

            sb.AppendLine(
                SparkAIObjectiveReasoner.Diagnose(manager));

                sb.AppendLine();
            sb.AppendLine(
                SparkAIRuntimeBindingAudit.BuildReport(manager));

                sb.AppendLine();
            sb.AppendLine("GLOBAL LEVEL PROGRESSION");

            sb.AppendLine(
                SparkAIProgressionReasoner.Explain(manager));

            return sb.ToString();
        }
    }
}
