#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEngine;
using ProjectSpark.Labels;

namespace ProjectSpark.Labels.Editor
{
    /// <summary>
    /// Surface-fit editable TMP lettering on actual cylindrical mesh body.
    /// Uses mesh-local bounds, body world transform and SceneView camera.
    /// Never edits the physical mesh, electronic component, terminal, or solver.
    /// </summary>
    public static class SparkPremiumComponentLabelCreator
    {
        private static readonly Color WarmWhite = new Color(.92f, .89f, .77f, 1f);
        private static readonly Color SoftGold  = new Color(1f, .76f, .27f, 1f);
        private static readonly Color IceCyan   = new Color(.46f, .92f, 1f, 1f);
        private static readonly Color BlackInk  = new Color(.045f, .045f, .05f, 1f);

        [MenuItem("Tools/Project Spark/Labels/Create Diode Body Print", false, 20)]
        private static void Diode() => Create("1N4007", WarmWhite, .40f, .008f);
        [MenuItem("Tools/Project Spark/Labels/Create Resistor Body Print", false, 21)]
        private static void Resistor() => Create("220Ω", BlackInk, .43f, .008f);
        [MenuItem("Tools/Project Spark/Labels/Create Capacitor Body Print", false, 22)]
        private static void Capacitor() => Create("4700µF 16V", WarmWhite, .35f, .008f);
        [MenuItem("Tools/Project Spark/Labels/Create Premium Gold Print", false, 23)]
        private static void Gold() => Create("PROJECT SPARK", SoftGold, .30f, .008f);
        [MenuItem("Tools/Project Spark/Labels/Create Neon Print", false, 24)]
        private static void Cyan() => Create("1N5819", IceCyan, .40f, .008f);

        [MenuItem("Tools/Project Spark/Labels/Auto Fit Selected Label", false, 35)]
        private static void FitSelected()
        {
            GameObject selected = Selection.activeGameObject;
            SparkCylinderText curve = selected != null ? selected.GetComponent<SparkCylinderText>() : null;
            if (curve == null)
            {
                EditorUtility.DisplayDialog("Project Spark Labels", "Select the label object containing SparkCylinderText.", "OK");
                return;
            }
            MeshFilter body = FindBody(curve.transform.parent != null ? curve.transform.parent.gameObject : null);
            if (body == null) { WarnNoMesh(); return; }
            TextMeshPro text = curve.GetComponent<TextMeshPro>();
            Undo.RecordObject(text, "Auto fit component label");
            Fit(curve.transform, curve, body);
            SetTextSize(text, body.sharedMesh.bounds, curve.Radius, .40f);
            curve.Refresh();
            EditorUtility.SetDirty(curve);
            EditorUtility.SetDirty(text);
            SceneView.RepaintAll();
        }

        [MenuItem("Tools/Project Spark/Labels/Auto Fit Selected Label", true)]
        private static bool CanFitSelected() => Selection.activeGameObject != null &&
            Selection.activeGameObject.GetComponent<SparkCylinderText>() != null;

        private static void Create(string value, Color color, float heightFraction, float offsetFraction)
        {
            GameObject selected = Selection.activeGameObject;
            MeshFilter body = FindBody(selected);
            if (body == null) { WarnNoMesh(); return; }

            GameObject go = new GameObject("Print_" + value.Replace(' ', '_'));
            Undo.RegisterCreatedObjectUndo(go, "Create Project Spark body print");
            Undo.SetTransformParent(go.transform, body.transform, "Parent body print");

            TextMeshPro text = Undo.AddComponent<TextMeshPro>(go);
            text.text = value;
            text.alignment = TextAlignmentOptions.Center;
            text.enableWordWrapping = false;
            text.overflowMode = TextOverflowModes.Overflow;
            text.color = color;
            text.fontStyle = FontStyles.Bold;
            text.raycastTarget = false;
            text.isTextObjectScaleStatic = true;

            SparkCylinderText curve = Undo.AddComponent<SparkCylinderText>(go);
            Fit(go.transform, curve, body);
            curve.SetSurfaceOffset(curve.Radius * offsetFraction);
            curve.SetDirection(SparkCylinderText.WrapDirection.AcrossHeight);
            SetTextSize(text, body.sharedMesh.bounds, curve.Radius, heightFraction);
            curve.Refresh();
            EditorUtility.SetDirty(go);
            Selection.activeGameObject = go;
            EditorGUIUtility.PingObject(go);
            SceneView.RepaintAll();
            Debug.Log("[Spark Labels] Surface-fitted " + go.name + " onto " + body.name +
                      ". If another side is desired, rotate Scene view and choose Auto Fit Selected Label.", go);
        }

