#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using ProjectSpark.UI.VisualTeaching;

/// <summary>Attaches connector beside the existing verified explanation bridge.</summary>
public static class SparkAIUnifiedConnectionBuilder
{
    [MenuItem("Tools/Project Spark/AI/Connect Existing AI Assistant")]
    public static void Create()
    {
        SparkAIExplanationRequestBridge explanation = null;
        if (Selection.activeGameObject != null)
            explanation = Selection.activeGameObject.GetComponentInParent<SparkAIExplanationRequestBridge>();
        if (explanation == null)
            explanation = Object.FindFirstObjectByType<SparkAIExplanationRequestBridge>(
                FindObjectsInactive.Include);
        if (explanation == null)
        {
            Debug.LogError("[Spark AI Connection] Install the Natural Language Explanation Bridge first.");
            return;
        }

        GameObject root = explanation.gameObject;
        SparkAIExplanationAssistantConnector connector =
            root.GetComponent<SparkAIExplanationAssistantConnector>();
        if (connector == null)
            connector = Undo.AddComponent<SparkAIExplanationAssistantConnector>(root);

        SparkAIUnityEventExplanationTransport transport =
            root.GetComponent<SparkAIUnityEventExplanationTransport>();
        if (transport == null)
            transport = Undo.AddComponent<SparkAIUnityEventExplanationTransport>(root);

        SerializedObject so = new SerializedObject(connector);
        SerializedProperty bridgeField = so.FindProperty("explanationBridge");
        SerializedProperty transportField = so.FindProperty("assistantTransport");
        if (bridgeField != null) bridgeField.objectReferenceValue = explanation;
        // Keep a previously assigned transport implementing the interface.
        if (transportField != null && transportField.objectReferenceValue == null)
            transportField.objectReferenceValue = transport;
        so.ApplyModifiedProperties();

        if (explanation.PromptReady.GetPersistentEventCount() > 0)
            Debug.LogWarning("[Spark AI Connection] Prompt Ready already has Inspector callbacks. " +
                "Remove duplicate old send callbacks if the assistant receives two prompts.", root);

        PrefabUtility.RecordPrefabInstancePropertyModifications(connector);
        EditorUtility.SetDirty(connector);
        EditorUtility.SetDirty(transport);
        Selection.activeGameObject = root;
        Debug.Log("[Spark AI Connection] Connector installed. " +
            "Now connect Send Prompt to your existing SparkAIUnifiedAssistant string request method " +
            "and its completed JSON callback to ReceiveAssistantJson.", root);
    }
}
#endif
