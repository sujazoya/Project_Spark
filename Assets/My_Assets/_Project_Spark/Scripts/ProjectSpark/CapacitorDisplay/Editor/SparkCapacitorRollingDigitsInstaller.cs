#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using TMPro;
using ProjectSpark.Gameplay;

namespace ProjectSpark.EditorTools
{
    /// <summary>Non-destructive rolling-number UI upgrade for the generated capacitor display.</summary>
    public static class SparkCapacitorRollingDigitsInstaller
    {
        private const string Menu = "Tools/Project Spark/Electrical/Capacitor Display/";
        private const string AnimationName = "Spark_RollingDigits";
        private const string GhostName = "Spark_RollingDigits_Ghost";

        [MenuItem(Menu + "Install Sci-Fi Rolling Numbers")]
        public static void Install()
        {
            GameObject selected = Selection.activeGameObject;
            if (selected == null || EditorUtility.IsPersistent(selected))
            {
                EditorUtility.DisplayDialog("Project Spark", "Select the CAPACITOR GAMEOBJECT or its generated screen hierarchy in the Scene/Prefab Stage.", "OK");
                return;
            }

            SparkCapacitorDigitalScreen screen = selected.GetComponentInParent<SparkCapacitorDigitalScreen>();
            if (screen == null) screen = selected.GetComponentInChildren<SparkCapacitorDigitalScreen>(true);
            if (screen == null)
            {
                EditorUtility.DisplayDialog("Project Spark", "Cannot find SparkCapacitorDigitalScreen beneath or above the selection. Build the capacitor UI first.", "OK");
                return;
            }

            SerializedObject screenSO = new SerializedObject(screen);
            SerializedProperty prop = screenSO.FindProperty("valueText");
            TMP_Text source = prop != null ? prop.objectReferenceValue as TMP_Text : null;
            if (source == null)
            {
                EditorUtility.DisplayDialog("Project Spark", "The capacitor screen's valueText reference is not assigned. Reassign Major Value in the Inspector first.", "OK");
                return;
            }
            if (!(source is TextMeshProUGUI))
            {
                EditorUtility.DisplayDialog("Project Spark", "This installer expects the generated TextMeshProUGUI 'Major Value' text.", "OK");
                return;
            }
            if (source.transform.parent == null) return;
            Transform parent = source.transform.parent;
            Transform existing = parent.Find(AnimationName);
            if (existing != null)
            {
                EditorUtility.DisplayDialog("Project Spark", "Rolling numbers are already installed. Use Remove Sci-Fi Rolling Numbers first to rebuild.", "OK");
                Selection.activeGameObject = existing.gameObject;
                return;
            }
            Undo.RecordObject(source, "Hide raw numeric data under rolling digits");
            Color original = source.color;
            Color hidden = original;
            hidden.a = 0f;
            source.color = hidden;

            GameObject mainGO = Object.Instantiate(source.gameObject, parent);
            mainGO.name = AnimationName;
            Undo.RegisterCreatedObjectUndo(mainGO, "Create capacitor rolling display");
            mainGO.transform.SetAsLastSibling();
            var main = mainGO.GetComponent<TextMeshProUGUI>();
            main.color = original;
            main.raycastTarget = false;

            GameObject ghostGO = Object.Instantiate(source.gameObject, parent);
            ghostGO.name = GhostName;
            Undo.RegisterCreatedObjectUndo(ghostGO, "Create capacitor rolling digit trail");
            ghostGO.transform.SetSiblingIndex(mainGO.transform.GetSiblingIndex());
            var ghost = ghostGO.GetComponent<TextMeshProUGUI>();
            ghost.color = Color.clear;
            ghost.raycastTarget = false;
            var anim = Undo.AddComponent<SparkCapacitorRollingDigits>(mainGO);
            anim.Configure(source, main, ghost, original);
            EditorUtility.SetDirty(source);
            EditorUtility.SetDirty(anim);
            Selection.activeGameObject = mainGO;
            Debug.Log("[CAP ROLLING] Installed count-up / slide animation. Existing SparkCapacitorDigitalScreen still writes original valueText; only the duplicate is visible.", anim);
        }

        [MenuItem(Menu + "Remove Sci-Fi Rolling Numbers")]
        public static void Remove()
        {
            GameObject selected = Selection.activeGameObject;
            if (selected == null) return;
            SparkCapacitorRollingDigits anim = selected.GetComponentInParent<SparkCapacitorRollingDigits>();
            if (anim == null) anim = selected.GetComponentInChildren<SparkCapacitorRollingDigits>(true);
            if (anim == null)
            {
                var screen = selected.GetComponentInParent<SparkCapacitorDigitalScreen>();
                if (screen == null) screen = selected.GetComponentInChildren<SparkCapacitorDigitalScreen>(true);
                if (screen != null) anim = screen.GetComponentInChildren<SparkCapacitorRollingDigits>(true);
            }
            if (anim == null) { Debug.LogWarning("[CAP ROLLING] No installed rolling numbers found."); return; }
            Undo.RecordObject(anim, "Restore source numeric color");
            anim.RestoreSourceColor();
            Transform parent = anim.transform.parent;
            if (parent != null)
            {
                Transform ghost = parent.Find(GhostName);
                if (ghost != null) Undo.DestroyObjectImmediate(ghost.gameObject);
            }
            Undo.DestroyObjectImmediate(anim.gameObject);
            Debug.Log("[CAP ROLLING] Original numeric display restored.");
        }
    }
}
#endif
