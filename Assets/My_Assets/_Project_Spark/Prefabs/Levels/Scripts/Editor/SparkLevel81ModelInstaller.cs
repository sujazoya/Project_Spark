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
    public static class SparkLevel81ModelInstaller
    {
        public const string Folder = "Assets/My_Assets/_Project_Spark/Prefabs/Levels/Level_81_Layout/";
        public const string Prefab = Folder + "Level_81_Layout.prefab";
        public const string ScenePath = "Assets/My_Assets/_Project_Spark/Scenes/MainMenu/MainMenu.unity";
        private static readonly string[] Models = {
            "IndustrialReliabilityArenaBase","TransparentDigitalFaultObservatory","FourChipletMissionComputerCarrier","CompletedPhotonicChipletComputer","OpticalTelemetryDock","HolographicDeepSpaceMissionDisplay","VirtualParticleStormProjector","TransparentFaultInjectionChamber","ParticleEventPatternDirector","DigitalFaultInjectionController","FaultEventTimelineOscilloscope","SimulatedParticleFluxMonitor","HolographicEventSpatialMap","GiantMemoryBitStateHologram","HardenedMemorySubsystemRack","ECCMemoryBankA","ECCMemoryBankB","MemoryAddressDecoder","SECDEDParityEncoder","ECCSyndromeDetectionEngine","SingleBitCorrectionEngine","DoubleBitErrorAlarmAnalyzer","BackgroundMemoryScrubber","ScrubCycleSequencer","MemoryBandwidthLoadMeter","MemoryPageQuarantineModule","ReadOnlyGoldenImageVault","CheckpointRecoveryController","HolographicCheckpointJournal","RedundantComputeCoreA","RedundantComputeCoreB","RedundantComputeCoreC","TripleLaneExecutionSupervisor","TripleModularMajorityVoter","VoterDiscrepancyAnalyzer","ComputeLaneHealthProbeArray","IndependentReferenceComputeUnit","DeterministicMissionTaskScheduler","HardwareWatchdogTimer","WatchdogChallengeResponseModule","SafeMissionCommandGate","ResilientInterchipCommunicationFabric","PacketCRCEncoder","PacketCRCVerificationEngine","PacketSequenceNumberMonitor","OpticalLinkErrorTelemetryBridge","PacketRetransmissionBuffer","TelemetryFrameIntegrityAnalyzer","ProtectedTelemetryMemoryBuffer","MissionSensorInputEmulator","NavigationStateEstimator","SpacecraftAttitudeActuatorSimulator","GuidanceCommandValidationUnit","CorrelatedFaultPatternAnalyzer","ErrorPropagationTraceAnalyzer","MissionSafetyPLCRack","CriticalECCFeedbackSupervisor","ProtectedPowerDistributionCabinet","Regulated5VControlSupply","GuardedFaultInjectionConsole","EmergencyMissionSafeHoldStation","HolographicDependabilitySCADA","ECCCorrectionArcHologram","TripleLaneVotingHologram","PacketIntegrityFlowDisplay","HolographicMissionHealthDisplay","FaultEventReplayChronicle","IndependentChecksumReferenceModule","PrecisionDigitalProbeBank","MissionReliabilityCertificationBeacon"
        };
        private const string Materials = Folder + "Models/Materials";
        internal const string MountsName = "Equipment terminal panels";
        private const string FixtureName = "L81_HD_VISUAL_ART";
        private const string BaseModule = "L81_IndustrialReliabilityArenaBase";

        private static readonly (string id, string label)[] Sockets =
        {
            ("SUPPLY_POS","SUP POS"),
            ("PLC_5V","PLC 5V"),
            ("SUPPLY_RET","SUP RET"),
            ("PLC_GND","PLC GND"),
            ("ECC_5V","ECC 5V"),
            ("ECC_GND","ECC GND"),
            ("MEM_5V","MEM 5V"),
            ("MEM_GND","MEM GND"),
            ("TMR_5V","TMR 5V"),
            ("TMR_GND","TMR GND"),
            ("OPT_5V","OPT 5V"),
            ("OPT_GND","OPT GND"),
            ("TASK_READY","TASK READY"),
            ("SEQ_READY","SEQ READY"),
            ("CORE_A_CLOCK","CORE A CLOCK"),
            ("CORE_A_CLK","CORE A CLK"),
            ("CORE_B_CLOCK","CORE B CLOCK"),
            ("CORE_B_CLK","CORE B CLK"),
            ("CORE_C_CLOCK","CORE C CLOCK"),
            ("CORE_C_CLK","CORE C CLK"),
            ("WD_SYNC","WD SYNC"),
            ("WD_CLOCK","WD CLOCK"),
            ("WD_STATUS","WD STATUS"),
            ("WD_TIME","WD TIME"),
            ("WD_REPLY","WD REPLY"),
            ("WD_VERIFY","WD VERIFY"),
            ("SCHED_PERMIT","SCHED PERMIT"),
            ("MISSION_SEQ","MISSION SEQ"),
            ("MEM_ADDR","MEM ADDR"),
            ("ECC_ADDR","ECC ADDR"),
            ("BANK_A_ADDR","BANK A ADDR"),
            ("MEM_A_ADDR","MEM A ADDR"),
            ("BANK_B_ADDR","BANK B ADDR"),
            ("MEM_B_ADDR","MEM B ADDR"),
            ("LOCKSTEP_OUT","LOCKSTEP OUT"),
            ("SEQ_SYNC","SEQ SYNC"),
            ("REF_READY","REF READY"),
            ("REF_VALID","REF VALID"),
            ("COMMAND_PERMIT","CMD PERMIT"),
            ("COMMAND_ENABLE","CMD ENABLE"),
            ("GUIDANCE_CMD","GUIDANCE CMD"),
            ("COMPUTE_IN","COMPUTE IN"),
            ("DEADLINE_OUT","DEADLINE OUT"),
            ("DEADLINE_IN","DEADLINE IN"),
            ("MEM_WORD","MEM WORD"),
            ("ECC_ENCODE","ECC ENCODE"),
            ("ECC_A_OUT","ECC A OUT"),
            ("BANK_A_WRITE","BANK A WRITE"),
            ("ECC_B_OUT","ECC B OUT"),
            ("BANK_B_WRITE","BANK B WRITE"),
            ("BANK_A_READ","BANK A READ"),
            ("SYNDROME_A","SYN A"),
            ("BANK_B_READ","BANK B READ"),
            ("SYNDROME_B","SYN B"),
            ("SINGLE_POS","SINGLE POS"),
            ("CORRECT_IN","CORR IN"),
            ("CORRECT_A","CORR A"),
            ("BANK_A_FIX","BANK A FIX"),
            ("CORRECT_B","CORR B"),
            ("BANK_B_FIX","BANK B FIX"),
            ("DOUBLE_BIT","DOUBLE BIT"),
            ("DOUBLE_ALARM","DOUBLE ALARM"),
            ("QUARANTINE_CMD","QUAR CMD"),
            ("QUARANTINE_IN","QUAR IN"),
            ("SCRUB_RATE","SCRUB RATE"),
            ("SCRUB_CLOCK","SCRUB CLOCK"),
            ("SCRUB_ADDR","SCRUB ADDR"),
            ("SCRUB_BANK","SCRUB BANK"),
            ("SCRUB_QUERY","SCRUB QUERY"),
            ("SCRUB_SYNDROME","SCRUB SYN"),
            ("SCRUB_RESULT","SCRUB RESULT"),
            ("CORR_RESULT","CORR RESULT"),
            ("SCRUB_BW","SCRUB BW"),
            ("SCRUB_ALLOW","SCRUB ALLOW"),
            ("SCRUB_LOAD","SCRUB LOAD"),
            ("BW_TELEM","BW TELEM"),
            ("CHECKPOINT_CMD","CKPT CMD"),
            ("CHECKPOINT_IN","CKPT IN"),
            ("VAULT_AUTH","VAULT AUTH"),
            ("BACKUP_ENABLE","BACKUP ENABLE"),
            ("RECHECK_OUT","RECHECK OUT"),
            ("PAGE_VERIFY","PAGE VERIFY"),
            ("CHECKSUM_OUT","CHECKSUM OUT"),
            ("VAULT_REF","VAULT REF"),
            ("LOCK_A","LOCK A"),
            ("LANE_A_IN","LANE A IN"),
            ("LOCK_B","LOCK B"),
            ("LANE_B_IN","LANE B IN"),
            ("LOCK_C","LOCK C"),
            ("LANE_C_IN","LANE C IN"),
            ("RESULT_A","RESULT A"),
            ("VOTE_A","VOTE A"),
            ("RESULT_B","RESULT B"),
            ("VOTE_B","VOTE B"),
            ("RESULT_C","RESULT C"),
            ("VOTE_C","VOTE C"),
            ("DISAGREE_OUT","DISAGREE OUT"),
            ("DISAGREE_IN","DISAGREE IN"),
            ("FAULT_LANE","FAULT LANE"),
            ("LANE_ISOLATE","LANE ISOLATE"),
            ("LANE_A_HEALTH","LANE A HEALTH"),
            ("PLC_A_HEALTH","PLC A HEALTH"),
            ("LANE_B_HEALTH","LANE B HEALTH"),
            ("PLC_B_HEALTH","PLC B HEALTH"),
            ("LANE_C_HEALTH","LANE C HEALTH"),
            ("PLC_C_HEALTH","PLC C HEALTH"),
            ("INDEP_REF","INDEP REF"),
            ("REF_CHECK","REF CHECK"),
            ("CORRELATED_OUT","CORR OUT"),
            ("CORRELATED_IN","CORR IN"),
            ("VOTE_PERMIT","VOTE PERMIT"),
            ("VOTE_AUTH","VOTE AUTH"),
            ("VOTE_RESULT","VOTE RESULT"),
            ("COMMAND_VOTE","CMD VOTE"),
            ("REF_GATE","REF GATE"),
            ("VALID_CMD","VALID CMD"),
            ("CMD_SAFE","CMD SAFE"),
            ("ACT_CMD","ACT CMD"),
            ("ACTUATE","ACTUATE"),
            ("SENSOR_PACKET","SENSOR PACKET"),
            ("SENSOR_IN","SENSOR IN"),
            ("NAV_PACKET","NAV PACKET"),
            ("NAV_IN","NAV IN"),
            ("CRC_FRAME","CRC FRAME"),
            ("OPTICAL_TX","OPTICAL TX"),
            ("OPTICAL_RX","OPTICAL RX"),
            ("FRAME_IN","FRAME IN"),
            ("CRC_OK","CRC OK"),
            ("SEQUENCE_IN","SEQUENCE IN"),
            ("SEQUENCE_OUT","SEQUENCE OUT"),
            ("ORDER_VALID","ORDER VALID"),
            ("CRC_STATUS","CRC STATUS"),
            ("CRC_VALID","CRC VALID"),
            ("GAP_REQ","GAP REQ"),
            ("RETRANSMIT","RETRANSMIT"),
            ("RETRY_OUT","RETRY OUT"),
            ("RETRY_IN","RETRY IN"),
            ("GOOD_PACKET","GOOD PACKET"),
            ("BUFFER_IN","BUFFER IN"),
            ("NAV_DATA","NAV DATA"),
            ("NAV_VALID","NAV VALID"),
            ("OPTICAL_VALID","OPTICAL VALID"),
            ("LINK_STATUS","LINK STATUS"),
            ("TELEMETRY_PERMIT","TEL PERMIT"),
            ("TELEMETRY_AUTH","TEL AUTH"),
            ("PACKET_AGE","PACKET AGE"),
            ("DROP_FRAME","DROP FRAME"),
            ("REPLAY_HOLD","REPLAY HOLD"),
            ("SENSOR_FRESH","SENSOR FRESH"),
            ("SCADA_INTEGRITY","SCADA INTEGRITY"),
            ("INJECT_CMD","INJECT CMD"),
            ("FAULT_SEED","FAULT SEED"),
            ("BANK_A_UPSET","BANK A UPSET"),
            ("BANK_B_UPSET","BANK B UPSET"),
            ("CORE_A_FAULT","CORE A FAULT"),
            ("CORE_B_FAULT","CORE B FAULT"),
            ("CORE_C_FAULT","CORE C FAULT"),
            ("CORREL_EVENT","CORREL EVENT"),
            ("CORREL_PATTERN","CORREL PATTERN"),
            ("PROPAGATION_RISK","PROP RISK"),
            ("ECC_DIAG_ALARM","ECC DIAG ALARM"),
            ("ECC_SYNDROME_STATUS_OUT","ECC SYN OUT"),
            ("CRITICAL_ECC_SYNDROME_IN","CRIT ECC IN"),
            ("ECC_HEALTH","ECC HEALTH"),
            ("ECC_DISPLAY","ECC DISPLAY"),
            ("TRACE_DIAG","TRACE DIAG"),
            ("COMPUTE_INHIBIT","COMPUTE INHIBIT"),
            ("PACKET_FAULT","PACKET FAULT"),
            ("SCADA_FAULTS","SCADA FAULTS"),
            ("WATCHDOG_EXPIRED","WD EXPIRED"),
            ("WATCHDOG_TRUST","WD TRUST"),
            ("SAFE_HOLD","SAFE HOLD"),
            ("RECOVERY_APPROVAL","RCV APPROVAL"),
            ("RESTORE_SUM","RESTORE SUM"),
            ("SUM_COMPARE","SUM COMPARE"),
            ("QUARANTINE_STATUS","QUAR STATUS"),
            ("WORD_FRESHNESS","WORD AGE"),
            ("ECC_FRESHNESS","ECC AGE"),
            ("RECERT_REQUEST","RECERT REQUEST"),
            ("RECOVERY_OK","RCV OK"),
            ("VOTE_CONFIDENCE","VOTE CONF"),
            ("REF_QUAL","REF QUAL"),
            ("PACKET_QUAL","PACKET QUAL"),
            ("FAULT_CONSOLE","FAULT CONSOLE"),
            ("MANUAL_RESET","MANUAL RESET"),
            ("PROTECTED_CONTROL","PROT CONTROL"),
            ("SUPPLY_STATUS","SUP STATUS"),
            ("ESTOP_NC","ESTOP NC"),
            ("SAFE_STOP_IN","SAFE STOP IN"),
            ("HOLD_FEEDBACK","HOLD FEEDBACK"),
            ("INJECT_PERMIT","INJECT PERMIT"),
            ("SCADA_PLC","SCADA PLC"),
            ("SCADA_ECC","SCADA ECC"),
            ("SCADA_QUARANTINE","SCADA QUAR"),
            ("SCADA_SCRUB","SCADA SCRUB"),
            ("SCADA_TMR","SCADA TMR"),
            ("SCADA_TIMING","SCADA TIMING"),
            ("SCADA_PACKET","SCADA PACKET"),
            ("MISSION_HEALTH","MISSION HEALTH"),
            ("OBJECTIVE_CERT","OBJ CERT"),
        };

        public static IEnumerable<string> TerminalIds => Sockets.Select(socket => socket.id);

        private static readonly Dictionary<string, string> Palette = new()
        {
            {"red","EE485C"},{"blue","1711E2"},{"amber","FFA82B"},{"violet","A282FF"},{"plasma","A45DFF"},
            {"mint","34F6C5"},{"cyan","23DAFD"},{"void","07111C"},{"white","D4F5FF"},{"glass","498BA8"}
        };

        [MenuItem("Tools/Project Spark/Levels/Create Level 81")]
        public static void Install()
        {
            SparkLevel81Builder.Create();
            var root = AssetDatabase.LoadAssetAtPath<GameObject>(Prefab);
            var level = root ? root.GetComponent<SparkLevel81Process>() : null;
            if (!level || !level.controllerDisplay || level.pinLabels.Length != 200) Fit();
            Register();
            ValidateAssets();
        }

        [MenuItem("Tools/Project Spark/Level 81/Validate Level Assets")]
        public static void ValidateAssets()
        {
            var root=AssetDatabase.LoadAssetAtPath<GameObject>(Prefab);
            var control=root != null ? root.GetComponent<SparkLevel81Process>() : null;
            var bindings=root != null ? root.GetComponent<SparkLevelSceneBindings>() : null;
            if (control==null || bindings==null || control.controllerDisplay==null || control.qualityDisplay==null || control.rig==null || control.rig.pivots.Length!=52 ||
                control.pins.Length!=200 || control.pins.Distinct().Count()!=200 ||
                control.pinLabels.Length!=200 || control.pinLabels.Any(label=>!label) ||
                !TerminalIds.SequenceEqual(control.pins.Select(pin=>pin.name)) ||
                control.pins.Any(pin=>!bindings.TryGetTerminal(pin.name,out var bound) || bound!=pin) ||
                SparkLevel81Process.Gates.Any(id=>!bindings.IsComponentIdDeclared(id)))
                throw new InvalidOperationException("Level 81 wiring, motion or binding references are incomplete.");
            Debug.Log("[LEVEL 81] Validated 200 distinct terminals, 130 player wires and 6 evaluator gates.",root);
        }

        private static void Register()
        {
            var scene = SceneManager.GetSceneByPath(ScenePath);
            bool opened = !scene.isLoaded;
            if (opened) scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
            bool dirty = scene.isDirty;
            var objects = scene.GetRootGameObjects();
            T Find<T>() where T : Component => objects.SelectMany(item => item.GetComponentsInChildren<T>(true)).Single();
            var level = AssetDatabase.LoadAssetAtPath<SparkLevelDefinition>(Folder + "Level_81.asset");
            var drawing = AssetDatabase.LoadAssetAtPath<SparkLevelDrawingAsset>(Folder + "Level_81_Drawing.asset");
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
            else if (dirty) Debug.Log("MainMenu had unsaved edits. Save the scene to persist Level 81's campaign and teaching entries.");
        }

        private static void Append(Object target, string field, Object value)
        {
            if (value == null) throw new InvalidOperationException("Missing Level 81 asset for " + field);
            var data = new SerializedObject(target);
            var list = data.FindProperty(field);
            for (int i = 0; i < list.arraySize; i++)
                if (list.GetArrayElementAtIndex(i).objectReferenceValue == value) return;
            list.arraySize++;
            list.GetArrayElementAtIndex(list.arraySize - 1).objectReferenceValue = value;
            data.ApplyModifiedProperties();
        }

        [MenuItem("Tools/Project Spark/Level 81/Fit Equipment Terminals")]
        public static void Fit()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Exit Play Mode before fitting Level 81.");
            var models = Models.Select((name, index) =>
            {
                string path = Folder + $"Models/L81_{index:00}_{name}_Fitted.fbx";
                return AssetDatabase.LoadAssetAtPath<GameObject>(path) ??
                    throw new InvalidOperationException("Missing fitted kit module " + path);
            }).ToArray();
            if (AssetDatabase.LoadAssetAtPath<GameObject>(Prefab) == null)
                throw new InvalidOperationException("The Level 81 prefab is missing.");

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
                    throw new InvalidOperationException("Unity could not save the Level 81 layout.");
                AssetDatabase.SaveAssets();
                Debug.Log("Level 81 fitted to the workshop table with 200 original equipment terminals.");
            }
            finally
            {
                if (root != null) PrefabUtility.UnloadPrefabContents(root);
                if (opened) EditorSceneManager.CloseScene(scene, true);
            }
        }

        private static void Fit(GameObject root, GameObject[] models, BoxCollider table, Matrix4x4 layout)
        {
            var bench = root.transform.Find("Reliability Workbench");
            var bindings = root.GetComponent<SparkLevelSceneBindings>();
            var loop = root.GetComponent<SparkLevel81Process>();
            if (bench == null || bindings == null || loop == null)
                throw new InvalidOperationException("Level 81 is missing its workbench, controls, or scene bindings.");

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
                instance.name = "L81_" + Models[index];
                var markers = instance.GetComponentsInChildren<Transform>(true)
                    .Where(item => item.name.StartsWith("L81_Board",StringComparison.Ordinal))
                    .ToDictionary(item => item.name);
                Transform Marker(string name) => markers["L81_Board" + name];
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
                if(key=="tint1D303F" && mesh.name.StartsWith("L81_TransparentDigitalFaultObservatory_",StringComparison.Ordinal)) key="glass";
                string surface=key switch
                {
                    "glass" when mesh.name.Contains("FaultInjectionChamber")=>"FaultGlass",
                    "glass"=>"OpticalGlass",
                    "tint162230" when mesh.name.Contains("ECCMemoryBank")=>"MemoryPCB",
                    "tintC2D0D6" when mesh.name.Contains("GoldenImageVault")=>"VaultAlloy",
                    "tintE9BC56" when mesh.name.Contains("OpticalTelemetryDock")=>"CopperBus",
                    "tintE9BC56"=>"GoldContact",
                    "tint47657C" or "tintC2D0D6" or "tintBCD0E2"=>"BrushedSteel",
                    "tint162230"=>"Elastomer",
                    "tint0C1620"=>"BlackCeramic",
                    "tint0D1D2F" when mesh.name.Contains("ArenaBase")=>"CleanroomFloor",
                    "tint0D1D2F" when mesh.name.Contains("CompletedPhotonic")=>"SiliconInterposer",
                    "tint0D1D2F" or "tint1D303F"=>"AnodizedPanel",
                    _=>null
                };
                var material = MaterialFor(shader,key,surface);
                // The fitted meshes can contain several imported submeshes.
                mesh.sharedMaterials = Enumerable.Repeat(material, mesh.GetComponent<MeshFilter>().sharedMesh.subMeshCount).ToArray();
                if (key is "glass" or "acrylic") mesh.shadowCastingMode = ShadowCastingMode.Off;
            }

            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (!loop.calibrationText || !loop.focusWireButton || !loop.plot || !loop.plot.GetComponent<CanvasRenderer>())
            {
                var oldControls = root.transform.Find("Level 81 Controls");
                if (oldControls) Object.DestroyImmediate(oldControls.gameObject);
                SparkLevel81Builder.BuildControls(root.transform, loop, font);
            }
            var labels = Child(bench, MountsName, Vector3.zero);
            SeatOriginalTerminals(bench, assembly, bindings, loop, labels, font);
            var deck = SparkEquipmentWorkbenchFit.MeasureArt(assembly.Find(BaseModule), bench.worldToLocalMatrix);
            float labelHeight = deck.max.y + .012f;
            float labelDepth = deck.min.z + .034f;
            var title = Label(labels, font, "Level name", "OUTLAST THE BITSTORM", new Vector3(0, labelHeight, labelDepth), .024f);
            title.color = new Color(.47f, .88f, 1f);
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
            Lamp("Mission work light",new Vector3(-.55f,.64f,-.21f),new Color(.91f,.94f,1),2.50f,4.6f);
            Lamp("Metrology fill",new Vector3(.58f,.44f,.17f),new Color(.52f,.78f,1),1.75f,3.7f);
            loop.rig.BindEffects(
                Lamp("ECC activity light",new Vector3(-.37f,.20f,-.03f),new Color(.29f,.75f,1),1,.45f),
                Lamp("TMR activity light",new Vector3(0,.20f,-.03f),new Color(.48f,.39f,1),1,.46f),
                Lamp("Telemetry activity light",new Vector3(.28f,.2f,-.03f),new Color(.58f,.37f,1),1,.45f),
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
            loop.controllerDisplay=Readout("ECC readout",new Vector3(-.87f,labelHeight,labelDepth),new Color(.46f,.94f,1f));
            loop.qualityDisplay=Readout("Mission status",new Vector3(.87f,labelHeight,labelDepth),new Color(.71f,1f,.86f));
        }

        private static void SeatOriginalTerminals(Transform bench, Transform assembly, SparkLevelSceneBindings bindings,
            SparkLevel81Process loop, Transform panels, Font font)
        {
            var data = AssetDatabase.LoadAssetAtPath<TextAsset>(Folder + "Equipment_Terminal_Mounts.csv");
            if (!data) throw new InvalidOperationException("The original Level 81 mount mapping is missing.");
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
                mount.position = assembly.TransformPoint(SparkEquipmentWorkbenchFit.SourcePosition(values));
                mount.rotation = bench.rotation;
                var jack = terminal.transform;
                jack.SetParent(mount, false);
                jack.localRotation = Quaternion.identity;
                jack.localScale = Vector3.one;
                // Place the pin at the face of the source socket insert.
                jack.localPosition = bench.InverseTransformVector(assembly.TransformVector(Vector3.back * (.0425f * SparkEquipmentWorkbenchFit.SourceArtUnit)));
                jack.gameObject.layer = pinLayer;
                float diameter = bench.InverseTransformVector(assembly.TransformVector(Vector3.right * (.120f * SparkEquipmentWorkbenchFit.SourceArtUnit))).magnitude;
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
                pinData.FindProperty("maxConnections").intValue = SparkLevel81Process.Wires.Count(wire => wire.a == index || wire.b == index);
                pinData.ApplyModifiedPropertiesWithoutUndo();

                var tag = Label(mount, font, "Label " + terminal.name, specs[terminal.name].label, new Vector3(0, .022f, -.009f), .019f);
                tag.transform.localRotation = Quaternion.Euler(30, 0, 0);
                tag.gameObject.SetActive(false);
                loop.pinLabels[index] = tag;
            }
            if (assigned.Count != loop.pins.Length || loop.pinLabels.Any(label => !label))
                throw new InvalidOperationException("Level 81 must map every circuit terminal to its own original equipment socket.");
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
            string materialName=surface==null?"L81 "+key:"L81 "+key+" "+surface;
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
                    throw new InvalidOperationException("Missing Level 81 surface maps for "+surface);
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



















