#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace AAAUI.Editor
{
    [CustomPropertyDrawer(typeof(UIPropertyBinding))]
    internal sealed class UIPropertyBindingDrawer
        : PropertyDrawer
    {
        public override void OnGUI(
            Rect position,
            SerializedProperty property,
            GUIContent label)
        {
            SerializedProperty reference =
                property.FindPropertyRelative("reference");

            if (reference == null)
            {
                EditorGUI.LabelField(
                    position,
                    "Property Binding is invalid."
                );

                return;
            }

            EditorGUI.PropertyField(
                position,
                reference,
                label,
                true
            );
        }

        public override float GetPropertyHeight(
            SerializedProperty property,
            GUIContent label)
        {
            SerializedProperty reference =
                property.FindPropertyRelative("reference");

            if (reference == null)
                return EditorGUIUtility.singleLineHeight;

            return EditorGUI.GetPropertyHeight(
                reference,
                label,
                true
            );
        }
    }
}

#endif