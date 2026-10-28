
using System;
using System.Text;
using UnityEngine;
using ProjectSpark.Gameplay;
using ProjectSpark.Circuit;
using ProjectSpark.Electrical;

namespace ProjectSpark.AI.Tutorial
{
    /// <summary>
    /// Global, read-only troubleshooting.
    /// Uses the same active scene bindings as
    /// SparkLevelEvaluator.
    ///
    /// Never changes wiring, components, or level state.
    /// </summary>
    public static class SparkAILiveTroubleshooter
    {
        public static string Diagnose(
            LevelGamePlayManager manager)
        {
            if (manager == null ||
                manager.ActiveLevel == null)
            {
                return "Start a level to diagnose its circuit.";
            }

            StringBuilder sb = new StringBuilder();

            sb.AppendLine("SPARK AI - CIRCUIT TROUBLESHOOTING");
            sb.AppendLine();
            sb.AppendLine(
                "Level: " + manager.ActiveLevel.DisplayName);

            sb.AppendLine(
                "Objectives: " +
                manager.SatisfiedTargetCount + "/" +
                manager.ActiveTargetCount);

            sb.AppendLine(
                "Status: " + manager.RuntimeState);
            sb.AppendLine();

            if (manager.IsCompleted)
            {
                sb.AppendLine(
                    "The level evaluator confirms completion.");

                return sb.ToString();
            }

            AppendFaults(sb, manager);
            AppendPendingTarget(sb, manager);

            if (!string.IsNullOrWhiteSpace(
                manager.LastEvaluationMessage))
            {
                sb.AppendLine();
                sb.AppendLine(
                    "Evaluator: " +
                    manager.LastEvaluationMessage);
            }

            return sb.ToString();
        }

