using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.Gameplay
{
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class SparkLevel99LogicTrace : MaskableGraphic
    {
        private static readonly Color[] Tints = { new(.42f, .90f, .91f), new(.91f, .71f, .37f), new(.62f, .95f, .49f) };
        private readonly byte[] samples = new byte[90];
        private int next, count;

        public void Add(bool timer, bool reset, bool heartbeat)
        {
            samples[next] = (byte)((timer ? 1 : 0) | (reset ? 2 : 0) | (heartbeat ? 4 : 0));
            next = (next + 1) % samples.Length;
            count = Mathf.Min(count + 1, samples.Length);
            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();
            var bounds = GetPixelAdjustedRect();
            RectFill(mesh, bounds.xMin, bounds.yMin, bounds.xMax, bounds.yMax, new Color(.012f, .035f, .047f));
            for (int trace = 0; trace < Tints.Length; trace++)
            {
                float baseline = bounds.yMin + bounds.height * (.08f + (2 - trace) * .31f);
                RectFill(mesh, bounds.xMin, baseline - .5f, bounds.xMax, baseline + .5f, new Color(.12f, .24f, .29f));
                float x = bounds.xMin + 3f;
                float previousY = baseline;
                float spacing = (bounds.width - 6f) / samples.Length;
                for (int i = 0; i < count; i++)
                {
                    int index = (next - count + i + samples.Length) % samples.Length;
                    float y = baseline + ((samples[index] & (1 << trace)) != 0 ? bounds.height * .175f : 0);
                    if (i > 0 && previousY != y)
                        RectFill(mesh, x - .7f, Mathf.Min(previousY, y), x + .7f, Mathf.Max(previousY, y), Tints[trace]);
                    RectFill(mesh, x, y - .57f, x + spacing, y + .57f, Tints[trace]);
                    previousY = y;
                    x += spacing;
                }
            }
        }

        private static void RectFill(VertexHelper mesh, float left, float bottom, float right, float top, Color tint)
        {
            int index = mesh.currentVertCount;
            mesh.AddVert(new Vector2(left, bottom), tint, Vector2.zero);
            mesh.AddVert(new Vector2(right, bottom), tint, Vector2.zero);
            mesh.AddVert(new Vector2(right, top), tint, Vector2.zero);
            mesh.AddVert(new Vector2(left, top), tint, Vector2.zero);
            mesh.AddTriangle(index, index + 1, index + 2);
            mesh.AddTriangle(index, index + 2, index + 3);
        }
    }
}
