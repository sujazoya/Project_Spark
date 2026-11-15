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
    public static class SparkLevel70ModelInstaller
    {
        public const string Folder = "Assets/My_Assets/_Project_Spark/Prefabs/Levels/Level_70_Layout/";
        public const string Prefab = Folder + "Level_70_Layout.prefab";
        public const string ScenePath = "Assets/My_Assets/_Project_Spark/Scenes/MainMenu/MainMenu.unity";
        private static readonly string[] Models = {
            "IndustrialCleanroomBase","TransparentSemiconductorCleanroom","TransparentCleanroomAirlock","IndustrialCorridorFacade","SemiconductorInspectionBench","SiliconWaferCarrierRack","AutomatedWaferTransferRobot","IndustrialAirHandlingUnit","AirIntakePrefilter","ChilledWaterCoolingCoil","ElectricReheatCoil","PrecisionAirHumidifier","CondensateDrainAssembly","SupplyAirBlowerMotor","ReturnAirBlowerMotor","ExhaustAirBlowerMotor","CeilingFanFilterUnitA","CeilingFanFilterUnitB","ReplaceableHEPACartridgeA","ReplaceableHEPACartridgeB","LowLevelReturnAirGrille","TransparentSupplyDuctNetwork","TransparentReturnDuctNetwork","FreshAirMixingDamper","CleanroomSupplyDamperA","AirlockSupplyDamperB","MotorizedPressureReliefDamper","CleanroomDifferentialPressureSensor","AirlockDifferentialPressureSensor","AirVelocityMeasurementProbe","CleanroomParticleCounter","AirlockParticleCounter","CleanroomTemperatureHumiditySensor","AirlockTemperatureHumiditySensor","AirDewPointSensor","FilterDifferentialPressureTransmitter","IndustrialAirShowerModule","AirlockInnerSafetyDoor","AirlockOuterSafetyDoor","HEPASealFailureSimulator","EmergencyStopAndDoorSafetyStation","AirQualityDisturbanceSimulator","EnvironmentalControlPLCRack","EnvironmentalDigitalIOModule","CascadeAirPressurePIDController","PsychrometricControlModule","HolographicCleanroomSCADAMonitor","Regulated5VControlSupply"
        };
        private const string Materials = Folder + "Models/Materials";
        private const float SocketHeight = .025f;
        private const float SocketDiameter = .026f;

        private static readonly (string id, string label, float x, float z, string color)[] Sockets =
        {
            ("SUPPLY_POS","SUP POS",-0.8500f,0.6800f,"red"),
            ("PLC_5V","PLC 5V",-0.7480f,0.6800f,"red"),
            ("SUPPLY_RET","SUP RET",-0.6460f,0.6800f,"blue"),
            ("PLC_GND","PLC GND",-0.5440f,0.6800f,"blue"),
            ("IO_5V","IO 5V",-0.4420f,0.6800f,"red"),
            ("IO_GND","IO GND",-0.3400f,0.6800f,"blue"),
            ("PID_5V","PID 5V",-0.2380f,0.6800f,"red"),
            ("PID_GND","PID GND",-0.1360f,0.6800f,"blue"),
            ("PSY_5V","PSY 5V",-0.0340f,0.6800f,"red"),
            ("PSY_GND","PSY GND",0.0680f,0.6800f,"blue"),
            ("PLC_SUP_RUN","PLC SUP RUN",0.1700f,0.6800f,"mint"),
            ("SUP_RUN_IN","SUP RUN IN",0.2720f,0.6800f,"mint"),
            ("PID_SUP_CMD","PID SUP CMD",0.3740f,0.6800f,"mint"),
            ("SUP_SPEED_IN","SUP SPEED IN",0.4760f,0.6800f,"mint"),
            ("SUP_TACH","SUP TACH",0.5780f,0.6800f,"mint"),
            ("IO_SUP_RPM","IO SUP RPM",0.6800f,0.6800f,"mint"),
            ("PLC_RET_RUN","PLC RET RUN",0.7820f,0.6800f,"mint"),
            ("RET_RUN_IN","RET RUN IN",0.8840f,0.6800f,"mint"),
            ("RET_REF","RET REF",-0.9200f,0.5250f,"mint"),
            ("RET_SPEED_IN","RET SPEED IN",-0.8150f,0.5250f,"mint"),
            ("RET_TACH","RET TACH",-0.7100f,0.5250f,"mint"),
            ("IO_RET_RPM","IO RET RPM",-0.6050f,0.5250f,"mint"),
            ("PLC_EXH_RUN","PLC EXH RUN",-0.5000f,0.5250f,"mint"),
            ("EXH_RUN_IN","EXH RUN IN",-0.3950f,0.5250f,"mint"),
            ("EXH_REF","EXH REF",-0.2900f,0.5250f,"mint"),
            ("EXH_SPEED_IN","EXH SPEED IN",-0.1850f,0.5250f,"mint"),
            ("EXH_TACH","EXH TACH",-0.0800f,0.5250f,"mint"),
            ("IO_EXH_RPM","IO EXH RPM",0.0250f,0.5250f,"mint"),
            ("PLC_FFU_A","PLC FFU A",0.1300f,0.5250f,"mint"),
            ("FFU_A_EN","FFU A EN",0.2350f,0.5250f,"mint"),
            ("PLC_FFU_B","PLC FFU B",0.3400f,0.5250f,"mint"),
            ("FFU_B_EN","FFU B EN",0.4450f,0.5250f,"mint"),
            ("FFU_STATUS","FFU STATUS",0.5500f,0.5250f,"mint"),
            ("PLC_FFU_OK","PLC FFU OK",0.6550f,0.5250f,"mint"),
            ("PLC_FRESH","PLC FRESH",0.7600f,0.5250f,"mint"),
            ("FRESH_CMD","FRESH CMD",0.8650f,0.5250f,"mint"),
            ("FRESH_POS","FRESH POS",-0.9200f,0.4250f,"mint"),
            ("IO_FRESH","IO FRESH",-0.8150f,0.4250f,"mint"),
            ("PID_ROOM_A","PID ROOM A",-0.7100f,0.4250f,"mint"),
            ("ROOM_DMP_CMD","ROOM DMP CMD",-0.6050f,0.4250f,"mint"),
            ("ROOM_DMP_POS","ROOM DMP POS",-0.5000f,0.4250f,"mint"),
            ("IO_ROOM_DMP","IO ROOM DMP",-0.3950f,0.4250f,"mint"),
            ("PID_LOCK_B","PID LOCK B",-0.2900f,0.4250f,"mint"),
            ("LOCK_DMP_CMD","LOCK DMP CMD",-0.1850f,0.4250f,"mint"),
            ("LOCK_DMP_POS","LOCK DMP POS",-0.0800f,0.4250f,"mint"),
            ("IO_LOCK_DMP","IO LOCK DMP",0.0250f,0.4250f,"mint"),
            ("PLC_RELIEF","PLC RELIEF",0.1300f,0.4250f,"mint"),
            ("RELIEF_CMD","RELIEF CMD",0.2350f,0.4250f,"mint"),
            ("RELIEF_POS","RELIEF POS",0.3400f,0.4250f,"mint"),
            ("IO_RELIEF","IO RELIEF",0.4450f,0.4250f,"mint"),
            ("DAMPER_PERMIT","DMP PERM",0.5500f,0.4250f,"amber"),
            ("DAMPER_RUN","DMP RUN",0.6550f,0.4250f,"mint"),
            ("DAMPER_DIAG","DMP DIAG",0.7600f,0.4250f,"mint"),
            ("ROOM_DP_5V","ROOM DP 5V",0.8650f,0.4250f,"red"),
            ("ROOM_DP_GND","ROOM DP GND",-1.0000f,-0.3550f,"blue"),
            ("ROOM_DP_MON","ROOM DP MON",-0.9190f,-0.3550f,"mint"),
            ("HMI_ROOM_DP","HMI ROOM DP",-0.8380f,-0.3550f,"mint"),
            ("ROOM_DP_OK","ROOM DP OK",-0.7570f,-0.3550f,"mint"),
            ("PLC_DP_OK","PLC DP OK",-0.6760f,-0.3550f,"mint"),
            ("LOCK_DP_5V","LOCK DP 5V",-0.5950f,-0.3550f,"red"),
            ("LOCK_DP_GND","LOCK DP GND",-0.5140f,-0.3550f,"blue"),
            ("LOCK_DP_OUT","LOCK DP OUT",-0.4330f,-0.3550f,"mint"),
            ("PID_LOCK_IN","PID LOCK IN",-0.3520f,-0.3550f,"mint"),
            ("LOCK_DP_OK","LOCK DP OK",-0.2710f,-0.3550f,"mint"),
            ("IO_LOCK_VALID","IO LOCK VALID",-0.1900f,-0.3550f,"mint"),
            ("VELOCITY_5V","VEL 5V",-0.1090f,-0.3550f,"red"),
            ("VELOCITY_GND","VEL GND",-0.0280f,-0.3550f,"blue"),
            ("AIR_VEL_OUT","AIR VEL OUT",0.0530f,-0.3550f,"mint"),
            ("AIR_PID_IN","AIR PID IN",0.1340f,-0.3550f,"mint"),
            ("AIR_VEL_OK","AIR VEL OK",0.2150f,-0.3550f,"mint"),
            ("PLC_AIR_OK","PLC AIR OK",0.2960f,-0.3550f,"mint"),
            ("PART_ROOM_5V","PART ROOM 5V",0.3770f,-0.3550f,"red"),
            ("PART_ROOM_GND","PART ROOM GND",0.4580f,-0.3550f,"blue"),
            ("ROOM_PART_OUT","RM P OUT",0.5390f,-0.3550f,"mint"),
            ("PLC_ROOM_PART","PLC RM P",0.6200f,-0.3550f,"mint"),
            ("ROOM_PART_OK","RM P OK",0.7010f,-0.3550f,"mint"),
            ("IO_ROOM_PART","IO RM P",0.7820f,-0.3550f,"mint"),
            ("PART_LOCK_5V","PART LOCK 5V",0.8630f,-0.3550f,"red"),
            ("PART_LOCK_GND","PART LOCK GND",0.9440f,-0.3550f,"blue"),
            ("LOCK_PART_OUT","LK P OUT",1.0250f,-0.3550f,"mint"),
            ("PLC_LOCK_PART","PLC LK P",-1.0000f,-0.5250f,"mint"),
            ("LOCK_PART_OK","LK P OK",-0.9230f,-0.5250f,"mint"),
            ("IO_LOCK_PART","IO LK P",-0.8460f,-0.5250f,"mint"),
            ("FILTER_DP_OUT","FLT DP OUT",-0.7690f,-0.5250f,"mint"),
            ("HEPA_DP_IN","HEPA DP IN",-0.6920f,-0.5250f,"mint"),
            ("FILTER_DP_OK","FLT DP OK",-0.6150f,-0.5250f,"mint"),
            ("IO_HEPA_DP","IO HEPA DP",-0.5380f,-0.5250f,"mint"),
            ("ROOM_TEMP","ROOM TEMP",-0.4610f,-0.5250f,"mint"),
            ("PSY_TEMP","PSY TEMP",-0.3840f,-0.5250f,"mint"),
            ("ROOM_RH","ROOM RH",-0.3070f,-0.5250f,"mint"),
            ("PSY_RH","PSY RH",-0.2300f,-0.5250f,"mint"),
            ("LOCK_TEMP","LOCK TEMP",-0.1530f,-0.5250f,"mint"),
            ("IO_LOCK_TEMP","IO LOCK TEMP",-0.0760f,-0.5250f,"mint"),
            ("LOCK_RH","LOCK RH",0.0010f,-0.5250f,"mint"),
            ("IO_LOCK_RH","IO LOCK RH",0.0780f,-0.5250f,"mint"),
            ("DEW_POINT","DEW POINT",0.1550f,-0.5250f,"mint"),
            ("PSY_DEW","PSY DEW",0.2320f,-0.5250f,"mint"),
            ("PSY_COOL","PSY COOL",0.3090f,-0.5250f,"mint"),
            ("COIL_CMD","COIL CMD",0.3860f,-0.5250f,"mint"),
            ("PSY_REHEAT","PSY REHEAT",0.4630f,-0.5250f,"mint"),
            ("REHEAT_CMD","REHEAT CMD",0.5400f,-0.5250f,"mint"),
            ("PSY_HUM","PSY HUM",0.6170f,-0.5250f,"mint"),
            ("HUM_CMD","HUM CMD",0.6940f,-0.5250f,"mint"),
            ("HUM_ACTIVE","HUM ACTIVE",0.7710f,-0.5250f,"mint"),
            ("IO_HUM","IO HUM",0.8480f,-0.5250f,"mint"),
            ("DRAIN_OK","DRAIN OK",0.9250f,-0.5250f,"mint"),
            ("IO_DRAIN","IO DRAIN",1.0020f,-0.5250f,"mint"),
            ("INNER_LOCK","INNER LOCK",-1.0000f,-0.7000f,"mint"),
            ("INNER_CLOSED","INNER CLOSED",-0.9230f,-0.7000f,"mint"),
            ("OUTER_LOCK","OUTER LOCK",-0.8460f,-0.7000f,"mint"),
            ("OUTER_CLOSED","OUTER CLOSED",-0.7690f,-0.7000f,"mint"),
            ("DOOR_FAULT","DOOR FAULT",-0.6920f,-0.7000f,"amber"),
            ("ESTOP_NC","ESTOP NC",-0.6150f,-0.7000f,"amber"),
            ("SAFE_STOP_IN","SAFE STOP IN",-0.5380f,-0.7000f,"amber"),
            ("SAFE_PLC_PERMIT","SAFE PLC PERM",-0.4610f,-0.7000f,"amber"),
            ("ROBOT_PERMIT","ROBOT PERM",-0.3840f,-0.7000f,"amber"),
            ("ROBOT_ENABLE","ROBOT EN",-0.3070f,-0.7000f,"mint"),
            ("ROBOT_START","ROBOT START",-0.2300f,-0.7000f,"mint"),
            ("ROBOT_BATCH","ROBOT BATCH",-0.1530f,-0.7000f,"mint"),
            ("SHOWER_EN","SHOWER EN",-0.0760f,-0.7000f,"mint"),
            ("SHOWER_READY","SHOWER READY",0.0010f,-0.7000f,"mint"),
            ("PLC_DP_SET","PLC DP SET",0.0780f,-0.7000f,"mint"),
            ("HMI_AIR_REQ","HMI AIR REQ",0.1550f,-0.7000f,"mint"),
            ("HMI_ROOM_REQ","HMI ROOM REQ",0.2320f,-0.7000f,"mint"),
            ("HMI_LOCK_REQ","HMI LOCK REQ",0.3090f,-0.7000f,"mint"),
            ("PLC_PSY_STATUS","PLC PSY STATUS",0.3860f,-0.7000f,"mint"),
            ("CONTAM_TRIP","CONTAM TRIP",0.4630f,-0.7000f,"amber"),
            ("BYPASS_TRIP","BYPASS TRIP",0.5400f,-0.7000f,"amber"),
            ("SCENARIO_IO","SCENARIO IO",0.6170f,-0.7000f,"mint"),
            ("HMI_STATUS","HMI STATUS",0.6940f,-0.7000f,"mint"),
            ("HMI_OPERATOR","HMI OPERATOR",0.7710f,-0.7000f,"mint"),
            ("SAFETY_RESET","SAFETY RESET",0.8480f,-0.7000f,"amber"),
            ("ROOM_DP_OUT","ROOM DP OUT",0.9250f,-0.7000f,"mint"),
            ("CASCADE_DP_IN","CAS DP IN",1.0020f,-0.7000f,"mint"),
        };

        public static IEnumerable<string> TerminalIds => Sockets.Select(socket => socket.id);

        private static readonly Dictionary<string, string> Palette = new()
        {
            {"red","EE485C"},{"blue","1701E2"},{"amber","FFA82B"},{"violet","A282FF"},
            {"mint","34F6C5"},{"cyan","23DAFD"},{"void","07111C"},{"white","D4F5FF"},{"glass","498BA8"}
        };

        [MenuItem("Tools/Project Spark/Levels/Create Level 70")]
        public static void Install()
        {
            SparkLevel70Builder.Create();
            var root = AssetDatabase.LoadAssetAtPath<GameObject>(Prefab);
            if (root == null || root.GetComponent<SparkLevel70Process>().controllerDisplay == null) Fit();
            Register();
            ValidateAssets();
        }

        [MenuItem("Tools/Project Spark/Level 70/Validate Level Assets")]
        public static void ValidateAssets()
        {
            var root=AssetDatabase.LoadAssetAtPath<GameObject>(Prefab);
            var control=root != null ? root.GetComponent<SparkLevel70Process>() : null;
            var bindings=root != null ? root.GetComponent<SparkLevelSceneBindings>() : null;
            if (control==null || bindings==null || control.controllerDisplay==null || control.qualityDisplay==null || control.rig==null || control.rig.pivots.Length!=19 ||
                control.pins.Length!=134 || control.pins.Distinct().Count()!=134 || 
                !TerminalIds.SequenceEqual(control.pins.Select(pin=>pin.name)) ||
                control.pins.Any(pin=>!bindings.TryGetTerminal(pin.name,out var bound) || bound!=pin) ||
                SparkLevel70Process.Gates.Any(id=>!bindings.IsComponentIdDeclared(id)))
                throw new InvalidOperationException("Level 70 wiring, motion or binding references are incomplete.");
            Debug.Log("[LEVEL 70] Validated 134 distinct terminals, 86 player wires and 6 evaluator gates.",root);
        }

        private static void Register()
        {
            var scene = SceneManager.GetSceneByPath(ScenePath);
            bool opened = !scene.isLoaded;
            if (opened) scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
            bool dirty = scene.isDirty;
            var objects = scene.GetRootGameObjects();
            T Find<T>() where T : Component => objects.SelectMany(item => item.GetComponentsInChildren<T>(true)).Single();
            var level = AssetDatabase.LoadAssetAtPath<SparkLevelDefinition>(Folder + "Level_70.asset");
            var drawing = AssetDatabase.LoadAssetAtPath<SparkLevelDrawingAsset>(Folder + "Level_70_Drawing.asset");
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
            else if (dirty) Debug.Log("MainMenu had unsaved edits. Save the scene to persist Level 70's campaign and teaching entries.");
        }

        private static void Append(Object target, string field, Object value)
        {
            if (value == null) throw new InvalidOperationException("Missing Level 70 asset for " + field);
            var data = new SerializedObject(target);
            var list = data.FindProperty(field);
            for (int i = 0; i < list.arraySize; i++)
                if (list.GetArrayElementAtIndex(i).objectReferenceValue == value) return;
            list.arraySize++;
            list.GetArrayElementAtIndex(list.arraySize - 1).objectReferenceValue = value;
            data.ApplyModifiedProperties();
        }

        [MenuItem("Tools/Project Spark/Level 70/Fit Breadboard Assembly")]
        public static void Fit()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Exit Play Mode before fitting Level 70.");
            var models = Models.Select((name, index) =>
            {
                string path = Folder + $"Models/L70_{index:00}_{name}_Fitted.fbx";
                return AssetDatabase.LoadAssetAtPath<GameObject>(path) ??
                    throw new InvalidOperationException("Missing fitted kit module " + path);
            }).ToArray();
            if (AssetDatabase.LoadAssetAtPath<GameObject>(Prefab) == null)
                throw new InvalidOperationException("The Level 70 prefab is missing.");

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
                    throw new InvalidOperationException("Unity could not save the Level 70 layout.");
                AssetDatabase.SaveAssets();
                Debug.Log("Level 70 fitted to the MainMenu breadboard with 134 seated terminals.");
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
            Vector3 right = Vector3.ProjectOnPlane(grid.GridOrigin.right, up).normalized * .070f;
            Vector3 forward = Vector3.ProjectOnPlane(grid.GridOrigin.forward, up).normalized * .070f;
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
            var bench = root.transform.Find("Cleanroom Workbench");
            var bindings = root.GetComponent<SparkLevelSceneBindings>();
            var loop = root.GetComponent<SparkLevel70Process>();
            if (bench == null || bindings == null || loop == null)
                throw new InvalidOperationException("Level 70 is missing its workbench, controls, or scene bindings.");

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

            foreach (string name in new[] {"Fitted Level 70 Models", "Breadboard Labels", "Motion pivots"})
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
            var assembly = Child(bench, "Fitted Level 70 Models", new Vector3(0,floor,.025f));
            foreach (var model in models)
            {
                var instance = Object.Instantiate(model, assembly, false).transform;
                var markers = instance.GetComponentsInChildren<Transform>(true)
                    .Where(item => item.name.StartsWith("L70_Board",StringComparison.Ordinal))
                    .ToDictionary(item => item.name);
                Transform Marker(string name) => markers["L70_Board" + name];
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
                var oldControls = root.transform.Find("Level 70 Controls");
                if (oldControls != null) Object.DestroyImmediate(oldControls.gameObject);
                SparkLevel70Builder.BuildControls(root.transform,loop,font);
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
                terminalData.FindProperty("maxConnections").intValue = SparkLevel70Process.Wires.Count(wire => wire.a == index || wire.b == index);
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
                float offset=terminalBelow?-.032f:label.column%2==0?-.041f:-.070f;
                Label(labels,font,label.id,label.label,label.seat+new Vector3(0,.019f,offset),.0105f);
            }
            foreach (var owner in owners)
                foreach (Transform child in owner)
                    if (child.GetComponent<SparkTerminal>() == null && child.GetComponent<Renderer>() != null)
                        child.gameObject.SetActive(false);

            var titles = new (string title, float x, float z)[]
            {






                ("AIR HANDLING",-.815f,.263f),("PRESSURE CASCADE",.862f,.263f),
                ("ENVIRONMENTAL CONTROL",-.58f,-.265f),("DOORS / WAFER SAFETY",.61f,-.265f)
            };
            foreach(var title in titles)
                Label(labels,font,title.title,title.title,new Vector3(title.x,floor+.058f,title.z),.0165f);
            Label(labels,font,"Cleanroom label","CLEANROOM",new Vector3(-.012f,floor+.345f,.108f),.012f);
            Label(labels,font,"Airlock label","AIRLOCK",new Vector3(.409f,floor+.295f,.102f),.012f);
            Transform Pivot(string module,string key)
            {
                var name="fitted_"+module+"_"+key;
                return assembly.GetComponentsInChildren<Transform>(true).Single(item=>item.name==name);
            }
            loop.rig.pivots=new[]{
                Pivot("SupplyAirBlowerMotor","ANIM_SUPPLY_BLOWER"),
                Pivot("ReturnAirBlowerMotor","ANIM_RETURN_BLOWER"),
                Pivot("ExhaustAirBlowerMotor","ANIM_EXHAUST_BLOWER"),
                Pivot("CeilingFanFilterUnitA","ANIM_FFU_A_FAN"),
                Pivot("CeilingFanFilterUnitB","ANIM_FFU_B_FAN"),
                Pivot("FreshAirMixingDamper","ANIM_FRESH_AIR_DAMPER"),
                Pivot("CleanroomSupplyDamperA","ANIM_SUPPLY_DAMPER_A"),
                Pivot("AirlockSupplyDamperB","ANIM_SUPPLY_DAMPER_B"),
                Pivot("MotorizedPressureReliefDamper","ANIM_RELIEF_DAMPER"),
                Pivot("AirlockInnerSafetyDoor","ANIM_INNER_DOOR"),
                Pivot("AirlockOuterSafetyDoor","ANIM_OUTER_DOOR"),
                Pivot("AutomatedWaferTransferRobot","ANIM_WAFER_ROBOT_BASE"),
                Pivot("AutomatedWaferTransferRobot","ANIM_WAFER_ROBOT_ARM"),
                Pivot("AutomatedWaferTransferRobot","ANIM_WAFER_ROBOT_TOOL"),
                Pivot("IndustrialAirShowerModule","ANIM_AIR_SHOWER"),
                Pivot("ReplaceableHEPACartridgeA","ANIM_HEPA_SERVICE_LATCH_A"),
                Pivot("ReplaceableHEPACartridgeB","ANIM_HEPA_SERVICE_LATCH_B"),
                Pivot("HolographicCleanroomSCADAMonitor","ANIM_CLEANROOM_SCADA"),
                Pivot("CleanroomParticleCounter","ANIM_PARTICLE_SAMPLER"),
            };
            var pivots=loop.rig.pivots;
            pivots[12].SetParent(pivots[11],true);
            pivots[13].SetParent(pivots[12],true);
            loop.rig.secondFfuA=Pivot("CeilingFanFilterUnitA","ANIM_FFU_A_FAN_SECOND");
            loop.rig.secondFfuB=Pivot("CeilingFanFilterUnitB","ANIM_FFU_B_FAN_SECOND");
            loop.rig.cartridgeA=assembly.GetComponentsInChildren<Transform>(true).Single(item=>item.name=="L70_ReplaceableHEPACartridgeA");
            var carried=GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            carried.name="Transfer wafer"; carried.transform.SetParent(pivots[13],false);
            Object.DestroyImmediate(carried.GetComponent<Collider>());
            carried.transform.localScale=new Vector3(.085f,.006f,.085f);
            carried.transform.localPosition=new Vector3(.037f,-.051f,0);
            carried.GetComponent<Renderer>().sharedMaterial=MaterialFor(shader,"cyan");
            carried.SetActive(false);
            loop.rig.wafer=carried.transform;
            loop.rig.flowMarkers=new Transform[6];
            for(int i=0;i<loop.rig.flowMarkers.Length;i++)
            {
                var marker=GameObject.CreatePrimitive(PrimitiveType.Sphere);
                marker.name="Filtered airflow indicator "+(i+1);
                marker.transform.SetParent(assembly,false);
                Object.DestroyImmediate(marker.GetComponent<Collider>());
                marker.GetComponent<Renderer>().sharedMaterial=MaterialFor(shader,"cyan");
                marker.transform.localScale=Vector3.one*.008f;
                marker.SetActive(false);
                loop.rig.flowMarkers[i]=marker.transform;
            }
            if(loop.plot.transform.Find("Trend legend") == null)
            {
                var oldPanel = root.transform.Find("Level 70 Controls");
                if (oldPanel != null) Object.DestroyImmediate(oldPanel.gameObject);
                SparkLevel70Builder.BuildControls(root.transform,loop,font);
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
            loop.controllerDisplay=Readout("Room pressure",new Vector3(-.815f,floor+.075f,.215f),new Color(.46f,.94f,1f));
            loop.qualityDisplay=Readout("Particle status",new Vector3(.862f,floor+.075f,.215f),new Color(.70f,1f,.86f));
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
            label.fontSize = 70;
            label.characterSize = size;
            label.anchor = TextAnchor.MiddleCenter;
            label.color = new Color(.89f, .96f, 1f);
            label.text = text;
            child.GetComponent<MeshRenderer>().sharedMaterial = font.material;
            return label;
        }

        private static Material MaterialFor(Shader shader, string key)
        {
            string path = $"{Materials}/L70 {key}.mat";
            if (key.StartsWith("tint",StringComparison.Ordinal) && key.Length==10) Palette[key]=key.Substring(4);
            var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing != null && key is not ("glass" or "acrylic") &&
                ColorUtility.TryParseHtmlString("#" + (Palette.TryGetValue(key, out var savedTint) ? savedTint : "9FB3C8"), out var savedColor) &&
                existing.GetColor("_BaseColor") == savedColor) return existing;
            ColorUtility.TryParseHtmlString("#" + (Palette.TryGetValue(key, out var tint) ? tint : "9FB3C8"), out var color);
            var material = existing != null ? existing : new Material(shader) { name = "L70 " + key };
            bool metal = key is "chrome" or "titanium" or "steel" or "gold" or "copper" or "aluminum" or "tintCFDCE2" or "tintA4B8C4" or "tintE36C27" or "tintDC9C2F";
            material.SetFloat("_Metallic", metal ? .70f : .02f);
            material.SetFloat("_Smoothness", metal ? .70f : key == "glass" ? .12f : .35f);
            if (key is "green" or "cyan" or "red" or "blue" or "orange" or "amber" or "mint" or "warm" or "ice")
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", color * .70f);
            }
            else if (key.StartsWith("tint", StringComparison.Ordinal))
            {
                material.EnableKeyword("_EMISSION");
                float strength = key is "tint19CCEF" or "tint27D700" or "tintFFCE34" ? .30f : .12f;
                material.SetColor("_EmissionColor", color * strength);
            }
            if (key is "glass" or "glass_body" or "glass_soft" or "acrylic" or "glassblue" or "film" or "Glass" or "guardglass")
            {
                color.a = key=="film" ? .70f : .10f;
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


