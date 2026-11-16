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
        private const float SocketHeight = .025f;
        private const float SocketDiameter = .026f;

        private static readonly (string id, string label, float x, float z, string color)[] Sockets =
        {
            ("SUPPLY_POS","SUP POS",-1.0000f,0.6800f,"red"),
            ("PLC_5V","PLC 5V",-0.9340f,0.6800f,"red"),
            ("SUPPLY_RET","SUP RET",-0.8680f,0.6800f,"blue"),
            ("PLC_GND","PLC GND",-0.8020f,0.6800f,"blue"),
            ("ECC_5V","ECC 5V",-0.7360f,0.6800f,"red"),
            ("ECC_GND","ECC GND",-0.6700f,0.6800f,"blue"),
            ("MEM_5V","MEM 5V",-0.6040f,0.6800f,"red"),
            ("MEM_GND","MEM GND",-0.5380f,0.6800f,"blue"),
            ("TMR_5V","TMR 5V",-0.4720f,0.6800f,"red"),
            ("TMR_GND","TMR GND",-0.4060f,0.6800f,"blue"),
            ("OPT_5V","OPT 5V",-0.3400f,0.6800f,"red"),
            ("OPT_GND","OPT GND",-0.2740f,0.6800f,"blue"),
            ("TASK_READY","TASK READY",-0.2080f,0.6800f,"mint"),
            ("SEQ_READY","SEQ READY",-0.1420f,0.6800f,"mint"),
            ("CORE_A_CLOCK","CORE A CLOCK",-0.0760f,0.6800f,"mint"),
            ("CORE_A_CLK","CORE A CLK",-0.0100f,0.6800f,"mint"),
            ("CORE_B_CLOCK","CORE B CLOCK",0.0560f,0.6800f,"mint"),
            ("CORE_B_CLK","CORE B CLK",0.1220f,0.6800f,"mint"),
            ("CORE_C_CLOCK","CORE C CLOCK",0.1880f,0.6800f,"mint"),
            ("CORE_C_CLK","CORE C CLK",0.2540f,0.6800f,"mint"),
            ("WD_SYNC","WD SYNC",0.3200f,0.6800f,"mint"),
            ("WD_CLOCK","WD CLOCK",0.3860f,0.6800f,"mint"),
            ("WD_STATUS","WD STATUS",0.4520f,0.6800f,"mint"),
            ("WD_TIME","WD TIME",0.5180f,0.6800f,"mint"),
            ("WD_REPLY","WD REPLY",0.5840f,0.6800f,"mint"),
            ("WD_VERIFY","WD VERIFY",0.6500f,0.6800f,"mint"),
            ("SCHED_PERMIT","SCHED PERMIT",0.7160f,0.6800f,"amber"),
            ("MISSION_SEQ","MISSION SEQ",0.7820f,0.6800f,"mint"),
            ("MEM_ADDR","MEM ADDR",0.8480f,0.6800f,"mint"),
            ("ECC_ADDR","ECC ADDR",0.9140f,0.6800f,"mint"),
            ("BANK_A_ADDR","BANK A ADDR",0.9800f,0.6800f,"mint"),
            ("MEM_A_ADDR","MEM A ADDR",-1.0000f,0.5450f,"mint"),
            ("BANK_B_ADDR","BANK B ADDR",-0.9340f,0.5450f,"mint"),
            ("MEM_B_ADDR","MEM B ADDR",-0.8680f,0.5450f,"mint"),
            ("LOCKSTEP_OUT","LOCKSTEP OUT",-0.8020f,0.5450f,"mint"),
            ("SEQ_SYNC","SEQ SYNC",-0.7360f,0.5450f,"mint"),
            ("REF_READY","REF READY",-0.6700f,0.5450f,"mint"),
            ("REF_VALID","REF VALID",-0.6040f,0.5450f,"mint"),
            ("COMMAND_PERMIT","CMD PERMIT",-0.5380f,0.5450f,"amber"),
            ("COMMAND_ENABLE","CMD ENABLE",-0.4720f,0.5450f,"mint"),
            ("GUIDANCE_CMD","GUIDANCE CMD",-0.4060f,0.5450f,"mint"),
            ("COMPUTE_IN","COMPUTE IN",-0.3400f,0.5450f,"mint"),
            ("DEADLINE_OUT","DEADLINE OUT",-0.2740f,0.5450f,"mint"),
            ("DEADLINE_IN","DEADLINE IN",-0.2080f,0.5450f,"mint"),
            ("MEM_WORD","MEM WORD",-0.1420f,0.5450f,"mint"),
            ("ECC_ENCODE","ECC ENCODE",-0.0760f,0.5450f,"mint"),
            ("ECC_A_OUT","ECC A OUT",-0.0100f,0.5450f,"mint"),
            ("BANK_A_WRITE","BANK A WRITE",0.0560f,0.5450f,"mint"),
            ("ECC_B_OUT","ECC B OUT",0.1220f,0.5450f,"mint"),
            ("BANK_B_WRITE","BANK B WRITE",0.1880f,0.5450f,"mint"),
            ("BANK_A_READ","BANK A READ",0.2540f,0.5450f,"mint"),
            ("SYNDROME_A","SYN A",0.3200f,0.5450f,"mint"),
            ("BANK_B_READ","BANK B READ",0.3860f,0.5450f,"mint"),
            ("SYNDROME_B","SYN B",0.4520f,0.5450f,"mint"),
            ("SINGLE_POS","SINGLE POS",0.5180f,0.5450f,"mint"),
            ("CORRECT_IN","CORR IN",0.5840f,0.5450f,"mint"),
            ("CORRECT_A","CORR A",0.6500f,0.5450f,"mint"),
            ("BANK_A_FIX","BANK A FIX",0.7160f,0.5450f,"mint"),
            ("CORRECT_B","CORR B",0.7820f,0.5450f,"mint"),
            ("BANK_B_FIX","BANK B FIX",0.8480f,0.5450f,"mint"),
            ("DOUBLE_BIT","DOUBLE BIT",0.9140f,0.5450f,"mint"),
            ("DOUBLE_ALARM","DOUBLE ALARM",0.9800f,0.5450f,"mint"),
            ("QUARANTINE_CMD","QUAR CMD",-1.0000f,0.4250f,"mint"),
            ("QUARANTINE_IN","QUAR IN",-0.9350f,0.4250f,"mint"),
            ("SCRUB_RATE","SCRUB RATE",-0.8700f,0.4250f,"mint"),
            ("SCRUB_CLOCK","SCRUB CLOCK",-0.8050f,0.4250f,"mint"),
            ("SCRUB_ADDR","SCRUB ADDR",-0.7400f,0.4250f,"mint"),
            ("SCRUB_BANK","SCRUB BANK",-0.6750f,0.4250f,"mint"),
            ("SCRUB_QUERY","SCRUB QUERY",-0.6100f,0.4250f,"mint"),
            ("SCRUB_SYNDROME","SCRUB SYN",-0.5450f,0.4250f,"mint"),
            ("SCRUB_RESULT","SCRUB RESULT",-0.4800f,0.4250f,"mint"),
            ("CORR_RESULT","CORR RESULT",-0.4150f,0.4250f,"mint"),
            ("SCRUB_BW","SCRUB BW",-0.3500f,0.4250f,"mint"),
            ("SCRUB_ALLOW","SCRUB ALLOW",-0.2850f,0.4250f,"mint"),
            ("SCRUB_LOAD","SCRUB LOAD",-0.2200f,0.4250f,"mint"),
            ("BW_TELEM","BW TELEM",-0.1550f,0.4250f,"mint"),
            ("CHECKPOINT_CMD","CKPT CMD",-0.0900f,0.4250f,"mint"),
            ("CHECKPOINT_IN","CKPT IN",-0.0250f,0.4250f,"mint"),
            ("VAULT_AUTH","VAULT AUTH",0.0400f,0.4250f,"mint"),
            ("BACKUP_ENABLE","BACKUP ENABLE",0.1050f,0.4250f,"mint"),
            ("RECHECK_OUT","RECHECK OUT",0.1700f,0.4250f,"mint"),
            ("PAGE_VERIFY","PAGE VERIFY",0.2350f,0.4250f,"mint"),
            ("CHECKSUM_OUT","CHECKSUM OUT",0.3000f,0.4250f,"mint"),
            ("VAULT_REF","VAULT REF",0.3650f,0.4250f,"mint"),
            ("LOCK_A","LOCK A",0.4300f,0.4250f,"mint"),
            ("LANE_A_IN","LANE A IN",0.4950f,0.4250f,"mint"),
            ("LOCK_B","LOCK B",0.5600f,0.4250f,"mint"),
            ("LANE_B_IN","LANE B IN",0.6250f,0.4250f,"mint"),
            ("LOCK_C","LOCK C",0.6900f,0.4250f,"mint"),
            ("LANE_C_IN","LANE C IN",0.7550f,0.4250f,"mint"),
            ("RESULT_A","RESULT A",0.8200f,0.4250f,"mint"),
            ("VOTE_A","VOTE A",0.8850f,0.4250f,"mint"),
            ("RESULT_B","RESULT B",0.9500f,0.4250f,"mint"),
            ("VOTE_B","VOTE B",1.0150f,0.4250f,"mint"),
            ("RESULT_C","RESULT C",-1.0000f,-0.3550f,"mint"),
            ("VOTE_C","VOTE C",-0.9410f,-0.3550f,"mint"),
            ("DISAGREE_OUT","DISAGREE OUT",-0.8820f,-0.3550f,"mint"),
            ("DISAGREE_IN","DISAGREE IN",-0.8230f,-0.3550f,"mint"),
            ("FAULT_LANE","FAULT LANE",-0.7640f,-0.3550f,"amber"),
            ("LANE_ISOLATE","LANE ISOLATE",-0.7050f,-0.3550f,"mint"),
            ("LANE_A_HEALTH","LANE A HEALTH",-0.6460f,-0.3550f,"mint"),
            ("PLC_A_HEALTH","PLC A HEALTH",-0.5870f,-0.3550f,"mint"),
            ("LANE_B_HEALTH","LANE B HEALTH",-0.5280f,-0.3550f,"mint"),
            ("PLC_B_HEALTH","PLC B HEALTH",-0.4690f,-0.3550f,"mint"),
            ("LANE_C_HEALTH","LANE C HEALTH",-0.4100f,-0.3550f,"mint"),
            ("PLC_C_HEALTH","PLC C HEALTH",-0.3510f,-0.3550f,"mint"),
            ("INDEP_REF","INDEP REF",-0.2920f,-0.3550f,"mint"),
            ("REF_CHECK","REF CHECK",-0.2330f,-0.3550f,"mint"),
            ("CORRELATED_OUT","CORR OUT",-0.1740f,-0.3550f,"mint"),
            ("CORRELATED_IN","CORR IN",-0.1150f,-0.3550f,"mint"),
            ("VOTE_PERMIT","VOTE PERMIT",-0.0560f,-0.3550f,"amber"),
            ("VOTE_AUTH","VOTE AUTH",0.0030f,-0.3550f,"mint"),
            ("VOTE_RESULT","VOTE RESULT",0.0620f,-0.3550f,"mint"),
            ("COMMAND_VOTE","CMD VOTE",0.1210f,-0.3550f,"mint"),
            ("REF_GATE","REF GATE",0.1800f,-0.3550f,"mint"),
            ("VALID_CMD","VALID CMD",0.2390f,-0.3550f,"mint"),
            ("CMD_SAFE","CMD SAFE",0.2980f,-0.3550f,"amber"),
            ("ACT_CMD","ACT CMD",0.3570f,-0.3550f,"mint"),
            ("ACTUATE","ACTUATE",0.4160f,-0.3550f,"mint"),
            ("SENSOR_PACKET","SENSOR PACKET",0.4750f,-0.3550f,"mint"),
            ("SENSOR_IN","SENSOR IN",0.5340f,-0.3550f,"mint"),
            ("NAV_PACKET","NAV PACKET",0.5930f,-0.3550f,"mint"),
            ("NAV_IN","NAV IN",0.6520f,-0.3550f,"mint"),
            ("CRC_FRAME","CRC FRAME",0.7110f,-0.3550f,"mint"),
            ("OPTICAL_TX","OPTICAL TX",0.7700f,-0.3550f,"mint"),
            ("OPTICAL_RX","OPTICAL RX",0.8290f,-0.3550f,"mint"),
            ("FRAME_IN","FRAME IN",0.8880f,-0.3550f,"mint"),
            ("CRC_OK","CRC OK",0.9470f,-0.3550f,"mint"),
            ("SEQUENCE_IN","SEQUENCE IN",1.0060f,-0.3550f,"mint"),
            ("SEQUENCE_OUT","SEQUENCE OUT",-1.0000f,-0.5250f,"mint"),
            ("ORDER_VALID","ORDER VALID",-0.9410f,-0.5250f,"mint"),
            ("CRC_STATUS","CRC STATUS",-0.8820f,-0.5250f,"mint"),
            ("CRC_VALID","CRC VALID",-0.8230f,-0.5250f,"mint"),
            ("GAP_REQ","GAP REQ",-0.7640f,-0.5250f,"mint"),
            ("RETRANSMIT","RETRANSMIT",-0.7050f,-0.5250f,"mint"),
            ("RETRY_OUT","RETRY OUT",-0.6460f,-0.5250f,"mint"),
            ("RETRY_IN","RETRY IN",-0.5870f,-0.5250f,"mint"),
            ("GOOD_PACKET","GOOD PACKET",-0.5280f,-0.5250f,"mint"),
            ("BUFFER_IN","BUFFER IN",-0.4690f,-0.5250f,"mint"),
            ("NAV_DATA","NAV DATA",-0.4100f,-0.5250f,"mint"),
            ("NAV_VALID","NAV VALID",-0.3510f,-0.5250f,"mint"),
            ("OPTICAL_VALID","OPTICAL VALID",-0.2920f,-0.5250f,"mint"),
            ("LINK_STATUS","LINK STATUS",-0.2330f,-0.5250f,"mint"),
            ("TELEMETRY_PERMIT","TEL PERMIT",-0.1740f,-0.5250f,"amber"),
            ("TELEMETRY_AUTH","TEL AUTH",-0.1150f,-0.5250f,"mint"),
            ("PACKET_AGE","PACKET AGE",-0.0560f,-0.5250f,"mint"),
            ("DROP_FRAME","DROP FRAME",0.0030f,-0.5250f,"mint"),
            ("REPLAY_HOLD","REPLAY HOLD",0.0620f,-0.5250f,"mint"),
            ("SENSOR_FRESH","SENSOR FRESH",0.1210f,-0.5250f,"mint"),
            ("SCADA_INTEGRITY","SCADA INTEGRITY",0.1800f,-0.5250f,"mint"),
            ("INJECT_CMD","INJECT CMD",0.2390f,-0.5250f,"mint"),
            ("FAULT_SEED","FAULT SEED",0.2980f,-0.5250f,"amber"),
            ("BANK_A_UPSET","BANK A UPSET",0.3570f,-0.5250f,"mint"),
            ("BANK_B_UPSET","BANK B UPSET",0.4160f,-0.5250f,"mint"),
            ("CORE_A_FAULT","CORE A FAULT",0.4750f,-0.5250f,"amber"),
            ("CORE_B_FAULT","CORE B FAULT",0.5340f,-0.5250f,"amber"),
            ("CORE_C_FAULT","CORE C FAULT",0.5930f,-0.5250f,"amber"),
            ("CORREL_EVENT","CORREL EVENT",0.6520f,-0.5250f,"mint"),
            ("CORREL_PATTERN","CORREL PATTERN",0.7110f,-0.5250f,"mint"),
            ("PROPAGATION_RISK","PROP RISK",0.7700f,-0.5250f,"mint"),
            ("ECC_DIAG_ALARM","ECC DIAG ALARM",0.8290f,-0.5250f,"mint"),
            ("ECC_SYNDROME_STATUS_OUT","ECC SYN OUT",0.8880f,-0.5250f,"mint"),
            ("CRITICAL_ECC_SYNDROME_IN","CRIT ECC IN",0.9470f,-0.5250f,"mint"),
            ("ECC_HEALTH","ECC HEALTH",1.0060f,-0.5250f,"mint"),
            ("ECC_DISPLAY","ECC DISPLAY",-1.0000f,-0.7000f,"mint"),
            ("TRACE_DIAG","TRACE DIAG",-0.9420f,-0.7000f,"mint"),
            ("COMPUTE_INHIBIT","COMPUTE INHIBIT",-0.8840f,-0.7000f,"mint"),
            ("PACKET_FAULT","PACKET FAULT",-0.8260f,-0.7000f,"amber"),
            ("SCADA_FAULTS","SCADA FAULTS",-0.7680f,-0.7000f,"amber"),
            ("WATCHDOG_EXPIRED","WD EXPIRED",-0.7100f,-0.7000f,"mint"),
            ("WATCHDOG_TRUST","WD TRUST",-0.6520f,-0.7000f,"mint"),
            ("SAFE_HOLD","SAFE HOLD",-0.5940f,-0.7000f,"amber"),
            ("RECOVERY_APPROVAL","RCV APPROVAL",-0.5360f,-0.7000f,"mint"),
            ("RESTORE_SUM","RESTORE SUM",-0.4780f,-0.7000f,"mint"),
            ("SUM_COMPARE","SUM COMPARE",-0.4200f,-0.7000f,"mint"),
            ("QUARANTINE_STATUS","QUAR STATUS",-0.3620f,-0.7000f,"mint"),
            ("WORD_FRESHNESS","WORD AGE",-0.3040f,-0.7000f,"mint"),
            ("ECC_FRESHNESS","ECC AGE",-0.2460f,-0.7000f,"mint"),
            ("RECERT_REQUEST","RECERT REQUEST",-0.1880f,-0.7000f,"mint"),
            ("RECOVERY_OK","RCV OK",-0.1300f,-0.7000f,"mint"),
            ("VOTE_CONFIDENCE","VOTE CONF",-0.0720f,-0.7000f,"mint"),
            ("REF_QUAL","REF QUAL",-0.0140f,-0.7000f,"mint"),
            ("PACKET_QUAL","PACKET QUAL",0.0440f,-0.7000f,"mint"),
            ("FAULT_CONSOLE","FAULT CONSOLE",0.1020f,-0.7000f,"amber"),
            ("MANUAL_RESET","MANUAL RESET",0.1600f,-0.7000f,"mint"),
            ("PROTECTED_CONTROL","PROT CONTROL",0.2180f,-0.7000f,"mint"),
            ("SUPPLY_STATUS","SUP STATUS",0.2760f,-0.7000f,"mint"),
            ("ESTOP_NC","ESTOP NC",0.3340f,-0.7000f,"amber"),
            ("SAFE_STOP_IN","SAFE STOP IN",0.3920f,-0.7000f,"amber"),
            ("HOLD_FEEDBACK","HOLD FEEDBACK",0.4500f,-0.7000f,"mint"),
            ("INJECT_PERMIT","INJECT PERMIT",0.5080f,-0.7000f,"amber"),
            ("SCADA_PLC","SCADA PLC",0.5660f,-0.7000f,"mint"),
            ("SCADA_ECC","SCADA ECC",0.6240f,-0.7000f,"mint"),
            ("SCADA_QUARANTINE","SCADA QUAR",0.6820f,-0.7000f,"mint"),
            ("SCADA_SCRUB","SCADA SCRUB",0.7400f,-0.7000f,"mint"),
            ("SCADA_TMR","SCADA TMR",0.7980f,-0.7000f,"mint"),
            ("SCADA_TIMING","SCADA TIMING",0.8560f,-0.7000f,"mint"),
            ("SCADA_PACKET","SCADA PACKET",0.9140f,-0.7000f,"mint"),
            ("MISSION_HEALTH","MISSION HEALTH",0.9720f,-0.7000f,"mint"),
            ("OBJECTIVE_CERT","OBJ CERT",1.0300f,-0.7000f,"mint"),
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
            if (root == null || root.GetComponent<SparkLevel81Process>().controllerDisplay == null) Fit();
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

        [MenuItem("Tools/Project Spark/Level 81/Fit Breadboard Assembly")]
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
                var board = FindBoard(scene);
                Physics.SyncTransforms();
                root = PrefabUtility.LoadPrefabContents(Prefab);
                Fit(root, models, board, layoutRoot != null ? layoutRoot.localToWorldMatrix : Matrix4x4.identity);
                if (PrefabUtility.SaveAsPrefabAsset(root, Prefab) == null)
                    throw new InvalidOperationException("Unity could not save the Level 81 layout.");
                AssetDatabase.SaveAssets();
                Debug.Log("Level 81 fitted to the MainMenu breadboard with 200 seated terminals.");
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
            var bench = root.transform.Find("Reliability Workbench");
            var bindings = root.GetComponent<SparkLevelSceneBindings>();
            var loop = root.GetComponent<SparkLevel81Process>();
            if (bench == null || bindings == null || loop == null)
                throw new InvalidOperationException("Level 81 is missing its workbench, controls, or scene bindings.");

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

            foreach (string name in new[] {"L81_HD_VISUAL_ART", "Breadboard Labels", "Motion pivots"})
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
            var assembly = Child(bench, "L81_HD_VISUAL_ART", new Vector3(0,floor,-.010f));
            foreach (var model in models)
            {
                var instance = Object.Instantiate(model, assembly, false).transform;
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

            assembly.localScale = Vector3.one * .48f;
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
            if (loop.calibrationText == null)
            {
                var oldControls = root.transform.Find("Level 81 Controls");
                if (oldControls != null) Object.DestroyImmediate(oldControls.gameObject);
                SparkLevel81Builder.BuildControls(root.transform,loop,font);
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
                terminalData.FindProperty("maxConnections").intValue = SparkLevel81Process.Wires.Count(wire => wire.a == index || wire.b == index);
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






                ("MEMORY / SCRUB",-.865f,.147f),("VOTER / TMR",.862f,.147f),
                ("MISSION / TELEMETRY",-.86f,-.235f),("SAFETY / ECC LINK",.86f,-.235f)
            };
            foreach(var title in titles)
                Label(labels,font,title.title,title.title,new Vector3(title.x,floor+.058f,title.z),.0165f);
            Label(labels,font,"Carrier labels","NAVIGATION  /  MANEUVER  /  DATA RETURN",new Vector3(0,floor+.068f,-.252f),.013f);
            Label(labels,font,"Optic path","FAULT-TOLERANT MISSION COMPUTER",new Vector3(0,floor+.15f,.13f),.012f);
            Transform Pivot(string module,string key)
            {
                var name="fitted_"+module+"_"+key;
                return assembly.GetComponentsInChildren<Transform>(true).Single(item=>item.name==name);
            }
            loop.rig.art=assembly;
            loop.rig.pivots=new[]{
                Pivot("VirtualParticleStormProjector","ANIM_BITSTORM_PROJECTOR_ROTATION"),
                Pivot("ParticleEventPatternDirector","ANIM_EVENT_DIRECTOR_X"),
                Pivot("ParticleEventPatternDirector","ANIM_EVENT_DIRECTOR_Y"),
                Pivot("TransparentFaultInjectionChamber","ANIM_FAULT_INJECTION_CHAMBER_DOOR"),
                Pivot("GuardedFaultInjectionConsole","ANIM_FAULT_CONSOLE_GUARD"),
                Pivot("VirtualParticleStormProjector","ANIM_PARTICLE_EVENT_HOLOGRAM"),
                Pivot("SimulatedParticleFluxMonitor","ANIM_PARTICLE_FLUX_MONITOR"),
                Pivot("HolographicEventSpatialMap","ANIM_EVENT_SPATIAL_MAP"),
                Pivot("FaultEventTimelineOscilloscope","ANIM_EVENT_TIMELINE_SCAN"),
                Pivot("FaultEventReplayChronicle","ANIM_FAULT_REPLAY_CHRONICLE"),
                Pivot("ECCMemoryBankA","ANIM_MEMORY_BANK_A_CARTRIDGE"),
                Pivot("ECCMemoryBankB","ANIM_MEMORY_BANK_B_CARTRIDGE"),
                Pivot("MemoryAddressDecoder","ANIM_MEMORY_ADDRESS_DECODER"),
                Pivot("SECDEDParityEncoder","ANIM_SECDED_PARITY_ENCODER"),
                Pivot("ECCSyndromeDetectionEngine","ANIM_ECC_SYNDROME_SCAN"),
                Pivot("SingleBitCorrectionEngine","ANIM_SINGLE_BIT_CORRECTION"),
                Pivot("DoubleBitErrorAlarmAnalyzer","ANIM_DOUBLE_BIT_ERROR_ALARM"),
                Pivot("BackgroundMemoryScrubber","ANIM_SCRUBBER_SCAN_HEAD"),
                Pivot("ScrubCycleSequencer","ANIM_SCRUBBER_CYCLE_DISPLAY"),
                Pivot("MemoryBandwidthLoadMeter","ANIM_MEMORY_BANDWIDTH_METER"),
                Pivot("MemoryPageQuarantineModule","ANIM_PAGE_QUARANTINE_GATE"),
                Pivot("ReadOnlyGoldenImageVault","ANIM_GOLDEN_CHECKPOINT_RESTORE"),
                Pivot("RedundantComputeCoreA","ANIM_COMPUTE_LANE_A"),
                Pivot("RedundantComputeCoreB","ANIM_COMPUTE_LANE_B"),
                Pivot("RedundantComputeCoreC","ANIM_COMPUTE_LANE_C"),
                Pivot("TripleModularMajorityVoter","ANIM_TMR_VOTER_GATE"),
                Pivot("VoterDiscrepancyAnalyzer","ANIM_VOTER_DISAGREEMENT_DISPLAY"),
                Pivot("ComputeLaneHealthProbeArray","ANIM_COMPUTE_HEALTH_PROBE"),
                Pivot("IndependentReferenceComputeUnit","ANIM_REFERENCE_COMPUTE_SCAN"),
                Pivot("HardwareWatchdogTimer","ANIM_WATCHDOG_TIMER"),
                Pivot("WatchdogChallengeResponseModule","ANIM_WATCHDOG_CHALLENGE_RESPONSE"),
                Pivot("SafeMissionCommandGate","ANIM_SAFE_COMMAND_GATE"),
                Pivot("ResilientInterchipCommunicationFabric","ANIM_INTERCHIP_DATA_FABRIC"),
                Pivot("PacketCRCEncoder","ANIM_CRC_ENCODER"),
                Pivot("PacketCRCVerificationEngine","ANIM_CRC_VALIDATOR"),
                Pivot("PacketSequenceNumberMonitor","ANIM_PACKET_SEQUENCE_SCAN"),
                Pivot("OpticalTelemetryDock","ANIM_OPTICAL_TELEMETRY_DOCK"),
                Pivot("PacketRetransmissionBuffer","ANIM_PACKET_RETRANSMISSION"),
                Pivot("ProtectedTelemetryMemoryBuffer","ANIM_TELEMETRY_BUFFER"),
                Pivot("MissionSensorInputEmulator","ANIM_SENSOR_INPUT_STREAM"),
                Pivot("NavigationStateEstimator","ANIM_NAVIGATION_STATE_DISPLAY"),
                Pivot("SpacecraftAttitudeActuatorSimulator","ANIM_ATTITUDE_ACTUATOR_HOLOGRAM"),
                Pivot("CorrelatedFaultPatternAnalyzer","ANIM_CORRELATED_FAULT_ANALYZER"),
                Pivot("ErrorPropagationTraceAnalyzer","ANIM_ERROR_PROPAGATION_TRACE"),
                Pivot("CriticalECCFeedbackSupervisor","ANIM_CRITICAL_ECC_FEEDBACK"),
                Pivot("ProtectedPowerDistributionCabinet","ANIM_POWER_DISTRIBUTION_ISOLATOR"),
                Pivot("EmergencyMissionSafeHoldStation","ANIM_EMERGENCY_SAFE_HOLD"),
                Pivot("ECCCorrectionArcHologram","ANIM_MEMORY_REPAIR_ARC"),
                Pivot("TripleLaneVotingHologram","ANIM_TRIPLE_VOTING_HOLOGRAM"),
                Pivot("PacketIntegrityFlowDisplay","ANIM_PACKET_INTEGRITY_HOLOGRAM"),
                Pivot("HolographicDependabilitySCADA","ANIM_DEPENDABILITY_SCADA"),
                Pivot("MissionReliabilityCertificationBeacon","ANIM_MISSION_CERTIFICATION_BEACON"),
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
            Lamp("Mission work light",new Vector3(-.38f,floor+.58f,-.14f),new Color(.91f,.94f,1),1.35f,1.2f);
            Lamp("Metrology fill",new Vector3(.54f,floor+.34f,.16f),new Color(.49f,.80f,1),.64f,.76f);
            loop.rig.BindEffects(
                Lamp("ECC activity light",new Vector3(-.37f,floor+.20f,-.03f),new Color(.29f,.75f,1),1,.45f),
                Lamp("TMR activity light",new Vector3(0,floor+.20f,-.03f),new Color(.48f,.39f,1),1,.46f),
                Lamp("Telemetry activity light",new Vector3(.28f,floor+.2f,-.03f),new Color(.58f,.37f,1),1,.45f),
                Lamp("Isolation alarm",new Vector3(.56f,floor+.24f,.03f),new Color(1,.18f,.12f),1.25f,.55f));
            if(loop.plot.transform.Find("Monitor ECC") == null)
            {
                var oldPanel = root.transform.Find("Level 81 Controls");
                if (oldPanel != null) Object.DestroyImmediate(oldPanel.gameObject);
                SparkLevel81Builder.BuildControls(root.transform,loop,font);
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
            loop.controllerDisplay=Readout("ECC readout",new Vector3(-.815f,floor+.075f,.215f),new Color(.46f,.94f,1f));
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



















