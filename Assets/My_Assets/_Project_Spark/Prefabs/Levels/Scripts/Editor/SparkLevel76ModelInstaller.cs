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
    public static class SparkLevel76ModelInstaller
    {
        public const string Folder = "Assets/My_Assets/_Project_Spark/Prefabs/Levels/Level_76_Layout/";
        public const string Prefab = Folder + "Level_76_Layout.prefab";
        public const string ScenePath = "Assets/My_Assets/_Project_Spark/Scenes/MainMenu/MainMenu.unity";
        private static readonly string[] Models = {
            "IndustrialCopperInterconnectBase","TransparentElectroplatingSafetyEnclosure","PrecisionCopperPlatingCell","TransparentElectrolyteBasin","PatternedDielectricTrenchWafer","ConductiveBarrierSeedLayer","HolographicTrenchViaCrossSection","PrecisionWaferCathodeChuck","CathodeRotarySpindle","CathodeVerticalLiftAssembly","AutomatedWaferTransferRobot","InterconnectWaferCassette","PrewetWaferPreparationChamber","ElectrolyteDegassingModule","VirtualCopperElectrolyteReservoir","ElectrolyteMixingVessel","MicroFiltrationSkid","PrecisionRecirculationPump","ElectrolyteMassFlowController","AnnularFlowDistributionManifold","MultiZoneElectrolyteNozzleArray","SegmentedCopperAnodeAssembly","ProtectedAnodeBasket","AnodeSeparatorMembrane","EdgeCurrentDistributionRing","PulsedCurrentPlatingRectifier","CathodeCurrentShunt","PrecisionCurrentTransducer","PlatingCellVoltageSensor","ReferenceElectrodeProbe","ElectrochemicalPotentialController","MultiZoneCurrentDistributionController","BathConductivitySensor","ElectrolyteTemperatureSensor","ElectrolyteThermalConditioner","VirtualBathConcentrationSensor","VirtualSuppressorCartridge","VirtualAcceleratorCartridge","VirtualLevelerCartridge","AdditiveMeteringManifold","ElectrolyteBubbleDetectionSensor","ElectrolyteLinePressureSensor","WaferCathodeContactResistanceMonitor","FourPointInterconnectTestProbe","OpticalCopperThicknessScanner","ThreeDVoidInspectionScanner","TrenchFillTomographyMonitor","CopperOverburdenAnalyzer","CopperSurfaceRoughnessScanner","PostPlatingWaferRinseChamber","WaferSpinDryer","WasteElectrolyteCollectionModule","ElectrolyteLeakDetectionRelay","ElectroplatingEmergencySafetyStation","CopperElectroplatingPLCRack","IndustrialAnalogIOModule","PlatingWaveformProcessController","HolographicInterconnectSCADA","Regulated5VControlSupply","SeedBarrierContinuityScanner"
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
            ("WAVE_5V","WAVE 5V",-0.5620f,0.6800f,"red"),
            ("WAVE_GND","WAVE GND",-0.4890f,0.6800f,"blue"),
            ("RECT_5V","RECT 5V",-0.4160f,0.6800f,"red"),
            ("RECT_GND","RECT GND",-0.3430f,0.6800f,"blue"),
            ("REF_5V","REF 5V",-0.2700f,0.6800f,"red"),
            ("REF_GND","REF GND",-0.1970f,0.6800f,"blue"),
            ("WAVE_I_CMD","WAVE I CMD",-0.1240f,0.6800f,"mint"),
            ("RECT_I_REQ","RECT I REQ",-0.0510f,0.6800f,"mint"),
            ("ANODE_SUPPLY","ANODE SUP",0.0220f,0.6800f,"mint"),
            ("ANODE_POWER","ANODE POWER",0.0950f,0.6800f,"mint"),
            ("CATHODE_RETURN","CATHODE RETURN",0.1680f,0.6800f,"mint"),
            ("CATHODE_POWER","CATH PWR",0.2410f,0.6800f,"mint"),
            ("SHUNT_OUT","SHUNT OUT",0.3140f,0.6800f,"mint"),
            ("IO_SHUNT","IO SHUNT",0.3870f,0.6800f,"mint"),
            ("CURRENT_OUT","CURR OUT",0.4600f,0.6800f,"mint"),
            ("WAVE_CURRENT","WAVE CURR",0.5330f,0.6800f,"mint"),
            ("CELL_V_OUT","CELL V OUT",0.6060f,0.6800f,"mint"),
            ("IO_CELL_V","IO CELL V",0.6790f,0.6800f,"mint"),
            ("ZONE_A_CMD","ZONE A CMD",0.7520f,0.6800f,"mint"),
            ("ANODE_A","ANODE A",0.8250f,0.6800f,"mint"),
            ("ZONE_B_CMD","ZONE B CMD",0.8980f,0.6800f,"mint"),
            ("ANODE_B","ANODE B",0.9710f,0.6800f,"mint"),
            ("EDGE_CMD","EDGE CMD",-1.0000f,0.5450f,"mint"),
            ("RING_CTRL","RING CTRL",-0.9270f,0.5450f,"mint"),
            ("CONTACT_OHM","CT OHM",-0.8540f,0.5450f,"mint"),
            ("IO_CONTACT","IO CT",-0.7810f,0.5450f,"mint"),
            ("DIST_REQ","DIST REQ",-0.7080f,0.5450f,"mint"),
            ("ANODE_DIST","ANODE DIST",-0.6350f,0.5450f,"mint"),
            ("POWER_READY","POWER READY",-0.5620f,0.5450f,"mint"),
            ("IO_POWER","IO POWER",-0.4890f,0.5450f,"mint"),
            ("RECT_PERMIT","RECT PERMIT",-0.4160f,0.5450f,"amber"),
            ("RECT_ENABLE","RECT ENABLE",-0.3430f,0.5450f,"mint"),
            ("OVERCURRENT","OVERCURR",-0.2700f,0.5450f,"mint"),
            ("IO_OVER_I","IO OVER I",-0.1970f,0.5450f,"mint"),
            ("CONTACT_OK","CT OK",-0.1240f,0.5450f,"mint"),
            ("PLC_CONTACT","PLC CT",-0.0510f,0.5450f,"mint"),
            ("ZONE_I_OUT","ZONE I OUT",0.0220f,0.5450f,"mint"),
            ("IO_ZONE_I","IO ZONE I",0.0950f,0.5450f,"mint"),
            ("PLC_PUMP","PLC PUMP",0.1680f,0.5450f,"mint"),
            ("PUMP_RUN","PUMP RUN",0.2410f,0.5450f,"mint"),
            ("PUMP_FLOW","PUMP FLOW",0.3140f,0.5450f,"mint"),
            ("IO_PUMP_FLOW","IO PUMP FLOW",0.3870f,0.5450f,"mint"),
            ("FLOW_OUT","FLOW OUT",0.4600f,0.5450f,"mint"),
            ("IO_FLOW","IO FLOW",0.5330f,0.5450f,"mint"),
            ("FLOW_CMD","FLOW CMD",0.6060f,0.5450f,"mint"),
            ("FLOW_CONTROLLER","FLOW CTRL",0.6790f,0.5450f,"mint"),
            ("MANIFOLD_OUT","MAN OUT",0.7520f,0.5450f,"mint"),
            ("NOZZLE_FEED","NOZ FEED",0.8250f,0.5450f,"mint"),
            ("NOZZLE_OK","NOZ OK",0.8980f,0.5450f,"mint"),
            ("IO_NOZZLE","IO NOZ",0.9710f,0.5450f,"mint"),
            ("FILTER_OK","FILTER OK",-1.0000f,0.4250f,"mint"),
            ("IO_FILTER","IO FILTER",-0.9270f,0.4250f,"mint"),
            ("PLC_FILTER","PLC FILTER",-0.8540f,0.4250f,"mint"),
            ("FILTER_RUN","FILTER RUN",-0.7810f,0.4250f,"mint"),
            ("RESERVOIR_FEED","RES IN",-0.7080f,0.4250f,"mint"),
            ("PUMP_IN","PUMP IN",-0.6350f,0.4250f,"mint"),
            ("RETURN_FLOW","RETURN FLOW",-0.5620f,0.4250f,"mint"),
            ("RESERVOIR_RETURN","RES RETURN",-0.4890f,0.4250f,"mint"),
            ("DEGAS_OK","DEGAS OK",-0.4160f,0.4250f,"mint"),
            ("IO_DEGAS","IO DEGAS",-0.3430f,0.4250f,"mint"),
            ("PLC_DEGAS","PLC DEGAS",-0.2700f,0.4250f,"mint"),
            ("DEGAS_RUN","DEGAS RUN",-0.1970f,0.4250f,"mint"),
            ("PRESSURE_OUT","PRESS OUT",-0.1240f,0.4250f,"mint"),
            ("IO_PRESSURE","IO PRESS",-0.0510f,0.4250f,"mint"),
            ("BUBBLE_OUT","BUBBLE OUT",0.0220f,0.4250f,"mint"),
            ("IO_BUBBLES","IO BUBBLES",0.0950f,0.4250f,"mint"),
            ("LEAK_OUT","LEAK OUT",0.1680f,0.4250f,"mint"),
            ("IO_LEAK","IO LEAK",0.2410f,0.4250f,"mint"),
            ("WASTE_LEVEL","WASTE LEVEL",0.3140f,0.4250f,"mint"),
            ("IO_WASTE","IO WASTE",0.3870f,0.4250f,"mint"),
            ("TEMP_OUT","TEMP OUT",0.4600f,0.4250f,"mint"),
            ("IO_TEMP","IO TEMP",0.5330f,0.4250f,"mint"),
            ("PLC_HEAT","PLC HEAT",0.6060f,0.4250f,"mint"),
            ("HEAT_CMD","HEAT CMD",0.6790f,0.4250f,"mint"),
            ("HEAT_OK","HEAT OK",0.7520f,0.4250f,"mint"),
            ("IO_HEAT","IO HEAT",0.8250f,0.4250f,"mint"),
            ("COND_OUT","COND OUT",0.8980f,0.4250f,"mint"),
            ("IO_COND","IO COND",0.9710f,0.4250f,"mint"),
            ("CONC_OUT","CONC OUT",-1.0000f,-0.3550f,"mint"),
            ("IO_CONC","IO CONC",-0.9250f,-0.3550f,"mint"),
            ("PLC_DOSE","PLC DOSE",-0.8500f,-0.3550f,"mint"),
            ("ADDITIVE_CMD","ADD CMD",-0.7750f,-0.3550f,"mint"),
            ("SUP_AVAIL","SUP AVAIL",-0.7000f,-0.3550f,"mint"),
            ("IO_SUP","IO SUP",-0.6250f,-0.3550f,"mint"),
            ("ACC_AVAIL","ACC AVAIL",-0.5500f,-0.3550f,"mint"),
            ("IO_ACC","IO ACC",-0.4750f,-0.3550f,"mint"),
            ("LEVEL_AVAIL","LEVEL AVAIL",-0.4000f,-0.3550f,"mint"),
            ("IO_LEVEL","IO LEVEL",-0.3250f,-0.3550f,"mint"),
            ("DOSE_OUT","DOSE OUT",-0.2500f,-0.3550f,"mint"),
            ("IO_ADDITIVE","IO ADD",-0.1750f,-0.3550f,"mint"),
            ("ADD_FEED","ADD FEED",-0.1000f,-0.3550f,"mint"),
            ("BATH_DOSE","BATH DOSE",-0.0250f,-0.3550f,"mint"),
            ("PLC_MIX","PLC MIX",0.0500f,-0.3550f,"mint"),
            ("MIXER_RUN","MIXER RUN",0.1250f,-0.3550f,"mint"),
            ("MIXER_OK","MIXER OK",0.2000f,-0.3550f,"mint"),
            ("IO_MIX","IO MIX",0.2750f,-0.3550f,"mint"),
            ("THERMAL_FB","THERMAL FB",0.3500f,-0.3550f,"mint"),
            ("PLC_TEMP","PLC TEMP",0.4250f,-0.3550f,"mint"),
            ("PLC_ROBOT","PLC ROBOT",0.5000f,-0.3550f,"mint"),
            ("ROBOT_RUN","ROBOT RUN",0.5750f,-0.3550f,"mint"),
            ("ROBOT_OK","ROBOT OK",0.6500f,-0.3550f,"mint"),
            ("IO_ROBOT","IO ROBOT",0.7250f,-0.3550f,"mint"),
            ("CASSETTE_RDY","CASSETTE RDY",0.8000f,-0.3550f,"mint"),
            ("IO_CASSETTE","IO CASSETTE",0.8750f,-0.3550f,"mint"),
            ("LOAD_REQ","LOAD REQ",0.9500f,-0.3550f,"mint"),
            ("LOAD_OP","LOAD OP",1.0250f,-0.3550f,"mint"),
            ("LIFT_SET","LIFT SET",-1.0000f,-0.5250f,"mint"),
            ("LIFT_CMD","LIFT CMD",-0.9285f,-0.5250f,"mint"),
            ("LIFT_POS","LIFT POS",-0.8570f,-0.5250f,"mint"),
            ("IO_LIFT","IO LIFT",-0.7855f,-0.5250f,"mint"),
            ("RPM_SET","RPM SET",-0.7140f,-0.5250f,"mint"),
            ("SPINDLE_CMD","SPIN CMD",-0.6425f,-0.5250f,"mint"),
            ("RPM_OUT","RPM OUT",-0.5710f,-0.5250f,"mint"),
            ("IO_RPM","IO RPM",-0.4995f,-0.5250f,"mint"),
            ("PLC_WET","PLC WET",-0.4280f,-0.5250f,"mint"),
            ("PREWET_RUN","PREWET RUN",-0.3565f,-0.5250f,"mint"),
            ("PREWET_DONE","PREWET DONE",-0.2850f,-0.5250f,"mint"),
            ("IO_WET","IO WET",-0.2135f,-0.5250f,"mint"),
            ("BUBBLE_READY","BUBBLE READY",-0.1420f,-0.5250f,"mint"),
            ("SEED_CHECK","SEED CHECK",-0.0705f,-0.5250f,"mint"),
            ("CHAMBER_CLOSE","LID CLOSE",0.0010f,-0.5250f,"mint"),
            ("CHAMBER_CLOSED","LID CLOSED",0.0725f,-0.5250f,"mint"),
            ("POT_I","POT I",0.1440f,-0.5250f,"mint"),
            ("POT_V","POT V",0.2155f,-0.5250f,"mint"),
            ("THICKNESS_OUT","THICK OUT",0.2870f,-0.5250f,"mint"),
            ("LOOP_VALID","LOOP VALID",0.3585f,-0.5250f,"mint"),
            ("TEST_OHM_OUT","TEST OHM OUT",0.4300f,-0.5250f,"mint"),
            ("REFERENCE_ELECTRODE_POTENTIAL_OUT","REF POT OUT",0.5015f,-0.5250f,"mint"),
            ("ELECTROCHEMICAL_CONTROLLER_REFERENCE_FEEDBACK_IN","REF FB IN",0.5730f,-0.5250f,"mint"),
            ("VOID_OUT","VOID OUT",0.6445f,-0.5250f,"mint"),
            ("FILL_OUT","FILL OUT",0.7160f,-0.5250f,"mint"),
            ("OVERBURDEN_OUT","OVERBD OUT",0.7875f,-0.5250f,"mint"),
            ("ROUGHNESS_OUT","ROUGH OUT",0.8590f,-0.5250f,"mint"),
            ("CONTACT_INTEGRITY","CT OK",0.9305f,-0.5250f,"mint"),
            ("SEED_OK","SEED OK",1.0020f,-0.5250f,"mint"),
            ("HOLO_FILL","HOLO FILL",-1.0000f,-0.7000f,"mint"),
            ("REFERENCE_VALID_STATUS","REF STATUS",-0.9285f,-0.7000f,"mint"),
            ("ESTOP_NC","ESTOP NC",-0.8570f,-0.7000f,"amber"),
            ("SAFE_STOP_IN","SAFE STOP IN",-0.7855f,-0.7000f,"amber"),
            ("LEAK_TRIP","LEAK TRIP",-0.7140f,-0.7000f,"amber"),
            ("RECT_DISABLE","RECT DISABLE",-0.6425f,-0.7000f,"mint"),
            ("SPINDLE_LOCK","SPIN LOCK",-0.5710f,-0.7000f,"mint"),
            ("LIFT_LOCK","LIFT LOCK",-0.4995f,-0.7000f,"mint"),
            ("PUMP_LOCK","PUMP LOCK",-0.4280f,-0.7000f,"mint"),
            ("POT_INHIBIT","POT INHIBIT",-0.3565f,-0.7000f,"mint"),
            ("OVER_I_TRIP","OVER I TRIP",-0.2850f,-0.7000f,"amber"),
            ("OVER_P_TRIP","OVER P TRIP",-0.2135f,-0.7000f,"amber"),
            ("OVER_T_TRIP","OVER T TRIP",-0.1420f,-0.7000f,"amber"),
            ("SAFETY_ALARM","SAFETY ALARM",-0.0705f,-0.7000f,"amber"),
            ("FAULT_LATCH","FAULT LATCH",0.0010f,-0.7000f,"amber"),
            ("ANALOG_SAMPLE","ANALOG SAMPLE",0.0725f,-0.7000f,"mint"),
            ("IO_ENABLE","IO ENABLE",0.1440f,-0.7000f,"mint"),
            ("WAVE_PHASE","WAVE PHASE",0.2155f,-0.7000f,"mint"),
            ("WAVE_START","WAVE START",0.2870f,-0.7000f,"mint"),
            ("OPERATOR_CMD","OPER CMD",0.3585f,-0.7000f,"mint"),
            ("PLC_STATUS","PLC STATUS",0.4300f,-0.7000f,"mint"),
            ("SCADA_REF","SCADA REF",0.5015f,-0.7000f,"mint"),
            ("SCADA_CURRENT","SCADA CURR",0.5730f,-0.7000f,"mint"),
            ("SCADA_FLOW","SCADA FLOW",0.6445f,-0.7000f,"mint"),
            ("SCADA_FILL","SCADA FILL",0.7160f,-0.7000f,"mint"),
            ("SCADA_VOID","SCADA VOID",0.7875f,-0.7000f,"mint"),
            ("OBJECTIVE_EVENT","OBJ EVENT",0.8590f,-0.7000f,"mint"),
            ("WAVE_TELEMETRY","WAVE TEL",0.9305f,-0.7000f,"mint"),
            ("CERT_RESULT","CERT RESULT",1.0020f,-0.7000f,"mint"),
        };

        public static IEnumerable<string> TerminalIds => Sockets.Select(socket => socket.id);

        private static readonly Dictionary<string, string> Palette = new()
        {
            {"red","EE485C"},{"blue","1711E2"},{"amber","FFA82B"},{"violet","A282FF"},{"plasma","A45DFF"},
            {"mint","34F6C5"},{"cyan","23DAFD"},{"void","07111C"},{"white","D4F5FF"},{"glass","498BA8"}
        };

        [MenuItem("Tools/Project Spark/Levels/Create Level 76")]
        public static void Install()
        {
            SparkLevel76Builder.Create();
            var root = AssetDatabase.LoadAssetAtPath<GameObject>(Prefab);
            if (root == null || root.GetComponent<SparkLevel76Process>().controllerDisplay == null) Fit();
            Register();
            ValidateAssets();
        }

        [MenuItem("Tools/Project Spark/Level 76/Validate Level Assets")]
        public static void ValidateAssets()
        {
            var root=AssetDatabase.LoadAssetAtPath<GameObject>(Prefab);
            var control=root != null ? root.GetComponent<SparkLevel76Process>() : null;
            var bindings=root != null ? root.GetComponent<SparkLevelSceneBindings>() : null;
            if (control==null || bindings==null || control.controllerDisplay==null || control.qualityDisplay==null || control.rig==null || control.rig.pivots.Length!=42 ||
                control.pins.Length!=170 || control.pins.Distinct().Count()!=170 || 
                !TerminalIds.SequenceEqual(control.pins.Select(pin=>pin.name)) ||
                control.pins.Any(pin=>!bindings.TryGetTerminal(pin.name,out var bound) || bound!=pin) ||
                SparkLevel76Process.Gates.Any(id=>!bindings.IsComponentIdDeclared(id)))
                throw new InvalidOperationException("Level 76 wiring, motion or binding references are incomplete.");
            Debug.Log("[LEVEL 76] Validated 170 distinct terminals, 110 player wires and 6 evaluator gates.",root);
        }

        private static void Register()
        {
            var scene = SceneManager.GetSceneByPath(ScenePath);
            bool opened = !scene.isLoaded;
            if (opened) scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
            bool dirty = scene.isDirty;
            var objects = scene.GetRootGameObjects();
            T Find<T>() where T : Component => objects.SelectMany(item => item.GetComponentsInChildren<T>(true)).Single();
            var level = AssetDatabase.LoadAssetAtPath<SparkLevelDefinition>(Folder + "Level_76.asset");
            var drawing = AssetDatabase.LoadAssetAtPath<SparkLevelDrawingAsset>(Folder + "Level_76_Drawing.asset");
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
            else if (dirty) Debug.Log("MainMenu had unsaved edits. Save the scene to persist Level 76's campaign and teaching entries.");
        }

        private static void Append(Object target, string field, Object value)
        {
            if (value == null) throw new InvalidOperationException("Missing Level 76 asset for " + field);
            var data = new SerializedObject(target);
            var list = data.FindProperty(field);
            for (int i = 0; i < list.arraySize; i++)
                if (list.GetArrayElementAtIndex(i).objectReferenceValue == value) return;
            list.arraySize++;
            list.GetArrayElementAtIndex(list.arraySize - 1).objectReferenceValue = value;
            data.ApplyModifiedProperties();
        }

        [MenuItem("Tools/Project Spark/Level 76/Fit Breadboard Assembly")]
        public static void Fit()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Exit Play Mode before fitting Level 76.");
            var models = Models.Select((name, index) =>
            {
                string path = Folder + $"Models/L76_{index:00}_{name}_Fitted.fbx";
                return AssetDatabase.LoadAssetAtPath<GameObject>(path) ??
                    throw new InvalidOperationException("Missing fitted kit module " + path);
            }).ToArray();
            if (AssetDatabase.LoadAssetAtPath<GameObject>(Prefab) == null)
                throw new InvalidOperationException("The Level 76 prefab is missing.");

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
                    throw new InvalidOperationException("Unity could not save the Level 76 layout.");
                AssetDatabase.SaveAssets();
                Debug.Log("Level 76 fitted to the MainMenu breadboard with 170 seated terminals.");
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
            var bench = root.transform.Find("Plating Workbench");
            var bindings = root.GetComponent<SparkLevelSceneBindings>();
            var loop = root.GetComponent<SparkLevel76Process>();
            if (bench == null || bindings == null || loop == null)
                throw new InvalidOperationException("Level 76 is missing its workbench, controls, or scene bindings.");

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

            foreach (string name in new[] {"L76_HD_VISUAL_ART", "Breadboard Labels", "Motion pivots"})
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
            var assembly = Child(bench, "L76_HD_VISUAL_ART", new Vector3(0,floor,-.010f));
            foreach (var model in models)
            {
                var instance = Object.Instantiate(model, assembly, false).transform;
                var markers = instance.GetComponentsInChildren<Transform>(true)
                    .Where(item => item.name.StartsWith("L76_Board",StringComparison.Ordinal))
                    .ToDictionary(item => item.name);
                Transform Marker(string name) => markers["L76_Board" + name];
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
                if(key=="tint1D303F" && (mesh.name.StartsWith("L76_TransparentElectroplatingSafetyEnclosure_Static__",StringComparison.Ordinal) ||
                    mesh.name.StartsWith("L76_PrecisionCopperPlatingCell_ANIM_PLATING_CHAMBER_LID__",StringComparison.Ordinal))) key="glass";
                var material = MaterialFor(shader, key);
                // The fitted meshes can contain several imported submeshes.
                mesh.sharedMaterials = Enumerable.Repeat(material, mesh.GetComponent<MeshFilter>().sharedMesh.subMeshCount).ToArray();
                if (key is "glass" or "acrylic") mesh.shadowCastingMode = ShadowCastingMode.Off;
            }

            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (loop.calibrationText == null)
            {
                var oldControls = root.transform.Find("Level 76 Controls");
                if (oldControls != null) Object.DestroyImmediate(oldControls.gameObject);
                SparkLevel76Builder.BuildControls(root.transform,loop,font);
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
                terminalData.FindProperty("maxConnections").intValue = SparkLevel76Process.Wires.Count(wire => wire.a == index || wire.b == index);
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






                ("BATH / FLOW",-.865f,.147f),("WAFER / METROLOGY",.862f,.147f),
                ("RECTIFIER / ZONES",-.86f,-.235f),("SAFETY / CONTROL",.86f,-.235f)
            };
            foreach(var title in titles)
                Label(labels,font,title.title,title.title,new Vector3(title.x,floor+.058f,title.z),.0165f);
            Label(labels,font,"Cell label","COPPER CELL",new Vector3(-.022f,floor+.30f,.095f),.012f);
            Label(labels,font,"Rinse label","RINSE / INSPECT",new Vector3(.35f,floor+.23f,-.075f),.012f);
            Transform Pivot(string module,string key)
            {
                var name="fitted_"+module+"_"+key;
                return assembly.GetComponentsInChildren<Transform>(true).Single(item=>item.name==name);
            }
            loop.rig.art=assembly;
            loop.rig.pivots=new[]{
                Pivot("PrecisionCopperPlatingCell","ANIM_PLATING_CHAMBER_LID"),
                Pivot("TransparentElectrolyteBasin","ANIM_ELECTROLYTE_SURFACE"),
                Pivot("HolographicTrenchViaCrossSection","ANIM_TRENCH_FILL_HOLOGRAM"),
                Pivot("PrecisionWaferCathodeChuck","ANIM_WAFER_CLAMP"),
                Pivot("CathodeRotarySpindle","ANIM_WAFER_CATHODE_ROTATION"),
                Pivot("CathodeVerticalLiftAssembly","ANIM_WAFER_CATHODE_Z"),
                Pivot("AutomatedWaferTransferRobot","ANIM_WAFER_ROBOT_BASE"),
                Pivot("AutomatedWaferTransferRobot","ANIM_WAFER_ROBOT_ARM"),
                Pivot("AutomatedWaferTransferRobot","ANIM_WAFER_ROBOT_TOOL"),
                Pivot("PrewetWaferPreparationChamber","ANIM_PREWET_CHAMBER"),
                Pivot("ElectrolyteDegassingModule","ANIM_DEGASSING_MODULE"),
                Pivot("ElectrolyteMixingVessel","ANIM_ELECTROLYTE_MIXER"),
                Pivot("MicroFiltrationSkid","ANIM_FILTRATION_PUMP"),
                Pivot("PrecisionRecirculationPump","ANIM_RECIRCULATION_PUMP"),
                Pivot("ElectrolyteMassFlowController","ANIM_FLOW_CONTROLLER"),
                Pivot("AnnularFlowDistributionManifold","ANIM_DISTRIBUTION_MANIFOLD"),
                Pivot("MultiZoneElectrolyteNozzleArray","ANIM_FLOW_NOZZLE_ARRAY"),
                Pivot("SegmentedCopperAnodeAssembly","ANIM_ANODE_SEGMENT_A"),
                Pivot("SegmentedCopperAnodeAssembly","ANIM_ANODE_SEGMENT_B"),
                Pivot("ProtectedAnodeBasket","ANIM_ANODE_SHIELD"),
                Pivot("EdgeCurrentDistributionRing","ANIM_CURRENT_DISTRIBUTION_RING"),
                Pivot("PulsedCurrentPlatingRectifier","ANIM_RECTIFIER_COOLING_FAN"),
                Pivot("ReferenceElectrodeProbe","ANIM_REFERENCE_ELECTRODE"),
                Pivot("ElectrochemicalPotentialController","ANIM_POTENTIAL_CONTROLLER_DISPLAY"),
                Pivot("ElectrolyteThermalConditioner","ANIM_BATH_THERMAL_FAN"),
                Pivot("VirtualSuppressorCartridge","ANIM_SUPPRESSOR_METERING"),
                Pivot("VirtualAcceleratorCartridge","ANIM_ACCELERATOR_METERING"),
                Pivot("VirtualLevelerCartridge","ANIM_LEVELER_METERING"),
                Pivot("ElectrolyteBubbleDetectionSensor","ANIM_BUBBLE_SENSOR"),
                Pivot("FourPointInterconnectTestProbe","ANIM_FOUR_POINT_PROBE"),
                Pivot("OpticalCopperThicknessScanner","ANIM_COPPER_THICKNESS_SCANNER"),
                Pivot("ThreeDVoidInspectionScanner","ANIM_VOID_INSPECTION_HEAD"),
                Pivot("TrenchFillTomographyMonitor","ANIM_TRENCH_FILL_TOMOGRAPHY"),
                Pivot("CopperOverburdenAnalyzer","ANIM_OVERBURDEN_SCANNER"),
                Pivot("CopperSurfaceRoughnessScanner","ANIM_SURFACE_ROUGHNESS_SCANNER"),
                Pivot("PostPlatingWaferRinseChamber","ANIM_RINSE_CHAMBER"),
                Pivot("WaferSpinDryer","ANIM_SPIN_DRYER"),
                Pivot("ElectroplatingEmergencySafetyStation","ANIM_ESTOP"),
                Pivot("CopperElectroplatingPLCRack","ANIM_PLC_STATUS_DISPLAY"),
                Pivot("PlatingWaveformProcessController","ANIM_WAVEFORM_DISPLAY"),
                Pivot("HolographicInterconnectSCADA","ANIM_INTERCONNECT_SCADA"),
                Pivot("SeedBarrierContinuityScanner","ANIM_SEED_CONTINUITY_SCANNER"),
            };
            var pivots=loop.rig.pivots;
            pivots[7].SetParent(pivots[6],true);
            pivots[8].SetParent(pivots[7],true);
            loop.rig.fillSample=pivots[2];
            if(loop.plot.transform.Find("Trend legend") == null)
            {
                var oldPanel = root.transform.Find("Level 76 Controls");
                if (oldPanel != null) Object.DestroyImmediate(oldPanel.gameObject);
                SparkLevel76Builder.BuildControls(root.transform,loop,font);
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
            var material = existing != null ? existing : new Material(shader) { name = "L76 " + key };
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


