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
    public static class SparkLevel55ModelInstaller
    {
        public const string Folder = "Assets/My_Assets/_Project_Spark/Prefabs/Levels/Level_55_Layout/";
        public const string Prefab = Folder + "Level_55_Layout.prefab";
        public const string ScenePath = "Assets/My_Assets/_Project_Spark/Scenes/MainMenu/MainMenu.unity";
        private static readonly string[] Models = {
            "Frame","Rotor","BearingL","BearingR","TouchL","TouchR","Motor","Encoder","Hub","ProbeL","ProbeR",
            "DriverL","DriverR","MotorDriver","Controller","Monitor","Disturbance","Guard","Axial","Supply"
        };
        private const string Materials = Folder + "Models/Materials";
        private const float SocketHeight = .025f;
        private const float SocketDiameter = .039f;

        private static readonly (string id, string label, float x, float z, string color)[] Sockets =
        {
            ("SUPPLY_POS","+5 V",0.7650f,0.4960f,"red"),
            ("CTRL_POS","C +",-0.4310f,-0.3900f,"red"),
            ("SUPPLY_RET","RETURN",0.9220f,0.4960f,"blue"),
            ("CTRL_GND","C GND",-0.3250f,-0.3900f,"blue"),
            ("MD_POS","MD +",-0.9610f,-0.3900f,"red"),
            ("MD_GND","MD GND",-0.8550f,-0.3900f,"blue"),
            ("LD_POS","L +",-0.9590f,-0.5730f,"red"),
            ("LD_GND","L GND",-0.8350f,-0.5730f,"blue"),
            ("RD_POS","R +",0.4050f,-0.5730f,"red"),
            ("RD_GND","R GND",0.5290f,-0.5730f,"blue"),
            ("HUB_POS","H +",-0.3390f,-0.5730f,"red"),
            ("HUB_GND","H GND",-0.2150f,-0.5730f,"blue"),
            ("HUB_XL","XL OUT",-0.0910f,-0.5730f,"cyan"),
            ("CTRL_XL","XL IN",-0.2190f,-0.3900f,"cyan"),
            ("HUB_YL","YL OUT",0.0330f,-0.5730f,"cyan"),
            ("CTRL_YL","YL IN",-0.1130f,-0.3900f,"cyan"),
            ("HUB_XR","XR OUT",0.1570f,-0.5730f,"cyan"),
            ("CTRL_XR","XR IN",-0.0070f,-0.3900f,"cyan"),
            ("HUB_YR","YR OUT",0.2810f,-0.5730f,"cyan"),
            ("CTRL_YR","YR IN",0.0990f,-0.3900f,"cyan"),
            ("ENC_A","ENC A",-0.6480f,0.4960f,"cyan"),
            ("CTRL_ENC_A","A IN",0.2050f,-0.3900f,"cyan"),
            ("ENC_B","ENC B",-0.4910f,0.4960f,"cyan"),
            ("CTRL_ENC_B","B IN",0.3110f,-0.3900f,"cyan"),
            ("ENC_Z","ENC Z",-0.3340f,0.4960f,"cyan"),
            ("CTRL_ENC_Z","Z IN",0.4170f,-0.3900f,"cyan"),
            ("CTRL_PWM","PWM OUT",0.5230f,-0.3900f,"cyan"),
            ("MD_PWM","MD PWM",-0.7490f,-0.3900f,"cyan"),
            ("MD_MPLUS","M OUT +",-0.6430f,-0.3900f,"amber"),
            ("MOTOR_PLUS","M +",-0.8050f,0.4960f,"amber"),
            ("MD_MMINUS","M OUT −",-0.5370f,-0.3900f,"blue"),
            ("MOTOR_MINUS","M −",-0.9620f,0.4960f,"blue"),
            ("CTRL_CMD_LEFT","L CMD",0.6290f,-0.3900f,"cyan"),
            ("LD_CMD","L CMD",-0.7110f,-0.5730f,"cyan"),
            ("CTRL_CMD_RIGHT","R CMD",0.7350f,-0.3900f,"cyan"),
            ("RD_CMD","R CMD",0.6530f,-0.5730f,"cyan"),
            ("LD_PWRPOS","L OUT +",-0.5870f,-0.5730f,"amber"),
            ("MAG_A_PWRPOS","COIL L +",-0.1770f,0.4960f,"amber"),
            ("LD_PWRNEG","L OUT −",-0.4630f,-0.5730f,"blue"),
            ("MAG_A_PWRNEG","COIL L −",-0.0200f,0.4960f,"blue"),
            ("RD_PWRPOS","R OUT +",0.7770f,-0.5730f,"amber"),
            ("MAG_B_PWRPOS","COIL R +",0.1370f,0.4960f,"amber"),
            ("RD_PWRNEG","R OUT −",0.9010f,-0.5730f,"blue"),
            ("MAG_B_PWRNEG","COIL R −",0.2940f,0.4960f,"blue"),
            ("GUARD_NC_OUT","GUARD",0.6080f,0.4960f,"cyan"),
            ("CTRL_SAFE","SAFE",0.8410f,-0.3900f,"cyan"),
            ("TOUCHDOWN_OUT","TD OUT",0.4510f,0.4960f,"cyan"),
            ("CTRL_TOUCHDOWN","TD IN",0.9470f,-0.3900f,"cyan"),
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
            {"titanium","456175"},
            {"blue_steel","2B4A61"},
            {"navy","0B1828"},
            {"carbon","091118"},
            {"dark","111A25"},
            {"chrome","BDD9E6"},
            {"silver","7DA6BF"},
            {"silver_dark","52708A"},
            {"copper","C4571F"},
            {"bronze","D67D2E"},
            {"gold","F5AD40"},
            {"electric","0D8FFA"},
            {"green","1AED6E"},
            {"white","D1F2FF"},
            {"acrylic","54B5DB"},
            {"acrylic_rim","6EB0D4"},
            {"rubber","090D13"},
            {"display","04111B"},
            {"grid","14384A"},
            {"warning","F2A614"},
            {"violet","8C59ED"},
        };

        [MenuItem("Tools/Project Spark/Levels/Create Level 55")]
        public static void Install()
        {
            SparkLevel55Builder.Create();
            var root = AssetDatabase.LoadAssetAtPath<GameObject>(Prefab);
            if (root == null || root.GetComponent<SparkLevel55Float>().controllerDisplay == null) Fit();
            Register();
            ValidateAssets();
        }

        [MenuItem("Tools/Project Spark/Level 55/Validate Level Assets")]
        public static void ValidateAssets()
        {
            var root=AssetDatabase.LoadAssetAtPath<GameObject>(Prefab);
            var control=root != null ? root.GetComponent<SparkLevel55Float>() : null;
            var bindings=root != null ? root.GetComponent<SparkLevelSceneBindings>() : null;
            if (control==null || bindings==null || control.controllerDisplay==null || control.qualityDisplay==null || control.rig==null ||
                control.pins.Length!=48 || control.pins.Distinct().Count()!=48 || 
                !TerminalIds.SequenceEqual(control.pins.Select(pin=>pin.name)) ||
                control.pins.Any(pin=>!bindings.TryGetTerminal(pin.name,out var bound) || bound!=pin) ||
                SparkLevel55Float.Gates.Any(id=>!bindings.IsComponentIdDeclared(id)))
                throw new InvalidOperationException("Level 55 wiring, motion or binding references are incomplete.");
            Debug.Log("[LEVEL 55] Validated 48 distinct terminals, 28 player wires and 6 evaluator gates.",root);
        }

        private static void Register()
        {
            var scene = SceneManager.GetSceneByPath(ScenePath);
            bool opened = !scene.isLoaded;
            if (opened) scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
            bool dirty = scene.isDirty;
            var objects = scene.GetRootGameObjects();
            T Find<T>() where T : Component => objects.SelectMany(item => item.GetComponentsInChildren<T>(true)).Single();
            var level = AssetDatabase.LoadAssetAtPath<SparkLevelDefinition>(Folder + "Level_55.asset");
            var drawing = AssetDatabase.LoadAssetAtPath<SparkLevelDrawingAsset>(Folder + "Level_55_Drawing.asset");
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
            else if (dirty) Debug.Log("MainMenu had unsaved edits. Save the scene to persist Level 55's campaign and teaching entries.");
        }

        private static void Append(Object target, string field, Object value)
        {
            if (value == null) throw new InvalidOperationException("Missing Level 55 asset for " + field);
            var data = new SerializedObject(target);
            var list = data.FindProperty(field);
            for (int i = 0; i < list.arraySize; i++)
                if (list.GetArrayElementAtIndex(i).objectReferenceValue == value) return;
            list.arraySize++;
            list.GetArrayElementAtIndex(list.arraySize - 1).objectReferenceValue = value;
            data.ApplyModifiedProperties();
        }

        [MenuItem("Tools/Project Spark/Level 55/Fit Breadboard Assembly")]
        public static void Fit()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Exit Play Mode before fitting Level 55.");
            var models = Models.Select((name, index) =>
            {
                string path = Folder + $"Models/L55_{index:00}_{name}_Fitted.fbx";
                return AssetDatabase.LoadAssetAtPath<GameObject>(path) ??
                    throw new InvalidOperationException("Missing fitted kit module " + path);
            }).ToArray();
            if (AssetDatabase.LoadAssetAtPath<GameObject>(Prefab) == null)
                throw new InvalidOperationException("The Level 55 prefab is missing.");

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
                    throw new InvalidOperationException("Unity could not save the Level 55 layout.");
                AssetDatabase.SaveAssets();
                Debug.Log("Level 55 fitted to the MainMenu breadboard with 48 seated terminals.");
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
            Vector3 right = Vector3.ProjectOnPlane(grid.GridOrigin.right, up).normalized * .055f;
            Vector3 forward = Vector3.ProjectOnPlane(grid.GridOrigin.forward, up).normalized * .055f;
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
            var bench = root.transform.Find("Levitation Workbench");
            var bindings = root.GetComponent<SparkLevelSceneBindings>();
            var loop = root.GetComponent<SparkLevel55Float>();
            if (bench == null || bindings == null || loop == null)
                throw new InvalidOperationException("Level 55 is missing its workbench, controls, or scene bindings.");

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

            foreach (string name in new[] {"Fitted Level 55 Models", "Breadboard Labels", "Motion pivots"})
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
            var assembly = Child(bench, "Fitted Level 55 Models", Vector3.up * floor);
            foreach (var model in models)
            {
                var instance = Object.Instantiate(model, assembly, false).transform;
                var markers = instance.GetComponentsInChildren<Transform>(true)
                    .Where(item => item.name.StartsWith("L55_Board",StringComparison.Ordinal))
                    .ToDictionary(item => item.name);
                Transform Marker(string name) => markers["L55_Board" + name];
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
            // Keep both front rows fully exposed from the gameplay camera.
            foreach (var instance in assembly.Cast<Transform>())
            {
                if (instance.name.StartsWith("L55_08_",StringComparison.Ordinal) ||
                    instance.name.StartsWith("L55_11_",StringComparison.Ordinal) ||
                    instance.name.StartsWith("L55_12_",StringComparison.Ordinal) ||
                    instance.name.StartsWith("L55_13_",StringComparison.Ordinal) ||
                    instance.name.StartsWith("L55_14_",StringComparison.Ordinal))
                    instance.localPosition += Vector3.forward * .123f;
                if (instance.name.StartsWith("L55_15_",StringComparison.Ordinal))
                    instance.localPosition += Vector3.forward * .16f;
            }
            foreach (var mesh in assembly.GetComponentsInChildren<MeshRenderer>())
            {
                string key = mesh.name.Substring(mesh.name.LastIndexOf("__", StringComparison.Ordinal) + 2);
                var material = MaterialFor(shader, key);
                // The fitted meshes can contain several imported submeshes.
                mesh.sharedMaterials = Enumerable.Repeat(material, mesh.GetComponent<MeshFilter>().sharedMesh.subMeshCount).ToArray();
                if (key is "glass" or "acrylic") mesh.shadowCastingMode = ShadowCastingMode.Off;
            }

            SparkLevel55Builder.ConfigurePowerHardware(loop.driver,loop.leftCoils,loop.rightCoils);
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (loop.calibrationText == null)
            {
                var oldControls = root.transform.Find("Level 55 Controls");
                if (oldControls != null) Object.DestroyImmediate(oldControls.gameObject);
                SparkLevel55Builder.BuildControls(root.transform,loop,font);
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

                float offset=spec.id is "CTRL_ENC_B" or "CTRL_XL" or "CTRL_PWM" or "MD_POS"
                    ? -.079f : Array.IndexOf(Sockets,spec)%2==0 ? -.046f : -.059f;
                Label(labels,font,spec.id,spec.label,seat+new Vector3(0,.014f,offset),.0135f);
            }
            foreach (var owner in owners)
                foreach (Transform child in owner)
                    if (child.GetComponent<SparkTerminal>() == null && child.GetComponent<Renderer>() != null)
                        child.gameObject.SetActive(false);

            var titles = new (string title, float x, float z)[]
            {






                ("LEFT MAGNETIC BEARING",-.41f,.34f),("RIGHT MAGNETIC BEARING",.41f,.34f),
                ("ACTIVE LEVITATION",.69f,-.295f),("SHAFT POSITION",-.47f,.655f)
            };
            foreach(var title in titles)
                Label(labels,font,title.title,title.title,new Vector3(title.x,floor+.027f,title.z),.018f);
            var motion=Child(bench,"Motion pivots",Vector3.zero);
            var parts=assembly.GetComponentsInChildren<Transform>(true);
            Transform Pivot(string name)
            {
                var part=parts.Single(item=>item.name==name);
                var pivot=Child(motion,name,Vector3.zero);
                pivot.SetPositionAndRotation(part.position,bench.rotation);
                part.SetParent(pivot,true);
                return pivot;
            }
            var rig=new SerializedObject(loop.rig);
            var spin=Pivot("ANIM_ROTOR_SPIN");
            var carrier=Child(motion,"Rotor position and tilt",spin.localPosition);
            spin.SetParent(carrier,true);
            rig.FindProperty("rotorSpin").objectReferenceValue=spin;
            rig.FindProperty("rotorArtRoot").objectReferenceValue=carrier;
            foreach(var (field,name) in new[] {
                ("encoderDisc","ANIM_ENCODER_DISC"),("safetyDoor","ANIM_SAFETY_DOOR"),("disturbancePlunger","ANIM_DISTURBANCE_PLUNGER") })
                rig.FindProperty(field).objectReferenceValue=Pivot(name);
            void Lamps(string field,string pivot)
            {
                var lamps=parts.Single(item=>item.name==pivot).GetComponentsInChildren<Renderer>(true);
                var property=rig.FindProperty(field);
                property.arraySize=lamps.Length;
                for(int i=0;i<lamps.Length;i++) property.GetArrayElementAtIndex(i).objectReferenceValue=lamps[i];
            }
            Lamps("leftIndicators","ANIM_LEFT_COIL_LIGHTS");
            Lamps("rightIndicators","ANIM_RIGHT_COIL_LIGHTS");
            rig.FindProperty("positionAmplification").floatValue=16;
            rig.FindProperty("supportSpanMeters").floatValue=.632f;
            rig.FindProperty("disturbanceArtTravel").floatValue=.032f;
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
            loop.controllerDisplay=Readout("Controller screen",new Vector3(.73f,floor+.126f,-.274f),new Color(.46f,.94f,1f));
            loop.qualityDisplay=Readout("Position monitor",new Vector3(0f,floor+.347f,.551f),new Color(.68f,1f,.86f));
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
            string path = $"{Materials}/L55 {key}.mat";
            var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing != null && key is not ("glass" or "acrylic") &&
                ColorUtility.TryParseHtmlString("#" + (Palette.TryGetValue(key, out var savedTint) ? savedTint : "9FB3C8"), out var savedColor) &&
                existing.GetColor("_BaseColor") == savedColor) return existing;
            ColorUtility.TryParseHtmlString("#" + (Palette.TryGetValue(key, out var tint) ? tint : "9FB3C8"), out var color);
            var material = existing != null ? existing : new Material(shader) { name = "L55 " + key };
            bool metal = key is "chrome" or "titanium" or "steel" or "gold" or "copper" or "aluminum";
            material.SetFloat("_Metallic", metal ? .55f : .02f);
            material.SetFloat("_Smoothness", metal ? .64f : key == "glass" ? .12f : .35f);
            if (key is "green" or "cyan" or "red" or "blue" or "orange" or "amber" or "mint" or "warm" or "ice")
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", color * .55f);
            }
            if (key is "glass" or "glass_body" or "glass_soft" or "acrylic" or "glassblue" or "film" or "Glass" or "guardglass")
            {
                color.a = key=="film" ? .55f : .10f;
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




