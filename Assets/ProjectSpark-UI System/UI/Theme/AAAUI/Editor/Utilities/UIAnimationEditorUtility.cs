#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace AAAUI.Editor
{
    internal static class UIAnimationEditorUtility
    {
        public static void Record(UnityEngine.Object target, string label)
        {
            if (target != null) Undo.RecordObject(target, label);
        }

        public static void Dirty(UnityEngine.Object target)
        {
            if (target != null) EditorUtility.SetDirty(target);
        }

        public static string TargetName(SerializedProperty targets, int index)
        {
            if (targets == null || !targets.isArray || index < 0 || index >= targets.arraySize)
                return "Slot " + index;

            SerializedProperty element = targets.GetArrayElementAtIndex(index);
            SerializedProperty transform = element.FindPropertyRelative("transform");
            return transform != null && transform.objectReferenceValue != null
                ? transform.objectReferenceValue.name
                : "Slot " + index;
        }
    }
}
#endif