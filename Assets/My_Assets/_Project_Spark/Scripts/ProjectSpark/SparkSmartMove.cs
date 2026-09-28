using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace ProjectSpark.Gameplay
{
    /// <summary>
    /// Standalone two-click smart movement for one specific object.
    ///
    /// Behavior:
    /// 1. Click this object -> movement mode activates.
    /// 2. Click another object -> this object smoothly moves to a
    ///    Vector3 position calculated from the target position + offset.
    /// 3. Movement mode immediately deactivates.
    ///
    /// This component does not modify or depend on the existing
    /// Project Spark selection, camera, viewer rotation, or gizmo systems.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkSmartMove : MonoBehaviour
    {
        // ============================================================
        // TARGET
        // ============================================================

        [Header("Movement Target")]

        [Tooltip("Transform that will be moved. Defaults to this object.")]
        [SerializeField]
        private Transform movementTarget;


        [Header("Target Offset")]

        [Tooltip(
            "World-space Vector3 offset added to the clicked target position. " +
            "Example: X = 0.5, Y = 0.2, Z = -0.3.")]
        [SerializeField]
        private Vector3 targetOffset = new Vector3(0.5f, 0.2f, -0.3f);


        // ============================================================
        // CLICK DETECTION
        // ============================================================

        [Header("Click Detection")]

        [Tooltip("Camera used for click raycasts. Defaults to Main Camera.")]
        [SerializeField]
        private Camera inputCamera;

        [Tooltip("Layers containing valid movement destinations.")]
        [SerializeField]
        private LayerMask targetLayers = ~0;

        [Tooltip("Maximum click raycast distance.")]
        [SerializeField]
        [Min(0.01f)]
        private float raycastDistance = 1000f;

        [Tooltip("Ignore clicks over UI.")]
        [SerializeField]
        private bool ignoreUI = true;

        [Tooltip("Maximum pointer movement allowed for a click.")]
        [SerializeField]
        [Min(0f)]
        private float clickThreshold = 5f;


        // ============================================================
        // MOVEMENT
        // ============================================================

        [Header("Smooth Movement")]

        [Tooltip("SmoothDamp time used when moving to the destination.")]
        [SerializeField]
        [Min(0.01f)]
        private float smoothTime = 0.2f;

        [Tooltip("Maximum movement speed.")]
        [SerializeField]
        [Min(0f)]
        private float maxSpeed = 100f;

        [Tooltip(
            "Distance from the Vector3 destination at which movement " +
            "is considered complete.")]
        [SerializeField]
        [Min(0.0001f)]
        private float stopDistance = 0.001f;


        // ============================================================
        // RUNTIME
        // ============================================================

        private Vector3 movementVelocity;

        private Vector3 targetPosition;

        private Transform destinationTransform;

        private Vector2 pointerDownPosition;

        private bool pointerDown;

        private bool moveModeActive;

        private bool moving;


        // ============================================================
        // PUBLIC STATE
        // ============================================================

        /// <summary>
        /// Returns true when this object is waiting for a destination click.
        /// </summary>
        public bool IsMoveModeActive
        {
            get
            {
                return moveModeActive;
            }
        }

        /// <summary>
        /// Returns true while the object is moving.
        /// </summary>
        public bool IsMoving
        {
            get
            {
                return moving;
            }
        }

        /// <summary>
        /// The object that was selected as the movement destination.
        /// </summary>
        public Transform Destination
        {
            get
            {
                return destinationTransform;
            }
        }

        /// <summary>
        /// Current calculated Vector3 movement destination.
        /// </summary>
        public Vector3 TargetPosition
        {
            get
            {
                return targetPosition;
            }
        }

        /// <summary>
        /// Current X/Y/Z offset applied to the clicked target position.
        /// </summary>
        public Vector3 TargetOffset
        {
            get
            {
                return targetOffset;
            }
        }


        // ============================================================
        // UNITY
        // ============================================================

        private void Awake()
        {
            ResolveReferences();
        }

        private void Update()
        {
            HandlePointer();

            if (moving)
            {
                UpdateMovement();
            }
        }

        private void OnDisable()
        {
            StopMovement();

            moveModeActive = false;
            pointerDown = false;
        }


        // ============================================================
        // POINTER
        // ============================================================

        private void HandlePointer()
        {
#if ENABLE_INPUT_SYSTEM

            if (Mouse.current == null)
            {
                return;
            }

            Vector2 pointer =
                Mouse.current.position.ReadValue();

            // --------------------------------------------------------
            // Mouse Down
            // --------------------------------------------------------

            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                if (ignoreUI &&
                    IsPointerOverUI())
                {
                    return;
                }

                pointerDownPosition =
                    pointer;

                pointerDown =
                    true;
            }

            // --------------------------------------------------------
            // Mouse Up
            // --------------------------------------------------------

            if (Mouse.current.leftButton.wasReleasedThisFrame)
            {
                if (!pointerDown)
                {
                    return;
                }

                pointerDown =
                    false;

                Vector2 delta =
                    pointer -
                    pointerDownPosition;

                float thresholdSqr =
                    clickThreshold *
                    clickThreshold;

                if (delta.sqrMagnitude >
                    thresholdSqr)
                {
                    return;
                }

                HandleClick(pointer);
            }

#endif
        }


        // ============================================================
        // CLICK
        // ============================================================

        private void HandleClick(
            Vector2 screenPosition)
        {
            ResolveReferences();

            if (inputCamera == null)
            {
                return;
            }

            Ray ray =
                inputCamera.ScreenPointToRay(
                    screenPosition);

            if (!Physics.Raycast(
                    ray,
                    out RaycastHit hit,
                    raycastDistance,
                    targetLayers,
                    QueryTriggerInteraction.Ignore))
            {
                return;
            }

            Transform clickedObject =
                hit.collider.transform;

            if (clickedObject == null)
            {
                return;
            }

            // ========================================================
            // MOVE MODE IS OFF
            // ========================================================

            if (!moveModeActive)
            {
                if (IsThisObject(clickedObject))
                {
                    ActivateMoveMode();
                }

                return;
            }

            // ========================================================
            // MOVE MODE IS ON
            // ========================================================

            if (IsThisObject(clickedObject))
            {
                // Clicking the source again does nothing.
                // It remains in move mode.
                return;
            }

            // --------------------------------------------------------
            // Second object = destination
            // --------------------------------------------------------

            if (MoveTo(clickedObject))
            {
                // Deactivate immediately after destination is selected.
                moveModeActive = false;
            }
        }


        // ============================================================
        // MOVE MODE
        // ============================================================

        private void ActivateMoveMode()
        {
            moveModeActive =
                true;
        }


        /// <summary>
        /// Cancels the active move mode without moving the object.
        /// </summary>
        public void CancelMoveMode()
        {
            moveModeActive =
                false;
        }


        // ============================================================
        // SOURCE CHECK
        // ============================================================

        private bool IsThisObject(
            Transform clickedObject)
        {
            if (clickedObject == null)
            {
                return false;
            }

            if (movementTarget == null)
            {
                return clickedObject == transform ||
                       clickedObject.IsChildOf(transform);
            }

            return clickedObject == movementTarget ||
                   clickedObject.IsChildOf(movementTarget) ||
                   movementTarget.IsChildOf(clickedObject);
        }


        // ============================================================
        // MOVEMENT
        // ============================================================

        private bool MoveTo(
            Transform destination)
        {
            if (movementTarget == null)
            {
                movementTarget =
                    transform;
            }

            if (destination == null)
            {
                return false;
            }

            if (destination == movementTarget)
            {
                return false;
            }

            // --------------------------------------------------------
            // Store the clicked object for reference.
            // Movement itself uses only the calculated Vector3.
            // --------------------------------------------------------

            destinationTransform =
                destination;

            // --------------------------------------------------------
            // Calculate the final Vector3 destination ONCE.
            //
            // Example:
            //
            // Target position = (10, 5, 2)
            // Offset          = (0.5, 0.2, -0.3)
            //
            // Final position  = (10.5, 5.2, 1.7)
            // --------------------------------------------------------

            targetPosition =
                destination.position +
                targetOffset;

            movementVelocity =
                Vector3.zero;

            moving =
                true;

            return true;
        }


        private void UpdateMovement()
        {
            if (movementTarget == null)
            {
                StopMovement();
                return;
            }

            // --------------------------------------------------------
            // Move toward the stored Vector3.
            //
            // The destination Transform is NOT used here.
            // --------------------------------------------------------

            movementTarget.position =
                Vector3.SmoothDamp(
                    movementTarget.position,
                    targetPosition,
                    ref movementVelocity,
                    smoothTime,
                    maxSpeed,
                    Time.deltaTime);

            // --------------------------------------------------------
            // Finish when close enough.
            // --------------------------------------------------------

            float distance =
                Vector3.Distance(
                    movementTarget.position,
                    targetPosition);

            if (distance <= stopDistance)
            {
                FinishMovement();
            }
        }


        private void FinishMovement()
        {
            if (movementTarget != null)
            {
                // Make sure the final position is exactly the
                // calculated Vector3 destination.
                movementTarget.position =
                    targetPosition;
            }

            movementVelocity =
                Vector3.zero;

            destinationTransform =
                null;

            moving =
                false;
        }


        /// <summary>
        /// Immediately stops movement.
        /// </summary>
        public void StopMovement()
        {
            movementVelocity =
                Vector3.zero;

            destinationTransform =
                null;

            moving =
                false;
        }


        // ============================================================
        // REFERENCES
        // ============================================================

        private void ResolveReferences()
        {
            if (movementTarget == null)
            {
                movementTarget =
                    transform;
            }

            if (inputCamera == null)
            {
                inputCamera =
                    Camera.main;
            }
        }


        // ============================================================
        // UI
        // ============================================================

        private bool IsPointerOverUI()
        {
            if (!ignoreUI)
            {
                return false;
            }

            return UnityEngine.EventSystems.EventSystem.current != null &&
                   UnityEngine.EventSystems.EventSystem.current
                       .IsPointerOverGameObject();
        }


        // ============================================================
        // VALIDATION
        // ============================================================

        private void OnValidate()
        {
            if (smoothTime < 0.01f)
            {
                smoothTime = 0.01f;
            }

            if (maxSpeed < 0f)
            {
                maxSpeed = 0f;
            }

            if (stopDistance < 0.0001f)
            {
                stopDistance = 0.0001f;
            }

            if (raycastDistance < 0.01f)
            {
                raycastDistance = 0.01f;
            }

            if (clickThreshold < 0f)
            {
                clickThreshold = 0f;
            }
        }
    }
}