using UnityEngine;

namespace ProjectSpark.Gameplay
{
    [DisallowMultipleComponent]
    public sealed class SparkLevel31ArtMotion : MonoBehaviour
    {
        public SparkLevel31SealIn controls;
        public Transform mainRocker, stopCap, startCap, driveBridge, relayArm, motorShaft;
        public Transform[] beltRibs, cargo;
        public Renderer runBeacon;
        public Light runLamp;

        private Vector3 stopRest, startRest;
        private float beltOffset, left, right;
        private float[] ribStart, cargoStart;

        public bool IsRunning { get; private set; }

        private void Awake()
        {
            stopRest = stopCap.localPosition;
            startRest = startCap.localPosition;
            left = float.PositiveInfinity;
            right = float.NegativeInfinity;
            ribStart = new float[beltRibs.Length];
            cargoStart = new float[cargo.Length];
            for (int i = 0; i < beltRibs.Length; i++)
            {
                float x = beltRibs[i].localPosition.x;
                ribStart[i] = x;
                left = Mathf.Min(left, x);
                right = Mathf.Max(right, x);
            }
            for (int i = 0; i < cargo.Length; i++) cargoStart[i] = cargo[i].localPosition.x;
        }

        private void LateUpdate()
        {
            mainRocker.localRotation = Quaternion.Euler(controls.main.IsClosed ? 11f : -11f, 0, 0);
            stopCap.localPosition = stopRest + Vector3.down * (controls.stop.IsOpen ? .0075f : 0);
            startCap.localPosition = startRest + Vector3.down * (controls.start.IsClosed ? .0075f : 0);
            relayArm.localRotation = Quaternion.Euler(0, 0, controls.relay.IsEnergized ? 0 : 16);
            driveBridge.localRotation = Quaternion.Euler(0, 0, controls.runContact.IsClosed ? 0 : 17);
            IsRunning = controls.relay.IsEnergized && controls.runContact.IsClosed && controls.motor.IsRunning;
            runBeacon.enabled = IsRunning;
            runLamp.enabled = IsRunning;
            if (!IsRunning) return;

            motorShaft.Rotate(Vector3.right, controls.motor.ElectricalState.Voltage * 160f * Time.deltaTime, Space.Self);
            float span = right - left;
            if (span <= .001f) return;
            beltOffset = Mathf.Repeat(beltOffset + Time.deltaTime * .38f, span);
            Advance(beltRibs, ribStart, left, right, beltOffset);
            float cargoLeft = left + span * .10f;
            float cargoRight = right - span * .10f;
            Advance(cargo, cargoStart, cargoLeft, cargoRight, beltOffset * .8f);
        }

        private static void Advance(Transform[] pieces, float[] initial, float from, float to, float distance)
        {
            for (int i = 0; i < pieces.Length; i++)
            {
                Vector3 position = pieces[i].localPosition;
                position.x = from + Mathf.Repeat(initial[i] - from + distance, to - from);
                pieces[i].localPosition = position;
            }
        }

        private void OnDisable()
        {
            IsRunning = false;
            if (runBeacon != null) runBeacon.enabled = false;
            if (runLamp != null) runLamp.enabled = false;
        }
    }
}
