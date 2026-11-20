using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.Gameplay
{
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class SparkLevel82Trend : MaskableGraphic
    {
        private readonly float[] polar=new float[12],cond=new float[16],baseline=new float[16],heat=new float[16];
        private float field;
        private int count;
        private bool valid,fault;

        public void Show(float[] radiation,int poses,float[] trace,float[] original,float[] nearH,float[] nearE,float measured,
            bool trusted,bool latched)
        {
            bool dirty=count!=poses||valid!=trusted||fault!=latched||Mathf.Abs(field-measured)>.015f;
            count=poses;valid=trusted;fault=latched;
            if(dirty)field=measured;
            for(int i=0;i<12;i++)
            {
                float peak=0;
                for(int group=0;group<6;group++)
                {
                    int j=group*12+(group%2==0?i:11-i);
                    if(j<poses)peak=Mathf.Max(peak,radiation[j]);
                }
                dirty|=polar[i]!=peak;polar[i]=peak;
            }
            for(int i=0;i<16;i++)
            {
                float hotspot=Mathf.Max(nearH[i],nearE[i]);
                dirty|=cond[i]!=trace[i]||heat[i]!=hotspot||baseline[i]!=original[i];
                cond[i]=trace[i];heat[i]=hotspot;baseline[i]=original[i];
            }
            if(dirty)SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();Rect r=rectTransform.rect;
            Vector2 Point(float x,float y)=>new Vector2(r.xMin+x*r.width,r.yMin+y*r.height);
            void Quad(Vector2 a,Vector2 b,Vector2 c,Vector2 d,Color32 color)
            {
                int i=mesh.currentVertCount;
                mesh.AddVert(a,color,Vector2.zero);mesh.AddVert(b,color,Vector2.zero);
                mesh.AddVert(c,color,Vector2.zero);mesh.AddVert(d,color,Vector2.zero);
                mesh.AddTriangle(i,i+1,i+2);mesh.AddTriangle(i,i+2,i+3);
            }
            void Tile(float x,float y,float w,float h,Color32 color)=>Quad(Point(x,y),Point(x,y+h),Point(x+w,y+h),Point(x+w,y),color);
            void Line(Vector2 first,Vector2 second,Color32 color)
            {
                Vector2 n=new Vector2(second.y-first.y,first.x-second.x).normalized*.75f;
                Quad(first-n,first+n,second+n,second-n,color);
            }
            var mint=new Color32(54,224,197,255);var gray=new Color32(47,80,99,255);
            var amber=new Color32(245,158,67,255);var red=new Color32(237,84,71,255);
            Tile(0,0,1,1,new Color32(8,19,32,243));
            Tile(.035f,.937f,.93f,.018f,fault?red:valid?mint:amber);
            for(int k=0;k<12;k++)
            {
                int next=(k+1)%12;
                Vector2 Radial(int a,float radius)
                {
                    float angle=a*.5235988f;
                    return Point(.16f+Mathf.Cos(angle)*radius*.123f,.46f+Mathf.Sin(angle)*radius*.224f);
                }
                Line(Radial(k,1),Radial(next,1),gray);
                if(count>0)Line(Radial(k,Mathf.Clamp01(polar[k]/.70f)),Radial(next,Mathf.Clamp01(polar[next]/.70f)),mint);
            }
            for(int i=0;i<16;i++)
            {
                float x=.32f+i*.0165f;
                Tile(x,.215f,.011f,baseline[i]*.71f,gray);
                Tile(x,.215f,.007f,cond[i]*.71f,mint);
                int col=i%4,row=i/4;
                if(row%2!=0)col=3-col;
                Tile(.60f+col*.049f,.215f+(3-row)*.11f,.046f,.103f,
                    heat[i]>.36f?red:heat[i]>.19f?amber:heat[i]>.06f?mint:gray);
            }
            Tile(.82f,.215f,.117f,.45f,gray);
            Tile(.831f,.232f,.095f,(valid?field:0)*.47f,fault?red:mint);
        }
    }
}
