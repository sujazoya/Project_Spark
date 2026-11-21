using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.Gameplay.EmcGauntlet
{
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class SparkLevel112TrainingPlot : MaskableGraphic
    {
        private readonly float[] bands = new float[SparkLevel112Emissions.BandCount];
        private bool nearField, sampled;

        internal void SetSpectrum(SparkLevel112Emissions.Band[] source, bool field)
        {
            bool changed = nearField != field || sampled != (source != null);
            nearField = field;
            sampled = source != null;
            if (sampled)
                for (int i = 0; i < bands.Length; i++)
                {
                    float value = field ? source[i].RadiatedDb : source[i].ConductedDb;
                    changed |= value != bands[i];
                    bands[i] = value;
                }
            if (changed) SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();
            Rect rect = GetPixelAdjustedRect();
            var grid = new Color(.15f, .27f, .34f);
            Quad(mesh, rect.xMin, rect.yMin, rect.xMax, rect.yMax, new Color(.009f, .025f, .039f));
            for (int i = 0; i <= 5; i++)
            {
                float y = Mathf.Lerp(rect.yMin, rect.yMax, i * .2f);
                Quad(mesh, rect.xMin, y - .34f, rect.xMax, y + .34f, grid);
            }
            float binWidth = rect.width / bands.Length;
            float limit = nearField ? SparkLevel112Emissions.RadiatedLimit : SparkLevel112Emissions.ConductedLimit;
            var warning = new Color(1f, .65f, .32f);
            for (int i = 0; i < bands.Length; i++)
            {
                float cut = rect.xMin + i * binWidth;
                Quad(mesh, cut - .3f, rect.yMin, cut + .3f, rect.yMax, grid);
                if (!sampled) continue;
                var color = bands[i] + SparkLevel112Emissions.DetectorUncertainty >= limit ? warning :
                    nearField ? new Color(.68f, .57f, 1f) : new Color(.31f, .85f, .96f);
                float top = Mathf.Lerp(rect.yMin, rect.yMax, Mathf.InverseLerp(10f, 60f, bands[i]));
                Quad(mesh, cut + 6f, rect.yMin, cut + binWidth - 6f, top, color);
            }
            float boundary = Mathf.Lerp(rect.yMin, rect.yMax, Mathf.InverseLerp(10f, 60f, limit));
            Quad(mesh, rect.xMin, boundary - .65f, rect.xMax, boundary + .65f, warning);
        }

        private static void Quad(VertexHelper mesh, float left, float bottom, float right, float top, Color color)
        {
            int index = mesh.currentVertCount;
            mesh.AddVert(new Vector2(left, bottom), color, Vector2.zero);
            mesh.AddVert(new Vector2(left, top), color, Vector2.zero);
            mesh.AddVert(new Vector2(right, top), color, Vector2.zero);
            mesh.AddVert(new Vector2(right, bottom), color, Vector2.zero);
            mesh.AddTriangle(index, index + 1, index + 2);
            mesh.AddTriangle(index, index + 2, index + 3);
        }
    }
}
