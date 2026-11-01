#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using ProjectSpark.AI.Tutorial;
using ProjectSpark.UI.VisualTeaching;

/// <summary>
/// Auto-binds the user's existing SparkAIUnifiedAssistant to the existing
/// Visual Teaching explanation connector. Reuses all existing UI objects.
/// </summary>
public static class SparkAIUnifiedAssistantDirectBuilder
{
    [MenuItem("Tools/Project Spark/AI/Connect Unified Assistant Directly")]
    public static void Connect()
    {
        SparkAIExplanationRequestBridge bridge = null;
        if (Selection.activeGameObject != null)
            bridge = Selection.activeGameObject
                .GetComponentInParent<SparkAIExplanationRequestBridge>();

        if (bridge == null)
            bridge = Object.FindFirstObjectByType<SparkAIExplanationRequestBridge>(
                FindObjectsInactive.Include);

        if (bridge == null)
        {
            Debug.LogError("[Spark AI Direct] Natural Explanation Request Bridge is missing. " +
                "Install the previous Natural Explanation Bridge package first.");
            return;
        }

        SparkAIUnifiedAssistant assistant =
            Object.FindFirstObjectByType<SparkAIUnifiedAssistant>(
                FindObjectsInactive.Include);
        if (assistant == null)
        {
            Debug.LogError("[Spark AI Direct] SparkAIUnifiedAssistant was not found in the scene.");
            return;
        }

        GameObject root = bridge.gameObject;
        SparkAIExplanationAssistantConnector connector =
            root.GetComponent<SparkAIExplanationAssistantConnector>();
        if (connector == null)
            connector = Undo.AddComponent<SparkAIExplanationAssistantConnector>(root);

        SparkAIUnifiedAssistantTransport transport =
            root.GetComponent<SparkAIUnifiedAssistantTransport>();
        if (transport == null)
            transport = Undo.AddComponent<SparkAIUnifiedAssistantTransport>(root);

        SerializedObject transportSO = new SerializedObject(transport);
        transportSO.FindProperty("assistant").objectReferenceValue = assistant;
        transportSO.ApplyModifiedProperties();

        SerializedObject connectorSO = new SerializedObject(connector);
        connectorSO.FindProperty("explanationBridge").objectReferenceValue = bridge;
        connectorSO.FindProperty("assistantTransport").objectReferenceValue = transport;
        connectorSO.ApplyModifiedProperties();

        PrefabUtility.RecordPrefabInstancePropertyModifications(transport);
        PrefabUtility.RecordPrefabInstancePropertyModifications(connector);
        EditorUtility.SetDirty(transport);
        EditorUtility.SetDirty(connector);

        if (bridge.PromptReady.GetPersistentEventCount() > 0)
            Debug.LogWarning("[Spark AI Direct] Prompt Ready has persistent Inspector " +
                "listeners. Remove old direct-send listeners to prevent duplicate requests.", root);

        Selection.activeGameObject = root;
        Debug.Log("[Spark AI Direct] Existing Ask/AnswerReady connected. " +
            "No edits to SparkAIUnifiedAssistant are required.", root);
    }
}
#endif
