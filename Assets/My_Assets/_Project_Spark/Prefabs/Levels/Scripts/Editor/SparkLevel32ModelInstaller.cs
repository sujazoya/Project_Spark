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
    public static class SparkLevel32ModelInstaller
    {
        public const string Folder = "Assets/My_Assets/_Project_Spark/Prefabs/Levels/Level_32_Layout/";
        public const string Prefab = Folder + "Level_32_Optical_Sorter_Layout.prefab";
        public const string ScenePath = "Assets/My_Assets/_Project_Spark/Scenes/MainMenu/MainMenu.unity";
        private const string Model = Folder + "Models/Level32_Fitted_Assembly.fbx";
        private const string Materials = Folder + "Models/Materials";
        private const float SocketHeight = .025f;
        private const float SocketDiameter = .088f;

        private static readonly (string id, string label, float x, float z, string color)[] Sockets =
        {
            ("Battery_positive", "+", -.888f, .305f, "red"), ("Battery_negative", "−", -.732f, .305f, "blue"),
            ("MAIN_IN", "IN", -.498f, .305f, "red"), ("MAIN_OUT", "OUT", -.342f, .305f, "red"),
            ("LDR_A", "A", -.108f, .305f, "red"), ("LDR_B", "SENSE", .048f, .305f, "cyan"),
            ("R_DIV_IN", "IN", .282f, .305f, "cyan"), ("R_DIV_OUT", "OUT", .438f, .305f, "blue"),
            ("Motor_A", "A", .672f, .305f, "red"), ("Motor_B", "B", .828f, .305f, "blue")
        };

        public static IEnumerable<string> TerminalIds => Sockets.Select(socket => socket.id);

        private static readonly Dictionary<string, string> Palette = new Dictionary<string, string>
        {
            {"amber","FFA82B"},{"belt","18202A"},{"bezel","0C1826"},{"blue","1671E2"},{"chrome","D2E1ED"},
            {"bright_tile","E7EEE5"},{"cyan","23DAFD"},{"dark_tile","232730"},{"deep_steel","1B2A3C"},
            {"gold","EDB846"},{"graphite","121D2B"},{"green","2CFC8F"},{"mid_steel","384C65"},{"orange","F97126"},
            {"red","FF4158"},{"review_tile","82929B"},{"rubber","181E25"},{"sensor_ceramic","EDC273"},
            {"steel","7089A2"},{"titanium","9FB3C8"},{"trace","A04D22"},{"void","070D18"},{"white","E7F5FB"}
        };

        [MenuItem("Tools/Project Spark/Level 32/Fit Breadboard Assembly")]
        public static void Fit()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Exit Play Mode before fitting Level 32.");
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(Model);
            if (model == null || AssetDatabase.LoadAssetAtPath<GameObject>(Prefab) == null)
                throw new InvalidOperationException("The fitted FBX or existing Level 32 prefab is missing.");

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
                    throw new InvalidOperationException("Unity could not save the Level 32 layout.");
                AssetDatabase.SaveAssets();
                Debug.Log("Level 32 fitted to the MainMenu breadboard with 10 seated terminals.");
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
            var bench = root.transform.Find("Optical Workbench");
            var bindings = root.GetComponent<SparkLevelSceneBindings>();
            var controls = root.GetComponent<SparkLevel32OpticalSorter>();
            if (bench == null || bindings == null || controls == null)
                throw new InvalidOperationException("Level 32 is missing its workbench, controls, or scene bindings.");

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

            foreach (string name in new[] {"Fitted Level 32 Models", "Fitted Level 32 Motion", "Breadboard Labels"})
            {
                var existing = bench.Find(name);
                if (existing != null) Object.DestroyImmediate(existing.gameObject);
            }
            foreach (string name in new[] {"Board", "Optical sorting conveyor"})
            {
                var existing = bench.Find(name);
                if (existing != null) existing.gameObject.SetActive(false);
            }

            var assembly = Object.Instantiate(model, bench, false).transform;
            assembly.name = "Fitted Level 32 Models";
            var parts = assembly.GetComponentsInChildren<Transform>(true).ToDictionary(item => item.name);
            Transform Part(string name) => parts["L32_" + name];
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
                Label(labels, font, spec.id, spec.label, seat + new Vector3(0, .014f, -.049f), .024f);
            }
            foreach (var owner in owners)
                foreach (Transform child in owner)
                    if (child.GetComponent<SparkTerminal>() == null) child.gameObject.SetActive(false);

            var titles = new (string title, float x, float z)[]
            {
                ("5 V SUPPLY", -.777f, .609f), ("MAIN", -.42f, .609f), ("OPTICAL LDR", -.03f, .609f),
                ("10 kΩ DIVIDER", .36f, .609f), ("33 Ω MOTOR", .75f, .609f),
                ("INSPECTION HEAD", -.13f, .038f), ("CONVEYOR", -.35f, -.382f), ("SORT GATE", .292f, -.334f),
                ("DARK", .68f, -.648f), ("REVIEW", .68f, -.332f), ("BRIGHT", .68f, -.016f)
            };
            foreach (var title in titles)
                Label(labels, font, title.title, title.title, new Vector3(title.x, floor + .020f, title.z), .030f);

            var motionRoot = Child(bench, "Fitted Level 32 Motion", Vector3.zero);
            Transform Wrap(string name)
            {
                var part = Part(name);
                var pivot = Child(motionRoot, name, Vector3.zero);
                pivot.SetPositionAndRotation(part.position, bench.rotation);
                part.SetParent(pivot, true);
                return pivot;
            }
            controls.mainRocker = Wrap("Main_Rocker");
            controls.shaft = Wrap("Motor_Shaft");
            controls.gate = Wrap("Gate_Arm");
            controls.beltSlats = parts.Keys.Where(key => key.StartsWith("L32_Conveyor_Rib_", StringComparison.Ordinal) && !key.Contains("__"))
                .OrderBy(key => key).Select(key => Wrap(key.Substring(4))).ToArray();
            if (controls.beltSlats.Length < 2)
                throw new InvalidOperationException("The conveyor ribs are missing from the FBX.");
            controls.beltStartX = controls.beltSlats.Min(rib => rib.localPosition.x);
            float last = controls.beltSlats.Max(rib => rib.localPosition.x);
            controls.beltLength = last - controls.beltStartX;
            var rib = controls.beltSlats[0].GetComponentInChildren<MeshRenderer>();
            Vector3 ribTop = bench.InverseTransformPoint(rib.bounds.center + bench.up * rib.bounds.extents.y);
            controls.part = Wrap("Conveyor_Sample");
            controls.partRenderer = controls.part.GetComponentInChildren<MeshRenderer>();
            controls.darkPart = MaterialFor(shader, "dark_tile");
            controls.brightPart = MaterialFor(shader, "bright_tile");
            controls.reviewPart = MaterialFor(shader, "review_tile");
            controls.part.gameObject.SetActive(false);
            controls.sampleStart = new Vector3(controls.beltStartX + .025f, ribTop.y + .002f, ribTop.z);
            controls.referencePosition = new Vector3(-.13f, controls.sampleStart.y, ribTop.z);
            controls.sampleAtGate = new Vector3(.28f, controls.sampleStart.y, ribTop.z);
            controls.gateSwing = -34.5f;
            controls.transferClearance = .065f;
            Transform[] Deposits(string name) => new[] { Part(name + "_Deposit_0"), Part(name + "_Deposit_1") };
            Vector3 Destination(Transform[] deposits)
            {
                var destination = bench.InverseTransformPoint((deposits[0].position + deposits[1].position) * .5f);
                foreach (var deposit in deposits) deposit.gameObject.SetActive(false);
                return destination;
            }
            controls.darkDeposits = Deposits("Dark");
            controls.brightDeposits = Deposits("Bright");
            controls.reviewDeposits = Deposits("Review");
            controls.darkDestination = Destination(controls.darkDeposits);
            controls.brightDestination = Destination(controls.brightDeposits);
            controls.reviewDestination = Destination(controls.reviewDeposits);
            controls.routeIndicators = new[] {Part("Gate_Route_1"), Part("Gate_Route_0"), Part("Gate_Route_2")}
                .Select(item => item.GetComponentInChildren<MeshRenderer>()).ToArray();
            foreach (var indicator in controls.routeIndicators) indicator.enabled = false;
            controls.inspectionEmitter = Part("Head_Lamp");
            var lamp = Child(controls.inspectionEmitter, "Inspection light", Vector3.zero).gameObject.AddComponent<Light>();
            lamp.type = LightType.Point;
            lamp.color = new Color(.40f, 1f, .88f);
            lamp.range = worldScale * .14f;
            lamp.intensity = 1.15f;
            lamp.enabled = false;
            controls.inspectionLight = lamp;
            controls.inspectionEmitter.gameObject.SetActive(false);
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
            string path = $"{Materials}/L32 {key}.mat";
            var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing != null) return existing;
            ColorUtility.TryParseHtmlString("#" + Palette[key], out var color);
            var material = new Material(shader) { name = "L32 " + key };
            bool metal = key is "chrome" or "titanium" or "steel" or "gold";
            material.SetFloat("_Metallic", metal ? .46f : .02f);
            material.SetFloat("_Smoothness", metal ? .64f : .35f);
            if (key is "green" or "cyan" or "red" or "blue" or "orange" or "amber")
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", color * .32f);
            }
            material.SetColor("_BaseColor", color);
            AssetDatabase.CreateAsset(material, path);
            return material;
        }
    }
}
#endif