        private static void AppendFaults(
            StringBuilder sb,
            LevelGamePlayManager manager)
        {
            sb.AppendLine("CIRCUIT CHECK");

            if (manager.IsFailed)
            {
                sb.AppendLine(
                    "Confirmed level failure: " +
                    manager.LastFailureReason);
            }

            if (manager.SourceShorted)
            {
                sb.AppendLine(
                    "FAULT: Source short circuit.");

                sb.AppendLine(
                    "NEXT: Inspect conductive paths " +
                    "between the supply terminals " +
                    "that may bypass the intended load.");
            }
            else if (manager.TargetShorted)
            {
                sb.AppendLine(
                    "FAULT: Target short circuit.");

                sb.AppendLine(
                    "NEXT: Check for connections " +
                    "bypassing the target component.");
            }
            else if (manager.Overloaded)
            {
                sb.AppendLine(
                    "FAULT: Power source overload.");

                sb.AppendLine(
                    "NEXT: Check current demand and " +
                    "the circuit's total load.");
            }
            else if (manager.WrongConnection)
            {
                sb.AppendLine(
                    "FAULT: Invalid connection reported.");

                sb.AppendLine(
                    "NEXT: Check the target's required " +
                    "terminal polarity and wire endpoints.");
            }
            else if (!manager.HasValidPowerSource)
            {
                sb.AppendLine(
                    "BLOCKER: No valid active power source.");

                sb.AppendLine(
                    "NEXT: Verify the configured power " +
                    "supply is enabled and its output active.");
            }
            else if (!manager.ClosedReturn)
            {
                sb.AppendLine(
                    "BLOCKER: Closed return not confirmed.");

                sb.AppendLine(
                    "NEXT: Inspect the switch and " +
                    "connections through the return path.");
            }
            else
            {
                sb.AppendLine(
                    "No manager-level short, overload, " +
                    "source, or return-path blocker " +
                    "was detected.");
            }

            sb.AppendLine();
        }

       
private static void AppendPendingTarget(
    StringBuilder sb,
    LevelGamePlayManager manager)
{
    var results = manager.TargetResults;
    var targets = manager.ActiveLevel.Targets;

    if (targets == null || targets.Length == 0)
    {
        sb.AppendLine(
            "No target definitions are configured.");
        return;
    }

    if (results == null || results.Count == 0)
    {
        sb.AppendLine(
            "Individual evaluator results are not " +
            "available yet.");
        return;
    }

    sb.AppendLine("OBJECTIVE RESULTS");

    foreach (var result in results)
    {
        sb.AppendLine(
            (result.satisfied
                ? "[PASS] "
                : "[PENDING] ") +
            (string.IsNullOrWhiteSpace(result.displayName)
                ? result.targetId
                : result.displayName));
    }

    sb.AppendLine();

   
SparkLevelTarget firstPending = null;

// LevelGamePlayManager builds TargetResults in
// ActiveLevel.Targets order, skipping null targets.
// Use that authoritative ordering instead of
// relying on TargetId uniqueness alone.

int resultIndex = 0;

for (int i = 0; i < targets.Length; i++)
{
    SparkLevelTarget target = targets[i];

    if (target == null)
        continue;

    if (resultIndex >= results.Count)
    {
        sb.AppendLine(
            "WARNING: The target result list is shorter " +
            "than the configured target list.");

        break;
    }

    var result = results[resultIndex];
    resultIndex++;

    // Inform the developer if the target data
    // and runtime result order are inconsistent.
    if (!string.Equals(
            result.targetId,
            target.TargetId,
            StringComparison.Ordinal))
    {
        sb.AppendLine(
            "WARNING: Target result mismatch at " +
            "target index " + i + ".");

        sb.AppendLine(
            "Configured ID: " + target.TargetId);

        sb.AppendLine(
            "Runtime ID: " + result.targetId);

        // Do not assign another target's result.
        continue;
    }

    if (!result.satisfied)
    {
        firstPending = target;
        break;
    }
}

    if (firstPending == null)
    {
        sb.AppendLine(
            "No unfinished target could be mapped " +
            "to its definition.");

        sb.AppendLine(
            "Configured target IDs and runtime " +
            "result IDs need checking.");

        sb.AppendLine();
        sb.AppendLine("CONFIGURED TARGET IDs");

        foreach (SparkLevelTarget target in targets)
        {
            if (target != null)
                sb.AppendLine(
                    target.DisplayName + ": " +
                    target.TargetId);
        }

        sb.AppendLine();
        sb.AppendLine("RUNTIME RESULT IDs");

        foreach (var result in results)
        {
            sb.AppendLine(
                result.displayName + ": " +
                result.targetId);
        }

        return;
    }

    sb.AppendLine("FIRST PENDING OBJECTIVE");

    sb.AppendLine(
        string.IsNullOrWhiteSpace(
            firstPending.DisplayName)
            ? firstPending.TargetId
            : firstPending.DisplayName);

    sb.AppendLine(
        "Type: " + firstPending.Type);

    if (!string.IsNullOrWhiteSpace(
            firstPending.Description))
    {
        sb.AppendLine(firstPending.Description);
    }

    if (!firstPending.IsConfigured() ||
        !firstPending.IsPolarityConfigurationValid())
    {
        sb.AppendLine(
            "WARNING: Target configuration " +
            "requires review.");
    }

    AppendTargetEvidence(
        sb,
        manager.SceneBindings,
        firstPending);
}



