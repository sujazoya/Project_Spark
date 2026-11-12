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
    public static class SparkLevel30ModelInstaller
    {
        public const string Folder = "Assets/My_Assets/_Project_Spark/Prefabs/Levels/Chapter_03/Level_30_Layout/";
        public const string Prefab = Folder + "Level_30_Lift_Layout.prefab";
        public const string Scene = "Assets/My_Assets/_Project_Spark/Scenes/MainMenu/MainMenu.unity";
        private const string Model = Folder + "Models/Level30_Fitted_Assembly.fbx";
        private const string Materials = Folder + "Models/Fitted Materials";
        private const string AssemblyName = "Level 30 Fitted Assembly";
        public const float SocketHeight = .025f;
        public const float SocketDiameter = .091f;

        public static readonly string[] TerminalIds =
        {
            "Battery_positive", "Battery_negative", "UP_Feed_IN", "UP_Feed_OUT",
            "UP_Return_IN", "UP_Return_OUT", "DOWN_Feed_IN", "DOWN_Feed_OUT",
            "DOWN_Return_IN", "DOWN_Return_OUT", "Upper_Limit_IN", "Upper_Limit_OUT",
            "Lower_Limit_IN", "Lower_Limit_OUT", "Motor_A", "Motor_B", "Top_Lamp_IN",
            "Top_Lamp_OUT", "R_LED_IN", "R_LED_OUT", "Lamp_A", "Lamp_K"
        };

        private static readonly string[] Titles =
        {
            "5 V SUPPLY", "UP FEED", "UP RETURN", "DOWN FEED", "DOWN RETURN",
            "UPPER LIMIT", "LOWER LIMIT", "33 Ω MOTOR", "TOP CONTACT", "330 Ω RESISTOR", "LAMP"
        };

        private static readonly Dictionary<string, string> Palette = new Dictionary<string, string>
        {
            {"titanium","9FB3C8"},{"steel","7089A2"},{"graphite","121D2B"},{"chrome","D2E1ED"},
            {"mid_steel","384C65"},{"rubber","181E25"},{"deep_steel","1B2A3C"},{"green","2CFC8F"},
            {"fog","A9C0CE"},{"orange","F97126"},{"white","E7F5FB"},{"cyan","23DAFD"},{"bezel","0C1826"},
            {"red","FF4158"},{"void","070D18"},{"amber","FFA82B"},{"screen","091F2C"},{"blue","1671E2"},
            {"copper","C2672F"},{"gold","EDB846"},{"warning","FFC842"},{"glass","57BCDA"},{"lamp_glass","FFB54C"}
        };

        [MenuItem("Tools/Project Spark/Level 30/Fit Breadboard Assembly")]
        public static void Fit()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Exit Play Mode before fitting Level 30.");
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(Model);
            if (model == null || AssetDatabase.LoadAssetAtPath<GameObject>(Prefab) == null)
                throw new InvalidOperationException("The Level 30 prefab or fitted model is missing.");
            var scene = SceneManager.GetSceneByPath(Scene);
            bool opened = !scene.isLoaded;
            if (opened) scene = EditorSceneManager.OpenScene(Scene, OpenSceneMode.Additive);
            GameObject root = null;
            try
            {
                var loader = scene.GetRootGameObjects()
                    .SelectMany(item => item.GetComponentsInChildren<SparkLevelLayoutLoader>(true)).Single();
                var layoutRoot = (Transform)new SerializedObject(loader).FindProperty("layoutRoot").objectReferenceValue;
                var board = FindBreadboard(scene);
                Physics.SyncTransforms();
                root = PrefabUtility.LoadPrefabContents(Prefab);
                Fit(root, model, board, layoutRoot != null ? layoutRoot.localToWorldMatrix : Matrix4x4.identity);
                if (PrefabUtility.SaveAsPrefabAsset(root, Prefab) == null)
                    throw new InvalidOperationException("Unity could not save the Level 30 prefab.");
                AssetDatabase.SaveAssets();
                Debug.Log("Level 30 assembly fitted to the actual MainMenu breadboard. All 22 terminals were seated.");
            }
            finally
            {
                if (root != null) PrefabUtility.UnloadPrefabContents(root);
                if (opened) EditorSceneManager.CloseScene(scene, true);
            }
        }

        public static Transform FindBreadboard(UnityEngine.SceneManagement.Scene scene)
        {
            return scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<Transform>(true))
                .Single(item => item.name == "bread-board");
        }

        public static Vector3 SurfacePoint(Transform board, Vector3 horizontalPosition, Vector3 up)
        {
            var ray = new Ray(horizontalPosition + up * 2f, -up);
            float nearest = float.PositiveInfinity;
            Vector3 surface = default;
            foreach (var collider in board.GetComponents<Collider>())
            {
                if (collider.enabled && collider.Raycast(ray, out var hit, 4f) &&
                    Vector3.Dot(hit.normal, up) > .8f && hit.distance < nearest)
                {
                    nearest = hit.distance;
                    surface = hit.point;
                }
            }
            if (float.IsInfinity(nearest))
                throw new InvalidOperationException($"No breadboard surface under {horizontalPosition}.");
            return surface;
        }

        private static void Fit(GameObject root, GameObject model, Transform board, Matrix4x4 layoutRoot)
        {
            var bench = root.transform.Find("Lift Workbench");
            var bindings = root.GetComponent<SparkLevelSceneBindings>();
            var visuals = root.GetComponent<SparkLevel30Visuals>();
            var controller = root.GetComponent<SparkLevel30Lift>();
            if (bench == null || bindings == null || visuals == null || controller == null)
                throw new InvalidOperationException("The existing Level 30 layout is missing its controller or scene bindings.");
            var grid = board.GetComponent<BreadboardGrid>();
            if (grid == null) throw new InvalidOperationException("The breadboard grid is missing.");
            var gridData = new SerializedObject(grid);
            float spacing = gridData.FindProperty("spacing").floatValue;
            var halfGrid = new Vector3((gridData.FindProperty("columns").intValue - 1) * spacing * .5f, 0,
                (gridData.FindProperty("rows").intValue - 1) * spacing * .5f);
            Vector3 gridCenter = grid.GridOrigin.TransformPoint(halfGrid);
            Matrix4x4 parentToWorld = layoutRoot * bench.parent.localToWorldMatrix;
            Vector3 benchUp = parentToWorld.MultiplyVector(bench.localRotation * Vector3.up).normalized;
            Vector3 centerOffset = gridCenter - parentToWorld.MultiplyPoint3x4(bench.localPosition);
            bench.localPosition += parentToWorld.inverse.MultiplyVector(Vector3.ProjectOnPlane(centerOffset, benchUp));
            float width = grid.GridOrigin.TransformVector(Vector3.right * halfGrid.x).magnitude * 2f;
            float depth = grid.GridOrigin.TransformVector(Vector3.forward * halfGrid.z).magnitude * 2f;
            float worldScale = Mathf.Min(width / 2.13f, depth / 1.34f);
            bench.localScale = Vector3.one * (worldScale / parentToWorld.MultiplyVector(Vector3.right).magnitude);
            var benchToWorld = layoutRoot * bench.localToWorldMatrix;
            Vector3 up = benchToWorld.MultiplyVector(Vector3.up).normalized;
            Vector3 ToWorld(Vector3 point) => benchToWorld.MultiplyPoint3x4(point);
            Vector3 ToLocal(Vector3 point) => benchToWorld.inverse.MultiplyPoint3x4(point);
            float SurfaceAt(Vector3 point) => ToLocal(SurfacePoint(board, ToWorld(point), up)).y;

            var oldModel = bench.Find(AssemblyName);
            if (oldModel != null) Object.DestroyImmediate(oldModel.gameObject);
            HidePrototypes(bench, bindings);
            var assembly = Object.Instantiate(model, bench, false).transform;
            assembly.name = AssemblyName;
            var parts = assembly.GetComponentsInChildren<Transform>(true).ToDictionary(part => part.name);
            Transform Part(string name) => parts["L30_" + name];
            Vector3 Axis(string name) => bench.InverseTransformVector(Part(name).position - Part("BoardCenter").position);
            assembly.localRotation = Quaternion.Inverse(Quaternion.LookRotation(-Axis("BoardFront"), Axis("BoardUp"))) * assembly.localRotation;
            assembly.localScale /= Axis("BoardRight").magnitude;
            assembly.localPosition += Vector3.up * SurfaceAt(Vector3.zero) - bench.InverseTransformPoint(Part("BoardCenter").position);
            if (Axis("BoardRight").x < .99f) throw new InvalidOperationException("The imported model has a mirrored board axis.");

            if (!AssetDatabase.IsValidFolder(Materials)) AssetDatabase.CreateFolder(Folder.TrimEnd('/') + "/Models", "Fitted Materials");
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) throw new InvalidOperationException("Level 30 requires the project's URP Lit shader.");
            foreach (var mesh in assembly.GetComponentsInChildren<MeshRenderer>())
            {
                string key = mesh.name.Substring(mesh.name.LastIndexOf("__", StringComparison.Ordinal) + 2);
                mesh.sharedMaterial = MaterialFor(shader, key, Palette[key]);
            }

            var floorLabels = Child(assembly, "Socket labels", Vector3.zero);
            // Keep labels in workbench coordinates, independent of the FBX's import transform.
            floorLabels.SetParent(bench, false);
            var previousLabels = bench.Find("Socket labels");
            if (previousLabels != null && previousLabels != floorLabels) Object.DestroyImmediate(previousLabels.gameObject);
            int pinLayer = LayerMask.NameToLayer("Pin");
            if (pinLayer < 0) throw new InvalidOperationException("The Pin layer is missing.");
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var jackPoints = new Vector3[TerminalIds.Length];
            var occupied = new HashSet<Vector2Int>();
            for (int i = 0; i < TerminalIds.Length; i++)
            {
                string id = TerminalIds[i];
                if (!bindings.TryGetTerminal(id, out var terminal))
                    throw new InvalidOperationException("Missing Level 30 terminal: " + id);
                var point = bench.InverseTransformPoint(Part("Jack_" + id).position);
                var projected = ToWorld(point);
                projected -= up * Vector3.Dot(projected - grid.GridOrigin.position, up);
                if (!grid.TryGetNearestHole(projected, .32f, out var hole, out int column, out int row) ||
                    !occupied.Add(new Vector2Int(column, row)))
                    throw new InvalidOperationException("No separate breadboard hole for " + id);
                point = ToLocal(hole);
                point.y = SurfaceAt(point);
                jackPoints[i] = point;
                string colorKey = i < 2 ? (i == 0 ? "red" : "blue") :
                    i is 2 or 3 or 4 or 5 or 10 or 11 ? "cyan" :
                    i is 6 or 7 or 8 or 9 or 12 or 13 ? "amber" : i is 14 or 15 ? "gold" : "green";
                SeatTerminal(terminal, bench, point, pinLayer, MaterialFor(shader, colorKey, Palette[colorKey]),
                    MaterialFor(shader, "void", Palette["void"]));
                string text = i < 2 ? (i == 0 ? "+" : "−") :
                    i is 14 or 20 ? "A" : i == 15 ? "B" : i == 21 ? "K" : i % 2 == 0 ? "IN" : "OUT";
                ColorUtility.TryParseHtmlString("#" + Palette[colorKey], out var color);
                Label(floorLabels, font, id, text, point + new Vector3(0, .004f, -.054f), .020f,
                    Color.Lerp(color, Color.white, .55f));
                Debug.Log($"[Level 30 seating] {id} base {ToWorld(point):F4} on {board.name}");
            }
            for (int i = 0; i < Titles.Length; i++)
            {
                var middle = (jackPoints[i * 2] + jackPoints[i * 2 + 1]) * .5f;
                Label(floorLabels, font, Titles[i], Titles[i], middle + new Vector3(0, .004f, -.090f), .021f, Color.white);
            }

            // Wrap FBX pivots in aligned transforms so the existing visual controller rotates about board axes.
            Transform Wrap(string part)
            {
                var target = Part(part);
                var wrapper = Child(bench, part + " Pivot", Vector3.zero);
                wrapper.position = target.position;
                wrapper.rotation = bench.rotation;
                target.SetParent(wrapper, true);
                return wrapper;
            }

            visuals.shaft = Wrap("Motor_Shaft");
            visuals.leadScrew = Wrap("Lift_Screw");
            visuals.bulb = Part("Lift_Bulb").GetComponentInChildren<MeshRenderer>();
            visuals.socketBulb = Part("Lamp_Bulb").GetComponentInChildren<MeshRenderer>();
            visuals.selectorUp = Part("Selector_UP").GetComponentInChildren<MeshRenderer>();
            visuals.selectorOff = Part("Selector_OFF").GetComponentInChildren<MeshRenderer>();
            visuals.selectorDown = Part("Selector_DOWN").GetComponentInChildren<MeshRenderer>();
            var selector = bench.InverseTransformPoint(Part("Selector").position);
            Label(floorLabels, font, "Direction display", "DIRECTION", selector + new Vector3(0, .004f, -.099f), .025f, Color.white);
            for (int i = 0; i < 3; i++)
                Label(floorLabels, font, "Direction " + i, new[] { "UP", "OFF", "DN" }[i],
                    selector + new Vector3((i - 1) * .068f, .004f, -.067f), .014f, Color.white);

            string[] contactNames = { "UpFeed", "UpReturn", "DownFeed", "DownReturn", "Upper", "Lower", "Top" };
            foreach (var visual in visuals.contacts)
            {
                var contact = visual.contact;
                int index = Array.IndexOf(new[] { "UP Feed", "UP Return", "DOWN Feed", "DOWN Return",
                    "Upper Limit", "Lower Limit", "Top Lamp Contact" }, contact.name);
                if (index < 0) throw new InvalidOperationException("Unknown Level 30 contact: " + contact.name);
                string name = contactNames[index];
                visual.lever = index is 4 or 5 ? Wrap(name + "_Arm") : null;
                visual.indicator = index < 4 ? Part(name + "_Indicator").GetComponentInChildren<MeshRenderer>() : null;
                if (index == 6)
                {
                    visuals.topContact = contact;
                    visuals.topPress = Wrap("Top_Press");
                }
            }
            var platform = Wrap("Lift_Platform");
            var oldPlatform = bench.Find("Moving platform");
            if (oldPlatform != null) oldPlatform.gameObject.SetActive(false);
            var lampLight = Child(Part("Lift_Bulb"), "Lift lamp light", Vector3.zero).gameObject.AddComponent<Light>();
            lampLight.type = LightType.Point;
            lampLight.range = bench.lossyScale.x * .24f;
            lampLight.color = new Color(1f, .75f, .3f);
            lampLight.enabled = false;
            visuals.bulbLight = lampLight;
            var lampData = new SerializedObject(visuals.lamp);
            lampData.FindProperty("ledHelix").objectReferenceValue = null;
            lampData.ApplyModifiedPropertiesWithoutUndo();
            var controllerData = new SerializedObject(controller);
            controllerData.FindProperty("platform").objectReferenceValue = platform;
            controllerData.FindProperty("travelHeight").floatValue = .20f;
            controllerData.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void HidePrototypes(Transform bench, SparkLevelSceneBindings bindings)
        {
            var oldNames = new[] { "Board", "Raised back edge", "Raised front edge", "Lift pedestal",
                "Guide rail", "Upper crossbar", "Rotating lead screw", "Moving platform" };
            for (int i = bench.childCount - 1; i >= 0; i--)
            {
                var child = bench.GetChild(i);
                if (child.name is "Socket labels" or "Lift_Platform Pivot" or "Top_Press Pivot" or "Motor_Shaft Pivot"
                    or "Lift_Screw Pivot" or "Upper_Arm Pivot" or "Lower_Arm Pivot")
                    Object.DestroyImmediate(child.gameObject);
                else if (Array.IndexOf(oldNames, child.name) >= 0) child.gameObject.SetActive(false);
            }
            var owners = new HashSet<Transform>();
            foreach (string id in TerminalIds)
            {
                if (!bindings.TryGetTerminal(id, out var terminal))
                    throw new InvalidOperationException("Missing terminal " + id);
                owners.Add(terminal.transform.parent);
            }
            foreach (var owner in owners)
                foreach (Transform child in owner)
                    if (child.GetComponent<SparkTerminal>() == null) child.gameObject.SetActive(false);
        }

        private static void SeatTerminal(SparkTerminal terminal, Transform bench, Vector3 basePoint, int pinLayer,
            Material rim, Material recess)
        {
            var item = terminal.transform;
            var center = bench.TransformPoint(basePoint + Vector3.up * (SocketHeight * .5f));
            item.SetPositionAndRotation(center, bench.rotation);
            item.localScale = new Vector3(SocketDiameter, SocketHeight * .5f, SocketDiameter);
            item.gameObject.layer = pinLayer;
            var renderer = item.GetComponent<Renderer>();
            renderer.enabled = true;
            renderer.sharedMaterial = rim;
            var disc = item.Find("Jack recess");
            if (disc == null)
            {
                var primitive = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                Object.DestroyImmediate(primitive.GetComponent<Collider>());
                disc = primitive.transform;
                disc.name = "Jack recess";
                disc.SetParent(item, false);
                disc.localPosition = new Vector3(0, 1.02f, 0);
                disc.localScale = new Vector3(.59f, .035f, .59f);
            }
            disc.GetComponent<Renderer>().sharedMaterial = recess;
            var box = item.GetComponent<BoxCollider>();
            foreach (var collider in item.GetComponents<Collider>())
                if (collider != box) Object.DestroyImmediate(collider);
            if (box == null) box = item.gameObject.AddComponent<BoxCollider>();
            box.size = new Vector3(1.04f, 2.24f, 1.04f);
        }

        private static Transform Child(Transform parent, string name, Vector3 position)
        {
            var child = new GameObject(name).transform;
            child.SetParent(parent, false);
            child.localPosition = position;
            return child;
        }

        private static void Label(Transform parent, Font font, string name, string value, Vector3 position, float size, Color color)
        {
            var child = Child(parent, name, position);
            child.localRotation = Quaternion.Euler(90, 0, 0);
            child.localScale = Vector3.one * .3f;
            var label = child.gameObject.AddComponent<TextMesh>();
            label.font = font;
            label.fontSize = 64;
            label.characterSize = size;
            label.anchor = TextAnchor.MiddleCenter;
            label.text = value;
            label.color = color;
            child.GetComponent<MeshRenderer>().sharedMaterial = font.material;
        }

        private static Material MaterialFor(Shader shader, string key, string hex)
        {
            string path = $"{Materials}/L30 {key}.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null) return material;
            ColorUtility.TryParseHtmlString("#" + hex, out Color color);
            material = new Material(shader) { name = "L30 " + key };
            material.SetColor("_BaseColor", color);
            bool metal = key is "chrome" or "copper" or "gold" or "steel" or "titanium";
            material.SetFloat("_Metallic", metal ? .50f : .02f);
            material.SetFloat("_Smoothness", metal ? .65f : .34f);
            if (key is "cyan" or "green" or "red" or "blue" or "gold" or "amber" or "lamp_glass" or "screen")
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", color * (key == "lamp_glass" || key == "screen" ? 0f : .32f));
            }
            if (key == "glass")
            {
                color.a = .24f;
                material.SetColor("_BaseColor", color);
                material.SetFloat("_Surface", 1f);
                material.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
                material.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
                material.SetFloat("_ZWrite", 0f);
                material.SetOverrideTag("RenderType", "Transparent");
                material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                material.SetShaderPassEnabled("ShadowCaster", false);
                material.renderQueue = (int)RenderQueue.Transparent;
            }
            AssetDatabase.CreateAsset(material, path);
            return material;
        }
    }
}
#endif
