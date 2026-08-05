using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.UI
{
    /// <summary>
    /// Controls the Project Spark UI VFX Shader.
    /// Works on Image, RawImage and any Graphic.
    /// </summary>
    [RequireComponent(typeof(Graphic))]
    public class UIVFXController : MonoBehaviour
    {
        private static readonly int BaseColorID = Shader.PropertyToID("_BaseColor");
        private static readonly int VFXColorID = Shader.PropertyToID("_VFXColor");
        private static readonly int GlowColorID = Shader.PropertyToID("_GlowColor");
        private static readonly int ScanColorID = Shader.PropertyToID("_ScanColor");
        private static readonly int SweepColorID = Shader.PropertyToID("_SweepColor");

        private static readonly int GlowID = Shader.PropertyToID("_Glow");
        private static readonly int ScanID = Shader.PropertyToID("_Scan");
        private static readonly int SweepID = Shader.PropertyToID("_Sweep");
        private static readonly int FlashID = Shader.PropertyToID("_Flash");

        private static readonly int GlitchID = Shader.PropertyToID("_Glitch");
        private static readonly int FlickerID = Shader.PropertyToID("_Flicker");

        private static readonly int DissolveID = Shader.PropertyToID("_Dissolve");
        private static readonly int RevealID = Shader.PropertyToID("_Reveal");

        private static readonly int SweepPositionID = Shader.PropertyToID("_SweepPosition");
        private static readonly int ScanEnabledID = Shader.PropertyToID("_ScanEnabled");

        [Header("Target")]
        [SerializeField] private Graphic targetGraphic;

        private Material runtimeMaterial;

        [Header("Colors")]
        public Color baseColor = Color.white;
        public Color vfxColor = Color.cyan;
        public Color glowColor = Color.cyan;
        public Color scanColor = Color.cyan;
        public Color sweepColor = Color.cyan;

        [Header("Effects")]
        [Range(0, 5)] public float glow;
        [Range(0, 5)] public float scan;
        [Range(0, 5)] public float sweep;
        [Range(0, 5)] public float flash;

        [Header("Animation")]
        [Range(-1, 2)] public float sweepPosition = 0.5f;
        public bool animateSweep = false;
        public float sweepSpeed = 1f;

        [Header("Noise")]
        [Range(0, 1)] public float glitch;
        [Range(0, 1)] public float flicker;

        [Header("Visibility")]
        [Range(0, 1)] public float dissolve;
        [Range(0, 1)] public float reveal = 1f;

        public bool scanEnabled = true;

        private float scanTimer;

        private void Awake()
        {
            if (targetGraphic == null)
                targetGraphic = GetComponent<Graphic>();

            runtimeMaterial = Instantiate(targetGraphic.material);
            runtimeMaterial.name += " (Runtime)";
            targetGraphic.material = runtimeMaterial;

            Apply();
        }
        private void OnEnable()
        {
            scanTimer = 0f;
            sweepPosition = -1f;

            reveal = 1f;
            dissolve = 0f;

            Apply();
        }

        private void Update()
        {
            if (animateSweep)
            {
                sweepPosition += Time.deltaTime * sweepSpeed;

                if (sweepPosition > 2f)
                    sweepPosition = -1f;
            }

            if (!animateSweep)
                return;

            scanTimer += Time.deltaTime;

            sweepPosition = Mathf.Lerp(
                -1f,
                2f,
                scanTimer * sweepSpeed);

            if (sweepPosition >= 2f)
            {
                scanTimer = 0f;
                sweepPosition = -1f;
            }

            Apply();
        }

        public void Apply()
        {
            if (runtimeMaterial == null)
                return;

            runtimeMaterial.SetColor(BaseColorID, baseColor);
            runtimeMaterial.SetColor(VFXColorID, vfxColor);
            runtimeMaterial.SetColor(GlowColorID, glowColor);
            runtimeMaterial.SetColor(ScanColorID, scanColor);
            runtimeMaterial.SetColor(SweepColorID, sweepColor);

            runtimeMaterial.SetFloat(GlowID, glow);
            runtimeMaterial.SetFloat(ScanID, scan);
            runtimeMaterial.SetFloat(SweepID, sweep);
            runtimeMaterial.SetFloat(FlashID, flash);

            runtimeMaterial.SetFloat(GlitchID, glitch);
            runtimeMaterial.SetFloat(FlickerID, flicker);

            runtimeMaterial.SetFloat(DissolveID, dissolve);
            runtimeMaterial.SetFloat(RevealID, reveal);

            runtimeMaterial.SetFloat(SweepPositionID, sweepPosition);
            runtimeMaterial.SetFloat(ScanEnabledID, scanEnabled ? 1f : 0f);
        }

        #region Public API

        public void SetGlow(float value)
        {
            glow = value;
        }

        public void SetScan(float value)
        {
            scan = value;
        }

        public void SetSweep(float value)
        {
            sweep = value;
        }

        public void SetFlash(float value)
        {
            flash = value;
        }

        public void SetGlitch(float value)
        {
            glitch = value;
        }

        public void SetFlicker(float value)
        {
            flicker = value;
        }

        public void SetReveal(float value)
        {
            reveal = value;
        }

        public void SetDissolve(float value)
        {
            dissolve = value;
        }

        public void SetSweepPosition(float value)
        {
            sweepPosition = value;
        }

        public void EnableScan(bool value)
        {
            scanEnabled = value;
        }

        public void SetTheme(Color color)
        {
            vfxColor = color;
            glowColor = color;
            scanColor = color;
            sweepColor = color;
        }

        #endregion

        private void OnDestroy()
        {
            if (runtimeMaterial != null)
                Destroy(runtimeMaterial);
        }
    }
}