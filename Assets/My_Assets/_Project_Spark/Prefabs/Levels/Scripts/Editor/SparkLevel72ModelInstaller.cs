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
    public static class SparkLevel72ModelInstaller
    {
        public const string Folder = "Assets/My_Assets/_Project_Spark/Prefabs/Levels/Level_72_Layout/";
        public const string Prefab = Folder + "Level_72_Layout.prefab";
        public const string ScenePath = "Assets/My_Assets/_Project_Spark/Scenes/MainMenu/MainMenu.unity";
        private static readonly string[] Models = {
            "IndustrialAtomicDepositionBase","TransparentALDReactorChamber","ReactorServiceLid","QuartzObservationViewport","MultiZoneHeatedWaferChuck","PrecisionSiliconWafer","AtomicThinFilmVisualization","HighAspectRatioTrenchSample","WaferLiftPinAssembly","WaferCarrierCassette","TransparentVacuumLoadLock","LoadLockGateValve","AutomatedWaferTransferRobot","VirtualPrecursorSupplyA","VirtualPrecursorSupplyB","InertPurgeGasSupply","HighSpeedPulseValveA","HighSpeedPulseValveB","HighSpeedPurgeValve","MassFlowControllerA","MassFlowControllerB","PurgeMassFlowTransmitter","SequentialGasDistributionManifold","PrecisionDepositionShowerhead","HeatedGasDeliveryPipeline","TurbomolecularVacuumPump","VacuumBackingPump","AutomaticVacuumThrottleValve","VacuumIsolationValve","IndustrialVacuumForeline","CapacitancePressureManometer","RoughVacuumPressureGauge","HeatedDeliveryLineSensor","WaferChuckTemperatureSensor","MultiZoneHeaterPowerBank","QuartzCrystalMicrobalance","InSituOpticalEllipsometer","OpticalThicknessViewport","ConformalFilmMetrologyScanner","SurfaceCoverageMonitor","ResidualVaporAnalyzer","ReactantOverlapMonitor","GrowthPerCycleAnalyzer","FourStageALDCycleSequencer","AtomicDepositionPLCRack","WaferTemperaturePIDController","VacuumPressurePIDController","PurgeInterlockProtectionRelay","VirtualExhaustTreatmentModule","EmergencyDepositionSafetyStation","HolographicAtomicDepositionSCADA","Regulated5VControlSupply"
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
            ("HEAT_5V","HEAT 5V",-0.1400f,0.6800f,"red"),
            ("HEAT_GND","HEAT GND",-0.0500f,0.6800f,"blue"),
            ("PURGE_5V","PURGE 5V",0.0400f,0.6800f,"red"),
            ("PURGE_GND","PURGE GND",0.1300f,0.6800f,"blue"),
            ("PLC_BACKING","PLC BACKING",0.2200f,0.6800f,"mint"),
            ("BACKING_RUN","BACKING RUN",0.3100f,0.6800f,"mint"),
            ("BACKING_SPEED","BACKING SPEED",0.4000f,0.6800f,"mint"),
            ("IO_BACK_RPM","IO BACK RPM",0.4900f,0.6800f,"mint"),
            ("PLC_TURBO","PLC TURBO",0.5800f,0.6800f,"mint"),
            ("TURBO_RUN","TURBO RUN",0.6700f,0.6800f,"mint"),
            ("TURBO_SPEED","TURBO SPEED",0.7600f,0.6800f,"mint"),
            ("IO_TURBO_RPM","IO TURBO RPM",0.8500f,0.6800f,"mint"),
            ("PLC_ISOLATION","PLC ISOL",-0.9300f,0.5250f,"mint"),
            ("ISOLATION_CMD","ISOL CMD",-0.8320f,0.5250f,"mint"),
            ("ISOLATION_POS","ISOL POS",-0.7340f,0.5250f,"mint"),
            ("IO_ISOLATION","IO ISOL",-0.6360f,0.5250f,"mint"),
            ("PID_THROTTLE","PID THROTTLE",-0.5380f,0.5250f,"mint"),
            ("THROTTLE_CMD","THROTTLE CMD",-0.4400f,0.5250f,"mint"),
            ("THROTTLE_POS","THROTTLE POS",-0.3420f,0.5250f,"mint"),
            ("IO_THROTTLE","IO THROTTLE",-0.2440f,0.5250f,"mint"),
            ("CHAMBER_PA","CHAMBER PA",-0.1460f,0.5250f,"mint"),
            ("PID_PRESSURE","PID PRESS",-0.0480f,0.5250f,"mint"),
            ("FORELINE_PA","FORELINE PA",0.0500f,0.5250f,"mint"),
            ("IO_FORELINE","IO FORELINE",0.1480f,0.5250f,"mint"),
            ("LOADLOCK_PA","LOADLOCK PA",0.2460f,0.5250f,"mint"),
            ("PLC_LOCK_PA","PLC LOCK PA",0.3440f,0.5250f,"mint"),
            ("ROUGH_PA","ROUGH PA",0.4420f,0.5250f,"mint"),
            ("IO_ROUGH","IO ROUGH",0.5400f,0.5250f,"mint"),
            ("HIGH_PA","HIGH PA",0.6380f,0.5250f,"mint"),
            ("IO_HIGH","IO HIGH",0.7360f,0.5250f,"mint"),
            ("RESIDUAL_PA","RES PA",0.8340f,0.5250f,"mint"),
            ("PLC_RGA","PLC RGA",0.9320f,0.5250f,"mint"),
            ("DOSE_PERMIT","DOSE PERM",-0.9300f,0.4250f,"amber"),
            ("DOSE_ENABLE","DOSE ENABLE",-0.8320f,0.4250f,"mint"),
            ("PLC_A_ENABLE","PLC A ENABLE",-0.7340f,0.4250f,"mint"),
            ("SOURCE_A_EN","SRC A EN",-0.6360f,0.4250f,"mint"),
            ("PLC_B_ENABLE","PLC B ENABLE",-0.5380f,0.4250f,"mint"),
            ("SOURCE_B_EN","SRC B EN",-0.4400f,0.4250f,"mint"),
            ("SEQ_A_CMD","SEQ A CMD",-0.3420f,0.4250f,"mint"),
            ("VALVE_A_CMD","VALVE A CMD",-0.2440f,0.4250f,"mint"),
            ("SEQ_B_CMD","SEQ B CMD",-0.1460f,0.4250f,"mint"),
            ("VALVE_B_CMD","VALVE B CMD",-0.0480f,0.4250f,"mint"),
            ("VALVE_A_POS","VALVE A POS",0.0500f,0.4250f,"mint"),
            ("IO_A_POSITION","IO A POS",0.1480f,0.4250f,"mint"),
            ("VALVE_B_POS","VALVE B POS",0.2460f,0.4250f,"mint"),
            ("IO_B_POSITION","IO B POS",0.3440f,0.4250f,"mint"),
            ("MFC_A_SET","MFC A SET",0.4420f,0.4250f,"mint"),
            ("FLOW_A_CMD","FLOW A CMD",0.5400f,0.4250f,"mint"),
            ("MFC_B_SET","MFC B SET",0.6380f,0.4250f,"mint"),
            ("FLOW_B_CMD","FLOW B CMD",0.7360f,0.4250f,"mint"),
            ("FLOW_A_OUT","FLOW A OUT",0.8340f,0.4250f,"mint"),
            ("IO_FLOW_A","IO FLOW A",0.9320f,0.4250f,"mint"),
            ("FLOW_B_OUT","FLOW B OUT",-1.0000f,-0.3550f,"mint"),
            ("IO_FLOW_B","IO FLOW B",-0.9250f,-0.3550f,"mint"),
            ("STOCK_A_OUT","STOCK A OUT",-0.8500f,-0.3550f,"mint"),
            ("IO_STOCK_A","IO STOCK A",-0.7750f,-0.3550f,"mint"),
            ("STOCK_B_OUT","STOCK B OUT",-0.7000f,-0.3550f,"mint"),
            ("IO_STOCK_B","IO STOCK B",-0.6250f,-0.3550f,"mint"),
            ("MIX_A_OUT","MIX A OUT",-0.5500f,-0.3550f,"mint"),
            ("MIX_A_MON","MIX A MON",-0.4750f,-0.3550f,"mint"),
            ("MIX_B_OUT","MIX B OUT",-0.4000f,-0.3550f,"mint"),
            ("MIX_B_MON","MIX B MON",-0.3250f,-0.3550f,"mint"),
            ("A_RESIDUAL","A RES",-0.2500f,-0.3550f,"mint"),
            ("SEQ_A_RESIDUAL","SEQ A RES",-0.1750f,-0.3550f,"mint"),
            ("B_RESIDUAL","B RES",-0.1000f,-0.3550f,"mint"),
            ("SEQ_B_RESIDUAL","SEQ B RES",-0.0250f,-0.3550f,"mint"),
            ("OVERLAP_OUT","OVERLAP OUT",0.0500f,-0.3550f,"mint"),
            ("PLC_OVERLAP","PLC OVERLAP",0.1250f,-0.3550f,"mint"),
            ("PLC_PURGE","PLC PURGE",0.2000f,-0.3550f,"mint"),
            ("SOURCE_P_EN","SRC P EN",0.2750f,-0.3550f,"mint"),
            ("SEQ_PURGE_CMD","SEQ PURGE CMD",0.3500f,-0.3550f,"mint"),
            ("PURGE_VALVE_CMD","P VALVE CMD",0.4250f,-0.3550f,"mint"),
            ("PURGE_VALVE_POS","P VALVE POS",0.5000f,-0.3550f,"mint"),
            ("IO_PURGE_POSITION","IO PURGE POS",0.5750f,-0.3550f,"mint"),
            ("P_FLOW_SET","P FLOW SET",0.6500f,-0.3550f,"mint"),
            ("PURGE_REG_CMD","PURGE REG CMD",0.7250f,-0.3550f,"mint"),
            ("PURGE_REG_POS","PURGE REG POS",0.8000f,-0.3550f,"mint"),
            ("IO_PURGE_REG","IO PURGE REG",0.8750f,-0.3550f,"mint"),
            ("PURGE_READY","PURGE READY",0.9500f,-0.3550f,"mint"),
            ("PLC_PURGE_READY","PLC PURGE READY",1.0250f,-0.3550f,"mint"),
            ("P_STOCK_OUT","P STOCK OUT",-1.0000f,-0.5250f,"mint"),
            ("IO_PURGE_STOCK","IO PURGE STOCK",-0.9285f,-0.5250f,"mint"),
            ("P_FLOW_OK","P FLOW OK",-0.8570f,-0.5250f,"mint"),
            ("PLC_FLOW_OK","PLC FLOW OK",-0.7855f,-0.5250f,"mint"),
            ("P_SENSOR_OK","P SENSOR OK",-0.7140f,-0.5250f,"mint"),
            ("IO_P_VALID","IO P VALID",-0.6425f,-0.5250f,"mint"),
            ("P_ZERO_REQ","P ZERO REQ",-0.5710f,-0.5250f,"mint"),
            ("P_ZERO_CMD","P ZERO CMD",-0.4995f,-0.5250f,"mint"),
            ("A_CLEAR_OUT","A CLEAR OUT",-0.4280f,-0.5250f,"mint"),
            ("SEQ_A_CLEAR","SEQ A CLEAR",-0.3565f,-0.5250f,"mint"),
            ("B_CLEAR_OUT","B CLEAR OUT",-0.2850f,-0.5250f,"mint"),
            ("SEQ_B_CLEAR","SEQ B CLEAR",-0.2135f,-0.5250f,"mint"),
            ("PURGE_TRIP","PURGE TRIP",-0.1420f,-0.5250f,"amber"),
            ("IO_PURGE_ALARM","IO PURGE ALARM",-0.0705f,-0.5250f,"amber"),
            ("RESIDUAL_OK","RES OK",0.0010f,-0.5250f,"mint"),
            ("PLC_CLEAR","PLC CLEAR",0.0725f,-0.5250f,"mint"),
            ("PLC_GATE","PLC GATE",0.1440f,-0.5250f,"mint"),
            ("GATE_CMD","GATE CMD",0.2155f,-0.5250f,"mint"),
            ("PLC_ROBOT","PLC ROBOT",0.2870f,-0.5250f,"mint"),
            ("ROBOT_CMD","ROBOT CMD",0.3585f,-0.5250f,"mint"),
            ("GATE_MON","GATE MON",0.4300f,-0.5250f,"mint"),
            ("ROBOT_MON","ROBOT MON",0.5015f,-0.5250f,"mint"),
            ("TRANSFER_OK","XFER OK",0.5730f,-0.5250f,"mint"),
            ("GATE_PRESS_OK","GATE PRESS OK",0.6445f,-0.5250f,"mint"),
            ("WAFER_LIFT","WAFER LIFT",0.7160f,-0.5250f,"mint"),
            ("CHUCK_CLAMP","CHUCK CLAMP",0.7875f,-0.5250f,"mint"),
            ("HEAT_PERMIT","HEAT PERM",0.8590f,-0.5250f,"amber"),
            ("CHUCK_CLOSED","CHUCK CLOSED",0.9305f,-0.5250f,"mint"),
            ("ZONE_PID_EN","ZONE PID EN",1.0020f,-0.5250f,"mint"),
            ("ZONE_TEMP_MON","ZONE TEMP MON",-1.0000f,-0.7000f,"mint"),
            ("METROLOGY_REQ","METRO REQ",-0.9285f,-0.7000f,"mint"),
            ("QCM_FEED","QCM FEED",-0.8570f,-0.7000f,"mint"),
            ("QCM_RESPONSE","QCM RESPONSE",-0.7855f,-0.7000f,"mint"),
            ("OPTICAL_ENABLE","OPTICAL ENABLE",-0.7140f,-0.7000f,"mint"),
            ("OPTICAL_READY","OPTICAL READY",-0.6425f,-0.7000f,"mint"),
            ("FILM_THICKNESS_MON","FILM THICK MON",-0.5710f,-0.7000f,"mint"),
            ("FILM_SCAN_EN","FILM SCAN EN",-0.4995f,-0.7000f,"mint"),
            ("UNIFORMITY_MON","UNIF MON",-0.4280f,-0.7000f,"mint"),
            ("COVERAGE_MON","COV MON",-0.3565f,-0.7000f,"mint"),
            ("REACTION_MON","RXN MON",-0.2850f,-0.7000f,"mint"),
            ("QUALITY_INTERLOCK","QUALITY LOCK",-0.2135f,-0.7000f,"mint"),
            ("TRENCH_SCAN","TRENCH SCAN",-0.1420f,-0.7000f,"mint"),
            ("ESTOP_NC","ESTOP NC",-0.0705f,-0.7000f,"amber"),
            ("SAFE_STOP_IN","SAFE STOP IN",0.0010f,-0.7000f,"amber"),
            ("PLC_SAFETY","PLC SAFETY",0.0725f,-0.7000f,"amber"),
            ("LID_LOCK","LID LOCK",0.1440f,-0.7000f,"mint"),
            ("INTERLOCK_MON","LOCK MON",0.2155f,-0.7000f,"mint"),
            ("SAFE_PURGE","SAFE PURGE",0.2870f,-0.7000f,"amber"),
            ("LOCK_STATUS","LOCK STATUS",0.3585f,-0.7000f,"mint"),
            ("HEATER_ALARM","HEATER ALARM",0.4300f,-0.7000f,"amber"),
            ("STUCK_ALARM","STUCK ALARM",0.5015f,-0.7000f,"amber"),
            ("RESET_READY","RESET READY",0.5730f,-0.7000f,"mint"),
            ("HMI_PRESS","HMI PRESS",0.6445f,-0.7000f,"mint"),
            ("HMI_REACTANT","HMI REACTANT",0.7160f,-0.7000f,"mint"),
            ("HMI_QUALITY","HMI QUALITY",0.7875f,-0.7000f,"mint"),
            ("HMI_SEQUENCE","HMI SEQ",0.8590f,-0.7000f,"mint"),
            ("PURGE_FLOW_TRANSMITTER_SIGNAL_OUT","PURGE FLOW OUT",0.9305f,-0.7000f,"mint"),
            ("ALD_SEQUENCER_PURGE_FEEDBACK_IN","PURGE FB IN",1.0020f,-0.7000f,"mint"),
        };

        public static IEnumerable<string> TerminalIds => Sockets.Select(socket => socket.id);

        private static readonly Dictionary<string, string> Palette = new()
        {
            {"red","EE485C"},{"blue","1711E2"},{"amber","FFA82B"},{"violet","A282FF"},{"plasma","A45DFF"},
            {"mint","34F6C5"},{"cyan","23DAFD"},{"void","07111C"},{"white","D4F5FF"},{"glass","498BA8"}
        };

        [MenuItem("Tools/Project Spark/Levels/Create Level 72")]
        public static void Install()
        {
            SparkLevel72Builder.Create();
            var root = AssetDatabase.LoadAssetAtPath<GameObject>(Prefab);
            if (root == null || root.GetComponent<SparkLevel72Process>().controllerDisplay == null) Fit();
            Register();
            ValidateAssets();
        }

        [MenuItem("Tools/Project Spark/Level 72/Validate Level Assets")]
        public static void ValidateAssets()
        {
            var root=AssetDatabase.LoadAssetAtPath<GameObject>(Prefab);
            var control=root != null ? root.GetComponent<SparkLevel72Process>() : null;
            var bindings=root != null ? root.GetComponent<SparkLevelSceneBindings>() : null;
            if (control==null || bindings==null || control.controllerDisplay==null || control.qualityDisplay==null || control.rig==null || control.rig.pivots.Length!=24 ||
                control.pins.Length!=146 || control.pins.Distinct().Count()!=146 || 
                !TerminalIds.SequenceEqual(control.pins.Select(pin=>pin.name)) ||
                control.pins.Any(pin=>!bindings.TryGetTerminal(pin.name,out var bound) || bound!=pin) ||
                SparkLevel72Process.Gates.Any(id=>!bindings.IsComponentIdDeclared(id)))
                throw new InvalidOperationException("Level 72 wiring, motion or binding references are incomplete.");
            Debug.Log("[LEVEL 72] Validated 146 distinct terminals, 94 player wires and 6 evaluator gates.",root);
        }

        private static void Register()
        {
            var scene = SceneManager.GetSceneByPath(ScenePath);
            bool opened = !scene.isLoaded;
            if (opened) scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
            bool dirty = scene.isDirty;
            var objects = scene.GetRootGameObjects();
            T Find<T>() where T : Component => objects.SelectMany(item => item.GetComponentsInChildren<T>(true)).Single();
            var level = AssetDatabase.LoadAssetAtPath<SparkLevelDefinition>(Folder + "Level_72.asset");
            var drawing = AssetDatabase.LoadAssetAtPath<SparkLevelDrawingAsset>(Folder + "Level_72_Drawing.asset");
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
            else if (dirty) Debug.Log("MainMenu had unsaved edits. Save the scene to persist Level 72's campaign and teaching entries.");
        }

        private static void Append(Object target, string field, Object value)
        {
            if (value == null) throw new InvalidOperationException("Missing Level 72 asset for " + field);
            var data = new SerializedObject(target);
            var list = data.FindProperty(field);
            for (int i = 0; i < list.arraySize; i++)
                if (list.GetArrayElementAtIndex(i).objectReferenceValue == value) return;
            list.arraySize++;
            list.GetArrayElementAtIndex(list.arraySize - 1).objectReferenceValue = value;
            data.ApplyModifiedProperties();
        }

        [MenuItem("Tools/Project Spark/Level 72/Fit Breadboard Assembly")]
        public static void Fit()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Exit Play Mode before fitting Level 72.");
            var models = Models.Select((name, index) =>
            {
                string path = Folder + $"Models/L72_{index:00}_{name}_Fitted.fbx";
                return AssetDatabase.LoadAssetAtPath<GameObject>(path) ??
                    throw new InvalidOperationException("Missing fitted kit module " + path);
            }).ToArray();
            if (AssetDatabase.LoadAssetAtPath<GameObject>(Prefab) == null)
                throw new InvalidOperationException("The Level 72 prefab is missing.");

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
                    throw new InvalidOperationException("Unity could not save the Level 72 layout.");
                AssetDatabase.SaveAssets();
                Debug.Log("Level 72 fitted to the MainMenu breadboard with 146 seated terminals.");
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
            var bench = root.transform.Find("Deposition Workbench");
            var bindings = root.GetComponent<SparkLevelSceneBindings>();
            var loop = root.GetComponent<SparkLevel72Process>();
            if (bench == null || bindings == null || loop == null)
                throw new InvalidOperationException("Level 72 is missing its workbench, controls, or scene bindings.");

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

            foreach (string name in new[] {"L72_HD_VISUAL_ART", "Breadboard Labels", "Motion pivots"})
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
            var assembly = Child(bench, "L72_HD_VISUAL_ART", new Vector3(0,floor,.025f));
            foreach (var model in models)
            {
                var instance = Object.Instantiate(model, assembly, false).transform;
                var markers = instance.GetComponentsInChildren<Transform>(true)
                    .Where(item => item.name.StartsWith("L72_Board",StringComparison.Ordinal))
                    .ToDictionary(item => item.name);
                Transform Marker(string name) => markers["L72_Board" + name];
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
                var oldControls = root.transform.Find("Level 72 Controls");
                if (oldControls != null) Object.DestroyImmediate(oldControls.gameObject);
                SparkLevel72Builder.BuildControls(root.transform,loop,font);
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
                terminalData.FindProperty("maxConnections").intValue = SparkLevel72Process.Wires.Count(wire => wire.a == index || wire.b == index);
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
                Label(labels,font,label.id,label.label,label.seat+new Vector3(0,.019f,offset),.0096f);
            }
            foreach (var owner in owners)
                foreach (Transform child in owner)
                    if (child.GetComponent<SparkTerminal>() == null && child.GetComponent<Renderer>() != null)
                        child.gameObject.SetActive(false);

            var titles = new (string title, float x, float z)[]
            {






                ("VACUUM / DELIVERY",-.865f,.169f),("WAFER / METROLOGY",.862f,.169f),
                ("PRECURSOR CONTROL",-.68f,-.265f),("PURGE / SAFETY",.72f,-.265f)
            };
            foreach(var title in titles)
                Label(labels,font,title.title,title.title,new Vector3(title.x,floor+.058f,title.z),.0165f);
            Label(labels,font,"Reactor label","ALD REACTOR",new Vector3(0,floor+.294f,.122f),.012f);
            Label(labels,font,"Load-lock label","LOAD LOCK",new Vector3(-.292f,floor+.230f,.102f),.012f);
            Transform Pivot(string module,string key)
            {
                var name="fitted_"+module+"_"+key;
                return assembly.GetComponentsInChildren<Transform>(true).Single(item=>item.name==name);
            }
            loop.rig.art=assembly;
            loop.rig.pivots=new[]{
                Pivot("ReactorServiceLid","ANIM_REACTOR_LID"),
                Pivot("LoadLockGateValve","ANIM_LOADLOCK_GATE"),
                Pivot("AutomatedWaferTransferRobot","ANIM_WAFER_ROBOT_BASE"),
                Pivot("AutomatedWaferTransferRobot","ANIM_WAFER_ROBOT_ARM"),
                Pivot("AutomatedWaferTransferRobot","ANIM_WAFER_ROBOT_TOOL"),
                Pivot("WaferLiftPinAssembly","ANIM_WAFER_LIFT"),
                Pivot("MultiZoneHeatedWaferChuck","ANIM_WAFER_CHUCK"),
                Pivot("HighSpeedPulseValveA","ANIM_PULSE_VALVE_A"),
                Pivot("HighSpeedPulseValveB","ANIM_PULSE_VALVE_B"),
                Pivot("HighSpeedPurgeValve","ANIM_PURGE_VALVE"),
                Pivot("MassFlowControllerA","ANIM_MFC_A"),
                Pivot("MassFlowControllerB","ANIM_MFC_B"),
                Pivot("TurbomolecularVacuumPump","ANIM_TURBO_ROTOR"),
                Pivot("VacuumBackingPump","ANIM_BACKING_ROTOR"),
                Pivot("AutomaticVacuumThrottleValve","ANIM_THROTTLE_VALVE"),
                Pivot("VacuumIsolationValve","ANIM_VACUUM_ISOLATION"),
                Pivot("PrecisionDepositionShowerhead","ANIM_SHOWERHEAD_SERVICE"),
                Pivot("InSituOpticalEllipsometer","ANIM_ELLIPSOMETER_HEAD"),
                Pivot("ConformalFilmMetrologyScanner","ANIM_METROLOGY_SCANNER"),
                Pivot("EmergencyDepositionSafetyStation","ANIM_ESTOP"),
                Pivot("HolographicAtomicDepositionSCADA","ANIM_ALD_SCADA"),
                Pivot("AtomicThinFilmVisualization","ANIM_ATOMIC_LAYER_GROWTH"),
                Pivot("FourStageALDCycleSequencer","ANIM_FOUR_STAGE_DISPLAY"),
                Pivot("HighAspectRatioTrenchSample","ANIM_TRENCH_CONFORMALITY"),
            };
            var pivots=loop.rig.pivots;
            pivots[3].SetParent(pivots[2],true);
            pivots[4].SetParent(pivots[3],true);
            loop.rig.wafer=assembly.GetComponentsInChildren<Transform>(true).Single(item=>item.name=="L72_PrecisionSiliconWafer");
            var carried=GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            carried.name="Transfer wafer"; carried.transform.SetParent(pivots[4],false);
            Object.DestroyImmediate(carried.GetComponent<Collider>());
            carried.transform.rotation=assembly.rotation;
            carried.transform.localScale=Vector3.Scale(new Vector3(.058f,.004f,.058f),new Vector3(
                assembly.lossyScale.x/pivots[4].lossyScale.x, assembly.lossyScale.y/pivots[4].lossyScale.y, assembly.lossyScale.z/pivots[4].lossyScale.z));
            carried.GetComponent<Renderer>().sharedMaterial=MaterialFor(shader,"cyan");
            loop.rig.carried=carried.transform;
            carried.SetActive(false);
            var tracer=GameObject.CreatePrimitive(PrimitiveType.Sphere);
            tracer.name="Reactor flow indicator";
            tracer.transform.SetParent(assembly,false);
            tracer.transform.position=pivots[21].position+assembly.TransformVector(Vector3.up*.044f);
            tracer.transform.localScale=new Vector3(.13f,.036f,.13f);
            Object.DestroyImmediate(tracer.GetComponent<Collider>());
            tracer.GetComponent<Renderer>().sharedMaterial=MaterialFor(shader,"plasma");
            tracer.GetComponent<Renderer>().shadowCastingMode=ShadowCastingMode.Off;
            loop.rig.tracer=tracer;
            tracer.SetActive(false);
            if(loop.plot.transform.Find("Trend legend") == null)
            {
                var oldPanel = root.transform.Find("Level 72 Controls");
                if (oldPanel != null) Object.DestroyImmediate(oldPanel.gameObject);
                SparkLevel72Builder.BuildControls(root.transform,loop,font);
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
            label.fontSize = 72;
            label.characterSize = size;
            label.anchor = TextAnchor.MiddleCenter;
            label.color = new Color(.89f, .96f, 1f);
            label.text = text;
            child.GetComponent<MeshRenderer>().sharedMaterial = font.material;
            return label;
        }

        private static Material MaterialFor(Shader shader, string key)
        {
            string path = $"{Materials}/L72 {key}.mat";
            if (key.StartsWith("tint",StringComparison.Ordinal) && key.Length==10) Palette[key]=key.Substring(4);
            var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing != null && key is not ("glass" or "acrylic") &&
                ColorUtility.TryParseHtmlString("#" + (Palette.TryGetValue(key, out var savedTint) ? savedTint : "9FB3C8"), out var savedColor) &&
                existing.GetColor("_BaseColor") == savedColor) return existing;
            ColorUtility.TryParseHtmlString("#" + (Palette.TryGetValue(key, out var tint) ? tint : "9FB3C8"), out var color);
            var material = existing != null ? existing : new Material(shader) { name = "L72 " + key };
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


