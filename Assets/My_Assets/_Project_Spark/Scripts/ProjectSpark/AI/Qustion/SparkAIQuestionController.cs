using System;
using UnityEngine;
using System.Globalization;
using System.Text.RegularExpressions;

namespace ProjectSpark.AI
{
    /// <summary>
    /// Handles questions asked by the player.
    ///
    /// The controller does not invent electrical values.
    /// Answers are based on the current authoritative Project Spark
    /// world, level snapshot and AI reasoning systems.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkAIQuestionController : MonoBehaviour
    {
        [Header("AI References")]

        [SerializeField]
        private SparkAIContextReasoner contextReasoner;

        [SerializeField]
        private SparkAIObservationCoordinator observationCoordinator;

        [SerializeField]
        private SparkAIVoiceController voiceController;

        [Header("Settings")]

        [SerializeField]
        private bool allowQuestions = true;

        
        [Header("AI Knowledge")]
        [SerializeField]
        private SparkAIKnowledge knowledge;

        [SerializeField]
        private SparkAIWorld world;

        private SparkAIKnowledgeReasoner knowledgeReasoner;
        [SerializeField]
        private bool speakAnswers = true;

        [SerializeField]
        private bool logQuestions = true;

        private bool initialized;

        public bool IsInitialized => initialized;

        public bool AllowQuestions => allowQuestions;

        public event Action<SparkAIPlayerQuestion> QuestionAsked;

        public event Action<string> AnswerCreated;


        

        private void Awake()
{
    initialized = false;

    if (contextReasoner == null)
    {
        Debug.LogError(
            "[Spark AI Question] Context Reasoner is not assigned.",
            this);
        return;
    }

    if (observationCoordinator == null)
    {
        Debug.LogError(
            "[Spark AI Question] Observation Coordinator is not assigned.",
            this);
        return;
    }

    // Voice is optional when speech output is disabled.
    if (speakAnswers && voiceController == null)
    {
        Debug.LogWarning(
            "[Spark AI Question] Voice Controller is missing. " +
            "Answers will be generated without speech.",
            this);
    }

    if (knowledge != null && world != null)
    {
        knowledgeReasoner = new SparkAIKnowledgeReasoner(
            knowledge,
            world);
    }
    else
    {
        Debug.LogWarning(
            "[SPARK AI QUESTION] Knowledge or World is not assigned. " +
            "Knowledge reasoning will be unavailable.",
            this);
    }

    initialized = true;
}

        private void OnDestroy()
        {
            // No persistent subscriptions.
        }

        /// <summary>
        /// Entry point used by the player question UI.
        /// </summary>
      public void AskQuestion(string text)
{
    if (!allowQuestions)
        return;

    if (!initialized)
    {
        Debug.LogWarning(
            "[SPARK AI QUESTION] Cannot answer because the controller " +
            "has not initialized successfully.",
            this);
        return;
    }

    SparkAIPlayerQuestion question =
        SparkAIPlayerQuestion.Create(text);

    if (!question.IsValid)
        return;

    if (logQuestions)
    {
        Debug.Log(
            "[SPARK AI QUESTION] " + question.Text,
            this);
    }

    QuestionAsked?.Invoke(question);

    string answer = BuildAnswer(question.Text);

    if (string.IsNullOrWhiteSpace(answer))
    {
        answer =
            "I could not produce a reliable answer from the current " +
            "Project Spark observations.";
    }

    AnswerCreated?.Invoke(answer);

    if (logQuestions)
    {
        Debug.Log(
            "[SPARK AI ANSWER] " + answer,
            this);
    }

    SpeakAnswer(answer);
}


private bool TryBuildOhmsLawCalculationAnswer(
    string question,
    out string answer)
{
    answer = string.Empty;

    if (string.IsNullOrWhiteSpace(question))
        return false;

    string normalized = Normalize(question);

    bool asksCurrent = ContainsAny(
        normalized,
        "calculate current",
        "calculate the current",
        "find current",
        "find the current",
        "work out current",
        "solve for current",
        "current from voltage",
        "how much current",
        "what is the current",
        "current through",
        "current flowing");

    bool asksVoltage = ContainsAny(
        normalized,
        "calculate voltage",
        "calculate the voltage",
        "find voltage",
        "find the voltage",
        "work out voltage",
        "solve for voltage",
        "voltage from current",
        "what is the voltage",
        "voltage across");

    bool asksResistance = ContainsAny(
        normalized,
        "calculate resistance",
        "calculate the resistance",
        "find resistance",
        "find the resistance",
        "work out resistance",
        "solve for resistance",
        "resistance from voltage",
        "what is the resistance");

    if (ContainsAny(
            normalized,
            "calculate",
            "work out",
            "solve for"))
    {
        if (!asksCurrent && normalized.Contains("current"))
            asksCurrent = true;

        if (!asksVoltage && normalized.Contains("voltage"))
            asksVoltage = true;

        if (!asksResistance && normalized.Contains("resistance"))
            asksResistance = true;
    }

    if (!asksCurrent && !asksVoltage && !asksResistance)
        return false;

    // Voltage: V, volt, volts
    Match voltageMatch = Regex.Match(
        question,
        @"(?<![\w.])(?<value>\d+(?:\.\d+)?)\s*(?:V\b|volts?\b)",
        RegexOptions.IgnoreCase);

    // Resistance: Ω, ohm, kΩ, kohm, kiloohm
    Match resistanceMatch = Regex.Match(
        question,
        @"(?<![\w.])(?<value>\d+(?:\.\d+)?)\s*(?<unit>k\s*Ω|Ω|k(?:ilo)?\s*ohms?)(?!\w)",
        RegexOptions.IgnoreCase);

    // Current: A, amp, amps, ampere, amperes, mA, milliamp
    Match currentMatch = Regex.Match(
        question,
        @"(?<![\w.])(?<value>\d+(?:\.\d+)?)\s*(?<unit>m\s*A|A\b|milliamperes?|milliamps?|amperes?|amps?)(?!\w)",
        RegexOptions.IgnoreCase);

    float voltage = 0f;
    float resistance = 0f;
    float current = 0f;

    bool hasVoltage =
        voltageMatch.Success &&
        float.TryParse(
            voltageMatch.Groups["value"].Value,
            NumberStyles.Float,
            CultureInfo.InvariantCulture,
            out voltage);

    bool hasResistance = false;

    if (resistanceMatch.Success)
    {
        hasResistance = float.TryParse(
            resistanceMatch.Groups["value"].Value,
            NumberStyles.Float,
            CultureInfo.InvariantCulture,
            out resistance);

        if (hasResistance)
        {
            string unit = resistanceMatch.Groups["unit"].Value
                .Replace(" ", string.Empty)
                .ToLowerInvariant();

            if (unit.StartsWith("k"))
                resistance *= 1000f;
        }
    }

    bool hasCurrent = false;

    if (currentMatch.Success)
    {
        hasCurrent = float.TryParse(
            currentMatch.Groups["value"].Value,
            NumberStyles.Float,
            CultureInfo.InvariantCulture,
            out current);

        if (hasCurrent)
        {
            string unit = currentMatch.Groups["unit"].Value
                .Replace(" ", string.Empty)
                .ToLowerInvariant();

            if (unit.StartsWith("m") ||
                unit.StartsWith("milli"))
            {
                current /= 1000f;
            }
        }
    }

    // I = V / R
    if (asksCurrent)
    {
        if (hasVoltage && hasResistance)
        {
            if (resistance <= 0f)
            {
                answer =
                    "Resistance must be greater than zero to calculate current using Ohm's law.";

                return true;
            }

            current = voltage / resistance;

            answer =
                "Using Ohm's law: I = V / R.\n\n" +
                "Voltage = " +
                voltage.ToString("0.###", CultureInfo.InvariantCulture) +
                " V\n" +
                "Resistance = " +
                resistance.ToString("0.###", CultureInfo.InvariantCulture) +
                " Ω\n\n" +
                "I = " +
                voltage.ToString("0.###", CultureInfo.InvariantCulture) +
                " / " +
                resistance.ToString("0.###", CultureInfo.InvariantCulture) +
                "\nI = " +
                current.ToString("0.######", CultureInfo.InvariantCulture) +
                " A\n\n" +
                "Current = " +
                (current * 1000f).ToString("0.###", CultureInfo.InvariantCulture) +
                " mA";

            return true;
        }

        answer =
            "To calculate current, use I = V / R.\n\n" +
            "Provide both the voltage and resistance, for example: " +
            "\"Calculate current using 5 V and 1 kΩ.\"";

        return true;
    }

    // V = I × R
    if (asksVoltage)
    {
        if (hasCurrent && hasResistance)
        {
            voltage = current * resistance;

            answer =
                "Using Ohm's law: V = I × R.\n\n" +
                "Current = " +
                current.ToString("0.######", CultureInfo.InvariantCulture) +
                " A\n" +
                "Resistance = " +
                resistance.ToString("0.###", CultureInfo.InvariantCulture) +
                " Ω\n\n" +
                "V = I × R\n" +
                "V = " +
                voltage.ToString("0.###", CultureInfo.InvariantCulture) +
                " V";

            return true;
        }

        answer =
            "To calculate voltage, use V = I × R.\n\n" +
            "Provide both the current and resistance.";

        return true;
    }

    // R = V / I
    if (asksResistance)
    {
        if (hasVoltage && hasCurrent)
        {
            if (Mathf.Abs(current) < 0.000001f)
            {
                answer =
                    "Resistance cannot be calculated using R = V / I when current is zero.";

                return true;
            }

            resistance = voltage / current;

            answer =
                "Using Ohm's law: R = V / I.\n\n" +
                "Voltage = " +
                voltage.ToString("0.###", CultureInfo.InvariantCulture) +
                " V\n" +
                "Current = " +
                current.ToString("0.######", CultureInfo.InvariantCulture) +
                " A\n\n" +
                "R = V / I\n" +
                "R = " +
                resistance.ToString("0.###", CultureInfo.InvariantCulture) +
                " Ω";

            return true;
        }

        answer =
            "To calculate resistance, use R = V / I.\n\n" +
            "Provide both the voltage and current.";

        return true;
    }

    return false;
}



private bool IsAskingLedOnOff(string normalizedQuestion)
{
    // Fault-diagnosis questions must reach the diagnosis handler.
    if (ContainsAny(
            normalizedQuestion,
            "why",
            "not working",
            "doesn't work",
            "doesnt work",
            "not glowing",
            "not lighting",
            "not lighting up",
            "not conducting",
            "not turning on",
            "what is wrong",
            "what's wrong",
            "whats wrong",
            "what is the problem",
            "what is wrong with",
            "problem with"))
    {
        return false;
    }

    return ContainsAny(
        normalizedQuestion,

        // LED state
        "is led on",
        "is the led on",
        "led on",
        "is led off",
        "is the led off",
        "led off",
        "is the led lit",
        "is led lit",
        "is the led illuminated",
        "is led illuminated",
        "is the led glowing",
        "is led glowing",
        "is the led working",
        "is led working",
        "does the led work",

        // Bulb state
        "is the bulb on",
        "is bulb on",
        "is the bulb off",
        "is bulb off",
        "is the bulb lit",
        "is the bulb glowing",
        "is the bulb working",

        // Lamp state
        "is the lamp on",
        "is lamp on",
        "is the lamp off",
        "is lamp off",
        "is the lamp lit",
        "is the lamp glowing",
        "is the lamp working");
}

private string BuildObjectAnswer(
    string question,
    SparkAIUnifiedObservation observation)
{
    Debug.Log(
        "[SPARK AI OBJECT ENTRY] BuildObjectAnswer called. " +
        "Question=" + question);

    if ( observation.World == null)
    {
        return "I cannot read the current circuit state right now.";
    }

    SparkAIElectronicObjectSnapshot target;

    bool foundObject =
        SparkAIObjectQuestionResolver.TryFindObject(
            question,
            observation.World,
            out target);

    if (!foundObject)
    {
        Debug.LogWarning(
            "[SPARK AI OBJECT RESOLVER] No matching object. " +
            "Question=" + question);
        return BuildGeneralAnswer(
            observation,
            contextReasoner.Reason(observation));
    }

    string objectName =
        string.IsNullOrWhiteSpace(target.Name)
            ? "That component"
            : target.Name;

    string normalizedQuestion = Normalize(question);

   
    

    Debug.Log(
        "[SPARK AI OBJECT STATE] " +
        "Name=" + objectName +
        " | Active=" + target.Active +
        " | Electrical=" + target.IsElectricalComponent +
        " | Enabled=" + target.ElectricalEnabled +
        " | Conduction=" + target.ConductionState +
        " | Voltage=" + target.Voltage +
        " | Current=" + target.Current);

    // ============================================================
    // 1. WHY / PROBLEM QUESTIONS
    // ============================================================

    if (IsAskingWhyNotWorking(normalizedQuestion) ||
        ContainsAny(
            normalizedQuestion,
            "what is wrong",
            "whats wrong",
            "what is the problem",
            "problem",
            "issue",
            "why is",
            "why isn't",
            "why isnt",
            "why does",
            "why doesn't",
            "why doesnt",
            "not working",
            "doesn't work",
            "doesnt work",
            "not on",
            "isn't on",
            "isnt on",
            "is not on",
            "not turning on",
            "not lighting",
            "not lighting up",
            "not glowing",
            "not conducting",
            "not receiving power",
            "not powered",
            "won't turn on",
            "will not turn on"))
    {
        return BuildObjectProblemAnswer(
    target,
    objectName,
    observation,
    false
    ,false
    );
    }

    // ============================================================
    // 2. SPECIFIC MEASUREMENTS
    // ============================================================

    if (ContainsAny(
            normalizedQuestion,
            "voltage",
            "volt",
            "how many volts",
            "potential difference"))
    {
        return BuildObjectVoltageAnswer(
            target,
            objectName);
    }

    if (ContainsAny(
            normalizedQuestion,
            "current",
            "amps",
            "amp",
            "ampere",
            "amperes"))
    {
        return BuildObjectCurrentAnswer(
            target,
            objectName);
    }

    if (ContainsAny(
            normalizedQuestion,
            "power",
            "watt",
            "watts"))
    {
        return BuildObjectPowerAnswer(
            target,
            objectName);
    }

    // ============================================================
    // 3. CONDUCTION QUESTIONS
    // ============================================================

    if (ContainsAny(
            normalizedQuestion,
            "conducting",
            "conduct",
            "current flowing"))
    {
        return BuildObjectConductionAnswer(
            target,
            objectName);
    }

    // ============================================================
    // 4. LED / BULB ON-OFF QUESTIONS
    // Check before generic activity or powered-state routing.
    // ============================================================

    bool asksIlluminationState =
        ContainsAny(
            normalizedQuestion,
            "is led on",
            "is the led on",
            "led on",
            "is led off",
            "is the led off",
            "led off",
            "is the led lit",
            "is the led illuminated",
            "is the led lighting",
            "is the bulb on",
            "is the bulb off",
            "is bulb on",
            "is bulb off",
            "is the bulb lit");

    bool targetLooksLikeLedOrBulb =
        objectName.IndexOf(
            "led",
            StringComparison.OrdinalIgnoreCase) >= 0 ||
        objectName.IndexOf(
            "bulb",
            StringComparison.OrdinalIgnoreCase) >= 0 ||
        objectName.IndexOf(
            "lamp",
            StringComparison.OrdinalIgnoreCase) >= 0;

    if (asksIlluminationState && targetLooksLikeLedOrBulb)
    {
        if (!target.Active || !target.ElectricalEnabled)
        {
            return objectName +
                   " is OFF because the component is inactive or its electrical model is disabled.";
        }

       bool conducting = string.Equals(
    target.ConductionState,
    "Conducting",
    StringComparison.OrdinalIgnoreCase);

        if (conducting && target.Current > 0f)
        {
            return "Yes, " + objectName +
                   " is electrically conducting with forward current. " +
                   "Its electrical state indicates that it should be ON.";
        }

        return "No, " + objectName +
               " is not currently confirmed ON. " +
               "Its conduction state is " +
               (string.IsNullOrWhiteSpace(target.ConductionState)
                   ? "unknown"
                   : target.ConductionState) +
               ", and its reported current is " +
               target.Current.ToString("F4") +
               " A.";
    }

    // ============================================================
    // 5. POWERED / ACTIVE QUESTIONS
    // ============================================================

    if (ContainsAny(
            normalizedQuestion,
            "receiving power",
            "getting power",
            "powered",
            "has power",
            "energized",
            "energy"))
    {
        return BuildObjectPowerStateAnswer(
            target,
            objectName);
    }

    // ============================================================
    // 6. GENERIC ON / OFF QUESTIONS
    // ============================================================

    if (ContainsAny(
            normalizedQuestion,
            "on",
            "off",
            "active",
            "inactive"))
    {
        return BuildObjectActivityAnswer(
            target,
            objectName);
    }

    // ============================================================
    // 7. GENERAL OBJECT QUESTION
    // ============================================================

    return BuildObjectGeneralAnswer(
        target,
        objectName);
}

private string BuildObjectVoltageAnswer(
    SparkAIElectronicObjectSnapshot target,
    string objectName)
{
    if (float.IsNaN(target.Voltage) ||
        float.IsInfinity(target.Voltage))
    {
        return "The voltage reading for " + objectName +
               " is unavailable or invalid in the current snapshot.";
    }

    return "The current electrical snapshot reports " +
           FormatValue(target.Voltage, "V") +
           " across " + objectName +
           ". This is the reported component voltage, not an independent meter measurement.";
}


private string BuildObjectCurrentAnswer(
    SparkAIElectronicObjectSnapshot target,
    string objectName)
{
    return objectName +
           " currently has " +
           FormatValue(target.Current, "A") +
           " of current.";
}

private string BuildObjectPowerAnswer(
    SparkAIElectronicObjectSnapshot target,
    string objectName)
{
    return objectName +
           " is currently using approximately " +
           FormatValue(target.Power, "W") +
           " of electrical power.";
}

private string BuildObjectConductionAnswer(
    SparkAIElectronicObjectSnapshot target,
    string objectName)
{
    string state =
        string.IsNullOrWhiteSpace(target.ConductionState)
            ? string.Empty
            : target.ConductionState.Trim();

    if (string.Equals(
            state,
            "Conducting",
            StringComparison.OrdinalIgnoreCase))
    {
        return objectName +
               " is currently conducting current.";
    }

    if (string.Equals(
            state,
            "NonConducting",
            StringComparison.OrdinalIgnoreCase))
    {
        return objectName +
               " is currently not conducting current.";
    }

    if (Math.Abs(target.Current) > 0.000001f)
    {
        return objectName +
               " has current flowing through it.";
    }

    return objectName +
           " is not currently showing significant current flow.";
}
private string BuildObjectPowerStateAnswer(
    SparkAIElectronicObjectSnapshot target,
    string objectName)
{
    if (!target.Active)
    {
        return objectName +
               " is currently inactive.";
    }

    if (!target.ElectricalEnabled)
    {
        return objectName +
               " is currently electrically disabled.";
    }

    if (Math.Abs(target.Voltage) > 0.000001f ||
        Math.Abs(target.Current) > 0.000001f)
    {
        return objectName +
               " is electrically active. " +
               "Voltage is " +
               FormatValue(target.Voltage, "V") +
               " and current is " +
               FormatValue(target.Current, "A") +
               ".";
    }

    return objectName +
           " is active, but it is not currently receiving significant electrical energy.";
}

private string BuildObjectActivityAnswer(
    SparkAIElectronicObjectSnapshot target,
    string objectName)
{
    if (!target.Active)
    {
        return objectName +
               " is currently inactive.";
    }

    if (!target.ElectricalEnabled)
    {
        return objectName +
               " is active, but its electrical function is disabled.";
    }

    return objectName +
           " is currently active.";
}


private string BuildObjectProblemAnswer(
    SparkAIElectronicObjectSnapshot target,
    string objectName,
    SparkAIUnifiedObservation observation,
    bool asksAboutReverseBias,
    bool asksAboutLedNotGlowing)
{
    // ============================================================
    // 1. OBJECT AVAILABILITY
    // ============================================================

    if (!target.ElectricalEnabled)
    {
        return objectName +
               " is electrically disabled. Enable its electrical " +
               "behavior before testing the circuit.";
    }

    if (!target.Active)
    {
        return objectName +
               " is inactive. Check whether the component is enabled " +
               "and configured correctly in the scene.";
    }

    SparkAIWorldSnapshot world = observation.World;
    bool hasValidCurrent =
        !float.IsNaN(target.Current) &&
        !float.IsInfinity(target.Current);

    bool hasSignificantComponentCurrent =
        hasValidCurrent &&
        Math.Abs(target.Current) > 0.000001f;
    // ============================================================
    // 2. CHECK THE COMPONENT'S OWN ELECTRICAL STATE FIRST
    //
    // Do not diagnose the whole circuit as open before inspecting
    // the actual component's voltage, current, and terminal polarity.
    // ============================================================

    bool hasTerminalIds =
        target.AnodeTerminalInstanceId != 0 &&
        target.CathodeTerminalInstanceId != 0;

    if (world != null && hasTerminalIds)
    {
        float anodeVoltage = 0f;
        float cathodeVoltage = 0f;

        bool foundAnode = false;
        bool foundCathode = false;

        if (logQuestions)
{
    Debug.Log(
        "[SPARK AI LED TERMINAL IDS] " +
        "Object=" + objectName +
        " | AnodeID=" + target.AnodeTerminalInstanceId +
        " | CathodeID=" + target.CathodeTerminalInstanceId +
        " | TerminalCount=" + world.Terminals.Count);
}

        for (int i = 0; i < world.Terminals.Count; i++)
        {
            SparkAITerminalSnapshot terminal =
                world.Terminals[i];


    if (logQuestions)
    {
        Debug.Log(
            "[SPARK AI LED TERMINAL SNAPSHOT] " +
            "ID=" + terminal.InstanceId +
            " | Name=" + terminal.Name +
            " | Owner=" + terminal.OwnerName +
            " | Voltage=" + terminal.Voltage);
    }


            if (terminal.InstanceId ==
                target.AnodeTerminalInstanceId)
            {
                anodeVoltage = terminal.Voltage;
                foundAnode = true;
            }

            if (terminal.InstanceId ==
                target.CathodeTerminalInstanceId)
            {
                cathodeVoltage = terminal.Voltage;
                foundCathode = true;
            }

            if (foundAnode && foundCathode)
            {
                break;
            }
        }


        bool validAnodeVoltage =
            foundAnode &&
            !float.IsNaN(anodeVoltage) &&
            !float.IsInfinity(anodeVoltage);

        bool validCathodeVoltage =
            foundCathode &&
            !float.IsNaN(cathodeVoltage) &&
            !float.IsInfinity(cathodeVoltage);

        if (validAnodeVoltage && validCathodeVoltage)
        {
            float ledVoltage = anodeVoltage - cathodeVoltage;

            const float polarityTolerance = 0.05f;

            // ----------------------------------------------------
            // 2A. REVERSE-BIASED LED
            // ----------------------------------------------------

            if (ledVoltage < -polarityTolerance)
            {
                return objectName +
                       " appears reverse-biased according to the current " +
                       "terminal-voltage snapshot. The anode is at " +
                       FormatValue(anodeVoltage, "V") +
                       " and the cathode is at " +
                       FormatValue(cathodeVoltage, "V") +
                       ", giving " +
                       FormatValue(ledVoltage, "V") +
                       " from anode to cathode. Verify the terminal " +
                       "identification and connect the anode toward the " +
                       "positive side and the cathode toward the negative " +
                       "side of the supply. Avoid exceeding the LED's " +
                       "reverse-voltage rating.";
            } 
            
       
            if (asksAboutReverseBias &&
                ledVoltage >= -polarityTolerance)
            {
                string answer =
                    objectName +
                    " is not reverse-biased according to the current " +
                    "terminal-voltage snapshot. The anode is at " +
                    FormatValue(anodeVoltage, "V") +
                    " and the cathode is at " +
                    FormatValue(cathodeVoltage, "V") +
                    ", giving " +
                    FormatValue(ledVoltage, "V") +
                    " from anode to cathode.";

                if (hasSignificantComponentCurrent)
                {
                    answer +=
                        " The component also reports " +
                        FormatValue(target.Current, "A") +
                        " of current, which supports electrical conduction.";
                }
                else
                {
                    answer +=
                        " These voltages do not establish whether the circuit " +
                        "has a complete return path. Check the actual terminal " +
                        "connections and the supply return before drawing a " +
                        "conclusion about current flow.";
                }

                return answer;
            }


            if (asksAboutReverseBias)
            {
                return objectName +
                    " cannot be confirmed as reverse-biased from the current " +
                    "terminal-voltage snapshot. The anode is at " +
                    FormatValue(anodeVoltage, "V") +
                    " and the cathode is at " +
                    FormatValue(cathodeVoltage, "V") +
                    ", giving " +
                    FormatValue(ledVoltage, "V") +
                    " from anode to cathode. A negative voltage difference " +
                    "beyond the polarity tolerance would indicate reverse bias. " +
                    "Check the terminal connections and the supply return path.";
            }                    
        }
    }

    // ============================================================
    // 3. COMPONENT CURRENT
    //
    // Current through the component is stronger evidence of
    // conduction than a global level flag.
    // ============================================================

        
if (asksAboutLedNotGlowing &&
    hasSignificantComponentCurrent)
{
    return objectName +
           " reports " +
           FormatValue(target.Current, "A") +
           " of current, which indicates electrical conduction " +
           "in the current snapshot. However, this reading alone " +
           "cannot confirm that the LED is visibly illuminated. " +
           "Check whether the LED's visual component is enabled " +
           "and whether its rendered light state follows its " +
           "electrical operating state.";
}


    if (target.IsElectricalComponent &&
    hasSignificantComponentCurrent)
    {
        return objectName +
               " currently has " +
               FormatValue(target.Current, "A") +
               " flowing through it. The electrical snapshot " +
               "indicates current is flowing. If the component " +
               "appears dark or inactive, check whether its visual " +
               "state is synchronized with its electrical state.";
    }

    // ============================================================
    // 4. COMPONENT VOLTAGE WITH NO SIGNIFICANT CURRENT
    // ============================================================

    const float voltageTolerance = 0.000001f;
    const float currentTolerance = 0.000001f;

    bool hasSignificantVoltage =
        Math.Abs(target.Voltage) > voltageTolerance;

    bool hasSignificantCurrent =
    hasValidCurrent &&
    Math.Abs(target.Current) > currentTolerance;

    if (!hasValidCurrent ||
    float.IsNaN(target.Voltage) ||
    float.IsInfinity(target.Voltage))
{
    return objectName +
           " cannot be diagnosed reliably because its electrical " +
           "voltage or current reading is invalid. Check the electrical " +
           "solver result and the component's electrical snapshot.";
}
    // ============================================================
    // 5. CIRCUIT-LEVEL DIAGNOSIS
    //
    // These are level-wide findings, not proof of a fault inside
    // this specific component. Report the evaluator's finding
    // accurately and prioritize an explicit source short.
    // ============================================================

    if (world != null)
    {
        if (world.Level.SourceShorted)
        {
            return objectName +
                   " is not operating as expected. The level evaluator " +
                   "reports a short circuit at the source. Inspect the " +
                   "wiring for an unintended direct connection between " +
                   "the supply terminals before continuing.";
        }

        if (!world.Level.HasValidPowerSource)
        {
            return objectName +
                   " may not be operating because the level evaluator " +
                   "reports no valid active power source. Check the " +
                   "supply's electrical enable state, output switch, " +
                   "voltage, and connections.";
        }

        if (!world.Level.ClosedReturn)
        {
            return objectName +
                   " is not conducting in the current snapshot, and the " +
                   "level evaluator reports that the circuit's return " +
                   "path is incomplete. This may explain the component's " +
                   "state, but it does not by itself prove that the " +
                   "component is faulty. Trace the path from the supply's " +
                   "positive terminal through the circuit and back to " +
                   "the supply's negative terminal.";
        }
    }


    
    // ============================================================
    // 6. NO SIGNIFICANT VOLTAGE OR CURRENT
    // ============================================================

    if (!hasSignificantVoltage && !hasSignificantCurrent)
    {
        string stateDescription =
            string.Equals(
                target.ConductionState,
                "Conducting",
                StringComparison.OrdinalIgnoreCase)
                ? "The snapshot reports a conducting state, but the " +
                  "measured voltage and current are both approximately zero. "
                : string.Equals(
                    target.ConductionState,
                    "NonConducting",
                    StringComparison.OrdinalIgnoreCase)
                    ? "The snapshot reports that the component is not conducting. "
                    : "The component's conduction state is unknown. ";

        return objectName +
               " has approximately 0 V across it and approximately 0 A " +
               "through it in the current electrical snapshot. " +
               stateDescription +
               "These readings alone cannot identify the cause. Check " +
               "whether the source is producing voltage, whether the " +
               "component terminals are connected to different electrical " +
               "potentials, and whether the intended circuit path is complete.";
    }


    // ============================================================
    // 7. FALLBACK
    // ============================================================

    return objectName +
           " is not showing a clear electrical fault in the available " +
           "snapshot. Check its terminal voltages, current, connections, " +
           "and visual-state synchronization.";
}
private string BuildObjectGeneralAnswer(
    SparkAIElectronicObjectSnapshot target,
    string objectName)
{
    return objectName +
           " is currently " +
           (target.Active ? "active" : "inactive") +
           ", with " +
           FormatValue(target.Voltage, "V") +
           ", " +
           FormatValue(target.Current, "A") +
           ", and approximately " +
           FormatValue(target.Power, "W") +
           ".";
}



private string BuildTerminalAnswer(
    string question,
    SparkAIUnifiedObservation observation)
{
    SparkAITerminalSnapshot terminal;

    if (!SparkAITerminalQuestionResolver.TryFindTerminal(
            question,
            observation.World,
            out terminal))
    {
        return
            "I could not identify the specific terminal you are asking about.";
    }

    string terminalName =
        string.IsNullOrWhiteSpace(terminal.Name)
            ? "That terminal"
            : terminal.Name;

    /*
     * Find every connection involving this terminal.
     */
    int connectionCount = 0;

    SparkAIConnectionSnapshot matchingConnection =
        default;

    for (int i = 0;
         i < observation.World.Connections.Count;
         i++)
    {
        SparkAIConnectionSnapshot connection =
            observation.World.Connections[i];

        if (connection.SourceTerminalInstanceId ==
            terminal.InstanceId)
        {
            connectionCount++;

            if (connectionCount == 1)
                matchingConnection = connection;
        }

        else if (connection.TargetTerminalInstanceId ==
                 terminal.InstanceId)
        {
            connectionCount++;

            if (connectionCount == 1)
                matchingConnection = connection;
        }
    }

    /*
     * No connection.
     */
    if (connectionCount == 0)
    {
        return terminalName +
               " is currently not connected to another terminal.";
    }

    /*
     * One connection.
     */
    if (connectionCount == 1)
    {
        string connectedName =
            GetOtherTerminalName(
                matchingConnection,
                terminal.InstanceId);

        if (string.IsNullOrWhiteSpace(connectedName))
        {
            return terminalName +
                   " has one connection.";
        }

        return terminalName +
               " is connected to " +
               connectedName +
               ".";
    }

    /*
     * Multiple connections.
     */
    return terminalName +
           " currently has " +
           connectionCount +
           " connections.";
}

private string GetOtherTerminalName(
    SparkAIConnectionSnapshot connection,
    int terminalInstanceId)
{
    if (connection.SourceTerminalInstanceId ==
        terminalInstanceId)
    {
        return connection.TargetTerminalName;
    }

    if (connection.TargetTerminalInstanceId ==
        terminalInstanceId)
    {
        return connection.SourceTerminalName;
    }

    return string.Empty;
}


        // =========================================================
        // ANSWER ENGINE
        // =========================================================
        private string BuildAnswer(string question)
        {
            SparkAIQuestionIntentResult intent =
                SparkAIQuestionIntentResolver.Resolve(question);

            SparkAIUnifiedObservation observation =
                observationCoordinator.ObserveCurrentWorld();

            if (!observation.IsValid)
            {
                return "I cannot read the Project Spark circuit right now.";
            }

            if (observation.World == null)
            {
                return "I cannot read the current Project Spark world state.";
            }

        SparkAIContextReasoning context =
            contextReasoner.Reason(observation);
                
            string normalizedQuestion = Normalize(question);
             bool asksAboutReverseBias =
    ContainsAny(
        normalizedQuestion,
        "does not light",
        "doesn't light",
        "reverse biased",
        "reverse-biased",
        "reverse bias");



            // Explicit LED/Bulb state questions.
            // Never select a generic Bulb when the player asked
            // specifically about an LED.
            bool asksWhetherLedIsOn =
                IsAskingLedOnOff(normalizedQuestion);

            bool explicitlyAsksAboutLed =
                ContainsAny(
                    normalizedQuestion,
                    "led",
                    "light emitting diode");

            bool explicitlyAsksAboutBulb =
                ContainsAny(
                    normalizedQuestion,
                    "bulb",
                    "lamp");

                    
            bool asksAboutLedNotGlowing =
                explicitlyAsksAboutLed &&
                ContainsAny(
                    normalizedQuestion,
                    "not glowing",
                    "not lighting",
                    "not lighting up",
                    "not illuminated",
                    "looks dark",
                    "appears dark",
                    "is dark");               

                    
            bool asksLedDiagnosis =
                IsAskingWhyNotWorking(normalizedQuestion) ||
                ContainsAny(
                    normalizedQuestion,
                    "why is",
                    "why isn't",
                    "why isnt",
                    "why does",
                    "why doesn't",
                    "why doesnt",
                    "not working",
                    "not glowing",
                    "not lighting",
                    "not lighting up",
                    "not turning on",
                    "not conducting",
                    "what is wrong",
                    "what's wrong",
                    "whats wrong",
                    "what is the problem");

            if (asksWhetherLedIsOn && !asksLedDiagnosis)
            {
                SparkAIElectronicObjectSnapshot stateTarget = default;          
              
                bool foundStateTarget = false;

                
                if (logQuestions)
                {
                    for (int i = 0;
                         i < observation.World.ElectronicObjects.Count;
                         i++)
                    {
                        SparkAIElectronicObjectSnapshot item =
                            observation.World.ElectronicObjects[i];

                       /* Debug.Log(
                            "[SPARK AI OBJECT CANDIDATE] " +
                            "Name=" + item.Name +
                            " | Active=" + item.Active +
                            " | ElectricalEnabled=" + item.ElectricalEnabled +
                            " | Conduction=" + item.ConductionState +
                            " | Current=" + item.Current);*/
                    }
                }


                for (int i = 0;
                     i < observation.World.ElectronicObjects.Count;
                     i++)
                {
                    SparkAIElectronicObjectSnapshot candidate =
                        observation.World.ElectronicObjects[i];

                    if (string.IsNullOrWhiteSpace(candidate.Name))
                        continue;

                    bool nameMatches =
                        explicitlyAsksAboutLed
                            ? ContainsAny(
                                Normalize(candidate.Name),
                                "led",
                                "light emitting diode")
                            : explicitlyAsksAboutBulb
                                ? ContainsAny(
                                    Normalize(candidate.Name),
                                    "bulb",
                                    "lamp")
                                : false;

                    if (!nameMatches)
                        continue;

                    stateTarget = candidate;
                    foundStateTarget = true;
                    break;
                }

                if (!foundStateTarget)
                {
                    return explicitlyAsksAboutLed
                        ? "I cannot identify an LED in the current circuit snapshot."
                        : "I cannot identify the requested light component in the current circuit snapshot.";
                }

                return BuildLedStateAnswer(stateTarget);
            }


           
        // ============================================================
        // LIVE COMPONENT STATE QUESTIONS
        // Must run before educational knowledge resolution.
        // ============================================================

      /*  bool asksWhetherLedIsOn =
            ContainsAny(
                normalizedQuestion,

                // LED state
                "is led on",
                "is the led on",
                "led on",
                "is led off",
                "is the led off",
                "led off",

                // LED operation / illumination
                "is the led working",
                "is led working",
                "does the led work",
                "is the led illuminated",
                "is led illuminated",
                "is the led lighting",
                "is the led lit",
                "is led lit",
                "is the led glowing",
                "is led glowing",
                "is the led turning on",
                "did the led turn on",
                "why is the led not glowing",

                // Bulb / lamp state
                "is the bulb on",
                "is bulb on",
                "is the bulb off",
                "is bulb off",
                "is the bulb working",
                "is the bulb lit",
                "is the bulb glowing",
                "is the lamp on",
                "is lamp on",
                "is the lamp off",
                "is the lamp working",
                "is the lamp lit");
*/

            // ============================================================
            // 1. AUTHORITATIVE LEVEL STATUS
            // ============================================================

            bool asksLevelStatus =
                ContainsAny(
                    normalizedQuestion,
                    "did i complete the level",
                    "did i complete level",
                    "have i completed the level",
                    "have i completed level",
                    "is the level complete",
                    "is level complete",
                    "is the level completed",
                    "is level completed",
                    "level complete",
                    "level completed",
                    "level status",
                    "level progress",
                    "did i win",
                    "have i won",
                    "am i finished",
                    "is this level finished",
                    "is level finished");

            bool asksCompletionTargetVoltage =
                ContainsAny(
                    normalizedQuestion,
                    "target voltage at completion",
                    "voltage at completion",
                    "target voltage when completed",
                    "voltage when completed");

            if (asksLevelStatus)
            {
                if (observation.World.Level.Completed)
                {
                    return BuildCompletedLevelAnswer(
                        normalizedQuestion,
                        observation);
                }

                return
                    "The current level is not marked as completed yet. " +
                    "Keep working until the level's actual completion " +
                    "conditions are satisfied.";
            }

            if (asksCompletionTargetVoltage)
            {
                if (observation.World.Level.Completed)
                {
                    return BuildCompletedLevelAnswer(
                        normalizedQuestion,
                        observation);
                }

                return
                    "The level has not been completed yet, so there is " +
                    "no completed-level target voltage to report.";
            }

            // ============================================================
            // 2. QUESTION CLASSIFICATION
            // ============================================================

            bool isMeasurementQuestion =
                SparkAIKnowledgeQuestionResolver.Resolve(question).Type ==
                SparkAIKnowledgeQuestionType.Measurement;

            bool isCalculationQuestion =
                normalizedQuestion.Contains("calculate") ||
                normalizedQuestion.Contains("ohm's law") ||
                normalizedQuestion.Contains("ohms law");

            // ============================================================
            // 3. SPECIFIC TERMINAL QUESTIONS
            // ============================================================

            bool looksLikeTerminalQuestion =
                ContainsAny(
                    normalizedQuestion,
                    "terminal",
                    "anode",
                    "cathode",
                    "positive side",
                    "negative side",
                    "positive terminal",
                    "negative terminal",
                    "pin",
                    "connected to",
                    "connect to",
                    "where does",
                    "where is connected");

            if (looksLikeTerminalQuestion)
            {
                SparkAITerminalSnapshot terminalTarget;

                if (SparkAITerminalQuestionResolver.TryFindTerminal(
                        question,
                        observation.World,
                        out terminalTarget))
                {
                    Debug.Log(
                        "[SPARK AI TERMINAL DEBUG] " +
                        "Terminal=" + terminalTarget.Name +
                        " | Owner=" + terminalTarget.OwnerName +
                        " | Polarity=" + terminalTarget.Polarity +
                        " | EffectivePolarity=" + terminalTarget.EffectivePolarity +
                        " | InstanceID=" + terminalTarget.InstanceId);

                    return BuildTerminalAnswer(
                        question,
                        observation);
                }
            }

            // ============================================================
            // 4. LED POLARITY QUESTIONS
            // ============================================================

            bool mentionsLed =
                ContainsAny(
                    normalizedQuestion,
                    "led",
                    "light emitting diode");

            bool asksLedPolarity =
                ContainsAny(
                    normalizedQuestion,
                    "correct polarity",
                    "wrong polarity",
                    "reversed polarity",
                    "reverse polarity",
                    "led polarity",
                    "led connected correctly",
                    "connected with correct polarity",
                    "connected with the correct polarity",
                    "led backwards",
                    "led backward");

            if (mentionsLed && asksLedPolarity)
            {
                return BuildObjectPolarityAnswer(
                    question,
                    observation);
            }
        // ============================================================
        // 5. RESOLVE THE LIVE ELECTRONIC OBJECT
        // ============================================================

        SparkAIElectronicObjectSnapshot objectTarget;

        bool hasObjectTarget =
            SparkAIObjectQuestionResolver.TryFindObject(
                question,
                observation.World,
                out objectTarget);

                

        bool asksObjectProblem =
            IsAskingWhyNotWorking(normalizedQuestion) ||
            ContainsAny(
                normalizedQuestion,
                "what is wrong",
                "what's wrong",
                "whats wrong",
                "what is the problem",
                "problem",
                "issue",
                "why is",
                "why isn't",
                "why isnt",
                "why does",
                "why doesn't",
                "why doesnt",
                "not working",
                "doesn't work",
                "doesnt work",
                "not on",
                "isn't on",
                "isnt on",
                "is not on",
                "not turning on",
                "not lighting",
                "not lighting up",
                "not glowing",
                "not conducting",
                "not receiving power",
                "not powered",
                "won't turn on",
                "will not turn on",
                "does not light",
                "doesn't light");

        // Explicit LED fallback if name matching did not resolve it.
       
        // Explicit LED diagnosis target.
        // Never diagnose an unrelated object when the player
        // explicitly asks why an LED is not working.
        if (mentionsLed && asksObjectProblem)
        {
            bool resolvedTargetIsLed =
                hasObjectTarget &&
                !string.IsNullOrWhiteSpace(objectTarget.Name) &&
                (
                    objectTarget.Name.IndexOf(
                        "LED",
                        System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                    objectTarget.Name.IndexOf(
                        "light emitting diode",
                        System.StringComparison.OrdinalIgnoreCase) >= 0
                );

            if (!resolvedTargetIsLed)
            {
                objectTarget = default;
                hasObjectTarget = false;

                for (int i = 0;
                     i < observation.World.ElectronicObjects.Count;
                     i++)
                {
                    SparkAIElectronicObjectSnapshot candidate =
                        observation.World.ElectronicObjects[i];

                    if (string.IsNullOrWhiteSpace(candidate.Name))
                        continue;

                    bool candidateIsLed =
                        candidate.Name.IndexOf(
                            "LED",
                            System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                        candidate.Name.IndexOf(
                            "light emitting diode",
                            System.StringComparison.OrdinalIgnoreCase) >= 0;

                    if (!candidateIsLed)
                        continue;

                    objectTarget = candidate;
                    hasObjectTarget = true;

                    if (logQuestions)
                    {
                        Debug.Log(
                            "[SPARK AI LED DIAGNOSIS] Target=" +
                            candidate.Name);
                    }

                    break;
                }
            }
        }


        Debug.Log(
            "[SPARK AI OBJECT ROUTING] Question=" + question +
            " | HasObjectTarget=" + hasObjectTarget +
            " | ObjectName=" +
            (hasObjectTarget ? objectTarget.Name : "<NONE>"));          

        // ============================================================
        // 6. LIVE OBJECT DIAGNOSIS
        // ============================================================
        

if (asksWhetherLedIsOn && !asksLedDiagnosis)
{
    // Simple LED state questions are handled by
    // BuildLedStateAnswer earlier in this method.
    // Do not route them into generic problem diagnosis.
}
else if (hasObjectTarget && asksObjectProblem)
{
    string objectName =
        string.IsNullOrWhiteSpace(objectTarget.Name)
            ? "That component"
            : objectTarget.Name;

    if (logQuestions)
    {
        Debug.Log(
            "[SPARK AI DIAGNOSIS ROUTING] " +
            "Question=" + normalizedQuestion +
            " | Object=" + objectName +
            " | AsksAboutReverseBias=" + asksAboutReverseBias +
            " | AsksAboutLedNotGlowing=" + asksAboutLedNotGlowing);
    }

    return BuildObjectProblemAnswer(
        objectTarget,
        objectName,
        observation,
        asksAboutReverseBias,
        asksAboutLedNotGlowing);
}


            // ============================================================
            // 7. LIVE MEASUREMENT CLASSIFICATION
            // ============================================================

        bool explicitCalculation =
            SparkAIQuestionIntentResolver.IsExplicitCalculationQuestion(
                question) ||
            SparkAIKnowledgeQuestionResolver.Resolve(question).Type ==
                SparkAIKnowledgeQuestionType.Calculation;

        bool asksLiveVoltage =
            ContainsAny(
                normalizedQuestion,
                "voltage across",
                "voltage at",
                "voltage on",
                "what voltage",
                "how much voltage");

        bool asksLiveCurrent =
            ContainsAny(
                normalizedQuestion,
                "current flowing",
                "current through",
                "current in",
                "how much current",
                "what current");

        bool mentionsCircuitObject =
            ContainsAny(
                normalizedQuestion,
                "led",
                "resistor",
                "switch",
                "source",
                "battery",
                "circuit",
                "component",
                "motor",
                "lamp",
                "bulb");

        bool asksLiveCircuitValue =
            (asksLiveVoltage || asksLiveCurrent) &&
            mentionsCircuitObject;

        // A calculation using supplied values must not be blocked just
        // because it mentions a component or says "current through".
        bool explicitLiveRequest =
            IsExplicitLiveValueQuestion(normalizedQuestion);

        if (explicitCalculation && !explicitLiveRequest)
        {
            string explicitCalculationAnswer;

            if (TryBuildOhmsLawCalculationAnswer(
                    question,
                    out explicitCalculationAnswer))
            {
                return explicitCalculationAnswer;
            }
        }

        if (!explicitCalculation &&
            !asksLiveCircuitValue &&
            !isMeasurementQuestion)
        {
            

            // ============================================================
            // 8. OHM'S LAW CALCULATION
            // Do not intercept actual live circuit measurements.
            // ============================================================

            string calculationAnswer;

            if (TryBuildOhmsLawCalculationAnswer(
                    question,
                    out calculationAnswer))
            {
                return calculationAnswer;
            }
        }

            // ============================================================
            // 9. COMPLETED LEVEL QUESTIONS
            // Additional completion wording.
            // ============================================================

            bool asksLevelCompletion =
                ContainsAny(
                    normalizedQuestion,
                    "did i complete",
                    "have i completed",
                    "is the level complete",
                    "is the level completed",
                    "did i win",
                    "have i won",
                    "level completion",
                    "level result");

            if (observation.World.Level.Completed &&
                asksLevelCompletion)
            {
                return BuildCompletedLevelAnswer(
                    normalizedQuestion,
                    observation);
            }

            // ============================================================
            // 10. OBJECT-AWARE LIVE QUESTIONS
            // ============================================================

            bool isLiveObjectMeasurement =
                hasObjectTarget && asksLiveCircuitValue;

            bool asksObjectStateOrValue =
                ContainsAny(
                    normalizedQuestion,
                    "why",
                    "working",
                    "work",
                    "wrong",
                    "problem",
                    "voltage",
                    "current",
                    "power",
                    "receiving",
                    "powered",
                    "powering",
                    "conducting",
                    "on",
                    "off",
                    "active",
                    "inactive",
                    "connected",
                    "disconnected",
                    "resistance",
                    "resistive");

            if ((!isMeasurementQuestion || isLiveObjectMeasurement) &&
                !isCalculationQuestion &&
                hasObjectTarget &&
                asksObjectStateOrValue)
            {
                return BuildObjectAnswer(
                    question,
                    observation);
            }

            // ============================================================
            // 11. EDUCATIONAL MEASUREMENT QUESTIONS
            // ============================================================

            if (isMeasurementQuestion &&
                knowledgeReasoner != null)
            {
                SparkAIKnowledgeReasoningResult measurementResult;

                if (knowledgeReasoner.TryExplainQuestion(
                        question,
                        out measurementResult))
                {
                    string measurementAnswer =
                        BuildKnowledgeReasonedAnswer(measurementResult);

                    if (!string.IsNullOrWhiteSpace(measurementAnswer))
                    {
                        return measurementAnswer;
                    }
                }
            }

            // ============================================================
            // 12. IDENTIFY LIVE CIRCUIT QUESTIONS
            // Prevent generic educational answers from intercepting them.
            // ============================================================

            bool isLiveCircuitQuestion =
                ContainsAny(
                    normalizedQuestion,

                    // Circuit diagnosis
                    "why isn't",
                    "why is not",
                    "why doesn't",
                    "why does not",
                    "not working",
                    "isn't working",
                    "is not working",
                    "doesn't work",
                    "does not work",
                    "not lighting",
                    "not lighting up",
                    "not turning on",
                    "not on",
                    "isn't on",
                    "is not on",
                    "not glowing",
                    "what is wrong",
                    "what's wrong",
                    "where is the problem",
                    "find the problem",
                    "problem with",

                    // Live measurements
                    "what voltage",
                    "how much voltage",
                    "how much current",
                    "how much power",
                    "is it connected",
                    "is it conducting",

                    // Voltage
                    "voltage across",
                    "voltage at",
                    "voltage on",
                    "measure voltage across",
                    "measure the voltage across",
                    "measure voltage at",
                    "check voltage across",
                    "check voltage at",

                    // Current
                    "current flowing",
                    "current through",
                    "current in the",
                    "how much current is flowing",
                    "measure current through",
                    "measure the current through",
                    "check current through",

                    // Power
                    "power consumed by",
                    "power used by",
                    "power dissipated by",
                    "power of the led",
                    "power of the resistor",

                    // Live component state
                    "is the led on",
                    "is the led off",
                    "is the resistor conducting",
                    "is the switch open",
                    "is the switch closed",
                    "is the component powered",
                    "is the component receiving power",
                    "is the battery supplying",
                    "is the source supplying");

            // ============================================================
            // 13. GENERAL KNOWLEDGE
            // Only for questions that are not live circuit diagnosis.
            // ============================================================

            if (!isLiveCircuitQuestion)
            {
                SparkAIKnowledgeReasoningResult knowledgeResult;

                if (knowledgeReasoner != null &&
                    knowledgeReasoner.TryExplainQuestion(
                        question,
                        out knowledgeResult))
                {
                    string knowledgeAnswer =
                        BuildKnowledgeReasonedAnswer(knowledgeResult);

                    if (!string.IsNullOrWhiteSpace(knowledgeAnswer))
                    {
                        return knowledgeAnswer;
                    }
                }
            }

            // ============================================================
            // 14. QUESTION INTENT
            // ============================================================

            switch (intent.Intent)
            {
                case SparkAIQuestionIntent.WhyNotWorking:
                    return BuildWhyNotWorkingAnswer(
                        observation,
                        context);

                case SparkAIQuestionIntent.FindProblem:
                    return BuildProblemAnswer(
                        observation,
                        context);

                case SparkAIQuestionIntent.NextStep:
                    return BuildNextStepAnswer(
                        observation,
                        context);

                case SparkAIQuestionIntent.Voltage:
                    return BuildVoltageAnswer(observation);

                case SparkAIQuestionIntent.Current:
                    return BuildCurrentAnswer(observation);

                case SparkAIQuestionIntent.Power:
                    return BuildPowerAnswer(observation);

                case SparkAIQuestionIntent.Source:
                    return BuildSourceAnswer(observation);

                case SparkAIQuestionIntent.Polarity:
                    return BuildPolarityAnswer(observation);

                case SparkAIQuestionIntent.CircuitPath:
                    return BuildPathAnswer(
                        observation,
                        context);

                case SparkAIQuestionIntent.Switch:
                    return BuildSwitchAnswer(
                        observation,
                        context);

                case SparkAIQuestionIntent.Calculation:
                    string answer;

                    if (TryBuildOhmsLawCalculationAnswer(
                            question,
                            out answer))
                    {
                        return answer;
                    }
                    return
                        "I can calculate current, voltage, or resistance using " +
                        "Ohm's law. Please provide the required electrical values.";

                case SparkAIQuestionIntent.Explanation:
                    return BuildGeneralAnswer(
                        observation,
                        context);

                case SparkAIQuestionIntent.Unknown:
                default:

                    // -----------------------------------------------
                    // Fallback: object problem
                    // -----------------------------------------------

                    if (hasObjectTarget &&
                        asksObjectProblem)
                    {
                        return BuildObjectAnswer(
                            question,
                            observation);
                    }

                    if (IsAskingWhyNotWorking(normalizedQuestion))
                    {
                        return BuildWhyNotWorkingAnswer(
                            observation,
                            context);
                    }

                    // -----------------------------------------------
                    // Fallback: general problem
                    // -----------------------------------------------

                    if (ContainsAny(
                            normalizedQuestion,
                            "where is the problem",
                            "what is wrong",
                            "whats wrong",
                            "where is wrong",
                            "find the problem",
                            "find problem",
                            "what should i fix"))
                    {
                        return BuildProblemAnswer(
                            observation,
                            context);
                    }

                    

                    // -----------------------------------------------
                    // Fallback: voltage
                    // -----------------------------------------------

                    if (ContainsAny(
                            normalizedQuestion,
                            "voltage",
                            "volt",
                            "how many volts"))
                    {
                        return BuildVoltageAnswer(observation);
                    }

                    // -----------------------------------------------
                    // Fallback: current
                    // -----------------------------------------------

                    if (ContainsAny(
                            normalizedQuestion,
                            "current",
                            "amps",
                            "ampere",
                            "amperes"))
                    {
                        return BuildCurrentAnswer(observation);
                    }

                    // -----------------------------------------------
                    // Fallback: power
                    // -----------------------------------------------

                    if (ContainsAny(
                            normalizedQuestion,
                            "power",
                            "watts",
                            "watt"))
                    {
                        return BuildPowerAnswer(observation);
                    }

                    // -----------------------------------------------
                    // Fallback: source
                    // -----------------------------------------------

                    if (ContainsAny(
                            normalizedQuestion,
                            "source",
                            "battery",
                            "power supply"))
                    {
                        return BuildSourceAnswer(observation);
                    }

                    // -----------------------------------------------
                    // Fallback: polarity
                    // -----------------------------------------------

                    if (ContainsAny(
                            normalizedQuestion,
                            "polarity",
                            "positive",
                            "negative",
                            "anode",
                            "cathode"))
                    {
                        return BuildPolarityAnswer(observation);
                    }

                    // -----------------------------------------------
                    // Fallback: circuit path
                    // -----------------------------------------------

                    if (ContainsAny(
                            normalizedQuestion,
                            "complete",
                            "closed circuit",
                            "open circuit",
                            "circuit path",
                            "path"))
                    {
                        return BuildPathAnswer(
                            observation,
                            context);
                    }

                    // -----------------------------------------------
                    // Fallback: switch
                    // -----------------------------------------------

                    if (ContainsAny(
                            normalizedQuestion,
                            "switch",
                            "switched",
                            "turn on",
                            "turn off"))
                    {
                        return BuildSwitchAnswer(
                            observation,
                            context);
                    }

                    return BuildGeneralAnswer(
                        observation,
                        context);
            }
        }


            private static bool IsExplicitLiveValueQuestion(string question)
            {
                return ContainsAny(
                    question,
                    "right now",
                    "currently",
                    "at the moment",
                    "in the current circuit",
                    "in my circuit",
                    "in this circuit",
                    "what does the meter read",
                    "what is the meter reading",
                    "what is the live voltage",
                    "what is the live current",
                    "read the voltage",
                    "read the current");
            }




private string BuildKnowledgeReasonedAnswer(
    SparkAIKnowledgeReasoningResult result)
{
    if (!result.IsValid)
        return string.Empty;

    string answer;

    switch (result.QuestionType)
    {
        case SparkAIKnowledgeQuestionType.Definition:
            answer = result.Definition;
            break;

        case SparkAIKnowledgeQuestionType.Explanation:
            answer = result.Explanation;
            break;

        case SparkAIKnowledgeQuestionType.Example:
            answer = result.Example;
            break;

        case SparkAIKnowledgeQuestionType.Misconception:
            answer = result.Misconception;
            break;

        case SparkAIKnowledgeQuestionType.Safety:
            answer = result.Safety;
            break;

        case SparkAIKnowledgeQuestionType.Measurement:
            answer = result.Measurement;
            break;

        case SparkAIKnowledgeQuestionType.Calculation:
            answer = result.Summary;
            break;

        default:
            answer = result.Definition;
            break;


    }

    // Fall back only if the selected answer field is empty.
    if (string.IsNullOrWhiteSpace(answer))
        answer = result.Summary;

    if (string.IsNullOrWhiteSpace(answer))
        return string.Empty;

        if (result.QuestionType == SparkAIKnowledgeQuestionType.Measurement ||
    result.QuestionType == SparkAIKnowledgeQuestionType.Safety)
{
    return answer;
}

    System.Text.StringBuilder builder =
        new System.Text.StringBuilder(answer);

    if (result.Observations != null)
    {
        foreach (string observation in result.Observations)
        {
            if (!string.IsNullOrWhiteSpace(observation))
            {
                builder.AppendLine();
                builder.AppendLine();
                builder.Append(observation);
            }
        }
    }

    if (result.TeachingPoints != null)
    {
        foreach (string teachingPoint in result.TeachingPoints)
        {
            if (string.IsNullOrWhiteSpace(teachingPoint))
                continue;

            if (answer.IndexOf(
                    teachingPoint,
                    System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                continue;
            }

            builder.AppendLine();
            builder.AppendLine();
            builder.Append(teachingPoint);
        }
    }

    Debug.Log(
    "[SPARK AI MEASUREMENT DEBUG] FinalType=" + result.QuestionType +
    " | FinalMeasurement=" +
    (string.IsNullOrWhiteSpace(result.Measurement)
        ? "<EMPTY>"
        : result.Measurement) +
    " | SelectedAnswer=" + answer);

    return builder.ToString();
}


        private string BuildNextStepAnswer(
    SparkAIUnifiedObservation observation,
    SparkAIContextReasoning context)
{
    SparkAILevelSnapshot level =
        observation.World.Level;

    if (!level.HasValidPowerSource)
    {
        return
            "First, identify and connect the electrical source.";
    }

    if (level.SourceShorted)
    {
        return
            "First, remove the short across the source.";
    }

    if (!level.ClosedReturn)
    {
        return
            "Next, complete the electrical path back to the opposite source terminal.";
    }

    if (level.WrongConnection)
    {
        return
            "Next, check the terminal connections and polarity.";
    }

    if (level.TargetShorted)
    {
        return
            "Next, remove the unintended short around the target.";
    }

    if (level.Overloaded)
    {
        return
            "Next, check the load and source operating conditions.";
    }

    if (level.TargetCurrent <= 0.000001f &&
        Mathf.Abs(level.TargetVoltage) <= 0.001f)
    {
        return
            "The basic path looks complete, but the target is not receiving useful electrical energy. " +
            "Let's inspect its electrical state.";
    }

    return
        "The circuit is operating. The next step is to examine the voltage, current, and power at the target.";
}

        // =========================================================
        // WHY IS IT NOT WORKING?
        // =========================================================

        private string BuildWhyNotWorkingAnswer(
            SparkAIUnifiedObservation observation,
            SparkAIContextReasoning context)
        {
            SparkAILevelSnapshot level =
                observation.World.Level;

            if (level.SourceShorted)
            {
                return
                    "The circuit has a source short. " +
                    "The source terminals are connected by a very low-resistance path. " +
                    "Remove the short before expecting the load to operate.";
            }

            if (level.WrongConnection)
            {
                return
                    "The circuit contains an incorrect connection. " +
                    "Check the terminal polarity and make sure each connection goes " +
                    "to the intended terminal.";
            }

            if (!level.HasValidPowerSource)
            {
                return
                    "The circuit does not have a valid power source yet. " +
                    "Connect the electrical source so it can provide a potential difference.";
            }

            if (!level.ClosedReturn)
            {
                return
                    "The circuit is open. " +
                    "The electrical path does not return to the other source terminal. " +
                    "Trace the connections from the source through the component and back.";
            }

            if (level.TargetShorted)
            {
                return
                    "The target is shorted. " +
                    "The current is taking an unintended low-resistance path around the target. " +
                    "Check the connections around the component.";
            }

            if (level.Overloaded)
            {
                return
                    "The circuit is overloaded. " +
                    "The electrical load is drawing more than the allowed operating condition. " +
                    "Check the source and the connected components.";
            }

            if (level.TargetCurrent > 0.000001f)
            {
                return
                    "Current is flowing through the target. " +
                    "The electrical circuit appears to be operating.";
            }

            if (Mathf.Abs(level.TargetVoltage) > 0.001f)
            {
                return
                    "The target has voltage, but there is not significant current flowing through it. " +
                    "Check whether the target is conducting and whether the circuit path is complete.";
            }

            if (context.IsValid &&
                !string.IsNullOrWhiteSpace(
                    context.Explanation))
            {
                return context.Explanation;
            }

            return
                "The target is not receiving useful electrical energy yet. " +
                "Check the source, connections, polarity, and return path.";
        }

        // =========================================================
        // PROBLEM
        // =========================================================

        private string BuildProblemAnswer(
            SparkAIUnifiedObservation observation,
            SparkAIContextReasoning context)
        {
            SparkAILevelSnapshot level =
                observation.World.Level;

            if (!level.HasValidPowerSource)
            {
                return
                    "The first problem is the power source. " +
                    "Make sure a valid electrical source is connected.";
            }

            if (level.SourceShorted)
            {
                return
                    "The problem is a short across the source. " +
                    "Remove the unintended low-resistance connection.";
            }

            if (level.WrongConnection)
            {
                return
                    "The problem is an incorrect connection. " +
                    "Check which terminals should be connected.";
            }

            if (!level.ClosedReturn)
            {
                return
                    "The problem is the circuit path. " +
                    "There is no complete return path to the opposite source terminal.";
            }

            if (level.TargetShorted)
            {
                return
                    "The target is shorted. " +
                    "Check for an unintended connection around the target.";
            }

            if (level.Overloaded)
            {
                return
                    "The circuit is overloaded. " +
                    "Check the load and source operating conditions.";
            }

            return
                "I do not see a major level fault. " +
                "The circuit appears to have a usable path, so we should inspect the target's electrical state.";
        }

        // =========================================================
        // VOLTAGE
        // =========================================================

        private string BuildVoltageAnswer(
            SparkAIUnifiedObservation observation)
        {
            SparkAILevelSnapshot level =
                observation.World.Level;

            if (!level.HasLevel)
            {
                return
                    "There is no active level with a readable target voltage.";
            }

            return
                "The current target voltage is " +
                FormatValue(level.TargetVoltage, "V") +
                ".";
        }

        // =========================================================
        // CURRENT
        // =========================================================

        private string BuildCurrentAnswer(
            SparkAIUnifiedObservation observation)
        {
            SparkAILevelSnapshot level =
                observation.World.Level;

            if (!level.HasLevel)
            {
                return
                    "There is no active level with a readable target current.";
            }

            if (Mathf.Abs(level.TargetCurrent) <= 0.000001f)
            {
                return
                    "The current target current is approximately 0 amps. " +
                    "That usually means the target is not currently conducting.";
            }

            return
                "The current target current is " +
                FormatValue(level.TargetCurrent, "A") +
                ".";
        }

        // =========================================================
        // POWER
        // =========================================================

        private string BuildPowerAnswer(
            SparkAIUnifiedObservation observation)
        {
            SparkAILevelSnapshot level =
                observation.World.Level;

            if (!level.HasLevel)
            {
                return
                    "There is no active level with a readable target power.";
            }

            return
                "The current target power is " +
                FormatValue(level.TargetPower, "W") +
                ".";
        }

        // =========================================================
        // SOURCE
        // =========================================================

        private string BuildSourceAnswer(
            SparkAIUnifiedObservation observation)
        {
            SparkAILevelSnapshot level =
                observation.World.Level;

            if (!level.HasValidPowerSource)
            {
                return
                    "The circuit does not currently have a valid power source.";
            }

            if (!string.IsNullOrWhiteSpace(
                    level.ActiveSourceName))
            {
                return
                    "The active electrical source is " +
                    level.ActiveSourceName +
                    ". It provides the potential difference needed to drive the circuit.";
            }

            return
                "A valid electrical source is present in the circuit.";
        }

        // =========================================================
        // POLARITY
        // =========================================================

        private string BuildPolarityAnswer(
            SparkAIUnifiedObservation observation)
        {
            SparkAILevelSnapshot level =
                observation.World.Level;

            if (level.WrongConnection)
            {
                return
                    "The circuit has a polarity or connection problem. " +
                    "Check that the source positive side reaches the intended positive or anode side, " +
                    "and the source negative side returns to the intended negative or cathode side.";
            }

            return
                "Polarity identifies the positive and negative sides of the electrical circuit. " +
                "For a polarized component, connect the intended positive source side to its positive or anode terminal, " +
                "and the return to its negative or cathode terminal.";
        }

        // =========================================================
        // PATH
        // =========================================================

        private string BuildPathAnswer(
            SparkAIUnifiedObservation observation,
            SparkAIContextReasoning context)
        {
            SparkAILevelSnapshot level =
                observation.World.Level;

            if (!level.HasValidPowerSource)
            {
                return
                    "There is no valid source yet, so we cannot establish a complete powered path.";
            }

            if (!level.ClosedReturn)
            {
                return
                    "The circuit path is open. " +
                    "Start at one source terminal, follow the connections through the circuit, " +
                    "and make sure the path returns to the opposite source terminal.";
            }

            return
                "The circuit has a closed return path. " +
                "The electrical route from the source through the circuit and back to the source is complete.";
        }

        // =========================================================
        // SWITCH
        // =========================================================

        private string BuildSwitchAnswer(
            SparkAIUnifiedObservation observation,
            SparkAIContextReasoning context)
        {
            if (!string.IsNullOrWhiteSpace(
                    context.Explanation))
            {
                return context.Explanation;
            }

            return
                "A closed switch provides a conducting path. " +
                "An open switch breaks that path and normally prevents current from flowing through it.";
        }

        // =========================================================
        // GENERAL
        // =========================================================

        private string BuildGeneralAnswer(
            SparkAIUnifiedObservation observation,
            SparkAIContextReasoning context)
        {
            SparkAILevelSnapshot level =
                observation.World.Level;

            if (level.HasLevel)
            {
                if (!level.HasValidPowerSource)
                {
                    return
                        "Right now the circuit has no valid power source. " +
                        "Start by identifying and connecting the source.";
                }

                if (!level.ClosedReturn)
                {
                    return
                        "Right now the circuit is open. " +
                        "The electrical path needs to return to the opposite source terminal.";
                }

                if (level.WrongConnection)
                {
                    return
                        "There is an incorrect connection in the circuit. " +
                        "Check the terminal labels and polarity.";
                }

                if (level.SourceShorted)
                {
                    return
                        "The source is shorted. " +
                        "There is an unintended low-resistance path between its terminals.";
                }

                if (level.Overloaded)
                {
                    return
                        "The circuit is overloaded. " +
                        "Check the source and load conditions.";
                }
            }

            if (context.IsValid &&
                !string.IsNullOrWhiteSpace(
                    context.Explanation))
            {
                return context.Explanation;
            }

            return
                "I can see the current Project Spark state, " +
                "but I need a more specific question to explain it.";
        }

        // =========================================================
        // COMPLETED LEVEL
        // =========================================================

        private string BuildCompletedLevelAnswer(
            string question,
            SparkAIUnifiedObservation observation)
        {
            SparkAILevelSnapshot level =
                observation.World.Level;

            if (ContainsAny(
                    question,
                    "what happened",
                    "why did",
                    "why is",
                    "how did",
                    "how did it work",
                    "why did it work",
                    "why is it working"))
            {
                return
                    "The level is complete because the required electrical conditions were satisfied. " +
                    "The source, circuit path, and target connection reached the level's success condition.";
            }

            if (ContainsAny(
                    question,
                    "voltage",
                    "volt"))
            {
                return
                    "The target voltage at completion was " +
                    FormatValue(level.TargetVoltage, "V") +
                    ".";
            }

            if (ContainsAny(
                    question,
                    "current",
                    "amps",
                    "ampere"))
            {
                return
                    "The target current at completion was " +
                    FormatValue(level.TargetCurrent, "A") +
                    ".";
            }

            return
                "The level is already complete. " +
                "The required circuit conditions were successfully satisfied.";
        }

        // =========================================================
        // QUESTION DETECTION
        // =========================================================

        private static bool IsAskingWhyNotWorking(
            string question)
        {
            return ContainsAny(
                question,
                "why isn't",
                "why isnt",
                "why is not",
                "why doesn't",
                "why doesnt",
                "not working",
                "isn't working",
                "isnt working",
                "won't work",
                "wont work",
                "why no current",
                "why no voltage",
                "why is the led off",
                "why is led off",
                "why is the bulb off",
                "why is bulb off",
                "why isn't my led",
                "why isnt my led",
                "why isn't the led",
                "why isnt the led");
        }

        
        private static bool ContainsAny(
            string text,
            params string[] values)
        {
            if (string.IsNullOrWhiteSpace(text) || values == null)
                return false;

            for (int i = 0; i < values.Length; i++)
            {
                if (ContainsPhrase(text, values[i]))
                    return true;
            }

            return false;
        }

        private static bool ContainsPhrase(
            string text,
            string phrase)
        {
            if (string.IsNullOrEmpty(text) ||
                string.IsNullOrEmpty(phrase))
                return false;

            int start = 0;

            while (start < text.Length)
            {
                int index = text.IndexOf(
                    phrase,
                    start,
                    StringComparison.OrdinalIgnoreCase);

                if (index < 0)
                    return false;

                int end = index + phrase.Length;

                bool leftBoundary =
                    index == 0 ||
                    !IsWordCharacter(text[index - 1]);

                bool rightBoundary =
                    end >= text.Length ||
                    !IsWordCharacter(text[end]);

                if (leftBoundary && rightBoundary)
                    return true;

                start = index + 1;
            }

            return false;
        }

        private static bool IsWordCharacter(char value)
        {
            return char.IsLetterOrDigit(value) || value == '_';
        }

        private static string Normalize(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            return text.Trim()
                .ToLowerInvariant()
                .Replace('\u2018', '\'')
                .Replace('\u2019', '\'')
                .Replace('\u201C', '"')
                .Replace('\u201D', '"')
                .Replace('\u2013', '-')
                .Replace('\u2014', '-');
        }

      
        
        private static string FormatValue(float value, string unit)
        {
            if (float.IsNaN(value) || float.IsInfinity(value))
                return "unavailable";

            return value.ToString(
                "0.###",
                CultureInfo.InvariantCulture) +
                " " +
                unit;
        }

        // =========================================================
        // VOICE
        // =========================================================

        private void SpeakAnswer(string answer)
        {
            if (!speakAnswers)
                return;

            if (voiceController == null)
                return;

            if (string.IsNullOrWhiteSpace(answer))
                return;

            voiceController.SpeakText(
                answer,
                SparkAIVoiceTextType.Explanation);
        } 



        

private string BuildLedStateAnswer(
    SparkAIElectronicObjectSnapshot target)
{
    string objectName =
        string.IsNullOrWhiteSpace(target.Name)
            ? "The LED"
            : target.Name;

    if (!target.Active)
    {
        return objectName +
               " is inactive in the current scene.";
    }

    if (!target.ElectricalEnabled)
    {
        return objectName +
               " has its electrical behavior disabled.";
    }

    if (float.IsNaN(target.Current) ||
        float.IsInfinity(target.Current))
    {
        return objectName +
               " has an invalid current reading. " +
               "I cannot reliably determine its operating state.";
    }

    if (string.Equals(
            target.ConductionState,
            "Conducting",
            StringComparison.OrdinalIgnoreCase))
    {
        if (Math.Abs(target.Current) > 0.000001f)
        {
            return objectName +
                   " is electrically conducting, with a reported current of " +
                   FormatValue(target.Current, "A") +
                   ". This supports electrical operation, but the electrical " +
                   "snapshot alone does not confirm that the rendered LED " +
                   "is visibly illuminated.";
        }

        return objectName +
               " reports a conducting state, but its reported current is " +
               FormatValue(target.Current, "A") +
               ". The conduction state and current reading disagree, so I " +
               "cannot reliably confirm illumination.";
    }

    if (string.Equals(
            target.ConductionState,
            "NonConducting",
            StringComparison.OrdinalIgnoreCase))
    {
        return objectName +
               " is not currently conducting according to the electrical " +
               "snapshot. Its reported current is " +
               FormatValue(target.Current, "A") +
               ".";
    }

    return objectName +
           " cannot be confirmed ON or OFF from the available electrical " +
           "evidence. Its conduction state is " +
           (string.IsNullOrWhiteSpace(target.ConductionState)
               ? "unknown"
               : target.ConductionState) +
           ", and its reported current is " +
           FormatValue(target.Current, "A") +
           ".";
}


        // ============================================================
// LED POLARITY DIAGNOSIS
// ============================================================

private string BuildObjectPolarityAnswer(
    string question,
    SparkAIUnifiedObservation observation)
{
   if (observation.World == null)
{
    return "I cannot determine the LED polarity because the current circuit observation is unavailable.";
}

    SparkAIWorldSnapshot world =
        observation.World;

    string normalizedQuestion =
        Normalize(question);

    SparkAIElectronicObjectSnapshot ledSnapshot = default;
    bool foundLed = false;

    // --------------------------------------------------------
    // Find the LED snapshot
    // --------------------------------------------------------

    for (int i = 0;
         i < world.ElectronicObjects.Count;
         i++)
    {
        SparkAIElectronicObjectSnapshot candidate =
            world.ElectronicObjects[i];

        string objectName =
            Normalize(candidate.Name);

        if (objectName.Contains("led") ||
            objectName.Contains("light emitting diode"))
        {
            ledSnapshot = candidate;
            foundLed = true;
            break;
        }
    }

    if (!foundLed)
    {
        return "I cannot determine the LED polarity because no LED was found in the current observation.";
    }

    if (!ledSnapshot.Active)
    {
        return "The LED is inactive. Activate it before evaluating its electrical polarity.";
    }

    if (!ledSnapshot.IsElectricalComponent ||
        !ledSnapshot.ElectricalEnabled)
    {
        return "I found the LED, but its electrical component is disabled or unavailable. I cannot reliably evaluate its polarity.";
    }

    int anodeId =
        ledSnapshot.AnodeTerminalInstanceId;

    int cathodeId =
        ledSnapshot.CathodeTerminalInstanceId;

    if (anodeId == 0 || cathodeId == 0)
    {
        return "I cannot determine the LED polarity because its anode or cathode terminal is not assigned.";
    }

    SparkAITerminalSnapshot anode = default;
    SparkAITerminalSnapshot cathode = default;

    bool foundAnode = false;
    bool foundCathode = false;

    // --------------------------------------------------------
    // Find actual anode and cathode terminal snapshots
    // --------------------------------------------------------

    for (int i = 0;
         i < world.Terminals.Count;
         i++)
    {
        SparkAITerminalSnapshot terminal =
            world.Terminals[i];

            if (logQuestions)
{
    Debug.Log(
        "[SPARK AI LED TERMINAL SNAPSHOT] " +
        "ID=" + terminal.InstanceId +
        " | Name=" + terminal.Name +
        " | Owner=" + terminal.OwnerName +
        " | Voltage=" + terminal.Voltage);
}

        if (terminal.InstanceId == anodeId)
        {
            anode = terminal;
            foundAnode = true;
        }
        else if (terminal.InstanceId == cathodeId)
        {
            cathode = terminal;
            foundCathode = true;
        }

        if (foundAnode && foundCathode)
        {
            break;
        }
    }

    if (!foundAnode || !foundCathode)
    {
        return "I cannot determine the LED polarity because its anode and cathode terminal readings are not available in the current observation.";
    }

    // --------------------------------------------------------
    // Validate terminal readings
    // --------------------------------------------------------

    if (float.IsNaN(anode.Voltage) ||
        float.IsInfinity(anode.Voltage) ||
        float.IsNaN(cathode.Voltage) ||
        float.IsInfinity(cathode.Voltage))
    {
        return "The LED terminal voltages are invalid, so I cannot determine its polarity.";
    }

    float ledVoltage =
        anode.Voltage - cathode.Voltage;

    const float voltageTolerance = 0.05f;

    // --------------------------------------------------------
    // Report polarity based on measured voltage
    // --------------------------------------------------------

    if (ledVoltage > voltageTolerance)
    {
        return
            "The LED has forward polarity: its anode is " +
            FormatValue(anode.Voltage, "V") +
            " and its cathode is " +
            FormatValue(cathode.Voltage, "V") +
            ". The anode is at a higher potential than the cathode. " +
            "This is the correct voltage polarity for forward operation, " +
            "but it does not by itself prove that enough current is flowing.";
    }

    if (ledVoltage < -voltageTolerance)
    {
        return
            "The LED has reverse polarity: its anode is " +
            FormatValue(anode.Voltage, "V") +
            " and its cathode is " +
            FormatValue(cathode.Voltage, "V") +
            ". The cathode is at a higher potential than the anode. " +
            "Check the LED connections and the supply polarity. " +
            "Avoid exceeding the LED's reverse-voltage rating.";
    }

    return
        "The voltage across the LED is approximately zero: anode " +
        FormatValue(anode.Voltage, "V") +
        ", cathode " +
        FormatValue(cathode.Voltage, "V") +
        ". The current readings do not establish a forward or reverse " +
        "voltage bias, so I cannot confirm that the LED is operating " +
        "with sufficient forward bias.";
}         


    }
}