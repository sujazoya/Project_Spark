#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.AutoLevelGenerator.Editor
{
    public static class SparkAutoLevelBatchMenu
    {
        [MenuItem("Tools/Project Spark/Automatic Level Generator/Generate All Imported Levels (Safe)")]
        public static void GenerateAllImported()
        {
            var found = SparkAutoRecipeRegistry.Discover();
            int built = 0, skipped = 0, blocked = 0;
            for (int i = 0; i < found.Count; i++)
            {
                SparkAutoLevelBlueprint blueprint = null;
                try
                {
                    var recipe = SparkAutoLevelSourceImporter.LoadRecipe(found[i].Path);
                    string models = recipe.sourceAssetsFolder + "/" + recipe.modelsCsv;
                    string scene = recipe.generatedAssetsFolder + "/L" + recipe.levelId + "_" + recipe.boardId + "_EMPTY_GAMEPLAY_AUTO_GENERATED.unity";
                    if (!File.Exists(SparkAutoLevelSourceImporter.ToAbsolute(models)) ||
                        File.Exists(SparkAutoLevelSourceImporter.ToAbsolute(scene)) ||
                        File.Exists(SparkAutoLevelSourceImporter.ToAbsolute(scene.Substring(0, scene.Length - 6) + ".prefab"))) { skipped++; continue; }
                    var report = new SparkAutoLevelBuildReport();
                    blueprint = SparkAutoLevelSourceImporter.Import(recipe, report);
                    if (report.Errors != 0)
                    {
                        blocked++;
                        Debug.LogWarning("[Spark AutoLevel] L" + recipe.levelId + " blocked by source errors:\n" + report.Render());
                        continue;
                    }
                    SparkAutoLevelSceneBuilder.Generate(recipe, blueprint, report, true, true);
                    blueprint = null;
                    built++;
                    string folder = SparkAutoLevelSourceImporter.ToAbsolute(recipe.generatedAssetsFolder);
                    Directory.CreateDirectory(folder);
                    File.WriteAllText(Path.Combine(folder, "L" + recipe.levelId + "_AutoGeneration_Validation.txt"), report.Render());
                }
                catch (Exception ex) { blocked++; Debug.LogError("[Spark AutoLevel] " + found[i].Path + " failed: " + ex); }
                finally { if (blueprint != null && !AssetDatabase.Contains(blueprint)) UnityEngine.Object.DestroyImmediate(blueprint); }
            }
            AssetDatabase.Refresh();
            Debug.Log("[Spark AutoLevel V2] Built " + built + ", skipped " + skipped + ", blocked " + blocked +
                ". No existing generated scenes or earlier levels were overwritten.");
        }
    }
}
#endif
