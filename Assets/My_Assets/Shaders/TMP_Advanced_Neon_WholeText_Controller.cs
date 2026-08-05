using UnityEngine;
using TMPro;

[ExecuteAlways]
[RequireComponent(typeof(TMP_Text))]
public class TMP_Advanced_Neon_WholeText_Controller : MonoBehaviour
{
    // ============================================================
    // COMPONENT
    // ============================================================

    private TMP_Text tmpText;

    private Material runtimeMaterial;


    // ============================================================
    // SHADER PROPERTY IDs
    // ============================================================

    private static readonly int MainTexProperty =
        Shader.PropertyToID("_MainTex");

    private static readonly int FaceColorProperty =
        Shader.PropertyToID("_FaceColor");

    private static readonly int GradientColorAProperty =
        Shader.PropertyToID("_GradientColorA");

    private static readonly int GradientColorBProperty =
        Shader.PropertyToID("_GradientColorB");

    private static readonly int GradientColorCProperty =
        Shader.PropertyToID("_GradientColorC");

    private static readonly int GradientDirectionProperty =
        Shader.PropertyToID("_GradientDirection");

    private static readonly int GradientSpeedProperty =
        Shader.PropertyToID("_GradientSpeed");

    private static readonly int GradientOffsetProperty =
        Shader.PropertyToID("_GradientOffset");

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

    private static readonly int TextMinProperty =
        Shader.PropertyToID("_TextMin");

    private static readonly int TextMaxProperty =
        Shader.PropertyToID("_TextMax");


    // ============================================================
    // GRADIENT
    // ============================================================

    public enum GradientDirection
    {
        Horizontal = 0,

        Vertical = 1,

        Diagonal = 2
    }


    [Header("Gradient Direction")]

    public GradientDirection direction =
        GradientDirection.Horizontal;


    [Header("Gradient Colors")]

    [ColorUsage(true, true)]
    public Color gradientColorA =
        Color.cyan;


    [ColorUsage(true, true)]
    public Color gradientColorB =
        Color.blue;


    [ColorUsage(true, true)]
    public Color gradientColorC =
        Color.magenta;


    [Header("Animation")]

    public bool animate =
        true;


    public float animationSpeed =
        0.25f;


    [Range(0f, 1f)]
    public float gradientOffset =
        0f;


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


    [Range(0f, 1f)]
    public float outlineWidth =
        0.15f;


    // ============================================================
    // EMISSION
    // ============================================================

    [Header("Emission")]

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
        if (tmpText == null)
        {
            tmpText =
                GetComponent<TMP_Text>();
        }


        if (tmpText == null)
        {
            return;
        }


        Shader shader =
            Shader.Find(
                "Custom/TMP Advanced Neon Whole Text URP"
            );


        if (shader == null)
        {
            Debug.LogError(
                "Could not find shader: " +
                "Custom/TMP Advanced Neon Whole Text URP"
            );

            return;
        }


        // ========================================================
        // CREATE UNIQUE MATERIAL
        // ========================================================

        if (
            runtimeMaterial == null
            ||
            runtimeMaterial.shader != shader
        )
        {
            runtimeMaterial =
                new Material(
                    shader
                );


            runtimeMaterial.name =
                tmpText.name +
                " - Advanced Neon TMP Material";


            runtimeMaterial.hideFlags =
                HideFlags.DontSave;
        }


        // ========================================================
        // GET TMP FONT ATLAS
        // ========================================================

        if (
            tmpText.font != null
            &&
            tmpText.font.atlasTexture != null
        )
        {
            runtimeMaterial.SetTexture(
                MainTexProperty,

                tmpText.font.atlasTexture
            );
        }


        // ========================================================
        // ASSIGN MATERIAL
        // ========================================================

        tmpText.fontMaterial =
            runtimeMaterial;


        // ========================================================
        // UPDATE
        // ========================================================

        UpdateTextBounds();

        ApplyProperties();
    }


    // ============================================================
    // LATE UPDATE
    // ============================================================

    private void LateUpdate()
    {
        if (tmpText == null)
        {
            Initialize();

            return;
        }


        if (runtimeMaterial == null)
        {
            Initialize();

            return;
        }


        // Force TMP to update its geometry first.
        tmpText.ForceMeshUpdate();


        // IMPORTANT:
        //
        // Bounds are calculated from the COMPLETE text mesh.
        //
        // This is the key to preventing the gradient
        // from restarting on every character.

        UpdateTextBounds();


        ApplyProperties();
    }


    // ============================================================
    // CALCULATE COMPLETE TEXT BOUNDS
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


        tmpText.ForceMeshUpdate();


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


        bool found =
            false;


        Vector3 min =
            Vector3.one *
            float.MaxValue;


        Vector3 max =
            Vector3.one *
            float.MinValue;


        // ========================================================
        // LOOP THROUGH ALL CHARACTERS
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
                vertices == null
                ||
                vertexIndex + 3 >=
                vertices.Length
            )
            {
                continue;
            }


            for (
                int v = 0;

                v < 4;

                v++
            )
            {
                Vector3 p =
                    vertices[
                        vertexIndex +
                        v
                    ];


                min.x =
                    Mathf.Min(
                        min.x,

                        p.x
                    );


                min.y =
                    Mathf.Min(
                        min.y,

                        p.y
                    );


                max.x =
                    Mathf.Max(
                        max.x,

                        p.x
                    );


                max.y =
                    Mathf.Max(
                        max.y,

                        p.y
                    );


                found =
                    true;
            }
        }


        if (!found)
        {
            return;
        }


        // ========================================================
        // SEND WHOLE TEXT BOUNDS TO SHADER
        // ========================================================

        runtimeMaterial.SetVector(
            TextMinProperty,

            new Vector4(
                min.x,

                min.y,

                0,

                0
            )
        );


        runtimeMaterial.SetVector(
            TextMaxProperty,

            new Vector4(
                max.x,

                max.y,

                0,

                0
            )
        );
    }


    // ============================================================
    // APPLY MATERIAL PROPERTIES
    // ============================================================

    private void ApplyProperties()
    {
        if (
            runtimeMaterial == null
        )
        {
            return;
        }


        runtimeMaterial.SetColor(
            FaceColorProperty,

            faceColor
        );


        runtimeMaterial.SetColor(
            GradientColorAProperty,

            gradientColorA
        );


        runtimeMaterial.SetColor(
            GradientColorBProperty,

            gradientColorB
        );


        runtimeMaterial.SetColor(
            GradientColorCProperty,

            gradientColorC
        );


        runtimeMaterial.SetFloat(
            GradientDirectionProperty,

            (float)direction
        );


        runtimeMaterial.SetFloat(
            GradientSpeedProperty,

            animate
                ? animationSpeed
                : 0f
        );


        runtimeMaterial.SetFloat(
            GradientOffsetProperty,

            gradientOffset
        );


        runtimeMaterial.SetColor(
            OutlineColorProperty,

            outlineColor
        );


        runtimeMaterial.SetFloat(
            OutlineWidthProperty,

            outlineWidth
        );


        runtimeMaterial.SetColor(
            EmissionColorProperty,

            emissionColor
        );


        runtimeMaterial.SetFloat(
            EmissionIntensityProperty,

            emissionIntensity
        );


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
    // PUBLIC RUNTIME FUNCTIONS
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


    public void SetDirection(
        GradientDirection newDirection
    )
    {
        direction =
            newDirection;

        ApplyProperties();
    }


    public void SetAnimation(
        bool enabled,

        float speed
    )
    {
        animate =
            enabled;

        animationSpeed =
            speed;

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