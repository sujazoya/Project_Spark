using UnityEngine;

namespace ProjectSpark.Gameplay
{
    public sealed class SparkLevel66VisualRig : MonoBehaviour
    {
        public Transform motor,shaft,cover,gears,coupling,roller,encoder,monitor,tool,fans,stop,guard;
        public Transform original,damaged,replacement;
        private Quaternion coverRest,guardRest,monitorRest;
        private Vector3 bearingSeat,spareRest,damagedRest,toolRest,stopRest;

        private void Awake()
        {
            coverRest=cover.localRotation;
            guardRest=guard.localRotation;
            monitorRest=monitor.localRotation;
            bearingSeat=original.position;
            spareRest=replacement.position;
            damagedRest=damaged.position;
            toolRest=tool.localPosition;
            stopRest=stop.localPosition;
        }

        public void Apply(float rpm,bool closed,bool stopped,float wear,int repair,float extract,float temp,float dt)
        {
            Spin(motor,rpm,dt); Spin(shaft,rpm,dt); Spin(coupling,rpm,dt); Spin(encoder,rpm,dt);
            Spin(gears,rpm*.25f,dt); Spin(roller,rpm*.25f,dt); Spin(fans,Mathf.Max(0,temp-24)*16,dt);
            cover.localRotation=Quaternion.RotateTowards(cover.localRotation,coverRest*Quaternion.AngleAxis(closed?0:26,Vector3.right),90*dt);
            guard.localRotation=Quaternion.RotateTowards(guard.localRotation,guardRest*Quaternion.AngleAxis(closed?0:50,Vector3.right),90*dt);
            monitor.localRotation=monitorRest;
            stop.localPosition=stopRest+Vector3.down*(stopped?.012f:0);
            tool.localPosition=toolRest+Vector3.right*(.14f*extract);
            original.gameObject.SetActive(repair==0&&wear<.20f);
            damaged.gameObject.SetActive(repair<2);
            damaged.position=repair==0&&wear<.20f?damagedRest:Vector3.Lerp(bearingSeat,damagedRest,extract);
            replacement.position=repair>=2?bearingSeat:spareRest;
        }

        private static void Spin(Transform pivot,float rpm,float dt)
        {
            pivot.Rotate(Vector3.right,rpm*6*dt,Space.Self);
        }
    }
}
