#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace AAAUI.Editor
{
    [CustomPropertyDrawer(typeof(UIPropertyReference))]
    internal sealed class UIPropertyReferenceDrawer
        : PropertyDrawer
    {
        public override void OnGUI(
            Rect position,
            SerializedProperty property,
            GUIContent label)
        {
            SerializedProperty propertyName =
                property.FindPropertyRelative("property");

            SerializedProperty propertyType =
                property.FindPropertyRelative("type");

            float line =
                EditorGUIUtility.singleLineHeight;

            Rect propertyRect =
                new Rect(
                    position.x,
                    position.y,
                    position.width,
                    line
                );

            Rect typeRect =
                new Rect(
                    position.x,
                    position.y + line + 2f,
                    position.width,
                    line
                );

            EditorGUI.PropertyField(
                propertyRect,
                propertyName,
                new GUIContent("Property")
            );

            EditorGUI.PropertyField(
                typeRect,
                propertyType,
                new GUIContent("Type")
            );
        }

        public override float GetPropertyHeight(
            SerializedProperty property,
            GUIContent label)
        {
            return
                EditorGUIUtility.singleLineHeight * 2f +
                2f;
        }
    }
}

#endif