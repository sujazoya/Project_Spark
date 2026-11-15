using UnityEngine;

namespace ProjectSpark.Gameplay
{
    public sealed class SparkLevel74VisualRig : MonoBehaviour
    {
        public Transform art,beam,carried;
        public Transform[] pivots;
        private readonly Vector3[] seats=new Vector3[42];
        private readonly Quaternion[] rotation=new Quaternion[42];
        private float lastTime, spin, pump, developer, dryer;

        private void Awake()
        {
            if(art==null||pivots==null||pivots.Length!=42){enabled=false;return;}
            for(int i=0;i<42;i++){seats[i]=pivots[i].localPosition;rotation[i]=pivots[i].localRotation;}
        }
        private void Move(int i,Vector3 displacement)
        {
            var pivot=pivots[i];
            pivot.localPosition=seats[i]+pivot.parent.InverseTransformVector(art.TransformVector(displacement));
        }
        private void Turn(int i,float angle,Vector3 axis)=>
            pivots[i].localRotation=rotation[i]*Quaternion.AngleAxis(angle,axis);

        public void Apply(SparkLevel74Plant plant,float now,float transfer,int mode,bool loaded,bool onStage,bool reticle,bool emergency)
        {
            if(!enabled)return;
            float dt=Mathf.Min(.12f,Mathf.Max(0,now-lastTime));
            lastTime=now;
            spin=Mathf.Repeat(spin+plant.Spin*2.4f*dt,360);
            pump=Mathf.Repeat(pump+plant.Flow*315*dt,360);
            developer=Mathf.Repeat(developer+(mode==4?365:0)*dt,360);
            dryer=Mathf.Repeat(dryer+(mode==5||mode==6?plant.Spin*2.5f:0)*dt,360);
            Turn(0,mode==1?spin:0,Vector3.up);
            Turn(1,mode==1?spin:0,Vector3.up);
            Turn(2,pump,Vector3.right);
            Turn(3,32*plant.Flow,Vector3.up);
            Move(4,Vector3.down*(mode==1?.010f:0));
            Move(5,Vector3.up*(mode==2?.015f:0));
            float travel=transfer>0?Mathf.Sin(Mathf.Clamp01(transfer/1.7f)*Mathf.PI):0;
            Turn(6,33*travel,Vector3.up);
            Turn(7,-15*travel,Vector3.forward);
            Turn(8,21*travel,Vector3.forward);
            Turn(9,loaded&&!onStage?5*travel:0,Vector3.up);
            Turn(10,28*plant.Lamp,Vector3.forward);
            Move(11,Vector3.right*(.020f*plant.Shutter));
            float sx=Mathf.Clamp((float)plant.X*.0000010f,-.036f,.036f);
            float sy=Mathf.Clamp((float)plant.Y*.0000010f,-.026f,.026f);
            float rx=Mathf.Clamp((float)plant.ReticleX*.00000036f,-.050f,.050f);
            float ry=Mathf.Clamp((float)plant.ReticleY*.00000036f,-.032f,.032f);
            Move(12,Vector3.right*rx);
            Move(13,Vector3.forward*ry);
            Move(14,Vector3.right*rx);
            Move(15,Vector3.down*Mathf.Clamp((float)plant.Focus*.000035f,-.01f,.01f));
            Turn(16,(plant.Aperture-.65f)*190,Vector3.up);
            Move(17,Vector3.right*sx);
            Move(18,Vector3.forward*sy);
            Move(19,Vector3.right*sx*.55f);
            Move(20,Vector3.forward*sy*.65f);
            Move(21,new Vector3(sx,-.006f*plant.Shutter,sy));
            Turn(22,Mathf.Clamp((float)plant.Focus*.016f,-3,3),Vector3.right);
            Turn(23,Mathf.Clamp((float)plant.Focus*.012f,-2.5f,2.5f),Vector3.forward);
            Turn(24,3*sx,Vector3.up);
            Move(25,Vector3.down*(onStage?.007f:0));
            Move(26,new Vector3(sx,0,sy));
            Turn(27,Mathf.Clamp((float)plant.Focus*.011f,-3,3),Vector3.forward);
            Move(28,Vector3.down*(onStage?.006f:0));
            Move(29,Vector3.up*(plant.Vibration*.00012f*Mathf.Sin(now*10)));
            Move(30,Vector3.down*(plant.Shutter*.003f));
            Move(31,Vector3.right*(plant.Shutter*.014f));
            Move(32,Vector3.down*(emergency?.008f:0));
            Turn(33,mode==3?12:0,Vector3.forward);
            Turn(34,developer,Vector3.up);
            Turn(35,mode==4?18:0,Vector3.up);
            Turn(36,dryer,Vector3.up);
            Move(37,Vector3.down*(mode==6?.006f:0));
            Turn(38,(float)plant.Overlay*.22f,Vector3.up);
            Turn(39,reticle?5:0,Vector3.right);
            Turn(40,mode==4?9*Mathf.Sin(now*3):0,Vector3.up);
            carried.gameObject.SetActive(transfer is >.26f and <1.56f);
            beam.gameObject.SetActive(plant.Shutter>.1f);
            beam.localScale=new Vector3(.020f,.034f,.020f)*(.83f+.17f*plant.Shutter);
        }
    }
}
