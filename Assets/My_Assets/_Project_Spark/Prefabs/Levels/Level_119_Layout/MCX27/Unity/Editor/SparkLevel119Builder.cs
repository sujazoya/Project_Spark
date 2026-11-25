using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using ProjectSpark.Gameplay;
using ProjectSpark.Gameplay.MotionCitadel;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace ProjectSpark.EditorTools.MotionCitadel
{
    [InitializeOnLoad]
    internal static partial class SparkLevel119Builder
    {
        internal const string Folder = "Assets/My_Assets/_Project_Spark/Prefabs/Levels/Level_119_Layout";
        internal const string Prefab = Folder + "/Level_119_Layout.prefab";
        internal const string Definition = Folder + "/Level_119.asset";
        internal const string PreviewScene = Folder + "/Level_119_Layout.unity";
        internal const string Scene = "Assets/My_Assets/_Project_Spark/Scenes/MainMenu/MainMenu.unity";
        private const string Kit = "Assets/ProjectSpark/Level119/MCX27";
        private const string Catalog = "Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Levels/LevelLayoutCatalog/SparkLevelLayoutCatalog.asset";
        private const string Report = "Logs/Level119Builder.txt";

        static SparkLevel119Builder() => EditorApplication.update += Poll;

        private static void Poll()
        {
            const string request = "Logs/Level119Builder.request";
            if (!File.Exists(request) || EditorApplication.isPlayingOrWillChangePlaymode ||
                EditorApplication.isCompiling || EditorApplication.isUpdating) return;
            File.Delete(request);
            try { Build(); }
            catch (Exception error) { File.WriteAllText(Report, error.ToString()); Debug.LogException(error); }
        }

        [MenuItem("Tools/Project Spark/Levels/Create Level 119")]
        internal static void Build()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            using (var sha = SHA256.Create())
            {
                string hash = BitConverter.ToString(sha.ComputeHash(File.ReadAllBytes(Kit + "/Data/ModelManifest.csv")))
                    .Replace("-", "").ToLowerInvariant();
                if (hash != SparkLevel119Process.ManifestSha256)
                    throw new InvalidOperationException("MCX-27 source manifest does not match the training revision.");
            }
            var placements = Rows("ComponentPlacements.csv");
            var plans = Rows("CopperRoutes_336_REQUIRES_SCHEMATIC.csv");
            var vias = Rows("ViaTasks_124_REQUIRES_SCHEMATIC.csv");
            var models = Rows("ModelManifest.csv").ToDictionary(row => row[1], StringComparer.Ordinal);
            if (placements.Length != SparkLevel119Process.PartCount || placements.Select(row => row[0]).Distinct().Count() != placements.Length ||
                models.Count != 304 || models.Values.Any(row => row[4] != "MCX-27" ||
                    !File.Exists(Kit + "/Models_OBJ/" + row[5]) || !File.Exists(Kit + "/Models_GLB/" + row[6])) || plans.Length != SparkLevel119Process.RouteCount || vias.Length != SparkLevel119Process.ViaCount ||
                plans.Any(row => row[2] != "UNASSIGNED") || vias.Any(row => row[1] != "UNASSIGNED"))
                throw new InvalidOperationException("The MCX-27 kit or task IDs differ from the training revision.");

            var materials = AssetDatabase.FindAssets("t:Material", new[] { Folder + "/URP_PBR_Materials" })
                .Select(id => AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(id)))
                .ToDictionary(material => material.name.Replace("L119_", ""), StringComparer.Ordinal);
            if (materials.Count != 36 || materials.Values.Any(material => !material.GetTexture("_BaseMap") ||
                !material.GetTexture("_BumpMap") || !material.GetTexture("_MetallicGlossMap") || !material.GetTexture("_OcclusionMap")))
                throw new InvalidOperationException("Expected all 36 complete MCX-27 PBR surface families.");
            // Keep the authored maps while controlling specular glare under the workshop lights.
            var mask = materials["solder_mask"];
            mask.SetFloat("_Smoothness", .54f);
            mask.SetFloat("_BumpScale", .72f);
            AssetDatabase.SaveAssetIfDirty(mask);
            foreach (var (family, smoothness) in new[]
            {
                ("heat_sink", .36f), ("aluminum", .49f), ("brushed_steel", .58f),
                ("solder", .56f), ("tin", .57f), ("nickel", .60f)
            })
            {
                var surface = materials[family];
                surface.SetFloat("_Smoothness", smoothness);
                AssetDatabase.SaveAssetIfDirty(surface);
            }

            var root = PrefabUtility.LoadPrefabContents(Prefab);
            try
            {
                while (root.transform.childCount > 0) Object.DestroyImmediate(root.transform.GetChild(0).gameObject);
                GameObjectUtility.RemoveMonoBehavioursWithMissingScript(root);
                if (root.TryGetComponent<SparkLevel119Process>(out var previous)) Object.DestroyImmediate(previous);
                var board = root.AddComponent<SparkLevel119Process>();
                if (!root.TryGetComponent<SparkLevelLayout>(out board.layout)) board.layout = root.AddComponent<SparkLevelLayout>();
                if (!root.TryGetComponent<SparkLevelSceneBindings>(out board.bindings)) board.bindings = root.AddComponent<SparkLevelSceneBindings>();

                board.fixture = Child(root.transform, "MCX-27 playable workbench");
                board.boardArt = Child(board.fixture, "Original 500 × 360 mm board");
                ImportModel(models["L119_PCB_Main"], board.boardArt, materials);
                board.referenceLayers = Child(board.fixture, "30 visual inspection layers");
                for (int layer = 1; layer <= SparkLevel119Process.LayerCount; layer++)
                    ImportModel(models[$"L119_L{layer}_LayerReference"], board.referenceLayers, materials).name = $"L{layer:00}_Reference";
                board.referenceLayers.gameObject.SetActive(false);
                board.assemblyArt = Child(board.fixture, "Source structural artwork").gameObject;
                foreach (var row in models.Values.Where(row => row[3] == "structural_visual" &&
                    row[1] != "L119_PCB_Main" && !row[1].EndsWith("_LayerReference", StringComparison.Ordinal)))
                {
                    var graphic = ImportModel(row, board.assemblyArt.transform, materials);
                    graphic.SetActive(row[1] is "L119_PowerStage_HeatsinkLeft" or "L119_PowerStage_HeatsinkRight" or
                        "L119_IsolationBarrier_Marking" or "L119_MotorUVW_TerminalGuard" or "L119_DualSTO_GuardZone");
                }
                var handednessIndicator = board.assemblyArt.transform.Find("L119_PowerStage_HeatsinkLeft");
                bool reflectImportedX = SparkEquipmentWorkbenchFit.MeasureArt(handednessIndicator,
                    board.fixture.worldToLocalMatrix).center.x > 0f;
                if (reflectImportedX)
                {
                    var handedness = new Vector3(-1f, 1f, 1f);
                    board.boardArt.localScale = handedness;
                    board.referenceLayers.localScale = handedness;
                    board.assemblyArt.transform.localScale = handedness;
                }
                var reference = ImportModel(new[] { "", "Assembled visual reference (inactive)", "", "", "",
                    "305_L119_AssembledReference.obj" }, board.fixture, materials);
                reference.SetActive(false);
                ImportParts(board, Child(board.fixture, "248 original footprints"), placements, models, materials, reflectImportedX);
                BuildFootprintLegend(board);

                var graph = new Circuit(Child(board.fixture, "MCX27-EDU-1 native training circuit"));
                List<string> mapping = ConfigureGraph(board, graph, plans, vias.Select(row => row[0]), materials);
                board.fixture.localScale = Vector3.one * (10.13f / .500f);
                var definition = BuildDefinition();
                Bind(board);
                Set(board.layout, ("levelDefinition", definition), ("sceneBindings", board.bindings));
                var saved = PrefabUtility.SaveAsPrefabAsset(root, Prefab);
                if (!saved || saved.GetComponent<SparkLevelLayout>().LevelDefinition != definition)
                    throw new InvalidOperationException("The playable MCX-27 prefab could not be reloaded.");

                string map = Kit + "/Data/MCX27_EDU_1_Training_Routes.csv";
                File.WriteAllLines(map, mapping);
                AssetDatabase.ImportAsset(map);
                AssetDatabase.SaveAssets();
                Register(definition, saved.GetComponent<SparkLevelLayout>());
                RefreshPreview(saved);
                File.WriteAllText(Report, "BUILT MCX27-EDU-1. Native campaign prefab and catalog registration verified." + Environment.NewLine);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }

        private static void RefreshPreview(GameObject prefab)
        {
            var scene = SceneManager.GetSceneByPath(PreviewScene);
            bool opened = !scene.isLoaded;
            if (opened) scene = EditorSceneManager.OpenScene(PreviewScene, OpenSceneMode.Additive);
            try
            {
                var scaffold = scene.GetRootGameObjects()
                    .SingleOrDefault(item => item.name == "L119_MCX27_THE_MOTION_CITADEL" ||
                        PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(item) == Prefab);
                if (!scaffold) return;
                if (scene.isDirty)
                {
                    Debug.LogWarning("Level 119's preview scene has unsaved changes. The playable campaign prefab was saved.");
                    return;
                }
                var preview = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
                preview.name = "MCX-27 campaign prefab preview";
                preview.transform.SetPositionAndRotation(scaffold.transform.position, scaffold.transform.rotation);
                preview.GetComponent<SparkLevel119Process>().fixture.localScale = Vector3.one;
                Object.DestroyImmediate(scaffold);
                if (!EditorSceneManager.SaveScene(scene))
                    throw new InvalidOperationException("Could not save the linked Level 119 preview.");
            }
            finally { if (opened) EditorSceneManager.CloseScene(scene, true); }
        }

        private static string[][] Rows(string file) => File.ReadLines(Kit + "/Data/" + file).Skip(1)
            .Where(line => !string.IsNullOrWhiteSpace(line)).Select(line => line.Split(',')).ToArray();

        private static void ImportParts(SparkLevel119Process board, Transform parent, string[][] placements,
            Dictionary<string, string[]> models, Dictionary<string, Material> materials, bool reflectImportedX)
        {
            board.parts = placements.Select((row, i) =>
            {
                string id = row[0];
                Vector3 position = new(Number(row[3]), Number(row[4]), Number(row[5]));
                var slot = Child(parent, id + " original footprint", position);
                var model = ImportModel(models[row[1]], slot, materials);
                if (reflectImportedX) model.transform.localScale = new Vector3(-1f, 1f, 1f);
                model.name = "Installed " + id;
                slot.gameObject.layer = LayerMask.NameToLayer("UI");
                var target = slot.gameObject.AddComponent<SparkLevel119Footprint>();
                target.level = board;
                target.index = i;
                Bounds extent = SparkEquipmentWorkbenchFit.MeasureArt(model.transform, slot.worldToLocalMatrix);
                var hit = slot.gameObject.AddComponent<BoxCollider>();
                hit.size = new Vector3(Mathf.Max(.006f, extent.size.x), Mathf.Max(.003f, extent.size.y), Mathf.Max(.006f, extent.size.z));
                hit.center = extent.center;
                model.SetActive(false);
                return new SparkLevel119Process.Part { label = id, id = "L119_" + id, footprint = slot, model = model };
            }).ToArray();
            int Part(string name)
            {
                int index = Array.FindIndex(board.parts, part => part.label == name);
                if (index < 0) throw new InvalidOperationException("Missing original footprint " + name);
                return index;
            }
            board.jumperPart = Part("W337");
            board.gatePart = Part("U12");
            board.encoderPart = Part("U9");
            board.brakePart = Part("U14");
        }

        private static float Number(string number) => float.Parse(number, CultureInfo.InvariantCulture);

        private static GameObject ImportModel(string[] row, Transform parent, Dictionary<string, Material> materials)
        {
            string path = Kit + "/Models_OBJ/" + row[5];
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (!asset) throw new FileNotFoundException("Missing original MCX-27 geometry " + path);
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(asset, parent);
            instance.name = row[1];
            foreach (var collider in instance.GetComponentsInChildren<Collider>(true)) collider.enabled = false;
            foreach (var renderer in instance.GetComponentsInChildren<MeshRenderer>(true))
            {
                var imported = renderer.sharedMaterials;
                var remapped = imported.Select(material =>
                {
                    const string marker = "__PSMAT__";
                    int tag = renderer.name.LastIndexOf(marker, StringComparison.Ordinal);
                    string raw = tag >= 0 ? renderer.name.Substring(tag + marker.Length) :
                        material ? material.name : renderer.name;
                    string key = materials.Keys.Where(family => raw.Equals(family, StringComparison.OrdinalIgnoreCase) ||
                        raw.Equals("L119_" + family, StringComparison.OrdinalIgnoreCase) ||
                        raw.StartsWith("L119_" + family + " ", StringComparison.OrdinalIgnoreCase) ||
                        raw.EndsWith("_" + family, StringComparison.OrdinalIgnoreCase))
                        .OrderByDescending(family => family.Length).FirstOrDefault();
                    if (key == null)
                        throw new InvalidOperationException("No original PBR family " + raw + " for " + renderer.name);
                    return materials[key];
                }).ToArray();
                renderer.sharedMaterials = remapped;
            }
            return instance;
        }
    }
}
