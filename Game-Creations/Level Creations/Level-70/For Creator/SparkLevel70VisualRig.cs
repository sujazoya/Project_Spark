using UnityEngine;

namespace ProjectSpark.Visuals
{
    /// <summary>Optional art-only rig adapter. Attach to L70_HD_VISUAL_ART and assign separated moving
    /// submeshes in the Inspector. Actual gameplay/controllers must call these methods.
    /// Never supplies authority for airflow, particle counts, pressure, doors, or safety.</summary>
    [DisallowMultipleComponent]
    public sealed class SparkLevel70VisualRig : MonoBehaviour
    {
        [Header("Optional moving mesh references - set after splitting imported FBX submeshes")]
        [SerializeField] private Transform supplyBlower;
        [SerializeField] private Transform returnBlower;
        [SerializeField] private Transform exhaustBlower;
        [SerializeField] private Transform fanFilterA;
        [SerializeField] private Transform fanFilterB;
        [SerializeField] private Transform innerDoor;
        [SerializeField] private Transform outerDoor;
        [SerializeField] private Transform supplyDamper;
        [SerializeField] private Transform airlockDamper;
        private readonly float[] rpm = new float[5];
        private float innerOpen, outerOpen, supplyOpen, airlockOpen;
        public void SetFanRPM(float supply, float ret, float exhaust, float ffuA, float ffuB)
        {
            rpm[0] = Mathf.Max(0,supply); rpm[1] = Mathf.Max(0,ret);
            rpm[2] = Mathf.Max(0,exhaust); rpm[3] = Mathf.Max(0,ffuA); rpm[4] = Mathf.Max(0,ffuB);
        }
        public void SetDoorOpenFractions(float inner, float outer)
        { innerOpen = Mathf.Clamp01(inner); outerOpen = Mathf.Clamp01(outer); }
        public void SetDamperOpenFractions(float supply, float airlock)
        { supplyOpen = Mathf.Clamp01(supply); airlockOpen = Mathf.Clamp01(airlock); }
        private void Update()
        {
            Transform[] fans = { supplyBlower, returnBlower, exhaustBlower, fanFilterA, fanFilterB };
            for (int i=0; i<fans.Length; i++)
                if (fans[i] != null) fans[i].Rotate(Vector3.forward, rpm[i] * 6f * Time.deltaTime, Space.Self);
            if (innerDoor != null) innerDoor.localRotation = Quaternion.Euler(0f, innerOpen*88f, 0f);
            if (outerDoor != null) outerDoor.localRotation = Quaternion.Euler(0f, -outerOpen*88f, 0f);
            if (supplyDamper != null) supplyDamper.localRotation = Quaternion.Euler(supplyOpen*85f,0f,0f);
            if (airlockDamper != null) airlockDamper.localRotation = Quaternion.Euler(airlockOpen*85f,0f,0f);
        }
    }
}
