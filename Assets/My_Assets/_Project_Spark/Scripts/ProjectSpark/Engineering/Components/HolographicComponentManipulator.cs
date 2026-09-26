using UnityEngine;

namespace ProjectSpark.HolographicViewer
{
    /// <summary>
    /// Performs physical manipulation of a single Transform supplied
    /// by the caller.
    ///
    /// This component does not own:
    /// - selection
    /// - tool activation
    /// - gizmo visuals
    /// - mouse input
    ///
    /// Rotation is driven by the actual angular movement of the
    /// mouse around the selected gizmo axis.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class HolographicComponentManipulator : MonoBehaviour
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

        [SerializeField, Min(0.001f)]
        private float rotationSensitivity = 1f;

        [SerializeField, Min(0f)]
        private float rotationSnap = 0f;

        [SerializeField, Min(0.0001f)]
        private float minimumRotationRadius = 0.001f;

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

private float rawAccumulatedRotation;

private float previousPointerAngle;

private bool hasPreviousPointerAngle;

private Vector3 previousRotationVector;
private bool hasPreviousRotationVector;

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

        public Vector3 ActiveRotationAxis
        {
            get { return worldRotationAxis; }
        }

        public float AccumulatedRotation
        {
            get { return accumulatedRotation; }
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

            if (viewerMouseInput == null)
            {
                viewerMouseInput =
                    GetComponentInParent<HolographicViewerMouseInput>();
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

            return InitializeMoveSession(
                target,
                screenPosition,
                out reason);
        }

        // ============================================================
        // BEGIN ROTATE
        // ============================================================

        public bool BeginRotate(
            Transform target,
            Vector3 worldAxis,
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

            if (worldAxis.sqrMagnitude < 0.000001f)
            {
                reason =
                    "Rotation axis is invalid.";

                return false;
            }

            return InitializeRotateSession(
                target,
                worldAxis,
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

                    UpdateMove(screenPosition);

                    break;

                case ManipulationMode.Rotate:

                    UpdateRotate(screenPosition);

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
        // INITIALIZE MOVE
        // ============================================================

        private bool InitializeMoveSession(
            Transform target,
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

            activeTarget =
                target;

            originalPosition =
                target.position;

            originalRotation =
                target.rotation;

            accumulatedRotation =
                0f;

            rawAccumulatedRotation =
                0f;

            worldRotationAxis =
                Vector3.zero;

            previousRotationVector =
                Vector3.zero;

            hasPreviousRotationVector =
                false;

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

            rotationStartScreenPosition =
                screenPosition;

            mode =
                ManipulationMode.Move;

            sessionActive =
                true;

            LockViewerControls(true);

            return true;
        }

        // ============================================================
        // INITIALIZE ROTATE
        // ============================================================

       private bool InitializeRotateSession(
    Transform target,
    Vector3 requestedWorldAxis,
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

    if (target == null)
    {
        reason =
            "Rotation target is missing.";

        return false;
    }

    Vector3 normalizedAxis =
        requestedWorldAxis.normalized;

    if (normalizedAxis.sqrMagnitude < 0.999f)
    {
        reason =
            "Rotation axis is invalid.";

        return false;
    }

    activeTarget =
        target;

    originalPosition =
        target.position;

    originalRotation =
        target.rotation;

    worldRotationAxis =
        normalizedAxis;

    accumulatedRotation =
        0f;

    rawAccumulatedRotation =
        0f;

    rotationStartScreenPosition =
        screenPosition;

    hasPreviousPointerAngle =
        TryGetPointerAngle(
            screenPosition,
            out previousPointerAngle);

    if (!hasPreviousPointerAngle)
    {
        ResetRuntimeState();

        reason =
            "Unable to establish rotation pointer.";

        return false;
    }

    grabOffset =
        Vector3.zero;

    mode =
        ManipulationMode.Rotate;

    sessionActive =
        true;

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
    if (!sessionActive ||
        activeTarget == null)
    {
        return;
    }

    if (!hasPreviousPointerAngle)
    {
        return;
    }

    if (!TryGetPointerAngle(
            screenPosition,
            out float currentAngle))
    {
        return;
    }

    float delta =
        Mathf.DeltaAngle(
            previousPointerAngle,
            currentAngle);

    /*
     * This is the actual amount the mouse moved around
     * the gizmo pivot on screen.
     */
    delta *=
        rotationSensitivity;

    rawAccumulatedRotation +=
        delta;

    previousPointerAngle =
        currentAngle;

    float finalAngle =
        rawAccumulatedRotation;

    if (rotationSnap > 0f)
    {
        finalAngle =
            Mathf.Round(
                finalAngle /
                rotationSnap) *
            rotationSnap;
    }

    accumulatedRotation =
        finalAngle;

    /*
     * IMPORTANT:
     *
     * Position is NEVER changed here.
     *
     * Only rotation is changed.
     */
    activeTarget.rotation =
        originalRotation *
        Quaternion.AngleAxis(
            accumulatedRotation,
            worldRotationAxis);
}


private bool TryGetPointerAngle(
    Vector2 screenPosition,
    out float angle)
{
    angle = 0f;

    if (viewerCamera == null)
    {
        return false;
    }

    if (activeTarget == null)
    {
        return false;
    }

    Vector3 pivotScreen3D =
        viewerCamera.WorldToScreenPoint(
            activeTarget.position);

    if (pivotScreen3D.z <= 0f)
    {
        return false;
    }

    Vector2 pivotScreen =
        new Vector2(
            pivotScreen3D.x,
            pivotScreen3D.y);

    Vector2 direction =
        screenPosition -
        pivotScreen;

    if (direction.sqrMagnitude <
        0.0001f)
    {
        return false;
    }

    angle =
        Mathf.Atan2(
            direction.y,
            direction.x) *
        Mathf.Rad2Deg;

    return true;
}

        // ============================================================
        // ROTATION POINTER → WORLD VECTOR
        // ============================================================

        private bool TryGetRotationVector(
            Vector2 screenPosition,
            out Vector3 rotationVector)
        {
            rotationVector =
                Vector3.zero;

            if (viewerCamera == null)
            {
                return false;
            }

            if (activeTarget == null)
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

            Vector3 hitPoint =
                ray.GetPoint(enter);

            Vector3 radial =
                hitPoint -
                activeTarget.position;

            /*
             * Remove any tiny component along the rotation axis.
             * This makes the vector mathematically planar.
             */
            radial -=
                worldRotationAxis *
                Vector3.Dot(
                    radial,
                    worldRotationAxis);

            float magnitude =
                radial.magnitude;

            if (magnitude <
                minimumRotationRadius)
            {
                return false;
            }

            rotationVector =
                radial / magnitude;

            return true;
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
    sessionActive =
        false;

    activeTarget =
        null;

    mode =
        ManipulationMode.None;

    accumulatedRotation =
        0f;

    rawAccumulatedRotation =
        0f;

    grabOffset =
        Vector3.zero;

    worldRotationAxis =
        Vector3.zero;

    previousPointerAngle =
        0f;

    hasPreviousPointerAngle =
        false;

    LockViewerControls(false);
}

        // ============================================================
        // RESET
        // ============================================================

       private void ResetRuntimeState()
{
    sessionActive =
        false;

    activeTarget =
        null;

    mode =
        ManipulationMode.None;

    accumulatedRotation =
        0f;

    rawAccumulatedRotation =
        0f;

    grabOffset =
        Vector3.zero;

    worldRotationAxis =
        Vector3.zero;

    previousPointerAngle =
        0f;

    hasPreviousPointerAngle =
        false;
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