using UnityEngine;
using UnityEngine.UI;
using TMPro;

[ExecuteAlways]
[RequireComponent(typeof(TMP_Text))]
public class TMP_WholeText_VerticalGradient_Controller : MonoBehaviour
{
    // ============================================================
    // TMP
    // ============================================================

    private TMP_Text tmpText;

    private Material runtimeMaterial;


    // ============================================================
    // SHADER IDS
    // ============================================================

    private static readonly int GradientTopID =
        Shader.PropertyToID("_GradientTop");

    private static readonly int GradientMiddleID =
        Shader.PropertyToID("_GradientMiddle");

    private static readonly int GradientBottomID =
        Shader.PropertyToID("_GradientBottom");

    private static readonly int FaceColorID =
        Shader.PropertyToID("_FaceColor");

    private static readonly int OutlineColorID =
        Shader.PropertyToID("_OutlineColor");

    private static readonly int OutlineWidthID =
        Shader.PropertyToID("_OutlineWidth");

    private static readonly int EmissionColorID =
        Shader.PropertyToID("_EmissionColor");

    private static readonly int EmissionIntensityID =
        Shader.PropertyToID("_EmissionIntensity");

    private static readonly int GlowColorID =
        Shader.PropertyToID("_GlowColor");

    private static readonly int GlowStrengthID =
        Shader.PropertyToID("_GlowStrength");

    private static readonly int GlowSoftnessID =
        Shader.PropertyToID("_GlowSoftness");


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
    // CACHE
    // ============================================================

    private TMP_MeshInfo[] cachedMeshInfo;


    // ============================================================
    // ENABLE
    // ============================================================

    private void OnEnable()
    {
        Initialize();
    }


    private void Awake()
    {
        Initialize();
    }


    // ============================================================
    // INITIALIZE
    // ============================================================

    private void Initialize()
    {
        tmpText =
            GetComponent<TMP_Text>();


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
                "Custom/TMP Whole Text Vertical Gradient URP"
            );


        if (
            shader ==
            null
        )
        {
            Debug.LogError(
                "Cannot find shader: " +
                "Custom/TMP Whole Text Vertical Gradient URP"
            );

            return;
        }


        // ========================================================
        // CREATE MATERIAL
        // ========================================================

        if (
            runtimeMaterial ==
            null
        )
        {
            runtimeMaterial =
                new Material(
                    shader
                );


            runtimeMaterial.name =
                tmpText.name +
                " - Whole Text Vertical Gradient";


            runtimeMaterial.hideFlags =
                HideFlags.DontSave;
        }


        // ========================================================
        // FONT ATLAS
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
                "_MainTex",

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

        UpdateShaderProperties();

        RebuildGradientUV();
    }


    // ============================================================
    // LATE UPDATE
    // ============================================================

    private void LateUpdate()
    {
        if (
            tmpText ==
            null
        )
        {
            return;
        }


        // ========================================================
        // TMP GENERATES ITS MESH
        // ========================================================

        tmpText.ForceMeshUpdate();


        // ========================================================
        // RECALCULATE UV2
        // ========================================================

        RebuildGradientUV();


        // ========================================================
        // UPDATE COLORS
        // ========================================================

        UpdateShaderProperties();
    }


    // ============================================================
    // REBUILD WHOLE TEXT GRADIENT UV
    // ============================================================

    private void RebuildGradientUV()
    {
        if (tmpText == null)
            return;

        tmpText.ForceMeshUpdate();

        TMP_TextInfo textInfo = tmpText.textInfo;

        if (textInfo == null || textInfo.characterCount == 0)
            return;

        // ============================================================
        // 1. FIND WHOLE TEXT LOCAL-SPACE Y BOUNDS
        // ============================================================

        float minY = float.MaxValue;
        float maxY = float.MinValue;

        bool found = false;

        for (int i = 0; i < textInfo.characterCount; i++)
        {
            TMP_CharacterInfo character =
                textInfo.characterInfo[i];

            if (!character.isVisible)
                continue;

            int materialIndex =
                character.materialReferenceIndex;

            int vertexIndex =
                character.vertexIndex;

            if (materialIndex < 0 ||
                materialIndex >= textInfo.meshInfo.Length)
                continue;

            Vector3[] vertices =
                textInfo.meshInfo[materialIndex].vertices;

            if (vertices == null ||
                vertexIndex < 0 ||
                vertexIndex + 3 >= vertices.Length)
                continue;

            for (int v = 0; v < 4; v++)
            {
                float y =
                    vertices[vertexIndex + v].y;

                minY =
                    Mathf.Min(minY, y);

                maxY =
                    Mathf.Max(maxY, y);

                found = true;
            }
        }

        if (!found)
            return;


        // ============================================================
        // 2. PREVENT DIVISION BY ZERO
        // ============================================================

        float height =
            Mathf.Max(
                maxY - minY,
                0.0001f
            );


        // ============================================================
        // 3. WRITE UV2
        // ============================================================

        for (
            int materialIndex = 0;
            materialIndex < textInfo.meshInfo.Length;
            materialIndex++
        )
        {
            TMP_MeshInfo meshInfo =
                textInfo.meshInfo[materialIndex];

            Mesh mesh =
                meshInfo.mesh;

            if (mesh == null)
                continue;


            // ========================================================
            // IMPORTANT FIX
            //
            // UV2 MUST HAVE EXACTLY THE SAME LENGTH
            // AS mesh.vertices
            // ========================================================

            Vector3[] vertices =
                mesh.vertices;

            int vertexCount =
                vertices.Length;

            if (vertexCount == 0)
                continue;


            Vector2[] uv2 = new Vector2[mesh.vertices.Length];


            // ========================================================
            // CALCULATE GRADIENT FOR EVERY VERTEX
            // ========================================================

            for (int v = 0; v < vertexCount; v++)
            {
                float normalizedY = Mathf.Clamp01(
                    (vertices[v].y - minY) /
                    Mathf.Max(height, 0.0001f)
                );

                uv2[v] = new Vector2(
                    normalizedY,
                    0f
                );
            }


            // ========================================================
            // SET COMPLETE UV2 ARRAY
            //
            // NO start/length PARAMETERS
            //
            // UNITY NOW RECEIVES AN ARRAY EXACTLY THE SAME SIZE
            // AS THE VERTEX ARRAY.
            // ========================================================

            mesh.uv2 =
                uv2;
        }
    }


    // ============================================================
    // MATERIAL PROPERTIES
    // ============================================================

    private void UpdateShaderProperties()
    {
        if (
            runtimeMaterial ==
            null
        )
        {
            return;
        }


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


        runtimeMaterial.SetColor(
            FaceColorID,

            faceColor
        );


        runtimeMaterial.SetColor(
            OutlineColorID,

            outlineColor
        );


        runtimeMaterial.SetFloat(
            OutlineWidthID,

            outlineWidth
        );


        runtimeMaterial.SetColor(
            EmissionColorID,

            emissionColor
        );


        runtimeMaterial.SetFloat(
            EmissionIntensityID,

            emissionIntensity
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
    }


    // ============================================================
    // RUNTIME API
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


        UpdateShaderProperties();
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


        UpdateShaderProperties();
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


        UpdateShaderProperties();
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


        UpdateShaderProperties();
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