#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.EditorTools
{
    /// <summary>Structural ART validation; cannot confirm functional Unity gameplay.</summary>
    public static class SparkLevel84AssetValidatorEditor
    {
        private const string Menu = "Tools/Project Spark/Levels/84/Validate Imported HD Art";

        [MenuItem(Menu)]
        public static void Validate()
        {
            Transform selected = Selection.activeTransform;
            if (selected == null)
            {
                EditorUtility.DisplayDialog("Level 84 Art Validator", "Select L84_HD_VISUAL_ART or its parent layout.", "OK");
                return;
            }
            Transform root = selected.name == "L84_HD_VISUAL_ART" ? selected : selected.Find("L84_HD_VISUAL_ART");
            if (root == null)
            {
                EditorUtility.DisplayDialog("Level 84 Art Validator", "L84_HD_VISUAL_ART was not found under selection.", "OK");
                return;
            }
            var pivotNames = new HashSet<string>();
            var socketNames = new HashSet<string>();
            int duplicatePivots = 0, duplicateSockets = 0;
            foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
            {
                if (t.name.StartsWith("ANIM_", StringComparison.Ordinal) && !pivotNames.Add(t.name)) duplicatePivots++;
                if (t.name.StartsWith("SOCKET_P", StringComparison.Ordinal) && !socketNames.Add(t.name)) duplicateSockets++;
            }
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            Bounds b = new Bounds(); bool validBounds = false;
            foreach (Renderer r in renderers)
            {
                if (!validBounds) { b = r.bounds; validBounds = true; }
                else b.Encapsulate(r.bounds);
            }
            bool ok = root.childCount == 76 && pivotNames.Count == 58 && socketNames.Count == 218 && duplicatePivots == 0 && duplicateSockets == 0 && validBounds;
            if (validBounds && Mathf.Abs(b.size.x - 27.0f * root.lossyScale.x) > 2.0f * Mathf.Max(1f, root.lossyScale.x)) ok = false;
            string report = "Models = " + root.childCount + "/76\n" +
                            "Distinct ANIM_ pivots = " + pivotNames.Count + "/58\n" +
                            "Distinct SOCKET_P anchors = " + socketNames.Count + "/218\n" +
                            "Duplicate pivots/sockets = " + duplicatePivots + "/" + duplicateSockets + "\n" +
                            "Renderers = " + renderers.Length + "\n" +
                            "World width = " + (validBounds ? b.size.x.ToString("F3") : "no geometry") + " m\n" +
                            "This is not a Unity compilation, electrical topology or Play Mode gameplay test.";
            Debug.Log("[Level84 Art Validator] " + (ok ? "ART STRUCTURE PASS" : "ART STRUCTURE CHECK REQUIRED") + "\n" + report);
            EditorUtility.DisplayDialog("Level 84 Art Validation", report, "OK");
        }
        [MenuItem(Menu, true)]
        private static bool SelectionValid() { return Selection.activeTransform != null; }
    }
}
#endif
