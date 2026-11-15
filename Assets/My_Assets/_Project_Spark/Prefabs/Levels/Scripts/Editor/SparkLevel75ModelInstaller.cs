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
    public static class SparkLevel75ModelInstaller
    {
        public const string Folder = "Assets/My_Assets/_Project_Spark/Prefabs/Levels/Level_75_Layout/";
        public const string Prefab = Folder + "Level_75_Layout.prefab";
        public const string ScenePath = "Assets/My_Assets/_Project_Spark/Scenes/MainMenu/MainMenu.unity";
        private static readonly string[] Models = {
            "IndustrialIonImplantationBase","TransparentBeamlineSafetyEnclosure","PrecisionIonSourceAssembly","VirtualDopantSourceCartridge","IonSourcePlasmaChamber","IonSourceEmissionModule","IonExtractionElectrodeAssembly","VirtualHighVoltagePowerCabinet","MagneticMassAnalyzer","MassResolvingAperture","BeamEnergySelectionModule","CeramicIonAccelerationColumn","NeutralParticleSeparationModule","QuadrupoleBeamFocusingLens","ElectromagneticBeamSteeringAssembly","ProgrammableXYBeamScanner","FastBeamBlankingGate","PrimaryBeamCurrentMonitor","PrecisionFaradayCupDosimeter","FourCornerBeamUniformityCups","WaferChargeNeutralizationModule","TransparentWaferEndstation","ElectrostaticWaferHoldingChuck","WaferTiltRotationStage","PatternedSiliconWafer","DevelopedPhotoresistImplantMask","ImplantedDopantRegionVisualization","HolographicDopantDepthProfile","AutomatedWaferTransferRobot","WaferCarrierCassette","VacuumWaferLoadLock","LoadLockGateValve","TurbomolecularVacuumPump","VacuumBackingPump","AutomaticVacuumThrottleValve","PrecisionVacuumPressureGauge","ResidualGasDiagnosticModule","WaferTemperatureSensor","WaferCoolingSystem","ProtectedBeamDumpCollector","BeamlineSafetyInterlockRelay","BeamProfileInspectionScanner","IonSpeciesPurityAnalyzer","IntegratedIonDoseController","WaferDoseUniformityAnalyzer","WaferSurfacePotentialSensor","CrystalLatticeDamageAnalyzer","RapidThermalActivationChamber","MultiZoneActivationHeater","ActivationTemperatureSensor","SheetResistanceMetrologyProbe","DopantJunctionDepthScanner","WaferRegistrationCamera","IonImplantationPLCRack","BeamPowerIsolationController","EmergencyImplantationSafetyStation","HolographicIonImplantationSCADA","Regulated5VControlSupply"
        };
        private const string Materials = Folder + "Models/Materials";
        private const float SocketHeight = .025f;
        private const float SocketDiameter = .026f;

        private static readonly (string id, string label, float x, float z, string color)[] Sockets =
        {
            ("SUPPLY_POS","SUP POS",-0.9100f,0.6800f,"red"),
            ("PLC_5V","PLC 5V",-0.8360f,0.6800f,"red"),
            ("SUPPLY_RET","SUP RET",-0.7620f,0.6800f,"blue"),
            ("PLC_GND","PLC GND",-0.6880f,0.6800f,"blue"),
            ("ISO_5V","ISO 5V",-0.6140f,0.6800f,"red"),
            ("ISO_GND","ISO GND",-0.5400f,0.6800f,"blue"),
            ("DOSE_5V","DOSE 5V",-0.4660f,0.6800f,"red"),
            ("DOSE_GND","DOSE GND",-0.3920f,0.6800f,"blue"),
            ("SAFE_5V","SAFE 5V",-0.3180f,0.6800f,"red"),
            ("SAFE_GND","SAFE GND",-0.2440f,0.6800f,"blue"),
            ("FARADAY_5V","FARADAY 5V",-0.1700f,0.6800f,"red"),
            ("FARADAY_GND","FARADAY GND",-0.0960f,0.6800f,"blue"),
            ("PLC_SOURCE","PLC SOURCE",-0.0220f,0.6800f,"mint"),
            ("SOURCE_EN","SOURCE EN",0.0520f,0.6800f,"mint"),
            ("ISO_BEAM","ISO BEAM",0.1260f,0.6800f,"mint"),
            ("BEAM_POWER_EN","BEAM POWER EN",0.2000f,0.6800f,"mint"),
            ("EXTRACT_CMD","EXTRACT CMD",0.2740f,0.6800f,"mint"),
            ("EXTRACT_REQ","EXTRACT REQ",0.3480f,0.6800f,"mint"),
            ("CARTRIDGE_OK","CARTRIDGE OK",0.4220f,0.6800f,"mint"),
            ("SOURCE_CART","SOURCE CART",0.4960f,0.6800f,"mint"),
            ("CHAMBER_OK","CHAMBER OK",0.5700f,0.6800f,"mint"),
            ("SOURCE_READY","SOURCE READY",0.6440f,0.6800f,"mint"),
            ("EMISSION_OUT","EMISSION OUT",0.7180f,0.6800f,"mint"),
            ("PLC_EMISSION","PLC EMISSION",0.7920f,0.6800f,"mint"),
            ("EXTRACT_OK","EXTRACT OK",0.8660f,0.6800f,"mint"),
            ("PLC_EXTRACT","PLC EXTRACT",0.9400f,0.6800f,"mint"),
            ("SRC_CURRENT","SRC CURR",-0.9300f,0.5450f,"mint"),
            ("IO_SRC_I","IO SRC I",-0.8540f,0.5450f,"mint"),
            ("EMISSION_PERMIT","EMISSION PERMIT",-0.7780f,0.5450f,"amber"),
            ("SOURCE_PERMIT","SOURCE PERMIT",-0.7020f,0.5450f,"amber"),
            ("POWER_PERMIT","POWER PERMIT",-0.6260f,0.5450f,"amber"),
            ("DOMAIN_PERMIT","DOMAIN PERMIT",-0.5500f,0.5450f,"amber"),
            ("SRC_POWER","SRC POWER",-0.4740f,0.5450f,"mint"),
            ("IO_SRC_PWR","IO SRC PWR",-0.3980f,0.5450f,"mint"),
            ("INTENSITY_SET","INTENSITY SET",-0.3220f,0.5450f,"mint"),
            ("SOURCE_SET","SOURCE SET",-0.2460f,0.5450f,"mint"),
            ("BEAM_FAULT","BEAM FAULT",-0.1700f,0.5450f,"amber"),
            ("IO_BEAM_FLT","IO BEAM FLT",-0.0940f,0.5450f,"mint"),
            ("TRANSPORT_IN","TRANSPORT IN",-0.0180f,0.5450f,"mint"),
            ("ENTRANCE_MON","ENTRANCE MON",0.0580f,0.5450f,"mint"),
            ("EXTRACTION_ENABLE","EXTRACTION ENABLE",0.1340f,0.5450f,"mint"),
            ("EXTRACTION_IN","EXTRACTION IN",0.2100f,0.5450f,"mint"),
            ("ISO_STATUS","ISO STATUS",0.2860f,0.5450f,"mint"),
            ("IO_ISO_READY","IO ISO READY",0.3620f,0.5450f,"mint"),
            ("MAG_SET","MAG SET",0.4380f,0.5450f,"mint"),
            ("FIELD_CMD","FIELD CMD",0.5140f,0.5450f,"mint"),
            ("FIELD_MON","FIELD MON",0.5900f,0.5450f,"mint"),
            ("IO_MAG_FIELD","IO MAG FIELD",0.6660f,0.5450f,"mint"),
            ("PURITY_OUT","PURITY OUT",0.7420f,0.5450f,"mint"),
            ("IO_PURITY","IO PURITY",0.8180f,0.5450f,"mint"),
            ("APERTURE_SET","APER SET",0.8940f,0.5450f,"mint"),
            ("APERTURE_CMD","APER CMD",0.9700f,0.5450f,"mint"),
            ("APERTURE_POS","APER POS",-0.9300f,0.4250f,"mint"),
            ("IO_APERTURE","IO APER",-0.8540f,0.4250f,"mint"),
            ("ENERGY_FB","ENERGY FB",-0.7780f,0.4250f,"mint"),
            ("IO_ENERGY","IO ENERGY",-0.7020f,0.4250f,"mint"),
            ("ENERGY_SET","ENERGY SET",-0.6260f,0.4250f,"mint"),
            ("SELECT_ENERGY","SELECT ENERGY",-0.5500f,0.4250f,"mint"),
            ("NEUTRAL_OUT","NEUTRAL OUT",-0.4740f,0.4250f,"mint"),
            ("IO_NEUTRAL","IO NEUTRAL",-0.3980f,0.4250f,"mint"),
            ("SPECIES_OK","SPECIES OK",-0.3220f,0.4250f,"mint"),
            ("PLC_SPECIES","PLC SPECIES",-0.2460f,0.4250f,"mint"),
            ("SPECIES_PERMIT","SPECIES PERMIT",-0.1700f,0.4250f,"amber"),
            ("PLC_SPEC_LOCK","PLC SPEC LOCK",-0.0940f,0.4250f,"mint"),
            ("SELECTED_I","SELECTED I",-0.0180f,0.4250f,"mint"),
            ("ANALYZER_IN","ANALYZER IN",0.0580f,0.4250f,"mint"),
            ("TRANSMITTED_I","XMIT I",0.1340f,0.4250f,"mint"),
            ("ANALYZER_OUT","ANALYZER OUT",0.2100f,0.4250f,"mint"),
            ("SPECIES_CAL","SPECIES CAL",0.2860f,0.4250f,"mint"),
            ("PURITY_CAL","PURITY CAL",0.3620f,0.4250f,"mint"),
            ("PURITY_OK","PURITY OK",0.4380f,0.4250f,"mint"),
            ("HMI_PURITY","HMI PURITY",0.5140f,0.4250f,"mint"),
            ("PLC_FOCUS","PLC FOCUS",0.5900f,0.4250f,"mint"),
            ("LENS_EN","LENS EN",0.6660f,0.4250f,"mint"),
            ("STEER_SET","STEER SET",0.7420f,0.4250f,"mint"),
            ("STEER_CMD","STEER CMD",0.8180f,0.4250f,"mint"),
            ("FOCUS_OUT","FOCUS OUT",0.8940f,0.4250f,"mint"),
            ("IO_FOCUS","IO FOCUS",0.9700f,0.4250f,"mint"),
            ("CENTROID","CENTROID",-1.0000f,-0.3550f,"mint"),
            ("IO_CENTROID","IO CENTROID",-0.9250f,-0.3550f,"mint"),
            ("SCAN_ENABLE","SCAN ENABLE",-0.8500f,-0.3550f,"mint"),
            ("XY_RUN","XY RUN",-0.7750f,-0.3550f,"mint"),
            ("SCAN_POS","SCAN POS",-0.7000f,-0.3550f,"mint"),
            ("IO_SCAN_POS","IO SCAN POS",-0.6250f,-0.3550f,"mint"),
            ("BEAM_PERMIT","BEAM PERMIT",-0.5500f,-0.3550f,"amber"),
            ("BLANK_ENABLE","BLANK ENABLE",-0.4750f,-0.3550f,"mint"),
            ("BLANK_POS","BLANK POS",-0.4000f,-0.3550f,"mint"),
            ("IO_BLANK","IO BLANK",-0.3250f,-0.3550f,"mint"),
            ("PROFILE_OK","PROFILE OK",-0.2500f,-0.3550f,"mint"),
            ("PLC_PROFILE","PLC PROFILE",-0.1750f,-0.3550f,"mint"),
            ("BEAM_CURRENT","BEAM CURR",-0.1000f,-0.3550f,"mint"),
            ("IO_BEAM_I","IO BEAM I",-0.0250f,-0.3550f,"mint"),
            ("PROFILER_REQ","PROFILER REQ",0.0500f,-0.3550f,"mint"),
            ("PROFILE_RUN","PROFILE RUN",0.1250f,-0.3550f,"mint"),
            ("PROFILE_OUT","PROFILE OUT",0.2000f,-0.3550f,"mint"),
            ("IO_PROFILE","IO PROFILE",0.2750f,-0.3550f,"mint"),
            ("ISO_OVERRIDE","ISO OVERRIDE",0.3500f,-0.3550f,"mint"),
            ("BLANK_OVERRIDE","BLANK OVERRIDE",0.4250f,-0.3550f,"mint"),
            ("DWELL_OUT","DWELL OUT",0.5000f,-0.3550f,"mint"),
            ("IO_DWELL","IO DWELL",0.5750f,-0.3550f,"mint"),
            ("ROBOT_REQ","ROBOT REQ",0.6500f,-0.3550f,"mint"),
            ("ROBOT_RUN","ROBOT RUN",0.7250f,-0.3550f,"mint"),
            ("ROBOT_RDY","ROBOT RDY",0.8000f,-0.3550f,"mint"),
            ("IO_ROBOT","IO ROBOT",0.8750f,-0.3550f,"mint"),
            ("LOCK_CTL","LOCK CTL",0.9500f,-0.3550f,"mint"),
            ("LOADLOCK_CMD","LOADLOCK CMD",1.0250f,-0.3550f,"mint"),
            ("GATE_SET","GATE SET",-1.0000f,-0.5250f,"mint"),
            ("GATE_CMD","GATE CMD",-0.9285f,-0.5250f,"mint"),
            ("GATE_POS","GATE POS",-0.8570f,-0.5250f,"mint"),
            ("IO_GATE","IO GATE",-0.7855f,-0.5250f,"mint"),
            ("CLAMP_REQ","CLAMP REQ",-0.7140f,-0.5250f,"mint"),
            ("CHUCK_EN","CHUCK EN",-0.6425f,-0.5250f,"mint"),
            ("CHUCK_POS","CHUCK POS",-0.5710f,-0.5250f,"mint"),
            ("IO_CLAMP","IO CLAMP",-0.4995f,-0.5250f,"mint"),
            ("ANGLE_SET","ANGLE SET",-0.4280f,-0.5250f,"mint"),
            ("WAFER_TILT","WAFER TILT",-0.3565f,-0.5250f,"mint"),
            ("ANGLE_FB","ANGLE FB",-0.2850f,-0.5250f,"mint"),
            ("IO_TILT","IO TILT",-0.2135f,-0.5250f,"mint"),
            ("REGISTER_OUT","REGISTER OUT",-0.1420f,-0.5250f,"mint"),
            ("IO_REG","IO REG",-0.0705f,-0.5250f,"mint"),
            ("MASK_OK","MASK OK",0.0010f,-0.5250f,"mint"),
            ("NEUTRAL_READY","NEUTRAL READY",0.0725f,-0.5250f,"mint"),
            ("FARADAY_CUP_CURRENT_OUT","CUP I OUT",0.1440f,-0.5250f,"mint"),
            ("ION_DOSE_CONTROLLER_BEAM_CURRENT_IN","DOSE I IN",0.2155f,-0.5250f,"mint"),
            ("DOSE_UPSTREAM","DOSE UPSTREAM",0.2870f,-0.5250f,"mint"),
            ("CORNER_UNIFORM","CORNER UNIF",0.3585f,-0.5250f,"mint"),
            ("DOSE_REACHED","DOSE REACHED",0.4300f,-0.5250f,"mint"),
            ("UNIFORM_OUT","UNIF OUT",0.5015f,-0.5250f,"mint"),
            ("PROFILE_MAP","PROFILE MAP",0.5730f,-0.5250f,"mint"),
            ("SPECIES_DOSE","SPECIES DOSE",0.6445f,-0.5250f,"mint"),
            ("WAFER_POTENTIAL","WAFER POTENTIAL",0.7160f,-0.5250f,"mint"),
            ("DAMAGE_WARN","DAMAGE WARN",0.7875f,-0.5250f,"mint"),
            ("SHEET_RESULT","SHEET RESULT",0.8590f,-0.5250f,"mint"),
            ("DEPTH_RESULT","DEPTH RESULT",0.9305f,-0.5250f,"mint"),
            ("DEPTH_START","DEPTH START",1.0020f,-0.5250f,"mint"),
            ("SHEET_START","SHEET START",-1.0000f,-0.7000f,"mint"),
            ("HMI_DOSE","HMI DOSE",-0.9285f,-0.7000f,"mint"),
            ("BACKING_EN","BACKING EN",-0.8570f,-0.7000f,"mint"),
            ("TURBO_EN","TURBO EN",-0.7855f,-0.7000f,"mint"),
            ("PRESSURE_FB","PRESS FB",-0.7140f,-0.7000f,"mint"),
            ("THROTTLE_CMD","THROTTLE CMD",-0.6425f,-0.7000f,"mint"),
            ("GAS_DIAG","GAS DIAG",-0.5710f,-0.7000f,"mint"),
            ("WAFER_TEMP","WAFER TEMP",-0.4995f,-0.7000f,"mint"),
            ("COOLING_EN","COOLING EN",-0.4280f,-0.7000f,"mint"),
            ("ACTIVATE_START","ACTIVATE START",-0.3565f,-0.7000f,"mint"),
            ("HEATER_STATUS","HEATER STATUS",-0.2850f,-0.7000f,"mint"),
            ("ACTIVATION_TEMP","ACTIV TEMP",-0.2135f,-0.7000f,"mint"),
            ("SAFETY_READY","SAFE READY",-0.1420f,-0.7000f,"amber"),
            ("ESTOP_NC","ESTOP NC",-0.0705f,-0.7000f,"amber"),
            ("SAFE_STOP_IN","SAFE STOP IN",0.0010f,-0.7000f,"amber"),
            ("ESTOP_STATUS","ESTOP STATUS",0.0725f,-0.7000f,"amber"),
            ("ISO_READY","ISO READY",0.1440f,-0.7000f,"mint"),
            ("ISO_CMD","ISO CMD",0.2155f,-0.7000f,"mint"),
            ("PLC_TELEMETRY","PLC TELEMETRY",0.2870f,-0.7000f,"mint"),
            ("HMI_CMD","HMI CMD",0.3585f,-0.7000f,"mint"),
            ("DOSE_INHIBIT","DOSE INHIBIT",0.4300f,-0.7000f,"mint"),
            ("DOSE_INTERLOCK","DOSE INTERLOCK",0.5015f,-0.7000f,"mint"),
            ("CHARGE_DIAG","CHARGE DIAG",0.5730f,-0.7000f,"mint"),
            ("HMI_DAMAGE","HMI DAMAGE",0.6445f,-0.7000f,"mint"),
            ("ACTIVATION_RESULT","ACTIV RESULT",0.7160f,-0.7000f,"mint"),
            ("HEAT_LIMIT","HEAT LIMIT",0.7875f,-0.7000f,"mint"),
            ("HMI_DEPTH","HMI DEPTH",0.8590f,-0.7000f,"mint"),
            ("FARADAY_VALID","FARADAY VALID",0.9305f,-0.7000f,"mint"),
            ("BEAM_BLANK_REQ","BEAM BLANK REQ",1.0020f,-0.7000f,"mint"),
        };

        public static IEnumerable<string> TerminalIds => Sockets.Select(socket => socket.id);

        private static readonly Dictionary<string, string> Palette = new()
        {
            {"red","EE485C"},{"blue","1711E2"},{"amber","FFA82B"},{"violet","A282FF"},{"plasma","A45DFF"},
            {"mint","34F6C5"},{"cyan","23DAFD"},{"void","07111C"},{"white","D4F5FF"},{"glass","498BA8"}
        };

        [MenuItem("Tools/Project Spark/Levels/Create Level 75")]
        public static void Install()
        {
            SparkLevel75Builder.Create();
            var root = AssetDatabase.LoadAssetAtPath<GameObject>(Prefab);
            if (root == null || root.GetComponent<SparkLevel75Process>().controllerDisplay == null) Fit();
            Register();
            ValidateAssets();
        }

        [MenuItem("Tools/Project Spark/Level 75/Validate Level Assets")]
        public static void ValidateAssets()
        {
            var root=AssetDatabase.LoadAssetAtPath<GameObject>(Prefab);
            var control=root != null ? root.GetComponent<SparkLevel75Process>() : null;
            var bindings=root != null ? root.GetComponent<SparkLevelSceneBindings>() : null;
            if (control==null || bindings==null || control.controllerDisplay==null || control.qualityDisplay==null || control.rig==null || control.rig.pivots.Length!=38 ||
                control.pins.Length!=164 || control.pins.Distinct().Count()!=164 || 
                !TerminalIds.SequenceEqual(control.pins.Select(pin=>pin.name)) ||
                control.pins.Any(pin=>!bindings.TryGetTerminal(pin.name,out var bound) || bound!=pin) ||
                SparkLevel75Process.Gates.Any(id=>!bindings.IsComponentIdDeclared(id)))
                throw new InvalidOperationException("Level 75 wiring, motion or binding references are incomplete.");
            Debug.Log("[LEVEL 75] Validated 164 distinct terminals, 106 player wires and 6 evaluator gates.",root);
        }

        private static void Register()
        {
            var scene = SceneManager.GetSceneByPath(ScenePath);
            bool opened = !scene.isLoaded;
            if (opened) scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
            bool dirty = scene.isDirty;
            var objects = scene.GetRootGameObjects();
            T Find<T>() where T : Component => objects.SelectMany(item => item.GetComponentsInChildren<T>(true)).Single();
            var level = AssetDatabase.LoadAssetAtPath<SparkLevelDefinition>(Folder + "Level_75.asset");
            var drawing = AssetDatabase.LoadAssetAtPath<SparkLevelDrawingAsset>(Folder + "Level_75_Drawing.asset");
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
            else if (dirty) Debug.Log("MainMenu had unsaved edits. Save the scene to persist Level 75's campaign and teaching entries.");
        }

        private static void Append(Object target, string field, Object value)
        {
            if (value == null) throw new InvalidOperationException("Missing Level 75 asset for " + field);
            var data = new SerializedObject(target);
            var list = data.FindProperty(field);
            for (int i = 0; i < list.arraySize; i++)
                if (list.GetArrayElementAtIndex(i).objectReferenceValue == value) return;
            list.arraySize++;
            list.GetArrayElementAtIndex(list.arraySize - 1).objectReferenceValue = value;
            data.ApplyModifiedProperties();
        }

        [MenuItem("Tools/Project Spark/Level 75/Fit Breadboard Assembly")]
        public static void Fit()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Exit Play Mode before fitting Level 75.");
            var models = Models.Select((name, index) =>
            {
                string path = Folder + $"Models/L75_{index:00}_{name}_Fitted.fbx";
                return AssetDatabase.LoadAssetAtPath<GameObject>(path) ??
                    throw new InvalidOperationException("Missing fitted kit module " + path);
            }).ToArray();
            if (AssetDatabase.LoadAssetAtPath<GameObject>(Prefab) == null)
                throw new InvalidOperationException("The Level 75 prefab is missing.");

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
                    throw new InvalidOperationException("Unity could not save the Level 75 layout.");
                AssetDatabase.SaveAssets();
                Debug.Log("Level 75 fitted to the MainMenu breadboard with 164 seated terminals.");
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
            var bench = root.transform.Find("Implantation Workbench");
            var bindings = root.GetComponent<SparkLevelSceneBindings>();
            var loop = root.GetComponent<SparkLevel75Process>();
            if (bench == null || bindings == null || loop == null)
                throw new InvalidOperationException("Level 75 is missing its workbench, controls, or scene bindings.");

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

            foreach (string name in new[] {"L75_HD_VISUAL_ART", "Breadboard Labels", "Motion pivots"})
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
            var assembly = Child(bench, "L75_HD_VISUAL_ART", new Vector3(0,floor,-.021f));
            foreach (var model in models)
            {
                var instance = Object.Instantiate(model, assembly, false).transform;
                var markers = instance.GetComponentsInChildren<Transform>(true)
                    .Where(item => item.name.StartsWith("L75_Board",StringComparison.Ordinal))
                    .ToDictionary(item => item.name);
                Transform Marker(string name) => markers["L75_Board" + name];
                Vector3 Axis(string name) => assembly.InverseTransformVector(Marker(name).position - Marker("Center").position);
                instance.localRotation = Quaternion.Inverse(Quaternion.LookRotation(-Axis("Front"), Axis("Up"))) * instance.localRotation;
                instance.localScale /= Axis("Right").magnitude;
                instance.localPosition -= assembly.InverseTransformPoint(Marker("Center").position);
                if (Axis("Right").x < .99f)
                    throw new InvalidOperationException("Unexpected axes on fitted model " + model.name);
            }

            assembly.localScale = Vector3.one * .65f;
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
                var oldControls = root.transform.Find("Level 75 Controls");
                if (oldControls != null) Object.DestroyImmediate(oldControls.gameObject);
                SparkLevel75Builder.BuildControls(root.transform,loop,font);
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
                terminalData.FindProperty("maxConnections").intValue = SparkLevel75Process.Wires.Count(wire => wire.a == index || wire.b == index);
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






                ("SOURCE / MASS",-.865f,.147f),("WAFER / DOSE",.862f,.147f),
                ("VACUUM / POWER",-.86f,-.235f),("SAFETY / METROLOGY",.86f,-.235f)
            };
            foreach(var title in titles)
                Label(labels,font,title.title,title.title,new Vector3(title.x,floor+.058f,title.z),.0165f);
            Label(labels,font,"Beamline label","VIRTUAL BEAMLINE",new Vector3(-.022f,floor+.30f,.095f),.012f);
            Label(labels,font,"Activation label","ACTIVATE / INSPECT",new Vector3(.35f,floor+.23f,-.075f),.012f);
            Transform Pivot(string module,string key)
            {
                var name="fitted_"+module+"_"+key;
                return assembly.GetComponentsInChildren<Transform>(true).Single(item=>item.name==name);
            }
            loop.rig.art=assembly;
            loop.rig.pivots=new[]{
                Pivot("PrecisionIonSourceAssembly","ANIM_ION_SOURCE"),
                Pivot("VirtualDopantSourceCartridge","ANIM_SOURCE_CARTRIDGE"),
                Pivot("IonSourcePlasmaChamber","ANIM_SOURCE_LID"),
                Pivot("IonSourceEmissionModule","ANIM_SOURCE_EMISSION"),
                Pivot("IonExtractionElectrodeAssembly","ANIM_EXTRACTION"),
                Pivot("MagneticMassAnalyzer","ANIM_ANALYZER_FIELD"),
                Pivot("MassResolvingAperture","ANIM_MASS_APERTURE"),
                Pivot("BeamEnergySelectionModule","ANIM_ENERGY_SELECTOR"),
                Pivot("CeramicIonAccelerationColumn","ANIM_ACCELERATION_COLUMN"),
                Pivot("QuadrupoleBeamFocusingLens","ANIM_QUAD_LENS_A"),
                Pivot("QuadrupoleBeamFocusingLens","ANIM_QUAD_LENS_B"),
                Pivot("ElectromagneticBeamSteeringAssembly","ANIM_BEAM_STEER_X"),
                Pivot("ElectromagneticBeamSteeringAssembly","ANIM_BEAM_STEER_Y"),
                Pivot("ProgrammableXYBeamScanner","ANIM_BEAM_SCAN_X"),
                Pivot("ProgrammableXYBeamScanner","ANIM_BEAM_SCAN_Y"),
                Pivot("FastBeamBlankingGate","ANIM_BEAM_BLANKER"),
                Pivot("PrecisionFaradayCupDosimeter","ANIM_FARADAY_CUP"),
                Pivot("FourCornerBeamUniformityCups","ANIM_CORNER_CUPS"),
                Pivot("WaferChargeNeutralizationModule","ANIM_NEUTRALIZER"),
                Pivot("TransparentWaferEndstation","ANIM_ENDSTATION_DOOR"),
                Pivot("ElectrostaticWaferHoldingChuck","ANIM_WAFER_CHUCK"),
                Pivot("WaferTiltRotationStage","ANIM_WAFER_TILT"),
                Pivot("WaferTiltRotationStage","ANIM_WAFER_ROTATION"),
                Pivot("ImplantedDopantRegionVisualization","ANIM_DOPANT_REGIONS"),
                Pivot("HolographicDopantDepthProfile","ANIM_DOPANT_DEPTH_HOLOGRAM"),
                Pivot("AutomatedWaferTransferRobot","ANIM_WAFER_ROBOT_BASE"),
                Pivot("AutomatedWaferTransferRobot","ANIM_WAFER_ROBOT_ARM"),
                Pivot("AutomatedWaferTransferRobot","ANIM_WAFER_ROBOT_TOOL"),
                Pivot("LoadLockGateValve","ANIM_LOADLOCK_GATE"),
                Pivot("TurbomolecularVacuumPump","ANIM_TURBO_ROTOR"),
                Pivot("VacuumBackingPump","ANIM_BACKING_ROTOR"),
                Pivot("AutomaticVacuumThrottleValve","ANIM_THROTTLE_VALVE"),
                Pivot("BeamProfileInspectionScanner","ANIM_BEAM_PROFILE_SCAN"),
                Pivot("RapidThermalActivationChamber","ANIM_ACTIVATION_DOOR"),
                Pivot("MultiZoneActivationHeater","ANIM_ACTIVATION_HEAT"),
                Pivot("DopantJunctionDepthScanner","ANIM_JUNCTION_METROLOGY"),
                Pivot("EmergencyImplantationSafetyStation","ANIM_IMPLANT_ESTOP"),
                Pivot("HolographicIonImplantationSCADA","ANIM_IMPLANT_SCADA"),
            };
            var pivots=loop.rig.pivots;
            pivots[26].SetParent(pivots[25],true);
            pivots[27].SetParent(pivots[26],true);
            var view=GameObject.CreatePrimitive(PrimitiveType.Sphere);
            view.name="Authorized beam diagnostic";
            view.transform.SetParent(assembly,false);
            view.transform.position=pivots[17].position+assembly.TransformVector(Vector3.up*.027f);
            view.transform.localScale=new Vector3(.14f,.014f,.014f);
            Object.DestroyImmediate(view.GetComponent<Collider>());
            view.GetComponent<Renderer>().sharedMaterial=MaterialFor(shader,"plasma");
            view.GetComponent<Renderer>().shadowCastingMode=ShadowCastingMode.Off;
            view.SetActive(false);
            loop.rig.beam=view.transform;
            if(loop.plot.transform.Find("Trend legend") == null)
            {
                var oldPanel = root.transform.Find("Level 75 Controls");
                if (oldPanel != null) Object.DestroyImmediate(oldPanel.gameObject);
                SparkLevel75Builder.BuildControls(root.transform,loop,font);
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
            label.fontSize = 75;
            label.characterSize = size;
            label.anchor = TextAnchor.MiddleCenter;
            label.color = new Color(.89f, .96f, 1f);
            label.text = text;
            child.GetComponent<MeshRenderer>().sharedMaterial = font.material;
            return label;
        }

        private static Material MaterialFor(Shader shader, string key)
        {
            string path = $"{Materials}/L75 {key}.mat";
            if (key.StartsWith("tint",StringComparison.Ordinal) && key.Length==10) Palette[key]=key.Substring(4);
            var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing != null && key is not ("glass" or "acrylic") &&
                ColorUtility.TryParseHtmlString("#" + (Palette.TryGetValue(key, out var savedTint) ? savedTint : "9FB3C8"), out var savedColor) &&
                existing.GetColor("_BaseColor") == savedColor) return existing;
            ColorUtility.TryParseHtmlString("#" + (Palette.TryGetValue(key, out var tint) ? tint : "9FB3C8"), out var color);
            var material = existing != null ? existing : new Material(shader) { name = "L75 " + key };
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


