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
    public static class SparkLevel53ModelInstaller
    {
        public const string Folder = "Assets/My_Assets/_Project_Spark/Prefabs/Levels/Level_53_Layout/";
        public const string Prefab = Folder + "Level_53_Layout.prefab";
        public const string ScenePath = "Assets/My_Assets/_Project_Spark/Scenes/MainMenu/MainMenu.unity";
        private const string Model = Folder + "Models/Level53_Fitted_Assembly.fbx";
        private const string Materials = Folder + "Models/Materials";
        private const float SocketHeight = .025f;
        private const float SocketDiameter = .045f;

        private static readonly (string id, string label, float x, float z, string color)[] Sockets =
        {
            ("SUPPLY_PLUS","+5 V",-0.952f,-0.568f,"red"),
            ("CTRL_PLUS","C +",-0.161f,-0.568f,"red"),
            ("SUPPLY_RETURN","RET",-0.839f,-0.568f,"blue"),
            ("CTRL_GND","C RET",-0.048f,-0.568f,"blue"),
            ("MDRV_PLUS","MD +",-0.726f,-0.568f,"red"),
            ("MDRV_GND","MD RET",-0.613f,-0.568f,"blue"),
            ("VIB_LEFT_PLUS","L +",-0.902f,0.415f,"red"),
            ("VIB_LEFT_GND","L RET",-0.709f,0.415f,"blue"),
            ("VIB_RIGHT_PLUS","R +",0.256f,0.415f,"red"),
            ("VIB_RIGHT_GND","R RET",0.449f,0.415f,"blue"),
            ("BDRV_PLUS","BD +",-0.017f,-0.225f,"red"),
            ("BDRV_GND","BD RET",0.136f,-0.225f,"blue"),
            ("CTRL_PWM","PWM",0.065f,-0.568f,"cyan"),
            ("MDRV_PWM","PWM IN",-0.5f,-0.568f,"cyan"),
            ("MDRV_MP","M OUT +",-0.387f,-0.568f,"cyan"),
            ("MOTOR_POS","M +",-0.935f,-0.225f,"amber"),
            ("MDRV_MN","M OUT −",-0.274f,-0.568f,"cyan"),
            ("MOTOR_NEG","M −",-0.782f,-0.225f,"amber"),
            ("VIB_LEFT_OUT","L OUT",-0.516f,0.415f,"cyan"),
            ("CTRL_VIB_L","VIB L",0.178f,-0.568f,"cyan"),
            ("VIB_RIGHT_OUT","R OUT",0.642f,0.415f,"cyan"),
            ("CTRL_VIB_R","VIB R",0.291f,-0.568f,"cyan"),
            ("ENCODER_A","ENC A",-0.323f,0.415f,"cyan"),
            ("CTRL_ENC_A","A IN",0.404f,-0.568f,"cyan"),
            ("ENCODER_B","ENC B",-0.13f,0.415f,"cyan"),
            ("CTRL_ENC_B","B IN",0.517f,-0.568f,"cyan"),
            ("ENCODER_Z","ENC Z",0.063f,0.415f,"cyan"),
            ("CTRL_ENC_Z","Z IN",0.63f,-0.568f,"cyan"),
            ("CTRL_BAL","BAL",0.743f,-0.568f,"cyan"),
            ("BDRV_CMD","CMD",0.289f,-0.225f,"amber"),
            ("BDRV_AP","A OUT +",0.442f,-0.225f,"amber"),
            ("PLANE_A_PLUS","PA +",-0.629f,-0.225f,"amber"),
            ("BDRV_AN","A OUT −",0.595f,-0.225f,"amber"),
            ("PLANE_A_NEG","PA −",-0.476f,-0.225f,"amber"),
            ("BDRV_BP","B OUT +",0.748f,-0.225f,"amber"),
            ("PLANE_B_PLUS","PB +",-0.323f,-0.225f,"amber"),
            ("BDRV_BN","B OUT −",0.901f,-0.225f,"amber"),
            ("PLANE_B_NEG","PB −",-0.17f,-0.225f,"amber"),
            ("GUARD_NC","GUARD",0.835f,0.415f,"cyan"),
            ("CTRL_SAFE","SAFE",0.856f,-0.568f,"cyan"),
        };

        public static IEnumerable<string> TerminalIds => Sockets.Select(socket => socket.id);

        private static readonly Dictionary<string, string> Palette = new Dictionary<string, string>
        {
            {"aluminum","B7C8D7"},
            {"amber","FFA82B"},
            {"amberlit","FFCD6B"},
            {"aqua","2DDDF9"},
            {"blue","1671E2"},
            {"brass","D4A94A"},
            {"carbon","101F2E"},
            {"chrome","D2E1ED"},
            {"coal","081420"},
            {"cyan","23DAFD"},
            {"darksteel","24364C"},
            {"deep_steel","1B2A3C"},
            {"electric","2B84F7"},
            {"glassblue","74CFF4"},
            {"graphite","121D2B"},
            {"gunmetal","435C73"},
            {"mid_steel","384C65"},
            {"mint","34F6C5"},
            {"mirror","E3EFF7"},
            {"nickel","91A7BD"},
            {"orange","F97126"},
            {"pale","EAF4FB"},
            {"rubber","181E25"},
            {"safety","FFAE31"},
            {"scarlet","F9445A"},
            {"screenback","071D2C"},
            {"titanium","9FB3C8"},
        };

        [MenuItem("Tools/Project Spark/Levels/Create Level 53")]
        public static void Install()
        {
            SparkLevel53Builder.Create();
            Fit();
            Register();
            ValidateAssets();
        }

        [MenuItem("Tools/Project Spark/Level 53/Validate Level Assets")]
        public static void ValidateAssets()
        {
            var root=AssetDatabase.LoadAssetAtPath<GameObject>(Prefab);
            var control=root != null ? root.GetComponent<SparkLevel53Couple>() : null;
            var bindings=root != null ? root.GetComponent<SparkLevelSceneBindings>() : null;
            if (control==null || bindings==null || control.controllerDisplay==null || control.qualityDisplay==null || control.rig==null ||
                control.pins.Length!=40 || control.pins.Distinct().Count()!=40 || 
                !TerminalIds.SequenceEqual(control.pins.Select(pin=>pin.name)) ||
                control.pins.Any(pin=>!bindings.TryGetTerminal(pin.name,out var bound) || bound!=pin) ||
                SparkLevel53Couple.Gates.Any(id=>!bindings.IsComponentIdDeclared(id)))
                throw new InvalidOperationException("Level 53 wiring, motion or binding references are incomplete.");
            Debug.Log("[LEVEL 53] Validated 40 distinct terminals, 24 player wires and 6 evaluator gates.",root);
        }

        private static void Register()
        {
            var scene = SceneManager.GetSceneByPath(ScenePath);
            bool opened = !scene.isLoaded;
            if (opened) scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
            bool dirty = scene.isDirty;
            var objects = scene.GetRootGameObjects();
            T Find<T>() where T : Component => objects.SelectMany(item => item.GetComponentsInChildren<T>(true)).Single();
            var level = AssetDatabase.LoadAssetAtPath<SparkLevelDefinition>(Folder + "Level_53.asset");
            var drawing = AssetDatabase.LoadAssetAtPath<SparkLevelDrawingAsset>(Folder + "Level_53_Drawing.asset");
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
            else if (dirty) Debug.Log("MainMenu had unsaved edits. Save the scene to persist Level 53's campaign and teaching entries.");
        }

        private static void Append(Object target, string field, Object value)
        {
            if (value == null) throw new InvalidOperationException("Missing Level 53 asset for " + field);
            var data = new SerializedObject(target);
            var list = data.FindProperty(field);
            for (int i = 0; i < list.arraySize; i++)
                if (list.GetArrayElementAtIndex(i).objectReferenceValue == value) return;
            list.arraySize++;
            list.GetArrayElementAtIndex(list.arraySize - 1).objectReferenceValue = value;
            data.ApplyModifiedProperties();
        }

        [MenuItem("Tools/Project Spark/Level 53/Fit Breadboard Assembly")]
        public static void Fit()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Exit Play Mode before fitting Level 53.");
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(Model);
            if (model == null || AssetDatabase.LoadAssetAtPath<GameObject>(Prefab) == null)
                throw new InvalidOperationException("The fitted FBX or existing Level 53 prefab is missing.");

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
                    throw new InvalidOperationException("Unity could not save the Level 53 layout.");
                AssetDatabase.SaveAssets();
                Debug.Log("Level 53 fitted to the MainMenu breadboard with 40 seated terminals.");
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
            Vector3 right = Vector3.ProjectOnPlane(grid.GridOrigin.right, up).normalized * .053f;
            Vector3 forward = Vector3.ProjectOnPlane(grid.GridOrigin.forward, up).normalized * .053f;
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
            var bench = root.transform.Find("Couple Workbench");
            var bindings = root.GetComponent<SparkLevelSceneBindings>();
            var loop = root.GetComponent<SparkLevel53Couple>();
            if (bench == null || bindings == null || loop == null)
                throw new InvalidOperationException("Level 53 is missing its workbench, controls, or scene bindings.");

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

            foreach (string name in new[] {"Fitted Level 53 Models", "Breadboard Labels", "Motion pivots"})
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
            var assembly = Object.Instantiate(model, bench, false).transform;
            assembly.name = "Fitted Level 53 Models";
            var parts = assembly.GetComponentsInChildren<Transform>(true).ToDictionary(item => item.name);
            Transform Part(string name) => parts["L53_" + name];
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

            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (loop.calibrationText == null)
            {
                var oldControls = root.transform.Find("Level 53 Controls");
                if (oldControls != null) Object.DestroyImmediate(oldControls.gameObject);
                SparkLevel53Builder.BuildControls(root.transform,loop,font);
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
                if (spec.id is "DRIVER_MOTOR_PLUS" or "DRIVER_MOTOR_MINUS")
                {
                    // The Wire tool checks the terminal owner's interaction flag.
                    // These contacts have no click collider and are driven by the controller.
                    var ownerData = new SerializedObject(terminal.Owner);
                    ownerData.FindProperty("interactionsEnabled").boolValue = true;
                    ownerData.ApplyModifiedPropertiesWithoutUndo();
                }
                owners.Add(terminal.transform.parent);
                terminal.GetComponent<Renderer>().enabled = true;
                var projected = ToWorld(new Vector3(spec.x, floor, spec.z));
                projected -= up * Vector3.Dot(projected - grid.GridOrigin.position, up);
                if (!grid.TryGetNearestHole(projected, .32f, out var hole, out int column, out int row) ||
                    !holes.Add(new Vector2Int(column, row)))
                    throw new InvalidOperationException("No separate breadboard hole for " + spec.id);
                Vector3 seat = ToLocal(SurfacePoint(grid, hole, up));
                Seat(terminal, bench, seat, pinLayer, MaterialFor(shader, spec.color), MaterialFor(shader, "void"));

                float offset=spec.id is "DRIVER_PWM_IN" or "CTRL_VIB_IN" or "CTRL_ENC_A" or "DRIVER_PLUS"
                    ? -.079f : Array.IndexOf(Sockets,spec)%2==0 ? -.046f : -.059f;
                Label(labels,font,spec.id,spec.label,seat+new Vector3(0,.014f,offset),.016f);
            }
            foreach (var owner in owners)
                foreach (Transform child in owner)
                    if (child.GetComponent<SparkTerminal>() == null && child.GetComponent<Renderer>() != null)
                        child.gameObject.SetActive(false);

            var titles = new (string title, float x, float z)[]
            {






                ("PLANE A",-.31f,.285f),("PLANE B",.26f,.285f),("DYNAMIC BALANCE",.04f,-.301f),
                ("DUAL VIBRATION",-.08f,.664f)
            };
            foreach(var title in titles) Label(labels,font,title.title,title.title,new Vector3(title.x,floor+.025f,title.z),.020f);
            var motion=Child(bench,"Motion pivots",Vector3.zero);
            Transform Pivot(string name)
            {
                var part=Part(name);
                var pivot=Child(motion,name,Vector3.zero);
                pivot.SetPositionAndRotation(part.position,bench.rotation);
                part.SetParent(pivot,true);
                return pivot;
            }
            var rig=new SerializedObject(loop.rig);
            var weightA=Pivot("PlaneA_Weight");
            var weightB=Pivot("PlaneB_Weight");
            loop.massAArt=weightA.GetChild(0);
            loop.massBArt=weightB.GetChild(0);
            foreach(var (field,pivot) in new[] {
                ("motorShaftRotateX",Pivot("Motor_Shaft")),("longRotorShaftRotateX",Pivot("Shaft_Train")),
                ("planeARotorRotateX",Pivot("PlaneA_Disc")),("planeBRotorRotateX",Pivot("PlaneB_Disc")),
                ("planeACorrectionWeightOrbitX",weightA),("planeBCorrectionWeightOrbitX",weightB),
                ("encoderABZDiscRotateX",Pivot("Encoder_Disc")),("safetyFrontGuardDoorHingeY",Pivot("Guard_Door")) })
                rig.FindProperty(field).objectReferenceValue=pivot;
            rig.FindProperty("guardOpenAngle").floatValue=65f;
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
            loop.controllerDisplay=Readout("Controller screen",new Vector3(.04f,floor+.132f,-.428f),new Color(.46f,.94f,1f));
            loop.qualityDisplay=Readout("Bearing vectors",new Vector3(-.08f,floor+.133f,.537f),new Color(.68f,1f,.86f));
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
            string path = $"{Materials}/L53 {key}.mat";
            var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing != null && (key!="film" || existing.GetFloat("_Surface")>.5f)) return existing;
            ColorUtility.TryParseHtmlString("#" + (Palette.TryGetValue(key, out var tint) ? tint : "9FB3C8"), out var color);
            var material = existing != null ? existing : new Material(shader) { name = "L53 " + key };
            bool metal = key is "chrome" or "titanium" or "steel" or "gold" or "copper" or "aluminum";
            material.SetFloat("_Metallic", metal ? .53f : .02f);
            material.SetFloat("_Smoothness", metal ? .64f : .35f);
            if (key is "green" or "cyan" or "red" or "blue" or "orange" or "amber" or "mint" or "warm" or "ice")
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", color * .53f);
            }
            if (key is "glass" or "glass_body" or "glass_soft" or "acrylic" or "glassblue" or "film")
            {
                color.a = key=="film" ? .53f : .22f;
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









