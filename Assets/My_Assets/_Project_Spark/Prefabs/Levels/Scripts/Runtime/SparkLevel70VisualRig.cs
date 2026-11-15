using UnityEngine;

namespace ProjectSpark.Gameplay
{
    public sealed class SparkLevel70VisualRig : MonoBehaviour
    {
        public Transform[] pivots,flowMarkers;
        public Transform secondFfuA,secondFfuB,cartridgeA,wafer;
        private readonly Quaternion[] rest=new Quaternion[19];
        private Vector3 innerClosed,outerClosed,filterSeat;
        private float previousTime;

        private void Awake()
        {
            for(int i=0;i<rest.Length;i++) rest[i]=pivots[i].localRotation;
            innerClosed=pivots[9].localPosition;
            outerClosed=pivots[10].localPosition;
            filterSeat=cartridgeA.localPosition;
        }

        private static void Spin(Transform pivot,Vector3 axis,float rpm,float dt)=>pivot.Rotate(axis,rpm*6*dt,Space.Self);

        public void Apply(SparkLevel70AirPlant plant,float time,float waferTime,bool shower,bool servicing)
        {
            float dt=Mathf.Max(0,time-previousTime);
            previousTime=time;
            for(int i=0;i<3;i++) Spin(pivots[i],Vector3.forward,plant.Fans[i].Rpm,dt);
            Spin(pivots[3],Vector3.up,plant.Fans[3].Rpm,dt);
            Spin(pivots[4],Vector3.up,plant.Fans[4].Rpm,dt);
            Spin(secondFfuA,Vector3.up,plant.Fans[3].Rpm*.96f,dt);
            Spin(secondFfuB,Vector3.up,plant.Fans[4].Rpm*.96f,dt);
            for(int i=0;i<4;i++) pivots[5+i].localRotation=rest[5+i]*Quaternion.AngleAxis(65*plant.Dampers[i],Vector3.right);
            pivots[9].localPosition=innerClosed+Vector3.forward*plant.InnerDoor*.24f;
            pivots[10].localPosition=outerClosed-Vector3.forward*plant.OuterDoor*.24f;
            float motion=waferTime>0?Mathf.Sin(Mathf.Clamp01(waferTime/3.5f)*Mathf.PI):0;
            pivots[11].localRotation=rest[11]*Quaternion.AngleAxis(32*motion,Vector3.up);
            pivots[12].localRotation=rest[12]*Quaternion.AngleAxis(-28*motion,Vector3.forward);
            pivots[13].localRotation=rest[13]*Quaternion.AngleAxis(30*motion,Vector3.forward);
            wafer.gameObject.SetActive(waferTime>.35f&&waferTime<3.25f);
            pivots[14].localRotation=rest[14]*Quaternion.AngleAxis(shower?2*Mathf.Sin(time*7):0,Vector3.up);
            pivots[15].localRotation=rest[15]*Quaternion.AngleAxis(servicing?35:0,Vector3.forward);
            pivots[16].localRotation=rest[16];
            pivots[17].localRotation=rest[17];
            pivots[18].localRotation=rest[18]*Quaternion.AngleAxis(plant.SupplyRoom>.09f?3*Mathf.Sin(time*2):0,Vector3.forward);
            cartridgeA.localPosition=Vector3.MoveTowards(cartridgeA.localPosition,filterSeat+(servicing?Vector3.forward*.082f:Vector3.zero),dt*.135f);

            for(int i=0;i<flowMarkers.Length;i++)
            {
                int branch=i/3;
                float flow=plant.Filters[branch].Flow;
                var marker=flowMarkers[i];
                marker.gameObject.SetActive(flow>.10f);
                float fraction=Mathf.Repeat(time*(.30f+flow*.4f)+(i%3)*.33f,1);
                marker.localPosition=new Vector3((branch==0?-.172f:.106f)+(i%3-1)*.025f,.318f-.21f*fraction,.085f);
            }
        }
    }
}
