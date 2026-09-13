using System.Collections.Generic;
using UnityEngine;

namespace ProjectSpark.UI.Animation
{
    /// <summary>
    /// Central runtime registry and controller for Project Spark UI animations.
    ///
    /// Responsibilities:
    /// - Register UIAnimator instances.
    /// - Register UIScreenAnimator instances.
    /// - Register UIPanelAnimator instances.
    /// - Safely unregister destroyed/disabled objects.
    /// - Provide ID-based animation control.
    /// - Provide immediate and animated transitions.
    /// - Provide state-preset transitions.
    /// - Provide animation effects such as punch/shake.
    ///
    /// This class does NOT contain animation logic itself.
    /// Individual animation components remain responsible for their
    /// own DOTween sequences and visual state.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class UIAnimationManager : MonoBehaviour
    {
        // ============================================================
        // SINGLETON
        // ============================================================

        public static UIAnimationManager Instance
        {
            get;
            private set;
        }

        // ============================================================
        // CONFIGURATION
        // ============================================================

        [Header("MANAGER")]

        [SerializeField]
        private bool dontDestroyOnLoad = true;

        [SerializeField]
        private bool cleanupInvalidReferences = true;

        // ============================================================
        // REGISTRIES
        // ============================================================

        private readonly Dictionary<string, UIAnimator> animators =
            new();

        private readonly Dictionary<string, UIScreenAnimator> screens =
            new();

        private readonly Dictionary<string, UIPanelAnimator> panels =
            new();

        // ============================================================
        // PUBLIC COUNTS
        // ============================================================

        public int AnimatorCount =>
            animators.Count;

        public int ScreenCount =>
            screens.Count;

        public int PanelCount =>
            panels.Count;

        // ============================================================
        // UNITY
        // ============================================================

        private void Awake()
        {
            if (Instance != null &&
                Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            if (dontDestroyOnLoad)
                DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;

            animators.Clear();
            screens.Clear();
            panels.Clear();
        }

        private void LateUpdate()
        {
            if (!cleanupInvalidReferences)
                return;

            CleanupInvalidReferences();
        }

        // ============================================================
        // UI ANIMATOR REGISTRATION
        // ============================================================

        public void Register(
            string id,
            UIAnimator animator)
        {
            if (!IsValidId(id) ||
                animator == null)
            {
                return;
            }

            animators[id] = animator;
        }

        public void Unregister(
            string id)
        {
            if (!IsValidId(id))
                return;

            animators.Remove(id);
        }

        public void Unregister(
            string id,
            UIAnimator animator)
        {
            if (!IsValidId(id))
                return;

            if (!animators.TryGetValue(
                    id,
                    out UIAnimator registered))
            {
                return;
            }

            if (registered == animator)
                animators.Remove(id);
        }

        // ============================================================
        // SCREEN REGISTRATION
        // ============================================================

        public void RegisterScreen(
            string id,
            UIScreenAnimator screen)
        {
            if (!IsValidId(id) ||
                screen == null)
            {
                return;
            }

            screens[id] = screen;
        }

        public void UnregisterScreen(
            string id)
        {
            if (!IsValidId(id))
                return;

            screens.Remove(id);
        }

        public void UnregisterScreen(
            string id,
            UIScreenAnimator screen)
        {
            if (!IsValidId(id))
                return;

            if (!screens.TryGetValue(
                    id,
                    out UIScreenAnimator registered))
            {
                return;
            }

            if (registered == screen)
                screens.Remove(id);
        }

        // ============================================================
        // PANEL REGISTRATION
        // ============================================================

        public void RegisterPanel(
            string id,
            UIPanelAnimator panel)
        {
            if (!IsValidId(id) ||
                panel == null)
            {
                return;
            }

            panels[id] = panel;
        }

        public void UnregisterPanel(
            string id)
        {
            if (!IsValidId(id))
                return;

            panels.Remove(id);
        }

        public void UnregisterPanel(
            string id,
            UIPanelAnimator panel)
        {
            if (!IsValidId(id))
                return;

            if (!panels.TryGetValue(
                    id,
                    out UIPanelAnimator registered))
            {
                return;
            }

            if (registered == panel)
                panels.Remove(id);
        }

        // ============================================================
        // LOOKUP
        // ============================================================

        public bool TryGetAnimator(
            string id,
            out UIAnimator animator)
        {
            if (!IsValidId(id))
            {
                animator = null;
                return false;
            }

            if (!animators.TryGetValue(
                    id,
                    out animator))
            {
                return false;
            }

            if (animator == null)
            {
                animators.Remove(id);
                return false;
            }

            return true;
        }

        public bool TryGetScreen(
            string id,
            out UIScreenAnimator screen)
        {
            if (!IsValidId(id))
            {
                screen = null;
                return false;
            }

            if (!screens.TryGetValue(
                    id,
                    out screen))
            {
                return false;
            }

            if (screen == null)
            {
                screens.Remove(id);
                return false;
            }

            return true;
        }

        public bool TryGetPanel(
            string id,
            out UIPanelAnimator panel)
        {
            if (!IsValidId(id))
            {
                panel = null;
                return false;
            }

            if (!panels.TryGetValue(
                    id,
                    out panel))
            {
                return false;
            }

            if (panel == null)
            {
                panels.Remove(id);
                return false;
            }

            return true;
        }

        // ============================================================
        // BASIC ANIMATOR CONTROL
        // ============================================================

        public bool Show(
            string id)
        {
            if (!TryGetAnimator(
                    id,
                    out UIAnimator animator))
            {
                return false;
            }

            animator.Show();

            return true;
        }

        public bool Hide(
            string id)
        {
            if (!TryGetAnimator(
                    id,
                    out UIAnimator animator))
            {
                return false;
            }

            animator.Hide();

            return true;
        }

        public bool Toggle(
            string id)
        {
            if (!TryGetAnimator(
                    id,
                    out UIAnimator animator))
            {
                return false;
            }

            animator.Toggle();

            return true;
        }

        // ============================================================
        // IMMEDIATE ANIMATOR CONTROL
        // ============================================================

        public bool ShowImmediate(
            string id)
        {
            if (!TryGetAnimator(
                    id,
                    out UIAnimator animator))
            {
                return false;
            }

            animator.Show(true);

            return true;
        }

        public bool HideImmediate(
            string id)
        {
            if (!TryGetAnimator(
                    id,
                    out UIAnimator animator))
            {
                return false;
            }

            animator.Hide(true);

            return true;
        }

        // ============================================================
        // STATE PRESET CONTROL
        // ============================================================

        public bool TransitionToPreset(
            string id,
            UIAnimationPreset preset)
        {
            if (!TryGetAnimator(
                    id,
                    out UIAnimator animator))
            {
                return false;
            }

            animator.TransitionToPreset(
                preset);

            return true;
        }

        public bool TransitionToPresetImmediate(
            string id,
            UIAnimationPreset preset)
        {
            if (!TryGetAnimator(
                    id,
                    out UIAnimator animator))
            {
                return false;
            }

            animator.TransitionToPreset(
                preset,
                true);

            return true;
        }

        // ============================================================
        // EFFECTS
        // ============================================================

        public bool PlayPunch(
            string id)
        {
            if (!TryGetAnimator(
                    id,
                    out UIAnimator animator))
            {
                return false;
            }

            animator.PlayPunch();

            return true;
        }

        public bool PlayShake(
            string id)
        {
            if (!TryGetAnimator(
                    id,
                    out UIAnimator animator))
            {
                return false;
            }

            animator.PlayShake();

            return true;
        }

        // ============================================================
        // SCREEN CONTROL
        // ============================================================

        public bool ShowScreen(
            string id)
        {
            if (!TryGetScreen(
                    id,
                    out UIScreenAnimator screen))
            {
                return false;
            }

            screen.Show();

            return true;
        }

        public bool HideScreen(
            string id)
        {
            if (!TryGetScreen(
                    id,
                    out UIScreenAnimator screen))
            {
                return false;
            }

            screen.Hide();

            return true;
        }

        public bool ShowScreenImmediate(
            string id)
        {
            if (!TryGetScreen(
                    id,
                    out UIScreenAnimator screen))
            {
                return false;
            }

            screen.Show(true);

            return true;
        }

        public bool HideScreenImmediate(
            string id)
        {
            if (!TryGetScreen(
                    id,
                    out UIScreenAnimator screen))
            {
                return false;
            }

            screen.Hide(true);

            return true;
        }

        public bool CancelScreenTransition(
            string id)
        {
            if (!TryGetScreen(
                    id,
                    out UIScreenAnimator screen))
            {
                return false;
            }

            screen.CancelTransition();

            return true;
        }

        // ============================================================
        // PANEL CONTROL
        // ============================================================

        public bool ShowPanel(
            string id)
        {
            if (!TryGetPanel(
                    id,
                    out UIPanelAnimator panel))
            {
                return false;
            }

            panel.Show();

            return true;
        }

        public bool HidePanel(
            string id)
        {
            if (!TryGetPanel(
                    id,
                    out UIPanelAnimator panel))
            {
                return false;
            }

            panel.Hide();

            return true;
        }

        public bool TogglePanel(
            string id)
        {
            if (!TryGetPanel(
                    id,
                    out UIPanelAnimator panel))
            {
                return false;
            }

            panel.Toggle();

            return true;
        }

        public bool ShowPanelImmediate(
            string id)
        {
            if (!TryGetPanel(
                    id,
                    out UIPanelAnimator panel))
            {
                return false;
            }

            panel.ShowImmediate();

            return true;
        }

        public bool HidePanelImmediate(
            string id)
        {
            if (!TryGetPanel(
                    id,
                    out UIPanelAnimator panel))
            {
                return false;
            }

            panel.HideImmediate();

            return true;
        }

        // ============================================================
        // GLOBAL CONTROL
        // ============================================================

        public void HideAll()
        {
            foreach (KeyValuePair<string, UIAnimator> pair
                     in animators)
            {
                UIAnimator animator = pair.Value;

                if (animator == null)
                    continue;

                animator.Hide();
            }

            foreach (KeyValuePair<string, UIScreenAnimator> pair
                     in screens)
            {
                UIScreenAnimator screen = pair.Value;

                if (screen == null)
                    continue;

                screen.Hide();
            }

            foreach (KeyValuePair<string, UIPanelAnimator> pair
                     in panels)
            {
                UIPanelAnimator panel = pair.Value;

                if (panel == null)
                    continue;

                panel.Hide();
            }
        }

        public void HideAllImmediate()
        {
            foreach (KeyValuePair<string, UIAnimator> pair
                     in animators)
            {
                UIAnimator animator = pair.Value;

                if (animator == null)
                    continue;

                animator.Hide(true);
            }

            foreach (KeyValuePair<string, UIScreenAnimator> pair
                     in screens)
            {
                UIScreenAnimator screen = pair.Value;

                if (screen == null)
                    continue;

                screen.Hide(true);
            }

            foreach (KeyValuePair<string, UIPanelAnimator> pair
                     in panels)
            {
                UIPanelAnimator panel = pair.Value;

                if (panel == null)
                    continue;

                panel.HideImmediate();
            }
        }

        public void ShowAll()
        {
            foreach (KeyValuePair<string, UIAnimator> pair
                     in animators)
            {
                UIAnimator animator = pair.Value;

                if (animator == null)
                    continue;

                animator.Show();
            }
        }

        public void ShowAllImmediate()
        {
            foreach (KeyValuePair<string, UIAnimator> pair
                     in animators)
            {
                UIAnimator animator = pair.Value;

                if (animator == null)
                    continue;

                animator.Show(true);
            }
        }

        // ============================================================
        // CLEANUP
        // ============================================================

        private void CleanupInvalidReferences()
        {
            CleanupAnimators();
            CleanupScreens();
            CleanupPanels();
        }

        private void CleanupAnimators()
        {
            if (animators.Count == 0)
                return;

            List<string> invalidIds = null;

            foreach (KeyValuePair<string, UIAnimator> pair
                     in animators)
            {
                if (pair.Value != null)
                    continue;

                invalidIds ??= new List<string>();
                invalidIds.Add(pair.Key);
            }

            if (invalidIds == null)
                return;

            for (int i = 0;
                 i < invalidIds.Count;
                 i++)
            {
                animators.Remove(
                    invalidIds[i]);
            }
        }

        private void CleanupScreens()
        {
            if (screens.Count == 0)
                return;

            List<string> invalidIds = null;

            foreach (KeyValuePair<string, UIScreenAnimator> pair
                     in screens)
            {
                if (pair.Value != null)
                    continue;

                invalidIds ??= new List<string>();
                invalidIds.Add(pair.Key);
            }

            if (invalidIds == null)
                return;

            for (int i = 0;
                 i < invalidIds.Count;
                 i++)
            {
                screens.Remove(
                    invalidIds[i]);
            }
        }

        private void CleanupPanels()
        {
            if (panels.Count == 0)
                return;

            List<string> invalidIds = null;

            foreach (KeyValuePair<string, UIPanelAnimator> pair
                     in panels)
            {
                if (pair.Value != null)
                    continue;

                invalidIds ??= new List<string>();
                invalidIds.Add(pair.Key);
            }

            if (invalidIds == null)
                return;

            for (int i = 0;
                 i < invalidIds.Count;
                 i++)
            {
                panels.Remove(
                    invalidIds[i]);
            }
        }

        // ============================================================
        // VALIDATION
        // ============================================================

        private static bool IsValidId(
            string id)
        {
            return !string.IsNullOrWhiteSpace(id);
        }

        // ============================================================
        // DEBUG / DEVELOPMENT
        // ============================================================

#if UNITY_EDITOR || DEVELOPMENT_BUILD

        public bool ContainsAnimator(
            string id)
        {
            return IsValidId(id) &&
                   animators.ContainsKey(id);
        }

        public bool ContainsScreen(
            string id)
        {
            return IsValidId(id) &&
                   screens.ContainsKey(id);
        }

        public bool ContainsPanel(
            string id)
        {
            return IsValidId(id) &&
                   panels.ContainsKey(id);
        }

        public void ClearRegistry()
        {
            animators.Clear();
            screens.Clear();
            panels.Clear();
        }

#endif
    }
}
