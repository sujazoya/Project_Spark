using System;
using System.Collections.Generic;
using System.Linq;
using ProjectSpark.AI.Tutorial;
using ProjectSpark.Circuit;
using ProjectSpark.Gameplay;
using UnityEngine;

namespace ProjectSpark.UI.VisualTeaching
{
    /// <summary>Optional read-only guidance for a level whose objective proof is separate from its physical task.</summary>
    public interface ISparkAILevelTeachingFocus
    {
        bool TryGetTeachingFocus(out Transform target, out string instruction);
    }

    internal static class SparkAILevelVisuals
    {
        private static readonly Color Cyan = new(.24f, .85f, 1);
        private static readonly Color Green = new(.38f, .91f, .69f);
        private static readonly Color Muted = new(.57f, .67f, .75f);

        internal static SparkVisualTeachingPlan ObjectivePage(SparkAILevelContext level, int page)
        {
            if (!level.Definition) return null;
            var targets = (level.Definition.Targets ?? Array.Empty<SparkLevelTarget>()).Where(target => target != null).ToArray();
            var cards = new List<SparkObjectiveCard>();
            const int pageSize = 6;
            int offset = page * pageSize;
            for (int i = 0; i < pageSize && offset + i < targets.Length; i++)
            {
                var target = targets[offset + i];
                bool? status = level.Status(target);
                cards.Add(new SparkObjectiveCard
                {
                    Title = Clip($"{offset + i + 1}. {SparkAILevelContext.TargetName(target)}", 92),
                    Description = Clip(string.IsNullOrWhiteSpace(target.Description) ?
                        SparkAILevelContext.TargetConditions(target) : target.Description, 110),
                    Status = status == true ? "PASSED" : status == false ? "PENDING" : "AWAITING EVALUATION",
                    Accent = status == true ? Green : status == false ? Cyan : Muted
                });
            }
            var next = level.NextTarget();
            string hint = next == null ? level.StateText() :
                "Next objective\n" + SparkAILevelContext.TargetName(next) + "\n" + next.Description;
            return new SparkVisualTeachingPlan
            {
                title = Clip(SparkAILevelContext.Title(level.Definition), 100),
                explanation = SparkAILevelContext.CompletionRule(level.Definition),
                instruction = Clip(hint, 260),
                status = "Objective status comes from gameplay evaluation.",
                schematic = new SparkSchematicData
                {
                    caption = "MISSION OBJECTIVES  /  " + level.StateText().Split('\n')[0],
                    objectiveCards = cards.ToArray()
                }
            };
        }

        internal static int ObjectivePageCount(SparkLevelDefinition level) =>
            level ? Math.Max(1, ((level.Targets?.Count(item => item != null) ?? 0) + 5) / 6) : 0;

