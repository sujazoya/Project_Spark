using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.Gameplay
{
    public sealed class SparkLevel57Plot : MaskableGraphic
    {
        private float voltage,delta;
        private bool phases;

        public void Show(float volts,float phaseDelta,bool showPhases)
        {
            voltage=volts;
            delta=phaseDelta;
            phases=showPhases;
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
            if (phases)
            {
                Color[] colors={cyan,new Color(1,.68f,.2f),new Color(.55f,.65f,1)};
                for(int phase=0;phase<3;phase++)
                {
                    Vector2 Point(int i)=>new(Mathf.Lerp(r.xMin,r.xMax,i/80f),
                        r.center.y+Mathf.Sin((i*4.5f-phase*120)*Mathf.Deg2Rad)*r.height*.40f*voltage/2.4f);
                    for(int i=1;i<=80;i++)Line(Point(i-1),Point(i),colors[phase],1.7f);
                }
            }
            else
            {
                var center=r.center;
                var direction=new Vector2(Mathf.Sin(delta*Mathf.Deg2Rad),Mathf.Cos(delta*Mathf.Deg2Rad));
                Line(center,center+Vector2.up*r.height*.42f,grid,2);
                Line(center,center+direction*r.height*.38f,cyan,3);
            }
        }
    }
}



