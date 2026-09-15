using UnityEngine;

namespace ProjectSpark
{
    /// <summary>
    /// Allows a Project Spark object to be moved, rotated, and scaled
    /// through a simple, reusable transform interface.
    /// </summary>
    public sealed class SparkTransformManipulable : MonoBehaviour
    {
        [Header("Transform Settings")]
        [SerializeField] private bool allowMove = true;
        [SerializeField] private bool allowRotate = true;
        [SerializeField] private bool allowScale = false;

        [Header("Movement")]
        [SerializeField] private float moveSensitivity = 1f;
        [SerializeField] private float maxMoveDistance = 100f;

        [Header("Rotation")]
        [SerializeField] private float rotateSensitivity = 90f;

        [Header("Scale")]
        [SerializeField] private float scaleSensitivity = 1f;
        [SerializeField] private float minScale = 0.1f;
        [SerializeField] private float maxScale = 10f;

        [Header("State")]
        [SerializeField] private bool selected;

        private Vector3 originalPosition;
        private Quaternion originalRotation;
        private Vector3 originalScale;

        public bool IsSelected => selected;

        public bool AllowMove => allowMove;
        public bool AllowRotate => allowRotate;
        public bool AllowScale => allowScale;

        public Vector3 OriginalPosition => originalPosition;
        public Quaternion OriginalRotation => originalRotation;
        public Vector3 OriginalScale => originalScale;

        private void Awake()
        {
            SaveTransform();
        }

        /// <summary>
        /// Sets the selection state of this object.
        /// </summary>
        public void SetSelected(bool value)
        {
            selected = value;
        }

        /// <summary>
        /// Saves the current transform as the manipulation baseline.
        /// </summary>
        public void SaveTransform()
        {
            originalPosition = transform.position;
            originalRotation = transform.rotation;
            originalScale = transform.localScale;
        }

        /// <summary>
        /// Restores the transform saved by SaveTransform().
        /// </summary>
        public void RestoreTransform()
        {
            transform.position = originalPosition;
            transform.rotation = originalRotation;
            transform.localScale = originalScale;
        }

        /// <summary>
        /// Moves the object by a world-space delta.
        /// </summary>
        public void Move(Vector3 worldDelta)
        {
            if (!allowMove)
                return;

            Vector3 delta = worldDelta * moveSensitivity;

            if (maxMoveDistance > 0f)
            {
                Vector3 targetPosition = transform.position + delta;

                float distanceFromOrigin =
                    Vector3.Distance(originalPosition, targetPosition);

                if (distanceFromOrigin > maxMoveDistance)
                {
                    Vector3 direction =
                        (targetPosition - originalPosition).normalized;

                    targetPosition =
                        originalPosition + direction * maxMoveDistance;
                }

                transform.position = targetPosition;
            }
            else
            {
                transform.position += delta;
            }
        }

        /// <summary>
        /// Moves the object toward a world-space position.
        /// </summary>
        public void MoveTo(Vector3 worldPosition)
        {
            if (!allowMove)
                return;

            if (maxMoveDistance > 0f)
            {
                float distanceFromOrigin =
                    Vector3.Distance(originalPosition, worldPosition);

                if (distanceFromOrigin > maxMoveDistance)
                {
                    Vector3 direction =
                        (worldPosition - originalPosition).normalized;

                    worldPosition =
                        originalPosition + direction * maxMoveDistance;
                }
            }

            transform.position = worldPosition;
        }

        /// <summary>
        /// Rotates the object around a world-space axis.
        /// </summary>
        public void Rotate(Vector3 axis, float amount)
        {
            if (!allowRotate)
                return;

            if (axis.sqrMagnitude < 0.000001f)
                return;

            float angle = amount * rotateSensitivity;

            transform.Rotate(
                axis.normalized,
                angle,
                Space.World);
        }

        /// <summary>
        /// Rotates the object by Euler angles.
        /// </summary>
        public void Rotate(Vector3 eulerAngles)
        {
            if (!allowRotate)
                return;

            transform.Rotate(
                eulerAngles * rotateSensitivity,
                Space.World);
        }

        /// <summary>
        /// Sets the world-space rotation.
        /// </summary>
        public void SetRotation(Quaternion rotation)
        {
            if (!allowRotate)
                return;

            transform.rotation = rotation;
        }

        /// <summary>
        /// Changes local scale uniformly.
        /// </summary>
        public void Scale(float amount)
        {
            if (!allowScale)
                return;

            Vector3 scale = transform.localScale;

            float newScale =
                scale.x + amount * scaleSensitivity;

            newScale = Mathf.Clamp(
                newScale,
                minScale,
                maxScale);

            transform.localScale =
                new Vector3(
                    newScale,
                    newScale,
                    newScale);
        }

        /// <summary>
        /// Sets a uniform local scale.
        /// </summary>
        public void SetUniformScale(float value)
        {
            if (!allowScale)
                return;

            value = Mathf.Clamp(
                value,
                minScale,
                maxScale);

            transform.localScale =
                Vector3.one * value;
        }

        /// <summary>
        /// Resets the object to its original transform.
        /// </summary>
        public void ResetTransform()
        {
            RestoreTransform();
        }

        /// <summary>
        /// Enables or disables movement.
        /// </summary>
        public void SetMoveEnabled(bool value)
        {
            allowMove = value;
        }

        /// <summary>
        /// Enables or disables rotation.
        /// </summary>
        public void SetRotateEnabled(bool value)
        {
            allowRotate = value;
        }

        /// <summary>
        /// Enables or disables scaling.
        /// </summary>
        public void SetScaleEnabled(bool value)
        {
            allowScale = value;
        }
    }
}