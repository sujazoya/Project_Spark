using UnityEngine;
using TMPro;

[ExecuteAlways]
[RequireComponent(typeof(TMP_Text))]
public class TMP_Advanced_Neon_Controller : MonoBehaviour
{
    // ============================================================
    // GRADIENT DIRECTION
    // ============================================================

    public enum GradientDirection
    {
        Horizontal = 0,
        Vertical = 1,
        Diagonal = 2
    }
    private static readonly int TextMinID =
    Shader.PropertyToID("_TextMin");

    private static readonly int TextMaxID =
        Shader.PropertyToID("_TextMax");

    // ============================================================
    // TMP COMPONENT
    // ============================================================

    [Header("TextMeshPro")]

    [SerializeField]
    private TMP_Text text;


    // ============================================================
    // GRADIENT
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
    // GRADIENT ANIMATION
    // ============================================================

    [Header("Gradient Animation")]

    [Tooltip(
        "Enable or disable the animated gradient."
    )]

    public bool animateGradient =
        true;


    [Tooltip(
        "Gradient animation speed. " +
        "Positive = forward. " +
        "Negative = reverse."
    )]

    [Range(-10f, 10f)]

    public float gradientSpeed =
        1f;


    [Tooltip(
        "Starting position of the gradient."
    )]

    [Range(0f, 1f)]

    public float gradientOffset =
        0f;


    [Tooltip(
        "0 = Horizontal, " +
        "1 = Vertical, " +
        "2 = Diagonal"
    )]

    public GradientDirection gradientDirection =
        GradientDirection.Horizontal;


    // ============================================================
    // FACE
    // ============================================================

    [Header("Face")]

    [ColorUsage(true, true)]

    public Color faceColor =
        Color.white;


    // ============================================================
    // OUTLINE
    // ============================================================

    [Header("Outline")]

    [ColorUsage(true, true)]

    public Color outlineColor =
        Color.cyan;


    [Range(0f, 0.5f)]

    public float outlineWidth =
        0.05f;


    // ============================================================
    // EMISSION
    // ============================================================

    [Header("Neon Emission")]

    [ColorUsage(true, true)]

    public Color emissionColor =
        Color.cyan;


    [Min(0f)]

    public float emissionIntensity =
        2f;


    // ============================================================
    // GLOW
    // ============================================================

    [Header("Glow")]

    [ColorUsage(true, true)]

    public Color glowColor =
        Color.cyan;


    [Min(0f)]

    public float glowStrength =
        1f;


    [Range(0f, 0.5f)]

    public float glowWidth =
        0.1f;


    // ============================================================
    // MATERIAL
    // ============================================================

    private Material runtimeMaterial;


    private bool initialized;


    // ============================================================
    // SHADER PROPERTY IDs
    // ============================================================

    private static readonly int MainTexID =
        Shader.PropertyToID(
            "_MainTex"
        );


    private static readonly int GradientColorAID =
        Shader.PropertyToID(
            "_GradientColorA"
        );


    private static readonly int GradientColorBID =
        Shader.PropertyToID(
            "_GradientColorB"
        );


    private static readonly int GradientColorCID =
        Shader.PropertyToID(
            "_GradientColorC"
        );


    private static readonly int GradientDirectionID =
        Shader.PropertyToID(
            "_GradientDirection"
        );


    private static readonly int GradientSpeedID =
        Shader.PropertyToID(
            "_GradientSpeed"
        );


    private static readonly int GradientOffsetID =
        Shader.PropertyToID(
            "_GradientOffset"
        );


    private static readonly int FaceColorID =
        Shader.PropertyToID(
            "_FaceColor"
        );


    private static readonly int OutlineColorID =
        Shader.PropertyToID(
            "_OutlineColor"
        );


    private static readonly int OutlineWidthID =
        Shader.PropertyToID(
            "_OutlineWidth"
        );


    private static readonly int EmissionColorID =
        Shader.PropertyToID(
            "_EmissionColor"
        );


    private static readonly int EmissionIntensityID =
        Shader.PropertyToID(
            "_EmissionIntensity"
        );


    private static readonly int GlowColorID =
        Shader.PropertyToID(
            "_GlowColor"
        );


    private static readonly int GlowStrengthID =
        Shader.PropertyToID(
            "_GlowStrength"
        );


    private static readonly int GlowWidthID =
        Shader.PropertyToID(
            "_GlowWidth"
        );


    // ============================================================
    // AWAKE
    // ============================================================

