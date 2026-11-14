using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.Gameplay
{
    public sealed class SparkLevel63Plot : MaskableGraphic
    {
        private readonly float[] hz=new float[160], slope=new float[160];
        private float refresh;
        private int next,count;
        public void Show(float frequency,float rocof)
        {
            if(Time.unscaledTime<refresh)return;
            refresh=Time.unscaledTime+.045f;
            hz[next]=frequency;
            slope[next]=rocof;
            next=(next+1)%hz.Length;
            count=Mathf.Min(count+1,hz.Length);
            SetVerticesDirty();
        }
        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();
            var r=rectTransform.rect;
            Vector2 P(float x,float y)=>new(r.xMin+x*r.width,r.yMin+y*r.height);
            Quad(mesh,P(0,0),P(1,1),new Color32(8,19,33,238));
            for(int i=1;i<6;i++)Line(mesh,P(.04f+i*.15f,.08f),P(.04f+i*.15f,.93f),.7f,new Color32(38,67,87,255));
            Line(mesh,P(.04f,.72f),P(.96f,.72f),1,new Color32(38,67,87,255));
            Line(mesh,P(.04f,.47f),P(.96f,.47f),1,new Color32(171,72,82,255));
            int Index(int i)=>(next-count+i+hz.Length)%hz.Length;
            Vector2 Point(int i,bool rate)=>P(.05f+i/(float)(hz.Length-1)*.90f,rate?
                .21f+Mathf.Clamp(slope[Index(i)],-1.5f,1.5f)*.07f:
                .72f+Mathf.Clamp(hz[Index(i)]-50,-.75f,.35f)*.50f);
            for(int i=1;i<count;i++)
            {
                Line(mesh,Point(i-1,false),Point(i,false),1.8f,new Color32(73,235,225,255));
                Line(mesh,Point(i-1,true),Point(i,true),1.4f,new Color32(255,179,85,240));
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


