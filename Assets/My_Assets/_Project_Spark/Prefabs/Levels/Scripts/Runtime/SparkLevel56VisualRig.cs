using UnityEngine;

namespace ProjectSpark.Gameplay
{
    public sealed class SparkLevel56VisualRig : MonoBehaviour
    {
        [SerializeField] private Transform motor, flywheel, encoder, guard, monitor, fanLeft, fanRight;
        [SerializeField] private Light bankLamp, brakeLamp;
        private Quaternion guardRest, monitorRest, motorRest, flywheelRest, encoderRest, fanLeftRest, fanRightRest;
        private float fanAngle;
        private void Awake()
        {
            guardRest=guard.localRotation;
            monitorRest=monitor.localRotation;
            motorRest=motor.localRotation;
            flywheelRest=flywheel.localRotation;
            encoderRest=encoder.localRotation;
            fanLeftRest=fanLeft.localRotation;
            fanRightRest=fanRight.localRotation;
        }
        public void Apply(float radians,bool open,float temperature,float bankAmps,float rpm,float delta)
        {
            Quaternion spin=Quaternion.AngleAxis(radians*Mathf.Rad2Deg,Vector3.right);
            motor.localRotation=motorRest*spin;
            flywheel.localRotation=flywheelRest*spin;
            encoder.localRotation=encoderRest*spin;
            guard.localRotation=guardRest*Quaternion.AngleAxis(open?68:0,Vector3.right);
            monitor.localRotation=monitorRest*Quaternion.AngleAxis(rpm>1?-5:0,Vector3.right);
            float cooling=Mathf.Clamp01((temperature-24)/15);
            fanAngle=Mathf.Repeat(fanAngle+cooling*900*delta,360);
            var fanSpin=Quaternion.AngleAxis(fanAngle,Vector3.up);
            fanLeft.localRotation=fanLeftRest*fanSpin;
            fanRight.localRotation=fanRightRest*fanSpin;
            if (bankLamp) {bankLamp.color=bankAmps<0?new Color(.3f,.7f,1):Color.green;bankLamp.intensity=Mathf.Abs(bankAmps)*.07f;}
            if (brakeLamp) brakeLamp.intensity=cooling*.13f;
        }
    }
}
