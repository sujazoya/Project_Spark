using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace ProjectSpark.UI.Animation
{
    /// <summary>
    /// Production panel animation controller.
    ///
    /// UIPanelAnimator owns panel-level sequencing.
    /// UIAnimator owns the actual transform/alpha animation.
    ///
    /// Child UI elements may optionally be animated in sequence.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(UIAnimator))]
    public sealed class UIPanelAnimator : MonoBehaviour
    {
        [System.Serializable]
        private sealed class ChildAnimationEntry
        {
            [SerializeField]
            private UIAnimator animator;

            [SerializeField]
            [Min(0f)]
            private float delay;

            public UIAnimator Animator => animator;

            public float Delay => delay;
        }

        [Header("References")]
        [SerializeField]
        private UIAnimator panelAnimator;

        [Header("Child Animation")]
        [SerializeField]
        private bool animateChildren = true;

        [SerializeField]
        private bool includeInactiveChildren = false;

        [SerializeField]
        [Min(0f)]
        private float childStagger = 0.055f;

        [SerializeField]
        private bool useExplicitChildren;

        [SerializeField]
        private List<ChildAnimationEntry> explicitChildren =
            new List<ChildAnimationEntry>();

        [Header("Behaviour")]
        [SerializeField]
        private bool animateOnEnable;

        [SerializeField]
        private bool hideChildrenOnDisable = true;

        [SerializeField]
        private bool childrenIgnoreTimeScale = true;

        private readonly List<UIAnimator> discoveredChildren =
            new List<UIAnimator>();

        private Sequence panelSequence;

        private bool isShown;

        public bool IsShown => isShown;

        public UIAnimator PanelAnimator => panelAnimator;

        private void Reset()
        {
            CacheReferences();
        }

        private void Awake()
        {
            CacheReferences();
            BuildChildCache();
        }

        private void OnEnable()
        {
            if (!animateOnEnable)
                return;

            Show();
        }

        private void OnDisable()
        {
            KillSequence();

            if (hideChildrenOnDisable)
                HideChildrenImmediate();

            isShown = false;
        }

        private void OnDestroy()
        {
            KillSequence();
        }

        // ============================================================
        // REFERENCES
        // ============================================================

        private void CacheReferences()
        {
            if (panelAnimator == null)
                panelAnimator = GetComponent<UIAnimator>();
        }

        // ============================================================
        // CHILD CACHE
        // ============================================================

        public void RebuildChildCache()
        {
            BuildChildCache();
        }

        private void BuildChildCache()
        {
            discoveredChildren.Clear();

            if (!animateChildren)
                return;

            if (useExplicitChildren)
            {
                for (int i = 0; i < explicitChildren.Count; i++)
                {
                    UIAnimator child =
                        explicitChildren[i]?.Animator;

                    if (child == null)
                        continue;

                    if (child == panelAnimator)
                        continue;

                    if (!discoveredChildren.Contains(child))
                        discoveredChildren.Add(child);
                }

                return;
            }

            UIAnimator[] children =
                GetComponentsInChildren<UIAnimator>(
                    includeInactiveChildren);

            for (int i = 0; i < children.Length; i++)
            {
                UIAnimator child = children[i];

                if (child == null)
                    continue;

                if (child == panelAnimator)
                    continue;

                if (!discoveredChildren.Contains(child))
                    discoveredChildren.Add(child);
            }
        }

        // ============================================================
        // SHOW
        // ============================================================

        public void Show()
        {
            Show(false);
        }

        public void Show(bool immediate)
        {
            CacheReferences();

            if (panelAnimator == null)
                return;

            KillSequence();

            gameObject.SetActive(true);

            if (immediate)
            {
                ShowImmediate();
                return;
            }

            panelAnimator.Show(false);

            if (!animateChildren ||
                discoveredChildren.Count == 0)
            {
                isShown = true;
                return;
            }

            panelSequence = DOTween.Sequence()
                .SetTarget(this)
                .SetUpdate(childrenIgnoreTimeScale);

            float accumulatedDelay = 0f;

            for (int i = 0;
                 i < discoveredChildren.Count;
                 i++)
            {
                UIAnimator child =
                    discoveredChildren[i];

                if (child == null)
                    continue;

                float delay =
                    GetChildDelay(i);

                panelSequence.InsertCallback(
                    accumulatedDelay + delay,
                    () => child.Show(false));

                accumulatedDelay += childStagger;
            }

            panelSequence.OnComplete(() =>
            {
                isShown = true;
                panelSequence = null;
            });
        }

        // ============================================================
        // HIDE
        // ============================================================

        public void Hide()
        {
            Hide(false);
        }

        public void Hide(bool immediate)
        {
            CacheReferences();

            if (panelAnimator == null)
                return;

            KillSequence();

            if (immediate)
            {
                HideImmediate();
                return;
            }

            if (animateChildren &&
                discoveredChildren.Count > 0)
            {
                panelSequence = DOTween.Sequence()
                    .SetTarget(this)
                    .SetUpdate(childrenIgnoreTimeScale);

                float accumulatedDelay = 0f;

                for (int i = discoveredChildren.Count - 1;
                     i >= 0;
                     i--)
                {
                    UIAnimator child =
                        discoveredChildren[i];

                    if (child == null)
                        continue;

                    float delay =
                        GetReverseChildDelay(i);

                    panelSequence.InsertCallback(
                        accumulatedDelay + delay,
                        () => child.Hide(false));

                    accumulatedDelay += childStagger;
                }

                panelSequence.AppendInterval(
                    GetChildAnimationBuffer());

                panelSequence.OnComplete(() =>
                {
                    panelAnimator.Hide(false);
                    isShown = false;
                    panelSequence = null;
                });
            }
            else
            {
                panelAnimator.Hide(false);
                isShown = false;
            }
        }

        // ============================================================
        // IMMEDIATE
        // ============================================================

        public void ShowImmediate()
        {
            KillSequence();

            gameObject.SetActive(true);

            panelAnimator.ShowImmediate();

            if (!animateChildren)
            {
                isShown = true;
                return;
            }

            for (int i = 0;
                 i < discoveredChildren.Count;
                 i++)
            {
                UIAnimator child =
                    discoveredChildren[i];

                if (child == null)
                    continue;

                child.ShowImmediate();
            }

            isShown = true;
        }

        public void HideImmediate()
        {
            KillSequence();

            for (int i = 0;
                 i < discoveredChildren.Count;
                 i++)
            {
                UIAnimator child =
                    discoveredChildren[i];

                if (child == null)
                    continue;

                child.HideImmediate();
            }

            panelAnimator.HideImmediate();

            isShown = false;
        }

        // ============================================================
        // CHILD TIMING
        // ============================================================

        private float GetChildDelay(int index)
        {
            if (useExplicitChildren &&
                index < explicitChildren.Count &&
                explicitChildren[index] != null)
            {
                return explicitChildren[index].Delay;
            }

            return 0f;
        }

        private float GetReverseChildDelay(int index)
        {
            if (useExplicitChildren)
            {
                int explicitIndex =
                    Mathf.Clamp(
                        index,
                        0,
                        explicitChildren.Count - 1);

                if (explicitIndex >= 0 &&
                    explicitIndex < explicitChildren.Count &&
                    explicitChildren[explicitIndex] != null)
                {
                    return explicitChildren[explicitIndex].Delay;
                }
            }

            return 0f;
        }

        private float GetChildAnimationBuffer()
        {
            if (discoveredChildren.Count <= 0)
                return 0f;

            return Mathf.Max(
                childStagger,
                0.01f);
        }

        // ============================================================
        // CONTROL
        // ============================================================

        public void Toggle()
        {
            if (isShown)
                Hide();
            else
                Show();
        }

        public void Toggle(bool immediate)
        {
            if (isShown)
                Hide(immediate);
            else
                Show(immediate);
        }

        public void Stop()
        {
            KillSequence();

            if (panelAnimator != null)
                panelAnimator.Kill();

            for (int i = 0;
                 i < discoveredChildren.Count;
                 i++)
            {
                UIAnimator child =
                    discoveredChildren[i];

                if (child != null)
                    child.Kill();
            }
        }

        public void ResetToHidden()
        {
            Stop();

            panelAnimator.HideImmediate();

            for (int i = 0;
                 i < discoveredChildren.Count;
                 i++)
            {
                UIAnimator child =
                    discoveredChildren[i];

                if (child != null)
                    child.HideImmediate();
            }

            isShown = false;
        }

        public void ResetToShown()
        {
            Stop();

            panelAnimator.ShowImmediate();

            for (int i = 0;
                 i < discoveredChildren.Count;
                 i++)
            {
                UIAnimator child =
                    discoveredChildren[i];

                if (child != null)
                    child.ShowImmediate();
            }

            isShown = true;
        }

        // ============================================================
        // CLEANUP
        // ============================================================

        private void KillSequence()
        {
            if (panelSequence == null)
                return;

            if (panelSequence.IsActive())
                panelSequence.Kill();

            panelSequence = null;
        }

        private void HideChildrenImmediate()
        {
            if (panelAnimator == null)
                return;

            if (!animateChildren)
                return;

            for (int i = 0;
                 i < discoveredChildren.Count;
                 i++)
            {
                UIAnimator child =
                    discoveredChildren[i];

                if (child != null)
                    child.HideImmediate();
            }
        }
    }
}