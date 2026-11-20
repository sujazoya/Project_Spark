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
    /// Full source-art and gameplay-binding layout for Level 105 ENX-13 Expansion Nexus + ECX-4 removable endpoint.
    /// Requires the installed Universal Auto Level Generator V2 runtime/editor classes.
    /// Prefers GLB-derived mesh-bearing OBJ fallback over unrenderable FBX.
    /// The imported OBJ is a VISUAL asset; no pin netlist or completed gameplay is inferred.
    /// </summary>
    public static class SparkAutoL105FullLayoutBuilder
    {
        private const int LevelId = 105;
        private const string RecipeFile = "Level105_ENX13.recipe.json";
        private const string LayoutName = "Level_105_Layout";
        private const float TableWidth = 15.79f;
        private const float TableDepth = 8.54f;
        private const float TargetWidth = 10.13f;
        private const float TargetDepth = 7.51f;
        private const int PartTotal = 100, RouteTotal = 132, ViaTotal = 38;
        private const int FaultTotal = 3, ObjectiveTotal = 6;
        // Binding anchors only: existing Project Spark systems own the actual actions and state.
        private static readonly string[] VirtualTools = {
            "Multimeter", "Oscilloscope", "DifferentialScope", "LogicAnalyzer", "NetTracer",
            "PCIELinkAnalyzer", "FourLaneMonitor", "EyeDiagramViewer", "ClockJitterAnalyzer",
            "EndpointEnumerator", "BidirectionalTransferTester", "ContactMicroscope",
            "PCBLayerViewer", "SolderDesolder", "ComponentReplacement", "BoardInspection"
        };
        private static readonly string[] Controls = {
            "INSPECT", "ASSEMBLE", "ROUTE", "VIA", "MEASURE", "LAYER_VIEW", "SEAT_ECX4",
            "REMOVE_ECX4", "CURRENT_LIMITED_POWER_ON", "CHECK_STANDBY", "CHECK_REFCLK",
            "CHECK_RESET", "RUN_LINK_TRAINING", "VERIFY_X4_5GT", "ENUMERATE_ENDPOINT",
            "RUN_TRANSFER_TEST", "DIAGNOSE_F1_AC_CAP", "REPAIR_F1", "DIAGNOSE_F2_REFCLK",
            "REPAIR_F2", "DIAGNOSE_F3_LANE3_CONTACT", "REPAIR_F3", "W133_DISCONNECT",
            "W133_RECONNECT", "W133_RESET_SAFETY", "REQUALIFY", "SHOW_OBJECTIVES",
            "SAVE_LEVEL", "RELOAD_LEVEL"
        };
        private static readonly string[] ExpansionSystems = {
            "MainBoard_ENX13", "RemovableEndpoint_ECX4", "PCIeSlotAndEdgeContacts",
            "CardInsertionAndPresence", "Power12VAnd3V3", "DifferentialReferenceClock_100MHz",
            "ResetAndLinkPrerequisites", "FourPCIeDifferentialLanes", "ACCouplingAndImpedance",
            "EducationalGen2_LTSSM", "LinkWidthSpeedNegotiation", "DeviceEnumeration",
            "BidirectionalTrafficIntegrity", "F1_Lane2_AC_Cap_Open", "F2_ClockIntegrity",
            "F3_Lane3_Contact_Intermittent", "W133_IndependentFeedbackSafety",
            "RevisionAwareTestEvidence", "LevelGamePlayManagerAdapter_UNBOUND"
        };
        // From the source ModelManifest: 94 ENX-13 parts, six ECX-4 parts.
        private static readonly HashSet<string> CardPartIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase) {
            "002_L105_U2", "010_L105_U10", "011_L105_U11",
            "079_L105_J3", "083_L105_J7", "088_L105_LED5"
        };
        private const string CardBareId = "110_L105_ECX4_RemovableCard";

        public static void CreateLevel105()
        {
            BuildFullLevel105();
        }

        [MenuItem("Tools/Project Spark/Automatic Level Generator/105/1. Build FULL Level_105_Layout")]
        public static void BuildFullLevel105()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogError("[L105 Layout] Stop Play Mode before generating.");
                return;
            }
            SparkAutoLevelBlueprint transient = null;
            var report = new SparkAutoLevelBuildReport();
            try
            {
                SparkAutoLevelRecipe recipe = SparkAutoLevelSourceImporter.LoadRecipe(RecipeFile);
                if (recipe.levelId != LevelId) throw new InvalidOperationException("Level 105 recipe expected");
                transient = SparkAutoLevelSourceImporter.Import(recipe, report);
                if (report.Errors != 0)
                    throw new InvalidOperationException("Source validation failed:\n" + report.Render());
                if (transient.Parts.Count != 128 || transient.InstallableCount != PartTotal ||
                    transient.Routes.Count != RouteTotal || transient.Vias.Count != ViaTotal ||
                    transient.Faults.Count != FaultTotal || transient.Objectives.Count != ObjectiveTotal)
                    throw new InvalidOperationException("Level 105 source counts have changed. Refusing partial build.");

                var modelPaths = ResolveModels(recipe, transient, report);
                string bareAsset;
                if (!modelPaths.TryGetValue(recipe.bareBoardId, out bareAsset))
                    throw new FileNotFoundException("Missing renderable EMPTY ENX-13 PCB: " + recipe.bareBoardId);
                int missing = 0;
                foreach (SparkAutoPart part in transient.Parts)
                    if (part.Installable && !modelPaths.ContainsKey(part.Id)) missing++;
                if (!modelPaths.ContainsKey(CardBareId)) throw new FileNotFoundException("Removable ECX-4 board source mesh is missing.");
                if (missing > 0) throw new FileNotFoundException(missing + " installable FBX/OBJ models failed import. Build cancelled.");

                string output = recipe.generatedAssetsFolder.TrimEnd('/');
                SparkAutoLevelSceneBuilder.EnsureFolder(output);
                string prefabPath = output + "/" + LayoutName + ".prefab";
                string scenePath = output + "/" + LayoutName + ".unity";
                if (AssetDatabase.LoadAssetAtPath<ScriptableObject>(output + "/Level_105.asset") != null)
                    throw new InvalidOperationException("This Level 105 prefab is registered for gameplay. Use Tools > Project Spark > Levels > Create Level 105 to refresh it.");
                if ((AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) != null ||
                     AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath) != null) &&
                    !EditorUtility.DisplayDialog("Regenerate Level 105?",
                        "This will overwrite ONLY the generated Level_105_Layout prefab and scene. " +
                        "Keep hand edits in a separate prefab. Previous levels are untouched.",
                        "Overwrite generated Level 105", "Cancel")) return;

                string blueprintPath = output + "/L105_SourceBlueprint.asset";
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
                Dictionary<string, Material> materials = SparkAutoL105PBRMaterials.Build(recipe, report);
                BuildLayoutScene(recipe, persistent, modelPaths, materials, prefabPath, scenePath, report);
                File.WriteAllText(SparkAutoLevelSourceImporter.ToAbsolute(output + "/L105_Layout_Validation.txt"),
                    report.Render() + "\nUniformly scaled ENX-13 fits inside 10.13 x 7.51 units; loose ECX-4 remains separate. " +
                    "Electrical pins/nets and gameplay service bindings still require authoritative design.\n");
                AssetDatabase.Refresh();
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                if (prefab != null) { Selection.activeObject = prefab; EditorGUIUtility.PingObject(prefab); }
                Debug.Log("[L105 Layout] Generated " + prefabPath + "\n" + report.Render());
            }
            catch (Exception e)
            {
                Debug.LogError("[L105 Layout] " + e);
                EditorUtility.DisplayDialog("Level 105 generation stopped", e.Message, "OK");
            }
            finally
            {
                if (transient != null) UnityEngine.Object.DestroyImmediate(transient);
            }
        }

        [MenuItem("Tools/Project Spark/Automatic Level Generator/105/2. Place Level 105 on SELECTED tabletop")]
        public static void PlaceOnSelectedTabletop()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            GameObject table = Selection.activeGameObject;
            if (table == null) { EditorUtility.DisplayDialog("Select the table", "Select your tabletop GameObject in the Hierarchy first.", "OK"); return; }
            string expected = "Assets/ProjectSpark/AutoGeneratedLevels/Level105/" + LayoutName + ".prefab";
            GameObject source = AssetDatabase.LoadAssetAtPath<GameObject>(expected);
            if (source == null) { EditorUtility.DisplayDialog("Missing layout", "Build FULL Level 105 layout first.", "OK"); return; }
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
                    "The board is uniformly scaled inside 10.13 x 7.51 world units, centered on the supplied " +
                    "15.79 x 8.54 tabletop. The anchor's Transform SCALE will be ignored.\n\n" +
                    "Cancel if the anchor's Y is not the tabletop surface height.",
                    "Place on anchor", "Cancel")) return;
            }

            GameObject placed = (GameObject)PrefabUtility.InstantiatePrefab(source);
            Undo.RegisterCreatedObjectUndo(placed, "Place Level 105 on tabletop");
            placed.name = LayoutName;
            placed.transform.position = placement;
            placed.transform.rotation = Quaternion.identity;
            // The layout scale was already calculated from imported PCB geometry at build time.
            // Never inherit an arbitrary Table_top Transform scale.
            placed.transform.localScale = Vector3.one;
            Selection.activeGameObject = placed;
            Debug.Log("[L105 Layout] Placed " + LayoutName + " at " + placement +
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
                Meta(root, "L105", "LEVEL_LAYOUT", "AUTHORING_GENERATED", "Gameplay starts empty. Authority integration required.");
                var fitConfig = root.AddComponent<SparkAutoL105WorkshopFit>();

                Transform fit = Child(root.transform, "L105_TabletopFittedMainboard_PreserveAspect");
                Transform modelSpace = Child(fit, "L105_NativeMeshModelSpace");
                Transform board = Child(modelSpace, "L105_BoardArtRoot");
                Transform cardDock = Child(modelSpace, "L105_ECX4_Unseated_Deck_Rotated90");
                Transform cardBoard = Child(cardDock, "L105_ECX4_BareCard_80x35mm");
                Transform cardSlots = Child(cardDock, "L105_ECX4_ComponentSlots_6_UNINSTALLED");
                Transform cardStructures = Child(cardDock, "L105_ECX4_StructuralReferences");
                Transform cardDocking = Child(modelSpace, "L105_ECX4_CardInsertionPivot_UNBOUND");
                Transform slots = Child(modelSpace, "L105_ENX13_ComponentSlots_94_UNINSTALLED");
                Transform pads = Child(modelSpace, "L105_PhysicalPads_SCHEMATIC_REQUIRED");
                Transform topology = Child(modelSpace, "L105_SolverTopology_SCHEMATIC_REQUIRED");
                Transform routes = Child(modelSpace, "L105_PlayerCopper_132_UNROUTED");
                Transform vias = Child(modelSpace, "L105_ViaSockets_38_UNASSIGNED");
                Transform layers = Child(modelSpace, "L105_CopperLayers_8_VisualOnly");
                Transform service = Child(modelSpace, "L105_ServiceableHardwareReferences_NotInstalled");
                Transform structural = Child(modelSpace, "L105_StructuralReferences");
                Transform faultVisual = Child(modelSpace, "L105_FaultOverlays_Disabled");
                Transform reference = Child(modelSpace, "L105_CompleteAssembledReference_NoProgress");
                Transform assembledFBX = Child(reference, "L105_CompleteAssembledMainboardModel");
                Transform modular = Child(reference, "L105_CompleteModularComposition");
                Transform systems = Child(root.transform, "L105_PCIeLinkAndExpansionSystems_UNBOUND");
                Transform safety = Child(root.transform, "L105_W133_IndependentSafety_UNBOUND");
                Transform objectives = Child(root.transform, "L105_Objectives_6_UNBOUND");
                Transform faults = Child(root.transform, "L105_RepairTargets_3_UNBOUND");
                Transform instruments = Child(root.transform, "L105_WorldSpaceVirtualInstruments_UNBOUND");
                Transform controls = Child(root.transform, "L105_WorldSpaceLevelControls_UNBOUND");

                GameObject empty = PlaceModel(models[recipe.bareBoardId], recipe.bareBoardId, board);
                SparkAutoL105PBRMaterials.Apply(empty, materials);
                Meta(empty, recipe.bareBoardId, "EMPTY_PCB", "VISIBLE_AT_START", "Primary PCB geometry; does not certify nets.", true);

                Bounds measured;
                if (!GetRendererBounds(empty, out measured))
                    throw new InvalidOperationException("Empty ENX-13 PCB has no MeshRenderer. Import Models_OBJ_Fallback and retry.");
                bool originalPlaneXY = measured.size.y > measured.size.z * 2.0f;
                if (originalPlaneXY) modelSpace.localRotation = Quaternion.Euler(-90f, 0f, 0f);
                if (!GetRendererBounds(empty, out measured) || measured.size.x < 0.00001f || measured.size.z < 0.00001f)
                    throw new InvalidOperationException("Empty PCB has invalid XZ bounds after axis correction.");
                float mmToNative = measured.size.x / recipe.widthMm;
                // Uniform scale preserves copper pad pitch and the endpoint's physical proportions.
                float uniformScale = Mathf.Min(TargetWidth / measured.size.x, TargetDepth / measured.size.z);
                float actualWidth = measured.size.x * uniformScale;
                float actualDepth = measured.size.z * uniformScale;
                fit.localScale = Vector3.one * uniformScale;
                fit.localPosition = new Vector3(0f, -measured.min.y * uniformScale, 0f);
                fitConfig.Configure(fit, TableWidth, TableDepth, actualWidth, actualDepth,
                    measured.size.x, measured.size.z, originalPlaneXY, mmToNative);

                string cardPath;
                if (!models.TryGetValue(CardBareId, out cardPath))
                    throw new FileNotFoundException("ECX-4 removable board mesh missing: " + CardBareId);
                GameObject card = PlaceModel(cardPath, CardBareId, cardBoard);
                SparkAutoL105PBRMaterials.Apply(card, materials);
                Meta(card, CardBareId, "REMOVABLE_ECX4_BARE_BOARD", "UNSEATED",
                    "Separate endpoint card; no connector contacts or gameplay installation certified.", true);
                // Rotate the loose 80x35 mm card so its 35 mm edge fits in the 2.83-unit side margin.
                // Both boards use the SAME physical scale; the loose card is NOT shrunk to fit.
                float cardShortEdge = 0.035f;
                float gap = 0.004f; // native meters (4 mm) before workshop scaling
                cardDock.localPosition = new Vector3(measured.size.x * 0.5f + cardShortEdge * 0.5f + gap, 0f, 0f);
                cardDock.localRotation = Quaternion.Euler(0f, 90f, 0f);
                Meta(cardDocking.gameObject, "ECX4_INSERTION", "INSERTION_BINDING", "UNBOUND",
                    "The card starts in the loose deck. Insert only through authoritative contact validation.",false,true);
                if (actualWidth + 0.0001f > TableWidth || actualDepth + 0.0001f > TableDepth)
                    throw new InvalidOperationException("Mainboard exceeds supplied tabletop bounds.");
                if ((measured.size.x * 0.5f + cardShortEdge + gap) * uniformScale > TableWidth * 0.5f)
                    throw new InvalidOperationException("ECX-4 loose deck exceeds supplied tabletop width.");

                int extra = 0;
                foreach (SparkAutoPart part in data.Parts)
                {
                    if (part.Installable || part.Id == recipe.bareBoardId || part.Id == CardBareId) continue;
                    string path;
                    if (!models.TryGetValue(part.Id, out path)) continue;
                    Transform destination = structural;
                    string type = (part.Type ?? "").ToUpperInvariant();
                    if (part.Id.Contains("ECX4") && !type.Contains("FAULT")) destination = cardStructures;
                    else if (type.Contains("LAYER") || type.Contains("PLANE")) destination = layers;
                    else if (type.Contains("HEATSINK") || type.Contains("SPREADER") ||
                             type.Contains("HEATPIPE") || type.Contains("THERMAL_PAD")) destination = service;
                    else if (type.Contains("FAULT") || type.Contains("OVERLAY")) destination = faultVisual;
                    GameObject mesh = PlaceModel(path, part.Id, destination);
                    SetPartPose(mesh.transform, part, mmToNative, originalPlaneXY);
                    SparkAutoL105PBRMaterials.Apply(mesh, materials);
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
                    var go = Child(CardPartIds.Contains(part.Id) ? cardSlots : slots, part.Id).gameObject;
                    SetPartPose(go.transform, part, mmToNative, originalPlaneXY);
                    BoxCollider collider = go.AddComponent<BoxCollider>();
                    Vector2 sizeMm = FootprintSize(part.Type);
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
                    SparkAutoL105PBRMaterials.Apply(visual, materials);
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
                Meta(pads.gameObject, "L105_PADS", "PADS", "SCHEMATIC_REQUIRED",
                    "The art source has no approved pad/terminal definitions: cannot create electrical contacts.", false, true);
                Meta(topology.gameObject, "L105_TOPOLOGY", "ELECTRICAL_GRAPH", "SCHEMATIC_REQUIRED",
                    "Do not synthesize arbitrary connections to meet route counts.", false, true);

                foreach (SparkAutoFault fault in data.Faults)
                    Meta(Child(faults, fault.Id + "_" + SafeName(fault.Title)).gameObject,
                        fault.Id, "REPAIR_SCENARIO", "NOT_STARTED",
                        "Mechanism=" + fault.Mechanism + "; Repair=" + fault.Repair, false, true);
                foreach (SparkAutoObjective objective in data.Objectives)
                    Meta(Child(objectives, "OBJ_" + objective.Id + "_" + SafeName(objective.Title)).gameObject,
                        objective.Id, "OBJECTIVE", "NOT_STARTED", objective.Criterion, false, true);
                foreach (string name in ExpansionSystems)
                    Meta(Child(systems, name).gameObject, name, "SYSTEM_BINDING", "UNBOUND",
                        "Bind actual Project Spark PCIe, expansion card, solver, repair and objective services.");
                foreach (string name in VirtualTools)
                    Meta(Child(instruments, name).gameObject, name, "WORLD_SPACE_TOOL", "UNBOUND",
                        "Bind existing virtual tool; no physical laboratory instrument is generated.");
                foreach (string name in Controls)
                    Meta(Child(controls, name).gameObject, name, "UI_CONTROL_BINDING", "UNBOUND",
                        "Use the existing World-Space Canvas UI/actions.");
                Meta(safety.gameObject, "W133", "INDEPENDENT_EXPANSION_SAFETY", "DISCONNECTED",
                    data.Safety.LogicalSource + " -> " + data.Safety.LogicalDestination +
                    "; physicalPins=" + data.Safety.PhysicalPinStatus + "; requires fresh data and explicit reset.", false, true);
                Meta(Child(safety, "W133_Jumper_Removal_And_Recovery").gameObject,
                    "W133", "REMOVABLE_JUMPER", "NOT_AUTHORIZED",
                    "Physical contacts and supervisor logic must be verified before enabling safety certification.", false, true);

                string assembledPath;
                bool hasAssembled = models.TryGetValue(recipe.assembledBoardModelName, out assembledPath);
                if (hasAssembled)
                {
                    GameObject mesh = PlaceModel(assembledPath, recipe.assembledBoardModelName, assembledFBX);
                    SparkAutoL105PBRMaterials.Apply(mesh, materials);
                    Meta(mesh, "L105_ENX13_ASSEMBLED", "COMPLETE_BOARD_MESH", "REFERENCE_ONLY",
                        "Never count this artwork as installed parts or completed objectives.", true);
                }
                else
                {
                    // Build an art-only assembled board from 136 imported modular models.
                    foreach (SparkAutoPart part in data.Parts)
                    {
                        if (part.Installable || part.Id == recipe.bareBoardId || part.Id == CardBareId) continue;
                        string path;
                        if (!models.TryGetValue(part.Id, out path)) continue;
                        GameObject mesh = PlaceModel(path, part.Id, modular);
                        SetPartPose(mesh.transform, part, mmToNative, originalPlaneXY);
                        SparkAutoL105PBRMaterials.Apply(mesh, materials);
                    }
                    GameObject boardCopy = PlaceModel(models[recipe.bareBoardId], recipe.bareBoardId, modular);
                    SparkAutoL105PBRMaterials.Apply(boardCopy, materials);
                    report.Warn("One-piece assembled mesh absent. Generated modular assembled art-only fallback.");
                }
                assembledFBX.gameObject.SetActive(hasAssembled);
                modular.gameObject.SetActive(!hasAssembled);
                reference.gameObject.SetActive(false);

                if (slots.childCount != 94 || cardSlots.childCount != 6)
                    throw new InvalidOperationException("ENX-13/ECX-4 placement ownership count mismatch.");
                if (placedSlots != PartTotal || routeCount != RouteTotal || viaCount != ViaTotal)
                    throw new InvalidOperationException("Generated layout counts do not match imported Level 105 manifest.");
                report.Info("Level_105_Layout: 94 ENX-13 + 6 ECX-4 component targets, 132 unapproved routes, 38 unapproved vias, " +
                    "3 faults, 6 objectives, 8 copper layer references, " + extra + " supplemental meshes.");
                report.Info("Model axis: " + (originalPlaneXY ? "XY rotated to XZ" : "native XZ") +
                    "; native import width=" + measured.size.x.ToString("F6", CultureInfo.InvariantCulture) +
                    "; native depth=" + measured.size.z.ToString("F6", CultureInfo.InvariantCulture));
                report.Info("WORKSHOP MAINBOARD: " + actualWidth.ToString("F3") + " x " + actualDepth.ToString("F3") + " world units, within target box " + TargetWidth + " x " + TargetDepth + "; table " + TableWidth +
                    " x " + TableDepth + " units; margins (centered) " + ((TableWidth - actualWidth) / 2f) +
                    " left/right, " + ((TableDepth - actualDepth) / 2f) + " front/back.");
                report.Warn("100 footprint pad mappings, routes W001-W132, vias V01-V38 and W133 physical pins " +
                    "are not approved; U12/U13 physical monitor/supervisor identities remain unspecified. Certification remains BLOCKED.");
                report.Warn("W133 logical U12/U13 labels require verified physical pin mappings; do not fabricate contacts.");
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
            go.AddComponent<SparkAutoL105LayoutRecord>().Configure(id, category, status, notes, isReference, schematic);
        }
    }
}
#endif
