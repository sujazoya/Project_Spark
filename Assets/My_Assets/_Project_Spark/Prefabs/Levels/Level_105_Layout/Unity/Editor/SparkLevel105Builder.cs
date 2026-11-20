using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using ProjectSpark.AutoLevelGenerator;
using ProjectSpark.Gameplay;
using ProjectSpark.Gameplay.ExpansionNexus;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ProjectSpark.EditorTools.ExpansionNexus
{
    [InitializeOnLoad]
    internal static partial class SparkLevel105Builder
    {
        internal const string Folder = "Assets/My_Assets/_Project_Spark/Prefabs/Levels/Level_105_Layout";
        internal const string Prefab = Folder + "/Level_105_Layout.prefab";
        internal const string Definition = Folder + "/Level_105.asset";
        internal const string Scene = "Assets/My_Assets/_Project_Spark/Scenes/MainMenu/MainMenu.unity";
        private const string Catalog = "Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Levels/LevelLayoutCatalog/SparkLevelLayoutCatalog.asset";
        private const string Report = "Logs/Level105Builder.txt";
        private const string CircuitRoot = "ENX13-EDU-1 training circuit";
        private const string Source = Folder + "/Assets/Data/";

        static SparkLevel105Builder() => EditorApplication.update += Poll;

        private static void Poll()
        {
            const string request = "Logs/Level105Builder.request";
            if (!File.Exists(request) || EditorApplication.isPlayingOrWillChangePlaymode ||
                EditorApplication.isCompiling || EditorApplication.isUpdating) return;
            File.Delete(request);
            try { Build(); }
            catch (Exception error) { File.WriteAllText(Report, error.ToString()); Debug.LogException(error); }
        }

        [MenuItem("Tools/Project Spark/Levels/Create Level 105")]
        internal static void Build()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            var placements = Rows("ComponentPlacements.csv");
            var manifest = Rows("ModelManifest.csv").Where(item => item[9] == "True").ToDictionary(item => item[1][5..]);
            var plans = Rows("CopperRoutes_132_REQUIRES_SCHEMATIC.csv");
            var barrels = Rows("ViaTasks_38_REQUIRES_SCHEMATIC.csv");
            if (placements.Length != SparkLevel105Process.PartCount || manifest.Count != placements.Length ||
                plans.Length != SparkLevel105Process.RouteCount || barrels.Length != SparkLevel105Process.ViaCount ||
                plans.Any(row => row[2] != "UNASSIGNED") || barrels.Any(row => row[3] != "UNASSIGNED"))
                throw new InvalidOperationException("The ENX-13 blank source or BOM has changed.");

            var root = PrefabUtility.LoadPrefabContents(Prefab);
            try
            {
                if (root.TryGetComponent<SparkLevel105Process>(out var previous)) Object.DestroyImmediate(previous);
                var board = root.AddComponent<SparkLevel105Process>();
                if (!root.TryGetComponent<SparkLevelLayout>(out board.layout)) board.layout = root.AddComponent<SparkLevelLayout>();
                if (!root.TryGetComponent<SparkLevelSceneBindings>(out board.bindings)) board.bindings = root.AddComponent<SparkLevelSceneBindings>();
                foreach (var node in root.GetComponentsInChildren<Transform>(true))
                    GameObjectUtility.RemoveMonoBehavioursWithMissingScript(node.gameObject);
                var fit = root.GetComponent<SparkAutoL105WorkshopFit>().ModelScaleRoot;
                board.fixture = fit.Find("L105_NativeMeshModelSpace");
                board.boardArt = board.fixture.Find("L105_BoardArtRoot");
                board.card = board.fixture.Find("L105_ECX4_Unseated_Deck_Rotated90");
                board.cardArt = board.card.Find("L105_ECX4_BareCard_80x35mm");
                board.cardArt.gameObject.layer = LayerMask.NameToLayer("UI");
                if (!board.cardArt.TryGetComponent<BoxCollider>(out board.cardSurface))
                    board.cardSurface = board.cardArt.gameObject.AddComponent<BoxCollider>();
                board.cardSurface.size = new Vector3(.08f, .006f, .035f);
                board.cardSurface.enabled = false;
                if (!board.cardArt.TryGetComponent<SparkLevel105CardInspector>(out var inspector))
                    inspector = board.cardArt.gameObject.AddComponent<SparkLevel105CardInspector>();
                inspector.level = board;
                board.referenceLayers = board.fixture.Find("L105_CopperLayers_8_VisualOnly");
                var reference = board.fixture.Find("L105_CompleteAssembledReference_NoProgress");
                reference.gameObject.SetActive(false);
                board.referenceLayers.gameObject.SetActive(false);
                board.parkPosition = board.card.localPosition;
                board.parkRotation = board.card.localRotation;

                var retired = board.fixture.Find(CircuitRoot);
                if (retired) Object.DestroyImmediate(retired.gameObject);
                var materials = AssetDatabase.FindAssets("t:Material", new[] { Folder + "/URP_PBR_Materials" })
                    .Select(guid => AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guid)))
                    .ToDictionary(mat => mat.name.Replace("L105_", "").Replace("_PBR", ""), StringComparer.OrdinalIgnoreCase);
                ImportParts(board, reference.Find("L105_CompleteModularComposition"), placements, manifest);
                BuildFootprintLegend(board);
                foreach (Transform item in board.fixture.Find("L105_StructuralReferences"))
                    if (item.name.Contains("LanePairs") || item.name.Contains("ClockPair") ||
                        item.name.Contains("ConnectorContacts") || item.name.Contains("SpringContacts") ||
                        item.name.Contains("GoldEdgeFingers") || item.name.Contains("DockingAnchor"))
                        item.gameObject.SetActive(false);

                var graph = new Circuit(Child(board.fixture, CircuitRoot));
                var rows = new List<string> { "task,domain,training_net,entry_layer,from,to,from_x_mm,from_z_mm,to_x_mm,to_z_mm,via,exit_layer" };
                ConfigureGraph(board, graph, plans, barrels.Select(item => item[0]), materials, rows);
                var definition = BuildDefinition();
                Bind(board);
                Set(board.layout, ("levelDefinition", definition), ("sceneBindings", board.bindings));
                var saved = PrefabUtility.SaveAsPrefabAsset(root, Prefab);
                if (!saved || saved.GetComponent<SparkLevelLayout>().LevelDefinition != definition)
                    throw new InvalidOperationException("The playable ENX-13 prefab failed to reload.");

                var routeMap = Source + "ENX13_EDU_1_Training_Routes.csv";
                File.WriteAllLines(routeMap, rows);
                AssetDatabase.ImportAsset(routeMap);
                AssetDatabase.SaveAssets();
                Register(definition, saved.GetComponent<SparkLevelLayout>());
                File.WriteAllText(Report, "BUILT ENX13-EDU-1. Registered Expansion Nexus in MainMenu and the layout catalog.\n");
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }

        private static string[][] Rows(string filename) => File.ReadLines(Source + filename)
            .Skip(1).Where(line => !string.IsNullOrWhiteSpace(line)).Select(line => line.Split(',')).ToArray();

        private static void ImportParts(SparkLevel105Process board, Transform reference, string[][] placements,
            Dictionary<string, string[]> manifest)
        {
            var main = board.fixture.Find("L105_ENX13_ComponentSlots_94_UNINSTALLED");
            var endpoint = board.card.Find("L105_ECX4_ComponentSlots_6_UNINSTALLED");
            board.parts = placements.Select((row, index) =>
            {
                var model = manifest[row[0]];
                bool onCard = row[1] == "ECX-4";
                if (model[3] != row[1]) throw new InvalidOperationException("Board assignment differs for " + row[0]);
                string name = Path.GetFileNameWithoutExtension(model[4]);
                var slot = (onCard ? endpoint : main).Find(name);
                Vector3 expected = new(float.Parse(row[2], CultureInfo.InvariantCulture), float.Parse(row[3], CultureInfo.InvariantCulture),
                    float.Parse(row[4], CultureInfo.InvariantCulture));
                if (!slot || (slot.localPosition - expected).sqrMagnitude > 1e-9f || !reference.Find(name))
                    throw new InvalidOperationException("Missing original pose or source mesh for " + row[0]);
                var retired = slot.Find("Installed component");
                if (retired) Object.DestroyImmediate(retired.gameObject);
                var visual = Object.Instantiate(reference.Find(name).gameObject, slot, false);
                visual.name = "Installed component";
                visual.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                if (visual.GetComponentsInChildren<MeshRenderer>(true).Length == 0)
                    throw new InvalidOperationException("No renderable source mesh for " + row[0]);
                visual.SetActive(false);
                slot.gameObject.layer = LayerMask.NameToLayer("UI");
                if (!slot.TryGetComponent<SparkLevel105Footprint>(out var target)) target = slot.gameObject.AddComponent<SparkLevel105Footprint>();
                target.level = board;
                target.index = index;
                return new SparkLevel105Process.Part { label = row[0], footprint = slot, model = visual, onCard = onCard };
            }).ToArray();
            if (board.parts.Count(part => part.onCard) != 6) throw new InvalidOperationException("Expected six ECX-4 parts.");
            board.jumperPart = Array.FindIndex(board.parts, part => part.label == "W133");
        }

        private static string Row(SparkLevel105Process.Route route) =>
            FormattableString.Invariant($"{route.id},{route.group},{route.net},{route.layer},{route.from.name},{route.to.name},{route.entry.x * 1000:F3},{route.entry.z * 1000:F3},{route.destination.x * 1000:F3},{route.destination.z * 1000:F3},{route.viaId},{route.exitLayer}");
    }
}
