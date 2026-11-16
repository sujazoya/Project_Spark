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
    public static class SparkLevel80ModelInstaller
    {
        public const string Folder = "Assets/My_Assets/_Project_Spark/Prefabs/Levels/Level_80_Layout/";
        public const string Prefab = Folder + "Level_80_Layout.prefab";
        public const string ScenePath = "Assets/My_Assets/_Project_Spark/Scenes/MainMenu/MainMenu.unity";
        private static readonly string[] Models = {
            "IndustrialPhotonicDataAtriumBase","TransparentPhotonicCoreEnclosure","CompletedFourChipletPackageCarrier","BondedFourChipletElectronicAssembly","CoPackagedSiliconPhotonicInterposer","HolographicOpticalWaveguideMap","PhotonicEdgeCouplerArray","FiberRibbonDockingStation","OpticalFiberRibbonCassette","MultiWavelengthLaserSourceRack","LaserCombStabilizationModule","LaserDriveController","LaserTemperatureControlMount","OpticalSafetyShutter","OpticalIsolatorModule","CalibratedOpticalTapSplitter","LaunchOpticalPowerMeter","PolarizationControlModule","VariableOpticalAttenuator","SpectralReferenceEtalon","HighSpeedTransmitterDriver","ElectroOpticalModulatorArray","ModulatorBiasController","FourChannelWDMMultiplexer","HolographicFourWavelengthCombiner","PhotonicWaveguideCrossSection","PhotonicThermalControlPlatform","MicroringResonatorChannelA","MicroringResonatorChannelB","MicroringResonatorChannelC","MicroringResonatorChannelD","FourZoneMicroringHeaterBank","MicroringHeaterDACController","MicroringThermalSensorArray","ThermalCrosstalkAnalyzer","FourChannelWDMDemultiplexer","DemultiplexerResonanceMonitor","FourChannelOpticalDropPortPanel","PhotodiodeReceiverArray","ReceiverTransimpedanceAmplifierBank","ClockAndDataRecoveryModule","ReceiverOpticalPowerMonitor","HighResolutionOpticalSpectrumAnalyzer","OpticalLinkPowerBudgetMeter","HighSpeedEyeDiagramAnalyzer","OpticalLinkBitErrorRateTester","PAM4WaveformSamplingInstrument","ForwardErrorCorrectionDecoder","PseudorandomBitPatternGenerator","ChipletOpticalTestRouter","MemoryOpticalLinkDiagnosticModule","LogicOpticalTransmitterInterface","IOOpticalOutputInterface","SensorOpticalTelemetryInterface","OpticalFiberMicrobendMonitor","ControlledOpticalFaultInjectionModule","OpticalLinkSupervisoryController","PhotonicLaserSafetyPLCRack","EmergencyOpticalShutterStation","PhotonicCoolingAirManifold","HolographicThermalResonanceDisplay","HolographicOpticalDataSCADA","PhotonicWaveguideDefectMicroscope","StrayLightSafetyMonitor","OpticalCalibrationReferenceCoupon","ESDGuardedPhotonicTestFixture","Regulated5VControlSupply","GiantWavelengthRoutingHologram"
        };
        private const string Materials = Folder + "Models/Materials";
        private const float SocketHeight = .025f;
        private const float SocketDiameter = .026f;

        private static readonly (string id, string label, float x, float z, string color)[] Sockets =
        {
            ("SUPPLY_POS","SUPPLY POS",-1.0000f,0.6800f,"red"),
            ("PLC_5V","PLC 5V",-0.9315f,0.6800f,"red"),
            ("SUPPLY_RET","SUPPLY RET",-0.8630f,0.6800f,"blue"),
            ("PLC_GND","PLC GND",-0.7945f,0.6800f,"blue"),
            ("TX_5V","TX 5V",-0.7260f,0.6800f,"red"),
            ("TX_GND","TX GND",-0.6575f,0.6800f,"blue"),
            ("RING_5V","RING 5V",-0.5890f,0.6800f,"red"),
            ("RING_GND","RING GND",-0.5205f,0.6800f,"blue"),
            ("RX_5V","RX 5V",-0.4520f,0.6800f,"red"),
            ("RX_GND","RX GND",-0.3835f,0.6800f,"blue"),
            ("SAFE_5V","SAFE 5V",-0.3150f,0.6800f,"red"),
            ("SAFE_GND","SAFE GND",-0.2465f,0.6800f,"blue"),
            ("LASER_PERMIT","LASER PERMIT",-0.1780f,0.6800f,"amber"),
            ("LASER_ENABLE","LASER ENABLE",-0.1095f,0.6800f,"mint"),
            ("LASER_CMD","LASER CMD",-0.0410f,0.6800f,"mint"),
            ("LASER_DAC","LASER DAC",0.0275f,0.6800f,"mint"),
            ("LAUNCH_TAP","LAUNCH TAP",0.0960f,0.6800f,"mint"),
            ("LAUNCH_IN","LAUNCH IN",0.1645f,0.6800f,"mint"),
            ("SOURCE_POWER","SOURCE POWER",0.2330f,0.6800f,"mint"),
            ("SHUTTER_CMD","SHUTTER CMD",0.3015f,0.6800f,"amber"),
            ("SHUTTER_OPEN","SHUTTER OPEN",0.3700f,0.6800f,"amber"),
            ("SHUTTER_FB","SHUTTER FB",0.4385f,0.6800f,"amber"),
            ("SHUTTER_CLOSED","SHUTTER CLOSED",0.5070f,0.6800f,"amber"),
            ("ATTEN_CMD","ATTEN CMD",0.5755f,0.6800f,"mint"),
            ("ATTEN_DAC","ATTEN DAC",0.6440f,0.6800f,"mint"),
            ("ATTEN_OUT","ATTEN OUT",0.7125f,0.6800f,"mint"),
            ("ATTEN_IN","ATTEN IN",0.7810f,0.6800f,"mint"),
            ("COMB_OUT","COMB OUT",0.8495f,0.6800f,"mint"),
            ("COMB_LOCK","COMB LOCK",0.9180f,0.6800f,"mint"),
            ("LASER_TEMP","LASER TEMP",0.9865f,0.6800f,"mint"),
            ("PLC_TEMP","PLC TEMP",-1.0000f,0.5450f,"mint"),
            ("TEMP_CMD","TEMP CMD",-0.9315f,0.5450f,"mint"),
            ("TEC_SET","TEC SET",-0.8630f,0.5450f,"mint"),
            ("REF_OUT","REF OUT",-0.7945f,0.5450f,"mint"),
            ("SPECTRAL_REF","SPECTRAL REF",-0.7260f,0.5450f,"mint"),
            ("STRAY_OUT","STRAY OUT",-0.6575f,0.5450f,"mint"),
            ("SAFE_STRAY","SAFE STRAY",-0.5890f,0.5450f,"amber"),
            ("ISOLATOR_CMD","ISOLATOR CMD",-0.5205f,0.5450f,"mint"),
            ("ISOLATOR_SET","ISOLATOR SET",-0.4520f,0.5450f,"mint"),
            ("ISOLATOR_OUT","ISOLATOR OUT",-0.3835f,0.5450f,"mint"),
            ("ISO_READY","ISO READY",-0.3150f,0.5450f,"mint"),
            ("POL_OUT","POL OUT",-0.2465f,0.5450f,"mint"),
            ("POL_READY","POL READY",-0.1780f,0.5450f,"mint"),
            ("TAP_HEALTH","TAP HEALTH",-0.1095f,0.5450f,"mint"),
            ("TAP_READY","TAP READY",-0.0410f,0.5450f,"mint"),
            ("SCADA_LAUNCH","SCADA LAUNCH",0.0275f,0.5450f,"mint"),
            ("TX_CMD","TX CMD",0.0960f,0.5450f,"mint"),
            ("TX_ENABLE","TX ENABLE",0.1645f,0.5450f,"mint"),
            ("TX_DRIVE","TX DRIVE",0.2330f,0.5450f,"mint"),
            ("TX_MOD","TX MOD",0.3015f,0.5450f,"mint"),
            ("BIAS_CMD","BIAS CMD",0.3700f,0.5450f,"mint"),
            ("MOD_BIAS","MOD BIAS",0.4385f,0.5450f,"mint"),
            ("MOD_RESP","MOD RESP",0.5070f,0.5450f,"mint"),
            ("TX_RESPONSE","TX RESPONSE",0.5755f,0.5450f,"mint"),
            ("LOGIC_OUT","LOGIC OUT",0.6440f,0.5450f,"mint"),
            ("TX_LOGIC","TX LOGIC",0.7125f,0.5450f,"mint"),
            ("MEMORY_OUT","MEMORY OUT",0.7810f,0.5450f,"mint"),
            ("TX_MEMORY","TX MEMORY",0.8495f,0.5450f,"mint"),
            ("IO_OUT","IO OUT",0.9180f,0.5450f,"mint"),
            ("TX_IO","TX IO",0.9865f,0.5450f,"mint"),
            ("SENSOR_OUT","SENSOR OUT",-1.0000f,0.4250f,"mint"),
            ("TX_SENSOR","TX SENSOR",-0.9330f,0.4250f,"mint"),
            ("PRBS_CMD","PRBS CMD",-0.8660f,0.4250f,"mint"),
            ("PRBS_START","PRBS START",-0.7990f,0.4250f,"mint"),
            ("PRBS_OUT","PRBS OUT",-0.7320f,0.4250f,"mint"),
            ("REF_PATTERN","REF PATTERN",-0.6650f,0.4250f,"mint"),
            ("TX_STATUS","TX STATUS",-0.5980f,0.4250f,"mint"),
            ("TX_ACTIVE","TX ACTIVE",-0.5310f,0.4250f,"mint"),
            ("MUX_CMD","MUX CMD",-0.4640f,0.4250f,"mint"),
            ("MUX_PERMIT","MUX PERMIT",-0.3970f,0.4250f,"amber"),
            ("MUX_OUT","MUX OUT",-0.3300f,0.4250f,"mint"),
            ("MUX_OK","MUX OK",-0.2630f,0.4250f,"mint"),
            ("MOD_CAL","MOD CAL",-0.1960f,0.4250f,"mint"),
            ("TX_CAL_OK","TX CAL OK",-0.1290f,0.4250f,"mint"),
            ("ROUTE_CMD","ROUTE CMD",-0.0620f,0.4250f,"mint"),
            ("ROUTE_IN","ROUTE IN",0.0050f,0.4250f,"mint"),
            ("ROUTE_OUT","ROUTE OUT",0.0720f,0.4250f,"mint"),
            ("TX_ROUTES","TX ROUTES",0.1390f,0.4250f,"mint"),
            ("TX_TELEM","TX TELEM",0.2060f,0.4250f,"mint"),
            ("SCADA_TX","SCADA TX",0.2730f,0.4250f,"mint"),
            ("BIAS_OUT","BIAS OUT",0.3400f,0.4250f,"mint"),
            ("SCADA_BIAS","SCADA BIAS",0.4070f,0.4250f,"mint"),
            ("RING_CMD","RING CMD",0.4740f,0.4250f,"mint"),
            ("RING_ENABLE","RING ENABLE",0.5410f,0.4250f,"mint"),
            ("HEAT_DAC","HEAT DAC",0.6080f,0.4250f,"mint"),
            ("RING_HEAT","RING HEAT",0.6750f,0.4250f,"mint"),
            ("TEMP_ARRAY","TEMP ARRAY",0.7420f,0.4250f,"mint"),
            ("RING_TEMP","RING TEMP",0.8090f,0.4250f,"mint"),
            ("CROSS_HEAT","CROSS HEAT",0.8760f,0.4250f,"mint"),
            ("CROSS_IN","CROSS IN",0.9430f,0.4250f,"mint"),
            ("A_HEAT","A HEAT",1.0100f,0.4250f,"mint"),
            ("A_DAC","A DAC",-1.0000f,-0.3550f,"mint"),
            ("A_TEMP","A TEMP",-0.9390f,-0.3550f,"mint"),
            ("A_TEMPERATURE","A TEMPERATURE",-0.8780f,-0.3550f,"mint"),
            ("B_HEAT","B HEAT",-0.8170f,-0.3550f,"mint"),
            ("B_DAC","B DAC",-0.7560f,-0.3550f,"mint"),
            ("B_TEMP","B TEMP",-0.6950f,-0.3550f,"mint"),
            ("B_TEMPERATURE","B TEMPERATURE",-0.6340f,-0.3550f,"mint"),
            ("C_HEAT","C HEAT",-0.5730f,-0.3550f,"mint"),
            ("C_DAC","C DAC",-0.5120f,-0.3550f,"mint"),
            ("C_TEMP","C TEMP",-0.4510f,-0.3550f,"mint"),
            ("C_TEMPERATURE","C TEMPERATURE",-0.3900f,-0.3550f,"mint"),
            ("D_HEAT","D HEAT",-0.3290f,-0.3550f,"mint"),
            ("D_DAC","D DAC",-0.2680f,-0.3550f,"mint"),
            ("D_TEMP","D TEMP",-0.2070f,-0.3550f,"mint"),
            ("D_TEMPERATURE","D TEMPERATURE",-0.1460f,-0.3550f,"mint"),
            ("A_LOCK","A LOCK",-0.0850f,-0.3550f,"mint"),
            ("A_RESONANCE","A RESONANCE",-0.0240f,-0.3550f,"mint"),
            ("B_LOCK","B LOCK",0.0370f,-0.3550f,"mint"),
            ("B_RESONANCE","B RESONANCE",0.0980f,-0.3550f,"mint"),
            ("C_LOCK","C LOCK",0.1590f,-0.3550f,"mint"),
            ("C_RESONANCE","C RESONANCE",0.2200f,-0.3550f,"mint"),
            ("D_LOCK","D LOCK",0.2810f,-0.3550f,"mint"),
            ("D_RESONANCE","D RESONANCE",0.3420f,-0.3550f,"mint"),
            ("CROSS_CMD","CROSS CMD",0.4030f,-0.3550f,"mint"),
            ("CROSS_COMP","CROSS COMP",0.4640f,-0.3550f,"mint"),
            ("DEMOD_OUT","DEMOD OUT",0.5250f,-0.3550f,"mint"),
            ("DEMOD_LOCK","DEMOD LOCK",0.5860f,-0.3550f,"mint"),
            ("TEMP_TELEM","TEMP TELEM",0.6470f,-0.3550f,"mint"),
            ("SCADA_TEMP","SCADA TEMP",0.7080f,-0.3550f,"mint"),
            ("COOL_CMD","COOL CMD",0.7690f,-0.3550f,"mint"),
            ("COOLING_SET","COOLING SET",0.8300f,-0.3550f,"mint"),
            ("DEMUX_CMD","DEMUX CMD",0.8910f,-0.3550f,"mint"),
            ("DEMUX_CONFIG","DEMUX CONFIG",0.9520f,-0.3550f,"mint"),
            ("DEMUX_OUT","DEMUX OUT",1.0130f,-0.3550f,"mint"),
            ("DEMUX_OK","DEMUX OK",-1.0000f,-0.5250f,"mint"),
            ("PD_CMD","PD CMD",-0.9390f,-0.5250f,"mint"),
            ("PD_ENABLE","PD ENABLE",-0.8780f,-0.5250f,"mint"),
            ("PD_OUT","PD OUT",-0.8170f,-0.5250f,"mint"),
            ("PD_CURRENT","PD CURRENT",-0.7560f,-0.5250f,"mint"),
            ("TIA_OUT","TIA OUT",-0.6950f,-0.5250f,"mint"),
            ("RX_ANALOG","RX ANALOG",-0.6340f,-0.5250f,"mint"),
            ("CDR_OUT","CDR OUT",-0.5730f,-0.5250f,"mint"),
            ("CDR_LOCK","CDR LOCK",-0.5120f,-0.5250f,"mint"),
            ("RX_TAP","RX TAP",-0.4510f,-0.5250f,"mint"),
            ("RX_METER","RX METER",-0.3900f,-0.5250f,"mint"),
            ("SCADA_RX_TAP","SCADA RX TAP",-0.3290f,-0.5250f,"mint"),
            ("SPEC_OUT","SPEC OUT",-0.2680f,-0.5250f,"mint"),
            ("SPEC_IN","SPEC IN",-0.2070f,-0.5250f,"mint"),
            ("BUDGET_OUT","BUDGET OUT",-0.1460f,-0.5250f,"mint"),
            ("BUDGET_IN","BUDGET IN",-0.0850f,-0.5250f,"mint"),
            ("DROP_OUT","DROP OUT",-0.0240f,-0.5250f,"mint"),
            ("DROP_OK","DROP OK",0.0370f,-0.5250f,"mint"),
            ("RX_OPTICAL_POWER_SIGNAL_OUT","RX POWER OUT",0.0980f,-0.5250f,"mint"),
            ("LINK_SUPERVISOR_RX_POWER_IN","LINK RX IN",0.1590f,-0.5250f,"mint"),
            ("METER_HEALTH","METER HEALTH",0.2200f,-0.5250f,"mint"),
            ("TIA_GAIN_REQ","TIA GAIN REQ",0.2810f,-0.5250f,"mint"),
            ("TIA_ACTUAL","TIA ACTUAL",0.3420f,-0.5250f,"mint"),
            ("PD_READY","PD READY",0.4030f,-0.5250f,"mint"),
            ("SCADA_SPEC","SCADA SPEC",0.4640f,-0.5250f,"mint"),
            ("SCADA_LOSS","SCADA LOSS",0.5250f,-0.5250f,"mint"),
            ("BER_ENABLE","BER ENABLE",0.5860f,-0.5250f,"mint"),
            ("PRBS_EXPECT","PRBS EXPECT",0.6470f,-0.5250f,"mint"),
            ("TEST_BITS","TEST BITS",0.7080f,-0.5250f,"mint"),
            ("BER_RESULT","BER RESULT",0.7690f,-0.5250f,"mint"),
            ("EYE_RESULT","EYE RESULT",0.8300f,-0.5250f,"mint"),
            ("PAM4_QUALITY","PAM4 QUALITY",0.8910f,-0.5250f,"mint"),
            ("FEC_STATUS","FEC STATUS",0.9520f,-0.5250f,"mint"),
            ("RAW_BITS","RAW BITS",1.0130f,-0.5250f,"mint"),
            ("FEC_BITS","FEC BITS",-1.0000f,-0.7000f,"mint"),
            ("CDR_REQ","CDR REQ",-0.9410f,-0.7000f,"mint"),
            ("CLOCK_OUT","CLOCK OUT",-0.8820f,-0.7000f,"mint"),
            ("ROUTE_PASS","ROUTE PASS",-0.8230f,-0.7000f,"mint"),
            ("MEMORY_RESP","MEMORY RESP",-0.7640f,-0.7000f,"mint"),
            ("LOGIC_STATUS","LOGIC STATUS",-0.7050f,-0.7000f,"mint"),
            ("IO_STATUS","IO STATUS",-0.6460f,-0.7000f,"mint"),
            ("SENSOR_TELEM","SENSOR TELEM",-0.5870f,-0.7000f,"mint"),
            ("FIBER_LOSS","FIBER LOSS",-0.5280f,-0.7000f,"mint"),
            ("FAULT_STATE","FAULT STATE",-0.4690f,-0.7000f,"amber"),
            ("INJECT_ENABLE","INJECT ENABLE",-0.4100f,-0.7000f,"mint"),
            ("WAVEGUIDE_SCOPE","WAVEGUIDE SCOPE",-0.3510f,-0.7000f,"mint"),
            ("COUPON_OK","COUPON OK",-0.2920f,-0.7000f,"mint"),
            ("ESD_STATUS","ESD STATUS",-0.2330f,-0.7000f,"mint"),
            ("DOCK_SEATED","DOCK SEATED",-0.1740f,-0.7000f,"mint"),
            ("EDGE_ALIGN","EDGE ALIGN",-0.1150f,-0.7000f,"mint"),
            ("FIBER_ID","FIBER ID",-0.0560f,-0.7000f,"mint"),
            ("CROSSTALK_MAP","CROSSTALK MAP",0.0030f,-0.7000f,"mint"),
            ("WAVEGUIDE_MAP","WAVEGUIDE MAP",0.0620f,-0.7000f,"mint"),
            ("WDM_HOLOGRAM","WDM HOLOGRAM",0.1210f,-0.7000f,"mint"),
            ("ROUTE_HOLOGRAM","ROUTE HOLOGRAM",0.1800f,-0.7000f,"mint"),
            ("DIAGNOSTICS","DIAGNOSTICS",0.2390f,-0.7000f,"mint"),
            ("ESTOP_NC","ESTOP NC",0.2980f,-0.7000f,"amber"),
            ("SAFE_STOP_IN","SAFE STOP IN",0.3570f,-0.7000f,"amber"),
            ("OVERRIDE_SAFE","OVERRIDE SAFE",0.4160f,-0.7000f,"amber"),
            ("OPTICAL_PERMIT","OPTICAL PERMIT",0.4750f,-0.7000f,"amber"),
            ("PLC_ALLOW","PLC ALLOW",0.5340f,-0.7000f,"mint"),
            ("SCADA_QUAL","SCADA QUAL",0.5930f,-0.7000f,"mint"),
            ("OPERATOR_CMD","OPERATOR CMD",0.6520f,-0.7000f,"mint"),
            ("RX_LOSS_TRIP","RX LOSS TRIP",0.7110f,-0.7000f,"amber"),
            ("LASER_OFF","LASER OFF",0.7700f,-0.7000f,"mint"),
            ("FAIL_CLOSED","FAIL CLOSED",0.8290f,-0.7000f,"mint"),
            ("W078_VALID","W078 VALID",0.8880f,-0.7000f,"mint"),
            ("OBJECTIVE_EVENT","OBJECTIVE EVENT",0.9470f,-0.7000f,"mint"),
            ("SAFETY_ALARM","SAFETY ALARM",1.0060f,-0.7000f,"amber"),
        };

        public static IEnumerable<string> TerminalIds => Sockets.Select(socket => socket.id);

        private static readonly Dictionary<string, string> Palette = new()
        {
            {"red","EE485C"},{"blue","1711E2"},{"amber","FFA82B"},{"violet","A282FF"},{"plasma","A45DFF"},
            {"mint","34F6C5"},{"cyan","23DAFD"},{"void","07111C"},{"white","D4F5FF"},{"glass","498BA8"}
        };

        [MenuItem("Tools/Project Spark/Levels/Create Level 80")]
        public static void Install()
        {
            SparkLevel80Builder.Create();
            var root = AssetDatabase.LoadAssetAtPath<GameObject>(Prefab);
            if (root == null || root.GetComponent<SparkLevel80Process>().controllerDisplay == null) Fit();
            Register();
            ValidateAssets();
        }

        [MenuItem("Tools/Project Spark/Level 80/Validate Level Assets")]
        public static void ValidateAssets()
        {
            var root=AssetDatabase.LoadAssetAtPath<GameObject>(Prefab);
            var control=root != null ? root.GetComponent<SparkLevel80Process>() : null;
            var bindings=root != null ? root.GetComponent<SparkLevelSceneBindings>() : null;
            if (control==null || bindings==null || control.controllerDisplay==null || control.qualityDisplay==null || control.rig==null || control.rig.pivots.Length!=50 ||
                control.pins.Length!=194 || control.pins.Distinct().Count()!=194 || 
                !TerminalIds.SequenceEqual(control.pins.Select(pin=>pin.name)) ||
                control.pins.Any(pin=>!bindings.TryGetTerminal(pin.name,out var bound) || bound!=pin) ||
                SparkLevel80Process.Gates.Any(id=>!bindings.IsComponentIdDeclared(id)))
                throw new InvalidOperationException("Level 80 wiring, motion or binding references are incomplete.");
            Debug.Log("[LEVEL 80] Validated 194 distinct terminals, 126 player wires and 6 evaluator gates.",root);
        }

        private static void Register()
        {
            var scene = SceneManager.GetSceneByPath(ScenePath);
            bool opened = !scene.isLoaded;
            if (opened) scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
            bool dirty = scene.isDirty;
            var objects = scene.GetRootGameObjects();
            T Find<T>() where T : Component => objects.SelectMany(item => item.GetComponentsInChildren<T>(true)).Single();
            var level = AssetDatabase.LoadAssetAtPath<SparkLevelDefinition>(Folder + "Level_80.asset");
            var drawing = AssetDatabase.LoadAssetAtPath<SparkLevelDrawingAsset>(Folder + "Level_80_Drawing.asset");
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
            else if (dirty) Debug.Log("MainMenu had unsaved edits. Save the scene to persist Level 80's campaign and teaching entries.");
        }

        private static void Append(Object target, string field, Object value)
        {
            if (value == null) throw new InvalidOperationException("Missing Level 80 asset for " + field);
            var data = new SerializedObject(target);
            var list = data.FindProperty(field);
            for (int i = 0; i < list.arraySize; i++)
                if (list.GetArrayElementAtIndex(i).objectReferenceValue == value) return;
            list.arraySize++;
            list.GetArrayElementAtIndex(list.arraySize - 1).objectReferenceValue = value;
            data.ApplyModifiedProperties();
        }

        [MenuItem("Tools/Project Spark/Level 80/Fit Breadboard Assembly")]
        public static void Fit()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Exit Play Mode before fitting Level 80.");
            var models = Models.Select((name, index) =>
            {
                string path = Folder + $"Models/L80_{index:00}_{name}_Fitted.fbx";
                return AssetDatabase.LoadAssetAtPath<GameObject>(path) ??
                    throw new InvalidOperationException("Missing fitted kit module " + path);
            }).ToArray();
            if (AssetDatabase.LoadAssetAtPath<GameObject>(Prefab) == null)
                throw new InvalidOperationException("The Level 80 prefab is missing.");

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
                    throw new InvalidOperationException("Unity could not save the Level 80 layout.");
                AssetDatabase.SaveAssets();
                Debug.Log("Level 80 fitted to the MainMenu breadboard with 194 seated terminals.");
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
            var bench = root.transform.Find("Photonic Workbench");
            var bindings = root.GetComponent<SparkLevelSceneBindings>();
            var loop = root.GetComponent<SparkLevel80Process>();
            if (bench == null || bindings == null || loop == null)
                throw new InvalidOperationException("Level 80 is missing its workbench, controls, or scene bindings.");

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

            foreach (string name in new[] {"L80_HD_VISUAL_ART", "Breadboard Labels", "Motion pivots"})
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
            var assembly = Child(bench, "L80_HD_VISUAL_ART", new Vector3(0,floor,-.010f));
            foreach (var model in models)
            {
                var instance = Object.Instantiate(model, assembly, false).transform;
                var markers = instance.GetComponentsInChildren<Transform>(true)
                    .Where(item => item.name.StartsWith("L80_Board",StringComparison.Ordinal))
                    .ToDictionary(item => item.name);
                Transform Marker(string name) => markers["L80_Board" + name];
                Vector3 Axis(string name) => assembly.InverseTransformVector(Marker(name).position - Marker("Center").position);
                instance.localRotation = Quaternion.Inverse(Quaternion.LookRotation(-Axis("Front"), Axis("Up"))) * instance.localRotation;
                instance.localScale /= Axis("Right").magnitude;
                instance.localPosition -= assembly.InverseTransformPoint(Marker("Center").position);
                if (Axis("Right").x < .99f)
                    throw new InvalidOperationException("Unexpected axes on fitted model " + model.name);
            }

            assembly.localScale = Vector3.one * .53f;
            if (!AssetDatabase.IsValidFolder(Materials)) AssetDatabase.CreateFolder(Folder + "Models", "Materials");
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) throw new InvalidOperationException("The project URP Lit shader is missing.");
            ConfigureTextures();
            foreach (var mesh in assembly.GetComponentsInChildren<MeshRenderer>())
            {
                string key = mesh.name.Substring(mesh.name.LastIndexOf("__", StringComparison.Ordinal) + 2);
                if(key=="tint1D303F" && mesh.name.StartsWith("L80_TransparentPhotonicCoreEnclosure_",StringComparison.Ordinal)) key="glass";
                string surface=key switch
                {
                    "glass"=>"OpticalGlass",
                    "tintDC803E"=>"CopperBus",
                    "tintE9BC56"=>"GoldContact",
                    "tint47657C" or "tintC2D0D6" or "tint97B3C4"=>"BrushedSteel",
                    "tint0C1620" when mesh.name.Contains("Fiber")=>"Elastomer",
                    "tint0C1620" or "tintE0DED3"=>"BlackCeramic",
                    "tint0D1D2F" when mesh.name.StartsWith("L80_IndustrialPhotonicDataAtriumBase_",StringComparison.Ordinal)=>"CleanroomFloor",
                    "tint0D1D2F" when mesh.name.Contains("Interposer")=>"SiliconInterposer",
                    "tint0D1D2F" or "tint1D303F"=>"AnodizedPanel",
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
                var oldControls = root.transform.Find("Level 80 Controls");
                if (oldControls != null) Object.DestroyImmediate(oldControls.gameObject);
                SparkLevel80Builder.BuildControls(root.transform,loop,font);
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
                terminalData.FindProperty("maxConnections").intValue = SparkLevel80Process.Wires.Count(wire => wire.a == index || wire.b == index);
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






                ("LASER / TX",-.865f,.147f),("RX / DATA",.862f,.147f),
                ("MICRORINGS / HEATERS",-.86f,-.235f),("SUPERVISOR / SAFETY",.86f,-.235f)
            };
            foreach(var title in titles)
                Label(labels,font,title.title,title.title,new Vector3(title.x,floor+.058f,title.z),.0165f);
            Label(labels,font,"Carrier labels","A LOGIC  /  B MEMORY  /  C I/O  /  D SENSOR",new Vector3(0,floor+.068f,-.252f),.013f);
            Label(labels,font,"Optic path","WDM MUX   /   DEMUX",new Vector3(0,floor+.15f,.13f),.012f);
            Transform Pivot(string module,string key)
            {
                var name="fitted_"+module+"_"+key;
                return assembly.GetComponentsInChildren<Transform>(true).Single(item=>item.name==name);
            }
            loop.rig.art=assembly;
            loop.rig.pivots=new[]{
                Pivot("MultiWavelengthLaserSourceRack","ANIM_LASER_SOURCE_DRAWER"),
                Pivot("LaserCombStabilizationModule","ANIM_LASER_COMB_LOCK"),
                Pivot("LaserTemperatureControlMount","ANIM_LASER_TEMPERATURE_STAGE"),
                Pivot("OpticalSafetyShutter","ANIM_SAFETY_SHUTTER"),
                Pivot("OpticalIsolatorModule","ANIM_OPTICAL_ISOLATOR"),
                Pivot("CalibratedOpticalTapSplitter","ANIM_TAP_SPLITTER"),
                Pivot("PolarizationControlModule","ANIM_POLARIZATION_CONTROLLER"),
                Pivot("VariableOpticalAttenuator","ANIM_OPTICAL_ATTENUATOR"),
                Pivot("SpectralReferenceEtalon","ANIM_SPECTRAL_REFERENCE_SCAN"),
                Pivot("LaunchOpticalPowerMeter","ANIM_LAUNCH_POWER_PROBE"),
                Pivot("TransparentPhotonicCoreEnclosure","ANIM_PHOTONIC_CORE_LID"),
                Pivot("FiberRibbonDockingStation","ANIM_FIBER_DOCK"),
                Pivot("PhotonicEdgeCouplerArray","ANIM_COUPLER_ARM"),
                Pivot("FourChannelWDMMultiplexer","ANIM_WDM_MULTIPLEXER"),
                Pivot("MicroringResonatorChannelA","ANIM_RING_A_RESONANCE"),
                Pivot("MicroringResonatorChannelB","ANIM_RING_B_RESONANCE"),
                Pivot("MicroringResonatorChannelC","ANIM_RING_C_RESONANCE"),
                Pivot("MicroringResonatorChannelD","ANIM_RING_D_RESONANCE"),
                Pivot("FourZoneMicroringHeaterBank","ANIM_FOUR_ZONE_HEATER_BANK"),
                Pivot("GiantWavelengthRoutingHologram","ANIM_WAVELENGTH_ROUTING_HOLOGRAM"),
                Pivot("FourChannelWDMDemultiplexer","ANIM_WDM_DEMULTIPLEXER"),
                Pivot("FourChannelOpticalDropPortPanel","ANIM_DROP_PORT_PANEL"),
                Pivot("PhotodiodeReceiverArray","ANIM_PHOTODIODE_ARRAY"),
                Pivot("ReceiverTransimpedanceAmplifierBank","ANIM_TRANSIMPEDANCE_DISPLAY"),
                Pivot("ClockAndDataRecoveryModule","ANIM_CLOCK_RECOVERY_DISPLAY"),
                Pivot("ReceiverOpticalPowerMonitor","ANIM_RX_POWER_MONITOR"),
                Pivot("HighResolutionOpticalSpectrumAnalyzer","ANIM_SPECTRUM_ANALYZER_SCAN"),
                Pivot("HighSpeedEyeDiagramAnalyzer","ANIM_EYE_DIAGRAM_DISPLAY"),
                Pivot("OpticalLinkBitErrorRateTester","ANIM_BER_TEST_DISPLAY"),
                Pivot("PAM4WaveformSamplingInstrument","ANIM_PAM4_SIGNAL_DISPLAY"),
                Pivot("PseudorandomBitPatternGenerator","ANIM_BIT_PATTERN_GENERATOR"),
                Pivot("ForwardErrorCorrectionDecoder","ANIM_FEC_DECODER_DISPLAY"),
                Pivot("ChipletOpticalTestRouter","ANIM_CHIPLET_DATA_ROUTER"),
                Pivot("LogicOpticalTransmitterInterface","ANIM_LOGIC_DATA_STREAM"),
                Pivot("MemoryOpticalLinkDiagnosticModule","ANIM_MEMORY_DATA_STREAM"),
                Pivot("IOOpticalOutputInterface","ANIM_IO_DATA_STREAM"),
                Pivot("SensorOpticalTelemetryInterface","ANIM_SENSOR_DATA_STREAM"),
                Pivot("OpticalFiberMicrobendMonitor","ANIM_FIBER_BEND_SENSOR"),
                Pivot("ControlledOpticalFaultInjectionModule","ANIM_FAULT_INJECTION_ARM"),
                Pivot("PhotonicWaveguideDefectMicroscope","ANIM_WAVEGUIDE_MICROSCOPE"),
                Pivot("PhotonicCoolingAirManifold","ANIM_COOLING_FAN_BANK"),
                Pivot("HolographicThermalResonanceDisplay","ANIM_THERMAL_CROSSTALK_MAP"),
                Pivot("DemultiplexerResonanceMonitor","ANIM_RESONANCE_LOCK_DISPLAY"),
                Pivot("OpticalLinkPowerBudgetMeter","ANIM_OPTICAL_POWER_BUDGET"),
                Pivot("EmergencyOpticalShutterStation","ANIM_SAFETY_EMERGENCY_SHUTTER"),
                Pivot("StrayLightSafetyMonitor","ANIM_STRAY_LIGHT_MONITOR"),
                Pivot("OpticalCalibrationReferenceCoupon","ANIM_REFERENCE_COUPON_STAGE"),
                Pivot("ESDGuardedPhotonicTestFixture","ANIM_TEST_FIXTURE_LID"),
                Pivot("HolographicOpticalDataSCADA","ANIM_PHOTONIC_MAIN_SCADA"),
                Pivot("GiantWavelengthRoutingHologram","ANIM_LEVEL80_CERTIFICATION_HOLOGRAM"),
            };
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
            Lamp("Optical work light",new Vector3(-.38f,floor+.58f,-.14f),new Color(.91f,.94f,1),1.35f,1.2f);
            Lamp("Metrology fill",new Vector3(.54f,floor+.34f,.16f),new Color(.49f,.80f,1),.64f,.76f);
            loop.rig.BindEffects(
                Lamp("Protected source light",new Vector3(-.37f,floor+.20f,-.03f),new Color(.29f,.75f,1),1,.45f),
                Lamp("WDM activity light",new Vector3(0,floor+.20f,-.03f),new Color(.48f,.39f,1),1,.46f),
                Lamp("Receiver lock light",new Vector3(.28f,floor+.2f,-.03f),new Color(.58f,.37f,1),1,.45f),
                Lamp("Isolation alarm",new Vector3(.56f,floor+.24f,.03f),new Color(1,.18f,.12f),1.25f,.55f));
            if(loop.plot.transform.Find("Channel A") == null)
            {
                var oldPanel = root.transform.Find("Level 80 Controls");
                if (oldPanel != null) Object.DestroyImmediate(oldPanel.gameObject);
                SparkLevel80Builder.BuildControls(root.transform,loop,font);
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
            loop.controllerDisplay=Readout("RX readout",new Vector3(-.815f,floor+.075f,.215f),new Color(.46f,.94f,1f));
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
            string materialName=surface==null?"L80 "+key:"L80 "+key+" "+surface;
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
                    throw new InvalidOperationException("Missing Level 80 surface maps for "+surface);
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















