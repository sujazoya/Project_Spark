using UnityEngine;

namespace ProjectSpark.UI.Animation
{
    [CreateAssetMenu(
        fileName = "UIAnimationProfile",
        menuName = "ProjectSpark/UI/Animation Profile")]
    public sealed class UIAnimationProfile :
        ScriptableObject
    {
        [Header("Timing")]

        [Min(0f)]
        public float duration = 0.25f;

        [Min(0f)]
        public float delay = 0f;

        [Header("Transform")]

        public Vector3 startScale =
            new Vector3(
                0.96f,
                0.96f,
                1f);

        public Vector3 endScale =
            Vector3.one;

        public Vector2 startOffset =
            Vector2.zero;

        public Vector2 endOffset =
            Vector2.zero;

        [Header("Alpha")]

        [Range(0f, 1f)]
        public float startAlpha = 0f;

        [Range(0f, 1f)]
        public float endAlpha = 1f;

        [Header("Easing")]

        public AnimationCurve curve =
            AnimationCurve.EaseInOut(
                0f,
                0f,
                1f,
                1f);
    }
}