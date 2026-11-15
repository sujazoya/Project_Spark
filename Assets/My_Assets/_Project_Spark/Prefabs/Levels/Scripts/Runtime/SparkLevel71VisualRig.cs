using UnityEngine;

namespace ProjectSpark.Gameplay
{
    public sealed class SparkLevel71VisualRig : MonoBehaviour
    {
        public Transform[] pivots;
        public Transform art, plasma, film, mask, carried;
        private readonly Quaternion[] rest = new Quaternion[21];
        private Vector3 gateSeat, liftSeat, clampSeat, probeSeat;
        private Vector3 gateTravel, liftTravel, clampTravel, probeTravel, plasmaSize;
        private float previousTime, turboPhase, backingPhase;

        private void Awake()
        {
            if (pivots == null || pivots.Length != rest.Length)
            {
                enabled = false;
                return;
            }
            for (int i = 0; i < rest.Length; i++) rest[i] = pivots[i].localRotation;
            gateSeat = pivots[1].localPosition;
            liftSeat = pivots[5].localPosition;
            clampSeat = pivots[6].localPosition;
            probeSeat = pivots[20].localPosition;
            gateTravel = VerticalTravel(1, .065f);
            liftTravel = VerticalTravel(5, .030f);
            clampTravel = VerticalTravel(6, .007f);
            probeTravel = VerticalTravel(20, .028f);
            Vector3 scale = plasma.parent.lossyScale;
            plasmaSize = Vector3.Scale(new Vector3(.152f,.055f,.152f), new Vector3(
                art.lossyScale.x/scale.x, art.lossyScale.y/scale.y, art.lossyScale.z/scale.z));
        }

        private Vector3 VerticalTravel(int pivot, float distance) =>
            pivots[pivot].parent.InverseTransformVector(art.TransformVector(Vector3.up * distance));

        private void Turn(int pivot, float angle, Vector3 axis) =>
            pivots[pivot].localRotation = rest[pivot] * Quaternion.AngleAxis(angle, axis);

        public void Apply(SparkLevel71EtchPlant plant, float now, float transfer, bool loaded, bool clamped, float metrology)
        {
            if (!enabled) return;
            float dt = Mathf.Max(0, now - previousTime);
            previousTime = now;
            float sweep = transfer > 0 ? Mathf.Sin(Mathf.Clamp01(transfer / 2.65f) * Mathf.PI) : 0;
            pivots[1].localPosition = gateSeat + gateTravel * plant.Gate;
            Turn(2, 38 * sweep, Vector3.up);
            Turn(3, -13 * sweep, Vector3.forward);
            Turn(4, 16 * sweep, Vector3.forward);
            pivots[5].localPosition = liftSeat + liftTravel * sweep;
            pivots[6].localPosition = clampSeat - (clamped ? clampTravel : Vector3.zero);
            turboPhase = Mathf.Repeat(turboPhase + plant.Turbo * 920f * dt, 360);
            backingPhase = Mathf.Repeat(backingPhase + plant.Backing * 570f * dt, 360);
            Turn(7, turboPhase, Vector3.up);
            Turn(8, backingPhase, Vector3.right);
            Turn(9, 80 * plant.Throttle, Vector3.up);
            Turn(10, 85 * plant.FlowA, Vector3.forward);
            Turn(11, 110 * plant.FlowB, Vector3.forward);
            Turn(12, 210 * plant.TuneA, Vector3.up);
            Turn(13, 210 * plant.TuneB, Vector3.up);
            Turn(14, 30 * plant.Activity, Vector3.forward);
            Turn(15, 90 * plant.Isolation, Vector3.up);
            Turn(18, 6 * plant.Reflection, Vector3.forward);
            pivots[20].localPosition = probeSeat - probeTravel * Mathf.Sin(Mathf.Clamp01(metrology / 1.6f) * Mathf.PI);
            pivots[17].gameObject.SetActive(loaded);
            film.gameObject.SetActive(loaded && plant.MeanFilm > 1);
            mask.gameObject.SetActive(loaded);
            carried.gameObject.SetActive(transfer is > .39f and < 2.50f);
            plasma.gameObject.SetActive(plant.Activity > .06f);
            float glow = Mathf.Sqrt(plant.Activity) * (.96f + .04f * Mathf.Sin(now * 10f));
            plasma.localScale = plasmaSize * glow;
        }
    }
}
