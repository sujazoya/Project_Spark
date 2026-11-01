using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ProjectSpark.UI
{
    /// <summary>
    /// One non-invasive launch drawer for the existing Project Spark AI features.
    /// Optional modules are resolved by class name at runtime; missing modules do not
    /// cause CS0246 or prevent compilation. It never changes circuit/gameplay data.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkAILearningPanel : MonoBehaviour
    {
        public enum DrawerMode { Hidden = 0, Icons = 1, Detailed = 2 }

        [Serializable]
        public struct ActionSpec
        {
            public string id, section, badge, title, description, className, methodName;
            public ActionSpec(string id, string section, string badge, string title,
                string description, string className, string methodName)
            {
                this.id = id;
                this.section = section;
                this.badge = badge;
                this.title = title;
                this.description = description;
                this.className = className;
                this.methodName = methodName;
            }
        }

        // Builder and runtime use EXACTLY the same catalog/order.
        // All methods are public, parameterless methods from the existing modules.
        public static readonly ActionSpec[] Catalog =
        {
            new ActionSpec("ask", "AI ASSISTANT & LESSONS", "AI", "Ask Spark", "Open your existing AI conversation", "SparkAIUnifiedAssistant", "Show"),
            new ActionSpec("hub", "AI ASSISTANT & LESSONS", "HB", "Tutorial Hub", "Central AI teaching controls", "SparkAIUnifiedTutorialHub", "Show"),
            new ActionSpec("session", "AI ASSISTANT & LESSONS", "SE", "Teaching Session", "Questions, steps and session timeline", "SparkAITeachingSessionController", "Show"),
            new ActionSpec("board", "AI ASSISTANT & LESSONS", "WB", "Circuit Whiteboard", "2D circuit symbols and verified routes", "SparkVisualTeachingManager", "OpenTeachingBoard"),
            new ActionSpec("world", "AI ASSISTANT & LESSONS", "3D", "World Highlights", "Show the 3D annotation view", "SparkVisualTeachingManager", "ViewWorld"),
            new ActionSpec("both", "AI ASSISTANT & LESSONS", "2D", "Both Teaching Views", "3D annotations and 2D whiteboard", "SparkVisualTeachingManager", "ViewBoth"),
            new ActionSpec("animate", "AI ASSISTANT & LESSONS", "AN", "Animated AI Answers", "Step-by-step verified explanations", "SparkAIAnimatedAnswerPlayer", "Show"),
            new ActionSpec("components", "AI ASSISTANT & LESSONS", "CP", "Component Lessons", "Resistor, LED, switch and faults", "SparkAIComponentVisualLessonPlayer", "Show"),
            new ActionSpec("natural", "AI ASSISTANT & LESSONS", "NL", "Natural Explanation", "Explain verified circuit facts", "SparkAIExplanationRequestBridge", "Show"),
            new ActionSpec("adaptive", "AI ASSISTANT & LESSONS", "AD", "Adaptive Diagnosis", "Contextual diagnostic explanations", "SparkAdaptiveExplanationPanel", "ShowCard"),
            new ActionSpec("library", "AI ASSISTANT & LESSONS", "LB", "Lesson Library", "Search and bookmark visual lessons", "SparkAILessonLibrary", "Show"),

            new ActionSpec("training", "PRACTICE & REPAIR", "TR", "Interactive Training", "Evaluator-verified practice challenges", "SparkAIInteractiveTrainingChallenges", "Show"),
            new ActionSpec("repair", "PRACTICE & REPAIR", "FX", "Guided Repair Missions", "Repair verified circuit faults", "SparkAIGuidedRepairMissions", "Show"),
            new ActionSpec("coach", "PRACTICE & REPAIR", "CO", "AI Learning Coach", "Personalized practice recommendations", "SparkAILearningCoach", "Show"),
            new ActionSpec("paths", "PRACTICE & REPAIR", "PT", "Guided Learning Paths", "Structured electronics learning courses", "SparkAIGuidedLearningPaths", "Show"),
            new ActionSpec("assessment", "PRACTICE & REPAIR", "EX", "Learning Assessments", "Knowledge and verified practical exams", "SparkAILearningPathAssessment", "Show"),

            new ActionSpec("student", "PROGRESS & ACHIEVEMENTS", "SC", "Student Progress Center", "All student learning tools", "SparkAIStudentProgressCenter", "Show"),
            new ActionSpec("dashboard", "PROGRESS & ACHIEVEMENTS", "DB", "Training Dashboard", "Challenges, achievements and history", "SparkAITrainingDashboard", "Show"),
            new ActionSpec("analytics", "PROGRESS & ACHIEVEMENTS", "CH", "Training Analytics", "Trends, difficulties and reports", "SparkAITrainingAnalytics", "Show"),
            new ActionSpec("certificate", "PROGRESS & ACHIEVEMENTS", "AW", "Course Certificates", "Local course-completion certificates", "SparkAICourseCertification", "Show"),
            new ActionSpec("notes", "PROGRESS & ACHIEVEMENTS", "SV", "Save / Resume Notes", "Review historical study sessions", "SparkAITeachingSessionSaveResume", "Show"),

            new ActionSpec("inspect", "QUICK ACTIONS", "IN", "Inspect Component", "Read electrical component information", "SparkAIUnifiedAssistant", "InspectUnderCursor"),
            new ActionSpec("explain", "QUICK ACTIONS", "EC", "Explain Circuit", "Current objective and solver diagnosis", "SparkAIUnifiedAssistant", "ExplainCircuit"),
            new ActionSpec("hint", "QUICK ACTIONS", "HI", "Next AI Hint", "Progressive level-aware guidance", "SparkAIUnifiedAssistant", "GiveHint"),
            new ActionSpec("tutorial", "QUICK ACTIONS", "TU", "Begin Tutorial", "Open your existing guided tutorial", "SparkAIUnifiedAssistant", "OpenTutorial"),
            new ActionSpec("prev", "QUICK ACTIONS", "<", "Previous Teaching Step", "Go to previous visual teaching step", "SparkVisualTeachingManager", "PreviousStep"),
            new ActionSpec("next", "QUICK ACTIONS", ">", "Next Teaching Step", "Advance the active teaching lesson", "SparkVisualTeachingManager", "NextStep"),
            new ActionSpec("replay", "QUICK ACTIONS", "RP", "Replay Teaching Step", "Replay the current visual lesson step", "SparkVisualTeachingManager", "ReplayStep"),
            new ActionSpec("telemetry", "QUICK ACTIONS", "ME", "Show Verified Measurement", "Highlight selected solver measurement", "SparkElectricalTelemetryBridge", "ShowSelectedInWorld"),
            new ActionSpec("trainverify", "QUICK ACTIONS", "VT", "Verify Training", "Check official evaluator challenge result", "SparkAIInteractiveTrainingChallenges", "VerifyChallenge"),
            new ActionSpec("repairverify", "QUICK ACTIONS", "VF", "Verify Repair", "Check a new fault solve and evaluation", "SparkAIGuidedRepairMissions", "VerifyRepair"),
            new ActionSpec("save", "QUICK ACTIONS", "S+", "Save Study Notes", "Save the current study session locally", "SparkAITeachingSessionSaveResume", "SaveNow"),
            new ActionSpec("export", "QUICK ACTIONS", "CSV", "Export Analytics", "Write learning report to persistent data", "SparkAITrainingAnalytics", "ExportReport"),
            new ActionSpec("issue", "QUICK ACTIONS", "OK", "Issue Eligible Certificate", "Check course requirements and issue", "SparkAICourseCertification", "IssueCertificate"),
        };

        [Serializable]
        public sealed class ActionBinding
        {
            public string id;
            public Button iconsButton;
            public Button detailedButton;
            public TMP_Text availabilityText;
            public Image badgeBackground;
        }

        [Serializable]
        public sealed class SectionBinding
        {
            public string section;
            public GameObject header;
        }

        [Header("Builder-Generated Drawer Objects")]
        [SerializeField] private GameObject hiddenTab;
        [SerializeField] private GameObject iconRail;
        [SerializeField] private GameObject detailedDrawer;
        [SerializeField] private CanvasGroup hiddenGroup;
        [SerializeField] private CanvasGroup iconsGroup;
        [SerializeField] private CanvasGroup detailedGroup;
        [SerializeField] private Button openIconsButton;
        [SerializeField] private Button iconsToHiddenButton;
        [SerializeField] private Button iconsToDetailedButton;
        [SerializeField] private Button detailedToIconsButton;
        [SerializeField] private Button detailedToHiddenButton;
        [SerializeField] private Button refreshButton;
        [SerializeField] private TMP_Text availableCountText;
        [SerializeField] private TMP_Text feedbackText;
        [SerializeField] private TMP_Text railTipText;
        [SerializeField] private GameObject iconTooltip;
        [SerializeField] private TMP_Text iconTooltipLabel;
        [SerializeField] private TMP_InputField searchInput;
        [SerializeField] private ActionBinding[] actionBindings = new ActionBinding[0];
        [SerializeField] private SectionBinding[] sectionBindings = new SectionBinding[0];

        [Header("Three Drawer Modes")]
        [SerializeField] private DrawerMode initialMode = DrawerMode.Hidden;
        [SerializeField] private bool rememberMode = true;
        [SerializeField] private bool animateTransitions = true;
        [SerializeField, Range(0.26f, 0.65f)] private float animationSeconds = 0.34f;
        [SerializeField, Range(0.5f, 1.7f)] private float slideDistanceMultiplier = 1f;
        [SerializeField, Range(0f, 0.08f)] private float enterDelayFraction = 0.035f;
        [SerializeField] private bool subtleScaleAnimation = true;
        [SerializeField] private bool collapseToIconsAfterAction = false;
        [SerializeField, Range(1f, 20f)] private float availabilityRefreshSeconds = 5f;

        private const string ModeKey = "ProjectSpark.AILearningPanel.Mode.v1";
        private readonly List<Tuple<Button, UnityAction>> handlers =
            new List<Tuple<Button, UnityAction>>();
        private readonly Dictionary<string, ActionBinding> bindingLookup =
            new Dictionary<string, ActionBinding>(StringComparer.Ordinal);
        private readonly Dictionary<string, bool> availability =
            new Dictionary<string, bool>(StringComparer.Ordinal);
        private DrawerMode currentMode;
        private Coroutine animation;
        private float nextRefresh;

        // Rest positions are captured once from the already-generated scene.
        // No UI builder changes or panel recreation are required.
        private readonly RectTransform[] drawerRects = new RectTransform[3];
        private readonly Vector2[] restingPositions = new Vector2[3];
        private readonly Vector3[] restingScales = new Vector3[3];
        private bool motionInitialized;

        private struct DrawerTween
        {
            public GameObject panel;
            public RectTransform rect;
            public CanvasGroup group;
            public Vector2 startPosition;
            public Vector3 startScale;
            public float startAlpha;
            public bool animate;
        }

        public DrawerMode CurrentMode => currentMode;

        private void Awake()
        {
            BindButton(openIconsButton, ShowIcons);
            BindButton(iconsToHiddenButton, HideDrawer);
            BindButton(iconsToDetailedButton, ShowDetailed);
            BindButton(detailedToIconsButton, ShowIcons);
            BindButton(detailedToHiddenButton, HideDrawer);
            BindButton(refreshButton, RefreshAvailability);
            for (int i = 0; i < actionBindings.Length; i++)
            {
                ActionBinding binding = actionBindings[i];
                if (binding == null || string.IsNullOrEmpty(binding.id)) continue;
                bindingLookup[binding.id] = binding;
                string id = binding.id; // Explicit per-button capture (no closure loop bug).
                BindButton(binding.iconsButton, () => Execute(id));
                BindButton(binding.detailedButton, () => Execute(id));
            }
            if (searchInput != null) searchInput.onValueChanged.AddListener(ApplySearch);
        }

        private void Start()
        {
            int modeNumber = rememberMode && PlayerPrefs.HasKey(ModeKey)
                ? PlayerPrefs.GetInt(ModeKey, (int)initialMode) : (int)initialMode;
            currentMode = (DrawerMode)Mathf.Clamp(modeNumber, 0, 2);
            CaptureRestPositions();
            SetModeImmediate(currentMode);
            RefreshAvailability();
        }

        private void OnDestroy()
        {
            foreach (var handler in handlers)
                if (handler.Item1 != null) handler.Item1.onClick.RemoveListener(handler.Item2);
            handlers.Clear();
            if (searchInput != null) searchInput.onValueChanged.RemoveListener(ApplySearch);
        }

        private void Update()
        {
            if (currentMode == DrawerMode.Hidden || Time.unscaledTime < nextRefresh) return;
            RefreshAvailability();
        }

        private void BindButton(Button button, UnityAction callback)
        {
            if (button == null) return;
            button.onClick.AddListener(callback);
            handlers.Add(new Tuple<Button, UnityAction>(button, callback));
        }

        public void HideDrawer() => ChangeMode(DrawerMode.Hidden);
        public void ShowIcons() => ChangeMode(DrawerMode.Icons);
        public void ShowDetailed() => ChangeMode(DrawerMode.Detailed);
        public void Toggle() => ChangeMode(currentMode == DrawerMode.Hidden ? DrawerMode.Icons : DrawerMode.Hidden);
        public void NextMode() => ChangeMode((DrawerMode)(((int)currentMode + 1) % 3));

        public void ChangeMode(DrawerMode mode)
        {
            mode = (DrawerMode)Mathf.Clamp((int)mode, 0, 2);
            if (mode == currentMode && motionInitialized) return;

            // Stop without resetting transforms: the replacement tween begins from
            // wherever the previous transition actually reached, with no jump.
            if (animation != null)
            {
                StopCoroutine(animation);
                animation = null;
            }

            currentMode = mode;
            if (rememberMode) PlayerPrefs.SetInt(ModeKey, (int)mode);
            if (!motionInitialized) CaptureRestPositions();
            if (iconTooltip != null) iconTooltip.SetActive(false);

            bool canAnimate = animateTransitions && isActiveAndEnabled &&
                              Application.isPlaying && HasValidDrawer(mode);
            if (canAnimate)
                animation = StartCoroutine(AnimateToMode(mode));
            else
                SetModeImmediate(mode);

            if (mode != DrawerMode.Hidden) RefreshAvailability();
        }

        private GameObject PanelAt(int index)
        {
            switch (index)
            {
                case 0: return hiddenTab;
                case 1: return iconRail;
                default: return detailedDrawer;
            }
        }

        private CanvasGroup GroupAt(int index)
        {
            switch (index)
            {
                case 0: return hiddenGroup;
                case 1: return iconsGroup;
                default: return detailedGroup;
            }
        }

        private void CaptureRestPositions()
        {
            for (int i = 0; i < 3; i++)
            {
                GameObject panel = PanelAt(i);
                RectTransform rect = panel != null ? panel.GetComponent<RectTransform>() : null;
                drawerRects[i] = rect;
                restingPositions[i] = rect != null ? rect.anchoredPosition : Vector2.zero;
                restingScales[i] = rect != null ? rect.localScale : Vector3.one;
            }
            motionInitialized = true;
        }

        private bool HasValidDrawer(DrawerMode mode)
        {
            int index = (int)mode;
            return PanelAt(index) != null && GroupAt(index) != null &&
                   drawerRects[index] != null;
        }

        private void SetModeImmediate(DrawerMode mode)
        {
            if (!motionInitialized) CaptureRestPositions();
            for (int i = 0; i < 3; i++)
            {
                GameObject panel = PanelAt(i);
                CanvasGroup group = GroupAt(i);
                RectTransform rect = drawerRects[i];
                bool active = i == (int)mode;
                if (rect != null)
                {
                    rect.anchoredPosition = restingPositions[i];
                    rect.localScale = restingScales[i];
                }
                if (group != null)
                {
                    group.alpha = active ? 1f : 0f;
                    group.interactable = active;
                    group.blocksRaycasts = active;
                }
                if (panel != null) panel.SetActive(active);
            }
            if (iconTooltip != null) iconTooltip.SetActive(false);
        }

        // Distance is proportional to each drawer's width. The compact tab
        // travels much less than the expanded panel and never zooms across the screen.
        private float EnterDistance(int index)
        {
            float width = drawerRects[index] != null ? drawerRects[index].rect.width : 100f;
            float amount = index == 0 ? 25f : index == 1 ? 83f : 164f;
            return Mathf.Min(amount, Mathf.Max(30f, width * 0.52f)) *
                   Mathf.Clamp(slideDistanceMultiplier, 0.5f, 1.7f);
        }

        private IEnumerator AnimateToMode(DrawerMode mode)
        {
            int target = (int)mode;
            var tracks = new DrawerTween[3];

            // Only the active outgoing drawer and the incoming drawer are
            // displayed. Previously inactive drawers do not flash for one frame.
            for (int i = 0; i < 3; i++)
            {
                GameObject panel = PanelAt(i);
                RectTransform rect = drawerRects[i];
                CanvasGroup group = GroupAt(i);
                if (panel == null || rect == null || group == null) continue;

                bool alreadyVisible = panel.activeSelf;
                bool animateThis = alreadyVisible || i == target;
                if (!animateThis) continue;

                if (!alreadyVisible)
                {
                    rect.anchoredPosition = restingPositions[i] +
                                            Vector2.right * EnterDistance(i);
                    rect.localScale = subtleScaleAnimation
                        ? restingScales[i] * 0.981f : restingScales[i];
                    group.alpha = 0f;
                    panel.SetActive(true);
                }

                tracks[i] = new DrawerTween
                {
                    panel = panel,
                    rect = rect,
                    group = group,
                    startPosition = rect.anchoredPosition,
                    startScale = rect.localScale,
                    startAlpha = group.alpha,
                    animate = true
                };

                // The incoming drawer absorbs pointer hits while transitioning
                // but its controls cannot fire until it has settled.
                group.interactable = false;
                group.blocksRaycasts = i == target;
            }

            float duration = Mathf.Max(0.26f, animationSeconds);
            float elapsed = 0f;
            while (elapsed < duration)
            {
                // Unscaled time works correctly while the player's game is paused.
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float enterTime = Mathf.Clamp01((t - enterDelayFraction) /
                                                Mathf.Max(0.01f, 1f - enterDelayFraction));
                float enterMove = EaseOutQuint(enterTime);
                float enterAlpha = Smooth01(Mathf.Clamp01(enterTime / 0.83f));
                float exitMove = EaseInCubic(t);
                float exitAlpha = Smooth01(Mathf.Clamp01(t / 0.77f));

                for (int i = 0; i < 3; i++)
                {
                    DrawerTween state = tracks[i];
                    if (!state.animate || state.rect == null || state.group == null) continue;

                    if (i == target)
                    {
                        state.rect.anchoredPosition = Vector2.LerpUnclamped(
                            state.startPosition, restingPositions[i], enterMove);
                        state.group.alpha = Mathf.Lerp(state.startAlpha, 1f, enterAlpha);
                        state.rect.localScale = Vector3.LerpUnclamped(
                            state.startScale, restingScales[i], enterMove);
                    }
                    else
                    {
                        Vector2 exitPosition = restingPositions[i] +
                            Vector2.right * (EnterDistance(i) * 0.46f);
                        state.rect.anchoredPosition = Vector2.LerpUnclamped(
                            state.startPosition, exitPosition, exitMove);
                        state.group.alpha = Mathf.Lerp(state.startAlpha, 0f, exitAlpha);
                        state.rect.localScale = Vector3.LerpUnclamped(
                            state.startScale,
                            subtleScaleAnimation ? restingScales[i] * 0.989f : restingScales[i],
                            exitMove);
                    }
                }
                yield return null;
            }

            SetModeImmediate(mode);
            animation = null;
        }

        private static float Smooth01(float t)
        {
            t = Mathf.Clamp01(t);
            return t * t * (3f - 2f * t);
        }

        private static float EaseOutQuint(float t)
        {
            t = 1f - Mathf.Clamp01(t);
            return 1f - t * t * t * t * t;
        }

        private static float EaseInCubic(float t)
        {
            t = Mathf.Clamp01(t);
            return t * t * t;
        }

        private void OnDisable()
        {
            if (animation != null)
            {
                StopCoroutine(animation);
                animation = null;
            }
            if (motionInitialized) SetModeImmediate(currentMode);
        }

        public void RefreshAvailability()
        {
            nextRefresh = Time.unscaledTime + Mathf.Max(1f, availabilityRefreshSeconds);
            MonoBehaviour[] modules = FindObjectsByType<MonoBehaviour>(
                FindObjectsInactive.Include, FindObjectsSortMode.None);
            int available = 0;
            for (int i = 0; i < Catalog.Length; i++)
            {
                ActionSpec spec = Catalog[i];
                MonoBehaviour module;
                MethodInfo method;
                bool ready = TryResolve(modules, spec, out module, out method);
                availability[spec.id] = ready;
                if (ready) available++;
                if (!bindingLookup.TryGetValue(spec.id, out ActionBinding row)) continue;
                if (row.iconsButton != null) row.iconsButton.interactable = ready;
                if (row.detailedButton != null) row.detailedButton.interactable = ready;
                if (row.availabilityText != null) row.availabilityText.text = ready ? "READY" : "MISSING";
            }
            if (availableCountText != null)
                availableCountText.text = available + " / " + Catalog.Length + " ACTIONS READY";
            if (railTipText != null && string.IsNullOrWhiteSpace(railTipText.text))
                railTipText.text = "AI TOOLS";
            if (searchInput != null) ApplySearch(searchInput.text);
        }

        public void ApplySearch(string query)
        {
            query = (query ?? string.Empty).Trim();
            bool[] sectionMatches = new bool[sectionBindings.Length];
            for (int i = 0; i < Catalog.Length; i++)
            {
                ActionSpec spec = Catalog[i];
                if (!bindingLookup.TryGetValue(spec.id, out ActionBinding binding)) continue;
                bool show = query.Length == 0 ||
                    spec.title.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    spec.description.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    spec.section.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0;
                if (binding.detailedButton != null)
                    binding.detailedButton.gameObject.SetActive(show);
                if (show)
                    for (int j = 0; j < sectionBindings.Length; j++)
                        if (sectionBindings[j] != null && sectionBindings[j].section == spec.section)
                            sectionMatches[j] = true;
            }
            for (int j = 0; j < sectionBindings.Length; j++)
                if (sectionBindings[j] != null && sectionBindings[j].header != null)
                    sectionBindings[j].header.SetActive(sectionMatches[j]);
        }

        public void ShowIconHint(string actionId, RectTransform source)
        {
            if (currentMode != DrawerMode.Icons || animation != null ||
                iconTooltip == null || source == null) return;
            foreach (ActionSpec spec in Catalog)
            {
                if (spec.id != actionId) continue;
                if (iconTooltipLabel != null) iconTooltipLabel.text = spec.title;
                RectTransform rootRect = transform as RectTransform;
                RectTransform tipRect = iconTooltip.transform as RectTransform;
                if (rootRect != null && tipRect != null)
                {
                    Vector3 centerWorld = source.TransformPoint(source.rect.center);
                    Vector2 point = RectTransformUtility.WorldToScreenPoint(null, centerWorld);
                    if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                        rootRect, point, null, out Vector2 localPoint))
                    {
                        tipRect.anchoredPosition = new Vector2(-103f,
                            Mathf.Clamp(localPoint.y, -rootRect.rect.height / 2 + 35f,
                                rootRect.rect.height / 2 - 35f));
                    }
                }
                iconTooltip.SetActive(true);
                break;
            }
        }

        public void HideIconHint()
        {
            if (iconTooltip != null) iconTooltip.SetActive(false);
        }

        public void Execute(string actionId)
        {
            ActionSpec? selected = null;
            foreach (ActionSpec spec in Catalog)
                if (spec.id == actionId) { selected = spec; break; }
            if (!selected.HasValue) return;
            ActionSpec action = selected.Value;
            MonoBehaviour[] modules = FindObjectsByType<MonoBehaviour>(
                FindObjectsInactive.Include, FindObjectsSortMode.None);
            if (!TryResolve(modules, action, out MonoBehaviour target, out MethodInfo method))
            {
                SetFeedback(action.title + " is not installed in this scene.");
                RefreshAvailability();
                return;
            }
            try
            {
                method.Invoke(target, null);
                SetFeedback("OPENED  /  " + action.title);
                if (collapseToIconsAfterAction && currentMode == DrawerMode.Detailed)
                    ShowIcons();
            }
            catch (TargetInvocationException error)
            {
                Debug.LogException(error.InnerException ?? error, target);
                SetFeedback(action.title + " failed. See Unity Console.");
            }
            catch (Exception error)
            {
                Debug.LogException(error, target);
                SetFeedback(action.title + " failed. See Unity Console.");
            }
        }

        private void SetFeedback(string message)
        {
            if (feedbackText != null) feedbackText.text = message;
            if (railTipText != null) railTipText.text = message.Length > 19
                ? message.Substring(0, 19) : message;
        }

        private static bool TryResolve(MonoBehaviour[] modules, ActionSpec spec,
            out MonoBehaviour target, out MethodInfo method)
        {
            target = null;
            method = null;
            // Prefer a live component; allow inactive panel roots as fallback.
            for (int pass = 0; pass < 2; pass++)
            {
                foreach (MonoBehaviour module in modules)
                {
                    if (module == null || module.GetType().Name != spec.className) continue;
                    if (pass == 0 && (!module.enabled || !module.gameObject.activeInHierarchy)) continue;
                    MethodInfo found = module.GetType().GetMethod(spec.methodName,
                        BindingFlags.Instance | BindingFlags.Public, null, Type.EmptyTypes, null);
                    if (found == null || found.ReturnType != typeof(void)) continue;
                    target = module;
                    method = found;
                    return true;
                }
            }
            return false;
        }
    }

    /// <summary>Hover-only tooltip, configured by the Editor builder.</summary>
    [DisallowMultipleComponent]
    public sealed class SparkAILearningHoverHint : MonoBehaviour,
        IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private SparkAILearningPanel owner;
        [SerializeField] private string actionId;

        public void Configure(SparkAILearningPanel panel, string id)
        {
            owner = panel;
            actionId = id;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (owner != null) owner.ShowIconHint(actionId, transform as RectTransform);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (owner != null) owner.HideIconHint();
        }

        private void OnDisable()
        {
            if (owner != null) owner.HideIconHint();
        }
    }
}
