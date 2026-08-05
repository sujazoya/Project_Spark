using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
[RequireComponent(typeof(Image))]
public class AdvancedUI_Image_Neon_Controller : MonoBehaviour
{
    // ============================================================
    // IMAGE
    // ============================================================

    private Image image;

    private Material runtimeMaterial;


    // ============================================================
    // SHADER
    // ============================================================

    private const string ShaderName =
        "Custom/UI/Advanced Image Neon URP";


    // ============================================================
    // GRADIENT
    // ============================================================

    [Header("GRADIENT")]

    [ColorUsage(true, true)]
    public Color gradientTop =
        Color.cyan;

    [ColorUsage(true, true)]
    public Color gradientMiddle =
        Color.blue;

    [ColorUsage(true, true)]
    public Color gradientBottom =
        Color.magenta;


    [Range(-1f, 1f)]
    public float gradientOffset =
        0f;


    [Range(0.01f, 5f)]
    public float gradientScale =
        1f;


    public GradientDirection gradientDirection =
        GradientDirection.Vertical;


    public enum GradientDirection
    {
        Vertical = 0,

        Horizontal = 1
    }


    // ============================================================
    // BASE
    // ============================================================

    [Header("BASE")]

    [ColorUsage(true, true)]
    public Color baseColor =
        Color.white;


    // ============================================================
    // ROUNDED CORNERS
    // ============================================================

    [Header("ROUNDED CORNERS")]

    [Range(0f, 0.5f)]
    public float cornerRadius =
        0.05f;


    [Range(0.001f, 0.2f)]
    public float cornerSoftness =
        0.02f;


    // ============================================================
    // OUTLINE
    // ============================================================

    [Header("OUTLINE")]

    [ColorUsage(true, true)]
    public Color outlineColor =
        Color.cyan;


    [Range(0f, 0.1f)]
    public float outlineWidth =
        0.01f;


    [Range(0.001f, 0.1f)]
    public float outlineSoftness =
        0.01f;


    // ============================================================
    // BEVEL
    // ============================================================

    [Header("BEVEL")]

    public bool bevelEnabled =
        true;


    [Range(0f, 0.5f)]
    public float bevelWidth =
        0.08f;


    [Range(0f, 2f)]
    public float bevelStrength =
        1f;


    [Range(0.001f, 0.2f)]
    public float bevelSoftness =
        0.03f;


    [ColorUsage(true, true)]
    public Color bevelHighlightColor =
        Color.white;


    [ColorUsage(true, true)]
    public Color bevelShadowColor =
        Color.black;


    // ============================================================
    // INNER SHADOW
    // ============================================================

    [Header("INNER SHADOW")]

    public bool innerShadowEnabled =
        true;


    [ColorUsage(true, true)]
    public Color innerShadowColor =
        Color.black;


    [Range(0f, 2f)]
    public float innerShadowStrength =
        0.5f;


    [Range(-0.5f, 0.5f)]
    public float innerShadowOffsetX =
        0.02f;


    [Range(-0.5f, 0.5f)]
    public float innerShadowOffsetY =
        -0.02f;


    [Range(0.001f, 0.2f)]
    public float innerShadowSoftness =
        0.05f;


    // ============================================================
    // GLOW
    // ============================================================

    [Header("GLOW")]

    public bool glowEnabled =
        true;


    [ColorUsage(true, true)]
    public Color glowColor =
        Color.cyan;


    [Range(0f, 5f)]
    public float glowStrength =
        1f;


    [Range(0.001f, 0.5f)]
    public float glowSoftness =
        0.1f;


    // ============================================================
    // EMISSION
    // ============================================================

    [Header("EMISSION")]

    public bool emissionEnabled =
        true;


    [ColorUsage(true, true)]
    public Color emissionColor =
        Color.cyan;


    [Range(0f, 20f)]
    public float emissionIntensity =
        2f;


    // ============================================================
    // FRESNEL
    // ============================================================

    [Header("FRESNEL")]

    public bool fresnelEnabled =
        true;


    [ColorUsage(true, true)]
    public Color fresnelColor =
        Color.cyan;


    [Range(0f, 5f)]
    public float fresnelStrength =
        1f;


    [Range(0.1f, 10f)]
    public float fresnelPower =
        2f;


    // ============================================================
    // SCANLINES
    // ============================================================

    [Header("SCANLINES")]

    public bool scanlineEnabled =
        false;


    [ColorUsage(true, true)]
    public Color scanlineColor =
        Color.cyan;


    [Range(1f, 200f)]
    public float scanlineDensity =
        50f;


    [Range(0f, 1f)]
    public float scanlineStrength =
        0.2f;


    [Range(-10f, 10f)]
    public float scanlineSpeed =
        1f;


