using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Globalization;
using ProjectSpark.Gameplay;
using ProjectSpark.Gameplay.MemoryMatrix;
using SparkLevel104Process = ProjectSpark.Gameplay.MemoryMatrix.SparkLevel104Process;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace ProjectSpark.EditorTools.MemoryMatrix
{
    [InitializeOnLoad]
    internal static partial class SparkLevel104Builder
    {
        internal const string Folder = "Assets/My_Assets/_Project_Spark/Prefabs/Levels/Level_104_Layout";
        internal const string Prefab = Folder + "/Level_104_Layout.prefab";
        internal const string Definition = Folder + "/Level_104.asset";
        internal const string Scene = "Assets/My_Assets/_Project_Spark/Scenes/MainMenu/MainMenu.unity";
        private const string Catalog = "Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Levels/LevelLayoutCatalog/SparkLevelLayoutCatalog.asset";
        private const string Report = "Logs/Level104Builder.txt";
        private const string CircuitRoot = "MMX12-EDU-1 training circuit";

        static SparkLevel104Builder() => EditorApplication.update += Poll;

        private static void Poll()
        {
            const string request = "Logs/Level104Builder.request";
            if (!File.Exists(request) || EditorApplication.isPlayingOrWillChangePlaymode ||
                EditorApplication.isCompiling || EditorApplication.isUpdating) return;
            File.Delete(request);
            try { Build(); }
            catch (Exception error) { File.WriteAllText(Report, error.ToString()); Debug.LogException(error); }
        }

        [MenuItem("Tools/Project Spark/Levels/104/Create Level 104")]
        internal static void Build()
        {
            var placements = SourceRows("ComponentPlacements.csv");
            var plans = SourceRows("CopperRoutes_124_REQUIRES_SCHEMATIC.csv");
            var barrels = SourceRows("ViaTasks_34_REQUIRES_SCHEMATIC.csv");
            if (placements.Length != SparkLevel104Process.PartCount || plans.Length != SparkLevel104Process.RouteCount ||
                barrels.Length != SparkLevel104Process.ViaCount || plans.Any(row => row[2] != "UNASSIGNED"))
                throw new InvalidOperationException("MMX-12 source manifest is incomplete or has changed.");
            var root = PrefabUtility.LoadPrefabContents(Prefab);
            try
            {
                if (root.TryGetComponent<SparkLevel104Process>(out var previous)) Object.DestroyImmediate(previous);
                var process = root.AddComponent<SparkLevel104Process>();
                if (!root.TryGetComponent<SparkLevelLayout>(out process.layout)) process.layout = root.AddComponent<SparkLevelLayout>();
                if (!root.TryGetComponent<SparkLevelSceneBindings>(out process.bindings)) process.bindings = root.AddComponent<SparkLevelSceneBindings>();
                foreach (var node in root.GetComponentsInChildren<Transform>(true))
                    GameObjectUtility.RemoveMonoBehavioursWithMissingScript(node.gameObject);
                process.fixture = root.transform.Find("Visuals");
                process.boardArt = process.fixture.Find("PCB_Base");
                process.referenceLayers = process.fixture.Find("LayerAndInspectionArt_DISABLED");
                process.referenceLayers.gameObject.SetActive(false);
                process.fixture.Find("AssembledReference_DISABLED").gameObject.SetActive(false);
                process.fixture.localScale = Vector3.one;

                var materials = AssetDatabase.FindAssets("t:Material", new[] { Folder + "/URP_PBR_Materials" })
                    .Select(guid => AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guid)))
                    .ToDictionary(mat => mat.name.Replace("L104_", ""), StringComparer.OrdinalIgnoreCase);
                var retired = process.fixture.Find(CircuitRoot);
                if (retired) Object.DestroyImmediate(retired.gameObject);
                var slots = process.fixture.Find("Playable footprints");
                if (!slots) slots = Child(process.fixture, "Playable footprints");
                ImportParts(root, process, slots, placements);

                var csv = new List<string> { "task,subsystem,training_net,entry_layer,from,to,from_x_mm,from_z_mm,to_x_mm,to_z_mm,via,exit_layer" };
                var circuit = new Circuit(Child(process.fixture, CircuitRoot));
                ConfigureBoard(process, circuit, plans, barrels.Select(row => row[0]), materials, csv);
                ConfigureController(process, circuit, materials, csv);
                process.fixture.localScale = Vector3.one * (10.13f / .26f);
                var definition = BuildDefinition();
                Bind(process);
                Set(process.layout, ("levelDefinition", definition), ("sceneBindings", process.bindings));
                var saved = PrefabUtility.SaveAsPrefabAsset(root, Prefab);
                if (!saved || saved.GetComponent<SparkLevelLayout>().LevelDefinition != definition)
                    throw new InvalidOperationException("Failed to save and reload the playable Level 104 prefab.");

                string design = Folder + "/Level104_MMX12/Data/MMX12_EDU_1_Training_Routes.csv";
                File.WriteAllLines(design, csv);
                AssetDatabase.ImportAsset(design);
                AssetDatabase.SaveAssets();
                Register(definition, saved.GetComponent<SparkLevelLayout>());
                File.WriteAllText(Report, "BUILT MMX12-EDU-1. Registered Memory Matrix in MainMenu and the layout catalog.\n");
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }

        private static string[][] SourceRows(string filename) => File.ReadAllLines(Folder + "/Level104_MMX12/Data/" + filename)
            .Skip(1).Where(line => !string.IsNullOrWhiteSpace(line)).Select(line => line.Split(',')).ToArray();

        private static void ImportParts(GameObject root, SparkLevel104Process process, Transform slots, string[][] placements)
        {
            var originals = root.transform.Find("ComponentPlacementTargets_94_UNINSTALLED");
            var parts = new List<SparkLevel104Process.Part>();
            foreach (var row in placements)
            {
                string id = row[0];
                var slot = slots.Find("SLOT_" + id + "_UNINSTALLED");
                if (!slot)
                {
                    slot = originals.Find("SLOT_" + id + "_UNINSTALLED");
                    if (!slot) throw new InvalidOperationException("Missing original component placement for " + id);
                    slot.SetParent(slots, false);
                }
                var expected = new Vector3(float.Parse(row[1], CultureInfo.InvariantCulture), float.Parse(row[2], CultureInfo.InvariantCulture),
                    float.Parse(row[3], CultureInfo.InvariantCulture));
                if ((slot.localPosition - expected).sqrMagnitude > 1e-8f)
                    throw new InvalidOperationException("Component placement differs from source: " + id);
                var model = AssetDatabase.LoadAssetAtPath<GameObject>(Folder + "/ComponentVisualPrefabs/L104_" + id + "_Visual.prefab");
                if (!model) throw new InvalidOperationException("Missing MMX-12 styled component: " + id);
                var retired = slot.Find("Installed component");
                if (retired) Object.DestroyImmediate(retired.gameObject);
                var instance = (GameObject)PrefabUtility.InstantiatePrefab(model, slot);
                instance.name = "Installed component";
                instance.transform.localPosition = Vector3.zero;
                instance.transform.localRotation = Quaternion.identity;
                var artNodes = instance.GetComponentsInChildren<Transform>(true);
                if (artNodes.Any(node => GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(node.gameObject) > 0))
                {
                    PrefabUtility.UnpackPrefabInstance(instance, PrefabUnpackMode.OutermostRoot, InteractionMode.AutomatedAction);
                    foreach (var node in artNodes)
                        GameObjectUtility.RemoveMonoBehavioursWithMissingScript(node.gameObject);
                }
                var renderers = instance.GetComponentsInChildren<MeshRenderer>(true);
                if (renderers.Length == 0) throw new InvalidOperationException("No geometry for " + id);
                Bounds hit = renderers[0].bounds;
                foreach (var renderer in renderers) hit.Encapsulate(renderer.bounds);
                if (!slot.TryGetComponent<BoxCollider>(out var collider)) collider = slot.gameObject.AddComponent<BoxCollider>();
                collider.center = slot.InverseTransformPoint(hit.center);
                collider.size = Vector3.Max(hit.size, new Vector3(.003f, .003f, .003f));
                instance.SetActive(false);
                slot.gameObject.layer = LayerMask.NameToLayer("UI");
                if (!slot.TryGetComponent<SparkLevel104Footprint>(out var target)) target = slot.gameObject.AddComponent<SparkLevel104Footprint>();
                target.level = process;
                target.index = parts.Count;
                parts.Add(new SparkLevel104Process.Part { label = id, footprint = slot, model = instance });
            }
            originals.gameObject.SetActive(false);
            root.transform.Find("SourceTasks_NOT_CERTIFIABLE").gameObject.SetActive(false);
            process.parts = parts.ToArray();
            process.jumperPart = parts.FindIndex(part => part.label == "W125");
            process.terminationPart = parts.FindIndex(part => part.label == "RN1");
            process.bgaPart = parts.FindIndex(part => part.label == "U3");
            if (parts.Count != SparkLevel104Process.PartCount || process.jumperPart < 0 || process.terminationPart < 0 || process.bgaPart < 0)
                throw new InvalidOperationException("MMX-12 component manifest has changed.");
        }

        private static string Row(SparkLevel104Process.Route route) =>
            FormattableString.Invariant($"{route.id},{route.group},{route.net},{route.layer},{route.from.name},{route.to.name},{route.entry.x * 1000:F3},{route.entry.z * 1000:F3},{route.destination.x * 1000:F3},{route.destination.z * 1000:F3},{route.viaId},{route.exitLayer}");
    }
}
