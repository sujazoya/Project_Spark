using UnityEngine;
using UnityEngine.UI;
using ProjectSpark.UI;

[ExecuteAlways]
[RequireComponent(typeof(Image))]
public class UI_Advanced_Neon_Controller : MonoBehaviour
{
    // ============================================================
    // UI IMAGE
    // ============================================================

    private Image uiImage;

    private Material runtimeMaterial;


    // ============================================================
    // SHADER PROPERTY IDS
    // ============================================================

    private static readonly int MainTex =
        Shader.PropertyToID("_MainTex");


    private static readonly int BaseColor =
        Shader.PropertyToID("_Color");


    private static readonly int GradientColorA =
        Shader.PropertyToID("_GradientColorA");


    private static readonly int GradientColorB =
        Shader.PropertyToID("_GradientColorB");


    private static readonly int GradientColorC =
        Shader.PropertyToID("_GradientColorC");

    private static readonly int GradientDirectionProperty =
        Shader.PropertyToID("_GradientDirection");


    private static readonly int GradientSpeed =
        Shader.PropertyToID("_GradientSpeed");


    private static readonly int GradientOffset =
        Shader.PropertyToID("_GradientOffset");


    private static readonly int GradientScaleX =
        Shader.PropertyToID("_GradientScaleX");


    private static readonly int GradientScaleY =
        Shader.PropertyToID("_GradientScaleY");


    private static readonly int OutlineColor =
        Shader.PropertyToID("_OutlineColor");


    private static readonly int OutlineWidth =
        Shader.PropertyToID("_OutlineWidth");


    private static readonly int EmissionColor =
        Shader.PropertyToID("_EmissionColor");


    private static readonly int EmissionIntensity =
        Shader.PropertyToID("_EmissionIntensity");


    private static readonly int GlowColor =
        Shader.PropertyToID("_GlowColor");


    private static readonly int GlowStrength =
        Shader.PropertyToID("_GlowStrength");


    private static readonly int GlowSize =
        Shader.PropertyToID("_GlowSize");


    private static readonly int GlowSoftness =
        Shader.PropertyToID("_GlowSoftness");


    // ============================================================
    // GRADIENT COLORS
    // ============================================================

    [Header("3 Color Gradient")]

    [ColorUsage(true, true)]
    public Color gradientColorA =
        Color.cyan;


    [ColorUsage(true, true)]
    public Color gradientColorB =
        Color.blue;


    [ColorUsage(true, true)]
    public Color gradientColorC =
        Color.magenta;


    // ============================================================
    // GRADIENT MODE
    // ============================================================

    public enum GradientDirection
    {
        Horizontal = 0,

        Vertical = 1,

        Diagonal = 2
    }


    [Header("Gradient Settings")]

    public GradientDirection direction =
        GradientDirection.Horizontal;


    [Tooltip(
        "0 = stopped. " +
        "Positive = forward. " +
        "Negative = reverse."
    )]

    public float animationSpeed =
        0.25f;


    [Range(0f, 1f)]

    public float gradientOffset =
        0f;


    // ============================================================
    // GRADIENT SCALE
    // ============================================================

    [Header("Gradient Scale")]

    public float gradientScaleX =
        1f;


    public float gradientScaleY =
        1f;


    // ============================================================
    // BASE COLOR
    // ============================================================

    [Header("Base Color")]

    [ColorUsage(true, true)]
    public Color baseColor =
        Color.white;


    // ============================================================
    // OUTLINE
    // ============================================================

    [Header("Outline")]

    [ColorUsage(true, true)]
    public Color outlineColor =
        Color.cyan;


    [Range(0f, 0.1f)]

    public float outlineWidth =
        0.01f;


    // ============================================================
    // EMISSION
    // ============================================================

    [Header("Neon Emission")]

    [ColorUsage(true, true)]
    public Color emissionColor =
        Color.cyan;


    [Range(0f, 20f)]

    public float emissionIntensity =
        2f;


    // ============================================================
    // GLOW
    // ============================================================

    [Header("Glow")]

    [ColorUsage(true, true)]
    public Color glowColor =
        Color.cyan;


    [Range(0f, 10f)]

    public float glowStrength =
        1.5f;


    [Range(0f, 0.25f)]

    public float glowSize =
        0.05f;


    [Range(0.001f, 0.25f)]

    public float glowSoftness =
        0.05f;


    // ============================================================
    // INITIALIZATION
    // ============================================================

    private void Awake()
    {
        Initialize();
    }


    private void OnEnable()
    {
        Initialize();
    }


    private void Initialize()
    {
        if (uiImage == null)
        {
            uiImage =
                GetComponent<Image>();
        }


        if (uiImage == null)
        {
            return;
        }


        // --------------------------------------------------------
        // CREATE UNIQUE MATERIAL
        // --------------------------------------------------------

        if (runtimeMaterial == null)
        {
            Shader shader =
                Shader.Find(
                    "Custom/UI Advanced Neon Gradient URP"
                );


            if (shader == null)
            {
                Debug.LogError(
                    "UI Advanced Neon Gradient URP shader not found."
                );

                return;
            }


            runtimeMaterial =
                new Material(
                    shader
                );


            runtimeMaterial.name =
                gameObject.name
                +
                " - UI Neon Runtime Material";


            runtimeMaterial.hideFlags =
                HideFlags.DontSave;
        }


        // --------------------------------------------------------
        // ASSIGN MATERIAL
        // --------------------------------------------------------

        uiImage.material =
            runtimeMaterial;


        // --------------------------------------------------------
        // APPLY SPRITE
        // --------------------------------------------------------

        UpdateMainTexture();


        ApplyProperties();
    }


