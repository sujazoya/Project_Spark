#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.EditorTools
{
    /// <summary>
    /// Read-only inventory of available Project Spark C# source and components in
    /// currently loaded scenes. Never modifies scripts, prefabs, scenes or gameplay.
    /// Source matches are evidence of declarations, not proof of runtime behavior.
    /// </summary>
    public static class SparkElectricalArchitectureAuditor
    {
        private const string MenuRoot = "Tools/Project Spark/Electrical/";
        private const int MaxFileSize = 2 * 1024 * 1024;

        private static readonly Regex TypeDeclaration = new Regex(
            @"(?m)^\s*(?:public|internal|private|protected)?\s*(?:(?:abstract|sealed|static|partial)\s+)*(?:class|struct|interface|enum)\s+([A-Za-z_]\w*)",
            RegexOptions.Compiled);
        private static readonly Regex NamespaceDeclaration = new Regex(
            @"\bnamespace\s+([A-Za-z_]\w*(?:\.[A-Za-z_]\w*)*)",
            RegexOptions.Compiled);
        private static readonly Regex PublicDeclaration = new Regex(
            @"(?m)^\s*public\s+([^\r\n]{1,230})", RegexOptions.Compiled);
        private static readonly Regex CallPublishFrame = new Regex(
            @"\.\s*PublishVerifiedFrame\s*\(", RegexOptions.Compiled);
        private static readonly Regex CallPublishNodes = new Regex(
            @"\.\s*PublishVerifiedNodes\s*\(", RegexOptions.Compiled);

        [Serializable]
        private sealed class FileInfoRecord
        {
            public string path;
            public string category;
            public string declaredNamespace;
            public List<string> types = new List<string>();
        }

        [Serializable]
        private sealed class ApiInfoRecord
        {
            public string path;
            public string declaration;
        }

        [Serializable]
        private sealed class CheckRecord
        {
            public string capability;
            public string result;
            public string evidence;
        }

        [Serializable]
        private sealed class SceneRecord
        {
            public string component;
            public int count;
        }

        [Serializable]
        private sealed class AuditData
        {
            public string generatedUtc;
            public string project;
            public int totalCSharpFiles;
            public int relevantSourceFiles;
            public List<FileInfoRecord> files = new List<FileInfoRecord>();
            public List<ApiInfoRecord> publicApi = new List<ApiInfoRecord>();
            public List<CheckRecord> checks = new List<CheckRecord>();
            public List<SceneRecord> loadedSceneComponents = new List<SceneRecord>();
        }

        private sealed class Source
        {
            public string path;
            public string text;
            public FileInfoRecord record;
        }

        [MenuItem(MenuRoot + "Run Architecture Audit")]
        public static void RunAudit()
        {
            string assets = Application.dataPath;
            string output = GetReportDirectory();
            Directory.CreateDirectory(output);

            var report = new AuditData
            {
                generatedUtc = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss 'UTC'"),
                project = Application.productName
            };
            var sources = new List<Source>();
            var knownTypes = new HashSet<string>(StringComparer.Ordinal);
            string[] paths = Directory.GetFiles(assets, "*.cs", SearchOption.AllDirectories);
            report.totalCSharpFiles = paths.Length;

            foreach (string file in paths)
            {
                try
                {
                    var info = new System.IO.FileInfo(file);
                    if (info.Length > MaxFileSize) continue;
                    string value = File.ReadAllText(file);
                    string assetPath = "Assets/" + file.Substring(assets.Length)
                        .TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                        .Replace('\\', '/');
                    if (!IsRelevant(assetPath, value)) continue;

                    var record = new FileInfoRecord
                    {
                        path = assetPath,
                        declaredNamespace = FirstGroup(NamespaceDeclaration, value),
                        category = Categorize(assetPath)
                    };
                    foreach (Match match in TypeDeclaration.Matches(value))
                    {
                        string typeName = match.Groups[1].Value;
                        if (!record.types.Contains(typeName)) record.types.Add(typeName);
                        knownTypes.Add(typeName);
                    }
                    report.files.Add(record);
                    sources.Add(new Source { path = assetPath, text = value, record = record });

                    foreach (Match match in PublicDeclaration.Matches(value))
                    {
                        string signature = "public " + match.Groups[1].Value.Trim();
                        // A declaration inventory, not a complete C# parser.
                        if (signature.Length > 220) signature = signature.Substring(0, 220);
                        report.publicApi.Add(new ApiInfoRecord { path = assetPath, declaration = signature });
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning("[Spark Electrical Audit] Unable to inspect " + file + ": " + e.Message);
                }
            }
            report.relevantSourceFiles = report.files.Count;

            AddTypeCheck(report, knownTypes, sources, "Electrical component base", "SparkElectricalComponent");
            AddTypeCheck(report, knownTypes, sources, "Solved component state", "SparkElectricalState");
            AddTypeCheck(report, knownTypes, sources, "Resistor", "SparkResistor");
            AddTypeCheck(report, knownTypes, sources, "Resistor thermal / burn", "SparkResistorBurn");
            AddTypeCheck(report, knownTypes, sources, "LED", "SparkLED");
            AddTypeCheck(report, knownTypes, sources, "Switch", "SparkSwitch");
            AddTypeCheck(report, knownTypes, sources, "Power supply", "SparkPowerSupply");
            AddTypeCheck(report, knownTypes, sources, "Level gameplay evaluator", "LevelGamePlayManager");
            AddTypeCheck(report, knownTypes, sources, "Verified frame data", "SparkVerifiedElectricalFrame");
            AddTypeCheck(report, knownTypes, sources, "Verified frame publisher", "SparkVerifiedElectricalPublisher");
            AddTypeCheck(report, knownTypes, sources, "Telemetry synchronizer", "SparkSolverTelemetrySync");
            AddTypeCheck(report, knownTypes, sources, "Virtual multimeter", "SparkVirtualMultimeter");
            AddTypeCheck(report, knownTypes, sources, "Experiment lab", "SparkCircuitExperimentLab");
            AddTypeCheck(report, knownTypes, sources, "Node potential feed", "SparkMeterVerifiedNodeFeed");

            AddCallCheck(report, sources, "External completed-frame publish call", CallPublishFrame,
                "SparkVerifiedElectricalPublisher.cs");
            AddCallCheck(report, sources, "External node-potential publish call", CallPublishNodes,
                "SparkMeterVerifiedNodeFeed.cs");

            var solverCandidates = report.files.Where(f => f.types.Any(t =>
                Regex.IsMatch(t, @"(Circuit|Electrical|Network|Nodal|MNA).*(Solver|Simulator|Simulation)|(?:Solver|Simulator)$", RegexOptions.IgnoreCase) &&
                !Regex.IsMatch(t, @"(Telemetry|Training|Visual|AI|UI|Publisher|NodeFeed)", RegexOptions.IgnoreCase)))
                .Select(f => f.path).Distinct().ToArray();
            report.checks.Add(new CheckRecord
            {
                capability = "Electrical solver source candidates",
                result = solverCandidates.Length > 0 ? "CANDIDATES FOUND" : "NO NAMED CANDIDATES IN SCANNED SOURCE",
                evidence = solverCandidates.Length > 0 ? string.Join("; ", solverCandidates) :
                    "A solver can use any class name, assembly or native plugin. This search is not definitive."
            });

            CollectLoadedSceneComponents(report);
            report.files.Sort((a, b) => string.CompareOrdinal(a.path, b.path));
            report.publicApi.Sort((a, b) =>
            {
                int byFile = string.CompareOrdinal(a.path, b.path);
                return byFile != 0 ? byFile : string.CompareOrdinal(a.declaration, b.declaration);
            });

            string markdownPath = Path.Combine(output, "SparkElectricalArchitectureAudit.md");
            string jsonPath = Path.Combine(output, "SparkElectricalArchitectureAudit.json");
            string apiPath = Path.Combine(output, "SparkElectricalPublicAPI.csv");
            File.WriteAllText(markdownPath, CreateMarkdown(report), new UTF8Encoding(false));
            File.WriteAllText(jsonPath, JsonUtility.ToJson(report, true), new UTF8Encoding(false));
            File.WriteAllText(apiPath, CreateCsv(report.publicApi), new UTF8Encoding(false));

            Debug.Log("[Spark Electrical Audit] Read-only audit complete: " + markdownPath +
                      "\nFiles inspected: " + report.totalCSharpFiles +
                      "; relevant source files: " + report.relevantSourceFiles +
                      ". Source presence does not confirm runtime wiring.");
            EditorUtility.RevealInFinder(markdownPath);
        }

        [MenuItem(MenuRoot + "Open Last Architecture Audit")]
        public static void OpenLastAudit()
        {
            string path = Path.Combine(GetReportDirectory(), "SparkElectricalArchitectureAudit.md");
            if (File.Exists(path)) EditorUtility.RevealInFinder(path);
            else Debug.LogWarning("[Spark Electrical Audit] Run Architecture Audit first.");
        }

        private static string GetReportDirectory()
        {
            return Path.Combine(Directory.GetParent(Application.dataPath).FullName,
                "ProjectSparkReports", "ElectricalArchitecture");
        }

        private static bool IsRelevant(string path, string contents)
        {
            string name = Path.GetFileNameWithoutExtension(path);
            return name.StartsWith("Spark", StringComparison.OrdinalIgnoreCase) ||
                   name.Equals("LevelGamePlayManager", StringComparison.OrdinalIgnoreCase) ||
                   Regex.IsMatch(name, @"(Circuit|Electrical|Resistor|Battery|PowerSupply|Led|Terminal|Wire|Solver|Simulator|Multimeter|Oscilloscope|Generator|Thermal)", RegexOptions.IgnoreCase) ||
                   contents.Contains("namespace ProjectSpark.Electrical") ||
                   contents.Contains("namespace ProjectSpark.Circuit");
        }

        private static string Categorize(string path)
        {
            string n = Path.GetFileNameWithoutExtension(path);
            if (Regex.IsMatch(n, @"(Resistor|ElectricalComponent|ElectricalState|LED|Switch|PowerSupply|Battery|Terminal|Wire|Diode|Capacitor|Motor|Burn)", RegexOptions.IgnoreCase))
                return "Electrical components / state";
            if (Regex.IsMatch(n, @"(Solver|Simulator|CircuitGraph|ElectricalNetwork|Nodal)", RegexOptions.IgnoreCase))
                return "Solver / topology candidate";
            if (Regex.IsMatch(n, @"(LevelGamePlayManager|LevelTarget|LevelEvaluator)", RegexOptions.IgnoreCase))
                return "Gameplay evaluator";
            if (Regex.IsMatch(n, @"(Publisher|VerifiedElectricalFrame|Telemetry|NodeFeed)", RegexOptions.IgnoreCase))
                return "Verified data transport";
            if (Regex.IsMatch(n, @"(Multimeter|ExperimentLab|Oscilloscope|SignalGenerator|Probe)", RegexOptions.IgnoreCase))
                return "Workbench instruments";
            if (Regex.IsMatch(n, @"(Teaching|Lesson|AI|Tutorial|Schematic|Annotation)", RegexOptions.IgnoreCase))
                return "Teaching / UI consumer";
            return "Other Project Spark source";
        }

        private static string FirstGroup(Regex regex, string input)
        {
            Match m = regex.Match(input);
            return m.Success ? m.Groups[1].Value : "(not declared in file)";
        }

        private static void AddTypeCheck(AuditData data, HashSet<string> types,
            List<Source> sources, string name, string requiredType)
        {
            bool found = types.Contains(requiredType);
            var locations = sources.Where(s => s.record.types.Contains(requiredType))
                .Select(s => s.path).ToArray();
            data.checks.Add(new CheckRecord
            {
                capability = name,
                result = found ? "DECLARED IN SOURCE" : "NOT FOUND IN SCANNED SOURCE",
                evidence = found ? string.Join("; ", locations) :
                    requiredType + " may be in a compiled assembly, package, or unscanned source."
            });
        }

        private static void AddCallCheck(AuditData data, List<Source> sources,
            string label, Regex expression, string ownSourceFile)
        {
            var refs = sources.Where(s =>
                !s.path.EndsWith("/" + ownSourceFile, StringComparison.OrdinalIgnoreCase) &&
                expression.IsMatch(s.text)).Select(s => s.path).Distinct().ToArray();
            data.checks.Add(new CheckRecord
            {
                capability = label,
                result = refs.Length > 0 ? "CALL SITES FOUND (VERIFY RUNTIME)" : "NO EXTERNAL CALL SITES FOUND",
                evidence = refs.Length > 0 ? string.Join("; ", refs) :
                    "A solver integration may use an event, reflection, DLL or different handoff. Check Play Mode."
            });
        }

        private static void CollectLoadedSceneComponents(AuditData data)
        {
            var counts = new Dictionary<string, int>(StringComparer.Ordinal);
            var scripts = UnityEngine.Object.FindObjectsByType<MonoBehaviour>(
                FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var component in scripts)
            {
                if (component == null || component.gameObject == null) continue;
                var scene = component.gameObject.scene;
                if (!scene.IsValid() || !scene.isLoaded) continue;
                string name = component.GetType().Name;
                if (!name.StartsWith("Spark", StringComparison.Ordinal) &&
                    name != "LevelGamePlayManager") continue;
                counts[name] = counts.TryGetValue(name, out int count) ? count + 1 : 1;
            }
            foreach (var kv in counts.OrderBy(k => k.Key, StringComparer.Ordinal))
                data.loadedSceneComponents.Add(new SceneRecord { component = kv.Key, count = kv.Value });
        }

        private static string CreateMarkdown(AuditData d)
        {
            var sb = new StringBuilder(16000);
            sb.AppendLine("# Project Spark — Electrical Architecture Audit");
            sb.AppendLine();
            sb.AppendLine("Generated: " + d.generatedUtc);
            sb.AppendLine("Project: " + d.project);
            sb.AppendLine("C# files inspected: " + d.totalCSharpFiles);
            sb.AppendLine("Relevant source files: " + d.relevantSourceFiles);
            sb.AppendLine();
            sb.AppendLine("**Interpretation:** DECLARED IN SOURCE means a class/type exists in scanned Assets C#; it does NOT prove that a component is in a loaded scene, connected to a solver, or functioning. 'Not found' does not mean the class is missing at runtime; DLLs, Packages, and generated sources may not be included.");
            sb.AppendLine();
            sb.AppendLine("## Electrical capability evidence");
            sb.AppendLine();
            sb.AppendLine("| Capability | Static result | Evidence | ");
            sb.AppendLine("|---|---|---|");
            foreach (var c in d.checks)
                sb.AppendLine("| " + Md(c.capability) + " | " + Md(c.result) + " | " + Md(c.evidence) + " |");
            sb.AppendLine();
            sb.AppendLine("## Components in currently loaded scenes");
            sb.AppendLine();
            sb.AppendLine("Edit Mode inventory only; disabled children are counted. Other scenes and prefab assets are not loaded or inspected.");
            sb.AppendLine();
            if (d.loadedSceneComponents.Count == 0) sb.AppendLine("No matching MonoBehaviour instances found in loaded scenes.");
            else
            {
                sb.AppendLine("| Component | Instance count |\n|---|---:|");
                foreach (var r in d.loadedSceneComponents)
                    sb.AppendLine("| " + Md(r.component) + " | " + r.count + " |");
            }
            sb.AppendLine();
            sb.AppendLine("## Source file inventory");
            sb.AppendLine();
            sb.AppendLine("| Category | Source path | Declared types |\n|---|---|---|");
            foreach (var f in d.files)
                sb.AppendLine("| " + Md(f.category) + " | `" + f.path.Replace("`", "") + "` | " +
                              Md(string.Join(", ", f.types.ToArray())) + " |");
            sb.AppendLine();
            sb.AppendLine("## Next implementation gates");
            sb.AppendLine();
            sb.AppendLine("1. Confirm that the actual electrical solver has a completed-solve callback. Publish frame revisions only after a valid solve, on Unity's main thread.");
            sb.AppendLine("2. Match each reading `anchorId` to a placed component and a `SparkTeachingAnchor`; do not infer identity from GameObject display names.");
            sb.AppendLine("3. Ensure `circuitId` changes when the circuit layout changes, and use increasing positive revisions for synchronized node measurements.");
            sb.AppendLine("4. For arbitrary terminal voltage, publish verified node potentials with the same `circuitId` and `revision` as the component frame.");
            sb.AppendLine("5. Verify short/open/overload flags in the authoritative evaluator before allowing AI or instruments to assert faults.");
            sb.AppendLine("6. Do not attempt time-domain oscilloscope or signal generator simulation until the solver supports time-varying signals and matching sample timestamps.");
            sb.AppendLine();
            sb.AppendLine("## Other outputs");
            sb.AppendLine();
            sb.AppendLine("- `SparkElectricalArchitectureAudit.json`: machine-readable scan results.");
            sb.AppendLine("- `SparkElectricalPublicAPI.csv`: C# public declaration lines for inspection. This is a lexical scan, not a full C# compiler AST.");
            sb.AppendLine();
            sb.AppendLine("Report generation is read-only to gameplay assets. No scene, component, script, prefab, reward or save record is modified.");
            return sb.ToString();
        }

        private static string Md(string s)
        {
            return (s ?? "").Replace("|", "\\|").Replace("\r", " ").Replace("\n", " ");
        }

        private static string CreateCsv(List<ApiInfoRecord> apis)
        {
            var sb = new StringBuilder();
            sb.AppendLine("AssetPath,PublicDeclaration");
            foreach (var api in apis)
                sb.AppendLine(Csv(api.path) + "," + Csv(api.declaration));
            return sb.ToString();
        }

        private static string Csv(string s)
        {
            s = s ?? "";
            // Guard spreadsheet viewers against interpreting source declarations as formulas.
            if (s.Length > 0 && "=+-@".IndexOf(s[0]) >= 0) s = "'" + s;
            return "\"" + s.Replace("\"", "\"\"") + "\"";
        }
    }
}
#endif
