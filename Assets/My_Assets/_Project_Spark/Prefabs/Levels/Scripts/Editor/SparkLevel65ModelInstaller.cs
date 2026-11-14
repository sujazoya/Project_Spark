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
    public static class SparkLevel65ModelInstaller
    {
        public const string Folder = "Assets/My_Assets/_Project_Spark/Prefabs/Levels/Level_65_Layout/";
        public const string Prefab = Folder + "Level_65_Layout.prefab";
        public const string ScenePath = "Assets/My_Assets/_Project_Spark/Scenes/MainMenu/MainMenu.unity";
        private static readonly string[] Models = {
            "IndustrialSmartFactoryBase","MainFeedConveyor","ConveyorMotorDrive","ConveyorRotaryEncoder","ProductFeederMagazine","OpticalEntrySensor","VisionInspectionCamera","InspectionLightingModule","VisionProcessingController","SixAxisIndustrialRobot","RobotServoDriveBank","RobotJointEncoderModule","VacuumGripperAssembly","VacuumPumpModule","VacuumPressureSensor","RobotPickupPlatform","SortingConveyorA","SortingConveyorB","SortingConveyorC","PneumaticRejectGate","PneumaticSolenoidValve","AirPressureRegulator","RejectGatePositionSensors","ProductCollectionBinA","ProductCollectionBinB","ProductCollectionBinC","DefectiveProductRejectBin","IndustrialPLCRack","DigitalInputOutputModule","RobotMotionController","ConveyorSynchronizationController","SafetyPLCModule","EmergencyStopStation","SafetyLightCurtain","HolographicFactoryMonitor","ProductQualityScanner","TransparentRobotSafetyEnclosure","Regulated5VControlSupply"
        };
        private const string Materials = Folder + "Models/Materials";
        private const float SocketHeight = .025f;
        private const float SocketDiameter = .026f;

        private static readonly (string id, string label, float x, float z, string color)[] Sockets =
        {
            ("SUPPLY_POS","SUPPLY POS",-0.8400f,0.6300f,"red"),
            ("CTRL_POS","CTRL POS",-0.7650f,0.6300f,"red"),
            ("SUPPLY_RET","SUPPLY RET",-0.6900f,0.6300f,"blue"),
            ("CTRL_GND","CTRL GND",-0.6150f,0.6300f,"blue"),
            ("IO_5V","IO 5V",-0.5400f,0.6300f,"red"),
            ("IO_GND","IO GND",-0.4650f,0.6300f,"blue"),
            ("VISION_5V","VIS 5V",-0.3900f,0.6300f,"red"),
            ("VISION_GND","VIS GND",-0.3150f,0.6300f,"blue"),
            ("SERVO_5V","SERVO 5V",-0.2400f,0.6300f,"red"),
            ("SERVO_GND","SERVO GND",-0.1650f,0.6300f,"blue"),
            ("MOTOR_5V","MOTOR 5V",-0.0900f,0.6300f,"red"),
            ("MOTOR_GND","MOTOR GND",-0.0150f,0.6300f,"blue"),
            ("ENC_5V","ENC 5V",0.0600f,0.6300f,"red"),
            ("ENC_GND","ENC GND",0.1350f,0.6300f,"blue"),
            ("EYE_5V","EYE 5V",0.2100f,0.6300f,"red"),
            ("EYE_GND","EYE GND",0.2850f,0.6300f,"blue"),
            ("SAFE_5V","SAFE 5V",0.3600f,0.6300f,"red"),
            ("SAFE_GND","SAFE GND",0.4350f,0.6300f,"blue"),
            ("PLC_RUN","PLC RUN",0.5100f,0.6300f,"mint"),
            ("DRIVE_RUN","DRIVE RUN",0.5850f,0.6300f,"mint"),
            ("PLC_SPEED","PLC SPEED",0.6600f,0.6300f,"mint"),
            ("DRIVE_SPEED","DRIVE SPEED",0.7350f,0.6300f,"mint"),
            ("BELT_ENABLE","BELT EN",0.8100f,0.6300f,"mint"),
            ("BELT_REF","BELT REF",0.8850f,0.6300f,"mint"),
            ("DRIVE_OK","DRIVE OK",0.9600f,0.6300f,"mint"),
            ("PLC_DRIVE_OK","PLC DRIVE OK",1.0350f,0.6300f,"mint"),
            ("DRIVE_FAULT","DRIVE FAULT",-1.0200f,-0.3900f,"amber"),
            ("PLC_DRIVE_FAULT","PLC DRIVE FAULT",-0.9384f,-0.3900f,"amber"),
            ("FEED_ENABLE","FEED EN",-0.8568f,-0.3900f,"mint"),
            ("SYNC_ENABLE","SYNC EN",-0.7752f,-0.3900f,"mint"),
            ("SYNC_REF","SYNC REF",-0.6936f,-0.3900f,"mint"),
            ("EYE_OUT","EYE OUT",-0.6120f,-0.3900f,"mint"),
            ("PLC_ENTRY","PLC ENTRY",-0.5304f,-0.3900f,"mint"),
            ("CAM_TRIGGER","CAM TRIGGER",-0.4488f,-0.3900f,"mint"),
            ("LIGHT_ENABLE","LIGHT EN",-0.3672f,-0.3900f,"mint"),
            ("CAM_READY","CAM READY",-0.2856f,-0.3900f,"mint"),
            ("PLC_CAM_READY","PLC CAM READY",-0.2040f,-0.3900f,"mint"),
            ("CAM_FRAME","CAM FRAME",-0.1224f,-0.3900f,"mint"),
            ("VISION_IN","VIS IN",-0.0408f,-0.3900f,"mint"),
            ("VISION_OUT","VIS OUT",0.0408f,-0.3900f,"mint"),
            ("PLC_CLASS","PLC CLASS",0.1224f,-0.3900f,"mint"),
            ("QUALITY_IN","QUALITY IN",0.2040f,-0.3900f,"mint"),
            ("VISION_VALID","VIS VALID",0.2856f,-0.3900f,"mint"),
            ("PLC_VISION_VALID","PLC VIS VALID",0.3672f,-0.3900f,"mint"),
            ("DEFECT_OUT","DEFECT OUT",0.4488f,-0.3900f,"mint"),
            ("PLC_DEFECT","PLC DEFECT",0.5304f,-0.3900f,"mint"),
            ("PLC_ROBOT","PLC ROBOT",0.6120f,-0.3900f,"mint"),
            ("SERVO_ENABLE","SERVO EN",0.6936f,-0.3900f,"mint"),
            ("MOTION_ENABLE","MOTION EN",0.7752f,-0.3900f,"mint"),
            ("ROBOT_HOME","ROBOT HOME",0.8568f,-0.3900f,"mint"),
            ("MOTION_CMD","MOTION CMD",0.9384f,-0.3900f,"mint"),
            ("SERVO_BUS","SERVO BUS",1.0200f,-0.3900f,"mint"),
            ("JOINT_OUT","JOINT OUT",-1.0200f,-0.5450f,"mint"),
            ("MOTION_FEEDBACK","MOTION FB",-0.9384f,-0.5450f,"mint"),
            ("MOTION_DONE","MOTION DONE",-0.8568f,-0.5450f,"mint"),
            ("PLC_DONE","PLC DONE",-0.7752f,-0.5450f,"mint"),
            ("HOME_OK","HOME OK",-0.6936f,-0.5450f,"mint"),
            ("PLC_HOME_OK","PLC HOME OK",-0.6120f,-0.5450f,"mint"),
            ("SERVO_FAULT","SERVO FAULT",-0.5304f,-0.5450f,"amber"),
            ("PLC_SERVO_FAULT","PLC SERVO FAULT",-0.4488f,-0.5450f,"amber"),
            ("PLC_GRIP","PLC GRIP",-0.3672f,-0.5450f,"mint"),
            ("PUMP_ENABLE","PUMP EN",-0.2856f,-0.5450f,"mint"),
            ("VAC_OUT","VAC OUT",-0.2040f,-0.5450f,"mint"),
            ("PLC_VAC","PLC VAC",-0.1224f,-0.5450f,"mint"),
            ("GRIP_CONFIRM","GRIP OK",-0.0408f,-0.5450f,"mint"),
            ("PLC_RELEASE","PLC RELEASE",0.0408f,-0.5450f,"mint"),
            ("GRIP_RELEASE","GRIP RELEASE",0.1224f,-0.5450f,"mint"),
            ("PUMP_FAULT","PUMP FAULT",0.2040f,-0.5450f,"amber"),
            ("PLC_PUMP_FAULT","PLC PUMP FAULT",0.2856f,-0.5450f,"amber"),
            ("PLC_REJECT","PLC REJECT",0.3672f,-0.5450f,"amber"),
            ("VALVE_ENABLE","VALVE EN",0.4488f,-0.5450f,"mint"),
            ("AIR_OK","AIR OK",0.5304f,-0.5450f,"mint"),
            ("PLC_AIR","PLC AIR",0.6120f,-0.5450f,"mint"),
            ("GATE_EXT","GATE EXT",0.6936f,-0.5450f,"mint"),
            ("PLC_EXT","PLC EXT",0.7752f,-0.5450f,"mint"),
            ("GATE_RET","GATE RET",0.8568f,-0.5450f,"blue"),
            ("PLC_RET","PLC RET",0.9384f,-0.5450f,"blue"),
            ("GATE_FAULT","GATE FAULT",1.0200f,-0.5450f,"amber"),
            ("PLC_GATE_FAULT","PLC GATE FAULT",-1.0200f,-0.7000f,"amber"),
            ("SYNC_OUT","SYNC OUT",-0.9384f,-0.7000f,"mint"),
            ("SORT_A","SORT A",-0.8568f,-0.7000f,"mint"),
            ("SORT_B","SORT B",-0.7752f,-0.7000f,"mint"),
            ("SORT_C","SORT C",-0.6936f,-0.7000f,"mint"),
            ("BIN_A","BIN A",-0.6120f,-0.7000f,"mint"),
            ("PLC_BIN_A","PLC BIN A",-0.5304f,-0.7000f,"mint"),
            ("BIN_B","BIN B",-0.4488f,-0.7000f,"mint"),
            ("PLC_BIN_B","PLC BIN B",-0.3672f,-0.7000f,"mint"),
            ("BIN_C","BIN C",-0.2856f,-0.7000f,"mint"),
            ("PLC_BIN_C","PLC BIN C",-0.2040f,-0.7000f,"mint"),
            ("BIN_REJECT","BIN REJECT",-0.1224f,-0.7000f,"amber"),
            ("PLC_BIN_REJECT","PLC BIN REJECT",-0.0408f,-0.7000f,"amber"),
            ("GUARD_NC","GUARD NC",0.0408f,-0.7000f,"mint"),
            ("CTRL_SAFE","CTRL SAFE",0.1224f,-0.7000f,"amber"),
            ("SAFE_DOOR","SAFE DOOR",0.2040f,-0.7000f,"amber"),
            ("ESTOP_NC","ESTOP NC",0.2856f,-0.7000f,"amber"),
            ("SAFE_ESTOP","SAFE ESTOP",0.3672f,-0.7000f,"amber"),
            ("CURTAIN_NC","CURTAIN NC",0.4488f,-0.7000f,"mint"),
            ("SAFE_CURTAIN","SAFE CURTAIN",0.5304f,-0.7000f,"amber"),
            ("PROD_READY","PROD READY",0.6120f,-0.7000f,"mint"),
            ("HMI_RUN","HMI RUN",0.6936f,-0.7000f,"mint"),
            ("ENC_RATE","ENC RATE",0.7752f,-0.7000f,"mint"),
            ("HMI_SPEED","HMI SPEED",0.8568f,-0.7000f,"mint"),
            ("ENC_PULSE","ENC PULSE",0.9384f,-0.7000f,"mint"),
            ("PLC_ENCODER","PLC ENCODER",1.0200f,-0.7000f,"mint"),
        };

        public static IEnumerable<string> TerminalIds => Sockets.Select(socket => socket.id);

        private static readonly Dictionary<string, string> Palette = new()
        {
            {"red","EE485C"},{"blue","1671E2"},{"amber","FFA82B"},{"violet","A282FF"},
            {"mint","34F6C5"},{"cyan","23DAFD"},{"void","07111C"},{"white","D4F5FF"}
        };

        [MenuItem("Tools/Project Spark/Levels/Create Level 65")]
        public static void Install()
        {
            SparkLevel65Builder.Create();
            var root = AssetDatabase.LoadAssetAtPath<GameObject>(Prefab);
            if (root == null || root.GetComponent<SparkLevel65Factory>().controllerDisplay == null) Fit();
            Register();
            ValidateAssets();
        }

        [MenuItem("Tools/Project Spark/Level 65/Validate Level Assets")]
        public static void ValidateAssets()
        {
            var root=AssetDatabase.LoadAssetAtPath<GameObject>(Prefab);
            var control=root != null ? root.GetComponent<SparkLevel65Factory>() : null;
            var bindings=root != null ? root.GetComponent<SparkLevelSceneBindings>() : null;
            if (control==null || bindings==null || control.controllerDisplay==null || control.qualityDisplay==null || control.rig==null ||
                control.pins.Length!=104 || control.pins.Distinct().Count()!=104 || 
                !TerminalIds.SequenceEqual(control.pins.Select(pin=>pin.name)) ||
                control.pins.Any(pin=>!bindings.TryGetTerminal(pin.name,out var bound) || bound!=pin) ||
                SparkLevel65Factory.Gates.Any(id=>!bindings.IsComponentIdDeclared(id)))
                throw new InvalidOperationException("Level 65 wiring, motion or binding references are incomplete.");
            Debug.Log("[LEVEL 65] Validated 104 distinct terminals, 66 player wires and 6 evaluator gates.",root);
        }

        private static void Register()
        {
            var scene = SceneManager.GetSceneByPath(ScenePath);
            bool opened = !scene.isLoaded;
            if (opened) scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
            bool dirty = scene.isDirty;
            var objects = scene.GetRootGameObjects();
            T Find<T>() where T : Component => objects.SelectMany(item => item.GetComponentsInChildren<T>(true)).Single();
            var level = AssetDatabase.LoadAssetAtPath<SparkLevelDefinition>(Folder + "Level_65.asset");
            var drawing = AssetDatabase.LoadAssetAtPath<SparkLevelDrawingAsset>(Folder + "Level_65_Drawing.asset");
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
            else if (dirty) Debug.Log("MainMenu had unsaved edits. Save the scene to persist Level 65's campaign and teaching entries.");
        }

        private static void Append(Object target, string field, Object value)
        {
            if (value == null) throw new InvalidOperationException("Missing Level 65 asset for " + field);
            var data = new SerializedObject(target);
            var list = data.FindProperty(field);
            for (int i = 0; i < list.arraySize; i++)
                if (list.GetArrayElementAtIndex(i).objectReferenceValue == value) return;
            list.arraySize++;
            list.GetArrayElementAtIndex(list.arraySize - 1).objectReferenceValue = value;
            data.ApplyModifiedProperties();
        }

        [MenuItem("Tools/Project Spark/Level 65/Fit Breadboard Assembly")]
        public static void Fit()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Exit Play Mode before fitting Level 65.");
            var models = Models.Select((name, index) =>
            {
                string path = Folder + $"Models/L65_{index:00}_{name}_Fitted.fbx";
                return AssetDatabase.LoadAssetAtPath<GameObject>(path) ??
                    throw new InvalidOperationException("Missing fitted kit module " + path);
            }).ToArray();
            if (AssetDatabase.LoadAssetAtPath<GameObject>(Prefab) == null)
                throw new InvalidOperationException("The Level 65 prefab is missing.");

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
                    throw new InvalidOperationException("Unity could not save the Level 65 layout.");
                AssetDatabase.SaveAssets();
                Debug.Log("Level 65 fitted to the MainMenu breadboard with 104 seated terminals.");
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
            Vector3 right = Vector3.ProjectOnPlane(grid.GridOrigin.right, up).normalized * .065f;
            Vector3 forward = Vector3.ProjectOnPlane(grid.GridOrigin.forward, up).normalized * .065f;
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
            var bench = root.transform.Find("Factory Workbench");
            var bindings = root.GetComponent<SparkLevelSceneBindings>();
            var loop = root.GetComponent<SparkLevel65Factory>();
            if (bench == null || bindings == null || loop == null)
                throw new InvalidOperationException("Level 65 is missing its workbench, controls, or scene bindings.");

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
            float worldScale = Mathf.Min(width / 2.05f, depth / 1.56f);
            bench.localScale = Vector3.one * (worldScale / parentWorld.MultiplyVector(Vector3.right).magnitude);
            Matrix4x4 workbenchWorld = layout * bench.localToWorldMatrix;
            Vector3 ToWorld(Vector3 point) => workbenchWorld.MultiplyPoint3x4(point);
            Vector3 ToLocal(Vector3 point) => workbenchWorld.inverse.MultiplyPoint3x4(point);
            float floor = ToLocal(SurfacePoint(grid, ToWorld(Vector3.zero), up)).y;

            foreach (string name in new[] {"Fitted Level 65 Models", "Breadboard Labels", "Motion pivots", "Product variants"})
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
            var assembly = Child(bench, "Fitted Level 65 Models", Vector3.up * floor);
            foreach (var model in models)
            {
                var instance = Object.Instantiate(model, assembly, false).transform;
                var markers = instance.GetComponentsInChildren<Transform>(true)
                    .Where(item => item.name.StartsWith("L65_Board",StringComparison.Ordinal))
                    .ToDictionary(item => item.name);
                Transform Marker(string name) => markers["L65_Board" + name];
                Vector3 Axis(string name) => assembly.InverseTransformVector(Marker(name).position - Marker("Center").position);
                instance.localRotation = Quaternion.Inverse(Quaternion.LookRotation(-Axis("Front"), Axis("Up"))) * instance.localRotation;
                instance.localScale /= Axis("Right").magnitude;
                instance.localPosition -= assembly.InverseTransformPoint(Marker("Center").position);
                if (Axis("Right").x < .99f)
                    throw new InvalidOperationException("Unexpected axes on fitted model " + model.name);
            }

            assembly.localScale = Vector3.one * .88f;
            if (!AssetDatabase.IsValidFolder(Materials)) AssetDatabase.CreateFolder(Folder + "Models", "Materials");
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) throw new InvalidOperationException("The project URP Lit shader is missing.");
            foreach (var mesh in assembly.GetComponentsInChildren<MeshRenderer>())
            {
                string key = mesh.name.Substring(mesh.name.LastIndexOf("__", StringComparison.Ordinal) + 2);
                var material = MaterialFor(shader, key);
                // The fitted meshes can contain several imported submeshes.
                mesh.sharedMaterials = Enumerable.Repeat(material, mesh.GetComponent<MeshFilter>().sharedMesh.subMeshCount).ToArray();
                if (key is "glass" or "acrylic") mesh.shadowCastingMode = ShadowCastingMode.Off;
            }

            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (loop.calibrationText == null)
            {
                var oldControls = root.transform.Find("Level 65 Controls");
                if (oldControls != null) Object.DestroyImmediate(oldControls.gameObject);
                SparkLevel65Builder.BuildControls(root.transform,loop,font);
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
                int index = Array.FindIndex(loop.pins, pin => pin == terminal);
                var terminalData = new SerializedObject(terminal);
                terminalData.FindProperty("maxConnections").intValue = SparkLevel65Factory.Wires.Count(wire => wire.a == index || wire.b == index);
                terminalData.ApplyModifiedPropertiesWithoutUndo();
                owners.Add(terminal.transform.parent);
                terminal.GetComponent<Renderer>().enabled = true;
                var projected = ToWorld(new Vector3(spec.x, floor, spec.z));
                projected -= up * Vector3.Dot(projected - grid.GridOrigin.position, up);
                if (!grid.TryGetNearestHole(projected, .32f, out _, out int column, out int row))
                    throw new InvalidOperationException("No breadboard row for " + spec.id);
                int columns = gridData.FindProperty("columns").intValue;
                int rows = gridData.FindProperty("rows").intValue;
                Vector3 hole = default;
                float best = float.PositiveInfinity;
                Vector2Int chosen = default;
                for (int r = 0; r < rows; r++)
                    for (int c = 0; c < columns; c++)
                    {
                        var index2D = new Vector2Int(c,r);
                        if (holes.Contains(index2D) || Mathf.Abs(r-row)>1 || Mathf.Abs(c-column)>4) continue;
                        var candidate = grid.GridOrigin.TransformPoint(new Vector3(c*spacing,0,r*spacing));
                        var local = ToLocal(candidate);
                        if (local.x < -1.02f || local.x > 1.04f || (spec.z>0 && local.x<-.87f)) continue;
                        float cost=(candidate-projected).sqrMagnitude;
                        if (cost>=best) continue;
                        best=cost;
                        chosen=index2D;
                        hole=candidate;
                    }
                if (float.IsPositiveInfinity(best))
                    throw new InvalidOperationException("No separate breadboard hole for " + spec.id);
                holes.Add(chosen);
                Vector3 seat = ToLocal(SurfacePoint(grid, hole, up));
                Seat(terminal, bench, seat, pinLayer, MaterialFor(shader, spec.color), MaterialFor(shader, "void"));

                float offset = Array.IndexOf(Sockets,spec)%2==0 ? -.041f : -.067f;
                Label(labels,font,spec.id,spec.label,seat+new Vector3(0,.014f,offset),.0115f);
            }
            foreach (var owner in owners)
                foreach (Transform child in owner)
                    if (child.GetComponent<SparkTerminal>() == null && child.GetComponent<Renderer>() != null)
                        child.gameObject.SetActive(false);

            var titles = new (string title, float x, float z)[]
            {






                ("FEED / VISION",-.57f,.33f),("ROBOT CELL",.49f,.33f),
                ("PLC / SAFETY",-.56f,-.29f),("SORTING / REJECT",.56f,-.29f)
            };
            foreach(var title in titles)
                Label(labels,font,title.title,title.title,new Vector3(title.x,floor+.028f,title.z),.018f);
            Transform Pivot(string module,string key)
            {
                var name="fitted_"+module+"_"+key;
                return assembly.GetComponentsInChildren<Transform>(true).Single(item=>item.name==name);
            }
            Transform Module(string name)=>assembly.GetComponentsInChildren<Transform>(true)
                .Single(item=>item.name=="L65_"+name);
            Vector3 Center(string name)
            {
                var renderers=Module(name).GetComponentsInChildren<Renderer>();
                var bounds=renderers[0].bounds;
                foreach(var mesh in renderers)bounds.Encapsulate(mesh.bounds);
                var local=bench.InverseTransformPoint(bounds.center);
                local.y=bench.InverseTransformPoint(bounds.center+bench.up*bounds.extents.y).y;
                return local;
            }
            var rig=loop.rig;
            rig.encoder=Pivot("ConveyorRotaryEncoder","ANIM_ENCODER_DISC");
            rig.joints=Enumerable.Range(1,6).Select(i=>Pivot("SixAxisIndustrialRobot","ANIM_ROBOT_J"+i)).ToArray();
            rig.gripper=Pivot("VacuumGripperAssembly","ANIM_GRIPPER_LIFT");
            rig.gate=Pivot("PneumaticRejectGate","ANIM_REJECT_GATE");
            rig.stop=Pivot("EmergencyStopStation","ANIM_EMERGENCY_STOP");
            rig.guard=Pivot("TransparentRobotSafetyEnclosure","ANIM_SAFETY_DOOR");
            rig.monitor=Pivot("HolographicFactoryMonitor","ANIM_MONITOR_TILT");
            for(int i=1;i<rig.joints.Length;i++)rig.joints[i].SetParent(rig.joints[i-1],true);
            rig.gripper.SetParent(rig.joints[5],true);
            rig.bench=bench;
            rig.feed=Center("ProductFeederMagazine");
            rig.pick=Center("RobotPickupPlatform");
            rig.vision=Center("VisionInspectionCamera");
            rig.vision.y=rig.pick.y;
            rig.feed.y=rig.pick.y;
            rig.gatePosition=Center("PneumaticRejectGate");
            rig.sortStarts=new[]{"SortingConveyorA","SortingConveyorB","SortingConveyorC","PneumaticRejectGate"}.Select(Center).ToArray();
            rig.bins=new[]{"ProductCollectionBinA","ProductCollectionBinB","ProductCollectionBinC","DefectiveProductRejectBin"}.Select(Center).ToArray();
            string[] variants={"Product_1_A_RedCube","Product_2_B_BlueCylinder","Product_3_C_YellowGear","Product_4_D_DefectivePart"};
            string[] tints={"red","blue","amber","violet"};
            var samples=Child(bench,"Product variants",Vector3.zero);
            rig.products=new GameObject[variants.Length];
            for(int i=0;i<variants.Length;i++)
            {
                var model=AssetDatabase.LoadAssetAtPath<GameObject>(Folder+"Models/"+variants[i]+".fbx");
                if(!model)throw new InvalidOperationException("Missing product "+variants[i]);
                var instance=Object.Instantiate(model,samples,false);
                instance.name=variants[i];
                var renderers=instance.GetComponentsInChildren<Renderer>();
                var bounds=renderers[0].bounds;
                foreach(var renderer in renderers)
                {
                    bounds.Encapsulate(renderer.bounds);
                    renderer.sharedMaterials=Enumerable.Repeat(MaterialFor(shader,tints[i]),renderer.sharedMaterials.Length).ToArray();
                }
                float productWorldScale=bench.TransformVector(Vector3.right).magnitude;
                var size=bounds.size;
                float factor=.058f*productWorldScale/Mathf.Max(size.x,Mathf.Max(size.y,size.z));
                var scaleRoot=Child(samples,"Part "+(i+1),Vector3.zero);
                instance.transform.SetParent(scaleRoot,false);
                instance.transform.localPosition-=samples.InverseTransformPoint(bounds.center);
                scaleRoot.localScale=Vector3.one*factor;
                rig.products[i]=scaleRoot.gameObject;
                scaleRoot.gameObject.SetActive(false);
            }

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
            loop.controllerDisplay=Readout("Bus readout",new Vector3(-.53f,floor+.18f,.42f),new Color(.46f,.94f,1f));
            loop.qualityDisplay=Readout("Power readout",new Vector3(.53f,floor+.325f,.44f),new Color(.68f,1f,.86f));
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
            label.fontSize = 65;
            label.characterSize = size;
            label.anchor = TextAnchor.MiddleCenter;
            label.color = new Color(.89f, .96f, 1f);
            label.text = text;
            child.GetComponent<MeshRenderer>().sharedMaterial = font.material;
            return label;
        }

        private static Material MaterialFor(Shader shader, string key)
        {
            string path = $"{Materials}/L65 {key}.mat";
            if (key.StartsWith("tint",StringComparison.Ordinal) && key.Length==10) Palette[key]=key.Substring(4);
            var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing != null && key is not ("glass" or "acrylic") &&
                ColorUtility.TryParseHtmlString("#" + (Palette.TryGetValue(key, out var savedTint) ? savedTint : "9FB3C8"), out var savedColor) &&
                existing.GetColor("_BaseColor") == savedColor) return existing;
            ColorUtility.TryParseHtmlString("#" + (Palette.TryGetValue(key, out var tint) ? tint : "9FB3C8"), out var color);
            var material = existing != null ? existing : new Material(shader) { name = "L65 " + key };
            bool metal = key is "chrome" or "titanium" or "steel" or "gold" or "copper" or "aluminum";
            material.SetFloat("_Metallic", metal ? .65f : .02f);
            material.SetFloat("_Smoothness", metal ? .65f : key == "glass" ? .12f : .35f);
            if (key is "green" or "cyan" or "red" or "blue" or "orange" or "amber" or "mint" or "warm" or "ice")
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", color * .65f);
            }
            if (key is "glass" or "glass_body" or "glass_soft" or "acrylic" or "glassblue" or "film" or "Glass" or "guardglass")
            {
                color.a = key=="film" ? .65f : .10f;
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





