using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using ProjectSpark.AAAArchitecture;
using ProjectSpark.Gameplay;
using ProjectSpark.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Object = UnityEngine.Object;

[InitializeOnLoad]
internal static class SparkMissionCardVerification
{
    private const string Trigger = "Logs/SparkMissionCard.run";
    private const string Report = "Logs/SparkMissionCardVerification.txt";
    private const string Running = "SparkMissionCard.running";
    private const string PreviousScene = "SparkMissionCard.previous";
    private const string Generated = "Assets/ProjectSpark/AAAArchitecture/Generated/Prefabs/";

    static SparkMissionCardVerification()
    {
        EditorApplication.update += Tick;
        EditorApplication.playModeStateChanged += state =>
        {
            if (!SessionState.GetBool(Running, false))
                return;
            if (state == PlayModeStateChange.EnteredPlayMode)
                VerifyPlay();
            if (state != PlayModeStateChange.EnteredEditMode)
                return;
            EditorSceneManager.playModeStartScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(
                SessionState.GetString(PreviousScene, ""));
            SessionState.EraseString(PreviousScene);
            SessionState.EraseBool(Running);
        };
    }

    private static void Tick()
    {
        if (!File.Exists(Trigger) || EditorApplication.isPlayingOrWillChangePlaymode ||
            EditorApplication.isCompiling || EditorApplication.isUpdating)
            return;
        File.Delete(Trigger);
        File.WriteAllText(Report, "Spark Level Button verification\n");
        try
        {
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            Require(SparkAAALevelCard.FindArtwork(120, 12)?.name == "L120", "Missing Level 120 sprite");
            for (int chapter = 1; chapter <= 13; chapter++)
                Require(SparkAAALevelCard.FindArtwork(-1, chapter), $"Missing artwork for chapter {chapter}");

            string path = Generated + "SparkLevelButton.prefab";
            string guid = AssetDatabase.AssetPathToGUID(path);
            var sample = SparkAAALevelCard.Create();
            try { Require(PrefabUtility.SaveAsPrefabAsset(sample.gameObject, path), "Prefab save failed"); }
            finally { Object.DestroyImmediate(sample.gameObject); }
            Require(AssetDatabase.AssetPathToGUID(path) == guid, "Existing card prefab GUID changed");
            var prefab = AssetDatabase.LoadAssetAtPath<SparkAAALevelCard>(path);
            var preview = AssetDatabase.LoadAssetAtPath<SparkAAAMissionPanel>(Generated + "SparkLevelSelectionPanel.prefab");
            var serializedPreview = new SerializedObject(preview);
            var reference = serializedPreview.FindProperty("cardPrefab");
            // Unity matches the existing root component when replacing the generated contents.
            if (reference.objectReferenceValue != prefab)
            {
                reference.objectReferenceValue = prefab;
                serializedPreview.ApplyModifiedPropertiesWithoutUndo();
                AssetDatabase.SaveAssetIfDirty(preview);
            }
            Require(new SerializedObject(preview).FindProperty("cardPrefab").objectReferenceValue == prefab,
                "Generated level panel lost the upgraded card prefab");
            Require(prefab.GetComponentsInChildren<Image>(true).Any(image => image.sprite && image.sprite.name == "L120"),
                "Saved inspector sample has no artwork");
            Record("PASS Saved upgraded prefab with its existing GUID, Level 120 artwork, and generated-panel reference.");

            var scene = AssetDatabase.LoadAssetAtPath<SceneAsset>(
                "Assets/My_Assets/_Project_Spark/Scenes/MainMenu/MainMenu.unity");
            SessionState.SetString(PreviousScene, AssetDatabase.GetAssetPath(EditorSceneManager.playModeStartScene));
            SessionState.SetBool(Running, true);
            EditorSceneManager.playModeStartScene = scene;
            EditorApplication.EnterPlaymode();
        }
        catch (Exception e) { Record("FAIL " + e); }
    }

