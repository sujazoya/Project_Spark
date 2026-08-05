using System.Collections;
using TMPro;
using UnityEngine;

namespace ProjectSpark.UI.VFX
{
    /// <summary>
    /// Project Spark VFX controller for TextMeshPro.
    ///
    /// Implements ISparkVFXController so it can be used by:
    /// - SparkVFXTarget
    /// - SparkVFXSequencePlayer
    /// - SparkVFXLoop
    /// - SparkVFXLayeredStateMachine
    ///
    /// This controller controls the TMP material only.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(TMP_Text))]
    public sealed class SparkTMPVFXController
        : MonoBehaviour,
          ISparkVFXController
    {
        // ============================================================
        // TARGET
        // ============================================================

        [Header("TMP Target")]

        [SerializeField]
        private TMP_Text targetText;


        // ============================================================
        // MATERIAL
        // ============================================================

        [Header("Material")]

        [SerializeField]
        private Material sourceMaterial;


        [SerializeField]
        private bool createRuntimeMaterial = true;


        private Material runtimeMaterial;


        // ============================================================
        // THEME
        // ============================================================

        [Header("Theme")]

        [SerializeField]
        private Color currentThemeColor =
            Color.white;


        // ============================================================
        // RUNTIME VALUES
        // ============================================================

        private float currentGlow;

        private float currentScan;

        private float currentSweep;

        private float currentSweepPosition;

        private float currentFlash;

        private float currentGlitch;

        private float currentFlicker;

        private float currentReveal;

        private float currentDissolve;


        // ============================================================
        // TRANSITION
        // ============================================================

        private Coroutine transitionRoutine;


        // ============================================================
        // INITIALIZATION
        // ============================================================

        private bool initialized;

        private bool initializing;


        // ============================================================
        // SHADER PROPERTY IDS
        // ============================================================

        private static readonly int VFXColorID =
            Shader.PropertyToID(
                "_VFXColor"
            );


        private static readonly int VFXGlowColorID =
            Shader.PropertyToID(
                "_VFXGlowColor"
            );


        private static readonly int VFXScanColorID =
            Shader.PropertyToID(
                "_VFXScanColor"
            );


        private static readonly int VFXSweepColorID =
            Shader.PropertyToID(
                "_VFXSweepColor"
            );


        private static readonly int VFXGlowID =
            Shader.PropertyToID(
                "_VFXGlow"
            );


        private static readonly int VFXScanID =
            Shader.PropertyToID(
                "_VFXScan"
            );


        private static readonly int VFXSweepID =
            Shader.PropertyToID(
                "_VFXSweep"
            );


        private static readonly int VFXSweepPositionID =
            Shader.PropertyToID(
                "_VFXSweepPosition"
            );


        private static readonly int VFXFlashID =
            Shader.PropertyToID(
                "_VFXFlash"
            );


        private static readonly int VFXGlitchID =
            Shader.PropertyToID(
                "_VFXGlitch"
            );


        private static readonly int VFXFlickerID =
            Shader.PropertyToID(
                "_VFXFlicker"
            );


        private static readonly int VFXRevealID =
            Shader.PropertyToID(
                "_VFXReveal"
            );


        private static readonly int VFXDissolveID =
            Shader.PropertyToID(
                "_VFXDissolve"
            );


        // ============================================================
        // PUBLIC INTERFACE
        // ============================================================

        public Color CurrentThemeColor
        {
            get
            {
                return currentThemeColor;
            }
        }


        public TMP_Text TargetText
        {
            get
            {
                return targetText;
            }
        }


        public Material RuntimeMaterial
        {
            get
            {
                EnsureInitialized();

                return runtimeMaterial;
            }
        }


        // ============================================================
        // UNITY
        // ============================================================

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
        }


        // ============================================================
        // INITIALIZE
        // ============================================================

