using System;
using ProjectSpark.AI.Tutorial;
using ProjectSpark.Gameplay;
using UnityEngine;

namespace ProjectSpark.UI.VisualTeaching
{
    /// <summary>
    /// Presents the loaded layout's authored drawing or an objective map and read-only workbench views.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkDrawCurrentLevel : MonoBehaviour
    {
        [SerializeField] private LevelGamePlayManager levelGameplayManager;
        [Tooltip("Prefer the loaded layout in our single-scene system; gameplay manager is fallback.")]
        [SerializeField] private SparkLevelLayoutLoader levelLoader;
        [SerializeField] private SparkVisualTeachingManager teachingManager;
        [SerializeField] private SparkLevelDrawingAsset[] drawings = Array.Empty<SparkLevelDrawingAsset>();
        [SerializeField] private bool openWhiteboard = true;
        [SerializeField] private bool logMissingDrawing = true;
        [Header("Level Switching")]
        [SerializeField] private bool redrawOnActiveLevelChange = true;
        [Tooltip("If enabled, starts drawing when a level becomes available, even before the button is pressed.")]
        [SerializeField] private bool drawOnFirstLevel = false;
        [SerializeField, Min(0.05f)] private float checkIntervalSeconds = 0.25f;
        [SerializeField] private int currentPage;

        private string lastDisplayedLevelId = string.Empty;
        private bool hasDrawn;
        private SparkLevelLayout lastDisplayedLayout;
        private SparkTeachingView currentView;
        private int displayedEvidence;
        private bool generatedPage;
        private bool objectivesOnly;
        private bool wiring;
        private int boardPage;
        private float nextCheckTime;
        private float nextWarningTime;

        public int CurrentPage => currentPage;
        public int PageCount
        {
            get
            {
                var context = ResolveContext();
                var drawing = FindDrawing(context.Definition ? NormalizeId(context.Definition.LevelId) : "");
                return !objectivesOnly && drawing && drawing.PageCount > 0 ? drawing.PageCount : SparkAILevelVisuals.ObjectivePageCount(context.Definition);
            }
        }
        public string CurrentLevelId
        {
            get
            {
                var level = ResolveContext().Definition;
                return level ? NormalizeId(level.LevelId) : string.Empty;
            }
        }

        private SparkAILevelContext ResolveContext()
        {
            Resolve();
            return SparkAILevelContext.Capture(levelGameplayManager, levelLoader);
        }

        private static string NormalizeId(string value) => (value ?? string.Empty).Trim();
        private void Awake() => Resolve();
        private void OnEnable() { nextCheckTime = 0f; Resolve(); }

        private void Resolve()
        {
            if (levelGameplayManager == null)
                levelGameplayManager = FindFirstObjectByType<LevelGamePlayManager>(FindObjectsInactive.Include);
            if (levelGameplayManager && levelGameplayManager.LayoutLoader)
                levelLoader = levelGameplayManager.LayoutLoader;
            else if (levelLoader == null)
                levelLoader = FindFirstObjectByType<SparkLevelLayoutLoader>(FindObjectsInactive.Include);
            if (teachingManager == null)
                teachingManager = FindFirstObjectByType<SparkVisualTeachingManager>(FindObjectsInactive.Include);
        }

        private void LateUpdate()
        {
            if (!redrawOnActiveLevelChange || (!hasDrawn && !drawOnFirstLevel)) return;
            if (Time.unscaledTime < nextCheckTime) return;
            nextCheckTime = Time.unscaledTime + Mathf.Max(0.05f, checkIntervalSeconds);

            var context = ResolveContext();
            if (hasDrawn && teachingManager && teachingManager.LevelDrawing != this)
            {
                hasDrawn = false;
                return;
            }
            bool newLayout = lastDisplayedLayout != context.Layout;
            if (hasDrawn && !newLayout)
            {
                if (!teachingManager || !teachingManager.PanelOpen || teachingManager.Paused ||
                    (!(generatedPage || wiring) && currentView != SparkTeachingView.WorldOnly) ||
                    displayedEvidence == Evidence(context)) return;
            }
            if (!context.Definition)
            {
                if (hasDrawn && teachingManager) teachingManager.ClearLevelPresentation();
                hasDrawn = false;
                lastDisplayedLayout = null;
                lastDisplayedLevelId = "";
                return;
            }
            bool panelOpen = !hasDrawn || (teachingManager && teachingManager.PanelOpen);
            bool shown = currentView == SparkTeachingView.WorldOnly ? ShowCurrentInWorld() :
                wiring ? ShowWiring(context) : TryShowLevel(context, newLayout);
            if (!shown)
            {
                if (hasDrawn && teachingManager) teachingManager.ClearLevelPresentation();
                hasDrawn = false;
                lastDisplayedLayout = null;
                lastDisplayedLevelId = "";
            }
            else if (!panelOpen) teachingManager.ClosePanel();
        }

        private int Evidence(SparkAILevelContext context)
        {
            if (!context.HasEvaluation) return -1;
            if (wiring && currentView != SparkTeachingView.WorldOnly)
                return context.Manager.CircuitSystem ? context.Manager.CircuitSystem.TopologyVersion : -1;
            unchecked
            {
                int value = (int)context.Manager.RuntimeState;
                if (context.Manager.TargetResults != null)
                    foreach (var result in context.Manager.TargetResults)
                        value = value * 31 + (result.satisfied ? 1 : 0);
                return value;
            }
        }

        private SparkLevelDrawingAsset FindDrawing(string levelId)
        {
            if (string.IsNullOrEmpty(levelId) || drawings == null) return null;
            foreach (var drawing in drawings)
            {
                if (drawing != null && string.Equals(NormalizeId(drawing.levelId),
                    levelId, StringComparison.OrdinalIgnoreCase)) return drawing;
            }
            return null;
        }

        [ContextMenu("Visual Teaching / Draw Current Level")]
        public void DrawCurrentLevel()
        {
            currentView = SparkTeachingView.Both;
            wiring = false;
            objectivesOnly = false;
            TryShowLevel(ResolveContext(), true);
        }

        public void DrawCurrentLevelFromAI() => DrawCurrentLevel();
        public void ShowOverview() => ShowPage(0);
        public void NextPage() => ShowPage(currentPage + 1);
        public void PreviousPage() => ShowPage(currentPage - 1);
        public void ShowSolution() => ShowPage(PageCount - 1);

        public bool ShowCurrentOnWhiteboard(bool liveWiring = false, bool objectives = false)
        {
            currentView = SparkTeachingView.WhiteboardOnly;
            wiring = liveWiring;
            objectivesOnly = objectives;
            var context = ResolveContext();
            return wiring ? ShowWiring(context) : TryShowLevel(context, true);
        }

        public bool ShowCurrentInWorld() => SwitchView(SparkTeachingView.WorldOnly);

        private bool ShowWiring(SparkAILevelContext context)
        {
            var plan = SparkAILevelVisuals.LiveWiring(context);
            if (plan != null && currentView == SparkTeachingView.Both)
                plan.annotations = SparkAILevelVisuals.WorldFocus(context)?.annotations;
            return Present(context, plan, 0);
        }

        internal bool SwitchView(SparkTeachingView view)
        {
            var context = ResolveContext();
            if (view == SparkTeachingView.WorldOnly)
            {
                var plan = SparkAILevelVisuals.WorldFocus(context);
                if (plan == null) return false;
                currentView = view;
                return Present(context, plan, 0);
            }
            currentView = view;
            return wiring ? ShowWiring(context) : TryShowPage(context, boardPage);
        }

        internal void ReplayDrawing() => SwitchView(currentView);

        public bool ShowPage(int page)
        {
            if (currentView == SparkTeachingView.WorldOnly) currentView = SparkTeachingView.WhiteboardOnly;
            wiring = false;
            return TryShowPage(ResolveContext(), page);
        }

        private bool TryShowLevel(SparkAILevelContext context, bool resetPage)
        {
            var drawing = context.Definition ? FindDrawing(NormalizeId(context.Definition.LevelId)) : null;
            int initialPage = !objectivesOnly && drawing ? drawing.initialPage : 0;
            return TryShowPage(context, resetPage ? initialPage : currentPage);
        }

        private bool TryShowPage(SparkAILevelContext context, int page)
        {
            if (!context.Definition || !teachingManager) return false;
            var drawing = FindDrawing(NormalizeId(context.Definition.LevelId));
            bool authored = !objectivesOnly && drawing && drawing.PageCount > 0;
            int count = authored ? drawing.PageCount : SparkAILevelVisuals.ObjectivePageCount(context.Definition);
            int selected = Mathf.Clamp(page, 0, count - 1);
            var plan = authored ? drawing.BuildPlan(selected) : SparkAILevelVisuals.ObjectivePage(context, selected);
            if (!authored && plan != null && currentView == SparkTeachingView.Both)
                plan.annotations = SparkAILevelVisuals.WorldFocus(context)?.annotations;
            if (!Present(context, plan, selected)) return false;
            boardPage = selected;
            return true;
        }

        private bool Present(SparkAILevelContext context, SparkVisualTeachingPlan plan, int page)
        {
            if (plan == null || !teachingManager) return false;
            if (!teachingManager.PresentValidatedPlan(plan))
            {
                Warn("Teaching manager rejected the current drawing.");
                return false;
            }
            currentPage = page;
            generatedPage = plan.schematic != null && plan.schematic.objectiveCards != null;
            displayedEvidence = Evidence(context);
            hasDrawn = true;
            lastDisplayedLevelId = NormalizeId(context.Definition.LevelId);
            lastDisplayedLayout = context.Layout;
            teachingManager.SetLevelDrawing(this, page, wiring || currentView == SparkTeachingView.WorldOnly ? 0 : PageCount);
            teachingManager.SetView(currentView);
            if (openWhiteboard) teachingManager.OpenPanel();
            return true;
        }

        private void Warn(string message)
        {
            if (!logMissingDrawing || Time.unscaledTime < nextWarningTime) return;
            nextWarningTime = Time.unscaledTime + 3f;
            Debug.LogWarning("[Level Draw] " + message, this);
        }

        [ContextMenu("Visual Teaching / Diagnose Current Level")]
        public void DiagnoseCurrentLevel()
        {
            Resolve();
            string id = CurrentLevelId;
            var asset = FindDrawing(id);
            Debug.Log("[Level Draw DIAGNOSTIC] Manager=" +
                      (levelGameplayManager != null ? levelGameplayManager.name : "NULL") +
                      " | ActiveLevelId='" + id + "'" +
                      " | LoaderLayout=" + (levelLoader != null && levelLoader.ActiveLayout != null ? levelLoader.ActiveLayout.name : "NONE") +
                      " | ManagerLevelId=" + (levelGameplayManager != null && levelGameplayManager.ActiveLevel != null ? levelGameplayManager.ActiveLevel.LevelId : "NONE") +
                      " | MatchedAsset=" + (asset != null ? asset.name : "NONE") +
                      " | LastPresentedId='" + lastDisplayedLevelId + "'" +
                      " | HasDrawn=" + hasDrawn +
                      " | RedrawOnChange=" + redrawOnActiveLevelChange +
                      " | RegisteredAssets=" + (drawings != null ? drawings.Length : 0), this);
        }
    }
}
