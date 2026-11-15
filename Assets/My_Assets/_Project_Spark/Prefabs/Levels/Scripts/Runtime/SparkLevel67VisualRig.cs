using UnityEngine;

namespace ProjectSpark.Gameplay
{
    public sealed class SparkLevel67VisualRig : MonoBehaviour
    {
        public Transform[] pivots, liquid;
        private readonly Quaternion[] rest=new Quaternion[14];
        private readonly Vector3[] fluidSize=new Vector3[4];
        private MaterialPropertyBlock color;
        private Vector3 stopPosition,doorPosition;
        private float previousTime;
        private int baseColor;
        private int emission;

        private void Awake()
        {
            color=new MaterialPropertyBlock();
            baseColor=Shader.PropertyToID("_BaseColor");
            emission=Shader.PropertyToID("_EmissionColor");
            for(int i=0;i<rest.Length;i++) rest[i]=pivots[i].localRotation;
            for(int i=0;i<fluidSize.Length;i++) fluidSize[i]=liquid[i].localScale;
            stopPosition=pivots[12].localPosition;
            doorPosition=pivots[13].localPosition;
        }
        public void Apply(SparkLevel67Hydraulics plant,float time,bool stopped,bool isolated)
        {
            float dt=Mathf.Max(0,time-previousTime);
            previousTime=time;
            for(int i=0;i<3;i++)
            {
                pivots[i].Rotate(Vector3.right,plant.Pumps[i].Rpm*6*dt,Space.Self);
                pivots[i+3].Rotate(Vector3.right,plant.Pumps[i].Rpm*6*dt,Space.Self);
            }
            for(int i=0;i<4;i++) pivots[i+6].localRotation=rest[i+6]*Quaternion.AngleAxis(160*plant.Valves[i],Vector3.up);
            pivots[10].Rotate(Vector3.up,plant.MixerRpm*6*dt,Space.Self);
            pivots[11].localRotation=rest[11];
            pivots[12].localPosition=stopPosition+(stopped?Vector3.down*.014f:Vector3.zero);
            pivots[13].localPosition=Vector3.MoveTowards(pivots[13].localPosition,doorPosition+(isolated?Vector3.zero:Vector3.forward*.025f),dt*.05f);
            for(int i=0;i<4;i++)
            {
                var tank=plant.Tanks[i];
                var size=fluidSize[i];
                size.y*=Mathf.Max(.001f,tank.Volume/tank.Capacity);
                liquid[i].localScale=size;
                liquid[i].localPosition=Vector3.up*size.y;
                var tint=Color.Lerp(new Color(.98f,.45f,.08f),new Color(.04f,.69f,.98f),tank.FractionA);
                color.SetColor(baseColor,tint); color.SetColor(emission,tint*.23f);
                liquid[i].GetComponent<Renderer>().SetPropertyBlock(color);
            }
        }
    }
}