        public void Initialize()
        {
            if (initialized)
            {
                return;
            }


            if (initializing)
            {
                return;
            }


            initializing =
                true;


            // --------------------------------------------------------
            // FIND TMP
            // --------------------------------------------------------

            if (targetText == null)
            {
                targetText =
                    GetComponent<TMP_Text>();
            }


            if (targetText == null)
            {
                targetText =
                    GetComponentInChildren<TMP_Text>(
                        true
                    );
            }


            if (targetText == null)
            {
                Debug.LogError(
                    "[SparkTMPVFXController] " +
                    "No TMP_Text found.",
                    this
                );


                initializing =
                    false;

                return;
            }


            // --------------------------------------------------------
            // CREATE MATERIAL
            // --------------------------------------------------------

            CreateRuntimeMaterial();


            // --------------------------------------------------------
            // INITIAL VALUES
            // --------------------------------------------------------

            if (runtimeMaterial != null)
            {
                currentGlow =
                    0f;

                currentScan =
                    0f;

                currentSweep =
                    0f;

                currentSweepPosition =
                    0.5f;

                currentFlash =
                    0f;

                currentGlitch =
                    0f;

                currentFlicker =
                    0f;

                currentReveal =
                    1f;

                currentDissolve =
                    0f;


                ApplyCurrentValues();
            }


            initialized =
                true;


            initializing =
                false;
        }


        // ============================================================
        // ENSURE INITIALIZED
        // ============================================================

        private bool EnsureInitialized()
        {
            if (
                initialized &&
                runtimeMaterial != null
            )
            {
                return true;
            }


            if (initializing)
            {
                return false;
            }


            Initialize();


            return
                initialized &&
                runtimeMaterial != null;
        }


        // ============================================================
        // CREATE RUNTIME MATERIAL
        // ============================================================

        private void CreateRuntimeMaterial()
        {
            if (targetText == null)
            {
                return;
            }


            Material materialToUse =
                sourceMaterial;


            // --------------------------------------------------------
            // SOURCE MATERIAL
            // --------------------------------------------------------

            if (materialToUse == null)
            {
                materialToUse =
                    targetText.fontMaterial;
            }


            if (materialToUse == null)
            {
                Debug.LogWarning(
                    "[SparkTMPVFXController] " +
                    "No TMP material found.",
                    this
                );


                return;
            }


            // --------------------------------------------------------
            // RUNTIME INSTANCE
            // --------------------------------------------------------

            if (createRuntimeMaterial)
            {
                runtimeMaterial =
                    new Material(
                        materialToUse
                    );


                runtimeMaterial.name =
                    materialToUse.name +
                    " (Spark TMP VFX Runtime)";


                targetText.fontMaterial =
                    runtimeMaterial;
            }
            else
            {
                runtimeMaterial =
                    materialToUse;
            }
        }


        // ============================================================
        // APPLY THEME COLOR
        // ============================================================

        public void ApplyThemeColor(
            Color color)
        {
            currentThemeColor =
                color;


            if (!EnsureInitialized())
            {
                return;
            }


            SetColor(
                VFXColorID,
                color
            );


            SetColor(
                VFXGlowColorID,
                color
            );


            SetColor(
                VFXScanColorID,
                color
            );


            SetColor(
                VFXSweepColorID,
                color
            );
        }


        // ============================================================
        // APPLY PROFILE
        // ============================================================

        public void ApplyProfile(
            SparkVFXProfile profile,
            Color themeColor,
            bool instant)
        {
            if (profile == null)
            {
                return;
            }


            if (!EnsureInitialized())
            {
                return;
            }


            Color finalColor =
                profile.UseThemeColor
                ? themeColor
                : profile.CustomColor;


            ApplyThemeColor(
                finalColor
            );


            if (instant)
            {
                ApplyImmediate(
                    profile
                );

                return;
            }


            if (transitionRoutine != null)
            {
                StopCoroutine(
                    transitionRoutine
                );
            }


            transitionRoutine =
                StartCoroutine(
                    TransitionToProfile(
                        profile
                    )
                );
        }
        public void ApplyProfile(
    SparkVFXProfile profile,
    Color themeColor)
        {
            ApplyProfile(
                profile,
                themeColor,
                false
            );
        }

        // ============================================================
        // TRANSITION
        // ============================================================