    // ============================================================
    // DISTORTION
    // ============================================================

    [Header("DISTORTION")]

    public bool distortionEnabled =
        false;


    [Range(0f, 0.1f)]
    public float distortionStrength =
        0.01f;


    [Range(1f, 100f)]
    public float distortionScale =
        20f;


    [Range(-10f, 10f)]
    public float distortionSpeed =
        1f;


    // ============================================================
    // COLOR CORRECTION
    // ============================================================

    [Header("COLOR CORRECTION")]

    [Range(0f, 3f)]
    public float brightness =
        1f;


    [Range(0f, 3f)]
    public float contrast =
        1f;


    [Range(0f, 3f)]
    public float saturation =
        1f;


    // ============================================================
    // PROPERTY IDS
    // ============================================================

    private static readonly int MainTexID =
        Shader.PropertyToID("_MainTex");

    private static readonly int ColorID =
        Shader.PropertyToID("_Color");

    private static readonly int GradientTopID =
        Shader.PropertyToID("_GradientTop");

    private static readonly int GradientMiddleID =
        Shader.PropertyToID("_GradientMiddle");

    private static readonly int GradientBottomID =
        Shader.PropertyToID("_GradientBottom");

    private static readonly int GradientOffsetID =
        Shader.PropertyToID("_GradientOffset");

    private static readonly int GradientScaleID =
        Shader.PropertyToID("_GradientScale");

    private static readonly int GradientDirectionID =
        Shader.PropertyToID("_GradientDirection");

    private static readonly int CornerRadiusID =
        Shader.PropertyToID("_CornerRadius");

    private static readonly int CornerSoftnessID =
        Shader.PropertyToID("_CornerSoftness");

    private static readonly int OutlineColorID =
        Shader.PropertyToID("_OutlineColor");

    private static readonly int OutlineWidthID =
        Shader.PropertyToID("_OutlineWidth");

    private static readonly int OutlineSoftnessID =
        Shader.PropertyToID("_OutlineSoftness");

    private static readonly int BevelEnabledID =
        Shader.PropertyToID("_BevelEnabled");

    private static readonly int BevelWidthID =
        Shader.PropertyToID("_BevelWidth");

    private static readonly int BevelStrengthID =
        Shader.PropertyToID("_BevelStrength");

    private static readonly int BevelSoftnessID =
        Shader.PropertyToID("_BevelSoftness");

    private static readonly int BevelHighlightID =
        Shader.PropertyToID("_BevelHighlightColor");

    private static readonly int BevelShadowID =
        Shader.PropertyToID("_BevelShadowColor");

    private static readonly int InnerShadowEnabledID =
        Shader.PropertyToID("_InnerShadowEnabled");

    private static readonly int InnerShadowColorID =
        Shader.PropertyToID("_InnerShadowColor");

    private static readonly int InnerShadowStrengthID =
        Shader.PropertyToID("_InnerShadowStrength");

    private static readonly int InnerShadowOffsetXID =
        Shader.PropertyToID("_InnerShadowOffsetX");

    private static readonly int InnerShadowOffsetYID =
        Shader.PropertyToID("_InnerShadowOffsetY");

    private static readonly int InnerShadowSoftnessID =
        Shader.PropertyToID("_InnerShadowSoftness");

    private static readonly int GlowEnabledID =
        Shader.PropertyToID("_GlowEnabled");

    private static readonly int GlowColorID =
        Shader.PropertyToID("_GlowColor");

    private static readonly int GlowStrengthID =
        Shader.PropertyToID("_GlowStrength");

    private static readonly int GlowSoftnessID =
        Shader.PropertyToID("_GlowSoftness");

    private static readonly int EmissionEnabledID =
        Shader.PropertyToID("_EmissionEnabled");

    private static readonly int EmissionColorID =
        Shader.PropertyToID("_EmissionColor");

    private static readonly int EmissionIntensityID =
        Shader.PropertyToID("_EmissionIntensity");

    private static readonly int FresnelEnabledID =
        Shader.PropertyToID("_FresnelEnabled");

    private static readonly int FresnelColorID =
        Shader.PropertyToID("_FresnelColor");

    private static readonly int FresnelStrengthID =
        Shader.PropertyToID("_FresnelStrength");

    private static readonly int FresnelPowerID =
        Shader.PropertyToID("_FresnelPower");

    private static readonly int ScanlineEnabledID =
        Shader.PropertyToID("_ScanlineEnabled");

    private static readonly int ScanlineColorID =
        Shader.PropertyToID("_ScanlineColor");

    private static readonly int ScanlineDensityID =
        Shader.PropertyToID("_ScanlineDensity");

    private static readonly int ScanlineStrengthID =
        Shader.PropertyToID("_ScanlineStrength");

