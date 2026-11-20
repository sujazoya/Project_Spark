using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ProjectSpark.AutoLevelGenerator;
using ProjectSpark.Gameplay;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace ProjectSpark.EditorTools
{
    [InitializeOnLoad]
    internal static class SparkLevel108Builder
    {
        internal const string Folder = "Assets/My_Assets/_Project_Spark/Prefabs/Levels/Level_108_Layout";
        internal const string Prefab = Folder + "/Level_108_Layout.prefab";
        internal const string Definition = Folder + "/Level_108.asset";
        internal const string Scene = "Assets/My_Assets/_Project_Spark/Scenes/MainMenu/MainMenu.unity";
        private const string Data = Folder + "/Level108/Data";
        private const string Report = "Logs/Level108Builder.txt";
        private const string TrainingRoot = "DFX-16 training circuit";
        private const string Catalog = "Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Levels/LevelLayoutCatalog/SparkLevelLayoutCatalog.asset";

        static SparkLevel108Builder() => EditorApplication.update += Poll;

        private static void Poll()
        {
            const string request = "Logs/Level108Builder.request";
            if (!File.Exists(request) || EditorApplication.isPlayingOrWillChangePlaymode ||
                EditorApplication.isCompiling || EditorApplication.isUpdating) return;
            File.Delete(request);
            try { Build(); }
            catch (Exception error) { File.WriteAllText(Report, error.ToString()); Debug.LogException(error); }
        }

        [MenuItem("Tools/Project Spark/Levels/108/Install DFX-16 Training Level")]
        internal static void Build()
        {
            var source = AssetDatabase.LoadAssetAtPath<SparkAutoLevelBlueprint>(Folder + "/L108_SourceBlueprint.asset");
            if (!source || source.LevelId != 108 || source.InstallableCount != 118 || source.Routes.Count != 156 ||
                source.Vias.Count != 50 || source.CopperLayers != 10 || source.Objectives.Count != 6)
                throw new InvalidOperationException("Missing original DFX-16 art blueprint or manifest records.");
            BuildDefinition(source);
            var root = PrefabUtility.LoadPrefabContents(Prefab);
            try
            {
                // The imported generator scaffold included missing placement scripts on these exact objects.
                // The training prefab replaces their behavior and keeps the source blueprint as art reference.
                GameObjectUtility.RemoveMonoBehavioursWithMissingScript(root);
                if (root.TryGetComponent<SparkAutoLevelRuntime>(out var scaffold)) Object.DestroyImmediate(scaffold);
                var old = root.GetComponent<SparkLevel108Process>();
                if (old) Object.DestroyImmediate(old);
                var process = root.AddComponent<SparkLevel108Process>();
                process.layout = root.GetComponent<SparkLevelLayout>() ?? root.AddComponent<SparkLevelLayout>();
                process.bindings = root.GetComponent<SparkLevelSceneBindings>() ?? root.AddComponent<SparkLevelSceneBindings>();
                process.fixture = root.GetComponent<SparkAutoL108WorkshopFit>().ModelScaleRoot;
                process.modelSpace = process.fixture.Find("L108_NativeFBXModelSpace");
                var slots = process.modelSpace.Find("L108_ComponentSlots_118_UNINSTALLED");
                var retired = process.modelSpace.Find(TrainingRoot);
                if (retired) Object.DestroyImmediate(retired.gameObject);
                var circuit = Child(process.modelSpace, TrainingRoot);
                var owner = Child(circuit, "Training pads").gameObject.AddComponent<SparkElectronicObject>();
                var materials = AssetDatabase.FindAssets("t:Material", new[] {Folder + "/URP_PBR_Materials"})
                    .Select(guid => AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guid)))
                    .ToDictionary(mat => mat.name.Replace("_AutoPBR", ""), StringComparer.OrdinalIgnoreCase);
                ModelParts(process, source, slots, materials);
                SparkTerminal Pin(string id, Vector3 position = default, bool interactive = false)
                {
                    var node = Child(circuit, id, position).gameObject;
                    if (interactive)
                    {
                        node.layer = LayerMask.NameToLayer("Pin");
                        node.AddComponent<SphereCollider>().radius = .0018f;
                    }
                    var terminal = node.AddComponent<SparkTerminal>();
                    Set(terminal, ("owner", owner), ("maxConnections", 12), ("providesOutput", true), ("acceptsInput", true));
                    return terminal;
                }
                T Device<T>(string name) where T : Component => Child(circuit, name).gameObject.AddComponent<T>();
                SparkSwitch Switch(string name, SparkTerminal a, SparkTerminal b, bool closed = false)
                {
                    var contact = Device<SparkSwitch>(name);
                    Set(contact, ("inputTerminal", a), ("outputTerminal", b), ("closedResistance", .001f),
                        ("state", closed ? SparkSwitchState.Closed : SparkSwitchState.Open));
                    return contact;
                }
                SparkResistor Load(string name, SparkTerminal a, SparkTerminal b, float resistance)
                {
                    var load = Device<SparkResistor>(name);
                    Set(load, ("terminalA", a), ("terminalB", b), ("resistance", resistance), ("maximumPowerWatts", 12f));
                    // These are bench model loads. The three diagnosed failures are controlled by their native contacts.
                    Set(load.GetComponent<SparkResistorBurn>(), ("ratedWatts", 12f),
                        ("thermalCapacityJPerC", .8f), ("coolingWattsPerC", .35f));
                    return load;
                }
                SparkPowerSupply Supply(string name, SparkTerminal a, SparkTerminal b, float voltage)
                {
                    var device = Device<SparkPowerSupply>(name);
                    Set(device, ("positiveTerminal", a), ("negativeTerminal", b), ("outputVoltage", voltage),
                        ("currentLimit", 2f), ("state", SparkPowerSupplyState.Active));
                    a.transform.SetParent(device.transform, true);
                    Set(a, ("owner", device));
                    return device;
                }

                process.input = Pin("SUPPLY_POS");
                process.ground = Pin("SUPPLY_RET");
                process.source = Supply("TRAINING_INPUT", process.input, process.ground, 12f);
                process.ground.transform.SetParent(process.source.transform, true);
                Set(process.ground, ("owner", process.source));
                Load("Bench reference", process.input, process.ground, 10000f);
                var routes = new List<SparkLevel108Process.Route>();
                var witnesses = new List<SparkTerminal>();
                var design = new List<string> {"task,subsystem,training_net,layer,from,to,a_x_local,a_z_local,b_x_local,b_z_local,via,exit_layer"};
                int index = 0, viaIndex = 0, groupIndex = 0;
                foreach (var group in source.Routes.GroupBy(route => route.Subsystem))
                {
                    int groupRoute = 0;
                    var previous = process.input;
                    foreach (var item in group)
                    {
                        float x = -.127f + groupRoute % 12 * .0224f;
                        float z = .083f - (groupIndex * 2 + groupRoute / 12) * .0128f;
                        var start = new Vector3(x, .012f, z);
                        var end = start + new Vector3(.0125f, 0, 0);
                        var a = Pin(item.Id + ".A", start, true);
                        var b = Pin(item.Id + ".B", end, true);
                        var route = new SparkLevel108Process.Route
                        {
                            id = item.Id, group = group.Key, layer = (index * 3 + groupIndex) % 10 + 1,
                            from = a, to = b, copper = Copper(circuit, materials["COPPER"], item.Id, start, end)
                        };
                        var linked = Switch(item.Id + " component contact", previous, a, index >= process.parts.Length);
                        a.transform.SetParent(linked.transform, true);
                        if (index < process.parts.Length) process.parts[index].solder = linked;
                        previous = b;
                        int viasInGroup = 7 + (groupIndex == 0 ? 1 : 0);
                        if (groupRoute < viasInGroup)
                        {
                            route.viaId = source.Vias[viaIndex++].Id;
                            route.exitLayer = (route.layer + 3) % 10 + 1;
                            previous = Pin(route.viaId + ".L" + route.exitLayer, end);
                            route.via = Switch(route.viaId + " training barrel", b, previous);
                            b.transform.SetParent(route.via.transform, true);
                            previous.transform.SetParent(route.via.transform, true);
                        }
                        routes.Add(route);
                        design.Add(Row(route, start, end));
                        index++;
                        groupRoute++;
                    }
                    Load(group.Key + " continuity witness", previous, process.ground, 1000f);
                    witnesses.Add(previous);
                    groupIndex++;
                }
                if (routes.Count != 156 || viaIndex != 50 || routes.Select(route => route.layer).Distinct().Count() != 10)
                    throw new InvalidOperationException("Incomplete DFX-16 educational routing specification.");
                process.routes = routes.ToArray();
                process.witnesses = witnesses.ToArray();

                process.shuntFrom = Pin("STANDBY_SHUNT_IN");
                process.shuntTo = Pin("STANDBY_SHUNT_OUT");
                process.benchPower = Switch("Bench disconnect", process.input, process.shuntFrom);
                // Unwired nodes enter the native solver graph through their carrying components.
                process.shuntFrom.transform.SetParent(process.benchPower.transform, true);
                var shunt = Load("One ohm current shunt", process.shuntFrom, process.shuntTo, 1f);
                process.shuntTo.transform.SetParent(shunt.transform, true);
                Load("Standby load", process.shuntTo, process.ground, 470f);
                var leak = Pin("CAP1_LEAK");
                process.leakContact = Switch("CAP1 isolation switch", process.shuntTo, leak);
                leak.transform.SetParent(process.leakContact.transform, true);
                Load("CAP1 leakage model", leak, process.ground, 19f);
                process.memory = Pin("DDR_TRAINING_CONTACT");
                process.memoryContact = Switch("Thermal U3 contact", process.shuntTo, process.memory);
                process.memory.transform.SetParent(process.memoryContact.transform, true);
                Load("DDR operation witness", process.memory, process.ground, 1000f);
                process.clock = Pin("PCIE_CLOCK_CONTACT");
                process.clockContact = Switch("PCIe intermittent clock contact", process.shuntTo, process.clock);
                process.clock.transform.SetParent(process.clockContact.transform, true);
                Load("PCIe endpoint witness", process.clock, process.ground, 1000f);

                int Part(string id) => Array.FindIndex(process.parts, part => part.footprint.name == id);
                int u16 = Part("117_L108_U16_DiagnosticMonitor"), u17 = Part("118_L108_U17_IndependentSafety");
                process.repairParts = new[] { Part("047_L108_CAP1"), Part("003_L108_U3"), Part("014_L108_U9_PCIeClockBuffer") };
                if (u16 < 0 || u17 < 0 || process.repairParts.Any(part => part < 0))
                    throw new InvalidOperationException("The source board is missing diagnostic and repair footprints.");
                Vector3 sourcePad = process.parts[u16].footprint.localPosition + new Vector3(.005f, .016f, -.008f);
                Vector3 sinkPad = process.parts[u17].footprint.localPosition + new Vector3(-.005f, .016f, -.008f);
                var pulseSupply = Pin("Heartbeat exciter");
                Supply("U16 signal supply", pulseSupply, process.ground, 3.3f);
                var output = Pin("U16.TRAINING_READY_OUT", sourcePad, true);
                process.receiver = Pin("U17.TRAINING_READY_IN", sinkPad, true);
                process.heartbeat = Switch("U16 diagnostic heartbeat", pulseSupply, output);
                output.transform.SetParent(process.heartbeat.transform, true);
                var pullDown = Load("U17 receiver bias", process.receiver, process.ground, 10000f);
                process.receiver.transform.SetParent(pullDown.transform, true);
                process.jumper = new SparkLevel108Process.Route
                {
                    id = "W157", group = "INDEPENDENT SAFETY", layer = 1, from = output, to = process.receiver,
                    copper = Copper(circuit, materials["GOLD"], "W157", sourcePad, sinkPad)
                };
                process.protectedFeed = Pin("PROTECTED_WORKLOAD");
                process.workload = Switch("U17 independent workload interlock", process.shuntTo, process.protectedFeed);
                process.protectedFeed.transform.SetParent(process.workload.transform, true);
                Load("Protected output witness", process.protectedFeed, process.ground, 1000f);
                design.Add(Row(process.jumper, sourcePad, sinkPad));

                process.placementMarker = Marker(circuit, "Assembly marker", materials["CYAN"]);
                process.fromMarker = Marker(circuit, "Source pad", materials["GOLD"]);
                process.toMarker = Marker(circuit, "Target pad", materials["CYAN"]);
                Bind(process);
                var level = LoadDefinition();
                Set(process.layout, ("levelDefinition", level), ("sceneBindings", process.bindings));
                var saved = PrefabUtility.SaveAsPrefabAsset(root, Prefab);
                if (!saved) throw new InvalidOperationException("Unity could not save the DFX-16 prefab.");
                var layout = saved.GetComponent<SparkLevelLayout>();
                level = LoadDefinition();
                if (!layout || layout.LevelDefinition != level)
                    throw new InvalidOperationException("The saved DFX-16 prefab lost its definition binding.");
                Register(level, layout);
                File.WriteAllLines(Data + "/DFX16_EDU_1_Training_Routes.csv", design);
                AssetDatabase.SaveAssets();
                AssetDatabase.ImportAsset(Data + "/DFX16_EDU_1_Training_Routes.csv");
                File.WriteAllText(Report, "BUILT DFX16-EDU-1. Original fitted workshop art retained. 118 native solder contacts, 156 training routes, 50 via barrels and native W157. MainMenu campaign and layout catalog registered.\n");
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }

        private static string Row(SparkLevel108Process.Route route, Vector3 start, Vector3 end) =>
            FormattableString.Invariant($"{route.id},{route.group},DFX16_EDU_{route.group.Replace(' ', '_')},{route.layer},{route.from.name},{route.to.name},{start.x:F5},{start.z:F5},{end.x:F5},{end.z:F5},{route.viaId},{route.exitLayer}");

        private static void ModelParts(SparkLevel108Process process, SparkAutoLevelBlueprint source, Transform slots, Dictionary<string, Material> materials)
        {
            var list = new List<SparkLevel108Process.Part>();
            foreach (var part in source.Parts.Where(item => item.Installable))
            {
                var slot = slots.Find(part.Id);
                var asset = AssetDatabase.LoadAssetAtPath<GameObject>(Folder + "/Level108/Models_FBX/" + part.Id + ".fbx");
                if (!slot || !asset) throw new InvalidOperationException("Missing DFX-16 footprint or model: " + part.Id);
                GameObjectUtility.RemoveMonoBehavioursWithMissingScript(slot.gameObject);
                var old = slot.Find("Installed component");
                if (old) Object.DestroyImmediate(old.gameObject);
                var model = (GameObject)PrefabUtility.InstantiatePrefab(asset, slot);
                model.name = "Installed component";
                model.transform.localPosition = Vector3.zero;
                model.transform.localRotation = Quaternion.identity;
                foreach (var renderer in model.GetComponentsInChildren<Renderer>(true))
                    renderer.sharedMaterials = renderer.sharedMaterials.Select(material => ResolveMaterial(material, materials)).ToArray();
                model.SetActive(false);
                slot.gameObject.layer = LayerMask.NameToLayer("UI");
                var target = slot.GetComponent<SparkLevel108Footprint>() ?? slot.gameObject.AddComponent<SparkLevel108Footprint>();
                target.level = process;
                target.index = list.Count;
                list.Add(new SparkLevel108Process.Part
                {
                    label = part.Id.Substring("000_L108_".Length).Replace('_', ' '), footprint = slot, model = model
                });
            }
            process.parts = list.ToArray();
        }

        private static Material ResolveMaterial(Material imported, Dictionary<string, Material> materials)
        {
            if (!imported) return imported;
            string name = imported.name.ToUpperInvariant().Replace(" (MATERIAL)", "");
            if (materials.TryGetValue(name, out var authored)) return authored;
            foreach (var item in materials)
                if (name.EndsWith("_" + item.Key, StringComparison.OrdinalIgnoreCase)) return item.Value;
            return imported;
        }

        private static SparkLevelDefinition LoadDefinition()
        {
            var definition = AssetDatabase.LoadAssetAtPath<SparkLevelDefinition>(Definition);
            if (!definition) throw new InvalidOperationException("Could not load the persisted Level 108 definition.");
            return definition;
        }

        private static void BuildDefinition(SparkAutoLevelBlueprint source)
        {
            var level = AssetDatabase.LoadAssetAtPath<SparkLevelDefinition>(Definition);
            bool exists = level;
            if (!exists) level = ScriptableObject.CreateInstance<SparkLevelDefinition>();
            Set(level, ("levelId", "level_108"), ("displayName", "Ghost in the Circuit"),
                ("description", "Assemble the DFX-16 training board, isolate three diagnostic faults, and prove the independent W157 safety recovery."),
                ("completionMode", 0), ("requiredTargetCount", 6), ("requireClosedReturn", false), ("requireMinimumVoltage", false),
                ("rejectShortCircuit", true), ("rejectTargetShort", true), ("unlockedByDefault", true), ("autoAdvanceOnCompletion", false));
            var data = new SerializedObject(level);
            var supplies = data.FindProperty("powerSources");
            supplies.arraySize = 1;
            var item = supplies.GetArrayElementAtIndex(0);
            item.FindPropertyRelative("sourceName").stringValue = "TRAINING_INPUT";
            item.FindPropertyRelative("positiveTerminalId").stringValue = "SUPPLY_POS";
            item.FindPropertyRelative("negativeTerminalId").stringValue = "SUPPLY_RET";
            item.FindPropertyRelative("nominalVoltage").floatValue = 12f;
            var targets = data.FindProperty("targets");
            targets.arraySize = 6;
            for (int i = 0; i < targets.arraySize; i++)
            {
                var target = targets.GetArrayElementAtIndex(i);
                target.FindPropertyRelative("targetId").stringValue = $"l108_{i + 1}";
                target.FindPropertyRelative("displayName").stringValue = i == 1 ? "Complete 156 training routes and 50 vias" : source.Objectives[i].Title;
                target.FindPropertyRelative("description").stringValue = i == 1 ? "Verify continuity on the ten-layer DFX16-EDU-1 training circuit." : source.Objectives[i].Criterion;
                target.FindPropertyRelative("targetComponentId").stringValue = SparkLevel108Process.Gate(i);
                target.FindPropertyRelative("targetType").enumValueIndex = (int)SparkLevelTarget.TargetType.ComponentPowered;
                target.FindPropertyRelative("requireElectricalEnabled").boolValue = true;
            }
            data.ApplyModifiedPropertiesWithoutUndo();
            if (!exists) AssetDatabase.CreateAsset(level, Definition);
            AssetDatabase.SaveAssetIfDirty(level);
        }

        private static void Bind(SparkLevel108Process process)
        {
            var data = new SerializedObject(process.bindings);
            var terminals = data.FindProperty("terminals");
            var pins = new[] {process.input, process.ground, process.jumper.from, process.jumper.to};
            terminals.arraySize = pins.Length;
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
            var proofs = data.FindProperty("runtimeComponentIds");
            proofs.arraySize = 6;
            for (int i = 0; i < proofs.arraySize; i++) proofs.GetArrayElementAtIndex(i).stringValue = SparkLevel108Process.Gate(i);
            data.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void Register(SparkLevelDefinition level, SparkLevelLayout layout)
        {
            var catalog = new SerializedObject(AssetDatabase.LoadAssetAtPath<SparkLevelLayoutCatalog>(Catalog));
            var entries = catalog.FindProperty("entries");
            var entry = Enumerable.Range(0, entries.arraySize).Select(entries.GetArrayElementAtIndex)
                .FirstOrDefault(item => item.FindPropertyRelative("level").objectReferenceValue == level ||
                    item.FindPropertyRelative("prefab").objectReferenceValue == layout);
            if (entry == null) entry = entries.GetArrayElementAtIndex(entries.arraySize++);
            entry.FindPropertyRelative("level").objectReferenceValue = level;
            entry.FindPropertyRelative("prefab").objectReferenceValue = layout;
            catalog.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.SaveAssetIfDirty(catalog.targetObject);

            var scene = SceneManager.GetSceneByPath(Scene);
            bool opened = !scene.isLoaded;
            if (opened) scene = EditorSceneManager.OpenScene(Scene, OpenSceneMode.Additive);
            try
            {
                var manager = scene.GetRootGameObjects()
                    .SelectMany(root => root.GetComponentsInChildren<LevelGamePlayManager>(true)).Single();
                var campaign = new SerializedObject(manager);
                var levels = campaign.FindProperty("levels");
                if (Enumerable.Range(0, levels.arraySize).Any(i => levels.GetArrayElementAtIndex(i).objectReferenceValue == level)) return;
                bool registeredOnDisk = File.ReadAllText(Scene).Contains("guid: " + AssetDatabase.AssetPathToGUID(Definition));
                if (scene.isDirty && !registeredOnDisk)
                    throw new InvalidOperationException("Campaign registration needs the saved MainMenu scene. The fitted prefab and catalog entry are ready.");

                int index = levels.arraySize;
                for (int i = 0; i < levels.arraySize; i++)
                {
                    var current = levels.GetArrayElementAtIndex(i).objectReferenceValue as SparkLevelDefinition;
                    if (current && (current.LevelId == "level_109" || current.LevelId == "level_110")) { index = i; break; }
                }
                levels.InsertArrayElementAtIndex(index);
                levels.GetArrayElementAtIndex(index).objectReferenceValue = level;
                var active = campaign.FindProperty("activeLevelIndex");
                if (active.intValue >= index && active.intValue >= 0) active.intValue++;
                campaign.ApplyModifiedPropertiesWithoutUndo();
                if (registeredOnDisk) return;
                EditorSceneManager.MarkSceneDirty(scene);
                if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("Could not save the Level 108 campaign registration.");
            }
            finally { if (opened) EditorSceneManager.CloseScene(scene, true); }
        }

        private static Transform Child(Transform root, string name, Vector3 position = default)
        {
            var node = new GameObject(name).transform;
            node.SetParent(root, false);
            node.localPosition = position;
            return node;
        }

        private static Renderer Marker(Transform root, string name, Material material)
        {
            var node = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            node.name = name;
            node.transform.SetParent(root, false);
            node.transform.localScale = new Vector3(.005f, .00035f, .005f);
            Object.DestroyImmediate(node.GetComponent<Collider>());
            var renderer = node.GetComponent<Renderer>();
            renderer.sharedMaterial = material;
            renderer.enabled = false;
            return renderer;
        }

        private static LineRenderer Copper(Transform root, Material material, string name, Vector3 start, Vector3 end)
        {
            var line = Child(root, name + " copper").gameObject.AddComponent<LineRenderer>();
            line.useWorldSpace = false;
            line.sharedMaterial = material;
            line.widthMultiplier = .001f;
            line.positionCount = 2;
            line.SetPosition(0, start);
            line.SetPosition(1, end);
            line.enabled = false;
            return line;
        }

        private static void Set(Object target, params (string name, object value)[] values)
        {
            var data = new SerializedObject(target);
            foreach (var (name, value) in values)
            {
                var property = data.FindProperty(name) ?? throw new InvalidOperationException(target.GetType().Name + "." + name);
                switch (value)
                {
                    case Object reference: property.objectReferenceValue = reference; break;
                    case string text: property.stringValue = text; break;
                    case bool flag: property.boolValue = flag; break;
                    case float number: property.floatValue = number; break;
                    case Enum item: property.enumValueIndex = Convert.ToInt32(item); break;
                    case int number when property.propertyType == SerializedPropertyType.Enum: property.enumValueIndex = number; break;
                    case int number: property.intValue = number; break;
                    default: throw new InvalidOperationException("Unsupported field " + name);
                }
            }
            data.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
