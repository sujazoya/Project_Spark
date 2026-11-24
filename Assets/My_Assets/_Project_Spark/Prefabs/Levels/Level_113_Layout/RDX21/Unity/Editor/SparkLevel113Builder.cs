using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using ProjectSpark.Gameplay;
using ProjectSpark.Gameplay.RedundancyCitadel;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ProjectSpark.EditorTools.RedundancyCitadel
{
    [InitializeOnLoad]
    internal static partial class SparkLevel113Builder
    {
        internal const string Folder = "Assets/My_Assets/_Project_Spark/Prefabs/Levels/Level_113_Layout";
        internal const string Prefab = Folder + "/Level_113_Layout.prefab";
        internal const string Definition = Folder + "/Level_113.asset";
        internal const string Scene = "Assets/My_Assets/_Project_Spark/Scenes/MainMenu/MainMenu.unity";
        private const string Kit = Folder + "/RDX21";
        private const string Catalog = "Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Levels/LevelLayoutCatalog/SparkLevelLayoutCatalog.asset";
        private const string Report = "Logs/Level113Builder.txt";

        static SparkLevel113Builder() => EditorApplication.update += Poll;

        private static void Poll()
        {
            const string request = "Logs/Level113Builder.request";
            if (!File.Exists(request) || EditorApplication.isPlayingOrWillChangePlaymode ||
                EditorApplication.isCompiling || EditorApplication.isUpdating) return;
            File.Delete(request);
            try { Build(); }
            catch (Exception error) { File.WriteAllText(Report, error.ToString()); Debug.LogException(error); }
        }

        [MenuItem("Tools/Project Spark/Levels/Create Level 113")]
        internal static void Build()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            var placements = Rows("L113_ComponentPlacements.csv");
            var plans = Rows("L113_CopperRoutes_232_SCHEMATIC_REQUIRED.csv");
            var vias = Rows("L113_ViaTasks_76_SCHEMATIC_REQUIRED.csv");
            var models = Rows("L113_ModelManifest.csv").ToDictionary(row => row[1], StringComparer.Ordinal);
            if (placements.Length != SparkLevel113Process.PartCount || placements.Select(row => row[0]).Distinct().Count() != placements.Length ||
                models.Count != 206 || plans.Length != SparkLevel113Process.RouteCount || vias.Length != SparkLevel113Process.ViaCount ||
                plans.Any(row => row[2] != "UNASSIGNED") || vias.Any(row => row[1] != "UNASSIGNED"))
                throw new InvalidOperationException("The RDX-21 kit or task IDs differ from the training revision.");

            var materials = AssetDatabase.FindAssets("t:Material", new[] { Folder + "/URP_PBR_Materials" })
                .Select(id => AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(id)))
                .ToDictionary(material => material.name.Replace("L113_", ""), StringComparer.Ordinal);
            if (materials.Count != 24 || materials.Values.Any(material => !material.GetTexture("_BaseMap") ||
                !material.GetTexture("_BumpMap") || !material.GetTexture("_MetallicGlossMap") || !material.GetTexture("_OcclusionMap")))
                throw new InvalidOperationException("Expected all 24 complete RDX-21 PBR surface families.");
            // Keep the PCB readable below the inspection lamps without flattening
            // the metallic heatsink, copper and solder families.
            var mask = materials["Soldermask_Cobalt"];
            mask.SetFloat("_Smoothness", .54f);
            mask.SetFloat("_BumpScale", .72f);
            AssetDatabase.SaveAssetIfDirty(mask);

            var root = PrefabUtility.LoadPrefabContents(Prefab);
            try
            {
                while (root.transform.childCount > 0) Object.DestroyImmediate(root.transform.GetChild(0).gameObject);
                if (root.TryGetComponent<ProjectSpark.Level113.SparkL113LayoutMarker>(out var visualOnly))
                    Object.DestroyImmediate(visualOnly);
                if (root.TryGetComponent<SparkLevel113Process>(out var previous)) Object.DestroyImmediate(previous);
                var board = root.AddComponent<SparkLevel113Process>();
                if (!root.TryGetComponent<SparkLevelLayout>(out board.layout)) board.layout = root.AddComponent<SparkLevelLayout>();
                if (!root.TryGetComponent<SparkLevelSceneBindings>(out board.bindings)) board.bindings = root.AddComponent<SparkLevelSceneBindings>();

                board.fixture = Child(root.transform, "RDX-21 playable workbench");
                board.boardArt = Child(board.fixture, "Original 380 × 270 mm board");
                ImportModel(models["PCB_Main"], board.boardArt, materials);
                board.referenceLayers = Child(board.fixture, "18 visual inspection layers");
                for (int layer = 1; layer <= SparkLevel113Process.LayerCount; layer++)
                    ImportModel(models[$"L{layer:00}_Reference"], board.referenceLayers, materials).name = $"L{layer:00}_Reference";
                board.referenceLayers.gameObject.SetActive(false);
                ImportParts(board, Child(board.fixture, "166 original footprints"), placements, models, materials);
                BuildFootprintLegend(board);

                var graph = new Circuit(Child(board.fixture, "RDX21-EDU-1 native training circuit"));
                List<string> mapping = ConfigureGraph(board, graph, plans, vias.Select(row => row[0]), materials);
                board.fixture.localScale = Vector3.one * (10.13f / .380f);
                var definition = BuildDefinition();
                Bind(board);
                Set(board.layout, ("levelDefinition", definition), ("sceneBindings", board.bindings));
                var saved = PrefabUtility.SaveAsPrefabAsset(root, Prefab);
                if (!saved || saved.GetComponent<SparkLevelLayout>().LevelDefinition != definition)
                    throw new InvalidOperationException("The playable RDX-21 prefab could not be reloaded.");

                string map = Kit + "/Data/RDX21_EDU_1_Training_Routes.csv";
                File.WriteAllLines(map, mapping);
                AssetDatabase.ImportAsset(map);
                AssetDatabase.SaveAssets();
                Register(definition, saved.GetComponent<SparkLevelLayout>());
                File.WriteAllText(Report, "BUILT RDX21-EDU-1. Native campaign prefab and catalog registration verified." + Environment.NewLine);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }

        private static string[][] Rows(string file) =>
            ProjectSpark.Level113.Editor.SparkL113FullLayoutBuilder.ReadCsv(Kit + "/Data/" + file).ToArray();

        private static void ImportParts(SparkLevel113Process board, Transform parent, string[][] placements,
            Dictionary<string, string[]> models, Dictionary<string, Material> materials)
        {
            board.parts = placements.Select((row, i) =>
            {
                string id = row[0];
                Vector3 position = new(Number(row[2]), Number(row[3]), Number(row[4]));
                var slot = Child(parent, id + " original footprint", position);
                slot.localRotation = Quaternion.Euler(0f, Number(row[5]), 0f);
                var model = ImportModel(models["L113_" + id], slot, materials);
                model.name = "Installed " + id;
                slot.gameObject.layer = LayerMask.NameToLayer("UI");
                var target = slot.gameObject.AddComponent<SparkLevel113Footprint>();
                target.level = board;
                target.index = i;
                Bounds extent = SparkEquipmentWorkbenchFit.MeasureArt(model.transform, slot.worldToLocalMatrix);
                var hit = slot.gameObject.AddComponent<BoxCollider>();
                hit.size = new Vector3(Mathf.Max(.006f, extent.size.x), Mathf.Max(.003f, extent.size.y), Mathf.Max(.006f, extent.size.z));
                hit.center = extent.center;
                model.SetActive(false);
                return new SparkLevel113Process.Part { label = id, id = "L113_" + id, footprint = slot, model = model };
            }).ToArray();
            int Part(string name)
            {
                int index = Array.FindIndex(board.parts, part => part.label == name);
                if (index < 0) throw new InvalidOperationException("Missing original footprint " + name);
                return index;
            }
            board.jumperPart = Part("W233");
            board.serviceParts = new[] { "U6", "U7", "Y1", "U15", "Q1" }.Select(Part).ToArray();
        }

        private static float Number(string number) => float.Parse(number, CultureInfo.InvariantCulture);

        private static GameObject ImportModel(string[] row, Transform parent, Dictionary<string, Material> materials)
        {
            string path = Kit + "/Models_OBJ/" + row[6];
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (!asset) throw new FileNotFoundException("Missing original RDX-21 geometry " + path);
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(asset, parent);
            instance.name = row[1];
            foreach (var collider in instance.GetComponentsInChildren<Collider>(true)) collider.enabled = false;
            foreach (var renderer in instance.GetComponentsInChildren<MeshRenderer>(true))
            {
                const string tag = "__PSMAT__";
                int split = renderer.name.LastIndexOf(tag, StringComparison.Ordinal);
                var imported = renderer.sharedMaterials;
                var remapped = imported.Select(material =>
                {
                    string key = split < 0 ? material ? material.name : "" : renderer.name.Substring(split + tag.Length);
                    if (!materials.TryGetValue(key, out var surface))
                        throw new InvalidOperationException("No original PBR family " + key + " for " + renderer.name);
                    return surface;
                }).ToArray();
                renderer.sharedMaterials = remapped;
            }
            return instance;
        }
    }
}

