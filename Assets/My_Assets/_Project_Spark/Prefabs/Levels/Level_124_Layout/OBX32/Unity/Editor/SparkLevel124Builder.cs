using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using ProjectSpark.Gameplay;
using ProjectSpark.Gameplay.OrbitalBastion;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace ProjectSpark.EditorTools.OrbitalBastion
{
    [InitializeOnLoad]
    internal static partial class SparkLevel124Builder
    {
        internal const string Folder = "Assets/My_Assets/_Project_Spark/Prefabs/Levels/Level_124_Layout";
        internal const string Prefab = Folder + "/Level_124_Layout.prefab";
        internal const string Definition = Folder + "/Level_124.asset";
        internal const string PreviewScene = Folder + "/Level_124_Layout.unity";
        internal const string Scene = "Assets/My_Assets/_Project_Spark/Scenes/MainMenu/MainMenu.unity";
        private const string Kit = Folder + "/OBX32";
        private const string Catalog = "Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Levels/LevelLayoutCatalog/SparkLevelLayoutCatalog.asset";
        private const string Report = "Logs/Level124Builder.txt";

        static SparkLevel124Builder() => EditorApplication.update += Poll;

        private static void Poll()
        {
            const string request = "Logs/Level124Builder.request";
            if (!File.Exists(request) || EditorApplication.isPlayingOrWillChangePlaymode ||
                EditorApplication.isCompiling || EditorApplication.isUpdating) return;
            File.Delete(request);
            try { Build(); }
            catch (Exception error) { File.WriteAllText(Report, error.ToString()); Debug.LogException(error); }
        }

        [MenuItem("Tools/Project Spark/Levels/Create Level 124")]
        internal static void Build()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            using (var sha = SHA256.Create())
            {
                string hash = BitConverter.ToString(sha.ComputeHash(File.ReadAllBytes(Kit + "/Data/ModelManifest.csv")))
                    .Replace("-", "").ToLowerInvariant();
                if (hash != SparkLevel124Process.ManifestSha256)
                    throw new InvalidOperationException("OBX-32 source manifest does not match the training revision.");
            }
            var placements = Rows("ComponentPlacements.csv");
            var plans = Rows("CopperRoutes_416_REQUIRES_SCHEMATIC.csv");
            var vias = Rows("ViaTasks_164_REQUIRES_SCHEMATIC.csv");
            var models = Rows("ModelManifest.csv").ToDictionary(row => row[1], StringComparer.Ordinal);
            if (placements.Length != SparkLevel124Process.PartCount || placements.Select(row => row[0]).Distinct().Count() != placements.Length ||
                models.Count != 384 || models.Values.Any(row => row[4] != "OBX-32" ||
                    !File.Exists(Kit + "/Models_OBJ/" + row[5]) || !File.Exists(Kit + "/Models_GLB/" + row[6])) || plans.Length != SparkLevel124Process.RouteCount || vias.Length != SparkLevel124Process.ViaCount ||
                plans.Any(row => row[2] != "UNASSIGNED") || vias.Any(row => row[1] != "UNASSIGNED"))
                throw new InvalidOperationException("The OBX-32 kit or task IDs differ from the training revision.");

            var materials = AssetDatabase.FindAssets("t:Material", new[] { Folder + "/URP_PBR_Materials" })
                .Select(id => AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(id)))
                .ToDictionary(material => material.name.Replace("L124_", ""), StringComparer.Ordinal);
            if (materials.Count != 46 || materials.Values.Any(material => !material.GetTexture("_BaseMap") ||
                !material.GetTexture("_BumpMap") || !material.GetTexture("_MetallicGlossMap") || !material.GetTexture("_OcclusionMap")))
                throw new InvalidOperationException("Expected all 46 complete OBX-32 PBR surface families.");
            // Keep the authored maps while controlling specular glare under the workshop lights.
            var mask = materials["mask"];
            mask.SetFloat("_Smoothness", .42f);
            mask.SetFloat("_BumpScale", .66f);
            AssetDatabase.SaveAssetIfDirty(mask);

            bool exists = File.Exists(Prefab);
            var scratch = exists ? default : EditorSceneManager.NewPreviewScene();
            var root = exists ? PrefabUtility.LoadPrefabContents(Prefab) : new GameObject("L124_OBX32_THE_ORBITAL_BASTION");
            if (!exists) SceneManager.MoveGameObjectToScene(root, scratch);
            try
            {
                while (root.transform.childCount > 0) Object.DestroyImmediate(root.transform.GetChild(0).gameObject);
                if (root.TryGetComponent<ProjectSpark.Level124.SparkL124LayoutMarker>(out var kitMarker)) Object.DestroyImmediate(kitMarker);
                GameObjectUtility.RemoveMonoBehavioursWithMissingScript(root);
                if (root.TryGetComponent<SparkLevel124Process>(out var previous)) Object.DestroyImmediate(previous);
                var board = root.AddComponent<SparkLevel124Process>();
                if (!root.TryGetComponent<SparkLevelLayout>(out board.layout)) board.layout = root.AddComponent<SparkLevelLayout>();
                if (!root.TryGetComponent<SparkLevelSceneBindings>(out board.bindings)) board.bindings = root.AddComponent<SparkLevelSceneBindings>();

                board.fixture = Child(root.transform, "OBX-32 playable workbench");
                board.boardArt = Child(board.fixture, "Original 600 × 460 mm board");
                ImportModel(models["L124_PCB_Main"], board.boardArt, materials);
                board.referenceLayers = Child(board.fixture, "40 visual inspection layers");
                for (int layer = 1; layer <= SparkLevel124Process.LayerCount; layer++)
                    ImportModel(models[$"L124_L{layer:00}_InspectionCopper"], board.referenceLayers, materials).name = $"L{layer:00}_Reference";
                board.referenceLayers.gameObject.SetActive(false);
                board.assemblyArt = Child(board.fixture, "Source structural artwork").gameObject;
                const bool reflectImportedX = true;
                board.boardArt.localScale = new Vector3(-1f, 1f, 1f);
                board.referenceLayers.localScale = new Vector3(-1f, 1f, 1f);
                foreach (var row in models.Values.Where(row => row[3] == "structural" && row[2] is not ("layer" or "board")))
                {
                    var graphic = ImportModel(row, board.assemblyArt.transform, materials);
                    graphic.transform.localPosition = new Vector3(Number(row[7]), Number(row[8]), Number(row[9]));
                    graphic.transform.localScale = new Vector3(-1f, 1f, 1f);
                    graphic.SetActive(row[2] is "heatsink" or "shield");
                }
                var reference = ImportModel(new[] { "", "Assembled visual reference (inactive)", "", "", "",
                    "385_L124_AssembledReference.obj" }, board.fixture, materials);
                reference.SetActive(false);
                ImportParts(board, Child(board.fixture, "328 original footprints"), placements, models, materials, reflectImportedX);
                BuildFootprintLegend(board);

                var graph = new Circuit(Child(board.fixture, "OBX32-EDU-1 native training circuit"));
                List<string> mapping = ConfigureGraph(board, graph, plans, vias.Select(row => row[0]), materials);
                board.fixture.localScale = Vector3.one * (10.13f / .600f);
                var definition = BuildDefinition();
                Bind(board);
                Set(board.layout, ("levelDefinition", definition), ("sceneBindings", board.bindings));
                var saved = PrefabUtility.SaveAsPrefabAsset(root, Prefab);
                if (!saved || saved.GetComponent<SparkLevelLayout>().LevelDefinition != definition)
                    throw new InvalidOperationException("The playable OBX-32 prefab could not be reloaded.");

                string map = Kit + "/Data/OBX32_EDU_1_Training_Routes.csv";
                File.WriteAllLines(map, mapping);
                AssetDatabase.ImportAsset(map);
                AssetDatabase.SaveAssets();
                Register(definition, saved.GetComponent<SparkLevelLayout>());
                RefreshPreview(saved);
                File.WriteAllText(Report, "BUILT OBX32-EDU-1. Native campaign prefab and catalog registration verified." + Environment.NewLine);
            }
            finally
            {
                if (exists) PrefabUtility.UnloadPrefabContents(root);
                else EditorSceneManager.ClosePreviewScene(scratch);
            }
        }

        private static void RefreshPreview(GameObject prefab)
        {
            var scene = SceneManager.GetSceneByPath(PreviewScene);
            bool opened = !scene.isLoaded;
            if (opened)
                scene = File.Exists(PreviewScene) ? EditorSceneManager.OpenScene(PreviewScene, OpenSceneMode.Additive) :
                    EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            try
            {
                if (scene.isDirty && File.Exists(PreviewScene))
                {
                    Debug.LogWarning("Level 124's preview scene has unsaved changes. The campaign prefab was saved.");
                    return;
                }
                var scaffold = scene.GetRootGameObjects()
                    .SingleOrDefault(item => PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(item) == Prefab);
                Vector3 position = scaffold ? scaffold.transform.position : Vector3.zero;
                Quaternion rotation = scaffold ? scaffold.transform.rotation : Quaternion.identity;
                if (scaffold) Object.DestroyImmediate(scaffold);
                var preview = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
                preview.name = "OBX-32 campaign prefab preview";
                preview.transform.SetPositionAndRotation(position, rotation);
                preview.GetComponent<SparkLevel124Process>().fixture.localScale = Vector3.one;
                if (!EditorSceneManager.SaveScene(scene, PreviewScene))
                    throw new InvalidOperationException("Could not save the linked Level 124 preview.");
            }
            finally { if (opened) EditorSceneManager.CloseScene(scene, true); }
        }

        private static string[][] Rows(string file) => File.ReadLines(Kit + "/Data/" + file).Skip(1)
            .Where(line => !string.IsNullOrWhiteSpace(line)).Select(line => line.Split(',')).ToArray();

        private static void ImportParts(SparkLevel124Process board, Transform parent, string[][] placements,
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
                var target = slot.gameObject.AddComponent<SparkLevel124Footprint>();
                target.level = board;
                target.index = i;
                Bounds extent = SparkEquipmentWorkbenchFit.MeasureArt(model.transform, slot.worldToLocalMatrix);
                var hit = slot.gameObject.AddComponent<BoxCollider>();
                hit.size = new Vector3(Mathf.Max(.006f, extent.size.x), Mathf.Max(.003f, extent.size.y), Mathf.Max(.006f, extent.size.z));
                hit.center = extent.center;
                model.SetActive(false);
                return new SparkLevel124Process.Part { label = id, id = "L124_" + id, footprint = slot, model = model };
            }).ToArray();
            int Part(string name)
            {
                int index = Array.FindIndex(board.parts, part => part.label == name);
                if (index < 0) throw new InvalidOperationException("Missing original footprint " + name);
                return index;
            }
            board.jumperPart = Part("W417");
            board.wheelPart = Part("J3");
            board.trackerPart = Part("U7");
            board.tachPart = Part("SN4");
        }

        private static float Number(string number) => float.Parse(number, CultureInfo.InvariantCulture);

        private static GameObject ImportModel(string[] row, Transform parent, Dictionary<string, Material> materials)
        {
            string path = Kit + "/Models_OBJ/" + row[5];
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (!asset) throw new FileNotFoundException("Missing original OBX-32 geometry " + path);
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
                        raw.Equals("L124_" + family, StringComparison.OrdinalIgnoreCase) ||
                        raw.StartsWith("L124_" + family + " ", StringComparison.OrdinalIgnoreCase) ||
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
