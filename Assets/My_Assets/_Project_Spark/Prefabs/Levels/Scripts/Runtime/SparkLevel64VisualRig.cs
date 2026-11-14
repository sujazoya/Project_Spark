using UnityEngine;
namespace ProjectSpark.Gameplay
{
    public sealed class SparkLevel64VisualRig : MonoBehaviour
    {
        public Transform[] breakers,rotors,primes,encoders;
        public Transform monitor,guard;
        private Quaternion[] rest;
        private Quaternion monitorRest,guardRest;
        private void Awake()
        {
            rest=new Quaternion[breakers.Length];
            for(int i=0;i<rest.Length;i++)rest[i]=breakers[i].localRotation;
            monitorRest=monitor.localRotation;
            guardRest=guard.localRotation;
        }
        public void Apply(bool[] closed,bool safe,float hzA,float hzB,float dt)
        {
            for(int i=0;i<rest.Length;i++)Tilt(breakers[i],rest[i],closed[i]?18:-12,dt);
            Tilt(monitor,monitorRest,hzA>45?8:0,dt);
            Tilt(guard,guardRest,safe?0:48,dt);
            for(int i=0;i<2;i++)
            {
                float spin=(i==0?hzA:hzB)*180*dt;
                rotors[i].Rotate(Vector3.right,spin,Space.Self);
                primes[i].Rotate(Vector3.right,spin,Space.Self);
                encoders[i].Rotate(Vector3.right,spin,Space.Self);
            }
        }
        private static void Tilt(Transform value,Quaternion neutral,float angle,float dt)
        {
            value.localRotation=Quaternion.RotateTowards(value.localRotation,neutral*Quaternion.AngleAxis(angle,Vector3.right),120*dt);
        }
    }
}
