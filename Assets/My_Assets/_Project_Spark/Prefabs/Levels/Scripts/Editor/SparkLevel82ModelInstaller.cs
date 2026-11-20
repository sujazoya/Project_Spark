#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
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
        private const float SourceArtUnit = .096f;

        private static readonly (string id, string label, string color)[] Sockets =
        {
            ("SUPPLY_POS","SUP POS","red"),
            ("PLC_5V","PLC 5V","red"),
            ("SUPPLY_RET","SUP RET","blue"),
            ("PLC_GND","PLC GND","blue"),
            ("SCAN_5V","SCAN 5V","red"),
            ("SCAN_GND","SCAN GND","blue"),
            ("EMI_5V","EMI 5V","red"),
            ("EMI_GND","EMI GND","blue"),
            ("FIELD_5V","FIELD 5V","red"),
            ("FIELD_GND","FIELD GND","blue"),
            ("SAFE_5V","SAFE 5V","red"),
            ("SAFE_GND","SAFE GND","blue"),
            ("AZ_CMD","AZ CMD","mint"),
            ("TABLE_SET","TABLE SET","mint"),
            ("AZ_ENCODER","AZ ENCODER","mint"),
            ("AZ_RAW","AZ RAW","mint"),
            ("AZ_ACTUAL","AZ ACTUAL","mint"),
            ("GEAR_CMD","GEAR CMD","mint"),
            ("TABLE_DRIVE","TABLE DRIVE","mint"),
            ("GEAR_READY","GEAR READY","mint"),
            ("GEAR_OK","GEAR OK","mint"),
            ("MAST_CMD","MAST CMD","mint"),
            ("HEIGHT_SET","HEIGHT SET","mint"),
            ("MAST_ENCODER","MAST ENCODER","mint"),
            ("HEIGHT_ACTUAL","HEIGHT ACTUAL","mint"),
            ("POL_CMD","POL CMD","mint"),
            ("POL_SET","POL SET","mint"),
            ("POL_ENCODER","POL ENCODER","mint"),
            ("POL_ACTUAL","POL ACTUAL","mint"),
            ("ANT_READY","ANT READY","mint"),
            ("ANT_CHECK","ANT CHECK","mint"),
            ("ANT_MEASURE","ANT MEASURE","mint"),
            ("ANT_STATUS","ANT STATUS","mint"),
            ("XY_CMD","XY CMD","mint"),
            ("GANTRY_XY","GANTRY XY","mint"),
            ("XY_ENCODER","XY ENCODER","mint"),
            ("XY_ACTUAL","XY ACTUAL","mint"),
            ("Z_CMD","Z CMD","mint"),
            ("GANTRY_Z","GANTRY Z","mint"),
            ("Z_ENCODER","Z ENCODER","mint"),
            ("Z_ACTUAL","Z ACTUAL","mint"),
            ("MAG_CMD","MAG CMD","mint"),
            ("H_PROBE","H PROBE","mint"),
            ("ELEC_CMD","ELEC CMD","mint"),
            ("E_PROBE","E PROBE","mint"),
            ("AZ_HOME","AZ HOME","mint"),
            ("HOME_SET","HOME SET","mint"),
            ("LISN_CMD","LISN CMD","mint"),
            ("LISN_ENABLE","LISN ENABLE","mint"),
            ("LISN_OUT","LISN OUT","mint"),
            ("CONDUCTED_IN","COND IN","mint"),
            ("CONDSPEC_OUT","CONDSPEC OUT","mint"),
            ("EMI_COND","EMI COND","mint"),
            ("CM_CLAMP","CM CLAMP","mint"),
            ("CM_I","CM I","mint"),
            ("DM_CLAMP","DM CLAMP","mint"),
            ("DM_I","DM I","mint"),
            ("CM_CAL","CM CAL","mint"),
            ("CM_ZERO","CM ZERO","mint"),
            ("DM_CAL","DM CAL","mint"),
            ("DM_ZERO","DM ZERO","mint"),
            ("BOND_MON","BOND MON","mint"),
            ("CHASSIS_OK","CHASSIS OK","mint"),
            ("TERM_OUT","TERM OUT","mint"),
            ("SHIELD_TERM","SHIELD TERM","mint"),
            ("HARNESS_CM","HARNESS CM","mint"),
            ("CM_MON","CM MON","mint"),
            ("HARNESS_DM","HARNESS DM","mint"),
            ("DM_MON","DM MON","mint"),
            ("CHOKE_OUT","CHOKE OUT","mint"),
            ("CHOKE_ATTEN","CHOKE ATTEN","mint"),
            ("LC_OUT","LC OUT","mint"),
            ("LC_ATTEN","LC ATTEN","mint"),
            ("FEED_OUT","FEED OUT","mint"),
            ("DC_PATH","DC PATH","mint"),
            ("SWITCH_OUT","SWITCH OUT","mint"),
            ("RIPPLE_IN","RIPPLE IN","mint"),
            ("COND_SCAN","COND SCAN","mint"),
            ("COND_MARGIN","COND MARGIN","mint"),
            ("BW_CMD","BW CMD","mint"),
            ("EMI_BW","EMI BW","mint"),
            ("SPEC_REF","SPEC REF","mint"),
            ("EMI_CAL","EMI CAL","mint"),
            ("SCADA_COND","SCADA COND","mint"),
            ("CONDUCT_CMD","CONDUCT CMD","mint"),
            ("CONDUCT_RUN","CONDUCT RUN","mint"),
            ("RADIATE_CMD","RADIATE CMD","mint"),
            ("RADIATE_RUN","RADIATE RUN","mint"),
            ("ANT_RF","ANT RF","mint"),
            ("EMI_RF","EMI RF","mint"),
            ("EMI_OUT","EMI OUT","mint"),
            ("SPEC_IN","SPEC IN","mint"),
            ("SCADA_RAD","SCADA RAD","mint"),
            ("SPEC_CAL","SPEC CAL","mint"),
            ("SPEC_ZERO","SPEC ZERO","mint"),
            ("PRESEL_OUT","PRESEL OUT","mint"),
            ("PRESEL_IN","PRESEL IN","mint"),
            ("FILTER_CMD","FILTER CMD","mint"),
            ("EMI_FILTER","EMI FILTER","mint"),
            ("HEIGHT_ACT","HEIGHT ACT","mint"),
            ("HEIGHT_TAG","HEIGHT TAG","mint"),
            ("POL_ACT","POL ACT","mint"),
            ("POL_TAG","POL TAG","mint"),
            ("AZ_ACT","AZ ACT","mint"),
            ("AZ_TAG","AZ TAG","mint"),
            ("H_OUT","H OUT","mint"),
            ("H_SAMPLE","H SAMPLE","mint"),
            ("E_OUT","E OUT","mint"),
            ("E_SAMPLE","E SAMPLE","mint"),
            ("SEAM_OUT","SEAM OUT","mint"),
            ("SEAM_SAMPLE","SEAM SAMPLE","mint"),
            ("WORST_OUT","WORST OUT","mint"),
            ("WORST_VALID","WORST VALID","mint"),
            ("PEAK_OUT","PEAK OUT","mint"),
            ("PEAK_CERT","PEAK CERT","mint"),
            ("CAL_SRC","CAL SRC","mint"),
            ("CAL_REF","CAL REF","mint"),
            ("HEATMAP_OUT","HEATMAP OUT","mint"),
            ("HEATMAP_IN","HEATMAP IN","mint"),
            ("NEAR_CMD","NEAR CMD","mint"),
            ("PROBE_ENABLE","PROBE ENABLE","mint"),
            ("GASKET_CMD","GASKET CMD","mint"),
            ("GASKET_SET","GASKET SET","mint"),
            ("GASKET_OUT","GASKET OUT","mint"),
            ("GASKET_ACT","GASKET ACT","mint"),
            ("CLAMP_CMD","CLAMP CMD","mint"),
            ("SHIELD_CLAMP","SHIELD CLAMP","mint"),
            ("CLAMP_OUT","CLAMP OUT","mint"),
            ("CLAMP_CONTACT","CLAMP CONTACT","mint"),
            ("CHOKE_CMD","CHOKE CMD","mint"),
            ("CHOKE_SET","CHOKE SET","mint"),
            ("CHOKE_ACT","CHOKE ACT","mint"),
            ("CHOKE_OK","CHOKE OK","mint"),
            ("LC_CMD","LC CMD","mint"),
            ("LC_SET","LC SET","mint"),
            ("LC_ACT","LC ACT","mint"),
            ("LC_OK","LC OK","mint"),
            ("FERRITE_CMD","FERRITE CMD","mint"),
            ("FERRITE_SET","FERRITE SET","mint"),
            ("FERRITE_ACT","FERRITE ACT","mint"),
            ("FERRITE_OK","FERRITE OK","mint"),
            ("FEED_CMD","FEED CMD","mint"),
            ("FEED_SET","FEED SET","mint"),
            ("FEED_ACT","FEED ACT","mint"),
            ("FEED_OK","FEED OK","mint"),
            ("ROUTE_CMD","ROUTE CMD","mint"),
            ("ROUTE_SET","ROUTE SET","mint"),
            ("ROUTE_ACTUAL","ROUTE ACTUAL","mint"),
            ("BOND_CHECK","BOND CHECK","mint"),
            ("BOND_INTEGRITY","BOND INTEGRITY","mint"),
            ("SENSOR_FILTER","SENSOR FILTER","mint"),
            ("SENSOR_RESP","SENSOR RESP","mint"),
            ("RF_SWEEP_REQ","RF SWEEP REQ","mint"),
            ("RF_WAVE","RF WAVE","mint"),
            ("IMMUNITY_OK","IMM OK","mint"),
            ("AMP_ACTUAL","AMP ACTUAL","mint"),
            ("FIELD_AUX","FIELD AUX","mint"),
            ("UNIFORMITY_CMD","UNIFORMITY CMD","mint"),
            ("FIELD_REF","FIELD REF","mint"),
            ("FIELD_CAL","FIELD CAL","mint"),
            ("SWEEP_TELEM","SWEEP TELEM","mint"),
            ("COUPLE_FIXTURE","COUPLE FIXTURE","mint"),
            ("FIXTURE_OK","FIXTURE OK","mint"),
            ("FIXTURE_STOP","FIXTURE STOP","amber"),
            ("SENSOR_CHECK","SENSOR CHECK","mint"),
            ("OPTICAL_CHECK","OPTICAL CHECK","mint"),
            ("CLOCK_CHECK","CLOCK CHECK","mint"),
            ("MISSION_REQ","MISSION REQ","mint"),
            ("ADC_SAMPLE","ADC SAMPLE","mint"),
            ("ADC_DEV","ADC DEV","mint"),
            ("PACKET_JITTER","PACKET JITTER","mint"),
            ("CLOCK_JITTER","CLOCK JITTER","mint"),
            ("ADC_COUPLING","ADC COUPLING","mint"),
            ("SPREAD_TELEM","SPREAD TELEM","mint"),
            ("SENSOR_FILTERED","SENSOR FILTERED","mint"),
            ("IR_THERMAL","IR THERMAL","mint"),
            ("WORKLOAD_OK","WORKLOAD OK","mint"),
            ("CMD_SAFE","CMD SAFE","amber"),
            ("CM_FAULT","CM FAULT","amber"),
            ("RETURN_FAULT","RETURN FAULT","amber"),
            ("SEAM_FAULT","SEAM FAULT","amber"),
            ("DESENSE_FAULT","DESENSE FAULT","amber"),
            ("DIAGNOSE","DIAGNOSE","mint"),
            ("SCADA_REPAIR","SCADA REPAIR","mint"),
            ("RF_ARM_REQ","RF ARM REQ","mint"),
            ("DOOR_NC","DOOR NC","mint"),
            ("DOOR_CLOSED_IN","DOOR CLOSED IN","mint"),
            ("RF_FIELD_MEASURED_OUT","FIELD OUT","mint"),
            ("RF_SAFE_FIELD_FEEDBACK_IN","RF SAFE IN","amber"),
            ("SCADA_FIELD","SCADA FIELD","mint"),
            ("AMP_PERMIT","AMP PERMIT","amber"),
            ("AMP_ENABLE","AMP ENABLE","mint"),
            ("RF_OVERRIDE","RF OVERRIDE","mint"),
            ("ESTOP_NC","ESTOP NC","amber"),
            ("SAFE_STOP_IN","SAFE STOP IN","amber"),
            ("AMP_DISABLED","AMP DISABLED","mint"),
            ("FIELD_HEALTH","FIELD HEALTH","mint"),
            ("FIELD_LIMIT","FIELD LIMIT","mint"),
            ("EXPOSURE_AZ","EXPOSURE AZ","mint"),
            ("EXPOSURE_HEIGHT","EXPOSURE HEIGHT","mint"),
            ("EUT_VALID","EUT VALID","mint"),
            ("SEAL_OK","SEAL OK","mint"),
            ("RF_LATCH","RF LATCH","mint"),
            ("RF_RESET","RF RESET","mint"),
            ("OBJECTIVE_STATE","OBJECTIVE STATE","mint"),
            ("EMC_CERT","EMC CERT","mint"),
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
            if (root == null || root.GetComponent<SparkLevel82Process>().pinLabels.Length != 206) Fit();
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
                control.pinLabels.Length!=206 || control.pinLabels.Any(label=>!label) ||
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

        [MenuItem("Tools/Project Spark/Level 82/Fit Equipment Terminals")]
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
                var table = FindTable(scene);
                Physics.SyncTransforms();
                root = PrefabUtility.LoadPrefabContents(Prefab);
                Fit(root, models, table, layoutRoot != null ? layoutRoot.localToWorldMatrix : Matrix4x4.identity);
                if (PrefabUtility.SaveAsPrefabAsset(root, Prefab) == null)
                    throw new InvalidOperationException("Unity could not save the Level 82 layout.");
                AssetDatabase.SaveAssets();
                Debug.Log("Level 82 fitted to the workshop table with 206 original equipment terminals.");
            }
            finally
            {
                if (root != null) PrefabUtility.UnloadPrefabContents(root);
                if (opened) EditorSceneManager.CloseScene(scene, true);
            }
        }

        private const string FixtureName = "L82_HD_VISUAL_ART";
        public const string MountsName = "Equipment terminal panels";
        private const string BaseModule = "L82_BlackstarEMCChamberBase";

        public static BoxCollider FindTable(Scene scene) => SparkEquipmentWorkbenchFit.FindTable(scene);

        public static Bounds MeasureArt(Transform root, Matrix4x4 worldToSpace) => SparkEquipmentWorkbenchFit.MeasureArt(root, worldToSpace);

        private static void Fit(GameObject root, GameObject[] models, BoxCollider table, Matrix4x4 layout)
        {
            var bench = root.transform.Find("EMC Workbench");
            var bindings = root.GetComponent<SparkLevelSceneBindings>();
            var loop = root.GetComponent<SparkLevel82Process>();
            if (bench == null || bindings == null || loop == null)
                throw new InvalidOperationException("Level 82 is missing its workbench, controls, or scene bindings.");

            // Keep electrical owners intact while the editor replaces the visual mounts.
            foreach (var terminal in loop.pins)
                terminal.transform.SetParent(terminal.Owner.transform, true);

            Matrix4x4 parentWorld = layout * bench.parent.localToWorldMatrix;
            Vector3 tableSize = Vector3.Scale(table.size, table.transform.lossyScale);
            Vector3 tableTop = table.transform.TransformPoint(table.center + Vector3.up * table.size.y * .5f);
            bench.localPosition = parentWorld.inverse.MultiplyPoint3x4(tableTop + table.transform.up * .025f);
            bench.localRotation = Quaternion.Inverse(parentWorld.rotation) * table.transform.rotation;
            float worldScale = Mathf.Min(tableSize.x / 2.60f, tableSize.z / 1.55f);
            bench.localScale = Vector3.one * (worldScale / parentWorld.MultiplyVector(Vector3.right).magnitude);
            const float floor = 0f;
            foreach (string name in new[] {FixtureName, MountsName, "Breadboard Labels", "Motion pivots"})
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
            var assembly = Child(bench, FixtureName, Vector3.zero);
            for (int index = 0; index < models.Length; index++)
            {
                var model = models[index];
                var instance = Object.Instantiate(model, assembly, false).transform;
                instance.name = "L82_" + Models[index];
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

            OpenChamberRoof(assembly);
            var artBounds = MeasureArt(assembly, bench.worldToLocalMatrix);
            var baseBounds = MeasureArt(assembly.Find(BaseModule), bench.worldToLocalMatrix);
            float fit = Mathf.Min((tableSize.x * .91f) / (artBounds.size.x * worldScale),
                (tableSize.z * .89f) / (artBounds.size.z * worldScale));
            assembly.localScale = Vector3.one * fit;
            assembly.localPosition = new Vector3(-artBounds.center.x, -baseBounds.min.y, -artBounds.center.z) * fit;
            var view = root.GetComponent<SparkLevel82WorkbenchView>() ?? root.AddComponent<SparkLevel82WorkbenchView>();
            view.Bench = bench;
            view.FramingBounds = MeasureArt(assembly, bench.worldToLocalMatrix);
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
            if (!loop.calibrationText || !loop.cableViewButton || !loop.plot ||
                !loop.plot.GetComponent<CanvasRenderer>() || loop.plot.rectTransform.anchorMax != Vector2.one)
            {
                var oldControls = root.transform.Find("Level 82 Controls");
                if (oldControls != null) Object.DestroyImmediate(oldControls.gameObject);
                SparkLevel82Builder.BuildControls(root.transform,loop,font);
            }
            var labels = Child(bench, MountsName, Vector3.zero);
            SeatOriginalTerminals(bench, assembly, bindings, loop, labels, font);
            var title = Label(labels, font, "Level name", "SILENCE THE SPECTRUM", new Vector3(0, .025f, -.74f), .025f);
            title.color = new Color(.53f, .85f, 1f);
            loop.rig.art = assembly;
            loop.rig.pivots = SparkEquipmentWorkbenchFit.SeatMotionPivots(assembly,
                AssetDatabase.LoadAssetAtPath<TextAsset>(Folder + "Equipment_Animation_Pivots.csv"));
            Transform turntable=loop.rig.pivots.Single(p=>p.name.EndsWith("ANIM_EUT_TURNTABLE",StringComparison.Ordinal));
            var equipment=assembly.Find("L82_FourChipletMissionComputerEUT");
            var rotatingArt=equipment.GetComponentsInChildren<Transform>(true)
                .FirstOrDefault(part=>part!=equipment && part.name=="L82_FourChipletMissionComputerEUT");
            if (rotatingArt)
            {
                // The front connector panel stays fixed while the board moves on the turntable.
                foreach (var contact in rotatingArt.Cast<Transform>().Where(part => part.name.EndsWith("__tintE9BC56", StringComparison.Ordinal)).ToArray())
                    contact.SetParent(equipment, true);
                rotatingArt.SetParent(turntable, true);
            }
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
            Lamp("EMC work light",new Vector3(-.52f,floor+.78f,-.31f),new Color(.94f,.96f,1),3.25f,4.6f);
            Lamp("Metrology fill",new Vector3(.61f,floor+.55f,.10f),new Color(.44f,.76f,1),2.1f,3.8f);
            loop.rig.BindEffects(
                Lamp("Near-field activity light",new Vector3(-.37f,floor+.20f,-.03f),new Color(.29f,.75f,1),1,.45f),
                Lamp("RF exposure light",new Vector3(0,floor+.20f,-.03f),new Color(.48f,.39f,1),1,.46f),
                Lamp("EUT telemetry light",new Vector3(.28f,floor+.2f,-.03f),new Color(.58f,.37f,1),1,.45f),
                Lamp("Isolation alarm",new Vector3(.56f,floor+.24f,.03f),new Color(1,.18f,.12f),1.25f,.55f));
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
            loop.controllerDisplay=Readout("Field readout",new Vector3(-.82f,floor+.060f,-.74f),new Color(.46f,.94f,1f));
            loop.qualityDisplay=Readout("Chamber status",new Vector3(.82f,floor+.060f,-.74f),new Color(.71f,1f,.86f));
        }

        private static void OpenChamberRoof(Transform assembly)
        {
            const float roofHeight = 8.40f * SourceArtUnit;
            string folder = Folder + "Models/Workbench";
            foreach (string module in new[] {"L82_SemiAnechoicShieldedHall", "L82_RFAbsorberPyramidWallSet"})
            foreach (var filter in assembly.Find(module).GetComponentsInChildren<MeshFilter>())
            {
                Mesh original = filter.sharedMesh;
                using var source = MeshUtility.AcquireReadOnlyMeshData(original);
                using var vertices = new NativeArray<Vector3>(source[0].vertexCount, Allocator.Temp);
                source[0].GetVertices(vertices);
                var toAssembly = assembly.worldToLocalMatrix * filter.transform.localToWorldMatrix;
                bool[] aboveRoof = vertices.Select(vertex => toAssembly.MultiplyPoint3x4(vertex).y >= roofHeight).ToArray();
                if (!aboveRoof.Contains(true)) continue;

                var remaining = new List<int>[original.subMeshCount];
                bool cut = false;
                for (int sub = 0; sub < remaining.Length; sub++)
                {
                    using var triangles = new NativeArray<int>(source[0].GetSubMesh(sub).indexCount, Allocator.Temp);
                    source[0].GetIndices(triangles, sub);
                    var keep = remaining[sub] = new List<int>(triangles.Length);
                    for (int i = 0; i < triangles.Length; i += 3)
                    {
                        int a = triangles[i], b = triangles[i + 1], c = triangles[i + 2];
                        if (aboveRoof[a] && aboveRoof[b] && aboveRoof[c]) { cut = true; continue; }
                        keep.Add(a);
                        keep.Add(b);
                        keep.Add(c);
                    }
                }
                if (!cut) continue;
                if (remaining.All(indices => indices.Count == 0))
                {
                    filter.gameObject.SetActive(false);
                    continue;
                }

                if (!AssetDatabase.IsValidFolder(folder)) AssetDatabase.CreateFolder(Folder + "Models", "Workbench");
                string path = $"{folder}/{filter.name}.asset";
                var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
                bool newAsset = !mesh;
                if (newAsset) mesh = new Mesh {name = filter.name};
                var writable = Mesh.AllocateWritableMeshData(1);
                writable[0].SetVertexBufferParams(source[0].vertexCount, original.GetVertexAttributes());
                for (int stream = 0; stream < source[0].vertexBufferCount; stream++)
                    writable[0].GetVertexData<byte>(stream).CopyFrom(source[0].GetVertexData<byte>(stream));
                Mesh.ApplyAndDisposeWritableMeshData(writable, mesh);
                mesh.indexFormat = IndexFormat.UInt32;
                mesh.subMeshCount = remaining.Length;
                for (int sub = 0; sub < remaining.Length; sub++)
                    mesh.SetTriangles(remaining[sub], sub, false);
                mesh.bounds = original.bounds;
                if (newAsset) AssetDatabase.CreateAsset(mesh, path);
                else EditorUtility.SetDirty(mesh);
                filter.sharedMesh = mesh;
            }
        }

        private static Vector3 SourcePosition(string[] values) => SparkEquipmentWorkbenchFit.SourcePosition(values);

        private static void SeatOriginalTerminals(Transform bench, Transform assembly, SparkLevelSceneBindings bindings,
            SparkLevel82Process loop, Transform panels, Font font)
        {
            var data = AssetDatabase.LoadAssetAtPath<TextAsset>(Folder + "Equipment_Terminal_Mounts.csv");
            if (!data) throw new InvalidOperationException("The original Level 82 mount mapping is missing.");
            int pinLayer = LayerMask.NameToLayer("Pin");
            if (pinLayer < 0) throw new InvalidOperationException("The Pin layer is missing.");
            var specs = Sockets.ToDictionary(socket => socket.id);
            var indices = loop.pins.Select((pin, index) => (pin, index)).ToDictionary(item => item.pin.name, item => item.index);
            var mounts = new HashSet<string>();
            var assigned = new HashSet<string>();
            var frames = new Dictionary<string, Transform>();
            loop.pinLabels = new TextMesh[loop.pins.Length];

            foreach (string row in data.text.Split('\n').Skip(1))
            {
                if (string.IsNullOrWhiteSpace(row)) continue;
                string[] values = row.TrimEnd('\r').Split(',');
                if (values.Length != 6 || !mounts.Add(values[0]) || !assigned.Add(values[5]) ||
                    !bindings.TryGetTerminal(values[5], out var terminal))
                    throw new InvalidOperationException("Duplicate or unresolved equipment socket: " + row);
                string equipment = values[1];
                if (!frames.TryGetValue(equipment, out var frame))
                {
                    if (!assembly.Find(equipment)) throw new InvalidOperationException("Missing original socket equipment " + equipment);
                    frame = Child(panels, equipment + " Fixed Terminals", Vector3.zero);
                    frames.Add(equipment, frame);
                }
                var mount = Child(frame, values[0] + " " + values[5], Vector3.zero);
                mount.position = assembly.TransformPoint(SourcePosition(values));
                mount.rotation = bench.rotation;
                var jack = terminal.transform;
                jack.SetParent(mount, false);
                jack.localRotation = Quaternion.identity;
                jack.localScale = Vector3.one;
                // The source gold collar ends .037 m beyond the authored socket center.
                jack.localPosition = bench.InverseTransformVector(assembly.TransformVector(Vector3.back * (.037f * SourceArtUnit)));
                jack.gameObject.layer = pinLayer;
                float diameter = bench.InverseTransformVector(assembly.TransformVector(Vector3.right * (.136f * SourceArtUnit))).magnitude;
                var collider = jack.GetComponent<BoxCollider>();
                foreach (var old in jack.GetComponents<Collider>())
                    if (old != collider) Object.DestroyImmediate(old);
                if (!collider) collider = jack.gameObject.AddComponent<BoxCollider>();
                collider.size = new Vector3(diameter, diameter, diameter * .36f);
                collider.center = Vector3.zero;
                if (jack.TryGetComponent<MeshRenderer>(out var renderer)) Object.DestroyImmediate(renderer);
                if (jack.TryGetComponent<MeshFilter>(out var mesh)) Object.DestroyImmediate(mesh);
                var recess = jack.Find("Jack recess");
                if (recess) Object.DestroyImmediate(recess.gameObject);
                int index = indices[values[5]];
                var pinData = new SerializedObject(terminal);
                pinData.FindProperty("maxConnections").intValue = SparkLevel82Process.Wires.Count(wire => wire.a == index || wire.b == index);
                pinData.ApplyModifiedPropertiesWithoutUndo();

                var tag = Label(mount, font, "Label " + terminal.name, specs[terminal.name].label, new Vector3(0, .022f, -.009f), .019f);
                tag.transform.localRotation = Quaternion.Euler(30, 0, 0);
                tag.gameObject.SetActive(false);
                loop.pinLabels[index] = tag;
            }
            if (assigned.Count != loop.pins.Length || loop.pinLabels.Any(label => !label))
                throw new InvalidOperationException("Level 82 must map every circuit terminal to its own original equipment socket.");
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





















