using UnityEngine;

namespace ProjectSpark.HolographicViewer
{
    /// <summary>
    /// Handles physical manipulation of one selected component.
    ///
    /// This class does not own selection.
    /// It only manipulates the Transform supplied by the caller.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class HolographicComponentManipulator
        : MonoBehaviour
    {
        public enum ManipulationMode
        {
            None,
            Move,
            Rotate
        }

        // ============================================================
        // REFERENCES
        // ============================================================

        [Header("References")]
        [SerializeField]
        private Camera viewerCamera;

        [SerializeField]
        private HolographicViewerMouseInput viewerMouseInput;

        // ============================================================
        // MOVE
        // ============================================================

        [Header("Move")]
        [SerializeField]
        private bool allowMove = true;

        [SerializeField]
        private bool preserveGrabOffset = true;

        // ============================================================
        // ROTATE
        // ============================================================

        [Header("Rotate")]
        [SerializeField]
        private bool allowRotate = true;

        [SerializeField]
        private Vector3 rotationAxis = Vector3.up;

        [SerializeField, Min(0.001f)]
        private float rotationSensitivity = 0.25f;

        [SerializeField, Min(0f)]
        private float rotationSnap = 0f;

        // ============================================================
        // RUNTIME
        // ============================================================

        private Transform activeTarget;

        private ManipulationMode mode =
            ManipulationMode.None;

        private Vector3 originalPosition;

        private Quaternion originalRotation;

        private Vector3 grabOffset;

        private Plane manipulationPlane;

        private Vector3 worldRotationAxis;

        private float accumulatedRotation;

        private Vector2 rotationStartScreenPosition;

        private bool sessionActive;

        // ============================================================
        // PUBLIC STATE
        // ============================================================

        public bool IsManipulating
        {
            get { return sessionActive; }
        }

        public ManipulationMode CurrentMode
        {
            get { return mode; }
        }

        public Transform ActiveTarget
        {
            get { return activeTarget; }
        }

        // ============================================================
        // INITIALIZATION
        // ============================================================

        private void Awake()
        {
            ResolveReferences();
        }

        private void ResolveReferences()
        {
            if (viewerCamera == null)
            {
                viewerCamera = Camera.main;
            }
        }

        // ============================================================
        // BEGIN MOVE
        // ============================================================

        public bool BeginMove(
            Transform target,
            Vector2 screenPosition,
            out string reason)
        {
            reason = null;

            if (!allowMove)
            {
                reason =
                    "Component movement is disabled.";

                return false;
            }

            if (target == null)
            {
                reason =
                    "Movement target is missing.";

                return false;
            }

            if (sessionActive)
            {
                reason =
                    "Another manipulation is already active.";

                return false;
            }

            return InitializeSession(
                target,
                ManipulationMode.Move,
                screenPosition,
                out reason);
        }

        // ============================================================
        // BEGIN ROTATE
        // ============================================================

        public bool BeginRotate(
            Transform target,
            Vector2 screenPosition,
            out string reason)
        {
            reason = null;

            if (!allowRotate)
            {
                reason =
                    "Component rotation is disabled.";

                return false;
            }

            if (target == null)
            {
                reason =
                    "Rotation target is missing.";

                return false;
            }

            if (sessionActive)
            {
                reason =
                    "Another manipulation is already active.";

                return false;
            }

            if (rotationAxis.sqrMagnitude <
                0.000001f)
            {
                reason =
                    "Rotation axis is invalid.";

                return false;
            }

            return InitializeSession(
                target,
                ManipulationMode.Rotate,
                screenPosition,
                out reason);
        }

        // ============================================================
        // UPDATE
        // ============================================================

        public void UpdateManipulation(
            Vector2 screenPosition)
        {
            if (!sessionActive ||
                activeTarget == null)
            {
                return;
            }

            switch (mode)
            {
                case ManipulationMode.Move:

                    UpdateMove(
                        screenPosition);

                    break;

                case ManipulationMode.Rotate:

                    UpdateRotate(
                        screenPosition);

                    break;
            }
        }

        // ============================================================
        // COMMIT
        // ============================================================

        public void Commit()
        {
            if (!sessionActive)
            {
                return;
            }

            EndSession();
        }

        // ============================================================
        // CANCEL
        // ============================================================

        public void Cancel()
        {
            if (!sessionActive)
            {
                return;
            }

            if (activeTarget != null)
            {
                activeTarget.position =
                    originalPosition;

                activeTarget.rotation =
                    originalRotation;
            }

            EndSession();
        }

        // ============================================================
        // INITIALIZE
        // ============================================================

        private bool InitializeSession(
            Transform target,
            ManipulationMode requestedMode,
            Vector2 screenPosition,
            out string reason)
        {
            reason = null;

            ResolveReferences();

            if (viewerCamera == null)
            {
                reason =
                    "Viewer camera is unavailable.";

                return false;
            }

            activeTarget = target;

            originalPosition =
                target.position;

            originalRotation =
                target.rotation;

            accumulatedRotation = 0f;

            // --------------------------------------------------------
            // MOVE
            // --------------------------------------------------------

            if (requestedMode ==
                ManipulationMode.Move)
            {
                manipulationPlane =
                    new Plane(
                        viewerCamera.transform.forward,
                        target.position);

                if (preserveGrabOffset)
                {
                    if (!TryProjectPointer(
                            screenPosition,
                            out Vector3 point))
                    {
                        ResetRuntimeState();

                        reason =
                            "Unable to establish manipulation point.";

                        return false;
                    }

                    grabOffset =
                        target.position - point;
                }
                else
                {
                    grabOffset =
                        Vector3.zero;
                }
            }

            // --------------------------------------------------------
            // ROTATE
            // --------------------------------------------------------

            if (requestedMode ==
                ManipulationMode.Rotate)
            {
                rotationStartScreenPosition =
                    screenPosition;

                worldRotationAxis =
                    target.TransformDirection(
                        rotationAxis.normalized);
            }

            mode =
                requestedMode;

            sessionActive = true;

            LockViewerControls(true);

            return true;
        }

        // ============================================================
        // MOVE
        // ============================================================

        private void UpdateMove(
            Vector2 screenPosition)
        {
            if (!TryProjectPointer(
                    screenPosition,
                    out Vector3 point))
            {
                return;
            }

            Vector3 desiredPosition;

            if (preserveGrabOffset)
            {
                desiredPosition =
                    point +
                    grabOffset;
            }
            else
            {
                desiredPosition =
                    point;
            }

            activeTarget.position =
                desiredPosition;
        }

        // ============================================================
        // ROTATE
        // ============================================================

        private void UpdateRotate(
            Vector2 screenPosition)
        {
            Vector2 delta =
                screenPosition -
                rotationStartScreenPosition;

            float projectedDelta;

            if (Mathf.Abs(delta.x) >=
                Mathf.Abs(delta.y))
            {
                projectedDelta =
                    delta.x;
            }
            else
            {
                projectedDelta =
                    -delta.y;
            }

            float angle =
                projectedDelta *
                rotationSensitivity;

            if (rotationSnap > 0f)
            {
                angle =
                    Mathf.Round(
                        angle /
                        rotationSnap) *
                    rotationSnap;
            }

            accumulatedRotation =
                angle;

            activeTarget.rotation =
                originalRotation *
                Quaternion.AngleAxis(
                    accumulatedRotation,
                    worldRotationAxis);
        }

        // ============================================================
        // POINTER → WORLD
        // ============================================================

        private bool TryProjectPointer(
            Vector2 screenPosition,
            out Vector3 point)
        {
            point = default;

            if (viewerCamera == null)
            {
                return false;
            }

            Ray ray =
                viewerCamera.ScreenPointToRay(
                    screenPosition);

            if (!manipulationPlane.Raycast(
                    ray,
                    out float enter))
            {
                return false;
            }

            if (enter < 0f)
            {
                return false;
            }

            point =
                ray.GetPoint(enter);

            return true;
        }

        // ============================================================
        // END
        // ============================================================

        private void EndSession()
        {
            sessionActive = false;

            activeTarget = null;

            mode =
                ManipulationMode.None;

            accumulatedRotation = 0f;

            grabOffset = Vector3.zero;

            worldRotationAxis = Vector3.zero;

            LockViewerControls(false);
        }

        // ============================================================
        // RESET
        // ============================================================

        private void ResetRuntimeState()
        {
            sessionActive = false;

            activeTarget = null;

            mode =
                ManipulationMode.None;

            accumulatedRotation = 0f;

            grabOffset = Vector3.zero;

            worldRotationAxis = Vector3.zero;
        }

        // ============================================================
        // VIEWER CONTROL
        // ============================================================

        private void LockViewerControls(
            bool locked)
        {
            if (viewerMouseInput == null)
            {
                return;
            }

            viewerMouseInput.SetScreenSpaceControl(
                !locked);
        }

        // ============================================================
        // CLEANUP
        // ============================================================

        private void OnDisable()
        {
            if (sessionActive)
            {
                Cancel();
                return;
            }

            LockViewerControls(false);
        }
    }
}