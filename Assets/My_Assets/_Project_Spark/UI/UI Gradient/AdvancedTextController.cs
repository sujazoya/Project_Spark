using UnityEngine;

public class AdvancedTextController : MonoBehaviour
{
    [SerializeField] Material material;

    public Color face = Color.white;

    public Color top = Color.white;
    public Color bottom = Color.cyan;

    public Color outline = Color.black;

    public Color glow = Color.cyan;

    public Color emission = Color.cyan;

    [Range(0,0.1f)]
    public float outlineSize=0.01f;

    [Range(0,10)]
    public float glowPower=2;

    [Range(0,20)]
    public float emissionPower=2;

    [Range(0,1)]
    public float gradientStrength=1;

    void Update()
    {
        material.SetColor("_FaceColor",face);

        material.SetColor("_GradientTop",top);
        material.SetColor("_GradientBottom",bottom);
        material.SetFloat("_GradientStrength",gradientStrength);

        material.SetColor("_OutlineColor",outline);
        material.SetFloat("_OutlineSize",outlineSize);

        material.SetColor("_GlowColor",glow);
        material.SetFloat("_GlowPower",glowPower);

        material.SetColor("_EmissionColor",emission);
        material.SetFloat("_EmissionIntensity",emissionPower);
    }
}