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
    public static class SparkLevel78ModelInstaller
    {
        public const string Folder = "Assets/My_Assets/_Project_Spark/Prefabs/Levels/Level_78_Layout/";
        public const string Prefab = Folder + "Level_78_Layout.prefab";
        public const string ScenePath = "Assets/My_Assets/_Project_Spark/Scenes/MainMenu/MainMenu.unity";
        private static readonly string[] Models = {
            "IndustrialDielectricProtectionBase","TransparentMultichamberSafetyEnclosure","InterconnectWaferCassette","PlanarizedCopperInterconnectWafer","CopperInterconnectLayerHologram","HighVacuumWaferLoadLock","PrecisionVacuumGateValve","CentralVacuumTransferHub","MultiAxisVacuumTransferRobot","OpticalWaferAlignmentStage","InterconnectSurfaceConditionScanner","CopperSurfacePreconditioningModule","CopperCapPECVDReactor","CopperCapReactorLid","CeramicCapGasShowerhead","CapPlasmaRFSourceAssembly","CapRFMatchingNetwork","CapElectrodeDistributionAssembly","CapHeatedWaferSusceptor","CapVacuumExhaustThrottle","CapPlasmaSheathVisualization","VirtualCapPrecursorCartridge","CapPrecursorDeliveryManifold","CapGasMassFlowController","CapFilmThicknessInterferometer","CapDepositionUniformityMonitor","CapContinuityInspectionModule","HolographicCopperDiffusionShield","LowKPECVDReactor","LowKDielectricGasShowerhead","LowKPlasmaSource","LowKRFMatchingAssembly","LowKHeatedWaferChuck","VirtualLowKPrecursorCartridge","VirtualPorogenCartridge","DualChannelLowKGasManifold","LowKGasFlowController","PlasmaIonEnergyMonitor","LowKSurfaceChargingSensor","LowKFTIRBondingAnalyzer","LowKFilmGrowthOpticalMonitor","UVThermalCureReactor","MultiZoneUVLampArray","PrecisionUVCureShutter","RotatingCureWaferPlaten","CureWaferTemperatureSensor","CureOutgassingMonitor","HolographicLowKPoreNetwork","WaferStressBowScanner","DielectricEllipsometryStation","InterlineCombTestFixture","DielectricLeakageCurrentElectrometer","AcceleratedBiasStressTestSource","DielectricReliabilityController","HolographicTDDBReliabilityGraph","SurfaceMoistureInspectionAnalyzer","InterconnectRCDelayAnalyzer","EmergencyIsolationSafetyStation","TurbomolecularVacuumPump","VacuumBackingPump","PrecisionChamberPressureGauge","DielectricProcessPLCRack","HolographicDielectricSCADA","Regulated5VControlSupply"
        };
        private const string Materials = Folder + "Models/Materials";
        private const float SocketHeight = .025f;
        private const float SocketDiameter = .026f;

        private static readonly (string id, string label, float x, float z, string color)[] Sockets =
        {
            ("SUPPLY_POS","SUP POS",-1.0000f,0.6800f,"red"),
            ("PLC_5V","PLC 5V",-0.9285f,0.6800f,"red"),
            ("SUPPLY_RET","SUP RET",-0.8570f,0.6800f,"blue"),
            ("PLC_GND","PLC GND",-0.7855f,0.6800f,"blue"),
            ("SCADA_5V","SCADA 5V",-0.7140f,0.6800f,"red"),
            ("SCADA_GND","SCADA GND",-0.6425f,0.6800f,"blue"),
            ("GAS_5V","GAS 5V",-0.5710f,0.6800f,"red"),
            ("GAS_GND","GAS GND",-0.4995f,0.6800f,"blue"),
            ("METER_5V","METER 5V",-0.4280f,0.6800f,"red"),
            ("METER_GND","METER GND",-0.3565f,0.6800f,"blue"),
            ("SAFE_5V","SAFE 5V",-0.2850f,0.6800f,"red"),
            ("SAFE_GND","SAFE GND",-0.2135f,0.6800f,"blue"),
            ("BACK_CMD","BACK CMD",-0.1420f,0.6800f,"mint"),
            ("BACK_RUN","BACK RUN",-0.0705f,0.6800f,"mint"),
            ("TURBO_CMD","TURBO CMD",0.0010f,0.6800f,"mint"),
            ("TURBO_RUN","TURBO RUN",0.0725f,0.6800f,"mint"),
            ("BACK_READY","BACK READY",0.1440f,0.6800f,"mint"),
            ("VAC_BACK_OK","VAC BACK OK",0.2155f,0.6800f,"mint"),
            ("VAC_DIAG","VAC DIAG",0.2870f,0.6800f,"mint"),
            ("PRESSURE_OUT","PRESS OUT",0.3585f,0.6800f,"mint"),
            ("VAC_PRESSURE","VAC PRESS",0.4300f,0.6800f,"mint"),
            ("GATE_CMD","GATE CMD",0.5015f,0.6800f,"mint"),
            ("GATE_DRIVE","GATE DRIVE",0.5730f,0.6800f,"mint"),
            ("GATE_CLOSED","GATE CLOSED",0.6445f,0.6800f,"mint"),
            ("VAC_GATE_OK","VAC GATE OK",0.7160f,0.6800f,"mint"),
            ("LOADLOCK_CMD","LOADLOCK CMD",0.7875f,0.6800f,"mint"),
            ("LOCK_PUMP","LOCK PUMP",0.8590f,0.6800f,"mint"),
            ("LOCK_PRESSURE","LOCK PRESS",0.9305f,0.6800f,"mint"),
            ("VAC_LOCK_OK","VAC LOCK OK",1.0020f,0.6800f,"mint"),
            ("ROBOT_CMD","ROBOT CMD",-1.0000f,0.5450f,"mint"),
            ("ROBOT_DRIVE","ROBOT DRIVE",-0.9285f,0.5450f,"mint"),
            ("ROBOT_HOME","ROBOT HOME",-0.8570f,0.5450f,"mint"),
            ("TRANSFER_HOME","TRANSFER HOME",-0.7855f,0.5450f,"mint"),
            ("ALIGN_CMD","ALIGN CMD",-0.7140f,0.5450f,"mint"),
            ("ALIGN_START","ALIGN START",-0.6425f,0.5450f,"mint"),
            ("ALIGN_OUT","ALIGN OUT",-0.5710f,0.5450f,"mint"),
            ("TRANSFER_ALIGN","TRANSFER ALIGN",-0.4995f,0.5450f,"mint"),
            ("PREP_CMD","PREP CMD",-0.4280f,0.5450f,"mint"),
            ("PREP_START","PREP START",-0.3565f,0.5450f,"mint"),
            ("PREP_OUT","PREP OUT",-0.2850f,0.5450f,"mint"),
            ("TRANSFER_SURFACE","TRANSFER SURFACE",-0.2135f,0.5450f,"mint"),
            ("PREP_DONE","PREP DONE",-0.1420f,0.5450f,"mint"),
            ("PLC_PREP_OK","PLC PREP OK",-0.0705f,0.5450f,"mint"),
            ("VAC_PERMIT","VAC PERMIT",0.0010f,0.5450f,"amber"),
            ("VAC_AUTH","VAC AUTH",0.0725f,0.5450f,"mint"),
            ("HUB_SAFE","HUB SAFE",0.1440f,0.5450f,"amber"),
            ("TRANSFER_SAFE","TRANSFER SAFE",0.2155f,0.5450f,"amber"),
            ("CAP_CMD","CAP CMD",0.2870f,0.5450f,"mint"),
            ("CAP_RUN","CAP RUN",0.3585f,0.5450f,"mint"),
            ("CAP_LID_CMD","CAP LID CMD",0.4300f,0.5450f,"mint"),
            ("CAP_LID","CAP LID",0.5015f,0.5450f,"mint"),
            ("CAP_LID_OUT","CAP LID OUT",0.5730f,0.5450f,"mint"),
            ("CAP_INTERLOCK","CAP INTERLOCK",0.6445f,0.5450f,"mint"),
            ("CAP_FLOW_CMD","CAP FLOW CMD",0.7160f,0.5450f,"mint"),
            ("CAP_MFC_SET","CAP MFC SET",0.7875f,0.5450f,"mint"),
            ("CAP_FLOW_OUT","CAP FLOW OUT",0.8590f,0.5450f,"mint"),
            ("CAP_FLOW_IN","CAP FLOW IN",0.9305f,0.5450f,"mint"),
            ("CAP_RESERVOIR","CAP RESERVOIR",1.0020f,0.5450f,"mint"),
            ("OUT_CAP_FUEL","OUT CAP FUEL",-1.0000f,0.4250f,"mint"),
            ("CAP_MANIFOLD","CAP MANIFOLD",-0.9285f,0.4250f,"mint"),
            ("CAP_DELIVERY","CAP DELIVERY",-0.8570f,0.4250f,"mint"),
            ("CAP_RF_CMD","CAP RF CMD",-0.7855f,0.4250f,"mint"),
            ("CAP_RF_RUN","CAP RF RUN",-0.7140f,0.4250f,"mint"),
            ("CAP_RF_OUT","CAP RF OUT",-0.6425f,0.4250f,"mint"),
            ("CAP_MATCH_IN","CAP MATCH IN",-0.5710f,0.4250f,"mint"),
            ("CAP_MATCH_OUT","CAP MATCH OUT",-0.4995f,0.4250f,"mint"),
            ("CAP_RF_OK","CAP RF OK",-0.4280f,0.4250f,"mint"),
            ("CAP_TEMP_CMD","CAP TEMP CMD",-0.3565f,0.4250f,"mint"),
            ("CAP_HEAT","CAP HEAT",-0.2850f,0.4250f,"mint"),
            ("CAP_TEMP_OUT","CAP TEMP OUT",-0.2135f,0.4250f,"mint"),
            ("CAP_HEAT_OK","CAP HEAT OK",-0.1420f,0.4250f,"mint"),
            ("CAP_THICK_OUT","CAP THICK OUT",-0.0705f,0.4250f,"mint"),
            ("CAP_THICK_IN","CAP THICK IN",0.0010f,0.4250f,"mint"),
            ("CAP_MAP_OUT","CAP MAP OUT",0.0725f,0.4250f,"mint"),
            ("CAP_UNIFORM","CAP UNIFORM",0.1440f,0.4250f,"mint"),
            ("CAP_PINHOLE_OUT","CAP PINHOLE OUT",0.2155f,0.4250f,"mint"),
            ("CAP_DEFECTS","CAP DEFECTS",0.2870f,0.4250f,"mint"),
            ("CAP_EXHAUST_CMD","CAP EXHAUST CMD",0.3585f,0.4250f,"mint"),
            ("CAP_THROTTLE","CAP THROTTLE",0.4300f,0.4250f,"mint"),
            ("LOWK_CMD","LOWK CMD",0.5015f,0.4250f,"mint"),
            ("LOWK_RUN","LOWK RUN",0.5730f,0.4250f,"mint"),
            ("SHOWER_CMD","SHOWER CMD",0.6445f,0.4250f,"mint"),
            ("LOWK_SHOWER","LOWK SHOWER",0.7160f,0.4250f,"mint"),
            ("LOWK_RF_CMD","LOWK RF CMD",0.7875f,0.4250f,"mint"),
            ("LOWK_RF_RUN","LOWK RF RUN",0.8590f,0.4250f,"mint"),
            ("LOWK_RF_OUT","LOWK RF OUT",0.9305f,0.4250f,"mint"),
            ("LOWK_MATCH_IN","LOWK MATCH IN",1.0020f,0.4250f,"mint"),
            ("LOWK_MATCH_OUT","LOWK MATCH OUT",-1.0000f,-0.3550f,"mint"),
            ("LOWK_RF_OK","LOWK RF OK",-0.9330f,-0.3550f,"mint"),
            ("LOWK_TEMP_CMD","LOWK TEMP CMD",-0.8660f,-0.3550f,"mint"),
            ("LOWK_HEAT","LOWK HEAT",-0.7990f,-0.3550f,"mint"),
            ("LOWK_TEMP_OUT","LOWK TEMP OUT",-0.7320f,-0.3550f,"mint"),
            ("LOWK_HEAT_OK","LOWK HEAT OK",-0.6650f,-0.3550f,"mint"),
            ("MATRIX_READY","MATRIX READY",-0.5980f,-0.3550f,"mint"),
            ("LOWK_MATRIX","LOWK MATRIX",-0.5310f,-0.3550f,"mint"),
            ("POROGEN_READY","POROGEN READY",-0.4640f,-0.3550f,"mint"),
            ("LOWK_POROGEN","LOWK POROGEN",-0.3970f,-0.3550f,"mint"),
            ("DUAL_MFC_CMD","DUAL MFC CMD",-0.3300f,-0.3550f,"mint"),
            ("GAS_MIXER","GAS MIXER",-0.2630f,-0.3550f,"mint"),
            ("MIX_FLOW_CMD","MIX FLOW CMD",-0.1960f,-0.3550f,"mint"),
            ("LOWK_MIX_REQ","LOWK MIX REQ",-0.1290f,-0.3550f,"mint"),
            ("LOWK_FLOW_OUT","LOWK FLOW OUT",-0.0620f,-0.3550f,"mint"),
            ("LOWK_FLOW_IN","LOWK FLOW IN",0.0050f,-0.3550f,"mint"),
            ("ION_ENERGY_OUT","ION ENERGY OUT",0.0720f,-0.3550f,"mint"),
            ("DAMAGE_IN","DAMAGE IN",0.1390f,-0.3550f,"mint"),
            ("CHARGE_OUT","CHARGE OUT",0.2060f,-0.3550f,"mint"),
            ("CHARGE_IN","CHARGE IN",0.2730f,-0.3550f,"mint"),
            ("FTIR_OUT","FTIR OUT",0.3400f,-0.3550f,"mint"),
            ("BOND_IN","BOND IN",0.4070f,-0.3550f,"mint"),
            ("LOWK_THICK_OUT","LOWK THICK OUT",0.4740f,-0.3550f,"mint"),
            ("GROWTH_IN","GROWTH IN",0.5410f,-0.3550f,"mint"),
            ("CURE_CMD","CURE CMD",0.6080f,-0.3550f,"mint"),
            ("CURE_RUN","CURE RUN",0.6750f,-0.3550f,"mint"),
            ("CURE_DOOR_OUT","CURE DOOR OUT",0.7420f,-0.3550f,"mint"),
            ("CURE_SAFE","CURE SAFE",0.8090f,-0.3550f,"amber"),
            ("LAMP_CMD","LAMP CMD",0.8760f,-0.3550f,"mint"),
            ("UV_ENABLE","UV ENABLE",0.9430f,-0.3550f,"mint"),
            ("LAMP_OUT","LAMP OUT",1.0100f,-0.3550f,"mint"),
            ("UV_ACTUAL","UV ACTUAL",-1.0000f,-0.5250f,"mint"),
            ("SHUTTER_CMD","SHUTTER CMD",-0.9350f,-0.5250f,"mint"),
            ("SHUTTER_SET","SHUTTER SET",-0.8700f,-0.5250f,"mint"),
            ("SHUTTER_OUT","SHUTTER OUT",-0.8050f,-0.5250f,"mint"),
            ("SHUTTER_POS","SHUTTER POS",-0.7400f,-0.5250f,"mint"),
            ("PLATEN_CMD","PLATEN CMD",-0.6750f,-0.5250f,"mint"),
            ("CURE_SPIN","CURE SPIN",-0.6100f,-0.5250f,"mint"),
            ("PLATEN_OUT","PLATEN OUT",-0.5450f,-0.5250f,"mint"),
            ("CURE_RPM","CURE RPM",-0.4800f,-0.5250f,"mint"),
            ("CURE_TEMP_CMD","CURE TEMP CMD",-0.4150f,-0.5250f,"mint"),
            ("CURE_HEAT","CURE HEAT",-0.3500f,-0.5250f,"mint"),
            ("CURE_TEMP_OUT","CURE TEMP OUT",-0.2850f,-0.5250f,"mint"),
            ("CURE_TEMP","CURE TEMP",-0.2200f,-0.5250f,"mint"),
            ("OUTGAS_IN","OUTGAS IN",-0.1550f,-0.5250f,"mint"),
            ("PORE_STATUS","PORE STATUS",-0.0900f,-0.5250f,"mint"),
            ("CURE_DONE","CURE DONE",-0.0250f,-0.5250f,"mint"),
            ("UV_SAFE","UV SAFE",0.0400f,-0.5250f,"amber"),
            ("ELLIPSOMETRY","ELLIPSOMETRY",0.1050f,-0.5250f,"mint"),
            ("FILM_BOW","FILM BOW",0.1700f,-0.5250f,"mint"),
            ("MOISTURE_IN","MOISTURE IN",0.2350f,-0.5250f,"mint"),
            ("BOND_QUALITY","BOND QUALITY",0.3000f,-0.5250f,"mint"),
            ("RC_IN","RC IN",0.3650f,-0.5250f,"mint"),
            ("COMB_READY","COMB READY",0.4300f,-0.5250f,"mint"),
            ("BIAS_STATUS","BIAS STATUS",0.4950f,-0.5250f,"mint"),
            ("STRESS_CMD","STRESS CMD",0.5600f,-0.5250f,"mint"),
            ("DIELECTRIC_LEAKAGE_ELECTROMETER_OUT","LEAK METER OUT",0.6250f,-0.5250f,"amber"),
            ("DIELECTRIC_RELIABILITY_CONTROLLER_LEAKAGE_IN","REL LEAK IN",0.6900f,-0.5250f,"amber"),
            ("METER_HEALTH","METER HEALTH",0.7550f,-0.5250f,"mint"),
            ("TEST_TELEM","TEST TELEM",0.8200f,-0.5250f,"mint"),
            ("TDDB_GRAPH","TDDB GRAPH",0.8850f,-0.5250f,"mint"),
            ("FILM_TELEM","FILM TELEM",0.9500f,-0.5250f,"mint"),
            ("STRESS_PERMIT","STRESS PERMIT",1.0150f,-0.5250f,"amber"),
            ("STRESS_ENABLE","STRESS ENABLE",-1.0000f,-0.7000f,"mint"),
            ("INCOMING_CHECK","INCOMING CHECK",-0.9350f,-0.7000f,"mint"),
            ("PINHOLE_WARN","PINHOLE WARN",-0.8700f,-0.7000f,"mint"),
            ("COVERAGE_OK","COVERAGE OK",-0.8050f,-0.7000f,"mint"),
            ("DAMAGE_STATUS","DAMAGE STATUS",-0.7400f,-0.7000f,"mint"),
            ("CHARGE_WARN","CHARGE WARN",-0.6750f,-0.7000f,"mint"),
            ("LOWK_CERT","LOWK CERT",-0.6100f,-0.7000f,"mint"),
            ("TEMP_HISTORY","TEMP HISTORY",-0.5450f,-0.7000f,"mint"),
            ("OUTGAS_OK","OUTGAS OK",-0.4800f,-0.7000f,"mint"),
            ("STRESS_HEALTH","STRESS HEALTH",-0.4150f,-0.7000f,"mint"),
            ("MOISTURE_OK","MOISTURE OK",-0.3500f,-0.7000f,"mint"),
            ("RC_RESULT","RC RESULT",-0.2850f,-0.7000f,"mint"),
            ("METER_CAL_OK","METER CAL OK",-0.2200f,-0.7000f,"mint"),
            ("COMB_ISOLATION","COMB ISOLATION",-0.1550f,-0.7000f,"mint"),
            ("RELIABILITY_OK","REL OK",-0.0900f,-0.7000f,"mint"),
            ("ESTOP_NC","ESTOP NC",-0.0250f,-0.7000f,"amber"),
            ("SAFE_STOP_IN","SAFE STOP IN",0.0400f,-0.7000f,"amber"),
            ("SAFETY_HEARTBEAT","SAFETY HEARTBEAT",0.1050f,-0.7000f,"amber"),
            ("CAP_RF_OFF","CAP RF OFF",0.1700f,-0.7000f,"mint"),
            ("LOWK_RF_OFF","LOWK RF OFF",0.2350f,-0.7000f,"mint"),
            ("UV_OFF","UV OFF",0.3000f,-0.7000f,"mint"),
            ("STRESS_OFF","STRESS OFF",0.3650f,-0.7000f,"mint"),
            ("FAULT_LATCH","FAULT LATCH",0.4300f,-0.7000f,"amber"),
            ("PLC_STATE","PLC STATE",0.4950f,-0.7000f,"mint"),
            ("OPERATOR_CMD","OPERATOR CMD",0.5600f,-0.7000f,"mint"),
            ("RELIABILITY_REQ","REL REQ",0.6250f,-0.7000f,"mint"),
            ("LEAK_VALID","LEAK VALID",0.6900f,-0.7000f,"amber"),
            ("LEAK_LOSS_TRIP","LEAK LOSS TRIP",0.7550f,-0.7000f,"amber"),
            ("TRANSFER_HOLD","TRANSFER HOLD",0.8200f,-0.7000f,"mint"),
            ("OBJECTIVE_EVENT","OBJECTIVE EVENT",0.8850f,-0.7000f,"mint"),
            ("SCADA_CERT","SCADA CERT",0.9500f,-0.7000f,"mint"),
            ("SCADA_ALARM","SCADA ALARM",1.0150f,-0.7000f,"mint"),
        };

        public static IEnumerable<string> TerminalIds => Sockets.Select(socket => socket.id);

        private static readonly Dictionary<string, string> Palette = new()
        {
            {"red","EE485C"},{"blue","1711E2"},{"amber","FFA82B"},{"violet","A282FF"},{"plasma","A45DFF"},
            {"mint","34F6C5"},{"cyan","23DAFD"},{"void","07111C"},{"white","D4F5FF"},{"glass","498BA8"}
        };

        [MenuItem("Tools/Project Spark/Levels/Create Level 78")]
        public static void Install()
        {
            SparkLevel78Builder.Create();
            var root = AssetDatabase.LoadAssetAtPath<GameObject>(Prefab);
            if (root == null || root.GetComponent<SparkLevel78Process>().controllerDisplay == null) Fit();
            Register();
            ValidateAssets();
        }

        [MenuItem("Tools/Project Spark/Level 78/Validate Level Assets")]
        public static void ValidateAssets()
        {
            var root=AssetDatabase.LoadAssetAtPath<GameObject>(Prefab);
            var control=root != null ? root.GetComponent<SparkLevel78Process>() : null;
            var bindings=root != null ? root.GetComponent<SparkLevelSceneBindings>() : null;
            if (control==null || bindings==null || control.controllerDisplay==null || control.qualityDisplay==null || control.rig==null || control.rig.pivots.Length!=46 ||
                control.pins.Length!=182 || control.pins.Distinct().Count()!=182 || 
                !TerminalIds.SequenceEqual(control.pins.Select(pin=>pin.name)) ||
                control.pins.Any(pin=>!bindings.TryGetTerminal(pin.name,out var bound) || bound!=pin) ||
                SparkLevel78Process.Gates.Any(id=>!bindings.IsComponentIdDeclared(id)))
                throw new InvalidOperationException("Level 78 wiring, motion or binding references are incomplete.");
            Debug.Log("[LEVEL 78] Validated 182 distinct terminals, 118 player wires and 6 evaluator gates.",root);
        }

        private static void Register()
        {
            var scene = SceneManager.GetSceneByPath(ScenePath);
            bool opened = !scene.isLoaded;
            if (opened) scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
            bool dirty = scene.isDirty;
            var objects = scene.GetRootGameObjects();
            T Find<T>() where T : Component => objects.SelectMany(item => item.GetComponentsInChildren<T>(true)).Single();
            var level = AssetDatabase.LoadAssetAtPath<SparkLevelDefinition>(Folder + "Level_78.asset");
            var drawing = AssetDatabase.LoadAssetAtPath<SparkLevelDrawingAsset>(Folder + "Level_78_Drawing.asset");
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
            else if (dirty) Debug.Log("MainMenu had unsaved edits. Save the scene to persist Level 78's campaign and teaching entries.");
        }

        private static void Append(Object target, string field, Object value)
        {
            if (value == null) throw new InvalidOperationException("Missing Level 78 asset for " + field);
            var data = new SerializedObject(target);
            var list = data.FindProperty(field);
            for (int i = 0; i < list.arraySize; i++)
                if (list.GetArrayElementAtIndex(i).objectReferenceValue == value) return;
            list.arraySize++;
            list.GetArrayElementAtIndex(list.arraySize - 1).objectReferenceValue = value;
            data.ApplyModifiedProperties();
        }

        [MenuItem("Tools/Project Spark/Level 78/Fit Breadboard Assembly")]
        public static void Fit()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Exit Play Mode before fitting Level 78.");
            var models = Models.Select((name, index) =>
            {
                string path = Folder + $"Models/L78_{index:00}_{name}_Fitted.fbx";
                return AssetDatabase.LoadAssetAtPath<GameObject>(path) ??
                    throw new InvalidOperationException("Missing fitted kit module " + path);
            }).ToArray();
            if (AssetDatabase.LoadAssetAtPath<GameObject>(Prefab) == null)
                throw new InvalidOperationException("The Level 78 prefab is missing.");

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
                    throw new InvalidOperationException("Unity could not save the Level 78 layout.");
                AssetDatabase.SaveAssets();
                Debug.Log("Level 78 fitted to the MainMenu breadboard with 182 seated terminals.");
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
            var bench = root.transform.Find("Dielectric Workbench");
            var bindings = root.GetComponent<SparkLevelSceneBindings>();
            var loop = root.GetComponent<SparkLevel78Process>();
            if (bench == null || bindings == null || loop == null)
                throw new InvalidOperationException("Level 78 is missing its workbench, controls, or scene bindings.");

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

            foreach (string name in new[] {"L78_HD_VISUAL_ART", "Breadboard Labels", "Motion pivots"})
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
            var assembly = Child(bench, "L78_HD_VISUAL_ART", new Vector3(0,floor,-.010f));
            foreach (var model in models)
            {
                var instance = Object.Instantiate(model, assembly, false).transform;
                var markers = instance.GetComponentsInChildren<Transform>(true)
                    .Where(item => item.name.StartsWith("L78_Board",StringComparison.Ordinal))
                    .ToDictionary(item => item.name);
                Transform Marker(string name) => markers["L78_Board" + name];
                Vector3 Axis(string name) => assembly.InverseTransformVector(Marker(name).position - Marker("Center").position);
                instance.localRotation = Quaternion.Inverse(Quaternion.LookRotation(-Axis("Front"), Axis("Up"))) * instance.localRotation;
                instance.localScale /= Axis("Right").magnitude;
                instance.localPosition -= assembly.InverseTransformPoint(Marker("Center").position);
                if (Axis("Right").x < .99f)
                    throw new InvalidOperationException("Unexpected axes on fitted model " + model.name);
            }

            assembly.localScale = Vector3.one * .55f;
            if (!AssetDatabase.IsValidFolder(Materials)) AssetDatabase.CreateFolder(Folder + "Models", "Materials");
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) throw new InvalidOperationException("The project URP Lit shader is missing.");
            foreach (var mesh in assembly.GetComponentsInChildren<MeshRenderer>())
            {
                string key = mesh.name.Substring(mesh.name.LastIndexOf("__", StringComparison.Ordinal) + 2);
                if(key=="tint1D303F" && mesh.name.StartsWith("L78_TransparentMultichamberSafetyEnclosure_",StringComparison.Ordinal)) key="glass";
                var material = MaterialFor(shader, key);
                // The fitted meshes can contain several imported submeshes.
                mesh.sharedMaterials = Enumerable.Repeat(material, mesh.GetComponent<MeshFilter>().sharedMesh.subMeshCount).ToArray();
                if (key is "glass" or "acrylic") mesh.shadowCastingMode = ShadowCastingMode.Off;
            }

            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (loop.calibrationText == null)
            {
                var oldControls = root.transform.Find("Level 78 Controls");
                if (oldControls != null) Object.DestroyImmediate(oldControls.gameObject);
                SparkLevel78Builder.BuildControls(root.transform,loop,font);
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
                terminalData.FindProperty("maxConnections").intValue = SparkLevel78Process.Wires.Count(wire => wire.a == index || wire.b == index);
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






                ("VACUUM / TRANSFER",-.865f,.147f),("FILM / METROLOGY",.862f,.147f),
                ("RF / DELIVERY",-.86f,-.235f),("COUPON / SAFETY",.86f,-.235f)
            };
            foreach(var title in titles)
                Label(labels,font,title.title,title.title,new Vector3(title.x,floor+.058f,title.z),.0165f);
            Label(labels,font,"Process title","CAP   /   LOW-K   /   UV CURE",new Vector3(0,floor+.20f,.14f),.013f);
            Label(labels,font,"Reliability title","PROTECTED COUPON TEST",new Vector3(.31f,floor+.18f,-.16f),.012f);
            Transform Pivot(string module,string key)
            {
                var name="fitted_"+module+"_"+key;
                return assembly.GetComponentsInChildren<Transform>(true).Single(item=>item.name==name);
            }
            loop.rig.art=assembly;
            loop.rig.pivots=new[]{
                Pivot("HighVacuumWaferLoadLock","ANIM_LOADLOCK_DOOR"),
                Pivot("PrecisionVacuumGateValve","ANIM_GATE_VALVE"),
                Pivot("MultiAxisVacuumTransferRobot","ANIM_TRANSFER_ROBOT_BASE"),
                Pivot("MultiAxisVacuumTransferRobot","ANIM_TRANSFER_ROBOT_ARM"),
                Pivot("MultiAxisVacuumTransferRobot","ANIM_TRANSFER_ROBOT_BLADE"),
                Pivot("OpticalWaferAlignmentStage","ANIM_WAFER_ALIGNER"),
                Pivot("CopperSurfacePreconditioningModule","ANIM_PRECONDITION_SHUTTER"),
                Pivot("CopperCapReactorLid","ANIM_CAP_CHAMBER_LID"),
                Pivot("CeramicCapGasShowerhead","ANIM_CAP_SHOWERHEAD"),
                Pivot("CapPlasmaRFSourceAssembly","ANIM_CAP_RF_PLASMA"),
                Pivot("CapRFMatchingNetwork","ANIM_CAP_RF_MATCHING"),
                Pivot("CapHeatedWaferSusceptor","ANIM_CAP_HEATED_SUSCEPTOR"),
                Pivot("CapHeatedWaferSusceptor","ANIM_CAP_WAFER_ROTATION"),
                Pivot("CapVacuumExhaustThrottle","ANIM_CAP_EXHAUST_THROTTLE"),
                Pivot("VirtualCapPrecursorCartridge","ANIM_CAP_PRECURSOR_VALVE"),
                Pivot("CapGasMassFlowController","ANIM_CAP_GAS_FLOW"),
                Pivot("CapFilmThicknessInterferometer","ANIM_CAP_THICKNESS_SCAN"),
                Pivot("HolographicCopperDiffusionShield","ANIM_CAP_COVERAGE_HOLOGRAM"),
                Pivot("LowKPECVDReactor","ANIM_LOWK_CHAMBER_LID"),
                Pivot("LowKDielectricGasShowerhead","ANIM_LOWK_SHOWERHEAD"),
                Pivot("LowKPlasmaSource","ANIM_LOWK_RF_PLASMA"),
                Pivot("LowKRFMatchingAssembly","ANIM_LOWK_MATCHING"),
                Pivot("LowKHeatedWaferChuck","ANIM_LOWK_SUSCEPTOR"),
                Pivot("VirtualLowKPrecursorCartridge","ANIM_LOWK_PRECURSOR_VALVE"),
                Pivot("VirtualPorogenCartridge","ANIM_LOWK_POROGEN_VALVE"),
                Pivot("DualChannelLowKGasManifold","ANIM_LOWK_GAS_FLOW"),
                Pivot("LowKSurfaceChargingSensor","ANIM_LOWK_CHARGE_MONITOR"),
                Pivot("LowKFilmGrowthOpticalMonitor","ANIM_LOWK_FILM_SCAN"),
                Pivot("UVThermalCureReactor","ANIM_UV_CURE_DOOR"),
                Pivot("MultiZoneUVLampArray","ANIM_UV_LAMP_ARRAY"),
                Pivot("PrecisionUVCureShutter","ANIM_UV_SHUTTER"),
                Pivot("RotatingCureWaferPlaten","ANIM_CURE_ROTATING_PLATEN"),
                Pivot("CureWaferTemperatureSensor","ANIM_CURE_THERMAL_PLATEN"),
                Pivot("CureOutgassingMonitor","ANIM_CURE_OUTGAS"),
                Pivot("HolographicLowKPoreNetwork","ANIM_PORE_NETWORK_HOLOGRAM"),
                Pivot("WaferStressBowScanner","ANIM_FILM_BOW_SCAN"),
                Pivot("DielectricEllipsometryStation","ANIM_ELLIPSOMETRY_HEAD"),
                Pivot("InterlineCombTestFixture","ANIM_LEAKAGE_TEST_PROBE"),
                Pivot("AcceleratedBiasStressTestSource","ANIM_BIAS_STRESS_GUARD"),
                Pivot("HolographicTDDBReliabilityGraph","ANIM_TDDB_GRAPH"),
                Pivot("SurfaceMoistureInspectionAnalyzer","ANIM_MOISTURE_PROBE"),
                Pivot("InterconnectRCDelayAnalyzer","ANIM_RC_TIMING_PROBE"),
                Pivot("EmergencyIsolationSafetyStation","ANIM_EMERGENCY_ISOLATION"),
                Pivot("TurbomolecularVacuumPump","ANIM_TURBO_ROTOR"),
                Pivot("VacuumBackingPump","ANIM_BACKING_ROTOR"),
                Pivot("HolographicDielectricSCADA","ANIM_DIELECTRIC_SCADA"),
            };
            var pivots=loop.rig.pivots;
            pivots[3].SetParent(pivots[2],true);
            pivots[4].SetParent(pivots[3],true);
            Light Lamp(string name,Vector3 position,Color color,float intensity,float range)
            {
                var node=Child(bench,name,position);
                node.SetParent(assembly,true);
                var lamp=node.gameObject.AddComponent<Light>();
                lamp.type=LightType.Point;
                lamp.color=color;
                lamp.intensity=intensity;
                lamp.range=range;
                return lamp;
            }
            Lamp("Cluster work light",new Vector3(0,floor+.56f,-.14f),new Color(.8f,.90f,1),2.5f,1.25f);
            Lamp("Metrology fill",new Vector3(.54f,floor+.34f,.16f),new Color(.44f,.80f,1),.95f,.76f);
            loop.rig.BindEffects(
                Lamp("Cap glow",new Vector3(-.37f,floor+.20f,-.03f),new Color(.29f,.75f,1),1,.45f),
                Lamp("Low-k glow",new Vector3(0,floor+.20f,-.03f),new Color(.48f,.39f,1),1,.46f),
                Lamp("UV cure glow",new Vector3(.28f,floor+.2f,-.03f),new Color(.58f,.37f,1),1,.45f),
                Lamp("Isolation alarm",new Vector3(.56f,floor+.24f,.03f),new Color(1,.18f,.12f),1.25f,.55f));
            if(loop.plot.transform.Find("Trend legend") == null)
            {
                var oldPanel = root.transform.Find("Level 78 Controls");
                if (oldPanel != null) Object.DestroyImmediate(oldPanel.gameObject);
                SparkLevel78Builder.BuildControls(root.transform,loop,font);
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
            loop.controllerDisplay=Readout("Electrometer readout",new Vector3(-.815f,floor+.075f,.215f),new Color(.46f,.94f,1f));
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
            label.fontSize = 76;
            label.characterSize = size;
            label.anchor = TextAnchor.MiddleCenter;
            label.color = new Color(.89f, .96f, 1f);
            label.text = text;
            child.GetComponent<MeshRenderer>().sharedMaterial = font.material;
            return label;
        }

        private static Material MaterialFor(Shader shader, string key)
        {
            string path = $"{Materials}/L78 {key}.mat";
            if (key.StartsWith("tint",StringComparison.Ordinal) && key.Length==10) Palette[key]=key.Substring(4);
            var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing != null && key is not ("glass" or "acrylic") &&
                ColorUtility.TryParseHtmlString("#" + (Palette.TryGetValue(key, out var savedTint) ? savedTint : "9FB3C8"), out var savedColor) &&
                existing.GetColor("_BaseColor") == savedColor) return existing;
            ColorUtility.TryParseHtmlString("#" + (Palette.TryGetValue(key, out var tint) ? tint : "9FB3C8"), out var color);
            var material = existing != null ? existing : new Material(shader) { name = "L77 " + key };
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







