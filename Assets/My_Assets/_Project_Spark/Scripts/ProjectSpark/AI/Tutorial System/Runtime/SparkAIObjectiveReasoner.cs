
using System;
using System.Text;
using UnityEngine;
using ProjectSpark.Gameplay;
using ProjectSpark.Electrical;

namespace ProjectSpark.AI.Tutorial
{
    /// <summary>
    /// Global, read-only diagnosis of the active level.
    ///
    /// Reads:
    /// - Active level definitions
    /// - Official evaluator results from the manager
    /// - Active runtime scene bindings
    /// - Electrical solver component states
    ///
    /// Does not modify gameplay, objectives or solver data.
    /// </summary>
    public static class SparkAIObjectiveReasoner
    {
        public static string Diagnose(
            LevelGamePlayManager manager,
            bool pendingOnly = false)
        {
            if (manager == null ||
                manager.ActiveLevel == null)
            {
                return "No active level is available.";
            }

            StringBuilder sb = new StringBuilder();

            sb.AppendLine("SPARK AI - GLOBAL LEVEL DIAGNOSIS");
            sb.AppendLine();
            sb.AppendLine(
                "Level: " + manager.ActiveLevel.DisplayName);

            sb.AppendLine(
                "Progress: " +
                manager.SatisfiedTargetCount + "/" +
                manager.ActiveTargetCount);

            sb.AppendLine(
                "Runtime state: " + manager.RuntimeState);

            if (manager.IsCompleted)
            {
                sb.AppendLine(
                    "The level evaluator confirms completion.");
            }

            if (manager.IsFailed)
            {
                sb.AppendLine(
                    "Level failure: " +
                    manager.LastFailureReason);
            }

            if (!string.IsNullOrWhiteSpace(
                    manager.LastEvaluationMessage))
            {
                sb.AppendLine(
                    "Evaluator message: " +
                    manager.LastEvaluationMessage);
            }

            AppendCircuitFlags(sb, manager);

            SparkLevelTarget[] targets =
                manager.ActiveLevel.Targets;

            if (targets == null || targets.Length == 0)
            {
                sb.AppendLine();
                sb.AppendLine(
                    "No target definitions are available.");

                return sb.ToString();
            }

            int reported = 0;

            foreach (SparkLevelTarget target in targets)
            {
                if (target == null)
                    continue;

                bool? passed =
                    GetOfficialTargetResult(
                        manager, target.TargetId);

                if (pendingOnly &&
                    passed.HasValue &&
                    passed.Value)
                {
                    continue;
                }

                reported++;

                AppendTargetDiagnosis(
                    sb,
                    manager.SceneBindings,
                    target,
                    passed);
            }

            if (reported == 0)
            {
                sb.AppendLine();
                sb.AppendLine(
                    "No pending target definitions were found.");
            }

            return sb.ToString();
        }

        private static bool? GetOfficialTargetResult(
            LevelGamePlayManager manager,
            string targetId)
        {
            if (manager.TargetResults == null)
                return null;

            foreach (var result in manager.TargetResults)
            {
                if (result.targetId == targetId)
                    return result.satisfied;
            }

            return null;
        }

        private static void AppendCircuitFlags(
            StringBuilder sb,
            LevelGamePlayManager manager)
        {
            sb.AppendLine();
            sb.AppendLine("CIRCUIT");

            if (manager.SourceShorted)
                sb.AppendLine(
                    "- Source short circuit detected.");

            if (manager.TargetShorted)
                sb.AppendLine(
                    "- Target short circuit detected.");

            if (manager.Overloaded)
                sb.AppendLine(
                    "- Power source overload detected.");

            if (manager.WrongConnection)
                sb.AppendLine(
                    "- Incorrect connection detected.");

            if (!manager.HasValidPowerSource)
                sb.AppendLine(
                    "- No valid active power source.");

            if (!manager.ClosedReturn)
                sb.AppendLine(
                    "- Closed return is not confirmed.");
        }

