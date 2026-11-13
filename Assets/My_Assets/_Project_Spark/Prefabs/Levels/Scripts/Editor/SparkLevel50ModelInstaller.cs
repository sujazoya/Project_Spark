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
    public static class SparkLevel50ModelInstaller
    {
        public const string Folder = "Assets/My_Assets/_Project_Spark/Prefabs/Levels/Level_50_Layout/";
        public const string Prefab = Folder + "Level_50_Layout.prefab";
        public const string ScenePath = "Assets/My_Assets/_Project_Spark/Scenes/MainMenu/MainMenu.unity";
        private const string Model = Folder + "Models/Level50_Fitted_Assembly.fbx";
        private const string Materials = Folder + "Models/Materials";
        private const float SocketHeight = .025f;
        private const float SocketDiameter = .045f;

        private static readonly (string id, string label, float x, float z, string color)[] Sockets =
        {
            ("SUPPLY_PLUS","+5 V",-0.91f,-0.575f,"red"),
            ("CTRL_PLUS","C +",-0.53f,-0.575f,"red"),
            ("SUPPLY_RETURN","RET",-0.74f,-0.575f,"blue"),
            ("CTRL_GND","C RET",-0.428f,-0.575f,"blue"),
            ("MARK_PLUS","MARK +",-0.91f,0.32f,"red"),
            ("MARK_GND","MARK RET",-0.765f,0.32f,"blue"),
            ("VISION_PLUS","VIS +",-0.185f,0.32f,"red"),
            ("VISION_GND","VIS RET",-0.04f,0.32f,"blue"),
            ("DRIVER_PLUS","DRV +",-0.236f,-0.245f,"red"),
            ("DRIVER_GND","DRV RET",-0.038f,-0.245f,"blue"),
            ("MARK_OUT","MARK OUT",-0.62f,0.32f,"cyan"),
            ("CTRL_MARK_IN","MARK IN",-0.326f,-0.575f,"cyan"),
            ("ENC_A","ENC A",-0.475f,0.32f,"cyan"),
            ("CTRL_ENC_A","A IN",-0.224f,-0.575f,"cyan"),
            ("ENC_B","ENC B",-0.33f,0.32f,"cyan"),
            ("CTRL_ENC_B","B IN",-0.122f,-0.575f,"cyan"),
            ("CTRL_PRINT_TRIG","PRINT",-0.02f,-0.575f,"cyan"),
            ("PRINT_HEAD_TRIG","P TRIG",-0.83f,-0.245f,"amber"),
            ("CTRL_CUT_TRIG","CUT",0.082f,-0.575f,"cyan"),
            ("CUT_HEAD_TRIG","C TRIG",-0.632f,-0.245f,"amber"),
            ("VISION_PASS","PASS",0.105f,0.32f,"cyan"),
            ("CTRL_PASS_IN","PASS IN",0.184f,-0.575f,"cyan"),
            ("VISION_FAIL","FAIL",0.25f,0.32f,"cyan"),
            ("CTRL_FAIL_IN","FAIL IN",0.286f,-0.575f,"cyan"),
            ("CTRL_SORT_CMD","SORT",0.388f,-0.575f,"cyan"),
            ("DRIVER_SORT_CMD","CMD",0.16f,-0.245f,"amber"),
            ("DRIVER_MOTOR_PLUS","OUT +",0.358f,-0.245f,"amber"),
            ("DIVERTER_MOTOR_PLUS","M +",0.7f,0.55f,"mint"),
            ("DRIVER_MOTOR_MINUS","OUT −",0.556f,-0.245f,"amber"),
            ("DIVERTER_MOTOR_MINUS","M −",0.86f,0.55f,"mint"),
            ("DIVERTER_HOME_OUT","HOME",0.22f,0.55f,"mint"),
            ("CTRL_DIVERTER_HOME","HOME IN",0.49f,-0.575f,"cyan"),
            ("GOOD_COUNT_OUT","GOOD",0.38f,0.55f,"mint"),
            ("CTRL_GOOD_COUNT","GOOD IN",0.592f,-0.575f,"cyan"),
            ("REJECT_COUNT_OUT","REJECT",0.54f,0.55f,"mint"),
            ("CTRL_REJECT_COUNT","REJ IN",0.694f,-0.575f,"cyan"),
            ("SAFETY_NC_OUT","SAFE OUT",-0.434f,-0.245f,"amber"),
            ("CTRL_SAFETY_IN","SAFE IN",0.796f,-0.575f,"cyan"),
        };

        public static IEnumerable<string> TerminalIds => Sockets.Select(socket => socket.id);

        private static readonly Dictionary<string, string> Palette = new Dictionary<string, string>
        {
            {"amber","FFA82B"},
            {"aqua","2DDEF4"},
            {"badbin","852C35"},
            {"beam","3CEBF5"},
            {"blue","1671E2"},
            {"bronze","B57136"},
            {"carbon","132231"},
            {"chrome","D2E1ED"},
            {"coal","0A141F"},
            {"cyan","23DAFD"},
            {"darksteel","223445"},
            {"deep_steel","1B2A3C"},
            {"glassblue","69C6E6"},
            {"gold","EDB846"},
            {"goodbin","146E51"},
            {"graphite","121D2B"},
            {"ice","DCF4FA"},
            {"ink","196175"},
            {"mid_steel","384C65"},
            {"mint","34F6C5"},
            {"neonblue","2C89F8"},
            {"nickel","93ADBE"},
            {"orange","F97126"},
            {"polished","D5E2E9"},
            {"red","FF4158"},
            {"rubber","181E25"},
            {"rubberblue","1C2E38"},
            {"safety","FDB236"},
            {"scarlet","F74153"},
            {"screenback","0B2835"},
            {"steel","7089A2"},
            {"tealshade","1A7683"},
            {"titanium","9FB3C8"},
            {"void","070D18"},
            {"webwhite","E6EFEA"},
            {"white","E7F5FB"},
        };

        [MenuItem("Tools/Project Spark/Levels/Create Level 50")]
        public static void Install()
        {
            SparkLevel50Builder.Create();
            Fit();
            Register();
            ValidateAssets();
        }

        [MenuItem("Tools/Project Spark/Level 50/Validate Level Assets")]
        public static void ValidateAssets()
        {
            var root=AssetDatabase.LoadAssetAtPath<GameObject>(Prefab);
            var control=root != null ? root.GetComponent<SparkLevel50Factory>() : null;
            var bindings=root != null ? root.GetComponent<SparkLevelSceneBindings>() : null;
            if (control==null || bindings==null || control.controllerDisplay==null || control.qualityDisplay==null || control.rig==null ||
                control.pins.Length!=38 || control.pins.Distinct().Count()!=38 || control.productTokens.Length!=5 ||
                control.webMarks.Length<1 || !TerminalIds.SequenceEqual(control.pins.Select(pin=>pin.name)) ||
                control.pins.Any(pin=>!bindings.TryGetTerminal(pin.name,out var bound) || bound!=pin) ||
                SparkLevel50Factory.Gates.Any(id=>!bindings.IsComponentIdDeclared(id)))
                throw new InvalidOperationException("Level 50 wiring, motion or binding references are incomplete.");
            Debug.Log("[LEVEL 50] Validated 38 distinct terminals, 22 player wires and 6 evaluator gates.",root);
        }

        private static void Register()
        {
            var scene = SceneManager.GetSceneByPath(ScenePath);
            bool opened = !scene.isLoaded;
            if (opened) scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
            bool dirty = scene.isDirty;
            var objects = scene.GetRootGameObjects();
            T Find<T>() where T : Component => objects.SelectMany(item => item.GetComponentsInChildren<T>(true)).Single();
            var level = AssetDatabase.LoadAssetAtPath<SparkLevelDefinition>(Folder + "Level_50.asset");
            var drawing = AssetDatabase.LoadAssetAtPath<SparkLevelDrawingAsset>(Folder + "Level_50_Drawing.asset");
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
            else if (dirty) Debug.Log("MainMenu had unsaved edits. Save the scene to persist Level 50's campaign and teaching entries.");
        }

        private static void Append(Object target, string field, Object value)
        {
            if (value == null) throw new InvalidOperationException("Missing Level 50 asset for " + field);
            var data = new SerializedObject(target);
            var list = data.FindProperty(field);
            for (int i = 0; i < list.arraySize; i++)
                if (list.GetArrayElementAtIndex(i).objectReferenceValue == value) return;
            list.arraySize++;
            list.GetArrayElementAtIndex(list.arraySize - 1).objectReferenceValue = value;
            data.ApplyModifiedProperties();
        }

        [MenuItem("Tools/Project Spark/Level 50/Fit Breadboard Assembly")]
        public static void Fit()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Exit Play Mode before fitting Level 50.");
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(Model);
            if (model == null || AssetDatabase.LoadAssetAtPath<GameObject>(Prefab) == null)
                throw new InvalidOperationException("The fitted FBX or existing Level 50 prefab is missing.");

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
                    throw new InvalidOperationException("Unity could not save the Level 50 layout.");
                AssetDatabase.SaveAssets();
                Debug.Log("Level 50 fitted to the MainMenu breadboard with 38 seated terminals.");
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
            var bench = root.transform.Find("Factory Workbench");
            var bindings = root.GetComponent<SparkLevelSceneBindings>();
            var loop = root.GetComponent<SparkLevel50Factory>();
            if (bench == null || bindings == null || loop == null)
                throw new InvalidOperationException("Level 50 is missing its workbench, controls, or scene bindings.");

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

            foreach (string name in new[] {"Fitted Level 50 Models", "Breadboard Labels", "Motion pivots"})
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
            assembly.name = "Fitted Level 50 Models";
            var parts = assembly.GetComponentsInChildren<Transform>(true).ToDictionary(item => item.name);
            Transform Part(string name) => parts["L50_" + name];
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

                float offset=Array.IndexOf(Sockets,spec)%2==0 ? -.047f : -.067f;
                Label(labels,font,spec.id,spec.label,seat+new Vector3(0,.014f,offset),.016f);
            }
            foreach (var owner in owners)
                foreach (Transform child in owner)
                    if (child.GetComponent<SparkTerminal>() == null && child.GetComponent<Renderer>() != null)
                        child.gameObject.SetActive(false);

            var titles = new (string title, float x, float z)[]
            {



                ("MARK",-.65f,.115f),("PRINT",-.34f,.18f),("CUT",0,.18f),("INSPECT",.34f,.18f),
                ("SORT",.69f,.185f),("FACTORY CONTROLLER",-.22f,-.304f),("QUALITY DASHBOARD",-.24f,.577f)
            };
            foreach(var title in titles) Label(labels,font,title.title,title.title,new Vector3(title.x,floor+.025f,title.z),.023f);
            var motion=Child(bench,"Motion pivots",Vector3.zero);
            Transform Pivot(string name)
            {
                var part=Part(name);
                var pivot=Child(motion,name,Vector3.zero);
                pivot.SetPositionAndRotation(part.position,bench.rotation);
                part.SetParent(pivot,true);
                return pivot;
            }
            var carriage=Pivot("Cut_Carriage");
            var blade=Pivot("Cut_Blade");
            blade.SetParent(carriage,true);
            var rig=new SerializedObject(loop.rig);
            foreach(var (field,pivot) in new[] {
                ("measuringRollerRotateZ",Pivot("Encoder_Roll")),("encoderDiscRotateZ",Pivot("Encoder_Disc")),
                ("printHeadStrokeY",Pivot("Print_Stroke")),("cutterCarriageSlideX",carriage),("cutterBladeStrokeY",blade),
                ("cutterLeadScrewRotateX",Pivot("Cut_Screw")),("sortingArmPivotY",Pivot("Sort_Gate")),
                ("sortMotorShaftRotateX",Pivot("Motor_Shaft")) })
                rig.FindProperty(field).objectReferenceValue=pivot;
            rig.FindProperty("carriageArtUnitsPerMeter").floatValue=2.7f;
            rig.FindProperty("printArtStroke").floatValue=.043f;
            rig.FindProperty("cutArtStroke").floatValue=.037f;
            rig.ApplyModifiedPropertiesWithoutUndo();
            loop.scanner=Part("Mark_Beam").gameObject;
            loop.visionLight=Part("Vision_Light").gameObject;
            loop.emergencyButton=Pivot("Safety_Button");
            loop.webMarks=parts.Keys.Where(key=>key.StartsWith("L50_Web_Mark") && !key.Contains("__"))
                .OrderBy(key=>key).Select(key=>Pivot(key.Substring(4))).ToArray();
            loop.productTokens=new Transform[5];
            for(int i=0;i<loop.productTokens.Length;i++)
            {
                var token=GameObject.CreatePrimitive(PrimitiveType.Cube);
                Object.DestroyImmediate(token.GetComponent<Collider>());
                token.name="Tracked product "+i;
                token.transform.SetParent(motion,false);
                token.transform.localPosition=new Vector3(0,floor+.122f,.045f);
                token.transform.localScale=new Vector3(.078f,.011f,.075f);
                token.GetComponent<Renderer>().sharedMaterial=MaterialFor(shader,"webwhite");
                loop.productTokens[i]=token.transform;
                token.SetActive(false);
            }
            TMP_Text Readout(string name,Vector3 position,Color color)
            {
                var child=Child(labels,name,position);
                child.localRotation=Quaternion.Euler(90,0,0);
                child.localScale=Vector3.one*.20f;
                var display=child.gameObject.AddComponent<TextMeshPro>();
                display.fontSize=.88f;
                display.alignment=TextAlignmentOptions.Center;
                display.textWrappingMode=TextWrappingModes.NoWrap;
                display.color=color;
                return display;
            }
            loop.controllerDisplay=Readout("Controller screen",new Vector3(-.22f,floor+.136f,-.41f),new Color(.46f,.94f,1f));
            loop.qualityDisplay=Readout("Inspection screen",new Vector3(-.25f,floor+.131f,.47f),new Color(.68f,1f,.86f));
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
            string path = $"{Materials}/L50 {key}.mat";
            var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing != null && (key!="film" || existing.GetFloat("_Surface")>.5f)) return existing;
            ColorUtility.TryParseHtmlString("#" + (Palette.TryGetValue(key, out var tint) ? tint : "9FB3C8"), out var color);
            var material = existing != null ? existing : new Material(shader) { name = "L50 " + key };
            bool metal = key is "chrome" or "titanium" or "steel" or "gold" or "copper" or "aluminum";
            material.SetFloat("_Metallic", metal ? .50f : .02f);
            material.SetFloat("_Smoothness", metal ? .64f : .35f);
            if (key is "green" or "cyan" or "red" or "blue" or "orange" or "amber" or "mint" or "warm" or "ice")
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", color * .50f);
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