        private IEnumerator TransitionToProfile(
            SparkVFXProfile profile)
        {
            float startGlow =
                currentGlow;

            float startScan =
                currentScan;

            float startSweep =
                currentSweep;

            float startSweepPosition =
                currentSweepPosition;

            float startFlash =
                currentFlash;

            float startGlitch =
                currentGlitch;

            float startFlicker =
                currentFlicker;

            float startReveal =
                currentReveal;

            float startDissolve =
                currentDissolve;


            float duration =
                profile.TransitionDuration;


            if (duration <= 0f)
            {
                ApplyImmediate(
                    profile
                );

                transitionRoutine =
                    null;

                yield break;
            }


            float time =
                0f;


            while (
                time <
                duration
            )
            {
                time +=
                    Time.unscaledDeltaTime;


                float normalized =
                    Mathf.Clamp01(
                        time /
                        duration
                    );


                float t;


                if (
                    profile.TransitionCurve !=
                    null
                )
                {
                    t =
                        profile.TransitionCurve.Evaluate(
                            normalized
                        );
                }
                else
                {
                    t =
                        normalized;
                }


                currentGlow =
                    Mathf.Lerp(
                        startGlow,
                        profile.Glow,
                        t
                    );


                currentScan =
                    Mathf.Lerp(
                        startScan,
                        profile.Scan,
                        t
                    );


                currentSweep =
                    Mathf.Lerp(
                        startSweep,
                        profile.Sweep,
                        t
                    );


                currentSweepPosition =
                    Mathf.Lerp(
                        startSweepPosition,
                        profile.SweepPosition,
                        t
                    );


                currentFlash =
                    Mathf.Lerp(
                        startFlash,
                        profile.Flash,
                        t
                    );


                currentGlitch =
                    Mathf.Lerp(
                        startGlitch,
                        profile.Glitch,
                        t
                    );


                currentFlicker =
                    Mathf.Lerp(
                        startFlicker,
                        profile.Flicker,
                        t
                    );


                currentReveal =
                    Mathf.Lerp(
                        startReveal,
                        profile.Reveal,
                        t
                    );


                currentDissolve =
                    Mathf.Lerp(
                        startDissolve,
                        profile.Dissolve,
                        t
                    );


                ApplyCurrentValues();


                yield return null;
            }


            ApplyImmediate(
                profile
            );


            transitionRoutine =
                null;
        }


        // ============================================================
        // APPLY IMMEDIATE
        // ============================================================

        private void ApplyImmediate(
            SparkVFXProfile profile)
        {
            currentGlow =
                profile.Glow;

            currentScan =
                profile.Scan;

            currentSweep =
                profile.Sweep;

            currentSweepPosition =
                profile.SweepPosition;

            currentFlash =
                profile.Flash;

            currentGlitch =
                profile.Glitch;

            currentFlicker =
                profile.Flicker;

            currentReveal =
                profile.Reveal;

            currentDissolve =
                profile.Dissolve;


            ApplyCurrentValues();
        }


        // ============================================================
        // APPLY CURRENT VALUES
        // ============================================================

        private void ApplyCurrentValues()
        {
            SetFloat(
                VFXGlowID,
                currentGlow
            );


            SetFloat(
                VFXScanID,
                currentScan
            );


            SetFloat(
                VFXSweepID,
                currentSweep
            );


            SetFloat(
                VFXSweepPositionID,
                currentSweepPosition
            );


            SetFloat(
                VFXFlashID,
                currentFlash
            );


            SetFloat(
                VFXGlitchID,
                currentGlitch
            );


            SetFloat(
                VFXFlickerID,
                currentFlicker
            );


            SetFloat(
                VFXRevealID,
                currentReveal
            );


            SetFloat(
                VFXDissolveID,
                currentDissolve
            );
        }


        // ============================================================
        // SEQUENCE VALUES
        // ============================================================

        public void SetGlowValue(
            float value)
        {
            if (!EnsureInitialized())
            {
                return;
            }


            currentGlow =
                Mathf.Clamp(
                    value,
                    0f,
                    5f
                );


            SetFloat(
                VFXGlowID,
                currentGlow
            );
        }


