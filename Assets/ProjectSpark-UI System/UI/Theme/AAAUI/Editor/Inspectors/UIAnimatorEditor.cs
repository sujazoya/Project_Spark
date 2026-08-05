#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using AAAUI;

namespace AAAUI.Editor
{
    [CustomEditor(typeof(UIAnimator))]
    internal sealed class UIAnimatorEditor : UnityEditor.Editor
    {
        private SerializedProperty profile;
        private SerializedProperty targets;

        private void OnEnable()
        {
            profile = serializedObject.FindProperty("profile");
            targets = serializedObject.FindProperty("targets");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(profile);

            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("Targets", EditorStyles.boldLabel);

            for (int i = 0; i < targets.arraySize; i++)
            {
                SerializedProperty element = targets.GetArrayElementAtIndex(i);
                SerializedProperty transform = element.FindPropertyRelative("transform");

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(i.ToString(), GUILayout.Width(20f));
                EditorGUILayout.PropertyField(transform, GUIContent.none);

                if (GUILayout.Button("Sync", GUILayout.Width(48f)))
                    SyncElement(element, transform.objectReferenceValue as Transform);

                if (GUILayout.Button("X", GUILayout.Width(22f)))
                {
                    targets.DeleteArrayElementAtIndex(i);
                    break;
                }
                EditorGUILayout.EndHorizontal();
            }

            if (GUILayout.Button("+ Target"))
                targets.InsertArrayElementAtIndex(targets.arraySize);

            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Preview", EditorStyles.boldLabel);

            UIAnimator animator = (UIAnimator)target;
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Open"))
                UIAnimationTimelinePreview.Begin(animator, UIAnimationSequenceType.Open);
            if (GUILayout.Button("Close"))
                UIAnimationTimelinePreview.Begin(animator, UIAnimationSequenceType.Close);
            if (GUILayout.Button("Loop"))
                UIAnimationTimelinePreview.Begin(animator, UIAnimationSequenceType.Loop);
            if (GUILayout.Button("Stop"))
                UIAnimationTimelinePreview.Stop();
            EditorGUILayout.EndHorizontal();

            if (profile.objectReferenceValue == null)
            {
                EditorGUILayout.HelpBox("Assign a UI Animation Profile.", MessageType.Info);
            }
            else
            {
                UIAnimationValidator.DrawProfileValidation(
                    profile.objectReferenceValue as UIAnimationProfile,
                    animator.EditorTargets);
            }
        }

        private static void SyncElement(SerializedProperty element, Transform target)
        {
            SerializedProperty canvasGroup = element.FindPropertyRelative("canvasGroup");
            SerializedProperty graphic = element.FindPropertyRelative("graphic");
            SerializedProperty renderer = element.FindPropertyRelative("renderer");

            canvasGroup.objectReferenceValue = target != null ? target.GetComponent<CanvasGroup>() : null;
            graphic.objectReferenceValue = target != null ? target.GetComponent<UnityEngine.UI.Graphic>() : null;
            renderer.objectReferenceValue = target != null ? target.GetComponent<Renderer>() : null;
        }
    }
}
#endif