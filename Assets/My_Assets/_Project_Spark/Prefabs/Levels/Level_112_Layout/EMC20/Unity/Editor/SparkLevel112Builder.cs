using System;
using System.Globalization;
using System.IO;
using System.Linq;
using ProjectSpark.Gameplay;
using ProjectSpark.Gameplay.EmcGauntlet;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ProjectSpark.EditorTools.EmcGauntlet
{
    [InitializeOnLoad]
    internal static partial class SparkLevel112Builder
    {
        internal const string Folder = "Assets/My_Assets/_Project_Spark/Prefabs/Levels/Level_112_Layout";
        internal const string Prefab = Folder + "/Level_112_Layout.prefab";
        internal const string Definition = Folder + "/Level_112.asset";
        internal const string Scene = "Assets/My_Assets/_Project_Spark/Scenes/MainMenu/MainMenu.unity";
        private const string Data = Folder + "/EMC20/Data/";
        private const string CircuitRoot = "EMC20-EDU-1 training circuit";
        private const string Catalog = "Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Levels/LevelLayoutCatalog/SparkLevelLayoutCatalog.asset";
        private const string Report = "Logs/Level112Builder.txt";

        static SparkLevel112Builder() => EditorApplication.update += Poll;

        private static void Poll()
        {
            const string request = "Logs/Level112Builder.request";
            if (!File.Exists(request) || EditorApplication.isPlayingOrWillChangePlaymode ||
                EditorApplication.isCompiling || EditorApplication.isUpdating) return;
            File.Delete(request);
            try { Build(); }
            catch (Exception error) { File.WriteAllText(Report, error.ToString()); Debug.LogException(error); }
        }

        [MenuItem("Tools/Project Spark/Levels/Create Level 112")]
        internal static void Build()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            var placements = Rows("L112_ComponentPlacements.csv");
            var routes = Rows("L112_CopperRoutes_208_SCHEMATIC_REQUIRED.csv");
            var vias = Rows("L112_ViaTasks_68_SCHEMATIC_REQUIRED.csv");
            if (placements.Length != SparkLevel112Process.PartCount || routes.Length != SparkLevel112Process.RouteCount ||
                vias.Length != SparkLevel112Process.ViaCount || routes.Any(row => row[2] != "UNASSIGNED") ||
                vias.Any(row => row[1] != "UNASSIGNED"))
                throw new InvalidOperationException("The EMC-20 source manifest has changed.");

            var root = PrefabUtility.LoadPrefabContents(Prefab);
            try
            {
                if (root.TryGetComponent<SparkLevel112Process>(out var previous)) Object.DestroyImmediate(previous);
                var board = root.AddComponent<SparkLevel112Process>();
                if (!root.TryGetComponent<SparkLevelLayout>(out board.layout)) board.layout = root.AddComponent<SparkLevelLayout>();
                if (!root.TryGetComponent<SparkLevelSceneBindings>(out board.bindings)) board.bindings = root.AddComponent<SparkLevelSceneBindings>();
                foreach (var node in root.GetComponentsInChildren<Transform>(true))
                    GameObjectUtility.RemoveMonoBehavioursWithMissingScript(node.gameObject);
                board.fixture = root.transform.Find("ArtRoot");
                board.fixture.localScale = Vector3.one;
                board.boardArt = board.fixture.Find("EmptyPCBPhysicalMetres");
                board.referenceLayers = board.fixture.Find("16LayerInspectionArtwork_DISABLED");
                board.referenceLayers.gameObject.SetActive(false);
                var showcase = root.transform.Find("AssembledShowcaseReference_DISABLED_NEVER_GAMEPLAY");
                showcase.gameObject.SetActive(false);
                var retire = board.fixture.Find(CircuitRoot);
                if (retire) Object.DestroyImmediate(retire.gameObject);
                var slotRoot = board.fixture.Find("Playable footprints");
                if (!slotRoot) slotRoot = Child(board.fixture, "Playable footprints");
                var authoring = root.transform.Find("GameplayAuthoringRoot");
                ImportParts(board, slotRoot, authoring.Find("152_UninstalledComponentTargets"), placements);
                authoring.gameObject.SetActive(false);
                root.transform.Find("OPTIONAL_EducationalEMCSpectrumDemo_NONCERTIFIABLE").gameObject.SetActive(false);
                root.transform.Find("ProjectSparkIntegrationContracts_UNBOUND").gameObject.SetActive(false);
                StageShields(board);
                BuildFootprintLegend(board);

                var mats = AssetDatabase.FindAssets("t:Material", new[] { Folder + "/URP_PBR_Materials" })
                    .Select(guid => AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guid)))
                    .ToDictionary(material => material.name.Replace("L112_", ""), StringComparer.Ordinal);
                var graph = new Circuit(Child(board.fixture, CircuitRoot));
                var map = ConfigureGraph(board, graph, routes, vias.Select(row => row[0]), mats);
                board.fixture.localScale = Vector3.one * (10.13f / .360f);
                var definition = BuildDefinition();
                Bind(board);
                Set(board.layout, ("levelDefinition", definition), ("sceneBindings", board.bindings));
                var asset = PrefabUtility.SaveAsPrefabAsset(root, Prefab);
                if (!asset || asset.GetComponent<SparkLevelLayout>().LevelDefinition != definition)
                    throw new InvalidOperationException("Could not save and reload the playable EMC-20 prefab.");
                string routeMap = Data + "EMC20_EDU_1_Training_Routes.csv";
                File.WriteAllLines(routeMap, map);
                AssetDatabase.ImportAsset(routeMap);
                AssetDatabase.SaveAssets();
                Register(definition, asset.GetComponent<SparkLevelLayout>());
                File.WriteAllText(Report, "BUILT EMC20-EDU-1. The EMC Gauntlet is registered in MainMenu and the layout catalog." + Environment.NewLine);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }

        private static string[][] Rows(string filename) => File.ReadLines(Data + filename)
            .Skip(1).Where(line => !string.IsNullOrWhiteSpace(line)).Select(line => line.Split(',')).ToArray();

        private static void ImportParts(SparkLevel112Process board, Transform slots, Transform original, string[][] placements)
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
                var visual = AssetDatabase.LoadAssetAtPath<GameObject>(Folder + "/ComponentVisualPrefabs/L112_" + id + ".prefab");
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
                if (!slot.TryGetComponent<SparkLevel112Footprint>(out var target)) target = slot.gameObject.AddComponent<SparkLevel112Footprint>();
                target.level = board;
                target.index = i;
                return new SparkLevel112Process.Part { label = id, footprint = slot, model = model };
            }).ToArray();
            board.jumperPart = Array.FindIndex(board.parts, part => part.label == "W209");
            board.chokePart = Array.FindIndex(board.parts, part => part.label == "L1");
        }
        private static void StageShields(SparkLevel112Process board)
        {
            var art = board.fixture.Find("OtherFactoryOnlyArtwork_DISABLED");
            var service = board.fixture.Find("Removable shield cans");
            if (!service) service = Child(board.fixture, "Removable shield cans");
            board.shields = new GameObject[3];
            var mounts = new[]
            {
                new Vector3(-.058f, .00155f, .028f),
                new Vector3(.058f, .00155f, -.055f),
                new Vector3(-.058f, .00155f, -.027f)
            };
            var shieldSurfaces = new System.Collections.Generic.List<Renderer>();
            for (int i = 0; i < board.shields.Length; i++)
            {
                string name = "VISUAL_ONLY_ShieldCan_" + (i + 1);
                var part = service.Find(name);
                if (!part)
                {
                    part = art.Find(name);
                    if (!part) throw new InvalidOperationException("Missing shield artwork " + name);
                    part.SetParent(service, false);
                }
                part.localPosition = mounts[i];
                part.gameObject.layer = LayerMask.NameToLayer("UI");
                if (!part.TryGetComponent<BoxCollider>(out var hit)) hit = part.gameObject.AddComponent<BoxCollider>();
                hit.size = new Vector3(.0526f, .010f, .0386f);
                hit.center = Vector3.up * .0045f;
                if (!part.TryGetComponent<SparkLevel112ShieldTarget>(out var target)) target = part.gameObject.AddComponent<SparkLevel112ShieldTarget>();
                target.board = board;
                target.index = i;
                part.gameObject.SetActive(false);
                board.shields[i] = part.gameObject;
                shieldSurfaces.AddRange(part.GetComponentsInChildren<MeshRenderer>(true));
            }
            board.shieldSurfaces = shieldSurfaces.ToArray();
        }

    }
}
