using UnityEngine;

namespace ProjectSpark.Gameplay
{
    public sealed class SparkLevel68VisualRig : MonoBehaviour
    {
        public Transform[] pivots, liquid;
        private readonly Quaternion[] rest=new Quaternion[14];
        private MaterialPropertyBlock color;
        private int baseColor,emission;
        private Vector3 stopPosition,doorPosition;
        private float previousTime;
        private static readonly Color Cool=new(.10f,.61f,1f);
        private static readonly Color Hot=new(1f,.29f,.04f);
        private static readonly Color Product=new(.65f,.33f,.92f);

        private void Awake()
        {
            color=new MaterialPropertyBlock();
            baseColor=Shader.PropertyToID("_BaseColor");
            emission=Shader.PropertyToID("_EmissionColor");
            for(int i=0;i<rest.Length;i++) rest[i]=pivots[i].localRotation;
            stopPosition=pivots[12].localPosition;
            doorPosition=pivots[13].localPosition;
        }
        public void Apply(SparkLevel68ThermalPlant plant,float time,bool stopped,bool isolated)
        {
            float dt=Mathf.Max(0,time-previousTime);
            previousTime=time;
            for(int i=0;i<3;i++)
            {
                pivots[i].Rotate(Vector3.right,plant.Pumps[i].Rpm*6*dt,Space.Self);
                pivots[i+3].Rotate(Vector3.right,plant.Pumps[i].Rpm*6*dt,Space.Self);
            }
            for(int i=0;i<5;i++) pivots[i+6].localRotation=rest[i+6]*Quaternion.AngleAxis(155*plant.Valves[i],Vector3.up);
            pivots[11].localRotation=rest[11];
            pivots[12].localPosition=stopPosition+(stopped?Vector3.down*.014f:Vector3.zero);
            pivots[13].localPosition=Vector3.MoveTowards(pivots[13].localPosition,doorPosition+(isolated?Vector3.zero:Vector3.forward*.024f),dt*.045f);

            Tint(0,Color.Lerp(new Color(.84f,.48f,.17f),Hot,Mathf.InverseLerp(43,86,plant.Hot)));
            Tint(1,Color.Lerp(Cool,new Color(.14f,.74f,.76f),Mathf.InverseLerp(20,43,plant.Cold)));
            Tint(2,Color.Lerp(new Color(.77f,.47f,.21f),Hot,Mathf.InverseLerp(40,84,plant.Buffer)));
            Tint(3,Color.Lerp(Product,new Color(.91f,.35f,.60f),Mathf.InverseLerp(25,72,plant.Product)));
        }
        private void Tint(int tank,Color tint)
        {
            color.SetColor(baseColor,tint);
            color.SetColor(emission,tint*.21f);
            liquid[tank].GetComponent<Renderer>().SetPropertyBlock(color);
        }
    }
}
