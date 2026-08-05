using UnityEngine;

namespace AAAUI
{
    internal static class UIPropertyAccessor
    {
        public static bool HasProperty(
            MaterialState state,
            int propertyId)
        {
            if (state == null || propertyId < 0)
                return false;

            Material material =
                state.GetWritableMaterial();

            return material != null &&
                   material.HasProperty(propertyId);
        }

        public static bool SetFloat(
            MaterialState state,
            int propertyId,
            float value)
        {
            if (!HasProperty(state, propertyId))
                return false;

            Material material =
                state.GetWritableMaterial();

            material.SetFloat(propertyId, value);

            return true;
        }

        public static bool SetColor(
            MaterialState state,
            int propertyId,
            Color value)
        {
            if (!HasProperty(state, propertyId))
                return false;

            Material material =
                state.GetWritableMaterial();

            material.SetColor(propertyId, value);

            return true;
        }

        public static bool SetVector(
            MaterialState state,
            int propertyId,
            Vector4 value)
        {
            if (!HasProperty(state, propertyId))
                return false;

            Material material =
                state.GetWritableMaterial();

            material.SetVector(propertyId, value);

            return true;
        }

        public static bool SetTexture(
            MaterialState state,
            int propertyId,
            Texture value)
        {
            if (!HasProperty(state, propertyId))
                return false;

            Material material =
                state.GetWritableMaterial();

            material.SetTexture(propertyId, value);

            return true;
        }
    }
}