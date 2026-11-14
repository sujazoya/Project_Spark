using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.Gameplay
{
    public sealed class SparkLevel56Plot : MaskableGraphic
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


            Color cyan = new(.27f,.94f,1);


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

            Line(new(r.xMin,r.yMin+4),new(r.xMax,r.yMin+4),grid,1);
            Line(new(r.xMin,r.center.y),new(r.xMax,r.center.y),grid,1);
            Line(new(r.xMin,r.yMax-4),new(r.xMax,r.yMax-4),grid,1);
            if (valid && x != null)
            {
                void Trace(float[] values,Color tint)
                {
                    Vector2 Point(int sample)
                    {
                        int index=(cursor+sample)%values.Length;
                        return new(Mathf.Lerp(r.xMin,r.xMax,sample/(values.Length-1f)),
                            Mathf.Lerp(r.yMin+4,r.yMax-4,values[index]));
                    }
                    for (int i=1;i<values.Length;i++) Line(Point(i-1),Point(i),tint,1.6f);
                }
                Trace(x,cyan);
                Trace(y,new Color(1,.66f,.2f));
            }


        }
    }
}


