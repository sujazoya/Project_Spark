using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.Gameplay
{
    public sealed class SparkLevel55Plot : MaskableGraphic
    {
        private float[] x, y;
        private int cursor;
        private bool valid;
        internal void Show(float[] xHistory,float[] yHistory,int writeCursor,bool validSamples)
        {
            x = xHistory;
            y = yHistory;
            cursor = writeCursor;
            valid = validSamples;


            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper geometry)
        {
            geometry.Clear();
            Rect r = GetPixelAdjustedRect();
            Color grid = new(.19f,.37f,.47f,.8f);


            Vector2 orbit = r.center;
            Color cyan = new(.27f,.94f,1);
            float radius = r.height*.45f;


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
            if (valid && x != null)
            {
                Vector2 Point(int sample)
                {
                    int index = (cursor + sample) % x.Length;
                    return orbit + new Vector2(x[index],y[index]) * (radius / 1.20f);
                }
                for (int i=1;i<x.Length;i++) Line(Point(i-1),Point(i),cyan,1.5f);
            }


        }
    }
}

