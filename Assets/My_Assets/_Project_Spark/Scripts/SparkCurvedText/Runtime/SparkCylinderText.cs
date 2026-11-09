using TMPro;
using UnityEngine;

namespace ProjectSpark.Labels
{
    /// <summary>Editable cylindrical labels for 3D TextMeshPro. Local units.</summary>
    [ExecuteAlways, DisallowMultipleComponent, RequireComponent(typeof(TextMeshPro))]
    [AddComponentMenu("Project Spark/Labels/Cylinder Text")]
    public sealed class SparkCylinderText : MonoBehaviour
    {
        public enum WrapDirection { AcrossHeight, AcrossWidth }

        [SerializeField] private WrapDirection wrap = WrapDirection.AcrossHeight;
        [SerializeField, Min(0.0001f)] private float radius = 0.5f;
        [SerializeField, Min(0f)] private float surfaceOffset = 0.001f;
        [SerializeField, Range(-180f, 180f)] private float angle;
        [SerializeField] private Vector2 layoutOffset;
        [SerializeField] private bool centerVisibleText = true;
        [Tooltip("Raises each glyph enough that its flat triangles do not cut into the cylinder. Recommended for solid models.")]
        [SerializeField] private bool preventSurfaceClipping = true;
        [SerializeField] private bool showGuides = true;

        private TextMeshPro label;
        private bool dirty = true;
        private bool rebuilding;
        public float Radius => radius;
        public WrapDirection Direction => wrap;
        public float SurfaceOffset => surfaceOffset;

        public void SetSurfaceOffset(float value)
        {
            surfaceOffset = Mathf.Max(0f, value);
            dirty = true;
        }

        public void SetDirection(WrapDirection value)
        {
            wrap = value;
            dirty = true;
        }
        public bool ShowGuides => showGuides;
        public float MaximumGlyphArc { get; private set; }

        private void OnEnable()
        {
            label = GetComponent<TextMeshPro>();
            label.OnPreRenderText -= Bend;
            label.OnPreRenderText += Bend;
            dirty = true;
        }

        private void OnDisable()
        {
            if (label == null) return;
            label.OnPreRenderText -= Bend;
            // Restore TMP's original geometry when the effect is disabled/removed.
            label.havePropertiesChanged = true;
            if (label.isActiveAndEnabled) label.ForceMeshUpdate();
        }

        private void OnValidate()
        {
            radius = Mathf.Max(0.0001f, radius);
            surfaceOffset = Mathf.Max(0f, surfaceOffset);
            dirty = true; // Do not rebuild meshes inside OnValidate.
        }

        private void Update()
        {
            if (dirty) Refresh();
        }

        public void SetRadius(float value)
        {
            radius = Mathf.Max(0.0001f, value);
            dirty = true;
        }

        public void SetAngle(float degrees)
        {
            angle = Mathf.Repeat(degrees + 180f, 360f) - 180f;
            dirty = true;
        }

        [ContextMenu("Refresh Curvature")]
        public void Refresh()
        {
            if (rebuilding || !isActiveAndEnabled) return;
            if (label == null) label = GetComponent<TextMeshPro>();
            if (label == null || !label.isActiveAndEnabled) return;
            rebuilding = true;
            try
            {
                dirty = false;
                label.havePropertiesChanged = true;
                label.ForceMeshUpdate();
            }
            finally { rebuilding = false; }
        }

        private void Bend(TMP_TextInfo info)
        {
            if (!isActiveAndEnabled) return;
            MaximumGlyphArc = 0f;
            bool height = wrap == WrapDirection.AcrossHeight;
            Vector2 min = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
            Vector2 max = new Vector2(float.NegativeInfinity, float.NegativeInfinity);
            bool found = false;
            for (int i = 0; i < info.characterCount; i++)
            {
                var c = info.characterInfo[i];
                if (!c.isVisible) continue;
                var vertices = info.meshInfo[c.materialReferenceIndex].vertices;
                for (int k = 0; k < 4; k++)
                {
                    Vector2 p = vertices[c.vertexIndex + k];
                    min = Vector2.Min(min, p);
                    max = Vector2.Max(max, p);
                }
                found = true;
            }
            if (!found) return;
            Vector2 center = centerVisibleText ? (min + max) * 0.5f : Vector2.zero;
            float r = Mathf.Max(0.0001f, radius);
            float rotation = angle * Mathf.Deg2Rad;
            for (int i = 0; i < info.characterCount; i++)
            {
                var c = info.characterInfo[i];
                if (!c.isVisible) continue;
                var vertices = info.meshInfo[c.materialReferenceIndex].vertices;
                float lo = float.PositiveInfinity, hi = float.NegativeInfinity;
                for (int k = 0; k < 4; k++)
                {
                    Vector3 p = vertices[c.vertexIndex + k];
                    float a = height ? p.y : p.x;
                    lo = Mathf.Min(lo, a);
                    hi = Mathf.Max(hi, a);
                }
                float arc = (hi - lo) / r;
                MaximumGlyphArc = Mathf.Max(MaximumGlyphArc, arc * Mathf.Rad2Deg);
                // TMP glyphs are quads. Compensate their chord depth for modest arcs.
                float lift = preventSurfaceClipping
                    ? 1f / Mathf.Max(0.5f, Mathf.Cos(Mathf.Min(arc * 0.5f, Mathf.PI / 3f)))
                    : 1f;
                for (int k = 0; k < 4; k++)
                {
                    int v = c.vertexIndex + k;
                    Vector3 p = vertices[v];
                    float x = p.x - center.x + layoutOffset.x;
                    float y = p.y - center.y + layoutOffset.y;
                    float theta = (height ? y : x) / r + rotation;
                    float shell = (r + surfaceOffset - p.z) * lift;
                    float around = Mathf.Sin(theta) * shell;
                    float depth = r - Mathf.Cos(theta) * shell;
                    vertices[v] = height ? new Vector3(x, around, depth) : new Vector3(around, y, depth);
                }
            }
            // TMP uploads these arrays after this callback. Never force a rebuild here.
        }
    }
}
