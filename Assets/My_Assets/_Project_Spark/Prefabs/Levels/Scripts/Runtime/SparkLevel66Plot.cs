using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.Gameplay
{
    public sealed class SparkLevel66Plot : MaskableGraphic
    {
        private readonly float[] samples=new float[128];
        private float refresh;
        private bool valid;
        public void Show(float[] bins,bool available)
        {
            if(Time.unscaledTime<refresh)return;
            refresh=Time.unscaledTime+.08f;
            valid=available;
            System.Array.Copy(bins,samples,samples.Length);
            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();
            var r=rectTransform.rect;
            Vector2 P(float x,float y)=>new(r.xMin+x*r.width,r.yMin+y*r.height);
            Quad(mesh,P(0,0),P(1,1),new Color32(8,19,33,238));
            for(int i=1;i<5;i++)Line(mesh,P(.05f,.12f+i*.16f),P(.96f,.12f+i*.16f),.8f,new Color32(38,67,87,255));
            if(!valid)return;
            for(int i=1;i<samples.Length;i++)
            {
                float x=.05f+.91f*i/samples.Length;
                float height=Mathf.Clamp01(samples[i]/1.1f)*.78f;
                if(height>.006f)Quad(mesh,P(x,.11f),P(x+.005f,.11f+height),new Color32(73,235,225,255));
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



