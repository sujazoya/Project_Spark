#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.AutoLevelGenerator.Editor
{
    /// <summary>Project-wide level generator with self-discovery, ZIP recipe onboarding and safe builds.</summary>
    public sealed class SparkAutoLevelGeneratorWindow : EditorWindow
    {
        [SerializeField] private string recipePath = "";
        [SerializeField] private string selectedZip = "";
        [SerializeField] private bool overwriteSourceAssets;
        [SerializeField] private bool overwriteGeneratedScenes;
        [SerializeField] private bool generateEmptyGameplay = true;
        [SerializeField] private bool generateCompletedReference = true;
        [SerializeField] private bool preflightOnlyBeforeBuild = true;
        [SerializeField] private int newLevelId = 111;
        [SerializeField] private string newBoardId = "NEW_BOARD";
        [SerializeField] private string newLevelTitle = "New Engineering Mission";
        [SerializeField] private float newBoardWidthMm;
        [SerializeField] private float newBoardHeightMm;
        [SerializeField] private int newCopperLayers = 10;
        private readonly List<SparkAutoRecipeRegistry.Entry> recipes = new List<SparkAutoRecipeRegistry.Entry>();
        private Vector2 scroll;
        private Vector2 recipeScroll;
        private string reportText = "Choose a discovered recipe or create one from an asset ZIP.";
        private bool showNewRecipe;
        private int selectedRecipeIndex = -1;

        [MenuItem("Tools/Project Spark/Automatic Level Generator/Open Generator")]
        public static void Open()
        {
            var window = GetWindow<SparkAutoLevelGeneratorWindow>("Spark Level Generator");
            window.minSize = new Vector2(720, 610);
            window.RefreshRecipes();
            window.Show();
        }

        [MenuItem("Tools/Project Spark/Automatic Level Generator/Generate Level 110")]
        public static void GenerateLevel110()
        {
            var window = GetWindow<SparkAutoLevelGeneratorWindow>("Spark Level Generator");
            window.RefreshRecipes();
            bool found = false;
            for (int i = 0; i < window.recipes.Count; i++)
                if (window.recipes[i].LevelId == 110)
                { window.SelectRecipe(i); found = true; break; }
            window.Show();
            if (!found) EditorUtility.DisplayDialog("Level 110 recipe missing",
                "Import the Level 110 recipe from the generator package or create one from its ZIP.", "OK");
        }

        private void OnEnable() { RefreshRecipes(); }

        private void RefreshRecipes()
        {
            recipes.Clear();
            recipes.AddRange(SparkAutoRecipeRegistry.Discover());
            string resolved = null;
            try { resolved = SparkAutoRecipeRegistry.Resolve(recipePath); }
            catch (Exception exception) { Debug.LogWarning("[Spark AutoLevel] Recipe selection needs attention: " + exception.Message); }
            selectedRecipeIndex = -1;
            for (int i = 0; i < recipes.Count; i++)
                if (string.Equals(recipes[i].Path, resolved, StringComparison.OrdinalIgnoreCase))
                { selectedRecipeIndex = i; break; }
            if (selectedRecipeIndex < 0 && recipes.Count > 0) SelectRecipe(0);
            Repaint();
        }

        private void SelectRecipe(int index)
        {
            if (index < 0 || index >= recipes.Count) return;
            selectedRecipeIndex = index;
            recipePath = recipes[index].Path;
            newLevelId = recipes[index].LevelId + 1;
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(7);
            EditorGUILayout.LabelField("PROJECT SPARK  |  UNIVERSAL AUTOMATIC LEVEL GENERATOR V2", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Generates data-backed art and gameplay scaffolding from any discovered level recipe. " +
                "Approved physical netlists and the real Project Spark gameplay adapters remain mandatory for playable electrical certification.", MessageType.Info);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Discovered level recipes: " + recipes.Count, EditorStyles.miniBoldLabel);
            if (GUILayout.Button("Refresh / Rescan Assets", GUILayout.Width(165))) RefreshRecipes();
            EditorGUILayout.EndHorizontal();
            if (recipes.Count > 0)
            {
                string[] labels = new string[recipes.Count];
                for (int i = 0; i < recipes.Count; i++) labels[i] = recipes[i].Description;
                int next = EditorGUILayout.Popup("Select Level", Mathf.Max(0, selectedRecipeIndex), labels);
                if (next != selectedRecipeIndex) SelectRecipe(next);
                if (selectedRecipeIndex >= 0 && selectedRecipeIndex < recipes.Count)
                    EditorGUILayout.LabelField("Recipe asset", recipes[selectedRecipeIndex].Path, EditorStyles.miniLabel);
            }
            else EditorGUILayout.HelpBox("No *.recipe.json files found anywhere under Assets. Copy the Recipes folder or create a recipe from ZIP below.", MessageType.Warning);

            EditorGUILayout.BeginHorizontal();
            recipePath = EditorGUILayout.TextField("Recipe path (optional)", recipePath);
            if (GUILayout.Button("Locate", GUILayout.Width(70)))
            {
                string resolved = SparkAutoRecipeRegistry.Resolve(recipePath);
                if (!string.IsNullOrEmpty(resolved))
                {
                    recipePath = resolved;
                    var obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(resolved);
                    if (obj != null) { Selection.activeObject = obj; EditorGUIUtility.PingObject(obj); }
                }
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(5);
            EditorGUILayout.BeginHorizontal();
            selectedZip = EditorGUILayout.TextField("Level asset ZIP", selectedZip);
            if (GUILayout.Button("Browse", GUILayout.Width(70)))
            {
                string candidate = EditorUtility.OpenFilePanel("Select Project Spark asset ZIP", "", "zip");
                if (!string.IsNullOrWhiteSpace(candidate)) selectedZip = candidate;
            }
            EditorGUILayout.EndHorizontal();
            showNewRecipe = EditorGUILayout.Foldout(showNewRecipe, "Create new level recipe automatically from ZIP", true);
            if (showNewRecipe)
            {
                EditorGUI.indentLevel++;
                newLevelId = EditorGUILayout.IntField("New Level ID", newLevelId);
                newBoardId = EditorGUILayout.TextField("Board ID", newBoardId);
                newLevelTitle = EditorGUILayout.TextField("Level title", newLevelTitle);
                newBoardWidthMm = EditorGUILayout.FloatField("Verified board width (mm)", newBoardWidthMm);
                newBoardHeightMm = EditorGUILayout.FloatField("Verified board height (mm)", newBoardHeightMm);
                newCopperLayers = EditorGUILayout.IntField("PCB copper layers", newCopperLayers);
                using (new EditorGUI.DisabledScope(string.IsNullOrWhiteSpace(selectedZip)))
                    if (GUILayout.Button("Inspect ZIP and Create Recipe", GUILayout.Height(28))) CreateRecipe();
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.Space(6);
            overwriteSourceAssets = EditorGUILayout.ToggleLeft("Allow replacing already imported source ZIP files (confirm)", overwriteSourceAssets);
            overwriteGeneratedScenes = EditorGUILayout.ToggleLeft("Allow replacing AUTO_GENERATED scenes/prefabs (confirm)", overwriteGeneratedScenes);
            generateEmptyGameplay = EditorGUILayout.ToggleLeft("Create EMPTY gameplay scene + prefab", generateEmptyGameplay);
            generateCompletedReference = EditorGUILayout.ToggleLeft("Create separate assembled reference scene + prefab", generateCompletedReference);
            preflightOnlyBeforeBuild = EditorGUILayout.ToggleLeft("Block generation on any manifest/import errors", preflightOnlyBeforeBuild);
            EditorGUILayout.Space(8);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("1. Validate", GUILayout.Height(35))) Execute(false, true);
            using (new EditorGUI.DisabledScope(string.IsNullOrWhiteSpace(selectedZip)))
                if (GUILayout.Button("2. Import ZIP + Build", GUILayout.Height(35))) Execute(true, false);
            if (GUILayout.Button("3. Build Imported", GUILayout.Height(35))) Execute(false, false);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Validate All Recipes", GUILayout.Height(26))) ValidateAll();
            if (GUILayout.Button("Generate All Imported Levels (Safe)", GUILayout.Height(26)))
                SparkAutoLevelBatchMenu.GenerateAllImported();
            EditorGUILayout.EndHorizontal();
            if (GUILayout.Button("Audit Project Spark Gameplay Integrations", GUILayout.Height(25)))
                reportText = SparkAutoProjectIntegrationAudit.Audit();
            EditorGUILayout.Space(7);
            EditorGUILayout.LabelField("VALIDATION + GENERATION REPORT", EditorStyles.boldLabel);
            scroll = EditorGUILayout.BeginScrollView(scroll);
            EditorGUILayout.TextArea(reportText, GUILayout.ExpandHeight(true));
            EditorGUILayout.EndScrollView();
        }

        private void CreateRecipe()
        {
            var report = new SparkAutoLevelBuildReport();
            try
            {
                recipePath = SparkAutoRecipeFactory.CreateFromZip(selectedZip, newLevelId, newBoardId,
                    newLevelTitle, newBoardWidthMm, newBoardHeightMm, newCopperLayers, report);
                RefreshRecipes();
                for (int i = 0; i < recipes.Count; i++) if (recipes[i].Path == recipePath) SelectRecipe(i);
            }
            catch (Exception e) { report.Error("Recipe creation failed: " + e.Message); Debug.LogException(e); }
            reportText = report.Render();
        }

        private void Execute(bool importZip, bool validateOnly)
        {
            var report = new SparkAutoLevelBuildReport();
            SparkAutoLevelRecipe recipe = null;
            SparkAutoLevelBlueprint blueprint = null;
            try
            {
                EditorUtility.DisplayProgressBar("Project Spark Level Generator", "Loading level recipe...", 0.08f);
                recipe = SparkAutoLevelSourceImporter.LoadRecipe(recipePath);
                if (importZip)
                {
                    if (overwriteSourceAssets && !EditorUtility.DisplayDialog("Replace source assets?",
                        "Existing imported files may be overwritten inside " + recipe.sourceAssetsFolder + ". Continue?", "Replace", "Cancel")) return;
                    EditorUtility.DisplayProgressBar("Project Spark Level Generator", "Importing asset package...", 0.20f);
                    SparkAutoLevelZipImporter.ImportZip(selectedZip, recipe, overwriteSourceAssets, report);
                }
                EditorUtility.DisplayProgressBar("Project Spark Level Generator", "Validating source data...", 0.52f);
                blueprint = SparkAutoLevelSourceImporter.Import(recipe, report);
                if (validateOnly) return;
                if (preflightOnlyBeforeBuild && report.Errors > 0)
                { report.Warn("Build blocked by preflight errors; fix manifests and retry"); return; }
                if (!generateEmptyGameplay && !generateCompletedReference)
                { report.Error("Select at least one output scene"); return; }
                if (!MayOverwrite(recipe)) return;
                EditorUtility.DisplayProgressBar("Project Spark Level Generator", "Creating Unity scenes and prefabs...", 0.79f);
                SparkAutoLevelSceneBuilder.Generate(recipe, blueprint, report, generateEmptyGameplay, generateCompletedReference);
                blueprint = null; // SceneBuilder now owns the blueprint and may destroy this temporary instance.
            }
            catch (Exception e)
            {
                report.Error("Generation aborted: " + e.Message);
                Debug.LogException(e);
            }
            finally
            {
                if (blueprint != null && !AssetDatabase.Contains(blueprint)) DestroyImmediate(blueprint);
                if (recipe != null) WriteReport(recipe, report);
                EditorUtility.ClearProgressBar();
                reportText = report.Render();
                Debug.Log("[Spark AutoLevel V2]\n" + reportText);
                Repaint();
            }
        }

        private bool MayOverwrite(SparkAutoLevelRecipe recipe)
        {
            string stem = "/L" + recipe.levelId + "_" + recipe.boardId;
            bool exists = (generateEmptyGameplay && (
                    File.Exists(SparkAutoLevelSourceImporter.ToAbsolute(recipe.generatedAssetsFolder + stem + "_EMPTY_GAMEPLAY_AUTO_GENERATED.unity")) ||
                    File.Exists(SparkAutoLevelSourceImporter.ToAbsolute(recipe.generatedAssetsFolder + stem + "_EMPTY_GAMEPLAY_AUTO_GENERATED.prefab")))) ||
                (generateCompletedReference && (
                    File.Exists(SparkAutoLevelSourceImporter.ToAbsolute(recipe.generatedAssetsFolder + stem + "_REFERENCE_AUTO_GENERATED.unity")) ||
                    File.Exists(SparkAutoLevelSourceImporter.ToAbsolute(recipe.generatedAssetsFolder + stem + "_REFERENCE_AUTO_GENERATED.prefab"))));
            if (!exists) return true;
            if (!overwriteGeneratedScenes) { EditorUtility.DisplayDialog("Generated level already exists",
                "Enable 'Allow replacing AUTO_GENERATED scenes/prefabs' to rebuild. Other levels will not be overwritten.", "OK"); return false; }
            return EditorUtility.DisplayDialog("Confirm generated level replacement",
                "Replace ONLY Level " + recipe.levelId + " generated scenes/prefabs? Manually edited generated files will be lost.", "Replace", "Cancel");
        }

        private void ValidateAll()
        {
            var all = new System.Text.StringBuilder();
            int checkedCount = 0;
            for (int i = 0; i < recipes.Count; i++)
            {
                var report = new SparkAutoLevelBuildReport();
                SparkAutoLevelBlueprint blueprint = null;
                try
                {
                    var recipe = SparkAutoLevelSourceImporter.LoadRecipe(recipes[i].Path);
                    blueprint = SparkAutoLevelSourceImporter.Import(recipe, report);
                }
                catch (Exception e) { report.Error(e.Message); }
                finally { if (blueprint != null) DestroyImmediate(blueprint); }
                all.AppendLine(recipes[i].Description);
                all.AppendLine(report.Render());
                checkedCount++;
            }
            reportText = "Validated " + checkedCount + " discovered recipes.\n\n" + all;
        }

        private static void WriteReport(SparkAutoLevelRecipe recipe, SparkAutoLevelBuildReport report)
        {
            try
            {
                string folder = SparkAutoLevelSourceImporter.ToAbsolute(recipe.generatedAssetsFolder);
                Directory.CreateDirectory(folder);
                File.WriteAllText(Path.Combine(folder, "L" + recipe.levelId + "_AutoGeneration_Validation.txt"), report.Render());
                AssetDatabase.Refresh();
            }
            catch (Exception e) { Debug.LogWarning("[Spark AutoLevel] Cannot write report: " + e.Message); }
        }
    }
}
#endif
