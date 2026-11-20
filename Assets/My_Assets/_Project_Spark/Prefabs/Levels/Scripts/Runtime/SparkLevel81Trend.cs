using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.Gameplay
{
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class SparkLevel81Trend : MaskableGraphic
    {
        private bool ecc,vote,watch,packet;
        private int phase,hold,scan;
        public void Show(SparkLevel81Plant p,bool valid,int current,float physicalHold)
        {
            int latch=Mathf.RoundToInt(physicalHold*32), head=(p.Inspected%256)/8;
            bool change=ecc!=valid||vote!=p.VoterValid||watch!=p.Watchdog||packet!=(p.LastPacket==SparkLevel81Plant.Packet.Accepted)||
                phase!=current||hold!=latch||scan!=head;
            ecc=valid;vote=p.VoterValid;watch=p.Watchdog;packet=p.LastPacket==SparkLevel81Plant.Packet.Accepted;
            phase=current;hold=latch;scan=head;
            if(change)SetVerticesDirty();
        }
        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();Rect r=rectTransform.rect;
            void Tile(float x,float y,float w,float h,Color32 color)
            {
                int i=mesh.currentVertCount;
                float a=r.xMin+x*r.width,b=r.yMin+y*r.height,c=a+w*r.width,d=b+h*r.height;
                mesh.AddVert(new Vector3(a,b),color,Vector2.zero);mesh.AddVert(new Vector3(a,d),color,Vector2.zero);
                mesh.AddVert(new Vector3(c,d),color,Vector2.zero);mesh.AddVert(new Vector3(c,b),color,Vector2.zero);
                mesh.AddTriangle(i,i+1,i+2);mesh.AddTriangle(i,i+2,i+3);
            }
            var cyan=new Color32(83,212,231,255);
            var amber=new Color32(237,165,84,255);
            Tile(0,0,1,1,new Color32(8,19,32,243));
            for(int i=0;i<4;i++)
            {
                bool on=i switch{0=>ecc,1=>vote,2=>watch,_=>packet};
                float x=.04f+i*.24f;
                Tile(x,.48f,.21f,.34f,new Color32(26,54,76,255));
                Tile(x+.02f,.55f,.17f,.035f,on?cyan:amber);
                Tile(x+.02f,.65f,.17f*(i==0?scan/32f:on?1:.18f),.072f,on?cyan:amber);
            }
            for(int i=0;i<3;i++)Tile(.04f+i*.315f,.21f,.28f,.10f,phase>i?cyan:new Color32(34,61,80,255));
            Tile(.04f,.081f,.91f*hold/32f,.046f,new Color32(229,89,77,255));
        }
    }
}
