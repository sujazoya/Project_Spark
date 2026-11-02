using System;
using ProjectSpark.Gameplay;
using UnityEngine;

namespace ProjectSpark.UI.VisualTeaching
{
    /// <summary>
    /// Selects an authored drawing by the active level manager's LevelId.
    /// Retries until presentation succeeds; does not cache failed level selections.
    /// Does not inspect or modify any electrical circuit.
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
        private float nextCheckTime;
        private float nextWarningTime;

        public int CurrentPage => currentPage;
        public int PageCount
        {
            get
            {
                SparkLevelDrawingAsset drawing = GetCurrentDrawing();
                return drawing != null ? drawing.PageCount : 0;
            }
        }
        public string CurrentLevelId
        {
            get
            {
                Resolve();
                // In the single-scene project, the layout loader owns the active layout.
                // The gameplay manager's ActiveLevel can lag behind a manual level switch.
                if (levelLoader != null && levelLoader.HasActiveLayout &&
                    levelLoader.ActiveLayout != null &&
                    levelLoader.ActiveLayout.LevelDefinition != null)
                    return NormalizeId(levelLoader.ActiveLayout.LevelDefinition.LevelId);

                var level = levelGameplayManager != null ? levelGameplayManager.ActiveLevel : null;
                return level != null ? NormalizeId(level.LevelId) : string.Empty;
            }
        }

        private static string NormalizeId(string value) => (value ?? string.Empty).Trim();
        private void Awake() => Resolve();
        private void OnEnable() { nextCheckTime = 0f; Resolve(); }

        private void Resolve()
        {
            if (levelLoader == null)
                levelLoader = FindFirstObjectByType<SparkLevelLayoutLoader>(FindObjectsInactive.Include);
            if (levelGameplayManager == null)
                levelGameplayManager = FindFirstObjectByType<LevelGamePlayManager>(FindObjectsInactive.Include);
            if (teachingManager == null)
                teachingManager = FindFirstObjectByType<SparkVisualTeachingManager>(FindObjectsInactive.Include);
        }

        private void LateUpdate()
        {
            if (!redrawOnActiveLevelChange || (!hasDrawn && !drawOnFirstLevel)) return;
            if (Time.unscaledTime < nextCheckTime) return;
            nextCheckTime = Time.unscaledTime + Mathf.Max(0.05f, checkIntervalSeconds);

            string id = CurrentLevelId;
            if (string.IsNullOrEmpty(id) || string.Equals(id, lastDisplayedLevelId, StringComparison.OrdinalIgnoreCase))
                return;

            // Don't remember an ID until PresentValidatedPlan returns true.
            TryShowLevel(id, true);
        }

        private SparkLevelDrawingAsset GetCurrentDrawing() => FindDrawing(CurrentLevelId);

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
            string id = CurrentLevelId;
            // Explicit button invocation always returns to the asset's initial page.
            TryShowLevel(id, true);
        }

        public void DrawCurrentLevelFromAI() => DrawCurrentLevel();
        public void ShowOverview() => ShowPage(0);
        public void NextPage() => ShowPage(currentPage + 1);
        public void PreviousPage() => ShowPage(currentPage - 1);
        public void ShowSolution() => ShowPage(PageCount - 1);

        public bool ShowPage(int page) => TryShowPage(CurrentLevelId, page);

        private bool TryShowLevel(string id, bool resetPage)
        {
            SparkLevelDrawingAsset drawing = FindDrawing(id);
            if (drawing == null)
            {
                Warn("No drawing asset found for active Level ID '" + id + "'.");
                return false;
            }
            return TryShowPage(id, resetPage ? drawing.initialPage : currentPage);
        }

        private bool TryShowPage(string id, int page)
        {
            Resolve();
            SparkLevelDrawingAsset drawing = FindDrawing(id);
            if (drawing == null || drawing.PageCount <= 0 || teachingManager == null)
            {
                Warn("Missing drawing/pages/teaching manager for active Level ID '" + id + "'.");
                return false;
            }

            int selected = Mathf.Clamp(page, 0, drawing.PageCount - 1);
            SparkVisualTeachingPlan plan = drawing.BuildPlan(selected);
            if (plan == null || !teachingManager.PresentValidatedPlan(plan))
            {
                Warn("Teaching manager rejected drawing '" + drawing.name + "' for level '" + id + "'.");
                return false;
            }

            currentPage = selected;
            hasDrawn = true;
            lastDisplayedLevelId = id;
            if (openWhiteboard) teachingManager.OpenTeachingBoard();
            /*Debug.Log("[Level Draw] PRESENTED asset='" + drawing.name + "' level='" + id +
                     "' page=" + (selected + 1) + "/" + drawing.PageCount + ".", this);*/
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
