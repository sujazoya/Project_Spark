#if UNITY_EDITOR
using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.EditorTools
{
    /// <summary>
    /// Editable TXT round-trip for ScriptableObject assets.
    /// Select the EXISTING destination asset before import.
    /// Uses EditorJsonUtility so Unity's serialized field names and data survive.
    /// Does not support reconstructing the older display-name-only TXT exports.
    /// </summary>
    public static class SparkAIKnowledgeTextExporterImporter
    {
        private const string ExportMenu = "Tools/Project Spark/TextExporterImporter/Export Selected Asset (Editable TXT)";
        private const string ImportMenu = "Tools/Project Spark/TextExporterImporter/Import TXT Into Selected Asset";
        private const string StartMarker = "----- BEGIN UNITY EDITOR JSON -----";
        private const string EndMarker = "----- END UNITY EDITOR JSON -----";
        private const string Signature = "PROJECT_SPARK_EDITABLE_SO_TXT_V1";

        [MenuItem(ExportMenu, false, 205)]
        private static void Export()
        {
            var asset = Selection.activeObject as ScriptableObject;
            if (asset == null) return;
            string assetPath = AssetDatabase.GetAssetPath(asset);
            if (string.IsNullOrEmpty(assetPath))
            {
                EditorUtility.DisplayDialog("Project Spark", "Save the ScriptableObject as an asset before exporting.", "OK");
                return;
            }

            string file = EditorUtility.SaveFilePanel("Export editable ScriptableObject TXT",
                Application.dataPath, asset.name + "_Editable", "txt");
            if (string.IsNullOrEmpty(file)) return;
            try
            {
                string json = EditorJsonUtility.ToJson(asset, true);
                var builder = new StringBuilder(json.Length + 1024);
                builder.AppendLine(Signature);
                builder.AppendLine("# Readable/editable Unity serialized data. Modify values in the JSON section.");
                builder.AppendLine("# Keep field names, object-reference structures, and section markers unchanged.");
                builder.AppendLine("# The import requires selecting the ORIGINAL asset in Unity.");
                builder.AppendLine("Asset Name: " + asset.name);
                builder.AppendLine("Asset Type: " + asset.GetType().AssemblyQualifiedName);
                builder.AppendLine("Asset Path: " + assetPath);
                builder.AppendLine("Asset GUID: " + AssetDatabase.AssetPathToGUID(assetPath));
                builder.AppendLine("Export Date: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                builder.AppendLine(StartMarker);
                builder.AppendLine(json);
                builder.AppendLine(EndMarker);
                File.WriteAllText(file, builder.ToString(), new UTF8Encoding(false));
                Debug.Log("[Spark TXT] Editable export: " + file, asset);
                EditorUtility.RevealInFinder(file);
            }
            catch (Exception ex) { Debug.LogException(ex, asset); EditorUtility.DisplayDialog("Export failed", ex.Message, "OK"); }
        }

        [MenuItem(ImportMenu, false, 206)]
        private static void Import()
        {
            var asset = Selection.activeObject as ScriptableObject;
            if (asset == null) return;
            string assetPath = AssetDatabase.GetAssetPath(asset);
            if (string.IsNullOrEmpty(assetPath)) return;

            string file = EditorUtility.OpenFilePanel("Import editable TXT into selected ScriptableObject",
                Application.dataPath, "txt");
            if (string.IsNullOrEmpty(file)) return;
            try
            {
                string txt = File.ReadAllText(file, Encoding.UTF8);
                if (!txt.StartsWith(Signature, StringComparison.Ordinal))
                    throw new InvalidDataException("This is a legacy readable TXT report, not an editable round-trip export. Re-export using 'Export Selected Asset (Editable TXT)'.");

                string type = GetHeader(txt, "Asset Type: ");
                string guid = GetHeader(txt, "Asset GUID: ");
                string exportedPath = GetHeader(txt, "Asset Path: ");
                string selectedGuid = AssetDatabase.AssetPathToGUID(assetPath);
                if (type != asset.GetType().AssemblyQualifiedName)
                    throw new InvalidDataException("Asset type mismatch. TXT: " + type + " / selected: " + asset.GetType().AssemblyQualifiedName);
                if (!string.Equals(guid, selectedGuid, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidDataException("Asset GUID mismatch. Select the exact asset this TXT was exported from.\nTXT: " + exportedPath + "\nSelected: " + assetPath);

                int start = txt.IndexOf(StartMarker, StringComparison.Ordinal);
                int end = txt.IndexOf(EndMarker, StringComparison.Ordinal);
                if (start < 0 || end <= start) throw new InvalidDataException("JSON section markers missing or reversed.");
                string json = txt.Substring(start + StartMarker.Length, end - start - StartMarker.Length).Trim();
                if (!json.StartsWith("{", StringComparison.Ordinal) || !json.EndsWith("}", StringComparison.Ordinal))
                    throw new InvalidDataException("JSON section does not contain a valid JSON object.");

                if (!EditorUtility.DisplayDialog("Import into " + asset.name + "?",
                    "This will overwrite serialized fields on:\n" + assetPath +
                    "\n\nA timestamped backup of the current .asset file will be saved outside Assets first.\n\nImport now?",
                    "Backup and Import", "Cancel")) return;

                // Backup is stored outside Assets to prevent Unity registering an extra ScriptableObject.
                string projectRoot = Directory.GetParent(Application.dataPath).FullName;
                string backupDir = Path.Combine(projectRoot, "ProjectSpark_TXT_Backups");
                Directory.CreateDirectory(backupDir);
                string sourceFile = Path.Combine(projectRoot, assetPath.Replace('/', Path.DirectorySeparatorChar));
                if (!File.Exists(sourceFile)) throw new IOException("Cannot find current asset file to back up: " + sourceFile);
                string backupFile = Path.Combine(backupDir,
                    Path.GetFileNameWithoutExtension(assetPath) + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss_fff") + ".asset.bak");
                File.Copy(sourceFile, backupFile, false);

                Undo.RegisterCompleteObjectUndo(asset, "Import Project Spark TXT");
                try
                {
                    EditorJsonUtility.FromJsonOverwrite(json, asset);
                    EditorUtility.SetDirty(asset);
                    AssetDatabase.SaveAssets();
                    Debug.Log("[Spark TXT] Imported into '" + assetPath + "'. Backup: " + backupFile, asset);
                    EditorUtility.DisplayDialog("Import completed", "Updated: " + asset.name + "\nBackup: " + backupFile, "OK");
                }
                catch
                {
                    // Restore the in-memory asset from the untouched backup and keep disk state safe.
                    AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
                    throw;
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex, asset);
                EditorUtility.DisplayDialog("TXT Import failed", ex.Message, "OK");
            }
        }

        private static string GetHeader(string text, string prefix)
        {
            using (var reader = new StringReader(text))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (line == StartMarker) break;
                    if (line.StartsWith(prefix, StringComparison.Ordinal)) return line.Substring(prefix.Length).Trim();
                }
            }
            throw new InvalidDataException("Required header missing: " + prefix);
        }

        [MenuItem(ExportMenu, true)]
        [MenuItem(ImportMenu, true)]
        private static bool CanUse() => Selection.activeObject is ScriptableObject;
    }
}
#endif
