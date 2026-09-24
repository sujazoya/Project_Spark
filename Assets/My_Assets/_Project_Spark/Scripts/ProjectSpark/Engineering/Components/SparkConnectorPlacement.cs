
using UnityEngine;

namespace ProjectSpark.Gameplay
{
    /// <summary>
    /// Generic physical connector placement tool.
    ///
    /// Attach this to a USB plug / connector object.
    ///
    /// The connector can:
    /// - Be picked up
    /// - Be moved by the mouse
    /// - Rotate with the mouse wheel
    /// - Detect a compatible jack
    /// - Snap into the jack
    /// - Be removed and repositioned
    ///
    /// This component does NOT perform electrical simulation.
    /// Electrical topology remains handled by the existing
    /// Project Spark circuit systems.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkConnectorPlacement :
        MonoBehaviour
    {
        #region Inspector

        [Header("Connector")]
        [SerializeField]
        private SparkConnectorType connectorType =
            SparkConnectorType.USB_A;

        [SerializeField]
        private Transform connectorTip;

        [Header("Placement")]
        [SerializeField]
        private float snapDistance = 0.15f;

        [SerializeField]
        private float rotationSpeed = 90f;

        [SerializeField]
        private LayerMask jackLayer = ~0;

        [Header("Movement")]
        [SerializeField]
        private Camera targetCamera;

        [SerializeField]
        private float movementPlaneHeight = 0f;

        [SerializeField]
        private float maxMoveDistance = 10f;

        [Header("Options")]
        [SerializeField]
        private bool rotateWithMouseWheel = true;

        [SerializeField]
        private bool allowReposition = true;

        [SerializeField]
        private bool snapOnRelease = true;

        [Header("Debug")]
        [SerializeField]
        private bool showDebugLog;

        #endregion


        #region Runtime

        private bool isDragging;

        private Plane movementPlane;

        private SparkConnectorJack currentJack;

        private Vector3 grabOffset;

        #endregion


        #region Properties

        public SparkConnectorType ConnectorType =>
            connectorType;

        public SparkConnectorJack CurrentJack =>
            currentJack;

        public bool IsPlaced =>
            currentJack != null;

        public bool IsDragging =>
            isDragging;

        #endregion


        #region Unity

        private void Awake()
        {
            if (targetCamera == null)
                targetCamera = Camera.main;

            CreateMovementPlane();
        }

        private void Update()
        {
            if (!isDragging)
                return;

            MoveConnector();

            if (rotateWithMouseWheel)
                RotateConnector();
        }

        #endregion


        #region Movement Plane

        private void CreateMovementPlane()
        {
            movementPlane =
                new Plane(
                    Vector3.up,
                    new Vector3(
                        0f,
                        movementPlaneHeight,
                        0f));
        }

        #endregion


        #region Mouse

        private void OnMouseDown()
        {
            if (!allowReposition &&
                IsPlaced)
            {
                return;
            }

            BeginPlacement();
        }

        private void OnMouseUp()
        {
            EndPlacement();
        }

        #endregion


        #region Begin Placement

        public void BeginPlacement()
        {
            if (targetCamera == null)
                targetCamera = Camera.main;

            if (targetCamera == null)
                return;

            if (IsPlaced)
            {
                DisconnectFromJack();
            }

            isDragging = true;

            Vector3 mousePosition =
                GetMouseWorldPosition();

            grabOffset =
                transform.position -
                mousePosition;

            if (showDebugLog)
            {
                Debug.Log(
                    $"[CONNECTOR] " +
                    $"BEGIN → {connectorType}",
                    this);
            }
        }

        #endregion


        #region End Placement

        public void EndPlacement()
        {
            if (!isDragging)
                return;

            isDragging = false;

            if (snapOnRelease)
            {
                TrySnapToNearestJack();
            }

            if (showDebugLog)
            {
                Debug.Log(
                    IsPlaced
                        ? $"[CONNECTOR] PLACED → {currentJack.JackName}"
                        : "[CONNECTOR] RELEASED → NO JACK",
                    this);
            }
        }

        #endregion


        #region Move

        private void MoveConnector()
        {
            Vector3 position =
                GetMouseWorldPosition();

            position += grabOffset;

            transform.position =
                position;
        }

        private Vector3 GetMouseWorldPosition()
        {
            if (targetCamera == null)
                return transform.position;

            Ray ray =
                targetCamera.ScreenPointToRay(
                    Input.mousePosition);

            if (!movementPlane.Raycast(
                    ray,
                    out float distance))
            {
                return transform.position;
            }

            Vector3 position =
                ray.GetPoint(distance);

            Vector3 center =
                Vector3.zero;

            Vector3 offset =
                position - center;

            if (offset.magnitude >
                maxMoveDistance)
            {
                offset =
                    offset.normalized *
                    maxMoveDistance;
            }

            return center + offset;
        }

        #endregion


        #region Rotation

        private void RotateConnector()
        {
            float wheel =
                Input.mouseScrollDelta.y;

            if (Mathf.Abs(wheel) <
                0.01f)
            {
                return;
            }

            transform.Rotate(
                Vector3.up,
                wheel *
                rotationSpeed *
                Time.deltaTime,
                Space.World);
        }

        #endregion


        #region Jack Detection

        private void TrySnapToNearestJack()
        {
            SparkConnectorJack jack =
                FindNearestCompatibleJack();

            if (jack == null)
                return;

            SnapToJack(jack);
        }

        private SparkConnectorJack
            FindNearestCompatibleJack()
        {
            Vector3 searchPosition =
                connectorTip != null
                    ? connectorTip.position
                    : transform.position;

            Collider[] colliders =
                Physics.OverlapSphere(
                    searchPosition,
                    snapDistance,
                    jackLayer,
                    QueryTriggerInteraction.Collide);

            SparkConnectorJack nearest =
                null;

            float nearestDistance =
                float.MaxValue;

            for (int i = 0;
                 i < colliders.Length;
                 i++)
            {
                if (colliders[i] == null)
                    continue;

                SparkConnectorJack jack =
                    colliders[i]
                        .GetComponentInParent<
                            SparkConnectorJack>();

                if (jack == null)
                    continue;

                if (!jack.CanAccept(
                        connectorType))
                {
                    continue;
                }

                float distance =
                    Vector3.Distance(
                        searchPosition,
                        jack.transform.position);

                if (distance <
                    nearestDistance)
                {
                    nearestDistance =
                        distance;

                    nearest =
                        jack;
                }
            }

            return nearest;
        }

        #endregion


        #region Snap

        private void SnapToJack(
            SparkConnectorJack jack)
        {
            if (jack == null)
                return;

            currentJack =
                jack;

            Transform socket =
                jack.SocketTransform;

            if (socket != null)
            {
                transform.position =
                    socket.position;

                transform.rotation =
                    socket.rotation;
            }
            else
            {
                transform.position =
                    jack.transform.position;
            }

            jack.Attach(this);

            if (showDebugLog)
            {
                Debug.Log(
                    $"[CONNECTOR] " +
                    $"{connectorType} → " +
                    $"{jack.JackName}",
                    this);
            }
        }

        #endregion


        #region Disconnect

        public void DisconnectFromJack()
        {
            if (currentJack == null)
                return;

            SparkConnectorJack oldJack =
                currentJack;

            currentJack = null;

            oldJack.Detach(this);
        }

        #endregion


        #region Gizmos

        private void OnDrawGizmosSelected()
        {
            Vector3 position =
                connectorTip != null
                    ? connectorTip.position
                    : transform.position;

            Gizmos.DrawWireSphere(
                position,
                snapDistance);
        }

        #endregion
    }


    public enum SparkConnectorType
    {
        USB_A,
        USB_C,
        MICRO_USB,
        LIGHTNING
    }
}
