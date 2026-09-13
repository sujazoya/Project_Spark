using DG.Tweening;
using UnityEngine;

namespace ProjectSpark.UI.Animation
{
    /// <summary>
    /// Production utility layer for Project Spark UI animation.
    ///
    /// This class contains only stateless helper functionality.
    /// Animation ownership remains inside the individual animator components.
    /// </summary>
    public static class UIAnimationUtility
    {
        // =========================================================
        // CANVAS GROUP
        // =========================================================

        public static CanvasGroup GetOrAddCanvasGroup(
            GameObject target)
        {
            if (target == null)
                return null;

            if (target.TryGetComponent(
                    out CanvasGroup group))
            {
                return group;
            }

            return target.AddComponent<CanvasGroup>();
        }


        // =========================================================
        // TWEEN MANAGEMENT
        // =========================================================

        public static void Kill(Component target)
        {
            if (target == null)
                return;

            DOTween.Kill(target);
        }

        public static void Kill(GameObject target)
        {
            if (target == null)
                return;

            DOTween.Kill(target);
        }

        public static void Kill(ref Tween tween)
        {
            if (tween == null)
                return;

            if (tween.IsActive())
                tween.Kill();

            tween = null;
        }

        public static void Kill(ref Sequence sequence)
        {
            if (sequence == null)
                return;

            if (sequence.IsActive())
                sequence.Kill();

            sequence = null;
        }


        // =========================================================
        // INTERACTION
        // =========================================================

        public static void SetInteractable(
            CanvasGroup canvasGroup,
            bool value)
        {
            if (canvasGroup == null)
                return;

            canvasGroup.interactable = value;
            canvasGroup.blocksRaycasts = value;
        }

        public static void EnableInteraction(
            CanvasGroup canvasGroup)
        {
            SetInteractable(
                canvasGroup,
                true);
        }

        public static void DisableInteraction(
            CanvasGroup canvasGroup)
        {
            SetInteractable(
                canvasGroup,
                false);
        }


        // =========================================================
        // VISIBILITY
        // =========================================================

        public static void SetShown(
            RectTransform target,
            CanvasGroup canvasGroup,
            Vector2 position,
            Vector3 scale)
        {
            if (target == null)
                return;

            target.anchoredPosition = position;
            target.localScale = scale;

            if (canvasGroup == null)
                return;

            canvasGroup.alpha = 1f;

            EnableInteraction(canvasGroup);
        }


        public static void SetHidden(
            RectTransform target,
            CanvasGroup canvasGroup,
            Vector2 shownPosition,
            Vector2 offset,
            Vector3 shownScale,
            Vector3 hiddenScale,
            float alpha)
        {
            if (target == null)
                return;

            target.anchoredPosition =
                shownPosition + offset;

            target.localScale =
                hiddenScale;

            if (canvasGroup == null)
                return;

            canvasGroup.alpha =
                Mathf.Clamp01(alpha);

            DisableInteraction(canvasGroup);
        }


        // =========================================================
        // TRANSFORM
        // =========================================================

        public static void SetPosition(
            RectTransform target,
            Vector2 position)
        {
            if (target == null)
                return;

            target.anchoredPosition = position;
        }

        public static void SetScale(
            RectTransform target,
            Vector3 scale)
        {
            if (target == null)
                return;

            target.localScale = scale;
        }

        public static void ResetScale(
            RectTransform target)
        {
            if (target == null)
                return;

            target.localScale = Vector3.one;
        }


        // =========================================================
        // ALPHA
        // =========================================================

        public static void SetAlpha(
            CanvasGroup canvasGroup,
            float alpha)
        {
            if (canvasGroup == null)
                return;

            canvasGroup.alpha =
                Mathf.Clamp01(alpha);
        }


        // =========================================================
        // RESET
        // =========================================================

        public static void ResetToShown(
            RectTransform target,
            CanvasGroup canvasGroup,
            Vector2 position,
            Vector3 scale)
        {
            SetShown(
                target,
                canvasGroup,
                position,
                scale);
        }


        public static void ResetToHidden(
            RectTransform target,
            CanvasGroup canvasGroup,
            Vector2 position,
            Vector3 scale)
        {
            if (target == null)
                return;

            target.anchoredPosition = position;
            target.localScale = scale;

            if (canvasGroup == null)
                return;

            canvasGroup.alpha = 0f;

            DisableInteraction(canvasGroup);
        }


        // =========================================================
        // SAFE HELPERS
        // =========================================================

        public static bool IsValid(
            RectTransform target)
        {
            return target != null;
        }

        public static bool IsValid(
            CanvasGroup group)
        {
            return group != null;
        }


        // =========================================================
        // SNAP
        // =========================================================

        public static void Snap(
            RectTransform target,
            Vector2 position,
            Vector3 scale)
        {
            if (target == null)
                return;

            target.anchoredPosition = position;
            target.localScale = scale;
        }


        // =========================================================
        // SAFE ACTIVE STATE
        // =========================================================

        public static void SetActive(
            GameObject target,
            bool active)
        {
            if (target == null)
                return;

            if (target.activeSelf != active)
                target.SetActive(active);
        }
    }
}