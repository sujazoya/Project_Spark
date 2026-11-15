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
    public static class SparkLevel69ModelInstaller
    {
        public const string Folder = "Assets/My_Assets/_Project_Spark/Prefabs/Levels/Level_69_Layout/";
        public const string Prefab = Folder + "Level_69_Layout.prefab";
        public const string ScenePath = "Assets/My_Assets/_Project_Spark/Scenes/MainMenu/MainMenu.unity";
        private static readonly string[] Models = {
            "IndustrialColdChainBase","VariableSpeedRefrigerationCompressor","CompressorCutawayAssembly","CompressorMotorDrive","AirCooledCondenser","CondenserFanBank","HighPressureLiquidReceiver","LiquidLineFilterDrier","RefrigerantSightGlass","ElectronicExpansionValveA","ElectronicExpansionValveB","EvaporatorCoilA","EvaporatorCoilB","EvaporatorFanAssemblyA","EvaporatorFanAssemblyB","TransparentColdStorageChamber","TransparentRapidCoolingChamber","TemperatureSensitiveCargoRack","SuctionAccumulator","CompressorOilSeparator","CopperRefrigerantPipelineNetwork","SuctionPressureTransmitter","DischargePressureTransmitter","SuctionTemperatureSensor","DischargeTemperatureSensor","LiquidLineTemperatureSensor","EvaporatorOutletSensorA","EvaporatorOutletSensorB","ColdChamberTemperatureSensor","RapidCoolingTemperatureSensor","RefrigerantMassFlowMeter","RefrigerationEnergyAnalyzer","PressureEnthalpyCycleDisplay","DualZoneTemperaturePIDController","ElectronicSuperheatController","AdaptiveDefrostController","EvaporatorDefrostHeaterBank","CondensateDrainAssembly","CompressorProtectionRelay","ColdChainSafetyPLC","IndustrialRefrigerationPLCRack","RefrigerationDigitalIOModule","ThermalLoadDisturbanceModule","HolographicRefrigerationSCADA","TransparentRefrigerationSafetyEnclosure","Regulated5VControlSupply"
        };
        private const string Materials = Folder + "Models/Materials";
        private const float SocketHeight = .025f;
        private const float SocketDiameter = .026f;

        private static readonly (string id, string label, float x, float z, string color)[] Sockets =
        {
            ("SUPPLY_POS","SUP POS",-0.8700f,0.6800f,"red"),
            ("PLC_5V","PLC 5V",-0.7780f,0.6800f,"red"),
            ("SUPPLY_RET","SUP RET",-0.6860f,0.6800f,"blue"),
            ("PLC_GND","PLC GND",-0.5940f,0.6800f,"blue"),
            ("IO_5V","IO 5V",-0.5020f,0.6800f,"red"),
            ("IO_GND","IO GND",-0.4100f,0.6800f,"blue"),
            ("SAFE_5V","SAFE 5V",-0.3180f,0.6800f,"red"),
            ("SAFE_GND","SAFE GND",-0.2260f,0.6800f,"blue"),
            ("TEMP_5V","TEMP 5V",-0.1340f,0.6800f,"red"),
            ("TEMP_GND","TEMP GND",-0.0420f,0.6800f,"blue"),
            ("SH_5V","SH 5V",0.0500f,0.6800f,"red"),
            ("SH_GND","SH GND",0.1420f,0.6800f,"blue"),
            ("ZONE_5V","ZONE 5V",0.2340f,0.6800f,"red"),
            ("ZONE_GND","ZONE GND",0.3260f,0.6800f,"blue"),
            ("ENERGY_5V","ENG 5V",0.4180f,0.6800f,"red"),
            ("ENERGY_GND","ENG GND",0.5100f,0.6800f,"blue"),
            ("DRIVE_5V","DRIVE 5V",0.6020f,0.6800f,"red"),
            ("DRIVE_GND","DRIVE GND",0.6940f,0.6800f,"blue"),
            ("DEFROST_5V","DEF 5V",0.7860f,0.6800f,"red"),
            ("DEFROST_GND","DEF GND",0.8780f,0.6800f,"blue"),
            ("COMP_PERMIT","COMP PERMIT",-0.9800f,0.5200f,"mint"),
            ("COMP_EN","COMP EN",-0.8960f,0.5200f,"mint"),
            ("PLC_C_REF","PLC C REF",-0.8120f,0.5200f,"mint"),
            ("DRIVE_REF","DRIVE REF",-0.7280f,0.5200f,"mint"),
            ("COMP_TACH","COMP TACH",-0.6440f,0.5200f,"mint"),
            ("IO_RPM","IO RPM",-0.5600f,0.5200f,"mint"),
            ("DRIVE_READY","DRIVE READY",-0.4760f,0.5200f,"mint"),
            ("PLC_DRIVE_OK","PLC DRIVE OK",-0.3920f,0.5200f,"mint"),
            ("DRIVE_TRIP","DRIVE TRIP",-0.3080f,0.5200f,"amber"),
            ("SAFE_C_TRIP","SAFE C TRIP",-0.2240f,0.5200f,"amber"),
            ("COMP_POWER","COMP POWER",-0.1400f,0.5200f,"mint"),
            ("ENERGY_KW","ENG KW",-0.0560f,0.5200f,"mint"),
            ("PLC_COND","PLC COND",0.0280f,0.5200f,"mint"),
            ("FAN_C_CMD","FAN C CMD",0.1120f,0.5200f,"mint"),
            ("FAN_C_TACH","FAN C TACH",0.1960f,0.5200f,"mint"),
            ("IO_C_FAN","IO C FAN",0.2800f,0.5200f,"mint"),
            ("FAN_C_OK","FAN C OK",0.3640f,0.5200f,"mint"),
            ("SAFE_C_FAN","SAFE C FAN",0.4480f,0.5200f,"amber"),
            ("LIQ_LINE_OK","LIQ LINE OK",0.5320f,0.5200f,"mint"),
            ("PLC_SUBCOOL","PLC SUBCOOL",0.6160f,0.5200f,"mint"),
            ("SH_A_CMD","SH A CMD",0.7000f,0.5200f,"mint"),
            ("EEV_A_IN","EEV A IN",0.7840f,0.5200f,"mint"),
            ("SH_B_CMD","SH B CMD",0.8680f,0.5200f,"mint"),
            ("EEV_B_IN","EEV B IN",0.9520f,0.5200f,"mint"),
            ("EEV_A_POS","EEV A POS",-1.0000f,-0.3550f,"mint"),
            ("IO_EEV_A","IO EEV A",-0.9260f,-0.3550f,"mint"),
            ("EEV_B_POS","EEV B POS",-0.8520f,-0.3550f,"mint"),
            ("IO_EEV_B","IO EEV B",-0.7780f,-0.3550f,"mint"),
            ("EEV_A_OK","EEV A OK",-0.7040f,-0.3550f,"mint"),
            ("PLC_EEV_A","PLC EEV A",-0.6300f,-0.3550f,"mint"),
            ("EEV_B_OK","EEV B OK",-0.5560f,-0.3550f,"mint"),
            ("PLC_EEV_B","PLC EEV B",-0.4820f,-0.3550f,"mint"),
            ("ZONE_A_FAN","ZONE A FAN",-0.4080f,-0.3550f,"mint"),
            ("EVAP_FAN_A","EVAP FAN A",-0.3340f,-0.3550f,"mint"),
            ("ZONE_B_FAN","ZONE B FAN",-0.2600f,-0.3550f,"mint"),
            ("EVAP_FAN_B","EVAP FAN B",-0.1860f,-0.3550f,"mint"),
            ("EVAP_A_TACH","EVAP A TACH",-0.1120f,-0.3550f,"mint"),
            ("IO_EF_A","IO EF A",-0.0380f,-0.3550f,"mint"),
            ("EVAP_B_TACH","EVAP B TACH",0.0360f,-0.3550f,"mint"),
            ("IO_EF_B","IO EF B",0.1100f,-0.3550f,"mint"),
            ("SUCTION_P","LP P",0.1840f,-0.3550f,"mint"),
            ("SH_PRESS_IN","SH PRESS IN",0.2580f,-0.3550f,"mint"),
            ("DISCHARGE_P","HP P",0.3320f,-0.3550f,"mint"),
            ("SAFE_HIGH_P","SAFE HIGH P",0.4060f,-0.3550f,"amber"),
            ("LOW_P_ALARM","LOW P ALARM",0.4800f,-0.3550f,"mint"),
            ("SAFE_LOW_P","SAFE LOW P",0.5540f,-0.3550f,"amber"),
            ("TRANS_P_OK","TRANS P OK",0.6280f,-0.3550f,"mint"),
            ("PLC_PRESS_OK","PLC PRESS OK",0.7020f,-0.3550f,"mint"),
            ("SUCTION_T","LP T",0.7760f,-0.3550f,"mint"),
            ("PLC_SUCTION","PLC LP",0.8500f,-0.3550f,"mint"),
            ("DISCHARGE_T","HP T",0.9240f,-0.3550f,"mint"),
            ("SAFE_DISCH_T","SAFE DISCH T",0.9980f,-0.3550f,"amber"),
            ("LIQ_LINE_T","LIQ LINE T",-1.0000f,-0.5250f,"mint"),
            ("ENERGY_LIQ","ENG LIQ",-0.9260f,-0.5250f,"mint"),
            ("EVAP_B_TEMP_OUT","EVAP B T OUT",-0.8520f,-0.5250f,"mint"),
            ("SH_B_TEMP_IN","SH B T IN",-0.7780f,-0.5250f,"mint"),
            ("CHAMBER_A_T","ROOM A T",-0.7040f,-0.5250f,"mint"),
            ("ZONE_A_TEMP","ZONE A TEMP",-0.6300f,-0.5250f,"mint"),
            ("CHAMBER_B_T","ROOM B T",-0.5560f,-0.5250f,"mint"),
            ("ZONE_B_TEMP","ZONE B TEMP",-0.4820f,-0.5250f,"mint"),
            ("FROST_B","FROST B",-0.4080f,-0.5250f,"mint"),
            ("DEF_FROST_IN","DEF FROST IN",-0.3340f,-0.5250f,"mint"),
            ("DEF_ENABLE","DEF ENABLE",-0.2600f,-0.5250f,"mint"),
            ("HEATER_IN","HEATER IN",-0.1860f,-0.5250f,"mint"),
            ("HEATER_FB","HEATER FB",-0.1120f,-0.5250f,"mint"),
            ("DEF_HEAT_FB","DEF HEAT FB",-0.0380f,-0.5250f,"mint"),
            ("DRAIN_LEVEL","DRAIN LEVEL",0.0360f,-0.5250f,"mint"),
            ("DEF_DRAIN_IN","DEF DRAIN IN",0.1100f,-0.5250f,"mint"),
            ("DEF_FAN_LOCK","DEF FAN LOCK",0.1840f,-0.5250f,"mint"),
            ("IO_FAN_LOCK","IO FAN LOCK",0.2580f,-0.5250f,"mint"),
            ("ESTOP_NC","ESTOP NC",0.3320f,-0.5250f,"amber"),
            ("SAFE_STOP_IN","SAFE STOP IN",0.4060f,-0.5250f,"amber"),
            ("MASS_FLOW","MASS FLOW",0.4800f,-0.5250f,"mint"),
            ("ENERGY_FLOW","ENG FLOW",0.5540f,-0.5250f,"mint"),
            ("ACC_LEVEL","ACC LEVEL",0.6280f,-0.5250f,"mint"),
            ("SAFE_ACC","SAFE ACC",0.7020f,-0.5250f,"amber"),
            ("OIL_LEVEL","OIL LEVEL",0.7760f,-0.5250f,"mint"),
            ("SAFE_OIL","SAFE OIL",0.8500f,-0.5250f,"amber"),
            ("DOOR_TEST","DOOR TEST",0.9240f,-0.5250f,"mint"),
            ("IO_DOOR","IO DOOR",0.9980f,-0.5250f,"mint"),
            ("PLC_RESET","PLC RESET",-1.0000f,-0.7000f,"mint"),
            ("SAFE_RESET","SAFE RESET",-0.9260f,-0.7000f,"amber"),
            ("SAFE_STATE","SAFE STATE",-0.8520f,-0.7000f,"amber"),
            ("PLC_SAFE","PLC SAFE",-0.7780f,-0.7000f,"amber"),
            ("REF_CHARGE","CHARGE",-0.7040f,-0.7000f,"mint"),
            ("IO_CHARGE","IO CHARGE",-0.6300f,-0.7000f,"mint"),
            ("HMI_RPM","HMI RPM",-0.5560f,-0.7000f,"mint"),
            ("HMI_KW","HMI KW",-0.4820f,-0.7000f,"mint"),
            ("HMI_CFAN","HMI CFAN",-0.4080f,-0.7000f,"mint"),
            ("HMI_LOW_P","HMI LOW P",-0.3340f,-0.7000f,"mint"),
            ("HMI_HIGH_P","HMI HIGH P",-0.2600f,-0.7000f,"mint"),
            ("HMI_SUCTION","HMI LP",-0.1860f,-0.7000f,"mint"),
            ("HMI_DISCH_T","HMI DISCH T",-0.1120f,-0.7000f,"mint"),
            ("HMI_LIQ_T","HMI LIQ T",-0.0380f,-0.7000f,"mint"),
            ("HMI_SH_B","HMI SH B",0.0360f,-0.7000f,"mint"),
            ("HMI_A_TEMP","HMI A TEMP",0.1100f,-0.7000f,"mint"),
            ("HMI_B_TEMP","HMI B TEMP",0.1840f,-0.7000f,"mint"),
            ("HMI_FROST","HMI FROST",0.2580f,-0.7000f,"mint"),
            ("HMI_DRAIN","HMI DRAIN",0.3320f,-0.7000f,"mint"),
            ("HMI_FLOW","HMI FLOW",0.4060f,-0.7000f,"mint"),
            ("HMI_ACC","HMI ACC",0.4800f,-0.7000f,"mint"),
            ("HMI_OIL","HMI OIL",0.5540f,-0.7000f,"mint"),
            ("HMI_SAFE","HMI SAFE",0.6280f,-0.7000f,"amber"),
            ("HMI_EEV_A","HMI EEV A",0.7020f,-0.7000f,"mint"),
            ("HMI_EEV_B","HMI EEV B",0.7760f,-0.7000f,"mint"),
            ("FAN_TEST","FAN TEST",0.8500f,-0.7000f,"mint"),
            ("EVAP_A_TEMP_OUT","EVAP A T OUT",0.9240f,-0.7000f,"mint"),
            ("SH_A_TEMP_IN","SH A T IN",0.9980f,-0.7000f,"mint"),
        };

        public static IEnumerable<string> TerminalIds => Sockets.Select(socket => socket.id);

        private static readonly Dictionary<string, string> Palette = new()
        {
            {"red","EE485C"},{"blue","1691E2"},{"amber","FFA82B"},{"violet","A282FF"},
            {"mint","34F6C5"},{"cyan","23DAFD"},{"void","07111C"},{"white","D4F5FF"},{"glass","498BA8"}
        };

        [MenuItem("Tools/Project Spark/Levels/Create Level 69")]
        public static void Install()
        {
            SparkLevel69Builder.Create();
            var root = AssetDatabase.LoadAssetAtPath<GameObject>(Prefab);
            if (root == null || root.GetComponent<SparkLevel69Process>().controllerDisplay == null) Fit();
            Register();
            ValidateAssets();
        }

        [MenuItem("Tools/Project Spark/Level 69/Validate Level Assets")]
        public static void ValidateAssets()
        {
            var root=AssetDatabase.LoadAssetAtPath<GameObject>(Prefab);
            var control=root != null ? root.GetComponent<SparkLevel69Process>() : null;
            var bindings=root != null ? root.GetComponent<SparkLevelSceneBindings>() : null;
            if (control==null || bindings==null || control.controllerDisplay==null || control.qualityDisplay==null || control.rig==null || control.rig.pivots.Length!=14 ||
                control.pins.Length!=128 || control.pins.Distinct().Count()!=128 || 
                !TerminalIds.SequenceEqual(control.pins.Select(pin=>pin.name)) ||
                control.pins.Any(pin=>!bindings.TryGetTerminal(pin.name,out var bound) || bound!=pin) ||
                SparkLevel69Process.Gates.Any(id=>!bindings.IsComponentIdDeclared(id)))
                throw new InvalidOperationException("Level 69 wiring, motion or binding references are incomplete.");
            Debug.Log("[LEVEL 69] Validated 128 distinct terminals, 82 player wires and 6 evaluator gates.",root);
        }

        private static void Register()
        {
            var scene = SceneManager.GetSceneByPath(ScenePath);
            bool opened = !scene.isLoaded;
            if (opened) scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
            bool dirty = scene.isDirty;
            var objects = scene.GetRootGameObjects();
            T Find<T>() where T : Component => objects.SelectMany(item => item.GetComponentsInChildren<T>(true)).Single();
            var level = AssetDatabase.LoadAssetAtPath<SparkLevelDefinition>(Folder + "Level_69.asset");
            var drawing = AssetDatabase.LoadAssetAtPath<SparkLevelDrawingAsset>(Folder + "Level_69_Drawing.asset");
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
            else if (dirty) Debug.Log("MainMenu had unsaved edits. Save the scene to persist Level 69's campaign and teaching entries.");
        }

        private static void Append(Object target, string field, Object value)
        {
            if (value == null) throw new InvalidOperationException("Missing Level 69 asset for " + field);
            var data = new SerializedObject(target);
            var list = data.FindProperty(field);
            for (int i = 0; i < list.arraySize; i++)
                if (list.GetArrayElementAtIndex(i).objectReferenceValue == value) return;
            list.arraySize++;
            list.GetArrayElementAtIndex(list.arraySize - 1).objectReferenceValue = value;
            data.ApplyModifiedProperties();
        }

        [MenuItem("Tools/Project Spark/Level 69/Fit Breadboard Assembly")]
        public static void Fit()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Exit Play Mode before fitting Level 69.");
            var models = Models.Select((name, index) =>
            {
                string path = Folder + $"Models/L69_{index:00}_{name}_Fitted.fbx";
                return AssetDatabase.LoadAssetAtPath<GameObject>(path) ??
                    throw new InvalidOperationException("Missing fitted kit module " + path);
            }).ToArray();
            if (AssetDatabase.LoadAssetAtPath<GameObject>(Prefab) == null)
                throw new InvalidOperationException("The Level 69 prefab is missing.");

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
                    throw new InvalidOperationException("Unity could not save the Level 69 layout.");
                AssetDatabase.SaveAssets();
                Debug.Log("Level 69 fitted to the MainMenu breadboard with 128 seated terminals.");
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
            Vector3 right = Vector3.ProjectOnPlane(grid.GridOrigin.right, up).normalized * .069f;
            Vector3 forward = Vector3.ProjectOnPlane(grid.GridOrigin.forward, up).normalized * .069f;
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
            var bench = root.transform.Find("Refrigeration Workbench");
            var bindings = root.GetComponent<SparkLevelSceneBindings>();
            var loop = root.GetComponent<SparkLevel69Process>();
            if (bench == null || bindings == null || loop == null)
                throw new InvalidOperationException("Level 69 is missing its workbench, controls, or scene bindings.");

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

            foreach (string name in new[] {"Fitted Level 69 Models", "Breadboard Labels", "Motion pivots"})
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
            var assembly = Child(bench, "Fitted Level 69 Models", new Vector3(0,floor,.040f));
            foreach (var model in models)
            {
                var instance = Object.Instantiate(model, assembly, false).transform;
                var markers = instance.GetComponentsInChildren<Transform>(true)
                    .Where(item => item.name.StartsWith("L69_Board",StringComparison.Ordinal))
                    .ToDictionary(item => item.name);
                Transform Marker(string name) => markers["L69_Board" + name];
                Vector3 Axis(string name) => assembly.InverseTransformVector(Marker(name).position - Marker("Center").position);
                instance.localRotation = Quaternion.Inverse(Quaternion.LookRotation(-Axis("Front"), Axis("Up"))) * instance.localRotation;
                instance.localScale /= Axis("Right").magnitude;
                instance.localPosition -= assembly.InverseTransformPoint(Marker("Center").position);
                if (Axis("Right").x < .99f)
                    throw new InvalidOperationException("Unexpected axes on fitted model " + model.name);
            }

            assembly.localScale = Vector3.one * .80f;
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
                var oldControls = root.transform.Find("Level 69 Controls");
                if (oldControls != null) Object.DestroyImmediate(oldControls.gameObject);
                SparkLevel69Builder.BuildControls(root.transform,loop,font);
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
                terminalData.FindProperty("maxConnections").intValue = SparkLevel69Process.Wires.Count(wire => wire.a == index || wire.b == index);
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
                float offset=terminalBelow?-.032f:label.column%2==0?-.041f:-.069f;
                Label(labels,font,label.id,label.label,label.seat+new Vector3(0,.019f,offset),.0105f);
            }
            foreach (var owner in owners)
                foreach (Transform child in owner)
                    if (child.GetComponent<SparkTerminal>() == null && child.GetComponent<Renderer>() != null)
                        child.gameObject.SetActive(false);

            var titles = new (string title, float x, float z)[]
            {






                ("DUAL COLD CHAMBERS",-.62f,.365f),("COMPRESSOR / CONDENSER",.42f,.365f),
                ("INSTRUMENTS",-.58f,-.25f),("EXPANSION / SAFETY",.61f,-.25f)
            };
            foreach(var title in titles)
                Label(labels,font,title.title,title.title,new Vector3(title.x,floor+.058f,title.z),.0165f);
            Label(labels,font,"Chamber A label","CHAMBER A",new Vector3(-.39f,floor+.372f,.196f),.012f);
            Label(labels,font,"Chamber B label","CHAMBER B",new Vector3(-.39f,floor+.198f,.009f),.012f);
            Transform Pivot(string module,string key)
            {
                var name="fitted_"+module+"_"+key;
                return assembly.GetComponentsInChildren<Transform>(true).Single(item=>item.name==name);
            }
            loop.rig.pivots=new[]{
                Pivot("VariableSpeedRefrigerationCompressor","ANIM_COMPRESSOR_ROTOR"),
                Pivot("CompressorCutawayAssembly","ANIM_COMPRESSOR_MECHANISM"),
                Pivot("CondenserFanBank","ANIM_CONDENSER_FAN_A"),
                Pivot("CondenserFanBank","ANIM_CONDENSER_FAN_B"),
                Pivot("EvaporatorFanAssemblyA","ANIM_EVAP_FAN_A"),
                Pivot("EvaporatorFanAssemblyB","ANIM_EVAP_FAN_B"),
                Pivot("ElectronicExpansionValveA","ANIM_EEV_A_ACTUATOR"),
                Pivot("ElectronicExpansionValveB","ANIM_EEV_B_ACTUATOR"),
                Pivot("TransparentColdStorageChamber","ANIM_CHAMBER_A_DOOR"),
                Pivot("TransparentRapidCoolingChamber","ANIM_CHAMBER_B_DOOR"),
                Pivot("EvaporatorCoilA","ANIM_EVAPORATOR_FROST"),
                Pivot("CondensateDrainAssembly","ANIM_CONDENSATE_DRAIN"),
                Pivot("HolographicRefrigerationSCADA","ANIM_REFRIGERATION_SCADA"),
                Pivot("TransparentRefrigerationSafetyEnclosure","ANIM_SAFETY_DOOR"),
            };
            loop.rig.fanA2=Pivot("EvaporatorFanAssemblyA","ANIM_EVAP_FAN_A_SECOND");
            loop.rig.fanB2=Pivot("EvaporatorFanAssemblyB","ANIM_EVAP_FAN_B_SECOND");
            loop.rig.frostB=Pivot("EvaporatorCoilB","ANIM_EVAPORATOR_FROST_B");
            loop.rig.droplets=new Transform[3];
            for(int i=0;i<loop.rig.droplets.Length;i++)
            {
                var drop=GameObject.CreatePrimitive(PrimitiveType.Sphere);
                drop.name="Drain drop "+(i+1); drop.transform.SetParent(loop.rig.pivots[11],false);
                Object.DestroyImmediate(drop.GetComponent<Collider>());
                drop.transform.localScale=Vector3.one*.026f;
                drop.transform.localPosition=new Vector3(i*.025f,0,0);
                drop.GetComponent<Renderer>().sharedMaterial=MaterialFor(shader,"cyan");
                drop.SetActive(false);
                loop.rig.droplets[i]=drop.transform;
            }
            if(loop.plot.transform.Find("Trend legend") == null)
            {
                var oldPanel = root.transform.Find("Level 69 Controls");
                if (oldPanel != null) Object.DestroyImmediate(oldPanel.gameObject);
                SparkLevel69Builder.BuildControls(root.transform,loop,font);
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
            loop.controllerDisplay=Readout("Compressor status",new Vector3(-.53f,floor+.15f,.386f),new Color(.46f,.94f,1f));
            loop.qualityDisplay=Readout("Chamber status",new Vector3(.53f,floor+.15f,.386f),new Color(.69f,1f,.86f));
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
            label.fontSize = 69;
            label.characterSize = size;
            label.anchor = TextAnchor.MiddleCenter;
            label.color = new Color(.89f, .96f, 1f);
            label.text = text;
            child.GetComponent<MeshRenderer>().sharedMaterial = font.material;
            return label;
        }

        private static Material MaterialFor(Shader shader, string key)
        {
            string path = $"{Materials}/L69 {key}.mat";
            if (key.StartsWith("tint",StringComparison.Ordinal) && key.Length==10) Palette[key]=key.Substring(4);
            var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing != null && key is not ("glass" or "acrylic") &&
                ColorUtility.TryParseHtmlString("#" + (Palette.TryGetValue(key, out var savedTint) ? savedTint : "9FB3C8"), out var savedColor) &&
                existing.GetColor("_BaseColor") == savedColor) return existing;
            ColorUtility.TryParseHtmlString("#" + (Palette.TryGetValue(key, out var tint) ? tint : "9FB3C8"), out var color);
            var material = existing != null ? existing : new Material(shader) { name = "L69 " + key };
            bool metal = key is "chrome" or "titanium" or "steel" or "gold" or "copper" or "aluminum" or "tintCFDCE2" or "tintA4B8C4" or "tintE36C27" or "tintDC9C2F";
            material.SetFloat("_Metallic", metal ? .69f : .02f);
            material.SetFloat("_Smoothness", metal ? .69f : key == "glass" ? .12f : .35f);
            if (key is "green" or "cyan" or "red" or "blue" or "orange" or "amber" or "mint" or "warm" or "ice")
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", color * .69f);
            }
            else if (key.StartsWith("tint", StringComparison.Ordinal))
            {
                material.EnableKeyword("_EMISSION");
                float strength = key is "tint19CCEF" or "tint27D690" or "tintFFCE34" ? .30f : .12f;
                material.SetColor("_EmissionColor", color * strength);
            }
            if (key is "glass" or "glass_body" or "glass_soft" or "acrylic" or "glassblue" or "film" or "Glass" or "guardglass")
            {
                color.a = key=="film" ? .69f : .10f;
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


