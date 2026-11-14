using UnityEngine;

namespace ProjectSpark.Gameplay
{
    public sealed class SparkLevel65VisualRig : MonoBehaviour
    {
        public Transform bench,encoder,gripper,gate,stop,guard,monitor;
        public Transform[] joints;
        public GameObject[] products;
        public Vector3 feed,vision,pick,gatePosition;
        public Vector3[] sortStarts,bins;
        private static readonly Vector3[] axes={Vector3.up,Vector3.right,Vector3.right,Vector3.forward,Vector3.right,Vector3.up};
        private static readonly float[] limits={157,87,125,160,100,170};
        private readonly Quaternion[] neutral=new Quaternion[6];
        private readonly float[] angles=new float[6];
        private Quaternion gateHome,guardHome,monitorHome;
        private Vector3 stopHome,homeTool;
        public Vector3 Tool => bench.InverseTransformPoint(gripper.position);
        public Vector3 HomeTool => homeTool;

        private void Awake()
        {
            if(!bench)return;
            for(int i=0;i<angles.Length;i++)neutral[i]=joints[i].localRotation;
            gateHome=gate.localRotation;
            guardHome=guard.localRotation;
            monitorHome=monitor.localRotation;
            stopHome=stop.localPosition;
            homeTool=Tool;
            // Teach the reachable pickup point nearest the authored pickup platform.
            Vector3 platform=pick;
            Transform root=joints[0];
            Vector3 basePoint=bench.InverseTransformPoint(root.position);
            Vector3 direction=Vector3.ProjectOnPlane(platform-basePoint,Vector3.up);
            var reach=Vector3.ProjectOnPlane(homeTool-basePoint,Vector3.up).magnitude;
            if(direction.sqrMagnitude<.001f)direction=Vector3.left;
            pick=basePoint+direction.normalized*Mathf.Clamp(reach,.075f,.25f)*.83f;
            pick.y=homeTool.y;
            vision=Vector3.Lerp(feed,pick,.52f);
        }

        public GameObject CreatePart(int kind)
        {
            var part=Instantiate(products[kind],bench,false);
            part.SetActive(true);
            return part;
        }

        public bool MoveTool(Vector3 localTarget,float step)
        {
            Vector3 target=bench.TransformPoint(localTarget);
            for(int sweep=0;sweep<3;sweep++)
            {
                for(int i=angles.Length-1;i>=0;i--)
                {
                    var joint=joints[i];
                    var axis=joint.TransformDirection(axes[i]);
                    Vector3 present=Vector3.ProjectOnPlane(gripper.position-joint.position,axis);
                    Vector3 desired=Vector3.ProjectOnPlane(target-joint.position,axis);
                    if(present.sqrMagnitude<1e-9f||desired.sqrMagnitude<1e-9f)continue;
                    float correction=Vector3.SignedAngle(present,desired,axis);
                    angles[i]=Mathf.Clamp(angles[i]+Mathf.Clamp(correction,-step,step),-limits[i],limits[i]);
                    joint.localRotation=neutral[i]*Quaternion.AngleAxis(angles[i],axes[i]);
                }
            }
            return Vector3.Distance(Tool,localTarget)<.039f;
        }

        public Vector3 DropTarget(int bin)
        {
            var basis=bench.InverseTransformPoint(joints[0].position);
            var toward=Vector3.ProjectOnPlane(sortStarts[bin]-basis,Vector3.up).normalized;
            var target=basis+toward*Mathf.Max(.10f,Vector3.ProjectOnPlane(pick-basis,Vector3.up).magnitude);
            target.y=pick.y;
            return target;
        }

        public bool Home(float amount)
        {
            bool settled=true;
            for(int i=0;i<angles.Length;i++)
            {
                angles[i]=Mathf.MoveTowards(angles[i],0,amount);
                joints[i].localRotation=neutral[i]*Quaternion.AngleAxis(angles[i],axes[i]);
                settled&=Mathf.Abs(angles[i])<.1f;
            }
            return settled;
        }

        public void Present(float beltDistance,float rejectPosition,bool safeDoor,bool emergency,bool powered,float dt)
        {
            if(!bench)return;
            encoder.Rotate(Vector3.right,beltDistance*1100,Space.Self);
            gate.localRotation=gateHome*Quaternion.AngleAxis(55*rejectPosition,Vector3.up);
            stop.localPosition=stopHome+Vector3.down*(emergency?.015f:0);
            guard.localRotation=Quaternion.RotateTowards(guard.localRotation,guardHome*Quaternion.AngleAxis(safeDoor?0:62,Vector3.up),120*dt);
            monitor.localRotation=Quaternion.RotateTowards(monitor.localRotation,monitorHome*Quaternion.AngleAxis(powered?8:0,Vector3.right),80*dt);
        }
    }
}
