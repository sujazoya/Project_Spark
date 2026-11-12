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
    public static class SparkLevel40DisplayInstaller
    {
        public const string Folder = "Assets/My_Assets/_Project_Spark/Prefabs/Levels/Level_40_Layout/";
        public const string Prefab = Folder + "Level_40_The_Long_Run_Layout.prefab";
        public const string ScenePath = "Assets/My_Assets/_Project_Spark/Scenes/MainMenu/MainMenu.unity";
        private const string Model = Folder + "Models/Level40_Fitted_Assembly.fbx";
        private const string Materials = Folder + "Models/Materials";
        private const float SocketHeight = .025f;
        private const float SocketDiameter = .088f;

        private static readonly (string id, string label, float x, float z, string color)[] Sockets =
        {
            ("LOOP_POS", "LOOP +", -.813f, .342f, "red"), ("LOOP_RETURN", "RETURN", -.527f, .342f, "blue"),
            ("TX_IN", "TX IN", -.143f, .342f, "red"), ("TX_OUT", "TX OUT", .143f, .342f, "amber"),
            ("CABLE_IN", "CABLE IN", .527f, .342f, "amber"), ("CABLE_OUT", "CABLE OUT", .813f, .342f, "amber"),
            ("LINE_IN", "LINE IN", .813f, -.383f, "amber"), ("LINE_OUT", "LINE OUT", .527f, -.383f, "green"),
            ("RX_IN", "RX IN", .143f, -.383f, "green"), ("RX_OUT", "RX OUT", -.143f, -.383f, "blue")
        };

        public static IEnumerable<string> TerminalIds => Sockets.Select(socket => socket.id);

        private static readonly Dictionary<string, string> Palette = new Dictionary<string, string>
        {
            {"amber","FFA82B"},
            {"bezel","0C1826"},
            {"blue","1671E2"},
            {"chrome","D2E1ED"},
            {"copper","C2672F"},
            {"cyan","23DAFD"},
            {"deep_steel","1B2A3C"},
            {"fog","A9C0CE"},
            {"glass","57BCDA"},
            {"glass_edge","6FDFF6"},
            {"gold","EDB846"},
            {"graphite","121D2B"},
            {"green","2CFC8F"},
            {"mid_steel","384C65"},
            {"orange","F97126"},
            {"red","FF4158"},
            {"rubber","181E25"},
            {"screen","091F2C"},
            {"steel","7089A2"},
            {"titanium","9FB3C8"},
            {"void","070D18"},
            {"warning","FFC842"},
            {"water","1399E3"},
            {"water_hi","39DCF4"},
            {"white","E7F5FB"},
        };

        [MenuItem("Tools/Project Spark/Level 40/Fit Complete Display Assembly")]
        public static void Fit()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Exit Play Mode before fitting Level 40.");
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(Model);
            if (model == null || AssetDatabase.LoadAssetAtPath<GameObject>(Prefab) == null)
                throw new InvalidOperationException("The fitted FBX or existing Level 40 prefab is missing.");

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
                    throw new InvalidOperationException("Unity could not save the Level 40 layout.");
                AssetDatabase.SaveAssets();
                Debug.Log("Level 40 fitted to the MainMenu breadboard with 10 seated terminals.");
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
            var bench = root.transform.Find("Current Loop Workbench");
            var bindings = root.GetComponent<SparkLevelSceneBindings>();
            var loop = root.GetComponent<SparkLevel40TheLongRun>();
            if (bench == null || bindings == null || loop == null)
                throw new InvalidOperationException("Level 40 is missing its workbench, controls, or scene bindings.");

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

            foreach (string name in new[] {"Fitted Level 40 Models", "Breadboard Labels", "Level40 Complete Display Assembly",
                "Visible socket guides", "Live line-break lever", "Live tank fill",
                "Live transmitter current", "Live receiver voltage", "Live cable resistance"})
            {
                var existing = bench.Find(name);
                if (existing != null) Object.DestroyImmediate(existing.gameObject);
            }
            foreach (string name in new[] {"Current loop board", "Process tank", "REGULATED", "LOOP FAULT"})
            {
                var existing = bench.Find(name);
                if (existing != null) existing.gameObject.SetActive(false);
            }

            var assembly = Object.Instantiate(model, bench, false).transform;
            assembly.name = "Fitted Level 40 Models";
            var parts = assembly.GetComponentsInChildren<Transform>(true).ToDictionary(item => item.name);
            Transform Part(string name) => parts["L40_" + name];
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

            var titles = new (string title, float x, float z)[]
            {
                ("12 V SUPPLY", -.67f, .627f), ("4–20 mA TRANSMITTER", 0, .627f), ("CABLE SPOOL", .67f, .627f),
                ("PROCESS TANK", -.67f, -.019f), ("250 Ω RECEIVER", 0, -.060f), ("LINE BREAK", .67f, -.060f)
            };
            foreach (var title in titles)
                Label(labels, font, title.title, title.title, new Vector3(title.x, floor + .020f, title.z), .033f);

            loop.tankFill = null;
            loop.normalIndicator = loop.faultIndicator = null;
            var visuals = root.GetComponent<SparkLevel40DisplayVisuals>();
            if (visuals == null) visuals = root.AddComponent<SparkLevel40DisplayVisuals>();
            visuals.loop = loop;

            Transform Pivot(string name, Vector3 position, Transform part)
            {
                var pivot = Child(bench, name, Vector3.zero);
                pivot.position = position;
                part.SetParent(pivot, true);
                return pivot;
            }
            Vector3 waterBottom = Part("WaterBottom").position;
            float nominalHeight = Vector3.Dot(Part("WaterNominalTop").position - waterBottom, bench.up);
            float fullHeight = Vector3.Dot(Part("WaterFullTop").position - waterBottom, bench.up);
            if (!(fullHeight > nominalHeight && nominalHeight > 0f))
                throw new InvalidOperationException("The process tank fill markers are invalid.");
            visuals.waterPivot = Pivot("Live tank fill", waterBottom, Part("Tank_Water"));
            visuals.fullWaterScale = fullHeight / nominalHeight;
            visuals.waterPivot.gameObject.SetActive(false);

            Vector3 hinge = Part("Hinge").position;
            visuals.leverPivot = Pivot("Live line-break lever", hinge, Part("Line_Lever"));
            Vector3 tip = bench.InverseTransformVector(Part("LeverTip").position - hinge);
            float direction = Mathf.Sign(tip.x);
            visuals.closedLeverAngle = -direction * Mathf.Atan2(tip.y, Mathf.Abs(tip.x)) * Mathf.Rad2Deg;
            visuals.openLeverAngle = visuals.closedLeverAngle + direction * 27f;
            visuals.leverPivot.localRotation = Quaternion.Euler(0, 0, visuals.closedLeverAngle);
            visuals.openLamp = Part("Line_OpenLamp").GetComponentInChildren<Renderer>();
            visuals.closedLamp = Part("Line_ClosedLamp").GetComponentInChildren<Renderer>();
            visuals.openLamp.enabled = false;
            visuals.closedLamp.enabled = true;

            TextMesh Readout(string name, string text, float x, float z, float size, Color color)
            {
                var label = Label(labels, font, name, text, new Vector3(x, floor + .027f, z), size);
                label.color = color;
                return label;
            }
            visuals.transmitterReadout = Readout("Live loop current", "— mA", 0, .181f, .043f, visuals.signalColor);
            visuals.cableReadout = Readout("Live cable resistance", "50 Ω", .67f, .181f, .043f, visuals.signalColor);
            visuals.receiverReadout = Readout("Live receiver voltage", "— V", 0, -.538f, .043f, visuals.signalColor);
            visuals.tankReadout = Readout("Live tank percentage", "TANK 0%", -.67f, -.538f, .036f, visuals.signalColor);
            visuals.receiverPercentReadout = Readout("Live receiver percentage", "RECEIVER —", 0, -.619f, .033f, visuals.signalColor);
            visuals.stateReadout = Readout("Live loop state", "WIRE LOOP", .67f, -.538f, .037f, visuals.signalColor);
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
            string path = $"{Materials}/L40 {key}.mat";
            var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing != null) return existing;
            ColorUtility.TryParseHtmlString("#" + Palette[key], out var color);
            var material = new Material(shader) { name = "L40 " + key };
            bool metal = key is "chrome" or "titanium" or "steel" or "gold" or "copper";
            material.SetFloat("_Metallic", metal ? .46f : .02f);
            material.SetFloat("_Smoothness", metal ? .64f : .35f);
            if (key is "green" or "cyan" or "red" or "blue" or "orange" or "amber" or "water_hi")
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", color * .44f);
            }
            if (key == "glass")
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




