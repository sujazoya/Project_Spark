using UnityEngine;

namespace AAAUI
{
    internal static class ShaderPropertyCache
    {
        public static int GetId(string propertyName)
        {
            return string.IsNullOrEmpty(propertyName) ? -1 : Shader.PropertyToID(propertyName);
        }
    }
}