#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace ProjectSpark.UI.Animation.Editor
{
    public sealed class UIAnimationWindow :
        EditorWindow
    {
        private UIAnimator selectedAnimator;

        private Vector2 scroll;

        [MenuItem(
            "Project Spark/UI/Animation")]
        public static void Open()
        {
            UIAnimationWindow window =
                GetWindow<UIAnimationWindow>();

            window.titleContent =
                new GUIContent(
                    "PS UI Animation");

            window.minSize =
                new Vector2(
                    360f,
                    500f);
        }

        private void OnGUI()
        {
            DrawHeader();

            EditorGUILayout.Space(8);

            DrawSelection();

            EditorGUILayout.Space(8);

            scroll =
                EditorGUILayout.BeginScrollView(
                    scroll);

            if (selectedAnimator == null)
            {
                EditorGUILayout.HelpBox(
                    "Select a GameObject containing UIAnimator.",
                    MessageType.Info);

                EditorGUILayout.EndScrollView();
                return;
            }

            DrawAnimatorControls();

            EditorGUILayout.EndScrollView();
        }

        private void DrawHeader()
        {
            GUIStyle title =
                new GUIStyle(
                    EditorStyles.boldLabel);

            title.fontSize = 18;

            EditorGUILayout.LabelField(
                "PROJECT SPARK",
                title);

            EditorGUILayout.LabelField(
                "UI ANIMATION TOOLKIT",
                EditorStyles.miniLabel);
        }

        private void DrawSelection()
        {
            EditorGUI.BeginChangeCheck();

            selectedAnimator =
                (UIAnimator)
                EditorGUILayout.ObjectField(
                    "Animator",
                    selectedAnimator,
                    typeof(UIAnimator),
                    true);

            if (EditorGUI.EndChangeCheck())
            {
                Repaint();
            }
        }

        private void DrawAnimatorControls()
        {
            EditorGUILayout.LabelField(
                "PREVIEW",
                EditorStyles.boldLabel);

            EditorGUILayout.Space(4);

            using (
                new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button(
                        "▶ SHOW",
                        GUILayout.Height(30)))
                {
                    selectedAnimator.Show();
                }

                if (GUILayout.Button(
                        "■ HIDE",
                        GUILayout.Height(30)))
                {
                    selectedAnimator.Hide();
                }
            }

            EditorGUILayout.Space(4);

            using (
                new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button(
                        "RESET",
                        GUILayout.Height(26)))
                {
                    selectedAnimator.ShowImmediate();
                }

                if (GUILayout.Button(
                        "KILL",
                        GUILayout.Height(26)))
                {
                    selectedAnimator.Kill();
                }
            }

            EditorGUILayout.Space(12);

            EditorGUILayout.HelpBox(
                "Animation preview uses the selected UIAnimator's current runtime configuration.",
                MessageType.None);
        }
    }
}

#endif