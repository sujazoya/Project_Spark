using UnityEngine;
using TMPro;

[ExecuteAlways]
[RequireComponent(typeof(TMP_Text))]
public class TMP_Advanced_Neon_ControllerStandalone : MonoBehaviour
{
    // ============================================================
    // COMPONENT
    // ============================================================

    private TMP_Text tmpText;

    private Material runtimeMaterial;


    // ============================================================
    // SHADER PROPERTY IDS
    // ============================================================

    private static readonly int GradientColorA =
        Shader.PropertyToID("_GradientColorA");

    private static readonly int GradientColorB =
        Shader.PropertyToID("_GradientColorB");

    private static readonly int GradientColorC =
        Shader.PropertyToID("_GradientColorC");


    private static readonly int GradientDirection =
        Shader.PropertyToID("_GradientDirection");

    private static readonly int GradientSpeed =
        Shader.PropertyToID("_GradientSpeed");

    private static readonly int GradientOffset =
        Shader.PropertyToID("_GradientOffset");


    private static readonly int TextMin =
        Shader.PropertyToID("_TextMin");

    private static readonly int TextMax =
        Shader.PropertyToID("_TextMax");


    private static readonly int OutlineColor =
        Shader.PropertyToID("_OutlineColor");

    private static readonly int OutlineWidth =
        Shader.PropertyToID("_OutlineWidth");

    private static readonly int OutlineSoftness =
        Shader.PropertyToID("_OutlineSoftness");


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
    // GRADIENT
    // ============================================================

    [Header("Gradient Colors")]

    [ColorUsage(true, true)]
    public Color colorA =
        Color.cyan;


    [ColorUsage(true, true)]
    public Color colorB =
        Color.blue;


    [ColorUsage(true, true)]
    public Color colorC =
        Color.magenta;


    public enum GradientMode
    {
        Horizontal = 0,
        Vertical = 1,
        Diagonal = 2
    }


    [Header("Gradient Settings")]

    public GradientMode gradientMode =
        GradientMode.Horizontal;


    [Tooltip(
        "0 = no animation. " +
        "Positive = moves forward. " +
        "Negative = moves backward."
    )]

    public float gradientSpeed =
        0.25f;


    [Range(0f, 1f)]

    public float gradientOffset =
        0f;


    // ============================================================
    // OUTLINE
    // ============================================================

    [Header("Outline")]

    [ColorUsage(true, true)]
    public Color outlineColor =
        Color.black;


    [Range(0f, 1f)]

    public float outlineWidth =
        0.15f;


    [Range(0f, 1f)]

    public float outlineSoftness =
        0.02f;


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


    [Range(0f, 1f)]

    public float glowSize =
        0.25f;


    [Range(0.001f, 1f)]

    public float glowSoftness =
        0.25f;


    // ============================================================
    // UPDATE SETTINGS
    // ============================================================

    [Header("Runtime")]

    public bool updateBoundsEveryFrame =
        true;


    // ============================================================
    // INITIALIZE
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
        if (tmpText == null)
        {
            tmpText =
                GetComponent<TMP_Text>();
        }


        if (tmpText == null)
        {
            return;
        }


        // --------------------------------------------------------
        // Create unique runtime material
        // --------------------------------------------------------

        if (runtimeMaterial == null)
        {
            runtimeMaterial =
                new Material(
                    tmpText.fontSharedMaterial
                );


            runtimeMaterial.name =
                tmpText.fontSharedMaterial.name
                +
                " - Advanced Neon Runtime";


            runtimeMaterial.hideFlags =
                HideFlags.DontSave;
        }


        tmpText.fontMaterial =
            runtimeMaterial;


        ApplyProperties();


        UpdateTextBounds();
    }


    // ============================================================
    // UPDATE
    // ============================================================

    private void Update()
    {
        if (tmpText == null)
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


        ApplyProperties();


        if (
            updateBoundsEveryFrame
        )
        {
            UpdateTextBounds();
        }
    }


    // ============================================================
    // APPLY SHADER PROPERTIES
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
        // GRADIENT
        // --------------------------------------------------------

        runtimeMaterial.SetColor(
            GradientColorA,

            colorA
        );


        runtimeMaterial.SetColor(
            GradientColorB,

            colorB
        );


        runtimeMaterial.SetColor(
            GradientColorC,

            colorC
        );


        runtimeMaterial.SetFloat(
            GradientDirection,

            (float)gradientMode
        );


        runtimeMaterial.SetFloat(
            GradientSpeed,

            gradientSpeed
        );


        runtimeMaterial.SetFloat(
            GradientOffset,

            gradientOffset
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


        runtimeMaterial.SetFloat(
            OutlineSoftness,

            outlineSoftness
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
    // UPDATE WHOLE TEXT BOUNDS
    // ============================================================

    private void UpdateTextBounds()
    {
        if (
            tmpText == null
            ||
            runtimeMaterial == null
        )
        {
            return;
        }


        // --------------------------------------------------------
        // Force TMP to update its geometry.
        // --------------------------------------------------------

        tmpText.ForceMeshUpdate(
            false,
            false
        );


        TMP_TextInfo textInfo =
            tmpText.textInfo;


        if (
            textInfo == null
            ||
            textInfo.characterCount == 0
        )
        {
            return;
        }


        // --------------------------------------------------------
        // Use TMP's generated mesh information.
        //
        // This gives us the bounds of the COMPLETE TEXT.
        //
        // We do NOT calculate bounds per letter.
        // --------------------------------------------------------

      // TMP_TextInfo textInfo = tmpText.textInfo;

Vector3 min = new Vector3(float.MaxValue, float.MaxValue, 0);
Vector3 max = new Vector3(float.MinValue, float.MinValue, 0);

for (int i = 0; i < textInfo.characterCount; i++)
{
    if (!textInfo.characterInfo[i].isVisible)
        continue;

    int mat = textInfo.characterInfo[i].materialReferenceIndex;
    int v = textInfo.characterInfo[i].vertexIndex;

    Vector3[] verts = textInfo.meshInfo[mat].vertices;

    for (int j = 0; j < 4; j++)
    {
        Vector3 p = verts[v + j];

        min = Vector3.Min(min, p);
        max = Vector3.Max(max, p);
    }
}


        // --------------------------------------------------------
        // Send bounds to shader.
        // --------------------------------------------------------

        runtimeMaterial.SetVector(
            TextMin,

            new Vector4(
                min.x,
                min.y,
                min.z,
                0
            )
        );


        runtimeMaterial.SetVector(
            TextMax,

            new Vector4(
                max.x,
                max.y,
                max.z,
                0
            )
        );
    }


    // ============================================================
    // PUBLIC RUNTIME FUNCTIONS
    // ============================================================

    public void SetGradientColors(
        Color a,
        Color b,
        Color c
    )
    {
        colorA = a;

        colorB = b;

        colorC = c;

        ApplyProperties();
    }


    public void SetGradientSpeed(
        float speed
    )
    {
        gradientSpeed =
            speed;

        ApplyProperties();
    }


    public void SetGradientDirection(
        GradientMode mode
    )
    {
        gradientMode =
            mode;

        ApplyProperties();
    }


    public void SetGradientOffset(
        float offset
    )
    {
        gradientOffset =
            offset;

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
        float size
    )
    {
        glowColor =
            color;

        glowStrength =
            strength;

        glowSize =
            size;

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