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
    public static class SparkLevel77ModelInstaller
    {
        public const string Folder = "Assets/My_Assets/_Project_Spark/Prefabs/Levels/Level_77_Layout/";
        public const string Prefab = Folder + "Level_77_Layout.prefab";
        public const string ScenePath = "Assets/My_Assets/_Project_Spark/Scenes/MainMenu/MainMenu.unity";
        private static readonly string[] Models = {
            "IndustrialCopperPlanarizationBase","TransparentTripleCMPEnclosure","CopperPlatedWaferCassette","AutomatedWaferHandlingRobot","PreCMPWaferInspectionScanner","ElectroplatedDamasceneWafer","CopperDielectricLayerCrossSection","HolographicCopperOverburdenMap","BulkCopperPolishingPlaten","BulkCopperPolishingPad","BulkPlatenServoDrive","BulkWaferCarrierHead","BulkCarrierVerticalActuator","BulkDownforceLoadCell","MultiZonePressureControlBank","BulkCopperSlurryReservoir","BulkSlurryDeliveryPump","BulkSlurryFlowTransmitter","MotorizedBulkSlurryArm","BulkSlurryDispenseNozzle","BulkPadConditioningAssembly","BulkPadWearMeasurementProbe","OpticalCopperRemovalEndpointSensor","EddyCurrentCopperThicknessGauge","BulkCopperEndpointController","OverheadWaferTransferGantry","SoftLandingPolishingPlaten","SoftLandingPrecisionPad","SoftLandingCarrierHead","SoftLandingForceRegulator","SelectiveProcessFluidManifold","BarrierClearingPolishingPlaten","BarrierSelectivePolishingPad","BarrierClearingCarrierHead","BarrierProcessFluidReservoir","BarrierFluidMeteringPump","BarrierFluidFlowSensor","BarrierFluidDispenseNozzle","DualWavelengthBarrierEndpointSpectrometer","TransparentOpticalEndpointViewport","BarrierClearanceTomographyMonitor","MaterialSelectivityAnalyzer","PolishingMotorTorqueAnalyzer","WaferInterfaceTemperatureSensor","CopperCorrosionPotentialMonitor","CopperSurfaceProtectionModule","PostCMPBrushCleaningChamber","PrecisionMegasonicCleaningModule","CopperResidueChemistryMonitor","UltrapureRinseWaterModule","HighSpeedWaferSpinDryer","WaferSurfaceDefectInspectionCamera","3DCopperDishingErosionScanner","ResidualCopperInspectionScanner","InterlineLeakageTestInstrument","ViaContinuityKelvinProbe","DielectricIntegrityAnalyzer","ThreeStageCMPProcessController","CopperPlanarizationPLCRack","CMPEmergencySafetyStation","HolographicCopperInterconnectSCADA","Regulated5VControlSupply"
        };
        private const string Materials = Folder + "Models/Materials";
        private const float SocketHeight = .025f;
        private const float SocketDiameter = .026f;

        private static readonly (string id, string label, float x, float z, string color)[] Sockets =
        {
            ("SUPPLY_POS","SUP POS",-1.0000f,0.6800f,"red"),
            ("PLC_5V","PLC 5V",-0.9270f,0.6800f,"red"),
            ("SUPPLY_RET","SUP RET",-0.8540f,0.6800f,"blue"),
            ("PLC_GND","PLC GND",-0.7810f,0.6800f,"blue"),
            ("IO_5V","IO 5V",-0.7080f,0.6800f,"red"),
            ("IO_GND","IO GND",-0.6350f,0.6800f,"blue"),
            ("DRV_5V","DRV 5V",-0.5620f,0.6800f,"red"),
            ("DRV_GND","DRV GND",-0.4890f,0.6800f,"blue"),
            ("OPT_5V","OPT 5V",-0.4160f,0.6800f,"red"),
            ("OPT_GND","OPT GND",-0.3430f,0.6800f,"blue"),
            ("SAFE_5V","SAFE 5V",-0.2700f,0.6800f,"red"),
            ("SAFE_GND","SAFE GND",-0.1970f,0.6800f,"blue"),
            ("BULK_RPM_CMD","BULK RPM CMD",-0.1240f,0.6800f,"mint"),
            ("BULK_DRV_SET","BULK DRV SET",-0.0510f,0.6800f,"mint"),
            ("BULK_RPM_OUT","BULK RPM OUT",0.0220f,0.6800f,"mint"),
            ("PLC_BULK_RPM","PLC BULK RPM",0.0950f,0.6800f,"mint"),
            ("BULK_SPIN_CMD","BULK SPIN CMD",0.1680f,0.6800f,"mint"),
            ("BULK_SPIN_SET","BULK SPIN SET",0.2410f,0.6800f,"mint"),
            ("BULK_SPIN_OUT","BULK SPIN OUT",0.3140f,0.6800f,"mint"),
            ("PLC_BULK_SPIN","PLC BULK SPIN",0.3870f,0.6800f,"mint"),
            ("BULK_LIFT_CMD","BULK LIFT CMD",0.4600f,0.6800f,"mint"),
            ("BULK_LIFT_SET","BULK LIFT SET",0.5330f,0.6800f,"mint"),
            ("BULK_LIFT_OUT","BULK LIFT OUT",0.6060f,0.6800f,"mint"),
            ("PLC_BULK_LIFT","PLC BULK LIFT",0.6790f,0.6800f,"mint"),
            ("BULK_FORCE_OUT","BULK FORCE OUT",0.7520f,0.6800f,"mint"),
            ("PLC_BULK_FORCE","PLC BULK FORCE",0.8250f,0.6800f,"mint"),
            ("ZONE_FORCE_CMD","ZONE FORCE CMD",0.8980f,0.6800f,"mint"),
            ("ZONE_FORCE_SET","ZONE FORCE SET",0.9710f,0.6800f,"mint"),
            ("ZONE_FORCE_OUT","ZONE FORCE OUT",-1.0000f,0.5450f,"mint"),
            ("PLC_ZONE_FORCE","PLC ZONE FORCE",-0.9270f,0.5450f,"mint"),
            ("SOFT_RPM_CMD","SOFT RPM CMD",-0.8540f,0.5450f,"mint"),
            ("SOFT_DRV_SET","SOFT DRV SET",-0.7810f,0.5450f,"mint"),
            ("SOFT_RPM_OUT","SOFT RPM OUT",-0.7080f,0.5450f,"mint"),
            ("PLC_SOFT_RPM","PLC SOFT RPM",-0.6350f,0.5450f,"mint"),
            ("SOFT_SPIN_CMD","SOFT SPIN CMD",-0.5620f,0.5450f,"mint"),
            ("SOFT_SPIN_SET","SOFT SPIN SET",-0.4890f,0.5450f,"mint"),
            ("SOFT_FORCE_OUT","SOFT FORCE OUT",-0.4160f,0.5450f,"mint"),
            ("PLC_SOFT_FORCE","PLC SOFT FORCE",-0.3430f,0.5450f,"mint"),
            ("BARR_RPM_CMD","BARR RPM CMD",-0.2700f,0.5450f,"mint"),
            ("BARR_DRV_SET","BARR DRV SET",-0.1970f,0.5450f,"mint"),
            ("BARR_RPM_OUT","BARR RPM OUT",-0.1240f,0.5450f,"mint"),
            ("PLC_BARR_RPM","PLC BARR RPM",-0.0510f,0.5450f,"mint"),
            ("BARR_LIFT_CMD","BARR LIFT CMD",0.0220f,0.5450f,"mint"),
            ("BARR_LIFT_SET","BARR LIFT SET",0.0950f,0.5450f,"mint"),
            ("BARR_LIFT_OUT","BARR LIFT OUT",0.1680f,0.5450f,"mint"),
            ("PLC_BARR_LIFT","PLC BARR LIFT",0.2410f,0.5450f,"mint"),
            ("TORQUE_OUT","TORQUE OUT",0.3140f,0.5450f,"mint"),
            ("PLC_TORQUE","PLC TORQUE",0.3870f,0.5450f,"mint"),
            ("SLURRY_PUMP_CMD","SLURRY PUMP CMD",0.4600f,0.5450f,"mint"),
            ("SLURRY_PUMP_RUN","SLURRY PUMP RUN",0.5330f,0.5450f,"mint"),
            ("SLURRY_PUMP_OUT","SLURRY PUMP OUT",0.6060f,0.5450f,"mint"),
            ("PLC_SLURRY_PUMP","PLC SLURRY PUMP",0.6790f,0.5450f,"mint"),
            ("SLURRY_FLOW_OUT","SLURRY FLOW OUT",0.7520f,0.5450f,"mint"),
            ("PLC_SLURRY_FLOW","PLC SLURRY FLOW",0.8250f,0.5450f,"mint"),
            ("SLURRY_NOZZLE_CMD","SLURRY NOZZLE CMD",0.8980f,0.5450f,"mint"),
            ("SLURRY_DISPENSE","SLURRY DISPENSE",0.9710f,0.5450f,"mint"),
            ("SLURRY_NOZZLE_OUT","SLURRY NOZZLE OUT",-1.0000f,0.4250f,"mint"),
            ("PLC_SLURRY_NOZZLE","PLC SLURRY NOZZLE",-0.9270f,0.4250f,"mint"),
            ("PAD_CMD","PAD CMD",-0.8540f,0.4250f,"mint"),
            ("PAD_RUN","PAD RUN",-0.7810f,0.4250f,"mint"),
            ("PAD_READY","PAD READY",-0.7080f,0.4250f,"mint"),
            ("PLC_PAD_READY","PLC PAD READY",-0.6350f,0.4250f,"mint"),
            ("BARR_PUMP_CMD","BARR PUMP CMD",-0.5620f,0.4250f,"mint"),
            ("BARR_PUMP_RUN","BARR PUMP RUN",-0.4890f,0.4250f,"mint"),
            ("BARR_PUMP_OUT","BARR PUMP OUT",-0.4160f,0.4250f,"mint"),
            ("PLC_BARR_PUMP","PLC BARR PUMP",-0.3430f,0.4250f,"mint"),
            ("BARR_FLOW_OUT","BARR FLOW OUT",-0.2700f,0.4250f,"mint"),
            ("PLC_BARR_FLOW","PLC BARR FLOW",-0.1970f,0.4250f,"mint"),
            ("BARR_NOZZLE_OUT","BARR NOZZLE OUT",-0.1240f,0.4250f,"mint"),
            ("PLC_BARR_NOZZLE","PLC BARR NOZZLE",-0.0510f,0.4250f,"mint"),
            ("SOFT_FLOW_CMD","SOFT FLOW CMD",0.0220f,0.4250f,"mint"),
            ("SOFT_MANIFOLD","SOFT MANIFOLD",0.0950f,0.4250f,"mint"),
            ("SOFT_FLOW_OUT","SOFT FLOW OUT",0.1680f,0.4250f,"mint"),
            ("PLC_SOFT_FLOW","PLC SOFT FLOW",0.2410f,0.4250f,"mint"),
            ("BRUSH_CMD","BRUSH CMD",0.3140f,0.4250f,"mint"),
            ("BRUSH_RUN","BRUSH RUN",0.3870f,0.4250f,"mint"),
            ("BRUSH_RPM_OUT","BRUSH RPM OUT",0.4600f,0.4250f,"mint"),
            ("PLC_BRUSH_RPM","PLC BRUSH RPM",0.5330f,0.4250f,"mint"),
            ("MEGA_CMD","MEGA CMD",0.6060f,0.4250f,"mint"),
            ("MEGA_RUN","MEGA RUN",0.6790f,0.4250f,"mint"),
            ("RINSE_READY","RINSE READY",0.7520f,0.4250f,"mint"),
            ("PLC_RINSE","PLC RINSE",0.8250f,0.4250f,"mint"),
            ("DRY_RPM_OUT","DRY RPM OUT",0.8980f,0.4250f,"mint"),
            ("PLC_DRY_RPM","PLC DRY RPM",0.9710f,0.4250f,"mint"),
            ("EDDY_OUT","EDDY OUT",-1.0000f,-0.3550f,"mint"),
            ("BULK_THICKNESS","BULK THICK",-0.9310f,-0.3550f,"mint"),
            ("COPPER_OPT_OUT","COPPER OPT OUT",-0.8620f,-0.3550f,"mint"),
            ("BULK_CLEAR_IN","BULK CLEAR IN",-0.7930f,-0.3550f,"mint"),
            ("FORCE_SAMPLE","FORCE SAMPLE",-0.7240f,-0.3550f,"mint"),
            ("BULK_FORCE_IN","BULK FORCE IN",-0.6550f,-0.3550f,"mint"),
            ("PAD_WEAR_OUT","PAD WEAR OUT",-0.5860f,-0.3550f,"mint"),
            ("PAD_WEAR_IN","PAD WEAR IN",-0.5170f,-0.3550f,"mint"),
            ("COPPER_MAP_OUT","COPPER MAP OUT",-0.4480f,-0.3550f,"mint"),
            ("THICK_TELEM","THICK TELEM",-0.3790f,-0.3550f,"mint"),
            ("LOAD_TORQUE_OUT","LOAD TORQUE OUT",-0.3100f,-0.3550f,"mint"),
            ("TORQUE_TELEM","TORQUE TELEM",-0.2410f,-0.3550f,"mint"),
            ("THERMAL_OUT","THERMAL OUT",-0.1720f,-0.3550f,"mint"),
            ("PLC_TEMP","PLC TEMP",-0.1030f,-0.3550f,"mint"),
            ("BARRIER_MAP_OUT","BARRIER MAP OUT",-0.0340f,-0.3550f,"mint"),
            ("PLC_BARR_MAP","PLC BARR MAP",0.0350f,-0.3550f,"mint"),
            ("BARRIER_ENDPOINT_SPECTROMETER_SIGNAL_OUT","BARR SPEC OUT",0.1040f,-0.3550f,"mint"),
            ("THREE_STAGE_CMP_CONTROLLER_ENDPOINT_IN","CMP END IN",0.1730f,-0.3550f,"mint"),
            ("WINDOW_HEALTH_OUT","WINDOW HEALTH OUT",0.2420f,-0.3550f,"mint"),
            ("OPTICS_READY","OPTICS READY",0.3110f,-0.3550f,"mint"),
            ("SPECTRAL_TELEM_OUT","SPEC TELEM OUT",0.3800f,-0.3550f,"mint"),
            ("SCADA_SPECTRAL","SCADA SPEC",0.4490f,-0.3550f,"mint"),
            ("PROFILE_OUT","PROFILE OUT",0.5180f,-0.3550f,"mint"),
            ("PLC_PROFILE","PLC PROFILE",0.5870f,-0.3550f,"mint"),
            ("RESIDUE_OUT","RESIDUE OUT",0.6560f,-0.3550f,"mint"),
            ("PLC_RESIDUE","PLC RESIDUE",0.7250f,-0.3550f,"mint"),
            ("BULK_DONE_OUT","BULK DONE OUT",0.7940f,-0.3550f,"mint"),
            ("STAGE_B_PERMIT","STAGE B PERMIT",0.8630f,-0.3550f,"amber"),
            ("OPTICAL_HEALTH_OUT","OPTICAL HEALTH OUT",0.9320f,-0.3550f,"mint"),
            ("OPTICAL_SAFETY_IN","OPTICAL SAFETY IN",1.0010f,-0.3550f,"amber"),
            ("BARR_RETRACT_CMD","BARR RETRACT CMD",-1.0000f,-0.5250f,"mint"),
            ("BARR_RETRACT","BARR RETRACT",-0.9330f,-0.5250f,"mint"),
            ("SELECTIVITY_CMD","SEL CMD",-0.8660f,-0.5250f,"mint"),
            ("SELECTIVITY_START","SEL START",-0.7990f,-0.5250f,"mint"),
            ("RATIO_OUT","RATIO OUT",-0.7320f,-0.5250f,"mint"),
            ("PLC_RATIO","PLC RATIO",-0.6650f,-0.5250f,"mint"),
            ("BARR_FLUID_READY","BARR FLUID READY",-0.5980f,-0.5250f,"mint"),
            ("PLC_BARR_FLUID","PLC BARR FLUID",-0.5310f,-0.5250f,"mint"),
            ("BARR_FEED_CMD","BARR FEED CMD",-0.4640f,-0.5250f,"mint"),
            ("BARR_FLUID_SET","BARR FLUID SET",-0.3970f,-0.5250f,"mint"),
            ("BARR_FLOW_SAMPLE","BARR FLOW SAMPLE",-0.3300f,-0.5250f,"mint"),
            ("SELECT_FLOW_IN","SELECT FLOW IN",-0.2630f,-0.5250f,"mint"),
            ("SOFT_FLUID_OUT","SOFT FLUID OUT",-0.1960f,-0.5250f,"mint"),
            ("SELECT_SOFT_IN","SELECT SOFT IN",-0.1290f,-0.5250f,"mint"),
            ("SELECT_ALARM","SELECT ALARM",-0.0620f,-0.5250f,"mint"),
            ("SELECT_CLEAR","SELECT CLEAR",0.0050f,-0.5250f,"mint"),
            ("SELECT_COPPER","SELECT COPPER",0.0720f,-0.5250f,"mint"),
            ("SELECT_TEMP","SELECT TEMP",0.1390f,-0.5250f,"mint"),
            ("SCADA_RATIO","SCADA RATIO",0.2060f,-0.5250f,"mint"),
            ("SOFT_MODE","SOFT MODE",0.2730f,-0.5250f,"mint"),
            ("REMOVAL_RATIO","REMOVAL RATIO",0.3400f,-0.5250f,"mint"),
            ("POLISH_PERMIT","POLISH PERMIT",0.4070f,-0.5250f,"amber"),
            ("POLISH_ENABLE","POLISH ENABLE",0.4740f,-0.5250f,"mint"),
            ("CORROSION_RISK","CORR RISK",0.5410f,-0.5250f,"mint"),
            ("PROTECT_ENABLE","PROTECT ENABLE",0.6080f,-0.5250f,"mint"),
            ("PROTECT_STATUS","PROTECT STATUS",0.6750f,-0.5250f,"mint"),
            ("CLEAN_RESIDUE","CLEAN RESIDUE",0.7420f,-0.5250f,"mint"),
            ("BRUSH_JOB","BRUSH JOB",0.8090f,-0.5250f,"mint"),
            ("BRUSH_DONE","BRUSH DONE",0.8760f,-0.5250f,"mint"),
            ("MEGA_JOB","MEGA JOB",0.9430f,-0.5250f,"mint"),
            ("MEGA_DONE","MEGA DONE",1.0100f,-0.5250f,"mint"),
            ("RINSE_SUPPLY","RINSE SUP",-1.0000f,-0.7000f,"mint"),
            ("DRY_DONE","DRY DONE",-0.9330f,-0.7000f,"mint"),
            ("DEFECT_COUNT","DEFECT COUNT",-0.8660f,-0.7000f,"mint"),
            ("SCADA_CLEAN","SCADA CLEAN",-0.7990f,-0.7000f,"mint"),
            ("INTERLINE_LEAK","INTERLINE LEAK",-0.7320f,-0.7000f,"mint"),
            ("VIA_KELVIN","VIA KELVIN",-0.6650f,-0.7000f,"mint"),
            ("DIELECTRIC_TEST","DIELECTRIC TEST",-0.5980f,-0.7000f,"mint"),
            ("SURFACE_DEFECTS","SURFACE DEFECTS",-0.5310f,-0.7000f,"mint"),
            ("GEOMETRY_MAP","GEOMETRY MAP",-0.4640f,-0.7000f,"mint"),
            ("CONDUCTIVE_MAP","COND MAP",-0.3970f,-0.7000f,"mint"),
            ("INCOMING_SCAN","INCOMING SCAN",-0.3300f,-0.7000f,"mint"),
            ("SCADA_VIA","SCADA VIA",-0.2630f,-0.7000f,"mint"),
            ("SCADA_ISOLATION","SCADA ISOLATION",-0.1960f,-0.7000f,"mint"),
            ("SCADA_DIELECTRIC","SCADA DIELECTRIC",-0.1290f,-0.7000f,"mint"),
            ("WAFER_CERT","WAFER CERT",-0.0620f,-0.7000f,"mint"),
            ("TEST_CAL","TEST CAL",0.0050f,-0.7000f,"mint"),
            ("ESTOP_NC","ESTOP NC",0.0720f,-0.7000f,"amber"),
            ("SAFE_STOP_IN","SAFE STOP IN",0.1390f,-0.7000f,"amber"),
            ("BULK_SAFE_OFF","BULK SAFE OFF",0.2060f,-0.7000f,"amber"),
            ("SOFT_SAFE_OFF","SOFT SAFE OFF",0.2730f,-0.7000f,"amber"),
            ("BARR_SAFE_OFF","BARR SAFE OFF",0.3400f,-0.7000f,"amber"),
            ("BULK_SAFE_LIFT","BULK SAFE LIFT",0.4070f,-0.7000f,"amber"),
            ("SOFT_SAFE_LIFT","SOFT SAFE LIFT",0.4740f,-0.7000f,"amber"),
            ("BARR_SAFE_LIFT","BARR SAFE LIFT",0.5410f,-0.7000f,"amber"),
            ("SAFE_OPTICS","SAFE OPTICS",0.6080f,-0.7000f,"amber"),
            ("STAGE_CONTROL","STAGE CONTROL",0.6750f,-0.7000f,"mint"),
            ("SCADA_SAFE","SCADA SAFE",0.7420f,-0.7000f,"amber"),
            ("OPERATOR_CMD","OPERATOR CMD",0.8090f,-0.7000f,"mint"),
            ("SCADA_PLC","SCADA PLC",0.8760f,-0.7000f,"mint"),
            ("OBJECTIVE_EVENT","OBJECTIVE EVENT",0.9430f,-0.7000f,"mint"),
            ("SCADA_CERT","SCADA CERT",1.0100f,-0.7000f,"mint"),
        };

        public static IEnumerable<string> TerminalIds => Sockets.Select(socket => socket.id);

        private static readonly Dictionary<string, string> Palette = new()
        {
            {"red","EE485C"},{"blue","1711E2"},{"amber","FFA82B"},{"violet","A282FF"},{"plasma","A45DFF"},
            {"mint","34F6C5"},{"cyan","23DAFD"},{"void","07111C"},{"white","D4F5FF"},{"glass","498BA8"}
        };

        [MenuItem("Tools/Project Spark/Levels/Create Level 77")]
        public static void Install()
        {
            SparkLevel77Builder.Create();
            var root = AssetDatabase.LoadAssetAtPath<GameObject>(Prefab);
            if (root == null || root.GetComponent<SparkLevel77Process>().controllerDisplay == null) Fit();
            Register();
            ValidateAssets();
        }

        [MenuItem("Tools/Project Spark/Level 77/Validate Level Assets")]
        public static void ValidateAssets()
        {
            var root=AssetDatabase.LoadAssetAtPath<GameObject>(Prefab);
            var control=root != null ? root.GetComponent<SparkLevel77Process>() : null;
            var bindings=root != null ? root.GetComponent<SparkLevelSceneBindings>() : null;
            if (control==null || bindings==null || control.controllerDisplay==null || control.qualityDisplay==null || control.rig==null || control.rig.pivots.Length!=44 ||
                control.pins.Length!=176 || control.pins.Distinct().Count()!=176 || 
                !TerminalIds.SequenceEqual(control.pins.Select(pin=>pin.name)) ||
                control.pins.Any(pin=>!bindings.TryGetTerminal(pin.name,out var bound) || bound!=pin) ||
                SparkLevel77Process.Gates.Any(id=>!bindings.IsComponentIdDeclared(id)))
                throw new InvalidOperationException("Level 77 wiring, motion or binding references are incomplete.");
            Debug.Log("[LEVEL 77] Validated 176 distinct terminals, 114 player wires and 6 evaluator gates.",root);
        }

        private static void Register()
        {
            var scene = SceneManager.GetSceneByPath(ScenePath);
            bool opened = !scene.isLoaded;
            if (opened) scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
            bool dirty = scene.isDirty;
            var objects = scene.GetRootGameObjects();
            T Find<T>() where T : Component => objects.SelectMany(item => item.GetComponentsInChildren<T>(true)).Single();
            var level = AssetDatabase.LoadAssetAtPath<SparkLevelDefinition>(Folder + "Level_77.asset");
            var drawing = AssetDatabase.LoadAssetAtPath<SparkLevelDrawingAsset>(Folder + "Level_77_Drawing.asset");
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
            else if (dirty) Debug.Log("MainMenu had unsaved edits. Save the scene to persist Level 77's campaign and teaching entries.");
        }

        private static void Append(Object target, string field, Object value)
        {
            if (value == null) throw new InvalidOperationException("Missing Level 77 asset for " + field);
            var data = new SerializedObject(target);
            var list = data.FindProperty(field);
            for (int i = 0; i < list.arraySize; i++)
                if (list.GetArrayElementAtIndex(i).objectReferenceValue == value) return;
            list.arraySize++;
            list.GetArrayElementAtIndex(list.arraySize - 1).objectReferenceValue = value;
            data.ApplyModifiedProperties();
        }

        [MenuItem("Tools/Project Spark/Level 77/Fit Breadboard Assembly")]
        public static void Fit()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Exit Play Mode before fitting Level 77.");
            var models = Models.Select((name, index) =>
            {
                string path = Folder + $"Models/L77_{index:00}_{name}_Fitted.fbx";
                return AssetDatabase.LoadAssetAtPath<GameObject>(path) ??
                    throw new InvalidOperationException("Missing fitted kit module " + path);
            }).ToArray();
            if (AssetDatabase.LoadAssetAtPath<GameObject>(Prefab) == null)
                throw new InvalidOperationException("The Level 77 prefab is missing.");

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
                    throw new InvalidOperationException("Unity could not save the Level 77 layout.");
                AssetDatabase.SaveAssets();
                Debug.Log("Level 77 fitted to the MainMenu breadboard with 176 seated terminals.");
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
            var bench = root.transform.Find("Finishing Workbench");
            var bindings = root.GetComponent<SparkLevelSceneBindings>();
            var loop = root.GetComponent<SparkLevel77Process>();
            if (bench == null || bindings == null || loop == null)
                throw new InvalidOperationException("Level 77 is missing its workbench, controls, or scene bindings.");

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

            foreach (string name in new[] {"L77_HD_VISUAL_ART", "Breadboard Labels", "Motion pivots"})
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
            var assembly = Child(bench, "L77_HD_VISUAL_ART", new Vector3(0,floor,-.010f));
            foreach (var model in models)
            {
                var instance = Object.Instantiate(model, assembly, false).transform;
                var markers = instance.GetComponentsInChildren<Transform>(true)
                    .Where(item => item.name.StartsWith("L77_Board",StringComparison.Ordinal))
                    .ToDictionary(item => item.name);
                Transform Marker(string name) => markers["L77_Board" + name];
                Vector3 Axis(string name) => assembly.InverseTransformVector(Marker(name).position - Marker("Center").position);
                instance.localRotation = Quaternion.Inverse(Quaternion.LookRotation(-Axis("Front"), Axis("Up"))) * instance.localRotation;
                instance.localScale /= Axis("Right").magnitude;
                instance.localPosition -= assembly.InverseTransformPoint(Marker("Center").position);
                if (Axis("Right").x < .99f)
                    throw new InvalidOperationException("Unexpected axes on fitted model " + model.name);
            }

            assembly.localScale = Vector3.one * .64f;
            if (!AssetDatabase.IsValidFolder(Materials)) AssetDatabase.CreateFolder(Folder + "Models", "Materials");
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) throw new InvalidOperationException("The project URP Lit shader is missing.");
            foreach (var mesh in assembly.GetComponentsInChildren<MeshRenderer>())
            {
                string key = mesh.name.Substring(mesh.name.LastIndexOf("__", StringComparison.Ordinal) + 2);
                if(key=="tint1D303F" && mesh.name.StartsWith("L77_TransparentTripleCMPEnclosure_",StringComparison.Ordinal)) key="glass";
                var material = MaterialFor(shader, key);
                // The fitted meshes can contain several imported submeshes.
                mesh.sharedMaterials = Enumerable.Repeat(material, mesh.GetComponent<MeshFilter>().sharedMesh.subMeshCount).ToArray();
                if (key is "glass" or "acrylic") mesh.shadowCastingMode = ShadowCastingMode.Off;
            }

            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (loop.calibrationText == null)
            {
                var oldControls = root.transform.Find("Level 77 Controls");
                if (oldControls != null) Object.DestroyImmediate(oldControls.gameObject);
                SparkLevel77Builder.BuildControls(root.transform,loop,font);
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
                terminalData.FindProperty("maxConnections").intValue = SparkLevel77Process.Wires.Count(wire => wire.a == index || wire.b == index);
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






                ("SLURRY / FLOW",-.865f,.147f),("WAFER / METROLOGY",.862f,.147f),
                ("DRIVES / PADS",-.86f,-.235f),("SAFETY / CONTROL",.86f,-.235f)
            };
            foreach(var title in titles)
                Label(labels,font,title.title,title.title,new Vector3(title.x,floor+.058f,title.z),.0165f);
            Label(labels,font,"CMP title","A · BULK   /   B · SOFT LAND   /   C · BARRIER",new Vector3(0,floor+.20f,.14f),.013f);
            Label(labels,font,"Clean title","PROTECT / RINSE / INSPECT",new Vector3(.32f,floor+.18f,-.16f),.012f);
            Transform Pivot(string module,string key)
            {
                var name="fitted_"+module+"_"+key;
                return assembly.GetComponentsInChildren<Transform>(true).Single(item=>item.name==name);
            }
            loop.rig.art=assembly;
            loop.rig.pivots=new[]{
                Pivot("AutomatedWaferHandlingRobot","ANIM_WAFER_ROBOT_BASE"),
                Pivot("AutomatedWaferHandlingRobot","ANIM_WAFER_ROBOT_ARM"),
                Pivot("AutomatedWaferHandlingRobot","ANIM_WAFER_ROBOT_TOOL"),
                Pivot("CopperDielectricLayerCrossSection","ANIM_LAYER_CROSS_SECTION"),
                Pivot("BulkCopperPolishingPlaten","ANIM_BULK_PLATEN_ROTATION"),
                Pivot("BulkCopperPolishingPad","ANIM_BULK_PAD_ROTATION"),
                Pivot("BulkWaferCarrierHead","ANIM_BULK_CARRIER_SPIN"),
                Pivot("BulkCarrierVerticalActuator","ANIM_BULK_CARRIER_LIFT"),
                Pivot("BulkDownforceLoadCell","ANIM_BULK_PRESSURE_RING"),
                Pivot("BulkSlurryDeliveryPump","ANIM_BULK_SLURRY_PUMP"),
                Pivot("BulkSlurryDispenseNozzle","ANIM_BULK_SLURRY_NOZZLE"),
                Pivot("BulkPadConditioningAssembly","ANIM_BULK_PAD_CONDITIONER"),
                Pivot("OpticalCopperRemovalEndpointSensor","ANIM_BULK_OPTICAL_SENSOR"),
                Pivot("EddyCurrentCopperThicknessGauge","ANIM_EDDY_CURRENT_SENSOR"),
                Pivot("OverheadWaferTransferGantry","ANIM_TRANSFER_GANTRY_X"),
                Pivot("OverheadWaferTransferGantry","ANIM_TRANSFER_GANTRY_Z"),
                Pivot("OverheadWaferTransferGantry","ANIM_TRANSFER_GRIPPER"),
                Pivot("SoftLandingPolishingPlaten","ANIM_SOFT_PLATEN_ROTATION"),
                Pivot("SoftLandingPrecisionPad","ANIM_SOFT_PAD_ROTATION"),
                Pivot("SoftLandingCarrierHead","ANIM_SOFT_CARRIER_SPIN"),
                Pivot("SoftLandingForceRegulator","ANIM_SOFT_CARRIER_LIFT"),
                Pivot("SoftLandingForceRegulator","ANIM_SOFT_PRESSURE_ACTUATOR"),
                Pivot("SelectiveProcessFluidManifold","ANIM_SOFT_PROCESS_FLUID_NOZZLE"),
                Pivot("BarrierClearingPolishingPlaten","ANIM_BARRIER_PLATEN_ROTATION"),
                Pivot("BarrierSelectivePolishingPad","ANIM_BARRIER_PAD_ROTATION"),
                Pivot("BarrierClearingCarrierHead","ANIM_BARRIER_CARRIER_SPIN"),
                Pivot("BarrierClearingCarrierHead","ANIM_BARRIER_CARRIER_LIFT"),
                Pivot("BarrierFluidMeteringPump","ANIM_BARRIER_SLURRY_PUMP"),
                Pivot("BarrierFluidDispenseNozzle","ANIM_BARRIER_SLURRY_NOZZLE"),
                Pivot("DualWavelengthBarrierEndpointSpectrometer","ANIM_BARRIER_SPECTROMETER_HEAD"),
                Pivot("TransparentOpticalEndpointViewport","ANIM_BARRIER_OPTICAL_WINDOW"),
                Pivot("BarrierClearanceTomographyMonitor","ANIM_BARRIER_TOMOGRAPHY"),
                Pivot("MaterialSelectivityAnalyzer","ANIM_SELECTIVITY_MANIFOLD"),
                Pivot("CopperCorrosionPotentialMonitor","ANIM_CORROSION_POTENTIAL_PROBE"),
                Pivot("PostCMPBrushCleaningChamber","ANIM_BRUSH_CLEANER"),
                Pivot("PrecisionMegasonicCleaningModule","ANIM_MEGASONIC_CLEANING"),
                Pivot("UltrapureRinseWaterModule","ANIM_RINSE_VALVE"),
                Pivot("HighSpeedWaferSpinDryer","ANIM_SPIN_DRYER"),
                Pivot("3DCopperDishingErosionScanner","ANIM_DISHING_EROSION_SCANNER"),
                Pivot("ResidualCopperInspectionScanner","ANIM_COPPER_RESIDUE_CAMERA"),
                Pivot("InterlineLeakageTestInstrument","ANIM_INTERLINE_ISOLATION_PROBE"),
                Pivot("ViaContinuityKelvinProbe","ANIM_VIA_KELVIN_PROBE"),
                Pivot("CMPEmergencySafetyStation","ANIM_ESTOP"),
                Pivot("HolographicCopperInterconnectSCADA","ANIM_INTERCONNECT_SCADA"),
            };
            var pivots=loop.rig.pivots;
            pivots[1].SetParent(pivots[0],true);
            pivots[2].SetParent(pivots[1],true);
            pivots[15].SetParent(pivots[14],true);
            pivots[16].SetParent(pivots[15],true);
            pivots[6].SetParent(pivots[7],true);
            pivots[19].SetParent(pivots[20],true);
            pivots[25].SetParent(pivots[26],true);
            Light Lamp(string name, Vector3 position, Color color, float intensity, float range)
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
            Lamp("CMP work light",new Vector3(0,floor+.62f,-.14f),new Color(.82f,.91f,1),2.3f,1.2f);
            Lamp("Metrology fill",new Vector3(.54f,floor+.34f,.16f),new Color(.42f,.78f,1),.85f,.72f);
            var alarm=Lamp("Critical fault light",new Vector3(.49f,floor+.29f,-.18f),new Color(1,.13f,.10f),1.2f,.72f);
            var optics=Lamp("Barrier optical light",new Vector3(.29f,floor+.25f,-.05f),new Color(.52f,.27f,1),.6f,.44f);
            alarm.enabled=optics.enabled=false;
            var fluidPath=Materials+"/CMP Process Spray.mat";
            var fluidMaterial=AssetDatabase.LoadAssetAtPath<Material>(fluidPath);
            var particleShader=Shader.Find("Universal Render Pipeline/Particles/Unlit");
            if (fluidMaterial==null && particleShader != null)
            {
                fluidMaterial=new Material(particleShader) {name="CMP Process Spray"};
                AssetDatabase.CreateAsset(fluidMaterial,fluidPath);
            }
            ParticleSystem Slurry(int index, Color tint)
            {
                var pivot=pivots[index];
                var nozzle=Child(bench,"Process spray " + index,bench.InverseTransformPoint(pivot.position));
                nozzle.SetParent(pivot,true);
                nozzle.rotation=bench.rotation*Quaternion.Euler(90,0,0);
                var effect=nozzle.gameObject.AddComponent<ParticleSystem>();
                var main=effect.main;
                main.playOnAwake=false;
                main.startLifetime=.28f;
                main.startSpeed=.18f;
                main.startSize=.0065f;
                main.startColor=tint;
                main.maxParticles=48;
                main.scalingMode=ParticleSystemScalingMode.Local;
                main.simulationSpace=ParticleSystemSimulationSpace.World;
                var shape=effect.shape;
                shape.angle=13;
                shape.radius=.006f;
                var emission=effect.emission;
                emission.rateOverTime=95;
                if (fluidMaterial != null) effect.GetComponent<ParticleSystemRenderer>().sharedMaterial=fluidMaterial;
                return effect;
            }
            loop.rig.BindEffects(Slurry(10,new Color(1,.66f,.32f)),Slurry(22,new Color(.42f,.89f,1)),
                Slurry(28,new Color(.63f,.48f,1)),alarm,optics);
            if(loop.plot.transform.Find("Trend legend") == null)
            {
                var oldPanel = root.transform.Find("Level 77 Controls");
                if (oldPanel != null) Object.DestroyImmediate(oldPanel.gameObject);
                SparkLevel77Builder.BuildControls(root.transform,loop,font);
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
            loop.controllerDisplay=Readout("Barrier endpoint",new Vector3(-.815f,floor+.075f,.215f),new Color(.46f,.94f,1f));
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
            string path = $"{Materials}/L76 {key}.mat";
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






