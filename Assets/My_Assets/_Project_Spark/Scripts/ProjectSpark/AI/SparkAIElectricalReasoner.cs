using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectSpark.AI
{
    /// <summary>
    /// Deterministic AI-side reasoning about observed electrical behavior.
    ///
    /// Uses authoritative electrical values already captured by
    /// SparkAIWorld.
    ///
    /// IMPORTANT:
    /// This component does NOT:
    /// - solve circuits
    /// - calculate replacement voltage/current values
    /// - modify the electrical solver
    /// - modify circuit topology
    /// - modify components
    /// - replace level evaluation
    ///
    /// It interprets observed electrical behavior for AI reasoning
    /// and educational explanation.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkAIElectricalReasoner : MonoBehaviour
    {
        [Header("Spark AI")]
        [SerializeField]
        private SparkAIWorld world;

        [Header("Reasoning Thresholds")]
        [SerializeField]
        private float voltageThreshold = 0.001f;

        [SerializeField]
        private float currentThreshold = 0.000001f;

        [SerializeField]
        private float powerThreshold = 0.000001f;

        [SerializeField]
        private float comparisonTolerance = 0.0001f;

        private readonly List<SparkAIElectricalReasoning> results =
            new List<SparkAIElectricalReasoning>(32);

        private bool initialized;

        public bool IsInitialized =>
            initialized;

        public IReadOnlyList<SparkAIElectricalReasoning> CurrentResults =>
            results;

        private void Awake()
        {
            if (world == null)
            {
                Debug.LogError(
                    "[Spark AI Electrical Reasoner] " +
                    "SparkAIWorld reference is missing.",
                    this);

                return;
            }

            voltageThreshold =
                Mathf.Max(0f, voltageThreshold);

            currentThreshold =
                Mathf.Max(0f, currentThreshold);

            powerThreshold =
                Mathf.Max(0f, powerThreshold);

            comparisonTolerance =
                Mathf.Max(0f, comparisonTolerance);

            initialized = true;
        }

        /// <summary>
        /// Analyzes the latest authoritative electrical snapshot.
        /// </summary>
        public IReadOnlyList<SparkAIElectricalReasoning>
            AnalyzeCurrentElectricalState()
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
        /// Analyzes electrical behavior from a supplied snapshot.
        /// </summary>
        public IReadOnlyList<SparkAIElectricalReasoning>
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

                if (!component.IsElectricalComponent)
                    continue;

                SparkAIElectricalReasoning reasoning =
                    AnalyzeComponent(
                        component);

                if (!reasoning.IsValid)
                    continue;

                results.Add(reasoning);
            }

            return results;
        }

        /// <summary>
        /// Analyzes one electrical component by instance ID.
        /// </summary>
        public bool TryAnalyzeComponent(
            SparkAIWorldSnapshot snapshot,
            int instanceId,
            out SparkAIElectricalReasoning reasoning)
        {
            reasoning =
                SparkAIElectricalReasoning.Invalid();

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

                if (!component.IsElectricalComponent)
                    return false;

                reasoning =
                    AnalyzeComponent(
                        component);

                return reasoning.IsValid;
            }

            return false;
        }

        private SparkAIElectricalReasoning AnalyzeComponent(
            SparkAIElectronicObjectSnapshot component)
        {
            float voltage =
                component.Voltage;

            float current =
                component.Current;

            float power =
                component.Power;

            bool hasVoltage =
                Mathf.Abs(voltage) >
                voltageThreshold;

            bool hasCurrent =
                Mathf.Abs(current) >
                currentThreshold;

            bool hasPower =
                Mathf.Abs(power) >
                powerThreshold;

            SparkAIElectricalState state =
                DetermineState(
                    component,
                    hasVoltage,
                    hasCurrent,
                    hasPower);

            string relationship =
                DetermineElectricalRelationship(
                    voltage,
                    current,
                    power,
                    hasVoltage,
                    hasCurrent,
                    hasPower);

            string energyBehavior =
                DetermineEnergyBehavior(
                    power,
                    hasPower);

            string educationalMeaning =
                BuildEducationalMeaning(
                    state,
                    relationship,
                    energyBehavior);

            string summary =
                BuildSummary(
                    component,
                    state,
                    relationship);

            string[] observations =
                BuildObservations(
                    component,
                    state,
                    relationship,
                    energyBehavior);

            string[] suggestions =
                BuildSuggestions(
                    component,
                    state,
                    relationship);

            return new SparkAIElectricalReasoning(
                true,
                Time.time,
                component.InstanceId,
                component.Name,
                state,
                voltage,
                current,
                power,
                hasVoltage,
                hasCurrent,
                hasPower,
                relationship,
                energyBehavior,
                summary,
                educationalMeaning,
                observations,
                suggestions);
        }

        private SparkAIElectricalState DetermineState(
            SparkAIElectronicObjectSnapshot component,
            bool hasVoltage,
            bool hasCurrent,
            bool hasPower)
        {
            if (!component.Active)
            {
                return SparkAIElectricalState.Inactive;
            }

            if (!component.ElectricalEnabled)
            {
                return SparkAIElectricalState.Disabled;
            }

            if (!hasVoltage &&
                !hasCurrent &&
                !hasPower)
            {
                return SparkAIElectricalState.ElectricallyIdle;
            }

            if (hasVoltage &&
                hasCurrent &&
                hasPower)
            {
                return SparkAIElectricalState.EnergizedAndConducting;
            }

            if (hasVoltage &&
                !hasCurrent)
            {
                return SparkAIElectricalState.VoltageWithoutCurrent;
            }

            if (!hasVoltage &&
                hasCurrent)
            {
                return SparkAIElectricalState.CurrentWithoutSignificantVoltage;
            }

            if (hasCurrent)
            {
                return SparkAIElectricalState.CurrentFlowing;
            }

            if (hasVoltage)
            {
                return SparkAIElectricalState.VoltagePresent;
            }

            return SparkAIElectricalState.Unknown;
        }

        private string DetermineElectricalRelationship(
            float voltage,
            float current,
            float power,
            bool hasVoltage,
            bool hasCurrent,
            bool hasPower)
        {
            if (!hasVoltage &&
                !hasCurrent &&
                !hasPower)
            {
                return "No significant electrical activity is observed.";
            }

            if (hasVoltage &&
                !hasCurrent)
            {
                return
                    "A voltage difference is present, but significant current is not observed.";
            }

            if (!hasVoltage &&
                hasCurrent)
            {
                return
                    "Current is observed while the measured voltage magnitude is very small.";
            }

            if (hasVoltage &&
                hasCurrent)
            {
                if (Mathf.Abs(power) > powerThreshold)
                {
                    return
                        "Voltage and current are both present, so the component is participating in electrical power transfer.";
                }

                return
                    "Voltage and current are present, but the reported power is near zero.";
            }

            return
                "Electrical activity is present but its relationship could not be classified.";
        }

        private string DetermineEnergyBehavior(
            float power,
            bool hasPower)
        {
            if (!hasPower)
            {
                return "No significant power transfer is observed.";
            }

            if (power > powerThreshold)
            {
                return
                    "The component is receiving positive electrical power according to the authoritative observation.";
            }

            if (power < -powerThreshold)
            {
                return
                    "The component is delivering electrical power according to the authoritative observation.";
            }

            return
                "The component has approximately zero net electrical power.";
        }

        private string BuildEducationalMeaning(
            SparkAIElectricalState state,
            string relationship,
            string energyBehavior)
        {
            switch (state)
            {
                case SparkAIElectricalState.VoltageWithoutCurrent:

                    return
                        "Voltage can exist across a component without significant current flowing. This commonly occurs when the surrounding circuit does not provide a complete conductive path.";

                case SparkAIElectricalState.CurrentWithoutSignificantVoltage:

                    return
                        "Current is being observed while the voltage magnitude is very small. The actual meaning depends on the component and circuit topology.";

                case SparkAIElectricalState.EnergizedAndConducting:

                    return
                        "The component has both voltage across it and current through it, indicating active participation in the electrical circuit.";

                case SparkAIElectricalState.CurrentFlowing:

                    return
                        "Electrical current is flowing through the observed component.";

                case SparkAIElectricalState.VoltagePresent:

                    return
                        "An electrical potential difference is present across the observed component.";

                case SparkAIElectricalState.ElectricallyIdle:

                    return
                        "The component currently shows no significant voltage, current, or power in the captured state.";

                case SparkAIElectricalState.Disabled:

                    return
                        "The component is currently disabled electrically, so its observed electrical state should be interpreted in that context.";

                case SparkAIElectricalState.Inactive:

                    return
                        "The component is inactive, so its electrical measurements should not be interpreted as normal operating behavior.";

                default:

                    return
                        relationship + " " + energyBehavior;
            }
        }

        private string BuildSummary(
            SparkAIElectronicObjectSnapshot component,
            SparkAIElectricalState state,
            string relationship)
        {
            string name =
                string.IsNullOrWhiteSpace(component.Name)
                    ? "The component"
                    : component.Name;

            return
                $"{name}: {GetStateText(state)}. {relationship}";
        }

        private string[] BuildObservations(
            SparkAIElectronicObjectSnapshot component,
            SparkAIElectricalState state,
            string relationship,
            string energyBehavior)
        {
            List<string> observations =
                new List<string>(8);

            observations.Add(
                $"Electrical state: {GetStateText(state)}.");

            observations.Add(
                $"Voltage: {component.Voltage:0.######} V.");

            observations.Add(
                $"Current: {component.Current:0.######} A.");

            observations.Add(
                $"Power: {component.Power:0.######} W.");

            observations.Add(
                relationship);

            observations.Add(
                energyBehavior);

            return observations.ToArray();
        }

        private string[] BuildSuggestions(
            SparkAIElectronicObjectSnapshot component,
            SparkAIElectricalState state,
            string relationship)
        {
            List<string> suggestions =
                new List<string>(4);

            switch (state)
            {
                case SparkAIElectricalState.VoltageWithoutCurrent:

                    suggestions.Add(
                        "Check whether the circuit has a complete conductive path.");

                    suggestions.Add(
                        "Inspect the switch and return connection.");

                    break;

                case SparkAIElectricalState.CurrentWithoutSignificantVoltage:

                    suggestions.Add(
                        "Inspect the component and surrounding connections to understand why current is flowing with little measured voltage across this component.");

                    break;

                case SparkAIElectricalState.EnergizedAndConducting:

                    suggestions.Add(
                        "Compare the component's voltage, current, and power with the learning objective.");

                    break;

                case SparkAIElectricalState.VoltagePresent:

                    suggestions.Add(
                        "Check whether the circuit provides a path for current through the component.");

                    break;

                case SparkAIElectricalState.CurrentFlowing:

                    suggestions.Add(
                        "Inspect where the current enters and leaves the component.");

                    break;

                case SparkAIElectricalState.ElectricallyIdle:

                    suggestions.Add(
                        "Trace the circuit connections and check whether the component is part of the active path.");

                    break;

                case SparkAIElectricalState.Disabled:

                    suggestions.Add(
                        "Check the component's electrical enable state.");

                    break;

                case SparkAIElectricalState.Inactive:

                    suggestions.Add(
                        "Check whether the component is active in the scene.");

                    break;
            }

            return suggestions.ToArray();
        }

        private string GetStateText(
            SparkAIElectricalState state)
        {
            switch (state)
            {
                case SparkAIElectricalState.ElectricallyIdle:
                    return "electrically idle";

                case SparkAIElectricalState.VoltagePresent:
                    return "voltage present";

                case SparkAIElectricalState.VoltageWithoutCurrent:
                    return "voltage present without significant current";

                case SparkAIElectricalState.CurrentFlowing:
                    return "current flowing";

                case SparkAIElectricalState.CurrentWithoutSignificantVoltage:
                    return "current flowing with little measured voltage";

                case SparkAIElectricalState.EnergizedAndConducting:
                    return "energized and conducting";

                case SparkAIElectricalState.Disabled:
                    return "electrically disabled";

                case SparkAIElectricalState.Inactive:
                    return "inactive";

                default:
                    return "unknown electrical state";
            }
        }
    }

    public enum SparkAIElectricalState
    {
        Unknown = 0,
        ElectricallyIdle = 1,
        VoltagePresent = 2,
        VoltageWithoutCurrent = 3,
        CurrentFlowing = 4,
        CurrentWithoutSignificantVoltage = 5,
        EnergizedAndConducting = 6,
        Disabled = 7,
        Inactive = 8
    }

    /// <summary>
    /// Immutable interpretation of one component's observed
    /// electrical behavior.
    /// </summary>
    public readonly struct SparkAIElectricalReasoning
    {
        public bool IsValid { get; }

        public float CapturedAt { get; }

        public int InstanceId { get; }

        public string Name { get; }

        public SparkAIElectricalState State { get; }

        public float Voltage { get; }

        public float Current { get; }

        public float Power { get; }

        public bool HasVoltage { get; }

        public bool HasCurrent { get; }

        public bool HasPower { get; }

        public string ElectricalRelationship { get; }

        public string EnergyBehavior { get; }

        public string Summary { get; }

        public string EducationalMeaning { get; }

        public string[] Observations { get; }

        public string[] Suggestions { get; }

        public SparkAIElectricalReasoning(
            bool isValid,
            float capturedAt,
            int instanceId,
            string name,
            SparkAIElectricalState state,
            float voltage,
            float current,
            float power,
            bool hasVoltage,
            bool hasCurrent,
            bool hasPower,
            string electricalRelationship,
            string energyBehavior,
            string summary,
            string educationalMeaning,
            string[] observations,
            string[] suggestions)
        {
            IsValid = isValid;
            CapturedAt = capturedAt;
            InstanceId = instanceId;
            Name = name ?? string.Empty;
            State = state;
            Voltage = voltage;
            Current = current;
            Power = power;
            HasVoltage = hasVoltage;
            HasCurrent = hasCurrent;
            HasPower = hasPower;

            ElectricalRelationship =
                electricalRelationship ?? string.Empty;

            EnergyBehavior =
                energyBehavior ?? string.Empty;

            Summary =
                summary ?? string.Empty;

            EducationalMeaning =
                educationalMeaning ?? string.Empty;

            Observations =
                observations ??
                Array.Empty<string>();

            Suggestions =
                suggestions ??
                Array.Empty<string>();
        }

        public static SparkAIElectricalReasoning Invalid()
        {
            return new SparkAIElectricalReasoning(
                false,
                Time.time,
                0,
                string.Empty,
                SparkAIElectricalState.Unknown,
                0f,
                0f,
                0f,
                false,
                false,
                false,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                Array.Empty<string>(),
                Array.Empty<string>());
        }
    }
}