        public void SetScanValue(
            float value)
        {
            if (!EnsureInitialized())
            {
                return;
            }


            currentScan =
                Mathf.Clamp(
                    value,
                    0f,
                    5f
                );


            SetFloat(
                VFXScanID,
                currentScan
            );
        }


        public void SetSweepValue(
            float value)
        {
            if (!EnsureInitialized())
            {
                return;
            }


            currentSweep =
                Mathf.Clamp(
                    value,
                    0f,
                    5f
                );


            SetFloat(
                VFXSweepID,
                currentSweep
            );
        }


        public void SetSweepPositionValue(
            float value)
        {
            if (!EnsureInitialized())
            {
                return;
            }


            currentSweepPosition =
                Mathf.Clamp(
                    value,
                    -1f,
                    2f
                );


            SetFloat(
                VFXSweepPositionID,
                currentSweepPosition
            );
        }


        public void SetFlashValue(
            float value)
        {
            if (!EnsureInitialized())
            {
                return;
            }


            currentFlash =
                Mathf.Clamp(
                    value,
                    0f,
                    5f
                );


            SetFloat(
                VFXFlashID,
                currentFlash
            );
        }


        public void SetGlitchValue(
            float value)
        {
            if (!EnsureInitialized())
            {
                return;
            }


            currentGlitch =
                Mathf.Clamp01(
                    value
                );


            SetFloat(
                VFXGlitchID,
                currentGlitch
            );
        }


        public void SetFlickerValue(
            float value)
        {
            if (!EnsureInitialized())
            {
                return;
            }


            currentFlicker =
                Mathf.Clamp01(
                    value
                );


            SetFloat(
                VFXFlickerID,
                currentFlicker
            );
        }


        public void SetRevealValue(
            float value)
        {
            if (!EnsureInitialized())
            {
                return;
            }


            currentReveal =
                Mathf.Clamp01(
                    value
                );


            SetFloat(
                VFXRevealID,
                currentReveal
            );
        }


        public void SetDissolveValue(
            float value)
        {
            if (!EnsureInitialized())
            {
                return;
            }


            currentDissolve =
                Mathf.Clamp01(
                    value
                );


            SetFloat(
                VFXDissolveID,
                currentDissolve
            );
        }


        // ============================================================
        // RESET
        // ============================================================

        public void ResetVFX()
        {
            if (!EnsureInitialized())
            {
                return;
            }


            if (transitionRoutine != null)
            {
                StopCoroutine(
                    transitionRoutine
                );


                transitionRoutine =
                    null;
            }


            currentGlow =
                0f;

            currentScan =
                0f;

            currentSweep =
                0f;

            currentSweepPosition =
                0.5f;

            currentFlash =
                0f;

            currentGlitch =
                0f;

            currentFlicker =
                0f;

            currentReveal =
                1f;

            currentDissolve =
                0f;


            ApplyCurrentValues();
        }


        // ============================================================
        // MATERIAL HELPERS
        // ============================================================

        private void SetFloat(
            int propertyID,
            float value)
        {
            if (runtimeMaterial == null)
            {
                return;
            }


            if (
                !runtimeMaterial.HasProperty(
                    propertyID
                )
            )
            {
                return;
            }


            runtimeMaterial.SetFloat(
                propertyID,
                value
            );
        }


        private void SetColor(
            int propertyID,
            Color color)
        {
            if (runtimeMaterial == null)
            {
                return;
            }


            if (
                !runtimeMaterial.HasProperty(
                    propertyID
                )
            )
            {
                return;
            }


            runtimeMaterial.SetColor(
                propertyID,
                color
            );
        }


        // ============================================================
        // CLEANUP
        // ============================================================

        private void OnDestroy()
        {
            if (transitionRoutine != null)
            {
                StopCoroutine(
                    transitionRoutine
                );


                transitionRoutine =
                    null;
            }


            if (
                runtimeMaterial != null &&
                createRuntimeMaterial
            )
            {
                if (Application.isPlaying)
                {
                    Destroy(
                        runtimeMaterial
                    );
                }
                else
                {
                    DestroyImmediate(
                        runtimeMaterial
                    );
                }


                runtimeMaterial =
                    null;
            }
        }
    }
}