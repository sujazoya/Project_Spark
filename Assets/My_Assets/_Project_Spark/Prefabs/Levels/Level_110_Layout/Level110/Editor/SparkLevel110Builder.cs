using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using ProjectSpark.AutoLevelGenerator;
using ProjectSpark.AutoLevelGenerator.Editor;
using ProjectSpark.Gameplay;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace ProjectSpark.EditorTools
{
    [InitializeOnLoad]
    internal static class SparkLevel110Builder
    {
        internal const string Folder = "Assets/My_Assets/_Project_Spark/Prefabs/Levels/Level_110_Layout";
        internal const string Kit = Folder + "/Level110";
        internal const string ScenePath = "Assets/My_Assets/_Project_Spark/Scenes/MainMenu/MainMenu.unity";
        internal const string DefinitionPath = Folder + "/Level_110.asset";
        internal const string PrefabPath = Folder + "/Level_110_Layout.prefab";
        internal const string Report = "Logs/Level110Builder.txt";

        static SparkLevel110Builder() => EditorApplication.update += Poll;

        private static void Poll()
        {
            const string request = "Logs/Level110Builder.request";
            if (!File.Exists(request) || EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling || EditorApplication.isUpdating) return;
            File.Delete(request);
            try { Build(); }
            catch (Exception e) { File.WriteAllText(Report, e.ToString()); Debug.LogException(e); }
        }

        [MenuItem("Tools/Project Spark/Levels/Create Level 110")]
        public static void Build()
        {
            var recipe = SparkAutoLevelSourceImporter.LoadRecipe("Level110_PWR18.recipe.json");
            recipe.sourceAssetsFolder = Kit;
            var sourceReport = new SparkAutoLevelBuildReport();
            var blueprint = SparkAutoLevelSourceImporter.Import(recipe, sourceReport);
            if (sourceReport.Errors != 0 || !blueprint || blueprint.Parts.Count != 168 || blueprint.InstallableCount != 130
                || blueprint.Routes.Count != 172 || blueprint.Vias.Count != 56 || blueprint.CopperLayers != 12)
                throw new InvalidOperationException("Level 110 source manifest is incomplete.\n" + sourceReport.Render());

            var definition = Define(blueprint);
            var preview = EditorSceneManager.NewPreviewScene();
            var root = new GameObject("Level_110_Layout");
            SceneManager.MoveGameObjectToScene(root, preview);
            try
            {
                var process = root.AddComponent<SparkLevel110Process>();
                process.layout = root.AddComponent<SparkLevelLayout>();
                process.bindings = root.AddComponent<SparkLevelSceneBindings>();
                Set(process.layout, ("levelDefinition", definition), ("sceneBindings", process.bindings));
                process.board = Child(root.transform, "PWR-18 Table Fixture", new Vector3(0, 1.3f, 0));
                process.board.localScale = Vector3.one * 24;
                var art = Child(process.board, "Modular source geometry", Vector3.zero);
                var footprints = Child(process.board, "Assembly footprints", Vector3.zero);
                var files = Directory.GetFiles(Kit + "/Models_GLB", "*.glb").OrderBy(path => path).ToArray();
                if (files.Length != 169) throw new InvalidOperationException("Expected 168 source modules and one complete visual reference.");
                const string meshFolder = Kit + "/PlayableMeshes";
                if (!AssetDatabase.IsValidFolder(meshFolder)) AssetDatabase.CreateFolder(Kit, "PlayableMeshes");

                var topology = Child(process.board, "Educational circuit", Vector3.zero);
                var owner = Child(topology, "Training pads", Vector3.zero).gameObject.AddComponent<SparkElectronicObject>();
                var parts = new List<SparkLevel110Process.Part>();
                var cooling = new List<GameObject>();
                var topLevelParts = new List<string>();
                AssetDatabase.StartAssetEditing();
                try
                {
                for (int i = 0; i < blueprint.Parts.Count; i++)
                {
                    var part = blueprint.Parts[i];
                    var directPath = files.Single(path => Path.GetFileNameWithoutExtension(path).EndsWith(part.Id, StringComparison.Ordinal));
                    var center = new Vector3(part.Xmm, part.Zmm, part.Ymm) * .001f;
                    var visual = SparkLevel110Art.Build(directPath, art, center, meshFolder, Folder + "/URP_PBR_Materials");
                    if (!part.Installable)
                    {
                        visual.SetActive(part.Type is "PCB" or "SOCKET");
                        if (part.Type is "CPU_HEATSINK" or "HEATPIPE" or "VRM_SPREAD" or "THERMAL_PAD"
                            or "CHIPSET_SHROUD" or "IO_SHROUD") cooling.Add(visual);
                        if (part.Type == "PCB")
                        {
                            var bounds = visual.GetComponent<MeshFilter>().sharedMesh.bounds;
                            if (Mathf.Abs(bounds.size.x - .310f) > .002f || Mathf.Abs(bounds.size.z - .230f) > .002f)
                                throw new InvalidOperationException("Bare PCB does not match 310 × 230 mm: " + bounds.size);
                        }
                        continue;
                    }
                    visual.SetActive(false);
                    string label = part.Id.Split('_')[1];
                    Bounds partBounds = visual.GetComponent<MeshFilter>().sharedMesh.bounds;
                    var footprint = Child(footprints, part.Id + " footprint", center + partBounds.center).gameObject;
                    int uiLayer = LayerMask.NameToLayer("UI");
                    if (uiLayer >= 0) footprint.layer = uiLayer;
                    footprint.AddComponent<BoxCollider>().size = Vector3.Max(partBounds.size, new Vector3(.0025f, .002f, .0025f));
                    var click = footprint.AddComponent<SparkLevel110Footprint>();
                    click.level = process;
                    click.index = parts.Count;
                    topLevelParts.Add(label);
                    parts.Add(new SparkLevel110Process.Part { id = part.Id, label = $"{label} {part.Type}", visual = visual });
                }

                string referencePath = files.Single(path => Path.GetFileName(path).Contains("Complete_Assembled_Board", StringComparison.OrdinalIgnoreCase));
                process.completedReference = SparkLevel110Art.Build(referencePath, art, Vector3.zero, meshFolder, Folder + "/URP_PBR_Materials");
                }
                finally { AssetDatabase.StopAssetEditing(); }
                process.completedReference.name = "Assembled board visual reference";
                process.completedReference.SetActive(false);
                process.parts = parts.ToArray();
                process.cooling = cooling.ToArray();
                process.faultParts = new[] { "C01", "R23", "R17" }.Select(id => topLevelParts.IndexOf(id)).ToArray();
                if (process.faultParts.Any(index => index < 0) || !topLevelParts.Contains("U18") || !topLevelParts.Contains("U19"))
                    throw new InvalidOperationException("Fault or W173 component reference is missing.");

                SparkTerminal Pad(string id, Vector3 position, bool interactive = false)
                {
                    var node = Child(topology, id, position).gameObject;
                    if (interactive)
                    {
                        var collider = node.AddComponent<SphereCollider>();
                        collider.radius = .0018f;
                        int layer = LayerMask.NameToLayer("Pin");
                        if (layer >= 0) node.layer = layer;
                    }
                    var result = node.AddComponent<SparkTerminal>();
                    Set(result, ("owner", owner), ("providesOutput", true), ("acceptsInput", true), ("maxConnections", 12));
                    return result;
                }
                T Device<T>(string name) where T : Component => Child(topology, name, Vector3.zero).gameObject.AddComponent<T>();
                SparkSwitch Contact(string name, SparkTerminal a, SparkTerminal b, bool closed)
                {
                    var contact = Device<SparkSwitch>(name);
                    Set(contact, ("inputTerminal", a), ("outputTerminal", b), ("state", closed ? SparkSwitchState.Closed : SparkSwitchState.Open),
                        ("closedResistance", .001f));
                    return contact;
                }
                SparkResistor Load(string name, SparkTerminal a, SparkTerminal b, float ohms)
                {
                    var load = Device<SparkResistor>(name);
                    Set(load, ("terminalA", a), ("terminalB", b), ("resistance", ohms), ("maximumPowerWatts", 5f));
                    return load;
                }
                SparkPowerSupply Supply(string name, SparkTerminal a, SparkTerminal b, float volts)
                {
                    var source = Device<SparkPowerSupply>(name);
                    Set(source, ("positiveTerminal", a), ("negativeTerminal", b), ("outputVoltage", volts),
                        ("currentLimit", 2f), ("state", SparkPowerSupplyState.Active));
                    return source;
                }

                process.ground = Pad("SUPPLY_RET", Vector3.zero);
                process.input = Pad("SUPPLY_POS", Vector3.zero);
                process.source = Supply("TRAINING_INPUT", process.input, process.ground, 12);
                process.input.transform.SetParent(process.source.transform, true);
                process.ground.transform.SetParent(process.source.transform, true);
                Set(process.input, ("owner", process.source));
                Set(process.ground, ("owner", process.source));
                Load("Bench reference load", process.input, process.ground, 10000);
                var copper = MaterialAt("COPPER");
                var designRows = new List<string> { "task,source_group,fictional_net,source_pad,destination_pad,layer,via,exit_layer,x1_mm,y1_mm,x2_mm,y2_mm" };
                var branches = new List<SparkLevel110Process.Branch>();
                var routes = new SparkLevel110Process.Route[blueprint.Routes.Count];
                int routeIndex = 0, partIndex = 0, viaIndex = 0;
                foreach (var group in blueprint.Routes.GroupBy(route => route.Subsystem))
                {
                    var previous = process.input;
                    int groupIndex = branches.Count;
                    int segmentIndex = 0;
                    foreach (var spec in group)
                    {
                        int layer = (routeIndex * 5 + groupIndex) % 12 + 1;
                        float x = -.139f + segmentIndex % 14 * .0205f;
                        float z = .096f - (groupIndex * 2 + segmentIndex / 14) * .0145f;
                        var a = Pad(spec.Id + ".A", new Vector3(x, .008f, z), true);
                        var b = Pad(spec.Id + ".B", new Vector3(x + .014f, .008f, z), true);
                        var route = new SparkLevel110Process.Route { id = spec.Id, group = spec.Subsystem, layer = layer, from = a, to = b };
                        route.copper = Copper(topology, copper, a, b);
                        if (partIndex < parts.Count)
                        {
                            var part = parts[partIndex++];
                            part.contact = Contact(part.id + ".TRAINING_SOLDER", previous, a, false);
                            a.transform.SetParent(part.contact.transform, true);
                        }
                        else
                        {
                            var feed = Contact(spec.Id + ".LOCAL_FEED", previous, a, true);
                            a.transform.SetParent(feed.transform, true);
                        }
                        previous = b;
                        // Eight verified spans per training domain. Layer labels model the synthetic board stack.
                        if (segmentIndex < 8)
                        {
                            var via = blueprint.Vias[viaIndex++];
                            route.viaId = via.Id;
                            route.exitLayer = (layer + 4) % 12 + 1;
                            previous = Pad(via.Id + ".L" + route.exitLayer, b.transform.localPosition);
                            route.via = Contact(via.Id + " plated barrel", b, previous, false);
                            b.transform.SetParent(route.via.transform, true);
                            previous.transform.SetParent(route.via.transform, true);
                        }
                        designRows.Add(FormattableString.Invariant(
                            $"{spec.Id},{spec.Subsystem},{spec.Subsystem}_TRAINING,{a.name},{b.name},{layer},{route.viaId},{route.exitLayer},{x * 1000:F2},{z * 1000:F2},{(x + .014f) * 1000:F2},{z * 1000:F2}"));
                        routes[routeIndex++] = route;
                        segmentIndex++;
                    }
                    Load(group.Key + " continuity sense", previous, process.ground, 1000);
                    branches.Add(new SparkLevel110Process.Branch { sense = previous, name = group.Key });
                }
                if (routeIndex != 172 || viaIndex != 56 || partIndex != 130 || !routes.Select(route => route.layer).Distinct().OrderBy(layer => layer).SequenceEqual(Enumerable.Range(1, 12)))
                    throw new InvalidOperationException("The training routing manifest is incomplete.");

                process.routes = routes;
                process.branches = branches.ToArray();
                var sender = Pad("U18.TRAINING_HEALTH_OUT", new Vector3(.112f, .008f, -.101f), true);
                process.healthReceiver = Pad("U19.TRAINING_HEALTH_IN", new Vector3(.144f, .008f, -.101f), true);
                process.healthTransmitter = Supply("Independent health transmitter", sender, process.ground, 3.3f);
                Set(process.healthTransmitter, ("state", SparkPowerSupplyState.Off));
                Set(sender, ("owner", process.healthTransmitter));
                sender.transform.SetParent(process.healthTransmitter.transform, true);
                var receiver = Load("Independent health receiver", process.healthReceiver, process.ground, 100000);
                process.healthReceiver.transform.SetParent(receiver.transform, true);
                process.jumper = new SparkLevel110Process.Route
                {
                    id = "W173", group = "TRUSTED HEALTH", layer = 1, from = sender, to = process.healthReceiver,
                    copper = Copper(topology, copper, sender, process.healthReceiver)
                };
                var protectedLoad = Pad("Protected workload feed", Vector3.zero);
                process.workloadInterlock = Contact("Independent workload interlock", process.input, protectedLoad, false);
                protectedLoad.transform.SetParent(process.workloadInterlock.transform, true);
                Load("Protected workload witness", protectedLoad, process.ground, 1000);
                designRows.Add("W173,INDEPENDENT_SAFETY,TRAINING_HEALTH,U18.TRAINING_HEALTH_OUT,U19.TRAINING_HEALTH_IN,1,,0,112,-101,144,-101");
                File.WriteAllLines(Kit + "/Data/PWR18_EDU_1_Training_Routes.csv", designRows);

                process.selectionMarker = Marker(process.board, "Footprint marker", MaterialAt("CYAN"));
                process.fromMarker = Marker(process.board, "Source pad marker", MaterialAt("GOLD"));
                process.toMarker = Marker(process.board, "Destination pad marker", MaterialAt("CYAN"));
                Bind(process);
                var saved = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
                if (!saved) throw new InvalidOperationException("Could not save the Level 110 layout prefab.");
                AssetDatabase.SaveAssets();
                Register(definition, saved.GetComponent<SparkLevelLayout>());
                File.WriteAllText(Report, "BUILT PWR18-EDU-1. 168 GLB modules plus independent reference, 130 parts, 172 training routes, 56 vias, W173 and campaign layout.\n");
            }
            finally
            {
                EditorSceneManager.ClosePreviewScene(preview);
                Object.DestroyImmediate(blueprint);
            }
        }

        private static Material MaterialAt(string id) =>
            AssetDatabase.LoadAssetAtPath<Material>($"{Folder}/URP_PBR_Materials/{id}_AutoPBR.mat");

        private static Renderer Marker(Transform parent, string name, Material material)
        {
            var node = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            node.name = name;
            node.transform.SetParent(parent, false);
            node.transform.localScale = new Vector3(.005f, .0003f, .005f);
            Object.DestroyImmediate(node.GetComponent<Collider>());
            var renderer = node.GetComponent<Renderer>();
            renderer.sharedMaterial = material;
            renderer.enabled = false;
            return renderer;
        }

        private static LineRenderer Copper(Transform parent, Material material, SparkTerminal a, SparkTerminal b)
        {
            var line = Child(parent, a.name + " copper", Vector3.zero).gameObject.AddComponent<LineRenderer>();
            line.useWorldSpace = false;
            line.sharedMaterial = material;
            line.widthMultiplier = .0012f;
            line.positionCount = 2;
            line.SetPosition(0, a.transform.localPosition);
            line.SetPosition(1, b.transform.localPosition);
            line.enabled = false;
            return line;
        }

        private static SparkLevelDefinition Define(SparkAutoLevelBlueprint blueprint)
        {
            var level = AssetDatabase.LoadAssetAtPath<SparkLevelDefinition>(DefinitionPath);
            bool existing = level;
            if (!level) level = ScriptableObject.CreateInstance<SparkLevelDefinition>();
            Set(level, ("levelId", "level_110"), ("displayName", "Voltage Dominion"),
                ("description", "Assemble PWR-18, commission a four-phase VRM, repair three power faults and recover W173 in the educational power lab."),
                ("completionMode", 0), ("requiredTargetCount", 6), ("requireClosedReturn", false), ("requireMinimumVoltage", false),
                ("rejectShortCircuit", true), ("rejectTargetShort", true), ("unlockedByDefault", true), ("autoAdvanceOnCompletion", false));
            var data = new SerializedObject(level);
            var sources = data.FindProperty("powerSources");
            sources.arraySize = 1;
            var source = sources.GetArrayElementAtIndex(0);
            source.FindPropertyRelative("sourceName").stringValue = "TRAINING_INPUT";
            source.FindPropertyRelative("positiveTerminalId").stringValue = "SUPPLY_POS";
            source.FindPropertyRelative("negativeTerminalId").stringValue = "SUPPLY_RET";
            source.FindPropertyRelative("nominalVoltage").floatValue = 12;
            var targets = data.FindProperty("targets");
            targets.arraySize = 6;
            for (int i = 0; i < 6; i++)
            {
                var item = targets.GetArrayElementAtIndex(i);
                item.FindPropertyRelative("targetId").stringValue = $"l110_{i + 1}";
                item.FindPropertyRelative("displayName").stringValue = blueprint.Objectives[i].Title;
                item.FindPropertyRelative("description").stringValue = blueprint.Objectives[i].Criterion;
                item.FindPropertyRelative("targetComponentId").stringValue = SparkLevel110Process.Gate(i);
                item.FindPropertyRelative("targetType").enumValueIndex = (int)SparkLevelTarget.TargetType.ComponentPowered;
                item.FindPropertyRelative("requireElectricalEnabled").boolValue = true;
            }
            data.ApplyModifiedPropertiesWithoutUndo();
            if (!existing) AssetDatabase.CreateAsset(level, DefinitionPath);
            return level;
        }

        private static void Bind(SparkLevel110Process process)
        {
            var data = new SerializedObject(process.bindings);
            var terminals = data.FindProperty("terminals");
            terminals.arraySize = 2;
            var pins = new[] { process.input, process.ground };
            for (int i = 0; i < pins.Length; i++)
            {
                var terminal = terminals.GetArrayElementAtIndex(i);
                terminal.FindPropertyRelative("id").stringValue = pins[i].name;
                terminal.FindPropertyRelative("terminal").objectReferenceValue = pins[i];
            }
            var components = data.FindProperty("components");
            components.arraySize = 1;
            components.GetArrayElementAtIndex(0).FindPropertyRelative("id").stringValue = "TRAINING_INPUT";
            components.GetArrayElementAtIndex(0).FindPropertyRelative("component").objectReferenceValue = process.source;
            var ids = data.FindProperty("runtimeComponentIds");
            ids.arraySize = 6;
            for (int i = 0; i < 6; i++) ids.GetArrayElementAtIndex(i).stringValue = SparkLevel110Process.Gate(i);
            data.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void Register(SparkLevelDefinition level, SparkLevelLayout layout)
        {
            var scene = SceneManager.GetSceneByPath(ScenePath);
            bool opened = !scene.isLoaded;
            if (opened) scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
            try
            {
                T Find<T>() where T : Component => scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<T>(true)).Single();
                var campaign = new SerializedObject(Find<LevelGamePlayManager>());
                var levels = campaign.FindProperty("levels");
                bool registered = Enumerable.Range(0, levels.arraySize).Any(index => levels.GetArrayElementAtIndex(index).objectReferenceValue == level);
                string definitionGuid = AssetDatabase.AssetPathToGUID(DefinitionPath);
                bool registeredOnDisk = File.ReadAllText(ScenePath).Contains("guid: " + definitionGuid);
                if (!registered && scene.isDirty && !registeredOnDisk)
                    throw new InvalidOperationException("MainMenu has unsaved edits. Level 110 assets are generated; save MainMenu before campaign registration.");
                var loader = new SerializedObject(Find<SparkLevelLayoutLoader>());
                var catalog = new SerializedObject(loader.FindProperty("catalog").objectReferenceValue);
                var entries = catalog.FindProperty("entries");
                var match = Enumerable.Range(0, entries.arraySize).Select(entries.GetArrayElementAtIndex)
                    .FirstOrDefault(item => item.FindPropertyRelative("level").objectReferenceValue == level);
                if (match == null) match = entries.GetArrayElementAtIndex(entries.arraySize++);
                match.FindPropertyRelative("level").objectReferenceValue = level;
                match.FindPropertyRelative("prefab").objectReferenceValue = layout;
                catalog.ApplyModifiedProperties();
                AssetDatabase.SaveAssetIfDirty(catalog.targetObject);
                if (registered || registeredOnDisk) return;
                int index = levels.arraySize++;
                levels.GetArrayElementAtIndex(index).objectReferenceValue = level;
                var activeIndex = campaign.FindProperty("activeLevelIndex");
                if (activeIndex.intValue == index && index > 0) activeIndex.intValue--;
                campaign.ApplyModifiedProperties();
                if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("Could not save the campaign registration.");
            }
            finally { if (opened) EditorSceneManager.CloseScene(scene, true); }
        }

        private static Transform Child(Transform parent, string name, Vector3 position)
        {
            var transform = new GameObject(name).transform;
            transform.SetParent(parent, false);
            transform.localPosition = position;
            return transform;
        }

        private static void Set(Object target, params (string name, object value)[] values)
        {
            var data = new SerializedObject(target);
            foreach (var entry in values)
            {
                var property = data.FindProperty(entry.name) ?? throw new InvalidOperationException($"Missing {target.GetType().Name}.{entry.name}");
                switch (entry.value)
                {
                    case Object reference: property.objectReferenceValue = reference; break;
                    case string text: property.stringValue = text; break;
                    case float number: property.floatValue = number; break;
                    case bool flag: property.boolValue = flag; break;
                    case Enum choice: property.enumValueIndex = Convert.ToInt32(choice); break;
                    case int index when property.propertyType == SerializedPropertyType.Enum: property.enumValueIndex = index; break;
                    case int number: property.intValue = number; break;
                    default: throw new InvalidOperationException($"Unsupported value for {entry.name}");
                }
            }
            data.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
