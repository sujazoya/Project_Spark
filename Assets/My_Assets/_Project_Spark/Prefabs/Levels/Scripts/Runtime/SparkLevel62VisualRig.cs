using UnityEngine;

namespace ProjectSpark.Gameplay
{
    public sealed class SparkLevel62VisualRig : MonoBehaviour
    {
        public Transform main,bypass,inject,motor,heavy,fans,monitor,guard;
        private Quaternion mainRest,bypassRest,injectRest,monitorRest,guardRest;

        private void Awake()
        {
            if(main)mainRest=main.localRotation;
            if(bypass)bypassRest=bypass.localRotation;
            if(inject)injectRest=inject.localRotation;
            if(monitor)monitorRest=monitor.localRotation;
            if(guard)guardRest=guard.localRotation;
        }
        public void Apply(bool energized,bool shunted,bool restoring,bool closed,float heavySpeed,float motorSpeed,float temp,float dt)
        {
            Tilt(main,mainRest,energized?18:-12,dt);
            Tilt(bypass,bypassRest,shunted?18:-12,dt);
            Tilt(inject,injectRest,restoring?18:-12,dt);
            Tilt(guard,guardRest,closed?0:48,dt);
            Tilt(monitor,monitorRest,energized?8:0,dt);
            if(motor)motor.Rotate(Vector3.right,540*motorSpeed*dt,Space.Self);
            if(heavy)heavy.Rotate(Vector3.right,740*heavySpeed*dt,Space.Self);
            if(fans&&restoring)fans.Rotate(Vector3.right,Mathf.Clamp(temp-20,0,40)*34*dt,Space.Self);
        }
        private static void Tilt(Transform t,Quaternion rest,float angle,float dt)
        {
            if(t)t.localRotation=Quaternion.RotateTowards(t.localRotation,rest*Quaternion.AngleAxis(angle,Vector3.right),120*dt);
        }
    }
}