    private static readonly int ScanlineSpeedID =
        Shader.PropertyToID("_ScanlineSpeed");

    private static readonly int DistortionEnabledID =
        Shader.PropertyToID("_DistortionEnabled");

    private static readonly int DistortionStrengthID =
        Shader.PropertyToID("_DistortionStrength");

    private static readonly int DistortionScaleID =
        Shader.PropertyToID("_DistortionScale");

    private static readonly int DistortionSpeedID =
        Shader.PropertyToID("_DistortionSpeed");

    private static readonly int BrightnessID =
        Shader.PropertyToID("_Brightness");

    private static readonly int ContrastID =
        Shader.PropertyToID("_Contrast");

    private static readonly int SaturationID =
        Shader.PropertyToID("_Saturation");


    // ============================================================
    // UNITY
    // ============================================================

    private void Awake()
    {
        Initialize();
    }


    private void OnEnable()
    {
        Initialize();
    }


    private void OnValidate()
    {
        if (
            !Application.isPlaying
        )
        {
            Initialize();
        }

        ApplyProperties();
    }


    // ============================================================
    // INITIALIZE
    // ============================================================

    private void Initialize()
    {
        image =
            GetComponent<Image>();


        if (
            image ==
            null
        )
        {
            return;
        }


        Shader shader =
            Shader.Find(
                ShaderName
            );


        if (
            shader ==
            null
        )
        {
            Debug.LogError(
                "Shader not found: " +
                ShaderName
            );

            return;
        }


        // ========================================================
        // CREATE UNIQUE MATERIAL
        // ========================================================

        if (
            runtimeMaterial ==
            null
            ||
            runtimeMaterial.shader !=
            shader
        )
        {
            runtimeMaterial =
                new Material(
                    shader
                );


            runtimeMaterial.name =
                image.name +
                " - Advanced UI Neon";


            runtimeMaterial.hideFlags =
                HideFlags.DontSave;
        }


        // ========================================================
        // MAIN IMAGE
        // ========================================================

        if (
            image.sprite !=
            null
        )
        {
            runtimeMaterial.SetTexture(
                MainTexID,

                image.sprite.texture
            );
        }


        image.material =
            runtimeMaterial;


        ApplyProperties();
    }


    // ============================================================
    // APPLY ALL PROPERTIES
    // ============================================================

