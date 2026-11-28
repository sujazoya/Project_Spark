using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProjectSpark.Gameplay;
using ProjectSpark.Measurement;
using ProjectSpark.UIManagement;
using UnityEngine.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace ProjectSpark.SparkV5.Pro.Editor
{
    [InitializeOnLoad]
    internal static class SparkNexusEditor
    {
        private const string Menu = "Project Spark/NEXUS/";
        private const string MainMenu = "Assets/My_Assets/_Project_Spark/Scenes/MainMenu/MainMenu.unity";
        private const string Prefab = "Assets/ProjectSpark/SparkV5/Pro/Integration/Nexus/Resources/SparkNexusPanel.prefab";
        private const string Docs = "Docs/Nexus";
        private const string Request = "Logs/Nexus/editor.request";
        private const string Running = "Spark.Nexus.PlayVerification";

        [Serializable]
        private sealed class CampaignRow
        {
            public int level;
            public string levelId, layout, bindings, reviewedPlan, error;
            public int electrical, ports, terminals, meters;
        }

        [Serializable] private sealed class CampaignAudit { public CampaignRow[] levels; }
        [Serializable] private sealed class CommandExport { public CommandRow[] commands; }
        [Serializable] private sealed class CommandRow
        {
            public string id, category, phrase, route, parameter, mode, approval, evidence;
            public string[] aliases, related;
        }

        static SparkNexusEditor()
        {
            EditorApplication.update += Poll;
            EditorApplication.playModeStateChanged += PlayChanged;
        }

        [MenuItem(Menu + "Audit Native Integration")]
        internal static void AuditNativeIntegration() => WithMainMenu(scene =>
        {
            string report = Validate(scene);
            Write("Scene-Validation.md", report);
            Debug.Log(report);
        });

        [MenuItem(Menu + "Install/Upgrade Panel")]
        private static void Install()
        {
            var asset = AssetDatabase.LoadAssetAtPath<SparkNexusPanel>(Prefab);
            if (!asset || !asset.GetComponent<RectTransform>())
            { Debug.LogError("NEXUS resource prefab or root RectTransform is missing."); return; }
            AssetDatabase.ImportAsset(Prefab, ImportAssetOptions.ForceUpdate);
            BindExistingSparkLive();
        }

        [MenuItem(Menu + "Bind Existing Spark Live")]
        private static void BindExistingSparkLive()
        {
            // Spark Live installs the NEXUS panel on the same workshop UI manager at runtime.
            // Validate the unique authored route rather than adding a second panel.
            string report = Validate(SceneManager.GetActiveScene());
            Write("Selected-Scene-Validation.md", report);
            Debug.Log(report);
        }

        [MenuItem(Menu + "Validate Selected Scene")]
        private static void ValidateSelected() => BindExistingSparkLive();

        [MenuItem(Menu + "Audit Campaign")]
        internal static void AuditCampaign()
        {
            WithMainMenu(scene =>
            {
                var manager = InScene<LevelGamePlayManager>(scene).Single();
                if (!manager.LayoutLoader) throw new InvalidOperationException("Campaign layout loader missing.");
                var plans = Resources.LoadAll<SparkNativeWiringPlan>("SparkNativeWirePlans");
                var rows = new List<CampaignRow>();
                foreach (var index in manager.GetCampaignLevelIndices())
                {
                    var definition = manager.GetLevel(index);
                    if (!definition) { rows.Add(new CampaignRow { error = "Missing level definition" }); continue; }
                    var row = new CampaignRow { level = definition.LevelNumber, levelId = definition.LevelId };
                    rows.Add(row);
                    try
                    {
                        var layout = manager.LayoutLoader.GetAuthoredPrefab(definition);
                        row.layout = layout ? layout.name : "Unavailable";
                        if (!layout || layout.LevelDefinition != definition)
                        { row.error = "Missing layout or mismatched definition"; continue; }
                        if (!layout.SceneBindings) row.bindings = "Missing";
                        else row.bindings = layout.SceneBindings.Validate(out string error) ? "Valid" : error;
                        row.electrical = layout.GetComponentsInChildren<SparkElectricalComponent>(true).Length;
                        row.ports = layout.GetComponentsInChildren<SparkEngineeringDevicePort>(true).Length;
                        row.terminals = layout.SceneBindings ? layout.SceneBindings.TerminalBindingCount : 0;
                        row.meters = layout.GetComponentsInChildren<SparkMultimeter>(true).Length;
                        var available = plans.Where(plan => plan.IsApproved && plan.LevelNumber == definition.LevelNumber &&
                            plan.LayoutName == layout.name).ToArray();
                        if (available.Length != 1)
                            row.reviewedPlan = available.Length == 0 ? "No approved plan" : "Ambiguous approval";
                        else
                        {
                            var plan = available[0];
                            bool matches = plan.Validate(out string reason) && plan.MatchesReviewedLayout(definition, layout, out reason);
                            bool unchanged = matches && SparkNativeWiringPlan.AuthoredRevision(definition, layout, plan.SchematicReference)
                                == plan.ReviewedRevision;
                            row.reviewedPlan = unchanged ? "Approved current revision" : reason ?? "Reviewed revision changed";
                        }
                    }
                    catch (Exception error) { row.error = error.Message; }
                }
                var ordered = rows.OrderBy(row => row.level).ToArray();
                var missing = Enumerable.Range(1, 130).Except(ordered.Select(row => row.level)).ToArray();
                var duplicates = ordered.GroupBy(row => row.level).Where(group => group.Count() > 1).Select(group => group.Key).ToArray();
                var report = new StringBuilder("# NEXUS authored campaign audit\n\n");
                report.AppendLine($"Audited {ordered.Length} campaign entries from the native registry. This is an asset audit.");
                report.AppendLine($"\nMissing 1–130: {(missing.Length == 0 ? "none" : string.Join(", ", missing))}. Duplicate numbers: {(duplicates.Length == 0 ? "none" : string.Join(", ", duplicates))}.\n");
                report.AppendLine("| Level | Layout | Bindings | Electrical | Terminals | Device ports | Meters | Auto Draw | Audit issue |");
                report.AppendLine("| --- | --- | --- | --- | --- | --- | --- | --- | --- |");
                foreach (var row in ordered)
                    report.AppendLine($"| {row.level} | {Cell(row.layout)} | {Cell(row.bindings)} | {row.electrical} | {row.terminals} | {row.ports} | {row.meters} | {Cell(row.reviewedPlan)} | {Cell(row.error)} |");
                Write("Campaign-Audit.md", report.ToString());
                Write("Campaign-Audit.json", JsonUtility.ToJson(new CampaignAudit { levels = ordered }, true));
                Debug.Log($"NEXUS campaign audit exported for {ordered.Length} entries.");
            });
        }

        [MenuItem(Menu + "Export Report")]
        internal static void ExportReport()
        {
            AuditNativeIntegration();
            AuditCampaign();
            var commands = SparkNexusCatalog.Native().Select(command => new CommandRow
            {
                id = command.Id, category = command.Category, phrase = command.Phrase, route = command.Capability.ToString(),
                parameter = command.Parameter.ToString(), mode = command.Mode.ToString(), approval = command.Approval.ToString(),
                evidence = command.Evidence.ToString(), aliases = command.Aliases.ToArray(), related = command.Related.ToArray()
            }).ToArray();
            var report = new StringBuilder("# NEXUS command registry\n\n");
            report.AppendLine("Availability is determined from the live handler and workbench. Spark Live continues to route free-form questions.\n");
            report.AppendLine("| ID | Domain | Phrase | Route | Mode | Approval | Evidence |");
            report.AppendLine("| --- | --- | --- | --- | --- | --- | --- |");
            foreach (var command in commands)
                report.AppendLine($"| {Cell(command.id)} | {Cell(command.category)} | {Cell(command.phrase)} | {Cell(command.route)} | {Cell(command.mode)} | {Cell(command.approval)} | {Cell(command.evidence)} |");
            Write("Command-Registry.md", report.ToString());
            Write("Command-Registry.json", JsonUtility.ToJson(new CommandExport { commands = commands }, true));
        }

        [MenuItem(Menu + "Run Tests")]
        internal static void RunTests() => QueuePlayTests(false);

        [MenuItem(Menu + "Test Embedded Panel and LED")]
        private static void RunEmbeddedTests() => QueuePlayTests(true);

        private static void QueuePlayTests(bool embeddedOnly)
        {
            if (EditorApplication.isCompiling || EditorApplication.isPlayingOrWillChangePlaymode)
            { Debug.LogWarning("NEXUS tests need Edit Mode after compilation."); return; }
            Write("Test-Results.md", "# NEXUS verification\n\n");
            try
            {
                RunEditChecks();
                if (SceneManager.GetActiveScene().path != MainMenu)
                { Log("NOT RUN Play Mode. Open MainMenu and run NEXUS > Run Tests."); return; }
                if (SceneManager.GetActiveScene().isDirty)
                    Log("Play Mode uses the already-open MainMenu with unsaved edits. No scene is saved.");
                SessionState.SetBool(Running + ".Embedded", embeddedOnly);
                SessionState.SetBool(Running, true);
                EditorApplication.EnterPlaymode();
            }
            catch (Exception error) { Log("FAIL " + error); Debug.LogException(error); }
        }

        private static void RunEditChecks()
        {
            var registry = new SparkNexusCatalog();
            var entries = SparkNexusCatalog.Native().ToArray();
            registry.Register("live", entries);
            Check(registry.Commands.Count() == entries.Length, "Exact catalog entry count.");
            Check(entries.Where(entry => entry.Parameter != NexusParameter.None)
                .All(entry => entry.Phrase.Contains("{value}") && !entry.Format("PowerSupply_01").Contains("{value}")),
                "Typed parameter insertion.");
            Check(entries.SelectMany(entry => entry.Related).All(id => entries.Any(entry => entry.Id == id)),
                "Related command IDs exist.");
            var device = entries.Single(entry => entry.Id == "device.on");
            Check(device.Format("PowerSupply_01") == "spark propose device PowerSupply_01 on", "Exact device ID case.");
            bool duplicate = false;
            try { registry.Register("collision", entries.Take(1)); }
            catch (InvalidOperationException) { duplicate = true; }
            Check(duplicate && registry.Commands.Count() == entries.Length, "Atomic collision validation.");
            registry.Unregister("live");
            Check(!registry.Commands.Any(), "Dynamic provider removal.");
            var prefab = AssetDatabase.LoadAssetAtPath<SparkNexusPanel>(Prefab);
            Check(prefab && prefab.GetComponent<RectTransform>(), "NEXUS prefab imported.");
            var material = Resources.Load<Material>("SparkNexusGlass");
            Check(material && material.shader && material.shader.name == "Project Spark/NEXUS/Cyan Glass"
                && !ShaderUtil.ShaderHasError(material.shader), "NEXUS shader and material imported.");
            Log($"PASS Edit Mode: {entries.Length} unique command descriptors, typed IDs, related links, collisions, provider removal, prefab and shader.");
        }

        private static async void PlayChanged(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.EnteredPlayMode || !SessionState.GetBool(Running, false)) return;
            try { await VerifyPlayMode(SessionState.GetBool(Running + ".Embedded", false)); }
            catch (Exception error) { Log("FAIL " + error); Debug.LogException(error); }
            finally { SessionState.EraseBool(Running); SessionState.EraseBool(Running + ".Embedded"); EditorApplication.ExitPlaymode(); }
        }

        private static async Task VerifyPlayMode(bool embeddedOnly)
        {
            var manager = Object.FindObjectsByType<LevelGamePlayManager>(FindObjectsInactive.Include, FindObjectsSortMode.None).Single();
            SparkProExecutive spark = null;
            await Wait(() =>
            {
                spark = Object.FindObjectsByType<SparkProExecutive>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                    .SingleOrDefault(candidate => candidate.isActiveAndEnabled && candidate.ActiveLevelManager == manager);
                return spark;
            }, "runtime Spark Live binding");
            spark.SetSpeakAnswers(false);
            if (manager.LevelProgression) manager.LevelProgression.enabled = false;
            var settings = new SerializedObject(manager);
            settings.FindProperty("enforceProgressionAccess").boolValue = false;
            settings.FindProperty("autoAdvanceOnCompletion").boolValue = false;
            settings.ApplyModifiedPropertiesWithoutUndo();
            var hub = spark.GetComponent<SparkEngineeringControlHub>();
            Check(hub, "Native engineering owner found.");
            await Wait(() => Object.FindObjectsByType<SparkNexusPanel>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .Any(panel => panel.BoundExecutive == spark), "NEXUS launcher integration");
            var panel = Object.FindObjectsByType<SparkNexusPanel>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .Single(item => item.BoundExecutive == spark);
            var live = panel.GetComponentInParent<SparkProLivePanel>(true);
            var ui = Object.FindObjectsByType<SparkUIManager>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .Single(candidate => candidate.Find(SparkProLivePanel.PanelId));
            Check(live && live.Nexus == panel && live.TryGetComponent<SparkLiveAdjustableWindow>(out _),
                "NEXUS uses the original Spark Live panel and its window controller.");
            Check(ui.Open(SparkProLivePanel.PanelId), "Open the original Spark Live panel.");
            var tabs = live.GetComponentsInChildren<Button>(true);
            var nexusTab = tabs.Single(item => item.name == "NEXUS tab");
            var chatTab = tabs.Single(item => item.name == "Chat tab");
            nexusTab.onClick.Invoke();
            Check(panel.Visible && panel.GetComponentsInChildren<TMP_InputField>().Length == 1, "NEXUS tab shows the embedded console.");
            Check(panel.GetComponentsInChildren<Canvas>(true).Length == 0 &&
                Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None).Length == 1, "Shared canvas and single event system.");
            var console = live.transform.Find("Console") as RectTransform;
            Check(console && panel.transform.IsChildOf(console) && console.Find("AAAResize7") &&
                console.Find("Header").GetComponent<SparkLiveWindowPointer>(),
                "NEXUS is inside the existing draggable and resizable shell.");
            await VerifyFloatingPanel(live, panel, ui, console, nexusTab);
            chatTab.onClick.Invoke();
            Check(!panel.Visible && live.Visible, "Chat tab restores the existing Spark Live page.");
            Log("PASS NEXUS tab opens inside Spark Live, uses the shared Canvas, drag and resize shell, and switches back to Chat.");
            using var context = new SparkNexusContext(spark);
            var workflow = new SparkNexusWorkflow(spark, context);
            var commands = SparkNexusCatalog.Native().ToDictionary(command => command.Id);
            int Slot(int number) => manager.GetCampaignLevelIndices().Single(index => manager.GetLevel(index).LevelNumber == number);
            if (embeddedOnly)
            {
                await VerifyStarterLed(manager, spark, workflow, context, commands["wire.draw"], Slot(1));
                ui.Open(SparkProLivePanel.PanelId);
                nexusTab.onClick.Invoke();
                await Task.Delay(350);
                ScreenCapture.CaptureScreenshot(Docs + "/NEXUS-Workbench.png");
                await Task.Delay(350);
                ui.Close(SparkProLivePanel.PanelId);
                return;
            }
            Check(manager.StartLevel(Slot(20)), "Open Level 20.");
            await Wait(() => manager.ActiveLevel && manager.ActiveLevel.LevelNumber == 20 && manager.LayoutLoader.ActiveLayout,
                "Level 20 layout and registration");
            context.Tick();
            Check(context.Snapshot.Layout && context.Snapshot.Level.LevelNumber == 20, "Level 20 context.");
            Check(context.Snapshot.Targets.Any(target => target.NativeId == "SwitchOutput"), "Authored terminal discovered.");
            nexusTab.onClick.Invoke();
            await Task.Delay(350);
            ScreenCapture.CaptureScreenshot(Docs + "/NEXUS-Workbench.png");
            await Task.Delay(350);
            ui.Close(SparkProLivePanel.PanelId);
            var available = Resources.LoadAll<SparkNativeWiringPlan>("SparkNativeWirePlans");
            var missing = manager.GetCampaignLevelIndices().Select(manager.GetLevel).First(definition => definition &&
                definition.LevelNumber > 20 && !available.Any(asset => asset.IsApproved && asset.LevelNumber == definition.LevelNumber));
            var oldLayout = context.Snapshot.Layout;
            Check(manager.StartLevel(Slot(missing.LevelNumber)), "Start a level without an approved plan.");
            await Wait(() => { context.Tick(); return context.Snapshot.Level == missing && context.Snapshot.Layout != oldLayout; },
                "same-scene context transition");
            workflow.Start(commands["wire.draw"], null);
            Check(workflow.Stage == NexusStage.Failed && !hub.IsExecuting, "Unapproved level did not dispatch construction.");
            Log($"PASS same-scene transition and unapproved Level {missing.LevelNumber} blocked before dispatch.");
            Check(manager.StartLevel(Slot(20)), "Return to reviewed Level 20.");
            await Wait(() => { context.Tick(); return context.Snapshot.Level && context.Snapshot.Level.LevelNumber == 20 &&
                    context.Snapshot.Targets.Any(target => target.NativeId == "SwitchOutput"); },
                "Level 20 context rebuilt after same-scene transition");
            int count = spark.History.Count(turn => turn.Speaker == "You");
            workflow.Start(null, "What is the current mission?");
            Check(spark.History.Count(turn => turn.Speaker == "You") == count + 1, "Shared Ask submitted exactly once.");
            void BlockModeRequest()
            {
                if (workflow.Stage == NexusStage.AwaitingModeAck) spark.SetExaminationMode(true);
            }
            spark.Ask("spark manual mode");
            workflow.Changed += BlockModeRequest;
            try
            {
                workflow.Start(commands["wire.suggest"], null);
                await Wait(() => { context.Tick(); workflow.Tick(); return !workflow.Busy; }, "rejected mode acknowledgement", 8f);
                Check(workflow.Stage == NexusStage.TimedOut && !hub.IsExecuting, "No command dispatched after missing mode acknowledgement.");
            }
            finally { workflow.Changed -= BlockModeRequest; spark.SetExaminationMode(false); }
            workflow.Start(commands["wire.suggest"], null);
            spark.Ask("spark manual mode");
            context.Tick(); workflow.Tick();
            Check(workflow.Stage == NexusStage.Failed && !hub.IsExecuting, "Manual override cancelled automatic dispatch.");
            workflow.Start(commands["wire.suggest"], null);
            context.Tick(); workflow.Tick();
            Check(hub.Mode == SparkEngineeringControlHub.OperatingMode.Assisted, "Assisted mode was acknowledged.");
            Check(!hub.IsExecuting, "Suggesting a wire did not approve it.");
            await SparkFlightLevel20Verification.InstallBridgeResistors(manager.LayoutLoader.ActiveLayout.transform, Log);
            var plan = hub.GetNexusConstructionPlan(out string reason);
            Check(plan, reason);
            await Wait(() => plan.Wires.All(step => manager.SceneBindings.TryGetTerminal(step.fromId, out _) &&
                manager.SceneBindings.TryGetTerminal(step.toId, out _)), "reviewed Level 20 native terminals");
            foreach (var supply in manager.LayoutLoader.ActiveLayout.GetComponentsInChildren<SparkPowerSupply>())
                if (supply.IsOutputActive)
                    supply.SetOutput(false, new SparkInteractionContext(spark, supply, SparkInteractionType.Toggle,
                        SparkToolType.Select, supply.transform.position, Vector3.up, default, supply.gameObject));
            context.Tick();
            workflow.Start(commands["wire.draw"], null);
            Check(workflow.Stage == NexusStage.AwaitingApproval && !hub.IsExecuting, workflow.Detail);
            workflow.Cancel();
            Check(workflow.Stage == NexusStage.Cancelled && !hub.IsExecuting, "Rejected approval made no dispatch.");
            workflow.Start(commands["wire.draw"], null);
            workflow.Confirm();
            context.Tick(); workflow.Tick();
            Check(hub.Mode == SparkEngineeringControlHub.OperatingMode.Auto && workflow.Stage == NexusStage.Running,
                workflow.Detail + " / " + hub.LastMessage);
            await Wait(() => { context.Tick(); workflow.Tick(); return !workflow.Busy; }, "native Level 20 construction", 165f);
            Check(workflow.Stage == NexusStage.Succeeded, workflow.Detail);
            Check(plan.Wires.All(step => manager.SceneBindings.TryGetTerminal(step.fromId, out var a) &&
                manager.SceneBindings.TryGetTerminal(step.toId, out var b) && manager.CircuitSystem.HasConnectionBetween(a, b)),
                "Native circuit contains each reviewed connection.");
            Log("PASS Level 20: original Ask, live NEXUS panel, single input system, mode acknowledgement and timeout, manual override, approval rejection, approved Auto Draw, and reviewed native connections.");

            var layout = manager.LayoutLoader.ActiveLayout;
            var powerSwitch = layout.GetComponentsInChildren<SparkEngineeringDevicePort>()
                .Single(port => port.DeviceId == "Power_Switch");
            var supplyPort = layout.GetComponentsInChildren<SparkEngineeringDevicePort>()
                .Single(port => port.DeviceId == "PowerSupply_01");
            async Task Operate(string deviceId, bool turnOn)
            {
                workflow.Start(commands[turnOn ? "device.on" : "device.off"], deviceId);
                context.Tick(); workflow.Tick();
                Check(workflow.Stage == NexusStage.Succeeded, workflow.Detail);
                workflow.Start(commands["device.approve"], null);
                Check(workflow.Stage == NexusStage.AwaitingApproval, workflow.Detail);
                workflow.Confirm();
                await Wait(() => { context.Tick(); workflow.Tick(); return !workflow.Busy; }, "verified device operation " + deviceId, 100f);
                Check(workflow.Stage == NexusStage.Succeeded, workflow.Detail);
            }
            if (!powerSwitch.Switch.IsClosed) await Operate(powerSwitch.DeviceId, true);
            await Wait(() => manager.HasCurrentElectricalEvaluation, "fresh evaluation before supply ON");
            await Operate(supplyPort.DeviceId, true);
            await Wait(() => manager.IsCompleted && manager.ActiveTargetCount == 5 && manager.SatisfiedTargetCount == 5,
                "Level 20 native evaluator 5/5");
            Log("PASS Level 20: NEXUS supply approval, fresh solver verification and all 5/5 native objectives.");
            var resistor = layout.GetComponentsInChildren<SparkResistor>()
                .First(item => Mathf.Abs(item.ElectricalState.Current) > 0.0001f);
            context.Focus(resistor); context.Tick();
            Check(context.Snapshot.Selected == resistor && context.Snapshot.SolveRevision == manager.ElectricalSolver.CompletedSolveRevision &&
                context.Snapshot.Telemetry.Contains($"{resistor.ElectricalState.Current:G5} A", StringComparison.Ordinal),
                "NEXUS indicator matches native solved resistor current.");
            Log("PASS Level 20 live resistor current matches the native solver.");
            await Operate(supplyPort.DeviceId, false);
            await VerifyStarterLed(manager, spark, workflow, context, commands["wire.draw"], Slot(1));
        }

        private static async Task VerifyFloatingPanel(SparkProLivePanel live, SparkNexusPanel panel,
            SparkUIManager ui, RectTransform dockedConsole, Button nexusTab)
        {
            var fixturePreferences = SparkLiveAdjustableWindow.FloatingPreferences;
            var numbers = new Dictionary<string, float>
            {
                ["x"] = .36f, ["y"] = .64f, ["width"] = 545f, ["height"] = 675f, ["scale"] = .92f, ["opacity"] = 1f
            };
            var toggles = new Dictionary<string, int> { ["collapsed"] = 0, ["snap"] = 0, ["locked"] = 0 };
            var savedNumbers = numbers.Keys.ToDictionary(key => key, key => PlayerPrefs.HasKey(fixturePreferences + key)
                ? (float?)PlayerPrefs.GetFloat(fixturePreferences + key) : null);
            var savedToggles = toggles.Keys.ToDictionary(key => key, key => PlayerPrefs.HasKey(fixturePreferences + key)
                ? (int?)PlayerPrefs.GetInt(fixturePreferences + key) : null);
            try
            {
                foreach (var entry in numbers) PlayerPrefs.SetFloat(fixturePreferences + entry.Key, entry.Value);
                foreach (var entry in toggles) PlayerPrefs.SetInt(fixturePreferences + entry.Key, entry.Value);

                static bool InsideSafeArea(RectTransform window)
                {
                    Rect safe = Screen.safeArea;
                    var corners = new Vector3[4];
                    window.GetWorldCorners(corners);
                    return corners.All(point =>
                    {
                        Vector2 screen = RectTransformUtility.WorldToScreenPoint(null, point);
                        return screen.x >= safe.xMin - 2 && screen.x <= safe.xMax + 2 &&
                            screen.y >= safe.yMin - 2 && screen.y <= safe.yMax + 2;
                    });
                }

                var detach = live.GetComponentsInChildren<Button>().Single(item => item.name == "Detach NEXUS");
                await Wait(() => InsideSafeArea(dockedConsole), "docked window fits the screen safe area", 4f);
                await Task.Delay(260);
                ScreenCapture.CaptureScreenshot(Docs + "/NEXUS-Docked.png");
                await Task.Delay(260);
                Vector2 originalSize = dockedConsole.sizeDelta;
                Vector2 originalPosition = dockedConsole.anchoredPosition;
                detach.onClick.Invoke();
                Check(live.NexusDetached && panel.Visible && !panel.transform.IsChildOf(dockedConsole),
                    "Detach moved the existing console into its independent floating window.");
                var floating = panel.GetComponentInParent<SparkLiveAdjustableWindow>();
                Check(floating && floating.gameObject != live.gameObject, "Independent layout controller.");
                floating.PersistLayout = false;
                var floatingConsole = (RectTransform)floating.transform.Find("Console");
                await Wait(() => InsideSafeArea(floatingConsole), "floating window fits the screen safe area", 4f);
                var controls = floating.GetComponentsInChildren<Button>(true);
                var dock = controls.Single(control => control.name == "Dock NEXUS");
                var hide = controls.Single(control => control.name == "Hide NEXUS");
                hide.onClick.Invoke();
                Check(!panel.Visible && live.NexusDetached, "Hide preserves the floating state.");
                nexusTab.onClick.Invoke();
                Check(panel.Visible, "Spark Live can reopen the floating window.");
                ui.Close(SparkProLivePanel.PanelId);
                Check(panel.Visible && !live.Visible, "Floating NEXUS stays open after closing Spark Live.");

                Vector2 position = floatingConsole.anchoredPosition;
                Vector2 size = floatingConsole.sizeDelta;
                floating.Move(new Vector2(70, -35));
                await Wait(() => (floatingConsole.anchoredPosition - position).sqrMagnitude > 25f, "floating window move", 4f);
                floating.Resize(new Vector2(140, -110), 1, -1);
                await Wait(() => floatingConsole.sizeDelta.x > size.x + 10f && floatingConsole.sizeDelta.y > size.y + 10f,
                    "floating window resize", 4f);
                await Task.Delay(280);
                await Wait(() => InsideSafeArea(floatingConsole), "resized window fits the screen safe area", 4f);
                ScreenCapture.CaptureScreenshot(Docs + "/NEXUS-Floating.png");
                await Task.Delay(280);

                dock.onClick.Invoke();
                Check(panel.Visible && live.Visible && !live.NexusDetached && panel.transform.IsChildOf(dockedConsole),
                    "Dock reopened Spark Live and restored the same console.");
                Check(live.Nexus == panel && Object.FindObjectsByType<SparkNexusPanel>(FindObjectsInactive.Include,
                    FindObjectsSortMode.None).Count(item => item.BoundExecutive == panel.BoundExecutive) == 1,
                    "Only one NEXUS instance across detach and dock.");
                Check((dockedConsole.sizeDelta - originalSize).sqrMagnitude < 1f &&
                    (dockedConsole.anchoredPosition - originalPosition).sqrMagnitude < 1f,
                    "Floating changes preserve the Spark Live layout.");
                Log("PASS NEXUS detached, stayed open independently, hid and reopened, moved, resized, stayed inside the screen safe area, and docked without changing Spark Live bounds.");
            }
            finally
            {
                foreach (var entry in savedNumbers)
                    if (entry.Value.HasValue) PlayerPrefs.SetFloat(fixturePreferences + entry.Key, entry.Value.Value);
                    else PlayerPrefs.DeleteKey(fixturePreferences + entry.Key);
                foreach (var entry in savedToggles)
                    if (entry.Value.HasValue) PlayerPrefs.SetInt(fixturePreferences + entry.Key, entry.Value.Value);
                    else PlayerPrefs.DeleteKey(fixturePreferences + entry.Key);
            }
        }

        private static async Task VerifyStarterLed(LevelGamePlayManager manager, SparkProExecutive spark,
            SparkNexusWorkflow workflow, SparkNexusContext context, NexusCommand draw, int slot)
        {
            Check(manager.StartLevel(slot), "Open authored Level 1.");
            await Wait(() =>
            {
                context.Tick();
                return context.Snapshot.Level && context.Snapshot.Level.LevelNumber == 1 && manager.HasCurrentElectricalEvaluation;
            }, "Level 1 solver and bindings");
            var layout = manager.LayoutLoader.ActiveLayout;
            var supply = layout.GetComponentsInChildren<SparkPowerSupply>().Single();
            var led = layout.GetComponentsInChildren<SparkLED>().Single();
            var interaction = new SparkInteractionContext(spark, supply, SparkInteractionType.Toggle,
                SparkToolType.Select, supply.transform.position, Vector3.up, default, supply.gameObject);
            if (supply.IsOutputActive) Check(supply.SetOutput(false, interaction).Succeeded, "Isolate Level 1 power.");
            context.Tick();
            workflow.Start(draw, null);
            Check(workflow.Stage == NexusStage.AwaitingApproval, workflow.Detail);
            workflow.Confirm();
            await Wait(() => { context.Tick(); workflow.Tick(); return !workflow.Busy; }, "Level 1 approved wiring", 105f);
            Check(workflow.Stage == NexusStage.Succeeded, workflow.Detail);
            Check(supply.SetOutput(true, interaction).Succeeded, "Enable Level 1 power through the native player control.");
            await Wait(() => manager.IsCompleted && manager.SatisfiedTargetCount == 1 && manager.HasCurrentElectricalEvaluation &&
                led.IsOn && Mathf.Abs(led.ElectricalState.Current) > 0.0001f, "Level 1 LED and native objective");
            context.Focus(led); context.Tick();
            Check(context.Snapshot.Selected == led && context.Snapshot.SolveRevision == manager.ElectricalSolver.CompletedSolveRevision &&
                context.Snapshot.Telemetry.Contains("LED conducting", StringComparison.Ordinal) &&
                context.Snapshot.Telemetry.Contains($"{led.ElectricalState.Current:G5} A", StringComparison.Ordinal) &&
                context.Snapshot.Telemetry.Contains($"{led.ElectricalState.Voltage:G5} V", StringComparison.Ordinal),
                "NEXUS LED indicator matches native solved current and voltage.");
            Log("PASS Level 1: reviewed wiring, 1/1 objective, and NEXUS LED voltage and current match the fresh native solver.");
        }

        private static async Task Wait(Func<bool> condition, string status, float seconds = 30f)
        {
            float end = Time.realtimeSinceStartup + seconds;
            while (EditorApplication.isPlaying && Time.realtimeSinceStartup < end)
            {
                if (condition()) return;
                await Task.Delay(50);
            }
            throw new TimeoutException(status);
        }

        private static string Validate(Scene scene)
        {
            var executives = InScene<SparkProExecutive>(scene);
            var managers = InScene<LevelGamePlayManager>(scene);
            var inputs = InScene<EventSystem>(scene);
            int missing = scene.IsValid() ? scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<Transform>(true))
                .Sum(item => GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(item.gameObject)) : 0;
            var prefab = AssetDatabase.LoadAssetAtPath<SparkNexusPanel>(Prefab);
            int duplicates = InScene<SparkNexusPanel>(scene).Length;
            return "# NEXUS scene validation\n\n" +
                $"- Scene: {scene.path}\n- Authored Spark Live executives: {executives.Length}\n" +
                $"- Campaign managers: {managers.Length}\n- Authored EventSystems: {inputs.Length}\n- Missing script components: {missing}\n" +
                $"- Authored NEXUS panels: {duplicates}\n- Runtime prefab: {(prefab ? "Imported" : "Missing")}\n" +
                "- MainMenu installs the NEXUS tab inside the adjustable Spark Live panel through its runtime UI bindings. See Test-Results.md for runtime evidence.\n";
        }

        private static T[] InScene<T>(Scene scene) where T : Component => scene.IsValid() && scene.isLoaded
            ? scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<T>(true)).ToArray()
            : Array.Empty<T>();

        private static void WithMainMenu(Action<Scene> action)
        {
            var scene = SceneManager.GetSceneByPath(MainMenu);
            bool preview = !scene.IsValid() || !scene.isLoaded;
            if (preview) scene = EditorSceneManager.OpenPreviewScene(MainMenu);
            try { action(scene); }
            finally { if (preview) EditorSceneManager.ClosePreviewScene(scene); }
        }

        private static void Poll()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating ||
                EditorApplication.isPlayingOrWillChangePlaymode || !File.Exists(Request)) return;
            string instruction = File.ReadAllText(Request).Trim();
            if (instruction != "audit" && instruction != "test" && instruction != "embedded") return;
            File.Delete(Request);
            try
            {
                ExportReport();
                                if (instruction == "test") RunTests();
                else if (instruction == "embedded") RunEmbeddedTests();
            }
            catch (Exception error) { Log("FAIL " + error); Debug.LogException(error); }
        }

        private static string Cell(string value) => (value ?? "").Replace("|", "\\|").Replace("\r", " ").Replace("\n", " ");
        private static void Check(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
        private static void Write(string name, string content)
        {
            Directory.CreateDirectory(Docs);
            File.WriteAllText(Path.Combine(Docs, name), content);
        }
        private static void Log(string line)
        {
            Directory.CreateDirectory(Docs);
            File.AppendAllText(Path.Combine(Docs, "Test-Results.md"), line + "\n\n");
        }
    }
}
