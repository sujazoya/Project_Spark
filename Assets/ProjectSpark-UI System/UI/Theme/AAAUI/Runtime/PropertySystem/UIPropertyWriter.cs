using UnityEngine;

namespace AAAUI
{
    internal static class UIPropertyWriter
    {
        public static bool SetFloat(
            PlaybackContext context,
            int targetIndex,
            int propertyId,
            float value)
        {
            MaterialState state =
                GetState(
                    context,
                    targetIndex
                );

            if (state == null ||
                propertyId < 0)
            {
                return false;
            }

            if (state.Renderer != null)
            {
                state.SetFloat(
                    propertyId,
                    value
                );

                return true;
            }

            Material material =
                state.GetWritableMaterial();

            if (material == null ||
                !material.HasProperty(propertyId))
            {
                return false;
            }

            material.SetFloat(
                propertyId,
                value
            );

            return true;
        }

        public static bool SetColor(
            PlaybackContext context,
            int targetIndex,
            int propertyId,
            Color value)
        {
            MaterialState state =
                GetState(
                    context,
                    targetIndex
                );

            if (state == null ||
                propertyId < 0)
            {
                return false;
            }

            if (state.Renderer != null)
            {
                state.SetColor(
                    propertyId,
                    value
                );

                return true;
            }

            Material material =
                state.GetWritableMaterial();

            if (material == null ||
                !material.HasProperty(propertyId))
            {
                return false;
            }

            material.SetColor(
                propertyId,
                value
            );

            return true;
        }

        public static bool SetVector(
            PlaybackContext context,
            int targetIndex,
            int propertyId,
            Vector4 value)
        {
            MaterialState state =
                GetState(
                    context,
                    targetIndex
                );

            if (state == null ||
                propertyId < 0)
            {
                return false;
            }

            Material material =
                state.GetWritableMaterial();

            if (material == null ||
                !material.HasProperty(propertyId))
            {
                return false;
            }

            material.SetVector(
                propertyId,
                value
            );

            return true;
        }

        public static bool SetTexture(
            PlaybackContext context,
            int targetIndex,
            int propertyId,
            Texture value)
        {
            MaterialState state =
                GetState(
                    context,
                    targetIndex
                );

            if (state == null ||
                propertyId < 0)
            {
                return false;
            }

            Material material =
                state.GetWritableMaterial();

            if (material == null ||
                !material.HasProperty(propertyId))
            {
                return false;
            }

            material.SetTexture(
                propertyId,
                value
            );

            return true;
        }

        private static MaterialState GetState(
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

            return context.Materials[targetIndex];
        }
    }
}