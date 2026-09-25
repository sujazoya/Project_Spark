#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace ProjectSpark.Gameplay.Editor
{
    /// <summary>
    /// Advanced custom inspector for SparkLevelSceneBindings.
    ///
    /// Handles:
    /// - Terminal bindings
    /// - Electrical component bindings
    /// - Electronic object bindings
    /// - GameObject output bindings
    /// - Automatic ID generation
    /// - Duplicate ID detection
    /// - Missing reference detection
    /// - Validation
    /// - Cleanup
    /// - Reordering
    /// - Object selection
    ///
    /// This editor only manages scene-side bindings.
    /// It does not create, modify, or own SparkLevelDefinition assets.
    /// </summary>
    [CustomEditor(typeof(SparkLevelSceneBindings))]
    public sealed class SparkLevelSceneBindingsEditor : UnityEditor.Editor
    {
        // ============================================================
        // SERIALIZED PROPERTIES
        // ============================================================

        private SerializedProperty terminalsProperty;
        private SerializedProperty componentsProperty;
        private SerializedProperty electronicObjectsProperty;
        private SerializedProperty outputsProperty;

        // ============================================================
        // REORDERABLE LISTS
        // ============================================================

        private ReorderableList terminalList;
        private ReorderableList componentList;
        private ReorderableList electronicObjectList;
        private ReorderableList outputList;

        // ============================================================
        // SECTION STATE
        // ============================================================

        private bool showOverview = true;
        private bool showTerminals = true;
        private bool showComponents = true;
        private bool showElectronicObjects = true;
        private bool showOutputs = true;
        private bool showTools = true;

        // ============================================================
        // VALIDATION STATE
        // ============================================================

        private readonly List<string> terminalErrors =
            new List<string>();

        private readonly List<string> componentErrors =
            new List<string>();

        private readonly List<string> electronicObjectErrors =
            new List<string>();

        private readonly List<string> outputErrors =
            new List<string>();

        private bool validationPerformed;

        // ============================================================
        // CONSTANTS
        // ============================================================

        private const float ButtonHeight = 24f;

        private const string TerminalPrefix = "Terminal";
        private const string ComponentPrefix = "Component";
        private const string ElectronicObjectPrefix = "ElectronicObject";
        private const string OutputPrefix = "Output";

        // ============================================================
        // UNITY
        // ============================================================

        private void OnEnable()
        {
            terminalsProperty =
                serializedObject.FindProperty("terminals");

            componentsProperty =
                serializedObject.FindProperty("components");

            electronicObjectsProperty =
                serializedObject.FindProperty("electronicObjects");

            outputsProperty =
                serializedObject.FindProperty("outputs");

            CreateLists();

            validationPerformed = false;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawTitle();

            EditorGUILayout.Space(4f);

            DrawOverview();

            EditorGUILayout.Space(4f);

            DrawTerminalSection();

            EditorGUILayout.Space(4f);

            DrawComponentSection();

            EditorGUILayout.Space(4f);

            DrawElectronicObjectSection();

            EditorGUILayout.Space(4f);

            DrawOutputSection();

            EditorGUILayout.Space(4f);

            DrawTools();

            serializedObject.ApplyModifiedProperties();
        }

        // ============================================================
        // TITLE
        // ============================================================

        private void DrawTitle()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.BeginHorizontal();

            GUILayout.Label(
                "SPARK LEVEL SCENE BINDINGS",
                EditorStyles.boldLabel);

            GUILayout.FlexibleSpace();

            if (GUILayout.Button(
                    "Validate",
                    GUILayout.Width(80f),
                    GUILayout.Height(ButtonHeight)))
            {
                ValidateAll();
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(2f);

            EditorGUILayout.LabelField(
                "Scene objects mapped to stable level IDs.",
                EditorStyles.miniLabel);

            EditorGUILayout.LabelField(
                "This component contains scene references only.",
                EditorStyles.miniLabel);

            EditorGUILayout.EndVertical();
        }

        // ============================================================
        // OVERVIEW
        // ============================================================

        private void DrawOverview()
        {
            showOverview = EditorGUILayout.Foldout(
                showOverview,
                "Binding Overview",
                true,
                EditorStyles.foldoutHeader);

            if (!showOverview)
            {
                return;
            }

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            int terminalCount =
                terminalsProperty.arraySize;

            int componentCount =
                componentsProperty.arraySize;

            int electronicObjectCount =
                electronicObjectsProperty.arraySize;

            int outputCount =
                outputsProperty.arraySize;

            EditorGUILayout.BeginHorizontal();

            DrawStatBox(
                "TERMINALS",
                terminalCount);

            DrawStatBox(
                "COMPONENTS",
                componentCount);

            DrawStatBox(
                "ELECTRONIC",
                electronicObjectCount);

            DrawStatBox(
                "OUTPUTS",
                outputCount);

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5f);

            int total =
                terminalCount +
                componentCount +
                electronicObjectCount +
                outputCount;

            EditorGUILayout.LabelField(
                "Total Scene Bindings",
                total.ToString(),
                EditorStyles.boldLabel);

            EditorGUILayout.EndVertical();
        }

        // ============================================================
        // STAT BOX
        // ============================================================

        private void DrawStatBox(
            string title,
            int value)
        {
            EditorGUILayout.BeginVertical(
                EditorStyles.helpBox,
                GUILayout.MinHeight(50f));

            GUIStyle valueStyle =
                new GUIStyle(EditorStyles.boldLabel)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = 18
                };

            GUIStyle titleStyle =
                new GUIStyle(EditorStyles.miniLabel)
                {
                    alignment = TextAnchor.MiddleCenter
                };

            GUILayout.Label(
                value.ToString(),
                valueStyle);

            GUILayout.Label(
                title,
                titleStyle);

            EditorGUILayout.EndVertical();
        }

        // ============================================================
        // TERMINALS
        // ============================================================

        private void DrawTerminalSection()
        {
            showTerminals = DrawSectionHeader(
                "Terminal Bindings",
                terminalsProperty.arraySize,
                showTerminals);

            if (!showTerminals)
            {
                return;
            }

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            terminalList.DoLayoutList();

            DrawValidationSummary(
                terminalErrors,
                terminalsProperty.arraySize,
                "terminal");

            EditorGUILayout.EndVertical();
        }

        // ============================================================
        // COMPONENTS
        // ============================================================

        private void DrawComponentSection()
        {
            showComponents = DrawSectionHeader(
                "Electrical Component Bindings",
                componentsProperty.arraySize,
                showComponents);

            if (!showComponents)
            {
                return;
            }

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            componentList.DoLayoutList();

            DrawValidationSummary(
                componentErrors,
                componentsProperty.arraySize,
                "electrical component");

            EditorGUILayout.EndVertical();
        }

        // ============================================================
        // ELECTRONIC OBJECTS
        // ============================================================

        private void DrawElectronicObjectSection()
        {
            showElectronicObjects = DrawSectionHeader(
                "Electronic Object Bindings",
                electronicObjectsProperty.arraySize,
                showElectronicObjects);

            if (!showElectronicObjects)
            {
                return;
            }

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            electronicObjectList.DoLayoutList();

            DrawValidationSummary(
                electronicObjectErrors,
                electronicObjectsProperty.arraySize,
                "electronic object");

            EditorGUILayout.EndVertical();
        }

        // ============================================================
        // OUTPUTS
        // ============================================================

        private void DrawOutputSection()
        {
            showOutputs = DrawSectionHeader(
                "GameObject Output Bindings",
                outputsProperty.arraySize,
                showOutputs);

            if (!showOutputs)
            {
                return;
            }

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            outputList.DoLayoutList();

            DrawValidationSummary(
                outputErrors,
                outputsProperty.arraySize,
                "GameObject output");

            EditorGUILayout.EndVertical();
        }

        // ============================================================
        // SECTION HEADER
        // ============================================================

        private bool DrawSectionHeader(
            string title,
            int count,
            bool expanded)
        {
            EditorGUILayout.BeginHorizontal(
                EditorStyles.helpBox);

            expanded = EditorGUILayout.Foldout(
                expanded,
                title,
                true,
                EditorStyles.foldoutHeader);

            GUILayout.FlexibleSpace();

            GUILayout.Label(
                count.ToString(),
                EditorStyles.miniBoldLabel,
                GUILayout.Width(30f));

            EditorGUILayout.EndHorizontal();

            return expanded;
        }

        // ============================================================
        // VALIDATION SUMMARY
        // ============================================================

        private void DrawValidationSummary(
            List<string> errors,
            int count,
            string typeName)
        {
            if (!validationPerformed)
            {
                return;
            }

            if (errors.Count == 0)
            {
                EditorGUILayout.HelpBox(
                    count == 0
                        ? $"No {typeName} bindings configured."
                        : $"All {typeName} bindings are valid.",
                    count == 0
                        ? MessageType.Info
                        : MessageType.Info);

                return;
            }

            for (int i = 0; i < errors.Count; i++)
            {
                EditorGUILayout.HelpBox(
                    errors[i],
                    MessageType.Error);
            }
        }

        // ============================================================
        // TOOLS
        // ============================================================

        private void DrawTools()
        {
            showTools = EditorGUILayout.Foldout(
                showTools,
                "Binding Tools",
                true,
                EditorStyles.foldoutHeader);

            if (!showTools)
            {
                return;
            }

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button(
                    "Validate All",
                    GUILayout.Height(ButtonHeight)))
            {
                ValidateAll();
            }

            if (GUILayout.Button(
                    "Generate Missing IDs",
                    GUILayout.Height(ButtonHeight)))
            {
                GenerateMissingIds();
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(3f);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button(
                    "Fix Duplicate IDs",
                    GUILayout.Height(ButtonHeight)))
            {
                FixDuplicateIds();
            }

            if (GUILayout.Button(
                    "Normalize IDs",
                    GUILayout.Height(ButtonHeight)))
            {
                NormalizeIds();
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(3f);

            if (GUILayout.Button(
                    "Remove Empty Entries",
                    GUILayout.Height(ButtonHeight)))
            {
                RemoveEmptyEntries();
            }

            EditorGUILayout.Space(5f);

            EditorGUILayout.HelpBox(
                "IDs are case-sensitive. IDs only need to be unique " +
                "inside their own binding category.",
                MessageType.Info);

            EditorGUILayout.EndVertical();
        }

        // ============================================================
        // LIST CREATION
        // ============================================================

        private void CreateLists()
        {
            terminalList =
                CreateBindingList(
                    terminalsProperty,
                    "Terminal Binding",
                    DrawTerminalElement,
                    AddTerminal,
                    RemoveTerminal);

            componentList =
                CreateBindingList(
                    componentsProperty,
                    "Electrical Component Binding",
                    DrawComponentElement,
                    AddComponent,
                    RemoveComponent);

            electronicObjectList =
                CreateBindingList(
                    electronicObjectsProperty,
                    "Electronic Object Binding",
                    DrawElectronicObjectElement,
                    AddElectronicObject,
                    RemoveElectronicObject);

            outputList =
                CreateBindingList(
                    outputsProperty,
                    "GameObject Output Binding",
                    DrawOutputElement,
                    AddOutput,
                    RemoveOutput);
        }

        // ============================================================
        // GENERIC REORDERABLE LIST
        // ============================================================

        private ReorderableList CreateBindingList(
            SerializedProperty property,
            string header,
            Action<Rect, SerializedProperty, int> drawElement,
            Action add,
            Action remove)
        {
            ReorderableList list =
                new ReorderableList(
                    serializedObject,
                    property,
                    true,
                    true,
                    true,
                    true);

            list.drawHeaderCallback =
                rect =>
                {
                    EditorGUI.LabelField(
                        rect,
                        header);
                };

            list.drawElementCallback =
                (rect, index, active, focused) =>
                {
                    if (index < 0 ||
                        index >= property.arraySize)
                    {
                        return;
                    }

                    SerializedProperty element =
                        property.GetArrayElementAtIndex(index);

                    drawElement(
                        rect,
                        element,
                        index);
                };

            list.elementHeightCallback =
                index =>
                {
                    return EditorGUIUtility.singleLineHeight * 3f +
                           12f;
                };

            list.onAddCallback =
                _ =>
                {
                    add();
                };

            list.onRemoveCallback =
                listInstance =>
                {
                    if (listInstance.index < 0 ||
                        listInstance.index >= property.arraySize)
                    {
                        return;
                    }

                    remove();
                };

            list.onReorderCallback =
                _ =>
                {
                    serializedObject.ApplyModifiedProperties();

                    EditorUtility.SetDirty(target);

                    validationPerformed = false;
                };

            return list;
        }

        // ============================================================
        // TERMINAL ELEMENT
        // ============================================================

        private void DrawTerminalElement(
            Rect rect,
            SerializedProperty element,
            int index)
        {
            SerializedProperty id =
                element.FindPropertyRelative("id");

            SerializedProperty reference =
                element.FindPropertyRelative("terminal");

            DrawBindingElement(
                rect,
                id,
                reference,
                index,
                "SparkTerminal");
        }

        // ============================================================
        // COMPONENT ELEMENT
        // ============================================================

        private void DrawComponentElement(
            Rect rect,
            SerializedProperty element,
            int index)
        {
            SerializedProperty id =
                element.FindPropertyRelative("id");

            SerializedProperty reference =
                element.FindPropertyRelative("component");

            DrawBindingElement(
                rect,
                id,
                reference,
                index,
                "SparkElectricalComponent");
        }

        // ============================================================
        // ELECTRONIC OBJECT ELEMENT
        // ============================================================

        private void DrawElectronicObjectElement(
            Rect rect,
            SerializedProperty element,
            int index)
        {
            SerializedProperty id =
                element.FindPropertyRelative("id");

            SerializedProperty reference =
                element.FindPropertyRelative(
                    "electronicObject");

            DrawBindingElement(
                rect,
                id,
                reference,
                index,
                "SparkElectronicObject");
        }

        // ============================================================
        // OUTPUT ELEMENT
        // ============================================================

        private void DrawOutputElement(
            Rect rect,
            SerializedProperty element,
            int index)
        {
            SerializedProperty id =
                element.FindPropertyRelative("id");

            SerializedProperty reference =
                element.FindPropertyRelative("target");

            DrawBindingElement(
                rect,
                id,
                reference,
                index,
                "GameObject");
        }

        // ============================================================
        // GENERIC BINDING ELEMENT
        // ============================================================

        private void DrawBindingElement(
            Rect rect,
            SerializedProperty id,
            SerializedProperty reference,
            int index,
            string referenceLabel)
        {
            float lineHeight =
                EditorGUIUtility.singleLineHeight;

            Rect firstLine =
                new Rect(
                    rect.x,
                    rect.y + 1f,
                    rect.width,
                    lineHeight);

            Rect secondLine =
                new Rect(
                    rect.x,
                    rect.y + lineHeight + 4f,
                    rect.width,
                    lineHeight);

            Rect thirdLine =
                new Rect(
                    rect.x,
                    rect.y + (lineHeight * 2f) + 7f,
                    rect.width,
                    lineHeight);

            // --------------------------------------------------------
            // INDEX
            // --------------------------------------------------------

            Rect indexRect =
                new Rect(
                    firstLine.x,
                    firstLine.y,
                    34f,
                    lineHeight);

            EditorGUI.LabelField(
                indexRect,
                $"[{index}]",
                EditorStyles.miniLabel);

            // --------------------------------------------------------
            // ID
            // --------------------------------------------------------

            Rect idRect =
                new Rect(
                    firstLine.x + 38f,
                    firstLine.y,
                    firstLine.width - 38f,
                    lineHeight);

            EditorGUI.PropertyField(
                idRect,
                id,
                new GUIContent("ID"));

            // --------------------------------------------------------
            // REFERENCE
            // --------------------------------------------------------

            EditorGUI.PropertyField(
                secondLine,
                reference,
                new GUIContent(referenceLabel));

            // --------------------------------------------------------
            // STATUS
            // --------------------------------------------------------

            DrawBindingStatus(
                thirdLine,
                id,
                reference);
        }

        // ============================================================
        // BINDING STATUS
        // ============================================================

        private void DrawBindingStatus(
            Rect rect,
            SerializedProperty id,
            SerializedProperty reference)
        {
            bool hasId =
                !string.IsNullOrWhiteSpace(
                    id.stringValue);

            bool hasReference =
                reference.objectReferenceValue != null;

            if (!hasId)
            {
                EditorGUI.LabelField(
                    rect,
                    "●  Missing ID",
                    EditorStyles.miniLabel);

                return;
            }

            if (!hasReference)
            {
                EditorGUI.LabelField(
                    rect,
                    "●  Missing scene reference",
                    EditorStyles.miniLabel);

                return;
            }

            EditorGUI.LabelField(
                rect,
                "●  Binding ready",
                EditorStyles.miniLabel);

            // --------------------------------------------------------
            // SELECT BUTTON
            // --------------------------------------------------------

            Rect selectRect =
                new Rect(
                    rect.xMax - 64f,
                    rect.y,
                    30f,
                    rect.height);

            Rect pingRect =
                new Rect(
                    rect.xMax - 30f,
                    rect.y,
                    30f,
                    rect.height);

            if (GUI.Button(
                    selectRect,
                    "S",
                    EditorStyles.miniButton))
            {
                Selection.activeObject =
                    reference.objectReferenceValue;
            }

            if (GUI.Button(
                    pingRect,
                    "P",
                    EditorStyles.miniButton))
            {
                EditorGUIUtility.PingObject(
                    reference.objectReferenceValue);
            }
        }

        // ============================================================
        // ADD TERMINAL
        // ============================================================

        private void AddTerminal()
        {
            serializedObject.Update();

            int index =
                terminalsProperty.arraySize;

            terminalsProperty.InsertArrayElementAtIndex(
                index);

            SerializedProperty element =
                terminalsProperty.GetArrayElementAtIndex(
                    index);

            SerializedProperty id =
                element.FindPropertyRelative("id");

            SerializedProperty reference =
                element.FindPropertyRelative("terminal");

            id.stringValue =
                GenerateUniqueId(
                    terminalsProperty,
                    TerminalPrefix);

            reference.objectReferenceValue = null;

            serializedObject.ApplyModifiedProperties();

            EditorUtility.SetDirty(target);

            validationPerformed = false;
        }

        // ============================================================
        // ADD COMPONENT
        // ============================================================

        private void AddComponent()
        {
            serializedObject.Update();

            int index =
                componentsProperty.arraySize;

            componentsProperty.InsertArrayElementAtIndex(
                index);

            SerializedProperty element =
                componentsProperty.GetArrayElementAtIndex(
                    index);

            SerializedProperty id =
                element.FindPropertyRelative("id");

            SerializedProperty reference =
                element.FindPropertyRelative("component");

            id.stringValue =
                GenerateUniqueId(
                    componentsProperty,
                    ComponentPrefix);

            reference.objectReferenceValue = null;

            serializedObject.ApplyModifiedProperties();

            EditorUtility.SetDirty(target);

            validationPerformed = false;
        }

        // ============================================================
        // ADD ELECTRONIC OBJECT
        // ============================================================

        private void AddElectronicObject()
        {
            serializedObject.Update();

            int index =
                electronicObjectsProperty.arraySize;

            electronicObjectsProperty.InsertArrayElementAtIndex(
                index);

            SerializedProperty element =
                electronicObjectsProperty.GetArrayElementAtIndex(
                    index);

            SerializedProperty id =
                element.FindPropertyRelative("id");

            SerializedProperty reference =
                element.FindPropertyRelative(
                    "electronicObject");

            id.stringValue =
                GenerateUniqueId(
                    electronicObjectsProperty,
                    ElectronicObjectPrefix);

            reference.objectReferenceValue = null;

            serializedObject.ApplyModifiedProperties();

            EditorUtility.SetDirty(target);

            validationPerformed = false;
        }

        // ============================================================
        // ADD OUTPUT
        // ============================================================

        private void AddOutput()
        {
            serializedObject.Update();

            int index =
                outputsProperty.arraySize;

            outputsProperty.InsertArrayElementAtIndex(
                index);

            SerializedProperty element =
                outputsProperty.GetArrayElementAtIndex(
                    index);

            SerializedProperty id =
                element.FindPropertyRelative("id");

            SerializedProperty reference =
                element.FindPropertyRelative("target");

            id.stringValue =
                GenerateUniqueId(
                    outputsProperty,
                    OutputPrefix);

            reference.objectReferenceValue = null;

            serializedObject.ApplyModifiedProperties();

            EditorUtility.SetDirty(target);

            validationPerformed = false;
        }

        // ============================================================
        // REMOVE TERMINAL
        // ============================================================

        private void RemoveTerminal()
        {
            RemoveSelected(
                terminalsProperty,
                terminalList);
        }

        // ============================================================
        // REMOVE COMPONENT
        // ============================================================

        private void RemoveComponent()
        {
            RemoveSelected(
                componentsProperty,
                componentList);
        }

        // ============================================================
        // REMOVE ELECTRONIC OBJECT
        // ============================================================

        private void RemoveElectronicObject()
        {
            RemoveSelected(
                electronicObjectsProperty,
                electronicObjectList);
        }

        // ============================================================
        // REMOVE OUTPUT
        // ============================================================

        private void RemoveOutput()
        {
            RemoveSelected(
                outputsProperty,
                outputList);
        }

        // ============================================================
        // REMOVE SELECTED
        // ============================================================

        private void RemoveSelected(
            SerializedProperty property,
            ReorderableList list)
        {
            if (list.index < 0 ||
                list.index >= property.arraySize)
            {
                return;
            }

            if (!EditorUtility.DisplayDialog(
                    "Remove Binding",
                    "Remove the selected scene binding?",
                    "Remove",
                    "Cancel"))
            {
                return;
            }

            serializedObject.Update();

            property.DeleteArrayElementAtIndex(
                list.index);

            serializedObject.ApplyModifiedProperties();

            EditorUtility.SetDirty(target);

            validationPerformed = false;
        }

        // ============================================================
        // GENERATE MISSING IDS
        // ============================================================

        private void GenerateMissingIds()
        {
            serializedObject.Update();

            int generated = 0;

            generated +=
                GenerateMissingIdsForArray(
                    terminalsProperty,
                    TerminalPrefix);

            generated +=
                GenerateMissingIdsForArray(
                    componentsProperty,
                    ComponentPrefix);

            generated +=
                GenerateMissingIdsForArray(
                    electronicObjectsProperty,
                    ElectronicObjectPrefix);

            generated +=
                GenerateMissingIdsForArray(
                    outputsProperty,
                    OutputPrefix);

            serializedObject.ApplyModifiedProperties();

            EditorUtility.SetDirty(target);

            validationPerformed = false;

            Debug.Log(
                $"[{nameof(SparkLevelSceneBindings)}] " +
                $"Generated {generated} missing ID(s).",
                target);
        }

        // ============================================================
        // GENERATE MISSING IDS FOR ARRAY
        // ============================================================

        private int GenerateMissingIdsForArray(
            SerializedProperty array,
            string prefix)
        {
            int generated = 0;

            for (int i = 0;
                 i < array.arraySize;
                 i++)
            {
                SerializedProperty element =
                    array.GetArrayElementAtIndex(i);

                SerializedProperty id =
                    element.FindPropertyRelative("id");

                if (!string.IsNullOrWhiteSpace(
                        id.stringValue))
                {
                    continue;
                }

                SerializedProperty reference =
                    FindReferenceProperty(element);

                string generatedId =
                    GenerateIdFromReference(
                        array,
                        prefix,
                        reference);

                id.stringValue = generatedId;

                generated++;
            }

            return generated;
        }

        // ============================================================
        // FIND REFERENCE PROPERTY
        // ============================================================

        private SerializedProperty FindReferenceProperty(
            SerializedProperty element)
        {
            SerializedProperty property =
                element.FindPropertyRelative(
                    "terminal");

            if (property != null)
            {
                return property;
            }

            property =
                element.FindPropertyRelative(
                    "component");

            if (property != null)
            {
                return property;
            }

            property =
                element.FindPropertyRelative(
                    "electronicObject");

            if (property != null)
            {
                return property;
            }

            return element.FindPropertyRelative(
                "target");
        }

        // ============================================================
        // GENERATE ID FROM REFERENCE
        // ============================================================

        private string GenerateIdFromReference(
            SerializedProperty array,
            string prefix,
            SerializedProperty reference)
        {
            if (reference != null &&
                reference.objectReferenceValue != null)
            {
                string objectName =
                    reference.objectReferenceValue.name;

                string cleanName =
                    SanitizeId(objectName);

                if (!string.IsNullOrWhiteSpace(cleanName))
                {
                    string candidate =
                        cleanName;

                    if (!ContainsId(array, candidate))
                    {
                        return candidate;
                    }

                    int suffix = 2;

                    while (true)
                    {
                        candidate =
                            $"{cleanName}_{suffix:00}";

                        if (!ContainsId(
                                array,
                                candidate))
                        {
                            return candidate;
                        }

                        suffix++;
                    }
                }
            }

            return GenerateUniqueId(
                array,
                prefix);
        }

        // ============================================================
        // SANITIZE ID
        // ============================================================

        private string SanitizeId(
            string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            value = value.Trim();

            char[] characters =
                value.ToCharArray();

            for (int i = 0;
                 i < characters.Length;
                 i++)
            {
                char character =
                    characters[i];

                if (char.IsLetterOrDigit(character) ||
                    character == '_' ||
                    character == '-')
                {
                    continue;
                }

                characters[i] = '_';
            }

            return new string(characters);
        }

        // ============================================================
        // UNIQUE ID
        // ============================================================

        private string GenerateUniqueId(
            SerializedProperty array,
            string prefix)
        {
            HashSet<string> ids =
                CollectIds(array);

            int index = 1;

            while (true)
            {
                string candidate =
                    $"{prefix}_{index:00}";

                if (!ids.Contains(candidate))
                {
                    return candidate;
                }

                index++;
            }
        }

        // ============================================================
        // COLLECT IDS
        // ============================================================

        private HashSet<string> CollectIds(
            SerializedProperty array)
        {
            HashSet<string> ids =
                new HashSet<string>(
                    StringComparer.Ordinal);

            for (int i = 0;
                 i < array.arraySize;
                 i++)
            {
                SerializedProperty element =
                    array.GetArrayElementAtIndex(i);

                SerializedProperty id =
                    element.FindPropertyRelative("id");

                if (string.IsNullOrWhiteSpace(
                        id.stringValue))
                {
                    continue;
                }

                ids.Add(
                    id.stringValue.Trim());
            }

            return ids;
        }

        // ============================================================
        // CONTAINS ID
        // ============================================================

        private bool ContainsId(
            SerializedProperty array,
            string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return false;
            }

            for (int i = 0;
                 i < array.arraySize;
                 i++)
            {
                SerializedProperty element =
                    array.GetArrayElementAtIndex(i);

                SerializedProperty idProperty =
                    element.FindPropertyRelative("id");

                if (string.Equals(
                        idProperty.stringValue,
                        id,
                        StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        // ============================================================
        // FIX DUPLICATE IDS
        // ============================================================

        private void FixDuplicateIds()
        {
            serializedObject.Update();

            int fixedCount = 0;

            fixedCount +=
                FixDuplicates(
                    terminalsProperty,
                    TerminalPrefix);

            fixedCount +=
                FixDuplicates(
                    componentsProperty,
                    ComponentPrefix);

            fixedCount +=
                FixDuplicates(
                    electronicObjectsProperty,
                    ElectronicObjectPrefix);

            fixedCount +=
                FixDuplicates(
                    outputsProperty,
                    OutputPrefix);

            serializedObject.ApplyModifiedProperties();

            EditorUtility.SetDirty(target);

            validationPerformed = false;

            Debug.Log(
                $"[{nameof(SparkLevelSceneBindings)}] " +
                $"Fixed {fixedCount} duplicate ID(s).",
                target);
        }

        // ============================================================
        // FIX DUPLICATES
        // ============================================================

        private int FixDuplicates(
            SerializedProperty array,
            string prefix)
        {
            HashSet<string> used =
                new HashSet<string>(
                    StringComparer.Ordinal);

            int fixedCount = 0;

            for (int i = 0;
                 i < array.arraySize;
                 i++)
            {
                SerializedProperty element =
                    array.GetArrayElementAtIndex(i);

                SerializedProperty id =
                    element.FindPropertyRelative("id");

                string value =
                    id.stringValue != null
                        ? id.stringValue.Trim()
                        : string.Empty;

                if (string.IsNullOrWhiteSpace(value))
                {
                    SerializedProperty reference =
                        FindReferenceProperty(element);

                    id.stringValue =
                        GenerateIdFromReference(
                            array,
                            prefix,
                            reference);

                    used.Add(
                        id.stringValue);

                    fixedCount++;

                    continue;
                }

                if (used.Add(value))
                {
                    id.stringValue = value;
                    continue;
                }

                SerializedProperty duplicateReference =
                    FindReferenceProperty(element);

                string newId =
                    GenerateIdFromReference(
                        array,
                        prefix,
                        duplicateReference);

                id.stringValue = newId;

                used.Add(newId);

                fixedCount++;
            }

            return fixedCount;
        }

        // ============================================================
        // NORMALIZE IDS
        // ============================================================

        private void NormalizeIds()
        {
            serializedObject.Update();

            NormalizeArrayIds(
                terminalsProperty);

            NormalizeArrayIds(
                componentsProperty);

            NormalizeArrayIds(
                electronicObjectsProperty);

            NormalizeArrayIds(
                outputsProperty);

            serializedObject.ApplyModifiedProperties();

            EditorUtility.SetDirty(target);

            validationPerformed = false;

            Debug.Log(
                $"[{nameof(SparkLevelSceneBindings)}] " +
                "IDs normalized.",
                target);
        }

        // ============================================================
        // NORMALIZE ARRAY
        // ============================================================

        private void NormalizeArrayIds(
            SerializedProperty array)
        {
            for (int i = 0;
                 i < array.arraySize;
                 i++)
            {
                SerializedProperty element =
                    array.GetArrayElementAtIndex(i);

                SerializedProperty id =
                    element.FindPropertyRelative("id");

                if (string.IsNullOrWhiteSpace(
                        id.stringValue))
                {
                    continue;
                }

                id.stringValue =
                    id.stringValue.Trim();
            }
        }

        // ============================================================
        // REMOVE EMPTY ENTRIES
        // ============================================================

        private void RemoveEmptyEntries()
        {
            serializedObject.Update();

            int removed = 0;

            removed +=
                RemoveEmptyFromArray(
                    terminalsProperty);

            removed +=
                RemoveEmptyFromArray(
                    componentsProperty);

            removed +=
                RemoveEmptyFromArray(
                    electronicObjectsProperty);

            removed +=
                RemoveEmptyFromArray(
                    outputsProperty);

            serializedObject.ApplyModifiedProperties();

            EditorUtility.SetDirty(target);

            validationPerformed = false;

            Debug.Log(
                $"[{nameof(SparkLevelSceneBindings)}] " +
                $"Removed {removed} completely empty binding(s).",
                target);
        }

        // ============================================================
        // REMOVE EMPTY FROM ARRAY
        // ============================================================

        private int RemoveEmptyFromArray(
            SerializedProperty array)
        {
            int removed = 0;

            for (int i = array.arraySize - 1;
                 i >= 0;
                 i--)
            {
                SerializedProperty element =
                    array.GetArrayElementAtIndex(i);

                SerializedProperty id =
                    element.FindPropertyRelative("id");

                SerializedProperty reference =
                    FindReferenceProperty(element);

                bool idEmpty =
                    string.IsNullOrWhiteSpace(
                        id.stringValue);

                bool referenceEmpty =
                    reference == null ||
                    reference.objectReferenceValue == null;

                if (!idEmpty ||
                    !referenceEmpty)
                {
                    continue;
                }

                array.DeleteArrayElementAtIndex(i);

                removed++;
            }

            return removed;
        }

        // ============================================================
        // VALIDATE ALL
        // ============================================================

        private void ValidateAll()
        {
            serializedObject.ApplyModifiedProperties();

            terminalErrors.Clear();
            componentErrors.Clear();
            electronicObjectErrors.Clear();
            outputErrors.Clear();

            ValidateArray(
                terminalsProperty,
                "Terminal",
                "terminal",
                terminalErrors);

            ValidateArray(
                componentsProperty,
                "Electrical component",
                "component",
                componentErrors);

            ValidateArray(
                electronicObjectsProperty,
                "Electronic object",
                "electronicObject",
                electronicObjectErrors);

            ValidateArray(
                outputsProperty,
                "GameObject output",
                "target",
                outputErrors);

            validationPerformed = true;

            Repaint();

            int totalErrors =
                terminalErrors.Count +
                componentErrors.Count +
                electronicObjectErrors.Count +
                outputErrors.Count;

            if (totalErrors == 0)
            {
                Debug.Log(
                    $"[{nameof(SparkLevelSceneBindings)}] " +
                    $"Validation successful on '{target.name}'.",
                    target);

                return;
            }

            Debug.LogError(
                $"[{nameof(SparkLevelSceneBindings)}] " +
                $"Validation failed on '{target.name}'. " +
                $"Errors: {totalErrors}.",
                target);
        }

        // ============================================================
        // VALIDATE ARRAY
        // ============================================================

        private void ValidateArray(
            SerializedProperty array,
            string displayName,
            string referencePropertyName,
            List<string> errors)
        {
            HashSet<string> ids =
                new HashSet<string>(
                    StringComparer.Ordinal);

            for (int i = 0;
                 i < array.arraySize;
                 i++)
            {
                SerializedProperty element =
                    array.GetArrayElementAtIndex(i);

                if (element == null)
                {
                    errors.Add(
                        $"{displayName} [{i}] is null.");

                    continue;
                }

                SerializedProperty id =
                    element.FindPropertyRelative("id");

                SerializedProperty reference =
                    element.FindPropertyRelative(
                        referencePropertyName);

                string idValue =
                    id.stringValue != null
                        ? id.stringValue.Trim()
                        : string.Empty;

                if (string.IsNullOrWhiteSpace(
                        idValue))
                {
                    errors.Add(
                        $"{displayName} [{i}] has an empty ID.");
                }
                else if (!ids.Add(idValue))
                {
                    errors.Add(
                        $"{displayName} [{i}] has duplicate ID " +
                        $"'{idValue}'.");
                }

                if (reference == null)
                {
                    errors.Add(
                        $"{displayName} '{idValue}' " +
                        $"has an invalid reference field.");

                    continue;
                }

                if (reference.objectReferenceValue == null)
                {
                    errors.Add(
                        $"{displayName} '{idValue}' " +
                        $"has no scene reference.");
                }
            }
        }

        // ============================================================
        // CONTEXT MENU
        // ============================================================

        [MenuItem(
            "CONTEXT/SparkLevelSceneBindings/Validate All Bindings")]
        private static void ValidateFromContext(
            MenuCommand command)
        {
            SparkLevelSceneBindings bindings =
                command.context as
                SparkLevelSceneBindings;

            if (bindings == null)
            {
                return;
            }

            if (bindings.Validate(out string error))
            {
                Debug.Log(
                    $"[{nameof(SparkLevelSceneBindings)}] " +
                    "All scene bindings are valid.",
                    bindings);

                return;
            }

            Debug.LogError(
                $"[{nameof(SparkLevelSceneBindings)}] " +
                $"Validation failed: {error}",
                bindings);
        }
    }
}

#endif
