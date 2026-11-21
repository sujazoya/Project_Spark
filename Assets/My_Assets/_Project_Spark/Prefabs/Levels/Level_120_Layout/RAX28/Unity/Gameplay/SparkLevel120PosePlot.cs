using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.Gameplay.AxisForge
{
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class SparkLevel120PosePlot : MaskableGraphic
    {
        private SparkLevel120Robot.Geometry measured;
        private Vector3 requested;
        private bool driving;

        internal void Present(SparkLevel120Robot.Geometry current, Vector3 target, bool active)
        {
            if (Vector3.Distance(measured.Tip.position, current.Tip.position) < .0001f &&
                Vector3.Distance(requested, target) < .0001f && driving == active) return;
            measured = current;
            requested = target;
            driving = active;
            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();
            Rect rect = GetPixelAdjustedRect();
            Vector2 Point(Vector3 point) => new(rect.xMin + rect.width * (.08f + (point.x - .21f * point.z) * .99f),
                rect.yMin + rect.height * (.10f + point.y * .93f));
            var basePoint = Point(Vector3.zero);
            var shoulder = Point(measured.Shoulder);
            var elbow = Point(measured.Elbow);
            var wrist = Point(measured.Wrist);
            var end = Point(measured.Tip.position);
            var goal = Point(requested);
            var steel = new Color(.30f, .57f, .66f);
            var axis = driving ? new Color(.48f, .90f, .95f) : new Color(.40f, .64f, .70f);
            Segment(mesh, new Vector2(rect.xMin + 3, basePoint.y), new Vector2(rect.xMax - 3, basePoint.y),
                .65f, new Color(.2f, .34f, .40f));
            Segment(mesh, basePoint, shoulder, 2.1f, steel);
            Segment(mesh, shoulder, elbow, 1.8f, axis);
            Segment(mesh, elbow, wrist, 1.7f, axis);
            Segment(mesh, wrist, end, 1.2f, new Color(.89f, .77f, .44f));
            Square(mesh, shoulder, 2.7f, steel);
            Square(mesh, elbow, 2.7f, steel);
            Square(mesh, wrist, 2.4f, steel);
            Square(mesh, end, 2.6f, Color.white);
            Segment(mesh, goal - Vector2.right * 4, goal + Vector2.right * 4, .55f, new Color(.94f, .67f, .31f));
            Segment(mesh, goal - Vector2.up * 4, goal + Vector2.up * 4, .55f, new Color(.94f, .67f, .31f));
        }

        private static void Square(VertexHelper mesh, Vector2 point, float size, Color color) =>
            Quad(mesh, point + new Vector2(-size, -size), point + new Vector2(size, -size),
                point + new Vector2(size, size), point + new Vector2(-size, size), color);

        private static void Segment(VertexHelper mesh, Vector2 a, Vector2 b, float width, Color color)
        {
            var direction = b - a;
            var normal = new Vector2(-direction.y, direction.x).normalized * width;
            Quad(mesh, a - normal, b - normal, b + normal, a + normal, color);
        }

        private static void Quad(VertexHelper mesh, Vector2 a, Vector2 b, Vector2 c, Vector2 d, Color color)
        {
            int i = mesh.currentVertCount;
            mesh.AddVert(a, color, Vector2.zero);
            mesh.AddVert(b, color, Vector2.zero);
            mesh.AddVert(c, color, Vector2.zero);
            mesh.AddVert(d, color, Vector2.zero);
            mesh.AddTriangle(i, i + 1, i + 2);
            mesh.AddTriangle(i, i + 2, i + 3);
        }
    }
}
