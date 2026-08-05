using UnityEngine;

namespace AAAUI.Runtime.Rendering
{
    [System.Serializable]
    public struct UIShaderProperties
    {
        public float glitchIntensity;
        public float glitchAmount;
        public float glitchSpeed;

        public float dissolveAmount;
        public float dissolveEdge;

        public float glow;
        public float glowStrength;

        public Color baseColor;
        public Color testColor;
        public Color glowColor;
        public Color dissolveEdgeColor;
    }
}