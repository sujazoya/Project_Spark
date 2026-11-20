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
    internal static partial class SparkLevel98Builder
    {
        internal const string Folder = "Assets/My_Assets/_Project_Spark/Prefabs/Levels/Level_98_Layout";
        internal const string Prefab = Folder + "/Level_98_Layout.prefab";
        internal const string Definition = Folder + "/Level_98.asset";
        internal const string Scene = "Assets/My_Assets/_Project_Spark/Scenes/MainMenu/MainMenu.unity";
        private const string Catalog = "Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Levels/LevelLayoutCatalog/SparkLevelLayoutCatalog.asset";
        private const string Report = "Logs/Level98Builder.txt";
        private const string CircuitRoot = "PIS6-EDU-1 training circuit";

        static SparkLevel98Builder() => EditorApplication.update += Poll;

        private static void Poll()
        {
            const string request = "Logs/Level98Builder.request";
            if (!File.Exists(request) || EditorApplication.isPlayingOrWillChangePlaymode ||
                EditorApplication.isCompiling || EditorApplication.isUpdating) return;
            File.Delete(request);
            try { Build(); }
            catch (Exception error) { File.WriteAllText(Report, error.ToString()); Debug.LogException(error); }
        }

        [MenuItem("Tools/Project Spark/Levels/106/Create Level 98")]
        internal static void Build()
        {
            var blueprint = AssetDatabase.LoadAssetAtPath<SparkAutoLevelBlueprint>(Folder + "/L98_SourceBlueprint.asset");
            if (!blueprint || blueprint.LevelId != 98 || blueprint.InstallableCount != 56 ||
                blueprint.Parts.Count != 74 || blueprint.Routes.Count != 74 || blueprint.Vias.Count != 16 ||
                blueprint.CopperLayers != 4 || blueprint.Objectives.Count != 6)
                throw new InvalidOperationException("Level 106 needs its complete PIS-6 source blueprint.");
            var root = PrefabUtility.LoadPrefabContents(Prefab);
            try
            {
                GameObjectUtility.RemoveMonoBehavioursWithMissingScript(root);
                if (root.TryGetComponent<SparkAutoLevelRuntime>(out var generator)) Object.DestroyImmediate(generator);
                if (root.TryGetComponent<SparkLevel98Process>(out var previous)) Object.DestroyImmediate(previous);
                var process = root.AddComponent<SparkLevel98Process>();
                process.layout = root.GetComponent<SparkLevelLayout>() ?? root.AddComponent<SparkLevelLayout>();
                process.bindings = root.GetComponent<SparkLevelSceneBindings>() ?? root.AddComponent<SparkLevelSceneBindings>();
                process.fixture = root.GetComponent<SparkAutoL98WorkshopFit>().ScaledVisualRoot;
                var modelSpace = process.fixture.Find("L98_NativeMeters_YUp_SourceGLBConverted");
                process.boardArt = modelSpace.Find("BoardArt_EmptyPIS6");
                process.referenceLayers = modelSpace.Find("FourCopperLayers_References_ArtOnly");
                var supports = modelSpace.Find("FactorySupportingArt_DisabledByDefault");
                foreach (var prefix in new[] { "58_L98_", "59_L98_", "60_L98_", "61_L98_" })
                {
                    var layer = supports.Cast<Transform>().FirstOrDefault(part => part.name.StartsWith(prefix, StringComparison.Ordinal));
                    if (layer) layer.SetParent(process.referenceLayers, true);
                }
                process.referenceLayers.gameObject.SetActive(false);
                Transform retiredCircuit = modelSpace.Find(CircuitRoot);
                if (retiredCircuit) Object.DestroyImmediate(retiredCircuit.gameObject);
                var materials = AssetDatabase.FindAssets("t:Material", new[] { Folder + "/URP_PBR_Materials" })
                    .Select(guid => AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guid)))
                    .ToDictionary(mat => mat.name.Replace("L98_", "").Replace("_Realistic", ""), StringComparer.OrdinalIgnoreCase);
                var slots = modelSpace.Find("ComponentSlots_56_UNINSTALLED");
                var axes = Child(modelSpace, CircuitRoot);
                axes.localRotation = Quaternion.Inverse(modelSpace.localRotation);
                ImportParts(process, blueprint, slots);
                var art = modelSpace.Find("FaultVisuals_Disabled");
                process.faultOverlays = new[] { "72_L98_Degraded_ESR_Fault_Visual", "73_L98_Open_Ground_Via_Fault_Visual", "74_L98_Kelvin_Sense_Fault_Visual" }
                    .Select(id => art.Find(id).gameObject).ToArray();
                foreach (var overlay in process.faultOverlays) overlay.SetActive(false);
                art.gameObject.SetActive(true);
                modelSpace.Find("CompleteAssembledBoard_ReferenceOnly_Disabled").gameObject.SetActive(false);

                var csv = new List<string> { "task,subsystem,training_net,layer,from,to,from_x_mm,from_z_mm,to_x_mm,to_z_mm,via,exit_layer" };
                process.profile = BuildProfile();
                var devices = new Circuit(axes);
                ConfigureBoard(process, blueprint, devices, materials, csv);
                ConfigureBench(process, devices, materials, csv);
                var definition = BuildDefinition();
                Bind(process);
                Set(process.layout, ("levelDefinition", definition), ("sceneBindings", process.bindings));
                var saved = PrefabUtility.SaveAsPrefabAsset(root, Prefab);
                if (!saved || saved.GetComponent<SparkLevelLayout>().LevelDefinition != definition)
                    throw new InvalidOperationException("Failed to save and reload the playable Level 106 prefab.");
                string design = Folder + "/Data/PIS6_EDU_1_Training_Routes.csv";
                File.WriteAllLines(design, csv);
                AssetDatabase.ImportAsset(design);
                AssetDatabase.SaveAssets();
                Register(definition, saved.GetComponent<SparkLevelLayout>());
                File.WriteAllText(Report, "BUILT PIS6-EDU-1. Registered playable prefab in MainMenu and the layout catalog.\n");
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }

        private static void ImportParts(SparkLevel98Process process, SparkAutoLevelBlueprint blueprint, Transform slots)
        {
            var parts = new List<SparkLevel98Process.Part>();
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
                var target = slot.GetComponent<SparkLevel98Footprint>() ?? slot.gameObject.AddComponent<SparkLevel98Footprint>();
                target.level = process;
                target.index = parts.Count;
                parts.Add(new SparkLevel98Process.Part { label = item.Id.Substring("00_L98_".Length), footprint = slot, model = instance });
            }
            process.parts = parts.ToArray();
            process.jumperPart = parts.FindIndex(part => part.label == "W075");
            process.capacitorPart = parts.FindIndex(part => part.label == "CP2");
            if (parts.Count != SparkLevel98Process.PartCount || process.jumperPart < 0 || process.capacitorPart < 0)
                throw new InvalidOperationException("The PIS-6 component manifest or W075 provision changed.");
        }

        private static SparkLevel98Profile BuildProfile()
        {
            const string path = Folder + "/PIS6_Training_Profile.asset";
            var profile = AssetDatabase.LoadAssetAtPath<SparkLevel98Profile>(path);
            if (profile) return profile;
            profile = ScriptableObject.CreateInstance<SparkLevel98Profile>();
            AssetDatabase.CreateAsset(profile, path);
            return profile;
        }

        private static string Row(SparkLevel98Process.Route route, Vector3 a, Vector3 b) =>
            FormattableString.Invariant($"{route.id},{route.group},PIS6_EDU_{route.group.Replace(' ', '_')},{route.layer},{route.from.name},{route.to.name},{a.x * 1000:F3},{a.z * 1000:F3},{b.x * 1000:F3},{b.z * 1000:F3},{route.viaId},{route.exitLayer}");
    }
}
