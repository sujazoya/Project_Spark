#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace AAAUI.Editor
{
    [CustomEditor(typeof(UIAnimationProfile))]
    internal sealed class UIAnimationProfileEditor : UnityEditor.Editor
    {
        private SerializedProperty open;
        private SerializedProperty close;
        private SerializedProperty loop;

        private SerializedProperty defaultDuration;
        private SerializedProperty defaultEase;

        private int sequenceIndex;

        private void OnEnable()
        {
            open = serializedObject.FindProperty("openSequence");
            close = serializedObject.FindProperty("closeSequence");
            loop = serializedObject.FindProperty("loopSequence");

            defaultDuration =
                serializedObject.FindProperty("defaultDuration");

            defaultEase =
                serializedObject.FindProperty("defaultEase");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawSharedDefaults();

            EditorGUILayout.Space(8f);

            string[] names =
            {
                "Open",
                "Close",
                "Loop"
            };

            sequenceIndex =
                GUILayout.Toolbar(sequenceIndex, names);

            EditorGUILayout.Space(6f);

            SerializedProperty selected =
                GetSelectedSequenceProperty();

            if (selected == null)
            {
                EditorGUILayout.HelpBox(
                    "Sequence property could not be found.",
                    MessageType.Error);

                serializedObject.ApplyModifiedProperties();
                return;
            }

            /*
             * IMPORTANT:
             *
             * These are SerializeReference fields.
             *
             * DO NOT USE:
             *
             * selected.objectReferenceValue
             *
             * Use:
             *
             * selected.managedReferenceValue
             */

            if (sequenceIndex == 2 &&
                selected.managedReferenceValue == null)
            {
                DrawLoopCreationUI(selected);
            }
            else
            {
                UIAnimationSequenceDrawer.Draw(selected);

                UIAnimationSequence sequence =
                    selected.managedReferenceValue
                    as UIAnimationSequence;

                if (sequence != null)
                {
                    EditorGUILayout.Space(8f);

                    UIAnimationTimelineGUI.Draw(
                        sequence  );
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawSharedDefaults()
        {
            EditorGUILayout.LabelField(
                "Shared Defaults",
                EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(
                defaultDuration);

            EditorGUILayout.PropertyField(
                defaultEase);
        }

        private SerializedProperty GetSelectedSequenceProperty()
        {
            switch (sequenceIndex)
            {
                case 0:
                    return open;

                case 1:
                    return close;

                case 2:
                    return loop;

                default:
                    return open;
            }
        }

        private void DrawLoopCreationUI(
            SerializedProperty selected)
        {
            EditorGUILayout.HelpBox(
                "No Loop Sequence assigned.",
                MessageType.Info);

            EditorGUILayout.Space(4f);

            if (GUILayout.Button("Create Loop Sequence"))
            {
                selected.managedReferenceValue =
                    new UIAnimationSequence();

                serializedObject.ApplyModifiedProperties();

                GUIUtility.ExitGUI();
            }
        }
    }
}

#endif