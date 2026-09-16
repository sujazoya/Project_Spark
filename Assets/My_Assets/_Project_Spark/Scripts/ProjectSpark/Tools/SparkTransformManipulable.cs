using ProjectSpark.Gameplay;
using UnityEngine;

namespace ProjectSpark.Tools
{
    public sealed class SparkTransformManipulable :
        MonoBehaviour,
        ISparkMovable,
        ISparkRotatable
    {
        [Header("Move")]
        [SerializeField]
        private bool allowMove = true;

        [SerializeField, Min(0f)]
        private float moveSnap = 0f;

        [Header("Rotate")]
        [SerializeField]
        private bool allowRotate = true;

        [SerializeField]
        private Vector3 rotationAxis = Vector3.up;

        [SerializeField, Min(0f)]
        private float rotationDegreesPerPixel = 0.25f;

        [SerializeField, Min(0f)]
        private float rotationSnap = 0f;

        private Camera activeCamera;
        private Plane movementPlane;

        private Vector3 startPosition;
        private Quaternion startRotation;

        private Vector3 grabOffset;
        private Vector2 startPointer;

        // ============================================================
        // MOVE
        // ============================================================

        public bool CanMove(
            in SparkInteractionContext context,
            out string reason)
        {
            if (!allowMove)
            {
                reason = "Movement is disabled.";
                return false;
            }

            reason = null;
            return true;
        }

        public SparkResult BeginMove(
            in SparkInteractionContext context)
        {
            if (!CanMove(
                    context,
                    out string reason))
            {
                return SparkResult.Rejected(reason);
            }

            activeCamera = Camera.main;

            if (activeCamera == null)
            {
                return SparkResult.Unavailable(
                    "No interaction camera found.");
            }

            startPosition =
                transform.position;

            movementPlane =
                new Plane(
                    activeCamera.transform.forward,
                    context.HitPosition);

            if (!TryProjectPointer(
                    context.ScreenPosition,
                    out Vector3 projectedPoint))
            {
                activeCamera = null;

                return SparkResult.Rejected(
                    "Pointer cannot be projected.");
            }

            grabOffset =
                transform.position -
                projectedPoint;

            return SparkResult.Success(
                "Move started.");
        }

        public SparkResult UpdateMove(
            in SparkInteractionContext context)
        {
            if (activeCamera == null)
            {
                return SparkResult.Unavailable(
                    "Move camera is unavailable.");
            }

            if (!TryProjectPointer(
                    context.ScreenPosition,
                    out Vector3 projectedPoint))
            {
                return SparkResult.Rejected(
                    "Pointer cannot be projected.");
            }

            Vector3 target =
                projectedPoint +
                grabOffset;

            if (moveSnap > 0f)
            {
                target.x =
                    Mathf.Round(
                        target.x / moveSnap) *
                    moveSnap;

                target.y =
                    Mathf.Round(
                        target.y / moveSnap) *
                    moveSnap;

                target.z =
                    Mathf.Round(
                        target.z / moveSnap) *
                    moveSnap;
            }

            transform.position =
                target;

            return SparkResult.Success();
        }

        public SparkResult EndMove(
            in SparkInteractionContext context)
        {
            activeCamera = null;

            return SparkResult.Success(
                "Move completed.");
        }

        public SparkResult CancelMove(
            in SparkInteractionContext context)
        {
            transform.position =
                startPosition;

            activeCamera = null;

            return SparkResult.Cancelled(
                "Move cancelled.");
        }

        // ============================================================
        // ROTATE
        // ============================================================

        public bool CanRotate(
            in SparkInteractionContext context,
            out string reason)
        {
            if (!allowRotate)
            {
                reason = "Rotation is disabled.";
                return false;
            }

            reason = null;
            return true;
        }

        public SparkResult BeginRotate(
            in SparkInteractionContext context)
        {
            if (!CanRotate(
                    context,
                    out string reason))
            {
                return SparkResult.Rejected(reason);
            }

            startRotation =
                transform.rotation;

            startPointer =
                context.ScreenPosition;

            return SparkResult.Success(
                "Rotation started.");
        }

        public SparkResult UpdateRotate(
            in SparkInteractionContext context)
        {
            Vector2 delta =
                context.ScreenPosition -
                startPointer;

            float rotationInput =
                delta.x -
                delta.y;

            float angle =
                rotationInput *
                rotationDegreesPerPixel;

            if (rotationSnap > 0f)
            {
                angle =
                    Mathf.Round(
                        angle / rotationSnap) *
                    rotationSnap;
            }

            Vector3 axis =
                rotationAxis.sqrMagnitude >
                0.000001f
                    ? rotationAxis.normalized
                    : Vector3.up;

            transform.rotation =
                startRotation *
                Quaternion.AngleAxis(
                    angle,
                    axis);

            return SparkResult.Success();
        }

        public SparkResult EndRotate(
            in SparkInteractionContext context)
        {
            return SparkResult.Success(
                "Rotation completed.");
        }

        public SparkResult CancelRotate(
            in SparkInteractionContext context)
        {
            transform.rotation =
                startRotation;

            return SparkResult.Cancelled(
                "Rotation cancelled.");
        }

        // ============================================================
        // POINTER → WORLD
        // ============================================================

        private bool TryProjectPointer(
            Vector2 screenPosition,
            out Vector3 worldPosition)
        {
            Ray ray =
                activeCamera.ScreenPointToRay(
                    screenPosition);

            if (movementPlane.Raycast(
                    ray,
                    out float distance))
            {
                if (distance >= 0f)
                {
                    worldPosition =
                        ray.GetPoint(distance);

                    return true;
                }
            }

            worldPosition = default;
            return false;
        }
    }
}