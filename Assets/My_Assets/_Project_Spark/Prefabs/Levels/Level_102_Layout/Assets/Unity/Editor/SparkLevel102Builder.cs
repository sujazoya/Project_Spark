using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Globalization;
using ProjectSpark.Gameplay;
using ProjectSpark.Gameplay.SignalPhantom;
using SparkLevel102Process = ProjectSpark.Gameplay.SignalPhantom.SparkLevel102Process;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace ProjectSpark.EditorTools.SignalPhantom
{
    [InitializeOnLoad]
    internal static partial class SparkLevel102Builder
    {
        internal const string Folder = "Assets/My_Assets/_Project_Spark/Prefabs/Levels/Level_102_Layout";
        internal const string Prefab = Folder + "/Level_102_Layout.prefab";
        internal const string Definition = Folder + "/Level_102.asset";
        internal const string Scene = "Assets/My_Assets/_Project_Spark/Scenes/MainMenu/MainMenu.unity";
        private const string Catalog = "Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Levels/LevelLayoutCatalog/SparkLevelLayoutCatalog.asset";
        private const string Report = "Logs/Level102Builder.txt";
        private const string CircuitRoot = "HSX10-EDU-1 training circuit";

        static SparkLevel102Builder() => EditorApplication.update += Poll;

        private static void Poll()
        {
            const string request = "Logs/Level102Builder.request";
            if (!File.Exists(request) || EditorApplication.isPlayingOrWillChangePlaymode ||
                EditorApplication.isCompiling || EditorApplication.isUpdating) return;
            File.Delete(request);
            try { Build(); }
            catch (Exception error) { File.WriteAllText(Report, error.ToString()); Debug.LogException(error); }
        }

        [MenuItem("Tools/Project Spark/Levels/102/Create Level 102")]
        internal static void Build()
        {
            var placements = SourceRows("ComponentPlacements.csv");
            var plans = SourceRows("RouteTasks_108_REQUIRES_NETLIST.csv");
            var barrels = SourceRows("ViaTasks_28_REQUIRES_NETLIST.csv");
            if (placements.Length != SparkLevel102Process.PartCount || plans.Length != SparkLevel102Process.RouteCount ||
                barrels.Length != SparkLevel102Process.ViaCount || plans.Any(row => row[2] != "UNASSIGNED"))
                throw new InvalidOperationException("HSX-10 source manifest is incomplete or has changed.");
            var root = PrefabUtility.LoadPrefabContents(Prefab);
            try
            {
                if (root.TryGetComponent<SparkLevel102Process>(out var previous)) Object.DestroyImmediate(previous);
                var process = root.AddComponent<SparkLevel102Process>();
                if (!root.TryGetComponent<SparkLevelLayout>(out process.layout)) process.layout = root.AddComponent<SparkLevelLayout>();
                if (!root.TryGetComponent<SparkLevelSceneBindings>(out process.bindings)) process.bindings = root.AddComponent<SparkLevelSceneBindings>();
                foreach (var node in root.GetComponentsInChildren<Transform>(true))
                    GameObjectUtility.RemoveMonoBehavioursWithMissingScript(node.gameObject);
                process.fixture = root.transform.Find("Visuals");
                process.boardArt = process.fixture.Find("PCB_Base");
                process.referenceLayers = process.fixture.Find("LayerInspection_ArtOnly_Disabled");
                process.referenceLayers.gameObject.SetActive(false);
                process.fixture.Find("Reference_Assembly_NonPlayable_Disabled").gameObject.SetActive(false);
                process.fixture.localScale = Vector3.one;

                var materials = AssetDatabase.FindAssets("t:Material", new[] { Folder + "/URP_PBR_Materials" })
                    .Select(guid => AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guid)))
                    .ToDictionary(mat => mat.name.Replace("L102_", ""), StringComparer.OrdinalIgnoreCase);
                var retired = process.fixture.Find(CircuitRoot);
                if (retired) Object.DestroyImmediate(retired.gameObject);
                var slots = process.fixture.Find("Playable footprints");
                if (!slots) slots = Child(process.fixture, "Playable footprints");
                ImportParts(root, process, slots, placements);

                var csv = new List<string> { "task,subsystem,training_net,entry_layer,from,to,from_x_mm,from_z_mm,to_x_mm,to_z_mm,via,exit_layer" };
                var circuit = new Circuit(Child(process.fixture, CircuitRoot));
                ConfigureBoard(process, circuit, plans, barrels.Select(row => row[0]), materials, csv);
                ConfigureController(process, circuit, materials, csv);
                process.fixture.localScale = Vector3.one * (10.13f / .25f);
                var definition = BuildDefinition();
                Bind(process);
                Set(process.layout, ("levelDefinition", definition), ("sceneBindings", process.bindings));
                var saved = PrefabUtility.SaveAsPrefabAsset(root, Prefab);
                if (!saved || saved.GetComponent<SparkLevelLayout>().LevelDefinition != definition)
                    throw new InvalidOperationException("Failed to save and reload the playable Level 102 prefab.");

                string design = Folder + "/Assets/Data/HSX10_EDU_1_Training_Routes.csv";
                File.WriteAllLines(design, csv);
                AssetDatabase.ImportAsset(design);
                AssetDatabase.SaveAssets();
                Register(definition, saved.GetComponent<SparkLevelLayout>());
                File.WriteAllText(Report, "BUILT HSX10-EDU-1. Registered Signal Phantom in MainMenu and the layout catalog.\n");
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }

        private static string[][] SourceRows(string filename) => File.ReadAllLines(Folder + "/Assets/Data/" + filename)
            .Skip(1).Where(line => !string.IsNullOrWhiteSpace(line)).Select(line => line.Split(',')).ToArray();

        private static void ImportParts(GameObject root, SparkLevel102Process process, Transform slots, string[][] placements)
        {
            var planning = root.transform.Find("GameplayAuthority_INTEGRATION_REQUIRED");
            var originals = planning.Find("ComponentFootprints_SOURCE_ONLY");
            var parts = new List<SparkLevel102Process.Part>();
            foreach (var row in placements)
            {
                string id = row[0];
                var slot = slots.Find("Slot_" + id + "_UNINSTALLED");
                if (!slot)
                {
                    slot = originals.Find("Slot_" + id + "_UNINSTALLED");
                    if (!slot) throw new InvalidOperationException("Missing original component placement for " + id);
                    slot.SetParent(slots, false);
                }
                var expected = new Vector3(float.Parse(row[2], CultureInfo.InvariantCulture), .0008f,
                    -float.Parse(row[3], CultureInfo.InvariantCulture));
                if ((slot.localPosition - expected).sqrMagnitude > 1e-8f)
                    throw new InvalidOperationException("Component placement differs from source: " + id);
                var model = AssetDatabase.LoadAssetAtPath<GameObject>(Folder + "/ComponentVisualPrefabs/L102_" + id + "_Visual.prefab");
                if (!model) throw new InvalidOperationException("Missing HSX-10 styled component: " + id);
                var retired = slot.Find("Installed component");
                if (retired) Object.DestroyImmediate(retired.gameObject);
                var instance = (GameObject)PrefabUtility.InstantiatePrefab(model, slot);
                instance.name = "Installed component";
                instance.transform.localPosition = Vector3.zero;
                instance.transform.localRotation = Quaternion.identity;
                var renderers = instance.GetComponentsInChildren<MeshRenderer>(true);
                if (renderers.Length == 0) throw new InvalidOperationException("No geometry for " + id);
                Bounds hit = renderers[0].bounds;
                foreach (var renderer in renderers) hit.Encapsulate(renderer.bounds);
                if (!slot.TryGetComponent<BoxCollider>(out var collider)) collider = slot.gameObject.AddComponent<BoxCollider>();
                collider.center = slot.InverseTransformPoint(hit.center);
                collider.size = Vector3.Max(hit.size, new Vector3(.003f, .003f, .003f));
                instance.SetActive(false);
                slot.gameObject.layer = LayerMask.NameToLayer("UI");
                if (!slot.TryGetComponent<SparkLevel102Footprint>(out var target)) target = slot.gameObject.AddComponent<SparkLevel102Footprint>();
                target.level = process;
                target.index = parts.Count;
                parts.Add(new SparkLevel102Process.Part { label = id, footprint = slot, model = instance });
            }
            planning.gameObject.SetActive(false);
            process.parts = parts.ToArray();
            process.jumperPart = parts.FindIndex(part => part.label == "W109");
            process.terminationPart = parts.FindIndex(part => part.label == "R1");
            if (parts.Count != SparkLevel102Process.PartCount || process.jumperPart < 0 || process.terminationPart < 0)
                throw new InvalidOperationException("HSX-10 component manifest has changed.");
        }

        private static string Row(SparkLevel102Process.Route route) =>
            FormattableString.Invariant($"{route.id},{route.group},{route.net},{route.layer},{route.from.name},{route.to.name},{route.entry.x * 1000:F3},{route.entry.z * 1000:F3},{route.destination.x * 1000:F3},{route.destination.z * 1000:F3},{route.viaId},{route.exitLayer}");
    }
}
