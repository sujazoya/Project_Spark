#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace AAAUI.Editor
{
    internal static class UIPropertyEditorUtility
    {
        public static void DrawMaterialPropertyStatus(
            UIAnimationTarget target,
            string property)
        {
            if (target == null)
            {
                EditorGUILayout.HelpBox(
                    "Target is null.",
                    MessageType.Error
                );

                return;
            }

            if (!target.IsAssigned)
            {
                EditorGUILayout.HelpBox(
                    "Target is not assigned.",
                    MessageType.Warning
                );

                return;
            }

            Material material =
                UIMaterialResolver.GetMaterial(target);

            if (material == null)
            {
                EditorGUILayout.HelpBox(
                    "Target has no material.",
                    MessageType.Warning
                );

                return;
            }

            if (string.IsNullOrEmpty(property))
            {
                EditorGUILayout.HelpBox(
                    "Shader property is empty.",
                    MessageType.Warning
                );

                return;
            }

            int id =
                UIPropertyRegistry.GetId(property);

            if (!material.HasProperty(id))
            {
                EditorGUILayout.HelpBox(
                    "Property not found:\n" +
                    property +
                    "\n\nMaterial: " +
                    material.name,
                    MessageType.Error
                );

                return;
            }

            EditorGUILayout.HelpBox(
                "Property valid:\n" +
                property,
                MessageType.Info
            );
        }
    }
}

#endif