        private static void AppendTargetEvidence(
            StringBuilder sb,
            SparkLevelSceneBindings bindings,
            SparkLevelTarget target)
        {
            sb.AppendLine();

            if (bindings == null)
            {
                sb.AppendLine(
                    "Cannot inspect target: active " +
                    "scene bindings are missing.");
                return;
            }

            if (target.Type ==
                SparkLevelTarget.TargetType.VoltageRange)
            {
                AppendVoltageRange(sb, bindings, target);
                return;
            }

            SparkElectricalComponent component = null;

            if (!string.IsNullOrWhiteSpace(
                target.TargetComponentId))
            {
                bindings.TryGetComponent(
                    target.TargetComponentId,
                    out component);
            }

            if (component == null)
            {
                if (target.Type ==
                    SparkLevelTarget.TargetType.TerminalPowered ||
                    target.Type ==
                    SparkLevelTarget.TargetType.VoltagePresent)
                {
                    AppendTerminalEvidence(
                        sb, bindings, target);
                }
                else
                {
                    sb.AppendLine(
                        "The objective's electrical component " +
                        "could not be resolved.");

                    sb.AppendLine(
                        "Binding ID: " +
                        target.TargetComponentId);

                    sb.AppendLine(
                        "NEXT: Verify the runtime scene " +
                        "binding points to the intended " +
                        "installed component.");
                }

                return;
            }

            SparkElectricalState state =
                component.ElectricalState;

            sb.AppendLine(
                "Measured component: " + component.name);

            sb.AppendLine(
                "Voltage: " +
                Mathf.Abs(state.Voltage).ToString("F3") +
                " V");

            sb.AppendLine(
                "Current: " +
                (Mathf.Abs(state.Current) * 1000f)
                    .ToString("F3") + " mA");

            sb.AppendLine(
                "Power: " +
                (Mathf.Abs(state.Power) * 1000f)
                    .ToString("F3") + " mW");

            if (target.RequireElectricalEnabled &&
                !component.ElectricalEnabled)
            {
                sb.AppendLine(
                    "FAILED REQUIREMENT: " +
                    "Electrical component is disabled.");
            }

            if (target.RequireConduction &&
                state.Conduction !=
                    SparkConductionState.Conducting)
            {
                sb.AppendLine(
                    "FAILED REQUIREMENT: " +
                    "Required conduction is not present.");
            }

            switch (target.Type)
            {
                case SparkLevelTarget.TargetType.CurrentRange:
                    AppendRange(
                        sb,
                        Mathf.Abs(state.Current),
                        target.MinimumCurrent,
                        target.UseMaximumCurrent,
                        target.MaximumCurrent,
                        1000f,
                        "mA");
                    break;

                case SparkLevelTarget.TargetType.PowerRange:
                    AppendRange(
                        sb,
                        Mathf.Abs(state.Power),
                        target.MinimumPower,
                        target.UseMaximumPower,
                        target.MaximumPower,
                        1000f,
                        "mW");
                    break;

                case SparkLevelTarget.TargetType.LEDOn:
                    SparkLED led =
                        component.GetComponentInParent<SparkLED>();

                    if (led == null)
                    {
                        led =
                            component.GetComponentInChildren<SparkLED>();
                    }

                    if (led != null)
                    {
                        sb.AppendLine(
                            "LED is on: " + led.IsOn);

                        if (!led.IsOn)
                        {
                            sb.AppendLine(
                                "NEXT: Check LED polarity, " +
                                "its voltage/current conditions, " +
                                "and circuit continuity.");
                        }
                    }

                    AppendMinimums(sb, state, target);
                    break;

                case SparkLevelTarget.TargetType.ComponentPowered:
                case SparkLevelTarget.TargetType.ComponentConducting:
                    AppendMinimums(sb, state, target);

                    if (target.Type ==
                        SparkLevelTarget.TargetType.ComponentConducting &&
                        state.Conduction !=
                            SparkConductionState.Conducting)
                    {
                        sb.AppendLine(
                            "The component is not conducting.");
                    }
                    break;
            }

            if (target.Inverted)
            {
                sb.AppendLine(
                    "NOTE: This objective's final " +
                    "condition is inverted.");
            }
        }

        private static void AppendMinimums(
            StringBuilder sb,
            SparkElectricalState state,
            SparkLevelTarget target)
        {
            if (Mathf.Abs(state.Voltage) <
                target.MinimumVoltage)
            {
                sb.AppendLine(
                    "Voltage is below the required " +
                    "minimum of " +
                    target.MinimumVoltage.ToString("F3") +
                    " V.");
            }

            if (Mathf.Abs(state.Current) <
                target.MinimumCurrent)
            {
                sb.AppendLine(
                    "Current is below the required " +
                    "minimum of " +
                    (target.MinimumCurrent * 1000f)
                        .ToString("F3") + " mA.");
            }

            if (Mathf.Abs(state.Power) <
                target.MinimumPower)
            {
                sb.AppendLine(
                    "Power is below the required " +
                    "minimum of " +
                    (target.MinimumPower * 1000f)
                        .ToString("F3") + " mW.");
            }
        }

        private static void AppendRange(
            StringBuilder sb,
            float value,
            float minimum,
            bool hasMaximum,
            float maximum,
            float scale,
            string unit)
        {
            sb.AppendLine(
                "Required minimum: " +
                (minimum * scale).ToString("F3") +
                " " + unit);

            if (hasMaximum)
            {
                sb.AppendLine(
                    "Required maximum: " +
                    (maximum * scale).ToString("F3") +
                    " " + unit);
            }

            if (float.IsNaN(value) ||
                float.IsInfinity(value))
            {
                sb.AppendLine(
                    "INVALID MEASUREMENT: " +
                    "The solver value is not finite.");
                return;
            }

            if (value < minimum)
            {
                sb.AppendLine(
                    "BELOW TARGET BY: " +
                    ((minimum - value) * scale)
                        .ToString("F3") +
                    " " + unit);

                sb.AppendLine(
                    "NEXT: Investigate why the measured " +
                    "value is too low. Check supply, " +
                    "connections, and component values.");
            }
            else if (hasMaximum && value > maximum)
            {
                sb.AppendLine(
                    "ABOVE TARGET BY: " +
                    ((value - maximum) * scale)
                        .ToString("F3") +
                    " " + unit);

                sb.AppendLine(
                    "NEXT: Investigate why the measured " +
                    "value is too high. Review the circuit " +
                    "load and component ratings.");
            }
            else
            {
                sb.AppendLine(
                    "Numeric range: SATISFIED.");

                sb.AppendLine(
                    "If the objective is pending, check " +
                    "other required conditions and " +
                    "whether this is the intended binding.");
            }
        }

