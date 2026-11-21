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
    public static partial class SparkLevel78ModelInstaller
    {
        public const string Folder = "Assets/My_Assets/_Project_Spark/Prefabs/Levels/Level_78_Layout/";
        public const string Prefab = Folder + "Level_78_Layout.prefab";
        public const string ScenePath = "Assets/My_Assets/_Project_Spark/Scenes/MainMenu/MainMenu.unity";
        private static readonly string[] Models = {
            "IndustrialDielectricProtectionBase","TransparentMultichamberSafetyEnclosure","InterconnectWaferCassette","PlanarizedCopperInterconnectWafer","CopperInterconnectLayerHologram","HighVacuumWaferLoadLock","PrecisionVacuumGateValve","CentralVacuumTransferHub","MultiAxisVacuumTransferRobot","OpticalWaferAlignmentStage","InterconnectSurfaceConditionScanner","CopperSurfacePreconditioningModule","CopperCapPECVDReactor","CopperCapReactorLid","CeramicCapGasShowerhead","CapPlasmaRFSourceAssembly","CapRFMatchingNetwork","CapElectrodeDistributionAssembly","CapHeatedWaferSusceptor","CapVacuumExhaustThrottle","CapPlasmaSheathVisualization","VirtualCapPrecursorCartridge","CapPrecursorDeliveryManifold","CapGasMassFlowController","CapFilmThicknessInterferometer","CapDepositionUniformityMonitor","CapContinuityInspectionModule","HolographicCopperDiffusionShield","LowKPECVDReactor","LowKDielectricGasShowerhead","LowKPlasmaSource","LowKRFMatchingAssembly","LowKHeatedWaferChuck","VirtualLowKPrecursorCartridge","VirtualPorogenCartridge","DualChannelLowKGasManifold","LowKGasFlowController","PlasmaIonEnergyMonitor","LowKSurfaceChargingSensor","LowKFTIRBondingAnalyzer","LowKFilmGrowthOpticalMonitor","UVThermalCureReactor","MultiZoneUVLampArray","PrecisionUVCureShutter","RotatingCureWaferPlaten","CureWaferTemperatureSensor","CureOutgassingMonitor","HolographicLowKPoreNetwork","WaferStressBowScanner","DielectricEllipsometryStation","InterlineCombTestFixture","DielectricLeakageCurrentElectrometer","AcceleratedBiasStressTestSource","DielectricReliabilityController","HolographicTDDBReliabilityGraph","SurfaceMoistureInspectionAnalyzer","InterconnectRCDelayAnalyzer","EmergencyIsolationSafetyStation","TurbomolecularVacuumPump","VacuumBackingPump","PrecisionChamberPressureGauge","DielectricProcessPLCRack","HolographicDielectricSCADA","Regulated5VControlSupply"
        };
        private const string Materials = Folder + "Models/Materials";
        internal const string FixtureName = "L78_HD_VISUAL_ART";
        private const string MountsName = "Equipment commissioning";
        private const string BaseModule = "L78_IndustrialDielectricProtectionBase";

        private static readonly (string id, string label, string color)[] Sockets =
        {
            ("SUPPLY_POS","SUP POS","red"),
            ("PLC_5V","PLC 5V","red"),
            ("SUPPLY_RET","SUP RET","blue"),
            ("PLC_GND","PLC GND","blue"),
            ("SCADA_5V","SCADA 5V","red"),
            ("SCADA_GND","SCADA GND","blue"),
            ("GAS_5V","GAS 5V","red"),
            ("GAS_GND","GAS GND","blue"),
            ("METER_5V","METER 5V","red"),
            ("METER_GND","METER GND","blue"),
            ("SAFE_5V","SAFE 5V","red"),
            ("SAFE_GND","SAFE GND","blue"),
            ("BACK_CMD","BACK CMD","mint"),
            ("BACK_RUN","BACK RUN","mint"),
            ("TURBO_CMD","TURBO CMD","mint"),
            ("TURBO_RUN","TURBO RUN","mint"),
            ("BACK_READY","BACK READY","mint"),
            ("VAC_BACK_OK","VAC BACK OK","mint"),
            ("VAC_DIAG","VAC DIAG","mint"),
            ("PRESSURE_OUT","PRESS OUT","mint"),
            ("VAC_PRESSURE","VAC PRESS","mint"),
            ("GATE_CMD","GATE CMD","mint"),
            ("GATE_DRIVE","GATE DRIVE","mint"),
            ("GATE_CLOSED","GATE CLOSED","mint"),
            ("VAC_GATE_OK","VAC GATE OK","mint"),
            ("LOADLOCK_CMD","LOADLOCK CMD","mint"),
            ("LOCK_PUMP","LOCK PUMP","mint"),
            ("LOCK_PRESSURE","LOCK PRESS","mint"),
            ("VAC_LOCK_OK","VAC LOCK OK","mint"),
            ("ROBOT_CMD","ROBOT CMD","mint"),
            ("ROBOT_DRIVE","ROBOT DRIVE","mint"),
            ("ROBOT_HOME","ROBOT HOME","mint"),
            ("TRANSFER_HOME","TRANSFER HOME","mint"),
            ("ALIGN_CMD","ALIGN CMD","mint"),
            ("ALIGN_START","ALIGN START","mint"),
            ("ALIGN_OUT","ALIGN OUT","mint"),
            ("TRANSFER_ALIGN","TRANSFER ALIGN","mint"),
            ("PREP_CMD","PREP CMD","mint"),
            ("PREP_START","PREP START","mint"),
            ("PREP_OUT","PREP OUT","mint"),
            ("TRANSFER_SURFACE","TRANSFER SURFACE","mint"),
            ("PREP_DONE","PREP DONE","mint"),
            ("PLC_PREP_OK","PLC PREP OK","mint"),
            ("VAC_PERMIT","VAC PERMIT","amber"),
            ("VAC_AUTH","VAC AUTH","mint"),
            ("HUB_SAFE","HUB SAFE","amber"),
            ("TRANSFER_SAFE","TRANSFER SAFE","amber"),
            ("CAP_CMD","CAP CMD","mint"),
            ("CAP_RUN","CAP RUN","mint"),
            ("CAP_LID_CMD","CAP LID CMD","mint"),
            ("CAP_LID","CAP LID","mint"),
            ("CAP_LID_OUT","CAP LID OUT","mint"),
            ("CAP_INTERLOCK","CAP INTERLOCK","mint"),
            ("CAP_FLOW_CMD","CAP FLOW CMD","mint"),
            ("CAP_MFC_SET","CAP MFC SET","mint"),
            ("CAP_FLOW_OUT","CAP FLOW OUT","mint"),
            ("CAP_FLOW_IN","CAP FLOW IN","mint"),
            ("CAP_RESERVOIR","CAP RESERVOIR","mint"),
            ("OUT_CAP_FUEL","OUT CAP FUEL","mint"),
            ("CAP_MANIFOLD","CAP MANIFOLD","mint"),
            ("CAP_DELIVERY","CAP DELIVERY","mint"),
            ("CAP_RF_CMD","CAP RF CMD","mint"),
            ("CAP_RF_RUN","CAP RF RUN","mint"),
            ("CAP_RF_OUT","CAP RF OUT","mint"),
            ("CAP_MATCH_IN","CAP MATCH IN","mint"),
            ("CAP_MATCH_OUT","CAP MATCH OUT","mint"),
            ("CAP_RF_OK","CAP RF OK","mint"),
            ("CAP_TEMP_CMD","CAP TEMP CMD","mint"),
            ("CAP_HEAT","CAP HEAT","mint"),
            ("CAP_TEMP_OUT","CAP TEMP OUT","mint"),
            ("CAP_HEAT_OK","CAP HEAT OK","mint"),
            ("CAP_THICK_OUT","CAP THICK OUT","mint"),
            ("CAP_THICK_IN","CAP THICK IN","mint"),
            ("CAP_MAP_OUT","CAP MAP OUT","mint"),
            ("CAP_UNIFORM","CAP UNIFORM","mint"),
            ("CAP_PINHOLE_OUT","CAP PINHOLE OUT","mint"),
            ("CAP_DEFECTS","CAP DEFECTS","mint"),
            ("CAP_EXHAUST_CMD","CAP EXHAUST CMD","mint"),
            ("CAP_THROTTLE","CAP THROTTLE","mint"),
            ("LOWK_CMD","LOWK CMD","mint"),
            ("LOWK_RUN","LOWK RUN","mint"),
            ("SHOWER_CMD","SHOWER CMD","mint"),
            ("LOWK_SHOWER","LOWK SHOWER","mint"),
            ("LOWK_RF_CMD","LOWK RF CMD","mint"),
            ("LOWK_RF_RUN","LOWK RF RUN","mint"),
            ("LOWK_RF_OUT","LOWK RF OUT","mint"),
            ("LOWK_MATCH_IN","LOWK MATCH IN","mint"),
            ("LOWK_MATCH_OUT","LOWK MATCH OUT","mint"),
            ("LOWK_RF_OK","LOWK RF OK","mint"),
            ("LOWK_TEMP_CMD","LOWK TEMP CMD","mint"),
            ("LOWK_HEAT","LOWK HEAT","mint"),
            ("LOWK_TEMP_OUT","LOWK TEMP OUT","mint"),
            ("LOWK_HEAT_OK","LOWK HEAT OK","mint"),
            ("MATRIX_READY","MATRIX READY","mint"),
            ("LOWK_MATRIX","LOWK MATRIX","mint"),
            ("POROGEN_READY","POROGEN READY","mint"),
            ("LOWK_POROGEN","LOWK POROGEN","mint"),
            ("DUAL_MFC_CMD","DUAL MFC CMD","mint"),
            ("GAS_MIXER","GAS MIXER","mint"),
            ("MIX_FLOW_CMD","MIX FLOW CMD","mint"),
            ("LOWK_MIX_REQ","LOWK MIX REQ","mint"),
            ("LOWK_FLOW_OUT","LOWK FLOW OUT","mint"),
            ("LOWK_FLOW_IN","LOWK FLOW IN","mint"),
            ("ION_ENERGY_OUT","ION ENERGY OUT","mint"),
            ("DAMAGE_IN","DAMAGE IN","mint"),
            ("CHARGE_OUT","CHARGE OUT","mint"),
            ("CHARGE_IN","CHARGE IN","mint"),
            ("FTIR_OUT","FTIR OUT","mint"),
            ("BOND_IN","BOND IN","mint"),
            ("LOWK_THICK_OUT","LOWK THICK OUT","mint"),
            ("GROWTH_IN","GROWTH IN","mint"),
            ("CURE_CMD","CURE CMD","mint"),
            ("CURE_RUN","CURE RUN","mint"),
            ("CURE_DOOR_OUT","CURE DOOR OUT","mint"),
            ("CURE_SAFE","CURE SAFE","amber"),
            ("LAMP_CMD","LAMP CMD","mint"),
            ("UV_ENABLE","UV ENABLE","mint"),
            ("LAMP_OUT","LAMP OUT","mint"),
            ("UV_ACTUAL","UV ACTUAL","mint"),
            ("SHUTTER_CMD","SHUTTER CMD","mint"),
            ("SHUTTER_SET","SHUTTER SET","mint"),
            ("SHUTTER_OUT","SHUTTER OUT","mint"),
            ("SHUTTER_POS","SHUTTER POS","mint"),
            ("PLATEN_CMD","PLATEN CMD","mint"),
            ("CURE_SPIN","CURE SPIN","mint"),
            ("PLATEN_OUT","PLATEN OUT","mint"),
            ("CURE_RPM","CURE RPM","mint"),
            ("CURE_TEMP_CMD","CURE TEMP CMD","mint"),
            ("CURE_HEAT","CURE HEAT","mint"),
            ("CURE_TEMP_OUT","CURE TEMP OUT","mint"),
            ("CURE_TEMP","CURE TEMP","mint"),
            ("OUTGAS_IN","OUTGAS IN","mint"),
            ("PORE_STATUS","PORE STATUS","mint"),
            ("CURE_DONE","CURE DONE","mint"),
            ("UV_SAFE","UV SAFE","amber"),
            ("ELLIPSOMETRY","ELLIPSOMETRY","mint"),
            ("FILM_BOW","FILM BOW","mint"),
            ("MOISTURE_IN","MOISTURE IN","mint"),
            ("BOND_QUALITY","BOND QUALITY","mint"),
            ("RC_IN","RC IN","mint"),
            ("COMB_READY","COMB READY","mint"),
            ("BIAS_STATUS","BIAS STATUS","mint"),
            ("STRESS_CMD","STRESS CMD","mint"),
            ("DIELECTRIC_LEAKAGE_ELECTROMETER_OUT","LEAK METER OUT","amber"),
            ("DIELECTRIC_RELIABILITY_CONTROLLER_LEAKAGE_IN","REL LEAK IN","amber"),
            ("METER_HEALTH","METER HEALTH","mint"),
            ("TEST_TELEM","TEST TELEM","mint"),
            ("TDDB_GRAPH","TDDB GRAPH","mint"),
            ("FILM_TELEM","FILM TELEM","mint"),
            ("STRESS_PERMIT","STRESS PERMIT","amber"),
            ("STRESS_ENABLE","STRESS ENABLE","mint"),
            ("INCOMING_CHECK","INCOMING CHECK","mint"),
            ("PINHOLE_WARN","PINHOLE WARN","mint"),
            ("COVERAGE_OK","COVERAGE OK","mint"),
            ("DAMAGE_STATUS","DAMAGE STATUS","mint"),
            ("CHARGE_WARN","CHARGE WARN","mint"),
            ("LOWK_CERT","LOWK CERT","mint"),
            ("TEMP_HISTORY","TEMP HISTORY","mint"),
            ("OUTGAS_OK","OUTGAS OK","mint"),
            ("STRESS_HEALTH","STRESS HEALTH","mint"),
            ("MOISTURE_OK","MOISTURE OK","mint"),
            ("RC_RESULT","RC RESULT","mint"),
            ("METER_CAL_OK","METER CAL OK","mint"),
            ("COMB_ISOLATION","COMB ISOLATION","mint"),
            ("RELIABILITY_OK","REL OK","mint"),
            ("ESTOP_NC","ESTOP NC","amber"),
            ("SAFE_STOP_IN","SAFE STOP IN","amber"),
            ("SAFETY_HEARTBEAT","SAFETY HEARTBEAT","amber"),
            ("CAP_RF_OFF","CAP RF OFF","mint"),
            ("LOWK_RF_OFF","LOWK RF OFF","mint"),
            ("UV_OFF","UV OFF","mint"),
            ("STRESS_OFF","STRESS OFF","mint"),
            ("FAULT_LATCH","FAULT LATCH","amber"),
            ("PLC_STATE","PLC STATE","mint"),
            ("OPERATOR_CMD","OPERATOR CMD","mint"),
            ("RELIABILITY_REQ","REL REQ","mint"),
            ("LEAK_VALID","LEAK VALID","amber"),
            ("LEAK_LOSS_TRIP","LEAK LOSS TRIP","amber"),
            ("TRANSFER_HOLD","TRANSFER HOLD","mint"),
            ("OBJECTIVE_EVENT","OBJECTIVE EVENT","mint"),
            ("SCADA_CERT","SCADA CERT","mint"),
            ("SCADA_ALARM","SCADA ALARM","mint"),
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
            if (root == null || root.GetComponent<SparkEquipmentWorkbenchView>() == null) Fit();
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
                root.GetComponentsInChildren<SparkTerminal>(true).Length!=182 ||
                control.pinLabels.Length!=182 || control.pinLabels.Any(tag=>!tag) ||
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

        [MenuItem("Tools/Project Spark/Level 78/Fit Equipment Workbench")]
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
                var table = SparkEquipmentWorkbenchFit.FindTable(scene);
                root = PrefabUtility.LoadPrefabContents(Prefab);
                Fit(root, models, table, layoutRoot != null ? layoutRoot.localToWorldMatrix : Matrix4x4.identity);
                if (PrefabUtility.SaveAsPrefabAsset(root, Prefab) == null)
                    throw new InvalidOperationException("Unity could not save the Level 78 layout.");
                AssetDatabase.SaveAssets();
                Debug.Log("Level 78 fitted to the workshop table with 182 equipment-mounted terminals.");
            }
            finally
            {
                if (root != null) PrefabUtility.UnloadPrefabContents(root);
                if (opened) EditorSceneManager.CloseScene(scene, true);
            }
        }

        private static void Fit(GameObject root, GameObject[] models, BoxCollider table, Matrix4x4 layout)
        {
            var bench = root.transform.Find("Dielectric Workbench");
            var bindings = root.GetComponent<SparkLevelSceneBindings>();
            var loop = root.GetComponent<SparkLevel78Process>();
            if (bench == null || bindings == null || loop == null)
                throw new InvalidOperationException("Level 78 is missing its workbench, controls, or scene bindings.");

            foreach (var terminal in loop.pins)
                terminal.transform.SetParent(terminal.Owner.transform, true);

            Matrix4x4 parentWorld = layout * bench.parent.localToWorldMatrix;
            Vector3 tableSize = Vector3.Scale(table.size, table.transform.lossyScale);
            Vector3 tableTop = table.transform.TransformPoint(table.center + Vector3.up * table.size.y * .5f);
            bench.localPosition = parentWorld.inverse.MultiplyPoint3x4(tableTop + table.transform.up * .025f);
            bench.localRotation = Quaternion.Inverse(parentWorld.rotation) * table.transform.rotation;
            float worldScale = Mathf.Min(tableSize.x / 2.55f, tableSize.z / 1.54f);
            bench.localScale = Vector3.one * (worldScale / parentWorld.MultiplyVector(Vector3.right).magnitude);

            foreach (string name in new[] { FixtureName, MountsName, "Breadboard Labels", "Motion pivots", "Temporary board" })
            {
                var existing = bench.Find(name);
                if (existing) Object.DestroyImmediate(existing.gameObject);
            }
            foreach (var renderer in bench.GetComponentsInChildren<MeshRenderer>(true))
                if (renderer.name=="Body") renderer.gameObject.SetActive(false);
            var assembly = Child(bench, FixtureName, Vector3.zero);
            for (int i = 0; i < models.Length; i++)
            {
                var model = models[i];
                var instance = Object.Instantiate(model, assembly, false).transform;
                instance.name = "L78_" + Models[i];
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

            var artBounds = SparkEquipmentWorkbenchFit.MeasureArt(assembly, bench.worldToLocalMatrix);
            var baseBounds = SparkEquipmentWorkbenchFit.MeasureArt(assembly.Find(BaseModule), bench.worldToLocalMatrix);
            float fit = Mathf.Min(tableSize.x * .91f / (artBounds.size.x * worldScale),
                tableSize.z * .89f / (artBounds.size.z * worldScale));
            assembly.localScale = Vector3.one * fit;
            assembly.localPosition = new Vector3(-artBounds.center.x, -baseBounds.min.y, -artBounds.center.z) * fit;
            var view = root.GetComponent<SparkEquipmentWorkbenchView>() ?? root.AddComponent<SparkEquipmentWorkbenchView>();
            view.Bench = bench;
            view.FramingBounds = SparkEquipmentWorkbenchFit.MeasureArt(assembly, bench.worldToLocalMatrix);
            if (!AssetDatabase.IsValidFolder(Materials)) AssetDatabase.CreateFolder(Folder + "Models", "Materials");
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) throw new InvalidOperationException("The project URP Lit shader is missing.");
            foreach (var mesh in assembly.GetComponentsInChildren<MeshRenderer>())
            {
                string key = mesh.name.Substring(mesh.name.LastIndexOf("__", StringComparison.Ordinal) + 2);
                if(key=="tint1D303F" && mesh.name.StartsWith("L78_TransparentMultichamberSafetyEnclosure_",StringComparison.Ordinal)) key="glass";
                var material = MaterialFor(shader, key, SurfaceFor(mesh.name, key));
                // The fitted meshes can contain several imported submeshes.
                mesh.sharedMaterials = Enumerable.Repeat(material, mesh.GetComponent<MeshFilter>().sharedMesh.subMeshCount).ToArray();
                if (key is "glass" or "acrylic") mesh.shadowCastingMode = ShadowCastingMode.Off;
            }

            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (!loop.focusWireButton || !loop.plot || !loop.plot.GetComponent<CanvasRenderer>() ||
                loop.plot.rectTransform.anchorMax != Vector2.one)
            {
                var oldControls = root.transform.Find("Level 78 Controls");
                if (oldControls) Object.DestroyImmediate(oldControls.gameObject);
                SparkLevel78Builder.BuildControls(root.transform, loop, font);
            }
            var labels = Child(bench, MountsName, Vector3.zero);
            using (new AssetDatabase.AssetEditingScope())
                ReplaceBakedContactBanks(assembly);
            SeatOriginalTerminals(bench, assembly, bindings, loop, font, shader);

            var deck = SparkEquipmentWorkbenchFit.MeasureArt(assembly.Find(BaseModule), bench.worldToLocalMatrix);
            float labelHeight = deck.max.y + .016f;
            float labelDepth = deck.min.z + .043f;
            var title = Label(labels, font, "Level name", "SEAL THE SIGNAL",
                new Vector3(0, labelHeight, labelDepth), .023f);
            title.color = new Color(.51f, .93f, 1f);
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
            Lamp("Cluster work light",new Vector3(0,.58f,-.14f),new Color(.8f,.90f,1),2.65f,4.9f);
            Lamp("Metrology fill",new Vector3(.54f,.34f,.16f),new Color(.44f,.80f,1),1.9f,3.9f);
            loop.rig.BindEffects(
                Lamp("Cap glow",new Vector3(-.37f,.20f,-.03f),new Color(.29f,.75f,1),1,.45f),
                Lamp("Low-k glow",new Vector3(0,.20f,-.03f),new Color(.48f,.39f,1),1,.46f),
                Lamp("UV cure glow",new Vector3(.28f,.20f,-.03f),new Color(.58f,.37f,1),1,.45f),
                Lamp("Isolation alarm",new Vector3(.56f,.24f,.03f),new Color(1,.18f,.12f),1.25f,.55f));
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
            loop.controllerDisplay=Readout("Electrometer readout",new Vector3(-.83f,labelHeight,labelDepth),new Color(.46f,.94f,1f));
            loop.qualityDisplay=Readout("Wafer status",new Vector3(.83f,labelHeight,labelDepth),new Color(.71f,1f,.86f));
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

        private static Material MaterialFor(Shader shader, string key,string surface=null)
        {
            string materialName=surface==null?"L78 "+key:"L78 "+key+" "+surface;
            string path = $"{Materials}/{materialName}.mat";
            if (key.StartsWith("tint",StringComparison.Ordinal) && key.Length==10) Palette[key]=key.Substring(4);
            var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing != null && surface == null && key is not ("glass" or "acrylic") &&
                ColorUtility.TryParseHtmlString("#" + (Palette.TryGetValue(key, out var savedTint) ? savedTint : "9FB3C8"), out var savedColor) &&
                existing.GetColor("_BaseColor") == savedColor) return existing;
            ColorUtility.TryParseHtmlString("#" + (Palette.TryGetValue(key, out var tint) ? tint : "9FB3C8"), out var color);
            var material = existing != null ? existing : new Material(shader) { name = materialName };
            bool metal = key is "chrome" or "titanium" or "steel" or "gold" or "copper" or "aluminum" or "tintCFDCE2" or "tintA4B8C4" or "tintE36C27" or "tintDC9C2F" or "tintDC803E" or "tintE9BC56" or "tint47657C" or "tintC2D0D6" or "tint97B3C4";
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
                string prefix=SurfaceMaps;
                var albedo=AssetDatabase.LoadAssetAtPath<Texture2D>(prefix+"Authored/"+surface+"_Albedo.png");
                var normal=AssetDatabase.LoadAssetAtPath<Texture2D>(prefix+"Authored/"+surface+"_Normal.png");
                var packed=AssetDatabase.LoadAssetAtPath<Texture2D>(prefix+surface+"_MetalSmoothAO.png");
                if(albedo==null||normal==null||packed==null)
                    throw new InvalidOperationException("Missing Level 78 surface maps for "+surface);
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