    private void ApplyProperties()
    {
        if (
            runtimeMaterial ==
            null
        )
        {
            return;
        }


        // GRADIENT

        runtimeMaterial.SetColor(
            GradientTopID,

            gradientTop
        );


        runtimeMaterial.SetColor(
            GradientMiddleID,

            gradientMiddle
        );


        runtimeMaterial.SetColor(
            GradientBottomID,

            gradientBottom
        );


        runtimeMaterial.SetFloat(
            GradientOffsetID,

            gradientOffset
        );


        runtimeMaterial.SetFloat(
            GradientScaleID,

            gradientScale
        );


        runtimeMaterial.SetFloat(
            GradientDirectionID,

            (float)
            gradientDirection
        );


        // BASE

        runtimeMaterial.SetColor(
            ColorID,

            baseColor
        );


        // ROUNDED

        runtimeMaterial.SetFloat(
            CornerRadiusID,

            cornerRadius
        );


        runtimeMaterial.SetFloat(
            CornerSoftnessID,

            cornerSoftness
        );


        // OUTLINE

        runtimeMaterial.SetColor(
            OutlineColorID,

            outlineColor
        );


        runtimeMaterial.SetFloat(
            OutlineWidthID,

            outlineWidth
        );


        runtimeMaterial.SetFloat(
            OutlineSoftnessID,

            outlineSoftness
        );


        // BEVEL

        runtimeMaterial.SetFloat(
            BevelEnabledID,

            bevelEnabled
                ? 1f
                : 0f
        );


        runtimeMaterial.SetFloat(
            BevelWidthID,

            bevelWidth
        );


        runtimeMaterial.SetFloat(
            BevelStrengthID,

            bevelStrength
        );


        runtimeMaterial.SetFloat(
            BevelSoftnessID,

            bevelSoftness
        );


        runtimeMaterial.SetColor(
            BevelHighlightID,

            bevelHighlightColor
        );


        runtimeMaterial.SetColor(
            BevelShadowID,

            bevelShadowColor
        );


        // INNER SHADOW

        runtimeMaterial.SetFloat(
            InnerShadowEnabledID,

            innerShadowEnabled
                ? 1f
                : 0f
        );


        runtimeMaterial.SetColor(
            InnerShadowColorID,

            innerShadowColor
        );


        runtimeMaterial.SetFloat(
            InnerShadowStrengthID,

            innerShadowStrength
        );


        runtimeMaterial.SetFloat(
            InnerShadowOffsetXID,

            innerShadowOffsetX
        );


        runtimeMaterial.SetFloat(
            InnerShadowOffsetYID,

            innerShadowOffsetY
        );


        runtimeMaterial.SetFloat(
            InnerShadowSoftnessID,

            innerShadowSoftness
        );


        // GLOW

        runtimeMaterial.SetFloat(
            GlowEnabledID,

            glowEnabled
                ? 1f
                : 0f
        );


        runtimeMaterial.SetColor(
            GlowColorID,

            glowColor
        );


        runtimeMaterial.SetFloat(
            GlowStrengthID,

            glowStrength
        );


        runtimeMaterial.SetFloat(
            GlowSoftnessID,

            glowSoftness
        );


        // EMISSION

        runtimeMaterial.SetFloat(
            EmissionEnabledID,

            emissionEnabled
                ? 1f
                : 0f
        );


        runtimeMaterial.SetColor(
            EmissionColorID,

            emissionColor
        );


        runtimeMaterial.SetFloat(
            EmissionIntensityID,

            emissionIntensity
        );


        // FRESNEL

        runtimeMaterial.SetFloat(
            FresnelEnabledID,

            fresnelEnabled
                ? 1f
                : 0f
        );


        runtimeMaterial.SetColor(
            FresnelColorID,

            fresnelColor
        );


        runtimeMaterial.SetFloat(
            FresnelStrengthID,

            fresnelStrength
        );


        runtimeMaterial.SetFloat(
            FresnelPowerID,

            fresnelPower
        );


        // SCANLINES

        runtimeMaterial.SetFloat(
            ScanlineEnabledID,

            scanlineEnabled
                ? 1f
                : 0f
        );


        runtimeMaterial.SetColor(
            ScanlineColorID,

            scanlineColor
        );


        runtimeMaterial.SetFloat(
            ScanlineDensityID,

            scanlineDensity
        );


        runtimeMaterial.SetFloat(
            ScanlineStrengthID,

            scanlineStrength
        );


        runtimeMaterial.SetFloat(
            ScanlineSpeedID,

            scanlineSpeed
        );


        // DISTORTION

        runtimeMaterial.SetFloat(
            DistortionEnabledID,

            distortionEnabled
                ? 1f
                : 0f
        );


        runtimeMaterial.SetFloat(
            DistortionStrengthID,

            distortionStrength
        );


        runtimeMaterial.SetFloat(
            DistortionScaleID,

            distortionScale
        );


        runtimeMaterial.SetFloat(
            DistortionSpeedID,

            distortionSpeed
        );


        // COLOR CORRECTION

        runtimeMaterial.SetFloat(
            BrightnessID,

            brightness
        );


        runtimeMaterial.SetFloat(
            ContrastID,

            contrast
        );


        runtimeMaterial.SetFloat(
            SaturationID,

            saturation
        );
    }


    // ============================================================
    // RUNTIME API
    // ============================================================

    public void SetGradient(
        Color top,

        Color middle,

        Color bottom
    )
    {
        gradientTop =
            top;

        gradientMiddle =
            middle;

        gradientBottom =
            bottom;

        ApplyProperties();
    }


    public void SetGradientDirection(
        GradientDirection direction
    )
    {
        gradientDirection =
            direction;

        ApplyProperties();
    }


    public void SetOutline(
        Color color,

        float width
    )
    {
        outlineColor =
            color;

        outlineWidth =
            width;

        ApplyProperties();
    }


    public void SetEmission(
        Color color,

        float intensity
    )
    {
        emissionColor =
            color;

        emissionIntensity =
            intensity;

        ApplyProperties();
    }


    public void SetGlow(
        Color color,

        float strength,

        float softness
    )
    {
        glowColor =
            color;

        glowStrength =
            strength;

        glowSoftness =
            softness;

        ApplyProperties();
    }


    public void SetBevel(
        bool enabled,

        float width,

        float strength
    )
    {
        bevelEnabled =
            enabled;

        bevelWidth =
            width;

        bevelStrength =
            strength;

        ApplyProperties();
    }


    public void SetBrightness(
        float value
    )
    {
        brightness =
            value;

        ApplyProperties();
    }


    public void SetColorCorrection(
        float newBrightness,

        float newContrast,

        float newSaturation
    )
    {
        brightness =
            newBrightness;

        contrast =
            newContrast;

        saturation =
            newSaturation;

        ApplyProperties();
    }


    // ============================================================
    // CLEANUP
    // ============================================================

    private void OnDestroy()
    {
        if (
            runtimeMaterial !=
            null
        )
        {
            if (
                Application.isPlaying
            )
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
        }
    }
}