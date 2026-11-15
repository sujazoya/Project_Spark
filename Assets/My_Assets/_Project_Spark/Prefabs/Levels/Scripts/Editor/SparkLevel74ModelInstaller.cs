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
    public static class SparkLevel74ModelInstaller
    {
        public const string Folder = "Assets/My_Assets/_Project_Spark/Prefabs/Levels/Level_74_Layout/";
        public const string Prefab = Folder + "Level_74_Layout.prefab";
        public const string ScenePath = "Assets/My_Assets/_Project_Spark/Scenes/MainMenu/MainMenu.unity";
        private static readonly string[] Models = {
            "IndustrialLithographyWorkstationBase","TransparentLithographySafetyEnclosure","AutomatedPhotoresistTrack","PhotoresistSpinCoatingBowl","PrecisionSpinChuckDrive","VirtualPhotoresistSupplyCartridge","PhotoresistMeteringPump","PrecisionPhotoresistNozzle","ResistThicknessMeasurementSensor","SoftBakeHotplateModule","SoftBakeTemperatureSensor","SiliconWaferCarrierCassette","AutomatedWaferHandlingRobot","WaferPrealignmentStation","PrecisionProjectionScannerFrame","VirtualDUVLightSource","OpticalIlluminationHomogenizer","HighSpeedExposureShutter","PrecisionReticleStorageCassette","HighResolutionCircuitReticle","PrecisionReticleMotionStage","ReticleStageLinearEncoder","ProjectionOpticsLensBarrel","ActiveOpticalFocusCorrector","NumericalApertureSelector","NanometreWaferXYStage","DualAxisLinearServoMotorAssembly","WaferZTiltFocusStage","WaferStageLaserInterferometer","WaferAlignmentMarkCamera","ElectrostaticWaferChuck","PlanarizedSiliconWafer","WaferOverlayAlignmentMarks","PhotosensitiveResistLayer","WaferTopographyFocusMap","WaferHeightLevelingSensor","ActiveVibrationIsolationPlatform","SixAxisVibrationMeasurementSensor","ProjectionLensThermalDriftSensor","ExposureDosePhotodiode","PrecisionScanSlitAssembly","LithographyEmergencySafetyStation","PostExposureBakeModule","PostExposureBakeTemperatureSensor","VirtualPhotoresistDeveloperModule","DeveloperDispensingNozzle","DeveloperFlowTransmitter","WaferRinseSpinDryer","CriticalDimensionInspectionScanner","OverlayAlignmentMetrologySystem","ReticleDefectInspectionModule","PhotoresistPatternDefectCamera","LithographyProcessPLCRack","StageSynchronizationController","HolographicLithographySCADAMonitor","Regulated5VControlSupply"
        };
        private const string Materials = Folder + "Models/Materials";
        private const float SocketHeight = .025f;
        private const float SocketDiameter = .026f;

        private static readonly (string id, string label, float x, float z, string color)[] Sockets =
        {
            ("SUPPLY_POS","SUP POS",-0.9100f,0.6800f,"red"),
            ("PLC_5V","PLC 5V",-0.8320f,0.6800f,"red"),
            ("SUPPLY_RET","SUP RET",-0.7540f,0.6800f,"blue"),
            ("PLC_GND","PLC GND",-0.6760f,0.6800f,"blue"),
            ("IO_5V","IO 5V",-0.5980f,0.6800f,"red"),
            ("IO_GND","IO GND",-0.5200f,0.6800f,"blue"),
            ("STAGE_5V","STG 5V",-0.4420f,0.6800f,"red"),
            ("STAGE_GND","STG GND",-0.3640f,0.6800f,"blue"),
            ("DUV_5V","DUV 5V",-0.2860f,0.6800f,"red"),
            ("DUV_GND","DUV GND",-0.2080f,0.6800f,"blue"),
            ("LASER_5V","LASER 5V",-0.1300f,0.6800f,"red"),
            ("LASER_GND","LASER GND",-0.0520f,0.6800f,"blue"),
            ("PLC_WAFER","PLC WFR",0.0260f,0.6800f,"mint"),
            ("WAFER_RUN","WFR RUN",0.1040f,0.6800f,"mint"),
            ("PLC_RETICLE","PLC RET",0.1820f,0.6800f,"mint"),
            ("RETICLE_RUN","RET RUN",0.2600f,0.6800f,"mint"),
            ("WAFER_X_SET","WFR X SET",0.3380f,0.6800f,"mint"),
            ("SERVO_X_CMD","SERVO X CMD",0.4160f,0.6800f,"mint"),
            ("WAFER_Y_SET","WFR Y SET",0.4940f,0.6800f,"mint"),
            ("SERVO_Y_CMD","SERVO Y CMD",0.5720f,0.6800f,"mint"),
            ("RETICLE_X_SET","RET X SET",0.6500f,0.6800f,"mint"),
            ("RETICLE_X_CMD","RET X CMD",0.7280f,0.6800f,"mint"),
            ("RETICLE_Y_SET","RET Y SET",0.8060f,0.6800f,"mint"),
            ("RETICLE_Y_CMD","RET Y CMD",0.8840f,0.6800f,"mint"),
            ("WAFER_ENC_X","WFR ENC X",-0.9300f,0.5450f,"mint"),
            ("IO_WAFER_X","IO WFR X",-0.8480f,0.5450f,"mint"),
            ("WAFER_ENC_Y","WFR ENC Y",-0.7660f,0.5450f,"mint"),
            ("IO_WAFER_Y","IO WFR Y",-0.6840f,0.5450f,"mint"),
            ("RETICLE_ENC_X","RET ENC X",-0.6020f,0.5450f,"mint"),
            ("IO_RETICLE_X","IO RET X",-0.5200f,0.5450f,"mint"),
            ("RETICLE_ENC_Y","RET ENC Y",-0.4380f,0.5450f,"mint"),
            ("IO_RETICLE_Y","IO RET Y",-0.3560f,0.5450f,"mint"),
            ("WAFER_HOME","WFR HOME",-0.2740f,0.5450f,"mint"),
            ("IO_WAFER_HOME","IO WFR HOME",-0.1920f,0.5450f,"mint"),
            ("RETICLE_HOME","RET HOME",-0.1100f,0.5450f,"mint"),
            ("IO_RET_HOME","IO RET HOME",-0.0280f,0.5450f,"mint"),
            ("WAFER_READY","WFR READY",0.0540f,0.5450f,"mint"),
            ("PLC_WAFER_RDY","PLC WFR RDY",0.1360f,0.5450f,"mint"),
            ("RETICLE_READY","RET READY",0.2180f,0.5450f,"mint"),
            ("PLC_RET_RDY","PLC RET RDY",0.3000f,0.5450f,"mint"),
            ("STAGE_FAULT","STG FAULT",0.3820f,0.5450f,"amber"),
            ("IO_STAGE_FAULT","IO STG FAULT",0.4640f,0.5450f,"amber"),
            ("SERVO_CURRENT","SERVO CURRENT",0.5460f,0.5450f,"mint"),
            ("IO_SERVO_I","IO SERVO I",0.6280f,0.5450f,"mint"),
            ("PLC_LASER","PLC LASER",0.7100f,0.5450f,"mint"),
            ("LASER_EN","LASER EN",0.7920f,0.5450f,"mint"),
            ("LASER_VALID","LASER VALID",0.8740f,0.5450f,"mint"),
            ("IO_LASER_VALID","IO LASER VALID",0.9560f,0.5450f,"mint"),
            ("LASER_REF","LASER REF",-0.9300f,0.4250f,"mint"),
            ("SYNC_ZERO","SYNC ZERO",-0.8480f,0.4250f,"mint"),
            ("LASER_Y_MON","LASER Y MON",-0.7660f,0.4250f,"mint"),
            ("IO_LASER_Y","IO LASER Y",-0.6840f,0.4250f,"mint"),
            ("MARK_A","MARK A",-0.6020f,0.4250f,"mint"),
            ("ALIGN_MARK_A","ALIGN MARK A",-0.5200f,0.4250f,"mint"),
            ("MARK_B","MARK B",-0.4380f,0.4250f,"mint"),
            ("ALIGN_MARK_B","ALIGN MARK B",-0.3560f,0.4250f,"mint"),
            ("ALIGN_THETA","ALIGN THETA",-0.2740f,0.4250f,"mint"),
            ("WAFER_THETA","WFR THETA",-0.1920f,0.4250f,"mint"),
            ("ALIGN_OK","ALIGN OK",-0.1100f,0.4250f,"mint"),
            ("PLC_ALIGN_OK","PLC ALIGN OK",-0.0280f,0.4250f,"mint"),
            ("HEIGHT_SENSOR","HEIGHT SENSOR",0.0540f,0.4250f,"mint"),
            ("FOCUS_HEIGHT","FOCUS HEIGHT",0.1360f,0.4250f,"mint"),
            ("MAP_READY","MAP READY",0.2180f,0.4250f,"mint"),
            ("IO_HEIGHT_MAP","IO HEIGHT MAP",0.3000f,0.4250f,"mint"),
            ("Z_SET","Z SET",0.3820f,0.4250f,"mint"),
            ("Z_SERVO_CMD","Z SERVO CMD",0.4640f,0.4250f,"mint"),
            ("TILT_X_SET","TILT X SET",0.5460f,0.4250f,"mint"),
            ("TILT_X_CMD","TILT X CMD",0.6280f,0.4250f,"mint"),
            ("TILT_Y_SET","TILT Y SET",0.7100f,0.4250f,"mint"),
            ("TILT_Y_CMD","TILT Y CMD",0.7920f,0.4250f,"mint"),
            ("FOCUS_ERR","FOCUS ERR",0.8740f,0.4250f,"mint"),
            ("IO_FOCUS_ERR","IO FOCUS ERR",0.9560f,0.4250f,"mint"),
            ("Z_FB","Z FB",-1.0000f,-0.3550f,"mint"),
            ("IO_Z_POS","IO Z POS",-0.9250f,-0.3550f,"mint"),
            ("FOCUS_OK","FOCUS OK",-0.8500f,-0.3550f,"mint"),
            ("PLC_FOCUS_OK","PLC FOCUS OK",-0.7750f,-0.3550f,"mint"),
            ("SHUTTER_PERMIT","SHUT PERMIT",-0.7000f,-0.3550f,"amber"),
            ("SHUTTER_ENABLE","SHUT ENABLE",-0.6250f,-0.3550f,"mint"),
            ("PLC_DUV","PLC DUV",-0.5500f,-0.3550f,"mint"),
            ("DUV_RUN","DUV RUN",-0.4750f,-0.3550f,"mint"),
            ("DUV_LEVEL","DUV LEVEL",-0.4000f,-0.3550f,"mint"),
            ("IO_DUV_LEVEL","IO DUV LEVEL",-0.3250f,-0.3550f,"mint"),
            ("DUV_READY","DUV READY",-0.2500f,-0.3550f,"mint"),
            ("PLC_DUV_RDY","PLC DUV RDY",-0.1750f,-0.3550f,"mint"),
            ("PLC_SLIT","PLC SLIT",-0.1000f,-0.3550f,"mint"),
            ("SLIT_CMD","SLIT CMD",-0.0250f,-0.3550f,"mint"),
            ("SLIT_POS","SLIT POS",0.0500f,-0.3550f,"mint"),
            ("IO_SLIT","IO SLIT",0.1250f,-0.3550f,"mint"),
            ("SHUTTER_POS","SHUT POS",0.2000f,-0.3550f,"mint"),
            ("IO_SHUTTER","IO SHUT",0.2750f,-0.3550f,"mint"),
            ("PHOTO_CURRENT","PHOTO CURRENT",0.3500f,-0.3550f,"mint"),
            ("DOSE_MEAS","DOSE MEAS",0.4250f,-0.3550f,"mint"),
            ("DOSE_CAL","DOSE CAL",0.5000f,-0.3550f,"mint"),
            ("PHOTO_ZERO","PHOTO ZERO",0.5750f,-0.3550f,"mint"),
            ("DOSE_READY","DOSE READY",0.6500f,-0.3550f,"mint"),
            ("PLC_DOSE_OK","PLC DOSE OK",0.7250f,-0.3550f,"mint"),
            ("NA_SET","NA SET",0.8000f,-0.3550f,"mint"),
            ("APERTURE_CMD","NA CMD",0.8750f,-0.3550f,"mint"),
            ("NA_POS","NA POS",0.9500f,-0.3550f,"mint"),
            ("IO_NA_POS","IO NA POS",1.0250f,-0.3550f,"mint"),
            ("LENS_TEMP","LENS TEMP",-1.0000f,-0.5250f,"mint"),
            ("IO_LENS_TEMP","IO LENS TEMP",-0.9285f,-0.5250f,"mint"),
            ("LENS_COOL","LENS COOL",-0.8570f,-0.5250f,"mint"),
            ("COOL_CMD","COOL CMD",-0.7855f,-0.5250f,"mint"),
            ("PLC_SPIN","PLC SPIN",-0.7140f,-0.5250f,"mint"),
            ("COATER_RUN","COATER RUN",-0.6425f,-0.5250f,"mint"),
            ("SPIN_TACH","SPIN TACH",-0.5710f,-0.5250f,"mint"),
            ("IO_COAT_RPM","IO COAT RPM",-0.4995f,-0.5250f,"mint"),
            ("RESIST_PUMP","RST PUMP",-0.4280f,-0.5250f,"mint"),
            ("COAT_PUMP_CMD","COAT PUMP CMD",-0.3565f,-0.5250f,"mint"),
            ("RESIST_FLOW","RST FLOW",-0.2850f,-0.5250f,"mint"),
            ("IO_RESIST_FLOW","IO RST FLOW",-0.2135f,-0.5250f,"mint"),
            ("RESIST_THICK","RST THICK",-0.1420f,-0.5250f,"mint"),
            ("IO_RESIST_NM","IO RST NM",-0.0705f,-0.5250f,"mint"),
            ("HOTPLATE_TEMP","HOTPLATE TEMP",0.0010f,-0.5250f,"mint"),
            ("IO_BAKE_TEMP","IO BAKE TEMP",0.0725f,-0.5250f,"mint"),
            ("NOZZLE_CMD","NOZZLE CMD",0.1440f,-0.5250f,"mint"),
            ("DISPENSE_EN","DISPENSE EN",0.2155f,-0.5250f,"mint"),
            ("COATER_VALID","COATER VALID",0.2870f,-0.5250f,"mint"),
            ("COAT_SCAN_RDY","COAT SCAN RDY",0.3585f,-0.5250f,"mint"),
            ("WAFER_TRANSFER","WFR TRANSFER",0.4300f,-0.5250f,"mint"),
            ("SOFTBAKE_CMD","SOFTBAKE CMD",0.5015f,-0.5250f,"mint"),
            ("POSTBAKE_TEMP","POSTBAKE TEMP",0.5730f,-0.5250f,"mint"),
            ("POSTBAKE_EN","POSTBAKE EN",0.6445f,-0.5250f,"mint"),
            ("DEVELOPER_EN","DEV EN",0.7160f,-0.5250f,"mint"),
            ("RINSE_EN","RINSE EN",0.7875f,-0.5250f,"mint"),
            ("SPIN_DRY_EN","SPIN DRY EN",0.8590f,-0.5250f,"mint"),
            ("DRY_SPEED_MON","DRY SPEED MON",0.9305f,-0.5250f,"mint"),
            ("CD_SCANNER_EN","CD SCANNER EN",1.0020f,-0.5250f,"mint"),
            ("CD_FOCUS_MON","CD FOCUS MON",-1.0000f,-0.7000f,"mint"),
            ("OVERLAY_EN","OVERLAY EN",-0.9285f,-0.7000f,"mint"),
            ("OVERLAY_POS","OVERLAY POS",-0.8570f,-0.7000f,"mint"),
            ("DEFECT_CAM_EN","DEFECT CAM EN",-0.7855f,-0.7000f,"mint"),
            ("METROLOGY_RDY","METRO RDY",-0.7140f,-0.7000f,"mint"),
            ("ESTOP_NC","ESTOP NC",-0.6425f,-0.7000f,"amber"),
            ("SAFE_STOP_IN","SAFE STOP IN",-0.5710f,-0.7000f,"amber"),
            ("PLC_SAFE","PLC SAFE",-0.4995f,-0.7000f,"amber"),
            ("DUV_GUARD","DUV GUARD",-0.4280f,-0.7000f,"mint"),
            ("TRIP_STAGE","TRIP STG",-0.3565f,-0.7000f,"amber"),
            ("TRIP_LENS","TRIP LENS",-0.2850f,-0.7000f,"amber"),
            ("VIB_MONITOR","VIB MONITOR",-0.2135f,-0.7000f,"mint"),
            ("ISOLATOR_FAULT","ISOLATOR FAULT",-0.1420f,-0.7000f,"amber"),
            ("ISOLATOR_EN","ISOLATOR EN",-0.0705f,-0.7000f,"mint"),
            ("FOCUS_INTERLOCK","FOCUS LOCK",0.0010f,-0.7000f,"mint"),
            ("COAT_TRIP","COAT TRIP",0.0725f,-0.7000f,"amber"),
            ("SYNC_WAFER_X","SYNC WFR X",0.1440f,-0.7000f,"mint"),
            ("SYNC_RETICLE_X","SYNC RET X",0.2155f,-0.7000f,"mint"),
            ("SYNC_LASER_OK","SYNC LASER OK",0.2870f,-0.7000f,"mint"),
            ("SCADA_DOSE","SCADA DOSE",0.3585f,-0.7000f,"mint"),
            ("SCADA_FOCUS","SCADA FOCUS",0.4300f,-0.7000f,"mint"),
            ("SCADA_QUALITY","SCADA QUALITY",0.5015f,-0.7000f,"mint"),
            ("SCADA_SAFE","SCADA SAFE",0.5730f,-0.7000f,"amber"),
            ("SCADA_MOTION","SCADA MOTION",0.6445f,-0.7000f,"mint"),
            ("SCADA_LENS","SCADA LENS",0.7160f,-0.7000f,"mint"),
            ("SCADA_RESIST","SCADA RST",0.7875f,-0.7000f,"mint"),
            ("SCADA_ALIGN","SCADA ALIGN",0.8590f,-0.7000f,"mint"),
            ("WAFER_STAGE_INTERFEROMETER_SIGNAL_OUT","LASER POS OUT",0.9305f,-0.7000f,"mint"),
            ("STAGE_SYNCHRONIZATION_POSITION_FEEDBACK_IN","SYNC POS FB IN",1.0020f,-0.7000f,"mint"),
        };

        public static IEnumerable<string> TerminalIds => Sockets.Select(socket => socket.id);

        private static readonly Dictionary<string, string> Palette = new()
        {
            {"red","EE485C"},{"blue","1711E2"},{"amber","FFA82B"},{"violet","A282FF"},{"plasma","A45DFF"},
            {"mint","34F6C5"},{"cyan","23DAFD"},{"void","07111C"},{"white","D4F5FF"},{"glass","498BA8"}
        };

        [MenuItem("Tools/Project Spark/Levels/Create Level 74")]
        public static void Install()
        {
            SparkLevel74Builder.Create();
            var root = AssetDatabase.LoadAssetAtPath<GameObject>(Prefab);
            if (root == null || root.GetComponent<SparkLevel74Process>().controllerDisplay == null) Fit();
            Register();
            ValidateAssets();
        }

        [MenuItem("Tools/Project Spark/Level 74/Validate Level Assets")]
        public static void ValidateAssets()
        {
            var root=AssetDatabase.LoadAssetAtPath<GameObject>(Prefab);
            var control=root != null ? root.GetComponent<SparkLevel74Process>() : null;
            var bindings=root != null ? root.GetComponent<SparkLevelSceneBindings>() : null;
            if (control==null || bindings==null || control.controllerDisplay==null || control.qualityDisplay==null || control.rig==null || control.rig.pivots.Length!=42 ||
                control.pins.Length!=158 || control.pins.Distinct().Count()!=158 || 
                !TerminalIds.SequenceEqual(control.pins.Select(pin=>pin.name)) ||
                control.pins.Any(pin=>!bindings.TryGetTerminal(pin.name,out var bound) || bound!=pin) ||
                SparkLevel74Process.Gates.Any(id=>!bindings.IsComponentIdDeclared(id)))
                throw new InvalidOperationException("Level 74 wiring, motion or binding references are incomplete.");
            Debug.Log("[LEVEL 74] Validated 158 distinct terminals, 102 player wires and 6 evaluator gates.",root);
        }

        private static void Register()
        {
            var scene = SceneManager.GetSceneByPath(ScenePath);
            bool opened = !scene.isLoaded;
            if (opened) scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
            bool dirty = scene.isDirty;
            var objects = scene.GetRootGameObjects();
            T Find<T>() where T : Component => objects.SelectMany(item => item.GetComponentsInChildren<T>(true)).Single();
            var level = AssetDatabase.LoadAssetAtPath<SparkLevelDefinition>(Folder + "Level_74.asset");
            var drawing = AssetDatabase.LoadAssetAtPath<SparkLevelDrawingAsset>(Folder + "Level_74_Drawing.asset");
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
            else if (dirty) Debug.Log("MainMenu had unsaved edits. Save the scene to persist Level 74's campaign and teaching entries.");
        }

        private static void Append(Object target, string field, Object value)
        {
            if (value == null) throw new InvalidOperationException("Missing Level 74 asset for " + field);
            var data = new SerializedObject(target);
            var list = data.FindProperty(field);
            for (int i = 0; i < list.arraySize; i++)
                if (list.GetArrayElementAtIndex(i).objectReferenceValue == value) return;
            list.arraySize++;
            list.GetArrayElementAtIndex(list.arraySize - 1).objectReferenceValue = value;
            data.ApplyModifiedProperties();
        }

        [MenuItem("Tools/Project Spark/Level 74/Fit Breadboard Assembly")]
        public static void Fit()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Exit Play Mode before fitting Level 74.");
            var models = Models.Select((name, index) =>
            {
                string path = Folder + $"Models/L74_{index:00}_{name}_Fitted.fbx";
                return AssetDatabase.LoadAssetAtPath<GameObject>(path) ??
                    throw new InvalidOperationException("Missing fitted kit module " + path);
            }).ToArray();
            if (AssetDatabase.LoadAssetAtPath<GameObject>(Prefab) == null)
                throw new InvalidOperationException("The Level 74 prefab is missing.");

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
                    throw new InvalidOperationException("Unity could not save the Level 74 layout.");
                AssetDatabase.SaveAssets();
                Debug.Log("Level 74 fitted to the MainMenu breadboard with 158 seated terminals.");
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
            var bench = root.transform.Find("Lithography Workbench");
            var bindings = root.GetComponent<SparkLevelSceneBindings>();
            var loop = root.GetComponent<SparkLevel74Process>();
            if (bench == null || bindings == null || loop == null)
                throw new InvalidOperationException("Level 74 is missing its workbench, controls, or scene bindings.");

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

            foreach (string name in new[] {"L74_HD_VISUAL_ART", "Breadboard Labels", "Motion pivots"})
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
            var assembly = Child(bench, "L74_HD_VISUAL_ART", new Vector3(0,floor,-.021f));
            foreach (var model in models)
            {
                var instance = Object.Instantiate(model, assembly, false).transform;
                var markers = instance.GetComponentsInChildren<Transform>(true)
                    .Where(item => item.name.StartsWith("L74_Board",StringComparison.Ordinal))
                    .ToDictionary(item => item.name);
                Transform Marker(string name) => markers["L74_Board" + name];
                Vector3 Axis(string name) => assembly.InverseTransformVector(Marker(name).position - Marker("Center").position);
                instance.localRotation = Quaternion.Inverse(Quaternion.LookRotation(-Axis("Front"), Axis("Up"))) * instance.localRotation;
                instance.localScale /= Axis("Right").magnitude;
                instance.localPosition -= assembly.InverseTransformPoint(Marker("Center").position);
                if (Axis("Right").x < .99f)
                    throw new InvalidOperationException("Unexpected axes on fitted model " + model.name);
            }

            assembly.localScale = Vector3.one * .67f;
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
                var oldControls = root.transform.Find("Level 74 Controls");
                if (oldControls != null) Object.DestroyImmediate(oldControls.gameObject);
                SparkLevel74Builder.BuildControls(root.transform,loop,font);
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
                terminalData.FindProperty("maxConnections").intValue = SparkLevel74Process.Wires.Count(wire => wire.a == index || wire.b == index);
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
                bool rearInnerRow=label.seat.z is >.36f and <.47f && Mathf.Abs(label.seat.x)<.70f;
                float offset=rearInnerRow?(label.column%2==0?.030f:.052f):terminalBelow?-.032f:label.column%2==0?-.041f:-.071f;
                Label(labels,font,label.id,label.label,label.seat+new Vector3(0,.019f,offset),.0096f);
            }
            foreach (var owner in owners)
                foreach (Transform child in owner)
                    if (child.GetComponent<SparkTerminal>() == null && child.GetComponent<Renderer>() != null)
                        child.gameObject.SetActive(false);

            var titles = new (string title, float x, float z)[]
            {






                ("COAT / BAKE",-.865f,.147f),("SCANNER / OPTICS",.862f,.147f),
                ("ALIGN / FOCUS",-.86f,-.235f),("SAFETY / METROLOGY",.86f,-.235f)
            };
            foreach(var title in titles)
                Label(labels,font,title.title,title.title,new Vector3(title.x,floor+.058f,title.z),.0165f);
            Label(labels,font,"Exposure label","4:1 SCANNER",new Vector3(-.022f,floor+.30f,.095f),.012f);
            Label(labels,font,"Development label","DEVELOP / INSPECT",new Vector3(.35f,floor+.23f,-.075f),.012f);
            Transform Pivot(string module,string key)
            {
                var name="fitted_"+module+"_"+key;
                return assembly.GetComponentsInChildren<Transform>(true).Single(item=>item.name==name);
            }
            loop.rig.art=assembly;
            loop.rig.pivots=new[]{
                Pivot("PhotoresistSpinCoatingBowl","ANIM_RESIST_SPIN_BOWL"),
                Pivot("PrecisionSpinChuckDrive","ANIM_RESIST_SPIN_CHUCK"),
                Pivot("PhotoresistMeteringPump","ANIM_RESIST_PUMP"),
                Pivot("PrecisionPhotoresistNozzle","ANIM_RESIST_NOZZLE"),
                Pivot("ResistThicknessMeasurementSensor","ANIM_RESIST_THICKNESS_PROBE"),
                Pivot("SoftBakeHotplateModule","ANIM_SOFTBAKE_LIFT"),
                Pivot("AutomatedWaferHandlingRobot","ANIM_WAFER_ROBOT_BASE"),
                Pivot("AutomatedWaferHandlingRobot","ANIM_WAFER_ROBOT_ARM"),
                Pivot("AutomatedWaferHandlingRobot","ANIM_WAFER_ROBOT_TOOL"),
                Pivot("WaferPrealignmentStation","ANIM_WAFER_PREALIGNER"),
                Pivot("VirtualDUVLightSource","ANIM_DUV_SOURCE_IRIS"),
                Pivot("HighSpeedExposureShutter","ANIM_EXPOSURE_SHUTTER"),
                Pivot("PrecisionReticleMotionStage","ANIM_RETICLE_STAGE_X"),
                Pivot("PrecisionReticleMotionStage","ANIM_RETICLE_STAGE_Y"),
                Pivot("ReticleStageLinearEncoder","ANIM_RETICLE_POSITION_MARKER"),
                Pivot("ActiveOpticalFocusCorrector","ANIM_LENS_FOCUS"),
                Pivot("NumericalApertureSelector","ANIM_NA_DIAPHRAGM"),
                Pivot("NanometreWaferXYStage","ANIM_WAFER_STAGE_X"),
                Pivot("NanometreWaferXYStage","ANIM_WAFER_STAGE_Y"),
                Pivot("DualAxisLinearServoMotorAssembly","ANIM_X_LINEAR_SERVO"),
                Pivot("DualAxisLinearServoMotorAssembly","ANIM_Y_LINEAR_SERVO"),
                Pivot("WaferZTiltFocusStage","ANIM_WAFER_STAGE_Z"),
                Pivot("WaferZTiltFocusStage","ANIM_WAFER_TILT_X"),
                Pivot("WaferZTiltFocusStage","ANIM_WAFER_TILT_Y"),
                Pivot("WaferStageLaserInterferometer","ANIM_INTERFEROMETER_HEAD"),
                Pivot("WaferAlignmentMarkCamera","ANIM_ALIGNMENT_CAMERA"),
                Pivot("ElectrostaticWaferChuck","ANIM_WAFER_CHUCK"),
                Pivot("WaferTopographyFocusMap","ANIM_TOPOGRAPHY_FOCUS_MAP"),
                Pivot("WaferHeightLevelingSensor","ANIM_HEIGHT_LEVELING_HEAD"),
                Pivot("ActiveVibrationIsolationPlatform","ANIM_ISOLATION_STAGE"),
                Pivot("ExposureDosePhotodiode","ANIM_DOSE_PHOTODIODE"),
                Pivot("PrecisionScanSlitAssembly","ANIM_SCAN_SLIT"),
                Pivot("LithographyEmergencySafetyStation","ANIM_LITHO_ESTOP"),
                Pivot("PostExposureBakeModule","ANIM_POSTBAKE_LID"),
                Pivot("VirtualPhotoresistDeveloperModule","ANIM_DEVELOPER_SPIN"),
                Pivot("DeveloperDispensingNozzle","ANIM_DEVELOPER_NOZZLE"),
                Pivot("WaferRinseSpinDryer","ANIM_RINSE_DRYER"),
                Pivot("CriticalDimensionInspectionScanner","ANIM_CD_SCANNER"),
                Pivot("OverlayAlignmentMetrologySystem","ANIM_OVERLAY_METROLOGY"),
                Pivot("ReticleDefectInspectionModule","ANIM_RETICLE_INSPECTION"),
                Pivot("PhotoresistPatternDefectCamera","ANIM_RESIST_DEFECT_CAMERA"),
                Pivot("HolographicLithographySCADAMonitor","ANIM_LITHOGRAPHY_SCADA"),
            };
            var pivots=loop.rig.pivots;
            pivots[7].SetParent(pivots[6],true);
            pivots[8].SetParent(pivots[7],true);
            var light=GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            light.name="Virtual exposure beam";
            light.transform.SetParent(assembly,false);
            light.transform.position=pivots[26].position+assembly.TransformVector(Vector3.up*.044f);
            light.transform.localScale=new Vector3(.024f,.036f,.024f);
            Object.DestroyImmediate(light.GetComponent<Collider>());
            light.GetComponent<Renderer>().sharedMaterial=MaterialFor(shader,"plasma");
            light.GetComponent<Renderer>().shadowCastingMode=ShadowCastingMode.Off;
            light.SetActive(false);
            loop.rig.beam=light.transform;
            var carried=GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            carried.name="Transfer wafer"; carried.transform.SetParent(pivots[8],false);
            Object.DestroyImmediate(carried.GetComponent<Collider>());
            carried.transform.rotation=assembly.rotation;
            carried.transform.localScale=Vector3.Scale(new Vector3(.058f,.004f,.058f),new Vector3(
                assembly.lossyScale.x/pivots[8].lossyScale.x, assembly.lossyScale.y/pivots[8].lossyScale.y, assembly.lossyScale.z/pivots[8].lossyScale.z));
            carried.GetComponent<Renderer>().sharedMaterial=MaterialFor(shader,"cyan");
            loop.rig.carried=carried.transform;
            carried.SetActive(false);
            if(loop.plot.transform.Find("Trend legend") == null)
            {
                var oldPanel = root.transform.Find("Level 74 Controls");
                if (oldPanel != null) Object.DestroyImmediate(oldPanel.gameObject);
                SparkLevel74Builder.BuildControls(root.transform,loop,font);
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
            label.fontSize = 74;
            label.characterSize = size;
            label.anchor = TextAnchor.MiddleCenter;
            label.color = new Color(.89f, .96f, 1f);
            label.text = text;
            child.GetComponent<MeshRenderer>().sharedMaterial = font.material;
            return label;
        }

        private static Material MaterialFor(Shader shader, string key)
        {
            string path = $"{Materials}/L74 {key}.mat";
            if (key.StartsWith("tint",StringComparison.Ordinal) && key.Length==10) Palette[key]=key.Substring(4);
            var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing != null && key is not ("glass" or "acrylic") &&
                ColorUtility.TryParseHtmlString("#" + (Palette.TryGetValue(key, out var savedTint) ? savedTint : "9FB3C8"), out var savedColor) &&
                existing.GetColor("_BaseColor") == savedColor) return existing;
            ColorUtility.TryParseHtmlString("#" + (Palette.TryGetValue(key, out var tint) ? tint : "9FB3C8"), out var color);
            var material = existing != null ? existing : new Material(shader) { name = "L74 " + key };
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


