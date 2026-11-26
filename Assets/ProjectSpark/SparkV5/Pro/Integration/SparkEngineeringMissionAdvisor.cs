using System;
using System.IO;
using System.Text;
using ProjectSpark.AI.Tutorial;
using ProjectSpark.Electrical;
using ProjectSpark.Gameplay;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

namespace ProjectSpark.SparkV5.Pro
{
    /// <summary>
    /// Read-only inspection of actual level, native connection graph and approved plan.
    /// No wiring, power toggling, approval or mission progress is performed here.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkEngineeringMissionAdvisor : MonoBehaviour
    {
        [SerializeField] private bool includeEveryPlanStep = true;

        private SparkProExecutive executive;
        private SparkEngineeringControlHub modeHub;
        private SparkNativeWireDirector director;
        private SparkAINativeWirePort nativePort;
        private SparkV5FlightController flight;
        private NavMeshAgent groundAgent;

        public event Action ReportChanged;
        public string LastReport { get; private set; } = "Select INSPECT to examine the current level without modifying it.";
        public string LastExportPath { get; private set; } = string.Empty;

        private void Awake()
        {
            executive = GetComponent<SparkProExecutive>();
            modeHub = GetComponent<SparkEngineeringControlHub>();
            director = GetComponent<SparkNativeWireDirector>();
            nativePort = GetComponent<SparkAINativeWirePort>();
            flight = GetComponent<SparkV5FlightController>();
            groundAgent = GetComponent<NavMeshAgent>();
        }

        private void OnEnable()
        {
            SceneManager.activeSceneChanged += OnSceneChanged;
        }

        private void OnDisable()
        {
            SceneManager.activeSceneChanged -= OnSceneChanged;
        }

        private void OnSceneChanged(Scene previous, Scene next)
        {
            // Scene context may not be initialized immediately following activeSceneChanged.
            LastReport = "Scene changed. Inspect after the level finishes loading.";
            ReportChanged?.Invoke();
        }

        private static string Key(string name)
        {
            return (name ?? string.Empty).Replace("(Clone)", string.Empty).Trim();
        }

        public string Inspect()
        {
            // This is deliberately on-demand. No per-frame scene searches or expensive scans.
            var report = new StringBuilder(2048);
            report.AppendLine("SPARK ENGINEERING MISSION ADVISOR v15")
                .AppendLine("Read-only inspection. No circuit, component, plan or mission changes.")
                .Append("Mode: ").Append(modeHub ? modeHub.Mode.ToString() : "v14 hub absent")
                .Append(" | Native director: ").Append(director ? "bound" : "MISSING")
                .Append(" | Player-native port: ")
                .Append(nativePort && nativePort.IsConfigured ? "configured" : "MISSING / unconfigured")
                .AppendLine();

            report.Append("Movement: NavMesh=");
            report.Append(groundAgent && groundAgent.isActiveAndEnabled && groundAgent.isOnNavMesh
                ? "on mesh" : "not currently on mesh");
            report.Append(" | Flight=").Append(flight ? flight.FlightStatus : "not bound").AppendLine();

            LevelGamePlayManager manager = executive ? executive.ActiveLevelManager : null;
            if (!manager)
            {
                report.AppendLine("BLOCKED: Active LevelGamePlayManager is missing.");
                return Complete(report);
            }
            SparkAILevelContext context = SparkAILevelContext.Capture(manager);
            if (!context.Definition || !context.Layout || !context.Layout.SceneBindings || !manager.CircuitSystem)
            {
                report.AppendLine("BLOCKED: Active authored level, scene bindings or circuit is missing.");
                return Complete(report);
            }

            report.Append("Level ").Append(context.Definition.LevelNumber)
                .Append(" | Layout: ").Append(Key(context.Layout.name))
                .Append(" | Active: ").Append(context.Layout.gameObject.activeInHierarchy)
                .AppendLine();
            report.Append("Evaluator: ")
                .Append(manager.IsFailed ? "FAILED" : manager.IsCompleted ? "COMPLETED" : "in progress")
                .Append(" | Independent exam: ").Append(executive.ExamMode ? "YES" : "no")
                .AppendLine();
            report.AppendLine(context.StateText());
            foreach (var objective in context.Definition.Targets)
            {
                if (objective == null) continue;
                bool? satisfied = context.Status(objective);
                report.Append(" • ").Append(SparkAILevelContext.TargetName(objective))
                    .Append(satisfied == true ? " [passed]" : satisfied == false ? " [pending]" : " [unverified]")
                    .AppendLine();
            }

            SparkPowerSupply[] supplies = context.Layout.GetComponentsInChildren<SparkPowerSupply>(true);
            int activeSupplies = 0;
            for (int i = 0; i < supplies.Length; i++)
                if (supplies[i] && supplies[i].IsOutputActive) activeSupplies++;
            report.Append("Virtual output: ")
                .Append(activeSupplies == 0 ? "OFF" : "ON (" + activeSupplies + " active supplies)")
                .AppendLine();

            if (executive.ExamMode || manager.IsFailed || activeSupplies > 0)
                report.AppendLine("SAFETY: Automatic physical wiring must not begin in this state.");
            if (executive.ExamMode)
            {
                report.AppendLine("Independent examination is active. Reviewed instructions are hidden.");
                return Complete(report);
            }
            if (director && director.IsBusy)
                report.AppendLine("STATUS: Native wire director is currently busy. No new wire action should start.");

            var plans = Resources.LoadAll<SparkNativeWiringPlan>("SparkNativeWirePlans");
            SparkNativeWiringPlan matched = null;
            int matches = 0;
            for (int i = 0; i < plans.Length; i++)
            {
                SparkNativeWiringPlan p = plans[i];
                if (p && p.IsApproved && p.LevelNumber == context.Definition.LevelNumber &&
                    string.Equals(p.LayoutName, Key(context.Layout.name), StringComparison.Ordinal))
                {
                    matches++;
                    matched = p;
                }
            }
            if (matches != 1)
            {
                report.Append("PLAN: ").Append(matches == 0
                    ? "No matching approved plan; no autonomous wiring should begin."
                    : "Ambiguous: multiple matching approved plans; no autonomous wiring should begin.")
                    .AppendLine();
                return Complete(report);
            }
            if (!matched.Validate(out string problem))
            {
                report.Append("PLAN INVALID: ").Append(problem).AppendLine();
                return Complete(report);
            }

            report.Append("Approved reviewed plan: ").Append(matched.name)
                .Append(" (wires=").Append(matched.Wires.Count).AppendLine(")");
            int verified = 0;
            int missing = 0;
            int invalid = 0;
            string firstMissing = null;
            for (int i = 0; i < matched.Wires.Count; i++)
            {
                var wire = matched.Wires[i];
                SparkTerminal a = null;
                SparkTerminal b = null;
                bool endpoints = false;
                if (wire != null)
                {
                    bool hasA = context.Layout.SceneBindings.TryGetTerminal(wire.fromId, out a);
                    bool hasB = context.Layout.SceneBindings.TryGetTerminal(wire.toId, out b);
                    endpoints = hasA && hasB;
                }
                bool valid = endpoints && a && b && a != b &&
                    a.gameObject.activeInHierarchy && b.gameObject.activeInHierarchy &&
                    a.transform.IsChildOf(context.Layout.transform) &&
                    b.transform.IsChildOf(context.Layout.transform) &&
                    !(a.Owner && a.Owner == b.Owner);
                string state;
                if (!valid) { state = "INVALID ENDPOINTS"; invalid++; }
                else if (manager.CircuitSystem.HasConnectionBetween(a, b))
                { state = "connected"; verified++; }
                else
                {
                    state = "missing"; missing++;
                    if (firstMissing == null) firstMissing = wire.fromId + " -> " + wire.toId;
                }
                if (includeEveryPlanStep)
                    report.Append(i + 1).Append(". ").Append(wire == null ? "<null>" : wire.fromId + " -> " + wire.toId)
                        .Append(" [").Append(state).AppendLine("]");
            }

            report.Append("Graph verification: ").Append(verified).Append(" connected / ")
                .Append(missing).Append(" missing / ").Append(invalid).AppendLine(" invalid.");
            if (invalid > 0)
                report.AppendLine("BLOCKED: Correct exact terminal bindings or approved plan before attempting AI wiring.");
            else if (firstMissing != null)
                report.Append("NEXT REVIEWED STEP (not auto-approved): ").AppendLine(firstMissing);
            else
                report.AppendLine("All reviewed wiring is present. Other objectives remain evaluator-controlled.");
            if (!groundAgent || !groundAgent.isOnNavMesh)
                report.AppendLine("MOVEMENT NOTE: Ground navigation is not presently attached to NavMesh; flight may be available, but physical approach must be verified during execution.");
            return Complete(report);
        }

        private string Complete(StringBuilder sb)
        {
            LastReport = sb.ToString();
            ReportChanged?.Invoke();
            Debug.Log("[Spark Mission Advisor v15] Inspection finished (no state changes).", this);
            return LastReport;
        }

        public string ExportLastReport()
        {
            // File output is explicit and initiated by the player/Editor, never by a scene scan.
            string folder = Path.Combine(Application.persistentDataPath, "SparkEngineeringAdvisor");
            try
            {
                Inspect();
                Directory.CreateDirectory(folder);
                string path = Path.Combine(folder,
                    "Mission_" + DateTime.UtcNow.ToString("yyyyMMdd_HHmmss_fff") + ".txt");
                File.WriteAllText(path, DateTime.UtcNow.ToString("o") + " UTC\n" + LastReport,
                    new UTF8Encoding(false));
                LastExportPath = path;
                ReportChanged?.Invoke();
                return path;
            }
            catch (Exception error)
            {
                return "Could not export mission report: " + error.Message;
            }
        }
    }
}
