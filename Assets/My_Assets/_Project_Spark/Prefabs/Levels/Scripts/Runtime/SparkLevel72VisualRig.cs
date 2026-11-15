using UnityEngine;

namespace ProjectSpark.Gameplay
{
    public sealed class SparkLevel72VisualRig : MonoBehaviour
    {
        public Transform art, wafer, carried;
        public Transform[] pivots;
        public GameObject tracer;
        private readonly Vector3[] seats=new Vector3[24];
        private readonly Quaternion[] rotations=new Quaternion[24];
        private Vector3 filmScale, tracerScale;
        private MaterialPropertyBlock tracerColor;
        private Renderer tracerRenderer;
        private float lastTime, turbine, backing;

        private void Awake()
        {
            if(art==null||pivots==null||pivots.Length!=24){enabled=false;return;}
            for(int i=0;i<pivots.Length;i++)
            {
                seats[i]=pivots[i].localPosition;
                rotations[i]=pivots[i].localRotation;
            }
            filmScale=pivots[21].localScale;
            tracerScale=tracer.transform.localScale;
            tracerRenderer=tracer.GetComponent<Renderer>();
            tracerColor=new MaterialPropertyBlock();
        }

        private void Move(int i,float vertical)
        {
            var pivot=pivots[i];
            pivot.localPosition=seats[i]+pivot.parent.InverseTransformVector(art.TransformVector(Vector3.up*vertical));
        }

        private void Turn(int i,float angle,Vector3 axis) =>
            pivots[i].localRotation=rotations[i]*Quaternion.AngleAxis(angle,axis);

        public void Apply(SparkLevel72Plant plant,float now,float transfer,bool loaded,bool clamped,float metrology,int phase,bool emergency)
        {
            if(!enabled)return;
            float dt=Mathf.Min(.16f,Mathf.Max(0,now-lastTime));
            lastTime=now;
            float travel=transfer>0?Mathf.Sin(Mathf.Clamp01(transfer/2.1f)*Mathf.PI):0;
            Move(1,.051f*plant.Gate);
            Turn(2,38*travel,Vector3.up);
            Turn(3,-16*travel,Vector3.forward);
            Turn(4,20*travel,Vector3.forward);
            Move(5,.023f*travel);
            Move(6,clamped?-.004f:0);
            Move(7,.010f*plant.ValveA);
            Move(8,.010f*plant.ValveB);
            Move(9,.010f*plant.PurgeValve);
            Turn(10,80*plant.FlowA/5.4f,Vector3.forward);
            Turn(11,80*plant.FlowB/5.4f,Vector3.forward);
            turbine=Mathf.Repeat(turbine+plant.Turbo*1240*dt,360);
            backing=Mathf.Repeat(backing+plant.Backing*460*dt,360);
            Turn(12,turbine,Vector3.up);
            Turn(13,backing,Vector3.right);
            Turn(14,75*plant.Throttle,Vector3.up);
            Turn(15,85*plant.Isolation,Vector3.up);
            Turn(17,plant.Optical*.43f,Vector3.forward);
            Move(18,metrology>0?.024f*Mathf.Sin(metrology/1.3f*Mathf.PI):0);
            Move(19,emergency?-.009f:0);
            Turn(22,phase*3f,Vector3.forward);
            Turn(23,metrology>0?19*plant.Conformality*Mathf.Sin(metrology/1.3f*Mathf.PI):0,Vector3.up);
            wafer.gameObject.SetActive(loaded);
            carried.gameObject.SetActive(transfer is >.34f and <1.91f);
            pivots[21].gameObject.SetActive(loaded&&plant.MeanFilm>.03f);
            pivots[21].localScale=filmScale*(.68f+.32f*Mathf.Clamp01(plant.MeanFilm/12));
            float glow=Mathf.Clamp01((plant.FlowA+plant.FlowB)*.18f+plant.PurgeFlow*.20f);
            tracer.SetActive(glow>.04f);
            if(glow>.04f)
            {
                var color=phase==1?new Color(.22f,.77f,1,.43f):phase==3?new Color(1,.58f,.14f,.43f):new Color(.49f,.88f,.76f,.28f);
                tracerColor.SetColor("_BaseColor",color);
                tracerColor.SetColor("_EmissionColor",color*(.65f+glow));
                tracerRenderer.SetPropertyBlock(tracerColor);
                tracer.transform.localScale=tracerScale*(.72f+.28f*glow);
            }
        }
    }
}
