using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectSpark.AI
{
    /// <summary>
    /// Converts the unified deterministic AI observation into a
    /// higher-level educational context.
    ///
    /// This is still deterministic reasoning.
    ///
    /// It does NOT:
    /// - modify the circuit
    /// - modify the electrical solver
    /// - complete/fail levels
    /// - control the player
    /// - invent electrical measurements
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkAIContextReasoner : MonoBehaviour
    {
        [Header("AI Observation")]
        [SerializeField]
        private SparkAIObservationCoordinator observationCoordinator;

        private SparkAIContextReasoning currentContext;

        private bool initialized;

        public bool IsInitialized =>
            initialized;

        public SparkAIContextReasoning CurrentContext =>
            currentContext;

        private void Awake()
        {
            if (observationCoordinator == null)
            {
                Debug.LogError(
                    "[Spark AI Context Reasoner] " +
                    "SparkAIObservationCoordinator reference is missing.",
                    this);

                return;
            }

            initialized = true;
        }

        /// <summary>
        /// Reasons about the latest Project Spark world.
        /// </summary>
        public SparkAIContextReasoning
            ReasonCurrentContext()
        {
            if (!initialized)
            {
                currentContext =
                    SparkAIContextReasoning.Invalid();

                return currentContext;
            }

            SparkAIUnifiedObservation observation =
                observationCoordinator.ObserveCurrentWorld();



              /*  if (observation.World.ElectronicObjects != null)
            {
                Debug.Log(
                    "[SPARK AI WORLD OBJECTS] Count=" +
                    observation.World.ElectronicObjects.Count);

                for (int i = 0;
                    i < observation.World.ElectronicObjects.Count;
                    i++)
                {
                    SparkAIElectronicObjectSnapshot item =
                        observation.World.ElectronicObjects[i];

                    Debug.Log(
                        "[SPARK AI WORLD OBJECT] Index=" + i +
                        " | Name=" + item.Name);
                }
            }
            else
            {
                Debug.Log("[SPARK AI WORLD OBJECTS] ElectronicObjects is NULL");
            }*/


            return Reason(observation);
        }

        /// <summary>
        /// Converts one unified observation into an educational context.
        /// </summary>
        public SparkAIContextReasoning Reason(
            SparkAIUnifiedObservation observation)
        {
            if (!initialized ||
                !observation.IsValid)
            {
                currentContext =
                    SparkAIContextReasoning.Invalid();

                return currentContext;
            }

            SparkAIContextState state =
                DetermineContextState(
                    observation);

            SparkAIContextFocus focus =
                DetermineFocus(
                    observation,
                    state);

            string summary =
                BuildSummary(
                    observation,
                    state,
                    focus);

            string explanation =
                BuildExplanation(
                    observation,
                    state,
                    focus);

            string teachingGoal =
                BuildTeachingGoal(
                    observation,
                    state,
                    focus);

            string[] evidence =
                BuildEvidence(
                    observation);

            string[] misconceptions =
                BuildPossibleMisconceptions(
                    observation,
                    state);

            string[] nextSteps =
                BuildNextSteps(
                    observation,
                    state,
                    focus);

            currentContext =
                new SparkAIContextReasoning(
                    true,
                    observation.CapturedAt,
                    state,
                    focus,
                    summary,
                    explanation,
                    teachingGoal,
                    evidence,
                    misconceptions,
                    nextSteps);

            return currentContext;
        }

        private SparkAIContextState DetermineContextState(
            SparkAIUnifiedObservation observation)
        {
            SparkAICircuitReasoning circuit =
                observation.CircuitReasoning;

            SparkAICircuitAnalysis analysis =
                observation.CircuitAnalysis;

            if (!analysis.IsValid)
            {
                return SparkAIContextState.Unknown;
            }

            if (analysis.SourceShorted ||
                circuit.TopologyState ==
                SparkAICircuitTopologyState.ShortCircuit)
            {
                return SparkAIContextState.ShortCircuit;
            }

            if (!analysis.HasSource)
            {
                return SparkAIContextState.NoPowerSource;
            }

            if (!analysis.HasPositiveSource ||
                !analysis.HasNegativeSource)
            {
                return SparkAIContextState.IncompletePowerSource;
            }

            if (circuit.TopologyState ==
                SparkAICircuitTopologyState.NoConnections)
            {
                return SparkAIContextState.NoConnections;
            }

            if (circuit.TopologyState ==
                SparkAICircuitTopologyState.OpenCircuit)
            {
                return SparkAIContextState.OpenCircuit;
            }

            if (circuit.TopologyState ==
                SparkAICircuitTopologyState.ClosedCircuit)
            {
                if (HasVoltageWithoutCurrent(
                        observation))
                {
                    return SparkAIContextState.VoltageWithoutUsefulCurrent;
                }

                if (HasActiveElectricalComponent(
                        observation))
                {
                    return SparkAIContextState.ActiveCircuit;
                }

                return SparkAIContextState.ClosedCircuit;
            }

            return SparkAIContextState.ObservingCircuit;
        }

        private SparkAIContextFocus DetermineFocus(
            SparkAIUnifiedObservation observation,
            SparkAIContextState state)
        {
            if (observation.World != null &&
                observation.World.Player.HasSelection)
            {
                string selectedName =
                    observation.World.Player.SelectedObjectName;

                if (!string.IsNullOrWhiteSpace(selectedName))
                {
                    if (Contains(
                            selectedName,
                            "switch"))
                    {
                        return SparkAIContextFocus.Switch;
                    }

                    if (Contains(
                            selectedName,
                            "resistor"))
                    {
                        return SparkAIContextFocus.Resistance;
                    }

                    if (Contains(
                            selectedName,
                            "led") ||
                        Contains(
                            selectedName,
                            "diode"))
                    {
                        return SparkAIContextFocus.Polarity;
                    }

                    if (Contains(
                            selectedName,
                            "battery") ||
                        Contains(
                            selectedName,
                            "supply") ||
                        Contains(
                            selectedName,
                            "source"))
                    {
                        return SparkAIContextFocus.VoltageSource;
                    }

                    if (Contains(
                            selectedName,
                            "multimeter") ||
                        Contains(
                            selectedName,
                            "meter"))
                    {
                        return SparkAIContextFocus.Measurement;
                    }
                }
            }

            switch (state)
            {
                case SparkAIContextState.NoPowerSource:
                case SparkAIContextState.IncompletePowerSource:

                    return SparkAIContextFocus.PowerSource;

                case SparkAIContextState.OpenCircuit:

                    return SparkAIContextFocus.CircuitPath;

                case SparkAIContextState.ShortCircuit:

                    return SparkAIContextFocus.ShortCircuit;

                case SparkAIContextState.VoltageWithoutUsefulCurrent:

                    return SparkAIContextFocus.CurrentFlow;

                case SparkAIContextState.ActiveCircuit:

                    return SparkAIContextFocus.ElectricalBehavior;

                default:

                    return SparkAIContextFocus.CircuitBasics;
            }
        }

        private string BuildSummary(
            SparkAIUnifiedObservation observation,
            SparkAIContextState state,
            SparkAIContextFocus focus)
        {
            string stateText =
                GetContextStateText(state);

            string focusText =
                GetFocusText(focus);

            return
                $"Current learning context: {stateText}. " +
                $"Primary focus: {focusText}.";
        }

        private string BuildExplanation(
            SparkAIUnifiedObservation observation,
            SparkAIContextState state,
            SparkAIContextFocus focus)
        {
            switch (state)
            {
                case SparkAIContextState.NoPowerSource:

                    return
                        "The circuit does not currently have an identified electrical source. " +
                        "Before studying current flow, the player needs a source that can establish an electrical potential difference.";

                case SparkAIContextState.IncompletePowerSource:

                    return
                        "A possible source is present, but the positive and negative source terminals have not both been identified as available to the circuit.";

                case SparkAIContextState.NoConnections:

                    return
                        "The circuit contains electrical objects, but there are not enough connections to form a useful circuit.";

                case SparkAIContextState.OpenCircuit:

                    return
                        "The circuit does not currently provide a complete conductive path between the identified source terminals. " +
                        "A break in the path can prevent useful current flow.";

                case SparkAIContextState.ShortCircuit:

                    return
                        "The source appears to have a low-resistance direct path that bypasses the intended load. " +
                        "This is different from building a useful circuit through a component.";

                case SparkAIContextState.VoltageWithoutUsefulCurrent:

                    return
                        "Voltage is present on at least one electrical component, but significant current is not being observed there. " +
                        "This is an opportunity to understand the difference between voltage and current.";

                case SparkAIContextState.ClosedCircuit:

                    return
                        "A complete connection path exists between the identified source terminals. " +
                        "The player can now observe how a closed path enables electrical behavior.";

                case SparkAIContextState.ActiveCircuit:

                    return
                        "The circuit has a connected path and active electrical behavior is being observed. " +
                        "This is a useful state for connecting voltage, current, resistance, and power concepts.";

                case SparkAIContextState.ObservingCircuit:

                    return
                        "The circuit is being observed, but the available evidence is not sufficient to classify a stronger electrical behavior.";

                default:

                    return
                        "The AI does not yet have enough reliable information to form a specific educational interpretation.";
            }
        }

        private string BuildTeachingGoal(
            SparkAIUnifiedObservation observation,
            SparkAIContextState state,
            SparkAIContextFocus focus)
        {
            switch (focus)
            {
                case SparkAIContextFocus.PowerSource:

                    return
                        "Understand that a source establishes an electrical potential difference.";

                case SparkAIContextFocus.CircuitPath:

                    return
                        "Understand that a useful circuit needs a complete conductive path.";

                case SparkAIContextFocus.CurrentFlow:

                    return
                        "Understand that voltage and current are related but are not the same thing.";

                case SparkAIContextFocus.ShortCircuit:

                    return
                        "Understand why a direct low-resistance path can bypass the intended load.";

                case SparkAIContextFocus.Switch:

                    return
                        "Understand how opening and closing a switch changes circuit continuity.";

                case SparkAIContextFocus.Resistance:

                    return
                        "Understand how resistance affects electrical current and power.";

                case SparkAIContextFocus.Polarity:

                    return
                        "Understand why polarity matters for polarity-sensitive components.";

                case SparkAIContextFocus.Measurement:

                    return
                        "Understand what the measuring instrument is observing and why the measurement depends on how it is connected.";

                case SparkAIContextFocus.VoltageSource:

                    return
                        "Understand how the source establishes voltage in the circuit.";

                case SparkAIContextFocus.ElectricalBehavior:

                    return
                        "Connect observed voltage, current, and power to the behavior of the component.";

                default:

                    return
                        "Understand the basic relationship between source, connection, load, voltage, and current.";
            }
        }

        private string[] BuildEvidence(
            SparkAIUnifiedObservation observation)
        {
            List<string> evidence =
                new List<string>(12);

            SparkAICircuitAnalysis circuit =
                observation.CircuitAnalysis;

            evidence.Add(
                $"Topology: {circuit.TopologyState}.");

            evidence.Add(
                $"Source detected: {circuit.HasSource}.");

            evidence.Add(
                $"Positive source terminal detected: {circuit.HasPositiveSource}.");

            evidence.Add(
                $"Negative source terminal detected: {circuit.HasNegativeSource}.");

            evidence.Add(
                $"Return path closed: {circuit.ReturnPathClosed}.");

            evidence.Add(
                $"Connections: {circuit.ConnectionCount}.");

            evidence.Add(
                $"Terminals: {circuit.TerminalCount}.");

            evidence.Add(
                $"Reachable terminals: {circuit.ReachableTerminalCount}.");

            if (observation.PathObservation.IsValid)
            {
                evidence.Add(
                    $"Source path exists: {observation.PathObservation.PathExists}.");
            }

            if (observation.World != null &&
                observation.World.Player.HasSelection)
            {
                evidence.Add(
                    $"Selected object: {observation.World.Player.SelectedObjectName}.");
            }

            return evidence.ToArray();
        }

        private string[] BuildPossibleMisconceptions(
            SparkAIUnifiedObservation observation,
            SparkAIContextState state)
        {
            List<string> misconceptions =
                new List<string>(4);

            switch (state)
            {
                case SparkAIContextState.VoltageWithoutUsefulCurrent:

                    misconceptions.Add(
                        "The player may assume that voltage automatically means current must be flowing.");

                    misconceptions.Add(
                        "The player may not yet distinguish electrical potential difference from charge flow.");

                    break;

                case SparkAIContextState.OpenCircuit:

                    misconceptions.Add(
                        "The player may think that connecting one side of a component is enough to create a working circuit.");

                    break;

                case SparkAIContextState.ShortCircuit:

                    misconceptions.Add(
                        "The player may interpret any closed connection as a useful circuit.");

                    misconceptions.Add(
                        "The player may not yet understand that the intended load should be part of the current path.");

                    break;

                case SparkAIContextState.NoPowerSource:

                    misconceptions.Add(
                        "The player may be trying to build a circuit without first establishing a source.");

                    break;

                case SparkAIContextState.IncompletePowerSource:

                    misconceptions.Add(
                        "The player may not yet understand that a source requires a usable positive and negative path.");

                    break;
            }

            return misconceptions.ToArray();
        }

        private string[] BuildNextSteps(
            SparkAIUnifiedObservation observation,
            SparkAIContextState state,
            SparkAIContextFocus focus)
        {
            List<string> steps =
                new List<string>(5);

            switch (state)
            {
                case SparkAIContextState.NoPowerSource:

                    steps.Add(
                        "Place or activate a valid power source.");

                    steps.Add(
                        "Identify the positive and negative terminals.");

                    break;

                case SparkAIContextState.IncompletePowerSource:

                    steps.Add(
                        "Inspect the source terminals.");

                    steps.Add(
                        "Connect the intended positive and negative paths.");

                    break;

                case SparkAIContextState.NoConnections:

                    steps.Add(
                        "Connect the source to the intended component.");

                    break;

                case SparkAIContextState.OpenCircuit:

                    steps.Add(
                        "Trace the path from the positive source terminal.");

                    steps.Add(
                        "Find the point where the return path is interrupted.");

                    break;

                case SparkAIContextState.ShortCircuit:

                    steps.Add(
                        "Find the low-resistance path bypassing the intended load.");

                    steps.Add(
                        "Restore the intended component into the current path.");

                    break;

                case SparkAIContextState.VoltageWithoutUsefulCurrent:

                    steps.Add(
                        "Inspect the complete circuit path.");

                    steps.Add(
                        "Compare the component's voltage and current observations.");

                    break;

                case SparkAIContextState.ClosedCircuit:

                    steps.Add(
                        "Observe which components now have voltage and current.");

                    break;

                case SparkAIContextState.ActiveCircuit:

                    steps.Add(
                        "Compare voltage, current, resistance, and power.");

                    steps.Add(
                        "Observe how changing the circuit changes electrical behavior.");

                    break;

                default:

                    steps.Add(
                        "Inspect the circuit connections and component states.");

                    break;
            }

            return steps.ToArray();
        }

        private bool HasVoltageWithoutCurrent(
            SparkAIUnifiedObservation observation)
        {
            IReadOnlyList<SparkAIElectricalReasoning>
                electrical =
                    observation.ElectricalReasonings;

            if (electrical == null)
                return false;

            for (int i = 0;
                 i < electrical.Count;
                 i++)
            {
                SparkAIElectricalReasoning reasoning =
                    electrical[i];

                if (reasoning.State ==
                    SparkAIElectricalState.VoltageWithoutCurrent)
                {
                    return true;
                }
            }

            return false;
        }

        private bool HasActiveElectricalComponent(
            SparkAIUnifiedObservation observation)
        {
            IReadOnlyList<SparkAIElectricalReasoning>
                electrical =
                    observation.ElectricalReasonings;

            if (electrical == null)
                return false;

            for (int i = 0;
                 i < electrical.Count;
                 i++)
            {
                SparkAIElectricalReasoning reasoning =
                    electrical[i];

                if (reasoning.HasCurrent ||
                    reasoning.HasPower)
                {
                    return true;
                }
            }

            return false;
        }

        private bool Contains(
            string value,
            string search)
        {
            return
                !string.IsNullOrEmpty(value) &&
                !string.IsNullOrEmpty(search) &&
                value.IndexOf(
                    search,
                    StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private string GetContextStateText(
            SparkAIContextState state)
        {
            switch (state)
            {
                case SparkAIContextState.NoPowerSource:
                    return "no power source";

                case SparkAIContextState.IncompletePowerSource:
                    return "incomplete power source";

                case SparkAIContextState.NoConnections:
                    return "no useful connections";

                case SparkAIContextState.OpenCircuit:
                    return "open circuit";

                case SparkAIContextState.ShortCircuit:
                    return "short circuit";

                case SparkAIContextState.VoltageWithoutUsefulCurrent:
                    return "voltage without useful current";

                case SparkAIContextState.ClosedCircuit:
                    return "closed circuit";

                case SparkAIContextState.ActiveCircuit:
                    return "active electrical circuit";

                case SparkAIContextState.ObservingCircuit:
                    return "circuit under observation";

                default:
                    return "unknown electrical context";
            }
        }

        private string GetFocusText(
            SparkAIContextFocus focus)
        {
            switch (focus)
            {
                case SparkAIContextFocus.PowerSource:
                    return "power source";

                case SparkAIContextFocus.VoltageSource:
                    return "voltage source";

                case SparkAIContextFocus.CircuitPath:
                    return "circuit path";

                case SparkAIContextFocus.CurrentFlow:
                    return "current flow";

                case SparkAIContextFocus.ShortCircuit:
                    return "short circuit";

                case SparkAIContextFocus.Switch:
                    return "switch behavior";

                case SparkAIContextFocus.Resistance:
                    return "resistance";

                case SparkAIContextFocus.Polarity:
                    return "polarity";

                case SparkAIContextFocus.Measurement:
                    return "measurement";

                case SparkAIContextFocus.ElectricalBehavior:
                    return "electrical behavior";

                default:
                    return "circuit fundamentals";
            }
        }
    }

    public enum SparkAIContextState
    {
        Unknown = 0,
        NoPowerSource = 1,
        IncompletePowerSource = 2,
        NoConnections = 3,
        OpenCircuit = 4,
        ShortCircuit = 5,
        VoltageWithoutUsefulCurrent = 6,
        ClosedCircuit = 7,
        ActiveCircuit = 8,
        ObservingCircuit = 9
    }

    public enum SparkAIContextFocus
    {
        CircuitBasics = 0,
        PowerSource = 1,
        VoltageSource = 2,
        CircuitPath = 3,
        CurrentFlow = 4,
        ShortCircuit = 5,
        Switch = 6,
        Resistance = 7,
        Polarity = 8,
        Measurement = 9,
        ElectricalBehavior = 10
    }

    /// <summary>
    /// Higher-level educational interpretation of the current
    /// deterministic AI observation.
    /// </summary>
    public readonly struct SparkAIContextReasoning
    {
        public bool IsValid { get; }

        public float CapturedAt { get; }

        public SparkAIContextState State { get; }

        public SparkAIContextFocus Focus { get; }

        public string Summary { get; }

        public string Explanation { get; }

        public string TeachingGoal { get; }

        public string[] Evidence { get; }

        public string[] PossibleMisconceptions { get; }

        public string[] NextSteps { get; }

        public SparkAIContextReasoning(
            bool isValid,
            float capturedAt,
            SparkAIContextState state,
            SparkAIContextFocus focus,
            string summary,
            string explanation,
            string teachingGoal,
            string[] evidence,
            string[] possibleMisconceptions,
            string[] nextSteps)
        {
            IsValid = isValid;
            CapturedAt = capturedAt;
            State = state;
            Focus = focus;
            Summary = summary ?? string.Empty;
            Explanation = explanation ?? string.Empty;
            TeachingGoal = teachingGoal ?? string.Empty;

            Evidence =
                evidence ??
                Array.Empty<string>();

            PossibleMisconceptions =
                possibleMisconceptions ??
                Array.Empty<string>();

            NextSteps =
                nextSteps ??
                Array.Empty<string>();
        }

        public static SparkAIContextReasoning Invalid()
        {
            return new SparkAIContextReasoning(
                false,
                Time.time,
                SparkAIContextState.Unknown,
                SparkAIContextFocus.CircuitBasics,
                string.Empty,
                string.Empty,
                string.Empty,
                Array.Empty<string>(),
                Array.Empty<string>(),
                Array.Empty<string>());
        }
    }
}