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
    public static class SparkLevel54ModelInstaller
    {
        public const string Folder = "Assets/My_Assets/_Project_Spark/Prefabs/Levels/Level_54_Layout/";
        public const string Prefab = Folder + "Level_54_Layout.prefab";
        public const string ScenePath = "Assets/My_Assets/_Project_Spark/Scenes/MainMenu/MainMenu.unity";
        private static readonly string[] Models = {
            "Frame","Motor","Shaft","Rotor","BearingL","BearingR","ProbeX","ProbeY","Encoder",
            "Damper","DamperDriver","MotorDriver","Controller","Monitor","Guard","Calibration","Supply"
        };
        private const string Materials = Folder + "Models/Materials";
        private const float SocketHeight = .025f;
        private const float SocketDiameter = .045f;

        private static readonly (string id, string label, float x, float z, string color)[] Sockets =
        {
            ("SUPPLY_POS","+5 V",-0.9520f,-0.5650f,"red"),
            ("CTRL_POS","C +",-0.1610f,-0.5650f,"red"),
            ("SUPPLY_NEG","RETURN",-0.8390f,-0.5650f,"blue"),
            ("CTRL_GND","C GND",-0.0480f,-0.5650f,"blue"),
            ("MDRV_POS","MD +",-0.7260f,-0.5650f,"red"),
            ("MDRV_GND","MD GND",-0.6130f,-0.5650f,"blue"),
            ("XS_POS","X +",-0.3190f,0.4170f,"red"),
            ("XS_GND","X GND",-0.1250f,0.4170f,"blue"),
            ("YS_POS","Y +",0.2630f,0.4170f,"red"),
            ("YS_GND","Y GND",0.4570f,0.4170f,"blue"),
            ("DDRV_POS","DD +",-0.0270f,-0.2230f,"red"),
            ("DDRV_GND","DD GND",0.2010f,-0.2230f,"blue"),
            ("CTRL_MOTOR_PWM","PWM",0.0650f,-0.5650f,"cyan"),
            ("MDRV_PWM","MD PWM",-0.5000f,-0.5650f,"cyan"),
            ("MDRV_MPOS","M OUT +",-0.3870f,-0.5650f,"amber"),
            ("MOTOR_POS","MOTOR +",-0.9390f,-0.2230f,"amber"),
            ("MDRV_MNEG","M OUT −",-0.2740f,-0.5650f,"blue"),
            ("MOTOR_NEG","MOTOR −",-0.7110f,-0.2230f,"blue"),
            ("XS_OUT","X OUT",0.0690f,0.4170f,"cyan"),
            ("CTRL_X_IN","X IN",0.1780f,-0.5650f,"cyan"),
            ("YS_OUT","Y OUT",0.6510f,0.4170f,"cyan"),
            ("CTRL_Y_IN","Y IN",0.2910f,-0.5650f,"cyan"),
            ("ENC_A","ENC A",-0.9010f,0.4170f,"cyan"),
            ("CTRL_ENC_A","A IN",0.4040f,-0.5650f,"cyan"),
            ("ENC_B","ENC B",-0.7070f,0.4170f,"cyan"),
            ("CTRL_ENC_B","B IN",0.5170f,-0.5650f,"cyan"),
            ("ENC_Z","ENC Z",-0.5130f,0.4170f,"cyan"),
            ("CTRL_ENC_Z","Z IN",0.6300f,-0.5650f,"cyan"),
            ("CTRL_DAMP_CMD","DAMP CMD",0.7430f,-0.5650f,"cyan"),
            ("DDRV_CMD","DD CMD",0.4290f,-0.2230f,"cyan"),
            ("DDRV_ACTPOS","DD OUT +",0.6570f,-0.2230f,"amber"),
            ("DAMP_POS","ACT +",-0.4830f,-0.2230f,"amber"),
            ("DDRV_ACTNEG","DD OUT −",0.8850f,-0.2230f,"blue"),
            ("DAMP_NEG","ACT −",-0.2550f,-0.2230f,"blue"),
            ("GUARD_NC","GUARD",0.8450f,0.4170f,"cyan"),
            ("CTRL_SAFE_IN","SAFE",0.8560f,-0.5650f,"cyan"),
        };

        public static IEnumerable<string> TerminalIds => Sockets.Select(socket => socket.id);

        private static readonly Dictionary<string, string> Palette = new Dictionary<string, string>
        {
            {"amber","FFA82B"},
            {"blue","1671E2"},
            {"cyan","23DAFD"},
            {"mint","34F6C5"},
            {"red","EE485C"},
            {"void","07111C"},
            {"titanium","24455C"},
            {"navy","0E2133"},
            {"carbon","081115"},
            {"gunmetal","30424C"},
            {"aluminum","6B8A99"},
            {"chrome","BFD9E6"},
            {"nickel","94A8B5"},
            {"electric","308CDE"},
            {"teal","21D4AD"},
            {"copper","C75E21"},
            {"scarlet","F5362B"},
            {"gold","F0BA59"},
            {"white","E6F7FC"},
            {"rubber","0F1618"},
            {"glass","5CC7E8"},
            {"smoke","214A5C"},
            {"yellow","FAD12E"},
        };

        [MenuItem("Tools/Project Spark/Levels/Create Level 54")]
        public static void Install()
        {
            SparkLevel54Builder.Create();
            var root = AssetDatabase.LoadAssetAtPath<GameObject>(Prefab);
            if (root == null || root.GetComponent<SparkLevel54Critical>().controllerDisplay == null) Fit();
            Register();
            ValidateAssets();
        }

        [MenuItem("Tools/Project Spark/Level 54/Validate Level Assets")]
        public static void ValidateAssets()
        {
            var root=AssetDatabase.LoadAssetAtPath<GameObject>(Prefab);
            var control=root != null ? root.GetComponent<SparkLevel54Critical>() : null;
            var bindings=root != null ? root.GetComponent<SparkLevelSceneBindings>() : null;
            if (control==null || bindings==null || control.controllerDisplay==null || control.qualityDisplay==null || control.rig==null ||
                control.pins.Length!=36 || control.pins.Distinct().Count()!=36 || 
                !TerminalIds.SequenceEqual(control.pins.Select(pin=>pin.name)) ||
                control.pins.Any(pin=>!bindings.TryGetTerminal(pin.name,out var bound) || bound!=pin) ||
                SparkLevel54Critical.Gates.Any(id=>!bindings.IsComponentIdDeclared(id)))
                throw new InvalidOperationException("Level 54 wiring, motion or binding references are incomplete.");
            Debug.Log("[LEVEL 54] Validated 36 distinct terminals, 22 player wires and 6 evaluator gates.",root);
        }

        private static void Register()
        {
            var scene = SceneManager.GetSceneByPath(ScenePath);
            bool opened = !scene.isLoaded;
            if (opened) scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
            bool dirty = scene.isDirty;
            var objects = scene.GetRootGameObjects();
            T Find<T>() where T : Component => objects.SelectMany(item => item.GetComponentsInChildren<T>(true)).Single();
            var level = AssetDatabase.LoadAssetAtPath<SparkLevelDefinition>(Folder + "Level_54.asset");
            var drawing = AssetDatabase.LoadAssetAtPath<SparkLevelDrawingAsset>(Folder + "Level_54_Drawing.asset");
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
            else if (dirty) Debug.Log("MainMenu had unsaved edits. Save the scene to persist Level 54's campaign and teaching entries.");
        }

        private static void Append(Object target, string field, Object value)
        {
            if (value == null) throw new InvalidOperationException("Missing Level 54 asset for " + field);
            var data = new SerializedObject(target);
            var list = data.FindProperty(field);
            for (int i = 0; i < list.arraySize; i++)
                if (list.GetArrayElementAtIndex(i).objectReferenceValue == value) return;
            list.arraySize++;
            list.GetArrayElementAtIndex(list.arraySize - 1).objectReferenceValue = value;
            data.ApplyModifiedProperties();
        }

        [MenuItem("Tools/Project Spark/Level 54/Fit Breadboard Assembly")]
        public static void Fit()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Exit Play Mode before fitting Level 54.");
            var models = Models.Select((name, index) =>
            {
                string path = Folder + $"Models/L54_{index:00}_{name}_Fitted.fbx";
                return AssetDatabase.LoadAssetAtPath<GameObject>(path) ??
                    throw new InvalidOperationException("Missing fitted kit module " + path);
            }).ToArray();
            if (AssetDatabase.LoadAssetAtPath<GameObject>(Prefab) == null)
                throw new InvalidOperationException("The Level 54 prefab is missing.");

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
                Fit(root, models, board, layoutRoot != null ? layoutRoot.localToWorldMatrix : Matrix4x4.identity);
                if (PrefabUtility.SaveAsPrefabAsset(root, Prefab) == null)
                    throw new InvalidOperationException("Unity could not save the Level 54 layout.");
                AssetDatabase.SaveAssets();
                Debug.Log("Level 54 fitted to the MainMenu breadboard with 36 seated terminals.");
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
            Vector3 right = Vector3.ProjectOnPlane(grid.GridOrigin.right, up).normalized * .054f;
            Vector3 forward = Vector3.ProjectOnPlane(grid.GridOrigin.forward, up).normalized * .054f;
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

        private static void Fit(GameObject root, GameObject[] models, BreadboardGrid grid, Matrix4x4 layout)
        {
            var bench = root.transform.Find("Critical Workbench");
            var bindings = root.GetComponent<SparkLevelSceneBindings>();
            var loop = root.GetComponent<SparkLevel54Critical>();
            if (bench == null || bindings == null || loop == null)
                throw new InvalidOperationException("Level 54 is missing its workbench, controls, or scene bindings.");

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

            foreach (string name in new[] {"Fitted Level 54 Models", "Breadboard Labels", "Motion pivots"})
            {
                var existing = bench.Find(name);
                if (existing != null) Object.DestroyImmediate(existing.gameObject);
            }
            foreach (string name in new[] {"Temporary board"})
            {
                var existing = bench.Find(name);
                if (existing != null) existing.gameObject.SetActive(false);
            }

            foreach (var renderer in bench.GetComponentsInChildren<MeshRenderer>(true))
                if (renderer.name=="Body") renderer.gameObject.SetActive(false);
            var assembly = Child(bench, "Fitted Level 54 Models", Vector3.up * floor);
            foreach (var model in models)
            {
                var instance = Object.Instantiate(model, assembly, false).transform;
                var markers = instance.GetComponentsInChildren<Transform>(true)
                    .Where(item => item.name.StartsWith("L54_Board",StringComparison.Ordinal))
                    .ToDictionary(item => item.name);
                Transform Marker(string name) => markers["L54_Board" + name];
                Vector3 Axis(string name) => assembly.InverseTransformVector(Marker(name).position - Marker("Center").position);
                instance.localRotation = Quaternion.Inverse(Quaternion.LookRotation(-Axis("Front"), Axis("Up"))) * instance.localRotation;
                instance.localScale /= Axis("Right").magnitude;
                instance.localPosition -= assembly.InverseTransformPoint(Marker("Center").position);
                if (Axis("Right").x < .99f)
                    throw new InvalidOperationException("Unexpected axes on fitted model " + model.name);
            }

            if (!AssetDatabase.IsValidFolder(Materials)) AssetDatabase.CreateFolder(Folder + "Models", "Materials");
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) throw new InvalidOperationException("The project URP Lit shader is missing.");
            foreach (var mesh in assembly.GetComponentsInChildren<MeshRenderer>())
            {
                string key = mesh.name.Substring(mesh.name.LastIndexOf("__", StringComparison.Ordinal) + 2);
                var material = MaterialFor(shader, key);
                // The fitted meshes can contain several imported submeshes.
                mesh.sharedMaterials = Enumerable.Repeat(material, mesh.GetComponent<MeshFilter>().sharedMesh.subMeshCount).ToArray();
                if (key == "glass") mesh.shadowCastingMode = ShadowCastingMode.Off;
            }

            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (loop.calibrationText == null)
            {
                var oldControls = root.transform.Find("Level 54 Controls");
                if (oldControls != null) Object.DestroyImmediate(oldControls.gameObject);
                SparkLevel54Builder.BuildControls(root.transform,loop,font);
            }
            var labels = Child(bench, "Breadboard Labels", Vector3.zero);
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

                float offset=spec.id is "MDRV_PWM" or "CTRL_X_IN" or "CTRL_ENC_A" or "MDRV_POS"
                    ? -.079f : Array.IndexOf(Sockets,spec)%2==0 ? -.046f : -.059f;
                Label(labels,font,spec.id,spec.label,seat+new Vector3(0,.014f,offset),.016f);
            }
            foreach (var owner in owners)
                foreach (Transform child in owner)
                    if (child.GetComponent<SparkTerminal>() == null && child.GetComponent<Renderer>() != null)
                        child.gameObject.SetActive(false);

            var titles = new (string title, float x, float z)[]
            {






                ("FLEXIBLE ROTOR",0f,.315f),("DISPLACEMENT X/Y",-.09f,.243f),
                ("CRITICAL SPEED CONTROL",.04f,-.301f),("ORBIT / SPECTRUM",-.14f,.664f)
            };
            foreach(var title in titles)
                Label(labels,font,title.title,title.title,new Vector3(title.x,floor+.025f,title.z),.020f);
            var motion = Child(bench, "Motion pivots", Vector3.zero);
            var parts = assembly.GetComponentsInChildren<Transform>(true);
            Transform Pivot(string name)
            {
                var part = parts.Single(item => item.name == name);
                var pivot = Child(motion,name,Vector3.zero);
                pivot.SetPositionAndRotation(part.position,bench.rotation);
                part.SetParent(pivot,true);
                return pivot;
            }

            var rig = new SerializedObject(loop.rig);
            foreach (var (field, pivot) in new[] {
                ("motorShaft", "MOTOR_SHAFT_ROTATE_X"), ("rotorDisc", "TEST_ROTOR_ROTATE_X"),
                ("encoderDisc", "ENCODER_DISC_ROTATE_X"), ("leftFlexureBearing", "LEFT_FLEXURE_BEARING_YZ"),
                ("rightFlexureBearing", "RIGHT_FLEXURE_BEARING_YZ"), ("activeDamperX", "DAMPER_X_FORCE"),
                ("activeDamperY", "DAMPER_Y_FORCE"), ("safetyDoorHinge", "SAFETY_DOOR_HINGE"),
                ("probeX", "X_PROBE_ADJUST"), ("probeY", "Y_PROBE_ADJUST") })
                rig.FindProperty(field).objectReferenceValue = Pivot(pivot);
            var sections = rig.FindProperty("flexibleShaftSegments");
            sections.arraySize = 7;
            for (int i = 0; i < 7; i++)
                sections.GetArrayElementAtIndex(i).objectReferenceValue = Pivot($"SHAFT_FLEX_SEGMENT_{i:00}");
            Renderer Indicator(Transform pivot, string name, Vector3 offset, Color tint)
            {
                var marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                marker.name = name;
                marker.transform.SetParent(pivot, false);
                marker.transform.localPosition = offset;
                marker.transform.localScale = Vector3.one * .022f;
                Object.DestroyImmediate(marker.GetComponent<Collider>());
                var material = MaterialFor(shader, "cyan");
                var renderer = marker.GetComponent<Renderer>();
                renderer.sharedMaterial = material;
                var block = new MaterialPropertyBlock();
                block.SetColor("_EmissionColor", tint);
                renderer.SetPropertyBlock(block);
                return renderer;
            }
            var motionParts = motion.GetComponentsInChildren<Transform>(true);
            Transform MotionPart(string id) => motionParts.First(item => item.name == id);
            rig.FindProperty("xSensorLamp").objectReferenceValue =
                Indicator(MotionPart("X_PROBE_ADJUST"),"X signal lamp",Vector3.up * .033f,Color.cyan);
            rig.FindProperty("ySensorLamp").objectReferenceValue =
                Indicator(MotionPart("Y_PROBE_ADJUST"),"Y signal lamp",Vector3.up * .036f,Color.yellow);
            rig.FindProperty("activeDamperLamp").objectReferenceValue =
                Indicator(MotionPart("DAMPER_X_FORCE"),"Damper lamp",new Vector3(.006f,.079f,.018f),Color.green);
            rig.FindProperty("machineFaultLamp").objectReferenceValue =
                Indicator(labels,"Machine safety lamp",new Vector3(.29f,floor+.132f,-.40f),Color.green);
            rig.FindProperty("visualArtUnitsPerMillimeter").floatValue = .022f;
            rig.FindProperty("findNamedPivotsAtAwake").boolValue = false;
            rig.ApplyModifiedPropertiesWithoutUndo();

            TMP_Text Readout(string name,Vector3 position,Color color)
            {
                var child=Child(labels,name,position);
                child.localRotation=Quaternion.Euler(90,0,0);
                child.localScale=Vector3.one*.20f;
                var display=child.gameObject.AddComponent<TextMeshPro>();
                display.fontSize=.86f;
                display.alignment=TextAlignmentOptions.Center;
                display.textWrappingMode=TextWrappingModes.NoWrap;
                display.color=color;
                display.text="—";
                return display;
            }
            loop.controllerDisplay=Readout("Controller screen",new Vector3(.04f,floor+.118f,-.413f),new Color(.46f,.94f,1f));
            loop.qualityDisplay=Readout("Shaft orbit readout",new Vector3(-.14f,floor+.132f,.546f),new Color(.68f,1f,.86f));
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
            string path = $"{Materials}/L54 {key}.mat";
            var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing != null && key != "glass" &&
                ColorUtility.TryParseHtmlString("#" + (Palette.TryGetValue(key, out var savedTint) ? savedTint : "9FB3C8"), out var savedColor) &&
                existing.GetColor("_BaseColor") == savedColor) return existing;
            ColorUtility.TryParseHtmlString("#" + (Palette.TryGetValue(key, out var tint) ? tint : "9FB3C8"), out var color);
            var material = existing != null ? existing : new Material(shader) { name = "L54 " + key };
            bool metal = key is "chrome" or "titanium" or "steel" or "gold" or "copper" or "aluminum";
            material.SetFloat("_Metallic", metal ? .54f : .02f);
            material.SetFloat("_Smoothness", metal ? .64f : key == "glass" ? .12f : .35f);
            if (key is "green" or "cyan" or "red" or "blue" or "orange" or "amber" or "mint" or "warm" or "ice")
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", color * .54f);
            }
            if (key is "glass" or "glass_body" or "glass_soft" or "acrylic" or "glassblue" or "film" or "Glass" or "guardglass")
            {
                color.a = key=="film" ? .54f : .10f;
                material.SetFloat("_BlendModePreserveSpecular", 0);
                material.SetFloat("_Cull", 0);
                material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
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




