using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using ProjectSpark.AI.Tutorial;
using ProjectSpark.Circuit;
using ProjectSpark.Gameplay;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProjectSpark.SparkV5.Pro
{
    /// <summary>
    /// Records observed real player circuit connections as unapproved drafts, then replays
    /// manually reviewed practice plans through SparkNativeWireDirector -> SignalWireBuilder.
    /// It never calls TryCreateConnection directly and never writes level results.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkNativeWireAutoEngineer : MonoBehaviour
    {
        [SerializeField] private SparkProExecutive executive;
        [SerializeField] private SparkNativeWireDirector director;
        [SerializeField] private bool requireVirtualPowerOff = true;
        [SerializeField, Range(5f, 45f)] private float perWireTimeout = 15f;
        [SerializeField] private bool verbose;

        private readonly List<SparkNativeWiringPlan.WireStep> recorded =
            new List<SparkNativeWiringPlan.WireStep>(64);
        private SparkCircuitSystem observedCircuit;
        private SparkLevelSceneBindings observedBindings;
        private Transform observedLayoutRoot;
        private int recordedLevel;
        private string recordedLayout;
        private bool recording;
        private Coroutine executing;
        private bool cancelRequested;
        private SparkNativeWiringPlan[] loadedPlans;
        private string lastStatus = "Ready; no approved practice wiring plan selected.";

        public bool IsRecording => recording;
        public bool IsExecuting => executing != null;
        public string LastStatus => lastStatus;

        private void Awake()
        {
            if (!executive) executive = GetComponent<SparkProExecutive>();
            if (!director) director = GetComponent<SparkNativeWireDirector>();
            ReloadPlans();
        }
        private void OnEnable() { SceneManager.activeSceneChanged += OnSceneChanged; }
        private void OnDisable()
        {
            SceneManager.activeSceneChanged -= OnSceneChanged;
            CancelAll();
        }
        private void OnSceneChanged(Scene a, Scene b) { CancelAll(); lastStatus = "Scene changed; auto wiring cancelled."; }

        private void ReloadPlans() => loadedPlans = Resources.LoadAll<SparkNativeWiringPlan>("SparkNativeWirePlans");
        private string Reply(string message)
        {
            lastStatus = message;
            if (verbose) Debug.Log("[Spark Auto Engineer] " + message, this);
            return message;
        }
        private bool TryContext(out SparkAILevelContext level, out string error)
        {
            error = null;
            level = default;
            if (!executive || !executive.ActiveLevelManager)
                { error = "No active gameplay level manager."; return false; }
            if (executive.ExamMode)
                { error = "Independent examination: AI auto wiring is disabled."; return false; }
            level = SparkAILevelContext.Capture(executive.ActiveLevelManager);
            if (!level.Definition || !level.Layout || !level.Layout.SceneBindings ||
                !level.Layout.gameObject.activeInHierarchy || !executive.ActiveLevelManager.CircuitSystem)
                { error = "The active level, scene bindings or circuit is unavailable."; return false; }
            return true;
        }
        private bool IsSameContext(SparkAILevelContext level)
        {
            return level.Layout && observedLayoutRoot &&
                level.Layout.transform == observedLayoutRoot &&
                executive && executive.ActiveLevelManager &&
                executive.ActiveLevelManager.CircuitSystem == observedCircuit;
        }
        private static string LayoutKey(string name) =>
            (name ?? string.Empty).Replace("(Clone)", string.Empty).Trim();

        public static bool TryHandleFrom(SparkProExecutive source, string message, out string answer)
        {
            answer = null;
            if (!source || string.IsNullOrWhiteSpace(message)) return false;
            string q = message.Trim().TrimEnd('.').ToLowerInvariant();
            bool recognized = q == "auto wiring status" || q == "spark wiring status" ||
                q == "start wiring recording" || q == "record my wiring" ||
                q == "stop wiring recording" || q == "finish wiring recording" ||
                q == "preview approved wiring plan" || q == "preview wiring plan" ||
                q == "auto wire practice level" || q == "spark wire this practice level" ||
                q == "cancel auto wiring" || q == "reload wiring plans";
            if (!recognized) return false;
            var agent = source.GetComponent<SparkNativeWireAutoEngineer>();
            if (!agent) { answer = "Auto Engineer v11 is not bound. Use AAA Laboratory > 19 - Bind Auto Engineer."; return true; }
            switch (q)
            {
                case "auto wiring status": case "spark wiring status": answer = agent.Status(); break;
                case "start wiring recording": case "record my wiring": answer = agent.StartRecording(); break;
                case "stop wiring recording": case "finish wiring recording": answer = agent.StopRecording(); break;
                case "preview approved wiring plan": case "preview wiring plan": answer = agent.PreviewPlan(); break;
                case "auto wire practice level": case "spark wire this practice level": answer = agent.StartApprovedPlan(); break;
                case "cancel auto wiring": agent.CancelAll(); answer = agent.Reply("Auto wiring and recording cancelled; already completed wires were not removed."); break;
                case "reload wiring plans": agent.ReloadPlans(); answer = agent.Reply("Reviewed wiring-plan assets reloaded."); break;
            }
            return true;
        }
        private string Status()
        {
            if (!TryContext(out SparkAILevelContext level, out string reason)) return Reply(reason);
            var plan = FindPlan(level, out _);
            return Reply("Level " + level.Definition.LevelNumber + " | recording=" + recording +
                " | executing=" + IsExecuting + " | recorded=" + recorded.Count +
                " | approved plan=" + (plan ? plan.name : "NONE") + " | " + lastStatus);
        }
        private string StartRecording()
        {
            if (recording || executing != null) return Reply("An auto-wiring operation is already active.");
            if (!TryContext(out SparkAILevelContext level, out string error)) return Reply(error);
            observedCircuit = executive.ActiveLevelManager.CircuitSystem;
            observedBindings = level.Layout.SceneBindings;
            observedLayoutRoot = level.Layout.transform;
            recordedLevel = level.Definition.LevelNumber;
            recordedLayout = LayoutKey(level.Layout.name);
            recorded.Clear();
            observedCircuit.ConnectionCreated += OnConnectionCreated;
            observedCircuit.ConnectionRemoved += OnConnectionRemoved;
            recording = true;
            return Reply("Recording real circuit connections for Level " + recordedLevel +
                ". Wire the lesson using the normal PLAYER tool, then say 'stop wiring recording'. " +
                "This is a DRAFT, not an approved solution.");
        }
        private void OnConnectionCreated(SparkCircuitConnection connection)
        {
            if (!recording || !observedBindings || !observedCircuit || !observedLayoutRoot ||
                !connection.A || !connection.B ||
                !connection.A.transform.IsChildOf(observedLayoutRoot) ||
                !connection.B.transform.IsChildOf(observedLayoutRoot)) return;
            if (!observedBindings.TryGetTerminalId(connection.A, out string a) ||
                !observedBindings.TryGetTerminalId(connection.B, out string b))
            { Debug.LogWarning("[Spark Auto Engineer] Ignoring unresolvable or ambiguous connection in recording.", this); return; }
            if (connection.A.Owner && connection.A.Owner == connection.B.Owner)
            { Debug.LogWarning("[Spark Auto Engineer] Ignoring same-component shortcut.", this); return; }
            for (int i = 0; i < recorded.Count; i++)
                if (Matches(recorded[i], a, b)) return;
            recorded.Add(new SparkNativeWiringPlan.WireStep { fromId = a, toId = b });
        }
        private static bool Matches(SparkNativeWiringPlan.WireStep w, string a, string b) =>
            (w.fromId == a && w.toId == b) || (w.fromId == b && w.toId == a);
        private void OnConnectionRemoved(SparkCircuitConnection connection)
        {
            if (!recording || !observedBindings || !connection.A || !connection.B ||
                !observedBindings.TryGetTerminalId(connection.A, out string a) ||
                !observedBindings.TryGetTerminalId(connection.B, out string b)) return;
            for (int i = recorded.Count - 1; i >= 0; i--)
                if (Matches(recorded[i], a, b)) recorded.RemoveAt(i);
        }
        private string StopRecording()
        {
            if (!recording) return Reply("No wiring recording is active.");
            Unsubscribe();
            recording = false;
            if (recorded.Count == 0) return Reply("Recording stopped; no new resolvable connections were observed.");
            var draft = new SparkNativeWiringPlan.RecordedDraft
            {
                schemaVersion = 1, levelNumber = recordedLevel,
                layoutName = recordedLayout,
                recordedUtc = DateTime.UtcNow.ToString("o"), wires = recorded.ToArray()
            };
            string folder = Path.Combine(Application.persistentDataPath, "SparkNativeWireDrafts");
            try
            {
                Directory.CreateDirectory(folder);
                string path = Path.Combine(folder, "Level_" + recordedLevel + "_WiringDraft.json");
                File.WriteAllText(path, JsonUtility.ToJson(draft, true));
                return Reply("Recorded " + recorded.Count + " observed connections. DRAFT saved: " + path +
                    ". Import and review it in the Unity Editor. Nothing was auto-approved.");
            }
            catch (Exception e) { return Reply("Recording stopped but could not save draft: " + e.Message); }
        }
        private void Unsubscribe()
        {
            if (!observedCircuit) return;
            observedCircuit.ConnectionCreated -= OnConnectionCreated;
            observedCircuit.ConnectionRemoved -= OnConnectionRemoved;
        }
        private SparkNativeWiringPlan FindPlan(SparkAILevelContext level, out string error)
        {
            error = "No reviewed plan for this active layout. Record a working PLAYER circuit first.";
            SparkNativeWiringPlan match = null;
            string key = LayoutKey(level.Layout.name);
            if (loadedPlans != null)
                for (int i = 0; i < loadedPlans.Length; i++)
                {
                    var p = loadedPlans[i];
                    if (!p || !p.IsApproved || p.LevelNumber != level.Definition.LevelNumber ||
                        !string.Equals(p.LayoutName, key, StringComparison.Ordinal)) continue;
                    if (match) { error = "More than one approved plan matches this layout; remove ambiguity."; return null; }
                    match = p;
                }
            if (match) error = null;
            return match;
        }
        private bool CheckPair(SparkAILevelContext level, SparkNativeWiringPlan.WireStep step,
            out SparkTerminal a, out SparkTerminal b, out string error)
        {
            a = b = null;
            error = null;
            var bindings = level.Layout.SceneBindings;
            if (step == null || !bindings.TryGetTerminal(step.fromId, out a) ||
                !bindings.TryGetTerminal(step.toId, out b) || !a || !b)
                error = "Unresolved endpoints: " + step?.fromId + " -> " + step?.toId;
            else if (a == b || !a.gameObject.activeInHierarchy || !b.gameObject.activeInHierarchy ||
                !a.transform.IsChildOf(level.Layout.transform) || !b.transform.IsChildOf(level.Layout.transform))
                error = "Inactive or out-of-level terminals at " + step.fromId + " -> " + step.toId;
            else if (a.Owner && a.Owner == b.Owner)
                error = "Unacceptable same-component shortcut at " + step.fromId + " -> " + step.toId;
            return error == null;
        }
        private bool PowerIsOff(SparkAILevelContext level)
        {
            if (!requireVirtualPowerOff) return true;
            SparkPowerSupply[] supplies = level.Layout.GetComponentsInChildren<SparkPowerSupply>(true);
            for (int i = 0; i < supplies.Length; i++)
                if (supplies[i] && supplies[i].IsOutputActive) return false;
            return true;
        }
        private string PreviewPlan()
        {
            if (!TryContext(out var level, out string error)) return Reply(error);
            var plan = FindPlan(level, out error);
            if (!plan) return Reply(error);
            var message = new System.Text.StringBuilder("REVIEWED PRACTICE WIRING PLAN: ").Append(plan.name);
            for (int i = 0; i < plan.Wires.Count; i++)
                message.Append("\n").Append(i + 1).Append(". ").Append(plan.Wires[i].fromId)
                    .Append(" -> ").Append(plan.Wires[i].toId);
            return Reply(message.ToString());
        }
        private string StartApprovedPlan()
        {
            if (GetComponent<SparkEngineeringControlHub>())
                return Reply("The Engineering Control Hub owns reviewed plan execution in this scene.");
            if (executing != null || recording) return Reply("Finish the current auto-wiring operation first.");
            if (!director) return Reply("Native Wire Director is not bound.");
            if (!TryContext(out var level, out string error)) return Reply(error);
            var plan = FindPlan(level, out error);
            if (!plan) return Reply(error);
            if (!plan.Validate(out error)) return Reply("Plan validation failed: " + error);
            if (!PowerIsOff(level)) return Reply("Turn OFF the virtual power output before Spark wires the level.");
            var circuit = executive.ActiveLevelManager.CircuitSystem;
            for (int i = 0; i < plan.Wires.Count; i++)
                if (!CheckPair(level, plan.Wires[i], out _, out _, out error))
                    return Reply("Step " + (i + 1) + ": " + error);
            observedLayoutRoot = level.Layout.transform;
            observedCircuit = circuit;
            cancelRequested = false;
            executing = StartCoroutine(Execute(plan, circuit, observedLayoutRoot));
            return Reply("Spark is performing " + plan.Wires.Count +
                " reviewed practice wires using the SAME PLAYER WIRE TOOL. " +
                "Watch the physical cables; mission completion is evaluator-controlled.");
        }
        private IEnumerator Execute(SparkNativeWiringPlan plan, SparkCircuitSystem circuit, Transform root)
        {
            // Defer first work until the coroutine handle has been stored.
            yield return null;
            int created = 0, already = 0;
            string failure = null;
            for (int i = 0; i < plan.Wires.Count; i++)
            {
                if (cancelRequested || !TryContext(out var level, out failure) ||
                    !root || level.Layout.transform != root || executive.ActiveLevelManager.CircuitSystem != circuit ||
                    !PowerIsOff(level)) { failure = failure ?? "Level, power or permissions changed."; break; }
                if (!CheckPair(level, plan.Wires[i], out var a, out var b, out failure)) break;
                if (circuit.HasConnectionBetween(a, b)) { already++; continue; }
                if (executive.ActiveLevelManager.IsFailed)
                    { failure = "Game evaluator already reports a failed state; cannot continue."; break; }
                if (!director.TryStartApprovedStep(plan.Wires[i].fromId, plan.Wires[i].toId, out string startInfo))
                    { failure = "Step " + (i + 1) + " rejected: " + startInfo; break; }
                float deadline = Time.realtimeSinceStartup + Mathf.Max(perWireTimeout, director.ActionTimeout);
                while (director.IsBusy && !cancelRequested && Time.realtimeSinceStartup < deadline) yield return null;
                if (director.IsBusy) director.Cancel();
                if (cancelRequested) { failure = "Cancelled by player."; break; }
                if (Time.realtimeSinceStartup >= deadline) { failure = "Native wire action timed out."; break; }
                // Native tool and its real visual must have completed before topology is trusted.
                if (!circuit || !circuit.HasConnectionBetween(a, b))
                    { failure = "Player's wire tool did not verify this physical electrical cable."; break; }
                created++;
                if (verbose) Debug.Log("[Spark Auto Engineer] Native cable " + (i + 1) +
                    " confirmed; topology version=" + circuit.TopologyVersion, this);
                yield return new WaitForFixedUpdate();
                if (executive.ActiveLevelManager.IsFailed)
                {
                    // Do not automatically undo somebody else's wires; native Undo owns only Spark's latest wire.
                    failure = "Evaluator reported failure after step " + (i + 1) + ". Inspect and undo the last Spark wire if necessary.";
                    break;
                }
            }
            executing = null;
            if (failure != null) Reply("AUTO WIRING STOPPED: " + failure + " | Spark-created=" + created +
                ", already present=" + already + ". Existing confirmed connections are retained.");
            else Reply("AUTO WIRING FINISHED: " + created + " real player cables created, " + already +
                " already connected. Evaluator: " + (executive.ActiveLevelManager.IsCompleted ?
                "level objectives completed" : "objectives still pending; other actions may be required") + ".");
        }
        private void CancelAll()
        {
            cancelRequested = true;
            if (executing != null) { StopCoroutine(executing); executing = null; }
            if (director && director.IsBusy) director.Cancel();
            Unsubscribe();
            recording = false;
            observedCircuit = null;
            observedBindings = null;
            observedLayoutRoot = null;
        }
    }
}
