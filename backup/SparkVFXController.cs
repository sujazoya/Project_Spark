using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.UI.VFX
{
    [DisallowMultipleComponent]
    public sealed class SparkVFXController : MonoBehaviour
    {
        // ============================================================
        // REFERENCES
        // ============================================================

        [Header("UI VFX Target")]

        [SerializeField]
        private Graphic targetGraphic;


        [SerializeField]
        private Material vfxMaterial;


        // ============================================================
        // RUNTIME MATERIAL
        // ============================================================

        private Material runtimeMaterial;


        // ============================================================
        // STATE
        // ============================================================

        private bool initialized;

        private bool initializing;


        // ============================================================
        // THEME
        // ============================================================

        [Header("Theme")]

        [SerializeField]
        private Color themeColor = Color.white;


        // ============================================================
        // SHADER PROPERTY IDS
        // ============================================================

        private static readonly int VFXColor =
            Shader.PropertyToID("_VFXColor");


        private static readonly int VFXGlowColor =
            Shader.PropertyToID("_VFXGlowColor");


        private static readonly int VFXScanColor =
            Shader.PropertyToID("_VFXScanColor");


        private static readonly int VFXSweepColor =
            Shader.PropertyToID("_VFXSweepColor");


        private static readonly int Glow =
            Shader.PropertyToID("_Glow");


        private static readonly int Scan =
            Shader.PropertyToID("_Scan");


        private static readonly int Sweep =
            Shader.PropertyToID("_Sweep");


        private static readonly int SweepPosition =
            Shader.PropertyToID("_SweepPosition");


        private static readonly int Flash =
            Shader.PropertyToID("_Flash");


        private static readonly int Glitch =
            Shader.PropertyToID("_Glitch");


        private static readonly int Flicker =
            Shader.PropertyToID("_Flicker");


        private static readonly int Reveal =
            Shader.PropertyToID("_Reveal");


        private static readonly int Dissolve =
            Shader.PropertyToID("_Dissolve");


        // ============================================================
        // UNITY
        // ============================================================

        private void Awake()
        {
            Initialize();
        }


        // ============================================================
        // INITIALIZE
        // ============================================================

        public void Initialize()
        {
            // --------------------------------------------------------
            // ALREADY INITIALIZED
            // --------------------------------------------------------

            if (initialized)
            {
                return;
            }


            // --------------------------------------------------------
            // PREVENT RECURSION
            // --------------------------------------------------------

            if (initializing)
            {
                return;
            }


            initializing =
                true;


            // --------------------------------------------------------
            // FIND TARGET
            // --------------------------------------------------------

            FindTargetGraphic();


            // --------------------------------------------------------
            // CREATE MATERIAL
            // --------------------------------------------------------

            CreateRuntimeMaterial();


            // --------------------------------------------------------
            // APPLY INITIAL THEME
            //
            // IMPORTANT:
            // Do NOT call public ApplyThemeColor() here.
            // That method calls EnsureInitialized().
            // --------------------------------------------------------

            if (runtimeMaterial != null)
            {
                ApplyThemeColorInternal(
                    themeColor
                );
            }


            // --------------------------------------------------------
            // COMPLETE
            // --------------------------------------------------------

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
            if (initialized)
            {
                return true;
            }


            if (initializing)
            {
                return runtimeMaterial != null;
            }


            Initialize();


            return runtimeMaterial != null;
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


            // --------------------------------------------------------
            // FIRST: SAME GAMEOBJECT
            // --------------------------------------------------------

            targetGraphic =
                GetComponent<Graphic>();


            if (targetGraphic != null)
            {
                return;
            }


            // --------------------------------------------------------
            // SECOND: CHILD
            // --------------------------------------------------------

            targetGraphic =
                GetComponentInChildren<Graphic>(
                    true
                );
        }


        // ============================================================
        // CREATE RUNTIME MATERIAL
        // ============================================================

        private void CreateRuntimeMaterial()
        {
            if (runtimeMaterial != null)
            {
                AssignRuntimeMaterial();

                return;
            }


            // --------------------------------------------------------
            // FIND SOURCE
            // --------------------------------------------------------

            Material sourceMaterial =
                vfxMaterial;


            if (sourceMaterial == null &&
                targetGraphic != null)
            {
                sourceMaterial =
                    targetGraphic.material;
            }


            // --------------------------------------------------------
            // NO MATERIAL
            // --------------------------------------------------------

            if (sourceMaterial == null)
            {
                Debug.LogWarning(
                    "[SparkVFXController] " +
                    "No VFX Material assigned.",
                    this
                );

                return;
            }


            // --------------------------------------------------------
            // CREATE RUNTIME INSTANCE
            // --------------------------------------------------------

            runtimeMaterial =
                new Material(
                    sourceMaterial
                );


            runtimeMaterial.name =
                sourceMaterial.name +
                " (SparkVFX Runtime)";


            // --------------------------------------------------------
            // ASSIGN
            // --------------------------------------------------------

            AssignRuntimeMaterial();


            // --------------------------------------------------------
            // VALIDATE
            // --------------------------------------------------------

            ValidateMaterial();
        }


        // ============================================================
        // ASSIGN MATERIAL
        // ============================================================

        private void AssignRuntimeMaterial()
        {
            if (targetGraphic == null)
            {
                return;
            }


            if (runtimeMaterial == null)
            {
                return;
            }


            targetGraphic.material =
                runtimeMaterial;
        }


        // ============================================================
        // VALIDATE
        // ============================================================

        private void ValidateMaterial()
        {
            if (runtimeMaterial == null)
            {
                return;
            }


            if (runtimeMaterial.shader == null)
            {
                Debug.LogError(
                    "[SparkVFXController] " +
                    "Runtime material has no shader.",
                    this
                );

                return;
            }


            Debug.Log(
                "[SparkVFXController] " +
                "VFX Ready: " +
                runtimeMaterial.shader.name,
                this
            );
        }


        // ============================================================
        // TARGET GRAPHIC
        // ============================================================

        public Graphic TargetGraphic
        {
            get
            {
                EnsureInitialized();

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
                EnsureInitialized();

                return runtimeMaterial;
            }
        }


        // ============================================================
        // CURRENT THEME COLOR
        // ============================================================

        public Color CurrentThemeColor
        {
            get
            {
                return themeColor;
            }
        }


        // ============================================================
        // READY
        // ============================================================

        public bool IsReady
        {
            get
            {
                return
                    targetGraphic != null &&
                    runtimeMaterial != null &&
                    runtimeMaterial.shader != null;
            }
        }


        // ============================================================
        // SET TARGET GRAPHIC
        // ============================================================

        public void SetTargetGraphic(
            Graphic graphic)
        {
            if (graphic == null)
            {
                Debug.LogWarning(
                    "[SparkVFXController] " +
                    "Cannot assign null Graphic.",
                    this
                );

                return;
            }


            targetGraphic =
                graphic;


            if (runtimeMaterial == null)
            {
                CreateRuntimeMaterial();
            }
            else
            {
                AssignRuntimeMaterial();
            }
        }


        // ============================================================
        // SET VFX MATERIAL
        // ============================================================

        public void SetVFXMaterial(
            Material material)
        {
            if (material == null)
            {
                Debug.LogWarning(
                    "[SparkVFXController] " +
                    "Cannot assign null material.",
                    this
                );

                return;
            }


            DestroyRuntimeMaterial();


            vfxMaterial =
                material;


            CreateRuntimeMaterial();


            ApplyThemeColorInternal(
                themeColor
            );
        }


        // ============================================================
        // APPLY THEME COLOR
        // ============================================================

        public void ApplyThemeColor(
            Color color)
        {
            themeColor =
                color;


            if (!EnsureInitialized())
            {
                return;
            }


            ApplyThemeColorInternal(
                color
            );
        }


        // ============================================================
        // APPLY THEME COLOR INTERNAL
        //
        // IMPORTANT:
        // This method NEVER calls Initialize().
        // ============================================================

        private void ApplyThemeColorInternal(
            Color color)
        {
            if (runtimeMaterial == null)
            {
                return;
            }


            SetColor(
                VFXColor,
                color
            );


            SetColor(
                VFXGlowColor,
                color
            );


            SetColor(
                VFXScanColor,
                color
            );


            SetColor(
                VFXSweepColor,
                color
            );
        }


        // ============================================================
        // SET COLOR
        // ============================================================

        private void SetColor(
            int propertyID,
            Color value)
        {
            if (runtimeMaterial == null)
            {
                return;
            }


            if (!runtimeMaterial.HasProperty(
                propertyID))
            {
                return;
            }


            runtimeMaterial.SetColor(
                propertyID,
                value
            );
        }


        // ============================================================
        // SET FLOAT
        // ============================================================

        private void SetFloat(
            int propertyID,
            float value)
        {
            if (runtimeMaterial == null)
            {
                return;
            }


            if (!runtimeMaterial.HasProperty(
                propertyID))
            {
                return;
            }


            runtimeMaterial.SetFloat(
                propertyID,
                value
            );
        }


        // ============================================================
        // DIRECT VFX COLOR
        // ============================================================

        public void SetVFXColor(
            Color color)
        {
            if (!EnsureInitialized())
            {
                return;
            }


            SetColor(
                VFXColor,
                color
            );
        }


        // ============================================================
        // GLOW COLOR
        // ============================================================

        public void SetGlowColor(
            Color color)
        {
            if (!EnsureInitialized())
            {
                return;
            }


            SetColor(
                VFXGlowColor,
                color
            );
        }


        // ============================================================
        // SCAN COLOR
        // ============================================================

        public void SetScanColor(
            Color color)
        {
            if (!EnsureInitialized())
            {
                return;
            }


            SetColor(
                VFXScanColor,
                color
            );
        }


        // ============================================================
        // SWEEP COLOR
        // ============================================================

        public void SetSweepColor(
            Color color)
        {
            if (!EnsureInitialized())
            {
                return;
            }


            SetColor(
                VFXSweepColor,
                color
            );
        }


        // ============================================================
        // APPLY PROFILE
        // ============================================================

        public void ApplyProfile(
            SparkVFXProfile profile,
            Color themeColor,
            bool instant = false)
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


            ApplyThemeColorInternal(
                finalColor
            );


            SetGlowValue(
                profile.Glow
            );


            SetScanValue(
                profile.Scan
            );


            SetSweepValue(
                profile.Sweep
            );


            SetSweepPositionValue(
                profile.SweepPosition
            );


            SetFlashValue(
                profile.Flash
            );


            SetGlitchValue(
                profile.Glitch
            );


            SetFlickerValue(
                profile.Flicker
            );


            SetRevealValue(
                profile.Reveal
            );


            SetDissolveValue(
                profile.Dissolve
            );
        }


        // ============================================================
        // GLOW
        // ============================================================

        public void SetGlowValue(
            float value)
        {
            if (!EnsureInitialized())
            {
                return;
            }


            SetFloat(
                Glow,
                Mathf.Max(
                    0f,
                    value
                )
            );
        }


        // ============================================================
        // SCAN
        // ============================================================

        public void SetScanValue(
            float value)
        {
            if (!EnsureInitialized())
            {
                return;
            }


            SetFloat(
                Scan,
                Mathf.Max(
                    0f,
                    value
                )
            );
        }


        // ============================================================
        // SWEEP
        // ============================================================

        public void SetSweepValue(
            float value)
        {
            if (!EnsureInitialized())
            {
                return;
            }


            SetFloat(
                Sweep,
                Mathf.Max(
                    0f,
                    value
                )
            );
        }


        // ============================================================
        // SWEEP POSITION
        // ============================================================

        public void SetSweepPositionValue(
            float value)
        {
            if (!EnsureInitialized())
            {
                return;
            }


            SetFloat(
                SweepPosition,
                value
            );
        }


        // ============================================================
        // FLASH
        // ============================================================

        public void SetFlashValue(
            float value)
        {
            if (!EnsureInitialized())
            {
                return;
            }


            SetFloat(
                Flash,
                Mathf.Max(
                    0f,
                    value
                )
            );
        }


        // ============================================================
        // GLITCH
        // ============================================================

        public void SetGlitchValue(
            float value)
        {
            if (!EnsureInitialized())
            {
                return;
            }


            SetFloat(
                Glitch,
                Mathf.Clamp01(
                    value
                )
            );
        }


        // ============================================================
        // FLICKER
        // ============================================================

        public void SetFlickerValue(
            float value)
        {
            if (!EnsureInitialized())
            {
                return;
            }


            SetFloat(
                Flicker,
                Mathf.Clamp01(
                    value
                )
            );
        }


        // ============================================================
        // REVEAL
        // ============================================================

        public void SetRevealValue(
            float value)
        {
            if (!EnsureInitialized())
            {
                return;
            }


            SetFloat(
                Reveal,
                Mathf.Clamp01(
                    value
                )
            );
        }


        // ============================================================
        // DISSOLVE
        // ============================================================

        public void SetDissolveValue(
            float value)
        {
            if (!EnsureInitialized())
            {
                return;
            }


            SetFloat(
                Dissolve,
                Mathf.Clamp01(
                    value
                )
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


            SetGlowValue(0f);

            SetScanValue(0f);

            SetSweepValue(0f);

            SetSweepPositionValue(0f);

            SetFlashValue(0f);

            SetGlitchValue(0f);

            SetFlickerValue(0f);

            SetRevealValue(1f);

            SetDissolveValue(0f);


            ApplyThemeColorInternal(
                themeColor
            );
        }


        // ============================================================
        // TEST
        // ============================================================

        [ContextMenu("TEST VFX")]
        public void TestVFX()
        {
            if (!EnsureInitialized())
            {
                Debug.LogError(
                    "[SparkVFXController] " +
                    "Test failed. " +
                    "Material is not ready.",
                    this
                );

                return;
            }


            Debug.Log(
                "[SparkVFXController] " +
                "TEST VFX STARTED.",
                this
            );


            ApplyThemeColor(
                Color.cyan
            );


            SetGlowValue(
                1f
            );


            SetScanValue(
                1f
            );


            SetSweepValue(
                1f
            );


            SetSweepPositionValue(
                0.5f
            );


            SetFlashValue(
                1f
            );


            SetGlitchValue(
                0.25f
            );


            SetFlickerValue(
                1f
            );


            SetRevealValue(
                1f
            );


            SetDissolveValue(
                0f
            );


            Debug.Log(
                "[SparkVFXController] " +
                "TEST VFX APPLIED.",
                this
            );
        }


        // ============================================================
        // DESTROY RUNTIME MATERIAL
        // ============================================================

        private void DestroyRuntimeMaterial()
        {
            if (runtimeMaterial == null)
            {
                return;
            }


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


        // ============================================================
        // CLEANUP
        // ============================================================

        private void OnDestroy()
        {
            DestroyRuntimeMaterial();
        }


        // ============================================================
        // EDITOR
        // ============================================================

#if UNITY_EDITOR

        private void OnValidate()
        {
            if (targetGraphic == null)
            {
                targetGraphic =
                    GetComponent<Graphic>();
            }
        }

#endif
    }
}