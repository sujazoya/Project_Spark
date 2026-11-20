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
    /// <summary>Source-backed LEVEL_100 full physical layout; no false electrical completion.</summary>
    public static class SparkAutoL100FullLayoutBuilder
    {
        private const int LevelId = 100;
        private const string RecipeFile = "Level100_DNX8.recipe.json";
        private const string LayoutName = "Level_100_Layout";
        private const float TableW = 15.79f, TableD = 8.54f;
        private const float BoardW = 10.13f, BoardD = 6.99452381f; // 10.13 * 145 / 210, preserving DNX-8 proportions
        private static readonly string[] Instruments = {
            "VirtualMultimeter", "VirtualOscilloscope", "LogicAnalyzer", "NetTracer", "PCBMicroscope",
            "ContinuityProbe", "VirtualI2CScanner", "VirtualSPIAnalyzer", "UARTTerminal", "FirmwareProgrammer",
            "SolderDesolder", "ComponentReplacement", "ViaRepair", "FourLayerInspector", "DigitalBusDiagnosticMonitor"
        };
        private static readonly string[] Actions = {
            "INSPECT", "ASSEMBLE", "PLACE_COMPONENT", "ALIGN_PIN_1", "SOLDER_CONTACT", "ROUTE_L1", "ROUTE_L4",
            "PLACE_VIA", "VIEW_L1_L2_L3_L4", "CHECK_CONTINUITY", "POWER_ON", "POWER_OFF_ISOLATE",
            "SWD_CONNECT", "FLASH_APPROVED_FIRMWARE", "BOOT_TEST", "I2C_SCAN", "I2C_100KHZ", "I2C_400KHZ",
            "SPI_DISCOVER", "SPI_TRANSFER", "SPI_MEMORY_TEST", "UART_115200_8N1", "UART_BIDIRECTIONAL_TEST",
            "DIAGNOSE_F01_I2C_PULLUP", "REPAIR_F01_I2C_PULLUP", "DIAGNOSE_F02_SPI_CLOCK_VIA", "REPAIR_F02_SPI_CLOCK_VIA",
            "DIAGNOSE_F03_UART_CONFIG", "REPAIR_F03_UART_CONFIG", "W093_INSTALL",
            "W093_SUPERVISED_DISCONNECT", "W093_RECONNECT", "W093_EXPLICIT_SAFETY_RESET",
            "FRESH_I2C_SPI_UART_QUALIFICATION", "VIEW_OBJECTIVES", "SAVE", "LOAD"
        };
        private static readonly string[] Services = {
            "ExistingLevelGamePlayManager_UNBOUND", "ExistingSparkCircuitSystem_UNBOUND",
            "ExistingSparkTerminalGraph_UNBOUND", "ExistingShopShelfInventory_UNBOUND",
            "BoardPartPlacementAuthority_UNBOUND", "CopperRouteAuthority_UNBOUND", "ViaAuthority_UNBOUND",
            "ProtectedPower5V_UNBOUND", "Regulated3V3_UNBOUND", "ClockResetFirmware_UNBOUND",
            "I2C_OpenDrainBus_UNBOUND", "I2C_Peripherals_Three_UNBOUND", "I2C_RiseTime_UNBOUND",
            "SPI_Bus_UNBOUND", "SPI_Peripherals_Two_UNBOUND", "SPI_SCK_ViaFault_UNBOUND",
            "UART_115200_8N1_UNBOUND", "FirmwareSettings_UNBOUND", "CommsEventTelemetry_UNBOUND",
            "FaultF01_I2CPullup_UNBOUND", "FaultF02_OpenSPISCK_UNBOUND", "FaultF03_UARTBaud_UNBOUND",
            "IndependentW093SafetySupervisor_UNBOUND", "WorldSpaceVirtualTools_UNBOUND", "EvidenceAndSaveLoad_UNBOUND"
        };

        [MenuItem("Tools/Project Spark/Levels/Create Level 100")]
        public static void CreateLevel100() { Build(); }

        [MenuItem("Tools/Project Spark/Automatic Level Generator/100/1. Build FULL Level_100_Layout")]
        public static void Build()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) { Debug.LogError("[L100] Stop Play Mode first."); return; }
            SparkAutoLevelBlueprint transient = null;
            SparkAutoLevelBuildReport report = new SparkAutoLevelBuildReport();
            try
            {
                SparkAutoLevelRecipe recipe = SparkAutoLevelSourceImporter.LoadRecipe(RecipeFile);
                if (recipe.levelId != LevelId) throw new InvalidOperationException("Expected Level 100 recipe.");
                transient = SparkAutoLevelSourceImporter.Import(recipe, report);
                if (report.Errors > 0) throw new InvalidOperationException("Source validation:\n" + report.Render());
                if (transient.Parts.Count != 88 || transient.InstallableCount != 70 ||
                    transient.Routes.Count != 92 || transient.Vias.Count != 20 ||
                    transient.Objectives.Count != 6 || transient.Faults.Count != 3)
                    throw new InvalidDataException("Level 100 manifest counts differ from authoritative source.");
                var models = ResolveModels(recipe, transient, report);
                if (!models.ContainsKey(recipe.bareBoardId)) throw new FileNotFoundException("No renderable empty PCB OBJ/FBX: " + recipe.bareBoardId);
                foreach (SparkAutoPart part in transient.Parts)
                    if (part.Installable && !models.ContainsKey(part.Id))
                        throw new FileNotFoundException("Renderable source part missing: " + part.Id);
                string outDir = recipe.generatedAssetsFolder.TrimEnd('/');
                SparkAutoLevelSceneBuilder.EnsureFolder(outDir);
                string prefabPath = outDir + "/" + LayoutName + ".prefab";
                string scenePath = outDir + "/" + LayoutName + ".unity";
                bool alreadyGenerated = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) != null ||
                    AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath) != null;
                if (alreadyGenerated && !EditorUtility.DisplayDialog("Regenerate Level 100?",
                    "This overwrites GENERATED Level_100_Layout, its scene and 70 generated visual component prefabs. Keep customizations in variants.",
                    "Overwrite generated layout", "Cancel")) return;
                string bpPath = outDir + "/L100_SourceBlueprint.asset";
                SparkAutoLevelBlueprint persistent = AssetDatabase.LoadAssetAtPath<SparkAutoLevelBlueprint>(bpPath);
                if (persistent == null) { persistent = transient; transient = null; AssetDatabase.CreateAsset(persistent, bpPath); }
                else { EditorUtility.CopySerialized(transient, persistent); EditorUtility.SetDirty(persistent); }
                foreach (SparkAutoPart part in persistent.Parts)
                { string p; if (models.TryGetValue(part.Id, out p)) part.AssetPath = p; }
                EditorUtility.SetDirty(persistent);
                AssetDatabase.SaveAssets();
                var materials = SparkAutoL100PBRMaterials.Build(recipe, report);
                BuildScene(recipe, persistent, models, materials, prefabPath, scenePath, report);
                File.WriteAllText(SparkAutoLevelSourceImporter.ToAbsolute(outDir + "/L100_Layout_Validation.txt"),
                    report.Render() + "\nART READY, ELECTRICAL AUTHORITY BLOCKED: 92 conceptual routes, 20 unassigned via nets, W093 pins and unverified footprints.\n");
                AssetDatabase.Refresh();
                GameObject made = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                if (made != null) { Selection.activeObject = made; EditorGUIUtility.PingObject(made); }
                Debug.Log("[L100] Full visual layout generated: " + prefabPath + "\n" + report.Render());
            }
            catch (Exception e)
            {
                Debug.LogError("[L100] " + e);
                EditorUtility.DisplayDialog("Level 100 generation stopped", e.Message, "OK");
            }
            finally { if (transient != null) UnityEngine.Object.DestroyImmediate(transient); }
        }

        [MenuItem("Tools/Project Spark/Automatic Level Generator/100/2. Place Level 100 on SELECTED tabletop")]
        public static void PlaceOnTable()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            GameObject selected = Selection.activeGameObject;
            if (selected == null) { EditorUtility.DisplayDialog("Select tabletop", "Choose Table_top in the Hierarchy.", "OK"); return; }
            var ids = AssetDatabase.FindAssets("Level_100_Layout t:Prefab");
            string assetPath = "Assets/ProjectSpark/AutoGeneratedLevels/Level100/" + LayoutName + ".prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (prefab == null)
            {
                foreach (string id in ids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(id);
                    if (Path.GetFileName(path).Equals(LayoutName + ".prefab", StringComparison.OrdinalIgnoreCase))
                    { prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path); break; }
                }
            }
            if (prefab == null) { EditorUtility.DisplayDialog("Missing layout", "Build FULL Level 100 layout first.", "OK"); return; }
            Bounds tabletopBounds;
            bool found = TryGetTableBounds(selected, out tabletopBounds);
            Vector3 pos;
            if (found)
            {
                if (tabletopBounds.size.x < BoardW || tabletopBounds.size.z < BoardD)
                { EditorUtility.DisplayDialog("Table too small", "Measured tabletop width/depth cannot fit a 10.13 x 6.995-unit board.", "OK"); return; }
                if (Mathf.Abs(tabletopBounds.size.x - TableW) > 0.75f || Mathf.Abs(tabletopBounds.size.z - TableD) > 0.75f)
                    if (!EditorUtility.DisplayDialog("Nonstandard tabletop bounds",
                        "Expected 15.79 x 8.54; observed " + tabletopBounds.size.x.ToString("F2") + " x " + tabletopBounds.size.z.ToString("F2") + ". Place anyway?", "Place", "Cancel")) return;
                pos = new Vector3(tabletopBounds.center.x, tabletopBounds.max.y, tabletopBounds.center.z);
            }
            else
            {
                pos = selected.transform.position;
                if (!EditorUtility.DisplayDialog("Transform-only Table_top",
                    "Selected table has no Renderer/Collider. Treat its world position as the tabletop SURFACE CENTER?\n" +
                    "Position: " + pos + "\nBoard size: 10.13 x 6.995. Table: 15.79 x 8.54.\n" +
                    "The selected Transform scale will NOT be copied. Cancel if Y is not the top surface.",
                    "Place board on anchor", "Cancel")) return;
            }
            // Re-running placement must not silently stack multiple complete layouts.
            foreach (GameObject existing in SceneManager.GetActiveScene().GetRootGameObjects())
            {
                SparkAutoGeneratedMarker marker = existing.GetComponent<SparkAutoGeneratedMarker>();
                if (marker != null && marker.LevelId == LevelId && !marker.ReferenceOnly)
                {
                    if (!EditorUtility.DisplayDialog("Existing Level 100 in scene",
                        "A generated Level 100 layout is already present. Replace only this scene instance?",
                        "Replace scene instance", "Cancel")) return;
                    Undo.DestroyObjectImmediate(existing);
                    break;
                }
            }
            var placed = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            Undo.RegisterCreatedObjectUndo(placed, "Place Level 100 board on tabletop");
            placed.name = LayoutName;
            placed.transform.position = pos;
            placed.transform.rotation = Quaternion.identity;
            placed.transform.localScale = Vector3.one;
            Selection.activeGameObject = placed;
            Debug.Log("[L100] Placed source-backed board: " + pos + ", 10.13 x 6.995 units. Side clearances 2.83; front/rear 0.773 units for full table.");
        }

        private static bool TryGetTableBounds(GameObject g, out Bounds bounds)
        {
            Collider c = g.GetComponent<Collider>();
            if (c != null && c.enabled) { bounds = c.bounds; return true; }
            Renderer r = g.GetComponent<Renderer>();
            if (r != null && r.enabled) { bounds = r.bounds; return true; }
            foreach (Collider cc in g.GetComponentsInChildren<Collider>())
                if (cc.enabled) { bounds = cc.bounds; return true; }
            foreach (Renderer rr in g.GetComponentsInChildren<Renderer>())
                if (rr.enabled) { bounds = rr.bounds; return true; }
            bounds = default(Bounds); return false;
        }
        private static Dictionary<string, string> ResolveModels(SparkAutoLevelRecipe r,
            SparkAutoLevelBlueprint bp, SparkAutoLevelBuildReport report)
        {
            var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            string root = r.sourceAssetsFolder.TrimEnd('/');
            foreach (SparkAutoPart p in bp.Parts)
            {
                string obj = root + "/Models_OBJ_Fallback/" + p.Id + ".obj";
                string fbx = root + "/Models_FBX/" + p.Id + ".fbx";
                if (HasMesh(obj)) map[p.Id] = obj;
                else if (HasMesh(fbx)) { map[p.Id] = fbx; report.Warn("Fallback OBJ missing; using FBX for " + p.Id + ". Manually confirm axes and dimensions before gameplay."); }
                else report.Warn("No measurable mesh for source model " + p.Id);
            }
            string reference = r.assembledBoardModelName;
            string aobj = root + "/Models_OBJ_Fallback/" + reference + ".obj";
            string afbx = root + "/Models_FBX/" + reference + ".fbx";
            if (HasMesh(aobj)) map[reference] = aobj;
            else if (HasMesh(afbx)) map[reference] = afbx;
            else report.Warn("No assembled model imported; disabled modular reference will be composed.");
            return map;
        }
        private static bool HasMesh(string assetPath)
        {
            GameObject obj = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (obj == null) return false;
            foreach (MeshFilter filter in obj.GetComponentsInChildren<MeshFilter>(true))
                if (filter.sharedMesh != null && filter.sharedMesh.vertexCount > 0) return true;
            foreach (SkinnedMeshRenderer skin in obj.GetComponentsInChildren<SkinnedMeshRenderer>(true))
                if (skin.sharedMesh != null && skin.sharedMesh.vertexCount > 0) return true;
            return false;
        }
        private static GameObject MeshInstance(string path, string name, Transform parent)
        {
            GameObject model = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (model == null) throw new FileNotFoundException("Model import failed: " + path);
            var go = (GameObject)PrefabUtility.InstantiatePrefab(model);
            if (go == null) throw new IOException("Could not instantiate model: " + path);
            go.name = name;
            go.transform.SetParent(parent, false);
            return go;
        }
        private static bool TryBounds(GameObject go, out Bounds result)
        {
            result = default(Bounds); bool found = false;
            foreach (Renderer r in go.GetComponentsInChildren<Renderer>(true))
            {
                if (!found) { result = r.bounds; found = true; }
                else result.Encapsulate(r.bounds);
            }
            return found;
        }
        private static Transform Child(Transform parent, string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            return go.transform;
        }
        private static Transform Group(Transform p, string name)
        {
            Transform found = p.Find(name);
            return found == null ? Child(p, name) : found;
        }
        private static string Safe(string s)
        {
            if (string.IsNullOrEmpty(s)) return "UNKNOWN";
            return s.Replace(' ', '_').Replace('\\', '_').Replace('/', '_').Replace(':', '_');
        }
        private static void Record(GameObject go, string id, string kind, string state, string notes,
            bool isArt = false, bool requiresSchematic = false)
        {
            go.AddComponent<SparkAutoL100LayoutRecord>().Configure(id, kind, state, notes, isArt, requiresSchematic);
        }
        private static void PlacePose(Transform transform, SparkAutoPart part, float mmToNative)
        {
            transform.localPosition = new Vector3(part.Xmm, part.Zmm, part.Ymm) * mmToNative;
            if (Mathf.Abs(part.OrientationDegrees) > 0.01f)
                transform.localRotation *= Quaternion.Euler(0f, part.OrientationDegrees, 0f);
        }
        private static Dictionary<string, Vector2> ReadHitAreas(SparkAutoLevelRecipe recipe)
        {
            string source = SparkAutoLevelSourceImporter.ToAbsolute(
                recipe.sourceAssetsFolder.TrimEnd('/') + "/Data/L100_VisualHitTargets_70.csv");
            if (!File.Exists(source)) throw new FileNotFoundException("Missing real mesh-bounds hit area CSV", source);
            var dict = new Dictionary<string, Vector2>(StringComparer.OrdinalIgnoreCase);
            string[] rows = File.ReadAllLines(source);
            for (int i = 1; i < rows.Length; ++i)
            {
                string[] cols = rows[i].Split(',');
                if (cols.Length < 7) throw new FormatException("Visual collider record invalid at line " + (i + 1));
                float width = float.Parse(cols[5], CultureInfo.InvariantCulture);
                float depth = float.Parse(cols[6], CultureInfo.InvariantCulture);
                if (width <= 0 || depth <= 0 || dict.ContainsKey(cols[0]))
                    throw new InvalidDataException("Invalid visual hit proxy size for " + cols[0]);
                dict.Add(cols[0], new Vector2(width, depth));
            }
            return dict;
        }
        private static void BuildScene(SparkAutoLevelRecipe recipe, SparkAutoLevelBlueprint data,
            Dictionary<string, string> meshes, Dictionary<string, Material> mats,
            string prefabPath, string scenePath, SparkAutoLevelBuildReport report)
        {
            Scene oldScene = SceneManager.GetActiveScene();
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            try
            {
                SceneManager.SetActiveScene(scene);
                var root = new GameObject(LayoutName);
                root.AddComponent<SparkAutoGeneratedMarker>().Configure(LevelId, false);
                SparkAutoLevelRuntime runtime = root.AddComponent<SparkAutoLevelRuntime>();
                runtime.Configure(data, false);
                Record(root, "L100", "FULL_LEVEL_LAYOUT", "UNCOMMISSIONED", "Game visuals assembled but gameplay requires approved circuit adapters.");
                SparkAutoL100WorkshopFit fitting = root.AddComponent<SparkAutoL100WorkshopFit>();
                Transform visual = Child(root.transform, "L100_WorkshopVisualFit");
                Transform native = Child(visual, "L100_NativeMetres_YUp_GLB_To_OBJ");
                Transform board = Child(native, "BoardArt_EmptyDNX8");
                Transform supports = Child(native, "FactorySupportingArt_DisabledByDefault");
                Transform layers = Child(native, "FourCopperLayers_References_ArtOnly");
                Transform faultArt = Child(native, "FaultVisuals_Disabled");
                Transform slots = Child(native, "ComponentSlots_70_UNINSTALLED");
                Transform routeRoot = Child(native, "PlayerCopper_92_Unrouted");
                Transform viaRoot = Child(native, "ViaSockets_20_Unplated");
                Transform physicalPads = Child(native, "PhysicalPads_UNASSIGNED");
                Transform topology = Child(native, "SolverTopology_UNVERIFIED");
                Transform complete = Child(native, "CompleteAssembledBoard_ReferenceOnly_Disabled");
                Transform completeMesh = Child(complete, "OriginalAssembledVisualMesh");
                Transform completeModular = Child(complete, "ModularReferenceFallback");
                Transform systems = Child(root.transform, "DigitalNexusProtocolAndRepairSystems_UNBOUND");
                Transform safety = Child(root.transform, "W093_TrustedCommunicationHealth_UNBOUND");
                Transform faults = Child(root.transform, "Faults_3_UNBOUND");
                Transform objectives = Child(root.transform, "Objectives_6_UNBOUND");
                Transform instruments = Child(root.transform, "ExistingVirtualToolBindings_UNBOUND");
                Transform controls = Child(root.transform, "WorldSpaceLevelControlBindings_UNBOUND");

                GameObject pcb = MeshInstance(meshes[recipe.bareBoardId], recipe.bareBoardId, board);
                SparkAutoL100PBRMaterials.Apply(pcb, mats);
                Record(pcb, recipe.bareBoardId, "EMPTY_BOARD", "FACTORY_ART_ONLY", "Factory-only copper visual; no player routes or vias electrically certified.", true);
                Bounds bounds;
                if (!TryBounds(pcb, out bounds) || bounds.size.x < 0.00001f || bounds.size.z < 0.00001f)
                    throw new InvalidOperationException("Imported empty PCB has no measurable XZ Renderer bounds; install Models_OBJ_Fallback.");
                // The DNX-8 source PCB is 210 x 145 mm. Fit the PCB, not any decorative connector overhang.
                // Do not scale to the entire mesh X bounds; that would shrink the actual PCB.
                // Estimate metres-per-mm from the actual 145 mm PCB depth.
                float mmToNative = bounds.size.z / recipe.heightMm;
                float nativeBoardWidth = recipe.widthMm * mmToNative;
                float nativeBoardDepth = recipe.heightMm * mmToNative;
                if (mmToNative <= 0f || (float.IsNaN(mmToNative) || float.IsInfinity(mmToNative)) ||
                    bounds.size.x < nativeBoardWidth * 0.95f ||
                    bounds.size.x > nativeBoardWidth * 1.14f)
                    throw new InvalidOperationException("PCB import axis/size unexpected. Expected " +
                        "210x145mm FR4 in XZ, with up to 14% decorative X overhang. " + bounds.size);
                float scale = Mathf.Min(BoardW / nativeBoardWidth, BoardD / nativeBoardDepth);
                float fittedW = scale * nativeBoardWidth, fittedD = scale * nativeBoardDepth;
                if (fittedW > TableW || fittedD > TableD)
                    throw new InvalidOperationException("Fitted PCB exceeds tabletop.");
                visual.localScale = Vector3.one * scale;
                visual.localPosition = new Vector3(0f, -bounds.min.y * scale, 0f);
                fitting.Configure(visual, new Vector2(TableW, TableD), new Vector2(fittedW, fittedD),
                    new Vector2(bounds.size.x, bounds.size.z), mmToNative);
                report.Info("PCB FR-4 footprint fitted uniformly: " + fittedW.ToString("F4") + " x " + fittedD.ToString("F4") +
                    "; imported visual mesh bounds " + bounds.size + "; scale=" + scale.ToString("F5") +
                    "; actual visual overhang X=" + (bounds.size.x * scale).ToString("F3") + "m.");

                foreach (SparkAutoPart part in data.Parts)
                {
                    if (part.Installable || part.Id == recipe.bareBoardId) continue;
                    string path; if (!meshes.TryGetValue(part.Id, out path)) continue;
                    string type = (part.Type ?? string.Empty).ToUpperInvariant();
                    Transform destination = type.Contains("LAYER") ? layers :
                        (type.Contains("FAULT") ? faultArt : supports);
                    GameObject mesh = MeshInstance(path, part.Id, destination);
                    SparkAutoL100PBRMaterials.Apply(mesh, mats);
                    Record(mesh, part.Id, "FACTORY_SUPPORTING_ART", "NOT_GAMEPLAY", part.Notes, true, true);
                }
                supports.gameObject.SetActive(false);
                layers.gameObject.SetActive(false);
                faultArt.gameObject.SetActive(false);

                Dictionary<string, Vector2> hitAreas = ReadHitAreas(recipe);
                string styledPartsFolder = recipe.generatedAssetsFolder.TrimEnd('/') + "/ComponentVisualPrefabs";
                SparkAutoLevelSceneBuilder.EnsureFolder(styledPartsFolder);
                int created = 0;
                foreach (SparkAutoPart part in data.Parts)
                {
                    if (!part.Installable) continue;
                    Vector2 area;
                    if (!hitAreas.TryGetValue(part.Id, out area)) throw new InvalidDataException("Missing source mesh hit dimensions for " + part.Id);
                    GameObject slot = Child(slots, part.Id).gameObject;
                    PlacePose(slot.transform, part, mmToNative);
                    var col = slot.AddComponent<BoxCollider>();
                    col.size = new Vector3(area.x * mmToNative, 3.5f * mmToNative, area.y * mmToNative);
                    col.center = new Vector3(0f, 1.75f * mmToNative, 0f);
                    GameObject visualPart = MeshInstance(meshes[part.Id], part.Id, completeModular);
                    PlacePose(visualPart.transform, part, mmToNative);
                    SparkAutoL100PBRMaterials.Apply(visualPart, mats);
                    // Persist native URP material overrides in a unique source-backed visual prefab.
                    // Installed objects never inherit an unremapped default-MTL material by accident.
                    string styledPath = styledPartsFolder + "/" + part.Id + "_PBR.prefab";
                    GameObject styledPrefab = PrefabUtility.SaveAsPrefabAsset(visualPart, styledPath);
                    if (styledPrefab == null) throw new IOException("Cannot save styled part prefab: " + styledPath);
                    var target = slot.AddComponent<SparkAutoPlacementTarget>();
                    target.Configure(part.Id, part.Type, runtime, false, styledPrefab);
                    Record(slot, part.Id, "PROVISIONAL_COMPONENT_FOOTPRINT", "UNINSTALLED",
                        "Source GLB-based visual selection bounds; not an approved physical footprint/contact pin map.", false, true);
                    created++;
                }
                if (created != 70) throw new InvalidDataException("Expected 70 installable targets, generated " + created);
                foreach (SparkAutoRoute r in data.Routes)
                {
                    Transform group = Group(routeRoot, Safe(r.Subsystem));
                    Record(Child(group, r.Id).gameObject, r.Id, "UNVERIFIED_COPPER_TASK", "UNROUTED",
                        "Original unverified source design: " + r.Subsystem + "; net=" + r.NetId + "; source=" + r.SourcePad + "; target=" + r.DestinationPad +
                        "; requires approved schematic and layer before electrical routing.", false, true);
                }
                foreach (SparkAutoVia v in data.Vias)
                {
                    GameObject socket = Child(viaRoot, v.Id).gameObject;
                    float x, z;
                    if (float.TryParse(v.Xmm, NumberStyles.Float, CultureInfo.InvariantCulture, out x) &&
                        float.TryParse(v.Ymm, NumberStyles.Float, CultureInfo.InvariantCulture, out z))
                        socket.transform.localPosition = new Vector3(x * mmToNative, 0.9f * mmToNative, z * mmToNative);
                    Record(socket, v.Id, "UNVERIFIED_VIA_TASK", "MISSING",
                        "Source layer placeholder " + v.FromLayer + "->" + v.ToLayer + "; net UNASSIGNED; concept marker only.", false, true);
                }
                for (int l = 1; l <= 4; l++)
                    Record(Child(layers, "L" + l + "_VisualOnly").gameObject,
                        "L" + l, "LAYER_ART", "UNVERIFIED", "Visual references not conductive topology.", true, true);
                Record(physicalPads.gameObject, "PADS", "PIN_GRAPH", "SCHEMATIC_REQUIRED",
                    "Do not infer physical pin IDs from nearby factory pad geometry.", false, true);
                Record(topology.gameObject, "TOPOLOGY", "ELECTRICAL_GRAPH", "UNBOUND",
                    "Missing approved 92 route endpoints, 20 via nets, and verified part pads.", false, true);
                foreach (SparkAutoFault f in data.Faults)
                    Record(Child(faults, f.Id + "_" + Safe(f.Title)).gameObject,
                        f.Id, "REPAIR_FAULT", "NOT_ACTIVE", "Cause=" + f.Mechanism + "; repair=" + f.Repair, false, true);
                foreach (SparkAutoObjective o in data.Objectives)
                    Record(Child(objectives, "OBJ_" + o.Id + "_" + Safe(o.Title)).gameObject,
                        o.Id, "OBJECTIVE", "NOT_STARTED", o.Criterion, false, true);
                foreach (string s in Services)
                    Record(Child(systems, s).gameObject, s, "RUNTIME_SERVICE", "UNBOUND", "Bind real Project Spark gameplay/solver API.");
                foreach (string s in Instruments)
                    Record(Child(instruments, s).gameObject, s, "VIRTUAL_TOOL", "UNBOUND", "Bind existing world-space tool, no physical FBX.");
                foreach (string s in Actions)
                    Record(Child(controls, s).gameObject, s, "WORLD_SPACE_UI", "UNBOUND", "Bind existing Project Spark UI action.");
                Record(safety.gameObject, "W093", "INDEPENDENT_COMMUNICATION_HEALTH_HEARTBEAT", "DISCONNECTED",
                    data.Safety.LogicalSource + " -> " + data.Safety.LogicalDestination +
                    "; physical pins unassigned; W093 carries health-valid digital feedback, not power or I2C/SPI/UART data.", false, true);
                Record(Child(safety, "W093_AuthoritativeContactBindings_MISSING").gameObject,
                    "W093_CONTACTS", "SCHEMATIC_REQUIRED", "UNBOUND",
                    "Qualified changing health pulses, safety inhibit/latch, reconnect, explicit reset and fresh I2C/SPI/UART requalification.", false, true);
                string assembledPath;
                if (meshes.TryGetValue(recipe.assembledBoardModelName, out assembledPath))
                {
                    GameObject art = MeshInstance(assembledPath, recipe.assembledBoardModelName, completeMesh);
                    SparkAutoL100PBRMaterials.Apply(art, mats);
                    Record(art, recipe.assembledBoardModelName, "COMPLETE_BOARD", "ART_ONLY_NO_PROGRESS",
                        "Art reference cannot grant placement, routing or objectives; no colliders, tool or solver authority.", true);
                    completeModular.gameObject.SetActive(false);
                }
                else
                {
                    GameObject artPcb = MeshInstance(meshes[recipe.bareBoardId], "BarePCB_reference", completeModular);
                    SparkAutoL100PBRMaterials.Apply(artPcb, mats);
                    completeMesh.gameObject.SetActive(false);
                    report.Warn("Assembled mesh missing: using modular reference composition.");
                }
                complete.gameObject.SetActive(false);
                report.Info("Generated 70 provisional placement targets, 92 conceptual copper tasks, 20 via tasks, 4 layer reference roots, 3 fault records, 6 objectives and inactive assembled reference.");
                report.Warn("ELECTRICAL AUTHORITY BLOCKED: 92 routes, 20 via nets, W093 safety pins and physical footprints remain unverified.");
                report.Info("Table 15.79 x 8.54; centered board " + fittedW.ToString("F3") + " x " + fittedD.ToString("F3") +
                    "; side/depth margins " + ((TableW - fittedW) / 2).ToString("F3") + "/" + ((TableD - fittedD) / 2).ToString("F3"));
                bool success;
                PrefabUtility.SaveAsPrefabAsset(root, prefabPath, out success);
                if (!success) throw new IOException("Cannot save Level 100 prefab: " + prefabPath);
                if (!EditorSceneManager.SaveScene(scene, scenePath)) throw new IOException("Cannot save Level 100 scene: " + scenePath);
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
                if (oldScene.IsValid() && oldScene.isLoaded) SceneManager.SetActiveScene(oldScene);
            }
        }
    }
}
#endif
