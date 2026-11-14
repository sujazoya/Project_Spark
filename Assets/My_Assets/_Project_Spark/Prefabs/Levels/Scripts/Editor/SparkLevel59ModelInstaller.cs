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
    public static class SparkLevel59ModelInstaller
    {
        public const string Folder = "Assets/My_Assets/_Project_Spark/Prefabs/Levels/Level_59_Layout/";
        public const string Prefab = Folder + "Level_59_Layout.prefab";
        public const string ScenePath = "Assets/My_Assets/_Project_Spark/Scenes/MainMenu/MainMenu.unity";
        private static readonly string[] Models = {
            "IndustrialBlackStartBase","BatteryStorageBank","BatteryManagementSystem","GridFormingInverter","DCPrechargeModule","DCPowerDistributionBus","ThreePhaseACBus","BackupGenerator","GeneratorStarter","GeneratorSynchronizationRelay","MainGridBreaker","GeneratorBreaker","EssentialLoadBreakerA","EssentialLoadBreakerB","EssentialLoadBreakerC","NonessentialLoadBreakerA","NonessentialLoadBreakerB","EmergencyLightingLoad","CriticalCoolingLoad","IndustrialMotorLoad","VoltageFrequencySensorHub","PowerEnergyMeter","RestorationSequencer","HolographicRestorationMonitor","TransparentSafetyEnclosure","Regulated5VControlSupply"
        };
        private const string Materials = Folder + "Models/Materials";
        private const float SocketHeight = .025f;
        private const float SocketDiameter = .032f;

        private static readonly (string id, string label, float x, float z, string color)[] Sockets =
        {
            ("SUPPLY_POS","SUPPLY POS",-0.8400f,0.5900f,"red"),
            ("CTRL_POS","CTRL POS",-0.7530f,0.5900f,"red"),
            ("SUPPLY_RET","SUPPLY RET",-0.6660f,0.5900f,"blue"),
            ("CTRL_GND","CTRL GND",-0.5790f,0.5900f,"blue"),
            ("BMS_5V","BMS 5V",-0.4920f,0.5900f,"red"),
            ("BMS_GND","BMS GND",-0.4050f,0.5900f,"blue"),
            ("SENSE_5V","SENSE 5V",-0.3180f,0.5900f,"red"),
            ("SENSE_GND","SENSE GND",-0.2310f,0.5900f,"blue"),
            ("SAFETY_5V","SAFETY 5V",-0.1440f,0.5900f,"red"),
            ("SAFETY_GND","SAFETY GND",-0.0570f,0.5900f,"blue"),
            ("BAT_POS","BAT POS",0.0300f,0.5900f,"red"),
            ("BMS_BAT_POS","BMS BAT POS",0.1170f,0.5900f,"mint"),
            ("BAT_NEG","BAT NEG",0.2040f,0.5900f,"blue"),
            ("DC_NEG","DC NEG",0.2910f,0.5900f,"blue"),
            ("BMS_OUT","BMS OUT",0.3780f,0.5900f,"mint"),
            ("PRE_IN","PRE IN",0.4650f,0.5900f,"mint"),
            ("PRE_OUT","PRE OUT",0.5520f,0.5900f,"mint"),
            ("DC_POS","DC POS",0.6390f,0.5900f,"red"),
            ("INV_DC_POS","INV DC POS",0.7260f,0.5900f,"red"),
            ("INV_DC_NEG","INV DC NEG",0.8130f,0.5900f,"blue"),
            ("CTRL_ENABLE","CTRL ENABLE",0.9000f,0.5900f,"mint"),
            ("INV_ENABLE","INV ENABLE",0.9870f,0.5900f,"mint"),
            ("INV_A","INV A",-0.9940f,-0.4130f,"cyan"),
            ("BUS_A","BUS A",-0.8995f,-0.4130f,"cyan"),
            ("INV_B","INV B",-0.8050f,-0.4130f,"amber"),
            ("BUS_B","BUS B",-0.7105f,-0.4130f,"amber"),
            ("INV_C","INV C",-0.6160f,-0.4130f,"violet"),
            ("BUS_C","BUS C",-0.5215f,-0.4130f,"violet"),
            ("INV_STATUS","INV STATUS",-0.4270f,-0.4130f,"mint"),
            ("CTRL_INV","CTRL INV",-0.3325f,-0.4130f,"mint"),
            ("CTRL_START","CTRL START",-0.2380f,-0.4130f,"mint"),
            ("START_CMD","START CMD",-0.1435f,-0.4130f,"mint"),
            ("START_POS","START POS",-0.0490f,-0.4130f,"red"),
            ("START_NEG","START NEG",0.0455f,-0.4130f,"blue"),
            ("GEN_A","GEN A",0.1400f,-0.4130f,"cyan"),
            ("QGEN_A","QGEN A",0.2345f,-0.4130f,"cyan"),
            ("GEN_B","GEN B",0.3290f,-0.4130f,"amber"),
            ("QGEN_B","QGEN B",0.4235f,-0.4130f,"amber"),
            ("GEN_C","GEN C",0.5180f,-0.4130f,"violet"),
            ("QGEN_C","QGEN C",0.6125f,-0.4130f,"violet"),
            ("GEN_V","GEN V",0.7070f,-0.4130f,"mint"),
            ("SYNC_V","SYNC V",0.8015f,-0.4130f,"mint"),
            ("GEN_HZ","GEN HZ",0.8960f,-0.4130f,"mint"),
            ("SYNC_HZ","SYNC HZ",0.9905f,-0.4130f,"mint"),
            ("SYNC_CLOSE","SYNC CLOSE",-0.9940f,-0.5900f,"mint"),
            ("QGEN_CMD","QGEN CMD",-0.8995f,-0.5900f,"mint"),
            ("GRID_REF","GRID REF",-0.8050f,-0.5900f,"mint"),
            ("QG_REF","QG REF",-0.7105f,-0.5900f,"mint"),
            ("QG_AUX","QG AUX",-0.6160f,-0.5900f,"mint"),
            ("CTRL_GRID","CTRL GRID",-0.5215f,-0.5900f,"mint"),
            ("LIGHT_FEED","LIGHT FEED",-0.4270f,-0.5900f,"mint"),
            ("INSTR_FEED","INSTR FEED",-0.3325f,-0.5900f,"mint"),
            ("COOL_FEED","COOL FEED",-0.2380f,-0.5900f,"mint"),
            ("MOTOR_A","MOTOR A",-0.1435f,-0.5900f,"cyan"),
            ("MOTOR_B","MOTOR B",-0.0490f,-0.5900f,"amber"),
            ("MOTOR_C","MOTOR C",0.0455f,-0.5900f,"violet"),
            ("AUX_FEED","AUX FEED",0.1400f,-0.5900f,"mint"),
            ("SENSE_BUS","SENSE BUS",0.2345f,-0.5900f,"mint"),
            ("SENSE_OUT","SENSE OUT",0.3290f,-0.5900f,"mint"),
            ("CTRL_SENSE","CTRL SENSE",0.4235f,-0.5900f,"mint"),
            ("METER_OUT","METER OUT",0.5180f,-0.5900f,"mint"),
            ("CTRL_METER","CTRL METER",0.6125f,-0.5900f,"mint"),
            ("BMS_STATUS","BMS STATUS",0.7070f,-0.5900f,"mint"),
            ("CTRL_BMS","CTRL BMS",0.8015f,-0.5900f,"mint"),
            ("GUARD_NC","GUARD NC",0.8960f,-0.5900f,"mint"),
            ("CTRL_SAFE","CTRL SAFE",0.9905f,-0.5900f,"mint"),
        };

        public static IEnumerable<string> TerminalIds => Sockets.Select(socket => socket.id);

        private static readonly Dictionary<string, string> Palette = new Dictionary<string, string>
        {
            {"acrylic","55B7DE"},
            {"acrylic_rim","6FB2D6"},
            {"amber","FFA82B"},
            {"blue","1671E2"},
            {"blue_steel","2C4B62"},
            {"carbon","091119"},
            {"chrome","BFDBE8"},
            {"copper","C7581F"},
            {"cyan","23DAFD"},
            {"dark","111A25"},
            {"display","04111B"},
            {"electric","0D90FD"},
            {"green","1AF06F"},
            {"mint","34F6C5"},
            {"navy","0C1828"},
            {"red","EE485C"},
            {"rubber","090D13"},
            {"silver","7EA8C2"},
            {"silver_dark","53728B"},
            {"titanium","466277"},
            {"violet","A282FF"},
            {"void","07111C"},
            {"white","D4F5FF"},
        };

        [MenuItem("Tools/Project Spark/Levels/Create Level 59")]
        public static void Install()
        {
            SparkLevel59Builder.Create();
            var root = AssetDatabase.LoadAssetAtPath<GameObject>(Prefab);
            if (root == null || root.GetComponent<SparkLevel59BlackStart>().controllerDisplay == null) Fit();
            Register();
            ValidateAssets();
        }

        [MenuItem("Tools/Project Spark/Level 59/Validate Level Assets")]
        public static void ValidateAssets()
        {
            var root=AssetDatabase.LoadAssetAtPath<GameObject>(Prefab);
            var control=root != null ? root.GetComponent<SparkLevel59BlackStart>() : null;
            var bindings=root != null ? root.GetComponent<SparkLevelSceneBindings>() : null;
            if (control==null || bindings==null || control.controllerDisplay==null || control.qualityDisplay==null || control.rig==null ||
                control.pins.Length!=66 || control.pins.Distinct().Count()!=66 || 
                !TerminalIds.SequenceEqual(control.pins.Select(pin=>pin.name)) ||
                control.pins.Any(pin=>!bindings.TryGetTerminal(pin.name,out var bound) || bound!=pin) ||
                SparkLevel59BlackStart.Gates.Any(id=>!bindings.IsComponentIdDeclared(id)))
                throw new InvalidOperationException("Level 59 wiring, motion or binding references are incomplete.");
            Debug.Log("[LEVEL 59] Validated 66 distinct terminals, 42 player wires and 6 evaluator gates.",root);
        }

        private static void Register()
        {
            var scene = SceneManager.GetSceneByPath(ScenePath);
            bool opened = !scene.isLoaded;
            if (opened) scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
            bool dirty = scene.isDirty;
            var objects = scene.GetRootGameObjects();
            T Find<T>() where T : Component => objects.SelectMany(item => item.GetComponentsInChildren<T>(true)).Single();
            var level = AssetDatabase.LoadAssetAtPath<SparkLevelDefinition>(Folder + "Level_59.asset");
            var drawing = AssetDatabase.LoadAssetAtPath<SparkLevelDrawingAsset>(Folder + "Level_59_Drawing.asset");
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
            else if (dirty) Debug.Log("MainMenu had unsaved edits. Save the scene to persist Level 59's campaign and teaching entries.");
        }

        private static void Append(Object target, string field, Object value)
        {
            if (value == null) throw new InvalidOperationException("Missing Level 59 asset for " + field);
            var data = new SerializedObject(target);
            var list = data.FindProperty(field);
            for (int i = 0; i < list.arraySize; i++)
                if (list.GetArrayElementAtIndex(i).objectReferenceValue == value) return;
            list.arraySize++;
            list.GetArrayElementAtIndex(list.arraySize - 1).objectReferenceValue = value;
            data.ApplyModifiedProperties();
        }

        [MenuItem("Tools/Project Spark/Level 59/Fit Breadboard Assembly")]
        public static void Fit()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Exit Play Mode before fitting Level 59.");
            var models = Models.Select((name, index) =>
            {
                string path = Folder + $"Models/L59_{index:00}_{name}_Fitted.fbx";
                return AssetDatabase.LoadAssetAtPath<GameObject>(path) ??
                    throw new InvalidOperationException("Missing fitted kit module " + path);
            }).ToArray();
            if (AssetDatabase.LoadAssetAtPath<GameObject>(Prefab) == null)
                throw new InvalidOperationException("The Level 59 prefab is missing.");

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
                    throw new InvalidOperationException("Unity could not save the Level 59 layout.");
                AssetDatabase.SaveAssets();
                Debug.Log("Level 59 fitted to the MainMenu breadboard with 66 seated terminals.");
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
            Vector3 right = Vector3.ProjectOnPlane(grid.GridOrigin.right, up).normalized * .059f;
            Vector3 forward = Vector3.ProjectOnPlane(grid.GridOrigin.forward, up).normalized * .059f;
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
            var bench = root.transform.Find("Black Start Workbench");
            var bindings = root.GetComponent<SparkLevelSceneBindings>();
            var loop = root.GetComponent<SparkLevel59BlackStart>();
            if (bench == null || bindings == null || loop == null)
                throw new InvalidOperationException("Level 59 is missing its workbench, controls, or scene bindings.");

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

            foreach (string name in new[] {"Fitted Level 59 Models", "Breadboard Labels", "Motion pivots"})
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
            var assembly = Child(bench, "Fitted Level 59 Models", Vector3.up * floor);
            foreach (var model in models)
            {
                var instance = Object.Instantiate(model, assembly, false).transform;
                var markers = instance.GetComponentsInChildren<Transform>(true)
                    .Where(item => item.name.StartsWith("L58_Board",StringComparison.Ordinal))
                    .ToDictionary(item => item.name);
                Transform Marker(string name) => markers["L58_Board" + name];
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
                var oldControls = root.transform.Find("Level 59 Controls");
                if (oldControls != null) Object.DestroyImmediate(oldControls.gameObject);
                SparkLevel59Builder.BuildControls(root.transform,loop,font);
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
            loop.rig.handles=new[]{"MainGridBreaker","GeneratorBreaker","EssentialLoadBreakerA","EssentialLoadBreakerB","EssentialLoadBreakerC","NonessentialLoadBreakerA","NonessentialLoadBreakerB"}.Select(module=>Pivot(module,"ANIM_BREAKER_HANDLE")).ToArray();
            loop.rig.rotor=Pivot("BackupGenerator","ANIM_GENERATOR_ROTOR");
            loop.rig.starter=Pivot("GeneratorStarter","ANIM_STARTER_PINION");
            loop.rig.fan=Pivot("CriticalCoolingLoad","ANIM_FAN_SPIN");
            loop.rig.motor=Pivot("IndustrialMotorLoad","ANIM_MOTOR_SPIN");
            loop.rig.monitor=Pivot("HolographicRestorationMonitor","ANIM_MONITOR_TILT");
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
            string path = $"{Materials}/L59 {key}.mat";
            var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing != null && key is not ("glass" or "acrylic") &&
                ColorUtility.TryParseHtmlString("#" + (Palette.TryGetValue(key, out var savedTint) ? savedTint : "9FB3C8"), out var savedColor) &&
                existing.GetColor("_BaseColor") == savedColor) return existing;
            ColorUtility.TryParseHtmlString("#" + (Palette.TryGetValue(key, out var tint) ? tint : "9FB3C8"), out var color);
            var material = existing != null ? existing : new Material(shader) { name = "L59 " + key };
            bool metal = key is "chrome" or "titanium" or "steel" or "gold" or "copper" or "aluminum";
            material.SetFloat("_Metallic", metal ? .59f : .02f);
            material.SetFloat("_Smoothness", metal ? .64f : key == "glass" ? .12f : .35f);
            if (key is "green" or "cyan" or "red" or "blue" or "orange" or "amber" or "mint" or "warm" or "ice")
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", color * .59f);
            }
            if (key is "glass" or "glass_body" or "glass_soft" or "acrylic" or "glassblue" or "film" or "Glass" or "guardglass")
            {
                color.a = key=="film" ? .59f : .10f;
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


