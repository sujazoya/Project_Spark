using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.Gameplay.MemoryMatrix
{
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class SparkLevel104TrainingPlot : MaskableGraphic
    {
        private readonly float[] margins = new float[8];

        internal void SetWindows(SparkLevel104Memory.Window[] windows)
        {
            bool changed = false;
            for (int i = 0; i < windows.Length; i++)
            {
                int read = i * 2, write = read + 1;
                changed |= margins[read] != windows[i].ReadMargin || margins[write] != windows[i].WriteMargin;
                margins[read] = windows[i].ReadMargin;
                margins[write] = windows[i].WriteMargin;
            }
            if (changed) SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();
            Rect rect = GetPixelAdjustedRect();
            var grid = new Color(.24f, .35f, .46f);
            Fill(mesh, rect.xMin, rect.yMin, rect.xMax, rect.yMax, new Color(.014f, .035f, .055f));
            for (int i = 0; i <= 5; i++)
            {
                float x = Mathf.Lerp(rect.xMin, rect.xMax, i / 5f);
                Fill(mesh, x - .35f, rect.yMin, x + .35f, rect.yMax, grid);
            }
            float row = rect.height / 4f;
            for (int lane = 0; lane < 4; lane++)
            {
                float bottom = rect.yMax - row * (lane + 1);
                float read = rect.width * Mathf.Clamp01(margins[lane * 2] / 250f);
                float write = rect.width * Mathf.Clamp01(margins[lane * 2 + 1] / 250f);
                Fill(mesh, rect.xMin, bottom, rect.xMax, bottom + .45f, grid);
                if (read > 0) Fill(mesh, rect.xMin, bottom + row * .53f, rect.xMin + read, bottom + row * .86f, new Color(.34f, .84f, .99f));
                if (write > 0) Fill(mesh, rect.xMin, bottom + row * .13f, rect.xMin + write, bottom + row * .46f, new Color(.70f, .62f, 1f));
            }
            float boundary = Mathf.Lerp(rect.xMin, rect.xMax, 85f / 250f);
            Fill(mesh, boundary - .7f, rect.yMin, boundary + .7f, rect.yMax, new Color(1f, .67f, .36f, .8f));
        }

        private static void Fill(VertexHelper mesh, float l, float b, float r, float t, Color color)
        {
            int index = mesh.currentVertCount;
            mesh.AddVert(new Vector2(l, b), color, Vector2.zero);
            mesh.AddVert(new Vector2(r, b), color, Vector2.zero);
            mesh.AddVert(new Vector2(r, t), color, Vector2.zero);
            mesh.AddVert(new Vector2(l, t), color, Vector2.zero);
            mesh.AddTriangle(index, index + 1, index + 2);
            mesh.AddTriangle(index, index + 2, index + 3);
        }
    }
}
