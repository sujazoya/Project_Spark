using System;
using ProjectSpark.AI.Tutorial;
using ProjectSpark.Gameplay;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProjectSpark.SparkV5.Pro
{
    /// <summary>
    /// Read-only, step-by-step laboratory demonstrations. Uses the proven v6.2 visual
    /// inspector and authoritative solver/evaluator. Never creates workbench wires,
    /// moves components, operates live instruments, awards scores or issues certificates.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkLabDemonstrationDirector : MonoBehaviour
    {
        [SerializeField] private SparkProExecutive executive;
        [SerializeField] private SparkLabTeachingDirector inspector;
        [SerializeField] private bool allowDemonstrations = true;

        private SparkLabDemoIntent.Kind active = SparkLabDemoIntent.Kind.None;
        private int nextStep;
        private Transform originalLayout;
        private string lastStep = "No demonstration has started.";

        public bool HasDemonstration => active != SparkLabDemoIntent.Kind.None;
        public string Status => HasDemonstration
            ? $"{Name(active)}: step {nextStep}/{StepCount(active)} next. {lastStep}"
            : "No active laboratory demonstration.";

        private void Awake()
        {
            if (!executive) executive = GetComponent<SparkProExecutive>();
            if (!inspector) inspector = GetComponent<SparkLabTeachingDirector>();
        }

        private void OnEnable() { SceneManager.activeSceneChanged += OnSceneChanged; }
        private void OnDisable()
        {
            SceneManager.activeSceneChanged -= OnSceneChanged;
            ResetSession(true);
        }

        private void OnSceneChanged(Scene previous, Scene next) { ResetSession(true); }

        public static bool TryHandleFrom(SparkProExecutive source, string question, out string reply)
        {
            reply = null;
            SparkLabDemoIntent.Kind intent = SparkLabDemoIntent.Classify(question);
            if (intent == SparkLabDemoIntent.Kind.None) return false;
            if (!source) { reply = "Spark executive is unavailable."; return true; }
            SparkLabDemonstrationDirector director = source.GetComponent<SparkLabDemonstrationDirector>();
            reply = director ? director.Handle(intent) :
                "Laboratory demonstrator is not attached. Run AAA Laboratory > 6 - AUTO Bind Demonstrator.";
            return true;
        }

        public string Handle(SparkLabDemoIntent.Kind intent)
        {
            if (!executive) executive = GetComponent<SparkProExecutive>();
            if (!inspector) inspector = GetComponent<SparkLabTeachingDirector>();
            if (intent == SparkLabDemoIntent.Kind.Stop)
            {
                ResetSession(true);
                return "Demonstration stopped and teaching markers cleared. Your circuit and mission were not changed.";
            }
            if (!allowDemonstrations) return "Laboratory demonstrations are disabled in this scene.";
            if (!executive) return "The active Spark executive is not assigned.";
            if (executive.ExamMode) return "Demonstrations are unavailable during independent examinations.";
            SparkProCharacter character = GetComponent<SparkProCharacter>() ??
                GetComponentInParent<SparkProCharacter>() ?? GetComponentInChildren<SparkProCharacter>();
            if (character && character.WarningActive)
                return "Demonstrations are paused while a safety warning is active.";

            LevelGamePlayManager manager = executive.ActiveLevelManager;
            SparkAILevelContext level = manager ? SparkAILevelContext.Capture(manager) : default;
            if (!manager || !level.Layout || !level.Layout.gameObject.activeInHierarchy)
                return "Load an active laboratory mission before starting a demonstration.";

            if (HasDemonstration && originalLayout != level.Layout.transform)
            {
                ResetSession(true);
                return "The active level changed. The old demonstration was cancelled; start a new one.";
            }
            if (intent == SparkLabDemoIntent.Kind.Status) return Status;
            if (intent == SparkLabDemoIntent.Kind.Repeat)
            {
                if (!HasDemonstration) return "Start a diode, switch or multimeter demonstration first.";
                int previous = Mathf.Max(0, nextStep - 1);
                return PresentStep(level, previous, false);
            }
            if (intent == SparkLabDemoIntent.Kind.Next)
            {
                if (!HasDemonstration) return "Start a diode, switch or multimeter demonstration first.";
                return PresentStep(level, nextStep, true);
            }
            if (intent == SparkLabDemoIntent.Kind.StartDiode ||
                intent == SparkLabDemoIntent.Kind.StartSwitch ||
                intent == SparkLabDemoIntent.Kind.StartMeter)
            {
                ResetSession(true);
                active = intent;
                originalLayout = level.Layout.transform;
                nextStep = 0;
                lastStep = "Select the relevant component using the in-game Select Tool.";
                return $"Starting {Name(intent)} — a read-only guided demonstration. " +
                    "This will not wire components, switch live power, move probes or change mission progress. " +
                    "Select the relevant workbench component, then say 'next demonstration step'.";
            }
            return "Unknown demonstration command.";
        }

        private string PresentStep(SparkAILevelContext level, int index, bool advance)
        {
            int total = StepCount(active);
            if (index >= total)
            {
                string name = Name(active);
                ResetSession(false);
                return $"{name} is finished. These were coaching steps, not a practical assessment. " +
                    "Use the official mission evaluator to check completion.";
            }

            string message;
            bool succeeded = true;
            switch (active)
            {
                case SparkLabDemoIntent.Kind.StartDiode:
                    succeeded = DiodeStep(level, index, out message);
                    break;
                case SparkLabDemoIntent.Kind.StartSwitch:
                    succeeded = SwitchStep(level, index, out message);
                    break;
                case SparkLabDemoIntent.Kind.StartMeter:
                    succeeded = MeterStep(level, index, out message);
                    break;
                default:
                    return "No active demonstration.";
            }
            if (advance && succeeded) nextStep = index + 1;
            lastStep = message;
            return $"{Name(active)} — step {index + 1}/{total}. {message}" +
                (succeeded ? "\nSay 'next demonstration step' to proceed, or 'repeat demonstration step'." :
                    "\nThis step remains pending. Correct the selection/evidence and retry the step.");
        }

        private bool Mark(SparkLabFocusIntent.Kind kind, out string detail)
        {
            if (!inspector) inspector = GetComponent<SparkLabTeachingDirector>();
            if (!inspector)
            {
                detail = "3D inspection director is missing. Run AAA Laboratory > 1 - AUTO Bind Prefab and Open Scenes.";
                return false;
            }
            bool success = inspector.TryShowCurrent(kind);
            detail = inspector.LastInspection;
            return success;
        }

        private SparkElectronicObject SelectedPart()
        {
            if (!executive || !executive.ActiveSelectionController) return null;
            var selected = executive.ActiveSelectionController.SelectedObject;
            return selected ? selected.GetComponentInParent<SparkElectronicObject>() : null;
        }

        private bool DiodeStep(SparkAILevelContext level, int step, out string message)
        {
            message = "";
            if (step == 0)
            {
                SparkElectronicObject selected = SelectedPart();
                if (!selected || !(selected is SparkDiode) && !(selected is SparkLED))
                {
                    message = "Select an actual in-level diode or LED with the Select Tool. A shelf item or resistor does not qualify.";
                    return false;
                }
                bool ok = Mark(SparkLabFocusIntent.Kind.Selected, out string marker);
                message = marker + " The device normally conducts in its forward direction only when the circuit supports suitable current.";
                return ok;
            }
            if (step == 1)
            {
                bool ok = Mark(SparkLabFocusIntent.Kind.PositiveTerminal, out string marker);
                message = marker + " This is the anode. Compare its authored placement with the schematic, not just cable color.";
                return ok;
            }
            if (step == 2)
            {
                bool ok = Mark(SparkLabFocusIntent.Kind.NegativeTerminal, out string marker);
                message = marker + " This is the cathode. Never short anode directly to cathode as a wiring shortcut.";
                return ok;
            }
            if (step == 3)
            {
                if (!SparkPhase4EvidenceGuard.EvaluatorIsCurrent(level))
                {
                    message = "Circuit/evaluator evidence is not current. Let the simulation settle; no electrical conclusion was made.";
                    return false;
                }
                if (!HasFreshSolve(level))
                {
                    message = "The electrical solver result is not fresh. No live polarity/fault conclusion was made.";
                    return false;
                }
                SparkElectronicObject selected = SelectedPart();
                if (selected is SparkDiode diode && diode.IsReverseFault)
                    message = "Verified by the solver: the selected diode exceeds its reverse-voltage limit. Isolate the training supply and inspect orientation.";
                else if (selected is SparkLED led && led.IsReverseVoltageExceeded)
                    message = "Verified by the solver: the selected LED exceeds its reverse-voltage limit. Isolate the training supply and inspect orientation.";
                else if (level.Manager.SourceShorted || level.Manager.TargetShorted)
                    message = "Verified by the evaluator: a short exists. This demo has not identified the exact wire at fault.";
                else if (level.Manager.Overloaded)
                    message = "Verified by the evaluator: an overload is active. Inspect the current limit and component ratings.";
                else if (level.Manager.WrongConnection)
                    message = "Verified by the evaluator: some connection is incorrect. Check the authored diagram and exact terminal labels.";
                else if (level.Definition.RequireClosedReturn && !level.Manager.ClosedReturn)
                    message = "Verified by the evaluator: the required return path is open. Trace the path with virtual power isolated.";
                else if (level.Manager.IsCompleted)
                    message = "The evaluator reports the current mission completed. No specific diode fault was established in this demonstration.";
                else
                    message = "Fresh electrical evidence is available, but it does not uniquely identify a diode fault. Check the next authored objective.";
                return true;
            }
            message = "Review: anode, cathode, current limiting, and a closed return. " +
                "Apply the authored objective using ordinary game tools; the evaluator alone decides whether the objective passes.";
            return true;
        }

        private bool SwitchStep(SparkAILevelContext level, int step, out string message)
        {
            message = "";
            if (step == 0)
            {
                message = "A continuous loop through a load and back to the supply is needed for steady current. " +
                    "Opening a switch interrupts the intended route. This step does not operate your live workbench switch.";
                return true;
            }
            if (step == 1)
            {
                try
                {
                    string observation = SparkProLabCircuit.Demonstrate("switch");
                    if (!observation.StartsWith("Isolated solver observation:", StringComparison.Ordinal))
                    {
                        message = observation + " No live-circuit result was inferred.";
                        return false;
                    }
                    message = observation + " These measurements were generated ONLY in an isolated training circuit.";
                    return true;
                }
                catch (Exception error)
                {
                    Debug.LogException(error, this);
                    message = "The isolated trainer failed. No active mission connection was changed.";
                    return false;
                }
            }
            if (step == 2)
            {
                if (!SparkPhase4EvidenceGuard.EvaluatorIsCurrent(level))
                {
                    message = "The live evaluator is stale. Update the circuit simulation before comparing it with the isolated experiment.";
                    return false;
                }
                if (!HasFreshSolve(level))
                {
                    message = "Live solver data is not fresh. Do not compare isolated current to the real mission yet.";
                    return false;
                }
                message = level.Manager.SourceShorted || level.Manager.TargetShorted
                    ? "The live evaluator flags a short. Isolate supply and inspect the route before continuing."
                    : level.Definition.RequireClosedReturn && !level.Manager.ClosedReturn
                    ? "The live evaluator reports that the return path is open."
                    : level.Manager.IsCompleted
                    ? "The live mission evaluator reports completion of its configured objectives."
                    : "The live mission remains incomplete, but no unique return-path fault was verified.";
                message += " These are live-evaluator facts, NOT measurements from the isolated trainer.";
                return true;
            }
            message = "Review: open switch, closed switch, and broken return are three distinct circuit conditions. " +
                "Use the authorized workbench tools to test your real mission independently.";
            return true;
        }

        private bool MeterStep(SparkAILevelContext level, int step, out string message)
        {
            message = "";
            if (step == 0)
            {
                bool ok = Mark(SparkLabFocusIntent.Kind.Selected, out string marker);
                message = marker + " Identify the two reference terminals before interpreting a voltage difference.";
                return ok;
            }
            if (step == 1)
            {
                message = "For a DC voltage measurement, select the correct voltage range and place the probes ACROSS " +
                    "the two intended terminals. Current uses a SERIES connection. Continuity and resistance require the training supply isolated. " +
                    "This instruction does not move or connect virtual probes.";
                return true;
            }
            if (step == 2)
            {
                if (!SparkPhase4EvidenceGuard.EvaluatorIsCurrent(level))
                {
                    message = "The electrical evaluator is stale. No numerical measurement can be claimed yet.";
                    return false;
                }
                SparkElectronicObject part = SelectedPart();
                if (!part)
                {
                    message = "Select an in-scene two-terminal component to inspect its solver voltage.";
                    return false;
                }
                string reading = SparkProDiagnostics.Measure(level, part);
                if (!reading.StartsWith("Current solver result", StringComparison.Ordinal))
                {
                    message = reading;
                    return false;
                }
                message = reading + " This is a solver result, not an instrument reading unless the multimeter probes and mode were verified.";
                return true;
            }
            message = "Review: state the reference terminal, sign, units, probe arrangement and solver revision. " +
                "The authorized multimeter tool must verify any requested practical measurement.";
            return true;
        }

        private static bool HasFreshSolve(SparkAILevelContext level)
        {
            var manager = level.Manager;
            if (!manager) return false;
            var solver = manager.ElectricalSolver;
            return solver && !solver.IsDirty && !solver.IsSolving && solver.LatestSolveResult != null;
        }

        private static int StepCount(SparkLabDemoIntent.Kind kind)
        {
            switch (kind)
            {
                case SparkLabDemoIntent.Kind.StartDiode: return 5;
                case SparkLabDemoIntent.Kind.StartSwitch: return 4;
                case SparkLabDemoIntent.Kind.StartMeter: return 4;
                default: return 0;
            }
        }
        private static string Name(SparkLabDemoIntent.Kind kind)
        {
            switch (kind)
            {
                case SparkLabDemoIntent.Kind.StartDiode: return "Diode polarity demonstration";
                case SparkLabDemoIntent.Kind.StartSwitch: return "Isolated switch demonstration";
                case SparkLabDemoIntent.Kind.StartMeter: return "Multimeter demonstration";
                default: return "Laboratory demonstration";
            }
        }

        private void ResetSession(bool clearMarker)
        {
            active = SparkLabDemoIntent.Kind.None;
            nextStep = 0;
            originalLayout = null;
            lastStep = "No demonstration has started.";
            if (clearMarker && inspector) inspector.CancelInspection();
        }
    }
}
