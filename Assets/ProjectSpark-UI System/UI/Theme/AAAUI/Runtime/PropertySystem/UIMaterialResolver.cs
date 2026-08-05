using UnityEngine;

namespace AAAUI
{
    internal static class UIMaterialResolver
    {
        public static Material GetMaterial(
            UIAnimationTarget target)
        {
            if (target == null)
                return null;

            if (target.Renderer != null)
                return target.Renderer.sharedMaterial;

            if (target.Graphic != null)
                return target.Graphic.material;

            return null;
        }

        public static Material GetWritableMaterial(
            PlaybackContext context,
            int targetIndex)
        {
            if (context == null)
                return null;

            if ((uint)targetIndex >=
                (uint)context.Materials.Length)
            {
                return null;
            }

            return context.Materials[targetIndex]
                .GetWritableMaterial();
        }

        public static bool HasProperty(
            PlaybackContext context,
            int targetIndex,
            int propertyId)
        {
            if (propertyId < 0)
                return false;

            Material material =
                GetWritableMaterial(
                    context,
                    targetIndex
                );

            return material != null &&
                   material.HasProperty(propertyId);
        }
    }
}