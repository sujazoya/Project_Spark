using UnityEngine;

namespace ProjectSpark.Gameplay
{
    public sealed class SparkLevel61VisualRig : MonoBehaviour
    {
        public Transform main,motor,capacitor,fans,monitor,guard;
        private Quaternion mainRest,capacitorRest,monitorRest,guardRest;

        private void Awake()
        {
            if(main)mainRest=main.localRotation;
            if(capacitor)capacitorRest=capacitor.localRotation;
            if(monitor)monitorRest=monitor.localRotation;
            if(guard)guardRest=guard.localRotation;
        }
        public void Apply(bool energized,bool loaded,bool compensated,bool closed,bool filtering,float load,float temp,float dt)
        {
            Tilt(main,mainRest,energized?18:-12,dt);
            Tilt(capacitor,capacitorRest,compensated?18:-12,dt);
            Tilt(guard,guardRest,closed?0:48,dt);
            Tilt(monitor,monitorRest,energized?8:0,dt);
            if(motor&&loaded)motor.Rotate(Vector3.right,540*load*dt,Space.Self);
            if(fans&&filtering)fans.Rotate(Vector3.right,Mathf.Clamp(temp-20,0,40)*34*dt,Space.Self);
        }
        private static void Tilt(Transform t,Quaternion rest,float angle,float dt)
        {
            if(t)t.localRotation=Quaternion.RotateTowards(t.localRotation,rest*Quaternion.AngleAxis(angle,Vector3.right),120*dt);
        }
    }
}
