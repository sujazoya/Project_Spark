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
    public static class SparkLevel62ModelInstaller
    {
        public const string Folder = "Assets/My_Assets/_Project_Spark/Prefabs/Levels/Level_62_Layout/";
        public const string Prefab = Folder + "Level_62_Layout.prefab";
        public const string ScenePath = "Assets/My_Assets/_Project_Spark/Scenes/MainMenu/MainMenu.unity";
        private static readonly string[] Models = {
            "IndustrialVoltageRestorationBase","ThreePhaseGridSource","ThreePhaseInputBus","MainInputContactor","SeriesInjectionTransformerA","SeriesInjectionTransformerB","SeriesInjectionTransformerC","DVRPowerConverter","DVROutputFilter","DVRDCLinkCapacitor","SupercapacitorEnergyBank","BidirectionalDCConverter","PrechargeProtectionUnit","SeriesBypassSwitch","InjectionContactorBank","InputVoltageSensor","OutputVoltageSensor","InjectionVoltageSensor","ThreePhaseCurrentSensor","PhaseReferencePLL","VoltageSagSwellDetector","DVRMasterController","VoltageCompensationController","SensitiveElectronicsLoad","PrecisionIndustrialMotor","HeavyMotorDisturbanceLoad","ProgrammableDisturbanceSimulator","PowerQualityEventRecorder","ThermalCoolingAssembly","ProtectionRelayModule","HolographicVoltageMonitor","TransparentSafetyEnclosure"
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
            ("PLL_5V","PLL 5V",-0.5120f,0.5900f,"red"),
            ("PLL_GND","PLL GND",-0.4325f,0.5900f,"blue"),
            ("DVR_5V","DVR 5V",-0.3530f,0.5900f,"red"),
            ("DVR_GND","DVR GND",-0.2735f,0.5900f,"blue"),
            ("SENSE_5V","SENSE 5V",-0.1940f,0.5900f,"red"),
            ("SENSE_GND","SENSE GND",-0.1145f,0.5900f,"blue"),
            ("GRID_A","GRID A",-0.0350f,0.5900f,"cyan"),
            ("MAIN_A","MAIN A",0.0445f,0.5900f,"cyan"),
            ("GRID_B","GRID B",0.1240f,0.5900f,"amber"),
            ("MAIN_B","MAIN B",0.2035f,0.5900f,"amber"),
            ("GRID_C","GRID C",0.2830f,0.5900f,"violet"),
            ("MAIN_C","MAIN C",0.3625f,0.5900f,"violet"),
            ("MAIN_OUT_A","MAIN OUT A",0.4420f,0.5900f,"cyan"),
            ("INPUT_A","INPUT A",0.5215f,0.5900f,"cyan"),
            ("MAIN_OUT_B","MAIN OUT B",0.6010f,0.5900f,"amber"),
            ("INPUT_B","INPUT B",0.6805f,0.5900f,"amber"),
            ("MAIN_OUT_C","MAIN OUT C",0.7600f,0.5900f,"violet"),
            ("INPUT_C","INPUT C",0.8395f,0.5900f,"violet"),
            ("TX_A_IN","TX A IN",0.9190f,0.5900f,"mint"),
            ("TX_B_IN","TX B IN",0.9985f,0.5900f,"mint"),
            ("TX_C_IN","TX C IN",-0.9940f,-0.4000f,"mint"),
            ("TX_A_OUT","TX A OUT",-0.9076f,-0.4000f,"mint"),
            ("PROTECTED_A","PROTECTED A",-0.8212f,-0.4000f,"cyan"),
            ("TX_B_OUT","TX B OUT",-0.7348f,-0.4000f,"mint"),
            ("PROTECTED_B","PROTECTED B",-0.6484f,-0.4000f,"amber"),
            ("TX_C_OUT","TX C OUT",-0.5620f,-0.4000f,"mint"),
            ("PROTECTED_C","PROTECTED C",-0.4756f,-0.4000f,"violet"),
            ("DVR_A","DVR A",-0.3892f,-0.4000f,"cyan"),
            ("FILTER_A","FILTER A",-0.3028f,-0.4000f,"cyan"),
            ("DVR_B","DVR B",-0.2164f,-0.4000f,"amber"),
            ("FILTER_B","FILTER B",-0.1300f,-0.4000f,"amber"),
            ("DVR_C","DVR C",-0.0436f,-0.4000f,"violet"),
            ("FILTER_C","FILTER C",0.0428f,-0.4000f,"violet"),
            ("TX_A_SEC","TX A SEC",0.1292f,-0.4000f,"mint"),
            ("TX_B_SEC","TX B SEC",0.2156f,-0.4000f,"mint"),
            ("TX_C_SEC","TX C SEC",0.3020f,-0.4000f,"mint"),
            ("STORE_POS","STORE POS",0.3884f,-0.4000f,"red"),
            ("CONVERTER_POS","CONVERTER POS",0.4748f,-0.4000f,"red"),
            ("STORE_NEG","STORE NEG",0.5612f,-0.4000f,"blue"),
            ("DC_NEG","DC NEG",0.6476f,-0.4000f,"blue"),
            ("CONVERTER_OUT","CONVERTER OUT",0.7340f,-0.4000f,"mint"),
            ("DC_POS","DC POS",0.8204f,-0.4000f,"red"),
            ("DVR_POS","DVR POS",0.9068f,-0.4000f,"red"),
            ("DVR_NEG","DVR NEG",0.9932f,-0.4000f,"blue"),
            ("CTRL_PRE","CTRL PRE",-0.9940f,-0.5600f,"mint"),
            ("PRECHARGE_CMD","PRECHARGE CMD",-0.9076f,-0.5600f,"mint"),
            ("LINK_OUT","LINK OUT",-0.8212f,-0.5600f,"mint"),
            ("CTRL_LINK","CTRL LINK",-0.7348f,-0.5600f,"mint"),
            ("INPUT_VA","INPUT VA",-0.6484f,-0.5600f,"mint"),
            ("INPUT_VB","INPUT VB",-0.5620f,-0.5600f,"mint"),
            ("INPUT_VC","INPUT VC",-0.4756f,-0.5600f,"mint"),
            ("OUTPUT_VA","OUTPUT VA",-0.3892f,-0.5600f,"mint"),
            ("OUTPUT_VB","OUTPUT VB",-0.3028f,-0.5600f,"mint"),
            ("OUTPUT_VC","OUTPUT VC",-0.2164f,-0.5600f,"mint"),
            ("INPUT_SENSOR_OUT","SENSE IN OUT",-0.1300f,-0.5600f,"mint"),
            ("PLL_IN","PLL IN",-0.0436f,-0.5600f,"mint"),
            ("INPUT_CT","INPUT CT",0.0428f,-0.5600f,"mint"),
            ("CTRL_INPUT_I","CTRL INPUT I",0.1292f,-0.5600f,"mint"),
            ("OUTPUT_CT","OUTPUT CT",0.2156f,-0.5600f,"mint"),
            ("CTRL_OUTPUT_I","CTRL OUTPUT I",0.3020f,-0.5600f,"mint"),
            ("MOTOR_A","MOTOR A",0.3884f,-0.5600f,"cyan"),
            ("MOTOR_B","MOTOR B",0.4748f,-0.5600f,"amber"),
            ("MOTOR_C","MOTOR C",0.5612f,-0.5600f,"violet"),
            ("SENSITIVE_IN","SENSITIVE IN",0.6476f,-0.5600f,"mint"),
            ("DISTURBANCE_BUS","DIST BUS",0.7340f,-0.5600f,"mint"),
            ("HEAVY_IN","HEAVY IN",0.8204f,-0.5600f,"mint"),
            ("CTRL_EVENT","CTRL EVENT",0.9068f,-0.5600f,"mint"),
            ("EVENT_CMD","EVENT CMD",0.9932f,-0.5600f,"mint"),
            ("CTRL_BYPASS","CTRL BYPASS",-0.9000f,-0.7000f,"mint"),
            ("BYPASS_CMD","BYPASS CMD",-0.7490f,-0.7000f,"mint"),
            ("PROTECTION_OUT","PROTECTION OUT",-0.5980f,-0.7000f,"mint"),
            ("CTRL_PROTECT","CTRL PROTECT",-0.4470f,-0.7000f,"mint"),
            ("PLL_OUT","PLL OUT",-0.2960f,-0.7000f,"mint"),
            ("CTRL_PHASE","CTRL PHASE",-0.1450f,-0.7000f,"mint"),
            ("BYPASS_AUX","BYPASS AUX",0.0060f,-0.7000f,"mint"),
            ("CTRL_AUX","CTRL AUX",0.1570f,-0.7000f,"mint"),
            ("GUARD_NC","GUARD NC",0.3080f,-0.7000f,"mint"),
            ("CTRL_SAFE","CTRL SAFE",0.4590f,-0.7000f,"mint"),
            ("INJECTION_V_FEEDBACK","INJ V OUT",0.6100f,-0.7000f,"mint"),
            ("DVR_V_IN","DVR V IN",0.7610f,-0.7000f,"mint"),
        };

        public static IEnumerable<string> TerminalIds => Sockets.Select(socket => socket.id);

        private static readonly Dictionary<string, string> Palette = new Dictionary<string, string>
        {
            {"acrylic","56BAE1"},
            {"acrylic_rim","71B5D9"},
            {"amber","FFA82B"},
            {"blue","1671E2"},
            {"blue_steel","2D4C64"},
            {"carbon","091119"},
            {"chrome","C2DFEC"},
            {"copper","CA591F"},
            {"cyan","23DAFD"},
            {"dark","111A26"},
            {"display","04111C"},
            {"gold","FCB242"},
            {"green","1AF471"},
            {"mint","34F6C5"},
            {"navy","0C1829"},
            {"red","EE485C"},
            {"rubber","090D14"},
            {"silver","80AAC4"},
            {"silver_dark","54738D"},
            {"titanium","476479"},
            {"violet","A282FF"},
            {"void","07111C"},
            {"white","D4F5FF"},
        };

        [MenuItem("Tools/Project Spark/Levels/Create Level 62")]
        public static void Install()
        {
            SparkLevel62Builder.Create();
            var root = AssetDatabase.LoadAssetAtPath<GameObject>(Prefab);
            if (root == null || root.GetComponent<SparkLevel62VoltageRestoration>().controllerDisplay == null) Fit();
            Register();
            ValidateAssets();
        }

        [MenuItem("Tools/Project Spark/Level 62/Validate Level Assets")]
        public static void ValidateAssets()
        {
            var root=AssetDatabase.LoadAssetAtPath<GameObject>(Prefab);
            var control=root != null ? root.GetComponent<SparkLevel62VoltageRestoration>() : null;
            var bindings=root != null ? root.GetComponent<SparkLevelSceneBindings>() : null;
            if (control==null || bindings==null || control.controllerDisplay==null || control.qualityDisplay==null || control.rig==null ||
                control.pins.Length!=84 || control.pins.Distinct().Count()!=84 || 
                !TerminalIds.SequenceEqual(control.pins.Select(pin=>pin.name)) ||
                control.pins.Any(pin=>!bindings.TryGetTerminal(pin.name,out var bound) || bound!=pin) ||
                SparkLevel62VoltageRestoration.Gates.Any(id=>!bindings.IsComponentIdDeclared(id)))
                throw new InvalidOperationException("Level 62 wiring, motion or binding references are incomplete.");
            Debug.Log("[LEVEL 62] Validated 84 distinct terminals, 54 player wires and 6 evaluator gates.",root);
        }

        private static void Register()
        {
            var scene = SceneManager.GetSceneByPath(ScenePath);
            bool opened = !scene.isLoaded;
            if (opened) scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
            bool dirty = scene.isDirty;
            var objects = scene.GetRootGameObjects();
            T Find<T>() where T : Component => objects.SelectMany(item => item.GetComponentsInChildren<T>(true)).Single();
            var level = AssetDatabase.LoadAssetAtPath<SparkLevelDefinition>(Folder + "Level_62.asset");
            var drawing = AssetDatabase.LoadAssetAtPath<SparkLevelDrawingAsset>(Folder + "Level_62_Drawing.asset");
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
            else if (dirty) Debug.Log("MainMenu had unsaved edits. Save the scene to persist Level 62's campaign and teaching entries.");
        }

        private static void Append(Object target, string field, Object value)
        {
            if (value == null) throw new InvalidOperationException("Missing Level 62 asset for " + field);
            var data = new SerializedObject(target);
            var list = data.FindProperty(field);
            for (int i = 0; i < list.arraySize; i++)
                if (list.GetArrayElementAtIndex(i).objectReferenceValue == value) return;
            list.arraySize++;
            list.GetArrayElementAtIndex(list.arraySize - 1).objectReferenceValue = value;
            data.ApplyModifiedProperties();
        }

        [MenuItem("Tools/Project Spark/Level 62/Fit Breadboard Assembly")]
        public static void Fit()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Exit Play Mode before fitting Level 62.");
            var models = Models.Select((name, index) =>
            {
                string path = Folder + $"Models/L62_{index:00}_{name}_Fitted.fbx";
                return AssetDatabase.LoadAssetAtPath<GameObject>(path) ??
                    throw new InvalidOperationException("Missing fitted kit module " + path);
            }).ToArray();
            if (AssetDatabase.LoadAssetAtPath<GameObject>(Prefab) == null)
                throw new InvalidOperationException("The Level 62 prefab is missing.");

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
                    throw new InvalidOperationException("Unity could not save the Level 62 layout.");
                AssetDatabase.SaveAssets();
                Debug.Log("Level 62 fitted to the MainMenu breadboard with 84 seated terminals.");
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
            Vector3 right = Vector3.ProjectOnPlane(grid.GridOrigin.right, up).normalized * .062f;
            Vector3 forward = Vector3.ProjectOnPlane(grid.GridOrigin.forward, up).normalized * .062f;
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
            var bench = root.transform.Find("Voltage Workbench");
            var bindings = root.GetComponent<SparkLevelSceneBindings>();
            var loop = root.GetComponent<SparkLevel62VoltageRestoration>();
            if (bench == null || bindings == null || loop == null)
                throw new InvalidOperationException("Level 62 is missing its workbench, controls, or scene bindings.");

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

            foreach (string name in new[] {"Fitted Level 62 Models", "Breadboard Labels", "Motion pivots"})
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
            var assembly = Child(bench, "Fitted Level 62 Models", Vector3.up * floor);
            foreach (var model in models)
            {
                var instance = Object.Instantiate(model, assembly, false).transform;
                var markers = instance.GetComponentsInChildren<Transform>(true)
                    .Where(item => item.name.StartsWith("L62_Board",StringComparison.Ordinal))
                    .ToDictionary(item => item.name);
                Transform Marker(string name) => markers["L62_Board" + name];
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
                var oldControls = root.transform.Find("Level 62 Controls");
                if (oldControls != null) Object.DestroyImmediate(oldControls.gameObject);
                SparkLevel62Builder.BuildControls(root.transform,loop,font);
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
                terminalData.FindProperty("maxConnections").intValue = SparkLevel62VoltageRestoration.Wires.Count(wire => wire.a == index || wire.b == index);
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






                ("SOURCE / DISTURBANCE",-.55f,.32f),("SERIES INJECTION",.54f,.32f),
                ("DC LINK / STORAGE",-.55f,-.27f),("PROTECTED LOADS",.54f,-.27f)
            };
            foreach(var title in titles)
                Label(labels,font,title.title,title.title,new Vector3(title.x,floor+.028f,title.z),.018f);
            Transform Pivot(string module,string key)
            {
                var name="fitted_"+module+"_"+key;
                return assembly.GetComponentsInChildren<Transform>(true).Single(item=>item.name==name);
            }
            loop.rig.main=Pivot("MainInputContactor","ANIM_MAIN_CONTACTOR");
            loop.rig.bypass=Pivot("SeriesBypassSwitch","ANIM_BYPASS_HANDLE");
            loop.rig.inject=Pivot("InjectionContactorBank","ANIM_INJECTION_CONTACTORS");
            loop.rig.motor=Pivot("PrecisionIndustrialMotor","ANIM_PRECISION_MOTOR");
            loop.rig.heavy=Pivot("HeavyMotorDisturbanceLoad","ANIM_DISTURBANCE_MOTOR");
            loop.rig.fans=Pivot("ThermalCoolingAssembly","ANIM_COOLING_FANS");
            loop.rig.monitor=Pivot("HolographicVoltageMonitor","ANIM_MONITOR_TILT");
            loop.rig.guard=Pivot("TransparentSafetyEnclosure","ANIM_SAFETY_DOOR");

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
            string path = $"{Materials}/L62 {key}.mat";
            var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing != null && key is not ("glass" or "acrylic") &&
                ColorUtility.TryParseHtmlString("#" + (Palette.TryGetValue(key, out var savedTint) ? savedTint : "9FB3C8"), out var savedColor) &&
                existing.GetColor("_BaseColor") == savedColor) return existing;
            ColorUtility.TryParseHtmlString("#" + (Palette.TryGetValue(key, out var tint) ? tint : "9FB3C8"), out var color);
            var material = existing != null ? existing : new Material(shader) { name = "L62 " + key };
            bool metal = key is "chrome" or "titanium" or "steel" or "gold" or "copper" or "aluminum";
            material.SetFloat("_Metallic", metal ? .62f : .02f);
            material.SetFloat("_Smoothness", metal ? .64f : key == "glass" ? .12f : .35f);
            if (key is "green" or "cyan" or "red" or "blue" or "orange" or "amber" or "mint" or "warm" or "ice")
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", color * .62f);
            }
            if (key is "glass" or "glass_body" or "glass_soft" or "acrylic" or "glassblue" or "film" or "Glass" or "guardglass")
            {
                color.a = key=="film" ? .62f : .10f;
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


