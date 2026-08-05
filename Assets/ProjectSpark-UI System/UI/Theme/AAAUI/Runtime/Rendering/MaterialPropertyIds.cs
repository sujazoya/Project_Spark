using UnityEngine;

namespace AAAUI.Runtime.Rendering
{
    public static class MaterialPropertyIds
    {
        public static readonly int GlitchIntensity =
            Shader.PropertyToID("_GlitchIntensity");

        public static readonly int GlitchAmount =
            Shader.PropertyToID("_GlitchAmount");

        public static readonly int GlitchSpeed =
            Shader.PropertyToID("_GlitchSpeed");

        public static readonly int DissolveAmount =
            Shader.PropertyToID("_DissolveAmount");

        public static readonly int DissolveEdge =
            Shader.PropertyToID("_DissolveEdge");

        public static readonly int Glow =
            Shader.PropertyToID("_Glow");

        public static readonly int GlowStrength =
            Shader.PropertyToID("_GlowStrength");

        public static readonly int BaseColor =
            Shader.PropertyToID("_BaseColor");

        public static readonly int TestColor =
            Shader.PropertyToID("_TestColor");

        public static readonly int GlowColor =
            Shader.PropertyToID("_GlowColor");

        public static readonly int DissolveEdgeColor =
            Shader.PropertyToID("_DissolveEdgeColor");
    }
}