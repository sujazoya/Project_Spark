using System;
using System.Globalization;
using System.IO;
using System.Linq;
using ProjectSpark.Gameplay;
using ProjectSpark.Gameplay.SignalCrucible;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ProjectSpark.EditorTools.SignalCrucible
{
    [InitializeOnLoad]
    internal static partial class SparkLevel111Builder
    {
        internal const string Folder = "Assets/My_Assets/_Project_Spark/Prefabs/Levels/Level_111_Layout";
        internal const string Prefab = Folder + "/Level_111_Layout.prefab";
        internal const string Definition = Folder + "/Level_111.asset";
        internal const string Scene = "Assets/My_Assets/_Project_Spark/Scenes/MainMenu/MainMenu.unity";
        private const string Data = Folder + "/SIX19/Data/";
        private const string CircuitRoot = "SIX19-EDU-1 training circuit";
        private const string Catalog = "Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Levels/LevelLayoutCatalog/SparkLevelLayoutCatalog.asset";
        private const string Report = "Logs/Level111Builder.txt";

        static SparkLevel111Builder() => EditorApplication.update += Poll;

        private static void Poll()
        {
            const string request = "Logs/Level111Builder.request";
            if (!File.Exists(request) || EditorApplication.isPlayingOrWillChangePlaymode ||
                EditorApplication.isCompiling || EditorApplication.isUpdating) return;
            File.Delete(request);
            try { Build(); }
            catch (Exception error) { File.WriteAllText(Report, error.ToString()); Debug.LogException(error); }
        }

        [MenuItem("Tools/Project Spark/Levels/Create Level 111")]
        internal static void Build()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            var placements = Rows("L111_ComponentPlacements.csv");
            var routes = Rows("L111_CopperRoutes_184_SCHEMATIC_REQUIRED.csv");
            var vias = Rows("L111_ViaTasks_60_SCHEMATIC_REQUIRED.csv");
            if (placements.Length != SparkLevel111Process.PartCount || routes.Length != SparkLevel111Process.RouteCount ||
                vias.Length != SparkLevel111Process.ViaCount || routes.Any(row => row[2] != "UNASSIGNED") ||
                vias.Any(row => row[1] != "UNASSIGNED"))
                throw new InvalidOperationException("The SIX-19 source manifest has changed.");

            var root = PrefabUtility.LoadPrefabContents(Prefab);
            try
            {
                if (root.TryGetComponent<SparkLevel111Process>(out var previous)) Object.DestroyImmediate(previous);
                var board = root.AddComponent<SparkLevel111Process>();
                if (!root.TryGetComponent<SparkLevelLayout>(out board.layout)) board.layout = root.AddComponent<SparkLevelLayout>();
                if (!root.TryGetComponent<SparkLevelSceneBindings>(out board.bindings)) board.bindings = root.AddComponent<SparkLevelSceneBindings>();
                foreach (var node in root.GetComponentsInChildren<Transform>(true))
                    GameObjectUtility.RemoveMonoBehavioursWithMissingScript(node.gameObject);
                board.fixture = root.transform.Find("ArtRoot");
                board.fixture.localScale = Vector3.one;
                board.boardArt = board.fixture.Find("EmptyPCBPhysicalMetres");
                board.referenceLayers = board.fixture.Find("14LayerInspectionArtwork_DISABLED");
                board.referenceLayers.gameObject.SetActive(false);
                var showcase = root.transform.Find("AssembledShowcaseReference_DISABLED_NEVER_GAMEPLAY");
                showcase.gameObject.SetActive(false);
                var retire = board.fixture.Find(CircuitRoot);
                if (retire) Object.DestroyImmediate(retire.gameObject);
                var slotRoot = board.fixture.Find("Playable footprints");
                if (!slotRoot) slotRoot = Child(board.fixture, "Playable footprints");
                var authoring = root.transform.Find("GameplayAuthoringRoot");
                ImportParts(board, slotRoot, authoring.Find("138_UninstalledComponentTargets"), placements);
                authoring.gameObject.SetActive(false);
                root.transform.Find("OPTIONAL_EducationalSignalDemo_NONCERTIFIABLE").gameObject.SetActive(false);
                root.transform.Find("ProjectSparkIntegrationContracts_UNBOUND").gameObject.SetActive(false);
                BuildFootprintLegend(board);

                var mats = AssetDatabase.FindAssets("t:Material", new[] { Folder + "/URP_PBR_Materials" })
                    .Select(guid => AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guid)))
                    .ToDictionary(material => material.name.Replace("L111_", ""), StringComparer.Ordinal);
                var graph = new Circuit(Child(board.fixture, CircuitRoot));
                var map = ConfigureGraph(board, graph, routes, vias.Select(row => row[0]), mats);
                board.fixture.localScale = Vector3.one * (10.13f / .330f);
                var definition = BuildDefinition();
                Bind(board);
                Set(board.layout, ("levelDefinition", definition), ("sceneBindings", board.bindings));
                var asset = PrefabUtility.SaveAsPrefabAsset(root, Prefab);
                if (!asset || asset.GetComponent<SparkLevelLayout>().LevelDefinition != definition)
                    throw new InvalidOperationException("Could not save and reload the playable SIX-19 prefab.");
                string routeMap = Data + "SIX19_EDU_1_Training_Routes.csv";
                File.WriteAllLines(routeMap, map);
                AssetDatabase.ImportAsset(routeMap);
                AssetDatabase.SaveAssets();
                Register(definition, asset.GetComponent<SparkLevelLayout>());
                File.WriteAllText(Report, "BUILT SIX19-EDU-1. The Signal Crucible is registered in MainMenu and the layout catalog." + Environment.NewLine);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }

        private static string[][] Rows(string filename) => File.ReadLines(Data + filename)
            .Skip(1).Where(line => !string.IsNullOrWhiteSpace(line)).Select(line => line.Split(',')).ToArray();

        private static void ImportParts(SparkLevel111Process board, Transform slots, Transform original, string[][] placements)
        {
            board.parts = placements.Select((row, i) =>
            {
                string id = row[0];
                var slot = slots.Find("SLOT_" + id);
                if (!slot)
                {
                    slot = original.Find("SLOT_" + id);
                    if (!slot) throw new InvalidOperationException("Missing original footprint " + id);
                    slot.SetParent(slots, false);
                }
                Vector3 expected = new(float.Parse(row[2], CultureInfo.InvariantCulture), float.Parse(row[3], CultureInfo.InvariantCulture),
                    float.Parse(row[4], CultureInfo.InvariantCulture));
                if ((slot.localPosition - expected).sqrMagnitude > 1e-9f)
                    throw new InvalidOperationException("Placement does not match source for " + id);
                slot.localRotation = Quaternion.Euler(0f, float.Parse(row[5], CultureInfo.InvariantCulture), 0f);
                var visual = AssetDatabase.LoadAssetAtPath<GameObject>(Folder + "/ComponentVisualPrefabs/L111_" + id + ".prefab");
                if (!visual) throw new InvalidOperationException("Missing component visual " + id);
                var old = slot.Find("Installed component");
                if (old) Object.DestroyImmediate(old.gameObject);
                var model = (GameObject)PrefabUtility.InstantiatePrefab(visual, slot);
                model.name = "Installed component";
                model.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                var nodes = model.GetComponentsInChildren<Transform>(true);
                if (nodes.Any(node => GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(node.gameObject) > 0))
                {
                    PrefabUtility.UnpackPrefabInstance(model, PrefabUnpackMode.OutermostRoot, InteractionMode.AutomatedAction);
                    foreach (var node in nodes) GameObjectUtility.RemoveMonoBehavioursWithMissingScript(node.gameObject);
                }
                if (model.GetComponentsInChildren<MeshRenderer>(true).Length == 0)
                    throw new InvalidOperationException("No renderable geometry for " + id);
                model.SetActive(false);
                slot.gameObject.layer = LayerMask.NameToLayer("UI");
                if (!slot.TryGetComponent<SparkLevel111Footprint>(out var target)) target = slot.gameObject.AddComponent<SparkLevel111Footprint>();
                target.level = board;
                target.index = i;
                return new SparkLevel111Process.Part { label = id, footprint = slot, model = model };
            }).ToArray();
            board.jumperPart = Array.FindIndex(board.parts, part => part.label == "W185");
            board.clockPart = Array.FindIndex(board.parts, part => part.label == "U5");
            board.retimerPart = Array.FindIndex(board.parts, part => part.label == "U8");
        }
    }
}
