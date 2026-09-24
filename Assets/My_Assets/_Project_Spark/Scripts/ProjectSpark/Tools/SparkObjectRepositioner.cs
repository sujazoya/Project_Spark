using UnityEngine;

namespace ProjectSpark.Gameplay
{
    /// <summary>
    /// Advanced physical repositioning component.
    ///
    /// This component ONLY handles physical transform
    /// manipulation.
    ///
    /// It does NOT handle:
    /// - Electrical connections
    /// - SparkTerminal
    /// - Circuit topology
    /// - Switches
    /// - LEDs
    /// - Cable logic
    ///
    /// It can be used on any Project Spark object:
    /// Charger, Power Bank, Mobile, Cable, USB plug, etc.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkObjectRepositioner :
        MonoBehaviour
    {
        #region Inspector

        [Header("Movement")]

        [SerializeField]
        private bool allowMove = true;

        [SerializeField]
        private bool smoothMove = true;

        [SerializeField, Min(0.01f)]
        private float moveSmoothSpeed = 15f;

        [SerializeField]
        private bool useWorldPosition = true;

        [Header("Move Axes")]

        [SerializeField]
        private bool allowMoveX = true;

        [SerializeField]
        private bool allowMoveY = true;

        [SerializeField]
        private bool allowMoveZ = true;

        [Header("Rotation")]

        [SerializeField]
        private bool allowRotate = true;

        [SerializeField]
        private bool smoothRotate = true;

        [SerializeField, Min(0.01f)]
        private float rotateSmoothSpeed = 15f;

        [Header("Rotation Axes")]

        [SerializeField]
        private bool allowRotateX = true;

        [SerializeField]
        private bool allowRotateY = true;

        [SerializeField]
        private bool allowRotateZ = true;

        [Header("Position Snap")]

        [SerializeField]
        private bool usePositionSnap;

        [SerializeField, Min(0.0001f)]
        private float positionSnapStep = 0.01f;

        [Header("Rotation Snap")]

        [SerializeField]
        private bool useRotationSnap;

        [SerializeField, Min(0.1f)]
        private float rotationSnapStep = 15f;

        [Header("Position Limits")]

        [SerializeField]
        private bool usePositionLimits;

        [SerializeField]
        private Vector3 minimumPosition =
            new Vector3(-10f, -10f, -10f);

        [SerializeField]
        private Vector3 maximumPosition =
            new Vector3(10f, 10f, 10f);

        [Header("Rotation Limits")]

        [SerializeField]
        private bool useRotationLimits;

        [SerializeField]
        private Vector3 minimumRotation =
            new Vector3(-360f, -360f, -360f);

        [SerializeField]
        private Vector3 maximumRotation =
            new Vector3(360f, 360f, 360f);

        [Header("Debug")]

        [SerializeField]
        private bool showDebugLog;

        #endregion


        #region Runtime

        private Vector3 initialPosition;
        private Quaternion initialRotation;
        private Vector3 targetPosition;
        private Quaternion targetRotation;

        private bool initialized;

        #endregion


        #region Properties

        public bool AllowMove =>
            allowMove;

        public bool AllowRotate =>
            allowRotate;

        public Vector3 InitialPosition =>
            initialPosition;

        public Quaternion InitialRotation =>
            initialRotation;

        public Vector3 TargetPosition =>
            targetPosition;

        public Quaternion TargetRotation =>
            targetRotation;

        #endregion


        #region Unity

        private void Awake()
        {
            CaptureInitialTransform();
        }

        private void LateUpdate()
        {
            if (!initialized)
                return;

            ApplyMovement();
            ApplyRotation();
        }

        #endregion


        #region Initialization

        public void CaptureInitialTransform()
        {
            initialPosition =
                transform.position;

            initialRotation =
                transform.rotation;

            targetPosition =
                transform.position;

            targetRotation =
                transform.rotation;

            initialized = true;
        }

        #endregion


        #region Position

        public void SetPosition(
            Vector3 position)
        {
            if (!allowMove)
                return;

            Vector3 finalPosition =
                position;

            if (!allowMoveX)
                finalPosition.x =
                    transform.position.x;

            if (!allowMoveY)
                finalPosition.y =
                    transform.position.y;

            if (!allowMoveZ)
                finalPosition.z =
                    transform.position.z;

            finalPosition =
                ApplyPositionSnap(
                    finalPosition);

            finalPosition =
                ApplyPositionLimits(
                    finalPosition);

            targetPosition =
                finalPosition;

            if (!smoothMove)
            {
                transform.position =
                    targetPosition;
            }
        }

        public void Move(
            Vector3 delta)
        {
            SetPosition(
                transform.position +
                delta);
        }

        #endregion


        #region Rotation

        public void SetRotation(
            Quaternion rotation)
        {
            if (!allowRotate)
                return;

            Vector3 euler =
                rotation.eulerAngles;

            Vector3 current =
                transform.eulerAngles;

            if (!allowRotateX)
                euler.x = current.x;

            if (!allowRotateY)
                euler.y = current.y;

            if (!allowRotateZ)
                euler.z = current.z;

            euler =
                ApplyRotationSnap(
                    euler);

            euler =
                ApplyRotationLimits(
                    euler);

            targetRotation =
                Quaternion.Euler(euler);

            if (!smoothRotate)
            {
                transform.rotation =
                    targetRotation;
            }
        }

        public void Rotate(
            Vector3 eulerDelta)
        {
            SetRotation(
                transform.rotation *
                Quaternion.Euler(
                    eulerDelta));
        }

        #endregion


        #region Smooth Application

        private void ApplyMovement()
        {
            if (!allowMove)
                return;

            if (!smoothMove)
                return;

            transform.position =
                Vector3.Lerp(
                    transform.position,
                    targetPosition,
                    1f -
                    Mathf.Exp(
                        -moveSmoothSpeed *
                        Time.deltaTime));
        }

        private void ApplyRotation()
        {
            if (!allowRotate)
                return;

            if (!smoothRotate)
                return;

            transform.rotation =
                Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    1f -
                    Mathf.Exp(
                        -rotateSmoothSpeed *
                        Time.deltaTime));
        }

