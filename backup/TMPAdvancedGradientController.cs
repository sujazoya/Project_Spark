using UnityEngine;
using TMPro;

[ExecuteAlways]
public class TMPAdvancedGradientController : MonoBehaviour
{
    [Header("TextMeshPro")]
    [SerializeField] private TMP_Text text;

    [Header("Gradient Colors")]
    [ColorUsage(true, true)]
    [SerializeField] private Color gradientColorA = Color.cyan;

    [ColorUsage(true, true)]
    [SerializeField] private Color gradientColorB = Color.blue;

    [ColorUsage(true, true)]
    [SerializeField] private Color gradientColorC = Color.magenta;

    [Header("Gradient Animation")]
    [SerializeField] private bool animateGradient = true;

    [SerializeField]
    private float gradientSpeed = 1f;

    [Range(0f, 1f)]
    [SerializeField]
    private float gradientOffset = 0f;

    [Header("Outline")]
    [Range(0f, 1f)]
    [SerializeField]
    private float outlineWidth = 0.1f;

    [Header("Emission")]
    [ColorUsage(true, true)]
    [SerializeField]
    private Color emissionColor = Color.cyan;

    [Min(0f)]
    [SerializeField]
    private float emissionIntensity = 3f;

    private Material runtimeMaterial;

    // Shader property IDs
    private static readonly int GradientA =
        Shader.PropertyToID("_GradientColorA");

    private static readonly int GradientB =
        Shader.PropertyToID("_GradientColorB");

    private static readonly int GradientC =
        Shader.PropertyToID("_GradientColorC");

    private static readonly int GradientSpeed =
        Shader.PropertyToID("_GradientSpeed");

    private static readonly int GradientOffset =
        Shader.PropertyToID("_GradientOffset");

    private static readonly int OutlineWidth =
        Shader.PropertyToID("_OutlineWidth");

    private static readonly int EmissionColor =
        Shader.PropertyToID("_EmissionColor");

    private static readonly int EmissionIntensity =
        Shader.PropertyToID("_EmissionIntensity");


    private void Awake()
    {
        SetupMaterial();
    }

    private void OnEnable()
    {
        SetupMaterial();
    }

    private void Update()
    {
        if (runtimeMaterial == null)
            SetupMaterial();

        if (runtimeMaterial == null)
            return;

        UpdateShaderProperties();
    }


    private void SetupMaterial()
    {
        if (text == null)
            text = GetComponent<TMP_Text>();

        if (text == null)
            return;

        // Creates a unique material instance.
        // This prevents changing every TMP object using the same material.
        runtimeMaterial = new Material(text.fontMaterial);

        runtimeMaterial.name =
            text.fontMaterial.name + " (Runtime)";

        text.fontMaterial = runtimeMaterial;
    }


    private void UpdateShaderProperties()
    {
        // Gradient Colors
        runtimeMaterial.SetColor(
            GradientA,
            gradientColorA
        );

        runtimeMaterial.SetColor(
            GradientB,
            gradientColorB
        );

        runtimeMaterial.SetColor(
            GradientC,
            gradientColorC
        );


        // Gradient Animation
        runtimeMaterial.SetFloat(
            GradientSpeed,
            animateGradient ? gradientSpeed : 0f
        );

        runtimeMaterial.SetFloat(
            GradientOffset,
            gradientOffset
        );


        // Outline
        runtimeMaterial.SetFloat(
            OutlineWidth,
            outlineWidth
        );


        // Emission
        runtimeMaterial.SetColor(
            EmissionColor,
            emissionColor
        );

        runtimeMaterial.SetFloat(
            EmissionIntensity,
            emissionIntensity
        );
    }


    // -----------------------------
    // PUBLIC RUNTIME FUNCTIONS
    // -----------------------------

    public void SetGradientColors(
        Color colorA,
        Color colorB,
        Color colorC
    )
    {
        gradientColorA = colorA;
        gradientColorB = colorB;
        gradientColorC = colorC;

        UpdateShaderProperties();
    }


    public void SetOutlineWidth(float width)
    {
        outlineWidth = Mathf.Clamp01(width);

        if (runtimeMaterial != null)
        {
            runtimeMaterial.SetFloat(
                OutlineWidth,
                outlineWidth
            );
        }
    }


    public void SetEmission(
        Color color,
        float intensity
    )
    {
        emissionColor = color;
        emissionIntensity = Mathf.Max(0f, intensity);

        if (runtimeMaterial != null)
        {
            runtimeMaterial.SetColor(
                EmissionColor,
                emissionColor
            );

            runtimeMaterial.SetFloat(
                EmissionIntensity,
                emissionIntensity
            );
        }
    }


    public void SetGradientSpeed(float speed)
    {
        gradientSpeed = speed;

        if (runtimeMaterial != null)
        {
            runtimeMaterial.SetFloat(
                GradientSpeed,
                gradientSpeed
            );
        }
    }


    public void SetGradientOffset(float offset)
    {
        gradientOffset = offset;

        if (runtimeMaterial != null)
        {
            runtimeMaterial.SetFloat(
                GradientOffset,
                gradientOffset
            );
        }
    }


    public void ToggleGradientAnimation(bool enabled)
    {
        animateGradient = enabled;

        if (runtimeMaterial != null)
        {
            runtimeMaterial.SetFloat(
                GradientSpeed,
                enabled ? gradientSpeed : 0f
            );
        }
    }


    // -----------------------------
    // PRESET EXAMPLES
    // -----------------------------

    public void CyberBlue()
    {
        SetGradientColors(
            Color.cyan,
            Color.blue,
            new Color(0.1f, 0.5f, 1f)
        );

        SetOutlineWidth(0.08f);

        SetEmission(
            Color.cyan,
            4f
        );

        SetGradientSpeed(1.5f);
    }


    public void CyberPurple()
    {
        SetGradientColors(
            Color.magenta,
            new Color(0.3f, 0f, 1f),
            Color.cyan
        );

        SetOutlineWidth(0.1f);

        SetEmission(
            Color.magenta,
            5f
        );

        SetGradientSpeed(1f);
    }


    public void WarningRed()
    {
        SetGradientColors(
            Color.red,
            new Color(1f, 0.3f, 0f),
            Color.yellow
        );

        SetOutlineWidth(0.12f);

        SetEmission(
            Color.red,
            6f
        );

        SetGradientSpeed(2f);
    }
}