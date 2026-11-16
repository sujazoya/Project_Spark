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
    public static class SparkLevel82ModelInstaller
    {
        public const string Folder = "Assets/My_Assets/_Project_Spark/Prefabs/Levels/Level_82_Layout/";
        public const string Prefab = Folder + "Level_82_Layout.prefab";
        public const string ScenePath = "Assets/My_Assets/_Project_Spark/Scenes/MainMenu/MainMenu.unity";
        private static readonly string[] Models = {
            "BlackstarEMCChamberBase","SemiAnechoicShieldedHall","RFAbsorberPyramidWallSet","RFSealedChamberDoor","RotatingEUTTurntable","TurntableServoGearbox","FourChipletMissionComputerEUT","SpacecraftAvionicsChassis","CoPackagedOpticalTransceiver","RemovableEMCShieldCanopy","ConductiveSeamGasketSet","AvionicsAccessHatch","ReferenceGroundPlane","ChassisEquipotentialBondingGrid","ShieldedHarnessBundle","HighSpeedDifferentialCable","FilteredBulkheadFeedthrough","DCFeedthroughFilterArray","CommonModeChokeModule","DifferentialLCFilterModule","ClipOnFerriteCoreRack","CableRoutingTray","360DegreeShieldTerminationClamp","ShieldGasketCompressionTester","EnclosureSeamLeakageProbe","NearFieldMagneticHProbe","NearFieldElectricEProbe","NearFieldScannerXYGantry","NearFieldScannerZLift","EMIComplianceMeasurementReceiver","HighDynamicRangeSpectrumAnalyzer","RFPreselectorAndAttenuator","SpectralCalibrationReferenceSource","BroadbandEmissionMeasurementAntenna","MotorizedAntennaHeightMast","AntennaPolarizationRotationHead","TurntableAzimuthEncoder","ThreeDimensionalRadiationPatternHologram","EMIFieldVectorHeatmapDisplay","CommonModeCurrentClamp","DifferentialCurrentProbe","DCPowerArtificialNetwork","ConductedEmissionMeasurementRack","RFImmunityCouplingFixture","ProgrammableRFSweepGenerator","ProtectedRFAmplifier","TriaxialRFFieldStrengthProbe","RFUniformityCalibrationFrame","RFImmunitySafetyPLC","ChamberDoorInterlockSwitch","AnechoicBoundaryConditionMonitor","AvionicsSensorSignalEmulator","AnalogInputSusceptibilityAnalyzer","OpticalLinkJitterAndControlMonitor","PrecisionClockEMIProbe","SwitchModePowerConverterModel","SpreadSpectrumClockController","ProtectedSensorInputFilter","InfraredThermalInspectionCamera","ShieldedCoolingAirDuct","RealTimeMissionWorkloadGenerator","FaultDiagnosisAndRepairRouter","CableInjectionFaultFixture","GroundReturnFaultSwitch","SeamGapFaultActuator","RFDesenseFaultEmulator","HolographicEMCAnalysisSCADA","EMCTestSequencingPLCRack","Regulated5VControlSupply","RFSafetyPowerCabinet","EMCCertificationHologram","EmergencyRFStopStation"
        };
        private const string Materials = Folder + "Models/Materials";
        private const float SocketHeight = .025f;
        private const float SocketDiameter = .026f;

        private static readonly (string id, string label, float x, float z, string color)[] Sockets =
        {
            ("SUPPLY_POS","SUP POS",-1.0000f,0.6800f,"red"),
            ("PLC_5V","PLC 5V",-0.9360f,0.6800f,"red"),
            ("SUPPLY_RET","SUP RET",-0.8720f,0.6800f,"blue"),
            ("PLC_GND","PLC GND",-0.8080f,0.6800f,"blue"),
            ("SCAN_5V","SCAN 5V",-0.7440f,0.6800f,"red"),
            ("SCAN_GND","SCAN GND",-0.6800f,0.6800f,"blue"),
            ("EMI_5V","EMI 5V",-0.6160f,0.6800f,"red"),
            ("EMI_GND","EMI GND",-0.5520f,0.6800f,"blue"),
            ("FIELD_5V","FIELD 5V",-0.4880f,0.6800f,"red"),
            ("FIELD_GND","FIELD GND",-0.4240f,0.6800f,"blue"),
            ("SAFE_5V","SAFE 5V",-0.3600f,0.6800f,"red"),
            ("SAFE_GND","SAFE GND",-0.2960f,0.6800f,"blue"),
            ("AZ_CMD","AZ CMD",-0.2320f,0.6800f,"mint"),
            ("TABLE_SET","TABLE SET",-0.1680f,0.6800f,"mint"),
            ("AZ_ENCODER","AZ ENCODER",-0.1040f,0.6800f,"mint"),
            ("AZ_RAW","AZ RAW",-0.0400f,0.6800f,"mint"),
            ("AZ_ACTUAL","AZ ACTUAL",0.0240f,0.6800f,"mint"),
            ("GEAR_CMD","GEAR CMD",0.0880f,0.6800f,"mint"),
            ("TABLE_DRIVE","TABLE DRIVE",0.1520f,0.6800f,"mint"),
            ("GEAR_READY","GEAR READY",0.2160f,0.6800f,"mint"),
            ("GEAR_OK","GEAR OK",0.2800f,0.6800f,"mint"),
            ("MAST_CMD","MAST CMD",0.3440f,0.6800f,"mint"),
            ("HEIGHT_SET","HEIGHT SET",0.4080f,0.6800f,"mint"),
            ("MAST_ENCODER","MAST ENCODER",0.4720f,0.6800f,"mint"),
            ("HEIGHT_ACTUAL","HEIGHT ACTUAL",0.5360f,0.6800f,"mint"),
            ("POL_CMD","POL CMD",0.6000f,0.6800f,"mint"),
            ("POL_SET","POL SET",0.6640f,0.6800f,"mint"),
            ("POL_ENCODER","POL ENCODER",0.7280f,0.6800f,"mint"),
            ("POL_ACTUAL","POL ACTUAL",0.7920f,0.6800f,"mint"),
            ("ANT_READY","ANT READY",0.8560f,0.6800f,"mint"),
            ("ANT_CHECK","ANT CHECK",0.9200f,0.6800f,"mint"),
            ("ANT_MEASURE","ANT MEASURE",0.9840f,0.6800f,"mint"),
            ("ANT_STATUS","ANT STATUS",-1.0000f,0.5450f,"mint"),
            ("XY_CMD","XY CMD",-0.9360f,0.5450f,"mint"),
            ("GANTRY_XY","GANTRY XY",-0.8720f,0.5450f,"mint"),
            ("XY_ENCODER","XY ENCODER",-0.8080f,0.5450f,"mint"),
            ("XY_ACTUAL","XY ACTUAL",-0.7440f,0.5450f,"mint"),
            ("Z_CMD","Z CMD",-0.6800f,0.5450f,"mint"),
            ("GANTRY_Z","GANTRY Z",-0.6160f,0.5450f,"mint"),
            ("Z_ENCODER","Z ENCODER",-0.5520f,0.5450f,"mint"),
            ("Z_ACTUAL","Z ACTUAL",-0.4880f,0.5450f,"mint"),
            ("MAG_CMD","MAG CMD",-0.4240f,0.5450f,"mint"),
            ("H_PROBE","H PROBE",-0.3600f,0.5450f,"mint"),
            ("ELEC_CMD","ELEC CMD",-0.2960f,0.5450f,"mint"),
            ("E_PROBE","E PROBE",-0.2320f,0.5450f,"mint"),
            ("AZ_HOME","AZ HOME",-0.1680f,0.5450f,"mint"),
            ("HOME_SET","HOME SET",-0.1040f,0.5450f,"mint"),
            ("LISN_CMD","LISN CMD",-0.0400f,0.5450f,"mint"),
            ("LISN_ENABLE","LISN ENABLE",0.0240f,0.5450f,"mint"),
            ("LISN_OUT","LISN OUT",0.0880f,0.5450f,"mint"),
            ("CONDUCTED_IN","COND IN",0.1520f,0.5450f,"mint"),
            ("CONDSPEC_OUT","CONDSPEC OUT",0.2160f,0.5450f,"mint"),
            ("EMI_COND","EMI COND",0.2800f,0.5450f,"mint"),
            ("CM_CLAMP","CM CLAMP",0.3440f,0.5450f,"mint"),
            ("CM_I","CM I",0.4080f,0.5450f,"mint"),
            ("DM_CLAMP","DM CLAMP",0.4720f,0.5450f,"mint"),
            ("DM_I","DM I",0.5360f,0.5450f,"mint"),
            ("CM_CAL","CM CAL",0.6000f,0.5450f,"mint"),
            ("CM_ZERO","CM ZERO",0.6640f,0.5450f,"mint"),
            ("DM_CAL","DM CAL",0.7280f,0.5450f,"mint"),
            ("DM_ZERO","DM ZERO",0.7920f,0.5450f,"mint"),
            ("BOND_MON","BOND MON",0.8560f,0.5450f,"mint"),
            ("CHASSIS_OK","CHASSIS OK",0.9200f,0.5450f,"mint"),
            ("TERM_OUT","TERM OUT",0.9840f,0.5450f,"mint"),
            ("SHIELD_TERM","SHIELD TERM",-1.0000f,0.4250f,"mint"),
            ("HARNESS_CM","HARNESS CM",-0.9370f,0.4250f,"mint"),
            ("CM_MON","CM MON",-0.8740f,0.4250f,"mint"),
            ("HARNESS_DM","HARNESS DM",-0.8110f,0.4250f,"mint"),
            ("DM_MON","DM MON",-0.7480f,0.4250f,"mint"),
            ("CHOKE_OUT","CHOKE OUT",-0.6850f,0.4250f,"mint"),
            ("CHOKE_ATTEN","CHOKE ATTEN",-0.6220f,0.4250f,"mint"),
            ("LC_OUT","LC OUT",-0.5590f,0.4250f,"mint"),
            ("LC_ATTEN","LC ATTEN",-0.4960f,0.4250f,"mint"),
            ("FEED_OUT","FEED OUT",-0.4330f,0.4250f,"mint"),
            ("DC_PATH","DC PATH",-0.3700f,0.4250f,"mint"),
            ("SWITCH_OUT","SWITCH OUT",-0.3070f,0.4250f,"mint"),
            ("RIPPLE_IN","RIPPLE IN",-0.2440f,0.4250f,"mint"),
            ("COND_SCAN","COND SCAN",-0.1810f,0.4250f,"mint"),
            ("COND_MARGIN","COND MARGIN",-0.1180f,0.4250f,"mint"),
            ("BW_CMD","BW CMD",-0.0550f,0.4250f,"mint"),
            ("EMI_BW","EMI BW",0.0080f,0.4250f,"mint"),
            ("SPEC_REF","SPEC REF",0.0710f,0.4250f,"mint"),
            ("EMI_CAL","EMI CAL",0.1340f,0.4250f,"mint"),
            ("SCADA_COND","SCADA COND",0.1970f,0.4250f,"mint"),
            ("CONDUCT_CMD","CONDUCT CMD",0.2600f,0.4250f,"mint"),
            ("CONDUCT_RUN","CONDUCT RUN",0.3230f,0.4250f,"mint"),
            ("RADIATE_CMD","RADIATE CMD",0.3860f,0.4250f,"mint"),
            ("RADIATE_RUN","RADIATE RUN",0.4490f,0.4250f,"mint"),
            ("ANT_RF","ANT RF",0.5120f,0.4250f,"mint"),
            ("EMI_RF","EMI RF",0.5750f,0.4250f,"mint"),
            ("EMI_OUT","EMI OUT",0.6380f,0.4250f,"mint"),
            ("SPEC_IN","SPEC IN",0.7010f,0.4250f,"mint"),
            ("SCADA_RAD","SCADA RAD",0.7640f,0.4250f,"mint"),
            ("SPEC_CAL","SPEC CAL",0.8270f,0.4250f,"mint"),
            ("SPEC_ZERO","SPEC ZERO",0.8900f,0.4250f,"mint"),
            ("PRESEL_OUT","PRESEL OUT",0.9530f,0.4250f,"mint"),
            ("PRESEL_IN","PRESEL IN",1.0160f,0.4250f,"mint"),
            ("FILTER_CMD","FILTER CMD",-1.0000f,-0.3550f,"mint"),
            ("EMI_FILTER","EMI FILTER",-0.9420f,-0.3550f,"mint"),
            ("HEIGHT_ACT","HEIGHT ACT",-0.8840f,-0.3550f,"mint"),
            ("HEIGHT_TAG","HEIGHT TAG",-0.8260f,-0.3550f,"mint"),
            ("POL_ACT","POL ACT",-0.7680f,-0.3550f,"mint"),
            ("POL_TAG","POL TAG",-0.7100f,-0.3550f,"mint"),
            ("AZ_ACT","AZ ACT",-0.6520f,-0.3550f,"mint"),
            ("AZ_TAG","AZ TAG",-0.5940f,-0.3550f,"mint"),
            ("H_OUT","H OUT",-0.5360f,-0.3550f,"mint"),
            ("H_SAMPLE","H SAMPLE",-0.4780f,-0.3550f,"mint"),
            ("E_OUT","E OUT",-0.4200f,-0.3550f,"mint"),
            ("E_SAMPLE","E SAMPLE",-0.3620f,-0.3550f,"mint"),
            ("SEAM_OUT","SEAM OUT",-0.3040f,-0.3550f,"mint"),
            ("SEAM_SAMPLE","SEAM SAMPLE",-0.2460f,-0.3550f,"mint"),
            ("WORST_OUT","WORST OUT",-0.1880f,-0.3550f,"mint"),
            ("WORST_VALID","WORST VALID",-0.1300f,-0.3550f,"mint"),
            ("PEAK_OUT","PEAK OUT",-0.0720f,-0.3550f,"mint"),
            ("PEAK_CERT","PEAK CERT",-0.0140f,-0.3550f,"mint"),
            ("CAL_SRC","CAL SRC",0.0440f,-0.3550f,"mint"),
            ("CAL_REF","CAL REF",0.1020f,-0.3550f,"mint"),
            ("HEATMAP_OUT","HEATMAP OUT",0.1600f,-0.3550f,"mint"),
            ("HEATMAP_IN","HEATMAP IN",0.2180f,-0.3550f,"mint"),
            ("NEAR_CMD","NEAR CMD",0.2760f,-0.3550f,"mint"),
            ("PROBE_ENABLE","PROBE ENABLE",0.3340f,-0.3550f,"mint"),
            ("GASKET_CMD","GASKET CMD",0.3920f,-0.3550f,"mint"),
            ("GASKET_SET","GASKET SET",0.4500f,-0.3550f,"mint"),
            ("GASKET_OUT","GASKET OUT",0.5080f,-0.3550f,"mint"),
            ("GASKET_ACT","GASKET ACT",0.5660f,-0.3550f,"mint"),
            ("CLAMP_CMD","CLAMP CMD",0.6240f,-0.3550f,"mint"),
            ("SHIELD_CLAMP","SHIELD CLAMP",0.6820f,-0.3550f,"mint"),
            ("CLAMP_OUT","CLAMP OUT",0.7400f,-0.3550f,"mint"),
            ("CLAMP_CONTACT","CLAMP CONTACT",0.7980f,-0.3550f,"mint"),
            ("CHOKE_CMD","CHOKE CMD",0.8560f,-0.3550f,"mint"),
            ("CHOKE_SET","CHOKE SET",0.9140f,-0.3550f,"mint"),
            ("CHOKE_ACT","CHOKE ACT",0.9720f,-0.3550f,"mint"),
            ("CHOKE_OK","CHOKE OK",1.0300f,-0.3550f,"mint"),
            ("LC_CMD","LC CMD",-1.0000f,-0.5250f,"mint"),
            ("LC_SET","LC SET",-0.9420f,-0.5250f,"mint"),
            ("LC_ACT","LC ACT",-0.8840f,-0.5250f,"mint"),
            ("LC_OK","LC OK",-0.8260f,-0.5250f,"mint"),
            ("FERRITE_CMD","FERRITE CMD",-0.7680f,-0.5250f,"mint"),
            ("FERRITE_SET","FERRITE SET",-0.7100f,-0.5250f,"mint"),
            ("FERRITE_ACT","FERRITE ACT",-0.6520f,-0.5250f,"mint"),
            ("FERRITE_OK","FERRITE OK",-0.5940f,-0.5250f,"mint"),
            ("FEED_CMD","FEED CMD",-0.5360f,-0.5250f,"mint"),
            ("FEED_SET","FEED SET",-0.4780f,-0.5250f,"mint"),
            ("FEED_ACT","FEED ACT",-0.4200f,-0.5250f,"mint"),
            ("FEED_OK","FEED OK",-0.3620f,-0.5250f,"mint"),
            ("ROUTE_CMD","ROUTE CMD",-0.3040f,-0.5250f,"mint"),
            ("ROUTE_SET","ROUTE SET",-0.2460f,-0.5250f,"mint"),
            ("ROUTE_ACTUAL","ROUTE ACTUAL",-0.1880f,-0.5250f,"mint"),
            ("BOND_CHECK","BOND CHECK",-0.1300f,-0.5250f,"mint"),
            ("BOND_INTEGRITY","BOND INTEGRITY",-0.0720f,-0.5250f,"mint"),
            ("SENSOR_FILTER","SENSOR FILTER",-0.0140f,-0.5250f,"mint"),
            ("SENSOR_RESP","SENSOR RESP",0.0440f,-0.5250f,"mint"),
            ("RF_SWEEP_REQ","RF SWEEP REQ",0.1020f,-0.5250f,"mint"),
            ("RF_WAVE","RF WAVE",0.1600f,-0.5250f,"mint"),
            ("IMMUNITY_OK","IMM OK",0.2180f,-0.5250f,"mint"),
            ("AMP_ACTUAL","AMP ACTUAL",0.2760f,-0.5250f,"mint"),
            ("FIELD_AUX","FIELD AUX",0.3340f,-0.5250f,"mint"),
            ("UNIFORMITY_CMD","UNIFORMITY CMD",0.3920f,-0.5250f,"mint"),
            ("FIELD_REF","FIELD REF",0.4500f,-0.5250f,"mint"),
            ("FIELD_CAL","FIELD CAL",0.5080f,-0.5250f,"mint"),
            ("SWEEP_TELEM","SWEEP TELEM",0.5660f,-0.5250f,"mint"),
            ("COUPLE_FIXTURE","COUPLE FIXTURE",0.6240f,-0.5250f,"mint"),
            ("FIXTURE_OK","FIXTURE OK",0.6820f,-0.5250f,"mint"),
            ("FIXTURE_STOP","FIXTURE STOP",0.7400f,-0.5250f,"amber"),
            ("SENSOR_CHECK","SENSOR CHECK",0.7980f,-0.5250f,"mint"),
            ("OPTICAL_CHECK","OPTICAL CHECK",0.8560f,-0.5250f,"mint"),
            ("CLOCK_CHECK","CLOCK CHECK",0.9140f,-0.5250f,"mint"),
            ("MISSION_REQ","MISSION REQ",0.9720f,-0.5250f,"mint"),
            ("ADC_SAMPLE","ADC SAMPLE",1.0300f,-0.5250f,"mint"),
            ("ADC_DEV","ADC DEV",-1.0000f,-0.7000f,"mint"),
            ("PACKET_JITTER","PACKET JITTER",-0.9440f,-0.7000f,"mint"),
            ("CLOCK_JITTER","CLOCK JITTER",-0.8880f,-0.7000f,"mint"),
            ("ADC_COUPLING","ADC COUPLING",-0.8320f,-0.7000f,"mint"),
            ("SPREAD_TELEM","SPREAD TELEM",-0.7760f,-0.7000f,"mint"),
            ("SENSOR_FILTERED","SENSOR FILTERED",-0.7200f,-0.7000f,"mint"),
            ("IR_THERMAL","IR THERMAL",-0.6640f,-0.7000f,"mint"),
            ("WORKLOAD_OK","WORKLOAD OK",-0.6080f,-0.7000f,"mint"),
            ("CMD_SAFE","CMD SAFE",-0.5520f,-0.7000f,"amber"),
            ("CM_FAULT","CM FAULT",-0.4960f,-0.7000f,"amber"),
            ("RETURN_FAULT","RETURN FAULT",-0.4400f,-0.7000f,"amber"),
            ("SEAM_FAULT","SEAM FAULT",-0.3840f,-0.7000f,"amber"),
            ("DESENSE_FAULT","DESENSE FAULT",-0.3280f,-0.7000f,"amber"),
            ("DIAGNOSE","DIAGNOSE",-0.2720f,-0.7000f,"mint"),
            ("SCADA_REPAIR","SCADA REPAIR",-0.2160f,-0.7000f,"mint"),
            ("RF_ARM_REQ","RF ARM REQ",-0.1600f,-0.7000f,"mint"),
            ("DOOR_NC","DOOR NC",-0.1040f,-0.7000f,"mint"),
            ("DOOR_CLOSED_IN","DOOR CLOSED IN",-0.0480f,-0.7000f,"mint"),
            ("RF_FIELD_MEASURED_OUT","FIELD OUT",0.0080f,-0.7000f,"mint"),
            ("RF_SAFE_FIELD_FEEDBACK_IN","RF SAFE IN",0.0640f,-0.7000f,"amber"),
            ("SCADA_FIELD","SCADA FIELD",0.1200f,-0.7000f,"mint"),
            ("AMP_PERMIT","AMP PERMIT",0.1760f,-0.7000f,"amber"),
            ("AMP_ENABLE","AMP ENABLE",0.2320f,-0.7000f,"mint"),
            ("RF_OVERRIDE","RF OVERRIDE",0.2880f,-0.7000f,"mint"),
            ("ESTOP_NC","ESTOP NC",0.3440f,-0.7000f,"amber"),
            ("SAFE_STOP_IN","SAFE STOP IN",0.4000f,-0.7000f,"amber"),
            ("AMP_DISABLED","AMP DISABLED",0.4560f,-0.7000f,"mint"),
            ("FIELD_HEALTH","FIELD HEALTH",0.5120f,-0.7000f,"mint"),
            ("FIELD_LIMIT","FIELD LIMIT",0.5680f,-0.7000f,"mint"),
            ("EXPOSURE_AZ","EXPOSURE AZ",0.6240f,-0.7000f,"mint"),
            ("EXPOSURE_HEIGHT","EXPOSURE HEIGHT",0.6800f,-0.7000f,"mint"),
            ("EUT_VALID","EUT VALID",0.7360f,-0.7000f,"mint"),
            ("SEAL_OK","SEAL OK",0.7920f,-0.7000f,"mint"),
            ("RF_LATCH","RF LATCH",0.8480f,-0.7000f,"mint"),
            ("RF_RESET","RF RESET",0.9040f,-0.7000f,"mint"),
            ("OBJECTIVE_STATE","OBJECTIVE STATE",0.9600f,-0.7000f,"mint"),
            ("EMC_CERT","EMC CERT",1.0160f,-0.7000f,"mint"),
        };

        public static IEnumerable<string> TerminalIds => Sockets.Select(socket => socket.id);

        private static readonly Dictionary<string, string> Palette = new()
        {
            {"red","EE485C"},{"blue","1711E2"},{"amber","FFA82B"},{"violet","A282FF"},{"plasma","A45DFF"},
            {"mint","34F6C5"},{"cyan","23DAFD"},{"void","07111C"},{"white","D4F5FF"},{"glass","498BA8"}
        };

        [MenuItem("Tools/Project Spark/Levels/Create Level 82")]
        public static void Install()
        {
            SparkLevel82Builder.Create();
            var root = AssetDatabase.LoadAssetAtPath<GameObject>(Prefab);
            if (root == null || root.GetComponent<SparkLevel82Process>().controllerDisplay == null) Fit();
            Register();
            ValidateAssets();
        }

        [MenuItem("Tools/Project Spark/Level 82/Validate Level Assets")]
        public static void ValidateAssets()
        {
            var root=AssetDatabase.LoadAssetAtPath<GameObject>(Prefab);
            var control=root != null ? root.GetComponent<SparkLevel82Process>() : null;
            var bindings=root != null ? root.GetComponent<SparkLevelSceneBindings>() : null;
            if (control==null || bindings==null || control.controllerDisplay==null || control.qualityDisplay==null || control.rig==null || control.rig.pivots.Length!=54 ||
                control.pins.Length!=206 || control.pins.Distinct().Count()!=206 || 
                !TerminalIds.SequenceEqual(control.pins.Select(pin=>pin.name)) ||
                control.pins.Any(pin=>!bindings.TryGetTerminal(pin.name,out var bound) || bound!=pin) ||
                SparkLevel82Process.Gates.Any(id=>!bindings.IsComponentIdDeclared(id)))
                throw new InvalidOperationException("Level 82 wiring, motion or binding references are incomplete.");
            Debug.Log("[LEVEL 82] Validated 206 distinct terminals, 134 player wires and 6 evaluator gates.",root);
        }

        private static void Register()
        {
            var scene = SceneManager.GetSceneByPath(ScenePath);
            bool opened = !scene.isLoaded;
            if (opened) scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
            bool dirty = scene.isDirty;
            var objects = scene.GetRootGameObjects();
            T Find<T>() where T : Component => objects.SelectMany(item => item.GetComponentsInChildren<T>(true)).Single();
            var level = AssetDatabase.LoadAssetAtPath<SparkLevelDefinition>(Folder + "Level_82.asset");
            var drawing = AssetDatabase.LoadAssetAtPath<SparkLevelDrawingAsset>(Folder + "Level_82_Drawing.asset");
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
            else if (dirty) Debug.Log("MainMenu had unsaved edits. Save the scene to persist Level 82's campaign and teaching entries.");
        }

        private static void Append(Object target, string field, Object value)
        {
            if (value == null) throw new InvalidOperationException("Missing Level 82 asset for " + field);
            var data = new SerializedObject(target);
            var list = data.FindProperty(field);
            for (int i = 0; i < list.arraySize; i++)
                if (list.GetArrayElementAtIndex(i).objectReferenceValue == value) return;
            list.arraySize++;
            list.GetArrayElementAtIndex(list.arraySize - 1).objectReferenceValue = value;
            data.ApplyModifiedProperties();
        }

        [MenuItem("Tools/Project Spark/Level 82/Fit Breadboard Assembly")]
        public static void Fit()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Exit Play Mode before fitting Level 82.");
            var models = Models.Select((name, index) =>
            {
                string path = Folder + $"Models/L82_{index:00}_{name}_Fitted.fbx";
                return AssetDatabase.LoadAssetAtPath<GameObject>(path) ??
                    throw new InvalidOperationException("Missing fitted kit module " + path);
            }).ToArray();
            if (AssetDatabase.LoadAssetAtPath<GameObject>(Prefab) == null)
                throw new InvalidOperationException("The Level 82 prefab is missing.");

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
                    throw new InvalidOperationException("Unity could not save the Level 82 layout.");
                AssetDatabase.SaveAssets();
                Debug.Log("Level 82 fitted to the MainMenu breadboard with 206 seated terminals.");
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
            var bench = root.transform.Find("EMC Workbench");
            var bindings = root.GetComponent<SparkLevelSceneBindings>();
            var loop = root.GetComponent<SparkLevel82Process>();
            if (bench == null || bindings == null || loop == null)
                throw new InvalidOperationException("Level 82 is missing its workbench, controls, or scene bindings.");

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

            foreach (string name in new[] {"L82_HD_VISUAL_ART", "Breadboard Labels", "Motion pivots"})
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
            var assembly = Child(bench, "L82_HD_VISUAL_ART", new Vector3(0,floor,-.010f));
            foreach (var model in models)
            {
                var instance = Object.Instantiate(model, assembly, false).transform;
                var markers = instance.GetComponentsInChildren<Transform>(true)
                    .Where(item => item.name.StartsWith("L82_Board",StringComparison.Ordinal))
                    .ToDictionary(item => item.name);
                Transform Marker(string name) => markers["L82_Board" + name];
                Vector3 Axis(string name) => assembly.InverseTransformVector(Marker(name).position - Marker("Center").position);
                instance.localRotation = Quaternion.Inverse(Quaternion.LookRotation(-Axis("Front"), Axis("Up"))) * instance.localRotation;
                instance.localScale /= Axis("Right").magnitude;
                instance.localPosition -= assembly.InverseTransformPoint(Marker("Center").position);
                if (Axis("Right").x < .99f)
                    throw new InvalidOperationException("Unexpected axes on fitted model " + model.name);
            }

            assembly.localScale = Vector3.one * .45f;
            if (!AssetDatabase.IsValidFolder(Materials)) AssetDatabase.CreateFolder(Folder + "Models", "Materials");
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) throw new InvalidOperationException("The project URP Lit shader is missing.");
            ConfigureTextures();
            foreach (var mesh in assembly.GetComponentsInChildren<MeshRenderer>())
            {
                string key = mesh.name.Substring(mesh.name.LastIndexOf("__", StringComparison.Ordinal) + 2);
                if(key=="tint1D303F" && mesh.name.StartsWith("L82_RFShieldedSemiAnechoicChamber_",StringComparison.Ordinal)) key="glass";
                string surface=key switch
                {
                    "tintFDB344"=>"HazardYellow",
                    "tintDC803E" or "tintD89D4C"=>"GroundCopper",
                    "tintE9BC56"=>"GoldContact",
                    "tint121B28" or "tint1C2735"=>"AbsorberFoam",
                    "tint262B34" when mesh.name.Contains("Ferrite")=>"FerriteComposite",
                    "tintCAD8E7"=>"AntennaTitanium",
                    "tint0C1620" when mesh.name.Contains("MissionComputer")=>"SpacecraftPCB",
                    "tint1D2637"=>"EMCBlueDisplay",
                    "tint0D1D2F" when mesh.name.Contains("ChamberBase")=>"AnechoicFloor",
                    "tint262B34" when mesh.name.Contains("Filter")=>"FilterHousing",
                    "tint7A8D9C" when mesh.name.Contains("SemiAnechoic")||mesh.name.Contains("Canopy")=>"RFShieldAlloy",
                    "tint47657C" or "tintC2D0D6" or "tint97B3C4"=>"BrushedSteel",
                    _=>null
                };
                var material = MaterialFor(shader,key,surface);
                // The fitted meshes can contain several imported submeshes.
                mesh.sharedMaterials = Enumerable.Repeat(material, mesh.GetComponent<MeshFilter>().sharedMesh.subMeshCount).ToArray();
                if (key is "glass" or "acrylic") mesh.shadowCastingMode = ShadowCastingMode.Off;
            }

            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (loop.calibrationText == null)
            {
                var oldControls = root.transform.Find("Level 82 Controls");
                if (oldControls != null) Object.DestroyImmediate(oldControls.gameObject);
                SparkLevel82Builder.BuildControls(root.transform,loop,font);
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
                terminalData.FindProperty("maxConnections").intValue = SparkLevel82Process.Wires.Count(wire => wire.a == index || wire.b == index);
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






                ("CONDUCTED / LISN",-.865f,.147f),("RADIATED / MAST",.862f,.147f),
                ("NEAR-FIELD / REPAIR",-.86f,-.235f),("IMMUNITY / SAFETY",.86f,-.235f)
            };
            foreach(var title in titles)
                Label(labels,font,title.title,title.title,new Vector3(title.x,floor+.058f,title.z),.0165f);
            Label(labels,font,"Carrier labels","CONDUCTED  /  RADIATED  /  IMMUNITY",new Vector3(0,floor+.068f,-.252f),.013f);
            Label(labels,font,"Optic path","BLACKSTAR EMC CHAMBER",new Vector3(0,floor+.15f,.13f),.012f);
            Transform Pivot(string module,string key)
            {
                var name="fitted_"+module+"_"+key;
                return assembly.GetComponentsInChildren<Transform>(true).Single(item=>item.name==name);
            }
            loop.rig.art=assembly;
            loop.rig.pivots=new[]{
                Pivot("RFSealedChamberDoor","ANIM_CHAMBER_RF_DOOR"),
                Pivot("RotatingEUTTurntable","ANIM_EUT_TURNTABLE"),
                Pivot("TurntableServoGearbox","ANIM_TURNTABLE_LOCK"),
                Pivot("SpacecraftAvionicsChassis","ANIM_EUT_CHASSIS_LID"),
                Pivot("RemovableEMCShieldCanopy","ANIM_SHIELD_CANOPY_LIFT"),
                Pivot("AvionicsAccessHatch","ANIM_ACCESS_HATCH"),
                Pivot("RFAbsorberPyramidWallSet","ANIM_ABSORBER_SERVICE_PANEL"),
                Pivot("ReferenceGroundPlane","ANIM_GROUND_PLANE_ACCESS"),
                Pivot("ConductiveSeamGasketSet","ANIM_SEAM_GASKET_COMPRESSION"),
                Pivot("ShieldedCoolingAirDuct","ANIM_SHIELDED_AIR_DUCT"),
                Pivot("FourChipletMissionComputerEUT","ANIM_EUT_LOAD_TRAY"),
                Pivot("TurntableAzimuthEncoder","ANIM_AZIMUTH_ENCODER"),
                Pivot("MotorizedAntennaHeightMast","ANIM_ANTENNA_MAST_VERTICAL"),
                Pivot("AntennaPolarizationRotationHead","ANIM_ANTENNA_POLARIZATION"),
                Pivot("BroadbandEmissionMeasurementAntenna","ANIM_ANTENNA_DISTANCE_SLIDE"),
                Pivot("NearFieldScannerXYGantry","ANIM_NEARFIELD_SCAN_X"),
                Pivot("NearFieldScannerXYGantry","ANIM_NEARFIELD_SCAN_Y"),
                Pivot("NearFieldScannerZLift","ANIM_NEARFIELD_SCAN_Z"),
                Pivot("NearFieldElectricEProbe","ANIM_EFIELD_PROBE_ARM"),
                Pivot("NearFieldMagneticHProbe","ANIM_HFIELD_PROBE_ARM"),
                Pivot("EnclosureSeamLeakageProbe","ANIM_SEAM_LEAK_PROBE"),
                Pivot("CommonModeCurrentClamp","ANIM_COMMONMODE_CLAMP"),
                Pivot("DifferentialCurrentProbe","ANIM_DIFFERENTIAL_PROBE"),
                Pivot("RFPreselectorAndAttenuator","ANIM_RF_PRESELECTOR_DIAL"),
                Pivot("HighDynamicRangeSpectrumAnalyzer","ANIM_SPECTRUM_SWEEP_INDICATOR"),
                Pivot("ThreeDimensionalRadiationPatternHologram","ANIM_RADIATION_HOLOGRAM"),
                Pivot("CableRoutingTray","ANIM_HARNESS_CABLE_TRAY"),
                Pivot("HighSpeedDifferentialCable","ANIM_DIFFERENTIAL_CABLE_ROUTE"),
                Pivot("FilteredBulkheadFeedthrough","ANIM_FEEDTHROUGH_PANEL"),
                Pivot("DCFeedthroughFilterArray","ANIM_DC_FILTER_TRAY"),
                Pivot("CommonModeChokeModule","ANIM_COMMONMODE_CHOKE_INSERT"),
                Pivot("DifferentialLCFilterModule","ANIM_LC_FILTER_INSERT"),
                Pivot("ClipOnFerriteCoreRack","ANIM_FERRITE_CLIP_LATCH"),
                Pivot("360DegreeShieldTerminationClamp","ANIM_SHIELD_TERMINATION_CLAMP"),
                Pivot("ChassisEquipotentialBondingGrid","ANIM_BONDING_STRAP_LATCH"),
                Pivot("ShieldGasketCompressionTester","ANIM_GASKET_TEST_HEAD"),
                Pivot("SwitchModePowerConverterModel","ANIM_SWITCHER_POWER_MODULE"),
                Pivot("SpreadSpectrumClockController","ANIM_SPREAD_SPECTRUM_DIAL"),
                Pivot("ProtectedSensorInputFilter","ANIM_SENSOR_FILTER_DRAWER"),
                Pivot("InfraredThermalInspectionCamera","ANIM_THERMAL_CAMERA_PAN"),
                Pivot("RFImmunityCouplingFixture","ANIM_RF_COUPLING_FIXTURE"),
                Pivot("ProgrammableRFSweepGenerator","ANIM_RF_GENERATOR_DIAL"),
                Pivot("ProtectedRFAmplifier","ANIM_RF_AMPLIFIER_SHUTTER"),
                Pivot("TriaxialRFFieldStrengthProbe","ANIM_FIELD_PROBE_CARRIAGE"),
                Pivot("RFUniformityCalibrationFrame","ANIM_RF_CALIBRATION_FRAME"),
                Pivot("ChamberDoorInterlockSwitch","ANIM_DOOR_INTERLOCK_LEVER"),
                Pivot("EmergencyRFStopStation","ANIM_RF_SAFETY_ESTOP"),
                Pivot("AvionicsSensorSignalEmulator","ANIM_SENSOR_INPUT_STREAM"),
                Pivot("OpticalLinkJitterAndControlMonitor","ANIM_OPTICAL_LINK_MONITOR"),
                Pivot("PrecisionClockEMIProbe","ANIM_CLOCK_JITTER_DISPLAY"),
                Pivot("CableInjectionFaultFixture","ANIM_FAULT_CABLE_INJECTOR"),
                Pivot("SeamGapFaultActuator","ANIM_SEAM_FAULT_ACTUATOR"),
                Pivot("HolographicEMCAnalysisSCADA","ANIM_SCADA_EMC_DISPLAY"),
                Pivot("EMCCertificationHologram","ANIM_CERTIFICATION_BEACON"),
            };
            Transform table=loop.rig.pivots.Single(p=>p.name.EndsWith("ANIM_EUT_TURNTABLE",StringComparison.Ordinal));
            var equipment=assembly.GetComponentsInChildren<Transform>(true).Single(p=>p.name=="L82_FourChipletMissionComputerEUT");
            equipment.SetParent(table,true);
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
            Lamp("EMC work light",new Vector3(-.38f,floor+.58f,-.14f),new Color(.91f,.94f,1),1.35f,1.2f);
            Lamp("Metrology fill",new Vector3(.54f,floor+.34f,.16f),new Color(.49f,.80f,1),.64f,.76f);
            loop.rig.BindEffects(
                Lamp("Near-field activity light",new Vector3(-.37f,floor+.20f,-.03f),new Color(.29f,.75f,1),1,.45f),
                Lamp("RF exposure light",new Vector3(0,floor+.20f,-.03f),new Color(.48f,.39f,1),1,.46f),
                Lamp("EUT telemetry light",new Vector3(.28f,floor+.2f,-.03f),new Color(.58f,.37f,1),1,.45f),
                Lamp("Isolation alarm",new Vector3(.56f,floor+.24f,.03f),new Color(1,.18f,.12f),1.25f,.55f));
            if(loop.plot.transform.Find("Band POLAR") == null)
            {
                var oldPanel = root.transform.Find("Level 82 Controls");
                if (oldPanel != null) Object.DestroyImmediate(oldPanel.gameObject);
                SparkLevel82Builder.BuildControls(root.transform,loop,font);
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
            loop.controllerDisplay=Readout("Field readout",new Vector3(-.815f,floor+.075f,.215f),new Color(.46f,.94f,1f));
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

        private static void ConfigureTextures()
        {
            foreach(string guid in AssetDatabase.FindAssets("t:Texture2D",new[]{Folder+"Textures"}))
            {
                string path=AssetDatabase.GUIDToAssetPath(guid);
                if(AssetImporter.GetAtPath(path) is not TextureImporter importer)continue;
                var type=path.EndsWith("_Normal.png",StringComparison.Ordinal)?TextureImporterType.NormalMap:TextureImporterType.Default;
                bool srgb=path.EndsWith("_Albedo.png",StringComparison.Ordinal);
                if(importer.textureType==type&&importer.sRGBTexture==srgb&&importer.wrapMode==TextureWrapMode.Repeat)continue;
                importer.textureType=type;
                importer.sRGBTexture=srgb;
                importer.wrapMode=TextureWrapMode.Repeat;
                importer.SaveAndReimport();
            }
        }

        private static Material MaterialFor(Shader shader, string key,string surface=null)
        {
            string materialName=surface==null?"L82 "+key:"L82 "+key+" "+surface;
            string path = $"{Materials}/{materialName}.mat";
            if (key.StartsWith("tint",StringComparison.Ordinal) && key.Length==10) Palette[key]=key.Substring(4);
            var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing != null && key is not ("glass" or "acrylic") &&
                ColorUtility.TryParseHtmlString("#" + (Palette.TryGetValue(key, out var savedTint) ? savedTint : "9FB3C8"), out var savedColor) &&
                existing.GetColor("_BaseColor") == savedColor) return existing;
            ColorUtility.TryParseHtmlString("#" + (Palette.TryGetValue(key, out var tint) ? tint : "9FB3C8"), out var color);
            var material = existing != null ? existing : new Material(shader) { name = materialName };
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
            if(surface!=null)
            {
                string prefix=Folder+"Textures/";
                var albedo=AssetDatabase.LoadAssetAtPath<Texture2D>(prefix+"Authored/"+surface+"_Albedo.png");
                var normal=AssetDatabase.LoadAssetAtPath<Texture2D>(prefix+"Authored/"+surface+"_Normal.png");
                var packed=AssetDatabase.LoadAssetAtPath<Texture2D>(prefix+surface+"_MetalSmoothAO.png");
                if(albedo==null||normal==null||packed==null)
                    throw new InvalidOperationException("Missing Level 82 surface maps for "+surface);
                material.SetTexture("_BaseMap",albedo);
                material.SetTexture("_BumpMap",normal);
                material.SetFloat("_BumpScale",.47f);
                material.EnableKeyword("_NORMALMAP");
                material.SetTexture("_MetallicGlossMap",packed);
                material.EnableKeyword("_METALLICSPECGLOSSMAP");
                material.SetTexture("_OcclusionMap",packed);
                material.SetFloat("_OcclusionStrength",.42f);
                material.EnableKeyword("_OCCLUSIONMAP");
                material.SetFloat("_Smoothness",.84f);
                float alpha=color.a;
                color=Color.Lerp(color,Color.white,.48f);
                color.a=alpha;
            }
            material.SetColor("_BaseColor", color);
            if (existing == null) AssetDatabase.CreateAsset(material, path);
            else EditorUtility.SetDirty(material);
            return material;
        }
    }
}
#endif





















