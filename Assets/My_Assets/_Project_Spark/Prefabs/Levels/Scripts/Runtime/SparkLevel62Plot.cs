using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.Gameplay
{
    public sealed class SparkLevel62Plot : MaskableGraphic
    {
        private readonly float[] input=new float[3], output=new float[3];
        private float refresh;
        public void Show(float[] before,float[] after)
        {
            if(Time.unscaledTime<refresh)return;
            refresh=Time.unscaledTime+.066f;
            for(int i=0;i<3;i++){input[i]=before[i];output[i]=after[i];}
            SetVerticesDirty();
        }
        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();
            var r=rectTransform.rect;
            Vector2 P(float x,float y)=>new(r.xMin+x*r.width,r.yMin+y*r.height);
            Quad(mesh,P(0,0),P(1,1),new Color32(8,19,33,238));
            for(int i=1;i<6;i++)Line(mesh,P(.04f+i*.15f,.11f),P(.04f+i*.15f,.91f),.7f,new Color32(38,67,87,255));
            var sourceTint=new Color32(255,179,85,240);
            var protectedTint=new Color32(73,235,225,255);
            for(int phase=0;phase<3;phase++)
            {
                float center=.23f+.27f*phase;
                int p=phase;
                Vector2 Trace(int i,float volts)=>P(.05f+i/84f*.90f,center+Mathf.Sin(i/84f*Mathf.PI*4-p*2*Mathf.PI/3)*volts*.105f);
                Line(mesh,P(.04f,center),P(.96f,center),1,new Color32(38,67,87,255));
                for(int i=1;i<=84;i++)
                {
                    Line(mesh,Trace(i-1,input[p]),Trace(i,input[p]),1.4f,sourceTint);
                    Line(mesh,Trace(i-1,output[p]),Trace(i,output[p]),1.7f,protectedTint);
                }
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