    private void Awake()
    {
        Initialize();
    }


    // ============================================================
    // ON ENABLE
    // ============================================================

    private void OnEnable()
    {
        Initialize();
    }


    // ============================================================
    // UPDATE
    // ============================================================
    private void UpdateTextBounds()
    {
        if (text == null || runtimeMaterial == null)
            return;

        text.ForceMeshUpdate();

        Bounds bounds =
            text.textBounds;

        Vector3 min =
            bounds.min;

        Vector3 max =
            bounds.max;

        runtimeMaterial.SetVector(
            TextMinID,
            new Vector4(
                min.x,
                min.y,
                min.z,
                0f
            )
        );

        runtimeMaterial.SetVector(
            TextMaxID,
            new Vector4(
                max.x,
                max.y,
                max.z,
                0f
            )
        );
    }
    private void Update()
    {
        if (
            text == null
        )
        {
            text =
                GetComponent<TMP_Text>();
        }


        if (
            text == null
        )
        {
            return;
        }


        if (
            !initialized ||
            runtimeMaterial == null
        )
        {
            Initialize();
        }


        if (
            runtimeMaterial == null
        )
        {
            return;
        }


        ApplyAllProperties();
        UpdateTextBounds();
    }
    private static readonly int WeightNormalID =
    Shader.PropertyToID("_WeightNormal");

    private static readonly int WeightBoldID =
        Shader.PropertyToID("_WeightBold");

    private void UpdateTMPPaddingSafe()
    {
        if (text == null)
            return;

        Material material =
            text.fontMaterial;

        if (material == null)
            return;

        if (!material.HasProperty(WeightNormalID))
        {
            Debug.LogError(
                $"TMP shader '{material.shader.name}' is missing _WeightNormal."
            );

            return;
        }

        if (!material.HasProperty(WeightBoldID))
        {
            Debug.LogError(
                $"TMP shader '{material.shader.name}' is missing _WeightBold."
            );

            return;
        }

        UpdateTMPPaddingSafe();
    }

    // ============================================================
    // INITIALIZE
    // ============================================================

    private void Initialize()
    {
        if (
            text == null
        )
        {
            text =
                GetComponent<TMP_Text>();
        }


        if (
            text == null
        )
        {
            return;
        }


        // --------------------------------------------------------
        // Already initialized
        // --------------------------------------------------------

        if (
            runtimeMaterial != null &&
            text.fontMaterial ==
            runtimeMaterial
        )
        {
            initialized =
                true;

            ApplyAllProperties();

            return;
        }


        // --------------------------------------------------------
        // Get current TMP material
        // --------------------------------------------------------

        Material sourceMaterial =
            text.fontMaterial;


        if (
            sourceMaterial == null
        )
        {
            return;
        }


        // --------------------------------------------------------
        // Create unique material instance
        // --------------------------------------------------------

        runtimeMaterial =
            new Material(
                sourceMaterial
            );


        runtimeMaterial.name =
            sourceMaterial.name +
            " - Advanced Neon Runtime";


        // --------------------------------------------------------
        // Assign to TMP
        // --------------------------------------------------------

        text.fontMaterial =
            runtimeMaterial;


        initialized =
            true;


        // --------------------------------------------------------
        // Apply properties
        // --------------------------------------------------------

        ApplyAllProperties();
    }


    // ============================================================
    // APPLY ALL PROPERTIES
    // ============================================================

