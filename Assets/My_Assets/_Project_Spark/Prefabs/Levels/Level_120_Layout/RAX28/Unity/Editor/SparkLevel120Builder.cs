using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using ProjectSpark.Gameplay;
using ProjectSpark.Gameplay.AxisForge;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace ProjectSpark.EditorTools.AxisForge
{
    [InitializeOnLoad]
    internal static partial class SparkLevel120Builder
    {
        internal const string Folder = "Assets/My_Assets/_Project_Spark/Prefabs/Levels/Level_120_Layout";
        internal const string Prefab = Folder + "/Level_120_Layout.prefab";
        internal const string Definition = Folder + "/Level_120.asset";
        internal const string PreviewScene = Folder + "/Level_120_Layout.unity";
        internal const string Scene = "Assets/My_Assets/_Project_Spark/Scenes/MainMenu/MainMenu.unity";
        private const string Kit = Folder + "/RAX28";
        private const string Catalog = "Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Levels/LevelLayoutCatalog/SparkLevelLayoutCatalog.asset";
        private const string Report = "Logs/Level120Builder.txt";

        static SparkLevel120Builder() => EditorApplication.update += Poll;

        private static void Poll()
        {
            const string request = "Logs/Level120Builder.request";
            if (!File.Exists(request) || EditorApplication.isPlayingOrWillChangePlaymode ||
                EditorApplication.isCompiling || EditorApplication.isUpdating) return;
            File.Delete(request);
            try { Build(); }
            catch (Exception error) { File.WriteAllText(Report, error.ToString()); Debug.LogException(error); }
        }

        [MenuItem("Tools/Project Spark/Levels/Create Level 120")]
        internal static void Build()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            using (var sha = SHA256.Create())
            {
                string hash = BitConverter.ToString(sha.ComputeHash(File.ReadAllBytes(Kit + "/Data/ModelManifest.csv")))
                    .Replace("-", "").ToLowerInvariant();
                if (hash != SparkLevel120Process.ManifestSha256)
                    throw new InvalidOperationException("RAX-28 source manifest does not match the training revision.");
            }
            var placements = Rows("ComponentPlacements.csv");
            var plans = Rows("CopperRoutes_352_REQUIRES_SCHEMATIC.csv");
            var vias = Rows("ViaTasks_132_REQUIRES_SCHEMATIC.csv");
            var models = Rows("ModelManifest.csv").ToDictionary(row => row[1], StringComparer.Ordinal);
            if (placements.Length != SparkLevel120Process.PartCount || placements.Select(row => row[0]).Distinct().Count() != placements.Length ||
                models.Count != 320 || models.Values.Any(row => row[4] != "RAX-28" ||
                    !File.Exists(Kit + "/Models_OBJ/" + row[5]) || !File.Exists(Kit + "/Models_GLB/" + row[6])) || plans.Length != SparkLevel120Process.RouteCount || vias.Length != SparkLevel120Process.ViaCount ||
                plans.Any(row => row[2] != "UNASSIGNED") || vias.Any(row => row[1] != "UNASSIGNED"))
                throw new InvalidOperationException("The RAX-28 kit or task IDs differ from the training revision.");

            var materials = AssetDatabase.FindAssets("t:Material", new[] { Folder + "/URP_PBR_Materials" })
                .Select(id => AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(id)))
                .ToDictionary(material => material.name.Replace("L120_", ""), StringComparer.Ordinal);
            if (materials.Count != 38 || materials.Values.Any(material => !material.GetTexture("_BaseMap") ||
                !material.GetTexture("_BumpMap") || !material.GetTexture("_MetallicGlossMap") || !material.GetTexture("_OcclusionMap")))
                throw new InvalidOperationException("Expected all 38 complete RAX-28 PBR surface families.");
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
                if (root.TryGetComponent<SparkLevel120Process>(out var previous)) Object.DestroyImmediate(previous);
                var board = root.AddComponent<SparkLevel120Process>();
                if (!root.TryGetComponent<SparkLevelLayout>(out board.layout)) board.layout = root.AddComponent<SparkLevelLayout>();
                if (!root.TryGetComponent<SparkLevelSceneBindings>(out board.bindings)) board.bindings = root.AddComponent<SparkLevelSceneBindings>();

                board.fixture = Child(root.transform, "RAX-28 playable workbench");
                board.boardArt = Child(board.fixture, "Original 520 × 380 mm board");
                ImportModel(models["L120_PCB_Main"], board.boardArt, materials);
                board.referenceLayers = Child(board.fixture, "32 visual inspection layers");
                for (int layer = 1; layer <= SparkLevel120Process.LayerCount; layer++)
                    ImportModel(models[$"L120_L{layer}_LayerReference"], board.referenceLayers, materials).name = $"L{layer:00}_Reference";
                board.referenceLayers.gameObject.SetActive(false);
                board.assemblyArt = Child(board.fixture, "Source structural artwork").gameObject;
                foreach (var row in models.Values.Where(row => row[3] == "structural_visual" &&
                    row[1] != "L120_PCB_Main" && !row[1].EndsWith("_LayerReference", StringComparison.Ordinal)))
                {
                    var graphic = ImportModel(row, board.assemblyArt.transform, materials);
                    graphic.SetActive(row[1].EndsWith("_Heatsink", StringComparison.Ordinal) ||
                        row[1] is "L120_DualSTO_GuardZone" or "L120_A_B_C_D_E_F_JointPhaseMarks");
                }
                var sink = board.assemblyArt.transform.Find("L120_AxisA_Heatsink");
                bool reflectImportedX = SparkEquipmentWorkbenchFit.MeasureArt(sink, board.fixture.worldToLocalMatrix).center.x < 0f;
                if (reflectImportedX)
                {
                    var handedness = new Vector3(-1f, 1f, 1f);
                    board.boardArt.localScale = handedness;
                    board.referenceLayers.localScale = handedness;
                    board.assemblyArt.transform.localScale = handedness;
                }
                var reference = ImportModel(new[] { "", "Assembled visual reference (inactive)", "", "", "",
                    "321_L120_AssembledReference.obj" }, board.fixture, materials);
                reference.SetActive(false);
                ImportParts(board, Child(board.fixture, "264 original footprints"), placements, models, materials, reflectImportedX);
                BuildFootprintLegend(board);

                var graph = new Circuit(Child(board.fixture, "RAX28-EDU-1 native training circuit"));
                List<string> mapping = ConfigureGraph(board, graph, plans, vias.Select(row => row[0]), materials);
                board.fixture.localScale = Vector3.one * (10.13f / .520f);
                var definition = BuildDefinition();
                Bind(board);
                Set(board.layout, ("levelDefinition", definition), ("sceneBindings", board.bindings));
                var saved = PrefabUtility.SaveAsPrefabAsset(root, Prefab);
                if (!saved || saved.GetComponent<SparkLevelLayout>().LevelDefinition != definition)
                    throw new InvalidOperationException("The playable RAX-28 prefab could not be reloaded.");

                string map = Kit + "/Data/RAX28_EDU_1_Training_Routes.csv";
                File.WriteAllLines(map, mapping);
                AssetDatabase.ImportAsset(map);
                AssetDatabase.SaveAssets();
                Register(definition, saved.GetComponent<SparkLevelLayout>());
                RefreshPreview(saved);
                File.WriteAllText(Report, "BUILT RAX28-EDU-1. Native campaign prefab and catalog registration verified." + Environment.NewLine);
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
                    .SingleOrDefault(item => item.name == "L120_RAX28_THE_AXIS_FORGE" ||
                        PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(item) == Prefab);
                if (!scaffold) return;
                if (scene.isDirty)
                {
                    Debug.LogWarning("Level 120's preview scene has unsaved changes. The playable campaign prefab was saved.");
                    return;
                }
                var preview = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
                preview.name = "RAX-28 campaign prefab preview";
                preview.transform.SetPositionAndRotation(scaffold.transform.position, scaffold.transform.rotation);
                preview.GetComponent<SparkLevel120Process>().fixture.localScale = Vector3.one;
                Object.DestroyImmediate(scaffold);
                if (!EditorSceneManager.SaveScene(scene))
                    throw new InvalidOperationException("Could not save the linked Level 120 preview.");
            }
            finally { if (opened) EditorSceneManager.CloseScene(scene, true); }
        }

        private static string[][] Rows(string file) => File.ReadLines(Kit + "/Data/" + file).Skip(1)
            .Where(line => !string.IsNullOrWhiteSpace(line)).Select(line => line.Split(',')).ToArray();

        private static void ImportParts(SparkLevel120Process board, Transform parent, string[][] placements,
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
                var target = slot.gameObject.AddComponent<SparkLevel120Footprint>();
                target.level = board;
                target.index = i;
                Bounds extent = SparkEquipmentWorkbenchFit.MeasureArt(model.transform, slot.worldToLocalMatrix);
                var hit = slot.gameObject.AddComponent<BoxCollider>();
                hit.size = new Vector3(Mathf.Max(.006f, extent.size.x), Mathf.Max(.003f, extent.size.y), Mathf.Max(.006f, extent.size.z));
                hit.center = extent.center;
                model.SetActive(false);
                return new SparkLevel120Process.Part { label = id, id = "L120_" + id, footprint = slot, model = model };
            }).ToArray();
            int Part(string name)
            {
                int index = Array.FindIndex(board.parts, part => part.label == name);
                if (index < 0) throw new InvalidOperationException("Missing original footprint " + name);
                return index;
            }
            board.jumperPart = Part("W353");
            board.gatePart = Part("U18");
            board.encoderPart = Part("U25");
            board.thermalPart = Part("U34");
        }

        private static float Number(string number) => float.Parse(number, CultureInfo.InvariantCulture);

        private static GameObject ImportModel(string[] row, Transform parent, Dictionary<string, Material> materials)
        {
            string path = Kit + "/Models_OBJ/" + row[5];
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (!asset) throw new FileNotFoundException("Missing original RAX-28 geometry " + path);
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
                        raw.Equals("L120_" + family, StringComparison.OrdinalIgnoreCase) ||
                        raw.StartsWith("L120_" + family + " ", StringComparison.OrdinalIgnoreCase) ||
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
