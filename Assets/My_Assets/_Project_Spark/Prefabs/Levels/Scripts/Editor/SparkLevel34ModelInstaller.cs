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
    public static class SparkLevel34ModelInstaller
    {
        public const string Folder = "Assets/My_Assets/_Project_Spark/Prefabs/Levels/Level_34_Layout/";
        public const string Prefab = Folder + "Level_34_Priority_Power_Layout.prefab";
        public const string ScenePath = "Assets/My_Assets/_Project_Spark/Scenes/MainMenu/MainMenu.unity";
        private const string Model = Folder + "Models/Level34_Fitted_Assembly.fbx";
        private const string Materials = Folder + "Models/Materials";
        private const float SocketHeight = .025f;
        private const float SocketDiameter = .088f;

        private static readonly (string id, string label, float x, float z, string color)[] Sockets =
        {
            ("Battery_positive", "+", -.868f, .305f, "red"), ("Battery_negative", "−", -.712f, .305f, "blue"),
            ("FEED_IN", "IN", -.468f, .305f, "red"), ("FEED_OUT", "BUS", -.312f, .305f, "cyan"),
            ("Pump_A", "A", .548f, .280f, "cyan"), ("Pump_B", "B", .712f, .280f, "blue"),
            ("AUX_IN", "IN", -.348f, -.111f, "cyan"), ("AUX_OUT", "OUT", -.192f, -.111f, "amber"),
            ("Fan_A", "A", .548f, -.141f, "amber"), ("Fan_B", "B", .712f, -.141f, "blue"),
            ("DEMAND_IN", "IN", -.348f, -.505f, "cyan"), ("DEMAND_OUT", "OUT", -.192f, -.505f, "orange"),
            ("R_DEMAND_IN", "IN", .548f, -.534f, "orange"), ("R_DEMAND_OUT", "OUT", .712f, -.534f, "blue")
        };

        public static IEnumerable<string> TerminalIds => Sockets.Select(socket => socket.id);

        private static readonly Dictionary<string, string> Palette = new Dictionary<string, string>
        {
            {"acrylic","52B3DD"},{"acrylic_edge","84DEF4"},{"amber","FFA82B"},{"bezel","0C1826"},
            {"black","141318"},{"blue","1671E2"},{"chrome","D2E1ED"},{"circuit_green","186150"},
            {"clear_window","579DD3"},{"copper","C2672F"},{"cyan","23DAFD"},{"cyan_soft","45A0D2"},
            {"deep_steel","1B2A3C"},{"display_green","60F8AE"},{"fan_blue","1984E0"},{"glass_edge","6FDFF6"},
            {"gold","EDB846"},{"graphite","121D2B"},{"green","2CFC8F"},{"mid_steel","384C65"},{"orange","F97126"},
            {"pump_blue","0F66A5"},{"red","FF4158"},{"resistor_cream","DBB170"},{"rubber","181E25"},
            {"screen","091F2C"},{"steel","7089A2"},{"titanium","9FB3C8"},{"violet","953ADA"},
            {"void","070D18"},{"white","E7F5FB"},{"yellow","F9DA48"}
        };

        [MenuItem("Tools/Project Spark/Level 34/Fit Breadboard Assembly")]
        public static void Fit()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Exit Play Mode before fitting Level 34.");
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(Model);
            if (model == null || AssetDatabase.LoadAssetAtPath<GameObject>(Prefab) == null)
                throw new InvalidOperationException("The fitted FBX or existing Level 34 prefab is missing.");

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
                    throw new InvalidOperationException("Unity could not save the Level 34 layout.");
                AssetDatabase.SaveAssets();
                Debug.Log("Level 34 fitted to the MainMenu breadboard with 14 seated terminals.");
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
            var bench = root.transform.Find("Priority Power Workbench");
            var bindings = root.GetComponent<SparkLevelSceneBindings>();
            var controls = root.GetComponent<SparkLevel34PriorityPower>();
            if (bench == null || bindings == null || controls == null)
                throw new InvalidOperationException("Level 34 is missing its workbench, controls, or scene bindings.");

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

            foreach (string name in new[] {"Fitted Level 34 Models", "Fitted Level 34 Motion", "Breadboard Labels"})
            {
                var existing = bench.Find(name);
                if (existing != null) Object.DestroyImmediate(existing.gameObject);
            }
            foreach (string name in new[] {"Board", "PUMP RUN", "FAN RUN", "AUX SHED", "DEMAND ON"})
            {
                var existing = bench.Find(name);
                if (existing != null) existing.gameObject.SetActive(false);
            }

            var assembly = Object.Instantiate(model, bench, false).transform;
            assembly.name = "Fitted Level 34 Models";
            var parts = assembly.GetComponentsInChildren<Transform>(true).ToDictionary(item => item.name);
            Transform Part(string name) => parts["L34_" + name];
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
                ("5 V SUPPLY", -.75f, .609f), ("10 Ω FEEDER", -.39f, .609f), ("CRITICAL PUMP", .63f, .609f),
                ("AUX CUTOFF", -.002f, .044f), ("AUX FAN", .337f, .037f),
                ("DEMAND", .021f, -.363f), ("DEMAND BANK\n47 Ω TOTAL", .300f, -.361f),
                ("BUS MONITOR", -.73f, -.023f)
            };
            foreach (var title in titles)
                Label(labels, font, title.title, title.title, new Vector3(title.x, floor + .020f, title.z), .027f);

            var motionRoot = Child(bench, "Fitted Level 34 Motion", Vector3.zero);
            Transform Wrap(string name)
            {
                var part = Part(name);
                var pivot = Child(motionRoot, name, Vector3.zero);
                pivot.SetPositionAndRotation(part.position, bench.rotation);
                part.SetParent(pivot, true);
                return pivot;
            }
            controls.cutoffArm = Wrap("Aux_Arm");
            controls.demandRocker = Wrap("Demand_Rocker");
            controls.pumpShaft = Wrap("Pump_Rotor");
            controls.fanShaft = Wrap("Fan_Rotor");
            controls.fanRotorAxis = Vector3.up;
            Renderer Indicator(string name)
            {
                var renderer = Part(name).GetComponentInChildren<Renderer>();
                renderer.enabled = false;
                return renderer;
            }
            controls.pumpIndicator = Indicator("Pump_Run");
            controls.fanIndicator = Indicator("Fan_Run");
            controls.shedIndicator = Indicator("Aux_Shed");
            controls.connectedIndicator = Indicator("Aux_Connected");
            controls.demandIndicator = Indicator("Bank_Demand");
            Part("Monitor_MonitorLED").gameObject.SetActive(false);

            Light StatusLight(Transform parent, string name, Color color)
            {
                var light = Child(parent, name, Vector3.zero).gameObject.AddComponent<Light>();
                light.type = LightType.Point;
                light.color = color;
                light.range = worldScale * .100f;
                light.intensity = .90f;
                light.enabled = false;
                return light;
            }
            controls.pumpLight = StatusLight(Part("Pump_Run"), "Pump run light", new Color(.23f, 1f, .45f));
            controls.fanLight = StatusLight(Part("Fan_Run"), "Fan run light", new Color(.20f, .83f, 1f));
            controls.shedLight = StatusLight(Part("Aux_Shed"), "Aux shed light", new Color(1f, .57f, .20f));
            controls.demandLight = StatusLight(Part("Bank_Demand"), "Demand light", new Color(1f, .66f, .23f));
            float top = floor + .132f;
            controls.busReadout = Label(labels, font, "Live bus voltage", "BUS —", new Vector3(-.73f, top, -.204f), .035f);
            controls.busReadout.color = new Color(.49f,.95f,1f);
            controls.sagReadout = Label(labels, font, "Observed sag", "SAG —", new Vector3(-.73f, top, -.252f), .026f);
            controls.sagReadout.color = new Color(.78f,.86f,.96f);
            controls.stateReadout = Label(labels, font, "Priority state", "READY TO WIRE", new Vector3(-.73f, top, -.298f), .025f);
            controls.stateReadout.color = new Color(.49f,.95f,1f);
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
            child.localScale = Vector3.one * .25f;
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
            string path = $"{Materials}/L34 {key}.mat";
            var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing != null) return existing;
            ColorUtility.TryParseHtmlString("#" + Palette[key], out var color);
            var material = new Material(shader) { name = "L34 " + key };
            bool metal = key is "chrome" or "titanium" or "steel" or "gold" or "copper";
            material.SetFloat("_Metallic", metal ? .46f : .02f);
            material.SetFloat("_Smoothness", metal ? .64f : .35f);
            if (key is "green" or "cyan" or "red" or "blue" or "orange" or "amber")
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", color * .32f);
            }
            if (key is "acrylic" or "clear_window")
            {
                color.a = key == "acrylic" ? .17f : .23f;
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



