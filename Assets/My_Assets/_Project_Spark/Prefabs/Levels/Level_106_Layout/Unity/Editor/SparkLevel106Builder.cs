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
    internal static partial class SparkLevel106Builder
    {
        internal const string Folder = "Assets/My_Assets/_Project_Spark/Prefabs/Levels/Level_106_Layout";
        internal const string Prefab = Folder + "/Level_106_Layout.prefab";
        internal const string Definition = Folder + "/Level_106.asset";
        internal const string Scene = "Assets/My_Assets/_Project_Spark/Scenes/MainMenu/MainMenu.unity";
        private const string Catalog = "Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Levels/LevelLayoutCatalog/SparkLevelLayoutCatalog.asset";
        private const string Report = "Logs/Level106Builder.txt";
        private const string CircuitRoot = "SFX14-EDU-1 training circuit";

        static SparkLevel106Builder() => EditorApplication.update += Poll;

        private static void Poll()
        {
            const string request = "Logs/Level106Builder.request";
            if (!File.Exists(request) || EditorApplication.isPlayingOrWillChangePlaymode ||
                EditorApplication.isCompiling || EditorApplication.isUpdating) return;
            File.Delete(request);
            try { Build(); }
            catch (Exception error) { File.WriteAllText(Report, error.ToString()); Debug.LogException(error); }
        }

        [MenuItem("Tools/Project Spark/Levels/106/Install First Boot Training Level")]
        internal static void Build()
        {
            var blueprint = AssetDatabase.LoadAssetAtPath<SparkAutoLevelBlueprint>(Folder + "/L106_SourceBlueprint.asset");
            if (!blueprint || blueprint.LevelId != 106 || blueprint.InstallableCount != 106 ||
                blueprint.Parts.Count != 136 || blueprint.Routes.Count != 140 || blueprint.Vias.Count != 42 ||
                blueprint.CopperLayers != 10 || blueprint.Objectives.Count != 6)
                throw new InvalidOperationException("Level 106 needs its complete SFX-14 source blueprint.");
            var root = PrefabUtility.LoadPrefabContents(Prefab);
            try
            {
                GameObjectUtility.RemoveMonoBehavioursWithMissingScript(root);
                if (root.TryGetComponent<SparkAutoLevelRuntime>(out var generator)) Object.DestroyImmediate(generator);
                if (root.TryGetComponent<SparkLevel106Process>(out var previous)) Object.DestroyImmediate(previous);
                var process = root.AddComponent<SparkLevel106Process>();
                process.layout = root.GetComponent<SparkLevelLayout>() ?? root.AddComponent<SparkLevelLayout>();
                process.bindings = root.GetComponent<SparkLevelSceneBindings>() ?? root.AddComponent<SparkLevelSceneBindings>();
                process.fixture = root.GetComponent<SparkAutoL106WorkshopFit>().ModelScaleRoot;
                var modelSpace = process.fixture.Find("L106_NativeFBXModelSpace");
                process.boardArt = modelSpace.Find("L106_BoardArtRoot");
                process.referenceLayers = modelSpace.Find("L106_CopperLayers_10_VisualOnly");
                process.referenceLayers.gameObject.SetActive(false);
                Transform retiredCircuit = modelSpace.Find(CircuitRoot);
                if (retiredCircuit) Object.DestroyImmediate(retiredCircuit.gameObject);
                var materials = AssetDatabase.FindAssets("t:Material", new[] { Folder + "/URP_PBR_Materials" })
                    .Select(guid => AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guid)))
                    .ToDictionary(mat => mat.name.Replace("_AutoPBR", ""), StringComparer.OrdinalIgnoreCase);
                var slots = modelSpace.Find("L106_ComponentSlots_106_UNINSTALLED");
                var axes = Child(modelSpace, CircuitRoot);
                axes.localRotation = Quaternion.Inverse(modelSpace.localRotation);
                ImportParts(process, blueprint, slots, materials);
                var art = modelSpace.Find("L106_FaultOverlays_Disabled");
                process.faultOverlays = new[] { "130_L106_CPU_PowerGood_Fault", "131_L106_SPI_SolderBridge_Fault", "132_L106_Storage_Contact_Fault" }
                    .Select(id => art.Find(id).gameObject).ToArray();
                foreach (var overlay in process.faultOverlays) overlay.SetActive(false);
                art.gameObject.SetActive(true);
                modelSpace.Find("L106_CompleteAssembledReference_NoProgress").gameObject.SetActive(false);

                var csv = new List<string> { "task,subsystem,training_net,layer,from,to,from_x_mm,from_z_mm,to_x_mm,to_z_mm,via,exit_layer" };
                var devices = new Circuit(axes);
                ConfigureBoard(process, blueprint, devices, materials, csv);
                ConfigureBoot(process, devices, materials, csv);
                var definition = BuildDefinition();
                Bind(process);
                Set(process.layout, ("levelDefinition", definition), ("sceneBindings", process.bindings));
                var saved = PrefabUtility.SaveAsPrefabAsset(root, Prefab);
                if (!saved || saved.GetComponent<SparkLevelLayout>().LevelDefinition != definition)
                    throw new InvalidOperationException("Failed to save and reload the playable Level 106 prefab.");
                Register(definition, saved.GetComponent<SparkLevelLayout>());
                string design = Folder + "/Data/SFX14_EDU_1_Training_Routes.csv";
                File.WriteAllLines(design, csv);
                AssetDatabase.ImportAsset(design);
                AssetDatabase.SaveAssets();
                File.WriteAllText(Report, "BUILT SFX14-EDU-1. Registered playable prefab in MainMenu and the layout catalog.\n");
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }

        private static void ImportParts(SparkLevel106Process process, SparkAutoLevelBlueprint blueprint, Transform slots,
            Dictionary<string, Material> materials)
        {
            var parts = new List<SparkLevel106Process.Part>();
            foreach (var item in blueprint.Parts.Where(part => part.Installable))
            {
                var slot = slots.Find(item.Id);
                var model = AssetDatabase.LoadAssetAtPath<GameObject>(Folder + "/Models_OBJ_Fallback/" + item.Id + ".obj");
                if (!slot || !model) throw new InvalidOperationException("Missing source slot or XY source mesh: " + item.Id);
                GameObjectUtility.RemoveMonoBehavioursWithMissingScript(slot.gameObject);
                if (slot.TryGetComponent<SparkAutoPlacementTarget>(out var obsolete)) Object.DestroyImmediate(obsolete);
                var retired = slot.Find("Installed component");
                if (retired) Object.DestroyImmediate(retired.gameObject);
                var instance = (GameObject)PrefabUtility.InstantiatePrefab(model, slot);
                instance.name = "Installed component";
                instance.transform.localPosition = Vector3.zero;
                instance.transform.localRotation = Quaternion.identity;
                foreach (var renderer in instance.GetComponentsInChildren<Renderer>(true))
                    renderer.sharedMaterials = renderer.sharedMaterials.Select(material => ArtMaterial(material, materials)).ToArray();
                if (!instance.GetComponentsInChildren<MeshFilter>(true).Any(filter => filter.sharedMesh && filter.sharedMesh.vertexCount > 0))
                    throw new InvalidOperationException("Source mesh has no geometry: " + item.Id);
                instance.SetActive(false);
                slot.gameObject.layer = LayerMask.NameToLayer("UI");
                var target = slot.GetComponent<SparkLevel106Footprint>() ?? slot.gameObject.AddComponent<SparkLevel106Footprint>();
                target.level = process;
                target.index = parts.Count;
                parts.Add(new SparkLevel106Process.Part { label = item.Id.Substring(9), footprint = slot, model = instance });
            }
            process.parts = parts.ToArray();
            if (parts.Count != SparkLevel106Process.PartCount || parts[^1].label != "W141")
                throw new InvalidOperationException("The SFX-14 component manifest or W141 provision changed.");
        }

        private static Material ArtMaterial(Material original, Dictionary<string, Material> materials)
        {
            if (!original) return original;
            string name = original.name.ToUpperInvariant().Replace(" (MATERIAL)", "");
            if (materials.TryGetValue(name, out var authored)) return authored;
            foreach (var item in materials)
                if (name.EndsWith("_" + item.Key, StringComparison.OrdinalIgnoreCase)) return item.Value;
            return original;
        }

        private static string Row(SparkLevel106Process.Route route, Vector3 a, Vector3 b) =>
            FormattableString.Invariant($"{route.id},{route.group},SFX14_EDU_{route.group.Replace(' ', '_')},{route.layer},{route.from.name},{route.to.name},{a.x * 1000:F3},{a.z * 1000:F3},{b.x * 1000:F3},{b.z * 1000:F3},{route.viaId},{route.exitLayer}");
    }
}