        private static void AppendTargetDiagnosis(
            StringBuilder sb,
            SparkLevelSceneBindings bindings,
            SparkLevelTarget target,
            bool? passed)
        {
            sb.AppendLine();

            string name =
                string.IsNullOrWhiteSpace(
                    target.DisplayName)
                    ? target.TargetId
                    : target.DisplayName;

            sb.AppendLine(
                (passed.HasValue
                    ? passed.Value
                        ? "[PASS] "
                        : "[PENDING] "
                    : "[UNKNOWN] ") + name);

            sb.AppendLine(
                "Target type: " + target.Type);

            if (!string.IsNullOrWhiteSpace(
                    target.Description))
            {
                sb.AppendLine(
                    "Goal: " + target.Description);
            }

            if (!target.IsConfigured())
            {
                sb.AppendLine(
                    "Configuration problem: " +
                    "target is incomplete.");
            }

            if (!target.IsPolarityConfigurationValid())
            {
                sb.AppendLine(
                    "Configuration problem: " +
                    "terminal polarity configuration invalid.");
            }

            if (target.Inverted)
            {
                sb.AppendLine(
                    "Inverted evaluation: enabled.");
            }

            switch (target.Type)
            {
                case SparkLevelTarget.TargetType.CurrentRange:
                case SparkLevelTarget.TargetType.PowerRange:
                    AppendComponentRange(
                        sb, bindings, target);
                    break;

                case SparkLevelTarget.TargetType.VoltageRange:
                    AppendVoltageRange(
                        sb, bindings, target);
                    break;

                default:
                    AppendGeneralTarget(
                        sb, bindings, target);
                    break;
            }

            sb.AppendLine(
                "Official result: " +
                (passed.HasValue
                    ? passed.Value
                        ? "PASSED"
                        : "PENDING"
                    : "Not evaluated"));

            if (target.Inverted)
            {
                sb.AppendLine(
                    "Note: the final result is inverted " +
                    "by the level evaluator.");
            }
        }

        private static void AppendComponentRange(
            StringBuilder sb,
            SparkLevelSceneBindings bindings,
            SparkLevelTarget target)
        {
            if (!TryGetTargetComponent(
                    bindings,
                    target,
                    out SparkElectricalComponent component))
            {
                sb.AppendLine(
                    "UNRESOLVED COMPONENT BINDING: " +
                    target.TargetComponentId);

                sb.AppendLine(
                    "The active scene bindings do not " +
                    "resolve this objective's component.");

                return;
            }

            SparkElectricalState state =
                component.ElectricalState;

            bool isCurrent =
                target.Type ==
                SparkLevelTarget.TargetType.CurrentRange;

            float measured =
                Mathf.Abs(isCurrent
                    ? state.Current
                    : state.Power);

            float minimum =
                isCurrent
                    ? target.MinimumCurrent
                    : target.MinimumPower;

            bool hasMaximum =
                isCurrent
                    ? target.UseMaximumCurrent
                    : target.UseMaximumPower;

            float maximum =
                isCurrent
                    ? target.MaximumCurrent
                    : target.MaximumPower;

            float multiplier =
                1000f;

            string unit =
                isCurrent ? "mA" : "mW";

            sb.AppendLine(
                "BOUND COMPONENT: " + component.name);

            sb.AppendLine(
                "Component type: " +
                component.GetType().Name);

            sb.AppendLine(
                "Binding ID: " +
                target.TargetComponentId);

            sb.AppendLine(
                "Measured: " +
                (measured * multiplier).ToString("F3") +
                " " + unit);

            sb.AppendLine(
                "Minimum: " +
                (minimum * multiplier).ToString("F3") +
                " " + unit);

            sb.AppendLine(
                hasMaximum
                    ? "Maximum: " +
                      (maximum * multiplier).ToString("F3") +
                      " " + unit
                    : "Maximum: Not enabled");

            if (target.RequireElectricalEnabled &&
                !component.ElectricalEnabled)
            {
                sb.AppendLine(
                    "FAILED CONDITION: Electrical " +
                    "component is disabled.");
            }

            if (target.RequireConduction &&
                state.Conduction !=
                    SparkConductionState.Conducting)
            {
                sb.AppendLine(
                    "FAILED CONDITION: Required " +
                    "conduction is not present.");
            }

            if (float.IsNaN(measured) ||
                float.IsInfinity(measured))
            {
                sb.AppendLine(
                    "FAILED CONDITION: Measurement " +
                    "is not a finite number.");

                return;
            }

            if (measured < minimum)
            {
                sb.AppendLine(
                    "BELOW MINIMUM BY: " +
                    ((minimum - measured) * multiplier)
                        .ToString("F3") +
                    " " + unit);
            }
            else if (hasMaximum && measured > maximum)
            {
                sb.AppendLine(
                    "ABOVE MAXIMUM BY: " +
                    ((measured - maximum) * multiplier)
                        .ToString("F3") +
                    " " + unit);
            }
            else
            {
                sb.AppendLine(
                    "Numeric range check: PASSED.");
            }
        }

