#if UNITY_EDITOR
using System.Text;
using UnityEditor;
using UnityEngine;
using ProjectSpark.UI.VisualTeaching;

public static class SparkTeachingAnchorEditor
{
    [MenuItem("Tools/Project Spark/AI/Add Anchors to Selected Scene Objects")]
    private static void MarkSelected()
    {
        int count=0;
        foreach (GameObject go in Selection.gameObjects)
        {
            if (go == null || !go.scene.IsValid()) continue;
            SparkTeachingAnchor anchor = go.GetComponent<SparkTeachingAnchor>();
            if (anchor == null) anchor = Undo.AddComponent<SparkTeachingAnchor>(go);
            SerializedObject so = new SerializedObject(anchor);
            so.FindProperty("anchorId").stringValue = IdFor(go.name);
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(anchor);
            count++;
        }
        Debug.Log("[Spark Teaching] Created/updated " + count + " scene anchors. " +
                  "Inspect IDs; IDs must be unique and match AI plan references.");
    }

    private static string IdFor(string value)
    {
        StringBuilder sb = new StringBuilder();
        foreach (char c in value.ToLowerInvariant())
        {
            if (char.IsLetterOrDigit(c) || c == '_') sb.Append(c);
            else if (sb.Length > 0 && sb[sb.Length-1] != '_') sb.Append('_');
        }
        return sb.ToString().Trim('_');
    }
}
#endif
