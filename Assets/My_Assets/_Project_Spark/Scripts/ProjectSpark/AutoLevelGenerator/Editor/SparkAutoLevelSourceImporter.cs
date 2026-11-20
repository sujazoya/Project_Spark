#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.AutoLevelGenerator.Editor
{
    [Serializable]
    public sealed class SparkAutoLevelRecipe
    {
        public int levelId;
        public string boardId;
        public string title;
        public float widthMm;
        public float heightMm;
        public int copperLayers;
        public float millimetersToUnityUnits = 0.001f;
        public float visualScaleMultiplier = 1.0f;
        public int expectedModels;
        public int expectedInstallable;
        public int expectedRoutes;
        public int expectedVias;
        public int expectedObjectives;
        public string archiveRoot;
        public string sourceAssetsFolder;
        public string generatedAssetsFolder;
        public string modelsCsv;
        public string componentsCsv;
        public string routesCsv;
        public string viasCsv;
        public string objectivesJson;
        public string faultsJson;
        public string safetyJson;
        public string bareBoardId;
        public string assembledBoardModelName;
    }

    public sealed class SparkAutoLevelBuildReport
    {
        private readonly List<string> messages = new List<string>();
        public int Errors { get; private set; }
        public int Warnings { get; private set; }
        public int Infos { get; private set; }
        public IReadOnlyList<string> Messages => messages;
        public void Error(string message) { Errors++; messages.Add("ERROR | " + message); }
        public void Warn(string message) { Warnings++; messages.Add("WARN  | " + message); }
        public void Info(string message) { Infos++; messages.Add("INFO  | " + message); }
        public string Render()
        {
            var result = new StringBuilder();
            result.AppendLine("PROJECT SPARK — AUTOMATIC LEVEL GENERATION VALIDATION");
            result.AppendLine("Errors: " + Errors + " | Warnings: " + Warnings + " | Info: " + Infos);
            result.AppendLine("STATUS: " + (Errors == 0 ? "ART BUILD VALIDATED (GAMEPLAY AUTHORITY STILL REQUIRED)" : "BLOCKED OR PARTIAL"));
            result.AppendLine();
            for (int i = 0; i < messages.Count; i++) result.AppendLine(messages[i]);
            return result.ToString();
        }
    }

    internal sealed class SparkAutoCsvTable
    {
        private readonly Dictionary<string, int> headers = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        private readonly List<string[]> rows = new List<string[]>();
        public int RowCount => rows.Count;
        public string[] Row(int row) => rows[row];
        public string Value(string[] row, params string[] aliases)
        {
            for (int i = 0; i < aliases.Length; i++)
            {
                int column;
                if (headers.TryGetValue(aliases[i], out column) && column < row.Length) return row[column].Trim();
            }
            return string.Empty;
        }
        public static SparkAutoCsvTable Parse(string source)
        {
            var table = new SparkAutoCsvTable();
            var records = new List<List<string>>();
            var currentRecord = new List<string>();
            var field = new StringBuilder();
            bool quoted = false;
            for (int i = 0; i < source.Length; i++)
            {
                char c = source[i];
                if (quoted)
                {
                    if (c == '"' && i + 1 < source.Length && source[i + 1] == '"') { field.Append('"'); i++; }
                    else if (c == '"') quoted = false;
                    else field.Append(c);
                }
                else if (c == '"') quoted = true;
                else if (c == ',') { currentRecord.Add(field.ToString()); field.Length = 0; }
                else if (c == '\r' || c == '\n')
                {
                    if (c == '\r' && i + 1 < source.Length && source[i + 1] == '\n') i++;
                    currentRecord.Add(field.ToString()); field.Length = 0;
                    if (currentRecord.Count > 1 || currentRecord[0].Length > 0) records.Add(currentRecord);
                    currentRecord = new List<string>();
                }
                else field.Append(c);
            }
            if (quoted) throw new FormatException("CSV contains an unterminated quote");
            if (field.Length > 0 || currentRecord.Count > 0)
            {
                currentRecord.Add(field.ToString()); records.Add(currentRecord);
            }
            if (records.Count == 0) throw new FormatException("CSV is empty");
            for (int i = 0; i < records[0].Count; i++) table.headers[records[0][i].Trim().TrimStart('\uFEFF')] = i;
            for (int i = 1; i < records.Count; i++) table.rows.Add(records[i].ToArray());
            return table;
        }
    }

    [Serializable] internal sealed class SparkSourceObjective { public string id; public string title; public string name; public string criterion; public string requirements; }
    [Serializable] internal sealed class SparkSourceObjectiveArray { public SparkSourceObjective[] items; }
    [Serializable]
    internal sealed class SparkSourceFault
    {
        public string id; public string name; public string model; public string symptoms;
        public string repair; public string mechanism; public string[] evidence;
    }
    [Serializable] internal sealed class SparkSourceFaultArray { public SparkSourceFault[] items; }
    [Serializable]
    internal sealed class SparkSourceSafety
    {
        public string jumper; public string source; public string destination; public string logical_source;
        public string logical_destination; public string physical_pin_mapping; public string physical_pins;
        public string netlist_status; public string initial_state;
        public string id; public string @from; public string to; public string electrical_pins;
    }

    public static class SparkAutoLevelSourceImporter
    {
        public static SparkAutoLevelRecipe LoadRecipe(string unityRecipePath)
        {
            string resolved = SparkAutoRecipeRegistry.Resolve(unityRecipePath);
            if (string.IsNullOrEmpty(resolved))
                throw new FileNotFoundException("Recipe not found. Open the Generator and select a discovered recipe, " +
                    "or use Create Recipe From ZIP. Requested: " + unityRecipePath);
            string absolute = ToAbsolute(resolved);
            SparkAutoLevelRecipe recipe = JsonUtility.FromJson<SparkAutoLevelRecipe>(File.ReadAllText(absolute));
            if (recipe == null || recipe.levelId <= 0) throw new FormatException("Invalid recipe: levelId required");
            TryDiscoverMovedSourceFolder(recipe);
            return recipe;
        }


        private static void TryDiscoverMovedSourceFolder(SparkAutoLevelRecipe recipe)
        {
            if (string.IsNullOrWhiteSpace(recipe.modelsCsv)) return;
            if (!string.IsNullOrWhiteSpace(recipe.sourceAssetsFolder) &&
                recipe.sourceAssetsFolder.StartsWith("Assets/", StringComparison.Ordinal))
            {
                string path = recipe.sourceAssetsFolder.TrimEnd('/') + "/" + recipe.modelsCsv;
                if (File.Exists(ToAbsolute(path))) return;
            }
            // Fallback for users whose asset package was extracted inside My_Assets/_Project_Spark.
            // Only act on a unique exact manifest filename; ambiguity requires an explicit path.
            string filename = Path.GetFileName(recipe.modelsCsv.Replace('\\', '/'));
            if (string.IsNullOrWhiteSpace(filename)) return;
            string[] matches = Directory.GetFiles(Application.dataPath, filename, SearchOption.AllDirectories);
            if (matches.Length != 1) return;
            string absolute = matches[0].Replace('\\', '/');
            string suffix = "/" + recipe.modelsCsv.Replace('\\', '/').TrimStart('/');
            if (!absolute.EndsWith(suffix, StringComparison.OrdinalIgnoreCase)) return;
            string root = absolute.Substring(0, absolute.Length - suffix.Length);
            string assets = Application.dataPath.Replace('\\', '/');
            if (root.StartsWith(assets + "/", StringComparison.OrdinalIgnoreCase))
            {
                recipe.sourceAssetsFolder = "Assets" + root.Substring(assets.Length);
                Debug.Log("[Spark AutoLevel] Detected relocated source asset folder: " + recipe.sourceAssetsFolder);
            }
        }

        public static string ToAbsolute(string unityPath)
        {
            if (string.IsNullOrEmpty(unityPath) || !(unityPath == "Assets" || unityPath.StartsWith("Assets/", StringComparison.Ordinal)))
                throw new ArgumentException("Path must be inside Unity Assets/: " + unityPath);
            string projectDir = Directory.GetParent(Application.dataPath).FullName;
            string combined = Path.GetFullPath(Path.Combine(projectDir, unityPath.Replace('/', Path.DirectorySeparatorChar)));
            string assetsRoot = Path.GetFullPath(Application.dataPath) + Path.DirectorySeparatorChar;
            if (!combined.StartsWith(assetsRoot, StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(combined, Application.dataPath, StringComparison.OrdinalIgnoreCase))
                throw new IOException("Asset path escapes Assets/: " + unityPath);
            return combined;
        }

        public static SparkAutoLevelBlueprint Import(SparkAutoLevelRecipe recipe, SparkAutoLevelBuildReport report)
        {
            ValidateRecipe(recipe, report);
            var parts = new List<SparkAutoPart>();
            var routes = new List<SparkAutoRoute>();
            var vias = new List<SparkAutoVia>();
            var objectives = new List<SparkAutoObjective>();
            var faults = new List<SparkAutoFault>();
            var safety = new SparkAutoSafety();

            var partCsv = ReadCsv(recipe, recipe.modelsCsv, report);
            if (partCsv != null) ParseParts(partCsv, parts, report);
            var routeCsv = ReadCsv(recipe, recipe.routesCsv, report);
            if (routeCsv != null) ParseRoutes(routeCsv, routes, report);
            var viaCsv = ReadCsv(recipe, recipe.viasCsv, report);
            if (viaCsv != null) ParseVias(viaCsv, vias, report);

            ReadObjectives(recipe, objectives, report);
            ReadFaults(recipe, faults, report);
            ReadSafety(recipe, safety, report);
            CrossCheckManifest(recipe, parts, report);

            var blueprint = ScriptableObject.CreateInstance<SparkAutoLevelBlueprint>();
            blueprint.Populate(recipe.levelId, recipe.boardId, recipe.title, recipe.widthMm, recipe.heightMm,
                recipe.copperLayers, recipe.millimetersToUnityUnits, parts, routes, vias, objectives, faults, safety);
            SparkAutoLevelValidator.Validate(recipe, blueprint, report);
            return blueprint;
        }

        private static void ValidateRecipe(SparkAutoLevelRecipe r, SparkAutoLevelBuildReport report)
        {
            if (r.widthMm <= 0 || r.heightMm <= 0 || r.copperLayers < 1 || r.millimetersToUnityUnits <= 0)
                report.Error("Recipe has invalid board dimensions, layer count or coordinate scale");
            if (r.sourceAssetsFolder == null || !r.sourceAssetsFolder.StartsWith("Assets/", StringComparison.Ordinal))
                report.Error("Recipe sourceAssetsFolder must be within Assets/");
        }

        private static SparkAutoCsvTable ReadCsv(SparkAutoLevelRecipe r, string name, SparkAutoLevelBuildReport report)
        {
            if (string.IsNullOrWhiteSpace(name)) { report.Error("Missing CSV name in recipe"); return null; }
            string location = SourceFilePath(r, name);
            if (!File.Exists(location)) { report.Error("Missing source CSV: " + name); return null; }
            try { return SparkAutoCsvTable.Parse(File.ReadAllText(location)); }
            catch (Exception e) { report.Error("CSV parse failed " + name + ": " + e.Message); return null; }
        }

        private static string SourceFilePath(SparkAutoLevelRecipe r, string relative)
        {
            if (relative.StartsWith("Assets/", StringComparison.Ordinal))
            {
                string exact = ToAbsolute(relative);
                if (File.Exists(exact)) return exact;
                // Repair legacy references after a generator was moved to My_Assets.
                string fileName = Path.GetFileName(relative);
                if (!string.IsNullOrEmpty(fileName))
                {
                    string[] results = Directory.GetFiles(Application.dataPath, fileName, SearchOption.AllDirectories);
                    if (results.Length == 1) return results[0];
                }
                return exact;
            }
            string root = ToAbsolute(r.sourceAssetsFolder);
            string file = Path.GetFullPath(Path.Combine(root, relative.Replace('/', Path.DirectorySeparatorChar)));
            if (!file.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
                throw new IOException("Source path escapes level folder: " + relative);
            return file;
        }

        private static float Number(string value, SparkAutoLevelBuildReport report, string id, string field, float fallback = 0)
        {
            if (string.IsNullOrWhiteSpace(value)) return fallback;
            float parsed;
            if (!float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out parsed) ||
                float.IsNaN(parsed) || float.IsInfinity(parsed))
            { report.Error("Invalid " + field + " for " + id + ": " + value); return fallback; }
            return parsed;
        }

        private static bool Boolean(string value)
        {
            return value.Equals("true", StringComparison.OrdinalIgnoreCase) || value == "1" || value.Equals("yes", StringComparison.OrdinalIgnoreCase);
        }

        private static void ParseParts(SparkAutoCsvTable csv, List<SparkAutoPart> parts, SparkAutoLevelBuildReport report)
        {
            var ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < csv.RowCount; i++)
            {
                var row = csv.Row(i);
                string id = csv.Value(row, "model", "asset_name");
                if (string.IsNullOrWhiteSpace(id) || !ids.Add(id)) { report.Error("Missing/duplicate model ID: " + id); continue; }
                parts.Add(new SparkAutoPart {
                    Id = id, Type = csv.Value(row, "type", "asset_type", "kind"),
                    AssetPath = csv.Value(row, "fbx", "source_geometry_asset"),
                    Xmm = Number(csv.Value(row, "x_mm"), report, id, "x_mm"),
                    Ymm = Number(csv.Value(row, "y_mm"), report, id, "y_mm"),
                    Zmm = Number(csv.Value(row, "z_mm"), report, id, "z_mm"),
                    OrientationDegrees = Number(csv.Value(row, "orientation_deg"), report, id, "orientation_deg"),
                    Installable = Boolean(csv.Value(row, "installable", "individually_installable")),
                    Notes = csv.Value(row, "notes")
                });
            }
        }

        private static void ParseRoutes(SparkAutoCsvTable csv, List<SparkAutoRoute> routes, SparkAutoLevelBuildReport report)
        {
            var ids = new HashSet<string>(StringComparer.Ordinal);
            for (int i = 0; i < csv.RowCount; i++)
            {
                var row = csv.Row(i);
                string id = csv.Value(row, "route_id", "id");
                if (string.IsNullOrEmpty(id) || !ids.Add(id)) { report.Error("Missing/duplicate route ID: " + id); continue; }
                routes.Add(new SparkAutoRoute {
                    Id = id, Subsystem = csv.Value(row, "subsystem"), NetId = csv.Value(row, "net_id", "net"),
                    SourceComponent = csv.Value(row, "source_component", "start_terminal"),
                    SourcePad = csv.Value(row, "source_pad", "source_pin"),
                    DestinationComponent = csv.Value(row, "destination_component", "end_terminal"),
                    DestinationPad = csv.Value(row, "destination_pad", "destination_pin"),
                    Layer = csv.Value(row, "layer", "copper_layer"),
                    SourceStatus = csv.Value(row, "status", "validation")
                });
            }
        }

        private static void ParseVias(SparkAutoCsvTable csv, List<SparkAutoVia> vias, SparkAutoLevelBuildReport report)
        {
            var ids = new HashSet<string>(StringComparer.Ordinal);
            for (int i = 0; i < csv.RowCount; i++)
            {
                var row = csv.Row(i);
                string id = csv.Value(row, "via_id", "id");
                if (string.IsNullOrEmpty(id) || !ids.Add(id)) { report.Error("Missing/duplicate via ID: " + id); continue; }
                vias.Add(new SparkAutoVia {
                    Id = id, NetId = csv.Value(row, "net_id", "net"),
                    FromLayer = csv.Value(row, "from_layer", "start_layer"),
                    ToLayer = csv.Value(row, "to_layer", "end_layer"),
                    Xmm = csv.Value(row, "x_mm"), Ymm = csv.Value(row, "y_mm"),
                    ViaType = csv.Value(row, "via_type"), SourceStatus = csv.Value(row, "status", "validation")
                });
            }
        }

        private static void ReadObjectives(SparkAutoLevelRecipe r, List<SparkAutoObjective> dest, SparkAutoLevelBuildReport report)
        {
            if (string.IsNullOrWhiteSpace(r.objectivesJson)) { report.Warn("No objectives file specified; no objectives will be generated"); return; }
            string path = SourceFilePath(r, r.objectivesJson);
            if (!File.Exists(path)) { report.Error("Objectives JSON missing: " + r.objectivesJson); return; }
            try
            {
                var data = JsonUtility.FromJson<SparkSourceObjectiveArray>("{\"items\":" + File.ReadAllText(path) + "}");
                if (data == null || data.items == null) { report.Error("Objectives JSON has invalid structure"); return; }
                for (int i = 0; i < data.items.Length; i++)
                {
                    var x = data.items[i];
                    dest.Add(new SparkAutoObjective { Id = x.id, Title = string.IsNullOrEmpty(x.title) ? x.name : x.title,
                        Criterion = string.IsNullOrEmpty(x.criterion) ? x.requirements : x.criterion });
                }
            }
            catch (Exception e) { report.Error("Objectives JSON failed: " + e.Message); }
        }

        private static void ReadFaults(SparkAutoLevelRecipe r, List<SparkAutoFault> dest, SparkAutoLevelBuildReport report)
        {
            if (string.IsNullOrWhiteSpace(r.faultsJson)) { report.Warn("No fault definitions specified"); return; }
            string path = SourceFilePath(r, r.faultsJson);
            if (!File.Exists(path)) { report.Error("Fault definitions missing: " + r.faultsJson); return; }
            try
            {
                if (path.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
                {
                    var csv = SparkAutoCsvTable.Parse(File.ReadAllText(path));
                    for (int i = 0; i < csv.RowCount; i++)
                    {
                        var x = csv.Row(i);
                        dest.Add(new SparkAutoFault { Id = csv.Value(x, "id"), Title = csv.Value(x, "name", "target"),
                            Mechanism = csv.Value(x, "mechanism"), Repair = csv.Value(x, "repair") });
                    }
                }
                else
                {
                    var data = JsonUtility.FromJson<SparkSourceFaultArray>("{\"items\":" + File.ReadAllText(path) + "}");
                    if (data == null || data.items == null) { report.Error("Fault JSON invalid"); return; }
                    for (int i = 0; i < data.items.Length; i++)
                    {
                        var x = data.items[i];
                        dest.Add(new SparkAutoFault { Id = x.id, Title = x.name,
                            Mechanism = string.IsNullOrEmpty(x.model) ? x.mechanism : x.model,
                            Evidence = x.evidence == null ? string.Empty : string.Join("; ", x.evidence), Repair = x.repair });
                    }
                }
            }
            catch (Exception e) { report.Error("Fault import failed: " + e.Message); }
        }

        private static void ReadSafety(SparkAutoLevelRecipe r, SparkAutoSafety dest, SparkAutoLevelBuildReport report)
        {
            if (string.IsNullOrWhiteSpace(r.safetyJson)) { report.Error("Safety definition is required"); return; }
            string path = SourceFilePath(r, r.safetyJson);
            if (!File.Exists(path)) { report.Error("Safety definition missing: " + path); return; }
            try
            {
                var data = JsonUtility.FromJson<SparkSourceSafety>(File.ReadAllText(path));
                if (data == null) { report.Error("Safety definition invalid"); return; }
                dest.JumperId = string.IsNullOrEmpty(data.jumper) ? data.id : data.jumper;
                dest.LogicalSource = string.IsNullOrEmpty(data.source) ? (string.IsNullOrEmpty(data.logical_source) ? data.@from : data.logical_source) : data.source;
                dest.LogicalDestination = string.IsNullOrEmpty(data.destination) ? (string.IsNullOrEmpty(data.logical_destination) ? data.to : data.logical_destination) : data.destination;
                dest.PhysicalPinStatus = string.IsNullOrEmpty(data.physical_pin_mapping) ? (string.IsNullOrEmpty(data.physical_pins) ? data.electrical_pins : data.physical_pins) : data.physical_pin_mapping;
                dest.NetlistStatus = data.netlist_status;
                dest.StartsDisconnected = !string.Equals(data.initial_state, "CONNECTED", StringComparison.OrdinalIgnoreCase);
            }
            catch (Exception e) { report.Error("Safety import failed: " + e.Message); }
        }

        private static void CrossCheckManifest(SparkAutoLevelRecipe r, List<SparkAutoPart> parts, SparkAutoLevelBuildReport report)
        {
            int installable = 0;
            for (int i = 0; i < parts.Count; i++) if (parts[i].Installable) installable++;
            if (r.expectedModels > 0 && parts.Count != r.expectedModels)
                report.Error("Models manifest count " + parts.Count + " != expected " + r.expectedModels);
            if (r.expectedInstallable > 0 && installable != r.expectedInstallable)
                report.Error("Installable count " + installable + " != expected " + r.expectedInstallable);
            if (!string.IsNullOrWhiteSpace(r.componentsCsv))
            {
                var csv = ReadCsv(r, r.componentsCsv, report);
                if (csv != null)
                {
                    int rows = csv.RowCount;
                    if (rows != installable) report.Error("Component manifest count " + rows + " != installable models " + installable);
                    var ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    for (int i = 0; i < parts.Count; i++) if (parts[i].Installable) ids.Add(parts[i].Id);
                    for (int i = 0; i < csv.RowCount; i++)
                    {
                        string id = csv.Value(csv.Row(i), "model");
                        if (!ids.Remove(id)) report.Error("Component is not a unique installable model: " + id);
                    }
                    if (ids.Count != 0) report.Error("Some installable models are not in the component manifest");
                }
            }
        }
    }
}
#endif