        private static void AppendVoltageRange(
            StringBuilder sb,
            SparkLevelSceneBindings bindings,
            SparkLevelTarget target)
        {
            if (bindings == null)
            {
                sb.AppendLine(
                    "Active scene bindings unavailable.");

                return;
            }

            float voltage;
            bool resolved = false;

            if (target.HasRequiredConnectionPair)
            {
                if (bindings.TryGetTerminal(
                        target.RequiredPositiveTerminalId,
                        out SparkTerminal positive) &&
                    bindings.TryGetTerminal(
                        target.RequiredNegativeTerminalId,
                        out SparkTerminal negative) &&
                    positive != null &&
                    negative != null)
                {
                    voltage = Mathf.Abs(
                        positive.ElectricalState.Voltage -
                        negative.ElectricalState.Voltage);

                    resolved = true;

                    if (target.RequireElectricalEnabled &&
                        (!positive.IsElectricalEnabled ||
                         !negative.IsElectricalEnabled))
                    {
                        sb.AppendLine(
                            "FAILED CONDITION: " +
                            "Required terminal is disabled.");
                    }

                    AppendVoltageMeasurement(
                        sb, target, voltage);
                }
            }
            else if (!string.IsNullOrWhiteSpace(
                         target.TargetComponentId))
            {
                if (TryGetTargetComponent(
                        bindings,
                        target,
                        out SparkElectricalComponent component))
                {
                    voltage = Mathf.Abs(
                        component.ElectricalState.Voltage);

                    resolved = true;

                    sb.AppendLine(
                        "BOUND COMPONENT: " +
                        component.name);

                    if (target.RequireElectricalEnabled &&
                        !component.ElectricalEnabled)
                    {
                        sb.AppendLine(
                            "FAILED CONDITION: " +
                            "Component is disabled.");
                    }

                    if (target.RequireConduction &&
                        component.ElectricalState.Conduction !=
                            SparkConductionState.Conducting)
                    {
                        sb.AppendLine(
                            "FAILED CONDITION: " +
                            "Conduction is not present.");
                    }

                    AppendVoltageMeasurement(
                        sb, target, voltage);
                }
            }
            else
            {
                if (bindings.TryGetTerminal(
                        target.TargetTerminalId,
                        out SparkTerminal terminal) &&
                    terminal != null)
                {
                    voltage = Mathf.Abs(
                        terminal.ElectricalState.Voltage);

                    resolved = true;

                    if (target.RequireElectricalEnabled &&
                        !terminal.IsElectricalEnabled)
                    {
                        sb.AppendLine(
                            "FAILED CONDITION: " +
                            "Terminal is disabled.");
                    }

                    AppendVoltageMeasurement(
                        sb, target, voltage);
                }
            }

            if (!resolved)
            {
                sb.AppendLine(
                    "Voltage measurement unresolved " +
                    "from the active scene bindings.");
            }
        }

