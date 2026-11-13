using UnityEngine;

namespace ProjectSpark.Gameplay
{
    /// <summary>
    /// Level 43 VISUAL adapter ONLY. Does not perform electrical solving,
    /// change topology or determine game-objective completion.
    /// Feed it the authoritative position and drive direction from your
    /// existing Level 43 gameplay controller after the solver updates.
    ///
    /// The carriage and sensor wiper are separately rigged in the FBX files.
    /// Assign their parent or the named Move_X hierarchy objects here.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkLevel43VisualMotionRig : MonoBehaviour
    {
        [Header("Mesh Animation Targets")]
        [SerializeField] private Transform carriageMoveRoot;
        [SerializeField] private Transform sensorWiperMoveRoot;
        [SerializeField] private Transform carriagePointerMoveRoot;
        [SerializeField] private Transform motorShaftRotateRoot;

        [Header("Travel Matches Level 43 Art")]
        [SerializeField] private float localMinX = -5.2f;
        [SerializeField] private float localMaxX =  5.2f;
        [SerializeField, Range(0f, 1f)] private float previewPosition01 = 0.5f;
        [SerializeField] private bool initializeAtPreviewPosition = true;

        [Header("Rotor Visual Only")]
        [SerializeField, Min(0f)] private float visualShaftRpm = 380f;

        private Vector3 carriageOrigin;
        private Vector3 wiperOrigin;
        private Vector3 pointerOrigin;
        private Quaternion shaftOrigin;
        private float lastAppliedPosition01;
        private float driveDirection;
        private bool initialized;

        public float NormalizedPosition => lastAppliedPosition01;

        private void Awake()
        {
            CaptureInitialTransforms();
            if (initializeAtPreviewPosition)
                SetNormalizedPosition(previewPosition01);
        }

        public void CaptureInitialTransforms()
        {
            if (initialized) return;
            if (carriageMoveRoot != null) carriageOrigin = carriageMoveRoot.localPosition;
            if (sensorWiperMoveRoot != null) wiperOrigin = sensorWiperMoveRoot.localPosition;
            if (carriagePointerMoveRoot != null) pointerOrigin = carriagePointerMoveRoot.localPosition;
            if (motorShaftRotateRoot != null) shaftOrigin = motorShaftRotateRoot.localRotation;
            initialized = true;
        }

        /// <summary>
        /// External level gameplay calls this with measured carriage position (0..1).
        /// The art does not independently simulate motion.
        /// </summary>
        public void SetNormalizedPosition(float position01)
        {
            CaptureInitialTransforms();
            position01 = Mathf.Clamp01(position01);
            lastAppliedPosition01 = position01;
            float offset = Mathf.Lerp(localMinX, localMaxX, position01);
            if (carriageMoveRoot != null)
                carriageMoveRoot.localPosition = carriageOrigin + Vector3.right * offset;
            if (sensorWiperMoveRoot != null)
                sensorWiperMoveRoot.localPosition = wiperOrigin + Vector3.right * offset;
            if (carriagePointerMoveRoot != null)
                carriagePointerMoveRoot.localPosition = pointerOrigin + Vector3.right * offset;
        }

        /// <summary>
        /// -1 = drive left; 0 = stopped; +1 = drive right (art-local X convention).
        /// This method does not energize or reverse an actual motor.
        /// </summary>
        public void SetDriveDirection(float direction)
        {
            driveDirection = Mathf.Clamp(direction, -1f, 1f);
        }

        public void StopVisualShaft() => SetDriveDirection(0f);

        private void Update()
        {
            if (motorShaftRotateRoot == null || Mathf.Abs(driveDirection) < 0.001f)
                return;

            motorShaftRotateRoot.Rotate(
                Vector3.right,
                driveDirection * visualShaftRpm * 6f * Time.deltaTime,
                Space.Self);
        }

        [ContextMenu("Visual Preview / Station A 25 Percent")]
        private void PreviewA() => SetNormalizedPosition(0.25f);

        [ContextMenu("Visual Preview / Station B 75 Percent")]
        private void PreviewB() => SetNormalizedPosition(0.75f);

        [ContextMenu("Visual Preview / Midpoint")]
        private void PreviewMiddle() => SetNormalizedPosition(0.5f);

        private void OnValidate()
        {
            visualShaftRpm = Mathf.Max(0f, visualShaftRpm);
            if (localMaxX <= localMinX) localMaxX = localMinX + 0.001f;
        }
    }
}
