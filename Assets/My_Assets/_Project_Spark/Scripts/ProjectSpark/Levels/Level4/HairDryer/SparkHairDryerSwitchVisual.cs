using UnityEngine;

namespace ProjectSpark.Gameplay
{
    /// <summary>
    /// Physical visual animation for a hair-dryer mechanical switch.
    ///
    /// Supports:
    /// - 2-position switches
    /// - 3-position switches
    /// - Local rotation
    /// - Optional local position movement
    /// - Smooth mechanical movement
    /// - Immediate positioning
    ///
    /// Visual only.
    /// Does NOT perform electrical simulation.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkHairDryerSwitchVisual : MonoBehaviour
    {
        [System.Serializable]
        private struct SwitchPosition
        {
            [Tooltip("Optional editor description.")]
            public string name;

            [Tooltip("Local rotation of the switch at this position.")]
            public Vector3 localEulerAngles;

            [Tooltip("Optional local position offset.")]
            public Vector3 localPosition;
        }

        [Header("Switch")]
        [SerializeField]
        private Transform switchTransform;

        [Header("Positions")]
        [SerializeField]
        private SwitchPosition[] positions =
        {
            new SwitchPosition
            {
                name = "Position 0",
                localEulerAngles = Vector3.zero,
                localPosition = Vector3.zero
            },
            new SwitchPosition
            {
                name = "Position 1",
                localEulerAngles = new Vector3(0f, 25f, 0f),
                localPosition = Vector3.zero
            }
        };

        [Header("Animation")]
        [SerializeField]
        [Min(0.01f)]
        private float rotationSpeed = 360f;

        [SerializeField]
        [Min(0.01f)]
        private float positionSpeed = 0.05f;

        [SerializeField]
        private bool usePositionMovement = false;

        [SerializeField]
        private bool useRotationMovement = true;

        [Header("Runtime")]
        [SerializeField]
        private int currentPosition;

        [SerializeField]
        private int targetPosition;

        private Quaternion targetRotation;
        private Vector3 targetLocalPosition;

        public int CurrentPosition => currentPosition;

        public int TargetPosition => targetPosition;

        public int PositionCount =>
            positions != null ? positions.Length : 0;

        public bool IsMoving { get; private set; }

        private void Awake()
        {
            Initialize();
        }

        private void Update()
        {
            if (!IsMoving)
                return;

            bool rotationComplete = true;
            bool positionComplete = true;

            if (useRotationMovement)
            {
                switchTransform.localRotation =
                    Quaternion.RotateTowards(
                        switchTransform.localRotation,
                        targetRotation,
                        rotationSpeed * Time.deltaTime);

                rotationComplete =
                    Quaternion.Angle(
                        switchTransform.localRotation,
                        targetRotation) <= 0.01f;
            }

            if (usePositionMovement)
            {
                switchTransform.localPosition =
                    Vector3.MoveTowards(
                        switchTransform.localPosition,
                        targetLocalPosition,
                        positionSpeed * Time.deltaTime);

                positionComplete =
                    Vector3.Distance(
                        switchTransform.localPosition,
                        targetLocalPosition) <= 0.0001f;
            }

            if (rotationComplete && positionComplete)
            {
                switchTransform.localRotation = targetRotation;

                if (usePositionMovement)
                    switchTransform.localPosition = targetLocalPosition;

                currentPosition = targetPosition;
                IsMoving = false;

                enabled = false;
            }
        }

        private void Initialize()
        {
            if (switchTransform == null)
                switchTransform = transform;

            if (positions == null || positions.Length == 0)
            {
                enabled = false;
                return;
            }

            currentPosition =
                Mathf.Clamp(
                    currentPosition,
                    0,
                    positions.Length - 1);

            targetPosition = currentPosition;

            ApplyPositionImmediate(currentPosition);

            enabled = false;
        }

        public void SetPosition(int index)
        {
            if (switchTransform == null)
                switchTransform = transform;

            if (positions == null || positions.Length == 0)
                return;

            index =
                Mathf.Clamp(
                    index,
                    0,
                    positions.Length - 1);

            targetPosition = index;

            SwitchPosition position =
                positions[targetPosition];

            targetRotation =
                Quaternion.Euler(
                    position.localEulerAngles);

            targetLocalPosition =
                position.localPosition;

            IsMoving = true;
            enabled = true;
        }

        public void SetPositionImmediate(int index)
        {
            if (switchTransform == null)
                switchTransform = transform;

            if (positions == null || positions.Length == 0)
                return;

            index =
                Mathf.Clamp(
                    index,
                    0,
                    positions.Length - 1);

            currentPosition = index;
            targetPosition = index;

            ApplyPositionImmediate(index);

            IsMoving = false;
            enabled = false;
        }

        private void ApplyPositionImmediate(int index)
        {
            SwitchPosition position =
                positions[index];

            if (useRotationMovement)
            {
                switchTransform.localRotation =
                    Quaternion.Euler(
                        position.localEulerAngles);
            }

            if (usePositionMovement)
            {
                switchTransform.localPosition =
                    position.localPosition;
            }
        }

        [ContextMenu("Test / Position 0")]
        private void TestPosition0()
        {
            SetPosition(0);
        }

        [ContextMenu("Test / Position 1")]
        private void TestPosition1()
        {
            if (PositionCount > 1)
                SetPosition(1);
        }

        [ContextMenu("Test / Position 2")]
        private void TestPosition2()
        {
            if (PositionCount > 2)
                SetPosition(2);
        }

        [ContextMenu("Test / Position 3")]
        private void TestPosition3()
        {
            if (PositionCount > 3)
                SetPosition(3);
        }
    }
}