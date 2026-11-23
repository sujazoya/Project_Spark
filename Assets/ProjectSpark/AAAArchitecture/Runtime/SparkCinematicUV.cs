using UnityEngine;
using UnityEngine.UI;
namespace ProjectSpark.AAAArchitecture
{
    // Rect-local UV independent of nine-slice sprite UVs and sprite atlas coordinates.
    [DisallowMultipleComponent]
    [AddComponentMenu("")]
    public sealed class SparkCinematicUV : BaseMeshEffect
    {
        protected override void OnEnable() { base.OnEnable(); ConfigureCanvas(); }
        protected override void OnCanvasHierarchyChanged() { base.OnCanvasHierarchyChanged(); ConfigureCanvas(); }
        private void ConfigureCanvas()
        {
            if (!graphic || !graphic.canvas) return;
            Canvas local = graphic.canvas;
            if ((local.additionalShaderChannels & AdditionalCanvasShaderChannels.TexCoord1) == 0)
                local.additionalShaderChannels |= AdditionalCanvasShaderChannels.TexCoord1;
            Canvas root = local.rootCanvas;
            if (Application.isPlaying && root && !root.GetComponent<SparkCinematicClock>())
                root.gameObject.AddComponent<SparkCinematicClock>();
        }
        public override void ModifyMesh(VertexHelper vh)
        {
            if (!IsActive() || vh.currentVertCount == 0) return;
            Rect r=graphic.rectTransform.rect;
            float invW=1f/Mathf.Max(r.width,.0001f), invH=1f/Mathf.Max(r.height,.0001f);
            UIVertex v=default;
            for(int j=0;j<vh.currentVertCount;j++)
            {
                vh.PopulateUIVertex(ref v,j);
                v.uv1=new Vector4((v.position.x-r.xMin)*invW,(v.position.y-r.yMin)*invH,0,0);
                vh.SetUIVertex(v,j);
            }
        }
    }
}
