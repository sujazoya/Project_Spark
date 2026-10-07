using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectSpark.AI
{
    /// <summary>
    /// Deterministic AI-side analyzer for Project Spark components.
    ///
    /// Converts observed electronic-object snapshots into
    /// educational component observations.
    ///
    /// IMPORTANT:
    /// This component is observational only.
    ///
    /// It does NOT:
    /// - modify components
    /// - modify SparkCircuitSystem
    /// - modify SparkElectricalSolver
    /// - calculate authoritative electrical values
    /// - change component states
    /// - replace level evaluation
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkAIComponentReasoner : MonoBehaviour
    {
        [Header("Spark AI")]
        [SerializeField]
        private SparkAIWorld world;

        private readonly List<SparkAIComponentReasoning> results =
            new List<SparkAIComponentReasoning>(32);

        private bool initialized;

        public bool IsInitialized =>
            initialized;

        public IReadOnlyList<SparkAIComponentReasoning> CurrentResults =>
            results;

        private void Awake()
        {
            if (world == null)
            {
                Debug.LogError(
                    "[Spark AI Component Reasoner] " +
                    "SparkAIWorld reference is missing.",
                    this);

                return;
            }

            initialized = true;
        }

        /// <summary>
        /// Analyzes all currently observed electronic objects.
        /// </summary>
        public IReadOnlyList<SparkAIComponentReasoning>
            AnalyzeCurrentComponents()
        {
            if (!initialized)
            {
                results.Clear();
                return results;
            }

            return Analyze(
                world.LatestSnapshot);
        }

        /// <summary>
        /// Analyzes components from a supplied world snapshot.
        /// </summary>
        public IReadOnlyList<SparkAIComponentReasoning>
            Analyze(
                SparkAIWorldSnapshot snapshot)
        {
            results.Clear();

            if (!initialized ||
                snapshot == null)
            {
                return results;
            }

            IReadOnlyList<SparkAIElectronicObjectSnapshot>
                objects =
                    snapshot.ElectronicObjects;

            if (objects == null)
                return results;

            for (int i = 0;
                 i < objects.Count;
                 i++)
            {
                SparkAIElectronicObjectSnapshot component =
                    objects[i];

                SparkAIComponentReasoning reasoning =
                    AnalyzeComponent(
                        component);

                if (!reasoning.IsValid)
                    continue;

                results.Add(reasoning);
            }

            return results;
        }

        /// <summary>
        /// Finds the AI reasoning result for one component.
        /// </summary>
        public bool TryAnalyzeComponent(
            SparkAIWorldSnapshot snapshot,
            int instanceId,
            out SparkAIComponentReasoning reasoning)
        {
            reasoning =
                default;

            if (!initialized ||
                snapshot == null ||
                instanceId == 0)
            {
                return false;
            }

            IReadOnlyList<SparkAIElectronicObjectSnapshot>
                objects =
                    snapshot.ElectronicObjects;

            if (objects == null)
                return false;

            for (int i = 0;
                 i < objects.Count;
                 i++)
            {
                SparkAIElectronicObjectSnapshot component =
                    objects[i];

                if (component.InstanceId != instanceId)
                    continue;

                reasoning =
                    AnalyzeComponent(
                        component);

                return reasoning.IsValid;
            }

            return false;
        }

        private SparkAIComponentReasoning AnalyzeComponent(
            SparkAIElectronicObjectSnapshot component)
        {
            if (component.InstanceId == 0)
            {
                return SparkAIComponentReasoning.Invalid();
            }

            string componentType =
                IdentifyComponentType(
                    component);

            SparkAIComponentState state =
                DetermineState(
                    component);

            string summary =
                BuildSummary(
                    component,
                    componentType,
                    state);

            string educationalMeaning =
                BuildEducationalMeaning(
                    componentType,
                    state);

            string[] observations =
                BuildObservations(
                    component,
                    componentType,
                    state);

            string[] suggestions =
                BuildSuggestions(
                    component,
                    componentType,
                    state);

            return new SparkAIComponentReasoning(
                true,
                Time.time,
                component.InstanceId,
                component.Name,
                componentType,
                state,
                component.Voltage,
                component.Current,
                component.Power,
                component.ElectricalEnabled,
                component.Active,
                summary,
                educationalMeaning,
                observations,
                suggestions);
        }

        /// <summary>
        /// Determines a broad component category.
        ///
        /// This intentionally uses the observed object name/type
        /// rather than creating dependencies on concrete component
        /// classes.
        /// </summary>
        private string IdentifyComponentType(
            SparkAIElectronicObjectSnapshot component)
        {
            string name =
                component.Name ?? string.Empty;

            string operationalState =
                component.OperationalState ??
                string.Empty;

            string conductionState =
                component.ConductionState ??
                string.Empty;

            if (ContainsAny(
                    name,
                    "power",
                    "supply",
                    "source",
                    "battery"))
            {
                return "PowerSource";
            }

            if (ContainsAny(
                    name,
                    "switch",
                    "toggle"))
            {
                return "Switch";
            }

            if (ContainsAny(
                    name,
                    "resistor",
                    "resistance",
                    "res"))
            {
                return "Resistor";
            }

            if (ContainsAny(
                    name,
                    "led",
                    "lightemitting",
                    "diode"))
            {
                return "DiodeOrLED";
            }

            if (ContainsAny(
                    name,
                    "capacitor",
                    "cap"))
            {
                return "Capacitor";
            }

            if (ContainsAny(
                    name,
                    "motor"))
            {
                return "Motor";
            }

            if (ContainsAny(
                    name,
                    "heater",
                    "heating"))
            {
                return "Heater";
            }

            if (ContainsAny(
                    name,
                    "multimeter",
                    "meter"))
            {
                return "MeasurementDevice";
            }

            if (!string.IsNullOrWhiteSpace(
                    conductionState))
            {
                return "ElectricalComponent";
            }

            if (!string.IsNullOrWhiteSpace(
                    operationalState))
            {
                return "ElectronicObject";
            }

            return "UnknownComponent";
        }

        private SparkAIComponentState DetermineState(
            SparkAIElectronicObjectSnapshot component)
        {
            if (!component.Active)
                return SparkAIComponentState.Inactive;

            if (!component.InteractionsEnabled)
                return SparkAIComponentState.InteractionDisabled;

            if (!component.IsElectricalComponent)
                return SparkAIComponentState.NonElectrical;

            if (!component.ElectricalEnabled)
                return SparkAIComponentState.ElectricalDisabled;

            if (IsClosedState(
                    component.ConductionState))
            {
                return SparkAIComponentState.Conducting;
            }

            if (IsOpenState(
                    component.ConductionState))
            {
                return SparkAIComponentState.NonConducting;
            }

            if (Mathf.Abs(component.Current) > 0.000001f)
            {
                return SparkAIComponentState.CurrentFlowing;
            }

            if (Mathf.Abs(component.Voltage) > 0.000001f)
            {
                return SparkAIComponentState.VoltagePresent;
            }

            return SparkAIComponentState.Idle;
        }

        private string BuildSummary(
            SparkAIElectronicObjectSnapshot component,
            string componentType,
            SparkAIComponentState state)
        {
            if (!string.IsNullOrWhiteSpace(
                    component.Name))
            {
                return
                    $"{component.Name} is observed as a {componentType} and is currently {GetStateText(state)}.";
            }

            return
                $"The component is observed as a {componentType} and is currently {GetStateText(state)}.";
        }

        private string BuildEducationalMeaning(
            string componentType,
            SparkAIComponentState state)
        {
            switch (componentType)
            {
                case "PowerSource":
                    return
                        "A power source provides electrical potential that can drive current through a connected circuit.";

                case "Switch":
                    return
                        "A switch controls whether a conductive path is open or closed.";

                case "Resistor":
                    return
                        "A resistor opposes current and produces a voltage drop according to the circuit conditions.";

                case "DiodeOrLED":
                    return
                        "A diode primarily allows current in one direction. An LED also converts electrical energy into light.";

                case "Capacitor":
                    return
                        "A capacitor stores electrical energy and its voltage changes according to its charging and discharging behavior.";

                case "Motor":
                    return
                        "A motor converts electrical energy into mechanical motion.";

                case "Heater":
                    return
                        "A heater converts electrical energy primarily into thermal energy.";

                case "MeasurementDevice":
                    return
                        "A measurement device observes electrical quantities without being the circuit's primary source or load.";

                case "ElectricalComponent":
                    return
                        "This component participates in the electrical network.";

                default:
                    return
                        "The component's specific educational behavior has not yet been classified.";
            }
        }

        private string[] BuildObservations(
            SparkAIElectronicObjectSnapshot component,
            string componentType,
            SparkAIComponentState state)
        {
            List<string> observations =
                new List<string>(8);

            observations.Add(
                $"Component type: {componentType}.");

            observations.Add(
                $"State: {GetStateText(state)}.");

            if (component.IsElectricalComponent)
            {
                observations.Add(
                    $"Observed voltage: {component.Voltage:0.###} V.");

                observations.Add(
                    $"Observed current: {component.Current:0.######} A.");

                observations.Add(
                    $"Observed power: {component.Power:0.###} W.");
            }

            if (!string.IsNullOrWhiteSpace(
                    component.OperationalState))
            {
                observations.Add(
                    $"Operational state: {component.OperationalState}.");
            }

            if (!string.IsNullOrWhiteSpace(
                    component.ConductionState))
            {
                observations.Add(
                    $"Conduction state: {component.ConductionState}.");
            }

            return observations.ToArray();
        }

        private string[] BuildSuggestions(
            SparkAIElectronicObjectSnapshot component,
            string componentType,
            SparkAIComponentState state)
        {
            List<string> suggestions =
                new List<string>(4);

            switch (componentType)
            {
                case "PowerSource":

                    if (state ==
                        SparkAIComponentState.ElectricalDisabled)
                    {
                        suggestions.Add(
                            "Check whether the power source is electrically enabled.");
                    }
                    else
                    {
                        suggestions.Add(
                            "Trace the source terminals into the rest of the circuit.");
                    }

                    break;

                case "Switch":

                    if (state ==
                        SparkAIComponentState.Conducting)
                    {
                        suggestions.Add(
                            "The switch is currently providing a conductive path.");
                    }
                    else
                    {
                        suggestions.Add(
                            "The switch is currently interrupting the conductive path.");
                    }

                    break;

                case "Resistor":

                    suggestions.Add(
                        "Compare the voltage across the resistor with the current through it.");

                    break;

                case "DiodeOrLED":

                    suggestions.Add(
                        "Check the diode polarity and whether the circuit provides the correct forward path.");

                    break;

                case "Capacitor":

                    suggestions.Add(
                        "Observe how the capacitor voltage changes as the circuit state changes.");

                    break;

                case "Motor":

                    suggestions.Add(
                        "Check whether the motor has a complete electrical path and sufficient applied voltage.");

                    break;

                case "Heater":

                    suggestions.Add(
                        "Check whether current is flowing through the heating element.");

                    break;

                case "MeasurementDevice":

                    suggestions.Add(
                        "Use the measurement to compare the observed electrical behavior with the circuit objective.");

                    break;

                default:

                    if (state ==
                        SparkAIComponentState.Idle)
                    {
                        suggestions.Add(
                            "Inspect how this component is connected to the surrounding circuit.");
                    }

                    break;
            }

            return suggestions.ToArray();
        }

        private bool ContainsAny(
            string value,
            params string[] terms)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;

            for (int i = 0;
                 i < terms.Length;
                 i++)
            {
                if (value.IndexOf(
                        terms[i],
                        StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsClosedState(
            string state)
        {
            if (string.IsNullOrWhiteSpace(state))
                return false;

            return string.Equals(
                       state,
                       "Closed",
                       StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(
                       state,
                       "Conducting",
                       StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(
                       state,
                       "On",
                       StringComparison.OrdinalIgnoreCase);
        }

        private bool IsOpenState(
            string state)
        {
            if (string.IsNullOrWhiteSpace(state))
                return false;

            return string.Equals(
                       state,
                       "Open",
                       StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(
                       state,
                       "NonConducting",
                       StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(
                       state,
                       "Off",
                       StringComparison.OrdinalIgnoreCase);
        }

        private string GetStateText(
            SparkAIComponentState state)
        {
            switch (state)
            {
                case SparkAIComponentState.Conducting:
                    return "conducting";

                case SparkAIComponentState.CurrentFlowing:
                    return "carrying current";

                case SparkAIComponentState.VoltagePresent:
                    return "energized";

                case SparkAIComponentState.NonConducting:
                    return "not conducting";

                case SparkAIComponentState.ElectricalDisabled:
                    return "electrically disabled";

                case SparkAIComponentState.InteractionDisabled:
                    return "interaction disabled";

                case SparkAIComponentState.Inactive:
                    return "inactive";

                case SparkAIComponentState.NonElectrical:
                    return "non-electrical";

                default:
                    return "idle";
            }
        }
    }

    public enum SparkAIComponentState
    {
        Unknown = 0,
        Idle = 1,
        VoltagePresent = 2,
        CurrentFlowing = 3,
        Conducting = 4,
        NonConducting = 5,
        ElectricalDisabled = 6,
        InteractionDisabled = 7,
        Inactive = 8,
        NonElectrical = 9
    }

    /// <summary>
    /// Immutable AI interpretation of one observed component.
    /// </summary>
    public readonly struct SparkAIComponentReasoning
    {
        public bool IsValid { get; }

        public float CapturedAt { get; }

        public int InstanceId { get; }

        public string Name { get; }

        public string ComponentType { get; }

        public SparkAIComponentState State { get; }

        public float Voltage { get; }

        public float Current { get; }

        public float Power { get; }

        public bool ElectricalEnabled { get; }

        public bool Active { get; }

        public string Summary { get; }

        public string EducationalMeaning { get; }

        public string[] Observations { get; }

        public string[] Suggestions { get; }

        public SparkAIComponentReasoning(
            bool isValid,
            float capturedAt,
            int instanceId,
            string name,
            string componentType,
            SparkAIComponentState state,
            float voltage,
            float current,
            float power,
            bool electricalEnabled,
            bool active,
            string summary,
            string educationalMeaning,
            string[] observations,
            string[] suggestions)
        {
            IsValid = isValid;
            CapturedAt = capturedAt;
            InstanceId = instanceId;
            Name = name ?? string.Empty;
            ComponentType = componentType ?? string.Empty;
            State = state;
            Voltage = voltage;
            Current = current;
            Power = power;
            ElectricalEnabled = electricalEnabled;
            Active = active;
            Summary = summary ?? string.Empty;
            EducationalMeaning =
                educationalMeaning ?? string.Empty;
            Observations =
                observations ?? Array.Empty<string>();
            Suggestions =
                suggestions ?? Array.Empty<string>();
        }

        public static SparkAIComponentReasoning Invalid()
        {
            return new SparkAIComponentReasoning(
                false,
                Time.time,
                0,
                string.Empty,
                string.Empty,
                SparkAIComponentState.Unknown,
                0f,
                0f,
                0f,
                false,
                false,
                string.Empty,
                string.Empty,
                Array.Empty<string>(),
                Array.Empty<string>());
        }
    }
}