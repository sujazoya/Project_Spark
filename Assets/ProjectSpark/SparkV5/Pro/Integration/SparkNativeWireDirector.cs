using System;
using System.Collections;
using System.Text.RegularExpressions;
using ProjectSpark.Circuit;
using ProjectSpark.Electrical;
using ProjectSpark.Gameplay;
using UnityEngine;
using UnityEngine.SceneManagement;

using ProjectSpark.AI.Tutorial;
namespace ProjectSpark.SparkV5.Pro
{
    /// <summary>
    /// Drives the player's real wire tool as Spark's avatar. No decorative cable, no
    /// graph-only connection. All live topology comes from the native player's tool.
    /// Fail closed if native tool or physical endpoints are not genuinely bound.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkNativeWireDirector : MonoBehaviour
    {
        [SerializeField] private SparkProExecutive executive;
        [SerializeField] private SparkProCharacter character;
        [SerializeField] private SparkAINativeWirePort playerWirePort;
        [SerializeField] private SparkWorkbenchNavigation workbench;
        [SerializeField] private bool requireWorkbenchNavigation;
        [SerializeField] private bool requireVirtualPowerOff = true;
        [SerializeField, Range(0.4f, 10f)] private float routingSeconds = 2.5f;
        [SerializeField, Range(0.05f, 2f)] private float gesturePauseSeconds = 0.35f;
        [SerializeField] private float cableLift = 0.14f;

        private const string Pattern = @"^(?:spark[ ,]+)?(?:physically\s+)?(?:draw|create|connect|route|make)\s+(?:a\s+)?(?:real\s+|native\s+)?wire\s+from\s+<?(?<a>[A-Za-z0-9_.-]+)>?\s+to\s+<?(?<b>[A-Za-z0-9_.-]+)>?\.?$";
        private static readonly Regex WireRegex = new Regex(Pattern,
            RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        private SparkTerminal pendingA, pendingB;
        private string fromId, toId;
        private Transform layoutRoot;
        private LevelGamePlayManager levelManager;
        private Coroutine wireRoutine;
        private bool cancelling;
        private SparkTerminal lastCompletedA, lastCompletedB;
        public string LastStatus { get; private set; } = "Native player Wire Tool has not been connected.";
        public bool IsBusy => wireRoutine != null;
        internal SparkWorkbenchNavigation Workbench => workbench;
        // The reviewed-plan runner must allow time for TWO walks and two hand reaches.
        // Read dynamically from the optional performer; preserve v11 timing if it is absent.
        internal float ActionTimeout
        {
            get
            {
                float legacy = workbench
                    ? 2f * SparkWorkbenchNavigation.WalkTimeout + routingSeconds + gesturePauseSeconds + 2f
                    : routingSeconds + gesturePauseSeconds + 2f;
                SparkNativeWireCinematic cinematic = GetComponent<SparkNativeWireCinematic>();
                return cinematic ? Mathf.Max(legacy, cinematic.SuggestedWireTimeoutSeconds) : legacy;
            }
        }
        public event Action<string> StatusChanged;

        private void Awake()
        {
            if (!executive) executive = GetComponent<SparkProExecutive>();
            if (!character) character = GetComponent<SparkProCharacter>();
            if (!playerWirePort) playerWirePort = GetComponent<SparkAINativeWirePort>();
        }
        private void OnEnable() { SceneManager.activeSceneChanged += SceneChanged; }
        private void OnDisable() { SceneManager.activeSceneChanged -= SceneChanged; Cancel(); }
        private void SceneChanged(Scene a, Scene b) { Cancel(); LastStatus = "Scene changed; native wiring cancelled."; }

        public static bool RecognizesCommand(string prompt)
        {
            if (string.IsNullOrWhiteSpace(prompt)) return false;
            string command = prompt.Trim();
            return WireRegex.IsMatch(command) ||
                command.Equals("native wire status", StringComparison.OrdinalIgnoreCase) ||
                command.Equals("real wire status", StringComparison.OrdinalIgnoreCase) ||
                command.Equals("confirm wire", StringComparison.OrdinalIgnoreCase) ||
                command.Equals("spark connect wire", StringComparison.OrdinalIgnoreCase) ||
                command.Equals("cancel wire", StringComparison.OrdinalIgnoreCase) ||
                command.Equals("stop wiring", StringComparison.OrdinalIgnoreCase) ||
                command.Equals("undo last AI wire", StringComparison.OrdinalIgnoreCase);
        }

        public static bool TryHandleFrom(SparkProExecutive source, string prompt, out string answer)
        {
            answer = null;
            if (!source || string.IsNullOrWhiteSpace(prompt)) return false;
            string command = prompt.Trim();
            Match match = WireRegex.Match(command);
            bool nativeStatus = command.Equals("native wire status", StringComparison.OrdinalIgnoreCase) ||
                command.Equals("real wire status", StringComparison.OrdinalIgnoreCase);
            bool confirm = command.Equals("confirm wire", StringComparison.OrdinalIgnoreCase) ||
                command.Equals("spark connect wire", StringComparison.OrdinalIgnoreCase);
            bool cancel = command.Equals("cancel wire", StringComparison.OrdinalIgnoreCase) ||
                command.Equals("stop wiring", StringComparison.OrdinalIgnoreCase);
            bool undo = command.Equals("undo last AI wire", StringComparison.OrdinalIgnoreCase);
            if (!match.Success && !nativeStatus && !confirm && !cancel && !undo) return false;
            SparkNativeWireDirector director = source.GetComponent<SparkNativeWireDirector>();
            if (!director) { answer = "Native Wire Director is not bound; run AAA Laboratory > 16 - Bind Native Wire Director."; return true; }
            if (nativeStatus) answer = director.Status();
            else if (cancel) { director.Cancel(); answer = director.Report("Native wiring cancelled; no unconfirmed wiring was committed."); }
            else if (undo) answer = director.Undo();
            else if (confirm) answer = director.Confirm();
            else answer = director.Prepare(match.Groups["a"].Value, match.Groups["b"].Value);
            return true;
        }

        /// <summary>Trusted, reviewed practice-plan entry point. Uses the EXACT manual native wire routine.</summary>
        public bool TryStartApprovedStep(string sourceId, string destinationId, out string feedback)
        {
            feedback = null;
            if (wireRoutine != null) { feedback = "Native wire action already running."; return false; }
            if (pendingA || pendingB)
            { feedback = "An unconfirmed manual wire is pending. Cancel it before auto wiring."; return false; }
            feedback = Prepare(sourceId, destinationId);
            if (!pendingA || !pendingB || fromId != sourceId || toId != destinationId) return false;
            feedback = Confirm();
            if (wireRoutine == null) pendingA = pendingB = null;
            return wireRoutine != null;
        }
        private string Status() => Report("Native player wire port: " +
            (playerWirePort ? (playerWirePort.IsConfigured ? "configured" : "not wired to player tool") : "missing") +
            (workbench ? ". Tabletop navigation is bound. " : ". ") + LastStatus);
        private string Report(string message)
        {
            LastStatus = message;
            StatusChanged?.Invoke(message);
            Debug.Log("[Spark Native Wire] " + message, this);
            return message;
        }
        private bool TryGetContext(out LevelGamePlayManager manager, out SparkAILevelContext level, out string error)
        {
            manager = executive ? executive.ActiveLevelManager : null;
            level = manager ? SparkAILevelContext.Capture(manager) : default;
            error = null;
            if (!manager || !level.Definition || !level.Layout || !level.Layout.gameObject.activeInHierarchy ||
                !manager.CircuitSystem || !level.Layout.SceneBindings)
            { error = "Active level, CircuitSystem or authored SceneBindings is missing."; return false; }
            if (executive.ExamMode) { error = "Independent exam mode prevents AI wiring."; return false; }
            if (character && character.WarningActive) { error = "A character safety warning is active."; return false; }
            if (requireVirtualPowerOff)
            {
                SparkPowerSupply[] supplies = level.Layout.GetComponentsInChildren<SparkPowerSupply>(true);
                for (int i = 0; i < supplies.Length; i++)
                    if (supplies[i] && supplies[i].IsOutputActive)
                    { error = "Turn off the virtual power supply before AI wiring."; return false; }
            }
            return true;
        }
        private bool TryPair(SparkAILevelContext level, string aId, string bId,
            out SparkTerminal a, out SparkTerminal b, out string error)
        {
            a = b = null;
            error = null;
            if (!level.Layout.SceneBindings.TryGetTerminal(aId, out a) ||
                !level.Layout.SceneBindings.TryGetTerminal(bId, out b) || !a || !b)
            { error = "Exact terminal IDs were not found in active level SceneBindings."; return false; }
            // Diagnostic v10.1: preserve all validation gates, identify the failing one.
            if (a == b)
            { error = "Terminal IDs resolve to the same SparkTerminal. No wire created."; return false; }
            if (!a.gameObject.activeInHierarchy)
            { error = "Source terminal '" + aId + "' is INACTIVE: " + a.name + ". No wire created."; return false; }
            if (!b.gameObject.activeInHierarchy)
            { error = "Destination terminal '" + bId + "' is INACTIVE: " + b.name + ". No wire created."; return false; }
            if (!a.transform.IsChildOf(level.Layout.transform))
            { error = "Source terminal '" + aId + "' is OUTSIDE active layout '" +
                level.Layout.name + "': " + a.name + " (parent=" +
                (a.transform.parent ? a.transform.parent.name : "NONE") + "). No wire created."; return false; }
            if (!b.transform.IsChildOf(level.Layout.transform))
            { error = "Destination terminal '" + bId + "' is OUTSIDE active layout '" +
                level.Layout.name + "': " + b.name + " (parent=" +
                (b.transform.parent ? b.transform.parent.name : "NONE") + "). No wire created."; return false; }
            if (a.Owner && b.Owner && a.Owner == b.Owner)
            { error = "Refusing to bypass one component by shorting its two own terminals."; return false; }
            return true;
        }
        private string Prepare(string idA, string idB)
        {
            if (wireRoutine != null) return Report("Already routing a wire; cancel first.");
            if (!playerWirePort || !playerWirePort.IsConfigured)
                return Report("Real PLAYER Wire Tool is not bound. No fake wire will be created. Bind the native tool first.");
            if (!TryGetContext(out var manager, out var level, out string error)) return Report(error);
            if (!TryPair(level, idA, idB, out var a, out var b, out error)) return Report(error);
            if (manager.CircuitSystem.HasConnectionBetween(a, b)) return Report("Already connected: no duplicate wire.");
            pendingA = a; pendingB = b;
            fromId = idA; toId = idB;
            layoutRoot = level.Layout.transform;
            levelManager = manager;
            if (character) character.PointAt(a.transform, false, 1f);
            return Report("Native PLAYER cable requested: " + idA + " → " + idB +
                ". No circuit change yet. Verify the actual schematic, then say 'confirm wire'.");
        }
        private string Confirm()
        {
            if (wireRoutine != null) return Report("Native wiring is already active.");
            if (!pendingA || !pendingB || !playerWirePort || !playerWirePort.IsConfigured)
                return Report("No valid pending native wire or player wire port.");
            if (!TryGetContext(out var m, out var l, out string error)) return Report(error);
            if (m != levelManager || l.Layout.transform != layoutRoot ||
                !TryPair(l, fromId, toId, out var a, out var b, out error) ||
                a != pendingA || b != pendingB)
                return Report("Level or terminal bindings changed; preview again.");
            if (m.CircuitSystem.HasConnectionBetween(a, b)) return Report("Already connected.");
            // V12.2 exact merge: the performer plans its own verified NavMesh path.
            // Preserve the legacy workbench planner whenever the performer is absent.
            SparkNativeWireCinematic cinematicV122 = GetComponent<SparkNativeWireCinematic>();
            if (requireWorkbenchNavigation && !workbench && !cinematicV122)
                return Report("Spark's worktable navigation is unavailable. Wiring cancelled.");
            Vector3 sourceStance = default, destinationStance = default;
            if (!cinematicV122 && workbench &&
                !workbench.TryPlan(character, layoutRoot, a, b, out sourceStance, out destinationStance, out error))
                return Report(error);
            cancelling = false;
            wireRoutine = StartCoroutine(DoWiring(m, m.CircuitSystem, layoutRoot, a, b, sourceStance, destinationStance));
            return Report(cinematicV122 ? "Spark is preparing to walk, pick up and route a real player cable." :
                workbench ? "Spark is walking to the source terminal." :
                "Spark is operating the player's Wire Tool.");
        }
        private bool StillValid(LevelGamePlayManager manager, SparkCircuitSystem circuit, Transform root,
            SparkTerminal a, SparkTerminal b, bool beforeCommit = true)
        {
            if (cancelling || !a || !b || !root || !playerWirePort || !circuit ||
                !a.gameObject.activeInHierarchy || !b.gameObject.activeInHierarchy ||
                !TryGetContext(out var m, out var l, out _) || m != manager || m.CircuitSystem != circuit ||
                m.IsFailed || l.Layout.transform != root ||
                !l.Layout.SceneBindings.TryGetTerminal(fromId, out var source) || source != a ||
                !l.Layout.SceneBindings.TryGetTerminal(toId, out var destination) || destination != b)
                return false;
            return !beforeCommit || !circuit.HasConnectionBetween(a, b);
        }

        private bool HasReached(Vector3 stance)
        {
            var agent = character ? character.NavigationAgent : null;
            return agent && agent.isActiveAndEnabled && agent.isOnNavMesh &&
                Vector3.Distance(agent.transform.position, stance) <= agent.stoppingDistance + .08f;
        }

        private void FaceTerminal(SparkTerminal terminal)
        {
            character.StopNavigation();
            Vector3 direction = Vector3.ProjectOnPlane(terminal.transform.position - character.transform.position, Vector3.up);
            if (direction.sqrMagnitude > .01f)
                character.transform.rotation = Quaternion.LookRotation(direction);
        }

        private IEnumerator DoWiring(LevelGamePlayManager manager, SparkCircuitSystem circuit, Transform root,
            SparkTerminal a, SparkTerminal b, Vector3 sourceStance, Vector3 destinationStance)
        {
            // SPARK_CINEMATIC_V12_2_BEGIN
            // This branch is driven by actual rig pivots, reachable NavMesh positions and
            // the same playerWirePort used by the existing player Wire Tool.
            // It does not fabricate connections or bypass the circuit solver.
            SparkNativeWireCinematic cinematicV122 = GetComponent<SparkNativeWireCinematic>();
            if (cinematicV122)
            {
                yield return null;
                if (!StillValid(manager, circuit, root, a, b))
                {
                    Fail("Native wiring preconditions changed before physical performance.");
                    yield break;
                }
                yield return cinematicV122.Perform(a, b, playerWirePort, workbench,
                    () => StillValid(manager, circuit, root, a, b),
                    () => StillValid(manager, circuit, root, a, b, beforeCommit: false));
                if (!cinematicV122.Succeeded)
                {
                    Fail("Spark could not complete the physical wiring: " + cinematicV122.LastError);
                    yield break;
                }
                // The physical cable and electrical graph MUST agree before success.
                yield return null;
                yield return new WaitForFixedUpdate();
                if (!manager || !circuit || manager.CircuitSystem != circuit ||
                    !circuit.HasConnectionBetween(a, b))
                {
                    Fail("A visual cable was placed, but the real circuit graph did not confirm the link.");
                    yield break;
                }
                lastCompletedA = a;
                lastCompletedB = b;
                pendingA = pendingB = null;
                wireRoutine = null;
                string completed = Report("Spark connected the physical player cable and verified the circuit link. " +
                    "Current and mission completion still require fresh solver and evaluator evidence.");
                if (executive) executive.PublishWirePerformanceNarration(completed);
                yield break;
            }
            // Existing tested 10-wire v11 action follows unchanged as the fallback.
            // SPARK_CINEMATIC_V12_2_END
            // Store the coroutine before an immediate rejection can clear it.
            yield return null;
            bool navigate = workbench;
            if (!StillValid(manager, circuit, root, a, b)) { Fail("Wiring preconditions changed."); yield break; }
            if (navigate)
            {
                if (!HasReached(sourceStance) && !character.WalkTo(sourceStance))
                { Fail("Spark could not start walking to the source terminal."); yield break; }
                float deadline = Time.realtimeSinceStartup + SparkWorkbenchNavigation.WalkTimeout;
                while (!HasReached(sourceStance))
                {
                    if (!workbench || !StillValid(manager, circuit, root, a, b) || Time.realtimeSinceStartup >= deadline)
                    { Fail("Spark did not reach the source terminal. Wiring cancelled."); yield break; }
                    yield return null;
                }
                FaceTerminal(a);
            }
            if (!playerWirePort.Begin(a, b, out string error)) { Fail("Native begin rejected: " + error); yield break; }
            if (character) { character.SetState(SparkState.Teaching); character.PointAt(a.transform, false, 1f); }
            yield return new WaitForSecondsRealtime(gesturePauseSeconds);
            if (!StillValid(manager, circuit, root, a, b) || (navigate && (!workbench || !HasReached(sourceStance))))
            { Fail("Interrupted before source attachment."); yield break; }
            if (!playerWirePort.AttachSource(a, out error)) { Fail("Native source rejected: " + error); yield break; }
            if (character) character.PlayGesture(SparkGesture.Grasp, 1.5f);
            if (navigate && !HasReached(destinationStance) && !character.WalkTo(destinationStance))
            { Fail("Spark could not start walking to the destination terminal."); yield break; }
            float elapsed = 0f;
            float destinationDeadline = Time.realtimeSinceStartup + SparkWorkbenchNavigation.WalkTimeout;
            Vector3 start = a.transform.position, end = b.transform.position;
            while (elapsed < routingSeconds || (navigate && !HasReached(destinationStance)))
            {
                if (!StillValid(manager, circuit, root, a, b) ||
                    (navigate && (!workbench || Time.realtimeSinceStartup >= destinationDeadline)))
                { Fail("Spark's cable route was interrupted before the destination."); yield break; }
                elapsed += Time.unscaledDeltaTime;
                float progress = Mathf.Min(elapsed / routingSeconds,
                    navigate && !HasReached(destinationStance) ? .94f : 1f);
                float t = Mathf.SmoothStep(0f, 1f, progress);
                Vector3 position = Vector3.Lerp(start, end, t) +
                    Vector3.up * (Mathf.Sin(t * Mathf.PI) * cableLift);
                if (!playerWirePort.MoveLooseEnd(position, out error))
                { Fail("Player tool refused wire movement: " + error); yield break; }
                if (character && Time.frameCount % 12 == 0) character.PointAtPosition(position, false, 0.4f);
                yield return null;
            }
            if (!StillValid(manager, circuit, root, a, b) || (navigate && (!workbench || !HasReached(destinationStance))))
            { Fail("Interrupted before destination attachment."); yield break; }
            if (navigate) FaceTerminal(b);
            if (!playerWirePort.MoveLooseEnd(end, out error) ||
                !playerWirePort.AttachDestination(b, out error))
            { Fail("Native destination rejected: " + error); yield break; }
            // Permit the original tool's contact/electrical events to propagate.
            yield return null;
            yield return new WaitForFixedUpdate();
            bool connected = manager && circuit && manager.CircuitSystem == circuit && circuit.HasConnectionBetween(a, b);
            if (!connected)
            {
                Fail("Native player tool finished visually but circuit graph did NOT confirm an electrical link. " +
                     "Inspect physical plug/contact registration; no success claimed.");
                yield break;
            }
            lastCompletedA = a;
            lastCompletedB = b;
            pendingA = pendingB = null;
            wireRoutine = null;
            Report("Player-native cable attached and confirmed by SparkCircuitSystem. " +
                   "Current still requires a complete powered route and fresh solver evidence.");
        }
        private void Fail(string why)
        {
            // Stop hand tracking and restore the existing NavMeshAgent state.
            SparkNativeWireCinematic cinematicV122 = GetComponent<SparkNativeWireCinematic>();
            if (cinematicV122) cinematicV122.CancelPerformance();
            if (character && workbench) character.StopNavigation();
            if (playerWirePort) playerWirePort.Abort();
            pendingA = pendingB = null;
            wireRoutine = null;
            Report(why);
        }
        public void Cancel()
        {
            cancelling = true;
            SparkNativeWireCinematic cinematicV122 = GetComponent<SparkNativeWireCinematic>();
            if (cinematicV122) cinematicV122.CancelPerformance();
            if (character && workbench) character.StopNavigation();
            if (wireRoutine != null) StopCoroutine(wireRoutine);
            wireRoutine = null;
            if (playerWirePort) playerWirePort.Abort();
            pendingA = pendingB = null;
            levelManager = null;
            layoutRoot = null;
        }
        private string Undo()
        {
            if (wireRoutine != null) return Report("Cancel the current physical wire first.");
            if (!lastCompletedA || !lastCompletedB) return Report("No confirmed AI-native wire to undo.");
            if (!TryGetContext(out var manager, out var level, out string error)) return Report(error);
            if (!manager.CircuitSystem.HasConnectionBetween(lastCompletedA,lastCompletedB))
            { lastCompletedA = lastCompletedB = null; return Report("Previously created wire no longer exists."); }
            if (!playerWirePort || !playerWirePort.UndoLastCompletedWire(out error))
                return Report("Native player tool cannot undo this connection: " + error);
            if (manager.CircuitSystem.HasConnectionBetween(lastCompletedA,lastCompletedB))
                return Report("Native undo callback ran, but circuit connection still exists; verify the player's Wire Tool.");
            lastCompletedA = lastCompletedB = null;
            return Report("Last AI-native wire removed using player's original wire tool.");
        }
    }
}
