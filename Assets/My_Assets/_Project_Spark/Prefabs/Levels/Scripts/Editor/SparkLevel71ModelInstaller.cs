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
    public static class SparkLevel71ModelInstaller
    {
        public const string Folder = "Assets/My_Assets/_Project_Spark/Prefabs/Levels/Level_71_Layout/";
        public const string Prefab = Folder + "Level_71_Layout.prefab";
        public const string ScenePath = "Assets/My_Assets/_Project_Spark/Scenes/MainMenu/MainMenu.unity";
        private static readonly string[] Models = {
            "IndustrialPlasmaFabricationBase","TransparentPlasmaEtchReactor","ReactorServiceLid","QuartzPlasmaObservationViewport","InductivelyCoupledRFCoil","PrecisionGasDistributionShowerhead","WaferBiasElectrode","ElectrostaticWaferChuck","PrecisionWaferLiftAssembly","SiliconWafer","WaferThinFilmLayer","PatternedWaferMask","WaferCarrierCassette","VacuumLoadLockChamber","LoadLockGateValve","AutomatedWaferTransferRobot","TurbomolecularVacuumPump","VacuumBackingPump","IndustrialVacuumForeline","VacuumIsolationValve","AutomaticVacuumThrottleValve","CapacitancePressureManometer","RoughVacuumPressureGauge","HighVacuumPressureGauge","ResidualGasAnalyzer","VirtualProcessGasSupplyA","VirtualProcessGasSupplyB","MassFlowControllerA","MassFlowControllerB","VacuumGasMixingManifold","HighFrequencyRFGenerator","AutomaticRFMatchingNetwork","RFVariableCapacitorA","RFVariableCapacitorB","ForwardReflectedPowerCoupler","RFVoltageCurrentSensor","RFArcProtectionModule","WaferTemperatureController","PrecisionWaferChiller","EmergencyPlasmaSafetyStation","OpticalEmissionSpectrometer","OpticalFiberViewportModule","AutomaticEtchEndpointController","PlasmaUniformityAnalyzer","ThinFilmThicknessMetrology","VacuumPressurePIDController","PlasmaProcessPLCRack","PlasmaDigitalIOModule","HolographicPlasmaSCADAMonitor","Regulated5VControlSupply"
        };
        private const string Materials = Folder + "Models/Materials";
        private const float SocketHeight = .025f;
        private const float SocketDiameter = .026f;

        private static readonly (string id, string label, float x, float z, string color)[] Sockets =
        {
            ("SUPPLY_POS","SUP POS",-0.8600f,0.6800f,"red"),
            ("PLC_5V","PLC 5V",-0.7700f,0.6800f,"red"),
            ("SUPPLY_RET","SUP RET",-0.6800f,0.6800f,"blue"),
            ("PLC_GND","PLC GND",-0.5900f,0.6800f,"blue"),
            ("IO_5V","IO 5V",-0.5000f,0.6800f,"red"),
            ("IO_GND","IO GND",-0.4100f,0.6800f,"blue"),
            ("VAC_5V","VAC 5V",-0.3200f,0.6800f,"red"),
            ("VAC_GND","VAC GND",-0.2300f,0.6800f,"blue"),
            ("RF_5V","RF 5V",-0.1400f,0.6800f,"red"),
            ("RF_GND","RF GND",-0.0500f,0.6800f,"blue"),
            ("OES_5V","OES 5V",0.0400f,0.6800f,"red"),
            ("OES_GND","OES GND",0.1300f,0.6800f,"blue"),
            ("PLC_BACKING","PLC BACKING",0.2200f,0.6800f,"mint"),
            ("BACKING_START","BACKING START",0.3100f,0.6800f,"mint"),
            ("BACKING_SPEED","BACKING SPEED",0.4000f,0.6800f,"mint"),
            ("PLC_BACK_RPM","PLC BACK RPM",0.4900f,0.6800f,"mint"),
            ("PLC_TURBO","PLC TURBO",0.5800f,0.6800f,"mint"),
            ("TURBO_START","TURBO START",0.6700f,0.6800f,"mint"),
            ("TURBO_SPEED","TURBO SPEED",0.7600f,0.6800f,"mint"),
            ("IO_TURBO_RPM","IO TURBO RPM",0.8500f,0.6800f,"mint"),
            ("PLC_ISOLATION","PLC ISOL",-0.9300f,0.5250f,"mint"),
            ("ISOLATION_CMD","ISOL CMD",-0.8320f,0.5250f,"mint"),
            ("ISOLATION_OPEN","ISOL OPEN",-0.7340f,0.5250f,"mint"),
            ("IO_ISOLATION","IO ISOL",-0.6360f,0.5250f,"mint"),
            ("PID_THROTTLE","PID THROTTLE",-0.5380f,0.5250f,"mint"),
            ("THROTTLE_CMD","THROTTLE CMD",-0.4400f,0.5250f,"mint"),
            ("THROTTLE_POS","THROTTLE POS",-0.3420f,0.5250f,"mint"),
            ("IO_THROTTLE","IO THROTTLE",-0.2440f,0.5250f,"mint"),
            ("CHAMBER_PA","CHAMBER PA",-0.1460f,0.5250f,"mint"),
            ("PID_PRESSURE","PID PRESS",-0.0480f,0.5250f,"mint"),
            ("FORELINE_PA","FORELINE PA",0.0500f,0.5250f,"mint"),
            ("PLC_FORELINE","PLC FORELINE",0.1480f,0.5250f,"mint"),
            ("LOCK_PA","LOCK PA",0.2460f,0.5250f,"mint"),
            ("PLC_LOCK_PRESS","PLC LOCK PRESS",0.3440f,0.5250f,"mint"),
            ("ROUGH_PA","ROUGH PA",0.4420f,0.5250f,"mint"),
            ("IO_ROUGH","IO ROUGH",0.5400f,0.5250f,"mint"),
            ("HIGH_PA","HIGH PA",0.6380f,0.5250f,"mint"),
            ("IO_HIGH","IO HIGH",0.7360f,0.5250f,"mint"),
            ("RGA_SIGNAL","RGA SIGNAL",0.8340f,0.5250f,"mint"),
            ("PLC_RGA","PLC RGA",0.9320f,0.5250f,"mint"),
            ("PLC_GAS_A","PLC GAS A",-0.9300f,0.4250f,"mint"),
            ("GAS_A_ENABLE","GAS A ENABLE",-0.8320f,0.4250f,"mint"),
            ("PLC_GAS_B","PLC GAS B",-0.7340f,0.4250f,"mint"),
            ("GAS_B_ENABLE","GAS B ENABLE",-0.6360f,0.4250f,"mint"),
            ("MFC_A_SET","MFC A SET",-0.5380f,0.4250f,"mint"),
            ("GAS_A_CMD","GAS A CMD",-0.4400f,0.4250f,"mint"),
            ("MFC_B_SET","MFC B SET",-0.3420f,0.4250f,"mint"),
            ("GAS_B_CMD","GAS B CMD",-0.2440f,0.4250f,"mint"),
            ("GAS_A_ACT","GAS A ACT",-0.1460f,0.4250f,"mint"),
            ("IO_FLOW_A","IO FLOW A",-0.0480f,0.4250f,"mint"),
            ("GAS_B_ACT","GAS B ACT",0.0500f,0.4250f,"mint"),
            ("IO_FLOW_B","IO FLOW B",0.1480f,0.4250f,"mint"),
            ("GAS_A_LEVEL","GAS A LEVEL",0.2460f,0.4250f,"mint"),
            ("PLC_INV_A","PLC INV A",0.3440f,0.4250f,"mint"),
            ("GAS_B_LEVEL","GAS B LEVEL",0.4420f,0.4250f,"mint"),
            ("PLC_INV_B","PLC INV B",0.5400f,0.4250f,"mint"),
            ("VALVE_A_STATE","VALVE A STATE",0.6380f,0.4250f,"mint"),
            ("IO_VALVE_A","IO VALVE A",0.7360f,0.4250f,"mint"),
            ("VALVE_B_STATE","VALVE B STATE",0.8340f,0.4250f,"mint"),
            ("IO_VALVE_B","IO VALVE B",0.9320f,0.4250f,"mint"),
            ("MIX_FLOW","MIX FLOW",-1.0000f,-0.3550f,"mint"),
            ("PLC_MIX_FLOW","PLC MIX FLOW",-0.9190f,-0.3550f,"mint"),
            ("MIX_RATIO","MIX RATIO",-0.8380f,-0.3550f,"mint"),
            ("PLC_MIX_RATIO","PLC MIX RATIO",-0.7570f,-0.3550f,"mint"),
            ("RF_PERMIT","RF PERMIT",-0.6760f,-0.3550f,"amber"),
            ("RF_ENABLE","RF ENABLE",-0.5950f,-0.3550f,"mint"),
            ("PLC_RF_SET","PLC RF SET",-0.5140f,-0.3550f,"mint"),
            ("RF_POWER_CMD","RF POWER CMD",-0.4330f,-0.3550f,"mint"),
            ("RF_FORWARD","RF FORWARD",-0.3520f,-0.3550f,"mint"),
            ("IO_FORWARD","IO FORWARD",-0.2710f,-0.3550f,"mint"),
            ("RF_REFLECT","RF REFLECT",-0.1900f,-0.3550f,"mint"),
            ("IO_REFLECT","IO REFLECT",-0.1090f,-0.3550f,"mint"),
            ("RF_CURRENT","RF CURRENT",-0.0280f,-0.3550f,"mint"),
            ("MATCH_I","MATCH I",0.0530f,-0.3550f,"mint"),
            ("RF_VOLTAGE","RF VOLTAGE",0.1340f,-0.3550f,"mint"),
            ("MATCH_V","MATCH V",0.2150f,-0.3550f,"mint"),
            ("MATCH_A","MATCH A",0.2960f,-0.3550f,"mint"),
            ("TUNER_A_CMD","TUNER A CMD",0.3770f,-0.3550f,"mint"),
            ("MATCH_B","MATCH B",0.4580f,-0.3550f,"mint"),
            ("TUNER_B_CMD","TUNER B CMD",0.5390f,-0.3550f,"mint"),
            ("TUNER_A_POS","TUNER A POS",0.6200f,-0.3550f,"mint"),
            ("IO_CAP_A","IO CAP A",0.7010f,-0.3550f,"mint"),
            ("TUNER_B_POS","TUNER B POS",0.7820f,-0.3550f,"mint"),
            ("IO_CAP_B","IO CAP B",0.8630f,-0.3550f,"mint"),
            ("RF_ARC","RF ARC",0.9440f,-0.3550f,"amber"),
            ("PLC_ARC_STOP","PLC ARC STOP",1.0250f,-0.3550f,"amber"),
            ("RF_READY","RF READY",-1.0000f,-0.5250f,"mint"),
            ("IO_RF_READY","IO RF READY",-0.9230f,-0.5250f,"mint"),
            ("RF_TEMP","RF TEMP",-0.8460f,-0.5250f,"mint"),
            ("IO_RF_TEMP","IO RF TEMP",-0.7690f,-0.5250f,"mint"),
            ("COUPLER_OK","COUPLER OK",-0.6920f,-0.5250f,"mint"),
            ("PLC_RF_METER","PLC RF METER",-0.6150f,-0.5250f,"mint"),
            ("RF_FREQ","RF FREQ",-0.5380f,-0.5250f,"mint"),
            ("IO_FREQ","IO FREQ",-0.4610f,-0.5250f,"mint"),
            ("BIAS_CMD","BIAS CMD",-0.3840f,-0.5250f,"mint"),
            ("WAFER_BIAS","WAFER BIAS",-0.3070f,-0.5250f,"mint"),
            ("BIAS_RD","BIAS RD",-0.2300f,-0.5250f,"mint"),
            ("IO_BIAS","IO BIAS",-0.1530f,-0.5250f,"mint"),
            ("PLASMA_STATE","PLASMA STATE",-0.0760f,-0.5250f,"mint"),
            ("PLC_PLASMA","PLC PLASMA",0.0010f,-0.5250f,"mint"),
            ("PLC_GATE","PLC GATE",0.0780f,-0.5250f,"mint"),
            ("GATE_CMD","GATE CMD",0.1550f,-0.5250f,"mint"),
            ("PLC_ROBOT","PLC ROBOT",0.2320f,-0.5250f,"mint"),
            ("ROBOT_CMD","ROBOT CMD",0.3090f,-0.5250f,"mint"),
            ("GATE_MON","GATE MON",0.3860f,-0.5250f,"mint"),
            ("ROBOT_MON","ROBOT MON",0.4630f,-0.5250f,"mint"),
            ("TRANSFER_OK","TRANSFER OK",0.5400f,-0.5250f,"mint"),
            ("LOCK_EQUAL","LOCK EQUAL",0.6170f,-0.5250f,"mint"),
            ("LIFT_CMD","LIFT CMD",0.6940f,-0.5250f,"mint"),
            ("CLAMP_CMD","CLAMP CMD",0.7710f,-0.5250f,"mint"),
            ("CHUCK_MON","CHUCK MON",0.8480f,-0.5250f,"mint"),
            ("PLC_GATE_MON","PLC GATE MON",0.9250f,-0.5250f,"mint"),
            ("OES_GATE","OES GATE",1.0020f,-0.5250f,"mint"),
            ("OPT_APERTURE","OPT APERTURE",-1.0000f,-0.7000f,"mint"),
            ("OES_FLOW_A","OES FLOW A",-0.9230f,-0.7000f,"mint"),
            ("OES_FLOW_B","OES FLOW B",-0.8460f,-0.7000f,"mint"),
            ("OES_PRESS","OES PRESS",-0.7690f,-0.7000f,"mint"),
            ("OES_LOCK","OES LOCK",-0.6920f,-0.7000f,"mint"),
            ("UNIFORMITY_IN","UNIFORMITY IN",-0.6150f,-0.7000f,"mint"),
            ("RF_MATCH_A","RF MATCH A",-0.5380f,-0.7000f,"mint"),
            ("RF_MATCH_B","RF MATCH B",-0.4610f,-0.7000f,"mint"),
            ("MATCH_MON","MATCH MON",-0.3840f,-0.7000f,"mint"),
            ("METROLOGY_EN","METROLOGY EN",-0.3070f,-0.7000f,"mint"),
            ("METROLOGY_MON","METROLOGY MON",-0.2300f,-0.7000f,"mint"),
            ("ESTOP_NC","ESTOP NC",-0.1530f,-0.7000f,"amber"),
            ("SAFE_STOP_IN","SAFE STOP IN",-0.0760f,-0.7000f,"amber"),
            ("RF_ARC_TRIP","RF ARC TRIP",0.0010f,-0.7000f,"amber"),
            ("VAC_PERMIT","VAC PERMIT",0.0780f,-0.7000f,"amber"),
            ("CHILL_PERMIT","CHILL PERMIT",0.1550f,-0.7000f,"amber"),
            ("CHUCK_BIAS_MON","CHUCK BIAS MON",0.2320f,-0.7000f,"mint"),
            ("WAFER_TEMP","WAFER TEMP",0.3090f,-0.7000f,"mint"),
            ("CHILLER_CMD","CHILLER CMD",0.3860f,-0.7000f,"mint"),
            ("ETCH_ENABLE","ETCH ENABLE",0.4630f,-0.7000f,"mint"),
            ("SCADA_FWD","SCADA FWD",0.5400f,-0.7000f,"mint"),
            ("SCADA_REF","SCADA REF",0.6170f,-0.7000f,"mint"),
            ("SCADA_PLASMA","SCADA PLASMA",0.6940f,-0.7000f,"mint"),
            ("SCADA_PRESS","SCADA PRESS",0.7710f,-0.7000f,"mint"),
            ("SCADA_GAS","SCADA GAS",0.8480f,-0.7000f,"mint"),
            ("OES_SIGNAL_OUT","OES SIGNAL OUT",0.9250f,-0.7000f,"mint"),
            ("ENDPOINT_OPTICAL_IN","ENDPOINT OPT IN",1.0020f,-0.7000f,"mint"),
        };

        public static IEnumerable<string> TerminalIds => Sockets.Select(socket => socket.id);

        private static readonly Dictionary<string, string> Palette = new()
        {
            {"red","EE485C"},{"blue","1711E2"},{"amber","FFA82B"},{"violet","A282FF"},{"plasma","A45DFF"},
            {"mint","34F6C5"},{"cyan","23DAFD"},{"void","07111C"},{"white","D4F5FF"},{"glass","498BA8"}
        };

        [MenuItem("Tools/Project Spark/Levels/Create Level 71")]
        public static void Install()
        {
            SparkLevel71Builder.Create();
            var root = AssetDatabase.LoadAssetAtPath<GameObject>(Prefab);
            if (root == null || root.GetComponent<SparkLevel71Process>().controllerDisplay == null) Fit();
            Register();
            ValidateAssets();
        }

        [MenuItem("Tools/Project Spark/Level 71/Validate Level Assets")]
        public static void ValidateAssets()
        {
            var root=AssetDatabase.LoadAssetAtPath<GameObject>(Prefab);
            var control=root != null ? root.GetComponent<SparkLevel71Process>() : null;
            var bindings=root != null ? root.GetComponent<SparkLevelSceneBindings>() : null;
            if (control==null || bindings==null || control.controllerDisplay==null || control.qualityDisplay==null || control.rig==null || control.rig.pivots.Length!=21 ||
                control.pins.Length!=140 || control.pins.Distinct().Count()!=140 || 
                !TerminalIds.SequenceEqual(control.pins.Select(pin=>pin.name)) ||
                control.pins.Any(pin=>!bindings.TryGetTerminal(pin.name,out var bound) || bound!=pin) ||
                SparkLevel71Process.Gates.Any(id=>!bindings.IsComponentIdDeclared(id)))
                throw new InvalidOperationException("Level 71 wiring, motion or binding references are incomplete.");
            Debug.Log("[LEVEL 71] Validated 140 distinct terminals, 90 player wires and 6 evaluator gates.",root);
        }

        private static void Register()
        {
            var scene = SceneManager.GetSceneByPath(ScenePath);
            bool opened = !scene.isLoaded;
            if (opened) scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
            bool dirty = scene.isDirty;
            var objects = scene.GetRootGameObjects();
            T Find<T>() where T : Component => objects.SelectMany(item => item.GetComponentsInChildren<T>(true)).Single();
            var level = AssetDatabase.LoadAssetAtPath<SparkLevelDefinition>(Folder + "Level_71.asset");
            var drawing = AssetDatabase.LoadAssetAtPath<SparkLevelDrawingAsset>(Folder + "Level_71_Drawing.asset");
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
            else if (dirty) Debug.Log("MainMenu had unsaved edits. Save the scene to persist Level 71's campaign and teaching entries.");
        }

        private static void Append(Object target, string field, Object value)
        {
            if (value == null) throw new InvalidOperationException("Missing Level 71 asset for " + field);
            var data = new SerializedObject(target);
            var list = data.FindProperty(field);
            for (int i = 0; i < list.arraySize; i++)
                if (list.GetArrayElementAtIndex(i).objectReferenceValue == value) return;
            list.arraySize++;
            list.GetArrayElementAtIndex(list.arraySize - 1).objectReferenceValue = value;
            data.ApplyModifiedProperties();
        }

        [MenuItem("Tools/Project Spark/Level 71/Fit Breadboard Assembly")]
        public static void Fit()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Exit Play Mode before fitting Level 71.");
            var models = Models.Select((name, index) =>
            {
                string path = Folder + $"Models/L71_{index:00}_{name}_Fitted.fbx";
                return AssetDatabase.LoadAssetAtPath<GameObject>(path) ??
                    throw new InvalidOperationException("Missing fitted kit module " + path);
            }).ToArray();
            if (AssetDatabase.LoadAssetAtPath<GameObject>(Prefab) == null)
                throw new InvalidOperationException("The Level 71 prefab is missing.");

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
                    throw new InvalidOperationException("Unity could not save the Level 71 layout.");
                AssetDatabase.SaveAssets();
                Debug.Log("Level 71 fitted to the MainMenu breadboard with 140 seated terminals.");
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
            Vector3 right = Vector3.ProjectOnPlane(grid.GridOrigin.right, up).normalized * .071f;
            Vector3 forward = Vector3.ProjectOnPlane(grid.GridOrigin.forward, up).normalized * .071f;
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
            var bench = root.transform.Find("Plasma Workbench");
            var bindings = root.GetComponent<SparkLevelSceneBindings>();
            var loop = root.GetComponent<SparkLevel71Process>();
            if (bench == null || bindings == null || loop == null)
                throw new InvalidOperationException("Level 71 is missing its workbench, controls, or scene bindings.");

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

            foreach (string name in new[] {"L71_HD_VISUAL_ART", "Breadboard Labels", "Motion pivots"})
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
            var assembly = Child(bench, "L71_HD_VISUAL_ART", new Vector3(0,floor,.025f));
            foreach (var model in models)
            {
                var instance = Object.Instantiate(model, assembly, false).transform;
                var markers = instance.GetComponentsInChildren<Transform>(true)
                    .Where(item => item.name.StartsWith("L71_Board",StringComparison.Ordinal))
                    .ToDictionary(item => item.name);
                Transform Marker(string name) => markers["L71_Board" + name];
                Vector3 Axis(string name) => assembly.InverseTransformVector(Marker(name).position - Marker("Center").position);
                instance.localRotation = Quaternion.Inverse(Quaternion.LookRotation(-Axis("Front"), Axis("Up"))) * instance.localRotation;
                instance.localScale /= Axis("Right").magnitude;
                instance.localPosition -= assembly.InverseTransformPoint(Marker("Center").position);
                if (Axis("Right").x < .99f)
                    throw new InvalidOperationException("Unexpected axes on fitted model " + model.name);
            }

            assembly.localScale = Vector3.one * .81f;
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
                var oldControls = root.transform.Find("Level 71 Controls");
                if (oldControls != null) Object.DestroyImmediate(oldControls.gameObject);
                SparkLevel71Builder.BuildControls(root.transform,loop,font);
            }
            var labels = Child(bench, "Breadboard Labels", Vector3.zero);
            var holes = new HashSet<Vector2Int>();
            var seatedLabels = new List<(string id, string label, Vector3 seat, int column)>();
            var owners = new HashSet<Transform>();
            int pinLayer = LayerMask.NameToLayer("Pin");
            if (pinLayer < 0) throw new InvalidOperationException("The Pin layer is missing.");
            foreach (var spec in Sockets)
            {
                if (!bindings.TryGetTerminal(spec.id, out var terminal))
                    throw new InvalidOperationException("Missing circuit terminal: " + spec.id);
                int index = Array.FindIndex(loop.pins, pin => pin == terminal);
                var terminalData = new SerializedObject(terminal);
                terminalData.FindProperty("maxConnections").intValue = SparkLevel71Process.Wires.Count(wire => wire.a == index || wire.b == index);
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

                seatedLabels.Add((spec.id,spec.label,seat,chosen.x));
            }
            foreach (var label in seatedLabels)
            {
                bool terminalBelow=seatedLabels.Any(next=>Mathf.Abs(next.seat.x-label.seat.x)<.025f&&
                    label.seat.z-next.seat.z is >.045f and <.108f);
                float offset=terminalBelow?-.032f:label.column%2==0?-.041f:-.071f;
                Label(labels,font,label.id,label.label,label.seat+new Vector3(0,.019f,offset),.0105f);
            }
            foreach (var owner in owners)
                foreach (Transform child in owner)
                    if (child.GetComponent<SparkTerminal>() == null && child.GetComponent<Renderer>() != null)
                        child.gameObject.SetActive(false);

            var titles = new (string title, float x, float z)[]
            {






                ("VACUUM / GAS",-.868f,.169f),("OPTICAL / METROLOGY",.862f,.169f),
                ("RF / MATCHING",-.58f,-.265f),("SAFETY / CHUCK",.61f,-.265f)
            };
            foreach(var title in titles)
                Label(labels,font,title.title,title.title,new Vector3(title.x,floor+.058f,title.z),.0165f);
            Label(labels,font,"Reactor label","PLASMA REACTOR",new Vector3(0,floor+.294f,.122f),.012f);
            Label(labels,font,"Load-lock label","LOAD LOCK",new Vector3(-.292f,floor+.230f,.102f),.012f);
            Transform Pivot(string module,string key)
            {
                var name="fitted_"+module+"_"+key;
                return assembly.GetComponentsInChildren<Transform>(true).Single(item=>item.name==name);
            }
            loop.rig.art=assembly;
            loop.rig.pivots=new[]{
                Pivot("ReactorServiceLid","ANIM_REACTOR_LID"),
                Pivot("LoadLockGateValve","ANIM_CHAMBER_GATE"),
                Pivot("AutomatedWaferTransferRobot","ANIM_WAFER_ROBOT_BASE"),
                Pivot("AutomatedWaferTransferRobot","ANIM_WAFER_ROBOT_ARM"),
                Pivot("AutomatedWaferTransferRobot","ANIM_WAFER_ROBOT_TOOL"),
                Pivot("PrecisionWaferLiftAssembly","ANIM_WAFER_LIFT"),
                Pivot("ElectrostaticWaferChuck","ANIM_WAFER_CLAMP"),
                Pivot("TurbomolecularVacuumPump","ANIM_TURBO_ROTOR"),
                Pivot("VacuumBackingPump","ANIM_BACKING_PUMP"),
                Pivot("AutomaticVacuumThrottleValve","ANIM_THROTTLE_VALVE"),
                Pivot("MassFlowControllerA","ANIM_MFC_A"),
                Pivot("MassFlowControllerB","ANIM_MFC_B"),
                Pivot("RFVariableCapacitorA","ANIM_RF_TUNER_A"),
                Pivot("RFVariableCapacitorB","ANIM_RF_TUNER_B"),
                Pivot("OpticalFiberViewportModule","ANIM_OES_APERTURE"),
                Pivot("VacuumIsolationValve","ANIM_VACUUM_ISOLATION"),
                Pivot("HolographicPlasmaSCADAMonitor","ANIM_PLASMA_SCADA"),
                Pivot("SiliconWafer","ANIM_WAFER_TRANSFER"),
                Pivot("AutomaticRFMatchingNetwork","ANIM_MATCHING_LIVE_DISPLAY"),
                Pivot("TransparentPlasmaEtchReactor","ANIM_PLASMA_VFX_CENTER"),
                Pivot("ThinFilmThicknessMetrology","ANIM_METROLOGY_PROBE"),
            };
            var pivots=loop.rig.pivots;
            pivots[3].SetParent(pivots[2],true);
            pivots[4].SetParent(pivots[3],true);
            var effect=GameObject.CreatePrimitive(PrimitiveType.Sphere);
            effect.name="Plasma glow"; effect.transform.SetParent(pivots[19],false);
            Object.DestroyImmediate(effect.GetComponent<Collider>());
            effect.transform.rotation=assembly.rotation;
            effect.GetComponent<Renderer>().sharedMaterial=MaterialFor(shader,"plasma");
            effect.GetComponent<Renderer>().shadowCastingMode=ShadowCastingMode.Off;
            loop.rig.plasma=effect.transform;
            effect.SetActive(false);
            loop.rig.mask=assembly.GetComponentsInChildren<Transform>(true).Single(item=>item.name=="L71_PatternedWaferMask");
            loop.rig.film=assembly.GetComponentsInChildren<Transform>(true).Single(item=>item.name=="L71_WaferThinFilmLayer");
            var carried=GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            carried.name="Transfer wafer"; carried.transform.SetParent(pivots[4],false);
            Object.DestroyImmediate(carried.GetComponent<Collider>());
            carried.transform.rotation=assembly.rotation;
            carried.transform.localScale=Vector3.Scale(new Vector3(.058f,.004f,.058f),new Vector3(
                assembly.lossyScale.x/pivots[4].lossyScale.x, assembly.lossyScale.y/pivots[4].lossyScale.y, assembly.lossyScale.z/pivots[4].lossyScale.z));
            carried.GetComponent<Renderer>().sharedMaterial=MaterialFor(shader,"cyan");
            loop.rig.carried=carried.transform;
            carried.SetActive(false);
            if(loop.plot.transform.Find("Trend legend") == null)
            {
                var oldPanel = root.transform.Find("Level 71 Controls");
                if (oldPanel != null) Object.DestroyImmediate(oldPanel.gameObject);
                SparkLevel71Builder.BuildControls(root.transform,loop,font);
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
            loop.controllerDisplay=Readout("Chamber pressure",new Vector3(-.815f,floor+.075f,.215f),new Color(.46f,.94f,1f));
            loop.qualityDisplay=Readout("Wafer status",new Vector3(.862f,floor+.075f,.215f),new Color(.71f,1f,.86f));
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
            label.fontSize = 71;
            label.characterSize = size;
            label.anchor = TextAnchor.MiddleCenter;
            label.color = new Color(.89f, .96f, 1f);
            label.text = text;
            child.GetComponent<MeshRenderer>().sharedMaterial = font.material;
            return label;
        }

        private static Material MaterialFor(Shader shader, string key)
        {
            string path = $"{Materials}/L71 {key}.mat";
            if (key.StartsWith("tint",StringComparison.Ordinal) && key.Length==10) Palette[key]=key.Substring(4);
            var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing != null && key is not ("glass" or "acrylic") &&
                ColorUtility.TryParseHtmlString("#" + (Palette.TryGetValue(key, out var savedTint) ? savedTint : "9FB3C8"), out var savedColor) &&
                existing.GetColor("_BaseColor") == savedColor) return existing;
            ColorUtility.TryParseHtmlString("#" + (Palette.TryGetValue(key, out var tint) ? tint : "9FB3C8"), out var color);
            var material = existing != null ? existing : new Material(shader) { name = "L71 " + key };
            bool metal = key is "chrome" or "titanium" or "steel" or "gold" or "copper" or "aluminum" or "tintCFDCE2" or "tintA4B8C4" or "tintE36C27" or "tintDC9C2F";
            material.SetFloat("_Metallic", metal ? .71f : .02f);
            material.SetFloat("_Smoothness", metal ? .71f : key == "glass" ? .12f : .35f);
            if (key is "green" or "cyan" or "red" or "blue" or "orange" or "amber" or "mint" or "warm" or "ice" or "plasma")
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", color * .71f);
            }
            else if (key.StartsWith("tint", StringComparison.Ordinal))
            {
                material.EnableKeyword("_EMISSION");
                float strength = key is "tint19CCEF" or "tint27D700" or "tintFFCE34" ? .30f : .12f;
                material.SetColor("_EmissionColor", color * strength);
            }
            if (key is "glass" or "glass_body" or "glass_soft" or "acrylic" or "glassblue" or "film" or "Glass" or "guardglass" or "plasma")
            {
                color.a = key=="plasma" ? .27f : key=="film" ? .70f : .10f;
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


