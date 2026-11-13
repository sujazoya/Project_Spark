using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.Gameplay
{
    public sealed class SparkLevel54Plot : MaskableGraphic
    {
        private float[] x, y;
        private int cursor;
        private bool valid;
        private SparkLevel54Measurement.Reading[] baseline, damped;

        internal void Show(float[] xHistory, float[] yHistory, int writeCursor, bool validSamples,
            SparkLevel54Measurement.Reading[] baselineSweep, SparkLevel54Measurement.Reading[] dampedSweep)
        {
            x = xHistory;
            y = yHistory;
            cursor = writeCursor;
            valid = validSamples;
            baseline = baselineSweep;
            damped = dampedSweep;
            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper geometry)
        {
            geometry.Clear();
            Rect r = GetPixelAdjustedRect();
            Color grid = new(.19f,.37f,.47f,.8f);
            Color cyan = new(.26f,.94f,1f), amber = new(1f,.67f,.27f);
            Vector2 orbit = new(r.xMin + r.width * .22f, r.center.y);
            float radius = r.height * .42f;
            float left = r.xMin + r.width * .44f, right = r.xMax - 6, bottom = r.yMin + 12, top = r.yMax - 8;

            void Line(Vector2 a, Vector2 b, Color tint, float width)
            {
                if ((b-a).sqrMagnitude < .00001f) return;
                Vector2 side = Vector2.Perpendicular((b-a).normalized) * width / 2;
                int first = geometry.currentVertCount;
                geometry.AddVert(a-side,tint,Vector2.zero);
                geometry.AddVert(a+side,tint,Vector2.zero);
                geometry.AddVert(b+side,tint,Vector2.zero);
                geometry.AddVert(b-side,tint,Vector2.zero);
                geometry.AddTriangle(first,first+1,first+2);
                geometry.AddTriangle(first,first+2,first+3);
            }

            Line(orbit-Vector2.right*radius, orbit+Vector2.right*radius,grid,1);
            Line(orbit-Vector2.up*radius, orbit+Vector2.up*radius,grid,1);
            Line(new(left,bottom),new(right,bottom),grid,1);
            Line(new(left,bottom),new(left,top),grid,1);
            for (int i=1;i<=3;i++)
                Line(new(left,bottom+(top-bottom)*i/3),new(right,bottom+(top-bottom)*i/3),grid,1);
            if (valid && x != null)
            {
                Vector2 Point(int sample)
                {
                    int index = (cursor + sample) % x.Length;
                    return orbit + new Vector2(x[index],y[index]) * (radius / 2.55f);
                }
                for (int i=1;i<x.Length;i++) Line(Point(i-1),Point(i),cyan,1.5f);
            }

            void Trace(SparkLevel54Measurement.Reading[] trace,Color tint)
            {
                if (trace == null) return;
                Vector2 Point(int i) => new(Mathf.Lerp(left,right,(trace[i].Rpm-44)/30),
                    Mathf.Lerp(bottom,top,trace[i].RadialRms/2.55f));
                for (int i=1;i<trace.Length;i++)
                    if (trace[i-1].Rpm>0 && trace[i].Rpm>0) Line(Point(i-1),Point(i),tint,2);
            }

            Trace(baseline,amber);
            Trace(damped,cyan);
        }
    }
}
