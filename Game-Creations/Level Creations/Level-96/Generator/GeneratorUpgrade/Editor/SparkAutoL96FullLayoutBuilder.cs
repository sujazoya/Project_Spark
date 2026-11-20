#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProjectSpark.AutoLevelGenerator.Editor
{
    /// <summary>
    /// Full source-art and gameplay-binding layout for Level 96 SMX-4 Sensor Matrix.
    /// Requires the installed Universal Auto Level Generator V2 runtime/editor classes.
    /// Prefers GLB-derived mesh-bearing OBJ fallback over unrenderable FBX.
    /// The imported OBJ is a VISUAL asset; no pin netlist or completed gameplay is inferred.
    /// </summary>
    public static class SparkAutoL96FullLayoutBuilder
    {
        private const int LevelId = 96;
        private const string RecipeFile = "Level96_SMX4.recipe.json";
        private const string LayoutName = "Level_96_Layout";
        private const float TableWidth = 15.79f;
        private const float TableDepth = 8.54f;
        private const float TargetWidth = 10.13f;
        private const float TargetDepth = 6.8742857f; // uniform: 10.13 * 95 / 140
        private const int PartTotal = 44, RouteTotal = 58, ViaTotal = 12;
        private const int FaultTotal = 3, ObjectiveTotal = 6;
        // Binding anchors only: existing Project Spark systems own the actual actions and state.
        private static readonly string[] VirtualTools = {
            "VirtualMultimeter", "VirtualOscilloscope", "LogicAnalyzer", "PCBNetTracer",
            "ThermalView", "FourChannelSensorScope", "ReferenceVoltageMeter",
            "ADC16BitInspector", "BridgeCalibration", "RTDCalibration", "PhotodiodeInspector",
            "HallCurrentInspector", "Microscope", "SolderDesolder", "ViaRepair",
            "CopperRouteEditor", "ComponentReplacement", "PCBLayerViewer", "EvidenceNotebook"
        };
        private static readonly string[] Controls = {
            "INSPECT", "ASSEMBLE", "ROUTE_F_CU", "ROUTE_B_CU", "PLACE_VIA",
            "VIEW_LAYERS", "MEASURE_VOLTAGE", "MEASURE_CONTINUITY",
            "POWER_5V", "MONITOR_3V3", "CHECK_2V500_REFERENCE",
            "ADC_ZERO_CALIBRATION", "ADC_SPAN_CALIBRATION", "TEST_RTD_CH0",
            "TEST_BRIDGE_CH1", "TEST_PHOTODIODE_CH2", "TEST_HALL_CH3",
            "START_128_SPS_ACQUISITION", "VIEW_BUFFER", "CHECK_SATURATION",
            "INJECT_F1_REFERENCE_DRIFT", "REPAIR_F1", "INJECT_F2_BRIDGE_VIA_OPEN",
            "REPAIR_F2", "INJECT_F3_OPTICAL_SATURATION", "REPAIR_F3",
            "W059_DISCONNECT", "W059_RECONNECT", "W059_EXPLICIT_RESET",
            "RUN_FRESH_FOUR_CHANNEL_TEST", "SHOW_OBJECTIVES", "SAVE_LEVEL", "RELOAD_LEVEL"
        };
        private static readonly string[] SensorAcquisitionSystems = {
            "Power_Protected5V", "DigitalRail_3V3", "PrecisionReference_U6_2V500",
            "CH0_PT1000_RTD_Excitation", "CH1_Wheatstone_Force_Bridge",
            "CH2_Photodiode_Transimpedance", "CH3_Hall_Current_Filter",
            "ADC_16Bit_4Channel", "SampleClock_128SPS_PerChannel",
            "Calibration_Offset_And_Gain", "SensorOutput_Quality_Status",
            "F1_ReferenceDrift", "F2_OpenBridgeVia", "F3_OpticalAmplifierClipping",
            "W059_IndependentReferenceSafety", "EvidenceRevisionAndPersistence",
            "ExistingLevelGamePlayManagerBinding_UNBOUND"
        };

        [MenuItem("Tools/Project Spark/Automatic Level Generator/96/1. Build FULL Level_96_Layout")]
        public static void BuildFullLevel96()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogError("[L96 Layout] Stop Play Mode before generating.");
                return;
            }
            SparkAutoLevelBlueprint transient = null;
            var report = new SparkAutoLevelBuildReport();
            try
            {
                SparkAutoLevelRecipe recipe = SparkAutoLevelSourceImporter.LoadRecipe(RecipeFile);
                if (recipe.levelId != LevelId) throw new InvalidOperationException("Level 96 recipe expected");
                transient = SparkAutoLevelSourceImporter.Import(recipe, report);
                if (report.Errors != 0)
                    throw new InvalidOperationException("Source validation failed:\n" + report.Render());
                if (transient.Parts.Count != 62 || transient.InstallableCount != PartTotal ||
                    transient.Routes.Count != RouteTotal || transient.Vias.Count != ViaTotal ||
                    transient.Faults.Count != FaultTotal || transient.Objectives.Count != ObjectiveTotal)
                    throw new InvalidOperationException("Level 96 source counts have changed. Refusing partial build.");

                var modelPaths = ResolveModels(recipe, transient, report);
                string bareAsset;
                if (!modelPaths.TryGetValue(recipe.bareBoardId, out bareAsset))
                    throw new FileNotFoundException("Missing renderable EMPTY SMX-4 PCB: " + recipe.bareBoardId);
                int missing = 0;
                foreach (SparkAutoPart part in transient.Parts)
                    if (part.Installable && !modelPaths.ContainsKey(part.Id)) missing++;
                if (missing > 0) throw new FileNotFoundException(missing + " installable FBX/OBJ models failed import. Build cancelled.");

                string output = recipe.generatedAssetsFolder.TrimEnd('/');
                SparkAutoLevelSceneBuilder.EnsureFolder(output);
                string prefabPath = output + "/" + LayoutName + ".prefab";
                string scenePath = output + "/" + LayoutName + ".unity";
                if ((AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) != null ||
                     AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath) != null) &&
                    !EditorUtility.DisplayDialog("Regenerate Level 96?",
                        "This will overwrite ONLY the generated Level_96_Layout prefab and scene. " +
                        "Keep hand edits in a separate prefab. Previous levels are untouched.",
                        "Overwrite generated Level 96", "Cancel")) return;

                string blueprintPath = output + "/L96_SourceBlueprint.asset";
                SparkAutoLevelBlueprint persistent = AssetDatabase.LoadAssetAtPath<SparkAutoLevelBlueprint>(blueprintPath);
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
                    if (modelPaths.TryGetValue(part.Id, out path)) part.AssetPath = path;
                }
                EditorUtility.SetDirty(persistent);
                AssetDatabase.SaveAssets();
                Dictionary<string, Material> materials = SparkAutoL96PBRMaterials.Build(recipe, report);
                BuildLayoutScene(recipe, persistent, modelPaths, materials, prefabPath, scenePath, report);
                File.WriteAllText(SparkAutoLevelSourceImporter.ToAbsolute(output + "/L96_Layout_Validation.txt"),
                    report.Render() + "\nGeometry centered and workshop footprint 10.13 x 6.874 units. " +
                    "Electrical pins/nets and gameplay service bindings still require authoritative design.\n");
                AssetDatabase.Refresh();
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                if (prefab != null) { Selection.activeObject = prefab; EditorGUIUtility.PingObject(prefab); }
                Debug.Log("[L96 Layout] Generated " + prefabPath + "\n" + report.Render());
            }
            catch (Exception e)
            {
                Debug.LogError("[L96 Layout] " + e);
                EditorUtility.DisplayDialog("Level 96 generation stopped", e.Message, "OK");
            }
            finally
            {
                if (transient != null) UnityEngine.Object.DestroyImmediate(transient);
            }
        }

        [MenuItem("Tools/Project Spark/Automatic Level Generator/96/2. Place Level 96 on SELECTED tabletop")]
        public static void PlaceOnSelectedTabletop()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            GameObject table = Selection.activeGameObject;
            if (table == null) { EditorUtility.DisplayDialog("Select the table", "Select your tabletop GameObject in the Hierarchy first.", "OK"); return; }
            string expected = "Assets/ProjectSpark/AutoGeneratedLevels/Level96/" + LayoutName + ".prefab";
            GameObject source = AssetDatabase.LoadAssetAtPath<GameObject>(expected);
            if (source == null) { EditorUtility.DisplayDialog("Missing layout", "Build FULL Level 96 layout first.", "OK"); return; }
            // An artist-authored tabletop may be a plain Transform with no mesh or Collider.
            // Support that as an explicitly confirmed placement anchor. Its Transform.scale
            // is NOT the tabletop's measured size; scaling the board by it would be wrong.
            Bounds top;
            bool hasBounds = TryTableBounds(table, out top);
            Vector3 placement;
            float actualTableWidth;
            float actualTableDepth;
            if (hasBounds)
            {
                actualTableWidth = top.size.x;
                actualTableDepth = top.size.z;
                if (actualTableWidth + 0.001f < TargetWidth || actualTableDepth + 0.001f < TargetDepth)
                {
                    EditorUtility.DisplayDialog("Table too small",
                        "Selected object's bounds are " + actualTableWidth.ToString("F2") +
                        " x " + actualTableDepth.ToString("F2") +
                        ". Select the actual tabletop (15.79 x 8.54 units) or use a Transform-only tabletop anchor.", "OK");
                    return;
                }
                if (Mathf.Abs(actualTableWidth - TableWidth) > 0.75f ||
                    Mathf.Abs(actualTableDepth - TableDepth) > 0.75f)
                {
                    bool goAhead = EditorUtility.DisplayDialog("Different tabletop dimensions",
                        "Expected 15.79 x 8.54. Measured " + actualTableWidth.ToString("F2") +
                        " x " + actualTableDepth.ToString("F2") +
                        ". Board footprint remains 10.13 x 6.874. Place anyway?",
                        "Place", "Cancel");
                    if (!goAhead) return;
                }
                // Board prefab was built with its base at local Y = 0.
                placement = new Vector3(top.center.x, top.max.y, top.center.z);
            }
            else
            {
                // The selected object (e.g. Table_top) is an anchor, not geometry.
                // Use the user-supplied real tabletop dimensions for clearance checks.
                actualTableWidth = TableWidth;
                actualTableDepth = TableDepth;
                placement = table.transform.position;
                if (!EditorUtility.DisplayDialog("Use Transform as tabletop anchor?",
                    "'" + table.name + "' has no enabled Renderer or Collider.\n\n" +
                    "Use its world position as the tabletop SURFACE CENTER?\n" +
                    "X=" + placement.x.ToString("F3") + ", Y=" + placement.y.ToString("F3") +
                    ", Z=" + placement.z.ToString("F3") + "\n\n" +
                    "The board will be 10.13 x 6.874 world units, centered on the supplied " +
                    "15.79 x 8.54 tabletop. The anchor's Transform SCALE will be ignored.\n\n" +
                    "Cancel if the anchor's Y is not the tabletop surface height.",
                    "Place on anchor", "Cancel")) return;
            }

            GameObject placed = (GameObject)PrefabUtility.InstantiatePrefab(source);
            Undo.RegisterCreatedObjectUndo(placed, "Place Level 96 on tabletop");
            placed.name = LayoutName;
            placed.transform.position = placement;
            placed.transform.rotation = Quaternion.identity;
            // The layout scale was already calculated from imported PCB geometry at build time.
            // Never inherit an arbitrary Table_top Transform scale.
            placed.transform.localScale = Vector3.one;
            Selection.activeGameObject = placed;
            Debug.Log("[L96 Layout] Placed " + LayoutName + " at " + placement +
                (hasBounds ? " using tabletop geometry." : " using confirmed Transform anchor.") +
                " Expected centered margins: " +
                ((actualTableWidth - TargetWidth) * 0.5f).ToString("F3") + " each side and " +
                ((actualTableDepth - TargetDepth) * 0.5f).ToString("F3") +
                " each end. Verify actual top-surface height in Scene view.");
        }

        private static bool TryTableBounds(GameObject table, out Bounds bounds)
        {
            Collider col = table.GetComponent<Collider>();
            if (col != null && col.enabled) { bounds = col.bounds; return true; }
            Renderer renderer = table.GetComponent<Renderer>();
            if (renderer != null && renderer.enabled) { bounds = renderer.bounds; return true; }
            Collider[] colliders = table.GetComponentsInChildren<Collider>();
            for (int i = 0; i < colliders.Length; i++)
                if (colliders[i].enabled) { bounds = colliders[i].bounds; return true; }
            Renderer[] renderers = table.GetComponentsInChildren<Renderer>();
            for (int i = 0; i < renderers.Length; i++)
                if (renderers[i].enabled) { bounds = renderers[i].bounds; return true; }
            bounds = default(Bounds);
            return false;
        }

        // Unity occasionally imports the original procedurally exported ASCII FBX as an
        // empty hierarchy.  This method selects real mesh-bearing OBJ fallbacks generated
        // from the package's matching GLBs, preserving native millimeter coordinates.
        // Prefer OBJ for ALL parts to keep the unit/axis conventions consistent.
        private static Dictionary<string, string> ResolveModels(SparkAutoLevelRecipe recipe,
            SparkAutoLevelBlueprint blueprint, SparkAutoLevelBuildReport report)
        {
            var paths = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            string sourceRoot = recipe.sourceAssetsFolder.TrimEnd('/');
            string fallbackRoot = sourceRoot + "/Models_OBJ_Fallback/";
            foreach (SparkAutoPart part in blueprint.Parts)
            {
                string obj = fallbackRoot + part.Id + ".obj";
                if (HasMeshGeometry(obj))
                {
                    paths[part.Id] = obj;
                    continue;
                }

                string direct = sourceRoot + "/" + (part.AssetPath ?? "").TrimStart('/');
                if (HasMeshGeometry(direct))
                {
                    paths[part.Id] = direct;
                    continue;
                }

                string[] guids = AssetDatabase.FindAssets(part.Id + " t:Model", new[] { sourceRoot });
                string match = null;
                foreach (string guid in guids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    string filename = Path.GetFileNameWithoutExtension(path);
                    if (!(path.EndsWith(".fbx", StringComparison.OrdinalIgnoreCase) ||
                          path.EndsWith(".obj", StringComparison.OrdinalIgnoreCase)) ||
                        !(filename == part.Id || filename.EndsWith("_" + part.Id, StringComparison.OrdinalIgnoreCase)) ||
                        !HasMeshGeometry(path)) continue;
                    if (match != null) { report.Warn("Ambiguous usable mesh for " + part.Id); match = null; break; }
                    match = path;
                }
                if (match != null) paths[part.Id] = match;
                else report.Warn("No renderable imported model found for " + part.Id +
                    ". Install Models_OBJ_Fallback or repair the original FBX import.");
            }
            string assembled = recipe.assembledBoardModelName;
            string assembledObj = fallbackRoot + assembled + ".obj";
            string assembledFbx = sourceRoot + "/Models_FBX/" + assembled + ".fbx";
            if (HasMeshGeometry(assembledObj)) paths[assembled] = assembledObj;
            else if (HasMeshGeometry(assembledFbx)) paths[assembled] = assembledFbx;
            else report.Warn("Optional assembled model has no imported mesh; modular assembled reference will be constructed instead.");
            return paths;
        }

        private static bool HasMeshGeometry(string assetPath)
        {
            GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (asset == null) return false;
            MeshFilter[] filters = asset.GetComponentsInChildren<MeshFilter>(true);
            for (int i = 0; i < filters.Length; ++i)
                if (filters[i].sharedMesh != null && filters[i].sharedMesh.vertexCount > 0)
                    return true;
            SkinnedMeshRenderer[] skinned = asset.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            for (int i = 0; i < skinned.Length; ++i)
                if (skinned[i].sharedMesh != null && skinned[i].sharedMesh.vertexCount > 0)
                    return true;
            return false;
        }

        private static void BuildLayoutScene(SparkAutoLevelRecipe recipe, SparkAutoLevelBlueprint data,
            Dictionary<string, string> models, Dictionary<string, Material> materials,
            string prefabPath, string scenePath, SparkAutoLevelBuildReport report)
        {
            Scene previous = SceneManager.GetActiveScene();
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            try
            {
                SceneManager.SetActiveScene(scene);
                GameObject root = new GameObject(LayoutName);
                root.AddComponent<SparkAutoGeneratedMarker>().Configure(LevelId, false);
                root.AddComponent<SparkAutoLevelRuntime>().Configure(data, false);
                Meta(root, "L96", "LEVEL_LAYOUT", "AUTHORING_GENERATED", "Gameplay starts empty. Authority integration required.");
                var fitConfig = root.AddComponent<SparkAutoL96WorkshopFit>();

                Transform fit = Child(root.transform, "L96_TabletopFittedBoard_10_13x6_874");
                Transform modelSpace = Child(fit, "L96_NativeOBJModelSpace");
                Transform board = Child(modelSpace, "L96_BoardArtRoot");
                Transform slots = Child(modelSpace, "L96_ComponentSlots_44_UNINSTALLED");
                Transform pads = Child(modelSpace, "L96_PhysicalPads_SCHEMATIC_REQUIRED");
                Transform topology = Child(modelSpace, "L96_SolverTopology_SCHEMATIC_REQUIRED");
                Transform routes = Child(modelSpace, "L96_PlayerCopper_58_UNROUTED");
                Transform vias = Child(modelSpace, "L96_ViaSockets_12_UNASSIGNED");
                Transform layers = Child(modelSpace, "L96_CopperLayers_2_VisualOnly");
                Transform service = Child(modelSpace, "L96_ServiceableHardwareReferences_NotInstalled");
                Transform structural = Child(modelSpace, "L96_StructuralReferences");
                Transform faultVisual = Child(modelSpace, "L96_FaultOverlays_Disabled");
                Transform reference = Child(modelSpace, "L96_CompleteAssembledReference_NoProgress");
                Transform assembledFBX = Child(reference, "L96_CompleteAssembledModel");
                Transform modular = Child(reference, "L96_CompleteModularComposition");
                Transform systems = Child(root.transform, "L96_SensorCalibrationAndRepairSystems_UNBOUND");
                Transform safety = Child(root.transform, "L96_W059_IndependentSafety_UNBOUND");
                Transform objectives = Child(root.transform, "L96_Objectives_6_UNBOUND");
                Transform faults = Child(root.transform, "L96_RepairTargets_3_UNBOUND");
                Transform instruments = Child(root.transform, "L96_WorldSpaceVirtualInstruments_UNBOUND");
                Transform controls = Child(root.transform, "L96_WorldSpaceLevelControls_UNBOUND");

                GameObject empty = PlaceModel(models[recipe.bareBoardId], recipe.bareBoardId, board);
                SparkAutoL96PBRMaterials.Apply(empty, materials);
                Meta(empty, recipe.bareBoardId, "EMPTY_PCB", "VISIBLE_AT_START", "Primary PCB geometry; does not certify nets.", false);

                Bounds measured;
                if (!GetRendererBounds(empty, out measured))
                    throw new InvalidOperationException("Empty PCB has no MeshRenderer. Import the Models_OBJ_Fallback folder from the Level 96 full update package and retry.");
                bool originalPlaneXY = measured.size.y > measured.size.z * 2.0f;
                if (originalPlaneXY) modelSpace.localRotation = Quaternion.Euler(-90f, 0f, 0f);
                if (!GetRendererBounds(empty, out measured) || measured.size.x < 0.00001f || measured.size.z < 0.00001f)
                    throw new InvalidOperationException("Empty PCB has invalid XZ bounds after axis correction.");
                float mmToNative = measured.size.x / recipe.widthMm;
                float uniformScale = Mathf.Min(TargetWidth / measured.size.x,
                                                TargetDepth / measured.size.z);
                fit.localScale = Vector3.one * uniformScale;
                // Bare board base at prefab local Y=0, positionable on tabletop surface.
                fit.localPosition = new Vector3(0f, -measured.min.y * uniformScale, 0f);
                float actualBoardWidth = measured.size.x * uniformScale;
                float actualBoardDepth = measured.size.z * uniformScale;
                fitConfig.Configure(fit, TableWidth, TableDepth, actualBoardWidth, actualBoardDepth,
                    measured.size.x, measured.size.z, originalPlaneXY, mmToNative);
                report.Info("Uniform scale " + uniformScale.ToString("F4", CultureInfo.InvariantCulture) +
                    "; actual SMX-4 board footprint " + actualBoardWidth.ToString("F4") + " x " + actualBoardDepth.ToString("F4"));

                int extra = 0;
                foreach (SparkAutoPart part in data.Parts)
                {
                    if (part.Installable || part.Id == recipe.bareBoardId) continue;
                    string path;
                    if (!models.TryGetValue(part.Id, out path)) continue;
                    Transform destination = structural;
                    string type = (part.Type ?? "").ToUpperInvariant();
                    if (type.Contains("LAYER") || type.Contains("PLANE")) destination = layers;
                    else if (type.Contains("HEATSINK") || type.Contains("SPREADER") ||
                             type.Contains("HEATPIPE") || type.Contains("THERMAL_PAD")) destination = service;
                    else if (type.Contains("FAULT") || type.Contains("OVERLAY")) destination = faultVisual;
                    GameObject mesh = PlaceModel(path, part.Id, destination);
                    SetPartPose(mesh.transform, part, mmToNative, originalPlaneXY);
                    SparkAutoL96PBRMaterials.Apply(mesh, materials);
                    Meta(mesh, part.Id, part.Type, "ART_ONLY", part.Notes, true);
                    extra++;
                }
                layers.gameObject.SetActive(false);
                service.gameObject.SetActive(false);
                // Unverified supporting-detail meshes have no approved board-specific pose;
                // keep them imported and inspectable, but disabled to avoid fake installed art.
                structural.gameObject.SetActive(false);
                faultVisual.gameObject.SetActive(false);

                Dictionary<string, Vector2> officialFootprints = ReadSourceFootprints(recipe, report);
                int placedSlots = 0;
                SparkAutoLevelRuntime runtime = root.GetComponent<SparkAutoLevelRuntime>();
                foreach (SparkAutoPart part in data.Parts)
                {
                    if (!part.Installable) continue;
                    string modelPath = models[part.Id];
                    var go = Child(slots, part.Id).gameObject;
                    SetPartPose(go.transform, part, mmToNative, originalPlaneXY);
                    BoxCollider collider = go.AddComponent<BoxCollider>();
                    Vector2 sizeMm;
                    if (!officialFootprints.TryGetValue(part.Id, out sizeMm))
                        throw new InvalidOperationException("Missing source footprint dimensions for " + part.Id);
                    float nativeHeight = 3.0f * mmToNative;
                    collider.size = originalPlaneXY
                        ? new Vector3(sizeMm.x * mmToNative, sizeMm.y * mmToNative, nativeHeight)
                        : new Vector3(sizeMm.x * mmToNative, nativeHeight, sizeMm.y * mmToNative);
                    collider.center = originalPlaneXY
                        ? new Vector3(0f, 0f, nativeHeight * 0.5f)
                        : new Vector3(0f, nativeHeight * 0.5f, 0f);
                    var placement = go.AddComponent<SparkAutoPlacementTarget>();
                    placement.Configure(part.Id, part.Type, runtime, false,
                        AssetDatabase.LoadAssetAtPath<GameObject>(modelPath));
                    Meta(go, part.Id, "COMPONENT_SLOT", "UNINSTALLED", "Footprint target; exact pin mapping required for certification.", false, true);
                    GameObject visual = PlaceModel(modelPath, part.Id, modular);
                    SetPartPose(visual.transform, part, mmToNative, originalPlaneXY);
                    SparkAutoL96PBRMaterials.Apply(visual, materials);
                    placedSlots++;
                }
                int routeCount = 0;
                foreach (SparkAutoRoute route in data.Routes)
                {
                    Transform group = GetOrAdd(routes, "SUBSYSTEM_" + SafeName(route.Subsystem));
                    Transform entry = Child(group, route.Id);
                    Meta(entry.gameObject, route.Id, "COPPER_ROUTE", "NOT_CERTIFIABLE",
                        "Net=" + route.NetId + "; source=" + route.SourceComponent + "; destination=" +
                        route.DestinationComponent + "; layer=" + route.Layer + "; status=" + route.SourceStatus,
                        false, true);
                    routeCount++;
                }
                int viaCount = 0;
                foreach (SparkAutoVia via in data.Vias)
                {
                    Transform entry = Child(vias, via.Id);
                    Meta(entry.gameObject, via.Id, "VIA_TASK", "NOT_CERTIFIABLE",
                        "Net=" + via.NetId + "; layer=" + via.FromLayer + " to " + via.ToLayer +
                        "; XY=" + via.Xmm + "," + via.Ymm + "; status=" + via.SourceStatus,
                        false, true);
                    viaCount++;
                }
                for (int layer = 1; layer <= data.CopperLayers; layer++)
                    Meta(Child(layers, "COPPER_LAYER_" + layer.ToString("D2")).gameObject,
                        "L" + layer.ToString("D2"), "LAYER_REFERENCE", "ART_ONLY", "Not real electrical connectivity.", true);
                Meta(pads.gameObject, "L96_PADS_187_UNASSIGNED", "PADS", "SCHEMATIC_REQUIRED",
                    "The art source has 187 UNASSIGNED pad records but no approved pin-level net IDs or precise pad positions: cannot create electrical contacts.", false, true);
                Meta(topology.gameObject, "L96_TOPOLOGY_UNVERIFIED", "ELECTRICAL_GRAPH", "SCHEMATIC_REQUIRED",
                    "Do not synthesize arbitrary connections to meet route counts.", false, true);

                // Source contact names are recorded for inspector, NOT mapped to a fabricated pad position.
                int contactCount = BuildUnresolvedContacts(recipe, pads);
                if (contactCount != 187)
                    throw new InvalidOperationException("Source contact plan must contain exactly 187 records; got " + contactCount);
                report.Info("Created 187 named physical-contact records. No electrical net or pad location invented.");

                foreach (SparkAutoFault fault in data.Faults)
                    Meta(Child(faults, fault.Id + "_" + SafeName(fault.Title)).gameObject,
                        fault.Id, "REPAIR_SCENARIO", "NOT_STARTED",
                        "Mechanism=" + fault.Mechanism + "; Repair=" + fault.Repair, false, true);
                foreach (SparkAutoObjective objective in data.Objectives)
                    Meta(Child(objectives, "OBJ_" + objective.Id + "_" + SafeName(objective.Title)).gameObject,
                        objective.Id, "OBJECTIVE", "NOT_STARTED", objective.Criterion, false, true);
                foreach (string name in SensorAcquisitionSystems)
                    Meta(Child(systems, name).gameObject, name, "SYSTEM_BINDING", "UNBOUND",
                        "Bind to existing Project Spark sensor, voltage, sampling, calibration, fault-repair and objective services.");
                foreach (string name in VirtualTools)
                    Meta(Child(instruments, name).gameObject, name, "WORLD_SPACE_TOOL", "UNBOUND",
                        "Bind existing virtual tool; no physical laboratory instrument is generated.");
                foreach (string name in Controls)
                    Meta(Child(controls, name).gameObject, name, "UI_CONTROL_BINDING", "UNBOUND",
                        "Use the existing World-Space Canvas UI/actions.");
                Meta(safety.gameObject, "W059", "INDEPENDENT_REFERENCE_VALID_SAFETY", "DISCONNECTED",
                    data.Safety.LogicalSource + " -> " + data.Safety.LogicalDestination +
                    "; physicalPins=" + data.Safety.PhysicalPinStatus + "; requires fresh data and explicit reset.", false, true);
                Meta(Child(safety, "W059_TrustedReference_Jumper_Recovery").gameObject,
                    "W059", "REMOVABLE_JUMPER", "NOT_AUTHORIZED",
                    "Physical contacts and supervisor logic must be verified before enabling safety certification.", false, true);

                string assembledPath;
                bool hasAssembled = models.TryGetValue(recipe.assembledBoardModelName, out assembledPath);
                if (hasAssembled)
                {
                    GameObject mesh = PlaceModel(assembledPath, recipe.assembledBoardModelName, assembledFBX);
                    SparkAutoL96PBRMaterials.Apply(mesh, materials);
                    Meta(mesh, "L96_ASSEMBLED", "COMPLETE_BOARD_MESH", "REFERENCE_ONLY",
                        "Never count this artwork as installed parts or completed objectives.", true);
                }
                else
                {
                    // Build an art-only assembled board from 62 imported modular models.
                    foreach (SparkAutoPart part in data.Parts)
                    {
                        if (part.Installable || part.Id == recipe.bareBoardId) continue;
                        string path;
                        if (!models.TryGetValue(part.Id, out path)) continue;
                        GameObject mesh = PlaceModel(path, part.Id, modular);
                        SetPartPose(mesh.transform, part, mmToNative, originalPlaneXY);
                        SparkAutoL96PBRMaterials.Apply(mesh, materials);
                    }
                    GameObject boardCopy = PlaceModel(models[recipe.bareBoardId], recipe.bareBoardId, modular);
                    SparkAutoL96PBRMaterials.Apply(boardCopy, materials);
                    report.Warn("One-piece assembled mesh absent. Generated modular assembled art-only fallback.");
                }
                assembledFBX.gameObject.SetActive(hasAssembled);
                modular.gameObject.SetActive(!hasAssembled);
                reference.gameObject.SetActive(false);

                if (placedSlots != PartTotal || routeCount != RouteTotal || viaCount != ViaTotal)
                    throw new InvalidOperationException("Generated layout counts do not match imported Level 96 manifest.");
                report.Info("Level_96_Layout: 44 uninstalled part targets, 58 unapproved routes, 12 unapproved vias, " +
                    "3 faults, 6 objectives, 2 visual copper-layer references, " + extra +
                    " imported supporting assets, disabled pending verified placement.");
                report.Info("Model axis: " + (originalPlaneXY ? "XY rotated to XZ" : "native XZ") +
                    "; native import width=" + measured.size.x.ToString("F6", CultureInfo.InvariantCulture) +
                    "; native depth=" + measured.size.z.ToString("F6", CultureInfo.InvariantCulture));
                report.Info("WORKSHOP BOARD: " + TargetWidth + " x " + TargetDepth + " units; table " + TableWidth +
                    " x " + TableDepth + " units; margins (centered) " + ((TableWidth - TargetWidth) / 2f) +
                    " left/right, " + ((TableDepth - TargetDepth) / 2f) + " front/back.");
                report.Warn("187 physical contact mappings, routes W001-W058, vias V01-V12 and W059 physical pins " +
                    "are not approved; W059 U14 source is missing from the BOM. All electrical and final objective authority remains BLOCKED.");
                report.Warn("W059 requires reviewed physical pins on U8 and U9; the source only provides a digital trust path.");
                bool saved;
                PrefabUtility.SaveAsPrefabAsset(root, prefabPath, out saved);
                if (!saved) throw new IOException("Could not save prefab " + prefabPath);
                if (!EditorSceneManager.SaveScene(scene, scenePath))
                    throw new IOException("Could not save scene " + scenePath);
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
                if (previous.IsValid() && previous.isLoaded) SceneManager.SetActiveScene(previous);
            }
        }

        private static Dictionary<string, Vector2> ReadSourceFootprints(
            SparkAutoLevelRecipe recipe, SparkAutoLevelBuildReport report)
        {
            string path = SparkAutoLevelSourceImporter.ToAbsolute(
                recipe.sourceAssetsFolder.TrimEnd('/') + "/Data/L96_AutoGen_Components_44.csv");
            var result = new Dictionary<string, Vector2>(StringComparer.OrdinalIgnoreCase);
            if (!File.Exists(path)) throw new FileNotFoundException("Missing Level 96 BOM dimensions", path);
            string[] lines = File.ReadAllLines(path);
            for (int i = 1; i < lines.Length; i++)
            {
                string[] cells = lines[i].Split(',');
                if (cells.Length < 13) throw new InvalidDataException("Bad component BOM line " + (i + 1));
                float w, d;
                if (!float.TryParse(cells[11], NumberStyles.Float, CultureInfo.InvariantCulture, out w) ||
                    !float.TryParse(cells[12], NumberStyles.Float, CultureInfo.InvariantCulture, out d))
                    throw new InvalidDataException("Non-numeric footprint dimension on line " + (i + 1));
                if (w <= 0f || d <= 0f || result.ContainsKey(cells[0]))
                    throw new InvalidDataException("Invalid / duplicate footprint " + cells[0]);
                result.Add(cells[0], new Vector2(w, d));
            }
            report.Info("Exact source footprint sizes: " + result.Count + " provisional art footprints (not electrically approved).");
            return result;
        }

        private static int BuildUnresolvedContacts(SparkAutoLevelRecipe recipe, Transform pads)
        {
            string path = SparkAutoLevelSourceImporter.ToAbsolute(
                recipe.sourceAssetsFolder.TrimEnd('/') + "/Data/L96_AutoGen_ContactPlan_187.csv");
            if (!File.Exists(path)) throw new FileNotFoundException("Missing Level 96 contact plan", path);
            string[] lines = File.ReadAllLines(path);
            int count = 0;
            for (int i = 1; i < lines.Length; i++)
            {
                string[] cells = lines[i].Split(',');
                if (cells.Length < 5) throw new InvalidDataException("Invalid contact plan line " + (i + 1));
                Transform owner = GetOrAdd(pads, cells[0]);
                Transform contact = Child(owner, SafeName(cells[1]));
                Meta(contact.gameObject, cells[1], "UNMAPPED_COMPONENT_CONTACT", "SCHEMATIC_REQUIRED",
                    "Source=" + cells[0] + "; signal=" + cells[3] + "; no implied physical position or connectivity.", false, true);
                count++;
            }
            return count;
        }

        private static Vector2 FootprintSize(string type)
        {
            string name = (type ?? "").ToUpperInvariant();
            if (name.Contains("CPU")) return new Vector2(45f, 45f);
            if (name.Contains("DDR")) return new Vector2(13f, 13f);
            if (name.Contains("INDUCTOR")) return new Vector2(11f, 11f);
            if (name.Contains("MOSFET")) return new Vector2(7f, 7f);
            if (name.Contains("CONTROLLER") || name.Contains("MONITOR")) return new Vector2(9f, 9f);
            if (name.Contains("CONNECTOR")) return new Vector2(10f, 6f);
            if (name.Contains("CAP")) return new Vector2(3.2f, 2.4f);
            if (name.Contains("RES")) return new Vector2(2.5f, 1.8f);
            return new Vector2(5f, 5f);
        }
        private static void SetPartPose(Transform transform, SparkAutoPart part, float nativePerMm, bool planeXY)
        {
            transform.localPosition = planeXY
                ? new Vector3(part.Xmm, part.Ymm, part.Zmm) * nativePerMm
                : new Vector3(part.Xmm, part.Zmm, -part.Ymm) * nativePerMm;
            if (Mathf.Abs(part.OrientationDegrees) > 0.001f)
                transform.localRotation *= planeXY
                    ? Quaternion.Euler(0f, 0f, part.OrientationDegrees)
                    : Quaternion.Euler(0f, part.OrientationDegrees, 0f);
        }
        private static GameObject PlaceModel(string path, string name, Transform parent)
        {
            GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (asset == null) throw new FileNotFoundException("Imported FBX/OBJ missing or invalid: " + path);
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(asset);
            instance.name = name;
            instance.transform.SetParent(parent, false);
            return instance;
        }
        private static bool GetRendererBounds(GameObject root, out Bounds bounds)
        {
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            bool found = false;
            bounds = default(Bounds);
            foreach (Renderer renderer in renderers)
            {
                if (!found) { bounds = renderer.bounds; found = true; }
                else bounds.Encapsulate(renderer.bounds);
            }
            return found;
        }
        private static Transform Child(Transform parent, string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            return go.transform;
        }
        private static Transform GetOrAdd(Transform parent, string name)
        {
            Transform child = parent.Find(name);
            return child != null ? child : Child(parent, name);
        }
        private static string SafeName(string name)
        {
            if (string.IsNullOrEmpty(name)) return "Unnamed";
            return name.Replace(' ', '_').Replace('/', '_').Replace('\\', '_').Replace(':', '_').Replace('&', '_');
        }
        private static void Meta(GameObject go, string id, string category, string status, string notes,
            bool isReference = false, bool schematic = false)
        {
            go.AddComponent<SparkAutoL96LayoutRecord>().Configure(id, category, status, notes, isReference, schematic);
        }
    }
}
#endif