    private void ApplyAllProperties()
    {
        if (
            runtimeMaterial == null
        )
        {
            return;
        }


        // ========================================================
        // GRADIENT COLORS
        // ========================================================

        runtimeMaterial.SetColor(
            GradientColorAID,
            gradientColorA
        );


        runtimeMaterial.SetColor(
            GradientColorBID,
            gradientColorB
        );


        runtimeMaterial.SetColor(
            GradientColorCID,
            gradientColorC
        );


        // ========================================================
        // GRADIENT DIRECTION
        // ========================================================

        runtimeMaterial.SetFloat(
            GradientDirectionID,
            (float)
            gradientDirection
        );


        // ========================================================
        // GRADIENT SPEED
        // ========================================================

        // The shader itself uses _Time.y.
        //
        // Speed = 0
        // means animation is OFF.
        //
        // Positive speed
        // moves forward.
        //
        // Negative speed
        // moves backward.

        float actualSpeed =
            animateGradient
                ? gradientSpeed
                : 0f;


        runtimeMaterial.SetFloat(
            GradientSpeedID,
            actualSpeed
        );


        // ========================================================
        // GRADIENT OFFSET
        // ========================================================

        runtimeMaterial.SetFloat(
            GradientOffsetID,
            gradientOffset
        );


        // ========================================================
        // FACE
        // ========================================================

        runtimeMaterial.SetColor(
            FaceColorID,
            faceColor
        );


        // ========================================================
        // OUTLINE
        // ========================================================

        runtimeMaterial.SetColor(
            OutlineColorID,
            outlineColor
        );


        runtimeMaterial.SetFloat(
            OutlineWidthID,
            outlineWidth
        );


        // ========================================================
        // EMISSION
        // ========================================================

        runtimeMaterial.SetColor(
            EmissionColorID,
            emissionColor
        );


        runtimeMaterial.SetFloat(
            EmissionIntensityID,
            emissionIntensity
        );


        // ========================================================
        // GLOW
        // ========================================================

        runtimeMaterial.SetColor(
            GlowColorID,
            glowColor
        );


        runtimeMaterial.SetFloat(
            GlowStrengthID,
            glowStrength
        );


        runtimeMaterial.SetFloat(
            GlowWidthID,
            glowWidth
        );


        // ========================================================
        // UPDATE TMP
        // ========================================================

        if (
            text != null
        )
        {
            text.UpdateMeshPadding();
        }
    }


    // ============================================================
    // SET 3 COLOR GRADIENT
    // ============================================================

    public void SetGradient(
        Color colorA,
        Color colorB,
        Color colorC
    )
    {
        gradientColorA =
            colorA;

        gradientColorB =
            colorB;

        gradientColorC =
            colorC;


        ApplyAllProperties();
    }


    // ============================================================
    // SET GRADIENT SPEED
    // ============================================================

    public void SetGradientSpeed(
        float speed
    )
    {
        gradientSpeed =
            speed;


        ApplyAllProperties();
    }


    // ============================================================
    // ENABLE / DISABLE ANIMATION
    // ============================================================

    public void SetGradientAnimation(
        bool enabled
    )
    {
        animateGradient =
            enabled;


        ApplyAllProperties();
    }


    // ============================================================
    // SET GRADIENT OFFSET
    // ============================================================

    public void SetGradientOffset(
        float offset
    )
    {
        gradientOffset =
            Mathf.Repeat(
                offset,
                1f
            );


        ApplyAllProperties();
    }


    // ============================================================
    // SET GRADIENT DIRECTION
    // ============================================================

    public void SetGradientDirection(
        GradientDirection direction
    )
    {
        gradientDirection =
            direction;


        ApplyAllProperties();
    }


    // ============================================================
    // SET OUTLINE
    // ============================================================

    public void SetOutline(
        Color color,
        float width
    )
    {
        outlineColor =
            color;


        outlineWidth =
            Mathf.Clamp(
                width,
                0f,
                0.5f
            );


        ApplyAllProperties();
    }


    // ============================================================
    // SET EMISSION
    // ============================================================

    public void SetEmission(
        Color color,
        float intensity
    )
    {
        emissionColor =
            color;


        emissionIntensity =
            Mathf.Max(
                0f,
                intensity
            );


        ApplyAllProperties();
    }


    // ============================================================
    // SET GLOW
    // ============================================================

    public void SetGlow(
        Color color,
        float strength,
        float width
    )
    {
        glowColor =
            color;


        glowStrength =
            Mathf.Max(
                0f,
                strength
            );


        glowWidth =
            Mathf.Clamp(
                width,
                0f,
                0.5f
            );


        ApplyAllProperties();
    }


    // ============================================================
    // SET EVERYTHING
    // ============================================================

    public void SetAll(
        Color colorA,
        Color colorB,
        Color colorC,

        GradientDirection direction,

        float speed,

        float offset,

        Color outline,

        float outlineSize,

        Color emission,

        float emissionPower,

        Color glow,

        float glowPower,

        float glowSize
    )
    {
        gradientColorA =
            colorA;


        gradientColorB =
            colorB;


        gradientColorC =
            colorC;


        gradientDirection =
            direction;


        gradientSpeed =
            speed;


        gradientOffset =
            Mathf.Repeat(
                offset,
                1f
            );


        outlineColor =
            outline;


        outlineWidth =
            Mathf.Clamp(
                outlineSize,
                0f,
                0.5f
            );


        emissionColor =
            emission;


        emissionIntensity =
            Mathf.Max(
                0f,
                emissionPower
            );


        glowColor =
            glow;


        glowStrength =
            Mathf.Max(
                0f,
                glowPower
            );


        glowWidth =
            Mathf.Clamp(
                glowSize,
                0f,
                0.5f
            );


        ApplyAllProperties();
    }


