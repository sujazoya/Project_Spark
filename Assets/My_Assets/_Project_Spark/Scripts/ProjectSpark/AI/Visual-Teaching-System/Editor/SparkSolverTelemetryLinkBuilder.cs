#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using ProjectSpark.UI.VisualTeaching;

public static class SparkSolverTelemetryLinkBuilder
{
    [MenuItem("Tools/Project Spark/AI/Connect Solver Telemetry Link")]
    public static void Install()
    {
        SparkVisualTeachingManager teacher = null;
        if (Selection.activeGameObject != null)
            teacher = Selection.activeGameObject.GetComponentInParent<SparkVisualTeachingManager>();
        if (teacher == null)
            teacher = Object.FindFirstObjectByType<SparkVisualTeachingManager>(
                FindObjectsInactive.Include);

        if (teacher == null)
        {
            Debug.LogError("[Solver Link] AAA Visual Teaching Manager not found. " +
                "Create the Visual Teaching System first.");
            return;
        }

        SparkElectricalTelemetryBridge telemetry =
            teacher.GetComponent<SparkElectricalTelemetryBridge>();
        if (telemetry == null)
            telemetry = Object.FindFirstObjectByType<SparkElectricalTelemetryBridge>(
                FindObjectsInactive.Include);

        if (telemetry == null)
        {
            Debug.LogError("[Solver Link] Live Electrical Telemetry not found. " +
                "Run Tools > Project Spark > AI > Add Live Electrical Telemetry first.", teacher);
            return;
        }

        SparkVisualTeachingLiveBridge liveBridge =
            teacher.GetComponent<SparkVisualTeachingLiveBridge>();
        if (liveBridge == null)
            liveBridge = Object.FindFirstObjectByType<SparkVisualTeachingLiveBridge>(
                FindObjectsInactive.Include);

        SparkVerifiedElectricalPublisher publisher =
            teacher.GetComponent<SparkVerifiedElectricalPublisher>();
        if (publisher == null)
        {
            SparkVerifiedElectricalPublisher[] publishers =
                Object.FindObjectsByType<SparkVerifiedElectricalPublisher>(
                    FindObjectsInactive.Include, FindObjectsSortMode.None);
            if (publishers.Length == 1) publisher = publishers[0];
        }
        if (publisher == null)
            publisher = Undo.AddComponent<SparkVerifiedElectricalPublisher>(teacher.gameObject);

        SparkSolverTelemetrySync sync = teacher.GetComponent<SparkSolverTelemetrySync>();
        if (sync == null)
            sync = Undo.AddComponent<SparkSolverTelemetrySync>(teacher.gameObject);

        SerializedObject serialized = new SerializedObject(sync);
        Set(serialized, "publisher", publisher);
        Set(serialized, "telemetry", telemetry);
        Set(serialized, "liveTeaching", liveBridge);
        serialized.ApplyModifiedProperties();
        EditorUtility.SetDirty(sync);
        PrefabUtility.RecordPrefabInstancePropertyModifications(sync);
        Selection.activeGameObject = sync.gameObject;

        Debug.Log("[Solver Link] Telemetry link installed on " + sync.name +
            ". Publisher is ready for REAL completed solver snapshots. " +
            "No measurements are invented or polled automatically.", sync);
        if (liveBridge == null)
            Debug.LogWarning("[Solver Link] Optional live teaching bridge was not found. " +
                "Measurements still work; auto fault teaching is unavailable.", sync);
    }

    private static void Set(SerializedObject serialized, string field, Object value)
    {
        SerializedProperty property = serialized.FindProperty(field);
        if (property == null)
            Debug.LogError("[Solver Link] Missing field: " + field);
        else
            property.objectReferenceValue = value;
    }
}
#endif
