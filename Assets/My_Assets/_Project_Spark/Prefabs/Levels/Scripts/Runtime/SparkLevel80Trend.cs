using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.Gameplay
{
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class SparkLevel80Trend : MaskableGraphic
    {
        private readonly int[] opening=new int[4],noise=new int[4];
        private bool fresh,traffic;
        public void Show(SparkLevel80Plant plant,bool valid,bool running)
        {
            bool changed=fresh!=valid||traffic!=running;
            fresh=valid;traffic=running;
            for(int i=0;i<4;i++)
            {
                int eye=Mathf.RoundToInt(plant.Eye[i]*30), jitter=Mathf.RoundToInt(plant.Jitter[i]*30);
                changed|=eye!=opening[i]||jitter!=noise[i];
                opening[i]=eye;noise[i]=jitter;
            }
            if(changed)SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();
            Rect rect=rectTransform.rect;
            Vector2 Point(float x,float y)=>new Vector2(rect.xMin+x*rect.width,rect.yMin+y*rect.height);
            void Quad(Vector2 a,Vector2 b,Vector2 c,Vector2 d,Color32 color)
            {
                int i=mesh.currentVertCount;
                mesh.AddVert(a,color,Vector2.zero);mesh.AddVert(b,color,Vector2.zero);
                mesh.AddVert(c,color,Vector2.zero);mesh.AddVert(d,color,Vector2.zero);
                mesh.AddTriangle(i,i+1,i+2);mesh.AddTriangle(i,i+2,i+3);
            }
            void Tile(float x,float y,float w,float h,Color32 color)=>Quad(Point(x,y),Point(x,y+h),Point(x+w,y+h),Point(x+w,y),color);
            void Segment(Vector2 a,Vector2 b,Color32 color)
            {
                Vector2 normal=new Vector2(b.y-a.y,a.x-b.x).normalized*.56f;
                Quad(a-normal,a+normal,b+normal,b-normal,color);
            }
            Tile(0,0,1,1,new Color32(8,19,32,242));
            Tile(.035f,.955f,.93f,.026f,fresh?new Color32(49,199,191,255):new Color32(237,96,71,255));
            for(int channel=0;channel<4;channel++)
            {
                float x=.027f+(channel%2)*.486f,y=.054f+(1-channel/2)*.453f;
                Tile(x,y,.46f,.405f,new Color32(16,37,54,255));
                Color32 tint=channel switch
                {
                    0=>new Color32(98,232,255,235),1=>new Color32(175,139,252,235),
                    2=>new Color32(113,237,163,235),_=>new Color32(252,187,91,235)
                };
                Tile(x+.020f,y+.022f,.041f,.021f,tint);
                Tile(x+.076f,y+.024f,.366f*opening[channel]/30,.016f,tint);
                if(!fresh||!traffic||opening[channel]<=0)continue;
                float aperture=opening[channel]/30f, jitter=noise[channel]/30f;
                for(int from=0;from<4;from++)
                for(int to=0;to<4;to++)
                {
                    Vector2 previous=Vector2.zero;
                    for(int j=0;j<=20;j++)
                    {
                        float t=j*.05f;
                        float transition=Mathf.Clamp01((t-.18f-jitter*(from-to)*.11f)/.64f);
                        transition=transition*transition*(3-2*transition);
                        float height=Mathf.Lerp(from,to,transition)/3;
                        Vector2 current=Point(x+.034f+t*.39f,y+.09f+.235f*height+
                            Mathf.Sin(t*21+from+to)*(1-aperture)*.020f);
                        if(j>0)Segment(previous,current,tint);
                        previous=current;
                    }
                }
            }
        }
    }
}
