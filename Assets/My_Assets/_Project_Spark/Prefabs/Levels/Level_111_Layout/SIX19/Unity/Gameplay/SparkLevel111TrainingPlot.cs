using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.Gameplay.SignalCrucible
{
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class SparkLevel111TrainingPlot : MaskableGraphic
    {
        private readonly float[] heights = new float[4], widths = new float[4];
        private float reflection, echo;
        private bool tdr;

        internal void SetEyes(SparkLevel111SignalModel.Snapshot[] measurements)
        {
            bool changed = tdr;
            tdr = false;
            for (int i = 0; i < heights.Length; i++)
            {
                changed |= heights[i] != measurements[i].EyeMv || widths[i] != measurements[i].EyePs;
                heights[i] = measurements[i].EyeMv;
                widths[i] = measurements[i].EyePs;
            }
            if (changed) SetVerticesDirty();
        }

        internal void SetReflection(float loss, float distanceMm)
        {
            float newEcho = 2f * distanceMm / SparkLevel111SignalModel.PropagationMmPerPs;
            float newReflection = Mathf.Pow(10f, -loss / 20f);
            bool changed = !tdr || newEcho != echo || newReflection != reflection;
            tdr = true;
            echo = newEcho;
            reflection = newReflection;
            if (changed) SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();
            Rect rect = GetPixelAdjustedRect();
            var grid = new Color(.17f, .29f, .37f);
            Fill(mesh, rect.xMin, rect.yMin, rect.xMax, rect.yMax, new Color(.012f, .030f, .050f));
            for (int row = 1; row < 4; row++)
            {
                float y = Mathf.Lerp(rect.yMin, rect.yMax, row / 4f);
                Fill(mesh, rect.xMin, y - .36f, rect.xMax, y + .36f, grid);
            }
            int columns = tdr ? 10 : 4;
            for (int col = 0; col <= columns; col++)
            {
                float x = Mathf.Lerp(rect.xMin, rect.xMax, col / (float)columns);
                Fill(mesh, x - .37f, rect.yMin, x + .37f, rect.yMax, grid);
            }
            if (tdr) DrawTdr(mesh, rect);
            else DrawEyes(mesh, rect);
        }

        private void DrawTdr(VertexHelper mesh, Rect rect)
        {
            if (echo <= 0f) return;
            Vector2 previous = new(rect.xMin, rect.center.y);
            var color = reflection > .20f ? new Color(1f, .68f, .38f) : new Color(.37f, .92f, 1f);
            for (int i = 0; i <= 96; i++)
            {
                float at = i / 96f;
                float launch = Mathf.Exp(-Mathf.Pow((at * 1000f - 60f) / 23f, 2f)) * .56f;
                float returned = Mathf.Exp(-Mathf.Pow((at * 1000f - echo) / 31f, 2f)) * reflection * 1.6f;
                var next = new Vector2(Mathf.Lerp(rect.xMin, rect.xMax, at), rect.center.y + (launch + returned) * rect.height * .5f);
                Stroke(mesh, previous, next, 1.2f, color);
                previous = next;
            }
        }

        private void DrawEyes(VertexHelper mesh, Rect rect)
        {
            float laneWidth = rect.width / 4f;
            for (int lane = 0; lane < heights.Length; lane++)
            {
                if (heights[lane] <= 0f) continue;
                float eyeHeight = Mathf.Clamp01(heights[lane] / 680f) * rect.height * .43f;
                float transition = Mathf.Max(.18f, (1f - widths[lane] / 100f) * .7f);
                float x = rect.xMin + lane * laneWidth + 4f;
                Vector2 lastHigh = new(x, rect.center.y), lastLow = lastHigh;
                Color high = heights[lane] >= 310f && widths[lane] >= 45f ? new Color(.36f, .85f, 1f) : new Color(1f, .61f, .34f);
                Color low = new(.64f, .57f, 1f);
                for (int i = 1; i <= 24; i++)
                {
                    float u = i / 24f;
                    float y = Mathf.SmoothStep(0f, eyeHeight, Mathf.Min(u, 1f - u) / transition);
                    float nextX = x + (laneWidth - 8f) * u;
                    Vector2 nextHigh = new(nextX, rect.center.y + y), nextLow = new(nextX, rect.center.y - y);
                    Stroke(mesh, lastHigh, nextHigh, 1.3f, high);
                    Stroke(mesh, lastLow, nextLow, 1.3f, low);
                    lastHigh = nextHigh;
                    lastLow = nextLow;
                }
            }
        }

        private static void Stroke(VertexHelper mesh, Vector2 start, Vector2 end, float width, Color color)
        {
            Vector2 direction = end - start;
            if (direction.sqrMagnitude < .0001f) return;
            var normal = new Vector2(-direction.y, direction.x).normalized * width * .5f;
            Quad(mesh, start - normal, end - normal, end + normal, start + normal, color);
        }

        private static void Fill(VertexHelper mesh, float l, float b, float r, float t, Color color) =>
            Quad(mesh, new Vector2(l, b), new Vector2(r, b), new Vector2(r, t), new Vector2(l, t), color);

        private static void Quad(VertexHelper mesh, Vector2 a, Vector2 b, Vector2 c, Vector2 d, Color color)
        {
            int index = mesh.currentVertCount;
            mesh.AddVert(a, color, Vector2.zero);
            mesh.AddVert(b, color, Vector2.zero);
            mesh.AddVert(c, color, Vector2.zero);
            mesh.AddVert(d, color, Vector2.zero);
            mesh.AddTriangle(index, index + 1, index + 2);
            mesh.AddTriangle(index, index + 2, index + 3);
        }
    }
}
