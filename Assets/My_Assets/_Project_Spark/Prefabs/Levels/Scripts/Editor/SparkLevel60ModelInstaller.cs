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
    public static class SparkLevel60ModelInstaller
    {
        public const string Folder = "Assets/My_Assets/_Project_Spark/Prefabs/Levels/Level_60_Layout/";
        public const string Prefab = Folder + "Level_60_Layout.prefab";
        public const string ScenePath = "Assets/My_Assets/_Project_Spark/Scenes/MainMenu/MainMenu.unity";
        private static readonly string[] Models = {
            "IndustrialResilienceBase","MainGridInterface","GridFormingInverter","BatteryStorageSystem","BatteryManagementUnit","BidirectionalPowerConverter","SolarPVSimulator","SolarMPPTController","BackupGenerator","GeneratorStarter","ThreePhaseDistributionBus","MainGridBreaker","GeneratorBreaker","BatteryInverterBreaker","RenewableSourceBreaker","EssentialLoadBreaker","PriorityLoadBreaker","IndustrialLoadBreaker","AuxiliaryLoadBreaker","CriticalServiceLoad","IndustrialMotorLoad","SmartProgrammableLoad","ProtectionRelayBank","VoltageCurrentSensorHub","ResilienceMasterController","HolographicSCADAMonitor","FaultScenarioSimulator","TransparentSafetyEnclosure"
        };
        private const string Materials = Folder + "Models/Materials";
        private const float SocketHeight = .025f;
        private const float SocketDiameter = .032f;

        private static readonly (string id, string label, float x, float z, string color)[] Sockets =
        {
            ("SUPPLY_POS","SUPPLY POS",-0.8300f,0.5900f,"red"),
            ("CTRL_POS","CTRL POS",-0.7505f,0.5900f,"red"),
            ("SUPPLY_RET","SUPPLY RET",-0.6710f,0.5900f,"blue"),
            ("CTRL_GND","CTRL GND",-0.5915f,0.5900f,"blue"),
            ("BMS_5V","BMS 5V",-0.5120f,0.5900f,"red"),
            ("BMS_GND","BMS GND",-0.4325f,0.5900f,"blue"),
            ("SENSE_5V","SENSE 5V",-0.3530f,0.5900f,"red"),
            ("SENSE_GND","SENSE GND",-0.2735f,0.5900f,"blue"),
            ("SAFETY_5V","SAFETY 5V",-0.1940f,0.5900f,"red"),
            ("SAFETY_GND","SAFETY GND",-0.1145f,0.5900f,"blue"),
            ("BAT_POS","BAT POS",-0.0350f,0.5900f,"red"),
            ("BMS_BAT_POS","BMS BAT POS",0.0445f,0.5900f,"mint"),
            ("BAT_NEG","BAT NEG",0.1240f,0.5900f,"blue"),
            ("DC_NEG","DC NEG",0.2035f,0.5900f,"blue"),
            ("BMS_OUT","BMS OUT",0.2830f,0.5900f,"mint"),
            ("CONV_BAT","CONV BAT",0.3625f,0.5900f,"mint"),
            ("CONV_DC","CONV DC",0.4420f,0.5900f,"mint"),
            ("DC_POS","DC POS",0.5215f,0.5900f,"red"),
            ("INV_DC_POS","INV DC POS",0.6010f,0.5900f,"red"),
            ("INV_DC_NEG","INV DC NEG",0.6805f,0.5900f,"blue"),
            ("EMS_INV","EMS INV",0.7600f,0.5900f,"mint"),
            ("INV_ENABLE","INV ENABLE",0.8395f,0.5900f,"mint"),
            ("INV_A","INV A",0.9190f,0.5900f,"cyan"),
            ("BUS_A","BUS A",0.9985f,0.5900f,"cyan"),
            ("INV_B","INV B",-0.9940f,-0.4240f,"amber"),
            ("BUS_B","BUS B",-0.9076f,-0.4240f,"amber"),
            ("INV_C","INV C",-0.8212f,-0.4240f,"violet"),
            ("BUS_C","BUS C",-0.7348f,-0.4240f,"violet"),
            ("PV_POS","PV POS",-0.6484f,-0.4240f,"mint"),
            ("MPPT_POS","MPPT POS",-0.5620f,-0.4240f,"mint"),
            ("PV_NEG","PV NEG",-0.4756f,-0.4240f,"blue"),
            ("MPPT_NEG","MPPT NEG",-0.3892f,-0.4240f,"blue"),
            ("MPPT_DC","MPPT DC",-0.3028f,-0.4240f,"mint"),
            ("MPPT_RET","MPPT RET",-0.2164f,-0.4240f,"blue"),
            ("EMS_START","EMS START",-0.1300f,-0.4240f,"mint"),
            ("START_CMD","START CMD",-0.0436f,-0.4240f,"mint"),
            ("START_POS","START POS",0.0428f,-0.4240f,"red"),
            ("START_NEG","START NEG",0.1292f,-0.4240f,"blue"),
            ("GEN_A","GEN A",0.2156f,-0.4240f,"cyan"),
            ("QGEN_A","QGEN A",0.3020f,-0.4240f,"cyan"),
            ("GEN_B","GEN B",0.3884f,-0.4240f,"amber"),
            ("QGEN_B","QGEN B",0.4748f,-0.4240f,"amber"),
            ("GEN_C","GEN C",0.5612f,-0.4240f,"violet"),
            ("QGEN_C","QGEN C",0.6476f,-0.4240f,"violet"),
            ("GEN_SYNC","GEN SYNC",0.7340f,-0.4240f,"mint"),
            ("SYNC_GEN","SYNC GEN",0.8204f,-0.4240f,"mint"),
            ("GRID_A","GRID A",0.9068f,-0.4240f,"cyan"),
            ("QGRID_A","QGRID A",0.9932f,-0.4240f,"cyan"),
            ("GRID_B","GRID B",-0.9940f,-0.5900f,"amber"),
            ("QGRID_B","QGRID B",-0.9076f,-0.5900f,"amber"),
            ("GRID_C","GRID C",-0.8212f,-0.5900f,"violet"),
            ("QGRID_C","QGRID C",-0.7348f,-0.5900f,"violet"),
            ("GRID_SYNC","GRID SYNC",-0.6484f,-0.5900f,"mint"),
            ("SYNC_GRID","SYNC GRID",-0.5620f,-0.5900f,"mint"),
            ("ESS_FEED","ESS FEED",-0.4756f,-0.5900f,"mint"),
            ("PRI_FEED","PRI FEED",-0.3892f,-0.5900f,"mint"),
            ("MOTOR_A","MOTOR A",-0.3028f,-0.5900f,"cyan"),
            ("MOTOR_B","MOTOR B",-0.2164f,-0.5900f,"amber"),
            ("MOTOR_C","MOTOR C",-0.1300f,-0.5900f,"violet"),
            ("AUX_FEED","AUX FEED",-0.0436f,-0.5900f,"mint"),
            ("SENSE_BUS","SENSE BUS",0.0428f,-0.5900f,"mint"),
            ("SENSE_OUT","SENSE OUT",0.1292f,-0.5900f,"mint"),
            ("EMS_SENSE","EMS SENSE",0.2156f,-0.5900f,"mint"),
            ("EMS_METER","EMS METER",0.3020f,-0.5900f,"mint"),
            ("BMS_STATUS","BMS STATUS",0.3884f,-0.5900f,"mint"),
            ("EMS_BMS","EMS BMS",0.4748f,-0.5900f,"mint"),
            ("MPPT_STATUS","MPPT STATUS",0.5612f,-0.5900f,"mint"),
            ("EMS_SOLAR","EMS SOLAR",0.6476f,-0.5900f,"mint"),
            ("PROTECT_OUT","PROTECT OUT",0.7340f,-0.5900f,"mint"),
            ("EMS_PROTECT","EMS PROTECT",0.8204f,-0.5900f,"mint"),
            ("GUARD_NC","GUARD NC",0.9068f,-0.5900f,"mint"),
            ("CTRL_SAFE","CTRL SAFE",0.9932f,-0.5900f,"mint"),
        };

        public static IEnumerable<string> TerminalIds => Sockets.Select(socket => socket.id);

        private static readonly Dictionary<string, string> Palette = new Dictionary<string, string>
        {
            {"acrylic","56B9E0"},
            {"acrylic_rim","70B3D8"},
            {"amber","FFA82B"},
            {"blue","1671E2"},
            {"blue_steel","2C4B63"},
            {"carbon","091119"},
            {"chrome","C0DDEA"},
            {"copper","C8581F"},
            {"cyan","23DAFD"},
            {"dark","111A26"},
            {"display","04111C"},
            {"electric","0D92FF"},
            {"green","1AF270"},
            {"mint","34F6C5"},
            {"navy","0C1828"},
            {"red","EE485C"},
            {"rubber","090D14"},
            {"silver","7FA9C3"},
            {"silver_dark","53728C"},
            {"titanium","466378"},
            {"violet","A282FF"},
            {"void","07111C"},
            {"white","D4F5FF"},
        };

        [MenuItem("Tools/Project Spark/Levels/Create Level 60")]
        public static void Install()
        {
            SparkLevel60Builder.Create();
            var root = AssetDatabase.LoadAssetAtPath<GameObject>(Prefab);
            if (root == null || root.GetComponent<SparkLevel60Resilience>().controllerDisplay == null) Fit();
            Register();
            ValidateAssets();
        }

        [MenuItem("Tools/Project Spark/Level 60/Validate Level Assets")]
        public static void ValidateAssets()
        {
            var root=AssetDatabase.LoadAssetAtPath<GameObject>(Prefab);
            var control=root != null ? root.GetComponent<SparkLevel60Resilience>() : null;
            var bindings=root != null ? root.GetComponent<SparkLevelSceneBindings>() : null;
            if (control==null || bindings==null || control.controllerDisplay==null || control.qualityDisplay==null || control.rig==null ||
                control.pins.Length!=72 || control.pins.Distinct().Count()!=72 || 
                !TerminalIds.SequenceEqual(control.pins.Select(pin=>pin.name)) ||
                control.pins.Any(pin=>!bindings.TryGetTerminal(pin.name,out var bound) || bound!=pin) ||
                SparkLevel60Resilience.Gates.Any(id=>!bindings.IsComponentIdDeclared(id)))
                throw new InvalidOperationException("Level 60 wiring, motion or binding references are incomplete.");
            Debug.Log("[LEVEL 60] Validated 72 distinct terminals, 46 player wires and 6 evaluator gates.",root);
        }

        private static void Register()
        {
            var scene = SceneManager.GetSceneByPath(ScenePath);
            bool opened = !scene.isLoaded;
            if (opened) scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
            bool dirty = scene.isDirty;
            var objects = scene.GetRootGameObjects();
            T Find<T>() where T : Component => objects.SelectMany(item => item.GetComponentsInChildren<T>(true)).Single();
            var level = AssetDatabase.LoadAssetAtPath<SparkLevelDefinition>(Folder + "Level_60.asset");
            var drawing = AssetDatabase.LoadAssetAtPath<SparkLevelDrawingAsset>(Folder + "Level_60_Drawing.asset");
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
            else if (dirty) Debug.Log("MainMenu had unsaved edits. Save the scene to persist Level 60's campaign and teaching entries.");
        }

        private static void Append(Object target, string field, Object value)
        {
            if (value == null) throw new InvalidOperationException("Missing Level 60 asset for " + field);
            var data = new SerializedObject(target);
            var list = data.FindProperty(field);
            for (int i = 0; i < list.arraySize; i++)
                if (list.GetArrayElementAtIndex(i).objectReferenceValue == value) return;
            list.arraySize++;
            list.GetArrayElementAtIndex(list.arraySize - 1).objectReferenceValue = value;
            data.ApplyModifiedProperties();
        }

        [MenuItem("Tools/Project Spark/Level 60/Fit Breadboard Assembly")]
        public static void Fit()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Exit Play Mode before fitting Level 60.");
            var models = Models.Select((name, index) =>
            {
                string path = Folder + $"Models/L60_{index:00}_{name}_Fitted.fbx";
                return AssetDatabase.LoadAssetAtPath<GameObject>(path) ??
                    throw new InvalidOperationException("Missing fitted kit module " + path);
            }).ToArray();
            if (AssetDatabase.LoadAssetAtPath<GameObject>(Prefab) == null)
                throw new InvalidOperationException("The Level 60 prefab is missing.");

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
                    throw new InvalidOperationException("Unity could not save the Level 60 layout.");
                AssetDatabase.SaveAssets();
                Debug.Log("Level 60 fitted to the MainMenu breadboard with 72 seated terminals.");
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
            Vector3 right = Vector3.ProjectOnPlane(grid.GridOrigin.right, up).normalized * .060f;
            Vector3 forward = Vector3.ProjectOnPlane(grid.GridOrigin.forward, up).normalized * .060f;
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
            var bench = root.transform.Find("Resilience Workbench");
            var bindings = root.GetComponent<SparkLevelSceneBindings>();
            var loop = root.GetComponent<SparkLevel60Resilience>();
            if (bench == null || bindings == null || loop == null)
                throw new InvalidOperationException("Level 60 is missing its workbench, controls, or scene bindings.");

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
            float worldScale = Mathf.Min(width / 2.05f, depth / 1.36f);
            bench.localScale = Vector3.one * (worldScale / parentWorld.MultiplyVector(Vector3.right).magnitude);
            Matrix4x4 workbenchWorld = layout * bench.localToWorldMatrix;
            Vector3 ToWorld(Vector3 point) => workbenchWorld.MultiplyPoint3x4(point);
            Vector3 ToLocal(Vector3 point) => workbenchWorld.inverse.MultiplyPoint3x4(point);
            float floor = ToLocal(SurfacePoint(grid, ToWorld(Vector3.zero), up)).y;

            foreach (string name in new[] {"Fitted Level 60 Models", "Breadboard Labels", "Motion pivots"})
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
            var assembly = Child(bench, "Fitted Level 60 Models", Vector3.up * floor);
            foreach (var model in models)
            {
                var instance = Object.Instantiate(model, assembly, false).transform;
                var markers = instance.GetComponentsInChildren<Transform>(true)
                    .Where(item => item.name.StartsWith("L60_Board",StringComparison.Ordinal))
                    .ToDictionary(item => item.name);
                Transform Marker(string name) => markers["L60_Board" + name];
                Vector3 Axis(string name) => assembly.InverseTransformVector(Marker(name).position - Marker("Center").position);
                instance.localRotation = Quaternion.Inverse(Quaternion.LookRotation(-Axis("Front"), Axis("Up"))) * instance.localRotation;
                instance.localScale /= Axis("Right").magnitude;
                instance.localPosition -= assembly.InverseTransformPoint(Marker("Center").position);
                if (Axis("Right").x < .99f)
                    throw new InvalidOperationException("Unexpected axes on fitted model " + model.name);
            }

            assembly.localScale = Vector3.one * .94f;
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
                var oldControls = root.transform.Find("Level 60 Controls");
                if (oldControls != null) Object.DestroyImmediate(oldControls.gameObject);
                SparkLevel60Builder.BuildControls(root.transform,loop,font);
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






                ("BATTERY / INVERTER",-.63f,.35f),("BACKUP GENERATOR",.53f,.35f),
                ("RESTORATION FEEDERS",0f,-.28f),("ENERGY / PROTECTION",.63f,-.28f)
            };
            foreach(var title in titles)
                Label(labels,font,title.title,title.title,new Vector3(title.x,floor+.028f,title.z),.018f);
            Transform Pivot(string module,string key)
            {
                var name="fitted_"+module+"_"+key;
                return assembly.GetComponentsInChildren<Transform>(true).Single(item=>item.name==name);
            }
            loop.rig.handles=new[]{"MainGridBreaker","GeneratorBreaker","BatteryInverterBreaker","RenewableSourceBreaker","EssentialLoadBreaker","PriorityLoadBreaker","IndustrialLoadBreaker","AuxiliaryLoadBreaker"}.Select(module=>Pivot(module,"ANIM_BREAKER_HANDLE")).ToArray();
            loop.rig.rotor=Pivot("BackupGenerator","ANIM_GENERATOR_ROTOR");
            loop.rig.starter=Pivot("GeneratorStarter","ANIM_STARTER_PINION");
            loop.rig.solar=Pivot("SolarPVSimulator","ANIM_SOLAR_TILT");
            loop.rig.motor=Pivot("IndustrialMotorLoad","ANIM_MOTOR_SPIN");
            loop.rig.monitor=Pivot("HolographicSCADAMonitor","ANIM_MONITOR_TILT");
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
            string path = $"{Materials}/L60 {key}.mat";
            var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing != null && key is not ("glass" or "acrylic") &&
                ColorUtility.TryParseHtmlString("#" + (Palette.TryGetValue(key, out var savedTint) ? savedTint : "9FB3C8"), out var savedColor) &&
                existing.GetColor("_BaseColor") == savedColor) return existing;
            ColorUtility.TryParseHtmlString("#" + (Palette.TryGetValue(key, out var tint) ? tint : "9FB3C8"), out var color);
            var material = existing != null ? existing : new Material(shader) { name = "L60 " + key };
            bool metal = key is "chrome" or "titanium" or "steel" or "gold" or "copper" or "aluminum";
            material.SetFloat("_Metallic", metal ? .60f : .02f);
            material.SetFloat("_Smoothness", metal ? .64f : key == "glass" ? .12f : .35f);
            if (key is "green" or "cyan" or "red" or "blue" or "orange" or "amber" or "mint" or "warm" or "ice")
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", color * .60f);
            }
            if (key is "glass" or "glass_body" or "glass_soft" or "acrylic" or "glassblue" or "film" or "Glass" or "guardglass")
            {
                color.a = key=="film" ? .60f : .10f;
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

