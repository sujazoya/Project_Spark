using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.Gameplay
{
    public sealed class SparkLevel73Trend : MaskableGraphic
    {
        private const int Capacity=120;
        private readonly Vector4[] traces=new Vector4[Capacity];
        private int count,cursor;
        private float time;
        private static readonly Color32[] Colors={new(56,218,250,255),new(255,186,60,255),new(121,242,154,255),new(194,156,240,255)};

        public void Sample(float dt,float a,float b,float product,float level)
        {
            time+=dt;
            if(time<.18f) return;
            time-=.18f;
            traces[cursor]=new Vector4(a/4,b/1.1f,product/105,level);
            cursor=(cursor+1)%Capacity;
            count=Mathf.Min(count+1,Capacity);
            SetVerticesDirty();
        }
        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();
            var r=rectTransform.rect;
            Vector2 P(float x,float y)=>new(r.xMin+x*r.width,r.yMin+y*r.height);
            Quad(mesh,P(0,0),P(0,1),P(1,1),P(1,0),new Color32(8,19,33,238));
            for(int i=1;i<5;i++) Line(mesh,P(.03f,i*.20f),P(.97f,i*.20f),.8f,new Color32(36,63,82,255));
            int start=(cursor-count+Capacity)%Capacity;
            for(int channel=0;channel<4;channel++)
                for(int i=1;i<count;i++)
                {
                    float a=Mathf.Clamp01(traces[(start+i-1)%Capacity][channel]);
                    float b=Mathf.Clamp01(traces[(start+i)%Capacity][channel]);
                    if(channel==0&&(a==0||b==0)) continue;
                    Line(mesh,P(.03f+.94f*(i-1)/(Capacity-1),.06f+.88f*a),P(.03f+.94f*i/(Capacity-1),.06f+.88f*b),1.15f,Colors[channel]);
                }
        }
        private static void Line(VertexHelper mesh,Vector2 a,Vector2 b,float width,Color32 color)
        {
            Vector2 delta=b-a, normal=new Vector2(-delta.y,delta.x).normalized*width*.5f;
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


