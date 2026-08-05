using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.UI.VFX
{
    /// <summary>
    /// Main low-level controller for Project Spark UI VFX.
    ///
    /// Responsibilities:
    /// - Finds and controls the target UI Graphic.
    /// - Creates a unique runtime material instance.
    /// - Controls Project Spark UI VFX shader properties.
    /// - Applies SparkVFXProfile values.
    /// - Supports theme colors.
    /// - Supports smooth transitions.
    /// - Never recursively calls Initialize().
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Graphic))]
    public sealed class SparkVFXController : MonoBehaviour, ISparkVFXController
    {
        // REFERENCES
        // ============================================================

        [Header("Target")]

        [Tooltip(
            "The UI Graphic that receives the VFX material. " +
            "For example: Image or RawImage."
        )]
        [SerializeField]
        private Graphic targetGraphic;


        // ============================================================
        // MATERIAL
        // ============================================================

        [Header("Material")]

        [Tooltip(
            "Optional material using the Project Spark UI VFX shader. " +
            "If empty, the current Graphic material is used."
        )]
        [SerializeField]
        private Material sourceMaterial;


        [Tooltip(
            "Create a unique runtime material instance. " +
            "Recommended for UI VFX."
        )]
        [SerializeField]
        private bool createRuntimeMaterial = true;


        // ============================================================
        // THEME
        // ============================================================

        [Header("Theme")]

        [SerializeField]
        private Color currentThemeColor = Color.white;


        [SerializeField]
        private bool applyThemeToVFXColor = true;


        [SerializeField]
        private bool applyThemeToGlowColor = true;


        [SerializeField]
        private bool applyThemeToScanColor = true;


        [SerializeField]
        private bool applyThemeToSweepColor = true;


        // ============================================================
        // RUNTIME STATE
        // ============================================================

        private Material runtimeMaterial;

        private bool initialized;

        private bool initializing;


        // ============================================================
        // SHADER PROPERTY IDS
        // ============================================================

        private static readonly int MainTexID =
            Shader.PropertyToID("_MainTex");


        private static readonly int BaseColorID =
            Shader.PropertyToID("_BaseColor");


        private static readonly int VFXColorID =
            Shader.PropertyToID("_VFXColor");


        private static readonly int GlowColorID =
            Shader.PropertyToID("_GlowColor");


        private static readonly int ScanColorID =
            Shader.PropertyToID("_ScanColor");


        private static readonly int SweepColorID =
            Shader.PropertyToID("_SweepColor");


        private static readonly int GlowID =
            Shader.PropertyToID("_Glow");


        private static readonly int ScanID =
            Shader.PropertyToID("_Scan");


        private static readonly int SweepID =
            Shader.PropertyToID("_Sweep");


        private static readonly int FlashID =
            Shader.PropertyToID("_Flash");


        private static readonly int GlitchID =
            Shader.PropertyToID("_Glitch");


        private static readonly int FlickerID =
            Shader.PropertyToID("_Flicker");


        private static readonly int DissolveID =
            Shader.PropertyToID("_Dissolve");


        private static readonly int RevealID =
            Shader.PropertyToID("_Reveal");


        private static readonly int SweepPositionID =
            Shader.PropertyToID("_SweepPosition");


        // ============================================================
        // INITIALIZATION
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


        /// <summary>
        /// Initializes the controller.
        ///
        /// IMPORTANT:
        /// This method NEVER calls ApplyThemeColor().
        /// This prevents recursive initialization.
        /// </summary>
        public void Initialize()
        {
            // --------------------------------------------------------
            // PREVENT RECURSION
            // --------------------------------------------------------

            if (initialized)
            {
                return;
            }


            if (initializing)
            {
                return;
            }


            initializing = true;


            // --------------------------------------------------------
            // FIND TARGET GRAPHIC
            // --------------------------------------------------------

            FindTargetGraphic();


            if (targetGraphic == null)
            {
                Debug.LogError(
                    "[SparkVFXController] " +
                    "No UI Graphic found. " +
                    "Assign an Image or RawImage.",
                    this
                );


                initializing = false;

                return;
            }


            // --------------------------------------------------------
            // CREATE MATERIAL
            // --------------------------------------------------------

            CreateRuntimeMaterial();


            // --------------------------------------------------------
            // INITIALIZE DEFAULT VALUES
            // --------------------------------------------------------

            if (runtimeMaterial != null)
            {
                SetFloat(
                    GlowID,
                    0f
                );


                SetFloat(
                    ScanID,
                    0f
                );


                SetFloat(
                    SweepID,
                    0f
                );


                SetFloat(
                    FlashID,
                    0f
                );


                SetFloat(
                    GlitchID,
                    0f
                );


                SetFloat(
                    FlickerID,
                    0f
                );


                SetFloat(
                    DissolveID,
                    0f
                );


                SetFloat(
                    RevealID,
                    1f
                );


                SetFloat(
                    SweepPositionID,
                    0.5f
                );
            }


            // --------------------------------------------------------
            // COMPLETE
            // --------------------------------------------------------

            initialized = true;

            initializing = false;
        }


        // ============================================================
        // FIND TARGET GRAPHIC
        // ============================================================

        private void FindTargetGraphic()
        {
            if (targetGraphic != null)
            {
                return;
            }


            targetGraphic =
                GetComponent<Graphic>();
        }


        // ============================================================
        // CREATE RUNTIME MATERIAL
        // ============================================================

        private void CreateRuntimeMaterial()
        {
            if (targetGraphic == null)
            {
                return;
            }


            Material materialToUse =
                sourceMaterial;


            // --------------------------------------------------------
            // USE SOURCE MATERIAL
            // --------------------------------------------------------

            if (materialToUse == null)
            {
                materialToUse =
                    targetGraphic.material;
            }


            // --------------------------------------------------------
            // NO MATERIAL
            // --------------------------------------------------------

            if (materialToUse == null)
            {
                Debug.LogWarning(
                    "[SparkVFXController] " +
                    "No source material found on target Graphic.",
                    this
                );


                return;
            }


            // --------------------------------------------------------
            // CREATE INSTANCE
            // --------------------------------------------------------

            if (createRuntimeMaterial)
            {
                runtimeMaterial =
                    new Material(
                        materialToUse
                    );


                runtimeMaterial.name =
                    materialToUse.name +
                    " (Spark VFX Runtime)";


                targetGraphic.material =
                    runtimeMaterial;
            }
            else
            {
                runtimeMaterial =
                    materialToUse;
            }
        }


        // ============================================================
        // ENSURE INITIALIZED
        // ============================================================

        private bool EnsureInitialized()
        {
            if (initialized &&
                runtimeMaterial != null)
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
        // THEME COLOR
        // ============================================================

        public void ApplyThemeColor(
            Color color)
        {
            currentThemeColor =
                color;


            // IMPORTANT:
            // Do not call Initialize() here before checking.
            //
            // This method is allowed to initialize once,
            // but Initialize() NEVER calls this method.
            if (!EnsureInitialized())
            {
                return;
            }


            if (applyThemeToVFXColor)
            {
                SetColor(
                    VFXColorID,
                    color
                );
            }


            if (applyThemeToGlowColor)
            {
                SetColor(
                    GlowColorID,
                    color
                );
            }


            if (applyThemeToScanColor)
            {
                SetColor(
                    ScanColorID,
                    color
                );
            }


            if (applyThemeToSweepColor)
            {
                SetColor(
                    SweepColorID,
                    color
                );
            }
        }


        // ============================================================
        // CURRENT THEME COLOR
        // ============================================================

        public Color CurrentThemeColor
        {
            get
            {
                return currentThemeColor;
            }
        }


        // ============================================================
        // TARGET GRAPHIC
        // ============================================================

        public Graphic TargetGraphic
        {
            get
            {
                return targetGraphic;
            }
        }


        // ============================================================
        // RUNTIME MATERIAL
        // ============================================================

        public Material RuntimeMaterial
        {
            get
            {
                return runtimeMaterial;
            }
        }


        // ============================================================
        // APPLY PROFILE
        // ============================================================

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


            currentThemeColor =
                themeColor;


            if (!EnsureInitialized())
            {
                return;
            }


            Color profileColor;


            if (profile.UseThemeColor)
            {
                profileColor =
                    themeColor;
            }
            else
            {
                profileColor =
                    profile.CustomColor;
            }


            // --------------------------------------------------------
            // COLORS
            // --------------------------------------------------------

            SetColor(
                VFXColorID,
                profileColor
            );


            SetColor(
                GlowColorID,
                profileColor
            );


            SetColor(
                ScanColorID,
                profileColor
            );


            SetColor(
                SweepColorID,
                profileColor
            );


            // --------------------------------------------------------
            // VALUES
            // --------------------------------------------------------

            if (instant)
            {
                SetFloat(
                    GlowID,
                    profile.Glow
                );


                SetFloat(
                    ScanID,
                    profile.Scan
                );


                SetFloat(
                    SweepID,
                    profile.Sweep
                );


                SetFloat(
                    SweepPositionID,
                    profile.SweepPosition
                );


                SetFloat(
                    FlashID,
                    profile.Flash
                );


                SetFloat(
                    GlitchID,
                    profile.Glitch
                );


                SetFloat(
                    FlickerID,
                    profile.Flicker
                );


                SetFloat(
                    RevealID,
                    profile.Reveal
                );


                SetFloat(
                    DissolveID,
                    profile.Dissolve
                );


                return;
            }


            // --------------------------------------------------------
            // SMOOTH TRANSITION
            // --------------------------------------------------------

            StopAllCoroutines();


            StartCoroutine(
                AnimateProfile(
                    profile
                )
            );
        }


        // ============================================================
        // ANIMATE PROFILE
        // ============================================================

        private System.Collections.IEnumerator AnimateProfile(
            SparkVFXProfile profile)
        {
            float duration =
                profile.TransitionDuration;


            if (duration <= 0f)
            {
                ApplyProfile(
                    profile,
                    currentThemeColor,
                    true
                );


                yield break;
            }


            float startGlow =
                GetFloat(
                    GlowID
                );


            float startScan =
                GetFloat(
                    ScanID
                );


            float startSweep =
                GetFloat(
                    SweepID
                );


            float startSweepPosition =
                GetFloat(
                    SweepPositionID
                );


            float startFlash =
                GetFloat(
                    FlashID
                );


            float startGlitch =
                GetFloat(
                    GlitchID
                );


            float startFlicker =
                GetFloat(
                    FlickerID
                );


            float startReveal =
                GetFloat(
                    RevealID
                );


            float startDissolve =
                GetFloat(
                    DissolveID
                );


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


                SetFloat(
                    GlowID,
                    Mathf.Lerp(
                        startGlow,
                        profile.Glow,
                        t
                    )
                );


                SetFloat(
                    ScanID,
                    Mathf.Lerp(
                        startScan,
                        profile.Scan,
                        t
                    )
                );


                SetFloat(
                    SweepID,
                    Mathf.Lerp(
                        startSweep,
                        profile.Sweep,
                        t
                    )
                );


                SetFloat(
                    SweepPositionID,
                    Mathf.Lerp(
                        startSweepPosition,
                        profile.SweepPosition,
                        t
                    )
                );


                SetFloat(
                    FlashID,
                    Mathf.Lerp(
                        startFlash,
                        profile.Flash,
                        t
                    )
                );


                SetFloat(
                    GlitchID,
                    Mathf.Lerp(
                        startGlitch,
                        profile.Glitch,
                        t
                    )
                );


                SetFloat(
                    FlickerID,
                    Mathf.Lerp(
                        startFlicker,
                        profile.Flicker,
                        t
                    )
                );


                SetFloat(
                    RevealID,
                    Mathf.Lerp(
                        startReveal,
                        profile.Reveal,
                        t
                    )
                );


                SetFloat(
                    DissolveID,
                    Mathf.Lerp(
                        startDissolve,
                        profile.Dissolve,
                        t
                    )
                );


                yield return null;
            }


            SetFloat(
                GlowID,
                profile.Glow
            );


            SetFloat(
                ScanID,
                profile.Scan
            );


            SetFloat(
                SweepID,
                profile.Sweep
            );


            SetFloat(
                SweepPositionID,
                profile.SweepPosition
            );


            SetFloat(
                FlashID,
                profile.Flash
            );


            SetFloat(
                GlitchID,
                profile.Glitch
            );


            SetFloat(
                FlickerID,
                profile.Flicker
            );


            SetFloat(
                RevealID,
                profile.Reveal
            );


            SetFloat(
                DissolveID,
                profile.Dissolve
            );
        }


        // ============================================================
        // DIRECT FLOAT
        // ============================================================

        public void SetVFXFloat(
            string propertyName,
            float value)
        {
            if (!EnsureInitialized())
            {
                return;
            }


            if (!runtimeMaterial.HasProperty(
                propertyName))
            {
                Debug.LogWarning(
                    "[SparkVFXController] Shader property not found: " +
                    propertyName,
                    this
                );


                return;
            }


            runtimeMaterial.SetFloat(
                propertyName,
                value
            );
        }


        // ============================================================
        // DIRECT COLOR
        // ============================================================

        public void SetVFXColor(
            string propertyName,
            Color color)
        {
            if (!EnsureInitialized())
            {
                return;
            }


            if (!runtimeMaterial.HasProperty(
                propertyName))
            {
                Debug.LogWarning(
                    "[SparkVFXController] Shader color property not found: " +
                    propertyName,
                    this
                );


                return;
            }


            runtimeMaterial.SetColor(
                propertyName,
                color
            );
        }


        // ============================================================
        // INTERNAL SET FLOAT
        // ============================================================

        private void SetFloat(
            int propertyID,
            float value)
        {
            if (runtimeMaterial == null)
            {
                return;
            }


            runtimeMaterial.SetFloat(
                propertyID,
                value
            );
        }


        // ============================================================
        // INTERNAL GET FLOAT
        // ============================================================

        private float GetFloat(
            int propertyID)
        {
            if (runtimeMaterial == null)
            {
                return 0f;
            }


            return runtimeMaterial.GetFloat(
                propertyID
            );
        }


        // ============================================================
        // INTERNAL SET COLOR
        // ============================================================

        private void SetColor(
            int propertyID,
            Color color)
        {
            if (runtimeMaterial == null)
            {
                return;
            }


            runtimeMaterial.SetColor(
                propertyID,
                color
            );
        }


        // ============================================================
        // TEST VFX
        // ============================================================

        [ContextMenu("TEST VFX / Glow")]
        private void TestGlow()
        {
            if (!EnsureInitialized())
            {
                return;
            }


            SetFloat(
                GlowID,
                5f
            );
        }


        [ContextMenu("TEST VFX / Scan")]
        private void TestScan()
        {
            if (!EnsureInitialized())
            {
                return;
            }


            SetFloat(
                ScanID,
                5f
            );
        }


        [ContextMenu("TEST VFX / Sweep")]
        private void TestSweep()
        {
            if (!EnsureInitialized())
            {
                return;
            }


            SetFloat(
                SweepID,
                5f
            );


            SetFloat(
                SweepPositionID,
                0f
            );


            StartCoroutine(
                TestSweepAnimation()
            );
        }


        private System.Collections.IEnumerator TestSweepAnimation()
        {
            float time =
                0f;


            while (
                time <
                2f
            )
            {
                time +=
                    Time.unscaledDeltaTime;


                float position =
                    Mathf.PingPong(
                        time,
                        1f
                    );


                SetFloat(
                    SweepPositionID,
                    position
                );


                yield return null;
            }
        }


        [ContextMenu("TEST VFX / Flash")]
        private void TestFlash()
        {
            if (!EnsureInitialized())
            {
                return;
            }


            StartCoroutine(
                FlashRoutine()
            );
        }


        private System.Collections.IEnumerator FlashRoutine()
        {
            SetFloat(
                FlashID,
                5f
            );


            float time =
                0f;


            while (
                time <
                0.35f
            )
            {
                time +=
                    Time.unscaledDeltaTime;


                float value =
                    Mathf.Lerp(
                        5f,
                        0f,
                        time /
                        0.35f
                    );


                SetFloat(
                    FlashID,
                    value
                );


                yield return null;
            }


            SetFloat(
                FlashID,
                0f
            );
        }


        [ContextMenu("TEST VFX / Glitch")]
        private void TestGlitch()
        {
            if (!EnsureInitialized())
            {
                return;
            }


            SetFloat(
                GlitchID,
                1f
            );
        }


        [ContextMenu("TEST VFX / Flicker")]
        private void TestFlicker()
        {
            if (!EnsureInitialized())
            {
                return;
            }


            SetFloat(
                FlickerID,
                1f
            );
        }


        [ContextMenu("TEST VFX / Reset")]
        public void ResetVFX()
        {
            if (!EnsureInitialized())
            {
                return;
            }


            StopAllCoroutines();


            SetFloat(
                GlowID,
                0f
            );


            SetFloat(
                ScanID,
                0f
            );


            SetFloat(
                SweepID,
                0f
            );


            SetFloat(
                FlashID,
                0f
            );


            SetFloat(
                GlitchID,
                0f
            );


            SetFloat(
                FlickerID,
                0f
            );


            SetFloat(
                DissolveID,
                0f
            );


            SetFloat(
                RevealID,
                1f
            );


            SetFloat(
                SweepPositionID,
                0.5f
            );
        }


        // ============================================================
        // CLEANUP
        // ============================================================

        private void OnDestroy()
        {
            StopAllCoroutines();


            if (runtimeMaterial != null)
            {
                Destroy(
                    runtimeMaterial
                );


                runtimeMaterial =
                    null;
            }
        }
        // ============================================================
        // SEQUENCE PLAYER VALUES
        // ============================================================

        public void SetGlowValue(
            float value)
        {
            if (!EnsureInitialized())
            {
                return;
            }

            SetFloat(
                GlowID,
                Mathf.Clamp(
                    value,
                    0f,
                    5f
                )
            );
        }


        public void SetScanValue(
            float value)
        {
            if (!EnsureInitialized())
            {
                return;
            }

            SetFloat(
                ScanID,
                Mathf.Clamp(
                    value,
                    0f,
                    5f
                )
            );
        }


        public void SetSweepValue(
            float value)
        {
            if (!EnsureInitialized())
            {
                return;
            }

            SetFloat(
                SweepID,
                Mathf.Clamp(
                    value,
                    0f,
                    5f
                )
            );
        }


        public void SetSweepPositionValue(
            float value)
        {
            if (!EnsureInitialized())
            {
                return;
            }

            SetFloat(
                SweepPositionID,
                Mathf.Clamp(
                    value,
                    -1f,
                    2f
                )
            );
        }


        public void SetFlashValue(
            float value)
        {
            if (!EnsureInitialized())
            {
                return;
            }

            SetFloat(
                FlashID,
                Mathf.Clamp(
                    value,
                    0f,
                    5f
                )
            );
        }


        public void SetGlitchValue(
            float value)
        {
            if (!EnsureInitialized())
            {
                return;
            }

            SetFloat(
                GlitchID,
                Mathf.Clamp01(
                    value
                )
            );
        }


        public void SetFlickerValue(
            float value)
        {
            if (!EnsureInitialized())
            {
                return;
            }

            SetFloat(
                FlickerID,
                Mathf.Clamp01(
                    value
                )
            );
        }


        public void SetRevealValue(
            float value)
        {
            if (!EnsureInitialized())
            {
                return;
            }

            SetFloat(
                RevealID,
                Mathf.Clamp01(
                    value
                )
            );
        }


        public void SetDissolveValue(
            float value)
        {
            if (!EnsureInitialized())
            {
                return;
            }

            SetFloat(
                DissolveID,
                Mathf.Clamp01(
                    value
                )
            );
        }
    }
}