#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using UnityEditor;

namespace ProjectSpark.AutoLevelGenerator.Editor
{
    public static class SparkAutoLevelZipImporter
    {
        private const long MaxSingleFileBytes = 512L * 1024 * 1024;
        private const long MaxTotalBytes = 4L * 1024 * 1024 * 1024;
        private static readonly HashSet<string> BlockedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase) {
            ".cs", ".dll", ".exe", ".bat", ".cmd", ".ps1", ".sh", ".asmdef", ".rsp", ".meta", ".unitypackage", ".so", ".dylib"
        };

        public static int ImportZip(string zipPath, SparkAutoLevelRecipe recipe, bool overwrite, SparkAutoLevelBuildReport report)
        {
            if (!File.Exists(zipPath)) { report.Error("ZIP not found: " + zipPath); return 0; }
            string root = SparkAutoLevelSourceImporter.ToAbsolute(recipe.sourceAssetsFolder);
            Directory.CreateDirectory(root);
            var import = new List<KeyValuePair<ZipArchiveEntry, string>>();
            int skipped = 0;
            long expectedBytes = 0;
            using (ZipArchive zip = ZipFile.OpenRead(zipPath))
            {
                for (int i = 0; i < zip.Entries.Count; i++)
                {
                    ZipArchiveEntry entry = zip.Entries[i];
                    string name = entry.FullName.Replace('\\', '/');
                    if (name.EndsWith("/", StringComparison.Ordinal)) continue;
                    if (!string.IsNullOrWhiteSpace(recipe.archiveRoot))
                    {
                        string prefix = recipe.archiveRoot.Trim('/') + "/";
                        if (!name.StartsWith(prefix, StringComparison.Ordinal)) continue;
                        name = name.Substring(prefix.Length);
                    }
                    if (name.Length == 0 || Path.IsPathRooted(name) || name.StartsWith("/", StringComparison.Ordinal) ||
                        name.IndexOf(':') >= 0 || name.IndexOf('\0') >= 0)
                    { report.Error("Unsafe ZIP path: " + entry.FullName); continue; }
                    string extension = Path.GetExtension(name);
                    if (BlockedExtensions.Contains(extension))
                    {
                        report.Warn("Skipped executable/script or Unity metadata file from ZIP: " + name);
                        skipped++;
                        continue;
                    }
                    if (entry.Length > MaxSingleFileBytes || entry.Length < 0 ||
                        expectedBytes > MaxTotalBytes - entry.Length)
                    { report.Error("ZIP size safety limit exceeded: " + name); continue; }
                    if (entry.CompressedLength > 0 && entry.Length > 10L * 1024 * 1024 &&
                        entry.Length / Math.Max(1L, entry.CompressedLength) > 200L)
                    { report.Error("Highly compressed suspicious ZIP entry: " + name); continue; }
                    string output = Path.GetFullPath(Path.Combine(root, name.Replace('/', Path.DirectorySeparatorChar)));
                    if (!output.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
                    { report.Error("ZIP traversal rejected: " + name); continue; }
                    if (File.Exists(output) && !overwrite) { skipped++; continue; }
                    import.Add(new KeyValuePair<ZipArchiveEntry, string>(entry, output));
                    expectedBytes += entry.Length;
                }
                // Preflight the complete ZIP before changing project assets.
                if (report.Errors > 0) { report.Warn("No files imported because ZIP preflight failed"); return 0; }
                int count = 0;
                for (int i = 0; i < import.Count; i++)
                {
                    var pair = import[i];
                    Directory.CreateDirectory(Path.GetDirectoryName(pair.Value));
                    // Write to a temporary sibling first; do not leave a half-written model on failed extraction.
                    string temporary = pair.Value + ".spark_import_tmp";
                    try
                    {
                        using (var source = pair.Key.Open())
                        using (var target = File.Create(temporary)) source.CopyTo(target);
                        if (File.Exists(pair.Value)) File.Replace(temporary, pair.Value, null);
                        else File.Move(temporary, pair.Value);
                        count++;
                    }
                    finally { if (File.Exists(temporary)) File.Delete(temporary); }
                }
                AssetDatabase.Refresh();
                report.Info("ZIP assets imported: " + count + " | skipped existing/unsafe scripts: " + skipped);
                return count;
            }
        }
    }
}
#endif
