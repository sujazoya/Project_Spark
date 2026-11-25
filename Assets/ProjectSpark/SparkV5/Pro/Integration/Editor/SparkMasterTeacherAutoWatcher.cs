
#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.SparkV5.Pro.Editor
{
    /// <summary>
    /// Automatically updates generated, ungraded teaching guides
    /// when the authored objective source changes.
    /// </summary>
    internal sealed class SparkMasterTeacherAutoWatcher : AssetPostprocessor
    {
        private const string MenuPath =
            "Project Spark/Spark V5 Pro/Master Teacher/8 — Auto Update Guides on Source Changes [Toggle]";

        private const string Flag =
            "ProjectSpark.MasterTeacher.AutoGuidesOnChanges.v2";

        private static bool pending;

        [MenuItem(MenuPath)]
        public static void Toggle()
        {
            bool enabled = !EditorPrefs.GetBool(Flag, true);

            EditorPrefs.SetBool(Flag, enabled);

            UnityEditor.Menu.SetChecked(MenuPath, enabled);

            Debug.Log(
                "[Spark Auto Teacher] Updates on authored objective changes " +
                (enabled ? "enabled" : "disabled"));
        }

        [MenuItem(MenuPath, true)]
        private static bool ValidateToggle()
        {
            UnityEditor.Menu.SetChecked(
                MenuPath,
                EditorPrefs.GetBool(Flag, true));

            return true;
        }

        private static void OnPostprocessAllAssets(
            string[] imported,
            string[] deleted,
            string[] moved,
            string[] movedFrom)
        {
            if (pending ||
                !EditorPrefs.GetBool(Flag, true) ||
                EditorApplication.isPlayingOrWillChangePlaymode ||
                imported == null ||
                !imported.Contains(SparkMasterTeacherImporter.SourcePath))
            {
                return;
            }

            pending = true;

            EditorApplication.delayCall += BuildOnNextSafeEditorTick;
        }

        private static void BuildOnNextSafeEditorTick()
        {
            if (EditorApplication.isCompiling ||
                EditorApplication.isUpdating ||
                EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorApplication.delayCall += BuildOnNextSafeEditorTick;
                return;
            }

            pending = false;

            try
            {
                SparkMasterTeacherAutoGenerator.OneClick();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }
}
#endif