        internal static SparkVisualTeachingPlan LiveWiring(SparkAILevelContext level)
        {
            if (!level.HasEvaluation || !level.Manager.CircuitSystem) return null;
            var native = new List<SparkCircuitConnection>();
            level.Manager.CircuitSystem.CopyConnections(native);
            var edges = native.Where(edge => edge.Kind == SparkConnectionKind.Wire &&
                edge.A && edge.B && edge.A.isActiveAndEnabled && edge.B.isActiveAndEnabled &&
                (edge.A.transform.IsChildOf(level.Layout.transform) ||
                 edge.B.transform.IsChildOf(level.Layout.transform))).ToArray();
            var pins = new List<SparkTerminal>();
            var selected = new List<SparkCircuitConnection>();
            foreach (var edge in edges)
            {
                if (selected.Count == 16) break;
                int newPins = (pins.Contains(edge.A) ? 0 : 1) + (pins.Contains(edge.B) ? 0 : 1);
                if (pins.Count + newPins > 12) continue;
                if (!pins.Contains(edge.A)) pins.Add(edge.A);
                if (!pins.Contains(edge.B)) pins.Add(edge.B);
                selected.Add(edge);
            }
            int columns = Mathf.Min(3, pins.Count);
            int rows = Mathf.CeilToInt(pins.Count / 3f);
            Vector2 Place(int index)
            {
                int row = index / columns;
                int rowCount = Mathf.Min(columns, pins.Count - row * columns);
                return new Vector2(.5f + (index % columns - (rowCount - 1) * .5f) * .31f,
                    .56f + ((rows - 1) * .5f - row) * .20f);
            }
            return new SparkVisualTeachingPlan
            {
                title = Clip(SparkAILevelContext.Title(level.Definition), 100),
                explanation = edges.Length == 0
                    ? "There are no recorded player wires attached to the loaded layout. Add a connection or request the objective map."
                    : $"Showing {selected.Count} of {edges.Length} recorded layout wires. Paths represent terminal connections; the simulator determines electrical state.",
                status = "Only recorded player wires are shown.",
                schematic = new SparkSchematicData
                {
                    caption = $"WORKBENCH WIRING  /  {selected.Count} OF {edges.Length} WIRES",
                    parts = pins.Select((pin, index) => new SparkSchematicPart
                    {
                        id = index.ToString(), kind = SparkSchematicKind.Generic, position = Place(index),
                        label = Clip(pin.name, 40), color = Cyan
                    }).ToArray(),
                    wires = selected.Select(edge => new SparkSchematicWire
                    {
                        points = new[] { Place(pins.IndexOf(edge.A)), Place(pins.IndexOf(edge.B)) },
                        color = Muted
                    }).ToArray()
                }
            };
        }

        internal static SparkVisualTeachingPlan WorldFocus(SparkAILevelContext level)
        {
            if (!level.Definition || !level.Layout) return null;
            Transform target = null;
            string instruction = null;
            foreach (var component in level.Layout.GetComponents<MonoBehaviour>())
            {
                if (component is ISparkAILevelTeachingFocus focus &&
                    focus.TryGetTeachingFocus(out target, out instruction) && target && target.gameObject.activeInHierarchy)
                    break;
                target = null;
            }
            var objective = level.NextTarget();
            if (!target && objective != null)
            {
                var bindings = level.Layout.SceneBindings;
                if (bindings && objective.HasRequiredConnectionPair &&
                    bindings.TryGetTerminal(objective.RequiredPositiveTerminalId, out var connectionStart))
                    target = connectionStart.transform;
                else if (bindings && bindings.TryGetTerminal(objective.TargetTerminalId, out var terminal)) target = terminal.transform;
                else if (bindings && bindings.TryGetComponent(objective.TargetComponentId, out var component)) target = component.transform;
                if (target) instruction = SparkAILevelContext.TargetName(objective);
            }
            if (!target || !target.gameObject.activeInHierarchy) return null;

            var point = new SparkVisualPoint { space = SparkPointSpace.WorldAnchor, liveTarget = target, anchorId = "loaded-target" };
            return new SparkVisualTeachingPlan
            {
                title = Clip(SparkAILevelContext.Title(level.Definition), 100),
                explanation = Clip(instruction, 300),
                status = "Workbench locator · position from the loaded layout.",
                annotations = new[]
                {
                    new SparkVisualCommand
                    {
                        shape = SparkVisualShape.Ring, from = point, to = point, color = Cyan,
                        drawSeconds = .3f, worldGeometry = true
                    },
                    new SparkVisualCommand
                    {
                        shape = SparkVisualShape.Label,
                        from = new SparkVisualPoint { space = SparkPointSpace.WorldAnchor, liveTarget = target,
                            anchorId = "loaded-target", pixelOffset = new Vector2(0, 75) },
                        to = point, text = Clip(instruction, 135), color = Cyan, fontSize = 17,
                        drawSeconds = .1f
                    }
                }
            };
        }

        private static string Clip(string value, int limit) =>
            string.IsNullOrEmpty(value) || value.Length <= limit ? value ?? "" : value.Substring(0, limit - 1) + "…";
    }
}
