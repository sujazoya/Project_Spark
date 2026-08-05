using UnityEngine;
using TMPro;

[ExecuteAlways]
[RequireComponent(typeof(TMP_Text))]
public class TMP_Neon_Controller : MonoBehaviour
{
    [Header("Text")]
    public TMP_Text text;

    [Header("Face")]
    public Color faceColor = Color.white;

    [Header("Outline")]
    [Range(0f, 1f)]
    public float outlineWidth = 0.2f;

    public Color outlineColor = Color.cyan;

    [Header("Glow")]
    public Color glowColor = Color.cyan;

    [Range(0f, 1f)]
    public float glowWidth = 0.3f;

    [Header("Glow Animation")]
    public bool animateGlow = true;

    [Range(0f, 10f)]
    public float glowSpeed = 2f;

    [Range(0f, 10f)]
    public float glowMin = 1f;

    [Range(0f, 20f)]
    public float glowMax = 5f;

    private Material materialInstance;

    private void OnEnable()
    {
        Setup();
    }

    private void Start()
    {
        Setup();
    }

    private void Update()
    {
        if (materialInstance == null)
            Setup();

        if (materialInstance == null)
            return;

        ApplySettings();
    }

    private void Setup()
    {
        if (text == null)
            text = GetComponent<TMP_Text>();

        if (text == null)
            return;

        // Create a unique material
        materialInstance = new Material(text.fontMaterial);

        materialInstance.name =
            text.fontMaterial.name + " - Runtime";

        text.fontMaterial = materialInstance;
    }

    private void ApplySettings()
    {
        // Face Color
        if (materialInstance.HasProperty("_FaceColor"))
        {
            materialInstance.SetColor(
                "_FaceColor",
                faceColor
            );
        }

        // Outline Color
        if (materialInstance.HasProperty("_OutlineColor"))
        {
            materialInstance.SetColor(
                "_OutlineColor",
                outlineColor
            );
        }

        // Outline Width
        if (materialInstance.HasProperty("_OutlineWidth"))
        {
            materialInstance.SetFloat(
                "_OutlineWidth",
                outlineWidth
            );
        }

        // Glow / Underlay Color
        if (materialInstance.HasProperty("_UnderlayColor"))
        {
            materialInstance.SetColor(
                "_UnderlayColor",
                glowColor
            );
        }

        // Glow Width
        if (materialInstance.HasProperty("_UnderlayDilate"))
        {
            materialInstance.SetFloat(
                "_UnderlayDilate",
                glowWidth
            );
        }

        // Animated Glow
        if (animateGlow &&
            materialInstance.HasProperty("_UnderlayColor"))
        {
            float pulse =
                Mathf.Lerp(
                    glowMin,
                    glowMax,
                    (Mathf.Sin(Time.time * glowSpeed) + 1f) * 0.5f
                );

            Color animatedGlow =
                glowColor * pulse;

            materialInstance.SetColor(
                "_UnderlayColor",
                animatedGlow
            );
        }
    }
}