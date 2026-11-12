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
    public static class SparkLevel36ModelInstaller
    {
        public const string Folder = "Assets/My_Assets/_Project_Spark/Prefabs/Levels/Level_36_Layout/";
        public const string Prefab = Folder + "Level_36_Thermal_Interlock_Layout.prefab";
        public const string ScenePath = "Assets/My_Assets/_Project_Spark/Scenes/MainMenu/MainMenu.unity";
        private const string Model = Folder + "Models/Level36_Fitted_Assembly.fbx";
        private const string Materials = Folder + "Models/Materials";
        private const float SocketHeight = .025f;
        private const float SocketDiameter = .088f;

        private static readonly (string id, string label, float x, float z, string color)[] Sockets =
        {
            ("Battery_positive", "+", -.113f, .410f, "red"), ("Battery_negative", "−", .113f, .410f, "blue"),
            ("PULLUP_IN", "IN", -.860f, .147f, "red"), ("PULLUP_OUT", "OUT", -.630f, .147f, "cyan"),
            ("SENSOR_IN", "SENSE", -.860f, -.260f, "cyan"), ("SENSOR_RETURN", "RET", -.630f, -.260f, "blue"),
            ("FAN_SW_IN", "IN", -.360f, .147f, "red"), ("FAN_SW_OUT", "OUT", -.130f, .147f, "cyan"),
            ("FAN_A", "A", -.360f, -.260f, "cyan"), ("FAN_B", "B", -.130f, -.260f, "blue"),
            ("HEAT_SW_IN", "IN", .130f, .147f, "red"), ("HEAT_SW_OUT", "OUT", .360f, .147f, "amber"),
            ("HEATER_IN", "IN", .130f, -.260f, "amber"), ("HEATER_OUT", "OUT", .360f, -.260f, "blue"),
            ("FAULT_SW_IN", "IN", .630f, .147f, "red"), ("FAULT_SW_OUT", "OUT", .860f, .147f, "orange"),
            ("LED_R_IN", "IN", .630f, -.260f, "orange"), ("LED_R_OUT", "OUT", .860f, -.260f, "orange"),
            ("LED_A", "A", .630f, -.620f, "orange"), ("LED_K", "K", .860f, -.620f, "blue")
        };

        public static IEnumerable<string> TerminalIds => Sockets.Select(socket => socket.id);

        private static readonly Dictionary<string, string> Palette = new Dictionary<string, string>
        {
            {"acrylic","46CEF6"},
            {"amber","FFA82B"},
            {"amber_led","FF8C39"},
            {"blue","1671E2"},
            {"blue_fan","26AAFD"},
            {"bronze","915130"},
            {"ceramic","DFD6BC"},
            {"chrome","D2E1ED"},
            {"copper","C2672F"},
            {"copper_bright","FF8F2F"},
            {"crimson","FF304D"},
            {"cyan","23DAFD"},
            {"dark_ceramic","1D2733"},
            {"dark_red","872525"},
            {"deep_steel","1B2A3C"},
            {"glass_gold","E6AC5A"},
            {"gold","EDB846"},
            {"graphite","121D2B"},
            {"green","2CFC8F"},
            {"light_cyan","88F9FB"},
            {"mid_steel","384C65"},
            {"orange","F97126"},
            {"red","FF4158"},
            {"red_hot","FF4F25"},
            {"resistor","DDB47E"},
            {"rubber","181E25"},
            {"screen","091F2C"},
            {"screen_green","66FBA1"},
            {"sensor_steel","91AFC7"},
            {"steel","7089A2"},
            {"titanium","9FB3C8"},
            {"void","070D18"},
            {"white","E7F5FB"},
        };

        [MenuItem("Tools/Project Spark/Level 36/Fit Breadboard Assembly")]
        public static void Fit()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Exit Play Mode before fitting Level 36.");
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(Model);
            if (model == null || AssetDatabase.LoadAssetAtPath<GameObject>(Prefab) == null)
                throw new InvalidOperationException("The fitted FBX or existing Level 36 prefab is missing.");

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
                    throw new InvalidOperationException("Unity could not save the Level 36 layout.");
                AssetDatabase.SaveAssets();
                Debug.Log("Level 36 fitted to the MainMenu breadboard with 20 seated terminals.");
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
            var bench = root.transform.Find("Thermal Interlock Workbench");
            var bindings = root.GetComponent<SparkLevelSceneBindings>();
            var controls = root.GetComponent<SparkLevel36ThermalInterlock>();
            if (bench == null || bindings == null || controls == null)
                throw new InvalidOperationException("Level 36 is missing its workbench, controls, or scene bindings.");

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

            foreach (string name in new[] {"Fitted Level 36 Models", "Fitted Level 36 Motion", "Breadboard Labels"})
            {
                var existing = bench.Find(name);
                if (existing != null) Object.DestroyImmediate(existing.gameObject);
            }
            foreach (string name in new[] {"Board"})
            {
                var existing = bench.Find(name);
                if (existing != null) existing.gameObject.SetActive(false);
            }

            var assembly = Object.Instantiate(model, bench, false).transform;
            assembly.name = "Fitted Level 36 Models";
            var parts = assembly.GetComponentsInChildren<Transform>(true).ToDictionary(item => item.name);
            Transform Part(string name) => parts["L36_" + name];
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
                Label(labels, font, spec.id, spec.label, seat + new Vector3(0, .014f, -.051f), .028f);
            }
            foreach (var owner in owners)
                foreach (Transform child in owner)
                    if (child.GetComponent<SparkTerminal>() == null &&
                        (child.GetComponent<Renderer>() != null || child.name == "Fan rotor"))
                        child.gameObject.SetActive(false);

            foreach (Transform child in controls.thermistor.transform)
                if (child.GetComponent<SparkTerminal>() == null && child.GetComponent<Renderer>() != null)
                    child.gameObject.SetActive(false);

            var titles = new (string title, float x, float z)[]
            {
                ("5 V SUPPLY", 0, .656f),
                ("10 kΩ PULL-UP", -.745f, .365f), ("FAN CONTACT", -.245f, .365f),
                ("HEATER CONTACT", .245f, .365f), ("FAULT CONTACT", .745f, .365f),
                ("NTC SENSOR", -.745f, -.015f), ("COOLING FAN", -.245f, -.015f),
                ("100 Ω HEATER", .245f, -.015f), ("330 Ω RESISTOR", .745f, -.015f),
                ("FAULT LED", .745f, -.398f)
            };
            foreach (var title in titles)
                Label(labels, font, title.title, title.title, new Vector3(title.x, floor + .020f, title.z), .033f);

            var motion = Child(bench, "Fitted Level 36 Motion", Vector3.zero);
            Transform Pivot(string name)
            {
                var part = Part(name);
                var pivot = Child(motion, name, Vector3.zero);
                pivot.SetPositionAndRotation(part.position, bench.rotation);
                part.SetParent(pivot, true);
                return pivot;
            }
            GameObject Indicator(string name, float lightRange = 0f)
            {
                var part = Part(name);
                if (lightRange > 0)
                {
                    var light = Child(part, "Output glow", Vector3.up * .06f).gameObject.AddComponent<Light>();
                    light.type = LightType.Point;
                    light.range = lightRange * worldScale;
                    light.intensity = .95f;
                    light.color = new Color(1f, .44f, .15f);
                }
                part.gameObject.SetActive(false);
                return part.gameObject;
            }
            controls.fanRotor = Pivot("Fan_Rotor");
            controls.fanArm = Pivot("FanContact_Arm");
            controls.heaterArm = Pivot("HeaterContact_Arm");
            controls.faultArm = Pivot("FaultContact_Arm");
            controls.supplyStatus = Indicator("Supply_Power");
            controls.sensorHeat = Indicator("Sensor_Heat");
            controls.fanStatus = Indicator("FanContact_Contact");
            controls.heaterStatus = Indicator("HeaterContact_Contact");
            controls.faultStatus = Indicator("FaultContact_Contact");
            controls.heaterGlow = Indicator("Heater_Glow", .21f);
            controls.warningGlow = Indicator("FaultLed_Lit", .19f);

            var monitorTitle = Label(labels, font, "Thermal monitor heading", "THERMAL INTERLOCK", new Vector3(-.28f, floor + .030f, -.438f), .031f);
            monitorTitle.color = new Color(.68f, .87f, 1f);
            controls.senseReadout = Label(labels, font, "Live SENSE", "SENSE —", new Vector3(-.28f, floor + .030f, -.511f), .041f);
            controls.senseReadout.color = new Color(.45f, .95f, 1f);
            controls.outputReadout = Label(labels, font, "Live output status", "FAN —    HEAT —    FAULT —",
                new Vector3(-.28f, floor + .030f, -.592f), .026f);
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
            string path = $"{Materials}/L36 {key}.mat";
            var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing != null) return existing;
            ColorUtility.TryParseHtmlString("#" + Palette[key], out var color);
            var material = new Material(shader) { name = "L36 " + key };
            bool metal = key is "chrome" or "titanium" or "steel" or "gold" or "copper";
            material.SetFloat("_Metallic", metal ? .46f : .02f);
            material.SetFloat("_Smoothness", metal ? .64f : .35f);
            if (key is "green" or "cyan" or "red" or "blue" or "orange" or "amber" or "red_hot" or "copper_bright" or "amber_led")
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", color * (key is "red_hot" or "copper_bright" or "amber_led" ? 1.4f : .32f));
            }
            if (key is "acrylic" or "glass_gold")
            {
                color.a = key == "glass_gold" ? .28f : .17f;
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




