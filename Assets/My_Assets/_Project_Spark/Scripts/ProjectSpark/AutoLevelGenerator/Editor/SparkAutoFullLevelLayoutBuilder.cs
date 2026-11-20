#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using ProjectSpark.AutoLevelGenerator;

namespace ProjectSpark.AutoLevelGenerator.Editor
{
    /// <summary>
    /// Generates the complete Level_110_Layout hierarchy in the style of prior Project Spark
    /// layouts, with real imported model art and source-backed route/via/objective records.
    /// Art and planning nodes are never substituted for approved electrical topology.
    /// </summary>
    public static class SparkAutoFullLevelLayoutBuilder
    {
        private const int TargetLevel = 110;
        private const string RecipeName = "Level110_PWR18.recipe.json";
        private const string PrefabName = "Level_110_Layout";
        private const string ReportName = "Level_110_Layout_Validation.txt";

        [MenuItem("Tools/Project Spark/Automatic Level Generator/Build FULL Level 110 Layout")]
        public static void BuildLevel110()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogError("[Spark FullLayout] Stop Play Mode before generating a level.");
                return;
            }
            var report = new SparkAutoLevelBuildReport();
            SparkAutoLevelBlueprint transient = null;
            try
            {
                SparkAutoLevelRecipe recipe = SparkAutoLevelSourceImporter.LoadRecipe(RecipeName);
                if (recipe.levelId != TargetLevel)
                    throw new InvalidOperationException("Selected recipe is not Level 110.");
                transient = SparkAutoLevelSourceImporter.Import(recipe, report);
                if (report.Errors > 0)
                {
                    Debug.LogError(report.Render());
                    EditorUtility.DisplayDialog("Level 110 sources incomplete",
                        "Level 110 manifests have errors. Import its full asset ZIP with the V2 generator first, then retry.\n\n" +
                        report.Render(), "OK");
                    return;
                }
                string output = recipe.generatedAssetsFolder.TrimEnd('/');
                SparkAutoLevelSceneBuilder.EnsureFolder(output);
                string prefabPath = output + "/" + PrefabName + ".prefab";
                string scenePath = output + "/" + PrefabName + ".unity";
                if ((AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) != null ||
                     AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath) != null) &&
                    !EditorUtility.DisplayDialog("Replace generated Level 110 layout?",
                        "The generated Level_110_Layout already exists. Rebuilding will overwrite that GENERATED prefab/scene. " +
                        "Save any hand edits separately first. Level 103/104 are never touched.",
                        "Replace generated files", "Cancel"))
                    return;

                var models = DiscoverModels(recipe, report);
                string barePath;
                if (!models.TryGetValue(recipe.bareBoardId, out barePath))
                {
                    report.Error("Bare PCB FBX was not imported: " + recipe.bareBoardId);
                    Debug.LogError(report.Render());
                    return;
                }
                int missingInstallables = 0;
                foreach (SparkAutoPart part in transient.Parts)
                    if (part.Installable && !models.ContainsKey(part.Id)) missingInstallables++;
                if (missingInstallables != 0)
                {
                    report.Error(missingInstallables + " of " + transient.InstallableCount +
                        " installable FBXs are missing or failed Unity import. No partial full layout will be saved.");
                    Debug.LogError(report.Render());
                    EditorUtility.DisplayDialog("Models missing",
                        "Import/convert all FBX models before building the full layout. Missing: " + missingInstallables,
                        "OK");
                    return;
                }
                // Save the blueprint used by both runtime and the new complete hierarchy.
                string blueprintPath = output + "/L110_Blueprint.asset";
                var persistent = AssetDatabase.LoadAssetAtPath<SparkAutoLevelBlueprint>(blueprintPath);
                if (persistent == null)
                {
                    persistent = transient;
                    transient = null;
                    AssetDatabase.CreateAsset(persistent, blueprintPath);
                }
                else
                {
                    EditorUtility.CopySerialized(transient, persistent);
                    EditorUtility.SetDirty(persistent);
                }
                foreach (SparkAutoPart part in persistent.Parts)
                {
                    string path;
                    if (models.TryGetValue(part.Id, out path)) part.AssetPath = path;
                }
                EditorUtility.SetDirty(persistent);
                AssetDatabase.SaveAssets();

                var materials = SparkAutoURPMaterialBuilder.Build(recipe, report);
                BuildPrefabAndScene(recipe, persistent, models, materials, prefabPath, scenePath, report);
                File.WriteAllText(SparkAutoLevelSourceImporter.ToAbsolute(output + "/" + ReportName),
                    report.Render() + "\nREQUIRED: Approved pin-level design + real gameplay authority adapter for certification.\n");
                AssetDatabase.Refresh();
                Debug.Log("[Spark FullLayout] Level 110 complete hierarchy generated:\n" + prefabPath + "\n" + report.Render());
                var built = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                if (built != null) { Selection.activeObject = built; EditorGUIUtility.PingObject(built); }
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                EditorUtility.DisplayDialog("Full Level Layout generation error", exception.Message, "OK");
            }
            finally
            {
                if (transient != null) UnityEngine.Object.DestroyImmediate(transient);
            }
        }

        private static void BuildPrefabAndScene(SparkAutoLevelRecipe recipe, SparkAutoLevelBlueprint blueprint,
            Dictionary<string, string> models, Dictionary<string, Material> materials,
            string prefabPath, string scenePath, SparkAutoLevelBuildReport report)
        {
            Scene previous = SceneManager.GetActiveScene();
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            GameObject root = null;
            try
            {
                SceneManager.SetActiveScene(scene);
                root = new GameObject(PrefabName);
                var marker = root.AddComponent<SparkAutoGeneratedMarker>();
                marker.Configure(TargetLevel, false);
                var levelRuntime = root.AddComponent<SparkAutoLevelRuntime>();
                levelRuntime.Configure(blueprint, false);
                Meta(root, "L110", "LEVEL_LAYOUT", "ART_AND_PLANNING_GENERATED",
                    "Complete hierarchy, pending approved electrical/netlist gameplay binding");

                Transform physical = Child(root.transform, "L110_BoardPhysicalRoot");
                Transform boardArt = Child(physical, "L110_BoardArtRoot");
                Transform slots = Child(physical, "L110_ComponentSlots");
                Transform pads = Child(physical, "L110_PhysicalPads_SCHEMATIC_REQUIRED");
                Transform topology = Child(physical, "L110_SolverTopology_SCHEMATIC_REQUIRED");
                Transform playerCopper = Child(physical, "L110_PlayerCopper");
                Transform viaSockets = Child(physical, "L110_ViaSockets_SCHEMATIC_REQUIRED");
                Transform copperLayers = Child(physical, "L110_CopperLayers_VisualReference");
                Transform cooling = Child(physical, "L110_ServiceableCooling");
                Transform repair = Child(physical, "L110_RepairTargets");
                Transform instrumentation = Child(physical, "L110_InstrumentationAnchors");
                Transform simulation = Child(physical, "L110_PowerIntegritySimulation_BindExistingSolver");
                Transform objectiveNodes = Child(physical, "L110_Objectives_BindLevelGamePlayManager");
                Transform safety = Child(physical, "L110_W173_IndependentSafety");
                Transform controls = Child(physical, "L110_PowerIntegrityControls_WorldSpaceUI_BindExisting");
                Transform reference = Child(root.transform, "L110_CompleteAssembledReference_NoProgress");
                Transform modularReference = Child(reference, "L110_Modular_130Parts_Reference");
                Transform prefabReference = Child(reference, "L110_FullAssembledFBX_Reference");
                Transform preview = Child(root.transform, "L110_InspectionLighting");

                // The board's permanent non-installable 3D geometry is visible at entry.
                int permanentModels = 0, serviceModels = 0, layerModels = 0;
                GameObject bare = null;
                foreach (SparkAutoPart part in blueprint.Parts)
                {
                    if (part.Installable) continue;
                    string modelPath;
                    if (!models.TryGetValue(part.Id, out modelPath))
                    {
                        report.Warn("Non-installable FBX missing: " + part.Id);
                        continue;
                    }
                    Transform target = boardArt;
                    if (IsCopperLayer(part)) { target = copperLayers; layerModels++; }
                    else if (IsCooling(part)) { target = cooling; serviceModels++; }
                    else permanentModels++;
                    GameObject placed = Place(modelPath, part, target, recipe, materials);
                    Meta(placed, part.Id, part.Type, "VISUAL_ART_NOT_ELECTRICAL", part.Notes, false, true);
                    if (string.Equals(part.Id, recipe.bareBoardId, StringComparison.OrdinalIgnoreCase)) bare = placed;
                }
                // Cooling and internal copper previews are inspection visuals. They can be enabled when relevant.
                cooling.gameObject.SetActive(false);
                copperLayers.gameObject.SetActive(false);
                if (bare == null) report.Error("Bare PCB art was not instantiated");

                // One gameplay slot with real selectable target + per-part visual reference for every installable part.
                // Actual materialized part meshes are separately positioned in the assembled showcase (disabled).
                int slotsCreated = 0, refParts = 0;
                foreach (SparkAutoPart part in blueprint.Parts)
                {
                    if (!part.Installable) continue;
                    Transform slot = Child(slots, part.Id);
                    SetPartTransform(slot, part, recipe);
                    var collider = slot.gameObject.AddComponent<BoxCollider>();
                    collider.size = TargetSize(part.Type);
                    collider.center = new Vector3(0, 0.001f, 0);
                    var target = slot.gameObject.AddComponent<SparkAutoPlacementTarget>();
                    string modelPath;
                    GameObject model = models.TryGetValue(part.Id, out modelPath)
                        ? AssetDatabase.LoadAssetAtPath<GameObject>(modelPath) : null;
                    target.Configure(part.Id, part.Type, levelRuntime, false, model);
                    Meta(slot.gameObject, part.Id, "COMPONENT_SLOT_" + part.Type, "UNINSTALLED",
                        "Reference art is separate; pin/footprint electrical verification required", true);
                    slotsCreated++;

                    if (model != null)
                    {
                        GameObject view = Place(modelPath, part, modularReference, recipe, materials);
                        view.name = part.Id;
                        Meta(view, part.Id, "ASSEMBLED_PART_" + part.Type, "REFERENCE_ONLY", part.Notes,
                            false, true);
                        refParts++;
                    }
                }
                // Unverified physical pins cannot legitimately be generated from just a center-of-package manifest.
                Meta(pads.gameObject, "L110_PADS", "PENDING_PIN_MAP", "SCHEMATIC_REQUIRED",
                    "No manufacturer pin IDs or net assignments supplied. Not electrical pads.", true);
                Meta(topology.gameObject, "L110_TOPOLOGY", "PENDING_GRAPH", "SCHEMATIC_REQUIRED",
                    "Routes must be mapped to approved component pins before any current can flow.", true);

                int routeCount = 0;
                foreach (SparkAutoRoute route in blueprint.Routes)
                {
                    Transform group = FindOrCreate(playerCopper, "L110_" + SafeName(route.Subsystem));
                    Transform entry = Child(group, route.Id);
                    bool approved = route.HasSchematicFields(blueprint.CopperLayers);
                    Meta(entry.gameObject, route.Id, "COPPER_ROUTE_TASK", approved ? "SOURCE_FIELDS_APPROVED" : "SCHEMATIC_REQUIRED",
                        "Net=" + route.NetId + "; source=" + route.SourceComponent + "." + route.SourcePad +
                        "; destination=" + route.DestinationComponent + "." + route.DestinationPad +
                        "; layer=" + route.Layer + "; sourceStatus=" + route.SourceStatus, !approved);
                    routeCount++;
                }
                int viaCount = 0;
                foreach (SparkAutoVia via in blueprint.Vias)
                {
                    Transform entry = Child(viaSockets, via.Id);
                    bool approved = via.HasSchematicFields(blueprint.CopperLayers);
                    Meta(entry.gameObject, via.Id, "VIA_TASK", approved ? "SOURCE_FIELDS_APPROVED" : "SCHEMATIC_REQUIRED",
                        "Net=" + via.NetId + "; span=" + via.FromLayer + " to " + via.ToLayer +
                        "; coord=" + via.Xmm + "," + via.Ymm + "; type=" + via.ViaType, !approved);
                    if (approved)
                    {
                        float x, y;
                        if (float.TryParse(via.Xmm, System.Globalization.NumberStyles.Float,
                            System.Globalization.CultureInfo.InvariantCulture, out x) &&
                            float.TryParse(via.Ymm, System.Globalization.NumberStyles.Float,
                            System.Globalization.CultureInfo.InvariantCulture, out y))
                            entry.localPosition = new Vector3(x, 0, y) * recipe.millimetersToUnityUnits;
                    }
                    viaCount++;
                }

                int faultCount = 0;
                foreach (SparkAutoFault fault in blueprint.Faults)
                {
                    Transform entry = Child(repair, fault.Id + "_" + SafeName(fault.Title));
                    Meta(entry.gameObject, fault.Id, "FAULT_REPAIR_TARGET", "EVIDENCE_REQUIRED",
                        "Mechanism=" + fault.Mechanism + "; evidence=" + fault.Evidence +
                        "; repair=" + fault.Repair, true);
                    faultCount++;
                }
                int objectiveCount = 0;
                foreach (SparkAutoObjective objective in blueprint.Objectives)
                {
                    Transform entry = Child(objectiveNodes, "OBJ_" + objective.Id + "_" + SafeName(objective.Title));
                    Meta(entry.gameObject, objective.Id, "OBJECTIVE", "NOT_STARTED",
                        objective.Criterion + " — Requires actual LevelGamePlayManager authority", true);
                    objectiveCount++;
                }

                AddSimulationNodes(simulation);
                AddVirtualToolAnchors(instrumentation);
                AddControlNodes(controls);
                Meta(safety.gameObject, blueprint.Safety.JumperId, "INDEPENDENT_SAFETY", "DISCONNECTED",
                    "Logical=" + blueprint.Safety.LogicalSource + " => " + blueprint.Safety.LogicalDestination +
                    "; physicalPins=" + blueprint.Safety.PhysicalPinStatus +
                    "; approval=" + blueprint.Safety.NetlistStatus, true);
                Transform socket = Child(safety, "W173_ProtectionLatch_RequiresSupervisor");
                Meta(socket.gameObject, "W173", "SAFETY_RESET", "NOT_AUTHORIZED",
                    "Disconnect/reconnect requires real graph edge, live health freshness, and explicit reset", true);

                // Put the actual one-piece complete assembled FBX under a DISABLED reference parent when available.
                string assembledPath;
                if (models.TryGetValue(recipe.assembledBoardModelName, out assembledPath))
                {
                    GameObject composed = (GameObject)PrefabUtility.InstantiatePrefab(
                        AssetDatabase.LoadAssetAtPath<GameObject>(assembledPath));
                    composed.name = "PWR18_Complete_Assembled_Board_FBX";
                    composed.transform.SetParent(prefabReference, false);
                    composed.transform.localScale = Vector3.one * recipe.visualScaleMultiplier;
                    SparkAutoURPMaterialBuilder.Apply(composed, materials);
                    Meta(composed, "L110_COMPLETE", "ASSEMBLED_FBX", "REFERENCE_ONLY",
                        "Complete board does NOT count toward 130 placements or electrical completion", false, true);
                }
                else
                {
                    // Fallback is a complete visual composition: bare board + structural/cooling models + parts.
                    int composedSupport = 0;
                    foreach (SparkAutoPart part in blueprint.Parts)
                    {
                        if (part.Installable || IsCopperLayer(part)) continue;
                        string path;
                        if (!models.TryGetValue(part.Id, out path)) continue;
                        GameObject placed = Place(path, part, modularReference, recipe, materials);
                        Meta(placed, part.Id, "ASSEMBLED_STRUCTURAL_ART", "REFERENCE_ONLY", part.Notes,
                            false, true);
                        composedSupport++;
                    }
                    report.Warn("Complete assembled FBX not found; using 130 modular part meshes and " +
                        composedSupport + " supporting models as a visual-only composition.");
                }
                // Avoid double geometry: use the one-piece FBX when available, otherwise modular composition.
                modularReference.gameObject.SetActive(!models.ContainsKey(recipe.assembledBoardModelName));
                prefabReference.gameObject.SetActive(models.ContainsKey(recipe.assembledBoardModelName));
                reference.gameObject.SetActive(false);

                var lightGo = Child(preview, "PCB_InspectionKeyLight").gameObject;
                var light = lightGo.AddComponent<Light>();
                light.type = LightType.Directional;
                light.intensity = 1.2f;
                light.shadows = LightShadows.Soft;
                lightGo.transform.localRotation = Quaternion.Euler(52, -35, 0);
                // No duplicate MainCamera tag; Project Spark's viewer can control this board.
                Meta(preview.gameObject, "L110", "LIGHTING", "PREVIEW", "Use project scene viewer and World-Space Canvas");

                if (slotsCreated != 130 || routeCount != 172 || viaCount != 56 ||
                    faultCount != 3 || objectiveCount != 6)
                {
                    report.Error("Source counts did not match expected 130/172/56/3/6. Got " +
                        slotsCreated + "/" + routeCount + "/" + viaCount + "/" + faultCount + "/" + objectiveCount);
                    throw new InvalidOperationException("Level 110 manifest counts unexpectedly changed.");
                }
                report.Info("Generated Level_110_Layout: " + slotsCreated + " component slots; " +
                    routeCount + " route task entries; " + viaCount + " via task entries; " +
                    faultCount + " fault entries; " + objectiveCount + " objectives");
                report.Info("Permanent art models=" + permanentModels + ", service art=" +
                    serviceModels + ", layer art=" + layerModels + ", installable reference models=" + refParts);
                report.Warn("No verified physical pin mapping; L110_PhysicalPads and L110_SolverTopology remain unbound. " +
                    "No certifiable routing or 6/6 completion is fabricated.");

                bool success;
                PrefabUtility.SaveAsPrefabAsset(root, prefabPath, out success);
                if (!success) throw new IOException("Failed to save: " + prefabPath);
                if (!EditorSceneManager.SaveScene(scene, scenePath))
                    throw new IOException("Failed to save scene: " + scenePath);
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
                if (previous.IsValid() && previous.isLoaded)
                    SceneManager.SetActiveScene(previous);
            }
        }

        private static Dictionary<string, string> DiscoverModels(SparkAutoLevelRecipe recipe, SparkAutoLevelBuildReport report)
        {
            var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            string[] guids;
            if (AssetDatabase.IsValidFolder(recipe.sourceAssetsFolder + "/Models_FBX"))
                guids = AssetDatabase.FindAssets("t:Model", new[] { recipe.sourceAssetsFolder + "/Models_FBX" });
            else
            {
                report.Warn("Expected Models_FBX folder moved; using project-wide exact-ID search.");
                guids = AssetDatabase.FindAssets("t:Model", new[] { "Assets" });
            }
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (!path.EndsWith(".fbx", StringComparison.OrdinalIgnoreCase)) continue;
                string name = Path.GetFileNameWithoutExtension(path);
                int under = name.IndexOf('_');
                int n;
                if (under > 0 && int.TryParse(name.Substring(0, under), out n))
                    name = name.Substring(under + 1);
                if (!name.StartsWith("L110_", StringComparison.OrdinalIgnoreCase) &&
                    !name.Equals(recipe.assembledBoardModelName, StringComparison.OrdinalIgnoreCase)) continue;
                if (map.ContainsKey(name))
                    report.Warn("Duplicate L110 asset ID: " + name + " (first chosen: " + map[name] + ")");
                else map.Add(name, path);
            }
            return map;
        }

        private static bool IsCooling(SparkAutoPart part)
        {
            string type = part.Type ?? string.Empty;
            return type.IndexOf("HEATSINK", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   type.IndexOf("THERMAL_PAD", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   type.IndexOf("HEATPIPE", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   type.IndexOf("SPREAD", StringComparison.OrdinalIgnoreCase) >= 0;
        }
        private static bool IsCopperLayer(SparkAutoPart part)
        {
            return part.Id.IndexOf("CopperLayer", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   part.Type.IndexOf("PLANE", StringComparison.OrdinalIgnoreCase) >= 0;
        }
        private static Transform FindOrCreate(Transform parent, string name)
        {
            Transform result = parent.Find(name);
            return result != null ? result : Child(parent, name);
        }
        private static Transform Child(Transform parent, string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            return go.transform;
        }
        private static void SetPartTransform(Transform t, SparkAutoPart p, SparkAutoLevelRecipe r)
        {
            t.localPosition = new Vector3(p.Xmm, p.Zmm, p.Ymm) * r.millimetersToUnityUnits;
            t.localRotation = Quaternion.Euler(0, -p.OrientationDegrees, 0);
        }
        private static Vector3 TargetSize(string type)
        {
            string kind = type ?? string.Empty;
            float xy = 0.006f;
            if (kind.IndexOf("CPU", StringComparison.OrdinalIgnoreCase) >= 0) xy = 0.035f;
            else if (kind.IndexOf("DDR", StringComparison.OrdinalIgnoreCase) >= 0) xy = 0.018f;
            else if (kind.IndexOf("INDUCTOR", StringComparison.OrdinalIgnoreCase) >= 0) xy = 0.010f;
            else if (kind.IndexOf("CONTROLLER", StringComparison.OrdinalIgnoreCase) >= 0) xy = 0.013f;
            else if (kind.IndexOf("IC", StringComparison.OrdinalIgnoreCase) >= 0) xy = 0.010f;
            else if (kind.IndexOf("CAPACITOR", StringComparison.OrdinalIgnoreCase) >= 0) xy = 0.004f;
            else if (kind.IndexOf("RESISTOR", StringComparison.OrdinalIgnoreCase) >= 0) xy = 0.003f;
            else if (kind.IndexOf("JUMPER", StringComparison.OrdinalIgnoreCase) >= 0) xy = 0.006f;
            return new Vector3(xy, 0.002f, xy);
        }
        private static GameObject Place(string path, SparkAutoPart part, Transform parent,
            SparkAutoLevelRecipe recipe, Dictionary<string, Material> materials)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) throw new IOException("Unity failed to import FBX: " + path);
            GameObject view = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            view.name = part.Id;
            view.transform.SetParent(parent, false);
            SetPartTransform(view.transform, part, recipe);
            view.transform.localScale = Vector3.one * recipe.visualScaleMultiplier;
            SparkAutoURPMaterialBuilder.Apply(view, materials);
            return view;
        }
        private static SparkAutoFullLayoutRecord Meta(GameObject go, string id, string type,
            string status, string detail, bool requiresSchematic = false, bool referenceOnly = false)
        {
            var record = go.GetComponent<SparkAutoFullLayoutRecord>();
            if (record == null) record = go.AddComponent<SparkAutoFullLayoutRecord>();
            record.SetDefinition(id, type, status, detail, requiresSchematic, referenceOnly);
            return record;
        }
        private static string SafeName(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return "Unspecified";
            return value.Replace('/', '_').Replace('\\', '_').Replace(':', '_').Replace(' ', '_');
        }
        private static void AddSimulationNodes(Transform parent)
        {
            string[] nodes = {"Input_12V_CurrentLimited", "Phase1_VRM", "Phase2_VRM", "Phase3_VRM",
                "Phase4_VRM", "VCORE_OutputFilter", "CPU_DynamicLoad", "PWM_VoltageFeedback",
                "PowerSequencing", "VoltageTransient", "CurrentShareMonitor", "EfficiencyLosses",
                "ThermalCoupling", "PowerGood", "ProtectionSupervisor", "EnduranceTelemetry"};
            foreach (string name in nodes)
                Meta(Child(parent, name).gameObject, name, "SIMULATION_BINDING", "UNBOUND",
                    "Connect to authoritative electrical and thermal simulator", true);
        }
        private static void AddVirtualToolAnchors(Transform parent)
        {
            string[] tools = {"Multimeter", "Oscilloscope", "PowerAnalyzer", "CurrentProbe", "ThermalScanner",
                "NetTracer", "LayerViewer", "SolderRepair", "ComponentReplace", "VRMCalibrator",
                "LoadTester", "QualificationRecorder"};
            foreach (string name in tools)
                Meta(Child(parent, name).gameObject, name, "VIRTUAL_TOOL_BINDING", "UNBOUND",
                    "Bind existing Project Spark World-Space Canvas instrument", false);
        }
        private static void AddControlNodes(Transform parent)
        {
            string[] controls = {"POWER_12V", "ENABLE_CPU_VCORE", "VRM_CALIBRATE", "SELECT_PHASE",
                "LOAD_IDLE", "LOAD_NOMINAL", "LOAD_PEAK", "LOAD_BURST", "CAPTURE_OSCILLOSCOPE",
                "MEASURE_RIPPLE", "COMPARE_PHASE_CURRENTS", "LAYER_VIEW", "ROUTE_COPPER",
                "PLATE_VIA", "DIAGNOSE_F1", "DIAGNOSE_F2", "DIAGNOSE_F3", "REPAIR",
                "RUN_ENDURANCE", "W173_DISCONNECT", "W173_RECONNECT", "RESET_SAFETY",
                "SAVE_BOARD", "RESUME_BOARD", "SHOW_OBJECTIVES", "SHOW_CERTIFICATION_REPORT"};
            foreach (string name in controls)
                Meta(Child(parent, name).gameObject, name, "WORLD_SPACE_UI_ACTION", "UNBOUND",
                    "Inspector-visible action target; connect to existing World-Space Canvas controller");
        }
    }
}
#endif
