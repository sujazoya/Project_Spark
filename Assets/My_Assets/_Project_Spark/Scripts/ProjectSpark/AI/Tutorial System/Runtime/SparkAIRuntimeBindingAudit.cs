
using System;
using System.Text;
using UnityEngine;
using ProjectSpark.Gameplay;
using ProjectSpark.Gameplay.Shelf;
using ProjectSpark.Electrical;
using ProjectSpark.Circuit;

namespace ProjectSpark.AI.Tutorial
{
    /// <summary>
    /// Audits target bindings in the currently loaded
    /// level layout.
    ///
    /// Read-only: never changes the solver, level
    /// evaluator, shelf placement, or scene bindings.
    /// </summary>
    public static class SparkAIRuntimeBindingAudit
    {
        public static string BuildReport(
            LevelGamePlayManager manager)
        {
            if (manager == null ||
                manager.ActiveLevel == null)
            {
                return "No active level to audit.";
            }

            SparkLevelSceneBindings bindings =
                manager.SceneBindings;

            if (bindings == null)
            {
                return
                    "The active level has no runtime " +
                    "SparkLevelSceneBindings.";
            }

            StringBuilder sb = new StringBuilder();

            sb.AppendLine("SPARK AI - RUNTIME BINDING AUDIT");
            sb.AppendLine();

            sb.AppendLine(
                "Level: " +
                manager.ActiveLevel.DisplayName);

            sb.AppendLine(
                "Active binding object: " +
                bindings.gameObject.name);

            sb.AppendLine();

            int checkedTargets = 0;
            int missingBindings = 0;
            int staleShelfReferences = 0;

            if (manager.ActiveLevel.Targets != null)
            {
                foreach (SparkLevelTarget target
                         in manager.ActiveLevel.Targets)
                {
                    if (target == null)
                        continue;

                    checkedTargets++;

                    sb.AppendLine(
                        "TARGET: " +
                        (string.IsNullOrWhiteSpace(
                            target.DisplayName)
                            ? target.TargetId
                            : target.DisplayName));

                    sb.AppendLine(
                        "Type: " + target.Type);

                    if (!string.IsNullOrWhiteSpace(
                            target.TargetComponentId))
                    {
                        string bindingId =
                            target.TargetComponentId;

                        sb.AppendLine(
                            "Component ID: " + bindingId);

                        if (!bindings.TryGetComponent(
                                bindingId,
                                out SparkElectricalComponent component) ||
                            component == null)
                        {
                            missingBindings++;

                            sb.AppendLine(
                                "[UNRESOLVED] No electrical " +
                                "component is registered " +
                                "under this ID.");
                        }
                        else
                        {
                            sb.AppendLine(
                                "Resolved: " +
                                component.name);

                            sb.AppendLine(
                                "Runtime type: " +
                                component.GetType().Name);

                            sb.AppendLine(
                                "GameObject active: " +
                                component.gameObject
                                    .activeInHierarchy);

                            SparkShelfItem shelfItem =
                                component.GetComponentInParent<
                                    SparkShelfItem>();

                            if (shelfItem != null)
                            {
                                bool correctlyInstalled =
                                    shelfItem.IsAtWorkbench &&
                                    shelfItem.CurrentSlot != null &&
                                    shelfItem.CurrentSlot.Occupant ==
                                        shelfItem;

                                sb.AppendLine(
                                    "Shelf item: " +
                                    shelfItem.ItemId);

                                sb.AppendLine(
                                    "Registered installation: " +
                                    correctlyInstalled);

                                if (!correctlyInstalled)
                                {
                                    staleShelfReferences++;

                                    sb.AppendLine(
                                        "[WARNING] This binding " +
                                        "points to a shelf item " +
                                        "that is not currently " +
                                        "registered as installed.");
                                }
                            }
                            else
                            {
                                sb.AppendLine(
                                    "Not owned by a SparkShelfItem. " +
                                    "This may be a fixed " +
                                    "level component.");
                            }

                            SparkElectricalState state =
                                component.ElectricalState;

                            sb.AppendLine(
                                "Solved voltage: " +
                                state.Voltage.ToString("F4") +
                                " V");

                            sb.AppendLine(
                                "Solved current: " +
                                (state.Current * 1000f)
                                    .ToString("F3") +
                                " mA");

                            sb.AppendLine(
                                "Solved power: " +
                                (state.Power * 1000f)
                                    .ToString("F3") +
                                " mW");
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(
                            target.TargetTerminalId))
                    {
                        if (!bindings.TryGetTerminal(
                                target.TargetTerminalId,
                                out SparkTerminal terminal) ||
                            terminal == null)
                        {
                            missingBindings++;

                            sb.AppendLine(
                                "[UNRESOLVED] Terminal ID: " +
                                target.TargetTerminalId);
                        }
                    }

                    if (target.HasRequiredConnectionPair)
                    {
                        bool positiveResolved =
                            bindings.TryGetTerminal(
                                target.RequiredPositiveTerminalId,
                                out SparkTerminal positive) &&
                            positive != null;

                        bool negativeResolved =
                            bindings.TryGetTerminal(
                                target.RequiredNegativeTerminalId,
                                out SparkTerminal negative) &&
                            negative != null;

                        if (!positiveResolved ||
                            !negativeResolved)
                        {
                            missingBindings++;

                            sb.AppendLine(
                                "[UNRESOLVED] Required " +
                                "terminal pair.");
                        }
                    }

                    sb.AppendLine();
                }
            }

            sb.AppendLine("AUDIT SUMMARY");
            sb.AppendLine(
                "Targets checked: " + checkedTargets);

            sb.AppendLine(
                "Unresolved binding checks: " +
                missingBindings);

            sb.AppendLine(
                "Uninstalled shelf references: " +
                staleShelfReferences);

            if (missingBindings == 0 &&
                staleShelfReferences == 0)
            {
                sb.AppendLine(
                    "No missing or uninstalled-shelf " +
                    "references were detected.");
            }

            sb.AppendLine();
            sb.AppendLine(
                "Note: A resolved binding is not " +
                "proof that it represents the " +
                "intended component. Binding IDs " +
                "must still be registered correctly " +
                "by the gameplay layout and placement systems.");

            return sb.ToString();
        }
    }
}
