#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.AutoLevelGenerator.Editor
{
    /// <summary>Command line/CI entry point. Use -sparkRecipePath or -sparkLevelId.</summary>
    public static class SparkAutoLevelBatchCLI
    {
        public static void Generate()
        {
            string[] args = Environment.GetCommandLineArgs();
            string path = Arg(args, "-sparkRecipePath", "");
            int levelId;
            bool haveId = int.TryParse(Arg(args, "-sparkLevelId", ""), out levelId);
            if (string.IsNullOrWhiteSpace(path) && haveId)
            {
                var entries = SparkAutoRecipeRegistry.Discover();
                for (int i = 0; i < entries.Count; i++) if (entries[i].LevelId == levelId)
                { path = entries[i].Path; break; }
            }
            var report = new SparkAutoLevelBuildReport();
            SparkAutoLevelBlueprint blueprint = null;
            SparkAutoLevelRecipe recipe = null;
            try
            {
                if (string.IsNullOrEmpty(path)) throw new ArgumentException(
                    "Pass -sparkRecipePath Assets/.../LevelN.recipe.json or -sparkLevelId 110");
                recipe = SparkAutoLevelSourceImporter.LoadRecipe(path);
                string zip = Arg(args, "-sparkZipPath", "");
                if (!string.IsNullOrEmpty(zip))
                    SparkAutoLevelZipImporter.ImportZip(zip, recipe, Has(args, "-sparkAllowOverwriteSource"), report);
                blueprint = SparkAutoLevelSourceImporter.Import(recipe, report);
                if (report.Errors > 0) throw new InvalidOperationException("Source preflight failed; see report");
                string scene = recipe.generatedAssetsFolder + "/L" + recipe.levelId + "_" + recipe.boardId + "_EMPTY_GAMEPLAY_AUTO_GENERATED.unity";
                string prefab = scene.Substring(0, scene.Length - ".unity".Length) + ".prefab";
                if ((File.Exists(SparkAutoLevelSourceImporter.ToAbsolute(scene)) ||
                     File.Exists(SparkAutoLevelSourceImporter.ToAbsolute(prefab))) &&
                     !Has(args, "-sparkAllowOverwriteGenerated"))
                    throw new IOException("Generated scene exists; pass -sparkAllowOverwriteGenerated explicitly");
                SparkAutoLevelSceneBuilder.Generate(recipe, blueprint, report, true, true);
                blueprint = null;
            }
            catch (Exception e) { report.Error("Batch failed: " + e.Message); Debug.LogException(e); }
            finally
            {
                if (blueprint != null && !AssetDatabase.Contains(blueprint)) UnityEngine.Object.DestroyImmediate(blueprint);
                if (recipe != null)
                {
                    string folder = SparkAutoLevelSourceImporter.ToAbsolute(recipe.generatedAssetsFolder);
                    Directory.CreateDirectory(folder);
                    File.WriteAllText(Path.Combine(folder, "L" + recipe.levelId + "_AutoGeneration_Validation.txt"), report.Render());
                    AssetDatabase.Refresh();
                }
                Debug.Log(report.Render());
                if (report.Errors > 0) EditorApplication.Exit(2);
            }
        }
        private static string Arg(string[] args, string key, string fallback)
        {
            for (int i = 0; i < args.Length - 1; i++) if (args[i] == key) return args[i + 1];
            return fallback;
        }
        private static bool Has(string[] args, string key)
        { for (int i = 0; i < args.Length; i++) if (args[i] == key) return true; return false; }
    }
}
#endif
