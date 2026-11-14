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
    public static class SparkLevel63ModelInstaller
    {
        public const string Folder = "Assets/My_Assets/_Project_Spark/Prefabs/Levels/Level_63_Layout/";
        public const string Prefab = Folder + "Level_63_Layout.prefab";
        public const string ScenePath = "Assets/My_Assets/_Project_Spark/Scenes/MainMenu/MainMenu.unity";
        private static readonly string[] Models = {
            "IndustrialFrequencyLabBase","ThreePhaseMicrogridSource","ThreePhaseDistributionBus","MainGridBreaker","SynchronousGenerator","GeneratorPrimeMover","GeneratorBreaker","HighInertiaFlywheelRotor","FlywheelMotorGenerator","FlywheelMagneticBearing","FlywheelShaftCoupling","FlywheelSpeedEncoder","FlywheelVibrationSensor","FlywheelPowerConverter","FlywheelDCEnergyLink","BatteryEnergyBuffer","BatteryManagementModule","GridFormingInverter","VirtualSynchronousMachineController","FrequencyDroopController","PowerOscillationDamper","FrequencyROCOFSensor","ThreePhaseVoltageSensor","CurrentTransformerBank","ProgrammableLoadBank","IndustrialMotorLoad","RenewableSourceSimulator","GeneratorTripSimulator","FastFrequencyResponseRelay","EnergyManagementController","PowerQualityEventRecorder","HolographicFrequencyMonitor","TransparentFlywheelSafetyEnclosure","Regulated5VControlSupply"
        };
        private const string Materials = Folder + "Models/Materials";
        private const float SocketHeight = .025f;
        private const float SocketDiameter = .029f;

        private static readonly (string id, string label, float x, float z, string color)[] Sockets =
        {
            ("SUPPLY_POS","SUPPLY POS",-0.8300f,0.5900f,"red"),
            ("CTRL_POS","CTRL POS",-0.7505f,0.5900f,"red"),
            ("SUPPLY_RET","SUPPLY RET",-0.6710f,0.5900f,"blue"),
            ("CTRL_GND","CTRL GND",-0.5915f,0.5900f,"blue"),
            ("GEN_5V","GEN 5V",-0.5120f,0.5900f,"red"),
            ("GEN_GND","GEN GND",-0.4325f,0.5900f,"blue"),
            ("VSM_5V","VSM 5V",-0.3530f,0.5900f,"red"),
            ("VSM_GND","VSM GND",-0.2735f,0.5900f,"blue"),
            ("SENSE_5V","SENSE 5V",-0.1940f,0.5900f,"red"),
            ("SENSE_GND","SENSE GND",-0.1145f,0.5900f,"blue"),
            ("GRID_A","GRID A",-0.0350f,0.5900f,"cyan"),
            ("MAIN_A","MAIN A",0.0445f,0.5900f,"cyan"),
            ("GRID_B","GRID B",0.1240f,0.5900f,"amber"),
            ("MAIN_B","MAIN B",0.2035f,0.5900f,"amber"),
            ("GRID_C","GRID C",0.2830f,0.5900f,"violet"),
            ("MAIN_C","MAIN C",0.3625f,0.5900f,"violet"),
            ("MAIN_OUT_A","MAIN OUT A",0.4420f,0.5900f,"cyan"),
            ("BUS_A","BUS A",0.5215f,0.5900f,"cyan"),
            ("MAIN_OUT_B","MAIN OUT B",0.6010f,0.5900f,"amber"),
            ("BUS_B","BUS B",0.6805f,0.5900f,"amber"),
            ("MAIN_OUT_C","MAIN OUT C",0.7600f,0.5900f,"violet"),
            ("BUS_C","BUS C",0.8395f,0.5900f,"violet"),
            ("GEN_A","GEN A",0.9190f,0.5900f,"cyan"),
            ("GEN_IN_A","GEN IN A",0.9985f,0.5900f,"cyan"),
            ("GEN_B","GEN B",-0.9940f,-0.4000f,"amber"),
            ("GEN_IN_B","GEN IN B",-0.9076f,-0.4000f,"amber"),
            ("GEN_C","GEN C",-0.8212f,-0.4000f,"violet"),
            ("GEN_IN_C","GEN IN C",-0.7348f,-0.4000f,"violet"),
            ("GEN_OUT_A","GEN OUT A",-0.6484f,-0.4000f,"cyan"),
            ("GEN_OUT_B","GEN OUT B",-0.5620f,-0.4000f,"amber"),
            ("GEN_OUT_C","GEN OUT C",-0.4756f,-0.4000f,"violet"),
            ("CTRL_PRIME","CTRL PRIME",-0.3892f,-0.4000f,"mint"),
            ("PRIME_CMD","PRIME CMD",-0.3028f,-0.4000f,"mint"),
            ("GEN_SPEED","GEN SPEED",-0.2164f,-0.4000f,"mint"),
            ("CTRL_GEN_SPEED","CTRL GEN SPEED",-0.1300f,-0.4000f,"mint"),
            ("GEN_POWER","GEN POWER",-0.0436f,-0.4000f,"mint"),
            ("CTRL_GEN_POWER","CTRL GEN POWER",0.0428f,-0.4000f,"mint"),
            ("TRIP_OUT","TRIP OUT",0.1292f,-0.4000f,"mint"),
            ("CTRL_TRIP","CTRL TRIP",0.2156f,-0.4000f,"mint"),
            ("PROTECT_OUT","PROTECT OUT",0.3020f,-0.4000f,"mint"),
            ("CTRL_GEN_AUX","CTRL GEN AUX",0.3884f,-0.4000f,"mint"),
            ("CTRL_TORQUE","CTRL TORQUE",0.4748f,-0.4000f,"mint"),
            ("MG_CMD","MG CMD",0.5612f,-0.4000f,"mint"),
            ("MG_POS","MG POS",0.6476f,-0.4000f,"red"),
            ("DRIVE_POS","DRIVE POS",0.7340f,-0.4000f,"red"),
            ("MG_NEG","MG NEG",0.8204f,-0.4000f,"blue"),
            ("DRIVE_NEG","DRIVE NEG",0.9068f,-0.4000f,"blue"),
            ("DRIVE_DC","DRIVE DC",0.9932f,-0.4000f,"mint"),
            ("DC_POS","DC POS",-0.9940f,-0.5600f,"red"),
            ("DRIVE_RET","DRIVE RET",-0.9076f,-0.5600f,"blue"),
            ("DC_NEG","DC NEG",-0.8212f,-0.5600f,"blue"),
            ("CTRL_DRIVE","CTRL DRIVE",-0.7348f,-0.5600f,"mint"),
            ("DRIVE_ENABLE","DRIVE ENABLE",-0.6484f,-0.5600f,"mint"),
            ("VIB_OUT","VIB OUT",-0.5620f,-0.5600f,"mint"),
            ("CTRL_VIB","CTRL VIB",-0.4756f,-0.5600f,"mint"),
            ("CTRL_BEARING","CTRL BEARING",-0.3892f,-0.5600f,"mint"),
            ("BAT_POS","BAT POS",-0.3028f,-0.5600f,"red"),
            ("BMS_POS","BMS POS",-0.2164f,-0.5600f,"red"),
            ("BAT_NEG","BAT NEG",-0.1300f,-0.5600f,"blue"),
            ("BMS_OUT","BMS OUT",-0.0436f,-0.5600f,"mint"),
            ("INV_POS","INV POS",0.0428f,-0.5600f,"red"),
            ("INV_NEG","INV NEG",0.1292f,-0.5600f,"blue"),
            ("CTRL_INV","CTRL INV",0.2156f,-0.5600f,"mint"),
            ("INV_ENABLE","INV ENABLE",0.3020f,-0.5600f,"mint"),
            ("INV_A","INV A",0.3884f,-0.5600f,"cyan"),
            ("INV_B","INV B",0.4748f,-0.5600f,"amber"),
            ("INV_C","INV C",0.5612f,-0.5600f,"violet"),
            ("FREQ_OUT","FREQ OUT",0.6476f,-0.5600f,"mint"),
            ("VSM_FREQ","VSM FREQ",0.7340f,-0.5600f,"mint"),
            ("DROOP_FREQ","DROOP FREQ",0.8204f,-0.5600f,"mint"),
            ("DAMP_FREQ","DAMP FREQ",0.9068f,-0.5600f,"mint"),
            ("DROOP_OUT","DROOP OUT",0.9932f,-0.5600f,"mint"),
            ("CTRL_DROOP","CTRL DROOP",-0.9000f,-0.7000f,"mint"),
            ("DAMP_OUT","DAMP OUT",-0.7970f,-0.7000f,"mint"),
            ("CTRL_DAMP","CTRL DAMP",-0.6940f,-0.7000f,"mint"),
            ("VSM_OUT","VSM OUT",-0.5910f,-0.7000f,"mint"),
            ("CTRL_VSM","CTRL VSM",-0.4880f,-0.7000f,"mint"),
            ("CTRL_CURRENT","CTRL I",-0.3850f,-0.7000f,"mint"),
            ("LOAD_A","LOAD A",-0.2820f,-0.7000f,"cyan"),
            ("MOTOR_B","MOTOR B",-0.1790f,-0.7000f,"amber"),
            ("RENEW_C","RENEW C",-0.0760f,-0.7000f,"violet"),
            ("CTRL_RENEW","CTRL RENEW",0.0270f,-0.7000f,"mint"),
            ("CTRL_EVENT","CTRL EVENT",0.1300f,-0.7000f,"mint"),
            ("EVENT_CMD","EVENT CMD",0.2330f,-0.7000f,"mint"),
            ("CTRL_REC","CTRL REC",0.3360f,-0.7000f,"mint"),
            ("CTRL_PROTECT","CTRL PROTECT",0.4390f,-0.7000f,"mint"),
            ("GUARD_NC","GUARD NC",0.5420f,-0.7000f,"mint"),
            ("CTRL_SAFE","CTRL SAFE",0.6450f,-0.7000f,"mint"),
            ("ENCODER_OUT","ENC OUT",0.7480f,-0.7000f,"mint"),
            ("CTRL_ENCODER","ENC IN",0.8510f,-0.7000f,"mint"),
        };

        public static IEnumerable<string> TerminalIds => Sockets.Select(socket => socket.id);

        private static readonly Dictionary<string, string> Palette = new Dictionary<string, string>
        {
            {"acrylic","57BBE2"},
            {"acrylic_rim","71B5DA"},
            {"amber","FFA82B"},
            {"blue","1671E2"},
            {"blue_steel","2D4C64"},
            {"carbon","091119"},
            {"chrome","C3E0ED"},
            {"copper","CB5920"},
            {"cyan","23DAFD"},
            {"dark","111A26"},
            {"display","04111C"},
            {"gold","FCB342"},
            {"green","1AF571"},
            {"grid","153A4C"},
            {"mint","34F6C5"},
            {"navy","0C1829"},
            {"red","EE485C"},
            {"rubber","090D14"},
            {"silver","81ABC5"},
            {"silver_dark","54748E"},
            {"titanium","476479"},
            {"violet","A282FF"},
            {"void","07111C"},
            {"white","D4F5FF"},
        };

        [MenuItem("Tools/Project Spark/Levels/Create Level 63")]
        public static void Install()
        {
            SparkLevel63Builder.Create();
            var root = AssetDatabase.LoadAssetAtPath<GameObject>(Prefab);
            if (root == null || root.GetComponent<SparkLevel63Frequency>().controllerDisplay == null) Fit();
            Register();
            ValidateAssets();
        }

        [MenuItem("Tools/Project Spark/Level 63/Validate Level Assets")]
        public static void ValidateAssets()
        {
            var root=AssetDatabase.LoadAssetAtPath<GameObject>(Prefab);
            var control=root != null ? root.GetComponent<SparkLevel63Frequency>() : null;
            var bindings=root != null ? root.GetComponent<SparkLevelSceneBindings>() : null;
            if (control==null || bindings==null || control.controllerDisplay==null || control.qualityDisplay==null || control.rig==null ||
                control.pins.Length!=90 || control.pins.Distinct().Count()!=90 || 
                !TerminalIds.SequenceEqual(control.pins.Select(pin=>pin.name)) ||
                control.pins.Any(pin=>!bindings.TryGetTerminal(pin.name,out var bound) || bound!=pin) ||
                SparkLevel63Frequency.Gates.Any(id=>!bindings.IsComponentIdDeclared(id)))
                throw new InvalidOperationException("Level 63 wiring, motion or binding references are incomplete.");
            Debug.Log("[LEVEL 63] Validated 90 distinct terminals, 58 player wires and 6 evaluator gates.",root);
        }

        private static void Register()
        {
            var scene = SceneManager.GetSceneByPath(ScenePath);
            bool opened = !scene.isLoaded;
            if (opened) scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
            bool dirty = scene.isDirty;
            var objects = scene.GetRootGameObjects();
            T Find<T>() where T : Component => objects.SelectMany(item => item.GetComponentsInChildren<T>(true)).Single();
            var level = AssetDatabase.LoadAssetAtPath<SparkLevelDefinition>(Folder + "Level_63.asset");
            var drawing = AssetDatabase.LoadAssetAtPath<SparkLevelDrawingAsset>(Folder + "Level_63_Drawing.asset");
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
            else if (dirty) Debug.Log("MainMenu had unsaved edits. Save the scene to persist Level 63's campaign and teaching entries.");
        }

        private static void Append(Object target, string field, Object value)
        {
            if (value == null) throw new InvalidOperationException("Missing Level 63 asset for " + field);
            var data = new SerializedObject(target);
            var list = data.FindProperty(field);
            for (int i = 0; i < list.arraySize; i++)
                if (list.GetArrayElementAtIndex(i).objectReferenceValue == value) return;
            list.arraySize++;
            list.GetArrayElementAtIndex(list.arraySize - 1).objectReferenceValue = value;
            data.ApplyModifiedProperties();
        }

        [MenuItem("Tools/Project Spark/Level 63/Fit Breadboard Assembly")]
        public static void Fit()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Exit Play Mode before fitting Level 63.");
            var models = Models.Select((name, index) =>
            {
                string path = Folder + $"Models/L63_{index:00}_{name}_Fitted.fbx";
                return AssetDatabase.LoadAssetAtPath<GameObject>(path) ??
                    throw new InvalidOperationException("Missing fitted kit module " + path);
            }).ToArray();
            if (AssetDatabase.LoadAssetAtPath<GameObject>(Prefab) == null)
                throw new InvalidOperationException("The Level 63 prefab is missing.");

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
                    throw new InvalidOperationException("Unity could not save the Level 63 layout.");
                AssetDatabase.SaveAssets();
                Debug.Log("Level 63 fitted to the MainMenu breadboard with 90 seated terminals.");
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
            Vector3 right = Vector3.ProjectOnPlane(grid.GridOrigin.right, up).normalized * .063f;
            Vector3 forward = Vector3.ProjectOnPlane(grid.GridOrigin.forward, up).normalized * .063f;
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
            var bench = root.transform.Find("Frequency Workbench");
            var bindings = root.GetComponent<SparkLevelSceneBindings>();
            var loop = root.GetComponent<SparkLevel63Frequency>();
            if (bench == null || bindings == null || loop == null)
                throw new InvalidOperationException("Level 63 is missing its workbench, controls, or scene bindings.");

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

            foreach (string name in new[] {"Fitted Level 63 Models", "Breadboard Labels", "Motion pivots"})
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
            var assembly = Child(bench, "Fitted Level 63 Models", Vector3.up * floor);
            foreach (var model in models)
            {
                var instance = Object.Instantiate(model, assembly, false).transform;
                var markers = instance.GetComponentsInChildren<Transform>(true)
                    .Where(item => item.name.StartsWith("L63_Board",StringComparison.Ordinal))
                    .ToDictionary(item => item.name);
                Transform Marker(string name) => markers["L63_Board" + name];
                Vector3 Axis(string name) => assembly.InverseTransformVector(Marker(name).position - Marker("Center").position);
                instance.localRotation = Quaternion.Inverse(Quaternion.LookRotation(-Axis("Front"), Axis("Up"))) * instance.localRotation;
                instance.localScale /= Axis("Right").magnitude;
                instance.localPosition -= assembly.InverseTransformPoint(Marker("Center").position);
                if (Axis("Right").x < .99f)
                    throw new InvalidOperationException("Unexpected axes on fitted model " + model.name);
            }

            assembly.localScale = Vector3.one * .88f;
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
                var oldControls = root.transform.Find("Level 63 Controls");
                if (oldControls != null) Object.DestroyImmediate(oldControls.gameObject);
                SparkLevel63Builder.BuildControls(root.transform,loop,font);
            }
            var labels = Child(bench, "Breadboard Labels", Vector3.zero);
            var holes = new HashSet<Vector2Int>();
            var owners = new HashSet<Transform>();
            int pinLayer = LayerMask.NameToLayer("Pin");
            if (pinLayer < 0) throw new InvalidOperationException("The Pin layer is missing.");
            foreach (var spec in Sockets)
            {
                if (!bindings.TryGetTerminal(spec.id, out var terminal))
                    throw new InvalidOperationException("Missing circuit terminal: " + spec.id);
                int index = Array.FindIndex(loop.pins, pin => pin == terminal);
                var terminalData = new SerializedObject(terminal);
                terminalData.FindProperty("maxConnections").intValue = SparkLevel63Frequency.Wires.Count(wire => wire.a == index || wire.b == index);
                terminalData.ApplyModifiedPropertiesWithoutUndo();
                owners.Add(terminal.transform.parent);
                terminal.GetComponent<Renderer>().enabled = true;
                var projected = ToWorld(new Vector3(spec.x, floor, spec.z));
                projected -= up * Vector3.Dot(projected - grid.GridOrigin.position, up);
                if (!grid.TryGetNearestHole(projected, .32f, out var hole, out int column, out int row) ||
                    !holes.Add(new Vector2Int(column, row)))
                    throw new InvalidOperationException("No separate breadboard hole for " + spec.id);
                Vector3 seat = ToLocal(SurfacePoint(grid, hole, up));
                Seat(terminal, bench, seat, pinLayer, MaterialFor(shader, spec.color), MaterialFor(shader, "void"));

                float offset = Array.IndexOf(Sockets,spec)%2==0 ? -.041f : -.067f;
                Label(labels,font,spec.id,spec.label,seat+new Vector3(0,.014f,offset),.0135f);
            }
            foreach (var owner in owners)
                foreach (Transform child in owner)
                    if (child.GetComponent<SparkTerminal>() == null && child.GetComponent<Renderer>() != null)
                        child.gameObject.SetActive(false);

            var titles = new (string title, float x, float z)[]
            {






                ("GRID / GENERATOR",-.56f,.32f),("INERTIA RESERVE",.50f,.32f),
                ("CONVERTER / BUFFER",-.53f,-.28f),("FREQUENCY RESPONSE",.54f,-.28f)
            };
            foreach(var title in titles)
                Label(labels,font,title.title,title.title,new Vector3(title.x,floor+.028f,title.z),.018f);
            Transform Pivot(string module,string key)
            {
                var name="fitted_"+module+"_"+key;
                return assembly.GetComponentsInChildren<Transform>(true).Single(item=>item.name==name);
            }
            loop.rig.grid=Pivot("MainGridBreaker","ANIM_BREAKER_HANDLE");
            loop.rig.generator=Pivot("GeneratorBreaker","ANIM_BREAKER_HANDLE");
            loop.rig.generatorRotor=Pivot("SynchronousGenerator","ANIM_GENERATOR_ROTOR");
            loop.rig.prime=Pivot("GeneratorPrimeMover","ANIM_PRIME_MOVER");
            loop.rig.rotor=Pivot("HighInertiaFlywheelRotor","ANIM_FLYWHEEL_ROTOR");
            loop.rig.drive=Pivot("FlywheelMotorGenerator","ANIM_FLYWHEEL_MG");
            loop.rig.coupling=Pivot("FlywheelShaftCoupling","ANIM_FLYWHEEL_COUPLING");
            loop.rig.encoder=Pivot("FlywheelSpeedEncoder","ANIM_ENCODER_DISC");
            loop.rig.motor=Pivot("IndustrialMotorLoad","ANIM_INDUSTRIAL_MOTOR");
            loop.rig.solar=Pivot("RenewableSourceSimulator","ANIM_SOLAR_TILT");
            loop.rig.monitor=Pivot("HolographicFrequencyMonitor","ANIM_MONITOR_TILT");
            loop.rig.guard=Pivot("TransparentFlywheelSafetyEnclosure","ANIM_SAFETY_DOOR");

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
            loop.controllerDisplay=Readout("Bus readout",new Vector3(-.53f,floor+.18f,.42f),new Color(.46f,.94f,1f));
            loop.qualityDisplay=Readout("Power readout",new Vector3(.53f,floor+.325f,.44f),new Color(.68f,1f,.86f));
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
            label.fontSize = 64;
            label.characterSize = size;
            label.anchor = TextAnchor.MiddleCenter;
            label.color = new Color(.89f, .96f, 1f);
            label.text = text;
            child.GetComponent<MeshRenderer>().sharedMaterial = font.material;
            return label;
        }

        private static Material MaterialFor(Shader shader, string key)
        {
            string path = $"{Materials}/L63 {key}.mat";
            var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing != null && key is not ("glass" or "acrylic") &&
                ColorUtility.TryParseHtmlString("#" + (Palette.TryGetValue(key, out var savedTint) ? savedTint : "9FB3C8"), out var savedColor) &&
                existing.GetColor("_BaseColor") == savedColor) return existing;
            ColorUtility.TryParseHtmlString("#" + (Palette.TryGetValue(key, out var tint) ? tint : "9FB3C8"), out var color);
            var material = existing != null ? existing : new Material(shader) { name = "L63 " + key };
            bool metal = key is "chrome" or "titanium" or "steel" or "gold" or "copper" or "aluminum";
            material.SetFloat("_Metallic", metal ? .63f : .02f);
            material.SetFloat("_Smoothness", metal ? .64f : key == "glass" ? .12f : .35f);
            if (key is "green" or "cyan" or "red" or "blue" or "orange" or "amber" or "mint" or "warm" or "ice")
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", color * .63f);
            }
            if (key is "glass" or "glass_body" or "glass_soft" or "acrylic" or "glassblue" or "film" or "Glass" or "guardglass")
            {
                color.a = key=="film" ? .63f : .10f;
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


