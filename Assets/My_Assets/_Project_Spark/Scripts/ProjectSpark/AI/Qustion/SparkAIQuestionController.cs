using System;
using UnityEngine;

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
                    "[Spark AI Question] " +
                    "Context Reasoner is not assigned.",
                    this);

                return;
            }

            if (observationCoordinator == null)
            {
                Debug.LogError(
                    "[Spark AI Question] " +
                    "Observation Coordinator is not assigned.",
                    this);

                return;
            }

            if (voiceController == null)
            {
                Debug.LogError(
                    "[Spark AI Question] " +
                    "Voice Controller is not assigned.",
                    this);

                return;
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
            if (!initialized)
            {
                Debug.LogWarning(
                    "[Spark AI Question] " +
                    "Controller is not initialized.",
                    this);

                return;
            }

            if (!allowQuestions)
                return;

            SparkAIPlayerQuestion question =
                SparkAIPlayerQuestion.Create(text);

            if (!question.IsValid)
            {
                Debug.LogWarning(
                    "[Spark AI Question] " +
                    "Player question was empty.",
                    this);

                return;
            }

            if (logQuestions)
            {
                Debug.Log(
                    "[SPARK AI PLAYER QUESTION] " +
                    question.Text,
                    this);
            }

            QuestionAsked?.Invoke(question);

            string answer =
                BuildAnswer(question.Text);

            if (string.IsNullOrWhiteSpace(answer))
            {
                answer =
                    "I could not determine the answer from the current circuit state.";
            }

            AnswerCreated?.Invoke(answer);

            if (logQuestions)
            {
                Debug.Log(
                    "[SPARK AI ANSWER] " +
                    answer,
                    this);
            }

            if (speakAnswers)
            {
                SpeakAnswer(answer);
            }
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
        return
            "I cannot read the Project Spark circuit right now.";
    }

    SparkAIContextReasoning context =
        contextReasoner.ReasonCurrentContext();

    string normalizedQuestion =
        Normalize(question);

    /*
     * ============================================================
     * LEVEL STATE HAS HIGHEST PRIORITY
     * ============================================================
     *
     * If the authoritative level is already complete,
     * do not continue explaining an old open-circuit state.
     */
    if (observation.World.Level.Completed)
    {
        return BuildCompletedLevelAnswer(
            normalizedQuestion,
            observation);
    }

    /*
     * ============================================================
     * QUESTION INTENT
     * ============================================================
     *
     * The intent resolver determines what the player is asking.
     * The actual answer still comes from the authoritative
     * Project Spark world state.
     */
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

            return BuildVoltageAnswer(
                observation);


        case SparkAIQuestionIntent.Current:

            return BuildCurrentAnswer(
                observation);


        case SparkAIQuestionIntent.Power:

            return BuildPowerAnswer(
                observation);


        case SparkAIQuestionIntent.Source:

            return BuildSourceAnswer(
                observation);


        case SparkAIQuestionIntent.Polarity:

            return BuildPolarityAnswer(
                observation);


        case SparkAIQuestionIntent.CircuitPath:

            return BuildPathAnswer(
                observation,
                context);


        case SparkAIQuestionIntent.Switch:

            return BuildSwitchAnswer(
                observation,
                context);


        case SparkAIQuestionIntent.Explanation:

            return BuildGeneralAnswer(
                observation,
                context);


        case SparkAIQuestionIntent.Unknown:
        default:

            /*
             * Fallback to the old keyword checks.
             *
             * This keeps the system robust if the intent resolver
             * cannot confidently classify an unusual question.
             */

            if (IsAskingWhyNotWorking(normalizedQuestion))
            {
                return BuildWhyNotWorkingAnswer(
                    observation,
                    context);
            }

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

            if (ContainsAny(
                    normalizedQuestion,
                    "voltage",
                    "volt",
                    "how many volts"))
            {
                return BuildVoltageAnswer(
                    observation);
            }

            if (ContainsAny(
                    normalizedQuestion,
                    "current",
                    "amps",
                    "ampere",
                    "amperes"))
            {
                return BuildCurrentAnswer(
                    observation);
            }

            if (ContainsAny(
                    normalizedQuestion,
                    "power",
                    "watts",
                    "watt"))
            {
                return BuildPowerAnswer(
                    observation);
            }

            if (ContainsAny(
                    normalizedQuestion,
                    "source",
                    "battery",
                    "power supply"))
            {
                return BuildSourceAnswer(
                    observation);
            }

            if (ContainsAny(
                    normalizedQuestion,
                    "polarity",
                    "positive",
                    "negative",
                    "anode",
                    "cathode"))
            {
                return BuildPolarityAnswer(
                    observation);
            }

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
            if (string.IsNullOrEmpty(text))
                return false;

            for (int i = 0; i < values.Length; i++)
            {
                if (text.Contains(values[i]))
                    return true;
            }

            return false;
        }

        private static string Normalize(
            string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            return text
                .Trim()
                .ToLowerInvariant();
        }

        private static string FormatValue(
            float value,
            string unit)
        {
            return
                value.ToString("0.###") +
                " " +
                unit;
        }

        // =========================================================
        // VOICE
        // =========================================================

        private void SpeakAnswer(string answer)
        {
            if (voiceController == null)
                return;

            /*
             * Uses the arbitrary-text method we added to
             * SparkAIVoiceController.
             */
            voiceController.SpeakText(
                answer,
                SparkAIVoiceTextType.Explanation);
        }
    }
}