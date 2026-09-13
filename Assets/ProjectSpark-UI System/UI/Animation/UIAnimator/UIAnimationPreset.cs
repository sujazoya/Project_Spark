using DG.Tweening;
using UnityEngine;

namespace ProjectSpark.UI.Animation
{
    [CreateAssetMenu(
        fileName = "UIAnimationPreset",
        menuName = "Project Spark/UI/Animation Preset",
        order = 100)]
    public sealed class UIAnimationPreset : ScriptableObject
    {
        // =========================================================
        // IDENTITY
        // =========================================================

        [Header("IDENTITY")]

        [SerializeField]
        private string presetId;

        [TextArea(2, 4)]
        [SerializeField]
        private string description;

        public string PresetId => presetId;
        public string Description => description;


        // =========================================================
        // POSITION
        // =========================================================

        [Header("POSITION")]

        [SerializeField]
        private bool usePosition = true;

        [SerializeField]
        private Vector2 hiddenPositionOffset =
            new Vector2(0f, -30f);

        [Min(0f)]
        [SerializeField]
        private float positionDuration = 0.35f;

        [SerializeField]
        private Ease positionEase =
            Ease.OutCubic;

        public bool UsePosition =>
            usePosition;

        public Vector2 HiddenPositionOffset =>
            hiddenPositionOffset;

        public float PositionDuration =>
            positionDuration;

        public Ease PositionEase =>
            positionEase;


        // =========================================================
        // SCALE
        // =========================================================

        [Header("SCALE")]

        [SerializeField]
        private bool useScale = true;

        [SerializeField]
        private Vector3 hiddenScale =
            new Vector3(
                0.94f,
                0.94f,
                0.94f);

        [Min(0f)]
        [SerializeField]
        private float scaleDuration = 0.30f;

        [SerializeField]
        private Ease scaleEase =
            Ease.OutCubic;

        public bool UseScale =>
            useScale;

        public Vector3 HiddenScale =>
            hiddenScale;

        public float ScaleDuration =>
            scaleDuration;

        public Ease ScaleEase =>
            scaleEase;


        // =========================================================
        // ROTATION
        // =========================================================

        [Header("ROTATION")]

        [SerializeField]
        private bool useRotation;

        [SerializeField]
        private Vector3 hiddenRotation;

        [Min(0f)]
        [SerializeField]
        private float rotationDuration = 0.30f;

        [SerializeField]
        private Ease rotationEase =
            Ease.OutCubic;

        public bool UseRotation =>
            useRotation;

        public Vector3 HiddenRotation =>
            hiddenRotation;

        public float RotationDuration =>
            rotationDuration;

        public Ease RotationEase =>
            rotationEase;


        // =========================================================
        // ALPHA
        // =========================================================

        [Header("ALPHA")]

        [SerializeField]
        private bool useAlpha = true;

        [Range(0f, 1f)]
        [SerializeField]
        private float hiddenAlpha;

        [Min(0f)]
        [SerializeField]
        private float fadeDuration = 0.25f;

        [SerializeField]
        private Ease fadeEase =
            Ease.OutQuad;

        public bool UseAlpha =>
            useAlpha;

        public float HiddenAlpha =>
            hiddenAlpha;

        public float FadeDuration =>
            fadeDuration;

        public Ease FadeEase =>
            fadeEase;


        // =========================================================
        // DELAY
        // =========================================================

        [Header("TIMING")]

        [Min(0f)]
        [SerializeField]
        private float delay;

        [Min(0f)]
        [SerializeField]
        private float exitMultiplier = 0.70f;

        [SerializeField]
        private bool ignoreTimeScale = true;

        public float Delay =>
            delay;

        public float ExitMultiplier =>
            exitMultiplier;

        public bool IgnoreTimeScale =>
            ignoreTimeScale;


        // =========================================================
        // INTERACTION
        // =========================================================

        [Header("INTERACTION")]

        [SerializeField]
        private bool disableInteractionDuringAnimation = true;

        [SerializeField]
        private bool disableObjectAfterHide = true;

        public bool DisableInteractionDuringAnimation =>
            disableInteractionDuringAnimation;

        public bool DisableObjectAfterHide =>
            disableObjectAfterHide;


        // =========================================================
        // PUNCH
        // =========================================================

        [Header("PUNCH")]

        [SerializeField]
        private bool usePunch;

        [SerializeField]
        private Vector3 punchScale =
            new Vector3(
                0.035f,
                0.035f,
                0f);

        [Min(0.01f)]
        [SerializeField]
        private float punchDuration = 0.20f;

        [Range(1, 20)]
        [SerializeField]
        private int punchVibrato = 5;

        [Range(0f, 1f)]
        [SerializeField]
        private float punchElasticity = 0.65f;

        public bool UsePunch =>
            usePunch;

        public Vector3 PunchScale =>
            punchScale;

        public float PunchDuration =>
            punchDuration;

        public int PunchVibrato =>
            punchVibrato;

        public float PunchElasticity =>
            punchElasticity;


        // =========================================================
        // SHAKE
        // =========================================================

        [Header("SHAKE")]

        [SerializeField]
        private bool useShake;

        [SerializeField]
        private Vector3 shakeStrength =
            new Vector3(
                5f,
                5f,
                0f);

        [Min(0.01f)]
        [SerializeField]
        private float shakeDuration = 0.20f;

        [Range(1, 50)]
        [SerializeField]
        private int shakeVibrato = 12;

        [Range(0f, 180f)]
        [SerializeField]
        private float shakeRandomness = 90f;

        public bool UseShake =>
            useShake;

        public Vector3 ShakeStrength =>
            shakeStrength;

        public float ShakeDuration =>
            shakeDuration;

        public int ShakeVibrato =>
            shakeVibrato;

        public float ShakeRandomness =>
            shakeRandomness;


        // =========================================================
        // LOOP
        // =========================================================

        [Header("LOOP")]

        [SerializeField]
        private bool loop;

        [SerializeField]
        private int loopCount = -1;

        [SerializeField]
        private LoopType loopType =
            LoopType.Yoyo;

        public bool Loop =>
            loop;

        public int LoopCount =>
            loopCount;

        public LoopType LoopType =>
            loopType;


        // =========================================================
        // VALIDATION
        // =========================================================

#if UNITY_EDITOR

        private void OnValidate()
        {
            positionDuration =
                Mathf.Max(
                    0f,
                    positionDuration);

            scaleDuration =
                Mathf.Max(
                    0f,
                    scaleDuration);

            rotationDuration =
                Mathf.Max(
                    0f,
                    rotationDuration);

            fadeDuration =
                Mathf.Max(
                    0f,
                    fadeDuration);

            delay =
                Mathf.Max(
                    0f,
                    delay);

            exitMultiplier =
                Mathf.Max(
                    0.01f,
                    exitMultiplier);

            hiddenAlpha =
                Mathf.Clamp01(
                    hiddenAlpha);

            punchDuration =
                Mathf.Max(
                    0.01f,
                    punchDuration);

            shakeDuration =
                Mathf.Max(
                    0.01f,
                    shakeDuration);

            punchVibrato =
                Mathf.Clamp(
                    punchVibrato,
                    1,
                    20);

            shakeVibrato =
                Mathf.Clamp(
                    shakeVibrato,
                    1,
                    50);

            shakeRandomness =
                Mathf.Clamp(
                    shakeRandomness,
                    0f,
                    180f);
        }

#endif
    }
}