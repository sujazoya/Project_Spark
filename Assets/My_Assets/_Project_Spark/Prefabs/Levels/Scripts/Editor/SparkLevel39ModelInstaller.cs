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
    public static class SparkLevel39ModelInstaller
    {
        public const string Folder = "Assets/My_Assets/_Project_Spark/Prefabs/Levels/Level_39_Layout/";
        public const string Prefab = Folder + "Level_39_Isolation_Gap_Layout.prefab";
        public const string ScenePath = "Assets/My_Assets/_Project_Spark/Scenes/MainMenu/MainMenu.unity";
        private const string Model = Folder + "Models/Level39_Fitted_Assembly.fbx";
        private const string Materials = Folder + "Models/Materials";
        private const float SocketHeight = .025f;
        private const float SocketDiameter = .088f;

        private static readonly (string id, string label, float x, float z, string color)[] Sockets =
        {
            ("TX_POS", "+", -.765f, .378f, "red"), ("TX_RETURN", "RET", -.515f, .378f, "blue"),
            ("SEND_IN", "IN", -.765f, .036f, "red"), ("SEND_OUT", "OUT", -.515f, .036f, "amber"),
            ("TX_R_IN", "IN", -.815f, -.434f, "amber"), ("TX_R_OUT", "OUT", -.585f, -.434f, "amber"),
            ("OPTO_A", "A", -.405f, -.434f, "amber"), ("OPTO_K", "K", -.185f, -.434f, "blue"),
            ("RX_POS", "+", .515f, .378f, "red"), ("RX_RETURN", "RET", .765f, .378f, "violet"),
            ("PULLUP_IN", "IN", .515f, .036f, "red"), ("PULLUP_OUT", "OUT", .765f, .036f, "cyan"),
            ("OPTO_C", "C", .185f, -.434f, "cyan"), ("OPTO_E", "E", .405f, -.434f, "violet")
        };

        public static IEnumerable<string> TerminalIds => Sockets.Select(socket => socket.id);

        private static readonly Dictionary<string, string> Palette = new Dictionary<string, string>
        {
            {"amber","FFA82B"},
            {"blue","1671E2"},
            {"brass","E8AE4C"},
            {"brown","804424"},
            {"ceramic","E6CA95"},
            {"chrome","D2E1ED"},
            {"copper","C2672F"},
            {"cyan","23DAFD"},
            {"deep_steel","1B2A3C"},
            {"fog","A9C0CE"},
            {"glass_body","4AABD3"},
            {"glass_edge","6FDFF6"},
            {"gold","EDB846"},
            {"graphite","121D2B"},
            {"green","2CFC8F"},
            {"insulator","343E4E"},
            {"mid_steel","384C65"},
            {"mint","32F5B9"},
            {"orange","F97126"},
            {"pcb","16595B"},
            {"red","FF4158"},
            {"rose","F5596F"},
            {"rubber","181E25"},
            {"screen","091F2C"},
            {"screen_green","41F0AC"},
            {"steel","7089A2"},
            {"titanium","9FB3C8"},
            {"violet","B774FB"},
            {"void","070D18"},
            {"white","E7F5FB"},
            {"yellow","FAD552"},
        };

        [MenuItem("Tools/Project Spark/Level 39/Fit Breadboard Assembly")]
        public static void Fit()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Exit Play Mode before fitting Level 39.");
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(Model);
            if (model == null || AssetDatabase.LoadAssetAtPath<GameObject>(Prefab) == null)
                throw new InvalidOperationException("The fitted FBX or existing Level 39 prefab is missing.");

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
                    throw new InvalidOperationException("Unity could not save the Level 39 layout.");
                AssetDatabase.SaveAssets();
                Debug.Log("Level 39 fitted to the MainMenu breadboard with 14 seated terminals.");
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

        [MenuItem("Tools/Project Spark/Level 39/Register in Open MainMenu")]
        public static void RegisterInOpenMainMenu()
        {
            var scene = SceneManager.GetSceneByPath(ScenePath);
            if (!scene.isLoaded || EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Open MainMenu in Edit Mode before registering Level 39.");
            var definition = AssetDatabase.LoadAssetAtPath<SparkLevelDefinition>(Folder + "Level_39.asset");
            var previous = AssetDatabase.LoadAssetAtPath<SparkLevelDefinition>(
                Folder.Replace("Level_39_Layout", "Level_38_Layout") + "Level_38.asset");
            if (definition == null || previous == null)
                throw new InvalidOperationException("The Level 38 or Level 39 definition is missing.");
            var manager = scene.GetRootGameObjects()
                .SelectMany(item => item.GetComponentsInChildren<LevelGamePlayManager>(true)).Single();
            var serialized = new SerializedObject(manager);
            var levels = serialized.FindProperty("levels");
            int previousIndex = -1;
            for (int i = 0; i < levels.arraySize; i++)
            {
                var entry = levels.GetArrayElementAtIndex(i).objectReferenceValue;
                if (entry == definition) return;
                if (entry == previous) previousIndex = i;
            }
            if (previousIndex < 0)
                throw new InvalidOperationException("Level 38 is missing from the campaign list.");
            levels.InsertArrayElementAtIndex(previousIndex + 1);
            levels.GetArrayElementAtIndex(previousIndex + 1).objectReferenceValue = definition;
            serialized.ApplyModifiedProperties();
            EditorSceneManager.MarkSceneDirty(scene);
            Debug.Log("Level 39 registered after Level 38 in the open MainMenu scene.");
        }

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
            var bench = root.transform.Find("Isolation Workbench");
            var bindings = root.GetComponent<SparkLevelSceneBindings>();
            var controls = root.GetComponent<SparkLevel39IsolationGap>();
            if (bench == null || bindings == null || controls == null)
                throw new InvalidOperationException("Level 39 is missing its workbench, controls, or scene bindings.");

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

            foreach (string name in new[] {"Fitted Level 39 Models", "Breadboard Labels"})
            {
                var existing = bench.Find(name);
                if (existing != null) Object.DestroyImmediate(existing.gameObject);
            }
            foreach (string name in new[] {"TX board", "RX board", "Isolation gap label", "TX LIGHT", "RX LOW", "ISOLATED", "BRIDGED"})
            {
                var existing = bench.Find(name);
                if (existing != null) existing.gameObject.SetActive(false);
            }

            var assembly = Object.Instantiate(model, bench, false).transform;
            assembly.name = "Fitted Level 39 Models";
            var parts = assembly.GetComponentsInChildren<Transform>(true).ToDictionary(item => item.name);
            Transform Part(string name) => parts["L39_" + name];
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
                    if (child.GetComponent<SparkTerminal>() == null && child.GetComponent<Renderer>() != null)
                        child.gameObject.SetActive(false);

            var titles = new (string title, float x, float z)[]
            {
                ("TX 5 V SUPPLY", -.64f, .624f), ("RX 5 V SUPPLY", .64f, .624f),
                ("SEND SWITCH", -.64f, .284f), ("1 kΩ PULL-UP", .64f, .284f),
                ("470 Ω TX LIMIT", -.70f, -.184f), ("TX OPTO LED", -.295f, -.184f),
                ("RX OPTO OUT", .295f, -.184f), ("RX MONITOR", .716f, -.181f),
                ("ISOLATION GAP", 0, .624f)
            };
            foreach (var title in titles)
                Label(labels, font, title.title, title.title, new Vector3(title.x, floor + .020f, title.z), .033f);

            GameObject Indicator(string name)
            {
                var indicator = Part(name).gameObject;
                indicator.SetActive(false);
                return indicator;
            }
            controls.transmitterGlow = Indicator("Emitter_Emitter");
            controls.receiverGlow = Indicator("Receiver_Receiving");
            controls.isolationGlow = Indicator("Barrier_Isolated");
            controls.opticalBeam = Indicator("Barrier_Beam");
            controls.txSupplyLight = Indicator("TxSupply_Power");
            controls.rxSupplyLight = Indicator("RxSupply_Power");
            controls.sendLight = Indicator("Send_Sending");
            controls.sendRocker = Part("Send_Rocker");
            controls.sendRockerRest = controls.sendRocker.localRotation;
            controls.bridgeGlow = null;

            controls.isolationReadout = Label(labels, font, "Live isolation state", "CHECK WIRING",
                new Vector3(0, floor + .028f, .515f), .040f);
            controls.txReadout = Label(labels, font, "Live TX current", "TX LED —",
                new Vector3(-.56f, floor + .028f, -.607f), .039f);
            controls.txReadout.color = new Color(1f, .76f, .36f);
            controls.rxReadout = Label(labels, font, "Live RX output", "RX OUT —",
                new Vector3(.56f, floor + .028f, -.607f), .039f);
            controls.rxReadout.color = new Color(.47f, .95f, 1f);
            controls.monitorReadout = Label(labels, font, "RX logic monitor", "—", Vector3.zero, .050f);
            controls.monitorReadout.color = new Color(.47f, .95f, 1f);
            var face = Part("MonitorFace").position;
            var faceUp = (Part("MonitorFaceUp").position - face).normalized;
            controls.monitorReadout.transform.SetPositionAndRotation(face,
                Quaternion.LookRotation(Vector3.Cross(bench.right, faceUp), faceUp));
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
            string path = $"{Materials}/L39 {key}.mat";
            var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing != null) return existing;
            ColorUtility.TryParseHtmlString("#" + Palette[key], out var color);
            var material = new Material(shader) { name = "L39 " + key };
            bool metal = key is "chrome" or "titanium" or "steel" or "gold" or "copper";
            material.SetFloat("_Metallic", metal ? .46f : .02f);
            material.SetFloat("_Smoothness", metal ? .64f : .35f);
            if (key is "green" or "cyan" or "red" or "blue" or "orange" or "amber" or "screen_green" or "mint" or "violet")
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", color * (key is "screen_green" or "mint" ? .60f : .32f));
            }
            if (key is "glass_soft" or "glass_body" or "acrylic")
            {
                color.a = .22f;
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




