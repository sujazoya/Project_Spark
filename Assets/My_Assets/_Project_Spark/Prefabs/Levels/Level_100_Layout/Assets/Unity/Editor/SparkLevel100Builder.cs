using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ProjectSpark.AutoLevelGenerator;
using ProjectSpark.Gameplay;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace ProjectSpark.EditorTools
{
    [InitializeOnLoad]
    internal static partial class SparkLevel100Builder
    {
        internal const string Folder = "Assets/My_Assets/_Project_Spark/Prefabs/Levels/Level_100_Layout";
        internal const string Prefab = Folder + "/Level_100_Layout.prefab";
        internal const string Definition = Folder + "/Level_100.asset";
        internal const string Scene = "Assets/My_Assets/_Project_Spark/Scenes/MainMenu/MainMenu.unity";
        private const string Catalog = "Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Levels/LevelLayoutCatalog/SparkLevelLayoutCatalog.asset";
        private const string Report = "Logs/Level100Builder.txt";
        private const string CircuitRoot = "DNX8-EDU-1 training circuit";

        static SparkLevel100Builder() => EditorApplication.update += Poll;

        private static void Poll()
        {
            const string request = "Logs/Level100Builder.request";
            if (!File.Exists(request) || EditorApplication.isPlayingOrWillChangePlaymode ||
                EditorApplication.isCompiling || EditorApplication.isUpdating) return;
            File.Delete(request);
            try { Build(); }
            catch (Exception error) { File.WriteAllText(Report, error.ToString()); Debug.LogException(error); }
        }

        [MenuItem("Tools/Project Spark/Levels/100/Create Level 100")]
        internal static void Build()
        {
            var blueprint = AssetDatabase.LoadAssetAtPath<SparkAutoLevelBlueprint>(Folder + "/L100_SourceBlueprint.asset");
            if (!blueprint || blueprint.LevelId != 100 || blueprint.InstallableCount != 70 ||
                blueprint.Parts.Count != 88 || blueprint.Routes.Count != 92 || blueprint.Vias.Count != 20 ||
                blueprint.CopperLayers != 4 || blueprint.Objectives.Count != 6)
                throw new InvalidOperationException("Level 100 needs its complete DNX-8 source blueprint.");
            var root = PrefabUtility.LoadPrefabContents(Prefab);
            try
            {
                GameObjectUtility.RemoveMonoBehavioursWithMissingScript(root);
                if (root.TryGetComponent<SparkAutoLevelRuntime>(out var generator)) Object.DestroyImmediate(generator);
                if (root.TryGetComponent<SparkLevel100Process>(out var previous)) Object.DestroyImmediate(previous);
                var process = root.AddComponent<SparkLevel100Process>();
                process.layout = root.GetComponent<SparkLevelLayout>() ?? root.AddComponent<SparkLevelLayout>();
                process.bindings = root.GetComponent<SparkLevelSceneBindings>() ?? root.AddComponent<SparkLevelSceneBindings>();
                process.fixture = root.GetComponent<SparkAutoL100WorkshopFit>().ScaledVisualRoot;
                var modelSpace = process.fixture.Find("L100_NativeMetres_YUp_GLB_To_OBJ");
                process.boardArt = modelSpace.Find("BoardArt_EmptyDNX8");
                process.referenceLayers = modelSpace.Find("FourCopperLayers_References_ArtOnly");
                var supports = modelSpace.Find("FactorySupportingArt_DisabledByDefault");
                foreach (var prefix in new[] { "072_L100_", "073_L100_", "074_L100_", "075_L100_" })
                {
                    var layer = supports.Cast<Transform>().FirstOrDefault(part => part.name.StartsWith(prefix, StringComparison.Ordinal));
                    if (layer) layer.SetParent(process.referenceLayers, true);
                }
                process.referenceLayers.gameObject.SetActive(false);
                Transform retiredCircuit = modelSpace.Find(CircuitRoot);
                if (retiredCircuit) Object.DestroyImmediate(retiredCircuit.gameObject);
                var materials = AssetDatabase.FindAssets("t:Material", new[] { Folder + "/URP_PBR_Materials" })
                    .Select(guid => AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guid)))
                    .ToDictionary(mat => mat.name.Replace("L100_", "").Replace("_Realistic", ""), StringComparer.OrdinalIgnoreCase);
                var slots = modelSpace.Find("ComponentSlots_70_UNINSTALLED");
                var axes = Child(modelSpace, CircuitRoot);
                axes.localRotation = Quaternion.Inverse(modelSpace.localRotation);
                ImportParts(process, blueprint, slots);
                var art = modelSpace.Find("FaultVisuals_Disabled");
                process.faultOverlays = new[] { "086_L100_I2C_Pullup_Fault_Overlay", "087_L100_SPI_OpenVia_Fault_Overlay", "088_L100_UART_Config_Fault_Overlay" }
                    .Select(id => modelSpace.GetComponentsInChildren<Transform>(true).Single(item => item.name == id).gameObject)
                    .ToArray();
                foreach (var overlay in process.faultOverlays)
                {
                    overlay.SetActive(false);
                    overlay.transform.SetParent(art, true);
                }
                art.gameObject.SetActive(true);
                modelSpace.Find("CompleteAssembledBoard_ReferenceOnly_Disabled").gameObject.SetActive(false);

                var csv = new List<string> { "task,subsystem,training_net,layer,from,to,from_x_mm,from_z_mm,to_x_mm,to_z_mm,via,exit_layer" };
                var devices = new Circuit(axes);
                ConfigureBoard(process, blueprint, devices, materials, csv);
                ConfigureController(process, devices, materials, csv);
                var definition = BuildDefinition();
                Bind(process);
                Set(process.layout, ("levelDefinition", definition), ("sceneBindings", process.bindings));
                var saved = PrefabUtility.SaveAsPrefabAsset(root, Prefab);
                if (!saved || saved.GetComponent<SparkLevelLayout>().LevelDefinition != definition)
                    throw new InvalidOperationException("Failed to save and reload the playable Level 100 prefab.");
                string design = Folder + "/Assets/Data/DNX8_EDU_1_Training_Routes.csv";
                File.WriteAllLines(design, csv);
                AssetDatabase.ImportAsset(design);
                AssetDatabase.SaveAssets();
                Register(definition, saved.GetComponent<SparkLevelLayout>());
                File.WriteAllText(Report, "BUILT DNX8-EDU-1. Registered playable prefab in MainMenu and the layout catalog.\n");
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }

        private static void ImportParts(SparkLevel100Process process, SparkAutoLevelBlueprint blueprint, Transform slots)
        {
            var parts = new List<SparkLevel100Process.Part>();
            foreach (var item in blueprint.Parts.Where(part => part.Installable))
            {
                var slot = slots.Find(item.Id);
                var model = AssetDatabase.LoadAssetAtPath<GameObject>(Folder + "/ComponentVisualPrefabs/" + item.Id + "_PBR.prefab");
                if (!slot || !model) throw new InvalidOperationException("Missing source slot or styled component prefab: " + item.Id);
                GameObjectUtility.RemoveMonoBehavioursWithMissingScript(slot.gameObject);
                if (slot.TryGetComponent<SparkAutoPlacementTarget>(out var obsolete)) Object.DestroyImmediate(obsolete);
                var retired = slot.Find("Installed component");
                if (retired) Object.DestroyImmediate(retired.gameObject);
                var instance = (GameObject)PrefabUtility.InstantiatePrefab(model, slot);
                instance.name = "Installed component";
                instance.transform.localPosition = Vector3.zero;
                instance.transform.localRotation = Quaternion.identity;
                if (!instance.GetComponentsInChildren<MeshFilter>(true).Any(filter => filter.sharedMesh && filter.sharedMesh.vertexCount > 0))
                    throw new InvalidOperationException("Source mesh has no geometry: " + item.Id);
                instance.SetActive(false);
                slot.gameObject.layer = LayerMask.NameToLayer("UI");
                var target = slot.GetComponent<SparkLevel100Footprint>() ?? slot.gameObject.AddComponent<SparkLevel100Footprint>();
                target.level = process;
                target.index = parts.Count;
                parts.Add(new SparkLevel100Process.Part { label = item.Id.Substring("000_L100_".Length), footprint = slot, model = instance });
            }
            process.parts = parts.ToArray();
            process.jumperPart = parts.FindIndex(part => part.label == "W093");
            process.pullupPart = parts.FindIndex(part => part.label == "R1");
            process.uartPart = parts.FindIndex(part => part.label == "U8");
            if (parts.Count != SparkLevel100Process.PartCount || process.jumperPart < 0 || process.pullupPart < 0 || process.uartPart < 0)
                throw new InvalidOperationException("The DNX-8 component manifest or W093 provision changed.");
        }

        private static string Row(SparkLevel100Process.Route route, Vector3 a, Vector3 b) =>
            FormattableString.Invariant($"{route.id},{route.group},{route.net},{route.layer},{route.from.name},{route.to.name},{a.x * 1000:F3},{a.z * 1000:F3},{b.x * 1000:F3},{b.z * 1000:F3},{route.viaId},{route.exitLayer}");
    }
}
