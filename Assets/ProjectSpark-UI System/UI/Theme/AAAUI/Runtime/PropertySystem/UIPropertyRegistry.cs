using System.Collections.Generic;
using UnityEngine;

namespace AAAUI
{
    internal static class UIPropertyRegistry
    {
        private static readonly Dictionary<string, int> ids =
            new Dictionary<string, int>();

        public static int GetId(string property)
        {
            if (string.IsNullOrEmpty(property))
                return -1;

            if (ids.TryGetValue(property, out int id))
                return id;

            id = Shader.PropertyToID(property);
            ids.Add(property, id);

            return id;
        }

        public static bool IsValid(
            Material material,
            string property)
        {
            if (material == null ||
                string.IsNullOrEmpty(property))
            {
                return false;
            }

            int id = GetId(property);

            return material.HasProperty(id);
        }

        public static void Clear()
        {
            ids.Clear();
        }
    }
}