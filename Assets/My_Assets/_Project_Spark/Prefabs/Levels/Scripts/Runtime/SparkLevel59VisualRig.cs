using UnityEngine;

namespace ProjectSpark.Gameplay
{
    public sealed class SparkLevel59VisualRig : MonoBehaviour
    {
        public Transform[] handles;
        public Transform rotor,starter,fan,motor,monitor,guard;
        private Quaternion[] rests;
        private Quaternion guardRest,monitorRest;

        private void Awake()
        {
            if(handles==null||handles.Length!=7)return;
            rests=new Quaternion[7];
            for(int i=0;i<7;i++)rests[i]=handles[i].localRotation;
            if(guard)guardRest=guard.localRotation;
            if(monitor)monitorRest=monitor.localRotation;
        }

        public void Apply(bool parallel,bool[] feeders,bool closed,float rpm,bool cranking,float motorSpeed,bool energized,float dt)
        {
            if(rests==null)return;
            for(int i=0;i<rests.Length;i++)
            {
                bool on=i==1?parallel:i>=2&&feeders[i-2];
                handles[i].localRotation=Quaternion.RotateTowards(handles[i].localRotation,
                    rests[i]*Quaternion.AngleAxis(on?18f:-16f,Vector3.right),120f*dt);
            }
            if(guard)guard.localRotation=Quaternion.RotateTowards(guard.localRotation,guardRest*
                Quaternion.AngleAxis(closed?0:48,Vector3.right),100f*dt);
            if(monitor)monitor.localRotation=Quaternion.RotateTowards(monitor.localRotation,monitorRest*
                Quaternion.AngleAxis(energized?7:0,Vector3.right),35f*dt);
            Spin(rotor,rpm*6,dt);
            Spin(starter,cranking?rpm*3:0,dt);
            Spin(fan,feeders[2]&&energized?640:0,dt);
            Spin(motor,motorSpeed*850,dt);
        }
        private static void Spin(Transform pivot,float degreesPerSecond,float dt)
        {
            if(pivot)pivot.Rotate(Vector3.right,degreesPerSecond*dt,Space.Self);
        }
    }
}
