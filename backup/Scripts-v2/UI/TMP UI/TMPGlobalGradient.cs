using TMPro;
using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(TextMeshProUGUI))]
public class TMPGlobalGradient : MonoBehaviour
{
    TMP_Text text;

    void Awake() => text = GetComponent<TMP_Text>();

    void LateUpdate()
    {
        text.ForceMeshUpdate();

        var info = text.textInfo;

        Bounds bounds = text.textBounds;

        float width = Mathf.Max(0.0001f, bounds.size.x);
        float height = Mathf.Max(0.0001f, bounds.size.y);

        for (int m = 0; m < info.meshInfo.Length; m++)
        {
            var uv2 = info.meshInfo[m].uvs2;
            var verts = info.meshInfo[m].vertices;

            for (int i = 0; i < verts.Length; i++)
            {
                uv2[i] = new Vector2(
                    (verts[i].x - bounds.min.x) / width,
                    (verts[i].y - bounds.min.y) / height
                );
            }

            info.meshInfo[m].mesh.uv2 = uv2;
        }

        text.UpdateVertexData(TMP_VertexDataUpdateFlags.Uv2);
    }
}