        private static void AppendVoltageRange(
            StringBuilder sb,
            SparkLevelSceneBindings bindings,
            SparkLevelTarget target)
        {
            float voltage = 0f;
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
                            "FAILED REQUIREMENT: " +
                            "One or both terminals are disabled.");
                    }
                }
            }
            else if (!string.IsNullOrWhiteSpace(
                         target.TargetComponentId))
            {
                if (bindings.TryGetComponent(
                        target.TargetComponentId,
                        out SparkElectricalComponent component) &&
                    component != null)
                {
                    voltage = Mathf.Abs(
                        component.ElectricalState.Voltage);

                    resolved = true;

                    if (target.RequireElectricalEnabled &&
                        !component.ElectricalEnabled)
                    {
                        sb.AppendLine(
                            "FAILED REQUIREMENT: " +
                            "Component is disabled.");
                    }

                    if (target.RequireConduction &&
                        component.ElectricalState.Conduction !=
                            SparkConductionState.Conducting)
                    {
                        sb.AppendLine(
                            "FAILED REQUIREMENT: " +
                            "Component is not conducting.");
                    }
                }
            }
            else if (bindings.TryGetTerminal(
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
                        "FAILED REQUIREMENT: " +
                        "Terminal is disabled.");
                }
            }

            if (!resolved)
            {
                sb.AppendLine(
                    "Voltage target binding is unresolved.");
                return;
            }

            sb.AppendLine(
                "Measured voltage: " +
                voltage.ToString("F3") + " V");

            AppendRange(
                sb,
                voltage,
                target.MinimumVoltage,
                target.UseMaximumVoltage,
                target.MaximumVoltage,
                1f,
                "V");
        }

       
private static void AppendTerminalEvidence(
    StringBuilder sb,
    SparkLevelSceneBindings bindings,
    SparkLevelTarget target)
{
    if (!bindings.TryGetTerminal(
            target.TargetTerminalId,
            out SparkTerminal terminal) ||
        terminal == null)
    {
        sb.AppendLine(
            "Target terminal is unresolved: " +
            target.TargetTerminalId);

        return;
    }

    // IMPORTANT:
    // SparkTerminal has SparkTerminalElectricalState,
    // not SparkElectricalState.
    SparkTerminalElectricalState state =
        terminal.ElectricalState;

    float voltage = Mathf.Abs(state.Voltage);
    float current = Mathf.Abs(state.Current);
    float power = Mathf.Abs(state.Power);

    sb.AppendLine(
        "Target terminal: " + terminal.name);

    sb.AppendLine(
        "Voltage: " +
        voltage.ToString("F3") + " V");

    sb.AppendLine(
        "Current: " +
        (current * 1000f).ToString("F3") + " mA");

    sb.AppendLine(
        "Power: " +
        (power * 1000f).ToString("F3") + " mW");

    if (target.RequireElectricalEnabled &&
        !terminal.IsElectricalEnabled)
    {
        sb.AppendLine(
            "FAILED REQUIREMENT: " +
            "Target terminal is electrically disabled.");
    }

    if (voltage < target.MinimumVoltage)
    {
        sb.AppendLine(
            "Voltage is below the minimum of " +
            target.MinimumVoltage.ToString("F3") +
            " V.");
    }

    if (current < target.MinimumCurrent)
    {
        sb.AppendLine(
            "Current is below the minimum of " +
            (target.MinimumCurrent * 1000f)
                .ToString("F3") +
            " mA.");
    }

    if (power < target.MinimumPower)
    {
        sb.AppendLine(
            "Power is below the minimum of " +
            (target.MinimumPower * 1000f)
                .ToString("F3") +
            " mW.");
    }
}

    }
}
