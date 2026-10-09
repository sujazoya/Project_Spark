using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectSpark.AI
{
    /// <summary>
    /// Connects stable electronics knowledge with the live Project Spark
    /// world without modifying gameplay or electrical simulation state.
    ///
    /// Responsibilities:
    /// - Retrieve a knowledge entry.
    /// - Read related knowledge.
    /// - Ground explanations using authoritative AI world observations.
    /// - Produce concise educational observations.
    ///
    /// Does NOT:
    /// - Modify the circuit.
    /// - Modify the solver.
    /// - Decide level success/failure.
    /// - Invent electrical values.
    /// </summary>
    public sealed class SparkAIKnowledgeReasoner
    {
        private readonly SparkAIKnowledge knowledge;
        private readonly SparkAIWorld world;

        public SparkAIKnowledgeReasoner(
            SparkAIKnowledge knowledge,
            SparkAIWorld world)
        {
            this.knowledge = knowledge;
            this.world = world;
        }

        public bool IsValid =>
            knowledge != null &&
            world != null;

        public bool TryGetKnowledge(
            string knowledgeId,
            out SparkAIKnowledge.Entry entry)
        {
            entry = null;

            if (knowledge == null ||
                string.IsNullOrWhiteSpace(knowledgeId))
            {
                return false;
            }

            return knowledge.TryGetEntry(
                knowledgeId,
                out entry);
        }
public bool TryExplain(
    string knowledgeId,
    out SparkAIKnowledgeReasoningResult result)
{
    result = SparkAIKnowledgeReasoningResult.Invalid();

    if (!IsValid)
        return false;

    SparkAIKnowledge.Entry entry;

    if (!knowledge.TryGetEntry(knowledgeId, out entry))
        return false;

    SparkAIWorldSnapshot snapshot = world.CaptureSnapshot();

    List<string> observations = new List<string>();
    List<string> teachingPoints = new List<string>();

    string summary = entry.Definition;

    AddLiveContext(
        entry,
        snapshot,
        observations,
        teachingPoints);

    result = new SparkAIKnowledgeReasoningResult(
        true,
        entry.Id,
        entry.Title,
        summary,
        entry.Definition,
        entry.Explanation,
        entry.Example,
        entry.Misconception,
        entry.Safety,
        observations,
        teachingPoints,
        SparkAIKnowledgeQuestionType.Unknown,
        entry.Measurement);

    return true;
}
public bool TryExplainQuestion(
    string question,
    out SparkAIKnowledgeReasoningResult result)
{
    result = SparkAIKnowledgeReasoningResult.Invalid();

    if (!IsValid || string.IsNullOrWhiteSpace(question))
        return false;

    SparkAIKnowledgeQuestionResult questionResult =
        SparkAIKnowledgeQuestionResolver.Resolve(question);


       Debug.Log(
    "[SPARK AI MEASUREMENT DEBUG] Question=" + question +
    " | Type=" + questionResult.Type);



    SparkAIKnowledge.Entry entry = null;

    string normalizedQuestion = question.Trim().ToLowerInvariant();

    bool isCalculationQuestion =
        normalizedQuestion.Contains("calculate") ||
        normalizedQuestion.Contains("ohm's law") ||
        normalizedQuestion.Contains("ohms law") ||
        normalizedQuestion.Contains("from voltage and resistance");

    bool foundEntry = false;

    // Prioritize the specific knowledge entry for measurement questions.
    if (questionResult.Type == SparkAIKnowledgeQuestionType.Measurement ||
        isCalculationQuestion)
    {
        foundEntry = TryResolveMeasurementEntry(
            question,
            out entry);
    }

    // Preserve the existing general knowledge matching as a fallback.
    if (!foundEntry)
    {
        foundEntry = knowledge.TryFindEntryForQuestion(
            question,
            out entry);
    }

    if (!foundEntry || entry == null)
        return false;


       /* Debug.Log(
    "[SPARK AI MEASUREMENT DEBUG] SelectedEntry=" + entry.Id +
    " | Title=" + entry.Title +
    " | Measurement=" +
    (string.IsNullOrWhiteSpace(entry.Measurement)
        ? "<EMPTY>"
        : entry.Measurement));*/

    if (!TryExplain(entry.Id, out result))
        return false;

        /*Debug.Log(
    "[SPARK AI MEASUREMENT DEBUG] ResultType=" + questionResult.Type +
    " | ResultEntry=" + result.KnowledgeId +
    " | ResultMeasurement=" +
    (string.IsNullOrWhiteSpace(result.Measurement)
        ? "<EMPTY>"
        : result.Measurement));*/

    // Preserve all existing reasoning data, including measurement steps.
    result = new SparkAIKnowledgeReasoningResult(
        result.IsValid,
        result.KnowledgeId,
        result.Title,
        result.Summary,
        result.Definition,
        result.Explanation,
        result.Example,
        result.Misconception,
        result.Safety,
        result.Observations,
        result.TeachingPoints,
        questionResult.Type,
        result.Measurement);

    return true;
}
        private static void AddLiveContext(
            SparkAIKnowledge.Entry entry,
            SparkAIWorldSnapshot snapshot,
            List<string> observations,
            List<string> teachingPoints)
        {
            string id =
                entry.Id != null
                    ? entry.Id.Trim()
                    : string.Empty;

            switch (id.ToLowerInvariant())
            {
                case "circuit":

                    AddCircuitContext(
                        snapshot,
                        observations,
                        teachingPoints);

                    break;

                case "electrical_source":

                    AddSourceContext(
                        snapshot,
                        observations,
                        teachingPoints);

                    break;

                case "circuit_path":

                    AddPathContext(
                        snapshot,
                        observations,
                        teachingPoints);

                    break;

                case "polarity":

                    AddPolarityContext(
                        snapshot,
                        observations,
                        teachingPoints);

                    break;

                case "open_circuit":

                    AddOpenCircuitContext(
                        snapshot,
                        observations,
                        teachingPoints);

                    break;

                case "closed_circuit":

                    AddClosedCircuitContext(
                        snapshot,
                        observations,
                        teachingPoints);

                    break;

                case "short_circuit":

                    AddShortCircuitContext(
                        snapshot,
                        observations,
                        teachingPoints);

                    break;

                case "wire":

                    AddWireContext(
                        snapshot,
                        observations,
                        teachingPoints);

                    break;

                case "led":

                    AddLEDContext(
                        snapshot,
                        observations,
                        teachingPoints);

                    break;

                case "resistor":

                    AddResistorContext(
                        snapshot,
                        observations,
                        teachingPoints);

                    break;
            }
        }

       
        private bool TryResolveMeasurementEntry(
            string question,
            out SparkAIKnowledge.Entry entry)
        {
            entry = null;

            if (string.IsNullOrWhiteSpace(question) || knowledge == null)
                return false;

            string q = question.Trim().ToLowerInvariant();

            // 1. Ohm's law calculations take priority over component matching.
            if (q.Contains("calculate current") ||
                q.Contains("calculate voltage") ||
                q.Contains("calculate resistance") ||
                q.Contains("current from voltage") ||
                q.Contains("voltage and resistance") ||
                q.Contains("resistance and voltage") ||
                q.Contains("ohm's law") ||
                q.Contains("ohms law"))
            {
                return knowledge.TryGetEntry("ohms_law", out entry);
            }

            // 2. LED measurement questions must precede generic voltage matching.
            if (q.Contains("led") &&
                (q.Contains("measure") ||
                q.Contains("test") ||
                q.Contains("voltage") ||
                q.Contains("current") ||
                q.Contains("check")))
            {
                return knowledge.TryGetEntry("led", out entry);
            }

            // 3. Source measurements must precede generic voltage/current matching.
            if ((q.Contains("source") ||
                q.Contains("battery") ||
                q.Contains("power supply")) &&
                (q.Contains("measure") ||
                q.Contains("voltage") ||
                q.Contains("current") ||
                q.Contains("check") ||
                q.Contains("test")))
            {
                return knowledge.TryGetEntry("electrical_source", out entry);
            }

            // 4. Resistor resistance measurements.
            if (q.Contains("resistance") ||
                q.Contains("measure resistor") ||
                q.Contains("test resistor") ||
                q.Contains("ohmmeter"))
            {
                return knowledge.TryGetEntry("resistor", out entry);
            }

            // 5. Wire continuity and open-wire checks.
            if (q.Contains("open wire") ||
                q.Contains("test a wire") ||
                q.Contains("check a wire") ||
                q.Contains("wire continuity") ||
                q.Contains("continuity of a wire"))
            {
                return knowledge.TryGetEntry("wire", out entry);
            }

            // 6. Circuit continuity and path measurements.
            if (q.Contains("open circuit"))
                return knowledge.TryGetEntry("open_circuit", out entry);

            if (q.Contains("closed circuit"))
                return knowledge.TryGetEntry("closed_circuit", out entry);

            if (q.Contains("circuit path") ||
                q.Contains("continuity of the path"))
            {
                return knowledge.TryGetEntry("circuit_path", out entry);
            }

            if (q.Contains("measure a circuit") ||
                q.Contains("measure the circuit"))
            {
                return knowledge.TryGetEntry("circuit", out entry);
            }

            // 7. Generic current measurement.
            if (q.Contains("current") ||
                q.Contains("ammeter") ||
                q.Contains("measure amps") ||
                q.Contains("measure amperes"))
            {
                return knowledge.TryGetEntry("current", out entry);
            }

            // 8. Generic voltage measurement.
            if (q.Contains("voltage") ||
                q.Contains("voltmeter") ||
                q.Contains("measure volts"))
            {
                return knowledge.TryGetEntry("voltage", out entry);
            }

            return false;
        }

        private static void AddCircuitContext(
            SparkAIWorldSnapshot snapshot,
            List<string> observations,
            List<string> teachingPoints)
        {
            SparkAILevelSnapshot level =
                snapshot.Level;

            if (!level.HasLevel)
                return;

            if (level.HasValidPowerSource)
            {
                observations.Add(
                    "A valid power source is currently present.");
            }

            if (level.ClosedReturn)
            {
                observations.Add(
                    "The current level reports a closed return path.");

                teachingPoints.Add(
                    "This demonstrates why a complete electrical path matters.");
            }
            else
            {
                observations.Add(
                    "The current level does not report a closed return path.");

                teachingPoints.Add(
                    "A broken path prevents normal current flow.");
            }
        }

        private static void AddSourceContext(
            SparkAIWorldSnapshot snapshot,
            List<string> observations,
            List<string> teachingPoints)
        {
            SparkAILevelSnapshot level =
                snapshot.Level;

            if (!level.HasLevel)
                return;

            if (level.HasValidPowerSource)
            {
                observations.Add(
                    "The current level has a valid power source.");
            }
            else
            {
                observations.Add(
                    "The current level does not currently have a valid power source.");
            }

            if (!string.IsNullOrWhiteSpace(
                    level.ActiveSourceName))
            {
                observations.Add(
                    "Active source: " +
                    level.ActiveSourceName);
            }

            teachingPoints.Add(
                "A source can provide voltage even when the circuit is open.");
        }

        private static void AddPathContext(
            SparkAIWorldSnapshot snapshot,
            List<string> observations,
            List<string> teachingPoints)
        {
            SparkAILevelSnapshot level =
                snapshot.Level;

            if (!level.HasLevel)
                return;

            if (level.ClosedReturn)
            {
                observations.Add(
                    "The circuit currently has a closed return path.");
            }
            else
            {
                observations.Add(
                    "The circuit currently does not have a closed return path.");

                teachingPoints.Add(
                    "Trace from one source terminal through the components and back to the other source terminal.");
            }
        }

        private static void AddPolarityContext(
            SparkAIWorldSnapshot snapshot,
            List<string> observations,
            List<string> teachingPoints)
        {
            SparkAILevelSnapshot level =
                snapshot.Level;

            if (!level.HasLevel)
                return;

            if (level.WrongConnection)
            {
                observations.Add(
                    "The current level reports an incorrect connection.");

                teachingPoints.Add(
                    "Check the positive and negative sides of polarized components.");
            }
            else
            {
                observations.Add(
                    "The current level is not currently reporting a wrong connection.");
            }
        }

        private static void AddOpenCircuitContext(
            SparkAIWorldSnapshot snapshot,
            List<string> observations,
            List<string> teachingPoints)
        {
            SparkAILevelSnapshot level =
                snapshot.Level;

            if (!level.HasLevel)
                return;

            if (!level.ClosedReturn)
            {
                observations.Add(
                    "The level currently does not report a closed return path.");

                teachingPoints.Add(
                    "An interrupted path can behave as an open circuit.");
            }
        }

        private static void AddClosedCircuitContext(
            SparkAIWorldSnapshot snapshot,
            List<string> observations,
            List<string> teachingPoints)
        {
            SparkAILevelSnapshot level =
                snapshot.Level;

            if (!level.HasLevel)
                return;

            if (level.ClosedReturn)
            {
                observations.Add(
                    "The level currently reports a closed return path.");

                teachingPoints.Add(
                    "A closed path allows current to flow when the source and circuit conditions permit it.");
            }
        }

        private static void AddShortCircuitContext(
            SparkAIWorldSnapshot snapshot,
            List<string> observations,
            List<string> teachingPoints)
        {
            SparkAILevelSnapshot level =
                snapshot.Level;

            if (!level.HasLevel)
                return;

            if (level.SourceShorted)
            {
                observations.Add(
                    "The level currently reports a source short circuit.");

                teachingPoints.Add(
                    "A very low-resistance path can allow excessive current.");
            }

            if (level.TargetShorted)
            {
                observations.Add(
                    "The level currently reports a target short circuit.");
            }
        }

        private static void AddWireContext(
            SparkAIWorldSnapshot snapshot,
            List<string> observations,
            List<string> teachingPoints)
        {
            int connectionCount =
                snapshot.Connections != null
                    ? snapshot.Connections.Count
                    : 0;

            if (connectionCount > 0)
            {
                observations.Add(
                    "The circuit currently contains " +
                    connectionCount +
                    " recorded electrical connection(s).");
            }

            teachingPoints.Add(
                "A wire is useful only when its conductive ends are connected to the intended terminals.");
        }

        private static void AddLEDContext(
            SparkAIWorldSnapshot snapshot,
            List<string> observations,
            List<string> teachingPoints)
        {
            if (snapshot.ElectronicObjects == null)
                return;

            for (int i = 0;
                 i < snapshot.ElectronicObjects.Count;
                 i++)
            {
                SparkAIElectronicObjectSnapshot objectSnapshot =
                    snapshot.ElectronicObjects[i];

                if (!IsLikelyLED(objectSnapshot))
                    continue;

                observations.Add(
                    objectSnapshot.Name +
                    " electrical state: " +
                    objectSnapshot.ConductionState +
                    ".");

                teachingPoints.Add(
                    "An LED needs appropriate polarity, a complete path, and suitable current for normal operation.");

                return;
            }
        }

        private static void AddResistorContext(
            SparkAIWorldSnapshot snapshot,
            List<string> observations,
            List<string> teachingPoints)
        {
            if (snapshot.ElectronicObjects == null)
                return;

            for (int i = 0;
                 i < snapshot.ElectronicObjects.Count;
                 i++)
            {
                SparkAIElectronicObjectSnapshot objectSnapshot =
                    snapshot.ElectronicObjects[i];

                if (!IsLikelyResistor(objectSnapshot))
                    continue;

                observations.Add(
                    objectSnapshot.Name +
                    " has voltage=" +
                    objectSnapshot.Voltage +
                    ", current=" +
                    objectSnapshot.Current +
                    ", power=" +
                    objectSnapshot.Power +
                    ".");

                teachingPoints.Add(
                    "A resistor can limit current and dissipate electrical power.");

                return;
            }
        }

        private static bool IsLikelyLED(
            SparkAIElectronicObjectSnapshot snapshot)
        {
            return Contains(
                       snapshot.Name,
                       "led") ||
                   Contains(
                       snapshot.Name,
                       "bulb");
        }

        private static bool IsLikelyResistor(
            SparkAIElectronicObjectSnapshot snapshot)
        {
            return Contains(
                snapshot.Name,
                "resistor");
        }

        private static bool Contains(
            string value,
            string search)
        {
            if (string.IsNullOrWhiteSpace(value) ||
                string.IsNullOrWhiteSpace(search))
            {
                return false;
            }

            return value.IndexOf(
                       search,
                       StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }

    /// <summary>
    /// Immutable result produced by SparkAIKnowledgeReasoner.
    /// </summary>
    public readonly struct SparkAIKnowledgeReasoningResult
    {
        public bool IsValid { get; }

        public string KnowledgeId { get; }

        public string Title { get; }

        public string Summary { get; }

        public string Definition { get; }

        public string Explanation { get; }

        public string Example { get; }

        public string Misconception { get; }

        public string Safety { get; }

        public IReadOnlyList<string> Observations { get; }

        public IReadOnlyList<string> TeachingPoints { get; }

        public SparkAIKnowledgeQuestionType QuestionType { get; }

        public string Measurement { get; }

            public SparkAIKnowledgeReasoningResult(
            bool isValid,
            string knowledgeId,
            string title,
            string summary,
            string definition,
            string explanation,
            string example,
            string misconception,
            string safety,
            IReadOnlyList<string> observations,
            IReadOnlyList<string> teachingPoints,
            SparkAIKnowledgeQuestionType questionType,
            string measurement = "")
        {
            IsValid = isValid;
            KnowledgeId = knowledgeId;
            Title = title;
            Summary = summary;
            Definition = definition;
            Explanation = explanation;
            Example = example;
            Misconception = misconception;
            Safety = safety;
            Observations = observations;
            TeachingPoints = teachingPoints;
            QuestionType = questionType;
            Measurement = measurement;
        }

        public static SparkAIKnowledgeReasoningResult Invalid()
        {
            return new SparkAIKnowledgeReasoningResult(
                false,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                null,
                null,
                SparkAIKnowledgeQuestionType.Unknown,
                string.Empty);
        }
    }
}