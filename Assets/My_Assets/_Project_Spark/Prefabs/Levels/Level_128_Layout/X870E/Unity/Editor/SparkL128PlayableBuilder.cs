using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using ProjectSpark.Gameplay;
using ProjectSpark.Gameplay.AorusWorkshop;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace ProjectSpark.Level128.Editor
{
    [InitializeOnLoad]
    internal static partial class SparkL128PlayableBuilder
    {
        internal const string Folder = "Assets/My_Assets/_Project_Spark/Prefabs/Levels/Level_128_Layout";
        internal const string Scene = "Assets/My_Assets/_Project_Spark/Scenes/MainMenu/MainMenu.unity";
        internal const string Prefab = Folder + "/Level_128_Layout.prefab";
        internal const string Definition = Folder + "/Level_128.asset";
        internal const string Preview = Folder + "/Level_128_Layout.unity";
        private const string Kit = Folder + "/X870E";
        private const string Catalog = "Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Levels/LevelLayoutCatalog/SparkLevelLayoutCatalog.asset";

        static SparkL128PlayableBuilder() => EditorApplication.update += Poll;

        private static void Poll()
        {
            const string request = "Logs/Level128Builder.request";
            if (!File.Exists(request) || EditorApplication.isPlayingOrWillChangePlaymode ||
                EditorApplication.isCompiling || EditorApplication.isUpdating) return;
            File.Delete(request);
            try { Build(); }
            catch (Exception error)
            {
                File.WriteAllText("Logs/Level128Builder.txt", "FAIL " + error);
                Debug.LogException(error);
            }
        }

        internal static void Build()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            SparkL128BoardBuilder.Validate();
            var definition = BuildDefinition();
            var root = PrefabUtility.LoadPrefabContents(Prefab);
            try
            {
                var fixture = root.transform.Find("AORUS Master fixture");
                if (!fixture) fixture = Child(root.transform, "AORUS Master fixture");
                foreach (string name in new[] { "FactoryBoard", "RemovableArmor", "ServicePartTargets_EMPTY", "InspectionArtwork_DISABLED" })
                {
                    var legacy = root.transform.Find(name);
                    if (legacy) legacy.SetParent(fixture, false);
                    if (!fixture.Find(name)) throw new InvalidOperationException("Missing original layout group " + name);
                }
                foreach (string name in new[] { "Training artwork", "X870E-EDU-1 circuit" })
                    if (fixture.Find(name)) Object.DestroyImmediate(fixture.Find(name).gameObject);
                if (root.TryGetComponent<SparkL128Workshop>(out var oldBoard)) Object.DestroyImmediate(oldBoard);

                var workshop = root.AddComponent<SparkL128Workshop>();
                workshop.layout = root.GetComponent<SparkLevelLayout>() ?? root.AddComponent<SparkLevelLayout>();
                workshop.bindings = root.GetComponent<SparkLevelSceneBindings>() ?? root.AddComponent<SparkLevelSceneBindings>();
                workshop.board = fixture;
                var factory = fixture.Find("FactoryBoard");
                workshop.substrate = factory.Find("PCB");
                var hardware = fixture.Find("RemovableArmor");
                var originals = Rows("ModelManifest.csv").Where(row => row[2] is "factory" or "structure" or "removable").ToArray();
                foreach (var row in originals)
                {
                    var anchor = (row[2] == "removable" ? hardware : factory).Find(row[0]);
                    if (!anchor || (anchor.localPosition - Point(row, 4)).sqrMagnitude > 1e-10f)
                        throw new InvalidOperationException("Original visual placement differs at " + row[0]);
                }

                var art = Child(fixture, "Training artwork");
                var materials = new Dictionary<string, Material>(StringComparer.Ordinal);
                foreach (string family in new[] { "Steel", "Plastic", "Armor", "Gold", "Chip", "PCB" })
                {
                    var material = AssetDatabase.LoadAssetAtPath<Material>(Folder + "/URP_PBR_Materials/L128_" + family + ".mat");
                    if (!material || !material.GetTexture("_BaseMap")) throw new InvalidOperationException("Missing URP material " + family);
                    materials.Add(family, material);
                }
                var definitionArt = BuildArt(workshop, factory, hardware, art, materials);
                var contacts = BuildCircuit(workshop, Child(fixture, "X870E-EDU-1 circuit"));
                Bind(workshop, contacts);
                definition = AssetDatabase.LoadAssetAtPath<SparkLevelDefinition>(Definition);
                if (!definition) throw new InvalidOperationException("Level 128 definition is missing after import.");
                Set(workshop.layout, ("levelDefinition", definition), ("sceneBindings", workshop.bindings));
                fixture.localScale = Vector3.one * (7.98f / .244f);

                var saved = PrefabUtility.SaveAsPrefabAsset(root, Prefab);
                if (!saved || !saved.TryGetComponent<SparkLevelLayout>(out var layout) || !layout.Validate(out _))
                    throw new InvalidOperationException("Cannot load a validated playable Level 128 prefab.");
                File.WriteAllLines(Kit + "/Data/X870E_EDU_1_Test_Points.csv", definitionArt);
                AssetDatabase.ImportAsset(Kit + "/Data/X870E_EDU_1_Test_Points.csv");
                Register(definition, layout);
                BuildPreview(saved);
                File.WriteAllText("Logs/Level128Builder.txt", $"BUILT {SparkL128Workshop.Revision}. Original anchors verified for {originals.Length} pieces.");
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }

        private static List<string> BuildArt(SparkL128Workshop workshop, Transform factory, Transform hardware, Transform art,
            Dictionary<string, Material> materials)
        {
            var rows = Rows("ServiceTargets.csv").ToDictionary(row => row[0], StringComparer.Ordinal);
            var result = new List<string> { "revision,domain,id,training_voltage_V,x_m,y_m,z_m,authority" };
            string[] targets = { "CPU", "CPU", "DIMM_2", "DIMM_4", "M2_1" };
            string[] names = { "AM5 socket", "AM5 cooler", "DDR5 A2", "DDR5 B2", "M.2 A boot drive" };
            var allSurfaces = AssetDatabase.FindAssets("t:Material", new[] { Folder + "/URP_PBR_Materials" })
                .Select(guid => AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guid)))
                .Where(material => material.name.StartsWith("L128_"))
                .ToDictionary(material => material.name.Substring(5), StringComparer.Ordinal);
            static Vector3 Position(string[] row) => Point(row, 2);
            workshop.service = names.Select((name, index) =>
            {
                string[] row = rows[targets[index]];
                Vector3 position = Position(row);
                if (index == 1) position.y += .007f;
                var site = CreateSite(art, name, position, index is 2 or 3 ? .0045f : .008f);
                site.model = index == 1 ? Cooler(site.anchor, materials) : ServiceModel(row[1], site.anchor, allSurfaces);
                site.model.SetActive(false);
                Record("service", index == 1 ? "AM5_COOLER" : targets[index], "", position);
                return site;
            }).ToArray();

            workshop.retention = hardware.Find("CPU_RETENTION").gameObject;
            workshop.socketLever = hardware.Find("CPU_LEVER").gameObject;
            workshop.vrmArmor = hardware.Find("VRM_TOP_ARMOR").gameObject;
            workshop.driveModels = new GameObject[4];
            workshop.driveArmor = new GameObject[4];
            for (int slot = 0; slot < 4; slot++)
            {
                workshop.driveArmor[slot] = hardware.Find($"M2_ARMOR_{slot + 1}").gameObject;
                if (slot == 0) workshop.driveModels[slot] = workshop.service[4].model;
                else
                {
                    string[] row = rows[$"M2_{slot + 1}"];
                    workshop.driveModels[slot] = ServiceModel("SERVICE_NVME",
                        Child(art, $"Expansion drive {slot + 1}", Position(row)), allSurfaces);
                    workshop.driveModels[slot].SetActive(false);
                    Record("service", row[0], "", Position(row));
                }
            }

            string[] references = { "VRM_Q03", "DDR5_A2", "M2B_CPU" };
            workshop.faultSites = references.Select(id => CreateSite(art, id,
                factory.Find(id).localPosition + Vector3.up * .006f, .0048f)).ToArray();
            for (int i = 0; i < references.Length; i++) Record("replaceable training contact", references[i], "", workshop.faultSites[i].anchor.localPosition);

            string[] ports = { "PCIEX16", "M2A_CPU", "M2B_CPU", "M2C_CPU", "M2D_SB" };
            workshop.interfaceSites = ports.Select((id, i) =>
                CreateSite(art, SparkL128Workshop.PortNames[i], factory.Find(id).localPosition + Vector3.up * .012f, .006f)).ToArray();
            for (int i = 0; i < ports.Length; i++) Record("virtual capacity fixture", ports[i], "3.3", workshop.interfaceSites[i].anchor.localPosition);

            string[] marks = { "TP_STANDBY", "TP_EPS_A", "TP_VCORE", "TP_DDR5", "TP_IO", "TP_EPS_B", "TP_PG" };
            string[] voltages = { "5", "12", "1.1", "1.1", "3.3", "12", "3.3" };
            Vector3[] positions = { new(.094f,.004f,.059f), new(-.078f,.004f,.133f), new(-.041f,.004f,.101f),
                new(.082f,.004f,.031f), new(.071f,.004f,-.036f), new(-.044f,.004f,.133f), new(.095f,.004f,.090f) };
            workshop.railSites = new Transform[marks.Length];
            for (int i = 0; i < marks.Length; i++)
            {
                workshop.railSites[i] = Box(art, marks[i], positions[i], new Vector3(.0036f, .0007f, .0036f), materials["Gold"]).transform;
                BoardText(art, marks[i].Replace("TP_", ""), positions[i] + new Vector3(-.006f, .001f, -.006f), .0018f, new Color(.61f, .88f, .99f));
                Record("isolated virtual test point", marks[i], voltages[i], positions[i]);
            }

            var accent = Flat("L128_TrainingIndicator", new Color(.09f, .16f, .19f), true);
            workshop.debugLamps = new[] { "LED_CPU", "LED_DRAM", "LED_VGA", "LED_BOOT" }.Select((name, index) =>
            {
                Vector3 position = factory.Find(name).localPosition + Vector3.up * .0055f;
                BoardText(art, name.Replace("LED_", ""), position + new Vector3(-.006f - .007f * (index % 2), .001f, .0002f),
                    .0017f, Color.white);
                return Box(art, "POST " + name, position, new Vector3(.0028f, .0009f, .0028f), accent).GetComponent<Renderer>();
            }).ToArray();
            Record("virtual fan tach", "FAN_TACH", "12", new Vector3(.030f, .007f, .138f));
            Record("upper GPU lane bank", "GPU_LANES_9_16", "3.3", workshop.interfaceSites[0].anchor.localPosition + Vector3.forward * .004f);
            BoardText(art, "X870E-EDU-1  /  LANE TEST BENCH", new Vector3(.100f, .005f, -.054f), .0042f,
                new Color(.73f, .88f, .99f));

            var cursor = Child(art, "Selected site").gameObject.AddComponent<LineRenderer>();
            cursor.sharedMaterial = Flat("L128_SelectionTrace", new Color(.36f, .76f, .98f), false);
            cursor.useWorldSpace = false;
            cursor.loop = true;
            cursor.widthMultiplier = .025f;
            cursor.positionCount = 4;
            cursor.SetPositions(new[] { new Vector3(-.011f, 0, -.011f), new Vector3(.011f, 0, -.011f),
                new Vector3(.011f, 0, .011f), new Vector3(-.011f, 0, .011f) });
            cursor.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            return result;

            void Record(string domain, string id, string voltage, Vector3 position) =>
                result.Add(FormattableString.Invariant($"{SparkL128Workshop.Revision},{domain},{id},{voltage},{position.x:F6},{position.y:F6},{position.z:F6},educational"));
        }

        private static GameObject Cooler(Transform anchor, Dictionary<string, Material> materials)
        {
            var cover = Child(anchor, "Boxed cooler and fan");
            Box(cover, "Contact plate", new Vector3(0,.002f,0), new Vector3(.039f,.003f,.040f), materials["Steel"]);
            for (int fin = -6; fin <= 6; fin++)
                Box(cover, "Aluminum fin", new Vector3(fin * .0028f,.009f,0), new Vector3(.0015f,.015f,.036f), materials["Armor"]);
            Box(cover, "Fan frame", new Vector3(0,.017f,0), new Vector3(.038f,.004f,.038f), materials["Plastic"]);
            var fan = Cylinder(cover, "Fan hub", new Vector3(0,.020f,0), new Vector3(.012f,.0022f,.012f), materials["Chip"]);
            for (int blade = 0; blade < 7; blade++)
            {
                var vane = Box(cover, "Fan blade", new Vector3(0,.019f,0), new Vector3(.006f,.0013f,.023f), materials["Steel"]);
                vane.transform.RotateAround(fan.transform.position, Vector3.up, (360f / 7f) * blade);
                vane.transform.localPosition += vane.transform.forward * .006f;
            }
            for (int corner = 0; corner < 4; corner++)
                Cylinder(cover, "Spring mount " + (corner + 1), new Vector3(corner % 2 == 0 ? -.023f : .023f,
                    .007f, corner < 2 ? -.023f : .023f), new Vector3(.005f,.007f,.005f), materials["Gold"]);
            return cover.gameObject;
        }

        private static GameObject ServiceModel(string id, Transform parent, Dictionary<string, Material> materials)
        {
            string path = Kit + "/Models_Native/L128_" + id + ".spark128mesh";
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (!model) throw new InvalidOperationException("Missing service model " + path);
            var copy = Object.Instantiate(model, parent, false);
            copy.name = id;
            foreach (var renderer in copy.GetComponentsInChildren<Renderer>(true))
            {
                int split = renderer.name.LastIndexOf("__", StringComparison.Ordinal);
                if (split < 0 || !materials.TryGetValue(renderer.name.Substring(split + 2), out var material))
                    throw new InvalidOperationException("Unbound service material on " + renderer.name);
                renderer.sharedMaterial = material;
            }
            return copy;
        }

        private static SparkL128Workshop.Site CreateSite(Transform parent, string name, Vector3 pos, float radius)
        {
            var anchor = Child(parent, name, pos);
            anchor.gameObject.layer = LayerMask.NameToLayer("SparkInteractable");
            var collider = anchor.gameObject.AddComponent<BoxCollider>();
            collider.size = new Vector3(radius * 2, .007f, radius * 2);
            return new SparkL128Workshop.Site { name = name, anchor = anchor, selectable = anchor.gameObject.AddComponent<SparkSelectable>() };
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
                    throw new InvalidOperationException("Could not save Level 128 preview.");
            }
            finally { if (opened) EditorSceneManager.CloseScene(scene, true); }
        }
    }
}