    private static async void VerifyPlay()
    {
        bool background = Application.runInBackground;
        try
        {
            Application.runInBackground = true;
            await Task.Delay(350);
            var panel = Object.FindFirstObjectByType<SparkCampaignPanel>();
            var manager = Object.FindFirstObjectByType<LevelGamePlayManager>();
            Require(panel && manager && manager.LayoutLoader.HasActiveLayout, "Live campaign scene bindings missing");
            var progression = manager.LevelProgression;
            typeof(SparkLevelProgression).GetField("saveProgress", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(progression, false);
            progression.ResetProgress();
            int index = manager.GetCampaignLevelIndices().Single(i => manager.GetLevel(i).LevelNumber == 120);

            var opener = panel.GetComponentsInChildren<Button>(true).Single(button => button.name == "Chapters");
            Click(opener);
            Require(panel.IsOpen, "HUD Chapters button failed");
            var root = (RectTransform)typeof(SparkCampaignPanel).GetField("overlay",
                BindingFlags.Instance | BindingFlags.NonPublic).GetValue(panel);
            Button Button(string name) => root.GetComponentsInChildren<Button>().Single(button => button.name == name);
            Click(Button("Chapter 12"));
            Canvas.ForceUpdateCanvases();
            var cards = root.GetComponentsInChildren<SparkAAALevelCard>();
            Require(cards.Length == 10, "Mission-Critical Systems should show ten live cards");
            var mission = cards.Single(card => card.name == "Level 120");
            Require(Label(mission.transform, "LevelNumber") == "LEVEL 120" &&
                Label(mission.transform, "MissionTitle") == "The Axis Forge" &&
                Label(mission.transform, "TargetCount") == "6 objectives" &&
                Label(mission.transform, "State") == "Locked", "Level 120 card data or lock state");
            Require(mission.GetComponentsInChildren<Image>().Any(image => image.sprite && image.sprite.name == "L120"),
                "Live card has no Level 120 artwork");
            Click(mission.Button);
            Require(root.GetComponentsInChildren<TMP_Text>().Any(title => title.name == "Level Title" && title.text == "The Axis Forge"),
                "Selecting card failed to open its briefing");
            Require(Label(root, "Objective List").Contains("6."), "All six authored objectives should be visible in the briefing scroll");
            Require(!Button("Play Selected").interactable, "Fresh profile can launch a locked mission");
            await Capture("Logs/SparkMissionCard120Locked.png");
            Record("PASS Live Chapters card has actual artwork, six objectives, clickable briefing and a real lock.");

            progression.UnlockLevel(index);
            Require(Label(mission.transform, "State") == "Available" && Button("Play Selected").interactable,
                "The card failed to update after a legitimate progression change");
            await Capture("Logs/SparkMissionCard120.png");
            Click(Button("Play Selected"));
            Require(!panel.IsOpen && manager.LayoutLoader.ActiveLayout.LevelDefinition == manager.GetLevel(index),
                "Play did not load the real Level 120 layout");
            Click(opener);
            var launched = root.GetComponentsInChildren<SparkAAALevelCard>().Single(card => card.name == "Level 120");
            Require(Label(launched.transform, "State") == "In play" && Label(root, "Level Access").Contains("/6 objectives passed"),
                "Current mission failed to show authoritative objective progress");

            progression.MarkCompleted(index);
            Require(Label(launched.transform, "State") == "In play", "Historical completion should not override a live attempt");
            Click(Button("Chapter 13"));
            Click(Button("Chapter 12"));
            Require(root.GetComponentsInChildren<SparkAAALevelCard>().Count() == 10, "Pooled cards failed to reset between chapters");
            Record("PASS Availability updates, native Level 120 launch, live objective progress and chapter browsing.");
            Record("PASS Level card Play Mode verification complete.");
        }
        catch (Exception e) { Record("FAIL " + e); }
        finally
        {
            Application.runInBackground = background;
            EditorApplication.ExitPlaymode();
        }
    }

    private static string Label(Transform root, string label) =>
        root.GetComponentsInChildren<TMP_Text>().Single(text => text.name == label).text;

    private static void Click(Button button)
    {
        Require(button.interactable, $"Button disabled: {button.name}");
        Canvas.ForceUpdateCanvases();
        var rect = (RectTransform)button.transform;
        var raycaster = rect.GetComponentInParent<GraphicRaycaster>();
        var data = new PointerEventData(EventSystem.current)
        {
            button = PointerEventData.InputButton.Left,
            position = RectTransformUtility.WorldToScreenPoint(raycaster.eventCamera, rect.TransformPoint(rect.rect.center))
        };
        var hits = new List<RaycastResult>();
        EventSystem.current.RaycastAll(data, hits);
        Require(hits.Count > 0 && hits[0].gameObject.GetComponentInParent<Button>() == button,
            $"{button.name} is covered by {hits.FirstOrDefault().gameObject}");
        ExecuteEvents.Execute(button.gameObject, data, ExecuteEvents.pointerClickHandler);
    }

    private static async Task Capture(string path)
    {
        await Task.Delay(250);
        ScreenCapture.CaptureScreenshot(path);
        for (int i = 0; i < 30 && !File.Exists(path); i++)
            await Task.Delay(100);
        Require(File.Exists(path), "Screenshot not captured: " + path);
    }

    private static void Require(bool condition, string detail)
    {
        if (!condition)
            throw new InvalidOperationException(detail);
    }

    private static void Record(string text) => File.AppendAllText(Report, text + "\n");
}

