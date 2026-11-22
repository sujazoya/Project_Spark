using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.Gameplay.AutonomySentinel
{
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class SparkLevel122TrainingPlot : MaskableGraphic
    {
        private const int Samples = SparkLevel122Navigation.FrameCount;
        private static readonly Color[] Traces =
        {
            new(1f, .65f, .34f), new(.37f, .87f, 1f), new(.78f, .63f, 1f), new(.53f, .95f, .68f)
        };
        private readonly float[,] values = new float[4, Samples];
        private int count;

        internal void ResetTrace()
        {
            count = 0;
            SetVerticesDirty();
        }

        internal void Record(float first, float second, float third, float fourth)
        {
            if (count >= Samples) return;
            values[0, count] = first;
            values[1, count] = second;
            values[2, count] = third;
            values[3, count] = fourth;
            count++;
            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();
            Rect rect = GetPixelAdjustedRect();
            var grid = new Color(.17f, .30f, .38f);
            Fill(mesh, rect.xMin, rect.yMin, rect.xMax, rect.yMax, new Color(.012f, .030f, .050f));
            Fill(mesh, Timeline(16), rect.yMin, Timeline(48), rect.yMax, new Color(.024f, .051f, .073f));
            for (int i = 0; i <= 8; i++)
            {
                float x = Timeline(i * 8);
                Fill(mesh, x - .35f, rect.yMin, x + .35f, rect.yMax, grid);
            }
            for (int i = 0; i <= 4; i++)
            {
                float y = Mathf.Lerp(rect.yMin, rect.yMax, i / 4f);
                Fill(mesh, rect.xMin, y - .34f, rect.xMax, y + .34f, grid);
            }
            if (count < 2) return;
            for (int series = 0; series < Traces.Length; series++)
            {
                Vector2 previous = Point(series, 0);
                for (int index = 1; index < count; index++)
                {
                    Vector2 next = Point(series, index);
                    Vector2 rise = next - previous;
                    var offset = new Vector2(-rise.y, rise.x).normalized * .67f;
                    Quad(mesh, previous - offset, next - offset, next + offset, previous + offset, Traces[series]);
                    previous = next;
                }
            }

            float Timeline(float frame) => Mathf.Lerp(rect.xMin, rect.xMax, frame / Samples);
            Vector2 Point(int series, int index) => new(Timeline(index + .5f),
                Mathf.Lerp(rect.yMin, rect.yMax, Mathf.Clamp01(values[series, index])));
        }

        private static void Fill(VertexHelper mesh, float left, float bottom, float right, float top, Color color) =>
            Quad(mesh, new Vector2(left, bottom), new Vector2(right, bottom), new Vector2(right, top), new Vector2(left, top), color);

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
