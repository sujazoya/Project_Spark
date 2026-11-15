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
    public static class SparkLevel68ModelInstaller
    {
        public const string Folder = "Assets/My_Assets/_Project_Spark/Prefabs/Levels/Level_68_Layout/";
        public const string Prefab = Folder + "Level_68_Layout.prefab";
        public const string ScenePath = "Assets/My_Assets/_Project_Spark/Scenes/MainMenu/MainMenu.unity";
        private static readonly string[] Models = {
            "IndustrialThermalProcessBase","RegenerativePlateHeatExchanger","HeatExchangerPlateStack","SecondaryTrimHeatExchanger","InsulatedHeatingTank","ColdWaterReservoir","ThermalBufferTank","ProductHoldingChamber","FinishedProductOutletModule","HotWaterCirculationPump","ColdWaterCirculationPump","ProductTransferPump","HotLoopPumpMotor","ColdLoopPumpMotor","ProductPumpMotor","ElectricHeatingElementBank","HeatingPowerController","VariableFrequencyDriveBank","HotWaterControlValve","CoolingWaterControlValve","ThreeWayMixingValve","RegenerativeBypassValve","ProductIsolationValve","TransparentThermalPipelineNetwork","ProductInletTemperatureSensor","ProductOutletRTDTemperatureSensor","HotWaterTemperatureSensor","ColdWaterTemperatureSensor","ThermalBufferTemperatureSensor","ThermalFlowMeterBank","DifferentialPressureTransmitter","HeatFluxMeasurementModule","RegenerativeEfficiencyAnalyzer","CascadeTemperaturePIDController","ThermalEnergyManagementController","HeatExchangerFoulingSimulator","InletTemperatureDisturbanceModule","OvertemperatureProtectionRelay","IndustrialThermalPLC","ThermalDigitalIOModule","HolographicThermalSCADAMonitor","EmergencyThermalShutdownStation","TransparentThermalSafetyEnclosure","Regulated5VControlSupply"
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
            ("RTD_5V","RTD 5V",-0.0040f,0.6800f,"red"),
            ("RTD_GND","RTD GND",0.0980f,0.6800f,"blue"),
            ("PID_5V","PID 5V",0.2000f,0.6800f,"red"),
            ("PID_GND","PID GND",0.3020f,0.6800f,"blue"),
            ("ENERGY_5V","ENG 5V",0.4040f,0.6800f,"red"),
            ("ENERGY_GND","ENG GND",0.5060f,0.6800f,"blue"),
            ("DRIVE_5V","DRIVE 5V",0.6080f,0.6800f,"red"),
            ("DRIVE_GND","DRIVE GND",0.7100f,0.6800f,"blue"),
            ("HEATER_5V","HEATER 5V",0.8120f,0.6800f,"red"),
            ("HEATER_GND","HEATER GND",0.9140f,0.6800f,"blue"),
            ("METER_5V","MTR 5V",-0.9800f,0.5150f,"red"),
            ("METER_GND","MTR GND",-0.8950f,0.5150f,"blue"),
            ("PLC_H_RUN","H RUN",-0.8100f,0.5150f,"mint"),
            ("HOT_EN","HOT EN",-0.7250f,0.5150f,"mint"),
            ("PLC_H_REF","PLC H REF",-0.6400f,0.5150f,"mint"),
            ("HOT_CMD","HOT CMD",-0.5550f,0.5150f,"mint"),
            ("HOT_RPM","HOT RPM",-0.4700f,0.5150f,"mint"),
            ("IO_H_RPM","IO H RPM",-0.3850f,0.5150f,"mint"),
            ("HOT_STATUS","HOT STATUS",-0.3000f,0.5150f,"mint"),
            ("PLC_H_OK","PLC H OK",-0.2150f,0.5150f,"mint"),
            ("HOT_TRIP","HOT TRIP",-0.1300f,0.5150f,"amber"),
            ("SAFE_H_TRIP","SAFE H TRIP",-0.0450f,0.5150f,"amber"),
            ("PLC_C_RUN","PLC C RUN",0.0400f,0.5150f,"mint"),
            ("COLD_EN","COLD EN",0.1250f,0.5150f,"mint"),
            ("PLC_C_REF","PLC C REF",0.2100f,0.5150f,"mint"),
            ("COLD_CMD","COLD CMD",0.2950f,0.5150f,"mint"),
            ("COLD_RPM","COLD RPM",0.3800f,0.5150f,"mint"),
            ("IO_C_RPM","IO C RPM",0.4650f,0.5150f,"mint"),
            ("COLD_TRIP","COLD TRIP",0.5500f,0.5150f,"amber"),
            ("SAFE_C_TRIP","SAFE C TRIP",0.6350f,0.5150f,"amber"),
            ("PLC_P_RUN","PLC P RUN",0.7200f,0.5150f,"mint"),
            ("PROD_EN","PROD EN",0.8050f,0.5150f,"mint"),
            ("PLC_P_REF","PLC P REF",0.8900f,0.5150f,"mint"),
            ("PROD_CMD","PROD CMD",0.9750f,0.5150f,"mint"),
            ("PROD_RPM","PROD RPM",-1.0200f,-0.3500f,"mint"),
            ("IO_P_RPM","IO P RPM",-0.9400f,-0.3500f,"mint"),
            ("PROD_STATUS","PROD STATUS",-0.8600f,-0.3500f,"mint"),
            ("PLC_P_OK","PLC P OK",-0.7800f,-0.3500f,"mint"),
            ("SAFE_HEAT","SAFE HEAT",-0.7000f,-0.3500f,"amber"),
            ("HEAT_EN","HEAT EN",-0.6200f,-0.3500f,"mint"),
            ("PID_HEAT","PID HEAT",-0.5400f,-0.3500f,"mint"),
            ("HEAT_CMD","HEAT CMD",-0.4600f,-0.3500f,"mint"),
            ("HEAT_KW","HEAT KW",-0.3800f,-0.3500f,"mint"),
            ("IO_HEAT_KW","IO HEAT KW",-0.3000f,-0.3500f,"mint"),
            ("HEAT_LIMIT","HEAT LIMIT",-0.2200f,-0.3500f,"amber"),
            ("SAFE_LIMIT","SAFE LIMIT",-0.1400f,-0.3500f,"amber"),
            ("HEAT_TRIP","HEAT TRIP",-0.0600f,-0.3500f,"amber"),
            ("SAFE_HEAT_TRIP","SAFE HEAT TRIP",0.0200f,-0.3500f,"amber"),
            ("INLET_RTD","INLET RTD",0.1000f,-0.3500f,"mint"),
            ("PID_INLET","PID INLET",0.1800f,-0.3500f,"mint"),
            ("HOT_RTD","HOT RTD",0.2600f,-0.3500f,"mint"),
            ("PID_HOT","PID HOT",0.3400f,-0.3500f,"mint"),
            ("COLD_RTD","COLD RTD",0.4200f,-0.3500f,"mint"),
            ("IO_COLD","IO COLD",0.5000f,-0.3500f,"mint"),
            ("BUFFER_RTD","BUF RTD",0.5800f,-0.3500f,"mint"),
            ("ENERGY_BUF","ENG BUF",0.6600f,-0.3500f,"mint"),
            ("RTD_OK","RTD OK",0.7400f,-0.3500f,"mint"),
            ("PLC_RTD_OK","PLC RTD OK",0.8200f,-0.3500f,"mint"),
            ("PLC_H_VALVE","PLC H VLV",0.9000f,-0.3500f,"mint"),
            ("HV_CMD","HV CMD",0.9800f,-0.3500f,"mint"),
            ("HV_POS","HV POS",-1.0200f,-0.5250f,"mint"),
            ("IO_H_VALVE","IO H VLV",-0.9400f,-0.5250f,"mint"),
            ("PLC_C_VALVE","PLC C VLV",-0.8600f,-0.5250f,"mint"),
            ("CV_CMD","CV CMD",-0.7800f,-0.5250f,"mint"),
            ("CV_POS","CV POS",-0.7000f,-0.5250f,"mint"),
            ("IO_C_VALVE","IO C VLV",-0.6200f,-0.5250f,"mint"),
            ("PLC_MIX","PLC MIX",-0.5400f,-0.5250f,"mint"),
            ("MIX_CMD","MIX CMD",-0.4600f,-0.5250f,"mint"),
            ("MIX_POS","MIX POS",-0.3800f,-0.5250f,"mint"),
            ("IO_MIX","IO MIX",-0.3000f,-0.5250f,"mint"),
            ("PLC_BYPASS","PLC BYPASS",-0.2200f,-0.5250f,"mint"),
            ("BYP_CMD","BYP CMD",-0.1400f,-0.5250f,"mint"),
            ("BYP_POS","BYP POS",-0.0600f,-0.5250f,"mint"),
            ("IO_BYPASS","IO BYPASS",0.0200f,-0.5250f,"mint"),
            ("SAFE_ISO","SAFE ISO",0.1000f,-0.5250f,"amber"),
            ("ISO_CMD","ISO CMD",0.1800f,-0.5250f,"mint"),
            ("ISO_POS","ISO POS",0.2600f,-0.5250f,"mint"),
            ("SAFE_ISO_FB","ISO FB",0.3400f,-0.5250f,"amber"),
            ("FLOW_P","FLOW P",0.4200f,-0.5250f,"mint"),
            ("PID_P_FLOW","PID P FLOW",0.5000f,-0.5250f,"mint"),
            ("FLOW_H","FLOW H",0.5800f,-0.5250f,"mint"),
            ("SAFE_H_FLOW","SAFE H FLOW",0.6600f,-0.5250f,"amber"),
            ("FLOW_C","FLOW C",0.7400f,-0.5250f,"mint"),
            ("SAFE_C_FLOW","SAFE C FLOW",0.8200f,-0.5250f,"amber"),
            ("HX_DP","HX DP",0.9000f,-0.5250f,"mint"),
            ("ENERGY_DP","ENG DP",0.9800f,-0.5250f,"mint"),
            ("HX_FLUX","HX FLUX",-1.0050f,-0.7000f,"mint"),
            ("ENERGY_FLUX","ENG FLUX",-0.9320f,-0.7000f,"mint"),
            ("ESTOP_NC","ESTOP NC",-0.8590f,-0.7000f,"amber"),
            ("SAFE_STOP_IN","SAFE STOP IN",-0.7860f,-0.7000f,"amber"),
            ("OVERTEMP","OVERTEMP",-0.7130f,-0.7000f,"mint"),
            ("SAFE_OT","SAFE OT",-0.6400f,-0.7000f,"amber"),
            ("PLC_RESET","PLC RESET",-0.5670f,-0.7000f,"mint"),
            ("SAFE_RESET","SAFE RESET",-0.4940f,-0.7000f,"amber"),
            ("HMI_H_RPM","HMI H RPM",-0.4210f,-0.7000f,"mint"),
            ("HMI_C_RPM","HMI C RPM",-0.3480f,-0.7000f,"mint"),
            ("HMI_P_RPM","HMI P RPM",-0.2750f,-0.7000f,"mint"),
            ("ENERGY_KW","ENG KW",-0.2020f,-0.7000f,"mint"),
            ("HMI_INLET","HMI INLET",-0.1290f,-0.7000f,"mint"),
            ("HMI_HOT","HMI HOT",-0.0560f,-0.7000f,"mint"),
            ("HMI_COLD","HMI COLD",0.0170f,-0.7000f,"mint"),
            ("HMI_BUF","HMI BUF",0.0900f,-0.7000f,"mint"),
            ("ENERGY_P_FLOW","ENG P FLOW",0.1630f,-0.7000f,"mint"),
            ("HMI_H_FLOW","HMI H Q",0.2360f,-0.7000f,"mint"),
            ("HMI_C_FLOW","HMI C Q",0.3090f,-0.7000f,"mint"),
            ("FOUL_DP","F DP",0.3820f,-0.7000f,"mint"),
            ("HX_RECOVERY","HX RECO",0.4550f,-0.7000f,"mint"),
            ("HMI_RECOVERY","HMI RECO",0.5280f,-0.7000f,"mint"),
            ("PID_PERMIT","PID PERMIT",0.6010f,-0.7000f,"mint"),
            ("HMI_ISO","HMI ISO",0.6740f,-0.7000f,"mint"),
            ("FOUL_TEST","FOUL TEST",0.7470f,-0.7000f,"mint"),
            ("INLET_TEST","INLET TEST",0.8200f,-0.7000f,"mint"),
            ("OUTLET_RTD_OUT","OUT RTD OUT",0.8930f,-0.7000f,"mint"),
            ("PID_PRODUCT_TEMP_IN","PID PROD T IN",0.9660f,-0.7000f,"mint"),
        };

        public static IEnumerable<string> TerminalIds => Sockets.Select(socket => socket.id);

        private static readonly Dictionary<string, string> Palette = new()
        {
            {"red","EE485C"},{"blue","1681E2"},{"amber","FFA82B"},{"violet","A282FF"},
            {"mint","34F6C5"},{"cyan","23DAFD"},{"void","07111C"},{"white","D4F5FF"},{"glass","498BA8"}
        };

        [MenuItem("Tools/Project Spark/Levels/Create Level 68")]
        public static void Install()
        {
            SparkLevel68Builder.Create();
            var root = AssetDatabase.LoadAssetAtPath<GameObject>(Prefab);
            if (root == null || root.GetComponent<SparkLevel68Process>().controllerDisplay == null) Fit();
            Register();
            ValidateAssets();
        }

        [MenuItem("Tools/Project Spark/Level 68/Validate Level Assets")]
        public static void ValidateAssets()
        {
            var root=AssetDatabase.LoadAssetAtPath<GameObject>(Prefab);
            var control=root != null ? root.GetComponent<SparkLevel68Process>() : null;
            var bindings=root != null ? root.GetComponent<SparkLevelSceneBindings>() : null;
            if (control==null || bindings==null || control.controllerDisplay==null || control.qualityDisplay==null || control.rig==null || control.rig.pivots.Length!=14 ||
                control.pins.Length!=122 || control.pins.Distinct().Count()!=122 || 
                !TerminalIds.SequenceEqual(control.pins.Select(pin=>pin.name)) ||
                control.pins.Any(pin=>!bindings.TryGetTerminal(pin.name,out var bound) || bound!=pin) ||
                SparkLevel68Process.Gates.Any(id=>!bindings.IsComponentIdDeclared(id)))
                throw new InvalidOperationException("Level 68 wiring, motion or binding references are incomplete.");
            Debug.Log("[LEVEL 68] Validated 122 distinct terminals, 78 player wires and 6 evaluator gates.",root);
        }

        private static void Register()
        {
            var scene = SceneManager.GetSceneByPath(ScenePath);
            bool opened = !scene.isLoaded;
            if (opened) scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
            bool dirty = scene.isDirty;
            var objects = scene.GetRootGameObjects();
            T Find<T>() where T : Component => objects.SelectMany(item => item.GetComponentsInChildren<T>(true)).Single();
            var level = AssetDatabase.LoadAssetAtPath<SparkLevelDefinition>(Folder + "Level_68.asset");
            var drawing = AssetDatabase.LoadAssetAtPath<SparkLevelDrawingAsset>(Folder + "Level_68_Drawing.asset");
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
            else if (dirty) Debug.Log("MainMenu had unsaved edits. Save the scene to persist Level 68's campaign and teaching entries.");
        }

        private static void Append(Object target, string field, Object value)
        {
            if (value == null) throw new InvalidOperationException("Missing Level 68 asset for " + field);
            var data = new SerializedObject(target);
            var list = data.FindProperty(field);
            for (int i = 0; i < list.arraySize; i++)
                if (list.GetArrayElementAtIndex(i).objectReferenceValue == value) return;
            list.arraySize++;
            list.GetArrayElementAtIndex(list.arraySize - 1).objectReferenceValue = value;
            data.ApplyModifiedProperties();
        }

        [MenuItem("Tools/Project Spark/Level 68/Fit Breadboard Assembly")]
        public static void Fit()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Exit Play Mode before fitting Level 68.");
            var models = Models.Select((name, index) =>
            {
                string path = Folder + $"Models/L68_{index:00}_{name}_Fitted.fbx";
                return AssetDatabase.LoadAssetAtPath<GameObject>(path) ??
                    throw new InvalidOperationException("Missing fitted kit module " + path);
            }).ToArray();
            if (AssetDatabase.LoadAssetAtPath<GameObject>(Prefab) == null)
                throw new InvalidOperationException("The Level 68 prefab is missing.");

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
                    throw new InvalidOperationException("Unity could not save the Level 68 layout.");
                AssetDatabase.SaveAssets();
                Debug.Log("Level 68 fitted to the MainMenu breadboard with 122 seated terminals.");
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
            Vector3 right = Vector3.ProjectOnPlane(grid.GridOrigin.right, up).normalized * .068f;
            Vector3 forward = Vector3.ProjectOnPlane(grid.GridOrigin.forward, up).normalized * .068f;
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
            var bench = root.transform.Find("Thermal Workbench");
            var bindings = root.GetComponent<SparkLevelSceneBindings>();
            var loop = root.GetComponent<SparkLevel68Process>();
            if (bench == null || bindings == null || loop == null)
                throw new InvalidOperationException("Level 68 is missing its workbench, controls, or scene bindings.");

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

            foreach (string name in new[] {"Fitted Level 68 Models", "Breadboard Labels", "Motion pivots"})
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
            var assembly = Child(bench, "Fitted Level 68 Models", new Vector3(0,floor,.035f));
            foreach (var model in models)
            {
                var instance = Object.Instantiate(model, assembly, false).transform;
                var markers = instance.GetComponentsInChildren<Transform>(true)
                    .Where(item => item.name.StartsWith("L68_Board",StringComparison.Ordinal))
                    .ToDictionary(item => item.name);
                Transform Marker(string name) => markers["L68_Board" + name];
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
                var oldControls = root.transform.Find("Level 68 Controls");
                if (oldControls != null) Object.DestroyImmediate(oldControls.gameObject);
                SparkLevel68Builder.BuildControls(root.transform,loop,font);
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
                terminalData.FindProperty("maxConnections").intValue = SparkLevel68Process.Wires.Count(wire => wire.a == index || wire.b == index);
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

                float offset = Array.IndexOf(Sockets,spec)%2==0 ? -.041f : -.068f;
                Label(labels,font,spec.id,spec.label,seat+new Vector3(0,.014f,offset),.0115f);
            }
            foreach (var owner in owners)
                foreach (Transform child in owner)
                    if (child.GetComponent<SparkTerminal>() == null && child.GetComponent<Renderer>() != null)
                        child.gameObject.SetActive(false);

            var titles = new (string title, float x, float z)[]
            {






                ("HOT / COOLING LOOPS",-.52f,.365f),("BUFFER / PRODUCT",.49f,.365f),
                ("FLOW / TEMPERATURE",-.54f,-.24f),("THERMAL SAFETY",.57f,-.24f)
            };
            foreach(var title in titles)
                Label(labels,font,title.title,title.title,new Vector3(title.x,floor+.028f,title.z),.018f);
            Transform Pivot(string module,string key)
            {
                var name="fitted_"+module+"_"+key;
                return assembly.GetComponentsInChildren<Transform>(true).Single(item=>item.name==name);
            }
            loop.rig.pivots=new[]{
                Pivot("HotWaterCirculationPump","ANIM_HOT_PUMP_IMPELLER"),
                Pivot("ColdWaterCirculationPump","ANIM_COLD_PUMP_IMPELLER"),
                Pivot("ProductTransferPump","ANIM_PRODUCT_PUMP_IMPELLER"),
                Pivot("HotLoopPumpMotor","ANIM_HOT_MOTOR"),
                Pivot("ColdLoopPumpMotor","ANIM_COLD_MOTOR"),
                Pivot("ProductPumpMotor","ANIM_PRODUCT_MOTOR"),
                Pivot("HotWaterControlValve","ANIM_HOT_VALVE"),
                Pivot("CoolingWaterControlValve","ANIM_COOLING_VALVE"),
                Pivot("ThreeWayMixingValve","ANIM_THREE_WAY_VALVE"),
                Pivot("RegenerativeBypassValve","ANIM_REGEN_BYPASS"),
                Pivot("ProductIsolationValve","ANIM_PRODUCT_ISOLATION"),
                Pivot("HolographicThermalSCADAMonitor","ANIM_THERMAL_SCADA"),
                Pivot("EmergencyThermalShutdownStation","ANIM_EMERGENCY_STOP"),
                Pivot("TransparentThermalSafetyEnclosure","ANIM_SAFETY_DOOR"),
            };
            var liquidMesh=new List<Transform>();
            var tanks=new (string name,float x,float z,float bottom,float radius,float height,string material)[]{
                ("Hot",-5.4f,-1.5f,1.40f,.664f,2.55f*.69f,"tintF87025"),
                ("Cooling",-5.25f,1.16f,1.37f,.582f,1.68f*.58f,"tint149DF9"),
                ("Buffer",4.45f,-1.55f,1.39f,.628f,2.15f*.60f,"tintF87025"),
                ("Product",4.52f,.50f,1.37f,.564f,2.05f*.53f,"tint9B55EA")
            };
            foreach(var tank in tanks)
            {
                var vessel=Child(assembly,tank.name+" fluid",new Vector3(tank.x,tank.bottom-.18f,-tank.z)*.111f);
                var liquid=GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                liquid.name="Live liquid"; liquid.transform.SetParent(vessel,false);
                Object.DestroyImmediate(liquid.GetComponent<Collider>());
                liquid.GetComponent<Renderer>().sharedMaterial=MaterialFor(shader,tank.material);
                liquid.transform.localPosition=Vector3.up*(tank.height*.111f*.5f);
                liquid.transform.localScale=new Vector3(tank.radius*2,tank.height*.5f,tank.radius*2)*.111f;
                liquidMesh.Add(liquid.transform);
            }
            loop.rig.liquid=liquidMesh.ToArray();
            if(loop.plot.transform.Find("Trend legend") == null)
            {
                var oldPanel = root.transform.Find("Level 68 Controls");
                if (oldPanel != null) Object.DestroyImmediate(oldPanel.gameObject);
                SparkLevel68Builder.BuildControls(root.transform,loop,font);
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
            loop.controllerDisplay=Readout("Thermal utility",new Vector3(-.53f,floor+.15f,.386f),new Color(.46f,.94f,1f));
            loop.qualityDisplay=Readout("Temperature status",new Vector3(.53f,floor+.15f,.386f),new Color(.68f,1f,.86f));
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
            label.fontSize = 68;
            label.characterSize = size;
            label.anchor = TextAnchor.MiddleCenter;
            label.color = new Color(.89f, .96f, 1f);
            label.text = text;
            child.GetComponent<MeshRenderer>().sharedMaterial = font.material;
            return label;
        }

        private static Material MaterialFor(Shader shader, string key)
        {
            string path = $"{Materials}/L68 {key}.mat";
            if (key.StartsWith("tint",StringComparison.Ordinal) && key.Length==10) Palette[key]=key.Substring(4);
            var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing != null && key is not ("glass" or "acrylic") &&
                ColorUtility.TryParseHtmlString("#" + (Palette.TryGetValue(key, out var savedTint) ? savedTint : "9FB3C8"), out var savedColor) &&
                existing.GetColor("_BaseColor") == savedColor) return existing;
            ColorUtility.TryParseHtmlString("#" + (Palette.TryGetValue(key, out var tint) ? tint : "9FB3C8"), out var color);
            var material = existing != null ? existing : new Material(shader) { name = "L68 " + key };
            bool metal = key is "chrome" or "titanium" or "steel" or "gold" or "copper" or "aluminum" or "tintCFDCE2" or "tintA4B8C4" or "tintE36C27" or "tintDC9C2F";
            material.SetFloat("_Metallic", metal ? .68f : .02f);
            material.SetFloat("_Smoothness", metal ? .68f : key == "glass" ? .12f : .35f);
            if (key is "green" or "cyan" or "red" or "blue" or "orange" or "amber" or "mint" or "warm" or "ice")
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", color * .68f);
            }
            else if (key.StartsWith("tint", StringComparison.Ordinal))
            {
                material.EnableKeyword("_EMISSION");
                float strength = key is "tint19CCEF" or "tint27D680" or "tintFFCE34" ? .30f : .12f;
                material.SetColor("_EmissionColor", color * strength);
            }
            if (key is "glass" or "glass_body" or "glass_soft" or "acrylic" or "glassblue" or "film" or "Glass" or "guardglass")
            {
                color.a = key=="film" ? .68f : .10f;
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

