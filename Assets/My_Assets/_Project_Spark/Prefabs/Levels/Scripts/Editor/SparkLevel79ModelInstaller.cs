#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using ProjectSpark.Circuit;
using ProjectSpark.Gameplay;
using ProjectSpark.UI.VisualTeaching;
using TMPro;
using Unity.Collections;
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
        internal const string FixtureName = "L79_HD_VISUAL_ART";
        internal const string MountsName = "Equipment commissioning";
        private const string BaseModule = "L79_Industrial3DHybridBondingBase";
        private const string SurfaceMaps = "Assets/My_Assets/_Project_Spark/Prefabs/Levels/Level_80_Layout/Textures/";
        // The fitted cabinet exports include this vertical offset in source units.
        private const float FittedContactLift = .08f;

        private static readonly (string id, string label, string color)[] Sockets =
        {
            ("SUPPLY_POS","SUP POS","red"),
            ("PLC_5V","PLC 5V","red"),
            ("SUPPLY_RET","SUP RET","blue"),
            ("PLC_GND","PLC GND","blue"),
            ("VISION_5V","VISION 5V","red"),
            ("VISION_GND","VISION GND","blue"),
            ("MOTION_5V","MOTION 5V","red"),
            ("MOTION_GND","MOTION GND","blue"),
            ("LASER_5V","LASER 5V","red"),
            ("LASER_GND","LASER GND","blue"),
            ("SAFE_5V","SAFE 5V","red"),
            ("SAFE_GND","SAFE GND","blue"),
            ("MOTION_ENABLE","MOTION ENABLE","mint"),
            ("GANTRY_ENABLE","GANTRY ENABLE","mint"),
            ("X_CMD","X CMD","mint"),
            ("X_SERVO","X SERVO","mint"),
            ("X_ENC","X ENC","mint"),
            ("X_FEEDBACK","X FEEDBACK","mint"),
            ("Y_CMD","Y CMD","mint"),
            ("Y_SERVO","Y SERVO","mint"),
            ("Y_ENC","Y ENC","mint"),
            ("Y_FEEDBACK","Y FEEDBACK","mint"),
            ("Z_CMD","Z CMD","mint"),
            ("Z_SERVO","Z SERVO","mint"),
            ("Z_ENC","Z ENC","mint"),
            ("Z_FEEDBACK","Z FEEDBACK","mint"),
            ("POSE_CMD","POSE CMD","mint"),
            ("HEXAPOD_DRIVE","HEXAPOD DRIVE","mint"),
            ("HEX_READY","HEX READY","mint"),
            ("POSE_READY","POSE READY","mint"),
            ("NANO_X_CMD","NANO X CMD","mint"),
            ("NANO_X_DRIVE","NANO X DRIVE","mint"),
            ("NANO_X_OUT","NANO X OUT","mint"),
            ("FINE_X","FINE X","mint"),
            ("NANO_Y_CMD","NANO Y CMD","mint"),
            ("NANO_Y_DRIVE","NANO Y DRIVE","mint"),
            ("NANO_Y_OUT","NANO Y OUT","mint"),
            ("FINE_Y","FINE Y","mint"),
            ("THETA_CMD","THETA CMD","mint"),
            ("THETA_DRIVE","THETA DRIVE","mint"),
            ("THETA_OUT","THETA OUT","mint"),
            ("THETA_IN","THETA IN","mint"),
            ("TILT_CMD","TILT CMD","mint"),
            ("TILT_DRIVE","TILT DRIVE","mint"),
            ("TILT_OUT","TILT OUT","mint"),
            ("TILT_IN","TILT IN","mint"),
            ("POSE_SAFE","POSE SAFE","amber"),
            ("MOTION_INTERLOCK","MOTION INTERLOCK","mint"),
            ("CAM_A_OUT","CAM A OUT","mint"),
            ("VISION_A","VISION A","mint"),
            ("CAM_B_OUT","CAM B OUT","mint"),
            ("VISION_B","VISION B","mint"),
            ("IR_REG_OUT","IR REG OUT","mint"),
            ("IR_REG_IN","IR REG IN","mint"),
            ("MOIRE_OUT","MOIRE OUT","mint"),
            ("OVERLAY_DIAG","OVERLAY DIAG","mint"),
            ("WARP_OUT","WARP OUT","mint"),
            ("WARP_IN","WARP IN","mint"),
            ("CAM_A_RES","CAM A RES","mint"),
            ("ALIGN_A","ALIGN A","mint"),
            ("CAM_B_RES","CAM B RES","mint"),
            ("ALIGN_B","ALIGN B","mint"),
            ("VISION_AGREE","VISION AGREE","mint"),
            ("ALIGN_VALID","ALIGN VALID","mint"),
            ("LASER_INTERFEROMETER_POSITION_OUT","LASER POS OUT","mint"),
            ("HEXAPOD_FINE_POSITION_IN","HEX FINE IN","mint"),
            ("LASER_TELEM","LASER TELEM","mint"),
            ("LASER_CMD","LASER CMD","mint"),
            ("LASER_ENABLE","LASER ENABLE","mint"),
            ("LASER_HEALTH","LASER HEALTH","mint"),
            ("SAFETY_LASER","SAFETY LASER","amber"),
            ("LASER_CAL_OK","LASER CAL OK","mint"),
            ("POSE_QUAL","POSE QUAL","mint"),
            ("BOND_POSE","BOND POSE","mint"),
            ("OVERLAY_VIEW","OVERLAY VIEW","mint"),
            ("IR_CERT","IR CERT","mint"),
            ("REG_CERT","REG CERT","mint"),
            ("WARP_COMP","WARP COMP","mint"),
            ("WARP_AUTH","WARP AUTH","mint"),
            ("OPTICS_VALID","OPTICS VALID","mint"),
            ("MOTION_VISION","MOTION VISION","mint"),
            ("GRIP_CMD","GRIP CMD","mint"),
            ("UPPER_GRIP","UPPER GRIP","mint"),
            ("RELEASE_OUT","RELEASE OUT","mint"),
            ("CHUCK_RELEASE_MON","CHUCK RELEASE MON","mint"),
            ("CHUCK_READY","CHUCK READY","mint"),
            ("RELEASE_OK","RELEASE OK","mint"),
            ("LOWER_CMD","LOWER CMD","mint"),
            ("BASE_GRIP","BASE GRIP","mint"),
            ("BASE_HELD","BASE HELD","mint"),
            ("BASE_HOLD_OK","BASE HOLD OK","mint"),
            ("FORCE_CMD","FORCE CMD","mint"),
            ("FORCE_ENABLE","FORCE ENABLE","mint"),
            ("FORCE_ARRAY","FORCE ARRAY","mint"),
            ("FORCE_IN","FORCE IN","mint"),
            ("CONTACT_PERMIT","CONTACT PERMIT","amber"),
            ("CONTACT_ENABLE","CONTACT ENABLE","mint"),
            ("CONTACT_OUT","CONTACT OUT","mint"),
            ("CONTACT_POSITION","CONTACT POSITION","mint"),
            ("GAP_OUT","GAP OUT","mint"),
            ("GAP_IN","GAP IN","mint"),
            ("WAVE_OUT","WAVE OUT","mint"),
            ("WAVE_IN","WAVE IN","mint"),
            ("WAVE_CERT","WAVE CERT","mint"),
            ("FRONT_CERT","FRONT CERT","mint"),
            ("CONTACT_CMD","CONTACT CMD","mint"),
            ("CONTACT_REQ","CONTACT REQ","mint"),
            ("RETRACT_CMD","RETRACT CMD","mint"),
            ("RETRACT_OVERRIDE","RETRACT OVERRIDE","mint"),
            ("APPROACH_OUT","APPROACH OUT","mint"),
            ("SAFE_APPROACH","SAFE APPROACH","amber"),
            ("CHUCK_CAL","CHUCK CAL","mint"),
            ("RELEASE_CAL","RELEASE CAL","mint"),
            ("DIE_CERT","DIE CERT","mint"),
            ("KNOWN_GOOD","KNOWN GOOD","mint"),
            ("PARTICLE_OUT","PARTICLE OUT","mint"),
            ("PARTICLES_IN","PARTICLES IN","mint"),
            ("FLATNESS_OUT","FLATNESS OUT","mint"),
            ("FLATNESS_IN","FLATNESS IN","mint"),
            ("ENERGY_OUT","ENERGY OUT","mint"),
            ("SURFACE_ENERGY","SURFACE ENERGY","mint"),
            ("CLEAN_CMD","CLEAN CMD","mint"),
            ("DRY_CLEAN","DRY CLEAN","mint"),
            ("CLEAN_OUT","CLEAN OUT","mint"),
            ("CLEAN_DONE","CLEAN DONE","mint"),
            ("ACTIVATE_CMD","ACTIVATE CMD","mint"),
            ("SURFACE_ACT","SURFACE ACT","mint"),
            ("ACTIVATE_OUT","ACTIVATE OUT","mint"),
            ("ACT_DONE","ACT DONE","mint"),
            ("ESD_GUARD","ESD GUARD","mint"),
            ("ESD_OK","ESD OK","mint"),
            ("REJECT_SENSOR","REJECT SENSOR","mint"),
            ("QUARANTINE_STATE","QUARANTINE STATE","mint"),
            ("REJECT_CMD","REJECT CMD","mint"),
            ("REJECT_BIN","REJECT BIN","mint"),
            ("ALIGNER_OUT","ALIGNER OUT","mint"),
            ("BASE_ALIGNED","BASE ALIGNED","mint"),
            ("SURFACE_CERT","SURFACE CERT","mint"),
            ("SURFACE_OK","SURFACE OK","mint"),
            ("LOCK_SAFE","LOCK SAFE","amber"),
            ("LOADLOCK_OK","LOADLOCK OK","mint"),
            ("ANNEAL_ENABLE","ANNEAL ENABLE","mint"),
            ("HEATER_SET","HEATER SET","mint"),
            ("HEATER_ACTUAL","HEATER ACTUAL","mint"),
            ("THERMAL_TEMP","THERMAL TEMP","mint"),
            ("EXPANSION_TELEM","EXPANSION TELEM","mint"),
            ("BOND_STRESS","BOND STRESS","mint"),
            ("THERMAL_SAMPLE","THERMAL SAMPLE","mint"),
            ("PROFILE_OK","PROFILE OK","mint"),
            ("THERMAL_CYCLE","THERMAL CYCLE","mint"),
            ("THERMAL_DONE","THERMAL DONE","mint"),
            ("STACK_WARP","STACK WARP","mint"),
            ("ANNEAL_SAFE","ANNEAL SAFE","amber"),
            ("HEATER_OFF","HEATER OFF","mint"),
            ("STRESS_OK","STRESS OK","mint"),
            ("ACOUSTIC_REQ","ACOUSTIC REQ","mint"),
            ("ACOUSTIC_MAP","ACOUSTIC MAP","mint"),
            ("IR_SCAN_REQ","IR SCAN REQ","mint"),
            ("IR_VOID_MAP","IR VOID MAP","mint"),
            ("SEAM_VIEW","SEAM VIEW","mint"),
            ("ADHESION_RESULT","ADHESION RESULT","mint"),
            ("KELVIN_RESULT","KELVIN RESULT","mint"),
            ("DAISY_CHAIN","DAISY CHAIN","mint"),
            ("ROUTING_TEST","ROUTING TEST","mint"),
            ("BER_RESULT","BER RESULT","mint"),
            ("LINK_TEST","LINK TEST","mint"),
            ("SYSTEM_CERT","SYSTEM CERT","mint"),
            ("BOND_TELEM","BOND TELEM","mint"),
            ("LINK_TELEM","LINK TELEM","mint"),
            ("ESTOP_NC","ESTOP NC","amber"),
            ("SAFE_STOP_IN","SAFE STOP IN","amber"),
            ("GANTRY_OFF","GANTRY OFF","mint"),
            ("HEX_HOLD","HEX HOLD","mint"),
            ("RELEASE_INHIBIT","RELEASE INHIBIT","mint"),
            ("APPROACH_INHIBIT","APPROACH INHIBIT","mint"),
            ("AXIS_STATUS","AXIS STATUS","mint"),
            ("ALIGN_STATUS","ALIGN STATUS","mint"),
            ("PLC_STATE","PLC STATE","mint"),
            ("OPERATOR_CMD","OPERATOR CMD","mint"),
            ("POSE_TELEM","POSE TELEM","mint"),
            ("OVERLAY_MAP","OVERLAY MAP","mint"),
            ("BOND_MAP","BOND MAP","mint"),
            ("CHIP_GRAPH","CHIP GRAPH","mint"),
            ("OBJECTIVE_EVENT","OBJECTIVE EVENT","mint"),
            ("MOTION_RESET","MOTION RESET","mint"),
            ("FAULT_LATCH","FAULT LATCH","amber"),
            ("LASER_LOSS","LASER LOSS","mint"),
            ("FINAL_CERT","FINAL CERT","mint"),
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
            if (root == null || !root.GetComponent<SparkEquipmentWorkbenchView>()) Fit();
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
                control.pins.Length!=188 || control.pins.Distinct().Count()!=188 || control.pinLabels.Length!=188 ||
                !control.focusWireButton || !root.GetComponent<SparkEquipmentWorkbenchView>() ||
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

        [MenuItem("Tools/Project Spark/Level 79/Fit Equipment Workbench")]
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
                var table = SparkEquipmentWorkbenchFit.FindTable(scene);
                root = PrefabUtility.LoadPrefabContents(Prefab);
                Fit(root, models, table, layoutRoot != null ? layoutRoot.localToWorldMatrix : Matrix4x4.identity);
                if (PrefabUtility.SaveAsPrefabAsset(root, Prefab) == null)
                    throw new InvalidOperationException("Unity could not save the Level 79 layout.");
                AssetDatabase.SaveAssets();
                Debug.Log("Level 79 fitted to the workshop table with 188 equipment-mounted terminals.");
            }
            finally
            {
                if (root != null) PrefabUtility.UnloadPrefabContents(root);
                if (opened) EditorSceneManager.CloseScene(scene, true);
            }
        }

        private static void Fit(GameObject root, GameObject[] models, BoxCollider table, Matrix4x4 layout)
        {
            var bench = root.transform.Find("Bonding Workbench");
            var bindings = root.GetComponent<SparkLevelSceneBindings>();
            var loop = root.GetComponent<SparkLevel79Process>();
            if (bench == null || bindings == null || loop == null)
                throw new InvalidOperationException("Level 79 is missing its workbench, controls, or scene bindings.");

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
                instance.name = "L79_" + Models[index];
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
                if(key=="tint1D303F" && mesh.name.StartsWith("L79_TransparentVerticalBondingTower_",StringComparison.Ordinal)) key="glass";
                string surface=key switch
                {
                    "glass"=>"OpticalGlass",
                    "tintDC803E"=>"CopperBus",
                    "tintE9BC56"=>"GoldContact",
                    "tint47657C" or "tintC2D0D6" or "tint97B3C4"=>"BrushedSteel",
                    "tint0C1620" when mesh.name.Contains("Cable")=>"Elastomer",
                    "tint0C1620" or "tintE0DED3"=>"BlackCeramic",
                    "tint0D1D2F" when mesh.name.StartsWith("L79_Industrial3DHybridBondingBase_",StringComparison.Ordinal)=>"CleanroomFloor",
                    "tint0D1D2F" when mesh.name.Contains("Wafer") || mesh.name.Contains("Chiplet")=>"SiliconInterposer",
                    "tint0D1D2F" or "tint1D303F"=>"AnodizedPanel",
                    _=>null
                };
                if (key == "tint2D3034") surface = "Elastomer";
                var material = MaterialFor(shader,key,surface);
                // The fitted meshes can contain several imported submeshes.
                mesh.sharedMaterials = Enumerable.Repeat(material, mesh.GetComponent<MeshFilter>().sharedMesh.subMeshCount).ToArray();
                if (key is "glass" or "acrylic") mesh.shadowCastingMode = ShadowCastingMode.Off;
            }

            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (!loop.focusWireButton || !loop.plot || !loop.plot.GetComponent<CanvasRenderer>() ||
                loop.plot.rectTransform.anchorMax != Vector2.one)
            {
                var oldControls = root.transform.Find("Level 79 Controls");
                if (oldControls) Object.DestroyImmediate(oldControls.gameObject);
                SparkLevel79Builder.BuildControls(root.transform, loop, font);
            }
            var labels = Child(bench, MountsName, Vector3.zero);
            RemoveUnusedSupplyContacts(assembly);
            SeatOriginalTerminals(bench, assembly, bindings, loop, font, shader);
            var deck = SparkEquipmentWorkbenchFit.MeasureArt(assembly.Find(BaseModule), bench.worldToLocalMatrix);
            float labelHeight = deck.max.y + .012f;
            float labelDepth = deck.min.z + .043f;
            var title = Label(labels, font, "Level name", "BOND THE WORLDS", new Vector3(0, labelHeight, labelDepth), .023f);
            title.color = new Color(.51f, .93f, 1f);
            loop.rig.art = assembly;
            loop.rig.pivots = SparkEquipmentWorkbenchFit.SeatMotionPivots(assembly,
                AssetDatabase.LoadAssetAtPath<TextAsset>(Folder + "Equipment_Animation_Pivots.csv"));

            Transform ArtModule(string name) => assembly.Find("L79_" + name);
            loop.rig.chiplets = new[] {"LogicChiplet", "MemoryChiplet", "IOChiplet", "SensorChiplet"}.Select(ArtModule).ToArray();
            loop.rig.stackHologram = ArtModule("Completed3DChipStackHologram");
            var pivots = loop.rig.pivots;
            for (int i = 5; i <= 9; i++) pivots[i].SetParent(pivots[i - 1], true);

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
            Lamp("Bonding work light",new Vector3(-.38f,.58f,-.14f),new Color(.91f,.94f,1),2.65f,4.9f);
            Lamp("Metrology fill",new Vector3(.54f,.34f,.16f),new Color(.49f,.80f,1),1.9f,3.9f);
            loop.rig.BindEffects(
                Lamp("Contact light",new Vector3(-.37f,.20f,-.03f),new Color(.29f,.75f,1),1,.45f),
                Lamp("Consolidation light",new Vector3(0,.20f,-.03f),new Color(.48f,.39f,1),1,.46f),
                Lamp("Inspection light",new Vector3(.28f,.2f,-.03f),new Color(.58f,.37f,1),1,.45f),
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
            loop.controllerDisplay=Readout("Laser readout",new Vector3(-.88f,labelHeight,labelDepth),new Color(.46f,.94f,1f));
            loop.qualityDisplay=Readout("Wafer status",new Vector3(.88f,labelHeight,labelDepth),new Color(.71f,1f,.86f));
        }

        private static void RemoveUnusedSupplyContacts(Transform assembly)
        {
            // The source panel has nine contact positions. The circuit uses the two
            // outermost contacts. Remove the seven decorative assemblies in between.
            const string folder = Folder + "Models/Workbench";
            var supply = assembly.Find("L79_Regulated5VControlSupply");
            foreach (var filter in supply.GetComponentsInChildren<MeshFilter>())
            {
                string tint = filter.name.Substring(filter.name.LastIndexOf("__", StringComparison.Ordinal) + 2);
                if (tint is not ("tint47657C" or "tintDC803E" or "tint32E095" or "tintED484F")) continue;

                Mesh original = filter.sharedMesh;
                using var data = MeshUtility.AcquireReadOnlyMeshData(original);
                var source = data[0];
                using var vertices = new NativeArray<Vector3>(source.vertexCount, Allocator.Temp);
                source.GetVertices(vertices);
                var toAssembly = assembly.worldToLocalMatrix * filter.transform.localToWorldMatrix;
                var removedContact = new int[vertices.Length];
                for (int vertex = 0; vertex < vertices.Length; vertex++)
                {
                    var point = toAssembly.MultiplyPoint3x4(vertices[vertex]) / SparkEquipmentWorkbenchFit.SourceArtUnit;
                    float y = point.y - FittedContactLift;
                    if (y < .987f || y > 1.141f || point.z < 4.103f || point.z > 4.197f) continue;
                    int contact = Mathf.RoundToInt((point.x + 9.91f) / .14f);
                    if (contact is > 0 and < 8 && Mathf.Abs(point.x - (-9.91f + contact * .14f)) < .052f)
                        removedContact[vertex] = contact;
                }
                if (!removedContact.Any(contact => contact != 0)) continue;

                var submeshes = new List<int>[source.subMeshCount];
                for (int sub = 0; sub < submeshes.Length; sub++)
                {
                    using var triangles = new NativeArray<int>(source.GetSubMesh(sub).indexCount, Allocator.Temp);
                    source.GetIndices(triangles, sub);
                    var remaining = submeshes[sub] = new List<int>(triangles.Length);
                    for (int i = 0; i < triangles.Length; i += 3)
                    {
                        int a = triangles[i], b = triangles[i + 1], c = triangles[i + 2];
                        int contact = removedContact[a];
                        if (contact != 0 && removedContact[b] == contact && removedContact[c] == contact) continue;
                        remaining.Add(a);
                        remaining.Add(b);
                        remaining.Add(c);
                    }
                }
                if (submeshes.All(indices => indices.Count == 0))
                {
                    filter.gameObject.SetActive(false);
                    continue;
                }

                if (!AssetDatabase.IsValidFolder(folder)) AssetDatabase.CreateFolder(Folder + "Models", "Workbench");
                string path = $"{folder}/Supply {tint}.asset";
                var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
                bool newAsset = !mesh;
                if (newAsset) mesh = new Mesh { name = filter.name };
                var writable = Mesh.AllocateWritableMeshData(1);
                writable[0].SetVertexBufferParams(source.vertexCount, original.GetVertexAttributes());
                for (int stream = 0; stream < source.vertexBufferCount; stream++)
                    writable[0].GetVertexData<byte>(stream).CopyFrom(source.GetVertexData<byte>(stream));
                Mesh.ApplyAndDisposeWritableMeshData(writable, mesh);
                mesh.indexFormat = IndexFormat.UInt32;
                mesh.subMeshCount = submeshes.Length;
                for (int sub = 0; sub < submeshes.Length; sub++)
                    mesh.SetTriangles(submeshes[sub], sub, false);
                mesh.bounds = original.bounds;
                if (newAsset) AssetDatabase.CreateAsset(mesh, path);
                else EditorUtility.SetDirty(mesh);
                filter.sharedMesh = mesh;
            }
        }

        private static void SeatOriginalTerminals(Transform bench, Transform assembly, SparkLevelSceneBindings bindings,
            SparkLevel79Process loop, Font font, Shader shader)
        {
            var data = AssetDatabase.LoadAssetAtPath<TextAsset>(Folder + "Equipment_Terminal_Mounts.csv");
            if (!data) throw new InvalidOperationException("The original equipment mount map is missing.");
            int pinLayer = LayerMask.NameToLayer("Pin");
            if (pinLayer < 0) throw new InvalidOperationException("The Pin layer is missing.");
            var specs = Sockets.ToDictionary(socket => socket.id);
            var indices = loop.pins.Select((pin, index) => (pin, index)).ToDictionary(item => item.pin.name, item => item.index);
            var mounts = new HashSet<string>();
            var assigned = new HashSet<string>();
            var positions = new HashSet<Vector3>();
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
                Vector3 position = MountPosition(values);
                if (!positions.Add(position))
                    throw new InvalidOperationException("Overlapping Level 79 contacts: " + row);
                var mount = Child(bench, values[0] + " " + values[5], Vector3.zero);
                mount.SetPositionAndRotation(assembly.TransformPoint(position), bench.rotation);
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
                    SocketSleeve(mount, "Panel socket housing", unit, .124f, .079f, .079f, housing);
                    SocketSleeve(mount, "Contact insert", unit, .073f, .004f, -.002f, insert);
                }

                int index = indices[values[5]];
                var pinData = new SerializedObject(terminal);
                pinData.FindProperty("maxConnections").intValue = SparkLevel79Process.Wires.Count(wire => wire.a == index || wire.b == index);
                pinData.ApplyModifiedPropertiesWithoutUndo();
                var tag = Label(mount, font, "Label " + terminal.name, specs[terminal.name].label, new Vector3(0, .022f, -.009f), .017f);
                tag.transform.localRotation = Quaternion.Euler(30, 0, 0);
                tag.gameObject.SetActive(false);
                loop.pinLabels[index] = tag;
            }
            if (assigned.Count != loop.pins.Length || loop.pinLabels.Any(label => !label))
                throw new InvalidOperationException("Level 79 must mount every circuit terminal on original equipment.");
        }

        internal static Vector3 MountPosition(string[] values)
        {
            Vector3 position = SparkEquipmentWorkbenchFit.SourcePosition(values);
            if (values[6] == "Modeled") position.y += FittedContactLift * SparkEquipmentWorkbenchFit.SourceArtUnit;
            return position;
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

        private static Material MaterialFor(Shader shader, string key,string surface=null)
        {
            string materialName=surface==null?"L79 "+key:"L79 "+key+" "+surface;
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
                    throw new InvalidOperationException("Missing Level 79 surface maps for "+surface);
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
