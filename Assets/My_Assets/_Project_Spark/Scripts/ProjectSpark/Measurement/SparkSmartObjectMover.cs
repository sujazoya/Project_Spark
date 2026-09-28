using UnityEngine;

namespace ProjectSpark.Gameplay
{
    /// <summary>
    /// Smoothly moves one selected object to a target Transform position.
    ///
    /// The selected object is supplied externally by the selection system.
    /// This component does not handle:
    /// - Selection
    /// - Raycasting
    /// - Camera movement
    /// - Viewer rotation
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkSelectedObjectMover : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField]
        private float smoothTime = 0.18f;

        [SerializeField]
        private float maxSpeed = 100f;

        [SerializeField]
        private float stopDistance = 0.001f;

        private Transform selectedObject;
        private Transform targetTransform;

        private Vector3 velocity;

        private bool moving;

        public bool IsMoving
        {
            get { return moving; }
        }

        public Transform SelectedObject
        {
            get { return selectedObject; }
        }

        public Transform TargetTransform
        {
            get { return targetTransform; }
        }

        private void Update()
        {
            if (!moving)
            {
                return;
            }

            if (selectedObject == null ||
                targetTransform == null)
            {
                Stop();
                return;
            }

            MoveToTarget();
        }

        /// <summary>
        /// Assigns the object that should be moved.
        /// </summary>
        public void SetSelectedObject(
            Transform targetObject)
        {
            if (targetObject == null)
            {
                return;
            }

            selectedObject =
                targetObject;

            targetTransform = null;

            velocity =
                Vector3.zero;

            moving =
                false;
        }

        /// <summary>
        /// Moves the currently selected object
        /// smoothly to the supplied target Transform.
        /// </summary>
        public bool MoveTo(
            Transform destination)
        {
            if (selectedObject == null ||
                destination == null)
            {
                return false;
            }

            if (destination == selectedObject)
            {
                return false;
            }

            targetTransform =
                destination;

            velocity =
                Vector3.zero;

            moving =
                true;

            return true;
        }

        private void MoveToTarget()
        {
            Vector3 targetPosition =
                targetTransform.position;

            float distance =
                Vector3.Distance(
                    selectedObject.position,
                    targetPosition);

            if (distance <= stopDistance)
            {
                Finish();
                return;
            }

            selectedObject.position =
                Vector3.SmoothDamp(
                    selectedObject.position,
                    targetPosition,
                    ref velocity,
                    smoothTime,
                    maxSpeed,
                    Time.deltaTime);

            if (Vector3.Distance(
                    selectedObject.position,
                    targetPosition) <= stopDistance)
            {
                Finish();
            }
        }

        private void Finish()
        {
            if (selectedObject != null &&
                targetTransform != null)
            {
                selectedObject.position =
                    targetTransform.position;
            }

            velocity =
                Vector3.zero;

            moving =
                false;

            targetTransform = null;
        }

        public void Stop()
        {
            velocity =
                Vector3.zero;

            moving =
                false;

            targetTransform = null;
        }

        public void ClearSelection()
        {
            Stop();

            selectedObject = null;
        }
    }
}