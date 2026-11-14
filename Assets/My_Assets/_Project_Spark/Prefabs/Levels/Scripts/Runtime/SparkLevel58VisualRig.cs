using UnityEngine;

namespace ProjectSpark.Gameplay
{
    public sealed class SparkLevel58VisualRig : MonoBehaviour
    {
        public Transform[] handles;
        public Transform motor,monitor,guard;
        private Quaternion[] handleRest;
        private Quaternion guardRest, monitorRest;

        private void Awake()
        {
            if(handles==null||handles.Length!=4)return;
            handleRest=new Quaternion[4];
            for(int i=0;i<4;i++)handleRest[i]=handles[i].localRotation;
            if(guard)guardRest=guard.localRotation;
            if(monitor)monitorRest=monitor.localRotation;
        }

        public void Apply(bool q0,bool q1,bool q2,bool q3,bool guardClosed,float motorCurrent,float dt)
        {
            if(handleRest==null)return;
            MoveHandle(0,q0,dt);
            MoveHandle(1,q1,dt);
            MoveHandle(2,q2,dt);
            MoveHandle(3,q3,dt);
            if(guard)guard.localRotation=Quaternion.RotateTowards(guard.localRotation,guardRest*Quaternion.AngleAxis(guardClosed?0f:50f,Vector3.right),100f*dt);
            if(monitor)monitor.localRotation=Quaternion.RotateTowards(monitor.localRotation,monitorRest*Quaternion.AngleAxis(q0?7f:0f,Vector3.right),45f*dt);
            if(motor)motor.Rotate(Vector3.right,Mathf.Clamp01(motorCurrent/.22f)*220f*dt,Space.Self);
        }

        private void MoveHandle(int index,bool closed,float dt)
        {
            var pivot=handles[index];
            var target=handleRest[index]*Quaternion.AngleAxis(closed?18f:-16f,Vector3.right);
            pivot.localRotation=Quaternion.RotateTowards(pivot.localRotation,target,200f*dt);
        }
    }
}
