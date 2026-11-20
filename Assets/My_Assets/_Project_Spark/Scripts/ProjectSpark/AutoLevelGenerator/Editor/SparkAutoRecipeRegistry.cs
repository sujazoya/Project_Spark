#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.AutoLevelGenerator.Editor
{
    /// <summary>
    /// Finds recipes anywhere under Assets, including installations inside My_Assets.
    /// No hard-coded level or generator location is required.
    /// </summary>
    public static class SparkAutoRecipeRegistry
    {
        public sealed class Entry
        {
            public string Path;
            public int LevelId;
            public string Title;
            public string BoardId;
            public string Description => "Level " + LevelId + " | " + BoardId + " | " + Title;
        }

        public static List<Entry> Discover()
        {
            var all = new List<Entry>();
            string assetsRoot = Application.dataPath;
            if (!Directory.Exists(assetsRoot)) return all;
            string[] files = Directory.GetFiles(assetsRoot, "*.recipe.json", SearchOption.AllDirectories);
            Array.Sort(files, StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < files.Length; i++)
            {
                try
                {
                    string unityPath = "Assets" + files[i].Substring(assetsRoot.Length).Replace('\\', '/');
                    var recipe = JsonUtility.FromJson<SparkAutoLevelRecipe>(File.ReadAllText(files[i]));
                    if (recipe == null || recipe.levelId <= 0) continue;
                    all.Add(new Entry { Path = unityPath, LevelId = recipe.levelId,
                        Title = recipe.title ?? string.Empty, BoardId = recipe.boardId ?? string.Empty });
                }
                catch (Exception exception)
                {
                    Debug.LogWarning("[Spark AutoLevel] Invalid recipe at " + files[i] + ": " + exception.Message);
                }
            }
            all.Sort((a, b) => { int n = a.LevelId.CompareTo(b.LevelId); return n != 0 ? n :
                string.Compare(a.Path, b.Path, StringComparison.OrdinalIgnoreCase); });
            return all;
        }

        public static string Resolve(string requestedPath)
        {
            if (!string.IsNullOrWhiteSpace(requestedPath))
            {
                string normalized = requestedPath.Replace('\\', '/').Trim();
                if (normalized.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase) &&
                    File.Exists(SparkAutoLevelSourceImporter.ToAbsolute(normalized))) return normalized;
            }
            var entries = Discover();
            if (entries.Count == 0) return null;
            if (string.IsNullOrWhiteSpace(requestedPath)) return entries[0].Path;
            string name = Path.GetFileName(requestedPath.Replace('\\', '/'));
            Entry match = null;
            for (int i = 0; i < entries.Count; i++)
            {
                if (!string.Equals(Path.GetFileName(entries[i].Path), name, StringComparison.OrdinalIgnoreCase)) continue;
                if (match != null) throw new InvalidOperationException(
                    "Multiple recipes named '" + name + "'. Choose the full Assets/... path. Candidates: " +
                    match.Path + ", " + entries[i].Path);
                match = entries[i];
            }
            if (match != null) return match.Path;
            return null;
        }

        public static string PreferredRecipesFolder()
        {
            // Install next to whichever version of this Editor assembly is active.
            string[] guids = AssetDatabase.FindAssets("SparkAutoRecipeRegistry t:Script");
            for (int i = 0; i < guids.Length; i++)
            {
                string editorPath = AssetDatabase.GUIDToAssetPath(guids[i]).Replace('\\','/');
                if (!editorPath.EndsWith("/SparkAutoRecipeRegistry.cs", StringComparison.OrdinalIgnoreCase)) continue;
                string generatorRoot = Path.GetDirectoryName(Path.GetDirectoryName(editorPath)).Replace('\\', '/');
                string recipes = generatorRoot + "/Recipes";
                SparkAutoLevelSceneBuilder.EnsureFolder(recipes);
                return recipes;
            }
            const string fallback = "Assets/ProjectSpark/AutoLevelGenerator/Recipes";
            SparkAutoLevelSceneBuilder.EnsureFolder(fallback);
            return fallback;
        }
    }
}
#endif
