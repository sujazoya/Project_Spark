using UnityEngine;

namespace ProjectSpark.Gameplay
{
    [DisallowMultipleComponent]
    public sealed class SparkInteractionController : MonoBehaviour
    {
        [Header("Camera")]

        [SerializeField]
        private Camera targetCamera;

        [Header("Raycast")]

        [SerializeField]
        [Min(0.1f)]
        private float interactionDistance = 5f;

        [SerializeField]
        private LayerMask interactionLayers = ~0;

        [SerializeField]
        private QueryTriggerInteraction triggerInteraction =
            QueryTriggerInteraction.Ignore;

        [Header("Input")]

        [SerializeField]
        private KeyCode interactKey = KeyCode.Mouse0;

        [Header("Settings")]

        [SerializeField]
        private bool interactionEnabled = true;

        [SerializeField]
        private bool highlightTarget = true;

        [Header("Runtime")]

        [SerializeField]
        private SparkInteractable currentTarget;

        [SerializeField]
        private SparkInteractable previousTarget;

        public SparkInteractable CurrentTarget =>
            currentTarget;

        public bool HasTarget =>
            currentTarget != null;

        public bool InteractionEnabled =>
            interactionEnabled;

        private void Awake()
        {
            ResolveCamera();
        }

        private void Update()
        {
            if (!interactionEnabled)
            {
                ClearTarget();
                return;
            }

            UpdateTarget();

            if (Input.GetKeyDown(interactKey))
            {
                TryInteract();
            }
        }

        private void ResolveCamera()
        {
            if (targetCamera != null)
                return;

            targetCamera = Camera.main;
        }

        private void UpdateTarget()
        {
            SparkInteractable detectedTarget =
                RaycastForInteractable();

            if (detectedTarget == currentTarget)
                return;

            previousTarget =
                currentTarget;

            currentTarget =
                detectedTarget;

            UpdateHighlightState();
        }

        private SparkInteractable RaycastForInteractable()
        {
            if (targetCamera == null)
            {
                ResolveCamera();

                if (targetCamera == null)
                    return null;
            }

            Ray ray =
                targetCamera.ScreenPointToRay(
                    Input.mousePosition);

            if (!Physics.Raycast(
                    ray,
                    out RaycastHit hit,
                    interactionDistance,
                    interactionLayers,
                    triggerInteraction))
            {
                return null;
            }

            return FindInteractable(hit.collider);
        }

        private SparkInteractable FindInteractable(
            Collider hitCollider)
        {
            if (hitCollider == null)
                return null;

            SparkInteractable interactable =
                hitCollider.GetComponent<
                    SparkInteractable>();

            if (interactable == null)
            {
                interactable =
                    hitCollider.GetComponentInParent<
                        SparkInteractable>();
            }

            if (interactable == null)
            {
                interactable =
                    hitCollider.GetComponentInChildren<
                        SparkInteractable>(
                            true);
            }

            if (interactable == null)
                return null;

            if (!interactable.isActiveAndEnabled)
                return null;

            if (!interactable.CanInteract())
                return null;

            return interactable;
        }

        private void TryInteract()
        {
            SparkInteractable target =
                currentTarget;

            if (target == null)
                return;

            if (!target.isActiveAndEnabled)
                return;

            if (!target.CanInteract())
                return;

            SparkObjectVisibilityController
                visibilityController =
                    FindVisibilityController(target);

            if (visibilityController != null)
            {
                visibilityController.OpenPanel();
                return;
            }

            target.Interact();
        }

        private SparkObjectVisibilityController
            FindVisibilityController(
                SparkInteractable interactable)
        {
            if (interactable == null)
                return null;

            SparkObjectVisibilityController controller =
                interactable.GetComponent<
                    SparkObjectVisibilityController>();

            if (controller != null)
                return controller;

            controller =
                interactable.GetComponentInParent<
                    SparkObjectVisibilityController>();

            if (controller != null)
                return controller;

            controller =
                interactable.GetComponentInChildren<
                    SparkObjectVisibilityController>(
                        true);

            return controller;
        }

        private void UpdateHighlightState()
        {
            if (!highlightTarget)
                return;

            if (previousTarget != null)
            {
                previousTarget.SetHighlighted(false);
            }

            if (currentTarget != null)
            {
                currentTarget.SetHighlighted(true);
            }
        }

        private void ClearTarget()
        {
            if (currentTarget != null)
            {
                currentTarget.SetHighlighted(false);
            }

            if (previousTarget != null &&
                previousTarget != currentTarget)
            {
                previousTarget.SetHighlighted(false);
            }

            currentTarget = null;
            previousTarget = null;
        }

        public void SetInteractionEnabled(
            bool enabled)
        {
            interactionEnabled =
                enabled;

            if (!enabled)
            {
                ClearTarget();
            }
        }

        public void SetInteractionDistance(
            float distance)
        {
            interactionDistance =
                Mathf.Max(
                    0.1f,
                    distance);
        }

        public void SetCamera(
            Camera camera)
        {
            targetCamera = camera;
        }

        [ContextMenu("Test / Clear Target")]
        private void TestClearTarget()
        {
            ClearTarget();
        }
    }
}