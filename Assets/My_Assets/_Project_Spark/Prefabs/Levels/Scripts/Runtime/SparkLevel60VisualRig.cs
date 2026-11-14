using UnityEngine;

namespace ProjectSpark.Gameplay
{
    public sealed class SparkLevel60VisualRig : MonoBehaviour
    {
        public Transform[] handles;
        public Transform rotor,starter,solar,motor,monitor,guard;
        private Quaternion[] rests;
        private Quaternion guardRest,monitorRest,solarRest;

        private void Awake()
        {
            if(handles==null||handles.Length!=8)return;
            rests=new Quaternion[8];
            for(int i=0;i<8;i++)rests[i]=handles[i].localRotation;
            if(guard)guardRest=guard.localRotation;
            if(monitor)monitorRest=monitor.localRotation;
            if(solar)solarRest=solar.localRotation;
        }

        public void Apply(bool grid,bool generator,bool battery,bool renewable,bool[] feeders,bool closed,float rpm,bool cranking,float irradiance,float motorSpeed,float dt)
        {
            if(rests==null)return;
            for(int i=0;i<rests.Length;i++)
            {
                bool on=i switch {0=>grid,1=>generator,2=>battery,3=>renewable,_=>feeders[i-4]};
                handles[i].localRotation=Quaternion.RotateTowards(handles[i].localRotation,
                    rests[i]*Quaternion.AngleAxis(on?18f:-16f,Vector3.right),120f*dt);
            }
            if(guard)guard.localRotation=Quaternion.RotateTowards(guard.localRotation,guardRest*
                Quaternion.AngleAxis(closed?0:48,Vector3.right),100f*dt);
            if(monitor)monitor.localRotation=Quaternion.RotateTowards(monitor.localRotation,monitorRest*
                Quaternion.AngleAxis(grid||battery?7:0,Vector3.right),35f*dt);
            Spin(rotor,rpm*6,dt);
            Spin(starter,cranking?rpm*3:0,dt);
            if(solar)solar.localRotation=Quaternion.RotateTowards(solar.localRotation,solarRest*Quaternion.AngleAxis((1-irradiance)*18,Vector3.right),32f*dt);
            Spin(motor,motorSpeed*850,dt);
        }
        private static void Spin(Transform pivot,float degreesPerSecond,float dt)
        {
            if(pivot)pivot.Rotate(Vector3.right,degreesPerSecond*dt,Space.Self);
        }
    }
}

