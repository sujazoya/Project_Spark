using UnityEngine;

namespace ProjectSpark.Gameplay
{
    public sealed class SparkLevel69VisualRig : MonoBehaviour
    {
        public Transform[] pivots,droplets;
        public Transform fanA2,fanB2,frostB;
        private readonly Quaternion[] rest=new Quaternion[14];
        private readonly Vector3[] doorRest=new Vector3[3];
        private float previousTime;

        private void Awake()
        {
            for(int i=0;i<rest.Length;i++) rest[i]=pivots[i].localRotation;
            doorRest[0]=pivots[8].localPosition;doorRest[1]=pivots[9].localPosition;doorRest[2]=pivots[13].localPosition;
        }
        private static void Spin(Transform pivot,Vector3 axis,float turnsPerMinute,float dt)
        {
            pivot.Rotate(axis,turnsPerMinute*6f*dt,Space.Self);
        }
        public void Apply(SparkLevel69ColdPlant plant,float time,bool isolated)
        {
            float dt=Mathf.Max(0,time-previousTime);
            previousTime=time;
            Spin(pivots[0],Vector3.right,plant.Rpm,dt);
            Spin(pivots[1],Vector3.right,plant.Rpm*.35f,dt);
            Spin(pivots[2],Vector3.forward,plant.Fan*840f,dt);
            Spin(pivots[3],Vector3.forward,plant.Fan*810f,dt);
            Spin(pivots[4],Vector3.forward,plant.Zones[0].AirFan*570,dt);
            Spin(pivots[5],Vector3.forward,plant.Zones[1].AirFan*570,dt);
            Spin(fanA2,Vector3.forward,plant.Zones[0].AirFan*554,dt);
            Spin(fanB2,Vector3.forward,plant.Zones[1].AirFan*554,dt);
            for(int i=0;i<2;i++)
                pivots[i+6].localRotation=rest[i+6]*Quaternion.AngleAxis(155*plant.Zones[i].Valve,Vector3.up);
            pivots[8].localPosition=doorRest[0]+Vector3.right*.43f*plant.Zones[0].Door;
            pivots[9].localPosition=doorRest[1];
            pivots[10].gameObject.SetActive(plant.Zones[0].Frost>.017f);
            frostB.gameObject.SetActive(plant.Zones[1].Frost>.025f);
            frostB.localScale=Vector3.one*(1+.30f*Mathf.InverseLerp(.025f,.12f,plant.Zones[1].Frost));
            pivots[12].localRotation=rest[12];
            pivots[13].localPosition=Vector3.MoveTowards(pivots[13].localPosition,doorRest[2]+(isolated?Vector3.zero:Vector3.up*.036f),dt*.055f);
            for(int i=0;i<droplets.Length;i++)
            {
                var drop=droplets[i];
                drop.gameObject.SetActive(plant.Drain>.002f);
                drop.localPosition=new Vector3(i*.025f,-.12f*Mathf.Repeat(time*1.8f+i*.33f,1),0);
            }
        }
    }
}
