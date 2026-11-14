using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.Gameplay
{
    public sealed class SparkLevel61Plot : MaskableGraphic
    {
        private static readonly int[] Orders={5,7,11,13};
        private readonly float[] raw=new float[4], clean=new float[4];
        private float fundamental;
        private float refresh;
        public void Show(float value,float[] before,float[] after)
        {
            if(Time.unscaledTime<refresh)return;
            refresh=Time.unscaledTime+.066f;
            fundamental=value;
            for(int i=0;i<4;i++){raw[i]=before[i];clean[i]=after[i];}
            SetVerticesDirty();
        }
        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();
            var r=rectTransform.rect;
            Vector2 P(float x,float y)=>new(r.xMin+x*r.width,r.yMin+y*r.height);
            Quad(mesh,P(0,0),P(1,1),new Color32(8,19,33,238));
            for(int i=1;i<6;i++)Line(mesh,P(.04f+i*.15f,.39f),P(.04f+i*.15f,.92f),.7f,new Color32(38,67,87,255));
            Line(mesh,P(.04f,.64f),P(.96f,.64f),1,new Color32(42,72,90,255));
            Vector2 Trace(int i,float[] data)
            {
                float theta=i/120f*Mathf.PI*4;
                float signal=fundamental*Mathf.Sin(theta);
                for(int h=0;h<Orders.Length;h++)signal+=data[h]*Mathf.Sin(theta*Orders[h]);
                return P(.04f+i/120f*.92f,.65f+Mathf.Clamp(signal/1.6f,-1,1)*.23f);
            }
            Color32 amber=new(255,184,91,240), cyan=new(71,235,229,255);
            for(int i=1;i<=120;i++)
            {
                Line(mesh,Trace(i-1,raw),Trace(i,raw),1.4f,amber);
                Line(mesh,Trace(i-1,clean),Trace(i,clean),1.8f,cyan);
            }
            for(int i=0;i<4;i++)
            {
                float x=.16f+i*.24f;
                Quad(mesh,P(x-.04f,.10f),P(x-.01f,.10f+Mathf.Min(.21f,raw[i]*.64f)),amber);
                Quad(mesh,P(x,.10f),P(x+.03f,.10f+Mathf.Min(.21f,clean[i]*.64f)),cyan);
            }
        }
        private static void Quad(VertexHelper mesh,Vector2 low,Vector2 high,Color32 color)
        {
            Add(mesh,low,new Vector2(low.x,high.y),high,new Vector2(high.x,low.y),color);
        }
        private static void Line(VertexHelper mesh,Vector2 a,Vector2 b,float width,Color32 color)
        {
            Vector2 delta=b-a;
            Vector2 normal=new Vector2(-delta.y,delta.x).normalized*width*.5f;
            Add(mesh,a-normal,a+normal,b+normal,b-normal,color);
        }
        private static void Add(VertexHelper mesh,Vector2 a,Vector2 b,Vector2 c,Vector2 d,Color32 color)
        {
            int n=mesh.currentVertCount;
            mesh.AddVert(a,color,Vector2.zero);mesh.AddVert(b,color,Vector2.zero);
            mesh.AddVert(c,color,Vector2.zero);mesh.AddVert(d,color,Vector2.zero);
            mesh.AddTriangle(n,n+1,n+2);mesh.AddTriangle(n,n+2,n+3);
        }
    }
}