        #endregion


        #region Position Snap

        private Vector3 ApplyPositionSnap(
            Vector3 position)
        {
            if (!usePositionSnap)
                return position;

            float step =
                Mathf.Max(
                    0.0001f,
                    positionSnapStep);

            position.x =
                Mathf.Round(
                    position.x / step) *
                step;

            position.y =
                Mathf.Round(
                    position.y / step) *
                step;

            position.z =
                Mathf.Round(
                    position.z / step) *
                step;

            return position;
        }

        #endregion


        #region Rotation Snap

        private Vector3 ApplyRotationSnap(
            Vector3 rotation)
        {
            if (!useRotationSnap)
                return rotation;

            float step =
                Mathf.Max(
                    0.1f,
                    rotationSnapStep);

            rotation.x =
                Mathf.Round(
                    NormalizeAngle(
                        rotation.x) /
                    step) *
                step;

            rotation.y =
                Mathf.Round(
                    NormalizeAngle(
                        rotation.y) /
                    step) *
                step;

            rotation.z =
                Mathf.Round(
                    NormalizeAngle(
                        rotation.z) /
                    step) *
                step;

            return rotation;
        }

        #endregion


        #region Position Limits

        private Vector3 ApplyPositionLimits(
            Vector3 position)
        {
            if (!usePositionLimits)
                return position;

            position.x =
                Mathf.Clamp(
                    position.x,
                    minimumPosition.x,
                    maximumPosition.x);

            position.y =
                Mathf.Clamp(
                    position.y,
                    minimumPosition.y,
                    maximumPosition.y);

            position.z =
                Mathf.Clamp(
                    position.z,
                    minimumPosition.z,
                    maximumPosition.z);

            return position;
        }

        #endregion


        #region Rotation Limits

        private Vector3 ApplyRotationLimits(
            Vector3 rotation)
        {
            if (!useRotationLimits)
                return rotation;

            rotation.x =
                Mathf.Clamp(
                    NormalizeAngle(rotation.x),
                    minimumRotation.x,
                    maximumRotation.x);

            rotation.y =
                Mathf.Clamp(
                    NormalizeAngle(rotation.y),
                    minimumRotation.y,
                    maximumRotation.y);

            rotation.z =
                Mathf.Clamp(
                    NormalizeAngle(rotation.z),
                    minimumRotation.z,
                    maximumRotation.z);

            return rotation;
        }

        #endregion


        #region Reset

        public void ResetTransform()
        {
            targetPosition =
                initialPosition;

            targetRotation =
                initialRotation;

            if (!smoothMove)
            {
                transform.position =
                    initialPosition;
            }

            if (!smoothRotate)
            {
                transform.rotation =
                    initialRotation;
            }

            if (showDebugLog)
            {
                Debug.Log(
                    $"[REPOSITION] RESET → {name}",
                    this);
            }
        }

        #endregion


        #region Utility

        private float NormalizeAngle(
            float angle)
        {
            angle %= 360f;

            if (angle > 180f)
                angle -= 360f;

            if (angle < -180f)
                angle += 360f;

            return angle;
        }

        #endregion
    }
}
