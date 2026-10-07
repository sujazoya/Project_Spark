using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectSpark.AI
{
    /// <summary>
    /// Converts SparkAIContextReasoning into a deterministic teaching
    /// decision.
    ///
    /// Teaching progression:
    /// Observe -> Question -> Hint -> Strong Hint -> Explanation
    ///
    /// This class does not:
    /// - modify the circuit
    /// - modify the solver
    /// - complete/fail levels
    /// - move objects
    /// - directly control the player
    ///
    /// It only decides what kind of teaching intervention is
    /// appropriate from the current authoritative observation.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkAITeachingReasoner : MonoBehaviour
    {
        [Header("AI Context")]
        [SerializeField]
        private SparkAIContextReasoner contextReasoner;

        [Header("Teaching")]
        [SerializeField]
        [Range(0f, 1f)]
        private float defaultConfidence = 0.8f;

        private SparkAITeachingDecision currentDecision;

        private bool initialized;

        public bool IsInitialized =>
            initialized;

        public SparkAITeachingDecision CurrentDecision =>
            currentDecision;

        private void Awake()
        {
            if (contextReasoner == null)
            {
                Debug.LogError(
                    "[Spark AI Teaching Reasoner] " +
                    "SparkAIContextReasoner reference is missing.",
                    this);

                return;
            }

            defaultConfidence =
                Mathf.Clamp01(defaultConfidence);

            initialized = true;
        }

        /// <summary>
        /// Creates a teaching decision from the latest world context.
        /// </summary>
        public SparkAITeachingDecision
            DecideCurrentTeaching()
        {
            if (!initialized)
            {
                currentDecision =
                    SparkAITeachingDecision.Invalid();

                return currentDecision;
            }

            SparkAIContextReasoning context =
                contextReasoner.ReasonCurrentContext();

            return Decide(context);
        }

        /// <summary>
        /// Creates a teaching decision from an existing context.
        /// </summary>
        public SparkAITeachingDecision Decide(
            SparkAIContextReasoning context)
        {
            if (!initialized ||
                !context.IsValid)
            {
                currentDecision =
                    SparkAITeachingDecision.Invalid();

                return currentDecision;
            }

            SparkAITeachingAction action =
                DetermineAction(context);

            SparkAITeachingTopic topic =
                DetermineTopic(context);

            SparkAITeachingIntensity intensity =
                DetermineIntensity(context);

            string message =
                BuildMessage(
                    context,
                    action,
                    topic,
                    intensity);

            string question =
                BuildQuestion(
                    context,
                    topic);

            string hint =
                BuildHint(
                    context,
                    topic,
                    intensity);

            string strongHint =
                BuildStrongHint(
                    context,
                    topic);

            string explanation =
                BuildExplanation(
                    context,
                    topic);

            string objective =
                BuildObjective(
                    context,
                    topic);

            string[] concepts =
                BuildConcepts(
                    context,
                    topic);

            currentDecision =
                new SparkAITeachingDecision(
                    true,
                    Time.time,
                    action,
                    topic,
                    intensity,
                    defaultConfidence,
                    message,
                    question,
                    hint,
                    strongHint,
                    explanation,
                    objective,
                    concepts);

            return currentDecision;
        }

        private SparkAITeachingAction DetermineAction(
            SparkAIContextReasoning context)
        {
            switch (context.State)
            {
                case SparkAIContextState.Unknown:
                    return SparkAITeachingAction.Observe;

                case SparkAIContextState.NoPowerSource:
                    return SparkAITeachingAction.Question;

                case SparkAIContextState.IncompletePowerSource:
                    return SparkAITeachingAction.Question;

                case SparkAIContextState.NoConnections:
                    return SparkAITeachingAction.Question;

                case SparkAIContextState.OpenCircuit:
                    return SparkAITeachingAction.Hint;

                case SparkAIContextState.ShortCircuit:
                    return SparkAITeachingAction.Warning;

                case SparkAIContextState.VoltageWithoutUsefulCurrent:
                    return SparkAITeachingAction.Question;

                case SparkAIContextState.ClosedCircuit:
                    return SparkAITeachingAction.Encourage;

                case SparkAIContextState.ActiveCircuit:
                    return SparkAITeachingAction.Question;

                case SparkAIContextState.ObservingCircuit:
                    return SparkAITeachingAction.Question;

                default:
                    return SparkAITeachingAction.Observe;
            }
        }

        private SparkAITeachingTopic DetermineTopic(
            SparkAIContextReasoning context)
        {
            switch (context.Focus)
            {
                case SparkAIContextFocus.PowerSource:
                    return SparkAITeachingTopic.Source;

                case SparkAIContextFocus.VoltageSource:
                    return SparkAITeachingTopic.Voltage;

                case SparkAIContextFocus.CircuitPath:
                    return SparkAITeachingTopic.CircuitPath;

                case SparkAIContextFocus.CurrentFlow:
                    return SparkAITeachingTopic.Current;

                case SparkAIContextFocus.ShortCircuit:
                    return SparkAITeachingTopic.ShortCircuit;

                case SparkAIContextFocus.Switch:
                    return SparkAITeachingTopic.Switch;

                case SparkAIContextFocus.Resistance:
                    return SparkAITeachingTopic.Resistance;

                case SparkAIContextFocus.Polarity:
                    return SparkAITeachingTopic.Polarity;

                case SparkAIContextFocus.Measurement:
                    return SparkAITeachingTopic.Measurement;

                case SparkAIContextFocus.ElectricalBehavior:
                    return SparkAITeachingTopic.Power;

                default:
                    return SparkAITeachingTopic.CircuitBasics;
            }
        }

        private SparkAITeachingIntensity DetermineIntensity(
            SparkAIContextReasoning context)
        {
            switch (context.State)
            {
                case SparkAIContextState.ShortCircuit:
                    return SparkAITeachingIntensity.Strong;

                case SparkAIContextState.OpenCircuit:
                    return SparkAITeachingIntensity.Medium;

                case SparkAIContextState.NoPowerSource:
                    return SparkAITeachingIntensity.Medium;

                case SparkAIContextState.IncompletePowerSource:
                    return SparkAITeachingIntensity.Medium;

                case SparkAIContextState.NoConnections:
                    return SparkAITeachingIntensity.Medium;

                case SparkAIContextState.VoltageWithoutUsefulCurrent:
                    return SparkAITeachingIntensity.Gentle;

                case SparkAIContextState.ClosedCircuit:
                    return SparkAITeachingIntensity.Gentle;

                case SparkAIContextState.ActiveCircuit:
                    return SparkAITeachingIntensity.Gentle;

                default:
                    return SparkAITeachingIntensity.Gentle;
            }
        }

        private string BuildMessage(
            SparkAIContextReasoning context,
            SparkAITeachingAction action,
            SparkAITeachingTopic topic,
            SparkAITeachingIntensity intensity)
        {
            switch (action)
            {
                case SparkAITeachingAction.Question:
                    return BuildQuestion(context, topic);

                case SparkAITeachingAction.Hint:
                    return BuildHint(
                        context,
                        topic,
                        intensity);

                case SparkAITeachingAction.Warning:
                    return BuildStrongHint(
                        context,
                        topic);

                case SparkAITeachingAction.Encourage:
                    return
                        "Good. The circuit has a useful connected path. Now observe what the electrical values are telling you.";

                default:
                    return
                        "Let's observe what is happening in the circuit.";
            }
        }

        private string BuildQuestion(
            SparkAIContextReasoning context,
            SparkAITeachingTopic topic)
        {
            switch (topic)
            {
                case SparkAITeachingTopic.Source:

                    return
                        "What component is providing the electrical potential difference in this circuit?";

                case SparkAITeachingTopic.Voltage:

                    return
                        "Where do you think the voltage in this circuit is coming from?";

                case SparkAITeachingTopic.CircuitPath:

                    return
                        "Can you trace a complete path from the positive side of the source back to the negative side?";

                case SparkAITeachingTopic.Current:

                    return
                        "If voltage is present, what else must be available for current to flow through the circuit?";

                case SparkAITeachingTopic.ShortCircuit:

                    return
                        "Is the current passing through the intended component, or has it found a path around it?";

                case SparkAITeachingTopic.Switch:

                    return
                        "What do you think will happen to the current path when this switch opens?";

                case SparkAITeachingTopic.Resistance:

                    return
                        "What do you expect to happen to current if the resistance becomes larger?";

                case SparkAITeachingTopic.Polarity:

                    return
                        "Which terminal should connect to the positive side, and which should connect to the negative side?";

                case SparkAITeachingTopic.Measurement:

                    return
                        "What electrical quantity is the instrument actually measuring in this connection?";

                case SparkAITeachingTopic.Power:

                    return
                        "If both voltage and current are present, what does that tell you about electrical power?";

                default:

                    return
                        "What do you think is happening in the circuit right now?";
            }
        }

        private string BuildHint(
            SparkAIContextReasoning context,
            SparkAITeachingTopic topic,
            SparkAITeachingIntensity intensity)
        {
            switch (topic)
            {
                case SparkAITeachingTopic.Source:

                    return
                        "Look for the component that establishes the voltage difference between two terminals.";

                case SparkAITeachingTopic.Voltage:

                    return
                        "Think of voltage as an electrical potential difference between two points.";

                case SparkAITeachingTopic.CircuitPath:

                    return
                        "Start at the positive source terminal and follow each connection. See whether you can eventually reach the negative terminal.";

                case SparkAITeachingTopic.Current:

                    return
                        "Current needs a conductive path. A break can leave voltage present while preventing useful current flow.";

                case SparkAITeachingTopic.ShortCircuit:

                    return
                        "Look for a connection that reaches the return side without passing through the intended load.";

                case SparkAITeachingTopic.Switch:

                    return
                        "A closed switch can provide continuity. An open switch breaks that path.";

                case SparkAITeachingTopic.Resistance:

                    return
                        "For the same voltage, increasing resistance generally reduces current.";

                case SparkAITeachingTopic.Polarity:

                    return
                        "Follow the positive and negative markings instead of choosing terminals by position alone.";

                case SparkAITeachingTopic.Measurement:

                    return
                        "First identify whether the instrument is measuring voltage, current, or another quantity.";

                case SparkAITeachingTopic.Power:

                    return
                        "Power depends on the electrical relationship between voltage and current.";

                default:

                    return
                        "Trace the source, the connections, and the component involved.";
            }
        }

        private string BuildStrongHint(
            SparkAIContextReasoning context,
            SparkAITeachingTopic topic)
        {
            switch (topic)
            {
                case SparkAITeachingTopic.Source:

                    return
                        "You need a valid source with an available positive and negative side before this circuit can demonstrate normal electrical behavior.";

                case SparkAITeachingTopic.CircuitPath:

                    return
                        "The circuit needs one continuous conductive route from the source's positive terminal through the intended circuit and back to the negative terminal.";

                case SparkAITeachingTopic.ShortCircuit:

                    return
                        "The source is seeing a direct low-resistance path. Remove or correct the bypass path so the intended load is part of the circuit.";

                case SparkAITeachingTopic.Current:

                    return
                        "Voltage alone does not guarantee current. The complete circuit path must allow current to flow.";

                case SparkAITeachingTopic.Polarity:

                    return
                        "Check the + and − terminals carefully. Polarity-sensitive components must be connected in the correct direction.";

                default:

                    return
                        "Inspect the circuit path and correct the connection that prevents the intended electrical behavior.";
            }
        }

        private string BuildExplanation(
            SparkAIContextReasoning context,
            SparkAITeachingTopic topic)
        {
            switch (topic)
            {
                case SparkAITeachingTopic.Source:

                    return
                        "A source establishes an electrical potential difference. That voltage provides the condition needed for current to flow when a suitable conductive path exists.";

                case SparkAITeachingTopic.Voltage:

                    return
                        "Voltage describes electrical potential difference between two points. It can exist even when significant current is not flowing.";

                case SparkAITeachingTopic.CircuitPath:

                    return
                        "A useful circuit provides a continuous conductive path through the intended components and back to the source.";

                case SparkAITeachingTopic.Current:

                    return
                        "Current describes the flow of electric charge. In a basic circuit, a source and a complete conductive path are required for useful current to flow.";

                case SparkAITeachingTopic.ShortCircuit:

                    return
                        "A short circuit creates an unintended low-resistance path that can bypass the intended load and produce excessive current.";

                case SparkAITeachingTopic.Switch:

                    return
                        "A switch controls continuity. Closing it can complete a path, while opening it can interrupt the path.";

                case SparkAITeachingTopic.Resistance:

                    return
                        "Resistance opposes current. With the same applied voltage, greater resistance generally produces less current.";

                case SparkAITeachingTopic.Polarity:

                    return
                        "Polarity identifies the positive and negative orientation of an electrical connection. Some components depend on being connected in the correct direction.";

                case SparkAITeachingTopic.Measurement:

                    return
                        "A measurement only makes sense when the instrument is connected in the way required for the quantity being measured.";

                case SparkAITeachingTopic.Power:

                    return
                        "Electrical power describes the rate of electrical energy transfer. In basic DC circuits, voltage and current are directly related to power.";

                default:

                    return
                        context.Explanation;
            }
        }

        private string BuildObjective(
            SparkAIContextReasoning context,
            SparkAITeachingTopic topic)
        {
            switch (topic)
            {
                case SparkAITeachingTopic.Source:
                    return "Identify the electrical source.";

                case SparkAITeachingTopic.Voltage:
                    return "Understand electrical potential difference.";

                case SparkAITeachingTopic.CircuitPath:
                    return "Build and recognize a complete conductive path.";

                case SparkAITeachingTopic.Current:
                    return "Understand when and why current flows.";

                case SparkAITeachingTopic.ShortCircuit:
                    return "Recognize and avoid unintended bypass paths.";

                case SparkAITeachingTopic.Switch:
                    return "Understand how switching changes continuity.";

                case SparkAITeachingTopic.Resistance:
                    return "Understand how resistance affects current.";

                case SparkAITeachingTopic.Polarity:
                    return "Recognize positive and negative orientation.";

                case SparkAITeachingTopic.Measurement:
                    return "Connect measurements to the correct electrical quantity.";

                case SparkAITeachingTopic.Power:
                    return "Connect voltage and current to power.";

                default:
                    return "Understand the current electrical behavior.";
            }
        }

        private string[] BuildConcepts(
            SparkAIContextReasoning context,
            SparkAITeachingTopic topic)
        {
            List<string> concepts =
                new List<string>(4);

            switch (topic)
            {
                case SparkAITeachingTopic.Source:

                    concepts.Add("source");
                    concepts.Add("voltage");

                    break;

                case SparkAITeachingTopic.Voltage:

                    concepts.Add("voltage");
                    concepts.Add("source");

                    break;

                case SparkAITeachingTopic.CircuitPath:

                    concepts.Add("closed_circuit");
                    concepts.Add("conductor");
                    concepts.Add("open_circuit");

                    break;

                case SparkAITeachingTopic.Current:

                    concepts.Add("current");
                    concepts.Add("closed_circuit");
                    concepts.Add("voltage");

                    break;

                case SparkAITeachingTopic.ShortCircuit:

                    concepts.Add("short_circuit");
                    concepts.Add("current");
                    concepts.Add("power");

                    break;

                case SparkAITeachingTopic.Switch:

                    concepts.Add("switch");
                    concepts.Add("open_circuit");
                    concepts.Add("closed_circuit");

                    break;

                case SparkAITeachingTopic.Resistance:

                    concepts.Add("resistance");
                    concepts.Add("ohms_law");

                    break;

                case SparkAITeachingTopic.Polarity:

                    concepts.Add("polarity");
                    concepts.Add("diode");

                    break;

                case SparkAITeachingTopic.Measurement:

                    concepts.Add("multimeter_voltage");
                    concepts.Add("multimeter_current");

                    break;

                case SparkAITeachingTopic.Power:

                    concepts.Add("power");
                    concepts.Add("voltage");
                    concepts.Add("current");

                    break;
            }

            return concepts.ToArray();
        }
    }

    public enum SparkAITeachingAction
    {
        Observe = 0,
        Question = 1,
        Hint = 2,
        Warning = 3,
        Encourage = 4
    }

    public enum SparkAITeachingTopic
    {
        CircuitBasics = 0,
        Source = 1,
        Voltage = 2,
        Current = 3,
        Resistance = 4,
        Power = 5,
        CircuitPath = 6,
        ShortCircuit = 7,
        Switch = 8,
        Polarity = 9,
        Measurement = 10
    }

    public enum SparkAITeachingIntensity
    {
        Gentle = 0,
        Medium = 1,
        Strong = 2
    }

    /// <summary>
    /// Immutable teaching decision produced from deterministic
    /// Project Spark observations.
    /// </summary>
    public readonly struct SparkAITeachingDecision
    {
        public bool IsValid { get; }

        public float CreatedAt { get; }

        public SparkAITeachingAction Action { get; }

        public SparkAITeachingTopic Topic { get; }

        public SparkAITeachingIntensity Intensity { get; }

        public float Confidence { get; }

        public string Message { get; }

        public string Question { get; }

        public string Hint { get; }

        public string StrongHint { get; }

        public string Explanation { get; }

        public string Objective { get; }

        public string[] ConceptIds { get; }

        public SparkAITeachingDecision(
            bool isValid,
            float createdAt,
            SparkAITeachingAction action,
            SparkAITeachingTopic topic,
            SparkAITeachingIntensity intensity,
            float confidence,
            string message,
            string question,
            string hint,
            string strongHint,
            string explanation,
            string objective,
            string[] conceptIds)
        {
            IsValid = isValid;
            CreatedAt = createdAt;
            Action = action;
            Topic = topic;
            Intensity = intensity;
            Confidence = Mathf.Clamp01(confidence);

            Message =
                message ?? string.Empty;

            Question =
                question ?? string.Empty;

            Hint =
                hint ?? string.Empty;

            StrongHint =
                strongHint ?? string.Empty;

            Explanation =
                explanation ?? string.Empty;

            Objective =
                objective ?? string.Empty;

            ConceptIds =
                conceptIds ??
                Array.Empty<string>();
        }

        public static SparkAITeachingDecision Invalid()
        {
            return new SparkAITeachingDecision(
                false,
                Time.time,
                SparkAITeachingAction.Observe,
                SparkAITeachingTopic.CircuitBasics,
                SparkAITeachingIntensity.Gentle,
                0f,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                Array.Empty<string>());
        }
    }
}