        private static void AppendVoltageMeasurement(
            StringBuilder sb,
            SparkLevelTarget target,
            float value)
        {
            sb.AppendLine(
                "Measured voltage: " +
                value.ToString("F3") + " V");

            sb.AppendLine(
                "Minimum: " +
                target.MinimumVoltage.ToString("F3") +
                " V");

            if (target.UseMaximumVoltage)
            {
                sb.AppendLine(
                    "Maximum: " +
                    target.MaximumVoltage.ToString("F3") +
                    " V");
            }

            if (float.IsNaN(value) ||
                float.IsInfinity(value))
            {
                sb.AppendLine(
                    "FAILED CONDITION: Invalid measurement.");
            }
            else if (value < target.MinimumVoltage)
            {
                sb.AppendLine(
                    "BELOW MINIMUM BY: " +
                    (target.MinimumVoltage - value)
                        .ToString("F3") + " V");
            }
            else if (target.UseMaximumVoltage &&
                     value > target.MaximumVoltage)
            {
                sb.AppendLine(
                    "ABOVE MAXIMUM BY: " +
                    (value - target.MaximumVoltage)
                        .ToString("F3") + " V");
            }
            else
            {
                sb.AppendLine(
                    "Numeric range check: PASSED.");
            }
        }

        private static void AppendGeneralTarget(
            StringBuilder sb,
            SparkLevelSceneBindings bindings,
            SparkLevelTarget target)
        {
            if (bindings == null)
            {
                sb.AppendLine(
                    "Active bindings unavailable.");
                return;
            }

            if (!string.IsNullOrWhiteSpace(
                    target.TargetComponentId))
            {
                if (TryGetTargetComponent(
                        bindings,
                        target,
                        out SparkElectricalComponent component))
                {
                    SparkElectricalState state =
                        component.ElectricalState;

                    sb.AppendLine(
                        "BOUND COMPONENT: " +
                        component.name);

                    sb.AppendLine(
                        "Voltage: " +
                        state.Voltage.ToString("F3") +
                        " V");

                    sb.AppendLine(
                        "Current: " +
                        (state.Current * 1000f)
                            .ToString("F3") +
                        " mA");

                    sb.AppendLine(
                        "Power: " +
                        (state.Power * 1000f)
                            .ToString("F3") +
                        " mW");

                    sb.AppendLine(
                        "Conducting: " +
                        (state.Conduction ==
                         SparkConductionState.Conducting));

                    sb.AppendLine(
                        "Electrical enabled: " +
                        component.ElectricalEnabled);
                }
                else
                {
                    sb.AppendLine(
                        "Component binding unresolved: " +
                        target.TargetComponentId);
                }
            }
            else if (!string.IsNullOrWhiteSpace(
                         target.TargetTerminalId))
            {
                if (bindings.TryGetTerminal(
                        target.TargetTerminalId,
                        out SparkTerminal terminal) &&
                    terminal != null)
                {
                    sb.AppendLine(
                        "BOUND TERMINAL: " + terminal.name);

                    sb.AppendLine(
                        "Voltage: " +
                        terminal.ElectricalState.Voltage
                            .ToString("F3") + " V");
                }
                else
                {
                    sb.AppendLine(
                        "Terminal binding unresolved: " +
                        target.TargetTerminalId);
                }
            }
        }

        private static bool TryGetTargetComponent(
            SparkLevelSceneBindings bindings,
            SparkLevelTarget target,
            out SparkElectricalComponent component)
        {
            component = null;

            return
                bindings != null &&
                target != null &&
                !string.IsNullOrWhiteSpace(
                    target.TargetComponentId) &&
                bindings.TryGetComponent(
                    target.TargetComponentId,
                    out component) &&
                component != null;
        }
    }
}