        private static void SetTextSize(TextMeshPro text, Bounds bounds, float radius, float heightFraction)
        {
            Vector3 size = bounds.size;
            float length = Mathf.Max(size.x, Mathf.Max(size.y, size.z));
            float usableLength = Mathf.Max(.0001f, length * .72f); // Keep clear of rings and endcaps.
            float maxHeight = Mathf.Max(.0001f, radius * 2f * heightFraction);
            text.rectTransform.sizeDelta = new Vector2(usableLength, Mathf.Max(radius * 2f, maxHeight * 2f));
            text.fontSize = maxHeight;
            // Measure flat TMP glyph extents before curvature; scale typography to fit the band-free body.
            for (int attempt = 0; attempt < 10; attempt++)
            {
                Vector2 preferred = text.GetPreferredValues(text.text);
                if (preferred.x <= usableLength && preferred.y <= maxHeight * 1.18f) break;
                float widthFit = usableLength / Mathf.Max(preferred.x, .00001f);
                float heightFit = maxHeight * 1.18f / Mathf.Max(preferred.y, .00001f);
                text.fontSize = Mathf.Max(.00001f, text.fontSize * Mathf.Min(.90f, widthFit, heightFit));
            }
        }

        private static MeshFilter FindBody(GameObject root)
        {
            if (root == null) return null;
            MeshFilter own = root.GetComponent<MeshFilter>();
            if (own != null && own.sharedMesh != null) return own;
            MeshFilter[] meshes = root.GetComponentsInChildren<MeshFilter>(true);
            MeshFilter best = null;
            float scoreBest = float.NegativeInfinity;
            foreach (MeshFilter mf in meshes)
            {
                if (mf == null || mf.sharedMesh == null) continue;
                string name = mf.name.ToLowerInvariant();
                Bounds b = mf.sharedMesh.bounds;
                Vector3 s = b.size;
                float longest = Mathf.Max(s.x, Mathf.Max(s.y, s.z));
                float smallest = Mathf.Min(s.x, Mathf.Min(s.y, s.z));
                float score = (name.Contains("body") ? 1000f : 0f) -
                              (name.Contains("strip") || name.Contains("lead") || name.Contains("wire") ? 2000f : 0f) +
                              (longest > 0f ? smallest / longest : 0f) + b.size.sqrMagnitude;
                if (score > scoreBest) { best = mf; scoreBest = score; }
            }
            return best;
        }

        private static void WarnNoMesh() => EditorUtility.DisplayDialog(
            "Project Spark Labels", "Select Diode/Body, Resistor/Body or another cylindrical MeshFilter, then create a print.", "OK");

        private static void Fit(Transform label, SparkCylinderText curve, MeshFilter body)
        {
            if (label.parent != body.transform)
                Undo.SetTransformParent(label, body.transform, "Fit component print");
            Bounds bounds = body.sharedMesh.bounds;
            Vector3 ext = bounds.extents;
            Vector3 size = bounds.size;
            // Mesh-local cylinder axis. X is always the label's text-reading direction.
            Vector3 axis = size.x >= size.y && size.x >= size.z ? Vector3.right :
                           size.y >= size.x && size.y >= size.z ? Vector3.up : Vector3.forward;
            Vector3 outward = Vector3.zero;
            SceneView scene = SceneView.lastActiveSceneView;
            if (scene != null && scene.camera != null)
            {
                Vector3 worldCenter = body.transform.TransformPoint(bounds.center);
                Vector3 cameraOutward = scene.camera.transform.position - worldCenter;
                outward = body.transform.InverseTransformDirection(cameraOutward);
                // Oblique and nonuniformly scaled objects need projected camera vector.
                outward -= axis * Vector3.Dot(outward, axis);
            }
            if (outward.sqrMagnitude < .000001f)
            {
                outward = axis == Vector3.up ? Vector3.forward : Vector3.up;
                outward -= axis * Vector3.Dot(outward, axis);
            }
            outward.Normalize();

            // Cross-section ray vs. mesh-bounds ellipsoid to support body axis X/Y/Z,
            // translated mesh pivots and slightly non-circular component bodies.
            float x = outward.x / Mathf.Max(ext.x, .00001f);
            float y = outward.y / Mathf.Max(ext.y, .00001f);
            float z = outward.z / Mathf.Max(ext.z, .00001f);
            float radius = 1f / Mathf.Sqrt(x*x + y*y + z*z);
            radius = Mathf.Max(.0001f, radius);

            Vector3 inward = -outward;
            Vector3 localUp = Vector3.Cross(inward, axis).normalized;
            Quaternion rotation = Quaternion.LookRotation(inward, localUp);

            Undo.RecordObject(label, "Align print to cylinder surface");
            Undo.RecordObject(curve, "Fit print curvature");
            label.localPosition = bounds.center + outward * radius;
            label.localRotation = rotation;
            label.localScale = Vector3.one;
            curve.SetRadius(radius);
            curve.SetAngle(0f);
            curve.SetDirection(SparkCylinderText.WrapDirection.AcrossHeight);
            PrefabUtility.RecordPrefabInstancePropertyModifications(label);
            PrefabUtility.RecordPrefabInstancePropertyModifications(curve);
        }
    }
}
#endif