    // ============================================================
    // PRESET: CYAN BLUE
    // ============================================================

    public void PresetCyanBlue()
    {
        gradientColorA =
            Color.cyan;


        gradientColorB =
            Color.blue;


        gradientColorC =
            Color.magenta;


        gradientDirection =
            GradientDirection.Horizontal;


        gradientSpeed =
            1f;


        gradientOffset =
            0f;


        animateGradient =
            true;


        faceColor =
            Color.white;


        outlineColor =
            Color.cyan;


        outlineWidth =
            0.05f;


        emissionColor =
            Color.cyan;


        emissionIntensity =
            3f;


        glowColor =
            Color.cyan;


        glowStrength =
            2f;


        glowWidth =
            0.1f;


        ApplyAllProperties();
    }


    // ============================================================
    // PRESET: VERTICAL NEON
    // ============================================================

    public void PresetVertical()
    {
        gradientColorA =
            Color.cyan;


        gradientColorB =
            Color.blue;


        gradientColorC =
            Color.magenta;


        gradientDirection =
            GradientDirection.Vertical;


        gradientSpeed =
            1f;


        gradientOffset =
            0f;


        animateGradient =
            true;


        faceColor =
            Color.white;


        outlineColor =
            Color.cyan;


        outlineWidth =
            0.06f;


        emissionColor =
            Color.cyan;


        emissionIntensity =
            4f;


        glowColor =
            Color.cyan;


        glowStrength =
            2f;


        glowWidth =
            0.1f;


        ApplyAllProperties();
    }


    // ============================================================
    // PRESET: PURPLE PINK
    // ============================================================

    public void PresetPurplePink()
    {
        gradientColorA =
            new Color(
                0.5f,
                0f,
                1f
            );


        gradientColorB =
            Color.magenta;


        gradientColorC =
            Color.cyan;


        gradientDirection =
            GradientDirection.Horizontal;


        gradientSpeed =
            1.5f;


        gradientOffset =
            0f;


        animateGradient =
            true;


        faceColor =
            Color.white;


        outlineColor =
            Color.magenta;


        outlineWidth =
            0.07f;


        emissionColor =
            Color.magenta;


        emissionIntensity =
            4f;


        glowColor =
            Color.magenta;


        glowStrength =
            2f;


        glowWidth =
            0.12f;


        ApplyAllProperties();
    }


    // ============================================================
    // PRESET: WARNING RED
    // ============================================================

    public void PresetWarning()
    {
        gradientColorA =
            Color.red;


        gradientColorB =
            new Color(
                1f,
                0.3f,
                0f
            );


        gradientColorC =
            Color.yellow;


        gradientDirection =
            GradientDirection.Horizontal;


        gradientSpeed =
            2f;


        gradientOffset =
            0f;


        animateGradient =
            true;


        faceColor =
            Color.white;


        outlineColor =
            Color.red;


        outlineWidth =
            0.08f;


        emissionColor =
            Color.red;


        emissionIntensity =
            5f;


        glowColor =
            Color.red;


        glowStrength =
            3f;


        glowWidth =
            0.15f;


        ApplyAllProperties();
    }


    // ============================================================
    // STOP ANIMATION
    // ============================================================

    public void StopGradientAnimation()
    {
        animateGradient =
            false;


        ApplyAllProperties();
    }


    // ============================================================
    // START ANIMATION
    // ============================================================

    public void StartGradientAnimation()
    {
        animateGradient =
            true;


        ApplyAllProperties();
    }


    // ============================================================
    // REVERSE ANIMATION
    // ============================================================

    public void ReverseGradient()
    {
        gradientSpeed =
            -gradientSpeed;


        ApplyAllProperties();
    }


    // ============================================================
    // RESET OFFSET
    // ============================================================

    public void ResetGradient()
    {
        gradientOffset =
            0f;


        ApplyAllProperties();
    }


    // ============================================================
    // CLEANUP
    // ============================================================

    private void OnDestroy()
    {
        CleanupMaterial();
    }


    private void CleanupMaterial()
    {
        if (
            runtimeMaterial == null
        )
        {
            return;
        }


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


        runtimeMaterial =
            null;


        initialized =
            false;
    }
}