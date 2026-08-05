#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace ProjectSpark.EditorTools
{
    public sealed class DeviceBuilderWindow
        : EditorWindow
    {
        private Vector2 scroll;

        [MenuItem("Project Spark/Device Builder")]
        private static void Open()
        {
            GetWindow<DeviceBuilderWindow>(
                "Device Builder");
        }

        private void OnGUI()
        {
            scroll =
                EditorGUILayout.BeginScrollView(scroll);

            GUILayout.Label(
                "Project Spark Device Builder",
                EditorStyles.boldLabel);

            GUILayout.Space(10);

            DrawToolbar();

            DrawInspector();

            EditorGUILayout.EndScrollView();
        }

        private void DrawToolbar()
        {

        }

        private void DrawInspector()
        {

        }
    }
}
#endif