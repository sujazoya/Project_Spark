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
    public static class SparkLevel66ModelInstaller
    {
        public const string Folder = "Assets/My_Assets/_Project_Spark/Prefabs/Levels/Level_66_Layout/";
        public const string Prefab = Folder + "Level_66_Layout.prefab";
        public const string ScenePath = "Assets/My_Assets/_Project_Spark/Scenes/MainMenu/MainMenu.unity";
        private static readonly string[] Models = {
            "IndustrialPredictiveMaintenanceBase","IndustrialThreePhaseMotor","MotorStatorCutaway","MotorRotorShaft","IndustrialGearboxHousing","GearboxGearAssembly","DriveShaftCoupling","DriveEndBearingCartridge","NonDriveEndBearingCartridge","DamagedBearingCartridge","HealthyReplacementBearing","IndustrialConveyorAssembly","ConveyorDriveRoller","VariableFrequencyMotorDrive","MotorCurrentSensorBank","ThreeAxisVibrationSensor","SecondaryVibrationSensor","PrecisionShaftEncoder","BearingTemperatureProbe","MotorTemperatureProbe","InfraredThermalCamera","LubricationMonitoringModule","ShaftAlignmentSensor","GearWearSensorModule","VibrationFFTAnalyzer","ConditionMonitoringController","PredictiveMaintenanceAIUnit","IndustrialDigitalTwinDisplay","MotorProtectionRelay","MaintenancePLCRack","DigitalInputOutputModule","BearingRemovalTool","PrecisionAlignmentGauge","IndustrialTorqueTool","MaintenancePartsStorageRack","ActiveCoolingAssembly","EmergencyStopAndLockoutPanel","HolographicDiagnosticMonitor","TransparentMachineSafetyEnclosure","Regulated5VControlSupply"
        };
        private const string Materials = Folder + "Models/Materials";
        private const float SocketHeight = .025f;
        private const float SocketDiameter = .026f;

        private static readonly (string id, string label, float x, float z, string color)[] Sockets =
        {
            ("SUPPLY_POS","SUPPLY POS",-0.8100f,0.6800f,"red"),
            ("PLC_5V","PLC 5V",-0.7070f,0.6800f,"red"),
            ("SUPPLY_RET","SUPPLY RET",-0.6040f,0.6800f,"blue"),
            ("PLC_GND","PLC GND",-0.5010f,0.6800f,"blue"),
            ("IO_5V","IO 5V",-0.3980f,0.6800f,"red"),
            ("IO_GND","IO GND",-0.2950f,0.6800f,"blue"),
            ("VFD_5V","VFD 5V",-0.1920f,0.6800f,"red"),
            ("VFD_GND","VFD GND",-0.0890f,0.6800f,"blue"),
            ("RELAY_5V","RELAY 5V",0.0140f,0.6800f,"red"),
            ("RELAY_GND","RELAY GND",0.1170f,0.6800f,"blue"),
            ("CM_5V","CM 5V",0.2200f,0.6800f,"red"),
            ("CM_GND","CM GND",0.3230f,0.6800f,"blue"),
            ("FFT_5V","FFT 5V",0.4260f,0.6800f,"red"),
            ("FFT_GND","FFT GND",0.5290f,0.6800f,"blue"),
            ("TWIN_5V","TWIN 5V",0.6320f,0.6800f,"red"),
            ("TWIN_GND","TWIN GND",0.7350f,0.6800f,"blue"),
            ("SENSOR_5V","SENSOR 5V",0.8380f,0.6800f,"red"),
            ("SENSOR_GND","SENSOR GND",0.9410f,0.6800f,"blue"),
            ("DRIVE_5V","DRIVE 5V",-0.9200f,0.5150f,"red"),
            ("DRIVE_GND","DRIVE GND",-0.8220f,0.5150f,"blue"),
            ("PLC_RUN","PLC RUN",-0.7240f,0.5150f,"mint"),
            ("VFD_RUN","VFD RUN",-0.6260f,0.5150f,"mint"),
            ("PLC_SPEED","PLC SPEED",-0.5280f,0.5150f,"mint"),
            ("VFD_REF","VFD REF",-0.4300f,0.5150f,"mint"),
            ("VFD_A","VFD A",-0.3320f,0.5150f,"mint"),
            ("MOTOR_A","MOTOR A",-0.2340f,0.5150f,"mint"),
            ("VFD_B","VFD B",-0.1360f,0.5150f,"mint"),
            ("MOTOR_B","MOTOR B",-0.0380f,0.5150f,"mint"),
            ("VFD_C","VFD C",0.0600f,0.5150f,"mint"),
            ("MOTOR_C","MOTOR C",0.1580f,0.5150f,"mint"),
            ("VFD_OK","VFD OK",0.2560f,0.5150f,"mint"),
            ("PLC_READY","PLC READY",0.3540f,0.5150f,"mint"),
            ("VFD_FAULT","VFD FAULT",0.4520f,0.5150f,"mint"),
            ("RELAY_IN","RELAY IN",0.5500f,0.5150f,"mint"),
            ("RELAY_OUT","RELAY OUT",0.6480f,0.5150f,"mint"),
            ("PLC_TRIP","PLC TRIP",0.7460f,0.5150f,"amber"),
            ("VFD_REF_MON","VFD REF MON",0.8440f,0.5150f,"mint"),
            ("TWIN_REF","TWIN REF",0.9420f,0.5150f,"mint"),
            ("CT_A","CT A",-1.0200f,-0.3900f,"mint"),
            ("CM_IA","CM IA",-0.9320f,-0.3900f,"mint"),
            ("CT_B","CT B",-0.8440f,-0.3900f,"mint"),
            ("CM_IB","CM IB",-0.7560f,-0.3900f,"mint"),
            ("CT_C","CT C",-0.6680f,-0.3900f,"mint"),
            ("CM_IC","CM IC",-0.5800f,-0.3900f,"mint"),
            ("ENC_PULSE","ENC PULSE",-0.4920f,-0.3900f,"mint"),
            ("CM_RPM","CM RPM",-0.4040f,-0.3900f,"mint"),
            ("PLC_RPM","PLC RPM",-0.3160f,-0.3900f,"mint"),
            ("FFT_SPEED","FFT SPEED",-0.2280f,-0.3900f,"mint"),
            ("BEARING_TEMP","BEARING TEMP",-0.1400f,-0.3900f,"mint"),
            ("CM_BT","CM BT",-0.0520f,-0.3900f,"mint"),
            ("MOTOR_TEMP","MOTOR TEMP",0.0360f,-0.3900f,"mint"),
            ("CM_MT","CM MT",0.1240f,-0.3900f,"mint"),
            ("IR_OUT","IR OUT",0.2120f,-0.3900f,"mint"),
            ("CM_IR","CM IR",0.3000f,-0.3900f,"mint"),
            ("OIL_OUT","OIL OUT",0.3880f,-0.3900f,"mint"),
            ("CM_OIL","CM OIL",0.4760f,-0.3900f,"mint"),
            ("ALIGN_OUT","ALIGN OUT",0.5640f,-0.3900f,"mint"),
            ("CM_ALIGN","CM ALIGN",0.6520f,-0.3900f,"mint"),
            ("GEAR_OUT","GEAR OUT",0.7400f,-0.3900f,"mint"),
            ("CM_GEAR","CM GEAR",0.8280f,-0.3900f,"mint"),
            ("VIB2_OUT","VIB2 OUT",0.9160f,-0.3900f,"mint"),
            ("CM_VIB2","CM VIB2",1.0040f,-0.3900f,"mint"),
            ("VIB3_Y","VIB3 Y",-1.0200f,-0.5450f,"mint"),
            ("FFT_Y","FFT Y",-0.9320f,-0.5450f,"mint"),
            ("VIB3_Z","VIB3 Z",-0.8440f,-0.5450f,"mint"),
            ("FFT_Z","FFT Z",-0.7560f,-0.5450f,"mint"),
            ("BEARING_BT_MON","BEARING BT MON",-0.6680f,-0.5450f,"mint"),
            ("TWIN_BT","TWIN BT",-0.5800f,-0.5450f,"mint"),
            ("MOTOR_MT_MON","MOTOR MT MON",-0.4920f,-0.5450f,"mint"),
            ("TWIN_MT","TWIN MT",-0.4040f,-0.5450f,"mint"),
            ("CT_MON","CT MON",-0.3160f,-0.5450f,"mint"),
            ("TWIN_I","TWIN I",-0.2280f,-0.5450f,"mint"),
            ("ENC_RATE","ENC RATE",-0.1400f,-0.5450f,"mint"),
            ("TWIN_SPEED","TWIN SPEED",-0.0520f,-0.5450f,"mint"),
            ("FFT_OUT","FFT OUT",0.0360f,-0.5450f,"mint"),
            ("CM_FFT","CM FFT",0.1240f,-0.5450f,"mint"),
            ("AI_FFT","AI FFT",0.2120f,-0.5450f,"mint"),
            ("CM_HEALTH","CM HEALTH",0.3000f,-0.5450f,"mint"),
            ("AI_HEALTH","AI HEALTH",0.3880f,-0.5450f,"mint"),
            ("TWIN_HEALTH","TWIN HEALTH",0.4760f,-0.5450f,"mint"),
            ("AI_OUT","AI OUT",0.5640f,-0.5450f,"mint"),
            ("PLC_SERVICE","PLC SERVICE",0.6520f,-0.5450f,"mint"),
            ("TWIN_DIAG","TWIN DIAG",0.7400f,-0.5450f,"mint"),
            ("HMI_SPECTRUM","HMI SPECTRUM",0.8280f,-0.5450f,"mint"),
            ("HMI_HEALTH","HMI HEALTH",0.9160f,-0.5450f,"mint"),
            ("PLC_COOL","PLC COOL",1.0040f,-0.5450f,"mint"),
            ("FAN_IN","FAN IN",-1.0200f,-0.7000f,"mint"),
            ("CONVEYOR_RUN","CONVEYOR RUN",-0.9320f,-0.7000f,"mint"),
            ("PLC_LOCK","PLC LOCK",-0.8440f,-0.7000f,"amber"),
            ("VFD_INHIBIT","VFD INHIBIT",-0.7560f,-0.7000f,"mint"),
            ("LOCK_AUX","LOCK AUX",-0.6680f,-0.7000f,"amber"),
            ("PLC_LOCK_FB","PLC LOCK FB",-0.5800f,-0.7000f,"amber"),
            ("MOUNT_OUT","MOUNT OUT",-0.4920f,-0.7000f,"mint"),
            ("PLC_MOUNT","PLC MOUNT",-0.4040f,-0.7000f,"mint"),
            ("GAUGE_IN","GAUGE IN",-0.3160f,-0.7000f,"mint"),
            ("TORQUE_OUT","TORQUE OUT",-0.2280f,-0.7000f,"mint"),
            ("PLC_TORQUE","PLC TORQUE",-0.1400f,-0.7000f,"mint"),
            ("TOOL_OUT","TOOL OUT",-0.0520f,-0.7000f,"mint"),
            ("PLC_TOOL","PLC TOOL",0.0360f,-0.7000f,"mint"),
            ("GUARD_NC","GUARD NC",0.1240f,-0.7000f,"mint"),
            ("CTRL_SAFE","CTRL SAFE",0.2120f,-0.7000f,"amber"),
            ("PLC_DOOR","PLC DOOR",0.3000f,-0.7000f,"mint"),
            ("ESTOP_NC","ESTOP NC",0.3880f,-0.7000f,"mint"),
            ("PLC_ESTOP","PLC ESTOP",0.4760f,-0.7000f,"mint"),
            ("TWIN_TRIP","TWIN TRIP",0.5640f,-0.7000f,"amber"),
            ("HMI_LOCK","HMI LOCK",0.6520f,-0.7000f,"amber"),
            ("HMI_TRIP","HMI TRIP",0.7400f,-0.7000f,"amber"),
            ("TWIN_SPECTRUM","TWIN SPECTRUM",0.8280f,-0.7000f,"mint"),
            ("VIB3_X","VIB3 X",0.9160f,-0.7000f,"mint"),
            ("FFT_X","FFT X",1.0040f,-0.7000f,"mint"),
        };

        public static IEnumerable<string> TerminalIds => Sockets.Select(socket => socket.id);

        private static readonly Dictionary<string, string> Palette = new()
        {
            {"red","EE485C"},{"blue","1671E2"},{"amber","FFA82B"},{"violet","A282FF"},
            {"mint","34F6C5"},{"cyan","23DAFD"},{"void","07111C"},{"white","D4F5FF"}
        };

        [MenuItem("Tools/Project Spark/Levels/Create Level 66")]
        public static void Install()
        {
            SparkLevel66Builder.Create();
            var root = AssetDatabase.LoadAssetAtPath<GameObject>(Prefab);
            if (root == null || root.GetComponent<SparkLevel66Maintenance>().controllerDisplay == null) Fit();
            Register();
            ValidateAssets();
        }

        [MenuItem("Tools/Project Spark/Level 66/Validate Level Assets")]
        public static void ValidateAssets()
        {
            var root=AssetDatabase.LoadAssetAtPath<GameObject>(Prefab);
            var control=root != null ? root.GetComponent<SparkLevel66Maintenance>() : null;
            var bindings=root != null ? root.GetComponent<SparkLevelSceneBindings>() : null;
            if (control==null || bindings==null || control.controllerDisplay==null || control.qualityDisplay==null || control.rig==null ||
                control.pins.Length!=110 || control.pins.Distinct().Count()!=110 || 
                !TerminalIds.SequenceEqual(control.pins.Select(pin=>pin.name)) ||
                control.pins.Any(pin=>!bindings.TryGetTerminal(pin.name,out var bound) || bound!=pin) ||
                SparkLevel66Maintenance.Gates.Any(id=>!bindings.IsComponentIdDeclared(id)))
                throw new InvalidOperationException("Level 66 wiring, motion or binding references are incomplete.");
            Debug.Log("[LEVEL 66] Validated 110 distinct terminals, 70 player wires and 6 evaluator gates.",root);
        }

        private static void Register()
        {
            var scene = SceneManager.GetSceneByPath(ScenePath);
            bool opened = !scene.isLoaded;
            if (opened) scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
            bool dirty = scene.isDirty;
            var objects = scene.GetRootGameObjects();
            T Find<T>() where T : Component => objects.SelectMany(item => item.GetComponentsInChildren<T>(true)).Single();
            var level = AssetDatabase.LoadAssetAtPath<SparkLevelDefinition>(Folder + "Level_66.asset");
            var drawing = AssetDatabase.LoadAssetAtPath<SparkLevelDrawingAsset>(Folder + "Level_66_Drawing.asset");
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
            else if (dirty) Debug.Log("MainMenu had unsaved edits. Save the scene to persist Level 66's campaign and teaching entries.");
        }

        private static void Append(Object target, string field, Object value)
        {
            if (value == null) throw new InvalidOperationException("Missing Level 66 asset for " + field);
            var data = new SerializedObject(target);
            var list = data.FindProperty(field);
            for (int i = 0; i < list.arraySize; i++)
                if (list.GetArrayElementAtIndex(i).objectReferenceValue == value) return;
            list.arraySize++;
            list.GetArrayElementAtIndex(list.arraySize - 1).objectReferenceValue = value;
            data.ApplyModifiedProperties();
        }

        [MenuItem("Tools/Project Spark/Level 66/Fit Breadboard Assembly")]
        public static void Fit()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Exit Play Mode before fitting Level 66.");
            var models = Models.Select((name, index) =>
            {
                string path = Folder + $"Models/L66_{index:00}_{name}_Fitted.fbx";
                return AssetDatabase.LoadAssetAtPath<GameObject>(path) ??
                    throw new InvalidOperationException("Missing fitted kit module " + path);
            }).ToArray();
            if (AssetDatabase.LoadAssetAtPath<GameObject>(Prefab) == null)
                throw new InvalidOperationException("The Level 66 prefab is missing.");

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
                    throw new InvalidOperationException("Unity could not save the Level 66 layout.");
                AssetDatabase.SaveAssets();
                Debug.Log("Level 66 fitted to the MainMenu breadboard with 110 seated terminals.");
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
            Vector3 right = Vector3.ProjectOnPlane(grid.GridOrigin.right, up).normalized * .066f;
            Vector3 forward = Vector3.ProjectOnPlane(grid.GridOrigin.forward, up).normalized * .066f;
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
            var bench = root.transform.Find("Maintenance Workbench");
            var bindings = root.GetComponent<SparkLevelSceneBindings>();
            var loop = root.GetComponent<SparkLevel66Maintenance>();
            if (bench == null || bindings == null || loop == null)
                throw new InvalidOperationException("Level 66 is missing its workbench, controls, or scene bindings.");

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

            foreach (string name in new[] {"Fitted Level 66 Models", "Breadboard Labels", "Motion pivots"})
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
            var assembly = Child(bench, "Fitted Level 66 Models", Vector3.up * floor);
            foreach (var model in models)
            {
                var instance = Object.Instantiate(model, assembly, false).transform;
                var markers = instance.GetComponentsInChildren<Transform>(true)
                    .Where(item => item.name.StartsWith("L66_Board",StringComparison.Ordinal))
                    .ToDictionary(item => item.name);
                Transform Marker(string name) => markers["L66_Board" + name];
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
                var oldControls = root.transform.Find("Level 66 Controls");
                if (oldControls != null) Object.DestroyImmediate(oldControls.gameObject);
                SparkLevel66Builder.BuildControls(root.transform,loop,font);
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
                terminalData.FindProperty("maxConnections").intValue = SparkLevel66Maintenance.Wires.Count(wire => wire.a == index || wire.b == index);
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






                ("MOTOR / VFD",-.55f,.33f),("DRIVE TRAIN",.48f,.33f),
                ("MAINTENANCE / LOCKOUT",-.55f,-.28f),("CONDITION MONITORING",.57f,-.28f)
            };
            foreach(var title in titles)
                Label(labels,font,title.title,title.title,new Vector3(title.x,floor+.028f,title.z),.018f);
            Transform Pivot(string module,string key)
            {
                var name="fitted_"+module+"_"+key;
                return assembly.GetComponentsInChildren<Transform>(true).Single(item=>item.name==name);
            }
            Transform Module(string name)=>assembly.GetComponentsInChildren<Transform>(true).Single(item=>item.name=="L66_"+name);
            var rig=loop.rig;
            rig.motor=Pivot("IndustrialThreePhaseMotor","ANIM_MOTOR_ROTOR");
            rig.shaft=Pivot("MotorRotorShaft","ANIM_MOTOR_ROTOR");
            rig.cover=Pivot("IndustrialGearboxHousing","ANIM_GEARBOX_COVER");
            rig.gears=Pivot("GearboxGearAssembly","ANIM_GEARBOX_GEARS");
            rig.coupling=Pivot("DriveShaftCoupling","ANIM_SHAFT_COUPLING");
            rig.roller=Pivot("ConveyorDriveRoller","ANIM_CONVEYOR_ROLLER");
            rig.encoder=Pivot("PrecisionShaftEncoder","ANIM_ENCODER_DISC");
            rig.monitor=Pivot("IndustrialDigitalTwinDisplay","ANIM_TWIN_MONITOR");
            rig.tool=Pivot("BearingRemovalTool","ANIM_BEARING_EXTRACTOR");
            rig.fans=Pivot("ActiveCoolingAssembly","ANIM_COOLING_FANS");
            rig.stop=Pivot("EmergencyStopAndLockoutPanel","ANIM_EMERGENCY_STOP");
            rig.guard=Pivot("TransparentMachineSafetyEnclosure","ANIM_SAFETY_DOOR");
            rig.original=Module("DriveEndBearingCartridge");
            rig.damaged=Module("DamagedBearingCartridge");
            rig.replacement=Module("HealthyReplacementBearing");

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
            label.fontSize = 66;
            label.characterSize = size;
            label.anchor = TextAnchor.MiddleCenter;
            label.color = new Color(.89f, .96f, 1f);
            label.text = text;
            child.GetComponent<MeshRenderer>().sharedMaterial = font.material;
            return label;
        }

        private static Material MaterialFor(Shader shader, string key)
        {
            string path = $"{Materials}/L66 {key}.mat";
            if (key.StartsWith("tint",StringComparison.Ordinal) && key.Length==10) Palette[key]=key.Substring(4);
            var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing != null && key is not ("glass" or "acrylic") &&
                ColorUtility.TryParseHtmlString("#" + (Palette.TryGetValue(key, out var savedTint) ? savedTint : "9FB3C8"), out var savedColor) &&
                existing.GetColor("_BaseColor") == savedColor) return existing;
            ColorUtility.TryParseHtmlString("#" + (Palette.TryGetValue(key, out var tint) ? tint : "9FB3C8"), out var color);
            var material = existing != null ? existing : new Material(shader) { name = "L66 " + key };
            bool metal = key is "chrome" or "titanium" or "steel" or "gold" or "copper" or "aluminum";
            material.SetFloat("_Metallic", metal ? .66f : .02f);
            material.SetFloat("_Smoothness", metal ? .66f : key == "glass" ? .12f : .35f);
            if (key is "green" or "cyan" or "red" or "blue" or "orange" or "amber" or "mint" or "warm" or "ice")
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", color * .66f);
            }
            if (key is "glass" or "glass_body" or "glass_soft" or "acrylic" or "glassblue" or "film" or "Glass" or "guardglass")
            {
                color.a = key=="film" ? .66f : .10f;
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





