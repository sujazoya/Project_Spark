using UnityEngine;
namespace ProjectSpark.Gameplay
{
    public sealed class SparkLevel63VisualRig : MonoBehaviour
    {
        public Transform grid,generator,generatorRotor,prime,rotor,drive,coupling,encoder,motor,solar,monitor,guard;
        private Quaternion gridRest,genRest,solarRest,monitorRest,guardRest;
        private void Awake()
        {
            gridRest=grid.localRotation; genRest=generator.localRotation;
            solarRest=solar.localRotation; monitorRest=monitor.localRotation; guardRest=guard.localRotation;
        }
        public void Apply(bool main,bool gen,bool closed,float generatorRad,float rotorRad,bool loaded,bool cloud,float dt)
        {
            Tilt(grid,gridRest,main?18:-12,dt);
            Tilt(generator,genRest,gen?18:-12,dt);
            Tilt(guard,guardRest,closed?0:48,dt);
            Tilt(monitor,monitorRest,main?8:0,dt);
            Tilt(solar,solarRest,cloud?24:0,dt);
            Spin(generatorRotor,generatorRad,dt);
            Spin(prime,generatorRad,dt);
            Spin(rotor,rotorRad,dt);
            Spin(drive,rotorRad,dt);
            Spin(coupling,rotorRad,dt);
            Spin(encoder,rotorRad,dt);
            Spin(motor,loaded?14:0,dt);
        }
        private static void Spin(Transform item,float radians,float dt)
        {
            if(item)item.Rotate(Vector3.right,radians*Mathf.Rad2Deg*dt,Space.Self);
        }
        private static void Tilt(Transform item,Quaternion rest,float angle,float dt)
        {
            if(item)item.localRotation=Quaternion.RotateTowards(item.localRotation,rest*Quaternion.AngleAxis(angle,Vector3.right),120*dt);
        }
    }
}
