using UnityEngine;
using TMPro;

[ExecuteAlways]
[RequireComponent(typeof(TMP_Text))]
public class TMP_WholeText_VerticalNeon_LocalSpace_Controller : MonoBehaviour
{
    // ============================================================
    // TMP
    // ============================================================

    private TMP_Text tmpText;

    private Material runtimeMaterial;


    // ============================================================
    // SHADER PROPERTY IDs
    // ============================================================

    private static readonly int MainTexProperty =
        Shader.PropertyToID("_MainTex");

    private static readonly int GradientTopProperty =
        Shader.PropertyToID("_GradientTop");

    private static readonly int GradientMiddleProperty =
        Shader.PropertyToID("_GradientMiddle");

    private static readonly int GradientBottomProperty =
        Shader.PropertyToID("_GradientBottom");

    private static readonly int FaceColorProperty =
        Shader.PropertyToID("_FaceColor");

    private static readonly int OutlineColorProperty =
        Shader.PropertyToID("_OutlineColor");

    private static readonly int OutlineWidthProperty =
        Shader.PropertyToID("_OutlineWidth");

    private static readonly int EmissionColorProperty =
        Shader.PropertyToID("_EmissionColor");

    private static readonly int EmissionIntensityProperty =
        Shader.PropertyToID("_EmissionIntensity");

    private static readonly int GlowColorProperty =
        Shader.PropertyToID("_GlowColor");

    private static readonly int GlowStrengthProperty =
        Shader.PropertyToID("_GlowStrength");

    private static readonly int GlowSoftnessProperty =
        Shader.PropertyToID("_GlowSoftness");

    private static readonly int TextMinYProperty =
        Shader.PropertyToID("_TextMinY");

    private static readonly int TextMaxYProperty =
        Shader.PropertyToID("_TextMaxY");


    // ============================================================
    // GRADIENT
    // ============================================================

    [Header("WHOLE TEXT VERTICAL GRADIENT")]

    [ColorUsage(true, true)]
    public Color gradientTop =
        Color.cyan;

    [ColorUsage(true, true)]
    public Color gradientMiddle =
        Color.blue;

    [ColorUsage(true, true)]
    public Color gradientBottom =
        Color.magenta;


    // ============================================================
    // FACE
    // ============================================================

    [Header("FACE")]

    [ColorUsage(true, true)]
    public Color faceColor =
        Color.white;


    // ============================================================
    // OUTLINE
    // ============================================================

    [Header("OUTLINE")]

    [ColorUsage(true, true)]
    public Color outlineColor =
        Color.cyan;

    [Range(0f, 1f)]
    public float outlineWidth =
        0.15f;


    // ============================================================
    // EMISSION
    // ============================================================

    [Header("EMISSION")]

    [ColorUsage(true, true)]
    public Color emissionColor =
        Color.cyan;

    [Range(0f, 20f)]
    public float emissionIntensity =
        2f;


    // ============================================================
    // GLOW
    // ============================================================

    [Header("GLOW")]

    [ColorUsage(true, true)]
    public Color glowColor =
        Color.cyan;

    [Range(0f, 10f)]
    public float glowStrength =
        1f;

    [Range(0.001f, 1f)]
    public float glowSoftness =
        0.1f;


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
        if (
            tmpText ==
            null
        )
        {
            tmpText =
                GetComponent<TMP_Text>();
        }


        if (
            tmpText ==
            null
        )
        {
            return;
        }


        // ========================================================
        // FIND SHADER
        // ========================================================

        Shader shader =
            Shader.Find(
                "Custom/TMP Whole Text Vertical Neon Local Space"
            );


        if (
            shader ==
            null
        )
        {
            Debug.LogError(
                "Shader not found: " +
                "Custom/TMP Whole Text Vertical Neon Local Space"
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
                tmpText.name +
                " - Whole Text Vertical Neon";


            runtimeMaterial.hideFlags =
                HideFlags.DontSave;
        }


        // ========================================================
        // GET FONT ATLAS
        // ========================================================

        if (
            tmpText.font !=
            null
            &&
            tmpText.font.atlasTexture !=
            null
        )
        {
            runtimeMaterial.SetTexture(
                MainTexProperty,

                tmpText.font.atlasTexture
            );
        }


        // ========================================================
        // IMPORTANT
        //
        // Use material, not fontMaterial.
        //
        // This creates a material specifically for this TMP
        // object instead of changing the shared font material.
        // ========================================================

        tmpText.material =
            runtimeMaterial;


        // ========================================================
        // UPDATE
        // ========================================================

        UpdateWholeTextBounds();

        ApplyProperties();
    }


    // ============================================================
    // UPDATE
    // ============================================================

