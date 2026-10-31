#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using ProjectSpark.UI.VisualTeaching;

public static class SparkVisualTeachingLiveBridgeBuilder
{
    [MenuItem("Tools/Project Spark/AI/Connect Live Teaching Bridge")]
    public static void Connect()
    {
        SparkVisualTeachingManager teacher =
            Object.FindFirstObjectByType<SparkVisualTeachingManager>(FindObjectsInactive.Include);
        SparkWorldAnnotationRenderer annotations =
            Object.FindFirstObjectByType<SparkWorldAnnotationRenderer>(FindObjectsInactive.Include);

        if (teacher == null || annotations == null)
        {
            Debug.LogError("[Spark Teaching] Run 'Create Visual Teaching System (AAA)' before connecting the live bridge.");
            return;
        }

        var existing = teacher.GetComponentInChildren<SparkVisualTeachingLiveBridge>(true);
        SparkVisualTeachingLiveBridge bridge = existing;
        if (bridge == null)
            bridge = Undo.AddComponent<SparkVisualTeachingLiveBridge>(teacher.gameObject);

        SerializedObject so = new SerializedObject(bridge);
        so.FindProperty("teacher").objectReferenceValue = teacher;
        so.FindProperty("worldAnnotations").objectReferenceValue = annotations;
        so.ApplyModifiedProperties();

        EditorUtility.SetDirty(bridge);
        PrefabUtility.RecordPrefabInstancePropertyModifications(bridge);
        Selection.activeGameObject = bridge.gameObject;

        Debug.Log("[Spark Teaching] Live bridge connected. Configure level lessons and VERIFIED fault presets in its Inspector.", bridge);
    }
}
#endif
