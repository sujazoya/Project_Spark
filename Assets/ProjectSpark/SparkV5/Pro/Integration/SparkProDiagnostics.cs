using System;
using System.Linq;
using ProjectSpark.AI.Tutorial;
using ProjectSpark.Gameplay;

namespace ProjectSpark.SparkV5.Pro
{
    internal static class SparkProDiagnostics
    {
        public static string Explain(SparkAILevelContext context, SparkElectronicObject selection, string question, bool brief)
        {
            if (!context.Definition) return "No mission is loaded. Open a level or ask for a theoretical example.";
            if (!context.HasEvaluation) return "The level evaluator has not published results for this workbench.";
            var manager = context.Manager;
            var solver = manager.ElectricalSolver;
            if (!solver || solver.IsDirty || solver.IsSolving || solver.LatestSolveResult == null)
                return "Fresh electrical evidence is unavailable. Let the simulation settle, then take a new reading.";
            var target = context.NextTarget();
            string goal = target == null ? "" : $"Next objective — {SparkAILevelContext.TargetName(target)}. ";
            string prefix = brief ? "" : goal;
            bool evaluated = SparkPhase4EvidenceGuard.EvaluatorIsCurrent(context);
            if (evaluated && (manager.SourceShorted || manager.TargetShorted))
                return "Verified by the evaluator: a short is active. Isolate the virtual supply before tracing the unintended path. Begin with continuity between supply and return while de-energized.";
            if (evaluated && manager.Overloaded)
                return "Verified by the evaluator: the circuit is overloaded. Isolate power, then inspect the load and compare its rating with the supply current limit.";
            if (selection is SparkLED led && led.IsReverseVoltageExceeded)
                return prefix + $"Verified by the solver: {led.name} exceeds its reverse-voltage limit. Isolate the training supply, then compare the installed leads to the authored anode and cathode marks.";
            if (selection is SparkDiode diode && diode.IsReverseFault)
                return prefix + $"Verified by the solver: {diode.name} exceeds its reverse-voltage limit. Check the terminal orientation with the training supply isolated.";
            if (selection is SparkSwitch control && control.IsOpen)
                return prefix + $"Verified switch state: {control.name} is open. Identify the intended return path before testing continuity with the supply isolated.";
            if (evaluated && manager.WrongConnection)
                return prefix + "The evaluator reports an incorrect connection. An incorrect endpoint or reversed polarity is possible. Inspect the authored terminal marks; then compare the wire endpoints with the objective.";
            var bindings = context.Layout.SceneBindings;
            var selectedSupply = selection as SparkPowerSupply;
            if (selectedSupply && !selectedSupply.IsOutputActive)
                return $"Verified setting: {selectedSupply.name} output is off. Verify the bench setup and permitted power state before enabling it.";
            if (evaluated && !manager.HasValidPowerSource)
                return prefix + "The evaluator has not found a qualifying supply. The output may be disabled or its terminals may be unbound. Verify the supply output across its own positive and return terminals first.";
            if (evaluated && context.Definition.RequireClosedReturn && !manager.ClosedReturn)
                return prefix + "Verified by the evaluator: the required return path is open. With supply isolated, trace continuity from the load return back to supply return.";
            if (!evaluated) return prefix + "A solver reading is available, but the objective evaluator has not verified this exact circuit revision.";
            if (target == null) return context.StateText();
            string positiveId = target.RequiredPositiveTerminalId;
            string negativeId = target.RequiredNegativeTerminalId;
            if (target.HasRequiredConnectionPair && bindings.TryGetTerminal(positiveId, out var positive) &&
                bindings.TryGetTerminal(negativeId, out var negative))
            {
                if (!manager.CircuitSystem.HasConnectionBetween(positive, negative))
                    return prefix + $"Topology shows no direct connection between {positive.name} and {negative.name}. Compare these endpoints with the objective. Inspect the existing route before changing it.";
            }
            if (!string.IsNullOrEmpty(target.TargetComponentId) && bindings.TryGetComponent(target.TargetComponentId, out var part))
            {
                if (part is SparkResistor resistor && solver.TryCalculateComponentCurrent(resistor, out float amps) && float.IsFinite(amps) &&
                    (target.MinimumCurrent > 0 && Math.Abs(amps) < target.MinimumCurrent ||
                    target.UseMaximumCurrent && Math.Abs(amps) > target.MaximumCurrent))
                    return prefix + $"The solved current through {part.name} is {Math.Abs(amps) * 1000:G4} mA. Its configured resistance is {resistor.Resistance:G4} Ω. Check the voltage across it before assuming the resistance is wrong.";
                if (selection == part && question.Contains("polarity", StringComparison.OrdinalIgnoreCase))
                    return prefix + $"Inspect the authored polarity on {part.name}. Swap meter-probe positions only after checking the two terminal labels; a negative voltage may just mean the probes are reversed.";
            }
            if (question.Contains("meter", StringComparison.OrdinalIgnoreCase) || question.Contains("probe", StringComparison.OrdinalIgnoreCase))
                return "For a voltage check choose DC volts and place the probes across the two specified terminals. Current uses a series connection. Continuity and resistance require the virtual supply to be isolated.";
            if (question.Contains("rail", StringComparison.OrdinalIgnoreCase))
            {
                var evidence = context.Layout.GetComponentsInChildren<SparkTerminal>()
                    .Where(pin => pin.name.IndexOf("3V3", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        pin.name.IndexOf("5V", StringComparison.OrdinalIgnoreCase) >= 0).Take(3).Select(pin => pin.name).ToArray();
                return prefix + (evidence.Length == 0 ? "No named rail was identified in the current layout." :
                    $"Candidate rail test points in this layout are {string.Join(", ", evidence)}. Confirm the correct return from the board schematic.") +
                    " Measure the input supply first, then the affected rail relative to its documented return.";
            }
            string requirement = SparkAILevelContext.TargetConditions(target);
            return prefix + (string.IsNullOrWhiteSpace(requirement) ? target.Description : requirement) +
                " The evaluator has yet to pass this objective. Start with the first required check.";
        }

        public static string Measure(SparkAILevelContext context, SparkElectronicObject selected)
        {
            var solver = context.HasEvaluation ? context.Manager.ElectricalSolver : null;
            if (!solver || solver.IsDirty || solver.IsSolving || solver.LatestSolveResult == null)
                return "A fresh solve for the current level is required before reporting a reading.";
            if (selected is not SparkElectricalComponent component || !component.ElectricalEnabled ||
                !solver.LatestSolveResult.Context.Graph.TryGetComponentTerminals(component, out var terminals) || terminals.Length != 2 ||
                !solver.LatestSolveResult.TryGetVoltage(terminals[0], terminals[1], out float voltage) || !float.IsFinite(voltage))
                return "Select a solved two-terminal component, or use the multimeter to choose an explicit reference pair.";
            return $"Current solver result for {component.name}: {voltage:G4} V ({terminals[0].name} relative to {terminals[1].name}). A meter reading depends on the chosen probes and mode.";
        }
    }
}
