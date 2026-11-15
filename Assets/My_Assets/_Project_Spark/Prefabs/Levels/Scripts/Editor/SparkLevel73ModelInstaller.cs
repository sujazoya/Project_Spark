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
    public static class SparkLevel73ModelInstaller
    {
        public const string Folder = "Assets/My_Assets/_Project_Spark/Prefabs/Levels/Level_73_Layout/";
        public const string Prefab = Folder + "Level_73_Layout.prefab";
        public const string ScenePath = "Assets/My_Assets/_Project_Spark/Scenes/MainMenu/MainMenu.unity";
        private static readonly string[] Models = {
            "IndustrialCMPWorkstationBase","TransparentCMPSafetyEnclosure","RotatingPolishingPlaten","GroovedPolishingPad","PlatenServoMotorDrive","PrecisionWaferCarrierHead","CarrierGimbalAssembly","MultiZonePressureBladder","WaferRetainingRing","SiliconWaferSubstrate","PatternedThinFilmLayer","MicroscopicSurfaceTopography","WaferCarrierCassette","AutomatedWaferTransferRobot","WaferLiftAndAlignmentAssembly","VirtualSlurrySupplyCabinet","AbrasiveSlurryMixingTank","PrecisionSlurryMeteringPump","SlurryMassFlowTransmitter","MotorizedSlurryDispensingArm","PrecisionSlurryNozzle","SlurryConcentrationSensor","SlurryTemperatureSensor","PadConditioningSwingArm","PadConditionerAbrasiveDisc","PadConditionerMotorDrive","PadWearMeasurementSensor","PadGrooveInspectionScanner","CarrierDownforceLoadCell","CarrierPressureRegulator","PlatenSpeedEncoder","CarrierSpeedEncoder","MotorTorqueCurrentAnalyzer","PolishingVibrationSensor","AcousticEmissionSensor","InterfaceTemperatureProbe","PolishingCoolingLoop","TransparentSlurrySplashGuard","WasteSlurryCollectionModule","CMPEmergencySafetyStation","PostCMPRinseChamber","WaferBrushCleaningAssembly","SpinRinseDryer","InSituOpticalEndpointSensor","EddyCurrentThicknessSensor","OpticalSurfaceProfilometer","WaferDefectInspectionCamera","DishingErosionAnalyzer","MultiZonePressurePIDController","PlatenCarrierMotionController","CMPProcessPLCRack","CMPDigitalIOModule","HolographicCMPSCADA","Regulated5VControlSupply"
        };
        private const string Materials = Folder + "Models/Materials";
        private const float SocketHeight = .025f;
        private const float SocketDiameter = .026f;

        private static readonly (string id, string label, float x, float z, string color)[] Sockets =
        {
            ("SUPPLY_POS","SUP POS",-0.8600f,0.6800f,"red"),
            ("PLC_5V","PLC 5V",-0.7750f,0.6800f,"red"),
            ("SUPPLY_RET","SUP RET",-0.6900f,0.6800f,"blue"),
            ("PLC_GND","PLC GND",-0.6050f,0.6800f,"blue"),
            ("IO_5V","IO 5V",-0.5200f,0.6800f,"red"),
            ("IO_GND","IO GND",-0.4350f,0.6800f,"blue"),
            ("DRIVE_5V","DRV 5V",-0.3500f,0.6800f,"red"),
            ("DRIVE_GND","DRV GND",-0.2650f,0.6800f,"blue"),
            ("MET_5V","MET 5V",-0.1800f,0.6800f,"red"),
            ("MET_GND","MET GND",-0.0950f,0.6800f,"blue"),
            ("FORCE_5V","FORCE 5V",-0.0100f,0.6800f,"red"),
            ("FORCE_GND","FORCE GND",0.0750f,0.6800f,"blue"),
            ("PLC_PLATEN","PLC PLAT",0.1600f,0.6800f,"mint"),
            ("PLATEN_RUN","PLAT RUN",0.2450f,0.6800f,"mint"),
            ("PLC_CARRIER","PLC CAR",0.3300f,0.6800f,"mint"),
            ("CARRIER_RUN","CAR RUN",0.4150f,0.6800f,"mint"),
            ("PLC_PL_RPM","PLC PL RPM",0.5000f,0.6800f,"mint"),
            ("PLATEN_SET","PLAT SET",0.5850f,0.6800f,"mint"),
            ("PLC_CA_RPM","PLC CA RPM",0.6700f,0.6800f,"mint"),
            ("CARRIER_SET","CAR SET",0.7550f,0.6800f,"mint"),
            ("PLATEN_ENC_A","PLAT ENC A",0.8400f,0.6800f,"mint"),
            ("DRIVE_P_ENC_A","DRV P ENC A",0.9250f,0.6800f,"mint"),
            ("PLATEN_ENC_B","PLAT ENC B",-0.9300f,0.5450f,"mint"),
            ("DRIVE_P_ENC_B","DRV P ENC B",-0.8410f,0.5450f,"mint"),
            ("CARRIER_ENC_A","CAR ENC A",-0.7520f,0.5450f,"mint"),
            ("DRIVE_C_ENC_A","DRV C ENC A",-0.6630f,0.5450f,"mint"),
            ("CARRIER_ENC_B","CAR ENC B",-0.5740f,0.5450f,"mint"),
            ("DRIVE_C_ENC_B","DRV C ENC B",-0.4850f,0.5450f,"mint"),
            ("PLATEN_TORQUE","PLAT TORQUE",-0.3960f,0.5450f,"mint"),
            ("IO_P_TORQUE","IO P TORQUE",-0.3070f,0.5450f,"mint"),
            ("CARRIER_TORQUE","CAR TORQUE",-0.2180f,0.5450f,"mint"),
            ("IO_C_TORQUE","IO C TORQUE",-0.1290f,0.5450f,"mint"),
            ("PLATEN_CURRENT","PLAT CURR",-0.0400f,0.5450f,"mint"),
            ("IO_P_CURRENT","IO P CURR",0.0490f,0.5450f,"mint"),
            ("CARRIER_CURRENT","CAR CURR",0.1380f,0.5450f,"mint"),
            ("IO_C_CURRENT","IO C CURR",0.2270f,0.5450f,"mint"),
            ("PLATEN_READY","PLAT READY",0.3160f,0.5450f,"mint"),
            ("PLC_P_READY","PLC P READY",0.4050f,0.5450f,"mint"),
            ("CARRIER_READY","CAR READY",0.4940f,0.5450f,"mint"),
            ("PLC_C_READY","PLC C READY",0.5830f,0.5450f,"mint"),
            ("DRIVE_FAULT","DRV FAULT",0.6720f,0.5450f,"mint"),
            ("PLC_DRIVE_ALARM","PLC DRV ALARM",0.7610f,0.5450f,"amber"),
            ("CURRENT_OK","CURR OK",0.8500f,0.5450f,"mint"),
            ("IO_DRIVE_OK","IO DRV OK",0.9390f,0.5450f,"mint"),
            ("CONTACT_PERMIT","CTCT PERMIT",-0.9300f,0.4250f,"amber"),
            ("CONTACT_ENABLE","CTCT ENABLE",-0.8410f,0.4250f,"mint"),
            ("PLC_Z_MOVE","PLC Z MOVE",-0.7520f,0.4250f,"mint"),
            ("CARRIER_Z_CMD","CAR Z CMD",-0.6630f,0.4250f,"mint"),
            ("Z_POSITION","Z POS",-0.5740f,0.4250f,"mint"),
            ("IO_HEAD_Z","IO HEAD Z",-0.4850f,0.4250f,"mint"),
            ("PID_CENTER","PID CENTER",-0.3960f,0.4250f,"mint"),
            ("ZONE_CENTER","ZONE CENTER",-0.3070f,0.4250f,"mint"),
            ("PID_MIDDLE","PID MIDDLE",-0.2180f,0.4250f,"mint"),
            ("ZONE_MIDDLE","ZONE MIDDLE",-0.1290f,0.4250f,"mint"),
            ("PID_EDGE","PID EDGE",-0.0400f,0.4250f,"mint"),
            ("ZONE_EDGE","ZONE EDGE",0.0490f,0.4250f,"mint"),
            ("PID_RING","PID RING",0.1380f,0.4250f,"mint"),
            ("RING_PRESSURE","RING PRESS",0.2270f,0.4250f,"mint"),
            ("CENTER_MEAS","CENTER MEAS",0.3160f,0.4250f,"mint"),
            ("IO_CENTER","IO CENTER",0.4050f,0.4250f,"mint"),
            ("MIDDLE_MEAS","MIDDLE MEAS",0.4940f,0.4250f,"mint"),
            ("IO_MIDDLE","IO MIDDLE",0.5830f,0.4250f,"mint"),
            ("EDGE_MEAS","EDGE MEAS",0.6720f,0.4250f,"mint"),
            ("IO_EDGE","IO EDGE",0.7610f,0.4250f,"mint"),
            ("RING_MEAS","RING MEAS",0.8500f,0.4250f,"mint"),
            ("IO_RING","IO RING",0.9390f,0.4250f,"mint"),
            ("GIMBAL_POS","GIMBAL POS",-1.0000f,-0.3550f,"mint"),
            ("IO_GIMBAL","IO GIMBAL",-0.9250f,-0.3550f,"mint"),
            ("FORCE_ZERO","FORCE ZERO",-0.8500f,-0.3550f,"mint"),
            ("PID_FORCE_ZERO","PID FORCE ZERO",-0.7750f,-0.3550f,"mint"),
            ("FORCE_VALID","FORCE VALID",-0.7000f,-0.3550f,"mint"),
            ("PLC_FORCE_VALID","PLC FORCE VALID",-0.6250f,-0.3550f,"mint"),
            ("OVERFORCE","OVERFORCE",-0.5500f,-0.3550f,"mint"),
            ("PLC_FORCE_TRIP","PLC FORCE TRIP",-0.4750f,-0.3550f,"amber"),
            ("FORCE_LIMIT","FORCE LIMIT",-0.4000f,-0.3550f,"mint"),
            ("PID_FORCE_LIMIT","PID FORCE LIMIT",-0.3250f,-0.3550f,"mint"),
            ("PLC_SLURRY","PLC SLURRY",-0.2500f,-0.3550f,"mint"),
            ("PUMP_RUN","PUMP RUN",-0.1750f,-0.3550f,"mint"),
            ("SLURRY_SPEED","SLURRY SPEED",-0.1000f,-0.3550f,"mint"),
            ("PUMP_CMD","PUMP CMD",-0.0250f,-0.3550f,"mint"),
            ("PUMP_TACH","PUMP TACH",0.0500f,-0.3550f,"mint"),
            ("IO_PUMP_RPM","IO PUMP RPM",0.1250f,-0.3550f,"mint"),
            ("SLURRY_FLOW","SLURRY FLOW",0.2000f,-0.3550f,"mint"),
            ("IO_FLOW","IO FLOW",0.2750f,-0.3550f,"mint"),
            ("SLURRY_TEMP","SLURRY TEMP",0.3500f,-0.3550f,"mint"),
            ("IO_SL_TEMP","IO SL TEMP",0.4250f,-0.3550f,"mint"),
            ("CONCENTRATION","CONCENTRATION",0.5000f,-0.3550f,"mint"),
            ("IO_CONC","IO CONC",0.5750f,-0.3550f,"mint"),
            ("SLURRY_LEVEL","SLURRY LEVEL",0.6500f,-0.3550f,"mint"),
            ("IO_TANK","IO TANK",0.7250f,-0.3550f,"mint"),
            ("PLC_MIXER","PLC MIXER",0.8000f,-0.3550f,"mint"),
            ("MIXER_RUN","MIXER RUN",0.8750f,-0.3550f,"mint"),
            ("PLC_NOZZLE","PLC NOZZLE",0.9500f,-0.3550f,"mint"),
            ("NOZZLE_CMD","NOZZLE CMD",1.0250f,-0.3550f,"mint"),
            ("NOZZLE_READY","NOZZLE READY",-1.0000f,-0.5250f,"mint"),
            ("IO_NOZZLE","IO NOZZLE",-0.9285f,-0.5250f,"mint"),
            ("PLC_COOL","PLC COOL",-0.8570f,-0.5250f,"mint"),
            ("COOL_RUN","COOL RUN",-0.7855f,-0.5250f,"mint"),
            ("COOL_READY","COOL READY",-0.7140f,-0.5250f,"mint"),
            ("IO_COOL","IO COOL",-0.6425f,-0.5250f,"mint"),
            ("INTERFACE_TEMP","INTERF TEMP",-0.5710f,-0.5250f,"mint"),
            ("IO_INTERFACE","IO INTERF",-0.4995f,-0.5250f,"mint"),
            ("WASTE_LEVEL","WASTE LEVEL",-0.4280f,-0.5250f,"mint"),
            ("IO_WASTE","IO WASTE",-0.3565f,-0.5250f,"mint"),
            ("PLC_CONDITION","PLC CONDITION",-0.2850f,-0.5250f,"mint"),
            ("COND_RUN","COND RUN",-0.2135f,-0.5250f,"mint"),
            ("PLC_COND_ARM","PLC COND ARM",-0.1420f,-0.5250f,"mint"),
            ("COND_ARM_CMD","COND ARM CMD",-0.0705f,-0.5250f,"mint"),
            ("COND_ARM_POS","COND ARM POS",0.0010f,-0.5250f,"mint"),
            ("IO_COND_ARM","IO COND ARM",0.0725f,-0.5250f,"mint"),
            ("PAD_WEAR","PAD WEAR",0.1440f,-0.5250f,"mint"),
            ("IO_PAD_WEAR","IO PAD WEAR",0.2155f,-0.5250f,"mint"),
            ("DISC_RUN","DISC RUN",0.2870f,-0.5250f,"mint"),
            ("COND_MOTOR_RUN","COND MOTOR RUN",0.3585f,-0.5250f,"mint"),
            ("PAD_SCAN_EN","PAD SCAN EN",0.4300f,-0.5250f,"mint"),
            ("PLC_PAD_WARN","PLC PAD WARN",0.5015f,-0.5250f,"mint"),
            ("PAD_DRAG_MON","PAD DRAG MON",0.5730f,-0.5250f,"mint"),
            ("PAD_WET_MON","PAD WET MON",0.6445f,-0.5250f,"mint"),
            ("PAD_FLAT_MON","PAD FLAT MON",0.7160f,-0.5250f,"mint"),
            ("PAD_TEMP_MON","PAD TEMP MON",0.7875f,-0.5250f,"mint"),
            ("OPTICAL_EN","OPT EN",0.8590f,-0.5250f,"mint"),
            ("METRO_ARM","METRO ARM",0.9305f,-0.5250f,"mint"),
            ("OPTICAL_REF","OPT REF",1.0020f,-0.5250f,"mint"),
            ("EDDY_EN","EDDY EN",-1.0000f,-0.7000f,"mint"),
            ("ENDPOINT_CURRENT","ENDPOINT CURR",-0.9285f,-0.7000f,"mint"),
            ("ENDPOINT_LOAD","ENDPOINT LOAD",-0.8570f,-0.7000f,"mint"),
            ("PROFILE_SCAN","PROFILE SCAN",-0.7855f,-0.7000f,"mint"),
            ("PROFILE_VALID","PROFILE VALID",-0.7140f,-0.7000f,"mint"),
            ("TOPOGRAPHY_IN","TOPO IN",-0.6425f,-0.7000f,"mint"),
            ("DEFECT_INSPECT","DEFECT INSPECT",-0.5710f,-0.7000f,"mint"),
            ("DISHING_MON","DISHING MON",-0.4995f,-0.7000f,"mint"),
            ("EROSION_MON","EROSION MON",-0.4280f,-0.7000f,"mint"),
            ("ESTOP_NC","ESTOP NC",-0.3565f,-0.7000f,"amber"),
            ("SAFE_STOP_IN","SAFE STOP IN",-0.2850f,-0.7000f,"amber"),
            ("PLC_SAFE","PLC SAFE",-0.2135f,-0.7000f,"amber"),
            ("GUARD_LOCK","GUARD LOCK",-0.1420f,-0.7000f,"mint"),
            ("ROBOT_TRANSFER","ROBOT TRANSFER",-0.0705f,-0.7000f,"mint"),
            ("WAFER_LIFT","WAFER LIFT",0.0010f,-0.7000f,"mint"),
            ("RINSE_ENABLE","RINSE ENABLE",0.0725f,-0.7000f,"mint"),
            ("BRUSH_EN","BRUSH EN",0.1440f,-0.7000f,"mint"),
            ("SPIN_DRY_EN","SPIN DRY EN",0.2155f,-0.7000f,"mint"),
            ("CARRIER_CLAMP","CAR CLAMP",0.2870f,-0.7000f,"mint"),
            ("SAFETY_ALARM","SAFETY ALARM",0.3585f,-0.7000f,"amber"),
            ("HMI_CONTACT","HMI CTCT",0.4300f,-0.7000f,"mint"),
            ("HMI_SLURRY","HMI SLURRY",0.5015f,-0.7000f,"mint"),
            ("HMI_EDGE","HMI EDGE",0.5730f,-0.7000f,"mint"),
            ("HMI_LOAD","HMI LOAD",0.6445f,-0.7000f,"mint"),
            ("HMI_METRO","HMI METRO",0.7160f,-0.7000f,"mint"),
            ("RESET_READY","RESET READY",0.7875f,-0.7000f,"mint"),
            ("HMI_FAULT","HMI FAULT",0.8590f,-0.7000f,"mint"),
            ("CARRIER_DOWNFORCE_SENSOR_OUT","FORCE OUT",0.9305f,-0.7000f,"mint"),
            ("MULTIZONE_PRESSURE_PID_FORCE_FEEDBACK_IN","FORCE FB IN",1.0020f,-0.7000f,"mint"),
        };

        public static IEnumerable<string> TerminalIds => Sockets.Select(socket => socket.id);

        private static readonly Dictionary<string, string> Palette = new()
        {
            {"red","EE485C"},{"blue","1711E2"},{"amber","FFA82B"},{"violet","A282FF"},{"plasma","A45DFF"},
            {"mint","34F6C5"},{"cyan","23DAFD"},{"void","07111C"},{"white","D4F5FF"},{"glass","498BA8"}
        };

        [MenuItem("Tools/Project Spark/Levels/Create Level 73")]
        public static void Install()
        {
            SparkLevel73Builder.Create();
            var root = AssetDatabase.LoadAssetAtPath<GameObject>(Prefab);
            if (root == null || root.GetComponent<SparkLevel73Process>().controllerDisplay == null) Fit();
            Register();
            ValidateAssets();
        }

        [MenuItem("Tools/Project Spark/Level 73/Validate Level Assets")]
        public static void ValidateAssets()
        {
            var root=AssetDatabase.LoadAssetAtPath<GameObject>(Prefab);
            var control=root != null ? root.GetComponent<SparkLevel73Process>() : null;
            var bindings=root != null ? root.GetComponent<SparkLevelSceneBindings>() : null;
            if (control==null || bindings==null || control.controllerDisplay==null || control.qualityDisplay==null || control.rig==null || control.rig.pivots.Length!=35 ||
                control.pins.Length!=152 || control.pins.Distinct().Count()!=152 || 
                !TerminalIds.SequenceEqual(control.pins.Select(pin=>pin.name)) ||
                control.pins.Any(pin=>!bindings.TryGetTerminal(pin.name,out var bound) || bound!=pin) ||
                SparkLevel73Process.Gates.Any(id=>!bindings.IsComponentIdDeclared(id)))
                throw new InvalidOperationException("Level 73 wiring, motion or binding references are incomplete.");
            Debug.Log("[LEVEL 73] Validated 152 distinct terminals, 98 player wires and 6 evaluator gates.",root);
        }

        private static void Register()
        {
            var scene = SceneManager.GetSceneByPath(ScenePath);
            bool opened = !scene.isLoaded;
            if (opened) scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
            bool dirty = scene.isDirty;
            var objects = scene.GetRootGameObjects();
            T Find<T>() where T : Component => objects.SelectMany(item => item.GetComponentsInChildren<T>(true)).Single();
            var level = AssetDatabase.LoadAssetAtPath<SparkLevelDefinition>(Folder + "Level_73.asset");
            var drawing = AssetDatabase.LoadAssetAtPath<SparkLevelDrawingAsset>(Folder + "Level_73_Drawing.asset");
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
            else if (dirty) Debug.Log("MainMenu had unsaved edits. Save the scene to persist Level 73's campaign and teaching entries.");
        }

        private static void Append(Object target, string field, Object value)
        {
            if (value == null) throw new InvalidOperationException("Missing Level 73 asset for " + field);
            var data = new SerializedObject(target);
            var list = data.FindProperty(field);
            for (int i = 0; i < list.arraySize; i++)
                if (list.GetArrayElementAtIndex(i).objectReferenceValue == value) return;
            list.arraySize++;
            list.GetArrayElementAtIndex(list.arraySize - 1).objectReferenceValue = value;
            data.ApplyModifiedProperties();
        }

        [MenuItem("Tools/Project Spark/Level 73/Fit Breadboard Assembly")]
        public static void Fit()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Exit Play Mode before fitting Level 73.");
            var models = Models.Select((name, index) =>
            {
                string path = Folder + $"Models/L73_{index:00}_{name}_Fitted.fbx";
                return AssetDatabase.LoadAssetAtPath<GameObject>(path) ??
                    throw new InvalidOperationException("Missing fitted kit module " + path);
            }).ToArray();
            if (AssetDatabase.LoadAssetAtPath<GameObject>(Prefab) == null)
                throw new InvalidOperationException("The Level 73 prefab is missing.");

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
                    throw new InvalidOperationException("Unity could not save the Level 73 layout.");
                AssetDatabase.SaveAssets();
                Debug.Log("Level 73 fitted to the MainMenu breadboard with 152 seated terminals.");
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
            var bench = root.transform.Find("CMP Workbench");
            var bindings = root.GetComponent<SparkLevelSceneBindings>();
            var loop = root.GetComponent<SparkLevel73Process>();
            if (bench == null || bindings == null || loop == null)
                throw new InvalidOperationException("Level 73 is missing its workbench, controls, or scene bindings.");

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

            foreach (string name in new[] {"L73_HD_VISUAL_ART", "Breadboard Labels", "Motion pivots"})
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
            var assembly = Child(bench, "L73_HD_VISUAL_ART", new Vector3(0,floor,-.021f));
            foreach (var model in models)
            {
                var instance = Object.Instantiate(model, assembly, false).transform;
                var markers = instance.GetComponentsInChildren<Transform>(true)
                    .Where(item => item.name.StartsWith("L73_Board",StringComparison.Ordinal))
                    .ToDictionary(item => item.name);
                Transform Marker(string name) => markers["L73_Board" + name];
                Vector3 Axis(string name) => assembly.InverseTransformVector(Marker(name).position - Marker("Center").position);
                instance.localRotation = Quaternion.Inverse(Quaternion.LookRotation(-Axis("Front"), Axis("Up"))) * instance.localRotation;
                instance.localScale /= Axis("Right").magnitude;
                instance.localPosition -= assembly.InverseTransformPoint(Marker("Center").position);
                if (Axis("Right").x < .99f)
                    throw new InvalidOperationException("Unexpected axes on fitted model " + model.name);
            }

            assembly.localScale = Vector3.one * .74f;
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
                var oldControls = root.transform.Find("Level 73 Controls");
                if (oldControls != null) Object.DestroyImmediate(oldControls.gameObject);
                SparkLevel73Builder.BuildControls(root.transform,loop,font);
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
                terminalData.FindProperty("maxConnections").intValue = SparkLevel73Process.Wires.Count(wire => wire.a == index || wire.b == index);
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






                ("DRIVES / ZONES",-.865f,.147f),("WAFER / METROLOGY",.862f,.147f),
                ("PAD / SLURRY",-.86f,-.235f),("SAFETY / PLC",.86f,-.235f)
            };
            foreach(var title in titles)
                Label(labels,font,title.title,title.title,new Vector3(title.x,floor+.058f,title.z),.0165f);
            Label(labels,font,"Platen label","POLISHING PLATEN",new Vector3(-.092f,floor+.30f,.113f),.012f);
            Label(labels,font,"Rinse label","RINSE / DRY",new Vector3(.41f,floor+.23f,-.018f),.012f);
            Transform Pivot(string module,string key)
            {
                var name="fitted_"+module+"_"+key;
                return assembly.GetComponentsInChildren<Transform>(true).Single(item=>item.name==name);
            }
            loop.rig.art=assembly;
            loop.rig.pivots=new[]{
                Pivot("RotatingPolishingPlaten","ANIM_PLATEN_ROTATION"),
                Pivot("GroovedPolishingPad","ANIM_PAD_ROTATION"),
                Pivot("PlatenServoMotorDrive","ANIM_PLATEN_SERVO_ROTOR"),
                Pivot("PrecisionWaferCarrierHead","ANIM_CARRIER_ROTATION"),
                Pivot("PrecisionWaferCarrierHead","ANIM_CARRIER_Z"),
                Pivot("CarrierGimbalAssembly","ANIM_CARRIER_GIMBAL"),
                Pivot("MultiZonePressureBladder","ANIM_PRESSURE_ZONE_INDICATOR"),
                Pivot("WaferRetainingRing","ANIM_RETAINING_RING"),
                Pivot("PatternedThinFilmLayer","ANIM_FILM_THICKNESS"),
                Pivot("MicroscopicSurfaceTopography","ANIM_TOPOGRAPHY_DISPLAY"),
                Pivot("AutomatedWaferTransferRobot","ANIM_WAFER_ROBOT_BASE"),
                Pivot("AutomatedWaferTransferRobot","ANIM_WAFER_ROBOT_ARM"),
                Pivot("AutomatedWaferTransferRobot","ANIM_WAFER_ROBOT_TOOL"),
                Pivot("WaferLiftAndAlignmentAssembly","ANIM_WAFER_LIFT"),
                Pivot("AbrasiveSlurryMixingTank","ANIM_SLURRY_MIXER"),
                Pivot("PrecisionSlurryMeteringPump","ANIM_SLURRY_PUMP"),
                Pivot("MotorizedSlurryDispensingArm","ANIM_SLURRY_ARM"),
                Pivot("PrecisionSlurryNozzle","ANIM_SLURRY_NOZZLE"),
                Pivot("PadConditioningSwingArm","ANIM_CONDITIONER_ARM"),
                Pivot("PadConditionerAbrasiveDisc","ANIM_CONDITIONER_DISC"),
                Pivot("PadConditionerMotorDrive","ANIM_CONDITIONER_MOTOR"),
                Pivot("PadWearMeasurementSensor","ANIM_PAD_WEAR_PROBE"),
                Pivot("PadGrooveInspectionScanner","ANIM_PAD_SCANNER"),
                Pivot("CarrierDownforceLoadCell","ANIM_DOWNFORCE_INDICATOR"),
                Pivot("PlatenSpeedEncoder","ANIM_PLATEN_ENCODER_DISC"),
                Pivot("CarrierSpeedEncoder","ANIM_CARRIER_ENCODER_DISC"),
                Pivot("PolishingCoolingLoop","ANIM_COOLING_FAN"),
                Pivot("CMPEmergencySafetyStation","ANIM_ESTOP"),
                Pivot("WaferBrushCleaningAssembly","ANIM_CLEANING_BRUSH"),
                Pivot("SpinRinseDryer","ANIM_SPIN_DRYER"),
                Pivot("InSituOpticalEndpointSensor","ANIM_ENDPOINT_SENSOR"),
                Pivot("EddyCurrentThicknessSensor","ANIM_EDDY_CURRENT_HEAD"),
                Pivot("OpticalSurfaceProfilometer","ANIM_PROFILOMETER"),
                Pivot("WaferDefectInspectionCamera","ANIM_DEFECT_CAMERA"),
                Pivot("HolographicCMPSCADA","ANIM_CMP_SCADA"),
            };
            var pivots=loop.rig.pivots;
            pivots[3].SetParent(pivots[4],true);
            pivots[11].SetParent(pivots[10],true);
            pivots[12].SetParent(pivots[11],true);
            loop.rig.wafer=assembly.GetComponentsInChildren<Transform>(true).Single(item=>item.name=="L73_SiliconWaferSubstrate");
            var carried=GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            carried.name="Transfer wafer"; carried.transform.SetParent(pivots[12],false);
            Object.DestroyImmediate(carried.GetComponent<Collider>());
            carried.transform.rotation=assembly.rotation;
            carried.transform.localScale=Vector3.Scale(new Vector3(.058f,.004f,.058f),new Vector3(
                assembly.lossyScale.x/pivots[12].lossyScale.x, assembly.lossyScale.y/pivots[12].lossyScale.y, assembly.lossyScale.z/pivots[12].lossyScale.z));
            carried.GetComponent<Renderer>().sharedMaterial=MaterialFor(shader,"cyan");
            loop.rig.carried=carried.transform;
            carried.SetActive(false);
            if(loop.plot.transform.Find("Trend legend") == null)
            {
                var oldPanel = root.transform.Find("Level 73 Controls");
                if (oldPanel != null) Object.DestroyImmediate(oldPanel.gameObject);
                SparkLevel73Builder.BuildControls(root.transform,loop,font);
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
            label.fontSize = 73;
            label.characterSize = size;
            label.anchor = TextAnchor.MiddleCenter;
            label.color = new Color(.89f, .96f, 1f);
            label.text = text;
            child.GetComponent<MeshRenderer>().sharedMaterial = font.material;
            return label;
        }

        private static Material MaterialFor(Shader shader, string key)
        {
            string path = $"{Materials}/L73 {key}.mat";
            if (key.StartsWith("tint",StringComparison.Ordinal) && key.Length==10) Palette[key]=key.Substring(4);
            var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing != null && key is not ("glass" or "acrylic") &&
                ColorUtility.TryParseHtmlString("#" + (Palette.TryGetValue(key, out var savedTint) ? savedTint : "9FB3C8"), out var savedColor) &&
                existing.GetColor("_BaseColor") == savedColor) return existing;
            ColorUtility.TryParseHtmlString("#" + (Palette.TryGetValue(key, out var tint) ? tint : "9FB3C8"), out var color);
            var material = existing != null ? existing : new Material(shader) { name = "L73 " + key };
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


