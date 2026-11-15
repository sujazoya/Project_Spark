using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.Gameplay
{
    public sealed class SparkLevel75Trend : MaskableGraphic
    {
        private readonly int[] states=new int[16];
        private readonly int[] doses=new int[16];
        private bool hasPosition;

        public void Show(SparkLevel75Plant plant,bool valid,int region)
        {
            bool redraw=hasPosition!=valid;
            hasPosition=valid;
            for(int i=0;i<16;i++)
            {
                int dose=Mathf.Clamp(Mathf.RoundToInt(plant.Dose[i]*.25f),0,25);
                int state=i==region?1:plant.Dose[i]>=97?4:plant.Dose[i]>.1f?2:0;
                redraw|=states[i]!=state||doses[i]!=dose;
                states[i]=state;doses[i]=dose;
            }
            if(redraw)SetVerticesDirty();
        }
        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();
            Rect rect=rectTransform.rect;
            void Tile(float x,float y,float w,float h,Color32 color)
            {
                int index=mesh.currentVertCount;
                float l=rect.xMin+x*rect.width, b=rect.yMin+y*rect.height;
                float r=l+w*rect.width, top=b+h*rect.height;
                mesh.AddVert(new Vector3(l,b),color,Vector2.zero);
                mesh.AddVert(new Vector3(l,top),color,Vector2.zero);
                mesh.AddVert(new Vector3(r,top),color,Vector2.zero);
                mesh.AddVert(new Vector3(r,b),color,Vector2.zero);
                mesh.AddTriangle(index,index+1,index+2);
                mesh.AddTriangle(index,index+2,index+3);
            }
            Tile(0,0,1,1,new Color32(8,20,33,242));
            Tile(.035f,.937f,.93f,.023f,hasPosition?new Color32(66,218,211,255):new Color32(238,93,82,255));
            for(int i=0;i<16;i++)
            {
                float x=.041f+(i%4)*.234f, y=.064f+(3-i/4)*.211f;
                Color32 color=states[i] switch
                {
                    1=>new Color32(226,172,69,255),
                    3=>new Color32(213,61,76,255),
                    2=>new Color32(54,151,203,255),
                    5=>new Color32(87,106,214,255),
                    4=>new Color32(52,190,145,255),
                    _=>new Color32(53,82,105,255)
                };
                Tile(x,y,.213f,.188f,color);
                Tile(x+.006f,y+.012f,.201f,.164f,new Color32((byte)(color.r/3),(byte)(color.g/3),(byte)(color.b/3),255));
                Tile(x+.014f,y+.035f,.184f*doses[i]/25,.035f,color);
                if(states[i]==4||states[i]==2)
                {
                    Tile(x+.024f,y+.111f,.063f,.018f,color);
                    Tile(x+.106f,y+.111f,.078f,.018f,color);
                }
            }
        }
    }
}
