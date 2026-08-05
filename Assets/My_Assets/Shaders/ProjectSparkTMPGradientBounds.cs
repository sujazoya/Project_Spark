using TMPro;
using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(TMP_Text))]
public sealed class ProjectSparkTMPGradientBounds : MonoBehaviour
{
    [SerializeField] private Material targetMaterial;

    private TMP_Text tmpText;

    private static readonly int GradientMinY =
        Shader.PropertyToID("_GradientMinY");

    private static readonly int GradientMaxY =
        Shader.PropertyToID("_GradientMaxY");

    private void OnEnable()
    {
        tmpText = GetComponent<TMP_Text>();
        UpdateGradientBounds();
    }

    private void OnValidate()
    {
        tmpText = GetComponent<TMP_Text>();
        UpdateGradientBounds();
    }

    private void LateUpdate()
    {
        UpdateGradientBounds();
    }

    private void UpdateGradientBounds()
    {
        if (tmpText == null)
            tmpText = GetComponent<TMP_Text>();

        if (tmpText == null)
            return;

        // Force TMP to generate the latest geometry.
        tmpText.ForceMeshUpdate();

        TMP_TextInfo textInfo = tmpText.textInfo;

        if (textInfo == null || textInfo.characterCount == 0)
            return;

        bool found = false;

        float minY = float.MaxValue;
        float maxY = float.MinValue;

        for (int i = 0; i < textInfo.characterCount; i++)
        {
            TMP_CharacterInfo character = textInfo.characterInfo[i];

            if (!character.isVisible)
                continue;

            int materialIndex = character.materialReferenceIndex;
            int vertexIndex = character.vertexIndex;

            if (materialIndex < 0 ||
                materialIndex >= textInfo.meshInfo.Length)
                continue;

            Vector3[] vertices =
                textInfo.meshInfo[materialIndex].vertices;

            if (vertices == null ||
                vertexIndex + 3 >= vertices.Length)
                continue;

            for (int v = 0; v < 4; v++)
            {
                float y = vertices[vertexIndex + v].y;

                minY = Mathf.Min(minY, y);
                maxY = Mathf.Max(maxY, y);

                found = true;
            }
        }

        if (!found)
            return;

        // Use TMP's actual material instance.
        Material material = targetMaterial != null
            ? targetMaterial
            : tmpText.fontMaterial;

        if (material == null)
            return;

        material.SetFloat(GradientMinY, minY);
        material.SetFloat(GradientMaxY, maxY);
    }
}