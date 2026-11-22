using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using ProjectSpark.Gameplay;
using ProjectSpark.Gameplay.RecoveryLab;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace ProjectSpark.Level130.Editor
{
    [InitializeOnLoad]
    internal static partial class SparkL130PlayableBuilder
    {
        internal const string Folder = "Assets/My_Assets/_Project_Spark/Prefabs/Levels/Level_130_Layout";
        internal const string Scene = "Assets/My_Assets/_Project_Spark/Scenes/MainMenu/MainMenu.unity";
        internal const string Prefab = Folder + "/Level_130_Layout.prefab";
        internal const string Definition = Folder + "/Level_130.asset";
        internal const string Preview = Folder + "/Level_130_Layout.unity";
        private const string Kit = Folder + "/H610ME";
        private const string Catalog = "Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Levels/LevelLayoutCatalog/SparkLevelLayoutCatalog.asset";

        static SparkL130PlayableBuilder() => EditorApplication.update += Poll;

        private static void Poll()
        {
            const string request = "Logs/Level130Builder.request";
            if (!File.Exists(request) || EditorApplication.isPlayingOrWillChangePlaymode ||
                EditorApplication.isCompiling || EditorApplication.isUpdating) return;
            File.Delete(request);
            try { Build(); }
            catch (Exception error)
            {
                File.WriteAllText("Logs/Level130Builder.txt", "FAIL " + error);
                Debug.LogException(error);
            }
        }

        internal static void Build()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            SparkL130BoardBuilder.Validate();
            var definition = BuildDefinition();
            var root = PrefabUtility.LoadPrefabContents(Prefab);
            try
            {
                var fixture = root.transform.Find("Recovery Lab fixture");
                if (!fixture) fixture = Child(root.transform, "Recovery Lab fixture");
                foreach (string name in new[] { "FactoryBoard", "RemovableHardware", "ServicePartTargets_EMPTY", "InspectionArtwork_DISABLED", "DiagnosticTargets_NON_ELECTRICAL" })
                {
                    var legacy = root.transform.Find(name);
                    if (legacy) legacy.SetParent(fixture, false);
                    if (!fixture.Find(name)) throw new InvalidOperationException("Missing original layout group " + name);
                }
                foreach (string name in new[] { "Training artwork", "H610-RECOVERY-1 circuit" })
                    if (fixture.Find(name)) Object.DestroyImmediate(fixture.Find(name).gameObject);
                if (root.TryGetComponent<SparkL130Workshop>(out var oldBoard)) Object.DestroyImmediate(oldBoard);

                var workshop = root.AddComponent<SparkL130Workshop>();
                if (!root.TryGetComponent<SparkLevelLayout>(out workshop.layout)) workshop.layout = root.AddComponent<SparkLevelLayout>();
                if (!root.TryGetComponent<SparkLevelSceneBindings>(out workshop.bindings)) workshop.bindings = root.AddComponent<SparkLevelSceneBindings>();
                workshop.board = fixture;
                var factory = fixture.Find("FactoryBoard");
                workshop.substrate = factory.Find("PCB");
                var hardware = fixture.Find("RemovableHardware");
                var originals = Rows("ModelManifest.csv").Where(row => row[2] is "factory" or "structure" or "removable").ToArray();
                foreach (var row in originals)
                {
                    var anchor = (row[2] == "removable" ? hardware : factory).Find(row[0]);
                    if (!anchor || (anchor.localPosition - Point(row, 4)).sqrMagnitude > 1e-10f)
                        throw new InvalidOperationException("Original visual placement differs at " + row[0]);
                }

                var art = Child(fixture, "Training artwork");
                var materials = new Dictionary<string, Material>(StringComparer.Ordinal);
                foreach (string family in new[] { "Aluminum", "Plastic", "Heatsink", "Gold", "Chip", "PCB" })
                {
                    var material = AssetDatabase.LoadAssetAtPath<Material>(Folder + "/URP_PBR_Materials/L130_" + family + ".mat");
                    if (!material || !material.GetTexture("_BaseMap")) throw new InvalidOperationException("Missing URP material " + family);
                    materials.Add(family, material);
                }
                var definitionArt = BuildArt(workshop, factory, hardware, art, materials);
                var contacts = BuildCircuit(workshop, Child(fixture, "H610-RECOVERY-1 circuit"));
                Bind(workshop, contacts);
                definition = AssetDatabase.LoadAssetAtPath<SparkLevelDefinition>(Definition);
                if (!definition) throw new InvalidOperationException("Level 130 definition did not finish importing.");
                Set(workshop.layout, ("levelDefinition", definition), ("sceneBindings", workshop.bindings));
                fixture.localScale = new Vector3(39.9f, 39.9f, 39.9f);

                var saved = PrefabUtility.SaveAsPrefabAsset(root, Prefab);
                if (!saved || !saved.TryGetComponent<SparkLevelLayout>(out var layout) || !layout.Validate(out _))
                    throw new InvalidOperationException("Cannot load a validated playable Level 130 prefab.");
                File.WriteAllLines(Kit + "/Data/H610_RECOVERY_1_Test_Points.csv", definitionArt);
                AssetDatabase.ImportAsset(Kit + "/Data/H610_RECOVERY_1_Test_Points.csv");
                Register(definition, layout);
                BuildPreview(saved);
                File.WriteAllText("Logs/Level130Builder.txt", $"BUILT {SparkL130Workshop.Revision}. Original anchors verified for {originals.Length} pieces.");
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }

        private static List<string> BuildArt(SparkL130Workshop workshop, Transform factory, Transform hardware, Transform art,
            Dictionary<string, Material> materials)
        {
            var rows = Rows("ServiceTargets.csv");
            var service = new List<SparkL130Workshop.Site>();
            var result = new List<string> { "revision,domain,id,training_voltage_V,x_m,y_m,z_m,authority" };
            string[] names = { "CPU socket", "Cooler mount", "DDR5 channel 1", "DDR5 channel 2", "M.2 drive" };
            for (int index = 0; index < names.Length; index++)
            {
                string[] row = rows[index > 1 ? index - 1 : 0];
                Vector3 position = Point(row, 2);
                if (index == 1) position.y += .006f;
                var site = CreateSite(art, names[index], position, index is 2 or 3 ? .0045f : .007f);
                site.model = index == 1 ? Cooler(site.anchor, materials) : ServiceModel(row[1], site.anchor);
                site.model.SetActive(false);
                service.Add(site);
                Record("service", row[0] == "CPU" && index == 1 ? "COOLER" : row[0], "", position);
            }
            workshop.service = service.ToArray();
            workshop.cover = ServiceModel("SERVICE_SOCKET_COVER", service[0].anchor);
            workshop.cover.transform.localPosition += Vector3.up * .0023f;
            workshop.retention = hardware.Find("CPU_RETENTION")?.gameObject;
            workshop.nvmeScrew = ServiceModel("M2_SCREW", art);
            workshop.nvmeScrew.transform.localPosition = factory.Find("M2_POST1").localPosition + Vector3.up * .008f;
            workshop.nvmeScrew.SetActive(false);

            var diagnosticRoot = workshop.board.Find("DiagnosticTargets_NON_ELECTRICAL");
            workshop.diagnostics = Rows("DiagnosticTargets.csv").Select(row =>
            {
                string label = row[0] + "_" + row[1] + "_VISUAL_ANCHOR";
                Transform anchor = diagnosticRoot.Find(label);
                Vector3 position = Point(row, 2);
                Transform original = factory.Find(row[1]);
                if (!original) original = hardware.Find(row[1]);
                if (!anchor || !original || (position - anchor.localPosition).sqrMagnitude > 1e-10f ||
                    (position - original.localPosition).sqrMagnitude > 1e-10f)
                    throw new InvalidOperationException("Original diagnostic anchor differs: " + label);
                anchor.gameObject.layer = LayerMask.NameToLayer("SparkInteractable");
                if (!anchor.TryGetComponent<BoxCollider>(out var collider)) collider = anchor.gameObject.AddComponent<BoxCollider>();
                collider.center = new Vector3(0f, .005f, 0f);
                collider.size = new Vector3(.007f, .008f, .007f);
                if (!anchor.TryGetComponent<SparkSelectable>(out var selectable)) selectable = anchor.gameObject.AddComponent<SparkSelectable>();
                Record("diagnostic visual anchor", row[1], "", position);
                return new SparkL130Workshop.Site { name = row[0] + " · " + row[1], anchor = anchor, selectable = selectable, model = original.gameObject };
            }).ToArray();
            workshop.fanOriginal = factory.Find("CPU_FAN1").gameObject;
            var spare = AssetDatabase.LoadAssetAtPath<GameObject>(Folder + "/DiagnosticSparePrefabs/SP03_CPU_FAN1_VISUAL_ONLY.prefab");
            if (!spare) throw new InvalidOperationException("Missing the original matching tach spare asset.");
            workshop.fanReplacement = (GameObject)PrefabUtility.InstantiatePrefab(spare, art);
            workshop.fanReplacement.transform.localPosition = workshop.fanOriginal.transform.localPosition;
            workshop.fanReplacement.SetActive(false);

            string[] marks = { "TP_STANDBY", "TP_EPS_SOURCE", "TP_EPS_SOCKET", "TP_VCORE", "TP_DDR5", "TP_IO" };
            string[] voltages = { "5", "12", "12", "1.1", "1.1", "3.3" };
            Vector3[] positions = { new(.086f,.004f,.040f), new(-.068f,.004f,.086f), new(-.050f,.004f,.092f),
                new(-.037f,.004f,.053f), new(.087f,.004f,.013f), new(.072f,.004f,-.069f) };
            workshop.railSites = new Transform[marks.Length];
            for (int i = 0; i < marks.Length; i++)
            {
                workshop.railSites[i] = Box(art, marks[i], positions[i], new Vector3(.0031f, .0007f, .0031f), materials["Gold"]).transform;
                BoardText(art, marks[i].Replace("TP_", " ").Replace('_', ' ').Trim(),
                    positions[i] + new Vector3(-.007f, .001f, -.005f), .00165f, new Color(.68f, .86f, .94f));
                Record("isolated virtual test point", marks[i], voltages[i], positions[i]);
            }
            var accent = Flat("L130_TrainingIndicator", new Color(.09f, .16f, .19f), true);
            workshop.debugLamps = new[] { "EZ_CPU", "EZ_DRAM", "EZ_VGA", "EZ_BOOT" }.Select((name, index) =>
            {
                Vector3 position = factory.Find(name).localPosition + Vector3.up * .0055f;
                BoardText(art, name.Replace("EZ_", ""), position + new Vector3(-.007f, .002f, .0002f), .00165f, Color.white);
                return Box(art, "POST " + name, position, new Vector3(.0031f, .0009f, .0031f), accent).GetComponent<Renderer>();
            }).ToArray();
            Vector3 spot = factory.Find("CPU_FAN1").localPosition + Vector3.up * .007f;
            Record("independent motor load", "CPU_FAN1.MOTOR", "12", spot);
            Record("independent tach load", "CPU_FAN1.TACH", "12", spot + Vector3.forward * .004f);
            Vector3 flash = factory.Find("U_BIOS").localPosition + Vector3.up * .005f;
            Record("fictional ROM power", "U_BIOS.POWER", "3.3", flash);
            Record("fictional ROM data ready", "U_BIOS.DATA_READY", "3.3", flash + Vector3.forward * .004f);

            BoardText(art, "H610-RECOVERY-1  /  RECOVERY LAB", new Vector3(.085f, .005f, -.043f), .0042f,
                new Color(.68f, .87f, .81f));
            var cursor = Child(art, "Selected site").gameObject.AddComponent<LineRenderer>();
            cursor.sharedMaterial = Flat("L130_SelectionTrace", new Color(.34f, .85f, .72f), false);
            cursor.useWorldSpace = false;
            cursor.loop = true;
            cursor.widthMultiplier = .019f;
            cursor.positionCount = 4;
            cursor.SetPositions(new[] { new Vector3(-.009f, 0, -.009f), new Vector3(.009f, 0, -.009f),
                new Vector3(.009f, 0, .009f), new Vector3(-.009f, 0, .009f) });
            cursor.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            return result;

            void Record(string domain, string id, string voltage, Vector3 position) =>
                result.Add(FormattableString.Invariant($"{SparkL130Workshop.Revision},{domain},{id},{voltage},{position.x:F6},{position.y:F6},{position.z:F6},educational"));
        }

        private static GameObject Cooler(Transform anchor, Dictionary<string, Material> materials)
        {
            var cover = Child(anchor, "Boxed cooler and fan");
            Box(cover, "Contact plate", new Vector3(0,.002f,0), new Vector3(.039f,.003f,.040f), materials["Aluminum"]);
            for (int fin = -6; fin <= 6; fin++)
                Box(cover, "Aluminum fin", new Vector3(fin * .0028f,.009f,0), new Vector3(.0015f,.015f,.036f), materials["Heatsink"]);
            Box(cover, "Fan frame", new Vector3(0,.017f,0), new Vector3(.038f,.004f,.038f), materials["Plastic"]);
            var fan = Cylinder(cover, "Fan hub", new Vector3(0,.020f,0), new Vector3(.012f,.0022f,.012f), materials["Chip"]);
            for (int blade = 0; blade < 7; blade++)
            {
                var vane = Box(cover, "Fan blade", new Vector3(0,.019f,0), new Vector3(.006f,.0013f,.023f), materials["Aluminum"]);
                vane.transform.RotateAround(fan.transform.position, Vector3.up, (360f / 7f) * blade);
                vane.transform.localPosition += vane.transform.forward * .006f;
            }
            for (int corner = 0; corner < 4; corner++)
                Cylinder(cover, "Spring mount " + (corner + 1), new Vector3(corner % 2 == 0 ? -.023f : .023f,
                    .007f, corner < 2 ? -.023f : .023f), new Vector3(.005f,.007f,.005f), materials["Gold"]);
            return cover.gameObject;
        }

        private static GameObject ServiceModel(string id, Transform parent)
        {
            var source = AssetDatabase.LoadAssetAtPath<GameObject>(Folder + "/ServicePartPrefabs/" + id + ".prefab");
            if (!source) throw new InvalidOperationException("Missing kit service model " + id);
            return (GameObject)PrefabUtility.InstantiatePrefab(source, parent);
        }

        private static SparkL130Workshop.Site CreateSite(Transform parent, string name, Vector3 pos, float radius)
        {
            var anchor = Child(parent, name, pos);
            anchor.gameObject.layer = LayerMask.NameToLayer("SparkInteractable");
            var collider = anchor.gameObject.AddComponent<BoxCollider>();
            collider.size = new Vector3(radius * 2, .007f, radius * 2);
            return new SparkL130Workshop.Site { name = name, anchor = anchor, selectable = anchor.gameObject.AddComponent<SparkSelectable>() };
        }

        private static void BoardText(Transform root, string value, Vector3 pos, float size, Color color)
        {
            Transform node = Child(root, value, pos);
            node.localRotation = Quaternion.Euler(0f, 90f, 0f) * Quaternion.Euler(90f, 180f, 0f);
            var label = node.gameObject.AddComponent<TextMesh>();
            label.text = value;
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (label.font) label.GetComponent<MeshRenderer>().sharedMaterial = label.font.material;
            label.anchor = TextAnchor.MiddleCenter;
            label.alignment = TextAlignment.Center;
            label.fontSize = 64;
            label.characterSize = size / 6f;
            label.color = color;
        }

        private static Material Flat(string name, Color color, bool emission)
        {
            string path = Folder + "/URP_PBR_Materials/" + name + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (!material)
            {
                material = new Material(Shader.Find(emission ? "Universal Render Pipeline/Lit" : "Universal Render Pipeline/Unlit"));
                AssetDatabase.CreateAsset(material, path);
            }
            material.SetColor("_BaseColor", color);
            if (emission) material.EnableKeyword("_EMISSION");
            EditorUtility.SetDirty(material);
            AssetDatabase.SaveAssetIfDirty(material);
            return material;
        }

        private static GameObject Box(Transform root, string name, Vector3 pos, Vector3 size, Material material) =>
            Shape(PrimitiveType.Cube, root, name, pos, size, material);

        private static GameObject Cylinder(Transform root, string name, Vector3 pos, Vector3 size, Material material) =>
            Shape(PrimitiveType.Cylinder, root, name, pos, size, material);

        private static GameObject Shape(PrimitiveType kind, Transform root, string name, Vector3 pos, Vector3 size, Material material)
        {
            var item = GameObject.CreatePrimitive(kind);
            item.name = name;
            item.transform.SetParent(root, false);
            item.transform.localPosition = pos;
            item.transform.localScale = size;
            Object.DestroyImmediate(item.GetComponent<Collider>());
            item.GetComponent<Renderer>().sharedMaterial = material;
            return item;
        }

        private static Transform Child(Transform root, string name, Vector3 position = default)
        {
            var node = new GameObject(name).transform;
            node.SetParent(root, false);
            node.localPosition = position;
            return node;
        }

        private static Vector3 Point(string[] row, int axis) => new(
            float.Parse(row[axis], CultureInfo.InvariantCulture),
            float.Parse(row[axis + 1], CultureInfo.InvariantCulture),
            float.Parse(row[axis + 2], CultureInfo.InvariantCulture));

        private static string[][] Rows(string file) => File.ReadLines(Kit + "/Data/" + file).Skip(1).Select(line => line.Split(',')).ToArray();

        private static void BuildPreview(GameObject prefab)
        {
            var scene = SceneManager.GetSceneByPath(Preview);
            bool opened = !scene.isLoaded;
            if (opened)
                scene = File.Exists(Preview) ? EditorSceneManager.OpenScene(Preview, OpenSceneMode.Additive) :
                    EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            try
            {
                if (!opened && scene.isDirty) return;
                if (!scene.GetRootGameObjects().Any(item => PrefabUtility.GetCorrespondingObjectFromSource(item) == prefab))
                    PrefabUtility.InstantiatePrefab(prefab, scene);
                if (!EditorSceneManager.SaveScene(scene, Preview))
                    throw new InvalidOperationException("Could not save Level 130 preview.");
            }
            finally { if (opened) EditorSceneManager.CloseScene(scene, true); }
        }
    }
}


