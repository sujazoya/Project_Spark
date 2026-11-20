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
        internal const string MountsName = "Equipment terminal panels";
        internal const string FixtureName = "L80_HD_VISUAL_ART";
        private const string BaseModule = "L80_IndustrialPhotonicDataAtriumBase";

        private static readonly (string id, string label, string color)[] Sockets =
        {
            ("SUPPLY_POS","SUPPLY POS","red"),
            ("PLC_5V","PLC 5V","red"),
            ("SUPPLY_RET","SUPPLY RET","blue"),
            ("PLC_GND","PLC GND","blue"),
            ("TX_5V","TX 5V","red"),
            ("TX_GND","TX GND","blue"),
            ("RING_5V","RING 5V","red"),
            ("RING_GND","RING GND","blue"),
            ("RX_5V","RX 5V","red"),
            ("RX_GND","RX GND","blue"),
            ("SAFE_5V","SAFE 5V","red"),
            ("SAFE_GND","SAFE GND","blue"),
            ("LASER_PERMIT","LASER PERMIT","amber"),
            ("LASER_ENABLE","LASER ENABLE","mint"),
            ("LASER_CMD","LASER CMD","mint"),
            ("LASER_DAC","LASER DAC","mint"),
            ("LAUNCH_TAP","LAUNCH TAP","mint"),
            ("LAUNCH_IN","LAUNCH IN","mint"),
            ("SOURCE_POWER","SOURCE POWER","mint"),
            ("SHUTTER_CMD","SHUTTER CMD","amber"),
            ("SHUTTER_OPEN","SHUTTER OPEN","amber"),
            ("SHUTTER_FB","SHUTTER FB","amber"),
            ("SHUTTER_CLOSED","SHUTTER CLOSED","amber"),
            ("ATTEN_CMD","ATTEN CMD","mint"),
            ("ATTEN_DAC","ATTEN DAC","mint"),
            ("ATTEN_OUT","ATTEN OUT","mint"),
            ("ATTEN_IN","ATTEN IN","mint"),
            ("COMB_OUT","COMB OUT","mint"),
            ("COMB_LOCK","COMB LOCK","mint"),
            ("LASER_TEMP","LASER TEMP","mint"),
            ("PLC_TEMP","PLC TEMP","mint"),
            ("TEMP_CMD","TEMP CMD","mint"),
            ("TEC_SET","TEC SET","mint"),
            ("REF_OUT","REF OUT","mint"),
            ("SPECTRAL_REF","SPECTRAL REF","mint"),
            ("STRAY_OUT","STRAY OUT","mint"),
            ("SAFE_STRAY","SAFE STRAY","amber"),
            ("ISOLATOR_CMD","ISOLATOR CMD","mint"),
            ("ISOLATOR_SET","ISOLATOR SET","mint"),
            ("ISOLATOR_OUT","ISOLATOR OUT","mint"),
            ("ISO_READY","ISO READY","mint"),
            ("POL_OUT","POL OUT","mint"),
            ("POL_READY","POL READY","mint"),
            ("TAP_HEALTH","TAP HEALTH","mint"),
            ("TAP_READY","TAP READY","mint"),
            ("SCADA_LAUNCH","SCADA LAUNCH","mint"),
            ("TX_CMD","TX CMD","mint"),
            ("TX_ENABLE","TX ENABLE","mint"),
            ("TX_DRIVE","TX DRIVE","mint"),
            ("TX_MOD","TX MOD","mint"),
            ("BIAS_CMD","BIAS CMD","mint"),
            ("MOD_BIAS","MOD BIAS","mint"),
            ("MOD_RESP","MOD RESP","mint"),
            ("TX_RESPONSE","TX RESPONSE","mint"),
            ("LOGIC_OUT","LOGIC OUT","mint"),
            ("TX_LOGIC","TX LOGIC","mint"),
            ("MEMORY_OUT","MEMORY OUT","mint"),
            ("TX_MEMORY","TX MEMORY","mint"),
            ("IO_OUT","IO OUT","mint"),
            ("TX_IO","TX IO","mint"),
            ("SENSOR_OUT","SENSOR OUT","mint"),
            ("TX_SENSOR","TX SENSOR","mint"),
            ("PRBS_CMD","PRBS CMD","mint"),
            ("PRBS_START","PRBS START","mint"),
            ("PRBS_OUT","PRBS OUT","mint"),
            ("REF_PATTERN","REF PATTERN","mint"),
            ("TX_STATUS","TX STATUS","mint"),
            ("TX_ACTIVE","TX ACTIVE","mint"),
            ("MUX_CMD","MUX CMD","mint"),
            ("MUX_PERMIT","MUX PERMIT","amber"),
            ("MUX_OUT","MUX OUT","mint"),
            ("MUX_OK","MUX OK","mint"),
            ("MOD_CAL","MOD CAL","mint"),
            ("TX_CAL_OK","TX CAL OK","mint"),
            ("ROUTE_CMD","ROUTE CMD","mint"),
            ("ROUTE_IN","ROUTE IN","mint"),
            ("ROUTE_OUT","ROUTE OUT","mint"),
            ("TX_ROUTES","TX ROUTES","mint"),
            ("TX_TELEM","TX TELEM","mint"),
            ("SCADA_TX","SCADA TX","mint"),
            ("BIAS_OUT","BIAS OUT","mint"),
            ("SCADA_BIAS","SCADA BIAS","mint"),
            ("RING_CMD","RING CMD","mint"),
            ("RING_ENABLE","RING ENABLE","mint"),
            ("HEAT_DAC","HEAT DAC","mint"),
            ("RING_HEAT","RING HEAT","mint"),
            ("TEMP_ARRAY","TEMP ARRAY","mint"),
            ("RING_TEMP","RING TEMP","mint"),
            ("CROSS_HEAT","CROSS HEAT","mint"),
            ("CROSS_IN","CROSS IN","mint"),
            ("A_HEAT","A HEAT","mint"),
            ("A_DAC","A DAC","mint"),
            ("A_TEMP","A TEMP","mint"),
            ("A_TEMPERATURE","A TEMPERATURE","mint"),
            ("B_HEAT","B HEAT","mint"),
            ("B_DAC","B DAC","mint"),
            ("B_TEMP","B TEMP","mint"),
            ("B_TEMPERATURE","B TEMPERATURE","mint"),
            ("C_HEAT","C HEAT","mint"),
            ("C_DAC","C DAC","mint"),
            ("C_TEMP","C TEMP","mint"),
            ("C_TEMPERATURE","C TEMPERATURE","mint"),
            ("D_HEAT","D HEAT","mint"),
            ("D_DAC","D DAC","mint"),
            ("D_TEMP","D TEMP","mint"),
            ("D_TEMPERATURE","D TEMPERATURE","mint"),
            ("A_LOCK","A LOCK","mint"),
            ("A_RESONANCE","A RESONANCE","mint"),
            ("B_LOCK","B LOCK","mint"),
            ("B_RESONANCE","B RESONANCE","mint"),
            ("C_LOCK","C LOCK","mint"),
            ("C_RESONANCE","C RESONANCE","mint"),
            ("D_LOCK","D LOCK","mint"),
            ("D_RESONANCE","D RESONANCE","mint"),
            ("CROSS_CMD","CROSS CMD","mint"),
            ("CROSS_COMP","CROSS COMP","mint"),
            ("DEMOD_OUT","DEMOD OUT","mint"),
            ("DEMOD_LOCK","DEMOD LOCK","mint"),
            ("TEMP_TELEM","TEMP TELEM","mint"),
            ("SCADA_TEMP","SCADA TEMP","mint"),
            ("COOL_CMD","COOL CMD","mint"),
            ("COOLING_SET","COOLING SET","mint"),
            ("DEMUX_CMD","DEMUX CMD","mint"),
            ("DEMUX_CONFIG","DEMUX CONFIG","mint"),
            ("DEMUX_OUT","DEMUX OUT","mint"),
            ("DEMUX_OK","DEMUX OK","mint"),
            ("PD_CMD","PD CMD","mint"),
            ("PD_ENABLE","PD ENABLE","mint"),
            ("PD_OUT","PD OUT","mint"),
            ("PD_CURRENT","PD CURRENT","mint"),
            ("TIA_OUT","TIA OUT","mint"),
            ("RX_ANALOG","RX ANALOG","mint"),
            ("CDR_OUT","CDR OUT","mint"),
            ("CDR_LOCK","CDR LOCK","mint"),
            ("RX_TAP","RX TAP","mint"),
            ("RX_METER","RX METER","mint"),
            ("SCADA_RX_TAP","SCADA RX TAP","mint"),
            ("SPEC_OUT","SPEC OUT","mint"),
            ("SPEC_IN","SPEC IN","mint"),
            ("BUDGET_OUT","BUDGET OUT","mint"),
            ("BUDGET_IN","BUDGET IN","mint"),
            ("DROP_OUT","DROP OUT","mint"),
            ("DROP_OK","DROP OK","mint"),
            ("RX_OPTICAL_POWER_SIGNAL_OUT","RX POWER OUT","mint"),
            ("LINK_SUPERVISOR_RX_POWER_IN","LINK RX IN","mint"),
            ("METER_HEALTH","METER HEALTH","mint"),
            ("TIA_GAIN_REQ","TIA GAIN REQ","mint"),
            ("TIA_ACTUAL","TIA ACTUAL","mint"),
            ("PD_READY","PD READY","mint"),
            ("SCADA_SPEC","SCADA SPEC","mint"),
            ("SCADA_LOSS","SCADA LOSS","mint"),
            ("BER_ENABLE","BER ENABLE","mint"),
            ("PRBS_EXPECT","PRBS EXPECT","mint"),
            ("TEST_BITS","TEST BITS","mint"),
            ("BER_RESULT","BER RESULT","mint"),
            ("EYE_RESULT","EYE RESULT","mint"),
            ("PAM4_QUALITY","PAM4 QUALITY","mint"),
            ("FEC_STATUS","FEC STATUS","mint"),
            ("RAW_BITS","RAW BITS","mint"),
            ("FEC_BITS","FEC BITS","mint"),
            ("CDR_REQ","CDR REQ","mint"),
            ("CLOCK_OUT","CLOCK OUT","mint"),
            ("ROUTE_PASS","ROUTE PASS","mint"),
            ("MEMORY_RESP","MEMORY RESP","mint"),
            ("LOGIC_STATUS","LOGIC STATUS","mint"),
            ("IO_STATUS","IO STATUS","mint"),
            ("SENSOR_TELEM","SENSOR TELEM","mint"),
            ("FIBER_LOSS","FIBER LOSS","mint"),
            ("FAULT_STATE","FAULT STATE","amber"),
            ("INJECT_ENABLE","INJECT ENABLE","mint"),
            ("WAVEGUIDE_SCOPE","WAVEGUIDE SCOPE","mint"),
            ("COUPON_OK","COUPON OK","mint"),
            ("ESD_STATUS","ESD STATUS","mint"),
            ("DOCK_SEATED","DOCK SEATED","mint"),
            ("EDGE_ALIGN","EDGE ALIGN","mint"),
            ("FIBER_ID","FIBER ID","mint"),
            ("CROSSTALK_MAP","CROSSTALK MAP","mint"),
            ("WAVEGUIDE_MAP","WAVEGUIDE MAP","mint"),
            ("WDM_HOLOGRAM","WDM HOLOGRAM","mint"),
            ("ROUTE_HOLOGRAM","ROUTE HOLOGRAM","mint"),
            ("DIAGNOSTICS","DIAGNOSTICS","mint"),
            ("ESTOP_NC","ESTOP NC","amber"),
            ("SAFE_STOP_IN","SAFE STOP IN","amber"),
            ("OVERRIDE_SAFE","OVERRIDE SAFE","amber"),
            ("OPTICAL_PERMIT","OPTICAL PERMIT","amber"),
            ("PLC_ALLOW","PLC ALLOW","mint"),
            ("SCADA_QUAL","SCADA QUAL","mint"),
            ("OPERATOR_CMD","OPERATOR CMD","mint"),
            ("RX_LOSS_TRIP","RX LOSS TRIP","amber"),
            ("LASER_OFF","LASER OFF","mint"),
            ("FAIL_CLOSED","FAIL CLOSED","mint"),
            ("W078_VALID","W078 VALID","mint"),
            ("OBJECTIVE_EVENT","OBJECTIVE EVENT","mint"),
            ("SAFETY_ALARM","SAFETY ALARM","amber"),
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
            if (root == null || root.GetComponent<SparkEquipmentWorkbenchView>() == null) Fit();
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
                control.pins.Length!=194 || control.pins.Distinct().Count()!=194 || !root.GetComponent<SparkEquipmentWorkbenchView>() ||
                control.pins.Any(pin => pin.GetComponent<Renderer>() || !pin.GetComponent<BoxCollider>()) ||
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

        [MenuItem("Tools/Project Spark/Level 80/Fit Equipment Workbench")]
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
                var table = SparkEquipmentWorkbenchFit.FindTable(scene);
                Physics.SyncTransforms();
                root = PrefabUtility.LoadPrefabContents(Prefab);
                Fit(root, models, table, layoutRoot != null ? layoutRoot.localToWorldMatrix : Matrix4x4.identity);
                if (PrefabUtility.SaveAsPrefabAsset(root, Prefab) == null)
                    throw new InvalidOperationException("Unity could not save the Level 80 layout.");
                AssetDatabase.SaveAssets();
                Debug.Log("Level 80 fitted to the workshop table with 194 equipment-mounted terminals.");
            }
            finally
            {
                if (root != null) PrefabUtility.UnloadPrefabContents(root);
                if (opened) EditorSceneManager.CloseScene(scene, true);
            }
        }

        private static void Fit(GameObject root, GameObject[] models, BoxCollider table, Matrix4x4 layout)
        {
            var bench = root.transform.Find("Photonic Workbench");
            var bindings = root.GetComponent<SparkLevelSceneBindings>();
            var loop = root.GetComponent<SparkLevel80Process>();
            if (bench == null || bindings == null || loop == null)
                throw new InvalidOperationException("Level 80 is missing its workbench, controls, or scene bindings.");

            foreach (var terminal in loop.pins)
                terminal.transform.SetParent(terminal.Owner.transform, true);

            Matrix4x4 parentWorld = layout * bench.parent.localToWorldMatrix;
            Vector3 tableSize = Vector3.Scale(table.size, table.transform.lossyScale);
            Vector3 tableTop = table.transform.TransformPoint(table.center + Vector3.up * table.size.y * .5f);
            bench.localPosition = parentWorld.inverse.MultiplyPoint3x4(tableTop + table.transform.up * .025f);
            bench.localRotation = Quaternion.Inverse(parentWorld.rotation) * table.transform.rotation;
            float worldScale = Mathf.Min(tableSize.x / 2.55f, tableSize.z / 1.54f);
            bench.localScale = Vector3.one * (worldScale / parentWorld.MultiplyVector(Vector3.right).magnitude);

            foreach (string name in new[] {FixtureName, MountsName, "Breadboard Labels", "Motion pivots", "Temporary board"})
            {
                var existing = bench.Find(name);
                if (existing) Object.DestroyImmediate(existing.gameObject);
            }
            foreach (var renderer in bench.GetComponentsInChildren<MeshRenderer>(true))
                if (renderer.name == "Body") renderer.gameObject.SetActive(false);

            var assembly = Child(bench, FixtureName, Vector3.zero);
            for (int index = 0; index < models.Length; index++)
            {
                var model = models[index];
                var instance = Object.Instantiate(model, assembly, false).transform;
                instance.name = "L80_" + Models[index];
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
            if (!loop.focusWireButton || !loop.plot || !loop.plot.GetComponent<CanvasRenderer>())
            {
                var oldControls = root.transform.Find("Level 80 Controls");
                if (oldControls) Object.DestroyImmediate(oldControls.gameObject);
                SparkLevel80Builder.BuildControls(root.transform, loop, font);
            }
            var labels = Child(bench, MountsName, Vector3.zero);
            SeatOriginalTerminals(bench, assembly, bindings, loop, font, shader);
            var deck = SparkEquipmentWorkbenchFit.MeasureArt(assembly.Find(BaseModule), bench.worldToLocalMatrix);
            float labelHeight = deck.max.y + .012f;
            float labelDepth = deck.min.z + .043f;
            var title = Label(labels, font, "Level name", "COMMAND THE LIGHT", new Vector3(0, labelHeight, labelDepth), .023f);
            title.color = new Color(.51f, .93f, 1f);
            loop.rig.art = assembly;
            loop.rig.pivots = SparkEquipmentWorkbenchFit.SeatMotionPivots(assembly,
                AssetDatabase.LoadAssetAtPath<TextAsset>(Folder + "Equipment_Animation_Pivots.csv"));
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
            Lamp("Optical work light",new Vector3(-.38f,.58f,-.14f),new Color(.91f,.94f,1),2.65f,4.9f);
            Lamp("Metrology fill",new Vector3(.54f,.34f,.16f),new Color(.49f,.80f,1),1.9f,3.9f);
            loop.rig.BindEffects(
                Lamp("Protected source light",new Vector3(-.37f,.20f,-.03f),new Color(.29f,.75f,1),1.3f,.88f),
                Lamp("WDM activity light",new Vector3(0,.20f,-.03f),new Color(.48f,.39f,1),1.3f,.96f),
                Lamp("Receiver lock light",new Vector3(.28f,.2f,-.03f),new Color(.58f,.37f,1),1,.45f),
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
            loop.controllerDisplay = Readout("RX readout", new Vector3(-.87f,labelHeight,labelDepth), new Color(.46f,.94f,1f));
            loop.qualityDisplay = Readout("Link status", new Vector3(.87f,labelHeight,labelDepth), new Color(.71f,1f,.86f));
        }

        private static void SeatOriginalTerminals(Transform bench, Transform assembly, SparkLevelSceneBindings bindings,
            SparkLevel80Process loop, Font font, Shader shader)
        {
            var data = AssetDatabase.LoadAssetAtPath<TextAsset>(Folder + "Equipment_Terminal_Mounts.csv");
            if (!data) throw new InvalidOperationException("The original equipment mount map is missing.");
            int pinLayer = LayerMask.NameToLayer("Pin");
            if (pinLayer < 0) throw new InvalidOperationException("The Pin layer is missing.");
            var specs = Sockets.ToDictionary(socket => socket.id);
            var indices = loop.pins.Select((pin, index) => (pin, index)).ToDictionary(item => item.pin.name, item => item.index);
            var mounts = new HashSet<string>();
            var assigned = new HashSet<string>();
            loop.pinLabels = new TextMesh[loop.pins.Length];
            float unit = bench.InverseTransformVector(assembly.TransformVector(
                Vector3.right * SparkEquipmentWorkbenchFit.SourceArtUnit)).magnitude;

            foreach (string row in data.text.Split('\n').Skip(1))
            {
                if (string.IsNullOrWhiteSpace(row)) continue;
                string[] values = row.TrimEnd('\r').Split(',');
                if (values.Length != 7 || !mounts.Add(values[0]) || !assigned.Add(values[5]) ||
                    !bindings.TryGetTerminal(values[5], out var terminal) || !(values[6] is "Modeled" or "Planned"))
                    throw new InvalidOperationException("Duplicate or unresolved equipment contact: " + row);
                var module = assembly.Find(values[1]);
                if (!module) throw new InvalidOperationException("Missing equipment " + values[1]);

                // Terminals belong to the fixed chassis rather than its moving shutters and test probes.
                var mount = Child(bench, values[0] + " " + values[5], Vector3.zero);
                mount.SetPositionAndRotation(assembly.TransformPoint(SparkEquipmentWorkbenchFit.SourcePosition(values)), bench.rotation);
                mount.SetParent(module, true);
                var jack = terminal.transform;
                jack.SetParent(mount, false);
                jack.localPosition = Vector3.zero;
                jack.localRotation = Quaternion.identity;
                jack.localScale = Vector3.one;
                jack.gameObject.layer = pinLayer;

                var collider = jack.GetComponent<BoxCollider>();
                foreach (var old in jack.GetComponents<Collider>())
                    if (old != collider) Object.DestroyImmediate(old);
                if (!collider) collider = jack.gameObject.AddComponent<BoxCollider>();
                float diameter = unit * (values[6] == "Modeled" ? .084f : .108f);
                collider.size = new Vector3(diameter, diameter, diameter * .38f);
                collider.center = Vector3.zero;
                if (jack.TryGetComponent<MeshRenderer>(out var renderer)) Object.DestroyImmediate(renderer);
                if (jack.TryGetComponent<MeshFilter>(out var mesh)) Object.DestroyImmediate(mesh);
                var recess = jack.Find("Jack recess");
                if (recess) Object.DestroyImmediate(recess.gameObject);
                if (values[6] == "Planned")
                {
                    // On a module without an authored connector mesh, the source plan supplies the front face.
                    var housing = MaterialFor(shader, "tint47657C", "BrushedSteel");
                    var insert = MaterialFor(shader, specs[terminal.name].color);
                    SocketSleeve(mount, "Panel socket housing", unit, .106f, .073f, .071f, housing);
                    SocketSleeve(mount, "Contact insert", unit, .067f, .004f, .010f, insert);
                }

                int index = indices[values[5]];
                var pinData = new SerializedObject(terminal);
                pinData.FindProperty("maxConnections").intValue = SparkLevel80Process.Wires.Count(wire => wire.a == index || wire.b == index);
                pinData.ApplyModifiedPropertiesWithoutUndo();
                var tag = Label(mount, font, "Label " + terminal.name, specs[terminal.name].label, new Vector3(0, .022f, -.009f), .017f);
                tag.transform.localRotation = Quaternion.Euler(30, 0, 0);
                tag.gameObject.SetActive(false);
                loop.pinLabels[index] = tag;
            }
            if (assigned.Count != loop.pins.Length || loop.pinLabels.Any(label => !label))
                throw new InvalidOperationException("Level 80 must mount every circuit terminal on original equipment.");
        }

        private static void SocketSleeve(Transform mount, string name, float unit, float diameter,
            float halfDepth, float offset, Material material)
        {
            var sleeve = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            Object.DestroyImmediate(sleeve.GetComponent<Collider>());
            sleeve.name = name;
            sleeve.transform.SetParent(mount, false);
            sleeve.transform.SetLocalPositionAndRotation(Vector3.forward * (unit * offset), Quaternion.Euler(90, 0, 0));
            sleeve.transform.localScale = new Vector3(diameter, halfDepth, diameter) * unit;
            sleeve.GetComponent<Renderer>().sharedMaterial = material;
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
