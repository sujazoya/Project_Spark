using UnityEngine;

namespace ProjectSpark.Gameplay
{
    public sealed class SparkLevel73VisualRig : MonoBehaviour
    {
        public Transform art, wafer, carried;
        public Transform[] pivots;
        private readonly Vector3[] seats=new Vector3[35];
        private readonly Quaternion[] rotations=new Quaternion[35];
        private Vector3 filmScale, waferSeat;
        private float lastTime, platen, carrier, slurry, condition, brush, dryer;

        private void Awake()
        {
            if(art==null||pivots==null||pivots.Length!=35){enabled=false;return;}
            for(int i=0;i<pivots.Length;i++){seats[i]=pivots[i].localPosition;rotations[i]=pivots[i].localRotation;}
            filmScale=pivots[8].localScale; waferSeat=wafer.localPosition;
        }
        private void Move(int i,float vertical)
        {
            var pivot=pivots[i];
            pivot.localPosition=seats[i]+pivot.parent.InverseTransformVector(art.TransformVector(Vector3.up*vertical));
        }
        private void Turn(int i,float angle,Vector3 axis)=>
            pivots[i].localRotation=rotations[i]*Quaternion.AngleAxis(angle,axis);
        private static float Advance(float angle,float rpm,float dt)=>Mathf.Repeat(angle+rpm*6*dt,360);

        public void Apply(SparkLevel73Plant plant,float now,float transfer,bool loaded,bool conditioning,int wash,float scan,bool emergency)
        {
            if(!enabled)return;
            float dt=Mathf.Min(.14f,Mathf.Max(0,now-lastTime));
            lastTime=now;
            platen=Advance(platen,plant.PlatenRpm,dt);
            carrier=Advance(carrier,plant.CarrierRpm,dt);
            slurry=Advance(slurry,plant.Feed*65,dt);
            condition=Advance(condition,conditioning?95:0,dt);
            brush=Advance(brush,wash==2?85:0,dt);
            dryer=Advance(dryer,wash==3?130:0,dt);
            Turn(0,platen,Vector3.up);
            Turn(1,platen,Vector3.up);
            Turn(2,platen,Vector3.up);
            Turn(3,carrier,Vector3.up);
            float lowering=-.031f*plant.Position;
            Move(4,lowering);
            Turn(5,2.2f*(plant.Force[2]-plant.Force[0]),Vector3.forward);
            Turn(6,34*plant.Downforce,Vector3.up);
            Turn(7,carrier,Vector3.up);
            Move(7,lowering);
            Move(8,lowering);
            Turn(8,carrier,Vector3.up);
            pivots[8].localScale=new Vector3(filmScale.x,filmScale.y*(.35f+.65f*plant.MeanFilm/102),filmScale.z);
            Move(9,.006f*Mathf.Clamp01(plant.Planarity/27));
            float sweep=transfer>0?Mathf.Sin(Mathf.Clamp01(transfer/2.15f)*Mathf.PI):0;
            Turn(10,39*sweep,Vector3.up);
            Turn(11,-22*sweep,Vector3.forward);
            Turn(12,26*sweep,Vector3.forward);
            Move(13,.027f*sweep);
            Turn(14,slurry,Vector3.up);
            Turn(15,slurry,Vector3.right);
            Turn(16,21*Mathf.Clamp01(plant.Feed),Vector3.up);
            Move(17,-.0035f*Mathf.Clamp01(plant.Feed));
            Turn(18,conditioning?24:0,Vector3.up);
            Turn(19,condition,Vector3.up);
            Turn(20,condition,Vector3.right);
            Move(21,-.005f*plant.Wear);
            Turn(22,conditioning?6*Mathf.Sin(now*5):0,Vector3.forward);
            Turn(23,24*plant.Downforce,Vector3.forward);
            Turn(24,platen,Vector3.up);
            Turn(25,carrier,Vector3.up);
            Turn(26,Mathf.Repeat(now*210+plant.Temperature*3,360),Vector3.forward);
            Move(27,emergency?-.009f:0);
            Turn(28,brush,Vector3.right);
            Turn(29,dryer,Vector3.up);
            Move(30,plant.Downforce>.45f?-.006f:0);
            Move(31,-.004f*Mathf.Clamp01(plant.Downforce));
            Move(32,scan>0?-.021f*Mathf.Sin(scan/1.24f*Mathf.PI):0);
            Turn(33,scan>0?12*Mathf.Sin(scan/1.24f*Mathf.PI):0,Vector3.up);
            wafer.localPosition=waferSeat+wafer.parent.InverseTransformVector(art.TransformVector(Vector3.up*lowering));
            wafer.gameObject.SetActive(loaded);
            pivots[8].gameObject.SetActive(loaded);
            carried.gameObject.SetActive(transfer is >.32f and <1.93f);
        }
    }
}
