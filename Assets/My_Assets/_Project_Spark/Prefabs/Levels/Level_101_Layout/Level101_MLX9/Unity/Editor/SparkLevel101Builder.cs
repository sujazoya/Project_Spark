using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Globalization;
using ProjectSpark.Gameplay;
using ProjectSpark.Gameplay.HiddenLayers;
using SparkLevel101Process = ProjectSpark.Gameplay.HiddenLayers.SparkLevel101Process;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace ProjectSpark.EditorTools.HiddenLayers
{
    [InitializeOnLoad]
    internal static partial class SparkLevel101Builder
    {
        internal const string Folder = "Assets/My_Assets/_Project_Spark/Prefabs/Levels/Level_101_Layout";
        internal const string Prefab = Folder + "/Level_101_Layout.prefab";
        internal const string Definition = Folder + "/Level_101.asset";
        internal const string Scene = "Assets/My_Assets/_Project_Spark/Scenes/MainMenu/MainMenu.unity";
        private const string Catalog = "Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Levels/LevelLayoutCatalog/SparkLevelLayoutCatalog.asset";
        private const string Report = "Logs/Level101Builder.txt";
        private const string CircuitRoot = "MLX9-EDU-1 training circuit";

        static SparkLevel101Builder() => EditorApplication.update += Poll;

        private static void Poll()
        {
            const string request = "Logs/Level101Builder.request";
            if (!File.Exists(request) || EditorApplication.isPlayingOrWillChangePlaymode ||
                EditorApplication.isCompiling || EditorApplication.isUpdating) return;
            File.Delete(request);
            try { Build(); }
            catch (Exception error) { File.WriteAllText(Report, error.ToString()); Debug.LogException(error); }
        }

        [MenuItem("Tools/Project Spark/Levels/101/Create Level 101")]
        internal static void Build()
        {
            var placements = SourceRows("Component_BOM_76.csv");
            var plans = SourceRows("Copper_Routes_100_UNVERIFIED.csv");
            var barrels = SourceRows("Vias_24_UNVERIFIED.csv");
            if (placements.Length != SparkLevel101Process.PartCount || plans.Length != SparkLevel101Process.RouteCount ||
                barrels.Length != SparkLevel101Process.ViaCount || plans.Any(row => row[2] != "UNASSIGNED"))
                throw new InvalidOperationException("MLX-9 source manifest is incomplete or has changed.");
            var root = PrefabUtility.LoadPrefabContents(Prefab);
            try
            {
                if (root.TryGetComponent<SparkLevel101Process>(out var previous)) Object.DestroyImmediate(previous);
                var process = root.AddComponent<SparkLevel101Process>();
                if (!root.TryGetComponent<SparkLevelLayout>(out process.layout)) process.layout = root.AddComponent<SparkLevelLayout>();
                if (!root.TryGetComponent<SparkLevelSceneBindings>(out process.bindings)) process.bindings = root.AddComponent<SparkLevelSceneBindings>();
                foreach (var node in root.GetComponentsInChildren<Transform>(true))
                    GameObjectUtility.RemoveMonoBehavioursWithMissingScript(node.gameObject);
                process.fixture = root.transform.Find("Visuals");
                process.boardArt = process.fixture.Find("EmptyPCB_FactoryOnly");
                process.referenceLayers = process.fixture.Find("LayerInspection_REFERENCE_DISABLED");
                process.referenceLayers.gameObject.SetActive(false);
                process.fixture.localScale = Vector3.one;

                var materials = AssetDatabase.FindAssets("t:Material", new[] { Folder + "/URP_PBR_Materials" })
                    .Select(guid => AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guid)))
                    .ToDictionary(mat => mat.name.Replace("L101_", ""), StringComparer.OrdinalIgnoreCase);
                var retired = process.fixture.Find(CircuitRoot);
                if (retired) Object.DestroyImmediate(retired.gameObject);
                var slots = process.fixture.Find("Playable footprints");
                if (!slots) slots = Child(process.fixture, "Playable footprints");
                ImportParts(root, process, slots, placements);

                var csv = new List<string> { "task,subsystem,training_net,entry_layer,from,to,from_x_mm,from_z_mm,to_x_mm,to_z_mm,via,exit_layer" };
                var circuit = new Circuit(Child(process.fixture, CircuitRoot));
                ConfigureBoard(process, circuit, plans, barrels.Select(row => row[0]), materials, csv);
                ConfigureController(process, circuit, materials, csv);
                process.fixture.localScale = Vector3.one * (10.13f / .23f);
                var definition = BuildDefinition();
                Bind(process);
                Set(process.layout, ("levelDefinition", definition), ("sceneBindings", process.bindings));
                var saved = PrefabUtility.SaveAsPrefabAsset(root, Prefab);
                if (!saved || saved.GetComponent<SparkLevelLayout>().LevelDefinition != definition)
                    throw new InvalidOperationException("Failed to save and reload the playable Level 101 prefab.");

                string design = Folder + "/Level101_MLX9/Data/MLX9_EDU_1_Training_Routes.csv";
                File.WriteAllLines(design, csv);
                AssetDatabase.ImportAsset(design);
                AssetDatabase.SaveAssets();
                Register(definition, saved.GetComponent<SparkLevelLayout>());
                File.WriteAllText(Report, "BUILT MLX9-EDU-1. Registered The Hidden Layers in MainMenu and the layout catalog.\n");
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }

        private static string[][] SourceRows(string filename) => File.ReadAllLines(Folder + "/Level101_MLX9/Data/" + filename)
            .Skip(1).Where(line => !string.IsNullOrWhiteSpace(line)).Select(line => line.Split(',')).ToArray();

        private static void ImportParts(GameObject root, SparkLevel101Process process, Transform slots, string[][] placements)
        {
            var planning = root.transform.Find("SourcePlanning_NOT_GAMEPLAY_AUTHORITY");
            var originals = planning.Find("FootprintTargets_76_ALL_UNINSTALLED");
            var parts = new List<SparkLevel101Process.Part>();
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
                var expected = new Vector3(float.Parse(row[3], CultureInfo.InvariantCulture), slot.localPosition.y,
                    float.Parse(row[4], CultureInfo.InvariantCulture));
                if ((slot.localPosition - expected).sqrMagnitude > 1e-8f)
                    throw new InvalidOperationException("Component placement differs from source: " + id);
                var model = AssetDatabase.LoadAssetAtPath<GameObject>(Folder + "/ComponentVisualPrefabs/L101_" + id + "_Visual.prefab");
                if (!model) throw new InvalidOperationException("Missing MLX-9 styled component: " + id);
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
                if (!slot.TryGetComponent<SparkLevel101Footprint>(out var target)) target = slot.gameObject.AddComponent<SparkLevel101Footprint>();
                target.level = process;
                target.index = parts.Count;
                parts.Add(new SparkLevel101Process.Part { label = id, footprint = slot, model = instance });
            }
            planning.gameObject.SetActive(false);
            process.parts = parts.ToArray();
            process.jumperPart = parts.FindIndex(part => part.label == "W101");
            if (parts.Count != SparkLevel101Process.PartCount || process.jumperPart < 0)
                throw new InvalidOperationException("MLX-9 component manifest has changed.");
        }

        private static string Row(SparkLevel101Process.Route route) =>
            FormattableString.Invariant($"{route.id},{route.group},{route.net},{route.layer},{route.from.name},{route.to.name},{route.entry.x * 1000:F3},{route.entry.z * 1000:F3},{route.destination.x * 1000:F3},{route.destination.z * 1000:F3},{route.viaId},{route.exitLayer}");
    }
}
