#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using ProjectSpark.Gameplay;

namespace ProjectSpark.Gameplay.Editor
{
    /// <summary>
    /// Converts the SELECTED EXISTING Wire Controller panel to a compact
    /// vertical popup. Moves existing Buttons (does not recreate them),
    /// preserving all their onClick listeners and references.
    /// </summary>
    public static class SparkVerticalWireControllerEditor
    {
        private const string MenuPath =
            "Tools/Project Spark/UI/Make Selected Wire Controller Vertical";

        [MenuItem(MenuPath, true)]
        private static bool ValidateSelection()
        {
            return Selection.activeGameObject != null &&
                   Selection.activeGameObject.GetComponent<RectTransform>() != null &&
                   Selection.activeGameObject.GetComponentInParent<Canvas>() != null;
        }

        [MenuItem(MenuPath)]
        private static void Build()
        {
            GameObject panel = Selection.activeGameObject;
            RectTransform rect = panel.GetComponent<RectTransform>();
            Canvas canvas = panel.GetComponentInParent<Canvas>();

            if (EditorUtility.IsPersistent(panel))
            {
                EditorUtility.DisplayDialog("Wire Controller",
                    "Select the wire panel in the SCENE Hierarchy, not the prefab asset.", "OK");
                return;
            }

            Button[] all = panel.GetComponentsInChildren<Button>(true);
            Button undoAll = FindButton(all, "undoall", "undo all");
            Button undoLast = FindButton(all, "undolast", "undo last");
            Button clear = FindButton(all, "clearwires", "clear wires", "clearall", "clear all");
            if (undoAll == null || undoLast == null || clear == null ||
                undoAll == undoLast || undoAll == clear || undoLast == clear)
            {
                EditorUtility.DisplayDialog("Wire Controller",
                    "Could not find the three existing buttons inside the selected panel. " +
                    "Their GameObject names or label text must contain Undo All, Undo Last, and Clear Wires. " +
                    "Nothing was changed.", "OK");
                return;
            }

            Transform previousContent = panel.transform.Find("WireToolbar_Content");
            if (previousContent != null)
            {
                EditorUtility.DisplayDialog("Wire Controller",
                    "WireToolbar_Content already exists. The toolbar has already been built. " +
                    "No changes made (avoids duplicating UI).", "OK");
                return;
            }

            Undo.RegisterCompleteObjectUndo(rect, "Compact wire panel placement");
            rect.anchorMin = new Vector2(1f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(1f, 1f);
            rect.anchoredPosition = new Vector2(-18f, -90f);
            rect.sizeDelta = new Vector2(116f, 204f);
            rect.localScale = Vector3.one;

            Image background = panel.GetComponent<Image>();
            if (background == null) background = Undo.AddComponent<Image>(panel);
            Undo.RecordObject(background, "Compact wire panel background");
            background.color = new Color(0.055f, 0.14f, 0.21f, 0.94f);
            background.raycastTarget = false;

            // Preserve the actual button components and their UnityEvents.
            // Old decorative children are only deactivated, not deleted.
            GameObject content = CreateUI("WireToolbar_Content", panel.transform);
            RectTransform contentRect = content.GetComponent<RectTransform>();
            contentRect.anchorMin = Vector2.zero;
            contentRect.anchorMax = Vector2.one;
            contentRect.offsetMin = Vector2.zero;
            contentRect.offsetMax = Vector2.zero;

            VerticalLayoutGroup group = Undo.AddComponent<VerticalLayoutGroup>(content);
            group.padding = new RectOffset(7, 7, 8, 8);
            group.spacing = 5f;
            group.childAlignment = TextAnchor.UpperCenter;
            group.childControlHeight = true;
            group.childControlWidth = true;
            group.childForceExpandHeight = false;
            group.childForceExpandWidth = true;

            GameObject header = CreateUI("WireToolbar_Header", content.transform);
            LayoutElement headerLayout = Undo.AddComponent<LayoutElement>(header);
            headerLayout.preferredHeight = 25f;
            headerLayout.flexibleHeight = 0f;
            Text title = Undo.AddComponent<Text>(header);
            title.text = "WIRE CTRL";
            title.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            title.fontSize = 12;
            title.fontStyle = FontStyle.Bold;
            title.alignment = TextAnchor.MiddleCenter;
            title.color = new Color(0.43f, 0.91f, 1f, 1f);
            title.raycastTarget = false;

            ConfigureExistingButton(undoAll, content.transform, "Undo All", new Color(0.13f, 0.30f, 0.42f, 1f));
            ConfigureExistingButton(undoLast, content.transform, "Undo Last", new Color(0.13f, 0.30f, 0.42f, 1f));
            ConfigureExistingButton(clear, content.transform, "Clear Wires", new Color(0.43f, 0.20f, 0.22f, 1f));

            // Any former labels/backgrounds remain preserved but inactive.
            List<GameObject> oldChildren = new List<GameObject>();
            foreach (Transform child in panel.transform)
                if (child != content.transform) oldChildren.Add(child.gameObject);
            foreach (GameObject old in oldChildren)
            {
                Undo.RecordObject(old, "Hide previous wire panel visuals");
                old.SetActive(false);
            }

            // Place controller OUTSIDE panel so SetActive(false) never stops it.
            SparkWireControllerPopup controller = canvas.GetComponentInChildren<SparkWireControllerPopup>(true);
            if (controller == null)
            {
                GameObject controllerGO = new GameObject("WirePopupVisibilityController");
                Undo.RegisterCreatedObjectUndo(controllerGO, "Create wire visibility controller");
                Undo.SetTransformParent(controllerGO.transform, canvas.transform, "Parent wire controller");
                controller = Undo.AddComponent<SparkWireControllerPopup>(controllerGO);
            }
            if (controller.transform == panel.transform || controller.transform.IsChildOf(panel.transform))
            {
                Undo.SetTransformParent(controller.transform, canvas.transform, "Move controller outside popup");
            }

            SerializedObject controllerSO = new SerializedObject(controller);
            controllerSO.FindProperty("popupPanel").objectReferenceValue = panel;
            SerializedProperty roots = controllerSO.FindProperty("wireVisualRoots");
            if (roots != null && roots.arraySize == 0)
            {
                Transform wireRoot = FindSceneObjectNamed("Wires");
                if (wireRoot != null)
                {
                    roots.arraySize = 1;
                    roots.GetArrayElementAtIndex(0).objectReferenceValue = wireRoot;
                }
            }
            SerializedProperty camera = controllerSO.FindProperty("gameplayCamera");
            if (camera != null && camera.objectReferenceValue == null && Camera.main != null)
                camera.objectReferenceValue = Camera.main;
            controllerSO.ApplyModifiedProperties();

            EditorUtility.SetDirty(panel);
            EditorUtility.SetDirty(controller);
            if (panel.scene.IsValid()) EditorSceneManager.MarkSceneDirty(panel.scene);
            Selection.activeGameObject = panel;

            bool hasRoot = roots != null && roots.arraySize > 0;
            Debug.Log("[Project Spark] Compact vertical wire panel created. " +
                "Existing button onClick listeners preserved. " +
                (hasRoot ? "Wire root assigned." : "IMPORTANT: Assign Wire Visual Roots on WirePopupVisibilityController."),
                panel);
        }

        private static Button FindButton(Button[] buttons, params string[] names)
        {
            foreach (Button b in buttons)
            {
                if (b == null) continue;
                string ownName = Normalize(b.gameObject.name);
                foreach (string name in names)
                    if (ownName.Contains(Normalize(name))) return b;

                foreach (Text t in b.GetComponentsInChildren<Text>(true))
                    foreach (string name in names)
                        if (Normalize(t.text).Contains(Normalize(name))) return b;

                // TMP UI is optional at runtime, so detect labels by reflected type
                // without hard-requiring a particular TextMeshPro assembly in this Editor.
                foreach (Component component in b.GetComponentsInChildren<Component>(true))
                {
                    if (component == null || component.GetType().FullName != "TMPro.TextMeshProUGUI")
                        continue;
                    System.Reflection.PropertyInfo prop = component.GetType().GetProperty("text");
                    string label = prop != null ? prop.GetValue(component, null) as string : null;
                    foreach (string name in names)
                        if (Normalize(label).Contains(Normalize(name))) return b;
                }
            }
            return null;
        }

        private static string Normalize(string s)
        {
            if (string.IsNullOrEmpty(s)) return string.Empty;
            return s.ToLowerInvariant().Replace(" ", "").Replace("_", "").Replace("-", "");
        }

        private static void ConfigureExistingButton(Button button, Transform content,
            string label, Color background)
        {
            Undo.SetTransformParent(button.transform, content, "Move existing wire button");
            Undo.RecordObject(button.gameObject, "Enable wire button");
            button.gameObject.SetActive(true);

            RectTransform rect = button.GetComponent<RectTransform>();
            if (rect != null)
            {
                Undo.RecordObject(rect, "Set wire button size");
                rect.localScale = Vector3.one;
            }
            LayoutElement layout = button.GetComponent<LayoutElement>();
            if (layout == null) layout = Undo.AddComponent<LayoutElement>(button.gameObject);
            Undo.RecordObject(layout, "Compact wire button layout");
            layout.minHeight = 44f;
            layout.preferredHeight = 44f;
            layout.flexibleHeight = 0f;

            Image image = button.GetComponent<Image>();
            if (image != null)
            {
                Undo.RecordObject(image, "Wire button color");
                image.color = background;
            }
            foreach (Text t in button.GetComponentsInChildren<Text>(true))
            {
                Undo.RecordObject(t, "Wire button label");
                t.text = label;
                t.fontSize = 12;
                t.alignment = TextAnchor.MiddleCenter;
                t.color = Color.white;
            }
            foreach (Component component in button.GetComponentsInChildren<Component>(true))
            {
                if (component == null || component.GetType().FullName != "TMPro.TextMeshProUGUI")
                    continue;
                Undo.RecordObject(component, "Wire button TMP label");
                Type type = component.GetType();
                type.GetProperty("text")?.SetValue(component, label, null);
                type.GetProperty("fontSize")?.SetValue(component, 12f, null);
            }
        }

        private static GameObject CreateUI(string name, Transform parent)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            Undo.RegisterCreatedObjectUndo(go, "Create " + name);
            Undo.SetTransformParent(go.transform, parent, "Parent " + name);
            return go;
        }

        private static Transform FindSceneObjectNamed(string name)
        {
            foreach (Transform t in UnityEngine.Object.FindObjectsOfType<Transform>(true))
            {
                if (t.gameObject.scene.IsValid() && t.name == name)
                    return t;
            }
            return null;
        }
    }
}
#endif
