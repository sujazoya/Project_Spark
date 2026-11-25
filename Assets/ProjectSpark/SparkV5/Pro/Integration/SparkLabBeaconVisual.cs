using TMPro;
using UnityEngine;

namespace ProjectSpark.SparkV5.Pro
{
    /// <summary>
    /// Presentation-only, noninteractive world-space markers. No collider, renderer override,
    /// circuit writes, mission writes, or scene-saved objects. Updated only while a cue is active.
    /// </summary>
    [DisallowMultipleComponent]
    internal sealed class SparkLabBeaconVisual : MonoBehaviour
    {
        [SerializeField] private float duration = 7f;
        [SerializeField] private float radius = .095f;
        private Transform primary, secondary, layout;
        private LineRenderer haloA, haloB, connector;
        private TextMeshPro label;
        private Material ownedMaterial;
        private Camera viewCamera;
        private float until;
        private bool active;

        internal bool IsShowing => active;

        private LineRenderer NewLine(string name, Color color, float width, bool loop)
        {
            var node = new GameObject(name);
            node.layer = 2; // Ignore Raycast
            node.transform.SetParent(transform, false);
            var line = node.AddComponent<LineRenderer>();
            line.sharedMaterial = ownedMaterial;
            line.useWorldSpace = true;
            line.loop = loop;
            line.alignment = LineAlignment.View;
            line.widthMultiplier = width;
            line.startColor = color;
            line.endColor = color;
            line.numCornerVertices = 3;
            line.numCapVertices = 3;
            line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            line.receiveShadows = false;
            line.enabled = false;
            return line;
        }

        private bool Initialize()
        {
            if (haloA) return true;
            Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (!shader) shader = Shader.Find("Sprites/Default");
            if (!shader)
            {
                Debug.LogWarning("[Spark Lab V6] No supported unlit line shader. 3D marker disabled.", this);
                return false;
            }
            ownedMaterial = new Material(shader) { name = "SparkLabBeacon_RuntimeOnly" };
            if (ownedMaterial.HasProperty("_BaseColor")) ownedMaterial.SetColor("_BaseColor", Color.white);
            if (ownedMaterial.HasProperty("_Color")) ownedMaterial.SetColor("_Color", Color.white);
            haloA = NewLine("Spark Target Halo", new Color(.10f, .95f, 1f, 1f), .012f, true);
            haloB = NewLine("Spark Partner Halo", new Color(1f, .72f, .23f, 1f), .008f, true);
            connector = NewLine("Spark Verified Endpoint Guide", new Color(.25f, .85f, 1f, .92f), .006f, false);
            var textNode = new GameObject("Spark Verified Target Label");
            textNode.layer = 2;
            textNode.transform.SetParent(transform, false);
            label = textNode.AddComponent<TextMeshPro>();
            label.fontSize = 2.1f;
            label.alignment = TextAlignmentOptions.Center;
            label.color = new Color(.64f, 1f, 1f, 1f);
            label.enableWordWrapping = false;
            label.transform.localScale = Vector3.one * .065f;
            label.gameObject.SetActive(false);
            return true;
        }

        internal bool Show(Transform target, Transform companion, Transform validLayout, string caption)
        {
            if (!target || !validLayout || !target.IsChildOf(validLayout) ||
                (companion && !companion.IsChildOf(validLayout))) return false;
            if (!Initialize()) return false;
            primary = target;
            secondary = companion;
            layout = validLayout;
            viewCamera = Camera.main;
            label.text = string.IsNullOrEmpty(caption) ? target.name : caption;
            until = Time.unscaledTime + Mathf.Clamp(duration, 1f, 30f);
            active = true;
            haloA.enabled = true;
            haloB.enabled = secondary;
            // A pair of component terminals is NOT proof that a wire belongs between them.
            // Never draw an apparent wiring route without independently verified topology.
            connector.enabled = false;
            label.gameObject.SetActive(true);
            enabled = true;
            Redraw();
            return true;
        }

        private void LateUpdate()
        {
            if (!active || !primary || !layout || !layout.gameObject.activeInHierarchy ||
                !primary.IsChildOf(layout) ||
                (secondary && !secondary.IsChildOf(layout)) || Time.unscaledTime >= until)
            {
                Clear();
                return;
            }
            Redraw();
        }

        private void DrawRing(LineRenderer ring, Vector3 center, float size, float phase)
        {
            if (!ring) return;
            const int segments = 48;
            ring.positionCount = segments;
            Vector3 right = viewCamera ? viewCamera.transform.right : Vector3.right;
            Vector3 up = viewCamera ? viewCamera.transform.up : Vector3.up;
            for (int i = 0; i < segments; i++)
            {
                float angle = (i * (Mathf.PI * 2f) / segments) + phase;
                ring.SetPosition(i, center + (right * Mathf.Cos(angle) + up * Mathf.Sin(angle)) * size);
            }
        }

        private void Redraw()
        {
            if (!primary) { Clear(); return; }
            if (!viewCamera) viewCamera = Camera.main;
            Vector3 a = primary.position;
            float pulse = 1f + .11f * Mathf.Sin(Time.unscaledTime * 5f);
            DrawRing(haloA, a, radius * pulse, 0f);
            if (secondary)
            {
                Vector3 b = secondary.position;
                DrawRing(haloB, b, radius * .8f, 0.4f);
                connector.positionCount = 2;
                connector.SetPosition(0, a);
                connector.SetPosition(1, b);
            }
            label.transform.position = a + (viewCamera ? viewCamera.transform.up : Vector3.up) * (radius * 1.6f);
            if (viewCamera) label.transform.rotation = viewCamera.transform.rotation;
        }

        internal void Clear()
        {
            active = false;
            primary = secondary = layout = null;
            if (haloA) haloA.enabled = false;
            if (haloB) haloB.enabled = false;
            if (connector) connector.enabled = false;
            if (label) label.gameObject.SetActive(false);
            enabled = false;
        }

        private void OnDestroy()
        {
            if (ownedMaterial) Destroy(ownedMaterial);
        }
    }
}
