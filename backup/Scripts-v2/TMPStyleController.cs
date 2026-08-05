using TMPro;
using UnityEngine;

public class TMPStyleController : MonoBehaviour
{
    public TMP_Text text;

    [Header("Colors")]
    public Color faceColor = Color.white;
    public Color outlineColor = Color.cyan;

    [Header("Outline")]
    [Range(0f, 1f)]
    public float outlineWidth = 0.2f;

    [Header("Glow")]
    public Color glowColor = Color.cyan;
    [Range(0f, 1f)]
    public float glowWidth = 0.3f;

    private Material mat;

    void Start()
    {
        // Create unique material
        mat = new Material(text.fontMaterial);
        text.fontMaterial = mat;

        Apply();
    }

    void Update()
    {
        Apply();
    }

    void Apply()
    {
        // Face
        mat.SetColor("_FaceColor", faceColor);

        // Outline
        mat.SetColor("_OutlineColor", outlineColor);
        mat.SetFloat("_OutlineWidth", outlineWidth);

        // Glow / Underlay
        mat.SetColor("_UnderlayColor", glowColor);
        mat.SetFloat("_UnderlayDilate", glowWidth);
    }
}