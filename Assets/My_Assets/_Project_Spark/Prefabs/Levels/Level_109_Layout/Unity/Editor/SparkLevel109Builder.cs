using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using ProjectSpark.AutoLevelGenerator;
using ProjectSpark.Gameplay;
using ProjectSpark.Gameplay.ThermalEndurance;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ProjectSpark.EditorTools.ThermalEndurance
{
    [InitializeOnLoad]
    internal static partial class SparkLevel109Builder
    {
        internal const string Folder = "Assets/My_Assets/_Project_Spark/Prefabs/Levels/Level_109_Layout";
        internal const string Prefab = Folder + "/Level_109_Layout.prefab";
        internal const string Definition = Folder + "/Level_109.asset";
        internal const string Scene = "Assets/My_Assets/_Project_Spark/Scenes/MainMenu/MainMenu.unity";
        private const string Source = "Assets/ProjectSpark/Level109/Data/";
        private const string Catalog = "Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Levels/LevelLayoutCatalog/SparkLevelLayoutCatalog.asset";
        private const string Report = "Logs/Level109Builder.txt";
        private const string CircuitRoot = "TRX17-EDU-1 training circuit";

        static SparkLevel109Builder() => EditorApplication.update += Poll;

        private static void Poll()
        {
            const string request = "Logs/Level109Builder.request";
            if (!File.Exists(request) || EditorApplication.isPlayingOrWillChangePlaymode ||
                EditorApplication.isCompiling || EditorApplication.isUpdating) return;
            File.Delete(request);
            try { Build(); }
            catch (Exception error) { File.WriteAllText(Report, error.ToString()); Debug.LogException(error); }
        }

        [MenuItem("Tools/Project Spark/Levels/Create Level 109")]
        internal static void Build()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            var placements = Rows("L109_Components_124.csv");
            var routes = Rows("L109_Routes_164_PLANNING.csv");
            var vias = Rows("L109_Vias_54_PLANNING.csv");
            var models = Rows("L109_Models_160.csv").ToDictionary(row => row[0], StringComparer.Ordinal);
            if (placements.Length != SparkLevel109Process.PartCount || placements.Select(row => row[0]).Distinct().Count() != placements.Length ||
                models.Count != 160 || routes.Length != SparkLevel109Process.RouteCount ||
                vias.Length != SparkLevel109Process.ViaCount || routes.Any(row => row[2] != "UNASSIGNED") ||
                vias.Any(row => row[1] != "UNASSIGNED"))
                throw new InvalidOperationException("The TRX-17 art or task source has changed.");

            var root = PrefabUtility.LoadPrefabContents(Prefab);
            try
            {
                if (root.TryGetComponent<SparkLevel109Process>(out var previous)) Object.DestroyImmediate(previous);
                var board = root.AddComponent<SparkLevel109Process>();
                if (!root.TryGetComponent<SparkLevelLayout>(out board.layout)) board.layout = root.AddComponent<SparkLevelLayout>();
                if (!root.TryGetComponent<SparkLevelSceneBindings>(out board.bindings)) board.bindings = root.AddComponent<SparkLevelSceneBindings>();
                foreach (var node in root.GetComponentsInChildren<Transform>(true))
                {
                    if (!node) continue;
                    if (PrefabUtility.IsAnyPrefabInstanceRoot(node.gameObject) &&
                        PrefabUtility.GetPrefabInstanceStatus(node.gameObject) == PrefabInstanceStatus.MissingAsset)
                    {
                        Object.DestroyImmediate(node.gameObject);
                        continue;
                    }
                    GameObjectUtility.RemoveMonoBehavioursWithMissingScript(node.gameObject);
                }
                board.fixture = root.GetComponent<SparkAutoL109WorkshopFit>().ModelScaleRoot;
                board.fixture.localScale = Vector3.one;
                board.fixture.localPosition = Vector3.zero;
                var modelSpace = board.fixture.Find("L109_NativeFBXModelSpace");
                modelSpace.localScale = Vector3.one * .001f;
                board.boardArt = modelSpace.Find("L109_BoardArtRoot");
                board.referenceLayers = modelSpace.Find("L109_CopperLayers_10_VisualOnly");
                board.referenceLayers.gameObject.SetActive(false);
                modelSpace.Find("L109_CompleteAssembledReference_NoProgress").gameObject.SetActive(false);
                var materials = AssetDatabase.FindAssets("t:Material", new[] { Folder + "/URP_PBR_Materials" })
                    .Select(guid => AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guid)))
                    .ToDictionary(mat => mat.name.Replace("_AutoPBR", ""), StringComparer.Ordinal);
                BindSurfaceTextures(materials);
                EnsureModel(models["L109_TRX17_EmptyPCB"], board.boardArt, "PCB", materials);
                for (int layer = 1; layer <= SparkLevel109Process.LayerCount; layer++)
                    EnsureModel(models[$"L109_Layer{layer:00}_Reference"], board.referenceLayers, "COPPER", materials);
                ImportParts(board, modelSpace.Find("L109_ComponentSlots_124_UNINSTALLED"), placements, models, materials);
                var cooling = modelSpace.Find("L109_PlayableCooling");
                if (!cooling) cooling = Child(modelSpace, "L109_PlayableCooling");
                var service = modelSpace.Find("L109_ServiceableCooling_NotInstalled");
                board.heatsink = StageCooling(models["L109_CPU_RemovableHeatsink"], service, cooling, board, true, materials);
                board.spreader = StageCooling(models["L109_VRM_RemovableSpreader"], service, cooling, board, false, materials);
                BuildFootprintLegend(board);

                var retired = board.fixture.Find(CircuitRoot);
                if (retired) Object.DestroyImmediate(retired.gameObject);
                var graph = new Circuit(Child(board.fixture, CircuitRoot));
                var map = ConfigureGraph(board, graph, routes, vias.Select(row => row[0]), materials);
                board.fixture.localScale = Vector3.one * (10.13f / .300f);
                var definition = BuildDefinition();
                Bind(board);
                Set(board.layout, ("levelDefinition", definition), ("sceneBindings", board.bindings));
                var saved = PrefabUtility.SaveAsPrefabAsset(root, Prefab);
                if (!saved || saved.GetComponent<SparkLevelLayout>().LevelDefinition != definition)
                    throw new InvalidOperationException("The playable TRX-17 prefab could not be saved and reloaded.");

                string mapPath = Folder + "/Unity/TRX17_EDU_1_Training_Routes.csv";
                File.WriteAllLines(mapPath, map);
                AssetDatabase.ImportAsset(mapPath);
                AssetDatabase.SaveAssets();
                Register(definition, saved.GetComponent<SparkLevelLayout>());
                File.WriteAllText(Report, "BUILT TRX17-EDU-1. Thermal Endurance is registered in MainMenu and the layout catalog." + Environment.NewLine);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }

        private static string[][] Rows(string file) => File.ReadLines(Source + file).Skip(1)
            .Where(line => !string.IsNullOrWhiteSpace(line)).Select(line => line.Split(',')).ToArray();

        private static void ImportParts(SparkLevel109Process board, Transform slots, string[][] placements,
            Dictionary<string, string[]> models, Dictionary<string, Material> materials)
        {
            board.parts = placements.Select((row, i) =>
            {
                string id = row[0];
                var slot = slots.Find(id);
                var expected = Position(row);
                if (!slot || (slot.localPosition - expected).sqrMagnitude > 1e-4f)
                    throw new InvalidOperationException("Original TRX-17 placement differs for " + id);
                var retired = slot.Find("Installed component");
                if (retired) Object.DestroyImmediate(retired.gameObject);
                var model = InstantiateModel(models[id], slot, "EPOXY", materials);
                model.name = "Installed component";
                model.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                if (model.GetComponentsInChildren<MeshRenderer>(true).Length == 0)
                    throw new InvalidOperationException("No renderable geometry for " + id);
                model.SetActive(false);
                slot.gameObject.layer = LayerMask.NameToLayer("UI");
                if (!slot.TryGetComponent<SparkLevel109Footprint>(out var target)) target = slot.gameObject.AddComponent<SparkLevel109Footprint>();
                target.level = board;
                target.index = i;
                return new SparkLevel109Process.Part { label = id.Split('_')[1], id = id, footprint = slot, model = model };
            }).ToArray();
            board.jumperPart = Array.FindIndex(board.parts, part => part.id == "L109_W165_TrustedThermalJumper");
            board.mosfetPart = Array.FindIndex(board.parts, part => part.id == "L109_Q05_VRM_MOSFET");
            board.memoryPart = Array.FindIndex(board.parts, part => part.id == "L109_U3_DDR4_B");
            if (board.jumperPart < 0 || board.mosfetPart < 0 || board.memoryPart < 0)
                throw new InvalidOperationException("TRX-17 fault or safety IDs are missing.");
        }

        private static GameObject StageCooling(string[] row, Transform original, Transform cooling, SparkLevel109Process board,
            bool cpu, Dictionary<string, Material> materials)
        {
            string id = row[0];
            var part = cooling.Find(id);
            if (!part)
            {
                part = original.Find(id);
                if (part) part.SetParent(cooling, false);
            }
            part = EnsureModel(row, cooling, "ALUMINUM", materials);
            part.gameObject.layer = LayerMask.NameToLayer("UI");
            if (!part.TryGetComponent<BoxCollider>(out var hit)) hit = part.gameObject.AddComponent<BoxCollider>();
            hit.size = cpu ? new Vector3(37f, 38f, 9f) : new Vector3(42f, 20f, 6f);
            hit.center = new Vector3(0f, 0f, 2f);
            if (!part.TryGetComponent<SparkLevel109CoolingTarget>(out var target))
                target = part.gameObject.AddComponent<SparkLevel109CoolingTarget>();
            target.board = board;
            target.cpu = cpu;
            part.gameObject.SetActive(false);
            return part.gameObject;
        }

        private static Transform EnsureModel(string[] row, Transform parent, string material, Dictionary<string, Material> materials)
        {
            var existing = parent.Find(row[0]);
            if (existing && existing.GetComponentsInChildren<MeshFilter>(true).Any(filter => filter.sharedMesh)) return existing;
            if (existing) Object.DestroyImmediate(existing.gameObject);
            var model = InstantiateModel(row, parent, material, materials).transform;
            model.localPosition = Position(row);
            return model;
        }

        private static Vector3 Position(string[] row) => new(float.Parse(row[2], CultureInfo.InvariantCulture),
            float.Parse(row[3], CultureInfo.InvariantCulture), float.Parse(row[4], CultureInfo.InvariantCulture));

        private static GameObject InstantiateModel(string[] row, Transform parent, string fallback, Dictionary<string, Material> materials)
        {
            string path = "Assets/ProjectSpark/Level109/Models_OBJ_Fallback/" + Path.GetFileNameWithoutExtension(row[7]) + ".obj";
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (!asset) throw new FileNotFoundException("Imported TRX-17 art is missing: " + path);
            var model = (GameObject)PrefabUtility.InstantiatePrefab(asset, parent);
            model.name = row[0];
            foreach (var renderer in model.GetComponentsInChildren<MeshRenderer>(true))
            {
                var mesh = renderer.GetComponent<MeshFilter>()?.sharedMesh;
                if (!mesh) continue;
                string family = materials.Keys.FirstOrDefault(key =>
                    renderer.name.EndsWith("_" + key, StringComparison.OrdinalIgnoreCase) ||
                    mesh.name.EndsWith("_" + key, StringComparison.OrdinalIgnoreCase));
                renderer.sharedMaterials = Enumerable.Repeat(materials[family ?? fallback], mesh.subMeshCount).ToArray();
            }
            return model;
        }
    }
}
