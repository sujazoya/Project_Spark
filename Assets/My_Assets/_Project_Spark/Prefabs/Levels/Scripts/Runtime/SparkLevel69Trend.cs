using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.Gameplay
{
    public sealed class SparkLevel69Trend : MaskableGraphic
    {
        private readonly Vector2[] cycle=new Vector2[4];
        private bool valid;
        private float refresh;
        public void Sample(SparkLevel69ColdPlant plant,bool tempValid,bool pressureValid)
        {
            if(Time.unscaledTime<refresh) return;
            refresh=Time.unscaledTime+.12f;
            valid=tempValid&&pressureValid&&plant.MassFlow>.0005f;
            for(int i=0;i<4;i++)
            {
                float p=i==0||i==3?plant.SuctionPressure:plant.DischargePressure;
                cycle[i]=new Vector2(Mathf.InverseLerp(185,480,plant.Enthalpy[i]),Mathf.InverseLerp(Mathf.Log(90),Mathf.Log(2400),Mathf.Log(p)));
            }
            SetVerticesDirty();
        }
        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();
            var r=rectTransform.rect;
            Vector2 P(float x,float y)=>new(r.xMin+x*r.width,r.yMin+y*r.height);
            Box(mesh,P(0,0),P(1,1),new Color32(8,19,33,238));
            var grid=new Color32(31,68,88,255);
            for(int i=1;i<=5;i++)
            {
                float v=i*.15f;
                Line(mesh,P(.07f,v),P(.95f,v),.65f,grid);
                Line(mesh,P(v,.09f),P(v,.91f),.65f,grid);
            }
            Line(mesh,P(.07f,.09f),P(.95f,.09f),1.2f,grid);
            Line(mesh,P(.07f,.09f),P(.07f,.91f),1.2f,grid);
            if(!valid) return;
            for(int i=0;i<4;i++)
            {
                var a=P(.08f+.85f*cycle[i].x,.10f+.80f*cycle[i].y);
                var next=cycle[(i+1)%4];
                var b=P(.08f+.85f*next.x,.10f+.80f*next.y);
                var fill=i is 0 or 3?new Color32(65,208,248,255):new Color32(255,177,57,255);
                Line(mesh,a,b,2.15f,fill);
                Box(mesh,a-Vector2.one*3,a+Vector2.one*3,fill);
            }
        }
        private static void Box(VertexHelper mesh,Vector2 a,Vector2 b,Color32 color)
        {
            Quad(mesh,a,new Vector2(a.x,b.y),b,new Vector2(b.x,a.y),color);
        }
        private static void Line(VertexHelper mesh,Vector2 a,Vector2 b,float width,Color32 color)
        {
            Vector2 delta=b-a,normal=new Vector2(-delta.y,delta.x).normalized*width*.5f;
            Quad(mesh,a-normal,a+normal,b+normal,b-normal,color);
        }
        private static void Quad(VertexHelper mesh,Vector2 a,Vector2 b,Vector2 c,Vector2 d,Color32 color)
        {
            int n=mesh.currentVertCount;
            mesh.AddVert(a,color,Vector2.zero);mesh.AddVert(b,color,Vector2.zero);mesh.AddVert(c,color,Vector2.zero);mesh.AddVert(d,color,Vector2.zero);
            mesh.AddTriangle(n,n+1,n+2);mesh.AddTriangle(n,n+2,n+3);
        }
    }
}
