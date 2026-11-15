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
    public static class SparkLevel79ModelInstaller
    {
        public const string Folder = "Assets/My_Assets/_Project_Spark/Prefabs/Levels/Level_79_Layout/";
        public const string Prefab = Folder + "Level_79_Layout.prefab";
        public const string ScenePath = "Assets/My_Assets/_Project_Spark/Scenes/MainMenu/MainMenu.unity";
        private static readonly string[] Models = {
            "Industrial3DHybridBondingBase","TransparentVerticalBondingTower","BaseWaferCarrier","BondReadyInterconnectWafer","CopperDielectricLandingPadMap","PrecisionChipletTray","LogicChiplet","MemoryChiplet","IOChiplet","SensorChiplet","KnownGoodDieCertificateScanner","PrecisionWaferLoadLock","DieTransferAirCurtain","HighPrecisionWaferPreAligner","SixAxisHybridBondingGantry","GantryXLinearRail","GantryYCrossbeam","GantryZPrecisionElevator","HexapodFineAlignmentStage","NanoXPositioningActuator","NanoYPositioningActuator","ThetaMicroRotationStage","TipTiltCompensationPlatform","UpperDieElectrostaticChuck","ChuckReleaseConditionMonitor","BaseWaferVacuumChuck","BondHeadMicroforceArray","FiducialOpticalCameraA","FiducialOpticalCameraB","ThroughSiliconInfraredCamera","LaserDisplacementInterferometer","MoireOverlayProjectionSystem","WaferBowInterferometryScanner","DarkfieldParticleInspectionSystem","AtomicScaleSurfaceFlatnessProfiler","SurfaceEnergyInspectionProbe","DrySurfaceCleaningModule","LowDamageSurfaceActivationModule","TransparentBondChamberWindow","ControlledContactInitiationHead","BondFrontOpticalImager","HolographicBondWaveCrossSection","InterfaceGapSensorArray","BondThermalConsolidationChamber","MultiZoneBondHeater","BondTemperatureSensor","DifferentialExpansionAnalyzer","BondInterfaceStressMonitor","ScanningAcousticVoidInspector","InfraredBondVoidTomography","BondSeamInspectionCamera","BondAdhesionWitnessCouponTester","CrossDieKelvinResistanceProbe","VerticalDaisyChainTester","ChipletInterconnectRoutingTester","HighSpeedBERJitterAnalyzer","ThermalCycleQualificationBay","StackWarpageMetrologyScanner","ChipletRejectQuarantineBin","ESDGuardedTransferStation","HybridBondingPLCRack","PrecisionAnalogIOModule","CriticalMotionSafetyStation","Holographic3DStackSCADA","Regulated5VControlSupply","Completed3DChipStackHologram"
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
            ("VISION_5V","VISION 5V",-0.7140f,0.6800f,"red"),
            ("VISION_GND","VISION GND",-0.6425f,0.6800f,"blue"),
            ("MOTION_5V","MOTION 5V",-0.5710f,0.6800f,"red"),
            ("MOTION_GND","MOTION GND",-0.4995f,0.6800f,"blue"),
            ("LASER_5V","LASER 5V",-0.4280f,0.6800f,"red"),
            ("LASER_GND","LASER GND",-0.3565f,0.6800f,"blue"),
            ("SAFE_5V","SAFE 5V",-0.2850f,0.6800f,"red"),
            ("SAFE_GND","SAFE GND",-0.2135f,0.6800f,"blue"),
            ("MOTION_ENABLE","MOTION ENABLE",-0.1420f,0.6800f,"mint"),
            ("GANTRY_ENABLE","GANTRY ENABLE",-0.0705f,0.6800f,"mint"),
            ("X_CMD","X CMD",0.0010f,0.6800f,"mint"),
            ("X_SERVO","X SERVO",0.0725f,0.6800f,"mint"),
            ("X_ENC","X ENC",0.1440f,0.6800f,"mint"),
            ("X_FEEDBACK","X FEEDBACK",0.2155f,0.6800f,"mint"),
            ("Y_CMD","Y CMD",0.2870f,0.6800f,"mint"),
            ("Y_SERVO","Y SERVO",0.3585f,0.6800f,"mint"),
            ("Y_ENC","Y ENC",0.4300f,0.6800f,"mint"),
            ("Y_FEEDBACK","Y FEEDBACK",0.5015f,0.6800f,"mint"),
            ("Z_CMD","Z CMD",0.5730f,0.6800f,"mint"),
            ("Z_SERVO","Z SERVO",0.6445f,0.6800f,"mint"),
            ("Z_ENC","Z ENC",0.7160f,0.6800f,"mint"),
            ("Z_FEEDBACK","Z FEEDBACK",0.7875f,0.6800f,"mint"),
            ("POSE_CMD","POSE CMD",0.8590f,0.6800f,"mint"),
            ("HEXAPOD_DRIVE","HEXAPOD DRIVE",0.9305f,0.6800f,"mint"),
            ("HEX_READY","HEX READY",1.0020f,0.6800f,"mint"),
            ("POSE_READY","POSE READY",-1.0000f,0.5450f,"mint"),
            ("NANO_X_CMD","NANO X CMD",-0.9285f,0.5450f,"mint"),
            ("NANO_X_DRIVE","NANO X DRIVE",-0.8570f,0.5450f,"mint"),
            ("NANO_X_OUT","NANO X OUT",-0.7855f,0.5450f,"mint"),
            ("FINE_X","FINE X",-0.7140f,0.5450f,"mint"),
            ("NANO_Y_CMD","NANO Y CMD",-0.6425f,0.5450f,"mint"),
            ("NANO_Y_DRIVE","NANO Y DRIVE",-0.5710f,0.5450f,"mint"),
            ("NANO_Y_OUT","NANO Y OUT",-0.4995f,0.5450f,"mint"),
            ("FINE_Y","FINE Y",-0.4280f,0.5450f,"mint"),
            ("THETA_CMD","THETA CMD",-0.3565f,0.5450f,"mint"),
            ("THETA_DRIVE","THETA DRIVE",-0.2850f,0.5450f,"mint"),
            ("THETA_OUT","THETA OUT",-0.2135f,0.5450f,"mint"),
            ("THETA_IN","THETA IN",-0.1420f,0.5450f,"mint"),
            ("TILT_CMD","TILT CMD",-0.0705f,0.5450f,"mint"),
            ("TILT_DRIVE","TILT DRIVE",0.0010f,0.5450f,"mint"),
            ("TILT_OUT","TILT OUT",0.0725f,0.5450f,"mint"),
            ("TILT_IN","TILT IN",0.1440f,0.5450f,"mint"),
            ("POSE_SAFE","POSE SAFE",0.2155f,0.5450f,"amber"),
            ("MOTION_INTERLOCK","MOTION INTERLOCK",0.2870f,0.5450f,"mint"),
            ("CAM_A_OUT","CAM A OUT",0.3585f,0.5450f,"mint"),
            ("VISION_A","VISION A",0.4300f,0.5450f,"mint"),
            ("CAM_B_OUT","CAM B OUT",0.5015f,0.5450f,"mint"),
            ("VISION_B","VISION B",0.5730f,0.5450f,"mint"),
            ("IR_REG_OUT","IR REG OUT",0.6445f,0.5450f,"mint"),
            ("IR_REG_IN","IR REG IN",0.7160f,0.5450f,"mint"),
            ("MOIRE_OUT","MOIRE OUT",0.7875f,0.5450f,"mint"),
            ("OVERLAY_DIAG","OVERLAY DIAG",0.8590f,0.5450f,"mint"),
            ("WARP_OUT","WARP OUT",0.9305f,0.5450f,"mint"),
            ("WARP_IN","WARP IN",1.0020f,0.5450f,"mint"),
            ("CAM_A_RES","CAM A RES",-1.0000f,0.4250f,"mint"),
            ("ALIGN_A","ALIGN A",-0.9285f,0.4250f,"mint"),
            ("CAM_B_RES","CAM B RES",-0.8570f,0.4250f,"mint"),
            ("ALIGN_B","ALIGN B",-0.7855f,0.4250f,"mint"),
            ("VISION_AGREE","VISION AGREE",-0.7140f,0.4250f,"mint"),
            ("ALIGN_VALID","ALIGN VALID",-0.6425f,0.4250f,"mint"),
            ("LASER_INTERFEROMETER_POSITION_OUT","LASER POS OUT",-0.5710f,0.4250f,"mint"),
            ("HEXAPOD_FINE_POSITION_IN","HEX FINE IN",-0.4995f,0.4250f,"mint"),
            ("LASER_TELEM","LASER TELEM",-0.4280f,0.4250f,"mint"),
            ("LASER_CMD","LASER CMD",-0.3565f,0.4250f,"mint"),
            ("LASER_ENABLE","LASER ENABLE",-0.2850f,0.4250f,"mint"),
            ("LASER_HEALTH","LASER HEALTH",-0.2135f,0.4250f,"mint"),
            ("SAFETY_LASER","SAFETY LASER",-0.1420f,0.4250f,"amber"),
            ("LASER_CAL_OK","LASER CAL OK",-0.0705f,0.4250f,"mint"),
            ("POSE_QUAL","POSE QUAL",0.0010f,0.4250f,"mint"),
            ("BOND_POSE","BOND POSE",0.0725f,0.4250f,"mint"),
            ("OVERLAY_VIEW","OVERLAY VIEW",0.1440f,0.4250f,"mint"),
            ("IR_CERT","IR CERT",0.2155f,0.4250f,"mint"),
            ("REG_CERT","REG CERT",0.2870f,0.4250f,"mint"),
            ("WARP_COMP","WARP COMP",0.3585f,0.4250f,"mint"),
            ("WARP_AUTH","WARP AUTH",0.4300f,0.4250f,"mint"),
            ("OPTICS_VALID","OPTICS VALID",0.5015f,0.4250f,"mint"),
            ("MOTION_VISION","MOTION VISION",0.5730f,0.4250f,"mint"),
            ("GRIP_CMD","GRIP CMD",0.6445f,0.4250f,"mint"),
            ("UPPER_GRIP","UPPER GRIP",0.7160f,0.4250f,"mint"),
            ("RELEASE_OUT","RELEASE OUT",0.7875f,0.4250f,"mint"),
            ("CHUCK_RELEASE_MON","CHUCK RELEASE MON",0.8590f,0.4250f,"mint"),
            ("CHUCK_READY","CHUCK READY",0.9305f,0.4250f,"mint"),
            ("RELEASE_OK","RELEASE OK",1.0020f,0.4250f,"mint"),
            ("LOWER_CMD","LOWER CMD",-1.0000f,-0.3550f,"mint"),
            ("BASE_GRIP","BASE GRIP",-0.9380f,-0.3550f,"mint"),
            ("BASE_HELD","BASE HELD",-0.8760f,-0.3550f,"mint"),
            ("BASE_HOLD_OK","BASE HOLD OK",-0.8140f,-0.3550f,"mint"),
            ("FORCE_CMD","FORCE CMD",-0.7520f,-0.3550f,"mint"),
            ("FORCE_ENABLE","FORCE ENABLE",-0.6900f,-0.3550f,"mint"),
            ("FORCE_ARRAY","FORCE ARRAY",-0.6280f,-0.3550f,"mint"),
            ("FORCE_IN","FORCE IN",-0.5660f,-0.3550f,"mint"),
            ("CONTACT_PERMIT","CONTACT PERMIT",-0.5040f,-0.3550f,"amber"),
            ("CONTACT_ENABLE","CONTACT ENABLE",-0.4420f,-0.3550f,"mint"),
            ("CONTACT_OUT","CONTACT OUT",-0.3800f,-0.3550f,"mint"),
            ("CONTACT_POSITION","CONTACT POSITION",-0.3180f,-0.3550f,"mint"),
            ("GAP_OUT","GAP OUT",-0.2560f,-0.3550f,"mint"),
            ("GAP_IN","GAP IN",-0.1940f,-0.3550f,"mint"),
            ("WAVE_OUT","WAVE OUT",-0.1320f,-0.3550f,"mint"),
            ("WAVE_IN","WAVE IN",-0.0700f,-0.3550f,"mint"),
            ("WAVE_CERT","WAVE CERT",-0.0080f,-0.3550f,"mint"),
            ("FRONT_CERT","FRONT CERT",0.0540f,-0.3550f,"mint"),
            ("CONTACT_CMD","CONTACT CMD",0.1160f,-0.3550f,"mint"),
            ("CONTACT_REQ","CONTACT REQ",0.1780f,-0.3550f,"mint"),
            ("RETRACT_CMD","RETRACT CMD",0.2400f,-0.3550f,"mint"),
            ("RETRACT_OVERRIDE","RETRACT OVERRIDE",0.3020f,-0.3550f,"mint"),
            ("APPROACH_OUT","APPROACH OUT",0.3640f,-0.3550f,"mint"),
            ("SAFE_APPROACH","SAFE APPROACH",0.4260f,-0.3550f,"amber"),
            ("CHUCK_CAL","CHUCK CAL",0.4880f,-0.3550f,"mint"),
            ("RELEASE_CAL","RELEASE CAL",0.5500f,-0.3550f,"mint"),
            ("DIE_CERT","DIE CERT",0.6120f,-0.3550f,"mint"),
            ("KNOWN_GOOD","KNOWN GOOD",0.6740f,-0.3550f,"mint"),
            ("PARTICLE_OUT","PARTICLE OUT",0.7360f,-0.3550f,"mint"),
            ("PARTICLES_IN","PARTICLES IN",0.7980f,-0.3550f,"mint"),
            ("FLATNESS_OUT","FLATNESS OUT",0.8600f,-0.3550f,"mint"),
            ("FLATNESS_IN","FLATNESS IN",0.9220f,-0.3550f,"mint"),
            ("ENERGY_OUT","ENERGY OUT",0.9840f,-0.3550f,"mint"),
            ("SURFACE_ENERGY","SURFACE ENERGY",-1.0000f,-0.5250f,"mint"),
            ("CLEAN_CMD","CLEAN CMD",-0.9390f,-0.5250f,"mint"),
            ("DRY_CLEAN","DRY CLEAN",-0.8780f,-0.5250f,"mint"),
            ("CLEAN_OUT","CLEAN OUT",-0.8170f,-0.5250f,"mint"),
            ("CLEAN_DONE","CLEAN DONE",-0.7560f,-0.5250f,"mint"),
            ("ACTIVATE_CMD","ACTIVATE CMD",-0.6950f,-0.5250f,"mint"),
            ("SURFACE_ACT","SURFACE ACT",-0.6340f,-0.5250f,"mint"),
            ("ACTIVATE_OUT","ACTIVATE OUT",-0.5730f,-0.5250f,"mint"),
            ("ACT_DONE","ACT DONE",-0.5120f,-0.5250f,"mint"),
            ("ESD_GUARD","ESD GUARD",-0.4510f,-0.5250f,"mint"),
            ("ESD_OK","ESD OK",-0.3900f,-0.5250f,"mint"),
            ("REJECT_SENSOR","REJECT SENSOR",-0.3290f,-0.5250f,"mint"),
            ("QUARANTINE_STATE","QUARANTINE STATE",-0.2680f,-0.5250f,"mint"),
            ("REJECT_CMD","REJECT CMD",-0.2070f,-0.5250f,"mint"),
            ("REJECT_BIN","REJECT BIN",-0.1460f,-0.5250f,"mint"),
            ("ALIGNER_OUT","ALIGNER OUT",-0.0850f,-0.5250f,"mint"),
            ("BASE_ALIGNED","BASE ALIGNED",-0.0240f,-0.5250f,"mint"),
            ("SURFACE_CERT","SURFACE CERT",0.0370f,-0.5250f,"mint"),
            ("SURFACE_OK","SURFACE OK",0.0980f,-0.5250f,"mint"),
            ("LOCK_SAFE","LOCK SAFE",0.1590f,-0.5250f,"amber"),
            ("LOADLOCK_OK","LOADLOCK OK",0.2200f,-0.5250f,"mint"),
            ("ANNEAL_ENABLE","ANNEAL ENABLE",0.2810f,-0.5250f,"mint"),
            ("HEATER_SET","HEATER SET",0.3420f,-0.5250f,"mint"),
            ("HEATER_ACTUAL","HEATER ACTUAL",0.4030f,-0.5250f,"mint"),
            ("THERMAL_TEMP","THERMAL TEMP",0.4640f,-0.5250f,"mint"),
            ("EXPANSION_TELEM","EXPANSION TELEM",0.5250f,-0.5250f,"mint"),
            ("BOND_STRESS","BOND STRESS",0.5860f,-0.5250f,"mint"),
            ("THERMAL_SAMPLE","THERMAL SAMPLE",0.6470f,-0.5250f,"mint"),
            ("PROFILE_OK","PROFILE OK",0.7080f,-0.5250f,"mint"),
            ("THERMAL_CYCLE","THERMAL CYCLE",0.7690f,-0.5250f,"mint"),
            ("THERMAL_DONE","THERMAL DONE",0.8300f,-0.5250f,"mint"),
            ("STACK_WARP","STACK WARP",0.8910f,-0.5250f,"mint"),
            ("ANNEAL_SAFE","ANNEAL SAFE",0.9520f,-0.5250f,"amber"),
            ("HEATER_OFF","HEATER OFF",1.0130f,-0.5250f,"mint"),
            ("STRESS_OK","STRESS OK",-1.0000f,-0.7000f,"mint"),
            ("ACOUSTIC_REQ","ACOUSTIC REQ",-0.9390f,-0.7000f,"mint"),
            ("ACOUSTIC_MAP","ACOUSTIC MAP",-0.8780f,-0.7000f,"mint"),
            ("IR_SCAN_REQ","IR SCAN REQ",-0.8170f,-0.7000f,"mint"),
            ("IR_VOID_MAP","IR VOID MAP",-0.7560f,-0.7000f,"mint"),
            ("SEAM_VIEW","SEAM VIEW",-0.6950f,-0.7000f,"mint"),
            ("ADHESION_RESULT","ADHESION RESULT",-0.6340f,-0.7000f,"mint"),
            ("KELVIN_RESULT","KELVIN RESULT",-0.5730f,-0.7000f,"mint"),
            ("DAISY_CHAIN","DAISY CHAIN",-0.5120f,-0.7000f,"mint"),
            ("ROUTING_TEST","ROUTING TEST",-0.4510f,-0.7000f,"mint"),
            ("BER_RESULT","BER RESULT",-0.3900f,-0.7000f,"mint"),
            ("LINK_TEST","LINK TEST",-0.3290f,-0.7000f,"mint"),
            ("SYSTEM_CERT","SYSTEM CERT",-0.2680f,-0.7000f,"mint"),
            ("BOND_TELEM","BOND TELEM",-0.2070f,-0.7000f,"mint"),
            ("LINK_TELEM","LINK TELEM",-0.1460f,-0.7000f,"mint"),
            ("ESTOP_NC","ESTOP NC",-0.0850f,-0.7000f,"amber"),
            ("SAFE_STOP_IN","SAFE STOP IN",-0.0240f,-0.7000f,"amber"),
            ("GANTRY_OFF","GANTRY OFF",0.0370f,-0.7000f,"mint"),
            ("HEX_HOLD","HEX HOLD",0.0980f,-0.7000f,"mint"),
            ("RELEASE_INHIBIT","RELEASE INHIBIT",0.1590f,-0.7000f,"mint"),
            ("APPROACH_INHIBIT","APPROACH INHIBIT",0.2200f,-0.7000f,"mint"),
            ("AXIS_STATUS","AXIS STATUS",0.2810f,-0.7000f,"mint"),
            ("ALIGN_STATUS","ALIGN STATUS",0.3420f,-0.7000f,"mint"),
            ("PLC_STATE","PLC STATE",0.4030f,-0.7000f,"mint"),
            ("OPERATOR_CMD","OPERATOR CMD",0.4640f,-0.7000f,"mint"),
            ("POSE_TELEM","POSE TELEM",0.5250f,-0.7000f,"mint"),
            ("OVERLAY_MAP","OVERLAY MAP",0.5860f,-0.7000f,"mint"),
            ("BOND_MAP","BOND MAP",0.6470f,-0.7000f,"mint"),
            ("CHIP_GRAPH","CHIP GRAPH",0.7080f,-0.7000f,"mint"),
            ("OBJECTIVE_EVENT","OBJECTIVE EVENT",0.7690f,-0.7000f,"mint"),
            ("MOTION_RESET","MOTION RESET",0.8300f,-0.7000f,"mint"),
            ("FAULT_LATCH","FAULT LATCH",0.8910f,-0.7000f,"amber"),
            ("LASER_LOSS","LASER LOSS",0.9520f,-0.7000f,"mint"),
            ("FINAL_CERT","FINAL CERT",1.0130f,-0.7000f,"mint"),
        };

        public static IEnumerable<string> TerminalIds => Sockets.Select(socket => socket.id);

        private static readonly Dictionary<string, string> Palette = new()
        {
            {"red","EE485C"},{"blue","1711E2"},{"amber","FFA82B"},{"violet","A282FF"},{"plasma","A45DFF"},
            {"mint","34F6C5"},{"cyan","23DAFD"},{"void","07111C"},{"white","D4F5FF"},{"glass","498BA8"}
        };

        [MenuItem("Tools/Project Spark/Levels/Create Level 79")]
        public static void Install()
        {
            SparkLevel79Builder.Create();
            var root = AssetDatabase.LoadAssetAtPath<GameObject>(Prefab);
            if (root == null || root.GetComponent<SparkLevel79Process>().controllerDisplay == null) Fit();
            Register();
            ValidateAssets();
        }

        [MenuItem("Tools/Project Spark/Level 79/Validate Level Assets")]
        public static void ValidateAssets()
        {
            var root=AssetDatabase.LoadAssetAtPath<GameObject>(Prefab);
            var control=root != null ? root.GetComponent<SparkLevel79Process>() : null;
            var bindings=root != null ? root.GetComponent<SparkLevelSceneBindings>() : null;
            if (control==null || bindings==null || control.controllerDisplay==null || control.qualityDisplay==null || control.rig==null || control.rig.pivots.Length!=48 ||
                control.pins.Length!=188 || control.pins.Distinct().Count()!=188 || 
                !TerminalIds.SequenceEqual(control.pins.Select(pin=>pin.name)) ||
                control.pins.Any(pin=>!bindings.TryGetTerminal(pin.name,out var bound) || bound!=pin) ||
                SparkLevel79Process.Gates.Any(id=>!bindings.IsComponentIdDeclared(id)))
                throw new InvalidOperationException("Level 79 wiring, motion or binding references are incomplete.");
            Debug.Log("[LEVEL 79] Validated 188 distinct terminals, 122 player wires and 6 evaluator gates.",root);
        }

        private static void Register()
        {
            var scene = SceneManager.GetSceneByPath(ScenePath);
            bool opened = !scene.isLoaded;
            if (opened) scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
            bool dirty = scene.isDirty;
            var objects = scene.GetRootGameObjects();
            T Find<T>() where T : Component => objects.SelectMany(item => item.GetComponentsInChildren<T>(true)).Single();
            var level = AssetDatabase.LoadAssetAtPath<SparkLevelDefinition>(Folder + "Level_79.asset");
            var drawing = AssetDatabase.LoadAssetAtPath<SparkLevelDrawingAsset>(Folder + "Level_79_Drawing.asset");
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
            else if (dirty) Debug.Log("MainMenu had unsaved edits. Save the scene to persist Level 79's campaign and teaching entries.");
        }

        private static void Append(Object target, string field, Object value)
        {
            if (value == null) throw new InvalidOperationException("Missing Level 79 asset for " + field);
            var data = new SerializedObject(target);
            var list = data.FindProperty(field);
            for (int i = 0; i < list.arraySize; i++)
                if (list.GetArrayElementAtIndex(i).objectReferenceValue == value) return;
            list.arraySize++;
            list.GetArrayElementAtIndex(list.arraySize - 1).objectReferenceValue = value;
            data.ApplyModifiedProperties();
        }

        [MenuItem("Tools/Project Spark/Level 79/Fit Breadboard Assembly")]
        public static void Fit()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Exit Play Mode before fitting Level 79.");
            var models = Models.Select((name, index) =>
            {
                string path = Folder + $"Models/L79_{index:00}_{name}_Fitted.fbx";
                return AssetDatabase.LoadAssetAtPath<GameObject>(path) ??
                    throw new InvalidOperationException("Missing fitted kit module " + path);
            }).ToArray();
            if (AssetDatabase.LoadAssetAtPath<GameObject>(Prefab) == null)
                throw new InvalidOperationException("The Level 79 prefab is missing.");

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
                    throw new InvalidOperationException("Unity could not save the Level 79 layout.");
                AssetDatabase.SaveAssets();
                Debug.Log("Level 79 fitted to the MainMenu breadboard with 188 seated terminals.");
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
            var bench = root.transform.Find("Bonding Workbench");
            var bindings = root.GetComponent<SparkLevelSceneBindings>();
            var loop = root.GetComponent<SparkLevel79Process>();
            if (bench == null || bindings == null || loop == null)
                throw new InvalidOperationException("Level 79 is missing its workbench, controls, or scene bindings.");

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

            foreach (string name in new[] {"L79_HD_VISUAL_ART", "Breadboard Labels", "Motion pivots"})
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
            var assembly = Child(bench, "L79_HD_VISUAL_ART", new Vector3(0,floor,-.010f));
            foreach (var model in models)
            {
                var instance = Object.Instantiate(model, assembly, false).transform;
                var markers = instance.GetComponentsInChildren<Transform>(true)
                    .Where(item => item.name.StartsWith("L79_Board",StringComparison.Ordinal))
                    .ToDictionary(item => item.name);
                Transform Marker(string name) => markers["L79_Board" + name];
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
            foreach (var mesh in assembly.GetComponentsInChildren<MeshRenderer>())
            {
                string key = mesh.name.Substring(mesh.name.LastIndexOf("__", StringComparison.Ordinal) + 2);
                if(key=="tint1D303F" && mesh.name.StartsWith("L79_TransparentVerticalBondingTower_",StringComparison.Ordinal)) key="glass";
                var material = MaterialFor(shader, key);
                // The fitted meshes can contain several imported submeshes.
                mesh.sharedMaterials = Enumerable.Repeat(material, mesh.GetComponent<MeshFilter>().sharedMesh.subMeshCount).ToArray();
                if (key is "glass" or "acrylic") mesh.shadowCastingMode = ShadowCastingMode.Off;
            }

            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (loop.calibrationText == null)
            {
                var oldControls = root.transform.Find("Level 79 Controls");
                if (oldControls != null) Object.DestroyImmediate(oldControls.gameObject);
                SparkLevel79Builder.BuildControls(root.transform,loop,font);
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
                terminalData.FindProperty("maxConnections").intValue = SparkLevel79Process.Wires.Count(wire => wire.a == index || wire.b == index);
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






                ("CHIPLETS / LOADING",-.865f,.147f),("OPTICAL METROLOGY",.862f,.147f),
                ("POSE / SURFACE",-.86f,-.235f),("LINK TEST / SAFETY",.86f,-.235f)
            };
            foreach(var title in titles)
                Label(labels,font,title.title,title.title,new Vector3(title.x,floor+.058f,title.z),.0165f);
            Label(labels,font,"Process title","LOGIC  /  MEMORY  /  I/O  /  SENSOR",new Vector3(0,floor+.068f,-.252f),.013f);
            Label(labels,font,"Reliability title","ACOUSTIC / IR / KELVIN",new Vector3(.31f,floor+.18f,-.16f),.012f);
            Transform Pivot(string module,string key)
            {
                var name="fitted_"+module+"_"+key;
                return assembly.GetComponentsInChildren<Transform>(true).Single(item=>item.name==name);
            }
            loop.rig.art=assembly;
            loop.rig.pivots=new[]{
                Pivot("PrecisionChipletTray","ANIM_CHIPLET_TRAY"),
                Pivot("PrecisionWaferLoadLock","ANIM_LOADLOCK_DOOR"),
                Pivot("DieTransferAirCurtain","ANIM_AIR_CURTAIN"),
                Pivot("HighPrecisionWaferPreAligner","ANIM_BASE_WAFER_ALIGNER"),
                Pivot("SixAxisHybridBondingGantry","ANIM_GANTRY_X"),
                Pivot("GantryYCrossbeam","ANIM_GANTRY_Y"),
                Pivot("GantryZPrecisionElevator","ANIM_GANTRY_Z"),
                Pivot("UpperDieElectrostaticChuck","ANIM_GRIPPER_SLIDE"),
                Pivot("UpperDieElectrostaticChuck","ANIM_GRIPPER_ROTATION"),
                Pivot("UpperDieElectrostaticChuck","ANIM_UPPER_CHUCK_CLAMP"),
                Pivot("BaseWaferVacuumChuck","ANIM_LOWER_CHUCK_THETA"),
                Pivot("BaseWaferVacuumChuck","ANIM_LOWER_WAFER_LIFT"),
                Pivot("HexapodFineAlignmentStage","ANIM_FINE_HEXAPOD"),
                Pivot("NanoXPositioningActuator","ANIM_NANO_X"),
                Pivot("NanoYPositioningActuator","ANIM_NANO_Y"),
                Pivot("ThetaMicroRotationStage","ANIM_MICRO_THETA"),
                Pivot("TipTiltCompensationPlatform","ANIM_TIP_AXIS"),
                Pivot("TipTiltCompensationPlatform","ANIM_TILT_AXIS"),
                Pivot("FiducialOpticalCameraA","ANIM_CAMERA_A"),
                Pivot("FiducialOpticalCameraB","ANIM_CAMERA_B"),
                Pivot("ThroughSiliconInfraredCamera","ANIM_INFRARED_LENS"),
                Pivot("LaserDisplacementInterferometer","ANIM_INTERFEROMETER_HEAD"),
                Pivot("MoireOverlayProjectionSystem","ANIM_MOIRE_OVERLAY"),
                Pivot("WaferBowInterferometryScanner","ANIM_WARPAGE_SCANNER"),
                Pivot("DarkfieldParticleInspectionSystem","ANIM_PARTICLE_SCANNER"),
                Pivot("LowDamageSurfaceActivationModule","ANIM_SURFACE_ACTIVATION"),
                Pivot("DrySurfaceCleaningModule","ANIM_DRY_CLEAN_HEAD"),
                Pivot("TransparentBondChamberWindow","ANIM_BOND_CHAMBER_LID"),
                Pivot("ControlledContactInitiationHead","ANIM_CONTACT_INITIATOR"),
                Pivot("BondHeadMicroforceArray","ANIM_MICROFORCE_ARRAY"),
                Pivot("HolographicBondWaveCrossSection","ANIM_BOND_WAVE_HOLOGRAM"),
                Pivot("InterfaceGapSensorArray","ANIM_INTERFACE_GAP_SCANNER"),
                Pivot("BondThermalConsolidationChamber","ANIM_ANNEAL_CHAMBER_DOOR"),
                Pivot("MultiZoneBondHeater","ANIM_UPPER_HEATER"),
                Pivot("MultiZoneBondHeater","ANIM_LOWER_HEATER"),
                Pivot("DifferentialExpansionAnalyzer","ANIM_THERMAL_EXPANSION_SCAN"),
                Pivot("ScanningAcousticVoidInspector","ANIM_ACOUSTIC_SCAN_X"),
                Pivot("ScanningAcousticVoidInspector","ANIM_ACOUSTIC_SCAN_Y"),
                Pivot("InfraredBondVoidTomography","ANIM_IR_TOMOGRAPHY"),
                Pivot("BondSeamInspectionCamera","ANIM_BOND_SEAM_CAMERA"),
                Pivot("BondAdhesionWitnessCouponTester","ANIM_ADHESION_COUPON_TEST"),
                Pivot("CrossDieKelvinResistanceProbe","ANIM_KELVIN_PROBE"),
                Pivot("VerticalDaisyChainTester","ANIM_DAISY_CHAIN_PROBE"),
                Pivot("HighSpeedBERJitterAnalyzer","ANIM_HIGH_SPEED_TEST_HEAD"),
                Pivot("ThermalCycleQualificationBay","ANIM_THERMAL_CYCLING_TRAY"),
                Pivot("StackWarpageMetrologyScanner","ANIM_STACK_WARPAGE_SCAN"),
                Pivot("CriticalMotionSafetyStation","ANIM_EMERGENCY_STOP"),
                Pivot("Holographic3DStackSCADA","ANIM_3D_STACK_SCADA"),
            };
            Transform ArtModule(string name)=>assembly.GetComponentsInChildren<Transform>(true)
                .Single(item=>item.name=="L79_"+name);
            loop.rig.chiplets=new[]{"LogicChiplet","MemoryChiplet","IOChiplet","SensorChiplet"}
                .Select(ArtModule).ToArray();
            foreach(var die in loop.rig.chiplets)die.SetParent(assembly,true);
            loop.rig.stackHologram=ArtModule("Completed3DChipStackHologram");
            var pivots=loop.rig.pivots;
            pivots[5].SetParent(pivots[4],true);
            pivots[6].SetParent(pivots[5],true);
            pivots[7].SetParent(pivots[6],true);
            pivots[8].SetParent(pivots[7],true);
            pivots[9].SetParent(pivots[8],true);
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
            Lamp("Bonding work light",new Vector3(-.38f,floor+.58f,-.14f),new Color(.91f,.94f,1),1.35f,1.2f);
            Lamp("Metrology fill",new Vector3(.54f,floor+.34f,.16f),new Color(.49f,.80f,1),.64f,.76f);
            loop.rig.BindEffects(
                Lamp("Contact light",new Vector3(-.37f,floor+.20f,-.03f),new Color(.29f,.75f,1),1,.45f),
                Lamp("Consolidation light",new Vector3(0,floor+.20f,-.03f),new Color(.48f,.39f,1),1,.46f),
                Lamp("Inspection light",new Vector3(.28f,floor+.2f,-.03f),new Color(.58f,.37f,1),1,.45f),
                Lamp("Isolation alarm",new Vector3(.56f,floor+.24f,.03f),new Color(1,.18f,.12f),1.25f,.55f));
            if(loop.plot.transform.Find("Trend legend") == null)
            {
                var oldPanel = root.transform.Find("Level 79 Controls");
                if (oldPanel != null) Object.DestroyImmediate(oldPanel.gameObject);
                SparkLevel79Builder.BuildControls(root.transform,loop,font);
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
            loop.controllerDisplay=Readout("Laser readout",new Vector3(-.815f,floor+.075f,.215f),new Color(.46f,.94f,1f));
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










