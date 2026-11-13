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
    public static class SparkLevel46ModelInstaller
    {
        public const string Folder = "Assets/My_Assets/_Project_Spark/Prefabs/Levels/Level_46_Layout/";
        public const string Prefab = Folder + "Level_46_Sync_Layout.prefab";
        public const string ScenePath = "Assets/My_Assets/_Project_Spark/Scenes/MainMenu/MainMenu.unity";
        private const string Model = Folder + "Models/Level46_Fitted_Assembly.fbx";
        private const string Materials = Folder + "Models/Materials";
        private const float SocketHeight = .025f;
        private const float SocketDiameter = .064f;

        private static readonly (string id, string label, float x, float z, string color)[] Sockets =
        {
            ("SUPPLY_PLUS_5V","+5 V",.725f,.390f,"red"), ("CTRL_PLUS_5V","C +",-.765f,.379f,"red"),
            ("SUPPLY_RETURN","RET",.914f,.390f,"blue"), ("CTRL_GND","C RET",.024f,.379f,"blue"),
            ("CTRL_MASTER_PWM","M PWM",-.51f,.379f,"cyan"), ("DRIVER_MASTER_PWM","M PWM",-.27f,-.372f,"cyan"),
            ("CTRL_FOLLOWER_PWM","F PWM",-.256f,.379f,"amber"), ("DRIVER_FOLLOWER_PWM","F PWM",-.27f,-.648f,"amber"),
            ("DRIVER_MASTER_PLUS","M +",0,-.372f,"cyan"), ("M_MOTOR_PLUS","M +",-.64f,-.365f,"cyan"),
            ("DRIVER_MASTER_MINUS","M −",.27f,-.372f,"cyan"), ("M_MOTOR_MINUS","M −",-.475f,-.365f,"cyan"),
            ("DRIVER_FOLLOWER_PLUS","F +",0,-.648f,"amber"), ("F_MOTOR_PLUS","F +",.477f,-.365f,"amber"),
            ("DRIVER_FOLLOWER_MINUS","F −",.27f,-.648f,"amber"), ("F_MOTOR_MINUS","F −",.65f,-.365f,"amber"),
            ("MASTER_ENC_A","M A",-.295f,.283f,"cyan"), ("CTRL_MASTER_A","M A IN",-.765f,.265f,"cyan"),
            ("MASTER_ENC_B","M B",-.094f,.283f,"cyan"), ("CTRL_MASTER_B","M B IN",-.51f,.265f,"cyan"),
            ("FOLLOWER_ENC_A","F A",.112f,.283f,"amber"), ("CTRL_FOLLOWER_A","F A IN",.34f,.379f,"amber"),
            ("FOLLOWER_ENC_B","F B",.314f,.230f,"amber"), ("CTRL_FOLLOWER_B","F B IN",.55f,.379f,"amber")
        };

        public static IEnumerable<string> TerminalIds => Sockets.Select(socket => socket.id);

        private static readonly Dictionary<string, string> Palette = new Dictionary<string, string>
        {
            {"amber","FFA82B"},
            {"blue","1671E2"},
            {"bronze","A36633"},
            {"ceramic","E4E3D3"},
            {"chrome","D2E1ED"},
            {"coolgrey","5F778B"},
            {"cyan","23DAFD"},
            {"deep_steel","1B2A3C"},
            {"fabric","33565F"},
            {"fabriclight","4B7B80"},
            {"glass","57BCDA"},
            {"gold","EDB846"},
            {"graphite","121D2B"},
            {"green","2CFC8F"},
            {"ice","D3EFF9"},
            {"insulation","152B38"},
            {"mid_steel","384C65"},
            {"mint","2BF6C2"},
            {"motor_blue","1367C5"},
            {"nickel","A1B6C4"},
            {"orange","F97126"},
            {"polished","C5D6E2"},
            {"red","FF4158"},
            {"rubber","181E25"},
            {"screen","091F2C"},
            {"silver","B8C6D2"},
            {"steel","7089A2"},
            {"titanium","9FB3C8"},
            {"void","070D18"},
            {"warm","FCAB4D"},
            {"white","E7F5FB"},
        };

        [MenuItem("Tools/Project Spark/Level 46/Install and Register")]
        public static void Install()
        {
            SparkLevel46Builder.Create();
            Fit();
            Register();
            ValidateAssets();
        }

        [MenuItem("Tools/Project Spark/Level 46/Validate Level Assets")]
        public static void ValidateAssets()
        {
            var root=AssetDatabase.LoadAssetAtPath<GameObject>(Prefab);
            var control=root != null ? root.GetComponent<SparkLevel46Sync>() : null;
            var bindings=root != null ? root.GetComponent<SparkLevelSceneBindings>() : null;
            if (control==null || bindings==null || control.rig==null || control.masterDisplay==null ||
                control.pins.Length!=24 || control.pins.Distinct().Count()!=24 || control.lamps.Length!=9 ||
                control.webSlats.Length<12 || !TerminalIds.SequenceEqual(control.pins.Select(pin=>pin.name)) ||
                control.pins.Any(pin=>!bindings.TryGetTerminal(pin.name,out var bound) || bound!=pin) ||
                SparkLevel46Sync.Gates.Any(id=>!bindings.IsComponentIdDeclared(id)))
                throw new InvalidOperationException("Level 46 wiring, motion or binding references are incomplete.");
            Debug.Log("[LEVEL 46] Validated 24 distinct terminals, 12 player wires and 6 evaluator gates.",root);
        }

        private static void Register()
        {
            var scene = SceneManager.GetSceneByPath(ScenePath);
            bool opened = !scene.isLoaded;
            if (opened) scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
            bool dirty = scene.isDirty;
            var objects = scene.GetRootGameObjects();
            T Find<T>() where T : Component => objects.SelectMany(item => item.GetComponentsInChildren<T>(true)).Single();
            var level = AssetDatabase.LoadAssetAtPath<SparkLevelDefinition>(Folder + "Level_46.asset");
            var drawing = AssetDatabase.LoadAssetAtPath<SparkLevelDrawingAsset>(Folder + "Level_46_Drawing.asset");
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
            else if (dirty) Debug.Log("MainMenu had unsaved edits. Save the scene to persist Level 46's campaign and teaching entries.");
        }

        private static void Append(Object target, string field, Object value)
        {
            if (value == null) throw new InvalidOperationException("Missing Level 46 asset for " + field);
            var data = new SerializedObject(target);
            var list = data.FindProperty(field);
            for (int i = 0; i < list.arraySize; i++)
                if (list.GetArrayElementAtIndex(i).objectReferenceValue == value) return;
            list.arraySize++;
            list.GetArrayElementAtIndex(list.arraySize - 1).objectReferenceValue = value;
            data.ApplyModifiedProperties();
        }

        [MenuItem("Tools/Project Spark/Level 46/Fit Breadboard Assembly")]
        public static void Fit()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Exit Play Mode before fitting Level 46.");
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(Model);
            if (model == null || AssetDatabase.LoadAssetAtPath<GameObject>(Prefab) == null)
                throw new InvalidOperationException("The fitted FBX or existing Level 46 prefab is missing.");

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
                    throw new InvalidOperationException("Unity could not save the Level 46 layout.");
                AssetDatabase.SaveAssets();
                Debug.Log("Level 46 fitted to the MainMenu breadboard with 24 seated terminals.");
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
            var bench = root.transform.Find("Sync Workbench");
            var bindings = root.GetComponent<SparkLevelSceneBindings>();
            var loop = root.GetComponent<SparkLevel46Sync>();
            if (bench == null || bindings == null || loop == null)
                throw new InvalidOperationException("Level 46 is missing its workbench, controls, or scene bindings.");

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

            foreach (string name in new[] {"Fitted Level 46 Models", "Breadboard Labels", "Motion pivots"})
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
            assembly.name = "Fitted Level 46 Models";
            var parts = assembly.GetComponentsInChildren<Transform>(true).ToDictionary(item => item.name);
            Transform Part(string name) => parts["L46_" + name];
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
                ("SYNC GOVERNOR",-.40f,.659f),("WEB TENSION",.28f,.666f),("5 V SUPPLY",.79f,.665f),
                ("MASTER",-.60f,-.083f),("FOLLOWER",.58f,-.083f),("BRAKE",.82f,.197f),("DUAL DRIVER",0,-.515f)
            };
            foreach (var title in titles)
                Label(labels,font,title.title,title.title,new Vector3(title.x,floor+.026f,title.z),.026f);

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
                ("masterMotorShaft","MasterMotor_Shaft"),("masterRoller","MasterRoller_Roll"),("masterEncoderDisc","Encoder_Master"),
                ("followerMotorShaft","FollowerMotor_Shaft"),("followerRoller","FollowerRoller_Roll"),("followerEncoderDisc","Encoder_Follower"),
                ("followerBrakeRotor","Brake_Rotor"),("brakeArm","Brake_Arm"),("brakePad","Brake_Pad") })
                rig.FindProperty(field).objectReferenceValue=Pivot(part,field);
            rig.FindProperty("brakePadTravel").floatValue=.017f;
            rig.ApplyModifiedPropertiesWithoutUndo();

            loop.webSlats=parts.Keys.Where(key=>key.StartsWith("L46_Web_Slat") && !key.Contains("__"))
                .OrderBy(key=>key).Select(key=>Pivot(key.Substring(4),key)).ToArray();
            loop.lamps=new[]{"Supply_Power","Driver_Master","Driver_Follower","Controller_Master","Controller_Follower","Brake_Load",
                "Monitor_Sync","Monitor_Slack","Monitor_Stop"}.Select(name=>Part(name).gameObject).ToArray();
            TMP_Text Readout(string name,Vector3 position,Color color)
            {
                var child=Child(labels,name,position);
                child.localRotation=Quaternion.Euler(90,0,0);
                child.localScale=Vector3.one*.20f;
                var display=child.gameObject.AddComponent<TextMeshPro>();
                display.fontSize=1.18f;
                display.alignment=TextAlignmentOptions.Center;
                display.textWrappingMode=TextWrappingModes.NoWrap;
                display.color=color;
                display.text="—";
                return display;
            }
            loop.masterDisplay=Readout("Master screen",new Vector3(-.60f,floor+.146f,.543f),new Color(.46f,.94f,1f));
            loop.followerDisplay=Readout("Follower screen",new Vector3(-.20f,floor+.146f,.543f),new Color(1f,.72f,.38f));
            loop.tensionDisplay=Readout("Tension screen",new Vector3(.28f,floor+.130f,.556f),new Color(.68f,1f,.86f));
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
            string path = $"{Materials}/L46 {key}.mat";
            var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing != null) return existing;
            ColorUtility.TryParseHtmlString("#" + (Palette.TryGetValue(key, out var tint) ? tint : "9FB3C8"), out var color);
            var material = new Material(shader) { name = "L46 " + key };
            bool metal = key is "chrome" or "titanium" or "steel" or "gold" or "copper" or "aluminum";
            material.SetFloat("_Metallic", metal ? .46f : .02f);
            material.SetFloat("_Smoothness", metal ? .64f : .35f);
            if (key is "green" or "cyan" or "red" or "blue" or "orange" or "amber" or "mint" or "warm" or "ice")
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", color * .46f);
            }
            if (key is "glass" or "glass_body" or "glass_soft" or "acrylic")
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





