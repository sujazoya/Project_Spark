#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using ProjectSpark.Circuit;
using ProjectSpark.Gameplay;
using ProjectSpark.UI.VisualTeaching;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace ProjectSpark.EditorTools
{
    public static class SparkLevel47ModelInstaller
    {
        public const string Folder = "Assets/My_Assets/_Project_Spark/Prefabs/Levels/Level_47_Layout/";
        public const string Prefab = Folder + "Level_47_Tension_Layout.prefab";
        public const string ScenePath = "Assets/My_Assets/_Project_Spark/Scenes/MainMenu/MainMenu.unity";
        private const string Model = Folder + "Models/Level47_Fitted_Assembly.fbx";
        private const string Materials = Folder + "Models/Materials";
        private const float SocketHeight = .025f;
        private const float SocketDiameter = .060f;

        private static readonly (string id, string label, float x, float z, string color)[] Sockets =
        {
            ("SUPPLY_PLUS_5V","+5 V",0.7300f,0.4370f,"red"),
            ("CTRL_PLUS_5V","C +",-0.8400f,0.4120f,"red"),
            ("SUPPLY_RETURN","RET",0.9300f,0.4370f,"blue"),
            ("CTRL_GND","C RET",-0.6300f,0.4120f,"blue"),
            ("CTRL_FEED_PWM","F PWM",-0.4200f,0.4120f,"cyan"),
            ("DRIVER_FEED_PWM","F PWM",-0.2700f,-0.3860f,"cyan"),
            ("CTRL_TAKEUP_PWM","T PWM",-0.2100f,0.4120f,"amber"),
            ("DRIVER_TAKEUP_PWM","T PWM",-0.2700f,-0.6380f,"amber"),
            ("DRIVER_FEED_PLUS","F +",0.0000f,-0.3860f,"cyan"),
            ("FEED_MOTOR_PLUS","F +",-0.6800f,-0.3530f,"cyan"),
            ("DRIVER_FEED_MINUS","F −",0.2700f,-0.3860f,"cyan"),
            ("FEED_MOTOR_MINUS","F −",-0.4800f,-0.3530f,"cyan"),
            ("DRIVER_TAKEUP_PLUS","T +",0.0000f,-0.6380f,"amber"),
            ("TAKEUP_MOTOR_PLUS","T +",0.4800f,-0.3530f,"amber"),
            ("DRIVER_TAKEUP_MINUS","T −",0.2700f,-0.6380f,"amber"),
            ("TAKEUP_MOTOR_MINUS","T −",0.6700f,-0.3530f,"amber"),
            ("FEED_ENC_A","F A",-0.1050f,0.3160f,"cyan"),
            ("CTRL_FEED_A","F A IN",-0.8600f,0.2820f,"cyan"),
            ("FEED_ENC_B","F B",-0.0520f,0.2220f,"cyan"),
            ("CTRL_FEED_B","F B IN",-0.6700f,0.2820f,"cyan"),
            ("TAKEUP_ENC_A","T A",0.1380f,0.2220f,"amber"),
            ("CTRL_TAKEUP_A","T A IN",-0.4800f,0.2820f,"amber"),
            ("TAKEUP_ENC_B","T B",0.3400f,0.2220f,"amber"),
            ("CTRL_TAKEUP_B","T B IN",-0.2900f,0.2820f,"amber"),
            ("SENSOR_PLUS_5V","S +",0.3600f,0.3710f,"red"),
            ("SENSOR_GND","S RET",0.5400f,0.3710f,"blue"),
            ("SENSOR_SIGNAL","S OUT",0.5100f,0.2470f,"mint"),
            ("CTRL_TENSION_IN","TENSION IN",-0.0050f,0.4120f,"mint"),
        };

        public static IEnumerable<string> TerminalIds => Sockets.Select(socket => socket.id);

        private static readonly Dictionary<string, string> Palette = new Dictionary<string, string>
        {
            {"amber","FFA82B"},
            {"blue","1671E2"},
            {"brass","E3A24A"},
            {"bronze","AE6B39"},
            {"ceramic","E5DECE"},
            {"chrome","D2E1ED"},
            {"cyan","23DAFD"},
            {"deep_steel","1B2A3C"},
            {"fabriclight","C8DBE6"},
            {"film","CAE5E8"},
            {"gold","EDB846"},
            {"graphite","121D2B"},
            {"green","2CFC8F"},
            {"ice","DAF1FB"},
            {"insulation","182734"},
            {"mid_steel","384C65"},
            {"mint","2BF6C2"},
            {"motor_blue","1D6EBC"},
            {"nickel","A9B5BE"},
            {"orange","F97126"},
            {"polished","C3DAE2"},
            {"red","FF4158"},
            {"rubber","181E25"},
            {"screen","091F2C"},
            {"silver","BFD0DB"},
            {"spring","EDAF30"},
            {"stainless","BBD3DB"},
            {"steel","7089A2"},
            {"titanium","9FB3C8"},
            {"void","070D18"},
            {"warm","FCAB4D"},
            {"white","E7F5FB"},
        };

        [MenuItem("Tools/Project Spark/Level 47/Install and Register")]
        public static void Install()
        {
            SparkLevel47Builder.Create();
            Fit();
            Register();
            ValidateAssets();
        }

        [MenuItem("Tools/Project Spark/Level 47/Validate Level Assets")]
        public static void ValidateAssets()
        {
            var root=AssetDatabase.LoadAssetAtPath<GameObject>(Prefab);
            var control=root != null ? root.GetComponent<SparkLevel47Tension>() : null;
            var bindings=root != null ? root.GetComponent<SparkLevelSceneBindings>() : null;
            if (control==null || bindings==null || control.rig==null || control.feedDisplay==null ||
                control.pins.Length!=28 || control.pins.Distinct().Count()!=28 || control.lamps.Length!=16 ||
                control.webSlats.Length<1 || !TerminalIds.SequenceEqual(control.pins.Select(pin=>pin.name)) ||
                control.pins.Any(pin=>!bindings.TryGetTerminal(pin.name,out var bound) || bound!=pin) ||
                SparkLevel47Tension.Gates.Any(id=>!bindings.IsComponentIdDeclared(id)))
                throw new InvalidOperationException("Level 47 wiring, motion or binding references are incomplete.");
            Debug.Log("[LEVEL 47] Validated 28 distinct terminals, 15 player wires and 6 evaluator gates.",root);
        }

        private static void Register()
        {
            var scene = SceneManager.GetSceneByPath(ScenePath);
            bool opened = !scene.isLoaded;
            if (opened) scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
            bool dirty = scene.isDirty;
            var objects = scene.GetRootGameObjects();
            T Find<T>() where T : Component => objects.SelectMany(item => item.GetComponentsInChildren<T>(true)).Single();
            var level = AssetDatabase.LoadAssetAtPath<SparkLevelDefinition>(Folder + "Level_47.asset");
            var drawing = AssetDatabase.LoadAssetAtPath<SparkLevelDrawingAsset>(Folder + "Level_47_Drawing.asset");
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(Prefab).GetComponent<SparkLevelLayout>();
            Append(Find<LevelGamePlayManager>(), "levels", level);
            Append(Find<SparkDrawCurrentLevel>(), "drawings", drawing);
            var loader = new SerializedObject(Find<SparkLevelLayoutLoader>());
            var catalog = new SerializedObject(loader.FindProperty("catalog").objectReferenceValue);
            var entries = catalog.FindProperty("entries");
            var entry = Enumerable.Range(0, entries.arraySize).Select(entries.GetArrayElementAtIndex)
                .FirstOrDefault(item => item.FindPropertyRelative("level").objectReferenceValue == level);
            if (entry == null)
            {
                entries.arraySize++;
                entry = entries.GetArrayElementAtIndex(entries.arraySize - 1);
            }
            entry.FindPropertyRelative("level").objectReferenceValue = level;
            entry.FindPropertyRelative("prefab").objectReferenceValue = prefab;
            catalog.ApplyModifiedProperties();
            AssetDatabase.SaveAssets();
            if (!dirty && scene.isDirty && !EditorSceneManager.SaveScene(scene))
                throw new InvalidOperationException("Could not save campaign registration.");
            if (opened) EditorSceneManager.CloseScene(scene, true);
            else if (dirty) Debug.Log("MainMenu had unsaved edits. Save the scene to persist Level 47's campaign and teaching entries.");
        }

        private static void Append(Object target, string field, Object value)
        {
            if (value == null) throw new InvalidOperationException("Missing Level 47 asset for " + field);
            var data = new SerializedObject(target);
            var list = data.FindProperty(field);
            for (int i = 0; i < list.arraySize; i++)
                if (list.GetArrayElementAtIndex(i).objectReferenceValue == value) return;
            list.arraySize++;
            list.GetArrayElementAtIndex(list.arraySize - 1).objectReferenceValue = value;
            data.ApplyModifiedProperties();
        }

        [MenuItem("Tools/Project Spark/Level 47/Fit Breadboard Assembly")]
        public static void Fit()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Exit Play Mode before fitting Level 47.");
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(Model);
            if (model == null || AssetDatabase.LoadAssetAtPath<GameObject>(Prefab) == null)
                throw new InvalidOperationException("The fitted FBX or existing Level 47 prefab is missing.");

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
                    throw new InvalidOperationException("Unity could not save the Level 47 layout.");
                AssetDatabase.SaveAssets();
                Debug.Log("Level 47 fitted to the MainMenu breadboard with 28 seated terminals.");
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
            var bench = root.transform.Find("Winding Workbench");
            var bindings = root.GetComponent<SparkLevelSceneBindings>();
            var loop = root.GetComponent<SparkLevel47Tension>();
            if (bench == null || bindings == null || loop == null)
                throw new InvalidOperationException("Level 47 is missing its workbench, controls, or scene bindings.");

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

            foreach (string name in new[] {"Fitted Level 47 Models", "Breadboard Labels", "Motion pivots"})
            {
                var existing = bench.Find(name);
                if (existing != null) Object.DestroyImmediate(existing.gameObject);
            }
            foreach (string name in new[] {"Temporary board"})
            {
                var existing = bench.Find(name);
                if (existing != null) existing.gameObject.SetActive(false);
            }

            var assembly = Object.Instantiate(model, bench, false).transform;
            assembly.name = "Fitted Level 47 Models";
            var parts = assembly.GetComponentsInChildren<Transform>(true).ToDictionary(item => item.name);
            Transform Part(string name) => parts["L47_" + name];
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
                const float labelOffset = -.056f;
                Label(labels, font, spec.id, spec.label, seat + new Vector3(0, .014f, labelOffset), .028f);
            }
            foreach (var owner in owners)
                foreach (Transform child in owner)
                    if (child.GetComponent<SparkTerminal>() == null && child.GetComponent<Renderer>() != null)
                        child.gameObject.SetActive(false);

            var titles = new (string title, float x, float z)[]
            {
                ("TENSION CONTROLLER",-.444f,.672f),("TENSION MONITOR",.29f,.674f),("5 V SUPPLY",.80f,.674f),
                ("FEED",-.54f,-.086f),("TAKE-UP",.54f,-.086f),("FEED BRAKE",-.83f,.171f),("DUAL DRIVER",0,-.518f)
            };
            foreach (var title in titles)
                Label(labels,font,title.title,title.title,new Vector3(title.x,floor+.027f,title.z),.025f);
            var tuning=new SerializedObject(loop);
            if (Mathf.Approximately(tuning.FindProperty("totalLength").floatValue,1.8f) &&
                Mathf.Approximately(tuning.FindProperty("tensionCorrection").floatValue,.068f))
            {
                tuning.FindProperty("totalLength").floatValue=3.8f;
                tuning.FindProperty("tensionCorrection").floatValue=.027f;
                tuning.FindProperty("speedP").floatValue=.0025f;
                tuning.FindProperty("speedI").floatValue=.006f;
                tuning.ApplyModifiedPropertiesWithoutUndo();
            }
            var motion=Child(bench,"Motion pivots",Vector3.zero);
            Transform Pivot(string part,string name)
            {
                var modelPart=Part(part);
                var pivot=Child(motion,name,Vector3.zero);
                pivot.SetPositionAndRotation(modelPart.position,bench.rotation);
                modelPart.SetParent(pivot,true);
                return pivot;
            }
            var rig=new SerializedObject(loop.rig);
            foreach (var (field,part) in new[] {
                ("feedMotorShaft","FeedMotor_Shaft"),("takeupMotorShaft","TakeupMotor_Shaft"),
                ("feedSpool","FeedSpool_Spool"),("takeupSpool","TakeupSpool_Spool"),
                ("feedWoundMaterial","FeedSpool_Material"),("takeupWoundMaterial","TakeupSpool_Material"),
                ("feedEncoderDisc","Encoder_Feed"),("takeupEncoderDisc","Encoder_Takeup"),
                ("dancerArm","Dancer_Arm"),("dancerRoller","Dancer_Roller"),("tensionSensorWiper","Sensor_Wiper"),
                ("feedBrakeRotor","Brake_Rotor"),("brakeLever","Brake_Lever"),("brakePad","Brake_Pad") })
                rig.FindProperty(field).objectReferenceValue=Pivot(part,field);
            var roller=(Transform)rig.FindProperty("dancerRoller").objectReferenceValue;
            var arm=(Transform)rig.FindProperty("dancerArm").objectReferenceValue;
            roller.SetParent(arm,true);
            rig.FindProperty("maximumBrakePadTravel").floatValue=.017f;
            rig.FindProperty("maximumDancerAngle").floatValue=22.92f;
            rig.ApplyModifiedPropertiesWithoutUndo();
            loop.webSlats=parts.Keys.Where(key=>key.StartsWith("L47_Web_Slat") && !key.Contains("__"))
                .OrderBy(key=>key).Select(key=>Pivot(key.Substring(4),key)).ToArray();
            loop.lamps=new[]{"Supply_Power","Driver_Feed","Driver_Takeup","Controller_Manual","Controller_Auto","Controller_Fault",
                "Brake_Load","Monitor_Safe","Monitor_Slack","Monitor_Trip","Sensor_Ready","Dancer_Centered",
                "Encoder_FeedA","Encoder_FeedB","Encoder_TakeupA","Encoder_TakeupB"}.Select(name=>Part(name).gameObject).ToArray();
            TMP_Text Readout(string name,Vector3 position,Color color)
            {
                var child=Child(labels,name,position);
                child.localRotation=Quaternion.Euler(90,0,0);
                child.localScale=Vector3.one*.20f;
                var display=child.gameObject.AddComponent<TextMeshPro>();
                display.fontSize=1.08f;
                display.alignment=TextAlignmentOptions.Center;
                display.textWrappingMode=TextWrappingModes.NoWrap;
                display.color=color;
                display.text="—";
                return display;
            }
            loop.feedDisplay=Readout("Winding controller screen",new Vector3(-.444f,floor+.139f,.563f),new Color(.46f,.94f,1f));
            loop.takeupDisplay=Readout("Dancer sensor screen",new Vector3(.09f,floor+.119f,.314f),new Color(1f,.72f,.38f));
            loop.tensionDisplay=Readout("Tension screen",new Vector3(.29f,floor+.127f,.577f),new Color(.68f,1f,.86f));
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
            string path = $"{Materials}/L47 {key}.mat";
            var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing != null && (key!="film" || existing.GetFloat("_Surface")>.5f)) return existing;
            ColorUtility.TryParseHtmlString("#" + (Palette.TryGetValue(key, out var tint) ? tint : "9FB3C8"), out var color);
            var material = existing != null ? existing : new Material(shader) { name = "L47 " + key };
            bool metal = key is "chrome" or "titanium" or "steel" or "gold" or "copper" or "aluminum";
            material.SetFloat("_Metallic", metal ? .47f : .02f);
            material.SetFloat("_Smoothness", metal ? .64f : .35f);
            if (key is "green" or "cyan" or "red" or "blue" or "orange" or "amber" or "mint" or "warm" or "ice")
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", color * .47f);
            }
            if (key is "glass" or "glass_body" or "glass_soft" or "acrylic" or "film")
            {
                color.a = key=="film" ? .53f : .14f;
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
            if (existing == null) AssetDatabase.CreateAsset(material, path);
            else EditorUtility.SetDirty(material);
            return material;
        }
    }
}
#endif






