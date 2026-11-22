
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
        [SerializeField] private LevelGamePlayManager levelManager;
        [SerializeField] private SparkLevelLayoutLoader levelLoader;

        public LevelGamePlayManager LevelManager
        {
            get
            {
                if (!levelManager) levelManager = FindFirstObjectByType<LevelGamePlayManager>();
                return levelManager;
            }
        }

        public SparkAILevelContext Capture() => SparkAILevelContext.Capture(LevelManager, levelLoader);
        public SparkLevelDefinition ActiveLevel => Capture().Definition;

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
            var context = Capture();
            var text = new StringBuilder("PROJECTSPARK • LOADED LEVEL").AppendLine();
            text.AppendLine(context.Describe());
            if (!context.HasEvaluation) return text.ToString();

            var manager = context.Manager;
            text.AppendLine().AppendLine("GAMEPLAY EVALUATOR");
            text.AppendLine($"Valid source {manager.HasValidPowerSource}; closed return {manager.ClosedReturn}; " +
                $"source short {manager.SourceShorted}; target short {manager.TargetShorted}; " +
                $"overload {manager.Overloaded}; wrong connection {manager.WrongConnection}.");
            text.AppendLine(SparkAIObjectiveReasoner.Diagnose(manager));
            text.AppendLine(SparkAIRuntimeBindingAudit.BuildReport(manager));

            text.AppendLine().AppendLine("REGISTERED WORKBENCH PARTS");
            foreach (var item in GetPlacedComponents())
            {
                if (item.transform.IsChildOf(context.Layout.transform) ||
                    item.CurrentSlot.transform.IsChildOf(context.Layout.transform))
                    text.AppendLine($"{item.ItemId} in {item.CurrentSlot.SlotId}");
            }
            return text.ToString();
        }
    }
}