    // ============================================================
    // UPDATE
    // ============================================================

    private void Update()
    {
        if (uiImage == null)
        {
            Initialize();
        }


        if (runtimeMaterial == null)
        {
            Initialize();
        }


        if (runtimeMaterial == null)
        {
            return;
        }


        UpdateMainTexture();


        ApplyProperties();
    }


    // ============================================================
    // UPDATE IMAGE TEXTURE
    // ============================================================

    private void UpdateMainTexture()
    {
        if (
            uiImage == null
            ||
            runtimeMaterial == null
        )
        {
            return;
        }


        Sprite sprite =
            uiImage.sprite;


        if (sprite == null)
        {
            return;
        }


        Texture texture =
            sprite.texture;


        if (texture == null)
        {
            return;
        }


        runtimeMaterial.SetTexture(
            MainTex,

            texture
        );
    }


    // ============================================================
    // APPLY PROPERTIES
    // ============================================================

    private void ApplyProperties()
    {
        if (
            runtimeMaterial == null
        )
        {
            return;
        }


        // --------------------------------------------------------
        // BASE
        // --------------------------------------------------------

        runtimeMaterial.SetColor(
            BaseColor,

            baseColor
        );


        // --------------------------------------------------------
        // GRADIENT
        // --------------------------------------------------------

        runtimeMaterial.SetColor(
            GradientColorA,

            gradientColorA
        );


        runtimeMaterial.SetColor(
            GradientColorB,

            gradientColorB
        );


        runtimeMaterial.SetColor(
            GradientColorC,

            gradientColorC
        );


        runtimeMaterial.SetFloat(
     GradientDirectionProperty,

     (float)direction
 );


        runtimeMaterial.SetFloat(
            GradientSpeed,

            animationSpeed
        );


        runtimeMaterial.SetFloat(
            GradientOffset,

            gradientOffset
        );


        runtimeMaterial.SetFloat(
            GradientScaleX,

            gradientScaleX
        );


        runtimeMaterial.SetFloat(
            GradientScaleY,

            gradientScaleY
        );


        // --------------------------------------------------------
        // OUTLINE
        // --------------------------------------------------------

        runtimeMaterial.SetColor(
            OutlineColor,

            outlineColor
        );


        runtimeMaterial.SetFloat(
            OutlineWidth,

            outlineWidth
        );


        // --------------------------------------------------------
        // EMISSION
        // --------------------------------------------------------

        runtimeMaterial.SetColor(
            EmissionColor,

            emissionColor
        );


        runtimeMaterial.SetFloat(
            EmissionIntensity,

            emissionIntensity
        );


        // --------------------------------------------------------
        // GLOW
        // --------------------------------------------------------

        runtimeMaterial.SetColor(
            GlowColor,

            glowColor
        );


        runtimeMaterial.SetFloat(
            GlowStrength,

            glowStrength
        );


        runtimeMaterial.SetFloat(
            GlowSize,

            glowSize
        );


        runtimeMaterial.SetFloat(
            GlowSoftness,

            glowSoftness
        );
    }


    // ============================================================
    // RUNTIME API
    // ============================================================

    public void SetGradientColors(
        Color a,
        Color b,
        Color c
    )
    {
        gradientColorA =
            a;

        gradientColorB =
            b;

        gradientColorC =
            c;

        ApplyProperties();
    }


    public void SetGradientDirection(
        GradientDirection newDirection
    )
    {
        direction =
            newDirection;

        ApplyProperties();
    }


    public void SetAnimationSpeed(
        float speed
    )
    {
        animationSpeed =
            speed;

        ApplyProperties();
    }


    public void SetGradientOffset(
        float offset
    )
    {
        gradientOffset =
            Mathf.Repeat(
                offset,

                1f
            );

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
        float size,
        float softness
    )
    {
        glowColor =
            color;

        glowStrength =
            strength;

        glowSize =
            size;

        glowSoftness =
            softness;

        ApplyProperties();
    }
    /// <summary>
    /// Applies a complete Project Spark UI neon theme configuration.
    /// </summary>
    /// <param name="data">
    /// The neon theme data to apply.
    /// </param>
    public void ApplyThemeData(UINeonThemeData data)
    {
        if (data == null)
        {
            return;
        }

        // ========================================================
        // APPLY THEME DATA TO CONTROLLER
        // ========================================================

        baseColor = data.baseColor;

        gradientColorA = data.gradientColorA;
        gradientColorB = data.gradientColorB;
        gradientColorC = data.gradientColorC;

        direction = data.direction;

        animationSpeed = data.animationSpeed;
        gradientOffset = data.gradientOffset;

        gradientScaleX = data.gradientScaleX;
        gradientScaleY = data.gradientScaleY;

        outlineColor = data.outlineColor;
        outlineWidth = data.outlineWidth;

        emissionColor = data.emissionColor;
        emissionIntensity = data.emissionIntensity;

        glowColor = data.glowColor;
        glowStrength = data.glowStrength;
        glowSize = data.glowSize;
        glowSoftness = data.glowSoftness;

        // ========================================================
        // REFRESH THE ACTUAL NEON MATERIAL / SHADER
        // ========================================================

        ApplyProperties();
    }


    // ============================================================
    // CLEANUP
    // ============================================================

    private void OnDestroy()
    {
        if (
            runtimeMaterial != null
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