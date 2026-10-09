
using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.EditorTools
{
    /// <summary>
    /// Exports a selected ScriptableObject to a readable UTF-8 TXT file.
    /// Uses Unity's serialized-property system to inspect serialized fields.
    /// Editor-only: place this script inside an Editor folder.
    /// </summary>
    public static class SparkAIKnowledgeTextExporter
    {
        private const string MenuPath =
            "Tools/Project Spark/Export Selected ScriptableObject to TXT";

        private const string FileExtension = "txt";

        [MenuItem(MenuPath, false, 200)]
        private static void ExportSelectedAsset()
        {
            UnityEngine.Object selected = Selection.activeObject;

            if (!(selected is ScriptableObject))
            {
                EditorUtility.DisplayDialog(
                    "Spark Text Exporter",
                    "Select a ScriptableObject asset in the Project window first.",
                    "OK");

                return;
            }

            ScriptableObject asset = (ScriptableObject)selected;

            string suggestedName =
                MakeSafeFileName(asset.name) + ".txt";

            string outputPath = EditorUtility.SaveFilePanel(
                "Export ScriptableObject to TXT",
                Application.dataPath,
                suggestedName,
                FileExtension);

            if (string.IsNullOrWhiteSpace(outputPath))
                return;

            try
            {
                string text = BuildText(asset);

                File.WriteAllText(
                    outputPath,
                    text,
                    new UTF8Encoding(false));

                Debug.Log(
                    "[Spark Text Exporter] Export completed.\n" +
                    "Asset: " + asset.name + "\n" +
                    "File: " + outputPath,
                    asset);

                EditorUtility.DisplayDialog(
                    "Export Complete",
                    "ScriptableObject exported successfully.\n\n" +
                    outputPath,
                    "OK");
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, asset);

                EditorUtility.DisplayDialog(
                    "Export Failed",
                    exception.Message,
                    "OK");
            }
        }

        [MenuItem(MenuPath, true)]
        private static bool ValidateExport()
        {
            return Selection.activeObject is ScriptableObject;
        }

        private static string BuildText(ScriptableObject asset)
        {
            StringBuilder builder = new StringBuilder(8192);

            SerializedObject serializedObject =
                new SerializedObject(asset);

            serializedObject.UpdateIfRequiredOrScript();

            builder.AppendLine(
                "============================================================");

            builder.AppendLine("PROJECT SPARK — SCRIPTABLEOBJECT EXPORT");

            builder.AppendLine(
                "============================================================");

            builder.AppendLine();

            builder.AppendLine("ASSET INFORMATION");
            builder.AppendLine("-----------------");
            builder.AppendLine("Asset Name: " + asset.name);
            builder.AppendLine(
                "Asset Type: " + asset.GetType().FullName);

            string assetPath = AssetDatabase.GetAssetPath(asset);

            builder.AppendLine(
                "Unity Asset Path: " +
                (string.IsNullOrEmpty(assetPath)
                    ? "(Not saved as a project asset)"
                    : assetPath));

            builder.AppendLine(
                "Export Date: " +
                DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

            builder.AppendLine();

            builder.AppendLine("SERIALIZED DATA");
            builder.AppendLine("---------------");
            builder.AppendLine();

            SerializedProperty iterator =
                serializedObject.GetIterator();

            bool enterChildren = true;
            bool wroteProperty = false;

            while (iterator.NextVisible(enterChildren))
            {
                enterChildren = false;

                // Script references are Unity implementation details,
                // not useful knowledge content.
                if (iterator.propertyPath == "m_Script")
                    continue;

                WriteProperty(
                    builder,
                    iterator,
                    0);

                wroteProperty = true;
            }

            if (!wroteProperty)
            {
                builder.AppendLine(
                    "(No visible serialized fields found.)");
            }

            builder.AppendLine();
            builder.AppendLine(
                "============================================================");
            builder.AppendLine("END OF EXPORT");
            builder.AppendLine(
                "============================================================");

            return builder.ToString();
        }

        private static void WriteProperty(
            StringBuilder builder,
            SerializedProperty property,
            int depth)
        {
            string indent = new string(' ', depth * 2);

            // Strings can report isArray in some contexts.
            // Do not treat them as lists.
            if (property.isArray &&
                property.propertyType != SerializedPropertyType.String)
            {
                builder.Append(indent);
                builder.AppendLine(
                    property.displayName + ":");

                int count = property.arraySize;

                if (count == 0)
                {
                    builder.Append(indent);
                    builder.AppendLine("  (Empty)");
                    return;
                }

                for (int i = 0; i < count; i++)
                {
                    SerializedProperty element =
                        property.GetArrayElementAtIndex(i);

                    builder.Append(indent);
                    builder.AppendLine("  [" + i + "]");

                    WriteProperty(
                        builder,
                        element,
                        depth + 2);
                }

                return;
            }

            if (property.propertyType ==
                SerializedPropertyType.Generic)
            {
                builder.Append(indent);
                builder.AppendLine(property.displayName + ":");

                WriteDirectChildren(
                    builder,
                    property,
                    depth + 1);

                return;
            }

            builder.Append(indent);
            builder.Append(property.displayName);
            builder.Append(": ");
            builder.AppendLine(GetPropertyValue(property));
        }

        private static void WriteDirectChildren(
            StringBuilder builder,
            SerializedProperty parent,
            int depth)
        {
            SerializedProperty iterator = parent.Copy();
            SerializedProperty end = iterator.GetEndProperty();

            bool enterChildren = true;
            bool foundChild = false;

            while (iterator.NextVisible(enterChildren) &&
                   !SerializedProperty.EqualContents(iterator, end))
            {
                enterChildren = false;

                // Only process immediate children here.
                // Nested properties are handled recursively.
                if (iterator.depth != parent.depth + 1)
                    continue;

                if (iterator.propertyPath == "m_Script")
                    continue;

                WriteProperty(
                    builder,
                    iterator,
                    depth);

                foundChild = true;
            }

            if (!foundChild)
            {
                builder.Append(
                    new string(' ', depth * 2));

                builder.AppendLine("(No visible fields)");
            }
        }

        private static string GetPropertyValue(
            SerializedProperty property)
        {
            switch (property.propertyType)
            {
                case SerializedPropertyType.String:
                    return FormatString(property.stringValue);

                case SerializedPropertyType.Boolean:
                    return property.boolValue ? "True" : "False";

                case SerializedPropertyType.Integer:
                    return property.longValue.ToString();

                case SerializedPropertyType.Float:
                    return property.doubleValue.ToString(
                        "G",
                        System.Globalization.CultureInfo.InvariantCulture);

                case SerializedPropertyType.Enum:
                    return property.enumValueIndex >= 0 &&
                           property.enumValueIndex <
                           property.enumDisplayNames.Length
                        ? property.enumDisplayNames[
                            property.enumValueIndex]
                        : property.enumValueIndex.ToString();

                case SerializedPropertyType.Character:
                    return ((char)property.intValue).ToString();

                case SerializedPropertyType.Color:
                    return property.colorValue.ToString();

                case SerializedPropertyType.ObjectReference:
                    return FormatObjectReference(
                        property.objectReferenceValue);

                case SerializedPropertyType.ExposedReference:
                    return FormatObjectReference(
                        property.exposedReferenceValue);

                case SerializedPropertyType.LayerMask:
                    return property.intValue.ToString();

                case SerializedPropertyType.Vector2:
                    return property.vector2Value.ToString("G");

                case SerializedPropertyType.Vector3:
                    return property.vector3Value.ToString("G");

                case SerializedPropertyType.Vector4:
                    return property.vector4Value.ToString("G");

                case SerializedPropertyType.Vector2Int:
                    return property.vector2IntValue.ToString();

                case SerializedPropertyType.Vector3Int:
                    return property.vector3IntValue.ToString();

                case SerializedPropertyType.Rect:
                    return property.rectValue.ToString();

                case SerializedPropertyType.RectInt:
                    return property.rectIntValue.ToString();

                case SerializedPropertyType.Bounds:
                    return property.boundsValue.ToString();

                case SerializedPropertyType.BoundsInt:
                    return property.boundsIntValue.ToString();

                case SerializedPropertyType.Quaternion:
                    return property.quaternionValue.eulerAngles.ToString("G")
                           + " (Euler angles)";

                case SerializedPropertyType.AnimationCurve:
                    return FormatCurve(property.animationCurveValue);

                case SerializedPropertyType.ManagedReference:
                    return property.managedReferenceValue == null
                        ? "(Null managed reference)"
                        : property.managedReferenceFullTypename;

                case SerializedPropertyType.Gradient:
                    return "(Gradient data not expanded by this exporter)";

                default:
                    return "(Value type: " +
                           property.propertyType + ")";
            }
        }

        private static string FormatObjectReference(
            UnityEngine.Object referencedObject)
        {
            if (referencedObject == null)
                return "(None)";

            string path =
                AssetDatabase.GetAssetPath(referencedObject);

            if (string.IsNullOrEmpty(path))
                return referencedObject.name;

            return referencedObject.name + " [" + path + "]";
        }

        private static string FormatString(string value)
        {
            if (string.IsNullOrEmpty(value))
                return "(Empty)";

            // Preserve multiline text while making empty strings explicit.
            return "\n" + value;
        }

        private static string FormatCurve(AnimationCurve curve)
        {
            if (curve == null)
                return "(None)";

            StringBuilder builder = new StringBuilder();

            builder.Append("Keys: ");
            builder.Append(curve.length);

            for (int i = 0; i < curve.length; i++)
            {
                Keyframe key = curve.keys[i];

                builder.Append(
                    "\n    Time: " +
                    key.time.ToString(
                        "G",
                        System.Globalization.CultureInfo.InvariantCulture) +
                    ", Value: " +
                    key.value.ToString(
                        "G",
                        System.Globalization.CultureInfo.InvariantCulture));
            }

            return builder.ToString();
        }

        private static string MakeSafeFileName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return "SparkKnowledge";

            foreach (char invalidChar in
                     Path.GetInvalidFileNameChars())
            {
                value = value.Replace(invalidChar, '_');
            }

            return value.Trim();
        }
    }
}
