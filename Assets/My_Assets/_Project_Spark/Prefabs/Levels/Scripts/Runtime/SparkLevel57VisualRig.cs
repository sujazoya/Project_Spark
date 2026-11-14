using UnityEngine;

namespace ProjectSpark.Gameplay
{
    public sealed class SparkLevel57VisualRig : MonoBehaviour
    {
        [SerializeField] private Transform needle, armature, phase, motor, monitor, transfer, fanA, fanB, guard, stop;
        private Quaternion needleRest,phaseRest,motorRest,monitorRest,transferRest,fanARest,fanBRest;
        private Vector3 armatureRest,stopRest;
        private float motorAngle,fanAngle;
        private void Awake()
        {
            needleRest=needle.localRotation; phaseRest=phase.localRotation;
            armatureRest=armature.localPosition; stopRest=stop.localPosition;
            transferRest=transfer.localRotation; monitorRest=monitor.localRotation;
            motorRest=motor.localRotation;fanARest=fanA.localRotation;fanBRest=fanB.localRotation;
        }
        public void Apply(float delta,bool closed,bool island,bool stopped,float rpm,float watts,float dt)
        {
            needle.localRotation=needleRest*Quaternion.AngleAxis(-delta,Vector3.forward);
            phase.localRotation=phaseRest*Quaternion.AngleAxis(delta,Vector3.up);
            armature.localPosition=armatureRest+Vector3.down*(closed ? .012f : 0);
            transfer.localRotation=transferRest*Quaternion.AngleAxis(island?32:0,Vector3.right);
            stop.localPosition=stopRest+Vector3.down*(stopped ? .012f : 0);
            monitor.localRotation=monitorRest*Quaternion.AngleAxis(watts>.1f?-4:0,Vector3.right);
            motorAngle=Mathf.Repeat(motorAngle+rpm*6*dt,360);
            fanAngle=Mathf.Repeat(fanAngle+watts*290*dt,360);
            motor.localRotation=motorRest*Quaternion.AngleAxis(motorAngle,Vector3.right);
            fanA.localRotation=fanARest*Quaternion.AngleAxis(fanAngle,Vector3.up);
            fanB.localRotation=fanBRest*Quaternion.AngleAxis(fanAngle,Vector3.up);
        }
    }
}

