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
    public static class SparkLevel31ModelInstaller
    {
        public const string Folder = "Assets/My_Assets/_Project_Spark/Prefabs/Levels/Level_31_Layout/";
        public const string Prefab = Folder + "Level_31_SealIn_Layout.prefab";
        public const string ScenePath = "Assets/My_Assets/_Project_Spark/Scenes/MainMenu/MainMenu.unity";
        private const string Model = Folder + "Models/Level31_Fitted_Assembly.fbx";
        private const string Materials = Folder + "Models/Materials";
        private const float SocketHeight = .025f;
        private const float SocketDiameter = .088f;

        private static readonly (string id, string label, float x, float z, string color)[] Sockets =
        {
            ("Battery_positive", "+", -.908f, .305f, "red"), ("Battery_negative", "−", -.752f, .305f, "blue"),
            ("MAIN_IN", "IN", -.498f, .305f, "red"), ("MAIN_OUT", "OUT", -.342f, .305f, "red"),
            ("STOP_IN", "IN", -.088f, .305f, "orange"), ("STOP_OUT", "OUT", .068f, .305f, "orange"),
            ("START_IN", "IN", .322f, .305f, "green"), ("START_OUT", "OUT", .478f, .305f, "green"),
            ("Coil_plus", "COIL +", -.649f, -.135f, "green"), ("Coil_minus", "COIL −", -.451f, -.135f, "blue"),
            ("Hold_COM", "COM", -.704f, -.303f, "orange"), ("Hold_NO", "NO", -.55f, -.303f, "green"),
            ("Hold_NC_spare", "NC SPARE", -.396f, -.303f, "fog"),
            ("RUN_IN", "IN", -.018f, -.111f, "red"), ("RUN_OUT", "OUT", .138f, -.111f, "cyan"),
            ("Motor_A", "A", .402f, -.120f, "cyan"), ("Motor_B", "B", .558f, -.120f, "blue")
        };

        public static IEnumerable<string> TerminalIds => Sockets.Select(socket => socket.id);

        private static readonly Dictionary<string, string> Palette = new Dictionary<string, string>
        {
            {"amber","FFA82B"},{"belt","151F2A"},{"bezel","0C1826"},{"blue","1671E2"},{"chrome","D2E1ED"},
            {"copper","C2672F"},{"cyan","23DAFD"},{"deep_steel","1B2A3C"},{"fog","A9C0CE"},{"glass_lid","47C1EB"},
            {"gold","EDB846"},{"graphite","121D2B"},{"green","2CFC8F"},{"mid_steel","384C65"},{"orange","F97126"},
            {"red","FF4158"},{"relay_copper","F48526"},{"relay_iron","495B65"},{"rubber","181E25"},
            {"steel","7089A2"},{"titanium","9FB3C8"},{"void","070D18"},{"white","E7F5FB"}
        };

        [MenuItem("Tools/Project Spark/Level 31/Fit Breadboard Assembly")]
        public static void Fit()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Exit Play Mode before fitting Level 31.");
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(Model);
            if (model == null || AssetDatabase.LoadAssetAtPath<GameObject>(Prefab) == null)
                throw new InvalidOperationException("The fitted FBX or existing Level 31 prefab is missing.");

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
                    throw new InvalidOperationException("Unity could not save the Level 31 layout.");
                AssetDatabase.SaveAssets();
                Debug.Log("Level 31 fitted to the MainMenu breadboard with 17 seated terminals.");
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
            var ray = new Ray(point + up * 2f, -up);
            float nearest = float.PositiveInfinity;
            Vector3 result = default;
            foreach (var collider in grid.GetComponents<Collider>())
            {
                if (collider.enabled && collider.Raycast(ray, out var hit, 4f) &&
                    hit.distance < nearest && Vector3.Dot(hit.normal, up) > .8f)
                {
                    nearest = hit.distance;
                    result = hit.point;
                }
            }
            if (float.IsPositiveInfinity(nearest))
                throw new InvalidOperationException($"The breadboard has no surface under {point}.");
            return result;
        }

        private static void Fit(GameObject root, GameObject model, BreadboardGrid grid, Matrix4x4 layout)
        {
            var bench = root.transform.Find("Relay Workbench");
            var bindings = root.GetComponent<SparkLevelSceneBindings>();
            var controls = root.GetComponent<SparkLevel31SealIn>();
            if (bench == null || bindings == null || controls == null)
                throw new InvalidOperationException("Level 31 is missing its workbench, controls, or scene bindings.");

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
            float worldScale = Mathf.Min(width / 2.05f, depth / 1.28f);
            bench.localScale = Vector3.one * (worldScale / parentWorld.MultiplyVector(Vector3.right).magnitude);
            Matrix4x4 workbenchWorld = layout * bench.localToWorldMatrix;
            Vector3 ToWorld(Vector3 point) => workbenchWorld.MultiplyPoint3x4(point);
            Vector3 ToLocal(Vector3 point) => workbenchWorld.inverse.MultiplyPoint3x4(point);
            float floor = ToLocal(SurfacePoint(grid, ToWorld(Vector3.zero), up)).y;

            foreach (string name in new[] {"Fitted Level 31 Models", "Fitted Level 31 Motion", "Breadboard Labels"})
            {
                var existing = bench.Find(name);
                if (existing != null) Object.DestroyImmediate(existing.gameObject);
            }
            foreach (string name in new[] {"Board", "Conveyor belt"})
            {
                var existing = bench.Find(name);
                if (existing != null) existing.gameObject.SetActive(false);
            }

            var assembly = Object.Instantiate(model, bench, false).transform;
            assembly.name = "Fitted Level 31 Models";
            var parts = assembly.GetComponentsInChildren<Transform>(true).ToDictionary(item => item.name);
            Transform Part(string name) => parts["L31_" + name];
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
                var projected = ToWorld(new Vector3(spec.x, floor, spec.z));
                projected -= up * Vector3.Dot(projected - grid.GridOrigin.position, up);
                if (!grid.TryGetNearestHole(projected, .32f, out var hole, out int column, out int row) ||
                    !holes.Add(new Vector2Int(column, row)))
                    throw new InvalidOperationException("No separate breadboard hole for " + spec.id);
                Vector3 seat = ToLocal(SurfacePoint(grid, hole, up));
                Seat(terminal, bench, seat, pinLayer, MaterialFor(shader, spec.color), MaterialFor(shader, "void"));
                Label(labels, font, spec.id, spec.label, seat + new Vector3(0, .004f, -.055f), .024f);
            }
            foreach (var owner in owners)
                foreach (Transform child in owner)
                    if (child.GetComponent<SparkTerminal>() == null) child.gameObject.SetActive(false);

            var titles = new (string title, float x, float z)[]
            {
                ("5 V SUPPLY", -.83f, .211f), ("MAIN", -.42f, .211f), ("STOP · NC", -.01f, .211f),
                ("START · NO", .4f, .211f), ("HOLD RELAY", -.55f, -.407f),
                ("MOTOR CONTACT", .06f, -.214f), ("33 Ω MOTOR", .48f, -.225f), ("CONVEYOR", .40f, -.576f)
            };
            foreach (var title in titles)
                Label(labels, font, title.title, title.title, new Vector3(title.x, floor + .004f, title.z), .030f);

            var motion = root.GetComponent<SparkLevel31ArtMotion>();
            if (motion == null) motion = root.AddComponent<SparkLevel31ArtMotion>();
            var motionRoot = Child(bench, "Fitted Level 31 Motion", Vector3.zero);
            Transform Wrap(string name)
            {
                var part = Part(name);
                var pivot = Child(motionRoot, name, Vector3.zero);
                pivot.SetPositionAndRotation(part.position, bench.rotation);
                part.SetParent(pivot, true);
                return pivot;
            }
            motion.controls = controls;
            motion.mainRocker = Wrap("Main_Rocker");
            motion.stopCap = Wrap("Stop_Button");
            motion.startCap = Wrap("Start_Button");
            motion.relayArm = Wrap("Relay_Arm");
            motion.driveBridge = Wrap("Drive_Bridge");
            motion.motorShaft = Wrap("Motor_Shaft");
            motion.beltRibs = parts.Keys.Where(key => key.StartsWith("L31_Conveyor_Rib_", StringComparison.Ordinal) && !key.Contains("__"))
                .OrderBy(key => key).Select(key => Wrap(key.Substring(4))).ToArray();
            motion.cargo = parts.Keys.Where(key => key.StartsWith("L31_Conveyor_Cargo_", StringComparison.Ordinal) && !key.Contains("__"))
                .OrderBy(key => key).Select(key => Wrap(key.Substring(4))).ToArray();
            if (motion.beltRibs.Length < 2 || motion.cargo.Length == 0)
                throw new InvalidOperationException("The conveyor motion pieces are missing from the FBX.");
            var beacon = Part("Conveyor_Beacon");
            motion.runBeacon = beacon.GetComponentInChildren<MeshRenderer>();
            motion.runBeacon.enabled = false;
            var lamp = Child(beacon, "RUN light", Vector3.zero).gameObject.AddComponent<Light>();
            lamp.type = LightType.Point;
            lamp.color = new Color(.22f, 1f, .42f);
            lamp.range = worldScale * .18f;
            lamp.intensity = 1.3f;
            lamp.enabled = false;
            motion.runLamp = lamp;
            controls.shaft = null;
            controls.relayArm = null;
            controls.beltSlats = Array.Empty<Transform>();
            controls.runLight = null;
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

        private static void Label(Transform parent, Font font, string name, string text, Vector3 position, float size)
        {
            var child = Child(parent, name, position);
            child.localRotation = Quaternion.Euler(90, 0, 0);
            child.localScale = Vector3.one * .25f;
            var label = child.gameObject.AddComponent<TextMesh>();
            label.font = font;
            label.fontSize = 64;
            label.characterSize = size;
            label.anchor = TextAnchor.MiddleCenter;
            label.color = new Color(.89f, .96f, 1f);
            label.text = text;
            child.GetComponent<MeshRenderer>().sharedMaterial = font.material;
        }

        private static Material MaterialFor(Shader shader, string key)
        {
            string path = $"{Materials}/L31 {key}.mat";
            var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing != null) return existing;
            ColorUtility.TryParseHtmlString("#" + Palette[key], out var color);
            var material = new Material(shader) { name = "L31 " + key };
            bool metal = key is "chrome" or "titanium" or "steel" or "copper" or "gold" or "relay_copper";
            material.SetFloat("_Metallic", metal ? .46f : .02f);
            material.SetFloat("_Smoothness", metal ? .64f : .35f);
            if (key is "green" or "cyan" or "red" or "blue" or "orange")
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", color * .32f);
            }
            if (key == "glass_lid")
            {
                color.a = .18f;
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
