using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using ProjectSpark.AI.Tutorial;
using ProjectSpark.Gameplay;
using ProjectSpark.UI.VisualTeaching;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

[InitializeOnLoad]
internal static class SparkDrawPlayModeVerification
{
    private const string Report = "Logs/SparkDrawVerification.txt";
    private const string Running = "SparkDrawVerification.running";

    static SparkDrawPlayModeVerification()
    {
        EditorApplication.playModeStateChanged += state =>
        {
            if (state != PlayModeStateChange.EnteredPlayMode || !SessionState.GetBool(Running, false)) return;
            SessionState.SetBool(Running, false);
            Verify();
        };
    }

    [MenuItem("Tools/Project Spark/Visual Teaching/Verify MainMenu Draw System")]
    private static void Run()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling ||
            EditorApplication.isUpdating || SceneManager.GetActiveScene().name != "MainMenu")
        {
            Debug.LogWarning("[Spark Draw] Open MainMenu in Edit Mode before running visual teaching verification.");
            return;
        }
        File.WriteAllText(Report, "MainMenu draw verification\n");
        SessionState.SetBool(Running, true);
        EditorApplication.EnterPlaymode();
    }

    private static async void Verify()
    {
        bool previousBackground = Application.runInBackground;
        try
        {
            Application.runInBackground = true;
            var manager = Object.FindFirstObjectByType<LevelGamePlayManager>();
            var assistant = Object.FindFirstObjectByType<SparkAIUnifiedAssistant>(FindObjectsInactive.Include);
            var draw = Object.FindFirstObjectByType<SparkDrawCurrentLevel>(FindObjectsInactive.Include);
            var teaching = Object.FindFirstObjectByType<SparkVisualTeachingManager>(FindObjectsInactive.Include);
            var world = Object.FindFirstObjectByType<SparkWorldAnnotationRenderer>(FindObjectsInactive.Include);
            var schematic = Object.FindFirstObjectByType<SparkSchematicRenderer>(FindObjectsInactive.Include);
            Require(manager && assistant && draw && teaching && world && schematic, "Scene integration missing.");
            await Until(() => manager.ActiveLevel, "initial level");
            StartWithoutProgression(manager, 130);
            var context = SparkAILevelContext.Capture(manager);
            Require(context.HasEvaluation, "Level 130 matching evaluator missing.");
            Require(assistant.LocalAnswer("What level am I on?").Contains("Level 130"), "Wrong active level ID.");
            Require(assistant.LocalAnswer("Explain Level 128").Contains("Level 128"), "Campaign lookup failed.");
            int registered = Enumerable.Range(0, manager.LevelCount).Select(manager.GetLevel).Where(level => level).Distinct().Count();
            Require(assistant.LocalAnswer("How many levels?").Contains(registered.ToString()), "Campaign count failed.");
            Require(manager.ActiveTargetCount == 6, "Expected six Level 130 objectives.");
            await Until(() => manager.TargetResults.Count == 6, "first Level 130 evaluation");
            Record("PASS Active authored identity, campaign count and lookup. Campaign slots = " + manager.LevelCount);
            int before = manager.SatisfiedTargetCount;
            assistant.Ask("draw this level in 2D");
            await Until(() => teaching.View == SparkTeachingView.WhiteboardOnly &&
                schematic.GetComponentsInChildren<RectTransform>(true).Count(rect => rect.name.StartsWith("Objective ")) == 6, "six objective cards");
            Require(schematic.GetComponentsInChildren<TMP_Text>(true).Any(t => t.text == "PENDING"), "Native pending statuses missing.");
            await Capture("Logs/SparkDraw_2D.png");
            teaching.ReplayStep();
            await Until(() => schematic.GetComponentsInChildren<TMP_Text>(true).Any(t => t.text == "PENDING"), "whiteboard replay");
            Record("PASS Ask Spark displayed all six Level 130 objective cards and replayed the page.");

            teaching.ViewWorld();
            await Until(() => teaching.View == SparkTeachingView.WorldOnly && world.WorldGeometryCount == 1, "3D mode button");
            teaching.OpenTeachingBoard();
            Require(teaching.View == SparkTeachingView.WhiteboardOnly, "Opening the board did not restore its 2D page.");
            teaching.ViewBoth();
            await Until(() => teaching.View == SparkTeachingView.Both && world.WorldGeometryCount == 1, "combined visual mode");
            assistant.Ask("show this objective in 3D");
            await Until(() => teaching.View == SparkTeachingView.WorldOnly && world.WorldGeometryCount == 1, "3D teaching ring");
            var askCanvas = typeof(SparkAIUnifiedAssistant).GetField("canvas", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(assistant) as Canvas;
            Require(askCanvas && !askCanvas.gameObject.activeInHierarchy, "Chat overlay obscures the 3D view.");
            await Until(() => world.GetComponentsInChildren<TMP_Text>().Any(text => text.text.Contains("landmark")), "3D callout");
            var provider = context.Layout.GetComponentInChildren<ISparkAILevelTeachingFocus>();
            Require(provider != null && provider.TryGetTeachingFocus(out _, out _), "Custom objective focus missing.");
            provider.TryGetTeachingFocus(out var focus, out _);
            var ring = world.GetComponentsInChildren<LineRenderer>().Single(r => r.name == "Spark 3D teaching ring");
            Require(ring.useWorldSpace && Vector3.Distance(ring.GetPosition(0), focus.position) < .8f, "Marker is detached from the actual objective.");
            var viewport = Camera.main.WorldToViewportPoint(focus.position);
            Require(viewport.z > 0 && viewport.x > 0 && viewport.x < 1 && viewport.y > 0 && viewport.y < 1,
                "Objective marker is outside the workbench camera.");
            Require(manager.SatisfiedTargetCount == before, "Drawing altered the evaluation.");
            await Capture("Logs/SparkDraw_3D.png");
            Record("PASS View buttons and chat both open a readable 3D landmark highlight; drawing does not change progress.");

            Require(draw.ShowCurrentOnWhiteboard(true), "Live wiring view failed.");
            Require(manager.LayoutLoader.LoadLevel(context.Definition), "Same-ID layout reload failed.");
            var reloaded = SparkAILevelContext.Capture(manager);
            Require(reloaded.Layout != context.Layout && reloaded.Definition == context.Definition && !reloaded.HasEvaluation,
                "A new layout reused stale evaluation.");
            Require(assistant.LocalAnswer("Explain this level").Contains("matching gameplay evaluation is unavailable"),
                "Assistant reported stale evaluation.");
            await Until(() => !teaching.PanelOpen, "stale wiring view hidden");
            assistant.Ask("draw this level in 2D");
            await Until(() => schematic.GetComponentsInChildren<TMP_Text>(true).Any(t => t.text == "AWAITING EVALUATION"),
                "unverified objective cards");
            Record("PASS Same-ID layout replacement suppresses stale wiring and unverified objective statuses.");

            StartWithoutProgression(manager, 1);
            var beginner = SparkAILevelContext.Capture(manager);
            Require(beginner.HasEvaluation, "Level 1 matching evaluator missing.");
            assistant.Ask("draw live circuit in 2D");
            Require(teaching.View == SparkTeachingView.WhiteboardOnly && teaching.PanelOpen, "Live wiring whiteboard did not open.");
            var source = beginner.Definition.PowerSources.FirstOrDefault();
            var target = beginner.Definition.Targets.FirstOrDefault(t => t != null && t.HasRequiredConnectionPair);
            var bindings = beginner.Layout.SceneBindings;
            if (source != null && target != null && bindings.TryGetTerminal(source.PositiveTerminalId, out var a) &&
                bindings.TryGetTerminal(target.RequiredPositiveTerminalId, out var b) && a != b &&
                manager.CircuitSystem.TryCreateConnection(a, b, SparkConnectionKind.Wire, SparkConnectionDirection.Bidirectional, out _))
            {
                await Until(() => schematic.GetComponentsInChildren<TMP_Text>(true).Any(t => t.text == a.name), "native wire on whiteboard");
                await Capture("Logs/SparkDraw_Wiring.png");
                Record("PASS Live wiring whiteboard automatically updated from a native player wire.");
            }
            else Record("SKIP Level 1 did not declare a compatible source-to-target pair for the positive native wire test.");

            draw.DrawCurrentLevel();
            Require(draw.PageCount > 0 && teaching.PanelOpen, "Authored Level 1 drawing did not open.");
            int page = draw.CurrentPage;
            teaching.NextStep();
            Require(draw.CurrentPage == Math.Min(page + 1, draw.PageCount - 1), "Existing Next control did not advance the authored drawing.");
            if (draw.CurrentPage > 0)
            {
                page = draw.CurrentPage;
                teaching.PreviousStep();
                Require(draw.CurrentPage == page - 1, "Existing Previous control did not return to the previous drawing page.");
            }
            Record("PASS Existing whiteboard controls navigate the authored beginner drawing. Pages = " + draw.PageCount);
            Record("PASS MainMenu Play Mode draw verification complete.");
        }
        catch (Exception exception) { Record("FAIL " + exception); }
        finally
        {
            Application.runInBackground = previousBackground;
            EditorApplication.ExitPlaymode();
        }
    }

    private static void StartWithoutProgression(LevelGamePlayManager manager, int number)
    {
        typeof(LevelGamePlayManager).GetField("enforceProgressionAccess", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(manager, false);
        if (manager.LevelProgression) manager.LevelProgression.enabled = false;
        for (int i = 0; i < manager.LevelCount; i++)
        {
            var definition = manager.GetLevel(i);
            if (!definition || !int.TryParse(definition.LevelId.Trim().Substring(5).Trim(' ', '_', '-'), out int parsed) || parsed != number) continue;
            Require(manager.StartLevel(i), "Could not start Level " + number);
            return;
        }
        throw new InvalidOperationException("Campaign does not contain Level " + number);
    }

    private static async Task Capture(string path)
    {
        await Task.Delay(450);
        File.Delete(path);
        ScreenCapture.CaptureScreenshot(path);
        await Until(() => File.Exists(path) && new FileInfo(path).Length > 5000, path);
    }

    private static async Task Until(Func<bool> condition, string label)
    {
        var stop = DateTime.UtcNow.AddSeconds(25);
        while (!condition())
        {
            Require(EditorApplication.isPlaying && DateTime.UtcNow < stop, "Timed out waiting for " + label);
            await Task.Yield();
        }
    }

    private static void Require(bool condition, string reason)
    {
        if (!condition) throw new InvalidOperationException(reason);
    }

    private static void Record(string text) => File.AppendAllText(Report, text + Environment.NewLine);
}
