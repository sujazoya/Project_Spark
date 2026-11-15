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
    public static class SparkLevel67ModelInstaller
    {
        public const string Folder = "Assets/My_Assets/_Project_Spark/Prefabs/Levels/Level_67_Layout/";
        public const string Prefab = Folder + "Level_67_Layout.prefab";
        public const string ScenePath = "Assets/My_Assets/_Project_Spark/Scenes/MainMenu/MainMenu.unity";
        private static readonly string[] Models = {
            "IndustrialProcessControlBase","TransparentSupplyTankA","TransparentSupplyTankB","CentralMixingVessel","FinishedProductTank","CentrifugalPumpA","CentrifugalPumpB","ProductTransferPump","PumpMotorA","PumpMotorB","TransferPumpMotor","VariableFrequencyDriveBank","ThreeChannelFlowMeterAssembly","FlowTransmitterA","FlowTransmitterB","ProductFlowTransmitter","UltrasonicLevelSensorA","UltrasonicLevelSensorB","ProductTankLevelSensor","PressureTransmitterA","PressureTransmitterB","MotorizedControlValveA","MotorizedControlValveB","ProductOutletControlValve","EmergencyIsolationValve","TransparentProcessPipelineNetwork","IndustrialMixingImpeller","MixingMotorDrive","LiquidRatioQualitySensor","CascadePIDController","FlowRatioController","TankLevelController","PumpProtectionRelay","CavitationDetectionSensor","ProcessSafetyPLC","IndustrialProcessPLCRack","ProcessDigitalIOModule","DownstreamDemandSimulator","HolographicSCADAMonitor","EmergencyStopAndAlarmStation","TransparentProcessSafetyEnclosure","Regulated5VControlSupply"
        };
        private const string Materials = Folder + "Models/Materials";
        private const float SocketHeight = .025f;
        private const float SocketDiameter = .026f;

        private static readonly (string id, string label, float x, float z, string color)[] Sockets =
        {
            ("SUPPLY_POS","SUP POS",-0.8200f,0.6800f,"red"),
            ("PLC_5V","PLC 5V",-0.7180f,0.6800f,"red"),
            ("SUPPLY_RET","SUP RET",-0.6160f,0.6800f,"blue"),
            ("PLC_GND","PLC GND",-0.5140f,0.6800f,"blue"),
            ("IO_5V","IO 5V",-0.4120f,0.6800f,"red"),
            ("IO_GND","IO GND",-0.3100f,0.6800f,"blue"),
            ("SAFE_5V","SAFE 5V",-0.2080f,0.6800f,"red"),
            ("SAFE_GND","SAFE GND",-0.1060f,0.6800f,"blue"),
            ("FLOW_A_5V","F A 5V",-0.0040f,0.6800f,"red"),
            ("FLOW_A_GND","F A GND",0.0980f,0.6800f,"blue"),
            ("FLOW_B_5V","F B 5V",0.2000f,0.6800f,"red"),
            ("FLOW_B_GND","F B GND",0.3020f,0.6800f,"blue"),
            ("FLOW_P_5V","F P 5V",0.4040f,0.6800f,"red"),
            ("FLOW_P_GND","F P GND",0.5060f,0.6800f,"blue"),
            ("PID_5V","PID 5V",0.6080f,0.6800f,"red"),
            ("PID_GND","PID GND",0.7100f,0.6800f,"blue"),
            ("DRIVE_5V","DRV 5V",0.8120f,0.6800f,"red"),
            ("DRIVE_GND","DRV GND",0.9140f,0.6800f,"blue"),
            ("QUALITY_5V","QA 5V",-0.9600f,0.5150f,"red"),
            ("QUALITY_GND","QA GND",-0.8700f,0.5150f,"blue"),
            ("PLC_RUN_A","PLC RUN A",-0.7800f,0.5150f,"mint"),
            ("DRIVE_A_EN","DRV A EN",-0.6900f,0.5150f,"mint"),
            ("PLC_RUN_B","PLC RUN B",-0.6000f,0.5150f,"mint"),
            ("DRIVE_B_EN","DRV B EN",-0.5100f,0.5150f,"mint"),
            ("PLC_RUN_P","PLC RUN P",-0.4200f,0.5150f,"mint"),
            ("DRIVE_P_EN","DRV P EN",-0.3300f,0.5150f,"mint"),
            ("PID_REF_A","PID REF A",-0.2400f,0.5150f,"mint"),
            ("DRIVE_A_CMD","DRV A CMD",-0.1500f,0.5150f,"mint"),
            ("PID_REF_B","PID REF B",-0.0600f,0.5150f,"mint"),
            ("DRIVE_B_CMD","DRV B CMD",0.0300f,0.5150f,"mint"),
            ("PID_REF_P","PID REF P",0.1200f,0.5150f,"mint"),
            ("DRIVE_P_CMD","DRV P CMD",0.2100f,0.5150f,"mint"),
            ("DRIVE_A_TACH","DRV A TACH",0.3000f,0.5150f,"mint"),
            ("IO_RPM_A","IO RPM A",0.3900f,0.5150f,"mint"),
            ("DRIVE_B_TACH","DRV B TACH",0.4800f,0.5150f,"mint"),
            ("IO_RPM_B","IO RPM B",0.5700f,0.5150f,"mint"),
            ("DRIVE_P_TACH","DRV P TACH",0.6600f,0.5150f,"mint"),
            ("IO_RPM_P","IO RPM P",0.7500f,0.5150f,"mint"),
            ("FLOW_A_OUT","F A OUT",0.8400f,0.5150f,"mint"),
            ("RATIO_A_IN","RATIO A IN",0.9300f,0.5150f,"mint"),
            ("FLOW_P_OUT","F P OUT",-1.0200f,-0.3500f,"mint"),
            ("PID_FLOW_IN","PID F IN",-0.9340f,-0.3500f,"mint"),
            ("FLOW_OK","F OK",-0.8480f,-0.3500f,"mint"),
            ("PLC_FLOW_OK","PLC F OK",-0.7620f,-0.3500f,"mint"),
            ("LEVEL_A_OUT","LVL A OUT",-0.6760f,-0.3500f,"mint"),
            ("IO_LEVEL_A","IO LVL A",-0.5900f,-0.3500f,"mint"),
            ("LEVEL_B_OUT","LVL B OUT",-0.5040f,-0.3500f,"mint"),
            ("IO_LEVEL_B","IO LVL B",-0.4180f,-0.3500f,"mint"),
            ("LEVEL_P_OUT","LVL P OUT",-0.3320f,-0.3500f,"mint"),
            ("LEVEL_PID_IN","LVL PID IN",-0.2460f,-0.3500f,"mint"),
            ("SUCTION_KPA","SUCTION KPA",-0.1600f,-0.3500f,"mint"),
            ("IO_SUCTION","IO SUCTION",-0.0740f,-0.3500f,"mint"),
            ("DISCHARGE_KPA","DISCH KPA",0.0120f,-0.3500f,"mint"),
            ("IO_DISCHARGE","IO DISCH",0.0980f,-0.3500f,"mint"),
            ("PLC_VALVE_A","PLC VALVE A",0.1840f,-0.3500f,"mint"),
            ("VALVE_A_CMD","VALVE A CMD",0.2700f,-0.3500f,"mint"),
            ("VALVE_A_POS","VALVE A POS",0.3560f,-0.3500f,"mint"),
            ("IO_POS_A","IO POS A",0.4420f,-0.3500f,"mint"),
            ("PLC_VALVE_B","PLC VALVE B",0.5280f,-0.3500f,"mint"),
            ("VALVE_B_CMD","VALVE B CMD",0.6140f,-0.3500f,"mint"),
            ("VALVE_B_POS","VALVE B POS",0.7000f,-0.3500f,"mint"),
            ("IO_POS_B","IO POS B",0.7860f,-0.3500f,"mint"),
            ("PLC_OUTLET","PLC OUT",0.8720f,-0.3500f,"mint"),
            ("OUTLET_CMD","OUT CMD",0.9580f,-0.3500f,"mint"),
            ("OUTLET_POS","OUT POS",1.0440f,-0.3500f,"mint"),
            ("IO_OUTLET","IO OUT",-1.0200f,-0.5250f,"mint"),
            ("SAFE_ISOLATE","SAFE ISOLATE",-0.9340f,-0.5250f,"amber"),
            ("XV_CMD","XV CMD",-0.8480f,-0.5250f,"mint"),
            ("XV_POS","XV POS",-0.7620f,-0.5250f,"mint"),
            ("SAFE_XV","SAFE XV",-0.6760f,-0.5250f,"amber"),
            ("PLC_MIX","PLC MIX",-0.5900f,-0.5250f,"mint"),
            ("MIX_EN","MIX EN",-0.5040f,-0.5250f,"mint"),
            ("MIX_RPM","MIX RPM",-0.4180f,-0.5250f,"mint"),
            ("IO_MIX","IO MIX",-0.3320f,-0.5250f,"mint"),
            ("QA_BLEND","QA BLEND",-0.2460f,-0.5250f,"mint"),
            ("RATIO_QUALITY","RATIO QA",-0.1600f,-0.5250f,"mint"),
            ("QA_PASS","QA PASS",-0.0740f,-0.5250f,"mint"),
            ("SAFE_RELEASE","SAFE RELEASE",0.0120f,-0.5250f,"amber"),
            ("LEVEL_FLOW_SP","LVL F SP",0.0980f,-0.5250f,"mint"),
            ("PID_SP","PID SP",0.1840f,-0.5250f,"mint"),
            ("RATIO_CMD","RATIO CMD",0.2700f,-0.5250f,"mint"),
            ("PLC_RATIO","PLC RATIO",0.3560f,-0.5250f,"mint"),
            ("PLC_READY","PLC READY",0.4420f,-0.5250f,"mint"),
            ("SCADA_READY","HMI READY",0.5280f,-0.5250f,"mint"),
            ("ESTOP_NC","ESTOP NC",0.6140f,-0.5250f,"amber"),
            ("SAFE_STOP_IN","SAFE STOP IN",0.7000f,-0.5250f,"amber"),
            ("CAVITATION","CAV",0.7860f,-0.5250f,"amber"),
            ("PROTECT_CAV","PRT CAV",0.8720f,-0.5250f,"amber"),
            ("PROTECT_TRIP","PRT TRIP",0.9580f,-0.5250f,"amber"),
            ("SAFE_TRIP","SAFE TRIP",1.0440f,-0.5250f,"amber"),
            ("HIGH_LEVEL","HIGH LEVEL",-1.0200f,-0.7000f,"mint"),
            ("SAFE_HIGH","SAFE HIGH",-0.9380f,-0.7000f,"amber"),
            ("LOW_LEVEL","LOW LEVEL",-0.8560f,-0.7000f,"mint"),
            ("SAFE_LOW","SAFE LOW",-0.7740f,-0.7000f,"amber"),
            ("PLC_RESET","PLC RESET",-0.6920f,-0.7000f,"mint"),
            ("SAFE_RESET","SAFE RESET",-0.6100f,-0.7000f,"amber"),
            ("DEMAND_TEST","DEMAND TEST",-0.5280f,-0.7000f,"mint"),
            ("IO_DEMAND","IO DEMAND",-0.4460f,-0.7000f,"mint"),
            ("SCADA_RPM_A","HMI RPM A",-0.3640f,-0.7000f,"mint"),
            ("SCADA_RPM_B","HMI RPM B",-0.2820f,-0.7000f,"mint"),
            ("SCADA_RPM_P","HMI RPM P",-0.2000f,-0.7000f,"mint"),
            ("SCADA_FLOW_A","HMI F A",-0.1180f,-0.7000f,"mint"),
            ("SCADA_FLOW_P","HMI F P",-0.0360f,-0.7000f,"mint"),
            ("SCADA_VOL_A","HMI VOL A",0.0460f,-0.7000f,"mint"),
            ("SCADA_VOL_B","HMI VOL B",0.1280f,-0.7000f,"mint"),
            ("SCADA_VOL_P","HMI VOL P",0.2100f,-0.7000f,"mint"),
            ("SCADA_SUCTION","HMI SUCTION",0.2920f,-0.7000f,"mint"),
            ("SCADA_HEAD","HMI HEAD",0.3740f,-0.7000f,"mint"),
            ("SCADA_BLEND","HMI BLEND",0.4560f,-0.7000f,"mint"),
            ("SCADA_TRIP","HMI TRIP",0.5380f,-0.7000f,"amber"),
            ("PID_PERMIT","PID PERMIT",0.6200f,-0.7000f,"mint"),
            ("IO_READY","IO READY",0.7020f,-0.7000f,"mint"),
            ("TRANSFER_INHIBIT","XFER INHIBIT",0.7840f,-0.7000f,"mint"),
            ("SCADA_FLOW_SP","HMI F SP",0.8660f,-0.7000f,"mint"),
            ("FLOW_B_OUT","F B OUT",0.9480f,-0.7000f,"mint"),
            ("RATIO_B_IN","RATIO B IN",1.0300f,-0.7000f,"mint"),
        };

        public static IEnumerable<string> TerminalIds => Sockets.Select(socket => socket.id);

        private static readonly Dictionary<string, string> Palette = new()
        {
            {"red","EE485C"},{"blue","1671E2"},{"amber","FFA82B"},{"violet","A282FF"},
            {"mint","34F6C5"},{"cyan","23DAFD"},{"void","07111C"},{"white","D4F5FF"},{"glass","498BA8"}
        };

        [MenuItem("Tools/Project Spark/Levels/Create Level 67")]
        public static void Install()
        {
            SparkLevel67Builder.Create();
            var root = AssetDatabase.LoadAssetAtPath<GameObject>(Prefab);
            if (root == null || root.GetComponent<SparkLevel67Process>().controllerDisplay == null) Fit();
            Register();
            ValidateAssets();
        }

        [MenuItem("Tools/Project Spark/Level 67/Validate Level Assets")]
        public static void ValidateAssets()
        {
            var root=AssetDatabase.LoadAssetAtPath<GameObject>(Prefab);
            var control=root != null ? root.GetComponent<SparkLevel67Process>() : null;
            var bindings=root != null ? root.GetComponent<SparkLevelSceneBindings>() : null;
            if (control==null || bindings==null || control.controllerDisplay==null || control.qualityDisplay==null || control.rig==null || control.rig.pivots.Length!=14 ||
                control.pins.Length!=116 || control.pins.Distinct().Count()!=116 || 
                !TerminalIds.SequenceEqual(control.pins.Select(pin=>pin.name)) ||
                control.pins.Any(pin=>!bindings.TryGetTerminal(pin.name,out var bound) || bound!=pin) ||
                SparkLevel67Process.Gates.Any(id=>!bindings.IsComponentIdDeclared(id)))
                throw new InvalidOperationException("Level 67 wiring, motion or binding references are incomplete.");
            Debug.Log("[LEVEL 67] Validated 116 distinct terminals, 74 player wires and 6 evaluator gates.",root);
        }

        private static void Register()
        {
            var scene = SceneManager.GetSceneByPath(ScenePath);
            bool opened = !scene.isLoaded;
            if (opened) scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
            bool dirty = scene.isDirty;
            var objects = scene.GetRootGameObjects();
            T Find<T>() where T : Component => objects.SelectMany(item => item.GetComponentsInChildren<T>(true)).Single();
            var level = AssetDatabase.LoadAssetAtPath<SparkLevelDefinition>(Folder + "Level_67.asset");
            var drawing = AssetDatabase.LoadAssetAtPath<SparkLevelDrawingAsset>(Folder + "Level_67_Drawing.asset");
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
            else if (dirty) Debug.Log("MainMenu had unsaved edits. Save the scene to persist Level 67's campaign and teaching entries.");
        }

        private static void Append(Object target, string field, Object value)
        {
            if (value == null) throw new InvalidOperationException("Missing Level 67 asset for " + field);
            var data = new SerializedObject(target);
            var list = data.FindProperty(field);
            for (int i = 0; i < list.arraySize; i++)
                if (list.GetArrayElementAtIndex(i).objectReferenceValue == value) return;
            list.arraySize++;
            list.GetArrayElementAtIndex(list.arraySize - 1).objectReferenceValue = value;
            data.ApplyModifiedProperties();
        }

        [MenuItem("Tools/Project Spark/Level 67/Fit Breadboard Assembly")]
        public static void Fit()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Exit Play Mode before fitting Level 67.");
            var models = Models.Select((name, index) =>
            {
                string path = Folder + $"Models/L67_{index:00}_{name}_Fitted.fbx";
                return AssetDatabase.LoadAssetAtPath<GameObject>(path) ??
                    throw new InvalidOperationException("Missing fitted kit module " + path);
            }).ToArray();
            if (AssetDatabase.LoadAssetAtPath<GameObject>(Prefab) == null)
                throw new InvalidOperationException("The Level 67 prefab is missing.");

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
                    throw new InvalidOperationException("Unity could not save the Level 67 layout.");
                AssetDatabase.SaveAssets();
                Debug.Log("Level 67 fitted to the MainMenu breadboard with 116 seated terminals.");
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
            Vector3 right = Vector3.ProjectOnPlane(grid.GridOrigin.right, up).normalized * .067f;
            Vector3 forward = Vector3.ProjectOnPlane(grid.GridOrigin.forward, up).normalized * .067f;
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
            var bench = root.transform.Find("Process Workbench");
            var bindings = root.GetComponent<SparkLevelSceneBindings>();
            var loop = root.GetComponent<SparkLevel67Process>();
            if (bench == null || bindings == null || loop == null)
                throw new InvalidOperationException("Level 67 is missing its workbench, controls, or scene bindings.");

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

            foreach (string name in new[] {"Fitted Level 67 Models", "Breadboard Labels", "Motion pivots"})
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
            var assembly = Child(bench, "Fitted Level 67 Models", new Vector3(0,floor,.035f));
            foreach (var model in models)
            {
                var instance = Object.Instantiate(model, assembly, false).transform;
                var markers = instance.GetComponentsInChildren<Transform>(true)
                    .Where(item => item.name.StartsWith("L67_Board",StringComparison.Ordinal))
                    .ToDictionary(item => item.name);
                Transform Marker(string name) => markers["L67_Board" + name];
                Vector3 Axis(string name) => assembly.InverseTransformVector(Marker(name).position - Marker("Center").position);
                instance.localRotation = Quaternion.Inverse(Quaternion.LookRotation(-Axis("Front"), Axis("Up"))) * instance.localRotation;
                instance.localScale /= Axis("Right").magnitude;
                instance.localPosition -= assembly.InverseTransformPoint(Marker("Center").position);
                if (Axis("Right").x < .99f)
                    throw new InvalidOperationException("Unexpected axes on fitted model " + model.name);
            }

            assembly.localScale = Vector3.one * .84f;
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
                var oldControls = root.transform.Find("Level 67 Controls");
                if (oldControls != null) Object.DestroyImmediate(oldControls.gameObject);
                SparkLevel67Builder.BuildControls(root.transform,loop,font);
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
                terminalData.FindProperty("maxConnections").intValue = SparkLevel67Process.Wires.Count(wire => wire.a == index || wire.b == index);
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






                ("SUPPLY A / B",-.55f,.365f),("MIX / PRODUCT",.49f,.365f),
                ("MEASUREMENTS",-.56f,-.24f),("PROCESS SAFETY",.57f,-.24f)
            };
            foreach(var title in titles)
                Label(labels,font,title.title,title.title,new Vector3(title.x,floor+.028f,title.z),.018f);
            Transform Pivot(string module,string key)
            {
                var name="fitted_"+module+"_"+key;
                return assembly.GetComponentsInChildren<Transform>(true).Single(item=>item.name==name);
            }
            loop.rig.pivots=new[]{
                Pivot("CentrifugalPumpA","ANIM_PUMP_A_IMPELLER"),
                Pivot("CentrifugalPumpB","ANIM_PUMP_B_IMPELLER"),
                Pivot("ProductTransferPump","ANIM_TRANSFER_PUMP"),
                Pivot("PumpMotorA","ANIM_MOTOR_A"),
                Pivot("PumpMotorB","ANIM_MOTOR_B"),
                Pivot("TransferPumpMotor","ANIM_MOTOR_TRANSFER"),
                Pivot("MotorizedControlValveA","ANIM_VALVE_A"),
                Pivot("MotorizedControlValveB","ANIM_VALVE_B"),
                Pivot("ProductOutletControlValve","ANIM_OUTLET_VALVE"),
                Pivot("EmergencyIsolationValve","ANIM_ISOLATION_VALVE"),
                Pivot("IndustrialMixingImpeller","ANIM_MIXER_SHAFT"),
                Pivot("HolographicSCADAMonitor","ANIM_SCADA_MONITOR"),
                Pivot("EmergencyStopAndAlarmStation","ANIM_ESTOP"),
                Pivot("TransparentProcessSafetyEnclosure","ANIM_SAFETY_DOOR"),
            };
            var liquidMesh=new List<Transform>();
            var tanks=new (string name,float x,float z,float bottom,float radius,float height,string material)[]{
                ("Supply A",-4.55f,-.95f,1.65f,.705f,2.42f,"tint1D92FB"),
                ("Supply B",-2.05f,-.95f,1.65f,.705f,2.42f,"tintFB9023"),
                ("Mix",.45f,-.95f,1.40f,.8366f,1.73f,"tint24A6CC"),
                ("Product",3.10f,-.95f,1.54f,.7614f,2.14f,"tint24A6CC")
            };
            foreach(var tank in tanks)
            {
                var vessel=Child(assembly,tank.name+" fluid",new Vector3(tank.x,tank.bottom-.18f,-tank.z)*.122f);
                var liquid=GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                liquid.name="Live liquid"; liquid.transform.SetParent(vessel,false);
                Object.DestroyImmediate(liquid.GetComponent<Collider>());
                liquid.GetComponent<Renderer>().sharedMaterial=MaterialFor(shader,tank.material);
                liquid.transform.localPosition=Vector3.up*(tank.height*.122f*.5f);
                liquid.transform.localScale=new Vector3(tank.radius*2,tank.height*.5f,tank.radius*2)*.122f;
                liquidMesh.Add(liquid.transform);
            }
            loop.rig.liquid=liquidMesh.ToArray();
            if(loop.plot.transform.Find("Trend legend") == null)
            {
                var oldPanel = root.transform.Find("Level 67 Controls");
                if (oldPanel != null) Object.DestroyImmediate(oldPanel.gameObject);
                SparkLevel67Builder.BuildControls(root.transform,loop,font);
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
            loop.controllerDisplay=Readout("Process flow",new Vector3(-.53f,floor+.15f,.386f),new Color(.46f,.94f,1f));
            loop.qualityDisplay=Readout("Process quality",new Vector3(.53f,floor+.15f,.386f),new Color(.68f,1f,.86f));
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
            label.fontSize = 67;
            label.characterSize = size;
            label.anchor = TextAnchor.MiddleCenter;
            label.color = new Color(.89f, .96f, 1f);
            label.text = text;
            child.GetComponent<MeshRenderer>().sharedMaterial = font.material;
            return label;
        }

        private static Material MaterialFor(Shader shader, string key)
        {
            string path = $"{Materials}/L67 {key}.mat";
            if (key.StartsWith("tint",StringComparison.Ordinal) && key.Length==10) Palette[key]=key.Substring(4);
            var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing != null && key is not ("glass" or "acrylic") &&
                ColorUtility.TryParseHtmlString("#" + (Palette.TryGetValue(key, out var savedTint) ? savedTint : "9FB3C8"), out var savedColor) &&
                existing.GetColor("_BaseColor") == savedColor) return existing;
            ColorUtility.TryParseHtmlString("#" + (Palette.TryGetValue(key, out var tint) ? tint : "9FB3C8"), out var color);
            var material = existing != null ? existing : new Material(shader) { name = "L67 " + key };
            bool metal = key is "chrome" or "titanium" or "steel" or "gold" or "copper" or "aluminum" or "tintCFDCE2" or "tintA4B8C4" or "tintE36C27" or "tintDC9C2F";
            material.SetFloat("_Metallic", metal ? .67f : .02f);
            material.SetFloat("_Smoothness", metal ? .67f : key == "glass" ? .12f : .35f);
            if (key is "green" or "cyan" or "red" or "blue" or "orange" or "amber" or "mint" or "warm" or "ice")
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", color * .67f);
            }
            else if (key.StartsWith("tint", StringComparison.Ordinal))
            {
                material.EnableKeyword("_EMISSION");
                float strength = key is "tint19CCEF" or "tint27D680" or "tintFFCE34" ? .30f : .12f;
                material.SetColor("_EmissionColor", color * strength);
            }
            if (key is "glass" or "glass_body" or "glass_soft" or "acrylic" or "glassblue" or "film" or "Glass" or "guardglass")
            {
                color.a = key=="film" ? .67f : .10f;
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




