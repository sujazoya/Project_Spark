using System.Collections;
using UnityEngine;
using UnityEngine.UI;



namespace ProjectSpark.UI.Transitions
{
    
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Graphic))]
    [RequireComponent(typeof(RectTransform))]
    public sealed class PS_UI_TransitionController : MonoBehaviour
    {

        private enum TransitionPreset
        {
            Clean,
            Technical,
            Boot,
            Glitch,
            Warning,
            Diagnostic
        }

        private enum TransitionDirection
        {
            LeftToRight,
            RightToLeft,
            BottomToTop,
            TopToBottom
        }

        private enum PlaybackState
        {
            Hidden,
            Entering,
            Visible,
            Exiting
        }
        [Header("Preset")]
[SerializeField]
private TransitionPreset preset =
    TransitionPreset.Clean;

        [System.Serializable]
        private sealed class AnimationSettings
        {
            [Min(0.01f)]
            public float duration = 0.45f;

            [Min(0f)]
            public float delay = 0f;

            public AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

            public AnimationCurve positionCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

            public AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

            public AnimationCurve rotationCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

            public AnimationCurve alphaCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        }

        [System.Serializable]
        private sealed class ShaderSettings
        {
            [Header("Core")]
            public Color baseColor = Color.white;

            [Range(0f, 1f)]
            public float revealSoftness = 0.06f;

            public float revealOffset = 0f;

            [Min(0.01f)]
            public float revealCurve = 1.5f;

            [Header("Scan")]
            [Min(0f)]
            public float scanWidth = 0.025f;

            [Min(0f)]
            public float scanIntensity = 1.5f;

            [Min(0f)]
            public float scanTrail = 0.12f;

            [Min(0.01f)]
            public float scanTrailPower = 2f;

            [Header("Edge")]
            [Min(0f)]
            public float edgeWidth = 0.015f;

            [Min(0f)]
            public float edgeIntensity = 2f;

            [Min(0.01f)]
            public float edgeSharpness = 3f;

            public Color edgeColor = new Color(0f, 0.9f, 1f, 1f);

            [Header("Scan Color")]
            public Color scanColor = new Color(0f, 0.95f, 1f, 1f);

            [Header("Dissolve")]
            [Range(0f, 1f)]
            public float dissolveAmount = 0f;

            [Range(0.0001f, 1f)]
            public float dissolveSoftness = 0.08f;

            [Min(0.01f)]
            public float dissolveScale = 12f;

            [Header("Glitch")]
            [Range(0f, 1f)]
            public float glitchAmount = 0f;

            [Min(0f)]
            public float glitchSpeed = 12f;

            [Min(0.01f)]
            public float glitchBlockSize = 18f;

            [Range(0f, 1f)]
            public float glitchThreshold = 0.82f;

            [Min(0f)]
            public float glitchOffset = 0.025f;

           // [Min(-10000f, 10000f)]
            public float glitchSeed = 0f;

            public Color glitchColor = Color.white;

            [Header("Chromatic")]
            [Min(0f)]
            public float chromaticAmount = 0f;

            [Header("Flicker")]
            [Range(0f, 1f)]
            public float flickerAmount = 0f;

            [Min(0f)]
            public float flickerSpeed = 15f;
        }

        [Header("References")]
        [SerializeField]
        private Graphic targetGraphic;

        [SerializeField]
        private RectTransform targetRectTransform;

        [SerializeField]
        private CanvasGroup canvasGroup;

        [Header("Startup")]
        [SerializeField]
        private bool playOnEnable = false;

        [SerializeField]
        private bool startHidden = true;

        [SerializeField]
        private bool instantiateMaterial = true;

        [SerializeField]
        private bool useUnscaledTime = true;

        [Header("Direction")]
        [SerializeField]
        private TransitionDirection direction = TransitionDirection.LeftToRight;

        [Header("Enter")]
        [SerializeField]
        private AnimationSettings enter = new AnimationSettings();

        [Header("Exit")]
        [SerializeField]
        private AnimationSettings exit = new AnimationSettings();

        [Header("Transform Motion")]
        [SerializeField]
        private bool animatePosition = true;

        [SerializeField]
        private bool animateScale = true;

        [SerializeField]
        private bool animateRotation = false;

        [SerializeField]
        private Vector2 positionOffset = new Vector2(120f, 0f);

        [SerializeField]
        private Vector3 hiddenScale = new Vector3(0.92f, 0.92f, 1f);

        [SerializeField]
        private Vector3 hiddenRotation = Vector3.zero;

        [SerializeField]
        private bool preserveInitialTransform = true;

        [Header("Alpha")]
        [SerializeField]
        private bool animateCanvasGroup = true;

        [SerializeField]
        [Range(0f, 1f)]
        private float hiddenAlpha = 0f;

        [Header("Automatic Glitch")]
        [SerializeField]
        private bool useGlitchEnvelope = true;

        [SerializeField]
        private AnimationCurve glitchEnvelope = new AnimationCurve(
            new Keyframe(0f, 0f),
            new Keyframe(0.15f, 0.15f),
            new Keyframe(0.45f, 0.5f),
            new Keyframe(0.65f, 0.15f),
            new Keyframe(1f, 0f));

        [SerializeField]
        [Range(0f, 1f)]
        private float glitchPeakAmount = 0.35f;

        [Header("Shader Values")]
        [SerializeField]
        private ShaderSettings shader = new ShaderSettings();

        private Material runtimeMaterial;

        private Coroutine transitionRoutine;

        private PlaybackState playbackState = PlaybackState.Hidden;

        private Vector2 originalAnchoredPosition;

        private Vector3 originalLocalScale;

        private Quaternion originalLocalRotation;

        private float originalCanvasAlpha = 1f;

        private bool initialized;

        private static readonly int BaseColorId =
            Shader.PropertyToID("_BaseColor");

        private static readonly int TransitionId =
            Shader.PropertyToID("_Transition");

        private static readonly int DirectionId =
            Shader.PropertyToID("_Direction");

        private static readonly int RevealSoftnessId =
            Shader.PropertyToID("_RevealSoftness");

        private static readonly int RevealOffsetId =
            Shader.PropertyToID("_RevealOffset");

        private static readonly int RevealCurveId =
            Shader.PropertyToID("_RevealCurve");

        private static readonly int ScanWidthId =
            Shader.PropertyToID("_ScanWidth");

        private static readonly int ScanIntensityId =
            Shader.PropertyToID("_ScanIntensity");

        private static readonly int ScanTrailId =
            Shader.PropertyToID("_ScanTrail");

        private static readonly int ScanTrailPowerId =
            Shader.PropertyToID("_ScanTrailPower");

        private static readonly int EdgeWidthId =
            Shader.PropertyToID("_EdgeWidth");

        private static readonly int EdgeIntensityId =
            Shader.PropertyToID("_EdgeIntensity");

        private static readonly int EdgeSharpnessId =
            Shader.PropertyToID("_EdgeSharpness");

        private static readonly int EdgeColorId =
            Shader.PropertyToID("_EdgeColor");

        private static readonly int ScanColorId =
            Shader.PropertyToID("_ScanColor");

        private static readonly int DissolveAmountId =
            Shader.PropertyToID("_DissolveAmount");

        private static readonly int DissolveSoftnessId =
            Shader.PropertyToID("_DissolveSoftness");

        private static readonly int DissolveScaleId =
            Shader.PropertyToID("_DissolveScale");

        private static readonly int GlitchAmountId =
            Shader.PropertyToID("_GlitchAmount");

        private static readonly int GlitchSpeedId =
            Shader.PropertyToID("_GlitchSpeed");

        private static readonly int GlitchBlockSizeId =
            Shader.PropertyToID("_GlitchBlockSize");

        private static readonly int GlitchThresholdId =
            Shader.PropertyToID("_GlitchThreshold");

        private static readonly int GlitchOffsetId =
            Shader.PropertyToID("_GlitchOffset");

        private static readonly int GlitchSeedId =
            Shader.PropertyToID("_GlitchSeed");

        private static readonly int GlitchColorId =
            Shader.PropertyToID("_GlitchColor");

        private static readonly int ChromaticAmountId =
            Shader.PropertyToID("_ChromaticAmount");

        private static readonly int FlickerAmountId =
            Shader.PropertyToID("_FlickerAmount");

        private static readonly int FlickerSpeedId =
            Shader.PropertyToID("_FlickerSpeed");

        public bool IsVisible
        {
            get
            {
                return playbackState == PlaybackState.Visible;
            }
        }

        public bool IsTransitioning
        {
            get
            {
                return playbackState == PlaybackState.Entering ||
                       playbackState == PlaybackState.Exiting;
            }
        }

        public float CurrentTransition
        {
            get
            {
                if (runtimeMaterial == null)
                {
                    return 0f;
                }

                if (!runtimeMaterial.HasProperty(TransitionId))
                {
                    return 0f;
                }

                return runtimeMaterial.GetFloat(TransitionId);
            }
        }

        private void Reset()
        {
            targetGraphic = GetComponent<Graphic>();
            targetRectTransform = GetComponent<RectTransform>();

            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
            }
        }

        private void Awake()
        {
            Initialize();
        }

        private void OnEnable()
        {
            if (!initialized)
            {
                Initialize();
            }

            if (playOnEnable)
            {
                PlayEnter();
            }
        }

        private void OnDisable()
        {
            StopTransition();
        }

        private void OnDestroy()
        {
            DestroyRuntimeMaterial();
        }

        private void Initialize()
        {
            if (initialized)
            {
                return;
            }

            if (targetGraphic == null)
            {
                targetGraphic = GetComponent<Graphic>();
            }

            if (targetRectTransform == null)
            {
                targetRectTransform = GetComponent<RectTransform>();
            }

            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
            }

            if (targetGraphic == null || targetRectTransform == null)
            {
                enabled = false;
                return;
            }

            originalAnchoredPosition =
                targetRectTransform.anchoredPosition;

            originalLocalScale =
                targetRectTransform.localScale;

            originalLocalRotation =
                targetRectTransform.localRotation;

            if (canvasGroup != null)
            {
                originalCanvasAlpha = canvasGroup.alpha;
            }

            if (instantiateMaterial)
            {
                CreateRuntimeMaterial();
            }
            else
            {
                runtimeMaterial = targetGraphic.material;
            }
            ApplyPreset();
           
            ApplyDirection();
             ApplyStaticShaderSettings();

            if (startHidden)
            {
                ApplyStateImmediately(0f, false);
                playbackState = PlaybackState.Hidden;
            }
            else
            {
                ApplyStateImmediately(1f, true);
                playbackState = PlaybackState.Visible;
            }
            

            initialized = true;

        }

        private void CreateRuntimeMaterial()
        {
            Material sourceMaterial = targetGraphic.material;

            if (sourceMaterial == null)
            {
                return;
            }

            runtimeMaterial = new Material(sourceMaterial);
            runtimeMaterial.name = sourceMaterial.name + " [Runtime]";

            targetGraphic.material = runtimeMaterial;
        }

        private void DestroyRuntimeMaterial()
        {
            if (runtimeMaterial == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(runtimeMaterial);
            }
            else
            {
                DestroyImmediate(runtimeMaterial);
            }

            runtimeMaterial = null;
        }

        private void ApplyStaticShaderSettings()
        {
            if (runtimeMaterial == null)
            {
                return;
            }

            SetColor(BaseColorId, shader.baseColor);
            SetFloat(RevealSoftnessId, shader.revealSoftness);
            SetFloat(RevealOffsetId, shader.revealOffset);
            SetFloat(RevealCurveId, shader.revealCurve);

            SetFloat(ScanWidthId, shader.scanWidth);
            SetFloat(ScanIntensityId, shader.scanIntensity);
            SetFloat(ScanTrailId, shader.scanTrail);
            SetFloat(ScanTrailPowerId, shader.scanTrailPower);

            SetFloat(EdgeWidthId, shader.edgeWidth);
            SetFloat(EdgeIntensityId, shader.edgeIntensity);
            SetFloat(EdgeSharpnessId, shader.edgeSharpness);
            SetColor(EdgeColorId, shader.edgeColor);

            SetColor(ScanColorId, shader.scanColor);

            SetFloat(DissolveAmountId, shader.dissolveAmount);
            SetFloat(DissolveSoftnessId, shader.dissolveSoftness);
            SetFloat(DissolveScaleId, shader.dissolveScale);

            SetFloat(GlitchAmountId, shader.glitchAmount);
            SetFloat(GlitchSpeedId, shader.glitchSpeed);
            SetFloat(GlitchBlockSizeId, shader.glitchBlockSize);
            SetFloat(GlitchThresholdId, shader.glitchThreshold);
            SetFloat(GlitchOffsetId, shader.glitchOffset);
            SetFloat(GlitchSeedId, shader.glitchSeed);
            SetColor(GlitchColorId, shader.glitchColor);

            SetFloat(ChromaticAmountId, shader.chromaticAmount);

            SetFloat(FlickerAmountId, shader.flickerAmount);
            SetFloat(FlickerSpeedId, shader.flickerSpeed);
        }

        private void ApplyDirection()
        {
            if (runtimeMaterial == null)
            {
                return;
            }

            Vector2 directionVector = GetDirectionVector();

            runtimeMaterial.SetVector(
                DirectionId,
                new Vector4(
                    directionVector.x,
                    directionVector.y,
                    0f,
                    0f));
        }

        private Vector2 GetDirectionVector()
        {
            switch (direction)
            {
                case TransitionDirection.LeftToRight:
                    return Vector2.right;

                case TransitionDirection.RightToLeft:
                    return Vector2.left;

                case TransitionDirection.BottomToTop:
                    return Vector2.up;

                case TransitionDirection.TopToBottom:
                    return Vector2.down;

                default:
                    return Vector2.right;
            }
        }

        private Vector2 GetEnterPositionOffset()
        {
            Vector2 normalizedDirection = GetDirectionVector();

            Vector2 configuredOffset = positionOffset;

            if (configuredOffset.sqrMagnitude <= 0.0001f)
            {
                configuredOffset =
                    normalizedDirection * 120f;
            }

            float horizontalMagnitude =
                Mathf.Abs(configuredOffset.x);

            float verticalMagnitude =
                Mathf.Abs(configuredOffset.y);

            if (Mathf.Abs(normalizedDirection.x) > 0.5f)
            {
                horizontalMagnitude =
                    Mathf.Max(horizontalMagnitude, 1f);

                return new Vector2(
                    -normalizedDirection.x * horizontalMagnitude,
                    0f);
            }

            verticalMagnitude =
                Mathf.Max(verticalMagnitude, 1f);

            return new Vector2(
                0f,
                -normalizedDirection.y * verticalMagnitude);
        }

        private float EvaluateCurve(
            AnimationCurve curve,
            float normalizedTime)
        {
            if (curve == null || curve.length == 0)
            {
                return normalizedTime;
            }

            return Mathf.Clamp01(curve.Evaluate(normalizedTime));
        }

        public void PlayEnter()
        {
            Initialize();

            if (!enabled)
            {
                return;
            }

            StopTransition();

            transitionRoutine =
                StartCoroutine(
                    RunTransition(
                        true));
        }

        public void PlayExit()
        {
            Initialize();

            if (!enabled)
            {
                return;
            }

            StopTransition();

            transitionRoutine =
                StartCoroutine(
                    RunTransition(
                        false));
        }

        public void Toggle()
        {
            if (IsVisible || playbackState == PlaybackState.Entering)
            {
                PlayExit();
            }
            else
            {
                PlayEnter();
            }
        }

        public void SetVisibleImmediate()
        {
            Initialize();

            StopTransition();

            ApplyStateImmediately(
                1f,
                true);

            playbackState =
                PlaybackState.Visible;
        }

        public void SetHiddenImmediate()
        {
            Initialize();

            StopTransition();

            ApplyStateImmediately(
                0f,
                false);

            playbackState =
                PlaybackState.Hidden;
        }

        public void SetTransitionProgress(
            float progress)
        {
            Initialize();

            progress =
                Mathf.Clamp01(progress);

            ApplyStateImmediately(
                progress,
                progress > 0f);
        }

        public void SetDirectionLeftToRight()
        {
            SetDirection(
                TransitionDirection.LeftToRight);
        }

        public void SetDirectionRightToLeft()
        {
            SetDirection(
                TransitionDirection.RightToLeft);
        }

        public void SetDirectionBottomToTop()
        {
            SetDirection(
                TransitionDirection.BottomToTop);
        }

        public void SetDirectionTopToBottom()
        {
            SetDirection(
                TransitionDirection.TopToBottom);
        }

        private void SetDirection(
            TransitionDirection newDirection)
        {
            direction = newDirection;
            ApplyDirection();

            if (playbackState == PlaybackState.Hidden)
            {
                ApplyStateImmediately(
                    0f,
                    false);
            }
        }

        private IEnumerator RunTransition(
            bool entering)
        {
            AnimationSettings settings =
                entering
                    ? enter
                    : exit;

            playbackState =
                entering
                    ? PlaybackState.Entering
                    : PlaybackState.Exiting;

            ApplyDirection();

            if (settings.delay > 0f)
            {
                float delayTimer = 0f;

                while (delayTimer < settings.delay)
                {
                    delayTimer += GetDeltaTime();

                    yield return null;
                }
            }

            float startProgress =
                entering
                    ? CurrentTransition
                    : CurrentTransition;

            float targetProgress =
                entering
                    ? 1f
                    : 0f;

            if (float.IsNaN(startProgress) ||
                float.IsInfinity(startProgress))
            {
                startProgress =
                    entering
                        ? 0f
                        : 1f;
            }

            float totalDistance =
                Mathf.Abs(
                    targetProgress -
                    startProgress);

            if (totalDistance <= 0.0001f)
            {
                ApplyStateImmediately(
                    targetProgress,
                    entering);

                playbackState =
                    entering
                        ? PlaybackState.Visible
                        : PlaybackState.Hidden;

                transitionRoutine = null;

                yield break;
            }

            float elapsed = 0f;

            while (elapsed < settings.duration)
            {
                elapsed += GetDeltaTime();

                float rawT =
                    Mathf.Clamp01(
                        elapsed /
                        settings.duration);

                float easedT =
                    EvaluateCurve(
                        settings.transitionCurve,
                        rawT);

                float progress =
                    Mathf.Lerp(
                        startProgress,
                        targetProgress,
                        easedT);

                ApplyAnimatedState(
                    progress,
                    rawT,
                    easedT,
                    entering,
                    settings);

                yield return null;
            }

            ApplyAnimatedState(
                targetProgress,
                1f,
                1f,
                entering,
                settings);

            playbackState =
                entering
                    ? PlaybackState.Visible
                    : PlaybackState.Hidden;

            transitionRoutine = null;
        }

        private void ApplyAnimatedState(
            float progress,
            float rawTime,
            float easedTime,
            bool entering,
            AnimationSettings settings)
        {
            ApplyShaderTransition(
                progress,
                easedTime,
                entering);

            ApplyTransformAnimation(
                easedTime,
                entering,
                settings);

            ApplyAlphaAnimation(
                easedTime,
                entering,
                settings);
        }

        private void ApplyShaderTransition(
            float progress,
            float normalizedTime,
            bool entering)
        {
            if (runtimeMaterial == null)
            {
                return;
            }

            runtimeMaterial.SetFloat(
                TransitionId,
                Mathf.Clamp01(progress));

            float envelope =
                0f;

            if (useGlitchEnvelope)
            {
                envelope =
                    EvaluateCurve(
                        glitchEnvelope,
                        normalizedTime);
            }

            float glitchValue =
                shader.glitchAmount;

            if (useGlitchEnvelope)
            {
                glitchValue *= envelope;
            }

            glitchValue =
                Mathf.Clamp01(
                    Mathf.Max(
                        glitchValue,
                        envelope *
                        glitchPeakAmount));

            if (!entering)
            {
                glitchValue *=
                    Mathf.Clamp01(
                        1f - normalizedTime * 0.35f);
            }

            runtimeMaterial.SetFloat(
                GlitchAmountId,
                glitchValue);
        }

        private void ApplyTransformAnimation(
            float normalizedTime,
            bool entering,
            AnimationSettings settings)
        {
            if (targetRectTransform == null)
            {
                return;
            }

            float positionT =
                EvaluateCurve(
                    settings.positionCurve,
                    normalizedTime);

            float scaleT =
                EvaluateCurve(
                    settings.scaleCurve,
                    normalizedTime);

            float rotationT =
                EvaluateCurve(
                    settings.rotationCurve,
                    normalizedTime);

            if (entering)
            {
                ApplyEnterTransform(
                    positionT,
                    scaleT,
                    rotationT);
            }
            else
            {
                ApplyExitTransform(
                    positionT,
                    scaleT,
                    rotationT);
            }
        }

        private void ApplyEnterTransform(
            float positionT,
            float scaleT,
            float rotationT)
        {
            if (animatePosition)
            {
                Vector2 hiddenPosition =
                    originalAnchoredPosition +
                    GetEnterPositionOffset();

                targetRectTransform.anchoredPosition =
                    Vector2.LerpUnclamped(
                        hiddenPosition,
                        originalAnchoredPosition,
                        positionT);
            }
            else
            {
                targetRectTransform.anchoredPosition =
                    originalAnchoredPosition;
            }

            if (animateScale)
            {
                targetRectTransform.localScale =
                    Vector3.LerpUnclamped(
                        hiddenScale,
                        originalLocalScale,
                        scaleT);
            }
            else
            {
                targetRectTransform.localScale =
                    originalLocalScale;
            }

            if (animateRotation)
            {
                Quaternion hiddenRotationQuaternion =
                    Quaternion.Euler(
                        hiddenRotation);

                targetRectTransform.localRotation =
                    Quaternion.SlerpUnclamped(
                        hiddenRotationQuaternion,
                        originalLocalRotation,
                        rotationT);
            }
            else
            {
                targetRectTransform.localRotation =
                    originalLocalRotation;
            }
        }

        private void ApplyExitTransform(
            float exitT,
            float scaleT,
            float rotationT)
        {
            Vector2 hiddenPosition =
                originalAnchoredPosition +
                GetEnterPositionOffset();

            if (animatePosition)
            {
                targetRectTransform.anchoredPosition =
                    Vector2.LerpUnclamped(
                        originalAnchoredPosition,
                        hiddenPosition,
                        exitT);
            }
            else
            {
                targetRectTransform.anchoredPosition =
                    originalAnchoredPosition;
            }

            if (animateScale)
            {
                targetRectTransform.localScale =
                    Vector3.LerpUnclamped(
                        originalLocalScale,
                        hiddenScale,
                        scaleT);
            }
            else
            {
                targetRectTransform.localScale =
                    originalLocalScale;
            }

            if (animateRotation)
            {
                Quaternion hiddenRotationQuaternion =
                    Quaternion.Euler(
                        hiddenRotation);

                targetRectTransform.localRotation =
                    Quaternion.SlerpUnclamped(
                        originalLocalRotation,
                        hiddenRotationQuaternion,
                        rotationT);
            }
            else
            {
                targetRectTransform.localRotation =
                    originalLocalRotation;
            }
        }

        private void ApplyAlphaAnimation(
            float normalizedTime,
            bool entering,
            AnimationSettings settings)
        {
            if (!animateCanvasGroup ||
                canvasGroup == null)
            {
                return;
            }

            float alphaT =
                EvaluateCurve(
                    settings.alphaCurve,
                    normalizedTime);

            if (entering)
            {
                canvasGroup.alpha =
                    Mathf.Lerp(
                        hiddenAlpha,
                        originalCanvasAlpha,
                        alphaT);
            }
            else
            {
                canvasGroup.alpha =
                    Mathf.Lerp(
                        originalCanvasAlpha,
                        hiddenAlpha,
                        alphaT);
            }
        }

        private void ApplyStateImmediately(
            float progress,
            bool visible)
        {
            if (runtimeMaterial != null)
            {
                runtimeMaterial.SetFloat(
                    TransitionId,
                    Mathf.Clamp01(progress));

                runtimeMaterial.SetFloat(
                    GlitchAmountId,
                    0f);
            }

            if (visible)
            {
                if (animatePosition)
                {
                    targetRectTransform.anchoredPosition =
                        originalAnchoredPosition;
                }

                if (animateScale)
                {
                    targetRectTransform.localScale =
                        originalLocalScale;
                }

                if (animateRotation)
                {
                    targetRectTransform.localRotation =
                        originalLocalRotation;
                }

                if (animateCanvasGroup &&
                    canvasGroup != null)
                {
                    canvasGroup.alpha =
                        originalCanvasAlpha;
                }
            }
            else
            {
                if (animatePosition)
                {
                    targetRectTransform.anchoredPosition =
                        originalAnchoredPosition +
                        GetEnterPositionOffset();
                }

                if (animateScale)
                {
                    targetRectTransform.localScale =
                        hiddenScale;
                }

                if (animateRotation)
                {
                    targetRectTransform.localRotation =
                        Quaternion.Euler(
                            hiddenRotation);
                }

                if (animateCanvasGroup &&
                    canvasGroup != null)
                {
                    canvasGroup.alpha =
                        hiddenAlpha;
                }
            }
        }

        public void PlayGlitchBurst()
        {
            if (!initialized)
            {
                Initialize();
            }

            if (runtimeMaterial == null)
            {
                return;
            }

            StartCoroutine(
                RunGlitchBurst());
        }

        public void PlayGlitchBurst(
            float amount,
            float duration)
        {
            if (!initialized)
            {
                Initialize();
            }

            if (runtimeMaterial == null)
            {
                return;
            }

            StartCoroutine(
                RunGlitchBurst(
                    amount,
                    duration));
        }

        private IEnumerator RunGlitchBurst()
        {
            yield return
                RunGlitchBurst(
                    glitchPeakAmount,
                    0.18f);
        }

        private IEnumerator RunGlitchBurst(
            float amount,
            float duration)
        {
            amount =
                Mathf.Clamp01(
                    amount);

            duration =
                Mathf.Max(
                    0.01f,
                    duration);

            float originalAmount =
                shader.glitchAmount;

            float halfDuration =
                duration * 0.5f;

            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += GetDeltaTime();

                float t =
                    Mathf.Clamp01(
                        elapsed /
                        duration);

                float burst;

                if (t <= 0.5f)
                {
                    burst =
                        Mathf.SmoothStep(
                            0f,
                            1f,
                            t * 2f);
                }
                else
                {
                    burst =
                        Mathf.SmoothStep(
                            1f,
                            0f,
                            (t - 0.5f) * 2f);
                }

                float finalAmount =
                    Mathf.Clamp01(
                        Mathf.Max(
                            originalAmount,
                            amount * burst));

                runtimeMaterial.SetFloat(
                    GlitchAmountId,
                    finalAmount);

                yield return null;
            }

            runtimeMaterial.SetFloat(
                GlitchAmountId,
                originalAmount);
        }

        public void SetGlitchAmount(
            float amount)
        {
            shader.glitchAmount =
                Mathf.Clamp01(
                    amount);

            SetFloat(
                GlitchAmountId,
                shader.glitchAmount);
        }

        public void SetGlitchSeed(
            float seed)
        {
            shader.glitchSeed =
                seed;

            SetFloat(
                GlitchSeedId,
                seed);
        }

        public void SetScanIntensity(
            float intensity)
        {
            shader.scanIntensity =
                Mathf.Max(
                    0f,
                    intensity);

            SetFloat(
                ScanIntensityId,
                shader.scanIntensity);
        }

        public void SetDissolveAmount(
            float amount)
        {
            shader.dissolveAmount =
                Mathf.Clamp01(
                    amount);

            SetFloat(
                DissolveAmountId,
                shader.dissolveAmount);
        }

        public void SetBaseColor(
            Color color)
        {
            shader.baseColor =
                color;

            SetColor(
                BaseColorId,
                color);
        }

        private float GetDeltaTime()
        {
            return useUnscaledTime
                ? Time.unscaledDeltaTime
                : Time.deltaTime;
        }

        private void StopTransition()
        {
            if (transitionRoutine == null)
            {
                return;
            }

            StopCoroutine(
                transitionRoutine);

            transitionRoutine = null;
        }

        private void SetFloat(
            int propertyId,
            float value)
        {
            if (runtimeMaterial == null)
            {
                return;
            }

            if (!runtimeMaterial.HasProperty(propertyId))
            {
                return;
            }

            runtimeMaterial.SetFloat(
                propertyId,
                value);
        }

        private void SetColor(
            int propertyId,
            Color value)
        {
            if (runtimeMaterial == null)
            {
                return;
            }

            if (!runtimeMaterial.HasProperty(propertyId))
            {
                return;
            }

            runtimeMaterial.SetColor(
                propertyId,
                value);
        }
        public void ApplyPreset()
        {
            switch (preset)
            {
                case TransitionPreset.Clean:
                    ApplyCleanPreset();
                    break;

                case TransitionPreset.Technical:
                    ApplyTechnicalPreset();
                    break;

                case TransitionPreset.Boot:
                    ApplyBootPreset();
                    break;

                case TransitionPreset.Glitch:
                    ApplyGlitchPreset();
                    break;

                case TransitionPreset.Warning:
                    ApplyWarningPreset();
                    break;

                case TransitionPreset.Diagnostic:
                    ApplyDiagnosticPreset();
                    break;
            }
        }
        private void ApplyCleanPreset()
        {
            shader.revealSoftness = 0.06f;
            shader.revealOffset = 0f;
            shader.revealCurve = 1.5f;

            shader.scanWidth = 0.015f;
            shader.scanIntensity = 0.8f;
            shader.scanTrail = 0.08f;
            shader.scanTrailPower = 2f;

            shader.edgeWidth = 0.012f;
            shader.edgeIntensity = 1.0f;
            shader.edgeSharpness = 3f;

            shader.dissolveAmount = 0f;

            shader.glitchAmount = 0f;
            shader.chromaticAmount = 0f;
            shader.flickerAmount = 0f;

           
        }
        private void ApplyTechnicalPreset()
        {
            shader.revealSoftness = 0.045f;
            shader.revealOffset = 0f;
            shader.revealCurve = 1.8f;

            shader.scanWidth = 0.025f;
            shader.scanIntensity = 1.8f;
            shader.scanTrail = 0.15f;
            shader.scanTrailPower = 2.2f;

            shader.edgeWidth = 0.018f;
            shader.edgeIntensity = 2.5f;
            shader.edgeSharpness = 3.5f;

            shader.dissolveAmount = 0f;

            shader.glitchAmount = 0.08f;
            shader.glitchSpeed = 12f;
            shader.glitchBlockSize = 18f;
            shader.glitchThreshold = 0.82f;
            shader.glitchOffset = 0.018f;

            shader.chromaticAmount = 0.003f;

            shader.flickerAmount = 0.025f;
            shader.flickerSpeed = 15f;

          
        }
        private void ApplyBootPreset()
        {
            shader.revealSoftness = 0.035f;
            shader.revealOffset = -0.02f;
            shader.revealCurve = 2.2f;

            shader.scanWidth = 0.04f;
            shader.scanIntensity = 4f;
            shader.scanTrail = 0.28f;
            shader.scanTrailPower = 1.8f;

            shader.edgeWidth = 0.02f;
            shader.edgeIntensity = 4f;
            shader.edgeSharpness = 4f;

            shader.dissolveAmount = 0.02f;

            shader.glitchAmount = 0.12f;
            shader.glitchSpeed = 18f;
            shader.glitchBlockSize = 24f;
            shader.glitchThreshold = 0.76f;
            shader.glitchOffset = 0.035f;

            shader.chromaticAmount = 0.006f;

            shader.flickerAmount = 0.08f;
            shader.flickerSpeed = 18f;

          
        }
       private void ApplyGlitchPreset()
        {
            shader.revealSoftness = 0.025f;
            shader.revealOffset = 0f;
            shader.revealCurve = 2.5f;

            shader.scanWidth = 0.035f;
            shader.scanIntensity = 3f;
            shader.scanTrail = 0.22f;
            shader.scanTrailPower = 1.7f;

            shader.edgeWidth = 0.02f;
            shader.edgeIntensity = 3.5f;
            shader.edgeSharpness = 4f;

            shader.dissolveAmount = 0.1f;

            shader.glitchAmount = 0.75f;
            shader.glitchSpeed = 22f;
            shader.glitchBlockSize = 28f;
            shader.glitchThreshold = 0.65f;
            shader.glitchOffset = 0.055f;

            shader.chromaticAmount = 0.015f;

            shader.flickerAmount = 0.12f;
            shader.flickerSpeed = 22f;

          
        }
        private void ApplyWarningPreset()
        {
            shader.revealSoftness = 0.035f;
            shader.revealOffset = 0f;
            shader.revealCurve = 1.7f;

            shader.scanWidth = 0.02f;
            shader.scanIntensity = 2.2f;
            shader.scanTrail = 0.12f;
            shader.scanTrailPower = 2f;

            shader.edgeWidth = 0.022f;
            shader.edgeIntensity = 4f;
            shader.edgeSharpness = 4f;

            shader.dissolveAmount = 0.03f;

            shader.glitchAmount = 0.2f;
            shader.glitchSpeed = 16f;
            shader.glitchBlockSize = 22f;
            shader.glitchThreshold = 0.72f;
            shader.glitchOffset = 0.03f;

            shader.chromaticAmount = 0.006f;

            shader.flickerAmount = 0.18f;
            shader.flickerSpeed = 20f;

          
        }
        private void ApplyDiagnosticPreset()
        {
            shader.revealSoftness = 0.05f;
            shader.revealOffset = 0f;
            shader.revealCurve = 1.4f;

            shader.scanWidth = 0.018f;
            shader.scanIntensity = 1.3f;
            shader.scanTrail = 0.1f;
            shader.scanTrailPower = 2.5f;

            shader.edgeWidth = 0.014f;
            shader.edgeIntensity = 1.8f;
            shader.edgeSharpness = 3.5f;

            shader.dissolveAmount = 0f;

            shader.glitchAmount = 0.05f;
            shader.glitchSpeed = 10f;
            shader.glitchBlockSize = 16f;
            shader.glitchThreshold = 0.88f;
            shader.glitchOffset = 0.012f;

            shader.chromaticAmount = 0.002f;

            shader.flickerAmount = 0.02f;
            shader.flickerSpeed = 12f;

           
        }
        public void RandomizeGlitchSeed()
        {
            shader.glitchSeed =
                Random.Range(
                    -10000f,
                    10000f);

            SetFloat(
                GlitchSeedId,
                shader.glitchSeed);
        }
    }
}