#if UNITY_EDITOR
using ProjectSpark.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ProjectSpark.UI.Editor
{
    /// <summary>
    /// Updates motion settings on existing panels without rebuilding any UI.
    /// Safe to run more than once. Does not change UI references or action bindings.
    /// </summary>
    public static class SparkAILearningPanelMotionPreset
    {
        [MenuItem("Tools/Project Spark/AI/Apply Smooth AI Drawer Motion Preset")]
        public static void Apply()
        {
            SparkAILearningPanel[] panels = Object.FindObjectsByType<SparkAILearningPanel>(
                FindObjectsInactive.Include, FindObjectsSortMode.None);
            if (panels == null || panels.Length == 0)
            {
                Debug.LogWarning("[AI Learning Panel] No AILearningPanel found in the open scene.");
                return;
            }

            int updated = 0;
            foreach (SparkAILearningPanel panel in panels)
            {
                if (panel == null) continue;
                Undo.RecordObject(panel, "Apply Smooth AI Drawer Motion Preset");
                SerializedObject so = new SerializedObject(panel);
                SetBool(so, "animateTransitions", true);
                SetFloat(so, "animationSeconds", 0.34f);
                SetFloat(so, "slideDistanceMultiplier", 1.0f);
                SetFloat(so, "enterDelayFraction", 0.035f);
                SetBool(so, "subtleScaleAnimation", true);
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(panel);
                if (panel.gameObject.scene.IsValid())
                    EditorSceneManager.MarkSceneDirty(panel.gameObject.scene);
                updated++;
            }
            Debug.Log("[AI Learning Panel] Applied smooth motion preset to " + updated +
                      " existing panel(s). No UI objects were rebuilt.");
        }

        private static void SetBool(SerializedObject so, string name, bool value)
        {
            SerializedProperty property = so.FindProperty(name);
            if (property != null) property.boolValue = value;
        }

        private static void SetFloat(SerializedObject so, string name, float value)
        {
            SerializedProperty property = so.FindProperty(name);
            if (property != null) property.floatValue = value;
        }
    }
}
#endif
