using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.UI.VFX
{
    /// <summary>
    /// Connects Project Spark ThemeManager with the
    /// runtime material used by the VFX system.
    ///
    /// Supports:
    /// - uGUI Graphic
    /// - TextMeshPro UI
    /// - Runtime material instances
    /// - Automatic theme refresh
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkVFXThemeBridge :
        MonoBehaviour,
        IThemeObject
    {
        #region Inspector

        [Header("Target")]

        [Tooltip(
            "Optional uGUI Graphic target. " +
            "Automatically detected if empty.")]
        [SerializeField]
        private Graphic targetGraphic;

        [Tooltip(
            "Optional TMP text target. " +
            "Automatically detected if empty.")]
        [SerializeField]
        private TMP_Text targetTMP;

        [Header("Material")]

        [Tooltip(
            "If enabled, creates a unique runtime " +
            "material instance for this object.")]
        [SerializeField]
        private bool createRuntimeMaterial = true;

        #endregion


        #region Runtime

        private Material runtimeMaterial;

        private Material originalGraphicMaterial;

        private Material originalTMPMaterial;

        private bool initialized;

        #endregion


        #region Shader Properties

        private static readonly int VFXColor =
            Shader.PropertyToID(
                "_VFXColor");

        private static readonly int VFXGlowColor =
            Shader.PropertyToID(
                "_VFXGlowColor");

        private static readonly int VFXScanColor =
            Shader.PropertyToID(
                "_VFXScanColor");

        private static readonly int VFXSweepColor =
            Shader.PropertyToID(
                "_VFXSweepColor");

        #endregion


        #region Unity Lifecycle

        private void Awake()
        {
            FindTarget();

            InitializeMaterial();
        }


        private void OnEnable()
        {
            FindTarget();

            InitializeMaterial();

            RegisterToThemeManager();

            ApplyTheme();
        }


        private void OnDisable()
        {
            UnregisterFromThemeManager();
        }


        private void OnDestroy()
        {
            RestoreOriginalMaterial();

            DestroyRuntimeMaterial();
        }

        #endregion


        #region Target Detection

        private void FindTarget()
        {
            if (targetGraphic == null)
            {
                targetGraphic =
                    GetComponent<Graphic>();
            }


            if (targetTMP == null)
            {
                targetTMP =
                    GetComponent<TMP_Text>();
            }
        }

        #endregion


        #region Material Initialization

        private void InitializeMaterial()
        {
            if (initialized)
            {
                return;
            }


            if (targetGraphic != null)
            {
                originalGraphicMaterial =
                    targetGraphic.material;
            }


            if (targetTMP != null)
            {
                originalTMPMaterial =
                    targetTMP.fontMaterial;
            }


            if (!createRuntimeMaterial)
            {
                initialized = true;

                return;
            }


            Material sourceMaterial =
                GetSourceMaterial();


            if (sourceMaterial == null)
            {
                Debug.LogWarning(
                    "SparkVFXThemeBridge: " +
                    "No source material found.",
                    this);

                initialized = true;

                return;
            }


            runtimeMaterial =
                new Material(
                    sourceMaterial);


            runtimeMaterial.name =
                sourceMaterial.name +
                " - Project Spark VFX";


            AssignRuntimeMaterial();


            initialized = true;
        }

        #endregion


        #region Material Assignment

        private Material GetSourceMaterial()
        {
            if (targetGraphic != null)
            {
                return targetGraphic.material;
            }


            if (targetTMP != null)
            {
                return targetTMP.fontMaterial;
            }


            return null;
        }


        private void AssignRuntimeMaterial()
        {
            if (runtimeMaterial == null)
            {
                return;
            }


            if (targetGraphic != null)
            {
                targetGraphic.material =
                    runtimeMaterial;
            }


            if (targetTMP != null)
            {
                targetTMP.fontMaterial =
                    runtimeMaterial;
            }
        }

        #endregion


        #region Theme Registration

        private void RegisterToThemeManager()
        {
            if (ThemeManager.Instance == null)
            {
                return;
            }


            ThemeManager.Instance.Register(
                this);
        }


        private void UnregisterFromThemeManager()
        {
            if (ThemeManager.Instance == null)
            {
                return;
            }


            ThemeManager.Instance.Unregister(
                this);
        }

        #endregion


        #region Theme Application


        [SerializeField]
        private Color currentThemeColor = Color.white;

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

        /// <summary>
        /// Called automatically by ThemeManager.
        /// </summary>
        public void ApplyTheme()
        {
            SparkVFXController controller =
                GetComponent<
                    SparkVFXController>();


            if (controller == null)
            {
                return;
            }


            Color themeColor =
                controller.CurrentThemeColor;


            ApplyThemeColor(
                themeColor);
        }


        private void ApplyThemeColor(
            Color color)
        {
            SetColor(
                VFXColor,
                color);


            SetColor(
                VFXGlowColor,
                color);


            SetColor(
                VFXScanColor,
                color);


            SetColor(
                VFXSweepColor,
                color);
        }
        #region Material Properties

        public void SetFloat(
            string property,
            float value)
        {
            if (runtimeMaterial == null)
            {
                return;
            }

            if (!runtimeMaterial.HasProperty(
                    property))
            {
                return;
            }

            runtimeMaterial.SetFloat(
                property,
                value);
        }


        public void SetFloat(
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
                value);
        }


        public void SetColor(
            string property,
            Color value)
        {
            if (runtimeMaterial == null)
            {
                return;
            }

            if (!runtimeMaterial.HasProperty(
                    property))
            {
                return;
            }

            runtimeMaterial.SetColor(
                property,
                value);
        }


        public void SetColor(
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
                value);
        }

        #endregion

        #endregion


        #region Material Restore

        private void RestoreOriginalMaterial()
        {
            if (targetGraphic != null)
            {
                targetGraphic.material =
                    originalGraphicMaterial;
            }


            if (targetTMP != null)
            {
                targetTMP.fontMaterial =
                    originalTMPMaterial;
            }
        }


        private void DestroyRuntimeMaterial()
        {
            if (runtimeMaterial == null)
            {
                return;
            }


            if (Application.isPlaying)
            {
                Destroy(
                    runtimeMaterial);
            }
            else
            {
                DestroyImmediate(
                    runtimeMaterial);
            }


            runtimeMaterial = null;
        }

        #endregion
    }
}