using UnityEngine;

namespace AAAUI.Runtime.Rendering
{
    public sealed class MaterialPropertyWriter
    {
        private readonly MaterialPropertyBlock _block;

        public MaterialPropertyWriter()
        {
            _block = new MaterialPropertyBlock();
        }

        public void Write(
            Renderer renderer,
            in UIShaderProperties properties)
        {
            if (renderer == null)
                return;

            renderer.GetPropertyBlock(_block);

            _block.SetFloat(
                MaterialPropertyIds.GlitchIntensity,
                properties.glitchIntensity);

            _block.SetFloat(
                MaterialPropertyIds.GlitchAmount,
                properties.glitchAmount);

            _block.SetFloat(
                MaterialPropertyIds.GlitchSpeed,
                properties.glitchSpeed);

            _block.SetFloat(
                MaterialPropertyIds.DissolveAmount,
                properties.dissolveAmount);

            _block.SetFloat(
                MaterialPropertyIds.DissolveEdge,
                properties.dissolveEdge);

            _block.SetFloat(
                MaterialPropertyIds.Glow,
                properties.glow);

            _block.SetFloat(
                MaterialPropertyIds.GlowStrength,
                properties.glowStrength);

            _block.SetColor(
                MaterialPropertyIds.BaseColor,
                properties.baseColor);

            _block.SetColor(
                MaterialPropertyIds.TestColor,
                properties.testColor);

            _block.SetColor(
                MaterialPropertyIds.GlowColor,
                properties.glowColor);

            _block.SetColor(
                MaterialPropertyIds.DissolveEdgeColor,
                properties.dissolveEdgeColor);

            renderer.SetPropertyBlock(_block);
        }
    }
}