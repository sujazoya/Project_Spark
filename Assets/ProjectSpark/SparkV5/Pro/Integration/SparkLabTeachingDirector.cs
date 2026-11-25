using System;
using System.Reflection;
using ProjectSpark.AI.Tutorial;
using ProjectSpark.Gameplay;
using ProjectSpark.UI.VisualTeaching;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProjectSpark.SparkV5.Pro
{
    /// <summary>
    /// Observes Spark Live answers and displays validated terminal locations in the current layout.
    /// It never modifies gameplay geometry, wiring, inventory, grades, mission state or electrical data.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkLabTeachingDirector : MonoBehaviour
    {
        [SerializeField] private SparkProExecutive executive;
        [SerializeField] private SparkProCharacter character;
        [SerializeField] private SparkSelectionController selection;
        [SerializeField] private bool enableVisualInspection = true;

        private SparkLabBeaconVisual visual;
        private int lastHandledRevision = -1;
        private bool skipNextAnswerReady;
        // A verified in-scene selection may be reused for the next spoken/text follow-up.
        // The cache is time-limited and checked against the current layout on EVERY use.
        private Transform lastVerifiedSelection;
        private float lastVerifiedAt;
        private const float SelectionRecallSeconds = 90f;
        private const BindingFlags TerminalMemberFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private static readonly string[] AnodeNames = { "AnodeTerminal", "anodeTerminal", "_anodeTerminal", "m_AnodeTerminal" };
        private static readonly string[] CathodeNames = { "CathodeTerminal", "cathodeTerminal", "_cathodeTerminal", "m_CathodeTerminal" };
        public bool ShowingInspection => visual && visual.IsShowing;
        public string LastInspection { get; private set; } = "No inspection yet.";
        public event Action<string> InspectionChanged;

        private void Awake()
        {
            if (!executive) executive = GetComponent<SparkProExecutive>();
            if (!character) character = GetComponent<SparkProCharacter>() ?? GetComponentInParent<SparkProCharacter>();
            if (executive && executive.ActiveSelectionController) selection = executive.ActiveSelectionController;
        }

        private void OnEnable()
        {
            if (!executive) executive = GetComponent<SparkProExecutive>();
            if (executive && executive.ActiveSelectionController) selection = executive.ActiveSelectionController;
            if (executive) executive.AnswerReady += OnAnswerReady;
            SceneManager.activeSceneChanged += OnActiveSceneChanged;
        }

        private void OnDisable()
        {
            if (executive) executive.AnswerReady -= OnAnswerReady;
            SceneManager.activeSceneChanged -= OnActiveSceneChanged;
            ClearCue();
            lastVerifiedSelection = null;
        }

        private void OnActiveSceneChanged(Scene previous, Scene next)
        {
            lastHandledRevision = -1;
            lastVerifiedSelection = null;
            ClearCue();
        }

        private SparkLabBeaconVisual GetVisual()
        {
            if (visual) return visual;
            var node = new GameObject("SparkLabInspectionCue_RuntimeOnly");
            node.layer = 2;
            node.transform.SetParent(transform, false);
            visual = node.AddComponent<SparkLabBeaconVisual>();
            visual.enabled = false;
            return visual;
        }

        private void ClearCue()
        {
            if (visual) visual.Clear();
        }

        private void OnAnswerReady(string answer)
        {
            if (skipNextAnswerReady) { skipNextAnswerReady = false; return; }
            if (!enabled || !enableVisualInspection || !executive) return;
            var history = executive.History;
            if (history == null || history.Count < 2) return;
            // Only a response immediately paired with a new user question may trigger a world cue.
            // A capped history buffer still works because Cortex revisions are monotonic.
            if (!string.Equals(history[history.Count - 2].Speaker, "You", StringComparison.Ordinal)) return;
            int revision = executive.Cortex.Revision;
            if (revision <= lastHandledRevision) return;
            lastHandledRevision = revision;
            string question = history[history.Count - 2].Text;
            SparkLabFocusIntent.Kind kind = SparkLabFocusIntent.Classify(question);
            if (kind == SparkLabFocusIntent.Kind.None) return;
            if (kind == SparkLabFocusIntent.Kind.Clear) { ClearCue(); LastInspection = "Inspection marker cleared."; InspectionChanged?.Invoke(LastInspection); return; }

            // Never draw when the reply explicitly says that target validation failed.
            if (string.IsNullOrWhiteSpace(answer) ||
                answer.IndexOf("unavailable", StringComparison.OrdinalIgnoreCase) >= 0 ||
                answer.IndexOf("stale", StringComparison.OrdinalIgnoreCase) >= 0 ||
                answer.IndexOf("cannot", StringComparison.OrdinalIgnoreCase) >= 0 ||
                answer.IndexOf("no additional required connection", StringComparison.OrdinalIgnoreCase) >= 0)
            { ClearCue(); return; }

            TryShowCurrent(kind);
        }

        /// <summary>
        /// Handle a visual command directly in SparkProExecutive before generic Cortex routing.
        /// The response reports the ACTUAL cue result, not a generic LED explanation.
        /// </summary>
        public string HandleVisualCommand(string question)
        {
            // AnswerReady will fire for the acknowledgement; don't process the command twice.
            skipNextAnswerReady = true;
            var kind = SparkLabFocusIntent.Classify(question);
            if (kind == SparkLabFocusIntent.Kind.None)
                return "This is not a laboratory visualization command.";
            if (kind == SparkLabFocusIntent.Kind.Clear)
            {
                CancelInspection();
                return "Laboratory inspection markers cleared.";
            }
            if (TryShowCurrent(kind))
                return LastInspection;
            return LastInspection;
        }

        private bool Fail(string reason)
        {
            ClearCue();
            LastInspection = reason;
            InspectionChanged?.Invoke(reason);
            return false;
        }

        public bool TryShowCurrent(SparkLabFocusIntent.Kind kind)
        {
            if (kind == SparkLabFocusIntent.Kind.Clear)
            {
                CancelInspection();
                return true;
            }
            if (!enableVisualInspection) return Fail("Laboratory visualization is disabled on this character.");
            if (!executive) executive = GetComponent<SparkProExecutive>();
            if (!executive) return Fail("Spark Lab is not connected to the active Spark executive.");
            if (executive.ActiveSelectionController) selection = executive.ActiveSelectionController;
            if (executive.ExamMode) return Fail("Inspection hints are unavailable during independent examinations.");
            if (character && character.WarningActive) return Fail("A safety warning is active; inspection hints are temporarily unavailable.");

            LevelGamePlayManager manager = executive.ActiveLevelManager;
            if (!manager) return Fail("No active laboratory mission is bound. Load a level to inspect its components.");
            SparkAILevelContext level = SparkAILevelContext.Capture(manager);
            if (!level.Layout || !level.Layout.gameObject.activeInHierarchy)
                return Fail("The active laboratory layout is not available. No world marker was shown.");

            Transform first = null, second = null;
            if (kind == SparkLabFocusIntent.Kind.NextConnection)
            {
                // Objectives can refer to the two terminals ON ONE diode. That is NOT evidence
                // of a required wire between them. We show endpoints without a connecting line.
                if (!SparkPhase4EvidenceGuard.EvaluatorIsCurrent(level))
                    return Fail("Mission evidence is stale. Update the evaluator before inspecting the next objective.");
                if (level.Manager.IsCompleted)
                    return Fail("The mission is complete. No additional required objective connection was identified.");
                if (!level.Layout.SceneBindings)
                    return Fail("No authored terminal binding table is available for this layout.");
                SparkLevelTarget target = level.NextTarget();
                if (target == null || !target.HasRequiredConnectionPair)
                    return Fail("The next incomplete objective has no unique authored terminal pair to inspect.");
                if (!level.Layout.SceneBindings.TryGetTerminal(target.RequiredPositiveTerminalId, out SparkTerminal a) ||
                    !level.Layout.SceneBindings.TryGetTerminal(target.RequiredNegativeTerminalId, out SparkTerminal b) ||
                    !a || !b || a == b)
                    return Fail("The next objective's terminal references cannot be resolved uniquely in this scene.");
                if (!a.transform.IsChildOf(level.Layout.transform) || !b.transform.IsChildOf(level.Layout.transform))
                    return Fail("One or more authored terminals are outside the loaded laboratory layout.");
                first = a.transform;
                second = b.transform;
            }
            else
            {
                Transform selected = GetVerifiedSelection(level.Layout.transform);
                if (!selected)
                    return Fail("No verified in-scene component is selected. Use the Select Tool on the actual workbench component, then retry. Editor Hierarchy selection is not gameplay selection.");
                if (kind == SparkLabFocusIntent.Kind.Selected)
                {
                    first = selected;
                }
                else
                {
                    if (!TryResolveRoleTerminal(selected, level.Layout.transform, kind, out SparkTerminal match, out string reason))
                        return Fail(reason);
                    first = match.transform;
                }
            }
            if (!first || !first.IsChildOf(level.Layout.transform))
                return Fail("The requested 3D target could not be validated in the current layout.");
            if (!GetVisual().Show(first, second, level.Layout.transform, first.name))
                return Fail("The 3D marker could not be displayed. Check the active camera and URP unlit shader.");
            if (character && !character.WarningActive) character.PointAt(first, false, 4f);
            LastInspection = second != null
                ? "Highlighted authored terminal references " + first.name + " and " + second.name +
                  ". These are NOT confirmed wire endpoints. Check the circuit schematic before making any connection; no wiring was changed."
                : "Highlighted the verified in-scene target " + first.name + ". No wiring was changed.";
            InspectionChanged?.Invoke(LastInspection);
            return true;
        }

        // Use current gameplay selection first; only then consider a recently verified target.
        // A new shelf/out-of-layout selection invalidates recall rather than targeting a stale item.
        private Transform GetVerifiedSelection(Transform currentLayout)
        {
            if (executive && executive.ActiveSelectionController)
                selection = executive.ActiveSelectionController;
            Transform current = selection && selection.SelectedObject ? selection.SelectedObject.transform : null;
            if (current)
            {
                if (!current.gameObject.activeInHierarchy || !current.IsChildOf(currentLayout))
                {
                    lastVerifiedSelection = null;
                    return null;
                }
                lastVerifiedSelection = current;
                lastVerifiedAt = Time.unscaledTime;
                return current;
            }
            if (lastVerifiedSelection && lastVerifiedSelection.gameObject.activeInHierarchy &&
                lastVerifiedSelection.IsChildOf(currentLayout) &&
                Time.unscaledTime - lastVerifiedAt <= SelectionRecallSeconds)
                return lastVerifiedSelection;
            lastVerifiedSelection = null;
            return null;
        }

        // SparkDiode / SparkLED components can assign anode and cathode references
        // explicitly, even when those SparkTerminal objects are siblings, not children,
        // and even when their generic Polarity is set to None/Unspecified.
        // We look up only the known serialized member names on those two types.
        private static SparkTerminal ReadAssignedTerminal(MonoBehaviour source, bool anode)
        {
            if (!source) return null;
            Type type = source.GetType();
            string[] names = anode ? AnodeNames : CathodeNames;
            for (int i = 0; i < names.Length; i++)
            {
                var field = type.GetField(names[i], TerminalMemberFlags);
                if (field != null && typeof(SparkTerminal).IsAssignableFrom(field.FieldType))
                    return field.GetValue(source) as SparkTerminal;
                var prop = type.GetProperty(names[i], TerminalMemberFlags);
                if (prop != null && prop.GetIndexParameters().Length == 0 &&
                    typeof(SparkTerminal).IsAssignableFrom(prop.PropertyType))
                {
                    try { return prop.GetValue(source, null) as SparkTerminal; }
                    catch (TargetInvocationException) { /* Ignore throwing accessors. */ }
                }
            }
            return null;
        }

        private static bool TryResolveRoleTerminal(Transform selected, Transform layout,
            SparkLabFocusIntent.Kind kind, out SparkTerminal terminal, out string failure)
        {
            terminal = null;
            failure = "No unique terminal was resolved.";
            bool anode = kind == SparkLabFocusIntent.Kind.PositiveTerminal;
            string label = anode ? "anode" : "cathode";

            // Prefer the actual diode/LED authoring references shown in the Inspector.
            // Do not infer anode from a generic positive supply terminal.
            Transform cursor = selected;
            while (cursor && cursor.IsChildOf(layout))
            {
                var scripts = cursor.GetComponents<MonoBehaviour>();
                for (int i = 0; i < scripts.Length; i++)
                {
                    MonoBehaviour script = scripts[i];
                    if (!script) continue;
                    string typeName = script.GetType().Name;
                    if (!string.Equals(typeName, "SparkDiode", StringComparison.Ordinal) &&
                        !string.Equals(typeName, "SparkLED", StringComparison.Ordinal)) continue;
                    SparkTerminal assigned = ReadAssignedTerminal(script, anode);
                    if (assigned)
                    {
                        if (!assigned.gameObject.activeInHierarchy || !assigned.transform.IsChildOf(layout))
                        {
                            failure = "The selected " + typeName + " has an assigned " + label +
                                " terminal, but it is not active inside this laboratory layout.";
                            return false;
                        }
                        terminal = assigned;
                        return true;
                    }
                }
                if (cursor == layout) break;
                cursor = cursor.parent;
            }

            // Fallback: exact terminal role names under the selected component, not
            // generic Positive/Negative polarity. Require a single unambiguous match.
            var component = selected.GetComponentInParent<SparkElectronicObject>();
            Transform scope = component ? component.transform : selected;
            var candidates = scope.GetComponentsInChildren<SparkTerminal>(true);
            SparkTerminal found = null;
            for (int i = 0; i < candidates.Length; i++)
            {
                SparkTerminal candidate = candidates[i];
                if (!candidate || !candidate.gameObject.activeInHierarchy ||
                    !candidate.transform.IsChildOf(layout) ||
                    candidate.name.IndexOf(label, StringComparison.OrdinalIgnoreCase) < 0) continue;
                if (found)
                {
                    failure = "More than one " + label + " terminal exists on this selection. Select a specific terminal.";
                    return false;
                }
                found = candidate;
            }
            if (found) { terminal = found; return true; }
            failure = "No unique " + label + " terminal reference was found. Confirm the diode or LED's " +
                (anode ? "Anode Terminal" : "Cathode Terminal") +
                " field is assigned in the in-scene component, then select that component using the Select Tool.";
            return false;
        }

        public void CancelInspection()
        {
            ClearCue();
            lastVerifiedSelection = null;
            LastInspection = "Inspection cancelled.";
            InspectionChanged?.Invoke(LastInspection);
        }
    }
}
