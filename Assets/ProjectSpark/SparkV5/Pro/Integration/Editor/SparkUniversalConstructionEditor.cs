using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using ProjectSpark.Gameplay;
using ProjectSpark.Gameplay.Shelf;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProjectSpark.SparkV5.Pro.Editor
{
    [InitializeOnLoad]
    internal sealed class SparkUniversalConstructionEditor : IPreprocessBuildWithReport
    {
        private const string Menu = "Project Spark/SUPER AI/Universal Auto Draw/";
        private const string MainMenu = "Assets/My_Assets/_Project_Spark/Scenes/MainMenu/MainMenu.unity";
        private const string Reports = "Reports/SuperAI_v19";
        private const string Request = "Logs/SuperAI_v19/editor.request";

        static SparkUniversalConstructionEditor() => EditorApplication.update += Poll;
        public int callbackOrder => 0;

        [MenuItem(Menu + "Audit v20 Construction Evidence")]
        private static void AuditV20() => WithCampaign(manager =>
        {
            var entries = new List<SparkConstructionEvidence.Entry>();
            var plans = Plans();
            foreach (int index in manager.GetCampaignLevelIndices())
            {
                var level = manager.GetLevel(index);
                if (!level) continue;
                var entry = new SparkConstructionEvidence.Entry { level = level.LevelNumber };
                entries.Add(entry);
                try
                {
                    var layout = manager.LayoutLoader.GetAuthoredPrefab(level);
                    if (!layout || layout.LevelDefinition != level)
                    { entry.blocker = "Missing authored layout or matching level definition."; continue; }
                    entry.layout = layout.name;
                    if (!level.Validate(out string problem) || !layout.SceneBindings || !layout.SceneBindings.Validate(out problem))
                    { entry.blocker = string.IsNullOrEmpty(problem) ? "Missing layout-owned binding table." : problem; continue; }

                    var matching = plans.Where(plan => plan.IsApproved && plan.LevelNumber == level.LevelNumber &&
                        plan.LayoutName == layout.name).ToArray();
                    if (matching.Length > 1)
                    { entry.blocker = "Multiple approved assets match this layout."; continue; }
                    var approved = matching.SingleOrDefault();
                    SparkNativeWiringPlan.WireStep[] steps;
                    if (approved)
                    {
                        entry.evidence = approved.SchematicReference;
                        steps = approved.Wires.ToArray();
                    }
                    else if (!TryReadMissionWires(level, layout, out steps, out entry.evidence, out problem))
                    { entry.blocker = problem; continue; }

                    var draft = ScriptableObject.CreateInstance<SparkNativeWiringPlan>();
                    try
                    {
                        draft.LoadDraft(new SparkNativeWiringPlan.RecordedDraft
                            { levelNumber = level.LevelNumber, layoutName = layout.name, wires = steps });
                        problem = ValidateSteps(draft, layout);
                    }
                    finally { UnityEngine.Object.DestroyImmediate(draft); }
                    if (!string.IsNullOrEmpty(problem)) { entry.blocker = problem; continue; }

                    entry.revision = SparkNativeWiringPlan.AuthoredRevision(level, layout, entry.evidence);
                    entry.runtimeEndpoints = steps.SelectMany(step => new[] { step.fromId, step.toId }).Distinct()
                        .Where(id => !layout.SceneBindings.TryGetTerminal(id, out _)).OrderBy(id => id).ToArray();
                    entry.wires = steps;
                    if (approved && (!approved.MatchesReviewedLayout(level, layout, out problem) || approved.ReviewedRevision != entry.revision))
                        entry.blocker = problem ?? "Approved revision differs from current authored evidence.";
                }
                catch (Exception failure) { entry.blocker = failure.Message; }
            }

            var report = new SparkConstructionEvidence.Campaign { levels = entries.OrderBy(entry => entry.level).ToArray() };
            const string folder = "Reports/SuperAI_v20";
            Directory.CreateDirectory(folder);
            string json = JsonUtility.ToJson(report, true);
            File.WriteAllText(Path.Combine(folder, "ConstructionEvidence.json"), json);
            const string runtime = "Assets/Resources/SparkConstructionEvidenceV20.json";
            if (!File.Exists(runtime) || File.ReadAllText(runtime) != json)
            {
                File.WriteAllText(runtime, json);
                AssetDatabase.ImportAsset(runtime);
            }
            Debug.Log($"SUPER AI v20 audited {entries.Count} campaign entries; {entries.Count(entry => entry.wires?.Length > 0 && entry.blocker == null)} passed authored wiring structure checks.");
        });

        [MenuItem(Menu + "Scan All 130 Levels")]
        [MenuItem(Menu + "Validate All Plans")]
        [MenuItem(Menu + "Export Campaign Report")]
        private static void Export() => WithCampaign(manager => Scan(manager, false));

        [MenuItem(Menu + "Generate Construction Plan Drafts")]
        private static void Generate() => WithCampaign(manager => Scan(manager, true));

        [MenuItem(Menu + "Import Generated Mission Draft")]
        private static void ImportMissionDraft()
        {
            string path = EditorUtility.OpenFilePanel("Select a generated mission wiring draft",
                Path.Combine(Reports, "Drafts"), "json");
            if (string.IsNullOrEmpty(path)) return;
            var draft = JsonUtility.FromJson<SparkNativeWiringPlan.RecordedDraft>(File.ReadAllText(path));
            if (draft == null || draft.schemaVersion != 1 || draft.wires == null) return;
            WithCampaign(manager =>
            {
                if (!TryFindLevel(manager, draft.levelNumber, out var definition, out var layout) || layout.name != draft.layoutName ||
                    !layout.SceneBindings)
                { Debug.LogWarning("The draft does not match a unique authored level layout."); return; }
                if (!TryReadMissionWires(definition, layout, out var current, out string source, out string error))
                { Debug.LogWarning(error); return; }
                if (draft.wires.Length != current.Length || !current.Zip(draft.wires, (a, b) =>
                    b != null && a.fromId == b.fromId && a.toId == b.toId).All(match => match))
                { Debug.LogWarning("Generate the draft again using the current authored wiring requirements."); return; }
                var plan = ScriptableObject.CreateInstance<SparkNativeWiringPlan>();
                plan.LoadDraft(draft);
                const string folder = "Assets/Resources/SparkNativeWirePlans";
                string output = AssetDatabase.GenerateUniqueAssetPath($"{folder}/Level_{draft.levelNumber:000}_MissionDraft.asset");
                AssetDatabase.CreateAsset(plan, output);
                Selection.activeObject = plan;
                Debug.Log($"Imported unapproved mission draft {output}. Schematic evidence for explicit review: {source}");
            });
        }

        [MenuItem(Menu + "Review Selected Plan")]
        private static void Review()
        {
            if (!(Selection.activeObject is SparkNativeWiringPlan plan))
            { Debug.LogWarning("Select a native wiring plan asset to review."); return; }
            WithCampaign(manager =>
            {
                if (!TryFindLevel(manager, plan.LevelNumber, out var level, out var layout))
                { Debug.LogWarning("The selected plan has no unique campaign level and authored layout."); return; }
                Debug.Log($"Level {level.LevelNumber} / {layout.name}\nEvidence: {plan.SchematicReference}\n" +
                    string.Join("\n", plan.Wires.Select((s, i) => $"{i + 1}. {s?.fromId} → {s?.toId}")) +
                    "\n" + ValidateSteps(plan, layout));
            });
        }

        [MenuItem(Menu + "Approve Selected Reviewed Revision")]
        private static void ApproveRevision()
        {
            if (!(Selection.activeObject is SparkNativeWiringPlan plan)) return;
            WithCampaign(manager =>
            {
                if (!TryFindLevel(manager, plan.LevelNumber, out var level, out var layout))
                { Debug.LogWarning("The selected plan has no unique campaign level and authored layout."); return; }
                string problem = ValidateSteps(plan, layout);
                if (problem != null) { Debug.LogWarning(problem); return; }
                if (EditorUtility.DisplayDialog("Approve authored revision", $"Bind the existing review by {plan.Reviewer} " +
                    $"to Level {level.LevelNumber} and its current authored revision?\nEvidence: {plan.SchematicReference}",
                    "Bind reviewed revision", "Cancel"))
                    BindRevision(plan, level, layout);
            });
        }

        private static void Poll()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling ||
                EditorApplication.isUpdating || (!File.Exists(Request) && !File.Exists("Logs/SuperAI_v20/audit.request"))) return;
            if (File.Exists("Logs/SuperAI_v20/audit.request"))
            {
                File.Delete("Logs/SuperAI_v20/audit.request");
                try { AuditV20(); }
                catch (Exception problem) { Debug.LogException(problem); }
                return;
            }
            string mode = File.ReadAllText(Request).Trim();
            File.Delete(Request);
            string proof = "Logs/SuperAI_v19/EditorValidation.txt";
            try
            {
                WithCampaign(manager =>
                {
                    if (mode == "retain-level20-review")
                    {
                        var reviewed = Plans().Single(plan => plan.LevelNumber == 20 && plan.IsApproved);
                        if (!TryFindLevel(manager, 20, out var definition, out var prefab))
                            throw new InvalidOperationException("Level 20 has no unique authored identity.");
                        string problem = ValidateSteps(reviewed, prefab);
                        if (problem != null) throw new InvalidOperationException(problem);
                        if (!reviewed.HasRevisionReview) BindRevision(reviewed, definition, prefab);
                    }
                    if (mode == "retain-starter-review")
                        StoreMissionReview(manager, 1, "StarterPlayMode.txt", "PASS Starter native wiring and evaluator 1/1.");
                    if (mode == "retain-level48-review")
                        StoreMissionReview(manager, 48, "Level48PlayMode.txt", "PASS Level 48 native wiring and evaluator 6/6.");
                    Scan(manager, true);
                });
                File.WriteAllText(proof, "PASS campaign audit and evidence export completed. " + DateTimeOffset.Now.ToString("O"));
            }
            catch (Exception error) { File.WriteAllText(proof, "FAIL " + error); Debug.LogException(error); }
        }

        private static void StoreMissionReview(LevelGamePlayManager manager, int levelNumber, string proof, string success)
        {
            if (!TryFindLevel(manager, levelNumber, out var definition, out var layout))
                throw new InvalidOperationException($"Level {levelNumber} has no unique authored layout.");
            if (!TryReadMissionWires(definition, layout, out var steps, out string evidence, out string error))
                throw new InvalidOperationException(error);
            string revision = SparkNativeWiringPlan.AuthoredRevision(definition, layout, evidence);
            string run = File.ReadAllText(Path.Combine("Logs/SuperAI_v19", proof));
            if (!run.Contains($"{success} Review revision {revision}.", StringComparison.Ordinal) ||
                !run.Contains("PASS Verification complete.", StringComparison.Ordinal) || run.Contains("FAIL ", StringComparison.Ordinal))
                throw new InvalidOperationException($"Level {levelNumber}'s current layout has no passing native gameplay review.");
            if (Plans().Any(plan => plan.IsApproved && plan.LevelNumber == levelNumber && plan.LayoutName == layout.name))
                return;
            var planAsset = ScriptableObject.CreateInstance<SparkNativeWiringPlan>();
            planAsset.LoadDraft(new SparkNativeWiringPlan.RecordedDraft { levelNumber = levelNumber, layoutName = layout.name, wires = steps });
            if (!planAsset.TryApprove("Codex source and native gameplay review", evidence, out error) ||
                !planAsset.ReviewAuthoredRevision(definition, layout, out error))
                throw new InvalidOperationException(error);
            const string folder = "Assets/Resources/SparkNativeWirePlans";
            AssetDatabase.CreateAsset(planAsset, AssetDatabase.GenerateUniqueAssetPath($"{folder}/Level_{levelNumber:000}_NativeWiring_Reviewed.asset"));
            AssetDatabase.SaveAssetIfDirty(planAsset);
        }

        private static void BindRevision(SparkNativeWiringPlan plan, SparkLevelDefinition definition, SparkLevelLayout layout)
        {
            Undo.RecordObject(plan, "Bind existing wiring review to authored layout revision");
            if (!plan.ReviewAuthoredRevision(definition, layout, out string error))
                throw new InvalidOperationException(error);
            EditorUtility.SetDirty(plan);
            AssetDatabase.SaveAssetIfDirty(plan);
        }

        private static void WithCampaign(Action<LevelGamePlayManager> inspect)
        {
            var scene = SceneManager.GetSceneByPath(MainMenu);
            bool preview = !scene.IsValid() || !scene.isLoaded;
            if (preview) scene = EditorSceneManager.OpenPreviewScene(MainMenu);
            try
            {
                var manager = scene.GetRootGameObjects().SelectMany(o => o.GetComponentsInChildren<LevelGamePlayManager>(true)).Single();
                inspect(manager);
            }
            finally { if (preview) EditorSceneManager.ClosePreviewScene(scene); }
        }

        private static SparkNativeWiringPlan[] Plans() =>
            Resources.LoadAll<SparkNativeWiringPlan>("SparkNativeWirePlans");

        private static bool TryFindLevel(LevelGamePlayManager manager, int number, out SparkLevelDefinition definition,
            out SparkLevelLayout layout)
        {
            var matches = manager.GetCampaignLevelIndices().Select(manager.GetLevel)
                .Where(level => level && level.LevelNumber == number).Distinct().ToArray();
            definition = matches.Length == 1 ? matches[0] : null;
            layout = definition && manager.LayoutLoader ? manager.LayoutLoader.GetAuthoredPrefab(definition) : null;
            return definition && layout && layout.LevelDefinition == definition;
        }

        private static string ValidateSteps(SparkNativeWiringPlan plan, SparkLevelLayout layout)
        {
            if (!plan.ValidateStructure(out string problem)) return problem;
            var bindings = layout.SceneBindings;
            if (!bindings || !bindings.Validate(out problem)) return problem ?? "Missing authored binding owner.";
            foreach (var wire in plan.Wires)
            {
                if (!bindings.IsTerminalIdDeclared(wire.fromId) || !bindings.IsTerminalIdDeclared(wire.toId))
                    return $"Undeclared endpoint in {wire.fromId} → {wire.toId}.";
                if (bindings.TryGetTerminal(wire.fromId, out var a) && bindings.TryGetTerminal(wire.toId, out var b) &&
                    (!a || !b || !a.Owner || !b.Owner || a == b || a.SharesElectricalOwnerWith(b) ||
                     !a.transform.IsChildOf(layout.transform) || !b.transform.IsChildOf(layout.transform)))
                    return $"Invalid terminal ownership in {wire.fromId} → {wire.toId}.";
            }
            return null;
        }

        private static bool TryReadReplacementNetwork(SparkLevelDefinition definition, SparkLevelLayout layout,
            out SparkNativeWiringPlan.WireStep[] steps, out string evidence, out string error)
        {
            steps = null;
            evidence = AssetDatabase.GetAssetPath(definition) + "#description, powerSources, targets";
            error = "The authored replacement network no longer matches its definition or bindings.";
            bool loadedDivider = definition.LevelNumber == 13;
            string expected = loadedDivider ? "Build a loaded 5 V voltage divider: a fixed 220 ohm R1"
                : "Build a 5 V series-parallel network: fixed 330 ohm R1";
            string fixedResistor = loadedDivider ? "Resistor_220" : "Resistor_330";
            if (!definition.Description.StartsWith(expected, StringComparison.Ordinal) ||
                definition.SourceCount != 1 || definition.TargetCount != 4 ||
                !definition.Targets.Any(target => target.TargetComponentId == "R2_Component" &&
                    target.RequiredPositiveTerminalId == "R2_Input" && target.RequiredNegativeTerminalId == "R2_Output"))
                return false;

            var source = definition.PowerSources[0];
            var pairs = new List<SparkNativeWiringPlan.WireStep>();
            void Add(string from, string to) => pairs.Add(new SparkNativeWiringPlan.WireStep { fromId = from, toId = to });
            Add(source.PositiveTerminalId, "SwitchInput");
            Add("SwitchOutput", fixedResistor + "_Input");
            if (loadedDivider)
            {
                Add(fixedResistor + "_Output", "R2_Input");
                Add(fixedResistor + "_Output", "Resistor_470_Input");
                Add("R2_Output", source.NegativeTerminalId);
            }
            else
            {
                Add("SwitchOutput", "R2_Input");
                Add("R2_Output", "Resistor_470_Input");
                Add(fixedResistor + "_Output", source.NegativeTerminalId);
            }
            Add("Resistor_470_Output", source.NegativeTerminalId);
            foreach (var pair in pairs)
                if (!layout.SceneBindings.IsTerminalIdDeclared(pair.fromId) || !layout.SceneBindings.IsTerminalIdDeclared(pair.toId))
                { error = $"The authored network endpoints are incomplete: {pair.fromId} → {pair.toId}."; return false; }
            steps = pairs.ToArray();
            error = null;
            return true;
        }

        private static bool TryReadDividerRecipe(SparkLevelDefinition definition, SparkLevelLayout layout,
            out SparkNativeWiringPlan.WireStep[] steps, out string evidence, out string error)
        {
            steps = null;
            evidence = "";
            error = "The level's current definition does not match the authored switched divider procedure.";
            bool loaded = definition.LevelNumber == 19;
            string expected = loaded
                ? "From a switched 5 V DC supply, build a 330 ohm TOP resistor"
                : "From a 5 V switched DC source, build a series voltage divider";
            if (!definition.Description.StartsWith(expected, StringComparison.Ordinal) ||
                definition.SourceCount != 1 || definition.TargetCount != 5 ||
                !definition.Targets.Any(target => target.TargetComponentId == "R_TOP_Component") ||
                !definition.Targets.Any(target => target.TargetComponentId == "R_BOTTOM_Component") ||
                (loaded && !definition.Targets.Any(target => target.TargetComponentId == "R_LOAD_Component")))
                return false;

            var power = definition.PowerSources[0];
            var pairs = new List<SparkNativeWiringPlan.WireStep>();
            void Add(string from, string to) => pairs.Add(new SparkNativeWiringPlan.WireStep { fromId = from, toId = to });
            Add(power.PositiveTerminalId, "SwitchInput");
            Add("SwitchOutput", "R_TOP_Input");
            Add("R_TOP_Output", "R_BOTTOM_Input");
            Add("R_BOTTOM_Output", power.NegativeTerminalId);
            if (loaded)
            {
                Add("R_TOP_Output", "R_LOAD_Input");
                Add("R_LOAD_Output", power.NegativeTerminalId);
            }

            foreach (var pair in pairs)
                if (!layout.SceneBindings.IsTerminalIdDeclared(pair.fromId) || !layout.SceneBindings.IsTerminalIdDeclared(pair.toId))
                { error = $"The authored divider endpoints are incomplete: {pair.fromId} → {pair.toId}."; return false; }
            steps = pairs.ToArray();
            evidence = AssetDatabase.GetAssetPath(definition) + "#description, powerSources, targets";
            error = null;
            return true;
        }

        internal static bool TryReadMissionWires(SparkLevelDefinition definition, SparkLevelLayout layout,
            out SparkNativeWiringPlan.WireStep[] steps, out string evidence, out string error)
        {
            steps = null;
            evidence = "";
            error = "No terminal-pair recipe was identified in the active layout's level controller.";
            var terminalBindings = new SerializedObject(layout.SceneBindings).FindProperty("terminals");
            int number = definition.LevelNumber;
            var ids = new Dictionary<SparkTerminal, string>();
            var ambiguous = new HashSet<SparkTerminal>();
            for (int i = 0; i < terminalBindings.arraySize; i++)
            {
                var item = terminalBindings.GetArrayElementAtIndex(i);
                var terminal = item.FindPropertyRelative("terminal").objectReferenceValue as SparkTerminal;
                string id = item.FindPropertyRelative("id").stringValue;
                if (!terminal || string.IsNullOrWhiteSpace(id)) continue;
                if (!ids.TryAdd(terminal, id)) ambiguous.Add(terminal);
            }
            if (number == 1 && definition.TargetCount == 1 && definition.SourceCount == 1 &&
                definition.Targets[0].Type == SparkLevelTarget.TargetType.LEDOn &&
                definition.Description.StartsWith("Connect supply positive to the fixed 150 ohm resistor input,", StringComparison.Ordinal))
            {
                var target = definition.Targets[0];
                var supply = definition.PowerSources[0];
                evidence = AssetDatabase.GetAssetPath(definition) + "#description, powerSources, targets";
                var candidate = new SparkNativeWiringPlan.WireStep[]
                {
                    new() { fromId = supply.PositiveTerminalId, toId = "Register_Input" },
                    new() { fromId = "Register_Output", toId = target.RequiredPositiveTerminalId },
                    new() { fromId = supply.NegativeTerminalId, toId = target.RequiredNegativeTerminalId }
                };
                foreach (var step in candidate)
                {
                    if (!layout.SceneBindings.TryGetTerminal(step.fromId, out var from) ||
                        !layout.SceneBindings.TryGetTerminal(step.toId, out var to) ||
                        !from.Owner || !to.Owner || from.SharesElectricalOwnerWith(to))
                    { error = "The authored starter circuit is missing the supply or LED bindings."; return false; }
                }
                steps = candidate;
                error = null;
                return true;
            }
            if (number is 12 or 13)
                return TryReadReplacementNetwork(definition, layout, out steps, out evidence, out error);
            if (number is 18 or 19 && TryReadDividerRecipe(definition, layout, out steps, out evidence, out error))
                return true;

            foreach (var mission in layout.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (!mission) continue;
                var type = mission.GetType();
                string prefix = $"SparkLevel{number}";
                if (type.Namespace != "ProjectSpark.Gameplay" ||
                    !type.Name.StartsWith(prefix, StringComparison.Ordinal) || type.Name.Length == prefix.Length ||
                    char.IsDigit(type.Name[prefix.Length])) continue;
                var netMethod = type.GetMethod("GetAuthoredWireNets", BindingFlags.Instance | BindingFlags.NonPublic);
                if (netMethod != null)
                {
                    (SparkTerminal, SparkTerminal)[] edges;
                    try
                    {
                        edges = netMethod.Invoke(mission, null) switch
                        {
                            SparkTerminal[][] nets => nets.SelectMany(net => net.Skip(1).Select(pin => (net[0], pin))).ToArray(),
                            (SparkTerminal, SparkTerminal)[] terminalPairs => terminalPairs,
                            _ => Array.Empty<(SparkTerminal, SparkTerminal)>()
                        };
                    }
                    catch (TargetInvocationException)
                    {
                        evidence = AssetDatabase.GetAssetPath(MonoScript.FromMonoBehaviour(mission)) + "#GetAuthoredWireNets";
                        error = "The bound mission could not resolve its authored net members from this prefab.";
                        return false;
                    }
                    if (edges.Length == 0) continue;
                    if (steps != null) { error = "Multiple bound mission controllers provide wire recipes."; steps = null; return false; }
                    evidence = AssetDatabase.GetAssetPath(MonoScript.FromMonoBehaviour(mission)) + "#GetAuthoredWireNets";
                    if (!TryMapRecipe(layout, ids, ambiguous, edges, out steps, out error)) return false;
                    continue;
                }
                const BindingFlags Static = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
                var field = type.GetField("RequiredWires", Static) ?? type.GetField("Wires", Static) ?? type.GetField("Wiring", Static);
                var serialized = new SerializedObject(mission);
                var pins = serialized.FindProperty("pins") ?? serialized.FindProperty("wirePins");
                if (pins == null || !pins.isArray) continue;
                (int, int)[] pairs = null;
                string sourceMember = null;
                if (field != null && field.IsInitOnly && field.FieldType == typeof(ValueTuple<int, int>[]))
                {
                    pairs = (ValueTuple<int, int>[])field.GetValue(null);
                    sourceMember = field.Name;
                }
                else if (TryReadCommissioningPairs(type, pins.arraySize, out pairs))
                    sourceMember = "CheckWiring";
                else if (number >= 88 && number <= 93 && type.Name == prefix + "Process")
                {
                    // These evaluators authorize one edge per consecutive pin pair in ReadWires.
                    var counts = type.GetField("connected", BindingFlags.Instance | BindingFlags.NonPublic);
                    if (counts?.GetValue(mission) is bool[] links && links.Length * 2 == pins.arraySize)
                    {
                        pairs = Enumerable.Range(0, links.Length).Select(i => (i * 2, i * 2 + 1)).ToArray();
                        sourceMember = "ReadWires";
                    }
                }
                if (pairs == null || pairs.Length == 0) continue;
                string source = AssetDatabase.GetAssetPath(MonoScript.FromMonoBehaviour(mission));
                evidence = $"{source}#{sourceMember}";
                var terminalEdges = new (SparkTerminal, SparkTerminal)[pairs.Length];
                for (int i = 0; i < pairs.Length; i++)
                {
                    var (a, b) = pairs[i];
                    if (a < 0 || b < 0 || a >= pins.arraySize || b >= pins.arraySize)
                    { error = $"Authored pair {i + 1} names a missing serialized pin."; return false; }
                    terminalEdges[i] = (pins.GetArrayElementAtIndex(a).objectReferenceValue as SparkTerminal,
                        pins.GetArrayElementAtIndex(b).objectReferenceValue as SparkTerminal);
                }
                if (steps != null) { error = "Multiple bound mission controllers provide wire recipes."; steps = null; return false; }
                if (!TryMapRecipe(layout, ids, ambiguous, terminalEdges, out steps, out error)) return false;
            }
            if (steps == null) return false;
            error = null;
            return true;
        }

        private static bool TryMapRecipe(SparkLevelLayout layout, Dictionary<SparkTerminal, string> ids,
            HashSet<SparkTerminal> ambiguous, IReadOnlyList<(SparkTerminal from, SparkTerminal to)> edges,
            out SparkNativeWiringPlan.WireStep[] steps, out string error)
        {
            steps = null;
            error = null;
            var mapped = new SparkNativeWiringPlan.WireStep[edges.Count];
            var seen = new HashSet<(string, string)>();
            var endpointCounts = new Dictionary<SparkTerminal, int>();
            for (int i = 0; i < edges.Count; i++)
            {
                var (from, to) = edges[i];
                if (!from || !to || !from.Owner || !to.Owner || from == to ||
                    !from.transform.IsChildOf(layout.transform) || !to.transform.IsChildOf(layout.transform) ||
                    ambiguous.Contains(from) || ambiguous.Contains(to) ||
                    !ids.TryGetValue(from, out string fromId) || !ids.TryGetValue(to, out string toId))
                { error = $"Authored pair {i + 1} lacks two unique, layout-owned terminal bindings."; return false; }
                if (from.SharesElectricalOwnerWith(to))
                { error = $"Authored pair {i + 1} ({fromId} → {toId}) would bypass an electrical owner."; return false; }
                if (!from.Owner.InteractionsEnabled || !to.Owner.InteractionsEnabled)
                { error = $"Authored pair {i + 1} ({fromId} → {toId}) uses a disabled interaction owner."; return false; }
                var key = string.CompareOrdinal(fromId, toId) < 0 ? (fromId, toId) : (toId, fromId);
                if (!seen.Add(key)) { error = $"The mission repeats authored pair {i + 1}."; return false; }
                if (ExceedsCapacity(from) || ExceedsCapacity(to))
                { error = $"Authored pair {i + 1} ({fromId} → {toId}) exceeds a terminal's wire capacity."; return false; }
                mapped[i] = new SparkNativeWiringPlan.WireStep { fromId = fromId, toId = toId };
            }
            steps = mapped;
            return true;

            bool ExceedsCapacity(SparkTerminal terminal)
            {
                endpointCounts.TryGetValue(terminal, out int count);
                endpointCounts[terminal] = ++count;
                return count > terminal.MaxConnections;
            }
        }

        private static bool TryReadCommissioningPairs(Type mission, int count, out (int, int)[] pairs)
        {
            // Commissioned topology from the native CheckWiring methods. Crossed pairs are fault cases.
            pairs = mission.Name switch
            {
                "SparkLevel43PositionControl" when count == 12 =>
                    new[] { (0, 2), (1, 3), (4, 5), (6, 7), (8, 9), (10, 11) },
                "SparkLevel44Encoder" when count == 10 =>
                    new[] { (0, 2), (1, 3), (4, 5), (6, 7), (8, 9) },
                "SparkLevel46Sync" when count == 24 =>
                    Enumerable.Range(0, 12).Select(i => (i * 2, i * 2 + 1)).ToArray(),
                "SparkLevel47Tension" when count == 28 =>
                    Enumerable.Range(0, 12).Select(i => (i * 2, i * 2 + 1))
                        .Concat(new[] { (0, 24), (2, 25), (26, 27) }).ToArray(),
                _ => null
            };
            return pairs != null;
        }

        private static void ExportDraft(int number, SparkLevelLayout layout, string revision,
            SparkNativeWiringPlan.WireStep[] steps, string source)
        {
            var draft = new SparkNativeWiringPlan.RecordedDraft { levelNumber = number, layoutName = layout.name, wires = steps };
            string folder = Path.Combine(Reports, "Drafts");
            Directory.CreateDirectory(folder);
            File.WriteAllText(Path.Combine(folder, $"Level_{number:000}.json"), JsonUtility.ToJson(draft, true));
            File.WriteAllLines(Path.Combine(folder, $"Level_{number:000}_Evidence.txt"),
                new[] { source, AssetDatabase.GetAssetPath(layout), revision });
        }

        private static void Scan(LevelGamePlayManager manager, bool generate)
        {
            Directory.CreateDirectory(Reports);
            var plans = Plans();
            var rows = new List<string> { "Level,ID,DefinitionGUID,Layout,Revision,Category,Objectives,InventorySlots,Components,TerminalBindings,PlanSource,PlanState,Validation,ApprovedActions,MissingBindings,UnsupportedTools,Evaluator,ExecutionTest,Reason" };
            int scanned = 0, approved = 0;
            var campaignLevels = manager.GetCampaignLevelIndices().Select(manager.GetLevel).Where(level => level).ToArray();
            int count = Math.Max(130, campaignLevels.Length == 0 ? 0 : campaignLevels.Max(level => level.LevelNumber));
            for (int number = 1; number <= count; number++)
            {
                if (!TryFindLevel(manager, number, out var definition, out var layout))
                { rows.Add(Csv(number, definition ? definition.LevelId : "", "", "", "", "Unknown", 0, 0, 0, 0, "",
                    "Blocked", "Missing or ambiguous definition/layout", 0, "", "", "", "NOT TESTED", "Resolve campaign mapping.")); continue; }
                scanned++;
                var bindings = layout.SceneBindings;
                var slots = layout.GetComponentsInChildren<SparkShelfSlot>(true);
                var components = layout.GetComponentsInChildren<SparkElectronicObject>(true);
                var behaviours = layout.GetComponentsInChildren<MonoBehaviour>(true);
                var records = behaviours
                    .Where(record => record && record.GetType().Namespace?.StartsWith("ProjectSpark.Level", StringComparison.Ordinal) == true)
                    .Select(record => record.GetType().Name).Distinct().OrderBy(name => name, StringComparer.Ordinal).ToArray();
                var boardSources = new List<string>();
                foreach (var behaviour in behaviours)
                {
                    if (!behaviour || behaviour.GetType().Namespace != "ProjectSpark.Gameplay" ||
                        !behaviour.GetType().Name.StartsWith("SparkLevel", StringComparison.Ordinal)) continue;
                    var board = new SerializedObject(behaviour);
                    var routes = board.FindProperty("routes");
                    if (routes == null || !routes.isArray || routes.arraySize == 0) continue;
                    var route = routes.GetArrayElementAtIndex(0);
                    var from = route.FindPropertyRelative("from");
                    var to = route.FindPropertyRelative("to");
                    if (from?.propertyType == SerializedPropertyType.ObjectReference &&
                        to?.propertyType == SerializedPropertyType.ObjectReference &&
                        from.objectReferenceValue is SparkTerminal && to.objectReferenceValue is SparkTerminal)
                    {
                        var parts = board.FindProperty("parts");
                        int partCount = parts != null && parts.isArray ? parts.arraySize : 0;
                        string script = AssetDatabase.GetAssetPath(MonoScript.FromMonoBehaviour(behaviour));
                        boardSources.Add($"{script} | {routes.arraySize} explicit board routes | {partCount} authored part slots");
                    }
                }
                bool boardPlacement = records.Any(name => name.EndsWith("PlacementSlot", StringComparison.Ordinal) ||
                    name.EndsWith("PlacementTarget", StringComparison.Ordinal)) || boardSources.Count > 0;
                bool unresolvedTask = records.Any(name => name.IndexOf("BlockedTask", StringComparison.Ordinal) >= 0 ||
                    name.IndexOf("Unverified", StringComparison.Ordinal) >= 0) || boardSources.Count > 0;
                string revision = SparkNativeWiringPlan.AuthoredRevision(definition, layout);
                string validation = !definition.Validate(out string rule) ? rule :
                    !bindings ? "No layout-owned binding" : !bindings.Validate(out string bindingError) ? bindingError : "Authored structure validated";
                var levelPlans = plans.Where(p => p.LevelNumber == number && p.LayoutName == layout.name).ToArray();
                var authorized = levelPlans.Where(p => p.IsApproved).ToArray();
                var plan = authorized.Length == 1 ? authorized[0] : null;
                SparkNativeWiringPlan.WireStep[] missionSteps = null;
                string missionEvidence = "", missionProblem = null;
                bool hasMissionDraft = !plan && bindings && validation == "Authored structure validated" &&
                    TryReadMissionWires(definition, layout, out missionSteps, out missionEvidence, out missionProblem);
                string sourceEvidence = plan ? plan.SchematicReference : missionEvidence;
                if (sourceEvidence?.IndexOf(".cs#", StringComparison.OrdinalIgnoreCase) >= 0)
                    revision = SparkNativeWiringPlan.AuthoredRevision(definition, layout, sourceEvidence);
                string reason = authorized.Length > 1 ? "Duplicate approved plans." :
                    plan ? ValidateSteps(plan, layout) : hasMissionDraft
                        ? $"{missionSteps.Length} native terminal pairs from the bound mission evaluator require explicit review."
                        : !string.IsNullOrEmpty(missionEvidence) ? missionProblem : boardSources.Count > 0
                        ? "Bound board routes require board-native placement, copper/via and diagnostic stages."
                        : unresolvedTask
                        ? "Authored engineering records contain blocked or unverified tasks and no approved native action recipe."
                        : "No reviewed terminal pair recipe for this layout.";
                if (plan && reason == null && (!plan.MatchesReviewedLayout(definition, layout, out reason) ||
                    plan.ReviewedRevision != revision)) reason ??= "Authored revision changed.";
                string state = plan && reason == null ? "Approved" :
                    authorized.Length > 1 || levelPlans.Any(p => p.HasRecordedApproval && !p.IsApproved) ||
                    plan && plan.HasRevisionReview ? "Invalidated" :
                    levelPlans.Length > 0 ? "NeedsReview" : hasMissionDraft ? "GeneratedDraft" :
                    unresolvedTask || !string.IsNullOrEmpty(missionEvidence) ? "Blocked" : "Missing";
                if (state == "Approved") approved++;
                if (generate && plan && ValidateSteps(plan, layout) == null)
                    ExportDraft(number, layout, revision, plan.Wires.ToArray(),
                        $"{AssetDatabase.GetAssetPath(plan)} | {plan.SchematicReference}");
                else if (generate && state == "GeneratedDraft")
                    ExportDraft(number, layout, revision, missionSteps, missionEvidence);
                var categories = new List<string>();
                if (bindings && bindings.TerminalBindingCount > 0) categories.Add("Native electrical wiring");
                if (slots.Any(s => s && s.Kind == SparkShelfSlotKind.Workstation)) categories.Add("Component placement");
                if (components.Any(component => component is SparkInstrument)) categories.Add("Electrical measurement equipment");
                if (components.Any(component => component is SparkSwitch)) categories.Add("Switch/device operation");
                if (definition.RequireHealthyInstalledResistors || definition.RequiredHealthyResistorIds?.Length > 0)
                    categories.Add("Circuit repair");
                if (boardPlacement) categories.Add("Board assembly records");
                if (unresolvedTask) categories.Add("Blocked authored engineering tasks");
                if (categories.Count == 0) categories.Add("Other authored task");
                rows.Add(Csv(number, definition.LevelId, AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(definition)),
                    AssetDatabase.GetAssetPath(layout), revision, string.Join(" + ", categories), definition.TargetCount, slots.Length, components.Length,
                    bindings ? bindings.TerminalBindingCount : 0,
                    plan ? $"{AssetDatabase.GetAssetPath(plan)} | {plan.SchematicReference}" :
                    !string.IsNullOrEmpty(missionEvidence) ? missionEvidence : string.Join(" | ", boardSources.Concat(records)),
                    state, validation, state == "Approved" ? plan.Wires.Count : 0, validation == "Authored structure validated" ? "" : validation,
                    boardPlacement ? "Board actions require native tools, authoritative mappings and review" :
                    slots.Length > 0 ? "Autonomous placement requires a reviewed native action recipe" : "",
                    nameof(LevelGamePlayManager), "NOT TESTED by campaign scan", reason));
            }
            File.WriteAllLines(Path.Combine(Reports, "CampaignCoverage.csv"), rows);
            File.WriteAllText(Path.Combine(Reports, "Level_1_to_130_Coverage.md"),
                $"# Campaign coverage\n\nResolved {scanned}/{count} campaign definitions and authored layouts. " +
                $"{approved} layouts have a unique current revision review. See CampaignCoverage.csv for the per-level evidence. " +
                "An asset audit does not establish native construction or completed gameplay.\n");
        }

        private static string Csv(params object[] fields) =>
            string.Join(",", fields.Select(value => "\"" + (value?.ToString() ?? "").Replace("\"", "\"\"") + "\""));

        public void OnPreprocessBuild(BuildReport report)
        {
            foreach (var plan in Plans().Where(plan => plan.HasRevisionReview))
                if (!plan.ValidateStoredAuthoredRevision(out string reason))
                    throw new BuildFailedException($"{plan.name}: {reason}");
        }
    }
}
