#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using ProjectSpark.Circuit;
using ProjectSpark.Gameplay;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace ProjectSpark.EditorTools
{
    public static class SparkLevel41ModelInstaller
    {
        public const string Folder = "Assets/My_Assets/_Project_Spark/Prefabs/Levels/Level_41_Layout/";
        public const string Prefab = Folder + "Level_41_Analog_Output_Layout.prefab";
        public const string ScenePath = "Assets/My_Assets/_Project_Spark/Scenes/MainMenu/MainMenu.unity";
        private const string Model = Folder + "Models/Level41_Fitted_Assembly.fbx";
        private const string Materials = Folder + "Models/Materials";
        private const float SocketHeight = .025f;
        private const float SocketDiameter = .073f;

        private static readonly (string id, string label, float x, float z, string color)[] Sockets =
        {
            ("DAC_RETURN", "RET", -.740f, .378f, "blue"),
            ("BIT2", "B2", -.670f, -.337f, "warm"), ("BIT1", "B1", 0, -.337f, "warm"), ("BIT0", "B0", .670f, -.337f, "warm"),
            ("B2_R_IN", "IN", -.794f, -.223f, "warm"), ("B2_R_OUT", "OUT", -.546f, .014f, "mint"),
            ("B1_R_IN", "IN", -.124f, -.223f, "warm"), ("B1_R_OUT", "OUT", .124f, .014f, "mint"),
            ("B0_R_IN", "IN", .546f, -.223f, "warm"), ("B0_R_OUT", "OUT", .794f, .014f, "mint"),
            ("LINK21_IN", "IN", -.484f, .087f, "mint"), ("LINK21_OUT", "OUT", -.232f, .087f, "mint"),
            ("LINK10_IN", "IN", .232f, .087f, "mint"), ("LINK10_OUT", "OUT", .484f, .087f, "mint"),
            ("TERM_R_IN", "IN", .621f, .378f, "mint"), ("TERM_OUT", "OUT", .856f, .378f, "blue")
        };

        public static IEnumerable<string> TerminalIds => Sockets.Select(socket => socket.id);

        private static readonly Dictionary<string, string> Palette = new Dictionary<string, string>
        {
            {"amber","FFA82B"},
            {"blue","1671E2"},
            {"brass","ECB347"},
            {"brown","7A4329"},
            {"ceramic","E6D0A7"},
            {"chrome","D2E1ED"},
            {"cyan","23DAFD"},
            {"deep_steel","1B2A3C"},
            {"fog","A9C0CE"},
            {"glass_edge","6FDFF6"},
            {"gold","EDB846"},
            {"graphite","121D2B"},
            {"green","2CFC8F"},
            {"mid_steel","384C65"},
            {"mint","31EBC3"},
            {"orange","F97126"},
            {"red","FF4158"},
            {"rubber","181E25"},
            {"screen","091F2C"},
            {"screen_green","59FAB1"},
            {"titanium","9FB3C8"},
            {"void","070D18"},
            {"warm","FCAB4D"},
            {"white","E7F5FB"},
        };

        [MenuItem("Tools/Project Spark/Level 41/Fit Breadboard Assembly")]
        public static void Fit()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Exit Play Mode before fitting Level 41.");
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(Model);
            if (model == null || AssetDatabase.LoadAssetAtPath<GameObject>(Prefab) == null)
                throw new InvalidOperationException("The fitted FBX or existing Level 41 prefab is missing.");

            var scene = SceneManager.GetSceneByPath(ScenePath);
            bool opened = !scene.isLoaded;
            if (opened) scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
            GameObject root = null;
            try
            {
                var loader = scene.GetRootGameObjects().SelectMany(item => item.GetComponentsInChildren<SparkLevelLayoutLoader>(true)).Single();
                var layoutRoot = (Transform)new SerializedObject(loader).FindProperty("layoutRoot").objectReferenceValue;
                var board = FindBoard(scene);
                Physics.SyncTransforms();
                root = PrefabUtility.LoadPrefabContents(Prefab);
                Fit(root, model, board, layoutRoot != null ? layoutRoot.localToWorldMatrix : Matrix4x4.identity);
                if (PrefabUtility.SaveAsPrefabAsset(root, Prefab) == null)
                    throw new InvalidOperationException("Unity could not save the Level 41 layout.");
                AssetDatabase.SaveAssets();
                Debug.Log("Level 41 fitted to the MainMenu breadboard with 16 seated terminals.");
            }
            finally
            {
                if (root != null) PrefabUtility.UnloadPrefabContents(root);
                if (opened) EditorSceneManager.CloseScene(scene, true);
            }
        }

        public static BreadboardGrid FindBoard(Scene scene) => scene.GetRootGameObjects()
            .SelectMany(item => item.GetComponentsInChildren<BreadboardGrid>(true))
            .Single(item => item.name == "bread-board");

        public static Vector3 SurfacePoint(BreadboardGrid grid, Vector3 point, Vector3 up)
        {
            float nearest = float.PositiveInfinity;
            // A hole can return its recessed bottom. Seat the rim on the deck
            // surrounding the hole while keeping its horizontal center snapped.
            Vector3 right = Vector3.ProjectOnPlane(grid.GridOrigin.right, up).normalized * .050f;
            Vector3 forward = Vector3.ProjectOnPlane(grid.GridOrigin.forward, up).normalized * .050f;
            var colliders = grid.GetComponents<Collider>();
            foreach (var offset in new[] { Vector3.zero, right, -right, forward, -forward })
            {
                var ray = new Ray(point + offset + up * 2f, -up);
                foreach (var collider in colliders)
                {
                    if (collider.enabled && collider.Raycast(ray, out var hit, 4f) &&
                        hit.distance < nearest && Vector3.Dot(hit.normal, up) > .8f)
                        nearest = hit.distance;
                }
            }
            if (float.IsPositiveInfinity(nearest))
                throw new InvalidOperationException($"The breadboard has no surface under {point}.");
            return point + up * (2f - nearest);
        }

        private static void Fit(GameObject root, GameObject model, BreadboardGrid grid, Matrix4x4 layout)
        {
            var bench = root.transform.Find("DAC Ladder Workbench");
            var bindings = root.GetComponent<SparkLevelSceneBindings>();
            var loop = root.GetComponent<SparkLevel41AnalogOutput>();
            if (bench == null || bindings == null || loop == null)
                throw new InvalidOperationException("Level 41 is missing its workbench, controls, or scene bindings.");

            var gridData = new SerializedObject(grid);
            float spacing = gridData.FindProperty("spacing").floatValue;
            var halfGrid = new Vector3((gridData.FindProperty("columns").intValue - 1) * spacing / 2, 0,
                (gridData.FindProperty("rows").intValue - 1) * spacing / 2);
            var gridCenter = grid.GridOrigin.TransformPoint(halfGrid);
            Matrix4x4 parentWorld = layout * bench.parent.localToWorldMatrix;
            Vector3 up = parentWorld.MultiplyVector(bench.localRotation * Vector3.up).normalized;
            Vector3 difference = gridCenter - parentWorld.MultiplyPoint3x4(bench.localPosition);
            bench.localPosition += parentWorld.inverse.MultiplyVector(Vector3.ProjectOnPlane(difference, up));
            float width = 2 * grid.GridOrigin.TransformVector(Vector3.right * halfGrid.x).magnitude;
            float depth = 2 * grid.GridOrigin.TransformVector(Vector3.forward * halfGrid.z).magnitude;
            float worldScale = Mathf.Min(width / 2.05f, depth / 1.36f);
            bench.localScale = Vector3.one * (worldScale / parentWorld.MultiplyVector(Vector3.right).magnitude);
            Matrix4x4 workbenchWorld = layout * bench.localToWorldMatrix;
            Vector3 ToWorld(Vector3 point) => workbenchWorld.MultiplyPoint3x4(point);
            Vector3 ToLocal(Vector3 point) => workbenchWorld.inverse.MultiplyPoint3x4(point);
            float floor = ToLocal(SurfacePoint(grid, ToWorld(Vector3.zero), up)).y;

            foreach (string name in new[] {"Fitted Level 41 Models", "Breadboard Labels", "Bit rocker pivots"})
            {
                var existing = bench.Find(name);
                if (existing != null) Object.DestroyImmediate(existing.gameObject);
            }
            foreach (string name in new[] {"DAC board", "Output label", "LINK CLOSED", "LINK OPEN"})
            {
                var existing = bench.Find(name);
                if (existing != null) existing.gameObject.SetActive(false);
            }

            var assembly = Object.Instantiate(model, bench, false).transform;
            assembly.name = "Fitted Level 41 Models";
            var parts = assembly.GetComponentsInChildren<Transform>(true).ToDictionary(item => item.name);
            Transform Part(string name) => parts["L41_" + name];
            Vector3 Axis(string name) => bench.InverseTransformVector(Part("Board" + name).position - Part("BoardCenter").position);
            assembly.localRotation = Quaternion.Inverse(Quaternion.LookRotation(-Axis("Front"), Axis("Up"))) * assembly.localRotation;
            assembly.localScale /= Axis("Right").magnitude;
            assembly.localPosition += Vector3.up * floor - bench.InverseTransformPoint(Part("BoardCenter").position);
            if (Axis("Right").x < .99f) throw new InvalidOperationException("The fitted FBX axis is mirrored.");

            if (!AssetDatabase.IsValidFolder(Materials)) AssetDatabase.CreateFolder(Folder + "Models", "Materials");
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) throw new InvalidOperationException("The project URP Lit shader is missing.");
            foreach (var mesh in assembly.GetComponentsInChildren<MeshRenderer>())
            {
                string key = mesh.name.Substring(mesh.name.LastIndexOf("__", StringComparison.Ordinal) + 2);
                mesh.sharedMaterial = MaterialFor(shader, key);
            }

            var labels = Child(bench, "Breadboard Labels", Vector3.zero);
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var holes = new HashSet<Vector2Int>();
            var owners = new HashSet<Transform>();
            int pinLayer = LayerMask.NameToLayer("Pin");
            if (pinLayer < 0) throw new InvalidOperationException("The Pin layer is missing.");
            foreach (var spec in Sockets)
            {
                if (!bindings.TryGetTerminal(spec.id, out var terminal))
                    throw new InvalidOperationException("Missing circuit terminal: " + spec.id);
                owners.Add(terminal.transform.parent);
                terminal.GetComponent<Renderer>().enabled = true;
                var projected = ToWorld(new Vector3(spec.x, floor, spec.z));
                projected -= up * Vector3.Dot(projected - grid.GridOrigin.position, up);
                if (!grid.TryGetNearestHole(projected, .32f, out var hole, out int column, out int row) ||
                    !holes.Add(new Vector2Int(column, row)))
                    throw new InvalidOperationException("No separate breadboard hole for " + spec.id);
                Vector3 seat = ToLocal(SurfacePoint(grid, hole, up));
                Seat(terminal, bench, seat, pinLayer, MaterialFor(shader, spec.color), MaterialFor(shader, "void"));
                Label(labels, font, spec.id, spec.label, seat + new Vector3(0, .014f, -.051f), .028f);
            }
            foreach (var owner in owners)
                foreach (Transform child in owner)
                    if (child.GetComponent<SparkTerminal>() == null && child.GetComponent<Renderer>() != null)
                        child.gameObject.SetActive(false);

            foreach (Transform child in loop.termination.transform)
                if (child.GetComponent<SparkTerminal>() == null && child.GetComponent<Renderer>() != null)
                    child.gameObject.SetActive(false);

            var titles = new (string title, float x, float z)[]
            {
                ("5 V REFERENCE", -.74f, .625f), ("DAC OUTPUT", -.385f, .536f), ("20 kΩ TERMINATION", .74f, .625f),
                ("10 kΩ LINK 21", -.358f, .316f), ("10 kΩ LINK 10", .358f, .316f),
                ("B2  /  20 kΩ", -.67f, -.002f), ("B1  /  20 kΩ", 0, -.002f), ("B0  /  20 kΩ", .67f, -.002f),
                ("B2 DRIVER", -.67f, -.622f), ("B1 DRIVER", 0, -.622f), ("B0 DRIVER", .67f, -.622f)
            };
            foreach (var title in titles)
                Label(labels, font, title.title, title.title, new Vector3(title.x, floor + .020f, title.z), .033f);

            loop.healthyLamp = loop.faultLamp = null;
            loop.referenceLamp = Part("Reference_Ready").gameObject;
            loop.bitRockers = new Transform[3];
            loop.bitLamps = new GameObject[3];
            var motion = Child(bench, "Bit rocker pivots", Vector3.zero);
            for (int bit = 0; bit < 3; bit++)
            {
                var rocker = Part($"Driver{bit}_Rocker");
                var pivot = Child(motion, $"B{bit} rocker pivot", Vector3.zero);
                pivot.SetPositionAndRotation(rocker.position, bench.rotation);
                rocker.SetParent(pivot, true);
                loop.bitRockers[bit] = pivot;
                loop.bitLamps[bit] = Part($"Driver{bit}_High").gameObject;
                loop.bitLamps[bit].SetActive(false);
            }
            loop.voltageBars = new Renderer[8];
            for (int i = 0; i < loop.voltageBars.Length; i++)
            {
                loop.voltageBars[i] = Part($"Monitor_Bar{i}").GetComponentInChildren<Renderer>();
                loop.voltageBars[i].enabled = false;
            }

            loop.voltageReadout = Label(labels, font, "Live DAC voltage", "— V", Vector3.zero, .063f);
            loop.voltageReadout.color = new Color(.47f, 1f, .76f);
            var face = Part("MonitorFace").position;
            var faceUp = (Part("MonitorFaceUp").position - face).normalized;
            loop.voltageReadout.transform.SetPositionAndRotation(face,
                Quaternion.LookRotation(Vector3.Cross(bench.right, faceUp), faceUp));
            loop.codeReadout = Label(labels, font, "Applied binary code", "CODE —",
                new Vector3(0, floor + .027f, .333f), .037f);
            loop.linkReadout = Label(labels, font, "Live termination state", "CHECK WIRING",
                new Vector3(.74f, floor + .027f, .284f), .031f);
        }

        private static void Seat(SparkTerminal terminal, Transform bench, Vector3 seat, int pinLayer, Material rim, Material center)
        {
            var jack = terminal.transform;
            jack.SetPositionAndRotation(bench.TransformPoint(seat + Vector3.up * (SocketHeight * .5f)), bench.rotation);
            jack.localScale = new Vector3(SocketDiameter, SocketHeight * .5f, SocketDiameter);
            jack.gameObject.layer = pinLayer;
            jack.GetComponent<Renderer>().sharedMaterial = rim;
            var disc = jack.Find("Jack recess");
            if (disc == null)
            {
                var primitive = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                Object.DestroyImmediate(primitive.GetComponent<Collider>());
                disc = primitive.transform;
                disc.name = "Jack recess";
                disc.SetParent(jack, false);
                disc.localPosition = new Vector3(0, 1.02f, 0);
                disc.localScale = new Vector3(.59f, .035f, .59f);
            }
            disc.GetComponent<Renderer>().sharedMaterial = center;
            var box = jack.GetComponent<BoxCollider>();
            foreach (var collider in jack.GetComponents<Collider>())
                if (collider != box) Object.DestroyImmediate(collider);
            if (box == null) box = jack.gameObject.AddComponent<BoxCollider>();
            box.size = new Vector3(1.04f, 2.24f, 1.04f);
        }

        private static Transform Child(Transform parent, string name, Vector3 position)
        {
            var child = new GameObject(name).transform;
            child.SetParent(parent, false);
            child.localPosition = position;
            return child;
        }

        private static TextMesh Label(Transform parent, Font font, string name, string text, Vector3 position, float size)
        {
            var child = Child(parent, name, position);
            child.localRotation = Quaternion.Euler(90, 0, 0);
            child.localScale = Vector3.one * .20f;
            var label = child.gameObject.AddComponent<TextMesh>();
            label.font = font;
            label.fontSize = 64;
            label.characterSize = size;
            label.anchor = TextAnchor.MiddleCenter;
            label.color = new Color(.89f, .96f, 1f);
            label.text = text;
            child.GetComponent<MeshRenderer>().sharedMaterial = font.material;
            return label;
        }

        private static Material MaterialFor(Shader shader, string key)
        {
            string path = $"{Materials}/L41 {key}.mat";
            var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing != null) return existing;
            ColorUtility.TryParseHtmlString("#" + Palette[key], out var color);
            var material = new Material(shader) { name = "L41 " + key };
            bool metal = key is "chrome" or "titanium" or "steel" or "gold" or "copper";
            material.SetFloat("_Metallic", metal ? .46f : .02f);
            material.SetFloat("_Smoothness", metal ? .64f : .35f);
            if (key is "green" or "cyan" or "red" or "blue" or "orange" or "amber" or "mint" or "warm" or "screen_green")
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", color * .44f);
            }
            if (key is "glass" or "glass_body" or "glass_soft" or "acrylic")
            {
                color.a = .14f;
                material.SetFloat("_Surface", 1);
                material.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
                material.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
                material.SetFloat("_ZWrite", 0);
                material.SetOverrideTag("RenderType", "Transparent");
                material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                material.SetShaderPassEnabled("ShadowCaster", false);
                material.renderQueue = (int)RenderQueue.Transparent;
            }
            material.SetColor("_BaseColor", color);
            AssetDatabase.CreateAsset(material, path);
            return material;
        }
    }
}
#endif