    private void LateUpdate()
    {
        if (
            tmpText ==
            null
        )
        {
            Initialize();

            return;
        }


        if (
            runtimeMaterial ==
            null
        )
        {
            Initialize();

            return;
        }


        // ========================================================
        // FORCE TMP MESH UPDATE
        // ========================================================

        tmpText.ForceMeshUpdate();


        // ========================================================
        // CALCULATE LOCAL SPACE BOUNDS
        // ========================================================

        UpdateWholeTextBounds();


        // ========================================================
        // APPLY MATERIAL
        // ========================================================

        ApplyProperties();
    }


    // ============================================================
    // GET WHOLE TEXT LOCAL-SPACE BOUNDS
    // ============================================================

    private void UpdateWholeTextBounds()
    {
        if (
            tmpText ==
            null
            ||
            runtimeMaterial ==
            null
        )
        {
            return;
        }


        // ========================================================
        // FORCE TMP GEOMETRY
        // ========================================================

        tmpText.ForceMeshUpdate();


        TMP_TextInfo textInfo =
            tmpText.textInfo;


        if (
            textInfo ==
            null
            ||
            textInfo.characterCount <=
            0
        )
        {
            return;
        }


        bool found =
            false;


        float minY =
            float.MaxValue;


        float maxY =
            float.MinValue;


        // ========================================================
        // LOOP THROUGH EVERY CHARACTER
        // ========================================================

        for (
            int i = 0;

            i <
            textInfo.characterCount;

            i++
        )
        {
            TMP_CharacterInfo character =
                textInfo.characterInfo[i];


            // ----------------------------------------------------
            // IGNORE SPACES / INVISIBLE CHARACTERS
            // ----------------------------------------------------

            if (
                !character.isVisible
            )
            {
                continue;
            }


            int vertexIndex =
                character.vertexIndex;


            int materialIndex =
                character.materialReferenceIndex;


            if (
                materialIndex <
                0
                ||
                materialIndex >=
                textInfo.meshInfo.Length
            )
            {
                continue;
            }


            Vector3[] vertices =
                textInfo
                .meshInfo[
                    materialIndex
                ]
                .vertices;


            if (
                vertices ==
                null
                ||
                vertexIndex <
                0
                ||
                vertexIndex +
                3 >=
                vertices.Length
            )
            {
                continue;
            }


            // ====================================================
            // FOUR VERTICES OF THIS CHARACTER
            // ====================================================

            for (
                int v = 0;

                v <
                4;

                v++
            )
            {
                Vector3 vertex =
                    vertices[
                        vertexIndex +
                        v
                    ];


                // ------------------------------------------------
                // THESE VERTICES ARE IN TMP LOCAL OBJECT SPACE.
                //
                // Exactly the same coordinate system used by
                // input.positionOS in the shader.
                // ------------------------------------------------

                minY =
                    Mathf.Min(
                        minY,

                        vertex.y
                    );


                maxY =
                    Mathf.Max(
                        maxY,

                        vertex.y
                    );


                found =
                    true;
            }
        }


        if (
            !found
        )
        {
            return;
        }


        // ========================================================
        // SEND LOCAL-SPACE BOUNDS TO SHADER
        // ========================================================

        runtimeMaterial.SetFloat(
            TextMinYProperty,

            minY
        );


        runtimeMaterial.SetFloat(
            TextMaxYProperty,

            maxY
        );
    }


    // ============================================================
    // APPLY MATERIAL PROPERTIES
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


        // ========================================================
        // GRADIENT
        // ========================================================

        runtimeMaterial.SetColor(
            GradientTopProperty,

            gradientTop
        );


        runtimeMaterial.SetColor(
            GradientMiddleProperty,

            gradientMiddle
        );


        runtimeMaterial.SetColor(
            GradientBottomProperty,

            gradientBottom
        );


        // ========================================================
        // FACE
        // ========================================================

        runtimeMaterial.SetColor(
            FaceColorProperty,

            faceColor
        );


        // ========================================================
        // OUTLINE
        // ========================================================

        runtimeMaterial.SetColor(
            OutlineColorProperty,

            outlineColor
        );


        runtimeMaterial.SetFloat(
            OutlineWidthProperty,

            outlineWidth
        );


        // ========================================================
        // EMISSION
        // ========================================================

        runtimeMaterial.SetColor(
            EmissionColorProperty,

            emissionColor
        );


        runtimeMaterial.SetFloat(
            EmissionIntensityProperty,

            emissionIntensity
        );


        // ========================================================
        // GLOW
        // ========================================================

        runtimeMaterial.SetColor(
            GlowColorProperty,

            glowColor
        );


        runtimeMaterial.SetFloat(
            GlowStrengthProperty,

            glowStrength
        );


        runtimeMaterial.SetFloat(
            GlowSoftnessProperty,

            glowSoftness
        );
    }


    // ============================================================
    // RUNTIME CONTROL
    // ============================================================

    public void SetVerticalGradient(
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