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
    /// Fully populated SOURCE/BINDING layout for PFX-15, not a simulated working circuit.
    /// Requires the installed Universal Auto Level Generator V2 runtime/editor classes.
    /// Native model geometry is inspected, rather than assuming Unity's ASCII FBX import scale.
    /// </summary>
    public static class SparkAutoL107FullLayoutBuilder
    {
        private const int LevelId = 107;
        private const string RecipeFile = "Level107_PFX15.recipe.json";
        private const string LayoutName = "Level_107_Layout";
        private const float TableWidth = 15.79f;
        private const float TableDepth = 8.54f;
        private const float TargetWidth = 10.13f;
        private const float TargetDepth = 7.51f;
        private const int PartTotal = 112, RouteTotal = 148, ViaTotal = 46;
        private const int FaultTotal = 3, ObjectiveTotal = 6;
        private static readonly string[] VirtualTools = {
            "Multimeter", "CurrentLimitedPowerSource", "Oscilloscope", "SPIAnalyzer", "FirmwareProgrammer",
            "ImageIntegrityVerifier", "RecoveryConsole", "NVRAMEditor", "POSTAnalyzer", "LogicAnalyzer",
            "Microscope", "SolderDesolder", "ComponentReplacement", "NetTracer", "TenLayerViewer",
            "EvidenceNotebook", "BootSequenceViewer", "ProtectedRecoveryBankInspector", "Calibration"
        };
        private static readonly string[] Controls = {
            "INSPECT", "ASSEMBLE", "ROUTE", "VIA", "SELECT", "MEASURE", "POWER_OFF",
            "CURRENT_LIMITED_POWER_ON", "READ_PRIMARY_SPI", "READ_RECOVERY_SPI", "VERIFY_FIRMWARE",
            "CHECK_SIGNATURE", "AUTHORIZE_RECOVERY", "ARBITRATE_SPI_BUS", "ERASE_PRIMARY",
            "PROGRAM_PRIMARY", "READBACK_VERIFY", "SIMULATE_INTERRUPTED_UPDATE", "RESTART_BOOT",
            "INSPECT_NVRAM", "RESET_NVRAM", "REPAIR_SPI_CS", "REPLACE_PULLUP_RESISTOR",
            "W149_DISCONNECT", "W149_RECONNECT", "W149_RESET_SAFETY", "REQUALIFY_POST",
            "RUN_DIAGNOSTICS", "SHOW_OBJECTIVES", "SAVE_LEVEL", "RELOAD_LEVEL"
        };
        private static readonly string[] DiagnosticSystems = {
            "Input12VAndStandby3V3", "CurrentLimitedStartup", "CPUAndVRM_RailReadiness",
            "U5_RecoveryController", "U6_PrimarySPINOR", "U7_ProtectedRecoverySPINOR",
            "SPIBusArbiter", "SPIChipSelectPhysicalContact", "FirmwareImageIntegrityAndTrust",
            "AuthorizedRecoveryTransaction", "DualBankReadbackValidation", "VirtualNVRAM",
            "POSTAndBootSequence", "ResetAndClockReadiness", "InterruptedUpdateScenario",
            "Fault_F1_PrimarySPIChipSelect", "Fault_F2_FirmwareCorruption",
            "Fault_F3_RecoveryInputPullup", "IndependentW149FirmwareTrustSafety",
            "AuthenticatedRepairAuthority", "EvidenceAndQualificationEpochs"
        };

        [MenuItem("Tools/Project Spark/Automatic Level Generator/107/1. Build FULL Level_107_Layout")]
        public static void BuildFullLevel107()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogError("[L107 Layout] Stop Play Mode before generating.");
                return;
            }
            SparkAutoLevelBlueprint transient = null;
            var report = new SparkAutoLevelBuildReport();
            try
            {
                SparkAutoLevelRecipe recipe = SparkAutoLevelSourceImporter.LoadRecipe(RecipeFile);
                if (recipe.levelId != LevelId) throw new InvalidOperationException("Level 107 recipe expected");
                transient = SparkAutoLevelSourceImporter.Import(recipe, report);
                if (report.Errors != 0)
                    throw new InvalidOperationException("Source validation failed:\n" + report.Render());
                if (transient.Parts.Count != 144 || transient.InstallableCount != PartTotal ||
                    transient.Routes.Count != RouteTotal || transient.Vias.Count != ViaTotal ||
                    transient.Faults.Count != FaultTotal || transient.Objectives.Count != ObjectiveTotal)
                    throw new InvalidOperationException("Level 107 source counts have changed. Refusing partial build.");

                var modelPaths = ResolveModels(recipe, transient, report);
                string bareAsset;
                if (!modelPaths.TryGetValue(recipe.bareBoardId, out bareAsset))
                    throw new FileNotFoundException("Missing EMPTY PFX-15 PCB FBX: " + recipe.bareBoardId);
                int missing = 0;
                foreach (SparkAutoPart part in transient.Parts)
                    if (part.Installable && !modelPaths.ContainsKey(part.Id)) missing++;
                if (missing > 0) throw new FileNotFoundException(missing + " installable FBX models failed import. Build cancelled.");

                string output = recipe.generatedAssetsFolder.TrimEnd('/');
                SparkAutoLevelSceneBuilder.EnsureFolder(output);
                string prefabPath = output + "/" + LayoutName + ".prefab";
                string scenePath = output + "/" + LayoutName + ".unity";
                if ((AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) != null ||
                     AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath) != null) &&
                    !EditorUtility.DisplayDialog("Regenerate Level 107?",
                        "This will overwrite ONLY the generated Level_107_Layout prefab and scene. " +
                        "Keep hand edits in a separate prefab. Previous levels are untouched.",
                        "Overwrite generated Level 107", "Cancel")) return;

                string blueprintPath = output + "/L107_SourceBlueprint.asset";
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
                Dictionary<string, Material> materials = SparkAutoURPMaterialBuilder.Build(recipe, report);
                BuildLayoutScene(recipe, persistent, modelPaths, materials, prefabPath, scenePath, report);
                File.WriteAllText(SparkAutoLevelSourceImporter.ToAbsolute(output + "/L107_Layout_Validation.txt"),
                    report.Render() + "\nGeometry centered and workshop footprint 10.13 x 7.51 units. " +
                    "Electrical pins/nets and gameplay service bindings still require authoritative design.\n");
                AssetDatabase.Refresh();
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                if (prefab != null) { Selection.activeObject = prefab; EditorGUIUtility.PingObject(prefab); }
                Debug.Log("[L107 Layout] Generated " + prefabPath + "\n" + report.Render());
            }
            catch (Exception e)
            {
                Debug.LogError("[L107 Layout] " + e);
                EditorUtility.DisplayDialog("Level 107 generation stopped", e.Message, "OK");
            }
            finally
            {
                if (transient != null) UnityEngine.Object.DestroyImmediate(transient);
            }
        }

        [MenuItem("Tools/Project Spark/Automatic Level Generator/107/2. Place Level 107 on SELECTED tabletop")]
        public static void PlaceOnSelectedTabletop()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            GameObject table = Selection.activeGameObject;
            if (table == null) { EditorUtility.DisplayDialog("Select the table", "Select your tabletop GameObject in the Hierarchy first.", "OK"); return; }
            string expected = "Assets/ProjectSpark/AutoGeneratedLevels/Level107/" + LayoutName + ".prefab";
            GameObject source = AssetDatabase.LoadAssetAtPath<GameObject>(expected);
            if (source == null) { EditorUtility.DisplayDialog("Missing layout", "Build FULL Level 107 layout first.", "OK"); return; }
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
                        ". Board footprint remains 10.13 x 7.51. Place anyway?",
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
                    "The board will be 10.13 x 7.51 world units, centered on the supplied " +
                    "15.79 x 8.54 tabletop. The anchor's Transform SCALE will be ignored.\n\n" +
                    "Cancel if the anchor's Y is not the tabletop surface height.",
                    "Place on anchor", "Cancel")) return;
            }

            Scene activeScene = SceneManager.GetActiveScene();
            if (activeScene.IsValid() && activeScene.isLoaded)
            {
                foreach (GameObject sceneRoot in activeScene.GetRootGameObjects())
                {
                    if (sceneRoot.name != LayoutName) continue;
                    EditorUtility.DisplayDialog("Level already placed",
                        "A Level_107_Layout root already exists in this scene. Select or remove it before placing another copy.", "OK");
                    return;
                }
            }
            GameObject placed = (GameObject)PrefabUtility.InstantiatePrefab(source);
            Undo.RegisterCreatedObjectUndo(placed, "Place Level 107 on tabletop");
            placed.name = LayoutName;
            placed.transform.position = placement;
            placed.transform.rotation = Quaternion.identity;
            // The layout scale was already calculated from imported PCB geometry at build time.
            // Never inherit an arbitrary Table_top Transform scale.
            placed.transform.localScale = Vector3.one;
            Selection.activeGameObject = placed;
            Debug.Log("[L107 Layout] Placed " + LayoutName + " at " + placement +
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

        private static Dictionary<string, string> ResolveModels(SparkAutoLevelRecipe recipe,
            SparkAutoLevelBlueprint blueprint, SparkAutoLevelBuildReport report)
        {
            var paths = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            string root = recipe.sourceAssetsFolder.TrimEnd('/');
            foreach (SparkAutoPart part in blueprint.Parts)
            {
                string direct = root + "/" + (part.AssetPath ?? "").TrimStart('/');
                if (AssetDatabase.LoadAssetAtPath<GameObject>(direct) != null) { paths[part.Id] = direct; continue; }
                string[] guids = AssetDatabase.FindAssets(part.Id + " t:Model", new[] { "Assets" });
                string match = null;
                foreach (string guid in guids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    string filename = Path.GetFileNameWithoutExtension(path);
                    if (!path.EndsWith(".fbx", StringComparison.OrdinalIgnoreCase) ||
                        !(filename == part.Id || filename.EndsWith("_" + part.Id, StringComparison.OrdinalIgnoreCase))) continue;
                    if (match != null) { report.Warn("Ambiguous FBX match for " + part.Id + "; ignoring fallback."); match = null; break; }
                    match = path;
                }
                if (match != null) paths[part.Id] = match;
                else report.Warn("FBX model not found: " + part.Id + " expected " + direct);
            }
            string assembled = recipe.assembledBoardModelName;
            string candidate = root + "/Models_FBX/" + assembled + ".fbx";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(candidate) != null) paths[assembled] = candidate;
            else report.Warn("Completed PFX-15 reference missing; modular assembled preview will be used.");
            return paths;
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
                Meta(root, "L107", "LEVEL_LAYOUT", "AUTHORING_GENERATED", "Gameplay starts empty. Authority integration required.");
                var fitConfig = root.AddComponent<SparkAutoL107WorkshopFit>();

                Transform fit = Child(root.transform, "L107_TabletopFittedBoard_10_13x7_51");
                Transform modelSpace = Child(fit, "L107_NativeFBXModelSpace");
                Transform board = Child(modelSpace, "L107_BoardArtRoot");
                Transform slots = Child(modelSpace, "L107_ComponentSlots_112_UNINSTALLED");
                Transform pads = Child(modelSpace, "L107_PhysicalPads_SCHEMATIC_REQUIRED");
                Transform topology = Child(modelSpace, "L107_SolverTopology_SCHEMATIC_REQUIRED");
                Transform routes = Child(modelSpace, "L107_PlayerCopper_148_UNROUTED");
                Transform vias = Child(modelSpace, "L107_ViaSockets_46_UNASSIGNED");
                Transform layers = Child(modelSpace, "L107_CopperLayers_10_VisualOnly");
                Transform service = Child(modelSpace, "L107_ServiceableComponents_NotInstalled");
                Transform structural = Child(modelSpace, "L107_StructuralReferences");
                Transform faultVisual = Child(modelSpace, "L107_FaultOverlays_Disabled");
                Transform reference = Child(modelSpace, "L107_CompleteAssembledReference_NoProgress");
                Transform assembledFBX = Child(reference, "L107_CompleteAssembledFBX");
                Transform modular = Child(reference, "L107_CompleteModularComposition");
                Transform systems = Child(root.transform, "L107_ElectricalFirmwareAndDiagnosticSystems_UNBOUND");
                Transform safety = Child(root.transform, "L107_W149_IndependentSafety_UNBOUND");
                Transform objectives = Child(root.transform, "L107_Objectives_6_UNBOUND");
                Transform faults = Child(root.transform, "L107_RepairTargets_3_UNBOUND");
                Transform instruments = Child(root.transform, "L107_WorldSpaceVirtualInstruments_UNBOUND");
                Transform controls = Child(root.transform, "L107_WorldSpaceLevelControls_UNBOUND");

                GameObject empty = PlaceModel(models[recipe.bareBoardId], recipe.bareBoardId, board);
                SparkAutoURPMaterialBuilder.Apply(empty, materials);
                Meta(empty, recipe.bareBoardId, "EMPTY_PCB", "VISIBLE_AT_START", "Primary PCB geometry; does not certify nets.", true);

                Bounds measured;
                if (!GetRendererBounds(empty, out measured))
                    throw new InvalidOperationException("Imported empty PCB has no measurable Renderer; cannot set workshop footprint.");
                bool originalPlaneXY = measured.size.y > measured.size.z * 2.0f;
                if (originalPlaneXY) modelSpace.localRotation = Quaternion.Euler(-90f, 0f, 0f);
                if (!GetRendererBounds(empty, out measured) || measured.size.x < 0.00001f || measured.size.z < 0.00001f)
                    throw new InvalidOperationException("Empty PCB has invalid XZ bounds after axis correction.");
                float nativeXPerMm = measured.size.x / recipe.widthMm;
                float nativeDepthPerMm = measured.size.z / recipe.heightMm;
                float mmToNative = nativeXPerMm;
                float scaleX = TargetWidth / measured.size.x;
                float scaleZ = TargetDepth / measured.size.z;
                float scaleY = (scaleX + scaleZ) * 0.5f;
                fit.localScale = new Vector3(scaleX, scaleY, scaleZ);
                fit.localPosition = new Vector3(0f, -measured.min.y * scaleY, 0f);
                fitConfig.Configure(fit, TableWidth, TableDepth, TargetWidth, TargetDepth,
                    measured.size.x, measured.size.z, originalPlaneXY, mmToNative);

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
                    SetPartPose(mesh.transform, part, nativeXPerMm, nativeDepthPerMm, originalPlaneXY);
                    SparkAutoURPMaterialBuilder.Apply(mesh, materials);
                    Meta(mesh, part.Id, part.Type, "ART_ONLY", part.Notes, true);
                    extra++;
                }
                layers.gameObject.SetActive(false);
                service.gameObject.SetActive(false);
                faultVisual.gameObject.SetActive(false);

                int placedSlots = 0;
                SparkAutoLevelRuntime runtime = root.GetComponent<SparkAutoLevelRuntime>();
                foreach (SparkAutoPart part in data.Parts)
                {
                    if (!part.Installable) continue;
                    string modelPath = models[part.Id];
                    var go = Child(slots, part.Id).gameObject;
                    SetPartPose(go.transform, part, nativeXPerMm, nativeDepthPerMm, originalPlaneXY);
                    BoxCollider collider = go.AddComponent<BoxCollider>();
                    Vector2 sizeMm = FootprintSize(part.Type);
                    float nativeHeight = 3.0f * nativeXPerMm;
                    collider.size = originalPlaneXY
                        ? new Vector3(sizeMm.x * nativeXPerMm, sizeMm.y * nativeDepthPerMm, nativeHeight)
                        : new Vector3(sizeMm.x * nativeXPerMm, nativeHeight, sizeMm.y * nativeDepthPerMm);
                    collider.center = originalPlaneXY
                        ? new Vector3(0f, 0f, nativeHeight * 0.5f)
                        : new Vector3(0f, nativeHeight * 0.5f, 0f);
                    var placement = go.AddComponent<SparkAutoPlacementTarget>();
                    placement.Configure(part.Id, part.Type, runtime, false,
                        AssetDatabase.LoadAssetAtPath<GameObject>(modelPath));
                    Meta(go, part.Id, "COMPONENT_SLOT", "UNINSTALLED", "Source-art footprint proxy, NOT a certified pad layout; verify package orientation and contacts.", false, true);
                    GameObject visual = PlaceModel(modelPath, part.Id, modular);
                    SetPartPose(visual.transform, part, nativeXPerMm, nativeDepthPerMm, originalPlaneXY);
                    SparkAutoURPMaterialBuilder.Apply(visual, materials);
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
                        "L" + layer.ToString("D2"), "LAYER_REFERENCE", "ART_ONLY", "Visual-only reference; does not modify solved circuit.", true);
                Meta(pads.gameObject, "L107_PADS", "PADS", "SCHEMATIC_REQUIRED",
                    "No verified pad-to-pin topology supplied; test-pad FBXs are visual references only.", false, true);
                Meta(topology.gameObject, "L107_TOPOLOGY", "ELECTRICAL_GRAPH", "SCHEMATIC_REQUIRED",
                    "Do not synthesize arbitrary connections to meet route counts.", false, true);

                foreach (SparkAutoFault fault in data.Faults)
                    Meta(Child(faults, fault.Id + "_" + SafeName(fault.Title)).gameObject,
                        fault.Id, "REPAIR_SCENARIO", "NOT_STARTED",
                        "Mechanism=" + fault.Mechanism + "; Repair=" + fault.Repair, false, true);
                foreach (SparkAutoObjective objective in data.Objectives)
                    Meta(Child(objectives, "OBJ_" + objective.Id + "_" + SafeName(objective.Title)).gameObject,
                        objective.Id, "OBJECTIVE", "NOT_STARTED", objective.Criterion, false, true);
                foreach (string name in DiagnosticSystems)
                    Meta(Child(systems, name).gameObject, name, "SYSTEM_BINDING", "UNBOUND",
                        "Bind actual Project Spark electrical graph, SPI firmware, POST, recovery and diagnostics.");
                foreach (string name in VirtualTools)
                    Meta(Child(instruments, name).gameObject, name, "WORLD_SPACE_TOOL", "UNBOUND",
                        "Bind existing virtual tool; no physical laboratory instrument is generated.");
                foreach (string name in Controls)
                    Meta(Child(controls, name).gameObject, name, "UI_CONTROL_BINDING", "UNBOUND",
                        "Use the existing World-Space Canvas UI/actions.");
                Meta(safety.gameObject, "W149", "FIRMWARE_TRUST_SAFETY", "DISCONNECTED",
                    data.Safety.LogicalSource + " -> " + data.Safety.LogicalDestination +
                    "; physicalPins=" + data.Safety.PhysicalPinStatus + "; requires fresh data and explicit reset.", false, true);
                Meta(Child(safety, "W149_Jumper_Removal_And_Recovery").gameObject,
                    "W149", "REMOVABLE_JUMPER", "NOT_AUTHORIZED",
                    "Physical contacts and supervisor logic must be verified before enabling safety certification.", false, true);

                string assembledPath;
                bool hasAssembled = models.TryGetValue(recipe.assembledBoardModelName, out assembledPath);
                if (hasAssembled)
                {
                    GameObject mesh = PlaceModel(assembledPath, recipe.assembledBoardModelName, assembledFBX);
                    // Source PFX-15 completed FBX reuses prior geometry (approximately 285 x 205 mm); target is 290 x 210 mm.
                    // Match fitted board X/Z dimensions at display scale, never claim electrical alignment.
                    Bounds referenceBounds;
                    if (GetRendererBounds(mesh, out referenceBounds) &&
                        referenceBounds.size.x > 0.00001f && referenceBounds.size.z > 0.00001f)
                    {
                        float refX = TargetWidth / referenceBounds.size.x;
                        float refDepth = TargetDepth / referenceBounds.size.z;
                        Vector3 current = mesh.transform.localScale;
                        mesh.transform.localScale = originalPlaneXY
                            ? new Vector3(current.x * refX, current.y * refDepth, current.z * (refX + refDepth) * 0.5f)
                            : new Vector3(current.x * refX, current.y * (refX + refDepth) * 0.5f, current.z * refDepth);
                        report.Warn("Completed L107 reference uses legacy procedural geometry; " +
                            "reference DISPLAY bounds normalized to fitted board. Gameplay authority remains unverified.");
                    }
                    SparkAutoURPMaterialBuilder.Apply(mesh, materials);
                    Meta(mesh, "L107_ASSEMBLED", "COMPLETE_BOARD_FBX", "REFERENCE_ONLY",
                        "Never count this artwork as installed parts or completed objectives.", true);
                }
                else
                {
                    // Build an art-only assembled board from 144 imported modular models.
                    foreach (SparkAutoPart part in data.Parts)
                    {
                        if (part.Installable || part.Id == recipe.bareBoardId) continue;
                        string path;
                        if (!models.TryGetValue(part.Id, out path)) continue;
                        GameObject mesh = PlaceModel(path, part.Id, modular);
                        SetPartPose(mesh.transform, part, nativeXPerMm, nativeDepthPerMm, originalPlaneXY);
                        SparkAutoURPMaterialBuilder.Apply(mesh, materials);
                    }
                    GameObject boardCopy = PlaceModel(models[recipe.bareBoardId], recipe.bareBoardId, modular);
                    SparkAutoURPMaterialBuilder.Apply(boardCopy, materials);
                    report.Warn("One-piece assembled FBX absent. Generated modular assembled art-only fallback.");
                }
                assembledFBX.gameObject.SetActive(hasAssembled);
                modular.gameObject.SetActive(!hasAssembled);
                reference.gameObject.SetActive(false);

                if (placedSlots != PartTotal || routeCount != RouteTotal || viaCount != ViaTotal)
                    throw new InvalidOperationException("Generated layout counts do not match imported Level 107 manifest.");
                report.Info("Level_107_Layout: 112 uninstalled part targets, 148 unapproved routes, 46 unapproved vias, " +
                    "3 faults, 6 objectives, 10 copper layer references, " + extra + " supplemental meshes.");
                report.Warn("Source PFX-15 bare board geometry = approx. 285x205 mm; specification = 290x210 mm. " +
                    "Artwork/footprints adjusted for 10.13x7.51 display size; no approved pad geometry inferred.");
                report.Info("Model axis: " + (originalPlaneXY ? "XY rotated to XZ" : "native XZ") +
                    "; native import width=" + measured.size.x.ToString("F6", CultureInfo.InvariantCulture) +
                    "; native depth=" + measured.size.z.ToString("F6", CultureInfo.InvariantCulture));
                report.Info("WORKSHOP BOARD: " + TargetWidth + " x " + TargetDepth + " units; table " + TableWidth +
                    " x " + TableDepth + " units; margins (centered) " + ((TableWidth - TargetWidth) / 2f) +
                    " left/right, " + ((TableDepth - TargetDepth) / 2f) + " front/back.");
                report.Warn("L107 physical pad mapping, routes W001-W148, vias V01-V46 and W149 physical pins " +
                    "are not approved; electrical and final objective authority remain BLOCKED.");
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
        private static void SetPartPose(Transform transform, SparkAutoPart part,
            float nativeXPerMm, float nativeDepthPerMm, bool planeXY)
        {
            transform.localPosition = planeXY
                ? new Vector3(part.Xmm * nativeXPerMm, part.Ymm * nativeDepthPerMm, part.Zmm * nativeXPerMm)
                : new Vector3(part.Xmm * nativeXPerMm, part.Zmm * nativeXPerMm, -part.Ymm * nativeDepthPerMm);
            if (Mathf.Abs(part.OrientationDegrees) > 0.001f)
                transform.localRotation *= planeXY
                    ? Quaternion.Euler(0f, 0f, part.OrientationDegrees)
                    : Quaternion.Euler(0f, part.OrientationDegrees, 0f);
        }
        private static GameObject PlaceModel(string path, string name, Transform parent)
        {
            GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (asset == null) throw new FileNotFoundException("Imported FBX missing/invalid: " + path);
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
            go.AddComponent<SparkAutoL107LayoutRecord>().Configure(id, category, status, notes, isReference, schematic);
        }
    }
}
#endif
